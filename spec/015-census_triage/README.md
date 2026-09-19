# Spec 015: Census Triage

**Created:** 2026-09-17
**Status:** **Writing complete, 41 of 41, 2026-09-19 — the acceptance walk is read and
`.accepted` is the maintainer's to create.**

> **Read `requirements.md` and `tasks.md`, not this file, on every content claim.** This README was
> written before anyone looked; its figures are 2026-09-17's, its acceptance criteria are a first
> draft that `requirements.md`'s twelve superseded, and **its open questions were superseded twice**
> — see *Superseded by `requirements.md`* below. What is current here is the *Status Checklist* and
> everything after it.

> **Re-derive this README before executing it.** It was written before anyone looked — check every
> count and every named gap against the tree, with the command beside the figure.

## Topic Overview

Spec 014 built `symbolcheck`, repaired seventeen of eighteen defects, and named what it could not
reach in its own closing sentence:

> **A dead API written into prose on an existing page — uncompiled, and not on `symbolwatch.tsv` —
> is caught by nothing.**

`--census` is the open-world report that can see those names. It prints **929** candidates across
**129** pages and **nobody has read the list**. 014's D12 established that no cheap filter separates
the documentation's own invented domain from a real API name, so **015 cannot be a gate** — the
census's own docstring forbids promoting it to one without a second measurement saying the
example-domain problem has gone away, and this spec does not expect to produce that.

So the question is the one 014 left, and it is about method rather than about a number:

> **Which slice of 929 candidates across 129 pages is worth a human triage pass, and by what
> method?**

## Subject

**process.**

The deliverable is a triage method, plus whatever change to the instrument the method needs. It is
not a page. Two clarifications, because the classification decides which requirements sections
apply:

- Its **output** serves a reader problem — a dead API in a printed example is a reader pasting
  something that does not compile. Names confirmed dead become `symbolwatch.tsv` rows, which is
  014's machinery and needs nothing new.
- Its **work** is method: deciding what a person reads, in what order, and what they are allowed to
  conclude from a name that does not resolve.

## Current State

**All figures measured 2026-09-17**, against Docs `master` at `35b02e7`, `../Brighter`
`origin/master` at `09f5d988f`, `../Darker` `origin/master` at `2f76cda`.

**The census reads both products' `origin/master`, so its number moves when they move.** Record the
product refs beside any figure quoted from it; a bare "929" is not reproducible.

### The census, re-derived

```bash
python3 tools/symbolcheck.py --census
```

| | |
|---|---:|
| pages examined | 161 |
| …with at least one C# fence | 145 |
| distinct tokens in those fences | 2764 |
| …after comments and strings | 2280 |
| …after page declarations, noise | 2019 |
| **UNRESOLVED at `src/` of both products, both refs** | **929** |
| pages carrying at least one | **129 of 145** |

**Three methods agree on 929** — the tool's `UNRESOLVED` line, its closing `N candidate(s)` line,
and counting the printed rows independently:

```bash
python3 tools/symbolcheck.py --census > /tmp/census.txt
awk '/^by page-spread/{f=1;next} /^[0-9]+ candidate/{f=0} f && NF' /tmp/census.txt \
  | awk '{print $5}' | sort -u | wc -l       # 929
```

**929 is the fourth number this idea has measured** — 831 → 881 → 932 → 929, across four filter
sets. The conclusion has never moved. The number is not a constant.

### The shape of the list — this is what a method has to be built around

```bash
awk '/^by page-spread/{f=1;next} /^[0-9]+ candidate/{f=0} f && NF' /tmp/census.txt > rows.txt
awk '{print $1}' rows.txt | sort -n | uniq -c        # the distribution
awk '$1>=7' rows.txt | wc -l                         # 40
awk '$1==1 && $3==1' rows.txt | wc -l                # 431
```

| Page-spread | Candidates |
|---:|---:|
| 1 page | **696** |
| 2 pages | 118 |
| 3 pages | 42 |
| 4–6 pages | 33 |
| ≥7 pages | **40** |

- **696 of 929 sit on exactly one page**, and **431** of those at a single site.
- **115** sit on ≥3 pages; **40** on ≥7.

The tail is three quarters of the list and the head is small enough to read in one sitting. That
asymmetry is the opening a method has; whether the tail is worth anything is open question 2.

### Three things measured here that D12 did not try

