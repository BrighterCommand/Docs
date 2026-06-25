# Design: Distributed Lock (Spec 007)

**Created:** 2026-06-25
**Status:** Draft — awaiting review
**Traceability:** Implements `requirements.md` (approved 2026-06-25)

This document is the documentation outline for the distributed lock topic. It is
written to be picked up and executed file-by-file without re-reading the source.

## 1. Documentation Structure

The topic follows the existing **Outbox pattern**: one general concept page plus one
configuration page per provider, mirroring `BrighterOutboxSupport.md` →
`DynamoOutbox.md` / `PostgresOutbox.md` / ….

```
contents/
├── BrighterOutboxSupport.md        (UPDATE) general Outbox/Sweeper/Archiver page
│      └── adds: Distributed Lock cross-link, Archiver subsection,
│               out-of-process worker example, "always run a Sweeper" note
│
├── DistributedLock.md              (CREATE) general concept + mechanics + deployment
│      ├── DynamoDbDistributedLock.md     (CREATE, P1)
│      ├── PostgresDistributedLock.md     (CREATE, P1)
│      ├── MsSqlDistributedLock.md        (CREATE, P1)
│      ├── MySqlDistributedLock.md        (CREATE, P1)
│      ├── AzureBlobDistributedLock.md    (CREATE, P2)
│      ├── MongoDbDistributedLock.md      (CREATE, P2)
│      └── FirestoreDistributedLock.md    (CREATE, P2)
│
├── Glossary.md                     (UPDATE) add "Distributed Lock" + "Archiver"
└── <transport>Outbox.md            (UPDATE, P2) cross-link near UseOutboxSweeper
```

### Reading Order (for a newcomer)

1. **`BrighterOutboxSupport.md`** — learns the Sweeper exists, must be a singleton,
   and that the Archiver is a related clean-out stage. A link sends them on when they
   scale out.
2. **`DistributedLock.md`** — learns *why* a lock is needed, the `IDistributedLock`
   contract, the default in-memory lock, how to wire `opt.DistributedLock`, and how to
   deploy out-of-process. An overview table routes them to their backend.
3. **`<Provider>DistributedLock.md`** — copies the concrete configuration for their
   chosen backend and is done.

The general page is self-contained; per-provider pages assume the reader has read it
and link back to it for shared concepts.

## 2. File-by-File Outline

### 2.1 `contents/DistributedLock.md` (CREATE) — general page

- **Purpose:** Explain what the distributed lock is, why the Sweeper/Archiver need it,
  the contract, configuration, out-of-process deployment, and route readers to a
  provider page.
- **Target length:** ~200–260 lines.
- **Section outline:**

  ```
  # Distributed Lock
  ## Why a Distributed Lock?            (the single-active-Sweeper/Archiver problem)
  ## The IDistributedLock Contract       (ObtainLockAsync/ReleaseLockAsync semantics)
  ## The Default In-Memory Lock          (InMemoryLock; when it is/ isn't enough)
  ## How the Sweeper and Archiver Use the Lock
                                         (resources "OutboxSweeper" / "Archiver",
                                          obtain → work → release, abandon-on-miss)
  ## Configuring a Distributed Lock      (opt.DistributedLock in AddProducers)
  ## Available Providers                 (overview table → per-provider pages)
  ## Lease Expiry vs Manual Release      (why leases exist; crash safety)
  ## Running Out of Process              (link to BrighterOutboxSupport worker example)
  ## Further Reading
  ```

- **Key code examples:**
  1. The `IDistributedLock` interface, quoted verbatim (2 methods) — source
     `src/Paramore.Brighter/IDistributedLock.cs`. *Complete.*
  2. Canonical `AddProducers` config wiring `opt.DistributedLock` with the DynamoDB V4
     provider + `UseOutboxSweeper` — adapted from the user-supplied snippet, verified
     against `DynamoDbLockingProviderOptions`. *Abbreviated* (`/* ... */` for unrelated
     options). This is the one full example here; deep provider detail is delegated.
