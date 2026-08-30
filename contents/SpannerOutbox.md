---
description: "The Spanner Outbox stores messages in a Google Cloud Spanner table, and it is configured entirely through the shared relational configuration."
layout:
  description:
    visible: false
---

# Spanner Outbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](/contents/BrighterOutboxSupport.md)

The Spanner Outbox stores messages in a Google Cloud Spanner table, and it is configured entirely through the shared relational configuration. Spanner's strong consistency makes the Outbox write and the business write one atomic commit.

## Spanner Outbox Configuration

```powershell
dotnet add package Paramore.Brighter.Outbox.Spanner
dotnet add package Paramore.Brighter.Spanner
```

**The Spanner Outbox has no options type of its own.** `SpannerOutbox` takes an
`IAmARelationalDatabaseConfiguration`, which is the same type every relational Outbox, Inbox,
provisioner and queue-table transport takes — so its options are documented once, at
[Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md#relational-database-configuration-options).
This page has no option table because there is no Spanner-specific surface to tabulate; what
follows is what Spanner does differently with the options that page lists.

Register the configuration against the interface as well as passing it to the Outbox: the
provisioner and the Sweeper resolve it from the container, and `producers.ConnectionProvider`
and `producers.TransactionProvider` take a `Type` the container activates.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.Spanner;
using Paramore.Brighter.Spanner;

public static class SpannerOutboxRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new RelationalDatabaseConfiguration(
            connectionString: "Data Source=projects/my-project/instances/my-instance/databases/brighter",
            outBoxTableName: "Outbox");

        services.AddSingleton<IAmARelationalDatabaseConfiguration>(configuration);

        services.AddBrighter()
            .AddProducers(producers =>
            {
                producers.Outbox = new SpannerOutbox(configuration);
                producers.ConnectionProvider = typeof(SpannerConnectionProvider);
                producers.TransactionProvider = typeof(SpannerUnitOfWork);
                // ... your producer registry
            })
            .UseOutboxSweeper()
            .AutoFromAssemblies();
    }
}
```

Two of the relational options behave differently here, and both change the table rather than
the code:

- **`binaryMessagePayload`** selects `BYTES(MAX)` for the `Body` column instead of
  `STRING(MAX)`. The two shapes are not interchangeable, so this is a decision made when the
  table is created.
- **`schemaName`** is not used here. Spanner does not organise tables into schemas the way the
  other relational backends do, and its provisioning extensions take no `schemaName` argument
  where the other four do.

## Provisioning the Spanner Outbox

You have the same two options every relational Outbox has, with one Spanner caveat on the
first.

**Option A — let Brighter provision it.** `AddSpannerOutbox` registers a provisioner that
creates the table at startup; see [Configuring Box
Provisioning](/contents/BoxProvisioningConfiguration.md#spanner). **Spanner uses the degenerate
runner**: it creates the table on a fresh database and cannot evolve an existing one. There is
no migration chain to run and `MigrationLockTimeout` is ignored, because Spanner serialises DDL
itself. An existing Spanner Outbox that predates a Brighter release therefore needs its new
columns added by hand — see [Per-backend differences to be aware
of](/contents/BoxProvisioning.md#per-backend-differences-to-be-aware-of).

**Option B — manage the DDL yourself.** `SpannerOutboxBuilder.GetDDL()` returns the DDL
Brighter ships, which you can drive through your own change-management tooling:

```csharp
using Paramore.Brighter.Outbox.Spanner;

public static class SpannerOutboxDdl
{
    public static string TextPayload() => SpannerOutboxBuilder.GetDDL("Outbox");

    // Body becomes BYTES(MAX) rather than STRING(MAX)
    public static string BinaryPayload() =>
        SpannerOutboxBuilder.GetDDL("Outbox", binaryMessagePayload: true);
}
```

The table is keyed on `MessageId` and uses GoogleSQL types throughout:

```sql
CREATE TABLE IF NOT EXISTS `Outbox`
(
  `MessageId` STRING(255) NOT NULL,
  `Topic` STRING(255),
  `MessageType` STRING(32),
  `Timestamp` TIMESTAMP,
  `CorrelationId` STRING(255),
  `ReplyTo` STRING(255),
  `ContentType` STRING(128),
  `PartitionKey` STRING(128),
  `Dispatched` TIMESTAMP,
  `HeaderBag` STRING(MAX),
  `Body` STRING(MAX),
  `Source` STRING(255),
  `Type` STRING(255),
  `DataSchema` STRING(255),
  `Subject` STRING(255),
  `TraceParent` STRING(255),
  `TraceState` STRING(255),
  `Baggage` STRING(MAX),
  `WorkflowId` STRING(255),
  `JobId` STRING(255),
  `DataRef` STRING(255),
  `SpecVersion` STRING(10)
) PRIMARY KEY (`MessageId`)
```

## Spanner Connection Provider

`SpannerConnectionProvider` opens the connections the Outbox reads and writes on, and
`SpannerUnitOfWork` is the transaction provider that lets your business write and the Outbox
write share one Spanner transaction. Both take the same
`IAmARelationalDatabaseConfiguration`, and `SpannerOutbox` builds a `SpannerConnectionProvider`
for you if you construct it with the configuration alone.

**Both set `EmulatorDetection.EmulatorOrProduction` on the connection string.** That means a
`SPANNER_EMULATOR_HOST` environment variable routes the connection to the emulator, and its
absence routes to production — you do not change the connection string between the two. It is
the one place the Spanner packages add a keyword to what you supply.

Two Spanner behaviours worth knowing when you read a stack trace:

- **A command timeout of zero is replaced by 60 seconds.** Spanner rejects a zero timeout, so
  the Outbox substitutes 60 rather than passing it through.
- **Duplicate keys surface as `SpannerException`** with an `AlreadyExists` status, which
  Brighter translates into its usual duplicate-message handling.

## Further Reading

- [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md) — every option this Outbox takes
- [Outbox Support](/contents/BrighterOutboxSupport.md) — what an Outbox is for and how the Sweeper dispatches from it
- [Spanner Inbox](/contents/SpannerInbox.md) — the same configuration type, one family over
- [Database Provisioning](/contents/BoxProvisioning.md) — the migration machinery, and why Spanner's runner is degenerate
- [Replay On Seen Reference](/contents/ReplayOnSeenReference.md) — the causation column an existing Spanner table needs adding by hand
