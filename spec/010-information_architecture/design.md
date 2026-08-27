# Spec 010: Information Architecture — Design

**Created:** 2026-08-07 · **Status:** Draft, for review
**Works from:** `requirements.md` (approved 2026-08-06), §16 *D0 as executed* (2026-08-07)
**Executes against:** `spec/011-authoring_conventions/worklist.md` (42 rows, 26 `split`, 16 `keep`)

---

## 1. What this design decides

The requirements left ten questions open and deliberately estimated nothing. This
document closes nine of them, and every figure in it was **derived from the corpus, not
predicted**:

| Decision | Answer | Derived how |
|---|---|---|
| The target tree (Q2) | **12 sections**, down from 19 | §3, and every one of the 110 pages is placed |
| Section-size threshold (Q8) | **≤ ~~12~~ 20 top-level entries per section, ≥ 2 pages, ≤ ~~3~~ 4 URL segments** | §4 — both ceilings amended after measurement; see the S2 and S3 rows |
| URL churn (Q3) | **74 of 110 URLs move; 36 do not** | measured, §5 |
| Pages the 26 splits create | **32**, taking the corpus 110 → **142** | §7, row by row |
| Anchor links to repoint | **34 inbound anchor links on 7 of the 26 pages**; 19 pages have none | measured, §8 |
| `llms.txt` summaries (Q9) | **Extracted from the page's own first sentence**, and the build fails when that sentence is unusable | §9.2 |
| `urlmap.py` in CI (Q10) | **Yes**, three checks; and it owns AC6's threshold | §9.1 |
| PR order (Q4) | **Restructure first, splits second**, one PR per family | §10 |
| `ReplayOnSeen.md`'s core (Q5) | **The Explanation is the core** and keeps the filename | §7.4 |
| `Requests, Commands and Events.md` (Q6) | **Dropped** — the rename makes the URL worse | §6.3 |

**Nothing here contradicts a `worklist.md` verdict.** Five *shapes* are executed
differently from the shape column, each with the arithmetic that forced it; those five
are collected in §7.7 so a reviewer can overrule them in one place.

---

## 2. Reading order for the reader this is for

The tree is ordered as a journey, and the ordering is the design as much as the grouping
is:

```text
Get Started                      →  "Is this for me, and can I see it work?"
Commands, Handlers and Pipelines →  "I am writing handlers."
Brighter Configuration           →  "How do I wire it up?"
Using an External Bus            →  "Now send it somewhere."
Transports                       →  "I am on Kafka." / "I am on RabbitMQ."
Outbox and Inbox                 →  "Make it survive a crash."
Scheduler                        →  "Send it later."
Darker                           →  "Now the query side."
Health Checks and Observability  →  "Is it healthy in production?"
V10 Migration                    →  "I am upgrading."
Understanding Brighter           →  "Why does it work like that?"
Reference                        →  "What does this word mean?"
```

Three ordering rules fall out of it, and they are what a future page should be filed by:

1. **Doing precedes understanding.** *Understanding Brighter* is eleventh, not second.
   Explanatory material is currently scattered across five sections at the bottom of the
   tree; this gathers it in one place at the bottom rather than promoting it.
2. **Technology families group by technology, never by mode.** A reader on Kafka wants
   configuration, gotchas and limits in one place. That is the decision already taken and
   stated publicly on #67, and it is why there is no `Reference` / `How To` /
   `Explanation` top level.
3. **A how-to lives beside its subject.** A how-to extracted from `Telemetry.md` sits
   under *Health Checks and Observability*, not in a bucket. This is requirements §10, and
   it is also §6.2's answer to "how does *How To* avoid becoming the new *Outbox and
   Inbox*": **there is no *How To* section.**

---

## 3. The target tree

### 3.1 File hierarchy — after the restructure, before the splits

This is deliverable **D1** exactly as it will be written, and it is the 110 pages that
exist today. Nothing is added, nothing is dropped, no file moves on disk.

```text
Get Started (3)
  Why Brighter? · Basic Concepts · Show me the code!

Commands, Handlers and Pipelines (15)
  Requests, Commands and Events
  Dispatching Requests
    └ Dispatching an Async Request · Returning Results from a Handler
  How to Implement a Request Handler
    └ How to Implement an Async Request Handler
  Building a Pipeline of Request Handlers
    └ Building an Async Pipeline · Passing Information Between Handlers
    └ Pipeline Validation and Diagnostics · Request Validation · Feature Switches
    └ Supporting Retry and Circuit Breaker · Failure and Fallback
  Agreement Dispatcher

Brighter Configuration (6)
  Basic Configuration
    └ Command Processor Configuration Reference · Dispatcher Configuration Reference
  InMemory Options · Test Double Options · Analyzer Support

Using an External Bus (12)
  Using an External Bus · Routing
  Message Mappers └ Default Message Mappers
  Cloud Events Support
  Claim Check └ S3 Luggage Store
  Compression · Dynamic Message Deserialization · AsyncAPI Document Generation
  Error Handling └ Error Handling Options

Transports (9)
  RabbitMQ Configuration
    └ RabbitMQ Durability · Migrating to Quorum Queues · RabbitMQ Connection Stability
  Kafka Configuration · AWS SNS and SQS Configuration
  Azure Service Bus Configuration · PostgreSQL Message Broker · Brighter Control API

Outbox and Inbox (32)
  Outbox Support        └ 8 stores
  Azure Blob Archive Provider └ Azure Archive Provider Configuration
  Sweeper Circuit Breaking
  Inbox Support         └ 6 stores
  Replay On Seen · Causation Tracking in a Custom Store
  Distributed Lock      └ 7 locks
  Box Provisioning      └ Configuring Box Provisioning · Upgrading Existing Deployments

Scheduler (8)
  Scheduler └ InMemory · Hangfire · Quartz · TickerQ · Aws · Azure
  Custom Scheduler

Darker (5)
  Darker Basic Configuration · Queries and Query Objects
  How to Implement a Query Handler · Query Pipeline and Decorators · Query Patterns

Health Checks and Observability (4)
  Logging · Monitoring · Health Checks · Telemetry

V10 Migration (2)
  V10 Migration Guide · Nullable Reference Types

Understanding Brighter (12)
  Command, Processor and Dispatcher Patterns · Using a Task Queue
  Microservices · Event Driven Collaboration · Event Carried State Transfer
  Outbox Pattern · CQRS with Brighter and Darker
  How the Command Processor Works └ How Configuring the Command Processor Works
  How the Dispatcher Works        └ How Configuring a Dispatcher for an External Bus Works
  Reactor and Proactor: Concurrency Models

Reference (2)
  Glossary · FAQ
```

**Verified by construction, not asserted:** 110 pages in, 110 pages out, no page dropped,
no page added, maximum URL depth **3 segments**.

### 3.2 Where the 19 sections went

| Today | Pages | Becomes | Why |
|---|---:|---|---|
| Overview | 3 | **Get Started** | "Overview" names a shape, not an intent, and 009's tutorials need a home |
| Brighter Configuration | 9 | **Brighter Configuration** (6) | Name kept. `PipelineValidation.md` → *Commands, Handlers and Pipelines*; the two `HowConfiguring…Works.md` explanations → *Understanding Brighter* |
| Darker Configuration | 1 | **Darker** | A singleton section; merged with the other Darker section |
| Brighter Request Handlers and Middleware Pipelines | 14 | **Commands, Handlers and Pipelines** | Six words of jargon → four words a newcomer parses |
| Darker Query Handlers and Middleware Pipelines | 4 | **Darker** | Q2 answered: Darker becomes one top-level section |
| CQRS Patterns | 1 | **Understanding Brighter** | Singleton |
| Using an External Bus | 12 | **Using an External Bus** | Name kept — it already states an intent |
| Guaranteed At Least Once | 11 | **Transports** (9) | The flagship rename. Accurate about semantics, useless as a signpost. The two Azure Blob *archive* pages were misfiled here and move to *Outbox and Inbox* |
| Outbox and Inbox | 27 | **Outbox and Inbox** (32) | Name kept — it is the biggest section, so keeping the name saves the most URLs. Absorbs *Database Provisioning* and the archive pages, and nests its 14 stores |
| Database Provisioning | 3 | **Outbox and Inbox** | Provisioning creates outbox and inbox tables; it is not a subject of its own |
| Health Checks and Observability | 4 | unchanged | |
| Scheduler | 8 | **Scheduler** | Name kept |
| V10 Migration | 2 | unchanged | |
| Commands, Processors and Dispatchers | 1 | **Understanding Brighter** | Singleton |
| Event Driven Architectures | 4 | **Understanding Brighter** | |
| Task Queues | 1 | **Understanding Brighter** | Singleton |
| Under the Hood | 3 | **Understanding Brighter** | Singleton-adjacent, and the leading-space hazard at `SUMMARY.md:154` disappears with the heading |
| Reference | 1 | **Reference** (2) | Singleton; absorbs FAQ |
| FAQ | 1 | **Reference** | Singleton |

