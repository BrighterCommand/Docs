# Azure Service Bus Scheduler

> **Reference** · Applies to **Brighter V10**

**Azure Service Bus** includes native support for scheduled message delivery using the `ScheduledEnqueueTimeUtc` property. In V10, Brighter integrates with Azure Service Bus's built-in scheduling capabilities to provide cloud-native scheduling for Azure workloads.

## When to Use Azure Service Bus Scheduler

Azure Service Bus Scheduler is recommended when:

- **Running on Azure**: Your application is deployed on Azure (VMs, App Service, AKS, Functions, etc.)
- **Using Service Bus**: Already using Azure Service Bus for messaging
- **Cloud-Native**: Prefer Azure-managed services over self-hosted solutions
- **Simple Scheduling**: Need basic scheduling without complex dependencies
- **Azure Ecosystem**: Want to leverage Azure services directly
- **Cost-Effective**: Want to avoid additional scheduler infrastructure

**When NOT to use:**

- **Multi-Cloud**: Need scheduler to work across cloud providers
- **On-Premises**: Application runs outside Azure
- **Reschedule Support**: Need to modify scheduled time without cancel/recreate
- **Complex Scheduling**: Need advanced features (recurring schedules, job chains, dependencies)
- **Dashboard**: Need visual management interface

## How Brighter Integrates with Azure Service Bus Scheduler

Brighter uses Azure Service Bus's native `ScheduledEnqueueTimeUtc` property through the `FireAzureScheduler` message approach:

```
Your Code → CommandProcessor.SendAsync(delay, command)
    ↓
Brighter creates FireAzureScheduler message
    ↓
Message sent to scheduler topic with ScheduledEnqueueTimeUtc
    ↓
Azure Service Bus holds message until scheduled time
    ↓
[Scheduled time reached]
    ↓
Azure Service Bus delivers FireAzureScheduler message
    ↓
AzureSchedulerFiredHandler executes
    ↓
CommandProcessor dispatches original command
    ↓
Your Handler executes
```

**Why FireAzureScheduler Approach?**

Unlike AWS EventBridge Scheduler (which can directly target any SNS/SQS), Azure Service Bus requires a topic/queue to hold the scheduled message. Brighter uses a centralized scheduler topic to handle all scheduled messages, then dispatches them to the appropriate handlers.

## Authentication and Credentials

Azure Service Bus Scheduler uses Azure Service Bus client providers for authentication. Brighter supports multiple authentication methods:

### Managed Identity (Recommended for Production)

```csharp
using Azure.Identity;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

var clientProvider = new ServiceBusManagedIdentityClientProvider(
    "myservicebus.servicebus.windows.net"
);
```

### Visual Studio Credentials (Development)

```csharp
var clientProvider = new ServiceBusVisualStudioCredentialClientProvider(
    "myservicebus.servicebus.windows.net"
);
```

### Connection String (Simple, Less Secure)

```csharp
var clientProvider = new ServiceBusConnectionStringClientProvider(
    "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=..."
);
```

### Default Azure Credentials (Flexible)

```csharp
var clientProvider = new ServiceBusDefaultAzureCredentialClientProvider(
    "myservicebus.servicebus.windows.net"
);
```

**Best Practice**: Use Managed Identity in production, Visual Studio/Default credentials for development.

## RBAC Permissions Required

When using Managed Identity or Azure AD authentication, the identity must have these Azure RBAC roles:

- **Azure Service Bus Data Sender**: To send scheduled messages
- **Azure Service Bus Data Receiver**: To receive FireAzureScheduler messages (Dispatcher)

```bash
# Assign permissions using Azure CLI
az role assignment create \
    --assignee <managed-identity-client-id> \
    --role "Azure Service Bus Data Sender" \
    --scope /subscriptions/<subscription-id>/resourceGroups/<resource-group>/providers/Microsoft.ServiceBus/namespaces/<namespace>

az role assignment create \
    --assignee <managed-identity-client-id> \
    --role "Azure Service Bus Data Receiver" \
    --scope /subscriptions/<subscription-id>/resourceGroups/<resource-group>/providers/Microsoft.ServiceBus/namespaces/<namespace>
```

