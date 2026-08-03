# Design: Spec 008 — Replay On Seen

**Created:** 2026-08-01
**Status:** Approved 2026-08-02
**Traces to:** `requirements.md` (approved 2026-08-01)

Scope items are referenced as **[P0-n]**, **[P1-n]**, **[P2-n]** throughout, matching
the numbered list in `requirements.md`.

## Documentation Structure

One new page carries the feature. Seven existing pages gain a hook into it, so a
reader arriving from any direction — the Inbox, the Outbox, a validation error, a
DynamoDB table, the Glossary — finds their way in.

```
contents/
├── ReplayOnSeen.md                 ← NEW. The feature page.
│
├── BrighterInboxSupport.md         ← Replay row in the OnceOnlyAction table  [P0-9]
├── BrighterOutboxSupport.md        ← note in "You always need a Sweeper"     [P0-7]
├── BrighterBasicConfiguration.md   ← Replay in the ActionOnExists bullet     (added at review)
├── PipelineValidation.md           ← ReplayRequiresCausationTracking rule    [P1-12]
├── DynamoOutbox.md                 ← the Causation GSI section               [P1-11]
├── BoxProvisioning.md              ← corrected version matrix                [P0-10]
├── BoxProvisioningUpgrade.md       ← refreshed V-current examples            [P0-10]
├── Glossary.md                     ← Causation Id, Replay (Inbox)            [P1-15]
└── V10MigrationGuide.md            ← IRequestContext.InstrumentationOptions  [P2-19]
```

### Reading order

The page assumes the reader knows what an Inbox and an Outbox are. It does not
re-teach them.

```
BrighterInboxSupport.md  ─┐
BrighterOutboxSupport.md ─┼─►  ReplayOnSeen.md  ─┬─►  BoxProvisioning.md      (migrate the schema)
PipelineValidation.md    ─┘                      ├─►  DynamoOutbox.md         (create the GSI)
                                                 ├─►  PipelineValidation.md   (read the findings)
                                                 └─►  Telemetry.md            (trace the replay)
```

Within `ReplayOnSeen.md` the order is deliberately **problem → concept → cascade →
turn it on → prove it works → what breaks it**. The cascade comes before
configuration because a reader who has not grasped that every step needs its own
`Replay` will configure one handler and conclude the feature is broken.

## File-by-File Outline

---

### 1. `contents/ReplayOnSeen.md` — NEW

**Purpose:** Explain what replay on seen does, when to reach for it, how to turn
it on, and every way it can silently fail to fire.

**Target length:** 450–520 lines. Deliberately over the 400 first floated at
design review: all P2 content is **kept** (the walkthrough, the custom-store
notes, and the design-rationale section), and completeness is worth the length
here. `CLAUDE.md` suggests considering a split past ~500 lines — if the page
lands materially above that, split the **custom-store implementation notes**
[P2-18] into their own page rather than cutting reader-facing material. That is
the only section aimed at a different audience (people writing a store, not
people using one), so it is the clean seam.

**Type:** Conceptual + how-to hybrid (per `CLAUDE.md` documentation types).

#### Section outline

