# Spec 015: Census Triage — Requirements

**Created:** 2026-09-17
**Status:** Reviewed 2026-09-18 — **six open questions ruled**, two acceptance criteria repaired at
the review. Awaiting `.requirements-approved`.

> **Two rulings overturned this document's own recommendation and both change what gets built:**
> **Q4 — confirmed-dead names are repaired in 015, not listed for later**, and **Q6 — `--census`
> pins its head refs rather than following them.** The recommendations are preserved below with the
> ruling beside each, because a recommendation that was overruled is evidence about how the spec
> was reasoned, not a draft to be tidied away.

> **Every number here carries the command that produced it.** All figures measured 2026-09-17
> against Docs `master` `35b02e7`, `../Brighter` `origin/master` `09f5d988f`, `../Darker`
> `origin/master` `2f76cda`, both re-confirmed unmoved at the time of writing — **and re-confirmed
> still unmoved at the review on 2026-09-18**, which is why 929 re-derived there against the same
> pair rather than by luck. **`--census` as shipped reads both products' `origin/master`, so its
> figures move when those move**; Q5 and Q6 ruled that it prints the SHAs and pins them, so a figure
> measured after P0-2 names its own world.

## Subject

**Process, with a corpus edit at the end of it.** The deliverable is a triage method, the repairs to
`--census` that method needs, and — **since Q4** — the repairs to the pages the method condemns.
**It creates no new page**, and that is a different claim from *it touches no page*.

> **Q4 changed this section, and the original sentence is worth keeping visible: *"It is not a
> page."*** That was true of a spec that listed dead names for someone else to act on. A spec that
> repairs them edits `contents/`, and three consequences follow that a process spec would otherwise
> have exempted itself from: the published site changes, so obligation 7 binds; the edited blocks
> enter rule 6's strict scope, so the compiler becomes an instrument; and **the N/A table below had
> to be re-reasoned rather than inherited.**

Two sections are **N/A**, named rather than left empty — one fewer than before the ruling:

| Section | Why N/A |
|---|---|
| **SUMMARY.md changes** | Still N/A, but **for a new reason**. It was *"nothing this spec ships is published"*, which Q4 falsified. It is now: P0-6 **edits pages that already exist and creates none**, and `SUMMARY.md` carries pages, not their contents. A repair that needed a new page would need this section back |
| **Mode mix** | Diátaxis applies to whole pages and to choosing what a page is for. P0-6 repairs a name inside a page whose type is already settled and which it must not change |

**Target audience is no longer N/A.** It was *"whoever runs the triage — this programme"*, which
held while the output was a list. P0-6's output is a corrected page, so **the audience of P0-6 is
the ordinary reader of the repaired page** — the same audience `CLAUDE.md` writes for, and the
reason this spec now has a reader on the other end at all. The audience for P0-1 to P0-5 is
unchanged and remains this programme.

**Deliverables 1 to 6 are not pages**, so the "which of the four page types" rule does not bind
them; **deliverable 0 is pages, and binds in the opposite direction** — P0-6 must not change any
repaired page's banner, type or opening sentence. Said out loud because a deliverables list with no
page types otherwise reads as an omission.

## Topic overview

014 built `symbolcheck`, repaired seventeen of eighteen defects, and named its own residual gap:

> **A dead API written into prose on an existing page — uncompiled, and not on `symbolwatch.tsv` —
> is caught by nothing.**

`--census` is the open-world report that can see such names. It prints **929** candidates across
**129** pages, and **nobody has read the list**. 014's D12 established that no cheap filter
separates the documentation's invented domain from a real API name, and `symbolcheck.py`'s own
docstring forbids promoting the census to a gate without a second measurement saying that problem
has gone away.

**015 does not expect to produce that measurement, and is not a gate.** Its question is:

> **Which slice of the census is worth a human triage pass, and by what method?**

## Current state

### The census, re-derived

```bash
python3 tools/symbolcheck.py --census
```

| Stage | Today | `design.md` §3.2 |
|---|---:|---:|
| Pages examined | 161 | 161 |
| …with at least one C# fence | 145 | 144 |
| Distinct PascalCase tokens in those fences | 2764 | 2,762 |
| …after comments and string literals | 2280 | 2,283 |
| …after page-declared **types** and noise | **2019** | **1,211** ← *"types **and members**"* |
| **Unresolved at `src/` of both products, both refs** | **929** | **831** |
| Pages carrying at least one | **129 of 145** | 127 of 144 |

