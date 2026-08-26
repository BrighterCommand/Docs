---
description: "Write your business data and the message announcing it in one Postgres transaction, then let the Sweeper deliver the message once that transaction has committed."
layout:
  description:
    visible: false
---

# Adding a Durable Outbox

> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Message Over a Broker](/contents/TutorialFirstMessage.md)

Write your business data and the message announcing it in one Postgres transaction, then let
the [Sweeper](/contents/Glossary.md#sweeper) deliver the message once that transaction has
committed.

This is the third rung of the ladder. Rung 2's sender called `Post` and hoped: if the process
had died between writing its data and reaching the broker, the data would exist and the
announcement would not, permanently. Here the message is written to a database table — the
[Outbox](/contents/Glossary.md#outbox) — inside the *same transaction* as your own row, so the
two commit together or not at all. A background service sends it afterwards.

The cost is a delay of a few seconds before the message goes out. What you buy with it is that
your data and your messages can no longer disagree.

## What You'll Build: A Durable Outbox

Rung 2's three projects, unchanged except for the sender, plus Postgres alongside RabbitMQ:

| What | Change from rung 2 |
|---|---|
| `Greetings` | unchanged |
| `GreetingsReceiver` | unchanged — it does not know or care how the message was sent |
| `GreetingsSender` | a command and a handler, a Postgres Outbox, a `Greeting` table of its own, and the Sweeper hosted in-process |

The sender prints `Committed.` at once and then keeps running. About ten seconds later the
receiver prints `Received: Hello from the sender`. **That gap is the whole point:** the send is
no longer on the request path.

Then you run it again with `--fail`, which throws after both writes and before the commit, and
find neither of them in the database.

## Before You Start the Durable Outbox

- **Rung 2 complete.** [Your First Message Over a Broker](/contents/TutorialFirstMessage.md)
  built the three projects this rung starts from, and only the sender changes here. Work in
  that solution; this page does not rebuild it.
- **The .NET 9 SDK.** Check with `dotnet --version`.
- **Docker Desktop**, running, with port **5432** free as well as rung 2's **5672** and
  **15672**.
- **A Postgres client.** Everything below uses `psql` inside the container, so you need
  nothing installed.
- **About twenty-five minutes**, most of it reading. The machine work measured **9.2 seconds**
  to add the six package references and **1.3 seconds** to build, starting from a rung 2
  solution and a NuGet cache holding only rung 2's packages. Pulling the `postgres` image the
  first time takes longer and depends on your connection.

## Step 1: Start Postgres

Rung 2's `docker-compose.yml` gains a second service. Replace the file with this:

```yaml
services:
  rabbitmq:
    image: rabbitmq:management
    hostname: rabbitmq-server
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  postgres:
    image: postgres
    ports:
      - "5432:5432"
    environment:
      POSTGRES_PASSWORD: password
      POSTGRES_DB: brightertests
```

```bash
docker compose up -d
```

**Expected result:** `docker ps` shows two containers running.

> **Neither service has a healthcheck**, so that command returns before either accepts
> connections. RabbitMQ is ready when <http://localhost:15672> answers. Postgres is ready when
> this prints a row:
>
> ```bash
> docker compose exec postgres psql -U postgres -d brightertests -c 'select 1'
> ```

## Step 2: Add the Packages

All six go to the sender. The receiver and the shared library do not change.

```bash
dotnet add GreetingsSender package Paramore.Brighter.Outbox.PostgreSql --version 10.7.0
dotnet add GreetingsSender package Paramore.Brighter.PostgreSql --version 10.7.0
dotnet add GreetingsSender package Paramore.Brighter.BoxProvisioning --version 10.7.0
dotnet add GreetingsSender package Paramore.Brighter.BoxProvisioning.PostgreSql --version 10.7.0
dotnet add GreetingsSender package Paramore.Brighter.Outbox.Hosting --version 10.7.0
dotnet add GreetingsSender package Npgsql --version 10.0.2
```

Five of the six are Brighter's. Two are worth pausing on:

- **`Paramore.Brighter.Outbox.Hosting`** is where `UseOutboxSweeper` lives. The Sweeper is the
  feature this rung is named after, and it ships in its own package rather than in the Outbox
  one.
- **`Npgsql`** is not a Brighter package at all. You need it because *you* are about to talk to
  Postgres directly — the `Greeting` table is yours, and creating it is your code's job. See
  step 3.

## Step 3: Create the Greeting Table

**Two tables will live in this database and they have different owners.** Brighter's
provisioning creates the **Outbox** table and nothing else. `Greeting` is your table: your
schema, your migrations, your problem. A reader who conflates the two goes looking for a
schema-management feature that Brighter does not have.

Your table is as small as a table gets:

```sql
create table if not exists Greeting (
    Id      serial primary key,
    Message text not null
)
```

The sample runs that in plain ADO.NET at the bottom of `Program.cs`, deliberately not through
Brighter, because it is not Brighter's table. You will see it in the next step.

## Step 4: Configure the Outbox

Replace `GreetingsSender/Program.cs`:

```csharp
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Greetings;
using GreetingsSender;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Paramore.Brighter;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.PostgreSql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.PostgreSql;

const string connectionString =
    "Host=localhost;Port=5432;Username=postgres;Password=password;Database=brightertests";

// Two tables live in this database and they have different owners. Greeting is yours: your
// schema, your migrations, your problem, and the line below is the whole of this sample's
// answer to that. The Outbox table is Brighter's, and UseBoxProvisioning creates it further
// down. Do not let the two blur — Brighter does not manage your schema.
await CreateGreetingTableAsync(connectionString);

// The Outbox and the provisioner both need to know where the database is and what the table
// is called. RelationalDatabaseConfiguration defaults the name to "Outbox".
var outboxConfiguration = new RelationalDatabaseConfiguration(connectionString);

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange")
};

// Unchanged from rung 2: the publication says where GreetingEvent goes. Naming the type here
// is also what loads the Greetings assembly, which AutoFromAssemblies below needs to have
// happened already — moving this line beneath the registration is a silent no-op.
var producerRegistry = new RmqProducerRegistryFactory(
    rmqConnection,
    [
        new RmqPublication<GreetingEvent>
        {
            Topic = new RoutingKey("greeting.event"),
            MakeChannels = OnMissingChannel.Create
        }
    ]).Create();

var builder = Host.CreateApplicationBuilder(args);

// The configuration goes into the container as well as into the two calls below, and this
// line is easy to leave out. TransactionProvider is given as a *type*, so the container
// activates PostgreSqlTransactionProvider, and its constructor asks for exactly this
// interface. Without it the host starts, provisions the Outbox and only then fails, on the
// first attempt to resolve a command processor.
builder.Services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);

builder.Services
    .AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = producerRegistry;

        // Rung 2 had none of these three lines and got an in-memory Outbox by default: good
        // enough to make Post work, gone the moment the process is. These three make it
        // durable. The transaction provider is the important one — it is what lets the
        // handler hand Brighter a transaction that the handler itself opened.
        configure.Outbox = new PostgreSqlOutbox(outboxConfiguration);
        configure.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        configure.TransactionProvider = typeof(PostgreSqlTransactionProvider);
    })
    .AutoFromAssemblies()

    // Creates and migrates the Outbox table at startup, before anything else runs. It owns
    // that table and nothing else; the Greeting table above was yours to create. This needs
    // rights to CREATE TABLE, which the Docker Postgres has and your production database
    // very likely does not — see the Box Provisioning page for the alternative.
    .UseBoxProvisioning(options => options.AddPostgreSqlOutbox(outboxConfiguration))

    // The Sweeper: a hosted service that wakes on a timer, finds undispatched messages in the
    // Outbox and sends them. Both values below are the defaults, spelled out because the
    // delay they produce is the thing this rung teaches — a message is picked up on the first
    // tick after it is MinimumMessageAge old, so expect five to ten seconds.
    .UseOutboxSweeper(options =>
    {
        options.TimerInterval = 5;
        options.MinimumMessageAge = TimeSpan.FromSeconds(5);
    });

var host = builder.Build();

// StartAsync rather than RunAsync, because we have work to do between starting the host and
// waiting on it. Starting is what provisions the Outbox table and starts the Sweeper, so it
// has to happen before the send rather than after.
await host.StartAsync();

var commandProcessor = host.Services.GetRequiredService<IAmACommandProcessor>();
var failBeforeCommit = args.Contains("--fail");

// The failing run says something different so you can prove it is absent afterwards rather
// than counting rows: a table that still holds one greeting is only interesting if you can
// see which greeting it is.
var greeting = failBeforeCommit ? "This greeting will not survive" : "Hello from the sender";

try
{
    await commandProcessor.SendAsync(new AddGreeting(greeting, failBeforeCommit));

    Console.WriteLine("Committed. The greeting and the message are both in Postgres.");
    Console.WriteLine("Waiting for the Sweeper to dispatch it. Ctrl+C to stop.");
}
catch (Exception e)
{
    Console.WriteLine($"Rolled back: {e.Message}");
    Console.WriteLine("Neither the greeting nor the message was written. Ctrl+C to stop.");
}

await host.WaitForShutdownAsync();

// Your table, created by your code. Plain ADO.NET: this is deliberately not going through
// Brighter, because it is not Brighter's table.
static async Task CreateGreetingTableAsync(string connection)
{
    await using var postgres = new NpgsqlConnection(connection);
    await postgres.OpenAsync();

    await using DbCommand command = postgres.CreateCommand();
    command.CommandText =
        """
        create table if not exists Greeting (
            Id      serial primary key,
            Message text not null
        )
        """;

    await command.ExecuteNonQueryAsync();
}
```

That will not compile yet — `AddGreeting` and its handler arrive in step 5. What is new since
rung 2:

- **Three lines on `AddProducers`.** Rung 2 set only `ProducerRegistry` and got the default
  in-memory Outbox: enough to make `Post` work, gone the moment the process is. `Outbox`,
  `ConnectionProvider` and `TransactionProvider` replace it with Postgres. The transaction
  provider is the important one — it is what lets your handler hand Brighter a transaction that
  the *handler* opened.
- **`AddSingleton<IAmARelationalDatabaseConfiguration>`, which is easy to leave out.**
  `TransactionProvider` is given as a **type**, not an instance, so the container activates
  `PostgreSqlTransactionProvider` and its constructor asks for that interface. Without the
  registration the host starts happily, provisions the Outbox, logs the Sweeper ticking, and
  only then throws — on the first attempt to resolve a command processor, naming a type your
  code never mentions.
- **`UseBoxProvisioning`** creates and migrates the **Outbox** table at startup, which is why
  there is no migration project and no second terminal here. It needs rights to `CREATE TABLE`
  and `ALTER TABLE`. The Docker Postgres above has them; a production database very often does
  not, and [Box Provisioning](/contents/Glossary.md#boxprovisioning) is one of two options for
  exactly that reason — see *Further Reading*.
- **`UseOutboxSweeper`** hosts the [Sweeper](/contents/Glossary.md#sweeper) in this process. It
  is an `IHostedService`, which is why the sender now runs a host instead of building one and
  throwing it away as rung 2 did.
- **`StartAsync`, not `RunAsync`.** There is work to do between starting the host and waiting
  on it, and starting is what provisions the table and starts the Sweeper — so it has to happen
  before the send rather than after.

Both Sweeper values are the defaults, spelled out because the delay they produce is the thing
this rung teaches: a message is picked up on the first tick that finds it at least
`MinimumMessageAge` old.

## Step 5: Write and Deposit in One Transaction

Rung 2's sender called `Post` straight from `Main`. A transaction needs somewhere to live, and
a [handler](/contents/Glossary.md#handler) is where Brighter puts one — so the work moves
behind a [command](/contents/Glossary.md#command).

Put this in `GreetingsSender/AddGreeting.cs`:

```csharp
using Paramore.Brighter;

namespace GreetingsSender;

/// <summary>
/// The instruction to record a greeting and tell the world about it. Unlike
/// <see cref="Greetings.GreetingEvent"/> this never leaves the process, so it lives here in
/// the sender rather than in the shared Greetings library: a command is addressed to one
/// handler, and that handler is in this assembly.
/// </summary>
public class AddGreeting : Command
{
    public AddGreeting(string greeting, bool failBeforeCommit = false) : base(Id.Random())
    {
        Greeting = greeting;
        FailBeforeCommit = failBeforeCommit;
    }

    public string Greeting { get; }

    /// <summary>
    /// Set by the <c>--fail</c> switch. It exists so the tutorial can run the unhappy path:
    /// the handler throws after the row and the message are both written and before the
    /// commit, which is the only interesting moment in the whole sample.
    /// </summary>
    public bool FailBeforeCommit { get; }
}
```

And this in `GreetingsSender/AddGreetingHandlerAsync.cs`:

```csharp
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Greetings;
using Paramore.Brighter;

namespace GreetingsSender;

/// <summary>
/// The whole point of rung 3. The business row and the outgoing message are written on the
/// same connection inside the same transaction, so the database commits both or neither.
/// </summary>
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
        // Ask the provider for the connection and transaction rather than opening your own.
        // This is the shared unit of work: the Outbox writes through the same pair, which is
        // what makes the two writes below one atomic act instead of two hopeful ones.
        DbConnection connection = await _transactionProvider.GetConnectionAsync(cancellationToken);
        DbTransaction transaction = await _transactionProvider.GetTransactionAsync(cancellationToken);

        try
        {
            // 1. Your write, to your table. Brighter neither knows nor cares what this is.
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

            // 2. Brighter's write, to the Outbox table, on that same transaction. Nothing has
            // gone to RabbitMQ yet — DepositPostAsync only stores the message.
            await _postBox.DepositPostAsync(
                new GreetingEvent(addGreeting.Greeting),
                _transactionProvider,
                cancellationToken: cancellationToken);

            // The deliberate failure the tutorial's last step runs. Both writes are pending
            // right now; neither is committed. Throwing here is the experiment.
            if (addGreeting.FailBeforeCommit)
            {
                throw new InvalidOperationException(
                    "Deliberate failure, after both writes and before the commit");
            }

            // 3. Both, or neither.
            await _transactionProvider.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Rolling back discards your row and the message together. There is no window in
            // which the greeting exists but the message does not, or the other way round.
            await _transactionProvider.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transactionProvider.Close();
        }

        // Note what is NOT here: ClearOutboxAsync. The message sits in the Outbox until the
        // Sweeper picks it up, which is the delay this rung exists to show you. Call
        // ClearOutboxAsync here instead and it dispatches immediately, at the cost of doing
        // the send on the request thread.
        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
```

Read the middle of that handler as three numbered acts, because that is the entire argument of
this rung:

1. **Your write**, to your table, on a connection and transaction you asked the *provider* for
   rather than opening yourself.
2. **Brighter's write**, to the Outbox table, on that same transaction. `DepositPostAsync` only
   stores the message; nothing has gone to RabbitMQ.
3. **Commit** — and both land, or the `catch` rolls back and neither does.

There is no window in which the greeting exists and the message does not, or the other way
round. That is not achieved by retrying, or by being careful; it is achieved by there being
only one write as far as the database is concerned.

> **The absent call the comment flags — `ClearOutboxAsync` — is what most production code does.**
> Called after the commit, it dispatches on the spot and the ten-second wait below never
> happens; the transactional guarantee is unaffected either way, because the row is already
> committed by then. This page leaves it out so that the Outbox is visible as a thing with
> contents rather than a formality. See
> [Transactional Messaging with the Outbox](/contents/TransactionalMessagingWithTheOutbox.md)
> for the version you would ship.

## Step 6: Run It

Three terminals this time: one for each app, and a third for the database — the sender no
longer exits, because it is hosting the Sweeper. **The receiver first**, as before; it is
still the process that declares the queue and the binding:

```bash
dotnet run --project GreetingsReceiver
```

```bash
dotnet run --project GreetingsSender
```

**Expected result** — the sender, which commits at once and then stays up:

```text
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioning Outbox 'Outbox'...
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioned Outbox 'Outbox' successfully
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Paramore.Brighter.CommandProcessor[1620710603]
      Found 0 to clear out of amount 100
info: Paramore.Brighter.CommandProcessor[1552112121]
      Save request: Greetings.GreetingEvent 01a03dfa-1542-7196-8db0-3dea053ae393
Committed. The greeting and the message are both in Postgres.
Waiting for the Sweeper to dispatch it. Ctrl+C to stop.
info: Paramore.Brighter.CommandProcessor[1620710603]
      Found 1 to clear out of amount 100
info: Paramore.Brighter.CommandProcessor[1310740404]
      Decoupled invocation of message: Topic:greeting.event Id:01a03dfa-1542-7196-8db0-3dea053ae393
info: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessageProducer[1379693184]
      Published message: 01a03dfa-1542-7196-8db0-3dea053ae393
```

And the receiver, about ten seconds later:

```text
info: Paramore.Brighter.CommandProcessor[258521805]
      Found 1 pipelines for event: Greetings.GreetingEvent 01a03dfa-1542-7196-8db0-3dea053ae393
Received: Hello from the sender
info: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessageConsumer[22364147]
      RmqMessageConsumer: Acknowledging message 01a03dfa-1542-7196-8db0-3dea053ae393 as completed with delivery tag 1
```

Both listings are trimmed; identifiers and timings differ on every run.

`Found 0 to clear` and then `Found 1 to clear` are the Sweeper's ticks: it woke, looked, found
nothing old enough, and on a later pass found your message and sent it. Those two lines are the
only place the delay is visible in the log.

**Ten seconds is a long time for a message.** It is also the only thing you gave up. While you
wait, look at the two tables:

```bash
docker compose exec postgres psql -U postgres -d brightertests \
  -c 'select * from Greeting;' \
  -c 'select messageid, topic, dispatched from Outbox;'
```

```text
 id |        message
----+-----------------------
  1 | Hello from the sender
(1 row)

              messageid               |     topic      | dispatched
--------------------------------------+----------------+------------
 01a03dfa-1542-7196-8db0-3dea053ae393 | greeting.event |
(1 row)
```

**`dispatched` is null.** That single column is the durable Outbox doing its job: the message is
committed, it is not yet sent, and nothing about it depends on this process staying alive. Run
the same query a few seconds later and it carries a timestamp.

You do not have to time it by hand — the row records both moments:

```bash
docker compose exec postgres psql -U postgres -d brightertests \
  -c 'select timestamp, dispatched, dispatched - timestamp as sweep_delay from Outbox;'
```

```text
           timestamp           |          dispatched           |   sweep_delay
-------------------------------+-------------------------------+-----------------
 2026-08-26 12:09:54.761362+00 | 2026-08-26 12:10:04.796541+00 | 00:00:10.035179
(1 row)
```

`TimerInterval` and `MinimumMessageAge` are five seconds each, so a message waits out its
minimum age and is then caught by the next tick: **about ten seconds**, give or take where in
the cycle it arrived.

## Step 7: Make It Fail

This is the step the page exists for. Stop the sender and run it again with `--fail`:

```bash
dotnet run --project GreetingsSender -- --fail
```

The handler writes the greeting, deposits the message, and throws before the commit. The
greeting it writes says something different on purpose, so you can look for its *absence*
rather than counting rows:

```text
info: Paramore.Brighter.CommandProcessor[1552112121]
      Save request: Greetings.GreetingEvent 01a03dfa-a8d6-7398-abe9-0242392c165c
Rolled back: Deliberate failure, after both writes and before the commit
Neither the greeting nor the message was written. Ctrl+C to stop.
```

**`Save request` is in that output, and the message was still not saved.** `DepositPostAsync`
really did write the Outbox row — inside a transaction that then rolled back. The log records
what your code asked for; the table records what survived, and only one of the two is the
authority.

Now query both tables again:

```bash
docker compose exec postgres psql -U postgres -d brightertests \
  -c 'select * from Greeting;' \
  -c 'select messageid, topic, dispatched from Outbox;'
```

```text
 id |        message
----+-----------------------
  1 | Hello from the sender
(1 row)

              messageid               |     topic      |          dispatched
--------------------------------------+----------------+-------------------------------
 01a03dfa-1542-7196-8db0-3dea053ae393 | greeting.event | 2026-08-26 12:10:04.796541+00
(1 row)
```

**Neither table gained a row.** `This greeting will not survive` is in neither, and the receiver
stayed silent — no message was ever sent, because no message was ever committed. The two writes
were never two writes; they were one transaction.

> **`Greeting.Id` skips a number after a failed run.** Postgres allocates from the sequence
> before the rollback and does not hand it back, so the ids go 1, 3. Nothing was lost — ids are
> not a count.

Stop both containers when you are done. The `-v` discards the Postgres volume, so the next run
starts from an empty database:

```bash
docker compose down -v
```

## What the Durable Outbox Showed You

Your handler gained a transaction and one extra write. The system gained a guarantee:

- **The Outbox turns two systems into one transaction.** Your data and the message announcing
  it are written to the same database on the same connection, so the database's own atomicity
  is what keeps them agreeing. No distributed transaction, no two-phase commit, no compensating
  logic.
- **The Sweeper decouples sending from committing.** Once the row is in the Outbox the message
  will go out — on the next tick, or after a restart, or when the broker comes back. The
  sending process no longer has to survive for the message to be delivered.
- **What you get is [at-least-once](/contents/Glossary.md#at-least-once), not exactly-once.** You
  watched `dispatched` stay null until *after* the message went to RabbitMQ — so a process that
  dies in that gap leaves a row the next sweep will send again. Combined with rung 2's
  redelivery-on-failure, that is two reasons your handlers should tolerate seeing the same
  message twice.
- **Brighter owns one table and you own the other.** Provisioning creates the Outbox; `Greeting`
  was yours to create, and your schema stays yours.

The next rung changes the transport rather than the guarantee:
[Streaming with Kafka](/contents/TutorialStreamingWithKafka.md), where messages are
partitioned, consumers form a group, and ordering is something you get per key rather than
per topic.

## Further Reading

- [Your First Message Over a Broker](/contents/TutorialFirstMessage.md) — rung 2, if you skipped
  it
- [Box Provisioning](/contents/BoxProvisioning.md#when-to-use-box-provisioning) — this page took
  Option A silently; if your database will not grant `CREATE TABLE`, read the other one
- [Using the PostgreSQL Outbox](/contents/PostgresOutbox.md) — the DDL, the configuration and
  the Entity Framework Core variant, in full
- [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) — every Outbox Brighter ships,
  the archiver, and how the Sweeper is configured beyond these two options
- [Outbox Pattern Support](/contents/OutboxPattern.md) — why the pattern exists, without any
  configuration in the way
- [Transactional Messaging with the Outbox](/contents/TransactionalMessagingWithTheOutbox.md) —
  the same idea taken to production, with an Inbox on the consuming side
- [Glossary](/contents/Glossary.md) — every term this page linked, and the rest