```
# Replay On Seen                                                    (H1)

  Intro — 3 sentences. A duplicate normally means "skip". Replay makes it
  mean "resend what this step already produced". Links: Inbox, Outbox.

## The Problem: A Workflow That Stalls Halfway                      (H2)   [P0-1]
  - Concrete scenario: Order → Payment → Shipping. Payment's handler ran and
    deposited ShipOrder, but Shipping never processed it.
  - Re-send OrderPlaced today: the Inbox throws or warns. Nothing moves.
  - Why the Inbox alone is not idempotency: it protects the handler's side
    effects but swallows the messages downstream steps are waiting for.

## How Replay Walks the Flow Forward                                (H2)   [P0-1]
  - The cascade, stated plainly: re-send the message at the front; every
    handler that already ran skips its work but re-raises what it produced;
    the flow advances a step at a time until it reaches the handler that
    never ran, which executes normally.
  - Explicit: the handler is NOT re-executed. The *same* stored messages are
    re-dispatched — not newly generated ones. (Why that matters: state may
    have moved on; regenerating could produce different messages.)
  - Callout box: **every already-seen step needs `OnceOnlyAction.Replay`.** A
    step still set to Throw or Warn stops the cascade dead and the flow never
    reaches the failure.                                                   [P0-8]

## Causation Id                                                     (H2)   [P0-2]
  - Definition: one handler invocation's outgoing messages share one Causation
    Id. Default value is the request's `Id`.
  - How it travels: `RequestContext.Bag`, key `RequestContextBagNames.CausationId`
    (`"Brighter-CausationId"`). The Inbox stamps it; the Outbox `Add` reads it.
  - Distinguish from **Correlation Id** (request-reply). One sentence, no more.
  - Stored in both boxes — that is what lets a duplicate find its own messages.

### What Happens on a Duplicate                                     (H3)   [P0-3]
  - Flow diagram (fenced text, adapted from ADR 0057).
  - Numbered walkthrough matching the diagram: Exists() → true; action is
    Replay; GetCausationId from the Inbox; ReplayCausation on the Outbox
    clears DispatchedAt; handler returns without running; the Sweeper
    re-dispatches on its next pass.
  - State the latency consequence: replay is not immediate — it happens on the
    Sweeper's next interval.

## Turning It On                                                    (H2)   [P0-4]

### On a Handler                                                    (H3)
  - `[UseInbox(...onceOnlyAction: OnceOnlyAction.Replay)]` — Example 1.
  - Async variant — Example 2. Link to pipelines-must-be-homogeneous.

### Globally                                                        (H3)
  - `AddConsumers(options => options.InboxConfiguration = new InboxConfiguration(
    ..., actionOnExists: OnceOnlyAction.Replay))` — Example 3.
  - Note `ActionOnExists` is constructor-set, not a settable property.
  - Cross-link BrighterBasicConfiguration.md#inbox.

## You Must Thread Your RequestContext                              (H2)   [P0-5]
  - Its own H2, immediately after configuration — this is the failure everyone
    hits. Warning callout.
  - Why: the causation id lives in the pipeline's RequestContext.Bag. Post
    without a context and the CommandProcessor makes a fresh one; the id is
    lost; the message stores a null CausationId; replay later matches nothing.
  - ❌ / ✅ pair — Example 4.
  - Applies to Post, PostAsync, DepositPost, DepositPostAsync.
  - Symptom to recognise: replay runs, logs nothing wrong, resends nothing.

## Before You Enable It                                             (H2)   [P0-6, P0-7]
  - Checklist table: requirement | how to satisfy | what happens if you don't.
    - Inbox implements IAmACausationTrackingInbox
    - Outbox implements IAmACausationTrackingOutbox
    - store schema migrated (SupportsCausationTracking() true)
    - an Outbox Sweeper is running
    - handlers thread Context
    - every already-seen step set to Replay
  - Sweeper sub-point: replay only ever happens through the Sweeper — there is
    no immediate-send path. Link to "You always need a Sweeper" rather than
    restating it; that section already covers the in-memory Outbox and the
    RabbitMQ/Kafka async-confirmation case.

## Store Support                                                    (H2)   [P1-11]
  - Matrix table: store | Inbox | Outbox | what you must do first.
  - Relational: Outbox V8 / Inbox V3 (PostgreSQL Inbox V2) via BoxProvisioning.
  - Spanner: via its provisioner; live column probe.
  - MongoDb, Firestore: schemaless, nothing to do.
  - DynamoDB: Inbox nothing to do; **Outbox needs the Causation GSI** → link to
    DynamoOutbox.md.
  - In-memory: works, development only.
  - Note: the column is `causationid` on PostgreSQL, `CausationId` elsewhere.

## When Replay Does Not Fire                                        (H2)   [P1-12, P1-16]
  - Startup findings table: finding | severity | cause | fix. Five rows from
    the ReplayRequiresCausationTracking rule.
  - Then the runtime silences (no startup error, nothing resent):
    - handler did not thread Context → null CausationId
    - entry predates the migration → null CausationId, no backfill
    - a downstream step is set to Throw/Warn → cascade stops there
    - custom IAmARequestContextFactory returning a non-RequestContext →
      one-time CustomContextDisablesReplay warning
    - no Outbox at all (terminal step) → safe no-op, validation warns

## Upgrading Without Migrating                                      (H2)   [P1-13]
  - The write-path gate: relational stores probe once per store instance for
    the causation column and fall back to the old INSERT when absent, so
    deposits are unchanged on an un-migrated schema.
  - Consequence: the probe result is memoized and never invalidated —
    provisioning that runs after a store instance was built needs a restart.
  - Link BoxProvisioningUpgrade.md.

## Observability                                                    (H2)   [P1-14]
  - Span events table: path | event name | tags.
  - The paramore.brighter.causation_id tag on the Replay event.
  - CustomContextDisablesReplay warning log.
  - Why the re-dispatch is a separate trace with no parent link (the sweep is
    asynchronous and may carry many replays).
  - Link Telemetry.md.

## Limitations                                                      (H2)   [P1-16]
  - Not saga orchestration: no coordinator, no compensation, no ordering.
  - Shared Causation Id when one message id reaches several [UseInbox]
    handlers — a replay resends all of them. Intended; note UseInbox is
    normally on Commands (one handler). Escape hatch: set your own Causation
    Id in the Bag before the Inbox handler runs.
  - Sweeper race: a message may go twice. Downstream de-duplication handles it.
  - Historical rows have no Causation Id and can never be replayed.

## A Worked Example                                                 (H2)   [P2-17]
  - The end-to-end scenario from the test: command handled and a downstream
    event deposited; the same command posted again; the event re-sent without
    the handler running. Narrate it as numbered steps against Example 5's
    handlers, noting what each store holds at each point.
  - Cite the test as the reference implementation (there is no sample app).

## Why It Works This Way                                            (H2)   [P2-20]
  - Short rationale section, three paragraphs, drawn from ADR 0057's
    Alternatives Considered:
    - Why not re-execute the handler? That defeats the Inbox — the whole point
      is that re-running is unsafe — and state may have moved on, so it could
      produce *different* messages. Replay resends the originals.
    - Why not reuse `CorrelationId`? It means request-reply; overloading it
      would conflate two unrelated relationships.
    - Why the Sweeper rather than an immediate send? The dispatch machinery
      already exists; replay only has to mark messages undispatched.

## Implementing Causation Tracking in Your Own Store                (H2)   [P2-18]
  - For readers writing a custom Inbox or Outbox, not for users.
  - The two role interfaces and what each method must do.
  - `SupportsCausationTracking()` must report the **live** state honestly —
    it is a runtime probe, not a static capability, because pipeline
    validation trusts it to decide whether Replay can work.
  - Skeleton — Example 12.

## Further Reading                                                  (H2)
  - Inbox Support, Outbox Support, Outbox Pattern, Box Provisioning,
    Upgrading Existing Deployments, Pipeline Validation, DynamoDb Outbox,
    Telemetry, Glossary.
  - ADR 0057 for the full design record.
```