**The `SUMMARY.md:154` hazard is retired by construction.** The ` ## Under the Hood`
line with its unique leading space is deleted, not normalised — the section no longer
exists. P0-1 still asserts that no heading in the new file carries leading whitespace,
because the hazard is the class, not the instance.

---

## 4. Q8 — the threshold that makes AC6 checkable

AC6 reads *"No section holds one page; no section is unnavigable without a middle
layer"*, which the review correctly called unfalsifiable. The replacement is three
mechanical rules over `SUMMARY.md`:

| # | Rule | Rationale |
|---|---|---|
| **S1** | Every section holds **at least 2 pages** | A section of one is not a category. Six exist today |
| **S2** | Every section holds **at most ~~12~~ 20 top-level entries** | ~~This is the number the navigation shows.~~ **Amended 2026-08-27 — spec 012 design §9.2.** That rationale was a claim about GitBook with nothing behind it, and the measured table below says our widest section was **10**, so 12 was our own data plus two. GitBook's own 182-page docs also top out at **10**, by nesting — so no platform limit is established at any number and S2 is an **editorial budget**. Nesting is still the escape hatch, and §3.1 of the requirements is why: a middle layer needs a real page to hang it from |
| **S3** | No published path exceeds **4 segments** | **Amended 2026-08-08 — see §17.** Four was measured on the live site, not assumed. It was 3, on the grounds that three is the deepest the site was *known* to work at; that was an evidence boundary rather than a platform limit, and it was distorting two placements |

**S2 counts entries, not pages, and that is the whole point.** *Outbox and Inbox* holds
32 pages and is perfectly navigable, because 21 of them are stores nested under three
parent pages and the reader sees **8 entries**. The failure the requirements named — 27
flat pages — was never about the total; it was about the flat list.

Measured against the tree in §3.1:

| Section | Pages | Top-level entries |
|---|---:|---:|
| Get Started | 3 | 3 |
| Commands, Handlers and Pipelines | 15 | 5 |
| Brighter Configuration | 6 | 4 |
| Using an External Bus | 12 | 9 |
| Transports | 9 | 6 |
| Outbox and Inbox | 32 | 8 |
| Scheduler | 8 | 2 |
| Darker | 5 | 5 |
| Health Checks and Observability | 4 | 4 |
| V10 Migration | 2 | 2 |
| Understanding Brighter | 12 | 10 |
| Reference | 2 | 2 |

S1 ✅ (minimum 2) · S2 ✅ (maximum 10 — **and that 10 is where S2's 12 came from**, see the
row above) · S3 ✅ (maximum 3). After all 32 split pages land
the worst case is *Understanding Brighter* at **10** entries and *Outbox and Inbox* at 39
pages / **9** — still inside S2 with two entries of headroom, and the deepest path is
**4 segments**, reached by exactly two pages. Those figures are pinned per split in
`tasks.md` Appendix A, which is where §15's *not measured* admission about this column was
discharged; §7.6's column is superseded by it.

**S1–S3 become code**, in `tools/urlmap.py --check-shape`, and gate CI — see §9.1. A rule
a reviewer has to apply by eye is the kind of rule this programme has watched decay.

---

## 5. The URL effect, measured

The tree in §3.1 is written out in full as
**`spec/010-information_architecture/SUMMARY.target.md`** — the exact file PR 2 installs,
kept beside this document so every figure below can be reproduced rather than believed.

Running the validated predictor over the current `SUMMARY.md` and that file:

| | |
|---|---:|
| Pages | 110 → 110 |
| Published paths that **change** | **74** |
| Published paths **unchanged** | **36** |
| Pages dropped from the tree | **0** — the predictor exits 1 if any is |
| Maximum path depth | 3 segments |

Reproduce it — this is D3's own diff, run early:

```bash
python3 - <<'PY'
import importlib.util
s = importlib.util.spec_from_file_location(
    "urlmap", "spec/010-information_architecture/urlmap.py")
m = importlib.util.module_from_spec(s); s.loader.exec_module(m)
old = m.published_paths(open("SUMMARY.md").read())
new = m.published_paths(open(
    "spec/010-information_architecture/SUMMARY.target.md").read())
of = {v: k for k, v in old.items()}; nf = {v: k for k, v in new.items()}
print("moved", sum(1 for f in of if nf.get(f) not in (None, of[f])))
print("unchanged", sum(1 for f in of if nf.get(f) == of[f]))
PY
```

**36 unchanged URLs is a design output, not luck.** Seven of the twelve section names are
deliberately unchanged — *Brighter Configuration*, *Using an External Bus*, *Outbox and
Inbox*, *Scheduler*, *Health Checks and Observability*, *V10 Migration*, *Reference* —
and that is the answer to Q3.

### 5.1 Q3 — which renames are worth their URL churn

A section rename moves every URL beneath it. So each was judged on whether the name
misleads a reader, never on how it reads:

**Renamed** — the name actively misdirects:

- *Guaranteed At Least Once* → **Transports**. A delivery guarantee filed as a product
  category. 11 URLs, and the clearest gain in the tree.
- *Overview* → **Get Started**. Names a position in the file, not a reader intent.
- *Brighter Request Handlers and Middleware Pipelines* → **Commands, Handlers and
  Pipelines**.
- The five sections that collapse into **Understanding Brighter**, and the two that
  collapse into **Darker** — those pages move whatever the new section is called.

**Kept** — the name already states the intent, so the churn buys nothing:

- *Using an External Bus* (12 pages), *Outbox and Inbox* (27), *Scheduler* (8),
  *Brighter Configuration* (9), *Health Checks and Observability* (4), *V10 Migration*
  (2), *Reference*.

Note that **retitling a link's display text is free** — the URL derives from the
filename, never from the label. That is what lets `HowServiceActivatorWorks.md` keep a
filename carrying a V9 term while its label reads *How the Dispatcher Works*, and it is
why the labels in §3.1 are tidied throughout at zero URL cost.

---

## 6. The three placement rules new pages are filed by

### 6.1 A split page sits beside the page it came from

Requirements §10, restated because it is load-bearing for all 32 new pages. A how-to
extracted from `Telemetry.md` goes under *Health Checks and Observability*; a reference
extracted from `CloudEventsSupport.md` goes under *Using an External Bus*.

### 6.2 There is no *How To* section, and 013 does not get one either

The README's seven buckets put roughly 40 pages into *How To* once Spec 013's guides
arrive — the *Outbox and Inbox* problem under a new name, which requirements §3.1 already
identified. **This design does not create the section.** The rule 013 inherits:

> A how-to lives with its subject. A how-to that genuinely spans two or more subjects —
> the publicly committed *PostgreSQL for both transport and outbox* guide is the example
> — goes wherever a reader would look first, and links from the other. Only if 013
> accumulates **three or more** genuinely cross-cutting guides does a *Guides* section
> become worth its own place in the tree, and that is 013's call, not this one.

Creating an empty section now would violate S1 on the day it landed.

### 6.3 Q6 — `Requests, Commands and Events.md` is **not** renamed

P2-2 proposed renaming the corpus's one awkward filename. **Dropped**, on a fact that
only turns up when you run the slug function:

| Filename | Published slug |
|---|---|
| `Requests, Commands and Events.md` (today) | `requests-commands-and-events` |
| `RequestsCommandsAndEvents.md` (PascalCase, as `CLAUDE.md` requires) | `requestscommandsandevents` |

The current filename produces the **better** URL. The rename would trade a readable,
hyphenated, already-indexed path for an unreadable one, and buy a single line of
`SUMMARY.md` that no longer needs `%2C%20`. `Requests-Commands-And-Events.md` would
preserve the URL exactly, but breaks the PascalCase convention in `CLAUDE.md`.

