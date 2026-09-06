# Design: Spec 013 — Task-Oriented How-To Guides

**Status:** **APPROVED 2026-09-06.** Q5, Q6 and Q7 answered in §11; Q3 answered by measurement; Q4 deferred to phase 2's PR
**Created:** 2026-09-05
**Requirements:** `spec/013-howto_guides/requirements.md` (APPROVED 2026-09-04, `af38910`, amended by #147/#148)

> Every figure in this document was derived on **2026-09-05** against Docs `master` at
> **`89195d8`**, Brighter at the **`10.7.0`** tag (`c1b8af8`) and Brighter
> **`origin/master`** (`cd50a43`). A total with no ref is not a fact — re-derive with the
> command given beside it. Where this document disagrees with the requirements, the
> disagreement is called out in §2 and the requirements are amended there, not silently.

---

## 1. What this design decides

1. **Four new pages and six edited ones**, with settled filenames — §4, §5.
2. **`SUMMARY.md` gains four nested entries and no section** — §6. The twelve-section tree
   and the top-level count of 12 do not move.
3. **§13 Q3 is answered by measurement, and its premise was stale** — the defects it asks
   about were repaired by 012's phase 10 at `05ab80c`. P0-3 links the table; it repairs
   nothing. §2.2.
4. **P0-2's scope is larger than the ten sites the requirements approved** — seventeen dead
   call sites and twenty defects in eight code blocks, because the blocks holding the ten are
   V9 throughout. §2.3, and Q5 in §11. **It is also six pages, not the five the requirements
   say three times** — §2.3.1.
5. **Two further dead-API families were found outside P0-2's scope**, on twenty sites
   across twelve pages — eleven of them pages P0-2 does not touch, and two of those eleven
   are P0-1's and P1-2's own prerequisite pages. §2.4. **Half of that became P0-4**, ruled
   2026-09-06 — Q6 in §11.
6. **Both composition guides pivot on one measured fact** — a single
   `RelationalDatabaseConfiguration` carries `queueStoreTable`, `outBoxTableName` and
   `inboxTableName` — so the composition is one configuration object, not three. §2.5.

---

## 2. What the design phase measured, and what it moves

The requirements phase measured *demand*. The design phase measured the *API the guides will
print*, which is where this programme's defects live. Six things moved.

### 2.1 The real resilience API, re-derived at both refs with controls

Requirements §3.2.1 is confirmed to the line. Re-derived per §11.6 — `git grep -w`, both
refs, with a control:

```bash
cd ../Brighter
for REF in 10.7.0 origin/master; do
  for T in Resilience DefaultResilience AddBrighterDefault ResiliencePipelines ConfigureResiliencePipelines; do
    echo "$REF $T $(git grep -w -l "$T" $REF -- src/ | wc -l)"
  done
done
```

| Name | Files in `src/` @ `10.7.0` | @ `origin/master` | State |
|---|---|---|---|
| `Resilience` | 6 | 6 | **live** |
| `DefaultResilience` | 2 | 2 | **live** |
| `AddBrighterDefault` | 5 | 5 | **live** |
| `ResiliencePipelines` | **0** | **0** | **dead or invented** |
| `ConfigureResiliencePipelines` | **0** | **0** | **dead or invented** |

The first three are the control: a sweep that finds them is a sweep that works, so the two
zeros are absence of the thing and not absence of a spelling.

**The declarations, for the writer:**

- `CommandProcessorBuilder.Resilience(ResiliencePipelineRegistry<string>, IPolicyRegistry<string>? = null)`
  — `CommandProcessorBuilder.cs:144`, interface at `:387`.
- `CommandProcessorBuilder.DefaultResilience()` — `:171`, interface at `:394`.
- `AddBrighterDefault(this ResiliencePipelineRegistry<string>)` —
  `Extensions/ResiliencePipelineRegistryExtensions.cs:57`.
- `BrighterOptions.ResiliencePipelineRegistry` is a **settable property**
  (`BrighterOptions.cs:59`). **There is no fluent DI method**, which is why the invented one
  was so easy to write.

**`.Policies(` is dead because the interface is gone.** `git grep -rn 'interface INeedPolicy'
10.7.0 -- src/` returns nothing, and the only `Policies` tokens in `CommandProcessorBuilder.cs`
are five doc-comment references, two of which are the dangling `cref`s raised as
[Brighter#4301](https://github.com/BrighterCommand/Brighter/issues/4301).

**The `??=` trap, confirmed with its enclosing methods** — the three sites are
`AddProducers` (two overloads, `:339`, `:463`) and the DI command-processor factory (`:705`).
At `:705` the assignment is followed two lines later by
`pollyBuilder.Resilience(options.ResiliencePipelineRegistry, options.PolicyRegistry)`, and
`Resilience()` throws `ConfigurationException` on its **first statement** (`:146-148`) when
`CommandProcessor.OutboxProducer` is absent. So supplying your own registry without
`.AddBrighterDefault()` fails at startup.

**The detail that makes the remedy pleasant, and it is not in the requirements:**
`AddBrighterDefault` uses **`TryAddBuilder`**, so it *backfills* rather than overwrites. A
reader adds their own pipelines and then calls `.AddBrighterDefault()`; nothing of theirs is
lost. The guide says so.

### 2.2 §13 Q3 is answered, and its premise was already false — RULED BY MEASUREMENT

**Q3 asked whether P0-3 repairs `HandlerFailure.md`'s nack table or links around it.** The
requirements state the table is *"both incomplete and flattening a material difference"* —
four absent rows, and six rows printing `No-op` where three discard the message and three do
not.

**Both defects were repaired on 2026-09-01 by 012's phase 10, at `05ab80c`**, three days
before the requirements were written. Measured:

```bash
git log --oneline -S'Seek` back to the message'   -- contents/HandlerFailure.md   # 05ab80c
git log --oneline -S'The no-ops are not equivalent' -- contents/HandlerFailure.md # 05ab80c
sed -n '263,275p' contents/HandlerFailure.md | grep -c '^|'                       # 12 = 10 rows + 2
```

The table has **ten transport rows**, Kafka's says `Seek` rather than `No-op`, and the
paragraph at `HandlerFailure.md:276` opens *"The no-ops are not equivalent"* and names the
three transports on which a nack discards the message.

**So: P0-3 links the table and repairs nothing.** There is no repair to make.

**This is what the requirements phase carried forward and did not re-check.** It read 012's
*finding* rather than the *page*, and the finding had been actioned by the spec that raised
it. It is the same shape as §2 of the requirements themselves — *an inherited gap list rots*
— arriving one document later, inside the spec that had just written that lesson down. The
cost was one `git log -S`, and the tell was free: the defect was recorded in the *closed*
spec's write-up, and a closed spec's defects are usually closed.

### 2.3 P0-2 is seventeen sites, not ten, and the extra seven are inside the same blocks

The requirements scope P0-2 as ten sites. All ten are confirmed at the recorded line numbers:

```bash
grep -rn -E '\.(ResiliencePipelines|ConfigureResiliencePipelines|Policies)\(' contents/   # 10
```

**Opening those blocks to repair them shows the blocks are V9 throughout.** Seven more dead
call sites live inside the very code being edited, and none can be left standing — a repaired
example that still calls a constructor that does not exist is not repaired.

| Dead name | Sites | Real name @ `10.7.0` | Evidence |
|---|---|---|---|
| `CommandProcessorBuilder.With()` | 5 | `CommandProcessorBuilder.StartNew()` | only `StartNew` is declared, `CommandProcessorBuilder.cs:115` |
| `DispatchBuilder.With()` | 1 | `DispatchBuilder.StartNew()` | `DispatchBuilder.cs:57` |
| `.Subscribers(` | 1 | `.Subscriptions(` | `DispatchBuilder.cs:142` |

**Seventeen dead call sites, then**, and re-deriving them also settles the block count that
sets P0-2's rule-6 budget. Counted by mapping each site into its enclosing fence rather than
by adding up pages:

| Page | Sites | Blocks |
|---|---|---|
| `PolicyRetryAndCircuitBreaker.md` | 4 | 2 |
| `MigratingToPollyV8.md` | 5 | 2 |
| `CommandProcessorConfigurationReference.md` | 1 | 1 |
| `CQRSWithBrighterAndDarker.md` | 1 | 1 |
| `HowConfiguringTheCommandProcessorWorks.md` | 2 | 1 |
| `HowConfiguringTheDispatcherWorks.md` | 6 | 1 |
| **Total** | **19** † | **8** |

† 19 counts the three *further* defects below alongside the 17 dead names, because a block is
strict whichever kind of defect pulls it into the diff. **Eight blocks is the rule-6 budget**,
not the eleven an earlier draft of this section asserted from the page count.

`With()` is absent from `CommandProcessorBuilder.cs` at **both** refs, against a control of 14
files declaring `StartNew` at `10.7.0`. It existed historically and was removed in
`bec0523d0`, so this is a genuine V9 survival rather than an invention.

**`HowConfiguringTheDispatcherWorks.md:54-74` is the worst single block in P0-2** and needs
rewriting rather than substituting. Besides the three dead calls above it carries:

- **`InputChannelFactory`** — **0 files at both refs**, whole repository, against 18 for
  `ChannelFactory`. A type that does not exist.
- **`new RmqMessageConsumerFactory(logger)`** — the constructor is
  `RmqMessageConsumerFactory(RmqMessagingGatewayConnection, IAmAMessageScheduler? = null)`
  (`RmqMessageConsumerFactory.cs:47`). A logger is not a connection.
- **`.MessageMappers(messageMapperRegistry)`** — one argument, where the method takes **four**
  (`DispatchBuilder.cs:87`): sync registry, async registry, transformer factory, async
  transformer factory.

That last one is exactly the shape requirements §11.2 warns about. Passing nulls for the async
half compiles and then throws at `Receive()`, because `Subscription<T>` defaults to
`Proactor`. **So this block is rewritten from the source repository's own test**, per §11.2 —
`tests/Paramore.Brighter.RMQ.Sync.Tests/MessageDispatch/When_building_a_dispatcher.cs` — and
not composed.

**Q5 in §11 puts the widened scope to review.** The recommendation is to accept it: the
alternative is shipping a block that compiles no better than the one it replaced.

### 2.3.1 P0-2 is six pages, and the approved requirements say five — three times

```bash
grep -rl -E '\.(ResiliencePipelines|ConfigureResiliencePipelines|Policies)\(' contents/ | wc -l   # 6
```

The requirements say *"Ten sites across **five** pages"* (§3.2.1), *"10 dead call sites across
**5** pages"* and *"Edits to **5** pages"* (§7 P0-2) — and **§3.2.1's own table immediately
below the first of those has six rows**, as does §9's deliverables table, which lists six edit
rows. `PROMPT.md` repeats *five* as well. **The tables are right and the prose is wrong**, so
nothing downstream was ever misled about *which* pages; only the count is wrong, and it is
wrong in the three places a reader would quote.

Nobody miscounted a set — the six-row table and the five-page sentence were almost certainly
written in the same pass, and the sentence has been copied forward ever since. This is
*re-derive a total, never increment* met inside a document whose own §7 opens by saying a
total with no ref is not a fact, and it is the fourth consecutive phase in this programme to
find a wrong count inside prose that no `grep -c` was ever pointed at. **This design uses six.**

### 2.4 Two further dead-API families, outside P0-2, found by one bounded sweep

Having found that a bare method name is invisible to every gate, the cheap question is *how
much else is there*. One bounded, controlled sweep over `contents/` — every line-leading
`.Method(` token, 103 distinct, each checked at both refs — returned 48 absent from Brighter's
`src/`. Most are correctly absent: EF Core, LINQ, Polly, OpenTelemetry, Hangfire, Moq, and
**Darker**, which versions independently and whose `.AddPolicies(` and
`.AddHandlersFromAssemblies(` are real (`Paramore.Darker.AspNetCore`, verified in `../Darker`).

**Two families are genuinely dead Brighter API.**

**(a) The V9 outbox-registration family — 10 sites, 6 pages.** Replaced at V10 by
`AddProducers(configure => { configure.Outbox = …; configure.ConnectionProvider = …; })`,
whose options type is `ProducersConfiguration` (`src/Paramore.Brighter/ProducersConfiguration.cs`).

| Dead call | Page and line |
|---|---|
| `.UseOutbox(` | `InMemoryOptions.md:219` |
| `.UseInMemoryOutbox(` | `ShowMeTheCode.md:205` |
| `.UseMsSqlOutbox(` | `SweeperCircuitBreaking.md:311`, `UsingSweeperCircuitBreaking.md:39` |
| `.UseMySqlOutbox(` | `DapperOutbox.md:47` |
| `.UseMongoDbOutbox(` | `SweeperCircuitBreaking.md:217` |
| `.UseDynamoDbOutbox(` | `DynamoOutbox.md:41` |
| `.UseDynamoDbTransactionConnectionProvider(` | `DynamoOutbox.md:42` |
| `.UseMySqTransactionConnectionProvider(` | `DapperOutbox.md:48` |
| `.UseInMemoryArchiveProvider(` | `InMemoryOptions.md:135` |

**Nine rows, ten sites, six pages** — the `.UseMsSqlOutbox(` row carries two, so counting rows
undercounts by one. *(An earlier draft of this section said "9 sites, 8 pages" by counting the
table's rows for the first figure and guessing the second. Both were wrong, and the command
that settles it is one line —* `grep -rnE '<the alternation>' contents/ | wc -l` *for sites
against* `grep -rlE` *for pages. `grep -l` counts **files**; this programme has recorded that
as a public-facing error once already, in Brighter#4302's PR body.)*

Each returns **0 files at both refs across the whole repository**, not merely `src/`.
(`.UseMySqTransactionConnectionProvider` also misspells MySql, which is how little anything
reads these.)

**(b) The relational transaction-provider family — 10 sites, 6 pages.** This one matters more,
because it sits in *prose* on the outbox pages, four lines above code blocks that get it right.

```bash
grep -rhoE '[A-Za-z]+(UnitOfWork|EntityFrameworkConnectionProvider)' contents/ | sort -u
```

| Name | @ both refs | Verdict |
|---|---|---|
| `DynamoDbUnitOfWork`, `MongoDbUnitOfWork`, `FirestoreUnitOfWork`, `SpannerUnitOfWork` | 4 / 1 / 1 / 1 | **live** — the control |
| `MsSqlUnitOfWork`, `MySqlUnitOfWork`, `PostgreSqlUnitOfWork`, `SqliteUnitOfWork` | **0** | **dead** |
| `MsSql…`, `MySql…`, `PostgreSql…`, `Sqlite…` `EntityFrameworkConnectionProvider` | **0** | **dead** |

The NoSQL stores really do have a unit of work; the **relational** ones have a
`…TransactionProvider` instead — `PostgreSqlTransactionProvider`
(`Paramore.Brighter.PostgreSql/PostgreSqlTransactionProvider.cs:13`) and
`PostgreSqlEntityFrameworkTransactionProvider<T>` (`…EntityFrameworkCore/…:15`). That the
live and dead names sit in one alphabetical list, four true and eight false, is why nobody
spotted it by reading.

**Affected pages:** `PostgresOutbox.md`, `MSSQLOutbox.md`, `MySQLOutbox.md`,
`SqliteOutbox.md`, `BrighterBasicConfiguration.md`, `CommandProcessorConfigurationReference.md`.

**Two of those are prerequisites of guides this spec writes.** `PostgresOutbox.md:124-125` is
P0-1's own linked page, and `MSSQLOutbox.md:123-124` is P1-2's, and both tell a reader to use
a type that has never existed — in the paragraph immediately above a code block using the
correct one. **A guide cannot honestly link a page that does this.** Q6 in §11 proposes P0-4.

*(A false positive was avoided here and is worth the sentence: `IUnitOfWork` also returns 0,
and it is `DispatchingARequest.md:153`'s illustration of the reader's **own** repository
interface. Not every absent name is a defect.)*

*(And a near-miss on the instrument: the first sweep of the PostgreSQL pages extracted only
`new X(`/`typeof(X)` positions and returned a clean two domain types. `PostgreSqlUnitOfWork`
lives in backticked **prose** and was invisible to it. Re-run including `` `Backticked` ``
identifiers, with the control *"is `PostgreSqlUnitOfWork` now in the candidate list"* asserted
before the totals were read, it found both. **A sweep over code positions cannot see a claim
made in a sentence**, and the claims that rot are made in sentences.)*

### 2.5 One configuration object, three tables — the pivot for both composition guides

`RelationalDatabaseConfiguration`'s constructor
(`src/Paramore.Brighter/RelationalDatabaseConfiguration.cs:21`):

```csharp
public RelationalDatabaseConfiguration(
    string connectionString,
    string? databaseName = null,
    string? outBoxTableName = null,
    string? inboxTableName = null,
    string? queueStoreTable = null,
    string? schemaName = null,
    bool binaryMessagePayload = false,
    bool jsonMessagePayload = false)
```

**The queue store, the Outbox and the Inbox are three parameters on one object.** That is the
whole composition, and it is why the guides are short: a reader does not reconcile two
configurations, they name three tables on one.

**Exactly one page in the corpus names both `queueStoreTable` and `outBoxTableName`** —
`RelationalDatabaseConfigurationReference.md`, 012's reference page — against a control of 4
pages naming the first and 8 naming the second. So the *fact* is documented and the *recipe*
is not, which is requirements §1's thesis arriving with a number attached. Both guides link
that page for the option table and never restate it (§11.5).

### 2.6 The claim check's wiring is on no page at all

`UseExternalLuggageStore<TStoreProvider>` — three overloads,
`ServiceCollectionExtensions.cs:951`, `:971`, `:992` — is how a luggage store reaches
Brighter. It appears on **0 of 157 pages**, against a control of **13** pages mentioning
`ClaimCheck`. That is the identical tell as `AddBrighterDefault` in requirements §3.2:
the corpus discusses the subject on thirteen pages and the entry point on none.

**And `ClaimCheck.md` lists one luggage store where six ship**: `S3LuggageStore` (and its
`.V4` twin), `AzureBlobLuggageStore`, `GcsLuggageStore`, `MongoDbLuggageStore`,
`FileSystemStorageProvider`, `InMemoryStorageProvider`, plus `NullLuggageStore`
(`git grep -l 'IAmAStorageProviderAsync' 10.7.0 -- src/`). The page's list ends after S3 with
an unclosed bold — `**IAmAStorageProviderAsync:` — which is how long it has been since anyone
read the bottom of it.

---

## 3. Documentation structure and reading order

Nothing is re-parented and no existing URL moves. Each guide hangs beneath the page a reader
would land on first, and is linked from the other subjects it composes.

```text
Using an External Bus
├── Claim Check                          ClaimCheck.md            (Explanation, 75)
│   ├── S3 Luggage Store                 S3LuggageStore.md        (Reference, 68)
│   └── Handling Large Messages          HandlingLargeMessages.md      ← P1-1, NEW
└── Error Handling                       HandlerFailure.md        (Explanation, 481)
    ├── Error Handling Options           ErrorHandlingOptions.md  (Reference, 228)
    └── Handling Poison Messages         HandlingPoisonMessages.md     ← P0-3, NEW

Transports
├── PostgreSQL Message Broker            PostgreSQLMessageBroker.md   (Reference, 611)
│   ├── PostgreSQL Broker Trade-Offs     PostgreSQLBrokerTradeOffs.md
│   └── PostgreSQL for Transport and Outbox
│                                        PostgreSQLTransportAndOutbox.md  ← P0-1, NEW
└── MSSQL Message Broker                 MSSQLMessageBroker.md        (Reference, 166)
    └── MSSQL for Transport, Inbox and Outbox
                                         MSSQLTransportInboxAndOutbox.md  ← P1-2, NEW

Edited in place, no move, no rename (P0-2):
  PolicyRetryAndCircuitBreaker.md · MigratingToPollyV8.md
  CommandProcessorConfigurationReference.md · CQRSWithBrighterAndDarker.md
  HowConfiguringTheCommandProcessorWorks.md · HowConfiguringTheDispatcherWorks.md
```

**Reading order a reader actually takes.** Each guide is entered from a search result, not
from the top of the tree, so each states its prerequisites as links in its banner and assumes
nothing else. The inbound routes:

| Guide | Reached from |
|---|---|
| P0-1 | `PostgreSQLMessageBroker.md` *Transactional Messaging*; `PostgresOutbox.md`; Docs#67 |
| P0-3 | `HandlerFailure.md`; `ErrorHandlingOptions.md`; each transport's DLQ section |
| P1-1 | `ClaimCheck.md`; `FAQ.md`'s *How do I handle large messages?* |
| P1-2 | `MSSQLMessageBroker.md`; `MSSQLOutbox.md`; `MSSQLInbox.md` |

---

## 4. File-by-file outline — the four new pages

All four are `How-to`. All four carry front matter with
`layout.description.visible: false`, one H1, a banner, `##` headings qualified by subject, and
a language tag on every fence. **All four use `## Step N: …` headings**, which `CLAUDE.md`
sanctions for a page a reader executes once — a guide is that shape, and Key Concepts /
Configuration / Best Practices fights the order the reader needs. This is the convention
`CLAUDE.md` records for tutorials applied to how-tos for the same reason; **§11 Q7 puts it to
review** rather than assuming it.

Every guide ends with a **verification step** (AC7) and a **failure section** naming the
exception a reader meets if they skip a step (requirements §4 clause 5).

### 4.1 P0-1 — `contents/PostgreSQLTransportAndOutbox.md`

| | |
|---|---|
| **Purpose** | Compose one PostgreSQL database as broker and Outbox so a business write and its message commit together. |
| **Type** | How-to · **Target length** ~330 lines |
| **Traces to** | Q&A [#3795](https://github.com/BrighterCommand/Brighter/discussions/3795), [#3626](https://github.com/BrighterCommand/Brighter/discussions/3626), [Docs#67](https://github.com/BrighterCommand/Docs/issues/67) — 3 askings (AC9) |

**Banner:**

```markdown
> **How-to** · Applies to **Brighter V10** · Prerequisites: [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md), [PostgreSQL Outbox](/contents/PostgresOutbox.md)
```

**Opening sentence** — 156 characters rendered, terminal punctuation, no colon, unique:

> One PostgreSQL database can be both your message broker and your Outbox, so the business
> write and the message announcing it commit in a single transaction.

**Section outline:**

```text
# Use PostgreSQL for Both Transport and Outbox
## Step 1: Install the Packages
## Step 2: Create the Queue and Outbox Tables
## Step 3: Describe Both Tables in One Configuration     ← §2.5, the pivot
## Step 4: Register the Configuration                    ← the #3721 trap
## Step 5: Wire the Producer and the Outbox
## Step 6: Wire the Consumer
## Step 7: Deposit and Clear Inside Your Transaction
## Step 8: Run the Outbox Sweeper
## Step 9: Verify It Worked                              ← AC7
## PostgreSQL Transport and Outbox Failures              ← the two exceptions, by text
## Further Reading
```

**The two failures the page must name, both from the field:**

1. `InvalidOperationException: No Async outbox defined.` — `OutboxProducerMediator.cs:502`,
   from `OutboxSweeper.SweepAsync`. This is Q&A #3795 verbatim, and it is resolved by setting
   `ConnectionProvider` and `TransactionProvider` on `AddProducers`.
2. The missing `IAmARelationalDatabaseConfiguration` registration — the host starts,
   provisions the Outbox and ticks the Sweeper, and only the first
   `GetRequiredService<IAmACommandProcessor>()` throws, naming a type the reader's code never
   mentions. 009's rung 3 met this; it is Brighter
   [#3721](https://github.com/BrighterCommand/Brighter/issues/3721) and
   [#3755](https://github.com/BrighterCommand/Brighter/issues/3755), both closed as
   *not a bug*, with the ergonomics tracked as
   [#4279](https://github.com/BrighterCommand/Brighter/issues/4279).

**Cross-links:** `RelationalDatabaseConfigurationReference.md` for the option table (never
restated), `PostgresDistributedLock.md` for multi-instance sweepers, `OutboxPattern.md`,
`BoxProvisioning.md`, `PostgreSQLBrokerTradeOffs.md`.

### 4.2 P0-3 — `contents/HandlingPoisonMessages.md`

| | |
|---|---|
| **Purpose** | Take a message that fails every time and get it off the channel and into a dead letter queue. |
| **Type** | How-to · **Target length** ~300 lines |
| **Traces to** | Q&A [#3218](https://github.com/BrighterCommand/Brighter/discussions/3218), [#2103](https://github.com/BrighterCommand/Brighter/discussions/2103); issues [#3667](https://github.com/BrighterCommand/Brighter/issues/3667), [#3808](https://github.com/BrighterCommand/Brighter/issues/3808) — 4 askings (AC9) |

**Banner:**

```markdown
> **How-to** · Applies to **Brighter V10** · Prerequisites: [Error Handling](/contents/HandlerFailure.md), [Error Handling Options](/contents/ErrorHandlingOptions.md)
```

**Opening sentence** — 150 rendered:

> A poison message is one that fails every time you process it, and this guide routes it to a
> dead letter queue instead of letting it block the channel.

**Section outline:**

```text
# Handle a Poison Message and Route It to a Dead Letter Queue
## Step 1: Confirm You Have a Poison Message
## Step 2: Choose Between Requeue, Reject and Don't Acknowledge
## Step 3: Set a Requeue Count and a Dead Letter Routing Key
## Step 4: Add a Backstop Attribute
## Step 5: Verify the Message Reaches the Dead Letter Queue    ← AC7
## Step 6: Read the Enrichment Headers
## Step 7: Decide Whether to Replay or Discard
## Poison Message Handling on Your Transport                   ← the per-transport table, LINKED
## Further Reading
```

**Per §2.2 this page repairs nothing.** *Step 2* and *Poison Message Handling on Your
Transport* link `HandlerFailure.md#transport-nack-behavior` and
`ErrorHandlingOptions.md#native-vs-brighter-managed-dlq` by anchor; neither table is copied
(requirements §8). The guide's contribution is the **route**, which is on no page: the
existing two carry semantics and options and never have the reader do it.

**Named APIs, all verified live at both refs:** `RejectMessageAction`, `DeferMessageAction`,
`DontAckAction`, `InvalidMessageAction`, the three `…OnErrorAttribute` backstops,
`RequeueCount`, `RequeueDelay`, `UnacceptableMessageLimit`, `UnacceptableMessageLimitWindow`,
`DontAckDelay`, `DeadLetterNamingConvention`, `InvalidMessageNamingConvention`,
`IUseBrighterDeadLetterSupport`.

**The gotcha the guide must state**, because it is the one thing the reader cannot infer: on
MSSQL, Redis and MQTT a nack **discards** the message, so `DontAckAction` loses it and
`DeferMessageAction` is the only safe choice. `HandlerFailure.md:276` says this; the guide
points at it rather than repeating it.

### 4.3 P1-1 — `contents/HandlingLargeMessages.md`

| | |
|---|---|
| **Purpose** | Put a payload too large for the transport into a luggage store and send a claim check instead. |
| **Type** | How-to · **Target length** ~260 lines |
| **Traces to** | 1 Q&A + `FAQ.md` *"How do I handle large messages?"*; 011 `worklist.md` §8's surviving gap (AC9) |

**Banner:**

```markdown
> **How-to** · Applies to **Brighter V10** · Prerequisites: [Claim Check](/contents/ClaimCheck.md), [Message Mappers](/contents/MessageMappers.md)
```

**Opening sentence** — 135 rendered:

> When a message body outgrows what your transport will carry, a claim check stores the
> payload elsewhere and sends a token in its place.

**Section outline:**

```text
# Put a Large Payload Behind a Claim Check
## Step 1: Find Your Transport's Message Size Limit
## Step 2: Choose a Luggage Store
## Step 3: Register the Luggage Store                    ← UseExternalLuggageStore, on 0 pages
## Step 4: Attach the Claim Check to Your Mapper
## Step 5: Choose a Threshold
## Step 6: Verify the Payload Went to the Store          ← AC7
## Claim Check Failures
## Further Reading
```

**Step 3 is the reason this page exists.** `UseExternalLuggageStore<TStoreProvider>` is on 0
of 157 pages (§2.6), so a reader following `ClaimCheck.md` attaches the attribute and gets no
store. *Step 2* also supplies the six-store list `ClaimCheck.md` lacks, as a linked table, and
`ClaimCheck.md` gains a pointer to it. **`MessageTransforms.md`'s ruling binds this page**: a
transform of your own needs a custom mapper to attach it to.

### 4.4 P1-2 — `contents/MSSQLTransportInboxAndOutbox.md`

| | |
|---|---|
| **Purpose** | Compose SQL Server as broker, Outbox and Inbox — the second instance, establishing that §2.5 generalises. |
| **Type** | How-to · **Target length** ~300 lines |
| **Traces to** | Q&A [#3960](https://github.com/BrighterCommand/Brighter/discussions/3960)'s first stated goal (AC9) |

**Banner:**

```markdown
> **How-to** · Applies to **Brighter V10** · Prerequisites: [MSSQL Message Broker](/contents/MSSQLMessageBroker.md), [MSSQL Outbox](/contents/MSSQLOutbox.md)
```

**Opening sentence** — 155 rendered:

> One SQL Server database can carry your message queue, your Outbox and your Inbox together,
> sharing a single connection string and one configuration object.

**Section outline** mirrors P0-1 with an Inbox step inserted after step 5, and its *Further
Reading* points back at P0-1 so the two read as one pattern. **It is written after P0-1 and
deliberately shares its shape** — the value here is establishing the pattern generalises, so
divergence would be a defect.

**The `MsSqlSubscription` caveat is load-bearing and comes from Brighter#4302.**
`MessagingGateway.MsSql/ChannelFactory.cs:46` **downcasts** `Subscription` to
`MsSqlSubscription` and throws `ConfigurationException` when the cast fails (again at `:65`
and `:88`). A `Subscription<T>` compiles perfectly and dies at `dispatcher.Receive()`. The
guide therefore types every subscription `MsSqlSubscription<T>`, as
`MSSQLMessageBroker.md:142` already does, and **says why**.

---

## 5. P0-2 — the repair, site by site

**Seventeen dead call sites and three further defects, in eight code blocks across six pages**
(§2.3, §2.3.1). Nothing is renamed, no URL moves, no `SUMMARY.md` entry changes. Each block is
repaired **and then compiled** (AC5).

| Page | Line | Today | Becomes |
|---|---|---|---|
| `PolicyRetryAndCircuitBreaker.md` | 351 | `CommandProcessorBuilder.With()` | `CommandProcessorBuilder.StartNew()` |
| | 355 | `.Policies(policyRegistry)` | *(fold into the next line)* |
| | 356 | `.ResiliencePipelines(registry)` | `.Resilience(registry, policyRegistry)` |
| | 372 | `.ConfigureResiliencePipelines(registry => …)` | `options.ResiliencePipelineRegistry = …` inside `AddBrighter` |
| `MigratingToPollyV8.md` | 99, 109 | `CommandProcessorBuilder.With()` | `…StartNew()` |
| | 101, 111 | `.Policies(policyRegistry)` | *(fold)* |
| | 112 | `.ResiliencePipelines(registry)` ✅-marked | `.Resilience(registry, policyRegistry)` |
| `CommandProcessorConfigurationReference.md` | 104 | `.ConfigureResiliencePipelines(…)` | `options.ResiliencePipelineRegistry = …` |
| `CQRSWithBrighterAndDarker.md` | 378 | `.ConfigureResiliencePipelines(…)` | `options.ResiliencePipelineRegistry = …` |
| `HowConfiguringTheCommandProcessorWorks.md` | 234 | `CommandProcessorBuilder.With()` | `…StartNew()` |
| | 236 | `.Policies(policyRegistry)` | `.Resilience(registry, policyRegistry)` |
| `HowConfiguringTheDispatcherWorks.md` | 54-74 | the whole block — see §2.3 | **rewritten from the RMQ dispatcher test** |

**`.Policies(` folds rather than maps.** The V9 chain had two calls where V10 has one with an
optional second parameter — `Resilience(resiliencePipelineRegistry, policyRegistry)` — so a
site-for-site substitution would print two calls where one belongs. **This is precisely what
`MigratingToPollyV8.md:116` gets wrong today**, telling the reader to use both methods
together. On the migration page, for the highest-demand topic in the corpus.

**Every edited block additionally gains what is on zero pages today** (requirements §7 P0-2):

- `.AddBrighterDefault()`, with a sentence on the `??=` trap and on `TryAddBuilder`
  backfilling rather than overwriting (§2.1).
- `UseResiliencePipelineAsync`, which is on 2 pages and neither is the resilience how-to.

**Two spellings are correct and must not be swept.** `.AddPolicies(` on
`QueryPipelinePolicies.md:56` and `DarkerBasicConfiguration.md:280` is **real in Darker
4.1.1**, and `.AddHandlersFromAssemblies(` across ten Darker pages is real in
`Paramore.Darker.AspNetCore`. Both were re-verified in `../Darker` for this design. **Ask
which product before editing.**

**Rule 6 budget.** The sites fall in **8 distinct C# blocks** (§2.3), every one of which
becomes strict under `--changed`, so each earns real `using` directives. The warning count is
**779** at `89195d8` and **must fall** — a phase that edits eight blocks and moves it by
nothing has not edited what it thought it did.

---

## 6. `SUMMARY.md` changes

Four nested entries, no new section, **twelve sections unchanged**, top-level count unchanged.
Per requirements §13 Q2 there is no *Guides* section and 010's three-or-more test is not met.

**Before / after — `## Using an External Bus`:**

```diff
 * [Claim Check](/contents/ClaimCheck.md)
   * [S3 Luggage Store](/contents/S3LuggageStore.md)
+  * [Handling Large Messages](/contents/HandlingLargeMessages.md)
 * [Compression](/contents/Compression.md)
@@
 * [Error Handling](/contents/HandlerFailure.md)
   * [Error Handling Options](/contents/ErrorHandlingOptions.md)
+  * [Handling Poison Messages](/contents/HandlingPoisonMessages.md)
```

**Before / after — `## Transports`:**

```diff
 * [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md)
   * [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md)
+  * [PostgreSQL for Transport and Outbox](/contents/PostgreSQLTransportAndOutbox.md)
 * [MSSQL Message Broker](/contents/MSSQLMessageBroker.md)
+  * [MSSQL for Transport, Inbox and Outbox](/contents/MSSQLTransportInboxAndOutbox.md)
 * [GCP Pub/Sub Configuration](/contents/GcpPubSubConfiguration.md)
```

**The `SUMMARY.md` entry text is the `/llms.txt` title**, not the page's H1 — 010 measured 32
pages where the two differ and the index used the `SUMMARY.md` text on all 32. The four
entries above are therefore chosen as titles, and each is shorter than its H1 on purpose.

**Predicted published URLs**, all three segments:

| Page | URL |
|---|---|
| P0-1 | `transports/postgresqlmessagebroker/postgresqltransportandoutbox` |
| P0-3 | `using-an-external-bus/handlerfailure/handlingpoisonmessages` |
| P1-1 | `using-an-external-bus/claimcheck/handlinglargemessages` |
| P1-2 | `transports/mssqlmessagebroker/mssqltransportinboxandoutbox` |

The corpus today is **74 pages at two segments, 81 at three, 2 at four** (`urlmap.py`,
`89195d8`), so S3's ceiling of 4 is not approached. **No existing URL moves**, because all
four are new files nested under pages that keep their own place — so
`--check-redirects` does not move, which has now held eight times.

**Filenames are settled here, not at writing time**, because the filename *is* the slug.
None of the four collides with an existing file or slug — verified.

> **`urlmap.py` prints its `N pages` banner to stderr, and a `tail -n +2` on its stdout
> therefore drops a real page.** The distribution above first came out as 73 / 81 / 2, which
> sums to **156** against a banner saying **157** — the dropped row was
> `brighter-configuration/analyzersupport`. Nothing errored and the shape of the answer was
> right, which is the whole problem. **The tell was free and arithmetic: a distribution that
> does not sum to the total it was derived from.** Redirect stderr (`2>/dev/null`) rather than
> skipping a line, and **sum the buckets before quoting any of them.**

---

## 7. Code examples plan

| # | Example | Page | Source | Complete? |
|---|---|---|---|---|
| 1 | Package installs | P0-1 | `PostgresOutbox.md` §NuGet, re-pinned | complete |
| 2 | Queue + Outbox DDL | P0-1 | `PostgreSQLMessageBroker.md:39`, `PostgresOutbox.md:64` | complete |
| 3 | One `RelationalDatabaseConfiguration`, three tables | P0-1 | **written from the type**, §2.5 | complete |
| 4 | Registering `IAmARelationalDatabaseConfiguration` | P0-1 | `PostgresOutbox.md:116` | complete |
| 5 | `AddProducers` with registry + outbox + providers | P0-1 | composed; **compiled** | complete |
| 6 | `AddConsumers` with `PostgresSubscription` + channel factory | P0-1 | `PostgreSQLMessageBroker.md:145` | complete |
| 7 | Deposit / commit / clear | P0-1 | `PostgreSQLMessageBroker.md:362`, made runnable | complete |
| 8 | `UseOutboxSweeper` | P0-1 | `PostgresOutbox.md:169` | complete |
| 9 | Verification SQL + expected log lines | P0-1 | **measured on a real run** | complete |
| 10 | A failing handler | P0-3 | written | complete |
| 11 | Subscription with `requeueCount` + `deadLetterRoutingKey` | P0-3 | `ErrorHandlingOptions.md:122` | complete |
| 12 | `RejectMessageOnErrorAttribute` backstop | P0-3 | `HandlerFailure.md:217` | complete |
| 13 | Reading the DLQ and its enrichment headers | P0-3 | written | complete |
| 14 | Luggage store registration | P1-1 | **written from `ServiceCollectionExtensions.cs:951`** | complete |
| 15 | Mapper with `[ClaimCheck]` / `[RetrieveClaim]` | P1-1 | `ClaimCheck.md:25`, `:38` | complete |
| 16 | MSSQL three-table configuration | P1-2 | mirrors #3 | complete |
| 17 | MSSQL producer + consumer + inbox | P1-2 | `MSSQLMessageBroker.md:107` | complete |
| 18-25 | The eight repaired P0-2 blocks | six pages | §5 | complete |

**Every block carries its real `using` directives** — the new pages by construction (a new
page is 100% added lines and strict), the eight repaired blocks by budget (§5). No block on a
new page uses `// ...` to escape rule 6; `// ...` marks genuine elision only.

**The harness is 009's and is not re-invented:** extract each page's own fences into one
project per block and build them, with **`<ImplicitUsings>disable</ImplicitUsings>`**, against
the packages `tools/optioncheck/optioncheck.csproj` pins. **Do not add
`Microsoft.Extensions.Hosting`** — that is `NU1605`.

**And compiling is necessary, not sufficient** (requirements §11.2). Three examples here sit
downstream of a base type or interface and therefore get the extra treatment — *mirror the
source repository's own test rather than composing an example*:

| Example | Why | Mirror |
|---|---|---|
| 18-25's dispatcher block | `ChannelFactory` downcasts `Subscription` | `Paramore.Brighter.RMQ.Sync.Tests/MessageDispatch/When_building_a_dispatcher.cs` |
| #17 | MsSql `ChannelFactory.cs:46` downcasts to `MsSqlSubscription` | the MsSql gateway tests |
| #5 | `TransactionProvider` is a `Type`, activated by the container | 009 rung 3's sample |

---

## 8. Style notes

- **No new terminology.** Every term is in `Glossary.md` or `BasicConcepts.md`. *Poison
  message*, *dead letter queue*, *claim check* and *luggage store* are all already defined;
  each guide links the definition on first use rather than redefining it.
- **`## Step N: …` headings**, per §4. They satisfy heading qualification on their own terms —
  a step heading names what the step does and is unique across pages — and *Further Reading*
  stays unqualified as an allowlisted navigation heading.
- **The reader's words, not the framework's.** Titles are *"Handle a Poison Message…"*, not
  *"RejectMessageAction Configuration"*. Three of the four clusters were diagnosed from an
  exception message, so each guide prints the **exception text** a reader will have searched.
- **Link, never copy.** Option values come from 012's tables by link. A copied table is drift
  with a checker that cannot see it — `optioncheck` binds a marker to a type, and a second
  unmarked copy is invisible to it.
- **Version markers.** No guide is expected to need ❌/✅, because all four document live API.
  P0-2's edits are the exception and they **remove** a ✅ that was marking a method which never
  existed.
- **API liveness.** Every API named by a new page is **live at `10.7.0`**, so no guide carries
  the *Not in a released package yet* blockquote. That is a property of this spec, not a rule:
  requirements §11.4 permits documenting a forthcoming API when it says so explicitly.

---

## 9. Gate movement, predicted before the work

Baseline at `89195d8`, all seven re-run 2026-09-05 and matching `9aef400`:

| Gate | Now | After P0-2 (edits) | After all four pages |
|---|---|---|---|
| `linkcheck.py` | 160 files | **160** | **164** |
| `pagelint.py` pages | 158 | **158** | **162** |
| `pagelint.py` warnings | 779 | **must fall** | must not rise |
| `urlmap.py --check-shape` | 157 pages, 12 sections, widest 12 of 20 | **unmoved** | **161 pages, 12 sections, widest 12 of 20** |
| `urlmap.py --check-redirects` | 77 entries, 7858 bytes | **unmoved** | **unmoved** |
| `versioncheck.py` | 0 stale of 18 across 5 pages | unmoved | unmoved |
| `optioncheck` | 0 across 59 tables, 519 rows | **unmoved** | **unmoved** — no guide carries a marker |
| `urlmap.py --verify` | 157/157 | unmoved | **161/161** after publication |

**Three predictions are load-bearing and each has failed for someone before:**

1. **A nested page does not move `--check-shape`'s widest.** Proven at 010's phase 7 with four
   nested pages at once. Assert it; do not assume it.
2. **`--check-redirects` does not move when a page is added to an existing section.** Slugs are
   filename-derived, so appending inside a section moves no URL. Held eight times.
3. **A P0-2-only PR moves nothing but the warning count**, so a vacuous `--changed` pass is
   invisible. **Read `--changed`'s scope line — `N code block(s) strict` — not its verdict.**
   Expect **8**, and `git add` before trusting a local run, because `git diff` cannot see an
   unstaged file.

---

## 10. Sequencing

One PR per phase; each phase is a coherent unit merged before the next branch starts.

| Phase | Deliverable | Gates expected to move |
|---|---|---|
| **1** | **P0-2**, all seventeen sites across six pages, eight blocks compiled; **P0-4**, 10 prose names across six pages | pagelint warnings only |
| **2** | **P0-1**, the committed guide, + `SUMMARY.md` + `pagetypes.tsv` | link, pagelint, shape, `--verify` |
| **3** | **P0-3**, poison messages | the same four |
| **4** | **P1-1** and **P1-2** | the same four |
| **5** | **Acceptance pass** — AC1-AC10 walked with evidence, AC9 **backwards** | none |

**P0-2 goes first** even though P0-1 is the public commitment, for one reason: it is a
correctness repair on published pages that tell readers to call methods that do not exist, and
`MigratingToPollyV8.md` is endorsing one of them with a ✅. **Nothing new should be written on
top of an API surface the corpus spells wrongly.**

**P1-2 follows P0-1 in the same phase deliberately** — its value is showing the pattern
generalises, which is only demonstrable once there is a pattern.

**AC9 is walked backwards at phase 5**, per requirements §12: every §3.1 cluster with two or
more askings, against the delivered set. Forwards can only ever find guides that were written;
backwards is what found 012's AC1 failure in one command at the last possible moment.

---

## 11. Questions for review

> **Q1 and Q2 were answered 2026-09-04 and are not re-raised.** Q3 is answered below by
> measurement rather than by ruling. Q4 stands deferred. Q5, Q6 and Q7 are new, and Q5 and Q6
> are scope questions this design cannot settle alone.

### Q3 — Does P0-3 repair `HandlerFailure.md`'s nack table, or link around it? **ANSWERED: neither — there is nothing to repair.**

The defects were fixed by 012's phase 10 at `05ab80c`, three days before the requirements
described them as open. The table has ten rows, Kafka's says `Seek`, and the paragraph at
`:276` distinguishes the three transports that discard from the three that redeliver. **P0-3
links it.** See §2.2 for the commands. *No maintainer ruling is needed; this is a measurement.*

### Q4 — Does any guide get a compiled sample in `../Brighter/samples/`? **STILL DEFERRED, as agreed.**

A write there is authorised **per PR**, so it is asked when the PR exists. This design records
which one wants it and why: **P0-1**, because a composition of two subsystems is exactly what
rots inside a markdown fence, and because its verification step (example #9) claims what a
reader sees on a real run. **Phase 2 is where the question is asked.**

### Q5 — Does P0-2 widen from ten sites to seventeen? **ANSWERED 2026-09-06: yes, widen.**

The requirements approved ten. Repairing them means opening blocks that contain seven more dead
calls — `With()` ×6, `.Subscribers(` — plus `InputChannelFactory`, a wrong
`RmqMessageConsumerFactory` argument and a one-argument `.MessageMappers(` that takes four
(§2.3). **Leaving them would ship a repaired example that still does not compile**, which
fails AC5 on the page P0-2 exists to fix. The cost is one block rewritten rather than
substituted, in `HowConfiguringTheDispatcherWorks.md`.

### Q6 — Do the two newly-found dead-API families become a P0-4? **ANSWERED 2026-09-06: yes, absorb the scope change — as recommended below.**

> **The ruling is the split below, taken as proposed** — **P0-4 is the relational
> transaction-provider family only**, and the V9 outbox-registration family stays *recorded,
> not scheduled*. Recorded explicitly because *"absorb this scope change"* is singular and the
> recommendation had two halves: **if the intent was to absorb both families, P0-4 doubles to
> 20 sites across 12 pages and this line is the one to correct.**

§2.4 found **20 sites across 12 pages**, eleven of those pages outside P0-2's scope, in two
families. They are the
same defect class as P0-2 and every gate is green on all of them. The design's recommendation
splits them:

- **The relational transaction-provider family (10 sites, 6 pages) should be P0-4.** Two of
  those pages — `PostgresOutbox.md` and `MSSQLOutbox.md` — are the stated prerequisites of
  P0-1 and P1-2. **A guide cannot honestly link a page that names a type which has never
  existed**, and the fix is two prose lines per page, in a paragraph whose own code block four
  lines below already uses the correct name. It is cheap and it is on this spec's critical
  path.
- **The V9 outbox-registration family (10 sites, 6 pages) should be recorded, not scheduled.**
  None is on a page this spec links, each needs a genuine V10 rewrite rather than a
  substitution, and the six pages are otherwise untouched by 013. It belongs to a sweep of
  its own, and it is evidence for 014's defect 8.

**This was a scope question, not a technical one, which is why it went to review rather than
being absorbed.** Requirements §7's priorities are demand-ordered; neither family has an
asking behind it, and both are correctness defects, which the priority scheme does not rank.

**P0-4, as ruled** — *Repair the relational transaction-provider names*, 10 sites across 6
pages: `PostgresOutbox.md`, `MSSQLOutbox.md`, `MySQLOutbox.md`, `SqliteOutbox.md`,
`BrighterBasicConfiguration.md`, `CommandProcessorConfigurationReference.md`. It ships in
**phase 1 beside P0-2**, because both are dead-API repairs on published pages and because P0-1
and P1-2 link two of these six.

**The replacement table, every name verified at both refs, with a control:**

| Dead | Live replacement | Files @ both refs |
|---|---|---|
| `MsSqlUnitOfWork` | `MsSqlTransactionProvider` | 1 |
| `MySqlUnitOfWork` | `MySqlTransactionProvider` | 1 |
| `PostgreSqlUnitOfWork` | `PostgreSqlTransactionProvider` | 1 |
| `SqliteUnitOfWork` | `SqliteTransactionProvider` | 1 |
| `MsSqlEntityFrameworkConnectionProvider<T>` | **`MsSqlEntityFrameworkCoreTransactionProvider<T>`** | 1 |
| `MySqlEntityFrameworkConnectionProvider<T>` | `MySqlEntityFrameworkTransactionProvider<T>` | 1 |
| `PostgreSqlEntityFrameworkConnectionProvider<T>` | `PostgreSqlEntityFrameworkTransactionProvider<T>` | 1 |
| `SqliteEntityFrameworkConnectionProvider<T>` | `SqliteEntityFrameworkTransactionProvider<T>` | 1 |

> **MSSQL's EF provider has `Core` in its name and the other three do not.** This paragraph
> first prescribed `MsSqlEntityFrameworkTransactionProvider`, by **pattern from its three
> siblings** — and that name returns **0 at both refs**. The real one is
> `MsSqlEntityFrameworkCoreTransactionProvider`
> (`src/Paramore.Brighter.MsSql.EntityFrameworkCore/`), and `MongoDbEntityFrameworkTransactionProvider`
> shows the un-`Core`d form is the majority, which is exactly what made the wrong guess
> comfortable. **A repair that invents a name is the defect it was written to fix**, and this
> one was caught only because the eight replacements were checked as a batch with a control
> rather than assumed from the four dead ones. `git grep -ohE 'class [A-Za-z]*EntityFramework[A-Za-z]*'`
> enumerates the family in one command; **enumerate, do not extrapolate.**

### Q7 — Do how-to pages use `## Step N: …` headings? **ANSWERED 2026-09-06: yes.**

`CLAUDE.md` records the `## Step N:` convention for **tutorials** and gives the reason: a page
a reader executes once is not a page they consult, so Key Concepts / Configuration / Best
Practices fights the order they need. **That reason applies to a how-to exactly as well**, and
§4 adopts it for all four guides. It is recorded here rather than assumed because `CLAUDE.md`
names only tutorials, and because 014's defect 1 is that no command asks this question at all.
**`CLAUDE.md`'s paragraph now says "Tutorial and How-to pages"**, edited 2026-09-06 in this
design's own PR rather than the spec's last, because the writing phases follow `CLAUDE.md`
and would otherwise follow the stale form. **It binds pages written from here on and does
not reach the 53 existing How-to pages**, whose published anchors would move for no reader
benefit.

---

## 12. Traceability

| Requirement | Where it is met |
|---|---|
| §4 target state, clauses 1-6 | §4 — every guide has steps, a verification step, and a failures section |
| §7 P0-1 | §4.1, phase 2 |
| §7 P0-2 | §5, phase 1 — **widened, Q5** |
| §7 P0-3 | §4.2, phase 3 — **Q3 answered, nothing to repair** |
| §7 P1-1, P1-2 | §4.3, §4.4, phase 4 |
| **P0-4** — not in the requirements; added at design review 2026-09-06 | §11 Q6, phase 1 |
| §8 out of scope | §4.2 and §8 — tables linked, never copied |
| §9 deliverables | §4, §5 — **filenames settled**, §6 |
| §10 `SUMMARY.md` | §6 — four nested entries, no *Guides* section |
| §11.1 `CLAUDE.md` conventions | §4, §8 — banners, front matter, qualified headings, opening sentences measured |
| §11.2 compile, and its limit | §7 — the harness, plus three examples mirroring the source's own tests |
| §11.3 rule 6 budget | §5, §9 — 8 strict blocks, 779 must fall |
| §11.4 API liveness, three states | §2.1, §2.3, §2.4, §8 — every name checked at **both** refs |
| §11.5 link, do not copy | §4.2, §7, §8 |
| §11.6 both refs, with a control | §2.1, §2.3, §2.4 — every sweep carries its control |
| §11.7 namespace claims | phase-level obligation; every `using Paramore.*` gets `git grep "namespace X" 10.7.0 -- src/` with a control |
| §11.8 samples by PR | §11 Q4 |
| §12 AC1-AC10 | §10 phase 5; AC9 walked **backwards** |
| §13 Q1, Q2 | settled, not re-raised |
| §13 Q3 | **§2.2, §11 — answered by measurement** |
| §13 Q4 | **§11 — deferred to phase 2's PR, as agreed** |
| §14 workflow friction | §13 below |

---

## 13. Workflow friction — more of Spec 014's evidence

Recorded as met, per 014's README instruction to record rather than route around. These are
**new**, beyond requirements §14's four.

5. **`/spec:design`'s contract has no step for verifying the API the design will print.** Its
   *"Key code examples needed (describe each, note source file/sample)"* asks where an example
   comes from and never whether it is *correct*. Everything in §2 of this document — six
   findings, two new dead-API families, a stale premise in an approved requirement — came from
   work the command does not ask for. This is 014's defect 8 met from the design side, having
   been predicted from the implement side.
6. **Nothing asks a design to re-verify its own requirements.** §2.2 found an approved
   requirement resting on a defect that had been repaired before it was written. The command
   says *"Reference the requirements for traceability"*, which reads as *quote them*. **A
   design that only quotes its requirements inherits their errors**, and this is the second
   consecutive phase in this spec to find a stale inherited claim — §2 of the requirements
   found five of six, and §2.2 here found one more.
7. **`/spec:design` prescribes no page-type or heading convention**, so Q7 exists. The command
   asks for a *"Section outline with headings (H1, H2, H3)"* and says nothing about the banner,
   qualified headings, opening sentences, front matter or the `## Step N:` convention — all of
   which `pagelint.py` enforces or `CLAUDE.md` requires. 014's scope sketch already proposes
   pointing the command at the conventions; this is the measured instance.
8. **No command asks a phase to predict which gates it will move.** §9 exists because
   `PROMPT.md` says to, not because any command does — and the reason it matters is that three
   of this spec's phases expect *no* movement, which is exactly when a vacuous pass is
   invisible.
