# Configuring Box Provisioning

This page is the how-to companion to [Box Provisioning](/contents/BoxProvisioning.md). It shows the NuGet packages you install, the call-site shapes for `UseBoxProvisioning`, the explicit and `connectionName` registration overloads, the migration-lock-timeout knob, and the per-backend quirks. Start with [Box Provisioning](/contents/BoxProvisioning.md) if you want the conceptual overview first — this page assumes you already know what the bootstrap path is and why an advisory lock is involved.

## Prerequisites

Before adding `UseBoxProvisioning`, confirm that:

- You are already calling `services.AddBrighter(...)` in your composition root (`Program.cs` or `Startup.cs`). Box Provisioning chains off the existing `IBrighterBuilder`.
- Your application targets a backend Box Provisioning supports (MSSQL, PostgreSQL, MySQL, SQLite, or Spanner). The [per-backend support matrix](/contents/BoxProvisioning.md#per-backend-support) lists the migration chain for each.
- The database user your application connects with has `CREATE TABLE` and `ALTER TABLE` rights in the target schema. Box Provisioning issues DDL at startup; if your runtime account is locked down to `SELECT/INSERT/UPDATE/DELETE`, you cannot use Option A — see the *Option B* discussion on [Box Provisioning](/contents/BoxProvisioning.md#when-to-use-box-provisioning).
- The hosting model runs `IHostedService` implementations at startup. `Microsoft.Extensions.Hosting`-based hosts (ASP.NET Core, Generic Host, Worker Service) all do; if you have a custom composition that does not start hosted services, the provisioner will not run.

## NuGet packages

Install the core provisioning package plus one package per backend you provision:

| Backend | Package |
|---------|---------|
| _(core, always required)_ | `Paramore.Brighter.BoxProvisioning` |
| MSSQL | `Paramore.Brighter.BoxProvisioning.MsSql` |
| PostgreSQL | `Paramore.Brighter.BoxProvisioning.PostgreSql` |
| MySQL | `Paramore.Brighter.BoxProvisioning.MySql` |
| SQLite | `Paramore.Brighter.BoxProvisioning.Sqlite` |
| Spanner | `Paramore.Brighter.BoxProvisioning.Spanner` |

A typical single-backend install (MSSQL shown):

```powershell
Install-Package Paramore.Brighter.BoxProvisioning
Install-Package Paramore.Brighter.BoxProvisioning.MsSql
```

You install the per-backend package *in addition* to the backend's own Outbox / Inbox package (for example `Paramore.Brighter.Outbox.MsSql`). The provisioning package only contains the runner and the migration catalog; the Outbox and Inbox themselves still come from their existing packages.

## The call-site shape

`UseBoxProvisioning` is an extension method on `IBrighterBuilder`. It takes a single `Action<BoxProvisioningOptions>` delegate. Inside the delegate, you call one or more per-backend `Add{Backend}Outbox` / `Add{Backend}Inbox` extensions to register each box you want Brighter to manage.

### Outbox only

The simplest configuration registers a single Outbox with an explicit `RelationalDatabaseConfiguration`:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.MsSql;
using Paramore.Brighter.Extensions.DependencyInjection;

var connectionString = builder.Configuration.GetConnectionString("BrighterDb");

var outboxConfig = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    outBoxTableName: "Outbox");

services.AddBrighter()
    .AddProducers(producers =>
    {
        // ... your producer registry, transaction provider, etc.
    })
    .UseBoxProvisioning(opts =>
    {
        opts.AddMsSqlOutbox(outboxConfig);
    });
```

The `RelationalDatabaseConfiguration` you pass to `AddMsSqlOutbox` is the same configuration object you already supply to the MSSQL Outbox itself. Reuse the singleton you registered for `IAmARelationalDatabaseConfiguration` rather than constructing a second one.

### Outbox and Inbox together

If your service uses both an Outbox and an Inbox, configure both inside the same `UseBoxProvisioning` delegate:

```csharp
var outboxConfig = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    outBoxTableName: "Outbox");

var inboxConfig = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    inboxTableName: "Inbox");

services.AddBrighter()
    .AddProducers(producers => { /* ... */ })
    .UseBoxProvisioning(opts =>
    {
        opts.AddMsSqlOutbox(outboxConfig);
        opts.AddMsSqlInbox(inboxConfig);
    });