Neither trade is worth taking. The file keeps its name and its URL; the encoded link in
`SUMMARY.md` stays, where it has worked since the site was published.

---

## 7. The 26 splits — file-by-file

Every line count below is measured from the file today (`## `-section spans, so they sum
to the page). **Types are proposals to confirm against the moved text when the page is
written**, since a banner type is a judgement `--fix` will never make for you.

### 7.1 The scheduler family — §5a, one decision applied six times

`worklist.md` §5a resolves the family to *five Reference cores, one shared how-to, one
enriched overview*. Two new pages:

| New page | Type | Source | Lines |
|---|---|---|---:|
| `SwitchingSchedulers.md` | How-to | The five `… Migration from Other Schedulers` sections | 36 + 52 + 38 + 61 + 34 = **221** |
| `SchedulingAMessage.md` | How-to | `BrighterSchedulerSupport.md` `## Brighter Scheduler Code Examples` (167) + `## Brighter Scheduler Configuration Examples` (45) | **212** |

`SwitchingSchedulers.md` outline:

```text
# Switching Schedulers
> **How-to** · Applies to **Brighter V10** · Prerequisites: [Scheduler](/contents/BrighterSchedulerSupport.md)

## Why You Would Switch Schedulers
## Switching Schedulers: What Changes and What Does Not     (the factory, and nothing else)
## Switching to Hangfire / Quartz / Aws / Azure / InMemory  (H3 per target, before/after pair each)
## Switching Schedulers Verification
## Further Reading
```

The five per-page migration sections are near-copies — `HangfireScheduler.md:751` and
`AwsScheduler.md:692` differ only in which factory they name — so the matrix collapses to
one before/after per target under a shared preamble. Code moves verbatim; blocks that
cannot carry their `using` directives mark the omission `// ...`.

**The `Comparison` sections fold up by merger, not by concatenation.**
`BrighterSchedulerSupport.md` already carries `## Choosing a Scheduler` (97 lines). The
comparison sections say what it already says; they are folded *into* it and the duplicate
prose is dropped, which is the one place in this spec where text is removed rather than
moved — and it is removed as **duplication**, not as information. The
no-information-loss check (D5) is run against the union of those sections and
`## Choosing a Scheduler`, not against each separately.

**There are four such sections, totalling 85 lines, not six totalling 96.** Corrected at
design review, and the correction matters because a name match is what produced the wrong
figure:

| Page | Section | Lines |
|---|---|---:|
| `HangfireScheduler.md` | `## Comparison: Hangfire vs Quartz` | 26 |
| `AwsScheduler.md` | `## AWS Scheduler Comparison with Other Schedulers` | 23 |
| `AzureScheduler.md` | `## Azure Scheduler Comparison with Other Schedulers` | 24 |
| `InMemoryScheduler.md` | `## Comparison with Production Schedulers` | 12 |
| | **Total** | **85** |

`QuartzScheduler.md` has no comparison section at all, and `AwsScheduler.md` has a second
heading matching the word — **`## Scheduling Modes Comparison` (11 lines), which stays in
the AWS Reference core.** It compares AWS's *own* two scheduling modes, direct-to-target
against `FireAwsScheduler`; it is not a comparison between schedulers and has no business
in `## Choosing a Scheduler`. `worklist.md` §5a and §6a both say "six", and they are wrong
on the count for the same reason.

**Deviation, with the arithmetic — see §7.7 item 1.** The `HangfireScheduler.md` row's
shape column also folds each page's `Overview` and `How Brighter Integrates` sections up
into the overview. Those five pairs total **278 lines**. `BrighterSchedulerSupport.md` is
578 lines and sheds 212, leaving 366; adding 278 + 85 would return it to **729 lines** —
larger than any page the split drains, and larger than it is today. The per-technology
orientation therefore **stays on its own page**, where a reader who has already chosen
Hangfire is looking. Only the comparisons fold up, which is what §5a's own conclusion
says.

Per-page effect:

| Page | Today | After | Loses |
|---|---:|---:|---|
| `HangfireScheduler.md` | 832 | ~770 | Migration (36), Comparison (26) |
| `AwsScheduler.md` | 775 | ~700 | Migration (52), Comparison (23). **`## Scheduling Modes Comparison` (11) stays** — it compares AWS's own modes, not schedulers |
| `QuartzScheduler.md` | 769 | ~731 | Migration (38). No comparison section |
| `AzureScheduler.md` | 717 | ~632 | Migration (61), Comparison (24) |
| `InMemoryScheduler.md` | 541 | ~495 | Migration (34), Comparison (12). **`## Important Warning` (16) and `## When to Use InMemory Scheduler` (63) stay** — it is the scheduler you must not ship |
| `BrighterSchedulerSupport.md` | 578 | ~400 | 212 out, ~35 net in after the merge |
| `TickerQScheduler.md` | 234 | 234 | **`keep`** — no migration section at all, `Best Practices` is 8 lines |

### 7.2 Transports — 3 new pages

| New page | Type | Source | Lines |
|---|---|---|---:|
| `PostgreSQLBrokerTradeOffs.md` | Explanation | `PostgreSQLMessageBroker.md`: `Benefits` (22), `When to Use` (19), `Limitations` (21), `JSON vs JSONB` (32), `Comparison with Other Transports` (16) | **110** |
| `AWSSQSMigrateToV10.md` | How-to | `AWSSQSConfiguration.md`: `V10 Migration Path` (196), `AWS SDK v4 Support` (49) | **245** |
| `InMemoryTransport.md` | Reference | `InMemoryOptions.md`: `## InMemory Transport` (118) — see §7.5 | **118** |

`AWSSQSConfiguration.md` is *"the exact RabbitMQ precedent"*: 245 lines of how-to at the
end of a reference page. The core keeps its name and its four `SQS *` sections and drops
615 → ~370.

**`## Transactional Messaging` (46 lines) stays in the PostgreSQL core** — §7.7 item 2.
Extracting it would produce a 46-line how-to that Spec 013 immediately supersedes: the
publicly committed *PostgreSQL for both transport and outbox* guide is exactly this
material, written properly. It is flagged to 013 rather than half-extracted here.

### 7.3 Darker — 12 new pages

**`QueryPatterns.md` — 1,291 lines, the largest page in the corpus.** Six independent
task-shaped recipes in one file; the core becomes a hub.

| New page | Type | Source section | Lines |
|---|---|---|---:|
| `ParameterizedQueryPatterns.md` | How-to | `Parameterized Query Patterns` | 270 |
| `PaginationQueryPatterns.md` | How-to | `Pagination Patterns` | 233 |
| `ProjectionQueryPatterns.md` | How-to | `Projection Patterns` | 173 |
| `AggregationQueryPatterns.md` | How-to | `Collection and Aggregation Patterns` | 162 |
| `EFCoreQueryIntegration.md` | How-to | `Entity Framework Core Integration` | 147 |

The hub keeps the introduction, `Performance Best Practices` (66),
`Real-World Example: Product Catalog Query` (197), `Best Practices Summary` and
`Common Pitfalls` — ~300 lines with a capstone worked example, not a stub. Guidance folds
into what it concerns (`worklist.md` §4 rule 3), and the performance material is
cross-cutting, so it belongs to the hub rather than to any one recipe.

| Page | New pages | Type | Source | Lines |
|---|---|---|---|---:|
| `ImplementAQueryHandler.md` (935 → ~646) | `TestingQueryHandlers.md` | How-to | `Testing Query Handlers` | 159 |
| | `QueryHandlerDependencies.md` | How-to | `Working with Dependencies` | 130 |
| `QueryPipeline.md` (928 → ~716) | `QueryPipelinePolicies.md` | How-to | `Configuring Polly Policies` | 159 |
| | `DarkerAndBrighterPipelines.md` | Explanation | `Comparison with Brighter Pipeline` | 53 |
| `QueriesAndQueryObjects.md` (877 → ~579) | `QueryResultTypes.md` | Explanation | `Query Result Types` | 179 |
| | `QueryObjectValidation.md` | How-to | `Validation in Query Objects` | 119 |
| `DarkerBasicConfiguration.md` (510 → ~435) | `DarkerConfigurationReference.md` | Reference | `Darker Configuration Options` | 75 |

