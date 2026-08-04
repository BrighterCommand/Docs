# Requirements: Spec 009 — Getting Started Tutorials

**Created:** 2026-08-03
**Status:** Approved 2026-08-03 (reviewed; five findings addressed in this revision)
**Responds to:** [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)

## Topic Overview

Brighter has no tutorial. Not a thin one — none at all.

A tutorial, in the Diátaxis sense, is a **guaranteed successful learning experience**:
the reader follows it exactly, it works, and they finish more confident than they
started. It is explicitly *not* the place for options, trade-offs or completeness.
Every fork in the road is a place the learner can fail, and a learner who fails at
step 4 does not file an issue — they write a Reddit thread.

This spec adds a four-rung ladder of tutorials, each ending in something the reader
has actually run on their own machine, and each backed by a compiled, CI-built sample
in `../Brighter/samples/`.

## Current State

### What a newcomer meets today

`SUMMARY.md` opens with an *Overview* section of three pages. None of them is a
tutorial:

| Page | Lines | What it actually is |
|---|---|---|
| `contents/ShowMeTheCode.md` | 221 | A showcase. Opens by stating *"It's not about how… It's not about why."* Disconnected snippets; you cannot follow it to a running result. |
| `contents/BasicConcepts.md` | 137 | 24 term definitions — a glossary, and a subset of `Glossary.md`'s 100 terms. |
| `contents/WhyBrighter.md` | 66 | Philosophy: Reactor pattern, type over convention. |

So the on-ramp is a trailer, a vocabulary list, and an argument. Nowhere do we say
*"do these seven things and you will have a working message flow."*

### Where a newcomer is sent instead

`contents/BrighterBasicConfiguration.md` — 1,068 lines, every option, every
transport, no single path through it. It is a good reference and a hostile first
experience.

### What exists in the samples repository

Surveyed 2026-08-03 against `../Brighter` at release **10.7.0**. Cite these findings
rather than re-deriving them.

| Sample | Shape | Fit for a tutorial |
|---|---|---|
| `samples/CommandProcessor/HelloWorld` | 3 files, 114 lines. `AddBrighter().AutoFromAssemblies()`, `Send`, no transport. | **Excellent.** Already minimal. This is rung 1 essentially as-is. |
| `samples/TaskQueue/RMQTaskQueue` | Sender / ReceiverConsole / shared `Greetings` library. `docker-compose-rmq.yaml` at repo root. | **Close, but carries extras a tutorial must not show:** Serilog wiring, a `CustomPublicationFinder` subclass, two event types, explicit `UseScheduler(new InMemorySchedulerFactory())`. Each is a deliberate teaching point for that sample and a fork for a learner. |
| `samples/WebAPI/WebAPI_Dapper` | Ports-and-adapters solution: `GreetingsWeb`, `GreetingsApp`, `Greetings_Sweeper`, `SalutationAnalytics`, migration assemblies, OpenTelemetry. | **Not tutorial-shaped.** Configuration is an env-var matrix — `BRIGHTER_GREETINGS_DATABASE` ∈ {Sqlite, MySql, Postgres, MsSQL} × `BRIGHTER_TRANSPORT` ∈ {RabbitMQ, Kafka} — and its README instructs the reader to run the Sweeper in a second terminal after hand-editing an absolute database path into `appsettings.*.json`. That is four forks and a manual step before the first message moves. |
| `samples/TaskQueue/KafkaTaskQueue` | Sender with `TimedMessageGenerator`, ReceiverConsole. Topic created with `NumPartitions = 3`; subscription uses `groupId: "kafka-GreetingsReceiverConsole-Sample"`, `numOfPartitions: 3`, `MessagePumpType.Proactor`. | **Close.** Already has the multi-partition topic and consumer group the tutorial needs. Sender carries a full Polly `PolicyRegistry` — an unnecessary fork. |
| `docker-compose-kafka.yaml` | Single-broker KRaft (`apache/kafka:4.0.2`) plus Schema Registry and Control Center. | **Sufficient.** See resolved question 2 below. |

