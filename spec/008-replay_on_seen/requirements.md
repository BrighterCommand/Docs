# Requirements: Spec 008 — Replay On Seen

**Created:** 2026-08-01
**Status:** Approved 2026-08-01

## Topic Overview

Brighter's [Inbox](/contents/BrighterInboxSupport.md) de-duplicates incoming requests. Until now, when the Inbox saw a request it had already handled it could only *throw* (`OnceOnlyAction.Throw`) or *warn and skip* (`OnceOnlyAction.Warn`).
In both cases the messages the handler originally produced stay put — so there is no way to nudge a stalled workflow forward by replaying it from the beginning, with handlers that have already processed a message, re-sending the messages they in turn raised, thus triggering the next step. Eventually the flow reaches an unvisited handler, which runs as normal.

**Replay on seen** adds a third option, `OnceOnlyAction.Replay`. When the Inbox detects a duplicate, Brighter looks up the **Causation Id** recorded against that Inbox entry, finds the Outbox messages stamped with the same Causation Id, and
clears their dispatched state. The Outbox Sweeper then re-dispatches those *same* messages. The handler itself is **not** re-run — only its direct effects (the outgoing messages) are replayed.

This matters because the Inbox alone does not give you true idempotency. Skipping a duplicate protects the handler's side effects, but it also swallows the outgoing messages that downstream steps depend on. Replay closes that hole: re-send the
original command and the workflow picks up from wherever it stalled.

The feature is **opt-in**, and it has real prerequisites — a causation-tracking Inbox *and* Outbox, a migrated store schema, a running Sweeper, and handlers that thread their `RequestContext` through `Post`/`DepositPost`. Miss the last one and replay is a **silent no-op**. Documentation needs to make each prerequisite unmissable.

## Current State

**What exists today:**

| Page | Relevance | Gap |
| :--- | :--- | :--- |
| `contents/BrighterInboxSupport.md` (129 lines) | The home of `OnceOnly` / `OnceOnlyAction` | Documents only `Warn` and `Throw`. The behaviour table at lines 41–45 is now incomplete. No mention of Causation Id or Replay. |
| `contents/BrighterOutboxSupport.md` (510 lines) | Outbox and Sweeper behaviour | No mention that dispatched state can be cleared by an external trigger, or of the `CausationId` column. |
| `contents/PipelineValidation.md` (331 lines) | Startup validation rules and "Common Mistakes and Fixes" | The new `ReplayRequiresCausationTracking` rule is undocumented; its five distinct findings have no fix guidance. |
| `contents/BoxProvisioning.md` | Per-backend migration-version matrix (lines 93–96) | **Stale.** Matrix says Outbox `V1..V7` and Inbox `V1..V2` (PostgreSQL Inbox "V1 only"). The shipped code is Outbox **V8**, Inbox **V3**, PostgreSQL Inbox **V2** — all from the `CausationId` migration. Lines 101, 108, 116 carry the same stale claims. |
| `contents/BoxProvisioningUpgrade.md` | Upgrade narrative, uses V7 as "current" throughout (lines 15, 63, 79, 130) | Stale example versions; no mention that the `CausationId` migration is the one you must run before Replay works. |
| `contents/Glossary.md` (577 lines) | Defines Inbox, Outbox, Sweeper, Migration Chain, etc. | No **Causation Id** or **Replay** entry. |
| `contents/V10MigrationGuide.md` | Breaking changes | Does not carry the `IRequestContext.InstrumentationOptions` source-breaking change. |

**Net:** the feature is entirely undocumented, and two existing pages now state
migration-version facts that the shipped code contradicts.

## Target State

A reader should be able to:

1. Understand what a Causation Id is and how it differs from Correlation Id  
2. Decide whether Replay fits their problem (and recognise when it does not — it is not saga orchestration).
3. Turn it on: attribute or global `InboxConfiguration`, migrate the store, confirm the Sweeper is running, thread `Context` through their `Post` calls.
4. Look up whether their chosen Inbox/Outbox pair supports causation tracking.
5. Diagnose a replay that did nothing, using the startup validation findings, the log messages, and the trace events.
6. Find the migration version their store needs, and what happens if they upgrade Brighter without running it.