`QueryPipelinePolicies.md` is §5b's finding executed: Brighter decomposes the same
subject across `BuildingAPipeline.md`, `PolicyRetryAndCircuitBreaker.md` and
`PolicyFallback.md` while Darker keeps it in one file, so splitting along Brighter's
existing seams restores the parallel. `DarkerAndBrighterPipelines.md` is thin at 53 lines
— `OutboxPattern.md` is 45 and `AzureBlobArchiveProvider.md` is 42, so it is not below
the corpus's floor. `DarkerConfigurationReference.md` at 75 lines parallels
`DispatcherConfigurationReference.md` (233) and is created for the parallel, not for the
size. `QueryPipeline.md` keeps `Available Decorators` (291) and `Decorator Patterns`
(162) because its worklist row says so; extracting the decorator reference is recorded in
§11 as a candidate for a later spec, not acted on.

**One verification task before anything moves.** `QueriesAndQueryObjects.md:746`
`## Query Patterns` (102 lines) versus the whole `QueryPatterns.md` page is a
**duplication flag, not a section** (`worklist.md` §7). Verify first; if it duplicates,
it is deleted and replaced with a link, and D5's no-information-loss check is run against
`QueryPatterns.md` as well as against the split originals. Task 4.5 of Spec 011 is why
this is a separate step: three specified "duplicate content" defects turned out not to
exist, and executing them as written would have destroyed correct material.

**Darker content is re-filed and split, never rewritten.** `../Darker` HEAD is ahead of
the deployed 4.1.1 and the site publishes the deployed version.

### 7.4 Q5 — `ReplayOnSeen.md`, and which mode is the core

1,039 lines, banner-typed Reference, three clean modes:

| Mode | Sections | Lines |
|---|---|---:|
| **Explanation** | `The Problem` (42), `How Replay Walks the Flow Forward` (59), `Causation Id` (94), `Why It Works This Way` (33) | **228** |
| How-to | `Turning It On` (122), `You Must Thread Your RequestContext` (85), `Before You Enable It` (64), `Upgrading Without Migrating` (67), `A Worked Example` (131) | **469** |
| Reference | `Store Support` (48), `When Replay Does Not Fire` (116), `Observability` (69), `Limitations` (78) | **311** |

**Ruling: the Explanation is the core, keeps `ReplayOnSeen.md`, and the banner changes
Reference → Explanation.** Two reasons, and the second is the stronger:

- A reader who searches "replay on seen" is asking what it is. The name is the concept.
- **`outbox-and-inbox/replayonseen` is one of the 36 URLs this restructure does not
  move.** Rule 1 of `worklist.md` §4 — the core keeps the filename — is what preserves
  it. Making the how-to the core would move the concept's URL to a page about switching a
  flag on.

New: `TurningOnReplayOnSeen.md` (How-to, 469) and `ReplayOnSeenReference.md` (Reference,
311). Both nest under `ReplayOnSeen.md`, so both publish at three segments.

### 7.5 The remaining §6d rows

| Page | Today → core | New pages | Type | Source | Lines |
|---|---|---|---|---|---:|
| `CQRSWithBrighterAndDarker.md` | 1,144 → ~935 | `CQRSUseCasesAndPatterns.md` | Explanation | `Use Cases and Patterns` | 209 |
| `NullableReferenceTypes.md` | 711 → ~447 | `MigratingToNullableReferenceTypes.md` | How-to | `Migration Guide` | 264 |
| `AgreementDispatcher.md` | 720 → ~400 | `AgreementDispatcherRouting.md` | Explanation | `Standard vs Agreement Dispatcher Routing` (74), `Use Cases` (146), `Limitations` (53), `Performance Implications` (47) | 320 |
| `PolicyRetryAndCircuitBreaker.md` | 687 → ~440 | `MigratingToPollyV8.md` | How-to | `Migration Guide: V9 to V10` (96), `Legacy: Using Polly v7 Policies (Deprecated)` (151) | 247 |
| `Telemetry.md` | 597 → ~390 | `ConfiguringOpenTelemetry.md` | How-to | `Configuring OpenTelemetry` (81), `Complete Configuration Example` (96), `Distributed Tracing Example` (30) | 207 |
| `DynamicMessageDeserialization.md` | 597 → ~281 | `RoutingMultipleMessageTypes.md` | How-to | `Using CloudEvents Type for Routing` (83), `Custom Routing Strategies` (63), `Handler Routing` (77), `Configuration Examples` (93) | 316 |
| `SweeperCircuitBreaking.md` | 527 → ~395 | `UsingSweeperCircuitBreaking.md` | How-to | `Usage Patterns` (61), `Advanced Scenarios` (71) | 132 |
| `BrighterOutboxSupport.md` | 517 → ~197 | `OutboxArchiver.md` | Reference | `Outbox Archiver` | 151 |
| | | `TransactionalMessagingWithTheOutbox.md` | How-to | `Complete Example: Transactional Messaging` | 169 |
| `CloudEventsSupport.md` | 475 → ~369 | `CloudEventsReference.md` | Reference | `CloudEvents Attributes` (34), `CloudEvents Across Transports` (72) | 106 |
| `MessageMappers.md` | 266 → ~147 | `MessageTransforms.md` | Explanation | `Transformers` (119) + `Transform Pipeline Example` from `DefaultMessageMappers.md` (145) | 264 |
| `DefaultMessageMappers.md` | 478 → ~333 | *(none — donates 145 lines)* | | | |
| `InMemoryOptions.md` | 695 → ~330 | `InMemoryTransport.md` · `InMemoryOutbox.md` · `InMemoryInbox.md` | Reference | see below | 118 / 79 / 68 |

**`CQRSWithBrighterAndDarker.md` keeps `## Example: E-Commerce Order System` (226
lines).** Requirements §8 is binding: 010 must not consume material Spec 009 needs, and
226 lines of end-to-end example is the closest thing the corpus has to a tutorial. The
core stays large at ~935 lines and drops to ~709 when 009 takes it. **Flagged, not
moved.**

**`MigratingToPollyV8.md` holds both tails.** The deprecated Polly v7 section is what a
reader migrates *from*; splitting them apart would leave a deprecated 151-line page with
no explanation of what replaces it, and 150 lines of deprecated material at the end of a
current page is the defect being fixed either way.

**`MessageMappers.md` — §5c's three-way break, and it carries a correctness fix.** The
default-mapper how-to §5c calls for **already exists**: `DefaultMessageMappers.md` (479
lines, already typed How-to). So §5c's first row is *establish it as the default route* —
a link and a pointer from `MessageMappers.md`, not a new page. `MessageTransforms.md`
must state that **transforms require a custom mapper** and form part of the pipeline; the
ruling was explicit that this is not currently clear, and a reader can today come away
believing transforms work with the default mapper. That sentence is the one piece of new
prose this spec authors.

**`InMemoryOptions.md` is a redistribution, not a mode split.** Five unrelated subjects,
each belonging beside its own family:

| Section | Lines | Goes to |
|---|---:|---|
| `InMemory Transport` | 118 | **new** `InMemoryTransport.md`, under *Transports* |
| `InMemory Outbox` | 79 | **new** `InMemoryOutbox.md`, under *Outbox Support* |
| `InMemory Inbox` | 68 | **new** `InMemoryInbox.md`, under *Inbox Support* |
| `InMemory Scheduler` | 53 | existing `InMemoryScheduler.md` |
| `InMemory Archive` | 47 | new `OutboxArchiver.md` (above) |

Three of the five family pages **do not exist** — §7.7 item 4. Every other transport has
a page (5 of them), every other outbox store has one (8), every other inbox store has one
(6); InMemory is the missing member of each set. Creating them is what makes "merge into
the matching family page" executable, and it fixes a real gap: a reader who wants the
in-memory outbox has no page to find. What remains keeps the name, retyped
**Reference → How-to**: a genuine testing guide of `Test Configuration Patterns` (42),
`Complete Testing Example` (99) and `Environment-Specific Configuration` (114). **Five
inbound links** — `ShowMeTheCode.md`, `FAQ.md`, `V10MigrationGuide.md`, `Glossary.md`,
`SUMMARY.md` — must be repointed.

**`SweeperCircuitBreaking.md` splits once, not twice** — §7.7 item 5. Its row implies an
Explanation page from `Overview` (11) and `How It Work` (29). **40 lines is a stub**, and
`worklist.md` §6a's own TickerQ ruling is the precedent: *the family shape does not
oblige a split where the sections are empty*. The two sections are the reference page's
necessary preamble and stay in the core. The `## How It Work` typo at line 16 is fixed in
passing (P1-4).

