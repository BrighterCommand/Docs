# Spec 016: A Committed Compile Gate — Design

**Created:** 2026-09-20
**Status:** **APPROVED 2026-09-20** — `.design-approved`. Reviewed 2026-09-20: The review re-ran the
committed probe from a clean directory and found **three defects in this document and its probe**:
a recipe that had never been executed (friction 56), a wall-clock quoted from the fastest of four
runs, and a `CS0101` claim that was false. All three are repaired in place, with what they said
recorded beside them.
**Requirements:** approved 2026-09-20, `.requirements-approved`, with **nine open questions left
open**. This design ran a probe that **settles four of them with measurements** — Q1, Q2, Q4 and
Q7 — and **Q6 was ruled *repair* by the maintainer at this review**. **Four remain open** (Q3, Q5,
Q8, Q9) **and none of them changes the plan.**

> **Every number here was measured by the probe in § *The probe*, not inherited.** The requirements'
> figures were re-derived rather than quoted, and § *Re-verifying the requirements* records the
> three that moved.

## Subject, and the sections it makes N/A

**Process**, as the requirements declared. Four sections are N/A, named rather than left empty:

| Section | Why N/A |
|---|---|
| **The outline, file by file** | This design creates **no page**. Its deliverables are a tool, a reference project, a baseline file, a row in `tools/README.md` and a CI job. The page-type / banner / opening-sentence rules have nothing to bind to |
| **SUMMARY.md changes** | A category error, not an empty section — `SUMMARY.md` carries pages |
| ~~**Target audience**~~ | **NOT N/A — Q6 ruled *repair*, 2026-09-20.** P0-1 to P0-8 are for whoever runs the gates; **P0-9's audience is the ordinary reader of a repaired page** |
| **Mode mix** | Nothing here decides what a page is for |

**No Brighter or Darker API is printed into any page by this design**, so the *verify the API* rule
applies instead to the two APIs the tool itself stands on, both resolved below: `pagelint.Page` and
Roslyn's `CSharpCompilation`.

## The probe — method, controls, and what it measured

**Run 2026-09-20 against Docs `master` `feea788`.** Everything below is in
`scratchpad/probe/`: a generator (`gen.py`), a package-union project (`probe.csproj`), and a
59-line Roslyn harness (`roslyn/Program.cs`). It was built to answer Q1, Q2 and Q4, **and it
answered a question nobody had asked.**

### What it does

1. **Emits all 985 C# blocks** through `pagelint.Page` — the shipped parser, per Constraint 2 —
   classifying each and wrapping it: `types` and `namespaced` as-is under `namespace B_<id>`,
   `members` inside a holder class, `statements` inside an `async Task` method.
2. **Gives each block no page context at all** — no scaffold, no prelude. The number it produces is
   therefore a **floor**, which is the honest direction for a decision about how big the job is.
3. **Compiles**, three ways: one MSBuild project holding everything, MSBuild one block at a time,
   and one `CSharpCompilation` per block in a single process.

### Its controls, and they are the reason any of this is quotable

| Control | Result |
|---|---|
| **Planted must-fail** — `NoSuchTypeXyz123 f;` | `FAIL`, `CS0246` |
| **Planted must-pass** — `public class C { public int F; }` | `CLEAN` |
| **Two methods, different code paths** — Roslyn in-process against MSBuild-one-block-at-a-time, over the same 59 sampled blocks | **59 of 59 agree, 0 disagree** |

The planted pair is two-way and **outside the enumeration** (obligation 3): neither is a block from
the corpus. The 59-block cross-check is obligation 1's *two methods that agree*, and it is what
licenses the headline figure.

### **The finding nobody was looking for — plausible zero number twelve**

**A compiler can report no errors for a file it did compile.** Measured three ways:

```text
BrighterSchedulerSupport_2 + HowServiceActivatorWorks_4, blocks that fail alone

  compiled alone          20 and 8 error lines
  batch of 2              20 and 8          both reported
  batch of 5               0 and 0          BOTH SILENT
```

