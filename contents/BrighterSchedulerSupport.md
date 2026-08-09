# Brighter Scheduler Support

> **Explanation** · Applies to **Brighter V10**

In distributed applications, it is often necessary to schedule messages for deferred execution—whether for implementing delayed workflows, retry mechanisms, or time-based business processes. Brighter provides built-in scheduling capabilities to handle these scenarios.

## Brighter Scheduler Overview

Brighter's scheduling support allows you to:

- **Defer message delivery** to a specific time or after a delay
- **Schedule commands and events** for future execution
- **Implement retry with delay** for transient failures
- **Build time-based workflows** and business processes

Scheduling integrates seamlessly with Brighter's messaging infrastructure, supporting both in-process handlers and external message brokers.

## Brighter Scheduler Use Cases

Common scenarios for message scheduling include:

- **Delayed Notifications**: Send reminder emails after a delay (e.g., "Complete your registration")
- **Retry with Backoff**: Retry failed operations with exponential backoff
- **Time-Based Workflows**: Execute business processes at specific times (e.g., "End of day reports")
- **Rate Limiting**: Spread message processing over time to avoid overwhelming downstream systems
- **Grace Periods**: Defer actions to allow for cancellation (e.g., "Cancel order within 30 minutes")
- **Scheduled Jobs**: Trigger recurring or one-time tasks at predetermined times

## Scheduling Methods

The `IAmACommandProcessor` interface provides methods for scheduling delayed messages:

```csharp
public interface IAmACommandProcessor
{
    Task<string> SendAsync<T>(T command, DateTimeOffset at, CancellationToken cancellationToken = default) where T : class, IRequest;
    Task<string> SendAsync<T>(T command, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IRequest;
    Task<string> PublishAsync<T>(T @event, DateTimeOffset at, CancellationToken cancellationToken = default) where T : class, IRequest;
    Task<string> PublishAsync<T>(T @event, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IRequest;
    Task<string> PostAsync<T>(T request, DateTimeOffset at, CancellationToken cancellationToken = default) where T : class, IRequest;
    Task<string> PostAsync<T>(T request, TimeSpan delay, CancellationToken cancellationToken = default) where T : class, IRequest;
}
```

### Method Parameters

#### DateTimeOffset at

Specifies the **absolute time** when the message should be delivered:

```csharp
// Schedule for specific date and time
var schedulerId = await commandProcessor.SendAsync(
    new ProcessOrderCommand { OrderId = orderId },
    at: new DateTimeOffset(2025, 1, 15, 14, 30, 0, TimeSpan.Zero)
);

// Schedule for 1 hour from now
var schedulerId = await commandProcessor.SendAsync(
    new SendReminderCommand { UserId = userId },
    at: DateTimeOffset.UtcNow.AddHours(1)
);
```

#### TimeSpan delay

Specifies a **relative delay** from the current time:

```csharp
// Schedule for 30 minutes from now
var schedulerId = await commandProcessor.SendAsync(
    new ProcessPaymentCommand { TransactionId = txId },
    delay: TimeSpan.FromMinutes(30)
);

// Schedule for 5 seconds from now (useful for retry)
var schedulerId = commandProcessor.Send(
    new RetryOperationCommand { AttemptNumber = 2 },
    delay: TimeSpan.FromSeconds(5)
);
```

### Return Value: Scheduler ID

All scheduling methods return a `string` representing the **scheduler ID**. This ID can be used to:

- **Cancel** a scheduled message before it executes
- **Reschedule** a message to a different time (cancel + schedule new)
- **Track** scheduled messages in your application

```csharp
// Save the scheduler ID for later use
string schedulerId = await commandProcessor.SendAsync(
    new CancelableOperationCommand { OperationId = opId },
    delay: TimeSpan.FromMinutes(30)
);

// Later, cancel the scheduled message if needed
await scheduler.CancelAsync(schedulerId);
```

**Note:** The ability to cancel/reschedule depends on your chosen scheduler implementation.

## How Scheduling Works Internally

When you call a scheduling method on the Command Processor, Brighter internally creates special scheduling messages that trigger your actual message at the specified time.

### Internal Message Types

Brighter uses two internal message types for scheduling:

1. **FireSchedulerMessage**: Used when scheduling messages to external brokers (with `Post` methods)
2. **FireSchedulerRequest**: Used when scheduling in-process commands/events (with `Send`/`Publish` methods)

### Scheduling Flow

```text
Your Application
       ↓
CommandProcessor.SendAsync(command, delay)
       ↓
[Creates FireSchedulerRequest with your command]
       ↓
Scheduler (e.g., Hangfire, Quartz)
       ↓
[Waits for specified delay/time]
       ↓
Scheduler triggers FireSchedulerRequest
       ↓
CommandProcessor dispatches your original command
       ↓
Your Handler executes
```

### Configuring a Scheduler

To use scheduling, you need to configure a scheduler when setting up Brighter:

```csharp
services.AddBrighter(options => { ... })
    .UseScheduler(scheduler: new HangfireMessageSchedulerFactory(
        connectionString: Configuration.GetConnectionString("Hangfire")
    ))
    .AutoFromAssemblies();
```

