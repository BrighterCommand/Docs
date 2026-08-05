# Page-splitting worklist for Spec 010

**Created:** 2026-08-05 · **Measured against:** 110 pages under `contents/`
**Produced by:** Spec 011, Task 7.1 (deliverable D8) · **Executed by:** Spec 010

This file is written to be read **without Spec 011 in context**. Everything you need to
act on a row is here or is one command away. Where a verdict rests on a decision taken
elsewhere, the decision is restated rather than cited.

---

## 1. What this is

Spec 011 measured how badly the corpus mixes Diátaxis modes within single pages, and
split the two worst examples as demonstrators. **The rest of the splitting belongs to
Spec 010**, because splitting creates pages that need names, `SUMMARY.md` entries and
redirects — all of which 010 is already changing. Doing it here would mean touching
every page twice and reviewing two large overlapping diffs.

So this is the list 010 works through. Each row carries a verdict you can execute or
disagree with, and a reason you can check.

**A row is not an instruction to split.** Twelve of the 42 rows say `keep`, and several
of those are pages a size-based or score-based rule would have split by mistake. Those
rows exist precisely so that 010 does not re-open a question that has already been
answered.

### How to read a row

| Column | Meaning |
|---|---|
| **Page** | Path under `contents/`, with its line count |
| **Score** | How many of four mode signals its headings hit — see §2 |
| **Type** | The page type in its banner today (`Reference` / `How-to` / `Explanation`) |
| **Verdict** | `split` · `keep` · `keep — outside Diátaxis` · `keep — paired` |
| **Shape** | For `split`: which sections go where |
| **Why** | One sentence, and always present where the verdict contradicts the score |

---

## 2. The mode score, and what it is not

Every page was scored by matching its H2/H3 heading text against vocabulary for four
signals — `reference`, `explanation`, `howto`, `guidance` — with navigation headings
(`Further Reading`, `Related Documentation`, `See Also`, `Next Steps`, `References`)
excluded because they appear almost everywhere and are not content.

Regenerate at any time, from the repo root:

```bash
python3 spec/011-authoring_conventions/modemix.py
```

Current output: **30 pages score ≥3**, **13 score 4**, **21 are >500 lines with ≥3
modes**, **9 are >500 lines with <3 modes**.

**Three warnings, all of which this list acts on:**

- **The score is a proxy, and it is wrong in both directions.** `ReactorAndProactor.md`
  scores four and reads as one coherent argument. `ReplayOnSeen.md` scores two and is
  1,039 lines carrying three clearly separable modes. The score's job is to stop us
  splitting by gut feel, not to decide anything.
- **`guidance` is not a page type.** It is a signal picked up from `Best Practices`,
  `Troubleshooting` and `Common Pitfalls` headings. Diátaxis has four modes and
  "guidance" is not among them, so guidance material is **folded into the section it
  concerns**, never split into a page of its own. Both demonstrator splits did this;
  see §4.
- **Size is not the criterion either.** `KafkaConfiguration.md` is 608 lines and scores
  one mode; `Glossary.md` is 591 lines and is a glossary. Both are `keep`. Conversely
  `BrighterBasicConfiguration.md` scored **two** and was split anyway, on the strength
  of being 1,070 lines of two plainly different jobs.

---

## 3. Do not split these — settled, do not re-open

**Legitimately outside Diátaxis.** These are material you *consult*, which is what
Reference means at its widest. Their mode scores are artefacts of covering many
subjects, not evidence of confusion:

| Page | Lines | Score |
|---|---|---|
| `FAQ.md` | 649 | 3 |
| `Glossary.md` | 591 | 2 |
| `V10MigrationGuide.md` | 891 | 3 |

`V10MigrationGuide.md` is structurally *Before You Start → Step 1 … Step 6 → Rollback
Plan* and is banner-typed **How-to** on that basis. That contradicted Spec 011's own
approved design, which had argued Reference; the outline won. If a maintainer overrules
it the page type changes, but the `keep` does not.

