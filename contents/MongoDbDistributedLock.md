# MongoDB Distributed Lock

The MongoDB locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) by writing lock documents to a MongoDB collection, so
a single [Outbox Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver
run when you scale out. It pairs naturally with the [MongoDB
Outbox](/contents/MongoDBOutbox.md).

## Package

* **Paramore.Brighter.Locking.MongoDb**

The provider stores its locks in a collection that you name through the MongoDB
configuration — see [Provisioning](#provisioning).

## Configuration

The MongoDB provider is configured through an `IAmAMongoDbConfiguration`, the same
configuration type used by the MongoDB Outbox. Set its `Locking` property to a
`MongoDbCollectionConfiguration` that names the lock collection and, optionally, a
time-to-live for lock documents:

```csharp
var configuration = new MongoDbConfiguration(
    connectionString: "mongodb://localhost:27017",
    databaseName: "orders")
{
    Locking = new MongoDbCollectionConfiguration
    {
        Name = "brighter_locks",
        TimeToLive = TimeSpan.FromMinutes(1)
    }
};

var lockingProvider = new MongoDbLockingProvider(configuration);
```

`MongoDbLockingProvider` also has a constructor that accepts an
`IAmAMongoDbConnectionProvider` alongside the configuration if you want to share a
connection provider.

| Setting (on `Locking`) | Type | Description |
|------------------------|------|-------------|
| `Name` | `string` | The collection that holds the lock documents. |
| `TimeToLive` | `TimeSpan?` | Optional expiry for lock documents, applied through a MongoDB TTL index. |

## Example

```csharp
var configuration = new MongoDbConfiguration("mongodb://localhost:27017", "orders")
{
    Locking = new MongoDbCollectionConfiguration { Name = "brighter_locks", TimeToLive = TimeSpan.FromMinutes(1) }
};

services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your MongoDB Outbox */;
        // ... connection/transaction providers for your Outbox ...

        opt.DistributedLock = new MongoDbLockingProvider(configuration);
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## Provisioning

The provider writes lock documents to the collection named by `Locking.Name` and relies
on a unique index on the resource so only one instance can hold a given lock. If you set
`TimeToLive`, a TTL index expires stale lock documents automatically, which protects you
if an instance crashes before releasing its lock. Ensure the application's MongoDB user
can create the collection and its indexes, or create them ahead of time.

## Notes

- Set `TimeToLive` longer than a typical Sweeper or Archiver batch so a lock is not
  expired mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).
- Point the provider at the same MongoDB database as your Outbox so the Sweeper,
  Archiver, and lock share infrastructure.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [MongoDB Outbox](/contents/MongoDBOutbox.md) — the matching Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
