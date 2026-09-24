---
description: "Brighter V10 provides a comprehensive suite of in-memory implementations for key components, making it easy to develop and test applications without external dependencies."
layout:
  description:
    visible: false
---

# InMemory Options for Development and Testing

> **How-to** · Applies to **Brighter V10**

## InMemory Options Overview

Brighter V10 provides a comprehensive suite of in-memory implementations for key components, making it easy to develop and test applications without external dependencies. These in-memory options replace databases, message brokers, and schedulers with simple, lightweight alternatives that run entirely in process.

**Key Benefits**:

- **Zero dependencies**: No databases, message brokers, or external services required
- **Fast execution**: Perfect for unit and integration tests
- **Simple configuration**: Minimal setup, get started immediately
- **Consistent APIs**: Same interfaces as production components
- **Deterministic behavior**: Predictable, repeatable test execution

**Important**: InMemory options are designed for **development and testing**. While robust, they are generally **not recommended for production** due to lack of persistence, distribution, and durability guarantees.

## Available InMemory Components

Brighter V10 provides InMemory implementations for the following components:

| Component | Purpose | Production Ready? |
|-----------|---------|-------------------|
| [InMemory Transport](/contents/InMemoryTransport.md) | Message publishing and consumption | Limited use cases |
| [InMemory Outbox](/contents/InMemoryOutbox.md) | Transactional messaging | Limited use cases |
| [InMemory Inbox](/contents/InMemoryInbox.md) | Message deduplication | Limited use cases |
| [InMemory Scheduler](/contents/InMemoryScheduler.md) | Delayed message scheduling | Limited use cases |
| [InMemory Archive](/contents/OutboxArchiver.md#inmemory-archive) | Message archiving | No |
| [InMemory Storage Provider](/contents/ClaimCheck.md) | Claim Check pattern | No |

## Test Configuration Patterns

When writing tests, you can use Brighter's `Func<IServiceProvider, T>` overloads and the Microsoft Options pattern to create isolated test configurations. This enables parallel test execution without serialization.

**Using PostConfigure for Test Overrides**

```csharp
public class MyTests
{
    private ServiceProvider BuildTestServiceProvider()
    {
        var services = new ServiceCollection();
        var internalBus = new InternalBus();

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AddProducers(options =>
        {
            options.ProducerRegistry = new InMemoryProducerRegistryFactory(
                internalBus,
                new[] { new Publication { Topic = new RoutingKey("TestTopic") } },
                InstrumentationOptions.All
            ).Create();
            options.Outbox = new InMemoryOutbox();
        })
        .AutoFromAssemblies();

        // Override specific options for this test
        services.PostConfigure<BrighterOptions>(options =>
        {
            options.RequestContextFactory = new TestRequestContextFactory();
        });

        return services.BuildServiceProvider();
    }
}
```

For more details on service provider overloads and the Options pattern, see [Service Provider Function Overloads](/contents/CommandProcessorConfigurationReference.md#service-provider-function-overloads) and [Using the Options Pattern](/contents/CommandProcessorConfigurationReference.md#using-the-options-pattern) in the Basic Configuration documentation.

## Complete Testing Example

Here's a complete example showing how to use multiple InMemory components together:

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Xunit;

public class IntegrationTests : IDisposable
{
    private readonly InternalBus _internalBus = new();
    private readonly ServiceProvider _serviceProvider;
    private readonly IAmACommandProcessor _commandProcessor;

    public IntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddConsumers(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;

            // InMemory Inbox for deduplication
            options.InboxConfiguration = new InboxConfiguration(
                new InMemoryInbox(TimeProvider.System),
                actionOnExists: OnceOnlyAction.Warn
            );

            options.Subscriptions = new Subscription[]
            {
                new InMemorySubscription<PersonCreated>(
                    new SubscriptionName("PersonAnalytics"),
                    new ChannelName("person.created"),
                    new RoutingKey("PersonCreated"),
                    messagePumpType: MessagePumpType.Proactor
                )
            };

            options.DefaultChannelFactory = new InMemoryChannelFactory(_internalBus, TimeProvider.System);
        })
        .AddProducers(options =>
        {
            // RequestType is how Brighter finds the publication for a PersonCreated
            var publication = new Publication
            {
                Topic = new RoutingKey("PersonCreated"),
                RequestType = typeof(PersonCreated)
            };

            options.ProducerRegistry = new InMemoryProducerRegistryFactory(_internalBus, new[] { publication }, InstrumentationOptions.All)
                .Create();
            options.Outbox = new InMemoryOutbox(TimeProvider.System);
        })
        .UseScheduler(new InMemorySchedulerFactory());  // InMemory Scheduler
        // This test registers no handlers of its own, so it has no need of AutoFromAssemblies

        _serviceProvider = services.BuildServiceProvider();
        _commandProcessor = _serviceProvider.GetRequiredService<IAmACommandProcessor>();
    }

    [Fact]
    public async Task Should_Publish_Message_With_InMemory_Components()
    {
        // Act - Post writes to the InMemory Outbox, then dispatches to the InMemory transport
        await _commandProcessor.PostAsync(new PersonCreated { Name = "Alice" });

        // Assert - the message is on the InMemory bus
        var messages = _internalBus.Stream(new RoutingKey("PersonCreated"));
        Assert.NotEmpty(messages);
    }

    [Fact]
    public async Task Should_Schedule_Message_With_InMemory_Scheduler()
    {
        // Act - schedule the post with the InMemory Scheduler
        await _commandProcessor.PostAsync(
            TimeSpan.FromMilliseconds(100),
            new PersonCreated { Name = "Bob" }
        );

        // Assert - nothing is sent until the delay has passed
        Assert.Empty(_internalBus.Stream(new RoutingKey("PersonCreated")));

        await Task.Delay(500);

        var messages = _internalBus.Stream(new RoutingKey("PersonCreated"));
        Assert.NotEmpty(messages);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
```

## Environment-Specific Configuration

Use InMemory components for development/testing, production components elsewhere:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.MessageScheduler.Hangfire;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public static class BrighterConfiguration
{
    public static IServiceCollection AddBrighterWithEnvironmentConfig(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var internalBus = new InternalBus();

        services.AddConsumers(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
            options.InboxConfiguration = GetInbox(environment, configuration);
            options.Subscriptions = GetSubscriptions();
            options.DefaultChannelFactory = GetChannelFactory(environment, configuration, internalBus);
        })
        .AddProducers(options =>
        {
            options.ProducerRegistry = GetProducerRegistry(environment, configuration, internalBus);
            options.Outbox = GetOutbox(environment, configuration);
        })
        .UseEnvironmentScheduler(environment)
        .AutoFromAssemblies();

        return services;
    }

    private static IAmAProducerRegistry GetProducerRegistry(
        IHostEnvironment environment,
        IConfiguration configuration,
        InternalBus bus)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            var publication = new Publication
            {
                Topic = new RoutingKey("PersonCreated"),
                RequestType = typeof(PersonCreated)
            };

            return new InMemoryProducerRegistryFactory(bus , new[] { publication }, InstrumentationOptions.All)
                .Create();
        }

        // Production: RabbitMQ, Kafka, AWS SQS, etc.
        return new RmqProducerRegistryFactory(/* production config */).Create();
    }

    // UseScheduler needs one type that is both a message and a request scheduler factory,
    // so this chooses the concrete factory itself rather than returning one of the two interfaces
    private static IBrighterBuilder UseEnvironmentScheduler(
        this IBrighterBuilder brighter,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return brighter.UseScheduler(new InMemorySchedulerFactory());
        }

        // Production: Quartz, Hangfire, AWS Scheduler, etc.; Hangfire's storage is configured with AddHangfire
        return brighter.UseScheduler(new HangfireMessageSchedulerFactory());
    }

    private static IAmAnOutbox GetOutbox(
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InMemoryOutbox(TimeProvider.System);
        }

        // Production: SQL Server, PostgreSQL, MySQL, DynamoDB, etc.
        return new MsSqlOutbox(/* production config */);
    }

    private static InboxConfiguration GetInbox(
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InboxConfiguration(
                new InMemoryInbox(TimeProvider.System),
                actionOnExists: OnceOnlyAction.Warn
            );
        }

        // Production: SQL Server, PostgreSQL, MySQL, DynamoDB, etc.
        return new InboxConfiguration(
            new MsSqlInbox(/* production config */),
            actionOnExists: OnceOnlyAction.Warn
        );
    }

    private static IAmAChannelFactory GetChannelFactory(
        IHostEnvironment environment,
        IConfiguration configuration,
        InternalBus bus)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InMemoryChannelFactory(bus, TimeProvider.System);
        }

        // Production: RabbitMQ, Kafka, AWS SQS, etc.
        return new ChannelFactory(new RmqMessageConsumerFactory(/* config */));
    }
}
```


## Comparison with Production Components

| Feature | InMemory | Production (DB/Broker) |
|---------|----------|------------------------|
| **Persistence** | None | Database/Disk |
| **Distribution** | Single process | Multi-instance |
| **Durability** | None | ACID guarantees |
| **Performance** | Very fast | Network/IO bound |
| **Setup** | Zero config | Requires infrastructure |
| **Testing** | Ideal | Complex setup |
| **Production** | Limited | Recommended |

## Migration to Production

When moving to production, replace InMemory components:

| InMemory Component | Production Alternative |
|-------------------|------------------------|
| InMemory Transport | RabbitMQ, Kafka, AWS SQS, Azure Service Bus |
| InMemory Outbox | MS SQL, PostgreSQL, MySQL, DynamoDB, MongoDB |
| InMemory Inbox | MS SQL, PostgreSQL, MySQL, DynamoDB, MongoDB |
| InMemory Scheduler | Quartz, Hangfire, AWS Scheduler, Azure Service Bus Scheduler |
| InMemory Archive | Database-backed archive provider |

**No code changes required** - just swap the registration in your DI container!

## InMemory Options Summary

Brighter V10 provides comprehensive InMemory options for all major components:

**Best For**:

- Unit and integration testing
- Local development
- Demos and POCs
- CI/CD pipelines (fast, no external dependencies)

**Not Recommended For**:

- Production systems requiring durability
- Distributed/multi-instance applications
- Long-running scheduled work

Use InMemory options to accelerate development and testing, then migrate to production components for deployed applications with durability and distribution requirements.

## Further Reading

- [InMemory Transport](/contents/InMemoryTransport.md) - The in-process transport
- [InMemory Outbox](/contents/InMemoryOutbox.md) - The in-process Outbox
- [InMemory Inbox](/contents/InMemoryInbox.md) - The in-process Inbox
- [InMemory Scheduler](/contents/InMemoryScheduler.md) - The in-process scheduler
- [Outbox Archiver](/contents/OutboxArchiver.md#inmemory-archive) - The in-process archive provider
