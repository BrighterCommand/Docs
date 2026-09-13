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

- [x] **Task 2.1:** Write the watchlist `tools/symbolwatch.tsv`
  - Input: `design.md` §4.2; 013 ledger entry 15; the probe's `IMessageScheduler` finding
  - Output: five columns — `symbol · product · replacement · evidence · first_seen` — and **five
    rows**, amended from four by phase 1's finding D: `IMessageSchedulerFactory` ·  `brighter` ·
    `IAmAMessageSchedulerFactory` · *014 probe, finding D* · `2026-09-13`
  - Notes: `product` is `brighter`, `darker` or `both`, and it is the column §4.3 rests on —
    `.AddPolicies(` is dead in Brighter and alive in Darker 4.1.1. Verify each replacement
    against the source before writing it: **the design's own first draft got
    `IAmACommandStoreAsync` wrong** and said `IAmAnOutbox` where the answer is `IAmAnInboxAsync`
  - **Done 2026-09-13.** All ten names resolved two ways at both refs with
    `git -C ../Brighter grep -lw '<symbol>' <ref> -- 'src/*.cs' | wc -l`, dead **0 → 0** and live
    **not** zero: `IAmACommandStoreAsync` 0→0 / `IAmAnInboxAsync` **9→9**; `UseExternalInbox`
    0→0, no successor of that name — `UseExternalBus` and `UseExternalLuggageStore` are the
    surviving `UseExternal*` pair, so the cell reads *(removed at V10 — not a rename)*;
    `IAmAnIbox` 0→0 / `IAmAnInbox` **6→8**; `IMessageScheduler` 0→0 /
    `IAmAMessageScheduler` **51→51**; `IMessageSchedulerFactory` 0→0 /
    `IAmAMessageSchedulerFactory` **11→11**. `IAmAnInboxAsync` checked for sense as well as
    liveness — it is the parameter `UseInboxHandlerAsync` actually takes
    (`src/Paramore.Brighter/Inbox/Handlers/UseInboxHandlerAsync.cs:55,67` @ `origin/master`).
    **The file supports `#` comments and blank lines**, which `pagetypes.tsv` does not; task 2.2's
    parser must skip them, and task 2.7's red-proof passes over a file that has them.
    **All five shipped rows are `brighter`, so the `product` column has no discriminating row in
    the corpus** — task 2.10's proof is the only thing that exercises it, which is exactly why the
    tasks review added it

- [x] **Task 2.2:** Write `tools/symbolcheck.py` — gate mode
  - Input: `linkcheck.py` and `pagelint.py` for the house exit-code contract and output shape
  - Output: default invocation walks `contents/`, reports `symbol · page · line · replacement`
  - Notes: **exit 1 on any hit, 0 when clean, 2 on bad arguments** — the contract the other three
    share. Accept file paths to check specific pages. It searches **code contexts and prose
    alike**, which is safe precisely because the list is curated
  - **Done 2026-09-13.** Run over `contents/` it reports **17 sites across 11 pages from 5
    entries over 161 pages**, which is §2's re-derived total reached by a second, independent
    route — and the per-symbol split is finding D's: `IMessageScheduler` **11 across 7**,
    `IMessageSchedulerFactory` **2 across 2**, `InMemoryOptions.md` under the factory only.
    Contract walked: clean page **0**, red page **1**, missing path **2**, a path outside
    `contents/` **2**, unknown option **2**. **Seven watchlist faults exit 2, not 1** — no
    header, empty list, bad `product`, wrong field count, blank replacement, duplicate symbol,
    missing file — because a tool with nothing to say about the corpus must not say the corpus
    is red. Matching is whole-word with **boundaries applied only at ends that have one**, so
    `.AddPolicies(` is listable and `IMessageSchedulerFactory` does not match under
    `IMessageScheduler`; eight matcher cases checked both ways. Product filter checked across
    all four banner states — Brighter, Darker, both, and **no banner, which is checked against
    everything**: a page must not escape this gate by failing pagelint's rule 1

