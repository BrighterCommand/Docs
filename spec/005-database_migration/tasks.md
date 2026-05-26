# Tasks: Box Database Migration

**Spec:** 005-database_migration
**Created:** 2026-05-26
**Status:** Draft — awaiting review
**Design:** [design.md](./design.md)
**Requirements:** [requirements.md](./requirements.md)

## Overview

**Total tasks:** 15, grouped into 4 phases.

| Phase | Goal | Tasks |
|-------|------|-------|
| Phase 1: Research & Preparation | Verify source-of-truth references before writing any prose | 3 |
| Phase 2: Core Documentation (P0) | Ship the user-facing surface needed for someone to adopt BoxProvisioning | 6 |
| Phase 3: Supporting Documentation (P1) | Round out glossary, upgrade story, conceptual support pages | 3 |
| Phase 4: Polish & Review | Wire navigation, verify links, QA against the CLAUDE.md checklist | 3 |

**Critical path:** 1.1 → 2.1 → 2.2 → 2.3a/2.3b/2.4 (parallel) → 4.1 → 4.2 → 4.3. Phase 3 can begin once Phase 2 P0 pages exist.

**Working-directory convention:** all paths under `contents/` and `SUMMARY.md` are in this `Docs` repo. All paths beginning `Brighter/` or `../Brighter` are read-only references (CLAUDE.md hard rule — never modify).

---

## Phase 1 — Research & Preparation

These tasks produce no published output. They produce *notes* the writer will reference in Phase 2. If any check fails, raise it before continuing — the design assumes specific names and shapes.

- [ ] **Task 1.1:** Verify BoxProvisioning extension-method surface against implementation
  - Input: `../Brighter/src/Paramore.Brighter.BoxProvisioning.MsSql/` (and `*.PostgreSql`, `*.MySql`, `*.Sqlite`, `*.Spanner` siblings); ADR 0053 §3
  - Output: short notes (in scratch, not committed) confirming or correcting: extension-method names (`AddMsSqlOutbox` etc.), parameter order, `connectionName` overload signature, `MigrationLockTimeout` parameter vs property shapes, the `BoxProvisioningOptions` type name
  - Notes: design.md §"Open items" line 110 and §"Code examples plan" line 726 both call this out as a pre-flight gate. ADRs sometimes drift from implementation. If anything differs, update the design code-example table *before* writing — do not silently diverge.

- [ ] **Task 1.2:** Verify sample anchor `Startup.cs:116` still exists
  - Input: `../Brighter/samples/WebAPI/WebAPI_Dapper/GreetingsWeb/Startup.cs`
  - Output: confirm the file exists and identify the line that hosts the `UseBoxProvisioning(...)` call (line number may have drifted from 116)
  - Notes: used in `BoxProvisioningConfiguration.md` Further Reading. If the call has been refactored into a factory or extracted, update the cross-link target.

- [ ] **Task 1.3:** Scan existing per-backend pages for exact insertion points
  - Input: `contents/MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md`, `SqliteOutbox.md`, `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`, `BrighterOutboxSupport.md`, `BrighterInboxSupport.md`
  - Output: a short table of `{file, line-number-for-insertion, exact-existing-text-to-soften}` for the per-backend Outbox pages (where the "you are responsible" paragraph lives), and the H2 location for the BrighterXxxSupport.md signposts
  - Notes: design.md gives `MSSQLOutbox.md line 23` and `BrighterOutboxSupport.md line ~196` as approximations. Confirm before editing. The four per-backend Inbox pages are append-only (new H2 at the bottom) — verify they're as short as design assumes (~32 lines) and contain no existing provisioning content.

---

## Phase 2 — Core Documentation (P0)

Foundational pages first. Tasks 2.3a, 2.3b, and 2.4 are parallelisable once 2.1 exists (they only need the new section to link *to*).

- [ ] **Task 2.1:** Write `contents/BoxProvisioning.md` (the conceptual page)
  - Input: design.md §"New file 1" (lines 103–246); ADRs 0053 §1/§2/§5/§10, 0057 §1/§3/§6
  - Output: `contents/BoxProvisioning.md` — H1 + 9 H2 sections per the exact heading list in design.md line 175–187; ~220–260 lines
  - Notes: includes the per-backend support matrix and per-backend differences table verbatim from design lines 199–217. One code example only: the abbreviated `__BrighterMigrationHistory` CREATE TABLE (MSSQL flavour). No `UseBoxProvisioning` call-site code on this page — that lives on 2.2. Introduce glossary terms (BoxProvisioning, Migration Chain, Migration History Table, Bootstrap Path, Advisory Lock) with `[term](/contents/Glossary.md#anchor)` — anchor targets are added in Task 3.1.

