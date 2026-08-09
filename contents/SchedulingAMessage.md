# Scheduling a Message

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Scheduler](/contents/BrighterSchedulerSupport.md)

This page shows you how to schedule a message or request for deferred execution, how to cancel one you have already scheduled, and how to configure each scheduler for the job. For what scheduling is and which scheduler to pick, see [Scheduler](/contents/BrighterSchedulerSupport.md).

## Message Scheduling Code Examples

### Basic Scheduling with DateTimeOffset

Schedule a command for a specific absolute time:

```csharp
// ...
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrder(Order order)
    {
        // Save order
        await _repository.SaveAsync(order);

        // Schedule order processing for tomorrow at 9 AM
        var processTime = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
        var schedulerId = await _commandProcessor.SendAsync(
            new ProcessOrderCommand { OrderId = order.Id },
            at: new DateTimeOffset(processTime)
        );

        // Store scheduler ID for potential cancellation
        order.ProcessSchedulerId = schedulerId;
    }
}
```

### Basic Scheduling with TimeSpan

Schedule a command with a relative delay:

```csharp
// ...
public class RegistrationService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task RegisterUser(User user)
    {
        // Create user account
        await _repository.SaveAsync(user);

        // Send welcome email immediately
        await _commandProcessor.SendAsync(new SendWelcomeEmailCommand { UserId = user.Id });

        // Schedule reminder email for 24 hours later
        await _commandProcessor.SendAsync(
            new SendReminderEmailCommand { UserId = user.Id },
            delay: TimeSpan.FromHours(24)
        );
    }
}
```

### Scheduling with Post for External Bus

Schedule a message to an external broker:

```csharp
// ...
public class NotificationService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task ScheduleNotification(NotificationRequest request)
    {
        // Schedule notification to be sent via external bus
        var schedulerId = await _commandProcessor.PostAsync(
            new NotificationEvent
            {
                UserId = request.UserId,
                Message = request.Message
            },
            delay: request.Delay
        );

        // Return scheduler ID for tracking
        return schedulerId;
    }
}
```

### Cancelling a Scheduled Message

Cancel a previously scheduled message:

```csharp
// ...
public class OrderService
{
    private readonly IMessageScheduler _scheduler;

    public async Task CancelOrder(Guid orderId)
    {
        var order = await _repository.GetAsync(orderId);

        // Cancel the scheduled order processing
        if (!string.IsNullOrEmpty(order.ProcessSchedulerId))
        {
            await _scheduler.CancelAsync(order.ProcessSchedulerId);
        }

        // Mark order as cancelled
        order.Status = OrderStatus.Cancelled;
        await _repository.UpdateAsync(order);
    }
}
```

**Note:** Every scheduler supports cancellation. Rescheduling is the operation that varies: the Azure Service Bus scheduler does not reschedule, so cancel the message and schedule it again instead. See [Choosing a Scheduler](/contents/BrighterSchedulerSupport.md#choosing-a-scheduler).

### Retry with Exponential Backoff

Implement retry logic with increasing delays:

```csharp
// ...
public class RetryService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task RetryWithBackoff(OperationCommand command, int attemptNumber)
    {
        // Calculate exponential backoff delay
        var delaySeconds = Math.Pow(2, attemptNumber); // 2^attempt seconds
        var maxDelay = TimeSpan.FromMinutes(30);
        var delay = TimeSpan.FromSeconds(Math.Min(delaySeconds, maxDelay.TotalSeconds));

        // Schedule retry
        await _commandProcessor.SendAsync(
            command with { AttemptNumber = attemptNumber + 1 },
            delay: delay
        );
    }
}
```

### Using Requeue with Delay in a Handler

```csharp
// ...
public class ProcessPaymentHandlerAsync : RequestHandlerAsync<ProcessPaymentCommand>
{
    private const int MaxRetries = 3;

    public override async Task<ProcessPaymentCommand> HandleAsync(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _paymentGateway.ProcessAsync(command.PaymentId, cancellationToken);
            return await base.HandleAsync(command, cancellationToken);
        }
        catch (PaymentGatewayUnavailableException)
        {
            // Throw DeferMessageAction to requeue with configured delay
            // Subscription must have requeueCount and requeueDelayInMilliseconds configured
            throw new DeferMessageAction();
        }
        catch (PaymentDeclinedException ex)
        {
            // Don't requeue for business logic failures
            _logger.LogWarning(ex, "Payment declined for {PaymentId}", command.PaymentId);
            return await base.HandleAsync(command, cancellationToken);
        }
    }
}
```

## Message Scheduling Configuration Examples

### Configuring with Hangfire

```csharp
// ...
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(
    scheduler: new HangfireMessageSchedulerFactory(
        connectionString: Configuration.GetConnectionString("Hangfire")
    )
)
.AutoFromAssemblies();
```

### Configuring with Quartz.NET

```csharp
// ...
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(
    scheduler: new QuartzMessageSchedulerFactory(
        configuration: Configuration.GetSection("Quartz")
    )
)
.AutoFromAssemblies();
```

### Configuring with InMemory (Development Only)

```csharp
// ...
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(
    scheduler: new InMemorySchedulerFactory()
)
.AutoFromAssemblies();
```

## Further Reading

- [Scheduler](/contents/BrighterSchedulerSupport.md) - What scheduling is, and choosing a scheduler
- [Switching Schedulers](/contents/SwitchingSchedulers.md) - Moving from one scheduler to another
- [Custom Scheduler](/contents/CustomScheduler.md) - Implementing your own scheduler
- [Handler Failure](/contents/HandlerFailure.md) - Error handling and retry strategies