## Native Transport Delay Support

Some message transports have **native support for message delay**, eliminating the need for an external scheduler in certain scenarios.

### Transports with Native Delay Support

| Transport | Native Delay Support | Max Delay | Notes |
|-----------|---------------------|-----------|-------|
| **RabbitMQ** | Yes | Unlimited | Uses [RabbitMQ Delayed Message Plugin](https://github.com/rabbitmq/rabbitmq-delayed-message-exchange) or TTL + DLX |
| **Azure Service Bus** | Yes | Unlimited | Uses `ScheduledEnqueueTimeUtc` property |
| **AWS SQS** | Limited | 15 minutes | Native delay only up to 15 minutes |
| **AWS SNS** | No | N/A | Requires scheduler |
| **Kafka** | No | N/A | Requires scheduler |
| **Redis** | No | N/A | Requires scheduler |

### When to Use Native Transport Delay

Use native transport delay when:

- Your transport supports it (RabbitMQ, Azure Service Bus)
- Delays are within transport limits (e.g., <15 min for SQS)
- You don't need to cancel/reschedule messages
- You want to minimize infrastructure dependencies

Use an external scheduler when:

- Your transport doesn't support native delay
- You need delays beyond transport limits (e.g., >15 min on SQS)
- You need to cancel or reschedule messages
- You need centralized scheduling management

## Requeue with Delay

**Requeue with Delay** is a special feature that allows handlers to defer message processing when encountering transient failures.

### How It Works

When your handler encounters a transient error (e.g., database temporarily unavailable), you can throw a `DeferMessageAction` exception to requeue the message with a delay:

```csharp
public class ProcessOrderHandlerAsync : RequestHandlerAsync<ProcessOrderCommand>
{
    public override async Task<ProcessOrderCommand> HandleAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _orderService.ProcessAsync(command.OrderId, cancellationToken);
            return await base.HandleAsync(command, cancellationToken);
        }
        catch (DatabaseUnavailableException ex)
        {
            // Requeue with delay to give database time to recover
            throw new DeferMessageAction();
        }
    }
}
```

### Requeue Delay Configuration

Configure requeue delay in your subscription:

```csharp
var subscription = new Subscription<ProcessOrderCommand>(
    new SubscriptionName("order.processor"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("order.process"),
    messagePumpType: MessagePumpType.Proactor,
    requeueCount: 3,                        // Retry up to 3 times
    requeueDelayInMilliseconds: 5000,       // Wait 5 seconds between retries
    timeOut: TimeSpan.FromMilliseconds(200)
);
```

### Transport-Specific Behavior

**Transports with Native Delay (RabbitMQ, Azure Service Bus):**

- Uses native transport delay mechanism
- No external scheduler required
- Message requeued directly on the broker

**Transports without Native Delay (Kafka, AWS SNS, etc.):**

- Requires an external scheduler (Quartz, Hangfire, etc.)
- Message is scheduled via the configured scheduler
- Falls back to immediate requeue if no scheduler configured

## Choosing a Scheduler

Brighter supports multiple scheduler implementations. Your choice depends on your deployment environment and requirements.

### Scheduler Comparison

| Feature | Quartz.NET | Hangfire | AWS Scheduler | Azure Service Bus | InMemory |
|---------|------------|----------|---------------|-------------------|----------|
| **Production Use** | Recommended | Recommended | Recommended | Recommended | Dev/Test Only |
| **Persistence** | Database | Database | AWS Managed | Azure Managed | None |
| **Cancellation** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Reschedule** | ✅ | ✅ | ✅ | ❌ Cancel + schedule | ✅ |
| **Cloud Native** | ❌ | ❌ | ✅ AWS only | ✅ Azure only | ❌ |
| **Managed Service** | ❌ | ❌ | ✅ | ✅ | ❌ |
| **Distribution and Clustering** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Dashboard** | ⚠️ Limited/third-party | ✅ Built-in web UI | AWS Console | Azure Portal | ❌ |
| **Setup Complexity** | ⚠️ Moderate | ✅ Easy | ⚠️ Moderate (IAM) | ✅ Easy | ✅ Minimal |
| **Cost Model** | Infrastructure | Infrastructure | Pay-per-use | Service Bus pricing | Free |
| **Multi-Cloud** | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Strong Naming** | ✅ | ❌ | ✅ | ✅ | ✅ |
| **Native Scheduling** | ❌ | ❌ | ❌ Separate service | ✅ Built-in | ❌ |
| **Automatic Retry** | ⚠️ Manual | ✅ Built-in | — | — | — |
| **Recurring Jobs** | ⚠️ Via triggers | ✅ Native support | — | — | — |
| **Testing** | Overkill | Overkill | ❌ | ❌ | ✅ Ideal |
| **Best For** | General production use | .NET applications with dashboard | AWS deployments | Azure deployments | Testing, development |

A dash means the scheduler pages do not compare that feature for that scheduler.

### Scheduler Recommendations

#### For Production

**Quartz.NET** - Best for general production use:

- Mature, battle-tested scheduling library
- Supports persistent job stores (SQL, MongoDB, etc.)
- Distributed scheduling with clustering
- Full cancellation and reschedule support
- You need advanced trigger types (Cron, Calendar)
- You prefer more control over scheduling logic
- Strong naming is required
- Dashboard is not essential
- See [Quartz Scheduler Documentation](QuartzScheduler.md)

**Hangfire** - Best for .NET applications needing a dashboard:

- Easy setup and configuration
- Built-in web dashboard for monitoring
- Persistent job storage (SQL Server, PostgreSQL, Redis, etc.)
- Background job processing beyond scheduling
- You need automatic retry handling
- Note: `Brighter.Hangfire` assembly is NOT strong-named due to Hangfire limitations
- See [Hangfire Scheduler Documentation](HangfireScheduler.md)

#### For Cloud Providers

**AWS Scheduler** - Best for AWS deployments:

- Native AWS service (EventBridge Scheduler)
- No infrastructure to manage
- Direct integration with SNS/SQS
- IAM-based security
- Prefer serverless/managed services
- Need high scalability
- Cost-effective pay-per-use model
- See [AWS Scheduler Documentation](AwsScheduler.md)

**Azure Service Bus Scheduler** - Best for Azure deployments:

- Built into Azure Service Bus
- No additional infrastructure
- Native message delay support
- Already using Azure Service Bus for messaging
- Want simplicity (no separate scheduler service)
- Don't need reschedule support (cancel+schedule is acceptable)
- See [Azure Scheduler Documentation](AzureScheduler.md)

#### For Development and Testing

**InMemory Scheduler** - Only for dev/test environments:

- No external dependencies
- Fast and simple for unit tests
- Supports cancellation and reschedule
- **NOT durable** - crashes lose all scheduled messages
- **NOT for production** unless data loss is acceptable
- See [InMemory Scheduler Documentation](InMemoryScheduler.md)

### Decision Guide

```text
┌─────────────────────────────────────────┐
│  Are you deploying to AWS?              │
└──────────────┬──────────────────────────┘
               │ Yes → AWS Scheduler
               │
               │ No
               ↓
┌─────────────────────────────────────────┐
│  Are you deploying to Azure?            │
└──────────────┬──────────────────────────┘
               │ Yes → Azure Service Bus Scheduler
               │
               │ No
               ↓
┌─────────────────────────────────────────┐
│  Do you need a management dashboard?    │
└──────────────┬──────────────────────────┘
               │ Yes → Hangfire
               │
               │ No → Quartz.NET
               ↓
┌─────────────────────────────────────────┐
│  Testing/Development only?              │
└──────────────┬──────────────────────────┘
               │ Yes → InMemory Scheduler
               │
               │ No → Choose Quartz or Hangfire
```

## Brighter Scheduler Best Practices

1. **Always store scheduler IDs** if you need to cancel or track scheduled messages
2. **Use DateTimeOffset for absolute times** and TimeSpan for relative delays
3. **Consider time zones** when scheduling with DateTimeOffset (use UTC when possible)
4. **Choose the right scheduler** based on your deployment environment
5. **Use InMemory scheduler only for testing** - it's not durable
6. **Configure requeue delay appropriately** - too short can overwhelm the system, too long delays recovery
7. **Implement idempotency** in handlers that process scheduled messages
8. **Monitor scheduled jobs** using your scheduler's dashboard/tools (Hangfire dashboard, AWS Console, etc.)
9. **Set reasonable max delays** for requeue to avoid indefinite retries
10. **Use native transport delay when available** to simplify infrastructure

## Related Documentation

- [Scheduling a Message](/contents/SchedulingAMessage.md) - Code and configuration examples for scheduling, cancelling and requeueing
- [Switching Schedulers](/contents/SwitchingSchedulers.md) - Moving from one scheduler to another
- [Quartz Scheduler](QuartzScheduler.md) - Quartz.NET scheduler configuration
- [Hangfire Scheduler](HangfireScheduler.md) - Hangfire scheduler configuration
- [AWS Scheduler](AwsScheduler.md) - AWS EventBridge Scheduler configuration
- [Azure Scheduler](AzureScheduler.md) - Azure Service Bus Scheduler configuration
- [InMemory Scheduler](InMemoryScheduler.md) - InMemory scheduler for testing
- [Custom Scheduler](CustomScheduler.md) - Implementing your own scheduler
- [Handler Failure](HandlerFailure.md) - Error handling and retry strategies

## Brighter Scheduler Summary

- Brighter provides comprehensive scheduling support for deferred message execution
- Choose between **absolute time** (DateTimeOffset) or **relative delay** (TimeSpan)
- **Native transport delay** available for RabbitMQ and Azure Service Bus (with limitations on SQS)
- **External schedulers** (Quartz, Hangfire) required for transports without native delay
- **Requeue with Delay** enables sophisticated retry strategies for transient failures
- Use **production schedulers** (Quartz, Hangfire, AWS/Azure) for reliable scheduling
- **InMemory scheduler** is only suitable for development and testing
- All scheduling methods return a **scheduler ID** for cancellation/tracking
