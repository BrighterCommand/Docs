# Requirements: Distributed Lock (Spec 007)

**Created:** 2026-06-25
**Status:** Draft — awaiting review

## Topic Overview

Brighter's **Outbox Sweeper** and **Outbox Archiver** are background processes that
dispatch and archive messages held in an Outbox. For correctness you must run only
**one** Sweeper and **one** Archiver against a given Outbox at any time. As you scale
an application horizontally for resilience and throughput, every instance would
otherwise start its own Sweeper and Archiver, producing duplicate dispatches and
contention.

Brighter solves this with a **distributed lock** — the `IDistributedLock`
abstraction (`../Brighter/src/Paramore.Brighter/IDistributedLock.cs`). The
`TimedOutboxSweeper` and `TimedOutboxArchiver` hosted services attempt to obtain a
named lock before each run; only the instance that acquires the lock does the work,
and the others abandon that tick. Brighter ships several lock providers (DynamoDB,
Postgres, MySQL, MS SQL, Azure Blob, MongoDB, Firestore, plus an in-memory default).

This documentation covers what the distributed lock is, why and when you need it,
the providers available, how to configure it, and the recommended deployment
patterns — including running the Sweeper/Archiver as a separate, scheduled
executable for production.

## Current State

**What exists today:**

- `contents/BrighterOutboxSupport.md` — has a **Singleton Sweeper** section
  (lines ~176-180) that states you should run only one Sweeper and that "Brighter
  supports a range of distributed locks for this purpose," plus a brief **Outbox
  Archiver** section (lines ~182-190). It does *not* show how to configure a lock,
  list providers, or show out-of-process deployment.
- `contents/BrighterBasicConfiguration.md` — one line noting Brighter "provides a
  variety of distributed lock implementations to help you run a single sweeper at a
  time." No detail.
- `contents/SweeperCircuitBreaking.md` — covers circuit breaking for the Sweeper;
  related but a distinct concern (handling failing topics, not single-instance
  coordination). No lock coverage.
- `contents/OutboxPattern.md` — conceptual; no Sweeper/Archiver/lock detail.
- `contents/Glossary.md` — defines **Sweeper** but not **Distributed Lock** or
  **Archiver**.
- Many transport/outbox pages (`DynamoOutbox.md`, `PostgresOutbox.md`, etc.) call
  `.UseOutboxSweeper()` but none configure or explain `DistributedLock`.

**What's missing:**

1. No dedicated documentation for the distributed lock — the `IDistributedLock`
   contract, the list of providers, or their configuration options.
2. No guidance on configuring `opt.DistributedLock` in `AddProducers`.
3. No documentation of the **Outbox Archiver** beyond a brief mention (no
   `UseOutboxArchiver` configuration, archive providers, or scheduling guidance).
4. No guide for running the Sweeper/Archiver **out of process** as a standalone,
   orchestrator-scheduled (e.g. Kubernetes) executable.
5. No `Glossary.md` entries for **Distributed Lock** and **Archiver**.

## Target State

When complete, a reader should be able to:

- Understand why a single Sweeper/Archiver must run per Outbox, and how the
  distributed lock enforces this across instances.
- Understand the `IDistributedLock` contract (resource name, lock id, lease/expiry
  semantics) at a conceptual level.
- Choose the right lock provider for their infrastructure and configure it.
- Wire `opt.DistributedLock` into `AddProducers` and combine it with
  `UseOutboxSweeper` / `UseOutboxArchiver`.
- Decide between running the Sweeper/Archiver in-process versus as a separate
  deployed executable, and know how to do the latter.
- Understand that the Sweeper is always required (even `InMemoryOutbox` depends on
  it), and when the lock matters (multi-instance, external Outbox) versus when the
  default in-memory lock is sufficient (single instance / in-process Outbox).

## Target Audience

- **Primary:** Intermediate Brighter users who already use an Outbox and a Sweeper
  and are now scaling to multiple instances or hardening for production.
- **Secondary:** Advanced users wanting provider-specific detail (lease validity,
  table/container naming, leaseholder groups) and out-of-process deployment.
- Beginners are served by clear "what" and "why" framing and cross-links back to the
  Outbox and Sweeper basics.

## Source Material

**Source code (Brighter — read only):**

- `src/Paramore.Brighter/IDistributedLock.cs` — interface contract
  (`ObtainLockAsync(resource, ct) -> string?`, `ReleaseLockAsync(resource, lockId, ct)`).
- `src/Paramore.Brighter/InMemoryLock.cs` — default in-memory lock (process-local).
- `src/Paramore.Brighter/ProducersConfiguration.cs:175` — `IDistributedLock? DistributedLock { get; set; }`.
- `src/Paramore.Brighter.Extensions.DependencyInjection/ServiceCollectionExtensions.cs`
  (~331-333, ~449-453) — defaults to `InMemoryLock` when none supplied.
