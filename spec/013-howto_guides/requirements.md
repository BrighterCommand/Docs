# Spec 013: Task-Oriented How-To Guides — Requirements

**Status:** Draft, awaiting review
**Created:** 2026-09-04
**Answers:** [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)

> Every figure in this document was derived on **2026-09-04** against Docs `master` at
> **`f7e0933`** and Brighter at the **`10.7.0`** tag. A total with no ref is not a fact —
> re-derive before quoting, with the command given beside it.

---

## 1. Topic overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) asks for documentation that
*"focus[es] on solving specific problems users face"*. This spec writes the guides that answer
questions readers have actually asked, and closes the one composition gap that was publicly
committed to.

The framing in this spec's README — written 2026-08-03 — is that the corpus lacks how-tos. **That
is not what the corpus measures today**, and the requirements phase exists to say so. Of 157 pages
under `contents/`, **53 are already typed `How-to`**:

```bash
awk -F'\t' 'NR>1{print $5}' spec/011-authoring_conventions/pagetypes.tsv | sort | uniq -c
```

| Verdict | Pages |
|---|---|
| Reference | 65 |
| **How-to** | **53** |
| Explanation | 34 |
| Tutorial | 5 |
| **Total** | **157** |

What is missing is not the *mode*. It is the **composition** — a recipe that spans two subjects
the reader has to reconcile alone — and a small number of task-shaped routes through mechanisms
the corpus explains but never has the reader *do*.

---

## 2. Current state — the README's list is two specs out of date

The README's inherited gap list was compiled before Spec 010 re-filed the corpus and Spec 012
tabulated it. **Re-derived rather than inherited, as this spec's README instructs:**

### 2.1 The two named content gaps are closed

| README claim (2026-08-03) | Measured 2026-09-04 |
|---|---|
| *"`contents/Logging.md` is a three-line stub — an H1 and the word `TODO`"* | **151 lines**, a real body, `Reference` banner. Written in 010's `810ec04` |
| *"`MessageMappers.md`'s `## Transformers` should become its own Explanation page"* | **`contents/MessageTransforms.md` exists**, 287 lines, `Explanation`, and carries the maintainer's ruling in its opening sentence |

### 2.2 Three of the four "missing how-tos" are closed, and only one survives

Spec 011's `worklist.md` §8 handed 013 four Explanation pages with no task-shaped route.
Spec 010's splits closed three of them:

| Explanation | How-to owed | State today |
|---|---|---|
| `MessageMappers.md` | use the default mapper | **CLOSED** — `DefaultMessageMappers.md`, 341 lines, typed **How-to**, and linked from `MessageMappers.md:24` |
| `DynamicMessageDeserialization.md` | route several message types down one channel | **CLOSED** — `RoutingMultipleMessageTypes.md`, 338 lines, typed **How-to** |
| `QueriesAndQueryObjects.md` | write a query and its handler | **CLOSED** — `ImplementAQueryHandler.md`, 655 lines, typed **How-to** |
| `ClaimCheck.md` (75 lines) | put a large payload behind a claim check | **OPEN** — still a 75-line `Explanation` with `S3LuggageStore.md` (68 lines, `Reference`) beneath it. No steps anywhere |

**PROMPT.md recorded "two of its listed content gaps are already closed". The measurement is
five of six.** This is the programme's own recurring shape — *an inherited gap list rots because a
later spec closed the gaps* — and it is why the README says re-derive.

### 2.3 There is no *How To* section, and 013 does not get one by default

The README's scope opens with *"A **How To** section in `SUMMARY.md` (structure agreed in Spec
010)"*. **Spec 010 agreed the opposite**, explicitly and twice — design §6.2 is headed *"There is
no How To section, and 013 does not get one either"*, and `tasks.md` repeats it as a standing
"do not". The rule 013 inherits, quoted from design §6.2:

> A how-to lives with its subject. A how-to that genuinely spans two or more subjects — the
> publicly committed *PostgreSQL for both transport and outbox* guide is the example — goes
> wherever a reader would look first, and links from the other. Only if 013 accumulates **three or
> more** genuinely cross-cutting guides does a *Guides* section become worth its own place in the
> tree, and that is 013's call, not this one.

So the section is a **decision this spec must make on evidence**, not a given. §10 makes the
proposal and §13 puts it to review.

---

## 3. Demonstrated demand — the measurement that shapes the list

The README's own risk mitigation binds this spec: guides are written for *demonstrated* demand —
**a real question asked more than once** — never speculatively. So demand was measured before the
list was written.

**The census, not a sample.** The repository holds **171 discussions**, of which **55 are Q&A**.
An earlier pass over the 100 most recent returned 31 and would have been a floor presented as a
total:

```bash
gh api graphql --paginate -f query='query($endCursor:String){repository(owner:"BrighterCommand",name:"Brighter"){discussions(first:100,after:$endCursor,orderBy:{field:CREATED_AT,direction:DESC}){pageInfo{hasNextPage endCursor} nodes{number title createdAt category{name}}}}}' \
  --jq '.data.repository.discussions.nodes[] | select(.category.name=="Q&A")'
```

`contents/FAQ.md` supplies the second source: **27 questions in 8 subject sections**
(`grep -c '^### ' contents/FAQ.md`).

### 3.1 The clusters, by number of independent askings