**Glossary terms used:** Inbox, Outbox, Sweeper, Migration Chain (all existing);
**Causation Id**, **Replay** (both new — defined here, summarised in the Glossary).

**Cross-links out:** `BrighterInboxSupport.md`, `BrighterOutboxSupport.md`
(`#you-always-need-a-sweeper`), `OutboxPattern.md`, `BrighterBasicConfiguration.md#inbox`,
`BoxProvisioning.md`, `BoxProvisioningUpgrade.md`, `PipelineValidation.md`,
`DynamoOutbox.md`, `Telemetry.md`, `Glossary.md`,
`BuildingAnAsyncPipeline.md` (Russian Doll), `DispatchingARequest.md#pipelines-must-be-homogeneous`.

**All P2 content is included** — the walkthrough [P2-17], the custom-store notes
[P2-18], and the rationale section [P2-20]. Nothing here is conditional on length.

---

### 2. `contents/BrighterInboxSupport.md` — UPDATE  [P0-9]

**Purpose of change:** The `OnceOnlyAction` list and behaviour table are the
canonical reference for what a duplicate does. They currently know only two of
three actions.

| Location | Change |
| :--- | :--- |
| Line 35–38 (the `OnceOnlyAction` bullet) | Add a third sub-bullet: `Replay` — resend the messages this handler produced the first time, without re-running it. Link `ReplayOnSeen.md`. |
| Lines 41–45 (behaviour table) | Add one row: `true` / `Replay` → "If a duplicate is found, replays the Outbox messages produced during the original handling and **stops** processing. Otherwise, processes it." |
| After the table (~line 47) | Two-sentence paragraph pointing at `ReplayOnSeen.md`, naming the prerequisites in one breath (causation-tracking stores, migrated schema, a Sweeper) so nobody enables it from this page alone. |

