# Spec 018: Compile Residual — Tasks

**Status:** Draft, for `/spec:review`
**Requirements:** approved 2026-10-04 · **Design:** approved 2026-10-05

**Eight phases, 61 tasks, one pull request per phase.** Phases merge under `tools/README.md`
§ *One phase is one pull request*, in order, each before the next branch starts. Phase 1 changes one
page and phase 2 changes none. Phases 3–7 change the published site, and each needs the
maintainer's sign-off, with the head-ref deletion asked for by name.

---

## 1. Standing obligations — stated once, binding every task

They are not repeated per task. A task that seems to need one restated is a task that has
forgotten this section.

**The programme's seven:**

1. **Re-derive any count before quoting it**, with the command beside the figure and **two methods
   that agree**. A figure inherited from `requirements.md`, `design.md` or an earlier phase is stale
   until re-derived
2. **Record the mismatch before fixing it**, once, as a fact (*said A; measured B*), in this file's
   ledgers. Then rewrite the text. Do not narrate how it was found, and do not leave the original
   standing beside the correction
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof
   with its output recorded here, and **every control is two-way**: a known-present case and a
   known-absent one
4. **Prose and permission ship together**, read in both directions. Any command file a phase
   touches is checked: every tool its prose names is granted, and every grant is reached by its
   prose
5. **Cite `CLAUDE.md`, `tools/README.md` and `design.md`; never restate them**
6. **Predict gate movement before the work, including "none"**, with the mechanism. Then
   reconcile, and explain any difference rather than adopting the new number
7. **Ask before merging anything that changes the published site**, and ask for the head-ref
   deletion **by name** in the same breath

**Inherited, by citation:** 016's obligations 8–12 (`spec/016-compile_gate/tasks.md` § 1), and 017's
13, 14, 16 and 17 (`spec/017-compile_repairs/tasks.md` § 1). 017's 15, *"`probe/attr_mismatch.py`
runs before any baseline row is written"*, is **replaced** by rule 8 from phase 1 on, which runs on
every push.

**018 adds six, from 017's friction ledger** (requirements P0-6):

18. **#69: an instrument outlives its session.** Every helper used by more than one task, and each
    behaviour run's `Program.cs` with its case and its control, is committed under
    `spec/018-compile_residual/probe/`. A run table's row names its file
19. **#68: the reading criteria run at every phase close**, not only at acceptance. The phase's
    changed blocks (`git diff -U0` hunks against fence ranges) are read against AC9 (each added
    `// ...` against `--explain`) and AC10 (each behavioural claim against a run with a control)
20. **#72: a prediction includes the off-group term.** It is predicted from the recurrence greps
    run before any repair, which name the pages and blocks a recurrence will touch
21. **#73: a verified defect is repaired at every recurrence in the phase that finds it.** Only a
    change of scope (a page rewritten, a feature removed, an issue filed upstream) is put to the
    maintainer. Every repair phase has a *second pass* task where a ruling lands
22. **#74: a defect-ledger row is written only after its grep or probe has run as written.** Its
    *Page* column names pages. A carried defect is closed by editing its row, never by opening a
    second
23. **#79: a hit is named by page and block, never by line**, in criteria, predictions and
    ledgers

---

## 2. Measured at tasks — 2026-10-05, `1b6f7f9`

`git diff --stat 3a79b20 1b6f7f9 -- contents tools` → empty, so no verdict can have moved since
the design measured.

| Figure | Method 1 | Method 2 | Agrees? |
|---|---|---|---|
| BUILT | `--report` rows, `cut -f1 \| uniq -c` → **299** | `grep -vc '^#' tools/blockcheck/baseline.tsv` → **299** | **yes** |
| FAILED | `--report` rows → **674** | `--classify \| wc -l` → **674** | **yes** |
| SKIPPED | `--report` rows → **17** | `--report`'s *"17 skipped"* line | **yes** |
| `pagelint` debt | summary line → **524 blocks, 66 pages** | `grep -c 'USING DIRECTIVES'` over its output → **524** | **yes** |
| Rule 8's input | `attr_mismatch.py` → **1**, `PipelineValidation.md` block 7 | design E4: the same block, by `--list` | **yes** |
| Off-tranche FAILED / pages | design E1 → **630 / 64** | `probe/offtable.py` row count → **64**, `failed` column sum → **630** | **yes** |

