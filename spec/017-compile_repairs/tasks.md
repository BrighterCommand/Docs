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
— design tranche 1's largest page, 14 reachable blocks — is absent at the committed pin**, because
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
are, not in half**: at the committed pin 22 of its 34 pages are in *Outbox and Inbox*, and they
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

- [ ] **Task 1.1:** Re-derive the starting state and write phase 1's prediction
  - Input: `tools/README.md` rows 1–9; `design.md` § *Gate Movement Predicted*, Phase 1 column
  - Output: § *Phase 1 as executed* in this file, opening with all nine gates' exit codes and
    figures at `master`, and a prediction row per gate. **§ *The AC2 before-report*** below holds
    the command that regenerates `before.tsv` from `c7329bb` in a worktree, and its summary line
    (989 / 101 / 872 / 16), so that every later AC2 diff can rebuild it and check it is the same
    report
  - Notes: the prediction for gate 9 is *BUILT unchanged at 101*; the one worth watching.
    `before.tsv` is **regenerated at each use, never kept**: a file under `$TMPDIR` does not survive
    between sessions (§ 2's work directory did not).

- [ ] **Task 1.2:** Add the `--types` mode to `tools/blockcheck/Program.cs`
  - Input: `spec/017-compile_repairs/probe/typedump/Program.cs`; `design.md` § *The Classify Mode*
  - Output: `dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll --types <refs.txt>` prints
    `type|ext<TAB>Name<TAB>Ns` rows; **two methods agree**: `diff` against the probe's `typedump`
    output over the same `refs.txt` is empty, recorded with both line counts. **Control, both
    ways:** `UseResiliencePipelineAsyncAttribute` and the `AddBrighter` extension are present, and
    `ITimerProvider` is absent
  - Notes: it skips `refs.txt`'s `#` stamp line, as the probe had to.

- [ ] **Task 1.3:** Add `--classify [file]` to `tools/blockcheck.py`, with its red-proof
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

- [ ] **Task 1.4:** Add `--list-skips` to `tools/blockcheck.py`, with its red-proof
  - Input: `design.md` § *The List Skips Mode*; `scan_skips` in `tools/blockcheck.py`
  - Output: the mode; recorded here — `--list-skips > s; echo $?` → **0**, and `wc -l < s` equals
    `--report`'s SKIPPED (16); **control, both ways:** `--list-skips contents/FAQ.md`, which
    carries skips, → exit **0** and one row per skip; `--list-skips contents/SpannerOutbox.md`,
    which carries none, → exit **2**
  - Notes: it must read `scan_skips`, not re-scan, so the two cannot disagree.

- [ ] **Task 1.5:** Expose `--explain` through `tools/blockcheck.py` and correct its summary (P1-2)
  - Input: `Program.cs` `--explain`; requirements § *Instrument quirks recorded by 016*
  - Output: `python3 tools/blockcheck.py --explain <id>...` works; its stderr reads *"N blocks
    explained"*; **control, both ways:** the old binary's line (*"989 blocks, 0 built, 989
    failing"* for 872 ids) and the new one, both recorded
  - Notes: ids go as separate arguments — the zsh `$ids` trap from session 84 does not apply to a
    Python front end, which is part of the reason to have one.

- [ ] **Task 1.6:** Enforce the unit rule in `--report`, with four plants
  - Input: `design.md` § *The Unit Rule, Enforced* (rules 1–4, output form, exit 1)
  - Output: `--report` prints `SCAFFOLD RULE: <unit>: <what>` and exits **1** on a violation.
    Recorded here: the four plants — a type no block names, a type a block on its page declares, an
    unnamed member, a `global using` — each → exit **1**; **positive control on the real tree:**
    before 1.7, `PageContext.cs` → **26** violations and the other 13 units → none
  - Notes: depends on nothing; 1.7 depends on it. The 26 is the design's dry-run figure;
    re-derive it by the rule as built, and record any difference.

- [ ] **Task 1.7:** Cut `PageContext.cs` to `connectionString` and rewrite `pages.tsv`'s rule comment
  - Input: `tools/blockcheck/scaffold/units/PageContext.cs`; `pages.tsv`'s comment;
    `python3 tools/blockcheck.py --show contents/DapperOutbox.md 1`; requirements Q6, P0-3
  - Output: `PageContext.cs` declares `connectionString` and nothing else; `pages.tsv`'s comment
    states rules 1–4 in place of "no domain type"; `--report` → exit **0** with the unit rule on
  - Notes: same PR as 1.6 — otherwise 1.6 turns `master` red (`tools/README.md` rule 3).

- [ ] **Task 1.8:** Grow the pin, measured alone
  - Input: `design.md` § *E2* and § *Phases* (the package list, less Jaeger by D2, less the AWS V4
    family by D3, plus the five Brighter packages); `tools/blockcheck/refs/refs.csproj`
  - Output: `refs.csproj` carries the packages, each at the version the design states (Quartz
    3.18.1, Hangfire 1.8.24, EF Core ≥ 9.0.15); recorded here — assembly count (design: **538**),
    and the AC2 `awk` diff of `--report` before and after → **no line printed** (design: 0 verdicts
    change). **Control:** the same diff over a copy with one row flipped prints exactly that row
  - Notes: a verdict that moves here moved for a reason nobody repaired. Stop and explain it before
    going on.

- [ ] **Task 1.9:** Update `tools/README.md` — row 9, § *The other modes*, the AC3 and AC10 commands
  - Input: `tools/README.md` row 9 and § *Reading a number before you trust it*; `design.md`
    § *The List Skips Mode* (last paragraph); the outputs of 1.3–1.8
  - Output: row 9's scope names the grown pin and the unit rule; § *The other modes* documents
    `--classify`, `--list-skips`, `--explain`; the two corrected commands are written in with the
    reason the old form fails. Recorded here, **old beside new**:
    `ls tools/blockcheck/*.csproj | wc -l` → 1 and `find tools/blockcheck -name '*.csproj' -not -path '*/obj/*' | wc -l`
    → 2; `grep -vc '^./spec/'` and `grep -vcE '^(\./)?spec/'` over the same `grep -rn` output, the
    first excluding nothing; `grep -cF '^(\./)?spec/' tools/README.md` → ≥ 1
  - Notes: `--list-skips`'s own instrument is 1.4's.

- [ ] **Task 1.10:** Fix the tranche page lists against the committed pin
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

- [ ] **Task 1.11:** Run the nine gates, reconcile phase 1, open the PR
  - Input: 1.1's prediction; `tools/README.md` rows 1–9
  - Output: § *Phase 1 as executed* closes with each gate's exit code and figure against its
    prediction; the PR open, green, no page under `contents/` in `git diff --name-only master`
  - Notes: no sign-off is owed (no published page); the `--admin` grant covers the merge.

---

## Phase 2 — Tranche 1a *(6 tasks, one PR, CHANGES THE SITE)*

**Goal:** every 0-hard-block page in the first five `SUMMARY.md` sections leaves whole. Page lists:
§ *The tranches*, phase 2 table (1.10). Tasks 2.2–2.5 are independent of each other.

- [ ] **Task 2.1:** Predict phase 2's movement
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

*Written by task 1.1: the command that regenerates `before.tsv` at `c7329bb`, and the summary line
a regenerated copy must match.*

## The tranches

*Written by task 1.10.*

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