## Azure Scheduler NuGet Package

To use Azure Service Bus Scheduler, install:

```bash
dotnet add package Paramore.Brighter.MessageScheduler.Azure
```

**Package**: `Paramore.Brighter.MessageScheduler.Azure`

**Dependencies**:

- `Azure.Messaging.ServiceBus` - Azure Service Bus SDK
- `Paramore.Brighter.MessagingGateway.AzureServiceBus` - Brighter's Azure Service Bus transport

## Azure Scheduler Configuration

### Basic Configuration

Configure Azure Service Bus Scheduler with minimal settings:

```csharp
using Microsoft.Extensions.Hosting;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Azure;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

var builder = WebApplication.CreateBuilder(args);

// Create Azure Service Bus client provider
var clientProvider = new ServiceBusManagedIdentityClientProvider(
    "myservicebus.servicebus.windows.net"
);

// Configure Brighter with Azure Service Bus Scheduler
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new AzureServiceBusSchedulerFactory(
    clientProvider,
    new RoutingKey("brighter-scheduler-topic")  // Scheduler topic name
))
.AutoFromAssemblies();

var app = builder.Build();
```

### Configuration with Dispatcher (FireAzureScheduler Handler)

Configure the Dispatcher to handle FireAzureScheduler messages:

```csharp
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

var builder = WebApplication.CreateBuilder(args);

var clientProvider = new ServiceBusManagedIdentityClientProvider(
    "myservicebus.servicebus.windows.net"
);

// Configure producer registry (includes scheduler topic)
var producerRegistry = new AzureServiceBusProducerRegistryFactory(
    clientProvider,
    [
        new AzureServiceBusPublication
        {
            Topic = new RoutingKey("brighter-scheduler-topic"),
            RequestType = typeof(FireAzureScheduler)
        }
    ]
).Create();

// Configure subscription to handle FireAzureScheduler messages
var subscriptions = new[]
{
    new AzureServiceBusSubscription<FireAzureScheduler>(
        new SubscriptionName("scheduler-subscription"),
        new ChannelName("scheduler-channel"),
        new RoutingKey("brighter-scheduler-topic"),
        bufferSize: 10,
        timeOut: TimeSpan.FromSeconds(30),
        messagePumpType: MessagePumpType.Proactor
    )
};

builder.Services.AddServiceActivator(options =>
{
    options.Subscriptions = subscriptions;
})
.UseExternalBus(configure =>
{
    configure.ProducerRegistry = producerRegistry;
})
.UseScheduler(new AzureServiceBusSchedulerFactory(
    clientProvider,
    new RoutingKey("brighter-scheduler-topic")
))
.AutoFromAssemblies();

var app = builder.Build();
```

### Configuration with Custom Sender Options

Customize Azure Service Bus sender behavior:

```csharp
using Azure.Messaging.ServiceBus;

var schedulerFactory = new AzureServiceBusSchedulerFactory(
    clientProvider,
    new RoutingKey("brighter-scheduler-topic")
)
{
    SenderOptions = new ServiceBusSenderOptions
    {
        // Identifier for the sender
        Identifier = "BrighterScheduler"
    }
};
```

### Configuration with Custom TimeProvider

Use a custom time provider for testing:

```csharp
var schedulerFactory = new AzureServiceBusSchedulerFactory(
    clientProvider,
    new RoutingKey("brighter-scheduler-topic")
)
{
    TimeProvider = TimeProvider.System  // Or custom for testing
};
```

## Azure Scheduler Code Examples

### Basic Scheduling with Delay