**Do not** expand this page further — the Inbox page stays about the Inbox.

---

### 3. `contents/BrighterOutboxSupport.md` — UPDATE  [P0-7]

**Purpose of change:** A reader on the Outbox page should learn that something
outside the Outbox can reset dispatch state.

| Location | Change |
| :--- | :--- |
| `### You always need a Sweeper` (line 182) | Append a short paragraph: replay clears `DispatchedAt` so the Sweeper resends — it is the *only* replay path, there is no immediate-send equivalent. Link `ReplayOnSeen.md`. |
| `### Consumer: Using Inbox for Deduplication` (line 432) | One sentence at the end: if a duplicate should push a stalled flow forward rather than be dropped, see `ReplayOnSeen.md`. |

That section already explains the in-memory Outbox's Sweeper dependency and the
RabbitMQ/Kafka asynchronous-confirmation case, so the new page **links** here
rather than repeating it.

---

### 4. `contents/BrighterBasicConfiguration.md` — UPDATE  *(added at design review, approved 2026-08-02)*

Found while designing the global-configuration example: line 884 enumerates
`ActionOnExists` as "`OnceOnlyAction.Throw` … the alternative is
`OnceOnlyAction.Warn`" — a closed list of two, now wrong. The change is one
sentence.

| Location | Change |
| :--- | :--- |
| Line 884 (`ActionOnExists` bullet) | Add `OnceOnlyAction.Replay` as the third option, one clause, linking `ReplayOnSeen.md`. |

---

### 5. `contents/PipelineValidation.md` — UPDATE  [P1-12]

| Location | Change |
| :--- | :--- |
| `### Handler Pipeline Checks (AddBrighter)` rule table (~line 38) | Add one row: **Replay requires causation tracking** / Error + Warning / "When a pipeline is configured with `OnceOnlyAction.Replay`, checks that the Inbox and Outbox both implement causation tracking and that their schemas support it." |
| "Example error messages" block (~line 44) | Add two of the five messages verbatim — one Error (Outbox missing the interface), one Warning (schema not migrated). |
| End of `## Common Mistakes and Fixes` — **after the `### Missing Handler for Subscription` section ends, immediately before `## Further Reading` (line 323)** | New `### Replay Without Causation Tracking` following the established **Before** (warning) / **After** (fixed) shape — Example 7. |

The five findings enumerated in full belong on `ReplayOnSeen.md`; this page keeps
its existing summary-table density.

---

### 6. `contents/DynamoOutbox.md` — UPDATE  [P1-11]

**Purpose of change:** This is the one store where Replay needs infrastructure
Brighter does not provision. The page has **no** table-provisioning content
today, so this is new material rather than an amendment.

| Location | Change |
| :--- | :--- |
| New `## Replay Support: The Causation Index` after the handler example (~line 89) | New table → the GSI comes for free from `DynamoDbTableFactory` (Example 8). Existing table → an explicit `UpdateTable` with `GlobalSecondaryIndexUpdates` (Example 9). Note AWS backfills a GSI asynchronously, so `SupportsCausationTracking()` — a live `DescribeTable` probe — keeps reporting false until the index is `ACTIVE`; a restart immediately after adding it will still fail validation. Consequence of skipping: validation reports the Outbox unsupported and Replay never fires. Link `ReplayOnSeen.md`. |

