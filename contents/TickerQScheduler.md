# TickerQ Scheduler

[TickerQ](https://github.com/Arcenox-co/TickerQ) is a high-performance, reflection-free background task scheduler for .NET that uses source generators. Brighter provides integration with TickerQ for [scheduler functionality](/contents/BrighterSchedulerSupport.md), offering a modern, lightweight alternative for distributed scheduling.

## TickerQ Overview

TickerQ is designed for performance and cloud-native environments. Unlike traditional schedulers, it leverages .NET source generators to avoid runtime reflection, making it extremely fast and memory-efficient. Key features include:

- **Reflection-Free**: Uses source generators for compile-time job discovery
- **High Performance**: Minimal memory footprint and fast startup
- **Dashboard**: Built-in real-time dashboard for monitoring jobs
- **Persistence**: Support for Entity Framework Core
- **Flexible Scheduling**: Cron expressions and time-based scheduling
- **Clean API**: Modern, type-safe API design

For more information, visit the [TickerQ website](https://tickerq.net/).

## How Brighter Integrates with TickerQ

Brighter integrates with TickerQ through:

1. **TickerQBrighterJob**: A specialized job that executes scheduled Brighter messages
2. **TickerQSchedulerFactory**: Factory that creates Brighter's message scheduler backed by TickerQ
3. **TickerQScheduler**: Scheduler that schedules Brighter messages using TickerQ

When you schedule a message with Brighter:

1. Brighter uses the TickerQ manager to schedule a job
2. TickerQ persists the job (if configured)
3. At the scheduled time, TickerQ triggers the execution
4. The `TickerQBrighterJob` dispatches the message via Brighter's Command Processor

## NuGet Packages

Install the required NuGet packages:

```bash
dotnet add package Paramore.Brighter.MessageScheduler.TickerQ
dotnet add package TickerQ
```

For persistence, add the EF Core package:

```bash
dotnet add package TickerQ.EntityFrameworkCore
```

## Configuration

### Basic Configuration

Configure Brighter with TickerQ scheduler in your `Program.cs`:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.TickerQ;
using TickerQ;

var builder = WebApplication.CreateBuilder(args);

// Configure TickerQ
builder.Services.AddTickerQ();

// Configure Brighter with TickerQ scheduler
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(provider =>
{
    var timeTickerManager = provider.GetRequiredService<ITimeTickerManager<TimeTickerEntity>>();
    var persistenceProvider = provider.GetRequiredService<ITickerPersistenceProvider<TimeTickerEntity, CronTickerEntity>>();
    var timeprovider = provider.GetRequiredService<TimeProvider>();
    return new TickerQSchedulerFactory(timeTickerManager, persistenceProvider, timeprovider);
});

var app = builder.Build();

app.UseTickerQ();
```

### Configuration with Persistence (EF Core)

For production scenarios, you should use persistent storage to ensure jobs are not lost during restarts:

```csharp
builder.Services.AddTickerQ(options =>
{
    options.AddOperationalStore(efOptions =>
 {
     efOptions.UseTickerQDbContext<TickerQDbContext>(dbOptions =>
     {
         dbOptions.UseSqlite(
             "Data Source=tickerq-brighter-sample.db",
            b => b.MigrationsAssembly(typeof(Program).Assembly));
     });
 });
});

var app = builder.Build();
// you must migrate the database 

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TickerQDbContext>();
    db.Database.Migrate();
}
app.UseTickerQ();
```
 for more information refer to the 
 [TickerQ EfCore](https://tickerq.net/features/entity-framework.html)

### Enabling the Dashboard

TickerQ comes with a built-in dashboard for monitoring scheduled jobs:
```bash
dotnet add package TickerQ.Dashboard
```

```csharp
builder.Services.AddTickerQ(options =>
{
    options.AddDashboard(o =>
    {
        o.SetBasePath("/dashboard"); //to configure the dashboard path
    });
});
```

You can then access the dashboard at `{{appurl}}/dashboard`.

## Code Examples

### Basic Scheduling

Scheduling a message with TickerQ execution is identical to other schedulers in Brighter, as the `IAmACommandProcessor` interface abstracts the underlying implementation.

```csharp
public class NotificationService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task ScheduleNotification(string userId)
    {
        // Schedule a reminder for 24 hours later
        var reminderCommand = new SendReminderCommand { UserId = userId };
        
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromHours(24),
            reminderCommand
        );

        Console.WriteLine($"Scheduled reminder with ID: {schedulerId}");
    }
}
```

### Cancelling a Scheduled Job

You can cancel a scheduled job using the ID returned during scheduling:

```csharp
public async Task CancelNotification(string schedulerId)
{
    // Cancel the specific job using the scheduler interface
    // Note: You typically need the IMessageScheduler interface here
    await _scheduler.CancelAsync(schedulerId);
}
```
### Rescheduling a Scheduled Job

You can reschedule a scheduled job using the ID returned during scheduling:

```csharp
public async Task RescheduleNotification(string schedulerId, DateTimeOffset at)
{
    // Reschedule the specific job using the scheduler interface
    // Note: You typically need the IMessageScheduler interface here
    // Note: You can't reschedule a job that has already been executed or  in progress
    await _scheduler.RescheduleAsync(schedulerId, at);
}
```

## Best Practices

### 1. Use Persistence in Production
Always configure TickerQ with a persistent store (like EF Core) for production environments. In-memory scheduling is suitable only for development or non-critical transient tasks.

### 2. Monitor via Dashboard
Leverage the TickerQ dashboard to inspect job states, failures, and upcoming schedules. This is invaluable for debugging and operations.

## Troubleshooting

### Jobs Not Firing
- **Check Host**: TickerQ runs as a hosted service. Ensure  TickerQ service started is called and the host is kept alive.
```csharp 
app.UseTickerQ(); 

```
- **Timezone**: Be aware of timezone settings when scheduling absolute times. Brighter typically uses UTC.
```csharp
builder.Services.AddTickerQ(options =>
{
    options.ConfigureScheduler(c =>
    {
        c.SchedulerTimeZone = TimeZoneInfo.Utc;
    });
});
```

### Dashboard Not Loading
- Verify `app.UseTickerQDashboard()` is called in the pipeline.
- Check if the configured path (default `/tickerq/dashboard`) conflicts with other routes.

## Related Documentation
- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [InMemory Scheduler](InMemoryScheduler.md) - Lightweight scheduler for testing
- [Hangfire Scheduler](HangfireScheduler.md) - Alternative production scheduler with dashboard
- [AWS Scheduler](AwsScheduler.md) - Cloud-native AWS scheduling
- [Azure Scheduler](AzureScheduler.md) - Cloud-native Azure scheduling
- [TickerQ Documentation](https://tickerq.net) - Official

## Summary

TickerQ integration for Brighter offers a modern, high-performance scheduling option.

- **Fast**: Source-generator based, low overhead.
- **Visual**: Integrated dashboard.
- **Standard**: Fully implements Brighter's `IMessageScheduler` interface.

Use TickerQ when you want a lightweight, modern scheduler without the legacy footprint of older libraries.
 