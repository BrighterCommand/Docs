# Requirements: Spec 012 — Configuration Reference Tables

**Created:** 2026-08-27
**Status:** Approved 2026-08-27 (reviewed; §13's three questions answered by the
maintainer and applied throughout — the first commissions five transport pages and
grew the spec). **Amended 2026-08-27 at design review**, which struck AC4's schedule
clause and added **D15** and **D16** — three pages, ten `SUMMARY.md` entries, and zero
additional options. Every amendment is marked in place; `design.md` §13 carries the
rulings.
**Supersedes on every point of fact:** `spec/012-configuration_reference/README.md`,
written 2026-08-03 against a tree Spec 010 has since changed and a scope list the
source no longer matches. §3 records where the two disagree.

---

## 1. Topic overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) asks for reference
documentation with *"precise technical information about the API"*, citing
[Microsoft Learn's API browser](https://learn.microsoft.com/en-us/dotnet/api/).

That comparison names two products, and the split the README drew is still right:

- **Generated API reference** — every public type and member, from XML doc comments.
  Built from source; belongs in the **Brighter repository**. Out of scope here, and
  §8 says so.
- **Prose configuration reference** — name, type, default, description, constraints,
  for the options a reader actually sets. **This is 012.**

Nobody is looking up `IAmACommandProcessor`'s member list. They are trying to find out
what `MakeChannels` defaults to — and **today that question has no answer in the
corpus.**

Measured 2026-08-27: `MakeChannels` appears **23 times across 11 pages** totalling
**5,642 lines**, and **not one of those lines pairs it with the word "default"**. The
answer is `OnMissingChannel.Create`, and it exists only in
`Brighter/src/Paramore.Brighter/Subscription.cs:208`.

That is the spec in one example: the option is named on eleven pages, demonstrated in
code samples, and its default is documented nowhere.

**What this spec adds beyond tables is a checker.** A hand-maintained default is stale
the moment someone changes it in Brighter, and stale *silently* — a wrong default in a
reference table is worse than an absent one, because the reader has no reason to doubt
it. `optioncheck` is the deliverable that makes the tables self-policing, and §5
records the decision that shapes it.

---

## 2. The configuration surface — measured, and it governs everything here

Measured at Brighter **`10.7.0`** (tag `c1b8af886`), the released version
`versioncheck.py` resolves from NuGet and every tutorial page pins. Re-derive with:

```bash
python3 spec/012-configuration_reference/survey.py --ref 10.7.0
```

| Quantity | At `10.7.0` |
|---|---|
| Configuration types | **67** |
| Reader-facing options | **619** |
| …constructor-driven types | **18** (317 params, **270** carrying a default) |
| …property-driven types | **49** (302 settable properties) |

**The surface comes in two shapes, and this is the finding that shapes the tool.**

- **Publications and options classes are property-driven.** `KafkaPublication` takes
  no constructor parameters; you set properties. Defaults live in property
  initialisers and are readable by instantiating the type and reading it back.
- **Subscriptions are constructor-driven.** `KafkaSubscription` takes **30**
  constructor parameters, **27** of them defaulted — `bufferSize = 1`,
  `commitBatchSize = 10`, `offsetDefault = AutoOffsetReset.Earliest` — and exposes
  get-only properties. `RmqSubscription` is 24/21, `SqsSubscription` 24/20,
  `PostgresSubscription` 24/21, `AzureServiceBusSubscription` 18/15, and the core
  `Subscription` 17/14.

**A property-only reflection pass sees 354 of the 619 options and misses 265** — 43%
of the surface, and the 43% a reader most needs a table for, because subscriptions are
where the tuning lives.

The README's *Default extraction* bullet contemplated only the property route and
called constructor defaults *"harder"*. They are not harder — `ParameterInfo`
exposes `HasDefaultValue` and `DefaultValue` straight from assembly metadata — but
they are **invisible to a checker that only walks properties**, which is the failure
this section exists to prevent. §5's decision turns on it.

> **This is *"ask what a figure counted"* caught in flight.** The first pass at this
> survey counted settable public properties and reported **356**. It was a correct
> count of the wrong quantity: it saw no subscription at all. The number is in this
> document only as a cautionary note, and `survey.py` prints both shapes and the miss
> figure precisely so the mistake cannot be made silently again.

---

## 3. Current state — measured 2026-08-27

### 3.1 What the corpus documents in a table today

Across all **147** pages under `contents/`, an option-shaped table — a header row
naming *Option*, *Property*, *Parameter*, *Setting*, *Name*, *Field* or *Member* —
appears on **9 pages**, in **12 tables**, carrying **44 option rows**.

| Page | Tables | Option rows |
|---|---|---|
| `PostgreSQLMessageBroker.md` | 3 | 17 |
| `AsyncAPISupport.md` | 1 | 7 |
| `CausationTrackingStores.md` | 2 | 6 |
| `AzureBlobDistributedLock.md` | 1 | 4 |
| `DynamoDbDistributedLock.md` | 1 | 4 |
| `FirestoreDistributedLock.md` | 1 | 2 |
| `MongoDbDistributedLock.md` | 1 | 2 |
| `PostgresDistributedLock.md` | 1 | 1 |
| `SweeperCircuitBreaking.md` | 1 | 1 |

**44 of 619 options appear in a table — 7%.** The claim is precisely that and no more:
many of the other 575 *are* documented, in prose or in a code sample. What they are not
is scannable, extractable into a retrieval chunk, or checkable by a tool.

### 3.2 The two pages already named *Configuration Reference* carry no table at all

Spec 010 created `contents/CommandProcessorConfigurationReference.md` (**679** lines)
and `contents/DispatcherConfigurationReference.md` (**240** lines). Both are organised
by API surface, both are good pages, and **both contain zero option tables** — they are
prose and code samples throughout.

This matters twice. It confirms 012's premise at the highest-traffic surface, measured
rather than asserted. And it means 012's first job on those two pages is **additive** —
give each section its table — not a rewrite. They are the natural P0 host pages, and
they already sit in `SUMMARY.md` under *Brighter Configuration*.

### 3.3 Five shipping transports have no configuration page

Ten distinct transports ship at `10.7.0` — counting `AWSSQS`/`AWSSQS.V4` as one
transport in two package generations, and `RMQ.Async`/`RMQ.Sync` as one transport with
two clients:

| Transport | Configuration page |
|---|---|
| RabbitMQ | `RabbitMQConfiguration.md` |
| Kafka | `KafkaConfiguration.md` |
| AWS SNS/SQS | `AWSSQSConfiguration.md` |
| Azure Service Bus | `AzureServiceBusConfiguration.md` |
| PostgreSQL | `PostgreSQLMessageBroker.md` |
| In-memory | `InMemoryTransport.md` |
| **GCP Pub/Sub** | **none** |
| **RocketMQ** | **none** |
| **MQTT** | **none** |
| **Redis** | **none** |
| **MSSQL** | **none** |

**Grepped before being recorded as absent**, which this programme has learned to do the
hard way. The result splits the five into two kinds, and the difference is the
interesting part:

- **GCP Pub/Sub and RocketMQ have zero mentions in 147 pages.** Not a page, not a row,
  not a passing reference.
- **MQTT, Redis and MSSQL appear only as rows in cross-cutting comparison tables** —
  `ReactorAndProactor.md:284`, `HandlerFailure.md:270`, `ErrorHandlingOptions.md:113`.
  So the corpus asserts their Reactor/Proactor support and their nack semantics, and
  then offers the reader nowhere to go to configure them. A reader learns from
  `ReactorAndProactor.md` that Redis supports both APIs natively and hits a dead end.

### 3.4 The cross-cutting tables are themselves stale, which is the drift 012 exists to stop

Those comparison tables are reference tables by any reading, and they do not match the
shipped transport set:

- `ReactorAndProactor.md`'s table lists **9 transports** and omits **GCP Pub/Sub** and
  **RocketMQ**.
- `HandlerFailure.md`'s nack table lists **6** and omits **GCP Pub/Sub**, **RocketMQ**,
  **PostgreSQL** and **MSSQL** — the last two being transports the neighbouring page
  does list.

Nothing is malformed and every gate in this repository is green on both pages. **This is
the drift problem occurring today, in the corpus, at exactly the kind of table 012
proposes to add 619 rows to** — which is the argument for building the checker
alongside the tables rather than after them.

### 3.5 Outbox, inbox, scheduler and lock coverage

- **Outbox stores at `10.7.0`**: DynamoDB, Firestore, MongoDb, MsSql, MySql,
  PostgreSql, Spanner, Sqlite. Pages exist for MSSQL, MySQL, Postgres, Sqlite, Dynamo,
  MongoDb and InMemory, plus Dapper and EF Core, which are *transaction providers*
  rather than stores. **Firestore and Spanner have no page**; both are named in
  `BoxProvisioning*.md` and `BrighterOutboxSupport.md`, so they are known to the corpus
  and unconfigurable from it.
- **Distributed locks**: 7 ship, 7 have pages. **Full coverage, and the only family
  with it** — which is also why five of the nine tables in §3.1 are lock pages. The
  locks are the worked precedent for what a good option table looks like here.
- **Schedulers**: 6 provider pages plus `CustomScheduler.md`. Coverage is complete;
  tables are absent.

### 3.6 Where the README disagrees with the source

Recorded so nobody reconciles them by hand a second time. The README is not wrong so
much as **stamped 2026-08-03**:

| README says | Measured at `10.7.0` |
|---|---|
| Transports: *"RabbitMQ, Kafka, AWS SNS/SQS, Azure Service Bus, Postgres, plus in-memory (7 surfaces)"* | **10 transports**, five of them undocumented (§3.3) |
| Outboxes: *"MSSQL, MySQL, Postgres, Sqlite, Dapper, EF Core, Dynamo, MongoDB"* | Adds **Firestore** and **Spanner**; Dapper and EF Core are transaction providers |
| Inboxes: *"MSSQL, MySQL, Postgres, Sqlite, Dynamo, MongoDB"* | Holds |
| Locks: *"DynamoDB, Postgres, MSSQL, MySQL, Azure Blob, MongoDB, Firestore"* | Holds — 7 of 7 |
| *"reading 565 lines"* for RabbitMQ options | `RabbitMQConfiguration.md` is **337** lines — Spec 010 split it. The longest configuration page is now `PostgreSQLMessageBroker.md` at **561** |
| Defaults are readable *"by reflection on an instantiated options object"* | True for 49 types, **false for 18** — see §2 |

---

## 4. Target state

When 012 closes:

1. **Every configuration surface a reader can set carries a table** with the four
   columns of §7.1, on the page that already owns that surface.
2. **`tools/optioncheck`** runs in CI and exits non-zero when a table and the assembly
   disagree — an option documented but gone, present but undocumented, or a default
   that differs.
3. **All ten shipping transports have a configuration page**, the five of §3.3 included.
   Decided at review 2026-08-27 — §13.1.
4. **The stale cross-cutting tables of §3.4 are corrected**, and brought under the
   checker where their content allows.
5. No option table restates *why* an option exists. That is explanation, and Spec 011's
   mode discipline sends it elsewhere.

---

## 5. `optioncheck` — the decision, taken 2026-08-27

The README left one question open and flagged it as spec-shaping. **Answered by the
maintainer: a C# tool, reflecting over restored NuGet packages for a pinned version.**

**Language: C#.** Reflection over .NET assemblies is native there. The alternative —
Python parsing C# source with a grammar — is fragile in exactly the place that matters:
§2's 270 defaulted constructor parameters are default *expressions*
(`AutoOffsetReset.Earliest`, `TimeSpan.FromMilliseconds(500)`), which a parser must
evaluate and reflection simply reads.

**Assembly source: restored NuGet packages, pinned.** Three reasons, and the third was
measured today:

1. It is what readers consume. A table describing tip-of-tree documents software nobody
   can install.
2. **It runs in CI.** A checker that needs a sibling checkout is a laptop-only gate —
   the exact failure 009's D9 hit when its version authority was
   `../Brighter/release_notes.md`, and the reason that authority moved to NuGet.
3. **The sibling checkout cannot be trusted as an authority anyway.** `../Brighter` is
   on another agent's branch (`spec/scoped-lifetime-per-pipeline`) at `4ebb49db2` —
   **173 commits past `10.7.0`** — with two further worktrees active. PROMPT records its
   HEAD moving three times across three sessions. Reflecting over it would document
   unreleased behaviour and would not be reproducible between two runs on the same day.

### 5.1 There are three default shapes, not two, and no single route reads all of them

The obvious design — walk `ParameterInfo.DefaultValue` for the constructor-driven types
and read properties off an instance for the rest — is **wrong**, and the source says so:

```csharp
// Subscription.cs:208  -- the parameter default
OnMissingChannel makeChannels = OnMissingChannel.Create,
TimeSpan? emptyChannelDelay = null,

// Subscription.cs:235-236  -- what the body actually assigns
MakeChannels = makeChannels;
EmptyChannelDelay = emptyChannelDelay ?? TimeSpan.FromMilliseconds(500);
```

`MakeChannels` defaults to `Create` in the parameter list. `EmptyChannelDelay` defaults
to **`null`** in the parameter list and to **500 ms** in the constructor body. A checker
reading `ParameterInfo.DefaultValue` would document the second as `null`, which is not
merely incomplete — it is **wrong**, and §14's note is that a wrong default is worse
than an absent one.

Measured across the six principal subscription types at `10.7.0`:

| Type | Defaulted params | …defaulting to `null` |
|---|---|---|
| `KafkaSubscription` | 27 | **15** |
| `PostgresSubscription` | 21 | **14** |
| `RmqSubscription` | 21 | **12** |
| `SqsSubscription` | 20 | **12** |
| `AzureServiceBusSubscription` | 15 | **9** |
| `Subscription` | 14 | **8** |
| **Total** | **118** | **70** |

**70 of 118 — 59% — default to `null` in the signature**, with the real value, where
there is one, applied by a `??` in the body.

So the three shapes are:

| Shape | Where the default lives | Read it by |
|---|---|---|
| Property initialiser | the property | instantiating and reading the property |
| Parameter default | the signature | `ParameterInfo.DefaultValue` |
| **Body coalesce** | `?? TimeSpan.FromMilliseconds(500)` | **instantiating and reading the property** — the signature says `null` |

### 5.2 What the decision therefore obliges

- **The checker reads both routes, and uses each for a different column.**
  `ParameterInfo` supplies the **`Option` name and `Type`** — and the name matters,
  because the parameter is `makeChannels` and the property is `MakeChannels`, so the
  spelling a reader types exists only on the parameter. **An instantiated object
  supplies the `Default`**, because that is the only route that survives shape three.
- It **must read constructor parameters as well as properties**, or it is blind to 265
  of 619 options. Neither route alone is sufficient, and the failure mode of picking one
  is a table full of confident `null`s.
- **Instantiation is not free**, and this is the residue §7.3 asks to size:
  `Subscription`'s constructor requires `subscriptionName`, `channelName` and
  `routingKey`, so the checker must synthesise arguments for every non-defaulted
  parameter before it can read a single default. Where it cannot, it must **say so per
  option** rather than emit a blank or a `null`.

> **This is inferred from the source, not yet measured by a running probe.** Reading
> `Subscription.cs:236` says `EmptyChannelDelay` comes back as 500 ms after
> construction; nothing here has instantiated the type and checked. **The first task of
> the implementation phase is that probe**, because the entire column design above rests
> on it — and *"reading the authority is not measuring it"* is this programme's own
> lesson, earned on this exact repository.
- It **pins its version explicitly**, and that pin is a claim with the same shelf life
  as a tutorial's. It belongs in `RELEASE_CHECKLIST.md` beside `versioncheck.py`'s.
- It follows the family contract `linkcheck.py` and `versioncheck.py` set: **exit 1 on
  a real finding, exit 2 when the authority is unreachable — which is not a pass** —
  and it **prints its scope before its verdict**, because `0 mismatches` of 0 tables
  and of 619 options must not print the same line.
- **It gates the build.** Decided at review 2026-08-27 (§13.3): exit 1 fails CI, the
  same as every other gate in this repository. §13.3 records why the objection that
  prompted the question does not apply.

**Still open, and it is a scoping question rather than a blocker:** which defaults the
checker declares itself unable to determine. Defaults applied later by a builder, or
computed at construction, are not reachable by either route. §7.3 scopes the tables to
what the checker can verify and marks the rest **manually verified** rather than letting
them fail silently — but the size of that residue is not known until the tool runs, and
the first phase should measure it before the tables are designed around it.

---

## 6. Target audience

**Intermediate to advanced.** A reader consulting an option table has already chosen
Brighter, chosen a transport, and hit a specific question. They do not need the concept
explained; they need the default, the type, and the constraint.

That is a deliberate contrast with Spec 009, whose ladder assumes nothing. A table that
tries to teach fails both readers: it is too slow to scan and too shallow to learn from.
**Link to the explanation; do not inline it.**

The second audience is a retrieval client. §7.1's shape is chosen partly because a
four-column row survives chunking as a self-contained fact, where the same content
written as a paragraph does not.

---

## 7. Scope

### 7.1 The table format — P0, and it gates everything else

One table per configuration surface, four columns, in this order:

| Option | Type | Default | Description |
|---|---|---|---|
| `MakeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates the queue or topic if absent, assumes it exists, or validates it. |
| `BufferSize` | `int` | `1` | Messages prefetched per read. Raise for throughput; lowers ordering guarantees. |

Rules, each of which the checker can enforce or deliberately cannot:

- **`Option`** is the member name exactly as a reader writes it, in backticks. For a
  constructor-driven type that is the **parameter** name, which is camel-case
  (`bufferSize`) where the property is Pascal (`BufferSize`). **The table gives the
  spelling the reader types**, and where the two differ the row says so. This is a real
  hazard: §2's 18 types are configured by one spelling and read by the other.
- **`Type`** is the declared type, nullable annotation included.
- **`Default`** is the literal default, in backticks, or **`none`** where there is
  genuinely no default. Never blank — a blank cell is indistinguishable from an
  unfinished row.
- **`Description`** is one sentence, present tense, no rationale.

### 7.2 P0 — must have

1. **The table format above, agreed at review**, since every later task depends on it.
2. **`tools/optioncheck`**, built early rather than last. The tables are 619 rows; the
   drift starts on the day the first one lands, not on the day the last one does.
3. **Core surfaces** — `CommandProcessorConfigurationReference.md` and
   `DispatcherConfigurationReference.md`. Highest traffic, already the right pages, and
   additive work (§3.2).
4. **The five documented transports** — RabbitMQ, Kafka, AWS SNS/SQS, Azure Service
   Bus, PostgreSQL. Subscription, publication and connection tables per transport.
5. **The five undocumented transports** (§3.3) — GCP Pub/Sub, RocketMQ, MQTT, Redis and
   MSSQL. **A page each, with tables.** Decided at review 2026-08-27; see §7.2.1 for
   what that costs.

#### 7.2.1 What commissioning the five costs, measured

**It does not add a single option.** `survey.py` walks all of `src/`, so those types
were inside the 619 from the first run — what the decision changes is the number of
*pages*, not the size of the surface. The five carry **151 of the 619 options, 24%**,
and the largest of them is larger than any transport now documented:

| Transport | Surfaces | Options |
|---|---|---|
| GCP Pub/Sub | `GcpPubSubSubscription` (33 ctor params), `GcpMessagingGatewayConnection` (7), `GcpPublication` (3) | **43** |
| Redis | `RedisSubscription` (19), `RedisMessagingGatewayConfiguration` (14) | **33** |
| RocketMQ | `RocketMqSubscription` (22), `RocketMessagingGatewayConnection` (4), `RocketMqPublication` (3) | **29** |
| MQTT | `MqttSubscription` (19), `MQTTMessagingGatewayConfiguration` (8) | **27** |
| MSSQL | `MsSqlSubscription` (19) | **19** |
| **Total** | | **151** |

`GcpPubSubSubscription`'s 33 constructor parameters make it the **widest subscription
in the codebase** — wider than `KafkaSubscription`'s 30 — with 30 defaulted and **19 of
those defaulting to `null`**, which is §5.1's third shape at its worst. The transport
with no corpus presence at all is the one whose table the checker is most needed for.

**Five new pages is five banners, five opening sentences, five `SUMMARY.md` entries and
five orphan-check passes**, and each page needs prose around its tables — a bare table
under an H1 is not a page. §10 is revised accordingly. **What none of them needs is a
tutorial or a running broker**: these are `Reference` pages describing an options
surface, and §8's exclusion of explanatory content applies to them as it does to the
five that already exist.

### 7.3 P1 — should have

6. **Outboxes and inboxes** — the eight outbox stores and six inbox stores that have
   pages, **plus new pages for Firestore and Spanner**, decided at review 2026-08-27
   under the same rule as the transports (§13.2). **The two are not the same job**, and
   the difference was measured rather than assumed:
   - **Firestore has a configuration type** — `FirestoreConfiguration`, 7 settable
     properties — so its page has a table of its own.
   - **Spanner has none.** At `10.7.0` it ships an outbox, an inbox, provisioning and a
     connection provider, and **no `*Configuration` type anywhere in
     `src/Paramore.Brighter*.Spanner*`**: `SpannerOutbox` takes
     `IAmARelationalDatabaseConfiguration` (`SpannerOutbox.cs:32`), the shared relational
     config. So Spanner's page **links** the relational options table rather than
     restating it, and carries what is genuinely Spanner-specific — provisioning and
     the connection provider. **Do not write it a table it does not have**; a duplicated
     table is the drift this spec exists to stop, authored on purpose.

   > **The same rule reaches two more pages, ruled at design review 2026-08-27.** The
   > inbox family ships **eight** stores at `10.7.0` and has seven pages: **Firestore and
   > Spanner inboxes have none either.** This section is right about the outbox family and
   > was silent about the inbox one, and §3.6 records the README's six-store inbox list as
   > *"Holds"* — against six stores it did. **D16** closes it, on this item's own rule.
7. **Distributed locks** — 7 of 7 have pages and five already have tables; this is
   normalisation to the §7.1 format rather than new work.
8. **Schedulers** — 6 providers plus custom.
9. **Correcting the stale cross-cutting tables of §3.4.**
10. **The checker's residue measured** — how many of the 619 it cannot verify, named
    option by option rather than totalled. §5.1 establishes the residue is real and
    §5.2 that it is a *construction* problem, not a reflection one; what is unknown is
    its size beyond the six types measured here.

### 7.4 P2 — nice to have

11. **Middleware attributes** — the `UsePolicy` / `UseInbox` / `RequestLogging` family:
    parameters, ordering semantics, defaults. Genuinely useful and genuinely harder,
    because attribute ordering is not a property of any one type.
12. **A configuration index page**, listing every surface and linking its table.
13. **Archive providers and luggage stores** — `S3LuggageOptions`,
    `AzureBlobLuggageOptions`, `GcsLuggageOptions`, `MongoDbLuggageStoreOptions`,
    `AzureBlobArchiveProviderOptions` — 38 options by §2's count, on pages that mostly
    exist.

---

## 8. Out of scope

- **Generated API reference.** Recommend it separately to the Brighter repository; do
  not attempt it here. It is built from source and belongs beside the source.
- **Explanatory content about why an option exists.** Spec 011's mode discipline sends
  that to the explanation pages. A table row that argues is a table row nobody scans.
- **Changing any default.** 012 documents Brighter; it does not design it. Where a
  default looks wrong, raise it on the Brighter tracker as 009 did four times.
- **The `.V4` package generations** as separate surfaces. `AWSSQS.V4`,
  `Outbox.DynamoDB.V4`, `Locking.DynamoDB.V4` and `Transformers.AWS.V4` mirror their
  siblings for a different AWS SDK major. One table per surface, with the difference
  noted in prose where one exists.
> **This list is one shorter than the draft reviewed on 2026-08-27.** It carried
> *"writing the five missing transport pages, unless §7.2 item 5 decides otherwise at
> review"*. **Review decided otherwise** (§13.1), so the five are now P0 work in §7.2
> and Firestore and Spanner are P1 work in §7.3. The bullet is recorded here rather
> than deleted, because an out-of-scope list that silently loses an entry is how a
> scope decision becomes invisible.

---

## 9. Deliverables

| D | File | Note |
|---|---|---|
| D1 | `tools/optioncheck/` | The checker. C#, `dotnet run`, restored packages pinned to a version. Family contract per §5 |
| D2 | `.github/workflows/docs.yml` (edit) | A third job, or a step in `versions`. Unguarded, per the lesson that a guard outliving its tool un-gates the check |
| D3 | `RELEASE_CHECKLIST.md` (edit) | The checker's package pin joins `versioncheck.py`'s |
| D4 | `contents/CommandProcessorConfigurationReference.md` (edit) | Tables per §7.1 |
| D5 | `contents/DispatcherConfigurationReference.md` (edit) | Tables per §7.1 |
| D6 | Five transport pages (edit) | RabbitMQ, Kafka, AWS SNS/SQS, Azure Service Bus, PostgreSQL |
| D7 | Outbox and inbox pages (edit) | P1 |
| D8 | Distributed lock pages (edit) | P1 — normalisation; five already have tables |
| D9 | Scheduler pages (edit) | P1 |
| D10 | `contents/ReactorAndProactor.md`, `contents/HandlerFailure.md` (edit) | The §3.4 corrections |
| D11 | `spec/012-configuration_reference/survey.py` | **Already built.** The source survey behind §2 and §3 |
| D12 | Five **new** transport pages | GCP Pub/Sub, RocketMQ, MQTT, Redis, MSSQL. P0 per §7.2 item 5; 151 options between them, sized in §7.2.1 |
| D13 | Two **new** outbox pages | Firestore (own config type) and Spanner (**no config type** — links the relational table). P1 per §7.3 item 6 |
| D14 | `SUMMARY.md` (edit) | ~~Seven~~ **ten** entries — D12, D13, and D15/D16 below. §10 |
| D15 | `contents/RelationalDatabaseConfigurationReference.md` | **Added at design review 2026-08-27.** The 8 relational options, documented once instead of thirteen times: **seventeen components across four families** take `RelationalDatabaseConfiguration`, including the MSSQL *and* PostgreSQL transports. `design.md` §8.4 |
| D16 | Two **new** inbox pages | **Added at design review 2026-08-27.** Firestore and Spanner inboxes ship at `10.7.0` and have no page — the same gap §13.2 closed on the outbox side. Neither introduces a configuration type, so it is two pages and **zero options**. `design.md` §12.6 |

**D11 exists as of this document.** It is a *source* survey, not the checker, and says
so in its own docstring — it sizes the work and records the two shapes. D1 is the tool
that gates CI.

**D12–D14 are the review of 2026-08-27**, and they are the only deliverables here that
create pages rather than edit them. **Do not read the D-count as the task count**: D12
is five pages and D13 is two, so fourteen rows describe twenty-one artefacts. 009's
PROMPT block on deliverable counts is the precedent — *the row count is the authority
for rows, and it is not a count of anything else.*

---

## 10. `SUMMARY.md` changes

**Revised at review 2026-08-27.** The draft of this section said *"most of 012 adds no
page and therefore no entry"* and made entries conditional on §7.2 item 5. **Item 5
commissioned the pages**, so seven entries are now certain:

| Entry | Section | From |
|---|---|---|
| GCP Pub/Sub Configuration | *Transports* | D12 |
| RocketMQ Configuration | *Transports* | D12 |
| MQTT Configuration | *Transports* | D12 |
| Redis Configuration | *Transports* | D12 |
| MSSQL Message Broker | *Transports* | D12 |
| Firestore Outbox | *Outbox* | D13 |
| Spanner Outbox | *Outbox* | D13 |
| **Relational Database Configuration Reference** | *Brighter Configuration* | **D15** |
| **Firestore Inbox** | *Inbox* | **D16** |
| **Spanner Inbox** | *Inbox* | **D16** |

**Three more were added at design review 2026-08-27**, taking it to ten. An eleventh joins
*Brighter Configuration* if P2 item 12's index page is built — **and it cannot join
*Transports***, which the five transport entries take to exactly twelve top-level entries,
S2's ceiling. Measured in `design.md` §9.2 before the diff was written.

**Five of the ten are nested and five are top-level**, which is what keeps the ceiling
above from getting worse: the four store pages nest under *Outbox Support* and *Inbox
Support*, and D15 nests under *Basic Configuration* beside the two Reference pages it
joins.

**The rest of the spec still adds no entry**, and that remains what makes 619 rows
affordable: the tables land on pages already in the tree.

**No entry moves a URL.** Slugs are filename-derived, so re-ordering inside a section
moves nothing — §2.1 of 009's tasks has held on that point five times, and adding to a
section is the same operation.

**`linkcheck.py`'s orphan check is what enforces the entry**, and it runs on every PR.
Seven new pages is seven chances to forget, which is precisely the case the check
exists for.

> **The title in `SUMMARY.md` is what reaches `/llms.txt`, not the H1**, and the two
> disagree on 32 pages today. Choose the seven entry texts deliberately: they are the
> titles a retrieval client sees. `CLAUDE.md`'s *llms.txt* section is the authority.

---

## 11. Constraints

- **`CLAUDE.md` is the authority**, and `pagelint.py` enforces most of it. Every edited
  page keeps its banner, its qualified `##` headings and its opening sentence.
- **Rule 6 turns strict on any code block your diff touches.** A page gaining a table
  beside an existing C# block may pull that block into `--changed` scope. Budget for it;
  the remedy is real `using` directives or a declared `// ...`.
- **The table is verified against the assembly, never against our own prose.**
  Transcribing existing documentation propagates whatever drift is already there — and
  §3.4 establishes that there is some.
- **Cite anchors, never line numbers**, in anything durable. This document cites
  `ReactorAndProactor.md:284` deliberately and knows it will rot; the durable anchors
  are **`#transport-native-support`** and **`#transport-nack-behavior`**, and the line
  numbers are there to be checked once.

  > A draft of this line asserted the anchor was `#transport-support-matrix`. There is
  > no such heading — it was invented, in the sentence instructing everyone else to cite
  > anchors. It cost one `grep -n '^#'` to find, which is the whole point.
- **Terminology**: "Dispatcher", not "ServiceActivator" or "Service Activator", in
  prose. Rule 5 is an error, not a warning.
- **Do not modify `../Brighter`.** 012's only source need is *read* access at a tag, and
  `git show <ref>:<path>` satisfies it without touching a working tree that belongs to
  another agent.

---

## 12. Acceptance criteria

| AC | Criterion | How it is checked |
|---|---|---|
| AC1 | Every P0 surface carries a table in the §7.1 format | Walked page by page at the acceptance pass |
| AC2 | Every documented default matches the assembly | `optioncheck` exit 0 |
| AC3 | `optioncheck` reads constructor parameters as well as properties | A red-proof that a changed ctor default is caught — **not** merely that the tool runs |
| AC3b | It reports a body-coalesced default as its real value, never as `null` | A red-proof on `EmptyChannelDelay`, whose signature says `null` and whose value is 500 ms (§5.1) |
| AC4 | `optioncheck` runs in CI, unguarded, on push and PR ~~and on a schedule~~ | `.github/workflows/docs.yml`, and a green run naming the job |
| AC5 | Exit 2 (authority unreachable) is distinguishable from exit 0 | Red-proof with the package source removed |
| AC6 | All ten shipping transports have a configuration page, and each of the five new ones carries its tables | Walked page by page; `linkcheck.py` reports no orphan and `pagelint.py` no error on any of the five |
| AC6b | Firestore has an outbox page with a table; Spanner has one **without** | Spanner's links the relational options table rather than restating it (§7.3 item 6) |
| AC7 | The §3.4 stale tables match the shipped transport set | Diffed against `survey.py` output at the release ref |
| AC8 | No table restates rationale | Review; there is no tool for this and §12's note says so |
| AC9 | The six existing gates stay green | The six commands in PROMPT's *state* block |

> **AC4's schedule clause was struck at design review, 2026-08-27**, and is struck rather
> than deleted so a reader finds the ruling instead of an absence. The reason is §13.3
> above, one section down: `optioncheck` reflects over a **pinned** package, so a
> scheduled run cannot change its answer without a commit here — it can only repeat the
> last PR verdict, or exit **2** because NuGet was briefly unreachable. That is the
> failure mode `urlmap.py --verify` is deliberately kept out of CI for.
> **This document asked for the schedule two sections after explaining why the pin makes
> one pointless**, which is the tell nobody looked for at requirements review. Full
> reasoning in `design.md` §6.5 and §13.2.

**AC3 and AC8 are the two worth naming here.** AC3 is the criterion §2 exists to
create — a checker that passes while blind to 43% of the surface is the vacuous green
this programme has met repeatedly, and *"prove a new gate fails before trusting it to
pass"* is the standing remedy. AC8 has **no tool behind it**, deliberately, because
whether a sentence argues or describes is a judgement; 009's AC7 was the criterion with
no tool and it was the one that turned out to be unmet at the close, so this one gets
walked early rather than at the acceptance pass.

---

## 13. Questions for the maintainer — ALL THREE ANSWERED 2026-08-27

**Answered, applied throughout, and kept rather than deleted**, because each answer
changed a section and a reader of that section deserves to find the ruling behind it.
The questions are struck; the answers are not.

### 13.1 ~~The five undocumented transports — document, or declare unsupported?~~

**Answered: document all five.** GCP Pub/Sub, RocketMQ, MQTT, Redis and MSSQL each get
a configuration page in 012. This was the option that grows the spec most, and it was
taken over the two cheaper ones — correcting only the comparison tables, or covering
only the three transports the corpus already makes claims about.

**It is the largest of the three questions and it is the one that changes the size of
the spec**, as the draft predicted. What the draft did not know, and §7.2.1 now
measures, is the shape of the cost: **the option count does not move at all** — those
types were always inside the 619 — while the page count goes up by five, and one of
those five (`GcpPubSubSubscription`, 33 constructor parameters) is the widest
subscription Brighter ships.

The reading that supports it: *declare unsupported* was never really available. All ten
transports ship, and `ReactorAndProactor.md` already tells a reader that Redis supports
both APIs natively. A documentation set cannot un-assert that by omission — it can only
leave the reader at the dead end §3.3 describes.

### 13.2 ~~Firestore and Spanner outboxes — same question, smaller?~~

**Answered: same rule as the transports — both get pages.** §7.3 item 6 carries it, and
records the one way the two differ, which was measured rather than assumed: **Firestore
has a `FirestoreConfiguration` and Spanner has no configuration type at all.** Spanner's
page therefore links the shared relational options table instead of restating it.

### 13.3 ~~Does the checker gate the build, or report?~~

**Answered: it gates**, exit 1 failing CI like every other gate here. §5.2 carries it.

**The objection in the original question does not apply, and the reason is worth
keeping.** The draft worried that *"`optioncheck` failing on the day Brighter ships a
new default would redden every PR until someone bumps the pin"*. It cannot: §5 pins the
package, so the checker reflects over `10.7.0` until a human changes that pin. **Brighter
shipping 10.8.0 does not move it.** The red arrives at the pin bump — a deliberate
`RELEASE_CHECKLIST.md` step (D3), performed by someone who is at that moment looking for
exactly this information.

**This is the opposite of `versioncheck.py`, and the two should not be reasoned about
together.** `versioncheck.py` resolves the *latest* version from NuGet, so it is
designed to go red on its own when Brighter ships — that failure is its signal.
`optioncheck` resolves a *pinned* version, so it never goes red on its own. Same family
contract, opposite trigger.

**Not open, and recorded so it is not re-raised:** the checker's language and assembly
source. Answered 2026-08-27 — **C#, restored NuGet packages, pinned** — with the
reasoning in §5.

---

## 14. Notes

- **Verify every default against the assembly.** A wrong default in a reference table is
  worse than an absent one, because the reader has no reason to doubt it. This is the
  README's closing note and it survives unchanged.
- **The volume parallelises well and the review does not.** Each surface is independent,
  so tasks fan out cleanly; 619 rows of review load is the real constraint, and it is an
  argument for the checker landing before the bulk of the tables rather than after.
- **The 2026-08-27 review grew the spec by seven pages and by zero options.** Worth
  stating in those terms, because the two costs are unrelated and the second is the one
  a reader would expect to move. The added load is authoring — banners, opening
  sentences, `SUMMARY.md` entries, prose around the tables — not surveying. **Design
  should sequence the five transport pages after the checker exists**, so their tables
  are the first written under a gate rather than the last brought under one.
- **This spec's own numbers are stamped `10.7.0`.** When Brighter ships 10.8.0 they are
  claims about a previous release. Re-run `survey.py --ref 10.8.0` rather than reasoning
  from the table in §2 — *a total needs a ref, not just a derivation*, and this document
  is not exempt from the rule it quotes.
