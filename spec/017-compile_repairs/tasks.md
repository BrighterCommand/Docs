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

- [ ] **Task 2.2:** Repair the *Get Started* tranche pages
  - Input: the phase 2 table's *Get Started* rows; `--classify` on each; `samples/CommandProcessor/HelloWorld`
    for `TutorialFirstCommand.md`
  - Output: each page whole — `--classify <page>` lists only blocks named in § *Blocks that stay
    FAILED*; their BUILT rows in `baseline.tsv`; `versioncheck` exit 0 at 18 pins
  - Notes: at § 2's pin, three tutorials' only FAILED blocks are same-page. Each gets the one
    sentence design § *Page Repair Rules* asks for **only if the page does not already say it**.
    Obligation 17 applies to every code change here.

- [ ] **Task 2.3:** Repair the *Commands, Handlers and Pipelines* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; stubs in `tools/blockcheck/scaffold/units/<Page>Context.cs`
    with their `pages.tsv` rows, `--report` exit 0 under the unit rule
  - Notes: `RequestValidation.md` sits in this section in `SUMMARY.md`; if 1.10 places it in
    tranche 1 it is this task's largest page.

- [ ] **Task 2.4:** Repair the *Brighter Configuration* and *Health Checks and Observability* tranche pages
  - Input: those sections' rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [ ] **Task 2.5:** Repair the *Using an External Bus* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [ ] **Task 2.6:** Close phase 2 — checks, figures, PR
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

---

## Phase 3 — Tranche 1b *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 0-hard-block page in the remaining four sections leaves whole. Page lists:
§ *The tranches*, phase 3 table. Tasks 3.2–3.5 are independent of each other.

- [ ] **Task 3.1:** Predict phase 3's movement
  - Input: § *The tranches*, phase 3 table; § *Phase 2 as executed*
  - Output: § *Phase 3 as executed* opens with a prediction per gate, from `master` after phase 2

- [ ] **Task 3.2:** Repair the *Transports* tranche pages
  - Input: that section's rows; `--classify` on each; `RelationalTransportContext.cs`, which already
    serves two pages
  - Output: each page whole; baseline rows; stubs share an existing unit where the pages share a
    world (design § *Scaffold Stub Rules*)

- [ ] **Task 3.3:** Repair the *Outbox and Inbox* tranche pages
  - Input: that section's rows; `--classify` on each
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [ ] **Task 3.4:** Repair the *Scheduler* tranche pages
  - Input: that section's rows; `--classify` on each; the Hangfire and Quartz packages 1.8 pinned
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows
  - Notes: BrighterCommand/Brighter#4414 is upstream's. A scheduler block that asserts a scheduled
    request runs through DI is run, and if it throws for #4414 the block stays FAILED or the claim
    is reworded — never shown working.

- [ ] **Task 3.5:** Repair the *Darker* tranche pages
  - Input: that section's rows; `--classify` on each; `../Darker` at `4.1.1`
  - Output: each page whole; baseline rows; any stubs with their `pages.tsv` rows

- [ ] **Task 3.6:** Close phase 3 — checks, figures, PR
  - Input: 3.1's prediction; the PR's diff
  - Output: as 2.6, for phase 3; plus **the ≤ 60 target read at tranche 1's close** — pages with
    nothing BUILT, by requirements' `awk`, against the 37 that must land

---

## Phase 4 — Tranche 2a *(5 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 1-hard-block page in *Outbox and Inbox* leaves whole, its one hard block repaired,
skipped with an accepted reason, or listed. Page list: § *The tranches*, phase 4 table.

- [ ] **Task 4.1:** Predict phase 4's movement
  - Input: § *The tranches*, phase 4 table; § *Phase 3 as executed*
  - Output: § *Phase 4 as executed* opens with a prediction per gate, **and a verdict per hard block
    before it is touched**: parse (placeholder / fragment / not code) or other (defect / wrapper
    artefact), from `--classify` and `--explain`

- [ ] **Task 4.2:** Repair the outbox and inbox pages of the tranche
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

## Splits

*One row per split fence: page, old ordinal, new ordinals.*

## Defect ledger

*One row per defect: defect, page, recurrence grep, before, after, found by.*

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
