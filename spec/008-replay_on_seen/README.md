# Spec 008: Replay On Seen

**Created:** 2026-08-01
**Status:** Implementation Phase — requirements, design, and tasks all approved
(2026-08-01 / 2026-08-02 / 2026-08-02)

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
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

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
