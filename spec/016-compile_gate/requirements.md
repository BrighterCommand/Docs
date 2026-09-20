# Spec 016: A Committed Compile Gate — Requirements

**Created:** 2026-09-20
**Status:** **APPROVED 2026-09-20** — `.requirements-approved`. Reviewed 2026-09-20: three defects
found in this document's own acceptance criteria **by running their instruments**, all three
repaired, with the prior wording recorded in § *What the review found in this document*.

> **THE NINE OPEN QUESTIONS WERE NOT RULED AT THIS REVIEW, AND THAT IS RECORDED RATHER THAN
> TIDIED.** Approval was given with them open, so **each question's recommendation becomes the
> design's working assumption** — stated as an assumption every time it is relied on, never as a
> ruling. **Q1 (a ratcheting baseline) and Q6 (this spec repairs what it finds) change what gets
> built**, and Q6 in particular decides whether *Target audience* stays N/A and whether obligation
> 7 binds. They carry forward to `/spec:design`'s review, where 015 ruled its six.
>
> **That is what happened. At the design review on 2026-09-20, Q1, Q2, Q4 and Q7 were settled by
> the design's probe, and Q6 was RULED *repair* by the maintainer** — so P1-1 became **P0-9**,
> *Target audience* stopped being N/A, and obligation 7 now binds. **Q3, Q5, Q8 and Q9 remain
> open**, and none of them changes what gets built.
>
> **015's Q4 is why this paragraph exists.** Two of its six recommendations were overturned at
> review, one of them falsifying its own subject section — so a recommendation carried silently
> into a design is a decision nobody took.

> **Every number here carries the command that produced it.** All figures measured 2026-09-20
> against Docs `master` `d412702`, working tree clean apart from this spec. Where a figure is a
> **gate** figure it is **cited from `tools/README.md` and not pasted** — obligation 10.

> **The README was re-derived before this document was written, and one of its figures was wrong in
> a way worth keeping.** It carried *985 C# blocks* from a single method. A second method returns
> **835**, and the 150-block gap is not noise — see § *Current state*, first table. The README's
> count survives; the confidence in it does not, and friction **53** is the entry.

## Subject

**Process.** The deliverable is an instrument and a gate — a ninth row in `tools/README.md` — plus
the method for extracting a C# block from a page and building it against the packages a reader
would install. **It creates no page.**

Three sections are **N/A**, named rather than left empty:

| Section | Why N/A |
|---|---|
| **SUMMARY.md changes** | A category error here rather than an empty heading. `SUMMARY.md` carries pages; this spec ships a tool, a baseline file, a CI job and a row in `tools/README.md`, none of which is published |
| ~~**Target audience**~~ | **NO LONGER N/A — Q6 was ruled *repair* on 2026-09-20.** The audience of P0-1 to P0-8 is whoever runs the gates, this programme and CI. **The audience of P0-9 is the ordinary reader of a repaired page** — the audience `CLAUDE.md` writes for. The conditional this row used to carry was written precisely so the ruling would not leave a stale N/A behind, which is what 015's Q4 did |
| **Mode mix** | Diátaxis applies to whole pages and to choosing what a page is for. Nothing here decides what a page is for |

> **015's Q4 is the warning this section is written against.** 015 declared itself "not a page",
> and a single ruling turned it into a spec that edited ten pages, at which point obligation 7 bound
> and the N/A table had to be re-reasoned rather than inherited. **Q6 below asks that question now,
> at the requirements review, instead of discovering it at a phase boundary.**

## Topic overview

015 closed by naming the gap it could not reach:

> **A name that still resolves — to a different signature, to a dependency's removed member, or to
> a package with no release — is caught by nothing here, because the only instrument that sees it
> was built in `/tmp` and was never committed.**

**Eight gates, and not one of them compiles a line of C#** (`tools/README.md` § *The eight gates*).
Every one resolves *names*: `symbolcheck` against a watchlist, `--census` against two products'
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

**They disagree by 150 blocks and 28 pages, and the whole difference is fence spelling:**