**Three methods agree on 929** — the tool's `UNRESOLVED` line, its closing `N candidate(s)` line,
and counting the printed rows:

```bash
python3 tools/symbolcheck.py --census > /tmp/census.txt
awk '/^by page-spread/{f=1;next} /^[0-9]+ candidate/{f=0} f && NF' /tmp/census.txt \
  | awk '{print $5}' | sort -u | wc -l      # 929
```

**929 is the fourth number this idea has measured** — 831 → 881 → 932 → 929. The conclusion has
never moved; the number is not a constant, and that is what Q5 and Q6 were asked about. **929
re-derived unchanged at the review**, by both methods, against the same SHA pair — which is
evidence that the instrument is stable, not that the figure is safe to inherit. After P0-2 a census
figure carries the SHAs that produced it and the distinction stops mattering.

### The shape of the list

```bash
awk '/^by page-spread/{f=1;next} /^[0-9]+ candidate/{f=0} f && NF' /tmp/census.txt > rows.txt
awk '{print $1}' rows.txt | sort -n | uniq -c
```

| Page-spread | Candidates |
|---:|---:|
| 1 page | **696** (431 of them at a single site) |
| 2 pages | 118 |
| 3 pages | 42 |
| 4–6 pages | 33 |
| ≥7 pages | **40** |

Three quarters of the list is the single-page tail; the head is small enough to read in one
sitting.

> **That asymmetry was written as *"the only opening a method has"*, and the design closed a
> different opening instead.** The shape argument assumed triage was a *reading* cost, so the only
> way in was to read less. The method that shipped makes the first stage **mechanical** — 0.4s a
> name — which removes the need to choose a slice at all, and Q2 was reversed on exactly that.
> **The asymmetry is still real; it stopped being the constraint.**

### Finding 1 — the scoping filter the design recorded was lost, and this is a regression

014's **finding E** records that the probe behind `design.md` §3.2 *"does not exist"* — its filter
set was never written to disk — and that the reconstruction lands ~6% high because *"the scoping
stage that is easiest to under-specify is the one that moves most"* (1,940 against a recorded
1,211).

**§3.2's own row label says what that stage did: "scoping out page-declared types *and members*".**
`--census` as shipped scopes types only: `DECL_RE` matches `class|interface|record|struct|enum` and
nothing else. So a filter the approved design records is absent from the instrument built from it.

Measured at the design's own stage, restoring a method-declaration filter moves **2019 → 1749**,
removing **270**:

```bash
python3 spec/015-census_triage/probe/methodprobe.py     # 929 -> 819 at the unresolved stage
```

| Stage | Count |
|---|---:|
| after page-declared types and noise (shipped) | 2019 |
| …also scoping page-declared **methods** | **1749** |
| the design's recorded figure | **1,211** |

**It closes 270 of the 808-name gap and leaves 538.** So *"members"* in the original probe meant
more than methods — properties, fields and locals are the candidates — and **015 can state how much
of finding E's gap is recoverable instead of leaving it as a ~6% shrug.** That is P1-1.

The worked case is the highest-spread Brighter-shaped name in the entire census:

```bash
grep -rl 'ConfigureBrighter' contents/ | wc -l                                          # 14 pages
grep -ron 'ConfigureBrighter' contents/ | wc -l                                         # 36 sites
grep -rn  'ConfigureBrighter' contents/ | grep -c 'private static void'                 # 21 declarations
git -C ../Brighter grep -l -F 'ConfigureBrighter' origin/master -- 'src/*.cs' | wc -l   # 0
git -C ../Brighter grep -l -F 'AddBrighter'       origin/master -- 'src/*.cs' | wc -l   # 11, the control
```

`ConfigureBrighter` is the reader's *own* private helper, mirroring `samples/WebAPI/*/Startup.cs`.
Every page using it also declares it. It has sat at the top of the census looking exactly like a
dead Brighter API.

### Finding 2 — the top 40 by page-spread contains no live Brighter API

*No instrument — read.* The head is the invented domain (`OrderId` 31 pages, `GreetingEvent` 18,
`CustomerId` 17, `MyCommand` 15), bare BCL and ASP.NET names (`HostBuilderContext` 14,
`WebApplication` 11, `AsNoTracking` 9), and page-declared helpers.

**This is not an argument against page-spread ordering** — that ordering is what found
`IMessageScheduler` at 7 pages, 014's one real discovery. It measures what the ordering costs
before it pays, which is what P0-4's budget has to be set from.

