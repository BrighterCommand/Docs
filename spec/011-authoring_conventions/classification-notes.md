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

## 10. Task 3.2 is complete

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
