---
description: "Quartz.NET is one of the most widely used enterprise-grade scheduling libraries in the .NET community."
layout:
  description:
    visible: false
---

# Quartz Scheduler

> **Reference** · Applies to **Brighter V10**

[Quartz.NET](https://www.quartz-scheduler.net/) is one of the most widely used enterprise-grade scheduling libraries in the .NET community. Brighter provides first-class integration with Quartz for [scheduler functionality](/contents/BrighterSchedulerSupport.md), making it an excellent choice for production systems requiring durable, distributed scheduling.

## Quartz.NET Overview

Quartz.NET is a full-featured, open-source job scheduling system that can be used from smallest apps to large-scale enterprise systems. It provides:

- **Persistent Job Stores**: Store jobs and triggers in databases
- **Clustering**: Run multiple scheduler instances for HA
- **Trigger Types**: Simple, Cron, Calendar-based triggers
- **Job Chains**: Execute jobs in sequence
- **Job Data**: Pass data to jobs
- **Listeners**: Monitor job execution
- **Plugins**: Extend functionality

For more information, visit the [Quartz.NET documentation](https://www.quartz-scheduler.net/documentation/index.html).

## How Brighter Integrates with Quartz

Brighter integrates with Quartz through:

1. **QuartzBrighterJob**: A Quartz job that executes scheduled Brighter messages
2. **BrighterResolver**: Custom job factory that resolves jobs from DI container
3. **QuartzSchedulerFactory**: Factory that creates Brighter's message scheduler backed by Quartz

When you schedule a message with Brighter:

1. Brighter creates a Quartz job with the message payload
2. Quartz persists the job to its job store
3. At the scheduled time, Quartz fires the job
4. QuartzBrighterJob receives the job execution
5. QuartzBrighterJob dispatches the message via Brighter's Command Processor
6. Your handler executes

## Quartz NuGet Packages

Install the required NuGet packages:

```bash
dotnet add package Paramore.Brighter.MessageScheduler.Quartz
dotnet add package Quartz
dotnet add package Quartz.Extensions.Hosting
```

For persistence, add a Quartz job store package:

```bash
# SQL Server
dotnet add package Quartz.Serialization.Json
# Add Quartz.Extensions.DependencyInjection for SQL configuration

# Or PostgreSQL, MySQL, etc. - see Quartz documentation
```

## Quartz Configuration

### Basic Configuration

Configure Brighter with Quartz scheduler:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Quartz;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    // Use unique instance name for clustering
    q.SchedulerId = "AUTO";  // Or provide unique ID

    // Register Brighter's job type
    q.UseMicrosoftDependencyInjectionJobFactory();
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// Register Brighter's Quartz job
builder.Services.AddSingleton<QuartzBrighterJob>();

// Configure Brighter with Quartz scheduler
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    return new QuartzSchedulerFactory(scheduler);
})
.AutoFromAssemblies();

