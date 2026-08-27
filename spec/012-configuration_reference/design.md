# Design: Spec 012 — Configuration Reference Tables

**Created:** 2026-08-27
**Status:** **Approved 2026-08-27.** Its four §13 questions were answered by the
maintainer the same day and applied throughout; two of them grew the spec by three pages
and no options, and one amended requirements §12's AC4.
**Requirements:** `spec/012-configuration_reference/requirements.md`, approved 2026-08-27
and merged as `9df5e89`. It supersedes the README on every point of fact. This design
supersedes nothing — where it measures something the requirements asserted, it says so
in the sentence that does it, and §12 collects those.

**Everything numeric here is stamped Brighter `10.7.0` (tag `c1b8af886`)** and was read
with `git show <ref>:<path>`, so the sibling working tree — another agent's branch, 173
commits past the release — was never touched. Re-derive with
`python3 spec/012-configuration_reference/survey.py --ref <tag>`.

**An option count in this document is a floor, not a total.** §12.1 and §12.5 measure
two families of surface `survey.py` cannot see; §2.1 explains why that is a property of
the instrument rather than a defect in the requirements, and §14 phase 1 fixes it before
any table is written.

---

## 1. What this design decides

The requirements settled *what* 012 builds: tables in the §7.1 format, a checker that
gates, ten transport pages, and Firestore and Spanner outbox pages. Six things were left
to design, and this document answers them:

| # | Decision | Where |
|---|---|---|
| 1 | How a table in markdown is **bound** to a .NET type, so a tool can check it | §5 |
| 2 | How `optioncheck` reads **both** default routes without the packages fighting each other | §6 |
| 3 | Which surface lands on **which page** — all of them, each exactly once | §7 |
| 4 | The shape of each **new** page, which is not uniform and cannot be templated | §8 |
| 5 | Where the **shared relational options table** lives, given seventeen components take it | §8.4 |
| 6 | The **order** the work goes in, with the checker before the tables §14 asks for | §14 |

**Its four review questions were answered on 2026-08-27, the day they were asked** (§13),
and two of them grew the spec: `RelationalDatabaseConfigurationReference.md` is
commissioned as D15, and the Firestore and Spanner **inbox** pages as D16.
**Three more pages and zero more options** — every option on the three is a link to a
table somewhere else. AC4's schedule clause is dropped, amending requirements §12.

It also records six measurements taken while designing, each of which changes what
someone would do. They are collected in §12; four of them qualify a figure the
requirements state correctly, and two find work the requirements do not list:

- **Schedulers have no configuration type at all.** D9's six providers are configured
  through *factory* properties, which `survey.py` cannot see — 25 options behind a P1
  deliverable that no figure in the requirements counts (§12.1).
- **Thirteen surfaces use a C# primary constructor**, which `survey.py`'s
  `widest_ctor` also cannot see — 40 parameters, and two whole types missing from the 67
  (§12.5). So **619 is a floor.**
- **Only one of the corpus's 44 documented option rows is already in the §7.1 shape**,
  and two of the twelve tables are not option tables at all (§12.2).
- **Adding the five transport pages took *Transports* to exactly 12 top-level entries,
  which was S2's ceiling — so S2 was measured, and moved to 20** (§9.2). It is the second
  rule in this programme to move, and for the same reason S3 did.
- **MSSQL-as-a-transport is a second Spanner case** — it takes the shared relational
  configuration and has no connection type of its own (§8.3).
- **The inbox family ships Firestore and Spanner stores with no page**, exactly mirroring
  the outbox gap the maintainer's §13.2 ruling commissioned two pages for (§12.6).

---

## 2. The surface, re-derived

`survey.py --ref 10.7.0`, run 2026-08-27 while writing this document, reproduces every
figure in requirements §2 exactly:

```text
67 configuration types at 10.7.0
   18 are constructor-driven  (317 params, 270 defaulted)
   49 are property-driven     (302 settable properties)

TOTAL reader-facing options: 619

A property-only reflection pass would see 354 of them and miss 265.

Of 290 defaulted constructor parameters across ALL 67 types, 173 (60%)
default to `null`.
```

**A type's option count is `max(properties, constructor parameters)`, not their sum**
(`survey.py:143`), because on a constructor-driven type the two are largely the same
members reached by two spellings. `KafkaSubscription` contributes **30**, not 47. Every
per-page figure in §7 uses that convention, so the totals add up to the same 619.

**The two populations in the last paragraph above are not the same quantity, and the
script says so in its own output** — 270 counts the 18 constructor-driven types, 290
counts defaulted constructor parameters wherever they occur. Quote whichever you mean
and name which it is.

### 2.1 What 619 counts, and therefore what it does not

`survey.py` selects files whose **name** ends
`Subscription|Publication|Configuration|Options|Connection`, and reads constructors
written in the classic `public TypeName(...)` form. That is the right instrument for
sizing a spec and it was never claimed to be more — requirements §2 introduces it as a
count of *configuration types*, and §14 says to re-run it rather than reason from its
table.

Two consequences design has to absorb, both measured in §12:

- **A surface that does not advertise itself in a filename is invisible** — the six
  scheduler factories (§12.1).
- **A surface declared with a C# primary constructor is invisible or under-counted** —
  thirteen types, 40 parameters, two of them absent from the 67 entirely (§12.5).

`optioncheck` therefore does **not** inherit that selector. It reflects over whatever
type a table names (§5), so its scope is the union of the tables 012 writes.

---

## 3. Documentation structure

### 3.1 File hierarchy

**Ten pages are created** and one tool; everything else edits a page that already exists.
`+` is new, `~` is edited, and the option counts are §7's.

```text
tools/
+ optioncheck/                              D1  — C# console, the gate
+   optioncheck.csproj                          pinned PackageReferences, one per surface package
+   Program.cs                                  scope print, verdict, exit 0/1/2
+   Binding.cs                                  parses the <!-- optioncheck: --> markers
+   Reflect.cs                                  the two routes of §6.2
+   Synthesise.cs                               constructor arguments — 20 of 24 types are strings
~ .github/workflows/docs.yml                D2  — a third job
~ RELEASE_CHECKLIST.md                      D3  — the pin joins versioncheck.py's

contents/
~ CommandProcessorConfigurationReference.md D4  — 47 options, 5 tables
~ DispatcherConfigurationReference.md       D5  — 26 options, 3 tables
+ RelationalDatabaseConfigurationReference.md D15 — 8 options, 1 table   (§8.4, NEW in design)

  the five documented transports                D6  — 186 options
~ RabbitMQConfiguration.md                      44 options, 3 tables
~ KafkaConfiguration.md                         58 options, 3 tables
~ AWSSQSConfiguration.md                        31 options, 3 tables
~ AzureServiceBusConfiguration.md               26 options, 3 tables
~ PostgreSQLMessageBroker.md                    27 options, 3 tables  (3 existing replaced)
~ InMemoryTransport.md                          2 options + §12.5, 1 table

  the five NEW transport pages                  D12 — 151 options
+ GcpPubSubConfiguration.md                     43 options, 3 tables
+ RedisConfiguration.md                         33 options, 2 tables
+ RocketMQConfiguration.md                      29 options, 3 tables
+ MQTTConfiguration.md                          27 options, 2 tables
+ MSSQLMessageBroker.md                         19 options, 1 table + a link (§8.3)

  the two NEW outbox pages                      D13
+ FirestoreOutbox.md                            7 options, 1 table
+ SpannerOutbox.md                              0 options, 0 tables + a link (§8.5)

  the two NEW inbox pages                       D16 — §13.4, zero options
+ FirestoreInbox.md                             0 options; links FirestoreOutbox.md's table
+ SpannerInbox.md                               0 options; links the relational table

~ outbox / inbox pages                      D7  — 8 pages, 35 options
~ distributed lock pages                    D8  — 7 pages, 8 own options + 3 links
~ scheduler pages                           D9  — 6 pages, 25 options   (§12.1)
~ ReactorAndProactor.md, HandlerFailure.md  D10 — the §3.4 corrections
~ SUMMARY.md                                D14 — ten entries (§9)
```

