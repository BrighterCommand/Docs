---
description: "One SQL Server database can carry your message queue, your Outbox and your Inbox together, sharing a single connection string and one configuration object."
layout:
  description:
    visible: false
---

# Use MSSQL for Transport, Outbox and Inbox

> **How-to** · Applies to **Brighter V10** · Prerequisites: [MSSQL Message Broker](/contents/MSSQLMessageBroker.md), [MSSQL Outbox](/contents/MSSQLOutbox.md)

One SQL Server database can carry your message queue, your Outbox and your Inbox together, sharing a single connection string and one configuration object.

[Use PostgreSQL for Both Transport and Outbox](/contents/PostgreSQLTransportAndOutbox.md) makes the case for composing a broker and an Outbox in one database: the business write and the message announcing it commit together, so there is no window in which the row exists and the message does not. That argument is not about PostgreSQL, and this guide is the demonstration — the same composition on SQL Server, with an Inbox added so the receiving end is idempotent too.

The shape is deliberately the same as the PostgreSQL guide's. Where the two differ, the difference is called out, and **step 2 is the one that matters** — SQL Server will not create your queue table for you.

A working version is in the Brighter repository at `Brighter/samples/TaskQueue/MsSqlMessagingGateway/GreetingsSender/`, with the consumer beside it in `GreetingsReceiverConsole/`.

## Step 1: Install the MSSQL Packages

The transport, the Outbox, the Inbox, the transaction provider, the Sweeper and the provisioner are separate packages:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.MsSql
dotnet add package Paramore.Brighter.Outbox.MsSql
dotnet add package Paramore.Brighter.Inbox.MsSql
dotnet add package Paramore.Brighter.MsSql
dotnet add package Paramore.Brighter.Outbox.Hosting
dotnet add package Paramore.Brighter.BoxProvisioning.MsSql
```

`Paramore.Brighter.MsSql` is the one people miss. It holds `MsSqlConnectionProvider` and `MsSqlTransactionProvider`, which are what let your handler and the Outbox share a transaction — without it there is no composition, only three subsystems pointed at the same database.

## Step 2: Create the Queue, Outbox and Inbox Tables

**The MSSQL transport does not create its queue table, and this is the one place the PostgreSQL pattern does not carry over.** `OnMissingChannel.Create` is accepted on an MSSQL publication and subscription and then never acted on — the gateway has no provisioning path at all, where the PostgreSQL gateway has one. Set the table up yourself before anything runs, or the first send fails against a table that is not there.

Brighter ships the DDL for you to run, in `MsSqlQueueBuilder`. Nothing in the product calls it, which is exactly why it is public:

```csharp
using Paramore.Brighter.MessagingGateway.MsSql;

// The queue table, and the index the consumer's topic lookup wants
string queueDdl = MsSqlQueueBuilder.GetDDL("QueueData");
string queueIndexDdl = MsSqlQueueBuilder.GetIndexDDL("QueueData");

// And the test for whether it is already there
string exists = MsSqlQueueBuilder.GetExistsQuery("QueueData", schemaName: "dbo");
```

That produces:

```sql
CREATE TABLE [QueueData] 
(
    [Id] [BIGINT] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Topic] [NVARCHAR](255) NOT NULL,
    [MessageType] [NVARCHAR](1024) NOT NULL,
    [Payload] [NVARCHAR](MAX) NOT NULL
);

CREATE NONCLUSTERED INDEX [IX_QueueData_Topic] ON [QueueData] ([Topic] ASC);
```

**The Outbox and Inbox tables** are created and migrated by [Box Provisioning](/contents/BoxProvisioning.md) at startup, which is the call in step 9. To manage them yourself instead, ask Brighter for the same DDL:

```csharp
using Paramore.Brighter.Inbox.MsSql;
using Paramore.Brighter.Outbox.MsSql;