Note that Job Id / Workflow Id exist but are reserved for future use and don't need discussion in this documentation.

## Target Audience

- **Primary — intermediate.** Users already running an Inbox and an Outbox with a Sweeper, who hit a stalled multi-step workflow. They know the patterns; they need the mechanics and the prerequisites.
- **Secondary — advanced.** Users writing a custom Inbox/Outbox who need to implement `IAmACausationTrackingInbox` / `IAmACausationTrackingOutbox`, and   users debugging a silent no-op.
- **Not the audience — beginners.** Someone who has not yet met the Inbox or Outbox should be sent to those pages first. The new page links back rather than re-explaining them.

## Source Material

**Design and rationale:**

- `../Brighter/docs/adr/0057-replay-outbox-on-inbox-duplicate.md` (661 lines) —  the authoritative design. Covers Causation Id semantics, the role interfaces, the write-path gate, schema evolution, pipeline validation, observability, the   key-components table, consequences, and alternatives considered.
- `../Brighter/specs/0027-replay-matching-outbox-events-when-inbox-has-already-seen/`  — `requirements.md` (problem statement, acceptance criteria, out-of-scope), `tasks.md`, and the four `review-*.md` documents. 
- GitHub issue BrighterCommand/Brighter#2541.

**Release notes:**

- `../Brighter/release_notes.md:304–338` — the user-facing summary, the **"thread your `RequestContext` through `Post`/`DepositPost`"** requirement with  ❌/✅ code samples, and the `IRequestContext.InstrumentationOptions` source-breaking change.

**Source code:**

- `../Brighter/src/Paramore.Brighter/Inbox/OnceOnlyAction.cs` — the enum, with the `Replay` member's XML doc.
- `../Brighter/src/Paramore.Brighter/Inbox/IAmACausationTrackingInbox.cs`
- `../Brighter/src/Paramore.Brighter/IAmACausationTrackingOutbox.cs`
- `../Brighter/src/Paramore.Brighter/RequestContextBagNames.cs:143` — `CausationId = "Brighter-CausationId"`.
- `../Brighter/src/Paramore.Brighter/Inbox/Handlers/UseInboxHandler.cs` and the async variant — the Replay branch and the `CustomContextDisablesReplay` warning.
- `../Brighter/src/Paramore.Brighter/InMemoryInbox.cs`, `InMemoryOutbox.cs` — the reference implementations.
- `../Brighter/src/Paramore.Brighter/RelationalDatabaseInbox.cs`,
  `RelationDatabaseOutbox.cs`, `IRelationalDatabaseInboxCausationQueries.cs`, `IRelationalDatabaseOutboxCausationQueries.cs` — how the relational stores get causation support via their per-backend `*Queries` class.
- `../Brighter/src/Paramore.Brighter.Outbox.DynamoDB{,.V4}/DynamoDbOutbox.cs` and `DynamoDbConfiguration.cs` — the `Causation` GSI
  (`CausationIndexName`, default `"Causation"`) and the `DescribeTable` probe.
- `../Brighter/src/Paramore.Brighter.BoxProvisioning.*/[Ms|My]Sql*MigrationCatalog.cs`, `PostgreSql*`, `Sqlite*` — confirm **Outbox V8** (`s_v8AddedColumns =  ["CausationId"]` plus the new replay index) and **Inbox V3** (PostgreSQL Inbox
  **V2**).
- `../Brighter/src/Paramore.Brighter/Extensions/.../ServiceCollectionExtensions.cs`
  — the extra `IAmACausationTrackingOutbox` DI registration.

**Tests (use as verified examples):**

- `../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/When_a_seen_message_is_replayed_end_to_end.cs`
  — a full end-to-end scenario: command → handler → downstream event → duplicate
  command → replay. The best single reference for the "how it hangs together"
  walkthrough.
