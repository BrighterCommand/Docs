# Spec 012 — Phase 1 probes

Four measurements taken before spec 012 writes a table, and the project that
takes them. Everything here is **kept and re-runnable**; the numbers below are
stamped at Brighter **`10.7.0`** and mean nothing without that ref.

```bash
dotnet run --project spec/012-configuration_reference/probes                 # 1.2, 1.3, 1.4
dotnet run --project spec/012-configuration_reference/probes -- default      # task 1.2
dotnet run --project spec/012-configuration_reference/probes -- packages     # task 1.3
dotnet run --project spec/012-configuration_reference/probes -- synthesis    # task 1.4
dotnet run --project spec/012-configuration_reference/probes -- counts       # task 1.5's oracle, TSV
```

Exit code is the family contract `linkcheck.py`, `pagelint.py` and
`versioncheck.py` already run under: **0** clean, **1** a real finding, **2** the
subject is unreachable. Nothing in CI runs these — they are a phase-1
instrument, not a gate. The gate is phase 2's `optioncheck`.

**`probes.csproj` is the pinned package list**, and task 2.1 carries it into
`optioncheck.csproj` verbatim rather than re-deriving it, so the repository holds
one proven list instead of two that can differ silently. No `.V4` packages:
requirements §8 puts them out of scope and design §6.4 names them as the pair
most likely to put two SDK majors in one process.

---

## Task 1.2 — the body-coalesced default: **PREMISE HOLDS**

Requirements §5.1 reads `Subscription.cs:208` and `:236` and *infers* that
`emptyChannelDelay` is `null` in the signature and 500 ms on the instance. It
says of itself that reading the authority is not measuring it. Measured:

```text
emptyChannelDelay  ParameterInfo.DefaultValue = null
EmptyChannelDelay  instance                   = 500 ms
```

The whole `Default` column, AC3b, and design §6.2's *one route for one column*
rest on that difference, and it is real.

**The shape is not rare and it is not uniform.** Of `Subscription`'s defaulted
constructor parameters the probe left alone, **six say `null` in the signature
and four come back as a value**:

| Parameter | Signature | Instance |
|---|---|---|
| `timeOut` | `null` | 300 ms |
| `requeueDelay` | `null` | 0 ms |
| `emptyChannelDelay` | `null` | **500 ms** |
| `channelFailureDelay` | `null` | 1000 ms |
| `channelFactory` | `null` | `null` |
| `unacceptableMessageLimitWindow` | `null` | `null` |

The last two are why a checker cannot discriminate by reading the signature:
`null` there means both *"there is no default"* and *"the default is assigned
below"*, and nothing distinguishes them until you build the object.

> **Found by construction, and requirements §5.1 could not have said it.**
> `Subscription` **cannot be built from its required parameters alone.** The
> constructor body throws `ConfigurationException` unless `messagePumpType` is
> something other than its own default of `Unknown`, and unless one of
> `requestType` / `getRequestType` is non-null. **Two defaulted parameters are
> required in practice.** Task 1.4 measured how far that generalises: it is
> **thirteen types**, every subscription in the product.

---

## Task 1.3 — every pinned package in one process: **CLEAN, with one thing to know**

**64 `Paramore.Brighter` assemblies in one process** — the 62 packages
`probes.csproj` names plus the Brighter packages they pull in — and **one type
instantiated from each**, for real, not by metadata reflection. **65 distinct
third-party assemblies** are referenced between them and every one of them
loads.

**Design §6.4's fallback is not needed. `optioncheck` can be a single project.**

### The conflict is real, and it is not the pair design predicted

§6.4 expected the AWS SDK pair, and removing the `.V4` packages did remove it.
The disagreement that survives is **RabbitMQ**:

```text
RabbitMQ.Client: loaded 7.0.0.0, but Paramore.Brighter.MessagingGateway.RMQ.Sync
                 asked for 6.0.0.0
```

One process holds one version of a simple name, so `RMQ.Sync` runs against an
assembly it has never seen. **Exactly one of its 57 types fails to load** — the
consumer, which derives from a `RabbitMQ.Client` type deleted in 7.x. Everything
else in the package is fine.

**It does not touch anything 012 documents**, which the probe asserts rather
than assumes, because design §7.2's RabbitMQ ruling needs both packages:

| Package | Result |
|---|---|
| `…RMQ.Async` | constructed, **24** ctor params, `queueType` **present** |
| `…RMQ.Sync` | constructed, **23** ctor params, `queueType` **absent** |

That is design §7.2's parameter diff **re-derived from the assemblies** rather
than from source, by a second instrument: one table, the Async client's, and
`queueType` is the one the Sync client does not have.

**Record it, do not fix it.** The next Brighter package to reach into
`RabbitMQ.Client` 6's API turns a latent disagreement into a load failure, and
the fix then is one line — drop `RMQ.Sync` from the checker's `csproj`, since
012 binds no type in it.

### Eleven packages have nothing the synthesiser can build, and none of them matters

