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
| **4** | **The three P0 commands, and the committed home they cite** | 6 | D5, D2, D6, D10 |
| **5** | **The three P1 commands and P2-1** | 6 | D3, D4, D1, D7 |
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
4. **Prose and permission ship together, and the check runs in both directions.** A command
   instructing a tool run gets the matching `allowed-tools` entry in the same commit (requirements
   constraint 3) — and a grant no reading of the prose reaches is a permission nobody asked for.
   **AMENDED 2026-09-13**, twice by evidence: the reverse direction found `/spec:requirements`
   holding `Bash(touch:*)` in phase 4, and the forward direction — re-run properly in phase 5 —
   found **eleven forbidden invocations in seven commands**, every one of them inside a `!`
   context block that runs on every invocation.
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

> **CORRECTED 2026-09-13 by phase 5. The mechanism below is wrong and the numbers name no
> corpus.** `-w` tests the characters *adjacent to the match*, not the ends of the pattern, so
> such a row matches wherever its neighbours are also non-word — `>.Handle()` does, `.Handle(x)`
> does not. The pair *0 / 23* reproduces in no single repository: `../Brighter` gives **0 / 179**,
> this one **4 / 22**. **The decision the finding supports is unchanged and correct**; only its
> reason and its figures are. Left standing, struck rather than rewritten, per obligation 2.

**A. ~~`git grep -w` silently zeroes any pattern that does not begin and end with a word
character.~~** `git grep -lwF '.Handle('` returns **0** files where the same search without `-w`
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
~~Five tasks~~ **six tasks**, one PR.

> **AMENDED 2026-09-13, at the top of the phase: task 5.1 executes here, not in phase 5.**
> Task 4.3 requires `/spec:review` to cite its numbers from `tools/README.md`, and 5.1 is what
> writes that file. Shipping 4.3 first would merge a command citing a file that does not exist —
> the failure this spec was written to stop, committed by the spec itself. The phase table in §0
> and `## Phase 5` below carry the same amendment. **The 39-task total does not move**; 5.1 keeps
> its number, because renumbering is how a spec's two counts start disagreeing.

- [x] **Task 4.1:** Rewrite `/spec:implement` (D5), 42 → ~70 lines
  - Input: `design.md` §6.1; `CLAUDE.md` §§ *File Organization Pattern*, *Page banner*,
    *The opening sentence*, *Page descriptions*
  - Output: `.claude/commands/spec/implement.md`
  - Notes: mode-aware structure (`## Step N:` for tutorials **and how-tos**); the banner; API
    liveness with the three states and the corpus's existing `> **Not in a released package
    yet.**` form; the compile obligation; and a **Quality Check split in two** — what a tool
    decides, and what you decide

- [x] **Task 4.2:** Rewrite `/spec:requirements` (D2), 48 → ~85 lines
  - Input: `design.md` §6.2
  - Output: `.claude/commands/spec/requirements.md`
  - Notes: the **subject declaration** — `feature` · `reader problem` · `process` — which decides
    both the research steps and which sections apply; acceptance criteria **each naming an
    instrument or marked as having none**; open questions by name; re-derive the README. Phrase
    criteria as *contains*, not *ends with* (defect 16), and say that **"a file exists" is not an
    instrument** (defect 17)

- [x] **Task 4.3:** Rewrite `/spec:review` (D6), 87 → ~120 lines
  - Input: `design.md` §6.3; the existing `linkcheck` block, which is the model to copy
  - Output: `.claude/commands/spec/review.md`
  - Notes: **all eight gates** with the pre-existing-versus-yours discipline; **the numbers are
    cited from `tools/README.md`, never pasted**; and a fourth phase checklist, **Acceptance**,
    which walks the no-instrument criteria first

- [x] **Task 4.4:** Update `allowed-tools` on all three, in the same commit as their prose
  - Input: the three frontmatter blocks; the eight gate commands from `tools/README.md`'s draft
  - Output: the three frontmatter blocks
  - Notes: standing obligation 4. Today **one command of nine permits any `python3`**, and only
    `linkcheck.py`. A prose-only change would read as covered and be forbidden

- [x] **Task 4.5:** Write § *Phase 4 as executed*
  - Input: tasks 4.1–4.4
  - Output: a section in this file, with the before/after line counts per command
  - Notes: record any place where citing `CLAUDE.md` was not possible because the convention is
    not written there — those are D10's and `CLAUDE.md`'s gaps, and phase 5 owns them

---

## Phase 4 as executed — 2026-09-13, `docs/014-phase4-p0-commands`

**Six tasks in one PR — 4.1 to 4.5 and 5.1, pulled forward.** No page under `contents/` changes,
so no merge sign-off was owed. The three P0 commands went from **177 lines to 323**, and
`tools/README.md` arrived at **158**.

| File | Before | After | Design target | Δ |
|---|---:|---:|---:|---|
| `.claude/commands/spec/implement.md` | 42 | **89** | ~70 | +47 |
| `.claude/commands/spec/requirements.md` | 48 | **90** | ~85 | +42 |
| `.claude/commands/spec/review.md` | 87 | **144** | ~120 | +57 |
| `tools/README.md` | — | **158** | — | new |
| **All nine commands** | **451** | **597** | — | +146 |

451 is requirements constraint 4's figure, re-derived here from `fc77c42` rather than quoted, and
it agreed. **Two of the three commands overshot their target by about 20%** and `implement.md` by
27%; every section in the overshoot is one §6 names, so the choice was to miss the target or drop
a mandated section. Recorded rather than trimmed — constraint 4 asks that growth be *justified*,
not that it be small.

### The sequencing decision, taken before any file was opened

Task 4.3 requires the gate numbers to be **cited from `tools/README.md`, never pasted**, and task
5.1 writes that file one phase later. The three options were to cite forward at a file that does
not exist, to paste the numbers and replace them in phase 5, or to move 5.1 here. **5.1 moved.**
Pasting is the drift `optioncheck` exists to prevent, aimed at ourselves; citing forward merges a
command carrying a dead reference, which is this spec's own subject. Nothing in the dependency
graph objected — 5.1's only input is the gate numbers.

### Gate movement: **predicted seven unmoved and one moved, and that is what happened**

**The handover predicted "none" for all eight, and it was wrong.** `linkcheck`'s corpus is the
**repository**, not the published tree: `tools/` is not in its `SKIP_DIRS`, so a new
`tools/README.md` enters it.

| Gate | At `fc77c42` | After | |
|---|---|---|---|
| `linkcheck` | 164 files / 0 broken | **165 files / 0 broken** | **moved, as predicted** |
| `pagelint` | 0 errors / 757 warnings / 162 pages | unchanged | scope is `contents/` + `ROOT_PAGES` |
| shape · redirects · `versioncheck` · `optioncheck` · `--verify` · `symbolcheck` | — | unchanged | no page, pin, table or `SUMMARY.md` entry touched |

The prediction was written to the scratchpad **before** the first file was created, with the
+1 named and its cause given. That is standing obligation 6 doing the work it exists for: the
number moved, and nobody had to decide after the fact whether it was supposed to.

### Four findings

**A. An anchored link into `CLAUDE.md` is reported `MISSING ANCHOR`, even when the heading
exists.** `CLAUDE.md` is in `linkcheck`'s `SKIP_FILES`, so the file resolves through
`os.path.exists` but its headings are never indexed. Found by probing before writing, with a
two-way control — a probe file with live links reported 0 problems across 165 files, the same file
with a dead link reported 3. **Consequence:** `tools/README.md` cites `CLAUDE.md` sections as
prose, never as anchored links. The commands live under `.claude/`, which is in `SKIP_DIRS`, and
are not subject to it.

**B. `CLAUDE.md` states a measurement in the present tense that its own rule has since
falsified.** Twice — *"`## Configuration` appears on 26 pages"* and *"`## Configuration` and
`## Best Practices` each appear on 26 pages"*. Measured on this branch: **0 and 0**, because spec
011 requalified them. The claims are sound as *motivation* and false as *fact*, and the cost was
real — the first attempt to red-proof the `pagelint` filter planted `## Configuration` in
`Glossary.md`, collided with nothing, and reported 0 errors. A tool looked vacuous for one round
because a rationale sentence read as current state. **This is a `CLAUDE.md` gap and phase 5's task
5.5 owns the file** — it is not repaired here, because a "while I'm here" edit to `CLAUDE.md` is
exactly what design §10 warns 014 is most likely to do quietly.