Index name comes from `DynamoDbConfiguration.CausationIndexName`, default
`"Causation"` — state the default and that it is configurable.

---

### 7. `contents/BoxProvisioning.md` — UPDATE  [P0-10]

**These are factual corrections, not additions.** The page currently states
versions the shipped catalogs contradict.

| Location | Current | Correct to |
| :--- | :--- | :--- |
| Line 93 (MSSQL row) | Outbox `V1..V7`, Inbox `V1..V2` | Outbox `V1..V8`, Inbox `V1..V3` |
| Line 94 (PostgreSQL row) | Outbox `V1..V7`, Inbox `V1 only` | Outbox `V1..V8`, Inbox `V1..V2` |
| Line 95 (MySQL row) | Outbox `V1..V7`, Inbox `V1..V2` | Outbox `V1..V8`, Inbox `V1..V3` |
| Line 96 (SQLite row) | Outbox `V1..V7`, Inbox `V1..V2` | Outbox `V1..V8`, Inbox `V1..V3` |
| Line 101 (version prose) | "…V7 added `DataRef` and `SpecVersion`" | Append: "V8 added `CausationId` and the replay index." |
| Line 108 (asymmetry table) | "PostgreSQL — Inbox is V1-only" | Now V1..V2; rewrite as "the PostgreSQL Inbox chain is one version shorter than the others" |
| Line 116 (the V1-only explanation) | "…no V2 exists. The chain is intentionally shorter." | Keep the historical explanation for why PostgreSQL skipped the `ContextKey` migration, but correct the conclusion: it now has a V2 — the `causationid` migration — so the chain is shorter by one, not absent. |

Also add: the Outbox V8 migration is the first to create an **index** rather than
only columns.

**Verify every number against the `*MigrationCatalog.cs` sources when writing** —
per the requirements' accuracy constraint. Values confirmed at design time:
MsSql/MySql/PostgreSql/Sqlite Outbox `Version: 8`; MsSql/MySql/Sqlite Inbox
`Version: 3`; PostgreSql Inbox `Version: 2, Description: "Add causationid column"`.

---

### 8. `contents/BoxProvisioningUpgrade.md` — UPDATE  [P0-10]

| Location | Change |
| :--- | :--- |
| Line 15 | "originally created at V4 and the current Brighter ships V7 … applies V5, V6, V7" → ships **V8**, applies V5–V8 |
| Line 63 | "originally at V4 against a Brighter release that ships V7" → **V8** |
| Line 79 | "New columns added by V5–V7" → **V5–V8** |
| Line 130 | "If V5, V6, and V7 are pending and V7 fails" → **V5–V8 pending and V8 fails** |
| `## What to verify after upgrade` (~line 53) | New bullet: if you intend to use Replay, verify the causation column landed — until it does, `SupportsCausationTracking()` returns false and startup validation warns. Link `ReplayOnSeen.md`. |

The sample log output at line 63 must be regenerated consistently, not
hand-patched — check every version number in that block.

---

### 9. `contents/Glossary.md` — UPDATE  [P1-15]

Two entries, following the existing `### Term` / definition / `See:` shape.

| Term | Placement | Definition sketch |
| :--- | :--- | :--- |
| **Causation Id** | `## Patterns`, after `### Inbox` (line ~179) | The identifier linking an Inbox entry to the Outbox messages produced while handling it. One handler invocation's outgoing messages share one Causation Id; it defaults to the request's id. Distinct from Correlation Id, which links a request to its reply. `See: [Replay On Seen](/contents/ReplayOnSeen.md)` |
| **Replay (Inbox)** | Immediately after Causation Id | The `OnceOnlyAction` that makes duplicate detection re-dispatch the Outbox messages produced during the original handling, instead of throwing or warning. The handler does not re-run. `See: [Replay On Seen](/contents/ReplayOnSeen.md)` |