---

## 3. The phases

**Why not Research → Core → Supporting → Polish.** That default would split each deliverable across
phases. The design has three deliverable shapes instead: a gate and the one page that satisfies it
(phase 1, `tools/README.md` rule 3); instruments that must exist before any table is drawn (phase 2,
AC8); and repairs grouped by `SUMMARY.md` section (phases 3–7, design § *Target And Phases*).
Acceptance is last.

```text
Phase 1  rule 8                        8 tasks   one page (the marker)   sign-off
Phase 2  instruments                  15 tasks   no page                  —
Phase 3  S1 Scheduler                  7 tasks   site                     sign-off
Phase 4  S2 Commands …; Get Started    6 tasks   site                     sign-off
Phase 5  S3 Darker; Understanding      6 tasks   site                     sign-off
Phase 6  S4 External Bus; Transports   6 tasks   site                     sign-off
Phase 7  S5 the remaining five         7 tasks   site                     sign-off
Phase 8  acceptance                    6 tasks   no page                  —
                                      ── 61
```

**Dependencies.** Phase 2 depends on phase 1 only through the merge order. Phases 3–7 each need phase
2's tables (task 2.14). Within phase 2, tasks 2.2, 2.5, 2.6 and 2.7 are independent of one another.
2.1 comes before 2.13, 2.2 before 2.3 and 2.4, and all of them come before 2.14. Within a repair
phase, N.1 comes first and the close comes last; the tasks between can run in any order.

---

## Phase 1 — Rule 8, `ATTRIBUTE KIND` *(8 tasks, one PR, changes one page)*

Design § *Rule 8*. **Predicted:** every gate unmoved, except a new `pagelint --plant` (exit 0).
`pagelint` reads **0 errors, 524 warnings, 162 pages** at both ends, because the marker is an HTML
comment and changes no block's text.

- [ ] **Task 1.1:** Add `PAIRED` and the `ATTRIBUTE KIND` check to `tools/pagelint.py`
  - Input: design § *Rule 8* (what it reads, what it reports, `PAIRED`, the message);
    `spec/017-compile_repairs/probe/attr_mismatch.py` (`scan()`, moved unchanged);
    `tools/pagelint.py` `:164` (`APPLIES_TO`), `:475` (`check_code_blocks`)
  - Output: `PAIRED` beside `APPLIES_TO` with its derivation comment, and a check reporting
    `ATTRIBUTE KIND` as an error repo-wide and under `--changed`. `python3 tools/pagelint.py` →
    exactly **1** error, on `PipelineValidation.md` block 7, recorded here as the rule's first red
    run
  - Notes: `PAIRED` is re-derived at both refs as you write it (§ 1 obligation 1); the design's 13
    are the expected answer

- [ ] **Task 1.2:** Add the `attr-mismatch-intended` opt-out and its three faults
  - Input: design § *Rule 8*, *The opt-out*; `tools/blockcheck.py:280–300`, the binding it copies
  - Output: a marker binding to the next C# block below it. A marker with no reason, a marker with no
    C# block after it, and a second marker on one block are each an error. Honoured markers print
    with their reasons and a count line, *"N block(s) marked attr-mismatch-intended"*

- [ ] **Task 1.3:** Add `pagelint --plant` and record its red-proof
  - Input: design § *Rule 8*, the five-plant table; 1.1 and 1.2
  - Output: `python3 tools/pagelint.py --plant; echo $?` → **0**, all five plants behaving. A
    red-proof run, with one plant's expectation inverted in a scratch copy, → **1**, recorded here
    with its output (AC1, AC3)

- [ ] **Task 1.4:** Mark `PipelineValidation.md` block 7
  - Input: design § *Rule 8*, *The page*
  - Output: the marker line above the fence of block 7. `python3 tools/pagelint.py` → **0
    errors**, *"1 block(s) marked attr-mismatch-intended"*. Recorded here: the run with the marker
    removed reports `ATTRIBUTE KIND` on block 7 (AC2). `blockcheck --report` is unmoved, at 990 / 299
  - Notes: changes the published site. Its sign-off is asked for in 1.8's PR