```

The order of `AddMsSqlOutbox` and `AddMsSqlInbox` inside the delegate does not matter — the hosted service always provisions every Outbox before any Inbox at startup (see [Startup ordering](#startup-ordering)). You can register multiple Outboxes and multiple Inboxes in the same delegate if your application provisions several boxes (for example, a per-tenant table name).

Outbox and Inbox configurations are independent. If they live in the same database they will share the single `__BrighterMigrationHistory` table; if they live in different databases each gets its own.

## Resolving connection strings at runtime (.NET Aspire and IConfiguration)

Some hosts populate `IConfiguration` after the DI container is built — most notably .NET Aspire, which discovers connection strings from the AppHost orchestrator at runtime. In that case the explicit `RelationalDatabaseConfiguration` overload does not work, because you do not yet have a connection string to put in it.

Every per-backend extension ships a second overload that takes a `connectionName` instead of a configuration object. The provisioner resolves the connection string from `IConfiguration.GetConnectionString(connectionName)` when the registration actually runs, not when you register it:

```csharp
services.AddBrighter()
    .AddProducers(producers => { /* ... */ })
    .UseBoxProvisioning(opts =>
    {
        opts.AddMsSqlOutbox(
            connectionName: "BrighterDb",
            outboxTableName: "Outbox");
    });
```

`connectionName` is the name you used in `appsettings.json`, in the Aspire AppHost `WithConnectionString`/`AddConnectionString` call, or wherever else your host writes connection strings. If the configuration entry does not exist at the time the hosted service runs, the provisioner throws `InvalidOperationException` with the message `Connection string '{name}' not found in configuration.` — fix the configuration source, not the code.

The `connectionName` overload accepts the same per-box parameters you would have set on `RelationalDatabaseConfiguration` (`outboxTableName`, `schemaName` where the backend supports it, `binaryMessagePayload`); each defaults sensibly so you can usually pass just the name.

## Tuning the migration lock timeout

`BoxProvisioningOptions.MigrationLockTimeout` controls how long a replica is willing to wait for the per-table advisory lock during startup. The default is 30 seconds. Override it inside the same delegate, before or after the `Add{Backend}*` calls — the timeout is read late, when registrations actually run, so placement inside the delegate does not matter:

```csharp
services.AddBrighter()
    .UseBoxProvisioning(opts =>
    {
        opts.MigrationLockTimeout = TimeSpan.FromMinutes(2);
        opts.AddMsSqlOutbox(outboxConfig);
        opts.AddMsSqlInbox(inboxConfig);
    });
