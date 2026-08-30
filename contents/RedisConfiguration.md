---
description: "Brighter's Redis transport turns a Redis list into a queue, and configures it with a gateway configuration shared by producers and consumers and a subscription per consumer."
layout:
  description:
    visible: false
---

# Redis Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Brighter's Redis transport turns a Redis list into a queue, and configures it with a gateway configuration shared by producers and consumers and a subscription per consumer.

## Redis General

Install the transport package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.Redis
```

Brighter stores each message body under a key and pushes its id onto a list per topic, so a
consumer pops an id and reads the body back. The transport is built on **ServiceStack.Redis**,
and three consequences follow from that:

- **The connection string is ServiceStack's**, not `StackExchange.Redis`'s. It is set on
  `RedisConnectionString`, and it accepts ServiceStack's query-string options —
  `localhost:6379?connectTimeout=1&sendTimeout=1000&`.
- **Most of the configuration sets ServiceStack's static `RedisConfig`**, so it is
  process-wide rather than per-gateway. Brighter applies a value only when you supply one; a
  null leaves ServiceStack's own default in place, which is why every default in the table
  below is `null`.
- **A message body is reclaimed independently of its queue entry.** `MessageTimeToLive` sets
  how long the body survives, and an id that outlives its body is rejected when it is read.

Redis ships a publication type, `RedisMessagePublication`, but it adds no option of its own —
it exists so the producer registry factory can be typed, and everything on it is a
[base publication option](/contents/CommandProcessorConfigurationReference.md#publication-options).
There is no Redis publication table below for that reason.

## Redis Configuration Options

`RedisMessagingGatewayConfiguration` takes its options as properties, and both the producer
registry factory and the consumer factory take one.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.Redis.RedisMessagingGatewayConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `DefaultConnectTimeout` | `int?` | `null` | The socket connect timeout in milliseconds. |
| `DefaultSendTimeout` | `int?` | `null` | The socket send timeout in milliseconds. |
| `DefaultReceiveTimeout` | `int?` | `null` | The socket receive timeout in milliseconds. |
| `DefaultIdleTimeOutSecs` | `int?` | `null` | The seconds a pooled connection may idle before it is treated as stale. |
| `DefaultRetryTimeout` | `int?` | `null` | The milliseconds a failed operation is retried for. |
| `BufferPoolMaxSize` | `int?` | `null` | The size in bytes of the buffer pool operations draw from. |
| `VerifyMasterConnections` | `bool?` | `null` | Whether connections to master hosts are re-verified as still being masters. |
| `HostLookupTimeoutMs` | `int?` | `null` | The connect timeout in milliseconds used when finding the next available host. |
| `DeactivatedClientsExpiry` | `TimeSpan?` | `null` | How long a deactivated client is held before its connection is disposed. |
| `DisableVerboseLogging` | `bool?` | `null` | Whether detailed Redis operations are kept out of debug logging. |
| `BackoffMultiplier` | `int?` | `null` | The exponential backoff interval in milliseconds between connection retries. |
| `MaxPoolSize` | `int?` | `null` | The largest number of connections this gateway's pool holds. |
| `MessageTimeToLive` | `TimeSpan?` | `null` | How long a message body persists in Redis before it is reclaimed. |
| `RedisConnectionString` | `string?` | `null` | The ServiceStack connection string the gateway connects with. |

One further setting is declared on this type and is **not** on the table:
`AssumeServerVersion` is `static`, so it is set once for the process rather than per gateway,
and it skips ServiceStack's server-version check by naming a minimum version as an integer —
`2.8.12` is `2812`.

## Redis Subscription

`RedisSubscription` takes its options as constructor arguments, so the option is the parameter
you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; the other two are Brighter's dead letter and invalid message routing keys, which
Redis supports rather than delegating to the broker.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.Redis.RedisSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the Redis list this consumer pops ids from. |
| `routingKey` | `RoutingKey` | `none` | The topic the channel subscribes to. |
| `requestType` | `Type?` | `none` | The request type messages on this channel are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the channel. |
| `bufferSize` | `int` | `1` | Messages read from the list at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this channel, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `1000 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; supply a `ChannelFactory` over a `RedisMessageConsumerFactory`. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key messages are dead-lettered to. |
| `invalidMessageRoutingKey` | `RoutingKey?` | `null` | The routing key unacceptable messages are routed to. |

The generic form `RedisSubscription<T>`, which the sample uses, supplies `requestType` from `T`
and defaults `subscriptionName`, `channelName` and `routingKey` to `T`'s full name. It also
defaults `messagePumpType` to `Proactor`, where the non-generic form requires it.

## Redis Configuration Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Redis;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var redisConnection = new RedisMessagingGatewayConfiguration
{
    RedisConnectionString = "localhost:6379?connectTimeout=1&sendTimeout=1000&",
    MaxPoolSize = 10,
    MessageTimeToLive = TimeSpan.FromMinutes(10)
};

builder.Services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new RedisProducerRegistryFactory(
            redisConnection,
            [
                new RedisMessagePublication
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
        new RedisSubscription<GreetingEvent>(
            new SubscriptionName("paramore.example.greeting"),
            new ChannelName("greeting.event"),
            new RoutingKey("greeting.event"),
            timeOut: TimeSpan.FromSeconds(1))
    ];
    options.DefaultChannelFactory = new ChannelFactory(
        new RedisMessageConsumerFactory(redisConnection));
});

builder.Services.AddHostedService<ServiceActivatorHostedService>();

var host = builder.Build();
await host.RunAsync();
```

A working pair of programs in this shape is in the Brighter repository at
`samples/TaskQueue/RedisTaskQueue`.

## Further Reading

- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — registering Brighter
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md) — the subscription options every transport shares
- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md) — the publication options every transport shares
- [Reactor and Proactor](/contents/ReactorAndProactor.md) — choosing a message pump
- [Error Handling Options](/contents/ErrorHandlingOptions.md) — what Brighter does with a message it cannot process
