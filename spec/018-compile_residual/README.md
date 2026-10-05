# Spec 018: Compile Residual

**Created:** 2026-10-04
**Status:** Tasks Phase — `tasks.md` drafted 2026-10-05, awaiting `/spec:review`

> **Re-derive this README before executing it.** It was written before anyone looked — check every
> count and every named gap against the tree, with the command beside the figure.

## Topic Overview

Spec 017 ended with **674 of 990** C# blocks that still do not compile against the released
packages. **630** of them are on the **64** pages that 017's tranches never reached. 018 picks up
017's residual sentence (`spec/017-compile_repairs/tasks.md` § *What 017 shipped*) in the order
that sentence sets out:

1. **`pagelint` rule 8**, the D4 ruling of 2026-09-29 (017 task 6.7). It turns
   `probe/attr_mismatch.py` into a standing gate. It needs a per-block opt-out with a reason,
   `PAIRED` beside `APPLIES_TO`, a `CLAUDE.md` ledger row, the red-proof carried over, and the
   probe retired.
2. **`--classify`'s second compile** (017 friction #71). It supplies the `using`s, compiles
   again, and classifies what remains, so the tranches are decided by one committed, red-proofed
   instrument rather than by a probe in `spec/`.
3. **Tranche the 64 unreached pages** by what that instrument finds, and repair them.

## Subject

**process** — a gate is built, an instrument is extended, and blocks are repaired page by page
against existing rules. No new reader-facing topic is added.

## Current State

Every figure below is **inherited from 017 at `e0385b4`**. All of them are stale until re-derived.

| Figure | Inherited value | Command |
|---|---:|---|
| C# blocks: BUILT / FAILED / SKIPPED | 990: 299 / 674 / 17 | `python3 tools/blockcheck.py --report $TMPDIR/r.tsv` (after building `refs.csproj` and `blockcheck.csproj`) |
| FAILED on the 64 off-tranche pages | 630 | `--classify` joined to 017 § *The tranches* |
| …by class: import / parse / page-type / values / other / same-page | 314 / 145 / 113 / 34 / 18 / 6 | `python3 tools/blockcheck.py --classify` |
| Off-tranche pages with nothing BUILT | 36 | the same join |
| `pagelint` `using` debt | 524 blocks, 66 pages | `python3 tools/pagelint.py` (warning count) |
| `attr_mismatch` hits | 1, the deliberate `PipelineValidation.md` #7 | `python3 spec/017-compile_repairs/probe/attr_mismatch.py` |

**Known lower bound:** *import* undercounts the work. At 017 task 1.10, **162** blocks that
`--classify` called *import* had a defect behind the missing `using` (friction #71). That is the
reason for item 2.

**Friction carried in from 017** that binds this spec: #69 (commit every helper used by more than
one task, and every behaviour run's `Program.cs`, under `spec/018-*/probe/`), #71 (above) and #79
(name a hit by page and block, never by line).

## Acceptance criteria

Provisional. `/spec:requirements` settles them.

1. **Rule 8 exists and is red-proofed.** `python3 tools/pagelint.py` reports `ATTRIBUTE KIND`. A
   plant hits on both mismatch directions and stays silent on a matched pair, and CI runs it.
   Decided by the plant command, run as a two-way control.
2. **Rule 8 is on the ledger.** `CLAUDE.md` § *The ledger* has the row, and § *Page Conventions*
   has the section. `pagelint` and `CLAUDE.md` read the same in both directions. **No instrument —
   checked by reading**, by the reviewer.
3. **The deliberate hit is marked, not counted.** `PipelineValidation.md` #7 carries
   `<!-- pagelint: attr-mismatch-intended <reason> -->`, and repo-wide rule 8 hits are **0**.
   Decided by `python3 tools/pagelint.py`.
4. **The probe is retired.** `spec/017-compile_repairs/probe/attr_mismatch.py` is no longer run by
   any task or workflow, and its history stays in 017. Decided by `git grep attr_mismatch -- .github tools`.
5. **`--classify` compiles twice.** A block whose only fault is a missing `using` classifies as
   fixable, and a block with a defect behind the `using` classifies by that defect. Decided by a
   red-proof with one case of each, recorded in `tasks.md`.
6. **The 64 pages are tranched by the committed instrument**, with no probe column in any tranche
   table. **No instrument — checked by reading**, by the reviewer.
7. **The residual falls, and the baseline holds what built.** BUILT rises from the re-derived
   start, and `--report` exits 0. Decided by `--report`, against the re-derived figure.
8. **Behavioural claims in repaired blocks are run with a control** (`CLAUDE.md` § *Compiling an
   example*). Each run's `Program.cs` is committed beside its row (friction #69). **No instrument —
   checked by reading**, by the reviewer.

## Open questions

1. **Does rule 8's plant live in a `pagelint --plant-attr` flag or a plant file?**
   *Recommendation:* a plant file under `tools/`, run by CI. `pagelint` keeps one CLI shape, and
   the plant can be read as a fixture. Depends on how CI invokes `pagelint` today.
2. **What does `--classify` output for a block that BUILDs once `using`s are supplied?**
   *Recommendation:* a new class, e.g. `import-only`, kept distinct from `import`. Then the
   lower bound and the true figure both stay visible. Depends on whether 017's class names are
   consumed by anything committed.
3. **Can 018 tranche all 64 pages, or must it close on a residual again?** *Recommendation:* set
   the tranche count after the second compile, from what it finds, and do not promise all 64
   up front. Depends on item 2's output.
4. **Does a tool-only phase 1 (rule 8 + second compile) ship as its own PR before any page
   changes?** *Recommendation:* yes, following 017's phase 1. It changes no page beyond the one
   marker, so it needs no site sign-off. The marker on `PipelineValidation.md` does change the
   published source, so ask anyway.

## Status Checklist

- [x] Requirements gathered — `requirements.md`, 2026-10-04
- [x] Requirements reviewed and approved — 2026-10-04
- [x] Documentation outline created — `design.md`, 2026-10-04
- [x] Outline reviewed and approved — 2026-10-05
- [x] Writing tasks identified — `tasks.md`, 61 tasks, 2026-10-05
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Re-derive the counts above
2. Read `SUMMARY.md` for where this sits, and `contents/` for what already covers it
3. Identify source material: 017's `tasks.md` (§ *What 017 shipped*, § *For the maintainer: D4*,
   the friction ledger 67–80), `tools/README.md`, `tools/pagelint.py`, `tools/blockcheck.py` and
   `CLAUDE.md`
4. Run `/spec:requirements`, and get the requirements approved before going further

## Notes

- `CLAUDE.md` is the authority on documentation standards; cite it rather than restating it
- `tools/README.md` is the authority on the gates and their expected numbers
- Source code lives in `../Brighter` and `../Darker`, and is **read-only**
- `SUMMARY.md` gets updated whenever a page is added, or the page is an orphan