Schedule a command to execute after a delay:

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);

        // Schedule payment processing for 30 minutes later
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromMinutes(30),
            new ProcessPaymentCommand { OrderId = order.Id, Amount = order.Total }
        );

        _logger.LogInformation(
            "Scheduled payment processing for order {OrderId}: {SchedulerId}",
            order.Id,
            schedulerId
        );
    }
}
```

### Scheduling with Absolute Time

Schedule for a specific time:

```csharp
public class ReportService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task ScheduleDailyReport()
    {
        // Schedule for tomorrow at 9 AM UTC
        var tomorrow9AM = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(9);

        var schedulerId = await _commandProcessor.SendAsync(
            tomorrow9AM,
            new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }
        );

        _logger.LogInformation("Scheduled daily report: {SchedulerId}", schedulerId);
    }
}
```

### Cancelling a Scheduled Job

Cancel a scheduled task before it executes:

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IMessageScheduler _scheduler;

    public async Task CancelOrder(Guid orderId)
    {
        var order = await _repository.GetAsync(orderId);

        // Cancel scheduled payment
        if (!string.IsNullOrEmpty(order.PaymentSchedulerId))
        {
            await _scheduler.CancelAsync(order.PaymentSchedulerId);
            _logger.LogInformation(
                "Cancelled scheduled payment for order {OrderId}",
                orderId
            );
        }

        order.Status = OrderStatus.Cancelled;
        await _repository.UpdateAsync(order);
    }
}
```

### Rescheduling (Cancel + Schedule)

Since Azure doesn't support rescheduling, cancel and create a new schedule:

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IMessageScheduler _scheduler;

    public async Task ReschedulePayment(Guid orderId, TimeSpan newDelay)
    {
        var order = await _repository.GetAsync(orderId);

        // Azure Service Bus doesn't support rescheduling
        // Must cancel existing schedule and create a new one
        if (!string.IsNullOrEmpty(order.PaymentSchedulerId))
        {
            await _scheduler.CancelAsync(order.PaymentSchedulerId);
        }

        // Create new schedule
        var newSchedulerId = await _commandProcessor.SendAsync(
            newDelay,
            new ProcessPaymentCommand { OrderId = order.Id, Amount = order.Total }
        );

        order.PaymentSchedulerId = newSchedulerId;
        await _repository.UpdateAsync(order);

        _logger.LogInformation(
            "Rescheduled payment for order {OrderId}: {NewSchedulerId}",
            orderId,
            newSchedulerId
        );
    }
}
```

## Azure Scheduler Comparison with Other Schedulers

| Feature | Azure Service Bus | AWS Scheduler | Quartz.NET | Hangfire | InMemory |
|---------|-------------------|---------------|------------|----------|----------|
| **Cloud Native** | ✅ Azure Only | ✅ AWS Only | ❌ | ❌ | ❌ |
| **Managed Service** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Persistence** | Azure Managed | AWS Managed | Database | Database | None |
| **Dashboard** | Azure Portal | AWS Console | Limited | Yes | No |
| **Cancellation** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Reschedule** | ❌ (Cancel+Schedule) | ✅ | ✅ | ✅ | ✅ |
| **Native Scheduling** | ✅ Built-in | ❌ Separate Service | ❌ | ❌ | ❌ |
| **Cost Model** | Service Bus Pricing | Pay-per-use | Infrastructure | Infrastructure | Free |
| **Setup Complexity** | Easy | Moderate (IAM) | Moderate | Easy | Minimal |
| **Production Ready** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Multi-Cloud** | ❌ | ❌ | ✅ | ✅ | ✅ |
| **Strong Naming** | ✅ | ✅ | ✅ | ❌ | ✅ |

**When to use Azure Service Bus Scheduler**:

- Running on Azure infrastructure
- Already using Azure Service Bus for messaging
- Want simplicity (no separate scheduler service)
- Don't need reschedule support (cancel+schedule is acceptable)

## Azure Scheduler Best Practices

### 1. Use Managed Identity in Production

```csharp
// Good - Managed Identity (no credentials in code)
var clientProvider = new ServiceBusManagedIdentityClientProvider(
    "myservicebus.servicebus.windows.net"
);
```

```csharp
// Bad - Connection string in code (security risk)
var clientProvider = new ServiceBusConnectionStringClientProvider(
    "Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=..."
);
```

### 2. Use Separate Scheduler Topic

```csharp
// Good - Dedicated scheduler topic
new RoutingKey("brighter-scheduler-topic")
```

```csharp
// Bad - Mixing business messages with scheduler messages
new RoutingKey("orders-topic")  // Don't use business topics for scheduling
```

### 3. Handle Reschedule as Cancel + Schedule

```csharp
// Good - Explicit cancel + schedule pattern
if (!string.IsNullOrEmpty(existingSchedulerId))
{
    await _scheduler.CancelAsync(existingSchedulerId);
}
var newSchedulerId = await _commandProcessor.SendAsync(newDelay, command);
```

```csharp
// Bad - Trying to reschedule (not supported)
await _scheduler.ReScheduleAsync(existingSchedulerId, newDelay);  // Won't work!
```

### 4. Set Appropriate Message TTL

```csharp
// Good - Configure topic with appropriate message TTL
// In Azure Portal or via ARM template
{
    "defaultMessageTimeToLive": "P14D"  // 14 days
}
```

### 5. Monitor with Azure Monitor

```csharp
// Use Azure Monitor to track:
// - Scheduled message count
// - Delivered message count
// - Dead-letter queue depth
// - Processing errors