**Net:** the raw material for all four rungs exists and runs, but no existing sample
is a tutorial, and the one covering the durable-outbox story is the least
tutorial-shaped of them all.

## Target State

A developer who has never used Brighter can, in one sitting:

1. Install the package, define a command, write a handler, send it, and see output —
   with no broker and no Docker. **Under 10 minutes.**
2. Put a message over RabbitMQ and watch a separate process consume it.
3. Replace the in-memory Outbox with Postgres, see the transactional guarantee hold,
   and watch the Sweeper dispatch.
4. Stream over Kafka, start a second consumer instance, and see partitions reassign
   while per-partition ordering holds.

At every rung: the reader runs code, sees the result the page said they would see,
and is handed a short "what you built / what to read next" list pointing at
explanation and reference pages. Concepts met along the way (publication,
subscription, routing key, Outbox, consumer group) are **named and linked out**, never
explained inline.

## Target Audience

- **Primary — beginners to Brighter.** Competent .NET developers, comfortable with
  `dotnet` CLI and Docker Desktop, who have never used Brighter and may never have
  used a message broker. Assume no knowledge of message-oriented middleware; define
  or link every term on first use.
- **Secondary — evaluators.** Developers deciding whether Brighter fits, who want to
  see a working message flow before reading 1,068 lines of configuration reference.
  Rung 4 is aimed squarely here: it makes a real architectural differentiator visible.
- **Not the audience — existing users.** Someone tuning a subscription or choosing an
  Outbox backend wants reference or a how-to, not a tutorial. Tutorials link out to
  those; they do not compete with them.

## Source Material

**Documentation to link out to (not duplicate):**

- `contents/BrighterBasicConfiguration.md` (1,068) — the reference these tutorials
  deliberately do not restate
- `contents/BrighterOutboxSupport.md` (514) — Outbox and Sweeper behaviour, rung 3
- `contents/BoxProvisioning.md` and `contents/BoxProvisioningConfiguration.md` — how
  rung 3's Outbox table gets created, and the Option A / Option B choice the tutorial
  takes silently and links out to
- `contents/RabbitMQConfiguration.md` (565) — rung 2
- `contents/KafkaConfiguration.md` (606) — rung 4
- `contents/ReactorAndProactor.md` (440) — the pump explanation rung 4 links to
- `contents/Glossary.md` — term definitions

**Code:**

- `../Brighter/samples/` — per the survey table above
- `../Brighter/docker-compose-rmq.yaml`, `docker-compose-kafka.yaml`,
  `docker-compose-postgres.yaml`
- `../Brighter/release_notes.md` — current release **10.7.0**; also the source for
  the Box provisioning/migration behaviour rung 3 depends on
- `../Brighter/src/Paramore.Brighter.ServiceActivator/` — for the pump claims in
  rung 4 (see Verification below)

## Resolved Questions

**Q1 — package versions: pin, verified per release.** *(decided 2026-08-03; mechanism
corrected at review after inspecting the sample project files)*

Tutorial **prose** states exact versions — `dotnet add package Paramore.Brighter
--version 10.7.0` — because a reader's first command must be reproducible.

The samples cannot carry that pin, and it matters that we say so rather than assume
otherwise. Brighter's samples reference the library by **`ProjectReference` into
`../../../src/`**, not `PackageReference`, and their third-party `PackageReference`
entries carry no `Version` attribute (central package management). See
`samples/CommandProcessor/HelloWorld/HelloWorld.csproj`. Tutorial samples follow that
convention like every other sample — deviating would make Brighter's own CI build
against its released binaries instead of its working tree, which is the wrong trade
for the source repository.

The consequence is that **no single mechanism proves a tutorial works.** Three
partial guarantees compose into one:

| Guarantee | Proves | Does not prove |
|---|---|---|
| Brighter CI builds the sample (`ProjectReference`) | The code shape compiles against current source | That it compiles against the **released package** the reader installs |
| `tools/versioncheck.py` in the Docs repo (D9) | Every version pinned in tutorial prose matches the current release | That the code works |
| Clean-machine end-to-end run before ship | The combination actually works | Nothing after the run — it is a point-in-time check |

The gap this leaves is real and worth naming: between a release and the next
end-to-end run, a source change that alters a public API will keep CI green while
breaking the reader's copy-paste. `versioncheck` narrows the window by failing the
build as soon as prose versions fall behind the release; the end-to-end run closes
it. Neither alone is sufficient, and the release checklist is what schedules the run.

**Consequence for acceptance criterion 2:** code blocks match the sample's `.cs`
files. They deliberately **do not** match its `.csproj` — the tutorial shows
`PackageReference` because that is what a reader creating a fresh project will have.
The project file is the one place the page and the sample legitimately differ, and
each page should say so in a single line rather than leaving a reader to trip over it.

**Q2 — Kafka needs a single broker, not a multi-broker compose.** *(resolved by
survey, 2026-08-03)* Consumer-group rebalancing is a function of **partitions and
consumer instances**, not brokers. `docker-compose-kafka.yaml` is already a
single-broker KRaft setup, and `KafkaTaskQueue` already creates a 3-partition topic.
The reader demonstrates rebalancing by starting a *second copy of the consumer app*,
which is both simpler to run and a truer picture of how services scale out.

**Q3 — rung 3 gets a purpose-built minimal sample.** *(decided 2026-08-03)* See
D6 below. `WebAPI_Dapper` is reused as a *link target* for readers who want the
fuller picture, not as the tutorial's code.

**Q4 — rung 4 runs the Kafka consumer as a Reactor.** *(decided 2026-08-03,
verified against source)* The existing `KafkaTaskQueue` sample uses
`MessagePumpType.Proactor`, which sits awkwardly with the rung's whole point. Reactor
is supported and is in fact the default: `KafkaMessageConsumer` implements
`IAmAMessageConsumerSync` (`../Brighter/src/Paramore.Brighter.MessagingGateway.Kafka/KafkaMessageConsumer.cs:47`),
`KafkaMessageConsumerFactory.Create` returns it as such (`KafkaMessageConsumerFactory.cs:65`),
and `KafkaSubscription`'s constructor already defaults `messagePumpType` to
`MessagePumpType.Reactor` (`KafkaSubscription.cs:298`).

Running Reactor means the reader sees a single-threaded pump per partition
assignment, which is precisely the architectural claim the tutorial is making —
the code on screen demonstrates the point rather than requiring the prose to explain
around it.

**Consequence for D7:** a Reactor pump needs a **synchronous** handler and message
mapper. `KafkaTaskQueue` supplies `GreetingEventHandlerAsync` and
`GreetingEventMessageMapperAsync`, so the tutorial sample needs sync equivalents.
This widens the delta from the existing sample and strengthens the case for a
separate tutorial sample rather than an in-place edit.

**Q5 — rung 3 uses Box Provisioning.** *(decided 2026-08-03, verified against docs
and source)* Brighter creates the Outbox table itself at startup via
`services.AddBrighter().UseBoxProvisioning(opts => opts.AddPostgreSqlOutbox(config))`.
*(Method name corrected 2026-08-03 during design — it is `AddPostgreSqlOutbox`, not
`AddPostgresOutbox`: `PostgreSqlBoxProvisioningExtensions.cs:17`.)*
On a brand-new database this takes the *fresh install* path — the table is created at
the latest shape and one history row is stamped; no migrations are replayed
(`contents/BoxProvisioning.md` § The three paths).

This removes an entire class of step from the tutorial: no migration assemblies, no
generated DDL, no hand-run SQL, no second terminal. The reader points the app at an
empty Postgres container and starts it.

Two things the page must still say plainly:

- The database user needs `CREATE TABLE` and `ALTER TABLE` rights. True by default
  for a Docker Postgres container; not true in many real deployments.