Three *unrelated* files joined the batch and both blocks' errors vanished. Not dependency
resolution — the files are namespace-isolated and share nothing. Scaled up it is worse:

| Compilation | Blocks | What it reported |
|---|---:|---|
| whole corpus, one project | 985 | 1,444 errors, **194 files, and not one semantic error** — parse failures suppress binding everywhere |
| 792 parse-clean blocks, one project | 792 | 1,904 errors across 353 files; **446 blocks looked clean** |
| those same "clean" blocks, 20 at a time | 20 | **all 20 failed.** A 100% false-clean rate |
| one block at a time | 59 | 6 clean, 53 failing |

**446 was wrong by a factor of seven**, and every one of those 446 blocks would have entered a
baseline as *known good*. A gate built the obvious way — compile the corpus, read the error list —
**certifies most of its corpus without examining it.**

> **The general form, and it is friction 54:** *an instrument that batches its subjects shares a
> failure budget between them.* The per-subject verdict is only as trustworthy as the reporting
> channel's capacity, and nothing in the output says the channel is full. This is the twelfth
> plausible zero and the first where **the silent thing is a compiler**.

### What it measured

**Q1 — how many blocks build unaided: 60 of 985, 6.1%.** One `CSharpCompilation` per block, no
scaffold:

```text
985 blocks, 60 clean, 925 failing          6.5s total, 7ms per block

  by shape:   types       22 / 306
              statements  24 / 532
              members     10 / 136
              namespaced   4 /  11

  blocks affected:  CS0246  714   a type or namespace is not there
                    CS0103  653   a name is not in scope
                    CS1002  131   parse: ';' expected
                    CS0234   82   a namespace member is not there
```

**The two dominant codes are not API drift — they are missing imports and missing page context.**
`services`, `commandProcessor`, `producerRegistry`: identifiers a page names in prose and never
declares in the block. **249 blocks declare an omission with `// ...`, and only 22 of those are
clean.** So the signal this gate exists to find — a name that resolves to the wrong thing, which is
`CS1061` and `CS7036` — is buried under 900 blocks that fail for reasons a reader would forgive.
**That is the design's central problem, and § *The scaffold* is the answer to it.**

**Q2 — how many projects: one.** The union of `optioncheck`'s 62 pinned packages plus the
harness's five non-conflicting extras — **67 packages — restores in one project in 1.16s**, exit 0
read bare rather than through a pipe. `NU1107` is specifically the `Transformers.AWS` /
`.AWS.V4` pair and nothing else. Its build output plus the framework reference pack gives
**391 reference assemblies**.

**Q4 — is an unwrappable fragment common? No.** Every one of the 985 wrapped mechanically under
four rules; **not one needed a hand-chosen wrapper.** 131 blocks still fail to parse, but that is a
verdict about the block, not a failure of the wrapper.

**And a question nobody asked — the cost:**

| Method | Per block | Whole corpus | Usable? |
|---|---:|---:|---|
| MSBuild, whole corpus at once | — | **1.1s** | **No** — saturates, see above |
| MSBuild, one block at a time | **1.03s** | **~17 min** (59 × 1.03s, extrapolated) | Yes, and too slow for a gate |
| **Roslyn, one `CSharpCompilation` per block, one process** | **7–40ms** | **6.5s – 39.5s** | **Yes** |

> **The Roslyn figure is a range because four runs over identical inputs gave 6.5s, 22.9s, 39.5s and
> 26.2s** — same 985 blocks, same 391 reference assemblies, varying machine load. **The design's
> first draft quoted 6.5s as though it were the figure**, which is the fastest of four and the one
> that flatters the decision. Quote the range, and **budget on the slow end**.

**Between 26× and 150× faster than the only other honest method** — and the decision does not turn
on which end, because the alternative is seventeen minutes. It is also isolated *by construction*:
separate `Compilation` objects cannot share a diagnostic bag, so friction 54 cannot recur.

