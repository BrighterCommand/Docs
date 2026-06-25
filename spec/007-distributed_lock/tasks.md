# Tasks: Distributed Lock (Spec 007)

**Created:** 2026-06-25
**Status:** Draft — awaiting review
**Traceability:** Implements `design.md` (approved 2026-06-25)

## Overview

**Total tasks: 23**, across 4 phases.

| Phase | Goal | Tasks |
|-------|------|-------|
| 1. Research & Verification | Confirm all class/option names and transaction types against source before writing | 3 |
| 2. Core Documentation (P0/P1) | Write the general page, update the Outbox page, add Glossary terms, write P1 provider pages | 9 |
| 3. Supporting Documentation (P2) | Write P2 provider pages and transport cross-links | 5 |
| 4. Polish & Review | SUMMARY.md, link/code verification, final read-through | 6 |

**Priority mapping (from requirements):**
- **P0:** general `DistributedLock.md`, `BrighterOutboxSupport.md` updates, Glossary.
- **P1:** DynamoDB, Postgres, MS SQL, MySQL provider pages; out-of-process worker.
- **P2:** Azure Blob, MongoDB, Firestore provider pages; transport cross-links.

**Source-of-truth note:** every C# example must be verified against the Brighter
source (read only, never modify). Key files are listed per task; the authoritative
constructor/option names are tabulated in `design.md` §2.2.

---

## Phase 1 — Research & Verification

> Goal: lock down every name and type the writing phase depends on, so no example
> ships a guessed symbol. Must complete before Phase 2.

- [x] **Task 1.1:** Verify all lock provider constructors and options classes
  - Input: `../Brighter/src/Paramore.Brighter.Locking.*/` (DynamoDB.V4, PostgresSql,
    MsSql, MySql, Azure, MongoDb, Firestore), plus `IDistributedLock.cs`,
    `InMemoryLock.cs`
  - Output: a confirmed name table appended as a comment block at the top of a scratch
    note (or inline ticks against `design.md` §2.2) — class names, ctor params, option
    property names/defaults
  - Notes: Confirm DynamoDB options ctor `DynamoDbLockingProviderOptions(LockTableName,
    LeaseholderGroupId)` and that MS SQL/MySQL take a connection provider (no options
    class). Flag any mismatch with the design before writing.

- [x] **Task 1.2:** Verify the out-of-process worker types and host extensions
  - Input: `../Brighter/src/Paramore.Brighter.Outbox.Hosting/HostedServiceCollectionExtensions.cs`,
    `TimedOutboxSweeper.cs`, `TimedOutboxArchiver.cs`,
    `../Brighter/src/Paramore.Brighter.Outbox.DynamoDB.V4/DynamoDbOutbox.cs`,
    `../Brighter/src/Paramore.Brighter.DynamoDb.V4/DynamoDbUnitOfWork.cs`
  - Output: confirmation that `DynamoDbOutbox : IAmAnOutbox<Message, TransactWriteItemsRequest>`
    and `DynamoDbUnitOfWork : IAmABoxTransactionProvider<TransactWriteItemsRequest>`;
    confirmed `UseOutboxSweeper` / `UseOutboxArchiver<TTransaction>` signatures and the
    lock resource names (`"OutboxSweeper"`, `"Archiver"`)
  - Notes: This is the provider-vs-transaction-type point — `TTransaction` =
    `TransactWriteItemsRequest`, `DynamoDbUnitOfWork` is the provider. Settle it here.

- [x] **Task 1.3:** Confirm NuGet package names and existing cross-link targets
  - Input: `.csproj`/`.nuspec` under each `Paramore.Brighter.Locking.*`; existing docs
    `contents/DynamoOutbox.md`, `contents/AzureBlobArchiveProvider.md`,
    `contents/Glossary.md`, `contents/SweeperCircuitBreaking.md`, `SUMMARY.md`
  - Output: confirmed package strings for the overview table; confirmed anchors for
    cross-links; note the broken SUMMARY "Azure Archive Provider Configuration" link
  - Notes: Mirror the V4-first wording already in `DynamoOutbox.md` lines 10–20.

---

## Phase 2 — Core Documentation (P0 / P1)

> Goal: write the foundational general page first (provider pages link back to it),
> then the Outbox/Glossary updates, then the four P1 provider pages.
> Task 2.1 should complete before the provider pages (2.5–2.8) so they can link to it.

- [x] **Task 2.1:** Write `contents/DistributedLock.md` — concept through configuration
  - Input: `design.md` §2.1; verified names from Phase 1; `IDistributedLock.cs`
  - Output: `DistributedLock.md` H1 + sections: Why a Distributed Lock?, The
    IDistributedLock Contract, The Default In-Memory Lock, How the Sweeper and Archiver
    Use the Lock, Configuring a Distributed Lock
  - Notes: Include the verbatim `IDistributedLock` interface and the canonical DynamoDB
    `AddProducers` snippet. Explain `null` = "not acquired, abandon this tick" and the
    resources `"OutboxSweeper"` / `"Archiver"`. Define **Distributed Lock** on first use.