### 3.2 Reading order

012 does not create a path a reader walks; it thickens pages a reader arrives at from a
search engine or from `/llms.txt`. The one ordering that matters is **where a reader goes
when the table is not on the page they are on**, and it has three rungs:

1. **The base tables come first in the tree.** `Subscription`'s 17 options (D5) and
   `Publication`'s 8 (D4) are inherited by every transport. A transport table lists what
   the transport *adds or overrides* and links up — so Kafka's page carries
   `KafkaSubscription`'s 30 and points at the base rather than restating it.
2. **A shared configuration type is documented once and linked.** §8.4's relational table
   serves seventeen components; `MongoDbConfiguration` serves the Mongo outbox, inbox and
   lock; `FirestoreConfiguration` serves the Firestore outbox, inbox and lock.
3. **Rationale stays on the explanation page it is already on.** §7.1's fourth column is
   one descriptive sentence; the *why* is a link, per requirements §8.

---

## 4. The table as it will ship

Requirements §7.1 fixes four columns in one order. Made concrete, with §5's marker above
it:

```markdown
<!-- optioncheck: Paramore.Brighter.MessagingGateway.Kafka.KafkaSubscription -->

| Option | Type | Default | Description |
|---|---|---|---|
| `bufferSize` | `int` | `1` | Messages prefetched per read. |
| `commitBatchSize` | `int` | `10` | Offsets committed in one batch. |
| `offsetDefault` | `AutoOffsetReset` | `Earliest` | Where a new consumer group starts. |
```

Four rules the checker enforces, and one it deliberately cannot:

- **`Option` is the spelling the reader types.** On a constructor-driven type that is the
  **parameter** — `bufferSize`, not `BufferSize`. Requirements §7.1 calls this a real
  hazard and it is: on those types the reader *sets* one spelling and *reads back* the
  other. Where the two differ by more than case, the description says so.
- **`Type`** is the declared type with its nullable annotation: `TimeSpan?`, never
  `TimeSpan`.
- **`Default`** is the value **after construction**, never the parameter default where
  the two differ (§6.2). Where there genuinely is none, the cell reads `none`. Never
  blank — a blank cell is indistinguishable from an unfinished row.
- **`Description`** is one sentence, present tense, no rationale.
- **Not checkable, and named here so review knows to look:** whether that sentence
  describes or argues. That is AC8, it has no tool, and requirements §12 says to walk it
  early rather than at the acceptance pass.

---

## 5. Binding a table to a type

**The problem:** a checker reading markdown has no way to know which .NET type a table
describes. Inferring it from the heading — `## Kafka Subscription` → `KafkaSubscription`
— works until it does not, and it fails as *a table nobody checked, reported as a table
that passed*.

**The decision: an HTML comment immediately above the table names the type.**

```markdown
<!-- optioncheck: Paramore.Brighter.MessagingGateway.Kafka.KafkaSubscription -->
```

Three reasons, and the third is why this is safe rather than merely convenient:

1. **It is invisible to readers.** GitBook renders markdown; an HTML comment produces
   nothing on the page and nothing in the `.md` variant a retrieval client reads.
2. **The precedent is already in this corpus.** `<!-- pagelint: allow-serviceactivator -->`
   sits on six pages and has survived publication since Spec 011 — so this is an
   established mechanism here, not a fresh bet on GitBook's renderer.
3. **It puts the scope list beside the thing it scopes.** An explicit inclusion list is a
   promise made forward in time to files nobody has created — the shape 009's
   `TUTORIAL_PAGES` had, and the standing lesson is that *a checker's inclusion list is
   where its unstated obligations live*. Here the list lives on the table it governs, so
   list and obligation cannot drift apart.

### 5.1 The marker's three keys

```markdown
<!-- optioncheck: Paramore.Brighter.MessagingGateway.Kafka.KafkaSubscription
     omit: channelFactory — not reader-set; supplied by AddConsumers
     manual: sweepUncommittedOffsetsInterval — default applied by the Dispatcher, not the type
-->
```

| Key | Meaning | What the checker does |
|---|---|---|
| *(the type)* | Assembly-qualified type name. Required | Reflects over it; every reader-facing member must appear as a row |
| `omit:` | Members deliberately absent from the table, each with a reason | Suppresses *"present but undocumented"* for those — **and counts them** |
| `manual:` | Members whose `Default` the tool cannot determine | Checks name and type; skips the default — **and counts it** |

**Both escapes declare rather than silence, and both are counted.** That is
`pagelint.py` rule 6's `// ...` applied to a second tool: it downgrades, never silences,
and the count keeps the debt visible in the run's own output. A silent exemption would
let 012 reach a green build by writing `omit:` over the hard half of every table.

**`manual:` is how requirements §7.3 item 10 gets measured rather than estimated.** The
residue is not a number someone guesses at the close; it is the sum of the `manual:`
declarations, printed per option and by name in the scope line.

### 5.2 What the marker does not carry

Not the version, not the package, not expected values. The version is pinned once in
`optioncheck.csproj` (§6.4); duplicating it per table would be a hundred pins to bump
instead of one, which is the failure `APPLIES_TO` exists to prevent on banners.

---

## 6. D1 — `tools/optioncheck`

C#, `dotnet run`, reflecting over restored NuGet packages pinned to a version.
Decided in requirements §5; designed here.

### 6.1 What it does, in order

```text
1. restore the pinned packages          → exit 2 if the feed is unreachable
2. scan contents/*.md for markers       → the scope: N tables, M rows, P types
3. for each marker, load the type       → exit 1 if the type is gone
4. enumerate reader-facing members      → settable properties; widest ctor's parameters
5. read each member's default (§6.2)
6. diff against the table's rows
7. print the scope, then the verdict    → exit 0 or 1
```

**It prints its scope before its verdict**, per the family contract `versioncheck.py`
set: `0 mismatches` across 0 tables and across 619 must not print the same line. The
scope names tables, rows, types, and the `omit:` and `manual:` counts.

### 6.2 The two routes, and which column each supplies

Requirements §5.1 establishes three default shapes and §5.2 that neither route alone
reads them all:

| Column | Route | Why that one |
|---|---|---|
| `Option` (name) | `ParameterInfo` / `PropertyInfo` | The parameter carries the spelling the reader types; the property does not |
| `Type` | `ParameterInfo` / `PropertyInfo` | Same metadata, no instance required |
| `Default` | **an instantiated object, read back** | The only route surviving shape three — a signature saying `null` where the body assigns 500 ms |

**The `Default` column comes from an instance always, including where the parameter
default would have been right.** Choosing per parameter would make the tool's
correctness depend on a judgement about which shape a parameter is in — and shape three
is precisely the case that *looks like* shape two. One route for one column.

### 6.3 The argument-synthesis burden — measured

