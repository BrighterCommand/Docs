# AWS Scheduler

> **Reference** · Applies to **Brighter V10**

**AWS EventBridge Scheduler** is a fully managed, serverless service on AWS that allows you to create, run, and manage scheduled tasks at scale. Brighter integrates with AWS EventBridge Scheduler to provide cloud-native scheduling for AWS workloads.

## When to Use AWS Scheduler

AWS Scheduler is recommended when:

- **Running on AWS**: Your application is deployed on AWS (EC2, ECS, Lambda, etc.)
- **Serverless Architecture**: You prefer managed services over self-hosted solutions
- **AWS Native**: You want to leverage AWS services directly
- **Scalability**: You need to handle millions of scheduled messages
- **Cost Optimization**: You want to pay per use without managing infrastructure
- **Multi-Region**: You need to schedule across AWS regions

**When NOT to use:**

- **Multi-Cloud**: Need scheduler to work across cloud providers
- **On-Premises**: Application runs outside AWS
- **Complex Scheduling**: Need advanced scheduling features (job chains, dependencies)
- **Dashboard**: Need visual management interface

## How Brighter Integrates with AWS Scheduler

Brighter provides two approaches for scheduling with AWS EventBridge Scheduler:

### 1. Direct to Target (Recommended)

When `UseMessageTopicAsTarget = true` (default), Brighter schedules messages directly to the target SNS topic or SQS queue:

```text
Your Code → CommandProcessor.SendAsync(delay, command)
    ↓
Brighter creates AWS EventBridge Schedule
    ↓
Schedule stored in AWS EventBridge Scheduler
    ↓
[Schedule fires at specified time]
    ↓
AWS EventBridge → Directly publishes to target SNS/SQS
    ↓
Your Dispatcher → Handler executes
```

**Benefits**:

- Fewer moving parts
- Lower latency (no intermediate handler)
- Simpler architecture

### 2. Via FireAwsScheduler Message

When `UseMessageTopicAsTarget = false` or using request scheduler, Brighter schedules through an intermediate `FireAwsScheduler` message:

```text
Your Code → CommandProcessor.SendAsync(delay, command)
    ↓
Brighter creates AWS EventBridge Schedule
    ↓
Schedule stored in AWS EventBridge Scheduler
    ↓
[Schedule fires at specified time]
    ↓
AWS EventBridge → Publishes FireAwsScheduler to scheduler topic/queue
    ↓
AwsSchedulerFiredHandler executes
    ↓
CommandProcessor dispatches original command
    ↓
Your Handler executes
```

**Use when**:

- Request scheduling (Send/Publish commands, not messages)
- Target topic doesn't exist at scheduling time
- Need centralized scheduler message handling

## IAM Role Requirements

AWS EventBridge Scheduler requires an IAM role that allows it to publish to SNS topics and send messages to SQS queues.

### Trust Policy

The role must trust the EventBridge Scheduler service:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": {
                "Service": "scheduler.amazonaws.com"
            },
            "Action": "sts:AssumeRole"
        }
    ]
}
```

### Permissions Policy

The role must have permissions to publish messages:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "sqs:SendMessage",
                "sns:Publish"
            ],
            "Resource": ["*"]
        }
    ]
}
```

**Best Practice**: Limit `Resource` to specific topic/queue ARNs instead of `"*"`.

### Automatic Role Creation

Brighter can create the role automatically if it doesn't exist:

```csharp
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    MakeRole = OnMissingRole.Create  // Brighter will create role if missing
};
```

**Options**:

- `OnMissingRole.Assume` (default): Assume role exists, throw error if not found
- `OnMissingRole.Create`: Create role if it doesn't exist

## AWS Scheduler NuGet Packages

AWS Scheduler integration is available in two versions:

### AWS SDK v4 (Recommended)

```bash
dotnet add package Paramore.Brighter.MessageScheduler.AWS.V4
```

Uses the latest AWS SDK for .NET v4.

### AWS SDK v3 (Legacy)

```bash
dotnet add package Paramore.Brighter.MessageScheduler.AWS
```

Uses AWS SDK for .NET v3 (deprecated in AWS).