### Finding 3 — a BCL resolution filter buys precision by hiding evidence

```bash
python3 spec/015-census_triage/probe/bclprobe.py        # 929 -> 818
```

Resolving candidates against the .NET 8 ref-pack XML removes **111** — and among them `Date`,
`Email`, `Total`, `Product`, `Cancelled`, `Country`: the documentation's own invented domain, struck
because `System.DateTime.Date` contributes the bare token `Date`.

**The generalisation, and it is D12's finding in a new suit:** every cheap filter available here is
a **name** filter, and names collide. The census resolves *tokens*, not *references*. **Only a
compiler resolves a reference** — which is why the 757 using-directive blocks are the other half of
this problem, and why triage has a human at its boundary by construction rather than by
under-investment.

The probe is preserved **as evidence against adopting it**, in `probe/` and deliberately not in
`tools/`.

## Target state

1. `--census` no longer reports a page's own helper methods as unresolved APIs, and **prints the
   SHA of every ref it resolved** — including the **pinned** head refs (Q6), so any figure it
   produces can be reproduced later by anyone with the same two checkouts.
2. A **written triage method** exists, with a stated **stopping condition** and a stated **slice**.
3. That method has been **executed once** over a bounded slice, and its verdicts are recorded per
   name with the evidence and the control behind each.
4. Names confirmed dead are `symbolwatch.tsv` rows — 014's machinery, unchanged — or carry a
   written reason they are not.
5. **The pages carrying a confirmed-dead name are repaired** (Q4), each repaired block building
   against the released packages, or the name carries a written reason it stays.
6. What 015 did **not** triage is stated as a number with a boundary, not left implied.

## Source material

**Read for this document, and cited above:**

- `tools/symbolcheck.py` — `--census`'s implementation and its docstring's own prohibition on
  becoming a gate (lines 316–340), `DECL_RE`, `NOISE_PREFIX`, `NOISE_EXACT`, the two-way controls
- `spec/014-documentation_workflow/design.md` §3 — D12 as executed, §3.2's table and its three
  conclusions, §3.4's `IMessageScheduler` discovery
- `spec/014-documentation_workflow/tasks.md` — the re-derivation table (831 → 881), **finding E**
  on the lost filter set, the defect ledger, friction 28
- `spec/014-documentation_workflow/README.md` — the residual-gap sentence 015 exists to answer
- `tools/README.md` — D10, the eight gates and their expected figures, the exit-code contract
- `CLAUDE.md` — the ledger, rule 6 and the strict-scope rule that now governs **P0-6**
- `.claude/commands/spec/new.md` — friction 41's subject

**Not read, and deliberately:** `contents/` pages are the *corpus*, not source material for a
process spec. They are read during P0-4, one name at a time, as the method directs — **and edited
during P0-6**, which Q4 added and which is the only part of 015 that writes to them.

## Scope

### P0 — the spec fails without these

| # | Item | Instrument |
|---|---|---|
| **P0-1** | **Restore the page-declared-member filter to `--census`**, at the candidates stage, with two-way controls whose positive case sits outside the enumeration (friction 36). Predict the movement before the change | `--census`; `probe/methodprobe.py` |
| **P0-2** | **`--census` pins its head refs and prints every resolved SHA** — the SHAs, not the ref names — in its header, so any figure it produces is reproducible later. **Census path only**: `PRODUCT_REFS` is shared with the gate, and the gate keeps following `origin/master` | the header's SHAs against `git rev-parse`, four of four |
| **P0-3** | **The triage method, written down**, with its slice, its per-name evidence requirement, and its **stopping condition** | no instrument — read |
| **P0-4** | **Execute it over the WHOLE census** — every candidate P0-1 leaves, not a slice (Q2, reversed). Ordered by page-spread, because that ordering is what has found something before | the triage record; `wc -l` against the count `--census` reports |
| **P0-5** | **Every confirmed-dead name becomes a `symbolwatch.tsv` row or carries a written reason it does not** | `--verify-list`; row count moves by the number added |
| **P0-6** | **Repair the pages carrying a confirmed-dead name** (Q4), bounded by what P0-4 finds in the ≥3-page slice. A repaired C# block enters `pagelint` rule 6 **strict scope** under `--changed`, so it needs its `using` directives or a declared `// ...` omission, and it **compiles against the released packages** before it ships | `pagelint --changed origin/master`; a build of each repaired block; `symbolcheck` |