```

Each backend's underlying lock primitive uses a different unit; Brighter converts your `TimeSpan` to the right shape for the backend at call time:

| Backend | Underlying lock primitive | Unit the backend expects | Brighter's conversion |
|---------|---------------------------|--------------------------|------------------------|
| MSSQL | `sp_getapplock` | milliseconds | `(int)timeout.TotalMilliseconds` |
| PostgreSQL | `pg_try_advisory_lock` (retry loop) | milliseconds — total retry budget | `(int)timeout.TotalMilliseconds` |
| MySQL | `GET_LOCK` | whole seconds | `(int)timeout.TotalSeconds`, rounded up to a minimum of 1 second |
| SQLite | `BEGIN IMMEDIATE` (file-level lock) | whole seconds | as MySQL — rounded up to a minimum of 1 second |
| Spanner | n/a (DDL is serialised by the Spanner service) | n/a | timeout ignored |

The MySQL and SQLite minimum of 1 second matters in two ways. Sub-second `TimeSpan` values (for example, `TimeSpan.FromMilliseconds(500)`) are rounded up to a 1-second wait on contention, so true sub-second precision is unavailable on those backends. And `TimeSpan.Zero` is *not* fail-fast on MySQL or SQLite: it rounds up to 1 second, just like any other sub-second value. If you want fail-fast behaviour, use MSSQL or PostgreSQL.

Match the timeout to your readiness-probe budget. The worst-case startup wait for one replica is approximately `MigrationLockTimeout × (N − 1) × T`, where *N* is the number of replicas racing the same startup and *T* is the number of tables the host provisions. On Kubernetes, size `initialDelaySeconds + (failureThreshold × periodSeconds)` to cover that window; on Azure App Service or similar, raise the cold-start / startup-probe timeout by the same budget. Shorten `MigrationLockTimeout` only if you accept more lock-contention retries — never so short that a legitimate first-run migration cannot finish.

## Startup ordering

`BoxProvisioningHostedService` runs once at host startup, before traffic is accepted. It iterates the registered provisioners in two phases:

1. **All Outboxes first.** Outboxes are on the critical path of `DepositPost` — a Brighter call that cannot find its Outbox table fails immediately. Provisioning Outboxes first ensures that if anything later breaks, the Outbox is already migrated and ready.
2. **All Inboxes second.** Inboxes are consumed by message handlers and are not touched until the Dispatcher starts pulling messages, which happens after `StartAsync` returns. Provisioning them in the second phase keeps Outbox-only failures from being mixed up with Inbox-only failures in logs.

Within each phase, provisioners run sequentially in registration order. The phases are not parallelised; if you provision an Outbox and an Inbox on the same database, the Inbox waits for the Outbox to finish.

If any provisioner throws, the hosted service wraps the exception in `ConfigurationException` and the host fails to start. This is deliberate: a service whose Outbox is missing or stale would silently lose messages, so failing fast is the safer default. The wrapped exception's `InnerException` carries the original error (typically a `SqlException`, `NpgsqlException`, or the underlying provider's failure type) — your logging should surface both layers. The exception is *not* wrapped if startup is cancelled — a cancellation token firing during `StartAsync` propagates as `OperationCanceledException` unchanged.

## Per-backend notes

The shape of every backend's extension is the same — two overloads (explicit configuration and `connectionName`), one for Outbox and one for Inbox — but the parameters and defaults differ.

### MSSQL

Two registration shapes (Outbox and Inbox have the same two-overload pair):

```csharp
opts.AddMsSqlOutbox(rdbmsConfiguration);
opts.AddMsSqlOutbox("BrighterDb",
    outboxTableName: "Outbox",
    schemaName: "dbo",
    binaryMessagePayload: false);
```

`schemaName` defaults to the MSSQL default (`dbo`) when omitted. `binaryMessagePayload: true` switches `Body` to `VARBINARY(MAX)` — set this to match how your Outbox was created. Trying to switch payload modes after a table exists throws `ConfigurationException` at startup; see [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#payload-mode-mismatch-binary-vs-text-vs-json).

MSSQL upgrades are all-or-nothing: a mid-chain failure rolls back every migration applied in that run. This matters when you skip several Brighter versions in a single deploy — see [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#mssql-multi-version-upgrades).

### PostgreSQL

```csharp
opts.AddPostgreSqlOutbox(rdbmsConfiguration);
opts.AddPostgreSqlOutbox("BrighterDb",
    outboxTableName: "Outbox",
    schemaName: "public",
    binaryMessagePayload: false);
```

`schemaName` defaults to `public`. PostgreSQL commits per migration, so a mid-chain failure leaves earlier migrations applied; the runner re-checks state under the lock and re-applies only what is still outstanding on the next start.

The PostgreSQL Inbox is V1-only — see the [Box Provisioning support matrix](/contents/BoxProvisioning.md#per-backend-support). The registration shape is identical to the Outbox; the difference is purely that there are no V2+ migrations to run.

### MySQL

```csharp
opts.AddMySqlOutbox(rdbmsConfiguration);
opts.AddMySqlOutbox("BrighterDb",
    outboxTableName: "Outbox",
    schemaName: "brighter",
    binaryMessagePayload: false);
```

MySQL 8.0 or later only — earlier versions are unsupported. `schemaName` corresponds to the MySQL database name; if you do not pass one the connection's default database is used. MySQL commits per migration, like PostgreSQL.

Remember the 1-second minimum on `MigrationLockTimeout` (see [Tuning the migration lock timeout](#tuning-the-migration-lock-timeout)).

### SQLite

SQLite has an additional `enableWalMode` parameter on every overload:

```csharp
opts.AddSqliteOutbox(rdbmsConfiguration, enableWalMode: true);
opts.AddSqliteOutbox("BrighterDb",
    outboxTableName: "Outbox",
    binaryMessagePayload: false,
    enableWalMode: true);