## Re-verifying the requirements — three deltas

Re-read against what the probe now knows, rather than quoted.

**1. One deliverable changed shape.** The requirements named `tools/blockcheck.py` as "the driver:
enumerate, classify, extract, build, report". **Compiling is not a job for a Python driver shelling
out to `dotnet build`** — that is the 17-minute method. But Constraint 2 forbids a second fence
parser, and `pagelint.Page` is Python. So the tool is **two processes with one pipeline**:
`tools/blockcheck.py` extracts through the shipped parser and stages the blocks;
`tools/blockcheck/` is a committed C# tool that compiles them with Roslyn. **Both constraints hold,
and neither would have if either half had been written alone.**

**2. One deliverable shrank, and one requirement dissolved.** Deliverable 2 read
`tools/blockcheck/*.csproj` **(count per Q2)** — it is **one**, measured. And **P0-3, "isolation so
188 clashing declarations coexist", is no longer a requirement to meet**: the wrapper namespaces
every block, so the 188 cross-page collisions the requirements sized **produced no error in any
probe run**, including the one holding all 985 blocks at once.

> **`CS0101` is not zero, and the first draft of this paragraph said it was.** Exactly **one** block
> raises it — `ImplementAQueryHandler.md` block 10 — and it is **inside one block**, not across two:
> the block declares `public sealed class GetOrderQueryHandler` **twice**, once
> `// Before (synchronous)` and once `// After (asynchronous)`. Isolation cannot help, because the
> collision is with itself. **Checked rather than asserted, and the assertion was wrong** — which is
> the third time in this spec that a claim written minutes earlier failed its own re-derivation.

**That one block is this gate's first documentation finding, and it generalises.** Six blocks
across four pages — `AgreementDispatcherRouting.md`, `CloudEventsSupport.md`,
`ImplementAQueryHandler.md`, `PolicyRetryAndCircuitBreaker.md` — carry a **before/after pair inside
a single fence**, and such a block cannot compile as shown. **`CLAUDE.md` § *Version markers on
code* already prescribes the repair**: two blocks, one marked ❌ and one ✅. So the verdict model
needs no new category — these are `FAILED` blocks whose repair is a convention this repository
already has, and they are **the first concrete candidates for Q6's repair pass**.

**3. One question stopped being about cost.** Q7 asked PR-wide or `--changed`, recommending
"whole baseline on every PR, add `--changed` when it hurts". **The whole 985-block corpus takes
between 6.5 and 39.5 seconds**, against the `options` job's measured 35s, so `--changed` buys
nothing and **P1-2 should be withdrawn** rather than deferred. The gate can *report* on every block
and *enforce* the baseline, every run.

## The design

```text
tools/
├── blockcheck.py                 extract + classify + stage + report   (Python, uses pagelint.Page)
├── blockcheck/
│   ├── blockcheck.csproj         the Roslyn tool             (Microsoft.CodeAnalysis.CSharp)
│   ├── Program.cs                one CSharpCompilation per block
│   ├── refs/refs.csproj          THE PIN — 67 packages, one place
│   ├── baseline.tsv              blocks required to stay CLEAN
│   └── scaffold/                 page context, one file per prelude
└── README.md                     row 9 — and the gate's number lives nowhere else
```

### The pipeline, and where each rule is enforced

| Step | Who | The rule it keeps |
|---|---|---|
| Enumerate C# blocks | `blockcheck.py`, via `pagelint.Page` | Constraint 2 — one parser. The 150 irregular fences are seen because `FENCE_RE` sees them |
| Classify and wrap | `blockcheck.py` — four rules, no hand choice | Q4: measured to cover 985 of 985 |
| Attach scaffold | `blockcheck.py`, per § *The scaffold* | AC8 — every supplied identifier is listed |
| Compile | `tools/blockcheck`, one `Compilation` per block | Friction 54 cannot recur by construction |
| Verdict and exit | `blockcheck.py` | Constraint 4's 0/1/2, Constraint 11's no-pipe rule |

