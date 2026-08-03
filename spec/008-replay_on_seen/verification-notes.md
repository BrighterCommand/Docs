# Verification Notes — Spec 008 (Replay On Seen)

Source of truth for every symbol, signature, and version number used in this spec's
documentation. Verified against the Brighter working tree (read only). Each section is
produced by one Phase 1 task; do not write an example that is not backed by an entry here.

---

## Task 1.1 — Configuration surface (verified 2026-08-02)

Feeds Examples 1, 2, 3 (Task 2.3) and the `ActionOnExists` edit (Task 2.8).

### `OnceOnlyAction`

`../Brighter/src/Paramore.Brighter/Inbox/OnceOnlyAction.cs`

Namespace: `Paramore.Brighter.Inbox`. Three members, in declaration order:

| Member | Line | XML doc (verbatim) |
|--------|------|--------------------|
| `Throw` | 35 | "Throw OnceOnlyException when OnceOnly is true" |
| `Warn` | 39 | "Log a WARN message when OnceOnly is true" |
| `Replay` | 45 | "When OnceOnly is true and a duplicate is detected, replay the outbox messages produced during the original handling (by clearing their dispatched state so the sweeper resends them) instead of re-running the handler. Requires causation-tracking inbox and outbox support." |

`Throw` is the enum's default (value 0), which matches the `onceOnlyAction` /
`actionOnExists` parameter defaults below.

The `Replay` doc comment is the tightest one-sentence statement of the feature in the
source, and it independently confirms three claims the page makes: the handler is **not**
re-run, the mechanism is **clearing dispatched state** so the **Sweeper** resends, and
both an inbox **and** an outbox must support causation tracking.

### `[UseInbox]` — `UseInboxAttribute`

`../Brighter/src/Paramore.Brighter/Inbox/Attributes/UseInboxAttribute.cs`

Namespace: `Paramore.Brighter.Inbox.Attributes` (the enum is in
`Paramore.Brighter.Inbox`, so an example that names `OnceOnlyAction.Replay` needs **both**
usings).

Two constructors:

```csharp
// line 49 — string contextKey
public UseInboxAttribute(
    int step,
    string? contextKey = null,
    bool onceOnly = false,
    HandlerTiming timing = HandlerTiming.Before,
    OnceOnlyAction onceOnlyAction = OnceOnlyAction.Throw)

// line 65 — Type contextKey, delegates to the above via contextKey.FullName
public UseInboxAttribute(
    int step,
    Type contextKey,
    bool onceOnly = false,
    HandlerTiming timing = HandlerTiming.Before,
    OnceOnlyAction onceOnlyAction = OnceOnlyAction.Throw)
```

Properties: `ContextKey` (get-only, line 37), `OnceOnly` (get-only, line 38),
`OnceOnlyAction` (**`get; set;`** — line 39).

### `[UseInboxAsync]` — `UseInboxAsyncAttribute`

`../Brighter/src/Paramore.Brighter/Inbox/Attributes/UseInboxAsyncAttribute.cs`

```csharp
// line 51 — string contextKey
public UseInboxAsyncAttribute(
    int step,
    bool onceOnly = false,
    string? contextKey = null,
    HandlerTiming timing = HandlerTiming.Before,
    OnceOnlyAction onceOnlyAction = OnceOnlyAction.Throw)

// line 67 — Type contextKey
public UseInboxAsyncAttribute(
    int step,
    Type contextKey,
    bool onceOnly = false,
    HandlerTiming timing = HandlerTiming.Before,
    OnceOnlyAction onceOnlyAction = OnceOnlyAction.Throw)
```

Properties: `OnceOnly`, `ContextKey`, `OnceOnlyAction` — **all three get-only** (lines
39–41), unlike the sync attribute.

> **⚠ Parameter order differs between the two attributes.** In the string-`contextKey`
> constructors the sync attribute is `(step, contextKey, onceOnly, …)` and the async
> attribute is `(step, onceOnly, contextKey, …)` — positions 2 and 3 are swapped. The
> `Type`-`contextKey` overloads happen to agree. **Examples 1 and 2 must use named
> arguments** for everything after `step`; a positional example copied from one attribute
> to the other compiles differently or not at all. This is the single highest-risk detail
> in Task 2.3.

