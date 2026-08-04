# Classification notes — Task 3.2

**Created:** 2026-08-04
**Source:** the maintainer's review of the 29 unproposed rows in
[`pagetypes.tsv`](pagetypes.tsv)

Design §1 says the difficulty of classifying a page *is* the worklist signal, and that
it has to be captured as the review happens because it is not recoverable afterwards.
This file is that capture. Task 7.1 reads it; so does Spec 010, and — for the two new
pages called for below — Spec 013.

Everything here is a maintainer ruling, not an inference from the corpus. Where a
ruling contradicts something already written down, the contradiction is called out
rather than quietly reconciled.

---

## 1. `BasicConcepts.md` does **not** merge into `Glossary.md`

**This reverses a scoped deliverable of Spec 010.** Its README currently carries the
merge in three places:

| Line | Text |
|---|---|
| 23–24 | "**Two glossaries.** `BasicConcepts.md` (24 terms) occupies one of three prime Overview slots and is a subset of `Glossary.md`" |
| 55–56 | Scope: "Merge `BasicConcepts.md` into `Glossary.md`, preserving every definition and its contextual prose" |
| 82–83 | Out of scope: "Editing page *bodies*, beyond the `BasicConcepts` → `Glossary` merge" |

**The maintainer's ruling:** the separation is deliberate and is to be kept. The goal is
that a newcomer can understand the key terms **without reading the whole glossary**.
`BasicConcepts.md` is a curated 24-term orientation set; `Glossary.md` is the complete
100-term reference. Those are two different jobs, and the smaller one is not redundant
just because its terms also appear in the larger.

The audit called this "two glossaries" and treated the subset relationship as
duplication. That was a misreading: it measured the overlap and inferred a defect from
it, without asking why the smaller set existed.

**What to do instead:** consider **greater use of links** — from `BasicConcepts.md`
into the fuller `Glossary.md` entry for each term, so a reader who wants depth has one
click rather than a second page to find. That is an addition, not a merge, and it is
cheap now that every glossary link carries an `#anchor` and `linkcheck.py` checks
anchors.

**Corroborating evidence this is a live convention, not an accident:** specs 002 and
006 both treat `BasicConcepts.md` as a maintained artefact. Spec 002 Task 3.1 added
three terms to it (DLQ, Nack, Poison Message) as a deliberate deliverable, and spec 006
lists it as a link target "on first use of *Command* / *Request*". Two completed specs
have been feeding this page on purpose.

**Action:** Spec 010's README must be amended before its requirements are written.
Done 2026-08-04 — see the note in that file.

---

## 2. `HandlerFailure.md` and `ErrorHandlingOptions.md` are one explanation and its
reference

Confirmed types — `HandlerFailure.md` **Explanation**, `ErrorHandlingOptions.md`
**Reference** — but the maintainer's point is about the *pairing*, which neither page
currently makes explicit:

> They relate to each other as an explanation of how we handle errors. Although
> `ErrorHandlingOptions.md` feels like a reference for the explanation in
> `HandlerFailure.md`, so it might need merging (possibly falls afoul of linking
> limits) or one used as reference for the other.

Two live options, and the choice is Spec 010's:

- **Keep them separate and make the relationship explicit.** `HandlerFailure.md`
  explains the strategies and how to choose; `ErrorHandlingOptions.md` documents the
  `Subscription` properties that implement them. This is the shape 011's conventions
  already assume, and the banner's *Prerequisites* segment plus a reciprocal link in
  each direction is the mechanism. **Recommended** — it is the split we are asking for
  everywhere else, and here the corpus already has it.
- **Merge them.** The concern the maintainer raises is real: a reference that is only
  reachable through an explanation, or vice versa, can leave a reader bouncing between
  pages for one answer. If merged, the result is ~685 lines carrying two modes, which
  is what the rest of this spec is pulling apart.

Both pages already cross-link (`ErrorHandlingOptions.md` opens by pointing at
`HandlerFailure.md`). What is missing is the *reverse* pointer and a banner that names
the relationship.

**For the worklist:** record as `keep — paired`, with the merge question flagged rather
than closed, and note that the pair is the corpus's best existing example of the
explanation/reference split this spec is arguing for.

---

## 3. Page-type verdicts, with the rulings that overrode the proposal

