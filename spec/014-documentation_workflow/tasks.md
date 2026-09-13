# Spec 014: The Documentation Workflow Itself — Tasks

**Created:** 2026-09-12 · **Status:** **APPROVED 2026-09-12** — `.tasks-approved`.
**Tasks review, 2026-09-12: three findings, all applied.** Task 2.10 was added — the `product`
column and the opt-out each had no control, which is standing obligation 3 broken inside the list
that states it, so the red-proofs went **five → seven** and the total **38 → 39**. Every task
gained an `Input` and an `Output`: twenty and six were missing, and **partial marking is worse
than none** (012's review finding 1). Task 3.8 now reads its ledger evidence out of task 2.11's
pasted red output, one PR back, because a ledger written from the repaired corpus proves nothing.
**Works from:** `design.md` (approved 2026-09-12, `.design-approved`; three questions answered,
five review findings applied) and `requirements.md` (approved 2026-09-12,
`.requirements-approved`; **amended twice by the design review** — AC7 to eleven pages, AC9 to
the watchlist form, and P0-5's model).

**Total tasks: 39, across 6 phases and 5 pull requests** — 38 as drafted, plus task 2.10, which
the tasks review added. Re-derived, not counted by hand:

```bash
grep -c '^- \[.\] \*\*Task' tasks.md     # 39
```

and the phase table below sums to 39 independently. **Keep both and re-derive both after any
edit** — 009's D-table spent three sessions wrong because a count was edited beside the row it
counted, and **012's two counts disagreed by one for exactly as long as it took to notice that
the review had added a task and not the row above it.** That is why this paragraph names the
task that moved the number.

| Phase / PR | Goal | Tasks | Deliverables |
|---:|---|---:|---|
| **1** | **The census probe** — executed 2026-09-12; its write-up ships with phase 2 | 1 | D12 |
| **2** | **`symbolcheck.py`, its watchlist, and seven red-proofs. No CI job** | 12 | D8 |
| **3** | **The four repairs AND the CI job, one PR** | 8 | D9 |
| **4** | **The three P0 commands** | 5 | D5, D2, D6 |
| **5** | **The three P1 commands, the committed home, and P2-1** | 7 | D3, D4, D1, D10, D7 |
| **6** | **Acceptance** — the walk, the ledgers, the close | 6 | D11 |

---

## 1. How this list is organised

**One phase is one pull request**, merged before the next branch starts — the contract 009, 010,
012 and 013 all ran under. **Phase 1 has no PR of its own**: it is already executed and its
write-up lands with phase 2.

**This spec writes no documentation page.** Its deliverables are command files, two tools and a
README, plus eleven `contents/` pages that exist only to be repaired. That is a deliberate
departure from the `/spec:tasks` template and §4 records it as friction rather than hiding it.

### The standing obligations — every task owes all seven

Do not restate these per task; they are assumed by all of them.

1. **Re-derive any count before quoting it**, with the command beside the figure. Two independent
   methods that must agree, per the total above.
2. **Record the mismatch before fixing it.** A repair that leaves no record of what was wrong is
   a spec that can prove the corpus is right *now* and cannot say it was ever wrong. §3's ledger
   is the only evidence 014 produces about the corpus.
3. **A check that has never failed has not been shown to work.** Every new check gets a
   red-proof, and every control is **two-way** — a known-present case and a known-absent one.
4. **Prose and permission ship together.** A command instructing a tool run gets the matching
   `allowed-tools` entry in the same commit (requirements constraint 3).
5. **Cite `CLAUDE.md`, never restate it.** Restating is how the two drifted apart.
6. **Predict gate movement before the work, including "none"** — a vacuous pass is invisible
   exactly when no movement is expected. `git add` before any `--changed` run, or the strict pass
   sees nothing.
7. **Ask before merging anything that changes the published site**, and **ask for the head-ref
   deletion by name in the same breath as the merge** — this repository does not auto-delete.

### Dependencies

- **1 gates 2.** The probe's numbers decide the checker's shape, and they did — D8 was a census
  in the requirements and is a watchlist in the design.
- **2 gates 3.** A repair with no instrument behind it is how these four survived. The gate must
  be red *before* the fix; that is the only proof the fix did anything.
- **2 gates 4.** D5 and D6 name `symbolcheck.py`, and `allowed-tools` must permit a tool that
  exists.
- **4 gates 5.** D3 and D4 mirror decisions made in D2 and D6; writing them second keeps one
  wording rather than two.
- **3, 4 and 5 are independent of each other after 2.** If one stalls, take the next.
- **Phases 3–6 are individually abandonable. 1 and 2 are not** — they are the instrument.

---

## 2. Counts re-derived at the top of this phase

Per standing obligation 1 and defect 15. **All four held; none was stale.**

| Inherited from design | Command | Result |
|---|---|---|
| D9 is 11 distinct pages | `grep -rl` over the four symbols, `sort -u`, `wc -l` | **11** |
| …with no page hit by two symbols | per-file count over the four | **no overlap** |
| The commands are 451 lines | `cat .claude/commands/spec/*.md \| wc -l` | **451** |
| `pagetypes.tsv` balances | `wc -l` = 162, one header | **161 rows = 161 pages** |

**And one figure the design did not carry, derived here because the repair tasks need it:
17 sites, not 11.** A page count sizes a red-proof; a site count sizes the work.

> **AMENDED 2026-09-13 by phase 1's write-up, finding D.** The `IMessageScheduler` row was a
> **substring** count and is two symbols: `IMessageScheduler` (7 pages, 11 sites) and
> **`IMessageSchedulerFactory`** (2 sites, one of them the whole of `InMemoryOptions.md`), dead
> at both refs with its own replacement `IAmAMessageSchedulerFactory`. **The totals do not move
> — 11 pages, 17 sites — and AC7's `= 11` was re-checked, not assumed.** The watchlist gains a
> fifth row; see task 2.1.

| Symbol | Pages | Sites |
|---|---:|---:|
| `IMessageScheduler` (whole word, `grep -rnw`) | 7 | **11** — `TickerQScheduler.md` ×3, `AzureScheduler.md` ×2, `InMemoryScheduler.md` ×2, four others ×1 |
| `IMessageSchedulerFactory` | 2 | 2 — `InMemoryOptions.md:241`, `InMemoryScheduler.md:180` |
| `IAmACommandStoreAsync` | 1 | 2 |
| `UseExternalInbox` | 1 | 1 |
| `IAmAnIbox` | 1 | 1 |
| **Total** | **11** | **17** |

**One page is hit by two symbols after all** — `InMemoryScheduler.md` — and the inherited
*"no page is hit by two"* held only while the two were miscounted as one. It changes nothing for
AC7, which counts pages.

---

## Phase 1 — the probe *(executed 2026-09-12; ships with phase 2)*

**Goal:** two numbers, before D8 is designed. **Done** — `design.md` §3 is the result and it
changed D8 from a census to a watchlist.

- [x] **Task 1.1:** Write § *Phase 1 as executed* — the probe's method, controls and findings
  - Input: `design.md` §3, and the probe script in the session scratchpad (`probe/census.py`)
  - Output: a section in this file, added on phase 2's branch
  - Notes: it must carry **the two numbers** (831 unresolved / 127 of 144 fenced pages), **the
    three instrument findings** (the empty-token-set near miss, the 146 spaced fences, the
    FluentUI.cs glyph file), and **the `IMessageScheduler` discovery with its two-way control**.
    The script is **not** added to the repository — it is throwaway by design, and saying so is
    part of the write-up

---

## Phase 1 as executed — probed 2026-09-12, written up 2026-09-13

**The probe did its job and then some.** It killed the census gate D8 was going to be (§4 of
`design.md`), it found `IMessageScheduler` on eight pages, and writing it up found a **fifth**
dead symbol that the design, the requirements and §2 of this file all carry as part of a fourth.
That last one is finding D and it amends task 2.1.

**Every number below was re-derived on 2026-09-13 at `c0d410a`, with the command beside it**
(standing obligation 1). Three of the design's figures did not reproduce exactly, and finding E
says why. **No conclusion moved.**

### The method, and the controls that came first

A token set is built per `(repo, ref)` by extracting every `[A-Z][A-Za-z0-9_]{3,}` word from
`.cs` files at that ref; a candidate is unresolved if it appears in **no** set. The presence test
is deliberately permissive — a name in a comment or an XML doc counts as present — so **every
count here is a lower bound on real reds**, which is the right direction for a probe that is
about to argue a gate cannot exist.

**Six sets**, and only four of them are the universe any conclusion rests on:

```bash
git -C ../Brighter grep -h -I -o -E '[A-Z][A-Za-z0-9_]{3,}' 10.7.0 -- 'src/*.cs' | sort -u | wc -l
```

| Set | Tokens |
|---|---:|
| `brighter@10.7.0/src` | 7,593 |
| `brighter@origin/master/src` | 7,713 |
| `brighter@10.7.0/` **all** `.cs` | 14,414 |
| `brighter@origin/master/` **all** `.cs` | 15,038 |
| `darker@4.1.1/src` | 313 |
| `darker@origin/master/src` | 313 |

**Two-way controls, run before any number was read**, per 013's rule that a control proving only
absence proves nothing: `CommandProcessor` must be in every Brighter set and `IAmAnIbox` in none.
The script exits on a control failure and on any empty token set.

**The controls are only reproducible with `git grep -w`, and that is worth knowing before task
2.6 is written.** `git grep` does not honour `\b` in its regex, so the flag is the whole of the
word-boundary behaviour:

```bash
git -C ../Brighter grep -lw 'CommandProcessor' 10.7.0 -- 'src/*.cs' | wc -l      # 43
```

| Symbol | `-lw`, 10.7.0 → master | without `-w` | What it is |
|---|---|---|---|
| `CommandProcessor` | **43 → 45** | 74 → 76 | control, present |
| `IAmAnIbox` | **0 → 0** | 0 → 0 | control, absent |
| `IAmAMessageScheduler` | **51 → 51** | 60 → 60 | the real name |
| `IMessageScheduler` | **0 → 0** | 0 → 0 | what eight pages say |

The unflagged column is not noise, it is the failure mode: `CommandProcessorBuilder` vouching for
`CommandProcessor` is harmless, but `IAmAMessageSchedulerFactory` vouching for
`IAmAMessageScheduler` is a live name lending its existence to a name that may be dead. **A
watchlist entry resolved without `-w` can be kept alive by a longer name that merely contains
it** — task 2.6 needs the flag, and task 2.9's red-proof is what will show it.

### The two numbers, and what reproduces

**831 unresolved symbols across 127 of 144 fenced pages** is what `design.md` §3.2 records and
what §4 was decided on. Re-derived today it is **881 across 128 of 145**:

| Stage | design §3.2 | re-derived 2026-09-13 |
|---|---:|---:|
| Pages with at least one C# fence | 144 of 161 | **144** loose-regex · **145** by `pagelint`'s `FENCE_RE` |
| Distinct PascalCase tokens in those fences | 2,762 | **2,763** |
| …after stripping comments and string literals | 2,283 | **2,279** |
| …after scoping out page-declared names and a third-party prefix filter | 1,211 | **1,940** |
| **Unresolved at `src/` of both products, both refs** | **831** | **881** |
| **Pages carrying at least one** | **127 of 144** | **128 of 145** |
| Prose-only surface, for comparison | 1,217 across 160 of 161 | **1,346 across 160 of 161** |

**Both page figures reproduce to within one, and both symbol figures are ~6% high**, which is the
signature of a thinner scoping filter rather than a different measurement — see finding E. **All
three of §3.2's conclusions survive unchanged**, because every one of them turns on an order of
magnitude and a ratio:

1. **An open-world census cannot be a gate.** 881 across 128 pages is no more gateable than 831
   across 127. Today's top of the list by page spread is `OrderId` (31 pages), `GreetingEvent`
   (18), `CustomerId` (17), `MyCommand` (15), `GreetingMade` (15) — **the docs' own invented
   domain**, which no cheap filter separates from a real API name.
2. **The prose surface is unusable for a census and fine for a watchlist.** 160 of 161 pages
   carry an unresolved prose token — reproduced exactly — because every capitalised English word
   qualifies. A *curated* name searched in prose has no false-positive problem at all.
3. **A code-context census structurally misses two of the three held symbols.** Re-run today,
   the first-generation script still surfaces `IAmACommandStoreAsync` and still fails to surface
   `UseExternalInbox` and `IAmAnIbox`, which are prose. **1 of 3.**

### Finding A — the empty-token-set guard, which earned itself before it was needed

The first attempt built the sets through a shell function whose quoting broke the pathspec.
**All six sets came back empty**, and without the guard that reads as *"nothing is unresolved —
the corpus is clean."* The evidence is still on disk: six zero-byte files, `b_1070_all.txt`,
`b_1070_src.txt`, `b_master_all.txt`, `b_master_src.txt`, `d_411_src.txt`, `d_master_src.txt`,
each the complete output of a set that was supposed to hold thousands of tokens.

This is the plausible-zero failure 013 recorded about `git grep` and `\b`, met again in a
different disguise in the same session it was written down. **`tools/symbolcheck.py` inherits the
guard** — task 2.5's `--census` refuses to report on an empty token set, and task 2.9's red-proof
is a corrupted ref name, which is this failure deliberately induced.

### Finding B — the spaced fences, and the regex that must not be written twice

**141 fence lines across 39 pages** are written ```` ``` csharp ```` with a space, not
```` ```csharp ````:

```bash
grep -rEic '`{3,}[ \t]+(csharp|cs|c#)\b' contents/*.md | grep -v ':0$' \
  | awk -F: '{n+=$2; p++} END {print n" fences across "p" pages"}'      # 141 across 39
```

The design says 146 across 39; the page count reproduces and the fence count is five high, with
no state having changed — 014 has edited no page. **Take 141.**

The sharper measurement is what the two regexes see, and it is a two-way control on the
instrument itself:

| Regex | Blocks | Pages | Does it see `IAmACommandStoreAsync`? |
|---|---:|---:|---|
| ```` ```(?:csharp\|cs) ```` | 843 | 118 | **no — comes back clean** |
| ```` ``` *(?:csharp\|cs) ```` | 983 | **144** | **yes — `BuildingAnAsyncPipeline.md`** |

**140 blocks across 26 pages are invisible to a regex that does not tolerate the space**, and one
of them holds a *known* dead symbol, which is how the probe's first run reported a green corpus.
`pagelint.py`'s `FENCE_RE` has `[ \t]*` and has always been right; **task 2.3 imports it rather
than writing a third one.** The 145-versus-144 discrepancy in the table above is also its doing —
`FENCE_RE` recognises the single ```` ``` c# ````-tagged fence that both ad-hoc regexes drop.

### Finding C — resolve against `src/` only, or a font vouches for an API

`SampleMauiTestApp/Resources/Fonts/FluentUI.cs` in Darker is a generated glyph table
contributing **1,579** distinct PascalCase names by itself:

```bash
git -C ../Darker grep -h -I -o -E '[A-Z][A-Za-z0-9_]{3,}' origin/master \
  -- '*FluentUI.cs' | sort -u | wc -l          # 1579
```

That is five times the whole of Darker's `src/` surface (313 tokens). Including tests and samples
in the universe lets a font glyph declare a dead API live, which is not a hypothetical: the
unresolved count against *all* `.cs` is 904 where against `src/` it is 1,211 — **the 307 symbols
in between are ones that only non-`src` code has ever heard of.**

### Finding D — `IMessageScheduler` is **two** dead symbols, and this file said one

**The discovery, unchanged:** `IMessageScheduler` does not exist at either ref, the real name is
`IAmAMessageScheduler`, and the docs have used the wrong one across the entire scheduler family —
green under all seven gates for as long as they have existed, and a paste gets `CS0246`.

**What re-deriving the site count found:** the inherited figure of *8 pages, 13 sites* is a
**substring** count, and it decomposes into two different dead names with two different
replacements.

```bash
grep -rlw 'IMessageScheduler' contents/ | wc -l          # 7   pages
grep -rnw 'IMessageScheduler' contents/ | wc -l          # 11  sites
grep -rnw 'IMessageSchedulerFactory' contents/           # 2   sites
```

| Symbol | At 10.7.0 → master (`-lw`) | Replacement | Pages | Sites |
|---|---|---|---:|---:|
| `IMessageScheduler` | 0 → 0 | `IAmAMessageScheduler` (51 → 51) | 7 | 11 |
| **`IMessageSchedulerFactory`** | **0 → 0** | **`IAmAMessageSchedulerFactory`** (11 → 11) | **2** | **2** |
| substring total, as inherited | | | 8 | 13 |

`InMemoryOptions.md:241` and `InMemoryScheduler.md:180` carry the factory —
`private static IMessageSchedulerFactory GetSchedulerFactory(` — and **`InMemoryOptions.md`
carries nothing else**, so under a word-boundary search for `IMessageScheduler` alone that page
would have gone green and stayed wrong. The live interface is
`src/Paramore.Brighter/IAmAMessageSchedulerFactory.cs`, whose `Create` returns an
`IAmAMessageScheduler`.

**This is a substring match that happened to land on a second real defect**, and the luck is the
lesson. A watchlist matching substrings would have reported `IMessageSchedulerFactory` under the
wrong entry and offered the wrong replacement; a watchlist matching whole words would have missed
it entirely. **Neither matching rule saves a list that is missing a row** — only the row does.

**Three amendments, applied below:**

- **Task 2.1's watchlist is FIVE rows, not four.** `IMessageSchedulerFactory` ·  `brighter` ·
  `IAmAMessageSchedulerFactory` · *014 probe, finding D* · `2026-09-13`.
- **Task 3.1 is two repairs, not one**: 11 sites across 7 pages for `IMessageScheduler`, and
  2 sites across 2 pages for `IMessageSchedulerFactory`.
- **§2's site table is corrected**, with the substring figure kept beside the word figure so the
  inherited number is still recognisable.

**AC7 is unaffected and was checked rather than assumed.** The union of D9's pages is still
**11** — `InMemoryOptions.md` was already inside the eight — so the red-proof
`git diff --name-only origin/master -- contents/ | wc -l` = 11 stands:

```bash
for s in IMessageScheduler IAmACommandStoreAsync UseExternalInbox IAmAnIbox; do
  grep -rl "$s" contents/; done | sort -u | wc -l      # 11
```

### Finding E — the throwaway script was thrown away, and that cost the exact numbers

**The script that produced §3.2's table does not exist.** What survives in the scratchpad is
generation one, and it reproduces *its own* numbers exactly on a re-run today — raw 2,979,
after-noise 2,884, unresolved 904 against all `.cs` and 1,211 against `src/`, 112 pages, 1 of 3
held symbols surfaced. Those are not §3.2's numbers. §3.2 came from a refined run, made minutes
later, whose filter set was never written to disk.

So the table above is a **reconstruction** — the design's own row labels rebuilt as a pipeline —
and it lands within one page of every page figure and about 6% high on every symbol figure,
because the scoping stage that is easiest to under-specify is the one that moves most (1,940
against a recorded 1,211).

**One arithmetic caveat, recorded rather than repaired.** `design.md` §4.1 and this file's START
HERE both say *"cheap filters moved 904 → 831"*. Those two numbers come from different universes:
904 is unresolved against `src` **plus tests plus samples**, 831 against `src/` **only**. The
filters did move the count a long way — 1,211 → 831 on the same universe — but the quoted pair is
not a clean before-and-after of the filters alone. **The conclusion it supports, that no cheap
filter reaches zero, is untouched.**

**The script is deliberately not added to the repository.** It is crude, it is single-use, and
the thing worth keeping from it is `--census` (task 2.5), which is the same idea written to the
house contract with the guard, the controls and `FENCE_RE` in it. **Friction 28 below is the
other half of that judgement**: throwaway is right, and *unrecorded* is what cost the exact
re-derivation.

### Gate movement — none, as predicted

Phase 1 wrote one section of one file under `spec/`. **No page changed, no tool ran in anger, and
all seven gates are where they were at `c0d410a`.** Task 2.12 is where that is measured; it is
named here only so the phase does not look like it declined to predict.

---

## Phase 2 — `symbolcheck.py`, the watchlist, and seven red-proofs

**Goal:** the instrument, proven to fire, **with no CI job**. Twelve tasks, one PR.

> **Corrected 2026-09-13.** This heading said *five red-proofs* and this line said *eleven
> tasks* — both the pre-2.10 figures, left behind when the tasks review added that task and
> updated the phase table, AC9 and the total but not the heading above them. **It is §1's own
> warning happening inside §1's own file**, and it is why every count here carries its command.

- [ ] **Task 2.1:** Write the watchlist `tools/symbolwatch.tsv`
  - Input: `design.md` §4.2; 013 ledger entry 15; the probe's `IMessageScheduler` finding
  - Output: five columns — `symbol · product · replacement · evidence · first_seen` — and **five
    rows**, amended from four by phase 1's finding D: `IMessageSchedulerFactory` ·  `brighter` ·
    `IAmAMessageSchedulerFactory` · *014 probe, finding D* · `2026-09-13`
  - Notes: `product` is `brighter`, `darker` or `both`, and it is the column §4.3 rests on —
    `.AddPolicies(` is dead in Brighter and alive in Darker 4.1.1. Verify each replacement
    against the source before writing it: **the design's own first draft got
    `IAmACommandStoreAsync` wrong** and said `IAmAnOutbox` where the answer is `IAmAnInboxAsync`

- [ ] **Task 2.2:** Write `tools/symbolcheck.py` — gate mode
  - Input: `linkcheck.py` and `pagelint.py` for the house exit-code contract and output shape
  - Output: default invocation walks `contents/`, reports `symbol · page · line · replacement`
  - Notes: **exit 1 on any hit, 0 when clean, 2 on bad arguments** — the contract the other three
    share. Accept file paths to check specific pages. It searches **code contexts and prose
    alike**, which is safe precisely because the list is curated

- [ ] **Task 2.3:** Import `FENCE_RE` from `pagelint.py` rather than writing a fence regex
  - Input: `tools/pagelint.py:192`
  - Output: the import, plus a comment saying why
  - Notes: **146 fences across 39 pages** are written ```` ``` csharp ```` with a space. The
    probe's first regex missed all 146, which is how a known-dead symbol came back clean on run
    one. This task exists so that mistake cannot be made a third time

- [ ] **Task 2.4:** Add the per-symbol opt-out
  - Input: rule 5's `<!-- pagelint: allow-serviceactivator -->` precedent
  - Output: `<!-- symbolcheck: allow <Symbol> -->`, scoped to one symbol, not a page
  - Notes: a page that says *"there is no `MsSqlOutboxBuilder`"* on purpose needs this. **Per
    symbol** so an opt-out cannot silently cover a second, unrelated dead name

- [ ] **Task 2.5:** Add `--census`, the open-world report
  - Input: `design.md` §4.4; the probe's method
  - Output: candidates sorted **by page-spread**, with counts; **never a gate**
  - Notes: it prints its own controls and **refuses to report on an empty token set**. Page-spread
    ordering is what surfaced `IMessageScheduler` at 7 among `CustomerId` and `OrderStatus`

- [ ] **Task 2.6:** Add `--verify-list`, product-aware, two refs
  - Input: the three-state ruling of 2026-09-05; `design.md` §4.2–4.3
  - Output: each row resolved at its product's pin and `origin/master`; any entry now **live** is
    reported and the run fails
  - Notes: this is what distinguishes *forthcoming* from *dead*. **An entry that becomes live is
    removed, not repaired.** It reads `../Brighter` and `../Darker` and therefore does **not**
    run in the `check` job — see task 3.6

- [ ] **Task 2.7:** Red-proof 1 and 2 — the gate fires, and stops
  - Input: tasks 2.1 and 2.2; the 11 pages and 17 sites in §2
  - Output: run before any repair → **red on 4 symbols across 11 pages, 17 sites**; run against a
    scratch copy with the 17 sites repaired → **green**
  - Notes: paste both outputs into the write-up. The scratch copy is a copy — phase 2 changes no
    page

- [ ] **Task 2.8:** Red-proof 3 — the list is what fires, not the corpus
  - Input: `tools/symbolwatch.tsv` from task 2.1
  - Output: delete one row, re-run, that symbol alone stops being reported
  - Notes: distinguishes "the tool reads its list" from "the tool has the names baked in"

- [ ] **Task 2.9:** Red-proof 4 and 5 — `--verify-list` discriminates
  - Input: task 2.6; Brighter at `10.7.0` and `origin/master`
  - Output: a corrupted ref name **exits non-zero** rather than passing clean; `CommandProcessor`
    is reported **live** (43 files at 10.7.0, 45 at master)
  - Notes: this is the two-way control at the tool level. **A control that only proves absence
    proves nothing about the grep** — `git grep` does not honour `\b`, and the probe met the
    plausible-zero failure twice in one session

- [ ] **Task 2.10:** Red-proof 6 and 7 — **the product column, and the opt-out**
  - Input: `design.md` §4.2–4.3; `CLAUDE.md` § *Page banner*; Darker at `4.1.1`
  - Output: two proofs. **Product** — add a `darker`-product row for a symbol live in Darker and
    dead in Brighter (`.AddPolicies(` is the documented case), confirm `--verify-list` calls it
    **live** and that the gate does **not** report it on a page bannered *Darker V4*, while a
    `brighter` row for the same symbol **is** reported on a Brighter page. **Opt-out** — a page
    carrying `<!-- symbolcheck: allow <Symbol> -->` goes green for that symbol **and stays red for
    a second listed symbol on the same page**
  - Notes: **added by the tasks review.** The `product` column carries §4.3's entire argument and
    had no control; an opt-out with no control is the same defect one level down. The second half
    of each proof is what makes it two-way — an opt-out that silences everything would pass a
    one-way test

- [ ] **Task 2.11:** Write §§ *Phase 1 as executed* and *Phase 2 as executed*
  - Input: tasks 1.1 and 2.7–2.10
  - Output: the probe write-up and phase 2's, with all **seven** red-proof outputs pasted
  - Notes: **the red-proofs live here because they cannot live in CI** — the gate is not wired
    until phase 3, so this section is the only record that it was ever red. Phase 3's ledger
    (task 3.8) reads its "what the corpus said" column out of this section

- [ ] **Task 2.12:** Re-run the seven gates; predict and confirm no movement
  - Input: the numbers at `c0d410a` in `requirements.md` §11.6
  - Output: the numbers, in the write-up
  - Notes: predicted **all seven unmoved** — phase 2 adds two files under `tools/` and touches no
    page. If anything moves, something was edited that should not have been

---

## Phase 3 — the four repairs, and the CI job

**Goal:** the corpus is right, and the gate that proves it goes green on its first CI run.
Eight tasks, one PR. **Ask before merging: this changes the published site.**

- [ ] **Task 3.1:** Repair the scheduler family — **two symbols**, 13 sites across 8 pages
  - Input: `IMessageScheduler` → `IAmAMessageScheduler` at `AwsScheduler.md`,
    `AzureScheduler.md` ×2, `HangfireScheduler.md`, `InMemoryScheduler.md` ×2,
    `QuartzScheduler.md`, `SchedulingAMessage.md`, `TickerQScheduler.md` ×3;
    **`IMessageSchedulerFactory` → `IAmAMessageSchedulerFactory`** at `InMemoryOptions.md:241`
    and `InMemoryScheduler.md:180`
  - Output: 8 pages
  - Notes: **substitution with a read-through, not sed** — and finding D is why. Verified with
    `git grep -lw`: `IAmAMessageScheduler` 51 files at both refs and `IMessageScheduler` 0 at
    both; `IAmAMessageSchedulerFactory` 11 at both and `IMessageSchedulerFactory` 0 at both.
    A blind substring pass over `IMessageScheduler` would produce the right factory name **by
    luck**, which is not a method. Check each block still compiles as a claim — a name repair
    can leave a signature wrong

- [ ] **Task 3.2:** Repair `IAmACommandStoreAsync`, `BuildingAnAsyncPipeline.md:36,38`
  - Input: `src/Paramore.Brighter/Inbox/Handlers/UseInboxHandlerAsync.cs:55,67` at `origin/master`
  - Output: the page's async command-sourcing example
  - Notes: **`IAmAnInboxAsync`** — command sourcing is the Inbox side, and the example is V8-era.
    This one needs reading: the surrounding `CommandSourcingHandlerAsync<T>` is the page's own
    illustrative class and is fine; the interface it depends on is not

- [ ] **Task 3.3:** Repair `UseExternalInbox`, `DispatcherConfigurationReference.md:255`
  - Input: the page; `InboxConfiguration` and the V10 inbox registration at `origin/master`
  - Output: the paragraph, rewritten
  - Notes: **not a rename — the method went.** A Reference page naming a method that does not
    exist is the worst case of the four, because Reference is what a reader consults rather than
    reads. Describe what V10 actually does

- [ ] **Task 3.4:** Repair `IAmAnIbox` → `IAmAnInbox`, `HowBrighterWorks.md:94`
  - Input: the page
  - Output: one line
  - Notes: a typo, and the cheapest entry in the ledger — which is the point of the ledger

- [ ] **Task 3.5:** Wire `symbolcheck.py` into the `check` job of `.github/workflows/docs.yml`
  - Input: the existing `linkcheck`/`pagelint` steps and their comments
  - Output: one step, with a comment saying why it ships **with** the repairs
  - Notes: **`docs.yml` triggers `on: push`.** A gate merged while the corpus is red turns
    `master` red and reddens its own PR. The rule, written into the comment: *a gate and the
    corpus that satisfies it merge together, or the gate merges second*

- [ ] **Task 3.6:** Wire `--verify-list` into the **scheduled `versions` job**, not `check`
  - Input: the `versions` job and its existing rationale comment
  - Output: `actions/checkout` for `BrighterCommand/Brighter` and `BrighterCommand/Darker`, plus
    the step
  - Notes: **nothing in the `check` job checks out either repository.** The `versions` job exists
    because *"the event that invalidates a pinned version is a release in another repository"* —
    a watchlist entry is invalidated by exactly that. Fetch depth must reach the pinned tag

- [ ] **Task 3.7:** Re-run all eight gates; confirm `symbolcheck` green and **not vacuous**
  - Input: the numbers at `c0d410a`; task 2.12's confirmation that phase 2 moved none
  - Output: the numbers
  - Notes: predicted — seven unmoved except `pagelint` warnings, which may fall if a repaired
    block earns a `using` line; `symbolcheck` **green with 4 entries loaded**. A green run over an
    empty watchlist is the vacuous case, so the run prints the entry count

- [ ] **Task 3.8:** Write § *Phase 3 as executed*, with the defect ledger
  - Input: **task 2.11's pasted red output** — that is where the "what the corpus said" column
    comes from, and it was captured in the previous PR precisely so the repair could not erase it
  - Output: what the corpus said, what the product says, and how each was found
  - Notes: standing obligation 2 — **record the mismatch before fixing it.** The evidence lives
    one PR back by design: a ledger written after the repair, from the repaired corpus, proves
    nothing. **Three of the four were found by a person and one by the probe** — record which,
    because that ratio is the argument both for and against the tool

---

## Phase 4 — the three P0 commands

**Goal:** the commands stop contradicting `CLAUDE.md` and start naming the instruments.
Five tasks, one PR.

- [ ] **Task 4.1:** Rewrite `/spec:implement` (D5), 42 → ~70 lines
  - Input: `design.md` §6.1; `CLAUDE.md` §§ *File Organization Pattern*, *Page banner*,
    *The opening sentence*, *Page descriptions*
  - Output: `.claude/commands/spec/implement.md`
  - Notes: mode-aware structure (`## Step N:` for tutorials **and how-tos**); the banner; API
    liveness with the three states and the corpus's existing `> **Not in a released package
    yet.**` form; the compile obligation; and a **Quality Check split in two** — what a tool
    decides, and what you decide

- [ ] **Task 4.2:** Rewrite `/spec:requirements` (D2), 48 → ~85 lines
  - Input: `design.md` §6.2
  - Output: `.claude/commands/spec/requirements.md`
  - Notes: the **subject declaration** — `feature` · `reader problem` · `process` — which decides
    both the research steps and which sections apply; acceptance criteria **each naming an
    instrument or marked as having none**; open questions by name; re-derive the README. Phrase
    criteria as *contains*, not *ends with* (defect 16), and say that **"a file exists" is not an
    instrument** (defect 17)

- [ ] **Task 4.3:** Rewrite `/spec:review` (D6), 87 → ~120 lines
  - Input: `design.md` §6.3; the existing `linkcheck` block, which is the model to copy
  - Output: `.claude/commands/spec/review.md`
  - Notes: **all eight gates** with the pre-existing-versus-yours discipline; **the numbers are
    cited from `tools/README.md`, never pasted**; and a fourth phase checklist, **Acceptance**,
    which walks the no-instrument criteria first

- [ ] **Task 4.4:** Update `allowed-tools` on all three, in the same commit as their prose
  - Input: the three frontmatter blocks; the eight gate commands from `tools/README.md`'s draft
  - Output: the three frontmatter blocks
  - Notes: standing obligation 4. Today **one command of nine permits any `python3`**, and only
    `linkcheck.py`. A prose-only change would read as covered and be forbidden

- [ ] **Task 4.5:** Write § *Phase 4 as executed*
  - Input: tasks 4.1–4.4
  - Output: a section in this file, with the before/after line counts per command
  - Notes: record any place where citing `CLAUDE.md` was not possible because the convention is
    not written there — those are D10's and `CLAUDE.md`'s gaps, and phase 5 owns them

---

## Phase 5 — the three P1 commands, the committed home, and P2-1

**Goal:** the rest of the workflow, and a home the commands can cite. Seven tasks, one PR.

- [ ] **Task 5.1:** Write `tools/README.md` (D10)
  - Input: `design.md` §7.4; the gate numbers at `c0d410a`; 012 §1's phase-is-a-PR contract
  - Output: a new file — the eight commands, what each checks, **the expected numbers at a named
    ref**, and the contract
  - Notes: **only what a command cites moves out of `PROMPT.md`** (Q3). The board, branch tips, CI
    census and open-question log stay there, and `PROMPT.md` stays ignored. Every number carries
    the ref it was measured at, or it is not a fact

- [ ] **Task 5.2:** Rewrite `/spec:design` (D3), 54 → ~85 lines
  - Input: `design.md` §7.1
  - Output: `.claude/commands/spec/design.md`
  - Notes: page type per file; the conventions, cited; **verify the API the design will print**;
    **re-verify the requirements rather than quote them**; predict gate movement including *none*.
    Add the step friction 23 asks for: **a design resting on an unmeasured number says which
    number and how it will be measured, before approval**

- [ ] **Task 5.3:** Rewrite `/spec:tasks` (D4), 63 → ~95 lines
  - Input: `design.md` §7.2; §1 of this file, which is the worked example
  - Output: `.claude/commands/spec/tasks.md`
  - Notes: phase-is-a-PR, citing `tools/README.md`; **the four-phase template kept but explicitly
    labelled a default**; a standing-obligations section; red-proofs; record-before-fixing;
    re-derive inherited counts; an acceptance phase last

- [ ] **Task 5.4:** Rewrite `/spec:new` (D1), 63 → ~75 lines
  - Input: `design.md` §7.3; this spec's own README, which the re-derive instruction is drawn from
  - Output: `.claude/commands/spec/new.md`
  - Notes: the README template gains **Acceptance criteria**, **Open questions**, and the
    re-derive instruction. 013's README named six gaps of which five were closed; 014's defect
    count was wrong by two before this spec started

- [ ] **Task 5.5:** Add the `CLAUDE.md` note at rule 2
  - Input: `CLAUDE.md` § *Page banner*; `design.md` §4.3
  - Output: one line recording that `symbolcheck` reads the banner for product
  - Notes: rule 2 is now a dependency of a second tool. Nobody should be able to weaken the banner
    without seeing what rests on it

- [ ] **Task 5.6:** Add the evidence requirement to `/spec:update-task` (D7, P2-1)
  - Input: `requirements.md` §3.1; 013's ledger entry 13
  - Output: `.claude/commands/spec/update-task.md`
  - Notes: the task's stated **Output** must exist before the box flips; where it cannot be
    checked, the tick says so. 013's task 4.8 was ticked with its two `pagetypes.tsv` rows
    unwritten and reached `master`

- [ ] **Task 5.7:** Write § *Phase 5 as executed*
  - Input: tasks 5.1–5.6
  - Output: a section in this file, including the P2 strikes
  - Notes: **P2-2 (`commandlint`) and P2-3 (`/spec:status` gate state) are not attempted.** P2 is
    *nice to have* and AC1 binds P0 only. Record them as struck-with-a-reason, not as owed

---

## Phase 6 — acceptance

**Goal:** walk AC1–AC12 with evidence and find what the phases did not. Six tasks, one PR.

- [ ] **Task 6.1:** Walk AC1–AC12 forwards, one paragraph each, naming the command and its output
  - Input: `requirements.md` §12, as amended by the design review
  - Output: one paragraph per criterion, each naming its instrument and that instrument's output
  - Notes: **start with the seven-and-a-half that have no tool** — AC1, AC2, AC3, AC4, AC10,
    AC12, AC6, and half each of AC8 and AC11. Both criteria ever found unmet at a close were
    unmarked ones. **Use a census, not a `tail`**: 013's only false finding came from
    `grep '^## ' | tail -4` on a seven-step page

- [ ] **Task 6.2:** Walk AC2 — no command contradicts `CLAUDE.md` — convention by convention
  - Input: `CLAUDE.md`'s seventeen-row rule ledger; the nine commands as rewritten
  - Output: a table, convention against the command that would cause a writer to break it
  - Notes: this is the criterion 014 exists to satisfy and it has **no tool**. Walking it by
    reading the commands against `CLAUDE.md`'s ledger is the whole test

- [ ] **Task 6.3:** Walk AC7 backwards — what changed that should not have
  - Input: `origin/master` at the pre-014 tip
  - Output: `git diff --name-only origin/master -- contents/ | wc -l` = **11**, and
    `git diff --stat origin/master -- SUMMARY.md` empty
  - Notes: **the criterion most likely to break quietly.** A spec about the workflow is the
    easiest place to talk yourself into a "while I'm here" edit

- [ ] **Task 6.4:** Write the defect ledger — every defect 014 found, and what the product says
  - Input: task 3.8's ledger; task 2.11's red output; the eighteen workflow defects
  - Output: the ledger table, with a *found by* column
  - Notes: it already has two entries the phases did not predict — `IMessageScheduler`, found by
    the probe on seven pages, and **`IMessageSchedulerFactory`, found by phase 1's write-up while
    re-deriving the first one's site count** — both green under all seven gates for as long as
    they have existed. The *found by* column has to distinguish them: one is the instrument's
    win, the other is the re-derivation's

- [ ] **Task 6.5:** Write the friction ledger — 014's own, continuing at 30
  - Input: `requirements.md` §14 (18–21), `design.md` §13 (22–24), §4 of this file (25–29, of
    which **28 and 29 were met in phase 1**)
  - Output: the collected ledger, plus whatever phases 2–5 met
  - Notes: **the ledger is 014's product as much as the commands are** — it is what the next
    workflow spec inherits, and the reason this one had eighteen defects to work from

- [ ] **Task 6.6:** Close — README checklist, `spec/.current-spec`, and what the next session needs
  - Input: the README's status checklist; `design.md` §4.1's coverage matrix
  - Output: the checklist ticked, and a closing section naming the residual gap
  - Notes: **do not repoint `.current-spec` until a next spec exists.** Record the residual gap —
    *a dead API written into prose on an existing page, uncompiled and unlisted, is caught by
    nothing* — because that is the line the next spec starts from, and the one to quote if anyone
    claims 014 closed defect 8

---

## 3. Acceptance criteria — where each is met

| # | Criterion | Met by |
|---|---|---|
| AC1 | Every P0 ships or is struck | tasks 6.1, and 5.7 for the P2 strikes |
| AC2 | No command contradicts `CLAUDE.md` | **task 6.2**, walked |
| AC3 | Eighteen defects repaired or struck, count re-derived | task 6.4 |
| AC4 | Every added obligation names its instrument | task 6.1 |
| AC5 | Eight gates named; `/spec:review` runs eight | tasks 4.3, 4.4 |
| AC6 | Prose and `allowed-tools` agree | task 4.4, walked at 6.1 |
| AC7 | `SUMMARY.md` unchanged; `contents/` exactly 11 pages | **task 6.3** |
| AC8 | Seven gates green at their numbers | tasks **2.12**, 3.7 |
| AC9 | The red-proofs — **seven, not five**, after the tasks review | tasks 2.7, 2.8, 2.9, **2.10** |
| AC10 | 014's friction recorded | task 6.5 |
| AC11 | Only what a command cites moved out of `PROMPT.md` | task 5.1 |
| AC12 | The probe ran and its numbers are cited | tasks 1.1, **2.11** |

---

## 4. Workflow friction — more of 014's evidence

Continuing the ledger at 25.

25. **`/spec:tasks` proposed a four-phase structure that contradicts the approved design, exactly
    as defect 14 predicted.** *Research & Preparation → Core Documentation → Supporting
    Documentation → Polish & Review* against a design whose six phases are deliverable-shaped and
    whose ordering is forced by a CI constraint. **This is the second spec in a row to record it**
    — 013's tasks §5 item 9 is the first — which is why D4 keeps the template but labels it a
    default rather than deleting it.

26. **The command's final-task rule cannot be satisfied and should not be.** *"Include a final
    task to update SUMMARY.md and verify all links"* — 014 changes no `SUMMARY.md` entry, and its
    final task is an acceptance walk. The rule is right for a page-writing spec and wrong here,
    which is friction 18's subject declaration arriving at its third consecutive phase. **Three
    phases, three category errors, one repair** (D2's subject declaration) — that is the pattern
    worth quoting when someone asks whether the repair is worth its length.

27. **Nothing asks a task list what it will cost to get wrong.** The phase ordering here is load
    bearing in a way no command asks about: wiring the CI job one phase early turns `master` red
    for everyone, and that is invisible in the design document. It was caught by reading
    `docs.yml`, not by any step the workflow prescribes. **D4 should ask each phase what happens
    if it merges alone** — the phases that answer badly are the ones that need a partner.

28. **"Throwaway" is a decision about the script and was taken as a decision about its output.**
    *Met in phase 1; see finding E.* The probe was right to be single-use and its numbers were
    right to be quoted — but the filter set that produced them lived only in the running process,
    so `design.md` §3.2's table can be *reconstructed* to within 6% and cannot be re-derived.
    **A throwaway instrument owes its committed output either the script or the exact commands**,
    and D3's *"say which number and how it will be measured"* step (task 5.2) should extend to
    *and how it will be measured again*.

29. **Nothing in the workflow re-derives an inherited count at the grain the next task will use
    it at.** *Met in phase 1; see finding D.* §2 of this file re-derived all four inherited
    counts per standing obligation 1, and all four held — because it re-derived them as **page
    counts**, which is the grain AC7 uses. The site count underneath was a `grep` substring
    total, and it was two dead symbols wearing one name. **The check that catches this is
    cheap and specific: re-derive with `-w`, and if the two disagree, the difference is a
    longer name that deserves its own row.** Four consecutive phases across two specs have now
    found a stale or mis-grained inherited count.