string outboxDdl = SqlOutboxBuilder.GetDDL("Outbox");
string inboxDdl = SqlInboxBuilder.GetDDL("InboxMessages");
```

**Mind those two type names.** Every other provider prefixes its builders — `PostgreSqlOutboxBuilder`, `MySqlOutboxBuilder`, `SqliteOutboxBuilder`, `SpannerOutboxBuilder` — and MSSQL alone does not. `MsSqlOutboxBuilder` and `MsSqlInboxBuilder` do not exist.

Your own tables are yours. Brighter does not create, migrate or know about them.

## Step 3: Describe All Three Tables in One Configuration

This is the pivot the whole guide turns on. The queue store, the Outbox and the Inbox are three parameters on **one** object, so there is no second configuration to keep in step:

```csharp
using Paramore.Brighter;

const string connectionString =
    "Server=localhost,14330;Database=BrighterTests;User Id=sa;Password=Password1!;Encrypt=false";

// One object, three tables.
var configuration = new RelationalDatabaseConfiguration(
    connectionString,
    databaseName: "BrighterTests",
    outBoxTableName: "Outbox",
    inboxTableName: "InboxMessages",
    queueStoreTable: "QueueData");
```

**There is no connection wrapper here, and that is a difference from PostgreSQL.** PostgreSQL's transport takes a `PostgresMessagingGatewayConnection` holding the configuration; MSSQL's takes the `RelationalDatabaseConfiguration` directly. One less type, and one less thing to construct.

For every option this type carries — including `schemaName` and the payload flags — see [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md#relational-database-configuration-options).

## Step 4: Register the MSSQL Configuration

Put the same object in the container:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

builder.Services.AddSingleton<IAmARelationalDatabaseConfiguration>(configuration);
```

**This line is easy to leave out and the failure lands nowhere near it.** `TransactionProvider` in the next step is given as a *type*, so the container activates `MsSqlTransactionProvider`, and its constructor asks for exactly this interface. Omit the registration and your application starts, provisions the boxes, and only then throws — see [Failures](#mssql-transport-outbox-and-inbox-failures).

## Step 5: Wire the MSSQL Producer and the Outbox

The publication says where the event goes; the three lines after `ProducerRegistry` are what make the Outbox durable and transactional:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.MsSql;
using Paramore.Brighter.MsSql;
using Paramore.Brighter.Outbox.MsSql;

var producerRegistry = new MsSqlProducerRegistryFactory(
    configuration,
    [
        new Publication<GreetingEvent>
        {
            Topic = new RoutingKey("greeting.event")
        }
    ]).Create();

builder.Services
    .AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = producerRegistry;

        configure.Outbox = new MsSqlOutbox(configuration);
        configure.ConnectionProvider = typeof(MsSqlConnectionProvider);
        configure.TransactionProvider = typeof(MsSqlTransactionProvider);
    })
    .AutoFromAssemblies();
```

`MsSqlProducerRegistryFactory` takes the `RelationalDatabaseConfiguration` from step 3 directly. **There is no `MsSqlPublication`** — the publications are plain `Publication<T>`, because the MSSQL transport has no per-publication settings of its own.

**The transaction provider is not optional here.** It is what fixes the transaction type Brighter matches the Outbox against, and `MsSqlOutbox` only satisfies that match when the provider is SQL Server's. Leave it out and registration itself fails, again in [Failures](#mssql-transport-outbox-and-inbox-failures).

## Step 6: Wire the MSSQL Consumer

The consumer reads from the same queue store table. Note the order: `AddConsumers` extends `IServiceCollection`, while `AddProducers` extends the builder it returns, so a consumer registration comes first and everything else chains off it.

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.MsSql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var subscriptions = new Subscription[]
{
    new MsSqlSubscription<GreetingEvent>(
        new SubscriptionName("paramore.example.greeting"),
        new ChannelName("greeting.event"),
        new RoutingKey("greeting.event"),
        timeOut: TimeSpan.FromMilliseconds(200),
        messagePumpType: MessagePumpType.Reactor)
};

builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(
            new MsSqlMessageConsumerFactory(configuration));
    })
    .AutoFromAssemblies();

builder.Services.AddHostedService<ServiceActivatorHostedService>();
```