**`BasicConcepts.md` does not merge into `Glossary.md`.** An earlier audit called these
"two glossaries" and scoped a merge, on the grounds that `BasicConcepts.md`'s 24 terms
are a subset of `Glossary.md`'s 100. **The merge was withdrawn by the maintainer on
2026-08-04**: the separation is deliberate, because a newcomer should be able to learn
the key terms *without* reading the full glossary. Two completed specs (002 and 006)
have been maintaining the smaller page on purpose. **What to do instead:** link each
`BasicConcepts.md` term to its fuller `Glossary.md` anchor. That is an addition, not a
merge, and every glossary link already carries an `#anchor` that `linkcheck.py` checks.

---

## 4. The precedent — what the two demonstrator splits did

Spec 011 split two pages. Follow the same shape unless a row says otherwise; the
reasoning behind each rule is given so you can tell when it does not apply.

| Original | Before | After |
|---|---|---|
| `RabbitMQConfiguration.md` | 566 | **330** + `RabbitMQDurability.md` (126, Explanation) + `RabbitMQMigrateToQuorumQueues.md` (82, How-to) + `RabbitMQConnectionStability.md` (144, How-to) |
| `BrighterBasicConfiguration.md` | 1,070 | **237** + `CommandProcessorConfigurationReference.md` (672, Reference) + `DispatcherConfigurationReference.md` (233, Reference) |

**Five rules those splits established:**

1. **The core keeps the original file name.** Both did, so no published URL moved and
   **no page-level redirect was needed**. The new pages are new URLs with nothing to
   redirect *from*. Adding a `redirects:` block for the *re-filing* remains 010's job;
   splitting alone does not create one.
2. **Anchor-level links break, and redirects cannot fix them.** GitBook redirects
   operate on pages, not fragments. The second split had to repoint **28 anchor links
   across 20 pages** by hand. Budget for this: run `python3 tools/linkcheck.py` after
   every split, and grep for the anchor before you move the heading that owns it.
3. **Guidance folds, it does not move.** RabbitMQ's four `Best Practices` sections
   (23 items) were folded into the sections they concerned rather than becoming a page.
4. **Move the text; do not improve it.** Verified mechanically both times, not by eye:
   every substantive line of each original was tested for verbatim presence across the
   resulting pages. Only deliberate edits came back — 24 lines and 4 lines respectively.
   Do the same; "I read the diff" is not the same check.
5. **A new page is 100% added lines, so its code blocks are strict.** `pagelint.py
   --changed` makes missing `using` directives an *error* for any block overlapping the
   diff — which, on a new page, is all of them. Across the two splits that was 42
   blocks. A block that genuinely cannot carry its `using` directives marks the omission
   with `// ...`, which **downgrades the finding to a warning and never silences it**.
   Use it when moving a block verbatim; do not backfill namespaces you have not checked.

---

## 5. Cross-cutting decisions — settle once, apply many times

Three groups where per-page rows would hide the fact that one decision covers all of
them. **Decide the shape once.**

### 5a. The scheduler family — one split, applied five times

Six scheduler implementation pages follow an **identical template**:

```text
Overview / When to Use  ->  How Brighter Integrates  ->  NuGet Packages
->  Configuration  ->  Code Examples  ->  Best Practices  ->  Troubleshooting
->  Migration from Other Schedulers  ->  Comparison  ->  Summary
```

Two findings that change the obvious plan:

- **The per-page `Migration from Other Schedulers` sections say the same thing.**
  Every one is "swap the factory" with a before/after pair — `HangfireScheduler.md:751`
  and `AwsScheduler.md:692` differ only in which factory they name. Six per-page how-tos
  would be six near-copies. **Write one `SwitchingSchedulers.md` how-to covering the
  matrix**, and delete the six sections into it.
- **The per-page `Comparison` sections duplicate a section that already exists.**
  `BrighterSchedulerSupport.md` already carries `## Choosing a Scheduler` (lines
  237–334). The comparisons should **fold up into that page**, not become five new
  explanations.

So the family resolves to: **five Reference cores** (keeping their file names), **one
new how-to**, and **one enriched family overview** — not eighteen pages.

**`PostgreSQLMessageBroker.md` is not a scheduler.** Spec 011's classification notes
listed it as the seventh member of this family. It is a **transport**, filed under
transports in `SUMMARY.md:72`; it merely happens to share the template. It gets its own
row in §6 and is not covered by this decision.

**`CustomScheduler.md`** (164 lines, How-to) sits in the same `SUMMARY.md` section but
is a build-your-own guide, not a template instance. Not in scope here.