### The four wrapper rules

Applied in order; each block matches exactly one. **Measured over all 985 — every block wrapped.**

| Shape | Test | Wrapper | Count |
|---|---|---|---:|
| `namespaced` | declares its own `namespace` | none — emitted verbatim | 11 |
| `types` | declares a `class`/`record`/`interface`/`struct`/`enum` | `using`s hoisted, rest inside `namespace B_<id>` | 306 |
| `members` | a line opens with an access or modifier keyword | the above, plus a holder class | 136 |
| `statements` | anything else | the above, plus `public async Task Run()` | 532 |

**The wrapper is declared, never invented.** `<id>` is `<page>_<n>`, so a verdict names the page and
the block's ordinal — which is also how a reader is told where to look.

### The scaffold — the part that decides whether this gate is worth having

**6.1% of blocks compile unaided, and the other 93.9% mostly fail on `CS0246` and `CS0103`.** A gate
that reports 925 failures reports nothing: the four defects 015 found by compiling would sit
somewhere in that list, indistinguishable from a block that simply does not declare `services`.

So a block enters the baseline **with a declared scaffold**: a named prelude supplying the
identifiers the page names but the block does not define. This is 015 phase 4's method — its
`Scaffold.cs` and four preludes are already in `harness/` — and it is **hand-work, one page at a
time**. It is also the whole argument for Q1's ratcheting baseline: at 7ms a block the *machine*
cost is nothing, and the *human* cost is the entire project.

**Three rules keep the scaffold honest**, and they are what AC13 and AC14 ask a reader to check:

1. **It supplies identifiers, never behaviour.** A prelude may declare `IServiceCollection services`;
   it may not define a type the page tells the reader to write.
2. **It is listed by every run** (AC8), so a `CLEAN` verdict states what it was given.
3. **It is per page, not per block** — a page's blocks share a world, which is what makes the unit of
   admission a page and the cost tractable.

### The verdict model

| Verdict | Means | Affects exit code? |
|---|---|---|
| `CLEAN` | compiled with zero errors | only if a baselined block stops being `CLEAN` |
| `FAILED` | compiled, errors reported | **yes, if baselined** |
| `SKIPPED` | a visible opt-out with a reason — the 8 ❌ V9 blocks and any other | no, **and always counted** |
| `NOT COMPILABLE` | no wrapper rule applies | no, **and always counted**. Currently **0 of 985** |

`0 findings` and `0 findings, 8 skipped` are different claims (Constraint 5, ruling 4), and the run
prints the second form whenever a skip exists.

### The baseline and its ratchet

`tools/blockcheck/baseline.tsv` — one row per block required to stay `CLEAN`: page, ordinal, the
scaffold it was admitted with, and the ref it was measured at. **Enforced both ways** (AC9): a
baselined block that stops compiling is exit 1, **and a baselined row naming a block that no longer
exists is exit 1** — otherwise a page deletion silently shrinks the gate's corpus, which is the
`tools/README.md` failure in another shape.

**No `--baseline <path>`** (Constraint 6): a gate that can be pointed at another list can be
silenced.

### The CI job

Modelled on `options`, whose reasoning `docs.yml` already argues. A fourth job: `setup-dotnet`,
restore the pinned `refs` project, run. **No guard, no `|| true`, no `schedule:`** — the packages
are pinned, so nothing outside this repository can change the verdict, and `versioncheck` already
owns the "a release happened" signal. Measured precedent: the `options` job costs **35s**; this one
adds a corpus pass of **6.5s to 39.5s** to a similar restore, so budget it as roughly doubling that
job rather than as free.

### The two APIs this design stands on, resolved