| Cluster | Askings | Answered by a page today? |
|---|---|---|
| **Resilience pipeline configuration** | **5** — Q&A [#3960](https://github.com/BrighterCommand/Brighter/discussions/3960), [#3862](https://github.com/BrighterCommand/Brighter/discussions/3862), [#3699](https://github.com/BrighterCommand/Brighter/discussions/3699); FAQ ×2 | **Partly** — see §3.2 |
| **Routing several message types over one channel** | **4** — #3829, #3674, #1874, #1571 | **Yes** — `RoutingMultipleMessageTypes.md` + `Routing.md`. Closed by 010 |
| **Poison messages and the DLQ** | **4** — Q&A [#3218](https://github.com/BrighterCommand/Brighter/discussions/3218) (Kafka), [#2103](https://github.com/BrighterCommand/Brighter/discussions/2103) (ASB); issues [#3667](https://github.com/BrighterCommand/Brighter/issues/3667), [#3808](https://github.com/BrighterCommand/Brighter/issues/3808) | **No task-shaped route** — see §3.3 |
| **PostgreSQL as transport *and* Outbox** | **3** — Q&A [#3795](https://github.com/BrighterCommand/Brighter/discussions/3795), [#3626](https://github.com/BrighterCommand/Brighter/discussions/3626); Docs#67 | **No** — see §3.4 |
| **Kafka offsets, rebalance and partitions** | 3 — #2263, #2147, #2407 | **Yes** — 009's rung 4, `TutorialStreamingWithKafka.md` |
| **CloudEvents type → request mapping** | 2 — #4213, #3950 | **Yes** — 11 pages, incl. `CloudEventsSupport.md`, `CloudEventsReference.md` |
| Large messages / claim check | 1 Q&A + FAQ *"How do I handle large messages?"* | **No steps** — §2.2's surviving gap |
| Health checks of the Dispatcher | 1 — #1539 (2021) | **Thinly** — see §3.5 |
| EF Core transactions with the Outbox | 1 — #3897 | **Yes** — `EFCoreOutbox.md`, 98 lines |
| Saga, AOT, SQS rate limiting, custom outbox | 1 each | Out of scope — §8 |

**Two clusters are evidence the recent work landed on real demand rather than on intuition**: the
four-asking routing cluster and the three-asking Kafka cluster are both closed, by 010 and 009
respectively. Neither spec was aiming at this measurement.

**One asking was nearly counted twice.** Brighter issue
[#3959](https://github.com/BrighterCommand/Brighter/issues/3959) and Q&A discussion
[#3960](https://github.com/BrighterCommand/Brighter/discussions/3960) share a title, a date and a
body: the maintainer's first comment is *"Going to move this to a discussion"*. Counting both put
the resilience cluster at 6. **A tracker that moves a thread leaves the same asking in two
enumerations**, and neither number is wrong — a census across issues *and* discussions has to
dedupe by thread, not by row.

### 3.2 The resilience cluster is the strongest demand in the corpus, and it is a page edit

Q&A [#3960](https://github.com/BrighterCommand/Brighter/discussions/3960) is worth reading in
full. A reader opens with *"I've been at this issue for at least 4 hours"*, states two goals —
**SQL Server for Inbox, Outbox and broker** and **a resilience pipeline on an async handler** —
and hits three walls: the synchronous `UseResiliencePipeline` attribute rejected inside an async
handler, a `KeyNotFound` on a non-generic pipeline, and no worked example. The maintainer points
at `PolicyRetryAndCircuitBreaker.md`, a contributor is pulled in, and the thread is finally
answered **by another user** posting a working `ResiliencePipelineRegistry<string>` recipe.

The maintainer's own assessment in that thread, which is the clearest statement of priority this
spec has: *"the configuration setup for Brighter is definitely a lot"* → **"100%, I think it is
one of our biggest weaknesses and we need to find a better, simpler path"**.

`PolicyRetryAndCircuitBreaker.md` is 445 lines and typed **How-to**, with twelve `##` sections
including *Type-Scoped Pipelines*, *Registering Pipelines with CommandProcessor* and
*Retry and Circuit Breaker Troubleshooting*. It is a good page. **Two things a reader needs are
not on it:**

| Symbol | Pages carrying it (of 157) | Where |
|---|---|---|
| `UseResiliencePipelineAsync` | **2** | `PipelineValidation.md`, `HandlerFailure.md` — **not** the resilience how-to |
| `AddBrighterDefault` | **0** | nowhere in the corpus |

`AddBrighterDefault` is **public core API at `10.7.0`** —
`src/Paramore.Brighter/Extensions/ResiliencePipelineRegistryExtensions.cs:57`, an extension on
`ResiliencePipelineRegistry<string>` whose XML comment documents the two default strategies it
registers (`CommandProcessor.OutboxProducer`, `CommandProcessor.RequestReply`). It is the API the
community member had to reverse-engineer. Verify with a control, since a zero is also what a
broken grep returns — `ResiliencePipelineRegistry` itself returns 5 pages:

```bash
git -C ../Brighter grep -ln 'AddBrighterDefault' 10.7.0 -- src/
grep -rl 'AddBrighterDefault' contents/ ; grep -rl 'ResiliencePipelineRegistry' contents/
```

**So the highest-demand item in this spec is not a new guide. It is an edit to an existing
How-to.** A spec that only knows how to add pages would have written a twelfth resilience page
beside eleven that already discuss `UseResiliencePipeline`.

### 3.3 The DLQ cluster is real, spans transports, and has no task-shaped route

Four askings across 2022–2025 and two different transports. Today **21 pages mention a dead-letter
queue** — every transport documents its own settings, `HandlerFailure.md` (481 lines,
`Explanation`) explains the semantics and `ErrorHandlingOptions.md` (228 lines, `Reference`) lists
the options. A reader with a poison message must assemble the route themselves.

Note that Brighter issue [#3808](https://github.com/BrighterCommand/Brighter/issues/3808) is a
*documentation* issue in this cluster — *"Emphasize that an Uncaught Exception in a Handler will
Ack. Ack will therefore Drop on Fail"* — and that Spec 012's phase 10 found `HandlerFailure.md`'s
nack table both incomplete and flattening a material difference (six of ten nacks print `No-op`,
and three of those six **discard the message** while three leave it on the channel). **The
explanation this guide would link to has known defects in exactly the area the guide covers**, so
the design phase must decide whether 013 repairs them or links around them.

### 3.4 The committed guide is still owed, and still has nothing standing in for it

Three independent askings, one of them the issue this programme answers. **Verified today:**

- `PostgreSQLMessageBroker.md` carries a `### Using the Outbox Pattern` subsection at line 360 — a
  code *sketch* ending in *"See [Outbox Pattern] and [PostgreSQL Outbox] for more details"*.
- `PostgresOutbox.md` names a broker exactly once, in its opening sentence, generically.
- Four pages mention both halves; **none composes them.**

Q&A [#3795](https://github.com/BrighterCommand/Brighter/discussions/3795) is what the composition
failure looks like in the field: `InvalidOperationException: No Async outbox defined` from
`OutboxSweeper.SweepAsync`, resolved by setting `ConnectionProvider` and `TransactionProvider` on
`AddProducers`. That is the guide's verification step, supplied by a reader.

This is also the shape 009 met on rung 3 — a Postgres outbox that starts cleanly, provisions, and
throws at the first `GetRequiredService<IAmACommandProcessor>()` because
`IAmARelationalDatabaseConfiguration` was never registered. **The guide must show the registration
and say what happens when it is missing.**

### 3.5 Two things the README proposed that the measurement does not support

- *"Add health checks for transport and Outbox"* — `HealthChecks.md` is **84 lines with two `##`
  headings and mentions neither the Outbox nor a transport**, so the gap is real. But demand is
  **one asking, from 2021** (#1539). P2, on demand grounds, not on gap grounds.
- *"Run Brighter with no external broker (in-memory, for tests)"* — `InMemoryOptions.md` (356
  lines, **How-to**) and `TestDoubleOptions.md` (272 lines, **How-to**) already do this. **Closed.**

---

## 4. Target state

A reader who arrives with one of the four demonstrated problems above finds a page that:

1. States the problem in the reader's words, not the framework's
2. Lists prerequisites as links, and assumes competence beyond them
3. Gives numbered steps with complete, compiling code
4. Ends with a **verification step** — what to run, and what a reader sees when it worked
5. Names the failure they will hit if they skip a step, with the exception text
6. Links out to reference and explanation rather than restating either

Clause 5 is not decoration. Three of the four clusters were *diagnosed from an exception message*
in the thread that raised them, and a reader searching that message is the likeliest arrival path.

---

## 5. Target audience

**Intermediate.** A how-to assumes the reader knows why they want the thing. Newcomers are served
by Spec 009's four-rung ladder, which this spec links to and must not duplicate. Where a guide
composes two subjects, assume competence in **neither** — the reader knows their goal, not both
halves.

---

## 6. Source material

| Source | Use |
|---|---|
| Brighter Q&A discussions (55 threads) and issues | Demonstrated demand, the reader's own words, and the exception text they searched for |
| `contents/FAQ.md` (27 questions) | An entry that keeps being asked is a guide that does not exist |
| The existing 157 pages | Most guides compose two or three; **link, never copy** |
| Spec 012's option tables (59 tables, 519 rows) | Linked, never restated — §8 |
| `../Brighter/samples/` | Working code to point at; a new sample is authorised **per PR** |
| `../Brighter` at `10.7.0` via `git show <ref>:<path>` | The API a guide claims exists |

---

## 7. Scope

Priorities are set by **demonstrated demand first, public commitment second, structural gap
third** — in that order, because the README's risk section binds this spec to demand.

### P0 — must have

| # | Deliverable | Why P0 | Shape |
|---|---|---|---|
| **P0-1** | **Use PostgreSQL for both transport and Outbox** | Publicly committed on #67 **twice** (2026-07-18 and the 2026-09-04 comment), 3 independent askings, nothing composes it | **New page** |
| **P0-2** | **Resilience pipelines on an async handler** — `UseResiliencePipelineAsync`, the registry keys, `AddBrighterDefault`, and the non-generic `KeyNotFound` trap | 5 askings, the largest cluster; a public API on **0 of 157 pages**; the maintainer calls configuration *"one of our biggest weaknesses"* | **Edit** to `PolicyRetryAndCircuitBreaker.md` |
| **P0-3** | **Handle a poison message and route it to a DLQ** | 4 askings, 2 transports, 2022–2025; 21 pages carry fragments and none carries a route | **New page**, plus a decision on §3.3's known defects |

### P1 — should have

| # | Deliverable | Why P1 |
|---|---|---|
| **P1-1** | **Put a large payload behind a claim check** | The one surviving gap from 011's four; FAQ *"How do I handle large messages?"*; `ClaimCheck.md` is 75 lines of Explanation with no steps |
| **P1-2** | **Use MSSQL for broker, Inbox and Outbox** | The *other* half of #3960's stated goal, and the second instance of the composition family. Its cost is low once P0-1 exists; its value is establishing that the pattern generalises |

### P2 — nice to have

| # | Deliverable | Why only P2 |
|---|---|---|
| **P2-1** | Health checks for transport and Outbox | Real gap (`HealthChecks.md` names neither), but one asking, from 2021 |
| **P2-2** | EF Core transactions with the Outbox (#3897) | `EFCoreOutbox.md` covers it in 98 lines; likely a clarifying edit, not a page |
| **P2-3** | An index or *Guides* section | Contingent on the count — see §10 and §13 Q2 |

### Recorded as closed, not written

Written down so no later phase re-opens them, each with its evidence: in-memory for tests
(`InMemoryOptions.md`, `TestDoubleOptions.md`), retry and circuit-break as such
(`PolicyRetryAndCircuitBreaker.md` — P0-2 is the *async* gap only), replay after an outage
(`TurningOnReplayOnSeen.md`, 487 lines, How-to — **but see §11.4, the feature is unreleased**),
provisioning schemas (`BoxProvisioning.md` + two How-to children), V9→V10 migration
(`V10MigrationGuide.md`, 910 lines, How-to), OpenTelemetry tracing (`ConfiguringOpenTelemetry.md`,
How-to), routing several types down one channel, and writing a query and its handler.

---

## 8. Out of scope

- **Restating reference material.** A guide links Spec 012's tables; it does not copy them. A
  copied table is drift with a checker that cannot see it — `optioncheck` binds a marker to a
  type, and a second unmarked copy of a table is invisible to it.
- **Teaching fundamentals.** Spec 009 owns the ladder.
- **Features that do not exist.** Saga (#3583) and AOT (#3701) are product questions, not
  documentation gaps.
- **Speculative compositions.** Every transport × every outbox is hundreds of pages. Nothing is
  written without an asking behind it.
- **Any change to the `/spec:*` commands.** That is Spec 014, queued behind this one. Friction met
  here is recorded as its evidence — §14.

---

## 9. Documentation deliverables

| File | Action | Type | Priority |
|---|---|---|---|
| `contents/PostgresForTransportAndOutbox.md` *(name to settle at design)* | create | How-to | P0-1 |
| `contents/PolicyRetryAndCircuitBreaker.md` | edit | How-to (unchanged) | P0-2 |
| `contents/HandlingPoisonMessages.md` *(name to settle at design)* | create | How-to | P0-3 |
| `contents/ClaimCheckLargePayloads.md` *(name to settle at design)* | create | How-to | P1-1 |
| `contents/MSSQLForTransportAndBoxes.md` *(name to settle at design)* | create | How-to | P1-2 |
| `SUMMARY.md` | edit | — | every created page |
| `spec/011-authoring_conventions/pagetypes.tsv` | append one row per new page | — | standing obligation 7 |

**Filenames are provisional.** A page's filename **is its published URL slug** — 010 established
that the slug is filename-derived, falling back to the `SUMMARY.md` title only when no file
exists — so naming is a design decision with a permanent consequence, not a formatting one.

---

## 10. SUMMARY.md changes

Per 010 design §6.2, **a how-to lives beside its subject**, and a cross-cutting guide goes where a
reader would look first and is linked from the other. Proposed:

| Page | Section | Placement | Linked from |
|---|---|---|---|
| P0-1 PostgreSQL both | *Transports* | nested under `PostgreSQLMessageBroker.md` | `PostgresOutbox.md` |
| P0-3 Poison messages | *Using an External Bus* | nested under `HandlerFailure.md`, beside `ErrorHandlingOptions.md` | each transport's DLQ section |
| P1-1 Claim check | *Using an External Bus* | nested under `ClaimCheck.md`, beside `S3LuggageStore.md` | `FAQ.md`'s large-messages answer |
| P1-2 MSSQL all three | *Transports* | nested under `MSSQLMessageBroker.md` | `MSSQLOutbox.md`, `MSSQLInbox.md` |

**Nesting is deliberate and it is the cheap kind.** All four are sub-topic nestings under a page
they elaborate, which `CLAUDE.md` notes absorbs detail without touching the top-level count — and
010's phase 7 measured exactly this: four nested pages moved `--check-shape`'s widest **not at
all**. A nested page's URL gains a segment, so this must be settled before publication, never
after.

**The *Guides* section stays unbuilt at this count.** 010 set the test at three or more genuinely
cross-cutting guides. P0-1, P0-3 and P1-2 are three — but P0-3 is arguably filed under its subject
rather than cross-cutting, and creating a section for two would violate S1 on the day it landed.
**§13 Q2 puts it to review rather than deciding it here.**

---

## 11. Constraints

1. **`CLAUDE.md` is the authority**, and it wins over any `/spec:*` command until 014 runs. Every
   page carries: `description:` front matter with `layout.description.visible: false`, one H1, a
   banner (`> **How-to** · Applies to **Brighter V10**` plus Prerequisites), `##` headings
   qualified by subject, an opening sentence ≤200 **rendered** characters that is unique across
   the corpus, and a language tag on every fence.
2. **Code compiles as printed.** 009's method, and the only instrument that has ever found a
   published example that does not: extract the page's own fences into a project and build them,
   with **`<ImplicitUsings>disable</ImplicitUsings>`**, because the question is whether the page
   *as printed* compiles. Reflection cannot answer it.
3. **Rule 6 is held off by placement.** A guide is new, so every block in it is 100% added lines
   and strict under `--changed`; each therefore carries its real `using` directives. Where P0-2
   edits an existing page, **the edited block becomes strict** and earns real directives — that is
   a budgeted cost, not a surprise.
4. **A page must not silently document an unreleased feature.** Replay On Seen is absent from
   `10.7.0` and present on Brighter `origin/master`; five pages carry a *not yet released*
   blockquote. Any guide touching it inherits that obligation, and the trigger for removing it is
   `versioncheck.py` going red.
5. **Link, do not copy** — configuration values come from Spec 012's tables by link.
6. **Verify a claim at the ref.** `git show 10.7.0:<path>`. A type name assembled from surrounding
   vocabulary is this programme's most-repeated defect: `RequeueAction`, `AwsMessagingGatewayConnection`
   and `#transport-support-matrix` were all plausible, all invented, and all invisible to every gate.
7. **A namespace claim is checked by nothing here.** Every `using Paramore.*` line a guide prints
   gets `git grep "namespace X" 10.7.0 -- src/` **with a control**, because a zero is also what a
   broken grep returns.
8. **A sample in `../Brighter/samples/` is authorised per PR**, always by pull request, and every
   new project is registered in `Brighter.slnx` in the same PR.

---

## 12. Acceptance criteria

Each names its instrument, and **the three with no tool are marked** — 009's AC7 and 012's AC1
were both unmet at the close, both on corpora green under every gate, and both were the criterion
with nothing mechanical behind it.

| # | Criterion | Instrument |
|---|---|---|
| **AC1** | Every P0 deliverable ships, or is struck with a recorded reason | walked — **no tool** |
| **AC2** | Every new page is linked from `SUMMARY.md` and no page is orphaned | `linkcheck.py` |
| **AC3** | Every internal link resolves, including anchors | `linkcheck.py` |
| **AC4** | Every new page passes all seven page rules | `pagelint.py`, and `--changed origin/master` |
| **AC5** | Every C# block on a new page compiles as printed | the harness in §11.2 — **no tool in CI** |
| **AC6** | Shape and redirects hold; nested pages move no existing URL | `urlmap.py --check-shape`, `--check-redirects`, `--verify` after publication |
| **AC7** | Every guide ends with a verification step naming what the reader sees | walked — **no tool** |
| **AC8** | `pagetypes.tsv` has a row per new page | walked — **no tool reads this file** |
| **AC9** | Each guide traces to an asking, cited by number | walked — **no tool** |

**AC9 is walked backwards, not forwards.** Forwards — every guide has a citation — can only ever
find guides that were written. Backwards — every cluster in §3.1 with two or more askings against
the delivered set — is what finds the one nobody planned. That is precisely how 012's AC1 failure
was found, one command, at the last possible moment.

---

## 13. Open questions for the maintainer

1. **Is the P0 list right?** It is set by demand, so it promotes **poison messages / DLQ** (4
   askings) above the claim check (1 asking + a structural gap), and it demotes half the README's
   proposals to *closed*. The README's ordering was written before any of this was measured.
2. **Does a *Guides* section get built?** 010 set the test at three or more cross-cutting guides
   and made it 013's call. On this list the count is two or three depending on whether P0-3 counts
   as cross-cutting. Recommendation: **no section**, file beside subject per §10, revisit if a
   fourth arrives.
3. **Does P0-3 repair `HandlerFailure.md`'s nack table, or link around it?** §3.3: the explanation
   it links to has known defects in the guide's own subject area. Repairing is in this spec's
   spirit and outside its stated scope.
4. **Does any guide get a compiled sample in `../Brighter/samples/`?** P0-1 is the shape that wants
   one — a composition is exactly what rots inside a markdown fence. A write there needs
   authorisation per PR.

**Two things found while measuring, needing a decision but not blocking this document:**

- **[Docs#70](https://github.com/BrighterCommand/Docs/issues/70) is OPEN and its defect is
  fixed.** *"AzureBlobConfiguration.md opens by describing Azure Service Bus"* — repaired by 010's
  `49ea480`; the page now opens *"The Azure Archive Provider writes messages swept from your
  Outbox into an Azure Blob Storage container."* It can be closed with that citation. **Not done —
  closing someone's issue is outward-facing and needs your say-so**, which is the lesson 012's
  task 11.4 bought.
- **`contents/EFCoreOutbox.md:56` ends `...of these APIs).x`** — a stray character, published,
  invisible to all seven gates. One-line fix, wants a home in a PR rather than one of its own.

---

## 14. Workflow friction — Spec 014's evidence

Recorded as met, per 014's README instruction to record rather than route around.

1. **`/spec:requirements` never asks which page types a topic needs.** 014's README predicted
   this; the sharper finding is that its template would have hidden P0-2, because **its highest-
   demand deliverable is an edit to an existing How-to, not a new page.** The command's
   *"Documentation deliverables — specific files to create or update"* does technically admit an
   edit; nothing in it prompts you to look for one.
2. **The command asks for neither acceptance criteria nor open questions.** Measured across the
   nine commands' **451 lines**: `grep -ril 'acceptance criteri'` → **0 files**;
   `grep -ril 'open question'` → **0 files**. Every spec in this programme has both, §12 and §13
   exist here only because previous specs' documents were copied, and **both criteria found unmet
   at a close (009's AC7, 012's AC1) were criteria the workflow never asked for.** This is beyond
   014's listed defect 5, which is about the acceptance *pass*.
3. **Its Research Steps are feature-shaped.** *"Look for relevant ADRs… check release notes…
   review source code and samples for the feature"* fits a spec documenting a new feature. 013's
   demand lives in **discussions, issues and the FAQ**, and the command names none of them —
   though this spec's own README does. A requirements phase run literally would have produced an
   intuition-ordered list.
4. **Nothing warns that an inherited gap list rots.** §2 found five of six gaps closed by later
   specs. The re-derivation happened only because this spec's README and `PROMPT.md` both say so
   in prose that no command reads.
