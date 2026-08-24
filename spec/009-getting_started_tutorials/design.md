# Design: Spec 009 — Getting Started Tutorials

**Created:** 2026-08-03
**Status:** Approved 2026-08-03 (reviewed; six findings addressed in this revision)
**Requirements:** [`requirements.md`](requirements.md) (approved 2026-08-03)

## Documentation Structure

```
Docs/
├── SUMMARY.md                                EDIT  new "Get Started" section, first
├── contents/
│   ├── GetStarted.md                         NEW   D10  landing page, the ladder
│   ├── TutorialFirstCommand.md               NEW   D1   rung 1
│   ├── TutorialFirstMessage.md               NEW   D2   rung 2
│   ├── TutorialDurableOutbox.md              NEW   D3   rung 3
│   ├── TutorialStreamingWithKafka.md         NEW   D8   rung 4
│   └── Glossary.md                           EDIT  D12  five missing terms
├── tools/
│   └── versioncheck.py                       NEW   D9   pinned-version gate
├── .github/workflows/docs.yml                EDIT  D9   wire the gate in (Spec 011 owns the file)
└── RELEASE_CHECKLIST.md                      NEW   D11  the run-them-again step

Brighter/  (separate repo — pull request only)
└── samples/
    ├── CommandProcessor/HelloWorld/          REUSE D4   rung 1, unchanged
    └── Tutorials/
        ├── 02-FirstMessage/                  NEW   D5   rung 2
        ├── 03-DurableOutbox/                 NEW   D6   rung 3
        └── 04-Kafka/                         NEW   D7   rung 4
```

### Reading order

The ladder is the design. Each rung is **one concept further** than the one below it,
and each sample is a readable diff of the previous sample:

```
GetStarted.md  ─ pick your starting point, check prerequisites
      │
      ▼
1. First Command      in-process. No broker, no Docker.            10 min
      │                   + a command, a handler, Send()
      ▼
2. First Message      + RabbitMQ, two processes, in-memory Outbox  20 min
      │                   + publication, subscription, routing key
      ▼
3. Durable Outbox     + Postgres, a business table, the Sweeper    25 min
      │                   + the transactional guarantee
      ▼
4. Streaming w/ Kafka + partitions, consumer group, offsets        30 min
                          + ordering, rebalancing
```

Rung 4 branches from rung 2 conceptually — it swaps the transport — but **its stated
prerequisites are rungs 2 and 3, both**, because a reader who has not met the Outbox
will ask where it went. Its banner, its *Before You Start* and this diagram all say the
same two rungs; an earlier draft named rung 2 in the banner and both in the prose, which
is the kind of small inconsistency a reader reads as a mistake in the ladder itself.
This costs nothing in sequencing: rung 3 is P0 and rung 4 is P1, so 3 ships first
regardless.

## Verification Items — Resolved

Requirements listed six items to confirm before writing. **All six are now resolved
against source** (item 3 closed at design review); all evidence is cited so the writing
phase does not repeat the work.

| # | Item | Resolution |
|---|---|---|
| 1 | Kafka partition key | `KafkaMessagePublisher.cs:79` sets `Key = message.Header.PartitionKey.Value`. Kafka's partition assignment therefore follows Brighter's `PartitionKey` header — so ordering is *per key*, and the tutorial must set it explicitly to demonstrate ordering honestly. |
| 2 | Offset-commit behaviour | Batched. `commitBatchSize` defaults to **10** (`KafkaMessageConsumer.cs:122`); offsets are stored then committed when the store reaches the batch size (`:319`, `:855`). Critically, offsets **are** committed for revoked partitions on rebalance (`:245`, `:895`) — so the rebalance demo does not silently lose position. On a crash, up to `commitBatchSize` messages are redelivered; that is the at-least-once point rung 4 must make. |
| 3 | Partition assignment vs pump instances | **Resolved at design review** — see below. One pump per *performer*, not per partition; `noOfPerformers` defaults to 1. |
| 4 | Provisioning method name | **`AddPostgreSqlOutbox`**, not `AddPostgresOutbox` as requirements stated. Two overloads: `(IAmARelationalDatabaseConfiguration)` and `(string connectionName)` — `PostgreSqlBoxProvisioningExtensions.cs:17,48`. Requirements corrected. |
| 5 | Rung 3 transaction shape | Resolved — see below. |
| 6 | Pinned versions | At writing time. Current release **10.7.0**. |

### Partition assignment and pump instances (item 3)

**One pump per performer — not one pump per partition.** Getting this backwards would
have put a false sentence in rung 4, so state it precisely:

- `noOfPerformers` defaults to **1** (`src/Paramore.Brighter/Subscription.cs:201`).
- `Dispatcher.CreateConsumers` loops `NoOfPerformers` times per subscription and builds
  one `Consumer` each (`src/Paramore.Brighter.ServiceActivator/Dispatcher.cs:589`).
  Each `Consumer` owns one channel, hence one `KafkaMessageConsumer`, hence one message
  pump — and therefore one **member of the consumer group**.
- So at the default, one running process is one group member. Kafka assigns it *all
  three* partitions, and its single thread drains them one message at a time.

**The architecture sentence rung 4 may make:** per-partition ordering holds because a
single-threaded pump processes that instance's whole assignment sequentially — ordering
across partitions is not preserved, and by extension ordering holds *per key* only
because the key selects the partition (item 1). This confirms the outline below: step 5
("one consumer takes all three partitions") and step 6 (a second instance triggers a
rebalance) are correct as written.

**Correction to carry outward:** earlier working notes described the pump as mapping
"one-to-one onto consumer-group partition assignment". That overstates it. The mapping
is pump ↔ group member; the assignment is whatever Kafka gives that member.

Raising `noOfPerformers` to 3 would put three members in the group from one process and
Kafka would give each a single partition. That is a real and interesting experiment, but
it is a fork — keep it out of the tutorial body; `ReactorAndProactor.md` is where it
belongs.

### The rung 3 transaction shape (item 5)

Taken from `samples/WebAPI/WebAPI_Dapper/GreetingsApp/Handlers/AddGreetingHandlerAsync.cs`,
which is compiled by Brighter's CI and is the canonical pattern:

```csharp
DbConnection conn = await _transactionProvider.GetConnectionAsync(cancellationToken);
DbTransaction tx  = await _transactionProvider.GetTransactionAsync(cancellationToken);
try
{
    // 1. the business write, on the same connection and transaction
    await conn.ExecuteAsync("insert into Greeting (Message) values (@Message)",
        new { greeting.Message }, tx);

    // 2. the message, deposited into the Outbox inside that same transaction
    posts.Add(await _postBox.DepositPostAsync(
        new GreetingMade(greeting.Greet()), _transactionProvider,
        cancellationToken: cancellationToken));

    // 3. both, or neither
    await _transactionProvider.CommitAsync(cancellationToken);
}
catch (Exception)
{
    await _transactionProvider.RollbackAsync(cancellationToken);
    throw;
}
finally
{
    _transactionProvider.Close();
}
```

Registration: `new PostgreSqlOutbox(configuration)` with `PostgreSqlConnectionProvider`
and `PostgreSqlTransactionProvider` (`samples/WebAPI/WebAPI_Common/DbMaker/OutboxFactory.cs:75`).

**Design decision — rung 3 does not call `ClearOutboxAsync`.** The WebAPI sample calls
it immediately after commit, dispatching without waiting for the Sweeper, and its own
comment notes the trade-off. For a tutorial *about* the durable Outbox, omitting it is
the better choice: the reader watches the row sit in the Outbox table and then get
dispatched by the Sweeper a moment later. That delay is the feature being taught. The
page names the alternative in one sentence and links out.

---

## File-by-File Outline

### D10 — `contents/GetStarted.md`

**Purpose:** The front door: what the ladder is, what you need, and where to start.
**Length:** ~90 lines. **Type:** Tutorial (banner per Spec 011).

```
# Get Started with Brighter
> **Tutorial** · Applies to **Brighter V10**

  (intro — 3 sentences: what you will have built by the end of the ladder)

## The Ladder                     table: rung | what you add | time | needs Docker
## What You Need Installed        .NET 9 SDK; Docker Desktop for rungs 2–4; ports
## Just Want to See the Code?     ShowMeTheCode.md — the two-minute look
## Where to Go After the Ladder   how-to guides, reference, explanation
```

**Cross-links:** all four tutorials; `ShowMeTheCode.md`; `BrighterBasicConfiguration.md`;
`Glossary.md`.

**The two-front-doors sentence** (from requirements § SUMMARY.md Changes) lives in
*Just Want to See the Code?*: the ladder is for building something, `ShowMeTheCode.md`
is for seeing what Brighter code reads like. Evaluators go there; learners come here.

---

### D1 — `contents/TutorialFirstCommand.md`

**Purpose:** Get a command dispatched to a handler in-process, in ten minutes, with no
broker and no Docker.
**Length:** ~180 lines. **Sample:** `samples/CommandProcessor/HelloWorld` (reused as-is).