`GetHandlerType()` returns `UseInboxHandler<>` (sync) and `UseInboxHandlerAsync<>`
(async) — the handlers Task 1.2 traces.

### Reference usage — the end-to-end test

`../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/TestDoubles/ProcessAndForwardHandler.cs:55`

```csharp
[UseInbox(1, onceOnly: true, onceOnlyAction: OnceOnlyAction.Replay, contextKey: typeof(ProcessAndForwardHandler))]
public override MyCommand Handle(MyCommand command)
```

All arguments after `step` are named, and `contextKey: typeof(…)` selects the `Type`
overload. Example 1 follows this form exactly, renaming `MyCommand` to the page's
Order/Payment/Shipping domain. Note the handler's own class type is the `contextKey` —
the convention the attribute's XML doc describes ("the name of the handler").

Two further details from the same file, useful to Tasks 2.4 and 3.6:

- Line 65: `_commandProcessor.Post(outgoing, Context as RequestContext);` — the
  `RequestHandler<T>.Context` property is `IRequestContext`, so threading it into `Post`
  requires the `as RequestContext` cast. This is the exact form Example 4's ✅ branch uses.
- The class doc comment states that on a duplicate the pipeline short-circuits **before**
  the handler runs, so nothing is forwarded again — the replay is driven by the outbox.

### `InboxConfiguration` (global configuration)

`../Brighter/src/Paramore.Brighter/InboxConfiguration.cs` — namespace
`Paramore.Brighter` (not `.Inbox`).

```csharp
// line 71
public InboxConfiguration(
    IAmAnInbox? inbox = null,
    InboxScope scope = InboxScope.All,
    bool onceOnly = true,
    OnceOnlyAction actionOnExists = OnceOnlyAction.Throw,
    Func<Type, string>? context = null)
```

Properties: `ActionOnExists` (line 47), `OnceOnly` (52), `Inbox` (57), `Scope` (62) —
**all get-only**; `Context` (69) is `get; set;`.

- **Design §1's assertion is confirmed:** `ActionOnExists` is constructor-set and has no
  setter. You cannot flip an existing configuration to Replay; you construct a new
  `InboxConfiguration`.
- Note the parameter is named `actionOnExists` here but `onceOnlyAction` on the
  attributes — same enum, two names. The page should not use them interchangeably in prose.
- Defaults worth stating: `onceOnly` defaults to `true` here but `false` on the
  attributes; `inbox` defaults to `new InMemoryInbox(TimeProvider.System)` (line 78).
  That default matters for Task 2.5 — the implicit in-memory Inbox is what you get if you
  set `actionOnExists` without naming an `inbox`.
- `InboxScope` is `[Flags]` with `Commands = 1`, `Events = 2`, `All = 3` (lines 30–36).

### Registration shape for Example 3

`ConsumersOptions.InboxConfiguration` is `{ get; set; }` and defaults to `new()` —
`../Brighter/src/Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection/ConsumersOptions.cs:20`.
So the `AddConsumers(options => options.InboxConfiguration = new InboxConfiguration(…))`
shape in design.md is correct.

`contents/BrighterBasicConfiguration.md` (around line 903) already shows this shape with
`inbox:`, `scope:`, `onceOnly:`, `actionOnExists:` named arguments — Example 3 should
match its layout so the two pages read as one convention. (That existing block is missing
a comma after the `inbox:` argument and trails off in `...`; it is illustrative rather
than compiling. Example 3 is abbreviated too, but must not reproduce the missing comma.)

---

## Task 1.2 — Runtime mechanics (verified 2026-08-02)

Feeds §Causation Id (Task 2.2), §Observability (Task 3.4), Example 12 (Task 3.8).

### ⚠ Citing the ADR