| What the fence actually looks like | Blocks |
|---|---:|
| ` ``` csharp` — a space between the fence and the info string | **140** |
| `   ```csharp` — indented three spaces, inside a list item | **8** |
| ` ``` c#` | **1** |
| ` ```csharp ` — one trailing space | **1** |
| | **150** |

`835 + 150 = 985`, so both methods are right about what they looked at. **CommonMark allows all
four and GitBook renders all four as C#**; `pagelint`'s `FENCE_RE` —
`^ {0,3}(`{3,}|~{3,})[ \t]*(\S*)` (`tools/pagelint.py`) — was written to accept them.

**Two page figures, and they mean different things:**

```bash
#   43 pages hold at least one irregular fence
#   28 pages hold NOTHING BUT irregular fences — 92 blocks, invisible to the grep entirely
#      20 KafkaConfiguration.md · 10 HowConfiguringTheCommandProcessorWorks.md
#       6 ShowMeTheCode.md ·  5 DispatcherConfigurationReference.md
```

> **This is a requirement, not an anecdote.** An extractor built the obvious way sees **835 of
> 985** blocks, compiles all 835, reports success, and is **silently blind to 15% of the corpus** —
> including all **92** blocks on the **28** pages it cannot see at all. **One of those 28 is
> `ShowMeTheCode.md`**, which is where a newcomer starts. Friction **53**, and the reason AC2
> exists.

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
command-line argument** (`raw` · `members` · `stmts` · `prelude`, `harness/extract.py:6`). At
thirteen blocks a person chooses; at 985 the choice is the spec.

**And they cannot share a compilation:**

```text
244 distinct type names declared across the blocks
 54 of them declared more than once — 188 declarations involved
    OrderService ×16   OrderHandler ×14   MyHandler ×10   GetOrderQueryHandler ×7
```

One project holding every block is **`CS0101` 188 times over**. Isolation per block is a
requirement, not a refinement.

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

`pagelint` rule 6's warning count is a **gate figure** and lives in `tools/README.md` **row 2**;
cite it there. What matters here is that **rule 6 counts lines and cannot compile**: 015 phase 4
added `using` directives to thirteen blocks and found, in the same fifteen blocks, a missing
parenthesis, a missing comma, four undeclared variables, and a property that takes a `Type`
(`tasks.md` § *Every edited block built against the released packages*). **The two checks answer
different questions and neither implies the other.**

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

`CLAUDE.md` § *Version markers on code* **requires** superseded V9 forms to be shown beside the
V10 one. A gate demanding that every C# block builds would demand the documentation stop doing
that. **Eight blocks, three pages** — small, known, and it must be opted out visibly (ruling 4).

### The instrument that exists

Rescued from `/tmp` into `spec/016-compile_gate/harness/` on 2026-09-20 and verified rather than
assumed — `diff -r --exclude=bin --exclude=obj` against the original is empty, with the `diff`
controlled by appending one byte; all three projects rebuild with `--no-incremental` to `core`
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

**The `options` job is the precedent and it is a good one.** It runs `dotnet run --project
tools/optioncheck`, pins its packages in one `.csproj`, and costs **35 seconds** from a cold NuGet
cache. Two of its design decisions transfer directly, and both are argued in `docs.yml`'s own
comments:

- **No `schedule:` trigger**, because it reflects over a **pinned** package and nothing outside
  this repository can change its verdict. A pinned compile gate has the same property.
- **No guard and no `|| true`.** Exit 2 is *nothing was checked*, which is not a pass.

**And it answers the project-family question partly:**

```bash
grep -c 'PackageReference' tools/optioncheck/optioncheck.csproj      # 63
```

**63 Paramore packages coexist in one project today**, proven to load in one process by spec 012's
probe 1.3. That project includes `Paramore.Brighter.Transformers.AWS` and **excludes**
`.Transformers.AWS.V4` — so the `NU1107` the harness hit is specifically the **`.AWS` / `.AWS.V4`
pair**, not a general property of the package set. The harness's *three* projects were built ad hoc
for thirteen blocks and are **not** evidence that three are needed.

Locally, a project build costs about three seconds warm (`core` 3.26s, `s3` from a deleted
`bin`/`obj` 2.85s) — so **cost scales with the number of projects and restores, not with the number
of blocks**, which is the single most important fact for the design.

## Target state

1. **A ninth gate.** `tools/README.md` gains a row: the command, what it checks, and the number it
   prints when nothing is wrong, at a named ref.
2. **One command anyone can run** over the whole corpus or one page, with this repository's exit
   contract — **0** clean, **1** a fault in the corpus, **2** nothing was checked.
3. **A committed baseline** of blocks known to build, which the gate enforces and which only grows.
   The gate is green on `master` from the commit it lands in — ruling 8, and 015 finding 4.
4. **A published verdict for every one of the 985 blocks** — built, skipped with a reason, or not
   compilable with a reason. **No block is silently absent**, which is the entire lesson of the
   150-block gap.
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
  compiled cleanly and still asserted false behaviour**; a compile gate does not touch that, and
  saying so here stops a green run being read as more than it is.
- **Prose.** 015 swept C# fences; **160 of 161 pages carry an unresolved prose token** (014 D12,
  inherited and not re-measured). A compiler reaches none of it.
