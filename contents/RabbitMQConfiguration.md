---
description: "RabbitMQ is OSS message-oriented-middleware and is well documented."
layout:
  description:
    visible: false
---

# RabbitMQ Configuration

> **Reference** · Applies to **Brighter V10**

## RabbitMQ General

RabbitMQ is OSS message-oriented-middleware and is [well documented](https://www.rabbitmq.com/documentation.html). Brighter handles the details of sending to or receiving from RabbitMQ. You may find it useful to understand the [building blocks](https://www.rabbitmq.com/tutorials/amqp-concepts.html) of the protocol. You might find the [documentation for the .NET SDK](https://www.rabbitmq.com/dotnet-api-guide.html) helpful when debugging, but you should not have to interact with it directly to use Brighter.

RabbitMQ offers an API that defines primitives used to configure the middleware used for messaging:

- **Exchange**: A routing table. Different types of exchanges route messages differently. An entry in the table is a **Routing Key**.
- **Queue**: A store-and-forward queue over which a consumer receives messages. A message is locked whilst a consumer has read it, until they ack it, upon which it is deleted from the queue, or nack it, upon which it is requeued or sent to a DLQ.
- **Binding**: Adds a queue as a target for a routing rule on an exchange. The routing key is used for this on a direct exchange (on the default exchange the routing key is the queue name).

We connect to RabbitMQ via a multiplexed TCP/IP connection - RabbitMQ calls these channels. Brighter uses a push consumer, so it has an open channel and can be seen on the consumers list in the management console. Brighter maintains a pool of connections and when asked for a new connection will take one from it's pool in preference to creating a new one.

## RabbitMQ.Client v7 Support

The `RabbitMQ.Client` library introduced significant breaking changes in version 7, most notably making its API entirely asynchronous. To support this new version without imposing a breaking change on all existing Brighter users, a new, separate package has been created:

- [Paramore.Brighter.MessagingGateway.RMQ.Async](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.RMQ.Async)

This is important because it allows you to choose the implementation that best fits your project:

- For existing projects, you can continue to use the `Paramore.Brighter.MessagingGateway.RMQ.Sync` package with `RabbitMQ.Client` v6.x and its synchronous API.
- For new projects, or when you are ready to adopt the `async`-native client, you can use the new `Paramore.Brighter.MessagingGateway.RMQ.Async` package. This package is designed to work with the fully asynchronous API of `RabbitMQ.Client` v7+, which can offer better performance and aligns with modern .NET asynchronous programming patterns.

## Breaking Changes: Package Rename and Proactor Subscription

With the introduction of the `Paramore.Brighter.MessagingGateway.RMQ.Async` package, the original `Paramore.Brighter.MessagingGateway.RMQ` package has been renamed to `Paramore.Brighter.MessagingGateway.RMQ.Sync`. This change better reflects its synchronous nature.

A significant breaking change is the removal of the proactor subscription model from the `Paramore.Brighter.MessagingGateway.RMQ.Sync` package. The proactor pattern is inherently asynchronous and is better suited for the new fully asynchronous `RabbitMQ.Client` v7.

If your application relies on proactor subscriptions for efficient, non-blocking message consumption, you must migrate to the `Paramore.Brighter.MessagingGateway.RMQ.Async` package. This package provides a native, high-performance asynchronous consumer that integrates correctly with the `RabbitMQ.Client` v7+ API.

## RabbitMQ Connection

The Connection to RabbitMQ is provided by an **RmqMessagingGatewayConnection** which allows you to configure the following:

- **Name**: A unique name for the connection, for diagnostic purposes
- **AmqpUri**: A connection to AMQP in the form of an [RabbitMQ Uri](https://www.rabbitmq.com/uri-spec.html) **Uri** with reliability options for a retry count (defaults to 3), **ConnectionRetryCount**, retry interval (defaults to 1000ms) **RetryWaitInMilliseconds** and a circuit breaker retry timeout (defaults to 60000ms), **CircuitBreakTimeInMilliseconds**, which introduces a delay when connections exceed the retry count.
- **Exchange**: The definition of the exchange. **Name** is the identifier for the exchange. All exchanges have a [**Type**](https://www.rabbitmq.com/tutorials/amqp-concepts.html), and the default is **ExchangeType.Direct**, but it is a string value that supports all RabbitMQ exchange types on the .NET SDK. The **Durable** flag is used to indicate if the exchange definition survives node failure or restart of the broker which defaults to *false*. **SupportDelay** indicates if the Exchange supports retry with delay, which defaults to *false*.
- **DeadLetterExchange**: Another exchange definition, but this one is used to host any Dead Letter Queues (DLQ). This could be the same exchange, but normal practice is to use a different exchange.
- **Heartbeat**: RabbitMQ uses a heartbeat to determine if a connection has died. This sets the interval for that heartbeat. Defaults to 20s.
- **PersistMessages**: Should messages be saved to disk? Saving messages to disk allows them to be recovered if a node fails, defaults to *false*. See [RabbitMQ Persistence Options](#rabbitmq-persistence-options) for more details.
- **ContinuationTimeout**: RabbitMQ protocol timeouts in seconds. Defaults to 20s. See [ConnectionFactory.ContinuationTimeout](https://www.rabbitmq.com/dotnet-api-guide.html) for more information.

In RabbitMQ, recreating an exiting primitive is a no-op provided the definition does not change.

## RabbitMQ Connection Options

`RmqMessagingGatewayConnection` takes its options as properties, so the option is the
property you set. The property is spelled `AmpqUri`, not `AmqpUri` — the transposition is in
Brighter's own API and a reader who types the protocol's spelling does not compile.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessagingGatewayConnection
     manual: Name — the default is Environment.MachineName, so there is no value a table can print
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | the machine name | Names the connection in the broker's connection list. |
| `AmpqUri` | `AmqpUriSpecification?` | `null` | The AMQP URI the client connects with, and its retry settings. |
| `Exchange` | `Exchange?` | `null` | The exchange messages are published to and queues bind against. |
| `DeadLetterExchange` | `Exchange?` | `null` | The exchange dead-lettered messages are routed to. |
| `Heartbeat` | `ushort` | `20` | Seconds between heartbeats before the broker treats the connection as dead. |
| `PersistMessages` | `bool` | `false` | Whether published messages are written to disk. |
| `ContinuationTimeout` | `ushort` | `20` | Seconds a protocol operation waits for its reply. |
| `ClientCertificate` | `X509Certificate2?` | `null` | The client certificate presented for mutual TLS. |
| `ClientCertificatePath` | `string?` | `null` | Path to a client certificate file, loaded when no certificate is supplied directly. |
| `ClientCertificatePassword` | `string?` | `null` | Password for the certificate file at `ClientCertificatePath`. |
| `TrustServerSelfSignedCertificate` | `bool` | `false` | Whether a self-signed certificate from the broker is accepted. |

`Name` is the one row with no default a table can state: it is `Environment.MachineName`, so
it differs on every machine the process runs on. Set it explicitly when you want to recognise
the connection in the management console. The four certificate options configure a TLS
connection to the broker and are not exercised by any example on this page.

The following code creates a typical RabbitMQ connection (here shown as part of configuring an External Bus):

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
       .AddProducers((configure) =>
        {
            configure.ProducerRegistry = new RmqProducerRegistryFactory(
                new RmqMessagingGatewayConnection
                {
                    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                    Exchange = new Exchange("paramore.brighter.exchange"),
                },

                ...//publication, see below
            
            ).Create();
        }    
}
```

## RabbitMQ Connection Reliability Options

These are the knobs on the connection above. Each has a default, so you set only
the ones your network obliges you to; for how to choose values, see
[RabbitMQ Connection Stability](/contents/RabbitMQConnectionStability.md).

**Configuration Options**:

- **connectionRetryCount**: Number of times to retry connecting to RabbitMQ before giving up (default: 3)
- **retryWaitInMilliseconds**: Time to wait between retry attempts (default: 1000ms)
- **circuitBreakerTimeInMilliseconds**: Time to wait before attempting to reconnect after exceeding retry count (default: 60000ms)
- **Heartbeat**: Interval for RabbitMQ heartbeat checks to detect dead connections (default: 20s)

## RabbitMQ Publication

For more on a *Publication* see the material on an *Add Producers* in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#using-an-external-bus).

We only support one custom property on RabbitMQ which configures shutdown delay to await pending confirmations.

Under the hood, Brighter uses [Publisher Confirms](https://www.rabbitmq.com/confirms.html) to update its Outbox for the dispatch time. This means that when publishing a message we allow RabbitMQ to confirm delivery of a message to all available nodes asynchronously, and then call us back, over blocking. This allows for higher throughput. But it means that we cannot update the Outbox to show a message as dispatched, until we receive the callback, which may occur after your handler pipeline for that message has completed and the message has been acknowledged.  

When shutting down a producer, it is possible that not all confirms have yet been received from RabbitMQ. The delay instructs Brighter to wait for a period of time, in order to allow the confirms to arrive. 

Missing a confirm will cause the *Outbox Sweeper* to resend a message, as it will not be marked as dispatched. (This is why we refer to Guaranteed *At Least Once* because there are many opportunities where messages may be duplicated in order to guarantee they were sent).  

## RabbitMQ Publication Options

`RmqPublication` adds one option to the
[base publication options](/contents/CommandProcessorConfigurationReference.md#publication-options),
which it inherits and which are set the same way.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqPublication -->

| Option | Type | Default | Description |
|---|---|---|---|
| `WaitForConfirmsTimeOutInMilliseconds` | `int` | `500` | Milliseconds a producer waits at shutdown for outstanding publisher confirms. |

The following code creates a *Publication* for RabbitMQ when configuring an *External Bus*

``` csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
      .AddProducers((configure) =>
        {
            configure.ProducerRegistry = new RmqProducerRegistryFactory(
 
                ...//connection, see above

                new RmqPublication[]{
                    new RmqPublication
                {
                    Topic = new RoutingKey("GreetingMade"),
                    WaitForConfirmsTimeOutInMilliseconds = 1000,
                    MakeChannels = OnMissingChannel.Create
                }}
            ).Create();

            // Outbox thresholds are producers configuration, not publication
            configure.MaxOutStandingMessages = 5;
            configure.MaxOutStandingCheckInterval = TimeSpan.FromMilliseconds(500);
}
```

## Putting It Together

Our combined code for the *Connection*  with a single *Publication* looks like this

``` csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
      .AddProducers((configure) =>
        {
            configure.ProducerRegistry = new RmqProducerRegistryFactory(
               new RmqMessagingGatewayConnection
                {
                    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                    Exchange = new Exchange("paramore.brighter.exchange"),
                },
                new RmqPublication[]{
                    new RmqPublication
                {
                    Topic = new RoutingKey("GreetingMade"),
                    WaitForConfirmsTimeOutInMilliseconds = 1000,
                    MakeChannels = OnMissingChannel.Create
                }}
            ).Create();

            // Outbox thresholds are producers configuration, not publication
            configure.MaxOutStandingMessages = 5;
            configure.MaxOutStandingCheckInterval = TimeSpan.FromMilliseconds(500);
        }
}
```

## RabbitMQ Subscription

For more on a *Subscription* see the material on configuring the *Dispatcher* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-dispatcher).

We support a number of RabbitMQ specific *Subscription* options:

- **DeadLetterChannelName**: The name of the queue to subscribe to DLQ notifications for this subscription (without a queue, the messages sent to the Dead Letter Exchange (DLX) will not be stored) 
- **DeadLetterRoutingKey**: The routing key that binds the DLQ to the DLX
- **HighAvailability**: [Deprecated] Not used on versions of RabbitMQ 3+. Prior to this, configuring that a queue should be mirrored was an API option, now it is a configuration management option on the broker.
- **IsDurable**: Should subscription definitions survive a restart of nodes in the broker.
- **MaxQueueLength**: [Deprecated] Prefer to use policy to set this instead (see [RabbitMQ docs](https://www.rabbitmq.com/maxlength.html)). The maximum length a RabbitMQ queue can grow to, before new messages are rejected (and sent to a DLQ if there is one).

## RabbitMQ Subscription Options

`RmqSubscription` takes its options as constructor arguments, so the option is the parameter
you type. The first seventeen are
[`Subscription`'s](/contents/DispatcherConfigurationReference.md#subscription-options) and
behave the same way here; the rest are RabbitMQ's own.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the RabbitMQ queue this subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The routing key the queue binds to on the exchange. |
| `requestType` | `Type?` | `none` | The request type messages on this queue are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the queue. |
| `bufferSize` | `int` | `1` | Messages read from the queue at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this queue, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the queue as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `isDurable` | `bool` | `false` | Whether the queue definition survives a broker restart. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; falls back to `DefaultChannelFactory` when null. |
| `highAvailability` | `bool` | `false` | Mirrors the queue across nodes on brokers before RabbitMQ 3. |
| `deadLetterChannelName` | `ChannelName?` | `null` | Names the queue bound to the dead letter exchange for this subscription. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key binding the dead letter queue to the dead letter exchange. |
| `ttl` | `TimeSpan?` | `null` | How long a message stays on the queue before RabbitMQ discards it. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates the queue and binding, validates them, or assumes them. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `maxQueueLength` | `int?` | `null` | Messages the queue holds before new ones are rejected. |
| `queueType` | `QueueType` | `Classic` | Selects a classic or quorum queue. |

`queueType` is the one option the two clients do not share: the `RMQ.Sync` package's
`RmqSubscription` takes 23 parameters and has no `queueType` among them, so quorum queues are
available only on `RMQ.Async`. `highAvailability` and `maxQueueLength` are both deprecated,
as the bullets above record.

The generic form `RmqSubscription<T>`, which every example below uses, takes the same options
and supplies four defaults the table cannot: `requestType` is `T`, `subscriptionName`,
`channelName` and `routingKey` are `T`'s full name, and `messagePumpType` is `Proactor`.

This is a typical *Subscription* configuration in a Consumer application:

``` csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new RmqSubscription<GreetingMade>(
            new SubscriptionName("paramore.sample.salutationanalytics"),
            new ChannelName("SalutationAnalytics"),
            new RoutingKey("GreetingMade"),
            messagePumpType: MessagePumpType.Reactor,
            timeOut: TimeSpan.FromMilliseconds(200),
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(
                    new Uri("amqp://guest:guest@localhost:5672")
                    connectionRetryCount: 5,
                    retryWaitInMilliseconds: 250,
                    circuitBreakerTimeInMilliseconds = 30000
                ),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    services.AddConsumers(options =>
        {
            options.Subscriptions = subscriptions;
            options.ChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
            ... //see Basic Configuration
        })
```

## RabbitMQ Quorum Queue Requirements

To use quorum queues, you must configure the subscription with:

- **queueType**: Set to `QueueType.Quorum`
- **isDurable**: Must be `true` (required for quorum queues)
- **highAvailability**: Must be `false` (quorum queues provide their own replication)

```csharp
// ...
var subscription = new RmqSubscription<GreetingMade>(
    new SubscriptionName("paramore.sample.salutationanalytics"),
    new ChannelName("SalutationAnalytics"),
    new RoutingKey("GreetingMade"),
    messagePumpType: MessagePumpType.Proactor,
    timeOut: TimeSpan.FromMilliseconds(200),
    isDurable: true,              // Required for quorum queues
    highAvailability: false,      // Must be false for quorum queues
    queueType: QueueType.Quorum,  // Use quorum queue
    makeChannels: OnMissingChannel.Create
);
```

### Validation

If you attempt to create a quorum queue without meeting the configuration requirements, Brighter will throw an exception during queue creation. The validation ensures:

- `isDurable` is `true`
- `highAvailability` is `false`

Why you would want a quorum queue, and what it costs, is in
[RabbitMQ Durability](/contents/RabbitMQDurability.md); moving an existing
subscription onto one is in
[Migrating to Quorum Queues](/contents/RabbitMQMigrateToQuorumQueues.md).

## RabbitMQ Persistence Options

For full persistence, you should configure:

**Producer Configuration**:

- Set `PersistMessages = true` on the connection
- Set `durable: true` on the Exchange definition
- Messages will be marked with `DeliveryMode = Persistent`

**Consumer Configuration**:

- Set `isDurable: true` on the subscription
- This ensures the queue definition survives broker restarts

**Complete Example**:

```csharp
// ...
// Producer Configuration
services.AddBrighter(...)
    .AddProducers((configure) =>
    {
        configure.ProducerRegistry = new RmqProducerRegistryFactory(
            new RmqMessagingGatewayConnection
            {
                AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                Exchange = new Exchange(
                    name: "paramore.brighter.exchange",
                    type: ExchangeType.Direct,
                    durable: true  // Exchange survives restarts
                ),
                PersistMessages = true  // Messages saved to disk
            },
            new RmqPublication[]
            {
                new RmqPublication
                {
                    Topic = new RoutingKey("GreetingMade"),
                    WaitForConfirmsTimeOutInMilliseconds = 1000,
                    MakeChannels = OnMissingChannel.Create
                }
            }
        ).Create();

        // Outbox thresholds are producers configuration, not publication
        configure.MaxOutStandingMessages = 5;
        configure.MaxOutStandingCheckInterval = TimeSpan.FromMilliseconds(500);
    });

// Consumer Configuration
var subscriptions = new Subscription[]
{
    new RmqSubscription<GreetingMade>(
        new SubscriptionName("paramore.sample.salutationanalytics"),
        new ChannelName("SalutationAnalytics"),
        new RoutingKey("GreetingMade"),
        messagePumpType: MessagePumpType.Proactor,
        timeOut: TimeSpan.FromMilliseconds(200),
        isDurable: true,  // Queue survives restarts
        makeChannels: OnMissingChannel.Create
    )
};

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange(
        name: "paramore.brighter.exchange",
        type: ExchangeType.Direct,
        durable: true
    )
};
```

The trade-offs these settings buy and pay for are in
[RabbitMQ Durability](/contents/RabbitMQDurability.md).

## RabbitMQ Ack and Nack Behaviour

We use RabbitMQ's queues to subscribe to a routing key on an exchange.

When we Accept/Ack a message, in response to a handler chain completing, we Ack the message to RabbitMQ using **Channel.BasicAck**. Note that we only Ack a message once we have completed running the chain. 

When we Reject/Nack a message (see [Handler Failure](/contents/HandlerFailure.md) for more on failure) then we use **Channel.Reject** to delete the message, and move it to a DLQ if there is one.

Brighter has an internal buffer for messages pushed to a *Performer* (a thread running a message pump). This buffer has thread affinity (in RabbitMQ we have to Ack or Nack from the thread that received the message). When a consumer closes its connection to RabbitMQ, messages in the buffer that have not been Ack'd or Nack'd will be returned to the queue.

## Further Reading

- [RabbitMQ Durability](/contents/RabbitMQDurability.md) — why quorum queues and message persistence exist, and what each costs
- [Migrating to Quorum Queues](/contents/RabbitMQMigrateToQuorumQueues.md) — moving an existing subscription onto a quorum queue
- [RabbitMQ Connection Stability](/contents/RabbitMQConnectionStability.md) — configuring retry, heartbeats and blocked-connection handling
- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — the Brighter-wide configuration these options sit inside