### 7.6 The 32 pages, and where each lands

| Section | Existing | New | Total | Top-level entries |
|---|---:|---:|---:|---:|
| Get Started | 3 | 0 | 3 | 3 |
| Commands, Handlers and Pipelines | 15 | 2 | 17 | 5 |
| Brighter Configuration | 6 | 0 | 6 | 4 |
| Using an External Bus | 12 | 3 | 15 | 9 |
| Transports | 9 | 3 | 12 | 7 |
| Outbox and Inbox | 32 | 7 | 39 | 10 |
| Scheduler | 8 | 2 | 10 | 4 |
| Darker | 5 | 12 | 17 | 5 |
| Health Checks and Observability | 4 | 1 | 5 | 4 |
| V10 Migration | 2 | 1 | 3 | 2 |
| Understanding Brighter | 12 | 1 | 13 | 10 |
| Reference | 2 | 0 | 2 | 2 |
| **Total** | **110** | **32** | **142** | max **10** |

**Every new page is a child of a page already in its final position, or a new top-level
entry. No page's URL moves twice**, which is the property Q4's sequencing exists to buy.

> **Amended 2026-08-08 — see §17, and `tasks.md` Appendix A for the pinned placements.**
> This paragraph continued: *"`AzureBlobArchiveProvider.md` and its configuration child stay
> top-level in Outbox and Inbox rather than nesting under `OutboxArchiver.md` for exactly
> this reason — nesting them would either force a second move or push them to four segments,
> and both are avoided at the cost of one extra top-level entry."*
>
> **Both halves of that reasoning are now spent.** Four segments was measured to work, so
> that half is void. The second move is accepted deliberately for **these two pages alone**:
> they move to *Outbox and Inbox* in PR 2 and re-parent under `OutboxArchiver.md` in PR 5,
> because `OutboxArchiver.md` does not exist until PR 5 and PR 2 touches no page body. **PR 5
> therefore owes two extra redirect entries**, for the intermediate paths
> `outbox-and-inbox/azureblobarchiveprovider` and its child. That is the whole cost, it is
> bounded at two pages, and the property holds for the other 140.

### 7.7 The five deviations from `worklist.md`'s shape column

Collected so a reviewer can overrule them in one place. **No verdict is changed** — every
`split` still splits and every `keep` is honoured.

| # | Row | Shape says | This design does | Because |
|---|---|---|---|---|
| 1 | `HangfireScheduler.md` and the family | `Overview` + `How Brighter Integrates` fold up into the overview | They stay on their own pages; only `Comparison` folds up | 278 lines of per-technology orientation would take `BrighterSchedulerSupport.md` from 578 to **729** — larger than it is today. §5a's own conclusion says *one enriched overview*, not five merged ones |
| 2 | `PostgreSQLMessageBroker.md` | Extract `Transactional Messaging` (46) as a how-to | Stays in the core; flagged to Spec 013 | 46 lines, and 013's publicly committed *PostgreSQL for both transport and outbox* guide is this material written properly |
| 3 | `DefaultMessageMappers.md` | `Configuration Reference` (54) becomes reference | Stays in the core | 54 lines is the how-to's own configuration table. `worklist.md` §6a: *splitting would produce stubs* |
| 4 | `InMemoryOptions.md` | *"Merge each into the matching family page"* | Creates `InMemoryTransport.md`, `InMemoryOutbox.md`, `InMemoryInbox.md` | Three of the five family pages do not exist. Every other transport, outbox store and inbox store has one; InMemory is the missing member |
| 5 | `SweeperCircuitBreaking.md` | Explanation / Reference core / How-to | Reference core / How-to only | The explanation is 40 lines. Same stub rule as item 3 |

### 7.8 Page length is not an acceptance criterion for this spec

`CLAUDE.md` says *"If a file exceeds ~500 lines, consider splitting into logical
sub-topics"*. It is a prompt to think, not a limit, and **this design deliberately leaves
sixteen pages above it.** A review that treats 500 lines as a threshold would reject work
that Spec 011 already measured and ruled on.

**The rule was disproved as a criterion before this spec started.** `worklist.md` §2 is
explicit — *"Size is not the criterion either"* — and gives both counter-examples:
`KafkaConfiguration.md` is 608 lines of a single coherent mode and is a `keep`, while
`BrighterBasicConfiguration.md` scored **two** modes and was split anyway on being 1,070
lines doing two plainly different jobs. **Mode mixing is the criterion. Length is a symptom
that is right often enough to be misleading.**

The sixteen pages, and why each is over:

| Page | After | Why it stays large |
|---|---:|---|
| `CQRSWithBrighterAndDarker.md` | ~935 | Holds Spec 009's 226-line worked example. Drops to ~709 when 009 takes it — requirements §8 forbids consuming it here |
| `V10MigrationGuide.md` | 891 | `keep` — outside Diátaxis, structurally *Before You Start → Step 1…6 → Rollback* |
| `HangfireScheduler.md` | ~770 | Reference core of the family template. Everything separable has already left |
| `QuartzScheduler.md` | ~731 | Same |
| `QueryPipeline.md` | ~716 | Keeps `Available Decorators` (291) because its worklist row says so — see §11 |
| `AwsScheduler.md` | ~700 | Same as Hangfire |
| `CommandProcessorConfigurationReference.md` | 672 | `keep` — created by 011's own split at this size, on purpose |
| `FAQ.md` | 649 | `keep` — outside Diátaxis |
| `ImplementAQueryHandler.md` | ~646 | Three handler patterns (311) plus registration and error handling; splitting further separates a reader from the thing they are implementing |
| `AzureScheduler.md` | ~632 | Same as Hangfire |
| `KafkaConfiguration.md` | 608 | `keep` — **the standing reminder that size misleads.** 608 lines, one mode |
| `Glossary.md` | 591 | `keep` — it is a glossary |
| `QueriesAndQueryObjects.md` | ~579 | May fall further if `## Query Patterns` (102) proves duplicate — verified in PR 3, not assumed |
| `PostgreSQLMessageBroker.md` | ~552 | Producer (80) and consumer (81) configuration belong side by side |
| `AsyncAPISupport.md` | 516 | `keep` — `Complete Examples` is half the page, but the fix is editorial and is not this spec's |
| `RequestValidation.md` | 501 | `keep` — three provider sections a reader wants side by side, each too small to stand alone |

**Seven of the sixteen are `keep` rows.** Re-opening them is what `worklist.md`'s sixteen
`keep` rows exist to prevent, and AC8 requires all sixteen to be honoured.

**`BrighterSchedulerSupport.md` is the case to name explicitly**, because it is the only
page in the corpus that *grows*. It is simultaneously a donor and a receiver: it sheds 212
lines of how-to and takes in the four comparison sections that duplicate its existing
`## Choosing a Scheduler`. It lands at ~400 as designed — but at **729** if §7.7's first
deviation is overruled and the five per-technology overviews fold up too. **A family
overview page legitimately carries more than a leaf page**, because a middle navigation
layer needs a real page to hang it from (requirements §3.1) and that page has to earn its
place with content. If the reviewer overrules deviation 1, the resulting 729 lines is the
consequence of that ruling, not a defect in the split.

The same applies to `BrighterOutboxSupport.md`, `BrighterInboxSupport.md` and
`DistributedLock.md`: all four are parent pages, and shrinking them to stubs to satisfy a
line count would hollow out the navigation the tree depends on.

---

## 8. Anchor links — the cost, measured before the work starts

`worklist.md` §4 rule 2 is the one that bit hardest in Spec 011: *anchor-level links
break, and redirects cannot fix them* — GitBook redirects operate on pages, not fragments.
The `BrighterBasicConfiguration.md` split repointed **28 anchor links across 20 pages** by
hand.

Measured across the whole corpus for the 26 split pages:

| Split page | Inbound anchor links | Anchors | Source files |
|---|---:|---:|---:|
| `BrighterOutboxSupport.md` | 16 | 5 | 13 |
| `ReplayOnSeen.md` | 8 | 5 | 4 |
| `AWSSQSConfiguration.md` | 3 | 1 | 3 |
| `MessageMappers.md` | 3 | 1 | 3 |
| `Telemetry.md` | 2 | 2 | 1 |
| `DarkerBasicConfiguration.md` | 1 | 1 | 1 |
| `PolicyRetryAndCircuitBreaker.md` | 1 | 1 | 1 |
| **19 other split pages** | **0** | | |
| **Total** | **34** | | |

