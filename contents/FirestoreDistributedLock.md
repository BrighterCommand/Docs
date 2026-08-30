---
description: "The Firestore locking provider implements Brighter's distributed lock using Google Cloud Firestore, so a single Outbox Sweeper and Archiver run when you scale out."
layout:
  description:
    visible: false
---

# Firestore Distributed Lock

> **Reference** · Applies to **Brighter V10**

The Firestore locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using Google Cloud Firestore, so a single [Outbox
Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when you
scale out. It is a good choice when your workloads already run on Google Cloud.

## Firestore Distributed Lock Package

* **Paramore.Brighter.Locking.Firestore**

The provider stores its locks in a Firestore collection that you name through the
Firestore configuration — see [Provisioning](#firestore-distributed-lock-provisioning).

## Firestore Distributed Lock Configuration

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

The lock has no options type of its own. `FirestoreConfiguration` and the
`FirestoreCollection` you assign to `Locking` are documented once, on the Outbox page: see
[Firestore Outbox Options](/contents/FirestoreOutbox.md#firestore-outbox-options). The two that
matter here are `Locking.Name`, which names the collection holding the lock documents, and
`Locking.Ttl`, which stamps each document with an expiry timestamp.

## Firestore Distributed Lock Example

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

## Firestore Distributed Lock Provisioning

The provider creates lock documents in the collection named by `Locking.Name`. It
acquires a lock with an atomic create that succeeds only when the document does not
already exist, so only one instance holds a given lock at a time. Ensure the application's
service account can read and write documents in the lock collection.

**`Ttl` writes a field; it does not delete anything on its own.** Brighter stamps a `Ttl`
timestamp onto each lock document, and Firestore removes an expired document only once you have
created a **TTL policy** on that field, through the Google Cloud console or the Firestore Admin
API. Without the policy a crashed instance's lock document stays where it is, so create the
policy if you are relying on expiry to recover the lock.

## Firestore Distributed Lock Notes

- Resource names are normalised for Firestore (for example `/` and `.` are replaced),
  so lock document ids are always valid. You do not need to do anything for this.
- Set `Ttl` longer than a typical Sweeper or Archiver batch so a lock is not expired
  mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [Firestore Outbox](/contents/FirestoreOutbox.md) — the matching Outbox, and where `FirestoreConfiguration` is documented
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