| Page | Proposed | **Verdict** | Note |
|---|---|---|---|
| `AgreementDispatcher.md` | Explanation | **How-to** | The registration syntax is the point of the page, not the pattern discussion around it |
| `CloudEventsSupport.md` | Explanation | **How-to** | Split, with the attribute tables and per-transport matrix as Reference overspill — see §4 |
| `NullableReferenceTypes.md` | How-to | **Reference** | "We switched to this with V10 but most folks should understand" — the migration steps are not where the durable value is |
| `ShowMeTheCode.md` | Explanation | **How-to** | "Intended as introductory How-to." Design §1 had ruled out Tutorial and called Reference a poor fit; How-to was the reading nobody proposed |
| `ClaimCheck.md` | Explanation | **Explanation** | Confirmed, but a How-to is missing — see §4 |
| `DynamicMessageDeserialization.md` | Explanation | **Explanation** | Confirmed, but a How-to is missing — see §4 |
| `MessageMappers.md` | How-to | **Explanation** | And it needs breaking up — see §4 |
| `QueriesAndQueryObjects.md` | Explanation | **Explanation** | Confirmed, but a How-to is missing — see §4 |
| `BasicConcepts.md` | Reference | **Reference** | Type confirmed; the merge is what changed — see §1 |
| `HandlerFailure.md` | Explanation | **Explanation** | Confirmed — see §2 |
| `ErrorHandlingOptions.md` | Reference | **Reference** | Confirmed — see §2 |

The other 18 rows were accepted as proposed.

**`ShowMeTheCode.md` is worth a second look at Spec 009.** It is now a How-to, it sits
in the first Overview slot, and 009 is building the tutorial ladder that page has been
standing in for. Design §1 flagged it as "a showcase; Spec 009 addresses the underlying
problem". The verdict does not settle what happens to it — only what banner it carries
today.

---

## 4. Missing pages this review surfaced

A recurring shape: a page explains a mechanism well and then leaves the reader without
a task-shaped route through it. **Four explanations are missing their how-to.** These
belong to **Spec 013**, and they are evidence for 013's scope rather than 011's work.

| Existing Explanation | Missing How-to |
|---|---|
| `ClaimCheck.md` | How to put a large payload behind a claim check |
| `DynamicMessageDeserialization.md` | How to route several message types down one channel |
| `QueriesAndQueryObjects.md` | How to write a query and its handler |
| `MessageMappers.md` | How to use the default mapper — see below |

### `MessageMappers.md` needs breaking into three

The maintainer's ruling, in full:

> Is Explanation but there should be a How-to focused on the default mapper, with this
> referred to for custom mappers. Transformers should be another explanation, but we
> should be clear that they need a custom mapper and form part of the pipeline.

That gives a three-page target:

| Page | Mode | Content |
|---|---|---|
| A default-mapper How-to | How-to | The common path. **`DefaultMessageMappers.md` (476 lines) already exists and is classified How-to** — so this may be a matter of pointing at it and making it the default route, not writing a new page. Establish that first |
| `MessageMappers.md` | Explanation | What a mapper is, the `Message` structure, and **when you need a custom one** |
| A transforms page | Explanation | Wrap / Unwrap / Transform and the transformer factory, currently `## Transformers` inside `MessageMappers.md`. **Must state that transforms require a custom mapper and form part of the pipeline** — the ruling is explicit that this is not currently clear |

The last point is a *correctness* gap, not a filing one: a reader can currently come
away thinking transforms work with the default mapper.

### `CloudEventsSupport.md` splits How-to plus Reference

473 lines, 4 modes, and the top split candidate from the mode analysis. The maintainer's
shape: a **How-to** core, with the parts that are consulted rather than followed
spilling into **Reference** — the required/optional/extension attribute tables and the
per-transport matrix (RabbitMQ, Kafka, SNS/SQS, Azure Service Bus).

This is the same shape as 011's own two demonstrator splits: the How-to keeps the
original file name so the published URL does not move, and the Reference is the new
page.

---

## 5. Family overview pages are Explanation, not Reference

**Ruled 2026-08-04: "agreed it's a pattern."**

Several sections have an overview page sitting above a set of per-technology pages.
The implementations are Reference — parameters, options, behaviour. The overview's job
is different: it explains **which one to pick and why**, which is Explanation.