- `src/Paramore.Brighter.Outbox.Hosting/TimedOutboxSweeper.cs` — lock resource name
  `"OutboxSweeper"`; obtain/abandon/release pattern.
- `src/Paramore.Brighter.Outbox.Hosting/TimedOutboxArchiver.cs` — lock resource name
  `"Archiver"`; sync/async archive paths.
- `src/Paramore.Brighter.Outbox.Hosting/HostedServiceCollectionExtensions.cs` —
  `UseOutboxSweeper`, `UseOutboxArchiver<TTransaction>` registration.
- Lock providers under `src/Paramore.Brighter.Locking.*`:
  - **DynamoDB** — `DynamoDbLockingProvider(IAmazonDynamoDB, DynamoDbLockingProviderOptions)`;
    options: `LockTableName`, `LeaseholderGroupId`, `LeaseValidity` (default 1 min),
    `ManuallyReleaseLock` (default false). (Also a `.DynamoDB.V4` variant.)
  - **Azure Blob** — `AzureBlobLockingProvider(AzureBlobLockingProviderOptions)`;
    options: `BlobContainerUri`, `TokenCredential`, `LeaseValidity` (default 1 min),
    `StorageLocationFunc`.
  - **Postgres** — `PostgresLockingProvider(PostgresLockingProviderOptions)`; uses
    `pg_try_advisory_lock`; options: `ConnectionString`.
  - **MS SQL** — `MsSqlLockingProvider(MsSqlConnectionProvider)`; uses `sp_getapplock`.
  - **MySQL** — `MySqlLockingProvider(MySqlConnectionProvider)`; uses `GET_LOCK`.
  - **MongoDB** — `MongoDbLockingProvider(IAmAMongoDbConfiguration[, connectionProvider])`;
    TTL-based.
  - **Firestore** — `FirestoreDistributedLock(FirestoreConfiguration[, connectionProvider])`.

**ADRs (Brighter — read only):**

- `docs/adr/0032-remove-semaphore-from-explicit-clear.md` — rationale for
  `IDistributedLock`: run multiple sweepers for resilience but keep only one active.
- `docs/adr/0021-move-archive-methods-...md` — `TimedOutboxArchiver` uses the global
  distributed lock so only one archiver runs.
- `docs/adr/0056-timedoutboxarchiver-sync-fallback.md` — archiver lock usage and
  sync/async fallback.
- `docs/adr/0028-...circuit-breaking...md` — Sweeper circuit breaking (related,
  cross-link only).

**Existing docs to cross-link / update:**

- `contents/BrighterOutboxSupport.md` (Singleton Sweeper, Outbox Archiver sections)
- `contents/BrighterBasicConfiguration.md`
- `contents/SweeperCircuitBreaking.md`
- `contents/DynamoOutbox.md` and other outbox pages
- `contents/Glossary.md`

**User-supplied example** (DynamoDB Outbox + `DynamoDbLockingProvider` +
`UseOutboxSweeper`) — to be tested/adapted as the canonical config example.

## Scope

### P0 — Must have

