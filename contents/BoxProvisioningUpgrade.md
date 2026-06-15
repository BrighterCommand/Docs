# Upgrading Existing Deployments

This page is for operators of a pre-existing Brighter deployment who are adopting [Database Provisioning](/contents/BoxProvisioning.md) for the first time. It describes what Brighter does when it starts against a table that was created and maintained by hand, what you should see in the logs, what to verify afterwards, and how to read the documented edge cases and the most likely failures.

If your application is greenfield — that is, the Outbox and Inbox tables do not yet exist — the *fresh install* path described in [Database Provisioning](/contents/BoxProvisioning.md#how-it-works) applies, and most of this page is not relevant. Read [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) instead.

## What happens on first start

The first start of your application after you enable `UseBoxProvisioning` follows the **bootstrap path**. The provisioner does not assume the table is empty, and it does not re-create it. In operator terms:

1. The provisioner connects to the database and looks for the configured table.
2. It finds the table and queries `__BrighterMigrationHistory` (or `BrighterMigrationHistory` on Spanner) for rows scoped to that table. There are none — this is a pre-existing deployment.
3. It introspects the table's columns through the database's `information_schema` (or `pragma_table_info` on SQLite) and walks the migration chain top-down until it finds the highest version whose columns are all present. That is the version Brighter originally shipped the table at.
4. It writes a single synthetic row into `__BrighterMigrationHistory` recording that version as already applied. No DDL runs at this point.
5. It then runs any subsequent migrations in order — for example, if your table was originally created at V4 and the current Brighter ships V7, the runner applies V5, V6, and V7, inserting a history row after each successful migration.
6. Every future start uses the normal path: the runner reads `MAX(MigrationVersion)` from the history table and applies any new migrations that have shipped since the last run.

The entire bootstrap path runs inside the [advisory lock](/contents/Glossary.md#advisory-lock) — so concurrent application instances starting at the same time cannot race on the synthetic history insertion.

## What gets logged

The hosted service logs at `Information` level during a successful provisioning. Watch for these lines on startup:

```text
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioning Outbox...
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioned Outbox successfully
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioning Inbox...
info: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Provisioned Inbox successfully
```

The Outbox is always provisioned before the Inbox — the Outbox is on the critical path for message production, so its failures surface first.

On PostgreSQL, if another instance currently holds the migration lock, the runner logs a retry diagnostic on each attempt:

```text
info: Paramore.Brighter.BoxProvisioning.PostgreSql.PostgreSqlBoxMigrationRunner[0]
      Waiting for migration lock on Outbox...
```

If a provisioner fails, the hosted service logs at `Error` level and then throws `ConfigurationException` — the application cannot start:

```text
fail: Paramore.Brighter.BoxProvisioning.BoxProvisioningHostedService[0]
      Failed to provision Outbox. The application cannot start without a valid
      box table. Check the database connection string and ensure the database
      is reachable.
```

## What to verify after upgrade

Once the application has started successfully, query the migration history table to confirm what was applied:

```sql
SELECT * FROM __BrighterMigrationHistory
WHERE BoxTableName = 'Outbox'
ORDER BY MigrationVersion;
```

A bootstrap of a table that was originally at V4 against a Brighter release that ships V7 produces something like:

```text
MigrationVersion | BoxTableName | AppliedAt           | Description
-----------------+--------------+---------------------+----------------------------------------
1                | Outbox       | 2026-05-26 09:14:01 | bootstrap: detected at V4
2                | Outbox       | 2026-05-26 09:14:01 | bootstrap: detected at V4
3                | Outbox       | 2026-05-26 09:14:01 | bootstrap: detected at V4
4                | Outbox       | 2026-05-26 09:14:01 | bootstrap: detected at V4
5                | Outbox       | 2026-05-26 09:14:02 | + DataRef + SpecVersion
6                | Outbox       | 2026-05-26 09:14:02 | + Source + Type + Subject
7                | Outbox       | 2026-05-26 09:14:03 | + DataSchema
```

Cross-check `MAX(MigrationVersion)` against the per-backend support matrix on [Database Provisioning](/contents/BoxProvisioning.md#per-backend-support). If the highest applied version matches `V_latest` for your backend, your table is current.

New columns added by V5–V7 on a bootstrapped table will be `NULL` for every row that already existed. That is expected and safe — every Brighter migration is additive, and Brighter accepts `NULL` for these columns on old rows. No backfill is required.

## Documented edge cases

A small number of pre-existing schemas do not slot cleanly into the migration chain. None of them block adoption, but each one is worth knowing about before you upgrade.

### Pre-#3042 message-ID column type (UNIQUEIDENTIFIER vs NVARCHAR)

Outbox tables created before Brighter PR #3042 (March 2024) store `MessageId` and `CorrelationId` as `UNIQUEIDENTIFIER` on MSSQL, MySQL, and PostgreSQL. Newer tables store them as `NVARCHAR`. The migration chain folds this type change into V4 — detection compares column names only, not types, so a pre-#3042 table is correctly detected as V4 and no migration is applied.

This is harmless for the vast majority of deployments: Brighter accepts both GUID and string IDs at the application layer. The one case worth flagging is application code that writes non-GUID message IDs (for example, a string identifier generated upstream) against a `UNIQUEIDENTIFIER` column — the database will reject the INSERT. If your application has always written GUID-shaped IDs you can ignore this; if not, you need to manually `ALTER` the column to `NVARCHAR` before adoption.

### Payload mode mismatch (binary vs text vs JSON)

The `binaryMessagePayload` flag on `IAmARelationalDatabaseConfiguration` produces a structurally different table — `VARBINARY(MAX)` instead of `NVARCHAR(MAX)` on MSSQL, `BYTEA` instead of `TEXT` on PostgreSQL, and so on. Today Brighter ships two storage modes: **text** (the default, in which JSON-serialised CloudEvents are stored as text) and **binary**. The "JSON" in the heading refers to the serialised payload that lives inside text mode — there is no separate JSON-typed column.

Changing the payload mode after the table has been created is **not supported** by the migration chain. V1 is already recorded in the history table, and the runner will not re-run it with a different DDL shape. The provisioner detects mode mismatches up front, before any migration runs:

- If the table's message-body column type does not match the configured `binaryMessagePayload` flag, the provisioner throws `ConfigurationException` at startup with a message identifying the column, the actual type, and the expected type.
- The fail-fast guard exists because storing binary data through a text column (or vice versa) silently corrupts data — far worse than failing to start.

To switch modes you must drop and recreate the table with the new mode, accepting the loss of any in-flight messages, or keep the existing mode.

### Pre-V1 (pre-2015) schemas

If your Outbox or Inbox table was created against a Brighter version that pre-dates the columns Brighter now treats as V1, the detection algorithm will not match any known version. The discriminator gate (`HeaderBag` for Outbox, `CommandBody` for Inbox) is missing, so the runner cannot bootstrap.

In practice this affects a vanishingly small number of installations — these column sets have been stable since 2015. If you hit it, your options are:

- Add the missing columns manually with `ALTER TABLE` until the table matches the V1 column set, then restart and let Brighter bootstrap it at V1.
- Drop and recreate the table, accepting the loss of in-flight messages.

### Spanner

The Spanner backend runs a degenerate provisioner — it has no migration chain and no advisory lock. There are no known production deployments. On first start against an existing Spanner table the runner applies the same discriminator gate as the other backends: if `HeaderBag` (Outbox) or `CommandBody` (Inbox) is present, the runner writes a synthetic history row at `V_latest` without migrating; if it is absent, the runner throws `ConfigurationException` with `"Spanner table exists but is not a Brighter box; check configured table name"`.

This is acceptable only when the existing table already matches `V_latest`. If you have a Spanner deployment that pre-dates the current builders, you are in untested territory — drop and recreate, or open an issue.

### Edge-case summary

| Edge case | Behaviour | What the operator should do |
|-----------|-----------|------------------------------|
| Pre-#3042 Outbox table with `UNIQUEIDENTIFIER` `MessageId` | Detected as V4. No migration applied. Brighter accepts both GUID and string IDs. | Nothing — Brighter handles both. Only worth knowing if application code stores non-GUID IDs (will throw on INSERT against the old column type). |
| Payload mode mismatch | Fail-fast at startup: `ConfigurationException`. The migration chain cannot switch modes. | Drop and recreate the table with the new payload mode, OR keep the existing mode. |
| Pre-V1 (pre-2015) schemas | Not detected by the migration chain — discriminator column is missing. | Manual `ALTER TABLE` to add missing columns, OR drop and recreate. |
| Spanner with an existing table | Degenerate runner — writes a synthetic `V_latest` history row without migrating. | Acceptable when the existing table matches `V_latest` (no known legacy Spanner installations). |

## MSSQL multi-version upgrades

The MSSQL runner wraps the entire migration chain — `sp_getapplock`, every `ALTER TABLE`, every history insert — in a single `SqlTransaction`. This produces **all-or-nothing** semantics for a single provisioning run:

- A mid-chain failure rolls back **every** migration the run touched. If V5, V6, and V7 are pending and V7 fails, V5 and V6 are also rolled back.
- The `__BrighterMigrationHistory` table will not contain partial progress. After a failure, `MAX(MigrationVersion)` reflects the last fully successful run, not the highest individual migration that succeeded in the failed run.
- PostgreSQL behaves the same way (transactional DDL). MySQL and SQLite differ: MySQL commits implicitly on each DDL statement and SQLite commits per-migration, so a mid-chain failure on those backends leaves the migrations that did succeed in the history table and the next run resumes from the next version.

This matters in one specific situation: you are upgrading MSSQL across multiple Brighter releases at once and one of the intermediate migrations fails. On a successful next attempt the chain re-runs from the last fully committed version — there is nothing to clean up — but during triage you should expect `__BrighterMigrationHistory` to show no trace of the partially-applied migrations.

Brighter is deliberately designed around this. Every V2+ UpScript is idempotent under the lock (it checks `information_schema` before issuing the `ALTER`), so re-running the chain is safe.

## Troubleshooting

The provisioner is fail-fast: any error short-circuits startup and wraps in `ConfigurationException`. The four failure modes below cover almost everything you will see in practice. Match on the message text — the strings below are quoted from the source.

### "Table {name} exists but is not a Brighter outbox/inbox"

The full message:

```text
ConfigurationException: Table {name} exists but is not a Brighter outbox/inbox
(missing discriminator column {column}); check your configured table name
```

The discriminator column is `HeaderBag` for Outbox tables and `CommandBody` for Inbox tables. This error means the provisioner found a table at the configured name, but that table has none of Brighter's identifying columns — it is not a Brighter table.

**Remediation**: verify the `OutBoxTableName` / `InBoxTableName` value on your `IAmARelationalDatabaseConfiguration` and the schema name if you set one. The provisioner is almost certainly pointed at the wrong object — perhaps a table from a previous application, or a typo on the configured name.

### "Table {name} appears to be a Brighter outbox/inbox but does not match any known schema version"

The full message:

```text
ConfigurationException: Table {name} appears to be a Brighter outbox/inbox
but does not match any known schema version; manual inspection required
```

The discriminator column is present, so the table is Brighter-shaped, but its column set does not match any V1..V_latest definition. Almost every occurrence in practice is a pre-V1 (pre-2015) schema — see [Pre-V1 (pre-2015) schemas](#pre-v1-pre-2015-schemas) above.

**Remediation**: inspect the table and reconcile its column set with the V1 definition for your backend (see the per-backend Outbox / Inbox pages linked under Further Reading). Add missing columns with `ALTER TABLE` and restart, or drop and recreate the table if losing in-flight data is acceptable.

### TimeoutException waiting for the advisory lock

The runner throws `TimeoutException` when it cannot acquire the advisory lock within `BoxProvisioningOptions.MigrationLockTimeout` (default 30 seconds). The hosted service wraps it in `ConfigurationException`. Common cases:

```text
ConfigurationException: Box provisioning failed for Outbox. See inner exception for details.
 ---> TimeoutException: Failed to acquire migration lock on Outbox within 00:00:30
```

**Remediation**:

- If you are deploying multiple application instances concurrently, the lock is doing its job — the first instance is migrating while the others wait. Increase `MigrationLockTimeout` to cover the longest migration you expect, then redeploy. See [Tuning the migration lock timeout](/contents/BoxProvisioningConfiguration.md#tuning-the-migration-lock-timeout).
- If no other instance is migrating, a previous instance may have crashed without releasing the lock. PostgreSQL and MySQL release session-scoped locks when the connection closes, so this usually resolves on its own; if not, identify the holding session and terminate it.
- On MSSQL the lock is transaction-scoped — a stuck migration is also a stuck transaction. Look for long-running transactions in `sys.dm_tran_active_transactions`.

### ConfigurationException from payload-mode mismatch

The full shape:

```text
ConfigurationException: Box provisioning failed for Outbox. See inner exception for details.
 ---> ConfigurationException: Configured binaryMessagePayload = true but column 'Body'
      on table 'Outbox' is NVARCHAR(MAX); expected VARBINARY(MAX).
```

**Remediation**: this is the [payload-mode-mismatch](#payload-mode-mismatch-binary-vs-text-vs-json) edge case. You cannot migrate between modes — pick one. Either set `binaryMessagePayload` back to what matches the existing column, or drop and recreate the table with the new mode.

## Rolling back

Brighter migrations are forward-only by design. There is no `DownScript`, no rollback runner, and no automated path to undo an applied migration.

If you need to step back to a previous Brighter version after upgrading — for example, because an application defect surfaced in production — the supported path is:

- Redeploy the previous Brighter version.
- Leave the additional columns added by V5..V_latest in place. They are nullable, additive, and ignored by the older Brighter code paths; no data is lost.
- The `__BrighterMigrationHistory` table also stays intact. If you later re-upgrade, Brighter sees the existing history rows and skips re-applying those migrations.

This is intentional. Backwards-compatible additive migrations and a redeploy-and-accept-extra-columns rollback are a much smaller failure surface than a `DownScript` runner that has to undo arbitrary DDL safely.

## Further Reading

- [Database Provisioning](/contents/BoxProvisioning.md) — the conceptual overview, support matrix, and per-backend differences.
- [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) — how to wire `UseBoxProvisioning` into your host, including `MigrationLockTimeout` tuning.
- [Glossary](/contents/Glossary.md) — definitions of [BoxProvisioning](/contents/Glossary.md#boxprovisioning), [Migration Chain](/contents/Glossary.md#migration-chain), [Migration History Table](/contents/Glossary.md#migration-history-table), [Bootstrap Path](/contents/Glossary.md#bootstrap-path), and [Advisory Lock](/contents/Glossary.md#advisory-lock).
- Per-backend Outbox pages: [MSSQL](/contents/MSSQLOutbox.md), [MySQL](/contents/MySQLOutbox.md), [PostgreSQL](/contents/PostgresOutbox.md), [SQLite](/contents/SqliteOutbox.md) — the V_latest DDL for each backend.
- Per-backend Inbox pages: [MSSQL](/contents/MSSQLInbox.md), [MySQL](/contents/MySQLInbox.md), [PostgreSQL](/contents/PostgresInbox.md), [SQLite](/contents/SqliteInbox.md).
- `Brighter/docs/adr/0057-box-schema-versioning-and-migrations.md` — versioning model, three-path runner, discriminator gate, mid-chain failure semantics.
- `Brighter/docs/adr/0053-box-database-migration.md` — hosted-service logging, MSSQL all-or-nothing transaction model, payload-mode validation.
- `Brighter/docs/adr/0061-box-provisioning-value-types.md` — the value types that replace primitives on the provisioning interfaces (a source-level change only; no effect on the operator-visible behaviour described on this page).