To read a default off an instance the tool must construct one, and `Subscription`'s
constructor requires `subscriptionName`, `channelName` and `routingKey`. Requirements
§5.2 flags this as the residue §7.3 asks to size. Sized here from the source at
`10.7.0`, using `survey.py`'s own parser so it is the same instrument the 619 came from:

**28 types have a public constructor taking parameters and 57 of those parameters carry
no default.** Excluding the four `.V4` duplicates requirements §8 puts out of scope as
separate surfaces: **24 types, 48 required parameters** — and they are strikingly
regular.

| Shape | Types | Required arguments | Params |
|---|---|---|---|
| Subscription | 11 | `subscriptionName`, `channelName`, `routingKey` | 33 |
| SQS subscription | 1 | those three plus `channelType` | 4 |
| One scalar | 3 | `channelName`; `connectionString` ×2 | 3 |
| **Objects the tool must build** | **4** | `MongoDbConfiguration(client, …)`, `HandlerConfiguration(subscriberRegistry, handlerFactory)`, `AWSS3Connection(credentials, region)`, `S3LuggageOptions(connection, …)` | 8 |
| *(all parameters defaulted)* | 5 | none | 0 |
| **Total** | **24** | | **48** |

So **a synthesiser that knows strings, enums and the three subscription arguments covers
20 of the 24 types**. Four need a hand-written factory or a `manual:` declaration; three
of those are P2 luggage-store types, and the fourth — `HandlerConfiguration` — is P0, on
D4.

**This is inferred from source and is not a measurement of the running tool.** §14 phase
1's first task is requirements §5.2's probe — instantiate `Subscription`, assert
`EmptyChannelDelay` comes back as 500 ms — and its second is re-deriving this table by
construction rather than by parsing. *Reading the authority is not measuring it.*

### 6.4 Loading the packages without them fighting

One project referencing every Brighter surface package puts several third-party SDKs in
one process. **The risk is real and it lands entirely on the instantiation route:**
metadata reflection never runs a static constructor, while instantiating a type resolves
its dependencies for real.

Cheap mitigation first, expensive one recorded as a contingency:

- **Pin one generation only.** Requirements §8 puts the `.V4` packages out of scope as
  separate surfaces, and they are exactly the pairs that would put two AWS SDK majors in
  one process. Not referencing them removes the largest known conflict at no cost to
  coverage.
- **If a conflict appears anyway, isolate by process, not by `AssemblyLoadContext`.** A
  load context does not isolate native dependencies, and several of these packages have
  them. One `dotnet run` per package family, results concatenated, is the fallback.
- **Do not reach for `MetadataLoadContext`.** It would solve loading and cannot
  instantiate, which is the one thing §6.2 requires.

**None of this is measured.** It is a risk register with a first move, and §14 phase 1's
third task is to load every pinned package in one process and assert it works — because a
conflict found after twelve tables are written is a redesign, and found on day two it is
a `csproj` edit.

### 6.5 Exit codes and the CI job

Family contract, unchanged: **0** clean, **1** a real finding, **2** the authority is
unreachable — *which is not a pass*.

```yaml
  # Spec 012 D1/D2.
  options:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet run --project tools/optioncheck
```

Unguarded, and the guard that would keep the build green while the tool is absent must
never be added — 009's D9 earned that: *a guard that outlives its tool silently un-gates
the check.*

> ### AC4's schedule clause is dropped — ruled 2026-08-27
>
> **AC4 required the job on pull request *and* on a schedule. It runs on push and pull
> request only**, on the requirements' own §13.3 reasoning. Ruled by the maintainer at
> design review (§13.2), and **requirements §12's AC4 is amended to match** rather than
> left contradicting the tool.
>
> `optioncheck` reflects over a **pinned** package, so nothing outside this repository
> can change its verdict. A scheduled run over an unchanged repository and an unchanged
> package is deterministic: it can only repeat the last PR run — or exit **2** because
> NuGet was briefly unreachable. That is a red build caused by the weather, and it is
> exactly why `urlmap.py --verify` is kept out of CI: *a check whose failure mode is "the
> site was slow" teaches people to ignore red builds.*
>
> `versioncheck.py` keeps its schedule for the opposite reason — it resolves the
> **latest** version, so a release in another repository changes its answer with no
> commit here. Same family, opposite trigger, and requirements §13.3 draws that
> distinction in so many words two sections before AC4 asks for the schedule anyway.
>
> **The clause is struck rather than deleted from AC4**, so that a reader who arrives at
> the acceptance criteria finds the ruling instead of an unexplained absence.

---

## 7. The surface-to-page mapping

Every surface assigned exactly once. **This is the answer to "where does this option go"
for every option in the spec**, and the file-by-file outlines are derived from it. Counts
are `max(props, ctor)` per §2, and are **floors** where §12.1 or §12.5 applies.

### 7.1 Core — D4 and D5

| Type | Options | Page | Table heading |
|---|---|---|---|
| `ProducersConfiguration` | 26 | `CommandProcessorConfigurationReference.md` | `## AddProducers Options` |
| `BrighterOptions` | 9 | `CommandProcessorConfigurationReference.md` | `## AddBrighter Options` |
| `Publication` | 8 | `CommandProcessorConfigurationReference.md` | `## Publication Options` |
| `HandlerConfiguration` | 2 | `CommandProcessorConfigurationReference.md` | `## Handler Configuration Options` |
| `BrighterPipelineValidationOptions` | 2 | `CommandProcessorConfigurationReference.md` | `## Pipeline Validation Options` |
| `Subscription` | 17 | `DispatcherConfigurationReference.md` | `## Subscription Options` |
| `InboxConfiguration` | 5 | `DispatcherConfigurationReference.md` | `## Global Inbox Options` |
| `ConsumersOptions` | 4 | `DispatcherConfigurationReference.md` | `## AddConsumers Options` |

**D4 is 47 options over 5 tables; D5 is 26 over 3.** `ProducersConfiguration`'s 26
settable properties make it the **second-widest surface in Brighter**, behind
`GcpPubSubSubscription`'s 33 — on a page that today carries no table at all.

### 7.2 Transports — D6 and D12

| Transport | Types (options) | Page | Tables |
|---|---|---|---|
| RabbitMQ | `RmqSubscription` 24, `RmqMessagingGatewayConnection` 19, `RmqPublication` 1 | `RabbitMQConfiguration.md` | 3 |
| Kafka | `KafkaSubscription` 30, `KafkaPublication` 17, `KafkaMessagingGatewayConfiguration` 11 | `KafkaConfiguration.md` | 3 |
| AWS SNS/SQS | `SqsSubscription` 24, `SqsPublication` 4, `SnsPublication` 3 | `AWSSQSConfiguration.md` | 3 |
| Azure Service Bus | `AzureServiceBusSubscription` 18, `AzureServiceBusSubscriptionConfiguration` 6, `AzureServiceBusConfiguration` 2 | `AzureServiceBusConfiguration.md` | 3 |
| PostgreSQL | `PostgresSubscription` 24, `PostgresPublication` 3, `PostgresMessagingGatewayConnection` 1† | `PostgreSQLMessageBroker.md` | 3 |
| In-memory | `InMemorySubscription` 2† | `InMemoryTransport.md` | 1 |
| **GCP Pub/Sub** | `GcpPubSubSubscription` 33, `GcpMessagingGatewayConnection` 7, `GcpPublication` 3 | **`GcpPubSubConfiguration.md`** | 3 |
| **Redis** | `RedisSubscription` 19, `RedisMessagingGatewayConfiguration` 14 | **`RedisConfiguration.md`** | 2 |
| **RocketMQ** | `RocketMqSubscription` 22, `RocketMessagingGatewayConnection` 4† | **`RocketMQConfiguration.md`** | 3 |
| **MQTT** | `MqttSubscription` 19, `MQTTMessagingGatewayConfiguration` 8 | **`MQTTConfiguration.md`** | 2 |
| **MSSQL** | `MsSqlSubscription` 19 | **`MSSQLMessageBroker.md`** | 1 + a link (§8.3) |

