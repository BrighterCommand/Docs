# Spec 017: Compile Repairs

**Created:** 2026-09-26
**Status:** **CLOSED 2026-10-02 — 42 of 42.** Awaiting the maintainer's acceptance; the residual is `tasks.md` § *What 017 shipped*

> **Re-derive this README before executing it.** It was written before anyone looked — check every
> count and every named gap against the tree, with the command beside the figure.

> **Superseded in part by `requirements.md` (2026-09-26).** The 102 and 364 below do not
> re-derive: the committed probe (`probe/run.sh`) reads **113** page-type and **368** import at 872
> FAILED, and only **46** import blocks build once given their `using`s — so the work goes by page,
> not by cause. The requirements own the figures from here.

## Topic Overview

Spec 016 built the gate: `tools/blockcheck.py` compiles every C# block on the site, alone, against
the packages a reader would install (Brighter 10.7.0, Darker 4.1.1, 71 packages pinned in
`tools/blockcheck/refs/refs.csproj`). It did not make the blocks compile. Its closing sentence,
`spec/016-compile_gate/tasks.md` § *What 016 shipped*, is this spec's subject:

> **872 of 989 C# blocks still do not compile against the released packages.** Most need a `using`
> directive or a stub for a type the page names but never shows (102 of them are ruled
> scaffoldable). The gate holds only the 101 that do, so 017 raises that number page by page,
> starting with those 102, and corrects the instruments for AC3, AC7 and AC10.

A block that does not compile is a block a reader cannot trust, and a block the gate cannot hold.
Every repair that makes one build adds its row to `tools/blockcheck/baseline.tsv` in the same PR,
and from then on CI keeps it building.

## Subject

**Process** — with a reader problem underneath it. No feature is being documented; the pages
already describe the features. What changes is whether their code is true of the released packages.

## Current State

Measured 2026-09-26 at `c7329bb`, after
`dotnet build tools/blockcheck/refs/refs.csproj -c Release` and
`dotnet build tools/blockcheck/blockcheck.csproj -c Release`.

| Figure | Value | Command |
|---|---:|---|
| C# blocks | **989** | `python3 tools/blockcheck.py --report $TMPDIR/r.tsv` — last summary line |
| BUILT | **101** | same line; equals the baseline's required count |
| FAILED | **872** | same line |
| SKIPPED (opt-out, each with a reason) | **16** | same line; the reasons print under `----- skipped by opt-out (16) -----` |
| Scaffold units / pages mapped | **14 / 15** | `ls tools/blockcheck/scaffold/units`; the run's first line |
| Pages with at least one failing block | **140** | `awk -F'\t' '$1=="FAILED"{print $2}' r.tsv \| sort -u \| wc -l` |
| …of which have **nothing** that builds | **97** | pages in the FAILED set absent from the BUILT set |
| C# blocks with no `using` directive | **743 across 115 pages** | `python3 tools/pagelint.py` — last line |

**Where the failures concentrate** (FAILED rows per page, from the same report): `QuartzScheduler.md`
27, `HangfireScheduler.md` 27, `QueryPipeline.md` 25, `AwsScheduler.md` 23, then four pages at 21
(`V10MigrationGuide.md`, `ImplementAQueryHandler.md`, `CommandProcessorConfigurationReference.md`,
`AzureScheduler.md`). **By diagnostic**, counting each code once per block: `CS0246` (type not found)
in 715, `CS0103` (name not found) in 631, `CS1002` (`;` expected) in 126.

### 016's classification is stale, and was never committed

The **102** (a type the page names and never shows) and the **364** (a missing name that *is* a
pinned type — the page needs a `using`) come from 016 task 3.3 (`tasks.md` line ~1465). They were
measured **at 92 BUILT / 905 FAILED**, before phase 4's repairs, by looking missing names up in a
dump of 27,921 public types built **in a scratch project that was not committed**. Neither figure
can be re-derived by anything in the tree today, and both have moved since by an unknown amount.
**The first task of requirements is to re-derive them with an instrument that is committed** — see
open question 1.

### The three instruments that disagree with their criterion — all three reproduce

