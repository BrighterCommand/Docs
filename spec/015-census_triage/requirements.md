# Spec 015: Census Triage — Requirements

**Created:** 2026-09-17
**Status:** Awaiting review — run `/spec:review`

> **Every number here carries the command that produced it.** All figures measured 2026-09-17
> against Docs `master` `35b02e7`, `../Brighter` `origin/master` `09f5d988f`, `../Darker`
> `origin/master` `2f76cda`, both re-confirmed unmoved at the time of writing. **`--census` reads
> both products' `origin/master`, so its figures move when those move** — which is open question 5.

## Subject

**Process.** The deliverable is a triage method, plus the repairs to `--census` that method needs.
It is not a page.

Three sections are therefore **N/A**, named rather than left empty:

| Section | Why N/A |
|---|---|
| **SUMMARY.md changes** | A category error on a process spec. Nothing this spec ships is published under `contents/`, so there is nothing for the table of contents to carry. If P2 is ever taken up, the pages it repairs are already in `SUMMARY.md` |
| **Target audience** | The audience is whoever runs the triage — this programme. There is no reader segment to write for |
| **Mode mix** | Diátaxis applies to pages. There are none |

**Deliverables are likewise not pages**, so the "which of the four page types" rule does not bind
them. Said out loud because a deliverables list with no page types otherwise reads as an omission.

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
never moved; the number is not a constant, and this is why open question 5 exists.

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
sitting. **That asymmetry is the only opening a method has.**

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

1. `--census` no longer reports a page's own helper methods as unresolved APIs, and says in its
   output which product refs it resolved against.
2. A **written triage method** exists, with a stated **stopping condition** and a stated **slice**.
3. That method has been **executed once** over a bounded slice, and its verdicts are recorded per
   name with the evidence and the control behind each.
4. Names confirmed dead are `symbolwatch.tsv` rows — 014's machinery, unchanged — or carry a
   written reason they are not.
5. What 015 did **not** triage is stated as a number with a boundary, not left implied.

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
- `CLAUDE.md` — the ledger, rule 6 and the strict-scope rule that governs P2
- `.claude/commands/spec/new.md` — friction 41's subject

**Not read, and deliberately:** `contents/` pages are the *corpus*, not source material for a
process spec. They are read during P0-4, one name at a time, as the method directs.

## Scope

### P0 — the spec fails without these

| # | Item | Instrument |
|---|---|---|
| **P0-1** | **Restore the page-declared-member filter to `--census`**, at the candidates stage, with two-way controls whose positive case sits outside the enumeration (friction 36). Predict the movement before the change | `--census`; `probe/methodprobe.py` |
| **P0-2** | **`--census` prints the resolved product refs** — the SHAs, not the ref names — in its header, so any figure it produces is reproducible later | `--census` output contains the SHAs |
| **P0-3** | **The triage method, written down**, with its slice, its per-name evidence requirement, and its **stopping condition** | no instrument — read |
| **P0-4** | **Execute it over the ≥3-page slice: 112 names** after P0-1 | the triage record; `wc -l` against 112 |
| **P0-5** | **Every confirmed-dead name becomes a `symbolwatch.tsv` row or carries a written reason it does not** | `--verify-list`; row count moves by the number added |

**P0-4's slice, measured after P0-1's filter:**

| Slice | Names |
|---|---:|
| ≥7 pages | 38 |
| ≥5 pages | 60 |
| ≥4 pages | 70 |
| **≥3 pages** | **112** |
| ≥2 pages | 220 |
| 1 page (the tail) | 599 |

### P1 — wanted, and the spec still closes without them

| # | Item | Instrument |
|---|---|---|
| **P1-1** | **State how much of finding E's 808-name gap is recoverable.** 270 by methods; name what the remaining 538 are, or say the original filter set is unrecoverable and stop guessing | the staged table, re-derived |
| **P1-2** | **A written policy on the 599-name tail** — in or out, with the reason and the expected yield | no instrument — read |

### P2 — recorded, not scheduled

| # | Item | Why it is P2 |
|---|---|---|
| **P2-1** | **Repair the pages carrying a confirmed-dead name** | A repair edits a C# block, which pulls it into `CLAUDE.md` rule 6 strict scope under `--changed` and turns a triage pass into a compile job. 014's ruling 7 pulls the other way for anything small; open question 4 settles where the line is |

## Out of scope

- **Promoting `--census` to a gate.** Forbidden by its own docstring absent a measurement 015 does
  not expect to produce, and by D12's conclusion 1.
- **The BCL/ASP.NET resolution filter.** Measured, rejected, preserved as evidence (finding 3).
- **The 757 using-directive blocks.** The other half of the same problem and the only real
  instrument for it, but a separate body of work with its own budget.
- **Prose-surface census.** D12 conclusion 2: 160 of 161 pages carry an unresolved prose token, so
  the prose surface is unusable for a census and fine only for a curated watchlist.
- **Anything in `../Brighter` or `../Darker`.** Read-only, and nothing here needs a sample.

## Deliverables

Not pages — see *Subject*. Page types do not apply.

| # | File | What changes |
|---|---|---|
| 1 | `tools/symbolcheck.py` | P0-1's member filter, P0-2's ref header. **`--census` only** — the gate's behaviour is untouched, and AC6 is what proves it |
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
   the same breath** (obligation 7). P0-1 through P0-5 change nothing published; P2-1 would.
8. **`../Brighter` and `../Darker` are read-only.**

## Acceptance criteria

