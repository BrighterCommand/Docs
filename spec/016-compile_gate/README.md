# Spec 016: A Committed Compile Gate

**Created:** 2026-09-20
**Status:** **CLOSED — 39 of 39, 2026-09-25.** `.accepted` waits on the maintainer reading `tasks.md` § *Acceptance walk*

> **Re-derive this README before executing it.** It was written before anyone looked — check every
> count and every named gap against the tree, with the command beside the figure.

## Topic Overview

**015's closing sentence is this spec's subject:**

> A name that still resolves — to a different signature, to a dependency's removed member, or to a
> package with no release — is caught by nothing here, because the only instrument that sees it was
> built in `/tmp` and was never committed.

`tools/README.md` lists **eight gates and none of them compiles a line of C#**. Every one resolves
*names*: `symbolcheck` against a watchlist, `--census` against two products' `src/`, `versioncheck`
against NuGet versions, `pagelint` rule 6 against the *presence* of `using` lines. A name that
resolves to the wrong thing passes all eight.

Six of 015's thirty ledgered defects were found by a compiler, and **four of those six are
invisible to every instrument in this repository** (`tasks.md` § *Defect ledger*, defects 4–8):

| Defect | Page | Why no gate here can see it |
|---|---|---|
| `S3Region.EUW1` does not exist in AWS SDK v4 | `S3LuggageStore.md` | a **dependency's** name — neither product's census reaches it |
| `IAmAMessageMapper<T>` gained `Context` and a `Publication` parameter | `MessageMappers.md` | the name **resolves**; the signature moved |
| `With()`→`StartNew()`, `DefaultPolicy()`→`DefaultResilience()`, `Build()` unreachable | `FeatureSwitches.md` | the names resolve, on a type that still exists |
| `Paramore.Brighter.{DB}.Dapper` has no V10 release | `DapperOutbox.md` | spelled correctly; it is the **package** that does not exist |

All four are fatal to a reader who copies the block, and a *packages you need* list is the first
thing a reader acts on.

## Subject

**process.** It is an instrument and a gate, not a page — the same shape as 014, and `SUMMARY.md`
is N/A for it.

**But it may not stay N/A.** 015 was scoped as a list and Q4 turned it into repairs, at which point
**obligation 7 began to bind** and the published site changed. A compile gate that is only built
and never run over the corpus proves nothing; a run over the corpus will find defects; repairing
them changes pages. **Assume this spec ships documentation changes and plan the sign-off, rather
than discovering it at a phase boundary.**

## Current State

Every figure here carries the command that produced it, measured 2026-09-20 at `d412702`.

**The corpus a compile gate would face:**

```bash
python3 -c "
import sys,re; sys.path.insert(0,'tools')
import pagelint as pl
pages=pl.load_pages(); tot=have=0; pgs=set()
for rel,p in pages.items():
    for b in p.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp','c#','cs'): continue
        tot+=1; pgs.add(rel)
        if any(re.match(r'\s*using\s',l) for _,l in b['body']): have+=1
print(tot,'csharp blocks across',len(pgs),'pages;',have,'carry a using line')"
#   985 csharp blocks across 145 pages; 248 carry a using line
```

- **985 C# blocks, 145 pages.** 248 carry a `using` line; **737 do not**, of which **213 declare
  the omission with `// ...`**.
- **This figure was checked by a second method during `/spec:requirements` and the two disagreed**
  — `grep -rc '^```csharp$'` returns **835 across 117 files**, because 150 blocks use a fence the
  grep does not match (` ``` csharp`, indented, ` ``` c#`). The count above survives; **the
  confidence in a single-method count does not.** `requirements.md` § *Current state* carries the
  breakdown and friction **53**.
- **13 of the 985 have ever been compiled** — 015 phase 4's fifteen edited blocks, minus the two
  that were prose-only sites. `ls /tmp/claude-501/blockcheck/*/blocks/*.cs | grep -cv Scaffold` →
  **13**. That is **1.3% of the corpus**.
- The using-directive debt is a **gate figure and lives in `tools/README.md` row 2** (obligation
  10). Do not paste it here; cite the row. What matters to this spec is that **rule 6 counts lines
  and cannot compile** — phase 4's fifteen blocks each needed more than imports pasted on top.

**The instrument that exists, and its problem:**

