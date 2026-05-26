# Box Provisioning

[BoxProvisioning](/contents/Glossary.md#boxprovisioning) is the Brighter library that creates and migrates your [Outbox](/contents/BrighterOutboxSupport.md) and [Inbox](/contents/BrighterInboxSupport.md) tables at application startup. You register it once with `services.AddBrighter().UseBoxProvisioning(...)`; on every start it inspects the database, applies any outstanding schema changes under a backend-level [advisory lock](/contents/Glossary.md#advisory-lock), and records what it did. This page explains what BoxProvisioning is, how it makes decisions, and what to expect operationally. The companion page [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) shows the call-site shapes.

## When to use Box Provisioning

You have two equally-supported ways to create and evolve the Outbox and Inbox tables:

- **Option A — Box Provisioning (this library).** Brighter manages the DDL: it creates the table on first start, applies new migrations as you upgrade Brighter, and tracks what's been applied in the migration history table. Suits greenfield services, container-orchestrated deployments, services that own their own database, and any environment where you are happy for the application to alter its own message-infrastructure tables at startup.
- **Option B — manage the DDL yourself.** You keep using the existing `*OutboxBuilder` / `*InboxBuilder` static classes — call `GetDDL()` from your own code, capture the SQL, and apply it through your own change-management pipeline (FluentMigrator, Flyway, Liquibase, dbatools, hand-written SQL reviewed in a pull request, whatever your operations team already runs). Suits regulated environments, governed change windows, deployments where the runtime service account does not have `CREATE TABLE` / `ALTER TABLE` rights, shared databases owned by another team, and any context where schema change is a ticketed, reviewed event.

Neither option is deprecated. Choose based on fit, not preference: if your platform expects services to bring their own infrastructure up at boot, use Option A; if your platform expects DDL to be released through a separate change pipeline, use Option B. You can also mix the two — for example, run Option A in development and CI (fast iteration) and Option B in production (governed change). The on-disk table shape is identical, so a service started under Option B can later switch to Option A without further migration; the first start under Option A simply runs the bootstrap path, detects the existing table, and stamps the migration history.

A few signals that point firmly at one option over the other:

- *You are deploying behind Kubernetes / Aspire / similar*, and replicas come and go on their own schedule → Option A. The advisory-lock coordination across replicas is exactly the problem BoxProvisioning was built to solve.
- *Your DBAs require change tickets for every DDL statement* → Option B. Generate the SQL from `*Builder.GetDDL()`, file the ticket, and apply through the change-management pipeline.
- *Your service account is locked down to `SELECT/INSERT/UPDATE/DELETE`* → Option B. BoxProvisioning needs `CREATE TABLE` and `ALTER TABLE` rights; if you cannot grant those, Option A is not an option.
- *You are upgrading an existing application that already has an Outbox / Inbox* → either works. Under Option A you will hit the bootstrap path on first start (see below); under Option B you apply the new version's DDL the same way you applied the original.

## How it works

When the host starts, the `BoxProvisioningHostedService` runs before traffic is accepted. For every Outbox or Inbox you registered (Outboxes first, then Inboxes — Outboxes are on the critical path of `DepositPost`), the matching provisioner:

1. Connects to the database and inspects the target table.
2. Decides which of three paths to take based on what it finds.
3. Acquires the advisory lock so that no other replica racing the same startup can run DDL concurrently.
4. Re-checks state under the lock (closing the time-of-check / time-of-use race that a bare `CREATE TABLE` would otherwise have).
5. Applies whatever DDL the chosen path requires.
6. Records the result in the [migration history table](/contents/Glossary.md#migration-history-table).
7. Releases the lock.

If any step fails, the host is not allowed to finish startup — the provisioner throws and the failure surfaces as `ConfigurationException`. This is deliberate: a service whose Outbox is missing or stale would silently lose messages. Failing fast is the safer default.

### The three paths

The runner branches on what is already in the database — the same code handles all three cases, but the work it does is different in each:

- **Fresh install** — the table does not exist. The runner creates the table at the *current* (latest) shape and stamps one history row recording that this database started at the latest version. Older migrations are never replayed. You hit this path on the first start of a brand-new service in a brand-new database, or when you provision an Outbox / Inbox for a previously-unused table name.
- **[Bootstrap path](/contents/Glossary.md#bootstrap-path)** — the table exists but the migration history table has no rows for it. This is what you hit the first time you adopt BoxProvisioning on a service that previously created its Outbox or Inbox using the static `*OutboxBuilder` / `*InboxBuilder` helpers. The runner inspects the actual columns of the existing table, works out which version the column set corresponds to, stamps history at that version, and then applies only the migrations newer than that version. The existing data is untouched — bootstrap only adds the missing audit row.
- **Normal migration** — the table and history both exist. The runner reads `MAX(MigrationVersion)` from history for that table, and applies the ordered list of migrations above that version. Each migration's `UpScript` is written to be idempotent, so re-running it on a partially-migrated database (for example, after a previous startup crashed mid-chain) is safe.

In all three paths the end-state is the same: the table is at the latest shipped version and history records the path that got it there. The operation is *idempotent* — restarting a replica that has already finished provisioning is a no-op beyond a lock acquire and release.

The bootstrap path also enforces a *discriminator check* before stamping history. Brighter looks for a column whose presence is strong evidence that the table really is a Brighter box — `HeaderBag` for an Outbox, `CommandBody` for an Inbox. If the discriminator is missing, the runner refuses to stamp history and throws — the operator has almost certainly pointed Brighter at the wrong table name. If the discriminator is present but no known version matches the column set, the runner also throws (this would indicate a hand-edited or otherwise unexpected schema). See [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) for the exact error text and remediation.

The asymmetry between "fresh install" and "bootstrap" matters because of *born-past-V1* backends. Some backends — the PostgreSQL Outbox, every Inbox — shipped with their final V1 column set already including columns that other backends only added later. For these, the bootstrap path detects a "V2-shaped" table on first contact and replays nothing; the migration chain is shorter than the V1..V_latest range you might expect. The per-backend `*MigrationCatalog` source records exactly which version each historical first-shipped DDL maps to.

### The migration history table

BoxProvisioning records every applied migration in a per-database table called `__BrighterMigrationHistory` (on Spanner the table is `BrighterMigrationHistory` — without the leading underscores, because Spanner identifier rules forbid them). One row per `(schema, table, version)` tuple.

```sql
CREATE TABLE [__BrighterMigrationHistory] (
    [MigrationVersion] INT NOT NULL,
    [SchemaName]       VARCHAR(256) NOT NULL DEFAULT 'dbo',
    [BoxTableName]     VARCHAR(256) NOT NULL,
    [Description]      NVARCHAR(512) NOT NULL,
    [AppliedAt]        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_BrighterMigrationHistory
        PRIMARY KEY ([SchemaName], [BoxTableName], [MigrationVersion])
);
```

The MSSQL DDL is shown above; the other backends use the same logical shape with backend-native column types (`VARCHAR(256)` becomes `TEXT` on SQLite, `VARCHAR` on PostgreSQL, and so on). One history table is shared across every Outbox and Inbox in the same database — the composite primary key `(SchemaName, BoxTableName, MigrationVersion)` makes that safe even when one database holds many boxes.

Each column has an operator-visible purpose:

- `MigrationVersion` — the integer version the row records. `1` for V1 (bootstrap or fresh install on a born-past-V1 backend), counting up from there.
- `SchemaName` — the database schema the box lives in (`dbo` on MSSQL, `public` on PostgreSQL, the database name on SQLite, and so on). Set to the backend's default schema when not otherwise specified.
- `BoxTableName` — the table name (`Outbox`, `Inbox`, `tenant_1_Outbox`, whatever you configured).
- `Description` — a human-readable note recording how the row was inserted: `"fresh install at V7"`, `"bootstrap: detected at V4"`, or the migration's own description string (e.g. `"V5: add CloudEvents columns"`). Useful when triaging an upgrade — it tells you whether a row came from the fresh path, the bootstrap path, or a real chain replay.
- `AppliedAt` — UTC timestamp set by the database default (`GETUTCDATE()` on MSSQL, `NOW()` on PostgreSQL, and so on). Useful for correlating against deploy times.

For day-to-day operations, treat the history table as read-only audit data. You can `SELECT` from it to confirm which migrations have run on a given environment (the [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) page shows the query); you should not delete or rewrite rows. Deleting a row would cause the runner to re-apply that migration on the next startup, which is safe but pointless.

## Concurrency and multi-instance startup

In a horizontally-scaled deployment (multiple Kubernetes pods, multiple App Service instances, a rolling deploy) every replica runs the hosted service at startup. Without coordination they would race each other to `CREATE TABLE` or `ALTER TABLE`.

BoxProvisioning serialises that work using a backend-level [advisory lock](/contents/Glossary.md#advisory-lock) — a database-side primitive that lets one connection hold an exclusive lock on a named resource while the others wait. The lock name is scoped to the specific table (`BrighterMigration_{schema}.{table}`), so an Outbox and an Inbox in the same database do not block each other; nor do two different tenants' boxes if you have provisioned them under different table names. Each backend uses its native primitive (`sp_getapplock`, `pg_try_advisory_lock`, `GET_LOCK`, `BEGIN IMMEDIATE`); the differences are documented in the per-backend table below.

While a replica holds the lock, others block — they retry until they acquire it, find no migrations are outstanding, and finish. The default wait budget is 30 seconds per table, tunable via `BoxProvisioningOptions.MigrationLockTimeout` (see [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md#tuning-the-migration-lock-timeout)).

**Readiness probes**: while a replica is waiting on the lock its HTTP endpoints are not yet responding. The worst-case wait window for one replica is approximately `MigrationLockTimeout × (N − 1) × T`, where *N* is the number of replicas racing the same startup and *T* is the number of tables the host provisions. With the default 30-second timeout, one outbox + one inbox, and four replicas in a rolling deploy, that's a worst case of ~3 minutes — though in practice almost every replica beyond the first finds nothing to do and finishes within milliseconds of acquiring the lock.

On Kubernetes, size your readiness probe's `initialDelaySeconds` and `failureThreshold × periodSeconds` so that this window does not trigger a restart loop. On App Service or similar platforms, raise the startup probe / cold-start timeout by the same budget. If the window is genuinely a problem, shorten `MigrationLockTimeout` and accept more lock-contention retries — but do not set it so short that a legitimate first-run migration cannot finish.

## Per-backend support

| Backend | NuGet package | Outbox versions | Inbox versions | Advisory-lock primitive |
|---------|---------------|-----------------|----------------|-------------------------|
| MSSQL | `Paramore.Brighter.BoxProvisioning.MsSql` | V1..V7 | V1..V2 | `sp_getapplock` |
| PostgreSQL | `Paramore.Brighter.BoxProvisioning.PostgreSql` | V1..V7 | V1 only | `pg_try_advisory_lock` |
| MySQL | `Paramore.Brighter.BoxProvisioning.MySql` | V1..V7 | V1..V2 | `GET_LOCK` |
| SQLite | `Paramore.Brighter.BoxProvisioning.Sqlite` | V1..V7 | V1..V2 | `BEGIN IMMEDIATE` |
| Spanner | `Paramore.Brighter.BoxProvisioning.Spanner` | fresh-install-only | fresh-install-only | n/a (DDL serialised by Spanner) |

Per-backend Outbox pages: [MSSQL](/contents/MSSQLOutbox.md), [MySQL](/contents/MySQLOutbox.md), [PostgreSQL](/contents/PostgresOutbox.md), [SQLite](/contents/SqliteOutbox.md). Per-backend Inbox pages: [MSSQL](/contents/MSSQLInbox.md), [MySQL](/contents/MySQLInbox.md), [PostgreSQL](/contents/PostgresInbox.md), [SQLite](/contents/SqliteInbox.md). Each of these pages shows the Option A and Option B shapes for that backend.

The "Outbox versions" and "Inbox versions" columns refer to the ordered [migration chain](/contents/Glossary.md#migration-chain). V1 is the historical first-shipped DDL for that backend; later versions add columns over time as Brighter's message model has grown (for example, V4 added `PartitionKey`; V5 added the CloudEvents columns; V7 added `DataRef` and `SpecVersion`). Each version's full column list is recorded in the backend's `*MigrationCatalog` source, alongside the PR and commit that introduced it.

## Per-backend differences to be aware of

| Backend | Asymmetry | What it means for operators |
|---------|-----------|------------------------------|
| MSSQL | All-or-nothing multi-version upgrade | A mid-chain failure rolls back *all* migrations in that run. See [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#mssql-multi-version-upgrades). |
| PostgreSQL | Inbox is V1-only | The Postgres Inbox shipped with its final column set in 2021; no V2 exists. The chain is intentionally shorter. |
| MySQL | Minimum 8.0 | Earlier MySQL versions are not supported. |
| SQLite | File-level locking only | Long migration chains block readers. Acceptable for the dev/test use case SQLite targets. |
| Spanner | Fresh-install-only (degenerate runner) | The runner can create the table but cannot evolve an existing table. Track-record: no known production deployments. |

These differences are not bugs — they reflect each backend's native semantics or the historical shape of the code that already shipped:

- **MSSQL's all-or-nothing rule** follows from MSSQL's single-transaction-spans-the-whole-run model, which is how MSSQL gives you transactional DDL at all. The runner wraps the lock-acquire, every migration's DDL, and every history-row insert in one transaction. A failure at migration *N* rolls back migrations *1..N-1* applied in the same run, which is consistent with MSSQL's general transactional-DDL behaviour. PostgreSQL, MySQL, and SQLite commit per-migration, so a mid-chain failure there leaves the earlier migrations intact. The MSSQL semantics matter mostly when you skip several Brighter versions at once; see [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#mssql-multi-version-upgrades) for the operator-facing detail.
- **The PostgreSQL inbox is V1-only** because the PostgreSQL Inbox was introduced in 2021 and shipped with `ContextKey` and the composite primary key from its very first commit. The MSSQL / MySQL / SQLite Inbox V2 migration added `ContextKey` to tables that pre-existed without it; PostgreSQL never had that earlier shape, so there is nothing to migrate. Not a missing feature — just a shorter chain.
- **MySQL's 8.0 minimum** reflects the underlying need for `JSON` and modern `INFORMATION_SCHEMA.COLUMNS` behaviour. MySQL 5.7 is end-of-life from Oracle and unsupported.
- **SQLite's file-level locking** is the SQLite design; long migrations on a high-throughput SQLite database would briefly block readers. In practice SQLite serves dev, test, and small-deployment workloads, where this is acceptable.
- **Spanner's degenerate runner** reflects the fact that no production Brighter installation has ever run on Spanner. The runner creates the table on a fresh database but does not attempt to evolve an existing one; if that ever changes, a full Spanner migration chain can be added without breaking the abstraction.

## What Box Provisioning does NOT do

BoxProvisioning is scoped narrowly. It does **not**:

- **Create or migrate your application's domain tables.** BoxProvisioning only manages Outbox and Inbox tables. Your application's `Orders`, `Customers`, or `Inventory` tables remain your responsibility — keep using whatever migration tool you use today (EF Core migrations, FluentMigrator, Liquibase, hand-written SQL applied through your release pipeline). See [Outbox Pattern](/contents/OutboxPattern.md) for the boundary between application data and the Outbox.
- **Manage your application-and-outbox transaction.** The Transactional Outbox pattern requires the Outbox `INSERT` and your business writes to share a transaction. BoxProvisioning gives you the Outbox table; the transaction itself is wired up via your `IAmATransactionConnectionProvider` registration. The two concerns are orthogonal — provisioning runs once at startup, the transaction runs on every `DepositPost`.
- **Create transport / queue tables.** Tables created by `MsSqlQueueBuilder` and similar transport-side helpers — the queue/topic shadow tables used by some brokers — are not part of the Box Provisioning surface. They remain managed by the transport's own setup code.
- **Migrate between payload modes.** A table created in text mode (`Body` stored as `NVARCHAR(MAX)` / `TEXT`) cannot be converted in place to binary mode (`VARBINARY(MAX)` / `BYTEA`) or to JSON / JSONB — the encodings are incompatible. The `*PayloadModeValidator` classes detect a mismatch at startup and throw `ConfigurationException`. To switch payload modes, archive the existing table and create a new one under a different name.
- **Resurrect pre-2015 schemas.** V1 is the baseline; anything older than V1 is treated as "not a Brighter box" and refused. If you have a long-lived deployment whose Outbox predates the V1 baseline, drop and recreate the table from a known-good DDL before adopting BoxProvisioning.
- **Run on read replicas.** The provisioner needs `CREATE TABLE` / `ALTER TABLE` rights on the connection it's given. Point it at the writable primary, not at a read-only follower. If your application normally reads from a follower and writes to a primary, configure BoxProvisioning with the primary's connection string explicitly.
- **Replace your back-up strategy.** Provisioning rolls forward; it does not roll back. If a deployment ships a broken migration, you recover from your database back-up, not from BoxProvisioning. Plan accordingly.

## Source-breaking changes on upgrade

The first release of BoxProvisioning ships two source-breaking interface changes. These are only relevant if you *implement* the affected interfaces yourself (most applications consume Brighter's built-in implementations and are unaffected).

- **`IAmARelationalDatabaseConfiguration.SchemaName`** is now an abstract member of the interface (previously a property on the concrete class only). If you implement `IAmARelationalDatabaseConfiguration` directly — for example to surface a custom configuration source — add a `public string? SchemaName => null;` member to preserve the pre-change behaviour. The provisioner treats `null` as "use the backend default schema" (`dbo` for MSSQL, `public` for PostgreSQL, and so on). The default-interface-member form is not available because `Paramore.Brighter` targets `netstandard2.0`.
- **`IAmABoxMigration`** gains one required member (`ISet<string> LogicalColumns`) and two optional members (`string? SourceReference`, `string? IdempotencyCheckSql`). If you author your own migration classes — for example, to ship a private extension column — implement the new members on recompile. The vast majority of users consume Brighter's built-in migrations and never touch this interface.

If you only call `AddBrighter().UseBoxProvisioning(...)` and the built-in `Add{Backend}Outbox` / `Add{Backend}Inbox` extensions, neither change affects you.

## Further Reading

- [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) — the call-site shapes (`UseBoxProvisioning`, `Add{Backend}Outbox`, `connectionName` overloads, lock-timeout tuning).
- [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) — what operators see on first start, how to read `__BrighterMigrationHistory`, error messages and remediation.
- [Outbox Support](/contents/BrighterOutboxSupport.md) and [Inbox Support](/contents/BrighterInboxSupport.md) — the Outbox / Inbox features themselves.
- Per-backend Outbox pages: [MSSQL](/contents/MSSQLOutbox.md) · [MySQL](/contents/MySQLOutbox.md) · [PostgreSQL](/contents/PostgresOutbox.md) · [SQLite](/contents/SqliteOutbox.md).
- Per-backend Inbox pages: [MSSQL](/contents/MSSQLInbox.md) · [MySQL](/contents/MySQLInbox.md) · [PostgreSQL](/contents/PostgresInbox.md) · [SQLite](/contents/SqliteInbox.md).
- [Outbox Pattern](/contents/OutboxPattern.md) — the underlying messaging pattern and the boundary between application data and the Outbox.
- ADRs: `Brighter/docs/adr/0053-box-database-migration.md` (architecture, hosted service, package layout) and `Brighter/docs/adr/0057-box-schema-versioning-and-migrations.md` (versioning model, three-path runner, advisory-lock design).
- If you are adding a new backend or a new column to an existing migration chain, see the implementor guides in the Brighter repository under `docs/guides/box-provisioning-*.md` — those guides are scoped to Brighter contributors, not consumers.