- **Provider overview table** (links out):

  | Provider | Backend | Package | Lease model |
  |----------|---------|---------|-------------|
  | [DynamoDB](/contents/DynamoDbDistributedLock.md) | DynamoDB | `Paramore.Brighter.Locking.DynamoDB.V4` | TTL lease / manual |
  | [Postgres](/contents/PostgresDistributedLock.md) | PostgreSQL | `Paramore.Brighter.Locking.PostgresSql` | session advisory lock |
  | [MS SQL](/contents/MsSqlDistributedLock.md) | SQL Server | `Paramore.Brighter.Locking.MsSql` | session app lock |
  | [MySQL](/contents/MySqlDistributedLock.md) | MySQL | `Paramore.Brighter.Locking.MySql` | session `GET_LOCK` |
  | [Azure Blob](/contents/AzureBlobDistributedLock.md) | Azure Blob Storage | `Paramore.Brighter.Locking.Azure` | blob lease |
  | [MongoDB](/contents/MongoDbDistributedLock.md) | MongoDB | `Paramore.Brighter.Locking.MongoDb` | TTL document |
  | [Firestore](/contents/FirestoreDistributedLock.md) | Google Firestore | `Paramore.Brighter.Locking.Firestore` | TTL document |

- **Cross-links:** → `BrighterOutboxSupport.md` (Sweeper/Archiver, worker example),
  `SweeperCircuitBreaking.md` (distinct concern), `Glossary.md`, each provider page.
- **Glossary terms:** define **Distributed Lock** on first use (link to Glossary);
  reference **Sweeper**, **Archiver**, **Advisory Lock**.

### 2.2 Per-provider pages (CREATE) — shared template

All seven pages share one structure so they can be written from a single template.
Each is **short (~50–90 lines)** and concrete, mirroring `DynamoOutbox.md`.

```
# <Provider> Distributed Lock
## Usage                  (1–2 sentences; link back to DistributedLock.md)
## Package                (NuGet package / namespace; backend prerequisites)
## Configuration          (constructor + options class, property-by-property)
## Example                (complete AddProducers snippet with opt.DistributedLock + UseOutboxSweeper)
## Provisioning           (table/container/collection setup, where applicable)
## Notes / Gotchas        (lease validity, manual release, name hashing, timeouts)
```

Per-provider specifics (verified against source — use these exact names):

| Page | Class / ctor | Options & key properties | Backend mechanism | Provisioning notes |
|------|--------------|--------------------------|-------------------|--------------------|
| `DynamoDbDistributedLock.md` | `DynamoDbLockingProvider(IAmazonDynamoDB, DynamoDbLockingProviderOptions)` | `LockTableName`, `LeaseholderGroupId`, `LeaseValidity` (1 min), `ManuallyReleaseLock` (false) | Conditional writes + `LeaseExpiry` | Lock table must exist; note **V4** package, legacy = migration only |
| `PostgresDistributedLock.md` | `PostgresLockingProvider(PostgresLockingProviderOptions)` | `ConnectionString` | `pg_try_advisory_lock` / `pg_advisory_unlock` | None (advisory locks need no table); connection held for lock duration |
| `MsSqlDistributedLock.md` | `MsSqlLockingProvider(MsSqlConnectionProvider)` | uses `MsSqlConnectionProvider` (no options class) | `sp_getapplock` / `sp_releaseapplock`, Exclusive/Session | None |
| `MySqlDistributedLock.md` | `MySqlLockingProvider(MySqlConnectionProvider)` | uses `MySqlConnectionProvider` | `GET_LOCK` / `RELEASE_LOCK`, 1s timeout | Lock name SHA-512 hashed to ≤64 chars |
| `AzureBlobDistributedLock.md` | `AzureBlobLockingProvider(AzureBlobLockingProviderOptions)` | `BlobContainerUri`, `TokenCredential`, `LeaseValidity` (1 min), `StorageLocationFunc` | Blob lease | Blob container must exist |
| `MongoDbDistributedLock.md` | `MongoDbLockingProvider(IAmAMongoDbConfiguration[, connectionProvider])` | locking settings on `IAmAMongoDbConfiguration.Locking` | Insert w/ unique index + TTL | TTL index for expiry |
| `FirestoreDistributedLock.md` | `FirestoreDistributedLock(FirestoreConfiguration[, connectionProvider])` | locking settings on `FirestoreConfiguration.Locking` | Atomic create, precondition `Exists=false` | TTL expiry; resource name normalised |

