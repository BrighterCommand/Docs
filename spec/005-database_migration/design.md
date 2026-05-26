# Design: Box Database Migration

**Spec:** 005-database_migration
**Created:** 2026-05-26
**Status:** Draft — awaiting review
**Requirements:** [requirements.md](./requirements.md)

## Open items — resolved before design

The requirements doc flagged four open items. Discovery for this design resolves them:

| # | Question | Resolution |
|---|----------|------------|
| 1 | Confirm filenames for the three new pages | **`BoxProvisioning.md`**, **`BoxProvisioningConfiguration.md`**, **`BoxProvisioningUpgrade.md`** — matches the requirements' tentative names and the existing `BrighterOutboxSupport.md` / `BrighterInboxSupport.md` PascalCase convention. |
| 2 | Do Spanner Outbox/Inbox docs exist in `contents/`? | **No.** `ls contents/ \| grep -i span` returns nothing. No per-backend Spanner pages exist today, so the per-backend update list stays at 8 pages (MSSQL/MySQL/Postgres/Sqlite × Outbox/Inbox). Spanner is covered only on the concept page (support matrix row marked "fresh-install-only; no Docs page yet"). |
| 3 | Should the MSSQL all-or-nothing caveat appear on the conceptual page, not just the P1 upgrade page? | **Yes — once, as a row in the per-backend differences table on `BoxProvisioning.md`.** The full discussion stays on `BoxProvisioningUpgrade.md`. This keeps the concept page complete (a reader who skips the upgrade page still sees the warning) without duplicating the discussion. |
| 4 | SUMMARY.md section name | **"Database Provisioning"** — confirmed. Sits between "Outbox and Inbox" and "Health Checks and Observability". |

## Documentation structure

### File tree (new + changed)

```
Docs/
├── SUMMARY.md                                    ← update: add new section
└── contents/
    ├── BoxProvisioning.md                        ← NEW (P0, conceptual)
    ├── BoxProvisioningConfiguration.md           ← NEW (P0, how-to)
    ├── BoxProvisioningUpgrade.md                 ← NEW (P1, how-to)
    │
    ├── BrighterOutboxSupport.md                  ← update: signpost paragraph
    ├── BrighterInboxSupport.md                   ← update: signpost paragraph
    │
    ├── MSSQLOutbox.md                            ← update: Option A/B framing
    ├── MySQLOutbox.md                            ← update: Option A/B framing
    ├── PostgresOutbox.md                         ← update: Option A/B framing
    ├── SqliteOutbox.md                           ← update: Option A/B framing
    │
    ├── MSSQLInbox.md                             ← update: add provisioning section
    ├── MySQLInbox.md                             ← update: add provisioning section
    ├── PostgresInbox.md                          ← update: add provisioning section (V1-only)
    ├── SqliteInbox.md                            ← update: add provisioning section
    │
    └── Glossary.md                               ← update: 5 new terms
```

Files explicitly NOT touched (out of scope):
- `DapperOutbox.md`, `EFCoreOutbox.md` — about transaction integration, not table provisioning.
- `DynamoOutbox.md`, `DynamoInbox.md`, `MongoDBOutbox.md`, `MongoDBInbox.md` — NoSQL backends are out of scope per the requirements.

### Reading order for a Brighter user

This is the **suggested left-nav order** within the new section, plus the suggested cross-link entry points from the existing docs:

1. **Entry from existing docs**:
   - A reader on `BrighterOutboxSupport.md` (or `BrighterInboxSupport.md`) sees a one-paragraph signpost: "Brighter can also create and migrate the table for you — see [Database Provisioning](/contents/BoxProvisioning.md)."
   - A reader on `MSSQLOutbox.md` (or any per-backend page) sees the **Option A** callout at the top of the configuration section: "Brighter ships a library that creates and migrates this table for you. See [Database Provisioning](/contents/BoxProvisioning.md). The rest of this page describes Option B — managing the DDL yourself."

2. **Inside the new section** (matches SUMMARY.md order):
   - `BoxProvisioning.md` — what it is, why it exists, the three paths, per-backend matrix.
   - `BoxProvisioningConfiguration.md` — how to register it, with code.
   - `BoxProvisioningUpgrade.md` — what happens against a pre-existing table.

3. **Out (Further Reading from each new page)**:
   - Back to the relevant per-backend Outbox/Inbox page (for table-shape details and Option B).
   - Out to the ADRs (`Brighter/docs/adr/0053-box-database-migration.md`, `0057-box-schema-versioning-and-migrations.md`) for design rationale.
   - Out to the implementor guides (`Brighter/docs/guides/box-provisioning-{adding-columns,new-backend}.md`) **as a one-line pointer for Brighter contributors only** — never duplicate their content.

## SUMMARY.md changes

### Before (lines 87–93)

```markdown
 * [Dynamo Inbox](/contents/DynamoInbox.md)
 * [MongoDb Inbox](/contents/MongoDbInbox.md)

## Health Checks and Observability

 * [Logging](/contents/Logging.md)
```

