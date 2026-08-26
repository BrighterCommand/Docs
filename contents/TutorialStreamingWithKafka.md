---
description: "Send the same greeting over a three-partition Kafka topic, then start a second copy of the receiver and watch Kafka hand it half the partitions."
layout:
  description:
    visible: false
---

# Streaming with Kafka

> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Message Over a Broker](/contents/TutorialFirstMessage.md), [Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)

Send the same greeting over a three-partition Kafka topic, then start a second copy of the
receiver and watch Kafka hand it half the [partitions](/contents/Glossary.md#partition).

This is the fourth and last rung of the ladder. Rungs 2 and 3 kept one queue and one reader.
Kafka splits a topic into partitions, and a group of readers divides those partitions between
them — so scaling out is starting the same process again, changing nothing. What you give up is
a single global order, and what you get back is order *per key*, which is usually the order you
actually wanted.

**Rung 4 branches from rung 2, not from rung 3.** The durable Outbox and the Kafka transport are
independent choices, and this rung changes the transport while keeping rung 2's plain `Post`.

## What You'll Build: A Partitioned Kafka Stream

Rung 2's three projects with the transport swapped, and nothing else moved:

| What | Change from rung 2 |
|---|---|
| `Greetings` | unchanged |
| `GreetingsReceiver` | the [subscription](/contents/Glossary.md#subscription) becomes a `KafkaSubscription`, with a group id. The [handler](/contents/Glossary.md#handler) is untouched |
| `GreetingsSender` | the [publication](/contents/Glossary.md#publication) becomes a `KafkaPublication` with three partitions, and each greeting carries a [partition key](/contents/Glossary.md#partition-key) |

The sender publishes nine greetings — three each for `alice`, `grace` and `mia`, interleaved.
The receiver prints them **grouped by recipient rather than in the order they were sent.** Then
you start a second receiver, watch the group rebalance, and send the nine again to see each
recipient's greetings stay in sequence on whichever consumer now owns them.

## Before You Start Streaming with Kafka

- **Rungs 2 and 3 complete.** [Your First Message Over a
  Broker](/contents/TutorialFirstMessage.md) builds the three projects, and [Adding a Durable
  Outbox](/contents/TutorialDurableOutbox.md) is the guarantee this rung leaves alone.
- **Start from a copy of rung 2.** Rung 3 changed rung 2's sender in place, so if you worked
  through it in the same folder, copy your rung 2 solution aside — or rebuild those three
  projects — before making the changes below. Nothing here builds on rung 3's Outbox.
- **The .NET 9 SDK.** Check with `dotnet --version`.
- **Docker Desktop**, running, with port **9092** free. RabbitMQ and Postgres are not needed;
  you can stop them.
- **About thirty minutes**, most of it reading and waiting for a rebalance. The machine work
  measured **5.2 seconds** to swap the package on both projects and **3.1 seconds** to build,
  starting from a rung 2 solution and a NuGet cache holding only rung 2's packages. Pulling the
  Kafka image the first time takes longer and depends on your connection.

## Step 1: Start Kafka

Put this in `docker-compose-kafka.yml` beside your solution:

```yaml
services:
  kafka:
    image: apache/kafka:4.0.2
    hostname: kafka
    container_name: kafka
    ports:
      - "9092:9092"
    environment:
      KAFKA_NODE_ID: 1
      KAFKA_PROCESS_ROLES: broker,controller
      KAFKA_CONTROLLER_QUORUM_VOTERS: 1@kafka:29093
      CLUSTER_ID: 'MkU3OEVBNTcwNTJENDM2Qk'
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:29092,PLAINTEXT_HOST://0.0.0.0:9092,CONTROLLER://0.0.0.0:29093
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT,CONTROLLER:PLAINTEXT
      KAFKA_CONTROLLER_LISTENER_NAMES: CONTROLLER
      KAFKA_AUTO_CREATE_TOPICS_ENABLE: "false"
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS: 0
```

```bash
docker compose -f docker-compose-kafka.yml up -d
```

This is a single broker running in **KRaft** mode — no ZooKeeper. One broker is enough to learn
partitions and consumer groups on, because both are properties of a *topic* rather than of the
cluster.

`KAFKA_AUTO_CREATE_TOPICS_ENABLE: "false"` is deliberate. With it on, a typo in a topic name
silently creates a new topic with the broker's default partition count, and you spend an
afternoon wondering why nothing arrives.

As in rung 2, the command returns before the broker is accepting connections. It is ready when
this stops failing:

```bash
docker exec kafka /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server localhost:9092
```

## Step 2: Swap the Transport

Both processes talk to the broker, so both change. Remove RabbitMQ and add Kafka:

```bash
dotnet remove GreetingsSender package Paramore.Brighter.MessagingGateway.RMQ.Async
dotnet remove GreetingsReceiver package Paramore.Brighter.MessagingGateway.RMQ.Async
dotnet add GreetingsSender package Paramore.Brighter.MessagingGateway.Kafka --version 10.7.0
dotnet add GreetingsReceiver package Paramore.Brighter.MessagingGateway.Kafka --version 10.7.0
```

That is the entire package change. `Paramore.Brighter`,
`Paramore.Brighter.Extensions.DependencyInjection` and the two
`Paramore.Brighter.ServiceActivator.*` packages are rung 2's and stay exactly as they were —
which is the point worth noticing: **a transport is a package, not an architecture.** The
[Command Processor](/contents/Glossary.md#command-processor), the handler and the event do not
know which broker they are running over.

Your `.csproj` files show `PackageReference` with a pinned version. The sample in the Brighter
repository uses `ProjectReference` into the source tree instead, which is the one place page and
sample differ on purpose.

## Step 3: Send with a Partition Key

Replace `GreetingsSender/Program.cs` with this:

```csharp
using System;
using Greetings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Kafka;

// Kafka is reached through a bootstrap server list rather than a single connection string.
// The client asks any broker in the list for the cluster's real topology, so one entry is
// enough to learn on; a production cluster names several so that startup survives one being
// down.
var kafkaConfiguration = new KafkaMessagingGatewayConfiguration
{
    Name = "greetings.sender",
    BootStrapServers = ["localhost:9092"]
};

// A publication tells Brighter where a request type goes: which topic, on which broker.
// NumPartitions is the delta from rung 2. A RabbitMQ queue is one ordered stream; a Kafka
// topic is NumPartitions of them, and that is what gives a consumer group something to
// share out. OnMissingChannel.Create means the sender creates the topic if it is missing, with
// this many partitions.
//
// Two defaults are doing quiet work here, and the ordering this rung teaches depends on both:
// EnableIdempotence is true and MaxInFlightRequestsPerConnection is 1. Without them a retry
// could reorder messages *within* a partition, and no amount of careful keying would help.
// Acquiring the idempotence id is also what logs "Failed to acquire idempotence PID ...
// retrying" if you start the sender before the broker has finished booting.
//
// Naming GreetingEvent here is also what loads the Greetings assembly, which matters below:
// AutoFromAssemblies scans the assemblies loaded so far, so anything it must find has to have
// been touched before the call. Reordering this below the registration is a silent no-op.
var producerRegistry = new KafkaProducerRegistryFactory(
    kafkaConfiguration,
    [
        new KafkaPublication<GreetingEvent>
        {
            Topic = new RoutingKey("greeting.event"),
            NumPartitions = 3,
            MakeChannels = OnMissingChannel.Create
        }
    ]).Create();

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBrighter()
    .AddProducers(configure => configure.ProducerRegistry = producerRegistry)
    .AutoFromAssemblies();

// Three recipients, three greetings each, sent round-robin so that the three streams are
// interleaved on the wire.
//
// These three names are not arbitrary. Kafka picks the partition by hashing the key, so you
// do not get to choose it, and with three keys over three partitions there is only a 2-in-9
// chance that they land one apiece. The first three names this sample tried, "alice", "bob"
// and "carol", all hashed to the same partition — which a single consumer hides completely,
// because it drains all three partitions and everything still arrives in order.
//
// These three were checked against the broker rather than calculated. The hash is
// crc32(key) % partition-count for librdkafka, which the Confluent .NET client wraps; the
// Java client uses murmur2 and would scatter the same three names differently. So the 2/0/1
// mapping below holds for this client at exactly three partitions and nowhere else.
string[] recipients = ["alice", "grace", "mia"];
const int greetingsPerRecipient = 3;

// The host is built but never run: Post is synchronous, so all we need from it is the
// container. Rung 3 does run the host, because it hosts the Outbox Sweeper.
using (var host = builder.Build())
{
    var commandProcessor = host.Services.GetRequiredService<IAmACommandProcessor>();

    for (var i = 1; i <= greetingsPerRecipient; i++)
    {
        foreach (var recipient in recipients)
        {
            // The partition key is how you choose a partition, and the partition is the only
            // thing Kafka orders. Keying on the recipient sends every one of alice's greetings
            // to the same partition, so they arrive in the order they were sent — while grace's
            // and mia's are free to be handled on other partitions.
            //
            // No message mapper is needed for this. JsonMessageMapper<T> is Brighter's
            // registered default for both the sync and the async path, and it already reads
            // the key out of the request context onto the message header.
            var context = new RequestContext();
            context.Bag[RequestContextBagNames.PartitionKey] = recipient;

            commandProcessor.Post(new GreetingEvent($"Hello {recipient} #{i}"), context);
        }
    }
}
// Delivery to Kafka is asynchronous: Post hands the message to the producer's send queue and
// returns, and the broker's acknowledgement arrives on a delivery report later. Disposing the
// host flushes that queue and waits for the reports, which is why this line sits after the
// brace rather than inside the loop.
Console.WriteLine($"Published {recipients.Length * greetingsPerRecipient} greetings to greeting.event");
```

Three things changed from rung 2's sender, and one thing deliberately did not.

**`NumPartitions = 3`.** A RabbitMQ queue is a single ordered stream. A Kafka topic is
`NumPartitions` of them, and that is what gives a group of consumers something to divide. The
topic is created with this many partitions the first time a process reaches a broker that does
not have it.

**A partition key on every message.** Kafka orders messages *within* a partition and promises
nothing across partitions, so the key is how you decide what shares an order with what. Keying
on the recipient means all of alice's greetings go to one partition and stay in sequence, while
grace's and mia's are free to be handled elsewhere.

**No message mapper.** `KafkaTaskQueue`, Brighter's reference Kafka sample, writes one in order
to set the partition key — but it does not have to. `JsonMessageMapper<T>` is the registered
default for both the synchronous and the asynchronous path, and it already reads the key out of
the request context onto the message header. Setting
`context.Bag[RequestContextBagNames.PartitionKey]` and passing the context to `Post` is all it
takes. [Dispatching a Request](/contents/DispatchingARequest.md) covers the request context
properly.

**`Post` is unchanged.** No Outbox, no transaction — rung 3's guarantee is orthogonal to this
one, and combining them is a decision you make per service rather than a step on this ladder.

## Step 4: Consume as a Reactor

Replace `GreetingsReceiver/Program.cs` with this:

```csharp
using Greetings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var kafkaConfiguration = new KafkaMessagingGatewayConfiguration
{
    Name = "greetings.receiver",
    BootStrapServers = ["localhost:9092"]
};

// groupId is the delta from rung 2, and it is what makes a second copy of this process
// interesting rather than redundant. Every instance sharing a group id is one member of one
// consumer group, and Kafka gives each member a disjoint set of partitions: run one instance
// and it holds all three, start a second and Kafka rebalances the group so they hold roughly
// half each. Two instances with *different* group ids would each get all three and both would
// see every greeting.
//
// messagePumpType is KafkaSubscription<T>'s own default — note the generic; the non-generic
// KafkaSubscription defaults to MessagePumpType.Unknown. It is written out anyway, because it
// is the point of this rung.
// A Reactor pump is a single thread per performer, and noOfPerformers defaults to 1 — so one
// instance is one member with one thread, draining its partitions in turn. That single thread
// is why per-key ordering holds; it is not that Brighter runs a pump per partition.
//
// numOfPartitions matches the publication so that whichever process reaches an empty broker
// first creates the same three-partition topic. Keep the two in step if you change either:
// when the topic already exists and does not match, Brighter logs "topic is misconfigured =>
// NumPartitions should be ..." as a *warning* and carries on, so raising one side alone gives
// you a puzzling half-working sample rather than an error.
var subscriptions = new Subscription[]
{
    new KafkaSubscription<GreetingEvent>(
        new SubscriptionName("greeting.subscription"),
        new ChannelName("greeting.event"),
        new RoutingKey("greeting.event"),
        groupId: "greeting.readers",
        numOfPartitions: 3,
        messagePumpType: MessagePumpType.Reactor,
        makeChannels: OnMissingChannel.Create)
};

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(new KafkaMessageConsumerFactory(kafkaConfiguration));
    })
    .AutoFromAssemblies();

builder.Services.AddHostedService<ServiceActivatorHostedService>();

var host = builder.Build();

await host.RunAsync();
```

**`groupId` is the delta that matters.** Every instance sharing a group id is one member of one
[consumer group](/contents/Glossary.md#consumer-group), and Kafka guarantees each member a
disjoint set of partitions. That is what makes starting a second copy of this process
*useful* rather than duplicative — two instances with different group ids would each receive
every greeting.

**The pump runs as a [Reactor](/contents/Glossary.md#reactor)**, which is
`KafkaSubscription<T>`'s own default and is written out because it is this rung's point. A
Reactor is a single thread per performer, and `noOfPerformers` defaults to 1 — so one running
process is one group member with one thread, draining its partitions in turn.

**That single thread is why per-key ordering holds.** It is not that Brighter runs a
[message pump](/contents/Glossary.md#message-pump) per partition; it runs one per performer,
and Kafka gives that member whatever partitions the group's split assigns it.
[Reactor and Proactor](/contents/ReactorAndProactor.md) covers the choice properly, including
what changes if you raise `noOfPerformers`.

**`GreetingEventHandler.cs` does not change at all** — it is rung 2's file, byte for byte. A
handler does not know what transport delivered its message, and that is the whole benefit of
having one.

## Step 5: Watch One Consumer Take All Three Partitions

Start the receiver:

```bash
dotnet run --project GreetingsReceiver
```

Among its startup logging, one line is the one to watch:

```text
Partition Added greeting.event : 0,greeting.event : 1,greeting.event : 2
```

**One member gets every partition.** The group has one instance, so the split is trivial — but
it is the same mechanism that will divide them in step 6.

Now, in a second terminal:

```bash
dotnet run --project GreetingsSender
```

```text
Published 9 greetings to greeting.event
```

And the receiver prints:

```text
Received: Hello alice #1
Received: Hello alice #2
Received: Hello alice #3
Received: Hello grace #1
Received: Hello grace #2
Received: Hello grace #3
Received: Hello mia #1
Received: Hello mia #2
Received: Hello mia #3
```

**Read that carefully, because it is not the order they were sent.** The sender interleaved the
recipients — alice #1, grace #1, mia #1, alice #2, and so on — and they came back grouped. One
consumer, one thread, and still the order changed.

That is partitions doing exactly what they promise. The single thread drains one partition at a
time, and each partition holds one recipient's greetings. **There is no global order to
preserve, and Kafka never claimed there was.** What survived is each recipient's own sequence.

You can see where the keys landed by reading the topic directly:

```bash
docker exec kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 --topic greeting.event --from-beginning \
  --max-messages 9 --property print.partition=true --property print.key=true \
  --property print.value=false --timeout-ms 15000
```

```text
Partition:0	grace
Partition:0	grace
Partition:0	grace
Partition:1	mia
Partition:1	mia
Partition:1	mia
Partition:2	alice
Partition:2	alice
Partition:2	alice
```

**Those three names were chosen, and the reason is a trap worth knowing about.** Kafka picks the
partition by hashing the key, so you choose the key and the hash chooses the partition. With
three keys over three partitions there is only a 2-in-9 chance they land one apiece — and the
first three names this tutorial used, `alice`, `bob` and `carol`, all hashed to partition 2.
Nothing looked wrong: one consumer holds every partition anyway, so all nine greetings arrived
in perfect order and the code appeared to work while demonstrating nothing at all. Running the
command above is what exposed it.

## Step 6: Start a Second Instance

Leave everything running and start the receiver again, in a third terminal, with no arguments
and no configuration change:

```bash
dotnet run --project GreetingsReceiver
```

The **first** receiver logs that its partitions were taken away and which it got back:

```text
Partitions for consumer revoked greeting.event : [0],greeting.event : [1],greeting.event : [2]
Partition Added greeting.event : 0,greeting.event : 2
```

The **second** logs what it was given:

```text
Partition Added greeting.event : 1
```

**That is a rebalance.** Kafka revoked the whole assignment and redistributed it, and the two
sets are disjoint and cover all three partitions between them. Which instance gets which is
Kafka's decision, not yours — run this twice and you may well see the split the other way
round.

Nothing was configured to make this happen. Scaling a Kafka consumer out is starting the process
again; scaling it in is stopping one. The group id is the only thing that had to agree.

## Step 7: Ordering Holds Per Key

With both receivers running, send the nine greetings again:

```bash
dotnet run --project GreetingsSender
```

The instance holding partitions 0 and 2 prints grace's and alice's:

```text
Received: Hello grace #1
Received: Hello grace #2
Received: Hello grace #3
Received: Hello alice #1
Received: Hello alice #2
Received: Hello alice #3
```

The instance holding partition 1 prints mia's:

```text
Received: Hello mia #1
Received: Hello mia #2
Received: Hello mia #3
```

**The work divided and every recipient's sequence survived.** No greeting was handled twice, no
recipient's greetings were split across the two processes, and each one's `#1`, `#2`, `#3`
arrived in that order on whichever consumer owned its partition.

**Which recipient's block prints first may differ on your machine, and that is the point rather
than a flaw.** The instance holding two partitions drains them in whatever order it polls them,
so grace and alice can swap places between runs — this page has seen both. What never varies is
the run *inside* each block. The order you are promised is the one you asked for with a key.

This is the guarantee Kafka actually offers, and it is worth stating exactly: **ordering is per
partition, and therefore per key.** If you need two events to be processed in order, give them
the same key. If they do not share a key, you have not asked for an order and you will not get
one — no matter how few consumers are running.

Two defaults are quietly making the per-key half true, and they are worth knowing before you
tune anything: `KafkaPublication` sets `EnableIdempotence = true` and
`MaxInFlightRequestsPerConnection = 1`. Without them, a producer retrying a failed send could
reorder messages within a partition, and careful keying would not save you.

## Step 8: Offsets and What a Rebalance Costs You

An [offset](/contents/Glossary.md#offset) is a consumer group's bookmark in a partition. Ask
Kafka where this group has got to:

```bash
docker exec kafka /opt/kafka/bin/kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 --describe --group greeting.readers
```

Run that immediately after the greetings are handled and the answer is surprising:

```text
TOPIC           PARTITION  CURRENT-OFFSET  LOG-END-OFFSET  LAG
greeting.event  0          -               3               -
greeting.event  1          -               3               -
greeting.event  2          -               3               -
```

**Nothing is committed, although every message was handled.** Handling a message *stores* an
offset; it does not commit one. Brighter commits on two paths, and at nine messages neither has
run yet:

- **`commitBatchSize`**, default **10** — offsets are committed in batches, and nine is not ten.
- **`sweepUncommittedOffsetsInterval`**, default **30 seconds** — a timer that commits whatever
  is still outstanding.

Wait half a minute and ask again:

```text
greeting.event  0          2               3               1
greeting.event  1          2               3               1
greeting.event  2          2               3               1
```

The sweeper has committed, and each partition is left **one short**. That residual `LAG 1` is a
Brighter defect rather than Kafka semantics, tracked as
[#4281](https://github.com/BrighterCommand/Brighter/issues/4281); a group that has handled
everything ought to reach `LAG 0`.

**Whatever is uncommitted when a rebalance happens gets handled again.** Look back at step 6: as
well as logging the new assignment, each receiver handled one greeting a second time — the one
sitting at the uncommitted offset on each partition it picked up. Three greetings, one per
partition, already handled by the instance that had them before.

That is **[at-least-once](/contents/Glossary.md#at-least-once) delivery**, and it is the honest
statement about every broker on this ladder. Rung 3's Outbox gave you at-least-once on the
sending side; this is the same promise on the receiving side. **Nothing is lost. Some things
arrive twice. A handler that does real work — charging a card, sending an email — has to
tolerate seeing the same message again.**

Tuning the two options above changes *how many* duplicates a rebalance costs, and cannot take it
to zero: a rebalance can always arrive between handling a message and committing its offset.
[Kafka Configuration](/contents/KafkaConfiguration.md) documents both, along with everything
else `KafkaSubscription` and `KafkaPublication` expose.

When you are finished:

```bash
docker compose -f docker-compose-kafka.yml down -v
```

## What Streaming with Kafka Showed You

You swapped one package and gained a scaling model:

- **A transport is a package.** The event, the handler and the Command Processor never learned
  which broker they were running over. Only the publication and the subscription changed, and
  the handler file is rung 2's byte for byte.
- **Partitions are the unit of parallelism, and the key chooses one.** You do not pick a
  partition; you pick a key and Kafka hashes it. Everything sharing a key shares an order.
- **A consumer group scales by starting the process again.** Two instances, one group id, and
  Kafka divided the partitions without either process being told about the other.
- **Ordering is per key, not per topic — and the single-threaded Reactor pump is what delivers
  it.** One pump per performer, one performer by default, so one member drains its partitions in
  turn. It is not a pump per partition.
- **Delivery is at-least-once, and a rebalance is when you notice.** Offsets commit in batches
  or on a timer, so anything uncommitted when partitions move is handled again.

That is the ladder. You have built a command and a handler, sent a message between two
processes, made the send survive a crash, and scaled the receiving side across a partitioned
stream. Everything after this is configuration, middleware and the shape of your own domain —
and the rest of this documentation is organised around exactly those.

## Further Reading

- [Adding a Durable Outbox](/contents/TutorialDurableOutbox.md) — rung 3, if you skipped it;
  its guarantee composes with this one
- [Kafka Configuration](/contents/KafkaConfiguration.md) — every option on `KafkaSubscription`
  and `KafkaPublication`, including `commitBatchSize`, the sweep interval and security
- [Reactor and Proactor](/contents/ReactorAndProactor.md) — why the pump is single-threaded,
  and what changes when you raise `noOfPerformers`
- [Dispatching a Request](/contents/DispatchingARequest.md) — the request context, and the other
  well-known keys besides the partition key
- [Get Started with Brighter](/contents/GetStarted.md) — the ladder's front door, and where to
  go next
- [Glossary](/contents/Glossary.md) — every term this page linked, and the rest
