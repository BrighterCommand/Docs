# Spec 017: Compile Repairs — Requirements

**Created:** 2026-09-26
**Status:** **APPROVED 2026-09-26** — `.requirements-approved`. The eight open questions were approved open, so each recommendation is the design's working assumption, stated as one wherever it is relied on.

> Every number here carries the command that produced it, measured 2026-09-26 against Docs `master`
> `c7329bb`. Gate figures are **cited from `tools/README.md`**, row 9 and row 2, not restated as
> this document's own.

> **The README's premise does not survive measurement.** It inherited 016's framing — *"most need a
> `using` directive or a stub"*, *"starting with those 102"* — and treats the two as separate
> tranches. Measured: **of 368 blocks whose missing names include a pinned type, 46 build once
> given their `using` directives; 226 of the rest then fail only on names no package ships.** The
> `using` and the stub are mostly the same block's two halves, so the work is organised **by page**,
> not by cause (§ *Current state*, the third table).

## Subject

**Process**, with a reader problem underneath it. The deliverable is repaired C# on existing pages,
the scaffold that lets a repaired block be judged alone, and corrected instruments. **It creates no
page.**

| Section | Status |
|---|---|
| **SUMMARY.md changes** | **N/A.** No page is added, moved or retitled. A category error on this spec, not an empty section |
| **Mode mix** | **N/A.** Diátaxis decides what a page is for; nothing here changes what any page is for |
| **Target audience** | **Applies**, as it did for 016's P0-9: every repair lands on a published page, read by the ordinary reader `CLAUDE.md` writes for |

## Topic overview

016 built `tools/blockcheck.py`, which compiles every C# block alone against 71 packages pinned in
`tools/blockcheck/refs/refs.csproj` (Brighter 10.7.0, Darker 4.1.1), and a baseline that CI's
`blocks` job enforces both ways. It closed on this sentence (`spec/016-compile_gate/tasks.md`
§ *What 016 shipped*):

> **872 of 989 C# blocks still do not compile against the released packages.** … The gate holds
> only the 101 that do, so 017 raises that number page by page, starting with those 102, and
> corrects the instruments for AC3, AC7 and AC10.

A block that does not build is one a reader cannot paste and trust, and one the gate cannot hold.
Every block this spec repairs enters `tools/blockcheck/baseline.tsv` in the same PR, and from then on
CI keeps it building.

The question this spec answers:

> **What does it take to move a page's C# blocks from FAILED to BUILT — honestly, with the reader
> seeing the `using`s and the gate seeing only identifiers — and how many pages can be moved?**

## Current state

### The gate, re-derived

```bash
dotnet build tools/blockcheck/refs/refs.csproj -c Release
dotnet build tools/blockcheck/blockcheck.csproj -c Release
python3 tools/blockcheck.py --report $TMPDIR/r.tsv > $TMPDIR/bc.out; echo $?      # 0
tail -4 $TMPDIR/bc.out | head -1
#   989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED, 0 NOT_COMPILABLE
python3 tools/pagelint.py | tail -1
#   0 errors, 743 warnings (using-directive debt: 743 blocks across 115 pages) across 162 pages.
```

Both match `tools/README.md` rows 9 and 2 at `b941837`. **Nothing has moved since 016 closed.**

| Figure | Value | Command, over `$TMPDIR/r.tsv` |
|---|---:|---|
| Pages with a FAILED block | **140** | `awk -F'\t' '$1=="FAILED"{print $2}' r.tsv \| sort -u \| wc -l` |
| …with **no** BUILT block | **97** | FAILED pages absent from the BUILT set |
| Worst pages | `QuartzScheduler.md` 27, `HangfireScheduler.md` 27, `QueryPipeline.md` 25, `AwsScheduler.md` 23; four at 21 | `awk -F'\t' '$1=="FAILED"{print $2}' r.tsv \| sort \| uniq -c \| sort -rn` |
| Blocks carrying `CS0246` / `CS0103` / `CS1002` | **715 / 631 / 126** | codes in column 6, counted once per block |