var app = builder.Build();
```

### Configuration with Persistent Job Store

For production, use a persistent job store:

```csharp
builder.Services.AddQuartz(q =>
{
    q.SchedulerId = "BrighterScheduler";

    // Configure persistent job store (SQL Server example)
    q.UsePersistentStore(store =>
    {
        store.UseProperties = true;  // Recommended for better compatibility
        store.RetryInterval = TimeSpan.FromSeconds(15);
        store.UseSqlServer(sqlServer =>
        {
            sqlServer.ConnectionString = builder.Configuration.GetConnectionString("Quartz");
            sqlServer.TablePrefix = "QRTZ_";
        });
        store.UseJsonSerializer();  // Use JSON serialization for job data
    });

    // Optional: Enable clustering for HA
    q.UseClustering(clustering =>
    {
        clustering.CheckinInterval = TimeSpan.FromSeconds(20);
        clustering.CheckinMisfireThreshold = TimeSpan.FromSeconds(30);
    });

    // Use Microsoft DI for job creation
    q.UseMicrosoftDependencyInjectionJobFactory();
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// Register Brighter's Quartz job
builder.Services.AddSingleton<QuartzBrighterJob>();

// Configure Brighter
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    return new QuartzSchedulerFactory(scheduler);
})
.AutoFromAssemblies();
```

### Configuration with appsettings.json

You can configure Quartz via appsettings.json:

```json
{
  "Quartz": {
    "quartz.scheduler.instanceName": "BrighterScheduler",
    "quartz.scheduler.instanceId": "AUTO",
    "quartz.jobStore.type": "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
    "quartz.jobStore.useProperties": "true",
    "quartz.jobStore.dataSource": "default",
    "quartz.jobStore.tablePrefix": "QRTZ_",
    "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz",
    "quartz.dataSource.default.provider": "SqlServer",
    "quartz.dataSource.default.connectionString": "Server=localhost;Database=Quartz;Trusted_Connection=True;",
    "quartz.serializer.type": "json",
    "quartz.jobStore.clustered": "true",
    "quartz.jobStore.clusterCheckinInterval": "20000"
  }
}
```

Then configure with:

```csharp
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Load configuration from appsettings
    var quartzConfig = builder.Configuration.GetSection("Quartz");
    q.UseProperties = true;

    // Apply configuration
    foreach (var item in quartzConfig.AsEnumerable())
    {
        if (!string.IsNullOrEmpty(item.Value))
        {
            q.SetProperty(item.Key.Replace("Quartz:", ""), item.Value);
        }
    }
});
```

## Persistence Options

Quartz supports multiple persistent job stores:

### SQL Server

```csharp
q.UsePersistentStore(store =>
{
    store.UseSqlServer(sqlServer =>
    {
        sqlServer.ConnectionString = connectionString;
        sqlServer.TablePrefix = "QRTZ_";
    });
    store.UseJsonSerializer();
});
```

**Database Scripts**: [SQL Server Scripts](https://github.com/quartznet/quartznet/tree/main/database/tables)

### PostgreSQL

```csharp
q.UsePersistentStore(store =>
{
    store.UsePostgres(postgres =>
    {
        postgres.ConnectionString = connectionString;
        postgres.TablePrefix = "qrtz_";
    });
    store.UseJsonSerializer();
});
```

**Database Scripts**: [PostgreSQL Scripts](https://github.com/quartznet/quartznet/tree/main/database/tables)

### MySQL

```csharp
q.UsePersistentStore(store =>
{
    store.UseMySql(mysql =>
    {
        mysql.ConnectionString = connectionString;
        mysql.TablePrefix = "QRTZ_";
    });
    store.UseJsonSerializer();
});
```

**Database Scripts**: [MySQL Scripts](https://github.com/quartznet/quartznet/tree/main/database/tables)

### In-Memory (Development Only)

For development, you can use the in-memory store (no persistence):

```csharp
// No UsePersistentStore call needed - in-memory is default
q.UseMicrosoftDependencyInjectionJobFactory();
```

**Warning**: In-memory store loses all jobs on restart. Use persistent store for production.

## Advanced Configuration

### Custom Scheduler ID Generation

By default, Brighter uses `Guid.NewGuid()` for scheduler IDs. Customize this behavior:

```csharp
builder.Services.UseScheduler(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

    return new QuartzSchedulerFactory(scheduler)
    {
        // Customize message scheduler ID
        GetOrCreateMessageSchedulerId = message => message.Id,

        // Customize request scheduler ID
        GetOrCreateRequestSchedulerId = request =>
        {
            if (request is MyCommand command)
            {
                return command.CorrelationId.ToString();
            }
            return request.Id.ToString();
        }
    };
})
```

**Why customize?**

- Use business-meaningful IDs for better tracking
- Correlate scheduled jobs with business entities
- Simplify debugging and monitoring

### Quartz Job Groups

Organize jobs into groups:

```csharp
return new QuartzSchedulerFactory(scheduler)
{
    Group = "BrighterJobs"  // All Brighter jobs in this group
};
```

**Benefits:**

- Logical organization of jobs
- Easier monitoring and management
- Group-level operations (pause, resume, delete)

### Misfire Handling

Configure how Quartz handles missed triggers:

```csharp
builder.Services.AddQuartz(q =>
{
    q.UseMisfire​Handler(options =>
    {
        options.MisfireThreshold = TimeSpan.FromMinutes(1);
    });

    // Additional Quartz configuration...
});
```

## Quartz Code Examples

### Basic Scheduling

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);

        // Schedule order processing for 1 hour later
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromHours(1),
            new ProcessOrderCommand { OrderId = order.Id }
        );

        // Store scheduler ID for potential cancellation
        order.ProcessSchedulerId = schedulerId;
        await _repository.UpdateAsync(order);
    }
}
```

