# Tasks: Replay On Seen (Spec 008)

**Created:** 2026-08-02
**Status:** Draft — awaiting review
**Traceability:** Implements `design.md` (approved 2026-08-02), which traces to
`requirements.md` (approved 2026-08-01). Scope items are cited as **[P0-n]**,
**[P1-n]**, **[P2-n]**.

## Overview

**Total tasks: 33**, across 4 phases.

| Phase | Goal | Tasks |
|-------|------|-------|
| 1. Research & Verification | Lock down every API name, version number, and store capability before a word is written | 5 |
| 2. Core Documentation (P0) | Write the P0 half of `ReplayOnSeen.md`, then the P0 edits to five existing pages | 10 |
| 3. Supporting Documentation (P1/P2) | Finish `ReplayOnSeen.md`, then the P1/P2 edits to four existing pages | 12 |
| 4. Polish & Review | SUMMARY.md, length, example verification, link check, style, final QA | 6 |

**Priority mapping (from requirements):**

- **P0:** `ReplayOnSeen.md` §problem→prerequisites; `BrighterInboxSupport.md`,
  `BrighterOutboxSupport.md`, `BrighterBasicConfiguration.md`, `BoxProvisioning.md`,
  `BoxProvisioningUpgrade.md`.
- **P1:** `ReplayOnSeen.md` §store support→limitations; `PipelineValidation.md`,
  `DynamoOutbox.md`, `Glossary.md`.
- **P2:** `ReplayOnSeen.md` §worked example, rationale, custom stores;
  `V10MigrationGuide.md`.

**Source-of-truth note:** every C# example and every version number must be verified
against the Brighter source (read only — **never modify** `../Brighter`). Spec 007
shipped two example defects that QA caught late; examples 5 and 9 here are written
from scratch and carry the same risk, so they are verified as they are written
(Task 4.3 is a second pass, not the first).

---

## Phase 1 — Research & Verification

> Goal: no example ships a guessed symbol and no table ships a stale version.
> Must complete before Phase 2. Tasks 1.1–1.5 are mutually independent.

- [x] **Task 1.1:** Verify the configuration surface — enum, attributes, `InboxConfiguration`
  - Input: `../Brighter/src/Paramore.Brighter/Inbox/OnceOnlyAction.cs`,
    `Inbox/Attributes/UseInboxAttribute.cs` and the async variant,
    `Inbox/InboxConfiguration.cs:70`,
    `../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/TestDoubles/ProcessAndForwardHandler.cs:55`
  - Output: a confirmed-names note in the spec directory (`verification-notes.md`) —
    exact `[UseInbox]` / `[UseInboxAsync]` parameter names and order, the
    `InboxConfiguration` constructor parameter list, and the `Replay` member's XML doc
  - Notes: Confirm `ActionOnExists` is constructor-set, not a settable property
    (design §1 "Turning It On" asserts this). Feeds Examples 1, 2, 3.

- [x] **Task 1.2:** Verify the runtime mechanics — role interfaces, handler branch, telemetry
  - Input: `../Brighter/src/Paramore.Brighter/Inbox/IAmACausationTrackingInbox.cs`,
    `IAmACausationTrackingOutbox.cs`, `Inbox/Handlers/UseInboxHandler.cs` + async variant,
    `RequestContextBagNames.cs:143`,
    `../Brighter/docs/adr/0057-replay-outbox-on-inbox-duplicate.md`
  - Output: appended to `verification-notes.md` — the interface method signatures
    (`GetCausationId`, `ReplayCausation`, `SupportsCausationTracking`), the exact span
    event names, the `paramore.brighter.causation_id` tag string, and the
    `CustomContextDisablesReplay` warning text
  - Notes: Confirm the bag key literal is `"Brighter-CausationId"` and that the default
    Causation Id is the request's `Id`. Feeds §Causation Id, §Observability, Example 12.