† **A floor — the type has a primary constructor `survey.py` cannot read (§12.5).**
`PostgresMessagingGatewayConnection` is not in the 67 at all, and `InMemorySubscription`
has **17** primary-constructor parameters against the 2 properties the survey sees. Write
these three tables from the type, never from this column.

**RabbitMQ ships the same type in two packages and they differ by one parameter.**
`RMQ.Async`'s `RmqSubscription` takes **24** constructor parameters, `RMQ.Sync`'s takes
**23**, and the extra one is **`queueType`** — how quorum queues are selected. Diffed
parameter list against parameter list, not inferred from the two counts. The page carries
**one** table, the Async client's, which is the V10 default, and `queueType`'s row says
the Sync client has no such parameter. §13 question 3 asks review to confirm.

### 7.3 Outbox, inbox and box provisioning — D7 and D13

| Type | Options | Page |
|---|---|---|
| `DynamoDbConfiguration` | 7 | `DynamoOutbox.md` |
| `MongoDbConfiguration` | 7 | `MongoDBOutbox.md` — linked by the Mongo inbox and lock |
| `FirestoreConfiguration` | 7 | **`FirestoreOutbox.md`** (new) — linked by the Firestore inbox and lock |
| `MongoDbCollectionConfiguration` | 5 | `MongoDBOutbox.md` |
| `InMemoryBoxConfiguration` | 4 | `InMemoryOutbox.md` |
| `TimedOutboxSweeperOptions` | 4 | `BrighterOutboxSupport.md` |
| `TimedOutboxArchiverOptions` | 4 | `OutboxArchiver.md` |
| `BoxProvisioningOptions` | 2 | `BoxProvisioningConfiguration.md` |
| `DynamoDbInboxConfiguration` | 1 | `DynamoInbox.md` |
| `OutboxCircuitBreakerOptions` | 1 | `SweeperCircuitBreaking.md` — already has this table |
| `RelationalDatabaseConfiguration` | 8 | **`RelationalDatabaseConfigurationReference.md`** (§8.4) |
| *(Spanner outbox)* | **0** | **`SpannerOutbox.md`** (new) — links the relational table (§8.5) |
| *(Firestore inbox)* | **0** | **`FirestoreInbox.md`** (new, D16) — links `FirestoreOutbox.md`'s table |
| *(Spanner inbox)* | **0** | **`SpannerInbox.md`** (new, D16) — links the relational table |

**Seventeen components take `RelationalDatabaseConfiguration`, and none of them declares
an options type of its own.** Measured at `10.7.0` by grepping for the interface in the
outbox, inbox, provisioning and gateway packages:

| Family | Packages |
|---|---|
| Outbox | MsSql, MySql, PostgreSql, Spanner, Sqlite |
| Inbox | MsSql, MySql, Postgres, Spanner, Sqlite |
| Box provisioning | MsSql, MySql, PostgreSql, Spanner, Sqlite |
| Transport | **MsSql, Postgres** |

That is thirteen pages linking one table instead of thirteen copies of it, and it is the
largest single application of *do not duplicate — link to the authoritative source* in
this spec. **The PostgreSQL transport is on that list too**, which was not obvious and
which §8.3's ruling for MSSQL therefore also covers.

### 7.4 Locks — D8, and none of them is relational

Measured constructor by constructor, because the family is less uniform than it looks:

| Lock | Configuration surface | Page |
|---|---|---|
| DynamoDB | `DynamoDbLockingProviderOptions` — 4 | `DynamoDbDistributedLock.md` |
| Azure Blob | `AzureBlobLockingProviderOptions` — 3 | `AzureBlobDistributedLock.md` |
| Postgres | `PostgresLockingProviderOptions` — **1**, and **absent from the 67** (§12.5) | `PostgresDistributedLock.md` |
| MongoDB | `MongoDbConfiguration` — links §7.3 | `MongoDbDistributedLock.md` |
| Firestore | `FirestoreConfiguration` — links §7.3 | `FirestoreDistributedLock.md` |
| MSSQL | `MsSqlConnectionProvider` — no options type at all | `MsSqlDistributedLock.md` |
| MySQL | `MySqlConnectionProvider` — no options type at all | `MySqlDistributedLock.md` |

**A draft of this section said the Postgres, MSSQL and MySQL locks take
`RelationalDatabaseConfiguration`.** None of them does: two take a connection provider
and the third has an options type of its own that the survey cannot see. It was checked
because §7.3's list did not name them, which is the tell.

**Five of the seven lock pages already have tables and not one is in the §7.1 shape** —
see §12.2.

### 7.5 Schedulers — D9, and it is not inside the 619

No scheduler ships a `*Configuration` or `*Options` type; the surface is factory
properties. See §12.1.

| Factory | Settable properties | Page |
|---|---|---|
| `AwsSchedulerFactory` | 10 | `AwsScheduler.md` |
| `QuartzSchedulerFactory` | 4 | `QuartzScheduler.md` |
| `AzureServiceBusSchedulerFactory` | 4 | `AzureScheduler.md` |
| `InMemorySchedulerFactory` | 4 | `InMemoryScheduler.md` |
| `HangfireMessageSchedulerFactory` | 3 | `HangfireScheduler.md` |
| `TickerQSchedulerFactory` | **0** | `TickerQScheduler.md` — no table; §10's rule |
| **Total** | **25** | |

### 7.6 P2 — not scheduled here, mapped so nothing is lost

`GcsLuggageOptions` 12, `S3LuggageOptions` 10, `AzureBlobArchiveProviderOptions` 6,
`AzureBlobLuggageOptions` 5, `MongoDbLuggageStoreOptions` 5, `AWSS3Connection` 3,
`FileSystemOptions` 1, `StorageOptions` 1 — the luggage-store and archive family,
requirements §7.4 item 13. `AsyncApiOptions` 6 → `AsyncAPISupport.md`, which already
carries a seven-row table in nearly the right shape.

---

## 8. File-by-file outline for the eight new pages

**Every line figure below is a prose budget, not a prediction.** 009 recorded four rung
pages against four estimates and overshot on all four; the one page that did not overshoot
carried no verbatim sample. These carry none either, which argues they will land close —
and that argument is exactly the kind 009's Phase 9 note warns against treating as a rule.
Re-derive with `wc -l` at the commit that creates each page.

### 8.1 The shape four of the five transport pages share

`GcpPubSubConfiguration.md`, `RedisConfiguration.md`, `RocketMQConfiguration.md`,
`MQTTConfiguration.md`:

```markdown
---
description: "..."          # the opening sentence, markdown stripped
layout:
  description:
    visible: false
---

# <Transport> Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

<one-sentence introduction — ≤200 rendered characters, unique across the corpus>

## <Transport> General          packages; what the transport is for; the caveat that matters
## <Transport> Connection       the connection/configuration type, its table
## <Transport> Publication      the publication type, its table          ← not on all four
## <Transport> Subscription     the subscription type, its table
## <Transport> Configuration Example   one C# block: connection + publication + subscription
## Further Reading
```