`SUMMARY.md` section skew proposed Reference for all of them, because it cannot tell an
overview from an implementation. Four pages move:

| Page | Proposed | **Verdict** |
|---|---|---|
| `BrighterSchedulerSupport.md` | Reference | **Explanation** |
| `DistributedLock.md` | Reference | **Explanation** |
| `BrighterOutboxSupport.md` | Reference | **Explanation** |
| `BrighterInboxSupport.md` | Reference | **Explanation** |

`BoxProvisioning.md` was already ruled Explanation in §3 and is the same shape — it is
the pattern's first instance rather than an exception to it.

**For Spec 010:** this is a navigational fact as well as a modal one. Each of these
pages is the entry point to a family, and the restructure should keep them there.

## 6. The scheduler pages are one split, applied seven times

Not a page-type finding — all seven stay Reference — but the strongest single entry for
Task 7.1's worklist.

`AwsScheduler` (773L), `AzureScheduler` (715L), `HangfireScheduler` (830L),
`QuartzScheduler` (767L), `TickerQScheduler` (232L), `InMemoryScheduler` (539L) and
`PostgreSQLMessageBroker` (660L) follow an **identical template**:

```
Overview / When to Use  ->  How Brighter Integrates  ->  NuGet Packages
->  Configuration  ->  Code Examples  ->  Best Practices  ->  Troubleshooting
->  Migration from Other Schedulers  ->  Summary
```

Every one scores 3–4 modes. Recording them as seven separate worklist rows would hide
the fact that **one split decision covers all seven** — Reference core keeps the file
name, the *When to Use* / *Comparison* material becomes Explanation, and
*Migration from Other Schedulers* becomes a How-to. Spec 010 should settle the shape
once and apply it, not relitigate it per scheduler.

## 7. Two verdicts applied on the assistant's recommendation

Both were put to the maintainer with reasoning and neither was explicitly ruled on. The
maintainer's instruction was "bulk-confirm, I can raise any exceptions later", so they
are applied — but they are flagged here because one contradicts an approved design, and
neither should be mistaken for a maintainer decision.

- **`CustomScheduler.md` → How-to** (proposed Reference). Its content is
  *Implementation Steps* — twice, which is also why it is a rule 3b page. It is a
  build-your-own guide, not a reference.
- **`V10MigrationGuide.md` → How-to** (proposed Reference). **This contradicts design
  §1**, which argued Reference on the grounds that the page is "consulted rather than
  read through". The page is structurally *Before You Start → Step 1 … Step 6 →
  Rollback Plan*. That is a how-to in every structural sense, and the design's argument
  does not survive contact with the outline. Note this does *not* conflict with the
  `NullableReferenceTypes.md` ruling in §3: there the migration steps are a section of a
  page about a language feature, here they are the whole page.

  If this is overruled, design §1's *Pages expected to need argument* list and the
  `pagetypes.tsv` verdict both need reverting.

## 8. Rule 5 widened to both spellings

**Ruled 2026-08-04: yes.** `pagelint.py` matched `ServiceActivator` but not
`Service Activator`, and `CLAUDE.md`'s pitfall list treats them as one violation.
`SERVICEACTIVATOR_RE` is now `Service\s*Activator`.

Findings went from **11 to 28**, covering 30 occurrences across 12 pages — two lines
carry both spellings, and the rule reports once per line.

| Page | Findings |
|---|---|
| `BrighterBasicConfiguration.md` | 8 |
| `BrighterControlAPI.md` | 4 |
| `HowServiceActivatorWorks.md` | 3 |
| `BasicConcepts.md`, `DispatchingARequest.md`, `HealthChecks.md`, `ImplementingExternalBus.md` | 2 each |
| `AzureServiceBusConfiguration.md`, `CQRSWithBrighterAndDarker.md`, `KafkaConfiguration.md`, `RabbitMQConfiguration.md`, `WhyBrighter.md` | 1 each |

All 28 must reach zero before Task 5.1 puts `pagelint.py` in CI. Most are identifiers
written in **bold** where `CLAUDE.md` asks for backticks; `HowServiceActivatorWorks.md`
takes the `<!-- pagelint: allow-serviceactivator -->` opt-out.