**C. Obligation 4 is stated one-way, and the reverse direction is where the defect was.** *"Prose
and permission ship together"* catches prose naming a tool the frontmatter forbids. Reading it
backwards — **which grants does the prose never use?** — found `/spec:requirements` holding
`Bash(touch:*)`, which is what creates `.requirements-approved`. **The command that writes the
requirements was permitted to approve them**, with no prose anywhere asking it to. Removed, along
with a `Bash(test:*)` narrowed to `test:spec/*`. P2-2's `commandlint` should check both
directions, not just the stated one.

**D. Task 4.3's own rule caught task 4.3.** The first draft of `review.md` explained the `pagelint`
filter with *"757 of them would bury everything else"* — a pasted gate number, in the file whose
job is to stop pasting gate numbers, written minutes after writing the rule. Caught by grepping
the three commands for every figure in `tools/README.md`. **A rule you have just written is not a
rule you are yet following.**

### Where citing `CLAUDE.md` was not possible

Two conventions the commands now state are **not in `CLAUDE.md` at all**, so they are stated in
the command rather than cited, and phase 5 owns the gap:

1. **The compile obligation.** That a C# block is built against the **released packages** rather
   than `ProjectReference`s into `src/` is 013 phase 2's finding F, recorded in a closed spec's
   `tasks.md` and nowhere a writer would look. `CLAUDE.md` § *Code Example Standards* says
   *"test all code examples"* and stops.
2. **That behaviour must be run, not compiled, and always with a control.** Four compiling,
   reviewed examples in this programme asserted false behaviour. `CLAUDE.md` has no rule, no
   ledger row, and no mention.

Both belong in `CLAUDE.md` § *Code Example Best Practices*. **Neither is added here** — phase 4's
PR touches no `contents/` page and no `CLAUDE.md` line, and it keeps that property.

### A decision taken, reversed, and why the reversal is the point

**`/spec:review` pre-runs all eight gates.** It first pre-ran **five**, on measured grounds:
`linkcheck`, `pagelint`, `--check-shape`, `--check-redirects` and `symbolcheck` cost **0.1–0.4s
each** and read only this repository, against 2.6s for `versioncheck` (NuGet), 1.8s for a warm
`optioncheck` (a build) and 0.5s for `--verify` (the live sitemap) — and a review command that is
slow or weather-dependent is one people learn to skip.

**Checking AC5 against its own stated instrument is what reversed it.** AC5 reads *"all eight
gates are named in `.claude/commands/spec/`, and `/spec:review` runs eight"*, and five-plus-three
satisfies the first clause and argues with the second. Design §6.3 — written later — says only
*"all eight gates"*, which the five-inline shape could be read to satisfy; **the approved
requirement is the narrower of the two and it wins.** The measured cost of the deviation was about
5 seconds, which is not grounds to reinterpret a criterion. The prose absorbs the flakiness
instead: three gates may print **exit 2**, and the command says in as many words that exit 2 is
*unchecked*, not clean, and is re-run once before being believed.

**This is the sequencing decision's lesson arriving twice in one phase.** Both times the question
was whether to bend a rule this spec had just written, and both times the answer was to move the
work instead.

### Two decisions phase 4 took that the design did not

1. **`pagelint` is piped through `grep -v '(warning)'` in the command's context block.** A bare
   run injects hundreds of debt lines. The filter was red-proved rather than assumed: with a
   cross-page collision and an in-page repeat planted in `Glossary.md`, **both errors and the
   summary survived the filter**, and the corpus was restored and re-measured at 0 errors
   afterwards.
2. **AC6 was measured, not asserted, with a throwaway checker — and the throwaway is written
   down.** Friction 28's rule is that a single-use instrument owes its output the exact commands,
   so: the check parses each command's `allowed-tools`, expands globs like
   `Bash(python3 tools/*.py:*)`, and runs **both directions**. Result across all nine commands:
   **0 unpermitted in the three P0 commands**, and one in `new.md` — *prose names `wc -l`, the
   frontmatter does not permit it* — which is **phase 5's task 5.4** and is left alone here.
   The reverse direction reports 17 unused grants, and **most are not defects**: `Bash(test:spec/*)`
   backs prose that says *"a `.tasks-approved` file in the spec directory"* without naming a
   command. **That is the honest boundary of the reverse check and P2-2 should inherit it** — the
   grants worth removing are the ones no reading of the prose reaches, which is how
   `Bash(touch:*)` was found.

---

## Phase 5 — the three P1 commands and P2-1

**Goal:** the rest of the workflow. ~~Seven tasks~~ **six tasks**, one PR — **task 5.1 shipped in
phase 4**, for the reason recorded in that phase's amendment banner. Its box is ticked below and
its number is unchanged.

- [x] **Task 5.1:** Write `tools/README.md` (D10)
  - Input: `design.md` §7.4; the gate numbers at `c0d410a`; 012 §1's phase-is-a-PR contract
  - Output: a new file — the eight commands, what each checks, **the expected numbers at a named
    ref**, and the contract
  - Notes: **only what a command cites moves out of `PROMPT.md`** (Q3). The board, branch tips, CI
    census and open-question log stay there, and `PROMPT.md` stays ignored. Every number carries
    the ref it was measured at, or it is not a fact

- [x] **Task 5.2:** Rewrite `/spec:design` (D3), 54 → ~85 lines
  - Input: `design.md` §7.1
  - Output: `.claude/commands/spec/design.md`
  - Notes: page type per file; the conventions, cited; **verify the API the design will print**;
    **re-verify the requirements rather than quote them**; predict gate movement including *none*.
    Add the step friction 23 asks for: **a design resting on an unmeasured number says which
    number and how it will be measured, before approval**

- [x] **Task 5.3:** Rewrite `/spec:tasks` (D4), 63 → ~95 lines
  - Input: `design.md` §7.2; §1 of this file, which is the worked example
  - Output: `.claude/commands/spec/tasks.md`
  - Notes: phase-is-a-PR, citing `tools/README.md`; **the four-phase template kept but explicitly
    labelled a default**; a standing-obligations section; red-proofs; record-before-fixing;
    re-derive inherited counts; an acceptance phase last

- [x] **Task 5.4:** Rewrite `/spec:new` (D1), 63 → ~75 lines
  - Input: `design.md` §7.3; this spec's own README, which the re-derive instruction is drawn from
  - Output: `.claude/commands/spec/new.md`
  - Notes: the README template gains **Acceptance criteria**, **Open questions**, and the
    re-derive instruction. 013's README named six gaps of which five were closed; 014's defect
    count was wrong by two before this spec started.
    **Carries an AC6 finding from phase 4, and it is wider than first recorded**: the command's
    own context block runs `ls -la spec/ | grep "^d" | wc -l | xargs -I {} echo …` against an
    `allowed-tools` of `Bash(mkdir:*), Bash(echo:*), Bash(date:*), Bash(ls:*)` — **`grep`, `wc`
    and `xargs` are all unpermitted**, and a `!` block is one shell invocation, so the whole line
    is forbidden rather than three-quarters of it. ~~The only unpermitted invocation in the nine
    commands~~ — **it was not: phase 5's re-run found eleven more, in seven other commands.**
    See § *Phase 5 as executed*

- [x] **Task 5.5:** Add the `CLAUDE.md` note at rule 2, **and three more from phase 4**
  - Input: `CLAUDE.md` §§ *Page banner*, *Heading qualification*, *Code Example Best Practices*;
    `design.md` §4.3; § *Phase 4 as executed*
  - Output: one line recording that `symbolcheck` reads the banner for product, **plus phase 4's
    three**
  - Notes: rule 2 is now a dependency of a second tool. Nobody should be able to weaken the banner
    without seeing what rests on it. Phase 4's three: the **two stale present-tense counts**
    (*"`## Configuration` appears on 26 pages"*, said twice — it appears on **0**, because the
    rule those sentences justify was then applied); the
    **compile-against-released-packages** obligation; and **behaviour must be run, not compiled,
    and always with a control**. The last two are stated in `/spec:implement` rather than cited,
    because `CLAUDE.md` has nowhere to cite them to

- [x] **Task 5.6:** Add the evidence requirement to `/spec:update-task` (D7, P2-1)
  - Input: `requirements.md` §3.1; 013's ledger entry 13
  - Output: `.claude/commands/spec/update-task.md`
  - Notes: the task's stated **Output** must exist before the box flips; where it cannot be
    checked, the tick says so. 013's task 4.8 was ticked with its two `pagetypes.tsv` rows
    unwritten and reached `master`

