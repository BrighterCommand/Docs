# Sweeper Circuit Breaking

> **Reference** · Applies to **Brighter V10**

## Overview

Sweeper Circuit Breaking is a resilience feature that prevents failures to publish to one topic from blocking attempts to publish to other topics, when publishing messages from the Outbox. When a topic repeatedly fails to publish, the circuit breaker "trips" that topic, temporarily preventing further publish attempts until a cooldown period expires.

This feature is particularly valuable in scenarios where:

- **Transport failures**: A message broker or queue becomes unavailable
- **Topic-specific issues**: A specific topic/queue has configuration problems or capacity issues
- **Cascade prevention**: Failing topics would otherwise block the Outbox Sweeper from processing healthy topics
- **Resource protection**: Repeated failures to unhealthy topics consume resources without benefit

## How It Work

The Sweeper Circuit Breaker operates at the topic level during Outbox clearing operations:

### Normal Operation

1. **Outbox Sweeper runs**: Periodically attempts to clear outstanding messages from the Outbox
2. **Messages grouped by topic**: Messages are organized by their routing key (topic)
3. **Publish attempts**: The sweeper attempts to publish messages to their respective topics
4. **Success**: Messages are published and marked as dispatched

### Circuit Breaking Behavior

When a topic fails to publish:

1. **Failure detected**: An exception occurs during message publication to a specific topic
2. **Circuit trips**: The circuit breaker marks that topic as "tripped"
3. **Cooldown begins**: A cooldown counter is set for the tripped topic (default: 10 sweeps)
4. **Subsequent sweeps**: On each sweep, the cooldown counter decrements for all tripped topics
5. **Recovery**: When the cooldown reaches zero, the topic is removed from the tripped list
6. **Retry**: The topic becomes available for publishing attempts again

### Benefits

- **Prevents blocking**: Healthy topics continue to be processed even when some topics fail
- **Automatic recovery**: Topics automatically recover after the cooldown period
- **Resource efficiency**: Avoids wasting resources on repeated failures to unhealthy topics
- **Observability**: Tripped topics can be monitored and alerted on

## Configuration

### Enabling Circuit Breaking

To enable Sweeper Circuit Breaking, register an `IAmAnOutboxCircuitBreaker` implementation with your IoC container:

```csharp
using Paramore.Brighter.CircuitBreaker;

public void ConfigureServices(IServiceCollection services)
{
    // Register the circuit breaker
    services.AddSingleton<IAmAnOutboxCircuitBreaker>(
        new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
        {
            CooldownCount = 10  // Number of sweeps before recovery (default: 10)
        })
    );

    services.AddBrighter(options =>
    {
        // Configure Brighter as normal
    })
    .AddProducers(/* producer configuration */)
    .UseOutboxSweeper();  // Enable the Outbox Sweeper
}
```

### Configuration Options

The `OutboxCircuitBreakerOptions` class provides the following configuration:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| **CooldownCount** | `int` | 10 | Number of sweeper iterations before a tripped topic is eligible for retry |

### Calculating Cooldown Time

The actual cooldown time depends on your Outbox Sweeper configuration:

**Formula**: `Cooldown Time = CooldownCount × SweepInterval`

**Example**:

- `CooldownCount = 10`
- Sweeper runs every 60 seconds
- **Cooldown Time = 10 × 60s = 10 minutes**

```csharp
services.AddBrighter(options =>
{
    // Sweeper configuration
    options.OutboxSweeper = new OutboxSweeperOptions
    {
        SweepInterval = TimeSpan.FromSeconds(60)  // Sweep every 60 seconds
    };
})
.UseOutboxSweeper();

// Circuit breaker with 10 cooldown sweeps = 10 minutes total cooldown
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 10  // 10 sweeps × 60s = 10 minutes
    })
);
```

## Usage Patterns

### Basic Setup with Outbox Sweeper

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register circuit breaker
    services.AddSingleton<IAmAnOutboxCircuitBreaker>(
        new InMemoryOutboxCircuitBreaker()  // Uses default cooldown of 10 sweeps
    );

    services.AddBrighter(options =>
    {
        options.OutboxSweeper = new OutboxSweeperOptions
        {
            SweepInterval = TimeSpan.FromMinutes(1),  // Sweep every minute
            BatchSize = 100  // Process up to 100 messages per sweep
        };
    })
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = /* your producer registry */;
    })
    .UseMsSqlOutbox(/* outbox configuration */)
    .UseOutboxSweeper();  // Enable sweeper with circuit breaking
}
```

### Custom Cooldown Configuration

Adjust the cooldown based on your needs:

```csharp
// Short cooldown for quickly recovering topics
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 3  // Recover after 3 sweeps
    })
);

// Long cooldown for persistent issues
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 30  // Recover after 30 sweeps
    })
);
```

### Without Circuit Breaking

If you don't register an `IAmAnOutboxCircuitBreaker`, the sweeper will continue to attempt publishing to all topics even after failures:

```csharp
// No circuit breaker registered - all topics always attempted
services.AddBrighter(/* configuration */)
    .UseOutboxSweeper();  // Sweeper without circuit breaking
