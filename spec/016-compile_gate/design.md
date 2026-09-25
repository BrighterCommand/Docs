# Spec 016: A Committed Compile Gate — Design

**Created:** 2026-09-20
**Status:** **APPROVED 2026-09-20** — `.design-approved`. The review re-ran the committed probe from
a clean directory and found three defects in this document and its probe, each repaired in place:
a recipe that had never been executed, now the run that was used (friction 56); a wall-clock quoted
from the fastest of four runs, now the range; and a `CS0101` claim that was false, now the one block
that raises it.
**Requirements:** approved 2026-09-20, `.requirements-approved`, with **nine open questions**. The
probe settles four with measurements — Q1, Q2, Q4 and Q7 — and **Q6 was ruled *repair* by the
maintainer at this review**. **Four remain open** (Q3, Q5, Q8, Q9), and none changes the plan.

> Every number here was measured by the probe in § *The probe*. § *Re-verifying the requirements*
> records the three requirements figures that moved.

## Subject, and the sections it makes N/A

**Process**, as the requirements declared. Four sections are N/A:

| Section | Why N/A |
|---|---|
| **The outline, file by file** | This design creates **no page**. Its deliverables are a tool, a reference project, a baseline file, a row in `tools/README.md` and a CI job |
| **SUMMARY.md changes** | `SUMMARY.md` carries pages, and there are none |
| ~~**Target audience**~~ | **NOT N/A — Q6 ruled *repair*, 2026-09-20.** P0-1 to P0-8 are for whoever runs the gates; **P0-9's audience is the ordinary reader of a repaired page** |
| **Mode mix** | Nothing here decides what a page is for |

No Brighter or Darker API is printed into any page by this design. The two APIs the tool stands on,
`pagelint.Page` and Roslyn's `CSharpCompilation`, are resolved below.

## The probe — method, controls, and what it measured

**Run 2026-09-20 against Docs `master` `feea788`.** It is in `scratchpad/probe/`: a generator
(`gen.py`), a package-union project (`probe.csproj`), and a 59-line Roslyn harness
(`roslyn/Program.cs`). It was built to answer Q1, Q2 and Q4.

### What it does

1. **Emits all 985 C# blocks** through `pagelint.Page` — the shipped parser, per Constraint 2 —
   classifying each and wrapping it: `types` and `namespaced` as-is under `namespace B_<id>`,
   `members` inside a holder class, `statements` inside an `async Task` method.
2. **Gives each block no page context** — no scaffold, no prelude — so the number it produces is a
   **floor**.
3. **Compiles** three ways: one MSBuild project holding everything, MSBuild one block at a time,
   and one `CSharpCompilation` per block in a single process.

### Its controls, and they are the reason any of this is quotable

| Control | Result |
|---|---|
| **Planted must-fail** — `NoSuchTypeXyz123 f;` | `FAIL`, `CS0246` |
| **Planted must-pass** — `public class C { public int F; }` | `CLEAN` |
| **Two methods, different code paths** — Roslyn in-process against MSBuild-one-block-at-a-time, over the same 59 sampled blocks | **59 of 59 agree, 0 disagree** |

The planted pair is two-way and outside the enumeration (obligation 3). The 59-block cross-check is
obligation 1's two methods that agree.

### **The finding nobody was looking for — plausible zero number twelve**

**A compiler can report no errors for a file it did compile.** Measured three ways:

```text
BrighterSchedulerSupport_2 + HowServiceActivatorWorks_4, blocks that fail alone

  compiled alone          20 and 8 error lines
  batch of 2              20 and 8          both reported
  batch of 5               0 and 0          BOTH SILENT
```

Three unrelated, namespace-isolated files joined the batch and both blocks' errors vanished. At
scale:

| Compilation | Blocks | What it reported |
|---|---:|---|
| whole corpus, one project | 985 | 1,444 errors, **194 files, and not one semantic error** — parse failures suppress binding everywhere |
| 792 parse-clean blocks, one project | 792 | 1,904 errors across 353 files; **446 blocks looked clean** |
| those same "clean" blocks, 20 at a time | 20 | **all 20 failed.** A 100% false-clean rate |
| one block at a time | 59 | 6 clean, 53 failing |

**446 was wrong by a factor of seven.** A gate that compiles the corpus in one batch and reads the
error list certifies most of its corpus without examining it.

> **Friction 54:** an instrument that batches its subjects shares a failure budget between them,
> and nothing in the output says the budget is spent. The twelfth plausible zero, and the first
> whose silent instrument is a compiler.

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

