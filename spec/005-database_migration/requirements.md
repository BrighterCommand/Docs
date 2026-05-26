# Requirements: Box Database Migration

**Spec:** 005-database_migration
**Created:** 2026-05-26
**Status:** Draft — awaiting review

## Topic Overview

Brighter has added a new `BoxProvisioning` family of NuGet packages that provision (create) and migrate Outbox and Inbox tables automatically at application startup. The library:

- Creates tables on fresh installs.
- Detects pre-existing tables from earlier Brighter releases and evolves their schema forward without data loss (the **bootstrap** path).
- Tracks applied migrations in a `__BrighterMigrationHistory` table.
- Holds a per-backend advisory lock so multiple host instances can race safely at startup.
- Supports MSSQL, PostgreSQL, MySQL, SQLite via a full migration chain; Spanner via a fresh-install-only degenerate runner.
- Exposes `services.AddBrighter().UseBoxProvisioning(opts => opts.Add{Backend}Outbox(...))` as the call-site API.

This documentation tells **Brighter users** how to adopt the new library to replace the hand-managed-DDL workflow described today in the per-backend Outbox/Inbox docs.

The audience is the Brighter user adopting the new library. **Implementor-facing material** (how to add a column, how to add a new backend, RDD role-interface surface) already exists in the Brighter repo and is out of scope here — see [Out of Scope](#out-of-scope).

## Current State

What exists in `Docs/` today:

- Per-backend Outbox/Inbox docs (`MSSQLOutbox.md`, `PostgresInbox.md`, etc.) describe the **old** hand-rolled DDL workflow: "You are responsible for creating and maintaining this table. This includes ... managing schema migrations when updating to new versions of Brighter."
- `BrighterOutboxSupport.md` / `BrighterInboxSupport.md` cover the Outbox/Inbox pattern at concept level, not provisioning.
- No documentation mentions `BoxProvisioning`, `UseBoxProvisioning`, `IAmABoxProvisioner`, `IAmABoxMigrationRunner`, `__BrighterMigrationHistory`, or the new `Paramore.Brighter.BoxProvisioning.*` packages.

What's missing or incomplete:

- No user-facing entry point that explains the provisioning feature, when to use it, and how to register it.
- The per-backend Outbox/Inbox docs make manual-DDL claims that are now optional (the new library is the recommended path).
- No migration story for users upgrading from earlier Brighter releases with pre-existing tables.
- No reference for the `__BrighterMigrationHistory` table operators will see appear in their database.
- No explanation of the three provisioning paths (fresh / bootstrap / normal) so operators can interpret what they see in logs.

## Target State

A new top-level documentation section ("Database Provisioning") with:

1. A conceptual page explaining the provisioning + migration feature, the three paths, the role of `__BrighterMigrationHistory`, and the per-backend support matrix.
2. A how-to page covering the call-site API: `UseBoxProvisioning`, the `Add{Backend}Outbox` / `Add{Backend}Inbox` overloads, the `connectionName` overload for `IConfiguration`-driven resolution (the .NET Aspire path), the `MigrationLockTimeout`, and the per-backend NuGet package names.
3. A migration / upgrade page for existing deployments — what happens on first start against a pre-existing table, what gets logged, what to verify, and the documented edge cases (pre-#3042 UNIQUEIDENTIFIER message IDs; payload-mode mismatch; Spanner degenerate case).
4. Updates to the existing per-backend Outbox/Inbox docs to (a) point users at the new provisioning library as the recommended option, (b) keep the manual-DDL workflow documented for users who prefer it, and (c) link to the migration history table reference.

When a user opens the docs at GitBook, they should be able to read the conceptual page, follow the how-to to register box provisioning with their backend, and confidently start their app knowing what the library will do to their database.

## Target Audience

- **Primary**: intermediate Brighter users adopting V10's new BoxProvisioning library — they already use an Outbox or Inbox today and want to stop maintaining DDL by hand.
- **Secondary**: operators (DBAs, SREs) who will see `__BrighterMigrationHistory` appear in their databases and need to understand what it is, who manages it, and what failure modes look like.
- **Tertiary**: newcomers evaluating Brighter — they should see "Brighter provisions its own Outbox/Inbox tables" as a feature, with the manual-DDL alternative still documented.

Tone matches the existing Brighter docs (see `BrighterOutboxSupport.md`): direct, second-person, "you can configure" rather than "one might configure."

## Source Material

**Specs (in the Brighter repo)** — capture the user-facing requirements:

- `../Brighter/specs/0023-box_database_migration/` — the original provisioning library spec (NuGet packages, `UseBoxProvisioning`, fresh-install support).
- `../Brighter/specs/0027-box-schema-versioning-and-migrations/` — the V1..V7 outbox / V1..V2 inbox migration chain that fixed the bootstrap path.
- `../Brighter/specs/0028-box-provisioning-rdd-role-interfaces/` — the RDD refactor (role interfaces + abstract bases). Mostly implementor-facing — only the user-visible surface (the call-site API stays the same) is relevant here.

**ADRs** — capture the design and trade-offs:

- `../Brighter/docs/adr/0053-box-database-migration.md` — architecture, NuGet package layout, registration pattern, Aspire integration via `connectionName`, fail-fast behaviour, MSSQL all-or-nothing semantics.
- `../Brighter/docs/adr/0057-box-schema-versioning-and-migrations.md` — versioning model, three-path runner (fresh / bootstrap / normal), per-backend advisory-lock primitives, Spanner degenerate runner, migration history table.
- `../Brighter/docs/adr/0058-box-provisioning-rdd-role-interfaces.md` — RDD role interfaces (implementor-facing — for context only).
- `../Brighter/docs/adr/0059-box-provisioning-abstract-base-naming-symmetry.md` — naming symmetry for the abstract bases (implementor-facing — for context only).

**Implementor guides (in the Brighter repo)** — context and cross-references, not material to copy:

- `../Brighter/docs/guides/box-provisioning-adding-columns.md`
- `../Brighter/docs/guides/box-provisioning-new-backend.md`

**Samples** — anchor the user-facing how-to in working code:

- `../Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs` (line 116 — the `UseBoxProvisioning` call site) and the corresponding `BoxProvisioningFactory` it delegates to.
- Sibling `WebAPI_*` samples for EF Core / raw ADO.NET variants if they also adopt the new API.

**Release notes**: `../Brighter/release_notes.md` should be checked for the user-facing summary of the BoxProvisioning shipment (including the `IAmABoxMigration` and `IAmARelationalDatabaseConfiguration` source-breaking changes noted in ADR 0053 and ADR 0057).

**Existing Brighter Docs files (to update, not to draw from)**:

- `contents/BrighterOutboxSupport.md`, `contents/BrighterInboxSupport.md` — link out to the new section.
- `contents/MSSQLOutbox.md`, `contents/MySQLOutbox.md`, `contents/PostgresOutbox.md`, `contents/SqliteOutbox.md`, `contents/MSSQLInbox.md`, `contents/MySQLInbox.md`, `contents/PostgresInbox.md`, `contents/SqliteInbox.md` — soften the "you are responsible" framing, add a pointer to the new provisioning option.
- Spanner Outbox/Inbox (if present) — call out the degenerate fresh-install-only behaviour explicitly.

## Scope

### P0 (must have)

1. **Conceptual overview page** (`BoxProvisioning.md` — exact filename TBD in design phase). Explains:
   - What problem the feature solves (no more hand-rolled DDL across versions).
   - The `BoxProvisioningHostedService` runs at startup, fail-fast on error, wrapping failures as `ConfigurationException`.
   - The three paths: **fresh install** (no table — runs current builder DDL once), **bootstrap** (table exists, no history — detects schema version and walks forward), **normal** (table + history — apply pending migrations only).
   - The `__BrighterMigrationHistory` table (purpose, location, PK shape `(SchemaName, BoxTableName, MigrationVersion)`, that operators will see it).
   - Per-backend support matrix: MSSQL / PostgreSQL / MySQL / SQLite have full V1..V7 outbox + V1..V2 inbox chains (Postgres inbox V1-only); Spanner is fresh-install-only.
   - Concurrency: each backend acquires an advisory lock — safe for multi-instance startup (e.g. Kubernetes).
   - Source-breaking changes users may encounter on upgrade: `IAmARelationalDatabaseConfiguration.SchemaName` and `IAmABoxMigration` member additions.

2. **How-to / configuration page** (`BoxProvisioningConfiguration.md` — exact filename TBD). Covers:
   - NuGet packages: `Paramore.Brighter.BoxProvisioning` (core) + `Paramore.Brighter.BoxProvisioning.{MsSql,PostgreSql,MySql,Sqlite,Spanner}`.
   - The `services.AddBrighter().UseBoxProvisioning(opts => { opts.Add{Backend}Outbox(...); opts.Add{Backend}Inbox(...); })` call-site shape, drawn from the WebAPI_Dapper sample.
   - The two overload shapes per `Add*`: (a) explicit `IAmARelationalDatabaseConfiguration`, (b) `connectionName` resolved from `IConfiguration` at runtime (the .NET Aspire path — per ADR 0053 §8).
   - `MigrationLockTimeout` (default 30s) — what it controls, per-backend unit conversions (MSSQL ms, MySQL whole seconds, etc. per ADR 0053 §5), how to tune it for Kubernetes readiness probes.
   - Outbox-before-inbox ordering at startup.
   - That `Spanner` registration uses the same shape but the runner is degenerate (no V_k chain).

3. **Update existing per-backend Outbox/Inbox pages** to present **two equally valid options** side by side:
   - **Option A — BoxProvisioning library**: let Brighter create and migrate the tables for you (link to the new pages).
   - **Option B — Manage the DDL yourself**: use `*OutboxBuilder.GetDDL()` / `*InboxBuilder.GetDDL()` with your own tooling (FluentMigrator, Flyway, Liquibase, an enterprise change-management pipeline, etc.). This is the existing workflow and remains fully supported.

   Frame the choice in terms of fit, not preference: small teams and greenfield apps benefit from the library's startup-time provisioning; enterprises with established schema-change governance, change-windows, or DBA approval workflows often prefer to drive the same DDL through their own tooling. Neither option is deprecated.

4. **SUMMARY.md updated** with a new "Database Provisioning" section placed immediately after "Outbox and Inbox" (so users reading about Outbox/Inbox immediately see the provisioning option). **Cross-linking is mandatory in both directions**:
   - Each new "Database Provisioning" page links to the relevant per-backend Outbox/Inbox pages (where the underlying table semantics live).
   - Each per-backend Outbox/Inbox page links to the new "Database Provisioning" pages from its "Option A" callout.

### P1 (should have)

5. **Migration / upgrade page** (`BoxProvisioningUpgrade.md` — exact filename TBD) for users upgrading an existing deployment:
   - What happens on first start against a pre-existing table (bootstrap path: detection by column name, synthetic history row stamped at detected version, then forward migrations).
   - What gets logged at `Information` level so operators can verify migration ran.
   - The documented edge cases:
     - Pre-#3042 outbox tables with `UNIQUEIDENTIFIER` MessageId / CorrelationId (per ADR 0057 §1 "Folded changes" + spec 0027 A-1) — these are version-equivalent at V4+ and require no migration; user code using non-GUID IDs may throw on INSERT.
     - Payload-mode mismatch (binary vs text vs JSON) is fail-fast (`ConfigurationException`) — cannot migrate between modes; user must drop and recreate.
     - Pre-V1 schemas (older than 2015 baseline) are not detected — manual ALTER required.
     - Spanner: degenerate runner; if Spanner adoption arises, a migration chain will be added.
   - MSSQL all-or-nothing semantics on multi-version upgrades (per ADR 0053 §5 + ADR 0057 §5a): a mid-chain failure rolls back the whole run; intermediate versions won't appear in history. PostgreSQL/MySQL/SQLite commit per-migration.

6. **Glossary additions**: `BoxProvisioning`, `Migration Chain`, `Migration History Table`, `Bootstrap Path`, `Advisory Lock`. Should link back into `Glossary.md`.

### P2 (nice to have)

7. **Troubleshooting section** within the upgrade page covering common errors:
   - `ConfigurationException: Table {name} exists but is not a Brighter outbox/inbox (missing discriminator column {column})` — operator pointed at the wrong table name.
   - `ConfigurationException: Table {name} appears to be a Brighter outbox/inbox but does not match any known schema version` — manual inspection required (corrupt / pre-V1).
   - `TimeoutException` waiting for the advisory lock — typically another instance still migrating, or stale lock from a crashed process; tune `MigrationLockTimeout`.

8. **Sample-walkthrough page** linking to and explaining `WebAPI_Dapper/GreetingsWeb/Startup.cs:116` and the `BoxProvisioningFactory` it uses, so users adopting the library have a working reference end-to-end.

## Out of Scope

- **Implementor-facing material** — how to add a new column to the migration chain, how to add a new backend, the role-interface surface (`IAmABoxMigrationCatalog`, `IAmAVersionDetectingMigrationHelper<TConn,TTx>`, `IAmAProvisioningUnitOfWork`, the `SqlBoxProvisioner` / `SqlBoxMigrationRunner` abstract bases). These already live in `../Brighter/docs/guides/box-provisioning-{adding-columns,new-backend}.md` and the corresponding ADRs, and are intended for Brighter contributors, not Brighter users. The user-facing docs may link to the implementor guides as a "Further Reading" pointer but must not duplicate their content.
- **DynamoDB / MongoDB / Firestore provisioning** — NoSQL backends are out of scope per spec 0023; existing Dynamo/Mongo Outbox/Inbox docs are unchanged.
- **Application database migration** — Brighter's library only manages its own Outbox/Inbox tables. Domain/business tables remain the application's responsibility (e.g. FluentMigrator, EF Core migrations, Flyway). The new docs should call this boundary out explicitly so users don't expect the library to do more than it does.
- **Deep .NET Aspire hosting integration** — the `connectionName` overload IS the Aspire path and IS in scope, but Aspire-specific `IResourceBuilder` extensions, health-check integration, OpenTelemetry enrichment, etc. are out of scope per ADR 0053 §8.
- **Queue/transport table provisioning** (`MsSqlQueueBuilder` etc.) — not covered by `BoxProvisioning`; out of scope.
- **Removing or deprecating the existing static builders** (`SqlOutboxBuilder`, `MsSqlInboxBuilder`, etc.) — the static builders remain a first-class option. Users with enterprise schema-change governance (FluentMigrator, Flyway, Liquibase, DBA-managed change windows) can keep driving Brighter's published DDL through their own tooling. The per-backend Outbox/Inbox docs describe both options as equally valid choices.
- **Spec-internal artefacts** — review notes, acceptance criteria, traceability tables, etc. inside the spec directories are working documents for Brighter contributors. The Docs site documents the shipped feature, not the spec process.

## Documentation Deliverables

The exact filenames are confirmed in the design phase (`/spec:design`), but the inventory is:

| File | Type | Action | Notes |
|------|------|--------|-------|
| `contents/BoxProvisioning.md` | Conceptual | Create | P0 conceptual overview |
| `contents/BoxProvisioningConfiguration.md` | How-to | Create | P0 configuration / call-site API |
| `contents/BoxProvisioningUpgrade.md` | How-to | Create | P1 migration / upgrade story |
| `contents/MSSQLOutbox.md` | Reference | Update | Add provisioning-library callout |
| `contents/MSSQLInbox.md` | Reference | Update | Same |
| `contents/MySQLOutbox.md` | Reference | Update | Same |
| `contents/MySQLInbox.md` | Reference | Update | Same |
| `contents/PostgresOutbox.md` | Reference | Update | Same |
| `contents/PostgresInbox.md` | Reference | Update | Same; note Postgres inbox is V1-only |
| `contents/SqliteOutbox.md` | Reference | Update | Same |
| `contents/SqliteInbox.md` | Reference | Update | Same |
| `contents/BrighterOutboxSupport.md` | Conceptual | Update | Link to new section |
| `contents/BrighterInboxSupport.md` | Conceptual | Update | Link to new section |
| `contents/Glossary.md` | Reference | Update | P1 — add 4–5 new terms |
| `SUMMARY.md` | Index | Update | Add new section |

If discovery during the design phase finds Spanner Outbox/Inbox docs, they get the same callout (with the additional note that Spanner is fresh-install-only).

## SUMMARY.md Changes

Add a new section between the existing "Outbox and Inbox" section and "Health Checks and Observability":

```
## Database Provisioning

 * [Box Provisioning](/contents/BoxProvisioning.md)
 * [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md)
 * [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md)
```

Filenames are subject to confirmation in the design phase. Section name is a recommendation — alternatives like "Outbox/Inbox Provisioning" can be considered during design.

## Constraints

- **Style**: follow `CLAUDE.md` — second-person voice, present tense, active voice. Define jargon on first use; link rather than duplicate.
- **Terminology** (must be used consistently across all new pages):
  - "BoxProvisioning" — the library / NuGet package family.
  - "Outbox" / "Inbox" (always capitalised when referring to Brighter's concepts).
  - "migration chain" — the V1..V_latest ordered list of `BoxMigration` records.
  - "bootstrap" / "fresh install" / "normal migration" — the three runner paths from ADR 0057 §3.
  - "advisory lock" — generic term; per-backend names (`sp_getapplock`, `pg_try_advisory_lock`, `GET_LOCK`, `BEGIN IMMEDIATE`) only when discussing per-backend specifics.
  - "migration history table" — long form on first use, then `__BrighterMigrationHistory` thereafter (or `BrighterMigrationHistory` for Spanner).
  - "fresh-install-only" / "degenerate runner" — used to describe Spanner. Do not say "broken" or "unsupported."
- **Cross-linking**: every new page has a "Further Reading" section linking to the relevant ADRs in the Brighter repo (using `Brighter/docs/adr/...` paths per CLAUDE.md "External Links" guidance) and to the existing Brighter Docs pages (e.g. `BrighterOutboxSupport.md`). The bidirectional links between the new "Database Provisioning" section and the per-backend Outbox/Inbox pages are part of this requirement and must be verified before close-out.
- **Code examples**: all C# examples must compile against the V10 API surface. Anchor at least one end-to-end example in `Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs` so the example is verifiable. Use `using` directives only when required for clarity.
- **No reproduction of implementor content**: the user docs may link to `Brighter/docs/guides/box-provisioning-*.md` as a "Further Reading" pointer for Brighter contributors, but must not duplicate their step-by-step instructions.
- **Brighter source is read-only**: per the project rules, the Brighter repo is not modified. If the implementor guides reference things that have shifted, raise a follow-up rather than editing them.

## Acceptance Criteria

The requirements document is ready to advance to design when:

- **AC-1**: A reader unfamiliar with the BoxProvisioning feature can read this document and understand what the feature is, who it's for, and which user-facing surfaces need documentation.
- **AC-2**: Scope distinguishes P0/P1/P2 clearly; nothing in P0 depends on a deliverable in P1 or P2.
- **AC-3**: Out-of-scope explicitly names implementor-facing material, NoSQL backends, application DB migration, deep Aspire hosting integration — so reviewers can spot scope creep in design.
- **AC-4**: Source material is concrete: every claim about the feature can be traced to a named spec, ADR, guide, or sample file.
- **AC-5**: SUMMARY.md placement is proposed and motivated.

## Next Steps

1. User reviews and approves (or revises) this document.
2. Run `/spec:design` to produce the documentation outline / page-level structure for the P0 + P1 pages.
3. After design approval, `/spec:tasks` breaks the work into writing tasks.
