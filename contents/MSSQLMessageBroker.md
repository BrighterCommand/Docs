---
description: "The MSSQL message broker turns a SQL Server table into a queue, so an application that already has a database can send messages between processes without adding a broker to its infrastructure."
layout:
  description:
    visible: false
---

# MSSQL Message Broker

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

The MSSQL message broker turns a SQL Server table into a queue, so an application that already has a database can send messages between processes without adding a broker to its infrastructure.

## MSSQL Message Broker Overview

Install the transport package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.MsSql
```

A producer inserts a row carrying the topic, the message type and the payload; a consumer
reads the oldest row for its topic and deletes it. That is the whole mechanism, and it is the
same idea as the [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md), with the
same trade-offs — [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md)
applies here too.

**A queue table beats a broker in one situation and loses in most others.** It wins when you
cannot add infrastructure — a customer's estate you do not control — and the message volume is
low: the queue is transactional with the work that produced it, it survives a reboot, and
several producers and consumers can share a topic. It loses on throughput, on fan-out, and on
everything a broker gives you for free, because every read is a query and every consumer polls.

Brighter does not create the queue table. Create it before you start, with the columns the
transport expects:

```sql
CREATE TABLE [dbo].[QueueData](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [Topic] [nvarchar](255) NOT NULL,
    [MessageType] [nvarchar](1024) NOT NULL,
    [Payload] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_QueueData] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE NONCLUSTERED INDEX [IX_Topic] ON [dbo].[QueueData] ([Topic] ASC);
```

## MSSQL Message Broker Connection

**This transport has no connection type and no publication type of its own.** Both the
producer registry factory and the consumer factory take a
`RelationalDatabaseConfiguration`, the same type the relational Outbox and Inbox take, and the
producer registry factory takes base
[`Publication`](/contents/CommandProcessorConfigurationReference.md#publication-options)
objects.

Its eight options are documented once, at
[Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md),
because seventeen Brighter components share them. Two of them matter here:
`queueStoreTable` names the table above and **the producer registry factory throws when it is
empty**, and `connectionString` reaches SQL Server.

## MSSQL Message Broker Subscription

`MsSqlSubscription` takes its options as constructor arguments, so the option is the parameter
you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave the
same way here; the other two are Brighter's dead letter and invalid message routing keys.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.MsSql.MsSqlSubscription
     manual: requestType — the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the channel this subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The topic the consumer reads rows for. |
| `requestType` | `Type?` | `none` | The request type messages on this channel are translated into. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `requestType` | Determines the request type from the message rather than from the channel. |
| `bufferSize` | `int` | `1` | Rows read at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this topic, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the channel as empty. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is treated as a poison pill; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `Proactor` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; supply a `ChannelFactory` over a `MsSqlMessageConsumerFactory`. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates missing infrastructure, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key messages are dead-lettered to. |
| `invalidMessageRoutingKey` | `RoutingKey?` | `null` | The routing key unacceptable messages are routed to. |

`messagePumpType` has a usable default here, which is unusual: eleven of Brighter's thirteen
subscription types reject their declared default and make the parameter required. This package
supports one pump, so the parameter defaults to `Proactor`.

The generic form `MsSqlSubscription<T>` supplies `requestType` from `T` and defaults
`subscriptionName`, `channelName` and `routingKey` to `T`'s full name.

## MSSQL Message Broker Configuration Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.MsSql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var configuration = new RelationalDatabaseConfiguration(
    @"Database=BrighterSqlQueue;Server=.\sqlexpress;Integrated Security=SSPI;",
    databaseName: "BrighterSqlQueue",
    queueStoreTable: "QueueData");

builder.Services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new MsSqlProducerRegistryFactory(
            configuration,
            [
                new Publication
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
        new MsSqlSubscription<GreetingEvent>(
            new SubscriptionName("paramore.example.greeting"),
            new ChannelName("greeting.event"),
            new RoutingKey("greeting.event"))
    ];
    options.DefaultChannelFactory = new ChannelFactory(
        new MsSqlMessageConsumerFactory(configuration));
});

builder.Services.AddHostedService<ServiceActivatorHostedService>();

var host = builder.Build();
await host.RunAsync();
```

A working pair of programs in this shape is in the Brighter repository at
`samples/TaskQueue/MsSqlMessagingGateway`.

## Further Reading

- [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) — the same idea on PostgreSQL, with visibility timeouts
- [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md) — when a queue table is the right answer, and when it is not
- [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md) — the eight options this transport shares with the relational stores
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md) — the subscription options every transport shares
- [MSSQL Outbox](/contents/MSSQLOutbox.md) — the same database as a transactional Outbox