- **Non-C# fences** — `bash`, `yaml`, `json`, `text`.
- **`../Brighter` and `../Darker`.** Read-only, and this spec has no sample-code exception to
  invoke: it compiles against **NuGet packages**, never a `ProjectReference` into `src/`.
- **Deciding a page's type, banner or opening sentence**, including on any page P1-1 repairs.
- **Promoting `--census` to a gate.** Ruling 12 stands and nothing here bears on it.

## Deliverables

Specific files. **None is a page**, so the "which of the four page types" rule does not bind — said
out loud, because a deliverables list with no page types otherwise reads as an omission.
**Q6 was ruled *repair* on 2026-09-20, so deliverable 8 is live and binds in the opposite
direction: on a page P0-9 repairs, no banner, page type or opening sentence may change.**

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

**N/A — and a category error rather than an empty section.** `SUMMARY.md` carries pages. This spec
ships a tool, a baseline, a CI job and a row in `tools/README.md`, none of which is published. A
repair under Q6 edits pages that **already exist** and creates none, so `SUMMARY.md` is untouched
even then.

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
    path.** Both were found in this document's own acceptance criteria at the requirements review —
    § *What the review found in this document*. A gate whose contract is **0 / 1 / 2** is unreadable
    downstream of a `|`, because `$?` is the last stage's; and `grep … | wc -l` prints `0` whether
    the corpus is clean or the path does not exist. **Redirect to a file, read the code, then
    filter** — or set `pipefail` and say so.

## Acceptance criteria

Numbered, each naming the command that decides it — or marked as having none, with a reader.
**Both criteria ever found unmet at a close — 009's AC7 and 012's AC1 — were the unmarked ones**,
so the three unmarked ones below are a known risk rather than an oversight.

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

**Found 2026-09-20 at `/spec:review`, by its own required step: *run each named instrument now and
check what it prints against what the criterion claims*.** Obligation 2 — the prior wording is
recorded here rather than overwritten.

| | What it said | What running it showed |
|---|---|---|
| **AC3** | `grep -rn 'ProjectReference' tools/blockcheck*` → **0** | **The criterion was satisfiable by the tool not existing.** `tools/blockcheck*` matches nothing today; the glob fails, and the form a criterion actually gets read through — `… \| wc -l` — prints **`0`**, which is what AC3 claims as its pass. The existence check now comes first |
| **AC1, AC2, AC10** | the verdict read through `\| awk`, `\| wc -l`, `\| grep -v` | **The exit code was the pipe's, not the tool's.** Controlled both ways: `python3 -c 'sys.exit(2)'` reads **2** bare, **0** piped to `head`, **2** piped with `pipefail`, and a clean run still reads **0** piped — so the green half of the control holds. **A gate whose whole contract is 0/1/2 had three criteria that could not see a 2** |
| **§ Quality checklist** | *"12 instrumented, 3 marked as having none"* | **Overstated.** Eleven of the twelve name `tools/blockcheck.py`, which this spec has not built yet. Only **AC11** — the eight gates — can be run today, and it is green |

**The first two are the same family as plausible zero 11** (a control naming a ref that does not
exist) **and friction 48** (the form that gets run is not the form that was reviewed). This
programme has now met that shape in a git flag, a zsh glob, a shell pipeline and a control's own
harness.