| 016 criterion | Said | Measured 2026-09-26 |
|---|---|---|
| **AC3** | `ls tools/blockcheck/*.csproj` lists the projects to check | lists **1**; `find tools/blockcheck -name '*.csproj'` finds **2** — the glob misses `refs/refs.csproj`, the one carrying the 71 pins |
| **AC7** | `python3 tools/blockcheck.py --list-skips` prints one reason per skip | exits **2**, *"unknown mode"*; the flag was never built. The skips *are* printed, on every `--report` run |
| **AC10** | `grep -vc '^./spec/'` excludes the spec documents | BSD grep's `-rn … .` prints `contents/…`, not `./contents/…`, so the pattern excludes **nothing**. `^(\./)?spec/` works under both |

### Found in 016 and not repaired

- `InMemoryScheduler.md` names `ITimerProvider`, which does not exist; the scheduler takes a
  `TimeProvider`
- `[UsePolicy(` on **async** handlers in `ReactorAndProactor.md`,
  `HowConfiguringTheCommandProcessorWorks.md`, `PolicyFallback.md`, `ImplementingExternalBus.md`
- `CQRSWithBrighterAndDarker.md` writes `Id = command.Id` against an `Order` it never shows

### What the pin cannot judge

No Jaeger exporter, no `Hangfire.AspNetCore`, no Quartz `IServiceCollectionQuartzConfigurator` in
`refs.csproj`, so blocks naming them fail for a reason that says nothing about the page.
`HangfireScheduler.md` and `QuartzScheduler.md` are the two worst pages above — how much of their
27 each is the pin rather than the page is unmeasured.

### Out of scope, recorded so nobody chases it