- [x] **Task 5.7:** Write § *Phase 5 as executed*
  - Input: tasks 5.1–5.6
  - Output: a section in this file, including the P2 strikes
  - Notes: **P2-2 (`commandlint`) and P2-3 (`/spec:status` gate state) are not attempted.** P2 is
    *nice to have* and AC1 binds P0 only. Record them as struck-with-a-reason, not as owed

---

## Phase 5 as executed — 2026-09-13, `docs/014-phase5-p1-commands`

**Six tasks in one PR — 5.2 to 5.7, plus the folded-in `a27d36e`.** No page under `contents/`
changes, so no merge sign-off is owed. The four rewritten commands went from **204 lines to 365**,
and all nine from **597 to 758**.

| File | Before | After | Design target | Δ |
|---|---:|---:|---:|---|
| `.claude/commands/spec/design.md` | 54 | **115** | ~85 | +61 |
| `.claude/commands/spec/tasks.md` | 63 | **106** | ~95 | +43 |
| `.claude/commands/spec/new.md` | 63 | **97** | ~75 | +34 |
| `.claude/commands/spec/update-task.md` | 24 | **47** | — | +23 |
| **All nine commands** | **597** | **758** | — | +161 |

Two methods, and they agree: `wc -l` over the nine sums to 758, and the nine rows above sum to 758
independently. 597 is phase 4's figure, re-derived from `412fd34` rather than quoted.

**Every rewrite overshot, `design.md` by 35%**, and the overshoot is the same trade phase 4
recorded: each section maps to something §7 mandates, so the choice was to miss the target or drop
a mandated section. `new.md`'s +34 is mostly inside its fenced README template, which gained the
two sections §7.3 asks for. Constraint 4 asks that growth be justified, not that it be small.

### Gate movement: predicted eight unmoved, and eight were

The prediction went to the scratchpad before the first file was opened — including the reasoning
that **phase 5 creates no new file anywhere**, which is what made "none" defensible this time after
phase 4's `tools/` surprise. `linkcheck` holds at 165 because editing a file already inside its
walk does not change a file count; `.claude/` is a `SKIP_DIR`, `CLAUDE.md` a `SKIP_FILE`, `spec/`
skipped. All eight re-measured after the work: unmoved.

**`tools/README.md` was re-referenced from `fc77c42` to `412fd34`, and that is not gate movement.**
Its `linkcheck` row read **164**, true at the ref it named and one merge behind `master`, where the
answer is **165** — the +1 that file's own arrival caused. Correcting a citation's ref is not a
number moving; the row now says so in as many words, so the next reader does not have to work it
out twice.

### The finding: **obligation 4's forward direction had never actually been run**

Phase 4 reported *"0 unpermitted in the three P0 commands"* and one in `new.md`. Re-running the
check in phase 5 — against all nine, with pipelines split — found **eleven forbidden invocations
across seven commands**:

| Command | Forbidden invocation | Where |
|---|---|---|
| `design`, `implement`, `requirements`, `review`, `tasks`, `update-task` | `echo "No active spec"` | the `!` context block |
| `status` | `echo "None"` | the `!` context block |
| `review` | `tail -20`, `tail -5` ×3 | the `!` gate blocks |