**P0-4's corpus after P0-1's filter is the whole census.** The bands below are kept because they
are the **ordering** the triage walks and the shape of the work, not a scope boundary any more —
and because two of the figures moved when the design settled *which* member filter ships, which is
[friction 43](#workflow-friction):

| Band | Names, all-or-nothing (requirements) | Names, **per page** (design, and what ships) |
|---|---:|---:|
| ≥7 pages | 38 | **36** |
| ≥5 pages | 60 | **57** |
| ≥4 pages | 70 | **65** |
| ≥3 pages | 112 | **103** |
| ≥2 pages | 220 | **210** |
| 1 page | 599 | **609** |
| **total — P0-4's corpus** | **819** | **819** |

**Both filters total 819 and the bands differ**, which is why the total was the wrong number to
budget from. `design.md` §2 carries the measurement and the command.

### P1 — wanted, and the spec still closes without them

| # | Item | Instrument |
|---|---|---|
| **P1-1** | **State how much of finding E's 808-name gap is recoverable.** 270 by methods; name what the remaining 538 are, or say the original filter set is unrecoverable and stop guessing | the staged table, re-derived |
| **P1-2** | ~~A written policy on the 599-name tail~~ — **withdrawn when Q2 was reversed.** The tail is triaged, so there is no policy to write about not triaging it. Kept struck through rather than deleted, because a deliverable that vanishes without trace looks like one that was forgotten | — |

### P2 — empty

**P2-1 was *repair the pages carrying a confirmed-dead name*, and Q4 ruled it into scope as P0-6.**
The section is kept rather than deleted so that the promotion is visible: the reason it had been
P2 — that editing a C# fence pulls the block into rule 6 strict scope and turns a triage pass into
a compile job — **is not an objection that went away**. It is now a cost P0-6 carries, which is why
P0-6 names the compiler as an instrument and not just `pagelint`.

**This is the ruling that makes 015 touch the published site.** P0-1 through P0-5 change nothing a
reader sees; P0-6 changes `contents/` pages, so standing obligation 7 now binds on its pull request
— ask before merging, and name the head-ref deletion in the same breath.

## Out of scope

- **Promoting `--census` to a gate.** Forbidden by its own docstring absent a measurement 015 does
  not expect to produce, and by D12's conclusion 1.
- **The BCL/ASP.NET resolution filter.** Measured, rejected, preserved as evidence (finding 3).
- **The 757 using-directive blocks.** The other half of the same problem and the only real
  instrument for it, but a separate body of work with its own budget. **One carve-out, forced by
  Q4:** a block P0-6 *edits* is in `--changed`'s strict scope and must carry its directives or
  declare the omission, so 015 repairs the directives in the blocks it touches and **only** those.
  Expect the repo-wide 757 to fall by a small number and **say which blocks moved it** — a warning
  count that drops for unexplained reasons is how debt figures stop meaning anything.
- ~~The 599-name single-page tail (Q2)~~ — **no longer out of scope.** Q2 was reversed at the
  design review once its *depends on* was measured; the tail is **609 names** under the filter that
  ships and P0-4 triages it. Struck through rather than deleted so the reversal is visible from the
  section it used to sit in.
- **Prose-surface census.** D12 conclusion 2: 160 of 161 pages carry an unresolved prose token, so
  the prose surface is unusable for a census and fine only for a curated watchlist.
- **Anything in `../Brighter` or `../Darker`.** Read-only, and nothing here needs a sample.

## Deliverables

Not pages — see *Subject*. Page types do not apply.

| # | File | What changes |
|---|---|---|
| 0 | `contents/*.md` | **P0-6's repairs** (Q4) — the pages carrying a confirmed-dead name, unknown until P0-4 runs. The only deliverable a reader sees, and the only one that needs a merge ask |
| 1 | `tools/symbolcheck.py` | P0-1's member filter, P0-2's pinned refs and SHA header. **`--census` only** — `PRODUCT_REFS` is shared, so the pin is census-scoped or the daily `versions` job starts measuring a frozen world. The gate's behaviour is untouched, and AC6 is what proves it |
| 2 | `tools/README.md` | the `symbolcheck` row, **only if** a gate figure moves. D10 is the single place a gate number changes |
| 3 | `spec/015-census_triage/triage.md` | the method (P0-3) and the executed record (P0-4), one row per name: verdict, evidence, control |
| 4 | `tools/symbolwatch.tsv` | P0-5's rows, if any |
| 5 | `spec/015-census_triage/probe/*.py` | already shipped in #167; `bclprobe.py` stays as the rejected-filter evidence |
| 6 | `spec/015-census_triage/tasks.md` | phases, findings, the friction ledger from 41 |

## Constraints

1. **`--census` must not become a gate**, and must keep **exit 0 whatever it finds**. An exit code
   that varies with findings is the first step towards someone wiring it into CI.
2. **Exit 2 stays "nothing was checked"** — the `tools/README.md` contract, shared with every gate.
3. **The two-way controls stay, and any new filter gets one** whose positive case sits **outside**
   the enumeration it is built from (friction 36).
4. **Predict gate movement before the work, including "none", and say why none** (standing
   obligation 6).
5. **Re-derive every count before quoting it**, command beside the figure, two methods agreeing
   (standing obligation 1) — and **record the mismatch before fixing it** (obligation 2).
6. **A gate number changes in `tools/README.md` and nowhere else.**
7. **Ask before merging anything that changes the published site, and name the head-ref deletion in
   the same breath** (obligation 7). P0-1 through P0-5 change nothing published; **P0-6 does**, so
   its pull request carries the ask.
8. **`../Brighter` and `../Darker` are read-only.**
9. **The pin is census-scoped** (Q6). `PRODUCT_REFS` at `tools/symbolcheck.py:103–106` feeds both
   `--census` and the gate, and the gate's `--verify-list` runs on a daily `schedule:` precisely
   because what invalidates a watchlist row is a removal in *another* repository. A pin applied to
   both would freeze the one gate whose whole purpose is to notice the world moving.
10. **Refreshing the pin is a commit, with the new count beside it.** A pin updated silently is a
    moving number wearing a fixed number's clothes, which is worse than following `origin/master`
    openly.

## Acceptance criteria

Numbered, each naming the command that decides it or saying it has none and who reads it. **Both
criteria ever found unmet at a close — 009's AC7 and 012's AC1 — were the unmarked ones**, so an
unmarked criterion below is a declared risk.

| # | Criterion | Instrument |
|---|---|---|
| **AC1** | `--census` **contains** no candidate that is declared as a method on every page using it | `probe/methodprobe.py` reports **0** removable after P0-1 — the probe becomes the red-proof |
| **AC2** | The census's candidate count moved from 929 by exactly the predicted amount, **both figures measured at the same resolved SHA pair**, and the prediction was written **before** the change | `--census` before and after, with P0-2's header showing the same SHAs on both runs; the prediction in `tasks.md`, dated, ahead of the commit |
| **AC3** | `--census`'s header prints, **for each of the four refs it resolves**, the SHA git resolves it to — and each printed SHA **equals** what `git rev-parse` returns for that ref | `--census \| head -6` matched against `git -C ../Brighter rev-parse origin/master` and the three others: **0 of 4 today, 4 of 4 after P0-2** |
| **AC4** | The triage method is written down **and names its stopping condition** | *no instrument — read*, by the maintainer at `/spec:review`. A method with no stopping condition is a backlog |
| **AC5** | **Every name in the census** has a verdict row — not a slice (Q2, reversed) | `grep -c` on `triage.md`'s table against the count `--census` reports (**819** today; re-derive, and re-derive the **bands** too, not just the total — they are what moved) |
| **AC6** | Each verdict cites evidence **and a control** | *no instrument — read.* **This is the criterion that fails quietly**: clearing a name because it "looks like an example" is the reasoning that left `ConfigureBrighter` unexamined |
| **AC7** | Every confirmed-dead name is a `symbolwatch.tsv` row or has a written reason it is not | `python3 tools/symbolcheck.py --verify-list` at 0 findings; row count moves by the number added |
| **AC8** | What 015 did **not** triage is stated as a number with its boundary. **Q2's reversal did not retire this criterion, it moved it**: the untriaged set is no longer the tail but the surfaces the census never reaches — the **110** names P0-1's filter removes as page-declared members, the tokens `NOISE_EXACT` and `NOISE_PREFIX` drop, and the **prose** surface D12 ruled unusable | `grep` for each figure in `tasks.md`; each must carry its own command |
| **AC9** | The eight gates are at `tools/README.md`'s figures, or moved on purpose with that file changed and nowhere else | the eight commands in `tools/README.md` |
| **AC10** | No count in any document 015 ships is unanchored | *no instrument — read.* 014 found **three** unanchored counts in `CLAUDE.md`; 015 quotes more numbers than 014 did |
| **AC11** | Every confirmed-dead name in the slice is **gone from `contents/`**, or its remaining sites carry a written opt-out (P0-6) | `grep -rn '<name>' contents/` per name, against the triage record; `symbolcheck` prints the opt-out count, and *0 findings, N silenced* is a different claim from *0 findings* |
| **AC12** | Every C# block P0-6 edited **builds against the released packages**, and any block left deliberately incomplete says so with `// ...` | the build, per block; `python3 tools/pagelint.py --changed origin/master` at **0 errors** — rule 6 is error-level on a block the diff touches |

**On AC1's phrasing:** it says *contains no* rather than *ends with* or *is clean*, because what is
being tested is a property of the whole list. 013's AC7 said a guide *"ends with"* a verification
step where it meant *"contains"* one, and that is the failure this wording avoids.

**On AC5 not being "a file exists":** the row count is checked against a slice count derived from
the tool, so something consumes the artefact. 013's AC8 was checked by a row count in
`pagetypes.tsv` that nothing read, which made it unmarked in practice.

### What AC2 and AC3 said before the review, and why they changed

**Recorded before it was fixed** — standing obligation 2 — because a criterion repaired silently
leaves no evidence that the review did anything.

**AC3 was `--census | grep -cE '[0-9a-f]{7,}'` ≥ 1, and it passed on 2026-09-18 with the feature
absent.** The census prints ref *names* and no SHAs; the pattern's single match in the whole report
was **`cceeded`, inside the candidate name `Succeeded`** on `HangfireScheduler.md`. So the
criterion was green before the work, could not go red, and would have been signed off by P0-2
shipping nothing.

```bash
python3 tools/symbolcheck.py --census | grep -nE '[0-9a-f]{7,}'
#   1 page(s)  1 site(s)  Succeeded  HangfireScheduler.md
```

Two things this is an instance of, and neither is new friction:

- **014's AC5 exactly** — *"`/spec:review` runs eight gates"*, measured by a grep returning 5.
  Friction 39 was raised for it, the step was shipped in #164, and **this is that step's first
  catch after shipping**. A rule that has now fired is worth more than a rule that has only been
  argued for.
- **Friction 30's family** — the instrument's own text entering the instrument's corpus. AC3's
  corpus was the candidate list the report prints, so the *body* of the report could satisfy a
  claim about its *header*. An instrument that reads the whole of a document to check one line of
  it will eventually be answered by the wrong line.

**The four SHAs AC3 now checks against**, each with the command, measured 2026-09-18:

```bash
git -C ../Brighter rev-parse --short origin/master   # 09f5d988f   <- pinned by Q6
git -C ../Brighter rev-parse --short 10.7.0          # c1b8af886
git -C ../Darker   rev-parse --short origin/master   # 2f76cda     <- pinned by Q6
git -C ../Darker   rev-parse --short 4.1.1           # ddb71ee
```

The tags carry **no `v` prefix** — `v10.7.0` is `fatal: Needed a single revision` in both
repositories — which is the sort of thing that costs an hour when P0-2 is being written and nothing
recorded it. Two of the four are already immutable; **Q6's pin is about the other two**, and all
four get printed because a header that prints some of its refs invites the reader to assume the
rest.

**AC2 was unmeasurable while the refs moved.** It asks that the count move "by exactly the
predicted amount", but until Q6 the census followed two `origin/master` refs, so an upstream merge
between prediction and measurement moved the count for reasons unrelated to P0-1 — or moved it
back, two errors cancelling into a pass. AC2 now requires both runs at the same SHA pair, which
**makes AC2 depend on AC3**. That dependency was always real and was not stated.

## Open questions — all six ruled at `/spec:review`, 2026-09-18

**Four rulings took the recommendation; two overturned it.** Each question keeps its original
recommendation and its *depends on*, with the ruling beside it, because the reasoning is the
evidence about how this spec was decided — and because an overturned recommendation is the more
useful half of the record.

| # | Subject | Ruling | Took the recommendation? |
|---|---|---|---|
| 1 | Member filter breadth | methods now; properties and fields measured, not implemented | yes |
| **2** | **The 599-name tail** | **IN — the whole census is triaged.** Ruled out 2026-09-18, **reversed the same day at the design review** once the measurement it depended on existed | **no — overturned, twice** |
| 3 | Triage unit | the name, ordered by page-spread | yes |
| **4** | **Repair or list?** | **repair** | **no — overturned** |
| 5 | Census reproducibility | print the resolved SHAs | yes |
| **6** | **Pin or follow?** | **pin** | **no — overturned** |

> **Q2 is the one that worked exactly as a *depends on* is supposed to.** It was ruled out on an
> explicit condition — *"what P0-4 costs per name on the head. Measure there first, then rule"* —
> the design measured it, the measurement contradicted the assumption the ruling rested on, the
> design **reported it rather than acting on it**, and the maintainer reversed the ruling. **Three
> of six recommendations are now overturned, and none of the three was overturned by argument.**

**1. Does `--census` get the member filter, and at what breadth — methods only, or properties and
fields too?**
*Recommendation:* **methods now, the rest measured and recorded, not implemented.** The design's own
row says "types and members", so this is restoring a recorded filter rather than inventing one —
which is a stronger warrant than the one the README opened with. Methods are unambiguous to match
and account for 270 of the 808. Properties and fields need a regex that will over-match, and
over-matching in a *report a person reads* hides evidence.
*Depends on:* whether P1-1 finds the remaining 538 to be mostly members or mostly something else.
**RULED: accepted.** P0-1 is methods only; the breadth question stays open as a measurement in
P1-1, not as code.

**2. Is the 599-name single-page tail in scope?**
*Recommendation:* **out of scope for 015, stated with its number rather than skipped.** A
single-page, single-site name is the least likely to be a real API and would dominate the budget.
But `IMessageScheduler` sat at 7 pages, not 1, so this is a claim about **expected yield**, not
impossibility.
*Depends on:* what P0-4 costs per name on the head. Measure there first, then rule.

**RULED 2026-09-18, morning: keep it out.** The tail stays out of 015 and P1-2 writes the policy
rather than the work.

**REVERSED 2026-09-18, at the design review: the whole census is triaged.** The recommendation
rested on *"431 of them would dominate the budget"*, and the design measured the budget instead of
assuming it: the mechanical stage costs **0.4s per name per product**, so the tail is minutes, not
a grind. **The recommendation was not wrong about the yield — it was wrong about the cost**, and
those are separable. The expected yield of a single-page single-site name is still low; it is
simply no longer a reason to skip it.

**What this changes:** P0-4 covers **all 819 census candidates**, not the 112-then-103 slice;
**P1-2's tail policy disappears** as a deliverable, because a policy about what you did not do is
not needed once you have done it; AC5 counts verdict rows against the whole census; and AC8's
*"what 015 did not triage"* stops being the tail and becomes the surfaces the census itself never
reaches.

**3. What is the triage unit — the name, or the page?**
*Recommendation:* **the name, ordered by page-spread.** It is the ordering that has found something
before. A page-ordered walk re-reads the same invented domain on all 31 pages carrying `OrderId`.
*Depends on:* nothing. Cheap to reverse.
**RULED: accepted.** The triage unit is the name, ordered by page-spread.

**4. Does a confirmed-dead name get repaired on the page, or only listed?**
*Recommendation:* **listed in 015; repaired in a later phase.** 014's ruling 7 — *a defect found
beside a repair gets repaired* — should be honoured for anything genuinely small, so the practical
line is: **a name repairable without editing a C# fence gets repaired; one that needs the fence
edited gets listed.** That keeps rule 6's strict scope out of a triage pass.
*Depends on:* how many come back dead, unknown until P0-4 runs.
**RULED: OVERTURNED — repair.** A confirmed-dead name gets repaired on the page in 015, not listed
for a later spec. P2-1 becomes **P0-6**, and the recommendation's own objection survives as that
item's cost rather than as a reason to defer: editing a C# fence puts the block in rule 6's strict
scope under `--changed`, so **AC12** makes the compiler an instrument alongside `pagelint`. The
practical line the recommendation drew — *repair what needs no fence edited, list the rest* — is
**gone**: both get repaired, and a name that stays does so behind a written opt-out that
`symbolcheck` prints and counts.

**This is 014's ruling 7 applied at the scale of a spec** — *a defect found beside a repair gets
repaired* — and it is the ruling that gives 015 a reader on the other end. It also makes P0-6 the
only unbounded item here, which is why it is bounded by P0-4's slice and by nothing else.

**5. How does `--census` become reproducible, given it reads two moving `origin/master` refs?**
*Recommendation:* **print the resolved SHAs in the header** (P0-2). The report already prints
token-set sizes per ref but never the refs' identities, so no figure it has ever produced can be
reproduced. This is the direct cause of 929 having been four numbers.
*Depends on:* nothing.
**RULED: accepted**, and strengthened by AC3's repair — the printed SHA is checked against
`git rev-parse` rather than merely being present, because a SHA-shaped string is not a SHA.

**6. Should the census pin `origin/master` instead of following it?**
*Recommendation:* **no — follow it, and print it.** A pinned census stops seeing names the products
removed last week, which is the whole point of reading master. Reproducibility is served by
recording the ref, not by freezing it.
*Depends on:* nothing, but it is worth ruling so nobody "fixes" the moving number later.
**RULED: OVERTURNED — pin**, in the form that answers the recommendation's objection rather than
ignoring it. **The census resolves a SHA recorded in the repository** — refreshed by a deliberate
commit with the new count beside it — **not the release tags alone.** The objection was aimed at
the tags-only form, which would stop the census seeing anything removed since the release; that is
the case `IMessageScheduler` came from, and it stays visible under a recorded pin because the pin
still points into master's history.

```python
CENSUS_PINS = {
    'brighter': '09f5d988f',   # origin/master @ 2026-09-16
    'darker':   '2f76cda',     # origin/master @ 2026-09-16
}
```

**Census-scoped, and this is the part that bites.** `PRODUCT_REFS` at `tools/symbolcheck.py:103–106`
feeds both `--census` and the gate, and `--verify-list` runs on a daily `schedule:` **because what
invalidates a watchlist row is a removal in another repository.** A pin applied to the shared
constant would freeze the one gate whose entire purpose is to notice the world moving — a plausible
zero of the eighth kind, arriving as a green daily run that had stopped looking. Constraint 9
carries this and AC6 is what proves the gate's behaviour is untouched.

## Workflow friction

41. **A `!` block in a slash command inherits the session's shell cwd, and under `bash` a
    cwd-relative glob that matches nothing returns empty output with no error.** `/spec:new`'s block
    is `ls -d spec/*/ 2>/dev/null`; from a directory with no `spec/`, `zsh` fails loudly with
    `no matches found` — which is what happened on 2026-09-17 and what prevented the failure —
    while `bash` returns `rc=1` and **nothing on stdout**, and `2>/dev/null` cannot suppress a
    shell-level glob error either way. Step 1 then says *"take the next ID from the listing above"*,
    and an empty listing reads as **"no specs exist"**, so the next ID is `001` and `mkdir -p`
    creates a second `spec/001-*` without complaint. **Plausible-zero number seven, and the first in
    a *command* rather than an instrument.**

42. **A filter recorded in an approved design was absent from the instrument built from it, and the
    design's own reconstruction attributed the gap to vagueness rather than to a missing feature.**
    014's §3.2 says "page-declared types *and members*"; `--census` scopes types only; finding E
    called the 6% discrepancy *"the signature of a thinner scoping filter"* and stopped there. It
    was recoverable — 270 of 808 — by reading the design's row label as a specification.
    **A design's table row is a claim about the instrument, and nothing checks that the instrument
    still makes it.** Same family as friction 37 — `CLAUDE.md` and the commands depending on each
    other with a step in only one direction — one level up: **the design and the tool.**

**The review met no friction 43, and that is worth saying rather than leaving as a gap.** The
review's one real catch — AC3 green before its feature existed — is **friction 39 working**, on its
first outing since #164 shipped the step, and its mechanism is friction 30's: the instrument's own
text inside the instrument's corpus. Minting a number for a rule that fired as designed would
inflate the ledger and hide the more useful fact, which is that **a step added at one spec's
acceptance walk caught a defect at the next spec's requirements review.**

The ledger therefore stands at **42 entries, 18–42, contiguous**, and the command that re-derives it
is in `PROMPT.md` beside 014's, extended by this document's 41 and 42.

## Quality checklist, applied to this document

- [x] Readable with no prior context — 014's residual gap and D12's conclusion are quoted, not cited
- [x] P0 / P1 / P2 distinguished, with P2 marked *recorded, not scheduled*
- [x] Specific files named — `tools/symbolcheck.py`, `design.md` §3.2, `tasks.md` finding E
- [x] A command beside every number, or the words *no instrument — read*
- [x] Every acceptance criterion carries an instrument or is marked as having none, with a reader
- [x] Every open question carries a recommendation and what it depends on
- [x] The N/A sections are named and reasoned rather than left empty
- [x] **Every acceptance criterion's instrument was run at the review**, and its output checked
      against what the criterion claims — friction 39's step. Two failed that check and were
      repaired, with the failure recorded above rather than edited away
- [x] All six open questions carry a ruling, dated, with the overturned recommendations preserved