**Four different ADRs in `../Brighter/docs/adr/` are numbered 0057.** The one this spec
depends on is `0057-replay-outbox-on-inbox-duplicate.md` ("57. Replay Outbox Messages on
Inbox Duplicate Detection", accepted 2026-04-16). Every reference in the docs must cite
the **filename**, not the bare number, or a reader following it lands on box schema
versioning, in-memory box expiry, or span-based performance instead.

### The context bag key

`../Brighter/src/Paramore.Brighter/RequestContextBagNames.cs:143`

```csharp
public const string CausationId = "Brighter-CausationId";
```

Confirmed literal. The XML doc states it is "distinct from the correlation id
(request-reply), the `JobId`, and the `WorkflowId`" and that "it defaults to the handled
request's id on first handling" — both claims the page makes, sourced here rather than
only from the ADR.

### `IAmACausationTrackingInbox`

`../Brighter/src/Paramore.Brighter/Inbox/IAmACausationTrackingInbox.cs:39` — namespace
`Paramore.Brighter` (**not** `Paramore.Brighter.Inbox`, despite living in the `Inbox`
folder).

```csharp
bool SupportsCausationTracking();
Task<bool> SupportsCausationTrackingAsync(CancellationToken cancellationToken = default);
string? GetCausationId(string id, string contextKey, RequestContext? requestContext, int timeoutInMilliseconds = -1);
Task<string?> GetCausationIdAsync(string id, string contextKey, RequestContext? requestContext, int timeoutInMilliseconds = -1, CancellationToken cancellationToken = default);
```

### `IAmACausationTrackingOutbox`

`../Brighter/src/Paramore.Brighter/IAmACausationTrackingOutbox.cs:41` — namespace
`Paramore.Brighter`.

```csharp
bool SupportsCausationTracking();
Task<bool> SupportsCausationTrackingAsync(CancellationToken cancellationToken = default);
bool ReplayCausation(string causationId, RequestContext? requestContext, Dictionary<string, object>? args = null);
Task<bool> ReplayCausationAsync(string causationId, RequestContext? requestContext, Dictionary<string, object>? args = null, CancellationToken cancellationToken = default);
```

> **⚠ The ADR is out of date here.** ADR 0057 §"New Role Interfaces" (lines 109–114)
> declares `void ReplayCausation` / `Task ReplayCausationAsync`. **The shipped code returns
> `bool` / `Task<bool>`** — `true` if the replay was performed, `false` if it was a no-op
> because the live schema lacks causation tracking ("callers use this to avoid reporting a
> successful replay when nothing was actually resent"). Example 12 must use the shipped
> `bool` signature. Do not copy the ADR's code block.

The return value's meaning is the page's material for the "inbox migrated, outbox not"
mixed state — the store returns `false` rather than throwing.

### The handler branch

`../Brighter/src/Paramore.Brighter/Inbox/Handlers/UseInboxHandler.cs` (async variant
mirrors it at `UseInboxHandlerAsync.cs`).

Sequence on every request (`Handle`, line 88):

1. Throw `ArgumentException("ContextKey must be set before Handling")` if `_contextKey`
   is null (line 90).
2. `ResolveRequestContext()` (line 93) — see below.
3. **Stamp the causation id if absent** (lines 95–96):
   `if (!requestContext.Bag.ContainsKey(RequestContextBagNames.CausationId)) requestContext.Bag[…] = request.Id.Value;`
   Confirms the default is the request's own `Id`, and that a caller who sets the key
   first wins (the escape hatch Task 3.5 documents).
4. Capture `Context?.Span` **once** into a local (line 99) before any branch. The async
   handler must do this before the first `await` because `RequestContext.Span` is
   thread-affine; the sync handler does it for parity.
5. If `_onceOnly` and `_inbox.Exists<T>(…)`, switch on the action (lines 109–126).

The Replay branch (line 121):

```csharp
case OnceOnlyAction.Replay:
    Log.CommandHasAlreadyBeenSeenReplayingOutbox(s_logger, request.Id.Value);
    var (causationId, replayed) = ReplayCausation(request, requestContext);
    WriteReplayEvent(span, request, causationId, replayed);
    return request;
```

`ReplayCausation` (line 178) returns `(null, false)` — nothing replayed — in three cases:

- the inbox is not an `IAmACausationTrackingInbox`, **or** the injected outbox is `null`
  (line 180);
- `GetCausationId` returns `null`, i.e. the stored row predates the migration or was
  written before causation tracking was on (line 184);
- otherwise it returns whatever `_outbox.ReplayCausation(causationId, requestContext)`
  returns (line 187) — `false` when the outbox schema does not support tracking.

These three are exactly the "runtime silences" Task 3.2 lists. Note the handler
**returns the request either way** — a failed replay is indistinguishable from a
successful one to the caller.

The outbox is injected as an optional constructor parameter
(`UseInboxHandler(IAmAnInboxSync inbox, IAmACausationTrackingOutbox? outbox = null)`,
line 67), so a terminal step with no outbox degrades to a plain skip.

### `ResolveRequestContext` and the custom-context warning

Lines 151–164. If `Context is RequestContext` it is returned; otherwise the handler falls
back to `new RequestContext { Span = Activity.Current }` — a throwaway whose Bag never
reaches the outbox `Add`. Before falling back, and only when `_onceOnlyAction is
OnceOnlyAction.Replay`, it logs **once process-wide** (guarded by
`Interlocked.CompareExchange` on a static `int`):

`CustomContextDisablesReplay`, `LogLevel.Warning`, message text verbatim
(`UseInboxHandler.cs:264`, identical at `UseInboxHandlerAsync.cs:282`):

> A custom IRequestContext (not a RequestContext) was supplied; the causation id cannot
> flow to downstream handlers, so OnceOnlyAction.Replay will be a no-op

"Once" is best-effort — the source comment notes a benign race may log it a couple of
extra times under concurrent first hits. Do not tell readers to expect exactly one line.

### Span events

Written by `WriteReplayEvent` (line 202) and `WriteInboxEvent` (line 234). Both are
no-ops unless `span is not null`, `Context is not null`, **and**
`Context.InstrumentationOptions.HasFlag(InstrumentationOptions.Brighter)`.

| Path | Event name | Tags |
|------|-----------|------|
| First handling (stored) | `UseInboxHandler Add` | `paramore.brighter.request.id` |
| Duplicate + Throw | `UseInboxHandler Duplicate Throw` | request id |
| Duplicate + Warn | `UseInboxHandler Duplicate Warn` | request id |
| Duplicate + Replay, messages resent | `UseInboxHandler Duplicate Replay` | request id, `paramore.brighter.causation_id` |
| Duplicate + Replay, nothing resent | `UseInboxHandler Duplicate Replay Skipped` | request id, `paramore.brighter.causation_id` (null) |

- The tag constant is `BrighterSemanticConventions.CausationId =
  "paramore.brighter.causation_id"` (`Observability/BrighterSemanticConventions.cs:54`).
  Confirmed verbatim.
- **The event names are identical in the async handler** — both emit the
  `"UseInboxHandler …"` prefix; there is no `"UseInboxHandlerAsync …"` variant. Verified
  at `UseInboxHandlerAsync.cs:121, 126, 147, 236–237`.
- > **⚠ The ADR is out of date here too.** Its Observability table (line 535) lists only
  > four events and omits `UseInboxHandler Duplicate Replay Skipped`, which shipped. The
  > "Skipped" name is chosen when `causationId is null || !replayed` (line 217) — so
  > operators can tell a real replay from a silent no-op. Task 3.4's table must have five
  > rows, not the ADR's four.
- No new spans are created; events attach to the existing pipeline span. The ADR
  (§"No new spans", line 544) confirms the re-dispatch has **no parent-child link** to the
  replay trigger, because the Sweeper creates its own Activity in `SweepAsync` and may
  pick up messages from several replays at once. That is the sentence Task 3.4 needs.

### Log messages (all from the `Log` nested class, `UseInboxHandler.cs:247`)

| Level | Message template |
|-------|------------------|
| Debug | `Checking if command {Id} has already been seen` |
| Debug | `Command {Id} has already been seen` |
| Warning | `Command {Id} has already been seen` (the `Warn` action) |
| Debug | `Command {Id} has already been seen; replaying its outbox messages` |
| Debug | `Writing command {Id} to the Inbox` |
| Warning | `CustomContextDisablesReplay` (text above) |

Note the replay log is **Debug**, not Information — worth saying plainly in Task 3.2, since
an operator at default log levels sees nothing when a replay happens.

---

## Task 1.3 — Migration versions and column casing (verified 2026-08-02)

Re-derived from the catalog sources, **not** from `design.md`. Gates Tasks 2.9, 2.10, 3.1.

### Outbox — all four catalog backends are at **V8**

| Backend | Max version | V8 `Description` | Column added | Type |
|---------|-------------|------------------|--------------|------|
| MsSql | 8 | `Add CausationId column and replay index` | `CausationId` | `NVARCHAR(255)` |
| MySql | 8 | `Add CausationId column and replay index` | `CausationId` | `VARCHAR(255)` |
| PostgreSql | 8 | `Add causationid column and replay index` | `causationid` | `character varying(255)` |
| Sqlite | 8 | `Add CausationId column and replay index` | `CausationId` | `TEXT` |

Sources: `MsSqlOutboxMigrationCatalog.cs:191`, `MySqlOutboxMigrationCatalog.cs:202`,
`PostgreSqlOutboxMigrationCatalog.cs:203`, `SqliteOutboxMigrationCatalog.cs:168`.

### Inbox — **not uniform**

| Backend | Max version | Description | Column added | Type |
|---------|-------------|-------------|--------------|------|
| MsSql | 3 | `Add CausationId column` | `CausationId` | `NVARCHAR(256)` |
| MySql | 3 | `Add CausationId column` | `CausationId` | `VARCHAR(256)` |
| Sqlite | 3 | `Add CausationId column` | `CausationId` | `TEXT` |
| **PostgreSql** | **2** | `Add causationid column` | `causationid` | `character varying(256)` |

Sources: `MsSqlInboxMigrationCatalog.cs:138`, `MySqlInboxMigrationCatalog.cs:150`,
`SqliteInboxMigrationCatalog.cs:119`, `PostgreSqlInboxMigrationCatalog.cs:132`.

PostgreSQL's inbox is one version behind because its **V1 already created the
`contextkey` column**, so it never needed the separate `ContextKey` migration the other
three have as their V2 (`PostgreSqlInboxMigrationCatalog.cs:41–49`). The chain is one
step shorter — **not absent**. That is the correction Task 2.9 must make at
`BoxProvisioning.md:116`: keep the historical reason, fix the conclusion.

### Column casing

**Never assert one casing across backends.** PostgreSQL uses all-lowercase unquoted
identifiers (`causationid`); MsSql, MySql, Sqlite and Spanner use `CausationId`. The
outbox column is **255** and the inbox column **256** on the sized backends — a real
asymmetry, so do not "tidy" one to match the other.

### The index — new in V8, outbox only

The outbox V8 migration creates **a column and an index**; the inbox migrations create a
column only. This is the first migration in any catalog to create an index — none of the
outbox catalogs indexed any column before V8 (stated in the source comments at
`MsSqlOutboxMigrationCatalog.cs:223`, `SqliteOutboxMigrationCatalog.cs:198`). Index names
differ per backend:

| Backend | Index name | Mechanism |
|---------|-----------|-----------|
| MsSql | `idx_<table>_CausationId` | `IF NOT EXISTS (… sys.indexes …)` then `EXEC('CREATE INDEX …')` |
| MySql | `idx_CausationId` (bare — MySQL scopes index names to the table) | `information_schema.statistics` probe driving a prepared statement |
| PostgreSql | `idx_<table>_causationid` | native `CREATE INDEX IF NOT EXISTS` |
| Sqlite | `idx_<table>_CausationId` | native `CREATE INDEX IF NOT EXISTS` |

### Spanner

`SpannerBoxMigrationRunner.cs:138–139`:

```csharp
public static readonly int VLatestOutbox = 8;
public static readonly int VLatestInbox = 3;
```

Both bumped in step with the relational catalogs, as ADR 0057 requires. `VLatestInbox = 3`
matches MsSql/MySql/Sqlite, not PostgreSql's 2.

---

## Task 1.4 — Store-by-store causation support (verified 2026-08-02)

Gates Tasks 3.1 and 3.10.

### Who implements the role interfaces

Exhaustive grep for `IAmACausationTracking` across `src` (excluding `bin`/`obj`) returns
implementations in: `InMemoryInbox`, `InMemoryOutbox`, `RelationalDatabaseInbox`,
`RelationDatabaseOutbox`, `DynamoDbInbox` (+`.V4`), `DynamoDbOutbox` (+`.V4`),
`FirestoreInbox`/`FirestoreOutbox`, `MongoDbInbox`/`MongoDbOutbox`.

**Every Brighter-maintained inbox and outbox implements its role interface.** There is no
store you must replace — only schemas you may have to migrate. The relational and Spanner
stores inherit the implementation from the two abstract base classes:

- `MsSqlInbox`, `MySqlInbox`, `PostgreSqlInbox`, `SqliteInbox`, `SpannerInboxAsync` all
  derive from `RelationalDatabaseInbox` (`RelationalDatabaseInbox.cs:41`).
- `MsSqlOutbox`, `MySqlOutbox`, `PostgreSqlOutbox`, `SqliteOutbox`, `SpannerOutbox` all
  derive from `RelationDatabaseOutbox` (`RelationDatabaseOutbox.cs:17`).

So Spanner is covered by the same base-class probe as the catalog backends, despite having
no migration catalog of its own.

### What `SupportsCausationTracking()` actually probes

| Store | Inbox probe | Outbox probe |
|-------|-------------|--------------|
| In-memory | `=> true` (`InMemoryInbox.cs:361`) | `=> true` (`InMemoryOutbox.cs:709`) |
| Relational + Spanner | live `CausationId` column-existence query, memoized per store instance (`RelationalDatabaseInbox.cs:278`) | same, `RelationDatabaseOutbox.cs:1009` |
| MongoDb | `=> true` (`MongoDbInbox.cs:259`) | `=> true` (`MongoDbOutbox.cs:853`) |
| Firestore | `=> true` (`FirestoreInbox.cs:498`) | `=> true` (`FirestoreOutbox.cs:856`) |
| DynamoDB / .V4 | `=> true` (`DynamoDbInbox.cs:257`) | **live `DescribeTable` check for the GSI** (`DynamoDbOutbox.cs:573`; `.V4` at the same member) |

**The DynamoDB outbox is the single asymmetry**, exactly as design.md predicted. Confirmed
in both directions:

- The DynamoDB **inbox** returns `true` unconditionally — it needs nothing. `GetCausationId`
  queries the table's own primary keys, so no new index.
- The DynamoDB **outbox** probes `DescribeTableAsync` and checks
  `GlobalSecondaryIndexes.Any(gsi => gsi.IndexName == _configuration.CausationIndexName)`
  (`DynamoDbOutbox.cs:588–596`), memoized in a `bool? _causationIndexExists` field.

### DynamoDB specifics for Task 3.10

- **Default index name:** `CausationIndexName = "Causation"`, set in the
  `DynamoDbConfiguration` constructor — `Outbox.DynamoDB.V4/DynamoDbConfiguration.cs:71`
  and `Outbox.DynamoDB/DynamoDbConfiguration.cs:78`. It is a `{ get; set; }` property
  (line 35 / 42), so it is configurable.
- **⚠ But the attribute hard-codes it.** `MessageItem.CausationId` is decorated
  `[DynamoDBGlobalSecondaryIndexHashKey(indexName: "Causation")]` (`MessageItem.cs:49`) —
  a compile-time literal, not the configuration value. So `DynamoDbTableFactory` always
  generates a GSI named `Causation`; changing `CausationIndexName` without also creating
  a matching index by hand makes the probe report "absent". Task 3.10 should say the
  property exists but that the default is what the table factory produces.
- **The GSI comes for free on a new table.**
  `DynamoDbTableFactory.GenerateCreateTableRequest<T>` calls
  `GetGlobalSecondaryIndices<T>(docType)` and adds the results to the request
  (`DynamoDb.V4/DynamoDbTableFactory.cs:80`), and that reflects over the
  `[DynamoDBGlobalSecondaryIndexHashKey]` attributes. Passing `MessageItem` therefore
  emits the `Causation` GSI with no extra code — Example 8's whole point.
- **GSI shape for Example 9:** hash key `CausationId` (string), no range key.
  `ReplayCausationAsync` issues a `QueryRequest` with `IndexName =
  _configuration.CausationIndexName`, `KeyConditionExpression = "CausationId = :causationId"`,
  and `ProjectionExpression = "MessageId"` (`DynamoDbOutbox.cs` V4:636–648). Since
  `MessageId` is the base table's hash key, a `KEYS_ONLY` projection satisfies the query.
- **`ReplayCausationAsync` degrades, it does not throw**: it returns `false` early when
  the probe says the GSI is absent, precisely so a duplicate does not unwind the pipeline
  with a DynamoDB `ValidationException` on a pre-feature table (V4:620–625).
- Writing the `CausationId` attribute to a table with no GSI is a harmless sparse-index
  no-op, so **normal deposits are unaffected** by skipping the index. Only replay needs it.
- The update loop restores the outstanding marker with
  `UpdateExpression = "SET OutstandingCreatedTime = CreatedTime REMOVE DeliveryTime, DeliveredAt"`,
  and swallows `ConditionalCheckFailedException` because **the GSI is eventually
  consistent** — it can list a `MessageId` whose base item has since been swept or
  TTL-deleted. Worth one sentence in Task 3.10's async-backfill note: a newly created GSI
  backfills asynchronously, so replay is incomplete until the backfill finishes.

---

## Task 1.5 — Validation findings and the end-to-end test (verified 2026-08-02)

Gates Tasks 3.2, 3.6, 3.9.

### ⚠ There are SIX findings, not five

`tasks.md` (written from `design.md`) says "the five findings". The shipped rule
`HandlerPipelineValidationRules.ReplayRequiresCausationTracking(inbox, outbox)`
(`Validation/HandlerPipelineValidationRules.cs:137`) produces **six** distinct findings —
the five the ADR sketched, plus a probe-failure Warning that was added when the probe was
wrapped in `SupportsCausationTrackingSafely` (line 219). Task 3.2's table needs six rows.
Task 3.9's wording ("two of the five messages") should be read as "two of the six".

The rule fires only when a pipeline configures Replay — `OnceOnlyActionOf(step.Attribute)
== OnceOnlyAction.Replay` across `BeforeSteps.Concat(AfterSteps)` (line 141). Non-Replay
pipelines yield nothing. `source` is `$"Handler '{d.HandlerType.Name}'"`.

| # | Severity | Condition | Message (verbatim, after the source prefix) |
|---|----------|-----------|---------------------------------------------|
| 1 | **Error** | inbox is not `IAmACausationTrackingInbox` | `OnceOnlyAction.Replay requires a causation-tracking inbox, but the configured inbox '<Type>' does not implement IAmACausationTrackingInbox — Replay cannot find the causation id of the original handling` |
| 2 | Warning | inbox probe returns `false` | `OnceOnlyAction.Replay requires causation tracking, but the inbox store schema does not support it — migrate the inbox schema to add the CausationId column for Replay to work` |
| 3 | Warning | no outbox configured | `OnceOnlyAction.Replay has no outbox to replay — on a duplicate the handler is skipped and no messages are resent (Replay is a graceful terminal step without an outbox)` |
| 4 | **Error** | outbox is not `IAmACausationTrackingOutbox` | `OnceOnlyAction.Replay requires a causation-tracking outbox, but the configured outbox '<Type>' does not implement IAmACausationTrackingOutbox — Replay cannot reset the dispatched state of the original messages` |
| 5 | Warning | outbox probe returns `false` | `OnceOnlyAction.Replay requires causation tracking, but the outbox store schema does not support it — migrate the outbox schema to add the CausationId column for Replay to work` |
| 6 | Warning | probe **throws** | `OnceOnlyAction.Replay could not verify causation-tracking support on the <inbox\|outbox> store — the schema capability probe failed (<ExceptionType>: <message>). Ensure the store is reachable and its schema is migrated before relying on Replay` |

In the inbox `'<Type>'` slot, a null inbox renders as `(none)`
(`inbox?.GetType().Name ?? "(none)"`).

Notes that matter for the page:

- Findings 1 and 4 are the only **Errors**. Everything else is a Warning — including
  "schema not migrated", which means a mis-provisioned store does not fail startup.
- Finding 6 exists because the probe hits a live store: an unreachable database or a
  missing permission would otherwise crash host startup from inside validation. A throwing
  probe is treated as "capability unknown", and the rule then suppresses finding 2/5 for
  that store (`SupportsCausationTrackingSafely` returns `null`).
- Findings 3 and 4 both `return findings` early — no outbox-schema finding follows them.
- Wiring: `PipelineValidator.ValidateHandlerPipelines()` adds the rule to the standard
  handler-pipeline spec list (`Validation/PipelineValidator.cs:116`), alongside
  `HandlerTypeVisibility`, `BackstopAttributeOrdering`, and `AttributeAsyncConsistency`.
  No separate code path.

### Confirmed: still no sample uses Replay

`grep -rl "OnceOnlyAction.Replay" samples/ --include='*.cs'`, excluding `bin`/`obj`,
returns **nothing** (the apparent hits are copies of `Paramore.Brighter.xml` under
`bin/`). Requirements open question 2 stands and **design deviation 2 is unchanged**:
the reference implementation to cite is the test, not a sample app.

### The end-to-end test

`../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/When_a_seen_message_is_replayed_end_to_end.cs`
— class `EndToEndReplayOnSeenTests`, test method
`When_a_seen_message_is_replayed_end_to_end_through_the_internal_bus` (line 158).

Setup: `InternalBus` as transport; `InMemoryInbox` and `InMemoryOutbox` (both with a
`FakeTimeProvider`); a real `Reactor` pump on a `Performer` background thread; a
`ProcessAndForwardHandler` registered for `MyCommand`. The container registers
`IAmACausationTrackingOutbox` **explicitly** as the same `InMemoryOutbox` instance
(line 129) — the DI detail ADR 0057 §"DI Registration" describes.

Trace, with store state at each step:

| # | Action | Inbox | Outbox | Bus (`MyEvent` topic) |
|---|--------|-------|--------|----------------------|
| 1 | `Post(command, new RequestContext())` — first time | empty | empty | empty |
| 2 | Pump delivers; `UseInboxHandler` stamps `Bag[CausationId] = command.Id`, sees no duplicate, runs the handler | — | — | — |
| 3 | Handler `Post`s `MyEvent`, **threading `Context as RequestContext`** | — | 1 message, `CausationId = command.Id`, dispatched immediately | 1 message |
| 4 | Handler returns; `UseInboxHandler` writes the inbox entry | 1 entry, `CausationId = command.Id` | unchanged | 1 |
| 5 | Assertions: `GetCausationId(command.Id, contextKey, …) == command.Id.Value`; `ReceivedCount == 1` | | | |
| 6 | `Post(command, …)` — **the same command again** | | | |
| 7 | `Exists` returns true → Replay branch → `GetCausationId` → `outbox.ReplayCausation` clears the dispatched state | unchanged | message flips back to **outstanding** | still 1 |
| 8 | Assertion: `ReceivedCount` is **still 1** — the handler did not re-run | | | |
| 9 | `ClearOutbox([outgoingMessageId], …)` re-dispatches | unchanged | dispatched again | **2** messages, both with the *same* message id |

The final assertion (`Assert.All(afterReplay, m => Assert.Equal(outgoingMessageId.Value, m.Id.Value))`)
is the proof that the *same* stored message is resent, not a newly generated one — the
claim Task 2.1 makes in prose.

Two honesty constraints on Task 3.6:

1. **The test does not run a Sweeper.** Line 187 comments "Re-dispatch the replayed message
   to the bus with the same primitive Post uses (no background sweeper needed)" and calls
   `ClearOutbox` directly. Replay's effect in the test is "the message became outstanding
   again"; the Sweeper is what would do step 9 in production. Do not write the trace as if
   a Sweeper ran.
2. **The test is one step, not a cascade.** It has a single handler forwarding a single
   event. Example 5's two-step Order → Payment → Shipping cascade (`ProcessPayment` already
   seen, `ShipOrder` never ran) is **not** traced from this test — the test verifies the
   mechanism for one step. Write Example 5 as an extrapolation and cite the test for the
   mechanism, not for the cascade.

Also useful (a race the test documents at lines 208–219): the handler signals from
*inside* the pipeline, but `UseInboxHandler` writes the inbox entry *after* the inner
handler returns. So "the handler ran" and "the inbox entry exists" are different moments.