### Cleared 2026-08-04 — `741890f`, 28 insertions and 28 deletions

Rule 5 is at **zero**. `pagelint.py` went 324 → 296 errors with warnings unchanged at
840, so no code block was touched; `linkcheck.py` clean at 107 files. Every changed
line is one of the 28 findings and nothing else.

The 28 turned out to be three kinds, not the two the table above implies, and each
wanted a different fix:

| Kind | Pages | Fix |
|---|---|---|
| Prose that means the Dispatcher | `AzureServiceBusConfiguration`, `KafkaConfiguration`, `RabbitMQConfiguration`, `BrighterControlAPI`, `CQRSWithBrighterAndDarker`, `DispatchingARequest`, `HealthChecks` | renamed to "Dispatcher" |
| Identifiers in **bold** | `BrighterBasicConfiguration` ×7, `BrighterControlAPI` ×1, `WhyBrighter` ×1 | backticks |
| The term as the page's own subject | `BasicConcepts` ×2, `ImplementingExternalBus` ×1, `BrighterBasicConfiguration` ×1, `HowServiceActivatorWorks` ×3 | per-line opt-out |

The first kind is the rule earning its place. The three transport pages carried an
identical sentence — "the material on configuring *Service Activator* in [Basic
Configuration](…#configuring-the-dispatcher)" — whose own link target says
*dispatcher*. That is the terminology drift stated and contradicted in one line, three
times, and nobody had noticed.

**Deviation from the remediation planned above: `HowServiceActivatorWorks.md` takes
per-line opt-outs, not the page-level one.** The page-level comment disables rule 5 for
all 460 lines, and only three of them are about the name; the rest use "Dispatcher"
correctly and should keep being checked. `check_terminology` honours a trailing
`<!-- pagelint: allow-serviceactivator -->` as well as a whole-line one, so the narrow
form was available. Same reasoning for `BrighterBasicConfiguration.md:704`, which cites
the Enterprise Integration Patterns pattern by name and then explains why we do not use
it — one line out of 1,068.

**Also a deviation: the `BrighterBasicConfiguration.md` fixes were not deferred to
Task 6.9.** The plan was to make them while splitting the page, on the grounds that it
is being edited there anyway. But Task 5.1 needs rule 5 at zero, Phase 6 is after
Phase 5, and a linter that cannot go into CI until a page split lands is a linter
gated on the largest remaining piece of work. Nine backtick changes now; the split
inherits correct prose.

**Two renames asserted API facts, and both were checked against Brighter source rather
than inferred** — the kind of claim this programme has been caught on before:

- The Control API's "node" **is** a Dispatcher.
  `DispatcherExtensions.GetNodeStatus(this IDispatcher dispatcher)` sets
  `NodeName = dispatcher.HostName.Value`
  (`src/Paramore.Brighter.ServiceActivator.Control/Extensions/DispatcherExtensions.cs:8`)
- The health check **is** a Dispatcher health check. `BrighterServiceActivatorHealthCheck`
  reads `((Dispatcher)_dispatcher).Subscriptions` and compares live consumers against
  `NoOfPerformers`
  (`src/Paramore.Brighter.ServiceActivator.Extensions.Diagnostics/HealthChecks/`)

### The gap this exposed: rule 5 cannot see headings

`Page._parse()` appends heading lines to `self.headings` and *everything else* to
`self.prose`, and `check_terminology` iterates `page.prose`. So a heading containing the
term is invisible to rule 5. Three exist:

| Heading | Legitimate? |
|---|---|
| `contents/BasicConcepts.md:135` `## Service Activator` | yes — the term is the entry being defined |
| `contents/Glossary.md:111` `### ServiceActivator` | yes — same |
| `contents/HowServiceActivatorWorks.md:454` `## Relationship to ServiceActivator Assembly` | yes — about the assembly name |

All three are fine today, so nothing is broken. What is missing is the *check*: nothing
stops a future `## Configuring Service Activator` from passing.

**Not fixed here, because the obvious fix breaks anchors.** Widening rule 5 to headings
needs an opt-out those three headings can use, and the trailing-comment form cannot be
it — `## Service Activator <!-- pagelint: allow-serviceactivator -->` puts the comment
inside the heading text, so `slug()` folds it into the anchor and every inbound link to
`#service-activator` breaks. `linkcheck.py` would catch that, but a convention whose
opt-out breaks a different rule is the wrong convention. An opt-out on the *preceding*
line is the likely answer. **Recorded for the maintainer as a rule-scope decision, not
taken unilaterally** — it changes rule 5's meaning, and `CLAUDE.md`'s ledger describes
rule 5 as applying to prose.

## 9. The banner sweep, and two things it nearly carried (Task 3.4, 2026-08-04)

105 files, **212 insertions, zero deletions**, every inserted line a banner or a blank.
Rules 1 and 2 report zero; `linkcheck.py` clean.

Two defects were caught only because the diff was inspected rather than trusted, and
both are worth recording because the next mechanical sweep will be tempted the same way:

- **The first run produced a banner with no blank line after it**, running straight into
  the following `##`. **`pagelint.py` passed anyway** — rule 1 only asks what the first
  non-blank line after the H1 is, and by that test the page was perfect. A green linter
  is not the same as a correct page, and no rule here can tell you so.
- **The second run silently added a trailing newline to the 18 files that lacked one.**
  Harmless in itself, and arguably an improvement, but it is not what a commit claiming
  to be nothing but banner insertions should contain. `apply_banners.py` now preserves
  the original ending. The 18 are listed in the commit; tidying them is a separate
  change if it is worth making at all.

**Prerequisites were omitted from every banner.** The segment is optional by design and
choosing prerequisites is a per-page judgement — including it would have stopped the
sweep being a sweep. They get added as pages are edited for other reasons, which means
the *Prerequisites* half of the banner grammar is currently unexercised across the
corpus. Worth a look at Task 3.5's rendered preview.

## 10. Darker is a sister project, and the classification had not treated it as one

**Maintainer, 2026-08-04:** *"Darker is Brighter's sister project, the query side to
Brighter's command side. So its documentation sits alongside. It often parallels, but
it is not the same."*

Checking the classification against that turned up two wrong calls and one structural
finding.

### The parallel pairs, and where they diverged

| Concept | Brighter | | Darker | |
|---|---|---|---|---|
| Basic configuration | `BrighterBasicConfiguration.md` | How-to | `DarkerBasicConfiguration.md` | How-to |
| The request / query object | `Requests, Commands and Events.md` | Explanation | `QueriesAndQueryObjects.md` | Explanation |
| Implementing a handler | `ImplementingAHandler.md` | How-to | `ImplementAQueryHandler.md` | How-to |
| The pipeline | `BuildingAPipeline.md` | How-to | `QueryPipeline.md` | ~~Explanation~~ → **How-to** |
| Patterns | *(none)* | | `QueryPatterns.md` | How-to |

Four of five already lined up. **`QueryPipeline.md` was the one that did not, and it was
my misreading rather than a real divergence** — I took its lead from *Introduction* and
*How the Query Pipeline Works* and classified it Explanation, when the payload is
*Available Decorators · Decorator Patterns · Configuring Polly Policies*. A reader who
wants logging or retry on a query handler comes here to do a task. Corrected to
**How-to**, which also restores the parallel.

### `WhyBrighter.md` applies to both, and plainly so

Its opening sentence is *"So why would you choose **Brighter & Darker**?"*, its
*Command Query Separation* section exists to explain that Brighter modifies state and
Darker reads it, and *Type over Convention* opens *"Brighter & Darker recognize…"*.
Filed as `Brighter V10` because the title says "Why Brighter?" — a title-shaped
judgement about a page whose content says otherwise. Corrected to
**`Brighter and Darker V10`**.

### The structural finding: Darker's docs are not thin, they are differently shaped

The interesting number is the reverse of what "sister project" might suggest.
`QueryPipeline.md` is **928 lines**; its Brighter counterpart `BuildingAPipeline.md` is
**177**.

The Darker page is five times the size because it absorbs material that on the Brighter
side lives in separate pages — decorators, Polly policy configuration, and resilience
patterns are all inside `QueryPipeline.md`, whereas Brighter splits them across
`BuildingAPipeline.md`, `PolicyRetryAndCircuitBreaker.md` and `PolicyFallback.md`.

So the asymmetry is **architectural, not editorial**: the same subject is decomposed one
way on one side and not at all on the other. That is a worklist entry for Task 7.1 and a
filing question for Spec 010 — not something to fix by moving a banner.

### There is no Darker V10 — the banner vocabulary was wrong (2026-08-04)

Design §1 fixed the vocabulary as `Brighter V10 | Darker V10 | Brighter and Darker V10`,
assuming Darker tracks Brighter's version line. **It does not.**

| | |
|---|---|
| `Paramore.Darker` latest release | **4.1.1** |
| Published prereleases | none |
| Tags | 4.1.1, 4.1.0, 4.0.1, 4.0.0, 3.0.0, 2.0.79 |
| Local `../Darker` HEAD | `4.1.1-7-g2f76cda` — 7 commits ahead of the tag |

So "Darker V10" is a version that has never existed, and the sweep put that claim on
**10 pages**, `Glossary.md` and `ShowMeTheCode.md` among them. The banner's whole reason
for existing is to stop a reader — or a model — acting on the wrong version, so a false
version marker is the worst thing it could carry.

**Corrected** to `APPLIES_TO = ('Brighter V10 and Darker V4', 'Brighter V10',
'Darker V4')`, defined once in `tools/pagelint.py`, imported by `apply_banners.py`, and
documented in `CLAUDE.md`. Five pages now read `Darker V4` and five
`Brighter V10 and Darker V4`.

**Deferred, on the maintainer's instruction:** Darker's next release is in flight and its
source is ahead of what is deployed, so the *content* of the Darker pages is not updated
now. They are positioned instead — the ten are identifiable as a set from the `applies`
column in `pagetypes.tsv`, and bumping them when the release lands is one edit to
`APPLIES_TO`, one to that column, and a re-run of `apply_banners.py`.

Do not update Darker page content against the `../Darker` working tree in the meantime:
it documents behaviour that is not released, and the docs site publishes the deployed
version.

### The gap this exposed in `apply_banners.py`

Retargeting the ten failed on the first attempt, and instructively. The script decided
whether a blockquote was its own by matching `BANNER_RE` — but those ten banners had been
written under the *old* vocabulary, so they no longer matched, and it correctly refused
to touch what it took for page content. It reported `10 FOREIGN BLOCKQUOTE` and wrote
nothing.

The distinction it lacked is between **"is this banner valid"** and **"is this banner
ours"**. Those differ exactly when a vocabulary changes — which is not an edge case: it
is what happens at every version bump, and the V11 bump will hit it across all 105 pages.

`BANNER_SHAPE_RE` now answers the second question structurally, and `BANNER_RE` keeps
answering the first. `apply_banners.py` replaces on shape; `pagelint.py` validates on
grammar. A page whose banner is stale reports `BANNER MALFORMED` — which is how the ten
surfaced — rather than being silently skipped.

### An open question this raises about the banner itself

`Applies to **Brighter V10**` now sits on **96 pages**, and on a cross-cutting page that
is an *exclusion claim*: it tells a Darker reader the page is not for them. Some are
certainly right — `RequestValidation.md` says in its own text that validation applies to
Brighter requests and not Darker queries. Others are unverified, and the likely
candidates are the middleware and resilience pages: `PolicyRetryAndCircuitBreaker.md`,
`PolicyFallback.md`, `UsingTheContextBag.md`, `FeatureSwitches.md`, `Telemetry.md`.

`QueryPipeline.md` documents Polly policies for Darker, so at least the resilience story
is shared in substance even where the pages are not. **Establishing which of those 96
genuinely exclude Darker needs the Darker source, not the docs**, and it is out of scope
here — but the banner has made an implicit claim explicit on every page at once, which
is the first time it has been checkable at all.

## 11. Task 3.2 is complete

**105 of 105 verdicts filled**, so `apply_banners.py` will run. Final distribution:

| Type | Pages |
|---|---|
| Reference | 48 |
| How-to | 30 |
| Explanation | 27 |

No page took `Tutorial`. That is correct today and is exactly the gap Spec 009 exists to
fill — the corpus has no tutorial, which is the substance of the criticism in
[Docs#67](https://github.com/BrighterCommand/Docs/issues/67).

The 59 `medium`/`high` rows outside the family-overview pattern were bulk-confirmed as
proposed, with exceptions to be raised later.