### Cancelling a Scheduled Job

```csharp
public class OrderService
{
    private readonly IMessageScheduler _scheduler;

    public async Task CancelOrder(Guid orderId)
    {
        var order = await _repository.GetAsync(orderId);

        // Cancel the scheduled processing
        if (!string.IsNullOrEmpty(order.ProcessSchedulerId))
        {
            await _scheduler.CancelAsync(order.ProcessSchedulerId);
            _logger.LogInformation("Cancelled scheduled processing for order {OrderId}", orderId);
        }

        order.Status = OrderStatus.Cancelled;
        await _repository.UpdateAsync(order);
    }
}
```

### Scheduling with Absolute Time

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

## Clustering and High Availability

Quartz supports clustering for high availability:

```csharp
builder.Services.AddQuartz(q =>
{
    q.SchedulerId = "AUTO";  // Each instance gets unique ID

    q.UsePersistentStore(store =>
    {
        store.UseSqlServer(sqlServer =>
        {
            sqlServer.ConnectionString = connectionString;
        });
        store.UseJsonSerializer();
    });

    // Enable clustering
    q.UseClustering(clustering =>
    {
        clustering.CheckinInterval = TimeSpan.FromSeconds(20);
        clustering.CheckinMisfireThreshold = TimeSpan.FromSeconds(30);
    });
});
```

**How clustering works:**

1. Multiple scheduler instances share the same job store (database)
2. Each instance regularly checks in with the job store
3. If an instance fails, other instances take over its jobs
4. Only one instance executes each job (no duplicates)
5. Provides automatic failover and load distribution

**Best practices for clustering:**

- Use `SchedulerId = "AUTO"` for unique instance IDs
- Set appropriate check-in intervals (default 20 seconds)
- Use persistent job store (required for clustering)
- Ensure clocks are synchronized across instances (NTP)
- Monitor instance health and check-ins

## Quartz Monitoring and Observability

### Quartz Listeners

Monitor job execution with listeners:

```csharp
public class BrighterJobListener : IJobListener
{
    private readonly ILogger<BrighterJobListener> _logger;

    public BrighterJobListener(ILogger<BrighterJobListener> logger)
    {
        _logger = logger;
    }

    public string Name => "BrighterJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Job {JobKey} about to execute", context.JobDetail.Key);
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        if (jobException != null)
        {
            _logger.LogError(jobException, "Job {JobKey} failed", context.JobDetail.Key);
        }
        else
        {
            _logger.LogInformation("Job {JobKey} completed successfully", context.JobDetail.Key);
        }
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Job {JobKey} execution vetoed", context.JobDetail.Key);
        return Task.CompletedTask;
    }
}

// Register listener
builder.Services.AddQuartz(q =>
{
    q.AddJobListener<BrighterJobListener>();
    // ... other configuration
});
```

### Health Checks

Monitor Quartz scheduler health:

```csharp
public class QuartzHealthCheck : IHealthCheck
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzHealthCheck(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            if (!scheduler.IsStarted)
            {
                return HealthCheckResult.Unhealthy("Scheduler is not started");
            }

            var metadata = await scheduler.GetMetaData(cancellationToken);
            var data = new Dictionary<string, object>
            {
                { "SchedulerName", metadata.SchedulerName },
                { "SchedulerInstanceId", metadata.SchedulerInstanceId },
                { "NumberOfJobsExecuted", metadata.NumberOfJobsExecuted },
                { "RunningSince", metadata.RunningSince }
            };

            return HealthCheckResult.Healthy("Scheduler is running", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Scheduler check failed", ex);
        }
    }
}

// Register health check
builder.Services.AddHealthChecks()
    .AddCheck<QuartzHealthCheck>("quartz");
```

## Quartz Best Practices

### 1. Always Use Persistent Job Store in Production

```csharp
// Good - Persistent store
q.UsePersistentStore(store =>
{
    store.UseSqlServer(connectionString);
    store.UseJsonSerializer();
});

// Bad - In-memory store in production
// No persistent store configuration (uses RAM store)
```

### 2. Enable Clustering for High Availability

```csharp
// Good - Clustered for HA
q.UseClustering(clustering =>
{
    clustering.CheckinInterval = TimeSpan.FromSeconds(20);
});

// Consider - Single instance (SPOF)
// No clustering configuration
```