**All thirty-seven proposed `##` headings across the ten new pages were checked against
the corpus and none collides**, so rule 3a's mechanical half is satisfied by construction
rather than discovered at the gate. Its editorial half is not mechanical, and §8.4 records
where this design nearly failed it.

| Page | Tables | Options | Budget |
|---|---|---|---|
| `GcpPubSubConfiguration.md` | Connection 7, Publication 3, Subscription 33 | 43 | ~190 |
| `RedisConfiguration.md` | Configuration 14, Subscription 19 | 33 | ~150 |
| `RocketMQConfiguration.md` | Connection 4†, Publication 3, Subscription 22 | 29 | ~155 |
| `MQTTConfiguration.md` | Configuration 8, Subscription 19 | 27 | ~140 |

### 8.2 The shape is not uniform, and a template would invent tables

**Redis and MQTT ship no publication type. MSSQL ships neither a publication nor a
connection type.** So §8.1's skeleton has a `## … Publication` heading two of its four
pages must not carry.

This is the Spanner ruling arriving one level up: *do not write a page a table it does not
have.* A writer working from the template would produce a `## Redis Publication` section
describing `Publication`'s base eight options as though Redis added something — and every
gate in this repository would be green on it.

### 8.3 `MSSQLMessageBroker.md` — the second relational transport

**Measured:** `MsSqlMessageProducer` takes `RelationalDatabaseConfiguration`
(`MsSqlMessageProducer.cs:69`, `:86`). There is no `MsSqlMessagingGatewayConfiguration`
and no `MsSqlPublication` anywhere in `src/Paramore.Brighter.MessagingGateway.MsSql`. The
PostgreSQL transport is the same shape (§7.3), which is why this ruling is a rule.

One table — `MsSqlSubscription`, 19 options — and a **link** to §8.4 for the connection.
Named `MSSQLMessageBroker.md` to sit parallel with `PostgreSQLMessageBroker.md`: both are
a relational database pressed into service as a transport, and requirements §10 already
chose *MSSQL Message Broker* as the `SUMMARY.md` title.

```markdown
# MSSQL Message Broker
> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](...)

## MSSQL Message Broker Overview         what it is; when a queue table beats a broker
## MSSQL Message Broker Connection       *links* the relational table; no table here
## MSSQL Message Broker Subscription     MsSqlSubscription, 19 options
## MSSQL Message Broker Configuration Example
## Further Reading                        PostgreSQLBrokerTradeOffs.md — the same trade-off
```

Budget ~130 lines. Sample material exists: `samples/TaskQueue/MsSqlMessagingGateway`,
eight files referencing the package.

### 8.4 `RelationalDatabaseConfigurationReference.md` — NEW, and the one structural addition this design proposes

**The problem, measured.** `RelationalDatabaseConfiguration` — 7 settable properties, 8
constructor parameters, 8 options by §2's convention — is taken by **seventeen
components across four families** (§7.3), reaching thirteen pages. Restating it thirteen
times is the drift this spec exists to stop, authored on purpose; requirements §7.3 says
exactly that about Spanner and the reasoning generalises.

**Why it needs a page rather than a host.** The obvious hosts fail on mode discipline,
and that was checked rather than assumed:

| Candidate | Banner today | Why not |
|---|---|---|
| `BrighterOutboxSupport.md` | **Explanation** | An options table is Reference — and inbox, lock and transport readers would be linking into the outbox subtree |
| `BrighterInboxSupport.md` | **Explanation** | Same, mirrored |
| `BrighterBasicConfiguration.md` | **How-to** | Same mode objection; it is the page showing you *how* to register the type |
| `PostgresOutbox.md` | Reference | Right mode, wrong owner — privileges one of five providers |

So: a **Reference** page, nested under *Basic Configuration* beside the two Configuration
Reference pages a reader already finds there.

```markdown
# Relational Database Configuration Reference
> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](...)

## Relational Database Configuration Options        the 8-option table
## Which Components Take the Relational Configuration   the seventeen, by family, linked
## Registering the Relational Configuration         IAmARelationalDatabaseConfiguration —
                                                    links PostgresOutbox.md, which shows it twice
## Further Reading
```

**Two of those headings were `## Which Components Take This Configuration` and
`## Registering the Configuration` in a draft.** Both are unique across the corpus, so
rule 3a's *tooled* half passes on them — and both are unattributable in a retrieval chunk,
which is the entire reason the convention exists. *A rule with a tool for half of it will
pass the half nobody checks*, and this design nearly shipped the illustration.

Budget ~140 lines. **This is D15, and an eighth `SUMMARY.md` entry beyond the seven
requirements §10 commits to.** §13 question 1 puts it to review rather than absorbing it
quietly, because a design that adds a page to an approved scope should say so where a
reviewer reads.

### 8.5 `FirestoreOutbox.md` and `SpannerOutbox.md` — D13

**These are not the same job, and requirements §7.3 item 6 is the ruling.**

`FirestoreOutbox.md` — `FirestoreConfiguration`, 7 options, one table:

```markdown
# Firestore Outbox
> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](/contents/BrighterOutboxSupport.md)

## Firestore Outbox Configuration     packages, registration
## Firestore Outbox Options           the 7-option table
## Provisioning the Firestore Outbox  links BoxProvisioning.md
## Further Reading
```

`SpannerOutbox.md` — **no configuration type exists** (`SpannerOutbox.cs:32` takes
`IAmARelationalDatabaseConfiguration`), so **no table**:

```markdown
# Spanner Outbox
> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](...)

## Spanner Outbox Configuration       links RelationalDatabaseConfigurationReference.md
## Provisioning the Spanner Outbox    what is genuinely Spanner-specific
## Spanner Connection Provider        the connection provider
## Further Reading
```

Budgets ~120 and ~110.

**`FirestoreInbox.md` and `SpannerInbox.md` — D16, ruled at §13.4 — take the same two
shapes one family over**, and neither carries a table: the Firestore inbox takes
`FirestoreConfiguration`, documented on `FirestoreOutbox.md`, and the Spanner inbox takes
the relational configuration, documented at §8.4.

```markdown
# Firestore Inbox                          # Spanner Inbox
## Firestore Inbox Configuration           ## Spanner Inbox Configuration
   links FirestoreOutbox.md's table           links RelationalDatabaseConfigurationReference.md
## Provisioning the Firestore Inbox        ## Provisioning the Spanner Inbox
## Further Reading                         ## Further Reading
```

Budgets ~100 each, and both are **Prerequisites: [Inbox Support](/contents/BrighterInboxSupport.md)**.
**These two pages are the cleanest statement of §10's rule in the spec**: four
sections, no table, and everything they document is a link.

### 8.6 The edited pages — D4, D5, D6, D7, D8, D9, D10

Additive, per requirements §3.2: each named surface gains a marker and a table under a
qualified heading, in the section that already discusses it. Two cautions throughout:

- **Rule 6 turns strict on any code block the diff touches.** A table inserted beside an
  existing C# block pulls that block into `--changed` scope. Requirements §11 says to
  budget for it; the remedy is real `using` directives or a declared `// ...`. **The
  cheapest defence is placement** — put the table *before* the section's code block where
  the prose allows, so the diff never reaches it.
- **`PostgreSQLMessageBroker.md` loses three tables and gains three.** Its existing three
  are in two non-§7.1 shapes and one has no `Default` column at all (§12.2), and its
  connection surface is §8.4's, not its own.

---

## 9. `SUMMARY.md` changes — D14

