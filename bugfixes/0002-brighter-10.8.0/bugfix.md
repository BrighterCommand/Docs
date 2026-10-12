# Bugfix: The documentation is written against Brighter 10.7.0, and 10.8.0 has shipped

**Linked release**: Brighter **10.8.0**, on NuGet 2026-10-06
**Status**: Fixed in this change. The follow-ups are listed below
**Source baseline**: Brighter tags `10.7.0` and `10.8.0`; Docs `c4f75c7`
**Procedure**: `RELEASE_CHECKLIST.md`, steps 1–6, plus the two `blockcheck` pins, which postdate the checklist

## Symptom

`python3 tools/versioncheck.py` → **18 stale pins of 18, across 5 pages**. `master`'s `versions` job
failed on every scheduled run from 2026-10-06, an hour after the release, and 2026-10-05's run
passed. That failure is checklist step 1's signal that a release happened.

## Scope, as ruled

Ruled by the maintainer on 2026-10-12, as recommended:
- **In this change:** the checklist, plus every claim the release made **false**
  - the pins and the tools
  - `optioncheck`'s rows
  - the block that stopped compiling
  - the "not in a released package yet" notices for features 10.8.0 shipped
  - the four tutorial replays
- **Not in this change:** the notes of the form *"At Brighter 10.7.0, X misbehaves"*. They stay
  true about 10.7.0, and each needs a run with a control against 10.8.0. They are listed under
  *Follow-ups*.

## What changed

### Pins and tools

| File | Change |
|---|---|
| 5 tutorial pages (`GetStarted`, `TutorialFirstCommand`, `TutorialFirstMessage`, `TutorialDurableOutbox`, `TutorialStreamingWithKafka`) | the 18 lines `versioncheck` named, 10.7.0 → 10.8.0. No other 10.7.0 was on those pages |
| `tools/optioncheck/optioncheck.csproj` | 62 Brighter packages → 10.8.0 (checklist step 5) |
| `tools/optioncheck/Synthesise.cs` | `NullHandlerFactory` gains `CreatePipelineScope() => null`. In 10.8.0, `IAmAHandlerFactory` requires it (`CS0535`). `null` is "no scope to offer", as Brighter's own `ControlBusHandlerFactory` answers |
| `tools/blockcheck/refs/refs.csproj`, `refs-v4/refs-v4.csproj` | 78 Brighter packages each → 10.8.0. The framework-tied pins also move, see below |
| `tools/symbolcheck.py` | `PRODUCT_REFS` brighter tag 10.7.0 → 10.8.0, "the release the documentation is written against". `--verify-list`: all 22 entries still dead at both refs |

**The `blockcheck` pins' companion packages, from the 10.8.0 nuspecs' `net9.0` groups.** The
restore fails `NU1605` without them:

| Package | 10.7.0 takes | 10.8.0 takes |
|---|---|---|
| `Hangfire.Core` (and `Hangfire.AspNetCore`, `Hangfire.SqlServer`) | 1.8.24 | **1.8.25** |
| `Quartz.Extensions.*` | 3.18.1 | **3.22.4** |
| `Microsoft.EntityFrameworkCore` | 9.0.15 | **9.0.20** |
| `TickerQ.Dashboard`, `TickerQ.EntityFrameworkCore` | 9.0.2 | **9.3.0** |
| `Confluent.SchemaRegistry.Serdes.Avro` | 2.15.0 | **2.15.1** |
| `Shouldly` | 4.3.0 (`Directory.Packages.props:150`) | 4.3.0 (`:152`) |

`Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4 is still the latest release for EF Core 9. The four
Hangfire storage packages are still each at their latest stable release. Every comment in the pins
that cited 10.7.0 was re-verified and now cites 10.8.0.

### What `optioncheck` said (checklist step 5's ledger)

After the bump: **11 mismatches across 59 tables**. Every one is a default or an option the release
added or changed:

| Page | Finding | Repair |
|---|---|---|
| `AzureBlobDistributedLock.md` | UNDOCUMENTED `StorageLocationFunc` | row added. It is now a property, so the "public field" paragraph was false and is rewritten |
| `AzureServiceBusConfiguration.md` | UNDOCUMENTED `MaxAutoLockRenewalDuration`, `SqlFilter`, `Rule`, `UseServiceBusQueue` | four rows. The "public fields" paragraph and the publication's "adds no properties" claim were false and are rewritten (`UseServiceBusQueue`, `TimeToLive`) |
| `CommandProcessorConfigurationReference.md` | UNDOCUMENTED `DefaultScopeAffinity` | row, and when it applies (spec 0036, scoped lifetime per pipeline) |
| `DynamoOutbox.md` | UNDOCUMENTED `CausationIndexName` | row; *five* global secondary indexes, not four |
| `GcpPubSubConfiguration.md` | UNDOCUMENTED `deadLetterRoutingKey`, `invalidMessageRoutingKey` | two rows, and how they differ from the native `deadLetter` policy |
| `KafkaConfiguration.md` | UNDOCUMENTED `errorLogLevel` | row |
| `RabbitMQConfiguration.md` | WRONG DEFAULT `isDurable`: `true`, the table said `false` | row; the `RMQ.Async`/`RMQ.Sync` difference, RabbitMQ 4.3's reason, and the upgrade failure |

After the repairs: **0 mismatches across 59 tables, 529 rows**.

### Claims the release made false

- **Replay On Seen shipped in 10.8.0.** `OnceOnlyAction.Replay`, `SupportsCausationTracking` and
  `CausationId` are present at the `10.8.0` tag and absent at `10.7.0`. The pages that said it was
  in "no version you can install today" now state a version floor:
  - `ReplayOnSeen`, `TurningOnReplayOnSeen`, `ReplayOnSeenReference`, `CausationTrackingStores`
  - `PipelineValidation`, three places
  - `DynamoOutbox`, `HandlingPoisonMessages`, `BrighterOutboxSupport`

  The pages were written against Brighter's development branch, so every API name, validation
  message, event name, tag and migration version on them was checked against the tag. They match.
- **`IRequestContext.InstrumentationOptions` shipped in 10.8.0.** `V10MigrationGuide.md` block 12
  (`MyRequestContext`) stopped compiling (`CS0535`). It now implements the property and copies it in
  `CreateCopy`, as `RequestContext` does. The section is rewritten as a live change.
- **SQS/SNS `maximumMessageSize`** (`PostgreSQLBrokerTradeOffs.md`) was "the release after 10.7.0".
  It is in 10.8.0.
- **Rejection routing** (`ErrorHandlingOptions.md`):
  - RabbitMQ (`RMQ.Async` and `RMQ.Sync`) gained `InvalidMessageRoutingKey` in 10.8.0 (commit
    `886a89a1de`).
  - GCP Pub/Sub gained `deadLetterRoutingKey` and `invalidMessageRoutingKey`.
  - The page's "None of those three routes invalid messages separately" was false for two of the
    three.
- **RabbitMQ `isDurable`:** `TutorialFirstMessage.md`'s comment and prose said the queue is
  non-durable. It is durable by default on `RMQ.Async` 10.8.0.
- **`DynamoOutbox.md`, found while checking the notices:**
  - *"`timeout` … nothing reads it"* is false. It is a whole-operation deadline.
  - The `Outstanding`/`Delivered` index names cannot be renamed freely; every index name is an
    attribute argument on `MessageItem`.

### Behavioural claims, run with a control

| Claim | Case | Controls | Probe |
|---|---|---|---|
| `DynamoDbConfiguration.timeout` is a whole-operation deadline; an overrun surfaces as `OperationCanceledException` | 1 ms → `TaskCanceledException`, which is an `OperationCanceledException` | 0 → added; −1 → added; 1 ms with per-call `outBoxTimeout: 0` → added; 500 → added; −2 → `ArgumentOutOfRangeException` | `probe/dynamo-timeout` (DynamoDB Local) |
| An existing non-durable queue fails to redeclare under `RMQ.Async` 10.8.0's durable default, as `ChannelFailureException` on the first receive | default `isDurable` on a non-durable queue → `ChannelFailureException`, inner `PRECONDITION_FAILED - inequivalent arg 'durable'` | `isDurable: false` on the same queue → receives; a fresh queue with the default → declared `durable: true` | `probe/rmq-durable` (RabbitMQ 4.2.9) |

### `blockcheck`

The pins resolve **551** reference assemblies on each pin. Against 10.8.0, before any page repair,
two verdicts moved:
- `CausationTrackingStores.md` #1, FAILED `CS0117` → BUILT. `RequestContextBagNames.CausationId`
  shipped. It is baselined in this change.
- `V10MigrationGuide.md` #12, BUILT → FAILED `CS0535`. Repaired, above.

`ReplayOnSeenReference.md` #1 and `TurningOnReplayOnSeen.md` #1 and #6 lost their `CS0117`, and
still fail on page types.

## Tutorial replays (checklist steps 3–4)

All four rungs were replayed on a clean machine, as `RELEASE_CHECKLIST.md` defines it:
- **SDK 9.0.100**, the pages' prerequisite, pinned by a `global.json`. The machine default, SDK
  10.0.401, downloads the `net9.0` targeting pack and adds about 60 s that a reader does not pay.
- All four NuGet caches redirected and empty. The HTTP cache filled: 29 MB for rung 1, 92 MB for
  rungs 2–4.
- Every block extracted from the page by script.

| Rung | As written on 10.8.0 | Defect | Repair |
|---|---|---|---|
| 1 `TutorialFirstCommand` | **passes**, output identical to the page | — | — |
| 2 `TutorialFirstMessage` | **does not build**: `NU1605`, `Microsoft.Extensions.Hosting` 10.0.12 → 10.0.10 | `ServiceActivator.Extensions.Hosting` 10.8.0 needs Hosting ≥ **10.0.12**, where 10.7.0 needed 10.0.10 | both projects' Hosting pin and the callout that explains it → 10.0.12 |
| 3 `TutorialDurableOutbox` | **does not build**: `NU1605` on Hosting (inherited) and Npgsql 10.0.3 → 10.0.2 | `BoxProvisioning.PostgreSql` 10.8.0 needs Npgsql ≥ **10.0.3** | the Npgsql pin → 10.0.3 |
| 4 `TutorialStreamingWithKafka` | builds and runs; **Step 8 states 10.7.0 behaviour** | #4281 (committed offset one short) is fixed in 10.8.0 (PR #4448). A graceful rebalance re-handles nothing | Step 8 rewritten from the observed listings; the summary bullet too |

With the repairs, every *Expected result* on rungs 2 and 3 matched. That includes rung 2's
queue declared `durable`, the 10.8.0 default, and rung 3's committed and rolled-back runs, with
the Outbox row and `dispatched` behaving as the page says. `versioncheck` cannot catch rungs 2
and 3: it reads only `Paramore.Brighter*` pins, by design. This is the gap checklist step 3 is
for.

**Rung 4's Step 8, measured** (`probe`-style runs in the replay's scratch, each on a fresh broker):

| Run | Listing straight after the second nine | Half a minute later | Re-handled |
|---|---|---|---|
| the page's order: rebalance after the 30 s sweep | — | `3 3 0` after the first nine; `6 6 0` after the second | **0** of 18 |
| rebalance inside the sweep window | `3 6 3` | `6 6 0` | **0** of 18 |
| **case:** a receiver killed with `SIGKILL` inside the sweep window, then a fresh one | `- 3 -` | — | **9 of 9** |

The page's old listing was `2 3 1`, and its note said each receiver handled one greeting again at
step 6. Both were #4281's off-by-one. `KafkaMessageConsumer` commits revoked partitions' stored
offsets (`SetPartitionsRevokedHandler` → `CommitOffsetsFor`, `:261–264`) and commits nothing for
lost ones (`:272`). So at 10.8.0 a duplicate costs a crash, not a rebalance.

**Machine time** is recorded in `RELEASE_CHECKLIST.md`'s run table. Every figure is longer than
the pages quote (rung 1: 95 s against 11 s), and in each the excess is the cold restore at about
280 KB/s. `curl` measured that rate from nuget.org's CDN on the day; build and run times match the
pages. The pages' durations are not changed on one slow day. Re-time them on a normal feed.

## Follow-ups, not in this change

1. **The "At Brighter 10.7.0" notes.** Each is true about 10.7.0. For each, determine whether 10.8.0
   fixed it, with a run and a control, and rewrite or re-date it:
   - the consumer-only Inbox line on the nine Inbox pages and `BrighterInboxSupport.md:86`
   - `FAQ.md:571`
   - `SchedulingAMessage.md:140`, `:213`
   - `InMemoryScheduler.md:241`, `:303`
   - `Telemetry.md:52`, `:70`, `:140`, `:206`
   - `ConfiguringOpenTelemetry.md:21`, `:31`
   - `Monitoring.md:122`
   - `Compression.md:46`
   - `SweeperCircuitBreaking.md:214`
   - `PostgreSQLMessageBroker.md:388`
   - `PostgresOutbox.md:70` (DDL generated at 10.7.0)
   - `BrighterControlAPI.md:71` (sample output's version)
   - `RelationalDatabaseConfigurationReference.md:47` (a count measured at 10.7.0)
2. **Undated behavioural claims against the 10.8.0 release notes' behaviour and breaking changes.**
   Among them:
   - message pumps reject messages with no handler (#4500)
   - backstop handlers preserve explicit pump actions
   - subscription/channel-factory validation becomes breaking (spec 0037)
   - SQS/GCP/RocketMQ `requeueCount` and rejection metadata (#4341)
   - RocketMQ `Requeue`/`Nack`
   - GCP stream fixes
   - RabbitMQ queue declaration at channel creation (#4519)
   - Azure configuration rebuild

   `HandlerFailure.md` is the first page to read against them.
3. **New 10.8.0 features with no page beyond an options row:** scoped lifetime per pipeline
   (spec 0036, `DefaultScopeAffinity`, `AddBrighterRequestScope`), Azure Service Bus queues and
   rules, and GCP rejection routing.
4. **From the replay, not repaired here:**
   - Rung 4's compose file sets `container_name: kafka`. That collides with Brighter's own compose
     stack, or with any other container named `kafka`.
   - The ladder says "solution", but no step creates a `.sln`.
   - Rung 4's "copy your rung 2 solution aside" arrives after rung 3 has already changed it.
   - Rung 2's "order only matters the first time" was not verified.
5. **Spec 018** measured its tables, its `--classify` classes and its baseline at 10.7.0, and its
   design calls `ReplayOnSeenReference.md` forthcoming (D3). Re-derive at phase 3's start.