- **The four Replay On Seen blocks** (`TurningOnReplayOnSeen.md` #1 and #6,
  `CausationTrackingStores.md` #1, `ReplayOnSeenReference.md` #1) use API on Brighter `master`, not
  in 10.7.0. They stay FAILED; the next pin bump brings them in through the ratchet
- **BrighterCommand/Brighter#4414** — `AutoFromAssemblies()` registers `FireSchedulerRequestHandler`
  twice at 10.7.0. Upstream's, not ours
- **Instrument quirks:** the `statements` wrapper (`Holder.Run()`) supplies no `args`; `--explain`
  prints a wrong summary line on stderr. In scope only if a repair trips on them

## Acceptance criteria

Draft — `/spec:requirements` sets the targets. Each verdict is read from a file with the exit code
taken first, never through a pipe (016's AC1/AC2/AC10 finding).

| # | Criterion | Decided by |
|---|---|---|
| **AC1** | BUILT rises from 101 to a target set at requirements, and `baseline.tsv` requires exactly the BUILT set | `python3 tools/blockcheck.py --report r.tsv > out; echo $?` → 0, 0 findings; BUILT count against the target |
| **AC2** | No block moves BUILT → FAILED, and no block is newly SKIPPED to make a number | diff of the 016-close report (`c7329bb`) against the final one: every changed row is `FAILED → BUILT`, or a skip with a reason the review accepted |
| **AC3** | The missing-name classification (import / page-type / same-page-type / values / other) is produced by a **committed** instrument and reproduces from the tree | the instrument run twice from a clean clone gives identical output; its figures replace 016's 102 and 364 |
| **AC4** | The scaffoldable page-type blocks are scaffolded, or each left out with its reason | the instrument of AC3: the page-type row at the close, every remaining block named with a reason |
| **AC5** | `pagelint`'s `using` debt falls, and no page is repaired with `// ...` standing in for a `using` it could have had | `python3 tools/pagelint.py` last line against 743; **no instrument** for the second half — checked by reading, by the reviewer |
| **AC6** | 016's AC3, AC7 and AC10 instruments are corrected, each with a control that shows the old form failing | each corrected command run beside the old one; the old reads wrong, the new reads right |
| **AC7** | The three unrepaired falsehoods (`ITimerProvider`, `[UsePolicy(` on async handlers, the unshown `Order`) are repaired **everywhere they occur** | `grep -rn 'ITimerProvider' contents/` → 0; the `UsePolicy` recurrence grep set at requirements; the `Order` page's block builds |
| **AC8** | A repaired block that asserts behaviour is **run, with a control** | **no instrument** — checked by reading, by the reviewer against the PR's recorded runs |
| **AC9** | `tools/README.md` carries the gate's new figures, and nowhere else does | 016 AC10's grep with the corrected `^(\./)?spec/`, plus its duplicated-figure control |
| **AC10** | Every other gate stays at `tools/README.md`'s figures | the eight other gates re-run at the close |

## Open questions

1. **How is the classification committed?** 016's type dump lived in a scratch project.
   *Recommendation:* a `--classify` mode on `blockcheck.py` that reads the pinned assemblies'
   public types through the existing Roslyn host and prints one row per failing block. It depends
   on whether a type dump of 501 assemblies is fast enough to run in CI; if not, commit it as a
   generated file with a staleness stamp, the way the reference list already is.
2. **What is a stub allowed to contain?** AC13 ruled the 102 scaffoldable "as stubs". A stub of
   `IPersonRepository` with no members lets `repository.Get(id)` fail; one with members invents API
   the page never showed. *Recommendation:* members exactly as the page's own blocks call them, no
   more, and never a type the page tells the reader to write (016 design rule 1 stands).
3. **Does the page change, or the scaffold?** A block that is missing a `using` can be fixed on the
   page (the reader benefits) or in the scaffold (only the gate does). *Recommendation:* `using`s go
   on the page, always — they are 016's *import* row and `CLAUDE.md` § *Complete code blocks* asks
   for them; scaffold is for context the reader would already have.
4. **Order of work.** *Recommendation:* the scaffoldable tranche first (no page edits, cheapest per
   block), then pages ranked by FAILED count, since a page repaired whole also clears its `pagelint`
   warnings. Depends on the re-derived classification.
5. **Does the pin grow?** Adding Hangfire.AspNetCore, a Jaeger exporter and Quartz's DI package
   would let the scheduler pages be judged. *Recommendation:* yes, as one early task, measured
   before and after so the change's effect on BUILT/FAILED is recorded on its own.
6. **What is the target?** 016 did not set one. *Recommendation:* set it after question 1 is
   answered, from the re-derived classification — not from 016's stale 102 + 364.
7. **Phasing and PR size.** 140 pages with failures is more than one PR can be reviewed in.
   *Recommendation:* one PR per phase, a phase being a tranche of pages, each landing its baseline
   rows with it; the maintainer's sign-off per site-changing PR stands.

## Status Checklist

- [x] Requirements gathered — `requirements.md`, 2026-09-26
- [x] Requirements reviewed and approved — 2026-09-26
- [x] Documentation outline created — `design.md`, 2026-09-26
- [x] Outline reviewed and approved — 2026-09-26
- [x] Writing tasks identified — `tasks.md`, 2026-09-26
- [x] Writing complete — **42 of 42**, 2026-10-02, re-derived:
      `grep -c '^- \[x\] \*\*Task' spec/017-compile_repairs/tasks.md`. PRs #190–#194, and phase 6's
- [x] Documentation reviewed — `tasks.md` § *Phase 6 as executed*, the acceptance walk. **AC6
      accepted by the maintainer 2026-09-28**; AC1–AC5, AC7, AC8's first half, AC9, AC10 (at 6.5) and
      AC12–AC14 met. AC8's second half and AC11 were unmet at 6.1 and repaired in phase 6's PR under the
      2026-09-28 ruling, so the readers re-read them in that PR's review
- [x] Spec closed — 2026-10-02. § *What 017 shipped* and the residual are the line 018 starts
      from. D4 ruled for 018 (6.7)

## Next Steps

1. Re-derive the counts above
2. Read `SUMMARY.md` for where this sits, and `contents/` for what already covers it
3. Identify source material — `spec/016-compile_gate/` (requirements, design, tasks and its AC
   walk), `tools/README.md`, `tools/blockcheck/`, and the pinned packages in `refs.csproj`
4. Run `/spec:requirements`, and get the requirements approved before going further

## Notes

- `CLAUDE.md` is the authority on documentation standards; cite it rather than restating it
- `tools/README.md` is the authority on the gates and their expected numbers
- Source code lives in `../Brighter` and `../Darker`, and is **read-only**
- `SUMMARY.md` gets updated whenever a page is added, or the page is an orphan