- `../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/TestDoubles/ProcessAndForwardHandler.cs:55,65`
  — the canonical handler shape:
  `[UseInbox(1, onceOnly: true, onceOnlyAction: OnceOnlyAction.Replay, contextKey: typeof(ProcessAndForwardHandler))]`
  and `_commandProcessor.Post(outgoing, Context as RequestContext);`.
- `../Brighter/tests/Paramore.Brighter.Base.Test` — the causation-tracking base
  tests, useful for confirming per-store behaviour claims.

**Existing Docs specs to align with:**

- `spec/003-pipeline-validation-at-startup` — the validation page this feature
  extends.
- `spec/005-database_migration` — the BoxProvisioning pages whose version matrix
  this feature invalidates.

## Scope

### P0 — Must have

1. **What Replay is and the problem it solves.** The stalled-workflow scenario: handler completed, downstream consumer never processed, re-sending the message currently achieves nothing. Contrast with `Throw` and `Warn`. Make the **cascade** explicit — you re-send the original message at the front of the flow, each already-seen handler skips its work but re-raises the messages it produced, and the flow walks forward step by step until it reaches the handler that never ran, which executes normally.
2. **Causation Id defined.** One handler invocation's outgoing messages share one Causation Id. Default value is the request's `Id`. Explicitly distinguish from `CorrelationId` (request-reply). Note it travels in `RequestContext.Bag` under `RequestContextBagNames.CausationId`.
3. **The end-to-end flow.** Duplicate detected → `GetCausationId` from the Inbox → `ReplayCausation` on the Outbox clears `DispatchedAt` → the Sweeper picks the messages up on its next run → handler never re-runs. Include the ADR's architecture diagram (or an equivalent) as a fenced text block.
4. **Enabling it.** Both routes, with working code:
   - Per handler:
     `[UseInbox(step: 1, contextKey: typeof(MyHandler), onceOnly: true, onceOnlyAction: OnceOnlyAction.Replay)]`
     and the `UseInboxAsync` equivalent.
   - Globally: `InboxConfiguration.ActionOnExists = OnceOnlyAction.Replay`, wired
     via `AddBrighter(...).UseExternalInbox(...)` — cross-link
     `BrighterBasicConfiguration.md#inbox`.
5. **The `RequestContext` threading requirement.** Its own clearly-signposted
   section with the ❌/✅ pair from the release notes. State plainly that
   `Post(evt)` without a context produces a `null` `CausationId` and a **silent
   no-op** at replay time; `Post(evt, Context)` is required. Applies to
   `PostAsync`, `DepositPost`, `DepositPostAsync`.
6. **Prerequisites checklist.** Inbox implements `IAmACausationTrackingInbox`;
   Outbox implements `IAmACausationTrackingOutbox`; store schema migrated so
   `SupportsCausationTracking()` returns `true`; handlers thread `Context`
   through their `Post`/`DepositPost` calls (see item 5).
7. **An Outbox Sweeper must be running.** Immediate send is not a replay path —
   replay works only by clearing dispatched state and letting the Sweeper resend.
   Register it with `.UseOutboxSweeper(...)` from
   `Paramore.Brighter.Outbox.Hosting` (which hosts `TimedOutboxSweeper`; the
   lower-level type is `OutboxSweeper`). This applies whichever Outbox you use,
   **including the implicit one**: if you configure no explicit Outbox, Brighter
   creates an `InMemoryOutbox` for you (`CreateDefaultOutbox` in
   `ServiceCollectionExtensions.cs`), and it does support Replay
   (`InMemoryOutbox` implements `IAmACausationTrackingOutbox`) — but without a
   Sweeper nothing re-sends. There is no in-memory-specific sweeper type.
8. **Replay must be configured on every already-seen step.** The cascade is the
   point of the feature, and it only propagates if each handler the flow passes
   back through is itself set to `OnceOnlyAction.Replay`. A downstream handler
   configured with `Throw` or `Warn` receives the replayed message, recognises
   the duplicate, and **stops the cascade there** — the flow never reaches the
   step that failed. State this explicitly with the consequence.
