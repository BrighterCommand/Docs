---
description: "How to wire circuit breaking into an Outbox Sweeper, tune its cooldown, and extend it with your own or a distributed breaker."
layout:
  description:
    visible: false
---

# Using Sweeper Circuit Breaking

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)

How to wire circuit breaking into an Outbox Sweeper, tune its cooldown, and extend it with your own or a distributed breaker. For what circuit breaking is and the options it takes, see [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md).

## Sweeper Circuit Breaking Usage Patterns

### Basic Setup with Outbox Sweeper

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MsSql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MsSql;

public void ConfigureServices(IServiceCollection services)
{
    // Register circuit breaker
    services.AddSingleton<IAmAnOutboxCircuitBreaker>(
        new InMemoryOutboxCircuitBreaker()  // Uses default cooldown of 10 sweeps
    );

    // ... producerRegistry and outboxConfiguration come from your transport
    // and your database configuration
    services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);

    services.AddBrighter()
        .AddProducers(configure =>
        {
            configure.ProducerRegistry = producerRegistry;
            configure.Outbox = new MsSqlOutbox(outboxConfiguration);
            configure.ConnectionProvider = typeof(MsSqlConnectionProvider);
            configure.TransactionProvider = typeof(MsSqlTransactionProvider);
        })
        .UseOutboxSweeper(options =>       // Enable sweeper with circuit breaking
        {
            options.TimerInterval = 60;    // Sweep every 60 seconds
            options.BatchSize = 100;       // Process up to 100 messages per sweep
        });
}
```

### Custom Cooldown Configuration

Adjust the cooldown based on your needs:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.CircuitBreaker;

// Short cooldown for quickly recovering topics
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 3  // Sit out 3 sweeps, retry on the 4th
    })
);

// Long cooldown for persistent issues
services.AddSingleton<IAmAnOutboxCircuitBreaker>(
    new InMemoryOutboxCircuitBreaker(new OutboxCircuitBreakerOptions
    {
        CooldownCount = 30  // Sit out 30 sweeps, retry on the 31st
    })
);
```

### Without Circuit Breaking

If you don't register an `IAmAnOutboxCircuitBreaker`, the sweeper will continue to attempt publishing to all topics even after failures:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.Hosting;

// No circuit breaker registered - all topics always attempted
services.AddBrighter(/* configuration */)
    .UseOutboxSweeper();  // Sweeper without circuit breaking
```

## Sweeper Circuit Breaking Advanced Scenarios

### Custom Circuit Breaker Implementation

Implement `IAmAnOutboxCircuitBreaker` for custom behavior. This one cools a topic down after a period of time rather than a number of sweeps, and counts how often each topic has tripped:

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;

public class CustomOutboxCircuitBreaker : IAmAnOutboxCircuitBreaker
{
    private static readonly TimeSpan CooldownPeriod = TimeSpan.FromMinutes(10);

    // A failed dispatch can trip a topic while a sweep is cooling topics down, so the map must be concurrent
    private readonly ConcurrentDictionary<RoutingKey, CircuitBreakerState> _topics = new();

    public void TripTopic(RoutingKey topic)
    {
        _topics.AddOrUpdate(
            topic,
            _ => new CircuitBreakerState(DateTime.UtcNow, FailureCount: 1),
            (_, state) => new CircuitBreakerState(DateTime.UtcNow, state.FailureCount + 1));

        // Custom logic: Log, emit metrics, send alerts, etc.
    }

    public void CoolDown()
    {
        var now = DateTime.UtcNow;

        foreach (var (topic, state) in _topics)
        {
            // Remove only the state we read, so a topic tripped again meanwhile stays tripped
            if (now - state.TrippedAt > CooldownPeriod
                && _topics.TryRemove(new KeyValuePair<RoutingKey, CircuitBreakerState>(topic, state)))
            {
                // Custom logic: Log recovery, emit metrics, etc.
            }
        }
    }

    public IEnumerable<RoutingKey> TrippedTopics => _topics.Keys;

    private sealed record CircuitBreakerState(DateTime TrippedAt, int FailureCount);
}
```

### Distributed Circuit Breaker

For multi-instance deployments, keep the tripped topics in shared storage, so a topic one instance trips is skipped by all of them. The store has to be able to list what is tripped, because the sweeper asks for `TrippedTopics` — a plain key-value cache such as `IDistributedCache` cannot enumerate its keys, so it cannot answer. A Redis sorted set can: each member is a topic, and its score is the time the trip expires.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;
using StackExchange.Redis;

public class RedisOutboxCircuitBreaker(IConnectionMultiplexer redis, TimeSpan cooldown) : IAmAnOutboxCircuitBreaker
{
    private const string TrippedTopicsKey = "brighter:outbox:tripped-topics";
    private readonly IDatabase _database = redis.GetDatabase();

    public void TripTopic(RoutingKey topic)
        => _database.SortedSetAdd(TrippedTopicsKey, topic.Value, Now() + cooldown.TotalMilliseconds);

    // A trip expires by time, so cooling down only removes the trips that have expired
    public void CoolDown()
        => _database.SortedSetRemoveRangeByScore(TrippedTopicsKey, double.NegativeInfinity, Now());

    public IEnumerable<RoutingKey> TrippedTopics
        => _database.SortedSetRangeByScore(TrippedTopicsKey, Now(), double.PositiveInfinity)
            .Select(topic => new RoutingKey(topic.ToString()));

    private static double Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
```

Because a trip expires by time rather than by counting `CoolDown` calls, the breaker behaves the same however many instances call `CoolDown`. Register it as a singleton, as [the basic setup](#basic-setup-with-outbox-sweeper) registers the in-memory breaker.

## Further Reading

- [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md) - Configuration, monitoring and troubleshooting
- [Outbox Support](/contents/BrighterOutboxSupport.md) - The Outbox and the Sweeper
- [Distributed Lock](/contents/DistributedLock.md) - Keeping a single Sweeper active