### 5b. Darker parallels Brighter, and the shapes should match

Darker is Brighter's sister project — the query side to Brighter's command side. Its
documentation sits alongside Brighter's and often parallels it without being the same:

| Concept | Brighter | Darker |
|---|---|---|
| Basic configuration | `BrighterBasicConfiguration.md` (How-to) | `DarkerBasicConfiguration.md` (How-to) |
| The request / query object | `Requests, Commands and Events.md` (Explanation) | `QueriesAndQueryObjects.md` (Explanation) |
| Implementing a handler | `ImplementingAHandler.md` (How-to) | `ImplementAQueryHandler.md` (How-to) |
| The pipeline | `BuildingAPipeline.md` (How-to, **177 lines**) | `QueryPipeline.md` (How-to, **928 lines**) |
| Patterns | *(none)* | `QueryPatterns.md` (How-to) |

**The pipeline row is the finding.** The five-fold difference is **architectural, not
editorial**: Brighter decomposes the same subject across `BuildingAPipeline.md`,
`PolicyRetryAndCircuitBreaker.md` and `PolicyFallback.md`, while Darker keeps decorators,
Polly configuration and resilience patterns in one file. Splitting `QueryPipeline.md`
along Brighter's existing seams restores the parallel *and* fixes the page. That is a
better argument than its mode score of 2 provides.

Likewise, `DarkerBasicConfiguration.md` should inherit the shape already applied to
`BrighterBasicConfiguration.md` — How-to core, configuration options extracted to a
Reference page.

**Constraint on Darker work.** Brighter and Darker version independently: Brighter is on
V10, `Paramore.Darker`'s latest release is **4.1.1**, and there has never been a Darker
V10. Darker's next release is in flight and the local `../Darker` working tree is ahead
of what is deployed, so **do not update Darker page content from that tree** — the docs
site publishes the deployed version. Re-filing and splitting are safe; rewriting
behaviour is not. The ten Darker-touching pages are identifiable as a set from the
`applies` column of `pagetypes.tsv`.

### 5c. The message-mapper cluster — three pages, one decision

Ruled by the maintainer 2026-08-04. `MessageMappers.md` (266 lines) is an Explanation
that needs breaking into three, and the pieces are entangled with a page that already
exists:

| Target | Mode | Content |
|---|---|---|
| The default-mapper how-to | How-to | **`DefaultMessageMappers.md` (478 lines) already exists and is already typed How-to.** Establish first whether this is a matter of pointing at it and making it the default route, rather than writing a new page |
| `MessageMappers.md` | Explanation | What a mapper is, the `Message` structure, and **when you need a custom one** |
| A transforms page | Explanation | Wrap / Unwrap / Transform and the transformer factory — currently `## Transformers` at `MessageMappers.md:148` |

**The transforms page carries a correctness fix, not just a filing one.** It must state
that transforms **require a custom mapper** and form part of the pipeline. The ruling
was explicit that this is not currently clear, and a reader can today come away thinking
transforms work with the default mapper.

---

## 6. The rows

Ordered by size within each group. Line counts are `modemix.py`'s, which counts lines
rather than newlines and so may read one higher than `wc -l` on a file with no trailing
newline (18 files in `contents/` lack one).

### 6a. Scheduler family — see §5a for the shape

