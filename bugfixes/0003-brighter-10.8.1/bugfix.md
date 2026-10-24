# Bugfix: The documentation is written against Brighter 10.8.0, and 10.8.1 has shipped

**Linked release**: Brighter **10.8.1**
**Status**: Fixed in this change. The follow-ups are listed below
**Source baseline**: Brighter tags `10.8.0` and `10.8.1`; Docs `f4d965f`
**Procedure**: `RELEASE_CHECKLIST.md`, steps 1–6, plus the two `blockcheck` pins and `symbolcheck`'s
release tag, as `bugfixes/0002-brighter-10.8.0/` did

## Symptom

`python3 tools/versioncheck.py` → **18 stale pins of 18, across 5 pages**, found at spec 018 task 6.2.
It read 0 of 18 at task 6.1 the day before. That failure is checklist step 1's signal that a release
happened.

## Scope, as ruled

Ruled by the maintainer on 2026-10-23, as for 10.8.0: a PR of its own, before spec 018 phase 6
resumes. **In this change:** the checklist, the pins and the tools, and every claim the release
made **false**. **Not in this change:** notes that stay true about 10.8.0, and new features with no
page. Both are listed under *Follow-ups*.

The release is larger than its number: 230 commits, 152 source files, and a `fix!` among them.
`release_notes.md` carries no per-version headings, so what is new is what 10.8.1's `## Master`
adds over 10.8.0's.

## What changed

### Pins and tools

| File | Change |
|---|---|
| 5 tutorial pages (`GetStarted`, `TutorialFirstCommand`, `TutorialFirstMessage`, `TutorialDurableOutbox`, `TutorialStreamingWithKafka`) | the 18 lines `versioncheck` named, 10.8.0 → 10.8.1. No other 10.8.0 was on those pages |
| `tools/optioncheck/optioncheck.csproj` | 62 Brighter packages → 10.8.1 (checklist step 5) |
| `tools/blockcheck/refs/refs.csproj`, `refs-v4/refs-v4.csproj` | 78 Brighter packages each → 10.8.1; `Confluent.SchemaRegistry.Serdes.Avro` 2.15.1 → **2.16.0**, the Confluent version `Paramore.Brighter.MessagingGateway.Kafka` 10.8.1 takes. Every comment citing a 10.8.0 dependency re-read against the 10.8.1 nuspecs' `net9.0` groups: Hangfire.Core 1.8.25, Quartz 3.22.4, EF Core 9.0.20 and TickerQ 9.3.0 are unchanged; Shouldly is `Directory.Packages.props:153` |
| `tools/symbolcheck.py` | `PRODUCT_REFS` brighter tag 10.8.0 → 10.8.1. `--verify-list`: all 24 entries still dead at both refs |

### What `optioncheck` said (checklist step 5's ledger)

Green before the bump (0 / 59 / 529). After it: **7 mismatches across 59 tables**, each a default or
option the release added or changed:

| Page | Finding | Repair |
|---|---|---|
| `AzureScheduler.md` | UNDOCUMENTED `AzureServiceBusSchedulerFactory.InstrumentationOptions` | row: `All`, the telemetry of a retry the scheduler sends to a queue |
| `AzureServiceBusConfiguration.md` | UNDOCUMENTED `ForwardTo` | row, and what it does under each `makeChannels` mode; *a requeue delay needs a queue* (`AzureServiceBusConsumer.cs:490`'s warning) |
| `CommandProcessorConfigurationReference.md` | UNDOCUMENTED `Publication.RequeueScheduler` | row: `Native` or `Global` |
| `DispatcherConfigurationReference.md` | WRONG TYPE and MANUAL NOT NEEDED, `InboxConfiguration` | `InboxConfiguration?`, default `null`, the marker's `manual:` removed; the global Inbox is opt-in (below) |
| `HangfireScheduler.md` | MANUAL NOT NEEDED, `Queue` | the three `manual:` lines rewritten: constructing the factory no longer reaches `JobStorage`; the "created eagerly" paragraph was false and is rewritten |
| `RelationalDatabaseConfigurationReference.md` | UNDOCUMENTED `outstandingCountTimeout` | row, and what the timeout bounds |

After the repairs: **0 mismatches across 59 tables, 533 rows**.

### Claims the release made false

- **The global Inbox is opt-in** (Brighter `bed521d21`). `AddConsumers` registers no Inbox unless
  `InboxConfiguration` is set. Repaired on `DispatcherConfigurationReference.md` (the row and a
  paragraph), `BrighterInboxSupport.md` (the attribute section and *Inbox Configuration*),
  `TransactionalMessagingWithTheOutbox.md` (its key point) and `MSSQLTransportInboxAndOutbox.md`. The
  per-store Inbox pages all set `InboxConfiguration` already.
- **`MSSQLTransportInboxAndOutbox.md`'s consumer-only callout** said `InboxConfiguration`'s policy never
  reaches a process with no producers. That was already false at 10.8.0, where
  `BuildCommandProcessor` passes it either way (10.8.0 DI `ServiceCollectionExtensions.cs:789`); the
  opt-in repair rewrites the same paragraph and its troubleshooting entry.
- **The Hangfire factory's client is lazy** (`a006e7514`, #4600). `HangfireScheduler.md` said the
  factory threw as it was constructed when no storage was registered.
- **Azure Service Bus requeue delay** (`BrighterSchedulerSupport.md`): a consumer reading a topic
  subscription directly no longer delays a requeue; only a queue consumer, through `UseServiceBusQueue`
  or `ForwardTo`, does (`AzureServiceBusConsumer.cs:349–362`, `:384–398`).
- **MongoDB transaction provider** (`b6b32adf9`, `fix!`, #4617). `IAmAMongoDbTransactionProvider` is an
  `IAmABoxTransactionProvider<IClientSessionHandle>`; `MongoDBOutbox.md` said passing it directly throws
  and cast it. The example now passes it directly; a note keeps the 10.8.0 behaviour.
- **`BoxProvisioning.md`**: `SchemaName` and `SourceReference` now reject empty values, and the implicit
  conversion turns an empty string into `null` (`af17f91c6`, #4589).
- **`BrighterOutboxSupport.md`'s `TimerInterval`** row described a check that 10.8.0 does not make;
  it is at `TimedOutboxSweeper.cs:84` at 10.8.1. The row now states the version.

- **The Dispatcher health check is obsolete** (spec 0038, #4573). `BrighterServiceActivatorHealthCheck`
  carries `[Obsolete]` (CS0618), naming `AddBrighterConsumerHealthChecks` as its replacement.
  `HealthChecks.md` now registers the two remedy checks, with `/health/live` and `/health/ready` mapped
  by their tags, and keeps the old form under ❌/✅ with the release note's migration order;
  `HowServiceActivatorWorks.md`'s *Health Checks* block follows it.
- **The old check reports channel failures** (`e18e95373`, #4555): Degraded when any consumer has 3 or
  more consecutive channel failures. `HealthChecks.md`'s status table said Healthy whenever every pump
  ran.
- **Two sweeper gauges** (`SweeperMeter.cs` at 10.8.1): `Telemetry.md` said the sweeper records three
  instruments; it records five. A hung sweep is visible, as a `sweep.running_time` that keeps growing,
  where the page said it records nothing. The stall alert is now the release note's
  `running_time > 3 × TimerInterval`, with a `tripped_topics > 0` alert beside it. The whole *Outbox
  Sweeper Metrics* section is 10.8.1's — none of its meters exists at 10.8.0 — and now says so.

### Behavioural claims, run with a control

Each probe runs once per tag, `dotnet run -p:BrighterVersion=10.8.1` (the case) and `10.8.0` (the
control).

| Claim | 10.8.1 | 10.8.0, and the other controls | Probe |
|---|---|---|---|
| `AddConsumers` registers no global Inbox: the same command sent twice reaches its handler twice | handled **2** of 2 | default: handled **1** of 2, second send `OnceOnlyException`; with `InboxConfiguration` on either tag: 1 of 2 | `probe/release-claims` |
| A handler marked `[UseInbox]` with no `InboxConfiguration` fails every send | `Action` overload: `ConfigurationException`, inner *Unable to resolve service for type 'Paramore.Brighter.IAmAnInboxSync'*; `Func` overload: `NullReferenceException` at `UseInboxHandler.cs:90`; handled **0** | 10.8.0: sent, then `OnceOnlyException`, handled 1; with `InboxConfiguration` on either tag, the same | `probe/inbox-attribute` |
| Constructing `HangfireMessageSchedulerFactory` reads no storage; reading `Client` does | construct: ok; read `Client`, no storage: `InvalidOperationException` | 10.8.0: construct throws `InvalidOperationException`; with `JobStorage.Current` set, read `Client`: ok on both | `probe/release-claims` |
| `outstandingCountTimeout` must be greater than zero and at most 2,147,483 s | `0`, `-1 s`, `2147484 s`: `ArgumentOutOfRangeException` | `null`, `500 ms`, `2147483 s`: accepted. Absent at 10.8.0 | `probe/release-claims` |
| `AddCheck<BrighterServiceActivatorHealthCheck>` still activates; it reports a consumer with consecutive channel failures as Degraded; `AddBrighterConsumerHealthChecks` registers a take-out-of-service and a restart check, by tag | old check: Healthy with a good consumer, Degraded with one whose every receive throws `ChannelFailureException`; new: take-out-of-service Degraded, restart Healthy | 10.8.0: the old check reads *Healthy – 2 healthy consumers* with the failing one; the new API does not exist | `probe/consumer-health-checks` |
| `sweep.running_time` grows through a hung sweep; `tripped_topics` counts tripped topics | hung lock: 1.5 then 3.5 s, one `tick.lag` and no duration or sweeps; a tripped topic: 1 | lock unavailable: running time 0, sweeps recorded; tripped topic with lock unavailable, or a breaker that throws: 0. 10.8.0: no sweeper instruments | `probe/sweeper-gauges` |
| `DepositPostAsync` takes the `IAmAMongoDbTransactionProvider` directly | direct and cast: both deposit | 10.8.0: direct throws `InvalidOperationException: Supplied transaction provider doesn't match configured transaction type.`; cast deposits | `probe/mongo-provider` (MongoDB 7 replica set; `output.txt`) |

**Read from source, not run:** the Azure Service Bus topic-subscription warning
(`AzureServiceBusConsumer.cs:490`), which needs a Service Bus namespace.

### `blockcheck`

Against 10.8.1 the pins resolve **553** reference assemblies on each pin. `--report` → **992 / 642 /
323 / 27, 0 findings**, and the report is byte-identical to 10.8.0's, diagnostic codes included: the
release touched no API any block uses.

## Tutorial replays (checklist steps 3–4)

All four rungs replayed on a clean machine, as `RELEASE_CHECKLIST.md` defines it: SDK 9.0.100 pinned
by `global.json`, all four NuGet caches redirected and verified empty, every block extracted by
script. The feed ran at ~17 MB/s. The 10.8.1 nuspecs keep 10.8.0's third-party floors (Hosting
10.0.12, Npgsql 10.0.3), which the pages already pin.

| Rung | As written on 10.8.1 |
|---|---|
| 1 `TutorialFirstCommand` | **passes**, output identical to the page |
| 2 `TutorialFirstMessage` | **passes**; queue declared durable, exchange as the page says |
| 3 `TutorialDurableOutbox` | **passes**; step 4's single `CS0246` as the page says; sweep delay 10.0–10.3 s; the `--fail` run writes nothing |
| 4 `TutorialStreamingWithKafka` | **passes**; Step 8's listings `3 6 3` then `6 6 0`, identical to the page, 0 of 18 re-handled |

Machine times are in `RELEASE_CHECKLIST.md`'s run table. Each is at or under the figure its page
quotes, so no duration changes.

## Follow-ups, not in this change

1. **An upstream issue, the maintainer's to file or not:** `AddConsumers`' `Func<IServiceProvider,
   ConsumersOptions>` overload registers `IAmAnInboxSync`/`IAmAnInboxAsync` factories that return `null`
   when no `InboxConfiguration` is set (10.8.1 DI `ServiceCollectionExtensions.cs:105–125`). A handler
   marked `[UseInbox]` then fails with a `NullReferenceException`, where the `Action` overload names the
   missing service.
2. **Two more for upstream, the maintainer's to file or not:**
   - `release_notes.md:147` says the obsolete Dispatcher check *"still returns the same status for
     the same conditions"*; from 10.8.1 it also reports Degraded on channel failures (#4555). The pages
     follow the code
   - a channel whose `Receive` throws `ChannelFailureException` reached 28,000–44,000 consecutive
     failures in 2 s with `channelFailureDelay` at 100 ms, which looks like the delay not being applied
     (`Reactor.cs:136`, `:145`). Seen in the health-check probe; not investigated
   - the 10.8.0 control of the sweeper probe crashed the process when a sweep failed, as #4560 says
     10.8.1 no longer does. A page describing sweep failures without a 10.8.1 floor is wrong for 10.8.0
3. **From the replay:**
   - `TutorialStreamingWithKafka.md` Step 5 states the receive order (alice, grace, mia) unconditionally;
     two runs saw it differ. Step 7 already says the order may differ; Step 5 should too.
   - Step 8's *straight after* listing is a race against the 30 s sweep.
   - Rung 4's `container_name: kafka` still collides with Brighter's own compose stack (0002's
     follow-up 4, still open).
4. **`MongoDBOutbox.md`'s example does not create the Outbox collection.** On both tags the first
   transactional write fails *"Collection namespace 'ProbeDb.Outbox' is already in use"* unless the
   collection exists first. Not caused by the release.
5. **Dated notes 10.8.1 may have fixed**, each to be re-run before it is rewritten:
   - the *"Resolve Hangfire's client first: that initialises its storage (Brighter#4600)"* comments on
     `HangfireScheduler.md`, `BrighterSchedulerSupport.md`, `InMemoryOptions.md`, `InMemoryScheduler.md`,
     `SchedulingAMessage.md` and `SwitchingSchedulers.md`. The code still works; the reason is obsolete
   - `HowServiceActivatorWorks.md`'s line citations at 10.8.0 (`Reactor.cs:208` and `Proactor.cs:247`
     move by four at 10.8.1); they are D21's skip markers, so a re-pin moves the markers too
   - `SweeperCircuitBreaking.md`'s hand-written circuit-breaker health check, now covered by the
     built-in check and the `tripped_topics` gauge
   - `RelationalDatabaseConfigurationReference.md`'s *"measured at `10.8.0`"* count (10.8.1 adds no
     component)
6. **New 10.8.1 features with no page:** the sweeper remedy health checks (`AddBrighterOutboxSweeperHealthChecks`) and the
   remedy-kind convention; `Dispatcher.GetLifecycleSnapshot()`; the Outbox meter
   (`paramore.brighter.outbox.*`); Azure Service Bus global retry mode; RMQ.Sync's coalesced confirms;
   Dispatcher shutdown's exceptions; `MaxOutStandingMessages` behaviour (#4552, #4554, #4584); a
   10.8.1 entry in `V10MigrationGuide.md`'s per-release breaking changes.
7. **Spec 018** measured phase 6's figures at 10.8.0 (tasks 6.1 and 6.2). Re-derive at the phase's
   resume; this change predicts no verdict moves, since the report is identical.