9. **Update the `OnceOnlyAction` table in `BrighterInboxSupport.md`.** Add the `Replay` row to the behaviour table and to the bullet list at lines 32–38, and  link out to the new page.
10. **Correct the stale BoxProvisioning version matrix.** `BoxProvisioning.md`
   lines 93–96 → Outbox `V1..V8`, Inbox `V1..V3`, PostgreSQL Inbox `V1..V2`;
   fix the supporting prose at lines 101, 108, 116 (the "PostgreSQL Inbox is
   V1-only" claim is no longer true) and the V7-as-current examples in
   `BoxProvisioningUpgrade.md` (lines 15, 63, 79, 130).

### P1 — Should have

11. **Store support matrix.** A table of Brighter-maintained stores × causation
   tracking, with the migration each needs:
   - Relational (MsSql, MySql, PostgreSql, Sqlite): supported; needs Outbox V8 /
     Inbox V3 (PostgreSQL Inbox V2) via BoxProvisioning.
   - Spanner: supported via its provisioner; live column probe.
   - DynamoDB / DynamoDB.V4: Inbox supported unconditionally; **Outbox requires
     the `Causation` GSI** (`DynamoDbConfiguration.CausationIndexName`, default
     `"Causation"`) — call this out, it is the one NoSQL store with a real
     prerequisite, and the only store where you must provision infrastructure
     yourself. See the dedicated deliverable below.
   - **How to create the DynamoDB `Causation` GSI.** BoxProvisioning does not
     cover DynamoDB, so this is the user's job and must be documented rather than
     assumed:
     - *New tables* get it for free — `MessageItem.CausationId` carries
       `[DynamoDBGlobalSecondaryIndexHashKey(indexName: "Causation")]`
       (`MessageItem.cs:49`), so
       `DynamoDbTableFactory.GenerateCreateTableRequest<MessageItem>()` emits the
       GSI along with the existing `Outstanding` / `Delivered` indexes.
     - *Existing tables* need an explicit `UpdateTable` with a
       `GlobalSecondaryIndexUpdates` create — show it, and note that AWS
       backfills a new GSI asynchronously, so `SupportsCausationTracking()` (a
       live `DescribeTable` probe) keeps reporting false until the index is
       `ACTIVE`.
     - State the consequence of skipping it: startup validation reports the
       Outbox as not supporting causation tracking, and Replay never fires.
   - Firestore, MongoDb: supported, schemaless, no migration.
   - In-memory: supported (development only).
12. **Startup validation.** Document the `ReplayRequiresCausationTracking` rule
    and all five findings — Inbox missing the interface (Error), Inbox
    `SupportsCausationTracking()` false (Warning), no Outbox configured (Warning,
    terminal step), Outbox missing the interface (Error), Outbox
    `SupportsCausationTracking()` false (Warning) — each with the fix. Add a
    "Common Mistakes and Fixes" entry in `PipelineValidation.md`.
13. **Upgrade-without-migrating behaviour.** The write-path gate: relational
    stores probe once per store instance for the `CausationId` column and fall
    back to the old INSERT when it is absent, so deposits keep working
    byte-for-byte. Consequence to state explicitly: **the memo is not
    invalidated**, so provisioning that runs after a store instance was
    constructed needs a process restart.
14. **Observability.** The four `UseInboxHandler` span events (`Add`,
    `Duplicate Throw`, `Duplicate Warn`, `Duplicate Replay`), the
    `paramore.brighter.causation_id` tag on the Replay event, and the
    `CustomContextDisablesReplay` warning log. Note that the Sweeper's re-dispatch
    is its own trace with no parent link back to the replay — say why (the sweep
    is asynchronous and may cover many replays).
15. **Glossary entries.** **Causation Id** and **Replay (Inbox)**, cross-linked to
    Inbox, Outbox, Sweeper, and Migration Chain.
16. **Gotchas / limitations.** Each stated with its consequence:
    - Historical rows have a `null` `CausationId` — no backfill, so entries
      written before the migration can never be replayed.
    - The default Causation Id is the request `Id`, so if the same message id is
      handled by several `[UseInbox]` handlers, a replay resends **all** their
      messages. Intended; note the escape hatch (set your own Causation Id in the
      Bag before the Inbox handler runs). Note also that `UseInbox` is normally
      applied to Commands (single handler), not Events.
    - Sweeper race: a message may be dispatched twice. Downstream de-duplication
      handles it.
    - A custom `IAmARequestContextFactory` returning a non-`RequestContext`
      `IRequestContext` degrades Replay to a no-op (with the one-time warning).
    - No Outbox (terminal step) → safe no-op, validation warns.

### P2 — Nice to have

17. **Worked end-to-end walkthrough** derived from
    `When_a_seen_message_is_replayed_end_to_end.cs`: command handled, downstream
    event deposited, same command re-posted, event re-sent.
18. **Implementing causation tracking in a custom store** — the two role
    interfaces, what `SupportsCausationTracking()` must honestly report, and why
    it is a runtime check rather than a static capability.
19. **`IRequestContext.InstrumentationOptions` source-breaking change** noted in
    `V10MigrationGuide.md` for anyone implementing `IRequestContext` directly.
20. **Design-rationale sidebar** — why not `CorrelationId`, and why not
    re-execute the handler (from the ADR's Alternatives Considered).

## Out of Scope

- **Saga / workflow orchestration.** Replay *does* cascade across steps — that is
  the point of the feature (see P0 items 1 and 8) — but it is not orchestration:
  there is no coordinator, no compensation, and no ordering guarantee. It is
  re-delivery of already-recorded messages, and each step advances only because
  its own Inbox is configured to replay. Draw that line once and move on; there
  is no orchestration page to link to yet.
- **Immediate-send replay.** Sweeper-only. Do not document a non-existent path.
- **Re-executing handler logic.** Explicitly not what this does.
- **Data backfill** for pre-migration rows.
- **Rewriting the Inbox or Outbox pages** beyond the additions listed above.
- **Per-backend BoxProvisioning mechanics** — owned by the existing
  `BoxProvisioning*.md` pages; this spec only corrects their version facts and
  links to them.
- **Darker.** Queries have no Inbox/Outbox; nothing to say.

## Documentation Deliverables

**New file:**

| File | Purpose |
| :--- | :--- |
| `contents/ReplayOnSeen.md` | The main page. Concept (Causation Id, the problem), **the cascade** — how replay walks a stalled flow forward step by step, and why every already-seen step needs `OnceOnlyAction.Replay` for it to propagate — how it works, enabling it, the `RequestContext` threading requirement, prerequisites, store support matrix, validation findings, observability, gotchas, further reading. Target 300–400 lines. |

**Updated files:**

| File | Change |
| :--- | :--- |
| `contents/BrighterInboxSupport.md` | Add `Replay` to the `OnceOnlyAction` bullet list and the behaviour table; a short paragraph pointing at `ReplayOnSeen.md`. |
| `contents/BrighterOutboxSupport.md` | Note in the Sweeper section that replay clears dispatched state so the Sweeper re-sends; link to `ReplayOnSeen.md`. |
| `contents/BrighterBasicConfiguration.md` | *(added at design review, 2026-08-02)* Line 884 presents `ActionOnExists` as a closed list of `Throw` and `Warn`; add `Replay` as the third option. |
| `contents/PipelineValidation.md` | Add the `ReplayRequiresCausationTracking` rule to "Handler Pipeline Checks" and a "Common Mistakes and Fixes" entry. |
| `contents/BoxProvisioning.md` | Correct the per-backend version matrix (Outbox V8, Inbox V3, PostgreSQL Inbox V2) and the prose that depends on it; mention the new `CausationId` replay index on the Outbox. |
| `contents/BoxProvisioningUpgrade.md` | Refresh the V7-as-current examples; add a line on the `CausationId` migration being the prerequisite for Replay. |
| `contents/DynamoOutbox.md` | Add a section on the `Causation` GSI — how a new table gets it from `DynamoDbTableFactory`, and the `UpdateTable` call that adds it to an existing table. The page has no table-provisioning content today, so this is new material, not an amendment. Note the async backfill and link to `ReplayOnSeen.md`. |
| `contents/Glossary.md` | Add **Causation Id** and **Replay (Inbox)**. |
| `contents/V10MigrationGuide.md` | Note `IRequestContext.InstrumentationOptions` (P2). |
| `SUMMARY.md` | Add the new page. |

## SUMMARY.md Changes

Place the new page in the **Outbox and Inbox** section, immediately after
`Inbox Support` — it is Inbox-triggered behaviour and reads best straight after
the Inbox page, before the long run of per-store pages:

```markdown
 * [Inbox Support](/contents/BrighterInboxSupport.md)
 * [Replay On Seen](/contents/ReplayOnSeen.md)
 * [MSSQL Outbox](/contents/MSSQLOutbox.md)
```

Rejected alternative: nesting it under `Inbox Support`. Nothing else in that
section nests except the Distributed Lock provider pages, where nesting signals
"one of N implementations" — the wrong signal here.

## Constraints

**Terminology** (follow `contents/Glossary.md` and `BasicConcepts.md`):

- **Causation Id** — capitalised as a defined term on first use, then `CausationId`
  when naming the column/property.
- **Replay** — the `OnceOnlyAction`. Write `OnceOnlyAction.Replay` when naming the
  enum member.
- **Dispatcher**, not "ServiceActivator".
- **Sweeper**, **Outbox**, **Inbox** — as already defined in the Glossary.
- Do not call this "idempotency". The page should say plainly that the Inbox does
  *not* make you idempotent, which is the reason Replay exists.

**Style:**

- Second person, active voice, present tense (per `CLAUDE.md`).
- V10 patterns only. Configuration enters through `AddBrighter()`.
- Every C# example verified against Brighter source or the tests listed above —
  the previous spec (007) found two example defects at QA, so verify names and
  registration shapes as they are written, not afterwards.
- Prefer the real shapes from `ProcessAndForwardHandler.cs` over invented ones.

**Cross-linking:**

- `ReplayOnSeen.md` links to: `BrighterInboxSupport.md`,
  `BrighterOutboxSupport.md`, `OutboxPattern.md`, `BoxProvisioning.md`,
  `BoxProvisioningUpgrade.md`, `PipelineValidation.md`, `Telemetry.md`,
  `Glossary.md`, and the relevant per-store pages (`DynamoOutbox.md` for the GSI).
- Each updated page links *back* to `ReplayOnSeen.md`.
- Reference the ADR for design rationale rather than restating it at length:
  `Brighter/docs/adr/0057-replay-outbox-on-inbox-duplicate.md`.

**Accuracy constraints:**

- Migration version claims must be re-verified against the `*MigrationCatalog.cs`
  sources at writing time, not taken from this document.
- **Column casing differs by backend.** PostgreSQL emits lowercase `causationid`
  (deliberately — ADR 0057 §1, and its migration description reads
  `"Add causationid column"`); MsSql, MySql, and Sqlite use `CausationId`. Never
  assert a single casing across all stores; where the column is named in prose,
  either use the backend's own spelling or name it generically ("the causation
  column").
- The store support matrix must be checked store by store — the DynamoDB Outbox
  GSI requirement is the one asymmetry and is easy to flatten by mistake.

## Open Questions

1. Should the P2 walkthrough be inline in `ReplayOnSeen.md` or a separate page?
   Recommendation: inline, kept short — a separate page risks duplicating the
   concept material.
2. Is there a sample in `../Brighter/samples/` that uses Replay? None found in
   this pass; if one lands, link it from "Sample Code".

---

**Next step:** run `/spec:review` when you are ready to approve these requirements.