| Page | Lines | Score | Type | Verdict | Shape / why |
|---|---|---|---|---|---|
| `HangfireScheduler.md` | 832 | 4 | Reference | **split** | Reference core keeps name (`Configuration`, `Storage Options`, `Dashboard`, `Code Examples`, `High Availability`, `Monitoring`). `Overview` + `How Brighter Integrates` + `Comparison: Hangfire vs Quartz` (787) fold up into `BrighterSchedulerSupport.md`; `Migration from Other Schedulers` (751) into the shared how-to |
| `AwsScheduler.md` | 775 | 4 | Reference | **split** | Same. `IAM Role Requirements` (81) stays in the Reference core — it is a prerequisite table, not explanation |
| `QuartzScheduler.md` | 769 | 3 | Reference | **split** | Same. `Persistence Options` (207) and `Clustering and High Availability` (411) stay in the core |
| `AzureScheduler.md` | 717 | 4 | Reference | **split** | Same. `Authentication and Credentials` (54) and `RBAC Permissions Required` (95) stay in the core |
| `InMemoryScheduler.md` | 541 | 4 | Reference | **split** | Same, with one difference: `Important Warning` (7) and `When to Use` (54) are the page's most valuable content — it is the scheduler you must *not* ship. Keep the warning in the core even though the rest of the when-to-use material folds up |
| `TickerQScheduler.md` | 234 | 3 | Reference | **keep** | Scores 3 on the family template but there is nothing to move: `Best Practices` is 8 lines, `Troubleshooting` 23, and there is no migration section at all. Splitting would produce stubs. **The family shape does not oblige a split where the sections are empty** |
| `BrighterSchedulerSupport.md` | 578 | 4 | Explanation | **split** | The family overview, and both a *donor* and a *receiver*. Sheds `Code Examples` (334–501) and `Configuration Examples` (501–546) — 210 lines of how-to on an explanation page — into a new "how to schedule a message" how-to. Receives the six per-page comparison sections into `## Choosing a Scheduler` |

### 6b. Transports and configuration reference

| Page | Lines | Score | Type | Verdict | Shape / why |
|---|---|---|---|---|---|
| `PostgreSQLMessageBroker.md` | 662 | 4 | Reference | **split** | Not a scheduler (see §5a). Reference core keeps name (`Configuration`, `Producer`/`Consumer Configuration`, `Configuration Options`, `Monitoring`). Explanation out: `Benefits` (22), `When to Use` (44), `Limitations` (63), `JSON vs JSONB` (338), `Comparison with Other Transports` (584). How-to out: `Transactional Messaging` (392) |
| `AWSSQSConfiguration.md` | 615 | 3 | Reference | **split** | **The exact RabbitMQ precedent.** `V10 Migration Path` (420–615) is 195 lines of how-to at the end of a reference page; `AWS SDK v4 Support` (371) is migration-flavoured too. Extract both to `AWSSQSMigrateToV10.md`; core keeps the name and the four `SQS *` sections |
| `KafkaConfiguration.md` | 608 | 1 | Reference | **keep** | **Standing reminder: size misleads.** 608 lines, a single mode, coherent reference throughout. A line-count rule would split this and damage it. Do not re-open |
| `CommandProcessorConfigurationReference.md` | 672 | 2 | Reference | **keep** | Created 2026-08-05 as the Reference half of the `BrighterBasicConfiguration.md` split. Deliberately this size; do not re-split |

### 6c. Darker — see §5b

| Page | Lines | Score | Type | Verdict | Shape / why |
|---|---|---|---|---|---|
| `QueryPatterns.md` | 1,291 | 2 | How-to | **split** | **The largest page in the corpus**, and the score of 2 badly understates it: six independent task-shaped recipes in one file. `Parameterized` (11), `Pagination` (281), `Projection` (514), `Collection and Aggregation` (687) are each a how-to; `Entity Framework Core Integration` (849) is a separate subject; `Real-World Example: Product Catalog Query` (1062–1259, 197 lines) is a worked example. Keep the name as a hub |
| `ImplementAQueryHandler.md` | 935 | 2 | How-to | **split** | `Testing Query Handlers` (743–902, 159 lines) is its own how-to and has no business inside "implement a handler". `Working with Dependencies` (446) is the next candidate. Core keeps the three handler patterns |
| `QueryPipeline.md` | 928 | 2 | How-to | **split** | See §5b — split along Brighter's existing seams. `Configuring Polly Policies` (540–699, 159 lines) becomes the Darker counterpart of `PolicyRetryAndCircuitBreaker.md`; `Comparison with Brighter Pipeline` (699) is explanation. Core keeps `Available Decorators` + `Decorator Patterns` |
| `QueriesAndQueryObjects.md` | 877 | 2 | Explanation | **split** | Explanation core keeps `The IQuery<TResult> Interface`, `Designing Query Objects`, `Design Principles`, `Naming Conventions`. Out: `Validation in Query Objects` (468–587) and `Query Result Types` (289–468). **Check `## Query Patterns` (746–848) against the whole `QueryPatterns.md` page before moving it** — that is a duplication flag, not a section |
| `DarkerBasicConfiguration.md` | 510 | 3 | How-to | **split** | Inherit the `BrighterBasicConfiguration.md` shape: How-to core keeps `Quick Start`, `Using IQueryProcessor`, `Common Configuration Patterns`; `Darker Configuration Options` (151–226) becomes a Reference page paralleling `CommandProcessorConfigurationReference.md` |