// Set up alerts for:
// - Messages in dead-letter queue
// - Processing failures
```

### 6. Use Service Bus Premium for Production

```csharp
// Production recommendation: Use Premium tier
// - Better performance
// - Dedicated resources
// - Virtual network integration
// - Geo-disaster recovery
```

### 7. Configure Dead-Letter Queue

```csharp
// Ensure dead-letter queue is monitored
var subscription = new AzureServiceBusSubscription<FireAzureScheduler>(
    new SubscriptionName("scheduler-subscription"),
    new ChannelName("scheduler-channel"),
    new RoutingKey("brighter-scheduler-topic"),
    deadLetterQueueHandling: true  // Enable DLQ handling
);
```

### 8. Test Locally with Azurite

```csharp
// Good - Test with Azurite (Azure Storage Emulator)
if (environment.IsDevelopment())
{
    var clientProvider = new ServiceBusConnectionStringClientProvider(
        "Endpoint=sb://localhost:5672;..."  // Azurite or local emulator
    );
}
```

## Azure Scheduler Troubleshooting

### Scheduled Messages Not Executing

**Symptom**: Messages scheduled but never delivered

**Possible Causes**:

1. FireAzureScheduler subscription not configured
2. Incorrect topic name
3. Message TTL expired before scheduled time
4. Service Bus connection issues
5. RBAC permissions missing

**Solutions**:

```csharp
// 1. Verify subscription is configured
var subscriptions = new[]
{
    new AzureServiceBusSubscription<FireAzureScheduler>(
        new SubscriptionName("scheduler-subscription"),
        new ChannelName("scheduler-channel"),
        new RoutingKey("brighter-scheduler-topic")
    )
};

// 2. Ensure topic name matches in factory and subscription
new AzureServiceBusSchedulerFactory(clientProvider, new RoutingKey("brighter-scheduler-topic"))

// 3. Check message TTL in Azure Portal
// 4. Verify Service Bus connection
// 5. Check RBAC permissions for Managed Identity
```

### Authentication Failures

**Symptom**: `UnauthorizedAccessException` when scheduling

**Cause**: Missing RBAC permissions or invalid credentials

**Solution**:

```bash
# Verify managed identity has required roles
az role assignment list \
    --assignee <managed-identity-object-id> \
    --scope /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.ServiceBus/namespaces/<ns>

# Assign missing roles
az role assignment create \
    --assignee <managed-identity-client-id> \
    --role "Azure Service Bus Data Sender" \
    --scope /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.ServiceBus/namespaces/<ns>
```

### Reschedule Not Working

**Symptom**: Attempting to reschedule fails or has no effect

**Cause**: Azure Service Bus doesn't support rescheduling

**Solution**:

```csharp
// Azure doesn't support reschedule - use cancel + schedule pattern
public async Task<string> RescheduleAsync(string oldSchedulerId, TimeSpan newDelay, Command command)
{
    // Cancel existing schedule
    await _scheduler.CancelAsync(oldSchedulerId);

    // Create new schedule
    var newSchedulerId = await _commandProcessor.SendAsync(newDelay, command);

    return newSchedulerId;
}
```

### Messages Going to Dead-Letter Queue

**Symptom**: Scheduled messages end up in dead-letter queue

**Possible Causes**:

- Handler exceptions not properly handled
- Message processing timeout exceeded
- Maximum delivery count reached
- Invalid message format

**Solutions**:

```csharp
// 1. Check dead-letter queue for error details
// 2. Increase lock timeout
var subscription = new AzureServiceBusSubscription<FireAzureScheduler>(
    ...
    lockTimeout: 60  // Increase if processing takes longer
);

