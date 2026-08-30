---
description: "Google Cloud Pub/Sub is a managed publish-subscribe service, and Brighter configures it with a gateway connection, a publication per topic, and a subscription per consumer."
layout:
  description:
    visible: false
---

# GCP Pub/Sub Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Google Cloud Pub/Sub is a managed publish-subscribe service, and Brighter configures it with a gateway connection, a publication per topic, and a subscription per consumer.

## GCP Pub/Sub General

Install the transport package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.GcpPubSub
```

Pub/Sub separates the **topic** a message is published to from the **subscription** a consumer
reads. Brighter maps its `RoutingKey` onto the topic and its `ChannelName` onto the
subscription, so one topic can feed several independent consumers, each with its own
acknowledgement deadline and its own backlog.

Two things are worth knowing before you configure it:

- **The consumer runs in one of two modes.** `SubscriptionMode.Stream`, the default, opens a
  streaming pull with the `SubscriberClient`; `SubscriptionMode.Pull` issues unary pull
  requests instead. The `streamingConfiguration` option reaches the streaming client's builder
  and does nothing in pull mode.
- **Brighter creates the topic, the subscription and their IAM bindings** when `makeChannels`
  is `OnMissingChannel.Create`. That includes granting the project's Pub/Sub service account
  the publisher role on a dead letter topic, so the credential you supply needs permission to
  read and set IAM policy on the topic if you use `deadLetter`.

Credentials come from the connection's `Credential` property, and Google's default credential
resolution applies when it is left null.

## GCP Pub/Sub Connection

`GcpMessagingGatewayConnection` holds the project and the credential, and hands the underlying
Google client builders to you for anything Brighter does not expose. It takes its options as
properties.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.GcpPubSub.GcpMessagingGatewayConnection -->

| Option | Type | Default | Description |
|---|---|---|---|
| `Credential` | `ICredential?` | `null` | The credential used to authenticate with Pub/Sub; null falls back to Google's default credential resolution. |
| `ProjectId` | `string` | `""` | The Google Cloud project topics and subscriptions are created in. |
| `TopicManagerConfiguration` | `Action<PublisherServiceApiClientBuilder>?` | `null` | Configures the client Brighter uses to create, update and delete topics. |
| `PublisherConfiguration` | `Action<PublisherClientBuilder>?` | `null` | Configures the client Brighter uses to publish messages. |
| `SubscriptionManagerConfiguration` | `Action<SubscriberServiceApiClientBuilder>?` | `null` | Configures the client Brighter uses to create, update and delete subscriptions. |
| `StreamConfiguration` | `Action<SubscriberClientBuilder>?` | `null` | Configures the client Brighter uses to consume messages. |
| `ProjectsClientConfiguration` | `Action<ProjectsClientBuilder>?` | `null` | Configures the client Brighter uses to look up the project's service account. |

## GCP Pub/Sub Publication

`GcpPublication` takes its options as properties and adds these three to the
[base publication options](/contents/CommandProcessorConfigurationReference.md#publication-options),
which it inherits.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.GcpPubSub.GcpPublication -->

| Option | Type | Default | Description |
|---|---|---|---|
| `TopicAttributes` | `TopicAttributes?` | `null` | The attributes applied to the topic when Brighter creates or updates it. |
| `EnableMessageOrdering` | `bool` | `false` | Whether the publisher sends messages with an ordering key. |
| `PublisherClientConfiguration` | `Action<PublisherClientBuilder>?` | `null` | Configures the publishing client for this topic alone. |

`TopicAttributes` is a class rather than an option, and it carries the settings Pub/Sub applies
to a topic at creation: `Name`, `ProjectId`, `Labels`, `MessageRetentionDuration`,
`StorePolicy`, `SchemaSettings`, `KmsKeyName` and a `TopicConfiguration` action that reaches
the underlying `Topic` object. The topic takes its name from the publication's `Topic` when
`Name` is left empty.

## GCP Pub/Sub Subscription

`GcpPubSubSubscription` takes its options as constructor arguments, so the option is the
parameter you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; the other sixteen are Pub/Sub's own. At thirty-three parameters it is the widest
subscription Brighter ships.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.GcpPubSub.GcpPubSubSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
     manual: timeProvider — initialised to TimeProvider.System, which has no printable value
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the Pub/Sub subscription this consumer reads. |
| `routingKey` | `RoutingKey` | `none` | The Pub/Sub topic the subscription is attached to. |
| `requestType` | `Type?` | `none` | The request type messages on this subscription are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the subscription. |
| `bufferSize` | `int` | `1` | Messages read at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this subscription, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; a `GcpPubSubChannelFactory` is used when null. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `projectId` | `string?` | `null` | The project this subscription and its topic live in; null uses the connection's project. |
| `topicAttributes` | `TopicAttributes?` | `null` | The attributes applied to the topic when Brighter creates it for this subscription. |
| `ackDeadlineSeconds` | `int` | `30` | Seconds the consumer has to acknowledge a message before Pub/Sub redelivers it. |
| `retainAckedMessages` | `bool` | `false` | Whether Pub/Sub keeps acknowledged messages for replay. |
| `messageRetentionDuration` | `TimeSpan?` | `null` | How long Pub/Sub retains an unacknowledged message. |
| `labels` | `MapField<string, string>?` | `empty` | Labels attached to the subscription; Brighter adds a `source` label of `brighter`. |
| `enableMessageOrdering` | `bool` | `false` | Whether messages published with an ordering key are delivered in order. |
| `enableExactlyOnceDelivery` | `bool` | `false` | Whether Pub/Sub applies its exactly-once delivery guarantee to this subscription. |
| `storage` | `CloudStorageConfig?` | `null` | The Cloud Storage bucket messages are exported to. |
| `expirationPolicy` | `ExpirationPolicy?` | `null` | How long the subscription survives inactivity before Pub/Sub deletes it. |
| `deadLetter` | `DeadLetterPolicy?` | `null` | The dead letter topic failed messages are forwarded to, and the attempts allowed first. |
| `maxRequeueDelay` | `TimeSpan?` | `600000 ms` | The ceiling on the exponential backoff Pub/Sub applies between redeliveries. |
| `timeProvider` | `TimeProvider?` | `TimeProvider.System` | The time source used for time-dependent operations such as purging. |
| `subscriptionMode` | `SubscriptionMode` | `Stream` | Selects the streaming pull consumer or the unary pull consumer. |
| `streamingConfiguration` | `Action<SubscriberClientBuilder>?` | `null` | Configures the streaming client for this subscription alone. |
| `subscriberMember` | `string?` | `null` | The IAM member granted the subscriber role; null derives the project's Pub/Sub service account. |

`DeadLetterPolicy` is a class rather than an option. It takes the dead letter topic and its
subscription as constructor arguments and exposes `PublisherMember`, `SubscriberMember`,
`AckDeadlineSeconds` (60) and `MaxDeliveryAttempts` (10) as properties.

The generic form `GcpPubSubSubscription<T>` supplies `requestType` from `T` and takes the same
options otherwise, apart from `streamingConfiguration`, which it does not expose. It still
requires a subscription name, a channel name and a routing key, and it leaves `messagePumpType`
required, so state Reactor or Proactor on every subscription.

## GCP Pub/Sub Configuration Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.GcpPubSub;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connection = new GcpMessagingGatewayConnection { ProjectId = "my-project" };

builder.Services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new GcpPubSubProducerRegistryFactory(
            connection,
            [
                new GcpPublication
                {
                    Topic = new RoutingKey("greeting.event"),
                    RequestType = typeof(GreetingEvent)
                }
            ]).Create();
    })
    .AutoFromAssemblies();

builder.Services.AddConsumers(options =>
{
    options.Subscriptions =
    [
        new GcpPubSubSubscription<GreetingEvent>(
            new SubscriptionName("paramore.example.greeting"),
            new ChannelName("greeting.event"),
            new RoutingKey("greeting.event"),
            messagePumpType: MessagePumpType.Proactor,
            ackDeadlineSeconds: 60,
            subscriptionMode: SubscriptionMode.Stream)
    ];
    options.DefaultChannelFactory = new GcpPubSubChannelFactory(connection);
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
