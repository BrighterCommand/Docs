# Hangfire Scheduler

> **Reference** · Applies to **Brighter V10**

[Hangfire](https://www.hangfire.io/) is one of the most widely used background job processing libraries in the .NET community, featuring a built-in monitoring dashboard. Brighter provides first-class integration with Hangfire for [scheduler functionality](/contents/BrighterSchedulerSupport.md), making it an excellent choice for production systems requiring easy setup, visual monitoring, and persistent scheduling.

## Production Recommendation

**Hangfire is highly recommended for production use** alongside Quartz.NET as one of the two primary production schedulers for Brighter.

### Why Choose Hangfire?

- ✅ **Built-in Dashboard**: Web UI for monitoring jobs, queues, and servers
- ✅ **Easy Setup**: Simple configuration and minimal boilerplate
- ✅ **Persistent**: Jobs stored in databases (SQL Server, PostgreSQL, Redis, etc.)
- ✅ **Reliable**: Jobs survive application restarts and crashes
- ✅ **Cancellation**: Full support for cancelling scheduled jobs
- ✅ **Automatic Retries**: Configurable retry policies for failed jobs
- ✅ **Multiple Storage**: SQL Server, PostgreSQL, MySQL, Redis, MongoDB
- ✅ **Background Processing**: Beyond scheduling - fire-and-forget, delayed, recurring jobs

### ⚠️ Important: Strong Naming Limitation

**The `Paramore.Brighter.MessageScheduler.Hangfire` assembly is NOT strong-named.**

This is due to Hangfire itself not using strong naming. If your application requires all assemblies to be strong-named (for example, for enterprise policy compliance), you will need to use an alternative scheduler (Quartz.NET or AWS/Azure schedulers).

**Why this matters:**
- Cannot mix strong-named and non-strong-named assemblies in some enterprise environments
- May cause issues with certain security policies
- Use Quartz.NET if strong naming is a hard requirement

## Hangfire Overview

Hangfire is a comprehensive background job processing library that provides:

- **Persistent Job Storage**: Store jobs in various backends
- **Automatic Retry**: Retry failed jobs with exponential backoff
- **Dashboard**: Web-based monitoring UI
- **Multiple Queue Support**: Organize jobs into different queues
- **Job Continuations**: Chain jobs together
- **Recurring Jobs**: Cron-based scheduling
- **Job Filters**: Intercept job execution
- **Distributed Processing**: Multiple servers can process jobs

For more information, visit the [Hangfire documentation](https://docs.hangfire.io/).

## How Brighter Integrates with Hangfire

Brighter integrates with Hangfire through:

1. **BrighterHangfireSchedulerJob**: A Hangfire job that executes scheduled Brighter messages
2. **HangfireMessageSchedulerFactory**: Factory that creates Brighter's message scheduler backed by Hangfire
3. **JobActivator Integration**: Uses Brighter's DI container to resolve jobs

When you schedule a message with Brighter:
1. Brighter creates a Hangfire background job with the message payload
2. Hangfire persists the job to its storage backend
3. At the scheduled time, Hangfire fires the job
4. BrighterHangfireSchedulerJob receives the job execution
5. BrighterHangfireSchedulerJob dispatches the message via Brighter's Command Processor
6. Your handler executes
7. Job appears in Hangfire dashboard with status

## Hangfire NuGet Packages

Install the required NuGet packages:

```bash
dotnet add package Paramore.Brighter.MessageScheduler.Hangfire
dotnet add package Hangfire
dotnet add package Hangfire.AspNetCore  # For dashboard
```

For persistence, add a Hangfire storage package:

```bash
# SQL Server
dotnet add package Hangfire.SqlServer

# PostgreSQL
dotnet add package Hangfire.PostgreSql

# MySQL
dotnet add package Hangfire.MySql

# Redis
dotnet add package Hangfire.Redis.StackExchange

# MongoDB
dotnet add package Hangfire.Mongo
```

## Hangfire Configuration

### Basic Configuration

Configure Brighter with Hangfire scheduler:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Hangfire;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("Hangfire"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

// Add Hangfire server
builder.Services.AddHangfireServer();

// Register Brighter's Hangfire job
builder.Services.AddSingleton<BrighterHangfireSchedulerJob>();

// Configure Brighter with Hangfire scheduler
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new HangfireMessageSchedulerFactory())
.AutoFromAssemblies();

var app = builder.Build();

// Add Hangfire dashboard
app.UseHangfireDashboard();

app.Run();
```

### Configuration with PostgreSQL

```csharp
using Hangfire.PostgreSql;

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Hangfire"));
    }));

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<BrighterHangfireSchedulerJob>();

builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new HangfireMessageSchedulerFactory())
.AutoFromAssemblies();
```

### Configuration with Redis

```csharp
using Hangfire.Redis.StackExchange;

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseRedisStorage(
        builder.Configuration.GetConnectionString("Redis"),
        new RedisStorageOptions
        {
            Prefix = "brighter:",
            ExpiryCheckInterval = TimeSpan.FromHours(1)
        }));

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<BrighterHangfireSchedulerJob>();

builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new HangfireMessageSchedulerFactory())
.AutoFromAssemblies();
```

### Configuration with Custom Queue

Organize Brighter jobs into a specific queue:

```csharp
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new HangfireMessageSchedulerFactory
{
    Queue = "brighter-scheduler"  // Custom queue for Brighter jobs
})
.AutoFromAssemblies();
```

**Benefits:**
- Isolate Brighter jobs from other Hangfire jobs
- Configure different server instances for different queues
- Monitor Brighter jobs separately in dashboard

## Storage Options

Hangfire supports multiple persistent storage backends:

### SQL Server

```csharp
using Hangfire.SqlServer;

configuration.UseSqlServerStorage(
    connectionString,
    new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
        SchemaName = "Hangfire"  // Optional custom schema
    });
```

### PostgreSQL

```csharp
using Hangfire.PostgreSql;

configuration.UsePostgreSqlStorage(options =>
{
    options.UseNpgsqlConnection(connectionString);
    options.SchemaName = "hangfire";  // Optional custom schema
});
```

### MySQL

```csharp
using Hangfire.MySql;

configuration.UseStorage(new MySqlStorage(
    connectionString,
    new MySqlStorageOptions
    {
        TransactionIsolationLevel = IsolationLevel.ReadCommitted,
        QueuePollInterval = TimeSpan.FromSeconds(15),
        JobExpirationCheckInterval = TimeSpan.FromHours(1),
        CountersAggregateInterval = TimeSpan.FromMinutes(5),
        PrepareSchemaIfNecessary = true,
        DashboardJobListLimit = 50000,
        TransactionTimeout = TimeSpan.FromMinutes(1),
        TablesPrefix = "Hangfire"
    }));
```

### Redis

```csharp
using Hangfire.Redis.StackExchange;

configuration.UseRedisStorage(
    connectionString,
    new RedisStorageOptions
    {
        Prefix = "hangfire:",
        ExpiryCheckInterval = TimeSpan.FromHours(1),
        InvisibilityTimeout = TimeSpan.FromMinutes(30)
    });
```

### In-Memory (Development Only)

```csharp
using Hangfire.MemoryStorage;

configuration.UseMemoryStorage();
```

⚠️ **Warning**: In-memory storage loses all jobs on restart. Use persistent storage for production.

## Dashboard

Hangfire includes a powerful web-based dashboard for monitoring jobs.

### Basic Dashboard Setup

```csharp
var app = builder.Build();

// Add dashboard at /hangfire
app.UseHangfireDashboard();

app.Run();
```

### Dashboard with Authentication

Secure the dashboard with authorization:

```csharp
using Hangfire.Dashboard;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow authenticated users
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}

// Apply filter
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "Brighter Scheduler Jobs"
});
```

### Dashboard with Custom Path and Options

```csharp
app.UseHangfireDashboard("/admin/jobs", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "Brighter Message Scheduler",
    StatsPollingInterval = 2000,  // Update every 2 seconds
    DisplayStorageConnectionString = false
});
```

### Dashboard Features

The Hangfire dashboard provides:

- **Jobs**: View all jobs (scheduled, processing, succeeded, failed)
- **Retries**: Monitor automatic retry attempts
- **Recurring Jobs**: Manage recurring jobs
- **Servers**: View active Hangfire servers
- **Succeeded/Failed**: Historical job execution data
- **Real-time Updates**: Live job status updates
- **Job Details**: Inspect job arguments and exceptions
- **Manual Actions**: Requeue, delete, or retry jobs

## Hangfire Code Examples

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

        // Job will appear in Hangfire dashboard
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

            // Job will be removed from Hangfire dashboard
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

        // View in dashboard: Jobs -> Scheduled
    }
}
```