### 016's classification, re-derived — the probe is now committed

016 task 3.3 split its 905 failing blocks by looking missing names up in a dump of the pinned
assemblies' public types, **built in a scratch project and never committed**. This spec re-derives
it with the same rules through `spec/017-compile_repairs/probe/` — `typedump/` (a
`System.Reflection.Metadata` walk of `refs.txt`), `classify.py`, `usings.py`, driven by:

```bash
bash spec/017-compile_repairs/probe/run.sh $TMPDIR/probe
```

**The rules, as 016 states them and as `classify.py` applies them, in order:** *other* — any
diagnostic that is not a missing name; *import* — a missing name **is** a pinned type; *context* —
every error is a missing name and none is a pinned type, split into *values* (all lower-case or
`_`), *same-page type* (declared by another block on the page) and *page type* (the rest).

| Class | 016, at 905 FAILED | **017, at 872 FAILED** | Pages |
|---|---:|---:|---:|
| other | 342 | **334** | 103 |
| import | 364 | **368** | 98 |
| context: page type | **102** | **113** | 50 |
| context: same-page type | 21 | **7** | 6 |
| context: values | 76 | **50** | 18 |
| | 905 | **872** | |

**The two methods agree on the large rows and disagree on the split of *context*.** The type dump
holds **21,365** distinct (name, namespace) pairs (`wc -l < $TMPDIR/probe/types.tsv`); 016 reported **27,921** "public types" by a
counting rule it did not record. *Same-page type* falling 21 → 7 while *page type* rises 102 → 113
says the two methods detect a page's own declarations differently — `classify.py` reads
`class|record|interface|struct|enum <Name>` from the staged blocks. **Which is right is unsettled,
and that is exactly why P0-1 exists.** 016's 102 is not quoted below as a current figure.