- [x] **Task 2.2:** Write `contents/DistributedLock.md` — providers, lease, deployment, further reading
  - Input: `design.md` §2.1 (overview table), Phase 1 package names
  - Output: remaining sections: Available Providers (overview table linking the 7
    provider pages), Lease Expiry vs Manual Release, Running Out of Process (link to
    `BrighterOutboxSupport.md` worker), Further Reading
  - Notes: The provider table links out only — no per-backend config here. Some link
    targets (provider pages) are created in Tasks 2.5–2.8 and 3.1–3.3; those links are
    validated later in Task 4.3, so writing them ahead of their targets is expected.
    Depends on 2.1.

- [x] **Task 2.3:** Update `contents/BrighterOutboxSupport.md` — Singleton Sweeper + always-run-a-Sweeper note
  - Input: `design.md` §2.3 edit 1; current lines 176–180
  - Output: edited Singleton Sweeper section linking `DistributedLock.md`, plus the
    note that you always need a Sweeper — `InMemoryOutbox` runs it in-process with the
    default `InMemoryLock`; an external Outbox scaled out is what requires a distributed
    lock
  - Notes: Keep existing prose; append/weave, don't rewrite wholesale.

- [x] **Task 2.4:** Update `contents/BrighterOutboxSupport.md` — Archiver subsection + out-of-process worker
  - Input: `design.md` §2.3 edits 2 & 3; verified worker types (Task 1.2);
    `contents/AzureBlobArchiveProvider.md`
  - Output: expanded Outbox Archiver section (shared lock resource `"Archiver"`,
    `UseOutboxArchiver<TTransaction>` registration, link to Azure Archive Provider) and
    a new "Running the Sweeper and Archiver Out of Process" subsection with the complete
    `Program.cs` worker + illustrative K8s `Deployment` snippet
  - Notes: Include the provider-vs-transaction-type clarifying note
    (`UseOutboxArchiver<DynamoDbUnitOfWork>` won't compile). K8s snippet stays
    illustrative; call out that the lock removes the `replicas: 1` requirement.

- [x] **Task 2.5:** Write `contents/DynamoDbDistributedLock.md` (P1)
  - Input: `design.md` §2.2 (DynamoDB row); user-supplied DynamoDB snippet; Task 1.1
  - Output: provider page (Usage / Package / Configuration / Example / Provisioning /
    Notes) for `DynamoDbLockingProvider` + `DynamoDbLockingProviderOptions`
    (`LockTableName`, `LeaseholderGroupId`, `LeaseValidity`, `ManuallyReleaseLock`)
  - Notes: Lead with the **V4** package; one-line legacy=migration-only note. Link back
    to `DistributedLock.md` and `DynamoOutbox.md`. Depends on 2.1.

- [x] **Task 2.6:** Write `contents/PostgresDistributedLock.md` (P1)
  - Input: `design.md` §2.2 (Postgres row); `PostgresLockingProvider` source; Task 1.1
  - Output: provider page; options class `PostgresLockingProviderOptions`
    (`ConnectionString`); note advisory locks need no table and the connection is held
    for the lock's duration
  - Notes: Link back to `DistributedLock.md` and `PostgresOutbox.md`. Depends on 2.1.

- [x] **Task 2.7:** Write `contents/MsSqlDistributedLock.md` (P1)
  - Input: `design.md` §2.2 (MS SQL row); `MsSqlLockingProvider` source; Task 1.1
  - Output: provider page using the **connection-provider** Configuration shape (takes
    `MsSqlConnectionProvider`, no options class); note `sp_getapplock`, Exclusive/Session
  - Notes: Use the connection-provider template variant (no lease settings). Link back
    to `DistributedLock.md` and `MSSQLOutbox.md`. Depends on 2.1.

- [x] **Task 2.8:** Write `contents/MySqlDistributedLock.md` (P1)
  - Input: `design.md` §2.2 (MySQL row); `MySqlLockingProvider` source; Task 1.1
  - Output: provider page using the connection-provider shape (`MySqlConnectionProvider`,
    no options class); note `GET_LOCK`/`RELEASE_LOCK`, 1s timeout, SHA-512 name hashing
    to ≤64 chars
  - Notes: Link back to `DistributedLock.md` and `MySQLOutbox.md`. Depends on 2.1.

- [x] **Task 2.9:** Add Glossary terms (P0)
  - Input: `design.md` §2.4; `contents/Glossary.md` around line 180 (Sweeper) and 218
    (Advisory Lock)
  - Output: `### Distributed Lock` and `### Archiver` entries in the Outbox cluster,
    cross-referencing Advisory Lock and the new pages
  - Notes: Match existing glossary term style (short definition + "See:" link).

---

## Phase 3 — Supporting Documentation (P2)

> Goal: the remaining provider pages and transport cross-links. Independent of each
> other; all depend on Task 2.1 (general page must exist to link back to).

- [x] **Task 3.1:** Write `contents/AzureBlobDistributedLock.md` (P2)
  - Input: `design.md` §2.2 (Azure Blob row); `AzureBlobLockingProvider` source
  - Output: provider page; options `AzureBlobLockingProviderOptions` (`BlobContainerUri`,
    `TokenCredential`, `LeaseValidity`, `StorageLocationFunc`); blob container must exist
  - Notes: Link back to `DistributedLock.md`.

- [x] **Task 3.2:** Write `contents/MongoDbDistributedLock.md` (P2)
  - Input: `design.md` §2.2 (MongoDB row); `MongoDbLockingProvider` source
  - Output: provider page; configuration via `IAmAMongoDbConfiguration.Locking`;
    TTL-document expiry; unique index
  - Notes: Link back to `DistributedLock.md` and `MongoDBOutbox.md`.

- [x] **Task 3.3:** Write `contents/FirestoreDistributedLock.md` (P2)
  - Input: `design.md` §2.2 (Firestore row); `FirestoreDistributedLock` source
  - Output: provider page; configuration via `FirestoreConfiguration.Locking`; atomic
    create with `Exists=false`; TTL expiry; resource-name normalisation
  - Notes: Link back to `DistributedLock.md`.

- [x] **Task 3.4:** Add cross-links to relational transport outbox pages (P2)
  - Input: `contents/DynamoOutbox.md`, `PostgresOutbox.md`, `MSSQLOutbox.md`,
    `MySQLOutbox.md`
  - Output: one-line cross-link near each `UseOutboxSweeper()` call → `DistributedLock.md`
    and the matching provider page
  - Notes: No structural change; minimal edits.

- [x] **Task 3.5:** Add cross-links to remaining transport outbox pages (P2)
  - Input: `contents/MongoDBOutbox.md` and any other outbox pages calling
    `UseOutboxSweeper`
  - Output: same one-line cross-link pattern as 3.4
  - Notes: Skip `InMemoryOptions.md` unless a natural fit; in-memory needs no
    distributed lock.

---

## Phase 4 — Polish & Review

> Goal: integrate into navigation and verify everything compiles, links resolve, and
> terminology is consistent. Run after Phases 2–3.

- [x] **Task 4.1:** Update `SUMMARY.md`
  - Input: `design.md` §3 (before/after); created files
  - Output: `[Distributed Lock]` entry after Sweeper Circuit Breaking with the 7
    provider pages nested beneath it
  - Notes: Spaces not tabs; consistent indentation. No Archiver entry (it's a section).

- [x] **Task 4.2:** Verify all C# examples against source
  - Input: every created/edited page; Phase 1 verification notes
  - Output: confirmation that all class/option/type names compile against V10 API;
    fixes for any drift
  - Notes: Special attention to the worker's `UseOutboxArchiver<TransactWriteItemsRequest>`
    and the MS SQL/MySQL connection-provider snippets.

- [x] **Task 4.3:** Verify all internal links resolve
  - Input: all new/edited pages, `SUMMARY.md`
  - Output: confirmation every `/contents/...` link and anchor resolves; fixes for
    broken ones
  - Notes: Includes the back-links from provider pages → `DistributedLock.md` and the
    `#outbox-archiver` anchor used by the Glossary.

- [x] **Task 4.4:** Decide on the pre-existing broken Azure archive SUMMARY link
  - Input: `SUMMARY.md` line 66 (`Azure Archive Provider Configuration` → `/contents/`)
  - Output: fix to point at `AzureBlobArchiveProvider.md` if trivial and correct;
    otherwise leave and note it
  - Notes: Out of strict scope; fix only if low-risk (per design §2.3 note).

- [x] **Task 4.5:** Terminology and style pass
  - Input: all new/edited pages; `contents/Glossary.md`, `BasicConcepts.md`
  - Output: consistent use of **Sweeper**, **Archiver**, **Distributed Lock**,
    `IDistributedLock`; second person, active voice; ≤500 lines per file
  - Notes: No "ServiceActivator". Confirm DynamoDB V4-first framing throughout.

- [x] **Task 4.6:** Final read-through against the QA checklist
  - Input: `CLAUDE.md` Quality Assurance Checklist; all deliverables
  - Output: tick the relevant items in the `README.md` Status Checklist (Writing
    complete, Documentation reviewed) and add a short closing summary note beneath it
    confirming code/content/structure/accuracy checks pass
  - Notes: Update the existing Status Checklist in `README.md` rather than adding a new
    section; mark the spec ready to close after this task.

---

## Dependency Summary

- **Phase 1** (1.1–1.3) → gates all writing.
- **Task 2.1** (general page core) → gates 2.2 and every provider page (2.5–2.8, 3.1–3.3)
  because they link back to it.
- **Tasks 2.3 / 2.4** (Outbox page) and **2.9** (Glossary) are independent of the
  provider pages and can run in parallel with them once Phase 1 is done.
- **Phase 3** tasks are mutually independent.
- **Phase 4** runs last; 4.1 (SUMMARY) before 4.3 (link check).

---

**Next step:** Run `/spec:review` to approve these tasks, then `/spec:implement` to
start writing.