**Use `MsSqlSubscription<T>`, not `Subscription<T>`.** `ChannelFactory` casts what it is given down to `MsSqlSubscription` and throws on failure:

```text
Paramore.Brighter.ConfigurationException: MS SQL ChannelFactory We expect an MsSqlSubscription or MsSqlSubscription<T> as a parameter
```

It does this in all three of its channel-creation methods. A plain `Subscription<T>` compiles perfectly and then dies when the Dispatcher starts reading — which is worse than a compile error, because everything looks right until the moment it runs.

**`ChannelFactory` is a name ten transports share.** This one is `Paramore.Brighter.MessagingGateway.MsSql.ChannelFactory`, so mind the `using` directive if your solution talks to more than one broker.

## Step 7: Add the Inbox

The Inbox is what makes the *consumer* idempotent: it records the messages a handler has already seen, so a redelivery is recognised rather than reprocessed. Register the store on `AddConsumers`, and put the policy on the handler:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.MsSql;
using Paramore.Brighter.MessagingGateway.MsSql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(
            new MsSqlMessageConsumerFactory(configuration));

        // This supplies the STORE. The policy lives on the handler, below.
        options.InboxConfiguration = new InboxConfiguration(new MsSqlInbox(configuration));
    })
    .AutoFromAssemblies();
```

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;

public class GreetingEventHandler : RequestHandler<GreetingEvent>
{
    [UseInbox(step: 0, contextKey: nameof(GreetingEventHandler), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Warn)]
    public override GreetingEvent Handle(GreetingEvent @event)
    {
        // Runs once per message id, however many times the transport delivers it
        return base.Handle(@event);
    }
}
```

**`MsSqlInbox` takes the same configuration object**, reading `inboxTableName` from it — the third of the three names step 3 set.

> **Why the attribute rather than the global configuration alone.** `InboxConfiguration`'s
> other arguments — `scope`, `onceOnly`, `actionOnExists` — only reach the pipeline through
> `CommandProcessorBuilder`'s `ExternalBus` overloads. A process that registers **no producers**
> takes the `NoExternalBus` branch instead (`ServiceCollectionExtensions.cs:657`), and **that
> overload accepts no inbox at all** — so a consumer-only application gets no de-duplication and
> no warning that it is missing. Measured on a receiver with the Inbox table present and
> provisioned: **0 rows written with no producer registered, 1 row with one registered.**
> `[UseInbox]` is an ordinary handler attribute and does not depend on there being a bus.

The attribute's arguments are the ones worth deciding rather than defaulting:

| Option | Default | What it does |
|---|---|---|
| `contextKey` | the handler's type name | Scopes the record to one handler, so two handlers of the same event each get their own row |
| `onceOnly` | `false` on the attribute | Whether to de-duplicate at all. `false` records without suppressing, which is useful as an audit log |
| `onceOnlyAction` | `OnceOnlyAction.Throw` | What a duplicate does. `Throw` raises `OnceOnlyException`, `Warn` logs and drops it without calling your handler |

`OnceOnlyAction.Throw` is the default and it is the safe one, but on a transport that redelivers it will fill your logs with exceptions for messages that are being handled correctly. `Warn` is usually what you want once you trust the Inbox — remembering that it means the duplicate is *silently dropped* after one log line.

**`InboxScope` is inert and you should not reach for it.** `InboxConfiguration` accepts a `scope:` of `Commands`, `Events` or `All`, but the enumeration is referenced nowhere in the product outside its own declaration — control: `OnceOnlyAction` has 43 references — so setting it changes nothing. Use `contextKey` and which handlers carry the attribute to control what is recorded.

See [Brighter Inbox Support](/contents/BrighterInboxSupport.md) for more on `[UseInbox]`.

## Step 8: Deposit and Clear Inside Your Transaction

Ask the transaction provider for the connection and the transaction rather than opening your own. That shared pair is what makes two writes one atomic act:

