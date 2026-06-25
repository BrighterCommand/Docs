# Firestore Distributed Lock

The Firestore locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using Google Cloud Firestore, so a single [Outbox
Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when you
scale out. It is a good choice when your workloads already run on Google Cloud.

## Package

* **Paramore.Brighter.Locking.Firestore**

The provider stores its locks in a Firestore collection that you name through the
Firestore configuration — see [Provisioning](#provisioning).

## Configuration

The Firestore provider is configured through a `FirestoreConfiguration`, whose
constructor takes your project id and database. Set its `Locking` property to a
`FirestoreCollection` that names the lock collection and, optionally, a time-to-live:

```csharp
var configuration = new FirestoreConfiguration(
    projectId: "my-gcp-project",
    database: "(default)")
{
    Locking = new FirestoreCollection
    {
        Name = "brighter-locks",
        Ttl = TimeSpan.FromMinutes(1)
    }
};

var lockingProvider = new FirestoreDistributedLock(configuration);
```

`FirestoreDistributedLock` also has a constructor that accepts an
`IAmAFirestoreConnectionProvider` alongside the configuration if you want to share a
connection provider.

| Setting (on `Locking`) | Type | Description |
|------------------------|------|-------------|
| `Name` | `string` | The collection that holds the lock documents. |
| `Ttl` | `TimeSpan?` | Optional expiry for lock documents. |

## Example

```csharp
var configuration = new FirestoreConfiguration("my-gcp-project", "(default)")
{
    Locking = new FirestoreCollection { Name = "brighter-locks", Ttl = TimeSpan.FromMinutes(1) }
};

services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your external Outbox */;
        // ... connection/transaction providers for your Outbox ...

        opt.DistributedLock = new FirestoreDistributedLock(configuration);
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## Provisioning

The provider creates lock documents in the collection named by `Locking.Name`. It
acquires a lock with an atomic create that succeeds only when the document does not
already exist, so only one instance holds a given lock at a time. Setting `Ttl` lets
stale locks expire, which protects you if an instance crashes before releasing its lock.
Ensure the application's service account can read and write documents in the lock
collection.

## Notes

- Resource names are normalised for Firestore (for example `/` and `.` are replaced),
  so lock document ids are always valid. You do not need to do anything for this.
- Set `Ttl` longer than a typical Sweeper or Archiver batch so a lock is not expired
  mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