- [x] **Task 1.3:** Verify migration versions and column casing
  - Input: `../Brighter/src/Paramore.Brighter.BoxProvisioning.*/` —
    `MsSql*MigrationCatalog.cs`, `MySql*`, `PostgreSql*`, `Sqlite*` (Outbox and Inbox)
  - Output: appended to `verification-notes.md` — a per-backend version table (Outbox
    max version, Inbox max version, the causation migration's `Description`), plus
    confirmation that the Outbox V8 migration creates an **index** as well as a column
  - Notes: **Do not copy the numbers from `design.md`** — the requirements' accuracy
    constraint says re-verify at writing time. Record PostgreSQL's lowercase
    `causationid` vs `CausationId` elsewhere. Gates Tasks 2.9 and 2.10.

- [x] **Task 1.4:** Verify store-by-store causation support
  - Input: `InMemoryInbox.cs`, `InMemoryOutbox.cs`, `RelationalDatabaseInbox.cs`,
    `RelationDatabaseOutbox.cs`, `IRelationalDatabase{Inbox,Outbox}CausationQueries.cs`,
    `Paramore.Brighter.{Inbox,Outbox}.DynamoDB{,.V4}/`
    (`DynamoDbOutbox.cs`, `DynamoDbConfiguration.cs:71`, `MessageItem.cs:49`),
    plus the Spanner, MongoDb, and Firestore Inbox/Outbox projects
  - Output: appended to `verification-notes.md` — which stores implement each role
    interface, what `SupportsCausationTracking()` probes on each, and the DynamoDB
    `CausationIndexName` default
  - Notes: The DynamoDB Outbox GSI is the one asymmetry and is easy to flatten by
    mistake — confirm the Inbox needs nothing while the Outbox needs the index. Also
    confirm `DynamoDbTableFactory.GenerateCreateTableRequest<MessageItem>` emits the
    GSI (`DynamoDbTableFactory.cs:60`). Gates Tasks 3.1 and 3.10.

- [x] **Task 1.5:** Verify the validation findings and read the end-to-end test
  - Input: `../Brighter/src/Paramore.Brighter/Validation/HandlerPipelineValidationRules.cs:137`
    (the `ReplayRequiresCausationTracking` rule) and `Validation/PipelineValidator.cs:119`
    (where it is invoked), ADR 0057 §validation,
    `../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/When_a_seen_message_is_replayed_end_to_end.cs`
  - Output: appended to `verification-notes.md` — the five findings with their exact
    message text and severity, and a step-by-step trace of the end-to-end test
    (what each store holds after each step)
  - Notes: Gates Tasks 3.2, 3.6, and 3.9. Confirm there is still no sample in
    `../Brighter/samples/` that uses Replay (requirements open question 2); if one has
    landed, note it — it changes design deviation 2.

---

## Phase 2 — Core Documentation (P0)

> Goal: the P0 spine. Tasks 2.1–2.5 build `ReplayOnSeen.md` top-down and are
> **sequential** (each appends to the file in reading order). Tasks 2.6–2.10 edit
> existing pages, are mutually independent, and can run in parallel with each other
> once 2.1 exists as a link target.

- [x] **Task 2.1:** Write `ReplayOnSeen.md` — intro, the problem, and the cascade  [P0-1, P0-8]
  - Input: `design.md` §1 (sections "The Problem" and "How Replay Walks the Flow
    Forward"); `requirements.md` P0-1 and P0-8; ADR 0057 problem statement
  - Output: `ReplayOnSeen.md` from the H1 to the end of `## How Replay Walks the Flow
    Forward` — 3-sentence intro, the Order → Payment → Shipping stall, why the Inbox
    alone is not idempotency, the cascade stated plainly, and the callout that **every**
    already-seen step needs `OnceOnlyAction.Replay`
  - Notes: Do not use "idempotent" to describe what Replay achieves — the argument is
    that the Inbox is *not* idempotency. State explicitly that the handler is not
    re-executed and that the *same* stored messages are re-dispatched. Link
    `BrighterInboxSupport.md` and `BrighterOutboxSupport.md` in the intro.

- [x] **Task 2.2:** Write `ReplayOnSeen.md` — Causation Id and the duplicate walkthrough  [P0-2, P0-3]
  - Input: Task 1.2 notes; ADR 0057 "Architecture Overview" diagram; `design.md` §1
  - Output: `## Causation Id` (definition, default, how it travels in
    `RequestContext.Bag`, one sentence distinguishing Correlation Id, stored in both
    boxes) and `### What Happens on a Duplicate` (Example 10 — the flow diagram as a
    fenced text block — plus the numbered walkthrough matching it)
  - Notes: Trim the ADR diagram; it is fenced text, not code. End the walkthrough on the
    latency consequence: replay happens on the Sweeper's next interval, not immediately.
    Depends on 2.1.

- [x] **Task 2.3:** Write `ReplayOnSeen.md` — Turning It On  [P0-4]
  - Input: Task 1.1 notes; `ProcessAndForwardHandler.cs:55`;
    `BrighterBasicConfiguration.md:905` for the registration shape
  - Output: `## Turning It On` with `### On a Handler` (Example 1 — complete handler with
    `[UseInbox(..., onceOnlyAction: OnceOnlyAction.Replay, contextKey: ...)]`; Example 2 —
    the `[UseInboxAsync]` variant) and `### Globally` (Example 3 — `AddConsumers` with
    `InboxConfiguration(..., actionOnExists: OnceOnlyAction.Replay)`, abbreviated)
  - Notes: Rename the test's `MyCommand`/`MyEvent` to the Order/Payment/Shipping domain
    used across the page. Note `ActionOnExists` is constructor-set. Link
    `BrighterBasicConfiguration.md#inbox` (established convention — see design's
    known-unverified-link note) and the homogeneous-pipeline anchor from Example 2.
    Depends on 2.2.

- [x] **Task 2.4:** Write `ReplayOnSeen.md` — You Must Thread Your RequestContext  [P0-5]
  - Input: `../Brighter/release_notes.md:315–330`; `ProcessAndForwardHandler.cs:65`
    (`Context as RequestContext`)
  - Output: `## You Must Thread Your RequestContext` — warning callout, the why (the
    causation id lives in the pipeline's `RequestContext.Bag`; a fresh context loses it
    and stores a null `CausationId`), Example 4 as an ❌/✅ pair, the list of affected
    methods (`Post`, `PostAsync`, `DepositPost`, `DepositPostAsync`), and the symptom
  - Notes: This is its own H2, not a pitfall — design deviation 1. It must sit
    immediately after configuration. Say "silent no-op" plainly: no log, no error,
    nothing resent. Depends on 2.3.

- [x] **Task 2.5:** Write `ReplayOnSeen.md` — Before You Enable It  [P0-6, P0-7]
  - Input: `design.md` §1 checklist; `BrighterOutboxSupport.md:182`
    (`### You always need a Sweeper`); `BrighterOutboxSupport.md:253` for Example 6
  - Output: `## Before You Enable It` — the six-row checklist table (requirement | how to
    satisfy | what happens if you don't) and the Sweeper sub-point with Example 6
    (`.UseOutboxSweeper(...)`, abbreviated)
  - Notes: **Link** to "You always need a Sweeper" rather than restating it — that
    section already covers the in-memory Outbox and the RabbitMQ/Kafka async-confirmation
    case. State that replay has no immediate-send path, and that the implicit
    `InMemoryOutbox` supports Replay but still needs a Sweeper. Depends on 2.4.

- [x] **Task 2.6:** Update `contents/BrighterInboxSupport.md`  [P0-9]
  - Input: `design.md` §2; current lines 32–47
  - Output: a third `Replay` sub-bullet in the `OnceOnlyAction` list, one new row in the
    behaviour table (`true` / `Replay`), and a two-sentence paragraph after the table
    naming the prerequisites and linking `ReplayOnSeen.md`
  - Notes: Do not expand the page further — the Inbox page stays about the Inbox. The
    prerequisite sentence exists so nobody enables Replay from this page alone.

- [x] **Task 2.7:** Update `contents/BrighterOutboxSupport.md`  [P0-7]
  - Input: `design.md` §3; current lines 182 and 432
  - Output: a short paragraph appended to `### You always need a Sweeper` (replay clears
    `DispatchedAt` so the Sweeper resends; it is the only replay path) and one sentence
    at the end of `### Consumer: Using Inbox for Deduplication`
  - Notes: Weave into the existing prose, don't rewrite it. Both additions link
    `ReplayOnSeen.md`.

- [x] **Task 2.8:** Update `contents/BrighterBasicConfiguration.md`
  - Input: `design.md` §4; current **line 886** (the design says 884 — the file has since
    shifted by two lines; verified at tasks review 2026-08-02)
  - Output: the `ActionOnExists` bullet extended with `OnceOnlyAction.Replay` as a third
    option, one clause, linking `ReplayOnSeen.md`
  - Notes: One sentence. The bullet currently presents a closed list of two ("The default,
    **OnceOnlyAction.Throw** … The alternative is **OnceOnlyAction.Warn**") and is now
    factually wrong. Added at design review 2026-08-02.

- [x] **Task 2.9:** Correct the version matrix in `contents/BoxProvisioning.md`  [P0-10]
  - Input: **Task 1.3 notes** (not `design.md`'s numbers); current lines 93–96, 101,
    108, 116
  - Output: corrected per-backend matrix rows; corrected version prose at 101 (append the
    causation migration); rewritten asymmetry row at 108; corrected V1-only explanation at
    116 (keep the historical reason PostgreSQL skipped the `ContextKey` migration, correct
    the conclusion — the chain is one shorter, not absent); a note that the Outbox
    causation migration is the first to create an index rather than only columns
  - Notes: These are factual corrections to spec 005's pages, not additions. Verify every
    number against the catalog sources. Depends on 1.3.

- [x] **Task 2.10:** Refresh the examples in `contents/BoxProvisioningUpgrade.md`  [P0-10]
  - Input: Task 1.3 notes; current lines 15, 53, 63, 79, 130
  - Output: version numbers refreshed at 15, 63, 79, 130 (including a consistently
    regenerated sample log block at 63), plus a new bullet under `## What to verify after
    upgrade` on confirming the causation column landed, linking `ReplayOnSeen.md`
  - Notes: Regenerate the log block wholesale rather than hand-patching individual lines —
    check every version number in it. Depends on 1.3.

---

## Phase 3 — Supporting Documentation (P1 / P2)

> Goal: finish the page and hook up the remaining entry points. Tasks 3.1–3.8 continue
> `ReplayOnSeen.md` in reading order and are sequential; 3.9–3.12 are mutually
> independent page edits.

- [x] **Task 3.1:** Write `ReplayOnSeen.md` — Store Support  [P1-11]
  - Input: Task 1.4 notes; Task 1.3 notes for the migration versions
  - Output: `## Store Support` — a matrix table (store | Inbox | Outbox | what you must do
    first) covering relational, Spanner, MongoDb, Firestore, DynamoDB (+.V4), and
    in-memory, with the column-casing note
  - Notes: Never assert a single column casing across stores. DynamoDB's Outbox row links
    to `DynamoOutbox.md`; relational rows link to `BoxProvisioning.md`. Depends on 2.5,
    1.3, 1.4.

- [x] **Task 3.2:** Write `ReplayOnSeen.md` — When Replay Does Not Fire  [P1-12, P1-16]
  - Input: Task 1.5 notes (the five findings verbatim)
  - Output: `## When Replay Does Not Fire` — the startup-findings table (finding |
    severity | cause | fix, five rows) followed by the five runtime silences that produce
    no startup error at all
  - Notes: This is the diagnostic section — target-state item 5 depends on it. Link
    `PipelineValidation.md`. Depends on 3.1, 1.5.

- [x] **Task 3.3:** Write `ReplayOnSeen.md` — Upgrading Without Migrating  [P1-13]
  - Input: ADR 0057 §write-path gate; `RelationDatabaseOutbox.cs`
  - Output: `## Upgrading Without Migrating` — the probe-and-fall-back behaviour, and the
    consequence that the memoized probe result is never invalidated, so provisioning that
    runs after a store instance was built needs a process restart
  - Notes: Link `BoxProvisioningUpgrade.md`. Depends on 3.2.

- [x] **Task 3.4:** Write `ReplayOnSeen.md` — Observability  [P1-14]
  - Input: Task 1.2 notes (event names, tag string, warning text)
  - Output: `## Observability` — the span-events table (path | event name | tags), the
    `paramore.brighter.causation_id` tag, the `CustomContextDisablesReplay` warning, and
    why the re-dispatch is a separate trace with no parent link
  - Notes: Link `Telemetry.md`. Depends on 3.3.

- [x] **Task 3.5:** Write `ReplayOnSeen.md` — Limitations  [P1-16]
  - Input: `requirements.md` P1-16 and the Out of Scope section; ADR 0057 §consequences
  - Output: `## Limitations` — not saga orchestration (no coordinator, no compensation,
    no ordering); the shared-Causation-Id case with its escape hatch; the Sweeper race;
    historical rows that can never be replayed
  - Notes: Draw the orchestration line once and move on — there is no orchestration page
    to link to. Note `UseInbox` is normally applied to Commands. Depends on 3.4.

- [x] **Task 3.6:** Write `ReplayOnSeen.md` — A Worked Example  [P2-17]
  - Input: Task 1.5's trace of `When_a_seen_message_is_replayed_end_to_end.cs`
  - Output: `## A Worked Example` — Example 5 (both handlers: `ProcessPayment`, already
    seen and set to Replay; `ShipOrder`, never ran) followed by numbered steps noting what
    each store holds at each point
  - Notes: **Example 5 is written from scratch** — model it on `ProcessAndForwardHandler`
    and verify every symbol as you write. Cite the test as the reference implementation;
    there is no sample app (unless Task 1.5 found one). Depends on 3.5.

- [x] **Task 3.7:** Write `ReplayOnSeen.md` — Why It Works This Way  [P2-20]
  - Input: ADR 0057 §Alternatives Considered
  - Output: `## Why It Works This Way` — three short paragraphs: why not re-execute the
    handler, why not reuse `CorrelationId`, why the Sweeper rather than an immediate send
  - Notes: Reference the ADR rather than restating it at length. Depends on 3.6.

- [x] **Task 3.8:** Write `ReplayOnSeen.md` — custom stores and Further Reading  [P2-18]
  - Input: Task 1.2 notes (interface signatures); `IAmACausationTrackingOutbox.cs`
  - Output: `## Implementing Causation Tracking in Your Own Store` (the two role
    interfaces, what each method must do, why `SupportsCausationTracking()` must report
    the **live** state, Example 12 — an abbreviated skeleton) and `## Further Reading`
  - Notes: Signpost this section as being for people *writing* a store, not using one.
    Further Reading covers Inbox Support, Outbox Support, Outbox Pattern, Box
    Provisioning, Upgrading Existing Deployments, Pipeline Validation, DynamoDb Outbox,
    Telemetry, Glossary, and ADR 0057. Depends on 3.7. Completes the page.

- [x] **Task 3.9:** Update `contents/PipelineValidation.md`  [P1-12]
  - Input: Task 1.5 notes; `design.md` §5; current lines ~38, ~44, and 323
  - Output: one new row in the `### Handler Pipeline Checks (AddBrighter)` rule table; two
    of the five messages added verbatim to the example-error block (one Error, one
    Warning); a new `### Replay Without Causation Tracking` section in the established
    Before/After shape (Example 7), placed at the **end of `## Common Mistakes and Fixes`,
    immediately before `## Further Reading`**
  - Notes: Keep the page's existing summary-table density — the five findings enumerated
    in full belong on `ReplayOnSeen.md`. Depends on 1.5.

- [x] **Task 3.10:** Add the Causation GSI section to `contents/DynamoOutbox.md`  [P1-11]
  - Input: Task 1.4 notes; `DynamoDbTableFactory.cs:60`; `MessageItem.cs:49`;
    `DynamoDbConfiguration.cs:71`
  - Output: a new `## Replay Support: The Causation Index` **appended at the end of the
    file** (the page is exactly 89 lines and ends inside the handler code fence, so this
    is an append, not a mid-file insertion) — Example 8 (new table, GSI for free from
    `DynamoDbTableFactory`,
    abbreviated) and Example 9 (`UpdateTable` with `GlobalSecondaryIndexUpdates` for an
    existing table, complete), the async-backfill note, the consequence of skipping it,
    and a link to `ReplayOnSeen.md`
  - Notes: **Example 9 is written from scratch** against the AWS SDK — verify the request
    shape as you write. State the default index name (`"Causation"`) and that it is
    configurable. The page has no provisioning content today, so this is new material.
    Depends on 1.4.

- [x] **Task 3.11:** Add Glossary entries  [P1-15]
  - Input: `design.md` §9; `Glossary.md` `### Inbox` at **line 174** (design says ~178)
  - Output: `### Causation Id` and `### Replay (Inbox)` entries in `## Patterns`
    immediately after `### Inbox`, each following the existing `Term` / definition /
    `See:` shape
  - Notes: **The broken-link half of this task is already done.** Design §9 calls for
    retargeting the `### Inbox` entry away from the non-existent
    `/contents/InboxConfiguration.md`, but the working tree already points it at
    `/contents/BrighterInboxSupport.md#inbox-configuration` — fixed by an uncommitted
    link sweep on this branch, confirmed at tasks review 2026-08-02. Verify it is still
    correct and make no change. This task is now additive only.

- [x] **Task 3.12:** Note `IRequestContext.InstrumentationOptions` in `contents/V10MigrationGuide.md`  [P2-19]
  - Input: `../Brighter/release_notes.md:338`
  - Output: a short breaking-changes entry — source-breaking for direct `IRequestContext`
    implementors only, with Example 11 as the one-line fix
  - Notes: Lowest priority in the spec. If effort is constrained this is the one to drop;
    say so in the final QA note if dropped.

---

## Phase 4 — Polish & Review

> Goal: navigation, accuracy, and the QA checklist. Runs after Phases 2 and 3.
> Order matters: 4.1 before 4.4; 4.2 before 4.5.

- [ ] **Task 4.1:** Update `SUMMARY.md`
  - Input: `design.md` §SUMMARY.md Changes; current lines 81–82
  - Output: `* [Replay On Seen](/contents/ReplayOnSeen.md)` inserted between
    `Inbox Support` and `MSSQL Outbox` in the **Outbox and Inbox** section, unnested
  - Notes: Single line, no nesting — nesting in that section signals "one of N
    implementations", the wrong signal here.

- [ ] **Task 4.2:** Check `ReplayOnSeen.md` length and decide on the split
  - Input: the finished page; `design.md` §1 target (450–520 lines)
  - Output: either confirmation the page is within target, or the custom-store section
    [P2-18] extracted to its own page with `SUMMARY.md` and the Further Reading links
    updated
  - Notes: Split **only** if the page lands materially past 500 lines, and split the
    custom-store notes — never cut reader-facing material. That section is the clean seam
    because it targets a different audience. Depends on 3.8.

- [ ] **Task 4.3:** Verify every code example against the source
  - Input: all 12 examples; the Phase 1 `verification-notes.md`
  - Output: confirmation each example compiles against the V10 API; fixes for any drift
  - Notes: Concentrate on Examples **5** (the two-step cascade) and **9** (the DynamoDB
    `UpdateTable`) — the two written from scratch. Also re-check Example 3's
    `InboxConfiguration` parameter names and Example 4's `Context as RequestContext` form.
    This is the second pass; examples were verified at writing time.

- [ ] **Task 4.4:** Verify all internal links resolve
  - Input: all new and edited pages, `SUMMARY.md`
  - Output: `python3 tools/linkcheck.py` exits zero; fixes for anything it reports
  - Notes: Includes the back-links into `ReplayOnSeen.md` from all eight edited pages and
    `BrighterOutboxSupport.md#you-always-need-a-sweeper`. The repo was clean at tasks
    review (106 files, zero broken links), so **anything this check reports is yours**.
    `BrighterBasicConfiguration.md#inbox` was confirmed resolving at tasks review — the
    design's "known-unverified link" concern is settled; seven pages use it. Depends on 4.1.

- [ ] **Task 4.5:** Terminology and style pass
  - Input: all deliverables; `contents/Glossary.md`, `BasicConcepts.md`,
    `design.md` §Style Notes
  - Output: consistent **Causation Id** / `CausationId`, **Replay** / `OnceOnlyAction.Replay`,
    **Sweeper**, **Inbox**, **Outbox**, **Dispatcher**; second person, active voice,
    present tense throughout
  - Notes: No "ServiceActivator". No "idempotent" as a description of what Replay
    achieves. "Cascade" stays descriptive — it is not introduced as a formal term and does
    not go in the Glossary. Confirm the page title "Replay On Seen" has not leaked into
    the running prose as a pseudo-term. Depends on 4.2.

- [ ] **Task 4.6:** Final read-through against the QA checklist
  - Input: `CLAUDE.md` Quality Assurance Checklist; all deliverables
  - Output: the `README.md` Status Checklist ticked (Writing complete, Documentation
    reviewed) with a short closing note beneath it confirming the code, content,
    structure, and accuracy checks pass — and naming anything deliberately dropped
  - Notes: Update the existing Status Checklist rather than adding a new section. Mark the
    spec ready to close after this task.

---

## Dependency Summary

- **Phase 1** (1.1–1.5) gates all writing. The five tasks are mutually independent and
  all append to a shared `verification-notes.md`.
- **`ReplayOnSeen.md` is written strictly top-down**: 2.1 → 2.2 → 2.3 → 2.4 → 2.5 →
  3.1 → 3.2 → 3.3 → 3.4 → 3.5 → 3.6 → 3.7 → 3.8. Each task appends the next section.
- **Existing-page edits** (2.6–2.10, 3.9–3.12) are mutually independent and can run in
  parallel with the page-writing chain once Task 2.1 has created the link target.
- **Specific data dependencies:** 1.3 → 2.9, 2.10, 3.1. 1.4 → 3.1, 3.10.
  1.5 → 3.2, 3.6, 3.9. 1.2 → 2.2, 3.4, 3.8. 1.1 → 2.3.
- **Phase 4** runs last: 4.1 before 4.4 (SUMMARY link must exist before the check);
  4.2 before 4.5 (settle the file layout before the style pass); 4.6 closes.

---

**Next step:** Run `/spec:review` to approve these tasks, then `/spec:implement` to
start writing.