## High Availability with Multiple Servers

Hangfire supports multiple server instances for high availability:

```csharp
// Server 1, 2, 3... all with same configuration
builder.Services.AddHangfire(configuration => configuration
    .UseSqlServerStorage(sharedConnectionString)  // Shared database
);

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = Environment.MachineName,  // Unique server name
    options.WorkerCount = Environment.ProcessorCount * 5,  // Worker threads
    options.Queues = new[] { "default", "brighter-scheduler" }  // Process these queues
});
```

**How HA works:**
1. Multiple servers connect to the same storage backend
2. Jobs are distributed among active servers
3. If a server fails, other servers take over its jobs
4. Each server processes jobs independently
5. Dashboard shows all servers and their status

**Best practices for HA:**
- Use shared persistent storage (SQL Server, PostgreSQL, etc.)
- Configure unique server names
- Monitor server health in dashboard
- Set appropriate worker counts per server
- Use queues to control job distribution

## Hangfire Monitoring and Observability

### Job Filters

Monitor job execution with filters:

```csharp
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;

public class BrighterJobLogFilter : IServerFilter, IApplyStateFilter
{
    private readonly ILogger<BrighterJobLogFilter> _logger;

    public BrighterJobLogFilter(ILogger<BrighterJobLogFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context)
    {
        _logger.LogInformation("Starting Brighter job {JobId}: {JobType}",
            context.BackgroundJob.Id,
            context.BackgroundJob.Job.Type.Name);
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            _logger.LogError(context.Exception,
                "Brighter job {JobId} failed",
                context.BackgroundJob.Id);
        }
        else
        {
            _logger.LogInformation("Brighter job {JobId} completed successfully",
                context.BackgroundJob.Id);
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        _logger.LogInformation("Brighter job {JobId} state changed to {State}",
            context.BackgroundJob.Id,
            context.NewState.Name);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // Optional: log state changes
    }
}

// Register filter
GlobalJobFilters.Filters.Add(new BrighterJobLogFilter(loggerFactory.CreateLogger<BrighterJobLogFilter>()));
```

### Health Checks

Monitor Hangfire health:

```csharp
public class HangfireHealthCheck : IHealthCheck
{
    private readonly JobStorage _storage;

    public HangfireHealthCheck(JobStorage storage)
    {
        _storage = storage;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoringApi = _storage.GetMonitoringApi();
            var servers = monitoringApi.Servers();

            if (!servers.Any())
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("No Hangfire servers running"));
            }

            var stats = monitoringApi.GetStatistics();
            var data = new Dictionary<string, object>
            {
                { "Servers", servers.Count },
                { "Queues", stats.Queues },
                { "Scheduled", stats.Scheduled },
                { "Processing", stats.Processing },
                { "Succeeded", stats.Succeeded },
                { "Failed", stats.Failed }
            };

            return Task.FromResult(HealthCheckResult.Healthy("Hangfire is running", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire check failed", ex));
        }
    }
}

// Register health check
builder.Services.AddHealthChecks()
    .AddCheck<HangfireHealthCheck>("hangfire");
```

## Hangfire Best Practices

### 1. Always Use Persistent Storage in Production

```csharp
// Good - Persistent storage
configuration.UseSqlServerStorage(connectionString);

// Bad - In-memory storage in production
configuration.UseMemoryStorage();
```

### 2. Secure the Dashboard

```csharp
// Good - Require authentication
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Bad - Publicly accessible dashboard
app.UseHangfireDashboard();  // No authorization
```

### 3. Use Custom Queues for Organization

```csharp
// Good - Separate queue for Brighter jobs
.UseScheduler(new HangfireMessageSchedulerFactory
{
    Queue = "brighter-scheduler"
})

// Consider - Everything in default queue (harder to manage)
.UseScheduler(new HangfireMessageSchedulerFactory())
```