**Also fix while in this file (approved 2026-08-02):** `Glossary.md:178` — the
existing `### Inbox` entry links to `/contents/InboxConfiguration.md`, **which
does not exist and never has** (no history for that path in this repo, so the
link was wrong when written). Retarget to
`/contents/BrighterInboxSupport.md#inbox-configuration` — that heading exists
(`BrighterInboxSupport.md:67`), it preserves the "Inbox Configuration" link text,
and it matches the sibling `### Outbox` entry's pattern of deep-linking into the
support page. Unrelated to this feature; fixed opportunistically.

---

### 10. `contents/V10MigrationGuide.md` — UPDATE  [P2-19]

| Location | Change |
| :--- | :--- |
| Breaking-changes section | Short entry: `IRequestContext` gains a required `InstrumentationOptions` member. Source-breaking for anyone implementing the interface directly; the shipped `RequestContext` already has it, so normal call sites are unaffected. One-line fix snippet — Example 11. |

Lowest priority in the spec. Drop if effort is constrained; it affects only
custom `IRequestContext` implementors.

## SUMMARY.md Changes

**Before** (lines 81–82):

```markdown
 * [Inbox Support](/contents/BrighterInboxSupport.md)
 * [MSSQL Outbox](/contents/MSSQLOutbox.md)
```

**After:**

```markdown
 * [Inbox Support](/contents/BrighterInboxSupport.md)
 * [Replay On Seen](/contents/ReplayOnSeen.md)
 * [MSSQL Outbox](/contents/MSSQLOutbox.md)
```

Single insertion, one line, unnested — consistent with every other page in the
**Outbox and Inbox** section except the Distributed Lock provider group, where
nesting signals "one of N implementations".

## Code Examples Plan

| # | Example | Source | Complete or abbreviated | Goes in |
| :-- | :--- | :--- | :--- | :--- |
| 1 | Handler with `[UseInbox(..., onceOnlyAction: OnceOnlyAction.Replay, contextKey: ...)]` | `Core.Tests/OnceOnly/TestDoubles/ProcessAndForwardHandler.cs:55` | Complete (handler + attribute) | ReplayOnSeen |
| 2 | `[UseInboxAsync(...)]` variant | Adapted from Example 1 + `BrighterInboxSupport.md:58` | Complete | ReplayOnSeen |
| 3 | Global config: `AddConsumers(options => options.InboxConfiguration = new InboxConfiguration(inbox: …, actionOnExists: OnceOnlyAction.Replay))` | Shape from `BrighterBasicConfiguration.md:905`; params verified against `InboxConfiguration.cs:70` | Abbreviated (`// ...` around it) | ReplayOnSeen |
| 4 | ❌ `Post(evt)` vs ✅ `Post(evt, Context)` | `release_notes.md:315–330`, real form at `ProcessAndForwardHandler.cs:65` (`Context as RequestContext`) | Complete pair | ReplayOnSeen |
| 5 | Two-step cascade: `ProcessPayment` (already seen, Replay) → `ShipOrder` (never ran) | Written from scratch, modelled on `ProcessAndForwardHandler` | Complete, both handlers | ReplayOnSeen |
| 6 | Sweeper registration `.UseOutboxSweeper(...)` | `BrighterOutboxSupport.md:253` | Abbreviated | ReplayOnSeen |
| 7 | Validation Before/After — Replay configured against a non-tracking Outbox | Messages from ADR 0057 validation rule | Complete pair | PipelineValidation |
| 8 | New DynamoDB table incl. the Causation GSI via `DynamoDbTableFactory.GenerateCreateTableRequest<MessageItem>(...)` | `DynamoDbTableFactory.cs:60`; GSI attribute at `MessageItem.cs:49` | Abbreviated | DynamoOutbox |
| 9 | `UpdateTable` adding the Causation GSI to an existing table | Written from scratch against the AWS SDK; index name from `DynamoDbConfiguration.cs:71` | Complete | DynamoOutbox |
| 10 | Flow diagram, duplicate → replay → sweep | ADR 0057 "Architecture Overview", trimmed | Fenced text, not code | ReplayOnSeen |
| 11 | `public InstrumentationOptions InstrumentationOptions { get; set; } = InstrumentationOptions.All;` | `release_notes.md:338` | Snippet | V10MigrationGuide |
| 12 | *(P2)* Skeleton `IAmACausationTrackingOutbox` implementation | `IAmACausationTrackingOutbox.cs` | Abbreviated | ReplayOnSeen |