Every one sits in a `!` block — **the part that runs automatically on every single invocation of
the command**, before the model does anything. `!`cat spec/.current-spec 2>/dev/null || echo "No
active spec"`` needs `echo` permitted, by the same ruling that made `new.md`'s whole `ls | grep |
wc | xargs` line forbidden rather than three-quarters forbidden: **a `!` block is one shell
invocation.**

**The corpus was already arguing with itself and nobody had read it.** `switch.md` and the old
`new.md` both granted `Bash(echo:*)`; the other seven used `echo` and did not. Two of nine authors
thought it was required. Nothing checked which was right.

**Why phase 4's run missed it** is the more useful half. Its checker looked at the *tools a
command's prose names* — `python3 tools/pagelint.py`, `dotnet run` — and `echo` in a fallback does
not read as naming a tool. The instrument encoded the question as *"which tools does this command
tell you to run?"* when the permission model asks *"which binaries will this shell line execute?"*
**Same words, different question, and the gap between them is a check that returns 0 for eleven
live defects.** This is the plausible-zero failure for the **fifth** time in 014 — phase 2's
handover predicted a fifth would arrive, and it arrived inside the obligation written to catch it.

**Repaired:** `Bash(echo:*)` on seven commands, `Bash(tail:*)` on `review.md`, in the same commit
as the prose. The re-run reports **0 unpermitted across 9 commands**, and the checker was
red-proved two ways first — a planted `sed -n` in a fence is reported, the unmodified file is not.
Per friction 28 the throwaway owes its exact command:

```bash
python3 <scratch>/permcheck.py .claude/commands/spec/*.md    # 0 unpermitted across 9 commands
```

Its stated boundary is phase 4's: the reverse direction over-reports, because a grant backing prose
that describes an action without naming a command — `Bash(test:spec/*)` behind *"a
`.tasks-approved` file in the spec directory"* — is not a defect. **P2-2 inherits the forward
direction's real definition: parse the shell, not the prose.**

### The second finding: a mechanism this spec explained wrongly, in two places, in shipped tooling

Writing `/spec:design`'s API-verification section meant restating 014's own `git grep -w` trap. It
does not reproduce. The recorded claim — *"a pattern whose last character is not a word character
can never satisfy the flag"*, in § *Phase 2 as executed* finding A, in `resolve()`'s docstring, and
in the handover — is **false**.

**What is actually true**, probed in a scratch repository with a four-case control:

| Case | `-w` | plain |
|---|---|---|
| `handler.Handle(command);` | miss | match |
| `IHandleRequests<T>.Handle()` | **match** | match |
| `x.Handle( y` | miss | match |
| `[.Handle(]` | **match** | match |

`-w` tests the characters **adjacent to the match**, not the ends of the pattern. So a pattern with
non-word edges matches wherever its neighbours are also non-word — common in prose, near-absent in
code, where the next character after `(` is an argument. **The zero is a property of the corpus,
not of the flag.**

**And the recorded numbers name no corpus.** *"0 files where the same search without `-w` returns
23"* is reproducible in neither repository: in `../Brighter` it is **0 against 179**; in this one,
at the ref the finding was written at, **4 against 22**. The pair mixes corpora. Re-derived at four
refs, the Docs figure was never 0 — it was **2** before 014 and **4** after phase 2, three of those
four matches being the finding's own text quoting itself.

`word_pattern()`'s docstring was wrong in the **mirror image**: it said an unconditional `\b` would
mean the row *"would never match anything"*. Measured, `\b\.AddPolicies\(\b` matches the **code**
form and misses all three **prose** forms — which is worse than never matching, because the gate's
corpus is prose. Python's `\b` demands a word character at a non-word edge; git's `-w` demands a
non-word one. **Two instruments, opposite meanings for the same idea, and this spec had a sentence
about each that got the direction wrong.**

**Both docstrings repaired, no behaviour changed** — `word_pattern` still returns
`\.AddPolicies\(` bare and `(?<![A-Za-z0-9_])CommandProcessor(?![A-Za-z0-9_])` guarded, and
`symbolcheck` still exits 0 with *5 entries, 161 pages, 1 silenced*. **The conclusion the false
mechanism supported was right all along**, which is exactly why it survived: a correct decision
protects its own bad reasoning from review. Finding A is struck through rather than rewritten, per
obligation 2.

### `CLAUDE.md` — four edits, and two of them are new rows in the ledger

1. **Rule 2 is a dependency of a second tool.** `symbolcheck` reads the banner's *Applies to* for
   the page's product, importing `APPLIES_TO` and `BANNER_RE` from `pagelint` rather than keeping a
   copy. Verified in the source, not taken from the design: a page with **no readable banner
   declares no product and is checked against every row**, not exempted from any.
2. and 3. **The two stale present-tense counts, repaired.** *"`## Configuration` appears on 26
   pages"*, twice. Re-derived two ways — `grep -rl` counting files and `grep -rh` counting lines —
   **0 and 0**, and 0 as far back as `c0d410a`. Both now read as history with the measurement
   beside them, because the sentences are sound as motivation and were false as fact.
4. **The two homeless obligations got a home** — § *Compiling an example, and against what*: a
   block compiles **against the released packages**, not a `ProjectReference` into `src/`; and a
   block asserting **behaviour is run, with a control**. `/spec:implement` stated both because
   `CLAUDE.md` had nowhere to cite; it can cite now.

**The ledger went from 17 rows to 19**, both review-only, joining version markers. That is the
ledger's own discipline applied — *"a rule in only one of the two places is how the next round of
decay begins"* — and it is why task 6.2's input line was amended in the same commit rather than
left to be re-derived later. **Friction 29 says to grep the whole spec when a count moves**, and
one occurrence was found.

### The P2 strikes — struck, not owed

- **P2-2, `commandlint`:** struck. AC1 binds P0 only. It now has a **sharper specification than it
  started with**, bought by this phase: parse each `!` block and fenced command as a shell line,
  check every binary in the pipeline against the expanded `allowed-tools` globs, run both
  directions, and report the reverse direction as advisory. A throwaway proved the check is worth
  ~40 lines and finds real defects on its first run — **twice now, in two directions**.
- **P2-3, `/spec:status` reports gate state:** struck. `/spec:review` pre-runs all eight, and a
  second command doing the same work would be a second place for the numbers to drift.

Both are recorded here as struck-with-a-reason so that the acceptance walk does not report them as
outstanding.

### Three decisions phase 5 took that the design did not

1. **The three P0 commands and `status.md` were edited, in a phase that owns neither.** Ruling 7 —
   a defect found beside a repair gets repaired — against design §10's warning about *"while I'm
   here"*. The tie-break: the defect class was found by phase 5's own instrument, the fix is one
   frontmatter token per file, and shipping three commands with forbidden context blocks while
   fixing three others would leave the corpus in a state no reading of obligation 4 defends.
2. **`tools/symbolcheck.py`'s comments were edited by a phase about commands.** Same ruling. The
   alternative was to ship `/spec:design` telling a writer one thing about `git grep -w` while the
   tool's own source told them the opposite.
3. **`new.md`'s context block was replaced, not repaired.** `ls -la | grep "^d" | wc -l | xargs`
   produced a *count* of specs; step 1 of the task needs the *next ID*. The replacement,
   `ls -d spec/*/`, needs one grant instead of four and answers the question actually being asked —
   the permission defect was a symptom of a context block that printed the wrong thing.

---

## Phase 6 — acceptance

**Goal:** walk AC1–AC12 with evidence and find what the phases did not. Six tasks, one PR.

- [x] **Task 6.1:** Walk AC1–AC12 forwards, one paragraph each, naming the command and its output
  - Input: `requirements.md` §12, as amended by the design review
  - Output: one paragraph per criterion, each naming its instrument and that instrument's output
  - Notes: **start with the seven-and-a-half that have no tool** — AC1, AC2, AC3, AC4, AC10,
    AC12, AC6, and half each of AC8 and AC11. Both criteria ever found unmet at a close were
    unmarked ones. **Use a census, not a `tail`**: 013's only false finding came from
    `grep '^## ' | tail -4` on a seven-step page

- [x] **Task 6.2:** Walk AC2 — no command contradicts `CLAUDE.md` — convention by convention
  - Input: `CLAUDE.md`'s rule ledger — **nineteen rows after task 5.5**, seventeen when this task
    was written; the nine commands as rewritten
  - Output: a table, convention against the command that would cause a writer to break it
  - Notes: this is the criterion 014 exists to satisfy and it has **no tool**. Walking it by
    reading the commands against `CLAUDE.md`'s ledger is the whole test

- [x] **Task 6.3:** Walk AC7 backwards — what changed that should not have
  - Input: `origin/master` at the pre-014 tip
  - Output: `git diff --name-only origin/master -- contents/ | wc -l` = **11**, and
    `git diff --stat origin/master -- SUMMARY.md` empty
  - Notes: **the criterion most likely to break quietly.** A spec about the workflow is the
    easiest place to talk yourself into a "while I'm here" edit

- [x] **Task 6.4:** Write the defect ledger — every defect 014 found, and what the product says
  - Input: task 3.8's ledger; task 2.11's red output; the eighteen workflow defects
  - Output: the ledger table, with a *found by* column
  - Notes: it already has two entries the phases did not predict — `IMessageScheduler`, found by
    the probe on seven pages, and **`IMessageSchedulerFactory`, found by phase 1's write-up while
    re-deriving the first one's site count** — both green under all seven gates for as long as
    they have existed. The *found by* column has to distinguish them: one is the instrument's
    win, the other is the re-derivation's

- [x] **Task 6.5:** Write the friction ledger — 014's own, ~~continuing at 30~~ **collected at 35**
  - Input: `requirements.md` §14 (18–21), `design.md` §13 (22–24), §4 of this file (25–29, of
    which **28 and 29 were met in phase 1**)
  - Output: the collected ledger, plus whatever phases 2–5 met
  - Notes: **the ledger is 014's product as much as the commands are** — it is what the next
    workflow spec inherits, and the reason this one had eighteen defects to work from

- [x] **Task 6.6:** Close — README checklist, `spec/.current-spec`, and what the next session needs
  - Input: the README's status checklist; `design.md` §4.1's coverage matrix
  - Output: the checklist ticked, and a closing section naming the residual gap
  - Notes: **do not repoint `.current-spec` until a next spec exists.** Record the residual gap —
    *a dead API written into prose on an existing page, uncompiled and unlisted, is caught by
    nothing* — because that is the line the next spec starts from, and the one to quote if anyone
    claims 014 closed defect 8

---

## Phase 6 as executed — 2026-09-16, `docs/014-phase6-acceptance`

**Twelve of twelve criteria are met, and the walk took five repairs to get there.** Two of them
were true when written and made false by a later edit elsewhere; one was a rule stated in only one
of its two directions; one was a checklist that never existed for a convention delegated to it; and
one had been live since before 014 began, green under the check written **one phase earlier to find
exactly that class of thing**.

**All five came out of AC2 and AC6, which are two of the seven and a half criteria with no
instrument.** The requirements said out loud that carrying more hand-walked criteria than
instrumented ones was *"worse than it looks"*; what the walk shows is narrower and less comfortable
than that. The instrumented criteria were all green and all uninformative. **Every finding 014 has
to show for its acceptance phase came from reading.**

### Gate movement: predicted eight unmoved, and eight were

The prediction went into the scratchpad before a file was opened, and this time the *why* was the
work: phase 6 edits `spec/`, `.claude/` and `CLAUDE.md`, and **none of the three is in any gate's
corpus**. `spec/` and `.claude/` are both in `linkcheck`'s `SKIP_DIRS` and `CLAUDE.md` is a skipped
file; `pagelint`'s corpus is `contents/` plus the root `README.md`; the three `urlmap` modes read
`SUMMARY.md`; `versioncheck`, `optioncheck` and `symbolcheck` read pages. **Phase 6 creates no file
anywhere**, which is the clause that made phase 5's "none" defensible and makes this one so.

Measured after the work — all eight at their `373e9c3` figures, which are `tools/README.md`'s
figures at `412fd34` unchanged:

```text
No broken internal links (165 files checked).
0 errors, 757 warnings (using-directive debt: 757 blocks across 116 pages) across 162 pages.
0 shape failures — 161 pages, 12 sections, deepest 4 of 4 segments, widest 12 of 20 top-level entries
0 redirect failures — 77 entries, 7858 bytes, all printable ASCII
0 stale pins of 18 examined across 5 page(s).
No watchlisted symbols found (5 entries, 161 pages checked, 1 silenced).
predicted 161, published 161, 161 agree
0 mismatches across 59 tables and 519 rows.
```

### Task 6.1 — the walk, criteria with no instrument first

| # | Instrument | Output | Verdict |
|---|---|---|---|
| AC1 | none — read | seven P0s, seven shipped, none struck | **met** |
| AC2 | none — read | nineteen rows walked; **four repairs** | **met, after repair** |
| AC3 | `grep -cE` on §3 | 17 rows, highest row 17, +§3.1 = **18** | **met, 17 fully / 1 partly** |
| AC4 | none — read | nine commands read entire; two explicit *no instrument* markings | **met** |
| AC5 | `grep -rlF`; ``grep -c '^!`'`` | eight named, **eight run** | **met; its second instrument is wrong** |
| AC6 | none — walked; a rebuilt checker | **2 forbidden**, repaired → 0 | **met, after repair** |
| AC7 | `git diff --name-only` | **11**; `SUMMARY.md` empty | **met** |
| AC8 | the seven commands | seven green; **two no longer at §11.6's numbers** | **met, with both movements named** |
| AC9 | `symbolcheck` + seven red-proofs | tasks 2.7–2.10, output pasted | **met** |
| AC10 | none — read | eighteen entries, 18–35, contiguous; **four added** | **met** |
| AC11 | `grep -rn` + walked reverse | six of nine commands cite `tools/README.md`; no orphan section | **met** |
| AC12 | none — read | both numbers recorded and cited; the unrepaired reds named | **met** |

