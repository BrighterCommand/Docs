# InMemory Scheduler

> **Reference** · Applies to **Brighter V10**

The **InMemory Scheduler** is a lightweight, timer-based scheduling implementation provided by Brighter for **testing, development, and demonstration purposes**. It requires no external dependencies and stores scheduled jobs in memory using timers.

## Important Warning

**The InMemory Scheduler is NOT durable and is NOT recommended for production use.**

- **No Persistence**: All scheduled jobs are lost if the application crashes or restarts
- **No Distribution**: Cannot be shared across multiple application instances
- **No Recovery**: Failed jobs are not automatically retried after restart
- **Memory Bound**: All scheduled jobs are held in memory

**Use this scheduler for:**

- Unit and integration tests
- Local development
- Demos and proof-of-concepts
- Production scenarios where losing scheduled work is acceptable

## What is the InMemory Scheduler?

The InMemory Scheduler uses .NET's `ITimerProvider` internally to schedule delayed execution of messages. When you schedule a message:

1. Brighter creates an in-memory timer for the specified delay
2. The timer fires at the scheduled time
3. Brighter dispatches your message to the appropriate handler
4. The timer is removed from memory

This simple approach makes it perfect for testing but unsuitable for production systems that require durability.

## InMemory Scheduler Architecture

```text
Your Code
    ↓
CommandProcessor.SendAsync(command, delay)
    ↓
InMemoryScheduler
    ↓
ITimerProvider.CreateTimer(delay)
    ↓
[Timer stored in memory]
    ↓
[Timer fires after delay]
    ↓
CommandProcessor dispatches command
    ↓
Your Handler executes
```

## When to Use InMemory Scheduler

### Recommended Use Cases

**Unit Testing:**

```csharp
[Fact]
public async Task Should_Schedule_Command_For_Later_Execution()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddBrighter(options => { ... })
        .UseScheduler(new InMemorySchedulerFactory())  // Perfect for tests
        .AutoFromAssemblies();

    var provider = services.BuildServiceProvider();
    var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();

    // Act
    var schedulerId = await commandProcessor.SendAsync(
        TimeSpan.FromMilliseconds(100),
        new TestCommand(),
    );

    // Assert
    Assert.NotNull(schedulerId);
    await Task.Delay(150);  // Wait for scheduled execution
    // Verify command was handled...
}
```

### Limited Production Scenarios

The InMemory Scheduler might be acceptable in production **only if**:

- Loss of scheduled work is acceptable (non-critical notifications, analytics, etc.)
- Your application rarely restarts
- Scheduled work has short delays (minutes, not hours/days)
- You have alternative mechanisms to recover lost work

**Example - Acceptable Production Use:**

```csharp
// Low-priority analytics events that can be lost
public class AnalyticsService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task TrackUserAction(string userId, string action)
    {
        // Track immediately
        await _repository.SaveActionAsync(userId, action);

        // Schedule low-priority aggregation (acceptable to lose)
        await _commandProcessor.SendAsync(
             TimeSpan.FromMinutes(5),
            new AggregateAnalyticsCommand { UserId = userId }
        );
    }
}
```

## InMemory Scheduler Configuration

### Basic Configuration

Configure the InMemory Scheduler with `InMemorySchedulerFactory`:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.InMemoryScheduler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new InMemorySchedulerFactory())  // Add InMemory Scheduler
.AutoFromAssemblies();

var app = builder.Build();
```

### Environment-Specific Configuration

Use InMemory for development, production schedulers elsewhere:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(GetSchedulerFactory(builder.Environment, builder.Configuration))
.AutoFromAssemblies();

static IMessageSchedulerFactory GetSchedulerFactory(
    IHostEnvironment environment,
    IConfiguration configuration)
{
    if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
    {
        return new InMemorySchedulerFactory();
    }

    // Production - use durable scheduler
    return new HangfireMessageSchedulerFactory(
        configuration.GetConnectionString("Hangfire")
    );
}
```

### Configuration with Custom Timer Provider

The InMemory Scheduler uses `ITimerProvider` internally. You can provide a custom implementation for testing:

```csharp
public class FakeTimerProvider : ITimerProvider
{
    public ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
    {
        // Custom timer implementation for testing
        return new FakeTimer(callback, state, dueTime, period);
    }
}

// Use in tests
services.AddBrighter(options => { ... })
    .UseScheduler(new InMemorySchedulerFactory(new FakeTimerProvider()))
    .AutoFromAssemblies();
```

