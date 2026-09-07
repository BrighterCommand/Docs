---
description: "One PostgreSQL database can be both your message broker and your Outbox, so the business write and the message announcing it commit in a single transaction."
layout:
  description:
    visible: false
---

# Use PostgreSQL for Both Transport and Outbox

> **How-to** · Applies to **Brighter V10** · Prerequisites: [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md), [PostgreSQL Outbox](/contents/PostgresOutbox.md)

One PostgreSQL database can be both your message broker and your Outbox, so the business write and the message announcing it commit in a single transaction.

That is the whole reason to compose them. With a separate broker you write your row, commit, then send — and a crash in between loses the message. Put the queue store and the [Outbox](/contents/OutboxPattern.md) in the database you were already writing to and there is no "in between": one transaction covers your row and the message, and the Sweeper moves the message onto the queue afterwards.

This guide assumes you have read the two pages in the banner above. It adds the part neither of them covers on its own — running both against one database.

A working version of everything below is in the Brighter repository at
`Brighter/samples/TaskQueue/PostgresTaskQueue/GreetingsSenderWithOutbox/`, with the
consumer beside it in `GreetingsReceiverConsole/`.

## Step 1: Install the Packages

The transport, the Outbox, the transaction provider, the Sweeper and the provisioner are separate packages:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.Postgres
dotnet add package Paramore.Brighter.Outbox.PostgreSql
dotnet add package Paramore.Brighter.PostgreSql
dotnet add package Paramore.Brighter.Outbox.Hosting
dotnet add package Paramore.Brighter.BoxProvisioning.PostgreSql
```

`Paramore.Brighter.PostgreSql` is the one people miss. It holds `PostgreSqlConnectionProvider` and `PostgreSqlTransactionProvider`, which are what let your handler and the Outbox share a transaction — without it there is no composition, only two subsystems pointed at the same host.

## Step 2: Create the Queue and Outbox Tables

You need two Brighter tables plus your own. Brighter can create both of its own, by two different routes, and you can also drive the DDL yourself.

**The queue store table** is created by the transport when a publication or subscription says `MakeChannels = OnMissingChannel.Create`. To manage it yourself, use `OnMissingChannel.Validate` and create it first — this is the DDL the transport itself runs, with `JSONB` in place of `JSON` when the payload is binary:

```sql
CREATE TABLE IF NOT EXISTS "public"."Queue"
(
    "id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "visible_timeout" TIMESTAMPTZ,
    "queue" VARCHAR(255),
    "content" JSON
);

CREATE INDEX IF NOT EXISTS "public_Queue_queue_visible_timeout_idx"
    ON "public"."Queue"("queue", "visible_timeout") INCLUDE ("id");
```

**The Outbox table** is created and migrated by [Box Provisioning](/contents/BoxProvisioning.md) at startup, which is the call in step 8. To manage it yourself instead, ask Brighter for the same DDL and run it through your own tooling:

```csharp
using Paramore.Brighter.Outbox.PostgreSql;

// The DDL for a table that stores the message body as TEXT
string ddl = PostgreSqlOutboxBuilder.GetDDL("Outbox");

// Pass binaryMessagePayload: true for a BYTEA body
string binaryDdl = PostgreSqlOutboxBuilder.GetDDL("Outbox", binaryMessagePayload: true);
```

**The two tables do not agree about capitals, and this will bite you at a `psql` prompt.** The transport quotes the configured name as you wrote it, so `queueStoreTable: "Queue"` becomes a table called `Queue`. The Outbox lowercases the name and *then* quotes it, so `outBoxTableName: "Outbox"` becomes `outbox` — deliberately, so that a configured `"Outbox"` still matches the table older Brighter versions created unquoted. The consequence is that `select * from "Queue"` works and `select * from "Outbox"` returns `relation "Outbox" does not exist`. Query the Outbox unquoted, or as `"outbox"`.

Your own tables are yours. Brighter does not create, migrate or know about them.

## Step 3: Describe Both Tables in One Configuration

This is the pivot the whole guide turns on. The queue store, the Outbox and the Inbox are three parameters on **one** object, so there is no second configuration to keep in step:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;

const string connectionString =
    "Host=localhost;Port=5432;Username=postgres;Password=password;Database=brightertests";

// One object, both tables. Both names below are the defaults; naming them is the point.
var configuration = new RelationalDatabaseConfiguration(
    connectionString,
    outBoxTableName: "Outbox",
    queueStoreTable: "Queue");

// The transport takes the same object, wrapped. PostgresMessagingGatewayConnection is a
// holder and adds no settings of its own.
var connection = new PostgresMessagingGatewayConnection(configuration);
```

