---
description: "MQTT is a lightweight publish-subscribe protocol for constrained networks, and Brighter configures it with a gateway configuration for each side of the wire and a subscription per consumer."
layout:
  description:
    visible: false
---

# MQTT Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

MQTT is a lightweight publish-subscribe protocol for constrained networks, and Brighter configures it with a gateway configuration for each side of the wire and a subscription per consumer.

## MQTT General

Install the transport package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.MQTT
```

Brighter speaks MQTT v3.1.1 through MQTTnet, publishing at **at-least-once** quality of
service. Four things shape how you configure it:

- **The topic prefix is required on the consumer.** `MqttMessageConsumer` subscribes to
  `{TopicPrefix}/#` and throws when `TopicPrefix` is null, so a consumer reads every topic
  beneath the prefix and Brighter routes by the message header from there. The publisher
  prefixes the topic in the same way, and leaves it alone when `TopicPrefix` is null.
- **Producers and consumers take different configuration types.**
  `MqttMessagingGatewayProducerConfiguration` and `MqttMessagingGatewayConsumerConfiguration`
  both derive from `MqttMessagingGatewayConfiguration` and add nothing to it; they exist so the
  two sides can be registered separately.
- **The package ships no producer registry factory**, so the registry is assembled from
  `MqttMessagePublisher` and `MqttMessageProducer` directly, as in the example below.
- **MQTT ships no publication type.** A producer takes the base
  [`Publication`](/contents/CommandProcessorConfigurationReference.md#publication-options),
  which is why there is no MQTT publication table here.

## MQTT Configuration Options

`MqttMessagingGatewayConfiguration` takes its options as properties, and the producer and
consumer configurations inherit all of them.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.MQTT.MqttMessagingGatewayConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `ClientID` | `string?` | `null` | The client identifier the connection registers with the broker. |
| `Username` | `string?` | `null` | The username the client authenticates with. |
| `Password` | `string?` | `null` | The password the client authenticates with. |
| `CleanSession` | `bool` | `false` | Whether the broker discards session state when the client connects. |
| `Hostname` | `string?` | `null` | The host name of the MQTT broker. |
| `Port` | `int` | `1883` | The TCP port the broker listens on. |
| `TopicPrefix` | `object?` | `null` | The prefix joined to the message topic when publishing, and the root of the `#` subscription when consuming. |

One further setting is declared on this type and is **not** on the table:
`ConnectionAttempts` has an `internal` setter, so it cannot be set from your code. It is `1`,
and it is the number of times the publisher and the consumer try to connect before giving up.

## MQTT Subscription

`MqttSubscription` takes its options as constructor arguments, so the option is the parameter
you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; the other two are Brighter's dead letter and invalid message routing keys, which
this transport supports itself.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.MQTT.MqttSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the channel this subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The topic messages on this channel carry in their header. |
| `requestType` | `Type?` | `none` | The request type messages on this channel are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the channel. |
| `bufferSize` | `int` | `1` | Messages taken from the client at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this channel, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `1000 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; supply a `ChannelFactory` over a `MqttMessageConsumerFactory`. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key messages are dead-lettered to. |
| `invalidMessageRoutingKey` | `RoutingKey?` | `null` | The routing key unacceptable messages are routed to. |

The generic form `MqttSubscription<T>` supplies `requestType` from `T` and defaults
`subscriptionName`, `channelName` and `routingKey` to `T`'s full name. It also defaults
`messagePumpType` to `Proactor`, where the non-generic form requires it.

## MQTT Configuration Example

```csharp
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.MQTT;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var producerConfiguration = new MqttMessagingGatewayProducerConfiguration
{
    Hostname = "localhost",
    Port = 1883,
    ClientID = "greetings-sender",
    TopicPrefix = "brighter"
};

var consumerConfiguration = new MqttMessagingGatewayConsumerConfiguration
{
    Hostname = "localhost",
    Port = 1883,
    ClientID = "greetings-receiver",
    TopicPrefix = "brighter"
};

var routingKey = new RoutingKey("greeting.event");

builder.Services.AddBrighter()
    .AddProducers(configure =>
    {
        var publisher = new MqttMessagePublisher(producerConfiguration);
        var producer = new MqttMessageProducer(
            publisher,
            new Publication { Topic = routingKey, RequestType = typeof(GreetingEvent) });

        configure.ProducerRegistry = new ProducerRegistry(
            new Dictionary<ProducerKey, IAmAMessageProducer>
            {
                [new ProducerKey(routingKey)] = producer
            });
    })
    .AutoFromAssemblies();

builder.Services.AddConsumers(options =>
{
    options.Subscriptions =
    [
        new MqttSubscription<GreetingEvent>(
            new SubscriptionName("paramore.example.greeting"),
            new ChannelName("greeting.event"),
            routingKey)
    ];
    options.DefaultChannelFactory = new ChannelFactory(
        new MqttMessageConsumerFactory(consumerConfiguration));
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
