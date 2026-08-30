---
description: "Apache RocketMQ is a distributed messaging platform, and Brighter configures it with a gateway connection wrapping the RocketMQ client, a publication per topic, and a subscription per consumer group."
layout:
  description:
    visible: false
---

# RocketMQ Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Apache RocketMQ is a distributed messaging platform, and Brighter configures it with a gateway connection wrapping the RocketMQ client, a publication per topic, and a subscription per consumer group.

## RocketMQ General

Install the transport package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.RocketMQ
```

Brighter publishes to a RocketMQ **topic** and consumes with a `SimpleConsumer` in a **consumer
group**, filtering by tag. Three things shape how you configure it:

- **Brighter cannot create a RocketMQ topic.** The RocketMQ C# client has no administrative
  API, so a publication with `MakeChannels` set to `Create` logs a warning and carries on.
  Provision topics and consumer groups with the RocketMQ tooling, and set `makeChannels` to
  `OnMissingChannel.Assume`.
- **The consumer group is a subscription option, and it has no usable default.** It defaults to
  an empty string, which RocketMQ rejects, so set `consumerGroup` on every subscription.
- **The endpoint lives on the RocketMQ client's own configuration**, not on a Brighter option.
  `RocketMessagingGatewayConnection` takes an `Org.Apache.Rocketmq.ClientConfig` as its
  constructor argument and exposes it as the get-only `ClientConfig` property, so the endpoint,
  TLS and request timeout are set with `ClientConfig.Builder()` and are not in the table below.

The consumer requires a `RocketSubscription`; a base `Subscription` raises a
`ConfigurationException` when the channel is created.

## RocketMQ Connection

`RocketMessagingGatewayConnection` takes its options as properties, alongside the
`ClientConfig` it is constructed with.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RocketMQ.RocketMessagingGatewayConnection
     manual: TimerProvider — initialised to TimeProvider.System, which has no printable value
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `TimerProvider` | `TimeProvider` | `TimeProvider.System` | The time source used for delayed and time-dependent operations. |
| `MaxAttempts` | `int` | `3` | Attempts the producer makes to deliver a message. |
| `Checker` | `ITransactionChecker?` | `null` | The callback RocketMQ uses to resolve the state of a half-committed transactional message. |
| `Instrumentation` | `InstrumentationOptions` | `All` | The telemetry detail producers and consumers emit. |

The property is spelled `TimerProvider`, with an `r`, where the rest of Brighter spells it
`TimeProvider`.

## RocketMQ Publication

`RocketMqPublication` takes its options as properties and adds these three to the
[base publication options](/contents/CommandProcessorConfigurationReference.md#publication-options),
which it inherits.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RocketMQ.RocketMqPublication -->

| Option | Type | Default | Description |
|---|---|---|---|
| `Tag` | `string?` | `null` | The tag messages are published with, which consumers filter on. |
| `Instrumentation` | `InstrumentationOptions?` | `null` | The telemetry detail this publication emits; null uses the connection's setting. |
| `TopicType` | `TopicType` | `Normal` | Selects a normal, delay or FIFO topic. |

## RocketMQ Subscription

The non-generic subscription type is `RocketSubscription`, and it takes its options as
constructor arguments, so the option is the parameter you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; the other six are RocketMQ's own, plus Brighter's dead letter and invalid
message routing keys.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RocketMQ.RocketSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
     manual: filter — initialised to a FilterExpression of *, which has no printable value
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the channel this subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The RocketMQ topic the consumer subscribes to. |
| `requestType` | `Type?` | `none` | The request type messages on this topic are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the topic. |
| `consumerGroup` | `string?` | `""` | The RocketMQ consumer group this consumer joins. |
| `bufferSize` | `int` | `1` | Messages received at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this topic, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; supply a `RocketMqChannelFactory` over a `RocketMessageConsumerFactory`. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `filter` | `FilterExpression?` | every tag | The tag expression the consumer subscribes with. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `receiveMessageTimeout` | `TimeSpan?` | `60000 ms` | How long a receive call waits at the broker before returning empty. |
| `invisibilityTimeout` | `TimeSpan?` | `30000 ms` | How long a received message is hidden from other consumers in the group. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key messages are dead-lettered to. |
| `invalidMessageRoutingKey` | `RoutingKey?` | `null` | The routing key unacceptable messages are routed to. |

The generic form is spelled differently from the non-generic one: `RocketMqSubscription<T>`
derives from `RocketSubscription`. It supplies `requestType` from `T` and takes the same options
otherwise. It still requires a subscription name, a channel name and a routing key, and it
leaves `messagePumpType` required, so state Reactor or Proactor on every subscription.

## RocketMQ Configuration Example

RocketMQ ships a message producer factory rather than a producer registry factory, so the
registry is built from the factory's dictionary.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.Apache.Rocketmq;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RocketMQ;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connection = new RocketMessagingGatewayConnection(
    new ClientConfig.Builder()
        .SetEndpoints("localhost:8081")
        .EnableSsl(false)
        .SetRequestTimeout(TimeSpan.FromSeconds(10))
        .Build());

var publications = new[]
{
    new RocketMqPublication
    {
        Topic = new RoutingKey("greeting.event"),
        RequestType = typeof(GreetingEvent),
        TopicType = TopicType.Normal,
        MakeChannels = OnMissingChannel.Assume
    }
};

builder.Services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new ProducerRegistry(
            new RocketMessageProducerFactory(connection, publications).Create());
    })
    .AutoFromAssemblies();

builder.Services.AddConsumers(options =>
{
    options.Subscriptions =
    [
        new RocketMqSubscription<GreetingEvent>(
            new SubscriptionName("paramore.example.greeting"),
            new ChannelName("greeting.event"),
            new RoutingKey("greeting.event"),
            consumerGroup: "greetings",
            messagePumpType: MessagePumpType.Proactor,
            makeChannels: OnMissingChannel.Assume)
    ];
    options.DefaultChannelFactory = new RocketMqChannelFactory(
        new RocketMessageConsumerFactory(connection));
});

builder.Services.AddHostedService<ServiceActivatorHostedService>();

var host = builder.Build();
await host.RunAsync();
```

## Further Reading

- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — registering Brighter
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md) — the subscription options every transport shares
- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md) — the publication options every transport shares
- [Reactor and Proactor](/contents/ReactorAndProactor.md) — choosing a message pump
- [Error Handling Options](/contents/ErrorHandlingOptions.md) — what Brighter does with a message it cannot process