- **Two configuration shapes** (the template's `## Configuration` section flexes):
  - **Options-class providers** (DynamoDB, Azure Blob, Postgres, MongoDB, Firestore):
    document the options class property-by-property.
  - **Connection-provider providers** (MS SQL, MySQL): these take a
    `MsSqlConnectionProvider` / `MySqlConnectionProvider` and have **no options class**.
    Their `## Configuration` section instead shows obtaining/sharing the connection
    provider (typically the same one the Outbox uses) and notes there are no lease
    settings to tune. Call this asymmetry out explicitly so the two pages don't force a
    non-existent options class.
- **Each page's code example:** a complete `AddProducers` block setting
  `opt.DistributedLock = new <Provider>(...)`. Source: written from the verified
  constructors/options above; adapt the user-supplied DynamoDB snippet for the DynamoDB
  page. *Complete* (small, copy-paste-ready).
- **Cross-links:** every page → `DistributedLock.md` (concept) and the matching
  `<Provider>Outbox.md` where one exists.

### 2.3 `contents/BrighterOutboxSupport.md` (UPDATE)

- **Purpose:** Wire the Sweeper/Archiver content to the new lock docs and add the
  out-of-process worker. Three concrete edits:

  1. **Singleton Sweeper** section (currently lines 176–180): after "Brighter supports
     a range of distributed locks for this purpose," append a sentence linking
     `DistributedLock.md`, and add the "**always run a Sweeper**" note — even
     `InMemoryOutbox` depends on one, but for `InMemoryOutbox` the Sweeper runs
     *in-process* and the default `InMemoryLock` is sufficient; an external Outbox is
     what makes a distributed lock necessary when scaled out.

  2. **Outbox Archiver** section (currently lines 182–190): keep as a section here
     (per requirements — a life-cycle/clean-out stage, not a separate page). Add: it
     shares the same distributed-lock mechanism (resource `"Archiver"`), registration
     via `UseOutboxArchiver<TTransaction>(archiveProvider, opts)`, and a cross-link to
     `DistributedLock.md`. The `archiveProvider` argument is an `IAmAnArchiveProvider`;
     **link the existing [Azure Archive Provider](/contents/AzureBlobArchiveProvider.md)**
     page for a concrete provider rather than re-documenting archive providers here.
     (Note: the SUMMARY.md "Azure Archive Provider Configuration" entry currently points
     at an empty `/contents/` link — flag it; fix only if trivial during writing.)

  3. **New subsection: "Running the Sweeper and Archiver Out of Process"** — the
     copy-paste-ready worker. Placed after the Archiver section.

- **Key code example (the worker):** a complete .NET Generic Host `Program.cs` worker:

  ```csharp
  var dynamoDb = new AmazonDynamoDBClient();
  var archiveProvider = /* your IAmAnArchiveProvider, e.g. an S3/blob archive */;

  var builder = Host.CreateApplicationBuilder(args);
  builder.Services
      .AddSingleton<IAmazonDynamoDB>(dynamoDb)
      .AddBrighter()
      .AddProducers(opt =>
      {
          // ---- swap these three for your backend ----
          opt.Outbox = new DynamoDbOutbox(dynamoDb, new DynamoDbConfiguration { /* ... */ });
          opt.ConnectionProvider = typeof(DynamoDbUnitOfWork);
          opt.TransactionProvider = typeof(DynamoDbUnitOfWork);
          opt.DistributedLock = new DynamoDbLockingProvider(
              dynamoDb, new DynamoDbLockingProviderOptions("LockTable", "sweeper-group"));
          // -------------------------------------------
      })
      .UseOutboxSweeper(opt => { opt.BatchSize = 10; })
      // TTransaction is the Outbox's transaction TYPE, not its provider.
      // For DynamoDB that is TransactWriteItemsRequest (DynamoDbUnitOfWork is the
      // provider, supplied above via opt.ConnectionProvider/TransactionProvider).
      .UseOutboxArchiver<TransactWriteItemsRequest>(
          archiveProvider, opt => { opt.MinimumAge = TimeSpan.FromHours(24); });

  var host = builder.Build();
  await host.RunAsync();
  ```

  Source: composed from the user-supplied snippet + verified against source. The
  generic `UseOutboxArchiver<TTransaction>` (→ `OutboxArchiver<Message, TTransaction>`)
  **must** be given the Outbox's **transaction type**, not the literal `TTransaction`
  and not the transaction *provider*. Verified from source: `DynamoDbOutbox :
  IAmAnOutboxSync<Message, TransactWriteItemsRequest>` and `DynamoDbUnitOfWork :
  IAmABoxTransactionProvider<TransactWriteItemsRequest>` — so for DynamoDB the generic
  is `TransactWriteItemsRequest`, while `DynamoDbUnitOfWork` is the provider used in the
  `opt.ConnectionProvider` / `opt.TransactionProvider` slots.

  **The page must include a short clarifying note** distinguishing the two roles
  (provider vs transaction type), since `UseOutboxArchiver<DynamoDbUnitOfWork>` is a
  natural-looking mistake that will not compile. For other backends the writing task
  confirms the transaction type from that Outbox's `IAmAnOutbox<Message, T>` declaration
  (e.g. the relevant `DbTransaction` for SQL backends). The three swap points (Outbox,
  connection/transaction provider, lock provider) are fenced with a comment band so they
  adapt to any backend. *Complete but templated.*
  Followed by a short, illustrative (not exhaustive) Kubernetes `Deployment` snippet
  noting `replicas: 1` is not required *because* the distributed lock guarantees a
  single active worker even at >1 replica.

- **Cross-links added:** → `DistributedLock.md`, provider pages.

### 2.4 `contents/Glossary.md` (UPDATE)

Add two terms, placed in the existing Outbox cluster (near `### Sweeper`, line 180):

- **`### Distributed Lock`** — "A lock held in shared infrastructure (a database,
  blob store, etc.) that lets multiple application instances coordinate so that only
  one runs a given task — in Brighter, ensuring a single active Outbox Sweeper or
  Archiver. See [Distributed Lock](/contents/DistributedLock.md)." Cross-reference the
  existing **Advisory Lock** term (line 218).
