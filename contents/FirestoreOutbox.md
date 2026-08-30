---
description: "The Firestore Outbox stores messages in a Google Cloud Firestore collection, so a message and the business write that produced it commit together."
layout:
  description:
    visible: false
---

# Firestore Outbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](/contents/BrighterOutboxSupport.md)

The Firestore Outbox stores messages in a Google Cloud Firestore collection, so a message and the business write that produced it commit together. It is a good choice when your workload already runs on Google Cloud.

## Firestore Outbox Configuration

The Outbox package pulls in the shared Firestore package, so you install one and get both:

```powershell
dotnet add package Paramore.Brighter.Outbox.Firestore
```

* **Paramore.Brighter.Outbox.Firestore** — the Outbox itself
* **Paramore.Brighter.Firestore** — `FirestoreConfiguration`, the connection provider and the unit of work

Everything is configured through a single `FirestoreConfiguration`. Register it in the
container *as well as* passing it to the Outbox: `producers.ConnectionProvider` and
`producers.TransactionProvider` take a **`Type`**, so the container activates them, and both
constructors ask for the configuration.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Firestore;
using Paramore.Brighter.Outbox.Firestore;
using Paramore.Brighter.Outbox.Hosting;

public static class FirestoreOutboxRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new FirestoreConfiguration(
            projectId: "my-gcp-project",
            database: "(default)")
        {
            Outbox = new FirestoreCollection { Name = "Outbox" }
        };

        // The providers below are activated by the container, and both take this type
        services.AddSingleton(configuration);

        services.AddBrighter()
            .AddProducers(producers =>
            {
                producers.Outbox = new FirestoreOutbox(configuration);
                producers.ConnectionProvider = typeof(FirestoreConnectionProvider);
                producers.TransactionProvider = typeof(FirestoreUnitOfWork);
                // ... your producer registry
            })
            .UseOutboxSweeper()
            .AutoFromAssemblies();
    }
}
```

`FirestoreOutbox` also has a constructor taking an `IAmAFirestoreConnectionProvider` alongside
the configuration, when you want to share one client across the Outbox, the Inbox and the lock.

## Firestore Outbox Options

`FirestoreConfiguration` is constructed with the two values that identify the database and
carries the rest as properties.

<!-- optioncheck: Paramore.Brighter.Firestore.FirestoreConfiguration
     manual: TimeProvider — initialised to TimeProvider.System, which has no printable value
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `Outbox` | `FirestoreCollection?` | `null` | Names the collection this Outbox writes messages to. |
| `Inbox` | `FirestoreCollection?` | `null` | Names the collection the [Firestore Inbox](/contents/FirestoreInbox.md) writes to. |
| `Locking` | `FirestoreCollection?` | `null` | Names the collection the [Firestore distributed lock](/contents/FirestoreDistributedLock.md) writes its lock documents to. |
| `TimeProvider` | `TimeProvider` | `TimeProvider.System` | Supplies the clock used for message timestamps and time-to-live expiry. |
| `Credential` | `ICredential?` | `null` | Authenticates to Firestore; Application Default Credentials apply when it is null. |
| `Instrumentation` | `InstrumentationOptions` | `All` | Sets how much telemetry detail the Outbox emits. |
| `Configure` | `Action<FirestoreClientBuilder>?` | `null` | Customises the `FirestoreClientBuilder` before the client is built. |

**`projectId` and `database` are constructor parameters, not properties you can set.** They
surface as the get-only `ProjectId` and `Database`, and the derived `DatabasePath` composes
them as `projects/{ProjectId}/databases/{Database}`. There is no default for either: a
`FirestoreConfiguration` cannot be constructed without both.

**`Outbox` defaults to `null` and the Outbox will not run without it.** `FirestoreOutbox`
throws `ArgumentException` from its constructor when `Outbox` is null or its `Name` is empty,
so treat the `null` in the table as *you must set this*, not as *this is optional*.

Three of those options take a `FirestoreCollection`, which carries two of its own:

<!-- optioncheck: Paramore.Brighter.Firestore.FirestoreCollection -->

| Option | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | `""` | Names the Firestore collection. |
| `Ttl` | `TimeSpan?` | `null` | Sets how long a document lives after it is written. |

`Ttl` writes a field; it does not delete anything on its own — see
[Provisioning](#provisioning-the-firestore-outbox) below.

## Provisioning the Firestore Outbox

**There is nothing to provision.** Firestore creates a collection the first time a document is
written to it, so unlike a relational Outbox there is no DDL, no builder and no migration
chain. [Database Provisioning](/contents/BoxProvisioning.md) covers the relational backends
only — MSSQL, PostgreSQL, MySQL, SQLite and Spanner — and Firestore is deliberately absent from
its [support matrix](/contents/BoxProvisioning.md#per-backend-support).

Two things do remain yours:

- **Access.** The application's service account needs read and write permission on the
  collection named by `Outbox.Name`.
- **Expiry, if you set `Ttl`.** Brighter writes a `Ttl` timestamp field onto each document, and
  that is all it does. Firestore deletes expired documents only once you have created a **TTL
  policy** on the `Ttl` field of that collection, through the Google Cloud console or the
  Firestore Admin API. Setting `Ttl` and skipping the policy leaves the field written and the
  documents in place.

## Further Reading

- [Outbox Support](/contents/BrighterOutboxSupport.md) — what an Outbox is for and how the Sweeper dispatches from it
- [Firestore Inbox](/contents/FirestoreInbox.md) — the same configuration type, one family over
- [Firestore Distributed Lock](/contents/FirestoreDistributedLock.md) — keeping one Sweeper active when you scale out
- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support) — where `producers.Outbox` fits
- [Outbox Archiver](/contents/OutboxArchiver.md) — moving dispatched messages out of the collection
