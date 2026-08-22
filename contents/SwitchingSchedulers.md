---
description: "This page shows you how to move an existing application from one Brighter scheduler to another."
layout:
  description:
    visible: false
---

# Switching Schedulers

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Scheduler](/contents/BrighterSchedulerSupport.md)

This page shows you how to move an existing application from one Brighter scheduler to another. Each scheduler documents its own configuration in full; what follows is only what changes when you swap one for another. For help deciding which scheduler to move to, see [Choosing a Scheduler](/contents/BrighterSchedulerSupport.md#choosing-a-scheduler).

## Why You Would Switch Schedulers

The common case is leaving the InMemory scheduler behind. When moving to production, replace InMemory with a durable scheduler: it holds its timers in process memory, so a restart loses every scheduled message.

The other cases are environmental. You move to [AWS Scheduler](/contents/AwsScheduler.md) or [Azure Service Bus](/contents/AzureScheduler.md) when you want the platform to run the schedule rather than your own database, and between [Hangfire](/contents/HangfireScheduler.md) and [Quartz](/contents/QuartzScheduler.md) when you need something the other one has — a dashboard, or a strong-named assembly.

## Switching Schedulers: What Changes and What Does Not

**No code changes required** - just swap the scheduler factory!

Your handlers, your commands and your calls to `SendAsync`, `PostAsync` and `PublishAsync` are all unchanged. The scheduler is supplied by a factory passed to `UseScheduler`, and that factory is the only thing you replace.

Before, on the InMemory scheduler:

```csharp
// ...
services.AddBrighter(options => { ... })
    .UseScheduler(new InMemorySchedulerFactory())
    .AutoFromAssemblies();
```

Before, on Quartz:

```csharp
// ...
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
```

What follows replaces the `UseScheduler` call, and nothing else.

## Switching to a Production Scheduler

### Switching to Hangfire

Hangfire needs its own storage, its server, and the job type Brighter schedules against, registered alongside the factory:

```csharp
// ...
services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
services.AddHangfireServer();
services.AddSingleton<BrighterHangfireSchedulerJob>();

services.AddBrighter(options => { ... })
    .UseScheduler(new HangfireMessageSchedulerFactory())
    .AutoFromAssemblies();
```

### Switching to Quartz

Quartz supplies its own `IScheduler`, which `QuartzSchedulerFactory` wraps, so the factory is resolved from the service provider rather than constructed directly:

```csharp
// ...
services.AddBrighter(options => { ... })
    .UseScheduler(provider =>
    {
        var factory = provider.GetRequiredService<ISchedulerFactory>();
        var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
        return new QuartzSchedulerFactory(scheduler);
    })
    .AutoFromAssemblies();
```

### Switching to AWS Scheduler

```csharp
// ...
// After (Production on AWS)
services.AddBrighter(options => { ... })
    .UseScheduler(new AwsSchedulerFactory(awsConnection, "scheduler-role")
    {
        SchedulerTopicOrQueue = new RoutingKey("scheduler-topic"),
        OnConflict = OnSchedulerConflict.Overwrite
    })
    .AutoFromAssemblies();
```

**Benefits of moving to AWS Scheduler**:

- No database required
- No server maintenance
- Automatic scaling
- Pay-per-use pricing

### Switching to Azure Service Bus

```csharp
// ...
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

**Benefits of moving to Azure Service Bus Scheduler**:

- Simpler (no separate scheduler infrastructure)
- Native Azure integration
- Reduced operational complexity
- No database required

**Considerations**:

- Must configure FireAzureScheduler subscription
- No reschedule support (cancel + schedule instead)
- Requires FireAzureScheduler topic in Service Bus

## Running Two Schedulers During a Transition

You can run both schedulers during transition, choosing between them at startup, so the change can be rolled forward and back without a redeployment:

```csharp
// ...
// Run both schedulers temporarily
services.AddBrighter(options => { ... })
    .UseScheduler(provider =>
    {
        // Choose based on feature flag or configuration
        if (Configuration.GetValue<bool>("UseQuartz"))
        {
            var factory = provider.GetRequiredService<ISchedulerFactory>();
            return new QuartzSchedulerFactory(factory.GetScheduler().Result);
        }
        else
        {
            return new HangfireMessageSchedulerFactory();
        }
    })
    .AutoFromAssemblies();
```

Reverse the condition to migrate the other way.

## Further Reading

- [Scheduler](/contents/BrighterSchedulerSupport.md) - What scheduling is, and choosing a scheduler
- [Scheduling a Message](/contents/SchedulingAMessage.md) - Code and configuration examples for scheduling
- [Hangfire Scheduler](/contents/HangfireScheduler.md) - Hangfire scheduler configuration
- [Quartz Scheduler](/contents/QuartzScheduler.md) - Quartz.NET scheduler configuration
- [AWS Scheduler](/contents/AwsScheduler.md) - AWS EventBridge Scheduler configuration
- [Azure Scheduler](/contents/AzureScheduler.md) - Azure Service Bus Scheduler configuration
- [InMemory Scheduler](/contents/InMemoryScheduler.md) - InMemory scheduler for testing