// 3. Add proper exception handling in handlers
public override async Task<FireAzureScheduler> HandleAsync(
    FireAzureScheduler command,
    CancellationToken cancellationToken = default)
{
    try
    {
        // Your logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing scheduled message");
        throw;  // Or handle appropriately
    }
}
```

## Azure Scheduler Migration from Other Schedulers

### From InMemory Scheduler

```csharp
// Before (Development)
services.AddBrighter(options => { ... })
    .UseScheduler(new InMemorySchedulerFactory())
    .AutoFromAssemblies();

// After (Production on Azure)
services.AddBrighter(options => { ... })
    .UseScheduler(new AzureServiceBusSchedulerFactory(
        clientProvider,
        new RoutingKey("brighter-scheduler-topic")
    ))
    .AutoFromAssemblies();
```

**Additional Setup Required**:

- Configure FireAzureScheduler subscription in Dispatcher
- Create scheduler topic in Azure Service Bus
- Configure RBAC permissions

### From Quartz or Hangfire to Azure Service Bus

```csharp
// Before (Quartz)
services.AddBrighter(options => { ... })
    .UseScheduler(provider =>
    {
        var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
        return new QuartzSchedulerFactory(
            schedulerFactory.GetScheduler().GetAwaiter().GetResult()
        );
    })
    .AutoFromAssemblies();

// After (Azure Service Bus)
services.AddBrighter(options => { ... })
    .UseScheduler(new AzureServiceBusSchedulerFactory(
        clientProvider,
        new RoutingKey("brighter-scheduler-topic")
    ))
    .AutoFromAssemblies();
```

**Benefits of moving to Azure Service Bus Scheduler**:

- Simpler (no separate scheduler infrastructure)
- Native Azure integration
- Reduced operational complexity
- No database required

**Considerations**:

- Must configure FireAzureScheduler subscription
- No reschedule support (cancel + schedule instead)
- Requires FireAzureScheduler topic in Service Bus

## Related Documentation

- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [Azure Service Bus Configuration](AzureServiceBusConfiguration.md) - Configuring Azure Service Bus transport
- [AWS Scheduler](AwsScheduler.md) - AWS equivalent
- [Quartz Scheduler](QuartzScheduler.md) - Alternative for non-Azure environments
- [Hangfire Scheduler](HangfireScheduler.md) - Alternative with dashboard
- [InMemory Scheduler](InMemoryScheduler.md) - For testing and development

## Azure Scheduler External Links

- [Azure Service Bus Message Sequencing](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing)
- [Azure Service Bus Scheduled Messages](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing#scheduled-messages)
- [Azure Service Bus Pricing](https://azure.microsoft.com/en-us/pricing/details/service-bus/)
- [Azure RBAC Roles for Service Bus](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-service-bus-data-sender)
- [Azure Managed Identity](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)

## Azure Scheduler Summary

Azure Service Bus Scheduler leverages Azure Service Bus's native scheduling capabilities for cloud-native scheduling:

- **Native Integration** - Uses Azure Service Bus `ScheduledEnqueueTimeUtc`
- **Managed Service** - No separate scheduler infrastructure
- **Simple Setup** - If already using Azure Service Bus
- **Azure Ecosystem** - Integrates with Azure monitoring and security

**Recommended for production** when running on Azure and using Azure Service Bus. Consider Quartz.NET or Hangfire for multi-cloud or on-premises deployments.

**Key Decision Factors**:
- ✅ Use Azure Service Bus Scheduler if: Running on Azure, using Service Bus, want simplicity
- ✅ Use AWS Scheduler if: Running on AWS, need reschedule support
- ✅ Use Quartz if: Multi-cloud, complex scheduling, need job clustering
- ✅ Use Hangfire if: Need dashboard, simple setup, OK without strong naming
- ✅ Use InMemory if: Testing only, not for production

**Important Note**: Azure Service Bus does **NOT** support rescheduling. Use cancel + schedule pattern when rescheduling is needed.