Only the anchors whose *section moves* need repointing:

| Anchor | Links | Moves? |
|---|---:|---|
| `BrighterOutboxSupport.md#implicit-clear` | 9 | no — stays in the core |
| `BrighterOutboxSupport.md#you-always-need-a-sweeper` | 2 | no |
| `BrighterOutboxSupport.md#outbox-archiver` | 3 | **yes** → `OutboxArchiver.md` |
| `BrighterOutboxSupport.md#running-the-sweeper-and-archiver-out-of-process` | 1 | **yes** → `OutboxArchiver.md` |
| `BrighterOutboxSupport.md#complete-example-transactional-messaging` | 1 | **yes** → `TransactionalMessagingWithTheOutbox.md` |
| `ReplayOnSeen.md#causation-id` | 2 | no — Explanation is the core |
| `ReplayOnSeen.md#when-replay-does-not-fire` · `#store-support` | 4 | **yes** → `ReplayOnSeenReference.md` |
| `ReplayOnSeen.md#upgrading-without-migrating` · `#replay-versus-replay-skipped` | 2 | **yes** → `TurningOnReplayOnSeen.md` |
| `AWSSQSConfiguration.md#migrating-from-aws-sdk-v3-to-v4` | 3 | **yes** → `AWSSQSMigrateToV10.md` |
| `MessageMappers.md#message-transformer-factory` | 3 | **yes** → `MessageTransforms.md` |
| `Telemetry.md#configurable-instrumentation` · `#inbox-tracing` | 2 | no — both stay in the core |
| `DarkerBasicConfiguration.md#query-processor-lifetime` | 1 | **yes** → `DarkerConfigurationReference.md` |
| `PolicyRetryAndCircuitBreaker.md#migration-guide-v9-to-v10` | 1 | **yes** → `MigratingToPollyV8.md` |

**≈19 anchor links to repoint across all 26 splits** — two thirds of what one split cost
in Spec 011. Re-derive per split rather than trusting this table; the standing obligation
is *grep for the anchor before you move the heading that owns it*, and run `linkcheck.py`
after every split.

---

## 9. Tooling

### 9.1 D3 — `tools/urlmap.py`, and Q10

`spec/010-information_architecture/urlmap.py` moves to `tools/`, beside the two tools that
already fail the build. It gains `--check-shape` and **gates CI** — Q10 answered **yes**.
A redirect block complete at merge and incomplete three PRs later is the same silent
failure in slow motion.

| Mode | Does | Exit |
|---|---|---|
| *(bare)* | prints the predicted tree | 0 |
| `--verify` | checks against the live `sitemap-pages.xml` | 0 / 1 / **2 if unreachable — not a pass** |
| `--redirects OLD` | emits the `.gitbook.yaml` block; refuses if a page is dropped | 0 / 1 |
| **`--check-shape`** *(new)* | asserts S1, S2, S3 from §4, and that no `SUMMARY.md` heading carries leading whitespace | 0 / 1 |
| **`--check-redirects`** *(new)* | every `redirects:` value resolves to a file that exists; every key is a path that no longer publishes; the file is pure ASCII | 0 / 1 |

`--verify` stays **out of CI** — it depends on an external site and would make the build
flaky. `--check-shape` and `--check-redirects` read only the repository and go into the
`check` job in `.github/workflows/docs.yml`.

**Parser choice, settled.** PyYAML is not available in this environment and `ruby -ryaml`
is an accident of the machine, not a dependency worth taking. The `redirects:` block is a
flat `key: value` map; D7 parses it with ~15 lines of Python **and asserts on bytes** —
no character outside printable ASCII anywhere in `.gitbook.yaml`. That check is the
point: the two U+200B zero-width spaces that made GitBook ignore the `structure:` block
for months came from **GitBook's own published example, which still contains them today,
with the `redirects:` snippet inside the same code block**. A YAML parser would have
parsed `​structure:` happily. **Type the block; never paste it.**

### 9.2 D6 — `tools/llmstxt.py`, and Q9

> **Superseded in its mechanism 2026-08-12, and vindicated in its rule.** `tools/llmstxt.py`
> is **not built**: GitBook owns `/llms.txt`, generates it from the published tree, and we
> cannot override it — measured in `tasks.md` §3, ruled at Task 10.3. **The summary now
> travels as `description:` front matter on the page**, which GitBook reads and emits into
> that canonical index as `- [Title](url): description`. Proven on the live site the same
> day: one page's entry gained exactly **105 bytes** — its 103-character description plus
> `": "` — and nothing else in the file moved.
>
> **Everything below about *the sentence* stands, and matters more now than when it was
> written.** There is no generator to fail the build, so the extraction rule becomes Task
> 10.4's check and the front matter it feeds. The paragraph's central claim — that a
> hand-written parallel table drifts and a sentence living in the page cannot — is exactly
> why the front-matter route is the same design by another mechanism.
>
> **One amendment the section asked for is now moot.** It proposed `llms.txt` emit published
> URLs rather than repository paths, and called it the maintainer's call. The platform emits
> published URLs already; there is no file of ours for the question to apply to.

`CLAUDE.md` fixes the format as `- [Title](path): Type — one sentence.` The type comes
from the banner. **The sentence comes from the page's own opening sentence**, and the
generator refuses to invent one:

> **The summary is the first sentence of the page's introduction** — the first sentence
> of the first non-blank prose line after the banner. `llmstxt.py` extracts it and
> **fails** if it is missing, longer than 200 characters, ends in a colon, or is
> byte-identical to another page's.

This is the answer to Q9's dilemma — *extract mechanically and accept the quality*, or
*author ~142 summaries by hand*. It takes neither. A page whose opening sentence does not
survive being read alone has a **bad opening sentence**, and fixing it improves the page
for every reader, not just for a retrieval client. A parallel table of hand-written
summaries would drift from the pages within a release; a sentence that lives in the page
cannot.

Expect the first run to fail on a double-figure number of pages. That is the deliverable
working, and the fixes are one sentence each.

**One amendment to `CLAUDE.md` this needs, and it is the maintainer's call.** The
documented format uses repository paths (`/contents/FileName.md`). A retrieval client
wants the **published URL**, which `urlmap.py` now emits and which was validated 110/110.
Recommendation: `llms.txt` emits published URLs and `CLAUDE.md` § llms.txt is amended to
match. Repository paths in a file whose entire audience is HTTP clients help nobody.

### 9.3 D7 — `linkcheck.py`

`linkcheck.py` keeps the four faults it reports today and gains nothing: the redirect
validation lives in `urlmap.py --check-redirects` (§9.1), because it needs the URL model
and `linkcheck.py` does not have one. P1-2 is satisfied by that check being in CI rather
than run once.

### 9.4 Code examples plan

**010 authors no code examples.** Every block in every new page is moved verbatim from
the page it came from — `worklist.md` §4 rule 4, *move the text; do not improve it*,
verified mechanically by D5 rather than by reading the diff.

The one consequence that bites: **a new page is 100% added lines**, so
`pagelint.py --changed` makes every C# block on it strict. A block that cannot carry its
`using` directives marks the omission `// ...`, which **downgrades the finding to a
warning and never silences it**, and the block still counts toward the 802-block debt. Do
not backfill namespaces you have not checked.

The single exception, and it is prose not code: the sentence in `MessageTransforms.md`
stating that transforms require a custom mapper (§7.5).

---

## 10. Q4 — sequencing, and the PR plan

**Restructure first, splits second.** The restructure touches `SUMMARY.md` and
`.gitbook.yaml` and **no page bodies at all**; the splits touch only page bodies and add
`SUMMARY.md` entries. They share one file. Splitting into a settled tree means no page's
URL moves twice — a property §7.6 now establishes by construction rather than by hope.