- Box Provisioning is **Option A of two**. Regulated environments manage the DDL
  themselves via `*OutboxBuilder.GetDDL()`. The tutorial takes Option A without
  discussion — tutorial discipline forbids the fork — but its "what to read next"
  section links to `contents/BoxProvisioning.md#when-to-use-box-provisioning` so the
  reader discovers the choice at the point where they are equipped to make it.

Packages: `Paramore.Brighter.BoxProvisioning` plus
`Paramore.Brighter.BoxProvisioning.PostgreSql`, **in addition to** the Outbox package
itself.

## Scope

### P0 — must have

- **T1. Your First Command** — in-process only. Install, define `GreetingCommand`,
  write `GreetingCommandHandler`, wire `AddBrighter().AutoFromAssemblies()`, `Send`,
  see console output. No transport, no Docker. Target: 10 minutes.
- **T2. Your First Message Over a Broker** — RabbitMQ via supplied compose file.
  Producer `Post`s, Dispatcher consumes in a second process, default in-memory
  Outbox. Introduces publication, subscription and routing key *in passing*, each
  linked out. Target: 20 minutes.
- **T3. Adding a Durable Outbox** — swap the in-memory Outbox for Postgres, let
  `UseBoxProvisioning` create the table at startup, show the transactional guarantee,
  run the Sweeper. Target: 25 minutes.

  **The guarantee needs a business write to be a guarantee about anything.** An
  Outbox on its own only stores messages; what makes it worth having is that the
  message and the state change it announces commit or fail *together*. So rung 3
  introduces a domain table — a `Greeting` row written by the handler in the same
  transaction that deposits `GreetingMade` into the Outbox. The tutorial shows the
  reader both halves:

  1. **Happy path** — send a request, see a row in `Greeting` and a dispatched
     message in the Outbox table.
  2. **Failure path** — the handler throws after the write but before commit; the
     reader queries both tables and finds *neither* the row nor the message. This is
     the step that makes the concept land, and it is the one a showcase cannot do.

  The domain table is created by the tutorial's own startup code, not by
  `UseBoxProvisioning` — which owns the Outbox table only. The page must not blur
  these, or a reader will assume Brighter manages their schema.
- **Sample code** for T1–T3, compiled by Brighter's CI (D4–D6).
- **`SUMMARY.md`** gains a *Get Started* section, placed **above** *Overview*.
- **`tools/versioncheck.py`** (D9) and the release-run checklist entry (D11) — the two
  halves of making the version pin hold.
- Every code block complete and compilable, `using` directives included — measured
  against the audit finding that only ~230 of ~1,050 existing C# blocks carry them.
- **Each tutorial executed end to end, from a clean clone and empty NuGet cache,
  before it ships.**

### P1 — should have

- **T4. Streaming with Kafka** — multi-partition topic, messages flowing, a second
  consumer instance joining the group, partitions reassigning, per-partition ordering
  holding, offset-commit behaviour made visible. Runs the pump as a **Reactor** (Q4),
  so the single-threaded-per-channel property the page is about is visible in the
  code the reader runs; the deeper treatment is left to `ReactorAndProactor.md`.
  P1 rather than P0 because its infrastructure cost is the highest of the four and
  two Kafka details still need confirming (partition key, offset-commit timing).
  Ships in the same batch if those hold.
- **Sample code** for T4 (D7).
- **A *Get Started* landing page** that states the ladder, prerequisites and expected
  time per rung, so a reader can see the whole path before starting it.

### P2 — nice to have

- A `docker-compose.tutorial.yml` in the Docs repo bundling only what the tutorials
  need, if the per-technology compose files at the Brighter root prove awkward to
  reference from a Docs page.
- Screenshots of the RabbitMQ management UI and Kafka Control Center at the point
  where the reader should check their result.

## Out of Scope

- **Rewriting `ShowMeTheCode.md`.** It stays a showcase; Spec 010 re-files it.
- **Darker tutorials.** Worth doing; land the Brighter ladder first.
- **Any change to `../Brighter` outside `samples/`.** `src/`, `tests/`, ADRs and
  release notes remain strictly read-only.
