# Spec 017: Compile Repairs — Tasks

**Created:** 2026-09-26
**Status:** **CLOSED 2026-10-02 — 42 of 42.** § *What 017 shipped* ends in the residual 018 starts from; acceptance is the maintainer's. Tasks approved 2026-09-26 (`.tasks-approved`), reviewed 2026-09-26, five findings, repaired
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
  | unwrap falls back to `DataRef` | bag entry removed → body restored | both absent → body stays `Claim Check {id}`, no throw; `DataRef` naming a missing claim → `ArgumentOutOfRangeException`; bag and `DataRef` disagree → the bag's (6.1) |
  | `HandlingLargeMessages.md` #4's three assertions | verbatim → pass | body under threshold → `StartsWithException` |
  | a missing store throws `NotImplementedException` | tracer registered, no store → *"This is a null store…"* | no tracer → the tracer's `InvalidOperationException` masks it; store + tracer → posts |
  | Decompress restores a Compress body | `PostAsync` → `application/gzip; charset=utf-8` on the bus, **not** decompressed (all three methods) | `Post` → `application/gzip`, round-trips; charset stripped → round-trips |
  | Compress honours its threshold | 153,600 B (150 KiB) → 811 B, `application/gzip`, `originalContentType` `application/json` (6.1) | body under 150 KB → uncompressed; Decompress on a plain body → unchanged |
  | `ErrorHandlingOptions.md` #4's commented values | verbatim → `orders.dlq`, `orders.invalid`, `dead-letter-orders` | topic `payments` → `payments.dlq`; template `dlq-{0}` → `dlq-orders`; `null` → `orders.dlq`, the default (6.1) |
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

- [x] **Task 4.3:** Repair the distributed-lock pages of the tranche
  - Input: the phase 4 rows whose page is a Distributed Lock; 4.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2
  - Notes: the lock pages share a shape; a defect found on one is grepped for on all before the
    next is opened.

- [x] **Task 4.4:** Repair the remaining *Outbox and Inbox* tranche pages
  - Input: the phase 4 rows not covered by 4.2 or 4.3 (at § 2's pin: `UsingSweeperCircuitBreaking.md`,
    `AzureBlobArchiveProvider.md`, `ReplayOnSeenReference.md`); 4.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2
  - Notes: `ReplayOnSeenReference.md#1` uses API not in 10.7.0 (requirements P2-2); it stays FAILED
    and is listed with that reason, not repaired toward `master`.

- [x] **Task 4.5:** Close phase 4 — checks, figures, PR
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
  `PostgresOutbox.md`, `SqliteOutbox.md`) repeat one block shape, and the six `*DistributedLock.md`
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

**Task 4.3 — the distributed-lock pages.** Six pages. **BUILT 214 → 226** (+12): #1 and #2 on each
of `AzureBlobDistributedLock.md`, `FirestoreDistributedLock.md`, `MongoDbDistributedLock.md`,
`MsSqlDistributedLock.md`, `MySqlDistributedLock.md` and `PostgresDistributedLock.md`. **None stays
FAILED.** `pagelint` **636 → 624**, two on each page (per page, against a worktree at `7d04918`).
Pages with nothing BUILT **64 → 58**, all six; requirements' `awk` and a Python join agree. **The
≤ 60 target is met**: phase 4 landed all ten of its pages with nothing BUILT and a reachable block,
and five of its six hard-only pages.

- **Said at 4.1:** *"the five `*DistributedLock.md` pages"* share a shape. **Measured:** the phase 4
  table holds six, and all six share it. Rewritten above
- **One repair, tried on all six before any page was opened alone.** Block 1 takes its `using`s.
  Block 2 takes them too, and its `opt.Outbox = /* your … Outbox */;` becomes `opt.Outbox = outbox;
  // your … Outbox` — the design's placeholder rule, completing the statement it sat in
- **One unit.** `DistributedLockProviderContext.cs` supplies `services` and `outbox`, typed
  `IAmAnOutbox` as `ProducersConfiguration.Outbox` is (`ProducersConfiguration.cs:212`). Each page
  configures its Outbox on the Outbox's own page and tells the reader to write neither (rule 1, by
  reading). `--report` → *"35 units checked, 0 violations"*
- **No defect.** Every page's prose was read against 10.7.0: `sp_getapplock` in `Exclusive` mode at
  `Session` scope with a zero timeout (`MsSqlLockingQueries.cs`); `GET_LOCK` with a one-second
  timeout and a SHA-512 name truncated to 160 bits (`MySqlLockingProvider.cs:170`);
  `pg_try_advisory_lock`; a MongoDB insert on `_id` with the duplicate key refused and a TTL index
  from `Locking.TimeToLive` (`BaseMongoDb.cs:111`); a Firestore create with `Exists = false`; an
  Azure blob uploaded if absent and leased for `LeaseValidity`, the container never created.
  `optioncheck` reads the two tables here, unchanged
- **Behaviour, run with controls** against released 10.7.0 packages and real servers in Docker
  (`postgres:16`, `mysql:8`, `azure-sql-edge`), net10.0, one process per case. Each provider is
  built as its page's block 1 builds it; a "crash" is the holder's server session killed without a
  release:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `PostgresDistributedLock.md`: released when the session closes, *"including if the holding instance crashes"* | A obtains; B refused; A's backend terminated → a new instance **obtains** | A alive → a new instance **refused** |
  | `MySqlDistributedLock.md`: the same | A obtains; B refused; A's connection killed → **obtains** | A alive → **refused** |
  | `MsSqlDistributedLock.md`: the same | A obtains; B refused; A's session killed → **obtains** | A alive → **refused** |

  Read, not run: the MongoDB, Firestore and Azure Blob providers, whose recovery is a TTL or a
  lease the pages already describe as bounded by it, and which need their emulators
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 12 rows at `b8f21ac`. `--report` → exit **0**, *"983 blocks: 226 BUILT, 741 FAILED,
  16 SKIPPED"*, baseline 226, 0 findings. Joined on page and ordinal against 4.2's report, all 12
  blocks that moved went `FAILED -> BUILT`, and there are no new keys
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; shape, redirects and `--verify` unmoved;
  `pagelint --changed origin/master` 0 errors. **Pages changed: 6** (`git diff --name-only
  7d04918..HEAD -- contents`), all on the tranche

**Task 4.4 — the remaining outbox pages.** Three pages, and one off the tranche by ruling. **BUILT
226 → 234** (+8): `AzureBlobArchiveProvider.md` #1, `UsingSweeperCircuitBreaking.md` #2–#5 and
`SweeperCircuitBreaking.md` #2, #5, #7. **One stays FAILED**,
`ReplayOnSeenReference.md` #1, by P2-2 (§ *Blocks that stay FAILED*). `pagelint` **624 → 616**, the
four `UsingSweeperCircuitBreaking.md` blocks that opened with `// ...` and four
`SweeperCircuitBreaking.md` blocks (#2, #5, #6, #7). Pages with nothing BUILT
**58 → 57**, `AzureBlobArchiveProvider.md`; `ReplayOnSeenReference.md` stays among them.

- **Said at 4.1:** a ceiling of **230** BUILT. **Measured: 234.** The ceiling left out 4.2's two
  off-tranche recurrence blocks (`BrighterBasicConfiguration.md` #3, #4) and the three
  `SweeperCircuitBreaking.md` blocks repaired by ruling, and counted `TickerQScheduler.md` #2, which
  stays FAILED on `Program`: 230 + 2 + 3 − 1 = 234
- **`UsingSweeperCircuitBreaking.md`.** #2, #3 take their `using`s, and the page's unit gains
  `services`, which block 1 takes as a parameter. #4, read with its `using`s, named a
  `CircuitBreakerState` no package or block declares, and shared a `Dictionary` between `TripTopic`
  and `CoolDown`; it now declares the state as a nested record and uses a `ConcurrentDictionary`
  with a conditional remove. #5 was the fragment 4.1 read, and **made whole** (design: the reader
  needs the whole — `TrippedTopics` is the part a distributed breaker has to get right). Its
  `IDistributedCache` cannot enumerate keys, so a whole version on it could not answer
  `TrippedTopics`; it is now a Redis sorted set, scored by expiry. A sixth block registering it was
  written and removed, a new *same-page* FAILED block for a registration #1 already shows; a
  sentence links #1 instead
- **`AzureBlobArchiveProvider.md`.** **Said at 4.1:** five defects behind the placeholders.
  **Measured, compiled with the placeholders filled** (the control below): the six 4.1's row names,
  and two more — `AzCliCredential` is no type in Azure.Identity (`AzureCliCredential`), and
  `BlobContainerUri` is a `Uri`. The block is rewritten against 10.7.0. The page gains an opening
  sentence after its banner, with the `description:` rule 7 checks against it, its Prerequisites, the two
  packages the provider does not bring in (`dotnet list package --include-transitive` on
  `Paramore.Brighter.Archive.Azure` 10.7.0 alone: neither `Azure.Identity` nor
  `Paramore.Brighter.Outbox.Hosting`), and a table of `AzureBlobArchiveProviderOptions`, read from
  `AzureBlobArchiveProviderOptions.cs`. **It is not `optioncheck`-marked:** marked, the tool reported
  `CANNOT CONSTRUCT` (it cannot synthesise the `AccessTier` constructor argument) and `ROW NAMES
  NOTHING` for `TagsFunc` and `StorageLocationFunc`, which are fields — a marker would check nothing
- **Behaviour, run with controls** against released 10.7.0 packages, net10.0, one process per case;
  Redis in Docker (`redis:7`):

  | Claim | Case → result | Control → result |
  |---|---|---|
  | #1: *"default cooldown of 10 sweeps"*; #2: *"Recover after 3 sweeps"*, *"30 sweeps"* — each sweep calls `CoolDown` and then reads `TrippedTopics` (`OutboxProducerMediator.cs:721`, `:735`) | `CooldownCount = 3` → skipped **3** sweeps, retried on the 4th | default → **10**; `30` → **30** |
  | #4 needs a concurrent map | the old block (its `CircuitBreakerState` written as it uses it), `TripTopic` and `CoolDown` on two threads for 3 s → **`InvalidOperationException`**, *"Collection was modified"*, twice in two runs | the new block → **none**, twice |
  | #5 shares trips across instances and expires them | two breakers on two connections, cooldown 2 s: A trips → A and B both list `orders`; +1 s, B cools down → both still list it | +2.5 s, B cools down → both empty, **0** members left |
  | `AzureBlobArchiveProvider.md` #1 configures the Archiver it says | the block in a host, built → `TimerInterval 5`, `ArchiveBatchSize 500`, `MinimumAge 31.00:00:00`, provider `AzureBlobArchiveProvider` | the old block, placeholders filled → `CS1003`; with `New …;` also mended, `CS0029`, `CS0103` ×3, `CS0246`, `CS7036` |

  Read, not run: #3's *"all topics always attempted"* (`_outboxCircuitBreaker?.TrippedTopics` is
  `null` with none registered, `OutboxProducerMediator.cs:735`); the Azure provider's writes — one
  blob per message named by its Id, the body only, an existing blob not rewritten, the container
  never created (`AzureBlobArchiveProvider.cs`), which need Azurite and a token credential it accepts
- **Off the tranche, repaired — maintainer's ruling, 2026-09-27: *"fix it in this PR"*.**
  `SweeperCircuitBreaking.md` #2 and #7 configured the sweeper through `options.OutboxSweeper = new
  OutboxSweeperOptions { SweepInterval = … }` — no such property or type at 10.7.0 (`git grep` → 0);
  `UseOutboxSweeper` takes a `TimedOutboxSweeperOptions`, whose interval is `TimerInterval`, an `int`
  of seconds (`TimedOutboxSweeper.cs`, a `Timer` of that period). Both blocks now configure it there,
  with their `using`s, and **build** (`FAILED -> BUILT`, baselined at `28b2d5f`). **Said in the first
  draft of this entry:** #2 and #5. **Measured:** #2 and #7; #5 is the MongoDB block
- **The same repair found the formula one sweep short.** The page said `Cooldown Time = CooldownCount ×
  SweepInterval` and, in its steps, that a topic recovers *"when the cooldown reaches zero"*.
  `CoolDown` runs first in each sweep (the sweeper is its only caller, `OutboxSweeper.cs:79`) and
  removes a topic when its count goes **below** zero, so a topic sits out `CooldownCount` sweeps and is
  retried on the next: `(CooldownCount + 1) × TimerInterval`. Rewritten there, in the page's steps,
  and in `UsingSweeperCircuitBreaking.md` #2's two comments (*"Recover after 3 sweeps"*). **Run**
  end to end, released 10.7.0, net10.0: a real `UseOutboxSweeper` host, `TimerInterval = 1`, an
  InMemory Outbox and a producer that always throws. `CooldownCount = 2` → sends every **3 s**; `3` →
  every **4 s**. Controls: no breaker → every **1 s**; `CooldownCount = 0` → every **1 s**. Each sweep
  made 4 send attempts, Brighter's own send retry
- **Then the page's two other falsehoods — maintainer's ruling, 2026-09-27: *"put them in this
  PR"*.** Read against 10.7.0 and run:
  - **Which Outboxes honour a trip.** #5 called `.UseMongoDbOutbox(…)` (`git grep` → 0) under
    *"fully integrated with MongoDB Outbox"*, and the page said breaking *"works with all Brighter
    Outbox implementations"*. The sweeper passes `TrippedTopics` to `OutstandingMessagesAsync`
    (`OutboxProducerMediator.cs:735`) and each Outbox filters, or does not. **Run** against released
    10.7.0 packages, net10.0, two messages (`orders`, `payments`), `OutstandingMessagesAsync` with
    `orders` tripped and, as each store's own control, with none: **SQLite** → `[payments]`;
    **MongoDB** (`mongo:7`) → `[payments]`; **DynamoDB** (`amazon/dynamodb-local`) → `[orders,payments]`;
    **Spanner** (the emulator) → `[orders,payments]`; every control → both. Read: DynamoDB V3 and V4
    take the parameter and never use it (`DynamoDbOutbox.cs:582`); Spanner's `PagedOutstandingCommand`
    has no `{1}` for the `NOT IN` clause `RelationDatabaseOutbox` formats into it
    (`SpannerQueries.cs:12`); MSSQL, MySQL and PostgreSQL carry the `{1}` and share SQLite's code;
    Firestore and InMemory filter (`FirestoreOutbox.cs:901`, `InMemoryOutbox.cs:566`, and the
    InMemory sweeper runs above). The section is now *Sweeper Circuit Breaking Outbox Support*, a
    table of that, and #5 registers a MongoDB Outbox as `MongoDBOutbox.md` does — **it builds**.
    **Upstream, filed 2026-09-28 by the maintainer's ruling, `Bug` and `0 - Backlog`:**
    BrighterCommand/Brighter#4443 (DynamoDB) and #4444 (Spanner), both still so on `master`
    `bb10b8fae`; the page's table links each
  - **Explicit clearing.** § 6 said explicit clearing is *"NOT subject to circuit breaking"*; § *Bulk
    Dispatch Support* said `ClearOutboxAsync` *"respects circuit breaker state"*, and that *"failed
    batches can be retried individually per topic"*. **Run**, an InMemory Outbox, a producer that
    always throws: a topic tripped beforehand → `ClearOutbox` and `ClearOutboxAsync` each still make
    **4** send attempts; a fresh breaker → `ClearOutboxAsync` leaves `orders` **tripped**,
    `ClearOutbox` leaves **none** (`DispatchAsync` trips on `!sent`, `Dispatch` only through a
    publish-confirmation callback, `:984`). Neither section was right. § 6 now says both halves; §
    *Bulk Dispatch Support* shows the sweeper's `UseBulk`, and says what it does — **run**: a bulk
    sweeper, `TimerInterval = 1`, a batch producer that throws, `CooldownCount = 2` → batches every
    **3 s**, 20 of 20 attempts through `SendAsync(IAmAMessageBatch)`; control, no breaker → every
    **1 s**. The *"retried individually"* claim has nothing behind it in the source and is gone
  - The page's step list said messages are *"grouped by topic"* on every sweep; only bulk groups.
    Step 2 now says the tripped topics are passed to the Outbox. A troubleshooting item names the
    two Outboxes that ignore them
  - #6 no longer names `cancellationToken`, so the unit loses it (the unit rule's violation, read
    from `--report`), and the page's five baselined rows are re-admitted at `a61893b` with #5
- **Recurrence greps, all 0 beyond the repaired lines:** `AzCliCredential`, `BlobContainerUri *= *"`,
  `new AzureBlobArchiveProviderOptions()`, `MinimumAge *= *[0-9]`, `BatchSize` within eight lines of
  `UseOutboxArchiver`, an unshown `CircuitBreakerState`, `IDistributedCache` in code; the page's two
  breakers are the only `: IAmAnOutboxCircuitBreaker` in `contents/`
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 5 rows at `2defac6`; `SweeperCircuitBreaking.md` #2, #7 at `28b2d5f`, then #5 and
  the page's other four rows re-admitted at `a61893b`. `--report` → exit **0**, *"983 blocks: 234
  BUILT, 733 FAILED, 16 SKIPPED"*, baseline 234, 35 units, 0 violations, 0 findings. Joined on page
  and ordinal against the report at `05fdeaf`, the 8 blocks that moved went `FAILED -> BUILT`, 983
  keys both sides
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings;
  `optioncheck` 0 mismatches across 59 tables, 519 rows; no `SUMMARY.md` change, so shape, redirects
  and `--verify` unmoved; `pagelint --changed origin/master` 0 errors. **Pages changed: 3**
  (`git diff --name-only 05fdeaf..HEAD -- contents`): the two tranche pages and
  `SweeperCircuitBreaking.md`


**Task 4.5 — phase 4 closed, 2026-09-28, branch at `ca6a0b2`.** Every gate run bare, exit code
read before its output:

| # | Gate | Exit | Read | Predicted (4.1) | Agrees? |
|---:|---|---:|---|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | none | **yes** |
| 2 | `pagelint` | 0 | 0 errors, **616** warnings across 76 pages, 162 pages | 0 errors; 658 → between 640 and 622, recurrences explained | **no — 6 lower**, explained below |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | none | **yes** |
| 4 | redirects | 0 | 77 entries, 7858 bytes | none | **yes** |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | none, scope held at 18 across 5 | **yes** |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | none | **yes** |
| 7 | `--verify` | 0 | 161 predicted = 161 published | none | **yes** |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced | none | **yes** |
| 9 | `blockcheck` | 0 | 983: **234** BUILT, 733 FAILED, 16 SKIPPED; 0 findings; **542** reference assemblies; baseline 234; **35** units, 0 violations; 53 pages mapped | BUILT 204–230; SKIPPED 16; exit 0; 0 violations | **no — 4 above the ceiling**, explained below |
| — | `attr_mismatch.py` | 1 | **7** | held at 7 | **yes** |

`pagelint --changed origin/master` → exit **0**, 0 errors.

**Gate 9, reconciled.** Phase 4 moved **45** blocks: 25 in 4.2, 12 in 4.3, 8 in 4.4. Against 4.1's
ceiling of 230: `BrighterBasicConfiguration.md` #3, #4 (+2, the unopened-lambda recurrence) and
`SweeperCircuitBreaking.md` #2, #5, #7 (+3, the maintainer's ruling) lie off the tranche, and
`TickerQScheduler.md` #2, counted in the ceiling, stays FAILED on `Program` (−1): 230 + 2 + 3 − 1 =
**234**. The pin carries **98** `PackageReference`s (`grep -c`), 95 at phase 3's close.

**Every FAILED block on the 22 tranche pages is listed**: `after.tsv`'s FAILED keys on those pages
are **1**, `ReplayOnSeenReference.md` #1, and § *Blocks that stay FAILED* holds it (P2-2).

**Gate 2, reconciled.** Per-page warnings at `d8633b1` (a worktree) against the branch: the 22
tranche pages fell by **36**, which is 4.1's floor of 622 exactly — every reachable and every hard
block given its `using`s. The further **−6** are on two pages outside the tranche:
`SweeperCircuitBreaking.md` −4 (the ruling) and `BrighterBasicConfiguration.md` −2 (the
recurrence). `DispatcherConfigurationReference.md`, also touched by the recurrence, moved nothing:
its blocks open with `// ...`. 658 − 36 − 6 = **616**, across 76 pages, down from 96.