### 6d. Large explanations and features

| Page | Lines | Score | Type | Verdict | Shape / why |
|---|---|---|---|---|---|
| `CQRSWithBrighterAndDarker.md` | 1,144 | 3 | Explanation | **split** | Explanation core keeps `CQRS Fundamentals`, `Brighter: The Command Side`, `Darker: The Query Side`, `When to Use CQRS`, `Trade-offs`. Out: `Use Cases and Patterns` (448–657, 209 lines) and `Example: E-Commerce Order System` (810–1036, 226 lines). **Flag the worked example to Spec 009** — the corpus has no tutorial, and 226 lines of end-to-end example is the closest thing to one |
| `ReplayOnSeen.md` | 1,039 | 2 | Reference | **split** | **Scores 2 and is the second-largest page** — the clearest case of the score understating. Three clean modes: Explanation (`The Problem`, `How Replay Walks the Flow Forward`, `Why It Works This Way`), How-to (`Turning It On`, `You Must Thread Your RequestContext`, `Before You Enable It`, `Upgrading Without Migrating`), Reference (`Store Support`, `Observability`, `Limitations`). Its banner says Reference; on this outline the core is arguably the Explanation |
| `NullableReferenceTypes.md` | 711 | 4 | Reference | **split** | The type ruling already contains the split: it was typed **Reference** on the grounds that "the migration steps are not where the durable value is". So extract `Migration Guide` (166–430, **264 lines**, more than a third of the page) as a how-to and leave the reference behind. The ruling and the split agree |
| `AgreementDispatcher.md` | 720 | 3 | How-to | **split** | Typed How-to because "the registration syntax is the point of the page, not the pattern discussion around it" — which again names the split. Core keeps `Registration Syntax` (231), `Synchronous and Asynchronous Registration` (293), `Complete Example` (475). Explanation out: `Standard vs Agreement Dispatcher Routing` (11–85), `Use Cases` (85–231), `Performance Implications` (378), `Limitations` (325) |
| `InMemoryOptions.md` | 695 | 3 | Reference | **split — by redistribution** | **Not a mode split.** This is a catalogue: `InMemory Transport` (32), `Outbox` (150), `Inbox` (229), `Scheduler` (297), `Archive` (350) are five unrelated subjects, each belonging beside its own family page. Merge each into the matching family page's InMemory entry; what remains is a genuine testing how-to (`Test Configuration Patterns`, `Complete Testing Example`, `Environment-Specific Configuration`). **Five inbound links** (`ShowMeTheCode`, `FAQ`, `V10MigrationGuide`, `Glossary`, `SUMMARY.md`) must be repointed. This page was also the corpus's worst within-page heading duplicate — 12 instances, since fixed — and the catalogue shape is why |
| `PolicyRetryAndCircuitBreaker.md` | 687 | 3 | How-to | **split** | Two tails, both separable: `Migration Guide: V9 to V10` (377–473) and `Legacy: Using Polly v7 Policies (Deprecated)` (537–687, **150 lines of deprecated material at the end of a current page**). Core keeps the `UseResiliencePipeline` how-to plus `All Available Polly v8 Strategies` (244) as its reference table |
| `Telemetry.md` | 597 | 4 | Reference | **split** | Reference core keeps the per-component span tables (`Command Processor Spans`, `Dispatcher Spans`, `Outbox`/`Inbox`/`Transform Pipeline Tracing`). How-to out: `Configuring OpenTelemetry` (19), `Complete Configuration Example` (366), `Distributed Tracing Example` (462), `Migration from V9` (515) |
| `DynamicMessageDeserialization.md` | 597 | 4 | Explanation | **split** | Typed Explanation, and a how-to is known to be missing — *"how to route several message types down one channel"*. **The material for it is already on the page**: `Using CloudEvents Type for Routing` (75), `Custom Routing Strategies` (158), `Handler Routing` (221), `Configuration Examples` (316). So it is an extraction, not new writing. Explanation core keeps `DataType Channel Pattern` (11), `Dynamic Message Deserialization` (46), `Performance Considerations` (298), `Comparison` (529) |
| `SweeperCircuitBreaking.md` | 527 | 4 | Reference | **split** | Explanation (`Overview`, `How It Work` — **note the typo at line 16**, fix it in passing), Reference core (`Configuration`, `Monitoring and Observability`, `Bulk Dispatch Support`), How-to (`Usage Patterns` 113, `Advanced Scenarios` 447) |
| `BrighterOutboxSupport.md` | 517 | 2 | Explanation | **split** | A family overview carrying two things that are not overview: `Outbox Archiver` (198–349, **151 lines**) and `Complete Example: Transactional Messaging` (349–517, 168 lines). For scale, the dedicated `AzureBlobArchiveProvider.md` is **42 lines** and `OutboxPattern.md` is **45** — the archiver material buried here is three times the size of both put together, and should be its own page |
| `CloudEventsSupport.md` | 475 | 4 | How-to | **split** | **The shape is already ruled**, not proposed: How-to core keeps the name; the parts that are consulted rather than followed become Reference — the required/optional/extension attribute tables (`CloudEvents Attributes`, 17) and the per-transport matrix (`CloudEvents Across Transports`, 205). Highest-confidence row in this file |
| `DefaultMessageMappers.md` | 478 | 4 | How-to | **split — with §5c** | Do not treat in isolation: this page is the "default mapper how-to" that `MessageMappers.md` needs to point at. `Configuration Reference` (340–394) is reference; `Transform Pipeline Example` (172–317, 145 lines) belongs with the transforms explanation §5c calls for |
| `MessageMappers.md` | 266 | 2 | Explanation | **split — three ways** | Below both bars and listed anyway, because the three-way break is a maintainer ruling and carries a correctness fix. See §5c |

