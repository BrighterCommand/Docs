---
description: "RelationalDatabaseConfiguration is the one type that configures every relational Outbox, Inbox, provisioner and queue-table transport Brighter ships."
layout:
  description:
    visible: false
---

# Relational Database Configuration Reference

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

`RelationalDatabaseConfiguration` is the one type that configures every relational Outbox,
Inbox, provisioner and queue-table transport Brighter ships. Seventeen components take it,
and none of them adds an option of its own.

That is why this page exists. A table repeated on seventeen pages is seventeen chances for
one of them to be wrong, and the wrong one is indistinguishable from the right ones to a
reader who only opens the page for the store they are using.

## Relational Database Configuration Options

The options are constructor parameters, so **the option is the parameter you type**; the
property you read back is the same word capitalised.

<!-- optioncheck: Paramore.Brighter.RelationalDatabaseConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `connectionString` | `string` | `none` | Connects to the database, in the provider's own format. |
| `databaseName` | `string?` | `"Brighter"` | Names the database holding the tables. |
| `outBoxTableName` | `string?` | `"Outbox"` | Names the Outbox table. |
| `inboxTableName` | `string?` | `"Inbox"` | Names the Inbox table; the property is `InBoxTableName`. |
| `queueStoreTable` | `string?` | `"Queue"` | Names the queue table the MSSQL and PostgreSQL transports read. |
| `schemaName` | `string?` | `null` | Qualifies the tables with a schema; the provider's own default applies when null. |
| `binaryMessagePayload` | `bool` | `false` | Stores the message body as bytes rather than as UTF-8 text. |
| `jsonMessagePayload` | `bool` | `false` | Stores the message body in the database's native JSON type. |

Only `connectionString` is required. The three table names default to `Outbox`, `Inbox` and
`Queue`, so a component that uses one of them needs no configuration beyond the connection.

`binaryMessagePayload` and `jsonMessagePayload` change the column the payload is written to,
so **changing either against an existing table is a migration, not a setting**. See
[Database Provisioning](/contents/BoxProvisioning.md).

## Which Components Take the Relational Configuration

Seventeen, across four families, measured at `10.7.0` by looking for
`IAmARelationalDatabaseConfiguration` in each package.

| Family | Provider | Page |
|---|---|---|
| Outbox | MSSQL | [MSSQL Outbox](/contents/MSSQLOutbox.md) |
| Outbox | MySQL | [MySQL Outbox](/contents/MySQLOutbox.md) |
| Outbox | PostgreSQL | [PostgreSQL Outbox](/contents/PostgresOutbox.md) |
| Outbox | SQLite | [SQLite Outbox](/contents/SqliteOutbox.md) |
| Outbox | Spanner | not yet documented |
| Inbox | MSSQL | [MSSQL Inbox](/contents/MSSQLInbox.md) |
| Inbox | MySQL | [MySQL Inbox](/contents/MySQLInbox.md) |
| Inbox | PostgreSQL | [PostgreSQL Inbox](/contents/PostgresInbox.md) |
| Inbox | SQLite | [SQLite Inbox](/contents/SqliteInbox.md) |
| Inbox | Spanner | not yet documented |
| Box provisioning | MSSQL, MySQL, PostgreSQL, SQLite, Spanner | [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) |
| Transport | PostgreSQL | [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) |
| Transport | MSSQL | [MSSQL Message Broker](/contents/MSSQLMessageBroker.md) |

**The two transports are the entries to know about.** A queue-table transport is not an
Outbox, and it is easy to assume the relational configuration reaches only the box packages —
it does not. `queueStoreTable` exists for those two and for nothing else.

## Registering the Relational Configuration

The components that take this type do not resolve it from the container by themselves:
Brighter's provisioning and sweeper hosted services do. So it is registered **once**, as a
singleton against the interface, and passed **explicitly** to each component that needs it.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.PostgreSql;

public void ConfigureServices(IServiceCollection services)
{
    var configuration = new RelationalDatabaseConfiguration(
        connectionString: DbConnectionString(),
        outBoxTableName: "Outbox");

    // Registered once, against the interface: this is what the provisioner reads
    services.AddSingleton<IAmARelationalDatabaseConfiguration>(configuration);

    services.AddBrighter()
        .AddProducers(configure =>
        {
            // ... your producer registry
            configure.Outbox = new PostgreSqlOutbox(configuration);   // passed explicitly too
            configure.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        })
        .AutoFromAssemblies();
}
```

Passing the same object twice — once to the container and once to the component — is the
shape every relational page shows, and [PostgreSQL Outbox](/contents/PostgresOutbox.md) shows
it in full.

## Further Reading

- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — where the registration above fits in a whole application
- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md) — the producers configuration that takes the Outbox this type configures
- [Database Provisioning](/contents/BoxProvisioning.md) — who creates the tables these options name
- [Outbox Support](/contents/BrighterOutboxSupport.md) — why a relational Outbox is worth the table