```csharp
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public class AddGreetingHandlerAsync : RequestHandlerAsync<AddGreeting>
{
    private readonly IAmATransactionConnectionProvider _transactionProvider;
    private readonly IAmACommandProcessor _postBox;

    public AddGreetingHandlerAsync(
        IAmATransactionConnectionProvider transactionProvider,
        IAmACommandProcessor postBox)
    {
        _transactionProvider = transactionProvider;
        _postBox = postBox;
    }

    public override async Task<AddGreeting> HandleAsync(
        AddGreeting addGreeting,
        CancellationToken cancellationToken = default)
    {
        DbConnection connection = await _transactionProvider.GetConnectionAsync(cancellationToken);
        DbTransaction transaction = await _transactionProvider.GetTransactionAsync(cancellationToken);

        try
        {
            // 1. Your write, to your table.
            await using (DbCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "insert into Greeting (Message) values (@message)";

                DbParameter message = command.CreateParameter();
                message.ParameterName = "message";
                message.Value = addGreeting.Greeting;
                command.Parameters.Add(message);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // 2. Brighter's write, to the Outbox, on that same transaction. Nothing has
            // reached the queue store table yet.
            await _postBox.DepositPostAsync(
                new GreetingEvent(addGreeting.Greeting),
                _transactionProvider,
                cancellationToken: cancellationToken);

            // 3. Both, or neither.
            await _transactionProvider.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _transactionProvider.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transactionProvider.Close();
        }

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
```

There is no `ClearOutboxAsync` call here on purpose: the message waits in the Outbox for the Sweeper. Call `ClearOutboxAsync` after the commit instead if you would rather dispatch immediately, at the cost of doing the send on the request thread.

## Step 9: Provision the Boxes and Run the Sweeper

The Sweeper is a hosted service that wakes on a timer, finds undispatched messages and sends them — here, into the queue store table in the same database. Chain both calls off the registration in step 5, and provision the Inbox alongside the Outbox:

```csharp
using System;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.MsSql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.Hosting;

builder.Services
    .AddBrighter()
    .AddProducers(configure =>
    {
        // ... as in step 5
    })
    .AutoFromAssemblies()

    // Creates and migrates both tables at startup. Needs rights to CREATE TABLE.
    .UseBoxProvisioning(options =>
    {
        options.AddMsSqlOutbox(configuration);
        options.AddMsSqlInbox(configuration);
    })

    .UseOutboxSweeper(options =>
    {
        options.TimerInterval = 5;
        options.MinimumMessageAge = TimeSpan.FromSeconds(5);
    });
```

Both Sweeper values above are the defaults. Together they mean a message is picked up on the first tick after it is five seconds old, so expect a five to ten second delay before it appears on the queue.

**Provisioning covers the Outbox and the Inbox and not the queue.** There is no `AddMsSqlQueue`, for the reason in step 2 — that table is yours to create.

Running more than one instance? Configure a [distributed lock](/contents/MsSqlDistributedLock.md) so only one Sweeper runs at a time.

## Step 10: Verify It Worked

Send one message, then look at the three tables.

Immediately after the send, the Outbox holds the message and the queue does not:

```sql
select MessageId, Topic, case when Dispatched is null then 0 else 1 end as Dispatched from Outbox;
select Id, Topic from QueueData;
```

Five to ten seconds later the Sweeper has dispatched it, and the same two queries show the message dispatched *and* sitting on the queue:

```text
MessageId                            Topic           Dispatched
------------------------------------ --------------- ----------
01a07aff-20b1-722b-a12a-c5ecb3c466f9 greeting.event  1

Id  Topic
--- ---------------
1   greeting.event
```

Start the consumer and the row leaves the queue table. Send the same message again and the Inbox is what you watch instead:

```sql
select CommandId, CommandType, ContextKey, Timestamp from InboxMessages;
```

`ContextKey` is what scopes a message to a handler. It is generated from the handler's class name unless you pass a `context` function to `InboxConfiguration`, which is why two different handlers can each record the same `CommandId` without either seeing the other's row.