**AC2, against a `before.tsv` regenerated from `c7329bb` in a worktree** (exit 0, *"989 blocks: 101
BUILT, 872 FAILED, 16 SKIPPED"*, 989 rows): the diff prints **135** lines. **134** are `FAILED ->
BUILT` — 89 through phase 3 and phase 4's 45. The 135th is ` -> BUILT contents/SchedulingAMessage.md
10`, the key § *Splits* explains. No `-> SKIPPED`. **Control, both ways:** the report against itself
prints **0** lines; a copy with `SweeperCircuitBreaking.md` #8 set FAILED prints exactly one line
beside the known key that is not `FAILED -> BUILT`, `BUILT -> FAILED contents/SweeperCircuitBreaking.md 8`.

**The ≤ 60 target: met, at 57.** Pages with nothing BUILT, by requirements' `awk` (`comm -23`) and a
Python join over the same report: **57** both. 4.1 said as low as 58; the 58th off the list is
`BrighterBasicConfiguration.md`, off the tranche. Phase 4 landed all ten of its pages with nothing
BUILT and a reachable block, and five of its six hard-only pages; `ReplayOnSeenReference.md` waits
on the pin. Phase 5 now has headroom, not a quota.

**Every behavioural block was run with its control** (P0-10): the tables under 4.2, 4.3 and 4.4,
against released 10.7.0 packages, with real servers or emulators where a claim needed one.

**Upstream:** BrighterCommand/Brighter#4335 stated on `BrighterInboxSupport.md` and linked from nine
inbox pages (ruling); #4443 (DynamoDB) and #4444 (Spanner) filed in 4.4, `Bug`, `0 - Backlog`,
linked from `SweeperCircuitBreaking.md`.

**The PR changes 28 pages** (`git diff --name-only origin/master..HEAD -- contents | wc -l`): **21**
of the 22 tranche pages (`ReplayOnSeenReference.md` untouched) and **7** outside it —
`BrighterBasicConfiguration.md` and `DispatcherConfigurationReference.md` by recurrence;
`BrighterInboxSupport.md`, `FirestoreInbox.md`, `MongoDBInbox.md` and `SpannerInbox.md` by the #4335
ruling; `SweeperCircuitBreaking.md` by ruling. **Said in session notes: 29** (20 + 6 + 3), which
counted `AzureBlobArchiveProvider.md` in both 4.2 and 4.4. Beside them: `tools/README.md` (rows 2 and
9, and a phase 4 paragraph), the baseline, `refs.csproj`, `pages.tsv` (36 → 53 pages mapped), 5 new
units and 4 changed, and this file.

---

## Phase 5 — Tranche 2b and the recorded falsehoods *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 1-hard-block page outside *Outbox and Inbox* leaves whole, and P0-7 is done
everywhere. Page list: § *The tranches*, phase 5 table.

- [x] **Task 5.1:** Predict phase 5's movement
  - Input: § *The tranches*, phase 5 table; § *Phase 4 as executed*; `attr_mismatch.py`'s count now
  - Output: § *Phase 5 as executed* opens with a prediction per gate and a verdict per hard block,
    as 4.1; and the P0-7 predictions — `ITimerProvider` 4 → 0, `attr_mismatch` to exactly the
    deliberate `PipelineValidation.md:250`

- [x] **Task 5.2:** Repair the tranche pages in *Commands, Handlers and Pipelines*, *Darker* and *Understanding Brighter*
  - Input: those sections' phase 5 rows; 5.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2

- [x] **Task 5.3:** Repair the tranche pages in *Transports*, *Using an External Bus* and *Health Checks and Observability*
  - Input: those sections' phase 5 rows; 5.1's verdicts
  - Output: each page whole; baseline rows; ledger rows as 4.2

- [x] **Task 5.4:** Repair `ITimerProvider` and the unshown `Order`
  - Input: `grep -rn ITimerProvider contents/` (4 lines at `c7329bb`, all `InMemoryScheduler.md`);
    `design.md` § *API Resolved* (`InMemorySchedulerFactory.TimeProvider`);
    `CQRSWithBrighterAndDarker.md`'s `Id = command.Id` block
  - Output: `grep -rn 'ITimerProvider' contents/ | wc -l` → **0**, read from a file; the `Order`
    block BUILT or showing `Order`; both in § *Defect ledger*
  - Notes: a sentence claiming how the scheduler uses time is run, with a control (P0-10).

- [x] **Task 5.5:** Repair the E4 attribute mismatches not already repaired
  - Input: `probe/attr_mismatch.py` output now; `design.md` § *E4* (verdict per hit) and
    § *API Resolved*
  - Output: `attr_mismatch.py > out; echo $?` → **1** with exactly `PipelineValidation.md:250` in
    `out`; `attr_mismatch.py --plant` → **0**; each repaired hit in § *Defect ledger*
  - Notes: `PipelineValidation.md:280` sits in a *Before (warning)* example about step order — the
    repair keeps what that example demonstrates and changes only the attribute's kind; `:289` also
    names an argument before a positional one. A page that shows a mismatch deliberately says it is
    wrong in prose, not only in a `// wrong` comment.

- [x] **Task 5.6:** Close phase 5 — checks, figures, PR
  - Input: 5.1's prediction; the PR's diff
  - Output: as 2.6, for phase 5; plus BUILT against **≥ 250** and pages with nothing BUILT against
    **≤ 60**, both read before phase 6 walks them

### Phase 5 as executed

**Prediction, 2026-09-28, from `master` `976e0e0`**, the merge of phase 4 (PR #193). The *Now*
column is this task's own run at `976e0e0`, every gate bare, exit code read before its output; all
nine exit 0 and read at `tools/README.md`'s figures. Each prediction names its mechanism:

| # | Gate | Now | Predicted after phase 5, and why |
|---:|---|---|---|
| 1 | `linkcheck` | 165 files, 0 broken | **none**. No file is added |
| 2 | `pagelint` | 0 errors, 616 warnings, 162 pages | **errors 0; warnings 616 → between 582 and 573, then lower by P0-7 and recurrences, each explained.** On the 16 pages, rule 6 warns on **45** blocks: **43** FAILED (**34** reachable, **9** hard) and **2** BUILT (`PostgreSQLMessageBroker.md` #11, `ProjectionQueryPatterns.md` #4), which no repair touches. Every reachable block given its `using`s: −34, to 582; each warning hard block repaired: −1 more, to 573 if all 9 are. The P0-7 blocks warn too, all but `CQRSWithBrighterAndDarker.md` #7: `InMemoryScheduler.md` #5, `HowServiceActivatorWorks.md` #16, `PipelineValidation.md` #9, #10, `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6, `V10MigrationGuide.md` #10, #12 — **up to −8** more, and none from a block that marks its omission `// ...` instead |
| 3, 4, 7 | shape, redirects, `--verify` | 161 / 77 / 161 | **none**. No `SUMMARY.md` change |
| 5 | `versioncheck` | 0 stale of **18, across 5 pages** | **none, scope held.** Its five pages (`tools/versioncheck.py:81`–`85`) are the tutorials and `GetStarted.md`, none in phase 5 |
| 6 | `optioncheck` | 0 mismatches, 59 tables, 519 rows | **none.** Of the tranche, the P0-7 pages and the recurrence pages (27 files), three carry a table it reads: `PostgreSQLMessageBroker.md` (3 tables, 28 rows), `InMemoryScheduler.md` (1, 4) and `QuartzScheduler.md` (1, 4) — 5 tables, 36 rows. `InMemoryScheduler.md`'s table already documents `InMemorySchedulerFactory.TimeProvider`, which is what the `ITimerProvider` repair points at. A row changes only if a repair finds a default wrong, and is then a defect in § *Defect ledger* |
| 8 | `symbolcheck` | 0 findings, 22 entries, 161 pages, 3 silenced | **none.** The 27 files read 0 findings with **2** of the 3 silenced sites, both `S3LuggageStore.md` (`AddS3LuggageStore`, `S3LuggageStoreCreation`); its repair keeps both opt-outs where they are |
| 9 | `blockcheck` | 983: 234 BUILT, 733 FAILED, 16 SKIPPED; 542 reference assemblies; 35 units; 53 pages mapped | **BUILT 234 → at least 246, at most 294; SKIPPED 16 plus accepted reasons only; reference assemblies 542, no pin change; exit 0; units 35 plus the new ones, 0 violations.** Mechanism below |
| — | `attr_mismatch.py` | **7**, exit 1 | **7 → 1, exit 1, the one hit `PipelineValidation.md:250`** — the deliberate *Before (error)* example. The other six are 5.5's repairs. `:250` holds its line only if nothing above it on that page changes length; 5.5's repairs at `:280` and `:289` are below it |
| — | `grep -rn ITimerProvider contents/` | **4** lines, all `InMemoryScheduler.md` (`:32`, `:50`, `:202`, `:205`) | **4 → 0.** Two sentences, the pipeline diagram and block #5 (`FakeTimerProvider : ITimerProvider`) |

**Gate 9, by source.** The probe re-run at `976e0e0` (§ *The tranches*' recipe, `Order` excluded)
reads the phase 5 table with **one page moved**: **62 FAILED, 45 reachable, 2 same-page, 15 hard, 6
BUILT**. **Said at 1.10:** `BrighterControlAPI.md` #1 hard, 0 reachable, 16 hard. **Measured:**
#1 reachable by an empty stub (`app`), 15 hard — task 2.4 repaired its `UseEndpoints` defect off
the tranche (`51b5fef`, § *Defect ledger*), which was the block's hard part. **Second method:**
`--classify` puts the same **62** FAILED blocks on the 16 pages. The 45 reachable are:

- **1** with a `using` alone — `BuildingAPipeline.md` #3
- **11** with an empty stub — `BrighterControlAPI.md` #1 (`app`), `CloudEventsReference.md` #1, #3,
  #4, `DarkerConfigurationReference.md` #1, #3, `HowConfiguringTheDispatcherWorks.md` #3,
  `ParameterizedQueryPatterns.md` #1, `PostgreSQLMessageBroker.md` #9, `S3LuggageStore.md` #2,
  `Telemetry.md` #3
- **18** needing a stub with members — `AggregationQueryPatterns.md` #2, #3,
  `BuildingAPipeline.md` #2, #4, `CQRSUseCasesAndPatterns.md` #1,
  `HowConfiguringTheDispatcherWorks.md` #2, `PostgreSQLMessageBroker.md` #2, #4, #5, #6, #7, #10,
  #13, `ProjectionQueryPatterns.md` #1, #2, `QueryHandlerDependencies.md` #1, #4, `Telemetry.md` #2
- **15** needing a typed value — `AgreementDispatcherRouting.md` #1–#7, #9–#11,
  `DarkerConfigurationReference.md` #2, `PostgreSQLMessageBroker.md` #1, #3, #8,
  `QueryHandlerDependencies.md` #3

The 2 same-page stay FAILED by rule: `ParameterizedQueryPatterns.md` #2, #6.

- **Floor 246** = 234 + 1 + 11: only the `using` and empty-stub blocks
- **Ceiling 294** = 234 + 45 + 14 + 1: every reachable block; every hard block but
  `S3LuggageStore.md` #1 (below); and `InMemoryScheduler.md` #5, the `ITimerProvider` block, once
  rewritten. **`CQRSWithBrighterAndDarker.md` #7, the `Order` block, is not in it**: the probe reads
  it SAME-PAGE on `PlaceOrderCommand`, so 5.4's output there is *"showing `Order`"*, not BUILT.
  Each hard block listed rather than repaired, and each defect a stub surfaces, lowers it by one
- **The ≥ 250 target is not met by the floor.** 246 is 4 short, so phase 5 must land **at least 4**
  of the 33 member, typed-value and hard blocks. The E4 blocks are no help: each is FAILED for
  reasons besides its attribute, and 5.5 changes only the attribute
- **The pin: no change predicted.** Every name the hard blocks need past the page's own types
  resolves at 542 (`--explain` on the probe's `stageP`), except `Paramore.Brighter.Transformers.AWS.V4`
- **Pages with nothing BUILT: 57 → at most 51, as low as 45** (requirements' `awk` and a Python join
  over the same report: **57** both). 12 phase 5 pages have no BUILT block. **6** reach one by a
  `using` or an empty stub (`BrighterControlAPI.md`, `BuildingAPipeline.md`,
  `CloudEventsReference.md`, `DarkerConfigurationReference.md`,
  `HowConfiguringTheDispatcherWorks.md`, `S3LuggageStore.md` by #2); **4** by a stub with members or
  a typed value (`AgreementDispatcherRouting.md`, `AggregationQueryPatterns.md`,
  `CQRSUseCasesAndPatterns.md`, `QueryHandlerDependencies.md`); **2** only by repairing their one
  hard block (`DarkerAndBrighterPipelines.md`, `PostgreSQLBrokerTradeOffs.md`). Seven pages P0-7 or a
  recurrence touches also have nothing BUILT — `HowServiceActivatorWorks.md`, `PipelineValidation.md`,
  `ReactorAndProactor.md`, `V10MigrationGuide.md`, `QueryPipeline.md`, `QueryPatterns.md`,
  `QuartzScheduler.md` — and are not counted: none of their touched blocks is predicted to build

**A verdict per hard block, before it is touched.** From `--classify` (on the page's own text) and
`--explain` (on the probe's `stageP`, `using`s supplied). *Parse* is placeholder, fragment or not
code; *other* is defect, wrapper or probe artefact, or the pin.

| Page | # | `--classify` / probe | Verdict | What the diagnostics and the text show |
|---|---:|---|---|---|
| `AgreementDispatcherRouting.md` | 8 | parse / PARSE | **parse — fragment** | The chain ends in a commented-out `// .AutoFromAssemblies()` and never takes its `;` (`CS1002`); `services` is a value. Its comment, *"Cannot use AutoFromAssemblies with Agreement Dispatcher"*, is a claim about behaviour: **run, with a control** (P0-10) |
| `BuildingAPipeline.md` | 1 | import / DEFECT | **other — defect** | A pre-V10 handler: `using Brighter.commandprocessor.Logging;`, `namespace Brighter.commandprocessor`, `logger.InfoFormat` (`CS0234`, `CS0103`). 10.7.0's is `Paramore.Brighter.Logging.Handlers.RequestLoggingHandler` (`src/Paramore.Brighter/Logging/Handlers/RequestLoggingHandler.cs`) |
| `CloudEventsReference.md` | 2 | import / DEFECT | **other — defect** | `PartitionKey = …` in a `Publication` initialiser, `CS0117`. At 10.7.0 `PartitionKey` is on `MessageHeader` (`MessageHeader.cs:263`), and the default mapper reads it from the request context (`JsonMessageMapper.cs:50`, `Context.GetPartitionKey()`). `OrderCreated` is a type the page never shows |
| `S3LuggageStore.md` | 1 | import / DEFECT | **other — the pin** | `CS0234`: `Paramore.Brighter.Transformers.AWS.V4` is not in the pin (D3, 018). The page lists both packages (`:17`, `:20`); under the V3 namespace the types resolve and only `serviceCollection` and `credentials`, values, remain. **Listed, as `DistributedLock.md` #2 was**, once it is built against the released V4 package in scratch |
| `PostgreSQLBrokerTradeOffs.md` | 1 | import / DEFECT | **other — not one program** | `var configuration` declared twice (`CS0128`): a JSONB form and a JSON form in one fence. A split (§ *Splits*) or two names; `connectionString` is a value |
| `PostgreSQLMessageBroker.md` | 12 | import / DEFECT | **other — two defects** | `[ClaimCheck(threshold: 102400, dataStore: typeof(S3LuggageStore))]`: `CS1739` — 10.7.0's constructor is `ClaimCheckAttribute(int step, int thresholdInKb = 0)` (`Transforms/Attributes/ClaimCheckAttribute.cs:43`), and the store is registered by `UseExternalLuggageStore`, not named on the attribute. `ProcessLargeOrderCommand : Command` with no constructor: `CS1729` — `Command` has `Command(Id)` and `Command(Guid)` only (`Command.cs:68`, `:77`) |
| `AggregationQueryPatterns.md` | 1 | import / DEFECT | **other — probe artefact** | `ApplicationDbContext`, never shown, is a page type; the `CS1061` on `TEntity.Name` is the `ToDictionaryAsync` inference failing behind it. A stub with members (`Categories`) reaches it. `--classify` reads *import* on `Id`, a member access |
| `DarkerAndBrighterPipelines.md` | 1 | parse / PARSE | **parse — placeholder, and a defect** | `...` as the last parameter of both signatures, and neither has a body (`CS8635`, `CS0501`). Beside them, `[RetryableQuery(2, "DefaultCircuitBreaker")]` — the open § *Defect ledger* row |
| `DarkerConfigurationReference.md` | 4 | import / DEFECT | **other — defect** | `.Handlers(registry, Activator.CreateInstance, t => {}, Activator.CreateInstance)`: `CS1503`. Darker 4.1.1's overload takes `Func<Type, IQueryHandler>` and `Func<Type, IQueryHandlerDecorator>` (`Builder/INeedHandlers.cs:9`), and `Activator.CreateInstance` returns `object`. Darker's own README carries the same line at 4.1.1 (`README.md:110`); that text is upstream's. The four query types are the page's |
| `ParameterizedQueryPatterns.md` | 4 | import / DEFECT | **other — defect** | `using System.Threading.Task;` (`CS0234`), then `Task<>` unresolved behind it. The rest are page and same-page types |
| `ProjectionQueryPatterns.md` | 3 | parse / PARSE | **parse — fragment** | A `.Select(o => new OrderDto { … })` with no receiver (`CS1513`, `CS1955`) |
| `QueryHandlerDependencies.md` | 2 | import / DEFECT | **other — probe artefact** | As `AggregationQueryPatterns.md` #1: `ApplicationDbContext`, `CustomerDto`, `GetCustomerWithOrdersQuery` are page types; the `CS1061`s on `TEntity` are the inference behind them |
| `Telemetry.md` | 5 | parse / PARSE | **parse — fragment** | `.SetSampler(new TraceIdRatioBasedSampler(0.1))`, one line with no receiver. `TraceIdRatioBasedSampler` resolves |
| `CQRSUseCasesAndPatterns.md` | 2 | import / DEFECT | **parse — placeholder and fragment** | `: IRequest { /* ... */ }` three times (`CS0535` is the omission), and two controller actions outside any class (`CS0116`, `Ok` non-invocable, `_commandProcessor` and `_queryProcessor` unshown). The probe calls it DEFECT; read, it is an excerpt that says so |
| `HowConfiguringTheDispatcherWorks.md` | 1 | import / DEFECT | **other — defect** | V9's registry: `new MessageMapperRegistry(messageMapperFactory) { { typeof(…), typeof(…) } }`. At 10.7.0 the constructor takes `(IAmAMessageMapperFactory?, IAmAMessageMapperFactoryAsync?)` and the class is not `IEnumerable` (`CS7036`, `CS1922`); registration is `Register<TRequest, TMapper>()` (`MessageMapperRegistry.cs:64`, `:296`). The page's own `:62` has the V10 constructor |

**By this reading, 5 parse and 10 other.** `--classify` reads 4 *parse* and 11 *import*; the probe,
4 PARSE and 11 DEFECT. The one block whose kind differs is `CQRSUseCasesAndPatterns.md` #2, an
excerpt the probe calls a DEFECT. The 10 *other*: **six defects** (`BuildingAPipeline.md` #1,
`CloudEventsReference.md` #2, `DarkerConfigurationReference.md` #4, `HowConfiguringTheDispatcherWorks.md`
#1, `ParameterizedQueryPatterns.md` #4, `PostgreSQLMessageBroker.md` #12), **two probe artefacts**
behind a page type, **one** two-programs-in-one-fence and **one** the pin. **One parse block carries
a defect beside its placeholder**: `DarkerAndBrighterPipelines.md` #1.

**Their recurrences, run now so that 5.2 and 5.3 open with them:**

| Defect | Grep | Hits | Pages |
|---|---|---:|---|
| The pre-V10 namespace | `grep -rnE 'Brighter\.commandprocessor' contents/` | **4** lines, 3 pages | `BuildingAPipeline.md` ×2; `FAQ.md:377` quotes an old exception message and `Monitoring.md:25` an old `app.config` section — both read in 5.2, neither is a `using` |
| `logger.InfoFormat` | `grep -rnE '\.InfoFormat\(' contents/` | **1** | `BuildingAPipeline.md` |
| `PartitionKey` on a `Publication` | `grep -rnE '\bPartitionKey *=' contents/` → **6** lines, 5 pages, read | **1** | `CloudEventsReference.md`. Of the other five: `KafkaConfiguration.md:213` is read in 5.3; `:724` and `MessageMappers.md:147` set `header.PartitionKey`, right; `UsingTheContextBag.md:350` is a constant; `V10MigrationGuide.md:350` sets `Context.PartitionKey`, and `IRequestContext` has no such property at 10.7.0 — part of the open `IRequestContext` row, 5.5's |
| `[ClaimCheck]` given a threshold in bytes and a store | `grep -rnE 'ClaimCheck\([^)]*(threshold:\|dataStore)' contents/` | **1** | `PostgreSQLMessageBroker.md` |
| A `Command` or `Event` with no constructor | the compiler: `CS1729 … 'Command'\|'Event' does not contain a constructor that takes 0 arguments` over the whole corpus under `stageP` → **4** blocks; a text scan of every fence → **4** classes | **5** on **4** pages, the union | `PostgreSQLMessageBroker.md` #12, `NullableReferenceTypes.md` #9, `MigratingToNullableReferenceTypes.md` #4, `V10MigrationGuide.md` #20 (both methods but the last), and `V10MigrationGuide.md` #1, a skipped V9 form (text scan only). The text scan missed `MigratingToNullableReferenceTypes.md` #4, whose `new CreateOrderCommand()` it read as a constructor |
| `Activator.CreateInstance` as a Darker factory | `grep -rn 'Activator\.CreateInstance' contents/` | **2** | `DarkerConfigurationReference.md:82`, `ImplementAQueryHandler.md:451` |
| `using System.Threading.Task;` | `grep -rn 'using System\.Threading\.Task;' contents/` | **1** | `ParameterizedQueryPatterns.md` |
| V9's `MessageMapperRegistry` initialiser | `grep -rn 'new MessageMapperRegistry(' contents/` → 2 lines, read | **1** | `HowConfiguringTheDispatcherWorks.md:40`; its `:62` is the V10 form |

**Carried into the repair tasks, not the prediction.** Each is on `master` now, measured at
`976e0e0`, and given the task whose section holds its page:

- **`[RetryableQuery]`'s second argument (5.2).** § *Defect ledger* recorded **9** lines on **5**
  pages. **Said: 9; measured: 20 lines on 6 pages** with any second argument (`grep -rnE
  'RetryableQuery\([^)]*,' contents/` and a Python scan of every `RetryableQuery(…)`, agreeing). The
  ledger's grep matched only `DefaultCircuitBreaker` and `circuitBreakerName`, and the defect is
  any breaker-shaped name used as though it added a breaker. `ShowMeTheCode.md:70` names a retry
  policy, the argument's right kind. The other **19**, on `QueryPipeline.md` (14),
  `CQRSWithBrighterAndDarker.md` (2), `DarkerAndBrighterPipelines.md`, `ImplementAQueryHandler.md`
  and `QueryPatterns.md`, are read one by one; `QueryPipeline.md:649` is the troubleshooting case,
  *"Policy name doesn't exist"*
- **`MessageBody` given a string content type (5.3).** `KafkaConfiguration.md:723` and
  `MessageMappers.md:146`, both still there (`grep -rnE 'new MessageBody\([^)]*, *("|MediaTypeNames)'
  contents/` → **2**)
- **Mapper excerpts omitting a required member with no `// ...` (5.3)**, the recurrence of the mapper
  world 5.3 opens: `Routing.md` #1, `V10MigrationGuide.md` #3, #18, `NullableReferenceTypes.md` #7,
  `FAQ.md` #7. Three of the four pages sit in no tranche, and fixing the issue, not the instance, is
  why they go with 5.3 rather than to the residual
- **The unshown `Order` (5.4)** is `CQRSWithBrighterAndDarker.md` #7, `:702`: FAILED `CS0103`,
  `CS0246`; `IOrderRepository`, `IProductRepository`, `OrderItem`, `OrderStatus` and
  `OrderPlacedEvent` are unshown beside it
- **`InMemoryScheduler.md` (5.4) says to install `Paramore.Brighter.InMemoryScheduler`** (`:225`,
  `:228`). No project of that name is in `src/` at 10.7.0, and `InMemorySchedulerFactory` is in
  `Paramore.Brighter` (`src/Paramore.Brighter/InMemorySchedulerFactory.cs:37`, `TimeProvider`).
  Checked against NuGet in 5.4 before it is called a defect. A sentence claiming how the scheduler
  uses time is run, with a control (P0-10)
- **`QuartzScheduler.md:359`'s U+200B (5.4)**, still the corpus's one (`grep -rlP '\x{200B}'
  contents/` → 1 page, 1 line). Removed with a tool that writes bytes, not the Edit tool
- **`IRequestContext` in `V10MigrationGuide.md` (5.5).** **Said at 2.3: `:320`. Measured: the class
  is at `:326`**, block #12, and the section around it also lists `PartitionKey` and `CustomHeaders`
  as new `IRequestContext` properties (`:316`, `:317`) and sets `Context.PartitionKey` and
  `Context.CustomHeaders` (`:350`, `:353`, block #13). At 10.7.0 the interface has neither
  (`git show 10.7.0:src/Paramore.Brighter/IRequestContext.cs | grep -c 'PartitionKey\|CustomHeaders'`
  → 0). A second implementation
  at `:381` is read beside it
- **E4's line numbers have drifted since the design.** **Said:** `ReactorAndProactor.md:190`,
  `V10MigrationGuide.md:282`. **Measured:** `:200` and `:288` (`attr_mismatch.py`, above). The
  seven hits sit in `HowServiceActivatorWorks.md` #16, `PipelineValidation.md` #7, #9, #10,
  `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6 and `V10MigrationGuide.md` #10,
  all FAILED, as the design found them
- **`Order` (the 1.10 ruling) acts on two reachable blocks.** `grep -cw Order` over the `--show` of
  the 62 FAILED blocks → **4**, none hard; `--classify` lists `Order` as a missing name on **2** of
  them, `PostgreSQLMessageBroker.md` #7 and `QueryHandlerDependencies.md` #1, and the other two use
  the word only (`CQRSUseCasesAndPatterns.md` #1, `PostgreSQLMessageBroker.md` #4). Both are read as
  a type the page never shows, and get a stub, never `using StackExchange.Redis`

**Task 5.2 — the *Commands, Handlers and Pipelines*, *Darker* and *Understanding Brighter* pages.**
Ten pages. **BUILT 234 → 262** (+28): **27** `FAILED -> BUILT` on nine of the ten, and one new key,
`QueryPipelinePolicies.md` #7, appended. The rest of the AC2 diff, joined on page and ordinal
against the report at `d69a554`: `DarkerAndBrighterPipelines.md` #1 `FAILED -> SKIPPED` and
`AgreementDispatcherRouting.md` #12 `- -> FAILED`, a split (§ *Splits*). `pagelint` **616 → 599**,
every one of the 17 on a page whose blocks gained `using`s: `AgreementDispatcherRouting.md` −5,
`BuildingAPipeline.md` −3, `CQRSUseCasesAndPatterns.md`, `DarkerConfigurationReference.md`,
`HowConfiguringTheDispatcherWorks.md`, `QueryHandlerDependencies.md` −2 each, `AgreementDispatcher.md`
−1 (per page, against a worktree at `origin/master`). Pages with nothing BUILT **57 → 49**, by
requirements' `awk` and a Python join over the same report: seven gained a BUILT block
(`AggregationQueryPatterns.md`, `AgreementDispatcherRouting.md`, `BuildingAPipeline.md`,
`CQRSUseCasesAndPatterns.md`, `DarkerConfigurationReference.md`, `HowConfiguringTheDispatcherWorks.md`,
`QueryHandlerDependencies.md`), and `DarkerAndBrighterPipelines.md` left because its one block is now
SKIPPED, not because it built.

- **Against 5.1's reading, block by block.** The floor was 246: 234 + 1 `using` + 11 empty stubs.
  **Said:** `BuildingAPipeline.md` #3 BUILT by a `using` alone. **Measured:** same-page — the `using`
  that builds it, `Paramore.Brighter.Logging.Handlers`, names Brighter's own `RequestLoggingHandler<>`,
  not the one block 1 writes; #2 likewise resolves `[RequestLogging]` to Brighter's attribute, not
  block 3's. **Said:** `DarkerConfigurationReference.md` #1 empty stub, #2 and
  `QueryHandlerDependencies.md` #3 typed value. **Measured:** each also names `Program`, which Q2
  rules out. **Said:** `HowConfiguringTheDispatcherWorks.md` #3 empty stub. **Measured:**
  `ServiceControl` and `HostControl` are Topshelf's, a package the pin does not carry; the block
  builds only since Topshelf was retired (second pass, below). **Said:** `BuildingAPipeline.md` #4 members. **Measured:** unit rule
  1 — the section tells the reader to write the handlers it chains. So **7** of the **27** reachable
  blocks on these pages stayed FAILED in the first pass (**6** after the second), and **7** of their **10** hard blocks built; one is SKIPPED and
  two stay FAILED (`ParameterizedQueryPatterns.md` #4, same-page once its `using` is right, and
  `ProjectionQueryPatterns.md` #3, a fragment). 20 + 7 + the appended block = **28**
- **Five units.** `AgreementDispatcherRoutingContext.cs` (the scenarios' requests and handlers,
  `services`, `registry`), `DarkerQueryPatternsContext.cs` (one EF Core model for four pages:
  `AggregationQueryPatterns.md`, `ProjectionQueryPatterns.md`, `ParameterizedQueryPatterns.md`,
  `QueryHandlerDependencies.md`), `DarkerConfigurationReferenceContext.cs`,
  `HowConfiguringTheDispatcherWorksContext.cs`, `CQRSUseCasesAndPatternsContext.cs`. None is a type a
  page tells the reader to write (rule 1, by reading). **Three shapes the rule forced, each recorded
  in its unit:** a type no block names but whose members a block reads (an order's items, its
  address) is a tuple, not a stub; a Darker handler stub is `abstract`, so it need not declare the
  `Execute` no block names — its first form did, and put **8** errors against the scaffold tree,
  which `--report` prints as a warning and does not fail on; a mapper stub derives from Brighter's
  `JsonMessageMapper<T>` and so declares no member. `CQRSUseCasesAndPatterns.md` #1 now declares its
  query and `DbContext` itself, because a unit that named the block's `Order` would not compile
  beside block 2. `ParameterizedQueryPatterns.md` #3, #5 and `ProjectionQueryPatterns.md` #4, BUILT
  before, are re-admitted with the new unit. `--report` → *"40 units checked, 0 violations"*
- **Made whole:** `AgreementDispatcherRouting.md` #8 (it ended in a commented-out call with no `;`,
  and now shows the scan working, below) and the three `{ /* ... */ }` routing lambdas that returned
  nothing; `CQRSUseCasesAndPatterns.md` #2, whose commands were `: IRequest { /* ... */ }` and whose
  actions sat outside any class, now a controller over `Command`s; `BuildingAPipeline.md` #1
- **Skipped, with an accepted reason:** `DarkerAndBrighterPipelines.md` #1, two attribute stacks on
  signatures shown side by side for comparison. Its `...` parameters are now the real
  `CancellationToken`, and its `[RetryableQuery]` the corrected form
- **Nine defects, and the carried `[RetryableQuery]` row closed** (§ *Defect ledger*). The pre-V10
  logging handler, and the recurrence of its namespace in `FAQ.md`'s quoted exception, which a run
  gives as `Paramore.Brighter.ICommand`; `.Successor =` for `SetSuccessor()`, and the manual chain
  said to work from the registry alone; *"Cannot use AutoFromAssemblies"* with an Agreement
  Dispatcher, on three pages; Darker's query processor said to default to Transient, and the
  unscoped failure said to be a disposed `DbContext`, on two; `Activator.CreateInstance` as a Darker
  factory, on two; `using System.Threading.Task;`; V9's `MessageMapperRegistry` initialiser;
  `RmqSubscription<T>`'s pump type given the wrong reason; `CQRSUseCasesAndPatterns.md` #1's handler
  reading members its own write model lacks. **`[RetryableQuery]`: 20 lines → 8**, every one left a
  policy name that is registered, or a deliberate *"doesn't exist"*, and described as the one policy
  the decorator runs; `QueryPipelinePolicies.md` gains the retry-and-breaker wrap it links to
- **`--explain` on all 46 blocks the diff touches**, after `pagelint --changed` asked for `using`s on
  four `QueryPipeline.md` fragments (now marked `// ...`): `[FallbackPolicy]` used with no `using
  Paramore.Darker.Attributes` on three `QueryPipeline.md` blocks, a placeholder handler body that
  returned nothing, and `System.Linq`, `System` and `System.Collections.Generic` missing from
  `ProjectionQueryPatterns.md` #1, `ParameterizedQueryPatterns.md` #2 and
  `CQRSWithBrighterAndDarker.md` #3. All repaired; what remains on those blocks is names their pages
  never show
- **The blocks that stay FAILED compile where their world exists** (§ *Blocks that stay FAILED*):
  `DarkerConfigurationReference.md` #1, #2 and `QueryHandlerDependencies.md` #3 as a `Program.cs`
  against Darker 4.1.1;
  `ParameterizedQueryPatterns.md`'s six blocks together, with the never-shown entity stubbed;
  `BuildingAPipeline.md` #1–#3 together in the pipeline run below — each **0** errors
- **Behaviour, run with controls** against released packages (Brighter 10.7.0, Darker 4.1.1,
  EF Core 9.0.15) in scratch console apps, net10.0, one process per case:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `BuildingAPipeline.md` #1–#3: the attribute puts the logging handler before the target | `AutoFromAssemblies()` → the handler logs, then *"Hello Ian"* | no attribute → *"Hello Ian"* only |
  | The new sentence: a custom open generic handler must be registered | `.Handlers(r => r.Register<…>())` plus `AddTransient(typeof(RequestLoggingHandler<>))` → logs | without it → `ConfigurationException`, *"Could not create handler RequestLoggingHandler`1[GreetingCommand]"*; the async form the same, both ways |
  | `AutoFromAssemblies` skips a non-public handler (why `BuildingAPipeline.md` says *public*) | public handler → found | `internal` → `ArgumentException`, *"No command handler was found"* |
  | #4: the manual chain | a handler factory returning the wired `MyLoggingHandler` → both handlers run | a factory returning a new one → the logging handler only |
  | `FAQ.md`: sending through `ICommand` | `ArgumentException`, *"… typeof command Paramore.Brighter.ICommand …"* | the concrete type → handled |
  | `AgreementDispatcherRouting.md` #8: scan beside an agreement | `AutoFromAssemblies(excludeDynamicHandlerTypes: […])`, either order → High → `HighPriorityHandler`, Low → `StandardHandler`, a scanned `OtherCommand` handled | no exclusion, either order → every `Send` of `MyCommand` throws *"More than one handler was found"* |
  | `DarkerConfigurationReference.md`: the default lifetime and the unscoped failure | `AddDarker()` → `IQueryProcessor` **Singleton**; scope validation on → *"Cannot resolve 'GetContextQueryHandler' from root provider because it requires scoped service"*; off → both scopes' queries see `DbContext` **#1**, never disposed | `QueryProcessorLifetime = Scoped` → **#1**, then **#2** |
  | #4 and `ImplementAQueryHandler.md`: manual registration with the casts | the query returns `Ada,Bob` | the old form → `CS1503`, compiled |
  | `QueryPipelinePolicies.md` #7: a retry wrapped round a breaker | `[RetryableQuery(1, "RetryAndBreak")]`, breaker of 2 → **2** attempts, then `BrokenCircuitException`; call 2 → **0** attempts | default → **4** attempts; the default breaker alone → **1**, then **0**; `"DefaultCircuitBreaker"` → `ConfigurationException`, 0 |
  | *"`AddPolicies()` requires both `Constants` names"* | a registry without the breaker → `ConfigurationException`, *"… missing the Darker.CircuitBreakerPolicy policy"* | both → accepted |
  | `HowConfiguringTheDispatcherWorks.md` #2 and its two warnings, against RabbitMQ | the block verbatim → a sent `GreetingCommand` handled **1** time | `Subscription<T>` → `ConfigurationException`, *"We expect an RmqSubscription"*; `Proactor` → *"You must provide a message mapper registry"*; `messagePumpType` omitted → handled, which is why the page now says `Reactor` is `RmqSubscription<T>`'s default |
  | The Darker query-pattern handlers against SQL Server 2022 | `AggregationQueryPatterns.md` #1–#3, `ProjectionQueryPatterns.md` #1, #2, `QueryHandlerDependencies.md` #2 → correct results; #3's statistics 155 / 51.67 / 25 / 100 | #3 on an empty range → zeros, its `?? new SalesStatisticsDto()`; `ProjectionQueryPatterns.md` #1's comment, *"SELECT Id, Name, Email only"*, against the whole entity → four columns |

  On SQLite, `AggregationQueryPatterns.md` #3 throws *"SQLite cannot apply aggregate operator 'Min'
  on expressions of type 'decimal'"* — the provider's limit, and the page names none. Read, not run:
  `CQRSUseCasesAndPatterns.md` #1 joins and aggregates as `ProjectionQueryPatterns.md` #2 does, run
  above; its #2 is a controller over the command and query processors
- **Four findings off the tranche were put to the maintainer** — `Monitoring.md`'s V9 `app.config`
  section, Topshelf, handlers declared without `public`, and `DarkerBasicConfiguration.md`'s naming
  and nesting rules — and each was ruled on and repaired in the second pass, below. Darker's own README
  carries the uncast `Activator.CreateInstance` line (`README.md:110`)
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 28 rows and 3 re-admissions at `1cafef9` (`09f5848`). `--report` → exit **0**,
  *"985 blocks: 262 BUILT, 706 FAILED, 17 SKIPPED"*, baseline 262, 0 findings
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings,
  22 entries, 3 silenced; `optioncheck` 0 mismatches across 59 tables, 519 rows; shape, redirects and
  `--verify` unmoved; `pagelint --changed origin/master` 0 errors. **Pages changed: 18**
  (`git diff --name-only d69a554..HEAD -- contents`): **9** of the 10 tranche pages —
  `AggregationQueryPatterns.md` built by its unit alone — and **9** by recurrence: `AgreementDispatcher.md`, `BuildingAnAsyncPipeline.md`, `CQRSWithBrighterAndDarker.md`,
  `DarkerBasicConfiguration.md`, `FAQ.md`, `ImplementAQueryHandler.md`, `QueryPatterns.md`,
  `QueryPipeline.md`, `QueryPipelinePolicies.md`

**Task 5.2, second pass — the four findings, ruled 2026-09-28.** The maintainer: rewrite
`Monitoring.md`; retire Topshelf; make the non-public handlers public; fix
`DarkerBasicConfiguration.md`'s troubleshooting. **BUILT 262 → 265**: `Monitoring.md` #1, #2 and
`HowConfiguringTheDispatcherWorks.md` #3, all `FAILED -> BUILT`, no other key moved. `pagelint`
**599 → 585**: `PolicyRetryAndCircuitBreaker.md` −6, `FeatureSwitches.md` −4, `Monitoring.md` −2,
`MigratingToPollyV8.md` and `PolicyFallback.md` −1 each (per page, against a worktree at `623c786`).
Pages with nothing BUILT **49 → 48**, `Monitoring.md`. `attr_mismatch.py` **7**, one hit's line moved
by the `using`s above it: `PolicyRetryAndCircuitBreaker.md:326` → `:359`. Repair `f2ce226`, baseline
`a347ba6`, `--report` exit 0, *"985 blocks: 265 BUILT, 703 FAILED, 17 SKIPPED"*.

- **`Monitoring.md`, rewritten for V10.** The page registered a V9 `app.config` section and a
  container call for `MonitorHandler<T>`, and linked the retired site's Control Bus page. At 10.7.0
  the handler takes an `IAmAControlBusSender` and a `MonitorConfiguration` (a plain class) from the
  container, and `AddBrighter()` makes `MonitorHandler<T>` available with either registration style.
  The page now builds a sender with `ControlBusSenderFactory`, shows `[Monitor]`, and prints the
  message format captured from a run. **Two upstream defects, found running it and stated on the
  page** (§ *Defect ledger*), and filed on the maintainer's word: BrighterCommand/Brighter#4453, #4454
- **Topshelf retired.** `HowConfiguringTheDispatcherWorks.md` now recommends `AddConsumers()`'s hosted
  service and, without HostBuilder, runs the Dispatcher from a console app until Ctrl+C. #3 builds
- **Handlers declared without `public`: 18 → 0** on 6 pages (a grep and a Python scan of every C#
  fence agreeing), and `CommandProcessorConfigurationReference.md` says the scan registers only public
  handlers. The mapper scan has no such filter, so the sentence names handlers alone
- **`DarkerBasicConfiguration.md`**: a handler must be exported — public, and nested only in a public
  class; its name does not matter
- **`--explain` on the 20 blocks the rulings touched found three more**, each repaired at every
  recurrence: `[UseResiliencePipeline]` stacked on one method, `CS0579` — the attribute is not
  repeatable, by design since BrighterCommand/Brighter#2580 — on **7** blocks across **5** pages, each
  now one pipeline composed in the order the stack meant; `PolicyRetryAndCircuitBreaker.md` saying a
  pipeline's strategies wrap *"inner to outer"* in the order added, and building its comprehensive
  pipeline timeout-first — Polly v8 makes the first strategy added the outermost, so that timeout
  wrapped every retry; and a `FeatureSwitches.md` handler that awaited without `async`. With them,
  `using`s on the touched blocks and two placeholder bodies that returned nothing
- **Behaviour, run with controls**, released 10.7.0 packages, net10.0:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `Monitoring.md` #1, #2 verbatim | `Send` → *"Hello Ada"*, **2** `MT_EVENT`s on `brighter.monitoring`, `EnterHandler` then `ExitHandler` | `IsMonitoringEnabled = false` → **0**; `.Handlers(…)` in place of `AutoFromAssemblies()` → **2** |
  | Turning it off at runtime | request 1 → **2** events; flag set `false`; request 2 → none | — the first request is its control |
  | A monitored handler that throws | `InvalidOperationException` in the handler → the caller gets **`NotSupportedException`**, *"… 'System.Reflection.MethodBase' instances is not supported"*; **1** event | the handler not throwing → **2** events |
  | `[MonitorAsync]` through the factory's sender | *"No message mapper defined for request"*; **0** events | `[Monitor]`, sync → **2** |
  | `HowConfiguringTheDispatcherWorks.md` #2 + #3 against RabbitMQ | Ctrl+C a second into a 4 s handler → *stopping*, the handler finishes, *ended*, exit **0** | without `e.Cancel = true` → exit **−2**, the handler never finishes |
  | Darker's scan | a handler named `FetchSomething`, and one nested in a public class → found | `internal` → `MissingHandlerException`; public nested in an `internal` class → the same |
  | Polly v8 order | `AddRetry().AddTimeout(100 ms)`, 300 ms work → **4** attempts | `AddTimeout().AddRetry()` → **1** attempt |
  | One composed pipeline in place of two attributes | `AddCircuitBreaker().AddRetry(3)`, always failing → sends 1, 2 **4** attempts each, send 3 `BrokenCircuitException`, **0** | two `[UseResiliencePipeline]` on the method → `CS0579` |


**Task 5.3 — the *Transports*, *Using an External Bus* and *Health Checks and Observability* pages.**
Six pages, all changed. **BUILT 265 → 288** (+23): **22** `FAILED -> BUILT` and two new keys BUILT,
less `Telemetry.md` #1's `BUILT -> FAILED`, which is the old #1 renumbered by a block inserted above
it (§ *Splits*). The rest of the AC2 diff, joined on page and ordinal against the report at
`5129c20`: `Telemetry.md` #6 `- -> FAILED` (the same insertion), `CloudEventsReference.md` #5 (a block inserted above it) and
`PostgreSQLBrokerTradeOffs.md` #2 (a split) `- -> BUILT`, `ConfiguringOpenTelemetry.md` #7 `FAILED ->
-`: its Jaeger block, old #2, was removed and the five after it moved up (§ *Blocks removed*). **985 → 987 blocks.** `pagelint` **585 → 562** (−23, per page against a
worktree at `5129c20`): `PostgreSQLMessageBroker.md` −10, `CloudEventsReference.md` −4,
`Telemetry.md` −3, and −1 each on `ConfiguringOpenTelemetry.md`, `MigratingToNullableReferenceTypes.md`,
`NullableReferenceTypes.md`, `PostgreSQLBrokerTradeOffs.md`, `S3LuggageStore.md`,
`V10MigrationGuide.md`. Pages with nothing BUILT **48 → 44**, by requirements' `awk` and a Python
join agreeing: `BrighterControlAPI.md`, `CloudEventsReference.md`, `PostgreSQLBrokerTradeOffs.md`,
`S3LuggageStore.md`. Repair `2d938c8` (WIP, session 101), `154f78c` and `8d2fb8b`; baseline `af77ae4`.

- **Five units.** `BrighterControlAPIContext.cs` (`app`), `CloudEventsReferenceContext.cs`,
  `PostgreSQLMessageBrokerContext.cs` (mapped to both PostgreSQL pages), `TelemetryContext.cs`, and
  `S3LuggageStoreContext.cs` (`serviceCollection`). None is a type a page tells the reader to write
  (rule 1, by reading). `PostgreSQLMessageBroker.md` #11 and `Telemetry.md` #4, BUILT before, are
  re-admitted with their units. `--report` → *"45 units checked, 0 violations"*
- **The maintainer's five rulings, 2026-09-28.** (1) Tracing: rewrite `Telemetry.md`,
  `PostgreSQLMessageBroker.md` #8 and, off the tranche, `ConfiguringOpenTelemetry.md`. (2) No pin
  change: the tracing blocks that need `OpenTelemetry.Extensions.Hosting` and
  `Paramore.Brighter.Extensions.Diagnostics` stay FAILED as a pin limit, each compiled in scratch
  against the released packages at **0** errors. (3) Fix `CloudEventsSupport.md`, off the tranche;
  the **11** non-generic subscriptions without `messagePumpType` are recorded, not repaired
  (§ *Defect ledger*). (4) File the `souce` bug: BrighterCommand/Brighter#4458, linked from
  `CloudEventsReference.md`. (5) `PostgreSQLBrokerTradeOffs.md`'s size: tested to 50 MB, larger may
  fit, 150 MB rejected; AWS SQS is 1 MiB in its comparison table, with Brighter's support for SQS
  messages over 256 KB said to ship in the release after 10.7.0
- **Twenty-three defects** (§ *Defect ledger*), twelve on the two PostgreSQL pages. Five recur off
  the tranche and were repaired there: `PostgresOutbox.md`, where the EF Core
  provider's package is installed, now states the Npgsql version it needs; `HandlingLargeMessages.md`'s S3 store,
  created with no `ACLs`; a `string` content type given to `MessageBody` on `KafkaConfiguration.md` and
  `MessageMappers.md`; mappers missing a member of `IAmAMessageMapper<T>` on `Routing.md`,
  `V10MigrationGuide.md` (#3, #18), `NullableReferenceTypes.md` (#7) and `FAQ.md` (#7, whose
  `MapToMessage` also returned nothing); `Command`/`Event`
  subclasses with no base-constructor call on `NullableReferenceTypes.md` (#9, now BUILT),
  `MigratingToNullableReferenceTypes.md` (#4) and `V10MigrationGuide.md` (#20), whose example also
  published where it meant to post
- **`--explain` on every block the diff touches** (`git diff -U0 5129c20` hunks against fence
  ranges): **39** blocks, **23** BUILT. It found the three constructor blocks without `using`s, now
  given them. The **16** FAILED are the six pin-limited blocks below, `S3LuggageStore.md` #1, and on
  the off-tranche pages names their pages never show and two blocks that put a type before a
  statement (`MigratingToNullableReferenceTypes.md` #4, `V10MigrationGuide.md` #20), not repaired
- **Behaviour, run with controls**, released 10.7.0 packages, net10.0, one process per case:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `PostgreSQLMessageBroker.md`: consumer and publication names | `channelName` equal to the publication's `Topic` → **1** handled | the page's names → **0**: the consumer reads `queue = ChannelName`, the producer writes `queue = Topic` |
  | Sending to the broker | `PostAsync` → a row in the table | `PublishAsync` → no row; it runs local handlers |
  | *Scheduled Messages* | `PostAsync(delay)` → the scheduler fires; the row appears at t≈6 s, visible at once | `PublishAsync(delay)` → a local handler, no row |
  | What `visible_timeout` delays | a deferred message with `requeueDelay` 20 s → visible again at 19.5 s | `requeueDelay` 0 → handled 3×, the row deleted on reject |
  | The outbox in an EF Core transaction | with the transaction provider, rollback → **0** outbox rows | without it → **1** |
  | Message size, `PostgreSQLBrokerTradeOffs.md` | 1 KB, 5 MB, 50 MB round-trip | 150 MB → Npgsql `54000`, *"total size of jsonb object elements exceeds the maximum of 268435455 bytes"*; 200 MB → *"JSON value of length 209715200 is too large"*. 100 MB posted and not received within 6 s, unresolved, so the page claims 50 |
  | `CloudEventsReference.md`: headers on the wire | RMQ.Async → `cloudEvents_id/source/specversion/time/type`, the content type in the AMQP property; Kafka → `ce_*` and `content-type` | a structured-mode mapper → the envelope in the body. SNS and ASB by reading |
  | The Kafka partition key | `RequestContextBagNames.PartitionKey` in the context bag → the record key | no bag entry → empty key |
  | `Telemetry.md`: spans need a registered tracer | `AddBrighterInstrumentation()` → **34** spans; a manual `BrighterTracer` with `AddSource("paramore.brighter")` → **34**, the name case-insensitive | `AddSource` alone → **0**; a wrong source → **0**; `Sdk.CreateTracerProviderBuilder().AddBrighterInstrumentation()` → **0** Brighter spans |
  | Trace propagation | the OTel SDK initialised → RMQ `cloudEvents_traceparent`, `cloudevents_tracestate`; Kafka `ce_traceparent`, `ce_tracestate` | a bare `ActivityListener` → Brighter writes neither: it injects through `Propagators.DefaultTextMapPropagator` |
  | `S3LuggageStore.md`: `ACLs`, on LocalStack | `ACLs` unset, bucket missing → `ConfigurationException`, *"No ACL setup on S3Luggage Store"*, no bucket | `ACLs = S3CannedACL.Private` → bucket created; `ACLs` unset, bucket present → accepted |
  | `BrighterControlAPI.md` against a V10 Dispatcher | `GET /control/status` → the page's JSON, captured; `PATCH …/orders-subscription/performers/3` → 200, 3 performers | by routing key → 400 *"No such subscription"*; wrong case → **500**, `InvalidOperationException` from `Dispatcher.SetActivePerformers`; `baseRoute` `/ops/brighter` → served there, `/control/status` 404 |
  | `V10MigrationGuide.md` #20: the default mapper | `PostAsync` → **1** message, `application/json` | `PublishAsync` → **0** |

  Every table in `Telemetry.md` was rewritten from captured spans. Every SQL block in
  `PostgreSQLMessageBroker.md` was run against the table
- **The blocks that stay FAILED compile where their world exists** (§ *Blocks that stay FAILED*):
  the six pin-limited tracing blocks — `Telemetry.md` #1, #6, `ConfiguringOpenTelemetry.md` #1, #5,
  #6 and `PostgreSQLMessageBroker.md` #8 — in scratch against
  `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics`; `S3LuggageStore.md`
  #1 against `Paramore.Brighter.Transformers.AWS.V4` 10.7.0 — each **0** errors.
  `ConfiguringOpenTelemetry.md` #2–#4 are exporter fragments marked `// ...`
- **Put to the maintainer, not repaired:** `DefaultMessageMappers.md` #4, an Avro mapper that also
  passes `CharacterEncoding.Raw` as `MessageBody`'s content type, but whose constructor
  (`AvroMessageMapper<T>(…)`) does not parse and whose Confluent calls do not match that API;
  whether `PublishAsync` shown as sending to a transport is swept across the corpus; and whether the
  Control API's wrong-case 500 is filed
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 24 rows and 2 re-admissions; `Telemetry.md` #1 and #4's rows removed. `--report` →
  exit **0**, *"987 blocks: 288 BUILT, 682 FAILED, 17 SKIPPED"*, baseline 288, 0 findings
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings,
  22 entries, 3 silenced, `--verify-list` clean; `optioncheck` 0 mismatches across 59 tables, 519 rows;
  shape, redirects and `--verify` 161 / 77 / 161; `pagelint --changed origin/master` 0 errors.
  **Pages changed: 17** (`git diff --name-only 5129c20..HEAD -- contents`): the **6** tranche pages
  and **11** by ruling or recurrence — `CloudEventsSupport.md`, `ConfiguringOpenTelemetry.md`,
  `FAQ.md`, `HandlingLargeMessages.md`, `KafkaConfiguration.md`, `MessageMappers.md`,
  `MigratingToNullableReferenceTypes.md`, `NullableReferenceTypes.md`, `PostgresOutbox.md`,
  `Routing.md`, `V10MigrationGuide.md`

**Task 5.4 — `ITimerProvider`, the unshown `Order`, and the three rulings on 5.3's questions.**
**`grep -rn ITimerProvider contents/ | wc -l` → 0**, read from a file (4 at `aec11c1`).
`CQRSWithBrighterAndDarker.md` now shows `Order`: the write model is a new block, #7, BUILT; the
handler that assigns `Id = command.Id` is #8 and stays FAILED, same-page. **BUILT 288 → 293** (+5),
every move `FAILED -> BUILT` or a new key: `InMemoryScheduler.md` #5, `BrighterSchedulerSupport.md`
#1, `DefaultMessageMappers.md` #4 and #5, and `CQRSWithBrighterAndDarker.md` #7 (inserted). The rest of the
AC2 diff, aligned by each fence's first non-`using` line against `aec11c1` rather than by ordinal:
`CQRSWithBrighterAndDarker.md` #8–#13 are old #7–#12 renumbered by the insertion;
`DefaultMessageMappers.md` #4–#6 are old #4 split, and #7–#15 are old #5–#13; and
`DynamicMessageDeserialization.md` old #7 is removed, with #7 and #8 being old #8 and #9. All of them
were FAILED on both sides (§ *Splits*, § *Blocks removed*). **987 → 989 blocks.** `pagelint`
**562 → 553** (−9): `DefaultMessageMappers.md` −4 (#2, #10, #11, and old #4 split into three blocks that
carry `using`s), and −1 each on `InMemoryScheduler.md` #5, `BrighterSchedulerSupport.md` #1,
`DispatchingARequest.md` #3, `V10MigrationGuide.md` #26, and the removed block. Pages with nothing
BUILT **44 → 42**, by the requirements' `awk` and a Python join agreeing: `BrighterSchedulerSupport.md`,
`DefaultMessageMappers.md`. Pin `07d878b`, `aeb65f4`; repair `b4c3ae0`, `e742502`; baseline `cfd964e`.

- **The maintainer's three rulings, 2026-09-28**, on 5.3's questions. (1) Rewrite
  `DefaultMessageMappers.md`'s Avro block against Confluent's API, in 5.4. (2) Sweep `PublishAsync`
  across the corpus and repair every instance that means the bus, keeping those that are in-process
  events. (3) File the Control API's wrong-case 500. It is BrighterCommand/Brighter#4465, `Bug`,
  `0 - Backlog`, linked from `BrighterControlAPI.md`
- **The pin grew twice, against 5.1's "no pin change"**, each change in its own commit and measured
  alone with no page changed: no verdict moved either time. `Microsoft.Extensions.TimeProvider.Testing`
  10.10.0 (`FakeTimeProvider`, latest stable, with a net9.0 build) went in at `07d878b`, and
  `Confluent.SchemaRegistry.Serdes.Avro` 2.15.0 (the Confluent release that
  `Paramore.Brighter.MessagingGateway.Kafka` 10.7.0 depends on) at `aeb65f4`. That makes **100**
  PackageReferences and **545** reference assemblies, against 542
- **`InMemoryScheduler.md`.** Two sentences, the pipeline diagram and block #5 now name
  `TimeProvider`, and block #5 is a `FakeTimeProvider` test. The old block also passed a
  `FakeTimerProvider` to a constructor that `InMemorySchedulerFactory` does not have. The page said to
  install `Paramore.Brighter.InMemoryScheduler`, which returns 404 on NuGet; the scheduler is in
  `Paramore.Brighter`, and `UseScheduler` is in the DI package. The test registers its handler
  with `AsyncHandlers`, because `AutoFromAssemblies()` hit #4414 on the first run and the page links
  `SchedulingAMessage.md`'s workaround
- **`CQRSWithBrighterAndDarker.md`.** Showing `Order` surfaced two hidden defects. `Id = command.Id`
  is `CS0029` against the read side's `Guid` key: `Id` converts implicitly only to `string`
  (`Id.cs:95`). And #2 passed a `CancellationToken` in `PublishAsync`'s `RequestContext?` slot,
  which is `CS1503`. #2 was also written against a different, unshown `Order` (`PlacedAt`,
  `OrderStatus.Placed`, a two-argument event); it now uses the model #7 shows, and its prose links there.
  Both `PublishAsync` calls on the page stay: they raise an in-process event to update the read model
- **`BrighterSchedulerSupport.md` #1** listed the six scheduled overloads with the request before the
  time, and without `RequestContext` or Post's `args`. It is now as 10.7.0 declares them
  (`IAmACommandProcessor.cs:98`, `:111`, `:171`, `:190`, `:261`, `:282`). No call site in the corpus
  followed the wrong order. The scan read every `Send`/`Publish`/`Post`/`DepositPost` call's arguments
- **`DefaultMessageMappers.md`, the Avro block, by ruling.** It is now an `IAmAMessageMapperAsync<T>`
  constrained to `ISpecificRecord`, calling `AvroSerializer<T>.SerializeAsync(T, SerializationContext)`
  and `AvroDeserializer<T>(registry).DeserializeAsync`. The `Id` travels in the header, and the content
  type is `application/octet-stream` (Confluent's wire format, as in `samples/TaskQueue/KafkaSchemaRegistry`).
  The page now shows the `.avsc`, and the avrogen partial's other half implements `IEvent`, because
  `RequestToMessageType` throws for a bare `IRequest`. The partial is in the schema's namespace, since
  avrogen refuses a schema without one. The example type is `OrderShipped`, because the page's
  `OrderCreated` is a different shape. Registration is per type with `RegisterAsync`, and the default
  form, #10, carries the consequence in a comment. #11's `mappers.Regiter<` (`CS1061`) is fixed
- **The `PublishAsync` sweep, by ruling.** `calls.py` read every `Publish`/`PublishAsync` call in a C#
  fence (**13** at `aec11c1`, **7** now), and a prose grep read every line tying either to a bus,
  broker, queue, topic, transport or the wire. Four places relied on `PublishAsync` reaching a mapper or
  the bus. `DefaultMessageMappers.md` #2 said the default mapper serialises a published event.
  `DispatchingARequest.md` #3 set CloudEvents extensions for a publish. `V10MigrationGuide.md` #26
  asserted that a published event lands on the `InternalBus`. Those three now post.
  `DynamicMessageDeserialization.md`'s practice *"Cache Performance-Critical Paths"* published three
  dummy events to warm the mapper cache, and it is removed: `PublishAsync` maps nothing, and
  `TransformPipelineBuilder`'s cache is keyed per mapper type and per direction, so posting warms only
  the producer's side and puts junk on a topic. The 7 calls left are 2 declarations, 2 in-process
  read-model events, a scheduled local event (`AwsScheduler.md`) and 2 test-double verifications. The
  sweep also found that **CloudEvents extension properties are written only by
  `CloudEventJsonMessageMapper<>`**. Measured: the default `JsonMessageMapper<>` drops them from both
  the request context and `Publication`. `DispatchingARequest.md`, `UsingTheContextBag.md` and
  `CommandProcessorConfigurationReference.md`'s `CloudEventsAdditionalProperties` row now say so.
  `FAQ.md`'s heading, which had a stray `c` and an unclosed code span, is fixed
- **`--explain` on every block the diff touches** (`git diff -U0 aec11c1` hunks against fence
  ranges): **13** blocks, **5** BUILT. The **8** FAILED are `CQRSWithBrighterAndDarker.md` #2 and
  #8, and `DefaultMessageMappers.md` #6 and #10, all same-page and each compiled with its page's blocks (below).
  The other four fail only on names their pages never show: `DefaultMessageMappers.md` #2 and #11,
  `DispatchingARequest.md` #3, and `V10MigrationGuide.md` #26, none of them tranche pages. Each got
  its `using`s, so `pagelint --changed origin/master` reports **0** errors
- **Behaviour, run with controls**, against the released 10.7.0 packages on net10.0:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `InMemoryScheduler.md`: the scheduler's clock is its `TimeProvider` | `FakeTimeProvider`, `SendAsync(5 min)`, `Advance(5 min)` → handled **1**, before `Advance` returns | no advance → **0**; advance 4:59 → **0**, then +1 s → **1**; 2.5 s of wall clock → **0**. Both TimeProvider.Testing 10.7.0 and 10.10.0 |
  | `CQRSWithBrighterAndDarker.md` #8: `Guid.Parse(command.Id)` | 10,000 `Id.Random()` → all parse | `new Id("order-1")` → `FormatException` |
  | `DefaultMessageMappers.md` #4–#6, the page's own blocks and `.avsc`, avrogen 1.12.2, cp-schema-registry 7.9.0, `InternalBus` | `RegisterAsync` + `PostAsync` → 16 bytes, magic byte 0, octet-stream, `MT_EVENT`; round trip → the record and its `Id`; subject `<topic>-value` registered | `Post` → **JSON** (the sync default; the async mapper is not used); another request type → JSON through the default. The earlier sync mapper under `PostAsync` → JSON too |
  | #10, the Avro default | `PostAsync` of an `ISpecificRecord` → Avro | a type that is not one → `ArgumentException`, *"violates the constraint of type 'T'"* |
  | The `Id` in the header | `MapToRequestAsync` → the `Id` restored | the record alone → a different `Id` |
  | `RequestToMessageType` | the partial as `IEvent` → maps | as bare `IRequest` → `ArgumentException`, *"can only map Commands and Events"* |
  | CloudEvents extensions (`DispatchingARequest.md`, `UsingTheContextBag.md`) | `PostAsync` + `CloudEventJsonMessageMapper<>` → in the envelope, from the context bag and from `Publication` | `JsonMessageMapper<>` → absent from body and header, both ways; `PublishAsync` → **0** messages, the local handler runs |

- **The blocks that stay FAILED compile where their world exists** (§ *Blocks that stay FAILED*):
  `CQRSWithBrighterAndDarker.md` #6, #7 and #8 together → **0** errors, and #2 with #7 → **0**. The
  control, #8 with the old `Id = command.Id`, gives `CS0029`, and #2 with the positional token gives
  `CS1503`. `DefaultMessageMappers.md` #4, #5 and #6 with avrogen's half → **0**; without it,
  `CS0311`
- **`attr_mismatch.py` → 7**, before the baseline rows
- **Baseline:** 5 rows. `--report` → exit **0**, *"989 blocks: 293 BUILT, 679 FAILED, 17 SKIPPED"*,
  baseline 293, 45 units, 0 findings
- `linkcheck` 165 files, 0 broken; `versioncheck` 0 stale of 18 across 5; `symbolcheck` 0 findings,
  22 entries, 3 silenced, `--verify-list` clean; `optioncheck` 0 mismatches across 59 tables, 519 rows;
  shape, redirects and `--verify` 161 / 77 / 161. **Pages changed: 11**
  (`git diff --name-only aec11c1..HEAD -- contents`): `InMemoryScheduler.md` and
  `CQRSWithBrighterAndDarker.md` for P0-7, plus `BrighterControlAPI.md`, `BrighterSchedulerSupport.md`,
  `CommandProcessorConfigurationReference.md`, `DefaultMessageMappers.md`, `DispatchingARequest.md`,
  `DynamicMessageDeserialization.md`, `FAQ.md`, `UsingTheContextBag.md` and `V10MigrationGuide.md`,
  by ruling or recurrence

**Task 5.5 — the E4 attribute mismatches, and what stood beside them.** **`attr_mismatch.py` 7 → 1,
exit 1**, the one hit `PipelineValidation.md:250`, the deliberate *Before (error)* example, on its
line; `--plant` → **OK**, exit 0. Six sync attributes on `HandleAsync` became the async form:
`HowServiceActivatorWorks.md` #16 (`UseInboxAsync`), `PipelineValidation.md` #9 and #10,
`PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6 and `V10MigrationGuide.md` #10
(`UseResiliencePipelineAsync`). **Second method:** a Python scan for any of the paired names without
`Async` whose next non-attribute line is `HandleAsync` reads **8 → 2**. The two left are `:250` and
`V10MigrationGuide.md` #8's `[TimeoutPolicy]`, the skipped *Before (V9)* form, whose attribute has
no pair at 10.7.0 and which `attr_mismatch.py` does not read. **BUILT 293 → 295** (+2), both
`FAILED -> BUILT` and nothing else moved in the AC2 diff against `608615a`'s report:
`V10MigrationGuide.md` #9 (the registry) and #12 (an `IRequestContext` implementation). **989 blocks**
on both sides, no fence added or removed. `pagelint` **553 → 543** (−10), per page against a worktree
at `608615a`: `V10MigrationGuide.md` −6 (#9–#14), `PipelineValidation.md` −2 (#9, #10),
`HowServiceActivatorWorks.md` −1 (#16), `ReactorAndProactor.md` −1 (#6; #5 already carried its
`using`s). Pages with nothing BUILT **42 → 41**, `V10MigrationGuide.md` leaving: pages with a FAILED block and
no BUILT one, by `comm` over the two `awk` lists and by a Python join, agreeing. A count of every page
without a BUILT block reads 43 → 42, because it also takes in a page whose blocks are all SKIPPED.
Repair `f4cfd88`; baseline `5c68181`.

- **`:250` says it is wrong in prose.** The sentence under *Async Handler with Sync Attributes* now
  says the example is wrong, that the compiler accepts it, that `ValidatePipelines()` reports it as an
  error, and that without validation the pipeline throws `ConfigurationException` on the first request
- **`PipelineValidation.md` #9, #10 kept what they demonstrate.** The attribute's kind changed, and
  `(step: 0, "RetryPipeline")` — a named argument ahead of a positional one, out of position — became
  `("RetryPipeline", step: 0)`. #9's comments called step 0 *inner* and step 1 *outer*, against the
  page's own *"lower step numbers are outer wrappers"* and the validator's text; now *outer* and
  *inner*. Both fragments are now whole handlers with `using`s, as #7 and #8 are, so `pagelint
  --changed` can read them. The example warning message spelt the attribute names without the
  `Attribute` suffix the validator prints (`AttributeType.Name`, captured below)
- **`V10MigrationGuide.md` § 4 had two defects beside its attribute.** #9 built the pipeline with
  `TryAddBuilder<ResiliencePropertyKey<RequestContext>>`, a generic builder whose pipeline Brighter
  never looks up, and #11 assigned the registry to `PolicyRegistry`, the obsolete Polly v7
  `IPolicyRegistry<string>` (a type mismatch). Now `AddBrighterDefault()` then `TryAddBuilder(name,
  …)`, as `PolicyRetryAndCircuitBreaker.md` shows, into `ResiliencePipelineRegistry`. The sentence
  under #9 names what happens without `AddBrighterDefault`, run below
- **`V10MigrationGuide.md` § 5, the open `IRequestContext` row, closed.** The section listed
  `PartitionKey` and `CustomHeaders` as new properties and #12 implemented `Guid Id`, `ISpan Span`
  and a `Dictionary` `Bag`; 10.7.0 has none of those shapes (`git show 10.7.0:src/Paramore.Brighter/IRequestContext.cs`)
  and has five members the section omitted: `Destination`, `ResiliencePipeline`, `Span` as an
  `Activity`, `FeatureSwitches`, `CreateCopy()`. The section now lists what changed against V9
  (`9.9.13`'s interface: `Bag`, `Policies`, `FeatureSwitches`) and links each new member to
  `UsingTheContextBag.md`. #12 implements 10.7.0's interface and is BUILT; its `CreateCopy` copies
  what the shipped `RequestContext.CreateCopy` copies. #13 was a handler setting properties that do
  not exist; it is now a service that puts the partition key and a header in the `Bag` under
  `RequestContextBagNames` and passes the context to `PostAsync`, run below
- **`IRequestContext.InstrumentationOptions` said *"added in 10.7.0"*.** It is not in the tag
  (`grep -c Instrumentation` → 0) and not on NuGet past 10.7.0; it is in `release_notes.md`'s
  *Master* section, added by `0950f2864`. **Forthcoming, not dead**, so by the 2026-09-05 ruling it
  stays, marked: the heading reads *(after 10.7.0)* and the section carries `> **Not in a released
  package yet.**`, worded as the Replay On Seen pages word theirs. #14 gained its two `using`s;
  `--explain` found `InstrumentationOptions` needs `Paramore.Brighter.Observability`
- **`ReactorAndProactor.md` said mappers have no async variants.** *"Message mappers remain
  synchronous"* under *Proactor Message Mappers*, and a note advising `Task.Run()` wrappers for async
  I/O. At 10.7.0 the Proactor takes `IAmAMessageMapperRegistryAsync` (`Proactor.cs:63`) and the
  Reactor `IAmAMessageMapperRegistry` (`Reactor.cs:63`). #5 is now an `IAmAMessageMapperAsync<T>`,
  and the section says what the run showed: a pump given only the other kind of mapper uses the
  default one, silently. The page's own `:43` already said mappers should be async under a
  Proactor; `:73`, *"Mixing sync and async implementations will cause runtime errors"*, now
  excepts mappers and links the section. Found reading around the E4 hit at `:200`
- **`--explain` on every block the diff touches:** **13**, 2 BUILT. The 11 FAILED name only page
  types and values: `MyCommand`, `OrderCreated`, `OrderPlaced`, `SomeAsyncOperation`, `services`,
  `resiliencePipelineRegistry`; #14 adds the `CS0535`s its `// ... other members` declares. `pagelint
  --changed origin/master` → **0** errors
- **Behaviour, run with controls**, against the released 10.7.0 packages on net10.0 (`valrun`,
  `pumprun`, `regrun`, `ctxrun` under the session scratchpad):

  | Claim | Case → result | Control → result |
  |---|---|---|
  | `PipelineValidation.md` #7 is an error | `Validate()` → 1 error, *"Async handler uses sync attribute 'RejectMessageOnErrorAttribute' at step 0"*; without validation, `PublishAsync` → `ConfigurationException`, *"All handlers in an async pipeline must derive from IHandleRequestsAsync"* | #8 → 0 errors, 0 warnings; publishes |
  | #9 is a warning, #10 is clean | repaired #9 → 0 errors, **1** warning, *"'RejectMessageOnErrorAsyncAttribute' at step 1 is after 'UseResiliencePipelineAsyncAttribute' at step 0"*; repaired #10 → 0, 0 | as published, #9 → 1 error **and** the warning; #10, the *After (fixed)*, → **1 error** |
  | A pump uses only its own kind of mapper | Proactor, sync mapper only → **the default mapper** mapped it; Reactor, async mapper only → **the default mapper** | Proactor, async mapper → the custom async mapper; Reactor, sync mapper → the custom sync mapper. `AddConsumers` + `AutoFromAssemblies` + `InMemoryChannelFactory`, as `InMemoryTransport.md` configures it |
  | `V10MigrationGuide.md` #9: start from `AddBrighterDefault` | own registry without it → `ConfigurationException`, *"missing the CommandProcessor.OutboxProducer resilience pipeline"* | with it → the `[UseResiliencePipelineAsync]` handler runs |
  | #13: the `Bag` keys reach the message | #13 verbatim through `PostAsync` → `PartitionKey` `tenant-42`, header `x-tenant-id` `tenant-42` | `PostAsync` with no context → `PartitionKey` empty, header absent |

- **`--report`** before the baseline rows → exit 1, 2 findings, both *"BUILT, not in the baseline"*;
  after → exit **0**, *"989 blocks: 295 BUILT, 677 FAILED, 17 SKIPPED"*, baseline 295, 45 units,
  0 findings. `linkcheck` 165, 0 broken; `versioncheck` 0 of 18; `symbolcheck` 0, 22 entries,
  3 silenced, `--verify-list` clean; `optioncheck` 0, 59 tables, 519 rows; shape, redirects,
  `--verify` 161 / 77 / 161. **Pages changed: 5** (`git diff --name-only 608615a..HEAD --
  contents`), the five E4 pages
- **Found on the way and put to the maintainer:** `PipelineValidation.md`'s Replay rule row and its
  two Replay example messages describe a forthcoming feature without the *Not in a released package
  yet* marker; `MessageMappers.md`, the mapper's home page, never mentions `IAmAMessageMapperAsync<T>`;
  and `UsingTheContextBag.md`'s handler examples set `Context.Bag` keys without posting. **Ruled
  2026-09-28: into phase 5**, the second pass below

**Task 5.5, second pass — the three items, by ruling.** Pages changed: **5**, `PipelineValidation.md`,
`MessageMappers.md`, `UsingTheContextBag.md`, and `DispatcherConfigurationReference.md` and
`BrighterOutboxSupport.md`, found by the Replay recurrence grep. **BUILT 295 → 295**; **989 → 990 blocks**, one inserted:
`MessageMappers.md` #2, the async mapper, FAILED on its page type `GreetingMade`. The AC2 diff against
the first pass's report reads `MessageMappers.md` #3 `BUILT -> FAILED`, #4 `FAILED -> BUILT` and #7
as a new key: the insert renumbered old #2–#6 to #3–#7, and old #3's baseline row moved to #4
(§ *Splits*). `pagelint` **543 → 537**, all on `UsingTheContextBag.md` (#2–#6 and #18, per page
against a worktree at `ac44085`); the new block carries its `using`s. Pages with nothing BUILT **41**,
unmoved. `attr_mismatch.py` **1**, exit 1, still **`PipelineValidation.md:250`**: the Replay note first
went in as a line after the example messages and moved the hit to `:252`; it now sits in the existing
**Example error messages** line, so nothing above `:250` changed length. `--plant` OK. Repair
`6ddcf7b`, `dc3e5bd`; baseline `4fc4040`. `--report` → exit **0**, *"990 blocks: 295 BUILT, 678 FAILED,
17 SKIPPED"*, baseline 295, 0 findings; `dc3e5bd` moved no verdict.

- **`PipelineValidation.md`, Replay.** `git grep -il causation 10.7.0 -- src` → **0** files; on
  `master`, `Validation/HandlerPipelineValidationRules.cs` and `PipelineValidator.cs`. The rule's row,
  the line introducing the example messages and § *Replay Without Causation Tracking* now say it
  ships after 10.7.0; the section carries the callout as the Replay On Seen pages word it. The other
  rules on the page are in the tag (5.5's run reported two of them); `UseBoxProvisioning` and
  `AddMsSqlOutbox`, in the section's blocks, are too
- **`MessageMappers.md`** gains *Synchronous and Asynchronous Message Mappers*: an
  `IAmAMessageMapperAsync<GreetingMade>` beside the page's sync one, and which path uses which, linking
  `DefaultMessageMappers.md` and `ReactorAndProactor.md`'s section. Written from the run below, all
  four producer calls, with the two pumps from the first pass
- **`UsingTheContextBag.md`.** #3 (partition key), #5 (headers) and #6 (CloudEvents extensions) set
  keys on the handler's own `Context` and posted nothing; #2 and #18 filled a context and passed it to
  `SendAsync`, which makes no message. Each now fills a `RequestContext` and passes it to the `Post`
  or `PostAsync` that makes the message, and a paragraph under *Setting Request Context Explicitly*
  says why, from the run. The Bag-key snippets under *Well-Known Context Bag Keys* and the practices
  are left: they show the key's name, not where it takes effect. *"Headers set here take precedence
  over static header configurations"* stays, now run (`DictionaryExtensions.Merge`, the context's
  entry overwrites the publication's)
- **`--explain` on the seven touched blocks** (#4 as well, `context` a value): page types and values only (`CreateOrderRequest`,
  `OrderCreated`, `MyCommand`, `TenantWorkDone`, `IOrderService`, `ProcessOrderCommand`,
  `OrderProcessed`, `PublishEventCommand`, `BusinessEventRaised`, `GreetingMade`; `tenantId`,
  `criticalHeaders`, `commandProcessor`, `orderCreated`). `pagelint --changed origin/master` → **0** errors
- **Behaviour, run with controls** (`bagrun`, `bagrun2`, `maprun`), 10.7.0, net10.0, `InternalBus`:

  | Claim | Case → result | Control → result |
  |---|---|---|
  | Keys act only on the context passed to the post | handler fills its own `Context.Bag`, `PostAsync(…, requestContext: Context as RequestContext)` → partition key and header on the message | same handler, `PostAsync` with no context → neither. A caller's context passed to `SendAsync` reaches the handler (its key is present) and still → neither, when the handler posts without one |
  | #3's shape, sync | `Post(…, requestContext: context)` inside `Handle` → partition key `tenant-7`, `x-order-priority` `high` over the publication's `DefaultHeaders` value | `Post` with no context → no partition key, the publication's `publication-default` |
  | Each call uses only its own kind of mapper | `Post`, `DepositPost` + `ClearOutbox` on a sync-only type → the custom sync mapper; `PostAsync`, `DepositPostAsync` + `ClearOutboxAsync` on an async-only type → the custom async mapper | the crossed four → **the default mapper (JSON)**, all four |

**Task 5.6 — phase 5 closed, 2026-09-28, branch at `5981b12`.** Every gate run bare, exit code
read before its output:

| # | Gate | Exit | Read | Predicted (5.1) | Agrees? |
|---:|---|---:|---|---|---|
| 1 | `linkcheck` | 0 | 165 files, 0 broken | none | **yes** |
| 2 | `pagelint` | 0 | 0 errors, **537** warnings across 66 pages, 162 pages | 0 errors; 616 → between 582 and 573, then up to −8 from P0-7, recurrences explained | **yes** — 35 in the band, 8 from P0-7, 36 more explained below |
| 3 | shape | 0 | 161 pages, 12 sections, widest 12 of 20, deepest 4 of 4 | none | **yes** |
| 4 | redirects | 0 | 77 entries, 7858 bytes | none | **yes** |
| 5 | `versioncheck` | 0 | 0 stale of 18, across 5 pages | none, scope held at 18 across 5 | **yes** |
| 6 | `optioncheck` | 0 | 0 mismatches, 59 tables, 519 rows | none | **yes** |
| 7 | `--verify` | 0 | 161 predicted = 161 published | none | **yes** |
| 8 | `symbolcheck` | 0 | 0 findings, 22 entries, 161 pages, 3 silenced; `--verify-list` exit 0 | none; `S3LuggageStore.md`'s two opt-outs kept | **yes** |
| 9 | `blockcheck` | 0 | 990: **295** BUILT, 678 FAILED, 17 SKIPPED; 0 findings; **545** reference assemblies; baseline 295; **45** units, 0 violations; 67 pages mapped | BUILT 246–294; SKIPPED 16 plus accepted reasons; 542, no pin change; exit 0; 0 violations | **no — 1 above the ceiling, and the pin grew**, explained below |
| — | `attr_mismatch.py` | 1 | **1**, `PipelineValidation.md:250`; `--plant` exit 0, OK | 7 → 1, exit 1, `:250` | **yes** |
| — | `grep -rn ITimerProvider contents/` | — | **0** lines, read from a file | 4 → 0 | **yes** |

`pagelint --changed origin/master` → exit **0**, 0 errors. `origin/master` is `976e0e0`, the merge of
phase 4.

**Gate 9, reconciled.** Phase 5 moved **61** blocks into the baseline: 28 in 5.2, 3 in its second
pass, 23 in 5.3, 5 in 5.4, 2 in 5.5. Against the report regenerated at `976e0e0` in a worktree
(*"983 blocks: 234 BUILT"*), **50** are on the tranche's 16 pages and **11** off it. Of the 62 blocks
FAILED on the tranche at `976e0e0`, **48** built, **13** stay FAILED and **1** is SKIPPED; the other
two tranche gains are new fences, `CloudEventsReference.md`'s inserted block and
`PostgreSQLBrokerTradeOffs.md` #2, the split. Against 5.1's ceiling of 294, which counted every
reachable block, every hard block but `S3LuggageStore.md` #1, and `InMemoryScheduler.md` #5:

- **−11**, ceiling blocks that did not build: `BuildingAPipeline.md` #2, #3, #4,
  `DarkerConfigurationReference.md` #1, #2, `ParameterizedQueryPatterns.md` #4,
  `ProjectionQueryPatterns.md` #3, `QueryHandlerDependencies.md` #3, `Telemetry.md` #6 (old #5),
  `PostgreSQLMessageBroker.md` #8, each in § *Blocks that stay FAILED*; and
  `DarkerAndBrighterPipelines.md` #1, SKIPPED with an accepted reason
- **+2**, the tranche's two new fences, BUILT
- **+10**, off the tranche and beside `InMemoryScheduler.md` #5, which the ceiling held:
  `Monitoring.md` #1, #2 (ruling), `QueryPipelinePolicies.md` #7 (appended), `NullableReferenceTypes.md`
  #9 (recurrence), `CQRSWithBrighterAndDarker.md` #7 (the `Order` write model, which 5.1 left out),
  `BrighterSchedulerSupport.md` #1, `DefaultMessageMappers.md` #4, #5 (ruling), `V10MigrationGuide.md`
  #9, #12 (beside the attributes)

294 − 11 + 2 + 10 = **295**. **The pin: said, no change; measured, 98 → 100 `PackageReference`s**
(`grep -c`), 542 → 545 reference assemblies — `Microsoft.Extensions.TimeProvider.Testing` and
`Confluent.SchemaRegistry.Serdes.Avro`, for 5.4's two repairs, each its own commit measured alone,
no verdict moved (`07d878b`, `aeb65f4`). **Units 35 → 45**: five in 5.2, five in 5.3 (`git diff
--name-status 976e0e0..HEAD -- tools/blockcheck/scaffold/units`, ten `A`); pages mapped 53 → 67.

**Every FAILED block on the 16 tranche pages is listed**: `after.tsv`'s FAILED keys on those pages
are **15** — the 13 above and two new fences, `AgreementDispatcherRouting.md` #12 (split) and
`Telemetry.md` #1 (inserted, FAILED on the pin) — and `comm` of them against § *Blocks that stay
FAILED* is silent. Corpus-wide, too: every listed row is FAILED in the report.

**Gate 2, reconciled.** Per-page warnings at `976e0e0` (a worktree) against the branch, **−79** in
three groups, and the six task entries above sum to it (17 + 14 + 23 + 9 + 10 + 6):

- **−35 on 11 tranche pages**, which is inside 5.1's band (616 − 35 = 581). Six
  `AgreementDispatcherRouting.md` blocks, #12 among them, open with `// ...`: they declare their
  omission, so they still warn, which is the proviso 5.1 wrote. The one warning left on
  `PostgreSQLMessageBroker.md` is #11, BUILT and untouched, as predicted
- **−19 on 6 P0-7 pages.** **8** are the P0-7 blocks 5.1 named, all of them: `InMemoryScheduler.md`
  #5, `HowServiceActivatorWorks.md` #16, `PipelineValidation.md` #9, #10,
  `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6, `V10MigrationGuide.md` #10, #12.
  The other **11**: `PolicyRetryAndCircuitBreaker.md` −5 (the public-handlers ruling),
  `V10MigrationGuide.md` −6 (#9, #11, #13, #14 beside the attributes, in 5.5; one block each by
  5.3's and 5.4's recurrences, #26 the latter)
- **−25 on 13 pages outside both**, each in its task's entry: `UsingTheContextBag.md` −6 (ruling),
  `DefaultMessageMappers.md` −4 (ruling), `FeatureSwitches.md` −4 (ruling), `Monitoring.md` −2
  (ruling), and −1 each on `AgreementDispatcher.md`, `BrighterSchedulerSupport.md`,
  `ConfiguringOpenTelemetry.md`, `DispatchingARequest.md`, `DynamicMessageDeserialization.md` (the
  removed block), `MigratingToNullableReferenceTypes.md`, `MigratingToPollyV8.md`,
  `NullableReferenceTypes.md`, `PolicyFallback.md`

616 − 79 = **537**, across 66 pages, down from 76.

**AC2, against a `before.tsv` regenerated from `c7329bb` in a worktree** (exit 0, *"989 blocks: 101
BUILT, 872 FAILED, 16 SKIPPED"*, 989 rows): the diff prints **207** lines. **194** are `FAILED ->
BUILT`. The other **13**, each read against § *Splits* or accepted:

- `FAILED -> SKIPPED` `DarkerAndBrighterPipelines.md` #1 — the accepted skip, *"a method signature
  shown for comparison, with no body or class to compile in"*
- `BUILT -> FAILED` `Telemetry.md` #1 and `MessageMappers.md` #3 — each a BUILT block renumbered by
  an insertion above it; both rows moved (§ *Splits*)
- ` -> BUILT` `CloudEventsReference.md` #5, `PostgreSQLBrokerTradeOffs.md` #2,
  `QueryPipelinePolicies.md` #7, `SchedulingAMessage.md` #10 — new keys (§ *Splits*)
- ` -> FAILED` `AgreementDispatcherRouting.md` #12, `CQRSWithBrighterAndDarker.md` #13,
  `DefaultMessageMappers.md` #14, #15, `MessageMappers.md` #7, `Telemetry.md` #6 — new keys, each
  the tail of an insertion or a split (§ *Splits*)

Nine `c7329bb` keys are absent from the report: `AnalyzerSupport.md` #2–#8,
`ConfiguringOpenTelemetry.md` #7 and `DynamicMessageDeserialization.md` #9, the shifts § *Blocks
removed* explains. **Two § *Splits* rows said less than the diff reads, and are now whole:**
`CQRSWithBrighterAndDarker.md`'s called its inserted #7 *"a new key"*, where the ordinal diff reads
#7 `FAILED -> BUILT` and #13 as the new key; `DefaultMessageMappers.md`'s did not say what the diff
reads. So 194 counts some inserted fences as `FAILED -> BUILT` on an old key, which is why phase 5's
own moves were each counted by aligning fences rather than by this diff. **Control, both ways:** the
report against itself prints **0** lines; a copy with `AgreementDispatcherRouting.md` #5 set FAILED
prints **206**, having lost that one; a copy with `AWSSQSConfiguration.md` #1 set FAILED prints
exactly one line beside the known 13 that is not `FAILED -> BUILT`, `BUILT -> FAILED
contents/AWSSQSConfiguration.md 1`.

**The targets, both read before phase 6 walks them.** **BUILT ≥ 250: met, at 295.** **Pages with
nothing BUILT ≤ 60: met, at 41** — requirements' `awk` (`comm -23` of the FAILED and BUILT page
lists) and a Python join over the same report, **41** both. **Said, by 5.1: at most 51, as low as
45. Measured: 41**, four below. All 12 tranche pages with nothing BUILT left the list —
`DarkerAndBrighterPipelines.md` by its skip, not by building — which is 5.1's 45; the other four are
off the tranche: `BrighterSchedulerSupport.md`, `DefaultMessageMappers.md`, `Monitoring.md` and
`V10MigrationGuide.md`. 5.1 set `V10MigrationGuide.md` aside because none of its touched blocks was
predicted to build; #9 and #12 did, in 5.5. No page joined the list.

**Every behavioural block was run with its control** (P0-10): the tables under 5.2, 5.3, 5.4 and
5.5, against released 10.7.0 packages (Darker 4.1.1) on net10.0, with a real broker or registry
where a claim needed one.

**Upstream, filed in phase 5 on the maintainer's word:** BrighterCommand/Brighter#4453, #4454 (5.2's
second pass), #4458 (5.3), #4465 (5.4). Each is stated on its page.

**The PR changes 53 pages** (`git diff --name-only origin/master..HEAD -- contents | wc -l`): **15**
of the 16 tranche pages (`AggregationQueryPatterns.md` was made whole by its unit alone) and **38**
outside it, by P0-7, recurrence and ruling. Beside them: `tools/README.md` (rows 2 and 9, and a
phase 5 paragraph), the baseline, `refs.csproj`, `pages.tsv` (53 → 67 pages mapped), 10 new units,
and this file.

---

## Phase 6 — Acceptance *(8 tasks, one PR, no page touched)*

**Goal:** every criterion decided by its command or its named reader, the page set checked for
widening, both ledgers written, and the residual sentence 018 starts from.

- [x] **Task 6.1:** Walk the three criteria with no instrument by design — AC6, AC8's second half, AC11
  - Input: `requirements.md` § *Acceptance criteria*; `--list-scaffold` per stub unit; the diff's
    `// ...` lines; each PR's recorded runs
  - Output: § *The acceptance walk*, one row each: the reader named, what they read, their verdict.
    Walked first — both criteria ever found unmet at a close were unmarked ones

- [x] **Task 6.2:** Walk the four criteria instrumented by this spec — AC3, AC4, AC5, AC12
  - Input: the red-proofs in § *Phase 1 as executed*; `--classify`; `--list-skips`
  - Output: one walk row each, the command and its output, exit code read first

- [x] **Task 6.3:** Walk AC1, AC2, AC7, AC8's first half, AC9, AC10, AC13, AC14
  - Input: `requirements.md`'s commands; `before.tsv`; § *Splits*; § *Defect ledger*
  - Output: one walk row each; AC2's diff read against § *Splits*; AC14 is all eight other gates,
    bare, against `tools/README.md`

- [x] **Task 6.4:** Run the backwards check
  - Input: `git diff --name-only c7329bb -- contents/ tools/`; § *The tranches*; phase 5's P0-7 pages
  - Output: § *Backwards check* — every changed page is a tranche page or a P0-7 page, and every
    changed `tools/` file is a design deliverable; anything else is named and justified or reverted

- [x] **Task 6.5:** Write the defect ledger
  - Input: § *Defect ledger* rows from phases 2–5
  - Output: the ledger complete — defect, page, recurrence grep, count before, count after (**0**),
    and *found by*: `--classify`, `attr_mismatch`, a run, or re-derivation

- [x] **Task 6.6:** Write the friction ledger
  - Input: the *as executed* sections of phases 1–5
  - Output: § *Friction ledger*, one row per place the workflow itself got in the way, each with
    what 018 should do about it

- [x] **Task 6.7:** Put D4 to the maintainer, with 017's runs as evidence
  - Input: `design.md` D4; every `attr_mismatch.py` run recorded in phases 2–5
  - Output: a § *For the maintainer* entry: the proposal (a `pagelint` rule or a `blockcheck` mode,
    with the `CLAUDE.md` ledger row it would need), the hits it found and when, and the maintainer's
    answer or *not yet ruled*

- [x] **Task 6.8:** Close the spec
  - Input: `README.md` § *Status Checklist*; `--classify` over the whole corpus
  - Output: the README checklist ticked through *Spec closed*; this file's and the README's status
    lines updated; § *What 017 shipped* ending in **one sentence naming the residual**, its figure
    re-derived by `--classify` (P2-1) — the line 018 starts from; `PROMPT.md` updated

### Phase 6 as executed

#### The acceptance walk: the three criteria with no instrument *(task 6.1)*

Walked first, at `960ce6d`. The walker prepares each reading; the verdict is the named reader's.

| # | Who reads it | What they read | Walker's finding | Verdict |
|---|---|---|---|---|
| **AC6** | **the maintainer** | `--list-scaffold`: *"427 identifiers from 45 unit(s) … 67 page(s) scaffolded"*; the 45 unit files (1,391 lines), each stub under a comment naming the block that uses it; **143 stubbed types** in 16 units, the other 29 units supplying values only | Rules 2–4 and rule 1's first half are enforced: `--report` → *"scaffold rule: 45 units checked, 0 violations"*. For rule 1's second half, every page line naming a stubbed type was read — **12** prose lines, **7** in-block comments — and **none** tells the reader to write the type: each says the type is the sample's or another page's. Worth the reader's eye: `TestingQueryHandlersContext.cs` stubs the two handlers under test, with bodies that throw (the page says they are written elsewhere); `TutorialStreamingWithKafkaContext.cs` stubs `GreetingEvent`, which the reader wrote in `TutorialFirstMessage.md` #1 | **Accepted**, the maintainer, 2026-09-28 |
| **AC8**, second half | **the reviewer** | the **44** `// ...` lines the diff `c7329bb..HEAD -- contents/` adds, each mapped to its block; and `pagelint`'s *"is marked `// ...`"* warnings at `c7329bb` and at `HEAD`, per page — **227 → 171** | **38** of the 44 sit in blocks that carry their `using`s. Of the six that do not, five are `AgreementDispatcherRouting.md` blocks that need none — #4, #5, #6, #10, #11 are BUILT with no `using`, #12 fails on `_database` alone. The marked set **grew on three pages**, by six blocks. `QueryPipeline.md` #12, #16, #17 are attribute lists with nothing to decorate (`CS7014`, `CS1002`): no `using` completes them. **Three were marked where the `using` could have been written:** `QueryPipeline.md` #8 (`RetryableQuery`, `Task<>`, `CancellationToken` unresolved), `PolicyFallback.md` #3 (`FallbackPolicy`, `UseResiliencePipeline`), `HowConfiguringTheCommandProcessorWorks.md` #6 (`UseResiliencePipeline`) — each by `--explain`. `PolicyFallback.md` #1 got its `using`s in the same commit (`f2ce226`) | **Unmet on three blocks** — ruled 2026-09-28: *repaired in this PR* (below) |
| **AC11** | **the reviewer** | the **101** rows of the phases' *Claim / Case / Control* tables and their *"Read, not run"* paragraphs, against the **207** repaired blocks whose change is more than a `using` or a fence (`git diff -U0 c7329bb HEAD -- contents/`, hunks against fence ranges), each read with its prose | **119** of the 207 assert behaviour. **89** are covered by a run with its control — about 20 of them by a run of the same claim on a sibling block or page, and two (`DarkerConfigurationReference.md` #4, `ImplementAQueryHandler.md` #16) with a compile failure as the control. **Three table rows have no control**, all 2.5's: unwrap falling back to `DataRef`, `ErrorHandlingOptions.md` #4's commented values, and Compress's threshold (no case). The **30** below are not run with a control | **Unmet** — ruled 2026-09-28: *repaired in this PR* (below) |

**AC11's thirty**, by what is missing — each block once, under the first thing it lacks:

| Missing | Blocks |
|---|---|
| **no run, a claim 017 wrote** | `KafkaConfiguration.md` #20 — *"a round-trip through a UTF-8 string would corrupt the header"*, added at `:692`; the Avro run (5.3) never takes that path |
| **no run, a claim 017 kept** | `QueryPipeline.md` #12, #16, #17 — Darker decorator order and what logging sees of retries; `FeatureSwitches.md` #1–#4 — a switched-off handler skipped, `dontAck` leaving the message (compiled only, § *Defect ledger*); `MessageMappers.md` #6 — the same UTF-8 claim as Kafka's; `InMemoryOutbox.md` #2's comment that `PostAsync` deposits; `AgreementDispatcherRouting.md` #9 — `.Handlers()` mixing agreement and fixed routes; `Telemetry.md` #6 — `TraceIdRatioBasedSampler(0.1)`, OpenTelemetry's claim |
| **half run** | `PolicyFallback.md` #3 and `HandlerFailure.md` #10, #11 — the breaker-around-retry half run, the backstop outside them not; `QueryPipeline.md` #1, #10 — the default retry run, step order and retry-before-fallback not; `BuildingAnAsyncPipeline.md` #2 — the public-handler half run, the async pipeline's order not; `PostgreSQLMessageBroker.md` #5 — the 60 s invisibility; `SchedulingAMessage.md` #5 — `SendAsync(delay)` run, the block's backoff not |
| **run, no control** | `Telemetry.md` #2, #3, #4 — span names and attributes captured from real spans |
| **read, not run** | `SchedulingAMessage.md` #6 (`Proactor.cs:522`), `V10MigrationGuide.md` #12 (`CreateCopy`), `DapperOutbox.md` #2 and `DynamoOutbox.md` #2 (the first is the handler 3.3 ran), `CloudEventsReference.md` #4, #5 (SNS, ASB), `BrighterSchedulerSupport.md` #7's delay |

1 + 11 + 8 + 3 + 7. **Every claim in the last row was read against 10.7.0 source and none has been
found false**; AC11 asks for a run, and these are recorded as read. **Ruled 2026-09-28: both repaired in this PR**, which therefore changes pages; phase 6's *"no page
touched"* no longer holds for task 6.1.

#### AC8 and AC11, repaired under the ruling *(task 6.1)*

**AC8.** The three blocks carry their `using`s where the `// ...` stood, and
`HowConfiguringTheCommandProcessorWorks.md` #5 beside them; its two `UsePolicy` blocks sit under
*Legacy* and are left. Writing `PolicyFallback.md` #3's `using` made its attribute resolve, and so
exposed `CS7036`: `[FallbackPolicy(backstop: true, step: 1)]` omits the required `circuitBreaker`.
Its recurrence is four lines on the page, all repaired; #4, #8 and #10, touched by that, gained
their `using`s and #4 its `return`.

**AC11.** Every one of the thirty run against released packages — Brighter `10.7.0`, Darker
`4.1.1`, net10.0, `Microsoft.Extensions.Hosting` `10.0.10` — in scratch console apps, with Postgres,
MySQL, `amazon/dynamodb-local` and LocalStack in Docker, each container removed after. 2.5's three
rows gained the case or control they lacked (their table, above).

| Claim | Case → result | Control → result | Verdict |
|---|---|---|---|
| Darker: the lowest step runs first (`QueryPipeline.md` #1) | `Log(1) Fallback(2) Retry(3)` → logging, fallback, retry, handler | steps reversed → retry, fallback, logging, handler | true: `PipelineBuilder` sorts `OrderByDescending(Step)`, so the lowest step is outermost |
| Darker: retries exhausted before the fallback (#10) | `Fallback(2) Retry(3)`, always throws → 4 attempts, then 1 fallback | `Retry(2) Fallback(3)` → 1 attempt, 1 fallback; with 2 transient failures still 1 and 1 | true for the block; **the prose beside #1 recommended the control's layout** |
| Darker: logging at step 1 logs retries (#12, #17; the pattern and best-practice prose) | `Log(1) Retry(2)`, 2 transient failures → one *Executing*, one *completed in 156ms*; attempts invisible | `Retry(1) Log(2)` → *Executing* three times | **false**; and #16's *"won't log individual retries"* is the reverse of the truth |
| Brighter: a backstop outside a breaker-and-retry pipeline (`PolicyFallback.md` #3, `HandlerFailure.md` #10, #11) | send 1, 2 → 4 attempts, then `Fallback()` with `InvalidOperationException`; send 3 → no attempt, `BrokenCircuitException` | fallback at step 3 → 1 attempt, no retry; `circuitBreaker: true` → sends 1, 2 throw, send 3 falls back | the ordering true; **the page's *"If the handler throws"* list false** — the original exception reaches Fallback, `BrokenCircuitException` only on later calls |
| Polly's fallback beside `[FallbackPolicy]` (`PolicyFallback.md` #10) | as written → `CS0029`, then `CS1929` (`AddFallback` needs `ResiliencePipelineBuilder<Product>`) | typed on the request with `UseTypePipeline` → substitutes the request, which `Send` discards; typed `<string>` → `ConfigurationException` | **false**; rewritten, the service call through `GetPipeline<Product>` inside the handler: failing service → 4 calls, `CreateOrder with default`; healthy → 1 call, `real` |
| Async pipeline: the lower step runs first (`BuildingAnAsyncPipeline.md` #2) | step 1, step 2, handler | swapped → the other attribute first | true |
| Feature switches (`FeatureSwitches.md` #1–#4) | registry On → runs; Off → skipped; `dontAck: true` in a consumer → message held (`bus depth=1`), runs when switched On | `dontAck: false` → acknowledged and lost (`depth=0`, never runs) | true; **two sentences false**: no registry → `Config` is On; a registry missing the handler → `ConfigurationException`; redelivery is at once on the in-memory transport, not *"after its visibility timeout"* |
| `.Handlers()` mixes agreement and fixed routes (`AgreementDispatcherRouting.md` #9) | route 1 → `Handler1`, 2 → `Handler2`, `OtherCommandHandler` ran | `AutoFromAssemblies()` without the exclusion → *"More than one handler was found"* | true |
| `PostAsync` deposits in the in-memory outbox (`InMemoryOutbox.md` #2) | `EntryCount=1`, `outstanding=0`, 1 on the bus | `PublishAsync` → 0 entries; `DepositPostAsync` → `outstanding=1`, bus 0 | **partly**: deposited *and dispatched at once*; comment rewritten, the unused `_outbox` field removed |
| `CreateCopy` copies what the shipped context copies (`V10MigrationGuide.md` #12) | shipped: a new `Bag` with the entries; `Span`, `Policies`, `ResiliencePipeline`, `FeatureSwitches`, `OriginatingMessage` by reference | `Destination`, `ResilienceContext` → `null` | **partly**: the page's copy omitted `Policies`; added |
| `SendAsync(delay)` backoff (`SchedulingAMessage.md` #5) | gaps 1.03, 2.00, 4.00 s | fixed 0 s → all by 0.03 s; 3 s → 3.04, 3.00, 3.00 s | true |
| A bare `DeferMessageAction` waits `requeueDelay` (#6; `BrighterSchedulerSupport.md` #7) | 5 s, count 3 → gaps 5.11, 5.02 s, 3 attempts, dropped | 2 s, count 5 → 2.0 s gaps, 5 attempts; an explicit 3000 ms → 3.0 s | true; **beside it, false**: *"falls back to immediate requeue if no scheduler configured"* — DI always registers `InMemorySchedulerFactory` (`ServiceCollectionExtensions.cs:195`); with `UseScheduler` removed, still 5 s |
| A schema-registry header survives only as `Raw` (`KafkaConfiguration.md` #20, `MessageMappers.md` #6) | id 200 through a UTF-8 string → `C8` becomes `EFBFBD`; `.Bytes` intact under `UTF8` **and** `Raw`; a MySQL text-column outbox → corrupt under both, `Raw` delivering base64 | id 65 → intact everywhere but the `Raw` text column; `binaryMessagePayload: true` → intact | **the advice false**: the encoding changes only `.Value`; the damage is a text-column outbox, and `binaryMessagePayload` is the repair |
| Telemetry attributes, span name, a handler's tag (`Telemetry.md` #2–#4) | as captured in 5.3 | `None` → only the app's tag; unset → the same as `None`; `All` → adds the body; `PublishAsync` → `create` and two `publish`; a tag outside the handler → on the app span | true; **two omissions**: unset is `None`, and with no `AddProducers` no Command Processor span at all — `CommandProcessorBuilder.Build()` passes no tracer when `_bus == null` (`:304`) |
| `TraceIdRatioBasedSampler(0.1)` (`Telemetry.md` #6) | N=2000, four runs → 8.6–10.1 % | 0.5 → 47.9–49.4 %; 1.0 → 100 % | true |
| Postgres: a received, unacknowledged message is invisible 60 s (`PostgreSQLMessageBroker.md` #5) | B sees nothing at 0–50 s, receives it at 60.2 s | 5 s → 5.0 s; 30 s → 30.1 s; acknowledged → never within 75 s | true |
| CloudEvents names on SNS and ASB (`CloudEventsReference.md` #4, #5) | SNS via LocalStack → `cloudeventheaders` with `souce`, as the page says; ASB, in process through `ConvertToServiceBusMessage` → the page's six names | no CloudEvents options → still written, source `http://goparamore.io/`, type empty | true — ASB is Brighter's hand-off to the SDK, not a broker's delivery |
| Entity and outbox commit together (`DapperOutbox.md` #2, `DynamoOutbox.md` #2) | MySQL → 1 greeting row, 1 outbox row; DynamoDB-local → both present | a throw after the deposit → 0 and 0; absent and absent | true; **beside it**: `DapperOutbox.md` #1 fails to resolve `MySqlTransactionProvider` — below |

**Beside the runs.** `DapperOutbox.md` #1 names `MySqlTransactionProvider` by `typeof` and never
registers the `IAmARelationalDatabaseConfiguration` it is built from, so the command processor
fails to resolve. Surveyed across every `typeof(...)` provider on the site: **15** blocks on **10**
pages lacked what their provider is built from — the relational configuration, the EF Core
`DbContext`, `IAmazonDynamoDB` or `IAmAMongoDbConfiguration` — each now registered in its block;
run, the MongoDB and DynamoDB cases fail without it and resolve with it. `--explain` over the 27
blocks the diff touches then found `EFCoreOutbox.md`'s V9 `Use{DB}Outbox` prose and a fragment
of literal `...`, `CommandProcessorConfigurationReference.md` #18's two, and on the MongoDB pages
`OnResolvingACollection.Create` (the value is `CreateIfNotExists`) and a `MongoClientSettings`
initialiser setting `ConnectionString`, which it has no such property for; all repaired. Given
their `using`s, **4** MongoDB blocks build; `attr_mismatch.py` read **1**, `PipelineValidation.md:250`,
before their rows (`bd95ee0`).

**Gate movement.** Said, by phase 6's heading: *"no page touched"*, so no gate moves. Measured,
once the ruling put the repairs here: `blockcheck` 295 → **299** BUILT (the four above, no other
verdict moved against `960ce6d`'s report), `pagelint` 537 → **524**; the other seven gates bare at
their figures. `tools/README.md` rows 2 and 9 moved to `bd95ee0`.

**For the maintainer, not yet ruled:** at 10.7.0 `CommandProcessorBuilder.Build()` gives the
Command Processor no tracer when there is no external bus (`:304`), so an application with no
`AddProducers` records no Command Processor span — run both ways. Stated on both pages; whether
to file it upstream is the maintainer's.

#### The acceptance walk: the four instrumented criteria *(task 6.2)*

Walked at `631f5e1`, each command's exit code read before its output. `--report` there: exit **0**,
*"990 blocks: 299 BUILT, 674 FAILED, 17 SKIPPED"*, *"scaffold rule: 45 units checked, 0
violations"*, *"0 findings"*.

| # | Command | Exit | Read | Verdict |
|---|---|---:|---|---|
| **AC3** | `python3 tools/blockcheck.py --classify > c1.tsv`, twice; `cmp`; against `probe/run.sh` at the same ref; `refs.txt` moved aside | **0**, **0**; control **2** | 674 rows, *"146 parse (45 pages), 323 import (64 pages), 21 other (13 pages), 19 same-page (11 pages), 35 values (11 pages), 130 page-type (54 pages)"*; the two runs byte-identical (stderr differs only in its temp directory's name). The probe classes the same 674 keys; the disagreements are below. Control: 0 rows, *"no reference list … nothing was checked"*; restored, byte-identical to the first run | **Met** |
| **AC4** | `--classify` over the 75 tranche pages (the § *The tranches* tables); keys against § *Blocks that stay FAILED* | **0** | 44 FAILED blocks, *"1 parse, 9 import, 3 other, 13 same-page, 1 values, 17 page-type"*. **0** not listed; the same 44 keys as `--report`'s FAILED rows on those pages (`cmp`). The list's other **7** rows are off-tranche pages, FAILED, each with its reason: `ConfiguringOpenTelemetry.md` #1, #5, #6, `CQRSWithBrighterAndDarker.md` #2, #8, `DefaultMessageMappers.md` #6, #10. Control: the list less `TutorialFirstCommand.md` #2 prints that key | **Met** |
| **AC5** | `--report` with a plant in `AzureSchedulerContext.cs`, one at a time, the unit restored (`git diff --quiet`) | type **1**; member **1**; real **0** | `class OrderService` (block 9 declares it) → 2 violations, rule 1's *"declared by … block 9"* and rule 3's *"named by no BUILT block"*; `property plantedValue` inside the holder → 1 violation, *"named by no BUILT block"*; verdicts unmoved by either (the AC2 diff against the real report, 0 lines). The real scaffold: 0 violations, `PageContext.cs` among the 45 — it passes, and needs no exception | **Met** |
| **AC12** | `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*' \| wc -l`; `grep -cF '^(\./)?spec/' tools/README.md`; `--list-skips > s` | —; —; **0** | **2** (the old `ls` glob, **1**); **1**; *"17 skipped of 990 blocks, across 7 pages"*, `wc -l < s` → **17**, AC1's SKIPPED; the keys identical to `--report`'s SKIPPED rows (`cmp`). Control: `--list-skips contents/SpannerOutbox.md`, no skip → **2** | **Met** |

**AC3's disagreements with the probe**, all 674 keys joined:

| Probe | `--classify` | Blocks | Why |
|---|---|---:|---|
| page-type / same-page / values / import | the same class | 130 / 19 / 35 / 241 | identical rules |
| other | **parse** | **146** | the design splits out blocks that do not parse. The Roslyn half's `--parse` over the probe's stage reads *"990 blocks, 151 do not parse"*: those 146 exactly, and 5 SKIPPED (`CloudEventsSupport.md` #11, #12, `DarkerAndBrighterPipelines.md` #1, `V10MigrationGuide.md` #4, #19) |
| other | **import** | **82** | the design orders *import* before *other*. **75** name a pinned type beside another diagnostic. **7** reach *import* through an extension method's name alone, and **all 7 are misread**: `AddOpenTelemetry` (×6 — `Telemetry.md` #1, #6, `ConfiguringOpenTelemetry.md` #1, #5, #6, `PostgreSQLMessageBroker.md` #8) is pinned only on `ILoggingBuilder`, and each block calls it on `IServiceCollection`; `Build` (`Logging.md` #4) is pinned only on OpenTelemetry's builders, and the block calls it on `INeedAHandlers` after a `// ...` elision. No `using` builds any of them |
| other | other | 21 | |

The seven are 1.10's blind spot a second time: `--classify` matches a name, not a receiver, as it
matches `Order` to `StackExchange.Redis`. **No class figure is misread on a tranche page because of
it**: the six OpenTelemetry blocks stay FAILED for the pin (§ *Blocks that stay FAILED*, ruling 2 of
5.3), and `Logging.md` is on no tranche. The `Order` reading, ruled at 1.10, now touches **22**
*import* blocks on **10** pages (1.10: 27 on 16), none on a tranche page. Not repaired, under the
same ruling; recorded here for 6.6.

#### The acceptance walk: the eight remaining criteria *(task 6.3)*

Walked at `f46221d`, which is `631f5e1` for every page, unit and tool; each exit code was read before
its output. The tools were built first (`refs.csproj`, then `blockcheck.csproj`, both Release), and
`before.tsv` was regenerated from `c7329bb` in a worktree, as § *The AC2 before-report* says. It read
*"989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED, 0 NOT_COMPILABLE"*, **989** rows, exit **0**.

| # | Command | Exit | Read | Verdict |
|---|---|---:|---|---|
| **AC1** | `python3 tools/blockcheck.py --report after.tsv > out`; the baseline's keys against the report's BUILT keys; pages with a FAILED block and no BUILT one (`comm -23` of the two `awk` page lists) | **0** | *"990 blocks: 299 BUILT, 674 FAILED, 17 SKIPPED, 0 NOT_COMPILABLE"*, *"baseline: 299 blocks required to build"*, *"0 findings, 17 skipped"*, 545 reference assemblies. The baseline's 299 (page, ordinal) keys and the report's 299 BUILT keys are **identical** (`cmp`). Against the target set at design: BUILT **299 ≥ 250**, and pages with nothing BUILT **39 ≤ 60**. That is 41 at 5.6 less `MongoDBInbox.md` and `MongoDBOutbox.md`, which have no baseline row at `5981b12` and four at `bd95ee0` | **Met** |
| **AC2** | the `awk` diff, `before.tsv` against `after.tsv`; and the reverse join, for keys the diff cannot print | **—** | **211** lines. **198** read `FAILED -> BUILT`: 5.6's 194, plus the four MongoDB blocks 6.1 made BUILT. The other **13** are 5.6's thirteen, unchanged, and each has a § *Splits* row or is the accepted skip: two `BUILT -> FAILED` (`Telemetry.md` #1, `MessageMappers.md` #3 — insertions above a BUILT block, whose rows moved), four ` -> BUILT` and six ` -> FAILED` new keys, and `FAILED -> SKIPPED` `DarkerAndBrighterPipelines.md` #1. The reverse join finds **9** `c7329bb` keys that are absent, and § *Blocks removed* lists every one of them. **Control:** `Telemetry.md` #2 flipped in a copy of `after.tsv` → exactly `BUILT -> FAILED contents/Telemetry.md 2`; the report against itself → **0** lines | **Met** |
| **AC7** | `grep -l '^global using' tools/blockcheck/scaffold/units/*.cs \| wc -l`; `python3 tools/pagelint.py --changed origin/master`; the same with `--changed c7329bb` | —; **0**; **0** | **0** of 45 units. `origin/master` is `960ce6d`, the merge base, so the first run covers this PR. The second covers all of 017's diff at once, which is more than *"every tranche PR"* asks for; each phase recorded its own run at its close. Both read *"0 errors"*. **Control:** a plant unit holding `global using System;` → **1**, removed (`git status` clean) | **Met** |
| **AC8**, first half | `python3 tools/pagelint.py \| tail -1`, at `HEAD` and in a `c7329bb` worktree; each debt warning mapped to its block by fence line (`enumerate_blocks()`); the blocks 017 touched, found from `git diff -U0 c7329bb HEAD -- contents/` hunks against fence ranges | **0**, **0** | **743 → 524**, a fall of **219** against **198** blocks newly BUILT. Every one of the 524 maps to a block: 466 FAILED, 41 BUILT, 17 SKIPPED. Of the **322** blocks 017 touched, **295** are out of debt and **27** remain in it. **All 27 are marked `// ...`** (3 BUILT, 22 FAILED, 2 SKIPPED), and those are the lines 6.1 read for AC8's second half. **None** of the touched blocks is in unmarked debt | **Met** |
| **AC9** | `grep -rn 'ITimerProvider' contents/ \| wc -l`; `attr_mismatch.py`; `attr_mismatch.py --plant`; `CQRSWithBrighterAndDarker.md` in AC1's report | —; **1**; **0** | **0**. The one hit is `contents/PipelineValidation.md:250 RejectMessageOnError on a async handler`, the deliberate example, which the page now calls wrong in prose. The plant: *"sync-on-async hit, async-on-sync hit, matched pair silent: OK"*. `CQRSWithBrighterAndDarker.md` #7, the write model declaring `class Order` (`:666`), is **BUILT**, as is #6 | **Met** |
| **AC10** | each row of § *Defect ledger*: its recurrence grep run as written (alternation unescaped from the table), its output read against the row's *After* | — | **116** rows. **64** reproduce their *After* exactly, from a single grep. Every other row with a grep reads what the row says, once its stated qualifier is applied: the hits it names as right (#5, #17, #32, #40, #41, #80, #81), an exclusion (#43, `Timed`), or a count inverted for an omission (#104, **3**). Rows decided by a run, a scan or a reading were read, not re-run. The rulings (*stated*, *documented*, *recorded, not repaired*) read as ruled. **Three rows are not whole.** **#15** (`MessageBody` given a string for its `ContentType`) and **#16** (mapper excerpts missing a member with no `// ...`) still read *"open"*, with no grep. Both were repaired in later phases: every `new MessageBody` on the two pages now takes a `ContentType` (`grep -rnE 'new MessageBody\([^)]*, *(MediaTypeNames\.[A-Za-z.]+\|"[^"]*") *[,)]' contents/` → **0**), and #16 is #83's defect on #83's five blocks, with #83 at **0**. **#100**'s second grep, `TryAddBuilder<`, now reads **1**: `PolicyFallback.md:306`'s `TryAddBuilder<Product>`, 6.1's correct repair for #109. The row's defect is the key type, `TryAddBuilder<ResiliencePropertyKey`, which reads **0**. #24, #55 and #61 name no grep at all | **Met on the pages; unmet in the ledger on three rows.** They go to 6.5, whose output is the ledger complete, and AC10 is re-read there |
| **AC13** | `grep -rn '299 BUILT' --include='*.md' --include='*.yml' --include='*.py' . > f; grep -vcE '^(\./)?spec/' f`; the same for `524 warnings` | —; — | **1** — `tools/README.md:91`, row 9. `524 warnings` → **1**, row 2. **Control:** `0 errors`, a figure several files quote → **7** | **Met** |
| **AC14** | the eight other gates, bare, against `tools/README.md` rows 1–8 | all **0** | `linkcheck` *"165 files checked"*, 0 broken. `pagelint` *"0 errors, 524 warnings … across 162 pages"*. Shape *"161 pages, 12 sections, deepest 4 of 4 segments, widest 12 of 20"*. Redirects *"77 entries, 7858 bytes"*. `versioncheck` *"0 stale pins of 18 examined across 5 page(s)"*. `optioncheck` *"0 mismatches across 59 tables and 519 rows"*. `--verify` *"predicted 161, published 161, 161 agree"*. `symbolcheck` *"22 entries, 161 pages checked, 3 silenced"*, and `--verify-list` clean. Every figure is its row's | **Met** |

**Seven met, and AC10 met on the pages.** Its ledger has three rows that do not say what the pages
now say. With 6.1's three and 6.2's four, every criterion has now been walked. AC10's ledger is the
one reading left open, and it closes with 6.5.

#### Backwards check *(task 6.4)*

Run at `2fdb256`. `git diff --name-only c7329bb -- contents/ tools/` lists **116** pages and **44**
tool files. Every page change is a modification: 017 added, removed or renamed no page, so
`SUMMARY.md` is untouched, as § *SUMMARY.md changes* said. Nothing outside `contents/`, `tools/`
and `spec/` changed (`git diff --name-only c7329bb | grep -vE '^(contents|tools|spec)/'` → **0**).

**The pages.** Of the 116, **64** are tranche pages and **7** are the P0-7 pages. The P0-7 pages are
`CQRSWithBrighterAndDarker.md`, `HowServiceActivatorWorks.md`, `InMemoryScheduler.md`,
`PipelineValidation.md`, `PolicyRetryAndCircuitBreaker.md`, `ReactorAndProactor.md` and
`V10MigrationGuide.md`, and all seven changed. The tranche list is the 75 pages of § *The
tranches*, taken from its four tables. **11** tranche pages have no diff, and each has a reason:

- **Made whole by a unit alone:** `AggregationQueryPatterns.md`, `ClaimCheck.md`,
  `MSSQLTransportInboxAndOutbox.md`, `RelationalDatabaseConfigurationReference.md` and
  `TutorialStreamingWithKafka.md`
- **Left FAILED:** the other six (`PaginationQueryPatterns.md`, `ReplayOnSeenReference.md`,
  `ReturningResultsFromAHandler.md`, `TutorialDurableOutbox.md`, `TutorialFirstCommand.md`,
  `TutorialFirstMessage.md`). Each FAILED block they carry has a row in § *Blocks that stay
  FAILED*: same-page, unit rule 1, or P2-2

**45 pages are neither.** None is reverted. Each one is reached by a § *Defect ledger* row, found
on a tranche or P0-7 page and repaired wherever its recurrence grep landed (constraint 6), or by a
ruling the maintainer gave. *Rows* below are the ledger rows that reach the page. Most name it in
their *Page* column. **#8** (`requeueCount`) reaches its pages through its grep, which lists 17
pages at `c7329bb`, and eight of them are here. The two phase 2 mapper-member lines on
`CloudEventsSupport.md` and `MigratingToNullableReferenceTypes.md` recur **#7**, whose *Page*
column says only *"11 pages"*. *Phases* are
the phases whose commits touched the page (`git log c7329bb..HEAD`); 6.1 is phase 6's one
page-changing task.

Each (page, phase) pair was checked against the *Found by* column of the rows naming it. Four
pairs did not match, and each was read in its commit:

- **`CloudEventsSupport.md` and `MigratingToNullableReferenceTypes.md`, phase 2:** a mapper's
  `Context` member (#7)
- **`HowConfiguringTheCommandProcessorWorks.md`, 6.1:** AC8's `using`s
- **`QueryPatterns.md`, phase 5:** #30's `circuitBreakerName` argument, recurring on a page the row
  already names

| Page | Diff | Phases | Rows |
|---|---|---|---|
| `AgreementDispatcher.md` | +12 −6 | 5 | #49, #70 |
| `AwsScheduler.md` | +1 −1 | 3 | #23, #26 |
| `AWSSQSConfiguration.md` | +16 −5 | 2, 3 | #8, #17, #19 |
| `AWSSQSMigrateToV10.md` | +18 −6 | 3 | #23 |
| `AzureServiceBusConfiguration.md` | +1 −1 | 2 | #8 |
| `BasicConcepts.md` | +2 −2 | 2 | #8 |
| `BrighterBasicConfiguration.md` | +24 −8 | 3, 4, 6.1 | #17, #31, #115 |
| `BrighterInboxSupport.md` | +6 −0 | 4 | #38 |
| `BrighterOutboxSupport.md` | +1 −1 | 5 | #103 |
| `BrighterSchedulerSupport.md` | +20 −10 | 2, 5, 6.1 | #8, #9, #26, #90, #111 |
| `CloudEventsSupport.md` | +34 −22 | 2, 5 | #7, #70, #78 |
| `CommandProcessorConfigurationReference.md` | +19 −7 | 3, 5, 6.1 | #17, #57, #94, #115 |
| `ConfiguringOpenTelemetry.md` | +83 −54 | 5, 6.1 | #75, #77, #113 |
| `DarkerBasicConfiguration.md` | +4 −5 | 5 | #50, #58 |
| `DefaultMessageMappers.md` | +95 −37 | 5 | #82, #91, #92, #93 |
| `DispatcherConfigurationReference.md` | +15 −15 | 2, 4, 5 | #8, #9, #31, #103 |
| `DispatchingARequest.md` | +8 −2 | 5 | #93, #94 |
| `DynamicMessageDeserialization.md` | +1 −14 | 5 | #70, #93 |
| `EFCoreOutbox.md` | +25 −13 | 6.1 | #115 |
| `FAQ.md` | +24 −4 | 2, 3, 5 | #16, #27, #47, #49, #70, #83, #95 |
| `FeatureSwitches.md` | +30 −10 | 5, 6.1 | #57, #61, #110 |
| `FirestoreInbox.md` | +2 −0 | 4 | #38 |
| `HandlerFailure.md` | +10 −8 | 2, 5, 6.1 | #8, #59, #110 |
| `HowConfiguringTheCommandProcessorWorks.md` | +19 −6 | 5, 6.1 | #59; in 6.1, AC8's ruling — `using`s for a block marked `// ...` |
| `ImplementAQueryHandler.md` | +9 −3 | 5 | #30, #51 |
| `ImplementingExternalBus.md` | +6 −1 | 2 | #6 |
| `InMemoryOptions.md` | +8 −2 | 3 | #17, #21, #26 |
| `KafkaConfiguration.md` | +37 −10 | 2, 3, 5, 6.1 | #6, #8, #15, #17, #22, #82, #112 |
| `MessageMappers.md` | +45 −4 | 2, 5, 6.1 | #6, #15, #82, #104, #112 |
| `MessageTransforms.md` | +2 −2 | 2 | #6 |
| `MigratingToNullableReferenceTypes.md` | +5 −1 | 2, 5 | #7, #84 |
| `MigratingToPollyV8.md` | +20 −8 | 5 | #57, #59 |
| `MongoDBInbox.md` | +18 −8 | 4, 6.1 | #38, #116 |
| `MongoDBOutbox.md` | +33 −8 | 6.1 | #115, #116 |
| `Monitoring.md` | +93 −49 | 5 | #47, #56, #62 |
| `NullableReferenceTypes.md` | +37 −4 | 2, 5 | #6, #16, #83, #84 |
| `OutboxArchiver.md` | +3 −1 | 2 | #6 |
| `PolicyFallback.md` | +57 −26 | 5, 6.1 | #59, #60, #88, #106, #108, #109 |
| `QueryPatterns.md` | +1 −1 | 5 | #30 |
| `QueryPipeline.md` | +46 −35 | 5, 6.1 | #30, #59, #107 |
| `RabbitMQConfiguration.md` | +4 −1 | 2, 3 | #8, #17 |
| `Routing.md` | +24 −3 | 2, 5 | #6, #16, #83 |
| `SpannerInbox.md` | +2 −0 | 4 | #38 |
| `SweeperCircuitBreaking.md` | +99 −61 | 4, 6.1 | #43, #44, #45, #46, #115 |
| `UsingTheContextBag.md` | +51 −17 | 5 | #94, #105 |

**The tools.** Every changed file in `tools/` is a deliverable in `requirements.md` §
*Deliverables*:

| Deliverable | Files changed | Verdict |
|---|---|---|
| `tools/blockcheck.py` | modified | `--classify`, `--list-skips`, `--explain` |
| `tools/blockcheck/Program.cs` | modified | the type dump, the unit-rule check, the `--explain` fix |
| `tools/blockcheck/scaffold/units/*.cs`, `pages.tsv` | **31** units added, **7** modified (`PageContext.cs` among them); `pages.tsv` | the row says *"for the tranche pages"*. **60** pages map to a changed unit. **58** are tranche pages. The other two are `AWSSQSConfiguration.md` (`TransportConfigurationContext.cs`, shared with five phase 3 transport pages, at `7edaada`) and `SweeperCircuitBreaking.md` (`SweeperCircuitBreakingContext.cs`, mapped before 017, grown at `a61893b` for the phase 4 ruling on #43–#46). Both are serving pages already reached by ledger rows |
| `tools/blockcheck/baseline.tsv` | modified | one row per newly BUILT block, keys identical to the BUILT keys (AC1, 6.3) |
| `tools/blockcheck/refs/refs.csproj` | modified, **71 → 100** `PackageReference`s | **Beyond the row.** The row says *"P1-1 only"*, and P1-1 is **24** of the 29 packages (1.8, `23aa74f`). The other five were each added in their own commit, measured alone with no page changed, and merged in a phase PR the maintainer signed off: `Npgsql.EntityFrameworkCore.PostgreSQL`, `TickerQ.Dashboard` and `TickerQ.EntityFrameworkCore` (phase 4, `bd2b1ed`); `Microsoft.Extensions.TimeProvider.Testing` (`07d878b`) and `Confluent.SchemaRegistry.Serdes.Avro` (`aeb65f4`), both 5.4. None is removed. Removing them would un-build the blocks they judge |
| `tools/README.md` | modified | rows 2 and 9, the corrected commands, the new modes |

**Result.** No change is outside the check's two sets without a reason on record: 45 pages are
reached by ledger rows or a ruling, and two units serve those pages. Five pin packages went beyond
P1-1's letter, each measured alone and signed off in its phase's PR. Nothing is reverted.
**Control:** the join that finds each page's rows, run over the 45, prints nothing. Run again
with `Glossary.md` added, a page no ledger row names and no 017 commit touched, it prints
*"NO ROW: Glossary.md"*, and nothing else.

#### The defect ledger complete *(task 6.5)*

Written at `ddcf445`, from 6.3's AC10 residue. No page, tool or gate touched. Six rows changed, one
more was corrected on the way, and two probes were added beside `v4scan.py`. Every *Before* was
re-read at `c7329bb` (`git grep` on that tree, or the probe with `--root` over `git archive`), and
every *After* at `HEAD`.

| Row | Was | Now | Before → after |
|---|---|---|---|
| **#15** | *"open"*, no grep | **#82**'s defect on #82's two blocks, repaired at `154f78c` (5.3). Given #82's grep | **2 → 0** |
| **#16** | *"open"*, no grep | **#83**'s defect on #83's five blocks. Given #83's scan | **5 → 0** |
| **#24** | no grep | `asyncscan.py`, its `CS1997` lines. At `c7329bb` it also finds `ImplementingAsyncHandler.md:50`, the `Task` return #1 already records | **1 → 0** |
| **#55** | no grep | Three parts. The write model is block-local: `o.Customer` is read on 5 other pages, each against its own block's model, and only a compile decides it. The `{ /* ... */ }` requests: a grep, **4 → 0**. Controller actions outside a class: a grep, **4 → 2** (below) | **2** blocks **→ 0** |
| **#61** | no grep | `asyncscan.py`, its `CS4032` lines | **1 → 0** |
| **#83** | *"reading, from 5.1's list"* | `mapperscan.py`. At `c7329bb` it reads **13**: #83's five, and eight missing only `Context`, which are **#7**'s | **5 → 0** |
| **#100** | `TryAddBuilder<` → **1** | `TryAddBuilder<ResiliencePropertyKey`: the key type is the defect, and a builder for the result type is right (`PolicyFallback.md`'s `TryAddBuilder<Product>`, 6.1's #109) | **2 → 0** |

**The probes, each with its control.** `asyncscan.py` finds both of its defects at `c7329bb` and
nothing at `HEAD`. It failed its first control, missing `FeatureSwitches.md` #2 twice over: the
`= default` parameter tripped an exclusion meant for `=>`, and the block's fence was written
`` ``` csharp ``, with a space, which the fence pattern did not allow. The first run also matched
three primary-constructor classes as methods. `mapperscan.py`, before its two exclusions,
read **16** at `c7329bb` and **3** at `HEAD`. The three are `V10MigrationGuide.md` #19, a skipped V9 form, and
`MigratingToNullableReferenceTypes.md` #22 and `OutboxArchiver.md` #4. Both of those carry a
`// ...` and were given `Context` in phase 2 with the other member left out. #83's defect is a
member missing *"with no `// ...` to say so"*, and `pagelint` reads the marker per block, so the
scan does the same and skips what `blockcheck` skips.

**#55's two survivors are residual, not recurrences.** `QueryPatterns.md` #5 and
`ShowMeTheCode.md` #1 show a controller action with no controller. Both pages are off-tranche and
both blocks are FAILED, and `--classify` calls each `import`. Design § P0-6 names this shape: a
method shown without its class is a wrapper to make whole, not an API defect. They are 018's.

**One correction outside the six.** 6.4 attributed the phase 2 `Context` lines on
`CloudEventsSupport.md` and `MigratingToNullableReferenceTypes.md` to #83. The scan's split shows
they are **#7**'s (*"A mapper class without `IRequestContext? Context`"*). #7's *Page* column
reads only *"11 pages"*, so no join by name could find it. 6.4's paragraph, its list and its two
table rows now say #7.

**AC10, re-read.** The seven rows above were run as written, alternation unescaped: every grep
and probe reads the row's *After*, and #55's second grep reads its two named survivors. The
other 109 rows are as 6.3 read them. All 116 rows parse as seven cells. **Met.**

#### The friction ledger *(task 6.6)*

Written at `549d36f`, from the *as executed* sections of phases 1–6 and the session notes behind
them. No page, tool or gate touched. **Fourteen entries, 67–80**, in § *Friction ledger*, each with
what 018 should do, and two recurrences left unnumbered (53, and 58's family). The ledger stands at
**80**. The count is re-derived by the grep under the table (**14**), and the same grep for an
**81** reads **0**. Every row parses as three cells.

The two costliest entries both concern *when* a check runs. **#67:** the ordinal key made every
insertion a hand-aligned diff. **#68:** the two reading criteria were first read at acceptance and
both were unmet there. **#70** puts the name-not-receiver blind spot, ruled at 1.10, back to the
maintainer as a proposal for 018, with 017's three families as its evidence. It asks for a new
ruling and does not reopen the old one.

#### For the maintainer: D4, `attr_mismatch` as a standing gate *(task 6.7)*

Written at `adc1005`. No page, tool or gate touched. Design D4 held one question to the close:
should `probe/attr_mismatch.py` become a standing gate, and if so in which instrument?

**What it guards against.** The probe catches a sync handler attribute on `HandleAsync`, or an
`…Async` one on `Handle`. The compiler accepts both. Brighter throws `ConfigurationException` when it
builds the pipeline (design E4, run with a control), and `ValidatePipelines()` reports it. So
`blockcheck` cannot see it: a block that carries the defect and compiles is written into the
baseline and certified. That is why design rule 8 ran the probe in every tranche PR.

**The runs.** Twenty-five in phases 2–5, each before its task's baseline rows (obligation 15),
with the design's run and the runs after them for context:

| When | Runs | Reading |
|---|---:|---|
| Design E4; § 2 at `c7329bb` | 2 | **7**, exit 1 — 6 defects on 5 pages, 1 deliberate |
| Phase 2, 2.1–2.6 | 6 | 7 each |
| Phase 3, 3.1–3.6 | 6 | 7 each |
| Phase 4, 4.1–4.5 | 5 | 7 each |
| Phase 5, 5.1–5.4 and 5.2's second pass | 5 | 7 each; two hits' lines moved (`:190` → `:200`, `:326` → `:359`) |
| 5.5 | 1 | **7 → 1**, the six repaired |
| 5.5's second pass, 5.6 | 2 | 1, `PipelineValidation.md:250` |
| 6.1's repairs, 6.3's AC9, and 6.7 at `adc1005` | 3 | 1, exit 1; `--plant` → OK, exit 0 |

**What the runs show.**

- **Every defect it found was found at the design, and it found nothing after.** The count never
  rose: no 017 repair wrote a new mismatch, and none made a mismatched block BUILT before its
  repair. Obligation 15 held, but no case ever tested it.
- **The risk it guarded is still latent.** The six repaired blocks are all still FAILED, and so is
  `:250`: `baseline.tsv` holds none of `HowServiceActivatorWorks.md` #16, `PipelineValidation.md`
  #7, #9, #10, `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6 or
  `V10MigrationGuide.md` #10. Suppose an edit reintroduces a mismatch into one of them. The PR
  that later makes that block build would baseline the broken form, and nothing would say so.
- **Its scope is right, and narrow.** It reads Brighter's `Handle`/`HandleAsync` only: Darker 4.1.1
  has no async twins, and the first run's six Darker false positives were removed at the design.
  An attribute on a class is `CS0592`, which the compiler already reports (`InMemoryInbox.md` #2,
  4.1). An attribute with no pair, like `V10MigrationGuide.md` #8's V9 `[TimeoutPolicy]`, has
  nothing to mismatch.
- **The pair list is version-bound.** 13 names, read from 10.7.0. `master` at `2461094a6` has the
  same 43 attribute classes, so the list is the same today. A future release may change it.
- **It named its deliberate hit by line**, and that line shaped where prose could go (friction #79).

**The proposal: `pagelint` rule 8, not a `blockcheck` mode.**

1. **It reads text, and must read FAILED blocks.** The six were FAILED when found. A `blockcheck`
   mode would build the .NET instrument to run a regex. It would also run only in CI's `blocks`
   job, beside a verdict set that means *compiles*.
2. **The probe is already half a `pagelint` rule.** It reads blocks through `pagelint.Page`, as
   016 friction 53 requires, and needs no pin to run.
3. **The opt-out is per block, with a reason.** A page-wide marker like `allow-serviceactivator`
   would also have silenced `PipelineValidation.md` #9 and #10, two real defects sitting beside
   the deliberate #7. So the proposal follows `blockcheck`'s form: the line before the fence
   carries `<!-- pagelint: attr-mismatch-intended <reason> -->`, and the reason is mandatory.
   `PipelineValidation.md` #7 gets that marker, and AC9's *"exactly one hit"* becomes **0**
   (friction #79).
4. **The pairs live beside `APPLIES_TO`** as a `PAIRED` tuple in `tools/pagelint.py`. Its comment
   holds the `git grep` that derives it, so the version bump that edits `APPLIES_TO` re-derives
   `PAIRED` in the same edit.
5. **The red-proof moves with it.** `pagelint` has no plants today. The probe's `--plant` (two
   cases that must hit, one that must not) becomes `pagelint --plant-attr` or a plant file, run in
   CI.

**What it needs.** One `CLAUDE.md` ledger row, and a short § *Handler attributes match their
handler* under *Page Conventions*:

| Convention | Rule | Repo-wide | `--changed` |
|---|---|---|---|
| A handler attribute's kind matches its method's: sync on `Handle`, `…Async` on `HandleAsync` | 8 (`ATTRIBUTE KIND`) | error | error |

Beside the row, it needs `tools/README.md`'s `pagelint` figures and the marker on
`PipelineValidation.md` #7. `probe/attr_mismatch.py` would then be retired, with its history kept
here. Its natural home is 018's phase 1, where the instruments are built.

**The maintainer's answer: ruled 2026-09-29, *`pagelint` rule 8, in 018*** — the proposal as
written: per-block opt-out with a reason, `PAIRED` beside `APPLIES_TO`, the ledger row, the
red-proof carried over, and the probe retired. 017 builds nothing; the rule is an input to 018's
phase 1.

#### What 017 shipped *(task 6.8)*

Measured at `e0385b4`, after building both projects. `--report` → exit **0**, *"990 blocks: 299
BUILT, 674 FAILED, 17 SKIPPED"*, *"baseline: 299 blocks required to build"*, *"scaffold rule: 45
units checked, 0 violations"*, *"0 findings, 17 skipped"*. `--classify` → exit **0**, 674 rows.

| | `c7329bb` (016's close) | Now |
|---|---:|---:|
| **BUILT**, and the baseline that holds them | 101 | **299** |
| FAILED / SKIPPED, of 989 → 990 blocks | 872 / 16 | **674 / 17** |
| Pages with a FAILED block / with nothing BUILT | 140 / 97 | **88 / 39** |
| `pagelint` `using` debt | 743 blocks, 115 pages | **524 blocks, 66 pages** |
| Scaffold units / pages mapped | 14 / 15 | **45 / 67**; 427 identifiers, 143 stubbed types |
| Packages pinned in `refs.csproj` | 71 | **100** |

| | |
|---|---|
| **The instruments** | `--classify`, which is committed, deterministic and red-proofed (AC3), and has replaced 016's uncommitted 102 and 364. `--list-skips` and `--explain`. The unit rule enforced on every `--report` (AC5). 016's AC3, AC7 and AC10 commands corrected, each with a control (AC12) |
| **The pages** | **116** changed under `contents/` (`git diff --name-only c7329bb -- contents/`): **64** of the 75 tranche pages (the other 11 made whole by a unit or listed FAILED), all 7 P0-7 pages, and 45 more, each reached by a defect-ledger row (6.4). PRs #190–#194, and phase 6's |
| **The ledgers** | **116** defect rows, each with its recurrence grep or probe, and *After* **0** (AC10, 6.5). Friction entries **67–80** (6.6) |
| **Upstream** | BrighterCommand/Brighter#4453, #4454, #4458, #4465, each filed on the maintainer's word and stated on its page |
| **Ruled for 018** | D4: `attr_mismatch` becomes `pagelint` rule 8 (6.7) |

**Open at the close, both the maintainer's:**

- **AC8's second half and AC11.** The named readers found them unmet at 6.1. Under the 2026-09-28
  ruling, they were repaired in this PR (§ *AC8 and AC11, repaired under the ruling*), and the
  walk rows still carry the 6.1 verdict. Re-reading the repairs is this PR's review.
- **6.1's `CommandProcessorBuilder` finding.** With no external bus, the Command Processor has no
  tracer. It is stated on both pages; filing upstream is *not yet ruled*.

**Where the residual is**, by `--classify` joined to § *The tranches*. The 75 tranche pages hold
**44** FAILED blocks on 24 pages, each named in § *Blocks that stay FAILED* (AC4). The other **630**
are on **64** pages no tranche reached, and 36 of those pages have nothing BUILT. By class, those
630 are 314 *import*, 145 *parse*, 113 *page-type*, 34 *values*, 18 *other* and 6 *same-page*.
*Import* is a lower bound on the work: at 1.10, **162** blocks it called *import* had a defect
behind the missing `using` (friction #71).

**The residual, the line 018 starts from:** **674 of 990 C# blocks still do not compile against
the released packages, and 630 of them sit on the 64 pages 017's tranches never reached — 314 of
those name a pinned type they never import, 145 do not parse and 113 need a type their page never
shows — so 018 first builds `pagelint` rule 8 and gives `--classify` its second compile, and
then tranches those 64 pages by what that instrument finds.**

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
| `ReplayOnSeenReference.md` | 1 | `CS0117` `RequestContextBagNames.CausationId`; `CS0246` `ProcessPayment`; `CS0103` `_commandProcessor`, `batchId`, `orderId` | P2-2: `CausationId` is on Brighter `master` (`RequestContextBagNames.cs:143`), in no release. The pin bump brings it in through the ratchet; the other names are the handler's, and want a unit then | 4 |
| `AgreementDispatcherRouting.md` | 12 | `CS0103` `_database` | the ❌ example calls a database the page never shows, through a type no block names, so unit rule 1 admits no stub. Split from #11 (§ *Splits*) | 5 |
| `BuildingAPipeline.md` | 2 | `CS0246` `GreetingCommand`, `RequestLogging` | same-page: block 3 declares `RequestLoggingAttribute`, and the next sentence says so (*"We implement the **RequestLoggingAttribute** by creating our own Attribute class"*). A `using` of Brighter's `Logging.Attributes` builds it against Brighter's attribute instead | 5 |
| `BuildingAPipeline.md` | 3 | `CS0246` `RequestLoggingHandler<>` | same-page: block 1 declares it. A `using` of Brighter's `Logging.Handlers` builds it against Brighter's handler instead. Blocks 1–3 compile together and run (§ *Phase 5 as executed*, 5.2) | 5 |
| `BuildingAPipeline.md` | 4 | `CS0246` `MyCommand`, `MyCommandHandler`, `MyLoggingHandler`; `CS0103` `log` | **unit rule 1**: the section tells the reader to write the handlers it chains (*"You can derive from **RequestHandler\<T\>** and call **base.Handle()**"*). Run in scratch with them written | 5 |
| `DarkerConfigurationReference.md` | 1 | `CS0246` `Program` | instrument: `typeof(Program).Assembly` takes the `statements` wrapper; Q2 (1.8) rules out a stub. Builds as a `Program.cs` against Darker 4.1.1 in scratch, **0** errors | 5 |
| `DarkerConfigurationReference.md` | 2 | `CS0246` `Program` | as #1 | 5 |
| `ParameterizedQueryPatterns.md` | 2 | `CS0246` `GetCustomerByEmailQuery` | same-page: block 1 declares it; block 2 is *"**Handler Example:**"* after it. The six blocks compile together, with the never-shown entity stubbed, at **0** errors | 5 |
| `ParameterizedQueryPatterns.md` | 4 | `CS0246` `GetOrdersByCustomerQuery`, `OrderSummaryDto` | same-page, once its `using System.Threading.Tasks` is right: block 3 declares both; *"**Handler with optional filters:**"* | 5 |
| `ParameterizedQueryPatterns.md` | 6 | `CS0246` `SearchProductsQuery`, `ProductDto` | same-page: block 5 declares both; *"**Handler with multiple optional criteria:**"* | 5 |
| `ProjectionQueryPatterns.md` | 3 | `CS1513`, `CS0103` `Select` | parse — a fragment: the `.Select(…)` of block 2's handler with no receiver, under *"Database-computed fields"*. The reader has the whole in block 2 | 5 |
| `QueryHandlerDependencies.md` | 3 | `CS0246` `Program`; `CS0103` `builder` | instrument, as `DarkerConfigurationReference.md` #1; `builder` is not stubbed, since no BUILT block would name it. Builds as a `Program.cs` against Darker 4.1.1, **0** errors | 5 |
| `Telemetry.md` | 1 | `CS0234` `Paramore.Brighter.Extensions.Diagnostics`; `CS1061` `AddOpenTelemetry` | pin: needs `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics`, which `refs.csproj` does not carry; ruled no pin change (5.3, ruling 2). Compiles in scratch against the released packages, **0** errors | 5 |
| `Telemetry.md` | 6 | as #1 | pin: needs `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics`, which `refs.csproj` does not carry; ruled no pin change (5.3, ruling 2). Compiles in scratch against the released packages, **0** errors | 5 |
| `PostgreSQLMessageBroker.md` | 8 | as `Telemetry.md` #1 | pin: needs `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics`, which `refs.csproj` does not carry; ruled no pin change (5.3, ruling 2). Compiles in scratch against the released packages, **0** errors | 5 |
| `ConfiguringOpenTelemetry.md` | 1 | as `Telemetry.md` #1 | off the tranche, rewritten by ruling 1. pin: needs `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics`, which `refs.csproj` does not carry; ruled no pin change (5.3, ruling 2). Compiles in scratch against the released packages, **0** errors | 5 |
| `ConfiguringOpenTelemetry.md` | 5 | as `Telemetry.md` #1 | as #1 | 5 |
| `ConfiguringOpenTelemetry.md` | 6 | as `Telemetry.md` #1 | as #1 | 5 |
| `S3LuggageStore.md` | 1 | `CS0234` `Paramore.Brighter.Transformers.AWS.V4`; `CS0246` `S3LuggageStore`, `S3LuggageOptions`, `AWSS3Connection` | pin (D3): the pin carries the V3 AWS transformer package, not `.V4`. Compiles in scratch against `Paramore.Brighter.Transformers.AWS.V4` 10.7.0, **0** errors, with `credentials` a parameter | 5 |
| `CQRSWithBrighterAndDarker.md` | 2 | `CS0246` `Order`, `OrderItem`, `IOrderRepository`, `OrderPlacedEvent`; `CS0103` `OrderStatus` | same-page: #7 shows the write model, and the prose above #2 links there. #2 with #7 → **0** errors | 5 |
| `CQRSWithBrighterAndDarker.md` | 8 | `CS0246` `PlaceOrderCommand`, `IOrderRepository`, `IProductRepository`, `Order`, `OrderItem`, `OrderPlacedEvent`; `CS0103` `OrderStatus` | same-page: #6 declares the command, #7 the write model. #6–#8 together → **0** errors; the task's *"showing `Order`"* | 5 |
| `DefaultMessageMappers.md` | 6 | `CS0246` `Orders`, `OrderShipped`, `AvroMessageMapperAsync<>`; `CS0103` `services` | same-page: #4 declares the mapper, #5 half of `OrderShipped`; avrogen generates the other half from the page's `.avsc`. #4–#6 with it → **0**; without it `CS0311` | 5 |
| `DefaultMessageMappers.md` | 10 | `CS0246` `Orders`, `OrderShipped`, `AvroMessageMapperAsync<>`; `CS0103` `services` | same-page, as #6; run as the Avro default (5.4's table) | 5 |

## Splits

*One row per split fence: page, old ordinal, new ordinals.*

| Page | Old # | New # | Why | Task |
|---|---:|---:|---|---:|
| `SchedulingAMessage.md` | 7, 8, 9 | 8, 9, 10 | not a split: a block inserted at #7, the #4414 workaround. All three were FAILED at `c7329bb` and are BUILT now, so the AC2 diff reads #7–#9 as `FAILED -> BUILT` and #10 as a new key | 3.4 |
| `AgreementDispatcherRouting.md` | 11 | 11, 12 | the ✅ and ❌ routing lambdas were one fence. #12's database is a type no block names, so no unit may supply it; apart, #11 builds and #12 is listed. #12 is a new key in the AC2 diff | 5.2 |
| `QueryPipelinePolicies.md` | — | 7 | not a split: a block appended after the page's last, the retry-and-breaker wrap. A new key, BUILT | 5.2 |
| `PostgreSQLBrokerTradeOffs.md` | 1 | 1, 2 | the JSONB and JSON schemas were one fence; each now builds. #2 is a new key, BUILT | 5.3 |
| `CloudEventsReference.md` | 3, 4 | 4, 5 | not a split: a block inserted at #3, the Kafka partition key set per message. Old #3 (SNS) and #4 (Azure Service Bus) are now #4 and #5; all five build, so the AC2 diff reads #3, #4 `FAILED -> BUILT` and #5 as a new key | 5.3 |
| `Telemetry.md` | 1–5 | 2–6 | not a split: a block inserted at #1, *Enabling Brighter's Spans*, FAILED on the pin. Old #1 and #4, BUILT, are now #2 and #5, their rows moved; so the AC2 diff reads #1 `BUILT -> FAILED` and #6 as a new key | 5.3 |
| `CQRSWithBrighterAndDarker.md` | — | 7 | not a split: the write model inserted above the handler, BUILT, so old #7–#12 are #8–#13, FAILED both sides. The AC2 diff reads #7 `FAILED -> BUILT`, the new block on old #7's key, and #13 as a new key, FAILED | 5.4 |
| `MessageMappers.md` | 2–6 | 3–7 | not a split: the async mapper inserted at #2. Old #3, BUILT, is now #4, its row moved (old row removed, new row at `6ddcf7b`); so the AC2 diff reads #3 `BUILT -> FAILED`, #4 `FAILED -> BUILT` and #7 as a new key. The rest FAILED both sides | 5.5 |
| `DefaultMessageMappers.md` | 4 | 4, 5, 6 | the Avro mapper rewritten as three fences: the mapper (#4, BUILT), the avrogen partial (#5, BUILT) and its registration (#6, same-page). Old #5–#13 are #7–#15, FAILED both sides. So the AC2 diff reads #4, #5 `FAILED -> BUILT` and #14, #15 as new keys, FAILED | 5.4 |

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
| `ConfiguringOpenTelemetry.md` | 2 | FAILED | the Jaeger exporter block — OpenTelemetry deprecated the exporter for OTLP, and the page now points the OTLP exporter at Jaeger (ruling 1). Old #3–#7 are now #2–#6, FAILED before and after, so the AC2 diff shows only #7 `FAILED -> -` | 5.3 |
| `DynamicMessageDeserialization.md` | 7 | FAILED | *"Cache Performance-Critical Paths"*: three `PublishAsync` calls of dummy events to warm the mapper cache. `PublishAsync` maps nothing, and posting warms only the producer's transform cache while putting junk on a topic. The page is unchanged from `c7329bb` to `aec11c1`, so this is #7 there too; old #8 and #9 are #7 and #8 | 5.4 |

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
| `IRequestContext` implemented with `Guid Id`, `ISpan Span`, `Dictionary<string, object> Bag`, `CustomHeaders` — at 10.7.0 the interface has no `Id` and no `CustomHeaders`, `Span` is an `Activity`, `Bag` a `ConcurrentDictionary`; the section also listed `PartitionKey` and `CustomHeaders` as new properties and set both on `Context` | `IRequestContext.cs` | `V10MigrationGuide.md` § 5, #12, #13 | `grep -rnE 'Context\??\.(PartitionKey\|CustomHeaders)\b\|`CustomHeaders`\|ISpan Span\|Guid Id \{ get; set; \}' contents/` → 6 at `608615a`, 2 now, both read and right (the corrective sentence; an entity's key) | **5** | **0** | 2.3, the row above's grep; closed in 5.5 |
| `MapToMessage(TRequest request)` — V9's mapper signature; V10's takes a `Publication` | `IAmAMessageMapper.cs:33` | `Compression.md`, `Routing.md`, `KafkaConfiguration.md`, `MessageTransforms.md`, `ImplementingExternalBus.md`, `V10MigrationGuide.md`, `MessageMappers.md`, `OutboxArchiver.md`, `NullableReferenceTypes.md` (`string? topic = null`, found by the second method) | `grep -rnE 'MapToMessage\([A-Za-z<>]+ [a-z][A-Za-z]*(, string\? topic = null)?\)' contents/` | **14** lines, 10 pages | **2** — both skipped V9 forms | 2.5, `--explain` |
| A mapper class without `IRequestContext? Context { get; set; }` — `CS0535` | `IAmAMessageMapper.cs:31` | 11 pages | mapper blocks declaring `: IAmAMessageMapper<` with no `IRequestContext? Context` | **17** | **1** — the skipped V9 form | 2.5, `--explain` after the signature fix |
| `requeueCount: N` described as N requeues (*"Times a message is requeued"*, *"On the 4th failure"*) — it is N handlings, N−1 requeues; `0` behaves as `1` | `Message.cs:161`, `HandledCount >= requeueCount`; run, table in § *Phase 2 as executed* | 17 pages, including all 12 option tables | `grep -rnE 'requeued before it is treated\|exceed(s\|ed\|ing)? the requeue count\|Requeue up to 3\|Retry up to 3\|4th failure\|RequeueCount. is exceeded\|retries (remain\|exhausted)\|retry a message before\|number of requeue attempts\|requeue count exceeded\)\|When the count is exceeded' contents/` | **25** | **0** | 2.5, running |
| `requeueDelayInMilliseconds` — V9; V10's `Subscription` takes `TimeSpan? requeueDelay` | `Subscription.cs:115` | `BrighterSchedulerSupport.md`, `DispatcherConfigurationReference.md`, `HowServiceActivatorWorks.md`, `SchedulingAMessage.md` | `grep -rnE 'requeueDelayInMilliseconds\|RequeueDelayInMilliseconds' contents/` | **5** | **0** | 2.5, reading the requeue lines |
| DLQ enrichment keys given as one PascalCase set for every transport — Kafka writes `OriginalTopic`, `OriginalType`, …; six transports write camelCase; RMQ, ASB, GCP and InMemory write none but the pump's free-text `RejectionReason`; the bag is case-sensitive. Also RMQ and ASB listed as routing invalid messages, and both keys as on *"all"* subscriptions | `KafkaMessageConsumer.cs:1080`, `MsSqlMessageConsumer.cs:382` and five twins, `MessageHeader.cs:162`, `Reactor.cs:426`; `IUseBrighterInvalidMessageSupport` implementors | `ErrorHandlingOptions.md` | `grep -rnE '\| .OriginalMessageType. \|\|Both are constructor parameters available on all' contents/` | **2** | **0** | 2.5, verifying the table |
| Decompress does not recognise `WrapAsync`'s output — `IsCompressed` compares `ContentType.ToString()` with `application/gzip`, and `WrapAsync` adds `; charset=utf-8` | run; `CompressPayloadTransformer.cs:125`, `:301` — **upstream, BrighterCommand/Brighter#4432** | `Compression.md` states it and links the issue | — | **1** | **stated** — maintainer's ruling | 2.5, running |
| `UseExternalLuggageStore` resolves `IAmABrighterTracer` with `GetRequiredService` and only `AddBrighterInstrumentation()` registers one, so a claim check without tracing throws on first `Post`; the page's null-store exception appears only with a tracer | run; `ServiceCollectionExtensions.cs:1008` — **upstream, BrighterCommand/Brighter#4433** | `HandlingLargeMessages.md` step 3 and its failures list | — | **1** | **documented** — maintainer's ruling | 2.5, running the null-store claim's control |
| A Reactor subscription paired with an async handler — the pump stops with `ConfigurationException` (`InvalidCastException` inside), and the handler never runs; the page's own step 4 says so | run, control Proactor | `HandlingPoisonMessages.md` step 3 → `Proactor` | reading: the other four tranche pages have no handler beside their subscriptions | **1** | **0** | 2.5, running block 2's claim |
| A log excerpt quoting messages Brighter does not emit (*"Failed to process message … requeueing"*, *"Requeue count exceeded"*) | `Reactor.cs:601–670`, the templates; replaced with a captured run | `HandlingPoisonMessages.md` step 1 | `grep -rnE 'Requeue count exceeded for message\|Failed to process message' contents/` | **3** | **0** | 2.5, reading step 1 against the run's log |
| `new MessageBody(bytes, "JSON")` / `(s, MediaTypeNames.Application.Octet, …)` — a string where 10.7.0 takes a `ContentType?` | `CS1503` from `--explain` | `KafkaConfiguration.md` #20, `MessageMappers.md` #5 | #82's: `grep -rnE 'new MessageBody\([^)]*, *("\|MediaTypeNames)' contents/` | **2** | **0** — carried as #82, repaired at `154f78c` (5.3) | 2.5, `--explain` on the touched blocks |
| Mapper excerpts that omit a required member with no `// ...` (`CS0535` `MapToRequest` / `MapToMessage`) | `CS0535` | `Routing.md` #1, `V10MigrationGuide.md` #3, #18, `NullableReferenceTypes.md` #7, `FAQ.md` #7 | #83's: `python3 spec/017-compile_repairs/probe/mapperscan.py` | **5** | **0** — carried as #83, the same five blocks | 2.5, `--explain` on the touched blocks |
| A `Publication` a reader posts through with no `RequestType` — `Post` throws `ConfigurationException` (*"No producer found for request type"*); BRT001 warns on it | `FindPublicationByPublicationTopicOrRequestType.cs:81`; run, control both ways | `InMemoryTransport.md`, `AWSSQSConfiguration.md`, `BrighterBasicConfiguration.md`, `CommandProcessorConfigurationReference.md`, `InMemoryOptions.md`, `KafkaConfiguration.md`, `RabbitMQConfiguration.md`, `V10MigrationGuide.md` | `python3 spec/017-compile_repairs/probe/pubscan.py` | **23** | **2** — `AnalyzerSupport.md`'s BRT001 suppression and `HandlingLargeMessages.md`'s `WrapAsync`, both right | 3.2, running the complete example |
| `InternalBus` said to have no backpressure and the in-memory transport no dead letter queue — `InternalBus(boundedCapacity)` blocks a sender when full; `DeadLetterRoutingKey` moves a rejected message to that topic | `InternalBus.cs:39`, `InMemoryMessageConsumer.cs:218`; run, control both ways | `InMemoryTransport.md` | `grep -rnE 'No backpressure\|No dead letter queues' contents/` | **2** | **0** | 3.2, reading the page against the source |
| `SqsPublication { SqsAttributes = … }` — the property is `QueueAttributes` (`CS0117`) | `SqsPublication.cs:73` | `AWSSQSConfiguration.md` | `grep -rnE '\bSqsAttributes\s*=' contents/` | **1** | **0** | 3.2, `--explain` after the block's `using`s |
| `CloudEventsType` as a `Publication` property — it is `Type` (`CS0117`) | `Publication.cs:96` | `V10MigrationGuide.md` (initializer and `publication.CloudEventsType`) | `grep -rnE '\bCloudEventsType\s*=\|\.CloudEventsType\b' contents/` | **2** | **0** | 3.2, `--explain` after the block's `using`s |
| `new InMemoryOutbox()` — V10's constructor takes a `TimeProvider` (`CS7036`) | `InMemoryOutbox.cs:91` | `InMemoryOptions.md`, `InMemoryOutbox.md` | `grep -rnE 'new InMemoryOutbox\(\)' contents/` | **2** | **0** | 3.2, `--explain` after the block's `using`s |
| `publication.SetConfigHook(…)` — the hook is on `KafkaProducerRegistryFactory`; the block also wrapped its publication as `new KafkaPublication() {publication}` and dropped a `;` | `KafkaProducerRegistryFactory.cs:87`; compiled, old form `CS1061` | `KafkaConfiguration.md` | `grep -rn 'publication\.SetConfigHook' contents/` | **1** | **0** | 3.2, reading the block beside its prose |
| A V4 package recommended with the V3 namespace, or the namespaces said to stay the same: every Brighter `.V4` package at 10.7.0 declares its types in `<V3 namespace>.V4` (`Locking.DynamoDb` → `Locking.DynamoDB.V4`), so a reader who installs the V4 package and copies a V3 `using` gets `CS0234` | `git ls-tree 10.7.0 src/*.V4`, each project's `namespace` lines; the lock blocks compiled against the V4 packages, control V3 `CS0234` | `AWSSQSMigrateToV10.md`, `AwsScheduler.md`, `DynamoInbox.md`; by reading `DynamoOutbox.md`, `DistributedLock.md`, `DynamoDbDistributedLock.md` | `python3 spec/017-compile_repairs/probe/v4scan.py` — blind to a page whose V4 package ID is also its namespace, which is why `DynamoOutbox.md` was read | **4** | **0** | 3.3, adding the lock blocks' `using`s |
| A value returned from `async Task` — `ScheduleNotification` returned `schedulerId` (`CS1997`) | `CS1997` from `--explain`; `PostAsync(TimeSpan, …)` returns `Task<string>`, `IAmACommandProcessor.cs:282` | `SchedulingAMessage.md` #3 | `python3 spec/017-compile_repairs/probe/asyncscan.py`, its `CS1997` lines. At `c7329bb` it reads **2**: this one, and `ImplementingAsyncHandler.md:50`, #1's `Task` return | **1** | **0** | 3.4, `--explain` after the block's `using`s |
| `command with { … }` on a request — needs a record, and a Brighter `Command` is a class, which a record cannot derive from | `Command.cs:42` | `SchedulingAMessage.md` #5 | `grep -rnE '\bwith \{' contents/` | **1** | **0** | 3.4, stubbing the block's `OperationCommand` |
| `AutoFromAssemblies()` or `AsyncHandlersFromAssemblies` with a scheduler — the scheduler's handlers registered twice, so a scheduled request throws when it falls due, and ends the process with InMemory | run; registry read, control `HandlersFromAssemblies` — **upstream, BrighterCommand/Brighter#4414**, fixed by #4419 on `master`, unreleased | `SchedulingAMessage.md` states it with the workaround. Recorded, not repaired: `AwsScheduler.md`, `AzureScheduler.md`, `BrighterSchedulerSupport.md`, `HangfireScheduler.md`, `InMemoryOptions.md`, `InMemoryScheduler.md`, `QuartzScheduler.md`, `SwitchingSchedulers.md`, `V10MigrationGuide.md` | `grep -rl 'UseScheduler' contents/ \| xargs grep -lE 'AutoFromAssemblies\(\|AsyncHandlersFromAssemblies'` | **10** pages | **stated** on 1 — maintainer's ruling; 9 recorded | 3.4, running |
| InMemory cancel and reschedule said to work on a request scheduled through the command processor — each scheduled call gets a new `InMemoryScheduler`, so `CancelAsync` finds nothing and the request runs; `ReSchedulerAsync` returns `False` | run, control same instance; `CommandProcessor.cs:427`, `InMemoryScheduler.cs:57` — **upstream, BrighterCommand/Brighter#4437**, filed 3.4 | `SchedulingAMessage.md`, `FAQ.md`, `InMemoryScheduler.md` (cancel example, `Should_Cancel_Scheduled_Command`) | `grep -rl 'issues/4437' contents/` | **3** pages | **stated** on all 3 — maintainer's ruling | 3.4, running block 4's claim |
| Darker's default policies described as *"exponential backoff"* and a breaker that *"opens after consecutive failures"*, and as applying once registered. They retry 3 times after 50, 100 and 150 ms, the breaker opens on 1 failure for 500 ms, and neither runs without `[RetryableQuery]` | Darker 4.1.1 `QueryProcessorBuilderExtensions.cs:51`, `RetryableQueryDecorator.cs`; run, control without the attribute | `QueryPipelinePolicies.md` (the list and block 2's comment) | `grep -rnE 'Retries with exponential backoff\|Opens after consecutive failures\|Retry policy with exponential backoff' contents/` | **3** | **0** | 3.5, reading the page against Darker's source, then running |
| *"The ASP.NET model binder will validate these attributes before the query reaches your handler"*. Only a controller marked `[ApiController]`, or a minimal API after `AddValidation()` (.NET 10), rejects the query; elsewhere it reaches the code | run on net10.0, controls both ways | `QueryObjectValidation.md` | `grep -rn 'model binder will validate' contents/` | **1** | **0** | 3.5, running block 2's claim |
| `[RetryableQuery]`'s second argument described and used as a circuit-breaker name that adds a breaker to the retry. It is a policy name, and the decorator runs that one policy. `"DefaultCircuitBreaker"` is not registered by `AddDefaultPolicies()`, so it throws `ConfigurationException`; `circuitBreakerName:` is not a parameter (`CS1739`) | Darker 4.1.1 `RetryableQueryAttribute.cs:11`, `Constants.cs`; run, control `Constants.CircuitBreakerPolicyName`; compiled | `QueryPipeline.md` (4 lines, and the parameter list at line 239), `CQRSWithBrighterAndDarker.md` (2), `DarkerAndBrighterPipelines.md`, `ImplementAQueryHandler.md`, `QueryPatterns.md` | `grep -rnE 'RetryableQuery\(.*(DefaultCircuitBreaker\|circuitBreakerName)' contents/` | **9** lines, 5 pages; **20** lines, 6 pages, with any second argument (`grep -rnE 'RetryableQuery\([^)]*,' contents/`) | **0**; **8** with any second argument, each a registered policy or the deliberate *"doesn't exist"*, described as the one policy the decorator runs — maintainer's ruling, repaired in 5.2 | 3.5, reading Darker's source for the tranche's policy defaults |
| `.ConfigureServices(hostContext, services) =>` — the lambda's parameter list never opened, and its body never closed (`CS1519`, `CS1001`) | compiled, old form `CS1519` | `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`, `DynamoInbox.md`, `AzureBlobArchiveProvider.md`, `BrighterBasicConfiguration.md` ×2, `DispatcherConfigurationReference.md` ×5 | `grep -rn 'ConfigureServices(hostContext, services) =>' contents/` | **13** lines, 8 pages | **0** | 4.1, `--classify` |
| `opt.InboxConfiguration` inside `AddConsumers(options => …)` — `CS0103` | compiled | `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md` | `grep -rn '^\s*opt\.InboxConfiguration' contents/` — **4** before, **1** after, `DynamoInbox.md`'s, whose parameter is `opt` | **3** | **0** | 4.1, a scan of every lambda |
| `[UseInboxAsync]` on a handler class — `CS0592`; `RequestHandlerAttribute` is valid on methods only | `RequestHandlerAttribute.cs`, `AttributeUsage(AttributeTargets.Method)` | `InMemoryInbox.md` #2 | `grep -rn -A1 '^\s*\[UseInbox' contents/ \| grep -c class` | **1** | **0** | 4.1, `--explain` |
| `await _commandProcessor.Post(…, cancellationToken: …)` — `Post` returns `void` and takes no `cancellationToken`; the async form is `PostAsync` | `IAmACommandProcessor.cs:205`, `:241` | `InMemoryOutbox.md` #2 | `grep -rnE 'await [_a-zA-Z.]*\.(Post\|Send\|Publish\|DepositPost\|ClearOutbox)\(' contents/` | **1** | **0** | 4.2, `--explain` after the block's `using`s |
| `new AmazonDynamoDBConfig { ServiceURL = "…"; }` — a `;` inside an object initialiser | compiled | `DynamoInbox.md` #1 | `grep -rnP 'new [A-Za-z_.<>]+(\([^()]*\))? *\{[^{}]*;[^{}]*\}' contents/`, one line only; control: the old page → **1** | **1** | **0** | 4.1, `--classify` |
| The InMemory Inbox said to keep every entry until restart (*"No cleanup"*, *"All seen message IDs held in memory"*). An entry expires `EntryTimeToLive` (5 min) after it is written, removed by a scan at most every `ExpirationScanInterval` (10 min); past `EntryLimit` (2048) adding compacts the oldest to half | `InMemoryBox.cs:64–100`, `InMemoryInbox.cs:316`; run, controls both ways | `InMemoryInbox.md` | `grep -rnE 'No cleanup\|All seen message IDs held in memory' contents/` | **2** | **0** | 4.2, reading the page against the source, then running |
| A global `actionOnExists: Warn` shown beside a `[UseInboxAsync]` that sets no `onceOnlyAction` — the attribute's default `Throw` wins, so a duplicate throws `OnceOnlyException` | `PipelineBuilder.cs:371`, `HasExistingUseInboxAttributesInPipeline`; run, control the attribute with `Warn` | `InMemoryInbox.md` #1, #2 | pages with `actionOnExists: OnceOnlyAction.Warn\|Replay` and a `[UseInbox…]` without `onceOnlyAction` on its line: 2, read — `TurningOnReplayOnSeen.md`'s attributes set it on the next line and the page states the precedence | **1** | **0** | 4.2, running #1 with #2 |
| A global `InboxConfiguration` in `AddConsumers` reaches the pipeline only through `ExternalBus(…)`, so an application that never calls `AddProducers` gets no global Inbox, and duplicates run again | `ServiceCollectionExtensions.cs:640–660`; run, control with `AddProducers` — **upstream, BrighterCommand/Brighter#4335**, fixed by #4396 on `master`, unreleased | `BrighterInboxSupport.md` states it with the workaround; linked from `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SqliteInbox.md`, `DynamoInbox.md`, `MongoDBInbox.md`, `FirestoreInbox.md`, `SpannerInbox.md`, `InMemoryInbox.md`. Not linked: the seven other pages that configure one | `git grep -l 'InboxConfiguration' d8633b1 -- contents` | **16** pages | **stated** on 1, linked from 9 — maintainer's ruling | 4.2, running `InMemoryInbox.md` #1 without #2's attribute |
| `CircuitBreakerState` — named in a custom `IAmAnOutboxCircuitBreaker` and declared by no package or block (`CS0246`) | `git grep CircuitBreakerState 10.7.0 -- src` → 0 | `UsingSweeperCircuitBreaking.md` #4 | an unshown `CircuitBreakerState` in `contents/` | **1** | **0** | 4.4, `--classify` |
| A custom breaker's `Dictionary` enumerated by `CoolDown` while `TripTopic` writes it — `InvalidOperationException` | run, control the concurrent form; 10.7.0's own breaker is concurrent for this (`InMemoryOutboxCircuitBreaker.cs`) | `UsingSweeperCircuitBreaking.md` #4 | `grep -rn ': IAmAnOutboxCircuitBreaker' contents/` → the page's two, both concurrent | **1** | **0** | 4.4, reading #4 once it built |
| A distributed breaker on `IDistributedCache`, with `CoolDown` and `TrippedTopics` left unwritten — the cache cannot enumerate keys, so `TrippedTopics` cannot be written on it | `IDistributedCache` has `Get`, `Set`, `Refresh`, `Remove` and their async forms only; the Redis form run across two connections | `UsingSweeperCircuitBreaking.md` #5 | `grep -rn 'IDistributedCache' contents/` → 1, the prose saying why | **1** | **0** | 4.4, making the fragment whole |
| The Azure archive block, against 10.7.0: `New AzCliCredential();` in an initialiser — no such type (`AzureCliCredential`); `AzureBlobArchiveProviderOptions` built parameterless, its `init` properties assigned, `BlobContainerUri` a string (`CS7036`, `CS0029`); `UseOutboxArchiver` without `TTransaction`; `BatchSize` for `ArchiveBatchSize`; `MinimumAge = 744` for a `TimeSpan`; option assignments with no `options.` (`CS0103`) | `AzureBlobArchiveProviderOptions.cs`, `HostedServiceCollectionExtensions.cs:52`, `TimedOutboxArchiverOptions.cs`; compiled, control the old block | `AzureBlobArchiveProvider.md` #1 | the seven greps in § *Phase 4 as executed*, 4.4's entry | **1** block, 8 defects | **0** | 4.1 (six), 4.4 (two, compiling the control) |
| The sweep interval set through `options.OutboxSweeper = new OutboxSweeperOptions { SweepInterval = … }` — no such type or property; it is `UseOutboxSweeper(o => o.TimerInterval = …)`, an `int` of seconds | `TimedOutboxSweeperOptions.cs`, `HostedServiceCollectionExtensions.cs:41`; compiled | `SweeperCircuitBreaking.md` #2, #7 and its formula | `grep -rnE 'SweepInterval\|OutboxSweeperOptions\b' contents/` (`Timed` excluded) | **3** lines | **0** | 4.4, reading `UsingSweeperCircuitBreaking.md`'s sibling; maintainer's ruling |
| Cooldown time given as `CooldownCount × interval`, recovery *"when the cooldown reaches zero"* — a topic sits out `CooldownCount` sweeps and is retried on the next, `(CooldownCount + 1) × TimerInterval` | `InMemoryOutboxCircuitBreaker.cs` (removes below zero), `OutboxProducerMediator.cs:721`; run end to end, controls no breaker and `0` | `SweeperCircuitBreaking.md` (formula, example, #2, #7 comments, steps), `UsingSweeperCircuitBreaking.md` #2 | `grep -rnE '(^\|[^+] )[0-9]+ (sweeps )?× [0-9]+s\|total cooldown\|[Rr]ecover after [0-9]\|When the cooldown reaches zero' contents/`, at `05fdeaf` and after | **8** | **0** | 4.4, running the sweeper for the row above |
| Circuit breaking said to work with every Outbox, and `.UseMongoDbOutbox(…)` — no such method. The DynamoDB (V3, V4) and Spanner Outboxes ignore `trippedTopics`, so a tripped topic is swept as normal | `DynamoDbOutbox.cs:582`, `SpannerQueries.cs:12`; run against DynamoDB Local and the Spanner emulator, controls SQLite and MongoDB — **upstream, BrighterCommand/Brighter#4443, #4444**, filed 4.4 | `SweeperCircuitBreaking.md` (section, #5, troubleshooting) | the next row's grep, its first four alternatives | **4** | **0** — the table states it | 4.4, maintainer's ruling |
| Explicit clearing said both to ignore the breaker and to respect it, and failed batches to be *"retried individually per topic"*. An explicit clear sends a tripped topic's messages; a failed `ClearOutboxAsync` trips the topic, a failed `ClearOutbox` does not unless the producer confirms publication | `OutboxProducerMediator.cs:425`, `:1220`, `:984`; run, sync and async, pre-tripped and fresh | `SweeperCircuitBreaking.md` § 6, § *Bulk Dispatch Support* | `grep -rnE 'UseMongoDbOutbox\|fully integrated with MongoDB\|works automatically with MongoDB\|works with all Brighter Outbox\|NOT subject to circuit breaking\|respects circuit breaker state\|retried individually per topic' contents/`, at `05fdeaf` and after; its last three alternatives are this row's | **3** | **0** | 4.4, maintainer's ruling |
| A pre-V10 logging handler: `using Brighter.commandprocessor.Logging`, `namespace Brighter.commandprocessor`, `logger.InfoFormat`; prose placing it in *"the Brighter.CommandProcessor packages"* and passing *"an ILog reference"* — and on the async page, prose about logging beside a block that writes to an Inbox; `FAQ.md` quoting `Brighter.commandprocessor.ICommand` in an exception | `Logging/Handlers/RequestLoggingHandler.cs`; run, the FAQ's message `Paramore.Brighter.ICommand` | `BuildingAPipeline.md`, `BuildingAnAsyncPipeline.md`, `FAQ.md` | `grep -rnE 'Brighter\.commandprocessor\|Brighter\.CommandProcessor packages\|ILog reference' contents/` | **8** | **0** — `Monitoring.md:25`'s `app.config` section went with the page's rewrite, by ruling | 5.1, `--explain` |
| `.Successor = …` and a method *"IHandleRequests\<TRequest\> Successor"* — the method is `SetSuccessor()`; and the manual chain said to run from the registry alone, which holds the handler's type, so a factory must return the wired instance | `RequestHandler.cs:74`; run, control a new instance | `BuildingAPipeline.md` | `grep -rnE '\.Successor *=\|TRequest\\> Successor\*\*\|Successor\.Handle\(\)' contents/` | **3** | **0** | 5.2, `--explain` |
| *"Cannot use AutoFromAssemblies"* with an Agreement Dispatcher, *"creates fixed mappings"*. `AutoFromAssemblies(excludeDynamicHandlerTypes: …)` scans beside an agreement; without the exclusion every `Send` throws *"More than one handler was found"* | `IBrighterBuilder.cs:46`, `ServiceCollectionBrighterBuilder.cs:238`; run, both orders, controls both ways | `AgreementDispatcherRouting.md`, `AgreementDispatcher.md`, `FAQ.md` | `grep -rnEi "cannot use .?AutoFromAssemblies\|AutoFromAssemblies.? (won't\|will not) work\|creates fixed (1-to-1 )?mappings\|Instead of AutoFromAssemblies" contents/` | **7** | **0** | 5.1, P0-10 — running #8's claim |
| Darker's query processor said to default to **Transient**, and an unscoped EF Core handler to fail on a *disposed DbContext*. It defaults to **Singleton** and resolves handlers from the root provider: with scope validation, *"Cannot resolve … from root provider"*; without, one `DbContext` for every query | Darker 4.1.1 `DarkerOptions.cs:9`; run, control `Scoped` | `DarkerConfigurationReference.md`, `DarkerBasicConfiguration.md` | `grep -rnEi 'disposed DbContext\|IQueryProcessor.{0,40}Transient\|Default Configuration \(Transient\)' contents/` | **5** | **0** | 5.2, reading the page against `DarkerOptions` |
| `.Handlers(registry, Activator.CreateInstance, t => {}, Activator.CreateInstance)` — Darker's factories are `Func<Type, IQueryHandler>` and `Func<Type, IQueryHandlerDecorator>`, and `Activator.CreateInstance` returns `object` (`CS1503`). Darker's README carries the same line | `Builder/INeedHandlers.cs:9`; compiled, and run with the casts | `DarkerConfigurationReference.md`, `ImplementAQueryHandler.md` | `grep -rnE 'Handlers\(registry, Activator\.CreateInstance' contents/` | **2** | **0** | 5.1, `--explain` |
| `using System.Threading.Task;` (`CS0234`) | compiled | `ParameterizedQueryPatterns.md` | `grep -rn 'using System\.Threading\.Task;' contents/` | **1** | **0** | 5.1, `--explain` |
| V9's `new MessageMapperRegistry(messageMapperFactory) { { typeof(…), typeof(…) } }` (`CS7036`, `CS1922`) | `MessageMapperRegistry.cs:64`, `:296` | `HowConfiguringTheDispatcherWorks.md` | `grep -rnE 'new MessageMapperRegistry\([a-zA-Z]+\)$' contents/` | **1** | **0** | 5.1, `--explain` |
| `messagePumpType` said to be set to `Reactor` because *"`Subscription<T>` defaults to `Proactor`"* — the block's `RmqSubscription<T>` (RMQ.Sync) defaults to `Reactor`; switching to `Proactor` is what fails | `RmqSubscription.cs:107`, `Subscription.cs:291`; run against RabbitMQ, controls omitted and `Proactor` | `HowConfiguringTheDispatcherWorks.md` | `grep -rn 'messagePumpType. is set explicitly' contents/` | **1** | **0** | 5.2, running #2's warnings |
| A query handler reading `o.Customer` and `o.CreatedAt` from a write model that declares neither (`CS1061` once its names resolve); commands written `: IRequest { /* ... */ }` and controller actions outside a class | compiled | `CQRSUseCasesAndPatterns.md` #1, #2 | The write model is block-local: `o.Customer` is read on 5 other pages, each against its own block's model, and only a compile says whether that model declares it. `grep -rnE ': *I(Request\|Query<[^>]*>\|Command\|Event) *\{ */\* *\.\.\. *\*/ *\}' contents/` → 4 before, 0 after. `grep -rnE '^\[Http(Get\|Post\|Put\|Patch\|Delete)' contents/` → 4 before, **2** after: `QueryPatterns.md` #5 and `ShowMeTheCode.md` #1, off-tranche FAILED blocks that `--classify` calls `import`. An action shown without its controller is a wrapper shape, not an API defect (design, P0-6), so both are residual | **2** blocks | **0** | 5.2, stubbing #1 |
| `Monitoring.md` written for V9: an `app.config` section typed `MonitoringConfigurationSection, Brighter.commandprocessor`, `container.Register<TRequest, MonitorHandler<TRequest>>`, and the Control Bus on the retired site. At 10.7.0 the handler takes an `IAmAControlBusSender` and a `MonitorConfiguration` from the container | `Monitoring/Handlers/MonitorHandler.cs`, `Configuration/MonitoringConfigurationSection.cs`; run end to end | `Monitoring.md` | `grep -rnE 'configSections\|MonitoringConfigurationSection\|brightercommand\.github\.io/Brighter/ControlBus' contents/` | **5** | **0** | 5.1, the pre-V10 namespace recurrence; maintainer's ruling |
| A handler declared without `public` — `AutoFromAssemblies()` scans only public handler types, so its request throws *"No command handler was found"* | `ServiceCollectionBrighterBuilder.cs:238`; run, control public | `BuildingAPipeline.md`, `BuildingAnAsyncPipeline.md`, `CommandProcessorConfigurationReference.md`, `FeatureSwitches.md`, `MigratingToPollyV8.md`, `PolicyRetryAndCircuitBreaker.md` | `grep -rnE '^\s*(internal\s+)?class\s+\w+\s*:\s*(RequestHandler\|RequestHandlerAsync)<' contents/`, and a scan of every C# fence | **18** | **0** | 5.2, running `BuildingAPipeline.md`; maintainer's ruling |
| Darker's scan said to need handlers named *"…Handler"* and not nested. It registers every exported non-abstract `IQueryHandler<,>`: the name and nesting in a public class do not matter; `internal` does | Darker 4.1.1 `QueryHandlerRegistry.cs:48`; run, four cases | `DarkerBasicConfiguration.md` | `grep -rnEi 'end with .?"?Handler\|not nested within' contents/` | **2** | **0** | 5.2, reading the lifetime row's page; maintainer's ruling |
| `[UseResiliencePipeline]` stacked on one method (`CS0579`) — not repeatable, by design since BrighterCommand/Brighter#2580; strategies are layered in one pipeline. `HowConfiguringTheCommandProcessorWorks.md` said *"you can use multiple attributes"* | `UseResiliencePipelineAttribute.cs:52`; compiled, and the composed form run | `PolicyRetryAndCircuitBreaker.md`, `HandlerFailure.md` ×2, `HowConfiguringTheCommandProcessorWorks.md`, `MigratingToPollyV8.md`, `PolicyFallback.md` ×2 | a scan of every C# fence for one attribute twice in one run of attribute lines | **7** blocks, 5 pages | **0**, `QueryPipeline.md`'s two `[RetryableQuery]` alternatives, commented as two handlers', aside | 5.2, `--explain` on the touched blocks |
| A Polly v8 pipeline's strategies said to wrap *"inner to outer"* in the order added, and `MyComprehensivePipeline` added timeout, retry, breaker. The first added is the outermost, so its 10 s timeout covered every retry together | run, both orders | `PolicyRetryAndCircuitBreaker.md` | `grep -rnE "order they.re added\|inner to outer" contents/`, and a scan of every `TryAddBuilder` chain | **1** sentence, 1 chain | **0** — `PolicyFallback.md`'s chain was right | 5.2, rewriting the stacked attributes |
| `await` in a handler not marked `async` (`CS4032`) | compiled | `FeatureSwitches.md` #2 | `python3 spec/017-compile_repairs/probe/asyncscan.py`, its `CS4032` lines | **1** | **0** | 5.2, `--explain` |
| A monitored handler that throws: the monitor's `ExceptionThrown` event carries the `Exception`, which `System.Text.Json` cannot serialize, so the caller gets `NotSupportedException` in place of the handler's exception. And `[MonitorAsync]` cannot send through `ControlBusSenderFactory`'s sender, which registers no async mapper. Both on Brighter `master` too | `MonitorEvent.cs:74`, `ControlBusSenderFactory.cs:56`; run, controls non-throwing and sync — **upstream, BrighterCommand/Brighter#4453, #4454**, filed 5.2 by the maintainer's word | `Monitoring.md` states both and links them | `grep -rn 'issues/445[34]' contents/` | **2** | **stated** | 5.2, running the rewritten page |
| A PostgreSQL consumer's `channelName` different from the publication's `Topic`: the consumer reads `queue = ChannelName`, the producer writes `queue = Topic`, so nothing is received | run: **0** handled → **1** | `PostgreSQLMessageBroker.md` | reading every `PostgresSubscription` on the two PostgreSQL pages | **1** | **0** | 5.3, running the page |
| `PublishAsync` said to send to the PostgreSQL broker; it runs local handlers and writes no row | run: `PostAsync` → a row; `PublishAsync` → none | `PostgreSQLMessageBroker.md` | `grep -n PublishAsync` on the two PostgreSQL pages | **2** lines, both sending with it | **2** lines, both saying it does not reach the table; the corpus-wide sweep is put to the maintainer | 5.3, running the page |
| *Scheduled Messages* said a delay sets `visible_timeout`; `PostAsync(delay)` goes through the scheduler and the row appears when it fires, visible at once. `visible_timeout` delays only a requeue | run: row at t≈6 s; `requeueDelay` 20 s → visible at 19.5 s, control 0 → handled 3× | `PostgreSQLMessageBroker.md` | `grep -n 'CURRENT_TIMESTAMP + delay'` | **1** | **0** | 5.3, running the page |
| *"(timeout not updated)"* during processing: retrieval moves `visible_timeout` on by the subscription's `visibleTimeout` | read, `PostgreSqlMessageConsumer` at 10.7.0; run | `PostgreSQLMessageBroker.md` | `grep -rn 'timeout not updated' contents/` | **1** | **0** | 5.3, running the page |
| The outbox write said to join an EF Core transaction without the transaction provider | run: rollback with the provider → **0** outbox rows; without → **1** | `PostgreSQLMessageBroker.md` | reading the page's outbox sections | **1** | **0** | 5.3, running the page |
| `[ClaimCheck(threshold:, dataStore:)]` — no such parameters; the attribute is `ClaimCheck(int step, int thresholdInKb)` on a mapper | `ClaimCheckAttribute.cs:43` | `PostgreSQLMessageBroker.md` | `grep -rnE 'ClaimCheck\([^)]*dataStore:' contents/` | **1** | **0** | 5.3, `--explain` |
| `PostgresSubscription<T>` without `messagePumpType` → `ConfigurationException`, *"You must set a message pump type"* | run, with the generic `Subscription<T>` and `KafkaSubscription<T>` as controls, which build | `PostgreSQLMessageBroker.md` | a Python scan of every `new PostgresSubscription<` call on the page | **4** of 5 | **0** of 5 | 5.3, running the page |
| The same, on **non-generic** subscriptions (`Subscription`, `KafkaSubscription`, `RmqSubscription` on RMQ.Async, `AzureServiceBusSubscription`, `SqsSubscription`) | run, as above | `AgreementDispatcher.md:122`, `CloudEventsSupport.md:185`, `DynamicMessageDeserialization.md:71`, `:210`, `:237`, `FAQ.md:254`, `:637`, `RoutingMultipleMessageTypes.md:104`, `:132`, `:195`, `:309` (lines at `5129c20`) | reading | **11** | **recorded, not repaired** — ruled 2026-09-28. The RMQ ones are to be checked as RMQ.Async: RMQ.Sync defaults to Reactor | 5.3, ruling 3 |
| SQL naming a `created_at` column the broker's table does not have | every SQL block run against the table | `PostgreSQLMessageBroker.md` | `grep -rn created_at contents/` | **4** | **0** | 5.3, running the SQL |
| *"Increase `timeOut` to reduce polling frequency"*; the empty-queue poll interval is `emptyChannelDelay` | read, `Subscription.cs` at 10.7.0 | `PostgreSQLMessageBroker.md` | ``grep -rn 'Increase `timeOut`' contents/`` | **1** | **0** | 5.3, reading |
| `Paramore.Brighter.PostgreSql.EntityFrameworkCore` 10.7.0 resolves EF Core 10, so Npgsql's EF provider must be 10.x | run: 9.0.4 → `MissingMethodException` | `PostgreSQLMessageBroker.md` | `grep -rn 'Npgsql.EntityFrameworkCore' contents/` | **0** | **2**, stated on the page and on `PostgresOutbox.md`, where the package is installed | 5.3, running the page |
| Message size: the page gave no measured limit, and AWS SQS as 256 KB | run: 50 MB round-trips, 150 MB rejected by Npgsql `54000`; SQS by the maintainer (ruling 5) | `PostgreSQLBrokerTradeOffs.md` | reading its comparison table | **1** | **0** | 5.3, running the page |
| Tracing configured with `AddSource("Paramore.Brighter…")` alone. `AddBrighter()` registers no `IAmABrighterTracer`, so no Brighter span is ever recorded; `AddBrighterInstrumentation()` registers one | run: `AddSource` alone → **0** spans; `AddBrighterInstrumentation()` → **34** | `Telemetry.md`, `ConfiguringOpenTelemetry.md`, `PostgreSQLMessageBroker.md` #8 | `grep -rnE 'AddSource\("[Pp]aramore\.[Bb]righter' contents/` | **5** | **2**, both sentences saying `AddSource` alone records nothing | 5.3, running the page |
| Span names, attributes and propagation headers in `Telemetry.md`'s tables | captured from spans, RMQ and Kafka, the SDK initialised | `Telemetry.md` | reading every table | every table | rewritten from the capture | 5.3, running the page |
| The Jaeger exporter, deprecated by OpenTelemetry and not in the pin | read | `ConfiguringOpenTelemetry.md` | `grep -rn Jaeger contents/` | **4** | **3**, all saying to point OTLP at Jaeger | 5.3, ruling 1 |
| CloudEvents header names and the content mode on the wire | captured from RMQ.Async and Kafka; SNS and ASB read | `CloudEventsReference.md`, `CloudEventsSupport.md` | reading every header list on both pages | both pages | rewritten from the capture | 5.3, running the page |
| AWS writes the CloudEvents source as `souce` | read; **upstream, BrighterCommand/Brighter#4458**, filed on the maintainer's word | `CloudEventsReference.md` states it and links it | `grep -rn souce contents/` | **0** | **2**, the statement | 5.3, reading |
| *"We default the **ACLs** … to **S3CannedACL.Private**"*: `ACLs` is `null`, and `CreateIfMissing` on a missing bucket throws | `S3LuggageOptions.cs:70`, `S3LuggageStore.cs:130` in both AWS packages; run on LocalStack, controls above | `S3LuggageStore.md`, `HandlingLargeMessages.md` (its store set no `ACLs`) | `grep -rn 'S3LuggageOptions' contents/` → 2 pages, every constructor read | **2** | **0** | 5.3, reading |
| The Control API's status JSON was V9's, its list indented into one paragraph, its route written `{{subscriptionName}}`, and `availableTopics`/`topicName` described as topics — they are subscription names | `ApiExtensions.cs`, `DispatcherExtensions.cs` at 10.7.0; run against a V10 Dispatcher | `BrighterControlAPI.md` | `grep -rn 'control/status' contents/` → the one page | **1** page | **0** | 5.3, running the page |
| A `string` passed as `MessageBody`'s content type, which is a `System.Net.Mime.ContentType` with no conversion from `string` | `MessageBody.cs:124` | `KafkaConfiguration.md:723`, `MessageMappers.md:146` | `grep -rnE 'new MessageBody\([^)]*, *("\|MediaTypeNames)' contents/`, and a scan of every `new MessageBody(` call's arguments | **2** | **0** — the scan found a third, `DefaultMessageMappers.md` #4, put to the maintainer with the rest of that block | 5.1, carried |
| A mapper missing a member of `IAmAMessageMapper<T>`, with no `// ...` to say so | `IAmAMessageMapper.cs` | `Routing.md` #1, `V10MigrationGuide.md` #3, #18, `NullableReferenceTypes.md` #7, `FAQ.md` #7 (whose `MapToMessage` also returned nothing; it gains `[RetrieveClaim]` on the way back) | `python3 spec/017-compile_repairs/probe/mapperscan.py`, written in 6.5 after 5.1 had found these by reading. It reads a `// ...` per block, as `pagelint` does, and skips `blockcheck`'s labelled V9 forms. At `c7329bb` it reads **13**: these five, each missing a `MapTo…` method, and eight missing only `Context`, which are #7's | **5** | **0** | 5.1, carried |
| A `Command` or `Event` subclass that never calls a base constructor; neither has a parameterless one | `Command.cs:68`, `Event.cs:68` | `NullableReferenceTypes.md` #9, `MigratingToNullableReferenceTypes.md` #4, `V10MigrationGuide.md` #20 | `grep -rnE 'class \w+ *: *(Command\|Event) *$\|…\{'` → 16 lines, each read; a Python scan of every C# fence for a base call | **3** | **0**; `V10MigrationGuide.md` #1 is the skipped V9 form | 5.1, carried |
| `V10MigrationGuide.md` #20: the default mapper shown by `PublishAsync` | run: `PostAsync` → **1** message, `application/json`; `PublishAsync` → **0** | `V10MigrationGuide.md` | the corpus sweep is put to the maintainer | **1** | **0** | 5.3, rewriting the block |
| `ITimerProvider` named as the InMemory scheduler's timer seam, with a `FakeTimerProvider : ITimerProvider` passed to `new InMemorySchedulerFactory(…)`. There is no such interface, and the factory has no constructor parameters: the seam is `InMemorySchedulerFactory.TimeProvider`, a `System.TimeProvider` | `InMemorySchedulerFactory.cs:37`, `InMemoryScheduler.cs:244`; run, `FakeTimeProvider` with controls | `InMemoryScheduler.md` | `grep -rn ITimerProvider contents/` | **4** | **0** | 5.4, P0-7 |
| `dotnet add package Paramore.Brighter.InMemoryScheduler`, a package that does not exist: the scheduler and its factory are in `Paramore.Brighter`, and `UseScheduler` is in the DI package | NuGet flat container → **404**; `git ls-tree 10.7.0 src/` has no such project | `InMemoryScheduler.md` | `grep -rnE 'Paramore\.Brighter\.InMemoryScheduler\b' contents/` | **2** | **0** | 5.4, P0-7 |
| `Order` never shown, and behind it `Id = command.Id` assigns a Brighter `Id` to the read side's `Guid` key, `CS0029`. The handler now uses `Guid.Parse(command.Id)`, run on 10,000 `Id.Random()` with a control | `Id.cs:95` (the implicit conversion is to `string` only) | `CQRSWithBrighterAndDarker.md` (#8; #2 was written against a different unshown `Order`) | `grep -rn 'Id = command\.Id' contents/` | **3** | **2**: `PolicyFallback.md`'s two are other properties on types that page never shows | 5.4, P0-7 |
| A `CancellationToken` passed positionally as `PublishAsync`'s second argument, which is `RequestContext?`: `CS1503`, hidden behind `CS0246` | `IAmACommandProcessor.cs:154` | `CQRSWithBrighterAndDarker.md` #2 | `calls.py`, every `Send`/`Publish`/`Post`/`DepositPost` call whose non-first positional argument names a token | **1** call | **0** | 5.4, compiling #2 with the write model |
| The six scheduled overloads listed request-first, `(T command, DateTimeOffset at, CancellationToken)`, without `RequestContext` or Post's `args`. At 10.7.0 the time comes first | `IAmACommandProcessor.cs:98`, `:111`, `:171`, `:190`, `:261`, `:282` | `BrighterSchedulerSupport.md` #1 | `grep -rnE '\(T(Request)? (command\|@event\|request), (DateTimeOffset\|TimeSpan)' contents/`; `calls.py`, any call whose second argument is a time | **6** lines; **0** calls | **0** | 5.4, the positional-token scan |
| The Avro mapper: `CharacterEncoding.Raw` as `MessageBody`'s content type; a constructor written `AvroMessageMapper<T>(…)`, which does not parse; `SerializeAsync(request).AsSyncOverAsync()`, which is not Confluent's API; and `new AvroDeserializer<T>()` with no registry. Rewriting it exposed three more. A sync mapper is bypassed by `PostAsync`, which maps with the async default. As the default mapper, any type that is not an `ISpecificRecord` throws. And `RequestToMessageType` rejects a bare `IRequest` | Confluent.SchemaRegistry.Serdes.Avro 2.15.0; `MessageType.cs:48`; `MessageMapperRegistry.cs` `ResolveClosedDefault`; run end to end against a schema registry | `DefaultMessageMappers.md` (#4, and #10, which named `AvroMessageMapper<>` as both defaults) | `grep -rn 'AvroMessageMapper<' contents/` | **5** | **0** | 5.3 (put to the maintainer); rewritten 5.4, by ruling |
| `mappers.Regiter<…>`, `CS1061` | — | `DefaultMessageMappers.md` #11 | `grep -rn 'Regiter\b' contents/` | **1** | **0** | 5.4, reading #10's neighbour |
| `PublishAsync` relied on to reach a mapper or the bus: the default mapper said to serialise a published event, CloudEvents extensions set for a publish, a test asserting that a published event lands on the `InternalBus`, and dummy events published to warm a mapper cache. `PublishAsync` dispatches to handlers in this process | run: `PublishAsync` → **0** messages, the local handler runs; `PostAsync` → **1** (sessions 101, 102 and 5.4) | `DefaultMessageMappers.md` #2, `DispatchingARequest.md` #3, `V10MigrationGuide.md` #26, `DynamicMessageDeserialization.md` (old #7, removed) | `calls.py` over every `Publish`/`PublishAsync` call; a prose grep tying either to a bus, broker, queue, topic, transport or the wire | **13** calls, **6** of them this defect | **7** calls, **0** of them: 2 declarations, 2 in-process read-model events, a scheduled local event, 2 test-double verifications | 5.3 (put to the maintainer); swept 5.4, by ruling |
| CloudEvents extension properties said to reach the message whatever the mapper. Only `CloudEventJsonMessageMapper<>` (and the CloudEvents transform's JSON form, by reading) writes them. The default `JsonMessageMapper<>` drops both the context-bag and the `Publication` properties | `CloudEventJsonMessageMapper.cs:71`, `:80`; `CloudEventsTransformer.cs:293`; run, both sources, both mappers | `DispatchingARequest.md`, `UsingTheContextBag.md`, `CommandProcessorConfigurationReference.md` | `grep -rn -i 'CloudEventsAdditionalProperties\|extension propert' contents/` | **3** claims | **0**: each names the mapper that writes them | 5.4, running the `PublishAsync` repair's context |
| A heading with a stray `c` and an unclosed code span, `**c` + backtick + `SendAsync…` | — | `FAQ.md` | read, beside a `PublishAsync` hit | **1** | **0** | 5.4, the sweep |
| A sync handler attribute on `HandleAsync` (E4). The compiler accepts it; the pipeline throws `ConfigurationException` when built, and `ValidatePipelines()` reports it | `PipelineBuilder.cs:431`; `HandlerPipelineValidationRules.cs:110`; run | `HowServiceActivatorWorks.md` #16, `PipelineValidation.md` #9, #10, `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6, `V10MigrationGuide.md` #10 | `attr_mismatch.py`; a Python scan, paired name without `Async` then `HandleAsync` → 8, 2 now (below) | **6** | **0**; `PipelineValidation.md:250` deliberate, now wrong in prose; `V10MigrationGuide.md` #8, the skipped V9 form | design E4; 5.5 |
| A named argument ahead of a positional one, out of position: `[UseResiliencePipeline(step: 0, "RetryPipeline")]` | `UseResiliencePipelineAttribute(string policy, int step)` | `PipelineValidation.md` #9, #10 | `grep -rnE '\[\w+\(\w+: *[^,()]+, *"' contents/` | **2** | **0** | 5.1, carried |
| *Before (warning)*'s comments called step 0 *inner* and step 1 *outer*; lower steps are outer | `HandlerPipelineValidationRules.cs:76`, `:57` | `PipelineValidation.md` #9 | `grep -rnE 'step: 0.*\(inner\)' contents/` | **1** | **0** | 5.5, reading `:280` |
| The example backstop warning named `'RejectMessageOnError'` and `'UseResiliencePipeline'`; the validator prints `AttributeType.Name` | `HandlerPipelineValidationRules.cs:74`; run | `PipelineValidation.md:61` | `grep -rnE "'(RejectMessageOnError\|UseResiliencePipeline)' at step" contents/` | **1** | **0** | 5.5, the run |
| A resilience registry assigned to `PolicyRegistry`, the obsolete Polly v7 `IPolicyRegistry<string>`; and built with `TryAddBuilder<ResiliencePropertyKey<RequestContext>>`, a typed builder Brighter never looks up | `BrighterOptions.cs:56`, `:59` | `V10MigrationGuide.md` #9, #11 | `grep -rnE 'PolicyRegistry *= *\w*[Rr]esilience' contents/`; `grep -rn 'TryAddBuilder<ResiliencePropertyKey' contents/` — the key type, since a `TryAddBuilder<T>` of the result type is right (`PolicyFallback.md`'s `TryAddBuilder<Product>`, #109) | **2** | **0** | 5.5, `--explain` of § 4 |
| `IRequestContext.InstrumentationOptions` *"added in 10.7.0"*: it is after 10.7.0, forthcoming, and was unmarked | `IRequestContext.cs` at 10.7.0 (0 hits); `release_notes.md` *Master*; NuGet's latest is 10.7.0 | `V10MigrationGuide.md` | `grep -rn 'added in 10\.7\.0' contents/`; `grep -rnE '(added\|new\|introduced\|since) (in )?(Brighter )?(V?10\.7(\.0)?)' contents/` → 0 more | **1** | **0**, marked *Not in a released package yet* | 5.5, reading § 5 |
| Mappers said to have no async variants, with `Task.Run()` wrappers advised; a Proactor maps with `IAmAMessageMapperAsync<T>` and skips a sync-only mapper for the default, silently | `Proactor.cs:63`, `Reactor.cs:63`; run both ways | `ReactorAndProactor.md` #5 and its note | `grep -rniE "mappers? (don't\|do not\|never) have async\|mappers? remain synchronous\|mappers? (are\|stay) synchronous" contents/` | **2** lines | **0** | 5.5, reading around `:200` |
| Forthcoming Replay validation shown unmarked: the rule's row, two example messages and a section, with no *Not in a released package yet* | `git grep -il causation 10.7.0 -- src` → 0; on `master`, the validation rules | `PipelineValidation.md` | `grep -rln 'OnceOnlyAction.Replay' contents/` → **8** pages, each read beside `grep -c 'Not in a released package yet'`: five marked already; `DispatcherConfigurationReference.md` (`ActionOnExists`) and `BrighterOutboxSupport.md` (the Sweeper) named Replay as an option with no marker, and now say it ships after 10.7.0 | **3** pages | **0** | 5.5, ruled into phase 5 |
| The mapper's home page names only `IAmAMessageMapper<T>`; nothing there says `PostAsync`, `DepositPostAsync` and a Proactor use `IAmAMessageMapperAsync<T>` and fall back to the default mapper silently | run, all four calls and both pumps | `MessageMappers.md` | `grep -c 'IAmAMessageMapperAsync' contents/MessageMappers.md` | **0** | **3** | 5.5, ruled into phase 5 |
| Partition key, headers or CloudEvents extensions set on a handler's own `Context` with nothing posted, or on a context passed to `SendAsync`; they act only on the context passed to the call that makes the message | `CommandProcessor.cs:1533` (`requestContext ?? _requestContextFactory.Create()`); run | `UsingTheContextBag.md` #2, #3, #5, #6, #18 | `grep -rnE 'Context\.Bag\[RequestContextBagNames\.(PartitionKey\|Headers\|CloudEvents)' contents/` → 3 lines, the page's key snippets, each read; `:117`, *"Or using a PartitionKey object"*, wrote to `Context` under an example that now fills `context`, and follows it; `grep -rnE 'SendAsync\([^)]*requestContext' contents/` | **5** | **0** | 5.5, ruled into phase 5 |
| `[FallbackPolicy(backstop: true, step: 1)]` — omits `circuitBreaker`, a required parameter (`CS7036`); hidden while the blocks had no `using`, since the attribute itself did not resolve | `FallbackPolicyAttribute.cs:46` | `PolicyFallback.md` #3, #4, #8, #10 | `grep -rnE 'FallbackPolicy\(backstop: (true\|false), step' contents/` | **4** | **0** | 6.1, writing AC8's `using`s |
| Darker's decorator order read backwards: logging at step 1 said to log each retry, and retry outside fallback offered as the way to reach the fallback only after retries | Darker 4.1.1 `PipelineBuilder.cs:155` (`OrderByDescending(Step)`); run, controls the reversed steps | `QueryPipeline.md` (the ordering bullets, the Logging + Retry pattern, best practice 1, pitfall 1 and its two blocks) | `grep -rniE 'Logs everything including retries\|including retries and fallbacks\|Won.t log individual retries\|fallback will be used before retries\|you might want retry before fallback\|Log all executions, including retries\|including retries$' contents/` | **8** | **0** | 6.1, a run |
| A backstop's exception flow: *"Fallback catches BrokenCircuitException"* after the retries — the original exception reaches Fallback, `BrokenCircuitException` only on later calls | run, controls fallback inside and `circuitBreaker: true` | `PolicyFallback.md` | `grep -rn 'Fallback catches BrokenCircuitException' contents/` | **1** | **0** | 6.1, a run |
| Polly's typed fallback on a non-generic builder (`CS0029`, `CS1929`), said to hand the handler a default value through `[UseResiliencePipeline]` — it can substitute only the request | compiled; run, controls typed on the request and on `string` | `PolicyFallback.md` #10 | `grep -rn 'FallbackStrategyOptions' contents/` → every use typed with `TryAddBuilder<T>` | **1** | **0** | 6.1, a run |
| A `Config` feature switch said to run *"only if configured"* — with no registry it is On, a missing entry throws `ConfigurationException`; a `DontAckAction` said to redeliver *"after its visibility timeout"* — the pump `Nack`s it and the transport decides | `FeatureSwitchHandler.cs:68`, `FluentConfigRegistry.cs:32`, `Proactor.cs:307`; run | `FeatureSwitches.md`, `HandlerFailure.md` ×2 | `grep -rniE 'only be run if it has been configured\|re-delivers it after its visibility timeout' contents/` | **4** | **0** | 6.1, a run |
| No scheduler said to mean *"immediate requeue"*, and such transports to *"require an external scheduler"* — DI registers `InMemorySchedulerFactory` | `ServiceCollectionExtensions.cs:195`; run with `UseScheduler` removed | `BrighterSchedulerSupport.md` | `grep -rniE 'immediate requeue\|Requires an external scheduler' contents/` | **2** | **0** | 6.1, a run |
| `CharacterEncoding.Raw` presented as what keeps binary bytes intact — the byte constructors keep them under any encoding; a text-column Outbox corrupts them under both, and `binaryMessagePayload: true` is the repair | run: a UTF-8 round-trip, the Kafka bytes path, a MySQL Outbox text and binary column; control a schema id below 0x80 | `KafkaConfiguration.md`, `MessageMappers.md` | `grep -rnE 'CharacterEncoding.Raw\*\*, as a round-trip\|\*\*CharacterEncoding.Raw\*\* if not' contents/` | **2** | **0** | 6.1, a run |
| Brighter's spans said to need only `AddBrighterInstrumentation()` — the Command Processor records none without `AddProducers`; and `InstrumentationOptions` unset is `None`, unstated | `CommandProcessorBuilder.cs:304`; run, controls with and without producers, and each option | `Telemetry.md`, `ConfiguringOpenTelemetry.md` | an omission: no grep finds it; both pages that say how to enable spans now state it | **2** | **0** | 6.1, a run |
| A custom `CreateCopy` shown without `Policies`, which the shipped context copies; an `InMemoryOutbox.md` comment saying `PostAsync` only deposits | `RequestContext.cs:131`; run | `V10MigrationGuide.md` #12, `InMemoryOutbox.md` #2 | `grep -rn 'Deposit message to outbox' contents/` | **1** | **0** | 6.1, a run |
| A provider named by `typeof(...)` in `AddProducers` with nothing registered that it is built from — `IAmARelationalDatabaseConfiguration`, the EF Core `DbContext`, `IAmazonDynamoDB`, `IAmAMongoDbConfiguration`; resolving the command processor throws | constructors at 10.7.0; run, MongoDB and DynamoDB, with and without the registration | `BrighterBasicConfiguration.md` ×2, `CommandProcessorConfigurationReference.md`, `DapperOutbox.md`, `DynamoOutbox.md`, `EFCoreOutbox.md`, `MongoDBOutbox.md` ×3, `MsSqlDistributedLock.md`, `MySqlDistributedLock.md`, `PostgresDistributedLock.md`, `SweeperCircuitBreaking.md` ×2, `UsingSweeperCircuitBreaking.md` | every block with `Provider = typeof(`, read for its registrations | **15** of 28 blocks | **0** of 28 | 6.1, a run |
| MongoDB: `OnResolvingACollection.Create` — the value is `CreateIfNotExists`; `new MongoClientSettings { ConnectionString = … }` — no such property (`CS0117`) | `OnResolvingACollection.cs`; compiled | `MongoDBOutbox.md`, `MongoDBInbox.md` | `grep -rnE 'OnResolvingACollection\.Create\b\|ConnectionString = new ConnectionString' contents/` | **6** | **0** | 6.1, `--explain` on the touched blocks |

## Friction ledger

The ledger stood at **66** when 017 opened (`spec/016-compile_gate/tasks.md` § *Workflow friction*,
60–66). 017 wrote none as it went; every entry below is read from phases 1–6's *as executed*
sections and the session notes behind them, and each names where it was met. Written at `549d36f`.

| # | What got in the way | What 018 should do |
|---:|---|---|
| **67** | **AC2's key is (page, ordinal), so an insertion renumbers every later block on its page.** A BUILT block below an inserted fence reads `BUILT -> FAILED` (`Telemetry.md` #1, `MessageMappers.md` #3) and its baseline row has to be removed and re-added; a removed block is invisible, because the diff reads only the after-report's keys (2.4; 6.3's reverse join found **9**); and 5.6's **194** `FAILED -> BUILT` counts some inserted fences against an old key. Two § *Splits* rows said less than the diff reads until 5.6 rewrote them. 5.4 counted its own moves by aligning each fence's first non-`using` line against the old ref, by hand | Give `blockcheck` a diff that aligns fences across two refs by content, the way 5.4 did by hand, and prints *inserted*, *removed* and *renumbered* as verdicts of their own, with the baseline keyed the same way. Obligation 16's § *Splits* is then checked output, not a table someone remembers to write |
| **68** | **The two acceptance criteria that are readings were read only at acceptance, and both were unmet.** AC8's second half (a `// ...` added where a `using` could have been written) and AC11 (every behavioural repair run with a control) were walked at 6.1: three blocks and thirty blocks short. The ruling repaired them in phase 6, so *"no page touched"* stopped being true, and the thirty runs found a false or partial sentence in more than half of 6.1's 18 rows. Three rows lacking a control had stood in phase 2's tables since 2.5 | Run both readings in every phase's close task, with 6.1's method: the blocks the phase's diff touches (`git diff -U0` hunks against fence ranges) against the phase's run tables, and each added `// ...` against `--explain`. The acceptance walk then re-reads criteria that have already been met, which is what obligation 3 wants of a check |
| **69** | **Nothing a session made survived it.** The behaviour runs (`valrun`, `pumprun`, `avrorun`, 6.1's thirty), the helpers that fed every task from 5.3 on (`touched.py`, the input to *"`--explain` on every block the diff touches"*; `show.py`; `calls.py`), the probe's working directory (about 2 minutes to rebuild) and the script that re-ran all 116 ledger rows at 6.3 all lived in the session scratchpad and were rebuilt from the notes. So was the harness knowledge: Brighter's static `ApplicationLogging` keeps the first container's `LoggerFactory`, so every run is one process per case (3.3); Darker throws without an `ILoggerFactory` (3.5); `avrogen` refuses a schema without a namespace (5.4). A reviewer reading a *Case / Control* table has nothing to re-run | Commit, under `spec/018-*/probe/`, every helper used in more than one task, and each behaviour run's `Program.cs` beside the table row it proves, with its case and control. A run table's row then names a file. `before.tsv` stays regenerated, not kept (tasks review #1): that is a report, and these are instruments |
| **70** | **`--classify` matches a name, not a receiver.** A pinned type or extension method with the same name as a page's own satisfies *import*: `Order` resolves to `StackExchange.Redis`'s enum (**27** blocks on 16 pages at 1.10; **22** on 10 at 6.2); `AddOpenTelemetry` ×6, pinned only on `ILoggingBuilder` (6.2); `Build` on OpenTelemetry's builders (6.2); a member access `Id` (5.1). The same blind spot reached a compile: `BuildingAPipeline.md` #3's `using` resolved Brighter's own `RequestLoggingHandler<>`, not the one block 1 writes, and the probe called it BUILT (5.2). **Ruled 2026-09-27: not repaired.** Every phase carried an `awk` exclusion in the tranche recipe and a reading rule in every repair task, and each pin growth could add a collision nobody looks for | Put the repair to the maintainer again, now with 017's three families as evidence: `--classify` reads the receiver the Roslyn half already has, and a pin change lists the pinned names that collide with a name a page uses as its own before any tranche reads it |
| **71** | **The tranche lists were decided by a probe in `spec/`, with the committed instrument as their lower bound.** At 1.10 `--classify` read **162** blocks as *import* that the probe, supplying the `using`s, found a DEFECT behind, and 16 tranche-2 pages as 0-hard. Every hard-block table (4.1, 5.1) needed three columns — `--classify`, probe, reading — and the three disagreed each time. The probe was wrong in its own way too: `RequestValidation.md` #10 HIDDEN (2.6), `BrighterControlAPI.md` #1 hard (5.1) | Give `--classify` the probe's second stage — supply the `using`s, compile again, classify what remains — so that one committed, red-proofed instrument decides what 018's tranches hold |
| **72** | **A prediction scoped to the tranche could not see the work it would cause.** `pagelint` came in below its band at 2.6 (−17), 3.6 (−6) and 4.5 (−6); BUILT above its ceiling at 4.5 (+4) and 5.6 (+1); 5.1's *"no pin change"* grew the pin twice. The mechanism is the same each time: obligation 14's recurrence grep lands off the tranche, `pagelint --changed` then asks rule 6 of every block it touched, and some of those build. From 3.1 on the predictions said *"lower further by recurrences, explained"* — a clause no outcome can fail | Predict the off-tranche term from the recurrence greps, which 4.1 and 5.1 already ran before any repair: their tables name the pages and blocks a recurrence repair will touch, so their rule 6 warnings and BUILT candidates can be counted then. A band that includes them can miss |
| **73** | **Every off-tranche finding stopped its task for a ruling, and the rulings converged.** 3.4, 3.5, 4.2, 4.4 (twice), 5.2, 5.3, 5.4, 5.5 and 6.1 put off-tranche findings to the maintainer; 5.2 and 5.5 each needed a second pass after the answer. Every ruling on an off-tranche defect but two said *repair it in this PR* — the two are 3.5's `[RetryableQuery]` row, carried to phase 5, and 5.3's eleven subscriptions, recorded. The backwards check (6.4) then had to justify **45** pages that are neither tranche nor P0-7 after the fact, by ledger row | Write the default the rulings reached into 018's standing obligations: a defect verified at the pinned release is repaired at every recurrence in the phase that finds it, and only a change of scope — a page rewritten, a feature removed, an issue filed upstream — is put to the maintainer. Give each repair phase a *second pass* task, so that a ruling has somewhere to land |
| **74** | **Defect-ledger rows were written before their greps had been run as written.** At 6.3, **#15** and **#16** still read *"open"* with no grep, though both were repaired, as **#82** and **#83**; #24, #55 and #61 named no grep; #100's matched a correct repair. The `[RetryableQuery]` row's grep matched the instance, not the defect (said 9 lines, measured 20, 5.1). #7's *Page* column read *"11 pages"*, so no join by name could find it (6.5). Closing them took task 6.5 and two new probes, one of which failed its first control | A row is written only once its grep or probe has run, as written, and read the *After* the row states; its *Page* column names pages; a defect carried to a later phase is closed by editing its row, not by opening a second one |
| **75** | **The `statements` wrapper declares no `Program`.** A `Program.cs` whose statements have no declaration after them, and which names `Program` (`typeof(Program).Assembly`), fails `CS0246`: `QueryPipelinePolicies.md` #1, `TickerQScheduler.md` #2, `DarkerConfigurationReference.md` #1, #2 and `QueryHandlerDependencies.md` #3, each built as a real `Program.cs` in scratch at **0** errors. Q2 (1.8) rules out a stub, so they are listed FAILED against the instrument, and 4.1's pin prediction missed by one on it | Stage a block of top-level statements that names `Program` as a top-level file, where the compiler synthesises `Program`, as P1-3 answered `args` by giving the wrapper's method the parameter |
| **76** | **The pin is not the world the reader builds in, and the difference lives in prose.** 15 of § *Blocks that stay FAILED*'s rows are FAILED for the instrument or the pin — the five above, four V4-family blocks (D3) and six OpenTelemetry blocks (5.3, ruling 2) — and 14 of them compile in scratch against the released packages at 0 errors, a second build that no gate records. The pin is `net9.0`; every behaviour run was `net10.0`, where a reader of `TickerQScheduler.md` gets TickerQ 10.4.0, not the pin's 9.0.2 (3.4) | Besides D3's V4 pin, make *compiles against the released packages, not in the pin* a verdict the gate reports and re-checks, and measure what the `net9.0` pin misses against `net10.0` before 018's tranches are drawn |
| **77** | **A scaffold unit that does not compile is a warning.** 5.2's first Darker handler stub declared an `Execute` no block names and put **8** errors against the scaffold tree; `--report` printed *"WARNING: 8 error(s) reported against scaffold trees, not counted against any block"* and exited 0. The unit rule caught the member; nothing caught the compile | Make an error against a scaffold tree a finding, exit 1, as a unit-rule violation is. A unit that does not compile can fail its page's blocks for a reason that is not the page's |
| **78** | **One unit per page assumes one world per page.** `pages.tsv` maps a page to one unit. `CQRSUseCasesAndPatterns.md` #1 and #2 assume different unshown `Order` models, and a unit that named #1's would not compile beside #2, so #1 was rewritten to declare its own query and `DbContext` (5.2). The page is better for it, but the instrument, not the reader, chose the change | Let `pages.tsv` map a unit per block where a page's blocks assume different worlds, so that a page is rewritten because a reader needs it, never because the scaffold cannot serve it |
| **79** | **A criterion named the deliberate hit by its line.** AC9 and 5.5's output read *exactly `PipelineValidation.md:250`*. 5.5's second pass put a Replay note after the example messages, moved the hit to `:252`, and rewrote the note into an existing line so that nothing above `:250` changed length. The design's E4 lines had drifted before phase 5 began (`ReactorAndProactor.md:190` → `:200`), and a `using` moved another hit `:326` → `:359` (5.2). A line number shaped where prose could go | Whichever form 6.7's D4 takes, exempt the deliberate example with a marker on the page, as `pagelint`'s `allow-serviceactivator` does, and name a hit in a criterion by page and block, never by line |
| **80** | **Two instruments cannot say what the work needed them to.** `pagelint` has no per-page output, so every task that reconciled rule 6 did it *"per page, against a worktree at"* the phase's base — **15** times, from 2.6 to 6.3. `optioncheck` could not mark `AzureBlobArchiveProvider.md`'s new options table: *"CANNOT CONSTRUCT"* for a primary-constructor argument and *"ROW NAMES NOTHING"* for two fields, so the table ships unchecked (4.4) | A `pagelint` per-page count, so a delta is a diff of two outputs. `optioncheck` construction of primary-constructor options, and rows that name fields |

**Recurrences, not numbered.** **Friction 53** — the fence spelled `` ``` csharp ``, with a space, which
the obvious pattern misses: `asyncscan.py` failed its first control on `FeatureSwitches.md` #2 for it
(6.5). It is also obligation 9 recurring, since the probe enumerated fences with its own pattern
rather than through `pagelint.Page`. **Friction 58's family** — zsh is not the shell a recipe was
written for: `"$r:contents/…"` applies zsh's `:c` modifier to the variable, and has to be written
`"${r}:contents/…"` (session notes, phase 5).

```bash
# 66 + 14 (67–80) = 80
grep -cE '^\| \*\*(6[7-9]|7[0-9]|80)\*\* \|' spec/017-compile_repairs/tasks.md     # 14
```

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
