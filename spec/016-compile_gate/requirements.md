# Spec 016: A Committed Compile Gate — Requirements

**Created:** 2026-09-20
**Status:** **APPROVED 2026-09-20** — `.requirements-approved`. The review found three defects in
this document's acceptance criteria by running their instruments; all three are repaired
(§ *What the review found in this document*).

> **The nine open questions were approved open**, so each recommendation is the design's working
> assumption, stated as an assumption wherever it is relied on. **At the design review on
> 2026-09-20, Q1, Q2, Q4 and Q7 were settled by the design's probe, and Q6 was ruled *repair* by
> the maintainer**: P1-1 became **P0-9**, *Target audience* stopped being N/A, and obligation 7
> binds. **Q3, Q5, Q8 and Q9 remain open**; none changes what gets built.

> Every number here carries the command that produced it, measured 2026-09-20 against Docs
> `master` `d412702`. Gate figures are **cited from `tools/README.md`, not pasted** — obligation 10.

> **The README's *985 C# blocks* came from one method. A second returns 835**; the 150-block gap is
> fence spelling (§ *Current state*, first table). 985 stands. Friction **53**.

## Subject

**Process.** The deliverable is an instrument and a gate — a ninth row in `tools/README.md` — plus
the method for extracting a C# block from a page and building it against the packages a reader
would install. **It creates no page.**

Two sections are **N/A**; a third was, until Q6:

| Section | Why N/A |
|---|---|
| **SUMMARY.md changes** | `SUMMARY.md` carries pages; this spec ships a tool, a baseline file, a CI job and a row in `tools/README.md`, none of which is published |
| ~~**Target audience**~~ | **Not N/A — Q6 was ruled *repair* on 2026-09-20.** P0-1 to P0-8 are for whoever runs the gates, this programme and CI. **P0-9 is for the ordinary reader of a repaired page**, the audience `CLAUDE.md` writes for |
| **Mode mix** | Diátaxis applies to whole pages and to choosing what a page is for. Nothing here decides what a page is for |

## Topic overview

015 closed by naming the gap it could not reach:

> **A name that still resolves — to a different signature, to a dependency's removed member, or to
> a package with no release — is caught by nothing here, because the only instrument that sees it
> was built in `/tmp` and was never committed.**

**None of the eight gates compiles C#** (`tools/README.md` § *The eight gates*). Each resolves
*names*: `symbolcheck` against a watchlist, `--census` against two products'
`src/`, `versioncheck` against NuGet version strings, `pagelint` rule 6 against the **presence** of
`using` lines. A name that resolves to the wrong thing is green in all eight.

Six of 015's thirty ledgered defects were found by a compiler
(`spec/015-census_triage/tasks.md` § *Defect ledger*, defects 4–8 and 10), and **four are reachable
by no instrument in this repository**:

| Defect | Page | Why no name-resolving gate can see it |
|---|---|---|
| `S3Region.EUW1` does not exist in AWS SDK v4, on a page recommending the v4 package | `S3LuggageStore.md` | a **dependency's** member — outside both products' `src/`, so outside every census |
| `IAmAMessageMapper<T>` gained `Context` and a `Publication` parameter | `MessageMappers.md` | the name **resolves**; the *signature* moved |
| `With()`→`StartNew()`, `DefaultPolicy()`→`DefaultResilience()`, `Build()` unreachable | `FeatureSwitches.md` | live names on a live type |
| `Paramore.Brighter.{DB}.Dapper` stops at **9.9.13** — the *packages you need* list was unbuyable | `DapperOutbox.md` | spelled correctly; the **package** is what does not exist |

The question this spec answers:

> **What does it take to compile the documentation's C# blocks against the released packages, as a
> committed gate that can be run by anyone and that fails when a block stops building?**

## Current state

### The corpus, by two methods that disagreed