**Recommendation**: Use V4 for new projects. Both packages provide identical Brighter functionality.

## AWS Scheduler Configuration

### Basic Configuration

Configure AWS Scheduler with minimal settings:

```csharp
using Microsoft.Extensions.Hosting;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.AWS.V4;
using Paramore.Brighter.MessagingGateway.AWSSQS;

var builder = WebApplication.CreateBuilder(args);

// Create AWS connection
var awsConnection = new AWSMessagingGatewayConnection(
    credentials,  // From AWS credentials provider
    RegionEndpoint.USEast1
);

// Configure Brighter with AWS Scheduler
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    // Schedule topic for FireAwsScheduler messages (when not direct to target)
    SchedulerTopicOrQueue = new RoutingKey("message-scheduler-topic"),
    OnConflict = OnSchedulerConflict.Overwrite
})
.AutoFromAssemblies();

var app = builder.Build();
```

### Configuration with Dispatcher (FireAwsScheduler Handler)

If using `FireAwsScheduler` approach, configure the Dispatcher to handle scheduler messages:

```csharp
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.AWSSQS.V4;

var builder = WebApplication.CreateBuilder(args);

var awsConnection = new AWSMessagingGatewayConnection(credentials, RegionEndpoint.USEast1);

// Configure producer registry (includes scheduler topic)
var producerRegistry = new SnsProducerRegistryFactory(
    awsConnection,
    [
        new SnsPublication
        {
            Topic = new RoutingKey("message-scheduler-topic"),
            RequestType = typeof(FireAwsScheduler)
        }
    ]
).Create();

// Configure subscriptions (handle FireAwsScheduler messages)
var subscriptions = new[]
{
    new SqsSubscription<FireAwsScheduler>(
        new SubscriptionName("scheduler-message-subscription"),
        new ChannelName("scheduler-message-channel"),
        new RoutingKey("message-scheduler-topic"),
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
.UseScheduler(new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    SchedulerTopicOrQueue = new RoutingKey("message-scheduler-topic"),
    UseMessageTopicAsTarget = false,  // Use FireAwsScheduler approach
    OnConflict = OnSchedulerConflict.Overwrite
})
.AutoFromAssemblies();

var app = builder.Build();
```

### Configuration with Scheduler Groups

AWS EventBridge Scheduler supports organizing schedules into groups:

```csharp
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    Group = new SchedulerGroup
    {
        Name = "orders-scheduler-group",
        Tags = new List<Tag>
        {
            new Tag { Key = "Environment", Value = "Production" },
            new Tag { Key = "Application", Value = "OrderProcessing" }
        },
        MakeSchedulerGroup = OnMissingSchedulerGroup.Create  // Create group if missing
    }
};
```

**Options**:

- `OnMissingSchedulerGroup.Assume` (default): Assume group exists
- `OnMissingSchedulerGroup.Create`: Create group if it doesn't exist

### Configuration with Flexible Time Window

EventBridge Scheduler supports flexible time windows for cost optimization:

```csharp
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    FlexibleTimeWindowMinutes = 5  // Execute within 5-minute window
};
```

A flexible time window allows AWS to batch executions for better efficiency. The schedule will execute within the specified window after the scheduled time.

### Configuration with Custom Scheduler ID

Customize how Brighter generates scheduler IDs for messages and requests:

```csharp
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-scheduler-role")
{
    // Custom scheduler ID for messages
    GetOrCreateMessageSchedulerId = message => $"msg-{message.Id}",

    // Custom scheduler ID for requests (idempotency)
    GetOrCreateRequestSchedulerId = request =>
    {
        if (request is ProcessOrderCommand cmd)
        {
            return $"order-{cmd.OrderId}";  // Idempotent per order
        }
        return request.Id.ToString();
    }
};
```

**Use Cases**:

- **Idempotency**: Same ID prevents duplicate schedules
- **Tracking**: Correlate scheduled tasks with business entities
- **Debugging**: Meaningful IDs in AWS Console

## AWS Scheduler Code Examples

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

### Publishing Events with Delay

Schedule an event to be published:

```csharp
public class TrialService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task StartTrial(User user)
    {
        user.TrialStartDate = DateTime.UtcNow;
        user.TrialEndDate = DateTime.UtcNow.AddDays(30);
        await _repository.SaveAsync(user);

        // Schedule trial expiry reminder for 7 days before end
        var reminderDate = user.TrialEndDate.AddDays(-7);
        var schedulerId = await _commandProcessor.PublishAsync(
            reminderDate,
            new TrialExpiringEvent { UserId = user.Id, DaysRemaining = 7 }
        );

        _logger.LogInformation(
            "Scheduled trial expiry reminder for user {UserId}: {SchedulerId}",
            user.Id,
            schedulerId
        );
    }
}
```

## Scheduling Modes Comparison

| Feature | Direct to Target | Via FireAwsScheduler |
|---------|------------------|----------------------|
| **Configuration** | `UseMessageTopicAsTarget = true` | `UseMessageTopicAsTarget = false` |
| **Use Case** | Message scheduling | Request scheduling |
| **Latency** | Lower (direct) | Higher (2-hop) |
| **Components** | Fewer | More (requires Dispatcher) |
| **Flexibility** | Target must exist | More flexible |
| **Recommended For** | Production messages | Requests and commands |

## AWS Scheduler Best Practices

### 1. Use Direct to Target for Messages

```csharp
// Good - Direct scheduling (default)
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-role")
{
    UseMessageTopicAsTarget = true  // Default, can omit
};
```

```csharp
// Bad - Unnecessary indirection for messages
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "my-role")
{
    UseMessageTopicAsTarget = false  // Forces FireAwsScheduler approach
};
```

### 2. Limit IAM Role Permissions

```csharp
// Good - Specific resource ARNs
{
    "Effect": "Allow",
    "Action": ["sqs:SendMessage", "sns:Publish"],
    "Resource": [
        "arn:aws:sqs:us-east-1:123456789012:my-queue",
        "arn:aws:sns:us-east-1:123456789012:my-topic"
    ]
}
```

```csharp
// Bad - Overly permissive
{
    "Effect": "Allow",
    "Action": ["sqs:*", "sns:*"],
    "Resource": ["*"]
}
```

### 3. Use Scheduler Groups for Organization

```csharp
// Good - Organized by feature/team
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "role")
{
    Group = new SchedulerGroup
    {
        Name = "payment-processing-schedules",
        Tags = new List<Tag>
        {
            new Tag { Key = "Team", Value = "Payments" },
            new Tag { Key = "CostCenter", Value = "Engineering" }
        }
    }
};
```

### 4. Handle OnConflict Appropriately

```csharp
// Good - Explicit conflict handling
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "role")
{
    OnConflict = OnSchedulerConflict.Overwrite,  // Or Throw, depending on use case
    GetOrCreateRequestSchedulerId = request => $"idempotent-{request.Id}"
};
```

### 5. Use Custom Scheduler IDs for Idempotency

```csharp
// Good - Idempotent based on business key
GetOrCreateRequestSchedulerId = request =>
{
    if (request is ProcessPaymentCommand cmd)
    {
        return $"payment-{cmd.TransactionId}";  // One schedule per transaction
    }
    return request.Id.ToString();
}
```

```csharp
// Bad - Always creates new schedule
GetOrCreateRequestSchedulerId = request => Guid.NewGuid().ToString();
```

### 6. Monitor AWS Scheduler Metrics

```csharp
// Use AWS CloudWatch to monitor:
// - InvocationAttempts
// - InvocationSuccessRate
// - InvocationThrottles
// - TargetErrorRate

// Set up CloudWatch alarms for failures
```

### 7. Use Flexible Time Windows for Cost Optimization

```csharp
// Good - Allow flexibility for non-critical schedules
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "role")
{
    FlexibleTimeWindowMinutes = 5  // AWS can batch executions
};
```

### 8. Test with LocalStack

```csharp
// Good - Test AWS Scheduler locally with LocalStack
var awsConnection = new AWSMessagingGatewayConnection(
    credentials,
    RegionEndpoint.USEast1,
    cfg =>
    {
        if (environment.IsDevelopment())
        {
            cfg.ServiceURL = "http://localhost:4566";  // LocalStack
        }
    }
);
```