**AC1 — every P0 ships or is struck.** Seven P0s, and the disposition is that none was struck: D5,
D2 and D6 in phase 4; D9's eleven pages and D8 with its two CI jobs in phases 2 and 3; D12 in phase
1; D11 is this section. `grep -n 'symbolcheck' .github/workflows/docs.yml` puts the gate at line 78
of the `check` job and `--verify-list` at line 161 of the scheduled `versions` job, which is the
only mechanical part of this criterion. **The strikes are all in P2** — P2-2 and P2-3, struck in
phase 5 with reasons — and AC1 does not reach them.

**AC3 — eighteen defects, re-derived not quoted.** Two methods that agree:
`grep -cE '^\| *[0-9]+ \|'` over §3 of `requirements.md` gives **17 rows**, and the highest row
number in the same range is **17**. §3.1's eighteenth is prose, counted by hand and by being the
only one. **Seventeen are repaired and one is repaired in part**: defect 8 — *nothing asks whether
the API a page names exists* — got its prose repairs in D3, D5 and D6 and its instrument in D8,
and D8 knows only the five names on its watchlist. That remainder is the residual gap task 6.6
names, and it is the line to quote to anyone who reads AC3 as saying 014 closed defect 8.

**AC4 — every added obligation names its instrument or is marked as having none.** A census, not a
sample: all nine command files were read end to end, which is **777** lines — `cat … | wc -l`,
`wc -l … | tail -1` and `awk 'END{print NR}'` all agree, and an earlier figure of 774 in this
section's own drafting was a count taken before the last repair landed. The repair shows up in two
places where a command says out loud that nothing checks something — `/spec:implement`'s
**"What you decide, because nothing here checks it"** heading over five questions, and
`/spec:update-task`'s *"where the Output cannot be checked mechanically, the tick says so"*. Every
other obligation names a command: `grep -c`, `wc -l`, `git -C ../Brighter grep -n`,
`urlmap.py --redirects`, `test -f`, the eight gates, or `tools/README.md` for a number.

**AC5 — eight gates named, and `/spec:review` runs eight.** All eight resolve in
`.claude/commands/spec/`: `symbolcheck.py` on three commands, `linkcheck.py`, `pagelint.py` and
`optioncheck` on two each, the three `urlmap` modes and `versioncheck.py` on `review.md`.
``grep -c '^!`' review.md`` is **8**, and those eight lines are the eight gates — the current-spec
block sits mid-line and is not counted. **The criterion is met and the instrument written for it is
not the instrument that measures it**: §12 says *"`grep -c python3` on `review.md`'s frontmatter"*,
which returns **5**, because the frontmatter holds one grant per *binary path* and `urlmap.py`
carries three of the eight gates. See friction 39.

**AC6 — prose and `allowed-tools` agree, in both directions.** This is the criterion that failed,
and § *The finding* below is its account. After repair: **0 forbidden, 7 unreached grants, 32
permitted invocations across 9 commands.**

**AC8 — the seven existing gates green.** All seven are green. **Two are not at the numbers §11.6
records**, and neither is adopted silently: `linkcheck` reads **165** where §11.6 says 164, because
`tools/README.md` entered its walk the day it was written (phase 4, predicted as a *finding* and
reconciled); `pagelint` reads **757 warnings** where §11.6 says 768, because eleven C# blocks earned
their `using` lines during phase 3's repairs — settled by diffing the two runs rather than by
arithmetic, and no warning of any other class moved. §11.6's list is the **pre-014** state and
should be read as a baseline, not as a target. **D8's disposition**, which AC8 deliberately left
open: decided by D12's numbers to be a watchlist rather than a census, built, and green at
*0 findings — 5 entries, 161 pages, 1 silenced*, which is a different claim from *0 findings*.

**AC9 — the red-proofs.** Seven, not five, recorded with their output in tasks 2.7–2.10 and
re-read here rather than re-run: the gate fires on all five watchlist symbols and stops when they
are repaired; deleting an entry proves the list is firing and not the corpus; `--verify-list` exits
non-zero on a corrupt ref and still reports `CommandProcessor` as live; and the `product` column
and the opt-out each got the two-way control the tasks review added them for.

**AC12 — the probe ran and its numbers are cited.** D12 covered all 161 pages — 144 of them
carrying a C# fence by a loose regex, 145 by `pagelint`'s `FENCE_RE`, and 161 for the prose surface
— and recorded both numbers with the command beside each. `design.md` §4 cites them to invert D8
from a census to a watchlist, and §5 cites them to size D9. **The reds it found that 014 does not
repair are named**: the 881 unresolved candidates are dominated by the documentation's own invented
domain — `OrderId` on 31 pages, `GreetingEvent` on 18, `CustomerId` on 17 — which no cheap filter
separates from a real API name, and that finding is *why* the gate is a curated list. What goes
un-gated goes to the residual gap in 6.6, not to a later spec's inbox.

### Task 6.2 — AC2 against the nineteen rows, and the four repairs it produced

The ledger is **nineteen rows**, re-derived: the table in `CLAUDE.md` § *The ledger* has 21 pipe
lines, of which two are the header and its separator. Walked row by row against the nine commands,
asking of each: which command would cause a writer to break this, and does that command cite the
rule, restate it, or contradict it?

| Rows | The command that would cause a writer to break it | Verdict |
|---|---|---|
| 1–2 — one H1, and no second | `/spec:implement`'s structure section | cites, by presupposing it: *"first non-blank line after the H1"* |
| 3–4 — the banner, and its grammar | `/spec:implement`, `/spec:design`, `/spec:review` | cites § *Page banner*; **one deliberate restatement**, below |
| 5–6 — heading qualification | `/spec:implement`, `/spec:design` | cites, with one example and **no copy of the allowlist** |
| 7 — a language tag on every fence | none | left to rule 4; `/spec:implement` runs `pagelint` on the paths it touched |
| 8 — "Dispatcher", not "ServiceActivator" | none | left to rule 5; neither spelling appears in any command |
| 9 — `using` directives | `/spec:implement` | cites **rule 6 by number**, and guards `// ...` against misuse |
| 10–16 — the opening sentence and `description:` | `/spec:implement`, `/spec:design` | cites §§ *The opening sentence* and *Page descriptions*; neither copies the 200-character limit nor the clauses |
| 17 — version markers | `/spec:implement` | **contradicted — repaired** |
| 18–19 — compile, and run with a control | `/spec:implement` states both; **`/spec:review` checked neither** | **contradicted — repaired** |

**Repair 1 — a command asserting a fact the ledger had already changed.**
`/spec:implement` line 87 read *"Are the ❌/✅ version markers right? That is **the one** convention
with no rule"*. `CLAUDE.md` line 552 reads *"**Three** conventions have no rule"*, because **phase 5
added the other two rows** — and phase 5 did not re-read the command phase 4 had shipped. The
command was also arguing with itself: it states the compile obligation and the run obligation three
paragraphs earlier and then calls version markers the only one. Repaired to name all three and cite
the ledger.

**Repair 2 — a delegation with no receiver.** `CLAUDE.md` says of those same three rows: *"All
three are checked in review"*. `grep -niE 'compile|control|version marker|❌|behaviour'` over
`/spec:review` returned **one line**, and it is the tasks-phase question about red-proofs — a
different subject. **The review command had no question for any of the three conventions
`CLAUDE.md` delegates to it.** So the ledger's review-only column pointed at a review that was not
looking. Repaired with a **Writing Review** checklist carrying all three, and — because the
checklist would otherwise be unreachable — a **Writing** row in the phase-detection table, the one
phase with no approval marker, detected by tasks approved and boxes still unticked.

**Repair 3 — a rule stated without its retroactivity.** `CLAUDE.md` is explicit that the
`## Step N:` convention *"does not reach every How-to page retroactively"*, because requalifying one
moves every published anchor on it. `/spec:implement` guarded **one** direction — *"do not tidy a
`## Step N:` page into sections"* — and left the other open while telling the writer that a How-to
uses step headings. A writer editing one of the pre-ruling How-to pages would have followed the
command straight into the thing the convention forbids. Repaired to forbid the conversion in both
directions and to name the date the rule binds from.