### 9.1 The diff

**Ten entries**: requirements §10's seven, plus D15 (§8.4) and D16's two (§8.5), all three
commissioned at design review. **Five are top-level and five are nested**, which is what
keeps §9.2 from getting worse.

```diff
 ## Brighter Configuration

 * [Basic Configuration](/contents/BrighterBasicConfiguration.md)
   * [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md)
   * [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md)
+  * [Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md)
 * [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)
```

```diff
 ## Transports

 * [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md)
   * [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md)
+* [MSSQL Message Broker](/contents/MSSQLMessageBroker.md)
+* [GCP Pub/Sub Configuration](/contents/GcpPubSubConfiguration.md)
+* [RocketMQ Configuration](/contents/RocketMQConfiguration.md)
+* [MQTT Configuration](/contents/MQTTConfiguration.md)
+* [Redis Configuration](/contents/RedisConfiguration.md)
 * [InMemory Transport](/contents/InMemoryTransport.md)
 * [Brighter Control API](/contents/BrighterControlAPI.md)
```

```diff
 * [Outbox Support](/contents/BrighterOutboxSupport.md)
   ...
   * [MongoDb Outbox](/contents/MongoDBOutbox.md)
+  * [Firestore Outbox](/contents/FirestoreOutbox.md)
+  * [Spanner Outbox](/contents/SpannerOutbox.md)
   * [InMemory Outbox](/contents/InMemoryOutbox.md)
 ...
 * [Inbox Support](/contents/BrighterInboxSupport.md)
   ...
   * [MongoDb Inbox](/contents/MongoDBInbox.md)
+  * [Firestore Inbox](/contents/FirestoreInbox.md)
+  * [Spanner Inbox](/contents/SpannerInbox.md)
   * [InMemory Inbox](/contents/InMemoryInbox.md)
```

MSSQL sits beside PostgreSQL because they are the same idea; the other four follow in the
order requirements §10 lists them. **The four store entries are nested**, like every other
store, so they add nothing to that section's top-level count, and each sits immediately
before its family's InMemory entry — matching the order the two families already use.
**D15 is nested** under *Basic Configuration*, like the two Reference pages it joins.

### 9.2 The five transport entries filled S2's old ceiling, so S2 was measured and moved

| Section | Top-level entries now | After D12 |
|---|---|---|
| Transports | 7 | **12** |

`urlmap.py --check-shape` failed at `len(entries) > 12`, so D12 landed on the ceiling
exactly, with **zero headroom** — the eleventh transport Brighter ships would have failed
the build. Found before writing the diff rather than at the gate, which is what made it
cheap to do the next part properly.

**S2's rationale did not survive being read.** The number appears exactly once as a value
(`tools/urlmap.py:58`) and once as a justification — 010 design §4's *"This is the number
the navigation shows"* — and that is a claim about GitBook with nothing behind it. **The
measured table four lines below that sentence says our own widest section was 10.** So 12
was our data plus two, wearing a platform fact's clothes: precisely the shape that made
**S3 the first rule in this programme to move**, where *"three is the deepest the live site
is known to work at"* turned out to be a fact about our `SUMMARY.md`.

**So the evidence was fetched, the way S3's was.** GitBook's own documentation, counted
from its raw `sitemap-pages.xml` rather than through a summarising fetch: **182 pages, 13
sections, widest section 10 top-level entries** — and its largest section,
*create-content*, holds **55 pages behind those 10**. Nothing there establishes a break at
12, at 20, or at any number. What it establishes is an *editorial habit of nesting*, which
is what S2's failure message recommends and what S2 exists to prompt.

**S2 is therefore an editorial budget, is documented as one, and is raised to 20**
(`tools/urlmap.py:58`, amended 2026-08-27; 010 design §4's row is struck in place, as S3's
was). **Red-proved before being trusted**: 21 entries fires it, exit 1, with the probe
asserting it had produced input over the threshold *before* reading the verdict — the
first attempt was vacuous twice over, once because nine entries pointing at one file
dedupe to one path, and once because Transports is 7 today rather than the 12 this section
projects.

**Why raised rather than re-parented.** Ten transports are **peers**: a reader picks one
and never reads the other nine, so a family parent page would add a click to every visit to
buy tidiness in a sidebar. And retrofitting one is not free — **nesting a page moves its
published URL**, so re-parenting the five documented transports costs five redirects.
`CLAUDE.md` § *The two kinds of nesting* records the general rule this produced: sub-topic
nesting absorbs detail, family nesting absorbs peers, and only the second controls a
section's width — which is why *Outbox and Inbox* carries 39 pages behind 9 entries.

**What remains true after the raise:** the P2 index page of requirements §7.4 item 12
still belongs in *Brighter Configuration* (four entries), which is where requirements §10
already files it — the ceiling was never what decided that.

No entry moves a URL: slugs are filename-derived, so ordering within a section moves
nothing. That has held five times, and adding to a section is the same operation.

### 9.3 The entry text is what reaches `/llms.txt`

The `SUMMARY.md` title, not the H1, is what the index prints — the two disagree on 32
pages today. All ten entries above are written to equal their page's H1, so nothing here relies on a
difference and nothing relies on the title-fallback, which applies only to entries with no
file behind them.

---

## 10. Style notes

- **Terminology: "Dispatcher".** Rule 5 is an error, and D5's page is the one most likely
  to want the assembly name. Backticks are the escape for a real type or assembly name;
  the `<!-- pagelint: allow-serviceactivator -->` comment is for a page discussing the
  name itself, and no 012 page does.
- **Do not write a page a table it does not have.** Spanner (no configuration type),
  MSSQL and PostgreSQL as transports (the shared relational configuration), MSSQL and
  MySQL locks (a connection provider), TickerQ (no factory properties), Redis and MQTT (no
  publication type). **Seven instances across four families**, which makes it a rule: *the
  absence of a configuration type is a fact about the product; the page reports it and
  links what is really there.*
- **One sentence per description, present tense, no rationale** — requirements §7.1, and
  AC8 with no tool behind it.
- **A `Default` cell is never blank.** `none` where there is no default; a `manual:`
  declaration in the marker where the tool cannot verify one.
- **The four headings are fixed at `Option | Type | Default | Description`.** Six shapes
  exist today (§12.2); normalising them is part of D8 and D10, not a separate tidy-up.
- **Deviation from the standard page pattern: none.** These are `Reference` pages, and
  `CLAUDE.md`'s ordering is satisfied by §8.1's skeleton with every heading qualified by
  its transport.

---

## 11. Code examples plan

| Example | Page | Source | Complete? |
|---|---|---|---|
| Kafka / RabbitMQ / SQS / ASB / Postgres configuration | D6 pages | already on the page; 012 does not touch them | unchanged |
| Redis configuration | `RedisConfiguration.md` | `samples/TaskQueue/RedisTaskQueue` — 3 files reference the package | complete, with `using` |
| MSSQL configuration | `MSSQLMessageBroker.md` | `samples/TaskQueue/MsSqlMessagingGateway` — 8 files | complete, with `using` |
| GCP Pub/Sub configuration | `GcpPubSubConfiguration.md` | **written from the source types** | complete, with `using` |
| RocketMQ configuration | `RocketMQConfiguration.md` | **written from the source types** | complete, with `using` |
| MQTT configuration | `MQTTConfiguration.md` | **written from the source types** | complete, with `using` |
| Firestore outbox registration | `FirestoreOutbox.md` | source types, in `PostgresOutbox.md`'s shape | complete, with `using` |
| Spanner outbox registration | `SpannerOutbox.md` | source types, in `PostgresOutbox.md`'s shape | complete, with `using` |
| Relational configuration registration | D15 | `PostgresOutbox.md` — it shows the line twice | complete, with `using` |