- [ ] **Task 1.5:** Run rule 8 in CI
  - Input: `.github/workflows/docs.yml`, the `check` job
  - Output: a bare `- run: python3 tools/pagelint.py --plant` step after the `pagelint` step, with
    a comment citing design § *Rule 8*

- [ ] **Task 1.6:** Write rule 8 into `CLAUDE.md`
  - Input: design § *Rule 8*, *`CLAUDE.md`*; `CLAUDE.md` § *The ledger* and § *Complete code blocks*
  - Output: one ledger row, and § *Handler attributes match their handler* after *Complete code
    blocks*. Then `grep -rn 'rule 7\|seven rules\|rules 1' .claude/commands/` for any command quoting
    a rule count this changes, each hit read and fixed (the *Writing Review* rule)

- [ ] **Task 1.7:** Update `tools/README.md` for rule 8
  - Input: `tools/README.md` row 2, § *What each gate actually checks*, § *The other modes*
  - Output: row 2 with the phase's ref (warnings unmoved); `pagelint`'s bullet naming rule 8; *The
    other modes* listing `pagelint.py --plant`. AC14's corrected-form count → **1** for any new figure

- [ ] **Task 1.8:** Close phase 1
  - Input: § 1 obligations 6, 7, 19; the phase's prediction
  - Output: § *Phase 1 as executed*, with every gate's figure against the prediction, and the
    reading criteria (AC9/AC10: **no block's text changed**, so none applies, stated). Then the PR,
    asking for sign-off (one page changed) and for deletion of its head ref by name

---

## Phase 2 — Instruments *(15 tasks, one PR, no page changes)*

Design § *The Second Compile*, § *The Handler Wrapper*, § *The V4 Pin*. **Predicted:** `blockcheck`
BUILT rises by the blocks the handler wrapper, Shouldly and the `Program` fix make build, a set 2.1
measures before any row; FAILED falls by the same; the corpus is unmoved, at 990. The main pin's
assemblies rise by Shouldly's, and there is a new scope line for the V4 pin. Every other gate is
unmoved. Tasks 2.8–2.11 are **P1**: one that does not fit is carried to the residual with its
reason, and does not hold the phase.

- [ ] **Task 2.1:** Measure what phase 2's staging changes will build, before any row
  - Input: design § *Gate Movement Predicted*, phase 2's BUILT row; E5's 32 blocks; E2's 6 Shouldly
    blocks; `--list` for blocks naming `Program`
  - Output: § *Phase 2 as executed*, *Predicted BUILT*: the block list each of 2.5, 2.7 and 2.8 is
    expected to make BUILT, with how it was found. This is the set 2.13 reconciles against

- [ ] **Task 2.2:** Give `--classify` its second compile, receiver-aware
  - Input: design § *The Second Compile* (classes, five columns, resolution); `tools/blockcheck.py`
    `:254`, `:1198–1204`; `spec/017-compile_repairs/probe/usings.py` (the `using` supply it
    promotes)
  - Output: `--classify` printing `page·ordinal·class·first·names` with the six classes, its
    per-class counts on stderr, exit 0, and exit 2 on nothing to classify. A name is resolved only
    when the page does not declare it and the staged base does not supply it

- [ ] **Task 2.3:** Red-proof the second compile (AC6)
  - Input: design § *The Second Compile*, *Red-proof*
  - Output: § *Phase 2 as executed*, a five-row table (block, expected class, actual class) with
    exactly one row per class named, plus the `Order` control. A second run from a scratch copy with
    the page-declared check removed turns the control `built-by-using`, recorded. The scratch copy is
    not committed

- [ ] **Task 2.4:** Reconcile `--classify` with the 018 probe (AC7)
  - Input: `probe/run.sh`'s `verdicts.tsv` at the same ref; 2.2's output
  - Output: `spec/018-compile_residual/probe/reconcile.py` (obligation 18), and § *Phase 2 as
    executed*, a disagreement table: every block where the two differ, each with its reason. E4's
    39 and 29 are expected, in the receiver-aware direction

- [ ] **Task 2.5:** Add the handler wrapper (P0-7)
  - Input: design § *The Handler Wrapper*; `tools/blockcheck.py:263`, `:355`
  - Output: members blocks overriding the four methods are staged with their fully qualified
    base. § *Phase 2 as executed* records how many of the 32 are wrapped, and names each one that is
    not. Red-proof: four plants, one per base, each staging and building. Control: `--stage` output
    byte-identical to `1b6f7f9` for every members block that is not a handler (`cmp`, silent)

- [ ] **Task 2.6:** Add the V4 pin (P0-5)
  - Input: design § *The V4 Pin*; `spec/018-compile_residual/probe/v4pin.sh`;
    `tools/blockcheck/scaffold/pages.tsv`
  - Output: `tools/blockcheck/refs-v4/refs-v4.csproj` with its comment; a `v4` cell on the four
    pages; `--report` enforcing the pairing rule and printing the V4 assemblies line. Red-proof,
    two-way: a V4 page unmarked, and a marked page with no V4 block, are each a finding (exit 1);
    the corrected map → exit 0. AC11's `grep -c 'CS0234.*V4'` over `--explain` of the six → **0**

- [ ] **Task 2.7:** Pin Shouldly (P0-8)
  - Input: design § *Page Repair Rules*, the Shouldly row; `tools/blockcheck/refs/refs.csproj`
  - Output: `Shouldly` `4.3.0` in `refs.csproj`, with a comment citing Brighter's
    `Directory.Packages.props:150`. The restore is clean and the assembly count is recorded. No page
    is touched; `TestDoubleOptions.md`'s repair is 7.4's

- [ ] **Task 2.8:** *(P1)* Let a `statements` block name `Program` (friction #75)
  - Input: friction #75 (`spec/017-compile_repairs/tasks.md` § *Friction ledger*); `WRAPPERS`
  - Output: such a block staged as a top-level file. Red-proof: one block naming `Program` builds;
    control: a `statements` block that does not name it stages exactly as before (`cmp`)

- [ ] **Task 2.9:** *(P1)* Make a scaffold unit that does not compile a finding (friction #77)
  - Input: friction #77; the unit rule in `--report`
  - Output: a compile error in a scaffold unit printed as a finding, exit 1. Red-proof: a planted
    broken unit → exit 1; the real units → **0** such findings at `1b6f7f9`, recorded

- [ ] **Task 2.10:** *(P1)* Give `pagelint` a per-page warning count (friction #80)
  - Input: friction #80; `tools/pagelint.py` `main`
  - Output: a flag printing `page<TAB>warnings`, sorted, whose column sums to the summary line's
    **524**: two methods that must agree

- [ ] **Task 2.11:** *(P1)* Measure the `net9.0` pin against `net10.0` (friction #76)
  - Input: design D7; `refs.csproj`'s `TargetFramework`
  - Output: § *Phase 2 as executed*, *net10.0*: a scratch `net10.0` build of the pin, the packages
    that resolve to a different version, and `--report`'s verdicts under it against the `net9.0`
    ones. If any verdict moves, it is put to the maintainer before 2.14

- [ ] **Task 2.12:** Update CI and `tools/README.md` for phase 2's instruments
  - Input: `.github/workflows/docs.yml` `blocks` job; `tools/README.md` row 9, § *Reading a number*,
    § *The other modes*, the `--classify` bullet
  - Output: a `dotnet build tools/blockcheck/refs-v4/refs-v4.csproj -c Release` step before the
    gate. Row 9 at the phase's ref. The fourth scope line explained. `--classify`'s description
    rewritten for five columns and six classes. AC14's count → **1** for each new figure

- [ ] **Task 2.13:** Baseline what phase 2 made build
  - Input: 2.1's predicted set; `--report` after 2.5–2.8
  - Output: rows in `tools/blockcheck/baseline.tsv` for exactly the blocks that now build;
    `--report` exit 0. § *Phase 2 as executed* reconciles them against 2.1, block by block. Each
    new BUILT block is clean under rule 8 (`pagelint` 0 errors)

- [ ] **Task 2.14:** Draw the S1–S5 page tables from `--classify` (AC8)
  - Input: `--classify` at phase 2's head; design § *The section groups*; `SUMMARY.md`
  - Output: § *The groups*, one table per group, every column derived from `--classify` by the
    command written above it, and no probe column. The three tranche pages with V4 blocks are in S4
    and S5. The tables' FAILED column sums to `--report`'s FAILED on the 64 + 3 pages

- [ ] **Task 2.15:** Close phase 2
  - Input: § 1 obligations 6, 19; the phase's prediction
  - Output: § *Phase 2 as executed*, gates against the prediction; readings (no page touched,
    stated); the PR, asking for deletion of its head ref by name

---

## Phases 3–7 — the section groups

**Each repair phase has the same spine.** N.1 predicts; N.2 and N.3 repair, under design § *Page
Repair Rules* and 017's rules (obligation 13); N.4 runs behaviour; N.5 is the second pass
(obligation 21); the last task closes. The page list is § *The groups*' table for that phase, drawn
at 2.14 and not before. **Every phase changes the site.**

### Phase 3 — S1, Scheduler *(7 tasks, one PR)*

- [ ] **Task 3.1:** Predict phase 3's gate movement
  - Input: § *The groups*, S1; the recurrence greps for every DEFECT in S1's table (obligation 20)
  - Output: § *Phase 3 as executed*, *Predicted*: BUILT, FAILED, corpus and `pagelint` debt as
    bands, the off-group term listed by page and block, and `symbolcheck` **22 → 23 entries, 3 → 8
    silenced** (design E6)

- [ ] **Task 3.2:** Repair S1's reachable blocks
  - Input: S1's table, the `built-by-using`, `values` and `page-type` rows; 017 § *Scaffold Stub
    Rules*
  - Output: each block given its `using`s in the block, or a stub in a scaffold unit. Each leaves
    BUILT, or listed in § *Blocks that stay FAILED* with its diagnostic

- [ ] **Task 3.3:** Repair S1's hard blocks
  - Input: S1's table, the `parse` and `defect` rows; design E2's buckets; 10.7.0 source for each
    defect
  - Output: each block in one of the four states. Literal `...` rewritten as `// ...`. The three
    JSON fences (`AwsScheduler.md` #13, #14, `AzureScheduler.md` #19) retagged `json`. Splits in
    § *Splits*, defects in § *Defect ledger* with their recurrence greps. `AwsScheduler.md` #2 and
    #3, the V4 blocks, are judged against the V4 pin

- [ ] **Task 3.4:** Repair `AddServiceActivator` and watch for it (P0-10)
  - Input: design E6 and D5; `tools/symbolwatch.tsv`'s row format
  - Output: `AwsScheduler.md` and `AzureScheduler.md` calling `AddConsumers`; one row in
    `tools/symbolwatch.tsv`; `<!-- symbolcheck: allow AddServiceActivator -->` beside the first site
    on `FAQ.md` and `V10MigrationGuide.md`. `symbolcheck` → *"23 entries … 8 silenced"*, exit 0;
    `symbolcheck --verify-list` → the new row DEAD at both refs. The red run (7 sites, exit 1) is
    recorded, and `tools/README.md` row 8 updated

- [ ] **Task 3.5:** Run S1's behavioural claims (AC10)
  - Input: every block 3.2–3.4 changed that asserts behaviour
  - Output: § *Phase 3 as executed*, a *Claim / Case / Control* table. Each row names its
    `probe/s1/<name>/Program.cs` (obligation 18)

- [ ] **Task 3.6:** Second pass for S1 (obligation 21)
  - Input: the recurrences 3.1 listed; any ruling asked for in 3.2–3.5
  - Output: every recurrence repaired, or its ruling recorded; § *Defect ledger* rows' *After*
    column filled from their greps, run as written

- [ ] **Task 3.7:** Close phase 3
  - Input: § 1 obligations 6, 7, 19
  - Output: baseline rows for every new BUILT block; `--report` exit 0; `pagelint` 0 errors; the
    phase's readings (AC9, AC10) over its diff; gates reconciled against 3.1; the PR, with sign-off
    and head-ref deletion asked by name

### Phase 4 — S2, Commands, Handlers and Pipelines; Get Started *(6 tasks, one PR)*

- [ ] **Task 4.1:** Predict phase 4's gate movement
  - Input: § *The groups*, S2; recurrence greps for S2's defects
  - Output: § *Phase 4 as executed*, *Predicted*, as 3.1's
- [ ] **Task 4.2:** Repair S2's reachable blocks
  - Input: S2's table, reachable rows
  - Output: as 3.2's, for S2. `ShowMeTheCode.md` is a tutorial, so obligation 17 applies
- [ ] **Task 4.3:** Repair S2's hard blocks
  - Input: S2's table, hard rows; 10.7.0 source
  - Output: as 3.3's, for S2. Each handler block on an S2 page that 2.5 did not wrap or build, as
    named in § *Phase 2 as executed*, is repaired or listed
- [ ] **Task 4.4:** Run S2's behavioural claims (AC10)
  - Input: S2's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s2/<name>/Program.cs`
- [ ] **Task 4.5:** Second pass for S2
  - Input: 4.1's recurrences; rulings asked
  - Output: as 3.6's
- [ ] **Task 4.6:** Close phase 4
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 5 — S3, Darker; Understanding Brighter *(6 tasks, one PR)*

- [ ] **Task 5.1:** Predict phase 5's gate movement
  - Input: § *The groups*, S3; recurrence greps for S3's defects
  - Output: § *Phase 5 as executed*, *Predicted*, as 3.1's
- [ ] **Task 5.2:** Repair S3's reachable blocks
  - Input: S3's table, reachable rows
  - Output: as 3.2's, for S3
- [ ] **Task 5.3:** Repair S3's hard blocks
  - Input: S3's table, hard rows; Darker `4.1.1` and Brighter `10.7.0` source
  - Output: as 3.3's, for S3. `QueryPipeline.md`'s 12 *parse* blocks are each made whole, skipped
    with an accepted reason, or listed
- [ ] **Task 5.4:** Run S3's behavioural claims (AC10)
  - Input: S3's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s3/<name>/Program.cs`
- [ ] **Task 5.5:** Second pass for S3
  - Input: 5.1's recurrences; rulings asked
  - Output: as 3.6's
- [ ] **Task 5.6:** Close phase 5
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 6 — S4, Using an External Bus; Transports *(6 tasks, one PR)*

- [ ] **Task 6.1:** Predict phase 6's gate movement
  - Input: § *The groups*, S4; recurrence greps for S4's defects
  - Output: § *Phase 6 as executed*, *Predicted*, as 3.1's
- [ ] **Task 6.2:** Repair S4's reachable blocks
  - Input: S4's table, reachable rows; the V4 pin for `S3LuggageStore.md` #1
  - Output: as 3.2's, for S4. `S3LuggageStore.md` #1 is BUILT or listed with a diagnostic that is
    not `CS0234 … V4`
- [ ] **Task 6.3:** Repair S4's hard blocks
  - Input: S4's table, hard rows; 10.7.0 source
  - Output: as 3.3's, for S4. `KafkaConfiguration.md`'s 14 *parse* blocks are each made whole,
    skipped with an accepted reason, or listed
- [ ] **Task 6.4:** Run S4's behavioural claims (AC10)
  - Input: S4's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s4/<name>/Program.cs`
- [ ] **Task 6.5:** Second pass for S4
  - Input: 6.1's recurrences; rulings asked
  - Output: as 3.6's
- [ ] **Task 6.6:** Close phase 6
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 7 — S5, the remaining five sections *(7 tasks, one PR)*

- [ ] **Task 7.1:** Predict phase 7's gate movement
  - Input: § *The groups*, S5; recurrence greps for S5's defects
  - Output: § *Phase 7 as executed*, *Predicted*, as 3.1's
- [ ] **Task 7.2:** Repair S5's reachable blocks
  - Input: S5's table, reachable rows; the V4 pin for `DistributedLock.md` #2 and
    `DynamoDbDistributedLock.md` #1, #2
  - Output: as 3.2's, for S5. The three V4 blocks are BUILT or listed with a diagnostic that is not
    `CS0234 … V4` (AC11, with 3.3 and 6.2)
- [ ] **Task 7.3:** Repair S5's hard blocks
  - Input: S5's table, hard rows; 10.7.0 source
  - Output: as 3.3's, for S5
- [ ] **Task 7.4:** Repair `TestDoubleOptions.md`'s Shouldly blocks (P0-8)
  - Input: design § *Page Repair Rules*, the Shouldly row; 2.7's pin
  - Output: `using Shouldly;` in each of the six blocks, and one sentence naming the package.
    `grep -c Shouldly contents/TestDoubleOptions.md` → **≥ 7**, from **0**; the six BUILT or listed
- [ ] **Task 7.5:** Run S5's behavioural claims (AC10)
  - Input: S5's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s5/<name>/Program.cs`
- [ ] **Task 7.6:** Second pass for S5
  - Input: 7.1's recurrences; rulings asked
  - Output: as 3.6's
- [ ] **Task 7.7:** Close phase 7
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

---

## Phase 8 — Acceptance *(6 tasks, one PR, no page touched)*

- [ ] **Task 8.1:** Walk the criteria that have no instrument first: AC4, AC8, AC9, AC10, and the
  reading halves of AC5 and AC13
  - Input: `requirements.md` § *Acceptance criteria*, word for word; every phase's readings
    (obligation 19)
  - Output: § *Phase 8 as executed*, one row per criterion: who reads it, what they read, the
    walker's finding, the verdict

- [ ] **Task 8.2:** Walk the instrumented criteria: AC1–AC3, AC5–AC7, AC11–AC14
  - Input: each criterion's command, run at the phase's head
  - Output: one row per criterion: the command, its output, the verdict. AC12's BUILT is against
    design's **≥ 560**, and the nothing-BUILT figure against **≤ 14**

- [ ] **Task 8.3:** Check backwards: what changed that should not have
  - Input: `git diff --name-only 3a79b20 -- contents/`
  - Output: every changed page in the 64 + 3, or justified by a defect-ledger row (a recurrence) or
    by P0-10 (`FAQ.md`, `V10MigrationGuide.md`). Any other page is listed and explained

- [ ] **Task 8.4:** Complete the defect ledger
  - Input: § *Defect ledger*
  - Output: every row with its recurrence grep, *Before*, *After* **0**, and *Found by*: the
    instrument (`--classify`, rule 8, `symbolcheck`), a run, or re-derivation

- [ ] **Task 8.5:** Write the friction ledger
  - Input: every phase's *as executed* section
  - Output: § *Friction ledger*, numbered from **81**, each entry with the repair it proposes for
    the next spec

- [ ] **Task 8.6:** Close 018
  - Input: § *Phase 8 as executed*; `--report` and `--classify` at the head
  - Output: § *What 018 shipped*, ending in **one sentence naming the residual**, with its figure
    re-derived by `--classify`; the README checklist ticked; `PROMPT.md` updated; the PR, asking for
    the `.accepted` file and for deletion of its head ref by name

---

## The groups

*Drawn at task 2.14 from `--classify`, and not before (AC8). Design § *The section groups* holds
the working figures they replace.*

## Blocks that stay FAILED

*One row per FAILED block on a reached page: page, block, diagnostic, why it stays, phase (AC13).*

| Page | # | Diagnostic | Why it stays | Phase |
|---|---:|---|---|---:|

## Splits

*One row per split fence: page, old ordinal, new ordinals (017 obligation 16).*

| Page | Old # | New # | Phase |
|---|---:|---|---:|

## Defect ledger

*Each row written only after its grep has run as written (obligation 22). These three were found at
design, and their greps were run there.*

| # | Defect | Page(s) | Recurrence grep | Before | After | Found by | Phase |
|---:|---|---|---|---:|---:|---|---:|
| 1 | `AddServiceActivator` called as current code; dead at `10.7.0` and `master` (doc comments only) | `AwsScheduler.md`, `AzureScheduler.md` | `grep -rn 'Services.AddServiceActivator(' contents/` | **2** | | re-derivation (design E2); the watchlist missed it | 3 |
| 2 | Shouldly assertions with the package never named and no `using` | `TestDoubleOptions.md` | `grep -c Shouldly contents/TestDoubleOptions.md` | **0** | | the second compile (design E2, *pin-gap*) | 7 |
| 3 | Package spelt `Paramore.Brighter.Inbox.DynamoDb.V4`; the project is `…DynamoDB.V4` | `DynamoInbox.md` (a 017 tranche page; repaired under obligation 21) | `grep -rn 'Inbox.DynamoDb.V4' contents/` | **1** | | re-derivation (requirements, the V4 pin) | 7 |

## Friction ledger

*Numbered from 81, continuing 017's 67–80. Written at task 8.5.*

## What the tasks review found

*Written at `/spec:review`.*