Numbered, each naming the command that decides it or saying it has none and who reads it. **Both
criteria ever found unmet at a close — 009's AC7 and 012's AC1 — were the unmarked ones**, so an
unmarked criterion below is a declared risk.

| # | Criterion | Instrument |
|---|---|---|
| **AC1** | `--census` **contains** no candidate that is declared as a method on every page using it | `probe/methodprobe.py` reports **0** removable after P0-1 — the probe becomes the red-proof |
| **AC2** | The census's candidate count moved from 929 by exactly the predicted amount, and the prediction was written **before** the change | `--census`; the prediction in `tasks.md`, dated, ahead of the commit |
| **AC3** | `--census` output **contains** the resolved SHA of each product ref it read | `python3 tools/symbolcheck.py --census \| grep -cE '[0-9a-f]{7,}'` ≥ 1 |
| **AC4** | The triage method is written down **and names its stopping condition** | *no instrument — read*, by the maintainer at `/spec:review`. A method with no stopping condition is a backlog |
| **AC5** | Every name in the ≥3-page slice has a verdict row | `grep -c` on `triage.md`'s table against the slice count P0-1 leaves (112 today; re-derive) |
| **AC6** | Each verdict cites evidence **and a control** | *no instrument — read.* **This is the criterion that fails quietly**: clearing a name because it "looks like an example" is the reasoning that left `ConfigureBrighter` unexamined |
| **AC7** | Every confirmed-dead name is a `symbolwatch.tsv` row or has a written reason it is not | `python3 tools/symbolcheck.py --verify-list` at 0 findings; row count moves by the number added |
| **AC8** | What 015 did **not** triage is stated as a number with its boundary | `grep` for the figure in `tasks.md`; it must carry its own command |
| **AC9** | The eight gates are at `tools/README.md`'s figures, or moved on purpose with that file changed and nowhere else | the eight commands in `tools/README.md` |
| **AC10** | No count in any document 015 ships is unanchored | *no instrument — read.* 014 found **three** unanchored counts in `CLAUDE.md`; 015 quotes more numbers than 014 did |

**On AC1's phrasing:** it says *contains no* rather than *ends with* or *is clean*, because what is
being tested is a property of the whole list. 013's AC7 said a guide *"ends with"* a verification
step where it meant *"contains"* one, and that is the failure this wording avoids.

**On AC5 not being "a file exists":** the row count is checked against a slice count derived from
the tool, so something consumes the artefact. 013's AC8 was checked by a row count in
`pagetypes.tsv` that nothing read, which made it unmarked in practice.

## Open questions

Each carries a recommendation and what it depends on.

**1. Does `--census` get the member filter, and at what breadth — methods only, or properties and
fields too?**
*Recommendation:* **methods now, the rest measured and recorded, not implemented.** The design's own
row says "types and members", so this is restoring a recorded filter rather than inventing one —
which is a stronger warrant than the one the README opened with. Methods are unambiguous to match
and account for 270 of the 808. Properties and fields need a regex that will over-match, and
over-matching in a *report a person reads* hides evidence.
*Depends on:* whether P1-1 finds the remaining 538 to be mostly members or mostly something else.

**2. Is the 599-name single-page tail in scope?**
*Recommendation:* **out of scope for 015, stated with its number rather than skipped.** A
single-page, single-site name is the least likely to be a real API and would dominate the budget.
But `IMessageScheduler` sat at 7 pages, not 1, so this is a claim about **expected yield**, not
impossibility.
*Depends on:* what P0-4 costs per name on the head. Measure there first, then rule.

**3. What is the triage unit — the name, or the page?**
*Recommendation:* **the name, ordered by page-spread.** It is the ordering that has found something
before. A page-ordered walk re-reads the same invented domain on all 31 pages carrying `OrderId`.
*Depends on:* nothing. Cheap to reverse.

**4. Does a confirmed-dead name get repaired on the page, or only listed?**
*Recommendation:* **listed in 015; repaired in a later phase.** 014's ruling 7 — *a defect found
beside a repair gets repaired* — should be honoured for anything genuinely small, so the practical
line is: **a name repairable without editing a C# fence gets repaired; one that needs the fence
edited gets listed.** That keeps rule 6's strict scope out of a triage pass.
*Depends on:* how many come back dead, unknown until P0-4 runs.

**5. How does `--census` become reproducible, given it reads two moving `origin/master` refs?**
*Recommendation:* **print the resolved SHAs in the header** (P0-2). The report already prints
token-set sizes per ref but never the refs' identities, so no figure it has ever produced can be
reproduced. This is the direct cause of 929 having been four numbers.
*Depends on:* nothing.

**6. Should the census pin `origin/master` instead of following it?**
*Recommendation:* **no — follow it, and print it.** A pinned census stops seeing names the products
removed last week, which is the whole point of reading master. Reproducibility is served by
recording the ref, not by freezing it.
*Depends on:* nothing, but it is worth ruling so nobody "fixes" the moving number later.

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

## Quality checklist, applied to this document

- [x] Readable with no prior context — 014's residual gap and D12's conclusion are quoted, not cited
- [x] P0 / P1 / P2 distinguished, with P2 marked *recorded, not scheduled*
- [x] Specific files named — `tools/symbolcheck.py`, `design.md` §3.2, `tasks.md` finding E
- [x] A command beside every number, or the words *no instrument — read*
- [x] Every acceptance criterion carries an instrument or is marked as having none, with a reader
- [x] Every open question carries a recommendation and what it depends on
- [x] The N/A sections are named and reasoned rather than left empty
