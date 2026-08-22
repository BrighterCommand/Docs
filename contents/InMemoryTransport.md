---
description: "The in-process transport: when to use it, how to configure it, a complete example, and its limits."
layout:
  description:
    visible: false
---

# InMemory Transport

> **Reference** · Applies to **Brighter V10** · Prerequisites: [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)

The in-process transport: when to use it, how to configure it, a complete example, and its limits. It is part of Brighter's [InMemory options for development and testing](/contents/InMemoryOptions.md).


The InMemory Transport provides lightweight message publishing and consumption without requiring a message broker like RabbitMQ, Kafka, or AWS SQS. It consists of three replacements:

- **InternalBus** An in memory collection of topics, and queues of messages to those topics. It implements `IAmABus` and can be used from the `InMemoryMessageProducer` and `InMemoryMessageConsumer` to exchange a message.
- **InMemoryMessageProducer** An implementation of `IAmAMessageProducerSync`, `IAmAMessageProducerAsync` and `IAmABulkMessageProducerAsync` that produces message to topics on the `InternalBus`.
- **InMemoryMessageConsumer** An implementation of `IAmAMessageConsumerSync` and `IAmAMessageConsumerAsync` that consumes messages from topics on the `InternalBus`.

## When to Use the InMemory Transport

**Perfect for**:

- Unit testing command and event handlers
- Integration testing without external dependencies
- Local development and debugging
- Demos and proof-of-concepts

**Production Use Cases** (limited):

- Single-process applications with no distribution requirements
- Internal message passing within a monolith
- Scenarios where message loss is acceptable

## InMemory Transport Configuration

**Internal Bus**:

```csharp
// ...
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
// ...

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

## InMemory Transport Complete Example

```csharp
// ...
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

## InMemory Transport Limitations

- **No persistence**: Messages are lost if the process crashes
- **Single process**: Cannot distribute across multiple instances
- **No backpressure**: Unlimited queue growth (memory bound)
- **No dead letter queues**: Failed messages are discarded
- **No message TTL**: Messages never expire

## Further Reading

- [InMemory Options for Development and Testing](/contents/InMemoryOptions.md) - The full set, and testing patterns