`Paramore.Brighter.MsSql`, `.MySql`, `.PostgreSql`, `.Sqlite`, `.Spanner`,
`Outbox.MongoDb`, `Outbox.Firestore`, `Inbox.MongoDb`, `Inbox.Firestore`,
`Locking.Firestore` and `Archive.Azure`. Their public types are connection
providers and store classes that want a live client or a real connection string
— and **design §10 already says those pages have no options type**: *the absence
of a configuration type is a fact about the product; the page reports it and
links what is really there.* Not one of them is a type 012 writes a table from.

---

## Task 1.4 — design §6.3's synthesis table, by construction

§6.3 says **24 types and 48 required parameters**, parsed with `survey.py`, and
says in the section itself that it is not a measurement of the running tool.
Constructed:

| | §6.3 | Measured |
|---|---:|---:|
| Types with a parameterful public constructor | 24 | **34** |
| Required parameters | 48 | **70** |
| Needing a hand-written factory | 4 | **2** |

**The population moved because the parser was wrong, not because the product
changed** — see task 1.5 below. Two independent errors happened to be in it:

- **`.V4` was subtracted twice over, and incompletely.** §6.3 says *"28 types
  … excluding the four `.V4` duplicates"*. There are **six** `.V4` types with a
  parameterful constructor; two of them carry no required parameter, which is
  why the *parameter* arithmetic came out right and the *type* count did not.
- **The parser could not see a primary constructor and assumed the class was the
  file**, which is task 1.5's subject.

**The rebuilt `survey.py` and this probe now agree exactly — 34 types, 70
required parameters — one number from parsing source, one from constructing
objects.**

### The finding: thirteen constructors reject their own defaults

Every subscription type in Brighter, plus `SqsSubscription`'s extra pair:

```text
InMemorySubscription, Subscription, SqsSubscription, AzureServiceBusSubscription,
GcpPubSubSubscription, KafkaSubscription, MqttSubscription, MsSqlSubscription,
PostgresSubscription, RmqSubscription (Async), RmqSubscription (Sync),
RedisSubscription, RocketSubscription
```

All thirteen need `requestType` and `messagePumpType` supplied, and all thirteen
need `makeChannels` — a defaulted `enum` whose declared default the body will
not accept. `KafkaSubscription` needs six such parameters, `SqsSubscription`
five, `GcpPubSubSubscription` four.

**This is the one thing phase 2 must budget for that design does not name.**
A synthesiser that supplies only the parameters carrying no default constructs
**19** of the 34; supplying values for defaulted enum and `Type` parameters too
takes it to **32**.

### Only two types genuinely need a factory, and the other two are a caveat

§6.3 names four: `MongoDbConfiguration`, `HandlerConfiguration`, `AWSS3Connection`
and `S3LuggageOptions`. Measured, **`AzureBlobArchiveProviderOptions` and
`S3LuggageOptions`** are the two that fail. The other three construct — because
their unbuildable parameters are *reference* types, and passing `null` for one
is accepted.

**That is a smaller burden and a new obligation.** An instance built with `null`
for its client or its credentials reads back defaults perfectly *unless a
constructor body derives one from the missing object*. Phase 2 must decide that
per type rather than per family, and where it cannot, the answer is a `manual:`
declaration on the marker — which declares and counts — never a silent pass.

---

## Task 1.5 — `survey.py` rebuilt, and the totals moved

**At `10.7.0`: 72 configuration types, 628 reader-facing options** — against the
**67 and 619** every document in this spec quotes. Re-derive, never quote
without the ref:

```bash
python3 spec/012-configuration_reference/survey.py --ref 10.7.0
```

The task asked for one fix — primary constructors, design §12.5. **Probe 1.4
found two more of the same kind, and the reflection oracle found two beyond
that.** All five are the same failure: the script was measuring something
adjacent to the type it named, and reported a number rather than an error.

| # | Defect | Found by | Cost |
|---:|---|---|---|
| 1 | **Primary constructors invisible.** `widest_ctor` matched `public TypeName(` *inside* a class body | design §12.5 | `InMemorySubscription` **2 → 17**; `PostgresLockingProviderOptions` and `PostgresMessagingGatewayConnection` absent from the population entirely |
| 2 | **The class was assumed to be the file** | probe 1.4 | `RocketMqSubscription.cs` declares **`RocketSubscription`** (23, not 22); `AWSMessagingGatewayConfiguration.cs` declares **`AWSMessagingGatewayConnection`**; `MQTTMessagingGatewayConfiguration.cs` declares **`MqttMessagingGatewayConfiguration`** and two more |
| 3 | **Properties counted per FILE, not per class** | probe 1.4 | `RmqMessagingGatewayConnection` **19 → 11**: the other 8 belong to `AmqpUriSpecification` and `Exchange`, which share its file |
| 4 | **A generic type argument contains a SPACE** — `Dictionary<string, object>?` never matched | the oracle diff | `Publication` **8 → 10**, `AsyncApiOptions` 6 → 7, `AzureBlobLuggageOptions` 5 → 6, `InboxConfiguration` 0 → 1 settable |
| 5 | **`private set`, `internal set` and `static` counted as reader-facing** | the oracle diff | `ProducersConfiguration` **26 → 22**, `MqttMessagingGatewayConfiguration` 8 → 7, `HandlerConfiguration` 2 → 0 settable |