- [ ] **Task 2.2:** Write `contents/BoxProvisioningConfiguration.md` (the how-to page)
  - Input: design.md §"New file 2" (lines 249–397); ADR 0053 §3/§5/§8/§9; verified call-site shapes from Task 1.1; sample link confirmed in Task 1.2
  - Output: `contents/BoxProvisioningConfiguration.md` — H1 + heading list per design line 323–341; ~280–340 lines
  - Notes: 7 code examples (inventoried in design line 343–356). Verify each compiles in V10 against the actual extension-method surface from Task 1.1 *before* publication. Includes NuGet package table and lock-timeout unit-conversion table (design lines 359–378). Use plain references for glossary terms (first occurrences live on `BoxProvisioning.md`, so anchors will already be familiar to readers who arrived via the suggested order).
  - Depends on: 1.1, 1.2 (verification gates); 2.1 (so cross-links to the concept page resolve)

- [ ] **Task 2.3a:** Update the four per-backend Outbox pages with Option A/B framing
  - Input: design.md §"Updated file template — per-backend Outbox pages" (lines 536–586); insertion-point notes from Task 1.3
  - Output: `MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md`, `SqliteOutbox.md` — each with the new "Provisioning the Outbox Table" H2 inserted after the H1 intro and before `## NuGet Packages`, plus the softened mid-page "you are responsible" note
  - Notes: uniform template; `{Backend}` substitutes to `MsSql` / `MySql` / `PostgreSql` / `Sqlite`. `SqliteOutbox.md` gets the file-locking one-liner appended (design line 585). `PostgresOutbox.md` has no per-page exception (the V1-only quirk is inbox-side). No "deprecated" or "legacy" language anywhere — Option B is a first-class choice.
  - Depends on: 1.3, 2.1

- [ ] **Task 2.3b:** Update the four per-backend Inbox pages with Option A/B framing
  - Input: design.md §"Updated file template — per-backend Inbox pages" (lines 589–620); Task 1.3 notes
  - Output: `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md` — each with a new "Provisioning the Inbox Table" H2 appended at the bottom. `PostgresInbox.md` additionally appends the V1-only sentence (design lines 614–618)
  - Notes: append-only edits, lower risk than 2.3a. The Postgres V1-only sentence is the *only* per-backend page where the V1-only quirk is called out explicitly; the support matrix on `BoxProvisioning.md` covers everyone else.
  - Depends on: 1.3, 2.1

- [ ] **Task 2.4:** Add signposts to `BrighterOutboxSupport.md` and `BrighterInboxSupport.md`
  - Input: design.md §"Updated file — `contents/BrighterOutboxSupport.md`" (lines 624–640) and `§"Updated file — `contents/BrighterInboxSupport.md`"` (lines 644–656); Task 1.3 notes
  - Output: new H2 "Provisioning the Outbox Table" inserted into `BrighterOutboxSupport.md` between `### Outbox Configuration` and `### Outbox Builder`; new H2 "Provisioning the Inbox Table" inserted into `BrighterInboxSupport.md` at the writer-confirmed location
  - Notes: `BrighterInboxSupport.md` placement was deferred to "writer to confirm when implementing" — use the same pattern as the Outbox page (immediately before any builder/DDL section, or near the end if no such section exists). Both signposts are short — one paragraph each.
  - Depends on: 1.3, 2.1

---

## Phase 3 — Supporting Documentation (P1)

These can begin once Phase 2 P0 pages are in place; 3.2 depends on 2.1 and 2.2 (it cross-links them) but not on the per-backend updates.

- [ ] **Task 3.1:** Update `contents/Glossary.md` with five BoxProvisioning terms
  - Input: design.md §"Updated file — `contents/Glossary.md`" (lines 660–702); existing `Glossary.md` structure for placement
  - Output: new H2 "Database Provisioning" inserted between `## Patterns` and `## Messaging`, containing five H3 term entries: BoxProvisioning, Migration Chain, Migration History Table, Bootstrap Path, Advisory Lock — with exact body text per design lines 669–697
  - Notes: anchors must be kebab-case (`#boxprovisioning`, `#migration-chain`, `#migration-history-table`, `#bootstrap-path`, `#advisory-lock`) and match the `[term](#anchor)` links seeded on `BoxProvisioning.md` in Task 2.1. Existing `### Outbox` and `### Inbox` entries under `## Patterns` are left alone (design line 700–702).
  - Depends on: 2.1 (anchor targets only meaningful once the concept page links to them)

- [ ] **Task 3.2:** Write `contents/BoxProvisioningUpgrade.md` (the operator-facing upgrade page)
  - Input: design.md §"New file 3" (lines 401–533); ADRs 0053 §2/§5/§6 and 0057 §1/§2/§3/§5a/§6
  - Output: `contents/BoxProvisioningUpgrade.md` — H1 + heading list per design lines 476–492; ~240–300 lines
  - Notes: operator-facing — no C# beyond cross-references. Five "code" examples are: SQL `SELECT`, log block, three error-message + remediation blocks. Quote the discriminator-gate error messages from ADR 0057 §2 *verbatim* (operators will grep for them). Includes edge-case summary table per design lines 510–516.
  - Depends on: 2.1, 2.2