**1. The top 40 by page-spread contains no live Brighter API — none.** Checked by reading the list.
It is the invented domain (`OrderId` 31 pages, `GreetingEvent` 18, `CustomerId` 17, `MyCommand` 15),
bare BCL and ASP.NET names (`HostBuilderContext` 14, `WebApplication` 11, `AsNoTracking` 9), and
page-declared helpers. **Page-spread is the ordering that found `IMessageScheduler` at 7 pages**, so
this is not an argument against the ordering — it is a measurement of what the ordering costs before
it pays.

**2. `DECL_RE` models type declarations and not method declarations, so every helper a page defines
for itself is counted as an unresolved API.** `ConfigureBrighter` — **14 pages, 36 sites**, the
highest-spread Brighter-shaped name in the whole census — is the reader's *own* private method,
mirroring `samples/WebAPI/*/Startup.cs`. **Every page that uses it also declares it**, which is what
the probe's *"declared on EVERY page using it"* classification means, and 21 of its 36 sites are the
declaration rather than a call:

```bash
grep -rl 'ConfigureBrighter' contents/ | wc -l                                          # 14 pages
grep -ron 'ConfigureBrighter' contents/ | wc -l                                         # 36 sites
grep -rn  'ConfigureBrighter' contents/ | grep -c 'private static void'                 # 21 declarations
git -C ../Brighter grep -l -F 'ConfigureBrighter' origin/master -- 'src/*.cs' | wc -l   # 0
git -C ../Brighter grep -l -F 'AddBrighter'       origin/master -- 'src/*.cs' | wc -l   # 11, the control
```

A method-declaration filter removes **110** of the 929, two-way controls passing
(`ConfigureBrighter` caught; `CommandProcessor` never a candidate at all). Probe preserved at
`probe/methodprobe.py`.

**3. A BCL/ASP.NET resolution filter removes 111 — and removes some of them for the wrong reason.**
Resolving candidates against the .NET 8 ref-pack XML documentation takes 929 → 818, but the names it
strikes include `Date`, `Email`, `Total`, `Product`, `Cancelled`, `Country`. Those are the
documentation's invented domain, removed because `System.DateTime.Date` contributes the token
`Date`. Probe preserved at `probe/bclprobe.py`.

**The generalisation, and it is D12's finding in a new suit:** every cheap filter available here is
a **name** filter, and names collide. The census resolves *tokens*, not *references*. Only a
compiler resolves references — which is why the 757 using-directive blocks are the other half of
this problem and why triage has a human at its boundary by construction, not by under-investment.

## Acceptance criteria

Every criterion names the command that decides it, or says it has none and who reads it. **Both
criteria ever found unmet at a close were unmarked ones** (014, AC2 and AC6), so an unmarked
criterion below is a declared risk, not an oversight.

1. **The census's precision defects are repaired or recorded as irreducible.** The two measured
   above get a ruling each: repair, or a written reason not to.
   `python3 tools/symbolcheck.py --census` — the candidate count moves, and the spec predicts the
   movement before the change, **including "none"**.
2. **A triage method exists, is written down, and names its stopping condition.** *No instrument —
   checked by reading*, by the maintainer at the requirements review. A method with no stopping
   condition is a backlog, not a method.
3. **The method has been executed over a bounded slice, and the slice is stated with its boundary.**
   `wc -l` on the triage record against the slice definition. A slice nobody states is a slice
   nobody can audit.
4. **Every name the triage confirms dead is either a `symbolwatch.tsv` row or has a written reason
   it is not.** `python3 tools/symbolcheck.py --verify-list` stays at 0 findings and the row count
   moves by the number added.
5. **Every name the triage clears is cleared against evidence, with a control.** *No instrument —
   checked by reading.* This is the criterion that fails quietly: clearing a name because it "looks
   like an example" is the same reasoning that left `ConfigureBrighter` unexamined for a year.
6. **The eight gates are unmoved, or moved on purpose with the figure changed in `tools/README.md`
   and nowhere else.** The gate commands in `tools/README.md`; D10 is the authority on the numbers.
7. **No count in any document this spec ships is unanchored.** `grep` for bare figures in the
   spec's own documents — 014 found three unanchored counts in `CLAUDE.md` and this spec quotes
   more numbers than 014 did.

## Open questions

Each has a recommendation, because a question that arrives at review unprepared gets decided by
whoever is typing.

