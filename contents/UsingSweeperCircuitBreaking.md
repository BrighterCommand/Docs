# Using Sweeper Circuit Breaking

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)

How to wire circuit breaking into an Outbox Sweeper, tune its cooldown, and extend it with your own or a distributed breaker. For what circuit breaking is and the options it takes, see [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md).

## Sweeper Circuit Breaking Usage Patterns

### Basic Setup with Outbox Sweeper

```csharp
// ...
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
// ...
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
// ...
// No circuit breaker registered - all topics always attempted
services.AddBrighter(/* configuration */)
    .UseOutboxSweeper();  // Sweeper without circuit breaking
```

## Sweeper Circuit Breaking Advanced Scenarios

### Custom Circuit Breaker Implementation

Implement `IAmAnOutboxCircuitBreaker` for custom behavior:

```csharp
// ...
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
// ...
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

## Further Reading

- [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md) - Configuration, monitoring and troubleshooting
- [Outbox Support](/contents/BrighterOutboxSupport.md) - The Outbox and the Sweeper
- [Distributed Lock](/contents/DistributedLock.md) - Keeping a single Sweeper active