**Three of the five new transports have no sample anywhere in Brighter.** Measured:
`git grep -l Paramore.Brighter.MessagingGateway.<X> 10.7.0 -- samples` returns **0** for
GcpPubSub, RocketMQ and MQTT; **3** for Redis and **8** for MsSql. Requirements §7.2.1
already rules that these pages need no running broker — they are Reference pages, not
tutorials — so nothing here proposes to build one.

**But an example nobody has run should at least be an example that compiles**, and 009's
durable method makes that cheap:

> **Every C# block on the eight new pages is extracted and compiled against the packages
> `optioncheck` has already pinned and restored.** Not run — compiled. It catches an
> invented method name, a wrong namespace and a missing `using`, which is the whole class
> of defect a written-from-source example is exposed to, and it reuses a restore that
> exists anyway.

A task, not a gate: the pages are static, so the compile belongs to the phase that writes
them.

---

## 12. Six measurements taken while designing

### 12.1 Schedulers have no configuration type, so D9 is not inside the 619

`git ls-tree -r --name-only 10.7.0 -- src | grep -i scheduler` matches **no**
`*Configuration.cs`, `*Options.cs` or `*Settings.cs`. The family is configured through
**factory properties**: `AwsSchedulerFactory` 10, `QuartzSchedulerFactory` 4,
`AzureServiceBusSchedulerFactory` 4, `InMemorySchedulerFactory` 4,
`HangfireMessageSchedulerFactory` 3, `TickerQSchedulerFactory` **0** — **25 options**.

**So `survey.py`'s 619 contains no scheduler option at all**, and requirements §7.3 item
8 — *"Schedulers — 6 providers plus custom"* — is P1 work whose size no figure in the
requirements carries. Nothing in the requirements is wrong: §2 is explicit that the
survey counts *configuration types*, and a factory is not one. This is *ask what a figure
counted*, and the answer is "types whose filename says they are a configuration surface".

**What it changes:** §5's marker binds a **type** and the tool does not care what the file
was called, so
`<!-- optioncheck: Paramore.Brighter.MessageScheduler.Aws.AwsSchedulerFactory -->` is a
valid marker. D9 needs no new mechanism — only the knowledge that it exists, which is
what this note is for.

### 12.2 Only one of the corpus's 44 documented option rows is already in the §7.1 shape

Requirements §3.1 counts 12 tables, 44 rows, 9 pages. Re-derived here and **reproduced
exactly**, with the column shapes added:

| Header shape | Tables | Rows |
|---|---|---|
| `Property \| Type \| Description \| Default` | 2 | 13 |
| `Setting \| Type \| Default \| Description` | 3 | 9 |
| `Property \| Type \| Default \| Description` | 1 | 7 |
| `Member \| What your implementation must do` | 2 | 6 |
| ``Setting (on `Locking`) \| Type \| Description`` | 2 | 4 |
| `Property \| Type \| Description` | 1 | 4 |
| **`Option \| Type \| Default \| Description`** | **1** | **1** |

Three findings, in ascending order of how much they change the work:

1. **Six distinct shapes**, two putting `Description` before `Default` and three carrying
   no `Default` column at all. The one table already in §7.1's shape is
   `SweeperCircuitBreaking.md`'s, and it has a single row.
2. **`CausationTrackingStores.md`'s two tables are not option tables.** They are
   `Member | What your implementation must do` — an interface contract for a reader
   writing their own store. No type, no default, nothing for reflection to check. They
   match §3.1's *option-shaped* regex correctly and are **out of scope for the checker**:
   they get no marker.
3. So **D8's *"normalisation; five already have tables"* understates it.** All twelve
   tables are touched, ten change shape, and two leave the checker's scope entirely.
   **38 checkable rows, not 44** — and requirements §3.1 is right about what it counted.

*(A first extractor written for this section reported 10 tables / 40 rows / 7 pages and
disagreed with the requirements. **The requirements were right**: two lock tables head
their first column ``Setting (on `Locking`)`` and the stricter regex demanded the header
word end the cell. *Two tools that disagree about the same file are not both right*, and
here the new one was the suspect.)*

### 12.3 The synthesis burden is 48 parameters across 24 types, and 20 of the 24 need only strings

§6.3, measured with `survey.py`'s own parser so it is the same instrument the 619 came
from rather than a second one.

### 12.4 Seventeen components take the relational configuration

§7.3's table, measured by grepping the outbox, inbox, provisioning and gateway packages
for `IAmARelationalDatabaseConfiguration` at `10.7.0`. **The PostgreSQL transport is on
that list**, which §8.3's MSSQL ruling therefore also covers, and **there is a Spanner
inbox** — see §12.6.

### 12.5 Thirteen surfaces use a primary constructor, and `survey.py` cannot read one

`survey.py`'s `widest_ctor` matches `public TypeName(` **inside** a class body. A C# 12
primary constructor is on the declaration line — `public class PostgresLockingProviderOptions(string connectionString)`
— and is invisible to it. Measured at `10.7.0`: **13 surface types, 40 parameters.**

| Consequence | Which |
|---|---|
| **Absent from the 67 entirely** | `PostgresLockingProviderOptions` (1), `PostgresMessagingGatewayConnection` (1) — both had zero settable properties *and* zero classic constructors, so nothing put them in the list |
| **Badly under-counted** | `InMemorySubscription` — **2** by the survey, **17** primary-constructor parameters |
| **Under-counted** | `AzureBlobArchiveProviderOptions` 6, `MongoDbLuggageStoreOptions` 3, `FirestoreConfiguration` 2, `AzureBlobLockingProviderOptions` 2, `DynamoDbLockingProviderOptions` 2, `RocketMessagingGatewayConnection` 1, `DynamoDbInboxConfiguration` 1, `FileSystemOptions` 1 |

**So 619 is a floor and 67 is a floor.** Requirements §14 already provides for this —
*"re-run `survey.py --ref` rather than reasoning from the table in §2"* — and this is that
clause being taken literally rather than a contradiction of it.

**What it changes, and it is small:** `optioncheck` reflects over assemblies, where a
primary constructor is just a constructor, so **the tool is unaffected**. What is affected
is any figure quoted *from the survey*, which is why §7's counts are marked as floors and
why §14 phase 1 fixes `survey.py` (D11) before a table is written. The alternative —
leaving it and quoting the numbers anyway — is the one this programme has already learned
is worst: **a figure that is wrong rather than absent.**

### 12.6 The inbox family has the same two gaps the outbox family had, and no deliverable names them

Measured: eight outbox stores and **eight inbox stores** ship at `10.7.0`.

| Family | Stores | Pages | Missing |
|---|---|---|---|
| Outbox | DynamoDB, **Firestore**, MongoDb, MsSql, MySql, PostgreSql, **Spanner**, Sqlite | 9 (incl. InMemory) | Firestore, Spanner → **D13 commissions both** |
| Inbox | DynamoDB, **Firestore**, MongoDb, MsSql, MySql, Postgres, **Spanner**, Sqlite | 7 (incl. InMemory) | **Firestore, Spanner — no deliverable** |

Requirements §3.5 is right about the outbox family and silent about these two on the inbox
side; §3.6 records the README's inbox list as *"Holds"*, and against six stores it did.
The maintainer's §13.2 ruling — *same rule as the transports, both get pages* — points
straight at them.

