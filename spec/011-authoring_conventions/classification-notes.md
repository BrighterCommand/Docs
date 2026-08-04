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

## 5. Still open

- **`Logging.md` is a 3-line stub** — an H1 and the word `TODO`. It is linked from
  `SUMMARY.md`, so it is published and navigable, and the orphan check cannot see it
  precisely *because* it is linked. Classified `Reference` to unblock the sweep, but
  that is a banner on nothing. Belongs to Spec 013 as a content gap. **No ruling yet.**
- **Rule 5 misses the two-word spelling.** `pagelint.py` matches `ServiceActivator` but
  not `Service Activator`, which appears **19 times across 11 pages** — worst are
  `DispatchingARequest.md` (4) and `BrighterControlAPI.md` (3). `CLAUDE.md`'s pitfall
  list treats both spellings as the same violation. Widening the rule to
  `Service\s*Activator` takes the pre-Task-5.1 remediation from 11 findings to 30.
  **No ruling yet.**
- **76 rows still to review** — the `medium` and `high` confidence proposals. Task 3.2
  is not complete until all 105 verdicts are filled, and `apply_banners.py` will refuse
  to run while any is blank.
