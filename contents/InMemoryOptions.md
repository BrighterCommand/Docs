# InMemory Options for Development and Testing

> **Reference** · Applies to **Brighter V10**

## Overview

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
| [InMemory Transport](#inmemory-transport) | Message publishing and consumption | Limited use cases |
| [InMemory Outbox](#inmemory-outbox) | Transactional messaging | Limited use cases |
| [InMemory Inbox](#inmemory-inbox) | Message deduplication | Limited use cases |
| [InMemory Scheduler](#inmemory-scheduler) | Delayed message scheduling | Limited use cases |
| [InMemory Archive](#inmemory-archive) | Message archiving | No |
| [InMemory Storage Provider](/contents/ClaimCheck.md) | Claim Check pattern | No |

## InMemory Transport

The InMemory Transport provides lightweight message publishing and consumption without requiring a message broker like RabbitMQ, Kafka, or AWS SQS. It consists of three replacements:

- **InternalBus** An in memory collection of topics, and queues of messages to those topics. It implements `IAmABus` and can be used from the `InMemoryMessageProducer` and `InMemoryMessageConsumer` to exchange a message.
- **InMemoryMessageProducer** An implementation of `IAmAMessageProducerSync`, `IAmAMessageProducerAsync` and `IAmABulkMessageProducerAsync` that produces message to topics on the `InternalBus`.
- **InMemoryMessageConsumer** An implementation of `IAmAMessageConsumerSync` and `IAmAMessageConsumerAsync` that consumes messages from topics on the `InternalBus`.

### When to Use

**Perfect for**:

- Unit testing command and event handlers
- Integration testing without external dependencies
- Local development and debugging
- Demos and proof-of-concepts

**Production Use Cases** (limited):

- Single-process applications with no distribution requirements
- Internal message passing within a monolith
- Scenarios where message loss is acceptable

### Configuration

**Internal Bus**:

```csharp
var internalBus = new InternalBus();
```

**Producer Configuration**:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var internalBus = new InternalBus();

services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(options =>
{
    var publication = new Publication() { Topic = new RoutingKey("Topic") };

    options.ProducerRegistry = new InMemoryProducerRegistryFactory(internalBus , new[] { publication }, InstrumentationOptions.All)
        .Create();
})
.AutoFromAssemblies();
```

**Consumer Configuration**:

```csharp

var internalBus = new InternalBus();

services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddConsumers(options =>
{
    options.Subscriptions = subscriptions;
    options.ChannelFactory = new InMemoryChannelFactory(internalBus, TimeProvider.System);
})
.AutoFromAssemblies()
.AddHostedService<ServiceActivatorHostedService>();
```

### Complete Example

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var internalBus = new InternalBus();

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AddProducers(options =>
        {
               var publication = new Publication() { Topic = new RoutingKey("GreetingMade") };

                options.ProducerRegistry = new InMemoryProducerRegistryFactory(internalBus , new[] { publication }, InstrumentationOptions.All)
                    .Create();
        })
        .AddConsumers(options =>
        {
            options.Subscriptions = new Subscription[]
            {
                new Subscription<GreetingMade>(
                    new SubscriptionName("GreetingAnalytics"),
                    new ChannelName("greeting.event"),
                    new RoutingKey("GreetingMade")
                )
            };
            options.ChannelFactory = new InMemoryChannelFactory(internalBus, TimeProvider.System);
        })
        .AutoFromAssemblies()
        .AddHostedService<ServiceActivatorHostedService>();
    }
}
```

### Limitations

- **No persistence**: Messages are lost if the process crashes
- **Single process**: Cannot distribute across multiple instances
- **No backpressure**: Unlimited queue growth (memory bound)
- **No dead letter queues**: Failed messages are discarded
- **No message TTL**: Messages never expire

## InMemory Outbox

The InMemory Outbox provides transactional messaging support without requiring a database. Note that if you do not specify a persistent Outbox, we will use the InMemoryOutbox, by default. Any use of the `CommandProcessor`'s `Post` method uses the default `InMemoryOutbox` and not the persistent Outbox, as it does not take a transaction provider as an argument.

### Flush of Expired Messages

The InMemory Outbox will flush expired messages. You can configure the time limit for a message, after which it will be flushed:

- **EntryTimeToLive** Defaults to 5 minutes. Governs how long a message can remain in the Outbox.
- **ExpirationScanInterval** Defaults to 10 mins. Governs how often a scan for expired messages runs.

### Compaction of the InMemoryOutbox

The InMemoryOutbox's capacity is constrained. You can configure the limit to the number of messages the Outbox contains. If you are using the InMemoryOutbox in production scenarios, you should pay attention to this limit. Once the limit is hit, the Outbox will compact, removing older messages first. You can set a compaction percentage, which governs how many messages will be purged from the InMemoryOutbox when we compact.

- **EntryLimit** Defaults to 2048. Governs how many messages the InMemoryOutbox can hold.
- **CompactionPercentage** When we hit a capacity limit, what percentage of messages should we purge. 

### When to Use

**Perfect for**:

- Testing transactional messaging patterns
- Unit testing the Outbox pattern
- Development without database dependencies

**Production Use Cases** (limited):

- Single-process applications
- Non-critical message publishing - the InMemoryOutbox is used in place of a persistent Outbox
- Scenarios where message loss on restart is acceptable

### Configuration

```csharp
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(options =>
{
    options.ProducerRegistry = /* your producer registry */;
    options.Outbox = new InMemoryOutbox();
})
.UseOutboxSweeper();  // Enable sweeper for reliability
```

### Example of Post

```csharp
public class CreatePersonHandler : RequestHandlerAsync<CreatePerson>
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IAmAnOutboxAsync<Message, CommittableTransaction> _outbox;
    private readonly PersonRepository _repository;

    public override async Task<CreatePerson> HandleAsync(
        CreatePerson command,
        CancellationToken cancellationToken = default)
    {
        // Start an in-memory transaction (no real transaction support)
        var person = new Person(command.Name, command.Email);
        await _repository.SaveAsync(person);

        // Deposit message to outbox (held in memory)
        await _commandProcessor.Post(new PersonCreated { PersonId = person.Id }, cancellationToken: cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

### Limitations

- **No persistence**: Messages lost on application restart
- **No transactions**: Cannot participate in database transactions
- **Single process**: State not shared across instances
- **Memory bound**: All outstanding messages held in memory

## InMemory Inbox

The InMemory Inbox provides message deduplication without requiring a database.

### When to Use

**Perfect for**:

- Unit testing duplicate message handling
- Development without database dependencies

**Production Use Cases** (limited):

- Single-process applications
- Short-lived message deduplication windows
- Non-critical deduplication scenarios

### Configuration

```csharp
var bus = new InternalBus();

services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddConsumers(options =>
{
    options.Inbox = new InboxConfiguration(
        new InMemoryInbox(TimeProvider.System),
        InboxConfiguration.NoActionOnExists
    );
    options.Subscriptions = subscriptions;
    options.ChannelFactory = new InMemoryChannelFactory(bus);
})
.AutoFromAssemblies();
```

### Example Usage

```csharp
[UseInboxAsync(step: 0, contextKey: typeof(PersonCreatedHandler), onceOnly: true)]
public class PersonCreatedHandler : RequestHandlerAsync<PersonCreated>
{
    private readonly PersonRepository _repository;

    [UseInboxAsync(0, typeof(PersonCreatedHandler), true)]
    public override async Task<PersonCreated> HandleAsync(
        PersonCreated @event,
        CancellationToken cancellationToken = default)
    {
        // Inbox ensures this handler processes each message only once
        var person = await _repository.GetByIdAsync(@event.PersonId);
        person.MarkAsCreated();
        await _repository.SaveAsync(person);

        return await base.HandleAsync(@event, cancellationToken);
    }
}
```

### Limitations

- **No persistence**: Deduplication state lost on restart
- **Single process**: Cannot deduplicate across instances
- **Memory bound**: All seen message IDs held in memory
- **No cleanup**: Old entries remain until process restart

## InMemory Scheduler

The InMemory Scheduler provides delayed message execution without requiring Quartz, Hangfire, or cloud schedulers.

### When to Use

See the complete [InMemory Scheduler](InMemoryScheduler.md) documentation for detailed information.

**Perfect for**:

- Unit and integration tests
- Local development
- Demos and proof-of-concepts

**Production Use Cases** (very limited):

- Non-critical scheduled work
- Short delays (minutes, not hours/days)
- Acceptable to lose scheduled work on restart

### Configuration

```csharp
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseScheduler(new InMemorySchedulerFactory())
.AutoFromAssemblies();
```

### Example Usage

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrder(Order order)
    {
        await _repository.SaveAsync(order);

        // Schedule confirmation email for 5 minutes later
        var schedulerId = await _commandProcessor.SendAsync(
            TimeSpan.FromMinutes(5),
            new SendOrderConfirmationCommand { OrderId = order.Id }
        );
    }
}
```

For complete documentation, see [InMemory Scheduler](InMemoryScheduler.md).

## InMemory Archive

The InMemory Archive stores dispatched messages in memory for diagnostics and replay.

### When to Use

**Perfect for**:

- Testing message archiving
- Development and debugging
- Inspecting sent messages in tests

**Not recommended for production** due to unbounded memory growth.

### Configuration

```csharp
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseOutboxArchiver(new InMemoryArchiveProvider())
.AddProducers(/* producer configuration */);
```

### Example Usage

```csharp
public class LargeMessageMapper : IAmAMessageMapper<LargeDataCommand>
{
    private readonly IAmAStorageProviderAsync _storageProvider;

    [ClaimCheck(0, thresholdInKb: 5)]  // Store payloads > 5KB
    public Message MapToMessage(LargeDataCommand request)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: new RoutingKey("LargeData"),
            messageType: MessageType.MT_COMMAND
        );

        var body = new MessageBody(JsonSerializer.Serialize(request));
        return new Message(header, body);
    }
}
```

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

For more details on service provider overloads and the Options pattern, see [Service Provider Function Overloads](/contents/BrighterBasicConfiguration.md#service-provider-function-overloads) and [Using the Options Pattern](/contents/BrighterBasicConfiguration.md#using-the-options-pattern) in the Basic Configuration documentation.

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

## Summary

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