- **`### Archiver`** — "A background process that moves messages older than a
  configured age out of the Outbox into long-term storage, keeping the Outbox small.
  Runs as a singleton, coordinated by a Distributed Lock. See [Outbox
  Archiver](/contents/BrighterOutboxSupport.md#outbox-archiver)."

### 2.5 Transport outbox pages (UPDATE, P2)

For `DynamoOutbox.md`, `PostgresOutbox.md`, `MSSQLOutbox.md`, `MySQLOutbox.md`,
`MongoDBOutbox.md` (and others using `UseOutboxSweeper`): add a one-line cross-link
near the `UseOutboxSweeper()` call pointing to `DistributedLock.md` and the matching
provider page. No structural change.

## 3. SUMMARY.md Changes

**Before** (lines 71–73):

```markdown
 * [Outbox Support](/contents/BrighterOutboxSupport.md)
 * [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)
 * [Inbox Support](/contents/BrighterInboxSupport.md)
```

**After:**

```markdown
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
```

The Archiver gains no SUMMARY entry (it is a section within `BrighterOutboxSupport.md`).

## 4. Code Examples Plan

| # | Example | Location | Source | Complete? |
|---|---------|----------|--------|-----------|
| 1 | `IDistributedLock` interface (2 methods) | DistributedLock.md | `IDistributedLock.cs` (verbatim) | Complete |
| 2 | Canonical `AddProducers` + DynamoDB lock + `UseOutboxSweeper` | DistributedLock.md | User snippet, verified vs options class | Abbreviated |
| 3 | Out-of-process worker `Program.cs` (+ illustrative K8s Deployment) | BrighterOutboxSupport.md | Composed; verified host extensions | Complete (templated swap points) |
| 4 | One `AddProducers` snippet per provider (×7) | each provider page | Written from verified ctors/options | Complete |

All C# examples use ```csharp fencing and V10 APIs. Before finalising, each
constructor/options name is checked against the source files listed in §2.2.

## 5. Style Notes

- **Terminology:** introduce **Distributed Lock** (capitalised on first use, then
  lowercase) and **Archiver**; both added to the Glossary. Use **Sweeper** and
  **Archiver** (never "ServiceActivator"). Refer to the interface as `IDistributedLock`
  and providers by their class names.
- **DynamoDB version:** lead with the **V4** package; add a one-line note that legacy
  (AWS SDK v3) providers exist only to aid migration, mirroring the existing wording in
  `DynamoOutbox.md` (lines 10–20). Do not document the legacy provider in depth.
- **Pattern fidelity:** the general + per-provider split, page structure, and "Usage /
  Package / Configuration / Example" headings deliberately mirror the Outbox docs so the
  section feels native. No deviations from standard patterns.
- **Out-of-process worker:** keep the .NET host example complete and runnable in shape,
  but clearly mark the three swap points (database, Outbox, lock provider) so it adapts
  to any backend; keep the Kubernetes snippet illustrative, not a production manifest.
- **No content duplication:** archive providers and Outbox provisioning are linked, not
  re-documented.

---

**Next step:** Run `/spec:review` to approve this design, then `/spec:tasks`.