### 6e. Keep — the score does not justify a split

| Page | Lines | Score | Type | Verdict | Why |
|---|---|---|---|---|---|
| `V10MigrationGuide.md` | 891 | 3 | How-to | **keep — outside Diátaxis** | See §3 |
| `FAQ.md` | 649 | 3 | Reference | **keep — outside Diátaxis** | See §3 |
| `Glossary.md` | 591 | 2 | Reference | **keep — outside Diátaxis** | See §3 |
| `AsyncAPISupport.md` | 516 | 3 | How-to | **keep** | Coherent how-to. But `Complete Examples` (257–506) is **half the page** — the fix is editorial (trim it, or move it to `samples/` and link), not a split |
| `RequestValidation.md` | 501 | 3 | How-to | **keep** | The three provider sections (`FluentValidation` 47, `DataAnnotations` 26, `Specification` 69) look like reference overspill but are each too small to stand alone, and a reader choosing a provider wants them side by side |
| `HowServiceActivatorWorks.md` | 486 | 3 | Explanation | **keep — but fold** | `Dispatcher Configuration` (147–223, 76 lines) now **overlaps `DispatcherConfigurationReference.md`**, which Spec 011 created on 2026-08-05. That split did not check this page. Fold the overlap into the reference page and link; do not split the explanation |
| `ReactorAndProactor.md` | 442 | 4 | Explanation | **keep** | **The standing counter-example: 442 lines, all four modes, one argument.** The four signals are the supporting material of a single explanation, and every extractable part (`Configuration` 32 lines, `Migration from V9 to V10` 35 lines) is too small to be a page. If a rule ever splits this, the rule is wrong |
| `UsingTheContextBag.md` | 471 | 3 | How-to | **keep** | `Request Context Capabilities` is 294 of the 471 lines — one section, not a mode mix. `Well-Known Context Bag Keys` (332–366) is the only reference-shaped piece and is 34 lines |
| `PipelineValidation.md` | 379 | 3 | How-to | **keep** | Too small; the guidance signal comes from `Common Mistakes and Fixes`, which is exactly where such material belongs on a how-to |
| `PolicyFallback.md` | 335 | 3 | How-to | **keep** | Too small |
| `TestDoubleOptions.md` | 265 | 3 | How-to | **keep** | Too small |
| `TickerQScheduler.md` | 234 | 3 | Reference | **keep** | See §6a |
| `HandlerFailure.md` + `ErrorHandlingOptions.md` | 468 + 221 | 2 + 2 | Explanation + Reference | **keep — paired** | Below the bar, listed because the maintainer raised a live question about them. They are **the corpus's best existing example of the explanation/reference split this programme is arguing for everywhere else**: `HandlerFailure.md` explains the strategies and how to choose, `ErrorHandlingOptions.md` documents the `Subscription` properties that implement them. **Recommended: keep separate and make the relationship explicit** — a *Prerequisites* segment in the banner plus a reciprocal link. Today only `ErrorHandlingOptions.md` points at `HandlerFailure.md`; the reverse pointer is missing. The merge remains open, but merging yields ~685 lines carrying two modes, which is what everything else here is pulling apart |