## InMemory Scheduler NuGet Package

To use the InMemory Scheduler, install the NuGet package:

```bash
dotnet add package Paramore.Brighter.InMemoryScheduler
```

**Package**: `Paramore.Brighter.InMemoryScheduler`

## InMemory Scheduler Code Examples

### Basic Scheduling

Schedule a command with a delay:

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);

        // Schedule order confirmation email for 5 minutes later
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromMinutes(5),
            new SendOrderConfirmationCommand { OrderId = order.Id }
        );

        _logger.LogInformation("Scheduled confirmation email: {SchedulerId}", schedulerId);
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
        // Schedule for tomorrow at 9 AM
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

Cancel a previously scheduled job:

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IMessageScheduler _scheduler;

    public async Task CancelOrder(Guid orderId)
    {
        var order = await _repository.GetAsync(orderId);

        // Cancel the scheduled confirmation email
        if (!string.IsNullOrEmpty(order.ConfirmationSchedulerId))
        {
            await _scheduler.CancelAsync(order.ConfirmationSchedulerId);
            _logger.LogInformation("Cancelled scheduled email for order {OrderId}", orderId);
        }

        order.Status = OrderStatus.Cancelled;
        await _repository.UpdateAsync(order);
    }
}
```

### Testing with InMemory Scheduler

Example unit test using InMemory Scheduler:

```csharp
public class SchedulingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly TestHandlerRegistry _handlerRegistry;

    public SchedulingTests()
    {
        var services = new ServiceCollection();
        _handlerRegistry = new TestHandlerRegistry();

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .UseScheduler(new InMemorySchedulerFactory())  // InMemory for tests
        .Handlers(_handlerRegistry)
        .AutoFromAssemblies();

        _serviceProvider = services.BuildServiceProvider();
        _commandProcessor = _serviceProvider.GetRequiredService<IAmACommandProcessor>();
    }

    [Fact]
    public async Task Should_Execute_Scheduled_Command_After_Delay()
    {
        // Arrange
        var command = new TestCommand { Id = Guid.NewGuid() };
        var delay = TimeSpan.FromMilliseconds(100);

        // Act
        var schedulerId = await _commandProcessor.SendAsync(delay, command);

        // Assert - command not yet executed
        Assert.False(_handlerRegistry.WasHandled(command.Id));

        // Wait for scheduled execution
        await Task.Delay(delay.Add(TimeSpan.FromMilliseconds(50)));

        // Assert - command executed
        Assert.True(_handlerRegistry.WasHandled(command.Id));
    }

    [Fact]
    public async Task Should_Cancel_Scheduled_Command()
    {
        // Arrange
        var command = new TestCommand { Id = Guid.NewGuid() };
        var delay = TimeSpan.FromSeconds(10);  // Long delay
        var scheduler = _serviceProvider.GetRequiredService<IMessageScheduler>();

        // Act
        var schedulerId = await _commandProcessor.SendAsync(delay, command);
        await scheduler.CancelAsync(schedulerId);  // Cancel immediately

        // Wait to ensure it would have executed
        await Task.Delay(TimeSpan.FromSeconds(11));

        // Assert - command was NOT executed
        Assert.False(_handlerRegistry.WasHandled(command.Id));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
```

## Comparison with Production Schedulers

| Feature | InMemory | Quartz.NET | Hangfire | AWS Scheduler | Azure Service Bus |
|---------|----------|------------|----------|---------------|-------------------|
| **Persistence** |  None |  Database |  Database | AWS | Azure |
| **Distribution** | No | Yes | Yes | Yes | Yes |
| **Cancellation** | Yes | Yes | Yes |  Limited | No |
| **Dashboard** | No | Limited | Yes | AWS Console | Azure Portal |
| **Setup Complexity** | Minimal | Moderate | Easy | ️Moderate | Easy |
| **Production Ready** | No | Yes | Yes | Yes | Yes |
| **Testing** | Ideal | Overkill | Overkill | No | No |

## InMemory Scheduler Best Practices

### 1. Use for Testing Only

```csharp
// Good - Environment-specific
if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
{
    services.UseScheduler(new InMemorySchedulerFactory());
}
```

```csharp
// Bad - Always using InMemory in production
services.UseScheduler(new InMemorySchedulerFactory());
```

### 2. Document Production Limitations

If you use InMemory in production, document why:

```csharp
// PRODUCTION NOTE: Using InMemory scheduler because loss of
// scheduled analytics aggregations is acceptable. These are
// non-critical and will be regenerated on next sync.
services.UseScheduler(new InMemorySchedulerFactory());
```

### 3. Keep Delays Short

If using in production, keep delays under a few minutes:

```csharp
// Good - Short delay acceptable to lose
await _commandProcessor.SendAsync(TimeSpan.FromMinutes(2), command);

// Bad - Long delay likely to be lost
await _commandProcessor.SendAsync(TimeSpan.FromHours(24), command);
```

### 4. Test Scheduler Behavior

Write tests that verify scheduled behavior:

```csharp
[Fact]
public async Task Should_Handle_Concurrent_Scheduled_Commands()
{
    // Schedule multiple commands with different delays
    var ids = new List<string>();
    for (int i = 0; i < 10; i++)
    {
        ids.Add(await _commandProcessor.SendAsync(
            TimeSpan.FromMilliseconds(50 + i * 10),
            new TestCommand { Number = i }
        ));
    }

    // Wait for all to execute
    await Task.Delay(TimeSpan.FromMilliseconds(200));

    // Verify all were handled
    Assert.Equal(10, _handlerRegistry.HandledCount);
}
```

### 5. Don't Rely on It for Critical Work

```csharp
// Bad - Critical payment processing
await _commandProcessor.SendAsync(
    TimeSpan.FromMinutes(5),
    new ProcessPaymentCommand { Amount = 1000.00m }
);

// Good - Critical work should use durable scheduler
await _commandProcessor.SendAsync(
    TimeSpan.FromMinutes(5),
    new ProcessPaymentCommand { Amount = 1000.00m }
);  // With Quartz or Hangfire in production
```

## Migration to Production Scheduler

When moving to production, replace InMemory with a durable scheduler:

### Before (Development):

```csharp
services.AddBrighter(options => { ... })
    .UseScheduler(new InMemorySchedulerFactory())
    .AutoFromAssemblies();
```

### After (Production with Hangfire):

```csharp
services.AddBrighter(options => { ... })
    .UseScheduler(new HangfireMessageSchedulerFactory(
        Configuration.GetConnectionString("Hangfire")
    ))
    .AutoFromAssemblies();
```

### After (Production with Quartz):

```csharp
services.AddBrighter(options => { ... })
    .UseScheduler(new QuartzMessageSchedulerFactory(
        Configuration.GetSection("Quartz")
    ))
    .AutoFromAssemblies();
```

**No code changes required** - just swap the scheduler factory!

## InMemory Scheduler Troubleshooting

### Scheduled Jobs Not Executing

**Symptom**: Jobs scheduled but never execute

**Possible Causes**:

1. Application stopped before timer fires
2. Delay too short (already passed)
3. Exception in handler preventing execution

**Solution**:

```csharp
// Add logging to verify scheduling
var schedulerId = await _commandProcessor.SendAsync(delay, command);
_logger.LogInformation("Scheduled job {SchedulerId} for {Delay}", schedulerId, delay);

// Verify handler is registered
Assert.NotNull(_serviceProvider.GetService<IHandleRequestsAsync<YourCommand>>());
```

### Scheduled Jobs Lost After Restart

**Symptom**: Application restart loses all scheduled jobs

**Cause**: This is expected behavior - InMemory scheduler has no persistence

**Solution**: Use a production scheduler (Quartz, Hangfire) if you need durability

### Memory Usage Growing

**Symptom**: Memory consumption increases over time

**Cause**: Too many scheduled jobs held in memory

**Solution**:

- Reduce number of concurrent scheduled jobs
- Use shorter delays
- Consider a production scheduler with external storage

## Related Documentation

- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [Quartz Scheduler](QuartzScheduler.md) - Production scheduler with persistence
- [Hangfire Scheduler](HangfireScheduler.md) - Production scheduler with dashboard
- [AWS Scheduler](AwsScheduler.md) - Cloud-native AWS scheduling
- [Azure Scheduler](AzureScheduler.md) - Cloud-native Azure scheduling

## InMemory Scheduler Summary

The InMemory Scheduler is a lightweight, zero-dependency scheduling solution perfect for:
- **Unit and integration tests** - No external dependencies
- **Local development** - Fast and simple
- **Demos and POCs** - Quick to set up

**NOT recommended for production** due to:
- **No persistence** - Jobs lost on restart
- **No distribution** - Single-process only
- **No recovery** - No durability guarantees

Use production schedulers (Quartz.NET, Hangfire, AWS Scheduler, Azure Service Bus Scheduler) for any system requiring durability and reliability.