`/tmp/claude-501/blockcheck/` — **68 lines of Python**, three `.csproj`, 13 block files, four
prelude files and a `Scaffold.cs`. It is **not committed, and `/tmp` is not durable**: it survived
sessions 74 and 75 by luck. `wc -l /tmp/claude-501/blockcheck/*.py` → 57 + 11.

| Piece | What it does |
|---|---|
| `extract.py` | pulls one fenced block out of a page **through `pagelint.Page`** — the shipped parser, not a second one — byte for byte, wrapping only as far as a class for a bare method or a method for bare statements |
| `preludes/`, `Scaffold.cs` | the identifiers a page names inside a **declared** omission — `producerRegistry`, `credentials`, the `Person`/`Greeting` domain. **Not part of any block**, and listed in the write-up so the boundary is visible |
| three projects | `Paramore.Brighter.DynamoDb` pulls `AWSSDK.Core` 3.x and `Paramore.Brighter.Transformers.AWS.V4` pulls 4.x; NuGet refuses the pair (**`NU1107`**). A reader installing one page's packages never hits this; a harness compiling every page at once does |

**All `PackageReference`s are at `10.7.0`, from NuGet.** Never a `ProjectReference` into
`../Brighter/src` — that compiles against unreleased code and vouches for an API nobody can install
(`CLAUDE.md` § *Compiling an example, and against what*; spec 013 phase 2 found exactly that).

**The gates today:** eight, at `tools/README.md`. Six re-run 2026-09-20 and all on that file's
figures — `linkcheck` 165/0, `pagelint` 0 errors, shape 161/12, redirects 77, `versioncheck` 0 of
18, `symbolcheck` 0 findings / 22 entries / 3 silenced. `optioncheck` and `--verify` not re-run.

## Acceptance criteria and open questions — **`requirements.md` is the authority**

**Both lists moved to `requirements.md` on 2026-09-20 and this section is deliberately a pointer.**
This README drafted **11** criteria and **7** questions; the requirements document carries **15**
and **9**, and one figure had already diverged between the two copies — a Darker package count
this README put at 17 and the measurement put at **6**.

> **Two copies of a list disagree the moment one of them is updated**, and the copy nobody is
> reading is the one that goes stale. `PROMPT.md` has carried **five** such duplicate pairs, and
> every one was written by somebody who had read the first copy and did not recognise it. This one
> was caught at fifteen minutes old rather than at two sessions.

See `requirements.md` § *Acceptance criteria* — 12 instrumented, **3 marked as having none with a
named reader** — and § *Open questions*, nine, each with a recommendation. **Q1 (the green bar) and
Q6 (does this spec repair what it finds) change what gets built.**

## Already done, 2026-09-20 — the rescue, with its evidence

**The harness is out of `/tmp` and into `spec/016-compile_gate/harness/`**, before any requirement
was written, because it is the evidence base for four of 015's defects and `/tmp` guarantees
nothing. `bin/` and `obj/` were excluded by the copy and are ignored anyway — spec 012's probes put
those lines in `.gitignore`.

**Four files were deliberately not committed, and the byte-identity claim below is scoped to that.**
`harness/hold/` held **two exact duplicates** of block files already in `core/blocks/` plus a
**superseded** `Scaffold.cs`, and `core/Probe.cs.txt` is a 31-line earlier draft of the 61-line
`core/Probe.cs` beside it. Committing them would plant the duplicate pair this programme has found
**five times in `PROMPT.md`** — in file form, where it is even easier to miss. All four survive in
`/tmp/claude-501/blockcheck/` for as long as it does; none is referenced by any project. **`core`
rebuilds 0 errors / 4 warnings without `Probe.cs.txt`**, which is the check that the subtraction
changed nothing.

**`core/Probe.cs` was kept and it is not scratch** — it is the **fourth instrument** of 015 phase 4:
reflection over the released `Paramore.*.dll` files, asking what NuGet actually ships rather than
what `src/` contains. Every replacement name in the seventeen watchlist rows was checked against it.

**A copy that does not build is not a rescue**, so all three projects were rebuilt at the new path
with `--no-incremental` (session 75: a 0.54s run reporting 0 warnings is MSBuild declining to work,
in a shape indistinguishable from a pass):

| Project | Recorded in `PROMPT.md` | At the new path |
|---|---|---|
| `core` | 0 errors / 4 warnings | **0 / 4** — `CS0618` on `PolicyRegistry`, three `CS0649` |
| `dynamo` | 0 / 0 | **0 / 0** |
| `s3` | 0 / 0 | **0 / 0** |