**The two dominant codes are missing imports and missing page context, not API drift**:
`services`, `commandProcessor`, `producerRegistry` — identifiers a page names in prose and never
declares in the block. **249 blocks declare an omission with `// ...`; 22 of those are clean.** The
signal this gate exists to find — a name that resolves to the wrong thing, `CS1061` and `CS7036` —
is buried under 900 blocks that fail for reasons a reader would forgive. § *The scaffold* addresses
that.

**Q2 — how many projects: one.** The union of `optioncheck`'s 62 pinned packages plus the
harness's five non-conflicting extras — **67 packages — restores in one project in 1.16s**, exit 0
read bare. `NU1107` is the `Transformers.AWS` / `.AWS.V4` pair only. Its build output plus the
framework reference pack gives **391 reference assemblies**.

**Q4 — is an unwrappable fragment common? No.** All 985 wrapped mechanically under four rules; none
needed a hand-chosen wrapper. 131 blocks still fail to parse, which is a verdict about the block,
not the wrapper.

**The cost:**

| Method | Per block | Whole corpus | Usable? |
|---|---:|---:|---|
| MSBuild, whole corpus at once | — | **1.1s** | **No** — saturates, see above |
| MSBuild, one block at a time | **1.03s** | **~17 min** (59 × 1.03s, extrapolated) | Yes, and too slow for a gate |
| **Roslyn, one `CSharpCompilation` per block, one process** | **7–40ms** | **6.5s – 39.5s** | **Yes** |

> **The Roslyn figure is a range: four runs over identical inputs gave 6.5s, 22.9s, 39.5s and
> 26.2s** — same 985 blocks, same 391 reference assemblies, varying machine load. Budget on the slow
> end.

**Between 26× and 150× faster than MSBuild one block at a time**, the only other isolated method,
and the alternative is seventeen minutes. Separate `Compilation` objects cannot share a diagnostic
bag, so friction 54 cannot recur.

## Re-verifying the requirements — three deltas

**1. One deliverable changed shape.** The requirements named `tools/blockcheck.py` as "the driver:
enumerate, classify, extract, build, report". Shelling out to `dotnet build` is the 17-minute
method, and Constraint 2 forbids a second fence parser while `pagelint.Page` is Python. So the tool
is **two processes with one pipeline**: `tools/blockcheck.py` extracts through the shipped parser
and stages the blocks; `tools/blockcheck/` is a committed C# tool that compiles them with Roslyn.
Both constraints hold.

**2. One deliverable shrank, and one requirement dissolved.** Deliverable 2 read
`tools/blockcheck/*.csproj` **(count per Q2)** — it is **one**, measured. **P0-3, "isolation so 188
clashing declarations coexist", is no longer a requirement to meet**: the wrapper namespaces every
block, and the 188 cross-page collisions **produced no error in any probe run**, including the one
holding all 985 blocks at once.

> **`CS0101` is not zero: exactly one block raises it** — `ImplementAQueryHandler.md` block 10 —
> and the collision is inside that block. It declares `public sealed class GetOrderQueryHandler`
> **twice**, once `// Before (synchronous)` and once `// After (asynchronous)`, so isolation cannot
> help.

**That one block is this gate's first documentation finding.** Six blocks across four pages —
`AgreementDispatcherRouting.md`, `CloudEventsSupport.md`, `ImplementAQueryHandler.md`,
`PolicyRetryAndCircuitBreaker.md` — carry a **before/after pair inside a single fence**, and such a
block cannot compile as shown. **`CLAUDE.md` § *Version markers on code* already prescribes the
repair**: two blocks, one marked ❌ and one ✅. The verdict model needs no new category: these are
`FAILED` blocks and **the first concrete candidates for Q6's repair pass**.

**3. One question stopped being about cost.** Q7 asked PR-wide or `--changed`, recommending
"whole baseline on every PR, add `--changed` when it hurts". **The whole 985-block corpus takes
between 6.5 and 39.5 seconds**, against the `options` job's measured 35s, so `--changed` buys
nothing and **P1-2 is withdrawn**. The gate reports on every block and enforces the baseline, every
run.

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
the block's ordinal.

### The scaffold — the part that decides whether this gate is worth having

**6.1% of blocks compile unaided, and the other 93.9% mostly fail on `CS0246` and `CS0103`.** A gate
reporting 925 failures cannot distinguish the four defects 015 found by compiling from a block that
does not declare `services`.

So a block enters the baseline **with a declared scaffold**: a named prelude supplying the
identifiers the page names but the block does not define. This is 015 phase 4's method — its
`Scaffold.cs` and four preludes are in `harness/` — and it is **hand-work, one page at a time**. At
7ms a block the machine cost is nothing; the human cost is the project, which is why Q1's baseline
ratchets.