---

## 7. Content defects found while compiling this list

Small, unrelated to filing, and cheap to fix while the page is open:

- **`contents/SweeperCircuitBreaking.md:16` — `## How It Work`.** Missing "s". Single
  occurrence in the corpus.
- **`contents/HowServiceActivatorWorks.md:147` duplicates
  `contents/DispatcherConfigurationReference.md`.** Created by Spec 011's own split on
  2026-08-05, which did not check the explanation page for overlapping configuration
  material. 76 lines.
- **`contents/QueriesAndQueryObjects.md:746` `## Query Patterns` versus the whole
  `contents/QueryPatterns.md` page.** Verify before moving either.

## 8. What belongs to other specs, not to 010

Recorded here so 010 does not absorb them by accident.

**Spec 013 — four explanations are missing their how-to.** A recurring shape: a page
explains a mechanism well and leaves the reader with no task-shaped route through it.

| Existing Explanation | Missing How-to |
|---|---|
| `ClaimCheck.md` (68 lines) | How to put a large payload behind a claim check |
| `DynamicMessageDeserialization.md` | How to route several message types down one channel — **extractable, see §6d** |
| `QueriesAndQueryObjects.md` | How to write a query and its handler |
| `MessageMappers.md` | How to use the default mapper — **see §5c** |

Also committed publicly on [Docs#67](https://github.com/BrighterCommand/Docs/issues/67):
a **PostgreSQL-for-both-transport-and-outbox** how-to in 013's first batch. No such page
exists today.

**Spec 009 — the corpus contains no tutorial.** All 110 pages are Reference (50), How-to
(33) or Explanation (27); **zero are Tutorial**. That is measured, not asserted, and it
is the substance of #67. Two things in this list are relevant to 009 and should not be
consumed by 010:

- `CQRSWithBrighterAndDarker.md`'s `## Example: E-Commerce Order System` (226 lines) —
  the closest thing the corpus has to end-to-end tutorial material.
- `ShowMeTheCode.md` (223 lines, typed How-to) sits in the first Overview slot and has
  been standing in for the tutorial ladder 009 is building. Its type settles what banner
  it carries today, not what happens to it.

---

## 9. Before you start, and after every split

```bash
python3 tools/linkcheck.py     # MISSING FILE / MISSING ANCHOR / WRONG CASE / ORPHAN
python3 tools/pagelint.py      # banners, heading qualification, code blocks
```

Both are in CI (`.github/workflows/docs.yml`) and both fail the build. As of 2026-08-05
the tree is **clean at 112 files** and **0 errors / 836 warnings across 110 pages**; the
warnings are a deliberate, counted `using`-directive debt. **Any error you see is
yours.**

Three obligations that bite on every split:

- **Every new page needs a `SUMMARY.md` entry.** `linkcheck.py`'s orphan check has no
  exemptions — a page not reachable from `SUMMARY.md` fails the build.
- **Every new page needs a banner** immediately below its H1, separated by one blank
  line: `> **Reference** · Applies to **Brighter V10** · Prerequisites: [...](...)`.
  The separator is ` · ` (U+00B7). A split is exactly the moment the *Prerequisites*
  segment earns its place — the new page usually has one, and it is the core.
- **Every `##` heading must be qualified by its subject** and unique across pages —
  `## Kafka Subscription Configuration`, not `## Configuration`. The five navigation
  headings (`Further Reading`, `Related Documentation`, `See Also`, `Next Steps`,
  `References`) are exempt and must stay uniform.