```

## Monitoring and Observability

### Checking Tripped Topics

You can query the circuit breaker to see which topics are currently tripped:

```csharp
public class OutboxMonitorService
{
    private readonly IAmAnOutboxCircuitBreaker _circuitBreaker;
    private readonly ILogger<OutboxMonitorService> _logger;

    public OutboxMonitorService(
        IAmAnOutboxCircuitBreaker circuitBreaker,
        ILogger<OutboxMonitorService> logger)
    {
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }

    public void CheckCircuitBreakerStatus()
    {
        var trippedTopics = _circuitBreaker.TrippedTopics;

        if (trippedTopics.Any())
        {
            _logger.LogWarning(
                "Circuit breaker has {Count} tripped topics: {Topics}",
                trippedTopics.Count(),
                string.Join(", ", trippedTopics.Select(t => t.Value))
            );
        }
        else
        {
            _logger.LogInformation("All topics are healthy");
        }
    }
}
```

### Logging and Alerts

Set up monitoring to track circuit breaker events:

```csharp
public class CircuitBreakerHealthCheck : IHealthCheck
{
    private readonly IAmAnOutboxCircuitBreaker _circuitBreaker;

    public CircuitBreakerHealthCheck(IAmAnOutboxCircuitBreaker circuitBreaker)
    {
        _circuitBreaker = circuitBreaker;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var trippedTopics = _circuitBreaker.TrippedTopics.ToList();

        if (!trippedTopics.Any())
        {
            return Task.FromResult(HealthCheckResult.Healthy("No tripped topics"));
        }

        var data = new Dictionary<string, object>
        {
            { "tripped_topics", trippedTopics.Select(t => t.Value).ToArray() },
            { "count", trippedTopics.Count }
        };

        return Task.FromResult(
            HealthCheckResult.Degraded(
                $"{trippedTopics.Count} topic(s) are currently tripped",
                data: data
            )
        );
    }
}

// Register health check
services.AddHealthChecks()
    .AddCheck<CircuitBreakerHealthCheck>("outbox_circuit_breaker");
```

## Transport-Specific Integration

Brighter V10 includes circuit breaking integration with specific transports:

### MongoDB Transport

Circuit breaking is fully integrated with MongoDB Outbox:

```csharp
services.AddBrighter(/* configuration */)
    .UseMongoDbOutbox(/* MongoDB configuration */)
    .UseOutboxSweeper();

// Circuit breaker works automatically with MongoDB transport
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker()
);
```

### Other Transports

Circuit breaking works with all Brighter Outbox implementations:

- **MS SQL Server** (`UseMsSqlOutbox`)
- **PostgreSQL** (`UsePostgreSqlOutbox`)
- **MySQL** (`UseMySqlOutbox`)
- **SQLite** (`UseSqliteOutbox`)
- **DynamoDB** (`UseDynamoDbOutbox`)
- **MongoDB** (`UseMongoDbOutbox`)

## Bulk Dispatch Support

V10 includes proper circuit breaking support for bulk dispatch operations. When dispatching multiple messages in a batch:

1. **Batch grouping**: Messages are grouped by topic
2. **Per-topic circuit breaking**: Each topic's circuit breaker status is checked before dispatching
3. **Healthy topics proceed**: Only topics that aren't tripped are dispatched
4. **Individual retry**: Failed batches can be retried individually per topic

```csharp
// Bulk dispatch respects circuit breaker state
await commandProcessor.ClearOutboxAsync(
    messageIds,  // List of message IDs to dispatch
    continueOnCapturedContext: false,
    cancellationToken: cancellationToken
);
```

## Best Practices

### 1. Choose Appropriate Cooldown Periods

Balance between quick recovery and avoiding repeated failures:

- **Short cooldown (3-5 sweeps)**: For transient issues, quick recovery desired
- **Medium cooldown (10-15 sweeps)**: General purpose, good balance
- **Long cooldown (20-30 sweeps)**: For persistent issues, reduce retry overhead

### 2. Align Cooldown with Sweep Interval

Consider the total cooldown time:

```csharp
// Fast sweeping with short cooldown = quick recovery
options.OutboxSweeper = new OutboxSweeperOptions
{
    SweepInterval = TimeSpan.FromSeconds(30)  // 30s sweep
};