- [ ] **Task 3.3:** *(reserved — see Notes)*
  - Notes: held open in case Task 1.1 surfaces an ADR/implementation drift that materially changes the configuration page's code examples after they've been written, requiring an explicit revisit-and-refresh task. If 1.1 comes back clean, mark this task n/a and skip.

---

## Phase 4 — Polish & Review

All three tasks run last. 4.1 must precede 4.2 (link verifier needs the new SUMMARY.md entries). 4.3 is the final gate.

- [ ] **Task 4.1:** Update `SUMMARY.md` with the new "Database Provisioning" section
  - Input: design.md §"SUMMARY.md changes" (lines 69–99) — before/after shown verbatim; CLAUDE.md "SUMMARY.md Management"
  - Output: insertion at lines 87–93 region of `SUMMARY.md` adding the new H2 with three child entries, placed between "Outbox and Inbox" and "Health Checks and Observability"
  - Notes: indentation uses single leading space + `*` to match surrounding section style. Three entries: `BoxProvisioning.md`, `BoxProvisioningConfiguration.md`, `BoxProvisioningUpgrade.md`.
  - Depends on: 2.1, 2.2, 3.2 (the three target files must exist; otherwise SUMMARY.md links break)

- [ ] **Task 4.2:** End-to-end cross-link verification
  - Input: requirements.md §Constraints "Cross-linking" + §3 bidirectional rule; design.md §"Cross-links" subsections (one per file outline)
  - Output: a verification pass confirming, for every claim in the design's cross-links subsections:
    - Every per-backend Outbox page (4) links to `BoxProvisioning.md` AND `BoxProvisioningConfiguration.md`
    - Every per-backend Inbox page (4) links to `BoxProvisioning.md` AND `BoxProvisioningConfiguration.md`
    - `BoxProvisioning.md` per-backend support matrix links to all 8 per-backend pages
    - `BoxProvisioning.md` per-backend differences table links to `BoxProvisioningUpgrade.md#mssql-multi-version-upgrades`
    - `BrighterOutboxSupport.md` and `BrighterInboxSupport.md` both link to `BoxProvisioning.md`
    - Glossary anchors resolve from `BoxProvisioning.md`
    - Sample link from `BoxProvisioningConfiguration.md` points to the file/line confirmed in Task 1.2
    - ADR links use `Brighter/docs/adr/...` relative form (CLAUDE.md External Links convention)
  - Notes: report-only; fix any broken or asymmetric link directly. No new prose. CLAUDE.md "Orphaned Files" rule: every new file must be reachable from SUMMARY.md (verified by 4.1).
  - Depends on: 4.1

- [ ] **Task 4.3:** CLAUDE.md QA checklist pass + close-out
  - Input: CLAUDE.md "Quality Assurance Checklist"; all files produced in Phases 2 and 3
  - Output: checklist completed; any issues fixed in place. Then update `spec/.current-spec` or mark spec closed per spec workflow.
  - Notes: covers compile-check of code examples (re-run against `../Brighter/src/Paramore.Brighter.BoxProvisioning.*/` one more time), V10-pattern check (no V9 ServiceActivator-style code), terminology consistency (the eight terms in design §"Style and terminology notes"), and the "Common Pitfalls to Avoid" list. This is the final gate before merge.
  - Depends on: 4.2

---

## Dependency graph (compact)

```
1.1 ─┐
1.2 ─┼──> 2.2 ────┐
1.3 ─┘            │
                  │
   2.1 ──┬──> 2.2 ─┤
         ├──> 2.3a │
         ├──> 2.3b │
         ├──> 2.4  ├──> 4.1 ──> 4.2 ──> 4.3
         └──> 3.1  │
   2.1, 2.2 ──> 3.2 ┘
```

## Acceptance criteria for tasks

The task list is ready to advance to writing (`/spec:implement`) when:

- **AC-T1:** Every design.md file (3 new, 12 updated, plus SUMMARY.md) has a writing task that produces it.
- **AC-T2:** Every task names concrete inputs (what to read) and a concrete output (what file/section).
- **AC-T3:** Phase 1 verification tasks gate Phase 2 — drift between ADR and implementation surfaces before code examples are written, not after.
- **AC-T4:** The final phase has both a SUMMARY.md update and a separate link-verification task (CLAUDE.md "Orphaned Files" + bidirectional cross-link rule).
- **AC-T5:** Tasks are small enough that each fits in one writing session; large pages (2.1, 2.2, 3.2) are one task each because they map to one published artefact.

## Next steps

1. User reviews and approves (or revises) this task list — run `/spec:review`.
2. On approval, run `/spec:implement` to begin Phase 1.