For every option this type carries — including `schemaName`, `inboxTableName` and the payload flags — see [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md#relational-database-configuration-options).

**One flag means two things, because two subsystems read it.** `binaryMessagePayload` tells the transport to store the queue's `content` column as `JSONB` rather than `JSON`, and tells the Outbox to store its `Body` column as `bytea` rather than `text`. Sharing the object shares the flag. If you want JSONB on the queue without moving the Outbox to `bytea`, set it per publication and per subscription instead — `PostgresPublication.BinaryMessagePayload` and `PostgresSubscription.BinaryMessagePayload` are both nullable and both override the shared value.

## Step 4: Register the Configuration

Put the same object in the container:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

builder.Services.AddSingleton<IAmARelationalDatabaseConfiguration>(configuration);
```

**This line is easy to leave out and the failure lands nowhere near it.** `TransactionProvider` in the next step is given as a *type*, so the container activates `PostgreSqlTransactionProvider`, and its constructor asks for exactly this interface. Omit the registration and your application starts, provisions the Outbox, and only then throws — see [Failures](#postgresql-transport-and-outbox-failures).

## Step 5: Wire the Producer and the Outbox

The publication says where the event goes; the three lines after `ProducerRegistry` are what make the Outbox durable and transactional:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.PostgreSql;

var producerRegistry = new PostgresProducerRegistryFactory(
    connection,
    [
        new PostgresPublication<GreetingEvent>
        {
            Topic = new RoutingKey("greeting.event"),
            MakeChannels = OnMissingChannel.Create
        }
    ]).Create();

builder.Services
    .AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = producerRegistry;

        configure.Outbox = new PostgreSqlOutbox(configuration);
        configure.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        configure.TransactionProvider = typeof(PostgreSqlTransactionProvider);
    })
    .AutoFromAssemblies();
```

`PostgresProducerRegistryFactory` takes the `PostgresMessagingGatewayConnection` from step 3, not the `RelationalDatabaseConfiguration` inside it.

**The transaction provider is not optional here.** It is what fixes the transaction type Brighter matches the Outbox against, and `PostgreSqlOutbox` only satisfies that match when the provider is PostgreSQL's. Leave it out and registration itself fails, again in [Failures](#postgresql-transport-and-outbox-failures).

## Step 6: Wire the Consumer

The consumer reads from the same queue store table. Note the order: `AddConsumers` extends `IServiceCollection`, while `AddProducers` extends the builder it returns, so a consumer registration comes first and everything else chains off it.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var subscriptions = new Subscription[]
{
    new PostgresSubscription<GreetingEvent>(
        new SubscriptionName("paramore.example.greeting"),
        new ChannelName("greeting.event"),
        new RoutingKey("greeting.event"),
        timeOut: TimeSpan.FromMilliseconds(2000),
        messagePumpType: MessagePumpType.Reactor,
        makeChannels: OnMissingChannel.Create)
};

builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new PostgresChannelFactory(connection);
    })
    .AutoFromAssemblies();

builder.Services.AddHostedService<ServiceActivatorHostedService>();
```

**Use `PostgresSubscription<T>`, not `Subscription<T>`.** `PostgresChannelFactory` casts what it is given down to `PostgresSubscription` and throws `ConfigurationException` if the cast fails — a plain `Subscription<T>` compiles and then dies when the Dispatcher starts reading.

## Step 7: Deposit and Clear Inside Your Transaction

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

## Step 8: Run the Outbox Sweeper

The Sweeper is a hosted service that wakes on a timer, finds undispatched messages and sends them — here, into the queue store table in the same database. Chain both calls off the registration in step 5:

```csharp
using System;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.PostgreSql;
using Paramore.Brighter.Outbox.Hosting;

builder.Services
    .AddBrighter()
    .AddProducers(configure =>
    {
        // ... as in step 5
    })
    .AutoFromAssemblies()

    // Creates and migrates the Outbox table at startup. Needs rights to CREATE TABLE.
    .UseBoxProvisioning(options => options.AddPostgreSqlOutbox(configuration))

    .UseOutboxSweeper(options =>
    {
        options.TimerInterval = 5;
        options.MinimumMessageAge = TimeSpan.FromSeconds(5);
    });