| PR | Contents | Pages touched | Gate |
|---:|---|---|---|
| **1** | This design; `README.md` amendments (§12) | 0 | — |
| **2** | **D1 + D2 + D3.** New `SUMMARY.md`; `redirects:` block with **74** entries; `urlmap.py` → `tools/`, `--check-shape` and `--check-redirects` in CI | 0 | `linkcheck.py`, `pagelint.py`, both new checks; **AC4 byte inspection before merge, never after** |
| **3** | **D9** — the three `worklist.md` §7 content fixes, plus the `QueriesAndQueryObjects.md` ↔ `QueryPatterns.md` duplication **verification** | 3 | verification is a finding, not an edit |
| **4** | Scheduler family — §7.1, 6 rows, 2 new pages | 8 | D5 per split |
| **5** | Outbox and Inbox — `BrighterOutboxSupport`, `ReplayOnSeen`, `SweeperCircuitBreaking`, `InMemoryOptions` redistribution | 6 + 3 new | D5; **19 of the 34 anchor links land here** |
| **6** | Darker — §7.3, 5 rows, 12 new pages | 5 | D5 |
| **7** | External bus — `MessageMappers` §5c, `DefaultMessageMappers`, `CloudEventsSupport`, `DynamicMessageDeserialization` | 4 | D5 |
| **8** | Transports — `AWSSQSConfiguration`, `PostgreSQLMessageBroker` | 2 | D5 |
| **9** | The rest of §6d — `CQRS`, `NullableReferenceTypes`, `AgreementDispatcher`, `PolicyRetryAndCircuitBreaker`, `Telemetry` | 5 | D5 |
| **10** | **D6** `llms.txt` + generator, once the page count is stable | 0 | first run will fail; fix the sentences |
| **11** | **D8** per-term `BasicConcepts.md` → `Glossary.md` links; **P2-1** the 17 files with no trailing newline; **P2-3** the changed-range echo in `docs.yml` | 2 + `docs.yml` | `linkcheck.py` |

**PRs 4–9 are individually shippable and individually abandonable.** AC7 is per-split and
partial completion is an explicit valid end state — the maintainer's ruling, and what
makes "interruptible" real. **PR 2 is the one that must not be partial**, because it is
the tree every later PR files into.

**Redirect timing.** Verify the block *before* merging PR 2. Redirect responses cache with
`stale-while-revalidate=2592000` — thirty days — so a wrong redirect outlives its fix at
the edge. And **every cached response reports `200`**, genuine 404s and genuine redirects
alike; the reliable tell is that no genuine page response carries a `location:` header.
Sync latency is 25–45 seconds.

**A redirect entry does not go stale when its page moves again**, because the value is a
repository path that GitBook resolves to wherever that page currently publishes
(requirements §16). So PR 2's block is written once and needs no revision as later PRs
add pages.

---

## 11. Recorded, deliberately not acted on

- **`QueryPipeline.md`'s `Available Decorators` (291 lines)** is a reference table inside
  a how-to and would split cleanly. Its worklist row says the core keeps it, so it keeps
  it. If a later spec revisits Darker, this is the first candidate.
- **`CQRSWithBrighterAndDarker.md` stays at ~935 lines** until Spec 009 takes its
  226-line worked example.
- **`AsyncAPISupport.md`'s `Complete Examples` is half the page** (`worklist.md` §6e) —
  the fix is editorial, not a split, and it is not this spec's.
- **`HandlerFailure.md` ↔ `ErrorHandlingOptions.md` are not merged.** They are the
  corpus's best existing explanation/reference pair. This design makes the relationship
  navigational instead: `ErrorHandlingOptions.md` nests under `HandlerFailure.md`, and
  ~~the missing reverse pointer~~ plus a *Prerequisites* segment is a one-line fix in PR 9.
  **Corrected 2026-08-12 at execution: the reverse pointer was never missing.**
  `HandlerFailure.md` has linked `ErrorHandlingOptions.md` since the page was created, and the
  nesting landed in PR 2, so PR 9 shipped the banner segment alone. `tasks.md`
  *Phase 9 as executed* has the trace.
- **The 802-block `using`-directive debt** is Spec 011's AC1 baseline and shrinks only as
  pages are edited. Splits move blocks; they do not backfill them.
- **Whether GitBook's automatic redirects persist** is still the one open platform
  unknown. They may be tied to revision history. Re-probe PR #77's old path —
  `command-processors-and-dispatchers/commandscommanddispatcherandprocessor` should still
  carry a `location:` header. The `.gitbook.yaml` block ships regardless; that is what it
  is for.

---

## 12. `README.md` amendments (PR 1)

Three corrections, all recorded in requirements §8 and §2.1:

1. **Out of Scope** reads *"Editing page bodies. Splitting mixed-mode pages is Spec 011."*
   Splitting moved into 010 on 2026-08-03, and `worklist.md` names 010 as its executor in
   its own header. **Page bodies are in scope**, for the 26 splits and the three §7
   content defects.
2. **The redirect example** targets `transports/RabbitMQConfiguration.md` — a directory
   that does not exist and that this spec does not create. `contents/` stays flat, so
   every target is `contents/<FileName>.md`.
3. **The proposed structure** in the README is superseded by §3 of this document.

---

## 13. Style notes

- **No new terminology.** The tree introduces two section names — *Get Started* and
  *Understanding Brighter* — and neither is a term of art. Everything else is an existing
  name kept or an existing name shortened.
- **"Dispatcher", never "ServiceActivator"**, in prose; `pagelint.py` rule 5 enforces it.
  `HowServiceActivatorWorks.md` keeps its filename — the V9 term is in the path, invisible
  to readers, and renaming it would move a URL to fix something no reader sees.
- **Every new page carries a banner** one blank line below its H1, separator ` · `
  (U+00B7), and a split is exactly where the *Prerequisites* segment earns its place: the
  new page's prerequisite is almost always the core it came from.
- **Every `##` heading is qualified by its subject** and unique across pages. The five
  navigation headings — `Further Reading`, `Related Documentation`, `See Also`,
  `Next Steps`, `References` — are exempt and stay uniform. A section moving to a new page
  usually needs requalifying: `## Migration from Other Schedulers` on a page called
  *Switching Schedulers* becomes `## Why You Would Switch Schedulers`.
- **Glossary work is D8**, not a side effect of the splits: per-term links from
  `BasicConcepts.md`'s 24 terms into `Glossary.md`'s 100. There are **zero** such links
  today. This replaces the withdrawn merge and is additive.
- **`BoxProvisioning.md#when-to-use-box-provisioning` must not be requalified without
  repointing** — Spec 009's rung 3 links to it. `linkcheck.py` catches it; redirects
  cannot.

---

## 14. Acceptance criteria — how each is met

| # | Criterion | Met by |
|---|---|---|
| AC1 | `SUMMARY.md` links all pages, zero orphans | PR 2 places 110; each split PR adds its own entries. `linkcheck.py` |
| AC2 | `linkcheck.py` and `pagelint.py` at **0 errors** | CI, every PR |
| AC3 | Every moved page has a redirect; none points at a missing file | `--redirects` emits 74 in PR 2; `--check-redirects` keeps it true |
| AC4 | The block is verified **mechanically** | `--check-redirects` parses and byte-inspects. **Before merge** |
| AC5a | The mechanism discriminates | **Done** — requirements §16, PRs #77/#78/#79 |
| AC5b | Post-merge sample returns the new page | Manual, after PR 2, reading the `location:` header and body size — not the status code |
| AC6 | No section of one page; none unnavigable | **S1/S2/S3** in §4, enforced by `--check-shape` |
| AC7 | Per split, no information loss, mechanically | D5 per split. Partial completion is a valid end state |
| AC8 | All 16 `keep` rows honoured | §7 touches none of them; §7.7 changes no verdict |
| AC9 | A summary per page, set at source, reaching the canonical `/llms.txt` | §9.2 **as re-scoped** — `description:` front matter carrying the page's own opening sentence, and the build fails when that sentence cannot be extracted. **Narrowed 2026-08-12**, requirements §12 |

---

## 15. What is measured, and what is not

**Measured — cite these:**

- 110 pages → 110 in the new tree, **74 URLs moved, 36 unchanged**, max depth 3 (§5)
- **12 sections**, 2–10 top-level entries each (§4)
- **32 new pages**, 110 → **142**, derived row by row from `## `-section spans (§7)
- **34 inbound anchor links** across 7 of the 26 split pages; 19 have none; **≈19 need
  repointing** (§8)
- The scheduler fold-up arithmetic: 578 − 212 + 278 + 85 = **729** (§7.1)

**Not measured — challenge these:**

- **Every "after" line count in §7 is arithmetic on today's sections**, not a count of a
  page that exists. They will move as headings are requalified and lead-ins rewritten.
  `InMemoryScheduler.md` at ~495 is the one to watch: it is four lines under 500 by
  arithmetic alone, so a banner and two lead-ins put it back over.