1. **New general page: Distributed Lock** (`contents/DistributedLock.md`)
   - What the distributed lock is and the problem it solves (single active
     Sweeper/Archiver across instances).
   - The `IDistributedLock` contract at a conceptual level (resource name → lock id;
     `null` means "not acquired, abandon this tick"; lease/expiry vs manual release).
   - The default `InMemoryLock` and when it is (and isn't) sufficient.
   - How `TimedOutboxSweeper` (resource `"OutboxSweeper"`) and `TimedOutboxArchiver`
     (resource `"Archiver"`) acquire/release the lock.
   - How to configure it: `opt.DistributedLock = new <Provider>(...)` inside
     `AddProducers`, shown end-to-end with one canonical (DynamoDB) example.
   - An **overview table** of available providers (provider, backend, package, lease
     model) that **links out to each provider's own configuration page** — provider
     detail lives in the per-provider pages, not here.
   - Cross-links to Outbox, Sweeper, Archiver pages and Glossary.

   **Structure decision:** follow the existing Outbox pattern — a general concept page
   (`DistributedLock.md`, analogous to `BrighterOutboxSupport.md`) plus one
   configuration page per provider (analogous to `DynamoOutbox.md`,
   `PostgresOutbox.md`, …). General mechanics and the "why" live in the general page;
   backend-specific configuration lives in the per-provider pages.

2. **Document configuring the lock on the Sweeper** — update
   `BrighterOutboxSupport.md` (Singleton Sweeper) to point at the new page and show a
   minimal lock configuration. The "always run a Sweeper" note (incl. `InMemoryOutbox`
   running in-process) belongs here.

3. **Glossary entries** — add **Distributed Lock** and **Archiver** to
   `contents/Glossary.md`.

### P1 — Should have

4. **Outbox Archiver documentation** — expand coverage of the Archiver: what it does,
   `UseOutboxArchiver<TTransaction>(archiveProvider, opts)`, `TimedOutboxArchiverOptions`
   (`TimerInterval`, `BatchSize`, `MinimumAge`), the archive provider concept, and the
   shared distributed lock. **Decision:** the Archiver is documented as a **section
   within the Sweeper documentation** (in `BrighterOutboxSupport.md`), framed as a
   life-cycle stage for cleaning out the Outbox — not a separate page.

5. **Out-of-process deployment guidance** — explain the trade-off: in-process Sweeper/
   Archiver competes for thread scheduling; at scale a dedicated, separately deployed
   executable scheduled by Kubernetes (or similar) is recommended. Note how the
   distributed lock still matters if more than one replica of that executable runs.
   **Decision:** provide a **complete, copy-paste-ready** standalone host example — a
   full `Program.cs` that configures the Outbox + distributed lock + Sweeper (and
   Archiver) and runs as a worker. It must be detailed enough that a reader can paste
   it, swap in their own database/Outbox and locking provider, and deploy. Keep the
   orchestrator (Kubernetes) manifest illustrative.

6. **Per-provider configuration pages** — one page per lock provider, each covering:
   the package/namespace, constructor and options class, key settings (lease validity,
   manual-vs-expiry release, table/container/collection naming, leaseholder groups),
   any backend provisioning needed, and a complete `AddProducers` configuration
   snippet. One page per provider that exists in source:
   - `contents/DynamoDbDistributedLock.md` — `DynamoDbLockingProvider` /
     `DynamoDbLockingProviderOptions` (`LockTableName`, `LeaseholderGroupId`,
     `LeaseValidity`, `ManuallyReleaseLock`). Document the **V4** provider
     (`Paramore.Brighter.Locking.DynamoDB.V4`); see Constraints.
   - `contents/PostgresDistributedLock.md` — `PostgresLockingProvider` (advisory locks,
     `ConnectionString`).
   - `contents/MsSqlDistributedLock.md` — `MsSqlLockingProvider` (`sp_getapplock`).
   - `contents/MySqlDistributedLock.md` — `MySqlLockingProvider` (`GET_LOCK`).
   - `contents/AzureBlobDistributedLock.md` — `AzureBlobLockingProvider` /
     `AzureBlobLockingProviderOptions` (`BlobContainerUri`, `TokenCredential`,
     `LeaseValidity`, `StorageLocationFunc`).
   - `contents/MongoDbDistributedLock.md` — `MongoDbLockingProvider` (TTL-based).
   - `contents/FirestoreDistributedLock.md` — `FirestoreDistributedLock`.

   These are **P1** for the backends matching the most-used Outboxes (DynamoDB,
   Postgres, MS SQL, MySQL) and **P2** for the rest (Azure Blob, MongoDB, Firestore).
   The exact split can be confirmed in the design phase.

### P2 — Nice to have

7. Cross-links from each transport outbox page (`DynamoOutbox.md`, `PostgresOutbox.md`,
   etc.) to the Distributed Lock page (and the matching per-provider page) near their
   `UseOutboxSweeper` usage.
8. Brief note distinguishing the distributed lock (single-instance coordination) from
   Sweeper circuit breaking (handling failing topics), cross-linking
   `SweeperCircuitBreaking.md`.

## Out of Scope

- Documenting the Inbox or its providers.
- Re-documenting the Outbox pattern itself (covered by `OutboxPattern.md` /
  `BrighterOutboxSupport.md`) beyond what's needed for context.
- Re-documenting Sweeper circuit breaking (covered by `SweeperCircuitBreaking.md`);
  cross-link only.
- Implementing a custom `IDistributedLock` provider walkthrough (mention extensibility,
  but a full how-to is not required).
- Provisioning scripts for each lock backend's tables/containers beyond noting they are
  required (link to existing provisioning docs where they exist).

## Documentation Deliverables

| File | Action | Description |
|------|--------|-------------|
| `contents/DistributedLock.md` | **Create** | General page: concept, contract, default lock, how Sweeper/Archiver use it, configuration approach, provider overview table linking to per-provider pages, out-of-process deployment. (P0/P1) |
| `contents/DynamoDbDistributedLock.md` | **Create** | DynamoDB (V4) provider configuration. (P1) |
| `contents/PostgresDistributedLock.md` | **Create** | Postgres advisory-lock provider configuration. (P1) |
| `contents/MsSqlDistributedLock.md` | **Create** | MS SQL `sp_getapplock` provider configuration. (P1) |
| `contents/MySqlDistributedLock.md` | **Create** | MySQL `GET_LOCK` provider configuration. (P1) |
| `contents/AzureBlobDistributedLock.md` | **Create** | Azure Blob lease provider configuration. (P2) |
| `contents/MongoDbDistributedLock.md` | **Create** | MongoDB TTL provider configuration. (P2) |
| `contents/FirestoreDistributedLock.md` | **Create** | Firestore provider configuration. (P2) |
| `contents/BrighterOutboxSupport.md` | **Update** | Expand Singleton Sweeper section; add an **Archiver** subsection (life-cycle stage, not a separate page); add the complete out-of-process worker example; cross-link the new pages; add "always run a Sweeper / InMemoryOutbox in-process" note. (P0/P1) |
| `contents/Glossary.md` | **Update** | Add **Distributed Lock** and **Archiver** terms. (P0) |
| Transport outbox pages | **Update (P2)** | Add cross-links to the Distributed Lock pages near `UseOutboxSweeper`. |

## SUMMARY.md Changes

Add the new page(s) under the **Outbox and Inbox** section, after
`SweeperCircuitBreaking.md` and before the transport-specific outbox entries:

```markdown
## Outbox and Inbox

 * [Outbox Support](/contents/BrighterOutboxSupport.md)
 * [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)
 * [Distributed Lock](/contents/DistributedLock.md)
   * [DynamoDB Distributed Lock](/contents/DynamoDbDistributedLock.md)
   * [Postgres Distributed Lock](/contents/PostgresDistributedLock.md)
   * [MSSQL Distributed Lock](/contents/MsSqlDistributedLock.md)
   * [MySQL Distributed Lock](/contents/MySqlDistributedLock.md)
   * [Azure Blob Distributed Lock](/contents/AzureBlobDistributedLock.md)
   * [MongoDB Distributed Lock](/contents/MongoDbDistributedLock.md)
   * [Firestore Distributed Lock](/contents/FirestoreDistributedLock.md)
 * [Inbox Support](/contents/BrighterInboxSupport.md)
 ...
```

The general `DistributedLock.md` page nests the per-provider configuration pages
beneath it (mirroring how the Outbox section is organised). The Archiver is documented
as a section within `BrighterOutboxSupport.md`, so it needs no separate SUMMARY entry.

## Constraints

- Follow `CLAUDE.md`: never modify Brighter/Darker source; only edit the Docs repo.
- Use V10 configuration patterns (`AddProducers`, `opt.DistributedLock`,
  `UseOutboxSweeper`, `UseOutboxArchiver`). No V9 legacy patterns.
- Terminology: use **Sweeper** and **Archiver** (not "ServiceActivator"); use
  **Distributed Lock** consistently; define on first use and add to the Glossary.
- All C# examples use ```csharp highlighting and must compile against the V10 API
  (verify constructor/options names against the source files listed above).
- Use relative `/contents/...` links; cross-link Outbox, Sweeper, Archiver, Glossary.
- Update `SUMMARY.md` for every new file (no orphans).
- Keep the canonical example aligned with the user-provided DynamoDB snippet, but
  verify the exact options class names (`DynamoDbLockingProviderOptions` constructor
  takes `LockTableName` and `LeaseholderGroupId`).
- **DynamoDB provider:** document the **V4** provider
  (`Paramore.Brighter.Locking.DynamoDB.V4`) as the recommended choice. Earlier AWS SDK
  major versions are out of support on AWS; older Brighter DynamoDB providers are kept
  only to aid migration. Note this briefly rather than documenting the legacy provider.
- **Out-of-process worker** example must be complete and copy-paste-ready (full
  `Program.cs` using the generic host / `IHostedService` registration), with clear
  swap points for the database, Outbox, and locking provider.

## Resolved Decisions

1. **Archiver placement:** documented as a **section within the Sweeper docs**
   (`BrighterOutboxSupport.md`), framed as an Outbox life-cycle/clean-out stage — not
   a separate page.
2. **Out-of-process deployment:** provide a **complete, copy-paste-ready** standalone
   worker (`Program.cs`) that a reader can adapt to their database and locking
   provider; Kubernetes manifest stays illustrative.
3. **DynamoDB version:** prefer the **V4** provider; mention legacy providers exist
   only for migration since earlier AWS SDK versions are out of support.
4. **General + per-provider split:** follow the existing Outbox documentation pattern —
   a general `DistributedLock.md` page for concept/mechanics/deployment, plus one
   per-provider configuration page (`<Provider>DistributedLock.md`) for backend-specific
   setup, linked from an overview table and nested under the general page in SUMMARY.md.

---

**Next step:** Run `/spec:review` when ready to approve these requirements.