```
# Your First Command
> **Tutorial** · Applies to **Brighter V10**

## What You'll Build          one console app; a command; a handler; console output
## Before You Start           .NET 9 SDK. No Docker. ~10 minutes.
## Step 1: Create the Project     dotnet new console; three dotnet add package lines
## Step 2: Define a Command       GreetingCommand.cs
## Step 3: Write a Handler        GreetingCommandHandler.cs
## Step 4: Wire Up Brighter       Program.cs — AddBrighter().AutoFromAssemblies()
## Step 5: Run It                 dotnet run + the exact expected output
## What You Built                 recap; how AutoFromAssemblies found the handler
## Further Reading                links out
```

**Code examples**

| # | Example | Source | Complete? |
|---|---|---|---|
| 1.1 | `dotnet new` + three `dotnet add package --version 10.7.0` | Written | Complete |
| 1.2 | `GreetingCommand` | `HelloWorld/GreetingCommand.cs` | Complete, with `using` |
| 1.3 | `GreetingCommandHandler` | `HelloWorld/GreetingCommandHandler.cs` | Complete |
| 1.4 | `Program.cs` | `HelloWorld/Program.cs` | Complete |
| 1.5 | Expected console output | Run it | Verbatim |

**Glossary terms** — link on first mention, do not define inline:
[command](/contents/Glossary.md#command), [handler](/contents/Glossary.md#handler),
[Command Processor](/contents/Glossary.md#command-processor). All three exist today.

**Note for the writer:** the sample's `Program.cs` calls `host.Run()` after `Send`,
so the app does not exit. For a tutorial, that is a confusing final step — the reader
expects a prompt back. Either drop `host.Run()` in the page's version (and explain
why the sample keeps it), or tell the reader to press Ctrl+C. **Decide at writing time
by running it**; do not guess.

---

### D2 — `contents/TutorialFirstMessage.md`

**Purpose:** Send a message over RabbitMQ from one process and consume it in another.
**Length:** ~260 lines. **Sample:** `samples/Tutorials/02-FirstMessage` (new, D5).

```
# Your First Message Over a Broker
> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Command](/contents/TutorialFirstCommand.md)

## What You'll Build          two console apps + RabbitMQ in Docker
## Before You Start           Docker Desktop; ports 5672 and 15672; rung 1 complete
## Step 1: Start RabbitMQ     docker-compose.yml (inlined — see note) + docker compose up -d
## Step 2: Define the Event   GreetingEvent
## Step 3: Build the Producer AddProducers, the publication, the routing key
## Step 4: Build the Consumer the subscription, AddConsumers, the hosted service
## Step 5: Run Both           expected output in each terminal
## Step 6: See It in RabbitMQ localhost:15672 — the exchange and queue that appeared
## What You Built             producer/consumer, and what each new term meant
## Further Reading
```

**The compose file is inlined in the page, not referenced.** `docker-compose-rmq.yaml`
lives in the *Brighter* repository root, and a reader following this tutorial has
created their own project and never cloned Brighter. Telling them to fetch a file from
a repo they do not have is exactly the kind of fork that breaks a tutorial. The page
carries a minimal compose file (~12 lines, `rabbitmq:management`, the two ports); the
sample in `samples/Tutorials/02-FirstMessage/` keeps using the Brighter root file.

**Code examples**

| # | Example | Source | Complete? |
|---|---|---|---|
| 2.1 | `docker-compose.yml` | Derived from `docker-compose-rmq.yaml`, trimmed | Complete |
| 2.2 | `GreetingEvent` | `RMQTaskQueue/Greetings/Ports/Commands/GreetingEvent.cs` | Complete |
| 2.3 | Producer `Program.cs` | Derived from `RMQTaskQueue/GreetingsSender` **minus** Serilog, `CustomPublicationFinder`, the second event, the explicit scheduler | Complete |
| 2.4 | Consumer `Program.cs` | Derived from `RMQTaskQueue/GreetingsReceiverConsole`, one subscription | Complete |
| 2.5 | Handler | `GreetingEventHandler.cs` | Complete |
| 2.6 | Two terminals' expected output | Run it | Verbatim |

**Glossary terms:** [event](/contents/Glossary.md#event),
[publication](/contents/Glossary.md#publication),
[subscription](/contents/Glossary.md#subscription),
[routing key](/contents/Glossary.md#routing-key),
[Dispatcher](/contents/Glossary.md#dispatcher). All five exist today — but `Dispatcher`
is defined **twice** in `Glossary.md` (`:95` and `:393`), one of the duplicates Spec 011
step 6 merges. Take whichever anchor survives that merge; do not link before it lands.

**Unblocked 2026-08-24 — see `tasks.md` §2.2.** That merge landed: there is one
`### Dispatcher`, at `:119`, and rung 2 may link `#dispatcher` directly. The instruction
above is the only one in this document that tells a writer *not* to do something now safe,
which is why it takes a pointer even though §2.1 and §2.2 name only two.

**The `ServiceActivator` collision.** The consumer needs
`using Paramore.Brighter.ServiceActivator.Extensions.Hosting;` and registers
`ServiceActivatorHostedService`. The prose says *Dispatcher* throughout, per
`CLAUDE.md`. One sentence, placed at the `using` block, explains that the older name
survives in the API surface — a beginner who meets the mismatch unexplained concludes
they are reading the wrong page.

---

### D3 — `contents/TutorialDurableOutbox.md`

**Purpose:** Replace the in-memory Outbox with Postgres and see the transactional
guarantee hold — and, more importantly, see it hold when things fail.
**Length:** ~300 lines. **Sample:** `samples/Tutorials/03-DurableOutbox` (new, D6).

```
# Adding a Durable Outbox
> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Message Over a Broker](/contents/TutorialFirstMessage.md)

## What You'll Build            rung 2 + Postgres + a business table + the Sweeper
## Before You Start             Docker; port 5432; rung 2 complete
## Step 1: Start Postgres       compose file, inlined
## Step 2: Add the Packages     Outbox.PostgreSql, PostgreSql, BoxProvisioning ×2
## Step 3: Create the Greeting Table    your table, your code — see the note
## Step 4: Configure the Outbox         UseBoxProvisioning + AddPostgreSqlOutbox
## Step 5: Write and Deposit in One Transaction    the handler
## Step 6: Run It               the row, the Outbox row, then the Sweeper dispatching
## Step 7: Make It Fail         throw before commit; query both tables; find neither
## What You Built               what "or neither" actually bought you
## Further Reading
```

**Step 3 must not blur two things.** `UseBoxProvisioning` creates the **Outbox** table
and nothing else. The `Greeting` table is the tutorial's own, created by its own
startup code. A reader who conflates them will believe Brighter manages their schema.
The page states the split in one sentence at the top of step 3.

**Step 7 is the point of the page.** Steps 1–6 could be mistaken for configuration
trivia; step 7 is where the concept lands, because the reader queries two tables and
finds nothing in either. Give it the same weight as the happy path — it is not an
appendix.

**Code examples**

| # | Example | Source | Complete? |
|---|---|---|---|
| 3.1 | Postgres `docker-compose.yml` | Derived from `docker-compose-postgres.yaml` | Complete |
| 3.2 | Four `dotnet add package` lines | Written; names verified | Complete |
| 3.3 | `Greeting` table DDL + startup creation | Written | Complete |
| 3.4 | Outbox + `UseBoxProvisioning(opts => opts.AddPostgreSqlOutbox(cfg))` | `PostgreSqlBoxProvisioningExtensions.cs:17`; `OutboxFactory.cs:75` | Complete |
| 3.5 | Transactional handler | `AddGreetingHandlerAsync.cs`, simplified — no Polly attributes, no `ClearOutboxAsync` | Complete |
| 3.6 | Sweeper registration | Hosted in-process | Complete |
| 3.7 | Two `SELECT` statements for step 6, repeated in step 7 | Written | Complete |
| 3.8 | The deliberate `throw` | Written | Complete, with a comment marking it as deliberate |

**Glossary terms:** [Outbox](/contents/Glossary.md#outbox),
[Sweeper](/contents/Glossary.md#sweeper),
[at-least-once](/contents/Glossary.md#at-least-once),
[Box Provisioning](/contents/Glossary.md#box-provisioning). The first two exist; the last
two **do not** — D12 adds them.

**Cross-links out:** `BrighterOutboxSupport.md`, and
`BoxProvisioning.md#when-to-use-box-provisioning` — the Option A / Option B choice the
tutorial takes silently, surfaced in *Further Reading* where a reader in a regulated
environment will find it.

---

### D8 — `contents/TutorialStreamingWithKafka.md` (P1)

**Purpose:** Show partitions, a consumer group rebalancing, per-key ordering and
offset commits — and connect ordering to Brighter's single-threaded pump.
**Length:** ~320 lines. **Sample:** `samples/Tutorials/04-Kafka` (new, D7).

```
# Streaming with Kafka
> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Message Over a Broker](/contents/TutorialFirstMessage.md), [Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)

## What You'll Build         3-partition topic; two consumer instances; ordering
## Before You Start          Docker; port 9092; rungs 2 and 3 complete
## Step 1: Start Kafka       single-broker KRaft compose, inlined
## Step 2: Create the Topic  NumPartitions = 3 on the publication
## Step 3: Send with a Partition Key    why the key decides ordering
## Step 4: Consume as a Reactor         sync handler and mapper; groupId
## Step 5: Watch One Consumer Take All Three Partitions
## Step 6: Start a Second Instance      the rebalance, live
## Step 7: Ordering Holds Per Key
## Step 8: Offsets and What a Crash Costs You
## What You Built            the pump connection, stated once, then linked out
## Further Reading           ReactorAndProactor.md, KafkaConfiguration.md
```

**Reactor, not Proactor** (requirements Q4). `KafkaMessageConsumer` implements
`IAmAMessageConsumerSync` (`:47`) and `KafkaSubscription` already defaults to
`MessagePumpType.Reactor` (`:298`). This obliges a **synchronous handler and mapper** —
the existing sample ships only the `…Async` pair, so D7 writes sync equivalents.

**Step 8 needs the numbers from verification item 2.** Offsets are batched at
`commitBatchSize` (default 10) and *are* committed for revoked partitions on rebalance,
so step 6 does not silently lose position. A crash, however, redelivers up to a batch.
That is the honest at-least-once statement, and it is where Kafka newcomers lose
messages.

**Code examples**: compose file; publication with `NumPartitions = 3`; producer setting
`PartitionKey`; `KafkaSubscription` with `groupId` and `MessagePumpType.Reactor`; sync
handler; sync mapper; the two terminals' rebalance output, captured verbatim.

**Glossary terms:** [partition](/contents/Glossary.md#partition),
[consumer group](/contents/Glossary.md#consumer-group),
[offset](/contents/Glossary.md#offset),
[partition key](/contents/Glossary.md#partition-key),
[Reactor](/contents/Glossary.md#reactor). Only the last two exist; `partition`,
`consumer group` and `offset` are added by D12.

---

### D12 — `contents/Glossary.md` (edit)

**Purpose:** Make good on the tutorials' central promise. The ladder names concepts and
links them out rather than explaining them inline — which only works if the target
entries exist. Five do not.

Added at design review, after checking every term the four outlines link:

| Term | Anchor | Status | Where it is first linked |
|---|---|---|---|
| `at-least-once` | `#at-least-once` | **Add** | D3, and again in D8 step 8 |
| `Box Provisioning` | `#box-provisioning` | **Add** — summarise, link to `BoxProvisioning.md` | D3 step 4 |
| `partition` | `#partition` | **Add** — distinct from the existing `Partition Key` (`:444`) | D8 step 2 |
| `consumer group` | `#consumer-group` | **Add** | D8 step 4 |
| `offset` | `#offset` | **Add** | D8 step 8 |

Confirmed present, link as-is: `#command` (`:13`), `#event` (`:21`),
`#command-processor` (`:63`), `#handler` (`:115`), `#outbox` (`:168`), `#sweeper`
(`:192`), `#subscription` (`:256`), `#publication` (`:262`), `#routing-key` (`:294`),
`#reactor` (`:328`), `#message-pump` (`:352`), `#partition-key` (`:444`).

**Re-measured 2026-08-23 — cite `tasks.md` §2.2's table, not the line numbers above.** The
verdicts hold: all five terms are still absent and all twelve anchors still resolve. The
line numbers do not — `Glossary.md` is 678 lines now and `#partition-key` is at `:533`.

**Why this is a 009 deliverable and not a 010/013 one.** Requirements § Out of Scope
rules that a target page *explaining a concept badly* is another spec's defect. A term
that is simply **absent** is different in kind: it breaks AC8 — "a reader can complete
it without visiting any page other than those the tutorial links to" — inside this
spec's own acceptance criteria. Five short entries are cheaper than a cross-spec
dependency, and the terms are ones the ladder is the first page in the repo to need.

**Anchored links, not bare ones.** Every glossary link in this spec carries its
`#anchor`. `Glossary.md` runs to 100 terms; a bare link asks a beginner to search a
page they have never seen for a word they have just met. Anchors also put the link
under `linkcheck.py`'s MISSING ANCHOR check, so the ordinary drift of heading edits
becomes a build failure instead of a silent dead end.

**Ordering constraint:** D12 lands **before or with** the page that first links each
term, or `linkcheck.py` fails on MISSING ANCHOR. Practically: land all five before D3.

---

### D9 — `tools/versioncheck.py`

**Purpose:** Fail the build when a version pinned in tutorial prose falls behind the
current Brighter release. Requirements § D9 is explicit about why this is a tool and
not a checklist line, so it has to be a gate that actually runs unattended.

**What it scans.** Every page named in `TUTORIAL_PAGES` (the five files above, listed
explicitly rather than globbed — a glob would silently start policing pages that
deliberately quote old versions). In each, every match of:

```
--version\s+(\d+\.\d+\.\d+)          # dotnet add package ... --version 10.7.0
Version="(\d+\.\d+\.\d+)"            # PackageReference in a .csproj block
```

restricted to lines that also mention `Paramore.Brighter`. Anything else is left alone.

**What it compares against — and the CI problem this solves.** Requirements assumed
`../Brighter/release_notes.md`. **That source cannot work in CI:** the Docs repo has no
sibling Brighter checkout, and Spec 011 is standing up the first workflow with only
this repository cloned. A gate that runs only on a maintainer's laptop is precisely the
mechanism requirements § D9 argues against.

So the authority is **NuGet**, queried once for the package the tutorials pin:

```
https://api.nuget.org/v3-flatcontainer/paramore.brighter/index.json
```

Take the highest non-prerelease version from `versions[]`. This is also the *truer*
source: the pin's job is to match what `dotnet add package` gives the reader, and NuGet
is what answers that command — `release_notes.md` is Brighter's record of it, one step
removed.

`--release-notes PATH` remains as an **offline fallback**, reading the latest release
heading from a local `release_notes.md`, so the tool still runs on a train. When both
are available the NuGet answer wins; a disagreement is reported, not silently resolved,
because it means a release shipped without notes or vice versa.

**Exit codes:** `0` clean · `1` at least one stale pin (prints file, line, found,
expected) · `2` could not determine the current version (network failure with no
`--release-notes`). **`2` must not be reported as a pass** — an unreachable authority is
an unchecked pin.

**CI wiring — a cross-spec dependency worth stating plainly.** Spec 011 creates
`.github/workflows/docs.yml`. D9 adds a step to *that* file; it does not create a second
workflow. The step runs on pull request **and** on a daily schedule, because the failure
this catches is caused by an event in another repository — a Brighter release — not by
a commit here. A PR-only trigger would leave a stale pin undetected until someone
happened to touch the docs.

**Same shape as `linkcheck.py`:** stdlib only, single file, paths optional, non-zero
exit. Spec 012's `optioncheck` is the third of the family.

---

### D11 — `RELEASE_CHECKLIST.md`

**Purpose:** Schedule the one check no tool can perform. `versioncheck` proves the
*numbers* are current; only running the tutorial proves the *code* still works. This is
the mechanism that closes the gap named in requirements § Q1 — the window in which a
source change keeps Brighter's CI green while breaking the reader's copy-paste.

**Length:** ~50 lines. **Type:** Reference (banner per Spec 011).

```
# Documentation Release Checklist
> **Reference** · Applies to **Brighter V10**

  (intro — when this runs: on every Brighter release, before the docs are
   re-published; who owns it)

## On Every Brighter Release
   1. run `python3 tools/versioncheck.py` — expect it to FAIL, that is the signal
   2. update the pinned versions it names
   3. run each shipped tutorial end to end on a clean machine (table below)
   4. re-time each; adjust the page if the stated duration has drifted
   5. run `python3 tools/linkcheck.py`
## The Clean-Machine Definition    fresh clone, empty NuGet cache, no Docker volumes
## The Tutorial Run Table          rung | page | sample | last run | last measured time
## When a Tutorial Fails           it is a release blocker for the docs, not a backlog item
```

**The run table carries a date column**, so "when was rung 3 last actually run against
a release?" has an answer in the repository rather than in someone's memory. A blank or
stale cell is the honest signal that a tutorial is unverified — which is the state
requirements § Notes calls worse than having no tutorial.

**Where it lives:** repository root, not `contents/`. It is a maintainer document, not
a published page — so it is deliberately **not** in `SUMMARY.md`, and `linkcheck.py`'s
orphan check does not apply to it (that check covers `contents/` only). Confirm this
when D11 lands: if the orphan check does object, the file moves rather than the check
being loosened.

---

## Sample Projects (Brighter repo, by pull request)

Each sample is **one delta** from the one below it. That is the pedagogic point, and it
is also the review argument: a reviewer can diff 03 against 02 and see exactly what a
durable Outbox costs.

| ID | Path | Derived from | Deltas |
|---|---|---|---|
| D4 | `samples/CommandProcessor/HelloWorld` | — | **None.** Verify it runs; change nothing unless it fails. |
| D5 | `samples/Tutorials/02-FirstMessage` | `RMQTaskQueue` | − Serilog, − `CustomPublicationFinder`, − `FarewellEvent`, − explicit scheduler |
| D6 | `samples/Tutorials/03-DurableOutbox` | D5 | + Postgres outbox, + `UseBoxProvisioning`, + `Greeting` table, + transactional handler, + Sweeper, + a failure switch |
| D7 | `samples/Tutorials/04-Kafka` | D5 | Transport → Kafka, + 3 partitions, + `PartitionKey`, + `groupId`, − Polly registry, async handler/mapper → **sync** |

**Convention:** samples reference Brighter by `ProjectReference` like every other
sample. The pages show `PackageReference` with a pinned version, because that is what a
reader creating a fresh project will have. This divergence is deliberate, documented in
requirements § Q1, and **each page states it in one line** so nobody trips over it.

---

## SUMMARY.md Changes

**Before** (first five lines of the file):

```markdown
## Overview

 * [Show me the code!](/contents/ShowMeTheCode.md)
 * [Basic Concepts](/contents/BasicConcepts.md)
 * [Why Brighter?](/contents/WhyBrighter.md)
```

**After:**

```markdown
## Get Started

 * [Get Started with Brighter](/contents/GetStarted.md)
 * [1. Your First Command](/contents/TutorialFirstCommand.md)
 * [2. Your First Message Over a Broker](/contents/TutorialFirstMessage.md)
 * [3. Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)
 * [4. Streaming with Kafka](/contents/TutorialStreamingWithKafka.md)

## Overview

 * [Show me the code!](/contents/ShowMeTheCode.md)
 * [Basic Concepts](/contents/BasicConcepts.md)
 * [Why Brighter?](/contents/WhyBrighter.md)
```

**Superseded 2026-08-24 — see `tasks.md` §2.1 for what the file looks like now.** Neither
block above describes `SUMMARY.md` today: Spec 010 replaced the nineteen-section tree, and
section 1 is *already* called `## Get Started`. The five entries join that section; no
section is created.

Numbered display text is deliberate — the ladder only works if its order is visible in
the navigation. **Each entry lands in the same commit as its page**: rung 4 and
`GetStarted.md` are P1, and a `SUMMARY.md` line pointing at a file that does not exist
fails `linkcheck.py` with MISSING FILE.

Placement coordinates with Spec 010, which restructures the whole table of contents.
The *Get Started*-first position is not up for renegotiation there.

---

## Code Examples Plan — Summary

| Rung | Blocks | From an existing sample | Written new |
|---|---|---|---|
| 1 | 5 | 4 (`HelloWorld`, verbatim) | 1 (CLI commands) |
| 2 | 6 | 3 (trimmed from `RMQTaskQueue`) | 3 (compose, CLI, output) |
| 3 | 8 | 2 (`AddGreetingHandlerAsync`, `OutboxFactory`) | 6 |
| 4 | 8 | 4 (from `KafkaTaskQueue`, sync-converted) | 4 |

Every block is complete and carries its `using` directives — the whole point, given
that only 16% of existing C# blocks do. Abbreviation with `// ...` is allowed only for
configuration a previous rung already showed in full, and then only with a link back to
that rung.

**Expected-output blocks are captured verbatim from a real run**, never hand-written.
A tutorial that predicts output the reader does not see is indistinguishable from a
broken tutorial.

**The licence header is elided — the one permitted departure from AC3.** Every `.cs`
file in `samples/` opens with a ~25-line MIT `#region Licence` block (see
`samples/CommandProcessor/HelloWorld/Program.cs:1`). Reproducing it would bury a
14-line `Program.cs` under a copyright notice and teach the reader nothing. Page blocks
therefore start at the first `using`. Read strictly, AC3 — "every C# block matches the
corresponding `.cs` file" — would be failed by every block in the spec on that
technicality, so it is worth naming here rather than discovering at acceptance: the
match is against the file **below its licence region**, exactly as the `.csproj`
divergence of § Q1 is the other documented exception. Nothing else is trimmed; if a
block would otherwise need cutting, that is a signal the sample is too big for the rung.

---

## Style Notes

- **No branching.** One path. Where a real choice exists (Sweeper versus
  `ClearOutbox`; Box Provisioning versus self-managed DDL), the tutorial takes one and
  names the other in a single sentence in *Further Reading*.
- **Terminology:** Dispatcher in prose, `ServiceActivator` where the API says so, with
  the one-sentence explanation at first collision (rung 2).
- **Deviation from the standard page pattern:** these pages use `## Step N: …` headings
  rather than the `CLAUDE.md` skeleton (Key Concepts / Configuration / Best Practices).
  Tutorials are a sequence, not a reference, and the skeleton would fight that. Spec
  011's heading rule is satisfied — the step headings are already subject-qualified and
  unique — but this deviation should be written into the conventions rather than left
  as an exception someone later "fixes".
- **Every step states its expected result** — console line, table row, or UI element —
  so a reader knows they have diverged the moment they do.
- **Time targets are measured, not asserted.** The 10/20/25/30-minute figures are
  estimates until the clean-machine run; the page is then adjusted to the measurement.

---

## Sequencing

| # | Step | Verified by |
|---|---|---|
| 1 | D12: add the five missing `Glossary.md` terms | `linkcheck.py` clean; anchors resolve |
| 2 | Build D5 sample; open the Brighter PR | Brighter CI green |
| 3 | Write D1 (reuses `HelloWorld`, no PR needed) and D2 | Clean-machine run, timed |
| 4 | Build D6 sample; PR | Brighter CI green |
| 5 | Write D3 | Clean-machine run, both paths incl. the failure |
| 6 | `tools/versioncheck.py`; wire it into 011's `docs.yml`; `RELEASE_CHECKLIST.md` | Passes against 10.7.0; the CI step runs and is green |
| 7 | Write D10 (`GetStarted.md`); add `SUMMARY.md` entries for what exists | `linkcheck.py` clean |
| 8 | Build D7 sample; PR; write D8 (Kafka, P1) | Brighter CI; clean-machine run |

Verification item 3 was step 1 in the draft; it is now resolved above, and D12 — which
has to precede D3 — takes the slot. Step 6 depends on Spec 011 having landed
`.github/workflows/docs.yml`; if 011 has not got there, write the tool and run it
locally, and leave the wiring as a follow-up rather than creating a competing workflow.

**Rungs 1–3 can ship without rung 4.** If Kafka slips, `GetStarted.md` and `SUMMARY.md`
list three rungs and the ladder still stands — which is why the numbering lives in the
display text and not in the file names.

### If a Brighter sample PR stalls

AC4 requires the companion sample's PR to be **merged** before its page ships. Steps 2,
4 and 8 therefore each hand control to review in a repository whose merge timing is not
ours, and all three P0 pages sit behind that. Worth deciding now rather than under
pressure:

- **D1 is not exposed.** It reuses `HelloWorld` unchanged and needs no PR, so rung 1 can
  ship on its own whatever happens to the rest.
- **D2/D3 wait.** Do not work around a stalled PR by inlining the sample into the page
  and dropping the `samples/` reference — that recreates exactly the rot the `CLAUDE.md`
  exception exists to prevent, and it fails AC4 rather than satisfying it.
- **What ships meanwhile** is whatever prefix of the ladder is complete. The `SUMMARY.md`
  rule already handles this: each entry lands in the same commit as its page, so a
  two-rung ladder is a coherent published state, not a broken one.
- **Escalate rather than wait silently.** These PRs exist to serve a public commitment on
  [#67](https://github.com/BrighterCommand/Docs/issues/67); if one sits beyond a couple
  of weeks, say so on the issue rather than letting the thread go quiet.

## Traceability

| Requirement | Design section |
|---|---|
| D1–D3, D8 tutorial pages | File-by-File Outline |
| D4–D7 samples | Sample Projects |
| D9 `versioncheck.py`, D11 checklist | D9 and D11 outlines; Sequencing step 6 |
| D10 `GetStarted.md` | File-by-File Outline |
| D12 `Glossary.md` additions *(added at design review)* | D12 outline; Sequencing step 1 |
| Q1 version pinning | Sample Projects § Convention |
| Q4 Reactor for Kafka | D8 outline |
| Q5 Box Provisioning | D3 outline, step 4 |
| Verification items 1–6 | Verification Items — Resolved (all six closed) |
| AC1, AC2 clean-machine run and timing | Sequencing; D11 run table |
| AC3 blocks match the sample | Code Examples Plan — two documented exceptions: `.csproj` (Q1) and the licence region |
| AC4 sample PR merged first | Sequencing § If a Brighter sample PR stalls |
| AC5 `linkcheck.py` clean | Sequencing steps 1 and 7 |
| AC6 `versioncheck.py` passes | D9 outline; Sequencing step 6 |
| AC7 prerequisites, pins, expected output, hand-off | Each page outline; Style Notes |
| AC8 completable without unlinked pages | D12 — the reason it is a 009 deliverable |
</content>
