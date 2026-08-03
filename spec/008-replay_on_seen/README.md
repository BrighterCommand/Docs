# Spec 008: Replay On Seen

**Created:** 2026-08-01
**Status:** Writing and review complete (2026-08-03) — ready to close

## Topic Overview

Brighter's Inbox provides duplicate detection via the `UseInbox`/`OnceOnly`
attribute. Historically, when the Inbox saw a message it had already processed it
either threw (`OnceOnlyAction.Throw`) or warned and skipped (`OnceOnlyAction.Warn`) —
in neither case were the downstream messages originally produced by that handler
resent. That leaves no way to re-trigger a partially-completed workflow: the handler
is correctly skipped, but the consumers downstream of it never hear anything.

**Replay on seen** closes that gap. A **Causation Id** links an incoming Inbox entry
to the outgoing Outbox messages produced while handling it. When the Inbox detects a
duplicate and replay is enabled, Brighter clears `DispatchedAt` on the Outbox messages
sharing that Causation Id, so the Outbox Sweeper re-dispatches the *same* outgoing
messages. The handler is not re-executed; only its direct effects are replayed.

This documentation needs to explain what Causation Id is (and how it differs from
Correlation Id), how to enable replay on duplicate detection, the prerequisites
(an Outbox, a Sweeper, and store support for `CausationId`), the schema evolution
delivered through BoxProvisioning, the startup pipeline validation that guards the
configuration, and the observability and gotchas around replay.

**Primary source material:**

- Brighter spec: `../Brighter/specs/0027-replay-matching-outbox-events-when-inbox-has-already-seen/`
  (`README.md`, `requirements.md`, `tasks.md`, and the review documents)
- ADR: `../Brighter/docs/adr/0057-replay-outbox-on-inbox-duplicate.md`
- Tests: `../Brighter/tests/Paramore.Brighter.Core.Tests/OnceOnly/When_a_seen_message_is_replayed_end_to_end.cs`
- Linked issue: BrighterCommand/Brighter#2541
- Release notes: `../Brighter/release_notes.md`

## Status Checklist

- [x] Requirements gathered (`requirements.md`)
- [x] Requirements reviewed and approved (2026-08-01)
- [x] Documentation outline created (`design.md`)
- [x] Outline reviewed and approved (2026-08-02)
- [x] Writing tasks identified (`tasks.md`)
- [x] Tasks reviewed and approved (2026-08-02)
- [x] Writing complete (2026-08-03)
- [x] Documentation reviewed (2026-08-03)
- [ ] Spec closed

### Closing note (2026-08-03)

All 33 tasks complete; every requirement P0-1 through P2-20 is covered. **Nothing was
dropped** — including P2-19, which the tasks flagged as the item to cut if effort ran
short.

**Deliverables:** `contents/ReplayOnSeen.md` (1,037 lines) and
`contents/CausationTrackingStores.md` (151 lines), plus updates to nine existing pages
(`BrighterInboxSupport`, `BrighterOutboxSupport`, `BrighterBasicConfiguration`,
`PipelineValidation`, `DynamoOutbox`, `Glossary`, `BoxProvisioning`,
`BoxProvisioningUpgrade`, `V10MigrationGuide`) and two `SUMMARY.md` entries.

**QA checklist result:**

- *Code* — all 12 examples verified against V10 source in Task 4.3. Every C# block is
  fenced `csharp`; the three unlabelled fences are plain-text output (the flow diagram
  and two quoted log blocks) and correctly carry no language. Three defects were found
  and fixed, notably two quoted validation errors naming stores that always implement
  the role interfaces and so could never produce them.
- *Content* — terminology matches the Glossary and design §Style Notes (Task 4.5, 5
  fixes). No "idempotent"; "cascade" stays descriptive; the page title never leaks into
  prose as a pseudo-term.
- *Structure* — both pages are in `SUMMARY.md`; `linkcheck.py` exits 0 across 108 files;
  each page ends in Further Reading.
- *Accuracy* — claims checked against source, not release notes alone. The
  `CausationIndexName` guidance was reversed to match `DynamoDbConfiguration.cs`, which
  says "Leave this at the default".

**Deliberate deviations, all recorded in `design.md` §Style Notes:** no Sample Code
section (no sample in `Brighter/samples/` uses replay — the end-to-end test is cited
instead); "You Must Thread Your RequestContext" is an H2 prerequisite rather than a
pitfall; store support is a matrix rather than per-store pages; and the
`BoxProvisioning.md` corrections belong to spec 005's pages but are fixed here.

**Known gap, out of scope:** `ReplayOnSeen.md` is 1,037 lines against a design target of
450–520. The target was mis-estimated at design time by roughly 2.2×, not overrun; the
sanctioned custom-store split removed 128 lines and could not close that distance. The
page sits within repo norms (`QueryPatterns.md` 1,289; `CQRSWithBrighterAndDarker.md`
1,142). See Task 4.2 for the full reasoning.

Separately, Task 4.4 recorded 10 pre-existing orphan pages absent from `SUMMARY.md`,
none introduced by this spec. They deserve their own change.

## Next Steps

1. Review existing documentation in SUMMARY.md for related content
   (Inbox / `OnceOnly`, Outbox, Outbox Sweeper, BoxProvisioning / database migration,
   pipeline validation at startup — see specs 003 and 005)
2. Identify source material (source code, ADRs, release notes, samples)
3. Identify gaps in current documentation
4. Create requirements document
5. Get requirements approved before proceeding

## Notes

- Follow CLAUDE.md guidelines for documentation standards
- Reference source code in ../Brighter and ../Darker as needed
- Ensure SUMMARY.md is updated when new files are created
- Related existing specs: `003-pipeline-validation-at-startup` (validation of the
  replay configuration at startup) and `005-database_migration` (BoxProvisioning,
  which delivers the `CausationId` column)