```bash
# method 1 — through the shipped parser
python3 -c "
import sys,re; sys.path.insert(0,'tools')
import pagelint as pl
pages=pl.load_pages(); tot=0; pgs=set()
for rel,p in pages.items():
    for b in p.blocks:
        if (b['info'] or '').strip().lower() in ('csharp','c#','cs'): tot+=1; pgs.add(rel)
print(tot,'blocks across',len(pgs),'pages')"
#   985 blocks across 145 pages

# method 2 — the obvious grep
grep -rc '^```csharp$' contents/ README.md | awk -F: '$2>0{n+=$2;p++} END{print n,p}'
#   835 117
```

**They differ by 150 blocks and 28 pages, all of it fence spelling:**

| What the fence actually looks like | Blocks |
|---|---:|
| ` ``` csharp` — a space between the fence and the info string | **140** |
| `   ```csharp` — indented three spaces, inside a list item | **8** |
| ` ``` c#` | **1** |
| ` ```csharp ` — one trailing space | **1** |
| | **150** |

`835 + 150 = 985`. **CommonMark allows all four and GitBook renders all four as C#**; `pagelint`'s
`FENCE_RE` — `^ {0,3}(`{3,}|~{3,})[ \t]*(\S*)` (`tools/pagelint.py`) — accepts them.

**Two page figures:**

```bash
#   43 pages hold at least one irregular fence
#   28 pages hold NOTHING BUT irregular fences — 92 blocks, invisible to the grep entirely
#      20 KafkaConfiguration.md · 10 HowConfiguringTheCommandProcessorWorks.md
#       6 ShowMeTheCode.md ·  5 DispatcherConfigurationReference.md
```

> An extractor built the obvious way sees **835 of 985** blocks and misses **15% of the corpus**,
> including all **92** blocks on the **28** pages it cannot see — one of them `ShowMeTheCode.md`,
> where a newcomer starts. Friction **53**; AC2.

### What those 985 blocks actually are

```bash
python3 -c "
import sys,re,collections; sys.path.insert(0,'tools')
import pagelint as pl
decl=re.compile(r'^\s*(?:public |internal |sealed |abstract |static |partial )*(class|record|interface|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)')
pages=pl.load_pages(); names=collections.Counter(); shapes=collections.Counter()
for rel,p in pages.items():
    for b in p.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp','c#','cs'): continue
        body=[l for _,l in b['body']]
        u=any(re.match(r'\s*using\s',l) for l in body); t=any(decl.match(l) for l in body)
        shapes['unit' if (u and t) else ('type only' if t else 'fragment')]+=1
        for l in body:
            m=decl.match(l)
            if m: names[m.group(2)]+=1
dups={k:v for k,v in names.items() if v>1}
print(dict(shapes)); print(len(names),'type names,',len(dups),'declared >1, ',sum(dups.values()),'declarations')"
```

| | Blocks | What it means for a compiler |
|---|---:|---|
| **fragment** — no type declaration at all | **668** | needs a wrapper invented for it: a class, a method body, or a prelude |
| **type only** — declares a type, no `using` | **213** | needs its imports supplied from outside the block |
| **unit** — `using` directives *and* a type | **104** | compiles as written, in principle |

**668 of 985 — 68% — are fragments.** In the rescued harness the wrapper is a **hand-chosen
command-line argument** (`raw` · `members` · `stmts` · `prelude`, `harness/extract.py:6`); at 985
blocks it has to be a rule.

**They cannot share a compilation:**

```text
244 distinct type names declared across the blocks
 54 of them declared more than once — 188 declarations involved
    OrderService ×16   OrderHandler ×14   MyHandler ×10   GetOrderQueryHandler ×7
```

One project holding every block is **`CS0101` 188 times over**, so each block must be isolated.

### The `using` debt, and why rule 6 is not this gate

```bash
python3 -c "
import sys,re; sys.path.insert(0,'tools')
import pagelint as pl
h=n=d=0
for rel,p in pl.load_pages().items():
    for b in p.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp','c#','cs'): continue
        body=[l for _,l in b['body']]
        if any(re.match(r'\s*using\s',l) for l in body): h+=1
        else:
            n+=1
            if any('// ...' in l for l in body): d+=1
print(h,'with a using line;',n,'without, of which',d,'declare // ...')"
#   248 with a using line; 737 without, of which 213 declare // ...
```

`pagelint` rule 6's warning count is a gate figure, in `tools/README.md` **row 2**. **Rule 6 counts
lines and cannot compile**: 015 phase 4 added `using` directives to thirteen blocks and found, in
the same fifteen blocks, a missing parenthesis, a missing comma, four undeclared variables, and a
property that takes a `Type` (`tasks.md` § *Every edited block built against the released
packages*). Neither check implies the other.

### Blocks that must NOT compile

```bash
python3 -c "
import sys; sys.path.insert(0,'tools')
import pagelint as pl
n=0; pg=set()
for rel,p in pl.load_pages().items():
    for b in p.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp','c#','cs'): continue
        if any('❌' in p.lines[i] for i in range(max(0,b['start']-5), b['start']-1)):
            n+=1; pg.add(rel)
print(n,'blocks behind a ❌ marker across',len(pg),'pages')"
#   8 blocks behind a ❌ marker across 3 pages
```

`CLAUDE.md` § *Version markers on code* **requires** superseded V9 forms beside the V10 one, so
these blocks must not build. **Eight blocks, three pages**, opted out visibly (ruling 4).

### The instrument that exists

Rescued from `/tmp` into `spec/016-compile_gate/harness/` on 2026-09-20.
`diff -r --exclude=bin --exclude=obj` against the original is empty, with the `diff` controlled by
appending one byte; all three projects rebuild with `--no-incremental` to `core`
**0 errors / 4 warnings**, `dynamo` **0/0**, `s3` **0/0**; red-proofed with
`configure.Outbox` → `configure.OutboxThatDoesNotExist` giving **`CS1061`**.

| Piece | `wc -l` | State |
|---|---:|---|
| `harness/extract.py` | **57** | extracts through `pagelint.Page`, byte for byte. **Hardcodes an absolute path to `tools/`** (line 17) and takes the wrapper as a hand-typed argument |
| `harness/list.py` | **11** | a listing helper |
| three `.csproj` | — | all `PackageReference`, all `10.7.0`. **Never a `ProjectReference` into `../Brighter/src`** |
| `harness/*/blocks/*.cs` | **13 blocks** | `ls harness/*/blocks/*.cs | grep -vc Scaffold` → 13. **1.3% of 985** |
| `harness/preludes/`, `Scaffold.cs` | 4 + 3 | identifiers a page names inside a **declared** omission — not part of any block |

### What CI already does, and what a dotnet gate costs there

```bash
gh run view $(gh run list --workflow=docs.yml --limit 1 --json databaseId --jq '.[0].databaseId') \
  --json jobs --jq '.jobs[]|"\(.name) \(.startedAt) \(.completedAt)"'
#   check    21:43:53 -> 21:44:03   10s   (python only)
#   options  21:43:53 -> 21:44:28   35s   (setup-dotnet, restore, build)
#   versions 21:43:53 -> 21:44:13   20s
```

**The `options` job is the precedent.** It runs `dotnet run --project tools/optioncheck`, pins its
packages in one `.csproj`, and costs **35 seconds** from a cold NuGet cache. Two of its decisions,
both argued in `docs.yml`'s comments, transfer directly:

- **No `schedule:` trigger**, because it reflects over a **pinned** package and nothing outside
  this repository can change its verdict. A pinned compile gate has the same property.
- **No guard and no `|| true`.** Exit 2 is *nothing was checked*, which is not a pass.

**It partly answers the project-family question:**

```bash
grep -c 'PackageReference' tools/optioncheck/optioncheck.csproj      # 63
```

**63 Paramore packages coexist in one project today**, proven to load in one process by spec 012's
probe 1.3. That project includes `Paramore.Brighter.Transformers.AWS` and **excludes**
`.Transformers.AWS.V4`, so the `NU1107` the harness hit is the **`.AWS` / `.AWS.V4` pair**, not a
property of the package set. The harness's *three* projects were built ad hoc for thirteen blocks
and are **not** evidence that three are needed.

Locally, a project build costs about three seconds warm (`core` 3.26s, `s3` from a deleted
`bin`/`obj` 2.85s): **cost scales with the number of projects and restores, not the number of
blocks.**

## Target state

1. **A ninth gate.** `tools/README.md` gains a row: the command, what it checks, and the number it
   prints when nothing is wrong, at a named ref.
2. **One command anyone can run** over the whole corpus or one page, with this repository's exit
   contract — **0** clean, **1** a fault in the corpus, **2** nothing was checked.
3. **A committed baseline** of blocks known to build, which the gate enforces and which only grows.
   The gate is green on `master` from the commit it lands in — ruling 8, and 015 finding 4.
4. **A published verdict for every one of the 985 blocks** — built, skipped with a reason, or not
   compilable with a reason. **No block is silently absent.**
5. **A CI job** modelled on `options`: pinned packages, no guard, no `schedule:`, no swallowed exit
   code.
6. **The boundary written down.** Every identifier the harness supplies that is not on the page is
   listed by the run, so a green verdict states what it was given.

## Source material

Read before writing, and cited above where used:

| Source | What it settles |
|---|---|
| `spec/015-census_triage/tasks.md` §§ *Defect ledger*, *Every edited block built against the released packages (task 4.8)*, *What compiling found that no watchlist could have* | the six compiler defects, and the only worked example of building these blocks |
| `spec/016-compile_gate/harness/` | the instrument, its wrapper vocabulary, its package pins |
| `tools/pagelint.py` — `FENCE_RE`, `class Page`, `check_code_blocks` | the fence grammar, and rule 6's actual claim |
| `tools/optioncheck/optioncheck.csproj`, `tools/optioncheck/Program.cs` | the committed-dotnet-tool precedent, the 63-package set, `CopyLocalLockFileAssemblies` |
| `.github/workflows/docs.yml` | the three jobs, the no-guard/no-`||true`/no-`schedule` reasoning, `fetch-depth: 0` |
| `tools/README.md` | the eight rows, the exit-code contract, "a number without a ref is not a fact" |
| `CLAUDE.md` §§ *Compiling an example, and against what*, *Version markers on code*, *Complete code blocks*, and the ledger's three **review only** rows | what a block owes a reader, and which two of the three rows this spec instruments |
| `spec/012-configuration_reference/probes/README.md` | that the 63 packages load in one process |

## Scope

### P0 — the spec fails without these

| | What |
|---|---|
| **P0-1** | **`tools/blockcheck`, committed.** The harness becomes a tool: no absolute paths, extraction through `pagelint.Page` (never a second parser), released packages only, this repository's exit contract |
| **P0-2** | **Block classification.** Every C# block is classified — *unit* · *type only* · *fragment* — and each class has a declared wrapper rule. A block the rules cannot wrap is reported **`NOT COMPILABLE`** with a count, never dropped |
| **P0-3** | **Isolation.** 188 clashing declarations must coexist in whatever compilation structure is chosen (namespace per block, project per family, or both) |
| **P0-4** | **The baseline and the ratchet.** A committed list of blocks that build; the gate is red when one of them stops building and when the list disagrees with the corpus in either direction |
| **P0-5** | **The visible opt-out.** The eight ❌ V9 blocks, and any other block that must not compile, carry a marker with a **reason**, and the run prints `N skipped` beside its findings — ruling 4 |
| **P0-6** | **Run it over all 985 and publish the distribution** — built / skipped / not compilable / **failing**. This is the measurement the design's scope decisions depend on, and nobody has it |
| **P0-7** | **The red-proof and its controls.** The gate is seen failing before it is trusted; the positive control is planted **outside** the enumeration the tool was built from (obligation 3) |
| **P0-8** | **The CI job**, on the `options` pattern, and the `tools/README.md` row that owns its number |

| **P0-9** | **Repair what the run finds**, on 015's P0-6 pattern. **Promoted from P1-1 by Q6's ruling**, 2026-09-20. **Scoped to defects of *claim*:** a block that cannot compile *by construction* (the six before/after-in-one-fence blocks), a name that resolves to the wrong thing (`CS1061`, `CS7036` — 015's class), a package that has no release, a type whose signature moved. **Not** the ~900 blocks failing on `CS0246`/`CS0103` for want of imports or page context: that is backlog item 2, it is the 744-block debt `tools/README.md` row 2 counts, and folding it in makes this spec unbounded. **Every block admitted to the baseline is repaired to compiling as a side effect**, which is how the debt actually falls — page by page, with a verdict, rather than by a sweep nothing checks |

### P1 — wanted, and cut first if the spec is too big

| | What |
|---|---|
| ~~**P1-1**~~ | **Promoted to P0-9** by Q6's ruling |
| ~~**P1-2**~~ | **WITHDRAWN at the design review**, not deferred. A `--changed` mode existed to control cost; the design measured the whole corpus at **6.5–39.5s**, so there is no cost to control |

### P2 — named so it is not silently assumed

| | What |
|---|---|
| **P2-1** | **Retiring the `using`-directive debt** (backlog item 2). This spec supplies the instrument that makes each of those blocks a job with a verdict; it does not walk the list |
| **P2-2** | **Darker's blocks.** `grep -rhoE 'Paramore\.Darker[A-Za-z0-9.]*' contents/ \| sort -u` → **6** distinct strings — `Paramore.Darker`, `.AspNetCore`, `.Builder`, `.Policies`, `.Policies.Constants`, `.QueryLogging` — **of which `.Builder` and `.Policies.Constants` are namespaces rather than packages**, so the package set is smaller still. Darker is at **4.1.1** and versions independently. Whether they join the first baseline is Q3 |

## Out of scope

- **Running a block to check its behaviour.** `CLAUDE.md`'s ledger has three review-only rows; this
  spec instruments **two** of them (compiling, and compiling *against the released packages*) and
  **not** the third — *a block asserting behaviour is run, with a control*. **Four merged examples
  compiled cleanly and still asserted false behaviour**; a green compile run says nothing about that.
- **Prose.** 015 swept C# fences; **160 of 161 pages carry an unresolved prose token** (014 D12,
  inherited and not re-measured). A compiler reaches none of it.
- **Non-C# fences** — `bash`, `yaml`, `json`, `text`.
- **`../Brighter` and `../Darker`.** Read-only, and this spec has no sample-code exception to
  invoke: it compiles against **NuGet packages**, never a `ProjectReference` into `src/`.
- **Deciding a page's type, banner or opening sentence**, including on any page P1-1 repairs.
- **Promoting `--census` to a gate.** Ruling 12 stands and nothing here bears on it.

## Deliverables

Specific files. **None is a page**, so the page-type rule does not bind. **Q6 was ruled *repair*
on 2026-09-20, so deliverable 8 is live: on a page P0-9 repairs, no banner, page type or opening
sentence may change.**

| | File | What it is |
|---|---|---|
| 1 | `tools/blockcheck.py` | the driver: enumerate, classify, extract, build, report |
| 2 | `tools/blockcheck/*.csproj` (count per Q2) | the package pins, at an explicit version, NuGet only |
| 3 | `tools/blockcheck/baseline.tsv` | the committed list of blocks that build, with its own header row |
| 4 | `tools/blockcheck/scaffold/` | the identifiers supplied to a block that declares an omission, one file per prelude, listed by the run |
| 5 | `tools/README.md` | the ninth row, its number, and its ref — **and the number lives nowhere else** |
| 6 | `.github/workflows/docs.yml` | the new job |
| 7 | `spec/016-compile_gate/{requirements,design,tasks}.md` | this spec's own record, including the run of P0-6 |
| 8 | **repairs under `contents/`** | **live — Q6 ruled *repair*, 2026-09-20.** Scope is P0-9's: defects of claim. The six before/after blocks are the opening list |

## SUMMARY.md changes

**N/A.** `SUMMARY.md` carries pages. This spec ships a tool, a baseline, a CI job and a row in
`tools/README.md`, none of which is published. Q6's repairs edit pages that **already exist** and
create none, so `SUMMARY.md` is untouched.

## Constraints

1. **Released packages, always.** Never a `ProjectReference` into `../Brighter/src` — it vouches
   for an API nobody can install (`CLAUDE.md`; spec 013 phase 2 found exactly that).
2. **One parser.** Extraction goes through `pagelint.Page`. A second fence parser is how 150 blocks
   go missing.
3. **What is compiled is what is on the page**, byte for byte, plus a declared wrapper and a
   declared scaffold — both listed by the run.
4. **The exit contract is `tools/README.md`'s**: 0 clean, 1 a corpus fault, **2 nothing was
   checked**. No guard, no `|| true`, no swallowing a code in a pipeline.
5. **An opt-out is never silent** (ruling 4). `0 findings` and `0 findings, 8 skipped` are
   different claims.
6. **No redirectable target.** By ruling 1's reasoning — a gate that can be pointed at another list
   can be silenced — there is **no `--baseline <path>`**.
7. **A build that declines to work is not a pass.** `--no-incremental`, or a proof it is not needed.
8. **The gate's number changes in `tools/README.md` and nowhere else** (obligation 10).
9. **Gate movement predicted before the work, including "none", argued from mechanism**
   (obligation 6). `tools/` is inside `linkcheck`'s walk and has moved it 164 → 165 before.
10. **Ask before merging anything that changes the published site**, and ask for the head-ref
    deletion by name in the same breath (obligation 7).
11. **No criterion reads an exit code through a pipe, and no criterion is satisfiable by a missing
    path** (§ *What the review found in this document*). Downstream of a `|`, `$?` is the last
    stage's; and `grep … | wc -l` prints `0` whether the corpus is clean or the path does not exist.
    **Redirect to a file, read the code, then filter** — or set `pipefail` and say so.

## Acceptance criteria

Numbered, each naming the command that decides it, or marked as having none, with a reader. Both
criteria ever found unmet at a close — 009's AC7 and 012's AC1 — were unmarked ones, so AC13–AC15
are a known risk.

| # | Criterion | Instrument |
|---:|---|---|
| **AC1** | `python3 tools/blockcheck.py` runs over the whole corpus and prints, for **every** C# block, exactly one verdict — `BUILT` · `SKIPPED` · `NOT COMPILABLE` · `FAILED` | `python3 tools/blockcheck.py --report > /tmp/r; echo $?` — **the exit code read before anything is piped** (see Constraint 11) — then `awk '{n[$1]++} END{for(k in n) print k,n[k]}' /tmp/r`, and **the four counts sum to the corpus count** from § *Current state* |
| **AC2** | The corpus it enumerates **contains** all 985 blocks, including the 150 with irregular fences | `python3 tools/blockcheck.py --list > /tmp/l; echo $?` then `wc -l < /tmp/l` equals the `pagelint.Page` count, **and** `--list` names all **20** blocks of `contents/KafkaConfiguration.md` — a page whose every fence is ` ``` csharp` and which a grep-shaped extractor sees as having no C# at all. **Named, not sampled**, because a sample of a 985-block corpus will not contain the 15% that is missing |
| **AC3** | No `ProjectReference` into either product, and every package carries an explicit version | **The existence check comes first, because a missing path is otherwise indistinguishable from a clean one:** `ls tools/blockcheck/*.csproj` — non-empty, count recorded — **then** `grep -c ProjectReference tools/blockcheck/*.csproj` → 0 per file, and `grep -c 'Version="'` equals `grep -c 'PackageReference'` per file |
| **AC4** | **Red-proof, two-way.** A baselined block is broken on purpose and the run reports the compiler's own error code; restored, the file is byte-identical and the run is green | the run's output showing `CS1061` (or equivalent), then `diff` against the pre-edit copy → empty, then exit 0. **The positive case is planted outside the enumeration the tool was built from** |
| **AC5** | **A build that does nothing is not reported as a pass.** Two consecutive runs with nothing touched report the **same** number of blocks built | run twice, diff the two `--report` outputs → empty, **and** the second run's wall clock is not an order of magnitude below the first |
| **AC6** | What is compiled is byte-identical to the page body | `python3 tools/blockcheck.py --verify-extraction` → **N of N identical**, N equal to AC1's corpus count |
| **AC7** | Skips are visible and counted, and every skip carries a reason | the run's last line reads `… , N skipped`; `python3 tools/blockcheck.py --list-skips` prints one reason per skip, **≥ 8** of them the ❌ V9 blocks |
| **AC8** | Scaffolding is declared: every identifier supplied from outside the page is listed by the run | `python3 tools/blockcheck.py --list-scaffold` prints them, and the count matches the files under `tools/blockcheck/scaffold/` |
| **AC9** | The baseline is enforced in **both** directions — a baselined block that stops building is exit 1, **and** a baselined block that has vanished from the corpus is exit 1 | delete one line from `baseline.tsv` and confirm exit 1; add a line naming a block that does not exist and confirm exit 1. **Two edits, both reverted** |
| **AC10** | The gate's headline figure appears in `tools/README.md` and nowhere else | `grep -rn '<figure>' --include='*.md' --include='*.yml' --include='*.py' . > /tmp/f; grep -vc '^./spec/' /tmp/f` → **1**, **and the same grep run for a figure that IS duplicated returns >1** — without that control, a typo'd search string returns 1 or 0 and reads as a pass |
| **AC11** | The eight gates are predicted before the work and reconciled after, with a reason for every "none" | each gate's own command, run twice; the prediction is written in `tasks.md` **before** the phase |
| **AC12** | P0-6's distribution is published — built / skipped / not compilable / failing across all 985 — with the command beside it | the `--report` output, committed into `tasks.md` |
| **AC13** | **No instrument — checked by reading, by the maintainer at the design review.** That the boundary between *the block* and *the scaffold* is one a reader would accept: the harness must not supply what the page should have printed | the maintainer, against `tools/blockcheck/scaffold/` and AC8's listing |
| **AC14** | **No instrument — checked by reading, by the maintainer at the acceptance walk.** That a `BUILT` verdict is not being bought by a wrapper that changes what the block means | the maintainer, against a sample of wrapped fragments and their pages |
| **AC15** | **No instrument — checked by reading, at the acceptance walk.** That this spec's own documents do not claim the gate checks **behaviour**, which it does not | whoever walks the criteria |

### What the review found in this document — three defects, recorded before they were fixed

Found 2026-09-20 at `/spec:review` by running each named instrument and comparing its output with
the criterion.

| | Was | Defect, and the fix |
|---|---|---|
| **AC3** | `grep -rn 'ProjectReference' tools/blockcheck*` → **0** | **Satisfiable by the tool not existing**: with no `tools/blockcheck*`, `… \| wc -l` prints **`0`**, AC3's pass. The existence check now comes first |
| **AC1, AC2, AC10** | the verdict read through `\| awk`, `\| wc -l`, `\| grep -v` | **The exit code was the pipe's.** `python3 -c 'sys.exit(2)'` reads **2** bare, **0** piped to `head`, **2** piped with `pipefail`; a clean run reads **0** piped. Each now redirects to a file and reads the code first |
| **§ Quality checklist** | *"12 instrumented, 3 marked as having none"* | **Overstated**: eleven of the twelve name `tools/blockcheck.py`, not yet built. They are marked **deferred**; only **AC11** runs today, and it is green |

The first two are the family of plausible zero 11 and friction 48: *the form that gets run is not
the form that was reviewed*. Eleven deferred instruments are not eleven unmarked criteria: they are
deferred because the deliverable is the instrument, and they become runnable when P0-1 ships.

## Open questions

Numbered, each with a recommendation and what it depends on, and its ruling where there is one.

**Q1 — What is the green bar: the whole corpus, or a ratcheting baseline?**
*Recommendation:* **a ratcheting baseline.** 985 blocks cannot be made green in one spec — 668 are
fragments — and a gate red on `master` is not a gate (ruling 8; 015 finding 4). The tool reports
the whole corpus; the **exit code** is decided by the committed list. *Depends on:* P0-6's
distribution, how many build unaided.

**Q2 — How many projects?**
*Recommendation:* **start from one**, reusing `optioncheck`'s proven 63-package set, and add a
second **only** for the `.Transformers.AWS.V4` conflict. *Depends on:* whether one project can
restore the union of the packages the pages name.

**Q3 — Does Darker's surface join the first baseline?**
*Recommendation:* **yes, and pinned separately at 4.1.1.** Darker versions independently of
Brighter, and the surface is small — **6** distinct `Paramore.Darker*` strings in `contents/`, two
of them namespaces rather than packages (P2-2). *Depends on:* whether Darker's packages coexist
with Brighter's in one project, unmeasured.

**Q4 — Is a fragment that cannot be wrapped honestly a *failure* or a *verdict*?**
*Recommendation:* **a verdict — `NOT COMPILABLE`, with a count and a reason**, never a silent
absence and never a red build. A one-line snippet showing a property assignment has no honest
compilation unit, and forcing one invents a page. *Depends on:* how large that class turns out to
be; if it is most of the 668, the spec's shape changes.

**Q5 — How is the opt-out spelled?**
*Recommendation:* follow the two existing ones —
`<!-- blockcheck: skip <reason> -->`, mirroring `<!-- symbolcheck: allow <name> -->` and
`<!-- pagelint: allow-serviceactivator -->` — and **require** the reason. *Depends on:* nothing.

**Q6 — Does this spec repair what it finds, or hand over a list?**
**RULED 2026-09-20 at the design review, by the maintainer: REPAIR — *"It should fix documentation
issues."*** *Recommendation was:* **repair, and rule it now** — 015 answered its Q4 *list*, was
overturned, and the overturn falsified its subject section mid-spec.

**Consequences, all applied:** *Target audience* stops being N/A; **obligation 7 binds** on the PR
carrying the repairs; **P1-1 becomes P0-9**.

> **P0-9 is scoped to defects of *claim*, not of *context*** (§ *Scope*). The design's probe
> measured **925 of 985 blocks failing**, roughly 900 of them on `CS0246`/`CS0103` — missing
> `using` directives and page-context identifiers. That is backlog item 2, a programme rather than
> a phase.

**Q7 — Does the gate run on every pull request, or only over changed blocks?**
**DISSOLVED at the design review, 2026-09-20.** The design measured the **whole 985-block corpus at
6.5–39.5 seconds**, against the `options` job's 35s, so the gate runs over everything on every pull
request and **P1-2 is withdrawn**. *Recommendation was:* whole baseline on every PR, adding
`--changed` if cost grew.

**Q8 — Does it get a `schedule:` trigger?**
*Recommendation:* **no.** `docs.yml` argues this for `optioncheck` already: a gate over **pinned**
packages cannot have its verdict changed by anything outside this repository, so a scheduled run
repeats the last answer or fails because NuGet was briefly unreachable. **`versioncheck` already
owns the "a release happened" signal.** *Depends on:* Q9.

**Q9 — Pinned at `10.7.0`, or following the latest release?**
*Recommendation:* **pinned**, with the version in one place, bumped deliberately — the pages
themselves pin `10.7.0` and `versioncheck` already gates that. A gate that follows `latest` turns
somebody else's release into a red build on an untouched repository. *Depends on:* nothing
technical; an editorial choice.

## Workflow friction

The ledger stands at **51** — 015's `tasks.md` §4 holds 46–51, plus one deliberately unnumbered
recurrence. **016 adds 52 and 53.**

**52 — A control whose two halves measure different things reports a difference that is not
there.** Checking the rescued harness was byte-identical after a red-proof, `shasum blocks/*.cs |
shasum` before and after **differed with the content unchanged**: `shasum` prints the path beside
each digest, and the runs were made from different directories (`core/blocks/…` against
`blocks/…`). The mirror image of a plausible zero; friction 48's family. The check that works is
`diff -r` against the untouched original, **with the `diff` controlled** by appending one byte.

**53 — An extractor built the obvious way misses 15% of its corpus and reports success on the
rest.** `grep -c '^```csharp$'` finds **835** blocks; `pagelint.Page` finds **985**. The 150 are
` ``` csharp` with a space (140), indented inside a list item (8), ` ``` c#` (1), and one trailing
space — **all four render as C#, and the grep misses them and 28 entire pages holding 92 blocks**,
`ShowMeTheCode.md` among them. **A tool that enumerates its own corpus owes a second method for the
size of that corpus.**

## Quality checklist, applied to this document

| | |
|---|---|
| Readable with no prior context | 015's closing sentence, the four defects and the gap are stated here rather than cited |
| P0 / P1 / P2 distinguished | eight P0, two P1, two P2, and **Q6 explicitly moves P1-1 between them** |
| Specific files, not gestures | `harness/extract.py:17`, `tools/pagelint.py`'s `FENCE_RE`, `tools/optioncheck/optioncheck.csproj`, `docs.yml`'s three jobs |
| A command beside every number | every figure in § *Current state* carries its command; the two gate figures are **cited from `tools/README.md`, not pasted** |
| Criteria name their instrument | 12 instrumented, of which **11 are deferred** until P0-1 builds `tools/blockcheck.py`, and **AC11 is runnable today and green**. **3 have no instrument, each with a named reader** — AC13, AC14, AC15 |
| Questions carry recommendations | nine, each with a recommendation and what it depends on |
| It applies the checklist to itself | this table, and the README re-derivation that produced friction 53 |

---

**Next step: `/spec:review`.** Nine questions want ruling; **Q1 and Q6 change what gets built** —
the green bar, and whether this spec edits published pages.