**Repair 4 — the third stale count in a rationale, found by re-deriving one the walk did not need
to.** The same sentence says *"the 53 pages already typed How-to"*. Measured today:

```bash
grep -rlE '^> \*\*How-to\*\*' contents/ | wc -l      # 57
grep -rhcE '^> \*\*How-to\*\*' contents/ | awk '{s+=$1} END {print s}'   # 57
```

**57, not 53** — and the sentence is not wrong, it is unanchored. 53 was true on 2026-09-06 when
the ruling was taken, and the sentence means that historical set; four How-to pages have been
written under the rule since. This is precisely the class phase 5 repaired twice — and phase 5
found its two by grepping for *"26 pages"*, the number it already knew about, so a third sentence
with a different number went past. `CLAUDE.md` now carries the date, today's 57, and the six of
those 57 that use step headings, with the note that **a rising count is the rule working**.

**What the four repairs cost, against constraint 4.** `/spec:review` 144 → 159 and
`/spec:implement` 89 → 93; `status.md` and `switch.md` lost a pipeline each and no lines. Fifteen of
the nineteen added lines are the *Writing Review* checklist and the phase row that makes it
reachable, which is the only one of the four that adds a section rather than correcting a sentence.
The nine commands stand at **777** against a pre-014 **451**.

**The one deliberate restatement, recorded rather than repaired.** `/spec:implement` says *"The
separator is ` · `, not a hyphen"*, which copies one token out of `CLAUDE.md`'s banner grammar
against constraint 1. It stays: the value is unguessable and a hyphen looks right, so a writer who
has not opened `CLAUDE.md` gets it wrong by default, and one character cannot drift the way a
grammar can. **Nothing else is restated** — `grep -rn` over the commands for the five allowlisted
headings, `APPLIES_TO`, the 200-character limit and the version strings returns nothing but
`new.md`'s `## Next Steps`, which is a heading in the spec-README template and not a page under
`contents/`. The four page-type names appear in `/spec:design` and `/spec:requirements`, which is
unavoidable: choosing one is the decision those commands exist to force.

### Task 6.3 — AC7 backwards, and the whole-repository diff

Forwards first, at the baseline this spec named:

```bash
git diff --name-only c0d410a -- contents/ | wc -l     # 11
git diff --stat c0d410a -- SUMMARY.md                 # empty
```

The eleven are D9's eleven, no more and no fewer. Backwards — **what changed that should not
have** — is `git diff --stat c0d410a -- .`, read line by line against the deliverable table:

| Changed | Why it is allowed |
|---|---|
| **all nine** commands | D1–D7 are seven of them; `status.md` took a permission repair in phase 5 and `switch.md` one in this phase, so the *"only one untouched"* row in the handover is now spent |
| `.github/workflows/docs.yml` +52 | D8's two CI jobs, tasks 3.5 and 3.6 |
| `CLAUDE.md` | D10, under constraint 2 — see below |
| eleven `contents/` pages | D9 |
| `tools/README.md`, `symbolcheck.py`, `symbolwatch.tsv` | D10, D8 |
| `spec/.current-spec`, `spec/014-*` | the spec's own |

**Nothing else moved.** No other spec directory appears in the diff, `.gitbook.yaml` is untouched,
and no file was renamed, so no published URL moved.

**The one entry that needed reading rather than matching is `CLAUDE.md`**, because §8 puts
*"rewriting `CLAUDE.md`'s conventions"* out of scope. Its diff is **five edits across six hunks**
and **not one of them changes a rule's verdict**: three repair stale counts inside rationale
sentences — two in phase 5, the third in this phase — one records that `symbolcheck` now depends on
rule 2, and one gives the compile and run obligations a home. The ledger **gained** two rows and
lost none. Under constraint 2's test — *`CLAUDE.md` says what a page must be; a command says what a
phase must do* — all five are page-level.

**The `contents/` diff is 100 changed lines and every one is a repair or its consequence.**
`git diff c0d410a -- contents/ | grep '^+'` over the additions is **44 `using` lines and 8
`// ...` markers**, which is the compile obligation applied to the blocks the repairs touched — the
rest is the symbol names themselves, plus one signature corrected from
`CancellationToken? ct = null` to `CancellationToken cancellationToken = default` and one `return`
statement added to a `Task<T>` method that had none. That last one is the finding phase 3 recorded
as the reverse of what 014 was built to catch: **the prose was right and the code was wrong.**

### The finding: the check written in phase 5 to find forbidden invocations could not see them

Phase 5's account of obligation 4 ends *"the re-run reports 0 unpermitted across 9 commands"*, and
its throwaway was kept to disk per friction 28. Re-run today over the nine commands **as phase 5
left them**, it still reports **0 unpermitted**. A checker written this phase, from the obligation rather than from the
previous instrument, reports **two forbidden invocations**:

```text
.claude/commands/spec/status.md: FORBIDDEN  sort
.claude/commands/spec/switch.md: FORBIDDEN  sort
```

Both are ``!`ls -d spec/*/ 2>/dev/null | sort` ``, both in a `!` context block that runs on **every**
invocation of the command, and both predate 014 — `switch.md` is the command 014 never touched.

**Why phase 5's run missed them is a single line of its own source.** It filters pipeline segments
through `LOOKS_LIKE_CMD`, a closed alternation of **twenty-four** binary names —
`cat|ls|grep|wc|xargs|echo|mkdir|date|touch|test|sed|awk|head|tail|find|git|python3|dotnet|rm|cp|mv|chmod|curl|gh`.
A segment whose first word is not on that list is **not recognised as a command at all**, so it is
never tested against the grants. **`sort` is not one of the twenty-four**, and it is in the corpus.

**The instrument answers "which of these twenty-four binaries does this line run?" where the
permission model asks "which binaries will this shell line execute?"** That is friction 33's
sentence exactly, written *in phase 5*, about *this instrument*, one phase before the instrument
proved it again from the inside. Phase 4's checker encoded the wrong question in its *prose
matching*; phase 5's encoded it in its *binary list*. The question was repaired and the
enumeration under it was not.

**The four-case control, run before either number was believed** — and case B is the one that
discriminates the two instruments rather than the two corpora:

| Case | Planted | Phase 6's checker | Phase 5's checker |
|---|---|---|---|
| A | nothing — the file as shipped | silent | silent |
| **B** | `jq -r .id` — unpermitted, **off** the twenty-four | **FORBIDDEN** | **silent** |
| C | `sed -n 1p` — unpermitted, **on** the twenty-four | FORBIDDEN | UNPERMITTED |
| D | `grep -v x` — permitted by `Bash(grep:*)` | silent | silent |

A and D are the known-absent half; B and C the known-present. **Without B this is two instruments
agreeing**, which is what phase 5 had.

**The repair is a deletion, not a grant**, following phase 5's own precedent with `/spec:new`'s
`ls | grep | wc | xargs` line: the shell is simplified rather than the permission widened, because
a grant for a binary nothing needs is a defect in the reverse direction by construction. `| sort` is
a no-op after `ls -d spec/*/` — both the glob and `ls` sort — proved rather than assumed:

```bash
diff <(ls -d spec/*/ 2>/dev/null) <(ls -d spec/*/ 2>/dev/null | sort)   # identical
```

Per friction 28, the throwaway owes its exact command:

```bash
python3 <scratch>/permcheck.py     # 0 forbidden, 7 unreached grants, 32 permitted, 9 commands
```

**Its boundary is phase 5's, and it held.** The reverse direction over-reports by design: all seven
unreached grants back prose that describes an action without naming a binary — `Bash(test:spec/*)`
behind *"a `.tasks-approved` file in the spec directory"* on three commands, `Bash(grep:*)` and
`Bash(test:*)` behind `/spec:status`'s *"Count `- [x]` vs `- [ ]`"*. **One is weaker than the other
six and is recorded rather than removed**: `/spec:tasks` holds `Bash(wc:*)` and its prose names only
`grep -c`. It is reachable through obligation 1's *"two methods that agree"*, where `wc -l` is the
obvious second, and removing a grant on that reasoning would be a guess in the other direction.

**This is the sixth plausible zero**, and the handover predicted a sixth would arrive. The five
before it: empty token sets in phase 1; the space-blind fence regex; `git grep -w` in phase 2;
phase 4's planted `## Configuration` colliding with nothing; and phase 5's permission checker asking
the wrong question. **The sixth is the fifth one's repair, still asking a wrong question.**