### After

```markdown
 * [Dynamo Inbox](/contents/DynamoInbox.md)
 * [MongoDb Inbox](/contents/MongoDbInbox.md)

## Database Provisioning

 * [Box Provisioning](/contents/BoxProvisioning.md)
 * [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md)
 * [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md)

## Health Checks and Observability

 * [Logging](/contents/Logging.md)
```

Indentation matches the surrounding section style (single leading space + `*`).

---

## New file 1 — `contents/BoxProvisioning.md`

**Type:** Conceptual
**Priority:** P0
**Target length:** ~220–260 lines
**Purpose (one sentence):** Explain what BoxProvisioning is, why it exists, the three runner paths, the `__BrighterMigrationHistory` table, the per-backend support matrix, and the safety guarantees — without showing call-site code (that's the next page).

### Section outline

```
# Box Provisioning

(intro — 2–3 sentences)

## When to use Box Provisioning

  (Option A vs Option B — fit, not preference)

## How it works

### The three paths

  - Fresh install
  - Bootstrap
  - Normal migration
  (one diagram or numbered list, no code)

### The migration history table

  - Name, location, PK shape
  - What operators will see
  - Cross-backend naming note (Spanner uses `BrighterMigrationHistory`, no underscores)

## Concurrency and multi-instance startup

  - Per-backend advisory lock (generic explanation)
  - Why startup is safe under Kubernetes / horizontal scaling
  - Readiness-probe / `initialDelaySeconds` operator hint

## Per-backend support

  (matrix: backend × outbox version range × inbox version range × notes)

## Per-backend differences to be aware of

  (table summarising the operator-visible asymmetries from ADR 0057)
  - MSSQL all-or-nothing multi-version upgrade
  - PostgreSQL inbox is V1-only
  - Spanner is fresh-install-only
  - MySQL minimum version (8.0)

## What Box Provisioning does NOT do

  - Application/domain tables
  - Queue/transport tables (MsSqlQueueBuilder etc.)
  - Cross-payload-mode migrations (binary↔text↔JSON)
  - Pre-2015 schemas

## Source-breaking changes on upgrade

  - `IAmARelationalDatabaseConfiguration.SchemaName` (V10 baseline)
  - `IAmABoxMigration` member additions (external implementors only)

## Further Reading

  - Configuring Box Provisioning
  - Upgrading Existing Deployments
  - The per-backend Outbox/Inbox pages
  - ADRs 0053, 0057
  - Implementor guides (one-line pointer)
```

### Headings (full list, exact)

- `# Box Provisioning` (H1)
- `## When to use Box Provisioning` (H2)
- `## How it works` (H2)
- `### The three paths` (H3)
- `### The migration history table` (H3)
- `## Concurrency and multi-instance startup` (H2)
- `## Per-backend support` (H2)
- `## Per-backend differences to be aware of` (H2)
- `## What Box Provisioning does NOT do` (H2)
- `## Source-breaking changes on upgrade` (H2)
- `## Further Reading` (H2)

### Code examples needed

| # | Purpose | Source | Shape |
|---|---------|--------|-------|
| 1 | Show the schema of `__BrighterMigrationHistory` | ADR 0053 §5 (MSSQL variant) | Abbreviated `CREATE TABLE` (MSSQL flavour) — about 10 lines, illustrates the PK shape. Note that other backends use the same logical shape with backend-native types. |

No call-site (`UseBoxProvisioning`) code on this page — that lives on the configuration page.

### Tables to include

**Per-backend support matrix** (anchors §"Per-backend support"):

| Backend | NuGet package | Outbox versions | Inbox versions | Advisory-lock primitive |
|---------|---------------|-----------------|----------------|-------------------------|
| MSSQL | `Paramore.Brighter.BoxProvisioning.MsSql` | V1..V7 | V1..V2 | `sp_getapplock` |
| PostgreSQL | `Paramore.Brighter.BoxProvisioning.PostgreSql` | V1..V7 | V1 only | `pg_try_advisory_lock` |
| MySQL | `Paramore.Brighter.BoxProvisioning.MySql` | V1..V7 | V1..V2 | `GET_LOCK` |
| SQLite | `Paramore.Brighter.BoxProvisioning.Sqlite` | V1..V7 | V1..V2 | `BEGIN IMMEDIATE` |
| Spanner | `Paramore.Brighter.BoxProvisioning.Spanner` | fresh-install-only | fresh-install-only | n/a (DDL serialised by Spanner) |

**Per-backend differences** (anchors §"Per-backend differences to be aware of"):

| Backend | Asymmetry | What it means for operators |
|---------|-----------|------------------------------|
| MSSQL | All-or-nothing multi-version upgrade | A mid-chain failure rolls back *all* migrations in that run. See [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#mssql-multi-version-upgrades). |
| PostgreSQL | Inbox is V1-only | The Postgres inbox shipped with its final column set in 2021; no V2 exists. The chain is intentionally shorter. |
| MySQL | Minimum 8.0 | Earlier MySQL versions are not supported. |
| SQLite | File-level locking only | Long migration chains block readers. Acceptable for the dev/test use case SQLite targets. |
| Spanner | Fresh-install-only (degenerate runner) | The runner can create the table but cannot evolve an existing table. Track-record: no known production deployments. |

### Cross-links (inbound and outbound)

Outbound (this page → others):
- Every row of the per-backend support matrix links to its per-backend Outbox AND Inbox page.
- "What it does NOT do" links to `OutboxPattern.md` for the application-DB-migration boundary.
- Further Reading: `BoxProvisioningConfiguration.md`, `BoxProvisioningUpgrade.md`, all 8 per-backend pages, `BrighterOutboxSupport.md`, `BrighterInboxSupport.md`, `Glossary.md`, two ADRs.

Inbound (required by requirements §3 bidirectional rule):
- `BrighterOutboxSupport.md` and `BrighterInboxSupport.md` link here.
- All 8 per-backend pages link here (from their Option A callout).

### Glossary terms first-introduced here

Mark with `[term](/contents/Glossary.md#term-anchor)` on first use:
- BoxProvisioning, Migration Chain, Migration History Table, Bootstrap Path, Advisory Lock.

### Source-material map (writer references — not duplicated in the page)

- ADR 0053 §1, §2 (architecture, hosted service, fail-fast behaviour).
- ADR 0053 §5 (advisory-lock primitives table, MSSQL all-or-nothing).
- ADR 0053 §10 (source-breaking change to `IAmARelationalDatabaseConfiguration.SchemaName`).
- ADR 0057 §1 (per-backend version chains — outbox V1..V7, inbox divergence).
- ADR 0057 §2 (discriminator gate — relevant for the troubleshooting hooks but discussed in the upgrade page).
- ADR 0057 §3 (three-path runner).
- ADR 0057 §4 (`IAmABoxMigration` interface change — source-breaking).
- ADR 0057 §6 (Spanner degenerate runner).
- ADR 0057 "Negative consequences" (source-breaking on `IAmABoxMigration`).

---

## New file 2 — `contents/BoxProvisioningConfiguration.md`

**Type:** How-to
**Priority:** P0
**Target length:** ~280–340 lines (code-heavy)
**Purpose (one sentence):** Walk the reader from `Install-Package` to a working `UseBoxProvisioning(...)` call, covering both registration shapes (explicit configuration and `connectionName`/Aspire), the lock timeout knob, and the ordering at startup.

### Section outline

```
# Configuring Box Provisioning

(intro — points back to BoxProvisioning.md for the "what")

## Prerequisites

  - You're already using AddBrighter
  - You have a relational backend supported by Box Provisioning
  - Database user has CREATE TABLE / ALTER TABLE permission for the target schema

## NuGet packages

  - Paramore.Brighter.BoxProvisioning (core)
  - Plus one of the per-backend packages
  (PowerShell Install-Package snippets for each)

## The call-site shape

### Outbox only

  (full Add{Backend}Outbox example with explicit IAmARelationalDatabaseConfiguration)

### Outbox and Inbox together

  (composed Add{Backend}Outbox + Add{Backend}Inbox example, showing startup ordering)

## Resolving connection strings at runtime (.NET Aspire and IConfiguration)

  (connectionName overload example)
  (one paragraph explaining why this exists — Aspire populates IConfiguration after DI is built)

## Tuning the migration lock timeout

  (MigrationLockTimeout = TimeSpan.FromSeconds(30) default)
  (per-backend unit conversion table from ADR 0053 §5)
  (Kubernetes readiness-probe alignment hint)

## Startup ordering

  (Outbox before Inbox; explained — outbox is the critical path)
  (What happens if a provisioner throws: ConfigurationException; host fails to start)

## Per-backend notes

  ### MSSQL
  ### PostgreSQL
  ### MySQL
  ### SQLite
  ### Spanner   (degenerate runner — point at concept page)

## Common pitfalls

  - Forgetting the per-backend package
  - Putting connectionName overload when not on Aspire (works fine, but no DI benefit)
  - Setting MigrationLockTimeout *after* calling AddXxxOutbox (silently ignored)

## Further Reading

  - Box Provisioning (conceptual)
  - Upgrading Existing Deployments
  - Per-backend Outbox/Inbox pages
  - Sample: WebAPI_Dapper/GreetingsWeb/Startup.cs
```

### Headings (full list, exact)

- `# Configuring Box Provisioning`
- `## Prerequisites`
- `## NuGet packages`
- `## The call-site shape`
- `### Outbox only`
- `### Outbox and Inbox together`
- `## Resolving connection strings at runtime (.NET Aspire and IConfiguration)`
- `## Tuning the migration lock timeout`
- `## Startup ordering`
- `## Per-backend notes`
- `### MSSQL`
- `### PostgreSQL`
- `### MySQL`
- `### SQLite`
- `### Spanner`
- `## Common pitfalls`
- `## Further Reading`

### Code examples needed

| # | Purpose | Source | Shape |
|---|---------|--------|-------|
| 1 | NuGet install (PowerShell) | written from package list | `Install-Package` for core + each backend, in a single block |
| 2 | Outbox-only registration with explicit config (MSSQL) | ADR 0053 §3 (`AddMsSqlOutbox`) | ~15 lines: `var dbConfig = new RelationalDatabaseConfiguration(...);` then `services.AddBrighter()...UseBoxProvisioning(opts => opts.AddMsSqlOutbox(dbConfig))` |
| 3 | Outbox + Inbox together (MSSQL) | derived from ADR 0053 §3 | ~20 lines: same shape, both `Add` calls inside the same `UseBoxProvisioning` |
| 4 | `connectionName` overload (MSSQL, Aspire path) | ADR 0053 §3 second overload | ~12 lines: `opts.AddMsSqlOutbox("BrighterDb", outboxTableName: "Outbox")` |
| 5 | Migration lock timeout — recommended (parameter) | ADR 0053 §3 | one-liner: `UseBoxProvisioning(opts => {...}, migrationLockTimeout: TimeSpan.FromMinutes(2))` |
| 6 | Migration lock timeout — ordering-sensitive (property) | ADR 0053 §3 (BoxProvisioningOptions.MigrationLockTimeout doc) | ~6 lines: set `opts.MigrationLockTimeout = ...` *before* any `Add` call — with comment warning that setting it after is silently ignored |
| 7 | Per-backend snippet (one per backend) | ADR 0053 §3 (extended to each backend) | ~6 lines each: just the `opts.Add{Backend}Outbox(config)` line, anchored to a per-backend NuGet identifier |

Sample anchor: `Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs:116` (the `UseBoxProvisioning(options => { BoxProvisioningFactory.AddOutbox(options, rdbms, outboxConfiguration); })` call) — link in Further Reading, **do not paste the factory's code**. The configuration page shows the direct shape (`opts.AddMsSqlOutbox(config)`) which is what the factory dispatches to internally; the factory itself exists because the sample supports multiple backends at runtime, which is a sample concern, not a user concern.

### Tables to include

**NuGet package list** (anchors §"NuGet packages"):

| Backend | Package |
|---------|---------|
| (core, always required) | `Paramore.Brighter.BoxProvisioning` |
| MSSQL | `Paramore.Brighter.BoxProvisioning.MsSql` |
| PostgreSQL | `Paramore.Brighter.BoxProvisioning.PostgreSql` |
| MySQL | `Paramore.Brighter.BoxProvisioning.MySql` |
| SQLite | `Paramore.Brighter.BoxProvisioning.Sqlite` |
| Spanner | `Paramore.Brighter.BoxProvisioning.Spanner` |

**Lock-timeout unit conversion** (anchors §"Tuning the migration lock timeout"; from ADR 0053 §5):

| Backend | Underlying lock primitive | Unit the backend expects | Brighter's conversion |
|---------|---------------------------|--------------------------|------------------------|
| MSSQL | `sp_getapplock` | milliseconds (int) | `(int)timeout.TotalMilliseconds` |
| PostgreSQL | `pg_try_advisory_lock` retry loop | milliseconds (int) — total retry budget | `(int)timeout.TotalMilliseconds` |
| MySQL | `GET_LOCK` | whole seconds (int) | `(int)timeout.TotalSeconds` (sub-second values floored at 1 — per ADR 0057 §5b "Item R") |
| SQLite | file-level lock | n/a | implicit serialisation |
| Spanner | n/a | n/a | DDL serialised by Spanner service |

### Cross-links

Outbound: `BoxProvisioning.md` (the what), `BoxProvisioningUpgrade.md` (what happens on first start), 8 per-backend pages, sample link.

Inbound: from `BoxProvisioning.md` and from each per-backend page's Option A callout.

### Glossary terms

Use the already-defined links from the Glossary updates — first occurrences are on `BoxProvisioning.md`, so this page uses plain references.

### Source-material map

- ADR 0053 §3 — registration extension and both overload shapes.
- ADR 0053 §5 — `MigrationLockTimeout` and per-backend unit conversions.
- ADR 0053 §8 — Aspire `connectionName` rationale.
- ADR 0053 §9 — package layout.
- ADR 0053 "Consequences → Negative → Startup blocking" — readiness-probe note.
- Sample `Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs:116` for the canonical call-site (link, do not duplicate).

---

## New file 3 — `contents/BoxProvisioningUpgrade.md`

**Type:** How-to
**Priority:** P1
**Target length:** ~240–300 lines
**Purpose (one sentence):** Tell a user with a pre-existing Brighter deployment what to expect when they first start their app under BoxProvisioning, what gets logged, the documented edge cases, and how to troubleshoot the most likely errors.

### Section outline

```
# Upgrading Existing Deployments

(intro — who this page is for: existing Brighter deployments adopting BoxProvisioning)

## What happens on first start

  (the bootstrap path explained in operator terms — not implementor terms)
  - The provisioner finds the table
  - It detects no migration history for this table
  - It introspects columns to figure out which Brighter version originally shipped the table
  - It writes a synthetic history row at that version
  - It runs any subsequent migrations in order
  - Future starts use the normal path

## What gets logged

  (Information-level messages an operator should see)
  - "Provisioning Outbox..." / "Provisioning Inbox..."
  - "Provisioned {BoxType} successfully"
  - For Postgres: "Waiting for migration lock on {tableName}..."  (retry diagnostic)
  - Error-level: "Failed to provision {BoxType}. The application cannot start..."

## What to verify after upgrade

  - SELECT * FROM __BrighterMigrationHistory (operator can see what was applied)
  - Compare MAX(MigrationVersion) against the support matrix
  - No new columns should be NULL for old rows — that's expected and safe (additive migrations only)

## Documented edge cases

### Pre-#3042 message-ID column type (UNIQUEIDENTIFIER vs NVARCHAR)
### Payload mode mismatch (binary vs text vs JSON)
### Pre-V1 (pre-2015) schemas
### Spanner

## MSSQL multi-version upgrades

  (the all-or-nothing semantics — what an operator sees)
  - A mid-chain failure rolls back EVERY migration in that run
  - History table will not contain partial progress
  - Other backends commit per migration
  - Why this matters: when upgrading across multiple Brighter releases at once

## Troubleshooting

### "Table {name} exists but is not a Brighter outbox/inbox"
### "Table {name} appears to be a Brighter outbox/inbox but does not match any known schema version"
### TimeoutException waiting for the advisory lock
### ConfigurationException from payload-mode mismatch

## Rolling back

  (short — migrations are forward-only by design; rollback is "redeploy the previous Brighter version
   and accept the extra columns sit NULL")

## Further Reading

  - Box Provisioning
  - Configuring Box Provisioning
  - ADR 0057 (versioning model)
  - Per-backend Outbox/Inbox pages
```

### Headings (full list, exact)

- `# Upgrading Existing Deployments`
- `## What happens on first start`
- `## What gets logged`
- `## What to verify after upgrade`
- `## Documented edge cases`
- `### Pre-#3042 message-ID column type (UNIQUEIDENTIFIER vs NVARCHAR)`
- `### Payload mode mismatch (binary vs text vs JSON)`
- `### Pre-V1 (pre-2015) schemas`
- `### Spanner`
- `## MSSQL multi-version upgrades`
- `## Troubleshooting`
- `### "Table {name} exists but is not a Brighter outbox/inbox"`
- `### "Table {name} appears to be a Brighter outbox/inbox but does not match any known schema version"`
- `### TimeoutException waiting for the advisory lock`
- `### ConfigurationException from payload-mode mismatch`
- `## Rolling back`
- `## Further Reading`

### Code examples needed

| # | Purpose | Source | Shape |
|---|---------|--------|-------|
| 1 | Operator query to inspect migration history | derived from ADR 0053 §5 schema | `SELECT * FROM __BrighterMigrationHistory WHERE BoxTableName = 'Outbox' ORDER BY MigrationVersion;` (one line, plus expected-output block of 3–5 rows) |
| 2 | Sample log output on bootstrap path | derived from ADR 0053 §2 (hosted-service logging) | Plain-text log block — illustrates the Information-level lines the operator should see |
| 3 | The error message + remediation for "not a Brighter box" | ADR 0057 §2 (error-message wording verbatim) | Plain-text exception block + 2-sentence remediation |
| 4 | The error message + remediation for "unknown schema version" | ADR 0057 §2 | Plain-text exception block + 2-sentence remediation |
| 5 | The error message + remediation for advisory-lock timeout | ADR 0053 §5 / ADR 0057 §5a | Plain-text exception block + remediation: tune `MigrationLockTimeout`, check for stuck processes |

No C# in this page beyond the configuration cross-references — this is an operator-facing page.

### Tables to include

**Edge-case summary** (anchors §"Documented edge cases"):

| Edge case | Behaviour | What the operator should do |
|-----------|-----------|------------------------------|
| Pre-#3042 outbox table with `UNIQUEIDENTIFIER` MessageId | Detected as V4-equivalent. No migration applied. Brighter accepts both GUID and string IDs. | Nothing — Brighter handles both. Only worth knowing if application code stores non-GUID IDs (will throw on INSERT against the old column type). |
| Payload mode mismatch | Fail-fast at startup: `ConfigurationException`. Cannot migrate between modes. | Drop and recreate the table with the new payload mode, OR keep the existing mode. |
| Pre-V1 (pre-2015) schemas | Not detected by the migration chain. | Manual `ALTER TABLE` to add missing columns, OR drop and recreate. |
| Spanner with an existing table | Degenerate runner: writes a synthetic V_latest history row without migrating. | Acceptable when the existing table matches V_latest (no known legacy Spanner installations). |

### Cross-links

Outbound: `BoxProvisioning.md`, `BoxProvisioningConfiguration.md`, every per-backend Outbox/Inbox page (for table shapes at V_latest), ADR 0057, `Glossary.md`.

Inbound: `BoxProvisioning.md` per-backend differences table links here for the MSSQL all-or-nothing detail.

### Source-material map

- ADR 0053 §2 — hosted-service log lines and `ConfigurationException` wrapping.
- ADR 0053 §5 (MSSQL all-or-nothing) and "Consequences → Negative" entry.
- ADR 0053 §6 (binary/text payload-mode validation, fail-fast).
- ADR 0057 §1 + spec 0027 A-1 (pre-#3042 MessageId/CorrelationId type change).
- ADR 0057 §2 (discriminator gate error messages — quote verbatim).
- ADR 0057 §3 (three-path runner — operator-facing description).
- ADR 0057 §5a (mid-chain failure recovery invariant).
- ADR 0057 §6 (Spanner degenerate fresh-only).

---

## Updated file template — per-backend Outbox pages (×4)

**Files:** `MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md`, `SqliteOutbox.md`
**Action:** Add one new top-of-page section ("Provisioning the Outbox Table") and soften one paragraph mid-page. Preserve the existing DDL/configuration sections — these are Option B.

### Insertion 1 — new section, placed immediately after the H1 intro and before `## NuGet Packages`

```markdown
## Provisioning the Outbox Table

You have two equally valid options for creating and maintaining the Outbox table:

**Option A — Let Brighter provision and migrate it for you (recommended for greenfield apps).**

Brighter ships a library that creates the table on first start and evolves its schema across Brighter releases automatically. See [Database Provisioning](/contents/BoxProvisioning.md) for the overview and [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md) for the call-site shape.

**Option B — Manage the DDL yourself (recommended where you have schema-change governance).**

Use `{Backend}OutboxBuilder.GetDDL()` to obtain the same DDL Brighter ships, then drive it through your own change-management tooling — FluentMigrator, Flyway, Liquibase, an enterprise change-window pipeline, or hand-rolled scripts. The rest of this page describes this option.

Neither option is deprecated. Choose based on fit: small teams and greenfield apps benefit from startup-time provisioning; teams with DBA approval workflows or change windows often prefer to drive the same DDL through their own tooling.
```

`{Backend}` substituted per page: `MsSql`, `MySql`, `PostgreSql`, `Sqlite`.

### Insertion 2 — soften the "you are responsible" note

**MSSQLOutbox.md line 23 (and the equivalent in each other backend page):**

Before:
```
**Note:** You are responsible for creating and maintaining this table. This includes tasks such as adding indexes to optimize query performance and managing schema migrations when updating to new versions of Brighter that may require additional columns.
```

After:
```
**Note:** When you choose Option B, you are responsible for creating the table and applying schema changes when upgrading to new versions of Brighter. Option A handles both for you — see [Database Provisioning](/contents/BoxProvisioning.md). Either way, application-level concerns like additional indexes for query performance remain your responsibility.
```

### Cross-link checks

After updating each Outbox page, verify:
- Top-of-page Option A callout links to `BoxProvisioning.md` and `BoxProvisioningConfiguration.md`.
- Mid-page softened note links to `BoxProvisioning.md`.
- No "deprecated" or "legacy" language anywhere on these pages.

### Per-page exceptions

- **PostgresOutbox.md**: same template. The V1-only inbox quirk is on the *inbox* page, not the outbox page (Postgres outbox is V1..V7 like the others).
- **SqliteOutbox.md**: add a one-line note at the end of the Option A paragraph: "SQLite serialises migrations via file-level locking — long upgrade chains briefly block readers."

---

## Updated file template — per-backend Inbox pages (×4)

**Files:** `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`
**Action:** Add one new H2 section ("Provisioning the Inbox Table") at the bottom of the page (these pages are ~32 lines and don't currently mention DDL provisioning at all).

### Insertion — append a new section

```markdown
## Provisioning the Inbox Table

You have two equally valid options for creating and maintaining the Inbox table:

**Option A — Let Brighter provision and migrate it for you.**

Brighter ships a library that creates the Inbox table on first start and evolves its schema across Brighter releases. See [Database Provisioning](/contents/BoxProvisioning.md) and [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md).

**Option B — Manage the DDL yourself.**

Use `{Backend}InboxBuilder.GetDDL()` to obtain the DDL Brighter ships and apply it via your own tooling (FluentMigrator, Flyway, Liquibase, or hand-rolled scripts).

Choose based on fit; neither option is deprecated.
```

`{Backend}` substituted per page: `MsSql`, `MySql`, `PostgreSql`, `Sqlite`.

### Per-page exception — `PostgresInbox.md`

Append one additional sentence to the Option A paragraph:

> The PostgreSQL Inbox is at schema version 1 — the table shipped with its final column set, so there are no inbox migrations for this backend to apply.

This is the only place in the user docs where the V1-only quirk needs to be explicit on a per-backend page; the support matrix on `BoxProvisioning.md` covers it for everyone else.

---

## Updated file — `contents/BrighterOutboxSupport.md`

**Action:** Add a one-paragraph signpost as a new H2 section, placed immediately before the existing `### Outbox Builder` H3 (line ~196).

Reasoning: the `### Outbox Builder` section already describes manual DDL ("you can use this as part of your application start up..."); a signpost just before it gives the alternative without disrupting the manual workflow.

### Insertion

Between the existing `### Outbox Configuration` (line 192) and `### Outbox Builder` (line 196):

```markdown
## Provisioning the Outbox Table

If your Outbox runs on a relational database (MSSQL, PostgreSQL, MySQL, SQLite, or Spanner), Brighter can create and migrate the table for you at application startup — see [Database Provisioning](/contents/BoxProvisioning.md). The "Outbox Builder" section below describes the alternative: managing the DDL yourself.
```

The existing `### Outbox Builder` section becomes the documented Option B and remains unchanged.

---

## Updated file — `contents/BrighterInboxSupport.md`

**Action:** Add the same shape of signpost as `BrighterOutboxSupport.md`. Placement: as a new H2 near the end of the page, before any "Further Reading" or sample section if one exists.

### Insertion

```markdown
## Provisioning the Inbox Table

If your Inbox runs on a relational database (MSSQL, PostgreSQL, MySQL, SQLite, or Spanner), Brighter can create and migrate the table for you at application startup — see [Database Provisioning](/contents/BoxProvisioning.md). The per-backend Inbox pages document the alternative: managing the DDL yourself.
```

(The writer should confirm exact placement when implementing — somewhere after the conceptual content and before the per-backend page links.)

---

## Updated file — `contents/Glossary.md`

**Action:** Add a new H2 section "Database Provisioning" with five term entries. Placement: after `## Patterns` and before `## Messaging` (so it sits near Outbox/Inbox/Sweeper definitions which are also under Patterns).

### Insertion — new H2 section

```markdown
## Database Provisioning

### BoxProvisioning

The Brighter library family that creates and migrates relational Outbox and Inbox tables at application startup. Shipped as `Paramore.Brighter.BoxProvisioning` (core) plus one per backend (`*.MsSql`, `*.PostgreSql`, `*.MySql`, `*.Sqlite`, `*.Spanner`). Registered via `services.AddBrighter().UseBoxProvisioning(...)`.

See: [Box Provisioning](/contents/BoxProvisioning.md), [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md)

### Migration Chain

The ordered list of `BoxMigration` records (V1..V_latest) that, when applied in sequence, evolve a Brighter Outbox or Inbox table from its earliest shipped shape to the current schema. The chain is per-backend and per-box-type. Outbox: V1..V7 on all four relational backends. Inbox: V1..V2 on MSSQL/MySQL/SQLite; V1 only on PostgreSQL.

See: [Box Provisioning](/contents/BoxProvisioning.md#per-backend-support)

### Migration History Table

The table BoxProvisioning creates and maintains to track which migrations have been applied. Named `__BrighterMigrationHistory` on every backend except Spanner (where it is `BrighterMigrationHistory` — no leading underscores, per Spanner naming rules). Primary key `(SchemaName, BoxTableName, MigrationVersion)`. Visible to operators and DBAs.

See: [Box Provisioning](/contents/BoxProvisioning.md#the-migration-history-table)

### Bootstrap Path

One of BoxProvisioning's three runner paths. Triggered when the Outbox or Inbox table exists but the migration history table has no rows for it — typically a pre-existing deployment adopting BoxProvisioning for the first time. The runner introspects columns to detect which schema version the table is at, writes a synthetic history row, then applies any subsequent migrations. Contrast with **fresh install** (no table) and **normal migration** (table + history).

See: [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md#what-happens-on-first-start)

### Advisory Lock

A database-native lock primitive that BoxProvisioning uses to serialise migration runs across multiple instances of the same application starting simultaneously. Per-backend: `sp_getapplock` (MSSQL), `pg_try_advisory_lock` (PostgreSQL), `GET_LOCK` (MySQL), `BEGIN IMMEDIATE` (SQLite — file-level lock, not strictly an advisory lock). Held for the duration of one migration run; released on commit or rollback. Configurable via `MigrationLockTimeout` (default: 30 seconds).

See: [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md#tuning-the-migration-lock-timeout)
```

### Existing entries to leave alone

The existing `### Outbox` and `### Inbox` entries under `## Patterns` already point at `BrighterOutboxSupport.md` / `BrighterInboxSupport.md` — those pages will be updated to signpost BoxProvisioning, so the Glossary entries themselves don't need to change.

---

## Code examples plan — summary

Cross-cuts the per-file outlines above so the writer can prepare them once.

| Example | Lives on | Source | Status |
|---------|----------|--------|--------|
| `__BrighterMigrationHistory` `CREATE TABLE` (MSSQL flavour, abbreviated) | `BoxProvisioning.md` | ADR 0053 §5 | Written from ADR — verify against actual `MsSqlBoxMigrationRunner` source before publish |
| NuGet `Install-Package` block | `BoxProvisioningConfiguration.md` | ADR 0053 §9 package layout | Written from scratch |
| Outbox-only registration (MSSQL, explicit config) | `BoxProvisioningConfiguration.md` | ADR 0053 §3 + sample line 116 | Written from ADR; pattern matches sample |
| Outbox + Inbox together (MSSQL) | `BoxProvisioningConfiguration.md` | Derived from above | Written from scratch |
| `connectionName` overload (MSSQL, Aspire path) | `BoxProvisioningConfiguration.md` | ADR 0053 §3 second overload | Written from ADR |
| Migration lock timeout — parameter form (recommended) | `BoxProvisioningConfiguration.md` | ADR 0053 §3 | One-liner |
| Migration lock timeout — property form (ordering-sensitive) | `BoxProvisioningConfiguration.md` | ADR 0053 §3 BoxProvisioningOptions doc | Short snippet with warning comment |
| Per-backend `opts.Add{Backend}Outbox(config)` snippets (×5) | `BoxProvisioningConfiguration.md` | ADR 0053 §3 extended | Written per backend |
| Operator query — inspect history | `BoxProvisioningUpgrade.md` | Schema from ADR 0053 §5 | One-line `SELECT` + expected output |
| Log output on bootstrap | `BoxProvisioningUpgrade.md` | ADR 0053 §2 logging | Plain text |
| Error message — "not a Brighter box" | `BoxProvisioningUpgrade.md` | ADR 0057 §2 (quote verbatim) | Plain text + remediation |
| Error message — "unknown schema version" | `BoxProvisioningUpgrade.md` | ADR 0057 §2 (quote verbatim) | Plain text + remediation |
| Error message — advisory-lock timeout | `BoxProvisioningUpgrade.md` | ADR 0057 §5a | Plain text + remediation |

**Verification**: every C# example must compile against V10 (per CLAUDE.md). Before publication, the writer should cross-check by reading the actual extension-method source in `../Brighter/src/Paramore.Brighter.BoxProvisioning.MsSql/` and confirming method names + parameter shapes match the ADR. ADRs sometimes ship with names that drift slightly during implementation — verify before writing.

---

## Style and terminology notes

Per requirements §Constraints — reproduced here for the writer's convenience:

- **Voice**: second person, present tense, active voice.
- **Capitalisation**: `BoxProvisioning` (one word, capital B and P); `Outbox` / `Inbox` capitalised when referring to Brighter concepts.
- **Three runner paths**: always **fresh install**, **bootstrap**, **normal migration** (lower-case, no hyphens unless mid-sentence as compound adjectives — "the fresh-install path" but "during a fresh install").
- **Migration history table**: long form `__BrighterMigrationHistory` (or `BrighterMigrationHistory` for Spanner) on first use per page; afterwards "the migration history table" is fine.
- **Advisory lock**: generic term. Use the per-backend primitive name (`sp_getapplock`, `pg_try_advisory_lock`, `GET_LOCK`, `BEGIN IMMEDIATE`) only inside per-backend sections.
- **Spanner**: "fresh-install-only" or "degenerate runner". Never "broken" or "unsupported."
- **Option B framing**: "manage the DDL yourself" — never "deprecated", "legacy", or "old way". Frame as a continuing first-class choice for governed-change-window environments.

### Deviations from standard patterns — none

This work fits the existing documentation patterns (conceptual + how-to + reference, with per-backend reference pages and a glossary). No deviations needed.

---

## Acceptance criteria for this design

The design is ready to advance to tasks (`/spec:tasks`) when:

- **AC-D1**: Every requirement deliverable in `requirements.md` §"Documentation Deliverables" has a corresponding section in this design (3 new files outlined in full; 12 updated files with insertion templates).
- **AC-D2**: All four open items from the requirements are resolved (see top of this document).
- **AC-D3**: Cross-linking is bidirectional and explicit — every per-backend page links to the new section, and every new page links to the relevant per-backend pages. The "Cross-links" subsection of each file outline names both directions.
- **AC-D4**: Code examples are inventoried with source references; no example is left to "writer to invent."
- **AC-D5**: SUMMARY.md before/after shows the exact insertion point.
- **AC-D6**: Terminology and style constraints from the requirements are reproduced for the writer.

## Next steps

1. User reviews and approves (or revises) this design.
2. Run `/spec:tasks` to break the work into per-file writing tasks. The natural decomposition:
   - Task: write `BoxProvisioning.md` (P0).
   - Task: write `BoxProvisioningConfiguration.md` (P0).
   - Task: update all 4 per-backend Outbox pages with the Option A/B template (P0, one task — uniform template).
   - Task: update all 4 per-backend Inbox pages (P0, one task — uniform template plus Postgres-V1-only exception).
   - Task: update `BrighterOutboxSupport.md` + `BrighterInboxSupport.md` (P0, one task — signposts).
   - Task: update `Glossary.md` with 5 new terms (P1).
   - Task: write `BoxProvisioningUpgrade.md` (P1).
   - Task: update `SUMMARY.md` (P0, must run after the 3 new files exist).
   - Task: end-to-end cross-link verification (P0, must run last).