```bash
grep -n 'FENCE_RE = \|^class Page\|^def load_pages' tools/pagelint.py
#   192:FENCE_RE = re.compile(r'^ {0,3}(`{3,}|~{3,})[ \t]*(\S*)')
#   252:class Page:
#   328:def load_pages():
```

**Live.** And `CSharpCompilation.Create` / `GetDiagnostics` — resolved by use: the probe's
`roslyn/Program.cs` builds and runs against `Microsoft.CodeAnalysis.CSharp` **4.11.0**.

## Predicted gate movement — **none, for phases 1 to 3, and here is the mechanism**

`tools/README.md` has the eight and the corpus each walks. Cite it for the numbers.

| Gate | Prediction | Why, from mechanism |
|---|---|---|
| `linkcheck` | **none** | It walks `.md` files, and `tools/` **is** inside that walk — it moved 164 → 165 when `tools/README.md` was written. This design **adds no `.md` under `tools/`** on purpose: the gate's documentation is the new row in the existing file. Adding `tools/blockcheck/README.md` would move it to 166, and that is a choice, not an accident |
| `pagelint` | **none** in phases 1–3; **DOWN in the P0-9 phase** | Its corpus is `contents/` + `README.md`, so a `.py`, a `.cs` and a `.csproj` cannot enter it. **Q6 was ruled *repair* on 2026-09-20, so this prediction is now makeable and it is movement, not "none":** every block admitted to the baseline gains the `using` directives it needs to compile, and rule 6's warning count falls by one per block so admitted — **exactly the mechanism that took it 757 → 744 in 015 phase 4, thirteen blocks, the only predicted movement that row has ever had.** The phase states its own before/after; `tools/README.md` row 2 owns the number |
| shape, redirects, `--verify` | **none** | All three read `SUMMARY.md` and `.gitbook.yaml`; neither is touched, and no page moves |
| `versioncheck` | **none** | It reads version pins **in page prose**, 18 across 5 pages. The 67 pins land in a `.csproj`, exactly as `optioncheck`'s 63 already do, and it has never read those |
| `symbolcheck` | **none** | Corpus is `contents/`. Unchanged unless Q6 rules *repair* |
| `optioncheck` | **none** | It reads option tables in `contents/` against reflected types |

**"None" is the prediction, and it is the case worth writing down**: seven of the eight are vacuous
passes here, and a vacuous pass is invisible exactly when no movement was expected.

## What the probe settled, and what it did not

| | Question | Status |
|---|---|---|
| **Q1** | the green bar | **Measured: 6.1% build unaided.** A whole-corpus bar means admitting ~925 blocks by hand. The recommendation — a ratcheting baseline — now rests on a number rather than an intuition. **Still the maintainer's to rule** |
| **Q2** | how many projects | **Settled: one.** 67 packages, one restore, 1.16s |
| **Q4** | unwrappable fragments | **Settled: none.** Four rules cover 985 of 985; `NOT COMPILABLE` stays in the model as a verdict that currently has no members |
| **Q7** | PR-wide or `--changed` | **Dissolved.** 6.5–39.5s for the whole corpus, against the `options` job's 35s. **P1-2 withdrawn** |
| **Q3** | Darker | **Open.** 6 `Paramore.Darker*` strings, two of them namespaces. Cheap to add to the one reference set; untested |
| **Q5** | opt-out spelling | **Open.** `<!-- blockcheck: skip <reason> -->` recommended |
| **Q6** | repair, or hand over a list | **RULED 2026-09-20: REPAIR** — *"It should fix documentation issues."* P1-1 became **P0-9**, *Target audience* stopped being N/A, obligation 7 now binds on the repair PR, and `pagelint`'s prediction became **down**. **The boundary is the design's to propose and is stated in P0-9: defects of claim, not the ~900 blocks failing for want of imports and page context** |
| **Q8** | a `schedule:` trigger | **Open; no** recommended, on `docs.yml`'s own argument for `optioncheck` |
| **Q9** | pinned or latest | **Open; pinned** recommended |

## Workflow friction

**54 — an instrument that batches its subjects shares a failure budget between them, and nothing in
the output says the budget is spent.** Two blocks reporting 20 and 8 errors in a batch of two
reported **zero** when three unrelated files joined them; at corpus scale a single compilation
certified 446 blocks it had not examined, and a 20-block sample of those was **100% false-clean**.
**Plausible zero number twelve, and the first whose silent instrument is a compiler.** The remedy is
architectural rather than procedural: one `Compilation` per block cannot share a diagnostic bag.

**56 — committing a probe is not the same as committing the run.** The probe README's first
recipe was three commands that had never been executed in that form: it said to take the reference
assemblies from `probe.csproj`'s output, and **that project compiles the blocks, so its build fails
and leaves `bin/` empty**. Anyone following it would have run the probe against **zero** references
and watched it call all 985 blocks broken — a *believable* result, in the direction that confirms
the design's thesis. The actual run used a references-only project that was never committed. Found
at the design review by **running the committed copy from a clean directory**, which is the only
check that distinguishes *committed* from *reproducible*. The remedy is the same shape as friction
48 and the two defects the requirements review found: **the form that gets run must be the form
that was written down**, and the way you find out is to run the written-down one.

**55 — an enumeration written without a trailing newline loses its last member to `while read`.**
The probe sampled 60 blocks and measured **59**; `'\n'.join(pick)` left no final newline, and the
loop dropped `MigratingToNullableReferenceTypes_22` silently. It changed no conclusion here, and it
is recorded because **this programme's instruments are enumerations**: the same slip in a baseline
file drops the last block from the gate's corpus, and the gate still says `0 findings`.

## Design quality checklist, applied to this document

| | |
|---|---|
| Self-contained | the probe's method, controls and outputs are here, not cited from elsewhere |
| Page types, banners, headings | **N/A — no page is created**, named in § *Subject* rather than skipped |
| Every API resolved, with the command | `pagelint.Page` and `FENCE_RE` by `grep -n`; Roslyn by building and running against it |
| Every number carries its command or its measurement | all from § *The probe*; the two gate figures are cited from `tools/README.md`, never pasted |
| Gate movement predicted, including "none" | eight rows, each argued from the corpus that gate walks, with the one Q6-dependent exception named |
| Structure obvious at a glance | the tree, the pipeline table, the four wrapper rules, the verdict model |
| Writable by someone who was not in the room | the wrapper rules are stated with their tests and counts; the scaffold has three rules; the baseline has its columns |

## What the repair phase will actually repair — P0-9's opening list

**Q6 is ruled, so this list is the plan rather than a possibility.** It is small on purpose: these
are defects of *claim*, each one a thing the page asserts that is not true.

| Defect | Where | Found by |
|---|---|---|
| **A block declaring the same type twice** — `// Before (synchronous)` and `// After (asynchronous)` in one fence, so it cannot compile as shown | `ImplementAQueryHandler.md` block 10, and the same shape on `AgreementDispatcherRouting.md`, `CloudEventsSupport.md`, `PolicyRetryAndCircuitBreaker.md` — **6 blocks, 4 pages** | the probe's only `CS0101` |
| **Whatever the baseline pass turns up in the `CS1061` / `CS7036` class** | unknown until P0-6 runs block by block | the compiler — 015's four, and the only class no gate here can see |

**The repair for the first is a convention this repository already has**: `CLAUDE.md`
§ *Version markers on code* says show both forms in **two** blocks, one ❌ and one ✅. So P0-9's
first act is not an invention, and the six blocks are a worked example for the rest.

**131 blocks fail to parse**, and those are a third category to triage in P0-6 rather than assume:
some will be this same shape, some will be genuine fragments, and the count is not yet split.

---

**Next step: `/spec:review`.** **Q6 is ruled; four questions remain open** — Q3 (Darker), Q5 (the
opt-out spelling), Q8 (no `schedule:`) and Q9 (pinned at `10.7.0`) — **and none of them changes the
plan.** What does want confirming is **P0-9's boundary**, which the ruling did not set.