**Three rules keep the scaffold honest**; AC13 and AC14 ask a reader to check them:

1. **It supplies identifiers, never behaviour.** A prelude may declare `IServiceCollection services`;
   it may not define a type the page tells the reader to write.
2. **It is listed by every run** (AC8), so a `CLEAN` verdict states what it was given.
3. **It is per page, not per block** — a page's blocks share a world, so the unit of admission is a
   page.

### The verdict model

| Verdict | Means | Affects exit code? |
|---|---|---|
| `CLEAN` | compiled with zero errors | only if a baselined block stops being `CLEAN` |
| `FAILED` | compiled, errors reported | **yes, if baselined** |
| `SKIPPED` | a visible opt-out with a reason — the 8 ❌ V9 blocks and any other | no, **and always counted** |
| `NOT COMPILABLE` | no wrapper rule applies | no, **and always counted**. Currently **0 of 985** |

`0 findings` and `0 findings, 8 skipped` are different claims (Constraint 5, ruling 4); the run
prints the second form whenever a skip exists.

### The baseline and its ratchet

`tools/blockcheck/baseline.tsv` — one row per block required to stay `CLEAN`: page, ordinal, the
scaffold it was admitted with, and the ref it was measured at. **Enforced both ways** (AC9): a
baselined block that stops compiling is exit 1, **and a baselined row naming a block that no longer
exists is exit 1**, so a page deletion cannot silently shrink the gate's corpus.

**No `--baseline <path>`** (Constraint 6): a gate that can be pointed at another list can be
silenced.

### The CI job

Modelled on `options`. A fourth job: `setup-dotnet`, restore the pinned `refs` project, run.
**No guard, no `|| true`, no `schedule:`** — the packages are pinned, so nothing outside this
repository can change the verdict, and `versioncheck` owns the "a release happened" signal. The
`options` job costs **35s**; this one adds a corpus pass of **6.5s to 39.5s** to a similar restore,
roughly doubling that job.

### The two APIs this design stands on, resolved