**Verification rule:** examples 5 and 9 are the only ones written from scratch and
so carry the most risk. Both must be checked against the real APIs as they are
written — spec 007 shipped two example defects that a QA pass caught late.

Examples 1, 2, 4 use `MyCommand`/`MyEvent` in the test source; rename to the
Order/Payment/Shipping domain used throughout this page, per the "realistic
examples" standard.

## Style Notes

**Terminology decisions:**

- **Causation Id** — spaced and capitalised as the concept; `CausationId` when
  naming the property or a non-PostgreSQL column. In cross-store prose use "the
  causation column" — PostgreSQL's is lowercase `causationid` by design.
- **Replay** as the action; `OnceOnlyAction.Replay` when naming the enum member.
  The page title is "Replay On Seen" but the running prose says "replay" — avoid
  turning the page title into a pseudo-term.
- **Cascade** — used descriptively for the step-by-step advance. Not introduced as
  a formal term and not added to the Glossary; there is no matching type or API.
- Avoid "idempotent" as a description of what this achieves. The page's argument
  is that the Inbox is *not* idempotency, which is precisely why Replay exists.
- **Sweeper**, **Inbox**, **Outbox**, **Dispatcher** per the Glossary.

**Deviations from standard patterns:**

1. **"You Must Thread Your RequestContext" gets its own H2** rather than living
   under Common Pitfalls. It is a *prerequisite*, not a pitfall — get it wrong and
   the feature does nothing, with no error anywhere. It sits immediately after
   configuration so it cannot be skipped.
2. **No "Sample Code" section.** No sample in `Brighter/samples/` uses Replay
   (open question 2 in the requirements). Rather than an empty heading, the
   worked example cites the end-to-end test as the reference implementation.
3. **Store support is a matrix, not per-store pages.** Only DynamoDB needs
   store-specific instructions, and those go on the existing `DynamoOutbox.md`
   rather than a new page.
4. **`BoxProvisioning.md` changes are corrections to another spec's pages**
   (spec 005). Called out so the reviewer can split them off if preferred; the
   recommendation is to keep them here, since they are wrong today and this is the
   change that makes them wrong.

## Resolved at Design Review (2026-08-02)

1. **`BrighterBasicConfiguration.md` is in scope** — added as section 4. Its
   `ActionOnExists` bullet presents a closed list of two actions and is now wrong.
2. **The broken Glossary link is fixed here** — retargeted to
   `/contents/BrighterInboxSupport.md#inbox-configuration` (section 9). The old
   target never existed in this repo.
3. **All P2 content is kept and the page may exceed 400 lines** — target raised to
   450–520. If it lands materially past 500, split out the custom-store notes
   [P2-18] rather than cutting anything a *user* reads.
4. **The design-rationale material is a real section again** — "Why It Works This
   Way" [P2-20], rather than being dissolved into other sections as the first
   draft had it.
5. **`PipelineValidation.md` insertion point made precise** — end of Common
   Mistakes, before `## Further Reading` (line 323), not after line 302.

**Known-unverified link:** `BrighterBasicConfiguration.md#inbox` targets a heading
written as `#### **Inbox**` (bold inside the heading), so the generated anchor may
not be literally `#inbox`. Three existing pages already use this exact link
(`BrighterInboxSupport.md:69`, `DynamoInbox.md:4`, `MongoDBInbox.md:4`); follow the
established convention rather than inventing a variant. If it is broken it is
broken repo-wide and is a separate fix.

---

**Next step:** run `/spec:tasks` to build the writing task list.