### Task 6.4 — the defect ledger

**Three kinds of defect, and the *found by* column is why they are three tables.** What 014 found
in the documentation, what it inherited to repair in the workflow, and what it found in itself.

**The corpus — eight defects on eleven pages, none of which any gate had ever reported:**

| Site | What it was | Found by |
|---|---|---|
| `BuildingAnAsyncPipeline.md:36,38` | `IAmACommandStoreAsync`, dead, inside a C# block — a paste gets `CS0246` | 013's manual sweep; **held** for 014 as the gate's first red |
| `DispatcherConfigurationReference.md:255` | `UseExternalInbox`, dead, in prose on a **Reference** page | 013's manual sweep |
| `HowBrighterWorks.md:94` | `IAmAnIbox`, a typo for `IAmAnInbox` | 013's manual sweep |
| 7 pages, 11 sites | `IMessageScheduler`, dead; the product's name is `IAmAMessageScheduler` | **the probe (D12)** — the instrument's win, and it tripled the repair phase |
| `InMemoryOptions.md:241`, `InMemoryScheduler.md:180` | `IMessageSchedulerFactory`, a **second** dead symbol hiding inside the first one's substring count | **phase 1's write-up, re-deriving a count it had just been given** — the re-derivation's win, not the instrument's |
| `TickerQScheduler.md:208` | `RescheduleAsync`, 0 files at both refs; the name is `ReSchedulerAsync` | **reading the block around a repair.** It sat two lines below a site the gate *did* report, and no watchlist knew the name |
| `BuildingAnAsyncPipeline.md` | `AddAsync` called with V8's arity | reading around a repair |
| `BuildingAnAsyncPipeline.md` | a `Task<T>` method with **no return statement**, under prose telling the reader to write `return await base.HandleAsync(...)` | reading around a repair. **The prose was right and the code was wrong** — the reverse of what 014 was built to catch, and nothing in this repository looks for it |

**The two rows that matter most are the two the *found by* column separates.** `IMessageScheduler`
is what an instrument buys: seven pages nobody suspected, found in one run. `IMessageSchedulerFactory`
is what re-deriving an inherited number buys, and no instrument would ever have found it — it was
**inside** the first row's count, wearing its name as a prefix. One is the case for building tools;
the other is the case for standing obligation 1, and 014 has exactly one example of each.

**The workflow — the eighteen, all repaired or repaired in part:**

| # | Repaired by | Shipped in |
|---:|---|---|
| 1, 2, 3 | D5, D6 | phase 4 |
| 4, 14, 15 | D4 | phase 5 |
| 5 | D2, D4, D6 | phases 4, 5 |
| 6 | D2, D1 | phases 4, 5 |
| 7, 16, 17 | D2 | phase 4 |
| **8** | D3, D5, D6 **and D8** | phases 2–5 — **in part; see the residual gap** |
| 9 | D1, D2, D3, D4 | phases 4, 5 |
| 10, 11, 12, 13 | D3 | phase 5 |
| 18 (§3.1) | D7 | phase 5, as P2-1 |

**What 014 found in its own instruments and documents:**

| Defect | Found by |
|---|---|
| Six token sets silently empty — a broken pathspec reading as *"nothing is unresolved"* | phase 1's guard, written **before** it was needed |
| A fence regex blind to ` ```csharp ` with leading spaces | phase 1's controls |
| The watchlist's replacement column right as a **name** and wrong as a **type** at **9 of 17** sites — repairs that name a live type, satisfy the gate and do not compile | phase 3, reading the code around each repair |
| `/spec:requirements` permitted to `touch` its own approval marker | phase 4, running obligation 4 **backwards** for the first time |
| Eleven forbidden invocations in seven commands | phase 5, re-running obligation 4 forwards and parsing the shell |
| `resolve()`'s docstring giving a **false mechanism** for a correct decision, and `word_pattern()`'s false in the mirror image | phase 5, writing `/spec:design` and finding the trap would not reproduce |
| Two stale present-tense counts in `CLAUDE.md` rationale | phase 4's planted heading colliding with nothing |
| **Two forbidden `sort` invocations, live since before 014** | **phase 6**, rebuilding the checker from the obligation |
| **`/spec:implement` asserting "the one convention with no rule" after phase 5 made it three** | **phase 6**, task 6.2's walk |
| **`/spec:review` checking none of the three conventions `CLAUDE.md` delegates to review** | **phase 6**, task 6.2's walk |
| **A third unanchored count in `CLAUDE.md` — 53 How-to pages, 57 today** | **phase 6**, re-deriving a number the walk did not strictly need |

### Task 6.5 — the friction ledger, collected

**Eighteen entries came into this phase, 18–35, contiguous and without duplicates; twenty-two leave
it.** Re-derived across the three documents that hold them rather than re-listed here — the command
returns **22** today and returned **18** before this phase's four were written:

```bash
{ awk '/^## 14\. Workflow friction/,0' requirements.md
  awk '/^## 13\. Workflow friction/,0' design.md
  awk '/^## 4\. Workflow friction/,0'  tasks.md
} | grep -oE '^[0-9]+\.' | tr -d '.' | sort -n | uniq | wc -l      # 22 — and 18..39 with no gap
```

| Where | Entries | What they are about |
|---|---|---|
| `requirements.md` §14 | 18–21 | the requirements template has no shape for a process spec |
| `design.md` §13 | 22–24 | the design command has no slot for an experiment |
| `tasks.md` §4 | 25–35 | the task command's defaults, and four phases of instrument failure |

**Phase 6 adds four, 36–39, in §4 below.** The ledger is 014's product as much as the commands are
— this spec had eighteen defects to work from because 013 wrote its friction down. **Phase 6's own
findings divide, and the division is the interesting part**: the `sort` defect is friction 33
recurring *inside its own repair*, so the ledger had already named it and naming it was not enough;
the other three sit on ground no entry covered, which is why they become 37, 38 and 39 rather than
a second citation of something old.

### Task 6.6 — the close

The README's status checklist is ticked and its *Next Steps* replaced with what actually happened.
**`spec/.current-spec` is deliberately left pointing at `014-documentation_workflow`**, per the
task's own note: there is no spec 015, and repointing it at a directory that does not exist breaks
`/spec:status` and `/spec:switch` for the next session.

**The residual gap, in one sentence, and it is the line the next spec starts from:**

> **A dead API written into prose on an existing page — uncompiled, and not on `symbolwatch.tsv` —
> is caught by nothing.**

Everything 014 built narrows that and none of it closes it. `symbolcheck` checks five curated names
and says so in its own output. `--census` reports the open world and **cannot be a gate**, which is
D12's finding and the reason D8 has the shape it does. `/spec:implement`'s compile obligation covers
only blocks a phase *touches*, and `pagelint`'s **757 warnings across 116 pages** are a fair proxy
for how much of the corpus no phase has yet touched. **The three defects phase 3 found by reading
around a repair are the proof**: each sat inside, or two lines from, a block the gate had already
flagged, and the gate reported none of the three.

**What that makes the honest claim about defect 8**: 014 built the instrument, repaired everything
the instrument can see, and demonstrated the size of what it cannot. `--census` is where a later
spec starts, and the question it has to answer is the one D12 answered *no* to for a gate and left
open for a report: **which slice of 881 candidates across 128 pages is worth a human triage pass?**

---

## 3. Acceptance criteria — where each is met

**All twelve met, walked 2026-09-16.** Five repairs were taken during the walk; the *Met by* column
names where the evidence is, and § *Phase 6 as executed* carries it.

| # | Criterion | Met by | At the walk |
|---|---|---|---|
| AC1 | Every P0 ships or is struck | tasks 6.1, and 5.7 for the P2 strikes | seven of seven ship, none struck |
| AC2 | No command contradicts `CLAUDE.md` | **task 6.2**, walked | **four repairs** |
| AC3 | Eighteen defects repaired or struck, count re-derived | task 6.4 | 17 fully, **defect 8 in part** |
| AC4 | Every added obligation names its instrument | task 6.1 | clean; two explicit *no instrument* markings |
| AC5 | Eight gates named; `/spec:review` runs eight | tasks 4.3, 4.4 | met — **its own instrument measures something else**, friction 39 |
| AC6 | Prose and `allowed-tools` agree | task 4.4, re-run at **6.1** | **2 forbidden found and repaired** |
| AC7 | `SUMMARY.md` unchanged; `contents/` exactly 11 pages | **task 6.3** | 11, and the backwards diff is clean |
| AC8 | Seven gates green at their numbers | tasks **2.12**, 3.7 | green; **two no longer at §11.6's figures**, both named |
| AC9 | The red-proofs — **seven, not five**, after the tasks review | tasks 2.7, 2.8, 2.9, **2.10** | re-read, not re-run |
| AC10 | 014's friction recorded | task 6.5 | eighteen collected, **four added** |
| AC11 | Only what a command cites moved out of `PROMPT.md` | task 5.1, walked at **6.1** | six of nine commands cite it; no orphan section |
| AC12 | The probe ran and its numbers are cited | tasks 1.1, **2.11** | both cited; the un-gated reds named |

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

30. **A gate's scope is not the site's scope, and nothing in the workflow asks which.** *Met in
    phase 4.* "No page under `contents/` changes" was read as "no gate moves", and `linkcheck`
    moved 164 → 165 because its corpus is the **repository**. The claim that failed was about the
    *published tree* and the claim being made was about *a tool's file walk*, and they are not the
    same claim on six of the eight gates. `tools/README.md` now states each gate's corpus, which is
    the cheapest place for the answer to live. **Obligation 6 worked exactly as designed** — the
    prediction was written down before the work, so the +1 was a confirmation rather than a
    discovery.

31. **Obligation 4 has a direction, and the defect was in the other one.** *Met in phase 4; see
    finding C.* *"Prose and permission ship together"* reads naturally as prose → permission, and
    every previous application of it has gone that way. Read permission → prose, the first audit
    ever run found `/spec:requirements` permitted to `touch` its own approval marker. **P2-2's
    `commandlint` must check both directions**, and the design's one-line description of it —
    *"every tool a command's prose names is permitted by its own `allowed-tools`"* — specifies only
    the half that found nothing.

32. **A rationale sentence that quotes a measurement in the present tense outlives the
    measurement.** *Met in phase 4; see finding B.* `CLAUDE.md` says `## Configuration` *"appears
    on 26 pages"*; it appears on none, because the rule that sentence justifies was then applied to
    all 26. Nothing is wrong with the argument and nothing checks the number, so it reads as
    current state to the next person — who planted a heading expecting a collision and got a clean
    run. **A measurement inside a rationale needs the ref it was taken at, exactly as a gate
    number does**, and `tools/README.md`'s rule — *a number without a ref is not a fact* — should
    extend to `CLAUDE.md`. *Repaired in phase 5, task 5.5: both sentences now carry the
    measurement and the date.*