### 4. Store Scheduler IDs for Cancellation

```csharp
// Good - Store ID for later cancellation
var schedulerId = await _commandProcessor.SendAsync(delay, command);
entity.SchedulerId = schedulerId;
await _repository.SaveAsync(entity);

// Bad - Lost ability to cancel
await _commandProcessor.SendAsync(delay, command);  // ID discarded
```

### 5. Configure Appropriate Worker Counts

```csharp
// Good - Based on workload
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5;  // Adjust based on I/O vs CPU
});

// Avoid - Too many workers (resource exhaustion)
options.WorkerCount = 1000;

// Avoid - Too few workers (slow processing)
options.WorkerCount = 1;
```

### 6. Monitor the Dashboard Regularly

```csharp
// Good - Check dashboard periodically
// Navigate to /hangfire to view:
// - Failed jobs (investigate and retry)
// - Long-running jobs (potential issues)
// - Server status (ensure all servers active)
```

### 7. Handle Timezone Correctly

```csharp
// Good - Use UTC for consistency
var scheduledTime = DateTimeOffset.UtcNow.AddHours(1);

// Bad - Local time (ambiguous across servers)
var scheduledTime = DateTimeOffset.Now.AddHours(1);
```

### 8. Configure Automatic Retry Policies

```csharp
// Good - Automatic retry for transient failures
builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

// Hangfire automatically retries failed jobs
// View retry attempts in dashboard
```

## Hangfire Troubleshooting

### Jobs Not Executing

**Symptom**: Jobs scheduled but never execute

**Possible Causes:**
1. Hangfire server not started
2. Database connectivity issues
3. BrighterHangfireSchedulerJob not registered
4. No workers processing the queue

**Solutions:**
```csharp
// Verify Hangfire server is started
builder.Services.AddHangfireServer();

// Check BrighterHangfireSchedulerJob is registered
builder.Services.AddSingleton<BrighterHangfireSchedulerJob>();

// Check dashboard for server status
// Navigate to /hangfire/servers
```

### Jobs Executing Multiple Times

**Symptom**: Same job executes multiple times

**Cause**: Multiple servers or configuration issues

**Solution:**
```csharp
// Ensure unique job IDs
// Hangfire uses job IDs to prevent duplicates
// Check dashboard for duplicate jobs

// Verify storage is shared across servers
// All servers must use the same connection string
```

### Dashboard Not Loading

**Symptom**: Dashboard URL returns 404

**Possible Causes:**
1. Dashboard not registered
2. Incorrect URL path
3. Authentication blocking access

**Solutions:**
```csharp
// Ensure UseHangfireDashboard is called
app.UseHangfireDashboard("/hangfire");

// Check the path matches your URL
// Navigate to: http://localhost:5000/hangfire

// Verify authorization filter allows access
// Temporarily disable auth to test:
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});
```

## Related Documentation

- [Brighter Scheduler Support](BrighterSchedulerSupport.md) - Overview of scheduling in Brighter
- [Switching Schedulers](/contents/SwitchingSchedulers.md) - Moving to or from this scheduler
- [InMemory Scheduler](InMemoryScheduler.md) - Lightweight scheduler for testing
- [Quartz Scheduler](QuartzScheduler.md) - Alternative production scheduler with strong naming
- [AWS Scheduler](AwsScheduler.md) - Cloud-native AWS scheduling
- [Azure Scheduler](AzureScheduler.md) - Cloud-native Azure scheduling
- [Hangfire Documentation](https://docs.hangfire.io/) - Official Hangfire documentation

## Hangfire Summary

Hangfire is an excellent production scheduler for Brighter offering:
- ✅ **Dashboard**: Built-in web UI for monitoring and management
- ✅ **Easy Setup**: Minimal configuration required
- ✅ **Persistent**: Jobs survive restarts with multiple storage options
- ✅ **Reliable**: Automatic retries and distributed processing
- ✅ **Visual Monitoring**: Real-time job status and history
- ⚠️ **Strong Naming**: NOT strong-named (use Quartz if required)

Use Hangfire when you need a production scheduler with a built-in dashboard and easy setup. If strong naming is required, use Quartz.NET instead.
