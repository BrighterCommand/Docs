---
description: "The Spanner Inbox records the messages a consumer has already handled in a Google Cloud Spanner table, so a redelivery is recognised rather than reprocessed."
layout:
  description:
    visible: false
---

# Spanner Inbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Inbox Support](/contents/BrighterInboxSupport.md)

The Spanner Inbox records the messages a consumer has already handled in a Google Cloud Spanner table, so a redelivery is recognised rather than reprocessed. It is configured entirely through the shared relational configuration.

## Spanner Inbox Configuration

```powershell
dotnet add package Paramore.Brighter.Inbox.Spanner
dotnet add package Paramore.Brighter.Spanner
```

**The class is `SpannerInboxAsync`, and there is no synchronous twin.** It implements
Brighter's inbox interfaces through `RelationalDatabaseInbox`, so it is used exactly like any
other Inbox; only the name is different.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Spanner;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public static class SpannerInboxRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new RelationalDatabaseConfiguration(
            connectionString: "Data Source=projects/my-project/instances/my-instance/databases/brighter",
            inboxTableName: "Inbox");

        services.AddSingleton<IAmARelationalDatabaseConfiguration>(configuration);

        services.AddConsumers(options =>
        {
            options.InboxConfiguration = new InboxConfiguration(
                new SpannerInboxAsync(configuration),
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Warn);
            // ... your subscriptions and channel factory
        });
    }
}
```

Constructed with the configuration alone, `SpannerInboxAsync` builds its own
`SpannerConnectionProvider`; a second constructor takes an `IAmARelationalDbConnectionProvider`
when you want to share one.

## Spanner Inbox Options

**The Spanner Inbox has no options type of its own.** It takes an
`IAmARelationalDatabaseConfiguration`, the type every relational Outbox, Inbox, provisioner and
queue-table transport takes, and its options are documented once at
[Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md#relational-database-configuration-options).
That is why this page has no table: there is no Spanner Inbox surface to tabulate.

The option that matters here is **`inboxTableName`**, which defaults to `Inbox`. Note the
asymmetry the reference page records — the constructor parameter is `inboxTableName` and the
property you read back is `InBoxTableName`, with a capital `B`.

`schemaName` is not used: Spanner does not organise tables into schemas the way the other
relational backends do, and its provisioning extensions take no `schemaName` argument.

## Provisioning the Spanner Inbox

You have the same two options every relational Inbox has.

**Option A — let Brighter provision it.** `AddSpannerInbox` registers a provisioner that
creates the table at startup; see [Configuring Box
Provisioning](/contents/BoxProvisioningConfiguration.md#spanner). Spanner uses the **degenerate
runner** — it creates the table on a fresh database and cannot evolve an existing one, so an
Inbox table created by an earlier Brighter release needs its new columns added by hand. See
[Per-backend differences to be aware
of](/contents/BoxProvisioning.md#per-backend-differences-to-be-aware-of), and
[Replay On Seen Reference](/contents/ReplayOnSeenReference.md) for the causation column that
[replay on seen](/contents/ReplayOnSeen.md) needs on both boxes.

**Option B — manage the DDL yourself.** `SpannerInboxBuilder.GetDDL()` returns the DDL Brighter
ships:

```csharp
using Paramore.Brighter.Inbox.Spanner;

public static class SpannerInboxDdl
{
    public static string Ddl() => SpannerInboxBuilder.GetDDL("Inbox");
}
```

The table is keyed on `(CommandId, ContextKey)`, so the same command handled by two different
handlers is two rows rather than a collision:

```sql
CREATE TABLE IF NOT EXISTS `Inbox`(
    `CommandId` STRING(256) NOT NULL,
    `CommandType` STRING(256),
    `CommandBody` JSON,
    `Timestamp` TIMESTAMP,
    `ContextKey` STRING(256)
) PRIMARY KEY (`CommandId`, `ContextKey`)
```

Duplicate inserts surface as a `SpannerException` carrying an `AlreadyExists` status, which the
Inbox translates into the `OnceOnlyAction` you configured.

## Further Reading

- [Inbox Support](/contents/BrighterInboxSupport.md) — what an Inbox is for and when you need one
- [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md) — every option this Inbox takes
- [Spanner Outbox](/contents/SpannerOutbox.md) — the same configuration type, one family over
- [Database Provisioning](/contents/BoxProvisioning.md) — the migration machinery, and why Spanner's runner is degenerate
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox) — where `InboxConfiguration` fits