**1. Does the census get the two precision filters, or only the method-declaration one?**
*Recommendation:* **method declarations yes, BCL resolution no.** The first is an extension of a
filter the tool already has, for the same reason — a page defining its own thing is not using an
API — and its controls pass cleanly. The second removes real domain names for the wrong reason, and
a filter that is right about 105 and wrong about 6 in a *report a person reads* buys precision by
hiding evidence. *Depends on:* whether anyone wants the head shortened more than they want it
honest.

**2. Is the 696-name single-page tail in scope at all?**
*Recommendation:* **out of scope for 015, stated rather than skipped.** A single-page, single-site
name is the least likely to be a real API and the most likely to be an example type, and 431 of them
would dominate the budget. But 014's `IMessageScheduler` sat at 7 pages, not 1, so this is a
statement about *expected yield*, not about impossibility. *Depends on:* what AC3's slice turns out
to cost per name — measure on the head first, then decide.

**3. What is the triage unit — the name, or the page?**
*Recommendation:* **the name, ordered by page-spread.** It is the ordering that has found something
before, and a page-ordered walk re-reads the same invented domain on all 31 pages that use
`OrderId`. *Depends on:* nothing; this one is cheap to reverse.

**4. Does a confirmed-dead name get repaired on the page, or only listed?**
*Recommendation:* **listed in 015, repaired in a later phase or spec.** 014's ruling 7 — *a defect
found beside a repair gets repaired* — pulls the other way, and it should be honoured for anything
small. But a repair edits a code block, which pulls the block into rule 6 strict scope and turns a
triage pass into a compile job. *Depends on:* how many names actually come back dead, which is
unknown until AC3 runs.

**5. Is `--census` reproducible enough to cite, given it reads two moving `origin/master` refs?**
*Recommendation:* **print the resolved refs in the report's header.** The report already prints
token-set sizes per ref but not the refs' SHAs, so no figure it produces can be reproduced later.
This is small and it is the reason 929 has been four numbers. *Depends on:* nothing.

## Superseded by `requirements.md`

**The open questions above are this README's first draft and `requirements.md` carries the live
set** — six rather than five, and **question 1's recommendation rests on different evidence now**.
This README argued the method-declaration filter was *an extension of a filter the tool already
has*. It is more than that: 014's `design.md` §3.2 records the probe as scoping out page-declared
*"types **and members**"*, so the filter is one the **approved design recorded and the shipped tool
lost** — finding E's ~6% discrepancy, 270 of its 808 names now accounted for. Restoring a
specification beats extending a heuristic, and that is friction 42.

Read `requirements.md` for anything this section touches.

## Status Checklist

- [x] Requirements gathered
- [x] Requirements reviewed and approved — 2026-09-18, six questions ruled, Q4 and Q6 overturning
      the recommendation; `.requirements-approved` exists
- [x] Documentation outline created — `design.md`, 2026-09-18. **It is not a page outline**: 015
      creates no page, so the outline sections are marked N/A with reasons and the design is the
      instrument change, the triage method and its pilot
- [x] Outline reviewed and approved — 2026-09-18. **Q2 was reversed at this review**, taking P0-4
      from a 103-name slice to all 819 candidates; the screen that followed found **twelve dead
      Brighter APIs in the tail and none in the head**. `.design-approved` exists
- [x] Writing tasks identified — `tasks.md`, 2026-09-18, **41 tasks / 5 phases / 5 PRs**, approved;
      three findings applied at the review and the total unmoved at 41, because all three were
      repairs to existing tasks
- [x] Writing complete — **41 of 41**, 2026-09-19. Re-derived, not incremented:
      `grep -c '^- \[x\] \*\*Task' tasks.md` against `grep -c '^- \[.\] \*\*Task' tasks.md`
- [x] Documentation reviewed — the acceptance walk, `tasks.md` § *Acceptance walk*. **Twelve of
      twelve criteria met, three repairs taken during the walk**, all eight gates at
      `tools/README.md`'s figures. The three uninstrumented criteria were walked **first** and
      produced two of the three repairs; the third came out of reconciling AC9 against the file
      that owns the gate figures
- [x] Spec closed — 2026-09-19, at 41 of 41. **`.accepted` is the maintainer's to create**, after
      the walk is read

## What 015 shipped