**Red-proofed at the new path**: `configure.Outbox` → `configure.OutboxThatDoesNotExist` in
`ShowMeTheCode_6.cs` produced **`CS1061`** and nothing else changed; restored, the build is 0/4
again. **And the copy was byte-identical to the original when it was taken** —
`diff -r --exclude=bin --exclude=obj harness/ /tmp/claude-501/blockcheck/` was empty, with the
`diff` itself controlled by appending one byte and confirming it reported the difference. **It is
no longer empty, by the deliberate subtraction above**: today it reports exactly the four dropped
files and nothing else, which is the check that nothing *else* drifted.

> **Friction candidate for `requirements.md` — number 52.** The first identity check run here was
> `shasum blocks/*.cs | shasum` before and after, and the two aggregates **differed** with the
> content unchanged: `shasum` prints the path beside the digest, and the two runs were made from
> different working directories. **A control whose two halves measure different things reports a
> difference that is not there** — the mirror image of a plausible zero, and it fails in the
> direction that wastes an hour rather than the direction that ships a defect. Same family as
> friction 48: *the form that gets run is not the form that was reviewed.*

## Status Checklist

- [x] Requirements gathered — `requirements.md`, 2026-09-20
- [x] Requirements reviewed and approved — `.requirements-approved`, 2026-09-20, **with the nine
      open questions left open**; their recommendations are the design's working assumptions
- [x] Documentation outline created — `design.md` + `probe/`, 2026-09-20
- [x] Outline reviewed and approved — `.design-approved`, 2026-09-20. **Q6 ruled *repair*** at that
      review; Q1, Q2, Q4 and Q7 settled by the probe; **Q3, Q5, Q8, Q9 still open** and none
      changes the plan
- [x] Writing tasks identified — `tasks.md`, 2026-09-20, **39 tasks / 5 phases / 5 PRs**, six
      review findings answered, two tasks inserted as `1.6a` and `4.3a`
- [x] Writing complete — **39 of 39**, 2026-09-25, re-derived:
      `grep -c '^- \[x\] \*\*Task' spec/016-compile_gate/tasks.md`. PRs #180–#183, and phase 5's
- [x] Documentation reviewed — `tasks.md` § *Acceptance walk*. **AC13 and AC14 accepted by the
      maintainer 2026-09-25**; AC15 met with one sentence qualified; the twelve instrumented
      criteria met, three of them (AC3, AC7, AC10) by instruments that disagree with their wording
- [x] Spec closed — 2026-09-25. § *What 016 shipped* and the residual gap are the line 017 starts
      from. **Not yet accepted**: `.accepted` is the maintainer's

## Next Steps

1. ~~**Re-derive the counts above**~~ **DONE 2026-09-20 at `/spec:requirements`**, and the
   re-derivation paid: a second method returned **835**, the 150-block gap is fence spelling, and
   **28 pages holding 92 blocks are invisible to a grep-shaped extractor** — `ShowMeTheCode.md`
   among them. Friction **53**.
2. ~~**Rescue `/tmp/claude-501/blockcheck/` first.**~~ **DONE 2026-09-20** — see § *Already done*.
   It is under `harness/`, byte-identical to the original, and all three projects rebuild at the
   new path to the recorded figures. **It is not yet committed**, and it is not yet a tool: it is
   three hand-made projects with the block files already extracted.
3. Read `spec/015-census_triage/tasks.md` § *Defect ledger* and § *Every edited block built against
   the released packages (task 4.8)* — the worked example this spec generalises.
4. Read `CLAUDE.md` § *Compiling an example, and against what* and the three **review only** rows
   of its ledger. Two of the three are what this spec would give an instrument to.
5. ~~Run `/spec:requirements`~~ **DONE 2026-09-20.** **Next: `/spec:review`** — nine questions want
   ruling, and **Q1** (the green bar) and **Q6** (does this spec repair what it finds) change what
   gets built.

## Notes

- `CLAUDE.md` is the authority on documentation standards; cite it rather than restating it
- `tools/README.md` is the authority on the gates and their expected numbers
- Source code lives in `../Brighter` and `../Darker`, and is **read-only**
- `SUMMARY.md` gets updated whenever a page is added, or the page is an orphan