## AWS Scheduler Troubleshooting

### Schedules Not Executing

**Symptom**: Schedules created but never fire

**Possible Causes**:

1. IAM role missing or incorrect permissions
2. Target topic/queue doesn't exist
3. Schedule created in wrong region
4. FireAwsScheduler handler not configured

**Solutions**:

```csharp
// 1. Verify IAM role permissions in AWS Console
// 2. Ensure target exists before scheduling
var schedulerFactory = new AwsSchedulerFactory(awsConnection, "role")
{
    MakeRole = OnMissingRole.Create  // Auto-create role
};

// 3. Verify region matches your resources
var awsConnection = new AWSMessagingGatewayConnection(
    credentials,
    RegionEndpoint.USEast1  // Match your SNS/SQS region
);

// 4. Configure FireAwsScheduler subscription if needed
```

### Access Denied Errors

**Symptom**: `AccessDeniedException` when creating schedules

**Cause**: Application credentials lack EventBridge Scheduler permissions

**Solution**:

```json
{
    "Effect": "Allow",
    "Action": [
        "scheduler:CreateSchedule",
        "scheduler:DeleteSchedule",
        "scheduler:GetSchedule",
        "scheduler:UpdateSchedule",
        "iam:PassRole"
    ],
    "Resource": "*"
}
```

### Schedule Conflicts

**Symptom**: `ConflictException` when creating schedule

**Cause**: Schedule with same ID already exists

**Solutions**:

```csharp
// Option 1: Overwrite existing schedule
OnConflict = OnSchedulerConflict.Overwrite

// Option 2: Throw exception and handle in code
OnConflict = OnSchedulerConflict.Throw
```

### High Costs

**Symptom**: Unexpected AWS Scheduler costs

**Causes**:
- Too many one-time schedules created and not cleaned up
- Short polling intervals

**Solutions**:

```csharp
// 1. Use flexible time windows
FlexibleTimeWindowMinutes = 5

// 2. Clean up old schedules
await _scheduler.CancelAsync(oldSchedulerId);

// 3. Consider Quartz/Hangfire for very frequent schedules
// AWS Scheduler is billed per schedule invocation
```

## Related Documentation

- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [Switching Schedulers](/contents/SwitchingSchedulers.md) - Moving to or from this scheduler
- [Quartz Scheduler](QuartzScheduler.md) - Alternative for non-AWS environments
- [Hangfire Scheduler](HangfireScheduler.md) - Alternative with dashboard
- [InMemory Scheduler](InMemoryScheduler.md) - For testing and development
- [Azure Scheduler](AzureScheduler.md) - Azure equivalent
- [AWS SNS Configuration](AWSSQSConfiguration.md) - Configuring AWS messaging

## AWS Scheduler External Links

- [AWS EventBridge Scheduler Documentation](https://docs.aws.amazon.com/scheduler/latest/UserGuide/what-is-scheduler.html)
- [AWS EventBridge Scheduler Pricing](https://aws.amazon.com/eventbridge/pricing/)
- [AWS EventBridge Scheduler Quotas](https://docs.aws.amazon.com/scheduler/latest/UserGuide/scheduler-quotas.html)
- [LocalStack EventBridge](https://docs.localstack.cloud/user-guide/aws/eventbridge/) - For local testing

## AWS Scheduler Summary

AWS EventBridge Scheduler is a cloud-native, serverless scheduling solution perfect for:

- **AWS Workloads** - Native integration with AWS services
- **Serverless Applications** - No infrastructure to manage
- **High Scalability** - Millions of schedules supported
- **Cost Optimization** - Pay only for what you use

**Recommended for production** when running on AWS infrastructure. Consider Quartz.NET or Hangfire for multi-cloud or on-premises deployments.

**Key Decision Factors**:
- ✅ Use AWS Scheduler if: Running on AWS, prefer managed services, need scalability
- ✅ Use Quartz if: Multi-cloud, complex scheduling, need job clustering
- ✅ Use Hangfire if: Need dashboard, simple setup, OK without strong naming
- ✅ Use InMemory if: Testing only, not for production