```

When `enableWalMode` is `true` (the default) the runner issues `PRAGMA journal_mode=WAL` on each migration call. WAL mode is the recommended setting for any SQLite database that handles concurrent reads and writes, including Brighter's Outbox. Set `enableWalMode: false` only if your host already manages SQLite journal mode itself — the pragma is database-file-wide and would override any `DELETE`/`TRUNCATE` choice the host made.

There is no `schemaName` parameter: SQLite has no schema concept. Migrations serialise via SQLite's file-level locking — long chains briefly block readers, which is acceptable for the dev/test workloads SQLite targets. The 1-second minimum on `MigrationLockTimeout` applies here too.

### Spanner

```csharp
opts.AddSpannerOutbox(rdbmsConfiguration);
opts.AddSpannerOutbox("BrighterDb",
    outboxTableName: "Outbox",
    binaryMessagePayload: false);
```

Spanner uses the *degenerate runner* — see [Box Provisioning](/contents/BoxProvisioning.md#per-backend-differences-to-be-aware-of). The runner can create the table on a fresh database but cannot evolve an existing one. There is no `schemaName` parameter (Spanner does not use schemas the way relational backends do), and `MigrationLockTimeout` is ignored because the runner does not use advisory locks — DDL serialisation is handled by the Spanner service.

## Common pitfalls

- **Forgetting the per-backend package.** Installing only `Paramore.Brighter.BoxProvisioning` leaves you with no `Add{Backend}*` extensions in scope. The compiler error names the missing method, but it can be confusing if you have the core package referenced and assume that is enough — every backend you provision needs its own package.
- **Calling `UseBoxProvisioning` more than once on the same builder.** A second invocation throws `ConfigurationException` with a message naming the duplicate call. Configure every Outbox and Inbox inside a single `UseBoxProvisioning(opts => { … })` delegate — the framework guards against this because a second invocation would double-register every provisioner and the hosted service would run each migration twice.
- **Using the `connectionName` overload when you are not on Aspire.** It still works — `GetConnectionString` reads from `IConfiguration` regardless of how the configuration was populated — but you give up the early-bound safety of the explicit overload. If you already have a `RelationalDatabaseConfiguration` singleton, pass it directly. The `connectionName` form exists for the case where the connection string is not knowable when DI is being built.
- **Pointing the provisioner at a read replica.** The runner needs `CREATE TABLE` / `ALTER TABLE` rights on the connection it is given. If your application normally reads from a follower and writes to a primary, configure Box Provisioning with the primary's connection string explicitly — do not let it default to your application's main connection if that connection is read-only.
- **Mixing payload modes between Option B and Option A.** If a table was originally created via `*OutboxBuilder.GetDDL(hasBinaryMessagePayload: true)` and you then register `Add{Backend}Outbox` with `binaryMessagePayload: false`, the payload-mode validator throws `ConfigurationException` at startup. Either match the mode in the configuration object or archive the existing table.
- **Setting `MigrationLockTimeout = TimeSpan.Zero` expecting fail-fast on MySQL/SQLite.** It rounds up to 1 second on those backends, so you still wait. Use MSSQL or PostgreSQL if you genuinely need fail-fast (0 ms) behaviour.

## Further Reading

- [Box Provisioning](/contents/BoxProvisioning.md) — what Box Provisioning is, how the three-path runner works, the migration history table, the per-backend support matrix.
- [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) — what operators see on first start, error messages, MSSQL all-or-nothing semantics, payload-mode mismatch remediation.
- Per-backend Outbox pages: [MSSQL](/contents/MSSQLOutbox.md) · [MySQL](/contents/MySQLOutbox.md) · [PostgreSQL](/contents/PostgresOutbox.md) · [SQLite](/contents/SqliteOutbox.md).
- Per-backend Inbox pages: [MSSQL](/contents/MSSQLInbox.md) · [MySQL](/contents/MySQLInbox.md) · [PostgreSQL](/contents/PostgresInbox.md) · [SQLite](/contents/SqliteInbox.md).
- Sample: `Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs:116` — the canonical `UseBoxProvisioning` call-site in the WebAPI Dapper sample. (The sample wraps the per-backend call in a small `BoxProvisioningFactory` because it supports multiple backends at runtime; in your application you call the per-backend extension directly, as the examples above show.)
- ADRs: `Brighter/docs/adr/0053-box-database-migration.md` (architecture, hosted service, package layout) and `Brighter/docs/adr/0057-box-schema-versioning-and-migrations.md` (advisory-lock design, migration runner).