**Eleven deferred instruments are not eleven unmarked criteria**, and the distinction is worth
stating rather than resolving by rule. *"An instrument that cannot be run yet is a criterion with
no instrument"* is aimed at a criterion whose instrument nobody will ever build; for a spec whose
**deliverable is the instrument**, every criterion about it is unrunnable at requirements time by
construction. What the rule properly demands here is that the document **say which is which** — so
the eleven are marked **deferred**, they become real the moment P0-1 ships, and AC11 is the only
one green today.

## Open questions

Numbered, by name, each with a recommendation and what it depends on. **A question nobody wrote
down gets decided by whoever is typing.**

**Q1 — What is the green bar: the whole corpus, or a ratcheting baseline?**
*Recommendation:* **a ratcheting baseline.** 985 blocks cannot be made green in one spec — 668 are
fragments needing a wrapper decision each — and a gate that is red on `master` is not a gate
(ruling 8; 015 finding 4). The tool knows the whole corpus; the **exit code** is decided by the
committed list. *Depends on:* P0-6's distribution — how many build unaided is the number nobody
has, and it could be 60% or 6%.

**Q2 — How many projects?**
*Recommendation:* **start from one**, reusing `optioncheck`'s proven 63-package set, and add a
second **only** for the `.Transformers.AWS.V4` conflict. The harness's three were built ad hoc for
thirteen blocks and prove nothing about the general case. *Depends on:* whether one project can
restore the union of the packages the pages name — measurable in an afternoon, and cheap to be
wrong about.

**Q3 — Does Darker's surface join the first baseline?**
*Recommendation:* **yes, and pinned separately at 4.1.1.** Darker versions independently of
Brighter, and the surface is small — **6** distinct `Paramore.Darker*` strings in `contents/`, two
of them namespaces rather than packages (P2-2). A small surface is a cheap answer, not a reason to
skip it. *Depends on:* whether Darker's packages coexist with Brighter's in one project —
unmeasured, and the first thing to try given Q2.

**Q4 — Is a fragment that cannot be wrapped honestly a *failure* or a *verdict*?**
*Recommendation:* **a verdict — `NOT COMPILABLE`, with a count and a reason**, never a silent
absence and never a red build. A one-line snippet showing a property assignment has no honest
compilation unit, and forcing one invents a page. *Depends on:* how large that class turns out to
be; if it is most of the 668, the spec's shape changes.

**Q5 — How is the opt-out spelled?**
*Recommendation:* follow the two existing ones —
`<!-- blockcheck: skip <reason> -->`, mirroring `<!-- symbolcheck: allow <name> -->` and
`<!-- pagelint: allow-serviceactivator -->` — and **require** the reason. *Depends on:* nothing;
decide it at the review and stop thinking about it.

**Q6 — Does this spec repair what it finds, or hand over a list?**
**RULED 2026-09-20 at the design review, by the maintainer: REPAIR — *"It should fix documentation
issues."*** The recommendation is preserved below, because a recommendation that was *upheld* is
still evidence about how the spec was reasoned.

*Recommendation was:* **repair, and rule it now.** 015 asked this as Q4, answered *list*, was
overturned, and the overturn falsified its own subject section mid-spec. The cost of ruling it at
the review is zero; the cost of discovering it at phase 4 is an amended requirements document.

**Three consequences, all applied:** *Target audience* stops being N/A — the audience of a repaired
page is its ordinary reader; **obligation 7 binds** on whichever PR carries the repairs; and
**P1-1 becomes P0-9**.

> **THE RULING SETS THE ANSWER, NOT THE BOUNDARY — and here the boundary carries the weight.**
> The design's probe measured **925 of 985 blocks failing**, and roughly 900 of those fail on
> `CS0246`/`CS0103`: missing `using` directives and undefined page-context identifiers. **That is
> the debt already named as backlog item 2** — a programme, not a phase. So **P0-9 is scoped to
> defects of *claim*, not defects of *context***, as § *Scope* now states. **If the wider reading
> was intended, that scope line is the thing to overrule** — and it would make 016 a multi-spec
> effort rather than a gate.