33. **An obligation is satisfied by whatever instrument someone builds for it, and nobody checks
    that the instrument asks the obligation's question.** *Met in phase 5.* Obligation 4's forward
    direction was reported green in phase 4 by a checker that asked *"which tools does this
    command's prose name?"*. The permission model asks *"which binaries will this shell line
    execute?"* — and the gap between the two questions was **eleven forbidden invocations in seven
    commands**, every one inside a `!` block that runs on every invocation. **A green from a
    single-use instrument is a claim about the instrument, not about the corpus**, and the
    workflow's only defence against that is friction 28's rule that the throwaway prints its exact
    command — which is how this one was re-run and disagreed with.

34. **A number recorded without its corpus cannot be re-derived, and reads as reproducible.**
    *Met in phase 5.* *"`git grep -lwF '.Handle('` returns 0 where the same search without `-w`
    returns 23"* names no repository. It is 0/179 in `../Brighter` and 4/22 here, and the pair as
    recorded reproduces nowhere. Obligation 1 says to put the command beside the figure; this is
    the next turn of the same screw — **the command is not enough, because the same command
    answers differently in two working directories.**

35. **A correct conclusion protects the wrong reasoning underneath it.** *Met in phase 5.*
    `resolve()` applies `-w` conditionally, which is right, and gave a mechanism for it that is
    false; `word_pattern()` does the mirror-image thing and gave a mechanism false in the mirror
    image. Both survived a design review, a tasks review and two phase reviews, because every
    review that reached them agreed with the *decision*. **Nothing in the workflow asks a review
    to check the reason separately from the ruling** — and a false reason is what the next person
    reasons from when the case is not identical.

36. **An instrument built from an enumeration is bounded by that enumeration, and nothing asks what
    it cannot see.** *Met in phase 6.* Phase 5's permission checker recognised a pipeline segment as
    a command only if its first word was one of **twenty-four** hard-coded binary names. `sort` is
    not one of them, so two forbidden invocations were invisible to it — and it
    reports **0 unpermitted** on today's files, beside a checker reporting two. This is friction 33
    recurring **one phase after it was written, inside the repair for it**: 33 says an instrument
    may not ask its obligation's question, and the repair fixed the *question* while leaving an
    enumeration underneath that re-imposed the old answer. **The control that catches it is one line
    of design: a probe over a closed list owes a two-way control whose positive case is deliberately
    outside the list.** Case B of phase 6's four-case control is that line, and without it the two
    instruments merely agree.

37. **`CLAUDE.md` and the commands depend on each other in both directions, and only one direction
    has a step.** *Met in phase 6.* Every command is told to cite `CLAUDE.md` and never restate it —
    and nothing tells a phase that **edits** `CLAUDE.md` to re-read the commands that cite it. Phase
    5 added two rows to the ledger; `/spec:implement` went on saying *"the one convention with no
    rule"*, shipped one phase earlier, true when written and false the moment the ledger moved.
    Session 60's rule states the mechanism from the other side: **the thing you just changed is the
    thing you stop testing.** The cheap repair is a step in `/spec:implement`'s and `/spec:review`'s
    quality checks — *if this phase changed `CLAUDE.md`, grep the commands for the claim you
    changed* — and 014 does not ship it, because it was found at the acceptance walk of the spec
    that would have had to write it.

    > **DISCHARGED 2026-09-16, after 014 closed.** The step is in `/spec:implement` § *Quality
    > check* and in `/spec:review`'s *Writing Review* checklist. **Two-way control, which a prose
    > step can still have**: run at `373e9c3` — the commit that shipped the ledger change — it
    > fires on `implement.md:87`, the live false assertion; run today it returns nothing. **And it
    > found one thing while being written**: the first draft of the new step quoted the offending
    > phrase verbatim as its own example, so the step matched its own documentation. That is
    > friction 30's shape one level in — *the instrument's own text enters the instrument's
    > corpus* — and it is why the shipped wording describes the claim instead of quoting it, and
    > says to read every hit.

38. **A convention that delegates its enforcement to "review" needs a named receiver, and
    `review only` was not one.** *Met in phase 6.* Three rows of the ledger read **review only**, and
    `CLAUDE.md` says in as many words that *"all three are checked in review"*. `/spec:review` — the
    review — had a question for none of them, and had not had one at any point in 014. The words
    look like a routing decision and are a **description of who is not doing it**; a row marked
    *review only* should name the checklist it lands in, exactly as an acceptance criterion must
    name its instrument. This is defect 6 one level up: **the ledger's own no-instrument rows were
    unmarked in the sense AC4 means.**

39. **Nothing checks that a criterion's named instrument measures the criterion.** *Met in phase 6.*
    AC5 says *"all eight gates are named, and `/spec:review` runs eight"* and names as its instrument
    `grep -c python3` **on `review.md`'s frontmatter**, which returns **5** — one grant per binary
    path, with `urlmap.py` carrying three of the eight gates. The criterion is met and its stated
    instrument has never been able to show it; ``grep -c '^!`' review.md`` is the one that returns 8.
    This continues friction 24 one step further: 24 asks a criterion to say **where** its evidence
    will come from, and this asks whether the thing named actually **measures what the sentence
    claims**. The test is cheap and belongs in `/spec:review`'s requirements checklist — **run each
    criterion's instrument at the review, before any of it is built, and check the number against
    what the criterion says.** An instrument that cannot be run yet is a criterion with no
    instrument, which is the thing AC4 exists to make visible.

    > **DISCHARGED 2026-09-16, after 014 closed.** The question is in `/spec:review`'s
    > *Requirements Review* checklist, one line below the *name its instrument* question it
    > extends, and it carries AC5 as its worked example: a criterion that is **met** and an
    > instrument that has **never been able to show it**. Naming the failure that way is
    > deliberate — the case to catch is not a wrong criterion, it is a right one wearing an
    > instrument that measures something else.