```bash
grep -n 'FENCE_RE = \|^class Page\|^def load_pages' tools/pagelint.py
#   192:FENCE_RE = re.compile(r'^ {0,3}(`{3,}|~{3,})[ \t]*(\S*)')
#   252:class Page:
#   328:def load_pages():
```

**Live.** `CSharpCompilation.Create` / `GetDiagnostics` are resolved by use: the probe's
`roslyn/Program.cs` builds and runs against `Microsoft.CodeAnalysis.CSharp` **4.11.0**.

## Predicted gate movement — **none, for phases 1 to 3, and here is the mechanism**

`tools/README.md` has the eight and the corpus each walks, and owns the numbers.

| Gate | Prediction | Why, from mechanism |
|---|---|---|
| `linkcheck` | **none** | It walks `.md` files, including `tools/` — it moved 164 → 165 when `tools/README.md` was written. This design adds no `.md` under `tools/`; the gate's documentation is a new row in the existing file. A `tools/blockcheck/README.md` would move it to 166 |
| `pagelint` | **none** in phases 1–3; **DOWN in the P0-9 phase** | Its corpus is `contents/` + `README.md`, so a `.py`, a `.cs` and a `.csproj` cannot enter it. With Q6 ruled *repair*, every block admitted to the baseline gains the `using` directives it needs, and rule 6's warning count falls by one per block — the mechanism that took it 757 → 744 in 015 phase 4, thirteen blocks. The phase states its own before/after; `tools/README.md` row 2 owns the number |
| shape, redirects, `--verify` | **none** | All three read `SUMMARY.md` and `.gitbook.yaml`; neither is touched, and no page moves |
| `versioncheck` | **none** | It reads version pins **in page prose**, 18 across 5 pages. The 67 pins land in a `.csproj`, as `optioncheck`'s 63 already do, and it does not read those |
| `symbolcheck` | **none** | Corpus is `contents/`. Unchanged unless Q6 rules *repair* |
| `optioncheck` | **none** | It reads option tables in `contents/` against reflected types |

Seven of the eight are vacuous passes, so each before-figure is read at the start of a phase.

## What the probe settled, and what it did not

| | Question | Status |
|---|---|---|
| **Q1** | the green bar | **Measured: 6.1% build unaided.** A whole-corpus bar means admitting ~925 blocks by hand, so a ratcheting baseline is recommended. **Still the maintainer's to rule** |
| **Q2** | how many projects | **Settled: one.** 67 packages, one restore, 1.16s |
| **Q4** | unwrappable fragments | **Settled: none.** Four rules cover 985 of 985; `NOT COMPILABLE` stays in the model as a verdict that currently has no members |
| **Q7** | PR-wide or `--changed` | **Dissolved.** 6.5–39.5s for the whole corpus, against the `options` job's 35s. **P1-2 withdrawn** |
| **Q3** | Darker | **Open.** 6 `Paramore.Darker*` strings, two of them namespaces. Cheap to add to the one reference set; untested |
| **Q5** | opt-out spelling | **Open.** `<!-- blockcheck: skip <reason> -->` recommended |
| **Q6** | repair, or hand over a list | **RULED 2026-09-20: REPAIR** — *"It should fix documentation issues."* P1-1 became **P0-9**, *Target audience* stopped being N/A, obligation 7 binds on the repair PR, and `pagelint`'s prediction became **down**. **The boundary, stated in P0-9: defects of claim, not the ~900 blocks failing for want of imports and page context** |
| **Q8** | a `schedule:` trigger | **Open; no** recommended, on `docs.yml`'s own argument for `optioncheck` |
| **Q9** | pinned or latest | **Open; pinned** recommended |

## Workflow friction

**54 — an instrument that batches its subjects shares a failure budget between them, and nothing in
the output says the budget is spent.** Two blocks reporting 20 and 8 errors in a batch of two
reported **zero** when three unrelated files joined them; at corpus scale a single compilation
certified 446 blocks it had not examined, and a 20-block sample of those was **100% false-clean**.
Plausible zero number twelve, and the first whose silent instrument is a compiler. The remedy is
architectural: one `Compilation` per block cannot share a diagnostic bag.

**56 — committing a probe is not the same as committing the run.** The probe README's first
recipe had never been executed: it said to take the reference assemblies from `probe.csproj`'s
output, and **that project compiles the blocks, so its build fails and leaves `bin/` empty**.
Following it runs the probe against **zero** references and reports all 985 blocks broken. The
actual run used a references-only project that was never committed. Found at the design review by
**running the committed copy from a clean directory**. Same shape as friction 48 and the two
defects the requirements review found: **the form that gets run must be the form that was written
down**.

**55 — an enumeration written without a trailing newline loses its last member to `while read`.**
The probe sampled 60 blocks and measured **59**; `'\n'.join(pick)` left no final newline, and the
loop dropped `MigratingToNullableReferenceTypes_22` silently. It changed no conclusion here. The same
slip in a baseline file would drop the last block from the gate's corpus while the gate still says
`0 findings`.

## Design quality checklist, applied to this document

| | |
|---|---|
| Self-contained | the probe's method, controls and outputs are here, not cited from elsewhere |
| Page types, banners, headings | **N/A — no page is created**, named in § *Subject* |
| Every API resolved, with the command | `pagelint.Page` and `FENCE_RE` by `grep -n`; Roslyn by building and running against it |
| Every number carries its command or its measurement | all from § *The probe*; the two gate figures are cited from `tools/README.md`, never pasted |
| Gate movement predicted, including "none" | eight rows, each from the corpus that gate walks, with the one Q6-dependent exception named |
| Structure obvious at a glance | the tree, the pipeline table, the four wrapper rules, the verdict model |
| Writable by someone who was not in the room | the wrapper rules are stated with their tests and counts; the scaffold has three rules; the baseline has its columns |

## What the repair phase will actually repair — P0-9's opening list

These are defects of *claim*, each a thing the page asserts that is not true.

| Defect | Where | Found by |
|---|---|---|
| **A block declaring the same type twice** — `// Before (synchronous)` and `// After (asynchronous)` in one fence, so it cannot compile as shown | `ImplementAQueryHandler.md` block 10, and the same shape on `AgreementDispatcherRouting.md`, `CloudEventsSupport.md`, `PolicyRetryAndCircuitBreaker.md` — **6 blocks, 4 pages** | the probe's only `CS0101` |
| **Whatever the baseline pass turns up in the `CS1061` / `CS7036` class** | unknown until P0-6 runs block by block | the compiler — 015's four, and the only class no gate here can see |

**The repair for the first is `CLAUDE.md` § *Version markers on code***: both forms in **two**
blocks, one ❌ and one ✅.

**131 blocks fail to parse**, a third category P0-6 triages: some will be this same shape, some
genuine fragments, and the count is not yet split.

---

**Next step: `/spec:review`.** **Q6 is ruled; four questions remain open** — Q3 (Darker), Q5 (the
opt-out spelling), Q8 (no `schedule:`) and Q9 (pinned at `10.7.0`) — and none changes the plan.
**P0-9's boundary** wants confirming, since the ruling did not set it.