- **The 32 is a count of pages this design decides to create**, not a prediction. If a
  reviewer overrules any of §7.7's five deviations, it changes: items 1, 2, 3 and 5 would
  add 5 pages and item 4 would remove 3.
- **§7.6's *top-level entries* column is an intention, not a measurement.** §4's column is
  reproducible from `SUMMARY.target.md` and was; §7.6's depends on where each of the 32 new
  pages nests, which no file in this repo yet records. It matters at the edge: if all seven
  of *Outbox and Inbox*'s new pages landed top-level it would show 15 entries and breach
  S2's ceiling of 12. `--check-shape` catches that at PR time, which is the containment —
  but the number is not evidence until the `SUMMARY.md` entries exist.
- **Whether `## Query Patterns` in `QueriesAndQueryObjects.md` really duplicates
  `QueryPatterns.md`.** Flagged, not verified. PR 3 verifies it before anything moves.
- **Whether GitBook's automatic redirects persist** (§11).
- **#67 has no reply.** Checked 2026-08-07, and **re-checked 2026-08-08 at design review:
  still two comments, both the maintainer's**, unchanged since 2026-08-03.
  Diátaxis-as-authoring-discipline was flagged there for pushback, so check again before
  PR 2 merges.

---

## 16. What the design review changed (2026-08-08)

Five findings. **No verdict, no page count, no threshold and no ruling moved** — every
correction was to a tally, which is the ninth time this programme has met that pattern and
the first time it has been predicted in advance.

**1. Every whole-page line count was one too high, on 23 of the 26 split rows.** The
figures were produced with `read().split("\n")`, which counts a phantom empty final line on
any file ending in a newline — 93 of the 110 pages. It is *correct* on the other 17, which
is why `AWSSQSConfiguration.md` (615), `BrighterOutboxSupport.md` (517) and
`TickerQScheduler.md` (234) were right and the rest were not. The same artefact inflates
the span of the **last `## ` section on such a page**, and exactly two of the ~60 spans
cited in §7 are last sections: `MessageMappers.md` `## Transformers` (120 → **119**) and
`PolicyRetryAndCircuitBreaker.md` `## Legacy: Using Polly v7 Policies` (152 → **151**).
Every interior span was already right, which is why 58 of 60 reproduced. All "today" and
"after" figures are re-derived with `splitlines()`.

> **The measuring convention, stated once so it does not drift again:** a page's length is
> `len(text.splitlines())`. `wc -l` counts newlines and so under-reports the 17 files with
> no trailing newline; `split("\n")` over-reports the 93 with one. Only `splitlines()` is
> right for both, and `worklist.md` already used it — which is why design and worklist had
> begun to disagree about the same pages (`QueryPipeline.md` 929 against 928).

**2. The scheduler fold-up is 85 lines across four sections, not 96 across six.** The 96
came from matching the word "Comparison", which swept in `AwsScheduler.md`'s
`## Scheduling Modes Comparison` — a table comparing AWS's *own* two scheduling modes,
which belongs in the AWS Reference core and not in `## Choosing a Scheduler`.
`QuartzScheduler.md` has no comparison section at all. §7.1's per-page table had already
excluded the AWS table, so 11 lines were sitting on both sides of the ledger. The
arithmetic behind §7.7 item 1 becomes **578 − 212 + 278 + 85 = 729**. **The deviation
stands**: 729 still exceeds the 578 the page is today, which was the whole argument.
`worklist.md` §5a and §6a say "six" and are wrong on the count for the same reason.

**3. §4's prose contradicted its own table** — *Outbox and Inbox* shows **8** top-level
entries before the splits, not 10. Ten is the post-split figure from §7.6. The table and
`SUMMARY.target.md` were right; the sentence carrying S2's argument was not.

**4. §15 now admits §7.6's entries column is an intention**, not a measurement, with the
breach case named.

**5. D7 is delivered differently from the approved requirements, and this ratifies it.**
Requirements D7 reads *"`linkcheck.py` extended to validate the redirect block"*; §9.3
puts that validation in `urlmap.py --check-redirects` instead, because it needs the URL
model and `linkcheck.py` has none. The deliverable is met and P1-2 is satisfied by the
check running in CI. Recorded here rather than in §7.7, which covers only deviations from
`worklist.md`'s shape column.

---

## 17. What the depth measurement changed (2026-08-08)

**S3's ceiling was an untested assumption, and it has been measured. It is 4, not 3.**

The original rule read *"No published path exceeds 3 segments — three is the deepest the
live site is known to work at (9 pages publish there today). Four is unverified, and this
programme does not ship unverified behaviour."* Read closely, that says nothing about
GitBook. It says nothing about our site either. It is a statement about **our own
`SUMMARY.md`**: nine pages happen to be nested one level, none is nested two, and the rule
promoted that accident into a constraint.

It then cost real navigation quality in two places, which is what prompted the measurement:

- `MigratingToPollyV8.md` had to become a **sibling of its own source**, because
  `PolicyRetryAndCircuitBreaker.md` already publishes at three.
- `AzureBlobConfiguration.md` could not sit under `AzureBlobArchiveProvider.md` under a new
  `OutboxArchiver.md` — the grouping those three pages obviously want.

**Two measurements, the first free.**

**1. The platform publishes four segments.** GitBook's own documentation has **30 pages** at
four segments below its site root, out of 182 — for example
`create-content/content-structure/page/tags`, which is section plus two ancestor pages plus
page, exactly our shape. Counted from the raw `sitemap-pages.xml`, not from a summary of it.

**2. So does this site.** PR #83 published one **new** page nested three levels deep, and PR
#84 reverted it minutes later. A brand-new page at a path that had never existed was used
deliberately — the same reasoning that made D0 a measurement rather than a hope, because **no
automatic redirect can mask a path that never resolved before**. No existing page moved, so
no real URL churned and no redirect cached against content a reader wants.

| Observation | Result |
|---|---|
| Path | `guaranteed-at-least-once/azureblobarchiveprovider/azureblobconfiguration/urldepthprobe` |
| Present in `sitemap-pages.xml` | **yes** — 112 entries, up from 111 |
| HTTP status | `200` |
| `location:` headers | **0** — no redirect; this is a genuine page |
| Body size | **529,080 bytes** — a 404 shell is ~189.5 KB and a redirect ~192 KB |
| `<title>` | `URL Depth Probe | Paramore Brighter Documentation` |
| After revert | sitemap back to **111**; tree byte-identical to `c4aedb5` |

**The status code was ignored, as it must be on this site** — every cached response reports
`200`. The load-bearing evidence is the sitemap entry, the absent `location:` header and the
529 KB body.

`urlmap.py` needed no change: its model is
`<slug(section)>/[<slug(ancestor)>/]*<slug(filename)>` and the `*` was never bounded. It
predicted the four-segment path exactly.

**What moves as a result:**

| | Was | Now |
|---|---|---|
| S3 | ≤ 3 segments, assumed | **≤ 4 segments, measured** |
| `MigratingToPollyV8.md` | sibling under `BuildingAPipeline.md` | **child of `PolicyRetryAndCircuitBreaker.md`** |
| `AzureBlobArchiveProvider.md` + child | top-level in *Outbox and Inbox* | **nested under `OutboxArchiver.md`** |
| *Outbox and Inbox* top-level entries | 8 or 10, depending on nesting | **9** |
| Deepest path after all splits | 3 | **4**, reached by exactly two pages |

**What does not move.** The twelve-section tree, every `keep` verdict, the 74/36 URL split,
the 32 new pages, and PR 2's tree — whose own maximum depth is still 3, because nothing in
the 110-page restructure needs the fourth segment. **S3 is still a rule and still gates CI**;
it is now a navigation-quality guard with evidence behind it rather than a proxy for
ignorance. Five segments remains untested, and GitBook's own site does not go there either.

**The lesson, which is the reusable part.** A threshold justified by *"X is the deepest we
know to work"* is a record of what has been tried, not a finding about what is possible — and
it will be read by everyone downstream as the latter. This one survived requirements, design
and a design review without being questioned, and cost two contorted placements before anyone
asked what caused it. **When a rule's stated rationale is the absence of evidence, that is not
a rationale; it is a measurement waiting to be taken.**