| | |
|---|---|
| **The instrument** | `--census` no longer counts a page's own methods (**110** names) and prints the **four SHAs** it resolved, so every figure it produces is reproducible. Census-scoped: the gate still follows `origin/master` |
| **The method** | `triage.md` §§1–4 — three verdicts, four stages, both broken query forms recorded, and an **exhaustive** stopping condition |
| **The record** | 819 verdicts, one per candidate, generated from a committed checkpoint; 44 rulings by a person, each with a quoted line and a control; `stage3.tsv` is the person's output |
| **The rows** | **17** new `symbolwatch.tsv` entries, all `DEAD` at both refs of their product |
| **The repairs** | **ten pages**, 26 of 28 sites removed and 2 kept behind visible opt-outs; **fifteen C# blocks rebuilt against the released packages** |
| **The ledgers** | thirty defects with a *found by* column, and friction **46–51** plus one recurrence |

## The residual gap — the line the next spec starts from

> **A name that still resolves — to a different signature, to a dependency's removed member, or to
> a package with no release — is caught by nothing here, because the only instrument that sees it
> was built in `/tmp` and was never committed.**

015 answered 014's sentence for the surface a census can reach: every dead Brighter name in a C#
fence now has a verdict, and the ten pages carrying one are repaired. **What it found on the way is
that the census was never the binding constraint.** Six of its thirty defects came from a compiler,
and four of those six are invisible to every census, watchlist and linter in this repository —
`S3Region.EUW1` belongs to the AWS SDK, a mapper that did not implement `IAmAMessageMapper<T>`
resolves perfectly, and `Paramore.Brighter.MySql.Dapper` is a package name that is **spelled
correctly and cannot be installed**.

Three smaller things are left on purpose, each measured rather than estimated:

- **`Use{DB}Outbox` in prose on `EFCoreOutbox.md`** — `grep -rn 'Use{DB}' contents/` returns **2**,
  both on that page. It is not an identifier, tokenises as nothing, and no census will ever
  nominate it. **Its code block was already repaired to V10 by somebody and the prose above it was
  not**, which is the whole gap in one page.
- **The using-directive debt** — the other half of the same problem, and the figure lives in
  `tools/README.md` with its ref.
- **The prose surface** — D12's **160 of 161** pages, ruled unusable for a census and fine only for
  a curated watchlist.

## Next Steps — as executed

1. ~~Re-derive the counts above~~ — done, and **four moved**: `origin/master` had left `09f5d988f`,
   and the twelve names touched nine pages rather than "at least six". `tasks.md` §2
2. ~~Read `SUMMARY.md` and `contents/`~~ — done; **015 creates no page**, and `design.md` §12
   records why the outline, nesting, redirect and glossary sections are N/A
3. ~~Identify source material~~ — done, `requirements.md` § *Source material*
4. ~~Run `/spec:requirements`~~ — done, approved 2026-09-18, six questions ruled, **two overturning
   the recommendation**; then `/spec:design` (Q2 reversed at the review), `/spec:tasks`, and five
   phases merged as #171, #172, #173, #174 and this one

## Notes

- `CLAUDE.md` is the authority on documentation standards; cite it rather than restating it
- `tools/README.md` is the authority on the gates and their expected numbers
- Source code lives in `../Brighter` and `../Darker`, and is **read-only**
- `SUMMARY.md` gets updated whenever a page is added, or the page is an orphan
- **The friction ledger stands at 40** (18–40, 23 entries, no gaps — re-derived 2026-09-17 with
  task 6.5's own command). 015's entries start at **41**, and §1 below already owes one.

## Workflow friction

41. **A `!` block in a slash command inherits the session's shell cwd, and under `bash` a
    cwd-relative glob that matches nothing returns empty output with no error.** `/spec:new`'s
    block is `ls -d spec/*/ 2>/dev/null`; run from a directory with no `spec/`, `zsh` fails loudly
    with `no matches found` — which is what happened on 2026-09-17 and is what prevented the
    failure — while `bash` returns `rc=1` and **nothing on stdout**, and `2>/dev/null` cannot
    suppress a shell-level glob error either way. Step 1 of the command then says *"take the next ID
    from the listing above"*, and an empty listing reads as **"no specs exist"**, so the next ID is
    `001` and `mkdir -p` creates a second `spec/001-*` without complaint. **This is plausible-zero
    number seven, and the shell dialect is the only thing deciding whether it is silent.** The
    programme's other six were all instruments returning a believable zero; this one is a *command*
    doing it, which is a place nobody has looked.