- **Transports other than RabbitMQ and Kafka.** A tutorial per transport is a
  reference table in disguise; the how-to guides of Spec 013 cover the rest.
- **Explaining the concepts the tutorials touch.** Publication, subscription, routing
  key, Outbox, consumer group are named and linked, never explained inline. If a
  target page explains one badly, that is a Spec 010/013 defect, not a licence to
  duplicate here.

## Documentation Deliverables

### Docs repository

| ID | File | Description | Priority |
|---|---|---|---|
| D1 | `contents/TutorialFirstCommand.md` | Rung 1. In-process command and handler. | P0 |
| D2 | `contents/TutorialFirstMessage.md` | Rung 2. RabbitMQ, producer and consumer processes. | P0 |
| D3 | `contents/TutorialDurableOutbox.md` | Rung 3. Postgres Outbox, transactional guarantee, Sweeper. | P0 |
| D8 | `contents/TutorialStreamingWithKafka.md` | Rung 4. Partitions, consumer group, ordering, offsets. | P1 |
| D10 | `contents/GetStarted.md` | Landing page: the ladder, prerequisites, time per rung. | P1 |
| D12 | `contents/Glossary.md` (edit) | *Added at design review 2026-08-03.* Five terms the tutorials link but that do not exist: `at-least-once`, `Box Provisioning`, `partition`, `consumer group`, `offset`. Absent — not merely badly explained — so AC8 fails without them. | P0 |

File names are provisional and coordinate with Spec 010, which owns the final
information architecture. The `Tutorial` prefix is deliberate — it groups them in a
directory listing and signals mode at a glance.

### Brighter repository — via pull request only

Per the sanctioned exception in `CLAUDE.md` (approved 2026-08-03): additions under
`samples/` only, always by PR, never a direct commit.

| ID | Path | Approach | Priority |
|---|---|---|---|
| D4 | `samples/CommandProcessor/HelloWorld` | **Reuse as-is.** Already minimal and correct. Verify it runs; change nothing unless it fails. | P0 |
| D5 | `samples/Tutorials/02-FirstMessage` | **New**, derived from `RMQTaskQueue`: one event type, no Serilog, no `CustomPublicationFinder`, no explicit scheduler. | P0 |
| D6 | `samples/Tutorials/03-DurableOutbox` | **New.** One app, Postgres only, RabbitMQ only, no env-var matrix, Sweeper hosted in-process, Outbox table created by `UseBoxProvisioning`, plus a `Greeting` domain table for the transactional demo and a deliberate failure path. A single delta from D5. | P0 |
| D7 | `samples/Tutorials/04-Kafka` | **New**, derived from `KafkaTaskQueue`: `MessagePumpType.Reactor` with **sync** handler and mapper, drop the Polly `PolicyRegistry`, keep the 3-partition topic and consumer group. | P1 |

**On preferring new samples over extending existing ones.** `CLAUDE.md` sets the
order reuse → extend → write new, and D5/D7 are proposed as new. The justification
is that the extras in `RMQTaskQueue` and `KafkaTaskQueue` — the publication finder,
the policy registry, Serilog — are *why those samples exist*. Trimming them destroys
their teaching purpose; leaving them in destroys the tutorial's. A tutorial ladder
also needs each rung to be a readable diff of the one below it, which a family under
`samples/Tutorials/` gives and scattered reuse does not. **This reasoning is open to
challenge at design stage** — if a reviewer would rather extend in place, that
decision is cheap to take now and expensive to take later.

### Process

| ID | Deliverable | Description | Priority |
|---|---|---|---|
| D9 | `tools/versioncheck.py` | Scans tutorial pages for pinned `Paramore.Brighter*` versions, compares each against the latest release heading in `../Brighter/release_notes.md`, and exits non-zero on a mismatch. Same shape as `linkcheck.py`, and the same shape as Spec 012's planned `optioncheck` — a gate, not a habit. | P0 |
| D11 | Release checklist entry | A documented step requiring all shipped tutorials to be run end to end on a clean machine when Brighter releases. `versioncheck` catches stale *numbers*; only the run catches stale *code*. | P0 |