- [x] **Task 2.3:** Import `FENCE_RE` from `pagelint.py` rather than writing a fence regex
  - Input: `tools/pagelint.py:192`
  - Output: the import, plus a comment saying why
  - Notes: **re-scoped by task 2.2, and the reason is the inversion.** The gate searches code and
    prose alike, so **gate mode needs no fence regex at all** — the task's premise was a census's
    premise. `symbolcheck.py` already imports `APPLIES_TO`, `BANNER_RE` and `products_named` from
    `pagelint.py` on the same never-restate principle; `FENCE_RE` joins them **when `--census`
    arrives in task 2.5**, which is the only mode that has code contexts to find. Kept as its own
    task rather than folded in, because **141 fences across 39 pages** are written
    ```` ``` csharp ```` with a space (146 as inherited; re-derived in phase 1, finding B), the
    probe's first regex missed all of them, and that is how a known-dead symbol came back clean
    on run one. This task exists so that mistake cannot be made a third time
  - **Done 2026-09-13, with task 2.5**, which is the mode that has fences to find. `FENCE_RE` and
    `CSHARP_TAGS` are imported beside `APPLIES_TO`, `BANNER_RE` and `products_named`, under a
    comment carrying the 141 fences and the reason. **The import is checked by its own output**:
    `--census` reports **145 pages with a C# fence**, which is the loose count, not the 118 a
    space-blind regex sees

- [x] **Task 2.4:** Add the per-symbol opt-out
  - Input: rule 5's `<!-- pagelint: allow-serviceactivator -->` precedent
  - Output: `<!-- symbolcheck: allow <Symbol> -->`, scoped to one symbol, not a page
  - Notes: a page that says *"there is no `MsSqlOutboxBuilder`"* on purpose needs this. **Per
    symbol** so an opt-out cannot silently cover a second, unrelated dead name
  - **Done 2026-09-13.** Own line, matched whole, like the precedent. The symbol is the rest of
    the comment rather than a word, so `.AddPolicies(` is opt-out-able too. **The two-way
    demonstration wrote itself**: `InMemoryScheduler.md` is the one page carrying two listed
    symbols, so opting out `IMessageScheduler` there silences **exactly its 2 sites** and
    `IMessageSchedulerFactory` **still reports, exit 1**. That is task 2.10's second half, met
    early on the page finding D created. Eight parse cases checked: recognised on its own line,
    with extra whitespace and indented, and **rejected** with no symbol, with trailing prose,
    inline in a sentence, and under another tool's name
  - **Two things the task did not ask for, and the reason for each.** The opt-out comment names
    its own symbol and so matched its own pattern — counted, every opt-out would have reported
    one more suppressed site than it has, and on a page whose only mention was the comment an
    unused opt-out would have looked used. It is skipped. And **an opt-out is never silent**:
    suppressed sites are printed with a count, and one that names an unlisted symbol or
    suppresses nothing is a **warning that does not change the exit code** — `0 findings` and
    `0 findings, 14 silenced` are different claims about the corpus. Warning, not error, on
    pagelint's rule-6 argument: debt that fails a build gets deleted rather than understood.
    **`contents/` carries no opt-out today**, so the proof above is the only evidence it works

- [x] **Task 2.5:** Add `--census`, the open-world report
  - Input: `design.md` §4.4; the probe's method
  - Output: candidates sorted **by page-spread**, with counts; **never a gate**
  - Notes: it prints its own controls and **refuses to report on an empty token set**. Page-spread
    ordering is what surfaced `IMessageScheduler` at 7 among `CustomerId` and `OrderStatus`
  - **Done 2026-09-13.** Runs in **0.55s** over 161 pages and prints, in order: the four token
    sets with their sizes, the controls, six stage counts, then **every** candidate — no cap,
    visible or otherwise — with page-spread, site count, its first four pages and a marker when
    it is already on the watchlist. **Exit is 0 whatever it finds**, deliberately: an exit code
    that varies with findings is the first step to someone wiring a report into CI
  - **The instrument fails loudly three ways, all walked.** A corrupted ref → *empty token set …
    a plausible zero is the failure this check exists to avoid*, **exit 2**; a missing checkout →
    **exit 2** naming the directory; and — the one that matters — **pointing `brighter` at
    Darker's repository gives a perfectly non-empty token set and is caught only by the
    control**, `CommandProcessor absent from brighter@4.1.1`, **exit 2**. That is the two-way
    control earning itself: a guard against emptiness alone would have passed it
  - **What it measures, and the number is not 831.** 145 fenced pages, 2,763 raw tokens, 2,280
    after comments and strings, 2,019 candidates, **932 unresolved across 129 of 145**. Phase 1
    recorded 831/127 from a script that no longer exists and reconstructed 881/128; this is a
    third filter set and therefore a third number. **The difference is the point of friction
    28 and its repair**: 932 is the first of these figures that anyone can reproduce with one
    command, so from here the number rots visibly instead of silently. The shipped filter
    deliberately does **not** exclude the example-domain nouns the reconstruction did — a report
    read by a person should over-report rather than hide a real dead name
  - **And it re-proves why D8 was inverted, from inside the tool that replaced it.** Of the five
    watchlisted symbols the census sees **three**: `UseExternalInbox` and `IAmAnIbox` are prose
    and structurally invisible to it. It also under-counts the one it found — **6 pages / 8
    sites against the gate's 7 / 11** — because the rest are in `//` comments and a sentence.
    A census would catch **3 of 5, none of them completely**; the watchlist catches 5 of 5,
    all 17 sites

- [x] **Task 2.6:** Add `--verify-list`, product-aware, two refs
  - Input: the three-state ruling of 2026-09-05; `design.md` §4.2–4.3
  - Output: each row resolved at its product's pin and `origin/master`; any entry now **live** is
    reported and the run fails
  - Notes: this is what distinguishes *forthcoming* from *dead*. **An entry that becomes live is
    removed, not repaired.** It reads `../Brighter` and `../Darker` and therefore does **not**
    run in the `check` job — see task 3.6
  - **Done 2026-09-13.** Green on the shipped list: **all 5 entries DEAD at both refs of their
    product, and every named replacement LIVE.** Two refs give **four** states, not three, and
    each gets its own advice because the advice about the *pages* differs — `LIVE` (both),
    `FORTHCOMING` (absent at pin, present at master — *a page may name it if it says so*),
    `WITHDRAWN` (present at pin, absent at master — *the corpus is right for that pin*). All
    three say **remove the row, never repair it**
  - **Six induced cases, all four states reached with real names, not fixtures:** `LIVE`
    `CommandProcessor` **43 → 45**, exit 1; `FORTHCOMING` `IAmACausationTrackingOutbox`
    **0 → 12**, exit 1; `WITHDRAWN` `RegisterConverters` **3 → 0**, exit 1 — that is #4276's fix
    showing up as a state; a dead row whose **replacement** is dead, exit 1 with its own message;
    a corrupted ref, **exit 2** carrying git's own `unable to resolve revision`; and the product
    case below. `--census --verify-list` together and `--verify-list` with a path are both exit 2
  - **The `product` column, demonstrated on one name.** `AddPolicies` as a `darker` row resolves
    against Darker's refs and comes back **LIVE, 1 → 1**; the same name is **0 → 0** in Brighter.
    One symbol, opposite verdicts, and only the column tells them apart — §4.3's argument, met
    with a real name. **Note for task 2.10: the row must be `AddPolicies`, not the design's
    `.AddPolicies(`** — the leading dot is how a *caller* writes it, and library source contains
    the definition
  - **A trap worth the whole task: `git grep -w` silently zeroes any pattern that does not begin
    and end with a word character.** `git grep -lwF '.Handle('` returns **0** files where the
    same search without `-w` returns **23**. A row like `.AddPolicies(` verified under an
    unconditional `-w` would read `DEAD` forever, whatever the truth. `resolve()` applies the flag
    only at ends that have a word character, mirroring the gate's matcher. **That is the
    plausible-zero failure for the third time in this spec, in a third disguise** — empty token
    sets in phase 1, the space-blind fence regex before it, and now a flag that cannot match

- [x] **Task 2.7:** Red-proof 1 and 2 — the gate fires, and stops
  - Input: tasks 2.1 and 2.2; the 11 pages and 17 sites in §2
  - Output: run before any repair → **red on 4 symbols across 11 pages, 17 sites**; run against a
    scratch copy with the 17 sites repaired → **green**
  - Notes: paste both outputs into the write-up. The scratch copy is a copy — phase 2 changes no
    page
  - **Done 2026-09-13. Red: `17 site(s) across 11 page(s), from 5 watchlist entries over 161
    pages`, exit 1** — five symbols, not the four the task says, per finding D. **Green on the
    repaired copy: `No watchlisted symbols found (5 entries, 161 pages checked)`, exit 0** —
    and it says *5 entries*, so the green is not the empty-list green. Full outputs in
    § *Phase 2 as executed* (task 2.11)
  - **The copy was repaired with the gate's own `word_pattern`**, so the repair cannot be right
    in a way the gate would not have accepted. **17 lines changed across 11 pages** —
    `InMemoryScheduler.md` ×3, `TickerQScheduler.md` ×3, `AzureScheduler.md` ×2,
    `BuildingAnAsyncPipeline.md` ×2, seven more ×1 — which is §2's site count derived a **third**
    way, after the design's grep and the gate's own report
  - **A green gate is not a repaired page, and the proof shows it.** Checked that the
    replacements *arrived* rather than the lines being deleted: `IAmAnInboxAsync` 0 → 2,
    `IAmAMessageScheduler` 0 → 11, `IAmAMessageSchedulerFactory` 2 → 4, `IAmAnInbox` 3 → 4,
    `UseExternalInbox` 1 → 0. But the mechanical stand-in for `UseExternalInbox` produced
    *"use the **InboxConfiguration** method call"* — a type described as a method, **wrong prose
    that the gate calls green**. That is design §4.1's residual row demonstrated live, it is why
    task 3.3 rewrites the paragraph instead of substituting a token, and it is the sentence to
    quote if anyone reads a green `symbolcheck` as "the page is correct"
  - **The real corpus was not touched**: `git status --short contents/` empty,
    `git diff --name-only origin/master -- contents/` **0**, and the gate is **still red at
    exit 1** in this repository — which is what phase 2 is required to leave behind for phase 3

- [x] **Task 2.8:** Red-proof 3 — the list is what fires, not the corpus
  - Input: `tools/symbolwatch.tsv` from task 2.1
  - Output: delete one row, re-run, that symbol alone stops being reported
  - Notes: distinguishes "the tool reads its list" from "the tool has the names baked in"
  - **Done 2026-09-13, as a full leave-one-out rather than one deletion.** Five runs, each
    dropping one row, and in every one the dropped symbol stops being reported, **the other four
    keep their exact site counts**, and the footer says **4 watchlist entries** instead of 5:

    | Dropped | Sites | Pages |
    |---|---|---|
    | *(baseline)* | 17 | 11 |
    | `IAmACommandStoreAsync` | 15 | 10 |
    | `UseExternalInbox` | 16 | 10 |
    | `IAmAnIbox` | 16 | 10 |
    | `IMessageScheduler` | **6** | **5** |
    | `IMessageSchedulerFactory` | 15 | 10 |

    The site arithmetic is exact each time. **The page arithmetic is the interesting one:**
    dropping `IMessageScheduler` takes 7 pages off an 11-page total and leaves **5**, not 4,
    because `InMemoryScheduler.md` carries the factory as well — finding D's overlap showing up
    as a number in an unrelated proof
  - **And the other direction, which deletion alone cannot prove.** Adding a row for a name
    **absent from the corpus** (`ZzzNeverWrittenAnywhere`) leaves the findings **byte-identical**
    and moves only the entry count, 5 → 6: a list can grow without manufacturing a finding.
    Adding one for a name **present and never listed** — `CommandSourcingHandlerAsync`, the
    page's own example class, 5 sites on `BuildingAnAsyncPipeline.md` — takes the report to
    **22 sites** with the other five unchanged. **Removal proves the tool forgets; addition
    proves it looks.** The far end is the empty list, which is **exit 2**, not a green
  - **No `--watchlist <path>` flag, deliberately**, which is why all of this ran against a
    scratch copy of `contents/` and `tools/`. A gate that can be pointed at another list can be
    silenced by pointing it at an empty one, and that is a single line in a workflow file.
    `git status --short tools/` is empty: the shipped list was never edited

- [x] **Task 2.9:** Red-proof 4 and 5 — `--verify-list` discriminates
  - Input: task 2.6; Brighter at `10.7.0` and `origin/master`
  - Output: a corrupted ref name **exits non-zero** rather than passing clean; `CommandProcessor`
    is reported **live** (43 files at 10.7.0, 45 at master)
  - Notes: this is the two-way control at the tool level. **A control that only proves absence
    proves nothing about the grep** — `git grep` does not honour `\b`, and the probe met the
    plausible-zero failure twice in one session
  - **Done 2026-09-13, as subprocess runs on a scratch copy** — the shipped tool and list were
    never edited (`git status --short tools/` empty). **Red-proof 4 was run twice, because there
    are two ways to have the wrong ref and only one of them is git's problem:**

    ```text
    4a  ref '10.7.O' (letter O)   --verify-list cannot run: ../Brighter@10.7.O:
                                  fatal: unable to resolve revision: 10.7.O      exit 2
    4b  ../Darker at 4.1.1,       control failed: CommandProcessor resolves DEAD,
        pointed at as 'brighter'  expected LIVE (0 files at 4.1.1, 0 at master)   exit 2
    ```

    **4b is the one the task is really about.** That ref resolves perfectly, the repository is
    real, `git grep` is happy, and every row comes back `DEAD` — a clean sweep that means
    nothing. Only the **present** half of the control catches it. An absence-only control
    passes 4b without a murmur, which is the note's point made with a run instead of an argument
  - **Red-proof 5: `CommandProcessor` added as a row is reported `LIVE`, 43 at `10.7.0` and 45 at
    `origin/master`, exit 1**, with the advice *REMOVE THE ROW*. The same two numbers print as a
    control on **every** `--verify-list` run, green or red, so the discrimination is visible even
    on a clean day
  - **A fifth proof nobody asked for, met by accident and worth more than one of the four.** The
    first run in the scratch copy had no sibling checkouts, and the tool said **`no checkout at
    ../Brighter … cannot run without it`, exit 2** rather than resolving five rows against
    nothing and reporting them all still dead. **That environment is the CI `check` job**, and
    this is the evidence behind task 3.6: in the job where `--verify-list` does not belong, it
    cannot produce a false green even if someone wires it there
  - **One cosmetic fix in the same commit:** the summary said *"1 entry need attention"*

- [x] **Task 2.10:** Red-proof 6 and 7 — **the product column, and the opt-out**
  - Input: `design.md` §4.2–4.3; `CLAUDE.md` § *Page banner*; Darker at `4.1.1`
  - Output: two proofs. **Product** — add a `darker`-product row for a symbol live in Darker and
    dead in Brighter (**`AddPolicies`**, not the design's `.AddPolicies(`: task 2.6 measured the
    dotted form at 0 files in both products, because the dot is how a caller writes it and
    library source holds the definition), confirm `--verify-list` calls it
    **live** and that the gate does **not** report it on a page bannered *Darker V4*, while a
    `brighter` row for the same symbol **is** reported on a Brighter page. **Opt-out** — a page
    carrying `<!-- symbolcheck: allow <Symbol> -->` goes green for that symbol **and stays red for
    a second listed symbol on the same page**
  - Notes: **added by the tasks review.** The `product` column carries §4.3's entire argument and
    had no control; an opt-out with no control is the same defect one level down. The second half
    of each proof is what makes it two-way — an opt-out that silences everything would pass a
    one-way test
  - **Done 2026-09-13.** `AddPolicies` is the right fixture and the corpus supplied it: **5 real
    sites across 3 pages, every one of them bannered *Darker V4*** — `DarkerBasicConfiguration.md`
    ×3, `QueryPipeline.md`, `QueryPipelinePolicies.md`. One site was **planted** on a scratch
    copy of `PolicyRetryAndCircuitBreaker.md`, a *Brighter V10* page, so the same symbol exists
    on both sides of the product line
  - **Proof 6 — one symbol, two rows, disjoint page sets and opposite verdicts:**

    | Row | Gate reports | `--verify-list` |
    |---|---|---|
    | `AddPolicies` · **brighter** | **1 site / 1 page** — the planted Brighter page alone; all 5 Darker sites invisible | **DEAD** 0 at `10.7.0`, 0 at master — *all 6 entries still dead*, green |
    | `AddPolicies` · **darker** | **5 sites / 3 pages** — exactly the Darker V4 pages; the planted Brighter site invisible | **LIVE** 1 at `4.1.1`, 1 at master — exit 1, *REMOVE THE ROW* |

    **That is §4.3's whole argument in one table**, and neither half of it is provable without
    the other: a `brighter` row that reported Darker pages would be a false positive nobody
    could silence except by deleting the row, and a row whose liveness was resolved against the
    wrong product would call a live method dead
  - **Proof 7 — the opt-out, on `InMemoryScheduler.md`**, the one page carrying two listed
    symbols. Before: `IMessageScheduler` **2 sites** and `IMessageSchedulerFactory` **1 site**,
    exit 1. After adding `<!-- symbolcheck: allow IMessageScheduler -->` and nothing else:
    `IMessageSchedulerFactory` **still reported, still exit 1**, with
    `----- silenced by opt-out (2 site(s)) ----- contents/InMemoryScheduler.md IMessageScheduler
    ×2` underneath. **The second half is the half that matters** — an opt-out that silenced the
    page would pass a one-way test and be a hole in the gate

- [x] **Task 2.11:** Write §§ *Phase 1 as executed* and *Phase 2 as executed*
  - Input: tasks 1.1 and 2.7–2.10
  - Output: the probe write-up and phase 2's, with all **seven** red-proof outputs pasted
  - Notes: **the red-proofs live here because they cannot live in CI** — the gate is not wired
    until phase 3, so this section is the only record that it was ever red. Phase 3's ledger
    (task 3.8) reads its "what the corpus said" column out of this section

- [x] **Task 2.12:** Re-run the seven gates; predict and confirm no movement
  - Input: the numbers at `c0d410a` in `requirements.md` §11.6
  - Output: the numbers, in the write-up
  - Notes: predicted **all seven unmoved** — phase 2 adds two files under `tools/` and touches no
    page. If anything moves, something was edited that should not have been

---

## Phase 2 as executed — 2026-09-13, `docs/014-phase2-symbolcheck`

**All twelve tasks in one PR. Two files added under `tools/` — `symbolcheck.py` (818 lines) and
`symbolwatch.tsv` (5 rows) — and no page changed.** The seven gates are exactly where
`requirements.md` §11.6 recorded them; the eighth is **red on purpose** and stays red until
phase 3.

### The seven red-proofs, as run

They live here because they **cannot live in CI**. The gate is not wired until phase 3, so this
section is the only record that it was ever red, and task 3.8's ledger reads its *what the corpus
said* column out of it.

**1 — the gate fires.** `python3 tools/symbolcheck.py`, before any repair:

```text
===== IAmACommandStoreAsync — 2 site(s) across 1 page(s) =====
    IAmAnInboxAsync   [013 ledger 15, listed 2026-09-10]
contents/BuildingAnAsyncPipeline.md:36  private readonly IAmACommandStoreAsync _commandStore;
contents/BuildingAnAsyncPipeline.md:38  public CommandSourcingHandlerAsync(IAmACommandStoreAsync commandStore)

===== UseExternalInbox — 1 site(s) across 1 page(s) =====
    (removed at V10 — not a rename)   [013 ledger 15, listed 2026-09-10]
contents/DispatcherConfigurationReference.md:255  To configure our *Inbox* we then need to use the UseExternalInbox method call…

===== IAmAnIbox — 1 site(s) across 1 page(s) =====
    IAmAnInbox   [013 ledger 15, listed 2026-09-10]
contents/HowBrighterWorks.md:94  30: UseInboxHandlerAsync calls IAmAnIbox\'s AddAsync method to write the command to the Inbox…

===== IMessageScheduler — 11 site(s) across 7 page(s) =====
    IAmAMessageScheduler   [014 design §3.4 — the probe, listed 2026-09-12]
contents/AwsScheduler.md:437       private readonly IMessageScheduler _scheduler;
contents/AzureScheduler.md:344     private readonly IMessageScheduler _scheduler;
contents/AzureScheduler.md:374     private readonly IMessageScheduler _scheduler;
contents/HangfireScheduler.md:429  private readonly IMessageScheduler _scheduler;
contents/InMemoryScheduler.md:284  private readonly IMessageScheduler _scheduler;
contents/InMemoryScheduler.md:357  var scheduler = _serviceProvider.GetRequiredService<IMessageScheduler>();
contents/QuartzScheduler.md:399    private readonly IMessageScheduler _scheduler;
contents/SchedulingAMessage.md:107 private readonly IMessageScheduler _scheduler;
contents/TickerQScheduler.md:194   // Note: You typically need the IMessageScheduler interface here
contents/TickerQScheduler.md:206   // Note: You typically need the IMessageScheduler interface here
contents/TickerQScheduler.md:257   - **Standard**: Fully implements Brighter's `IMessageScheduler` interface.

===== IMessageSchedulerFactory — 2 site(s) across 2 page(s) =====
    IAmAMessageSchedulerFactory   [014 tasks, phase 1 finding D, listed 2026-09-13]
contents/InMemoryOptions.md:241    private static IMessageSchedulerFactory GetSchedulerFactory(
contents/InMemoryScheduler.md:180  static IMessageSchedulerFactory GetSchedulerFactory(

17 site(s) across 11 page(s), from 5 watchlist entries over 161 pages.          exit 1
```

**2 — and stops.** The same tool over a scratch copy with all 17 sites repaired by the gate's own
`word_pattern`:

```text
No watchlisted symbols found (5 entries, 161 pages checked).                    exit 0
```

It says **5 entries**, so that green is not the empty-list green.

**3 — the list is what fires, not the corpus.** A full leave-one-out; each run drops one row and
nothing else:

```text
baseline                        17 site(s) across 11 page(s), from 5 watchlist entries
without IAmACommandStoreAsync   15 site(s) across 10 page(s), from 4 watchlist entries
without UseExternalInbox        16 site(s) across 10 page(s), from 4 watchlist entries
without IAmAnIbox               16 site(s) across 10 page(s), from 4 watchlist entries
without IMessageScheduler        6 site(s) across  5 page(s), from 4 watchlist entries
without IMessageSchedulerFactory 15 site(s) across 10 page(s), from 4 watchlist entries
```

In every run the dropped symbol goes and **the other four keep their exact counts**. Both
additions were run too: a row for a name **absent** from the corpus leaves the findings
byte-identical, and a row for a name **present and unlisted** (`CommandSourcingHandlerAsync`,
5 sites) takes the total to 22. **Removal proves the tool forgets; addition proves it looks.**

**4 — a bad ref cannot pass clean**, and there are two ways to have one:

```text
ref '10.7.O' (letter O)     cannot run: ../Brighter@10.7.O: fatal: unable to
                            resolve revision: 10.7.O                           exit 2
../Darker pointed at as     control failed: CommandProcessor resolves DEAD,
'brighter', ref 4.1.1       expected LIVE (0 files at 4.1.1, 0 at master)      exit 2
```

**5 — `--verify-list` discriminates.** `CommandProcessor` added as a row:

```text
CommandProcessor  brighter  LIVE  43 at 10.7.0, 45 at origin/master
===== 1 entry needs attention =====
CommandProcessor (brighter, line 22): LIVE — 43 files at 10.7.0, 45 at origin/master.
    REMOVE THE ROW. The name exists at both refs; policing it tells a writer to
    replace correct text.                                                      exit 1
```

Those two numbers print as a **control on every run**, green or red.

**6 — the `product` column.** `AddPolicies` has 5 real sites across 3 pages, **all bannered
*Darker V4***; one more was planted on a scratch copy of `PolicyRetryAndCircuitBreaker.md`, a
*Brighter V10* page. One symbol, two rows:

| Row | The gate reports | `--verify-list` says |
|---|---|---|
| `AddPolicies` · **brighter** | **1 site / 1 page** — the planted Brighter page; the 5 Darker sites invisible | **DEAD**, 0 at `10.7.0`, 0 at master — green |
| `AddPolicies` · **darker** | **5 sites / 3 pages** — the Darker V4 pages; the planted site invisible | **LIVE**, 1 at `4.1.1`, 1 at master — exit 1 |

**7 — the opt-out, per symbol.** On `InMemoryScheduler.md`, the one page carrying two listed
symbols, adding `<!-- symbolcheck: allow IMessageScheduler -->` and nothing else:

```text
===== IMessageSchedulerFactory — 1 site(s) across 1 page(s) =====
contents/InMemoryScheduler.md:181  static IMessageSchedulerFactory GetSchedulerFactory(
----- silenced by opt-out (2 site(s)) -----
contents/InMemoryScheduler.md  IMessageScheduler ×2                            exit 1
```

### The gates — task 2.12, predicted unmoved, and unmoved

| Gate | §11.6 | Measured 2026-09-13 |
|---|---|---|
| `linkcheck` | 164 files / 0 broken | **164 / 0** |
| `pagelint` | 0 errors / 768 warnings / 162 pages | **0 / 768 / 162** |
| `--check-shape` | 161 / 12 / widest 12 of 20 | **161 / 12 / 12 of 20**, deepest 4 of 4 |
| `--check-redirects` | 77 entries / 7858 bytes | **77 / 7858** |
| `versioncheck` | 0 of 18 across 5 | **0 of 18 across 5** |
| `optioncheck` | 0 across 59 tables / 519 rows | **0 / 59 / 519** |
| `--verify` | 161 = 161 | **predicted 161, published 161, 161 agree** |
| **`symbolcheck`** | — | **17 sites / 11 pages, exit 1 — red, and required to be** |

Nothing moved, which is what a phase that adds two files under `tools/` and edits no page must
show. `git diff --name-only origin/master -- contents/` is **0** and `SUMMARY.md` is untouched.

### Five findings the task list did not predict

**A. `git grep -w` silently zeroes any pattern that does not begin and end with a word
character.** `git grep -lwF '.Handle('` returns **0** files where the same search without `-w`
returns **23**. A watchlist row like `.AddPolicies(` verified under an unconditional `-w` would
read `DEAD` for ever, whatever the truth. `resolve()` applies the flag only at ends that have a
word character. **This is the plausible-zero failure for the third time in this spec** — empty
token sets in phase 1, the space-blind fence regex before that, now a flag that cannot match.
It is also why the design's `.AddPolicies(` became **`AddPolicies`**: the dotted form is 0 files
in *both* products, because the dot is how a caller writes it and library source holds the
definition.

**B. A green gate is not a repaired page, and red-proof 2 demonstrated it by accident.** The
mechanical stand-in for `UseExternalInbox` produced *"use the **InboxConfiguration** method
call"* — a type described as a method, **wrong prose that `symbolcheck` calls green**. That is
design §4.1's residual row, live. It is why task 3.3 rewrites the paragraph rather than
substituting a token, and it is the sentence to quote if anyone reads a green `symbolcheck` as
*"this page is correct"*.

**C. In a tree with no sibling checkouts, `--verify-list` refuses instead of reporting all-dead.**
The first scratch run said `no checkout at ../Brighter … cannot run without it`, **exit 2**,
rather than resolving five rows against nothing. **That tree is the CI `check` job**, and it is
the evidence behind task 3.6: in the job where `--verify-list` does not belong, it cannot produce
a false green even if someone wires it there.

**D. Two refs give four states, not three.** The three-state ruling names *live*,
*forthcoming-and-said-so* and *a defect*; resolving at two refs also produces **WITHDRAWN** —
present at the pin, gone on master. Each state now carries its own advice, because what it
implies about the *pages* differs, and all three non-dead states mean **remove the row, never
repair it**. Every state was reached with a real name: `CommandProcessor` 43 → 45 LIVE,
`IAmACausationTrackingOutbox` 0 → 12 FORTHCOMING, `RegisterConverters` 3 → 0 WITHDRAWN — which
is Brighter#4276's fix turning up as a state.

**E. The census's number is 932, and that is the third number this spec has had for it.** Phase
1 recorded **831** from a script that no longer exists and reconstructed **881**; the shipped
`--census` measures **932 unresolved across 129 of 145 fenced pages**. Three filter sets, three
numbers, one unchanged conclusion. **932 is the first of them anyone can reproduce with one
command**, which is friction 28's actual repair: the figure can now rot visibly. The shipped
filter deliberately keeps the example-domain nouns the reconstruction dropped — a report a
person reads should over-report rather than hide a real dead name.

### Four decisions phase 2 took that the design did not

1. **No `--watchlist <path>` flag.** A gate that can be pointed at another list can be silenced
   by pointing it at an empty one, and that is one line in a workflow file. Every red-proof ran
   against a scratch copy of `contents/` and `tools/` instead; `git status --short tools/` stayed
   empty throughout.
2. **A malformed or empty watchlist is exit 2, not 1.** Seven faults take that path — no header,
   empty list, bad `product`, wrong field count, blank replacement, duplicate symbol, missing
   file. A tool with nothing to say about the corpus must not say the corpus is red.
3. **`--verify-list` also resolves each row's replacement**, and reports one that is not live.
   The design's own first draft sent `IAmACommandStoreAsync` to `IAmAnOutbox`; a gate that hands
   a writer a dead name is worse than no gate.
4. **An opt-out is never silent.** Suppressed sites print with a count, and one that names an
   unlisted symbol or suppresses nothing is a **warning that does not change the exit code** —
   `0 findings` and `0 findings, 14 silenced` are different claims. Warning rather than error on
   pagelint's rule-6 argument: debt that fails a build gets deleted rather than understood.

---

## Phase 3 — the four repairs, and the CI job

**Goal:** the corpus is right, and the gate that proves it goes green on its first CI run.
Eight tasks, one PR. **Ask before merging: this changes the published site.**

- [x] **Task 3.1:** Repair the scheduler family — **two symbols**, 13 sites across 8 pages
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

- [x] **Task 3.2:** Repair `IAmACommandStoreAsync`, `BuildingAnAsyncPipeline.md:36,38`
  - Input: `src/Paramore.Brighter/Inbox/Handlers/UseInboxHandlerAsync.cs:55,67` at `origin/master`
  - Output: the page's async command-sourcing example
  - Notes: **`IAmAnInboxAsync`** — command sourcing is the Inbox side, and the example is V8-era.
    This one needs reading: the surrounding `CommandSourcingHandlerAsync<T>` is the page's own
    illustrative class and is fine; the interface it depends on is not

- [x] **Task 3.3:** Repair `UseExternalInbox`, `DispatcherConfigurationReference.md:255`
  - Input: the page; `InboxConfiguration` and the V10 inbox registration at `origin/master`
  - Output: the paragraph, rewritten
  - Notes: **not a rename — the method went.** A Reference page naming a method that does not
    exist is the worst case of the four, because Reference is what a reader consults rather than
    reads. Describe what V10 actually does

- [x] **Task 3.4:** Repair `IAmAnIbox` → `IAmAnInbox`, `HowBrighterWorks.md:94`
  - Input: the page
  - Output: one line
  - Notes: a typo, and the cheapest entry in the ledger — which is the point of the ledger

- [x] **Task 3.5:** Wire `symbolcheck.py` into the `check` job of `.github/workflows/docs.yml`
  - Input: the existing `linkcheck`/`pagelint` steps and their comments
  - Output: one step, with a comment saying why it ships **with** the repairs
  - Notes: **`docs.yml` triggers `on: push`.** A gate merged while the corpus is red turns
    `master` red and reddens its own PR. The rule, written into the comment: *a gate and the
    corpus that satisfies it merge together, or the gate merges second*

- [x] **Task 3.6:** Wire `--verify-list` into the **scheduled `versions` job**, not `check`
  - Input: the `versions` job and its existing rationale comment
  - Output: `actions/checkout` for `BrighterCommand/Brighter` and `BrighterCommand/Darker`, plus
    the step
  - Notes: **nothing in the `check` job checks out either repository.** The `versions` job exists
    because *"the event that invalidates a pinned version is a release in another repository"* —
    a watchlist entry is invalidated by exactly that. Fetch depth must reach the pinned tag

- [x] **Task 3.7:** Re-run all eight gates; confirm `symbolcheck` green and **not vacuous**
  - Input: the numbers at `c0d410a`; task 2.12's confirmation that phase 2 moved none
  - Output: the numbers
  - Notes: predicted — seven unmoved except `pagelint` warnings, which may fall if a repaired
    block earns a `using` line; `symbolcheck` **green with 4 entries loaded**. A green run over an
    empty watchlist is the vacuous case, so the run prints the entry count

- [x] **Task 3.8:** Write § *Phase 3 as executed*, with the defect ledger
  - Input: **task 2.11's pasted red output** — that is where the "what the corpus said" column
    comes from, and it was captured in the previous PR precisely so the repair could not erase it
  - Output: what the corpus said, what the product says, and how each was found
  - Notes: standing obligation 2 — **record the mismatch before fixing it.** The evidence lives
    one PR back by design: a ledger written after the repair, from the repaired corpus, proves
    nothing. **Three of the four were found by a person and one by the probe** — record which,
    because that ratio is the argument both for and against the tool

---

## Phase 3 as executed — 2026-09-13, `docs/014-phase3-repairs`

**All eight tasks in one PR. Eleven pages repaired, two CI steps wired, no page added or moved.**
`symbolcheck` went from **17 sites across 11 pages, exit 1** to **exit 0 with 5 entries over 161
pages and 1 site silenced** — the number of pages touched is exactly the 11 AC7 predicts, and
`git diff --name-only c0d410a -- contents/` counts them.

### The defect ledger — five symbols, and what each one actually was

*What the corpus said* is read out of § *Phase 2 as executed*'s red-proof 1, captured one PR
early so that the repair could not erase it.

| # | What the corpus said | What the product says | How it was found | Repair |
|---|---|---|---|---|
| 1 | `IMessageScheduler`, 11 sites / 7 pages | 0 files at `10.7.0` and at master; `IAmAMessageScheduler` 51 at both — **but it is a marker interface with no members** | **the probe** (014 design §3.4) | `IAmAMessageSchedulerAsync` at 8 sites, `IAmAMessageScheduler` named in prose at 1 |
| 2 | `IMessageSchedulerFactory`, 2 sites | 0 at both; `IAmAMessageSchedulerFactory` 11 at both | **the probe**, re-derived — phase 1 finding D | a rename, both sites |
| 3 | `IAmACommandStoreAsync`, 2 sites | 0 at both; `IAmAnInboxAsync` 9 at both | **a person**, 013 ledger 15 | the block rewritten — see below |
| 4 | `UseExternalInbox`, 1 site | 0 at both — removed, not renamed | **a person**, 013 ledger 15 | paragraph rewritten; the name **kept** behind an opt-out |
| 5 | `IAmAnIbox`, 1 site | 0 at both; a typo | **a person**, 013 ledger 15 | `IAmAnInboxAsync`, **not** the `IAmAnInbox` the watchlist suggested |

**Three by a person, two by the probe** — the tasks predicted "three and one", and the split of
row 1 into rows 1 and 2 is where the fourth became a fifth. The ratio is the argument both ways
and it should be read both ways: the probe found the **13 sites that repeat**, a person found the
**3 that do not**, and no amount of either would have found the other.

### The finding that governs the whole phase: **the replacement column is a name, not a type**

`symbolcheck` polices names. Two of the five rows carry a replacement that is the right *name*
and the wrong *type*, and in both cases a mechanical substitution would have gone green and left
the page wrong:

```text
IAmAMessageScheduler   public interface IAmAMessageScheduler;          <- no members at all
IAmAMessageSchedulerAsync : IAmAMessageScheduler    ScheduleAsync, ReSchedulerAsync, CancelAsync
IAmAnInbox             IAmABrighterTracer Tracer { set; }              <- one member, not AddAsync
IAmAnInboxAsync : IAmAnInbox                        AddAsync, ExistsAsync, ...
```

Eight of the scheduler sites declare a field and then `await _scheduler.CancelAsync(...)`. Given
the watchlist's replacement verbatim they would name a live type, satisfy the gate, and **not
compile** — `CancelAsync` is on `IAmAMessageSchedulerAsync`. The same holds for
`HowBrighterWorks.md:94`, the "cheapest entry in the ledger": the sentence has
`UseInboxHandlerAsync` calling `AddAsync`, and the handler's own field is `IAmAnInboxAsync`
(`src/Paramore.Brighter/Inbox/Handlers/UseInboxHandlerAsync.cs:55`). **The cheapest repair in the
ledger was the one where the watchlist's own advice was wrong.**

This is **finding B for the second and third time**, now on `master` rather than on a scratch
copy, and it is the standing answer to anyone who reads a green `symbolcheck` as *"this page is
correct"*. It is also why tasks 3.1–3.4 were specified as read-through repairs: the tool cannot
be given the judgement, so the writer keeps it. **Both sites were verified against the interface
declarations, not against the count of files matching a name.**

**The watchlist rows were left unchanged.** `IAmAnIbox → IAmAnInbox` is true as a typo repair;
what it is not is a type decision, and a replacement column cannot hold one. Making the column
say `IAmAnInboxAsync` would be right for this page and wrong for the next.

### A dead name the watchlist never knew, found by reading the block around a repair

`TickerQScheduler.md:208` called `_scheduler.RescheduleAsync(schedulerId, at)`. Resolved the way
every watchlist row is:

```text
RescheduleAsync    0 files at 10.7.0, 0 at origin/master      <- the docs' name
ReSchedulerAsync   9 files at 10.7.0, 9 at origin/master      <- the product's name
```

It was two lines below a site the gate *did* report, and the gate has nothing to say about it —
it is not on the list, and 014's own census argues nothing cheap can put it there. It was found
because task 3.1 says **substitution with a read-through, not sed**. Repaired to
`ReSchedulerAsync`.

### `BuildingAnAsyncPipeline.md` was three defects deep, and the gate could see one

The dead interface was the shallowest of them:

| What | Was | Is |
|---|---|---|
| the interface | `IAmACommandStoreAsync` — dead | `IAmAnInboxAsync` |
| the call | `AddAsync(command, -1, ct)` | `AddAsync(command, "CommandSourcing", Context as RequestContext, -1, cancellationToken)` |
| the signature | `HandleAsync(T command, CancellationToken? ct = null)` | `HandleAsync(T command, CancellationToken cancellationToken = default)` |
| the body | `Task<T>` with **no return statement** | `return await base.HandleAsync(...)` |

The page's own prose, two paragraphs down, told the reader to *"call **return await
base.HandleAsync(command, ct)**"* — which the code beneath it did not do, and had not done for as
long as the block has been there. **The prose was right and the code was wrong**, which is the
opposite of the failure this spec was built to catch, and no gate in this repository looks for
it. The prose was updated to name the parameter the code now uses.

### The opt-out's first use in the live corpus

`DispatcherConfigurationReference.md` keeps the name `UseExternalInbox` on purpose, in a *Coming
from V9?* note, behind `<!-- symbolcheck: allow UseExternalInbox -->`. The reasoning is the
banner's own: pre-V10 Brighter is well represented in blog posts and Stack Overflow answers, so
the reader most likely to search for that method is the reader who most needs the page saying it
was removed. Deleting the name would have made the gate green by making the page silent.

The run says so out loud, which is the property phase 2 decision 4 bought:

```text
----- silenced by opt-out (1 site(s)) -----
contents/DispatcherConfigurationReference.md  UseExternalInbox ×1

No watchlisted symbols found (5 entries, 161 pages checked, 1 silenced).          exit 0
```

**`0 findings` and `0 findings, 1 silenced` are different claims**, and only the second is true
of this corpus. Red-proof 7 showed the opt-out working on a scratch copy; this is the first time
it has run on `master`.

### The CI wiring, and the control that proves `fetch-depth: 0` is load-bearing

Task 3.5 puts `symbolcheck.py` in the `check` job beside `pagelint`, in the same PR as the
repairs it polices, with the rule written into the comment. Task 3.6 puts `--verify-list` in the
scheduled `versions` job — **and that job now checks this repository out into `Docs/` with
`Brighter/` and `Darker/` beside it**, because `symbolcheck` resolves rows at `../Brighter` and
`actions/checkout` cannot write outside the workspace. Every step in that job names its
`working-directory`.

The layout was **run, not reasoned about** — three local clones arranged as CI arranges them:

```text
Docs/ Brighter/ Darker/ as siblings     control: CommandProcessor LIVE  43 at 10.7.0, 45 at master
                                        control: IAmAnIbox        DEAD   0 at 10.7.0,  0 at master
                                        All 5 entries still dead …                        exit 0
no siblings at all                      no checkout at ../Brighter … cannot run without it exit 2
Brighter cloned --depth 1               ../Brighter@origin/master: fatal: unable to resolve
  (tag present, origin/master absent)   revision: origin/master                            exit 2
```

The third row is the two-way control for the `fetch-depth: 0` line: remove it and the step does
not quietly report five dead rows, it refuses. A pinned tag survives a shallow clone and
`origin/master` does not, so the shallow failure is the one that would have been plausible.

### The gates — task 3.7, predicted and measured

| Gate | At `c0d410a` | Now | Predicted |
|---|---|---|---|
| `linkcheck` | 164 files / 0 broken | **164 / 0** | unmoved ✓ |
| `pagelint` | 0 errors / 768 warnings / 162 pages | **0 / 757 / 162** | *"may fall if a repaired block earns a `using` line"* ✓ |
| `--check-shape` | 161 / 12 / widest 12 of 20 | **161 / 12 / 12 of 20**, deepest 4 of 4 | unmoved ✓ |
| `--check-redirects` | 77 entries / 7858 bytes | **77 / 7858** | unmoved ✓ |
| `versioncheck` | 0 stale of 18 across 5 | **0 of 18 across 5** | unmoved ✓ |
| `optioncheck` | 0 across 59 tables / 519 rows | **0 / 59 / 519** | unmoved ✓ |
| `--verify` | 161 = 161 | **predicted 161, published 161, 161 agree** | unmoved ✓ |
| **`symbolcheck`** | 17 sites / 11 pages, exit 1 | **0 findings, 5 entries, 161 pages, 1 silenced, exit 0** | green and **not vacuous** ✓ |

**The one number that moved was diffed rather than asserted.** 768 → 757 is **eleven** warnings,
and eleven is not ten:

```text
$ diff <(pagelint at HEAD~) <(pagelint now) | grep -c '^<.*USING DIRECTIVES'    11
$ diff … | grep -v 'USING DIRECTIVES' | grep '^[<>]'    (only the summary line)
```

Ten came from the strict `--changed` pass, which turns rule 6 into an error for any block
overlapping the diff and named exactly ten. The eleventh is `BuildingAnAsyncPipeline.md`'s block,
which had earned its `using` lines during task 3.2 *before* that pass ran and so was never in its
error list. No warning of any other class moved. The strict pass reports **11 files, 38 hunks, 11
pages, 13 code blocks strict, 0 errors**.

**Task 3.7's own text predicted "green with 4 entries loaded".** It is **5**, and that figure is
the pre-finding-D count surviving in a sentence the amendment banners did not reach. Recorded
rather than quietly corrected: it is friction 29 — *an inherited count re-quoted at the wrong
grain* — turning up one phase after it was written down.

### Three decisions phase 3 took that the design did not

1. **Repairs target the async interface, not the marker.** The watchlist's replacement is advice
   about a *name*; which type a block should name is a judgement about the code around it, and it
   stays with the writer. The rows were not edited to say otherwise.
2. **A removed name may stay in prose, behind a visible opt-out.** The alternative — deleting
   every mention of `UseExternalInbox` — makes the gate green by making the docs useless to the
   reader arriving from V9. The opt-out is per symbol, prints its count, and is the reason this
   is a decision and not a hole.
3. **A defect found beside a repair gets repaired.** `RescheduleAsync` is not on the watchlist and
   will never be; it was two lines from one that is. Task 3.1's *"read-through, not sed"* is what
   found it, and the phase treats that instruction as covering what the reading turns up.

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