### How defects 4 and 5 were found, and why that method is worth keeping

`survey.py --tsv` and `probes -- counts` print **the same quantity** — assembly,
type, settable properties, widest constructor, `max` — one by parsing source and
one by reflecting over the assemblies. Diffing them:

```text
survey 72, reflection 68, both 63, disagree 0
```

**Zero disagreements on all 63 types both instruments see.** The nine the survey
sees alone are the `.V4` packages, which the probe deliberately does not
reference. The five reflection sees alone carry **zero options** — `max` of 0 —
and the survey drops them by rule.

The first run of that diff had **nine** disagreements. Every one was a defect in
the parser and none was visible any other way: each returned a plausible smaller
number. **Run this diff after any change to `survey.py`.**

### The rows that moved against design §7

Design §7 is approved and this does not amend it. What it does is discharge
standing obligation 3 in advance — *write the table from the type, never from
the survey* — by saying which rows a writer would have copied wrongly:

| Type | Design §7 | Measured `10.7.0` | Page |
|---|---:|---:|---|
| `ProducersConfiguration` | 26 | **22** | `CommandProcessorConfigurationReference.md` (D4) |
| `Publication` | 8 | **10** | `CommandProcessorConfigurationReference.md` (D4) |
| `RmqMessagingGatewayConnection` | 19 | **11** | `RabbitMQConfiguration.md` (D6) |
| `RmqSubscription` (Sync) | 24 | **23** | not documented — the page carries the Async table |
| `InMemorySubscription` | 2 † | **17** | `InMemoryTransport.md` (D6) |
| `RocketMqSubscription` | 22 | **`RocketSubscription`, 23** | `RocketMQConfiguration.md` (D12) |
| `MQTTMessagingGatewayConfiguration` | 8 | **`MqttMessagingGatewayConfiguration`, 7** | `MQTTConfiguration.md` (D12) |
| `AsyncApiOptions` | 6 | **7** | `AsyncAPISupport.md` (P2, task 8.4) |
| `AzureBlobLuggageOptions` | 5 | **6** | P2, unscheduled |

**Two of those are type NAMES, not counts, and they matter more.** A marker
binds a fully-qualified type; `<!-- optioncheck: …RocketMqSubscription -->` and
`…MQTTMessagingGatewayConfiguration` name types that do not exist, and the
checker would exit 1 with *the type is gone*. It fails loudly, which is the
design working — but it fails in phase 6, and it is free to fix now.

**`D4 is 47 options over 5 tables` becomes 45**: `ProducersConfiguration` loses
4 and `Publication` gains 2.

### Design §12.5 overstates its own effect, and §2.9 of `tasks.md` inherits that

§12.5 measures 13 primary-constructor surfaces and **40 parameters**, and
`tasks.md` §2.9 turns that into per-type claims — *"`FirestoreConfiguration`
under-counted by 2"*, *"`AzureBlobLockingProviderOptions` by 2"*. Those are
**additive** claims, and the convention is **`max(props, ctor)`** (design §2,
`survey.py`). Measured with the primary constructor now readable, **three of the
thirteen move**:

| Surface | Design §7 | Now | Why |
|---|---:|---:|---|
| `InMemorySubscription` | 2 | **17** | 17 primary-ctor params against 2 properties |
| `PostgresLockingProviderOptions` | — | **1** | was absent from the population |
| `PostgresMessagingGatewayConnection` | — | **1** | was absent from the population |
| `FirestoreConfiguration` | 7 | 7 | 7 properties, 2 primary-ctor params — `max` is 7 |
| `AzureBlobLockingProviderOptions` | 3 | 3 | its two ctor parameters are **assigned to two of its three properties** |
| `DynamoDbLockingProviderOptions` | 4 | 4 | same shape |
| `DynamoDbInboxConfiguration` | 1 | 1 | same shape |
| `RocketMessagingGatewayConnection` | 4 | 4 | same shape |
| the five P2 types | — | unchanged | same shape |

**A C# primary constructor on a class assigns to properties rather than creating
them**, so its parameters are usually the same options the properties already
are. That is why `max` is the right convention and why counting them as
additional would double-count.

**§2.9's verdict is untouched and its instruction is the right one** — *never
infer from an unmarked row that its figure is a total* — and this is that
instruction applied to §2.9 itself.

### Two things neither instrument counts

- **Public fields.** `AzureBlobLockingProviderOptions.StorageLocationFunc` is a
  public field with a default, reader-facing in every sense, and invisible to a
  property scan and to `PropertyInfo` alike. There are few, and phase 8 should
  look at the type rather than at either tool.
- **Surface types whose name does not say so.** `AmqpUriSpecification` and
  `Exchange` are configuration a RabbitMQ reader writes by hand, and neither name
  ends in one of the five suffixes. The old survey counted their properties by
  accident, as part of `RmqMessagingGatewayConnection`'s file; the new one drops
  them honestly. **Phase 5 owns the decision** about whether
  `RabbitMQConfiguration.md` gains a table for them.