**Why D9 is a tool and not a checklist item.** The pinning decision only pays off if
something notices when the pin goes stale. A human checklist that fires once per
release, on the busiest day of the release, is exactly the mechanism that has already
let 0 of 110 pages acquire metadata. `versioncheck` fails the build instead. D11
remains human because no tool can run a tutorial for you — but it has a much smaller
job once the numbers are machine-checked.

## SUMMARY.md Changes

A new section at the **top** of the file, above `## Overview` — the on-ramp should be
the first thing in the navigation, not the fourth:

```markdown
## Get Started

 * [Get Started with Brighter](/contents/GetStarted.md)
 * [1. Your First Command](/contents/TutorialFirstCommand.md)
 * [2. Your First Message Over a Broker](/contents/TutorialFirstMessage.md)
 * [3. Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)
 * [4. Streaming with Kafka](/contents/TutorialStreamingWithKafka.md)

## Overview
…
```

Numbered display text is deliberate: the ladder only works if the order is visible in
the navigation. Placement coordinates with Spec 010, which restructures the whole
table of contents — 010 must not silently re-sort these.

**Each entry lands in the same commit as its page, never before.** The block above is
the finished state, but `GetStarted.md` and the Kafka page are P1: if either slips,
`SUMMARY.md` would point at a file that does not exist and `linkcheck.py` would fail
on MISSING FILE. Add the line and the page together.

**The two-on-ramps problem.** *Get Started* sits directly above *Overview*, whose
first entry is `ShowMeTheCode.md` — a showcase that also presents itself as a
starting point. A newcomer now sees two front doors. Spec 010 owns the eventual
structure; what this spec owns is the relationship, and it is one line on
`GetStarted.md`: the ladder is the path for building something, `ShowMeTheCode.md` is
the two-minute look at what Brighter code reads like. Send evaluators there, send
learners down the ladder.

Run `python3 tools/linkcheck.py` after every page added. The orphan check is what
enforces the no-orphans rule, and it only reports on a whole-repo run.

## Technical Accuracy — Verify Before Writing

Claims that cannot be asserted from the survey alone. Each must be confirmed against
`../Brighter/src/` before the corresponding page is written:

1. **Kafka partition key.** Establish how a message is assigned to a partition — what
   Brighter sends as the Kafka message key — before telling a reader ordering is
   preserved "for a given key".
2. **Offset-commit behaviour.** `commitBatchSize: 5` and
   `sweepUncommittedOffsetsInterval` in the existing sample imply batched commits.
   Confirm exactly when offsets are committed; this is where Kafka newcomers lose
   messages and the tutorial must be precise.
3. ~~**Partition assignment and pump instances.**~~ **Resolved at design review
   2026-08-03.** `noOfPerformers` defaults to **1** (`Subscription.cs:201`) and
   `Dispatcher.CreateConsumers` builds one `Consumer` — one channel, one consumer, one
   pump — per performer (`Dispatcher.cs:589`). So it is one pump per **group member**,
   *not* one pump per partition: at the default, a single process is assigned all three
   partitions and its one thread drains them sequentially. Full statement in
   `design.md` § Partition assignment and pump instances.
4. ~~**`AddPostgresOutbox` signature.**~~ **Resolved during design 2026-08-03.** The
   method is **`AddPostgreSqlOutbox`**, with two overloads —
   `(IAmARelationalDatabaseConfiguration)` and `(string connectionName)` —
   `PostgreSqlBoxProvisioningExtensions.cs:17,48`.
5. **Transaction provider for rung 3.** Confirm how the handler enlists its `Greeting`
   write and `DepositPost` in one transaction under Postgres — which transaction
   provider is registered and what the handler's call sequence is. The whole rung
   rests on this.