**Q7 — Does the gate run on every pull request, or only over changed blocks?**
**DISSOLVED at the design review, 2026-09-20 — the question was about cost, and the cost is not
there.** The design measured the **whole 985-block corpus at 6.5–39.5 seconds**, against the
`options` job's 35s. So the gate runs over everything on every pull request, and **P1-2 is
withdrawn rather than deferred**.

*Recommendation was:* whole baseline on every PR to begin with, adding `--changed` when the
baseline grew large enough to hurt. *It depended on* the measured cost, which is why it dissolved
rather than being ruled.

**Q8 — Does it get a `schedule:` trigger?**
*Recommendation:* **no.** `docs.yml` argues this for `optioncheck` already: a gate over **pinned**
packages cannot have its verdict changed by anything outside this repository, so a scheduled run
repeats the last answer or fails because NuGet was briefly unreachable. **`versioncheck` already
owns the "a release happened" signal.** *Depends on:* Q9.

**Q9 — Pinned at `10.7.0`, or following the latest release?**
*Recommendation:* **pinned**, with the version in one place, bumped deliberately — the pages
themselves pin `10.7.0` and `versioncheck` already gates that. A gate that follows `latest` turns
somebody else's release into a red build on an untouched repository, which is how people learn to
ignore red builds. *Depends on:* nothing technical; it is an editorial choice about what the gate
is *for*.

## Workflow friction

The ledger stands at **51** — 015's `tasks.md` §4 holds 46–51, plus one deliberately unnumbered
recurrence. **016 adds 52 and 53.**

**52 — A control whose two halves measure different things reports a difference that is not
there.** Verifying that the rescued harness was byte-identical after a red-proof, the check was
`shasum blocks/*.cs | shasum` before and after. The two aggregates **differed with the content
unchanged**: `shasum` prints the path beside each digest, and the two runs were made from different
working directories, so the "before" hashed `core/blocks/…` and the "after" hashed `blocks/…`.
**The mirror image of a plausible zero** — it fails in the direction that wastes an hour rather
than the direction that ships a defect, which is why it is worth writing down rather than being
grateful for. Same family as friction 48: *the form that gets run is not the form that was
reviewed.* The honest check was `diff -r` against the untouched original, **with the `diff` itself
controlled** by appending one byte and confirming it was reported.

**53 — An extractor built the obvious way is silently blind to 15% of its corpus, and reports
success on the 85% it can see.** `grep -c '^```csharp$'` finds **835** blocks; `pagelint.Page`
finds **985**. The 150 are ` ``` csharp` with a space (140), indented inside a list item (8),
` ``` c#` (1), and one trailing space — **all four render as C# and all four are invisible to the
grep, along with 28 entire pages and the 92 blocks on them**, `ShowMeTheCode.md` among
them. The general form: **a tool that enumerates its own corpus owes
a second method for the size of that corpus**, because every verdict it prints is conditional on an
enumeration nothing checked. Obligation 1 already says *two methods that agree*; this is the case
where they **disagreed**, and the disagreement was the finding.

## Quality checklist, applied to this document

| | |
|---|---|
| Readable with no prior context | 015's closing sentence, the four defects and the gap are stated here rather than cited |
| P0 / P1 / P2 distinguished | eight P0, two P1, two P2, and **Q6 explicitly moves P1-1 between them** |
| Specific files, not gestures | `harness/extract.py:17`, `tools/pagelint.py`'s `FENCE_RE`, `tools/optioncheck/optioncheck.csproj`, `docs.yml`'s three jobs |
| A command beside every number | every figure in § *Current state* carries its command; the two gate figures are **cited from `tools/README.md`, not pasted** |
| Criteria name their instrument | 12 instrumented, of which **11 are deferred** — they name `tools/blockcheck.py`, which P0-1 builds — and **1, AC11, is runnable today and green**. **3 have no instrument, each with a named reader** — AC13, AC14, AC15. The bare count *"12 instrumented"* was the review's third finding |
| Questions carry recommendations | nine, each with a recommendation and what it depends on |
| It applies the checklist to itself | this table, and the README re-derivation that produced friction 53 |

---

**Next step: `/spec:review`.** Nine questions want ruling, and **Q1 and Q6 change what gets
built** — the green bar, and whether this spec edits published pages.