A second delivery of the same message id adds no row and, with `onceOnlyAction: OnceOnlyAction.Warn`, logs rather than throws — the handler does not run twice. Measured by replaying an identical row onto the queue:

```text
delivery 1, message id X : handler ran 1 time
delivery 2, message id X : handler ran 0 times
  warn: Paramore.Brighter.Inbox.Handlers.UseInboxHandler
        Command 01a08aef-c8a4-738e-ad13-317ce5686212 has already been seen
InboxMessages rows after both : 1
```

**The check that matters is the failure case**, because it is the reason for all of this. Throw inside the handler between the two writes and the commit, and both disappear together — the row counts in your table and in `Outbox` are unchanged, and nothing reaches the queue.

## MSSQL Transport, Outbox and Inbox Failures

**`Invalid object name 'QueueData'`**

The queue table does not exist, and nothing was ever going to create it. `OnMissingChannel.Create` is inert on this transport. Run the DDL from step 2.

**`Paramore.Brighter.ConfigurationException: MS SQL ChannelFactory We expect an MsSqlSubscription or MsSqlSubscription<T> as a parameter`**

A `Subscription<T>` where an `MsSqlSubscription<T>` was needed. It compiles; it fails when the Dispatcher builds its channels. Step 6.

**`Unable to register outbox of type MsSqlOutbox - no transaction provider has been registered that matches the outbox's transaction type`**

A `ConfigurationException`, thrown by `AddProducers` while your application is still starting. Brighter takes the transaction type from `TransactionProvider`, falling back to `InMemoryTransactionProvider` when you do not set one, and then checks that the Outbox you supplied implements the Outbox interfaces for *that* transaction type. Set both `ConnectionProvider` and `TransactionProvider` as step 5 shows.

**`Unable to resolve service for type 'Paramore.Brighter.IAmARelationalDatabaseConfiguration' while attempting to activate 'Paramore.Brighter.MsSql.MsSqlTransactionProvider'`**

You skipped step 4. What makes this one expensive is how healthy everything looks first: the host starts, both boxes are provisioned, and the exception arrives only on the first attempt to resolve a command processor, naming a type your code never mentions.

**Every message is handled twice, and the Inbox table stays empty**

The handler has no `[UseInbox]`, or the process registers no producers and you were relying on `InboxConfiguration`'s policy arguments — which never reach the pipeline without an external bus. Step 7. Setting `scope:` will not help: `InboxScope` is inert.

**Every message is handled twice, and the Inbox table has rows**

`onceOnly` defaulted to `false` on the attribute, so the Inbox is recording without suppressing. Set `onceOnly: true`.

**`The type or namespace name 'MsSqlOutboxBuilder' could not be found`**

There is no such type. It is `SqlOutboxBuilder`, and the Inbox one is `SqlInboxBuilder` — MSSQL is the only provider whose DDL builders carry no prefix. Step 2.

## Further Reading

- [Use PostgreSQL for Both Transport and Outbox](/contents/PostgreSQLTransportAndOutbox.md) — the same composition on PostgreSQL, and the argument for doing it at all
- [MSSQL Message Broker](/contents/MSSQLMessageBroker.md) — the transport on its own, with every subscription option
- [MSSQL Outbox](/contents/MSSQLOutbox.md) — the Outbox on its own, and its DDL
- [MSSQL Inbox](/contents/MSSQLInbox.md) — the Inbox on its own
- [Brighter Inbox Support](/contents/BrighterInboxSupport.md) — the `[UseInbox]` attribute, and Inbox behaviour in general
- [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md) — every option on the configuration object step 3 builds
- [Outbox Pattern](/contents/OutboxPattern.md) — why an Outbox, and what it does and does not guarantee
- [Box Provisioning](/contents/BoxProvisioning.md) — startup provisioning and migration for the Outbox and Inbox tables
- [MSSQL Distributed Lock](/contents/MsSqlDistributedLock.md) — required once more than one instance runs a Sweeper
