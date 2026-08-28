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

## InMemory Subscription Options

`InMemorySubscription` takes seventeen constructor arguments, and every one of them is
[`Subscription`'s](/contents/DispatcherConfigurationReference.md#subscription-options): the
in-memory transport adds no option of its own, because there is no broker to configure. The
table is here so that the claim is checked rather than asserted — the day a parameter is
added, it appears as a row this page does not have.

<!-- optioncheck: Paramore.Brighter.InMemorySubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the channel on the `InternalBus` this subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The topic the channel subscribes to on the `InternalBus`. |
| `requestType` | `Type?` | `none` | The request type messages on this channel are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the channel. |
| `bufferSize` | `int` | `1` | Messages held in the channel at once, and read from the bus at once. |
| `noOfPerformers` | `int` | `1` | Threads reading this channel, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; supply an `InMemoryChannelFactory` over the same `InternalBus`. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |

**Two options are set as properties after construction rather than as constructor
arguments**, so they are not on the table above: `DeadLetterRoutingKey` and
`InvalidMessageRoutingKey`, both `RoutingKey?` and both `null`.

The generic form `InMemorySubscription<T>` supplies `requestType` from `T` and still requires
a subscription name, a channel name and a routing key.

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