6. **Pinned versions.** Confirm the exact published version for each
   `Paramore.Brighter.*` package the tutorials name, at the time of writing.

Two earlier items are now closed: per-partition ordering under Proactor (resolved by
Q4 — the tutorial runs Reactor) and whether the Outbox table needs manual DDL
(resolved by Q5 — Box Provisioning creates it).

## Constraints

**Tutorial discipline (the constraint that makes these pages different):**

- **No branching.** One path. No "if you prefer X…", no option tours, no alternative
  transports mid-page. Choices belong in how-to guides and reference.
- **Every step produces observable output.** The reader must be able to confirm they
  are on track without reading ahead.
- **State the expected result explicitly** — the console line, the queue count, the
  row in the table — so a reader knows immediately when they have diverged.
- **Prerequisites stated up front**: the **.NET 9 SDK** (every sample surveyed targets
  `net9.0`), Docker Desktop for rungs 2–4, and the ports each compose file binds —
  5672/15672 for RabbitMQ, 9092 for Kafka, 5432 for Postgres. A reader with something
  already on 5432 needs to know that before step 1, not at the failure.
- **End with "what you built / what to read next"**, linking to explanation and
  reference — the hand-off out of tutorial mode.

**From `CLAUDE.md`:**

- Second person, active voice, present tense.
- V10 patterns only.
- Prefer "Dispatcher" over "ServiceActivator" in prose. **Note the collision:** the
  namespaces are genuinely `Paramore.Brighter.ServiceActivator.Extensions.*` and the
  hosted service is genuinely `ServiceActivatorHostedService`. Rung 2 must show the
  real names and add one sentence explaining that the older name survives in the API
  surface — a beginner who sees the mismatch unexplained assumes they are on the
  wrong page.
- Every C# block gets `csharp` highlighting and complete `using` directives.
- Link concepts on first mention; never duplicate an authoritative page.
- Reference working samples explicitly.

**Coordination with other specs:**

- **Spec 011** adds a page banner to every page. These pages must carry it once its
  format is fixed: `> **Tutorial** · Applies to **Brighter V10** · Prerequisites: …`.
  If 011 lands after these pages, they are in the sweep like everything else.
- **Spec 010** owns the final file locations and the table of contents. Names here
  are provisional; the *Get Started*-first placement is not.
- **Spec 013** owns how-to guides. Anything that starts to read like "how to
  configure X for Y" belongs there, not here.

## Acceptance Criteria

A tutorial is done when:

1. It has been **executed end to end on a clean machine** — fresh clone, empty NuGet
   cache, no pre-existing Docker volumes — and reached the stated result, **following
   the page's own `dotnet add package` instructions rather than the sample's project
   references**. This is the only check that exercises what the reader actually does.
2. The run was **timed**, and the page's stated duration is within sight of it. The
   10/20/25-minute figures are currently estimates; publishing an estimate a reader
   overshoots by double is a small betrayal of the tutorial's promise. Adjust the page
   to the measurement, not the measurement to the page.
3. Every C# block in the page compiles and matches the corresponding `.cs` file in the
   companion sample. The `.csproj` is the documented exception (see Q1).
4. The companion sample builds in Brighter's CI, and its PR is merged **before** the
   Docs page ships — a page pointing at an unmerged sample is a broken link with extra
   steps.
5. `python3 tools/linkcheck.py` is clean across the repo, including the orphan check.
6. `python3 tools/versioncheck.py` passes.
7. The page states prerequisites, pinned versions, expected output at each step, and
   a "what to read next" hand-off.
8. A reader can complete it without visiting any page other than those the tutorial
   links to.

## Notes

- **The failure mode this spec exists to prevent is a tutorial that breaks at step 4.**
  That is worse than no tutorial: it converts a curious evaluator into a public
  critic. Verification is not a formality here — it is the deliverable.
- `#67` stays open until this work lands, and the issue author has been told the
  ladder is coming. Check the thread for a reply before finalising the shape.
</content>
