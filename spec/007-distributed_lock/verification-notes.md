# Phase 1 Verification Notes (Spec 007)

Verified against Brighter source on 2026-06-25 (read only). These are the authoritative
names for the writing phase. ✅ = matches design.md §2.2; ⚠️ = correction to carry forward.

## Core (Task 1.2)

- `IDistributedLock` — `Task<string?> ObtainLockAsync(string resource, CancellationToken)`,
  `Task ReleaseLockAsync(string resource, string lockId, CancellationToken)`. ✅
- `InMemoryLock : IDistributedLock` — process-local; lock id is `""` on success, `null`
  when not acquired. Case-insensitive resource keys. ✅
- Lock resource names: Sweeper = `"OutboxSweeper"`, Archiver = `"Archiver"`
  (`TimedOutboxSweeper.cs:49`, `TimedOutboxArchiver.cs:62`). ✅
- `DynamoDbOutbox : IAmAnOutboxSync/Async<Message, TransactWriteItemsRequest>`;
  `DynamoDbUnitOfWork : IAmABoxTransactionProvider<TransactWriteItemsRequest>`.
  → `UseOutboxArchiver<TransactWriteItemsRequest>`; `DynamoDbUnitOfWork` is the provider. ✅

## Providers (Task 1.1)

| Provider | Ctor | Options / config |
|----------|------|------------------|
| DynamoDB (V4) | `DynamoDbLockingProvider(IAmazonDynamoDB, DynamoDbLockingProviderOptions[, TimeProvider])` | `DynamoDbLockingProviderOptions(string lockTableName, string leaseholderGroupId)`; props `LockTableName`, `LeaseholderGroupId`, `LeaseValidity` (=1 min), `ManuallyReleaseLock` (=false) ✅ |
| Postgres | `PostgresLockingProvider(PostgresLockingProviderOptions)` | `PostgresLockingProviderOptions(string connectionString)`; prop `ConnectionString` ✅ |
| MS SQL | `MsSqlLockingProvider(MsSqlConnectionProvider)` | **No options class** — connection-provider shape ✅ |
| MySQL | `MySqlLockingProvider(MySqlConnectionProvider)` | **No options class** — connection-provider shape ✅ |
| Azure Blob | `AzureBlobLockingProvider(AzureBlobLockingProviderOptions)` | options ctor `(Uri blobContainerUri, TokenCredential tokenCredential, …)`; props `BlobContainerUri`, `TokenCredential`, `LeaseValidity` (=1 min), `StorageLocationFunc` (public field, default `lock-{resource}`) ✅ |
| MongoDB | `MongoDbLockingProvider(IAmAMongoDbConnectionProvider, IAmAMongoDbConfiguration)` or `(IAmAMongoDbConfiguration)` | locking collection via `IAmAMongoDbConfiguration.Locking` (`MongoDbCollectionConfiguration`: `Name`, `TimeToLive`, …) ✅ |
| Firestore | `FirestoreDistributedLock(FirestoreConfiguration)` or `(IAmAFirestoreConnectionProvider, FirestoreConfiguration)` | locking collection via `FirestoreConfiguration.Locking` (`FirestoreCollection`) ✅ |

All NuGet package ids are the default `Paramore.Brighter.Locking.<Backend>` (Task 1.3). ✅

## Host extensions (Task 1.2)

- `UseOutboxSweeper(Action<TimedOutboxSweeperOptions>?)` → `AddHostedService<TimedOutboxSweeper>()`.
- `UseOutboxArchiver<TTransaction>(IAmAnArchiveProvider archiveProvider, Action<TimedOutboxArchiverOptions>?)`
  → builds `OutboxArchiver<Message, TTransaction>`, `AddHostedService<TimedOutboxArchiver<Message, TTransaction>>()`.

## ⚠️ Corrections to carry into writing

1. **Archiver batch-size property is `ArchiveBatchSize`** (not `BatchSize`).
   `TimedOutboxArchiverOptions`: `TimerInterval` (int s, =15), `MinimumAge` (TimeSpan,
   =24h), `ArchiveBatchSize` (int, =100), `Instrumentation`. The worker example in
   design §2.3 sets `opt.MinimumAge` — correct; if showing batch size use
   `opt.ArchiveBatchSize`.
2. **Sweeper `MinimumMessageAge` is a `TimeSpan`** (default 5s), not milliseconds.
   `TimedOutboxSweeperOptions`: `TimerInterval` (int s, =5), `MinimumMessageAge`
   (TimeSpan, =5s), `BatchSize` (int, =100), `UseBulk` (bool, =false), `Args` (dict).
   Note: existing `BrighterOutboxSupport.md` text says "milliseconds" — do **not**
   propagate that; the new content should use TimeSpan semantics. (Existing doc fix is
   out of scope unless trivial.)
