---
description: "Azure Service Bus (ASB) is a fully managed enterprise message broker and is well documented Brighter handles the details of sending to or receiving from ASB."
layout:
  description:
    visible: false
---

# Azure Service Bus Configuration

> **Reference** · Applies to **Brighter V10**

## Azure Service Bus General
Azure Service Bus (ASB) is a fully managed enterprise message broker and is [well documented](https://docs.microsoft.com/en-us/azure/service-bus-messaging/) Brighter handles the details of sending to or receiving from ASB.  You may find it useful to understand the [concepts](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions) of the ASB.

## Azure Service Bus Connection
The connection to ASB id defined by an **IServiceBusClientProvider**, Brighter proviedes the following Implimentations

* **ServiceBusChainedClientProvider**: A client provider that allows you to specific a chain of **TokenCredentials** to authenticate with.

* **ServiceBusConnectionStringClientProvider**: A client provider that accepts a connection string (containg Authentication information)

* **ServiceBusDefaultAzureClientProvider**: A client provider that uses the Default Azure Credential to authenticate.

* **ServiceBusManagedIdentityClientProvider**: A client provider that uses Azure Managed Identity to authenticate.

* **ServiceBusVisualStudioCredentialClientProvider**: A client provider that uses Visual Studio Credential to authenticate.

In Brighter's implementation of the Messaging Gateway *Publications* and *Subscriptions* have their own Individual configuration.

## Azure Service Bus Connection Options

`AzureServiceBusConfiguration` is what the producer and consumer factories take when you
supply a connection string rather than a client provider. It takes both options as
constructor arguments, so the option is the parameter you type.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `connectionString` | `string` | `none` | The connection string the client authenticates and connects with. |
| `bulkSendBatchSize` | `int` | `10` | Messages sent in one transmission when the producer sends more than one. |

Both are read back capitalised — `ConnectionString` and `BulkSendBatchSize` — and both are
get-only, so they are set once at construction.

## Azure Service Bus Publication

`AzureServiceBusPublication` adds no properties to the
[base publication options](/contents/CommandProcessorConfigurationReference.md#publication-options).
It does add one public *field*, `UseServiceBusQueue`, which defaults to `false` and sends to a
Service Bus queue instead of a topic.

Basic Brighter configutarion publications is as follows

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AddProducers(
        new AzureServiceBusProducerRegistryFactory(
                asbConnection,
                new AzureServiceBusPublication[]
                {
                    new() { Topic = new RoutingKey("greeting.event") },
                    new() { Topic = new RoutingKey("greeting.addGreetingCommand") },
                    new() { Topic = new RoutingKey("greeting.Asyncevent") }
                }
            )
            .Create()
    )
}
```

For more on a *Publication* see the material on an *Add Producers* in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#using-an-external-bus).

## Azure Service Bus Subscription

For more on a *Subscription* see the material on configuring the *Dispatcher* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-dispatcher).

A subscription has two configuration surfaces: `AzureServiceBusSubscription`, which is the
Brighter subscription, and `AzureServiceBusSubscriptionConfiguration`, which describes the
Service Bus entity and is passed to the subscription as `subscriptionConfiguration`.

## Azure Service Bus Entity Options

`AzureServiceBusSubscriptionConfiguration` takes its options as properties, so the option is
the property you set.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusSubscriptionConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `MaxDeliveryCount` | `int` | `5` | Deliveries the transport attempts before dead-lettering a message. |
| `DeadLetteringOnMessageExpiration` | `bool` | `true` | Whether an expired message is dead-lettered rather than dropped. |
| `LockDuration` | `TimeSpan` | `60000 ms` | How long a message lock is held while a handler runs. |
| `DefaultMessageTimeToLive` | `TimeSpan` | `259200000 ms` | How long a message sits on the entity before it expires. |
| `QueueIdleBeforeDelete` | `TimeSpan` | `TimeSpan.MaxValue` | How long a queue is idle before Service Bus deletes it. |
| `RequireSession` | `bool` | `false` | Whether the subscription is session-enabled. |

`MaxDeliveryCount` is the transport's own count and is not Brighter's `requeueCount`: it fires
on lock expiry, which is what a slow or crashed handler looks like to Service Bus.
`LockDuration` is one minute and `DefaultMessageTimeToLive` is three days.

**Two further options are public fields rather than properties**, so they are not on the table
above: `SqlFilter` (default `""`), a [Topic Filter](https://docs.microsoft.com/en-us/azure/service-bus-messaging/topic-filters)
applied to the subscription, and `UseServiceBusQueue` (default `false`), which reads a Service
Bus queue instead of a topic subscription.

## Azure Service Bus Subscription Options

`AzureServiceBusSubscription` takes its options as constructor arguments, so the option is the
parameter you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; `subscriptionConfiguration` is the one it adds.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
     manual: subscriptionConfiguration — read back as Configuration, and the body substitutes a default AzureServiceBusSubscriptionConfiguration when it is null
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the Service Bus subscription or queue this reads. |
| `routingKey` | `RoutingKey` | `none` | The Service Bus topic the subscription is created on. |
| `requestType` | `Type?` | `none` | The request type messages on this channel are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the channel. |
| `bufferSize` | `int` | `1` | Messages read from the entity at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this channel, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the entity as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; falls back to `DefaultChannelFactory` when null. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates the topic and subscription, validates them, or assumes them. |
| `subscriptionConfiguration` | `AzureServiceBusSubscriptionConfiguration?` | a default `AzureServiceBusSubscriptionConfiguration` | Describes the Service Bus entity Brighter creates; read back as `Configuration`. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |

Leaving `subscriptionConfiguration` null gives you the defaults in the table above it, so the
entity is created with a one-minute lock and a three-day time to live.

The generic form `AzureServiceBusSubscription<T>`, which every example below uses, takes the
same options and supplies four defaults the table cannot: `requestType` is `T`,
`subscriptionName`, `channelName` and `routingKey` are `T`'s full name, and `messagePumpType`
is `Proactor`.

This is a typical *Subscription* configuration in a Consumer application:

``` csharp
// ...
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new AzureServiceBusSubscription<GreetingAsyncEvent>(
            new SubscriptionName(GreetingEventAsyncMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(GreetingEventAsyncMessageMapper.Topic),
            timeOut: TimeSpan.FromMilliseconds(400),
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            messagePumpType: MessagePumpType.Proactor,
            noOfPerformers: 2, unacceptableMessageLimit: 1),
        new AzureServiceBusSubscription<GreetingEvent>(
            new SubscriptionName(GreetingEventMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(GreetingEventMessageMapper.Topic),
            timeOut: TimeSpan.FromMilliseconds(400),
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            messagePumpType: MessagePumpType.Reactor,
            noOfPerformers: 2),
        new AzureServiceBusSubscription<AddGreetingCommand>(
            new SubscriptionName(AddGreetingMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(AddGreetingMessageMapper.Topic),
            timeOut: TimeSpan.FromMilliseconds(400),
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            messagePumpType: MessagePumpType.Proactor,
            noOfPerformers: 2)
    };

    var clientProvider = new ServiceBusVisualStudioCredentialClientProvider("my-awesome-asb.servicebus.windows.net");

    var asbConsumerFactory = new AzureServiceBusConsumerFactory(clientProvider);

    builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new AzureServiceBusChannelFactory(asbConsumerFactory);
        
    }
```

## Complete Reject

We use ASB's *Subscription* to surscribe to a Topic on a namespace.

When we Complete a message, in response to a handler chain completing, we Complete the message on ASB using **messageReceiver.CompleteMessageAsync**. Note that we only Complete a message once we have completed running the chain and only if AckOnRead is set to false (as the messages is removed from the queue otherwise).

When we Dead Letter a message (see [Handler Failure](/contents/HandlerFailure.md) for more on failure) then we use **messageReceiver.DeadLetterMessageAsync** to delete the message, and move it to a DLQ.
