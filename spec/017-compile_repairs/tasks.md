# Spec 017: Compile Repairs — Tasks

**Created:** 2026-09-26
**Status:** **APPROVED 2026-09-26** — `.tasks-approved`. Reviewed 2026-09-26, five findings, repaired
**Requirements:** approved 2026-09-26 · **Design:** approved 2026-09-26

**Six phases, 42 tasks, one pull request per phase.** Phases merge under `tools/README.md`
§ *One phase is one pull request*. Phase 1 changes no page and needs no sign-off; phases 2–5 change
the published site and each needs the maintainer's, with the head-ref deletion asked for by name.

---

## 1. Standing obligations — stated once, binding every task

They are not repeated per task. A task that seems to need one restated is a task that has
forgotten this section.

**The programme's seven:**

1. **Re-derive any count before quoting it** — command beside the figure, **two methods that
   agree**. A figure inherited from `requirements.md`, `design.md` or an earlier phase is stale;
   § *Measured at tasks* below is the proof that it matters here
2. **Record the mismatch before fixing it**, once, as a fact — *said A; measured B* — in this file's
   ledgers, then rewrite the text. Do not narrate how it was found, and do not leave the original
   standing beside the correction
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof
   with its output recorded here, and **every control is two-way** — a known-present case and a
   known-absent one
4. **Prose and permission ship together**, read in both directions
5. **Cite `CLAUDE.md`, `tools/README.md` and `design.md`; never restate them**
6. **Predict gate movement before the work, including "none"** — with the mechanism — then
   reconcile, and explain any difference rather than adopting the new number
7. **Ask before merging anything that changes the published site**, and ask for the head-ref
   deletion **by name** in the same breath

**Inherited from 016** (`spec/016-compile_gate/tasks.md` § 1, obligations 8–12): the unit of
compilation is one block; a corpus is enumerated through `pagelint.Page`; a gate number changes in
`tools/README.md` and nowhere else; no verdict is read through a pipe or satisfiable by a missing
path; run the committed form, from a clean directory.

**017 adds five, from its own subject:**

13. **A repair follows `design.md` § *Page Repair Rules* and § *Scaffold Stub Rules*.** Every
    FAILED block on a tranche page leaves in one of its four states. A skip reason outside the
    three accepted ones is put to the maintainer in the PR, not written and hoped for
14. **A DEFECT is verified against Brighter `10.7.0` / Darker `4.1.1` source before it is called
    one, and its recurrence grep is run across `contents/`** — the grep and its count before and
    after go in § *Defect ledger*. A wrapper artefact is not a defect
15. **`probe/attr_mismatch.py` runs before any baseline row is written, and its count only falls**
    (design rule 8). It reads **7** at `c7329bb`
16. **A split fence is listed in § *Splits*, with its old and new ordinals**, because AC2's key is
    (page, ordinal) and a split renumbers every later block on its page
17. **A tutorial repair that changes code, not only `using`s, stops and asks** (design rule 9;
    sample writes are per-PR, `CLAUDE.md` § *Key Constraints*)

---

## 2. Measured at tasks — 2026-09-26, `c7329bb`

The design's tranche table was measured against the **E2 scratch pin**, which is not committed.
Re-run today against the **committed** pin:

```bash
dotnet build tools/blockcheck/refs/refs.csproj -c Release
dotnet build tools/blockcheck/blockcheck.csproj -c Release
bash spec/017-compile_repairs/probe/run.sh $TMPDIR/probe017
python3 spec/017-compile_repairs/probe/pages.py $TMPDIR/probe017 $TMPDIR/probe017/r.tsv
python3 spec/017-compile_repairs/probe/stubs.py $TMPDIR/probe017
python3 spec/017-compile_repairs/probe/tranches.py $TMPDIR/probe017
```

| Figure | Design (E2 pin) | Measured (committed pin) | Agrees? |
|---|---:|---:|---|
| E0 classes: page-type / same-page / values / import / other | 113 / 7 / 50 / 368 / 334 | 113 / 7 / 50 / 368 / 334 | **yes** — the design's "current pin" figures |
| E0 import blocks BUILT by a `using` alone | 46 | 46 | **yes** |
| E1 BUILT / STUB / PARSE / DEFECT | 64 / 431 / 175 / 202 (current-pin column) | 64 / 431 / 175 / 202 | **yes** |
| E3 BUILT / MEMBERS / HIDDEN / SAME-PAGE | 95 / 183 / 120 / 43 | 93 / 184 / 114 / 40 | differs — the E2 pin has 10 more STUB blocks |
| Tranche 1 (0 hard) pages / tranche 2 (≤ 1 hard) pages | **35 / 75** | **34 / 68** | differs |
| `attr_mismatch.py` hits, exit | 7, 1 | 7, 1 | **yes** |
| `pagelint` warnings | 743 across 115 pages | 743 across 115 pages | **yes** |

**The rows that agree are a reproduction, not two methods**: the design's figures came from the
same probe. The second method for the total is `--report`: the five classes sum to **872**, its
FAILED count. The tranche lists get their second method at task 1.10, from `--classify`.

**The tranche membership differs with the pin**, and not only at the edges: **`RequestValidation.md`
— design tranche 1's largest page, 14 FAILED blocks — is absent at the committed pin**, because
its blocks wait on the three Validation packages phase 1 adds. So the page lists for phases 2–5 are
**fixed by task 1.10, against the pin phase 1 commits**, and not quoted here as final. The split
*by `SUMMARY.md` section* is what this file commits to; which pages fall in each section is 1.10's
output.

---

## 3. The phases

| Phase | Goal | Tasks | Published site | PR |
|---:|---|---:|---|---|
| **1** | **Instruments and pin** — `--types`, `--classify`, `--list-skips`, `--explain`, the unit rule, `PageContext.cs` cut, the pin grown, the tranches fixed | 11 | untouched | one, no sign-off |
| **2** | **Tranche 1a** — 0-hard-block pages in *Get Started*, *Commands, Handlers and Pipelines*, *Brighter Configuration*, *Using an External Bus*, *Health Checks and Observability* | 6 | **CHANGED — sign-off** | one |
| **3** | **Tranche 1b** — 0-hard-block pages in *Transports*, *Outbox and Inbox*, *Scheduler*, *Darker* | 6 | **CHANGED — sign-off** | one |
| **4** | **Tranche 2a** — 1-hard-block pages in *Outbox and Inbox* | 5 | **CHANGED — sign-off** | one |
| **5** | **Tranche 2b** — 1-hard-block pages in every other section, plus P0-7's recorded falsehoods | 6 | **CHANGED — sign-off** | one |
| **6** | **Acceptance** — the walk, the backwards check, both ledgers, the close | 8 | untouched | one, no sign-off |

**The order is not Research → Core → Supporting → Polish, and does not pretend to be.** The design
names two deliverable kinds — instruments, and repaired pages — and the instruments decide the
pages: `--classify` and the grown pin are what fix the tranche lists (§ 2), and the unit rule must
be enforced before a tranche writes the first stub it would check. So phase 1 carries every
instrument and no page; phases 2–5 are one tranche-half each, split by `SUMMARY.md` section so that
a reviewer reads related pages together (design § *Phases*). **Tranche 2 is split by where its pages
are, not in half**: at the committed pin 22 of its 38 pages are in *Outbox and Inbox*, and they
share one scaffold world.

**No phase depends on an unmerged one.** Phase 1's unit rule must be on `master` before phase 2
writes a stub; each tranche's baseline rows are on `master` before the next tranche's `--report`
runs against them.

---

## Phase 1 — Instruments and pin *(11 tasks, one PR, no page touched)*

**Goal:** every instrument the tranches need is committed and red-proofed, the pin is grown, and
the tranche page lists are fixed against it. **BUILT stays 101** (design § *Gate Movement
Predicted*).

**Order.** 1.1 first. Then three independent strands: **1.2 → 1.3**; **1.4** and **1.5**;
**1.6 → 1.7**. **1.8** after 1.3, because 1.3's Q2 reading is taken again once xunit is pinned.
**1.9** after 1.3–1.8. **1.10** after 1.3 and 1.8. **1.11** last.

- [x] **Task 1.1:** Re-derive the starting state and write phase 1's prediction
  - Input: `tools/README.md` rows 1–9; `design.md` § *Gate Movement Predicted*, Phase 1 column
  - Output: § *Phase 1 as executed* in this file, opening with all nine gates' exit codes and
    figures at `master`, and a prediction row per gate. **§ *The AC2 before-report*** below holds
    the command that regenerates `before.tsv` from `c7329bb` in a worktree, and its summary line
    (989 / 101 / 872 / 16), so that every later AC2 diff can rebuild it and check it is the same
    report
  - Notes: the prediction for gate 9 is *BUILT unchanged at 101*; the one worth watching.
    `before.tsv` is **regenerated at each use, never kept**: a file under `$TMPDIR` does not survive
    between sessions (§ 2's work directory did not).

- [x] **Task 1.2:** Add the `--types` mode to `tools/blockcheck/Program.cs`
  - Input: `spec/017-compile_repairs/probe/typedump/Program.cs`; `design.md` § *The Classify Mode*
  - Output: `dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll --types <refs.txt>` prints
    `type|ext<TAB>Name<TAB>Ns` rows; **two methods agree**: `diff` against the probe's `typedump`
    output over the same `refs.txt` is empty, recorded with both line counts. **Control, both
    ways:** `UseResiliencePipelineAsyncAttribute` and the `AddBrighter` extension are present, and
    `ITimerProvider` is absent
  - Notes: it skips `refs.txt`'s `#` stamp line, as the probe had to.

- [x] **Task 1.3:** Add `--classify [file]` to `tools/blockcheck.py`, with its red-proof
  - Input: `design.md` § *The Classify Mode* (rules, output, exit codes); `probe/classify.py`;
    requirements Q2
  - Output: the mode; recorded here — two runs `diff` empty; exit 0; its per-class counts beside
    `probe/run.sh`'s at the same ref, **each disagreement explained** (the probe has no `parse`
    class, so *other* must split). **Classification controls, both ways:** `AWSSQSConfiguration.md`
    block 1 with its `using` lines deleted → *import*, and unmodified → absent (it is BUILT); a
    block from design E5's seeded sample → *page-type*. **Exit controls:** `refs.txt` moved aside →
    exit **2**; `--classify contents/SpannerOutbox.md`, whose C# blocks all build → exit **2**
  - Notes: Q2 asked for `Program`, `Assert`, `Xunit` to have their own class; the design's rules do
    not give them one. Once 1.8 pins xunit, `Assert` and `Xunit` become *import*. Record where
    `Program` lands after 1.8, and whether that is right, rather than adding a class the design did
    not. The exit-2 control uses a page with C# blocks, none failing: a page with no C# block at
    all would exit 2 for a different reason.

- [x] **Task 1.4:** Add `--list-skips` to `tools/blockcheck.py`, with its red-proof
  - Input: `design.md` § *The List Skips Mode*; `scan_skips` in `tools/blockcheck.py`
  - Output: the mode; recorded here — `--list-skips > s; echo $?` → **0**, and `wc -l < s` equals
    `--report`'s SKIPPED (16); **control, both ways:** `--list-skips contents/FAQ.md`, which
    carries skips, → exit **0** and one row per skip; `--list-skips contents/SpannerOutbox.md`,
    which carries none, → exit **2**
  - Notes: it must read `scan_skips`, not re-scan, so the two cannot disagree.

- [x] **Task 1.5:** Expose `--explain` through `tools/blockcheck.py` and correct its summary (P1-2)
  - Input: `Program.cs` `--explain`; requirements § *Instrument quirks recorded by 016*
  - Output: `python3 tools/blockcheck.py --explain <id>...` works; its stderr reads *"N blocks
    explained"*; **control, both ways:** the old binary's line (*"989 blocks, 0 built, 989
    failing"* for 872 ids) and the new one, both recorded
  - Notes: ids go as separate arguments — the zsh `$ids` trap from session 84 does not apply to a
    Python front end, which is part of the reason to have one.

- [x] **Task 1.6:** Enforce the unit rule in `--report`, with four plants
  - Input: `design.md` § *The Unit Rule, Enforced* (rules 1–4, output form, exit 1)
  - Output: `--report` prints `SCAFFOLD RULE: <unit>: <what>` and exits **1** on a violation.
    Recorded here: the four plants — a type no block names, a type a block on its page declares, an
    unnamed member, a `global using` — each → exit **1**; **positive control on the real tree:**
    before 1.7, `PageContext.cs` → **26** violations and the other 13 units → none
  - Notes: depends on nothing; 1.7 depends on it. The 26 is the design's dry-run figure;
    re-derive it by the rule as built, and record any difference.

- [x] **Task 1.7:** Cut `PageContext.cs` to `connectionString` and rewrite `pages.tsv`'s rule comment
  - Input: `tools/blockcheck/scaffold/units/PageContext.cs`; `pages.tsv`'s comment;
    `python3 tools/blockcheck.py --show contents/DapperOutbox.md 1`; requirements Q6, P0-3
  - Output: `PageContext.cs` declares `connectionString` and nothing else; `pages.tsv`'s comment
    states rules 1–4 in place of "no domain type"; `--report` → exit **0** with the unit rule on
  - Notes: same PR as 1.6 — otherwise 1.6 turns `master` red (`tools/README.md` rule 3).

- [x] **Task 1.8:** Grow the pin, measured alone
  - Input: `design.md` § *E2* and § *Phases* (the package list, less Jaeger by D2, less the AWS V4
    family by D3, plus the five Brighter packages); `tools/blockcheck/refs/refs.csproj`
  - Output: `refs.csproj` carries the packages, each at the version the design states (Quartz
    3.18.1, Hangfire 1.8.24, EF Core ≥ 9.0.15); recorded here — assembly count (design: **538**),
    and the AC2 `awk` diff of `--report` before and after → **no line printed** (design: 0 verdicts
    change). **Control:** the same diff over a copy with one row flipped prints exactly that row
  - Notes: a verdict that moves here moved for a reason nobody repaired. Stop and explain it before
    going on.

- [x] **Task 1.9:** Update `tools/README.md` — row 9, § *The other modes*, the AC3 and AC10 commands
  - Input: `tools/README.md` row 9 and § *Reading a number before you trust it*; `design.md`
    § *The List Skips Mode* (last paragraph); the outputs of 1.3–1.8
  - Output: row 9's scope names the grown pin and the unit rule; § *The other modes* documents
    `--classify`, `--list-skips`, `--explain`; the two corrected commands are written in with the
    reason the old form fails. Recorded here, **old beside new**:
    `ls tools/blockcheck/*.csproj | wc -l` → 1 and `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*' | wc -l`
    → 2; `grep -vc '^./spec/'` and `grep -vcE '^(\./)?spec/'` over the same `grep -rn` output, the
    first excluding nothing; `grep -cF '^(\./)?spec/' tools/README.md` → ≥ 1
  - Notes: `--list-skips`'s own instrument is 1.4's.

- [x] **Task 1.10:** Fix the tranche page lists against the committed pin
  - Input: 1.8's pin; `probe/pages.py`, `stubs.py`, `tranches.py` with `REFS` at the new
    `refs.txt`; `--classify`; `SUMMARY.md`'s `##` sections; § 2 above
  - Output: § *The tranches* in this file — one table per phase 2–5, one row per page: section,
    FAILED now, reachable, same-page, hard, BUILT now. Beside it: pages per tranche against the
    design's **35 / 40** and § 2's **34 / 34**, reconciled; the **reachable-page count for the
    ≤ 60 target** re-derived (design: 43, of which 37 must land); and whether any E4 page or
    `InMemoryScheduler.md` / `CQRSWithBrighterAndDarker.md` falls in a tranche (which moves its
    P0-7 repair to that phase, design rule 8). **P1-3's trigger:** the tranche blocks, if any, whose
    diagnostics name `args` (`--explain`). Each is named, and P1-3 is carried by the first phase
    holding one; if there are none, that is recorded and P1-3 is not done
  - Notes: two methods — the probe's join and `--classify`'s hard count (`parse` + `other`) per
    page — must agree on which pages have zero or one hard block. The section split in § 3 stands
    unless a section is empty or holds most of a tranche; either is recorded and the split redrawn
    here, not in a later phase.

- [x] **Task 1.11:** Run the nine gates, reconcile phase 1, open the PR
  - Input: 1.1's prediction; `tools/README.md` rows 1–9
  - Output: § *Phase 1 as executed* closes with each gate's exit code and figure against its
    prediction; the PR open, green, no page under `contents/` in `git diff --name-only master`
  - Notes: no sign-off is owed (no published page); the `--admin` grant covers the merge.

---

### Phase 1 as executed

**Starting state, 2026-09-26, `master` `0e1eae9`** — every gate run bare, exit code read from `$?`
before its output:

| # | Gate | Exit | Read | Phase 1 prediction, and the mechanism |
|---:|---|---:|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | **none** — `tools/README.md` is already walked; phase 1 adds no `.md` |
| 2 | `pagelint` | 0 | 0 errors, 743 warnings, 162 pages | **none** — no page changes |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | **none** — no `SUMMARY.md` change |
| 4 | redirects | 0 | 77 entries, 7858 bytes | **none** — no `SUMMARY.md` change |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | **none** — it reads its five tutorial pages, not `refs.csproj` |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | **none** — its pin is its own |
| 7 | `--verify` | 0 | 161 predicted = 161 published | **none** — no page added or moved |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced | **none** — no page changes |
| 9 | `blockcheck` | 0 | 989: 101 BUILT, 872 FAILED, 16 SKIPPED; 0 findings; 501 reference assemblies; 14 units, 15 pages mapped | **BUILT, FAILED and SKIPPED unchanged**, and the AC2 diff empty — the grown pin alone moves nothing (design E2). **Reference assemblies 501 → 538.** **Units 14, exit 0** — `PageContext.cs` is cut in the same PR that turns the unit rule on. The scope line gains the rule's result |

**Task 1.2 — `--types`.** `dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll --types
tools/blockcheck/refs/bin/Release/net9.0/refs.txt` → exit **0**, stderr *"501 reference
assemblies: 21365 types, 3471 extension methods"* — the design's 21,365 and 3,471.

| Check | Result |
|---|---|
| Two methods: `cmp` against the probe's `typedump` over the same `refs.txt` | **identical**, 24,836 rows each |
| Known present | `type UseResiliencePipelineAsyncAttribute Paramore.Brighter.Policies.Attributes`; `ext AddBrighter Paramore.Brighter.Extensions.DependencyInjection` |
| Known absent | `ITimerProvider` → **0** rows |
| No such `refs.txt` / no argument | exit **2**, *"nothing was listed"* / usage |

**Task 1.3 — `--classify`.** `python3 tools/blockcheck.py --classify > c.tsv 2> c.err; echo $?` →
**0**, 872 terminated rows, 15 s. Stderr: *"872 FAILED blocks classified: 175 parse (66 pages), 495
import (108 pages), 32 other (19 pages), 7 same-page (6 pages), 50 values (18 pages), 113 page-type
(50 pages)"*. A second run is byte-identical (`cmp`). The tool preconditions moved into
`tool_unready()`, shared with `--report`, whose output is byte-identical before and after, exit 0.

**Against `probe/run.sh` at the same ref, block by block** — the same 872 (page, ordinal) keys:

| Probe | `--classify` | Blocks | Why |
|---|---|---:|---|
| page-type / same-page / values / import | the same class | 113 / 7 / 50 / 368 | identical rules |
| other | **parse** | **175** | the design splits out blocks that do not parse. `--parse` finds **179**; the other 4 are SKIPPED (`CloudEventsSupport` #11, #12, `V10MigrationGuide` #4, #19) |
| other | **import** | **127** | the design orders *import* before *other*; the probe ordered *other* first. **126** name a pinned type beside another diagnostic; **1** is caught by an extension method alone (`CS1061`), which the probe's type-only rule could not see |
| other | other | 32 | |

**Controls, both ways:**

| Case | Expected | Result |
|---|---|---|
| `AWSSQSConfiguration.md` #1, unmodified (BUILT) | absent | **0** rows |
| …with its 7 `using` lines deleted (page restored after) | *import* | **import** — `CredentialProfileStoreChain, Environment, IServiceCollection, InvalidOperationException, RegionEndpoint` |
| `ClaimCheck.md` #1 | *page-type* | **page-type** `GreetingEvent`; the page declares no `GreetingEvent` (`grep -cE '(class\|record\|interface\|struct) GreetingEvent'` → 0) |
| `PaginationQueryPatterns.md` #3 | *same-page* | **same-page** `OrderDto`; block 1 declares it |
| `--classify contents/SpannerOutbox.md` — 2 blocks, both BUILT | exit 2 | **2**, *"2 blocks, none FAILED: nothing to classify"*, 0 rows |
| `--classify contents/Glossary.md` — no C# block | exit 2, a different reason | **2**, *"no C# blocks on contents/Glossary.md"* |
| `refs.txt` moved aside | exit 2 | **2**, 0 rows, *"no reference list … nothing was checked"* |

**Q2, at the current pin.** The *page-type* names include `Program` ×8, `Assert` ×3 and `Xunit` ×2 —
not domain types a stub should supply. `Assert` and `Xunit` are unpinned today; they are re-read
after 1.8 pins xunit. **`args` is named by 36 FAILED blocks on 21 pages** (`awk` over the `names`
column; a count of the per-block listing agrees): `statements`-wrapper blocks that use `string[] args`, which `Holder.Run()` does not supply.
P1-3 is therefore live, and 1.10 names the tranche pages it reaches.

**Task 1.4 — `--list-skips`.** Said, by 016's AC7: *"`--list-skips` prints one reason per skip"*;
measured at `c7329bb`: exit **2**, *"unknown mode"*. Now `python3 tools/blockcheck.py --list-skips >
s; echo $?` → **0**, stderr *"16 skipped of 989 blocks, across 6 pages"*, `wc -l < s` → **16**,
equal to `--report`'s SKIPPED. **Two methods:** the (page, ordinal) keys of `s` and of `--report`'s
SKIPPED rows are identical (`cmp`).

| Control | Expected | Result |
|---|---|---|
| `--list-skips contents/FAQ.md` — carries a skip | exit 0, one row | **0**, `contents/FAQ.md 15 V9 form, shown beside its V10 replacement (labelled Old (V9))` |
| `--list-skips contents/SpannerOutbox.md` — 2 blocks, no skip | exit 2 | **2**, 0 rows, *"2 blocks, no opt-out: nothing to list"* |
| `--list-skips contents/Glossary.md` — no C# block | exit 2, a different reason | **2**, *"no C# blocks on contents/Glossary.md: nothing was listed"* |

**Task 1.5 — `--explain` through `blockcheck.py`.** `python3 tools/blockcheck.py --explain <id>...`
takes the ids in `--report`'s fourth column, stages only those blocks, and passes stdout, stderr
and the exit code through.

Three defects in the Roslyn half's `--explain`, each measured on the old binary:

| # | Said | Measured | Now |
|---:|---|---|---|
| 1 | the stderr summary describes the run | *"989 blocks, 0 built, 989 failing"* on a run that explained **872** ids | *"872 blocks explained, 5654 diagnostics"* |
| 2 | exit 2 when nothing was checked | an id matching no block → exit **0**, having explained nothing | exit **2**, *"1 id(s) match no staged block, first: NoSuch_9"* (the Python front end refuses it first, the same way) |
| 3 | a listing diffs clean between runs | three runs over the same 872 ids gave **three different row orders**: Roslyn does not order `GetDiagnostics()` | rows sorted by position, code, message; three runs byte-identical |

| Check | Result |
|---|---|
| All 872 FAILED ids | exit **0**, 5,654 rows; **the same row set as the old binary** (`sort \| cmp`) |
| `ClaimCheck_1` (FAILED) | one row, `CS0246 … 'GreetingEvent'` |
| `AWSSQSConfiguration_1` (BUILT) | exit 0, **0** rows, *"1 blocks explained, 0 diagnostics"* |
| `NoSuch_9` / no argument | exit **2** / usage, exit **2** |
| `--classify` and `--report` after the change | exit 0, **byte-identical** to before |

**Task 1.6 — the unit rule in `--report`.** `unit_rule_violations()` runs on every `--report`, after
the compile, and prints `SCAFFOLD RULE: <unit>: <what>` before the verdict counts; each violation
is a finding, so it exits **1**. The scope line gains *"scaffold rule: N units checked, M
violations"*. Two readings of design § *The Unit Rule, Enforced*, made here:

- **The holder class is exempt from rule 1.** Every unit declares the static class its own
  `// blockcheck: using static X;` line brings into scope, and no block names it. Its members are
  not exempt from rule 3
- **Rule 3 counts fields, properties and methods, not locals.** `--identifiers` called every
  variable declarator a `field`; it now says `local` for one outside a field declaration.
  `--list-scaffold` is byte-identical before and after (99 rows)

**Positive control, the real tree** — `python3 tools/blockcheck.py --report r.tsv 2> err; echo $?`
→ **1**, *"scaffold rule: 14 units checked, 26 violations"*, **all 26 in `PageContext.cs`**: 9 types
(8 classes, 1 interface) and 17 members (14 properties, 3 methods) — the design's dry-run figures.
The one member that passes is `connectionString`. **The other 13 units: 0** — the negative control.
The verdict rows are byte-identical to before.

**Plants, both ways** — each added to `AzureSchedulerContext.cs` alone, `--report` run, the unit
restored (`git diff --quiet`). The real tree exits 1 on `PageContext.cs` until 1.7, so the evidence
is the violation count moving by exactly the planted lines:

| Plant | Exit | Violations | The planted line |
|---|---:|---:|---|
| none | 1 | 26 | — |
| a type no block names (`PlantedStub`) | 1 | **27** | `class PlantedStub is named by no BUILT block on contents/AzureScheduler.md` |
| a type a block on its page declares (`OrderService`, block 9) | 1 | **28** | `class OrderService is declared by contents/AzureScheduler.md block 9, and a stub must not supply it`, and rule 1's line |
| a member no block names (`plantedValue`) | 1 | **27** | `property plantedValue is named by no BUILT block on contents/AzureScheduler.md` |
| `global using System.Text;` | 1 | **27** | `declares a global using, which would supply a namespace to every block it reaches` |

The real scaffold exiting **0** is 1.7's output, in the same PR: this branch is not pushed between
the two, since CI's `blocks` job would fail on 1.6 alone.

**Task 1.7 — `PageContext.cs` cut.** Said, by `pages.tsv`: *"A UNIT SUPPLIES VALUES … AND NOTHING
ELSE — no domain type"*; measured: `PageContext.cs` declared **9 types and 17 members** no BUILT
block on its one page names (1.6). It now declares its holder and `connectionString`, the one
identifier `DapperOutbox.md` #1 uses (`--show contents/DapperOutbox.md 1`, line 14). The file keeps
its name, because `baseline.tsv` records `PageContext.cs` as that block's scaffold. `pages.tsv`'s
comment states rules 1–4 and the holder exemption in place of "no domain type"; rule 1's
"never a type the reader is told to write" is marked as checked by reading (AC6), since `--report`
cannot decide it.

| Check | Result |
|---|---|
| `python3 tools/blockcheck.py --report r.tsv 2> err; echo $?` | **0**; *"scaffold rule: 14 units checked, 0 violations"*; *"989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED"*; *"0 findings, 16 skipped"* |
| AC2 diff against `before.tsv` | **0** lines — no verdict moved |
| `--list-scaffold` | 53 identifiers from 14 units, down from 84; `PageContext.cs` lists `class PageContext` and `property connectionString` only |
| The one row that changed | `DapperOutbox.md` #2, **FAILED before and after**: 12 errors → 16, because it now misses `AddGreeting`, `Person`, `GreetingMade`, `Greeting`, which the old unit supplied to a block that never built. That is the leak the rule exists to stop. Those four are `DapperOutbox.md`'s page-type names for its tranche |

**Task 1.8 — the pin grown, measured alone.** `refs.csproj` gains **24 packages**, each named on a
page — the design's E2 set less Jaeger (D2), plus the five Brighter packages, and not the AWS V4
family (D3). The design names some by family; resolved from Brighter's `src/` at `10.7.0` and the
NuGet `nuspec` of each, all twelve Brighter packages exist at 10.7.0:

| Group | Packages | Version, and why |
|---|---|---|
| Brighter, five | `AsyncAPI.NJsonSchema`, `ServiceActivator.Control`, `ServiceActivator.Control.Api`, `ServiceActivator.Extensions.Diagnostics`, `Testing` | 10.7.0 |
| Brighter EF, four | `MsSql`, `MySql`, `PostgreSql`, `Sqlite` `.EntityFrameworkCore` | 10.7.0. `MongoDb.EntityFramework` also exists and no page names it |
| Brighter Validation, three | `DataAnnotations`, `FluentValidation`, `Specification` | 10.7.0 |
| Hangfire | `Core`, `AspNetCore`, `SqlServer` | **1.8.24** — what `MessageScheduler.Hangfire` 10.7.0 depends on (Brighter's `Directory.Packages.props` at the tag) |
| Hangfire storage, third party | `MemoryStorage` 1.8.1.2, `MySqlStorage` 2.0.3, `PostgreSql` 1.21.1, `Redis.StackExchange` 1.12.0 | latest stable; the design gave none |
| Quartz | `Extensions.DependencyInjection`, `Extensions.Hosting` | **3.18.1** — what `MessageScheduler.Quartz` 10.7.0 depends on |
| EF Core | `Microsoft.EntityFrameworkCore` | **9.0.15**, the net9.0 floor the Brighter EF packages set |
| Tests | `xunit` 2.9.3 (Brighter's own), `Moq` 4.21.0 (latest stable) | |

| Check | Predicted | Result |
|---|---|---|
| Restore and build | clean | exit **0**, no `NU1xxx` |
| Reference assemblies, `grep -vc '^#' refs.txt` | **538** | **538**; the run's own line reads *"538 reference assemblies"* |
| `--report` | exit 0, 101 BUILT | exit **0**, *"989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED"*, 0 findings, 0 scaffold-rule violations |
| AC2 diff against `before.tsv` | **0** lines | **0** |
| Control: the same diff with `DapperOutbox.md` #1 flipped | that row | exactly `BUILT -> FAILED contents/DapperOutbox.md 1` |

**What did move** is below the verdict. 36 FAILED rows changed their error codes, and `--classify`
now reads *"175 parse (66 pages), 500 import (109 pages), 15 other (10 pages), 7 same-page (6 pages),
54 values (19 pages), 121 page-type (54 pages)"*. Block by block against 1.3's run, **19 blocks
changed class, all out of a more expensive class**: other → page-type 10, other → values 4,
other → import 3, page-type → import 2. None moved into *other* or *parse*.

**Q2, re-read.** With xunit pinned, `Assert`, `Xunit` and `Fact` leave *page-type* (e.g.
`InMemoryScheduler.md` #9, `InMemoryOptions.md` #2 now name them as *import*). **`Program` ×8 is
what remains** — a top-level-statements artefact, not a type any stub should supply.

**A gap the design's set leaves.** `UseNpgsql` is called on `HangfireScheduler.md` and
`PostgresOutbox.md`, and lives in `Npgsql.EntityFrameworkCore.PostgreSQL`, which E2 did not
include. Not added here: the pin is the design's measured set. 1.10 records whether either page is
in a tranche; if one is, adding the package is put to the maintainer in that phase's PR.

**Task 1.9 — `tools/README.md`.** Four changes, each read against the tool as built at `23aa74f`:
row 9 and the paragraph above the table name the grown pin (71 → **95** packages, `grep -c
'<PackageReference'` on `master` and the branch; **538** reference assemblies) and the unit rule
(*"14 units checked, 0 violations"*), with the figure unmoved; § *Reading a number before you trust
it* names `blockcheck`'s three scope lines and carries the two corrected commands in a table, old
form beside new, with the reason each old form fails; § *The other modes* lists `--classify`,
`--list-skips`, `--explain` with their output shapes and exit-2 rule; § *What each gate actually
checks* says `--report` enforces the unit rule, in place of *"is stated in `pages.tsv`"* alone.

Said, by the `blockcheck` bullet: the scaffold *"supplies values typed from a pinned package, never
a type the page tells the reader to write"*; measured against `pages.tsv` since 1.7: a stub may
supply a type a page names and never shows (P0-3). Rewritten to cite `pages.tsv` and say what
`--report` enforces. The tool's usage line reads `--classify [page...]`, not the design's
`[file]`; the README documents the tool.

| Check | Old | New |
|---|---:|---:|
| `ls tools/blockcheck/*.csproj \| wc -l` / `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*' \| wc -l` | **1** | **2** |
| `grep -rn '101 BUILT' --include='*.md' --include='*.yml' --include='*.py' . > f` (9 lines, all `spec/…` or `tools/…`, none with `./`), then `grep -vc '^./spec/' f` / `grep -vcE '^(\./)?spec/' f` | **9** — excludes nothing | **1** — `tools/README.md` row 9 |
| `grep -cF '^(\./)?spec/' tools/README.md` | **0** on `master` | **1** |
| The same AC13 count after the edit — the new table must not add a copy of the figure | — | **1** (the table says *"the BUILT figure"*, not the figure) |
| `python3 tools/linkcheck.py` | — | exit **0**, 165 files |

**Task 1.10 — the tranches.** § *The tranches* holds the lists, the reconciliation and the two
methods. Four facts from it bind later phases: **phase 2 carries P1-3** (`args`, 4 of its blocks);
**phase 4 asks for `Npgsql.EntityFrameworkCore.PostgreSQL`** (`PostgresOutbox.md` #3); **no P0-7
page is in a tranche**, so phase 5 keeps them all; and **`--classify` reads `Order` as an import**.

That last one is an instrument defect, found here and not repaired. 1.8's pin added
`StackExchange.Redis`, whose `enum Order` now satisfies *import* for **27 FAILED blocks on 16 pages** (none declares it) that name a
domain `Order` their page never shows. `--classify`'s rule has no way to tell a pinned name
from a page's own type that happens to share it. **Ruled by the maintainer, 2026-09-27:
`--classify` is not repaired.** Every repair phase reads `Order` as a type the page never shows, as
§ *The tranches* does. On those 27 blocks an *import* row naming `Order` is a page-type name, and it
is never answered with `using StackExchange.Redis;`.

**Task 1.11 — phase 1 closed, 2026-09-27, branch at `bfb1009`.** Every gate run bare, exit code
read before its output:

| # | Gate | Exit | Read | Predicted (1.1) | Agrees? |
|---:|---|---:|---|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | none | **yes** |
| 2 | `pagelint` | 0 | 0 errors, 743 warnings, 162 pages | none | **yes** |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | none | **yes** |
| 4 | redirects | 0 | 77 entries, 7858 bytes | none | **yes** |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | none | **yes** |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | none | **yes** |
| 7 | `--verify` | 0 | 161 predicted = 161 published | none | **yes** |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced | none | **yes** |
| 9 | `blockcheck` | 0 | 989: 101 BUILT, 872 FAILED, 16 SKIPPED; 0 findings; **538** reference assemblies; 14 units, **0 violations**; 15 pages mapped | counts unchanged; 501 → 538; 14 units, exit 0 | **yes** |

**AC2, against a `before.tsv` regenerated from `c7329bb` in a worktree** (exit 0, *"501 reference
assemblies"*, *"989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED"*, 989 rows): the `awk` diff against
the branch's report prints **0** lines. **Control:** the same diff over a copy with
`AWSSQSConfiguration.md` #1 flipped prints exactly `BUILT -> FAILED contents/AWSSQSConfiguration.md 1`.

**No page changed:** `git diff --name-only master | grep -c '^contents/'` → **0**. The diff is eight
files, all under `tools/` and `spec/017-compile_repairs/`. Phase 1 moved no gate figure, as
predicted. What it changed is the scope of gate 9 (the pin and the unit rule), and that is
recorded in `tools/README.md` row 9.

## Phase 2 — Tranche 1a *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 0-hard-block page in the first five `SUMMARY.md` sections leaves whole. Page lists:
§ *The tranches*, phase 2 table (1.10). Tasks 2.2–2.5 are independent of each other.

- [x] **Task 2.1:** Predict phase 2's movement
  - Input: § *The tranches*, phase 2 table; `design.md` § *Gate Movement Predicted*
  - Output: § *Phase 2 as executed* opens with a prediction per gate — BUILT +N (the table's
    reachable sum as ceiling), `pagelint` warnings −M, `versioncheck` scope held at **18 across 5**,
    `attr_mismatch` count held or falling

- [x] **Task 2.2:** Repair the *Get Started* tranche pages
  - Input: the phase 2 table's *Get Started* rows; `--classify` on each; `samples/CommandProcessor/HelloWorld`
    for `TutorialFirstCommand.md`
  - Output: each page whole — `--classify <page>` lists only blocks named in § *Blocks that stay
    FAILED*; their BUILT rows in `baseline.tsv`; `versioncheck` exit 0 at 18 pins
  - Notes: at § 2's pin, three tutorials' only FAILED blocks are same-page. Each gets the one
    sentence design § *Page Repair Rules* asks for **only if the page does not already say it**.
    Obligation 17 applies to every code change here.

- [x] **Task 2.3:** Repair the *Commands, Handlers and Pipelines* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; stubs in `tools/blockcheck/scaffold/units/<Page>Context.cs`
    with their `pages.tsv` rows, `--report` exit 0 under the unit rule
  - Notes: `RequestValidation.md` sits in this section in `SUMMARY.md`; if 1.10 places it in
    tranche 1 it is this task's largest page.

- [x] **Task 2.4:** Repair the *Brighter Configuration* and *Health Checks and Observability* tranche pages
  - Input: those sections' rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [x] **Task 2.5:** Repair the *Using an External Bus* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [x] **Task 2.6:** Close phase 2 — checks, figures, PR
  - Input: 2.1's prediction; the PR's diff
  - Output: § *Phase 2 as executed* records — `attr_mismatch.py` before any baseline row (obligation
    15); every behavioural block run with its control (P0-10); `pagelint --changed origin/master`
    exit 0; the AC2 diff against `before.tsv` printing only `FAILED -> BUILT` or accepted skips,
    read against § *Splits*; the nine gates against 2.1; `tools/README.md` rows 2 and 9 moved.
    The PR opens with the maintainer's sign-off asked for, and the head-ref deletion by name

### Phase 2 as executed

**Prediction, 2026-09-27, from `master` `9549ab4`.** The *Now* column is 1.11's run at `fba508c`, all
nine exit 0; `git diff fba508c 9549ab4` is empty, so the merge's tree is the tree 1.11 measured. Each prediction names its
mechanism:

| # | Gate | Now | Predicted after phase 2, and why |
|---:|---|---|---|
| 1 | `linkcheck` | 165 files, 0 broken | **none**. No file is added; a same-page sentence that links an earlier page adds a link, not a file |
| 2 | `pagelint` | 0 errors, 743 warnings | **errors 0; warnings 743 → 723, or 722.** On the 17 pages, rule 6 warns on **23** blocks: **20** are *import* blocks that a repair gives `using`s (−20); `ReturningResultsFromAHandler.md` #1 is *page-type* (`CreateTaskCommand`, `commandProcessor`), −1 only if its stub needs the block to import something; `RequestValidation.md` #11 (BUILT) and #14 (same-page) are not touched. A split that makes a block without `using`s would raise it, and is explained if so |
| 3, 4, 7 | shape, redirects, `--verify` | 161 / 77 / 161 | **none**. No `SUMMARY.md` change |
| 5 | `versioncheck` | 0 stale of **18, across 5 pages** | **none, scope held at 18 across 5.** Phase 2 edits four of its five pages (all of *Get Started* except the overview); a fall means a repair deleted a pinned version |
| 6 | `optioncheck` | 0 mismatches, 59 tables, 519 rows | **none**. One phase 2 page carries a table (`RelationalDatabaseConfigurationReference.md`); its one FAILED block gets `using`s, not rows |
| 8 | `symbolcheck` | 0 findings, 22 entries, 3 silenced | **none**. No repair here names a watchlisted symbol |
| 9 | `blockcheck` | 101 BUILT, 872 FAILED, 16 SKIPPED | **BUILT 101 → at most 138, at least 117; SKIPPED 16 plus accepted reasons only; exit 0; units 14 plus the new ones, 0 violations.** Mechanism below |
| — | `attr_mismatch.py` | **7**, exit 1 | **held at 7.** None of its hits is on a phase 2 page, nor on `InMemoryScheduler.md` #4 |

**Gate 9, by source.** Phase 2's table (§ *The tranches*): **36 reachable** — 10 build with a
`using` alone, 5 with an empty stub, 15 need a stub with members, 6 need a typed value; **18
same-page** stay FAILED by rule. **Plus one block outside the tranche, from P1-3.** Simulated
in a worktree at `9549ab4`, with the `statements` wrapper's `Run()` changed to
`Run(string[] args)`: `--report` → exit **1**, *"989 blocks: 102 BUILT, 871 FAILED, 16 SKIPPED"*,
and the AC2 diff against `master` prints exactly one line, `FAILED -> BUILT
contents/InMemoryScheduler.md 4`. Its only diagnostic was `'args'`, so it builds and is unlisted,
which is a finding. **P1-3 therefore adds that baseline row in the same PR.** It moved **no** block
BUILT → FAILED, so no BUILT block declares a local `args` that the parameter would shadow.

- **Ceiling 138** = 101 + 36 + 1: every stub works
- **Floor 117** = 101 + 10 + 5 + 1: only the `using` and empty-stub blocks, and P1-3's one. The
  21 blocks that need members or typed values are where a stub may surface a defect; each that
  does stays FAILED and is listed in § *Blocks that stay FAILED*
- **Pages with nothing BUILT: 97 → as low as 88.** Nine phase 2 pages have no BUILT block and at
  least one reachable block. **Five** reach one by a `using` or an empty stub alone —
  `ImplementingAHandler.md`, `ImplementingAsyncHandler.md`, `Compression.md`,
  `ErrorHandlingOptions.md`, `HandlingLargeMessages.md`; the other four depend on stubs with
  members or typed values

`InMemoryScheduler.md` #4 is a DI configuration block (`UseScheduler`, `AutoFromAssemblies`), and
it asserts no behaviour a run could falsify. BrighterCommand/Brighter#4414 is about *running*
`AutoFromAssemblies` with a scheduler, not about compiling it. The page's own P0-7 falsehood,
`ITimerProvider`, is in other blocks and stays in phase 5.

**Task 2.2 — *Get Started*.** Four pages; **no page under `contents/` changed**.

- **P1-3 landed here**, as 1.10 assigned it: the `statements` wrapper's method is
  `Run(string[] args)`. `TutorialFirstMessage.md` #2 and #4 no longer report `'args'`, and
  `InMemoryScheduler.md` #4 builds. Nothing moved BUILT → FAILED, as 2.1's simulation said
- **`TutorialStreamingWithKafka.md` #1 and #2 build** with a new unit,
  `TutorialStreamingWithKafkaContext.cs`: `Greetings.GreetingEvent : Event` and the one
  constructor block 1 calls. It is the first unit to supply a type. Rule 1 admits it, because
  the page names rung 2's `GreetingEvent` (*"`Greetings` | unchanged"*) and never tells the reader
  to write it. It has no `Greeting` property, because no block on the page names one (rule 3).
  `--identifiers` → `class GreetingEvent` and a parameter; `--report` → *"15 units checked, 0
  violations"*. **Control, both ways:** with `: Event` and the base call removed, both blocks
  fail on `CS0311` (*"cannot be used as type parameter 'T'"*, `KafkaPublication<T>`,
  `KafkaSubscription<T>`); restored, both have **0** diagnostics
- **The three other tutorials' seven FAILED blocks are all same-page**, and each page already
  says the step builds on an earlier one (§ *Blocks that stay FAILED* quotes the line), so
  **no sentence was added**. `--classify` calls five of them *page-type*, because each also
  names rung 1's or 2's types from another page, and its *same-page* class needs every missing
  name to be declared on this page. The probe's SAME-PAGE is the reading the rules act on
- **`attr_mismatch.py` → 7**, exit 1, run before the baseline rows (obligation 15)
- **`versioncheck`** untouched: none of its five pages changed
- **Baseline:** three rows at `8090477`, the commit that made them build. `--report` → exit **0**,
  *"989 blocks: 104 BUILT, 869 FAILED, 16 SKIPPED"*, baseline 104, 0 findings; the AC2 diff against
  `before.tsv` prints exactly the three `FAILED -> BUILT` lines, and nothing else

**Task 2.3 — *Commands, Handlers and Pipelines*.** Five pages. **BUILT 104 → 113** (+9), each block
listed in the baseline commit; 12 blocks stay FAILED and are listed. `pagelint` **743 → 737**.

- **`using`s:** `ImplementingAHandler.md` #1, #2; `ImplementingAsyncHandler.md` #1, #2;
  `BuildingAnAsyncPipeline.md` #2, #3; `RequestValidation.md` #1, #3, #5, #7, #8, #10, #12, #13, #15
- **Two defects, repaired at every recurrence** (§ *Defect ledger*): the V9 `HandleAsync`
  signature on two pages, and the `Guid Id` that hid `Command.Id` on one. `Command(Guid)` is a
  live 10.7.0 constructor (`Command.cs:77`), so `base(Guid.NewGuid())` is not one and stays.
  **Control, both ways:** `ImplementingAsyncHandler.md`'s two blocks compiled together with
  `IpFyApi` stubbed → **0** diagnostics; with the old signature restored → `CS0115` *"no suitable
  method found to override"* and `CS1503`. `BuildingAnAsyncPipeline.md`'s three blocks together,
  with `GreetingCommand` and `IpFyApi` stubbed → **0**
- **A unit, `RequestValidationContext.cs`:** `GreetingCommand` (`Name`, `Email`) and `PlaceOrder`
  (`Quantity`, `Sku`), which the page validates and never shows, and `builder`,
  `commandProcessor`, `command`. `--report` → *"16 units checked, 0 violations"*. Blocks 2 and 11
  were BUILT before and are now compiled with the unit, so their rows change scaffold
- **Behaviour, run with controls** against released 10.7.0 packages (a scratch console app, the
  page's types verbatim; `GreetingCommand`, `PlaceOrder` and a `PlaceOrderHandler` are the
  harness's):

  | Claim | Case → result | Control → result |
  |---|---|---|
  | Quick Start: an invalid request prints both failures and never reaches the handler | `Name = "", Email = "not-an-email"` → `RequestValidationException`, 2 errors, the page's two lines verbatim; no *"Registered"* | valid → *"Registered Ada <ada@example.com>"*, no exception |
  | Specification: `And` reports both errors | both rules broken → 2 errors | one broken → 1 error; none → handler runs |
  | A missing validator is a `ConfigurationException` | FluentValidation, none registered → `ConfigurationException`; Specification, none → `ConfigurationException` | validator registered → `RequestValidationException`, 2 errors |

  The page's lifetime claim — `Specification<T>` keeps per-evaluation state — holds:
  `Specification.cs:76`, `private IReadOnlyList<ValidationResult> _lastResults`. **Upstream, not
  ours:** Brighter's own missing-specification message suggests `services.AddSingleton<ISpecification<PlaceOrder>>(...)`,
  which that state makes unsafe
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 9 rows added and 2 re-admitted with the unit (`RequestValidation.md` #2, #11), all at
  `0363fae`. `--report` → exit **0**, *"989 blocks: 113 BUILT, 860 FAILED, 16 SKIPPED"*, baseline 113,
  0 findings. The AC2 diff against `before.tsv` prints 12 lines, every one `FAILED -> BUILT`: 2.2's
  three and these nine

**Task 2.4 — *Brighter Configuration* and *Health Checks and Observability*.** Three pages.
**BUILT 113 → 120 → 113**: +7 by repair, then −7 when the maintainer ruled BRT006–008 out of
`AnalyzerSupport.md`. **None stays FAILED**. `pagelint` **737 → 730**.

- **`using`s:** `AnalyzerSupport.md` #3, #4, #5, #6, #8 — `Paramore.Brighter` and
  `Paramore.Brighter.MessagingGateway.Kafka`, the two blocks 1 and 2 already carry
- **Two units.** `RelationalDatabaseConfigurationReferenceContext.cs` supplies
  `DbConnectionString()`, which block 1 calls and the page leaves to the reader's configuration.
  `HealthChecksContext.cs` supplies the `WebApplicationBuilder` block 1 elides as *"Web
  Application Builder code goes here"*. `--report` → *"18 units checked, 0 violations"*
- **A defect found by running, not compiling** (§ *Defect ledger*): `HealthChecks.md` #1 called
  `app.UseEndpoints(...)` on a `WebApplication` with no `app.UseRouting()`, and **throws at
  startup** — `InvalidOperationException`, *"EndpointRoutingMiddleware … must be added to the
  request execution pipeline before EndpointMiddleware"*. It compiled clean with its `using`s;
  only the run found it. Repaired to `app.MapHealthChecks(...)` on the app. The one recurrence,
  `BrighterControlAPI.md` #1 (a phase 5 page), is repaired the same way, to
  `app.MapBrighterControlEndpoints()`, with its `using`; it stays FAILED on `app` alone
  (*values*), for phase 5
- **Behaviour, run with controls** against released 10.7.0 packages: a scratch web app, the
  page's block verbatim after a harness `builder` and a `Dispatcher` with one
  `InMemorySubscription<Ping>` of two performers

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `/health` returns 200 with the status as its body | dispatcher started → `Healthy` [200] | dispatcher not started → `Unhealthy` [503] |
  | `/health/detail` returns the page's JSON shape | `{"status":"Healthy","results":{"Brighter":{"status":"Healthy","description":"2 healthy consumers.",…}},"totalDuration":…}` — the page's keys and casing | not started → `"Unhealthy"`, *"ping has 0 of 2 expected consumers"* |
  | the block runs at all | repaired form → serves both endpoints | the page's old `UseEndpoints` form → throws before serving |

  The *Health Status* table's third row is the control's result; its *Degraded* row is read from
  `BrighterServiceActivatorHealthCheck.cs` at 10.7.0 (`activeConsumers > 0 ? Degraded :
  Unhealthy`), not run
- **Unshipped features removed, on the maintainer's ruling (option a).** `AnalyzerSupport.md`
  documented **BRT006–BRT008**, the Kafka partitioner analyzers and their code fixes. **No release
  carries them**: 10.7.0's `src/Paramore.Brighter.Analyzer/Analyzers/` has three analyzers
  (BRT001–005, `AnalyzerReleases.Shipped.md`), there is **no** code-fix project at 10.7.0, and
  Brighter `master` has none of the files; they exist only on BrighterCommand/Brighter#4255, open.
  Offered (a) remove, (b) mark as coming, (c) hold the PR for #4255; the maintainer ruled **(a)**.
  The page now describes BRT001–005, drops the code-fix claim from its introduction and the
  *Code fix* column, and keeps its suppression guidance, generalised to BRT001. **Run, with
  controls,** against `Paramore.Brighter.Analyzer.Package` 10.7.0:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | BRT001: a `Publication` without `RequestType` warns | `warning BRT001: RequestType assignment is Missing from Publication` | `RequestType = typeof(Ping)` → no warning |
  | BRT003: a `Subscription` without `MessagePumpType` warns | `warning BRT003: MessagePump assignment is Missing from InMemorySubscription` | `messagePumpType: MessagePumpType.Reactor` → no warning |
  | the page's pragma block suppresses BRT001 | the block verbatim → builds, no warning | pragmas removed → BRT001 |
  | `.editorconfig` `severity = none` suppresses | `dotnet_diagnostic.BRT001.severity = none` → no BRT001 | without it → BRT001 |

  BRT002, BRT004 and BRT005 are read from the 10.7.0 analyzers, not run. **Seven blocks left the
  page** (§ *Blocks removed*); its one C# block is the new pragma block, BUILT
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline, in two steps.** The repair (`51b5fef`) added 7 rows → *"989 blocks: 120 BUILT"*.
  The removal (`ec38400`) then took `AnalyzerSupport.md` from 8 C# blocks to 1: the gate reported
  *"baselined block no longer exists (7)"* and exited 1, as it should; rows 2–8 are deleted and
  row 1 re-points at `ec38400`, since it now names a different block. `--report` → exit **0**,
  *"982 blocks: 113 BUILT, 853 FAILED, 16 SKIPPED"*, baseline 113, 0 findings. `before.tsv`
  regenerated from `c7329bb` (989 rows, *"101 BUILT, 872 FAILED, 16 SKIPPED"*); the AC2 diff prints
  **14** lines, every one `FAILED -> BUILT`: 2.2–2.3's twelve, `HealthChecks.md` #1 and
  `RelationalDatabaseConfigurationReference.md` #1. **The diff cannot see a removed block** — it
  reads the after-report's keys — so the seven are listed in § *Blocks removed*, and two of them
  (`AnalyzerSupport.md` #2 and #7) were BUILT at `c7329bb`: against 2.1's prediction, phase 2's
  BUILT count carries **−2** from deleting false content, not from a regression
- `linkcheck` 165 files, 0 broken; `symbolcheck` 0 findings; `versioncheck` 0 stale of 18 across 5;
  `pagelint --changed origin/master` 0 errors

**Task 2.5 — *Using an External Bus*.** Five pages. **BUILT 113 → 129** (+16, the table's 16
reachable), **1 stays FAILED** (same-page). `pagelint` **730 → 706**. Running the blocks found
nine defects; two are upstream bugs, filed on the maintainer's ruling. All are in § *Defect ledger*.

- **`using`s:** `Compression.md` #1, #2; `ErrorHandlingOptions.md` #1–#6; `HandlingLargeMessages.md`
  #1 (`System.Net.Http`), #4 (`Xunit`)
- **Four units and one grown.** `ClaimCheckContext.cs` and `CompressionContext.cs` supply
  `GreetingEvent : Event`; `ErrorHandlingOptionsContext.cs` supplies `PlaceOrder : Command`;
  `HandlingLargeMessagesContext.cs` supplies `services`, `awsCredentials` and `LargeOrderPlaced : Event`.
  `HandlingPoisonMessagesContext.cs` gains `PlaceOrder` (`OrderId`, `Quantity`) and
  `commandProcessor`. No page tells the reader to write any of them (rule 1, by reading).
  `--report` → *"22 units checked, 0 violations"*. `ClaimCheck.md` #3 was BUILT before and is
  re-admitted with the page's new unit
- **The V9 mapper shape, repaired at every unlabelled occurrence.** `Compression.md` #1 declared
  `MapToMessage(GreetingEvent request)`; V10's `IAmAMessageMapper<T>` is `MapToMessage(TRequest,
  Publication)` plus `IRequestContext? Context { get; set; }` (`IAmAMessageMapper.cs:31`, `:33`).
  Signature: 14 lines on 10 pages → 2, and both of those are skipped V9 forms. Found by a second
  method: `NullableReferenceTypes.md`'s `MapToMessage(CreateOrderCommand request, string? topic =
  null)`. `Context`: 17 mapper blocks without it → 1, the skipped V9 form. The touched blocks on
  other tranches' pages fell under `pagelint --changed`'s rule 6 and got their `using`s; `--explain`
  confirms that every namespace resolves. They stay FAILED on their pages' own types, for their
  phases
- **Behaviour, run with controls** against released 10.7.0 packages, in scratch console apps:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `ClaimCheck`: a body at the threshold is checked in | 1024 bytes, 1 KB → `Claim Check {id}`, bag and `DataRef` set | 1023 bytes → inline, no claim |
  | `retain: false` deletes the luggage | unwrap → `HasClaimAsync` false | `retain: true` → still true |
  | unwrap falls back to `DataRef` | bag entry removed → body restored | — |
  | `HandlingLargeMessages.md` #4's three assertions | verbatim → pass | body under threshold → `StartsWithException` |
  | a missing store throws `NotImplementedException` | tracer registered, no store → *"This is a null store…"* | no tracer → the tracer's `InvalidOperationException` masks it; store + tracer → posts |
  | Decompress restores a Compress body | `PostAsync` → `application/gzip; charset=utf-8` on the bus, **not** decompressed (all three methods) | `Post` → `application/gzip`, round-trips; charset stripped → round-trips |
  | Compress honours its threshold | — | body under 150 KB → uncompressed; Decompress on a plain body → unchanged |
  | `ErrorHandlingOptions.md` #4's commented values | verbatim → `orders.dlq`, `orders.invalid`, `dead-letter-orders` | — |
  | `requeueCount: 3` → DLQ | handler runs **3** times, then DLQ | `0` and `1` → 1 run; `2` → 2; `-1` → 80,269 runs in 1.5 s, no DLQ |
  | `unacceptableMessageLimit` stops the pump | limit 3, 6 throwing messages → 3 runs, `DS_STOPPED` | limit 0 → 6 runs, still running |
  | `[RejectMessageOnErrorAsync]` sends escapes to the DLQ | Proactor → DLQ 1 | no attribute → DLQ 0; **the page's Reactor** → `ConfigurationException`, pump stops, handler never runs |

  Read, not run: `DepositPostAsync` taking the `WrapAsync` path, the window's reset rule, and the
  per-transport DLQ keys (10.7.0 consumers; the subagent's reading, spot-checked at
  `HeaderNames.cs`, `MsSqlMessageConsumer.cs:382` and `MessageHeader.cs:162`)
- **Upstream, filed on the maintainer's ruling:** BrighterCommand/Brighter#4432 (Decompress vs
  `WrapAsync`'s charset) and #4433 (`UseExternalLuggageStore` requires a tracer). Each gives a
  minimal repro with its control, run as written. Both pages now state the limitation and link
  the issue
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 16 rows added and `ClaimCheck.md` #3 re-admitted with its unit, all at `71d0c60`.
  `--report` → exit **0**, *"982 blocks: 129 BUILT, 837 FAILED, 16 SKIPPED"*, baseline 129,
  0 findings. `before.tsv` regenerated from `c7329bb` (989 rows, *"101 BUILT, 872 FAILED,
  16 SKIPPED"*); the AC2 diff prints **30** lines, every one `FAILED -> BUILT`. No block was added
  or removed, so no ordinal moved
- `linkcheck` 165 files, 0 broken; `symbolcheck` 0 findings; `versioncheck` 0 stale of 18 across 5;
  `optioncheck` 0 mismatches across 59 tables, 519 rows (the twelve `requeueCount` rows changed
  description only); `pagelint --changed origin/master` 0 errors

**Task 2.6 — phase 2 closed, 2026-09-27, branch at `5407298`.** Every gate run bare, exit code
read before its output:

| # | Gate | Exit | Read | Predicted (2.1) | Agrees? |
|---:|---|---:|---|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | none | **yes** |
| 2 | `pagelint` | 0 | 0 errors, **706** warnings across 107 pages, 162 pages | 0 errors; 743 → 723 or 722 | **no — 17 lower**, explained below |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | none | **yes** |
| 4 | redirects | 0 | 77 entries, 7858 bytes | none | **yes** |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | none, scope held at 18 across 5 | **yes** |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | none | **yes** |
| 7 | `--verify` | 0 | 161 predicted = 161 published | none | **yes** |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced | none | **yes** |
| 9 | `blockcheck` | 0 | 982: **129** BUILT, 837 FAILED, 16 SKIPPED; 0 findings; 538 reference assemblies; baseline 129; **22** units, 0 violations; 23 pages mapped | BUILT 117–138; SKIPPED 16; exit 0; 0 violations | **yes** — within the range, with −2 from the removal below |
| — | `attr_mismatch.py` | 1 | **7** | held at 7 | **yes** |

`pagelint --changed origin/master` → exit **0**, 0 errors.

**Gate 9, reconciled against 2.1's 36 reachable.** 29 of them built; 5 were `AnalyzerSupport.md`
blocks that built and then left the page with the unshipped BRT006–008 (§ *Blocks removed*);
`ReturningResultsFromAHandler.md` #1 stays FAILED under unit rule 1; and `RequestValidation.md` #10
stays FAILED as same-page. **Said, by the probe at `9549ab4`: #10 is HIDDEN** (`PlaceOrder`,
`builder`), so 2.1 counted it reachable. **Measured:** with the unit supplying both, its one
remaining diagnostic is `CS0103` `OrderSpecification`, which block 9 declares. 29 + 5 + 1 + 1 = 36.
Add `InMemoryScheduler.md` #4 from P1-3 and BUILT is 101 + 29 + 1 = **131**, less the two
`AnalyzerSupport.md` blocks that were BUILT at `c7329bb` and were removed: **129**.

**Every FAILED block on the 17 tranche pages is listed**: `after.tsv`'s FAILED keys on those pages
and § *Blocks that stay FAILED*'s rows are the same 20 (`diff` silent). **Pages with nothing BUILT:
97 → 88**, which is 2.1's floor, both counted from the report by an `awk` and a Python join.

**Gate 2, reconciled.** Per-page warnings at `9549ab4` against the branch: the 17 tranche pages fell
by **20**, which is 2.1's prediction (`ReturningResultsFromAHandler.md` #1 was not touched, so 723,
not 722). The further **−17** are on 11 pages outside the tranche — `Routing.md` −3, `KafkaConfiguration.md`,
`NullableReferenceTypes.md`, `ReactorAndProactor.md` and `V10MigrationGuide.md` −2 each, six more
−1 each. They are blocks that 2.4's `UseEndpoints` repair and 2.5's mapper and `requeueCount`
repairs touched, which `--changed` rule 6 then required to carry `using`s. 2.1 predicted only the
tranche because it did not foresee repairs off it. 743 − 20 − 17 = **706**.

**AC2, against a `before.tsv` regenerated from `c7329bb` in a worktree** (exit 0, *"989 blocks: 101
BUILT, 872 FAILED, 16 SKIPPED"*, 989 rows): the diff prints **30** lines, every one `FAILED ->
BUILT`. § *Splits* is empty, so no ordinal moved; the seven removed blocks are in § *Blocks
removed*, because the diff reads only the after-report's keys. **Control, both ways:** a copy of
the branch's report with `HealthChecks.md` #1 set FAILED prints 29 lines and loses that one; a copy
with `AWSSQSConfiguration.md` #1 set FAILED prints exactly one line that is not `FAILED -> BUILT`,
`BUILT -> FAILED contents/AWSSQSConfiguration.md 1`.

**Every behavioural block was run with its control** (P0-10): the tables under 2.3, 2.4 and 2.5.
`InMemoryScheduler.md` #4 asserts no behaviour (2.1).

**The PR changes 39 pages** (`git diff --name-only origin/master..HEAD -- contents | wc -l`): **10**
of the 17 tranche pages, and **29** outside the tranche, changed by repairing recurrences, 2.5's
defects among them. The other seven tranche pages were made whole by a unit or the wrapper alone
(the four *Get Started* pages, `ClaimCheck.md`, `RelationalDatabaseConfigurationReference.md`), or
left as they were (`ReturningResultsFromAHandler.md`). Beside them are 12 files under `tools/` and
this one.

---

## Phase 3 — Tranche 1b *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 0-hard-block page in the remaining four sections leaves whole. Page lists:
§ *The tranches*, phase 3 table. Tasks 3.2–3.5 are independent of each other.

- [x] **Task 3.1:** Predict phase 3's movement
  - Input: § *The tranches*, phase 3 table; § *Phase 2 as executed*
  - Output: § *Phase 3 as executed* opens with a prediction per gate, from `master` after phase 2

- [x] **Task 3.2:** Repair the *Transports* tranche pages
  - Input: that section's rows; `--classify` on each; `RelationalTransportContext.cs`, which already
    serves two pages
  - Output: each page whole; baseline rows; stubs share an existing unit where the pages share a
    world (design § *Scaffold Stub Rules*)

- [x] **Task 3.3:** Repair the *Outbox and Inbox* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [x] **Task 3.4:** Repair the *Scheduler* tranche pages
  - Input: that section's rows; `--classify` on each; the Hangfire and Quartz packages 1.8 pinned
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows
  - Notes: BrighterCommand/Brighter#4414 is upstream's. A scheduler block that asserts a scheduled
    request runs through DI is run, and if it throws for #4414 the block stays FAILED or the claim
    is reworded — never shown working.

- [x] **Task 3.5:** Repair the *Darker* tranche pages
  - Input: that section's rows; `--classify` on each; `../Darker` at `4.1.1`
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [x] **Task 3.6:** Close phase 3 — checks, figures, PR
  - Input: 3.1's prediction; the PR's diff
  - Output: as 2.6, for phase 3; plus **the ≤ 60 target read at tranche 1's close** — pages with
    nothing BUILT, by requirements' `awk`, against the 37 that must land

### Phase 3 as executed

**Prediction, 2026-09-27, from `master` `e256e2b`**, the merge of phase 2 (PR #191). The *Now*
column is this task's own run at `e256e2b`, every gate bare, exit code read before its output; all
nine exit 0 and read at `tools/README.md`'s figures. Each prediction names its mechanism:

| # | Gate | Now | Predicted after phase 3, and why |
|---:|---|---|---|
| 1 | `linkcheck` | 165 files, 0 broken | **none**. No file is added |
| 2 | `pagelint` | 0 errors, 706 warnings, 162 pages | **errors 0; warnings 706 → 664, or 663.** On the 20 pages, rule 6 warns on **43** blocks. **42** are FAILED reachable blocks, and a repair gives each its `using`s whether or not it then builds (−42): 9 build with a `using` alone, 10 with an empty stub, 10 need a stub with members, 13 a typed value. The 43rd, `QueryPipelinePolicies.md` #3, is BUILT and marks its omission `// ...`; −1 only if a repair touches it. The other **25** FAILED blocks on these pages, the three same-page ones among them, already carry `using`s; nine of the 20 pages have no rule 6 warning at all. Repairs of recurrences off the tranche lower it further, as 2.6 found (−17), and are explained if so |
| 3, 4, 7 | shape, redirects, `--verify` | 161 / 77 / 161 | **none**. No `SUMMARY.md` change |
| 5 | `versioncheck` | 0 stale of **18, across 5 pages** | **none, scope held at 18 across 5.** None of its five pages (`tools/versioncheck.py:81`) is in phase 3 |
| 6 | `optioncheck` | 0 mismatches, 59 tables, 519 rows | **none.** Seven phase 3 pages carry tables it reads — the six transport pages with one FAILED block each, and `DynamoDbDistributedLock.md`. Their repairs give blocks `using`s and stubs, not rows; a row changes only if a repair finds a documented default wrong, and is then a defect in § *Defect ledger* |
| 8 | `symbolcheck` | 0 findings, 22 entries, 161 pages, 3 silenced | **none**. No repair here names a watchlisted symbol |
| 9 | `blockcheck` | 982: 129 BUILT, 837 FAILED, 16 SKIPPED; 22 units | **BUILT 129 → at most 193, at least 153; SKIPPED 16 plus accepted reasons only; exit 0; units 22 plus the new ones, 0 violations.** Mechanism below |
| — | `attr_mismatch.py` | **7**, exit 1 | **held at 7.** Its hits are on five pages (`HowServiceActivatorWorks.md`, `PipelineValidation.md` ×3, `PolicyRetryAndCircuitBreaker.md`, `ReactorAndProactor.md`, `V10MigrationGuide.md`), none of them in phase 3 |

**Gate 9, by source.** The probe re-run at `e256e2b` (§ *The tranches*' recipe, `Order` excluded)
reads the phase 3 table unchanged: **67 FAILED, 64 reachable, 3 same-page, 0 hard, 14 BUILT**.
Phase 2 touched seven of these pages (`GcpPubSubConfiguration.md`, `InMemoryTransport.md`,
`MQTTConfiguration.md`, `MSSQLMessageBroker.md`, `RedisConfiguration.md`, `RocketMQConfiguration.md`,
`SchedulingAMessage.md`), one line each, in the `requeueCount` and `requeueDelay` repairs; no verdict
moved. The 64 reachable are **11** with a `using` alone, **13** with an empty stub, **23** needing a
stub with members, **17** needing a typed value. The three same-page blocks are
`PaginationQueryPatterns.md` #2, #3, #4.

- **Ceiling 193** = 129 + 64: every stub works
- **Floor 153** = 129 + 11 + 13: only the `using` and empty-stub blocks. The 40 blocks that need
  members or typed values are where a stub may surface a defect; each that does stays FAILED and is
  listed in § *Blocks that stay FAILED*
- **P1-3 is spent.** 1.10 put seven of the `args` blocks in phase 3; `--explain` over all 837 FAILED
  blocks at `e256e2b` finds **0** `'args'` diagnostics, so no block here is waiting on the wrapper
- **Pages with nothing BUILT: 88 → as low as 73, at least 81.** 15 phase 3 pages have no BUILT block,
  and every one has a reachable block. **Seven** reach one by a `using` or an empty stub alone —
  `InMemoryTransport.md`, `RabbitMQMigrateToQuorumQueues.md`, `BoxProvisioningConfiguration.md`,
  `DistributedLock.md`, `DynamoDbDistributedLock.md`, `QueryObjectValidation.md`,
  `QueryResultTypes.md`; the other eight depend on stubs with members or typed values. 88 counted
  by requirements' `awk` against `--report` at `e256e2b`
- **The ≤ 60 target, read ahead.** Tranche 1 was to land 24 of the 37 pages; phase 2 landed 9, so
  phase 3 must land all 15 for tranche 1 to meet its share. At the ceiling, 73 − 60 = **13** of
  tranche 2's 19 must follow in phases 4 and 5; each page phase 3 misses adds one to that

**Carried into the repair tasks, not the prediction:**

- **`Order`.** `SchedulingAMessage.md` #1 and `TestingQueryHandlers.md` #1 are both *members* here,
  and both name an `Order` their page never shows. By 1.10's ruling each is read as a type the page
  never shows, never with `using StackExchange.Redis;`
- **BrighterCommand/Brighter#4414** binds 3.4: a scheduler block asserting that a scheduled request
  runs through DI is run, and if it throws for #4414 it stays FAILED or the claim is reworded
- **The open `MessageBody` defect** (§ *Defect ledger*) has no recurrence on a phase 3 page:
  `grep -rnE 'new MessageBody\([^)]*"' contents/` → one line, `KafkaConfiguration.md:709`, which is in
  no tranche. It stays with phase 5

**Task 3.2 — *Transports*.** Nine pages. **BUILT 129 → 147** (+18): all **17** reachable blocks,
and `AWSSQSConfiguration.md` #6 from a recurrence repair. **None stays FAILED**. `pagelint`
**706 → 696**. Pages with nothing BUILT **88 → 81**: the seven transport pages that had none.

- **`using`s:** `RabbitMQMigrateToQuorumQueues.md` #1 (`System`, the RMQ `Async` gateway — ten
  `using`s in `contents/` name `Async`, one `Sync`); `InMemoryTransport.md` #1–#4;
  `RedisConfiguration.md` #1, `RocketMQConfiguration.md` #1, `PostgreSQLTransportAndOutbox.md` #5
  (`System`, for `TimeSpan`)
- **Two units and one grown.** `InMemoryTransportContext.cs` supplies `services`, `subscriptions` and
  `GreetingMade : Event`. `TransportConfigurationContext.cs` supplies `GreetingEvent : Event` to six
  pages that share a world — `GcpPubSubConfiguration.md`, `MQTTConfiguration.md`,
  `MSSQLMessageBroker.md`, `RedisConfiguration.md`, `RocketMQConfiguration.md`, each configuring a
  publication and a subscription for the reader's event, and `AWSSQSConfiguration.md` (below).
  `RelationalTransportContext.cs` gains `connection` (step 3's `PostgresMessagingGatewayConnection`),
  `GreetingEvent(string)` and `AddGreeting` (`Greeting`). Both pages take those two from the samples
  they link to, and neither tells the reader to write them (rule 1, by reading). The linked Postgres
  sample, `GreetingsSenderWithOutbox`, is on Brighter `master` and not in 10.7.0's tree; a repository
  link, not a package, so it stands. `--report` → *"24 units checked, 0 violations"*
- **`AWSSQSConfiguration.md` joined the shared unit** because the `RequestType` repair (next bullet)
  made its BUILT blocks #1, #3, #4, #5 name `GreetingEvent`. `--report` read them `BUILT -> FAILED`
  until it did; they are re-admitted with the unit. #6 builds for the first time, once its
  `SqsAttributes` defect was repaired
- **A `Publication` without `RequestType` cannot be posted to.** `InMemoryTransport.md`'s complete
  example, run verbatim against 10.7.0, throws `ConfigurationException: No producer found for request
  type. Have you set the request type on the Publication?` at `Post`.
  `FindPublicationByPublicationTopicOrRequestType.cs:81` finds a publication by a `Destination` on the
  context, then a `[PublicationTopic]` attribute on the request, then `RequestType` — and a
  configuration page sets none of them. It is transport-agnostic, so the in-memory run stands for
  every transport. Brighter's own analyzer says the same (BRT001). Repaired at every publication a
  reader posts through: **20 on 8 pages** (`spec/017-compile_repairs/probe/pubscan.py`, **23 → 2**;
  the other repair is `KafkaConfiguration.md`'s `new KafkaPublication() {publication}`, rewritten
  below). The two left are right as they are: `AnalyzerSupport.md` shows how to suppress BRT001 for
  exactly this, and `HandlingLargeMessages.md` passes a `Publication` to `WrapAsync`, not a registry
- **Five more defects, found by the `using`s the recurrence repair required** (`pagelint --changed`
  rule 6), each verified at 10.7.0 and repaired at every recurrence (§ *Defect ledger*):
  `SqsPublication.SqsAttributes` is `QueueAttributes`; `Publication.CloudEventsType` is `Type`;
  `InMemoryOutbox()` takes a `TimeProvider`; `KafkaConfiguration.md` called `SetConfigHook` on the
  publication, where 10.7.0 has it on `KafkaProducerRegistryFactory`, as the page's own prose says —
  the old form fails `CS1061` against the Kafka package, the new one builds; and
  `InMemoryTransport.md`'s *Limitations* said there is no backpressure and no dead letter queue
- **Behaviour, run with controls** against released 10.7.0 packages, in scratch console apps:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `InMemoryTransport.md` #4, the complete example, delivers a posted event | verbatim → `ConfigurationException`, handler never runs | `RequestType = typeof(GreetingMade)` → handler runs once |
  | *"No dead letter queues: Failed messages are discarded"* | `InMemorySubscription` with `DeadLetterRoutingKey`, handler throws `RejectMessageAction` → **1** message on the DLQ topic | no `DeadLetterRoutingKey` → 0 anywhere, discarded |
  | *"No backpressure: Unlimited queue growth"* | `new InternalBus(1)` → send 1 returns, send 2 **blocks** (still waiting at 2 s) | `new InternalBus()` → three sends return, bus holds 3 |
  | `MSSQLTransportInboxAndOutbox.md` #8: *"Runs once per message id"* | block 8 verbatim, one event published 3 times → handler runs **1** | `onceOnly: false` → **3**. Run on `InMemoryInbox`: `[UseInbox]` is store-independent (`UseInboxHandler.cs:95`, `Warn` returns without calling on) |
  | `RabbitMQMigrateToQuorumQueues.md` #1: `PersistMessages = true` persists | block 1 verbatim against RabbitMQ 3 in Docker → `delivery_mode` **2** | `PersistMessages = false` → `delivery_mode` **1** |

  Read, not run: the limitation bullets left standing — no persistence, one process, no message TTL
  (`InternalBus.cs` has no expiry). The configuration pages' other blocks register a transport and
  assert no behaviour a run without their brokers could falsify
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 18 rows added and `AWSSQSConfiguration.md` #1, #3, #4, #5 re-admitted with
  `TransportConfigurationContext.cs`, all at `7edaada`. `--report` → exit **0**, *"982 blocks: 147
  BUILT, 819 FAILED, 16 SKIPPED"*, baseline 147, 0 findings. Against `master`'s report, 18 blocks
  moved, every one `FAILED -> BUILT`
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; `pagelint --changed origin/master` 0 errors.
  **Pages changed: 13** (`git diff --name-only e256e2b..HEAD -- contents`) — **5** of the nine
  tranche pages (the other four were made whole by a unit alone: `GcpPubSubConfiguration.md`,
  `MQTTConfiguration.md`, `MSSQLMessageBroker.md`, `MSSQLTransportInboxAndOutbox.md`), and **8**
  outside it by recurrence: `AWSSQSConfiguration.md`, `BrighterBasicConfiguration.md`,
  `CommandProcessorConfigurationReference.md`, `InMemoryOptions.md`, `InMemoryOutbox.md`,
  `KafkaConfiguration.md`, `RabbitMQConfiguration.md`, `V10MigrationGuide.md`. Of `pagelint`'s −10,
  4 are on the tranche (`InMemoryTransport.md` 3, `RabbitMQMigrateToQuorumQueues.md` 1) and 6 on the
  recurrence pages

**Task 3.3 — *Outbox and Inbox*.** Four pages. **BUILT 147 → 159** (+12): `BoxProvisioningConfiguration.md`
#1–#9, `DistributedLock.md` #1, `TransactionalMessagingWithTheOutbox.md` #1, #2. **Three stay FAILED**:
the DynamoDB lock blocks, for the V4 pin. `pagelint` **696 → 682**, all −14 on the tranche
(`BoxProvisioningConfiguration.md` 8, the other three 2 each; per page, against a worktree at
`7163fb0`). Pages with nothing BUILT **81 → 78**: the first three of those pages. Two methods, one
figure: requirements' `awk` and `comm -23` of the FAILED and BUILT page sets.

- **Said at 3.1:** `DynamoDbDistributedLock.md` reaches a BUILT block with an empty stub, and
  `DistributedLock.md` #2 with a `using`. **Measured:** both pages tell the reader to install
  `Paramore.Brighter.Locking.DynamoDB.V4`. Every Brighter V4 package at 10.7.0 declares its types in a
  `.V4` namespace, and the V4 family is not in the pin (D3). **Maintainer's ruling, 2026-09-27:** the
  blocks take the V4 `using`s their reader needs, and stay FAILED until 018's V4 pin. Compiled in
  scratch against the released V4 packages (`Paramore.Brighter.Locking.DynamoDB.V4`,
  `Outbox.DynamoDB.V4`, `Outbox.Hosting`, both DI packages, 10.7.0): **0 errors**. The control, the
  same blocks with the V3 namespaces, fails `CS0234`. With V3 `using`s against the pin, all three
  also build, so nothing else is hidden behind the namespace
- **`using`s:** `BoxProvisioningConfiguration.md` #1 (`Microsoft.Extensions.Configuration`, for
  `GetConnectionString`), #2–#4, and each backend's namespace on the per-backend fragments #5–#9.
  `DistributedLock.md` #1 (`System.Threading`, `System.Threading.Tasks`). On
  `TransactionalMessagingWithTheOutbox.md` #1 and #2 the `// ...` gives way to the `using`s the
  sample's handlers carry, less the sample's own namespaces
- **Two units.** `BoxProvisioningConfigurationContext.cs` supplies `builder`, `services`,
  `connectionString`, `outboxConfig`, `inboxConfig`, `opts` and `rdbmsConfiguration`: the composition
  root's values, earlier blocks' locals, and the delegate parameter the fragments sit in.
  `TransactionalMessagingWithTheOutboxContext.cs` supplies `Retry`, `AddGreeting`, `Person`,
  `Greeting`, `GreetingMade`, `Salutation` and `SalutationReceived`, each carrying only the members
  a block names, typed as `samples/WebAPI/WebAPI_Dapper/` types them at 10.7.0. The page named no
  sample; it now cites that one, and tells the reader nowhere to write these types (rule 1, by
  reading). `--report` → *"26 units checked, 0 violations"*
- **The V4 namespaces, repaired where V4 is recommended** (§ *Defect ledger*):
  `AWSSQSMigrateToV10.md` step 3 said *"The namespace structure remains the same in most cases"*
  and showed only AWS SDK namespaces. It now maps all seven V3 → V4 namespaces and names the two that
  do not map by suffix: `Locking.DynamoDb` → `Locking.DynamoDB.V4`, and `DynamoDbTableFactory`,
  which V3 declares in `Outbox.DynamoDB` and V4 in `DynamoDb.V4`. Its step 2 table gains the three
  V4 packages it lacked, each on NuGet at 10.7.0. The rewrite keeps block 1 BUILT (the SDK
  namespaces alone; the duplicate pair went). `DynamoOutbox.md` and `DynamoInbox.md` show V3
  `using`s under a V4 recommendation, and each now names its V4 namespaces in one sentence.
  `AwsScheduler.md`'s *Basic Configuration* block imported `MessageScheduler.AWS.V4` beside the V3
  `MessagingGateway.AWSSQS`, which the V4 scheduler's dependency (`AWSSQS.V4`) does not declare,
  so it now takes `.V4`; the block stays FAILED and its other gaps stay with its phase
- **Behaviour, run with controls** against released 10.7.0 packages in scratch console apps, one
  process per case: Brighter's static `ApplicationLogging` keeps the first container's
  `LoggerFactory`, so a second container in the same process throws `ObjectDisposedException`, which
  is a harness artefact, not a defect:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `TransactionalMessagingWithTheOutbox.md` #1: *"Both entity writes and message writes succeed or fail together"* | block 1 verbatim, SQLite outbox and `SqliteTransactionProvider`, in-memory producer → Greeting **1**, Outbox **1**, dispatched **1**, on the bus **1** | the block with a `throw` between `DepositPostAsync` and `CommitAsync` → Greeting **0**, Outbox **0**, bus **0**; `SendAsync` returns, as the block's `catch` hands on |
  | #2: *"UseInboxAsync … ensures the message is only processed once"*; the duplicate is dropped and `SalutationReceived` not sent again | block 2 verbatim, the same `GreetingMade` published twice → publish 2 throws `OnceOnlyException`; Salutation **1**, Outbox **1**, bus **1** | `onceOnly: false` → both return; Salutation **2**, Outbox **2**, bus **2** |
  | `BoxProvisioningConfiguration.md` #4: the timeout *"is read late … placement inside the delegate does not matter"* | block 4 verbatim (set before the `Add` calls) → both MSSQL runners' `_lockTimeout` **00:02:00**; set after them → **00:02:00** | not set → **00:00:30** |
  | #2: *"the hosted service always provisions every Outbox before any Inbox"*, whatever the registration order | SQLite, Inbox registered before Outbox → *"Provisioning Outbox 'Outbox'"* then *"Provisioning Inbox 'Inbox'"* | two Outboxes registered B then A → B then A: within a phase, registration order holds |

  Read, not run: a duplicate reaching the pump arrives as `OnceOnlyException`, which `Proactor.cs`'s
  catch-all logs, counts as unacceptable and then acknowledges, so the message is dropped as the
  page says. `UseBoxProvisioning` twice throws, and the `connectionName` overloads throw
  `InvalidOperationException` with the page's message (`MsSqlBoxProvisioningExtensions.cs:62`)
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 12 rows at `24e6e53`. `--report` → exit **0**, *"982 blocks: 159 BUILT, 807 FAILED,
  16 SKIPPED"*, baseline 159, 0 findings. The gate is enforced both ways, so 147 BUILT before was
  exactly the 147 baseline rows, and the 12 new rows are the only blocks that moved, every one
  `FAILED -> BUILT`
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; `pagelint --changed origin/master` 0 errors.
  **Pages changed: 8** (`git diff --name-only 7163fb0..HEAD -- contents`): the **4** tranche pages,
  and **4** outside the tranche by recurrence: `AWSSQSMigrateToV10.md`, `AwsScheduler.md`,
  `DynamoInbox.md`, `DynamoOutbox.md`. Their rule 6 counts are unchanged

**Task 3.4 — *Scheduler*.** Two pages. **BUILT 159 → 173** (+14): `SchedulingAMessage.md` #1–#10,
one of them a block this task added, and `TickerQScheduler.md` #1, #4, #7, #8. **Two stay FAILED**,
`TickerQScheduler.md` #2 and #3, for packages not in the pin. `pagelint` **682 → 668**, all −14 on the
tranche (`SchedulingAMessage.md` 9, `TickerQScheduler.md` 5; per page, against a worktree at
`423e215`). Pages with nothing BUILT **78 → 77**: `SchedulingAMessage.md`. Two methods, one figure:
requirements' `awk` and `comm -23` of the FAILED and BUILT page sets.

- **`using`s:** every block on both pages. On `SchedulingAMessage.md` #1–#9 they replace the leading
  `// ...`. `TickerQScheduler.md` #1's `using TickerQ;` resolves but supplies nothing the block uses;
  it gives way to the namespaces the linked sample imports (`TickerQ.DependencyInjection`,
  `TickerQ.Utilities.Entities`, `.Interfaces`, `.Interfaces.Managers`)
- **Fields the classes used and never declared:** `_repository` on `SchedulingAMessage.md` #1, #2, #4,
  and `_paymentGateway` and `_logger` on #6. Each is now a `private readonly` field beside
  `_commandProcessor`, typed `IOrderRepository`, `IUserRepository`, `IPaymentGateway` and
  `ILogger<ProcessPaymentHandlerAsync>`. A unit cannot supply a value whose type no pinned package
  declares, and the reader sees what the service depends on
- **One unit and one grown.** `SchedulingAMessageContext.cs` supplies `services` and the domain the
  page never shows: `Order` (1.10's ruling), `OrderStatus`, `User`, the two repositories, six
  requests, `NotificationRequest`, `IPaymentGateway`, the two payment exceptions and
  `ProcessOrderHandlerAsync`, each carrying only the members a block names. `TickerQSchedulerContext.cs`
  gains `builder`, `app` and `SendReminderCommand`. Neither page tells the reader to write any of
  these (rule 1, by reading). `--report` → *"27 units checked, 0 violations"*
- **Two defects on the page** (§ *Defect ledger*): #3 returned `schedulerId` from `async Task`
  (`CS1997`), now `Task<string>`; #5 scheduled `command with { AttemptNumber = … }`, which needs a
  record, and a Brighter `Command` is a class a record cannot derive from. It now sets
  `AttemptNumber` and schedules the command
- **BrighterCommand/Brighter#4414, run.** It is closed by #4419, which is on `master` and in no
  release. At 10.7.0, `AutoFromAssemblies()` and `AsyncHandlersFromAssemblies` register
  `FireSchedulerRequestHandler` and `FireSchedulerMessageHandler` twice. `HandlersFromAssemblies`
  registers each once, because the scheduler's handlers are async. **Maintainer's ruling,
  2026-09-27: state it, with the workaround.** `SchedulingAMessage.md` gains *Registering Handlers
  When You Schedule Requests* before its configuration examples. It states the failure, links
  #4414, and shows explicit `AsyncHandlers` registration as a new block 7, so the page's old #7–#9
  are now #8–#10 (§ *Splits*). Its recurrences on other pages are recorded, not repaired (§ *Defect
  ledger*)
- **BrighterCommand/Brighter#4437, filed this task.** The InMemory scheduler cannot cancel or
  reschedule a request scheduled through the command processor. `CommandProcessor` calls
  `_schedulerFactory.CreateAsync(this)` for each scheduled call, and `InMemoryScheduler` keeps its
  timers in an instance field. It is the same on `master`. **Maintainer's ruling, 2026-09-27: file
  it and state it.** The ruling covers every page that makes the claim: `SchedulingAMessage.md`'s
  cancellation note, and `FAQ.md`'s *Can I cancel or reschedule*. On `InMemoryScheduler.md`, the
  cancel example and the `Should_Cancel_Scheduled_Command` test now schedule through
  `IAmARequestSchedulerAsync`. The issue's repro was run verbatim, and the reschedule result was
  added as a comment
- **The `TickerQScheduler.md` blocks against the released packages.** A reader on net10.0 gets TickerQ
  10.4.0, not the pin's 9.0.2 (the Brighter package's net10.0 dependency group). Blocks 1, 2, 3 and 8
  were compiled in scratch against `Paramore.Brighter.MessageScheduler.TickerQ` 10.7.0 with
  `TickerQ`, `TickerQ.EntityFrameworkCore` and `TickerQ.Dashboard` at 9.0.2 on net9.0, and at 10.4.0
  on net10.0: **0 errors, all eight builds**. **Phase 4 asks** for `TickerQ.EntityFrameworkCore` and
  `TickerQ.Dashboard` 9.0.2 alongside `Npgsql.EntityFrameworkCore.PostgreSQL`; until then #2 and #3
  stay FAILED
- **Behaviour, run with controls** against released 10.7.0 packages in scratch console apps, net10.0,
  one process per case:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `SchedulingAMessage.md` #8–#10 with #1–#3: a request scheduled after `AutoFromAssemblies()` runs | `UseScheduler(new InMemorySchedulerFactory())`, `AutoFromAssemblies()`, `SendAsync(1 s, …)` → `ArgumentException` *"More than one handler was found … FireSchedulerRequest"*, unhandled on the timer thread; **the process ends** | explicit `AsyncHandlers` → handler runs **1** |
  | #7, the workaround: explicit registration makes scheduled requests run | block 7 verbatim, `SendAsync(TimeSpan)` and `SendAsync(DateTimeOffset)`, 1 s each → handler runs **2** | the same with `AutoFromAssemblies()` → the process ends |
  | A narrower scan avoids it | `AutoFromAssemblies([own])`, `AsyncHandlersFromAssemblies([own])` → the process ends | registry read: `HandlersFromAssemblies([own])` → **1** of each handler; `AutoFromAssemblies()` → **2** of each |
  | #4, *"Every scheduler supports cancellation"*, for InMemory | `SendAsync(1 s)`, then `CancelAsync(id)` on the registered `IAmAMessageSchedulerAsync` or `IAmARequestSchedulerAsync` → handler runs **1** | schedule and cancel on one `IAmARequestSchedulerAsync` → **0** |
  | Rescheduling, for InMemory (`FAQ.md`) | `ReSchedulerAsync(id, 10 s)` for an id from `SendAsync(1 s)` → returns **`False`**; the request runs at 1 s | scheduled through the same instance → returns **`True`**; not run 3 s later |

  Read, not run: #6's bare `DeferMessageAction` requeues after the subscription's delay
  (`Proactor.cs:522`, `Channel.RequeueAsync(message, delay ?? RequeueDelay)`). The configuration
  blocks for Hangfire, Quartz and TickerQ register a scheduler, and a scheduled request through
  either external scheduler reaches the same `processor.SendAsync(FireSchedulerRequest)`
  (`BrighterHangfireSchedulerJob.cs:32`)
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 14 rows at `3509d3e`. `--report` → exit **0**, *"983 blocks: 173 BUILT, 794 FAILED,
  16 SKIPPED"*, baseline 173, 0 findings. Joined on page and ordinal against the report at
  `423e215`, every block that moved went `FAILED -> BUILT`. The one new key is
  `SchedulingAMessage.md` #10, the added block's renumbering
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; `pagelint --changed origin/master` 0 errors.
  **Pages changed: 4** (`git diff --name-only 423e215..HEAD -- contents`): the **2** tranche pages,
  and **2** outside it for #4437, `FAQ.md` and `InMemoryScheduler.md`. Their rule 6 counts are
  unchanged, and `--explain` on the two touched `InMemoryScheduler.md` blocks (#8, #9) finds only
  types the page never shows

**Task 3.5 — *Darker*.** Five pages. **BUILT 173 → 189** (+16): `QueryObjectValidation.md` #1–#3,
`QueryPipelinePolicies.md` #2, #4, #5, #6, `QueryResultTypes.md` #1–#6 and `TestingQueryHandlers.md`
#1–#3. **Four stay FAILED**: `PaginationQueryPatterns.md` #2, #3, #4 (same-page) and
`QueryPipelinePolicies.md` #1 (`Program`). `pagelint` **668 → 658**, all −10 on the tranche
(`QueryResultTypes.md` 6, `QueryPipelinePolicies.md` 3, `QueryObjectValidation.md` 1; per page,
against a worktree at `842b757`). Pages with nothing BUILT **77 → 74**: `QueryObjectValidation.md`,
`QueryResultTypes.md`, `TestingQueryHandlers.md`. Two methods, one figure: requirements' `awk` and
`comm -23` of the FAILED and BUILT page sets.

- **`using`s:** every FAILED block on the four pages that change. On `QueryResultTypes.md` #1–#6,
  `QueryObjectValidation.md` #3 and `QueryPipelinePolicies.md` #4–#6 they replace the leading
  `// ...`. `QueryPipelinePolicies.md` #1 and #2 gain `Microsoft.AspNetCore.Builder` for
  `WebApplication`. `QueryPipelinePolicies.md` #3 stays BUILT and keeps its `// ...`
- **Three units.** `QueryResultTypesContext.cs` supplies five empty stubs, `Customer`,
  `OrderSummary`, `DataRow`, `Product` and `Order`: the page is about result types and names no
  member of any. `QueryObjectValidationContext.cs` supplies `PagedResult<T>`, `Order`, `Product`,
  `User`, `IUserRepository` and `UserNotFoundException`. `TestingQueryHandlersContext.cs` supplies
  the domain, the `ApplicationDbContext`, the two queries and handlers under test, and the xunit
  fixture types. Each carries only the members a block names. `Order` is read as 1.10 rules, and
  `DataRow` as a type the page never shows, not `System.Data.DataRow`. None of these is a type the
  page tells the reader to write (rule 1, by reading). `--report` → *"30 units checked, 0
  violations"*
- **`QueryPipelinePolicies.md` #1 stays FAILED on `Program`.** It is a `Program.cs`, but it has no
  declaration after its statements, so it takes the `statements` wrapper and `typeof(Program)` finds
  nothing. Q2's re-read (1.8) ruled `Program` a top-level artefact no stub should supply. Built in
  scratch as a `Program.cs` against the released Darker 4.1.1 packages, net10.0 Web SDK, implicit
  usings off: **0** errors. Control, without `using Microsoft.AspNetCore.Builder;`: `CS0103`
  `WebApplication`
- **Two defects on the tranche, both found by running** (§ *Defect ledger*). `QueryPipelinePolicies.md`
  called the default retry *"exponential backoff"* and said the breaker *"opens after consecutive
  failures"*. It now gives the values, and says a policy applies only to a handler that carries
  `[RetryableQuery]`. `QueryObjectValidation.md` said *"The ASP.NET model binder will validate these
  attributes before the query reaches your handler."* It now names the two cases where ASP.NET Core
  rejects an invalid query, and what happens everywhere else
- **One defect off the tranche, recorded rather than repaired.** Five Darker pages treat
  `[RetryableQuery]`'s second argument as a circuit-breaker name that adds a breaker to the retry.
  At Darker 4.1.1 it is a policy name, and the decorator runs that one policy. `"DefaultCircuitBreaker"`
  throws `ConfigurationException`, and `circuitBreakerName:` is `CS1739`. **Maintainer's ruling,
  2026-09-27: record it for phase 5.** No page off the tranche changed
- **Behaviour, run with controls** against the released Darker 4.1.1 packages in scratch apps,
  net10.0, one process per case:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `QueryPipelinePolicies.md`: the default retry policy | `AddDefaultPolicies()`, handler with `[RetryableQuery(1)]` that always throws → **4** attempts, at 0, +56, +158, +308 ms | the same handler without the attribute, with or without `AddDefaultPolicies()` → **1** attempt |
  | The default circuit breaker | `[RetryableQuery(1, Constants.CircuitBreakerPolicyName)]`: call 1 → the handler's exception, **1** attempt; call 2 at once → `BrokenCircuitException`, **0** attempts | call 3 after 600 ms → the handler runs again, **1** attempt |
  | A policy name `AddDefaultPolicies()` does not register (off the tranche) | `[RetryableQuery(1, "DefaultCircuitBreaker")]` → `ConfigurationException` *"Policy does not exist in policy registry: DefaultCircuitBreaker"*, **0** attempts | `Constants.CircuitBreakerPolicyName`, above → the handler runs |
  | `QueryObjectValidation.md` #2: data annotations stop an invalid query before the handler | `[ApiController]` controller, `MaxResults=5000` → **400** | a controller without `[ApiController]` → **200**, action runs, `ModelState.IsValid` `False` |
  | The same, on a minimal API | `[AsParameters]` endpoint after `AddValidation()` → **400** | without `AddValidation()` → **200**, handler runs with `5000` |

  Every case with a valid query (`SearchTerm=shoes&MaxResults=10`) returned 200. Darker needs an
  `ILoggerFactory` in the container: without `AddLogging()` the first query throws
  `TypeInitializationException` from `PipelineBuilder<T>` (`ApplicationLogging`, `factory` null).
  An ASP.NET Core host registers logging itself, so no page here meets it
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 16 rows at `3462412`. `--report` → exit **0**, *"983 blocks: 189 BUILT, 778 FAILED,
  16 SKIPPED"*, baseline 189, 0 findings. Joined on page and ordinal against the report at
  `842b757`, all 16 blocks that moved went `FAILED -> BUILT`, and there are no new keys
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; `pagelint --changed origin/master` 0 errors.
  **Pages changed: 4** (`git diff --name-only 842b757..HEAD -- contents`), all on the tranche.
  `PaginationQueryPatterns.md` did not change: its three FAILED blocks are same-page, and each
  follows the block that declares its types

**Task 3.6 — phase 3 closed, 2026-09-27, branch at `2223fcb`.** Every gate run bare, exit code
read before its output:

| # | Gate | Exit | Read | Predicted (3.1) | Agrees? |
|---:|---|---:|---|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | none | **yes** |
| 2 | `pagelint` | 0 | 0 errors, **658** warnings across 96 pages, 162 pages | 0 errors; 706 → 664 or 663 | **no — 6 lower**, explained below |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | none | **yes** |
| 4 | redirects | 0 | 77 entries, 7858 bytes | none | **yes** |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | none, scope held at 18 across 5 | **yes** |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | none | **yes** |
| 7 | `--verify` | 0 | 161 predicted = 161 published | none | **yes** |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced | none | **yes** |
| 9 | `blockcheck` | 0 | 983: **189** BUILT, 778 FAILED, 16 SKIPPED; 0 findings; 538 reference assemblies; baseline 189; **30** units, 0 violations; 36 pages mapped | BUILT 153–193; SKIPPED 16; exit 0; 0 violations | **yes** — within the range |
| — | `attr_mismatch.py` | 1 | **7** | held at 7 | **yes** |

`pagelint --changed origin/master` → exit **0**, 0 errors.

**Gate 9, reconciled against 3.1's 64 reachable.** **58** of them built. The six that did not are
in § *Blocks that stay FAILED*: `DistributedLock.md` #2 and `DynamoDbDistributedLock.md` #1, #2 for
the V4 pin, `TickerQScheduler.md` #2, #3 for packages phase 4 asks for, and `QueryPipelinePolicies.md`
#1 for `Program`. Beside them, one block the tranche added (`SchedulingAMessage.md` #7, the #4414
workaround) and one off the tranche (`AWSSQSConfiguration.md` #6, after its `SqsAttributes` repair).
129 + 58 + 1 + 1 = **189**, 4 below the ceiling.

**Every FAILED block on the 20 tranche pages is listed**: `after.tsv`'s FAILED keys on those pages
and § *Blocks that stay FAILED*'s phase 3 rows are the same 9 — the six above and
`PaginationQueryPatterns.md` #2–#4, same-page.

**Gate 2, reconciled.** Per-page warnings at `e256e2b` (a worktree) against the branch: the 20
tranche pages fell by **42**, which is 3.1's 664 — `QueryPipelinePolicies.md` #3 was not touched, so
not 663. The further **−6** are on four pages outside the tranche, all changed in 3.2 by
recurrence: `KafkaConfiguration.md` −3 (the `SetConfigHook` and `RequestType` repairs),
`AWSSQSConfiguration.md`, `InMemoryOptions.md` and `V10MigrationGuide.md` −1 each. 3.1 said
recurrences would lower it further and would be explained; 3.3's and 3.4's recurrence pages moved
nothing. 706 − 42 − 6 = **658**, across 96 pages, down from 107.

**AC2, against a `before.tsv` regenerated from `c7329bb` in a worktree** (exit 0, *"989 blocks: 101
BUILT, 872 FAILED, 16 SKIPPED"*, 989 rows): the diff prints **90** lines. **89** are `FAILED ->
BUILT` — 30 phase 2's, 58 on this tranche's pages and `AWSSQSConfiguration.md` #6. The 90th is
` -> BUILT contents/SchedulingAMessage.md 10`, a key `before.tsv` does not hold, read against
§ *Splits*: the block inserted at #7 moved old #7–#9 to #8–#10. No `-> SKIPPED`. **Control, both
ways:** a copy of the branch's report with `QueryResultTypes.md` #1 set FAILED prints 89 lines and
loses that one; a copy with `AWSSQSConfiguration.md` #1 set FAILED prints exactly one line beside the
new key that is not `FAILED -> BUILT`, `BUILT -> FAILED contents/AWSSQSConfiguration.md 1`.

**The ≤ 60 target, read at tranche 1's close.** Pages with nothing BUILT: **74** — requirements'
`awk` (FAILED pages absent from the BUILT set, `comm -23`) and a Python join over the same report
agree. 97 at `c7329bb`, so **23** of the 37 have landed: 9 in phase 2 and **14** in phase 3, one short
of the 15 3.1 predicted. The one is `DynamoDbDistributedLock.md`, whose two blocks wait on the V4
pin (3.3). Tranche 1 was to land 24 and landed 23. **74 − 60 = 14 of tranche 2's 19** must now land in
phases 4 and 5, one more than 3.1's 13; 5 may miss.

**Every behavioural block was run with its control** (P0-10): the tables under 3.2, 3.3, 3.4 and
3.5, against released 10.7.0 and Darker 4.1.1 packages.

**The PR changes 29 pages** (`git diff --name-only origin/master..HEAD -- contents | wc -l`): **15**
of the 20 tranche pages, and **14** outside the tranche by recurrence — 8 in 3.2, 4 in 3.3, 2 in
3.4. The other five tranche pages were made whole by a unit alone (`GcpPubSubConfiguration.md`,
`MQTTConfiguration.md`, `MSSQLMessageBroker.md`, `MSSQLTransportInboxAndOutbox.md`) or left as they
were (`PaginationQueryPatterns.md`). Beside them are 13 files under `tools/` — `README.md`, the
baseline, `pages.tsv` (23 → 36 pages mapped), 8 new units and 2 grown — two probe scripts, and this
one.

---

## Phase 4 — Tranche 2a *(5 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 1-hard-block page in *Outbox and Inbox* leaves whole, its one hard block repaired,
skipped with an accepted reason, or listed. Page list: § *The tranches*, phase 4 table.

- [x] **Task 4.1:** Predict phase 4's movement
  - Input: § *The tranches*, phase 4 table; § *Phase 3 as executed*
  - Output: § *Phase 4 as executed* opens with a prediction per gate, **and a verdict per hard block
    before it is touched**: parse (placeholder / fragment / not code) or other (defect / wrapper
    artefact), from `--classify` and `--explain`

- [x] **Task 4.2:** Repair the outbox and inbox pages of the tranche
  - Input: the phase 4 rows whose page is an Outbox or Inbox; 4.1's verdicts
  - Output: each page whole; baseline rows; each defect in § *Defect ledger* with its recurrence
    grep (obligation 14)

- [ ] **Task 4.3:** Repair the distributed-lock pages of the tranche
  - Input: the phase 4 rows whose page is a Distributed Lock; 4.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2
  - Notes: the lock pages share a shape; a defect found on one is grepped for on all before the
    next is opened.

- [ ] **Task 4.4:** Repair the remaining *Outbox and Inbox* tranche pages
  - Input: the phase 4 rows not covered by 4.2 or 4.3 (at § 2's pin: `UsingSweeperCircuitBreaking.md`,
    `AzureBlobArchiveProvider.md`, `ReplayOnSeenReference.md`); 4.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2
  - Notes: `ReplayOnSeenReference.md#1` uses API not in 10.7.0 (requirements P2-2); it stays FAILED
    and is listed with that reason, not repaired toward `master`.

- [ ] **Task 4.5:** Close phase 4 — checks, figures, PR
  - Input: 4.1's prediction; the PR's diff
  - Output: as 2.6, for phase 4

### Phase 4 as executed

**Prediction, 2026-09-27, from `master` `d8633b1`**, the merge of phase 3 (PR #192). The *Now*
column is this task's own run at `d8633b1`, every gate bare, exit code read before its output; all
nine exit 0 and read at `tools/README.md`'s figures. Each prediction names its mechanism:

| # | Gate | Now | Predicted after phase 4, and why |
|---:|---|---|---|
| 1 | `linkcheck` | 165 files, 0 broken | **none**. No file is added |
| 2 | `pagelint` | 0 errors, 658 warnings, 162 pages | **errors 0; warnings 658 → between 640 and 622.** On the 22 pages, rule 6 warns on **36** blocks, every one FAILED: the **18** reachable blocks and **18** of the 22 hard ones. The four hard blocks that do not warn already carry `using`s (`DapperOutbox.md` #2, `DynamoInbox.md` #1, `DynamoOutbox.md` #2, `ReplayOnSeenReference.md` #1). A reachable repair gives its block `using`s whether or not it then builds (−18, to 640); each hard block repaired rather than listed takes one more, to 622 if all 18 are. Recurrence repairs off the tranche lower it further, and are explained if so |
| 3, 4, 7 | shape, redirects, `--verify` | 161 / 77 / 161 | **none**. No `SUMMARY.md` change |
| 5 | `versioncheck` | 0 stale of **18, across 5 pages** | **none, scope held at 18 across 5.** Its five pages (`tools/versioncheck.py:80`) are the tutorials and `GetStarted.md`, none in phase 4 |
| 6 | `optioncheck` | 0 mismatches, 59 tables, 519 rows | **none.** Five phase 4 pages carry a table it reads — `AzureBlobDistributedLock.md`, `DynamoInbox.md`, `DynamoOutbox.md`, `InMemoryOutbox.md`, `PostgresDistributedLock.md`, 5 tables, 16 rows (`dotnet run --project tools/optioncheck -- <page>`, per page). A row changes only if a repair finds a documented default wrong, and is then a defect in § *Defect ledger* |
| 8 | `symbolcheck` | 0 findings, 22 entries, 161 pages, 3 silenced | **none**. No repair here names a watchlisted symbol |
| 9 | `blockcheck` | 983: 189 BUILT, 778 FAILED, 16 SKIPPED; 538 reference assemblies; 30 units | **BUILT 189 → at least 204, at most 230; SKIPPED 16 plus accepted reasons only; reference assemblies up with the pin; exit 0; units 30 plus the new ones, 0 violations.** Mechanism below |
| — | `attr_mismatch.py` | **7**, exit 1 | **held at 7.** Its hits are on five pages, none in phase 4. `InMemoryInbox.md` #2's defect is an attribute on a class, not on a handler of the other kind, so the script does not read it |

**Gate 9, by source.** The probe re-run at `d8633b1` (§ *The tranches*' recipe, `Order` excluded)
reads the phase 4 table unchanged: **40 FAILED, 18 reachable, 0 same-page, 22 hard, 6 BUILT**.
The 18 reachable are **9** with a `using` alone (`AzureBlobDistributedLock.md` #1,
`DynamoOutbox.md` #3, `FirestoreDistributedLock.md` #1, `MongoDbDistributedLock.md` #1,
`MsSqlDistributedLock.md` #1, `MySQLOutbox.md` #1, `MySqlDistributedLock.md` #1,
`PostgresDistributedLock.md` #1, `SqliteOutbox.md` #1), **6** with an empty stub (`services`:
`MSSQLOutbox.md` #2, `MySQLOutbox.md` #2, `PostgresOutbox.md` #2, `SqliteOutbox.md` #2,
`UsingSweeperCircuitBreaking.md` #2, #3), **2** needing a stub with members (`InMemoryOutbox.md` #2,
`UsingSweeperCircuitBreaking.md` #4) and **1** a typed value (`InMemoryInbox.md` #1, `services`).

- **Floor 204** = 189 + 9 + 6: only the `using` and empty-stub blocks
- **Ceiling 230** = 189 + 18 + 21 + 2: every reachable block; every hard block but
  `ReplayOnSeenReference.md` #1, which stays FAILED by P2-2; and `TickerQScheduler.md` #2, #3, off
  the tranche, once the pin carries the two TickerQ packages. Each hard block that is listed rather
  than repaired, and each defect a stub surfaces, lowers it by one
- **The pin grows by three, and is measured alone first**, as 1.8 was:
  `Npgsql.EntityFrameworkCore.PostgreSQL` for `PostgresOutbox.md` #3, and `TickerQ.EntityFrameworkCore`
  and `TickerQ.Dashboard` at 9.0.2 for `TickerQScheduler.md` #2, #3 (§ *Blocks that stay FAILED*,
  where both built in scratch against the released packages). The pin carries **95**
  `PackageReference`s (`grep -c`). Predicted alone: **+2 BUILT** (the TickerQ blocks), no other
  verdict moves, because `UseNpgsql` sits behind `PostgresOutbox.md` #3's shape error. The other
  three EF Core providers are already in the reference set, transitively
  (`Microsoft.EntityFrameworkCore.SqlServer.dll`, `.Sqlite.dll`, `Pomelo.EntityFrameworkCore.MySql.dll`
  in `refs.txt`), so `MSSQLOutbox.md`, `MySQLOutbox.md` and `SqliteOutbox.md` #3 need no ask
- **Pages with nothing BUILT: 74 → at most 66, as low as 58.** 17 phase 4 pages have no BUILT
  block. **8** reach one by a `using` or an empty stub alone (`AzureBlobDistributedLock.md`,
  `FirestoreDistributedLock.md`, `MongoDbDistributedLock.md`, `MsSqlDistributedLock.md`,
  `MySQLOutbox.md`, `MySqlDistributedLock.md`, `PostgresDistributedLock.md`, `SqliteOutbox.md`);
  **2** by a stub with members or a typed value (`InMemoryInbox.md`, `InMemoryOutbox.md`); **6**
  only by repairing their one hard block (`AzureBlobArchiveProvider.md`, `DynamoInbox.md`,
  `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`); and
  `ReplayOnSeenReference.md` by none. 74 − 16 = **58**
- **The ≤ 60 target, read ahead.** Phases 4 and 5 must land 14 of tranche 2's 19 (§ *Phase 3 as
  executed*). Phase 4 holds **10** of the 19 — its pages with nothing BUILT and ≥ 1 reachable block,
  the first ten above — and phase 5 the other 9. So phase 4 must land **at least 5** of the 10 if
  phase 5 lands all nine. The six hard-only pages are not among the 19 but count toward ≤ 60 all the
  same: every one landed is one phase 5 need not

**A verdict per hard block, before it is touched.** From `--classify` (on the page's own text) and
`--explain` (on the probe's `stageP`, `using`s supplied). *Parse* is placeholder, fragment or not
code; *other* is defect or wrapper artefact. **Seven parse blocks carry a defect in the text
beside the placeholder**; a placeholder repair that does not repair it leaves the block FAILED.

| Page | # | `--classify` / probe | Verdict | What the diagnostics and the text show |
|---|---:|---|---|---|
| `AzureBlobArchiveProvider.md` | 1 | parse / PARSE | **parse — placeholder, and defects** | `{ ...  }` and a trailing `...` (`CS8635`); `.ConfigureServices(hostContext, services) =>` opens no lambda (`CS1519`, `CS1001`). Behind them, read at 10.7.0: `New AzCliCredential();` inside an initialiser; `AzureBlobArchiveProviderOptions` has no parameterless constructor (a primary constructor taking `blobContainerUri`, `tokenCredential`, `accessTier`, `tagBlobs`) and its properties are `init`; `UseOutboxArchiver<TTransaction>` cannot infer `TTransaction`; `BatchSize` is `ArchiveBatchSize`; `MinimumAge` is a `TimeSpan`, not `744`; the option assignments name no `options.` |
| `AzureBlobDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `opt.Outbox = /* your external Outbox */;` (`CS1525`), alone |
| `DapperOutbox.md` | 2 | other / DEFECT | **other — wrapper artefact** | A `public override` `HandleAsync` outside its handler class: the `members` wrapper derives from `object` (`CS0117` on `base.HandleAsync`), and `_transactionProvider`, `_postBox`, `_logger` are the unshown class's fields |
| `DynamoInbox.md` | 1 | parse / PARSE | **parse — placeholder, and defects** | `...` twice and the same unopened `ConfigureServices` lambda; `{ ServiceURL = "…"; }`, a `;` inside an object initialiser; `credentials` is never shown |
| `DynamoOutbox.md` | 2 | other / DEFECT | **other — wrapper artefact** | As `DapperOutbox.md` #2: an `override` outside its class, `CS0117`, the same three fields |
| `FirestoreDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `/* your external Outbox */;`, alone |
| `InMemoryInbox.md` | 2 | import / DEFECT | **other — defect** | `[UseInboxAsync(...)]` on the class: `CS0592`, *"only valid on 'method' declarations"* (`RequestHandlerAttribute`'s usage at 10.7.0). The same attribute is repeated, correctly, on `HandleAsync`. `grep -rn -A1 '^\s*\[UseInbox' contents/ \| grep class` → **1**, this page |
| `InMemoryOutbox.md` | 1 | parse / PARSE | **parse — placeholder** | `/* your producer registry */;`, alone |
| `MSSQLInbox.md` | 1 | parse / PARSE | **parse — placeholder, and a defect** | `...` twice and the unopened `ConfigureServices` lambda |
| `MSSQLOutbox.md` | 3 | import / DEFECT | **other — wrapper artefact** | `CS0116`: a free `public void ConfigureServices` beside a class; no wrapper hosts both. `UseSqlServer` resolves |
| `MongoDbDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `/* your MongoDB Outbox */;`, alone |
| `MsSqlDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `/* your MS SQL Outbox */;`, alone |
| `MySQLInbox.md` | 1 | parse / PARSE | **parse — placeholder, and defects** | As `MSSQLInbox.md` #1, and `opt.InboxConfiguration` inside a lambda whose parameter is `options` |
| `MySQLOutbox.md` | 3 | parse / PARSE | **parse — placeholder** | `....` (`CS8635`, then `CS0029` reading it as a range), inside the same free-method shape as `MSSQLOutbox.md` #3 |
| `MySqlDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `/* your MySQL Outbox */;`, alone |
| `PostgresDistributedLock.md` | 2 | parse / PARSE | **parse — placeholder** | `/* your Postgres Outbox */;`, alone |
| `PostgresInbox.md` | 1 | parse / PARSE | **parse — placeholder, and defects** | As `MySQLInbox.md` #1 |
| `PostgresOutbox.md` | 3 | import / DEFECT | **other — wrapper artefact, and the pin** | `CS0116` as `MSSQLOutbox.md` #3, and `CS1061` `UseNpgsql`: `Npgsql.EntityFrameworkCore.PostgreSQL` is not in the pin |
| `ReplayOnSeenReference.md` | 1 | other / DEFECT | **other — API not in 10.7.0** | `CS0117`: `RequestContextBagNames` at 10.7.0 has `CloudEventsAdditionalProperties`, `JobId`, `PartitionKey`, `Headers`, `WorkflowId` — no `CausationId`. Stays FAILED by P2-2 (task 4.4) |
| `SqliteInbox.md` | 1 | parse / PARSE | **parse — placeholder, and defects** | As `MySQLInbox.md` #1 |
| `SqliteOutbox.md` | 3 | import / DEFECT | **other — wrapper artefact** | `CS0116` as `MSSQLOutbox.md` #3. `UseSqlite` resolves |
| `UsingSweeperCircuitBreaking.md` | 5 | import / DEFECT | **parse — fragment** | `CS0535` for `CoolDown()` and `TrippedTopics`, under the block's own `// Implement other methods using distributed cache`. `TripTopic(RoutingKey)` matches the 10.7.0 interface. The probe calls it DEFECT; read, it is a partial implementation that says so |

**By this reading, 15 parse and 7 other.** `--classify` reads 14 *parse*, 3 *other* and 5 *import*;
the probe, 14 PARSE and 8 DEFECT. The one block whose kind differs is `UsingSweeperCircuitBreaking.md`
#5, a fragment the probe calls a DEFECT. The 7 *other*:

- **Four wrapper artefacts, two shapes.** An `override` outside its class (`DapperOutbox.md`,
  `DynamoOutbox.md` #2) and a free method beside a class (`MSSQLOutbox.md`, `SqliteOutbox.md` #3).
  `PostgresOutbox.md` #3 is the second shape **and** waits on the pin, and `MySQLOutbox.md` #3, a
  *parse* block for its `....`, is the second shape behind it
- **One defect**, `InMemoryInbox.md` #2, and **one API not in 10.7.0**, `ReplayOnSeenReference.md` #1

**Six parse blocks carry a defect beside the placeholder**, so a repair that removes only the
placeholder leaves them FAILED: `AzureBlobArchiveProvider.md` #1 and the five inbox pages'
`#1`s. Their recurrences, run now so that 4.2 opens with them:

| Defect | Grep | Hits | Pages |
|---|---|---:|---|
| `.ConfigureServices(hostContext, services) =>` opens no lambda | `grep -rn 'ConfigureServices(hostContext, services) =>' contents/` | **13** | **8**: the six tranche pages once each, `BrighterBasicConfiguration.md` ×2, `DispatcherConfigurationReference.md` ×5. Both off-tranche pages have **nothing BUILT** (5 FAILED blocks each), so a recurrence repair there can move the ≤ 60 count too |
| `opt.` inside a lambda whose parameter is `options` | a Python scan of every `(p => { … })` for an assignment through another common builder name: **10** hits, read one by one | **3** | `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`. The other 7 are nested lambdas (`AddProducers(configure =>` inside `AddBrighter(options =>`, `AddOtlpExporter(o =>`, `UseMisfireHandler(options =>`), which is correct code |
| `[UseInbox…]` on a class | `grep -rn -A1 '^\s*\[UseInbox' contents/ \| grep class` | **1** | `InMemoryInbox.md` |
| `New AzCliCredential();` | `grep -rn 'New Az' contents/` | **1** | `AzureBlobArchiveProvider.md` |
| `UseOutboxArchiver` without its type argument | `grep -rn 'UseOutboxArchiver' contents/` → 7 lines on 2 pages | **1** | `AzureBlobArchiveProvider.md`; `OutboxArchiver.md`'s three calls all name `<TTransaction>` |

**Carried into the repair tasks, not the prediction:**

- **`Order`**: no phase 4 block names it (`grep -cw Order` over each of the 40 FAILED blocks'
  `--show` output → 0). The 1.10 ruling has nothing to act on here
- **Shared worlds.** The four EF Core outbox pages (`MSSQLOutbox.md`, `MySQLOutbox.md`,
  `PostgresOutbox.md`, `SqliteOutbox.md`) repeat one block shape, and the five `*DistributedLock.md`
  pages another; a unit or a repair on one is tried on its siblings before the next page is opened.
  `InMemoryInbox.md` #1 wants `services` and `subscriptions`, which `InMemoryTransportContext.cs`
  already supplies
- **Found off the tranche:** `QuartzScheduler.md:359` carries a U+200B zero-width space between
  `UseMisfire` and `Handler` (`od -c`; `grep -rlP '\x{200B}' contents/` → that page only, 1 line).
  **It compiles**: C# removes formatting characters before comparing identifiers. Run in a scratch
  net10.0 console app, a call spelled with the U+200B (written by `printf '\u200b'`) resolved to
  `UseMisfireHandler` and printed `called`; the control, `C.UseMisfireHandlr()`, is `CS0117`. So it is not a compile defect but a
  text one — a search of the page for `UseMisfireHandler` misses the line. The page is in no
  tranche; recorded for phase 5 to remove

**Task 4.2 — the outbox and inbox pages.** Thirteen pages. **BUILT 189 → 214** (+25): all **22**
FAILED blocks on the thirteen, `TickerQScheduler.md` #3 from the pin, and `BrighterBasicConfiguration.md`
#3, #4 from the lambda recurrence. **None of the thirteen keeps a FAILED block.** `pagelint`
**658 → 636**: **19** on the tranche pages, **2** on `BrighterBasicConfiguration.md` and **1** on
`AzureBlobArchiveProvider.md`, both touched by the recurrence (per page, against a worktree at
`d8633b1`). Pages with nothing BUILT **74 → 64**: `DynamoInbox.md`, `InMemoryInbox.md`,
`InMemoryOutbox.md`, `MSSQLInbox.md`, `MySQLInbox.md`, `MySQLOutbox.md`, `PostgresInbox.md`,
`SqliteInbox.md`, `SqliteOutbox.md` and `BrighterBasicConfiguration.md`. Two methods, one figure:
requirements' `awk` and a Python join over the same report.

- **The pin, measured alone first** (`bd2b1ed`): `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4 and
  `TickerQ.Dashboard`, `TickerQ.EntityFrameworkCore` 9.0.2; **98** `PackageReference`s, **542**
  reference assemblies. **Said at 4.1:** +2 BUILT. **Measured:** +1, `TickerQScheduler.md` #3.
  #2 now fails on `Program` alone — `typeof(Program).Assembly` in a `Program.cs` with no declaration
  after its statements, the `statements` wrapper's limitation that keeps `QueryPipelinePolicies.md`
  #1 FAILED (Q2). Its row in § *Blocks that stay FAILED* is rewritten to that reason
- **The ≤ 60 target.** Of phase 4's ten pages with nothing BUILT and a reachable block, **4** landed
  (`InMemoryInbox.md`, `InMemoryOutbox.md`, `MySQLOutbox.md`, `SqliteOutbox.md`); the other six are
  lock pages, 4.3's. Five of the six hard-only pages landed as well, and `BrighterBasicConfiguration.md`
  off the tranche. 64 − 60 = **4** still to land
- **`using`s:** every repaired block. On `MSSQLInbox.md`, `InMemoryInbox.md` and `InMemoryOutbox.md`
  they replace the leading `// ...`
- **Four units and two grown.** `RelationalOutboxContext.cs` supplies `services` to the four EF Core
  outbox pages; `RelationalInboxContext.cs` supplies `connectionString` to the four relational inbox
  pages — two units, because each outbox page's block 2 declares its own `connectionString`.
  `InMemoryBoxContext.cs` supplies `services`, `subscriptions`, `producerRegistry` and the small
  `Person` domain both InMemory pages' handlers use. `DynamoInboxContext.cs` supplies `credentials`.
  `PageContext.cs` (`DapperOutbox.md`) and `DynamoOutboxContext.cs` gain the requests and entities of
  the samples their handlers come from, `WebAPI_Dapper` and `WebAPI_Dynamo`, typed as 10.7.0's samples
  type them; each page now names its sample. None is a type a page tells the reader to write (rule 1,
  by reading). `MSSQLOutbox.md` and `PostgresOutbox.md` #1, BUILT before, are re-admitted with the
  new unit. `--report` → *"34 units checked, 0 violations"*
- **The wrapper artefacts, made whole** (design: a fragment the reader needs whole). The EF Core
  outbox pages' free `public void ConfigureServices` now sits in `public class Startup`, with a
  comment that in `Program.cs` the same calls go on `builder.Services`. `DapperOutbox.md` and
  `DynamoOutbox.md` #2, an `override` with no class, are shown in `AddGreetingHandlerAsync` with the
  fields and constructor the sample declares. `MySQLOutbox.md`'s `....` is `// ... other Brighter
  options`, as its siblings write it
- **The unopened `ConfigureServices` lambda, at every recurrence: 13 → 0 on 8 pages**, the two
  off-tranche pages included. `pagelint --changed` then asked for `using`s on the touched
  `BrighterBasicConfiguration.md` #3, #4 and `AzureBlobArchiveProvider.md` #1; with them,
  `--explain` found only a bare `...` in each `BrighterBasicConfiguration.md` block, now `// ...`,
  and both build. `AzureBlobArchiveProvider.md` #1 keeps its five defects for 4.4. The five
  `DispatcherConfigurationReference.md` blocks each open with `// ...`, so rule 6 does not reach them;
  they stay FAILED on placeholders and names the page never shows
- **Six defects, and one upstream** (§ *Defect ledger*): `[UseInboxAsync]` on a class; `Post` awaited
  with a `cancellationToken` it does not take; a `;` inside an object initialiser; `opt.` in a lambda
  whose parameter is `options`; the InMemory Inbox said to keep entries until restart; the page's
  `Warn` configuration beside an attribute whose default `Throw` wins. **BrighterCommand/Brighter#4335**
  — a global `InboxConfiguration` is ignored unless the application calls `AddProducers`. Fixed by
  #4396 on `master`, in no release. **Maintainer's ruling, 2026-09-27: state it once, link from the
  inbox pages.** `BrighterInboxSupport.md` gains *Global Inbox Configuration in a Consumer-Only
  Application*, with the workaround, and the nine inbox pages one sentence each after their
  configuration block
- **Behaviour, run with controls** against released 10.7.0 packages in scratch console apps,
  net10.0, one process per case:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `InMemoryInbox.md`: *"No cleanup: Old entries remain until process restart"* | entry written, fake clock +11 min, another add → entry **gone**, count **1** | no advance → present, count **2**; +6 min, under the 10-min scan interval → present |
  | *"Memory bound: All seen message IDs held in memory"* | `EntryLimit = 4`, 10 adds → **8** held (compacted once, to half, then not again within the interval) | `EntryLimit = -1` → **10** |
  | #1 with #2: a duplicate reaching the handler | the page's `Warn` configuration and #2's attribute, the same event published twice → publish 2 throws **`OnceOnlyException`**; handler runs **1** | the attribute with `onceOnlyAction: OnceOnlyAction.Warn` → both return; runs **1** |
  | The global Inbox, from `InboxConfiguration` alone (#4335) | no attribute, `AddConsumers` only → both return; handler runs **2** | the same with `AddProducers` → runs **1**. The attribute without producers → runs **1** |
  | The deduplication window, end to end | publish, fake clock +11 min, publish another, publish the first again → runs **2** | — the first two rows are its controls |
  | `SqliteOutbox.md` intro: messages *"saved within the same transaction as your business logic"* | #1's DDL, #3 verbatim in `Startup`, an insert and `DepositPostAsync` in the EF transaction, commit → Greeting **1**, Outbox **1** | rollback → Greeting **0**, Outbox **0** |

  Read, not run: `DapperOutbox.md` #2 is the handler `TransactionalMessagingWithTheOutbox.md` #1 ran
  in 3.3, unchanged but for its class. `DynamoOutbox.md` #2 and `DynamoInbox.md` #1 need DynamoDB;
  they register and transact through types each compiled against, and assert nothing the SQLite run
  above does not. The relational inbox blocks configure the store 3.2's MSSQL inbox run exercised
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 1 row at `bd2b1ed` (the pin), 24 rows and 2 re-admissions at `a8ede7c`. `--report` →
  exit **0**, *"983 blocks: 214 BUILT, 753 FAILED, 16 SKIPPED"*, baseline 214, 0 findings. Joined on
  page and ordinal against the report at `9479991`, all 25 blocks that moved went `FAILED -> BUILT`,
  and there are no new keys
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; shape, redirects and `--verify` unmoved;
  `pagelint --changed origin/master` 0 errors. **Pages changed: 20** (`git diff --name-only
  9479991..HEAD -- contents`): the **13** tranche pages, `AzureBlobArchiveProvider.md` (4.4's) and
  **6** outside the tranche — `BrighterBasicConfiguration.md` and `DispatcherConfigurationReference.md`
  by the lambda recurrence, `BrighterInboxSupport.md`, `FirestoreInbox.md`, `MongoDBInbox.md` and
  `SpannerInbox.md` by the #4335 ruling

---

## Phase 5 — Tranche 2b and the recorded falsehoods *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 1-hard-block page outside *Outbox and Inbox* leaves whole, and P0-7 is done
everywhere. Page list: § *The tranches*, phase 5 table.

- [ ] **Task 5.1:** Predict phase 5's movement
  - Input: § *The tranches*, phase 5 table; § *Phase 4 as executed*; `attr_mismatch.py`'s count now
  - Output: § *Phase 5 as executed* opens with a prediction per gate and a verdict per hard block,
    as 4.1; and the P0-7 predictions — `ITimerProvider` 4 → 0, `attr_mismatch` to exactly the
    deliberate `PipelineValidation.md:250`

- [ ] **Task 5.2:** Repair the tranche pages in *Commands, Handlers and Pipelines*, *Darker* and *Understanding Brighter*
  - Input: those sections' phase 5 rows; 5.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2

- [ ] **Task 5.3:** Repair the tranche pages in *Transports*, *Using an External Bus* and *Health Checks and Observability*
  - Input: those sections' phase 5 rows; 5.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2

- [ ] **Task 5.4:** Repair `ITimerProvider` and the unshown `Order`
  - Input: `grep -rn ITimerProvider contents/` (4 lines at `c7329bb`, all `InMemoryScheduler.md`);
    `design.md` § *API Resolved* (`InMemorySchedulerFactory.TimeProvider`);
    `CQRSWithBrighterAndDarker.md`'s `Id = command.Id` block
  - Output: `grep -rn 'ITimerProvider' contents/ | wc -l` → **0**, read from a file; the `Order`
    block BUILT or showing `Order`; both in § *Defect ledger*
  - Notes: a sentence claiming how the scheduler uses time is run, with a control (P0-10).

- [ ] **Task 5.5:** Repair the E4 attribute mismatches not already repaired
  - Input: `probe/attr_mismatch.py` output now; `design.md` § *E4* (verdict per hit) and
    § *API Resolved*
  - Output: `attr_mismatch.py > out; echo $?` → **1** with exactly `PipelineValidation.md:250` in
    `out`; `attr_mismatch.py --plant` → **0**; each repaired hit in § *Defect ledger*
  - Notes: `PipelineValidation.md:280` sits in a *Before (warning)* example about step order — the
    repair keeps what that example demonstrates and changes only the attribute's kind; `:289` also
    names an argument before a positional one. A page that shows a mismatch deliberately says it is
    wrong in prose, not only in a `// wrong` comment.

- [ ] **Task 5.6:** Close phase 5 — checks, figures, PR
  - Input: 5.1's prediction; the PR's diff
  - Output: as 2.6, for phase 5; plus BUILT against **≥ 250** and pages with nothing BUILT against
    **≤ 60**, both read before phase 6 walks them

---

## Phase 6 — Acceptance *(8 tasks, one PR, no page touched)*

**Goal:** every criterion decided by its command or its named reader, the page set checked for
widening, both ledgers written, and the residual sentence 018 starts from.

- [ ] **Task 6.1:** Walk the three criteria with no instrument by design — AC6, AC8's second half, AC11
  - Input: `requirements.md` § *Acceptance criteria*; `--list-scaffold` per stub unit; the diff's
    `// ...` lines; each PR's recorded runs
  - Output: § *The acceptance walk*, one row each: the reader named, what they read, their verdict.
    Walked first — both criteria ever found unmet at a close were unmarked ones

- [ ] **Task 6.2:** Walk the four criteria instrumented by this spec — AC3, AC4, AC5, AC12
  - Input: the red-proofs in § *Phase 1 as executed*; `--classify`; `--list-skips`
  - Output: one walk row each, the command and its output, exit code read first

- [ ] **Task 6.3:** Walk AC1, AC2, AC7, AC8's first half, AC9, AC10, AC13, AC14
  - Input: `requirements.md`'s commands; `before.tsv`; § *Splits*; § *Defect ledger*
  - Output: one walk row each; AC2's diff read against § *Splits*; AC14 is all eight other gates,
    bare, against `tools/README.md`

- [ ] **Task 6.4:** Run the backwards check
  - Input: `git diff --name-only c7329bb -- contents/ tools/`; § *The tranches*; phase 5's P0-7 pages
  - Output: § *Backwards check* — every changed page is a tranche page or a P0-7 page, and every
    changed `tools/` file is a design deliverable; anything else is named and justified or reverted

- [ ] **Task 6.5:** Write the defect ledger
  - Input: § *Defect ledger* rows from phases 2–5
  - Output: the ledger complete — defect, page, recurrence grep, count before, count after (**0**),
    and *found by*: `--classify`, `attr_mismatch`, a run, or re-derivation

- [ ] **Task 6.6:** Write the friction ledger
  - Input: the *as executed* sections of phases 1–5
  - Output: § *Friction ledger*, one row per place the workflow itself got in the way, each with
    what 018 should do about it

- [ ] **Task 6.7:** Put D4 to the maintainer, with 017's runs as evidence
  - Input: `design.md` D4; every `attr_mismatch.py` run recorded in phases 2–5
  - Output: a § *For the maintainer* entry: the proposal (a `pagelint` rule or a `blockcheck` mode,
    with the `CLAUDE.md` ledger row it would need), the hits it found and when, and the maintainer's
    answer or *not yet ruled*

- [ ] **Task 6.8:** Close the spec
  - Input: `README.md` § *Status Checklist*; `--classify` over the whole corpus
  - Output: the README checklist ticked through *Spec closed*; this file's and the README's status
    lines updated; § *What 017 shipped* ending in **one sentence naming the residual**, its figure
    re-derived by `--classify` (P2-1) — the line 018 starts from; `PROMPT.md` updated

---

## The AC2 before-report

Every AC2 diff reads against this report. It is rebuilt at each use, from a worktree, with the
tools as they stood at `c7329bb`:

```bash
git worktree add --detach $TMPDIR/wt-c7329bb c7329bb
cd $TMPDIR/wt-c7329bb
dotnet build tools/blockcheck/refs/refs.csproj -c Release
dotnet build tools/blockcheck/blockcheck.csproj -c Release
python3 tools/blockcheck.py --report $TMPDIR/before.tsv; echo $?      # 0
cd - && git worktree remove --force $TMPDIR/wt-c7329bb
```

**A regenerated copy must read** `989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED, 0 NOT_COMPILABLE`
**and have 989 rows** (`wc -l < $TMPDIR/before.tsv`). The diff, from `requirements.md` AC2:

```bash
awk -F'\t' 'NR==FNR{a[$2 FS $3]=$1;next} a[$2 FS $3]!=$1{print a[$2 FS $3]" -> "$1, $2, $3}' \
    $TMPDIR/before.tsv <after.tsv>
```

Measured 2026-09-26 (task 1.1): the regenerated report is **byte-identical** to `--report` at
`master` `0e1eae9` (`cmp` silent), which is the second method — `git diff --stat c7329bb 0e1eae9 --
contents tools` is empty, so the two must agree. **Control, both ways:** the diff of the report
against itself prints **0** lines; against a copy with `Telemetry.md#1` flipped it prints exactly
`BUILT -> FAILED contents/Telemetry.md 1`.

## The tranches

**Fixed by task 1.10, 2026-09-27, against the pin 1.8 committed (`23aa74f`).** Regenerate with:

```bash
bash spec/017-compile_repairs/probe/run.sh $W
awk -F'\t' '!($1=="type" && $2=="Order" && $3=="StackExchange.Redis")' $W/types.tsv > t && mv t $W/types.tsv
python3 spec/017-compile_repairs/probe/pages.py $W $W/r.tsv
python3 spec/017-compile_repairs/probe/stubs.py $W
python3 spec/017-compile_repairs/probe/tranches.py $W
python3 tools/blockcheck.py --classify > $W/c.tsv; echo $?                  # 0
```

and a join of `$W/verdicts.tsv`, `$W/stubs.tsv` and `$W/r.tsv` per page, each page's section being
the `SUMMARY.md` `##` it is listed under. **Hard** is PARSE + DEFECT. **Reachable** is BUILT by a
`using` or an empty stub, plus MEMBERS and HIDDEN: the blocks a `using` and a stub can reach.
**Same-page** stays FAILED by rule. *BUILT now* is the page's rows in `--report` today.

**The `awk` line removes one row, and the tables depend on it.** 1.8's pin brought in
`StackExchange.Redis`, which ships `enum Order`. **27 FAILED blocks** name an `Order` their page
never declares — a domain type, P0-3's kind. The probe resolves it to the Redis enum, adds `using
StackExchange.Redis;`, and reports the error that follows as a DEFECT. Without the line the probe
reads **35 / 40**; with it, **37 / 38**. The two pages that move are `SchedulingAMessage.md` #1
(*"'Order' does not contain a definition for 'ProcessSchedulerId'"*) and `TestingQueryHandlers.md`
#1 (*"'Order' does not contain a definition for 'Id'"*), each the page's one hard block. With
`Order` excluded, both are 0-hard. **`--classify` has the same blind spot**: it puts both blocks
in *import* on `Order`, so a phase that followed it would add the Redis `using`. **Ruled
2026-09-27: `--classify` stays as it is, and every repair phase reads `Order` as a type the page
never shows** (§ *Phase 1 as executed*, 1.10).

| Figure | Design (E2 pin) | § 2 (pin at `c7329bb`) | Now, `Order` excluded |
|---|---:|---:|---:|
| Tranche 1 pages (0 hard) / tranche 2 pages (exactly 1) | 35 / 40 | 34 / 34 | **37 / 38** |
| Tranche 1 reachable / same-page | 88 / 21 | — | **100 / 21** |
| Tranche 2 reachable / hard | 72 / 40 | — | **62 / 38** |
| Pages with nothing BUILT; of those, in a tranche with ≥ 1 reachable block; of those, reachable by a `using` or empty stub alone | 97; 43; 25 | — | **97; 43; 25** |

**Reconciled against § 2**, by running the probe again in a worktree at `master` `0e1eae9`, whose
pin predates 1.8. It reads **34 / 68**, § 2's figures. By page, the grown pin:

| Page | § 2 | Now | Why |
|---|---|---|---|
| `RequestValidation.md` | — | tranche 1 | #4, #7, #8, #10 DEFECT → STUB: `DataAnnotations` and the Validation packages resolve |
| `PaginationQueryPatterns.md` | — | tranche 1 | #2, #4 DEFECT → STUB: EF Core (`DbContext`, `ToListAsync`, `CountAsync`) |
| `TestingQueryHandlers.md` | — | tranche 1 | #2, #3 DEFECT → STUB: xunit (`[Fact]`, `Assert`) and EF Core |
| `AggregationQueryPatterns.md`, `ParameterizedQueryPatterns.md`, `ProjectionQueryPatterns.md`, `QueryHandlerDependencies.md` | — | tranche 2 | DEFECT → STUB on EF Core blocks, one hard block left each |
| `SchedulingAMessage.md` | tranche 1 | tranche 1 | unmoved once `Order` is excluded; tranche 2 if it is not |

34 + 3 = **37**; 34 + 4 = **38**. The design's 35 / 40 is what the committed pin reads when `Order`
is counted as a defect. E2 carried `Hangfire.Redis.StackExchange` (design § *E2*) and so the same
enum, which means the design's figures carry the same misreading.

**The ≤ 60 target stands:** 97 − 60 = 37 of the 43 reachable pages must gain a BUILT block — 24 in
tranche 1, 19 in tranche 2. Pages with nothing BUILT: the FAILED pages in `r.tsv`
(`awk -F'\t' '$1=="FAILED"{print $2}' | sort -u`, **140**) less those with a BUILT row → **97**.

**Two methods agree.** Block by block over all 872 FAILED blocks, `--classify`'s class against the
probe's verdict, keys identical:

| `--classify` | probe | Blocks |
|---|---|---:|
| parse | PARSE | **175**, every block of both |
| other | DEFECT | **15**, every *other* is a DEFECT |
| import | DEFECT | **162**: a missing `using` stops the compiler before the defect, and the probe supplies the `using` and sees it |
| import / page-type / values / same-page | STUB or BUILT | 520 |

So `--classify`'s hard count (`parse` + `other`) is never above the probe's, on any of the 140
pages. On the 75 tranche pages it agrees with the probe on all **37** of tranche 1, and on **22** of
tranche 2. The other **16** read 0 in `--classify` and 1 in the probe, each through one
*import* → DEFECT block. The tranche lists are therefore the probe's, with `--classify` as their
lower bound: **a phase that repairs an *import* block should expect a defect behind it.**

**The § 3 split stands.** No section is empty. *Outbox and Inbox* holds 22 of tranche 2's 38
pages, which is what phase 4 was designed for.

**The P0-7 pages fall in no tranche.** Over the 75 pages, `grep -c` for `HowServiceActivatorWorks`,
`PipelineValidation`, `PolicyRetryAndCircuitBreaker`, `ReactorAndProactor`, `V10MigrationGuide`
(E4; `attr_mismatch.py` → **7**, exit 1, unchanged), `InMemoryScheduler` and
`CQRSWithBrighterAndDarker` → **0** each. Their repairs stay in phase 5.

**P1-3's trigger, `args`.** `python3 tools/blockcheck.py --explain` over the 223 FAILED blocks on
tranche pages (exit 0, *"223 blocks explained, 1197 diagnostics"*), then `grep -F "'args'"` →
**11 blocks on 8 pages**, each `CS0103 The name 'args' does not exist`. The same grep over all 872
FAILED blocks → **36**, 1.3's figure. **Phase 2 holds the first, so phase 2 carries P1-3:**

| Phase | Blocks naming `args` |
|---:|---|
| 2 | `TutorialFirstMessage.md` #2, #4; `TutorialStreamingWithKafka.md` #1, #2 |
| 3 | `GcpPubSubConfiguration.md` #1, `MQTTConfiguration.md` #1, `MSSQLMessageBroker.md` #1, `RedisConfiguration.md` #1, `RocketMQConfiguration.md` #1, `QueryPipelinePolicies.md` #1, `TickerQScheduler.md` #1 |

**`UseNpgsql` reaches phase 4.** `PostgresOutbox.md` is in tranche 2. Its one hard block, #3, reads
`CS1061 'DbContextOptionsBuilder' does not contain a definition for 'UseNpgsql'` once the probe's
`using`s are supplied, beside a `CS0116` shape error. Adding `Npgsql.EntityFrameworkCore.PostgreSQL`
to the pin goes to the maintainer in phase 4's PR (1.8). `HangfireScheduler.md` is in no tranche.

**Said, by design § *Target And Tranches*:** *"`RequestValidation.md` (14 reachable) …
`AnalyzerSupport.md` (6 each)"*. **Measured:** those are FAILED counts, not reachable ones.
`RequestValidation.md` is 14 FAILED: 8 reachable and 6 same-page. `AnalyzerSupport.md` has **5**
FAILED blocks, unchanged since `c7329bb`, since no verdict moved in phase 1. The design's sentence
is rewritten against the tables below.

### Phase 2 — tranche 1a — 17 pages

| Section | Page | FAILED now | Reachable | Same-page | Hard | BUILT now |
|---|---|---:|---:|---:|---:|---:|
| Get Started | `TutorialDurableOutbox.md` | 2 | 0 | 2 | 0 | 1 |
| Get Started | `TutorialFirstCommand.md` | 2 | 0 | 2 | 0 | 1 |
| Get Started | `TutorialFirstMessage.md` | 3 | 0 | 3 | 0 | 1 |
| Get Started | `TutorialStreamingWithKafka.md` | 2 | 2 | 0 | 0 | 0 |
| Commands, Handlers and Pipelines | `BuildingAnAsyncPipeline.md` | 2 | 0 | 2 | 0 | 1 |
| Commands, Handlers and Pipelines | `ImplementingAHandler.md` | 2 | 1 | 1 | 0 | 0 |
| Commands, Handlers and Pipelines | `ImplementingAsyncHandler.md` | 2 | 1 | 1 | 0 | 0 |
| Commands, Handlers and Pipelines | `RequestValidation.md` | 14 | 8 | 6 | 0 | 2 |
| Commands, Handlers and Pipelines | `ReturningResultsFromAHandler.md` | 1 | 1 | 0 | 0 | 0 |
| Brighter Configuration | `AnalyzerSupport.md` | 5 | 5 | 0 | 0 | 3 |
| Brighter Configuration | `RelationalDatabaseConfigurationReference.md` | 1 | 1 | 0 | 0 | 0 |
| Using an External Bus | `ClaimCheck.md` | 2 | 2 | 0 | 0 | 1 |
| Using an External Bus | `Compression.md` | 2 | 2 | 0 | 0 | 0 |
| Using an External Bus | `ErrorHandlingOptions.md` | 6 | 6 | 0 | 0 | 0 |
| Using an External Bus | `HandlingLargeMessages.md` | 4 | 3 | 1 | 0 | 0 |
| Using an External Bus | `HandlingPoisonMessages.md` | 3 | 3 | 0 | 0 | 3 |
| Health Checks and Observability | `HealthChecks.md` | 1 | 1 | 0 | 0 | 0 |
| **Total** | **17 pages** | **54** | **36** | **18** | **0** | **13** |

### Phase 3 — tranche 1b — 20 pages

| Section | Page | FAILED now | Reachable | Same-page | Hard | BUILT now |
|---|---|---:|---:|---:|---:|---:|
| Transports | `GcpPubSubConfiguration.md` | 1 | 1 | 0 | 0 | 0 |
| Transports | `InMemoryTransport.md` | 4 | 4 | 0 | 0 | 0 |
| Transports | `MQTTConfiguration.md` | 1 | 1 | 0 | 0 | 0 |
| Transports | `MSSQLMessageBroker.md` | 1 | 1 | 0 | 0 | 0 |
| Transports | `MSSQLTransportInboxAndOutbox.md` | 4 | 4 | 0 | 0 | 6 |
| Transports | `PostgreSQLTransportAndOutbox.md` | 3 | 3 | 0 | 0 | 4 |
| Transports | `RabbitMQMigrateToQuorumQueues.md` | 1 | 1 | 0 | 0 | 0 |
| Transports | `RedisConfiguration.md` | 1 | 1 | 0 | 0 | 0 |
| Transports | `RocketMQConfiguration.md` | 1 | 1 | 0 | 0 | 0 |
| Outbox and Inbox | `BoxProvisioningConfiguration.md` | 9 | 9 | 0 | 0 | 0 |
| Outbox and Inbox | `DistributedLock.md` | 2 | 2 | 0 | 0 | 0 |
| Outbox and Inbox | `DynamoDbDistributedLock.md` | 2 | 2 | 0 | 0 | 0 |
| Outbox and Inbox | `TransactionalMessagingWithTheOutbox.md` | 2 | 2 | 0 | 0 | 0 |
| Scheduler | `SchedulingAMessage.md` | 9 | 9 | 0 | 0 | 0 |
| Scheduler | `TickerQScheduler.md` | 6 | 6 | 0 | 0 | 2 |
| Darker | `PaginationQueryPatterns.md` | 3 | 0 | 3 | 0 | 1 |
| Darker | `QueryObjectValidation.md` | 3 | 3 | 0 | 0 | 0 |
| Darker | `QueryPipelinePolicies.md` | 5 | 5 | 0 | 0 | 1 |
| Darker | `QueryResultTypes.md` | 6 | 6 | 0 | 0 | 0 |
| Darker | `TestingQueryHandlers.md` | 3 | 3 | 0 | 0 | 0 |
| **Total** | **20 pages** | **67** | **64** | **3** | **0** | **14** |

### Phase 4 — tranche 2a — 22 pages

| Section | Page | FAILED now | Reachable | Same-page | Hard | BUILT now |
|---|---|---:|---:|---:|---:|---:|
| Outbox and Inbox | `AzureBlobArchiveProvider.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `AzureBlobDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `DapperOutbox.md` | 1 | 0 | 0 | 1 | 1 |
| Outbox and Inbox | `DynamoInbox.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `DynamoOutbox.md` | 2 | 1 | 0 | 1 | 2 |
| Outbox and Inbox | `FirestoreDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `InMemoryInbox.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `InMemoryOutbox.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `MSSQLInbox.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `MSSQLOutbox.md` | 2 | 1 | 0 | 1 | 1 |
| Outbox and Inbox | `MongoDbDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `MsSqlDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `MySQLInbox.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `MySQLOutbox.md` | 3 | 2 | 0 | 1 | 0 |
| Outbox and Inbox | `MySqlDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `PostgresDistributedLock.md` | 2 | 1 | 0 | 1 | 0 |
| Outbox and Inbox | `PostgresInbox.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `PostgresOutbox.md` | 2 | 1 | 0 | 1 | 1 |
| Outbox and Inbox | `ReplayOnSeenReference.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `SqliteInbox.md` | 1 | 0 | 0 | 1 | 0 |
| Outbox and Inbox | `SqliteOutbox.md` | 3 | 2 | 0 | 1 | 0 |
| Outbox and Inbox | `UsingSweeperCircuitBreaking.md` | 4 | 3 | 0 | 1 | 1 |
| **Total** | **22 pages** | **40** | **18** | **0** | **22** | **6** |

### Phase 5 — tranche 2b — 16 pages

| Section | Page | FAILED now | Reachable | Same-page | Hard | BUILT now |
|---|---|---:|---:|---:|---:|---:|
| Commands, Handlers and Pipelines | `AgreementDispatcherRouting.md` | 11 | 10 | 0 | 1 | 0 |
| Commands, Handlers and Pipelines | `BuildingAPipeline.md` | 4 | 3 | 0 | 1 | 0 |
| Using an External Bus | `CloudEventsReference.md` | 4 | 3 | 0 | 1 | 0 |
| Using an External Bus | `S3LuggageStore.md` | 2 | 1 | 0 | 1 | 0 |
| Transports | `BrighterControlAPI.md` | 1 | 0 | 0 | 1 | 0 |
| Transports | `PostgreSQLBrokerTradeOffs.md` | 1 | 0 | 0 | 1 | 0 |
| Transports | `PostgreSQLMessageBroker.md` | 12 | 11 | 0 | 1 | 1 |
| Darker | `AggregationQueryPatterns.md` | 3 | 2 | 0 | 1 | 0 |
| Darker | `DarkerAndBrighterPipelines.md` | 1 | 0 | 0 | 1 | 0 |
| Darker | `DarkerConfigurationReference.md` | 4 | 3 | 0 | 1 | 0 |
| Darker | `ParameterizedQueryPatterns.md` | 4 | 1 | 2 | 1 | 2 |
| Darker | `ProjectionQueryPatterns.md` | 3 | 2 | 0 | 1 | 1 |
| Darker | `QueryHandlerDependencies.md` | 4 | 3 | 0 | 1 | 0 |
| Health Checks and Observability | `Telemetry.md` | 3 | 2 | 0 | 1 | 2 |
| Understanding Brighter | `CQRSUseCasesAndPatterns.md` | 2 | 1 | 0 | 1 | 0 |
| Understanding Brighter | `HowConfiguringTheDispatcherWorks.md` | 3 | 2 | 0 | 1 | 0 |
| **Total** | **16 pages** | **62** | **44** | **2** | **16** | **6** |

## Blocks that stay FAILED

*One row per block a tranche page leaves FAILED: page, ordinal, diagnostic, why it is not 017's.*

| Page | # | Diagnostic | Why it stays | Phase |
|---|---:|---|---|---:|
| `TutorialDurableOutbox.md` | 1 | `CS0246` `AddGreeting`, `GreetingsSender`; `GreetingEvent`, `Greetings` | same-page: block 2 declares `AddGreeting` in `namespace GreetingsSender`. The page says so at line 289 (*"`AddGreeting` and its handler arrive in step 5"*) | 2 |
| `TutorialDurableOutbox.md` | 3 | `CS0246` `AddGreeting`; `GreetingEvent`, `Greetings` | same-page: block 2's `AddGreeting`; line 478, *"all three projects build again"* | 2 |
| `TutorialFirstCommand.md` | 2 | `CS0246` `GreetingCommand` | same-page: block 1 declares it; step 3 opens *"Deriving from `RequestHandler<GreetingCommand>`"*, in the same project | 2 |
| `TutorialFirstCommand.md` | 3 | `CS0246` `GreetingCommand`, `HelloWorld` | same-page: block 1 declares both; step 4 names the handler step 3 wrote | 2 |
| `TutorialFirstMessage.md` | 2 | `CS0246` `GreetingEvent`, `Greetings` | same-page: block 1 declares both; line 132, `dotnet add GreetingsSender reference Greetings` | 2 |
| `TutorialFirstMessage.md` | 3 | `CS0246` `GreetingEvent`, `Greetings` | same-page; line 221, `dotnet add GreetingsReceiver reference Greetings` | 2 |
| `TutorialFirstMessage.md` | 4 | `CS0246` `GreetingEvent`, `Greetings` | same-page; line 221 | 2 |
| `BuildingAnAsyncPipeline.md` | 2 | `CS0246` `UseCommandSourcingAsync`, `GreetingCommand`, `IpFyApi` | same-page: block 3 declares the attribute. The three blocks compile together, with the two never-shown types stubbed, at **0** diagnostics | 2 |
| `BuildingAnAsyncPipeline.md` | 3 | `CS0246` `CommandSourcingHandlerAsync<>` | same-page: block 1 declares it | 2 |
| `ImplementingAHandler.md` | 2 | `CS0246` `GreetingCommand` | same-page: block 1; *"Then derive your handler from `RequestHandler<GreetingCommand>`"* | 2 |
| `ImplementingAsyncHandler.md` | 2 | `CS0246` `GreetingCommand`, `IpFyApi` | same-page: block 1; *"Then derive your handler from `RequestHandlerAsync<GreetingCommand>`"*. With `IpFyApi` stubbed, both blocks compile together at **0** diagnostics | 2 |
| `RequestValidation.md` | 3 | `CS0246` `RegisterUser` | same-page: block 2, *"2. Mark the handler"* after *"1. Declare the rules on the request"* | 2 |
| `RequestValidation.md` | 5 | `CS0246` `RegisterUser` | same-page: block 2, step 4 of the numbered Quick Start | 2 |
| `RequestValidation.md` | 7 | `CS0246` `GreetingCommandValidator` | same-page: block 6, *"Register the validator"* | 2 |
| `RequestValidation.md` | 10 | `CS0103` `OrderSpecification` | same-page: block 9, *"Register the specification"* | 2 |
| `RequestValidation.md` | 13 | `CS0246` `RegisterUser` | same-page: block 2; the section reuses the Quick Start's request | 2 |
| `RequestValidation.md` | 14 | `CS0246` `RegisterUser` | same-page: block 2 | 2 |
| `RequestValidation.md` | 16 | `CS0246` `MyRequestHandler<>` | same-page: block 15, *"2. Map the provider-agnostic handler to your implementation"* | 2 |
| `ReturningResultsFromAHandler.md` | 1 | `CS0246` `CreateTaskCommand`; `CS0103` `commandProcessor` | **unit rule 1**: the page tells the reader to write this type (*"add a property to the **Command** that you can initialize from the Handler"*), so it may not be stubbed | 2 |
| `HandlingLargeMessages.md` | 3 | `CS0246` `LargeOrderMessageMapper` | same-page: block 2 declares it; block 3 follows *"**Register the mapper**, or none of this runs"* | 2 |
| `DistributedLock.md` | 2 | `CS0234` `Paramore.Brighter.DynamoDb.V4`, `Locking.DynamoDB.V4`, `Outbox.DynamoDB.V4` | V4 package, not in the pin (D3, 018). The page recommends the V4 package, so the block carries its namespaces; it builds against the released V4 packages in scratch, **0** errors | 3 |
| `DynamoDbDistributedLock.md` | 1 | `CS0234` `Locking.DynamoDB.V4`; `CS0103` `dynamoDb` | V4 package, not in the pin (D3, 018). `dynamoDb` wants a value stub once V4 is pinned | 3 |
| `DynamoDbDistributedLock.md` | 2 | `CS0234` `Paramore.Brighter.DynamoDb.V4`, `Locking.DynamoDB.V4`, `Outbox.DynamoDB.V4` | V4 package, not in the pin (D3, 018); builds against the released V4 packages in scratch, **0** errors | 3 |
| `TickerQScheduler.md` | 2 | `CS0246` `Program` | instrument: `typeof(Program).Assembly` in a `Program.cs` with no declaration after its statements takes the `statements` wrapper, which declares no `Program`; Q2 (1.8) rules out a stub. Builds against the released packages in scratch, net9.0 and net10.0, **0** errors | 3 |
| `PaginationQueryPatterns.md` | 2 | `CS0246` `GetOrdersPageQuery`, `PagedResult<>`, `OrderDto`; `ApplicationDbContext` | same-page: block 1 declares the first three; block 2 is *"Handler with pagination:"*, straight after it | 3 |
| `PaginationQueryPatterns.md` | 3 | `CS0246` `OrderDto` | same-page: block 1 declares it | 3 |
| `PaginationQueryPatterns.md` | 4 | `CS0246` `GetOrdersCursorQuery`, `CursorPagedResult<>`, `OrderDto`; `ApplicationDbContext` | same-page: block 3 declares the first two, block 1 `OrderDto`; block 4 is *"Handler with cursor pagination:"*, straight after block 3 | 3 |
| `QueryPipelinePolicies.md` | 1 | `CS0246` `Program` | instrument: a `Program.cs` with no declaration after its statements takes the `statements` wrapper, which declares no `Program`; Q2 (1.8) rules out a stub. Builds as a `Program.cs` in scratch against Darker 4.1.1, **0** errors | 3 |

## Splits

*One row per split fence: page, old ordinal, new ordinals.*

| Page | Old # | New # | Why | Task |
|---|---:|---:|---|---:|
| `SchedulingAMessage.md` | 7, 8, 9 | 8, 9, 10 | not a split: a block inserted at #7, the #4414 workaround. All three were FAILED at `c7329bb` and are BUILT now, so the AC2 diff reads #7–#9 as `FAILED -> BUILT` and #10 as a new key | 3.4 |

## Blocks removed

*One row per block a phase deletes with the content it illustrated. The AC2 diff reads the
after-report's keys and cannot show these, so they are listed here.*

| Page | Old # | At `c7329bb` | Why | Task |
|---|---:|---|---|---:|
| `AnalyzerSupport.md` | 2 | BUILT | BRT006's fixed form; BRT006–008 ship in no release | 2.4 |
| `AnalyzerSupport.md` | 3 | FAILED | BRT007's warning case | 2.4 |
| `AnalyzerSupport.md` | 4 | FAILED | BRT007's fixed form | 2.4 |
| `AnalyzerSupport.md` | 5 | FAILED | BRT008's warning case | 2.4 |
| `AnalyzerSupport.md` | 6 | FAILED | BRT008's fixed form | 2.4 |
| `AnalyzerSupport.md` | 7 | BUILT | the `using` for the code fix's `Partitioner` | 2.4 |
| `AnalyzerSupport.md` | 8 | FAILED | the BRT007 pragma, rewritten as BRT001 in the new block 1 | 2.4 |

Old block 1 (BRT006's warning case, BUILT) also went; its address now holds the new pragma block,
BUILT, re-admitted at `ec38400`.

## Defect ledger

*One row per defect: defect, page, recurrence grep, before, after, found by.*

| Defect | Verified against 10.7.0 | Page | Recurrence grep | Before | After | Found by |
|---|---|---|---|---:|---:|---|
| `HandleAsync(T, CancellationToken? ct = null)` — V9's signature; V10 overrides `Task<TRequest> HandleAsync(TRequest command, CancellationToken cancellationToken = default)` | `RequestHandlerAsync.cs:119` | `ImplementingAsyncHandler.md` (also returned `Task`, not `Task<GreetingCommand>`; its prose said to *"default to null"*), `BuildingAnAsyncPipeline.md` | `grep -rn 'CancellationToken?' contents/` | **2** | **0** | 2.3, `--explain` after the page's `using`s |
| `public Guid Id { get; set; }` on a request — hides `IRequest.Id`, which is an `Id` | `IRequest.cs:47` | `ImplementingAsyncHandler.md` | `grep -rn 'public Guid Id\b' contents/` | **2** | **1** — the other is not this defect (next row) | 2.3, reading |
| `app.UseEndpoints(...)` on a `WebApplication` with no `app.UseRouting()` — throws `InvalidOperationException` at startup | run against 10.7.0 packages, net10.0; the control is the page's old block | `HealthChecks.md`, `BrighterControlAPI.md` | `grep -rn 'UseEndpoints' contents/` | **2** | **0** | 2.4, running the block |
| BRT006–BRT008 (Kafka partitioner analyzers, code fixes) documented as shipped — in no release; only on BrighterCommand/Brighter#4255, open. Also *"includes code fixes"*: 10.7.0 has no code-fix project | `git ls-tree 10.7.0 src/Paramore.Brighter.Analyzer/Analyzers/` → 3 analyzers, BRT001–005 | `AnalyzerSupport.md` | `grep -rln 'BRT00[678]\|code fix' contents/` | **1** | **0** — removed, maintainer's ruling (a) | 2.4, verifying the page's reference code |
| `IRequestContext` implemented with `Guid Id`, `ISpan Span`, `Dictionary<string, object> Bag`, `CustomHeaders` — at 10.7.0 the interface has no `Id` and no `CustomHeaders`, `Span` is an `Activity`, `Bag` a `ConcurrentDictionary` | `IRequestContext.cs` | `V10MigrationGuide.md:320` | — | **1** | **open — phase 5**, which holds that page for its E4 repair | 2.3, the row above's grep |
| `MapToMessage(TRequest request)` — V9's mapper signature; V10's takes a `Publication` | `IAmAMessageMapper.cs:33` | `Compression.md`, `Routing.md`, `KafkaConfiguration.md`, `MessageTransforms.md`, `ImplementingExternalBus.md`, `V10MigrationGuide.md`, `MessageMappers.md`, `OutboxArchiver.md`, `NullableReferenceTypes.md` (`string? topic = null`, found by the second method) | `grep -rnE 'MapToMessage\([A-Za-z<>]+ [a-z][A-Za-z]*(, string\? topic = null)?\)' contents/` | **14** lines, 10 pages | **2** — both skipped V9 forms | 2.5, `--explain` |
| A mapper class without `IRequestContext? Context { get; set; }` — `CS0535` | `IAmAMessageMapper.cs:31` | 11 pages | mapper blocks declaring `: IAmAMessageMapper<` with no `IRequestContext? Context` | **17** | **1** — the skipped V9 form | 2.5, `--explain` after the signature fix |
| `requeueCount: N` described as N requeues (*"Times a message is requeued"*, *"On the 4th failure"*) — it is N handlings, N−1 requeues; `0` behaves as `1` | `Message.cs:161`, `HandledCount >= requeueCount`; run, table in § *Phase 2 as executed* | 17 pages, including all 12 option tables | `grep -rnE 'requeued before it is treated\|exceed(s\|ed\|ing)? the requeue count\|Requeue up to 3\|Retry up to 3\|4th failure\|RequeueCount. is exceeded\|retries (remain\|exhausted)\|retry a message before\|number of requeue attempts\|requeue count exceeded\)\|When the count is exceeded' contents/` | **25** | **0** | 2.5, running |
| `requeueDelayInMilliseconds` — V9; V10's `Subscription` takes `TimeSpan? requeueDelay` | `Subscription.cs:115` | `BrighterSchedulerSupport.md`, `DispatcherConfigurationReference.md`, `HowServiceActivatorWorks.md`, `SchedulingAMessage.md` | `grep -rnE 'requeueDelayInMilliseconds\|RequeueDelayInMilliseconds' contents/` | **5** | **0** | 2.5, reading the requeue lines |
| DLQ enrichment keys given as one PascalCase set for every transport — Kafka writes `OriginalTopic`, `OriginalType`, …; six transports write camelCase; RMQ, ASB, GCP and InMemory write none but the pump's free-text `RejectionReason`; the bag is case-sensitive. Also RMQ and ASB listed as routing invalid messages, and both keys as on *"all"* subscriptions | `KafkaMessageConsumer.cs:1080`, `MsSqlMessageConsumer.cs:382` and five twins, `MessageHeader.cs:162`, `Reactor.cs:426`; `IUseBrighterInvalidMessageSupport` implementors | `ErrorHandlingOptions.md` | `grep -rnE '\| .OriginalMessageType. \|\|Both are constructor parameters available on all' contents/` | **2** | **0** | 2.5, verifying the table |
| Decompress does not recognise `WrapAsync`'s output — `IsCompressed` compares `ContentType.ToString()` with `application/gzip`, and `WrapAsync` adds `; charset=utf-8` | run; `CompressPayloadTransformer.cs:125`, `:301` — **upstream, BrighterCommand/Brighter#4432** | `Compression.md` states it and links the issue | — | **1** | **stated** — maintainer's ruling | 2.5, running |
| `UseExternalLuggageStore` resolves `IAmABrighterTracer` with `GetRequiredService` and only `AddBrighterInstrumentation()` registers one, so a claim check without tracing throws on first `Post`; the page's null-store exception appears only with a tracer | run; `ServiceCollectionExtensions.cs:1008` — **upstream, BrighterCommand/Brighter#4433** | `HandlingLargeMessages.md` step 3 and its failures list | — | **1** | **documented** — maintainer's ruling | 2.5, running the null-store claim's control |
| A Reactor subscription paired with an async handler — the pump stops with `ConfigurationException` (`InvalidCastException` inside), and the handler never runs; the page's own step 4 says so | run, control Proactor | `HandlingPoisonMessages.md` step 3 → `Proactor` | reading: the other four tranche pages have no handler beside their subscriptions | **1** | **0** | 2.5, running block 2's claim |
| A log excerpt quoting messages Brighter does not emit (*"Failed to process message … requeueing"*, *"Requeue count exceeded"*) | `Reactor.cs:601–670`, the templates; replaced with a captured run | `HandlingPoisonMessages.md` step 1 | `grep -rnE 'Requeue count exceeded for message\|Failed to process message' contents/` | **3** | **0** | 2.5, reading step 1 against the run's log |
| `new MessageBody(bytes, "JSON")` / `(s, MediaTypeNames.Application.Octet, …)` — a string where 10.7.0 takes a `ContentType?` | `CS1503` from `--explain` | `KafkaConfiguration.md` #20, `MessageMappers.md` #5 | — | **2** | **open — phases 3 and 5** | 2.5, `--explain` on the touched blocks |
| Mapper excerpts that omit a required member with no `// ...` (`CS0535` `MapToRequest` / `MapToMessage`) | `CS0535` | `Routing.md` #1, `V10MigrationGuide.md` #3, #18, `NullableReferenceTypes.md` #7, `FAQ.md` #7 | — | **5** | **open — their phases** | 2.5, `--explain` on the touched blocks |
| A `Publication` a reader posts through with no `RequestType` — `Post` throws `ConfigurationException` (*"No producer found for request type"*); BRT001 warns on it | `FindPublicationByPublicationTopicOrRequestType.cs:81`; run, control both ways | `InMemoryTransport.md`, `AWSSQSConfiguration.md`, `BrighterBasicConfiguration.md`, `CommandProcessorConfigurationReference.md`, `InMemoryOptions.md`, `KafkaConfiguration.md`, `RabbitMQConfiguration.md`, `V10MigrationGuide.md` | `python3 spec/017-compile_repairs/probe/pubscan.py` | **23** | **2** — `AnalyzerSupport.md`'s BRT001 suppression and `HandlingLargeMessages.md`'s `WrapAsync`, both right | 3.2, running the complete example |
| `InternalBus` said to have no backpressure and the in-memory transport no dead letter queue — `InternalBus(boundedCapacity)` blocks a sender when full; `DeadLetterRoutingKey` moves a rejected message to that topic | `InternalBus.cs:39`, `InMemoryMessageConsumer.cs:218`; run, control both ways | `InMemoryTransport.md` | `grep -rnE 'No backpressure\|No dead letter queues' contents/` | **2** | **0** | 3.2, reading the page against the source |
| `SqsPublication { SqsAttributes = … }` — the property is `QueueAttributes` (`CS0117`) | `SqsPublication.cs:73` | `AWSSQSConfiguration.md` | `grep -rnE '\bSqsAttributes\s*=' contents/` | **1** | **0** | 3.2, `--explain` after the block's `using`s |
| `CloudEventsType` as a `Publication` property — it is `Type` (`CS0117`) | `Publication.cs:96` | `V10MigrationGuide.md` (initializer and `publication.CloudEventsType`) | `grep -rnE '\bCloudEventsType\s*=\|\.CloudEventsType\b' contents/` | **2** | **0** | 3.2, `--explain` after the block's `using`s |
| `new InMemoryOutbox()` — V10's constructor takes a `TimeProvider` (`CS7036`) | `InMemoryOutbox.cs:91` | `InMemoryOptions.md`, `InMemoryOutbox.md` | `grep -rnE 'new InMemoryOutbox\(\)' contents/` | **2** | **0** | 3.2, `--explain` after the block's `using`s |
| `publication.SetConfigHook(…)` — the hook is on `KafkaProducerRegistryFactory`; the block also wrapped its publication as `new KafkaPublication() {publication}` and dropped a `;` | `KafkaProducerRegistryFactory.cs:87`; compiled, old form `CS1061` | `KafkaConfiguration.md` | `grep -rn 'publication\.SetConfigHook' contents/` | **1** | **0** | 3.2, reading the block beside its prose |
| A V4 package recommended with the V3 namespace, or the namespaces said to stay the same: every Brighter `.V4` package at 10.7.0 declares its types in `<V3 namespace>.V4` (`Locking.DynamoDb` → `Locking.DynamoDB.V4`), so a reader who installs the V4 package and copies a V3 `using` gets `CS0234` | `git ls-tree 10.7.0 src/*.V4`, each project's `namespace` lines; the lock blocks compiled against the V4 packages, control V3 `CS0234` | `AWSSQSMigrateToV10.md`, `AwsScheduler.md`, `DynamoInbox.md`; by reading `DynamoOutbox.md`, `DistributedLock.md`, `DynamoDbDistributedLock.md` | `python3 spec/017-compile_repairs/probe/v4scan.py` — blind to a page whose V4 package ID is also its namespace, which is why `DynamoOutbox.md` was read | **4** | **0** | 3.3, adding the lock blocks' `using`s |
| A value returned from `async Task` — `ScheduleNotification` returned `schedulerId` (`CS1997`) | `CS1997` from `--explain`; `PostAsync(TimeSpan, …)` returns `Task<string>`, `IAmACommandProcessor.cs:282` | `SchedulingAMessage.md` #3 | — | **1** | **0** | 3.4, `--explain` after the block's `using`s |
| `command with { … }` on a request — needs a record, and a Brighter `Command` is a class, which a record cannot derive from | `Command.cs:42` | `SchedulingAMessage.md` #5 | `grep -rnE '\bwith \{' contents/` | **1** | **0** | 3.4, stubbing the block's `OperationCommand` |
| `AutoFromAssemblies()` or `AsyncHandlersFromAssemblies` with a scheduler — the scheduler's handlers registered twice, so a scheduled request throws when it falls due, and ends the process with InMemory | run; registry read, control `HandlersFromAssemblies` — **upstream, BrighterCommand/Brighter#4414**, fixed by #4419 on `master`, unreleased | `SchedulingAMessage.md` states it with the workaround. Recorded, not repaired: `AwsScheduler.md`, `AzureScheduler.md`, `BrighterSchedulerSupport.md`, `HangfireScheduler.md`, `InMemoryOptions.md`, `InMemoryScheduler.md`, `QuartzScheduler.md`, `SwitchingSchedulers.md`, `V10MigrationGuide.md` | `grep -rl 'UseScheduler' contents/ \| xargs grep -lE 'AutoFromAssemblies\(\|AsyncHandlersFromAssemblies'` | **10** pages | **stated** on 1 — maintainer's ruling; 9 recorded | 3.4, running |
| InMemory cancel and reschedule said to work on a request scheduled through the command processor — each scheduled call gets a new `InMemoryScheduler`, so `CancelAsync` finds nothing and the request runs; `ReSchedulerAsync` returns `False` | run, control same instance; `CommandProcessor.cs:427`, `InMemoryScheduler.cs:57` — **upstream, BrighterCommand/Brighter#4437**, filed 3.4 | `SchedulingAMessage.md`, `FAQ.md`, `InMemoryScheduler.md` (cancel example, `Should_Cancel_Scheduled_Command`) | `grep -rl 'issues/4437' contents/` | **3** pages | **stated** on all 3 — maintainer's ruling | 3.4, running block 4's claim |
| Darker's default policies described as *"exponential backoff"* and a breaker that *"opens after consecutive failures"*, and as applying once registered. They retry 3 times after 50, 100 and 150 ms, the breaker opens on 1 failure for 500 ms, and neither runs without `[RetryableQuery]` | Darker 4.1.1 `QueryProcessorBuilderExtensions.cs:51`, `RetryableQueryDecorator.cs`; run, control without the attribute | `QueryPipelinePolicies.md` (the list and block 2's comment) | `grep -rnE 'Retries with exponential backoff\|Opens after consecutive failures\|Retry policy with exponential backoff' contents/` | **3** | **0** | 3.5, reading the page against Darker's source, then running |
| *"The ASP.NET model binder will validate these attributes before the query reaches your handler"*. Only a controller marked `[ApiController]`, or a minimal API after `AddValidation()` (.NET 10), rejects the query; elsewhere it reaches the code | run on net10.0, controls both ways | `QueryObjectValidation.md` | `grep -rn 'model binder will validate' contents/` | **1** | **0** | 3.5, running block 2's claim |
| `[RetryableQuery]`'s second argument described and used as a circuit-breaker name that adds a breaker to the retry. It is a policy name, and the decorator runs that one policy. `"DefaultCircuitBreaker"` is not registered by `AddDefaultPolicies()`, so it throws `ConfigurationException`; `circuitBreakerName:` is not a parameter (`CS1739`) | Darker 4.1.1 `RetryableQueryAttribute.cs:11`, `Constants.cs`; run, control `Constants.CircuitBreakerPolicyName`; compiled | `QueryPipeline.md` (4 lines, and the parameter list at line 239), `CQRSWithBrighterAndDarker.md` (2), `DarkerAndBrighterPipelines.md`, `ImplementAQueryHandler.md`, `QueryPatterns.md` | `grep -rnE 'RetryableQuery\(.*(DefaultCircuitBreaker\|circuitBreakerName)' contents/` | **9** lines, 5 pages | **open — phase 5**, maintainer's ruling | 3.5, reading Darker's source for the tranche's policy defaults |
| `.ConfigureServices(hostContext, services) =>` — the lambda's parameter list never opened, and its body never closed (`CS1519`, `CS1001`) | compiled, old form `CS1519` | `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`, `DynamoInbox.md`, `AzureBlobArchiveProvider.md`, `BrighterBasicConfiguration.md` ×2, `DispatcherConfigurationReference.md` ×5 | `grep -rn 'ConfigureServices(hostContext, services) =>' contents/` | **13** lines, 8 pages | **0** | 4.1, `--classify` |
| `opt.InboxConfiguration` inside `AddConsumers(options => …)` — `CS0103` | compiled | `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md` | `grep -rn '^\s*opt\.InboxConfiguration' contents/` — **4** before, **1** after, `DynamoInbox.md`'s, whose parameter is `opt` | **3** | **0** | 4.1, a scan of every lambda |
| `[UseInboxAsync]` on a handler class — `CS0592`; `RequestHandlerAttribute` is valid on methods only | `RequestHandlerAttribute.cs`, `AttributeUsage(AttributeTargets.Method)` | `InMemoryInbox.md` #2 | `grep -rn -A1 '^\s*\[UseInbox' contents/ \| grep -c class` | **1** | **0** | 4.1, `--explain` |
| `await _commandProcessor.Post(…, cancellationToken: …)` — `Post` returns `void` and takes no `cancellationToken`; the async form is `PostAsync` | `IAmACommandProcessor.cs:205`, `:241` | `InMemoryOutbox.md` #2 | `grep -rnE 'await [_a-zA-Z.]*\.(Post\|Send\|Publish\|DepositPost\|ClearOutbox)\(' contents/` | **1** | **0** | 4.2, `--explain` after the block's `using`s |
| `new AmazonDynamoDBConfig { ServiceURL = "…"; }` — a `;` inside an object initialiser | compiled | `DynamoInbox.md` #1 | `grep -rnP 'new [A-Za-z_.<>]+(\([^()]*\))? *\{[^{}]*;[^{}]*\}' contents/`, one line only; control: the old page → **1** | **1** | **0** | 4.1, `--classify` |
| The InMemory Inbox said to keep every entry until restart (*"No cleanup"*, *"All seen message IDs held in memory"*). An entry expires `EntryTimeToLive` (5 min) after it is written, removed by a scan at most every `ExpirationScanInterval` (10 min); past `EntryLimit` (2048) adding compacts the oldest to half | `InMemoryBox.cs:64–100`, `InMemoryInbox.cs:316`; run, controls both ways | `InMemoryInbox.md` | `grep -rnE 'No cleanup\|All seen message IDs held in memory' contents/` | **2** | **0** | 4.2, reading the page against the source, then running |
| A global `actionOnExists: Warn` shown beside a `[UseInboxAsync]` that sets no `onceOnlyAction` — the attribute's default `Throw` wins, so a duplicate throws `OnceOnlyException` | `PipelineBuilder.cs:371`, `HasExistingUseInboxAttributesInPipeline`; run, control the attribute with `Warn` | `InMemoryInbox.md` #1, #2 | pages with `actionOnExists: OnceOnlyAction.Warn\|Replay` and a `[UseInbox…]` without `onceOnlyAction` on its line: 2, read — `TurningOnReplayOnSeen.md`'s attributes set it on the next line and the page states the precedence | **1** | **0** | 4.2, running #1 with #2 |
| A global `InboxConfiguration` in `AddConsumers` reaches the pipeline only through `ExternalBus(…)`, so an application that never calls `AddProducers` gets no global Inbox, and duplicates run again | `ServiceCollectionExtensions.cs:640–660`; run, control with `AddProducers` — **upstream, BrighterCommand/Brighter#4335**, fixed by #4396 on `master`, unreleased | `BrighterInboxSupport.md` states it with the workaround; linked from `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`, `DynamoInbox.md`, `MongoDBInbox.md`, `FirestoreInbox.md`, `SpannerInbox.md`, `InMemoryInbox.md`. Not linked: the seven other pages that configure one | `git grep -l 'InboxConfiguration' d8633b1 -- contents` | **16** pages | **stated** on 1, linked from 9 — maintainer's ruling | 4.2, running `InMemoryInbox.md` #1 without #2's attribute |

## Friction ledger

*One row per place the workflow got in the way.*

## What the tasks review found — 2026-09-26

All nine gates read at `tools/README.md`'s figures; none exited 2. Five findings, all repaired
above.

| # | Said | Measured | Now |
|---:|---|---|---|
| **1** | the AC2 before-report is kept as `$TMPDIR/before.tsv` | `$TMPDIR` did not keep § 2's work directory between sessions 84 and 85 | regenerated at each use from `c7329bb`, checked against a recorded summary line (1.1, § *The AC2 before-report*) |
| **2** | `--classify` is red-proofed | its controls tested only the exit paths; none showed a block put in the right class. The exit-2 page, `Glossary.md`, has **0** C# blocks | a two-way classification control (a known *import* block, a known *page-type* block); the exit-2 page is `SpannerOutbox.md`, **2** blocks, all BUILT. `--types` and `--list-skips` get two-way controls too (`FAQ.md` carries **1** skip) |
| **3** | phase 1's tasks are ordered | no dependency was stated; 1.3's Q2 note relied on 1.8's pin | § Phase 1 *Order* |
| **4** | P0/P1 items are all carried | **P1-3** (`args` in the `statements` wrapper) was in no task | 1.10 names the blocks that need it, and the phase holding the first one carries it |
| **5** | § 2 re-derives counts | its agreeing rows re-ran the design's own probe, which is one method run twice | the second method is named: `--report`'s FAILED for the total; `--classify` at 1.10 for the tranche lists |

---

**Totals: 42 tasks** — phase 1 11, phase 2 6, phase 3 6, phase 4 5, phase 5 6, phase 6 8.
