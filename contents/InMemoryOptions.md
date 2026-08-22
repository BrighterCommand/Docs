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
public class IntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly InMemoryMessageProducer _inMemoryProducer;
    private readonly _internalBus  = new InternalBus();

    public IntegrationTests()
    {
        var services = new ServiceCollection();
        var internalBus  = new InternalBus();

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AddProducers(options =>
        {
            var publication = new Publication() { Topic = new RoutingKey("PersonCreated") };

            options.ProducerRegistry = new InMemoryProducerRegistryFactory(_internalBus , new[] { publication }, InstrumentationOptions.All)
                .Create();
            options.Outbox = new InMemoryOutbox();
        })
        .AddConsumers(options =>
        {
            // InMemory Inbox for deduplication
            options.Inbox = new InboxConfiguration(
                new InMemoryInbox(TimeProvider.System),
                InboxConfiguration.NoActionOnExists
            );

            options.Subscriptions = new Subscription[]
            {
                new InMemorySubscription<PersonCreated>(
                    new SubscriptionName("PersonAnalytics"),
                    new ChannelName("person.created"),
                    new RoutingKey("PersonCreated")
                )
            };

            options.ChannelFactory = new InMemoryChannelFactory(_internalBus, TimeProvider.System);
        })
        .UseScheduler(new InMemorySchedulerFactory())  // InMemory Scheduler
        .UseInMemoryArchiveProvider()  // InMemory Archive
        .AutoFromAssemblies();

        _serviceProvider = services.BuildServiceProvider();
        _commandProcessor = _serviceProvider.GetRequiredService<IAmACommandProcessor>();
    }

    [Fact]
    public async Task Should_Publish_And_Consume_Message_With_InMemory_Components()
    {
        // Arrange
        var command = new CreatePersonCommand { Name = "Alice", Email = "alice@example.com" };

        // Act - Publish with InMemory Outbox
        await _commandProcessor.SendAsync(command);
        await _commandProcessor.ClearOutboxAsync();

        // Wait for InMemory consumer to process
        await Task.Delay(100);

         var messages = _internalBus.Stream(new RoutingKey("PersonCreated"));
         Assert.Any(messages);
    }

    [Fact]
    public async Task Should_Schedule_Message_With_InMemory_Scheduler()
    {
        // Arrange
        var command = new SendEmailCommand { To = "alice@example.com" };

        // Act - Schedule with InMemory Scheduler
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromMilliseconds(100),
            command
        );

        // Assert - Wait for execution
        await Task.Delay(150);

        var messages = _internalBus.Stream(new RoutingKey("PersonCreated"));
         Assert.Any(messages);
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
public static class BrighterConfiguration
{
    public static IServiceCollection AddBrighterWithEnvironmentConfig(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var internalBus = new InternalBus();

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AddProducers(options =>
        {
            options.ProducerRegistry = GetProducerRegistry(environment, configuration, internalBus);
        })
        .UseOutbox(GetOutbox(environment, configuration))
        .UseScheduler(GetSchedulerFactory(environment, configuration))
        .AddConsumers(options =>
        {
            options.Inbox = GetInbox(environment, configuration);
            options.Subscriptions = GetSubscriptions();
            options.ChannelFactory = GetChannelFactory(environment, configuration, internalBus);
        })
        .AutoFromAssemblies();

        return services;
    }

    private static IAmAProducerRegistry GetProducerRegistry(
        IHostEnvironment environment,
        IConfiguration configuration,
        IAmABus bus)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InMemoryProducerRegistryFactory(bus , new[] { publication }, InstrumentationOptions.All)
                .Create();
        }

        // Production: RabbitMQ, Kafka, AWS SQS, etc.
        return new RmqProducerRegistryFactory(/* production config */).Create();
    }

    private static IMessageSchedulerFactory GetSchedulerFactory(
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InMemorySchedulerFactory();
        }

        // Production: Quartz, Hangfire, AWS Scheduler, etc.
        return new HangfireMessageSchedulerFactory(
            configuration.GetConnectionString("Hangfire")
        );
    }

    private static IAmAnOutbox<Message, CommittableTransaction> GetOutbox(
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
                InboxConfiguration.NoActionOnExists
            );
        }

        // Production: SQL Server, PostgreSQL, MySQL, DynamoDB, etc.
        return new InboxConfiguration(
            new MsSqlInbox(/* production config */),
            InboxConfiguration.NoActionOnExists
        );
    }

    private static IAmAChannelFactory GetChannelFactory(
        IHostEnvironment environment,
        IConfiguration configuration,
        IAmABus bus)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return new InMemoryChannelFactory(bus);
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