services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 5  // 5 × 30s = 2.5 minutes total cooldown
    })
);
```

### 3. Monitor Tripped Topics

Set up monitoring and alerting:

- **Health checks**: Use ASP.NET Core health checks to expose tripped topics
- **Metrics**: Export circuit breaker metrics to Prometheus, DataDog, etc.
- **Logging**: Log when topics trip and recover
- **Alerts**: Alert when topics remain tripped for extended periods

### 4. Investigate Root Causes

When topics trip repeatedly:

1. **Check broker health**: Ensure message broker is operational
2. **Verify permissions**: Ensure the application has permissions to publish
3. **Check queue/topic existence**: Verify the destination exists
4. **Review capacity**: Check if the queue/topic has reached capacity limits
5. **Inspect network**: Look for network connectivity issues

### 5. Use with Outbox Sweeper

Circuit breaking is designed to work with the Outbox Sweeper:

```csharp
// Always enable UseOutboxSweeper when using circuit breaking
services.AddBrighter(/* configuration */)
    .UseOutboxSweeper()  // Required for circuit breaking to function
    .UseMsSqlOutbox(/* outbox config */);
```

### 6. Consider Immediate vs. Sweeper Clearing

Circuit breaking only applies to **sweeper-based clearing**:

```csharp
// Immediate clearing - NOT subject to circuit breaking
await postBox.ClearOutboxAsync(messageIds);

// Sweeper clearing - subject to circuit breaking
// Happens automatically via UseOutboxSweeper
```

### 7. Test Failure Scenarios

Regularly test circuit breaker behavior:

- Simulate broker outages
- Test individual topic failures
- Verify healthy topics continue processing
- Confirm automatic recovery after cooldown

## Troubleshooting

### Topics Not Recovering

**Problem**: Topics remain tripped indefinitely

**Solutions**:

1. Verify Outbox Sweeper is running
2. Check cooldown count is not excessively high
3. Ensure sweeper interval is appropriate
4. Confirm circuit breaker is properly registered

### All Topics Tripping

**Problem**: All topics become tripped at once

**Possible Causes**:

- Broker is completely down
- Network connectivity issues
- Authentication/authorization failures
- Shared resource exhaustion

**Solutions**:

1. Check broker health and connectivity
2. Verify credentials and permissions
3. Review broker logs for errors
4. Consider infrastructure capacity

### Messages Stuck in Outbox

**Problem**: Messages accumulate in Outbox without being dispatched

**Check**:

1. Is the Outbox Sweeper enabled?
2. Are topics currently tripped? Check `TrippedTopics`
3. Is the circuit breaker cooldown too long?
4. Are there persistent transport issues?

**Solutions**:

- Enable Outbox Sweeper if not already enabled
- Investigate why topics are tripping
- Reduce cooldown count if appropriate
- Fix underlying transport issues

### Circuit Breaker Not Working

**Problem**: Failed topics continue to be retried

**Verify**:

1. Circuit breaker is registered: `services.AddSingleton<IAmAnOutboxCircuitBreaker>`
2. Using the sweeper: `UseOutboxSweeper()`
3. Exceptions are being thrown during publish (not silently failing)
4. Circuit breaker implementation is correct

## Advanced Scenarios

### Custom Circuit Breaker Implementation

Implement `IAmAnOutboxCircuitBreaker` for custom behavior:

```csharp
public class CustomOutboxCircuitBreaker : IAmAnOutboxCircuitBreaker
{
    private readonly Dictionary<RoutingKey, CircuitBreakerState> _topics = new();

    public void TripTopic(RoutingKey topic)
    {
        _topics[topic] = new CircuitBreakerState
        {
            TrippedAt = DateTime.UtcNow,
            FailureCount = _topics.ContainsKey(topic)
                ? _topics[topic].FailureCount + 1
                : 1
        };

        // Custom logic: Log, emit metrics, send alerts, etc.
    }

    public void CoolDown()
    {
        var now = DateTime.UtcNow;
        var recovered = new List<RoutingKey>();

        foreach (var kvp in _topics)
        {
            var cooldownPeriod = TimeSpan.FromMinutes(10);
            if (now - kvp.Value.TrippedAt > cooldownPeriod)
            {
                recovered.Add(kvp.Key);
            }
        }

        foreach (var topic in recovered)
        {
            _topics.Remove(topic);
            // Custom logic: Log recovery, emit metrics, etc.
        }
    }

    public IEnumerable<RoutingKey> TrippedTopics => _topics.Keys;
}
```

### Distributed Circuit Breaker

For multi-instance deployments, consider a distributed circuit breaker using Redis, SQL, or other shared storage:

```csharp
public class DistributedOutboxCircuitBreaker : IAmAnOutboxCircuitBreaker
{
    private readonly IDistributedCache _cache;

    public void TripTopic(RoutingKey topic)
    {
        var key = $"circuit-breaker:{topic.Value}";
        _cache.SetString(key, DateTime.UtcNow.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
    }

    // Implement other methods using distributed cache
}
```

## Summary

Sweeper Circuit Breaking provides automatic resilience for Outbox clearing operations by:

- **Preventing cascade failures** when specific topics fail
- **Automatically recovering** after a configurable cooldown period
- **Allowing healthy topics** to continue processing
- **Protecting resources** from repeated failures

Enable circuit breaking by registering `IAmAnOutboxCircuitBreaker` with your IoC container and configuring appropriate cooldown periods for your application's needs.