**Ruled at §13.4: two more pages, `FirestoreInbox.md` and `SpannerInbox.md`, on the same
rule and with the same asymmetry** — Firestore links `FirestoreConfiguration` on its
outbox page, Spanner links the relational table. They are **D16**, and they cost two pages
and zero options.

**The general shape, and it is the reason this was worth looking for:** a ruling made
about one family is a claim about a *rule*, and the cheapest way to test it is to point
it at the neighbouring family before anyone else does. §13.2 of the requirements settled
the outbox side; nobody asked what the inbox side looked like, and it took one
`git ls-tree` to find out that it looked identical.

---

## 13. Questions for review — ALL FOUR ANSWERED 2026-08-27

**Answered by the maintainer on the day they were asked, and kept struck rather than
deleted**, because each changed a section and a reader of that section deserves to find
the ruling behind it. Two of the four grew the spec.

### 13.1 ~~D15 — a ninth new page and an eighth `SUMMARY.md` entry?~~

**Answered: yes, build it.** `RelationalDatabaseConfigurationReference.md` (§8.4) carries
the eight relational options once instead of thirteen times. The alternatives were
thirteen copies — the drift 012 exists to stop — or a Reference table on an Explanation
or How-to page, which is the mode discipline 011 exists to keep.

### 13.2 ~~AC4's schedule clause?~~

**Answered: drop it.** `optioncheck` runs on push and pull request only.
**Requirements §12's AC4 is amended accordingly**, and this is the second time a ruling
has edited that approved document — the first was §13's own three.

The reasoning is the requirements' own §13.3: a **pinned** checker cannot change its
answer without a commit here, so a scheduled run can only repeat the last PR verdict or
exit **2** because NuGet was briefly unreachable. That is the failure mode
`urlmap.py --verify` is deliberately kept out of CI for. `versioncheck.py` keeps its
schedule for the opposite reason — it resolves *latest*, so a release elsewhere changes
its answer with no commit here. **Same family contract, opposite trigger; do not reason
about the two together.**

### 13.3 ~~RabbitMQ's Sync client — one table or two?~~

**Answered: one** — the Async client's, which is the V10 default, with `queueType`'s row
saying the Sync client has no such parameter. Two tables would be 23 near-duplicate rows
for a one-parameter difference.

### 13.4 ~~Firestore and Spanner inboxes — two more pages?~~

**Answered: yes, on the same rule as §13.2 of the requirements.** `FirestoreInbox.md` and
`SpannerInbox.md` join D13 as **D16**, with the same asymmetry the outbox pair has:
Firestore links `FirestoreConfiguration` on its outbox page, Spanner links the relational
table. Neither introduces a configuration type, so they are the cheapest pages in the
spec — **two pages and zero options**, which is the shape §7.2.1 already measured for the
five transports and is worth noticing a second time.

**The cost of the four answers, stated the way requirements §14 states its own:
three more pages, two more `SUMMARY.md` entries than the eight this design opened with,
and zero additional options.** Every option on the three new pages is a link to a table
somewhere else.

---

## 14. Sequencing

Requirements §14 obliges one ordering: **the five new transport pages come after the
checker exists**, so their tables are the first written under a gate rather than the last
brought under one. With the standing rule that one PR is one coherent unit, merged before
the next branch starts:

| Phase | Delivers | Why here |
|---|---|---|
| 1 | **Three probes and a `survey.py` fix.** Instantiate `Subscription` and read `EmptyChannelDelay`; load every pinned package in one process; re-derive §6.3's synthesis table by construction. Then teach `survey.py` primary constructors (§12.5) and re-run it with a ref | Requirements §5.2 names the first as the implementation phase's first task, and the whole `Default` column rests on it. §6.4's conflict risk is cheapest to find on day two. The survey fix precedes every table, because a table written from a floor is a table written from a wrong number |
| 2 | **D1 + D2 + D3** — the checker, its CI job, the pin in `RELEASE_CHECKLIST.md`, and the AC3 / AC3b / AC5 red-proofs | *Prove a new gate fails before trusting it to pass.* AC3b is the one that matters: a body-coalesced default reported as `null` |
| 3 | **D4 + D5** — the two core Reference pages, 73 options | Additive, highest traffic, and the first tables written under the gate |
| 4 | **D15** — the relational reference page | Thirteen pages link it, so it precedes D6, D7 and D8 |
| 5 | **D6** — the five documented transports, 186 options | Existing pages; the checker is proven by now |
| 6 | **D12** — the five new transport pages, 151 options | §14's ordering obligation, satisfied |
| 7 | **D13 + D16** — the Firestore and Spanner outbox *and* inbox pages, four pages and zero options | Small, and three of the four depend on D15 existing. One PR: they are one decision (§13.4) and share every link |
| 8 | **D7 + D8** — outbox, inbox and lock pages, including §12.2's twelve-table normalisation | The largest editing phase, and the one that most needs the checker to already be trusted |
| 9 | **D9** — schedulers, 25 options | The family §12.1 found; nothing depends on it |
| 10 | **D10** — the two stale cross-cutting tables | Last, because it is the one *corrective* rather than additive edit, and diffing it against `survey.py` is easiest once every transport has a page to check against |
| 11 | **Acceptance pass** — AC1–AC9 walked with evidence | |

**AC8 is walked in every phase that writes a table, not only at phase 11.** 009's AC7 was
the criterion with no tool behind it, and it was the one found unmet at the close, on
pages green under all six gates. The remedy that follows is not a bigger acceptance pass;
it is checking the untooled criterion while the work is still small enough to fix.

---

## 15. Traceability

| AC | Met by | Note |
|---|---|---|
| AC1 | §7's mapping; phases 3–9 | Every P0 surface has a named page and a named heading |
| AC2 | §6 | `optioncheck` exit 0 |
| AC3 | §6.2; phase 2's red-proof | The route that reads constructor parameters |
| AC3b | §6.2; phase 2's red-proof | `EmptyChannelDelay`, whose signature says `null` |
| AC4 | §6.5 | **Amended 2026-08-27** — push/PR only, schedule clause struck; §13.2 |
| AC5 | §6.5 | Exit 2, red-proof with the package source removed |
| AC6 | §8.1–8.3, §9 | Five new transport pages, five entries, the orphan check |
| AC6b | §8.5 | Firestore with a table, Spanner without — **and the same pair one family over**, §13.4 |
| AC7 | phase 10 | Diffed against `survey.py` at the release ref — after phase 1 fixes it |
| AC8 | §10, §14 | No tool; walked per phase |
| AC9 | every phase | The six gates, whose values at `9df5e89` are in PROMPT's state block |

| Requirement | Answered by |
|---|---|
| §5.1's three default shapes | §6.2 — one route per column, an instance for every default |
| §5.2's synthesis residue | §6.3 sized; §5.1's `manual:` key makes it countable |
| §7.1's table format | §4, and §12.2 measures how far the corpus is from it |
| §7.2.1's five pages | §8.1–8.3, and §8.2 records why they are not uniform |
| §7.3 item 6 — Firestore and Spanner | §8.5; §12.6 found the same gap one family over and §13.4 closed it |
| §7.3 item 10 — the residue named per option | §5.1's `manual:` declarations, printed in the scope line |
| §10's `SUMMARY.md` entries | §9 — seven, plus D15 and D16's two, all ruled at §13 |
| §14's "checker before the tables" | phases 2 and 6 |