### 3. Store Scheduler IDs for Cancellation

```csharp
// Good - Store ID for later cancellation
var schedulerId = await _commandProcessor.SendAsync(delay, command);
entity.SchedulerId = schedulerId;
await _repository.SaveAsync(entity);

// Bad - Lost ability to cancel
await _commandProcessor.SendAsync(delay, command);  // ID discarded
```

### 4. Use JSON Serialization

```csharp
// Good - JSON serialization (more portable)
store.UseJsonSerializer();

// Avoid - Binary serialization (versioning issues)
store.UseBinarySerializer();
```

### 5. Configure Appropriate Check-In Intervals

```csharp
// Good - Balanced check-in
q.UseClustering(clustering =>
{
    clustering.CheckinInterval = TimeSpan.FromSeconds(20);
    clustering.CheckinMisfireThreshold = TimeSpan.FromSeconds(30);
});

// Avoid - Too frequent (database load)
clustering.CheckinInterval = TimeSpan.FromSeconds(1);

// Avoid - Too infrequent (slow failover)
clustering.CheckinInterval = TimeSpan.FromMinutes(5);
```

### 6. Monitor Job Execution

```csharp
// Good - Add listeners for monitoring
q.AddJobListener<BrighterJobListener>();
q.AddTriggerListener<BrighterTriggerListener>();
```

### 7. Handle Timezone Correctly

```csharp
// Good - Use UTC for consistency
var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);

// Bad - Local time (ambiguous across instances)
var scheduledTime = DateTimeOffset.Now.AddHours(1);
```

### 8. Set Appropriate Connection Pool Size

```csharp
// For high-volume systems, increase connection pool
"Server=localhost;Database=Quartz;Max Pool Size=50;..."
```

## Quartz Troubleshooting

### Jobs Not Executing

**Symptom**: Jobs scheduled but never execute

**Possible Causes**:

1. Quartz scheduler not started
2. Database connectivity issues
3. QuartzBrighterJob not registered
4. Misfire threshold exceeded

**Solutions**:

```csharp
// Verify scheduler is started
var scheduler = await _schedulerFactory.GetScheduler();
var isStarted = scheduler.IsStarted;

// Check QuartzBrighterJob is registered
services.AddSingleton<QuartzBrighterJob>();

// Verify database connectivity
// Check Quartz logs for errors
```

### Jobs Executing Multiple Times

**Symptom**: Same job executes on multiple instances

**Cause**: Clustering not configured or clock drift

**Solution**:

```csharp
// Enable clustering
q.UseClustering();

// Ensure clocks are synchronized (use NTP)
// Check for clock drift across instances
```

### Database Deadlocks

**Symptom**: Database deadlocks in Quartz tables

**Cause**: High concurrency with insufficient connection pool

**Solution**:
```csharp
// Increase connection pool size
"Server=localhost;Database=Quartz;Max Pool Size=100;..."

// Reduce clustering check-in frequency
clustering.CheckinInterval = TimeSpan.FromSeconds(30);

// Consider database-specific optimizations
// (indexes, isolation levels, etc.)
```

## Related Documentation

- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [Switching Schedulers](/contents/SwitchingSchedulers.md) - Moving to or from this scheduler
- [InMemory Scheduler](InMemoryScheduler.md) - Lightweight scheduler for testing
- [Hangfire Scheduler](HangfireScheduler.md) - Alternative production scheduler with dashboard
- [AWS Scheduler](AwsScheduler.md) - Cloud-native AWS scheduling
- [Azure Scheduler](AzureScheduler.md) - Cloud-native Azure scheduling
- [Quartz.NET Documentation](https://www.quartz-scheduler.net/documentation/index.html) - Official Quartz documentation

## Quartz Summary

Quartz.NET is an excellent production scheduler for Brighter offering:

- **Durability**: Jobs persisted to database survive restarts
- **Clustering**: High availability with automatic failover
- **Maturity**: Battle-tested in production for years
- **Flexibility**: Rich API for complex scheduling scenarios
- **Reliability**: Strong guarantees for job execution
- **Monitoring**: Comprehensive monitoring and observability
- **Strong Naming**: Fully signed assemblies for enterprise use

Use Quartz when you need a robust, enterprise-grade scheduler with clustering support and don't require a built-in dashboard (otherwise consider Hangfire).