The names the *page type* row misses most (`awk` over the probe's `classes.tsv`):
`GreetingEvent` 19, `MyCommand` 12, `GreetingCommand` 10, `Program` 8, `OrderCreated` 7,
`Greetings` 7, `GetPeopleQuery` 7, `OrderStatus` 6. **`Program`, `Assert` and `Xunit` are in it**,
which are not domain types a stub should supply — a class the probe cannot see past.

### What a `using` actually buys — the measurement that changes the plan

`usings.py` gives each of the 368 *import* blocks one `using` per missing pinned name — Paramore,
then System, then Microsoft, then shortest, when a name lives in several namespaces — recompiles,
and repeats until nothing new is found (two rounds; 361 blocks, then 8).

| After the `using`s | Blocks |
|---|---:|
| **BUILT** | **46** of 368 |
| FAILED only on `CS0246`/`CS0103` for names **no pinned package ships** | **226** |
| FAILED on something else | 96 — `CS1061` 26, `CS0117` 22, `CS0535` 17, `CS1739` 12, `CS1503` 10, `CS7036` 7, `CS0104` 6 |

Corpus-wide the probe takes BUILT from **101 to 147**. So:

1. **A `using` alone repairs 46 blocks.** "Needs a `using`" and "is fixed by a `using`" are different
   claims, and 016's *import* row measured the first
2. **226 blocks need a `using` and a stub.** Add them to *context*'s 170 and roughly **400 blocks**
   are waiting on scaffold — four times the 102 the README starts from
3. **The 96, and the *other* row's 334, are where the page defects are.** `CS1061` (no such member),
   `CS0117` (no such static member), `CS1739` (no such parameter), `CS7036` (missing argument) are
   the shapes of 015's four compiler-found defects: API that moved. Those are the findings a reader
   is owed, not an obstacle between the gate and its number

**Caveat on the probe:** it prefers a namespace by a ranking a page author might not choose, and
`CS0104` (ambiguous name) appears 6 times after it. The 46 is a probe's figure, not a promise.

### The scaffold as it stands — one unit breaks its own rule

```bash
ls tools/blockcheck/scaffold/units | wc -l                     # 14
grep -vc '^#' tools/blockcheck/scaffold/pages.tsv              # 15 pages mapped
grep -E '^public (class|interface)' tools/blockcheck/scaffold/units/PageContext.cs | wc -l   # 9
```

`pages.tsv` states the rule: *"A UNIT SUPPLIES VALUES, TYPED FROM A PINNED PACKAGE OR THE BCL, AND
NOTHING ELSE — no domain type … every member of a unit is named by a block that builds with it."*
**`PageContext.cs` declares nine types** — `GreetingMade`, `AddGreeting`, `Person`, `Greeting`,
`CreateOrderCommand`, `OrderCreatedEvent`, `IOrderRepository`, `MyFeatureSwitchedConfigHandler`,
`SalutationPolicy` — and is mapped to one page, `DapperOutbox.md`, whose one baselined block
(`python3 tools/blockcheck.py --show contents/DapperOutbox.md 1`) names none of them. It was phase
1's red-proof unit, carried from 015's harness. `CreateOrderCommand` and `IOrderRepository` are the
very types AC13 ruled the four preludes deleted for. **Nothing checks the unit rule, so nothing
noticed.**

### The three instruments 016's AC walk found wrong — all three reproduce

| 016 criterion | Said | Measured 2026-09-26 |
|---|---|---|
| **AC3** | `ls tools/blockcheck/*.csproj` | **1** file. `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*'` → **2**; the glob misses `refs/refs.csproj`, the 71 pins |
| **AC7** | `python3 tools/blockcheck.py --list-skips` | exit **2**, *"unknown mode: nothing was checked"*. Never built; `--report` prints the 16 reasons |
| **AC10** | `grep -vc '^./spec/'` | BSD `grep -rn … .` prints `contents/…`, no `./`, so it excludes **nothing**. `^(\./)?spec/` works under both |

016 is closed and accepted; its documents are history. The correction is **the instrument** and
`tools/README.md`, not a rewrite of 016's criteria.

### Found in 016 and not repaired

```bash
grep -rn 'ITimerProvider' contents/          # InMemoryScheduler.md — the type does not exist
grep -rln '\[UsePolicy(' contents/           # the async-handler cases are among these
```

- `ITimerProvider` in `InMemoryScheduler.md`; the scheduler takes a `TimeProvider`
- `[UsePolicy(` on **async** handlers — `ReactorAndProactor.md`,
  `HowConfiguringTheCommandProcessorWorks.md`, `PolicyFallback.md`, `ImplementingExternalBus.md`.
  **Both legacy attributes are `[Obsolete]` at the 10.7.0 tag** — `UsePolicy` for
  `UseResiliencePipeline`, `UsePolicyAsync` for `UseResiliencePipelineAsync`
  (`git -C ../Brighter show 10.7.0:src/Paramore.Brighter/Policies/Attributes/UsePolicyAsyncAttribute.cs | grep Obsolete`).
  So the repair is `[UseResiliencePipelineAsync(`, not `[UsePolicyAsync(` — swapping one obsolete
  attribute for another would be a fix that is still wrong. A fifth page, `MigratingToPollyV8.md`,
  carries `[UsePolicy(` deliberately, as the V9 side of a migration, and is not in scope
  (`grep -rln '\[UsePolicy(' contents/` → 5 pages)
- `CQRSWithBrighterAndDarker.md` writes `Id = command.Id` against an `Order` it never shows

### Instrument quirks recorded by 016

- `--explain` prints a wrong summary on stderr — **reproduced**: *"989 blocks, 0 built, 989
  failing"* on a run that explained 872 ids
- The `statements` wrapper (`Holder.Run()`) supplies no `args`

## Target state

- **Every page this spec touches is whole**: each of its C# blocks is BUILT, SKIPPED with a reason
  the review accepts, or listed in `tasks.md` with the diagnostic that keeps it FAILED and why that
  is not this spec's to fix
- **The classification is a committed mode of the gate**, so the next spec starts from a command,
  not from a paragraph in this one
- **The scaffold's rule is checked by the tool**, not by a maintainer reading 14 files
- **Page defects the compiler finds are repaired**, everywhere the same falsehood recurs
- `tools/README.md` rows 2 and 9 carry the new figures, and 016's three instruments are corrected

## Target audience

- **The reader of a repaired page** — sees complete `using` directives and code that builds against
  the packages the page names. This is who the repairs are for
- **Whoever runs the gates** — `--classify`, `--list-skips` and the unit-rule check are for them
- **The next spec** — which inherits a committed classifier and a residual figure it can re-derive

## Source material

| Read | For |
|---|---|
| `spec/016-compile_gate/requirements.md`, `design.md`, `tasks.md` | the gate, its rules, the AC walk (tasks § *Phase 5 as executed*), the classification (§ *The scaffold, and the line AC13 will be read against*), the residual sentence |
| `tools/README.md` rows 2 and 9, § *Exit codes* | the figures and the 0/1/2 contract every new mode inherits |
| `tools/blockcheck.py`, `tools/blockcheck/Program.cs` | the modes, `--explain` (in `Program.cs` only, not exposed by the Python front end), the wrappers |
| `tools/blockcheck/scaffold/pages.tsv`, `units/*.cs` | the unit rule and the 14 units |
| `tools/blockcheck/refs/refs.csproj` | the pin — what can and cannot be judged |
| `tools/pagelint.py` rule 6 | the `using` rule, and `--changed` making it an error on touched blocks |
| `CLAUDE.md` § *Complete code blocks*, § *Compiling an example, and against what*, § *Version markers on code* | what a repaired block must look like, and why it is built against packages not `src/` |
| `.claude/commands/spec/review.md` § *Fixing what the review found* | fix the issue everywhere, rewrite, never annotate |
| `../Brighter/src/` at the 10.7.0 tag, `../Darker/src/` at 4.1.1 | **read-only**: what a moved API moved to |

## Scope

### P0 — the spec is not done without these

| # | Requirement |
|---|---|
| **P0-1** | **A committed classifier.** `python3 tools/blockcheck.py --classify [file]` prints one row per FAILED block — page, ordinal, class, the missing names — from a type dump of the **pinned** references, and a summary line of counts. It honours the exit contract (2 when the references are stale or unbuilt). The probe in `spec/017-compile_repairs/probe/` is its prototype, and its disagreements with 016 (same-page 21 vs 7) are **settled by reading a sample, not by picking the nicer number** |
| **P0-2** | **The unit rule is enforced.** A run fails (exit 1) when a unit declares a type, or declares a member no BUILT block on its mapped pages names. `PageContext.cs` is brought under the rule or ruled an exception by the maintainer, in writing — **not left as the one silent counter-example** |
| **P0-3** | **Stubs for types a page names and never shows**, per AC13's ruling. A stub carries **exactly the members the page's own blocks use**, typed as the page implies — never a member the page does not call, and never a type the page tells the reader to write (016 design rule 1 stands). This relaxes `pages.tsv`'s "no domain type" for this one class, and `pages.tsv`'s comment says so |
| **P0-4** | **`using` directives go on the page.** A block the compiler shows is missing an import gets the `using` a reader needs, in the block. The scaffold never supplies a namespace — that would make the gate green and leave the reader where they were |
| **P0-5** | **Repair by page, in tranches.** A tranche is a set of pages; each page leaves the tranche whole (Target state, first bullet). Tranche order is set at design from P0-1's output |
| **P0-6** | **Compiler-found page defects are repaired**, everywhere the same falsehood recurs (a grep per defect, recorded). A defect is a diagnostic that says the page's API is wrong — `CS1061`, `CS0117`, `CS1739`, `CS1503`, `CS7036`, `CS0535` and kin — checked against the 10.7.0 / 4.1.1 source before it is called one |
| **P0-7** | **The recorded falsehoods are repaired**: `ITimerProvider` → `TimeProvider` in `InMemoryScheduler.md`; **every handler attribute whose sync/async form does not match the method it decorates** — any of the 13 attributes Brighter ships in both forms, sync on `HandleAsync` or async on `Handle` — except where the page shows the mismatch deliberately as an error; the unshown `Order` in `CQRSWithBrighterAndDarker.md` |
| **P0-8** | **016's AC3, AC7 and AC10 instruments corrected.** AC7 by building `--list-skips` (one reason per line, terminated, exit 2 on an empty enumeration as every listing mode does); AC3 and AC10 by the corrected commands, recorded in `tools/README.md` where a later spec will find them |
| **P0-9** | **Baseline and figures move together.** Each PR that makes a block build adds its row to `baseline.tsv`; `tools/README.md` rows 2 and 9 carry the new figures, with the ref |
| **P0-10** | **Behavioural claims are run, with a control.** A repaired block asserting behaviour — an exception, an order, a precedence, whether an option does anything — is executed, and so is the case that should behave differently (`CLAUDE.md` § *Compiling an example*) |

### P1 — should, if the tranches leave room

| # | Requirement |
|---|---|
| **P1-1** | **The pin grows to judge the scheduler pages.** `Hangfire.AspNetCore`, a Jaeger exporter, Quartz's DI package (`IServiceCollectionQuartzConfigurator`). Measured before and after on its own, so its effect on BUILT/FAILED is not confused with a repair's |
| **P1-2** | **`--explain` exposed through `blockcheck.py`**, and its stderr summary corrected |
| **P1-3** | **The `statements` wrapper supplies `string[] args`**, if a tranche page needs it |

### P2 — later, recorded

| # | Requirement |
|---|---|
| **P2-1** | Pages beyond the tranches: the residual, re-derived by `--classify`, becomes 018's opening sentence |
| **P2-2** | The four Replay On Seen blocks enter through the next pin bump, not through this spec |

## Out of scope

- **Brighter or Darker source.** `../Brighter/samples/` writes are per-PR asks and none is planned
- **BrighterCommand/Brighter#4414** — `AutoFromAssemblies()` registers `FireSchedulerRequestHandler`
  twice at 10.7.0. Upstream's
- **Skipping to make a number.** A new `<!-- blockcheck: skip … -->` needs a reason the review
  accepts; "does not compile" is not one
- **The V9 skip rule.** 016 left *label vs API* open for the maintainer (tasks § *For the
  maintainer*). This spec adds no V9 skips and so does not need it settled
- **New pages, restructuring, retitling** — nothing moves in `SUMMARY.md`

## Deliverables

No deliverable is a page, so none takes a Diátaxis type. Pages the tranches repair keep the type
their banner already declares; a repair that finds a page mistyped records it and does not retype
it here.

| Deliverable | What |
|---|---|
| `tools/blockcheck.py` | `--classify`, `--list-skips`, and (P1-2) `--explain` |
| `tools/blockcheck/Program.cs` | the type dump behind `--classify`; the unit-rule check; the `--explain` summary fix |
| `tools/blockcheck/scaffold/units/*.cs`, `pages.tsv` | stubs and values for the tranche pages; `PageContext.cs` resolved |
| `tools/blockcheck/baseline.tsv` | one row per newly BUILT block |
| `tools/blockcheck/refs/refs.csproj` | P1-1 only |
| `contents/*.md` — the tranche pages | `using` directives, API repairs, recurrences |
| `tools/README.md` | rows 2 and 9; the corrected AC3/AC10 commands; `--classify` and `--list-skips` under § *The other modes* |
| `spec/017-compile_repairs/probe/` | the requirements probe, committed so every figure above re-derives |

## SUMMARY.md changes

**N/A** — see § *Subject*. No page is added, moved or retitled.

## Constraints

1. **Build against the released packages, never `src/`** (`CLAUDE.md` § *Compiling an example*)
2. **One phase is one PR** (`tools/README.md` § *One phase is one pull request*); a PR changing the
   published site needs the maintainer's sign-off, with head-ref deletion asked for by name
3. **`pagelint --changed` makes a missing `using` an error on every block a PR touches**, unless the
   block marks its omission with `// ...`, which downgrades it to a counted warning. A repair
   declares an omission only where the `using` genuinely cannot be written — AC8's second half
4. **Exit contract** — every new mode is 0/1/2 as `tools/README.md` § *Exit codes* states; 2 is never
   swallowed, and no verdict is read through a pipe
5. **No `--baseline <path>`** and no flag that points the gate at another list (016 constraint 6)
6. **Fix the issue, not the instance** — a falsehood found on one page is grepped for on all of them

## Acceptance criteria

Each verdict is read from a file with the exit code taken **first** (016's AC1/AC2/AC10 finding).

| # | Criterion | Decided by |
|---|---|---|
| **AC1** | BUILT reaches the target set at design (Q3), and `baseline.tsv` requires exactly the BUILT set | `python3 tools/blockcheck.py --report r.tsv > out; echo $?` → **0**; `0 findings`; the BUILT count in `out` against the target |
| **AC2** | No block moves BUILT → FAILED; no block is newly SKIPPED without an accepted reason | `awk -F'\t' 'NR==FNR{a[$2 FS $3]=$1;next} a[$2 FS $3]!=$1{print a[$2 FS $3]" -> "$1, $2, $3}' before.tsv after.tsv`, `before.tsv` being `--report` at `c7329bb`: every line printed is `FAILED -> BUILT`, or `-> SKIPPED` with its reason in the review record. **Control, run 2026-09-26:** a copy of the report with `Telemetry.md#1` flipped prints exactly `BUILT -> FAILED contents/Telemetry.md 1`. **The key is not stable across a split** — a fence split in two renumbers every later block on its page, as four did in 016 phase 4 — so each split is listed in `tasks.md` with its old and new ordinals, and the diff is read against that list |
| **AC3** | `--classify` exists, is deterministic, and agrees with the committed probe on the same tree | **No instrument until P0-1 ships** — `--classify` does not exist at requirements. Once it does: two runs `diff` empty; exit 0; its counts against `probe/run.sh` at the same ref, **each disagreement explained in `tasks.md`**. **Control:** with `refs.txt` removed it exits **2** |
| **AC4** | Every page in the tranches is whole | **No instrument until P0-1 ships.** Then: `--classify` restricted to the tranche pages lists **0** FAILED blocks not named in `tasks.md` with a reason |
| **AC5** | The unit rule is enforced | **No instrument until P0-2 ships** — no mode checks the rule today, which is how `PageContext.cs` went unnoticed. Once it does: a plant unit declaring a type → exit **1**; a plant member no block names → exit **1**; the real scaffold → exit **0**. `PageContext.cs` either passes or has the maintainer's written exception in `tasks.md` |
| **AC6** | Stubs carry only members their page's blocks use | **no instrument — checked by reading**, by the maintainer, against `--list-scaffold` for each stub unit, as AC13 was |
| **AC7** | The `using`s are on the page, not in the scaffold | A `using` in a unit is **file-scoped** and cannot reach a block; only a `global using` can. `grep -l '^global using' tools/blockcheck/scaffold/units/*.cs \| wc -l` → **0** (reads **0** at `c7329bb`); **and** `python3 tools/pagelint.py --changed origin/master > out; echo $?` → **0** on every tranche PR (reads **0** at `c7329bb`) |
| **AC8** | `pagelint`'s `using` debt falls by at least the blocks repaired, and no repair uses `// ...` where it could have written the `using` | `python3 tools/pagelint.py \| tail -1` against 743. **Second half: no instrument — checked by reading**, by the reviewer, over the diff's `// ...` lines |
| **AC9** | The recorded falsehoods are gone, everywhere | `grep -rn 'ITimerProvider' contents/ \| wc -l` → **0** (reads **4** at `c7329bb`); `python3 spec/017-compile_repairs/probe/attr_mismatch.py > out; echo $?` → **1** with exactly the deliberate `PipelineValidation.md` example in `out`, or **0** if that example carries an opt-out (reads **7** hits at `c7329bb`). **Red-proof:** `attr_mismatch.py --plant` exits **0** — a sync-on-async plant and an async-on-sync plant hit, a matched pair does not; `CQRSWithBrighterAndDarker.md`'s block builds or shows `Order`, read from AC1's report |
| **AC10** | Every page defect found (P0-6) is repaired at every recurrence | `tasks.md`'s defect ledger: one row per defect, the grep that found its recurrences, and that grep's count after → **0**. **The ledger is read by the acceptance walk** — it is the one place the recurrence greps live |
| **AC11** | Behavioural claims in repaired blocks were run, with a control | **no instrument — checked by reading**, by the reviewer, against the runs recorded in each PR |
| **AC12** | 016's instruments corrected | `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*' \| wc -l` → **2** (reads **2**; the old `ls` glob reads **1**); `grep -cF '^(\./)?spec/' tools/README.md` → **≥ 1** (reads **0** at `c7329bb`). **`--list-skips` has no instrument until P0-8 ships** (exits **2** today); then `--list-skips > s; echo $?` → **0** and `wc -l < s` = SKIPPED in AC1 |
| **AC13** | `tools/README.md` carries the gate's new figures and nowhere else does | `grep -rn '<BUILT figure> BUILT' --include='*.md' --include='*.yml' --include='*.py' . > f; grep -vcE '^(\./)?spec/' f` → **1**. **Control:** the same grep for a duplicated figure returns **> 1** |
| **AC14** | Every other gate is at `tools/README.md`'s figures at the close | the eight other gates run bare, each exit code read, against their rows |

**Three criteria have no instrument by design — AC6, AC8's second half and AC11 — and each names
who reads it. Three more have none yet, because the tool that decides them is a deliverable — AC3
and AC4 (P0-1), AC5 (P0-2), and AC12's `--list-skips` clause (P0-8).** Each becomes instrumented only when its tool is red-proofed; until then it is unmarked, and
both criteria ever found unmet at a close were unmarked ones.

## Open questions

| # | Question | Recommendation | Depends on |
|---:|---|---|---|
| **Q1** | **Where does the type dump live?** 501 assemblies, ~21k types | **Computed by `Program.cs` at run time**, beside the compile — the probe takes seconds, and a generated file is one more thing that goes stale. Fall back to a stamped file, as `refs.txt` is, only if CI time says so | P0-1; the `blocks` job's wall clock |
| **Q2** | **Does "page type" include `Program`, `Assert`, `Xunit`?** | **No.** `Program` is a top-level-statements artefact (a wrapper question); `Xunit`/`Assert` mean an unpinned test package. `--classify` gives each its own class rather than letting them inflate the stub count | reading the 113 |
| **Q3** | **What is the target?** | **Set at design, from `--classify`, as pages not blocks:** the number of pages taken whole, plus the BUILT figure that implies. Working assumption for design: **BUILT ≥ 250** and **pages with nothing BUILT 97 → ≤ 60**. The probe's 147 is the floor a `using` pass alone reaches | P0-1 |
| **Q4** | **Tranche order** | Pages whose FAILED blocks are all *import* or *context* first (cheapest, most likely to go whole); then pages ranked by FAILED count; the scheduler pages after P1-1 | P0-1's per-page output |
| **Q5** | **Does P0-3 need the maintainer again?** AC13 ruled the 102 "may be scaffolded as stubs"; P0-3 extends that to the ~226 import-then-context blocks, and states what a stub may contain | **Yes, at the requirements review** — one ruling on P0-3's wording covers both | the maintainer |
| **Q6** | **`PageContext.cs`** | Cut it to what `DapperOutbox.md#1` names (`connectionString`), and move the rest nowhere. It is the only unit that predates the rule | P0-2 |
| **Q7** | **Does the pin grow (P1-1)?** | **Yes, early**, in its own PR, measured alone. The two worst pages cannot be judged without it, and every later measurement is cleaner after it | the maintainer (pin changes alter what "released packages" means here) |
| **Q8** | **Phasing** | Phase 1 the instruments (P0-1, P0-2, P0-8, P1-2); phase 2 the pin (P1-1); phases 3+ one tranche each; last phase the close. Each phase one PR | Q3, Q4 |

## What the review found — 2026-09-26

Every named instrument was run at `c7329bb`; the nine gates all read at `tools/README.md`'s figures.
Six findings, all repaired in this document.

| # | Finding | Now |
|---:|---|---|
| **1** | **Five criteria named tools that do not exist yet as if they could be run** — AC3 and AC4 (`--classify`), AC5 (a unit-rule check), AC9's `UsePolicy` grep ("set at design"), AC12's `--list-skips` (exits 2) | each marked *no instrument until P0-n ships*, and counted in the paragraph under the table |
| **2** | **AC7's instrument measured nothing.** `grep -c '^using '` over the units counts file-scoped directives, which cannot reach a block | `^global using` → **0**, the only form that can |
| **3** | **AC2 named no command**, and its (page, ordinal) key renumbers when a fence is split | the `awk` diff, run with a control that prints the one flipped row; splits listed in `tasks.md` |
| **4** | **AC12's third clause was not a command** — `grep -vcE … in tools/README.md` | `grep -cF '^(\./)?spec/' tools/README.md` → ≥ 1; reads **0** today |
| **5** | **Constraint 3 overstated `pagelint --changed`**: said a touched block must carry its `using`s; measured, `// ...` downgrades it to a counted warning (`CLAUDE.md` § *Complete code blocks*) | stated with the exception, tied to AC8 |
| **6** | **The 21,365 had no command** | `wc -l < $TMPDIR/probe/types.tsv` |

## Maintainer rulings — 2026-09-26, at the design review

| Ruling | Effect here |
|---|---|
| **D1: P0-7 re-scoped.** Said: *"`[UsePolicy(` on async handlers"* on four pages; measured: all five occurrences decorate a sync `Handle`, so the clause targeted **0** blocks. Ruled: the survey 016 asked for — any paired attribute on a handler of the other kind | P0-7 and AC9 rewritten; `probe/attr_mismatch.py` is AC9's instrument, red-proofed |
| **D2: no Jaeger exporter in the pin**; D3: **the AWS V4 second pin is 018's** | P1-1 grows the pin without them (`design.md` § *Phases*) |

## Quality checklist

- [x] Readable with no prior context — the gate, the scaffold and the classification are each
      introduced before they are used
- [x] P0 / P1 / P2 distinguished, in three tables
- [x] Specific files named — `PageContext.cs`, `pages.tsv`, `InMemoryScheduler.md`, the four
      `[UsePolicy(` pages, the probe's three files
- [x] A command beside every number — the gate lines, the `awk`s, `probe/run.sh`, the `grep`s.
      **Exception:** 016's 27,921 has no command, because 016 recorded none; it is quoted as 016's
- [x] Every acceptance criterion names its instrument or says it has none, and who reads it
- [x] Every open question has a recommendation and a dependency
- [x] Subject declared; N/A sections named and marked