```

Both Sweeper values above are the defaults. Together they mean a message is picked up on the first tick after it is five seconds old, so expect a five to ten second delay before it appears on the queue.

Running more than one instance? Configure a [distributed lock](/contents/PostgresDistributedLock.md) so only one Sweeper runs at a time.

## Step 9: Verify It Worked

Send one message, then look at the three tables. The output below is from a real run of the companion sample.

Immediately after the send, the Outbox holds the message and the queue does not:

```sql
select messageid, topic, dispatched is not null as dispatched from outbox;
select id, queue from "Queue";
```

Five to ten seconds later the Sweeper has dispatched it, and the sender logs both halves:

```text
info: Paramore.Brighter.CommandProcessor[1620710603]
      Found 1 to clear out of amount 100
info: Paramore.Brighter.CommandProcessor[1310740404]
      Decoupled invocation of message: Topic:greeting.event Id:01a07aff-20b1-722b-a12a-c5ecb3c466f9
```

Now the same two queries show the message dispatched *and* sitting on the queue:

```text
              messageid               |     topic      | dispatched
--------------------------------------+----------------+------------
 01a07aff-20b1-722b-a12a-c5ecb3c466f9 | greeting.event | t

 id |     queue
----+----------------
  1 | greeting.event
```

Start the consumer and the row leaves the queue table:

```text
Received Greeting. Message Follows
Hello from the sender
info: Paramore.Brighter.MessagingGateway.Postgres.PostgresMessageConsumer[1174086769]
      PostgresPullMessageConsumer: Deleted the message 01a07aff-20b1-722b-a12a-c5ecb3c466f9 with receipt handle 1 on the queue greeting.event
```

**The check that matters is the failure case**, because it is the reason for all of this. Throw inside the handler between the two writes and the commit, and both disappear together — the row counts in your table and in `outbox` are unchanged, and nothing reaches the queue.

## PostgreSQL Transport and Outbox Failures

Two mistakes account for most of the traffic on this composition, and neither error message names the line you need to change.

**`Unable to register outbox of type PostgreSqlOutbox - no transaction provider has been registered that matches the outbox's transaction type`**

A `ConfigurationException`, thrown by `AddProducers` while your application is still starting. Brighter takes the transaction type from `TransactionProvider`, falling back to `InMemoryTransactionProvider` when you do not set one, and then checks that the Outbox you supplied actually implements the Outbox interfaces for *that* transaction type. `PostgreSqlOutbox` does not implement them for the in-memory transaction. Set both `ConnectionProvider` and `TransactionProvider` as step 5 shows.

Older Brighter versions had no such check, and the mismatch surfaced much later as `InvalidOperationException: No Async outbox defined.` from the Sweeper. If you find that message in a search result, this registration guard is its modern equivalent — on V10 you will meet the `ConfigurationException` first.

**`Unable to resolve service for type 'Paramore.Brighter.IAmARelationalDatabaseConfiguration' while attempting to activate 'Paramore.Brighter.PostgreSql.PostgreSqlTransactionProvider'`**

You skipped step 4. What makes this one expensive is how healthy everything looks first: the host starts, the Outbox is provisioned — you will see `Provisioned Outbox 'Outbox' successfully` — and the exception arrives only on the first attempt to resolve a command processor, naming a type your code never mentions. Add the `AddSingleton<IAmARelationalDatabaseConfiguration>` line from step 4.

**`relation "Outbox" does not exist`**

Not a wiring fault at all — the Outbox table is `outbox`, in lower case, for the reason in step 2. Your messages are there.

## Further Reading

- [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) — the transport on its own, with every subscription and publication option
- [PostgreSQL Outbox](/contents/PostgresOutbox.md) — the Outbox on its own, including the Entity Framework Core provider
- [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md) — every option on the configuration object step 3 builds
- [Outbox Pattern](/contents/OutboxPattern.md) — why an Outbox, and what it does and does not guarantee
- [Box Provisioning](/contents/BoxProvisioning.md) — startup provisioning and migration for the Outbox table
- [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md) — when a table-based queue is the wrong answer
- [Postgres Distributed Lock](/contents/PostgresDistributedLock.md) — required once more than one instance runs a Sweeper
