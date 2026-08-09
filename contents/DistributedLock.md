# Distributed Lock

> **Explanation** · Applies to **Brighter V10**

A **distributed lock** lets multiple instances of your application coordinate so that
only one of them performs a given task at a time. Brighter uses it to guarantee that a
single [Outbox Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and a single
[Outbox Archiver](/contents/OutboxArchiver.md) run against an
Outbox, even when you scale your application out across many processes or containers.

This page explains why the lock matters, the contract it implements, the default
in-process lock, how the Sweeper and Archiver use it, and how to configure a provider.
Backend-specific configuration lives on the per-provider pages linked below.

## Why a Distributed Lock?

The Outbox Sweeper dispatches messages that are sitting in your Outbox, and the Outbox
Archiver moves old messages out of it. Both are background processes, and for
correctness you should run **only one Sweeper and one Archiver per Outbox at a time**.

If you run the Sweeper in-process and then scale your producer application to several
instances for resilience and throughput, every instance starts its own Sweeper. They
then compete to dispatch the same messages, which wastes work and increases the chance
of sending a message more than once.

A distributed lock solves this. Before each run, the Sweeper (or Archiver) tries to
acquire a named lock held in shared infrastructure — a database table, a blob lease, an
advisory lock, and so on. Only the instance that acquires the lock does the work; the
others see the lock is held and skip that cycle. As instances come and go, whichever one
holds the lock is the single active worker.

> A distributed lock is different from [Sweeper Circuit
> Breaking](/contents/SweeperCircuitBreaking.md). The lock ensures *one* Sweeper runs;
> circuit breaking stops a *failing topic* from blocking the others. You can use both.

## The IDistributedLock Contract

A lock provider implements `IDistributedLock`:

```csharp
public interface IDistributedLock
{
    /// <summary>Attempt to obtain a lock on a resource</summary>
    /// <returns>The id of the lock that was acquired, or null if no lock could be acquired</returns>
    Task<string?> ObtainLockAsync(string resource, CancellationToken cancellationToken);

    /// <summary>Release a previously obtained lock</summary>
    Task ReleaseLockAsync(string resource, string lockId, CancellationToken cancellationToken);
}
```

The contract is small:

- **`resource`** is the name of the thing you are locking — for the Sweeper this is
  `"OutboxSweeper"`, for the Archiver it is `"Archiver"`.
- **`ObtainLockAsync`** returns a **lock id** string when it acquires the lock, or
  **`null`** when the lock is already held by someone else. `null` means "I did not get
  the lock — abandon this cycle and try again next time."
- **`ReleaseLockAsync`** hands the lock back, using the lock id that `ObtainLockAsync`
  returned so that only the holder can release it.

Most providers also give the lock a **lease**: a time after which the lock expires
automatically. This protects you if the holder crashes before it can release the lock —
see [Lease Expiry vs Manual Release](#lease-expiry-vs-manual-release).

## The Default In-Memory Lock

If you do not configure a provider, Brighter uses `InMemoryLock`. It coordinates only
*within a single process*, so it is enough when:

- You run a single instance of your application, or
- You use the `InMemoryOutbox`, which lives inside one process anyway.

You still need a Sweeper for the `InMemoryOutbox` — it runs in-process, where the
in-memory lock is sufficient. The moment you use an **external Outbox** (a database,
DynamoDB, MongoDB, and so on) **and** run more than one instance, the in-memory lock can
no longer coordinate across processes, and you need a real distributed lock provider.

## How the Sweeper and Archiver Use the Lock

The hosted Sweeper and Archiver follow the same obtain-work-release pattern. Each cycle:

1. Call `ObtainLockAsync` for its resource (`"OutboxSweeper"` or `"Archiver"`).
2. If the result is `null`, another instance holds the lock, so it logs and abandons
   this cycle.
3. If it gets a lock id, it does its work (dispatch or archive a batch), then releases
   the lock in a `finally` block so the lock is always returned.

You do not write this code yourself — you supply a lock provider and Brighter wires it
into the `TimedOutboxSweeper` and `TimedOutboxArchiver` hosted services. The same
configured `IDistributedLock` is shared by both, using their distinct resource names.

## Configuring a Distributed Lock

You set the lock on the producer registration through the `DistributedLock` property in
`AddProducers`. The example below uses DynamoDB for the Outbox and the DynamoDB lock
provider, then registers the Sweeper:

```csharp
var dynamoDb = new AmazonDynamoDBClient();

services
    .AddSingleton<IAmazonDynamoDB>(dynamoDb)   // shared by the Outbox and the lock
    .AddConsumers(opt => { /* consumer options */ })
    .AddProducers(opt =>
    {
        opt.Outbox = new DynamoDbOutbox(dynamoDb, new DynamoDbConfiguration { /* ... */ });
        opt.ConnectionProvider = typeof(DynamoDbUnitOfWork);
        opt.TransactionProvider = typeof(DynamoDbUnitOfWork);

        // Coordinate a single Sweeper/Archiver across instances
        opt.DistributedLock = new DynamoDbLockingProvider(
            dynamoDb,
            new DynamoDbLockingProviderOptions("brighter-locks", "sweeper-group"));
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

If you omit `opt.DistributedLock`, Brighter falls back to the in-process `InMemoryLock`.

For production, run the Sweeper and Archiver as a separate deployed executable rather
than inside your producer application — see [Running the Sweeper and Archiver Out of
Process](/contents/OutboxArchiver.md#running-the-sweeper-and-archiver-out-of-process).

## Available Providers

Brighter ships a distributed lock provider for each of the backends below. Choose the
one that matches infrastructure you already run — typically the same store as your
Outbox. Each provider page covers its package, configuration, and provisioning.

| Provider | Backend | NuGet package | Lease model |
|----------|---------|---------------|-------------|
| [DynamoDB](/contents/DynamoDbDistributedLock.md) | Amazon DynamoDB | `Paramore.Brighter.Locking.DynamoDB.V4` | Timed lease, auto-expiry or manual release |
| [Postgres](/contents/PostgresDistributedLock.md) | PostgreSQL | `Paramore.Brighter.Locking.PostgresSql` | Session advisory lock |
| [MS SQL](/contents/MsSqlDistributedLock.md) | SQL Server | `Paramore.Brighter.Locking.MsSql` | Session application lock |
| [MySQL](/contents/MySqlDistributedLock.md) | MySQL | `Paramore.Brighter.Locking.MySql` | Session `GET_LOCK` |
| [Azure Blob](/contents/AzureBlobDistributedLock.md) | Azure Blob Storage | `Paramore.Brighter.Locking.Azure` | Blob lease |
| [MongoDB](/contents/MongoDbDistributedLock.md) | MongoDB | `Paramore.Brighter.Locking.MongoDb` | TTL document |
| [Firestore](/contents/FirestoreDistributedLock.md) | Google Cloud Firestore | `Paramore.Brighter.Locking.Firestore` | TTL document |

For AWS, prefer the **V4** package (built on AWS SDK v4). Earlier AWS SDK major versions
are out of support on AWS; the legacy `Paramore.Brighter.Locking.DynamoDB` package
exists only to help you migrate.

## Lease Expiry vs Manual Release

Most providers acquire the lock with a **lease** — a validity period after which the
lock expires on its own. This matters for crash safety: if the instance holding the lock
dies before it can call `ReleaseLockAsync`, the lease still expires and another instance
can take over on a later cycle. Without a lease, a crashed holder could block the Sweeper
indefinitely.

The trade-off is in choosing the lease length:

- **Too short**, and the lease can expire while the Sweeper is still working a long
  batch, letting a second instance start in parallel.
- **Too long**, and recovery after a crash is delayed until the old lease expires.

Set the lease comfortably longer than a typical Sweeper or Archiver cycle. Lease-based
providers default to a one-minute lease (for example DynamoDB's `LeaseValidity` and Azure
Blob's `LeaseValidity`). The advisory-lock providers (Postgres, MS SQL, MySQL) instead
hold the lock for the lifetime of a database session, so the lock is released when the
connection closes rather than on a timed lease.

Some providers also let you choose to rely on expiry rather than releasing explicitly —
for example DynamoDB's `ManuallyReleaseLock` option. See the provider page for details.

## Further Reading

- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper, the Archiver, and
  running them out of process
- [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md) — a complementary
  concern: isolating a failing topic
- [Glossary](/contents/Glossary.md) — definitions of **Distributed Lock**, **Sweeper**,
  **Archiver**, and **Advisory Lock**
- Provider configuration: [DynamoDB](/contents/DynamoDbDistributedLock.md),
  [Postgres](/contents/PostgresDistributedLock.md),
  [MS SQL](/contents/MsSqlDistributedLock.md), [MySQL](/contents/MySqlDistributedLock.md),
  [Azure Blob](/contents/AzureBlobDistributedLock.md),
  [MongoDB](/contents/MongoDbDistributedLock.md),
  [Firestore](/contents/FirestoreDistributedLock.md)
