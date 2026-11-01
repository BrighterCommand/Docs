# Spec 018: Compile Residual — Tasks

**Status:** **APPROVED 2026-10-05** — `.tasks-approved`. Three review findings repaired (§ *What the tasks review found*).
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

- [x] **Task 1.1:** Add `PAIRED` and the `ATTRIBUTE KIND` check to `tools/pagelint.py`
  - Input: design § *Rule 8* (what it reads, what it reports, `PAIRED`, the message);
    `spec/017-compile_repairs/probe/attr_mismatch.py` (`scan()`, moved unchanged);
    `tools/pagelint.py` `:164` (`APPLIES_TO`), `:475` (`check_code_blocks`)
  - Output: `PAIRED` beside `APPLIES_TO` with its derivation comment, and a check reporting
    `ATTRIBUTE KIND` as an error repo-wide and under `--changed`. `python3 tools/pagelint.py` →
    exactly **1** error, on `PipelineValidation.md` block 7, recorded here as the rule's first red
    run
  - Notes: `PAIRED` is re-derived at both refs as you write it (§ 1 obligation 1); the design's 13
    are the expected answer

- [x] **Task 1.2:** Add the `attr-mismatch-intended` opt-out and its three faults
  - Input: design § *Rule 8*, *The opt-out*; `tools/blockcheck.py:280–300`, the binding it copies
  - Output: a marker binding to the next C# block below it. A marker with no reason, a marker with no
    C# block after it, and a second marker on one block are each an error. Honoured markers print
    with their reasons and a count line, *"N block(s) marked attr-mismatch-intended"*

- [x] **Task 1.3:** Add `pagelint --plant` and record its red-proof
  - Input: design § *Rule 8*, the five-plant table; 1.1 and 1.2
  - Output: `python3 tools/pagelint.py --plant; echo $?` → **0**, all five plants behaving. A
    red-proof run, with one plant's expectation inverted in a scratch copy, → **1**, recorded here
    with its output (AC1, AC3)

- [x] **Task 1.4:** Mark `PipelineValidation.md` block 7
  - Input: design § *Rule 8*, *The page*
  - Output: the marker line above the fence of block 7. `python3 tools/pagelint.py` → **0
    errors**, *"1 block(s) marked attr-mismatch-intended"*. Recorded here: the run with the marker
    removed reports `ATTRIBUTE KIND` on block 7 (AC2). `blockcheck --report` is unmoved, at 990 / 299
  - Notes: changes the published site. Its sign-off is asked for in 1.8's PR

- [x] **Task 1.5:** Run rule 8 in CI
  - Input: `.github/workflows/docs.yml`, the `check` job
  - Output: a bare `- run: python3 tools/pagelint.py --plant` step after the `pagelint` step, with
    a comment citing design § *Rule 8*

- [x] **Task 1.6:** Write rule 8 into `CLAUDE.md`
  - Input: design § *Rule 8*, *`CLAUDE.md`*; `CLAUDE.md` § *The ledger* and § *Complete code blocks*
  - Output: one ledger row, and § *Handler attributes match their handler* after *Complete code
    blocks*. Then `grep -rn 'pagelint' .claude/commands/` (**2** files at `77b7113`: `implement.md`,
    `review.md`), each hit read against the new rule and fixed where it quotes a rule set or count
    (the *Writing Review* rule)

- [x] **Task 1.7:** Update `tools/README.md` for rule 8
  - Input: `tools/README.md` row 2, § *What each gate actually checks*, § *The other modes*
  - Output: row 2 with the phase's ref (warnings unmoved); `pagelint`'s bullet naming rule 8; *The
    other modes* listing `pagelint.py --plant`. AC14's corrected-form count → **1** for any new figure

- [x] **Task 1.8:** Close phase 1
  - Input: § 1 obligations 6, 7, 19; the phase's prediction
  - Output: § *Phase 1 as executed*, with every gate's figure against the prediction, and the
    reading criteria (AC9/AC10: **no block's text changed**, so none applies, stated). Then the PR,
    asking for sign-off (one page changed) and for deletion of its head ref by name


### Phase 1 as executed — 2026-10-05

**Measured at phase start, `94ad0ed`.** `master` moved after tasks were approved: #195, a bugfix
outside the programme, changed six pages and added `bugfixes/0001-unmapped-type-guidance/`. Two
figures moved with it. The prediction above was made at `1b6f7f9` and stands as written:

| Figure | Said (`1b6f7f9`) | Measured (`94ad0ed`), method 1 | Method 2 | Cause |
|---|---:|---|---|---|
| `pagelint` warnings | 524 | summary line → **512 / 66 pages** | `grep -c 'USING DIRECTIVES'` → **512** | #195 repaired 12 debt blocks on its six pages |
| `linkcheck` files | 165 | **166** | a worktree at `019ef8e` → **165** | #195's `bugfix.md` is inside the walk |
| `blockcheck` | 990 / 299 / 674 / 17 | `--report` rows → **990 / 299 / 674 / 17** | `grep -vc '^#' baseline.tsv` → **299** | unmoved |
| `PAIRED` | 13 | the derivation at Brighter `10.7.0` → **13** | at Brighter `origin/master` → the same 13 | — |

`tools/README.md` rows 1 and 2 now carry `94ad0ed`'s figures with their refs.

**Rule 8's first red run** (1.1), the whole repo before the marker, exit 1:

```text
contents/PipelineValidation.md:250: ATTRIBUTE KIND: [RejectMessageOnError] is the sync attribute, on HandleAsync. Use [RejectMessageOnErrorAsync], or mark the block <!-- pagelint: attr-mismatch-intended <reason> --> if the mismatch is the point

1 errors, 512 warnings (using-directive debt: 512 blocks across 66 pages) across 162 pages.
```

`--changed origin/master` reports the same error, exit 1, so the rule is an error at both levels.

**The marker's three faults** (1.2), each planted on `PipelineValidation.md` and reverted. Each is
an error, and the first and third leave the block checked:

```text
contents/PipelineValidation.md:248: ATTRIBUTE KIND MARKER: the block at line 249 already has a marker, at line 247. The block below is still checked
contents/PipelineValidation.md:420: ATTRIBUTE KIND MARKER: no C# block follows it, so it marks nothing
contents/PipelineValidation.md:247: ATTRIBUTE KIND MARKER: no reason given; write <!-- pagelint: attr-mismatch-intended <reason> -->. The block below is still checked
```

**`--plant`'s red-proof** (1.3, AC1, AC3): a scratch copy with the matched-pair plant expecting 1
hit rather than 0, exit **1**. The real one exits **0**, 5 of 5. `--plant x` exits 2.

```text
FAILED matched pair, sync on Handle: ATTRIBUTE KIND x0, expected x1
4 of 5 plants behave.
```

**The page** (1.4, AC2): with the marker, `pagelint` reports **0 errors** and *"1 block(s) marked
attr-mismatch-intended"*. Without it, the red run above. `blockcheck --report` gives
`PipelineValidation.md`'s 16 verdicts identically with and without the marker, and the corpus stays
at 990 / 299.

**Gates against the prediction** (*every gate unmoved; `pagelint` 0 errors at both ends*):

| Gate | Predicted | Measured | Why any difference |
|---|---|---|---|
| `linkcheck` | unmoved | 166, 0 broken | 165 → 166 is #195, before the phase |
| `pagelint` | 0 errors, 524 warnings | **0 errors, 512 warnings, 162 pages**; 1 marked | 524 → 512 is #195; the phase moved none |
| `pagelint --plant` | exit 0 | exit **0**, 5 of 5 | — |
| `symbolcheck` | unmoved | 22 entries, 161 pages, 3 silenced, 0 found | — |
| `blockcheck` | 990 / 299 | **990 / 299 / 674 / 17** | — |

**Reading criteria** (obligation 19): the marker is an HTML comment above a fence, so **no block's
text changed**. Neither AC9 nor AC10 applies.

**Commands that cite what changed** (1.6): `grep -rn 'pagelint' .claude/commands/` → `implement.md`
(`:69` rule 6, `:80` the gate commands) and `review.md` (`:2`, `:12`, `:25`, `:81`). None quotes a
rule count that rule 8 moves. The ledger's three review-only conventions are still three, and
`review.md`'s error filter keeps the new marker lines.

---

## Phase 2 — Instruments *(15 tasks, one PR, no page changes)*

Design § *The Second Compile*, § *The Handler Wrapper*, § *The V4 Pin*. **Predicted:** `blockcheck`
BUILT rises by the blocks the handler wrapper, Shouldly and the `Program` fix make build, a set 2.1
measures before any row; FAILED falls by the same; the corpus is unmoved, at 990. The main pin's
assemblies rise by Shouldly's, and there is a new scope line for the V4 pin. Every other gate is
unmoved. Tasks 2.8–2.11 are **P1**: one that does not fit is carried to the residual with its
reason, and does not hold the phase.

- [x] **Task 2.1:** Measure what phase 2's staging changes will build, before any row
  - Input: design § *Gate Movement Predicted*, phase 2's BUILT row; E5's 32 blocks; E2's 6 Shouldly
    blocks; `--list` for blocks naming `Program`
  - Output: § *Phase 2 as executed*, *Predicted BUILT*: the block list each of 2.5, 2.7 and 2.8 is
    expected to make BUILT, with how it was found. This is the set 2.13 reconciles against

- [x] **Task 2.2:** Give `--classify` its second compile, receiver-aware
  - Input: design § *The Second Compile* (classes, five columns, resolution); `tools/blockcheck.py`
    `:254`, `:1198–1204`; `spec/017-compile_repairs/probe/usings.py` (the `using` supply it
    promotes)
  - Output: `--classify` printing `page·ordinal·class·first·names` with the six classes, its
    per-class counts on stderr, exit 0, and exit 2 on nothing to classify. A name is resolved only
    when the page does not declare it and the staged base does not supply it

- [x] **Task 2.3:** Red-proof the second compile (AC6)
  - Input: design § *The Second Compile*, *Red-proof*
  - Output: § *Phase 2 as executed*, a table (block, expected class, actual class) with exactly one
    row per class named, plus one control per check that decides a name (D8): page-declared and
    page evidence. A run from a scratch copy with each check removed moves its control into the
    wrong class, recorded. The scratch copies are not committed

- [x] **Task 2.4:** Reconcile `--classify` with the 018 probe (AC7)
  - Input: `probe/run.sh`'s `verdicts.tsv` at the same ref; 2.2's output
  - Output: `spec/018-compile_residual/probe/reconcile.py` (obligation 18), and § *Phase 2 as
    executed*, a disagreement table: every block where the two differ, each with its reason. E4's
    39 and 29 are expected, in the receiver-aware direction

- [x] **Task 2.5:** Add the handler wrapper (P0-7)
  - Input: design § *The Handler Wrapper*; `tools/blockcheck.py:263`, `:355`
  - Output: members blocks overriding the four methods, and (D9) `Fallback` / `FallbackAsync`, are
    staged with their fully qualified base. § *Phase 2 as executed* records how many are wrapped, and
    names each one that is not. Red-proof: one plant per row of the wrapper's table, each staging and
    building. Control: `--stage` output byte-identical to `1b6f7f9` for every block that is not
    wrapped (`cmp`, silent)

- [x] **Task 2.6:** Add the V4 pin (P0-5)
  - Input: design § *The V4 Pin*; `spec/018-compile_residual/probe/v4pin.sh`;
    `tools/blockcheck/scaffold/pages.tsv`
  - Output: `tools/blockcheck/refs-v4/refs-v4.csproj` with its comment; a `v4` cell on the four
    pages; `--report` enforcing the pairing rule and printing the V4 assemblies line. Red-proof,
    two-way: a V4 page unmarked, and a marked page with no V4 block, are each a finding (exit 1);
    the corrected map → exit 0. AC11's `grep -c 'CS0234.*V4'` over `--explain` of the six → **0**

- [x] **Task 2.7:** Pin Shouldly (P0-8)
  - Input: design § *Page Repair Rules*, the Shouldly row; `tools/blockcheck/refs/refs.csproj`
  - Output: `Shouldly` `4.3.0` in `refs.csproj`, with a comment citing Brighter's
    `Directory.Packages.props:150`. The restore is clean and the assembly count is recorded. No page
    is touched; `TestDoubleOptions.md`'s repair is 7.4's

- [x] **Task 2.8:** *(P1)* Let a `statements` block name `Program` (friction #75)
  - Input: friction #75 (`spec/017-compile_repairs/tasks.md` § *Friction ledger*); `WRAPPERS`
  - Output: such a block staged as a top-level file. Red-proof: one block naming `Program` builds;
    control: a `statements` block that does not name it stages exactly as before (`cmp`)

- [x] **Task 2.9:** *(P1)* Make a scaffold unit that does not compile a finding (friction #77)
  - Input: friction #77; the unit rule in `--report`
  - Output: a compile error in a scaffold unit printed as a finding, exit 1. Red-proof: a planted
    broken unit → exit 1; the real units → **0** such findings at `1b6f7f9`, recorded

- [x] **Task 2.10:** *(P1)* Give `pagelint` a per-page warning count (friction #80)
  - Input: friction #80; `tools/pagelint.py` `main`
  - Output: a flag printing `page<TAB>warnings`, sorted, whose column sums to the summary line's
    **512**: two methods that must agree

- [x] **Task 2.11:** *(P1)* Measure the `net9.0` pin against `net10.0` (friction #76)
  - Input: design D7; `refs.csproj`'s `TargetFramework`
  - Output: § *Phase 2 as executed*, *net10.0*: a scratch `net10.0` build of the pin, the packages
    that resolve to a different version, and `--report`'s verdicts under it against the `net9.0`
    ones. If any verdict moves, it is put to the maintainer before 2.14

- [x] **Task 2.12:** Update CI and `tools/README.md` for phase 2's instruments
  - Input: `.github/workflows/docs.yml` `blocks` job; `tools/README.md` row 9, § *Reading a number*,
    § *The other modes*, the `--classify` bullet
  - Output: a `dotnet build tools/blockcheck/refs-v4/refs-v4.csproj -c Release` step before the
    gate. Row 9 at the phase's ref. The fourth scope line explained. `--classify`'s description
    rewritten for five columns and six classes. AC14's count → **1** for each new figure

- [x] **Task 2.13:** Baseline what phase 2 made build
  - Input: 2.1's predicted set; `--report` after 2.5–2.8
  - Output: rows in `tools/blockcheck/baseline.tsv` for exactly the blocks that now build;
    `--report` exit 0. § *Phase 2 as executed* reconciles them against 2.1, block by block. Each
    new BUILT block is clean under rule 8 (`pagelint` 0 errors)

- [x] **Task 2.14:** Draw the S1–S5 page tables from `--classify` (AC8)
  - Input: `--classify` at phase 2's head; design § *The section groups*; `SUMMARY.md`
  - Output: § *The groups*, one table per group, every column derived from `--classify` by the
    command written above it, and no probe column. The three tranche pages with V4 blocks are in S4
    and S5. The tables' FAILED column sums to `--report`'s FAILED on the 64 + 3 pages

- [x] **Task 2.15:** Close phase 2
  - Input: § 1 obligations 6, 19; the phase's prediction
  - Output: § *Phase 2 as executed*, gates against the prediction; readings (no page touched,
    stated); the PR, asking for deletion of its head ref by name


### Phase 2 as executed

**Measured at phase start, `f6a9a7c`.** `--report` → **990 / 299 / 674 / 17**, exit 0, unmoved
since `1b6f7f9` (§ 2).

#### Predicted BUILT (2.1)

**How it was found.** `probe/predict.py` (obligation 18) prototypes each staging change **on its
candidates only**, in scratch, and compiles them with the gate's own Roslyn tool. Nothing in
`tools/` is changed. It writes one row per candidate: set, page, block, verdict, codes.

```bash
python3 spec/018-compile_residual/probe/predict.py <scratch>    # exit 0; 61 rows
```

| Set | Task | Candidates, method 1 (`predict.py`) | Method 2 | Predicted BUILT |
|---|---|---:|---|---:|
| **H**: `members` blocks overriding `Handle`, `HandleAsync`, `Execute` or `ExecuteAsync`, staged `Holder : <fully qualified base>` | 2.5 | **32** FAILED; **29** wrapped | design E5 → **32** | **0** |
| **S**: blocks calling `.Should…`, against a scratch pin with `Shouldly` `4.3.0` (**548** assemblies, main pin 545) | 2.7 | **6** FAILED | design E2, *pin-gap* → **6** | **0** |
| **P**: `statements` blocks naming `Program`, staged unwrapped as a top-level file | 2.8 | **17** FAILED | every block naming `Program`, by verdict and shape → **17** FAILED `statements`, and **0** BUILT `statements` | **4** |

**The set 2.13 reconciles against is four blocks, all from 2.8:**

| Page | # | Today | Under 2.8 |
|---|---:|---|---|
| `DarkerConfigurationReference.md` | 1 | FAILED | BUILT |
| `DarkerConfigurationReference.md` | 2 | FAILED | BUILT |
| `QueryPipelinePolicies.md` | 1 | FAILED | BUILT |
| `TickerQScheduler.md` | 2 | FAILED | BUILT |

**2.8 cannot lose a baseline row.** No BUILT `statements` block names `Program`; the BUILT blocks
that do are `types` or `toplevel` (`MSSQLOutbox.md` #3, `MySQLOutbox.md` #3, `PostgresOutbox.md` #3,
`SqliteOutbox.md` #3, `QueryPipelinePolicies.md` #2), and 2.8 does not touch those shapes.

**Why H and S move nothing.** The wrapper does what it is for: `Context` and `base.HandleAsync`
stop being errors, and an abstract member left unimplemented now shows as `CS0534` against the base
(`ImplementAQueryHandler.md` #5). What remains is the page's own: names it never declares
(`MyCommand`, `TaskReminderCommand`, `_mailGateway`) and the `using` directives it omits
(`Task<>`, `CancellationToken`). Each of the 29 still has a `CS0246` or `CS0103` of that kind. The
six Shouldly blocks fail on `SpyCommandProcessor`, `CommandType` and the page's message types,
**even with `using Shouldly;` added**, the repair 7.4 was to make. So D2 and D4 are needed by the
phases that repair those pages, and on their own move no verdict.

**The 29 the wrapper reads**, all still FAILED and each repaired or listed by its page's phase
(4.3 and kin): `BrighterInboxSupport.md` #1; `BrighterOutboxSupport.md` #1; `CloudEventsSupport.md`
#5; `CQRSWithBrighterAndDarker.md` #11, #12, #13; `DispatchingARequest.md` #8; `EFCoreOutbox.md` #2;
`FAQ.md` #8; `HowConfiguringTheCommandProcessorWorks.md` #5, #6, #8, #9; `ImplementAQueryHandler.md`
#4, #5, #8; `PolicyFallback.md` #3, #4, #5; `QueryPatterns.md` #3; `QueryPipeline.md` #8, #13, #22,
#23; `ShowMeTheCode.md` #2, #3, #4, #5; `TurningOnReplayOnSeen.md` #4.

**The three blocks the wrapper cannot read** are `EFCoreQueryIntegration.md` #1–#3. Each is written
`ExecuteAsync(...)`, so there is no parameter to read the query type from.

**Said and measured:**

| Said | Measured | Where the text now stands |
|---|---|---|
| Design § *Gate Movement Predicted*: BUILT rises *"up to 32 + 6"* by D2, #75 and D4 | **+4**, all #75; D2 **0** of 29, D4 **0** of 6 | the row now carries **+4** |
| Design § *The Handler Wrapper*: a one-line regex reads 27, and the other 5 *"break the signature across lines"* | joining lines reads 27. Of the other 5, 2 (`BrighterInboxSupport.md` #1, `ShowMeTheCode.md` #5) name their parameter `@event` and read once `@` is admitted, so **29**. 3 have no parameter at all | the paragraph now says 29, and why the 3 stay unwrapped |
| 017 friction #75: `QueryHandlerDependencies.md` #3 builds *"as a real `Program.cs` … at 0 errors"* | **`CS0103`, `builder`**. The block calls `builder.Services` and never declares `builder` | a page defect, read in its phase; not an instrument gap |


#### The second compile (2.2)

`--classify` now prints `page·ordinal·class·first·names`. `first` is the first compile's class, and
its column is **byte-identical** to the old four-column output's class on all 674 rows (`cmp` of
`cut -f1-3` from the old tool against `cut -f1,2,4` from the new, silent). `names` is what decided
`class`: the namespaces supplied for `built-by-using`, the diagnostic codes for `defect` (and
`wrapper:` names), the missing names otherwise. The `using`s are supplied in up to four rounds, as
017's probe did.

**Receiver-aware**, design D1 and D8. A name is not resolved when any block on the page declares
it. Nor is it resolved when it is a member of a handler base the staged `Holder` lacks. Those
members were read from source at Brighter `10.7.0` and Darker `4.1.1`, and the command is written
beside `HANDLER_BASE_MEMBERS`. A block left holding one is `defect` with `wrapper:`, never
`built-by-using`. First-party namespaces are supplied before third-party ones. A third-party
namespace is supplied only when a block on the page imports from its root (D8, ruled at 2.3). The
compile settles in **3** rounds; not settling within `USING_ROUNDS` (8) is exit 2.

| Figure | Method 1 | Method 2 | Agrees? |
|---|---|---|---|
| Second compile, at `f6a9a7c`, after 2.4's `CS0308` repair | stderr → **141 parse, 32 built-by-using, 119 defect, 28 same-page, 67 values, 287 page-type** | `cut -f3 \| sort \| uniq -c` → the same six | **yes** |
| First compile | stderr → **141 / 315 / 21 / 19 / 35 / 143** | the old tool's summary line → the same | **yes** |
| Rows | **674** | `--report`'s FAILED → **674** | **yes** |

**Checked:** two runs from fresh staging `cmp` silent. Exit **2** on a page with nothing FAILED
(`AggregationQueryPatterns.md`) and on a page with no blocks. `--report` is unmoved: 990 / 299 /
674 / 17, 0 findings. AC6's red-proof is 2.3.

**Said and measured.** The old tool counted a pinned type in the global namespace as `import`
evidence. The first draft of the second compile dropped such types from the shared table, and
`ImplementAQueryHandler.md` #9's `first` moved `import` → `page-type`. The first compile now reads
every name the dump lists, as before. Only the second compile needs a namespace, because it writes
the `using`.


#### Red-proof (2.3, AC6)

**Run 1**, the tool as committed, over the five pages, exit 0:

```bash
python3 tools/blockcheck.py --classify contents/Routing.md contents/AWSSQSConfiguration.md \
    contents/TestDoubleOptions.md contents/BuildingAPipeline.md contents/NullableReferenceTypes.md
```

| Block | Expected | Actual | Why it is known |
|---|---|---|---|
| `Routing.md` #2 | `built-by-using` | **`built-by-using`**, `Paramore.Brighter` | one of E1's 34 |
| `AWSSQSConfiguration.md` #7 | `defect` | **`defect`**, `CS1739` | behind `Paramore.Brighter` and `…MessagingGateway.AWSSQS`: *"The best overload for 'SqsSubscription' does not have a parameter named 'name'"*, an *api* block |
| `TestDoubleOptions.md` #11 | `page-type` | **`page-type`**, `Mock`, `MyCommand`, `MyHandler`, `Times` | `MyCommand` and `MyHandler` are never shown; Moq is not pinned |
| `TestDoubleOptions.md` #2 | `parse` | **`parse`** | does not parse as staged |
| **Control, page-declared:** `BuildingAPipeline.md` #3 | not `built-by-using` | **`same-page`**, `RequestLoggingHandler` | the page declares `RequestLoggingHandler<TRequest>`, and `Paramore.Brighter.Logging.Handlers` ships one |
| **Control, page evidence:** `NullableReferenceTypes.md` #4 | not given `StackExchange.Redis` | **`page-type`**, `CreateOrderCommand`, `IOrderRepository`, `Order` | the page never declares `Order` and imports nothing from `StackExchange` |

**Run 2**, a scratch copy with the page-declared check removed
(`if name in declared or …` → `if name in artefacts or …`). Of the 37 rows on the five pages, only
the first control moves:
`BuildingAPipeline.md` #3 → **`built-by-using`**, `Paramore.Brighter.Logging.Handlers`.

**Run 3**, a scratch copy with page evidence removed (`new = first_party or new`). Of the 37 rows,
only the second control changes class: `NullableReferenceTypes.md` #4 → **`defect`**, `CS0117`, given
`StackExchange.Redis` for `Order`. `TestDoubleOptions.md` #11 keeps its class and drops `Mock` and
`Times` from its names, because Moq was supplied.

Each check therefore decides its control, and the four class rows hold with both checks in place.

**What D1 alone let through**, found by this task's first draft. The design's control was an
`Order` block that page-declares `Order`. Searching for one showed that D1's page-declared check
cannot reach most `Order` blocks: the pages that use `Order` mostly never declare it. A scratch run
that printed every supplied namespace, over all 674 rows:

| Name → namespace | Rows, D1 as designed | Pages |
|---|---:|---|
| `Order` → `StackExchange.Redis` | **20** | `AwsScheduler.md`, `AzureScheduler.md`, `DispatchingARequest.md`, `HangfireScheduler.md`, `ImplementAQueryHandler.md`, `InMemoryScheduler.md`, `NullableReferenceTypes.md`, `QuartzScheduler.md`, `QueriesAndQueryObjects.md` |
| `Context` → `Polly` | **11** | `TurningOnReplayOnSeen.md`, `UsingTheContextBag.md`: a `types` block whose `RequestHandler` base was imported in the same round |
| `Tag`, `User`, `Id`, `ICache`, `ILog`, `LogProvider`, `Build`, `Topic`, `Seconds`, `Log`, `Priority` | 1–2 each | MongoDB, AWS IAM, `Json.Schema`, Hangfire, Quartz, OpenTelemetry, GCP, Humanizer, NLog, Spanner |

So 2.2's record had said *"0 rows supply `StackExchange.Redis`"*, and that was measured on the
`names` column. That column carries supplied namespaces only for `built-by-using` rows. The
supplied set is what decides, and it held 20 such rows. **D8**, ruled by the maintainer 2026-10-05,
added tiering and page evidence:

| Third-party supplied | D1 as designed | + tiered | + page evidence (D8, committed) |
|---|---:|---:|---:|
| `StackExchange.Redis` | 20 | 20 | **0** |
| `Polly` | 17 | 10 | **6**, all on pages that import Polly |
| `built-by-using` | 32 | 32 | **32** |
| class changes against D1 | — | 0 | **20**: 16 `defect` → `page-type`, 2 `same-page` → `page-type`, 1 `values` → `page-type`, 1 `values` → `defect` |

All **21** rows that still receive a third-party namespace are on pages importing from its root:
AWS, Hangfire, Kafka, Polly and Xunit pages. **The cost runs the other way**, and it is known:
`AwsScheduler.md` #2, #3, #21 and #22 need `Amazon.RegionEndpoint`, and the page imports nothing
from `Amazon`, so they read `page-type`. The page is one of the V4 pages (2.6).

**The page-declared check has a cost too.** `QueriesAndQueryObjects.md` declares
`interface IQuery<out TResult>` to show Darker's, and `BuildingAPipeline.md` declares
`RequestLoggingHandler<TRequest>` to show Brighter's. So `QueriesAndQueryObjects.md` #22, #23 and
`BuildingAPipeline.md` #3 read `same-page` where `using Paramore.Darker;` or
`using Paramore.Brighter.Logging.Handlers;` would build them. They are read at their phases.


#### Reconciled with the probe (2.4, AC7)

`probe/reconcile.py` (obligation 18) joins `--classify` to `probe/run.sh`'s `verdicts.tsv`, both
written at `f6a9a7c` in one run, block by block. `same-page`, `values` and `page-type` fold onto
the probe's STUB. It derives each disagreement's reason from the data, and exits 1 when a row has
none.

```bash
bash spec/018-compile_residual/probe/run.sh <W>                    # exit 0
python3 spec/018-compile_residual/probe/reconcile.py <W>            # exit 0: 14 disagreements, 0 unexplained
```

| Probe | `--classify` | Blocks |
|---|---|---:|
| PARSE | parse | 141 |
| BUILT | built-by-using | 32 |
| BUILT | STUB | **3** |
| DEFECT | defect | 118 |
| DEFECT | STUB | **10** |
| STUB | defect | **1** |
| STUB | STUB | 369 |

674 rows on each side, the same blocks (the script refuses otherwise). **14 disagree:**

| Page | # | Probe | `--classify` | Reason |
|---|---:|---|---|---|
| `AwsScheduler.md` | 4 | DEFECT | page-type | page evidence: `MongoDB.Driver` for `Tag` |
| `AwsScheduler.md` | 10 | DEFECT | page-type | page evidence: `Amazon.IdentityManagement.Model` for `User`, and `Json.Schema` |
| `AwsScheduler.md` | 15 | DEFECT | page-type | page evidence: `MongoDB.Driver` for `Tag` |
| `BuildingAPipeline.md` | 3 | BUILT | same-page | page-declared: `RequestLoggingHandler` |
| `DispatchingARequest.md` | 1 | DEFECT | page-type | `CS0308` on `GetService<T>`, read as a missing `using` |
| `ImplementAQueryHandler.md` | 7 | DEFECT | page-type | page evidence: `Amazon.Runtime.Internal.Util` for `ICache` |
| `MessageMappers.md` | 6 | DEFECT | page-type | page evidence: `Google.Cloud.PubSub.V1` for `Topic` |
| `MigratingToPollyV8.md` | 8 | STUB | defect | page evidence: `Humanizer` for `1.Seconds()`, which the page never names. `CS1061` is the right reading |
| `QueriesAndQueryObjects.md` | 22 | BUILT | same-page | page-declared: `IQuery` |
| `QueriesAndQueryObjects.md` | 23 | BUILT | same-page | page-declared: `IQuery` |
| `RabbitMQConnectionStability.md` | 3 | DEFECT | page-type | page evidence: `NLog.Fluent` for `Log` |
| `UsingTheContextBag.md` | 13 | DEFECT | page-type | page evidence: `Polly` for `Context` |
| `UsingTheContextBag.md` | 14 | DEFECT | page-type | page evidence: `Polly` for `Context` |
| `UsingTheContextBag.md` | 17 | DEFECT | page-type | page evidence: `Polly` for `Context` |

All but `MigratingToPollyV8.md` #8 run in the receiver-aware direction. That one runs the other way
for the same reason: the probe made a call compile with a package the page never names.

**Two readings for the repair phases.** The three `same-page` rows are 2.3's known cost of the
page-declared check: each builds with a `using`. `UsingTheContextBag.md` #13, #14 and #17 are
`statements` blocks: a handler's lines shown outside any class, naming `RequestHandler.Context`. Read
`page-type` on `Context`, but their repair is not a type stub.

**Red-proof**, two-way: a copy of the run's inputs with `Routing.md` #2 planted as `page-type` →
**15 disagreements, 1 unexplained** (the plant), exit **1**. The real inputs → exit **0**.

**Said and measured.** Design § *The Second Compile* expected *"E4's 39 and 29"* disagreements;
measured **14**. The 39 are first-compile *import* rows, not verdicts. The probe's `run.sh` already
drops `Order` from its type table by name, so its 22 `Order` rows cannot disagree. E4's 29
second-compile DEFECTs land as follows:

| E4 tag (first, by `hardcat.py`'s order) | `defect` | `page-type` | Why the `defect`s stay |
|---|---:|---:|---|
| `wrapper` (22) | **18** | 4 | 16 are `members` handler blocks failing `CS0117` (`'object'` has no `HandleAsync`): the base class is missing, which 2.5 supplies, and receiver-aware resolution cannot. `AsyncDispatchARequest.md` #4 is a `statements` block calling `base.HandleAsync`. `DispatchingARequest.md` #1 is the `CS0308` row above, now `page-type` |
| `collision?` (7) | **3** | 4 | `MigratingToPollyV8.md` #11, #12: the page's own `Task` (never declared) meets `System.Threading.Tasks.Task`, and `[Validation]` is no Brighter attribute at 10.7.0. `UsingTheContextBag.md` #8: `Activity.SetAttribute` and `AddEvent(string)` do not exist on `System.Diagnostics.Activity`. Each is read at its phase |

**Defect in 2.2's instrument, repaired here** (obligation 21). A generic extension method whose type
has a non-generic instance overload of the same name is reported as `CS0308`, not `CS1061`. One such
call is `serviceProvider.GetService<T>()`, where the overload is `IServiceProvider.GetService(Type)`.
The second compile read that as a defect. `NON_GENERIC_RE` now reads it as the missing `using` it
is. At the first compile there are **0** `CS0308`s (`grep -c CS0308` over the probe's
`explain.txt`), so `first` cannot move, and it did not (`cmp`). At the second compile, exactly one
row moves: `DispatchingARequest.md` #1, `defect` → `page-type` (`GreetingCommand`). Its `using`s plus
`Microsoft.Extensions.DependencyInjection`, compiled by hand, leave only `CS0246` on
`GreetingCommand`.


#### The handler wrapper (2.5)

`handler_base` reads the base from the joined signature, and `wrap` writes
`public abstract class Holder : <base>`, fully qualified (design § *The Handler Wrapper*).

**D9, ruled by the maintainer 2026-10-09.** Five `members` blocks show a handler by its `Fallback`
alone: `PolicyFallback.md` #6, #7, #9 (`MyCommand Fallback(MyCommand)`, Brighter's) and
`QueryPipeline.md` #24, #25 (`Task<string> FallbackAsync(Query)`, Darker's). Each was `defect` with
`wrapper:Context`. The wrapper now reads `Fallback` and `FallbackAsync` too, and the return type
picks the product.

**The Holder is abstract**, found by the plants. A Darker `Fallback` shown alone fails `CS0534` in a
concrete `Holder`, for want of the `Execute` its base declares abstract. Over the corpus the change
moves **0** verdicts and **0** classes: `--classify` before and after is identical (`cmp`), and
`--report` differs only in error counts. At the first compile **9** wrapped blocks each lose one
`CS0534`: `CQRSWithBrighterAndDarker.md` #13, `ImplementAQueryHandler.md` #4, #5,
`QueryPatterns.md` #3, `QueryPipeline.md` #13, #22–#25 and `ShowMeTheCode.md` #3. Each was a
cascade. When the block's own `CancellationToken` or query type is unresolved, Roslyn reports the
abstract member unimplemented and does not report `CS0115`. At the second compile, with the
`using`s supplied, none of the 9 carries `CS0534` either way.

**Wrapped: 34**, every one FAILED today, so no baseline row moves. They are 2.1's 29 and D9's 5.
`handler_base` → 34 against 2.1's `predict.py` 29 + the D9 search's 5: **agrees**. **Not wrapped,
with the reason it gives:**

| Page | # | Verdict | Reason |
|---|---:|---|---|
| `EFCoreQueryIntegration.md` | 1 | FAILED | `ExecuteAsync(...)` names no request type to read |
| `EFCoreQueryIntegration.md` | 2 | FAILED | the same |
| `EFCoreQueryIntegration.md` | 3 | FAILED | the same |
| `DarkerAndBrighterPipelines.md` | 1 | SKIPPED | its overrides imply two bases: `RequestHandlerAsync<AddGreetingCommand>` and `QueryHandlerAsync<GetPersonNameQuery, string>` |

**Red-proof**, `probe/handler_plants.py` (obligation 18). Each plant is a `Block` staged by `wrap`
itself, and compiled by the gate's Roslyn tool against a scaffold unit that declares `PlantCommand`
and `PlantQuery`. Exit 0, **11 of 11**:

| Plant | Wrapped | Base-less |
|---|---|---|
| `Handle` | BUILT | FAILED `CS0103`, `CS0115`, `CS0117` |
| `HandleAsync` | BUILT | FAILED `CS0103`, `CS0115`, `CS0117` |
| `Execute` | BUILT | FAILED `CS0115` |
| `ExecuteAsync` | BUILT | FAILED `CS0115` |
| `Fallback`, Brighter | BUILT | FAILED `CS0115`, `CS0117` |
| `Fallback`, Darker | BUILT | FAILED `CS0115` |
| `FallbackAsync`, Brighter | BUILT | FAILED `CS0115`, `CS0117` |
| `FallbackAsync`, Darker | BUILT | FAILED `CS0115` |
| `ExecuteAsync(PlantQuery)`, no `CancellationToken` | FAILED `CS0115`, as it must: abstract hides no wrong override | — |
| `ExecuteAsync(...)` | unwrapped: *names no request type to read* | — |
| `Handle` and `Execute` in one block | unwrapped: *its overrides imply two bases* | — |

A scratch copy of `blockcheck.py` with the wrapper's base disabled runs the same probe to exit
**1**: every plant fails wrapped. The copy is not committed.

**Control.** `--stage` from `HEAD`'s `blockcheck.py` against the new one: `index.tsv` identical, and
**956 of 956** unwrapped blocks byte-identical, the 92 non-handler `members` blocks among them.
**34 of 34** wrapped blocks differ in exactly one line, the `Holder`. `--verify-extraction` → 990 of
990 identical, exit 0.

**Gates against the prediction** (2.1: the wrapper makes **0** BUILT): `--report` → 990 / 299 / 674 /
17, **0 findings**. `--classify`, second compile, before → after the wrapper:

| Among the 34 | Blocks |
|---|---:|
| `defect` → `page-type` | **15** |
| `defect` → `defect` | 7 |
| `parse` → `parse` | 11 |
| `page-type` → `page-type` | 1 |
| `CS0117` (`'object'` has no `HandleAsync`) | 19 → **0** |
| `wrapper:` notes, corpus-wide | 6 → **0** |
| class changes outside the 34 | **0** |

Corpus-wide: **141 parse, 32 built-by-using, 104 defect, 28 same-page, 67 values, 302 page-type**. The
first compile moved with the staging, which it reads: five of the 34 changed `first` (`other` →
`page-type` 4, `import` → `other` 1).

**The 7 still `defect`** carry codes the wrapper cannot reach, each read at its phase:
`BrighterOutboxSupport.md` #1 (`CS1520`, a method with no return type), `CloudEventsSupport.md` #5
(`CS1061`), `FAQ.md` #8, `QueryPatterns.md` #3 and `TurningOnReplayOnSeen.md` #4 (`CS0111`, a
member declared twice: two snippets in one fence), `PolicyFallback.md` #5 (`CS0104`, `CS0161`), and
`QueryPipeline.md` #8 (`CS0161`).


#### The V4 pin (2.6)

`tools/blockcheck/refs-v4/refs-v4.csproj` is `refs.csproj` with the seven AWS packages swapped
for their `.V4` twins. Its comment cites `refs.csproj:191–192`, and says the two move in one edit.
It builds with **0** errors and **0** warnings to **545** reference assemblies, with
`awssdk.sqs/4.0.100.7` against the main pin's `3.7.500.5`. `pages.tsv` gains a fourth column,
`pin`. `S3LuggageStore.md` is marked `v4`, and the other three V4 pages get rows of their own
(`-`, `-`, `v4`).

**Four pages**, two methods: `grep -rlE '^\s*using [A-Za-z.]*\.V4\b' contents/*.md` → **4**, the
same four the design names. **30 blocks** compile against the V4 pin, and **3** of them build.

**Built on the way, each a decision the design left open:**

- **The stamp names its project.** `refs.txt`'s first line is now `# refs-v4.csproj SHA256 …`.
  `stale_pin` takes the project and list it checks, and reads the label from the project's file
  name
- **`AssemblyName` is `RefsV4`.** TickerQ's source generator writes the assembly name into code,
  and `refs-v4` failed `CS0246` on `v4`
- **`pin_drift`**: the two package lists may differ only by the seven swaps, or exit 2
- **The scaffold name carries the pin**, so the three BUILT blocks on V4 pages are re-admitted:
  `AwsScheduler.md` #19 (`-` → `v4`), `DistributedLock.md` #1 (`-` → `v4`), `S3LuggageStore.md` #2
  (`S3LuggageStoreContext.cs` → `S3LuggageStoreContext.cs+v4`). Each is written at `f6a9a7c`, the
  ref measured from, and 2.13 moves them with its own rows to the phase's commit
- **`--explain` and `--classify` are pin-aware**, as `--report` is. One run per pin; `--classify`
  keeps a type table per pin

**Red-proofs**, each two-way:

| Case | Result | Exit |
|---|---|---:|
| The four V4 pages unmarked (the map before the marks) | 4 × `PIN RULE: … imports a .V4 namespace and is not marked v4`. Verdicts unmoved | **1** |
| `AWSSQSConfiguration.md` marked `v4`, a page with no V4 import | `PIN RULE: … marked v4, and no C# block on it imports a .V4 namespace`, and #1, #3–#6 stop building (E3's reverse) | **1** |
| The corrected map, before the three rows were re-admitted | 3 × *scaffold changed since admission*, the rows above | **1** |
| The corrected map and rows | 0 findings; `pin rule: 4 pages on the v4 pin, 0 violations` | **0** |
| `pin_drift` on the committed files | `None` | — |
| …on a scratch copy with one V4 package at `10.6.0` | *expected, absent* / *present, unexpected* for that package | — |
| …on a scratch copy with one swap undone | the same, for `Transformers.AWS` | — |
| `refs-v4.csproj` edited without a rebuild | the stale stamp, *nothing was checked* | **2** |

The scratch copies are not committed, and the map was restored after each planted row (`cmp`).

**AC11:** `grep -c 'CS0234.*V4'` over `--explain` of the six blocks → **0**. What is left is E3's:
one `CS0103` each on `DistributedLock.md` #2, `DynamoDbDistributedLock.md` #1, #2 and
`S3LuggageStore.md` #1, and 6 and 14 missing names on `AwsScheduler.md` #2 and #3.

**Gates.** `--report` → **990 / 299 / 674 / 17, 0 findings**, exit 0. Its scope lines now include
`545 reference assemblies (v4 pin)` and `30 blocks, 3 built, 27 failing, 1 scaffold unit(s) (v4
pin)`. Verdicts are identical to the run before the pin, block by block (`cmp` of `cut -f1-3`). Only
6 rows changed codes, all on V4 pages. `--verify-extraction` → 990 of 990. `--classify` changes
**exactly six** rows, the six blocks above. Four become `values`, each needing one value stub as E3
found. `AwsScheduler.md` #2 becomes `page-type` on `RegionEndpoint`, D8's known cost. #3 stays `defect`
(`CS1061`, `CS1503`) without its `CS0234`. Corpus-wide: **141 parse, 32 built-by-using, 99 defect,
28 same-page, 71 values, 303 page-type**.

**CI is not yet green on this branch.** The `blocks` job does not build the V4 pin, so the gate
exits 2 there until 2.12 adds the step.

**Said and measured.** Design E3: *"546 reference assemblies"* for the V4 pin. Measured **545** by
the tool's own scope line and by `grep -vc '^#'`. `wc -l` gives 546 because it counts the stamp.
E3's construction rebuilt today gives the same 545-assembly set (`diff`, silent). The design now
says 545.

**`blockcheck.csproj` was left globbing `refs-v4/`.** It removed `refs/**` from its sources and not
`refs-v4/**`, so once `refs-v4.csproj` was built, `dotnet build tools/blockcheck/blockcheck.csproj`
failed with **7 × `CS0579`** (duplicate assembly attributes from `refs-v4/obj/…/AssemblyInfo.cs`).
2.6's runs used a `blockcheck.dll` built before the pin's first restore. `<Compile Remove="refs-v4/**" />`
was added at 2.9. 2.12's CI step builds `refs-v4` before `blockcheck`, so without it the job would fail there.


#### Shouldly in the pin (2.7)

`Shouldly` `4.3.0` is in `refs.csproj`, with a comment citing Brighter's
`Directory.Packages.props:150`, read at `10.7.0`: `<PackageVersion Include="Shouldly" Version="4.3.0" />`.
It is in `refs-v4.csproj` too, at the same place, **because 2.6's `pin_drift` requires it**: a
package in one pin and not the other is exit 2. It sits after line 191, so the V4 pin's citation of
`refs.csproj:191–192` still holds.

**Restore and build:** both pins, **0** errors and **0** warnings. Assemblies **545 → 548** in each
pin, by `--report`'s scope lines and by `grep -vc '^#'` of each `refs.txt`. The three are
`shouldly/4.3.0`, `diffengine/11.3.0` and `emptyfiles/4.4.0`. 2.1's scratch pin measured the same
548.

**No page is touched**, and nothing moves, as 2.1 predicted. `--report` → 990 / 299 / 674 / 17, 0
findings, with verdicts **and** error codes identical to 2.6's run (`diff`, silent). `--classify` is
identical (`diff`, silent). The six blocks stay `defect` on `CS1061`, because D8 supplies a
third-party namespace only on page evidence, and the page imports nothing from `Shouldly`
(`grep -c Shouldly contents/TestDoubleOptions.md` → **0**, defect ledger row 2). That is the right
reading: the page never tells its reader which package `ShouldBe` comes from.

**The pin is shown to work, two-way.** The six blocks were staged with the `using`s `--classify`
supplies them (`System`, `Paramore.Brighter`, `Paramore.Brighter.Testing`) and compiled against the
new pin, without and then with `using Shouldly;`:

| | `Should…` errors | Blocks with one | All errors |
|---|---:|---:|---:|
| without `using Shouldly;` | **10** | 6 | 32 |
| with it | **0** | 0 | 22 |

The 22 that remain are 7.4's to read. Most are page types (`PaymentProcessed`, `SendNotification`,
`CommandA`), and two are `.First()` without `System.Linq`. `TestDoubleOptions.md` #8 also carries
`CS8130` and `CS1503` on a deconstruction of `Id`s, which is likely an API mismatch, read at 7.4.


#### `Program` (2.8, friction #75)

A `statements` block that names `Program` in its code is staged with no wrapper, as the top-level
file it is. The Roslyn tool sees the global statements and compiles it as an executable, where C#
synthesises `Program`. `names_program` reads the block with comments and string literals removed,
so a `// Program.cs` header does not trigger it. Today all **17** such blocks name it as
`typeof(Program).Assembly`: 16 inside `AddHandlersFromAssemblies`, 1 inside `MigrationsAssembly`.
That is the same 17 2.1 counted, by its search and by `names_program`.

**Control.** The corpus was staged twice by the same tool, with `names_program` forced off and on.
**973 of 973** blocks that do not name `Program` stage byte-identically, **515** other
`statements` blocks among them. The 17 that do differ. `index.tsv` differs in exactly those 17
rows, their wrapper count now 0. `--verify-extraction` → 990 of 990.

**Red-proof, two-way.** A block naming `Program` builds: the four below, each FAILED `CS0246` on
`Program` with the change off and BUILT with it on. A block that does not name it stages exactly as
before (the control).

**Against 2.1's prediction, block by block:** the four predicted are the four that build.

| Page | # | Before | After |
|---|---:|---|---|
| `DarkerConfigurationReference.md` | 1 | FAILED `CS0246` | **BUILT** |
| `DarkerConfigurationReference.md` | 2 | FAILED `CS0246` | **BUILT** |
| `QueryPipelinePolicies.md` | 1 | FAILED `CS0246` | **BUILT** |
| `TickerQScheduler.md` | 2 | FAILED `CS0246` | **BUILT** |

`--report` → **990 / 303 / 670 / 17**, exit **1**, with exactly four findings: these four, under
*builds and is not baselined*. Their rows are 2.13's. The other 13 lose the `Program` `CS0246`. Twelve
keep only a `CS0103` (`builder` or `WebApplication`, which the page never declares).
`DarkerBasicConfiguration.md` #10 keeps `CS0246` for a type of its own. Nothing else moves.

**`--classify`, the 13 still FAILED:** 4 become **`built-by-using`**, each one
`using Microsoft.AspNetCore.Builder;` away from building: `DarkerBasicConfiguration.md` #1 and
`QueryPipeline.md` #2, #4, #6. 6 become `values` on `builder`: `DarkerBasicConfiguration.md` #6, #7,
`EFCoreQueryIntegration.md` #4, `QueryHandlerDependencies.md` #3 and `QueryPipeline.md` #18, #19.
`DarkerBasicConfiguration.md` #2 stays `page-type` (`GetPeopleQuery`, `GetPersonNameQuery`). #9 stays
`defect` (`CS1061`, `CS1929`) and #10 stays `defect` (`CS1501`); both are read at their phase.
Corpus-wide: **670 FAILED: 141 parse, 36 built-by-using, 99 defect, 28 same-page, 77 values, 289
page-type**.

#### A unit that does not compile (2.9, friction #77)

The Roslyn tool keeps an error reported against a scaffold unit's tree out of the block's verdict,
as before. It now keeps each one, keyed by unit, code, position and message, and prints it once at
the end of the run as a `scaffold error:` line, with how many blocks it rode with and the first.
That replaces the `WARNING: N error(s) reported against scaffold trees` line, which counted one
error once per block. `--report` takes those lines out of the scope lines, lists them under
*scaffold unit does not compile* by unit file, line and column, and counts each as a finding. The
summary line becomes `scaffold rule: N units checked, N violations, N compile errors`.

**Red-proof, two-way:** `probe/unit_plants.py` (obligation 18). It runs `--report` in process twice,
with `load_scaffold` patched so the units of `PageContext.cs` point at a scratch copy. The plant is
`using Plant.DoesNotExist;` prepended, a `CS0246` in the unit's own tree. The unit rule cannot see
it, because it reads declarations and `global using` and not plain `using`s. So the only check that
can turn the plant into a finding is the one under test.

| Run | Exit | Findings | Unit rule | Compile errors |
|---|---:|---:|---:|---:|
| control, the real units | 1 | 4 | 0 | **0** |
| plant, `PageContext.cs` | **1** | **5** | 0 | **1** |

The plant's finding: `tools/blockcheck/scaffold/units/PageContext.cs:1:7  CS0246 … -- in 2
block(s), first contents/DapperOutbox.md block 1`. The control's exit is already 1, from 2.8's four
unbaselined blocks (2.13's rows). So the probe asserts the plant adds exactly one finding, rather
than asserting an exit code the control already has.

**Said and measured.** The task said *the real units → 0 such findings at `1b6f7f9`*. The instrument
did not exist at that ref, so the figure is measured at 2.9's commit: **45 units** (44 main, 1 V4),
**0** compile errors. `--report` is otherwise unmoved: **990 / 303 / 670 / 17**, exit 1, the same
four findings.

#### `pagelint --per-page` (2.10, friction #80)

`--per-page` replaces the findings with one `page<TAB>warnings` row per page that has a warning,
sorted by path, on stdout. The summary line goes to stderr: `N errors, N warnings across N pages
with a warning, of N checked`. So two runs diff clean, and a phase's debt delta is a `diff` of two
outputs rather than a worktree at its base. It combines with paths and `--changed`. With `--fix` it
exits 2, because `--fix` prints its repairs first. Errors are not rows, but they set exit 1 as a
full run does. `tools/README.md` § *The other modes* lists it.

**Two methods that agree,** at 2.10's commit:

| Method | Rows | Sum |
|---|---:|---:|
| `pagelint.py --per-page`, `awk -F'\t' '{s+=$2} END{print s, NR}'` | **66** | **512** |
| `pagelint.py \| grep '(warning)' \| cut -d: -f1 \| LC_ALL=C sort \| uniq -c` | **66** | **512** |

The two tables are byte-identical (`diff`, silent), and both equal the full run's summary line,
*512 warnings (using-directive debt: 512 blocks across 66 pages)*. `LC_ALL=C` is needed: the
shell's default collation orders `AWSSQS…` after `AgreementDispatcher.md`, and Python's does not.

**Red-proof, two-way.** For a known-present and a known-absent page, `--per-page
contents/AgreementDispatcher.md contents/Glossary.md` gives one row, `AgreementDispatcher.md 12`,
and no row for `Glossary.md` (0 warnings), exit 0. For the error path, `print_per_page` was given
one error and one warning in process: one row (the warning), `1 errors`, exit **1**.

**Said and measured.** The task said the column sums to **524**. Measured **512**: #195 moved the
debt from 524 to 512 before phase 1 (§ *Phase 1 as executed*). The task now says 512.

#### net10.0 (2.11, friction #76, D7)

`probe/net10.py` (obligation 18) copies both pins into scratch with `net10.0`, restores them, and
compiles the same staged corpus against the committed `net9.0` reference lists and the scratch
`net10.0` ones. Nothing under `tools/` is written.

**The framework alone does not restore.** With only `<TargetFramework>` changed, the main pin fails
**NU1605**. `Paramore.Brighter.MsSql.EntityFrameworkCore` 10.7.0 needs `Microsoft.EntityFrameworkCore`
≥ **10.0.10** on `net10.0`, and the pin's 9.0.15 is then a downgrade. Three of the pin's explicit
packages are framework-tied by their own comments. The scratch copies raise them to what a `net10.0`
reader gets:

| Package | `net9.0` pin | `net10.0` | Source |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore` | 9.0.15 | **10.0.10** | the `net10.0` group of `Paramore.Brighter.MsSql.EntityFrameworkCore` 10.7.0's nuspec |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | **10.0.3** | its latest stable for EF Core 10 (nuget.org) |
| `TickerQ.Dashboard`, `TickerQ.EntityFrameworkCore` | 9.0.2 | **10.4.0** | the `net10.0` group of `Paramore.Brighter.MessageScheduler.TickerQ` 10.7.0's nuspec |

**Packages that resolve differently**, from each restore's `project.assets.json`, are the same on
both pins. On `net9.0` the pin resolves **317** packages, and on `net10.0` it resolves **291**.
**17** resolve at a different version:
- the EF Core family (10 packages, from 9.0.15 to 10.0.10)
- TickerQ (4 packages, to 10.4.0)
- `Npgsql` (10.0.2 → 10.0.3) and `FlexLabs.EntityFrameworkCore.Upsert` (9.0.0 → 10.0.0)
- `Microsoft.NETCore.Platforms` (5.0.0 → 1.1.0)

**27** resolve on `net9.0` only, all `System.*` / `Microsoft.*` packages that the `net10.0`
framework supplies in-box (317 − 27 + 1 = 291, which agrees). **1** resolves on `net10.0` only: `System.Reflection.TypeExtensions`
4.7.0. The restores warn `NU1608`, because `Pomelo.EntityFrameworkCore.MySql` 9.0.0 caps EF Core
Relational at 9.0.999. They also warn `NU1903`, on `SQLitePCLRaw.lib.e_sqlite3` 2.1.11. Reference
assemblies: **548 → 553** on each pin.

**Verdicts: none moved.** Of 973 judged blocks (17 SKIPPED excluded, as `--report` discards them),
every verdict and every FAILED block's code set is identical. Two methods agree: the block-by-block
comparison finds **0** moved, and BUILT is **300 / 300** on the main pin and **3 / 3** on the V4 pin,
which matches `--report`'s 303.

**Control.** The `net10.0` main list with TickerQ's assemblies removed moves **5** verdicts, all
on `TickerQScheduler.md` (#1, #2, #3, #7, #8). So the comparison can see a move, and its 0 is a
measurement. The probe exits 1 when the control does not move.

**D7's condition is not met.** No verdict moves, so nothing goes to the maintainer before 2.14,
and P2-2 (*compiles against the released packages, not in the pin* as a verdict) stays P2. The pin
stays `net9.0`.

#### CI and `tools/README.md` (2.12)

**CI.** The `blocks` job builds `refs-v4/refs-v4.csproj` between `refs.csproj` and
`blockcheck.csproj`. That order needs 2.9's `<Compile Remove="refs-v4/**" />`, without which the
third build fails `CS0579` (§ *The V4 pin (2.6)*).

**`tools/README.md`:**
- a paragraph for this phase above the table
- row 9 rewritten, with its `bd95ee0` figures kept as history
- the `blocks` job's build list grown to three, with the drift check's exit 2 stated
- § *Reading a number*: four scope lines, not three. The fourth is `pin rule: N pages on the v4
  pin, 0 violations`. The per-pin compiler lines and their `(v4 pin)` suffix are explained, and so
  are 2.9's `compile errors`
- the `--classify` bullet rewritten for five columns, the six classes, `first` and D8's resolution

`pagelint --per-page` was listed at 2.10.

**Row 9 is written as it reads at the phase's commit.** Its verdict figures are measured:
`--report` gives **990 / 303 / 670 / 17**, 548 assemblies on each pin, 30 blocks on the V4 pin,
45 units with 0 violations and 0 compile errors, and 4 pages on the V4 pin with 0 violations.
Its **`0 findings`** becomes true only when 2.13 writes the four rows. Today it reads **4**. So
2.13 re-derives row 9 before the phase is committed.

**AC14**, the corrected form (`grep -rn '<figure>' --include='*.md' --include='*.yml'
--include='*.py' . | grep -vcE '^(\./)?spec/'`), is **1** for each of: `303 BUILT`,
`670 FAILED`, `548 reference assemblies`, `30 blocks on the v4 pin`, `4 pages on the v4 pin`.
`0 compile errors` counts **2**: row 9, and § *Reading a number*'s line format
(`scaffold rule: N units checked, 0 violations, 0 compile errors`), which is a template and not a
copy of the figure, as `0 violations` beside it already was. The first draft repeated
`548 reference assemblies` in the phase paragraph, which counted 2. The paragraph now points at the
row instead.

#### Baselined (2.13)

`baseline.tsv` gains **4** rows and moves **3**, all to ref **`937db00`**. That is the commit
holding 2.6 and 2.8, the work that made each block build under its present scaffold name. Each row
sits beside its page's other rows, and a header comment says what the phase admitted.

| Page | # | Scaffold | Row |
|---|---:|---|---|
| `DarkerConfigurationReference.md` | 1 | `DarkerConfigurationReferenceContext.cs` | new (2.8) |
| `DarkerConfigurationReference.md` | 2 | `DarkerConfigurationReferenceContext.cs` | new (2.8) |
| `QueryPipelinePolicies.md` | 1 | `-` | new (2.8) |
| `TickerQScheduler.md` | 2 | `TickerQSchedulerContext.cs` | new (2.8) |
| `AwsScheduler.md` | 19 | `v4` | `f6a9a7c` → `937db00` (2.6) |
| `DistributedLock.md` | 1 | `v4` | `f6a9a7c` → `937db00` (2.6) |
| `S3LuggageStore.md` | 2 | `S3LuggageStoreContext.cs+v4` | `f6a9a7c` → `937db00` (2.6) |

**Against 2.1, block by block.** BUILT now, minus the rows at `f6a9a7c`
(`comm -13` of the two sorted `page<TAB>ordinal` lists), gives exactly the four 2.1 predicted for
2.8. The reverse, `comm -23`, is **empty**: no block stopped building. 2.1 predicted **0** for the
handler wrapper (2.5) and **0** for Shouldly (2.7), and 0 is what each made build.

**Two methods agree:** `grep -v '^#' baseline.tsv | grep -vc '^$'` → **303**, and `--report`'s
BUILT rows → **303**.

**Rule 8.** `pagelint` on the three pages gives **0 errors**. The four blocks are `statements`
blocks with no handler method and no handler attribute, so rule 8 has nothing to read in them. The
pages' one warning is `QueryPipelinePolicies.md`'s block at `:117`, a declared `// ...`, and it is
not one of the four.

**Gates.** `--report` → **990 / 303 / 670 / 17, 0 findings, exit 0**, with `baseline: 303 blocks
required to build`, `scaffold rule: 45 units checked, 0 violations, 0 compile errors`, `pin rule:
4 pages on the v4 pin, 0 violations`, and 548 reference assemblies on each pin. **`tools/README.md`
row 9, written at 2.12, re-derived:** every figure in it equals this run, including `0 findings`,
so it needs no edit.

#### Phase 2 closed (2.15)

**`master` moved during the phase.** #199 merged at `6a57698` on 2026-10-10 (outbox sweeper health
metrics). It changed `BrighterOutboxSupport.md` (4 lines, no block) and `Telemetry.md` (+142, three
new C# blocks). `origin/master` was merged into this branch with no conflict. Its three blocks are
`Telemetry.md` #6, #7 and #8, all FAILED and all `defect` on `CS0234`. `Telemetry.md` is a 017
tranche page, so they join that page's FAILED blocks outside § *The groups*: **36 → 39**. The
tables were re-drawn at the merge, and `diff` against 2.14's was silent.

**Gates against the prediction.** The prediction is § *Phase 2*'s header.

| Gate | Predicted | At `1b05c3e` (before the merge) | Merged with #199 |
|---|---|---|---|
| `blockcheck` corpus | unmoved, 990 | **990** | **993**, #199's three blocks |
| BUILT | + 2.1's set (4) | 299 → **303** | **303** |
| FAILED | − the same | 674 → **670** | **673**, #199's three |
| SKIPPED / findings | — | 17 / **0**, exit 0 | 17 / **0**, exit 0 |
| assemblies | main pin + Shouldly's | 545 → **548**, and **548** on the V4 pin | same |
| scope lines | + the V4 pin's | the `(v4 pin)` lines, and `pin rule: 4 pages, 0 violations` | same |
| scaffold rule | unmoved | 45 units, 0 violations, **0 compile errors** (2.9) | same |
| `linkcheck` | unmoved | 166 / 0 | 166 / 0 |
| `pagelint` | unmoved | 0 errors / 512 / 162, `--plant` 5 / 5 | same: #199's blocks carry their `using`s |
| shape · redirects | unmoved | 161 pages, widest 12 of 20 · 77 entries, 7858 bytes | same |
| `optioncheck` | unmoved | 0 / 59 tables / 519 rows | same |
| `symbolcheck` | unmoved | 0, 22 entries, 3 silenced | same |
| `versioncheck` | unmoved, 0 of 18 | **18 stale of 18** | **18 stale of 18** |
| `--verify-extraction` | — | 990 / 990 | — |
| probes | — | `handler_plants.py`, `unit_plants.py`, `net10.py`, `groups.py`: exit 0 | — |

**Every difference has its mechanism:**
- **The extra scope lines** are 2.6's pin rule and 2.9's compile errors. The prediction named only
  the V4 assemblies line. Both are this phase's instruments, and neither is a regression.
- **The corpus figures after the merge** are #199's three blocks, which `master`'s own gate
  admitted as FAILED.
- **`versioncheck` is red for a reason outside this repository.** Brighter **10.8.0** reached
  NuGet on **2026-10-06**. The five tutorial pages pin 10.7.0, so all 18 pins are stale. `master`'s
  `versions` job has failed on every scheduled run from 2026-10-06, an hour after the release, to
  2026-10-11, and on #199's push. 2026-10-05's run passed. The phase touched none of those pages. Bumping them changes the published site, and the
  bump belongs with the `refs` pin (10.7.0) and a replay of each tutorial. So it is put to the
  maintainer as its own change and not made here.

**Readings (obligation 19).** `git diff --name-only f6a9a7c -- contents SUMMARY.md README.md
.gitbook.yaml` lists only #199's two pages, which arrived by merge. **Phase 2 changed no page**, so
it adds no `// ...` and no behavioural claim, and AC9 and AC10 have nothing to read.

**`tools/README.md` row 9** was re-derived at the merged head: 993 / 303 / 673 / 17, 0 findings.
AC14's corrected form is **1** for `993 blocks`, `303 BUILT` and `673 FAILED`.

**Carried to later phases:**
- `Telemetry.md` #6, #7, #8 (`CS0234`, #199's): tranche-page blocks for phase 8's residual reading
- the readings named in § *Phase 2 as executed* for the repair phases

---

## Phases 3–7 — the section groups

**Each repair phase has the same spine.** N.1 predicts; N.2 and N.3 repair, under design § *Page
Repair Rules* and 017's rules (obligation 13); N.4 runs behaviour; N.5 is the second pass
(obligation 21); the last task closes. The page list is § *The groups*' table for that phase, drawn
at 2.14 and not before. **Every phase changes the site.**

### Phase 3 — S1, Scheduler *(7 tasks, one PR)*

- [x] **Task 3.1:** Predict phase 3's gate movement
  - Input: § *The groups*, S1; the recurrence greps for every DEFECT in S1's table (obligation 20)
  - Output: § *Phase 3 as executed*, *Predicted*: BUILT, FAILED, corpus and `pagelint` debt as
    bands, the off-group term listed by page and block, and `symbolcheck` **22 → 24 entries, 3 → 14
    silenced** (design E6, D10)

- [x] **Task 3.2:** Repair S1's reachable blocks
  - Input: S1's table, the `built-by-using`, `values` and `page-type` rows; 017 § *Scaffold Stub
    Rules*
  - Output: each block given its `using`s in the block, or a stub in a scaffold unit. Each leaves
    BUILT, or listed in § *Blocks that stay FAILED* with its diagnostic

- [x] **Task 3.3:** Repair S1's hard blocks
  - Input: S1's table, the `parse` and `defect` rows; design E2's buckets; 10.8.0 source for each
    defect, the pinned release
  - Output: each block in one of the four states. Literal `...` rewritten as `// ...`. The three
    JSON fences (`AwsScheduler.md` #13, #14, `AzureScheduler.md` #19) retagged `json`. Splits in
    § *Splits*, defects in § *Defect ledger* with their recurrence greps. `AwsScheduler.md` #2 and
    #3, the V4 blocks, are judged against the V4 pin

- [x] **Task 3.4:** Repair `AddServiceActivator` and `UseExternalBus`, and watch for both (P0-10)
  - Input: design E6, D5, D10 and D11; `tools/symbolwatch.tsv`'s row format
  - Output: `AwsScheduler.md` and `AzureScheduler.md` calling `AddConsumers` and `AddProducers`; two
    rows in `tools/symbolwatch.tsv`; `<!-- symbolcheck: allow AddServiceActivator -->` and
    `<!-- symbolcheck: allow UseExternalBus -->` beside the first site on `FAQ.md` and
    `V10MigrationGuide.md`. `symbolcheck` → *"24 entries … 14 silenced"*, exit 0;
    `symbolcheck --verify-list` → the new rows DEAD at both refs. The red run (15 sites, exit 1) is
    recorded, and `tools/README.md` row 8 updated

- [x] **Task 3.5:** Run S1's behavioural claims (AC10)
  - Input: every block 3.2–3.4 changed that asserts behaviour
  - Output: § *Phase 3 as executed*, a *Claim / Case / Control* table. Each row names its
    `probe/s1/<name>/Program.cs` (obligation 18)

- [x] **Task 3.6:** Second pass for S1 (obligation 21)
  - Input: the recurrences 3.1 listed; any ruling asked for in 3.2–3.5
  - Output: every recurrence repaired, or its ruling recorded; § *Defect ledger* rows' *After*
    column filled from their greps, run as written

- [x] **Task 3.7:** Close phase 3
  - Input: § 1 obligations 6, 7, 19
  - Output: baseline rows for every new BUILT block; `--report` exit 0; `pagelint` 0 errors; the
    phase's readings (AC9, AC10) over its diff; gates reconciled against 3.1; the PR, with sign-off
    and head-ref deletion asked by name

### Phase 3 as executed

**Measured at phase start, `f3f685f`.** `--report` → **992 / 304 / 671 / 17, 0 findings**, exit 0;
`--classify` → 141 parse, 36 built-by-using, 96 defect, 28 same-page, 77 values, 293 page-type,
settled in 3 rounds. 551 reference assemblies on each pin.

#### Predicted (3.1)

**Said and measured.** The pins moved to Brighter **10.8.0** (bugfix 0002, `5e0a63b`) after
§ *The groups* was drawn at 10.7.0.

| Said | Measured | Where the text now stands |
|---|---|---|
| § *The groups*, drawn at 10.7.0: S5 **22 pages / 136** FAILED, 11 with nothing BUILT; totals **634**, 93 defect, 276 page-type, 37 with nothing BUILT | `groups.py` at `f3f685f`: **S1 identical row for row** (8 / 129, 3 with nothing BUILT). S2–S4 unchanged in pages and FAILED. S5 **22 / 134**, 10 with nothing BUILT: `CausationTrackingStores.md` #1 builds, and `V10MigrationGuide.md` lost a block. Totals **632**, 88 defect, 279 page-type, **36** with nothing BUILT. Two methods: the FAILED column, 632; `awk` over `--report` and the tranche list, 628 on 64 pages + the 4 V4 blocks, 632 | the tables and totals are re-drawn |
| Design E2 and D3: four `OnceOnlyAction.Replay` blocks are *forthcoming*, SKIPPED | it shipped in 10.8.0. `TurningOnReplayOnSeen.md`'s 5 defect blocks are now 1 defect + 4 page-type, so D3's first reason has **0** blocks | design E2, § *Page Repair Rules* and § *The targets* say so |
| Task 3.3: *"10.7.0 source for each defect"* | the pins are 10.8.0 | the task says 10.8.0 |
| Design D5, E6 and task 3.4: `symbolcheck` 22 → 23 entries, 3 → 8 silenced | both sites also chain `UseExternalBus`, dead the same way. **D10** adds its row: **24 entries, 14 silenced** | design D10 and the gate row; tasks 3.1 and 3.4 |
| S1's table: **12 defect** blocks | the second compile's diagnostics (below): **5** are API defects, **4** are two snippets in one fence, **1** is a fragment, and **2** are misreads by the classifier | the table stands, being the instrument's; the repairs follow the reading |

**Reading S1's 12 defect blocks.** `--explain` shows only the first compile, where a missing `using`
hides what comes after it. `probe/second.py` (obligation 18) keeps `--classify`'s stage and explains
it after the `using`s are supplied:

```bash
python3 spec/018-compile_residual/probe/second.py <scratch> contents/AwsScheduler.md … contents/SwitchingSchedulers.md
# 129 FAILED blocks classified … 12 defect (6 pages); 524 diagnostic lines
```

| Page | # | Second compile | Reading against 10.8.0 | Repair |
|---|---:|---|---|---|
| `AwsScheduler.md` | 3 | `CS1061` `AddServiceActivator`; `CS1503` ×3 on `SqsSubscription<FireAwsScheduler>` | ledger 1 and 4. The generic `SqsSubscription<T>` begins `getRequestType` (V4) or `subscriptionName, channelName, channelType, routingKey` (V3); the page passes three names positionally | 3.4; 3.3, named arguments |
| `AzureScheduler.md` | 6 | `CS1061` `AddServiceActivator` | ledger 1 and 4 | 3.4 |
| `AzureScheduler.md` | 22 | `CS1739` `deadLetterQueueHandling` | no such parameter; dead-lettering is set on `AzureServiceBusSubscriptionConfiguration` (`DeadLetteringOnMessageExpiration`) | 3.3, ledger 5 |
| `CustomScheduler.md` | 1 | `CS0535` ×9 | `IAmAMessageSchedulerAsync`/`Sync` have five members each; the block shows two, without the `CancellationToken` | 3.3, ledger 6 |
| `CustomScheduler.md` | 3 | `CS0535` ×10 | the same, for `IAmARequestSchedulerAsync`/`Sync` | 3.3, ledger 6 |
| `BrighterSchedulerSupport.md` | 2, 3 | `CS0128` `schedulerId` | two snippets in one fence | 3.3, split |
| `HangfireScheduler.md` | 25 | `CS0128` `scheduledTime` | two snippets | 3.3, split |
| `QuartzScheduler.md` | 23 | `CS0128` `scheduledTime` | two snippets | 3.3, split |
| `AwsScheduler.md` | 22 | `CS0841` `awsConnection` | a fragment: the value is used in the line that declares it | 3.3 |
| `HangfireScheduler.md` | 1 | `CS1501` `GetConnectionString` | **a misread.** The classifier supplied `Microsoft.EntityFrameworkCore`, whose `DatabaseFacade.GetConnectionString()` takes no argument. With `using Microsoft.Extensions.Configuration;` the block **builds** | 3.2, a `using` |
| `HangfireScheduler.md` | 7 | `CS0266` `IsolationLevel` | **a misread.** It supplied `System.Data`; Hangfire.MySql takes `System.Transactions.IsolationLevel`. With that `using`, only `configuration` and `connectionString` remain: *values* | 3.2, a `using` and a stub |

The two misreads were each compiled with the corrected `using` in scratch, against the main pin.

**Recurrence greps (obligation 20), run as written at `f3f685f`:**

| Defect | Grep | Hits | Off-group |
|---|---|---:|---|
| 1 `AddServiceActivator` as current code | `grep -rn 'Services.AddServiceActivator(' contents/` | **2** | none; the 5 V9 discussion lines on `FAQ.md` and `V10MigrationGuide.md` take an opt-out (D5) |
| 4 `UseExternalBus` as current code | `grep -rn '^\.UseExternalBus(' contents/` | **2** | none; the 6 discussion lines take an opt-out (D10) |
| 5 `deadLetterQueueHandling` | `grep -rn 'deadLetterQueueHandling' contents/` | **1** | none |
| 6 a scheduler interface implemented in part | `grep -rnE 'class \w+ : IAmA(Message\|Request)SchedulerAsync' contents/` | **2** | none |
| 7 `SqsSubscription<T>` given its names positionally | `grep -rn -A1 'new SqsSubscription<' contents/ \| grep -E '^\S+-\s+new SubscriptionName\('` | **1** | none. `AWSSQSConfiguration.md`'s four use `name:`, a different defect, which `--classify` already counts on that page (`AWSSQSConfiguration.md` #7, `CS1739`) |

**The off-group term is two pages and no block.** `FAQ.md` and `V10MigrationGuide.md` gain the two
opt-out comments, outside any fence, so no verdict and no `pagelint` count can move on them.

**Predicted:**

| Gate | At `f3f685f` | Predicted at the close | Mechanism |
|---|---|---|---|
| corpus | 992 | **993–1,000** | −3 for the JSON retags; **+4** at least for the four `CS0128` splits; up to +4 more if a parse block holds types and statements |
| BUILT | 304 | **370–425** | S1's 86 reachable blocks (8 built-by-using, 42 values, 36 page-type), less those a stub cannot reach, plus the repaired hard blocks and their splits. Off-group: **0** |
| FAILED | 671 | **544–580** | −129 for S1, plus what stays FAILED and listed: at least the 2 *same-page*, `HangfireScheduler.md` #12 and #20 |
| SKIPPED | 17 | **17–35** | D3's *single option shown alone*: `AwsScheduler.md` #17, #23, #24, `AzureScheduler.md` #15, #16 and kin, each where its type resolves in the pin |
| `pagelint` | 0 errors / 512 | **0 errors / 400–460** | S1's 111 warnings (21 + 20 + 5 + 4 + 18 + 11 + 25 + 7, `--per-page`) fall as blocks gain their `using`s or leave the C# corpus; a declared `// ...` keeps its warning. Off-group: **0** |
| `symbolcheck` | 22 entries, 3 silenced | **24 entries, 14 silenced**, exit 0 | D5 and D10 |
| scaffold rule | 45 units | **46–52 units**, 0 violations, 0 compile errors | a unit per S1 page that needs a stub; `AzureSchedulerContext.cs` exists |
| `linkcheck` · shape · redirects · `--verify` | 168 / 0 · 161, widest 12 · 77 · 161 = 161 | **unmoved** | no link or `SUMMARY.md` change; `spec/` is outside the walk |
| `versioncheck` · `optioncheck` | 0 / 18 · 0 / 59 / 529 | **unmoved** | no tutorial pin and no option table touched |
| `pagelint --plant` | 5 / 5 | **unmoved** | |

#### `AddServiceActivator` and `UseExternalBus` (3.4, D5, D10, D11)

**The pages.** `AwsScheduler.md` #3 and `AzureScheduler.md` #6 now call `AddConsumers` and chain
`AddProducers`, each with `using Paramore.Brighter.Extensions.DependencyInjection;` added beside the
ServiceActivator one, since `AddProducers` lives there at 10.8.0. `AwsScheduler.md` #3 also passes
its three names to `SqsSubscription<FireAwsScheduler>` by name (ledger 7). At the second compile,
`AzureScheduler.md` #6 has **no** diagnostic left, and `AwsScheduler.md` #3 has two, `credentials`
and `RegionEndpoint`: a value and a `using`, both 3.2's.

**The watchlist.** Two rows, `AddServiceActivator` → `AddConsumers` and `UseExternalBus` →
`AddProducers`. Both opt-outs sit on their own lines at each discussion page's first site: in
`FAQ.md` above the list that names them, and in `V10MigrationGuide.md` under *Breaking Change*,
above the V9 block's skip marker and not between it and its fence.

| Run | Exit | Output |
|---|---:|---|
| **Red**: the two rows, pages as at `f3f685f` (`git stash push -- contents/`) | **1** | *"15 site(s) across 4 page(s), from 24 watchlist entries"*: `AddServiceActivator` 7, `UseExternalBus` 8 |
| **Green**: pages repaired, opt-outs in | **0** | *"24 entries, 161 pages checked, 14 silenced"*: `FAQ.md` 2 + 2, `V10MigrationGuide.md` 3 + 4, and the 3 from before |

**`--verify-list` (D11).** Task 3.4 said the new rows would resolve DEAD. They resolved **LIVE**,
exit 1: `AddServiceActivator` in 2 files and `UseExternalBus` in 1, at 10.8.0 and at
`origin/master`, every hit a `///` doc comment. `resolve()` now reads each file `git grep -l`
names, at its ref, through `strip_noncode`.

| Case | Before D11 | After D11 |
|---|---|---|
| `AddServiceActivator` (comments only) | LIVE, 2 / 2 | **DEAD**, 0 / 0 |
| `UseExternalBus` (comments only) | LIVE, 1 / 1 | **DEAD**, 0 / 0 |
| Control present, `CommandProcessor` | LIVE, 45 / 45 | **LIVE**, 15 / 15 |
| Control absent, `IAmAnIbox` | DEAD, 0 / 0 | **DEAD**, 0 / 0 |
| The 22 rows before this phase | DEAD | **DEAD**, all 22 |
| Every replacement, `AddConsumers` and `AddProducers` included | live | **live** |
| Exit | **1** | **0**, *"All 24 entries still dead at both refs"* |

**`CommandProcessor` 45 → 15 was read, not trusted.** Each of the 30 files that dropped out was
re-read at 10.8.0, and every line naming `CommandProcessor` as a word starts `//`, `*` or `/*`.
None is code, so the strip took nothing a compiler would see.

#### The repairs (3.2, 3.3)

**Every S1 block leaves in one of the four states.** Of the 129 FAILED at `f3f685f`, **122** are BUILT,
**3** are retagged `json` (`AwsScheduler.md` #13, #14; `AzureScheduler.md` #19), **4** stay FAILED and
are listed (§ *Blocks that stay FAILED*), all *same-page*, and **none** is SKIPPED. Four fences were
split (§ *Splits*), so the S1 pages now hold 141 C# blocks and 137 of them build. Eight scaffold
units are new (`AwsScheduler`, `BrighterSchedulerSupport`, `CustomScheduler`, `HangfireScheduler`,
`InMemoryScheduler`, `QuartzScheduler`, `SwitchingSchedulers`) or extended (`AzureScheduler`).

**No block was SKIPPED under D3.** Each single option shown alone (`AwsScheduler.md` #15, #16, #21,
#22 as they now number; `AzureScheduler.md` #15, #16) was made whole inside the call it belongs to,
which costs a line or two and gives the reader something that runs.

**What the repairs found that the table did not.** The table's 12 *defect* blocks held 5 API defects
(3.1). The *parse* and *values* rows hid 15 more falsehoods (ledger 8–19), and running the blocks
found the twentieth (below). The pattern behind them is the same one: a block that does not compile
has never been checked for anything else.

**Statements before types.** `HangfireScheduler.md` #11, #17, #18 and `QuartzScheduler.md` #15, #16
declared a type and then wrote statements after it, which C# rejects. Each now registers first and
declares after, which is the order a `Program.cs` needs.

**A rule the repairs kept: services take their dependencies.** Every *values* block that named a
`_repository`, `_logger` or `_commandProcessor` it never declared (`OrderService`, `ReportService`,
`TrialService`, `AnalyticsService` on five pages) now takes them by primary constructor. A field
stub could not have typed `_repository` without inventing a type the page never names (017
§ *Scaffold Stub Rules*, rule 1).

#### Behaviour (3.5, AC10)

**Every run is against the released 10.8.0 packages**, as `probe/s1/<name>/` projects, and each
prints `ok` or `FAIL` per row and exits 1 on any `FAIL`. Each block is copied verbatim, the probe's
own lines marked `// probe`.

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `CustomScheduler.md` #1, #2: a message scheduled through `CustomMessageScheduler` is produced when its job fires and `CustomSchedulerHandler` sends the payload | schedule, fire | schedule, do not fire | `custom-scheduler` | produced / not produced |
| `CustomScheduler.md` #3, #4: a request scheduled through `CustomRequestScheduler` reaches its handler when `CustomRequestHandler` sends the payload | schedule, fire | schedule, do not fire | `custom-scheduler` | handled / not handled |
| `InMemoryScheduler.md` #9: `Should_Execute_Scheduled_Command_After_Delay` and `Should_Cancel_Scheduled_Command` pass | both tests, as written | the cancel test without its `CancelAsync` line, which must fail | `inmemory-tests` | pass, pass / `Assert.False() Failure` |
| `InMemoryScheduler.md` #1: `SendAsync(delay)` returns an id, and the command runs after the delay | the block, then the count after its `Task.Delay(150)` | the count straight after `SendAsync` | `inmemory-block1` | 1 / 0 |
| `SwitchingSchedulers.md` #7: a flag chooses the scheduler at startup | `UseQuartz=true` → both factories Quartz's | `UseQuartz=false` → both Hangfire's | `switching-flag` | as claimed, both ways |
| `AzureScheduler.md` #21: `DeadLetteringOnMessageExpiration = true` and `MaxDeliveryCount = 5` are the defaults | the block's subscription | a subscription with no configuration: the same values | `azure-config` | true, 5 / true, 5 |
| `AzureScheduler.md` #25: `LockDuration` set on the configuration is the subscription's, and 2 minutes increases it | the block's subscription | no configuration: 1 minute | `azure-config` | 2:00 / 1:00 |
| `QuartzScheduler.md` #2: `store.UseClustering(…)` turns clustering on | the block's store | the same store without it | `quartz-config` | `clustered = true` / absent |
| `QuartzScheduler.md` #10: `q.MisfireThreshold` sets the threshold | the block | no setting | `quartz-config` | `60000` / absent |
| `QuartzScheduler.md` #8: the two hooks decide the scheduler ids | the block's factory | the default factory | `quartz-config` | the message's id and the correlation id / neither |
| `BrighterSchedulerSupport.md` #8, #9: `DeferMessageAction` requeues after a delay; `requeueCount: 3` handles at most 3 times | block 8's handler always deferring, under block 9's subscription | a service that succeeds | `defer-requeue` | 3 handlings / 1 |
| `HangfireScheduler.md` #1 and every page registering Hangfire the same way: the application starts and Brighter schedules through Hangfire | the repaired form (below), then `SendAsync(TimeSpan.FromMinutes(5), …)` | the page's form | `hangfire-factory` | 1 job in Hangfire's storage / `InvalidOperationException` at startup |

**The last row overturned the page.** `HangfireMessageSchedulerFactory` initialises `Client = new
BackgroundJobClient()` as a property default (`HangfireMessageSchedulerFactory.cs:18` at 10.8.0).
That reads Hangfire's static `JobStorage.Current`, which `services.AddHangfire(…)` leaves unset until
the provider resolves Hangfire's configuration. So `.UseScheduler(new HangfireMessageSchedulerFactory())`,
written while services are configured, throws, and an object initializer setting `Client` cannot
help, because the default runs first: the first attempt at a repair failed for exactly that reason.
The repair builds the factory in `UseScheduler(provider => …)` after resolving
`IBackgroundJobClientV2`, which initialises the storage. It is ledger 20, at **12** sites on **6**
pages, two of them outside S1 (`InMemoryOptions.md`, `SchedulingAMessage.md`). Obligation 21 repairs
those here. The maintainer ruled that the cause be filed upstream: **BrighterCommand/Brighter#4600**,
2026-10-14.

**Two readings the runs corrected:**

| Said | Measured | Where the text now stands |
|---|---|---|
| This phase's reading of `ServiceCollectionExtensions.cs` at 10.8.0: `UseScheduler(provider => …)` registers only the factory, so a page injecting `IAmAMessageSchedulerAsync` beside it would fail | `scheduler-injection`: both overloads leave `IAmAMessageSchedulerAsync` and `IAmARequestSchedulerAsync` resolvable; `AddBrighter` registers them from whichever factory is present | no page changed on that reading |
| `BrighterSchedulerSupport.md` #9: *"Wait 5 seconds between attempts"* | `defer-requeue`: each gap is the delay **plus** up to one empty-channel poll, 500 ms by default (500 ms → 1010, 1007 ms; 1500 ms → 2015, 1508 ms) | *"Wait at least 5 seconds between attempts"* |

**At 10.8.0, `AutoFromAssemblies()` with a scheduled request no longer fails.** `inmemory-block1`
runs `InMemoryScheduler.md` #1 to the handler with no `ArgumentException`. That bears on the
*"At Brighter 10.7.0"* note at `InMemoryScheduler.md:241`, which is bugfix 0002's follow-up 1 and
not this phase's to rule on; it is recorded here for that follow-up.

**Read, not run.** Each needs a service the probes do not stand up, and each is read against its
source:
- `HangfireScheduler.md` #30, the allow-all filter: Hangfire's `IDashboardAuthorizationFilter`
  contract, `true` authorises
- `HangfireScheduler.md` #6, `PostgreSqlStorageOptions.SchemaName`: Hangfire.PostgreSql 1.21.1, a
  live PostgreSQL to observe the schema
- `AzureScheduler.md` #25's handler rethrowing, and the message reaching the dead-letter queue after
  `MaxDeliveryCount`: Azure Service Bus's own delivery-count rule

#### Second pass (3.6)

**Every recurrence is repaired, and no ruling was asked to stop one.** Ledger rows 1 and 4–20 each
carry their *After*, from their grep run as written: **0** for every row but 6 and 16, whose greps
count sites that are still there, now whole. The recurrence greps found only one off-group site
set, ledger 20's (`InMemoryOptions.md`, `SchedulingAMessage.md`), repaired here under obligation 21.
`SchedulingAMessage.md` #8 is the one baselined off-group block whose text changed, and it still
builds. The off-group term is therefore four pages: the two opt-out pages of 3.1, and these two.

#### Phase 3 closed (3.7)

**Baselined.** `baseline.tsv` lists every BUILT block on the eight S1 pages, **137** rows,
replacing their **11** earlier ones (§ the file's own header says how). Five keep their original
ref: `AzureScheduler.md` #1, #17, #18, and #19, #20, which were #20, #21 before the JSON retag,
matched by text against `f3f685f`. Every other row is at **`9aafd19`**. `SchedulingAMessage.md` #8
moves to `9aafd19` too, re-measured after ledger 20's repair. **Two methods agree:** the rows,
`grep -v '^#' baseline.tsv | grep -vc '^$'` → **430**, and `--report`'s BUILT → **430**.

**Rule 8.** `pagelint` → **0 errors**, so no block this phase admitted carries a handler attribute
of the wrong kind.

**Readings (obligation 19).** `git diff -U0 f3f685f HEAD -- contents`, hunks against fence ranges:
- **AC9.** The phase added `// ...` at **14** lines in **14** blocks: `AzureScheduler.md` #25,
  `BrighterSchedulerSupport.md` #7, `HangfireScheduler.md` #22, `InMemoryScheduler.md` #1,
  `QuartzScheduler.md` #8, #15, #19, and `SwitchingSchedulers.md` #1–#7. All 14 are BUILT, so
  `--explain` has nothing to report for any of them: no added omission hides an error.
  `HangfireScheduler.md` #22 and `QuartzScheduler.md` #19 are a service method's body shown alone;
  their `// ...` names the service, and CI's strict `pagelint --changed` asked for it. `QuartzScheduler.md` #15's is the page's own, moved
  with the registration it sits in
- **AC10.** § *Behaviour (3.5)*: twelve claims run with a control, three read and named

**Gates against 3.1.**

| Gate | Predicted (3.1) | Measured at the close | |
|---|---|---|---|
| corpus | 993–1,000 | **993** | in band: −3 retags, +4 splits; no block held types and statements that a split needed, since the five that did were reordered instead |
| BUILT | 370–425 | **430** | **above the band by 5** (below) |
| FAILED | 544–580 | **546** | in band |
| SKIPPED | 17–35 | **17** | the floor (below) |
| `pagelint` | 0 errors / 400–460 | **0 / 410** | in band |
| `symbolcheck` | 24 entries, 14 silenced | **24, 14**, exit 0; `--verify-list` all 24 DEAD | as predicted, with D11 |
| scaffold rule | 46–52 units | **52**, 0 violations, 0 compile errors | the top of the band: seven new units |
| assemblies | not predicted | 551 → **552** on each pin | D12, ruled at 3.3 after the prediction |
| `linkcheck` · shape · redirects · `--verify` | unmoved | 168 / 0 · 161, widest 12 of 20 · 77, 7858 bytes · 161 = 161 | as predicted |
| `versioncheck` · `optioncheck` | unmoved | 0 of 18 · 0 / 59 / 529 | as predicted |
| `pagelint --plant` | unmoved | 5 / 5 | as predicted |

**Why BUILT came in above the band, and SKIPPED at its floor: the same mechanism.** 3.1 expected D3
to skip the single options shown alone, named five of them and *"kin"*, and held BUILT's ceiling
below S1's whole table by that allowance. None was skipped. Each was made whole inside its call
(§ *The repairs*) and built, nine blocks in all (`AwsScheduler.md` #15, #16, #21, #22;
`AzureScheduler.md` #15, #16; `HangfireScheduler.md` #21; `QuartzScheduler.md` #25, #28). The
excess is that allowance, not a new mechanism, and the number is not adopted: the next phase
predicts knowing that a fragment is made whole before it is skipped.

---

### Phase 4 — S2, Commands, Handlers and Pipelines; Get Started *(6 tasks, one PR)*

- [x] **Task 4.1:** Predict phase 4's gate movement
  - Input: § *The groups*, S2; recurrence greps for S2's defects
  - Output: § *Phase 4 as executed*, *Predicted*, as 3.1's
- [x] **Task 4.2:** Repair S2's reachable blocks
  - Input: S2's table, reachable rows
  - Output: as 3.2's, for S2. `ShowMeTheCode.md` cites the WebAPI sample, so obligation 17 applies
    to its blocks (017 design rule 9)
- [x] **Task 4.3:** Repair S2's hard blocks
  - Input: S2's table, hard rows; 10.8.0 source, the pinned release
  - Output: as 3.3's, for S2. Each handler block on an S2 page that 2.5 did not wrap or build, as
    named in § *Phase 2 as executed*, is repaired or listed
- [x] **Task 4.4:** Run S2's behavioural claims (AC10)
  - Input: S2's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s2/<name>/Program.cs`
- [x] **Task 4.5:** Second pass for S2
  - Input: 4.1's recurrences; rulings asked
  - Output: as 3.6's
- [x] **Task 4.5a:** Sweep the prose that dates a defect to 10.7.0 (D19)
  - Input: `grep -rn '10\.7\.0' contents/`; the five defects D19 names, and `OnceOnlyAction.Replay`
  - Output: each defect run at 10.7.0 (the control) and 10.8.0 under `probe/<group>/<name>/`, a
    *Claim / Case / Control* table; every site repaired; one § *Defect ledger* row per defect
- [x] **Task 4.5b:** Refresh the relational DDL (D20)
  - Input: R7's seven blocks; each builder's `GetDDL` at 10.7.0 and 10.8.0
  - Output: each block regenerated from 10.8.0 and run against its database, under
    `probe/s5/<name>/`; the page says Replay needs `CausationId`; one ledger row
- [x] **Task 4.6:** Close phase 4
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 4 as executed

**Measured at phase start, `5f2294c`.** `--report` → **993 / 430 / 546 / 17, 0 findings**, exit 0;
`--classify` → 112 parse, 28 built-by-using, 84 defect, 28 same-page, 36 values, 258 page-type,
settled in 3 rounds. 552 reference assemblies on each pin; scaffold rule 52 units. `pagelint`
0 errors / **410** warnings; `symbolcheck` 24 entries, 14 silenced; `linkcheck` 168 / 0;
`versioncheck` 0 of 18.

#### Predicted (4.1)

**S2 is identical row for row** to § *The groups*: `groups.py` at `5f2294c` gives 10 pages, **107**
FAILED (15 parse, 2 built-by-using, 11 defect, 1 same-page, 11 values, 67 page-type), 3 BUILT,
7 with nothing BUILT. Two methods: the FAILED column, 107; `awk` over `--report` on the ten pages,
107, with 3 BUILT and 4 SKIPPED (`MigratingToPollyV8.md` #1, #3, #5, `PolicyFallback.md` #2, each a
labelled V9 form). The S2 pages hold **114** C# blocks.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| Task 4.2: *"`ShowMeTheCode.md` is a tutorial, so obligation 17 applies"* | its banner is **How-to**. Obligation 17 still binds, by 017 design rule 9's trigger, the sample a block cites: the page cites `samples/WebAPI` | task 4.2 gives the reason that holds |
| Task 4.3: *"10.7.0 source"* | the pins are 10.8.0 | the task says 10.8.0 |
| S2's table: **11 defect** blocks | the second compile's diagnostics (below): **3** are API defects, **1** is an API the pin does not carry, **4** are fragments, and **3** are misreads | the table stands, being the instrument's; the repairs follow the reading |

**Reading S2's 11 defect blocks** with `probe/second.py` over the ten pages (497 diagnostic lines),
each against the 10.8.0 tag:

| Page | # | Second compile | Reading against 10.8.0 | Repair |
|---|---:|---|---|---|
| `AgreementDispatcher.md` | 7 | `CS1061` `RegisterAsync` on `IAmASubscriberRegistry` | **defect.** `.Handlers(…)` takes `Action<IAmASubscriberRegistry>`; `RegisterAsync<TRequest>(router, handlerTypes)` is on `IAmAnAsyncSubcriberRegistry`, which `.AsyncHandlers(…)` takes (`IBrighterBuilder.cs:54`) | 4.3, `.AsyncHandlers` |
| `UsingTheContextBag.md` | 8 | `CS1061` `Activity.SetAttribute` ×3; `CS1503` `AddEvent(string)` ×2 | **defect.** `IRequestContext.Span` is `System.Diagnostics.Activity?` (`IRequestContext.cs:102`), which has `SetTag` and `AddEvent(ActivityEvent)`; `SetAttribute` is OpenTelemetry's `TelemetrySpan` | 4.3, `SetTag` and `new ActivityEvent(…)` |
| `MigratingToPollyV8.md` | 8 | `CS1061` `int.Seconds()` ×3 | **defect.** No pinned package declares `Seconds(this int)`, and nothing in Brighter 10.8.0 does; it is a test library's helper the page never names | 4.3, `TimeSpan.FromSeconds` |
| `FeatureSwitches.md` | 6 | `CS0305` `ConfigurationManager<T>`, `CS0029` ×2 | **an API the pin does not carry.** The classifier supplied a namespace whose `ConfigurationManager` is generic; the page means .NET Framework's `System.Configuration.ConfigurationManager.AppSettings`, which a .NET 9 project has only through a package the page never names | 4.3: the registry read from `IConfiguration`, or the package named; judged at the repair |
| `AsyncDispatchARequest.md` | 4 | `CS0117` `'object'` has no `HandleAsync` | a fragment: one `return await base.HandleAsync(…)` statement, which the wrapper cannot reach (§ *Phase 2 as executed*) | 4.3, made whole inside its handler |
| `PipelineValidation.md` | 7, 8 | `CS0161` | a fragment: an `async Task<OrderCreated>` whose body is `// ...` alone. #7 carries rule 8's `attr-mismatch-intended` marker, which a body leaves alone | 4.3, a `return` |
| `PolicyFallback.md` | 5 | `CS0104` `FallbackPolicy`, `CS0161` | `CS0104` is **a misread**: the classifier supplied Darker's attributes beside Brighter's. `CS0161` is a fragment, as above. `FallbackPolicyAttribute(backstop, circuitBreaker, step)` matches 10.8.0 (`FallbackPolicyAttribute.cs:46`) | 4.3, a `return`; 4.2, Brighter's `using` only |
| `MigratingToPollyV8.md` | 11, 12 | `CS0653` `ValidationAttribute`; `CS1061` `Task.TaskName` ×3 | **misreads.** `Task` is the page's domain type, not `System.Threading.Tasks.Task`, and `[Validation]` is no Brighter attribute at 10.8.0 (no `Paramore.Brighter.Validation.*` package declares one), so both are the reader's: *page-type*. Whether the page should say so is read at 4.3 | 4.2, stubs |
| `ShowMeTheCode.md` | 1 | `CS1955` `Ok` | **a misread**: a controller action outside its controller. *page-type* | 4.2, obligation 17 |

**Recurrence greps (obligation 20), run as written at `5f2294c`:**

| Defect | Grep | Hits | Off-group |
|---|---|---:|---|
| `RegisterAsync` inside `.Handlers(…)` | `grep -rn -A30 '\.Handlers(registry' contents/ \| grep RegisterAsync` | **1** (`AgreementDispatcher.md` #7) | none. Of the other **8** `\.RegisterAsync<` lines that are not mappers, 3 are already under `.AsyncHandlers(…)` and 5 show the registry alone |
| `Activity.SetAttribute`, `AddEvent(string)` | `grep -rnE 'Span\??\.(SetAttribute\(\|AddEvent\(")' contents/` | **8** lines in 3 blocks | **`CloudEventsSupport.md` #5** (S4). On-group: `UsingTheContextBag.md` #8, #17 |
| `int.Seconds()` | `grep -rnE '[0-9]\.Seconds\(\)' contents/` | **4** lines in 2 blocks | none: `MigratingToPollyV8.md` #1 (SKIPPED, V9 form) and #8 |
| `ConfigurationManager.AppSettings` | `grep -rn 'ConfigurationManager.AppSettings' contents/` | **1** (`FeatureSwitches.md` #6) | none |

**The off-group term is one page and one block**, `CloudEventsSupport.md` #5, FAILED today
(`CS0246`, `CS1061`). Obligation 21 repairs its `SetAttribute` here; whether it then builds depends
on its other diagnostic, so it is **0–1** BUILT.

**Predicted:**

| Gate | At `5f2294c` | Predicted at the close | Mechanism |
|---|---|---|---|
| corpus | 993 | **993–1,000** | splits only, where a fence holds alternatives: `PipelineValidation.md` #3 and #13 are candidates. No S2 parse block reads as JSON or YAML |
| BUILT | 430 | **522–537** | S2's 107, less what stays FAILED (at least `PolicyRetryAndCircuitBreaker.md` #11, *same-page*) and any SKIPPED, plus the repaired defects and splits, plus 0–1 off-group. Phase 3 made 122 of 129 build (95%) by making fragments whole rather than skipping them, and this band assumes the same |
| FAILED | 546 | **438–450** | −107 for S2, −0–1 off-group, plus what stays FAILED and listed |
| SKIPPED | 17 | **17–21** | a V9 form found unlabelled on an S2 page, labelled as such. D3's single-option skip is not expected: a fragment is made whole first |
| `pagelint` | 0 errors / 410 | **0 errors / 331–345** | S2's **79** warnings (12 + 4 + 6 + 2 + 10 + 13 + 5 + 10 + 12 + 5, `--per-page`) fall as blocks gain their `using`s; a declared `// ...` keeps its warning. Off-group: `CloudEventsSupport.md` −0–1 |
| `symbolcheck` | 24 entries, 14 silenced | **unmoved** | none of S2's defects is a Brighter name gone dead; `SetAttribute` and `Seconds` never were Brighter's |
| scaffold rule | 52 units | **52–62 units**, 0 violations, 0 compile errors | a unit per S2 page that needs a stub |
| assemblies | 552 | **552–553** | +1 only if `FeatureSwitches.md` #6 keeps `ConfigurationManager.AppSettings` and its package is pinned |
| `linkcheck` · shape · redirects · `--verify` | 168 / 0 · 161, widest 12 of 20 · 77 · 161 = 161 | **unmoved** | no link or `SUMMARY.md` change |
| `versioncheck` · `optioncheck` | 0 of 18 · 0 / 59 / 529 | **unmoved** | no tutorial pin and no option table touched |
| `pagelint --plant` | 5 / 5 | **unmoved** | rule 8's marker on `PipelineValidation.md` #7 stays, bound to the same block |

#### The repairs (4.2)

**S2's reachable rows, 80 blocks** (2 built-by-using, 11 values, 67 page-type), plus
`MigratingToPollyV8.md` #11, #12, which 4.1 read as page-type misreads. **77 build**, and three stay
FAILED: `FeatureSwitches.md` #5 and `PolicyRetryAndCircuitBreaker.md` #17 are *same-page* (§ *Blocks
that stay FAILED*), and `PolicyRetryAndCircuitBreaker.md` #12 waits on a ruling (below).
`ShowMeTheCode.md` #1, also read as a misread, waits on a ruling too. `AgreementDispatcher.md` #7's
defect (4.1) was repaired with its page, since the block was otherwise whole. Nine scaffold units
are new (`AgreementDispatcher`, `AsyncDispatchARequest`, `DispatchingARequest`, `FeatureSwitches`,
`PipelineValidation`, `PolicyFallback`, `ShowMeTheCode`, `UsingTheContextBag`) or extended
(`MigratingToPollyV8`, `PolicyRetryAndCircuitBreaker`), with **0** unit-rule violations.

**Measured after 4.2**, at `074eb99`: `--report` → **993 / 512 / 464 / 17**; 81 findings, every one a BUILT block not
yet baselined, or `UsingTheContextBag.md` #12, whose scaffold changed (4.6 writes the rows).
`pagelint` 0 errors / **372**.

**What the reachable rows hid.** As in phase 3, a block that did not compile had not been checked
for anything else. Ten falsehoods sat in *page-type* and *values* rows, ledger 21–30, and one of them
was a behaviour claim the page's own troubleshooting repeated:

- **`UseTypePipeline` (ledger 24).** The page said Brighter looks the pipeline up under
  `"<handler type full name>.<policy>"`, as the attribute's XML documentation also says.
  `ResilienceExceptionPolicyHandler.cs:78` at 10.8.0 calls `GetPipeline<TRequest>(policy)`.
  `probe/s2/type-pipeline` registers each form and sends a command whose handler fails once:
  the page's key throws `ConfigurationException` ← `KeyNotFoundException` before the handler runs;
  a pipeline typed for the request, under the plain name, runs it twice; the control (no
  `UseTypePipeline`, an untyped pipeline) runs it twice. The same probe measured the two
  *Troubleshooting* symptoms the page gave as `InvalidOperationException` (ledger 26)
- **Readonly dependencies never assigned.** `DispatchingARequest.md` #2–#4 and `PolicyFallback.md`
  #1, #8 declared `_commandProcessor`, `_cache` and others as fields no constructor set, or used
  fields they never declared. Each now takes them by primary constructor (phase 3's rule).
  `PolicyFallback.md` #9, a `Fallback` using three such fields, is made whole in its class

**Rulings asked** (obligation 21: a change of scope goes to the maintainer), and **ruled 2026-10-15, each as recommended**: R1–R4 are design **D13–D16**. R4 is filed as **BrighterCommand/Brighter#4609**.

| # | Block | Question |
|---:|---|---|
| R1 | `PolicyRetryAndCircuitBreaker.md` #12 | `AddConcurrencyLimiter` is in `Polly.RateLimiting`, which no pin carries and the page never names. Pin `Polly.RateLimiting` 8.8.0 (Polly's version on the pin), as D12 pinned `Quartz.Serialization.Json`, and name the package on the page? |
| R2 | `FeatureSwitches.md` #6 | `ConfigurationManager.AppSettings` is .NET Framework's. Rewrite the registry to read `IConfiguration`, or keep it and name `System.Configuration.ConfigurationManager`? |
| R3 | `ShowMeTheCode.md` #1, #4 | #1 is the sample's `GreetingsController.Post` without its class, so `Ok(…)` cannot resolve; building it means showing the controller (obligation 17). #4 sends `AddGreeting { PersonId }`, which the sample's `AddGreeting(name, greeting)` does not have |
| R4 | upstream | `UseResiliencePipelineAttribute.UseTypePipeline`'s XML documentation describes the handler-type key the runtime does not use. File it, as Brighter#4600 was filed? |

#### The repairs (4.3)

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| *The repairs (4.2)*: `--report` **993 / 510 / 466 / 17** after 4.2 | at `074eb99`, **993 / 512 / 464 / 17**, by two runs: one on the pins as they were, one with D13's `Polly.RateLimiting` added, identical block for block. Off S2 the two agree with the close of 4.3 (427 / 439 / 13), so the 2 are S2's: **85** BUILT, 25 FAILED, 4 SKIPPED at `074eb99` | 4.2's line gives `074eb99`'s figures |
| 4.1, assemblies: +1 *"only if `FeatureSwitches.md` #6 keeps `ConfigurationManager.AppSettings` and its package is pinned"* | **553**, +1, from D13's `Polly.RateLimiting`; D14 rewrote #6 without a package | the band held; the mechanism was D13, not D14 |

**S2's 25 FAILED blocks at `074eb99`**: **23 are BUILT**, and the two *same-page* rows of § *Blocks
that stay FAILED* (`FeatureSwitches.md` #5, `PolicyRetryAndCircuitBreaker.md` #17) stay. None is
SKIPPED, and none is retagged. One fence was split (`PipelineValidation.md` #3, two alternative
chains, § *Splits*), so the S2 pages hold **115** C# blocks: **109 BUILT**, 2 FAILED, 4 SKIPPED,
`awk` over `--report` on the ten pages, against the summary's 994 / 536 / 441 / 17 less the
unchanged 427 / 439 / 13 off S2.

- **The rulings.** D13: `Polly.RateLimiting` 8.8.0 in both refs csproj, and the page tells its reader
  to add it before the *Rate Limiter Example*. D14: `FeatureSwitches.md` #6 is a
  `ConfigurationFeatureSwitchRegistry` taking `IConfiguration`, its configuration `appsettings.json`
  under a `FeatureSwitches` section; the old registry also called `.ToLower()` on a missing key,
  which throws before `MissingConfigStrategy` is read. D15: `ShowMeTheCode.md` #1 is the sample's
  `GreetingsController.Post`, in its controller and with the sample's `FindPersonByName` check;
  #4 inserts for the person `AddGreeting.Name` names
- **Made whole, as phase 3 did.** `AsyncDispatchARequest.md` #1 (its class closed, its `using`s)
  and #4 (the `return` inside a `GreetingCommandHandlerAsync`); `PipelineValidation.md` #3–#8 and
  #11–#15, each chain on the builder it extends, each handler with its `return`, the Reactor and
  missing-handler examples with real subscriptions; `PolicyFallback.md` #5;
  `AgreementDispatcher.md` #1, the signature written as a call; `ShowMeTheCode.md` #3, the
  sample's `QueryHandlerAsync` in its class; `UsingTheContextBag.md` #16, its statement before its
  type
- **What the hard rows hid.** Ledger 31–35: a parameter name no overload has; `Activity` given
  OpenTelemetry's `TelemetrySpan` API (and `CloudEventsSupport.md` #5, off-group, obligation 21);
  `int.Seconds()`; and a query that dropped the sample's `where p.Name = @name`, so it returned
  every person's greetings and `Single()` threw once there were two people
- **Off-group, ledger 21.** `new List<Guid>()` is **0**: `BrighterOutboxSupport.md` and
  `MongoDBOutbox.md` collect `Id`s, and `EFCoreOutbox.md`'s handler is the 10.8.0
  `WebAPI_EFCore` sample's, which starts the transaction through the `IAmATransactionConnectionProvider`
  and passes it to `DepositPostAsync`. The three pages stay FAILED on stubs (phase 7's units)
- **Scaffold.** Five units changed (`AgreementDispatcher`, `AsyncDispatchARequest`,
  `PipelineValidation`, `ShowMeTheCode`, `UsingTheContextBag`); 60 units, **0** violations after
  `Salutation.Words`, which no block names, was removed

**Measured at 4.3's close:** `--report` → **994 / 536 / 441 / 17**, 107 findings, all BUILT blocks not
yet baselined or `UsingTheContextBag.md` #12 (4.6). 553 assemblies per pin. `pagelint` 0 errors /
**347**; `--changed origin/master` 0 errors; `--plant` 5 / 5. `linkcheck` 168 / 0. `symbolcheck` 24
entries / 14 silenced.

**For 4.4** (claims a changed block makes): `FeatureSwitches.md` #6 resolves a dotted handler name
under `FeatureSwitches:` and falls back to `MissingConfigStrategy` when the key is absent;
`PipelineValidation.md` § *Missing Handler*, `AutoFromAssemblies()` scans only assemblies already
loaded (`ServiceCollectionBrighterBuilder.cs:120` reads `AppDomain.CurrentDomain.GetAssemblies()`)
and `ValidatePipelines()` then reports the subscription; `EFCoreOutbox.md`, the EF Core provider is
resolvable as `IAmATransactionConnectionProvider` from the `AddProducers` registration.

**D17, carried out.** `MongoDBOutbox.md` § *Using the Outbox in Handlers* is rewritten and run.
`probe/s5/mongo-outbox` registers the page's configuration over a single-node replica set
(`mongo:7`), and its output, at 10.8.0:

| Case | Handler | Outcome | Greeting | Outbox | Bus |
|---|---|---|---:|---:|---:|
| A | the page as it was: `IAmATransactionConnectionProvider`, cast to `MongoDbUnitOfWork` | `Send` throws `ConfigurationException` ← *Unable to resolve service for type `IAmATransactionConnectionProvider`* | 0 | 0 | 0 |
| B | R5's first form: `IAmAMongoDbTransactionProvider` passed to `DepositPostAsync` | throws *Supplied transaction provider doesn't match configured transaction type* | 0 | 0 | 0 |
| C | B's control, failing after the deposit | the same throw, before the failure is reached | 0 | 0 | 0 |
| B′ | the page now: the provider as `IAmABoxTransactionProvider<IClientSessionHandle>` | returns | 1 | 1 | 1 |
| C′ | B′'s control, the page's block with a failure planted after the deposit | catches, rolls back, returns | 0 | 0 | 0 |

**Said and measured.** R5 recommended passing `IAmAMongoDbTransactionProvider`; case B throws.
Brighter takes the transaction type from the *last* generic `IAmABoxTransactionProvider<>` on the
registered type (`CommandProcessor.cs:286`), which for `MongoDbUnitOfWork` is
`IClientSessionHandle`, the type `MongoDbOutbox` writes in. `IAmAMongoDbTransactionProvider` is
the `IClientSession` form, so it never matches, and its session cannot be handed to the driver's
collection methods either. D17 records the amendment, and D18 files it upstream as
**BrighterCommand/Brighter#4616**. B′ is the page's block verbatim but for the
planted line, and C′ is the control that shows the two writes share one transaction.
`BrighterOutboxSupport.md`'s sentence that every handler takes an `IAmATransactionConnectionProvider`
now says relational, and links the DynamoDB and MongoDB pages. The block stays FAILED on page-type
stubs (`AddGreeting`, `Person`, `GreetingMade`), phase 7's unit.

**Ruling asked** (obligation 21, a change of scope), and **ruled 2026-10-15**: R5 is design **D17**, carried out at 4.3 rather than phase 7 (*D17, carried out*, above).

| # | Block | Question |
|---:|---|---|
| R5 | `MongoDBOutbox.md` #5 | The handler casts an `IAmATransactionConnectionProvider` to `MongoDbUnitOfWork`, which does not implement it, so the cast is always null; and it reads `unitOfWork.Database` and `.Session`, which `MongoDbUnitOfWork` does not have at 10.8.0 (ledger 36). No sample covers MongoDB. Rewrite the handler on `IAmAMongoDbTransactionProvider` (`GetTransactionAsync`, `Client.GetDatabase`, `DepositPostAsync(…, provider)`) with a behaviour probe against MongoDB, in phase 7 where the page's units are written? |

#### Behaviour (4.4, AC10)

**Every run is against the released 10.8.0 packages**, as `probe/s2/<name>/` projects, and each
prints `ok` or `FAIL` per row and exits 1 on any `FAIL`. Each block is copied verbatim, the probe's
own lines marked `// probe`. Two are cited, not rerun: `type-pipeline` (4.2, ledger 24, 26) and
`probe/s5/mongo-outbox` (4.3, D17's table).

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `AgreementDispatcher.md` #7, as it was: a route returning several handlers for `ProcessOrder`, a *Command* | `Send` a domestic, low-value order (two handlers, the fewest the route returns) | the same route on an *Event*, published; a route returning one handler, sent | `agreement-dispatcher` | `ArgumentException`, *"More than one handler was found"* / all four run; one runs. **Overturned** (ledger 37) |
| `AgreementDispatcher.md` #7, as repaired: the route on `OrderPlaced`, published | a high-value international order | a domestic, low-value order | `agreement-dispatcher` | all three run / `OrderConfirmationHandler` alone |
| `AgreementDispatcher.md` § *Returning Multiple Handlers*, written at this task: `Publish` runs the handlers concurrently, in no order, and one failing does not stop the others | three runs of the four-handler `Publish` | `FraudCheckHandler` throws | `agreement-dispatcher` | three different orders, `ValidateOrderPlacedHandler` first in one, last in another (`Parallel.ForEach`, `CommandProcessor.cs:499`) / the other two run, `Publish` throws `AggregateException` |
| `AgreementDispatcher.md` #12: a returned handler missing from the list *"will fail at runtime"* | the page's *Bad* registration | the *Good* one; the *Good* list with `Handler3` returned | `agreement-dispatcher` | `ConfigurationException`, *"Handler Factory could not construct handler of type Handler3"* / runs; runs |
| `AgreementDispatcher.md` #14: `AutoFromAssemblies()` beside an agreement throws *"More than one handler"*, and `excludeDynamicHandlerTypes` cures it | the page's *Solution* | the *Problem*, no exclusion; the *Solution* routing to the other excluded handler | `agreement-autoscan` | `MyHandler` runs / `ArgumentException` as quoted; `MyOtherHandler` runs |
| `MigratingToPollyV8.md` § *Legacy*: several policies *"are evaluated left to right, so … 'MyCircuitBreakerPolicy' wraps 'MyExceptionPolicy'"* | the page's `[UsePolicy(new [] {CB, Exception})]`, a handler that always throws, sent twice | the array reversed | `legacy-policy-order` | 4 attempts, then `BrokenCircuitException` / 1 attempt |
| `MigratingToPollyV8.md` § *Legacy*: *"put a Circuit Breaker UsePolicy attribute as an earlier step than the Retry UsePolicy attribute"* | the two attributes on one method, `-p:DefineConstants=TWO_ATTRIBUTES` | the one-attribute array, above | `legacy-policy-order` | `CS0579` *Duplicate 'UsePolicy' attribute* / builds and wraps. **Overturned** (ledger 38) |
| `MigratingToPollyV8.md` § *Legacy*, unstated: `new PolicyRegistry()` is a registry Brighter takes | the page's registry and policies | `new DefaultPolicy()` with the same | `legacy-policy-order` | `ConfigurationException`, *"missing the CommandProcessor.RETRYPOLICY policy"* / resolves. **Overturned** (ledger 39) |
| `PolicyRetryAndCircuitBreaker.md` #15: a `ResilienceContext` set on `RequestContext` reaches `OnRetry` | the page's send, `tenant-42` attached; the handler fails once | the same send with no `RequestContext` | `resilience-context` | `OnRetry` reads `tenant-42` / nothing |
| `UsingTheContextBag.md` #10: the `ResilienceContext` shares data across policies | the caller attaches an empty context; the page's handler sets `tenant-id` | no context attached | `resilience-context` | `OnRetry` reads the handler's value / nothing: `Context.ResilienceContext` is null, so the handler's `if` skips. The page now says so |
| `FeatureSwitches.md` #6: a dotted handler name resolves under `FeatureSwitches:`; a missing key falls back to `MissingConfigStrategy` | `"on"` in `appsettings.json`; no key under `SilentOff`; no key under the default | `"off"`; no key under `SilentOn` | `feature-switch-config` | runs, skipped, `InvalidOperationException` / skipped, runs |
| `FeatureSwitches.md` § opening: no registry makes `Config` behave as `On`; the fluent registry's default throws `ConfigurationException` for a handler it lacks | no registry; a fluent registry without the handler | a fluent registry with it `On` | `feature-switch-config` | runs, `ConfigurationException` / runs |
| `PipelineValidation.md` § *Missing Handler*: `AutoFromAssemblies()` scans only loaded assemblies, and `ValidatePipelines()` reports the subscription | the page's *Before* block, `Handlers.dll` never touched | the *After* block; the *Before* block after `Handlers.dll` is loaded | `missing-handler` (one process per case) | *"No handler registered for 'OrderCreated'"*, not loaded / no error, loaded; no error, loaded |
| `EFCoreOutbox.md`: the EF Core provider resolves as `IAmATransactionConnectionProvider` from `AddProducers`, and its transaction is the context's | resolve it in a scope and begin | — (read, not compared) | `efcore-outbox` | `MySqlEntityFrameworkTransactionProvider<GreetingsEntityGateway>`; `CurrentTransaction` set |
| `EFCoreOutbox.md`: `SaveChangesAsync` and the Outbox write share the transaction | the page's handler, against `mysql:8` | a failure after `SaveChangesAsync`, before the commit; the same with the message deposited without the provider | `efcore-outbox` | greeting 1, Outbox 1, bus 1 / 0, 0, 0; 0, **1**, 0. The second control is what tells one transaction from two writes that both failed |

**Three claims overturned, each repaired at every recurrence (obligation 21):**

- **Ledger 37.** `AgreementDispatcher.md` #7 is now a route on an *Event*, `OrderPlaced`, whose three
  handlers are independent of each other. The page's own § *Returning Multiple Handlers* said `Send`
  throws on several handlers, and now also says the list is not a sequence, since a probe found
  `Publish` running handlers in parallel. The scaffold unit lost `ProcessOrder.Total`,
  `IsInternational` and the four `ProcessOrder` handlers, and gained `OrderPlaced` and its three
- **Ledger 38.** The two-attribute advice is the array, circuit breaker first, which the run above
  shows wrapping the retry
- **Ledger 39.** Brighter 10.8.0 and 9.33 both refuse a policy registry without
  `CommandProcessor.RETRYPOLICY` and `CIRCUITBREAKER` (`CommandProcessorBuilder.cs:168` at 10.8.0,
  `:136` at 9.33; `ServiceCollectionExtensions.cs:762`). `DefaultPolicy` holds both and exists at
  both versions. Three sites: `MigratingToPollyV8.md` #1 (the V9 form, wrong at V9 too) and #8, and
  off-group `HowConfiguringTheCommandProcessorWorks.md` #7, an S3 page, whose sentence that the keys
  *"can be overridden"* now also says Brighter requires them. That block gained its `using`s, since
  `--changed` made it strict, and stays FAILED on the page's `Globals` (phase 5)

**Measured after 4.4:** `--report` → **994 / 536 / 441 / 17**, 107 findings, unmoved: every changed
S2 block stays BUILT, and the off-group block stays FAILED. Scaffold 60 units, 0 violations.
`pagelint` 0 errors / **346** (−1, the off-group block's `using`s); `--changed origin/master` 0
errors. `linkcheck` 168 / 0; `symbolcheck` 24 entries, 0 hits on the four pages.

#### Second pass (4.5)

**Every phase-4 ledger row holds.** Rows 21–39 each give their *After* from their grep, run as
written at `0a134b5`: **0** for every row but 30 and 35, whose greps count text that is now there
(`CausationId`, 1; `where p.Name = @name`, 1). 4.1's four recurrence greps are rows 28, 32, 33 and
34, so they are among them.

**Widened, because a grep that matches one phrasing finds one instance.** Each phase-4 defect was
searched again by its shape rather than its words:

| Defect | Widened grep | Hits | Reading |
|---|---|---:|---|
| 37, several handlers for a `Send` | `Register(Async)?<\w+>\(` routes outside `AgreementDispatcher.md`, and `multiple handlers` | 14 routes on 4 pages; 1 claim | every route returns one handler. **`AgreementDispatcherRouting.md`**'s *Characteristics* said the dispatcher *"Supports multiple handlers"* of a `MyCommand` route: repaired, ledger 37 |
| 38, two `[UsePolicy]` on one method | adjacent `[UsePolicy` lines | 0 | |
| 39, `new PolicyRegistry()` | `new PolicyRegistry\b` | 3 | `QueryPipelinePolicies.md` (2), `DarkerBasicConfiguration.md`: Darker's, and each registers both `Constants` names, which the page says `AddPolicies()` requires. Not recurrences |
| 21, 23, 25–27, 31–33, 36 | `List<Guid>\|_uow\|Predicates.Field\|GetListAsync`; `BrighterContext`; `AddHedging`; `pipeline with key`; `\[Validation\b`; `routingFunc`; `.SetAttribute(\|AddEvent("`; `[0-9].(Seconds\|Minutes\|Milliseconds\|Hours)()`; `MongoDbUnitOfWork` | 0 beyond the repaired sites | `MongoDbUnitOfWork` remains only as `TransactionProvider = typeof(…)`, which is correct (D17) |

**Found beside them: a fix the prose said was unreleased.** Reading the `More than one handler`
hits, `SchedulingAMessage.md` § *Registering Handlers When You Schedule Requests* (S1) said #4414
was *"merged and not yet released"*, and told the reader to register handlers explicitly *"until it
is"*. Its fix is in the 10.8.0 tag, which the pins have carried since phase 3. Run at both versions:

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `SchedulingAMessage.md`: `AutoFromAssemblies()` and `AsyncHandlersFromAssemblies` register the scheduler's handlers twice, and a scheduled `SendAsync` then ends the process | 10.8.0, each scan, one InMemory-scheduled `SendAsync` | 10.7.0, the same | `probe/s1/scheduler-autoscan` (`-p:BV=`, `-- async`) | handled, exit 0, both scans / *"More than one handler was found … FireSchedulerRequest"*, exit 134, both scans. **The 10.7.0 claim holds; "not yet released" is overturned** (ledger 40) |

Repaired at its three sites: the section says 10.8.0 fixes it and keeps the explicit registration
for 10.7.0; `InMemoryScheduler.md` and `PostgreSQLMessageBroker.md`, which link to it, say the same.
Prose only, so no block moves. The same grep found **29** lines on **22** pages dating a defect to
10.7.0, five of whose defects have fixes in the 10.8.0 tag. That is a change of scope, put as R6 and
ruled **D19**: task 4.5a.

**Off-group term:** `AgreementDispatcherRouting.md`, `SchedulingAMessage.md`, `InMemoryScheduler.md`,
`PostgreSQLMessageBroker.md`, all prose. `pagelint` 0 errors / **346**; `--changed origin/master`
0 errors; `linkcheck` 0 on the four pages.

#### Sweep (4.5a, D19)

**Measured at `5347f53`:** `grep -rn '10\.7\.0' contents/` → **29** lines on **22** pages. Each line is
either a defect dated to 10.7.0, or a measurement labelled with it. **Two methods agree** on the
defect sites: the grep, and `grep -rl 'issues/<n>' contents/` for each issue the lines cite.

**Every defect was run at both versions**, each probe a `-p:BV=` project defaulting to 10.8.0, so
10.7.0 is the control that reproduces the page. The pin is the case:

| Claim (as the page said it at 10.7.0) | Case, 10.8.0 | Control, 10.7.0 | Probe | Result |
|---|---|---|---|---|
| #4335: a consumer-only application gets no global Inbox, so a duplicate is handled again | `AddConsumers` + `InboxConfiguration`, no `AddProducers`, the same command twice | the same; with `AddProducers`; `[UseInbox]` alone | `s5/inbox-no-producers` (`-- consumer\|producers\|attribute`) | handled 1, `OnceOnlyException` / **handled 2, no error**; handled 1; handled 1. **Fixed** (ledger 41) |
| #4437: the InMemory scheduler cannot cancel or reschedule a request the command processor scheduled | cancel and reschedule by the id `SendAsync` returns | the same; an uncancelled schedule; `scheduler.ScheduleAsync` then cancel | `s1/inmemory-cancel` | cancelled; rescheduled to the new time / **runs anyway; `ReSchedulerAsync` false**; runs; cancelled at both. **Fixed** (ledger 42) |
| `OnceOnlyAction.Replay` is *"not in a released package yet"* | a one-line use of `OnceOnlyAction.Replay` | the same | `s5/replay-released` | builds, prints `Replay` / `CS0117`. **Released** (ledger 43) |
| #4432: Decompress misses a body compressed by `WrapAsync` | GZip, Zlib, Brotli, async wrap then unwrap | the same; the sync path | `s5/compress-async` | all decompress / **all three stay compressed**; sync decompresses. **Fixed** (ledger 44) |
| #4510: no external bus, no Command Processor span | a `Send` with the tracer and no producers | the same; with `AddProducers` | `s5/tracer-no-bus` | 1 span / **0 spans**; 1 span. **Fixed** (ledger 45) |
| #4453, #4454: a monitored handler that throws loses its exception; `[MonitorAsync]` has no mapper | `Monitoring.md` #1 verbatim, a throwing handler; an async handler | the same; a handler that returns | `s5/monitor-defects` | the handler's exception; returns / **`NotSupportedException`; *"No message mapper"***; returns. **Fixed** (ledger 46) |
| #4443, #4444: the DynamoDB and Spanner Outboxes ignore `TrippedTopics` | two topics outstanding, one tripped | the same; nothing tripped | `s5/tripped-topics` (`-- dynamo\|spanner`; DynamoDB V3 package run, V4 read) | the tripped topic left out / **both returned**; both returned. **Fixed** (ledger 49) |
| `messaging.system` is always `internal_bus` on pump spans | a Reactor over Postgres | the same; the in-memory bus | `s5/messaging-system` (`-- postgres\|inmemory`) | `postgres` / **`internal_bus`**; `internal_bus` at both. **Fixed** (ledger 50) |

**Every 10.7.0 claim held at 10.7.0, and every one is false at the pin.** The pages now state the
10.8.0 behaviour and keep each 10.7.0 workaround as such: `BrighterInboxSupport.md` and the nine
inbox pages that link to it; `SchedulingAMessage.md`, `FAQ.md`, `InMemoryScheduler.md`;
`DispatcherConfigurationReference.md`; `Compression.md`; `Telemetry.md`,
`ConfiguringOpenTelemetry.md`; `Monitoring.md`; `SweeperCircuitBreaking.md`. Task 4.5's
`TutorialFirstMessage.md` was not a site: its *4437* is inside a log event id.

**What the runs hid** (as phase 3's hard rows hid falsehoods behind a compile):
- **Ledger 47.** `Monitoring.md` #1 registered the control-bus sender as `IAmAControlBusSender`
  only, and `MonitorHandlerAsync` takes `IAmAControlBusSenderAsync`, so `[MonitorAsync]` fails at
  **both** versions with `ConfigurationException` ← *"Unable to resolve service for type
  'Paramore.Brighter.IAmAControlBusSenderAsync'"*. The block registers both, which
  `s5/monitor-defects`'s registered row runs; it stays BUILT
- **Ledger 48.** 10.8.0 added `InstrumentationOptions.CircuitBreaker` and put it in `All`
  (`InstrumentationOptions.cs:43–44`), so `Telemetry.md`'s *"`All` | Every flag above"* was short a
  flag. The table gains its row, read from `BrighterTracer.CreateCircuitBreakerSpan`; not run

**Measurements, re-derived rather than relabelled:**
- `Telemetry.md`'s `RequestContext` row and § *Adding Custom Span Attributes*: still true at 10.8.0.
  `InstrumentationOptions.RequestContext` appears only in `///` comments at both tags, and
  `BrighterTracer.cs` reads no request context at either. Both relabelled 10.8.0
- `ConfiguringOpenTelemetry.md`: the assembly version is `10.0.0.0` at 10.8.0, printed by every
  probe above. Relabelled
- `RelationalDatabaseConfigurationReference.md`'s seventeen components: the 10.8.0 tag adds two
  packages that name the type, `Extensions.DependencyInjection` (the hosted services that resolve
  it) and `MessagingGateway.MsSql` (already a row). Seventeen, relabelled
- `BrighterControlAPI.md`'s sample response carries `"executingAssemblyVersion": "10.7.0+…"`: a
  captured response, true as captured. Unchanged
- `PostgresOutbox.md`'s DDL, *"against Brighter V10 (10.7.0)"*: **not true at 10.8.0**, whose
  relational builders all add `CausationId` (and the Outboxes an index). Put as R7, below

**Ruling asked** (obligation 21, a change of scope):

| # | Block | Question |
|---:|---|---|
| R7 | the Outbox DDL on `MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md`, `SpannerOutbox.md`, `SqliteOutbox.md`, the Inbox DDL on `SpannerInbox.md`, and `TutorialDurableOutbox.md`'s table (7 blocks) | Each shows its builder's 10.7.0 output. At 10.8.0 every relational builder adds `CausationId`, and the Outbox builders a causation index. A 10.7.0 table still deposits (the stores probe for the column), but Replay needs it. Regenerate each from 10.8.0's `GetDDL`, run against its database, and say on each page that Replay needs the column? |

**Ruled 2026-10-18, as recommended:** R7 is design **D20**, task 4.5b.

**Measured after 4.5a:** `--report` → **994 / 536 / 441 / 17**, 107 findings, unmoved: the one block
changed, `Monitoring.md` #1, stays BUILT. `pagelint` 0 errors / **346**; `--changed origin/master`
0 errors. `linkcheck` 168 / 0. `grep -rn '10\.7\.0' contents/` → **15**, every one a 10.7.0 workaround
or history beside the 10.8.0 statement, the captured response, or R7's DDL.

#### The relational DDL (4.5b, D20)

**Each block against its builder**, `probe/s5/ddl-refresh` (`gen DIR` writes each `GetDDL` the page
names; `run` creates each table under a unique name on its database, probes the column, drops it):

| Page | The block against 10.7.0's `GetDDL` | Created at 10.8.0 / 10.7.0 (the control) |
|---|---|---|
| `PostgresOutbox.md` | identical, but for whitespace | `CausationId` present / absent |
| `SpannerOutbox.md` | identical, but for whitespace | present / absent; `GetCausationIndexDDL` exists / does not |
| `SpannerInbox.md` | identical, but for whitespace | present / absent |
| `MSSQLOutbox.md` | **not the builder's**: `UNIQUEIDENTIFIER` ids, `DATETIME2`, a `CreatedID` identity and `PK_Outbox`, none of which any 10.x `SqlOutboxBuilder` writes | present / absent |
| `MySQLOutbox.md` | **not the builder's**: no `IF NOT EXISTS`, `PartitionKey` 255 not 128, no `DataRef` or `SpecVersion` | present / absent |
| `SqliteOutbox.md` | **not the builder's**, and not valid SQLite: a `PRIMARY KEY AUTOINCREMENT` column beside a `PRIMARY KEY` constraint | present / absent |

**Said and measured.** Three pages said *"Running `…GetDDL("Outbox")` will generate the following"*
over DDL no release generates; the other three were 10.7.0's output. The Spanner probe first
reported the column present at **both** versions: Spanner's reader is lazy, so a missing column
throws at `Read()`, not `ExecuteReader()`. Fixed, it reads; the control then failed as it should.

**Repaired:** each block is 10.8.0's output, verbatim, and each page says `CausationId` arrived at
10.8.0, that an older table still deposits, and that Replay needs it, linking
`ReplayOnSeenReference.md` § *Store Support*. `SpannerOutbox.md` gains
`GetCausationIndexDDL`'s statement, which `run` creates; the Spanner pages say the provisioner adds
the column only on a fresh install, as `ReplayOnSeenReference.md` already did. `TutorialDurableOutbox.md`
was not a site: its *CREATE TABLE* is a comment, and the tutorial provisions the table.
SQL blocks, so `blockcheck` does not move. `pagelint` 0 errors on the six; `linkcheck` 0.

#### Phase 4 closed (4.6)

**Baselined.** `baseline.tsv` lists every BUILT block on the ten S2 pages, **109** rows, replacing
their **3** earlier ones (§ the file's own header says how). None keeps its ref: the three kept their
text, but each one's scaffold unit changed in the phase (`MigratingToPollyV8Context.cs`,
`PolicyRetryAndCircuitBreakerContext.cs`; `UsingTheContextBag.md` #12 gained its page's unit). Every
row is at **`6ad8b0a`**. Off-group, `Monitoring.md` #1 moves there too, re-measured after ledger 47;
#2, unchanged, keeps `f2ce226`. **Two methods agree:** the rows, `grep -v '^#' baseline.tsv | grep
-vc '^$'` → **536**, and `--report`'s BUILT → **536**. `--report` exit **0**, **0 findings**.

**Rule 8.** `pagelint` → **0 errors**; the one `attr-mismatch-intended` marker stays on
`PipelineValidation.md` #7, and `--plant` → 5 / 5.

**Readings (obligation 19).** `git diff -U0 5f2294c HEAD -- contents`, hunks against fence ranges:
42 pages, **79** changed C# blocks on 16 of them.
- **AC9.** The phase added `// ...` at **8** lines in **7** blocks: `AgreementDispatcher.md` #11,
  #12; `PipelineValidation.md` #12, #15, #16 (two); `PolicyFallback.md` #9; `UsingTheContextBag.md`
  #12. All 7 are BUILT, so `--explain` has nothing to report for any of them: no added omission
  hides an error
- **AC10.** § *Behaviour (4.4)*: fifteen claims run with a control, two cited. § *Second pass
  (4.5)*, *Sweep (4.5a)* and *The relational DDL (4.5b)*: ten more, each at 10.7.0 and 10.8.0. The
  six changed C# blocks off S2 are each run or assert no behaviour: `EFCoreOutbox.md` #2
  (`efcore-outbox`), `MongoDBOutbox.md` #5 (`s5/mongo-outbox`),
  `HowConfiguringTheCommandProcessorWorks.md` #7 (`legacy-policy-order`), `Monitoring.md` #1
  (`s5/monitor-defects`); `BrighterOutboxSupport.md` #1 and `CloudEventsSupport.md` #5 are API
  repairs (ledger 21, 32). Read and named, not run: `Telemetry.md`'s `CircuitBreaker` row (ledger 48)

**Gates against 4.1.**

| Gate | Predicted (4.1) | Measured at the close | |
|---|---|---|---|
| corpus | 993–1,000 | **994** | in band: one split, `PipelineValidation.md` #3; #13 needed none |
| BUILT | 522–537 | **536** | in band |
| FAILED | 438–450 | **441** | in band |
| SKIPPED | 17–21 | **17** | the floor: no unlabelled V9 form was found |
| `pagelint` | 0 errors / 331–345 | **0 / 346** | **above the band by 1** (below) |
| `symbolcheck` | unmoved | **24, 14**, exit 0 | as predicted |
| scaffold rule | 52–62 units | **60**, 0 violations, 0 compile errors | in band |
| assemblies | 552–553 | **553** on each pin | in band, by D13 rather than the mechanism named (§ *The repairs (4.3)*) |
| `linkcheck` · shape · redirects · `--verify` | unmoved | 168 / 0 · 161, widest 12 of 20 · 77, 7858 bytes · 161 = 161 | as predicted |
| `versioncheck` · `optioncheck` | unmoved | 0 of 18 · 0 / 59 / 529 | as predicted |
| `pagelint --plant` | unmoved | 5 / 5 | as predicted |

**Why `pagelint` came in above the band.** 4.1 expected S2's **79** warnings to fall as blocks gained
their `using`s, less the declared omissions. They fell to **19**, read block by block against the diff:
- **7 declared `// ...`**, as predicted: `AgreementDispatcher.md` #10–#12, `UsingTheContextBag.md`
  #12, and `MigratingToPollyV8.md` #1, #3, #5, SKIPPED V9 forms
- **11 BUILT blocks no repair touched**, which build on their page's scaffold alone and so never
  gained a `using`: `AgreementDispatcher.md` #2–#5, #8, #13; `AsyncDispatchARequest.md` #2, #3;
  `DispatchingARequest.md` #5–#7
- **1 SKIPPED V9 form, untouched**: `PolicyFallback.md` #2

The band assumed every S2 block would be edited. Off-group, −4 rather than −0–1, from 4.3's and 4.4's
off-group repairs (`CloudEventsSupport.md`, `EFCoreOutbox.md`,
`HowConfiguringTheCommandProcessorWorks.md`, `MongoDBOutbox.md`). 79 − 19 + 4 = **64**, and 410 − 64 =
**346**. The number is not adopted: the next phase predicts a block's `using`s only for the blocks its
repairs touch.

**Off the prediction.** 4.5a and 4.5b were added by D19 and D20 after 4.1 and moved no gate: prose and
SQL, and one C# block that was already BUILT. `tools/README.md` rows 2 and 9 now give the close's
figures, and its 552 is 553 (D13).

### Phase 5 — S3, Darker; Understanding Brighter *(6 tasks, one PR)*

- [x] **Task 5.1:** Predict phase 5's gate movement
  - Input: § *The groups*, S3; recurrence greps for S3's defects
  - Output: § *Phase 5 as executed*, *Predicted*, as 3.1's
- [x] **Task 5.2:** Repair S3's reachable blocks
  - Input: S3's table, reachable rows
  - Output: as 3.2's, for S3
- [x] **Task 5.3:** Repair S3's hard blocks
  - Input: S3's table, hard rows; Darker `4.1.1` and Brighter `10.8.0` source, the pinned releases
  - Output: as 3.3's, for S3. `QueryPipeline.md`'s 12 *parse* blocks are each made whole, skipped
    with an accepted reason, or listed
- [x] **Task 5.4:** Run S3's behavioural claims (AC10)
  - Input: S3's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s3/<name>/Program.cs`
- [x] **Task 5.5:** Second pass for S3
  - Input: 5.1's recurrences; rulings asked
  - Output: as 3.6's
- [x] **Task 5.6:** Close phase 5
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 5 as executed

**Measured at phase start, `0fc79a3`** (#203's merge). `--report` → **994 / 536 / 441 / 17, 0 findings**,
exit 0; `--classify` → 97 parse, 26 built-by-using, 71 defect, 29 same-page, 25 values, 193 page-type,
settled in 3 rounds. 553 reference assemblies on each pin (`--report`'s own scope lines); scaffold rule
60 units. `pagelint` 0 errors / **346** warnings; `symbolcheck` 24 entries, 14 silenced; `linkcheck`
168 / 0; `versioncheck` 0 of 18.

#### Predicted (5.1)

**S3 is identical row for row** to § *The groups*: `groups.py` at `0fc79a3` gives 10 pages, **137**
FAILED (31 parse, 7 built-by-using, 16 defect, 5 same-page, 7 values, 71 page-type), 8 BUILT,
6 with nothing BUILT. Two methods: the FAILED column, 137; `grep` over `--report` on the ten pages,
137, with 8 BUILT and 1 SKIPPED (`ReactorAndProactor.md` #11). The S3 pages hold **146** C# blocks.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| Task 5.3: *"Brighter `10.7.0` source"* | the pins are 10.8.0 | the task says 10.8.0 |
| S3's table: **16 defect** blocks | the second compile's diagnostics (below): **5** are API defects, **2** are a missing `using` the classifier misread, **6** are fragments or alternatives in one fence, and **3** are misreads | the table stands, being the instrument's; the repairs follow the reading |

**Reading S3's 16 defect blocks** with `probe/second.py` over the ten pages (600 diagnostic lines),
each against the 10.8.0 and 4.1.1 tags:

| Page | # | Second compile | Reading against the pins | Repair |
|---|---:|---|---|---|
| `HowConfiguringTheCommandProcessorWorks.md` | 3 | `CS0535` `CreatePipelineScope()`; `CS0246` `TinyIoCContainer` | **defect.** A V9 handler factory: `Create(Type)` and `Release(IHandleRequests)`. At 10.8.0 `IAmAHandlerFactory` declares only `CreatePipelineScope()`, and `IAmAHandlerFactorySync` adds `Create(Type, IAmALifetime)` and `Release(IHandleRequests, IAmALifetime)`. The prose before it names `IAmAHandlerFactory` | 5.3, the V10 interface, in the block and the prose |
| `HowServiceActivatorWorks.md` | 7 | `CS1739` `deadLetterChannelName` on `Subscription` | **defect.** `Subscription<T>` has no dead-letter parameters at 10.8.0; only `RmqSubscription` (both RMQ packages) takes `deadLetterChannelName` and `deadLetterRoutingKey`, as `ErrorHandlingOptions.md` already says | 5.3, `RmqSubscription<T>`, saying dead letters are set per transport |
| `HowServiceActivatorWorks.md` | 13 | `CS0117` `StopChannel`, `StartChannel`; `CS7036` `subscriptionName` | **defect.** `ConfigurationCommandType` is `CM_STOPALL`, `CM_STARTALL`, `CM_STOPCHANNEL`, `CM_STARTCHANNEL` (`ConfigurationCommand.cs:30`), and the constructor takes `(ConfigurationCommandType, SubscriptionName)` (`:63`) | 5.3 |
| `ImplementAQueryHandler.md` | 13 | `CS0535` `IQueryHandler.Context`, `Fallback`, `FallbackAsync` | **defect.** At Darker 4.1.1 `IQueryHandler<TQuery, TResult>` declares `Execute`, `Fallback`, `ExecuteAsync` and `FallbackAsync`, and `IQueryHandler` a `Context`; the class implements two of the five | 5.3, every member, or the page's advice changed; judged at the repair |
| `DarkerBasicConfiguration.md` | 9 | `CS1061` `AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`; `CS1929` `IsDevelopment` | **an API the pin does not carry**: Swashbuckle, which the page never names and which has nothing to do with Darker. `CS1929` is a misread: without `Microsoft.Extensions.Hosting` the classifier found the obsolete `IHostingEnvironment` overload | 5.3: the Swagger lines removed, or the package named and pinned; judged at the repair |
| `CQRSWithBrighterAndDarker.md` | 4 | `CS1501` `GetConnectionString` takes 1 argument | **a missing `using`**, misread as a defect. Without `Microsoft.Extensions.Configuration`, the only `GetConnectionString` in scope is EF Core's `RelationalDatabaseFacadeExtensions.GetConnectionString(this DatabaseFacade)` | 5.2, the `using` |
| `DarkerBasicConfiguration.md` | 10 | `CS1501`, as above | as above | 5.2, the `using` |
| `CQRSWithBrighterAndDarker.md` | 1 | `CS0111` `GetCustomer` | **alternatives** in one fence: a *Bad* and a *Good* `GetCustomer`, as members with no class | 5.3, split or made whole |
| `HowServiceActivatorWorks.md` | 1 | `CS0128` `message`; `CS0103` `channel`, `timeOut` | **alternatives**: a Reactor and a Proactor receive, each declaring `message`. `Receive(TimeSpan?)` and `ReceiveAsync(TimeSpan?, CancellationToken)` match 10.8.0 (`IAmAChannelSync.cs:55`, `IAmAChannelAsync.cs:59`) | 5.3, split or made whole |
| `QueryPatterns.md` | 2 | `CS0128` `orders` ×2 | **alternatives**: one *Bad* and two *Good* queries, each declaring `orders` | 5.3, split or made whole |
| `QueryPatterns.md` | 3 | `CS0111` `ExecuteAsync` | **alternatives**: a *Good* and a *Bad* `ExecuteAsync` | 5.3, split or made whole |
| `QueriesAndQueryObjects.md` | 8 | `CS0116`, `CS0825` | a fragment: a type declaration followed by its *Usage* statements | 5.3, made whole |
| `QueryPipeline.md` | 8 | `CS0161` | a fragment: a method whose body is a comment. `RetryableQuery(int step, string policyName)` matches 4.1.1 (`RetryableQueryAttribute.cs:11`) | 5.3, a body |
| `CQRSWithBrighterAndDarker.md` | 10 | `CS1061` `'TEntity'` has no `Id` | **a misread**: `ApplicationDbContext` and the DTOs are the reader's, so `Orders` resolved to nothing. *page-type* | 5.2, stubs |
| `DarkerBasicConfiguration.md` | 5 | `CS0029` `TResult` to `int` | **a misread**: with `GetCountQuery` undeclared, `Execute` cannot infer its result. *page-type* | 5.2, a stub `GetCountQuery : IQuery<int>` |
| `QueryPatterns.md` | 5 | `CS1955` `Ok` | **a misread**: a controller action outside its controller, as `ShowMeTheCode.md` #1 was. *page-type* | 5.2 |

**Recurrence greps (obligation 20), run as written at `0fc79a3`:**

| Defect | Grep | Hits | Off-group |
|---|---|---:|---|
| V9 handler factory | `grep -rnE 'IHandleRequests\??\s+Create\(Type \w+\)\|void Release\(IHandleRequests \w+\)\|: IAmAHandlerFactory(\s\|$\|,\|\{)' contents/` | **3** lines in 1 block (`HowConfiguringTheCommandProcessorWorks.md` #3) | none |
| `deadLetterChannelName` off `RmqSubscription` | `grep -rn deadLetterChannelName contents/` | **4**: 2 are `ErrorHandlingOptions.md` and `RabbitMQConfiguration.md` prose on `RmqSubscription`, which is right | none. On-group: `HowServiceActivatorWorks.md` #7, and **#15**, a lone argument under *Use Dead Letter Queues* (*parse*) |
| `ConfigurationCommandType` members | `grep -rnE 'ConfigurationCommandType\.\w+' contents/`; `grep -rn 'new ConfigurationCommand(' contents/` | **2**; **2**, all `HowServiceActivatorWorks.md` #13 | none |
| `IQueryHandler<,>` implemented directly | `grep -rnE ': IQueryHandler<' contents/` | **1** (`ImplementAQueryHandler.md` #13) | none |
| Swashbuckle unpinned | `grep -rnE 'AddSwaggerGen\|UseSwagger' contents/` | **3** lines in 1 block (`DarkerBasicConfiguration.md` #9) | none |

**The off-group term is zero.** No S3 defect recurs on a page outside S3.

**`pagelint`, block by block** (phase 4's lesson: predict a block's `using`s only where a repair
touches it). S3's **87** warnings (4 + 2 + 5 + 16 + 12 + 19 + 4 + 14 + 11 on nine pages, `--per-page`;
`DarkerBasicConfiguration.md` has none), read against `--report` and `--classify`:
- **2 on blocks no repair touches**: `QueriesAndQueryObjects.md` #1 (BUILT) and `ReactorAndProactor.md`
  #11 (SKIPPED). They stay
- **10 on FAILED blocks that already declare `// ...`**: `EFCoreQueryIntegration.md` #1, #3;
  `HowServiceActivatorWorks.md` #7, #10; `QueryPipeline.md` #12, #16, #17, #22, #23;
  `ReactorAndProactor.md` #12. Each keeps its warning unless its repair gives it `using`s
- **7 on FAILED blocks whose first compile needed no `using`** (`page-type` or `values` at both
  compiles): `HowServiceActivatorWorks.md` #2, #3, #6, #9; `ImplementAQueryHandler.md` #15;
  `QueryPipeline.md` #18, #19. A stub alone may build them, and they then keep their warning, as
  phase 4's eleven did
- **68 on the other FAILED blocks** (23 *parse*, 45 needing a `using`), which their repairs edit

**Predicted:**

| Gate | At `0fc79a3` | Predicted at the close | Mechanism |
|---|---|---|---|
| corpus | 994 | **994–1,000** | splits only, where a fence holds alternatives: `CQRSWithBrighterAndDarker.md` #1, `HowServiceActivatorWorks.md` #1, `QueryPatterns.md` #2 (three) and #3, `QueriesAndQueryObjects.md` #8, up to +6. Each may instead be made whole in one fence |
| BUILT | 536 | **662–676** | 673, plus splits (0–6), less what stays FAILED (3–8) and any SKIPPED (0–3). S3's 146 blocks less the SKIPPED one, all BUILT, would be 673. The 5 *same-page* blocks (`CQRSWithBrighterAndDarker.md` #2, #8; `ImplementAQueryHandler.md` #14; `QueriesAndQueryObjects.md` #22, #23) are the likely stays; phases 3 and 4 kept 6 of their 236 FAILED (§ *Blocks that stay FAILED*) |
| FAILED | 441 | **307–312** | 304 (−137 for S3) plus what stays FAILED and listed; off-group 0 |
| SKIPPED | 17 | **17–20** | a V9 form found unlabelled on an S3 page, labelled as such; `HowConfiguringTheCommandProcessorWorks.md` is the page that shows V9-era code |
| `pagelint` | 0 errors / 346 | **0 errors / 266–281** | 346 less 65–80: of the 85 warnings on FAILED blocks, 5–20 stay (the 10 declared `// ...`, the 7 a stub may build untouched, and any repaired fragment that keeps an omission without `using`s). Off-group 0 |
| `symbolcheck` | 24 entries, 14 silenced | **unmoved** | no S3 page holds a watchlisted name; whether `IHandleRequests Create(Type)` or `ConfigurationCommandType.StopChannel` earn a row is read at 5.5 |
| scaffold rule | 60 units | **60–69 units**, 0 violations, 0 compile errors | a unit per S3 page that needs a stub; `HowConfiguringTheCommandProcessorWorks.md` has one, and `QueryPatterns.md` may share `DarkerQueryPatternsContext` |
| assemblies | 553 | **553–554** | unmoved, unless `DarkerBasicConfiguration.md` #9's repair pins Swashbuckle |
| `linkcheck` · `versioncheck` · `pagelint --plant` | 168 / 0 · 0 of 18 · 5 / 5 | **unmoved** | |

#### The repairs (5.2)

**S3's reachable rows, 85 blocks** (7 built-by-using, 7 values, 71 page-type), plus the five 5.1 read
as misreads (`CQRSWithBrighterAndDarker.md` #4, #10; `DarkerBasicConfiguration.md` #5, #10;
`QueryPatterns.md` #5). **72 of the 85 build**, and 13 do not: 8 turned out *same-page* (below) and 5
wait on a ruling (R8). Of the misreads, `DarkerBasicConfiguration.md` #5 and #10 build; the other three
are *same-page*. Three blocks whose page was otherwise whole were repaired with it, as 4.2 repaired
`AgreementDispatcher.md` #7: `DarkerBasicConfiguration.md` #9 (ledger 52) and
`ImplementAQueryHandler.md` #13 (ledger 55) build, and `QueriesAndQueryObjects.md` #22 and #23, classed
*same-page* because block 1 lists Darker's `IQuery`, needed only `using Paramore.Darker;` and build.
Seven scaffold units are new (`DarkerBasicConfiguration`, `EFCoreQueryIntegration`,
`HowServiceActivatorWorks`, `ImplementAQueryHandler`, `QueriesAndQueryObjects`, `QueryPipeline`,
`ReactorAndProactor`) and two extended (`HowConfiguringTheCommandProcessorWorks`, and
`DarkerQueryPatterns`, which now also serves `QueryPatterns.md`), with **0** unit-rule violations.

**Measured after 5.2:** `--report` → **994 / 614 / 363 / 17**; 83 findings, all expected: 78 BUILT
blocks not yet baselined, and 5 BUILT blocks whose page gained a unit (`ImplementAQueryHandler.md`
#1, #2; `QueriesAndQueryObjects.md` #1–#3). 67 units. `pagelint` 0 errors / **304**; `--changed
origin/master` 0 errors. `linkcheck` 168 / 0. `symbolcheck` 24 / 14 silenced.

**Why eight *page-type* rows are *same-page*.** The classifier calls a block *same-page* only when
every name it lacks is declared on the page. A block lacking one page-declared type and one
undeclared one is *page-type*, and stays FAILED all the same, since no stub may supply a type
another block declares (unit rule 2). `ImplementAQueryHandler.md` #3, #5, #6, #16, #21, #23 and
`CQRSWithBrighterAndDarker.md` #3, #5 are that case. With the three misreads that are *same-page*,
the three original *same-page* rows still FAILED (`CQRSWithBrighterAndDarker.md` #2, #8;
`ImplementAQueryHandler.md` #14) and R8's five, **19** S3 blocks now look unlikely to build, against
5.1's 3–8. 5.6 reconciles it.

**What the reachable rows hid.** As in phases 3 and 4, a block that did not compile had not been
checked for anything else:
- **The message pump's own calls (ledger 53).** `HowServiceActivatorWorks.md` § *Message Flow* quotes
  the pump's private methods. Read against `Reactor.cs` and `Proactor.cs` at 10.8.0, four of five
  blocks were wrong. The section now quotes each call as the pump makes it, with its file and line, and
  puts *Request Context Creation* before *Message Deserialization*, the order the pump runs them in.
  No link named either anchor (`grep -rn 'HowServiceActivatorWorks.md#' contents/ SUMMARY.md` → 0)
- **A router's return type (ledger 54).** `HowServiceActivatorWorks.md` #9 returned `Type[]`; it now
  uses collection expressions, as `AgreementDispatcher.md` does
- **Darker's interfaces (ledger 55).** `ImplementAQueryHandler.md` #12 listed `IQueryHandler<,>` as two
  methods, and #13 implemented the listing, which is why it failed. Both now follow 4.1.1, and the page
  says the base classes supply `Fallback` and `FallbackAsync`. `QueriesAndQueryObjects.md` #1, already
  BUILT, lacked `IQuery<TResult>`'s base
- **Dependencies never declared.** `ImplementAQueryHandler.md` #10, #11, #17–#21, #23 and
  `ReactorAndProactor.md` #1, #4 used a `_repository` no block declared. Each now takes its repository
  by primary constructor (phase 3's rule). One static stub could not have served them: `GetByIdAsync`
  returns a customer in one block and an order in another
- `QueryPatterns.md` #5, a controller action shown alone, is made whole in its controller, as
  `ShowMeTheCode.md` #1 was (D15). It is *same-page* on `GetProductCatalogQuery`

**For 5.3:** `HowServiceActivatorWorks.md` #15 is ledger 53's neighbour, not its recurrence: a lone
`deadLetterChannelName:` argument, repaired with #7. `ImplementAQueryHandler.md` #22 declares an
`UnauthorizedAccessException` that shadows the BCL's, and #16 registers a `GetPersonQueryHandler` no
block declares; both are read at 5.5. **For 5.4:** the page now says the base classes' `Fallback`
returns `default(TResult)` when the `[FallbackPolicy]` decorator calls it, a claim to run.

**Ruling asked** (obligation 21: no accepted skip reason fits), and **ruled 2026-10-20 as D21**, below the table:

| # | Block | Question |
|---:|---|---|
| R8 | `HowServiceActivatorWorks.md` #1–#6 | Each quotes one line of the Reactor or Proactor message loop (#1, added at 5.3, its two `Receive` calls), which call private methods (`InitRequestContext`, `TranslateMessage`, `InvokeDispatchRequest`). The Reactor and Proactor methods take the same parameters and differ only in return type, so no unit can stub both, and no reader can call them. The lines are now 10.8.0's. Accept a fourth skip reason, *"a line quoted from Brighter's source, calling a private member"*, with the file and line cited in the block? Or retag them `text`, or list them as FAILED? |

**R8 ruled (D21).** A fourth accepted skip reason, widened from the one asked: *"a line quoted from Brighter's source, calling a private member or reading the pump's own state"*, with the file and line cited in the block. The question was wrong about #1: `Channel.Receive` is public, and #1 fails because the pump's `Channel` is an `IAmAChannelSync` in the Reactor and an `IAmAChannelAsync` in the Proactor, so the reason was widened to cover it. All eight cited lines were read at the 10.8.0 tag and match; #2, which cited its lines only in the prose, now cites them in the block. Each block carries its marker. S3 after the ruling: **114 BUILT, 19 FAILED** (all *same-page*), **11 SKIPPED**

#### The repairs (5.3)

**S3's hard rows, 40 blocks** (31 *parse* and the 9 *defects* 5.2 left), counted as they stood at
5.2. Every one is made whole, skipped with an accepted reason, retagged, or listed:
- **Made whole, BUILT: 26.** `QueryPipeline.md`'s 13 (#8, #12–#17, #20–#25, every *parse* block the
  task names) now build on one running example, a `GetOrderQuery` answered through an external API:
  attribute lists gain the method they decorate, #14's two policies their two handlers, #15's delays
  their policy, #21's registration its registry and `AddPolicies`. `EFCoreQueryIntegration.md` #1–#3,
  whose `(...)` parameters became the real ones. `HowConfiguringTheCommandProcessorWorks.md` #2 (a
  missing `;`) and #3 (ledger 56). `HowServiceActivatorWorks.md` #7 (ledger 59), #11 (ledger 57), #13
  (ledger 58), #14. `QueriesAndQueryObjects.md` #8, its usage moved above the type it creates.
  `QueryPatterns.md` #1–#3, its alternatives given distinct names
- **Split: 3**, each a ✅ and ❌ pair that cannot share a wrapper (§ *Splits*):
  `CQRSWithBrighterAndDarker.md` #1, `EFCoreQueryIntegration.md` #1, `QueryPatterns.md` #3
- **Skipped: 4**, each with an accepted reason. `ImplementAQueryHandler.md` #4, #8: *a method signature
  shown for comparison*. `HowServiceActivatorWorks.md` #15, #17: *a fragment of the configuration shown
  whole in block* 7 and 10, which the prose now links
- **Retagged `text`: 5.** `QueriesAndQueryObjects.md` #13–#17 were lists of query names
- **Made whole, still FAILED: 5**, six blocks after the split, each *same-page* or R8's and listed: `CQRSWithBrighterAndDarker.md` #1, #2
  (old #1) and #12–#14 (old #11–#13, whose bodies lacked a `return`); `HowServiceActivatorWorks.md`
  #1 joins R8

**Measured after 5.3:** `--report` → **992 / 642 / 329 / 21**; 113 findings, all expected: 107 BUILT
blocks not yet baselined, 5 whose page gained a unit, and `CQRSWithBrighterAndDarker.md` #6
*"stopped building"*, which is the split moving old #6 (BUILT, now #7) off its ordinal. 67 units,
0 violations. S3: **114 BUILT, 25 FAILED** (19 *same-page*, 6 R8), **5 SKIPPED**.

**Said and measured, at 5.2 and corrected here.** 5.2 wrote on `ImplementAQueryHandler.md` that the
base classes' `Fallback` and `FallbackAsync` return `default(TResult)`. At 4.1.1 each base class
returns it only for its own mode: `QueryHandler.Fallback` and `QueryHandlerAsync.FallbackAsync`; the
other mode's members throw `NotImplementedException`. The page now says so, and 5.4 runs it.

**What the hard rows hid**, besides ledger 56–59:
- **Pitfall 4 (ledger 60).** `QueryPipeline.md` said a `[FallbackPolicy]` handler without
  `FallbackAsync` fails at runtime. `QueryHandlerAsync`'s own returns `default(TResult)`, so the
  caller gets `null` and no exception. The pitfall now says that; 5.4 runs it with a control
- **A unit-rule edge.** `EFCoreQueryIntegration.md` never named the entity behind `Orders`, and a
  `DbSet<T>` needs a class, so no stub could satisfy rule 1. Block 3 now says what it loads (*"each
  Order with its Customer, and each OrderItem with its Product"*), which names the entity for the
  reader and the rule alike. `Customers` keeps 5.2's stand-in, the projection's DTO

**For 5.4:** pitfall 4 and the base classes' fallbacks (above); 5.2's `ImplementAQueryHandler.md`
interface claims are read, not run; the Dispatcher reading `AddConsumers`' instrumentation (ledger 57)
is read from source and worth a run; `QueryPipeline.md`'s pitfalls 1 (decorator order), 2 (no
policies) and 3 (an unknown policy name throws `ConfigurationException`) predate the phase and each
asserts behaviour.

#### Behaviour (5.4, AC10)

**Every run is against the released packages**, Darker **4.1.1** and Brighter **10.8.0**, as
`probe/s3/<name>/` projects; each prints `ok` or `FAIL` per row and exits 1 on any `FAIL`. Each block
is copied as the page shows it, the probe's own lines marked `// probe`. `darker-pipeline` runs every
row in one process, its containers sharing one `ILoggerFactory`, since Darker's decorators cache a
static logger per generic type. `dispatcher-instrumentation` runs one process per case.

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `QueryPipeline.md` pitfall 1: retry at a lower step than fallback puts the fallback inside it, so nothing is retried | `[RetryableQuery(1)] [FallbackPolicy(2)]`, an API that always fails, the default policies | `[FallbackPolicy(1)] [RetryableQuery(2)]` | `darker-pipeline` | 1 attempt, the fallback's value / 4 attempts, then the fallback's |
| `QueryPipeline.md` § *Decorator Ordering*: logging at step 1 logs once, the completion marked *(with fallback)*, and no retries | `[QueryLogging(1)] [FallbackPolicy(2)] [RetryableQuery(3)]`, always failing | the same handler succeeding | `darker-pipeline` | 4 attempts, 1 *Executing*, 1 completion *(with fallback)* / 1 completion, unmarked |
| the same: a query that fails after its last retry logs only its *Executing* line | `[QueryLogging(1)] [RetryableQuery(2)]`, always failing | — (the row above's completion is the control) | `darker-pipeline` | 4 attempts, 1 *Executing*, 0 completions, the exception |
| the same: logging inside retry logs every attempt, and a completion only for the one that succeeds | `[RetryableQuery(1)] [QueryLogging(2)]`, failing twice | the rows above, logging outside | `darker-pipeline` | 3 attempts, 3 *Executing*, 1 completion |
| `QueryPipeline.md` pitfall 2: `[RetryableQuery]` with no policies *"will cause runtime errors"* | no `AddDefaultPolicies()` or `AddPolicies()` | `AddDefaultPolicies()` | `darker-pipeline` | the processor builds; the first query throws `MissingHandlerDecoratorException`, *"Handler decorator could not be created for type: …RetryableQueryDecorator…"* / runs. True, and now named |
| `QueryPipeline.md` pitfall 3: an unknown policy name throws `ConfigurationException` the first time the handler runs | `[RetryableQuery(2, "MyCircuitBreaker")]`, the default registry | the registry holding the name | `darker-pipeline` | the processor builds; the first query throws *"Policy does not exist in policy registry: MyCircuitBreaker"* / runs |
| `QueryPipeline.md` pitfall 3's ✅ block: a registry holding only `"MyCircuitBreaker"`, passed to `AddPolicies` | the block as it was | the block as repaired, with the two defaults beside the name | `darker-pipeline` | `ConfigurationException` at `AddPolicies`, *"The policy registry is missing the Darker.RetryPolicy policy which is required"* / runs. **Overturned** (ledger 61) |
| `QueryPipeline.md` pitfall 4 (ledger 60): `[FallbackPolicy]` without `FallbackAsync` answers with `default(TResult)`, no exception | the ❌ handler | `FallbackAsync` overridden; no `[FallbackPolicy]` | `darker-pipeline` | `null` / the override's value; the exception reaches the caller |
| `ImplementAQueryHandler.md`: each base class's own-mode fallback returns `default(TResult)`; the other mode's `Execute` and fallback throw `NotImplementedException` | `QueryHandler`: `Fallback`, `FallbackAsync`, `ExecuteAsync`; `QueryHandlerAsync`: `FallbackAsync`, `Fallback`, `Execute` | each against the other mode's member | `darker-pipeline` | `default`, `NotImplementedException` ×2, on each base class; and `[FallbackPolicy]` on a throwing `QueryHandler`, through `Execute`, returns `null` |
| `ImplementAQueryHandler.md` #13: `IQueryHandler<TQuery, TResult>` and `IQueryHandler` have five members; `QueriesAndQueryObjects.md`: `IQuery` and `IQuery<TResult>` have none | reflection over the 4.1.1 assemblies | the same reflection over `IQueryHandler`, which finds its member | `darker-pipeline` | `Context`, `Execute`, `ExecuteAsync`, `Fallback`, `FallbackAsync`; 0 members, `IQuery<T> : IQuery` / 3 |
| `HowServiceActivatorWorks.md` #11 (ledger 57): the Dispatcher reads its instrumentation from `AddConsumers`' options | `AddConsumers(All)` alone, a message enqueued on the internal bus | `AddConsumers(RequestInformation)` alone | `dispatcher-instrumentation` | the pump's process span carries `messaging.message.body` / it does not |
| `HowServiceActivatorWorks.md`, the sentence before #11: *"The Command Processor takes its own, separately, from `AddBrighter`'s options"* | `AddConsumers(All)` alone | `AddBrighter(All).Handlers(…)`, then `AddConsumers(RequestInformation)`, the shape of five transport pages' #1 | `dispatcher-instrumentation` | the Command Processor's span under the pump carries `paramore.brighter.request.body`: `AddConsumers`' options configure both / the Command Processor takes `AddBrighter`'s, the pump `AddConsumers`', the handler runs. **Overturned** for an app that calls `AddConsumers` alone (ledger 62) |
| unstated, found by the control: handlers registered on `AddConsumers`' builder in an app that called `AddBrighter` first | `AddBrighter(All)`, then `AddConsumers(…).Handlers(…)` | the five transport pages' shape, above | `dispatcher-instrumentation` | *"Found 0 pipelines for event: Greeting"*, the message rejected / handled. The page now says to register on `AddBrighter`'s builder; no page shows the failing shape (block scan for both calls: `FAQ.md` #3 and the five transport pages, each registering on `AddBrighter`'s) |
| `HowServiceActivatorWorks.md` § 3: the pump releases the transform pipeline straight away, or after the handler when a transform left cleanup pending | — | — | read, not run | `Reactor.cs:213–217` at 10.8.0: `if (!delivery.HasPendingCleanup) ReleasePipeline(…)` before `InvokeDispatchRequest` |

**Two claims overturned, each repaired at every recurrence (obligation 21):**

- **Ledger 61.** `QueryPipeline.md` pitfall 3's ✅ block registers `"MyCircuitBreaker"` beside
  `Constants.RetryPolicyName` and `Constants.CircuitBreakerPolicyName`, and its sentence says
  `AddPolicies` requires both. Block scan for a block calling `AddPolicies` on a registry with no
  `RetryPolicyName`: **1** (`QueryPipeline.md` #21) → **0**; `grep -rn -B10 '\.AddPolicies(' contents/ |
  grep -c 'new PolicyRegistry()'`: **1** → **0**. `DarkerBasicConfiguration.md` #11 and
  `QueryPipelinePolicies.md` #2 hold both keys already
- **Ledger 62.** `HowServiceActivatorWorks.md` now says `AddConsumers`' options configure the
  Command Processor too when it is the only registration, and that an app calling `AddBrighter` first
  gets two option sets and must register its handlers on `AddBrighter`'s builder

**Pitfall 2 was true and vague**, so it now names the exception and when it is thrown. No ledger
row: nothing it said was false.

**Measured after 5.4:** `blockcheck --explain QueryPipeline_21 HowServiceActivatorWorks_11` → 0
diagnostics. `--report` → **992 / 642 / 323 / 27**, 113 findings, as predicted: against 5.3, R8's six
blocks moved FAILED → SKIPPED (D21), and 5.4's edits, prose and one registry, moved nothing. Two
methods: the summary line, and `cut -f1` of the rows. `pagelint` 0 / **271**; `linkcheck` and
`symbolcheck` clean on the two pages.

#### Second pass (5.5)

**Every phase-5 ledger row holds.** Rows 52–62 each give their *After* from their grep, run as
written at `fcd6f7b`: **0** for every row but 55's second grep, which counts the repaired
`Fallback` that is now there (**1**). 5.1's five recurrence greps are rows 52, 56, 58, 59 and 55's
first, so they are among them. No ruling is open.

**Widened, by shape rather than words:**

| Defect | Widened grep | Hits | Reading |
|---|---|---:|---|
| 52, an unpinned package in a setup | `swagger\|swashbuckle`, case-blind | 1 | `AsyncAPISupport.md`: *"OpenAPI (Swagger)"*, an analogy in prose |
| 53, the pump's private calls | `InitRequestContext\|TranslateMessage\|InvokeDispatchRequest\|ReleasePipeline` outside `HowServiceActivatorWorks.md` | 0 | |
| 54, a route returning an array | `new\[\] *\{ *typeof\|new Type\[\]` | 1 | `AsyncAPISupport.md`: `AssembliesToScan`, an assembly list, not a route |
| 55, Darker's interfaces listed short | `interface IQuery(Handler)?\b` | 4 | the four repaired listings, each matching 4.1.1 |
| 56, a V9 handler factory | `Create\(Type\|Release\(IHandleRequests\|TinyIoC\|IAmAHandlerFactory\b` | 5 | the repaired factory (2); `CommandProcessorConfigurationReference.md`'s `handlerFactory` parameter, an `IAmAHandlerFactory` at 10.8.0 (`CommandProcessor.cs:157`); prose on 2 pages naming the family |
| 57, 62, the consumer's options | block scan for `AddBrighter(` and `AddConsumers(` in one block, and what chains off `AddConsumers` | 6 blocks | `FAQ.md` #3 and #1 of `GcpPubSubConfiguration.md`, `MQTTConfiguration.md`, `MSSQLMessageBroker.md`, `RedisConfiguration.md`, `RocketMQConfiguration.md`: each registers handlers on `AddBrighter`'s builder, the shape 5.4 ran as `transport-pages`, and chains nothing off `AddConsumers` |
| 58, the control bus | `ConfigurationCommand\|ControlBus\|controlBus` | 11 | the repaired block; `Monitoring.md`'s `IAmAControlBusSender` and `ControlBusSenderFactory`, live at 10.8.0 (phase 4) |
| 59, dead letters off `RmqSubscription` | `new \w*Subscription<` within 8 lines before a `deadLetter…` argument, not `Rmq` | 5 | `KafkaSubscription` and `SqsSubscription` on `ErrorHandlingOptions.md` (4) and `HandlingPoisonMessages.md`: both take `deadLetterRoutingKey` at 10.8.0 (`IUseBrighterDeadLetterSupport`) |
| 60, a fallback's failure mode | `fallback` beside `runtime\|throw\|exception\|error\|default(` on the 20 Darker pages | 10 | each matches `darker-pipeline`'s rows |
| 61, a registry short of the defaults | `new PolicyRegistry\b` | 4 | each registers both `Constants` names |
| pitfall 2, policies required | `without … AddDefaultPolicies\|AddPolicies`, and the reverse | 4 | the repaired pitfalls 2 and 3, and `QueryPipelinePolicies.md`, which already says `AddPolicies()` requires both names |

**The two blocks 5.3 left to be read here, both defects:**

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `ImplementAQueryHandler.md` #16: manual registration builds each handler with `Activator.CreateInstance`, and the sentence after it says each handler needs a parameterless constructor | the block, less the `GetPersonQueryHandler` no block declares, executing `GetPeopleQuery` and a `GetOrderQuery` whose handler takes `IOrderRepository`, as every `GetOrderQueryHandler` on the page does | the factory constructing that handler with its repository | `darker-pipeline` | `GetPeopleQuery` answered; `MissingMethodException` on `GetOrderQuery` / both answered. **Overturned** (ledger 63) |

- **Ledger 63.** #16 is now a `BuildQueryProcessor(IOrderRepository)` whose handler factory constructs
  `GetOrderQueryHandler` itself, and its sentence says `Activator.CreateInstance` throws
  `MissingMethodException` for a handler with no parameterless constructor. `GetPersonQueryHandler`
  is gone from the page. `DarkerConfigurationReference.md` #4 registers its own `GetPersonQueryHandler`
  through `Activator` too, but its handlers are the reader's and its prose states the requirement:
  not a recurrence
- **Ledger 64.** #22 declared a `UnauthorizedAccessException` that hides `System.UnauthorizedAccessException`
  under the section *Domain Exceptions*. It is `OrderAccessDeniedException` now, and #23 throws it. No
  other page declares a BCL exception's name
- **Found beside them.** § *Blocks that stay FAILED* gave #23's diagnostic as `OrderNotFoundException`
  alone; it also fails on `IAuthService` and `Order.IsDeleted`, which the page never declares. A unit
  stub for them was tried and refused by unit rule 1 (*"named by no BUILT block"*, 3 violations), since
  #23 stays FAILED on block 22's exceptions; the row now gives all four. #16's row names the three
  types it still lacks

**`symbolcheck` rows (5.1):** `IHandleRequests Create(Type)` is a signature, not a name the
watchlist matches, and `ConfigurationCommandType.StopChannel` has **0** sites left and was never on
more than one block. Neither earns a row, so nothing is put to the maintainer; `symbolcheck` stays
24 entries.

**Carried, not run:** `ReactorAndProactor.md` § *Handler and Mapper Requirements* says mixing sync
and async handlers *"will cause runtime errors"*. It predates the phase, no phase-5 block touches it,
and 017's run covered only the mapper half. Phase 8's reading criteria, or a later spec.

**Off-group term: 0.** Every edit is on `ImplementAQueryHandler.md` and its unit. `blockcheck
--explain` → #22 builds; #16 fails on `GetPeopleQuery`, `GetPeopleQueryHandler`, `GetOrderQueryHandler`;
#23 on `OrderAccessDeniedException`, `OrderNotFoundException`, `IAuthService`, `IsDeleted`. `--report` →
**992 / 642 / 323 / 27**, unmoved, as predicted: #16 and #23 stay FAILED, #22 stays BUILT. Two methods:
the summary line, and `cut -f1` of the rows. 67 units, 0 violations. `pagelint` 0 / **271**.

#### Phase 5 closed (5.6)

**Baselined.** `baseline.tsv` lists every BUILT block on the ten S3 pages, **114** rows, replacing
their **8** earlier ones (§ the file's own header). Three keep their ref, their text and scaffold
unchanged at their present ordinal: `CQRSWithBrighterAndDarker.md` #7 and #8 (which were #6 and #7,
before the split of #1) and `HowConfiguringTheCommandProcessorWorks.md` #10. The other five earlier
rows gained their page's unit; with the rest, they are at **`b9183b0`**, the phase's last page commit.
The rows were appended as phase 4's were, so the diff is 8 out and 114 in. Text unchanged was judged
against a `git worktree` at `0fc79a3`, never a stash. **Two methods agree:** `grep -v '^#' baseline.tsv
| grep -vc '^$'` → **642**, and `--report`'s BUILT → **642**. `--report` exit **0**, **0 findings**.

**Rule 8.** `pagelint` → **0 errors**; the one `attr-mismatch-intended` marker stays on
`PipelineValidation.md` #7, and `--plant` → 5 / 5.

**Readings (obligation 19).** `git diff -U0 0fc79a3 HEAD -- contents`, hunks against fence ranges:
10 pages, **100** changed C# blocks, all on S3. Two methods: the hunks, and S3 blocks whose text
is not on their page at `0fc79a3`, **100**.
- **AC9.** The phase added `// ...` at **17** lines in **9** blocks. `HowServiceActivatorWorks.md`
  #11, #14 and `QueriesAndQueryObjects.md` #1 are BUILT, so `--explain` has nothing to report for
  them. `HowServiceActivatorWorks.md` #1–#6 are R8's SKIPPED quotes (D21), whose `// ...` marks the
  source elided around the quoted line, and whose markers cite that line
- **AC10.** § *Behaviour (5.4)*: thirteen claims run with a control, one read from source.
  § *Second pass (5.5)*: one more run, `ImplementAQueryHandler.md` #16. No changed block lies off
  S3. Carried, not run: `ReactorAndProactor.md`'s *"will cause runtime errors"*, which no phase-5
  block touches (§ *Second pass (5.5)*)

**Gates against 5.1.**

| Gate | Predicted (5.1) | Measured at the close | |
|---|---|---|---|
| corpus | 994–1,000 | **992** | **below the band by 2** (below) |
| BUILT | 662–676 | **642** | **below the band by 20** (below) |
| FAILED | 307–312 | **323** | **above the band by 11** (below) |
| SKIPPED | 17–20 | **27** | **above the band by 7** (below) |
| `pagelint` | 0 errors / 266–281 | **0 / 271** | in band |
| `symbolcheck` | unmoved | **24, 14**, exit 0 | as predicted; 5.5 found neither candidate earns a row |
| scaffold rule | 60–69 units | **67**, 0 violations, 0 compile errors | in band |
| assemblies | 553–554 | **553** on each pin | in band: Swashbuckle was removed from `DarkerBasicConfiguration.md` #9, not pinned (ledger 52) |
| `linkcheck` · shape · redirects · `--verify` | unmoved | 168 / 0 · 161, widest 12 of 20 · 77, 7858 bytes · 161 = 161 | as predicted |
| `versioncheck` · `optioncheck` | unmoved | 0 of 18 · 0 / 59 / 529 | as predicted |
| `pagelint --plant` | unmoved | 5 / 5 | as predicted |
| off-group | **0** | **0** changed blocks off S3 | as predicted |

**Why four verdict gates fell outside their bands.** One account covers all four. 5.1 started from
*S3's 146 blocks, less the one SKIPPED, all BUILT* (673), and allowed 3–8 to stay FAILED and 0–3 new
skips:
- **Five retags, unpredicted.** `QueriesAndQueryObjects.md` #13–#17 were lists of query names, not
  C#. With three splits, the corpus is 994 + 3 − 5 = **992**, and S3 holds 144 blocks
- **Nineteen stay FAILED, not 3–8**, every one *same-page* and listed. 5.1 counted the five blocks
  already *same-page*. 5.3's repairs made fragments whole on one running example per page
  (`CQRSWithBrighterAndDarker.md`'s e-commerce model, `ImplementAQueryHandler.md`'s domain
  exceptions), so a block now names types another block on its page declares, which unit rule 1 does
  not let a stub supply. 304 + 19 = **323**
- **Ten new skips, not 0–3.** Six are R8's, under a reason that did not exist at 5.1 (D21); four are
  5.3's signatures and fragments shown whole elsewhere. 17 + 10 = **27**
- **BUILT follows from the three:** 673 + 3 − 5 − 19 − 10 = **642**

`pagelint` landed in band by the mechanism 5.1 named: S3's 87 warnings fell to **12**, 11 of them
on SKIPPED blocks and the twelfth on `QueriesAndQueryObjects.md` #1, BUILT and untouched, which 5.1
said would stay. 346 − 75 = **271**. None of the measured numbers is adopted. The lesson for phase 6:
a repair that makes blocks whole on a running example moves blocks from *page-type* to *same-page*,
so predict the stays from how many blocks the example spans, not from the *same-page* count before
the work. `tools/README.md` rows 2 and 9 now give the close's figures.

### Phase 6 — S4, Using an External Bus; Transports *(6 tasks, one PR)*

- [x] **Task 6.1:** Predict phase 6's gate movement
  - Input: § *The groups*, S4; recurrence greps for S4's defects
  - Output: § *Phase 6 as executed*, *Predicted*, as 3.1's
- [x] **Task 6.2:** Repair S4's reachable blocks
  - Input: S4's table, reachable rows; the V4 pin for `S3LuggageStore.md` #1
  - Output: as 3.2's, for S4. `S3LuggageStore.md` #1 is BUILT or listed with a diagnostic that is
    not `CS0234 … V4`
- [x] **Task 6.3:** Repair S4's hard blocks
  - Input: S4's table, hard rows; Brighter `10.8.0` source, the pinned release
  - Output: as 3.3's, for S4. `KafkaConfiguration.md`'s 14 *parse* blocks are each made whole,
    skipped with an accepted reason, or listed
- [x] **Task 6.4:** Run S4's behavioural claims (AC10)
  - Input: S4's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s4/<name>/Program.cs`
- [x] **Task 6.5:** Second pass for S4
  - Input: 6.1's recurrences; rulings asked
  - Output: as 3.6's
- [x] **Task 6.6:** Close phase 6
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 6 as executed

**Measured at phase start, `f4d965f`** (#204's merge). `--report` → **992 / 642 / 323 / 27, 0 findings**,
exit 0; `--classify` → 66 parse, 19 built-by-using, 57 defect, 33 same-page, 18 values, 130 page-type,
settled in 3 rounds. 553 reference assemblies on each pin; scaffold rule 67 units, 0 violations; pin
rule 4 pages. `pagelint` 0 errors / **271** warnings, `--plant` 5 / 5; `symbolcheck` 24 entries, 14
silenced; `linkcheck` 168 / 0; `versioncheck` 0 of 18 across 5 pages; `optioncheck` 0 / 59 / 529.

#### Predicted (6.1)

**S4 at `f4d965f`**: `groups.py` gives 17 pages, **125** FAILED (35 parse, 7 built-by-using,
**15** defect, 3 same-page, 6 values, **59** page-type), 14 BUILT, 10 with nothing BUILT. Two methods:
the FAILED column, 125; `grep` over `--report` on the seventeen pages, 125, with 14 BUILT and 4
SKIPPED (`CloudEventsSupport.md` #7, #9, #11, #12). The S4 pages hold **143** C# blocks.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| Tasks 6.3 and 7.3: *"10.7.0 source"* | the pins are 10.8.0 | both tasks say 10.8.0 |
| S4's table: `CloudEventsSupport.md` **1** defect, **6** page-type; S4 **16** defect, **58** page-type | **0** and **7**; S4 **15** and **59**. #5 was repaired off-group at task 4.3 (ledger 32) and now fails only on the reader's `MyCommand` | the table stands, being the instrument's at `f3f685f`; this row is the reading at `f4d965f` |
| S4's table: **15** defect blocks (as measured) | the second compile's diagnostics (below): **8** are API defects, **1** is a `using` the classifier could not choose, **2** are alternatives in one fence, and **4** are fragments | the table stands; the repairs follow the reading |

**Reading S4's 15 defect blocks** with `probe/second.py` over the seventeen pages (547 diagnostic
lines), each against the 10.8.0 tag:

| Page | # | Second compile | Reading against the pin | Repair |
|---|---:|---|---|---|
| `AWSSQSConfiguration.md` | 7, 8, 9, 10 | `CS1739` `name` on `SqsSubscription` | **defect.** Both `SqsSubscription<T>` constructors take `subscriptionName:` (`SqsSubscription.cs:123`, `:206`); there is no `name` | 6.2 |
| `MessageTransforms.md` | 7 | `CS1739` `compressionType`; `CS0103` `CompressionType` | **defect.** `CompressAttribute(int step, CompressionMethod compressionMethod = GZip, CompressionLevel compressionLevel = Optimal, int thresholdInKb = 0)` (`CompressAttribute.cs:47`); the enum is `CompressionMethod.GZip`, `Brotli`, `Zlib` (`CompressionMethod.cs:3`) | 6.3 |
| `MessageTransforms.md` | 8 | as #7; `CS0246` `RemovePII`, `RestorePII` | **defect**, twice: the compression arguments as #7, and two attributes Brighter does not ship and the page never declares. At 10.8.0 the shipped transforms are `ClaimCheck`/`RetrieveClaim`, `Compress`/`Decompress`, `CloudEvents` and the JustSaying pair. The page's earlier blocks teach the reader to write a transform | 6.3: the PII pair declared as the reader's own transform, or replaced by a shipped one; judged at the repair |
| `RabbitMQConnectionStability.md` | 1 | `CS1739` `circuitBreakerTimeInMilliseconds` | **defect.** `AmqpUriSpecification(Uri uri, int connectionRetryCount = 3, int retryWaitInMilliseconds = 1000, int circuitBreakTimeInMilliseconds = 60000)` (`RmqMessagingGatewayConnection.cs:116`, both RMQ packages) | 6.2 |
| `ImplementingExternalBus.md` | 3 | `CS0246` `ILog`; `CS0103` `LogProvider`; `CS1729` | **defect.** A V9 handler: LibLog's `ILog` and `LogProvider`, and a `RequestHandler` constructor taking a logger, none of which exist at 10.8.0. `CommandProcessor.CIRCUITBREAKER`/`RETRYPOLICY` (`CommandProcessor.cs:103`, `:111`) and `UsePolicy(string[], int)` (`UsePolicyAtttribute.cs:62`) are live. The page's banner says Brighter V10 and does not label the block V9 | 6.3, the V10 handler |
| `RabbitMQConfiguration.md` | 5 | `CS0104` `RmqSubscription<>` ambiguous | **a `using` the classifier could not choose**: the pin carries both RMQ packages. The block sets `queueType: QueueType.Quorum`, which only `RMQ.Async`'s `RmqSubscription` takes (`RmqSubscription.cs:135`, `:199`) | 6.2, `using Paramore.Brighter.MessagingGateway.RMQ.Async;` |
| `AWSSQSMigrateToV10.md` | 5 | `CS0128` `credentials` | **alternatives**: a *Problem* and a *Solution*, each declaring `credentials` | 6.3, split or made whole |
| `AWSSQSMigrateToV10.md` | 6 | `CS0128` `connection`; `CS0103` `credentials` | **alternatives**, as #5. `AWSMessagingGatewayConnection(AWSCredentials, RegionEndpoint, Action<ClientConfig>?)` matches 10.8.0 (`AWSMessagingGatewayConfiguration.cs:42`) | 6.3, split or made whole |
| `AsyncAPISupport.md` | 8 | `CS0161` | a fragment: a method whose body is a comment. `IAmASchemaGenerator.GenerateAsync(Type?, CancellationToken)` matches 10.8.0 (`IAmASchemaGenerator.cs:38`) | 6.3, a body |
| `DefaultMessageMappers.md` | 14 | `CS0161`; `CS0246` `LargeEvent` | a fragment, as #8. `ClaimCheck(int step, int thresholdInKb = 0)` matches (`ClaimCheckAttribute.cs:43`) | 6.3, a body |
| `HandlerFailure.md` | 9 | `CS0161`; `CS0246` `PlaceOrder` | a fragment: `MapToMessage`'s body is `// ...`. `InvalidMessageAction(string?)` and `(string?, Exception?)` match (`InvalidMessageAction.cs:56`, `:63`) | 6.3, a body |
| `DynamicMessageDeserialization.md` | 7 | `CS7036` `subscriptionName` | a fragment: `KafkaSubscription`'s required arguments and the `switch`'s arms elided as `// ...` | 6.3, made whole, or skipped as a fragment shown whole elsewhere on the page |

**Recurrence greps (obligation 20), run as written at `f4d965f`:**

| Defect | Grep | Hits | Off-group |
|---|---|---:|---|
| `name:` for `subscriptionName:` | `grep -rnE 'name: new SubscriptionName' contents/` | **4**, `AWSSQSConfiguration.md` #7–#10 | none |
| Compression arguments | `grep -rnE 'compressionType\|CompressionType\.' contents/` | **2**, `MessageTransforms.md` #7, #8 | none |
| `circuitBreakerTimeInMilliseconds` | `grep -rn circuitBreakerTimeInMilliseconds contents/` | **3**: `RabbitMQConnectionStability.md` #1; on-group and hidden by a *parse* error, `RabbitMQConfiguration.md` **#4** (`= 30000`, an assignment inside an argument list); and `RabbitMQConfiguration.md`'s *Connection Reliability Options* prose | none. The prose is repaired with #4 |
| LibLog | `grep -rnE 'LogProvider\|\bILog\b\|: base\(logger\)' contents/` | **2** lines in 1 block (`ImplementingExternalBus.md` #3) | none |
| Unshipped PII transforms | `grep -rnE 'RemovePII\|RestorePII' contents/` | **2** lines in 1 block (`MessageTransforms.md` #8) | none |
| Quorum on a subscription | `grep -rn 'QueueType\.Quorum' contents/` | **2**: `RabbitMQConfiguration.md` #5 and the prose naming it | none |

**The off-group term is zero.** No S4 defect recurs on a page outside S4.

**The parse blocks** (35) hide what lies behind them, as `RabbitMQConfiguration.md` #4 shows. None of
them is another language in a C# fence, so no retag is predicted. Read by their first lines, they are:
- **quotes of Brighter's source**, candidates for D21's skip if their lines match 10.8.0:
  `MessageMappers.md` #3 and #5 (`MessageBody.cs:126`, `:164`) and `MessageTransforms.md` #4
  (`TransformAttributeBase.cs:5`)
- **lone `configHook:` arguments**: `KafkaConfiguration.md` #9–#13 and #18, made whole or skipped as
  fragments of a configuration shown whole on the page (task 6.3's six of fourteen)
- **alternatives in one fence**: `CloudEventsSupport.md` #13–#15, `DynamicMessageDeserialization.md`
  #3 and #5, `DefaultMessageMappers.md` #13, split or made whole
- **configurations with `...` for arguments**, the rest: made whole

**Where blocks will stay *same-page*** (phase 5's lesson: count from what the running example spans,
not from the class today). A unit may not stand in for a type the page declares, so the blocks at
risk name a type another block on their page declares (`probe`'s reading of `second.py`'s `CS0246` and
`CS0103` names against the page's own `class`/`record`/`interface` declarations):
- **the 3 already *same-page***: `AsyncAPISupport.md` #4 and #9, `Routing.md` #3
- **4 more already named by a declaration**: `AsyncAPISupport.md` #11 and #12 (`OrderCreatedEvent`,
  declared in #5); `DefaultMessageMappers.md` #6 and #10 (`OrderShipped`, declared in #5)
- **the running examples, undeclared today**, which a repair that makes a fragment whole by declaring
  them would move: `PlaceOrder` across all 11 of `HandlerFailure.md`'s FAILED blocks, `GreetingEvent`
  across 11 of `KafkaConfiguration.md`'s, `OrderCreated` across 10 of `DefaultMessageMappers.md`'s and
  5 of `CloudEventsSupport.md`'s. A stub serves each while no block declares it; the band allows one
  such example to be declared

**`pagelint`, block by block.** S4's **94** warnings (`--per-page` on the seventeen pages; 16 carry
one, `S3LuggageStore.md` none), read against `--report` and `--classify`:
- **5 on blocks no repair touches**: `AWSSQSMigrateToV10.md` #7 (BUILT); `CloudEventsSupport.md` #7,
  #9, #11, #12 (SKIPPED). They stay
- **39 on FAILED blocks that already declare `// ...`**, on eleven pages. Each keeps its warning unless
  its repair gives it `using`s; phase 5's ten such blocks mostly gained them
- **5 on FAILED blocks whose first compile needed no `using`**: `CloudEventsSupport.md` #16;
  `DefaultMessageMappers.md` #1, #7, #8, #12. A stub alone may build them, and they keep their warning
- **45 on the other FAILED blocks** (18 *parse*, 16 *page-type*, 5 *defect*, 3 *values*, 2 *same-page*,
  1 *built-by-using*), which their repairs edit. A block skipped keeps its warning

**Predicted:**

| Gate | At `f4d965f` | Predicted at the close | Mechanism |
|---|---|---|---|
| corpus | 992 | **992–1,000** | splits only, up to +8: `AWSSQSMigrateToV10.md` #5, #6; `CloudEventsSupport.md` #13–#15; `DynamicMessageDeserialization.md` #3, #5; `DefaultMessageMappers.md` #13. Each may instead be made whole in one fence. No retag |
| BUILT | 642 | **740–770** | 767 (642 + S4's 125, all BUILT), plus splits (0–8), less new skips (2–9) and what stays FAILED (3–18) |
| FAILED | 323 | **201–216** | 198 (−125 for S4) plus what stays FAILED and listed: 3–18, the three groups above; off-group 0 |
| SKIPPED | 27 | **29–36** | D21's three source quotes, 2–3 of them as their lines read at 10.8.0; `KafkaConfiguration.md`'s six `configHook:` fragments, 0–6 |
| `pagelint` | 0 errors / 271 | **0 errors / 185–210** | 271 less 94 plus what of S4's stays: the 5 untouched, 0–5 a stub builds untouched, the 3–9 new skips, 0–10 declared `// ...` that keep their omission, 0–4 on a split's new fence. Off-group 0 |
| `symbolcheck` | 24 entries, 14 silenced | **unmoved** | the S4 pages carry 2 of the 14 silenced; whether `compressionType:`, `circuitBreakerTimeInMilliseconds` or `LogProvider` earn a row is read at 6.5 |
| scaffold rule | 67 units | **74–81 units**, 0 violations, 0 compile errors | 3 S4 pages have units (`MessageMappersContext`, `TransportConfigurationContext`, `S3LuggageStoreContext`); 7–14 of the other fourteen need one, fewer where a transport page shares `TransportConfigurationContext` |
| pin rule | 4 pages | **unmoved**, 0 violations | `S3LuggageStore.md` #1 fails only `CS0103` `credentials` on the V4 pin, the namespace resolving; a unit or a declaration builds it |
| assemblies | 553 | **unmoved** | every package S4 names is pinned (AsyncAPI, AWSSQS, Azure Service Bus, Kafka, both RMQ, the AWS transformers) |
| `optioncheck` | 0 / 59 / 529 | **0 mismatches, 59 / 529 unless a repair adds a row** | five S4 pages carry marked tables; the `circuitBreakerTimeInMilliseconds` prose is a list, not a marked table |
| `linkcheck` · `versioncheck` · `pagelint --plant` | 168 / 0 · 0 of 18 · 5 / 5 | **unmoved** | |

#### The repairs (6.2)

**S4's reachable rows, 78 blocks**: the 72 classed `built-by-using` (7), `values` (6) and `page-type` (59), and
the six 6.1's reading assigned here (`AWSSQSConfiguration.md` #7–#10, `RabbitMQConnectionStability.md` #1,
`RabbitMQConfiguration.md` #5; `S3LuggageStore.md` #1 is among the six `values`). **73 of the 78 build**, and
5 do not: four stay FAILED because the type they lack is declared by another block on their page (below), and
one needs a package the pin does not carry. `AWSSQSConfiguration.md` #7–#10, `RabbitMQConnectionStability.md` #1
and `RabbitMQConfiguration.md` #5 build once their `name:`, `circuitBreakTimeInMilliseconds` and
`using Paramore.Brighter.MessagingGateway.RMQ.Async;` are in. **`S3LuggageStore.md` #1 is BUILT** on the V4 pin: a
`credentials` value in `S3LuggageStoreContext` is all it lacked, and the namespace resolved. Beside the 78,
`AsyncAPISupport.md` #4 and #9 were given their `using`s although they stay FAILED, so the diagnostic that
remains is the declared type's alone. Eleven scaffold units are new (`AsyncAPISupport`, `CloudEventsSupport`,
`DefaultMessageMappers`, `DynamicMessageDeserialization`, `HandlerFailure`, `ImplementingExternalBus`,
`KafkaConfiguration`, `MessageTransforms`, `RabbitMQConfiguration`, `Routing`, `RoutingMultipleMessageTypes`) and two
extended (`MessageMappersContext`, `S3LuggageStoreContext`), with **0** unit-rule violations.

**Measured after 6.2:** `--report` → **992 / 715 / 250 / 27**; 78 findings, all expected: 73 BUILT blocks
not yet baselined (6.6), and 5 BUILT blocks whose page gained a unit (`AsyncAPISupport.md` #5;
`CloudEventsSupport.md` #8, #10; `DefaultMessageMappers.md` #4, #5). The scaffold rule reads **78 units, 0
violations, 0 compile errors**; the pin rule 4 pages, 0 violations; the V4 pin 28 blocks, 25 built, 3 failing (24 and 4
before). `groups.py` over this `--classify` gives S4 **15 pages, 52 FAILED** (35 parse, 9 defect, 5 same-page, 3
page-type), 76 BUILT, one page with nothing BUILT (`AzureServiceBusConfiguration.md`); `RoutingMultipleMessageTypes.md`
and `S3LuggageStore.md` are whole and have left the table. Two methods: the FAILED column, 52; `--report` on the
seventeen pages, 52, with 87 BUILT and 4 SKIPPED. `pagelint` 0 errors / **218** warnings (271 before; S4's own 94 →
**41**, `--per-page` on the seventeen pages), `--changed origin/master` 0 errors, `--plant` 5 / 5. `linkcheck` 168 / 0.
`symbolcheck` 24 entries, 14 silenced. `optioncheck` 0 mismatches, 59 tables, 529 rows.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *Predicted (6.1)*, assemblies: *"every package S4 names is pinned"* | `RabbitMQConnectionStability.md` #3 names Serilog (`Log`, `LoggerConfiguration`, `AddSerilog`, a Seq sink). No pin carries it: `grep -in serilog tools/blockcheck/refs/refs.csproj tools/blockcheck/refs-v4/refs-v4.csproj` → 0. Serilog is named on one page only (`grep -rln Serilog contents/` → 1) | the prediction stands, reconciled at 6.6; the block stays FAILED (§ *Blocks that stay FAILED*) and the pin is unmoved at 553 |
| § *Predicted (6.1)*, `versioncheck` *0 of 18* | `python3 tools/versioncheck.py` → **18 of 18 stale**: NuGet's latest `Paramore.*` is 10.8.1 and the five tutorial pages pin 10.8.0. No S4 page is on its list | not S4's, not a 6.2 change; the pins are `10.8.0` on purpose (the refs pin) and the tutorial pages are phase 7's or the maintainer's to move. Carried to 6.6 |
| § *Predicted (6.1)*, `pagelint`: *0 errors / 185–210 at the close* | 218 after 6.2 alone, with 6.3's *parse* and *defect* blocks (44 of S4's 52) still to be edited | the band stands for the close; reconciled at 6.6 |

**What stays FAILED from the reachable rows (5).** The unit rule (2) forbids a stub for a type another block
on the page declares, so these cannot be repaired by a stub; making them whole would declare the type a second
time, a change of what the page teaches:
- `AsyncAPISupport.md` #11 and #12 (`OrderCreatedEvent`, declared by #5) and #4 (the same, and `rmqConnection`,
  which no BUILT block names, so no unit may stub it). The page now says so before #4 and before the *Complete
  Examples* (two sentences); #9 follows #8, which declares `MySchemaGenerator`
- `DefaultMessageMappers.md` #6 and #10 (`OrderShipped` and `AvroMessageMapperAsync<T>`, declared by #4 and #5, and the
  `Orders` namespace #5 declares)
- `RabbitMQConnectionStability.md` #3 (Serilog; above)

**What the reachable rows hid.** A block that did not compile had not been checked for anything else:
- **A type that does not exist (ledger 67).** `CloudEventsSupport.md` #1 and `DefaultMessageMappers.md` #1
  constructed `RabbitMQProducerRegistryFactory`; the type is `RmqProducerRegistryFactory`, in both RMQ packages.
  The classifier called both *page-type* because the lacking `OrderCreated` and `services` were the first errors
- **A plain `Publication` given to a transport's registry factory (ledger 68).** `RmqProducerRegistryFactory` takes
  `IEnumerable<RmqPublication>` and `KafkaProducerRegistryFactory` `IEnumerable<KafkaPublication>`, so a
  `Publication[]` is `CS1503`. `CloudEventsSupport.md` #1 and `DefaultMessageMappers.md` #1 now construct
  `RmqPublication`, `RoutingMultipleMessageTypes.md` #2's three `KafkaPublication`. `CloudEventsSupport.md` #2 builds a
  `Publication` and passes it nowhere, and stays one; so does `InMemoryOptions.md`, whose factory takes `Publication`
- **A required member left to a comment (ledger 69).** `KafkaMessagingGatewayConfiguration { /* ... */ }` leaves
  `BootStrapServers`, which is `required`, unset (`CS9035`). `RoutingMultipleMessageTypes.md` #2 now sets it and keeps its
  `// ...`
- **`name:` for `subscriptionName:` (ledger 65)** and **`circuitBreakerTimeInMilliseconds` (ledger 66)**, the two 6.1
  predicted. The second recurs in `RabbitMQConfiguration.md` #4 (a *parse* block) and in its *RabbitMQ Connection
  Reliability Options* list; both now say `circuitBreakTimeInMilliseconds`

**Units, by what they supply.** Each stub is a request (`Event` or `Command`) and nothing more unless a BUILT block
names a member: `OrderCreated` in `DefaultMessageMappersContext` carries `CustomerId` and `Total`, `PlaceOrder` in
`HandlerFailureContext` carries `OrderId`, `Amount` and `CustomerId`, `TaskReminderCommand` takes the four arguments
`ImplementingExternalBus.md` #1 names. A mapper stub derives from `JsonMessageMapper<T>` and adds nothing, so
`mappers.Register<LargeOrder, CompressedOrderMapper>()` is still checked against the real constraint. The values
(`services`, `_commandProcessor`, `options`, `builder`, `host`, `subscription`, `_logger`) are typed from a pinned package
and return a default; `reminder` is a tuple of the four strings the block reads; `ProcessOrder` and `ProcessOrderAsync` are
statics that do nothing, reached through `using static`, standing for the `// ...` of the reader's own handler.
`MessageMappersContext` also supplies the four fields of the Kafka mapper `MessageMappers.md` #6 shows on its own.

**For 6.3.**
- `RabbitMQConfiguration.md` #4: the name is repaired (ledger 66); the parse errors remain (a missing comma and `=`
  in an argument list) and the block needs `GreetingMade`, which `RabbitMQConfigurationContext` now supplies
- `KafkaConfiguration.md`: `GreetingEvent` is now in `KafkaConfigurationContext`, and `new KafkaMessagingGatewayConfiguration {...}`
  is the `BootStrapServers` defect of ledger 69 when made whole
- `DynamicMessageDeserialization.md` #6 builds with `// ...` as its argument list (`KafkaSubscription<T>` has a
  parameterless constructor); #7 is still a fragment (`CS7036`)
- `MessageTransforms.md` #7, #8: `GreetingEvent` is the unit's; `LargeOrder`, `SensitiveOrder` are not, and are declared by no block
- `ImplementingExternalBus.md` #3 stays 6.3's V10 handler; `TaskReminderCommand` is in the unit

**For 6.4.** Blocks edited by 6.2 whose comments or prose assert behaviour, none run here:
- `KafkaConfiguration.md` #15: `CooperativeSticky` with manual commits throws (the page names `ArgumentOutOfRangeException`)
- `DefaultMessageMappers.md` #1, #8, #9, #12: the default mapper is `JsonMessageMapper<T>`; binary-mode CloudEvents as the default,
  structured-mode with `CloudEventJsonMessageMapper<>`; #3, #9 and #15 choose a default mapper; #1 now also `RmqPublication` (ledger 68)
- `CloudEventsSupport.md` #1, #6: the Publication's CloudEvents attributes reach the header; a claim-checked message over
  256 KB is stored and its `dataref` set
- `HandlerFailure.md` #1–#8, #10, #11: each attribute's action on an unhandled exception (defer with a delay, reject to the
  dead letter queue, don't acknowledge), their step order, and which exception type each action is
- `DynamicMessageDeserialization.md` #4, #8; `RoutingMultipleMessageTypes.md` #1, #3, #4, #6–#9: `getRequestType` resolves
  by CloudEvents type or header and an unmapped message throws `InvalidMessageAction`; #6's handler routing by property
- `AsyncAPISupport.md` #2, #10: `GenerateAsyncApiDocumentAsync` writes JSON and YAML
- `AWSSQSConfiguration.md` #7–#10: the FIFO and point-to-point attributes (ledger 65)

#### The hard blocks (6.3)

**Every S4 hard block leaves in one of the four states.** The 52 FAILED at the start of 6.3 (35 *parse*, 9 *defect*,
5 *same-page*, 3 *page-type*) are now **40 BUILT, 2 SKIPPED and 10 FAILED and listed** (§ *Blocks that stay FAILED*):
all ten are *same-page* or the Serilog block 6.2 already listed, and none is retagged. `KafkaConfiguration.md`'s 14
*parse* blocks (#1–#13, #18) are all BUILT; the six `configHook:` fragments (#9–#13, #18) were each made whole inside the
`KafkaSubscription<GreetingEvent>` call they belong to, as 3.3 did, so none is SKIPPED under D3. No fence was split: the
alternatives 6.1 listed (`AWSSQSMigrateToV10.md` #5, #6; `CloudEventsSupport.md` #13–#15; `DynamicMessageDeserialization.md`
#3, #5; `DefaultMessageMappers.md` #13) are each one whole fence, so the corpus holds the same 994 blocks.

**Measured after 6.3:** `--report` → **994 / 757 / 208 / 29**, 118 findings, all expected: 113 BUILT blocks not yet
baselined (6.6; 73 from 6.2 and 40 from this task) and the same 5 whose page gained a unit. The scaffold rule reads **79
units, 0 violations, 0 compile errors** (78 + `AzureServiceBusConfigurationContext`); the pin rule 4 pages, 0 violations.
S4 alone: **127 BUILT, 10 FAILED, 6 SKIPPED** (87, 52, 4 before), 143 blocks. Two methods: `--report` grepped on the
seventeen pages, 127 / 10 / 6; `groups.py` over this `--classify`, S4 **4 pages, 10 FAILED** (7 *same-page*, 3
*page-type*), and no S4 page with nothing BUILT. `pagelint` 0 errors / **185** warnings (218 before: S4's own 41 → **8**
by `--per-page` on the seventeen pages, and 218 − 185 = 33 = 41 − 8), `--changed origin/master` 0 errors, `--plant` 5 / 5.
`linkcheck` 169 / 0. `symbolcheck` 24 entries, 14 silenced. `versioncheck` 0 stale of 18. `optioncheck` 0 mismatches on
the four S4 pages with marked tables (10 tables, 133 rows); no table was touched.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| Task 6.3: *"Brighter `10.8.0` source, the pinned release"* | the pins are 10.8.1 (bugfix 0003, #206). Every API read here is at the `10.8.1` tag; `MessageBody.cs:126`, `:164`, `:191` and `TransformAttributeBase.cs:5` read the same at `10.8.0` and `10.8.1` | the task line stands as written; this row is the reading |
| § *The repairs (6.2)*, *Measured after 6.2*: **992 / 715 / 250 / 27** at 10.8.0 | after the rebase onto 10.8.1, **994 / 717 / 250 / 27**: the +2 are `HealthChecks.md` #2 and #3, added and baselined by #206, off-group. No S4 verdict moved (87 / 52 / 4 at both); `--report` at 10.8.1 was byte-identical to 10.8.0's at #206, codes included. `pagelint` 0 / 218, `versioncheck` 0 of 18, `linkcheck` 169 / 0, `symbolcheck` 24 / 14, scaffold 78 units, 0 violations | 6.1 and 6.2 stand as measured at 10.8.0; this row is the reading at the start of 6.3 |
| § *Predicted (6.1)*: `MessageMappers.md` #3, #5 and `MessageTransforms.md` #4 are candidates for D21's skip *"if their lines match 10.8.0"* | the lines match (`MessageBody.cs:126`, `:164`, `:191`; `TransformAttributeBase.cs:5`), but D21's reason is *"calling a private member or reading the pump's own state"*, and these are a public constructor's signature and a public class's declaration. #3 and #5 are SKIPPED under 017's *"a method signature shown for comparison, with no body or class to compile in"*, their braces closed; #4 is made whole, as a copy of the declaration at 10.8.1 (ledger 76). D21 is not used | the prediction stands, reconciled at 6.6 |
| § *Predicted (6.1)*: `MessageTransforms.md` #8's `RemovePII`/`RestorePII` *"declared as the reader's own transform, or replaced by a shipped one; judged at the repair"* | judged: replaced (ledger 72). A declared pair needs two transformer classes implementing `IAmAMessageTransform`, an invented PII scrubber the page does not teach; the shipped `Compress` then `ClaimCheck` is the ordering the page's own *Step* paragraph describes | the prediction stands |
| § *Predicted (6.1)*: *"`KafkaConfiguration.md`'s six `configHook:` fragments, 0–6"* skipped | 0 skipped: each is whole | the prediction stands |
| § *Predicted (6.1)*: *"`MessageMappers.md` #3 and #5"* (two blocks) and `Routing.md` among the *same-page* 3 | #5 holds two constructors in one fence; both are in the one SKIPPED block. `Routing.md` #4 and `AsyncAPISupport.md` #7 join the *same-page* list: each names a type another block declares (`MyReply`, block 2; `OrderCreatedEvent`, block 5), hidden until its parse errors were gone | § *Blocks that stay FAILED* carries both |

**Defects the prediction did not name (ledger 73–76), each found in a block 6.3 was repairing and repaired in place.**
- **`Routing.md` #3 and #4 (ledger 73).** #3 did not report that it assigned a `Guid` to `Id` (`CS0029`), because `MyRequest`
  was unresolved and `request` untyped; compiled with block 2's classes in scope, it did. Its serialiser, and the one #4
  needed, used `new JProperty("Id", request.Id)`, which Newtonsoft cannot build from the record. `probe/s4/routing-id`: case
  (`request.Id.Value`) round-trips the Id; control (`request.Id`) throws `ArgumentException: Could not determine JSON object
  type for type Paramore.Brighter.Id`
- **`CloudEventsSupport.md` #14 (ledger 74).** `new Uri("/orders/12345")` is `file:///orders/12345` on macOS and Linux
  (`Uri.IsAbsoluteUri` true, checked on .NET 9) and throws on Windows; now `UriKind.Relative`. Nothing in `Paramore.Brighter`
  reads `AbsoluteUri`, so a relative source passes through; that is read, not run
- **`AzureServiceBusConfiguration.md` #2 (ledger 75).** `builder.Services` in a method that takes `services`; the three
  mapper `.Topic` constants it read, from mappers the page never shows, are now the routing keys block 1 gives the matching
  publications, paired by name

**Recurrences, by their greps (ledger 65–76).** `compressionType` and `RemovePII` and `LogProvider` recur nowhere; the
`KafkaMessagingGatewayConfiguration {...}` of ledger 69 recurred on `KafkaConfiguration.md` #7, #8 and is closed by editing
that row. No S4 defect recurs on a page outside S4.

**Units.** One unit is new (`AzureServiceBusConfigurationContext`: three requests). Six are extended, each stub a request or
a member a BUILT block names: `GreetingEvent.CustomerId` (`KafkaConfiguration`), `LargeOrder` (`MessageTransforms`),
`LargeEvent` (`DefaultMessageMappers`), `OrderCreated` (`DynamicMessageDeserialization`), `services` (`RabbitMQConfiguration`,
for #6's top-level statements), and `TaskReminderCommand`'s four members with `IAmAMailGateway`, `TaskReminder`, `TaskName`
and `EmailAddress` (`ImplementingExternalBus`). `pages.tsv` gained one row. `AsyncAPISupportContext`'s comment names the two
blocks that now use its `services`.

**For 6.4.** Blocks 6.3 wrote or edited whose text asserts behaviour, none run here but `routing-id`:
- `MessageTransforms.md` #8 and the sentence after it: *"On receive, they execute in reverse order"* and the step numbers
  of the send and receive chains (`Compress` 0, `ClaimCheck` 1; `RetrieveClaim` 0, `Decompress` 1)
- `Routing.md` #3, #4: the Id round trip is run; the `ReplyAddress` routing is not
- `CloudEventsSupport.md` #13–#15: that a relative `Source` and a `Subject` reach the header
- `DynamicMessageDeserialization.md` #3, #4 (the `InvalidMessageAction` default arm), #7
- `KafkaConfiguration.md` #9–#13, #18: that each `configHook` setting reaches the consumer, and #2's `SslCaLocation` ternary
- `RabbitMQConfiguration.md` #4: `circuitBreakTimeInMilliseconds: 30000`, the argument ledger 66 names

#### The behavioural claims (6.4, AC10)

**Every run is against the released 10.8.1 packages**, as `probe/s4/<name>/` projects; each prints `ok` or `FAIL` per
row and exits 1 on any `FAIL`. Each block is copied as the page shows it, the probe's own lines marked `// probe`. Two
claims needed a broker, and each got one: `kafka-consumer` runs against `apache/kafka:4.0.2` and `sqs-channels` against
`localstack/localstack:4.4`, each started on `localhost` for the run and removed after it, as the csproj comments say.
RabbitMQ's claims are about Brighter's connection policies, which the probe drives directly, so no broker was needed.
`routing-id` is 6.3's, rerun here at 10.8.1 with the same output.

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `MessageTransforms.md`, the sentence after #8, as it stood: *"The transforms execute in order based on the `step` parameter. On receive, they execute in reverse order"* | recording transforms, A at step 0 and B at step 1, on a mapper's send and receive | the steps swapped | `transform-order` | send B, A; receive B, A / send A, B; receive A, B. **Both pipelines run the highest step first.** **Overturned** (ledger 77) |
| `MessageTransforms.md` #8, as it stood: `[Compress(step: 0)] // First: Compress`, `[ClaimCheck(step: 1)] // Second` | #8 as repaired (`Compress` 1, `ClaimCheck` 0), a 600 KB body of one repeated character, through Brighter's `TransformPipelineBuilder`, an `InMemoryStorageProvider` the luggage store | #8 as it stood | `transform-order` | 707 bytes sent, not claim-checked / claim-checked, 68 bytes sent: the claim token compressed, the payload stored uncompressed. **Overturned** (ledger 77) |
| `MessageTransforms.md` #8 as repaired: `RetrieveClaim` (step 1) fetches before `Decompress` (step 0) | 1.2 MB of random hex, still over 512 KB once compressed, so the stored luggage is compressed | the receive as it stood (`RetrieveClaim` 0, `Decompress` 1) | `transform-order` | claim-checked, restored / `JsonException`, *"'0x1F' is an invalid start of a value"*: the gzip bytes reach the mapper |
| `DefaultMessageMappers.md` § *Transform Pipelines* (`254e8a7`): *"Brighter ships the Claim Check and Compression transforms; for anything else … you write your own"* | reflection over `Paramore.Brighter`, `Paramore.Brighter.Transformers.JustSaying` and `…MassTransit` 10.8.1 for concrete `WrapWithAttribute` and `UnwrapWithAttribute` types | the same reflection over the probe's assembly, which declares an `EncryptAttribute` | `shipped-transforms` | `ClaimCheck`/`RetrieveClaim`, `Compress`/`Decompress`, `CloudEvents`/`ReadFromCloudEvents`; `JustSaying`/`JustSayingDecompress`; `MassTransitWrap`/`MassTransitUnwrap` / `EncryptAttribute`. **Overturned** (ledger 78) |
| `HandlerFailure.md` #1: `[DeferMessageOnError(0, 5000)]` turns any unhandled exception into `DeferMessageAction`; `0` or omitted leaves the Subscription's delay | a handler that throws | no attribute; `delayMilliseconds` omitted | `handler-failure` | `DeferMessageAction`, `InnerException` the original, `Delay` 5 s / the original exception; `Delay` null |
| `HandlerFailure.md` #2, #5, #8, #10, #11: a backstop at step 0 outside a retry at step 1 fires only once the retries are spent | each page handler, a retry of 3 the probe supplies for the page's unshown `OrderRetryPolicy`, always failing; #2 failing twice | the backstop at step 2, inside the retry | `handler-failure` | 4 runs, then `DeferMessageAction`, `RejectMessageAction` (#5, #10, and #11 async) or `DontAckAction`, each with the original inside; failing twice, 3 runs and no exception / 1 run, then the action |
| `HandlerFailure.md` #6: `[FeatureSwitch(…, dontAck: true)]` throws `DontAckAction` when the feature is off; without it the message is silently consumed | the handler switched off | `dontAck` omitted | `handler-failure` | `DontAckAction`, handler not run / no exception, handler not run |
| `HandlerFailure.md` #9: the mapper turns a deserialisation failure into `InvalidMessageAction` | a body that is not JSON; `null` | a valid body | `handler-failure` | `InvalidMessageAction`, `JsonException` inside; `InvalidMessageAction` / maps |
| `KafkaConfiguration.md` #9–#13, #18: each `configHook` setting reaches the consumer; #12 raises the session timeout | each hook, the consumer built by `KafkaMessageConsumerFactory`, its `ConsumerConfig` read | no hook | `kafka-consumer` | every value set / unset; #12 45000 / 10000, the hook running after the group protocol applies the subscription's; #18 a different `greeter-…` id per consumer |
| `KafkaConfiguration.md` #15 and § *Available Strategies*, as they stood: *"CooperativeSticky: Not supported with Brighter's manual offset commits"*, *"will throw an `ArgumentOutOfRangeException`"* | `CooperativeSticky`: a message produced, received, acknowledged, the consumer closed; and a second consumer joining a three-partition topic | `RoundRobin`, the default | `kafka-consumer` | no exception: round trip done; rebalanced, the first consumer keeping 2 partitions and the second taking 1, 12 messages acknowledged across both / no exception: round trip done; rebalanced, 1 and 2. **Overturned** (ledger 79) |
| `RabbitMQConnectionStability.md` #1, as it stood: `retryWaitInMilliseconds: 250, // Wait between retries`; `RabbitMQConfiguration.md`'s list, *"Time to wait between retry attempts"* | #1's connection, its `ConnectionPolicyFactory` retry driven with `BrokerUnreachableException` | `retryWaitInMilliseconds: 500` | `rmq-connection` | 6 attempts (`connectionRetryCount: 5` is 5 retries), gaps 512, 1002, 2002, 4001, 8001 ms / 1001, 2001, 4002, 8002, 16002 ms: an exponential back-off from twice the value. **Overturned** (ledger 80) |
| `RabbitMQConfiguration.md` #4 (ledger 66) and its list: `circuitBreakTimeInMilliseconds` is how long Brighter waits before reconnecting | `30000`, one unreachable broker, a call 1 s later | `500` | `rmq-connection` | refused, `BrokenCircuitException` / goes through |
| `AWSSQSConfiguration.md` #7–#10 (ledger 65): each subscription makes the channel its heading names | each block's subscription, made by Brighter's `ChannelFactory` on LocalStack, then read back | the four against each other | `sqs-channels` | #7 a standard queue and topic, one subscription; #8 a FIFO queue and FIFO topic, content-based deduplication on the topic; #9 a standard queue, no topic; #10 a FIFO queue with deduplication, no topic |
| `AWSSQSConfiguration.md`, the sentences before #8 and #10, as they stood: the names *"must end with `.fifo`"* | #8 and #10, their names ending `.fifo` | the same with names that do not | `sqs-channels` | `greeting…_queue.fifo` and `…_topic.fifo`, FIFO / the same FIFO resources: Brighter appends `.fifo` (and turns each `.` into `_`). **Overturned** (ledger 81) |
| `CloudEventsSupport.md` #1: the Publication's `Source`, `Type` and `Subject` reach the message | the page's publication through the default `JsonMessageMapper<T>` | a Publication setting none | `cloudevents-mappers` | the three values in the header / `http://goparamore.io/`, empty, null |
| `CloudEventsSupport.md` #13–#15 (ledger 74): `Type`, a relative `Source`, `Subject` | the blocks' good examples | an absolute `Source` | `cloudevents-mappers` | `/orders/12345`, not absolute, in the header and as `"source"` in a structured envelope; the type and subjects as written |
| `CloudEventsSupport.md` #6: over the threshold the payload is stored and `dataref` set, and the receive restores it | 300 KB through `LargeOrderMapper` | 100 KB | `cloudevents-mappers` | `DataRef` set, the luggage stored, 48 bytes sent, restored / no `DataRef`, 102,479 bytes sent |
| `DefaultMessageMappers.md` #8, #12, #15: the default is `JsonMessageMapper<T>`, binary mode; #3, #9: `CloudEventJsonMessageMapper<>` named, structured mode; #1 relies on the first | each block's registration, the registry `AddBrighter` builds asked for `OrderCreated`'s mapper, which is run | an explicit mapper registered beside a default | `cloudevents-mappers` | `JsonMessageMapper<OrderCreated>`, default, attributes in the header; `CloudEventJsonMessageMapper<OrderCreated>`, default, the envelope in the body / the explicit mapper, not default |
| `DynamicMessageDeserialization.md` #3, #4, #7, #8 and the prose beside them: known types reach their handlers; `InvalidMessageAction` requests rejection as unacceptable; an ordinary exception is acknowledged | #4's callback on Brighter's Reactor, the in-memory transport, an invalid-message channel set; created, updated and an unmapped type enqueued | `ArgumentException` in the default arm | `dynamic-request-type` | each handler once; the unmapped message on the invalid-message channel / each handler once; the invalid-message channel empty and nothing left on the channel |
| `RoutingMultipleMessageTypes.md` #3 (header), #4 (body), #6 (handler router) | `Create`/`Update`/`Cancel`; each `eventType`; `US`, `UK` | an unknown value and no header; an unknown, a missing `eventType`, not JSON; `FR` | `routing-callbacks` | the matching types; `USOrderCreatedHandler`, `UKOrderCreatedHandler` / `InvalidMessageAction` throughout, the last with a `JsonReaderException`, a `JsonException`, inside; `DefaultOrderCreatedHandler` |
| `AsyncAPISupport.md` #2, #10 and § *Output Formats*: `asyncapi.json` and `asyncapi.yaml`; `.json` appended to a path without it; no `UseAsyncApi()` throws `InvalidOperationException` | #10's call | a path without `.json`; a host without `UseAsyncApi()` | `asyncapi-files` | both files, the title in each / `contracts.json`, `contracts.yaml`; `InvalidOperationException` |
| `AsyncAPISupport.md` #8, #9: a generator registered before `UseAsyncApi()` is not overridden | #9's registration of #8's `MySchemaGenerator` | no registration | `asyncapi-files` | `MySchemaGenerator`, no `Amount` in the payload / `NJsonSchemaGenerator`, `Amount` |
| `Routing.md` #3, #4 (ledger 73): the Id round trip | `request.Id.Value` | `request.Id` | `routing-id` (6.3) | the Id kept / `ArgumentException` |
| `Routing.md` #3, #4: the reply goes to the `ReplyAddress` the request carried, with its correlation id | a request replying to `replies.a`, mapped out and in, its reply mapped out and in | `replies.b` | `routing-reply` | the reply on `replies.a`, correlation kept / on `replies.b` |
| `ImplementingExternalBus.md` #3 (ledger 71), unstated: the V10 handler builds under `AddBrighter`'s defaults and `RETRYPOLICY` retries | a gateway failing once | the handler without `[UsePolicy]` | `external-bus-handler` | sent, the gateway called twice / `TimeoutException`, called once |

**Five claims overturned, each repaired at every recurrence (obligation 21):**
- **Ledger 77.** `MessageTransforms.md` #8 now gives `Compress` step 1 and `ClaimCheck` step 0, `RetrieveClaim` step 1 and
  `Decompress` step 0, its comments unchanged; the sentence before it and the one after it, and § *Step*, say the highest
  step runs first, on send and receive. No other page chains two transforms (`grep -rnE '\[(Compress|ClaimCheck|RetrieveClaim|Decompress)\(' contents/`
  → one step value per method everywhere else)
- **Ledger 78.** `DefaultMessageMappers.md` names the CloudEvents transform and the JustSaying and MassTransit packages
- **Ledger 79.** `KafkaConfiguration.md`'s strategy list describes `CooperativeSticky` as the incremental strategy, its
  *Limitation* paragraph is *Using CooperativeSticky Strategy*, #15 loses its *"This will throw"* and *"❌ Not supported"*
  comments, and the librdkafka sentence is gone: issue 4059 was closed on 2025-03-25, and 10.8.1 pins Confluent.Kafka 2.16.0
- **Ledger 80.** `RabbitMQConnectionStability.md` #1's comment and both of `RabbitMQConfiguration.md`'s descriptions say
  the back-off doubles from twice the value. Both RMQ packages compute it the same way
- **Ledger 81.** The two `AWSSQSConfiguration.md` sentences say AWS requires the suffix and Brighter appends it

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *The repairs (6.2)*, *For 6.4*: *"`KafkaConfiguration.md` #15: `CooperativeSticky` with manual commits throws"* | it does not, on a round trip or a rebalance (above); Brighter's own `When_a_partition_is_revoked_offsets_are_committed` runs both strategies at 10.8.1 | the list stands; ledger 79 |
| Ledger 72: Brighter ships *"`ClaimCheck`/`RetrieveClaim`, `Compress`/`Decompress`, `CloudEvents` and the JustSaying pair"* | the MassTransit pair as well, in `Paramore.Brighter.Transformers.MassTransit` 10.8.1 | row 72 stands; row 78 carries the full list |
| `docs/adr/0053` (Brighter's), the pipeline description sorting transform steps `OrderBy(s => s.Step)` | the 10.8.1 source sorts them `OrderByDescending`, for the description and for both pipelines | not this repository's to change; recorded for the ruling below |

**Read, not run:**
- `KafkaConfiguration.md` #2's `SslCaLocation` ternary: plain C#, and a Homebrew path the probe cannot vouch for
- `AWSSQSMigrateToV10.md` #5, #6: the AWS SDK's credential and region resolution, not Brighter's
- `AzureServiceBusConfiguration.md` #2 (ledger 75): no Service Bus emulator here; its routing keys pair by name with #1's publications
- `RoutingMultipleMessageTypes.md` #1, #7–#9: the same `message.Header.Type` switch as `DynamicMessageDeserialization.md` #4, which
  `dynamic-request-type` runs on the pump; #3 over RabbitMQ: header values pass through `RmqMessageCreator.ParseHeaderValue`, and the
  probe's are strings
- `DynamicMessageDeserialization.md`, *"logs it, increments the unacceptable-message count"*: the probe observes the acknowledgement only
- `CloudEventsSupport.md` #6's comment, *"Set the CloudEvents dataref attribute to the storage location"*: `ClaimCheckTransformer` sets
  `DataRef` to the id its store returns, which for S3 is the object's key

**Measured after 6.4:** `--report` → **994 / 757 / 208 / 29**, 118 findings, unmoved: the three blocks edited
(`MessageTransforms.md` #8, `KafkaConfiguration.md` #15, `RabbitMQConnectionStability.md` #1) stay BUILT. Two methods: the summary
line, and `cut -f1 | sort | uniq -c` of the rows. `pagelint` 0 errors / **185**, `--changed origin/master` 0 errors, `--plant` 5 / 5;
`linkcheck` 169 / 0; `symbolcheck` 24 entries, 14 silenced; `optioncheck` 0 mismatches, 59 tables, 533 rows (no marked table touched).

#### Second pass (6.5)

**Every phase-6 ledger row holds.** Rows 65–81 each give their *After* from their grep, run as written at `2f68a96`:
**0** for every row but 68, whose grep still counts `InMemoryOptions.md`'s `Publication`, which its factory takes (**1**).
6.1's six recurrence greps, run corpus-wide as written:

| Defect | 6.1's grep | At `f4d965f` | At `2f68a96` | Ledger |
|---|---|---:|---:|---:|
| `name:` for `subscriptionName:` | `grep -rnE 'name: new SubscriptionName' contents/` | 4 | **0** | 65 |
| Compression arguments | `grep -rnE 'compressionType\|CompressionType\.' contents/` | 2 | **0** | 70 |
| `circuitBreakerTimeInMilliseconds` | `grep -rn circuitBreakerTimeInMilliseconds contents/` | 3 | **0** | 66 |
| LibLog | `grep -rnE 'LogProvider\|\bILog\b\|: base\(logger\)' contents/` | 2 | **0** | 71 |
| Unshipped PII transforms | `grep -rnE 'RemovePII\|RestorePII' contents/` | 2 | **0** | 72 |
| Quorum on a subscription | `grep -rn 'QueueType\.Quorum' contents/` | 2 | **2** | — |

The last counts sites that are now right: `RabbitMQConfiguration.md` #5 builds on `RMQ.Async`'s `RmqSubscription`, the
one that takes `queueType`, and the list beside it names the value.

**Widened, by shape rather than words:**

| Defect | Widened grep | Hits | Reading |
|---|---|---:|---|
| 65, a subscription's argument misnamed | `^\s*name: ` | 3 | `RabbitMQConfiguration.md` #6, twice: `Exchange(name: …)`, whose constructor takes `name` (`RmqMessagingGatewayConnection.cs:195` in RMQ.Async, `:203` in RMQ.Sync); `OutboxArchiver.md`, a YAML key |
| 66, the circuit-break argument | `circuitBreak\w*Time\|CircuitBreakerTime`, case-blind | 4 | each `circuitBreakTimeInMilliseconds`, the name 10.8.1 takes |
| 67, a `RabbitMQ`-prefixed type | `\bRabbitMQ[A-Z]\w+` | 32 | every one a page's file name in a link |
| 68, a plain `Publication` given to a factory | `new \w+ProducerRegistryFactory\(` across all ten factories the corpus names, within 40 lines before and 14 after a `new Publication` | 8, on 5 pages | `InMemoryOptions.md` (3), `InMemoryTransport.md` (2), `Monitoring.md`, `V10MigrationGuide.md`: `InMemoryProducerRegistryFactory`, which takes `Publication`. `MSSQLMessageBroker.md` #1: `MsSqlProducerRegistryFactory(RelationalDatabaseConfiguration, IEnumerable<Publication>)` at 10.8.1, BUILT |
| 69, a required member left to a comment | `new \w+(Configuration\|Connection\|Options)\s*\{` holding only `/* ... */`, `...` or a comment | 5 | `RmqMessagingGatewayConnection { /* ... */ }` on `DefaultMessageMappers.md` #1 and `CloudEventsSupport.md` #1, both BUILT; `DynamoDbConfiguration { /* ... */ }` on `DistributedLock.md`, `DynamoDbDistributedLock.md`, `OutboxArchiver.md`, which declares no `required` member at 10.8.1 (`DynamoDbConfiguration.cs:71`, every argument optional). Not recurrences |
| 70, compression's names | `CompressionType\|\bGzip\b\|compressionLevel\|CompressionLevel` | 2 | `Compression.md`: `CompressionMethod.GZip` and `CompressionLevel.Optimal`, both live |
| 71, a V9 logger | `LibLog\|\bILog\b\|ILogger\w*<…> \w+\)\s*:\s*base\(\|: base\(\w*[lL]og` | 0 | |
| 72, a transform Brighter does not ship | every attribute within four lines above a `MapToMessage`/`MapToRequest`; every class deriving `WrapWithAttribute`, `UnwrapWithAttribute` or `TransformAttribute` | 24; 1 | `ClaimCheck`, `RetrieveClaim`, `Compress`, `Decompress` only, on 9 pages; `MessageTransforms.md` #5, a copy of the shipped `ClaimCheck` |
| 73, the `Id` record | `JProperty\("Id"\|\.Id = Guid\.\|Id = Guid\.NewGuid\(\)` | 7 | the two repaired `request.Id.Value`; `DefaultMessageMappers.md` #2's `Guid.NewGuid().ToString()`, a `string`, which `Id` converts from; `CloudEventsSupport.md`'s `Guid` pairs, V9 forms (SKIPPED) beside their V10 `Id` forms (BUILT) |
| 74, a relative `Uri` | `new Uri\("/` | 1 | the repaired `UriKind.Relative` |
| 75, `builder` inside a method | the 30 lines before each of 140 `builder.Services` sites, for a method header | 15 headers | none takes an `IServiceCollection`; 6.3's reading stands |
| 76, `InitializerParams`' signature | `InitializerParams` | 5 | `MessageTransforms.md` #4 as repaired; two overrides returning `object[]` (`MessageTransforms.md` #5, `BuildingAPipeline.md`), as 10.8.1's own overrides do (`RequestLoggingAttribute.cs:49`); two prose mentions |
| 77, transform step order | `\bsteps?\b` within 80 characters of `first\|order\|reverse\|ascending\|descending\|lowest\|highest\|lower\|higher`, case-blind, either way round, read for transforms | 57 lines, 6 on transforms | `MessageTransforms.md`'s five (§ *Step*, the lead-in to *Chaining Multiple Transforms*, #8's two `// First` comments, the sentence after #8), as repaired; **`HandlingLargeMessages.md`** § *Troubleshooting*: *"The claim check unwraps at `step: 0` by convention because it has to run before anything that reads the body"*, which takes the lowest step to run first. **A recurrence**, repaired (below). The rest are handler or Darker pipelines, where the lowest step is the outermost, a different mechanism the pages state correctly |
| 78, what Brighter ships | `(brighter\|it) (ships\|provides\|includes) .{0,60}transform`, case-blind | 1 | the repaired sentence |
| 79, `CooperativeSticky` | `CooperativeSticky` | 4 | `KafkaConfiguration.md`, as repaired |
| 80, the retry wait | `retryWait\|retry ?wait\|wait between\|between retr`, case-blind | 4 | the three repaired sites and `RabbitMQConfiguration.md` #4's argument |
| 81, the FIFO suffix | `\.fifo`; `fifo` within 60 characters of `suffix\|end in\|end with` | 10; 2 | the two repaired sentences; the rest are names in blocks |

**The recurrence (ledger 77, extended).** `HandlingLargeMessages.md` now says a `[RetrieveClaim]` sharing `MapToRequest` with
another transform takes the highest step, because Brighter runs a mapper's transforms from the highest step to the lowest,
on receive as on send, and that `step: 0` is the convention when it is alone. Its other clause, that the step ordering can
put the retrieve *"after something that already tried to deserialize"*, is gone: the mapper deserializes after every
transform, so no ordering of transforms explains a `Claim Check <guid>` body; a missing `[RetrieveClaim]` does. The claim the
sentence now makes is `transform-order`'s, run at 6.4 (receive, highest step first). Prose only; no block moves.

**Rulings, recorded.** Session 132, on 6.3's spot check:
- Ledger 73–76 stand. 6.3's extension of row 69 with the `KafkaConfiguration.md` recurrence, rather than a new row, stands
- `MessageMappers.md` #3, #5, SKIPPED under 017's *"a method signature shown for comparison, with no body or class to compile
  in"* rather than D21: **agreed**
- `MessageTransforms.md` #8's unshipped `RemovePII`/`RestorePII`, replaced by shipped transforms: **agreed**. A PII-protecting
  transform is filed upstream as **Brighter #4649** (envelope encryption, a pluggable key store, the key's reference in the
  headers)
- `DefaultMessageMappers.md`'s `SensitiveOrder`/`SecureLargeOrderMapper` and `SensitiveData`/`EncryptedMapper`: **kept, labelled
  as the reader's own types**, at `254e8a7` (the prose before blocks 7 and 11, and their comments). That commit's sentence on
  the shipped transforms was overturned at 6.4 (ledger 78)

Session 134, on 6.4's pause:
- Ledger 77–81 stand. **(a)** Ledger 79, `CooperativeSticky` works with Brighter's commits: **agreed**
- **(b)** Transform order: **10.8.1 is as intended, and no upstream issue is filed.** The transforms nest: you enter one, it
  hands on to the next lower step, and control comes back to it when that returns, so the highest step runs first on send
  and on receive (`TransformPipelineBuilder.cs:366`, `:395`, `OrderByDescending`). 6.4's *"`docs/adr/0053` … `OrderBy`"* row
  is a misreading: that ADR's `OrderBy` only formats a handler pipeline for a log line. Read against the ruling,
  `MessageTransforms.md`'s three sentences (§ *Step*, the lead-in to *Chaining Multiple Transforms*, the sentence after #8)
  each give one order, highest step first, and the last says *"on receive as on send"*; none sets a sender's order against a
  receiver's, so none is changed. `HandlingLargeMessages.md`, above, now says the same
- **(c)** `RetryWaitInMilliseconds`' doc comment, which says a fixed wait where both RMQ gateways wait `base × 2^attempt`, is
  filed upstream as **Brighter #4650**
- 6.4 started and removed two docker containers for its runs, `apache/kafka:4.0.2` and `localstack/localstack:4.4`

**`symbolcheck` rows (6.1).** Each candidate resolved with `git grep -lw` at `10.8.1` and Brighter's `master`
(`f27d3cc2b`): `LogProvider`, `ILog`, `circuitBreakerTimeInMilliseconds`, `CompressionType`, `RemovePII`, `RestorePII` and
`RabbitMQProducerRegistryFactory` are **0** at both. None earns a row, on 5.5's reading, so nothing is put to the
maintainer and `symbolcheck` stays 24 entries:
- `circuitBreakerTimeInMilliseconds`, `RemovePII`, `RestorePII`, `RabbitMQProducerRegistryFactory` were never a released
  name (`git grep -lw` → 0 for each at `8.1.1399` and `9.33`, the last V8 and V9 tags), and the watchlist holds *"names known
  dead in a released product, each with the live replacement"*. Each is 0 sites now, and the blocks that held them build,
  so `blockcheck` catches their return in those blocks
- `compressionType:` is an argument name, and `CompressionType` is live in `Confluent.Kafka` 2.16.0, which the Kafka package
  brings onto the pin: a row would police a name a Kafka producer's configuration can rightly use
- `LogProvider` and `ILog` are LibLog's, live at `8.1.1399` (73 and 71 files) and gone at `9.33`. They stood in one block and
  are 0 sites now, as `ConfigurationCommandType.StopChannel` was at 5.5
- `new JProperty("Id", request.Id)` is an expression, not a name a watchlist matches, as `IHandleRequests Create(Type)` was

**Put to the maintainer: `RabbitMQConnectionStability.md` #3 (Serilog).** No earlier task ruled on it (`grep -n Serilog` over
this file: 6.2's two mentions, 6.3's one and the stay-FAILED row, none a ruling). The block configures Serilog with a Seq sink and hands it to
`AddLogging`; `Log`, `LoggerConfiguration`, `WriteTo.Seq` and `AddSerilog` come from `Serilog`, `Serilog.Sinks.Console`,
`Serilog.Sinks.Seq` and `Serilog.Extensions.Logging` (or `.Hosting`), none of them on a pin. Adding a package to a pin is the
maintainer's, so the choice is a ruling, not a repair: add the packages to `tools/blockcheck/refs/refs.csproj` (553
assemblies move), rewrite the block to `Microsoft.Extensions.Logging` alone, skip it with a reason, or leave it FAILED and
listed as it is. Its comment, *"Blocked events will be logged automatically"*, is a behavioural claim no task has run.

**Off-group term:** `HandlingLargeMessages.md`, prose, in S4's *Using an External Bus* section but outside its seventeen
FAILED-bearing pages. **Measured after 6.5:** `--report` → **994 / 757 / 208 / 29**, unmoved, 118 findings (6.6's to
baseline); two methods, the summary line and `cut -f1 | sort | uniq -c` of the rows. `pagelint` 0 errors / **185**,
`--changed origin/master` 0 errors; `linkcheck` 169 / 0; `symbolcheck` 24 entries, 161 pages, 14 silenced. No marked option
table touched.

#### Addendum: the Serilog ruling

**Ruled (after `3671922`): `RabbitMQConnectionStability.md` #3 is rewritten to `Microsoft.Extensions.Logging` alone, and the
pin is unmoved.** The block now adds a console provider and an `Information` filter on the
`Paramore.Brighter.MessagingGateway.RMQ` categories, and the sentence before it says any `ILoggerProvider` (Serilog, Seq,
OpenTelemetry) receives the same lines. `services` comes from a new unit, `RabbitMQConnectionStabilityContext` (one member,
typed `IServiceCollection`), mapped in `pages.tsv`; blocks 1 and 2 are now compiled with it and still build. **#3 is BUILT**
(`blockcheck --explain`: 0 diagnostics), and its row is gone from § *Blocks that stay FAILED*. It read: *"`CS0103` `Log`,
`services`; `CS0246` `LoggerConfiguration` | Serilog (and its Seq sink) is on no pin, and the page names it alone. Adding it
moves the pin's 553 assemblies and is not a repair of this page: put to 6.5 | 6"*.

**The block's claim, run.** Its comment said *"Blocked events will be logged automatically"*, and the section opened *"Brighter
automatically logs blocked and unblocked events"*. The gateway logs through `static readonly ILogger s_logger =
ApplicationLogging.CreateLogger<RmqMessageGateway>()` (`RmqMessageGateway.cs:57`, RMQ.Async; `:54`, RMQ.Sync), and
`ApplicationLogging.LoggerFactory` is a provider-less `LoggerFactory` until `BuildCommandProcessor`
(`Extensions.DependencyInjection/ServiceCollectionExtensions.cs:776`) or `BuildDispatcher` (ServiceActivator's
`ServiceCollectionExtensions.cs:141`) replaces it with the container's. A real broker alarm drove the events:
`rabbitmqctl set_vm_memory_high_watermark 0`, a publish under it, then `0.4`.

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `RabbitMQConnectionStability.md` #3, as it stood: *"Blocked events will be logged automatically"* once a provider is added | #3 as rewritten, a capturing provider beside the console; the producer registry made in `AddProducers`' configure, as every RMQ page shows it; the command processor resolved, then `PostAsync`, the alarm, a publish under it, the alarm cleared | the same, but the producer sends once before the command processor is resolved | `rmq-logging`, against `rabbitmq:3.13-management` | the `Warning` *"… Subscription to amqp://\*\*\*\*\*@localhost:5679/ blocked. Reason: low on memory"* and the `Information` *"… unblocked"* reach the provider, with every other RMQ line / **nothing** from any RMQ category reaches it, not even the later `Published message` lines, while `rabbitmqctl list_connections state` reads `blocked` in both. **Conditional** (ledger 82) |

The hypothesis given with the ruling, that creating the registry during configuration binds the gateway's logger early, is
**not** what happens: the case arm creates it there, and its lines arrive. A logger is bound when its class first logs, at any
level, so only a class that *logs* before the swap is affected. The Dispatcher half is read from source, not run.

- **Ledger 82.** `RabbitMQConnectionStability.md`'s section now says the gateway logs *"through your application's logging"*,
  and a paragraph after #3 says the classes take the container's `ILoggerFactory` once the command processor or Dispatcher is
  built, and that a class which logs before then keeps a provider-less logger for the life of the process. **Recurs on
  `Logging.md`** (S5, off-group): *"If you use the DI extensions, there is nothing to do"*, with `AddBrighter` and
  `AddConsumers` resolving the factory *"as they build"*. It now says the command processor and the Dispatcher each resolve it
  when first resolved, adds the same consequence, and its *Nothing appears* pitfall says to resolve the command processor before
  using a producer directly. Prose only on that page; its blocks are unchanged. `grep -rniE 'logged automatically|automatically
  logs?' contents/` → 0; `grep -rn 'ApplicationLogging\|ILoggerFactory' contents/` → `Logging.md` and this page only
- **Ledger 83.** The two sample lines showed `amqp://localhost:5672`; the gateway logs `GetSanitizedUri()`, which masks the
  credentials and keeps `Uri.ToString()`'s trailing slash (`RmqMessagingGatewayConnection.cs:137`), as the probe printed. They
  now read `amqp://*****@localhost:5672/`, the page's own URI (`guest:guest`) masked

**Upstream, for the maintainer (not filed).** Arguably yes: every Brighter type that logs holds a `static readonly` logger from
whatever factory `ApplicationLogging` held at the type's first log, so any Brighter code that logs before the command processor
or Dispatcher is resolved loses its lines for the life of the process, silently. A logger that resolves the current factory
on each call, or installing the factory when the container is built rather than when the command processor is, would remove
the ordering trap the two pages now describe.

**Measured after the addendum:** see the return; `--report` → **994 / 758 / 207 / 29**, 119 findings (#3 joins 6.6's
built-not-baselined; blocks 1 and 2 stay in it with their new scaffold). `pagelint` 0 errors / **184** (the old #3's
using-directive warning gone).

#### Phase 6 closed (6.6)

**Baselined.** `baseline.tsv` lists every BUILT block on the seventeen S4 pages, **128** rows, replacing their
**14** earlier ones (§ the file's own header). Seven keep their ref, their text, ordinal and scaffold unchanged:
`AWSSQSConfiguration.md` #1, #3–#6 (`TransportConfigurationContext.cs`, which the phase did not touch) and
`AWSSQSMigrateToV10.md` #1, #7. Of the other seven earlier rows, five gained their page's unit (`AsyncAPISupport.md`
#5, `CloudEventsSupport.md` #8, #10, `DefaultMessageMappers.md` #4, #5) and two kept a unit the phase extended
(`MessageMappers.md` #4, `S3LuggageStore.md` #2); with the 114 new rows, they are at **`b6a23f8`**, the phase's last
page commit. The rows were appended as phase 5's were, in `--report`'s order, so the diff is 14 out and 128 in. Text
unchanged was judged against a `git worktree` at `e46f47e`, the base the branch was rebased onto, never a stash: all
fourteen read the same there, and no S4 page gained or lost a block. **Two methods agree:** `grep -v '^#' baseline.tsv
| grep -vc '^$'` → **758**, and `--report`'s BUILT → **758**. `--report` exit **0**, **0 findings**.

**Rule 8.** `pagelint` → **0 errors**; the one `attr-mismatch-intended` marker stays on `PipelineValidation.md` #7, and
`--plant` → 5 / 5.

**Readings (obligation 19).** `git diff -U0 e46f47e HEAD -- contents`, 223 hunks against fence ranges: 18 pages,
**101** changed C# blocks on 16 of them, all on S4. Two methods: the hunks, and S4 blocks whose text is not on their
page at `e46f47e` (a `git worktree`), **101** on the same 16 pages. `HandlingLargeMessages.md` and `Logging.md` changed
prose only.
- **AC9.** The hunks add `// ...` at **23** lines in **17** blocks: `AWSSQSConfiguration.md` #2;
  `AzureServiceBusConfiguration.md` #1, #2; `DynamicMessageDeserialization.md` #2, #7; `KafkaConfiguration.md` #1–#3,
  #6–#8; `RabbitMQConfiguration.md` #1–#4, #6; `RoutingMultipleMessageTypes.md` #2. They remove **6**, as fragments
  were made whole (one each on `AsyncAPISupport.md`, `HandlerFailure.md`, `MessageTransforms.md` and
  `RabbitMQConnectionStability.md`) and two reworded on `DynamicMessageDeserialization.md`; net **+17**, which a count
  of `// ...` lines in C# blocks at both refs confirms. All 17 blocks are BUILT, so `--explain` has nothing to report
  for any of them: no added omission hides an error
- **AC10.** § *The behavioural claims (6.4)*: **twenty-five** claims run with a control against the released 10.8.1
  packages, in fourteen probes (thirteen new and 6.3's `routing-id`), two of them against a broker. § *Addendum*: one
  more, `rmq-logging`, against a real RabbitMQ alarm. Five were overturned at 6.4 (ledger 77–81) and one found
  conditional at the addendum (ledger 82). Read and named, not run: 6.4's six (`KafkaConfiguration.md` #2's ternary,
  `AWSSQSMigrateToV10.md` #5, #6, `AzureServiceBusConfiguration.md` #2, `RoutingMultipleMessageTypes.md` #1, #7–#9,
  the unacceptable-message count, `CloudEventsSupport.md` #6's `dataref` comment) and the addendum's Dispatcher half. No
  changed block lies off S4. The off-group prose makes claims already run: `HandlingLargeMessages.md`'s is
  `transform-order`'s, `Logging.md`'s is `rmq-logging`'s

**Gates against 6.1.**

| Gate | Predicted (6.1) | Measured at the close | |
|---|---|---|---|
| corpus | 992–1,000 | **994** | in band, **not by the mechanism named** (below) |
| BUILT | 740–770 | **758** | in band |
| FAILED | 201–216 | **207** | in band: 323 − 125 + 9 |
| SKIPPED | 29–36 | **29** | the floor, by a different reason (below) |
| `pagelint` | 0 errors / 185–210 | **0 / 184** | **below the band by 1** (below) |
| `symbolcheck` | unmoved | **24, 14**, exit 0 | as predicted; 6.5 found no candidate earns a row |
| scaffold rule | 74–81 units | **80**, 0 violations, 0 compile errors | in band: 13 new units; the Kafka, RabbitMQ and Service Bus pages each took their own rather than sharing `TransportConfigurationContext` |
| pin rule | 4 pages, 0 violations | **4, 0**; the V4 pin 28 blocks, **25** built | as predicted: `S3LuggageStore.md` #1 BUILT on a `credentials` value |
| assemblies | 553 | **553** on each pin | as predicted, **not by the mechanism named** (below) |
| `optioncheck` | 0 / 59 / 529 | **0 / 59 / 533** | the +4 rows are bugfix 0003's (10.8.1), merged before the rebase; no S4 marked table touched |
| `linkcheck` | 168 / 0 | **169 / 0** | the +1 file is bugfix 0003's record, merged before the rebase |
| `versioncheck` | 0 of 18 | **0 of 18** | as predicted; 6.2's *18 of 18 stale* at 10.8.0 was cleared by bugfix 0003 moving the tutorial pins to 10.8.1 |
| shape · redirects · `--verify` | unmoved | 161, widest 12 of 20 · 77, 7858 bytes · 161 = 161 | as predicted |
| `pagelint --plant` | 5 / 5 | 5 / 5 | as predicted |
| off-group | **0** | **0** changed blocks off S4; prose on 2 pages | as predicted for blocks (below) |

**Every number in band but `pagelint`, and four by a mechanism other than the one 6.1 gave.** One account:
- **The corpus.** 6.1's mechanism was *"splits only, up to +8"*. No fence was split: each set of alternatives was made
  whole in its one fence (§ *The hard blocks (6.3)*). The +2 is `HealthChecks.md` #2 and #3, added and baselined by
  bugfix 0003 (#206) and on `master` before the rebase. The phase's own movement is **0**
- **BUILT, FAILED, SKIPPED.** S4's 125 FAILED ended **114 BUILT, 9 FAILED, 2 SKIPPED**: 642 + 2 + 114 = **758**, 323 −
  125 + 9 = **207**, 27 + 2 = **29**. The nine stays are inside 3–18 and are all *same-page* and listed (§ *Blocks that
  stay FAILED*; two methods, the nine phase-6 blocks there and `--report` grepped on the seventeen pages, 9): the seven
  6.1 named, and `AsyncAPISupport.md` #7 and `Routing.md` #4, hidden behind *parse* (6.3). No running example was
  declared, so none moved a page's blocks to *same-page*, which phase 5's lesson guarded against. The two skips are
  `MessageMappers.md` #3 and #5, under 017's *"a method signature shown for comparison"*, not D21 as 6.1 said; the
  third D21 candidate, `MessageTransforms.md` #4, was made whole (ledger 76), and none of `KafkaConfiguration.md`'s six
  `configHook:` fragments was skipped
- **`pagelint`.** S4's 94 warnings fell to **7** (`--per-page` on the seventeen pages): the 5 6.1 said stay
  (`AWSSQSMigrateToV10.md` #7; `CloudEventsSupport.md` #7, #9, #11, #12), and the 2 new skips. 271 − 94 + 7 = **184**,
  and off-group 0. The band's floor, 271 − 94 + 5 + 3, assumed three new skips; two came. At 6.3 the gate read 185,
  because `RabbitMQConnectionStability.md` #3 stayed FAILED with its warning, a block 6.1 counted as one its repair
  would edit; the Serilog ruling rewrote it, and the gate fell to 184
- **Assemblies.** 6.1 said *"every package S4 names is pinned"*; `RabbitMQConnectionStability.md` #3 named Serilog,
  which is on no pin (6.2). The figure held because the ruling rewrote the block to `Microsoft.Extensions.Logging`
  rather than growing the pin, not because the claim was true
- **`optioncheck`, `linkcheck`, `versioncheck`** moved, or were cleared, by bugfix 0003, merged after 6.1 measured and
  rebased under the branch before 6.3. None moved inside the phase

**What 6.1 named, and what the phase found.** 6.1 read S4's 15 *defect* blocks as 8 API defects and predicted five
ledger rows from them (65, 66, 70, 71, 72) and one `using`. The phase wrote **nineteen** (65–83), all repaired at every
recurrence: **3** hidden behind *page-type* (67–69), **4** behind *parse* or *same-page* (73–76), and **7** that only a
run could find (77–83), five overturned at 6.4 and two at the addendum. Two recurred off the seventeen pages, in prose:
77 on `HandlingLargeMessages.md` (S4's section) and 82 on `Logging.md` (S5). 6.1's off-group term was zero *from its
greps*, and those greps held; the two recurrences are of defects no compile reading could name. None of the measured
numbers is adopted. The lesson for phase 7: a compile reading predicts the blocks that build, not the claims they make,
so predict the off-group term from the behavioural claims as well, as recurrences the reading cannot see.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| `2f68a96`'s commit subject: *"26 rows"* in 6.4's *Claim / Case / Control* table | **25**: the rows between the table's header and the blank line after it, and the *Probe* column's entries, each 25 | the table stands; a commit message is not this file's text |
| § *The repairs (6.2)* and § *The hard blocks (6.3)*: four predictions *"reconciled at 6.6"* (assemblies, `versioncheck`, `pagelint`'s band, D21) | reconciled above | the predictions stand |

**Rulings, recorded.** Session 135, on the Serilog addendum:
- Ledger **82** (the conditional logging claim, and the repairs on `RabbitMQConnectionStability.md` and, off-group,
  `Logging.md`) and **83** (the sanitised URIs): **accepted**
- The addendum's *"Upstream, for the maintainer (not filed)"*: **filed** as **Brighter #4690**, the static logger
  bound at a type's first log, related to #4184 and #4588
- `RabbitMQConnectionStability.md` #3's row in § *Blocks that stay FAILED*, removed once the block built rather than
  annotated: **noted**, and it stands

**Upstream, filed during the phase:** **#4646**, **#4647** and **#4648** (bugfix 0003, recorded there); **#4649**, a
PII-protecting transform (session 132); **#4650**, the RMQ `RetryWaitInMilliseconds` doc comment (session 134); **#4690**,
the static logger binding (session 135).

**Off the prediction.** 6.4 started and removed two containers (`apache/kafka:4.0.2`, `localstack/localstack:4.4`), the
addendum one (`rabbitmq:3.13-management`, `probe-rmq-logging`); none is left (`docker ps -a`). `tools/README.md` rows 2
and 9 now give the close's figures; rows 1 and 6 already gave bugfix 0003's, which are the close's.

### Phase 7 — S5, the remaining five sections *(7 tasks, one PR)*

- [x] **Task 7.1:** Predict phase 7's gate movement
  - Input: § *The groups*, S5; recurrence greps for S5's defects
  - Output: § *Phase 7 as executed*, *Predicted*, as 3.1's
- [x] **Task 7.2:** Repair S5's reachable blocks
  - Input: S5's table, reachable rows; the V4 pin for `DistributedLock.md` #2 and
    `DynamoDbDistributedLock.md` #1, #2
  - Output: as 3.2's, for S5. The three V4 blocks are BUILT or listed with a diagnostic that is not
    `CS0234 … V4` (AC11, with 3.3 and 6.2)
- [x] **Task 7.3:** Repair S5's hard blocks
  - Input: S5's table, hard rows; Brighter `10.8.0` source, the pinned release
  - Output: as 3.3's, for S5
- [x] **Task 7.4:** Repair `TestDoubleOptions.md`'s Shouldly blocks (P0-8)
  - Input: design § *Page Repair Rules*, the Shouldly row; 2.7's pin
  - Output: `using Shouldly;` in each of the six blocks, and one sentence naming the package.
    `grep -c Shouldly contents/TestDoubleOptions.md` → **≥ 7**, from **0**; the six BUILT or listed
- [x] **Task 7.5:** Run S5's behavioural claims (AC10)
  - Input: S5's changed blocks
  - Output: a *Claim / Case / Control* table, each row naming `probe/s5/<name>/Program.cs`
- [x] **Task 7.6:** Second pass for S5
  - Input: 7.1's recurrences; rulings asked
  - Output: as 3.6's
- [ ] **Task 7.7:** Close phase 7
  - Input: § 1 obligations 6, 7, 19
  - Output: as 3.7's

### Phase 7 as executed

**Measured at phase start, `dc09559`** (#210's merge). `--report` → **994 / 758 / 207 / 29, 0 findings**,
exit 0; `--classify` → 31 parse, 12 built-by-using, 42 defect, 37 same-page, 12 values, 73 page-type,
settled in 3 rounds. 553 reference assemblies on each pin, 28 blocks on the V4 pin (25 built); scaffold rule
80 units, 0 violations, 0 compile errors; pin rule 4 pages. `pagelint` 0 errors / **184** warnings (39 pages),
`--plant` 5 / 5; `symbolcheck` 24 entries, 14 silenced; `linkcheck` 169 / 0; `versioncheck` 0 of 18 across 5
pages, so no Brighter release beyond the pinned 10.8.1; `optioncheck` 0 / 59 / 533; shape 161 pages, widest 12
of 20; redirects 77, 7858 bytes.

#### Predicted (7.1)

**S5 at `dc09559`**: `groups.py` gives 22 pages, **134** FAILED (30 parse, 12 built-by-using, **32** defect,
3 same-page, 9 values, **48** page-type), 41 BUILT, 10 with nothing BUILT. Two methods: the FAILED column, 134;
`grep` over `--report` on the twenty-two pages, 134, with 41 BUILT and 7 SKIPPED (`FAQ.md` #15,
`V10MigrationGuide.md` #1, #4, #6, #8, #14, #18). The S5 pages hold **182** C# blocks. Off S5, `--report` has
73 FAILED on 29 pages (207 − 134).

**The inventory.** *Reachable*, **69**: 12 built-by-using, 9 values, 48 page-type. *Hard*, **62**: 30 parse,
32 defect. *Same-page*, **3**: `V10MigrationGuide.md` #3, #10, #17. *V4*, **3**, all *values* on the V4 pin:
`DistributedLock.md` #2 (`services`), `DynamoDbDistributedLock.md` #1 (`dynamoDb`) and #2 (`services`).
*Shouldly*, **6**: `TestDoubleOptions.md` #1, #5–#9, each classed *defect* on `CS1061 ShouldBe…`. Two methods: the
six `CS1061` rows of `--classify`, and a scan of the page's blocks for `.Should(Be|NotBe|Contain|Throw)`,
**17** calls in the same six blocks.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| S5's table: `MongoDBOutbox.md` **1** defect, **0** page-type; S5 **33** defect, **47** page-type | **0** and **1**; S5 **32** and **48**. #5 was repaired off-group at task 4.3 (ledger 36) and now fails only on the reader's `AddGreeting`, `GreetingMade`, `Person` | the table stands, being the instrument's at `f3f685f`; this row is the reading at `dc09559` |
| Task 7.3: *"10.8.0 source"* | the pins are **10.8.1** (bugfix 0003) | the reading below is against the 10.8.1 tag; the task's text is not this task's to edit |
| S5's table: **32** defect blocks (as measured) | the second compile's diagnostics (below): **5** are API defects, **3** a pin limit already ruled, **6** Shouldly's missing `using`, **2** 017's mapper defect recurring, **5** alternatives in one fence, **3** types and statements in one fence, **7** fragments, and **1** a source-generated quote | the table stands; the repairs follow the reading |

**Reading S5's 32 defect blocks** with `probe/second.py` over the twenty-two pages (527 diagnostic lines),
each against the 10.8.1 tag:

| Page | # | Second compile | Reading against the pin | Repair |
|---|---:|---|---|---|
| `FAQ.md` | 2, 14, 16 | `CS1503` ×2 on `UseResiliencePipeline(1, "…")`; #2 also `CS0592` `RequestLoggingAsync` | **defect**, twice: `UseResiliencePipelineAttribute(string policy, int step)` (`UseResiliencePipelineAttribute.cs:68`), so the arguments are reversed; and the attributes decorate the class, where `[AttributeUsage(AttributeTargets.Method)]` (`:52`) allows only a method. The handlers are `RequestHandlerAsync`, so the method is `HandleAsync` and the attributes their `…Async` twins (`CLAUDE.md` § *Handler attributes match their handler*). Rule 8 reads methods, so a class-level attribute never reaches it | 7.3 |
| `FAQ.md` | 1 | `CS0452` | **defect**: `class MyHandlerBase<T> : RequestHandlerAsync<T>` without the `where T : class, IRequest` the base requires | 7.3 |
| `CommandProcessorConfigurationReference.md` | 20 | `CS1520`, `CS0118` | **defect**: a quote of `JsonSerialisationOptions`' static constructor showing **4** of the **12** converters 10.8.1 adds (`JsonSerialisationOptions.cs:33–44`; the 8 missing are `SubscriptionNameConverter`, `RoutingKeyConvertor`, `ChannelNameConverter`, `BaggageConverter`, `IdConverter`, `PartitionKeyConverter`, `TraceStateConverter` and `TraceParentConverter`). The prose before #21 says the hook takes an `Action<JsonSerialisationOptions>`; it takes `Action<JsonSerializerOptions>` (`ServiceCollectionExtensions.cs:866–867`), the class being static | 7.3: the listing brought to the tag, and SKIPPED under D21 if it cites the file and line, or made whole; the prose with it |
| `ConfiguringOpenTelemetry.md` | 1, 5, 6 | `CS0234` `Paramore.Brighter.Extensions.Diagnostics`; `CS1929` `AddOpenTelemetry` | **a pin limit, already ruled**: neither `Paramore.Brighter.Extensions.Diagnostics` nor `OpenTelemetry.Extensions.Hosting` is on a pin. 017's ruling (2) of 2026-09-28 (`spec/017-compile_repairs/tasks.md` § *Phase 5 as executed*): *"No pin change … stay FAILED as a pin limit, each compiled in scratch"*. `Telemetry.md` #1, #6–#9 and `PostgreSQLMessageBroker.md` #8, tranche pages, fail on the same `CS0234` | stay FAILED and listed, each compiled in scratch against the released packages; a ruling to pin would move all nine |
| `TestDoubleOptions.md` | 1, 5–9 | `CS1061` `ShouldBe…` | **Shouldly with no `using`** (ledger 2, D4): pinned at 4.3.0 since phase 2, never named by the page | 7.4 |
| `MigratingToNullableReferenceTypes.md` | 22 | `CS0535` `MapToMessage` | **017's mapper defect recurring**: an `IAmAMessageMapper<T>` with one of its two members, under a `// ...` that declares nothing about the member | 7.3 |
| `OutboxArchiver.md` | 4 | `CS0535` `MapToRequest` | the same, the other member | 7.3 |
| `NullableReferenceTypes.md` | 1, 10, 11 | `CS0128` | **alternatives**: a *before* and an *after*, each declaring the same local. #1's second line is the `CS8600` warning the page teaches | 7.3, split or made whole |
| `FAQ.md` | 8 | `CS0111` | **alternatives**: two handlers' `HandleAsync` in one fence | 7.3, split or made whole |
| `TurningOnReplayOnSeen.md` | 4 | `CS0111` | **alternatives**: a ❌ and a ✅ `Handle` | 7.3, split or made whole |
| `MigratingToNullableReferenceTypes.md` | 4, 7 | `CS0825`, `CS0116` | **types and statements in one fence**: a class, then `var command = …` | 7.3, split or made whole |
| `NullableReferenceTypes.md` | 12 | `CS0825`, `CS0116` | the same | 7.3, split or made whole |
| `BrighterOutboxSupport.md` | 1 | `CS1520` | a fragment: a handler's constructor without its class | 7.3 |
| `BrighterOutboxSupport.md` | 2 | `CS1106` | a fragment: an extension method outside a static class | 7.3 |
| `CausationTrackingStores.md` | 2 | `CS0535` ×11 | a fragment: `IAmAnOutboxSync<,>` implemented in part, the rest declared `// ...` | 7.3, made whole or skipped |
| `CommandProcessorConfigurationReference.md` | 10 | `CS0117` `Environment.IsDevelopment` | a fragment: a `Startup` method whose `Environment` is the class's `IWebHostEnvironment`, so `System.Environment` binds | 7.3 |
| `Logging.md` | 4 | `CS1061` `INeedAHandlers.Build` | a fragment: the builder's chain elided as `// ...` | 7.3 |
| `InMemoryOptions.md` | 3 | `CS7036`, `CS1729` ×2, `CS0104` `ChannelFactory` | a fragment: four constructors given `/* production config */` for arguments, and a `ChannelFactory` the pin's MQTT and RMQ.Sync packages both declare | 7.3, made whole |
| `SweeperCircuitBreaking.md` | 1 | `CS1501` `AddProducers` | a fragment: `.AddProducers(/* producer configuration */)` | 7.3, made whole |
| `Logging.md` | 5 | `CS8795` | **a quote of Brighter's source**: `CommandProcessor.Log`'s `[LoggerMessage]` partial (`CommandProcessor.cs:1617–1618` at 10.8.1), whose implementation a source generator writes and `blockcheck` does not run | 7.3: SKIPPED under D21 if cited and matching, or listed |

**Behind *parse*, read by first lines and by `--explain`** (30 blocks; phase 6's lesson that a *parse* block
hides what lies behind it):
- **an API that does not exist**, hidden: `FAQ.md` #3 and #5 call `services.AddProducers(…)`, on
  `IServiceCollection` where it extends `IBrighterBuilder` (`CLAUDE.md` § *Version markers on code*), with
  `options.UseRabbitMQ(...)`, `.Publication<T>(…)` and `.Subscription<T>(…)`. `git grep UseRabbitMQ` finds
  nothing in Brighter's `src` at 10.8.1 or at `origin/master`
- **lone chained calls, arguments and initialisers**, candidates for D3's *"a single option shown alone"*
  where the type resolves: `FAQ.md` #4 and #20; `MongoDBInbox.md` #4 and `MongoDBOutbox.md` #4
  (`MongoDbCollectionConfiguration`); `CommandProcessorConfigurationReference.md` #21; `OutboxArchiver.md` #1.
  `ConfiguringOpenTelemetry.md` #2–#4 have the shape, but their exporters resolve in no pin, which D3 requires;
  they stay with #1, #5, #6 under 017's ruling
- **types and statements in one fence**: `SweeperCircuitBreaking.md` #4, a class then its registration;
  `V10MigrationGuide.md` #19, `PersonCreated` then a `PostAsync`
- **`...` or a comment for arguments or a value**, made whole: `CommandProcessorConfigurationReference.md` #1,
  #5, #13, #14, #15, #17, #19; `DispatcherConfigurationReference.md` #1–#5; `V10MigrationGuide.md` #5 and #16;
  `OutboxArchiver.md` #2
- **a statement missing its `;`**: `BrighterInboxSupport.md` #2
- **a listing of expressions**: `TestDoubleOptions.md` #2, the spy's members, each live in
  `SpyCommandProcessor.cs` at 10.8.1; made whole, or retagged `text` as phase 5 retagged lists of names

**Packages on no pin.** 6.1 said *"every package S4 names is pinned"* and was wrong once (Serilog). Read here
from `--classify`'s *page-type* names against `refs.csproj`'s **103** `PackageReference`s:
- `NSubstitute`: `TestDoubleOptions.md` #12 (`Substitute`, `Arg`). Unpinned, and named in no spec
- `BenchmarkDotNet`: `V10MigrationGuide.md` #26 (`[Benchmark]`). Unpinned, and named in no spec
- `OpenTelemetry.Extensions.Hosting`, `Paramore.Brighter.Extensions.Diagnostics` and the Zipkin, OTLP and Azure
  Monitor exporters: `ConfiguringOpenTelemetry.md` #1–#6, under 017's ruling
- `Moq` and `xunit` are pinned, so `TestDoubleOptions.md` #11 (`Mock`, `Times`) and
  `CommandProcessorConfigurationReference.md` #11 (`Fact`) need only their `using`s; `Confluent.Kafka` comes with
  the Kafka package, so `V10MigrationGuide.md` #22, #23 (`PartitionAssignmentStrategy`) the same

**Recurrence greps (obligation 20), run as written at `dc09559`:**

| Defect | Grep | Hits | Off-group |
|---|---|---:|---|
| `UseResiliencePipeline` given its step first | `grep -rnE '\[UseResiliencePipeline(Async)?\([0-9]' contents/` | **4**: `FAQ.md` #2, #14, #16; and `PipelineValidation.md`'s `text` fence under *Full Detail*, a log line naming the attribute and its step, not a call | none. Against **22** sites that pass the name first (`grep -rnE '\[UseResiliencePipeline(Async)?\("' contents/`) |
| A handler attribute on a class | `grep -rn -A2 -E '^\[(RequestLogging\|UseResiliencePipeline\|UsePolicy\|UseInbox\|FallbackPolicy\|FeatureSwitch\|Monitor\|ValidateRequest\|DeferMessageOnError\|RejectMessageOnError\|DontAckOnError\|Timeout)(Async)?\(' contents/ \| grep -E '^\S+-[0-9]+-\s*public (abstract \|sealed )?class '` | **3**, `FAQ.md` #2, #14, #16 | none |
| A generic handler base without its constraint | `grep -rnE 'class \w+<T>\s*:\s*RequestHandler(Async)?<T>\s*$' contents/` | **1**, `FAQ.md` #1 | none |
| `JsonSerialisationOptions` short of 10.8.1 | `grep -rnF 'Action\<JsonSerialisationOptions\>' contents/`; `grep -c SubscriptionNameConverter contents/CommandProcessorConfigurationReference.md` | **1**, the prose before `CommandProcessorConfigurationReference.md` #21; **0** | none. The page's other uses of `JsonSerialisationOptions.Options`, and those on 10 other pages, name the public static property, which is live |
| `UseRabbitMQ` and `services.AddProducers` | `grep -rnE 'UseRabbitMQ\(' contents/`; `grep -rnE '[Ss]ervices\.AddProducers\(' contents/` | **3** lines in 2 blocks; **2**: `FAQ.md` #3, #5 | none |
| 017's mapper missing a member | a scan of every C# block declaring `class … : IAmAMessageMapper(Async)?<` for both of its `MapTo…` members (`probe`'s reading of `enumerate_blocks()`) | **2**: `MigratingToNullableReferenceTypes.md` #22, `OutboxArchiver.md` #4 | none |
| `System.Environment` bound for a host's | `grep -rnE '(^\|[^.A-Za-z_])Environment\.IsDevelopment\(\)' contents/` | **1**, `CommandProcessorConfigurationReference.md` #10 | none |
| Arguments given as a comment | `grep -rnE '\(/\* [^*]+ \*/\)' contents/` | **8**: `InMemoryOptions.md` #3 (4), `SweeperCircuitBreaking.md` #1, `OutboxArchiver.md` #3; and `MigratingToPollyV8.md` #5 and `UsingSweeperCircuitBreaking.md` #3 | `MigratingToPollyV8.md` #5 is SKIPPED (V9), `UsingSweeperCircuitBreaking.md` #3 is BUILT; neither is a defect, so no off-group repair |
| A source-generated `[LoggerMessage]` | `grep -rn 'LoggerMessage(' contents/` | **1**, `Logging.md` #5 | none |
| Ledger 2, Shouldly unnamed | `grep -c Shouldly contents/TestDoubleOptions.md` | **0** | none: the 17 calls are all on this page |
| Ledger 3, `…DynamoDb.V4` | `grep -rn 'Inbox.DynamoDb.V4' contents/` | **1**, `DynamoInbox.md`'s package list | **`DynamoInbox.md`**, a 017 tranche page, prose only (obligation 21) |
| Unpinned packages | `grep -rnE 'Substitute\.For<\|using NSubstitute' contents/`; `grep -rnE '\[Benchmark\|BenchmarkDotNet' contents/` | **1**; **1** | none |

**The off-group term, from the compile reading, is one page and no block**: `DynamoInbox.md`'s package name
(ledger 3). **From the behavioural claims**, phase 6's lesson, these are the claims in blocks 7.2–7.4 will
touch, for 7.5 to run, with their recurrences:
- `TurningOnReplayOnSeen.md` #4: a `Post` without the pipeline's context is *"a silent no-op under Replay"*
- `CommandProcessorConfigurationReference.md` #10 and `InMemoryOptions.md`: `PostConfigure<BrighterOptions>`
  reaches the command processor's `RequestContextFactory` (`ServiceCollectionExtensions.cs:68–75` registers
  the options pattern; what reads it is the claim). Both pages are S5
- `MongoDBInbox.md` #4, `MongoDBOutbox.md` #4: `TimeToLive` deletes messages after 30 days
- `FAQ.md` #20: `channelCount` is the number of concurrent message pumps
- `V10MigrationGuide.md` #19: no mapper registration is needed, the default mapper serialising
- `TestDoubleOptions.md` #2: `Observe<T>()` dequeues in FIFO order, consuming
- `SweeperCircuitBreaking.md` #1: `CooldownCount` defaults to 10. Read, not run: `OutboxCircuitBreakerOptions.cs:37`
  at 10.8.1 says `= 10`. Recurs off-group on `UsingSweeperCircuitBreaking.md`, and holds there by the same line

A claim overturned on these is repaired at every recurrence the 7.6 greps find, as 6.5 found
`HandlingLargeMessages.md` and `Logging.md`.

**Where blocks will stay *same-page*.** A unit may not stand in for a type the page declares; `probe`'s
reading of each S5 FAILED block's `names` against the page's own `class`/`record`/`interface` declarations:
- **the 3 already *same-page***: `V10MigrationGuide.md` #3, #17 (`PersonCreated`, declared in #19) and #10
  (`MyCommand`, declared in #14 and #15)
- **5 more already named by a declaration**: `V10MigrationGuide.md` #16 and #25 (`PersonCreated`, #19);
  `NullableReferenceTypes.md` #4 and #5 (`CreateOrderCommand`, #3 and #9) and #7 (`CreateUserCommand`, #6)
- **the running examples, undeclared today**: `GreetingMade` across `CommandProcessorConfigurationReference.md`
  #7, #16, #22 and `BrighterInboxSupport.md` #1; `GreetingEvent` across `V10MigrationGuide.md` #21–#24;
  `MyCommand` across `FAQ.md` and `TestDoubleOptions.md`; `ProcessPayment`/`ShipOrder` across
  `TurningOnReplayOnSeen.md` #1, #2, #4, #6. A stub serves each while no block declares it

**Blocks expected to stay FAILED**, each to be listed in § *Blocks that stay FAILED*:
- **3–8 *same-page***, the two groups above
- **6 under 017's pin-limit ruling**: `ConfiguringOpenTelemetry.md` #1–#6, unless the ruling is revisited
- **0–2 on an unpinned package**: `TestDoubleOptions.md` #12 and `V10MigrationGuide.md` #26, by the ruling asked
  below: pinned (D4, D12, D13), rewritten (the Serilog ruling), or listed
- **0–1 source-generated**: `Logging.md` #5, if D21 is not accepted for it

So **9–17** stay; with the ruling to pin all three families, **3–9**.

**`pagelint`, block by block.** S5's **123** warnings (`--per-page` on the twenty-two pages; 15 carry one),
read against `--report` and `--classify`:
- **29 on blocks no repair touches**: 22 BUILT (20 declared `// ...`, 2 bare: `NullableReferenceTypes.md` #8,
  #16) and 7 SKIPPED (`FAQ.md` #15; `V10MigrationGuide.md` #1, #4, #6, #8, #14, #18). They stay
- **41 on FAILED blocks that already declare `// ...`**: 7 *defect*, 13 *page-type*, 19 *parse*, 2 *values*
- **53 on the other FAILED blocks**: 16 *defect*, 15 *page-type*, 11 *built-by-using*, 9 *parse*, 2 *values*

Phase 6 took its 94 to 7; nearly every declared omission gained its `using`s. The floor here is 184 − 94 = **90**.

**Predicted:**

| Gate | At `dc09559` | Predicted at the close | Mechanism |
|---|---|---|---|
| corpus | 994 | **993–1,004** | splits only, up to +10: the 8 alternatives and mixed fences in the defect reading and the 2 behind *parse* (`SweeperCircuitBreaking.md` #4, `V10MigrationGuide.md` #19). Each may instead be made whole in its fence, as phase 6 did with all of its own. −1 if `TestDoubleOptions.md` #2 is retagged `text` |
| BUILT | 758 | **866–899** | 892 (758 + S5's 134), plus splits (0–10), less new skips (0–8), stays (3–17) and a retag (0–1). Off-group **0** |
| FAILED | 207 | **76–90** | 73 (−134 for S5) plus the stays: 3–17, the groups above. Off-group 0 |
| SKIPPED | 29 | **29–37** | D3's lone options, 0–6 (`FAQ.md` #4, #20; `MongoDBInbox.md` #4; `MongoDBOutbox.md` #4; `CommandProcessorConfigurationReference.md` #21; `OutboxArchiver.md` #1); D21's source quotes, 0–2 (`CommandProcessorConfigurationReference.md` #20, `Logging.md` #5) |
| `pagelint` | 0 errors / 184 | **0 errors / 90–115** | 184 less S5's 94 on FAILED blocks, plus what of S5 keeps one: 0–9 on stays not given their `using`s, 0–5 on new skips that carry a warning today (`FAQ.md` #4, #20, the two MongoDB initialisers, `CommandProcessorConfigurationReference.md` #20), 0–10 on a split's new fence, 0–10 declared `// ...` that keep their omission. Off-group 0 |
| `symbolcheck` | 24 entries, 14 silenced | **24–25 entries, 14 silenced**, exit 0 | `UseRabbitMQ` is dead at 10.8.1 and at `origin/master`, as `AddServiceActivator` was (D5); whether it earns a row is read at 7.6. The opt-outs on `FAQ.md` and `V10MigrationGuide.md` stay |
| scaffold rule | 80 units | **89–98 units**, 0 violations, 0 compile errors | 3 S5 pages have units (`FAQContext`, `MigratingToNullableReferenceTypesContext`, `SweeperCircuitBreakingContext`); 9–18 of the other nineteen need one, the two V4 pages as `<unit>+v4` |
| pin rule | 4 pages, 0 violations | **unmoved**; the V4 pin 28 blocks, **28** built | `DistributedLock.md` #2, `DynamoDbDistributedLock.md` #1, #2 each fail only on a value (AC11) |
| assemblies | 553 | **553**, unless the ruling below pins a package | every other package S5 names is pinned |
| nothing BUILT | 12 pages (10 S5, and `ReplayOnSeenReference.md`, `ReturningResultsFromAHandler.md`) | **2–3** | each S5 page gains a BUILT block but `ConfiguringOpenTelemetry.md`, whose six blocks are all under the pin limit |
| `optioncheck` | 0 / 59 / 533 | **0 mismatches, 59 / 533 unless a repair adds a row** | seven S5 pages carry marked tables; no defect above is in one |
| `linkcheck` · `versioncheck` · shape · redirects · `pagelint --plant` | 169 / 0 · 0 of 18 · 161, widest 12 · 77 · 5 / 5 | **unmoved** | no link, tutorial pin or `SUMMARY.md` change |

#### Rulings (session 136)

Given by the maintainer on 2026-10-31 and recorded as given; 7.1's three questions are answered by 1 and 2, and
3 approves the four defects 7.1 named:

> 1. **NSubstitute and BenchmarkDotNet: pin.** `TestDoubleOptions.md` #12 (NSubstitute) and `V10MigrationGuide.md` #26
>    (BenchmarkDotNet) — add the packages to the appropriate pin (follow the D4 Shouldly / D12 / D13 precedent for how a
>    package joins a pin), rather than rewriting or listing the blocks.
> 2. **OpenTelemetry: 017's 2026-09-28 "no pin change" ruling no longer binds in 018. Change the pin if needed** —
>    `OpenTelemetry.Extensions.Hosting` and `Paramore.Brighter.Extensions.Diagnostics` (and any exporter packages
>    `ConfiguringOpenTelemetry.md` #1–#6 need) may be pinned. This also moves `Telemetry.md` #1, #6–#9 and
>    `PostgreSQLMessageBroker.md` #8 outside S5; rebuild and record their movement.
> 3. **The four defects 7.1 named are approved for repair in phase 7, each with a defect-ledger row and recurrence greps
>    across the corpus:**
>    - `FAQ.md` #2, #14, #16: `[UseResiliencePipeline(1, "…")]` has arguments reversed (ctor is `(string policy, int step)`)
>      and sits on the class (`AttributeTargets.Method`). Fix: the async attribute `[UseResiliencePipelineAsync("…", 1)]` on
>      `HandleAsync` (handlers are `RequestHandlerAsync`), with `[RequestLoggingAsync]` moved there too.
>    - `FAQ.md` #1: `MyHandlerBase<T> : RequestHandlerAsync<T>` missing `where T : class, IRequest`.
>    - `CommandProcessorConfigurationReference.md` #20 and the prose before #21: `ConfigureJsonSerialisation` takes
>      `Action<JsonSerializerOptions>` (not `JsonSerialisationOptions`); the `JsonSerialisationOptions` static-ctor listing
>      shows 4 of 12 converters — bring it into line with the 10.8.1 source.
>    - `FAQ.md` #3, #5: `services.AddProducers(o => o.UseRabbitMQ(...).Publication<T>/.Subscription<T>)` — API never
>      existed. Rewrite to the real V10 shape: `services.AddConsumers(...)` first, then `.AddProducers(...)` chained off the
>      returned `IBrighterBuilder`, with `RmqProducerRegistryFactory` and `RmqSubscription<T>`. Keep what the FAQ answer
>      teaches (the difference between AddProducers and AddConsumers).

**Carried out at 7.2.** Ruling 1 is **D22** and ruling 2 **D23** (`design.md` § *Design Decisions*). Ruling 3's five blocks
are all *hard* rows (`FAQ.md` #1, #2, #14, #16 *defect*; #3, #5 *parse*; `CommandProcessorConfigurationReference.md` #20
*defect*, and the prose before #21), so the repairs, their ledger rows and their recurrence greps are 7.3's, written when
each grep has run after its repair (obligation 22). 7.1's recurrence greps stand as run. The pin changes were made here, so 7.3
and 7.4 inherit them. 7.3's text says *"Brighter `10.8.0` source"*; the pins are **10.8.1**, and 7.3 reads 10.8.1.

**Further rulings, 2026-11-01 (session 136, continued)**, given by the maintainer and recorded as given:

> 1. **D24 ratified: keep `Polly.Extensions` 8.8.0** on both pins.
> 2. **Pin `Npgsql.OpenTelemetry`** (new D25, the shape of D22 and D23) on both pins, at a version compatible with the Npgsql
>    already on the pin. Rebuild `PostgreSQLMessageBroker.md` #8 and record its movement.

Carried out at 7.3: ruling 1 is `design.md` D24 (status *Yes, the maintainer, 2026-11-01*) and ruling 2 is **D25**, **9.0.3**.

**Rulings on 7.3's PAUSE, 2026-11-01 (session 136, continued)**, given by the maintainer and recorded as given:

> 1. **Ledger 88–91 confirmed**: `FAQ.md` #20 `channelCount` → `noOfPerformers`; `ExternalBusConfiguration` → `ProducersConfiguration`;
>    the Inbox/Outbox `GetExistsQuery` check that was always true; `IsDevelopment()` in a `Startup`.
> 2. **`OutboxArchiver.md` § *InMemory Archive Example Usage*: replace the Claim Check mapper with a real `InMemoryArchiveProvider`
>    example**, read at the 10.8.1 tag, compiling against the pins, with prose that matches what it does. Keep the heading. Delete the
>    Claim Check scaffolding if nothing else uses it. List any behavioural claim for 7.5.
> 3. **`Logging.md` #5: option (a), make it whole** as a top-level `internal static partial class Log`. If `blockcheck` does not run the
>    `Microsoft.Extensions.Logging` source generator (so `CS8795` remains), leave it listed as FAILED with that diagnostic, and PAUSE
>    explaining; no skip reason is invented. The lead-in must not claim the block is verbatim if the modifier changes.

Carried out at 7.4 (§ *The repairs (7.4)*): 1 is recorded on ledger rows 88–91; 2 is done and builds; 3 is done on the page and
stays FAILED on `CS8795`, the fallback the ruling names.

**Ruling on 7.4's PAUSE, 2026-11-01 (session 136, continued)**, given by the maintainer and recorded as given:

> **Option (c).** `Logging.md` #5 stays listed as FAILED (`CS8795`; `blockcheck` runs no source generators). Making `blockcheck` run the
> source generators its pinned packages ship is recorded as a **follow-up after spec 018**, not done in 018.

Carried out at 7.5: the row in § *Blocks that stay FAILED* and 7.4's question are annotated, and the follow-up is recorded in § *The
behavioural claims (7.5, AC10)*, *Follow-up after 018*.

**Rulings on 7.5's PAUSE, 2026-11-01 (session 136, continued)**, given by the maintainer and recorded as given:

> 1. **Ledger 93 confirmed, and filed upstream as two bugs** (filed by the orchestrator): **BrighterCommand/Brighter#4716**, *MongoDB:
>    TimeToLive is ignored unless MakeCollection is CreateIfNotExists*; **BrighterCommand/Brighter#4717**, *MongoDB: TTL index on TimeStamp
>    never expires documents (DateTimeOffset stored as a subdocument)*.
> 2. **Ledger 94 accepted.**
> 3. **D24 reverted: drop the `Polly.Extensions` pin**, since no block needs it after ledger 94 (`grep -rn 'AddResiliencePipeline(' contents/`
>    → 0). Remove it from both pins, mark D24 reverted in `design.md`, rebuild, and confirm nothing moved.
> 4. **Note, don't change: `MigratingToPollyV8.md` Step 1 tells readers to add `Polly.Extensions` 8.0.0.** Given ledger 94, that package is
>    probably unnecessary for Brighter. Record it as an observation, a candidate for 8.5's friction ledger or a later spec; do not edit the page.

Carried out at 7.6 (§ *Second pass (7.6)*): 1 is on ledger row 93 and in *Upstream, filed during the phase*; 3 is done, 580 → **579**
reference assemblies on each pin and no verdict moved; 4 is recorded as an observation.

#### The repairs (7.2)

**The pins.** Ten packages joined `refs.csproj` and `refs-v4.csproj` together (103 → **113** `PackageReference`s each;
the two lists still differ only by the seven AWS swaps). Each is built against the released packages, never a
`ProjectReference`:

| Package | Version | Why that version | Needed by |
|---|---|---|---|
| `NSubstitute` | 6.2.0 | latest stable; Brighter's own tests use FakeItEasy | `TestDoubleOptions.md` #12 (D22) |
| `BenchmarkDotNet` | 0.15.8 | Brighter's own (`Directory.Packages.props` at 10.8.1) | `V10MigrationGuide.md` #26 (D22) |
| `Paramore.Brighter.Extensions.Diagnostics` | 10.8.1 | the pinned release | `ConfiguringOpenTelemetry.md` #1, #5, #6; `Telemetry.md` #1, #6–#9 (D23) |
| `OpenTelemetry.Extensions.Hosting`, `.Exporter.Zipkin`, `.Exporter.OpenTelemetryProtocol` | 1.19.1 | what `Paramore.Brighter.Extensions.Diagnostics` 10.8.1 depends on (`OpenTelemetry.Api.ProviderBuilderExtensions` 1.19.1) | `ConfiguringOpenTelemetry.md` #1–#6 (D23) |
| `OpenTelemetry.Instrumentation.AspNetCore`, `.Http` | 1.19.0 | Brighter's own | `ConfiguringOpenTelemetry.md` #5, which the page already names (D23) |
| `Azure.Monitor.OpenTelemetry.Exporter` | 1.10.0 | latest stable | `ConfiguringOpenTelemetry.md` #4 (D23) |
| `Polly.Extensions` | 8.8.0 | `Polly.Core`'s version on the pin | `FAQ.md` #13 (**D24**, below) |

Reference assemblies: **553 → 579** on each pin (576 after the first nine packages; the two instrumentation packages
and `Polly.Extensions` added 2 and 1). No restore warning on either project. Three packages were found only by building:
`OpenTelemetry.Instrumentation.AspNetCore` and `.Http` (7.1's list stopped at the exporters), and `Polly.Extensions` (below).

**D24 is not ruled.** `FAQ.md` #13 reads *values* (`services`) in 7.1's table. Once `services` was supplied the second
compile said `CS1061 AddResiliencePipeline`: the extension lives in `Polly.Extensions`, which no pin carries and the page
never named, and 7.1's list of packages on no pin (read from *page-type* names) could not see it. It is the same shape as D13,
so it was pinned here and the page now says `AddResiliencePipeline` comes from `Polly.Extensions`. **Reverting is one line in
each project and one sentence in the page; the maintainer is asked to ratify it.**

**Ruled 2026-11-01: ratified.** `Polly.Extensions` 8.8.0 stays on both pins (D24, `design.md`).

**What the pins moved, by page and ordinal** (`--report` before and after, diffed by `(page, ordinal)`):

| Block | Before | After | Mechanism |
|---|---|---|---|
| `ConfiguringOpenTelemetry.md` #1, #5, #6 | FAILED (*defect*, `CS0234`/`CS1929`) | **BUILT** | the pin alone; no page or unit change |
| `ConfiguringOpenTelemetry.md` #2, #3, #4 | FAILED (*parse*) | FAILED (*parse*) | lone `.AddZipkinExporter(…)`, `.AddOtlpExporter(…)`, `.AddAzureMonitorTraceExporter(…)` calls, whose receivers now resolve in the pin: D3's *"a single option shown alone"* applies, 7.3's |
| `Telemetry.md` #1, #9 | FAILED (`CS0234`) | **BUILT** | the pin alone |
| `Telemetry.md` #6, #7, #8 | FAILED (`CS0234`) | **BUILT** | the pin, then the second compile read *values* (`builder`, `CS0103`); `TelemetryContext` gained `WebApplicationBuilder builder` and each block `using Microsoft.Extensions.DependencyInjection;` (obligation 21: a block the ruling made reachable is repaired by the phase that moved it) |
| `PostgreSQLMessageBroker.md` #8 | FAILED (`CS0234`) | FAILED (`CS7036`) | `AddNpgsql()` is `Npgsql.OpenTelemetry`'s extension on `TracerProviderBuilder`, which the page names and no pin carries; binding falls to `Npgsql.EntityFrameworkCore.PostgreSQL`'s. **Not pinned: no ruling covers it** (a question for the maintainer, below) |

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *Predicted (7.1)*, `Telemetry.md` #1, #6–#9 and `PostgreSQLMessageBroker.md` #8 *"fail on the same `CS0234`"* as `ConfiguringOpenTelemetry.md` #1, #5, #6: nine blocks a pin ruling would move | pinned: **5** build on the pin alone (`ConfiguringOpenTelemetry.md` #1, #5, #6; `Telemetry.md` #1, #9), **3** more after a `builder` value (`Telemetry.md` #6–#8, which were *values*, not `CS0234`, once the pin was in), and **1** (`PostgreSQLMessageBroker.md` #8) fails on another unpinned package. `ConfiguringOpenTelemetry.md` #2–#4, not among the nine, stay *parse* | the prediction stands; this row is the reading at 7.2 |
| § *Predicted (7.1)*, assemblies: *"553, unless the ruling below pins a package"*; *"every other package S5 names is pinned"* | **579**; and `Polly.Extensions` (`FAQ.md` #13) and `Npgsql.OpenTelemetry` (`PostgreSQLMessageBroker.md` #8) are two more packages on no pin | the prediction stands; reconciled at 7.7 |
| § *Predicted (7.1)*, *Reachable*, **69** | **65** build and **4** stay: `NullableReferenceTypes.md` #4, #5, #7 and `V10MigrationGuide.md` #25, the *page-type* rows 7.1 named as already declared by another block. Three *hard* rows (`ConfiguringOpenTelemetry.md` #1, #5, #6) build on the pin, so S5 loses **68** FAILED, not 65 | the table stands; the stays are in § *Blocks that stay FAILED* |
| § *Predicted (7.1)*, scaffold rule *89–98 units* | **94** units, 0 violations, 0 compile errors: 14 new, 3 extended | in band |
| § *Predicted (7.1)*, nothing BUILT *2–3* | **2**: `DispatcherConfigurationReference.md` and `BrighterOutboxSupport.md`, both all-hard | in band |

**S5's reachable rows: 65 build, 4 stay.**
- **12 *built-by-using*** got their `using`s: `CommandProcessorConfigurationReference.md` #2, `NullableReferenceTypes.md` #2, #3,
  #6, #13, #14, `TestDoubleOptions.md` #10, `V10MigrationGuide.md` #2, #15, `MongoDBInbox.md` #1, #3,
  `SweeperCircuitBreaking.md` #3
- **the 3 V4 blocks are BUILT** on the V4 pin, none with a `CS0234 … V4` diagnostic: `DistributedLock.md` #2 and
  `DynamoDbDistributedLock.md` #1, #2 each lacked only a value (`services`; `dynamoDb`, an `IAmazonDynamoDB`). They got a unit
  each (`DistributedLockContext`, `DynamoDbDistributedLockContext`), and the pin column is still `v4`. The V4 pin is now
  **28 blocks, 28 built, 0 failing** (25 built before); `DistributedLock.md` #1 was already BUILT and its scaffold changed
- **the *page-type* rows** are stubs in 14 new units, each stub a request or an interface with only the members a BUILT
  block names, plus usings: `BrighterBasicConfiguration`, `BrighterInboxSupport`, `CommandProcessorConfigurationReference`,
  `EFCoreOutbox`, `InMemoryOptions`, `Logging`, `MongoDBOutbox`, `NullableReferenceTypes`, `OutboxArchiver`,
  `TestDoubleOptions`, `TurningOnReplayOnSeen`, `V10MigrationGuide`, and the two V4 units. `FAQContext`,
  `MigratingToNullableReferenceTypesContext` and `TelemetryContext` were extended. `TestDoubleOptions.md` #3, #4, #10, #11,
  #12 build; its Shouldly blocks (#1, #5–#9) and #2 are 7.4's and 7.3's, and `TestDoubleOptionsContext` carries only what the
  five need
- **`OutboxArchiver.md` #3** was a fragment (`.AddProducers(/* producer configuration */)`, `CS1501`), the 7.1 table's
  *arguments given as a comment* row. It is made whole: `AddProducers(options => { // ... the producer registry for your
  bus` then `options.Outbox = new InMemoryOutbox(TimeProvider.System); })`. The declared omission is the registry, which the
  reader has
- **`EFCoreOutbox.md` #2** gained a comment, `// uow.People is the DbSet<Person> on your context`. The unit rule names a
  type only if a BUILT block does, and `Person` appears in none; the comment says what `People` is, which the reader needs
- **The 4 that stay** cannot take a stub: `NullableReferenceTypes.md` #4, #5, #7 name `CreateOrderCommand` and
  `CreateUserCommand`, declared by #3, #9 and #6, and the other undeclared types they name (`Order`, `IOrderRepository`,
  `CreateOrderDto`, `CreateUserDto`) are named by no BUILT block, so unit rule 1 refuses them. They were given their
  `using`s, so the diagnostics left are the undeclared types'. `V10MigrationGuide.md` #25 names `PersonCreated` (#19) and
  `BuildServiceProvider`. The sentence before each now says what it assumes

**What the reachable rows hid: nothing the compiler could see.** Every block that became BUILT did so with no diagnostic
beyond the names and `using`s; the second compile (`probe/second.py`, 131 FAILED blocks on S5's 22 pages once the pin was
in) named no API defect behind a *page-type* or *values* row, which 6.2 found in S4 and 3.2 in S1. The two readings above are
not defects of the page's claims: a package no pin carried (D24) and a fragment (`OutboxArchiver.md` #3). No ledger row is
written. What compiling cannot say is in § *For 7.5*.

**Prose added** (the packages and the sentences the stays need):
- `TestDoubleOptions.md`: *Add the `Moq` package…* and *Add the `NSubstitute` package…* under their headings (7.4 adds the
  Shouldly sentence)
- `V10MigrationGuide.md`: *…with the `BenchmarkDotNet` package*, and the sentence before #25
- `FAQ.md`: *`AddResiliencePipeline` comes from the `Polly.Extensions` package*
- `NullableReferenceTypes.md`: a clause before #4, #5 and #7 saying which block declares the command

**Measured after 7.2** (`dc09559` + this task's tree; `git add -A` first):

| Gate | At `dc09559` | After 7.2 | |
|---|---|---|---|
| `--report` | 994 / 758 / 207 / 29, 0 findings | **994 / 831 / 134 / 29**, 87 findings, exit 1 | 73 FAILED → BUILT and **nothing else moved** (diffed by page and ordinal); the 87 are 73 BUILT blocks not yet baselined and 14 BUILT blocks whose page gained a unit, all 7.7's |
| scaffold rule · pin rule | 80 units, 0 · 4 pages, 0 | **94** units, 0 violations, 0 compile errors · 4 pages, 0 violations | the first run read 3 violations (`EFCoreOutboxContext`: `Person`, `Message`; `FAQContext`: `Name`), each a stub no BUILT block names, repaired |
| reference assemblies | 553 | **579** on each pin | above |
| `--classify` | 31 / 12 / 42 / 37 / 12 / 73 | **31 parse, 0 built-by-using, 34 defect, 37 same-page, 3 values, 29 page-type**, 3 rounds | the 3 *values* are off-S5 (`AgreementDispatcherRouting.md` #12, `InMemoryScheduler.md` #14, `QueryHandlerDependencies.md` #3) |
| `groups.py`, S5 | 22 pages, 134 FAILED | **18 pages, 66 FAILED** (30 parse, 0 built-by-using, 29 defect, 3 same-page, 0 values, 4 page-type), 98 BUILT, 2 with nothing BUILT | `BrighterBasicConfiguration.md`, `DistributedLock.md`, `DynamoDbDistributedLock.md`, `EFCoreOutbox.md` are whole and have left the table. Two methods: the FAILED column, 66; `--report` on the same 18 pages, **66**, with 98 BUILT and 7 SKIPPED. `groups.py` total 29 pages, 100 FAILED: S5's 66 and 34 on the 11 pages S1–S4 still hold. The other 34 of the 134 are on tranche pages |
| `pagelint` | 0 / 184 | **0 errors / 142** | S5's 123 → **81** (`--per-page` on the 22 pages), nothing off-S5 moved; `--changed origin/master` 0 errors; `--plant` 5 / 5 |
| `linkcheck` · `symbolcheck` · `optioncheck` | 169 / 0 · 24 / 14 · 0 / 59 / 533 | 169 / 0 · 24 entries, 14 silenced, exit 0 · 0 mismatches (5 tables, 47 rows on the five pages touched) | no table was touched |
| `versioncheck` · shape | 0 of 18 · 161, widest 12 | unmoved | |

Against 7.1's bands, which 7.7 reconciles: BUILT **831** now against **866–899** at the close (7.3–7.5 still to come);
FAILED **134** against **76–90**; `pagelint` 142 against **90–115**; units 94 in band.

**For 7.3.**
- the five blocks of ruling 3 (above) and the 30 *parse* and 29 *defect* blocks; `CommandProcessorConfigurationReference.md`
  gained `GreetingMade`, `MyCommand`, `GetSubscriptions` and a `builder` in its unit, so #10 (`Environment`) and #20/#21 find
  them
- `ConfiguringOpenTelemetry.md` #2–#4: D3's *single option shown alone*, each now resolving in the pin
- `V10MigrationGuide.md` #3, #10, #16, #17 stay *same-page* or *parse* as 7.1 read them; `MyCommand` and `PersonCreated` are
  declared by blocks, so `V10MigrationGuideContext` may not stub them
- `FAQ.md` #1 needs `where T : class, IRequest`; #2, #14, #16 are the async attribute on `HandleAsync` (their
  `[RequestLoggingAsync]` with them); `FAQContext` already supplies `MyCommand` (with `Value`), `MyEvent`, `GreetingCommand`
  and `registry`

**For 7.4.** `TestDoubleOptionsContext` supplies `spy`, `MyCommand`, `MyEvent` and `MyHandler`. Blocks #1, #5–#9 name
`PlaceOrderHandler`, `PlaceOrder`, `OrderPlaced`, `TriggerCommand`, `DownstreamCommand` and more, undeclared by any block,
and #2's `spy.Observe<MyCommand>()` lines are the *listing of expressions* 7.1 read.

**For 7.5.** Blocks 7.2 touched whose prose or comment asserts behaviour, none run here:
- `FAQ.md` #10–#12: a base-typed `ICommand` sent with `Send` throws *"No command handler was found for the typeof command
  Paramore.Brighter.ICommand"*, and `Send((dynamic)command)` resolves the runtime type
- `FAQ.md` #13: a pipeline registered with `services.AddResiliencePipeline("MyRetryPolicy", …)` is the one
  `[UseResiliencePipelineAsync("MyRetryPolicy", …)]` finds. It is the claim D24 depends on
- `TestDoubleOptions.md` #11, #12: `Times.Once` and `Received(1)` verify one `Publish`; both frameworks' calls compile
  against the real `IAmACommandProcessor.Publish<T>(T, RequestContext?)`
- `OutboxArchiver.md` #3: `.UseOutboxArchiver<CommittableTransaction>(new InMemoryArchiveProvider())` is the InMemory Outbox's
  transaction type (`System.Transactions.CommittableTransaction`)
- `TurningOnReplayOnSeen.md` #1–#3, #5, #6 and `BrighterInboxSupport.md` #1 built with a stub for the request; the claims
  7.1 listed (#4's silent no-op under Replay) are unchanged
- `V10MigrationGuide.md` #22–#24: `PartitionAssignmentStrategy.Range` is `Confluent.Kafka`'s, and `GroupProtocol`
  replaces the flat settings

**Question for the maintainer, from 7.2.** (1) Ratify **D24**, `Polly.Extensions` 8.8.0 on both pins. (2) `PostgreSQLMessageBroker.md` #8
(a tranche page, off-S5) now fails only because `Npgsql.OpenTelemetry`, which the page names, is on no pin: pin it (as
D22 and D23), or list the block? Until ruled it stays FAILED and is in § *Blocks that stay FAILED*.

**Ruled 2026-11-01:** (1) D24 ratified. (2) `Npgsql.OpenTelemetry` is pinned (**D25**); `PostgreSQLMessageBroker.md` #8 builds, and
its row has left § *Blocks that stay FAILED* (§ *The repairs (7.3)*).

#### The repairs (7.3)

**S5's hard blocks leave in the four states.** At the start of 7.3 (7.2's tree plus the D25 pin) S5 held **66** FAILED: 30 *parse*,
29 *defect*, 3 *same-page*, 4 *page-type*. Of the 59 *parse* and *defect* rows, 6 are 7.4's (`TestDoubleOptions.md` #1, #5–#9, Shouldly),
leaving **53** hard blocks, all on S5 pages and none off-group. They are now **51 BUILT**, **2 FAILED and listed** (§ *Blocks that stay
FAILED*: `Logging.md` #5, a source-generated quote; `V10MigrationGuide.md` #16, which cleared its parse errors and is now *same-page*
on `PersonCreated`), **0 SKIPPED** and **0 retagged**. No fence was split, so the corpus stays **994** and § *Splits* gains no row.

**No block was SKIPPED under D3.** Each single option shown alone was made whole inside the call it belongs to, as 3.3 and 6.3 did:
`FAQ.md` #4 and #20 (an `RmqSubscription<MyEvent>`), `MongoDBInbox.md` #4 and `MongoDBOutbox.md` #4 (a `MongoDbConfiguration`),
`CommandProcessorConfigurationReference.md` #21 (a `ConfigureServices`), `OutboxArchiver.md` #1 (an `AddBrighter()` chain), and
`ConfiguringOpenTelemetry.md` #2–#4, each now a `ServiceCollection` with `AddOpenTelemetry().WithTracing(…)`, the exporter's package named
in the sentence before it. Alternatives in one fence were made whole as two classes (`TurningOnReplayOnSeen.md` #4) or renamed so the
locals no longer collide (`NullableReferenceTypes.md` #1, #10, #11, `FAQ.md` #8); types and statements in one fence were reordered
statements first (`MigratingToNullableReferenceTypes.md` #4, #7; `NullableReferenceTypes.md` #12; `SweeperCircuitBreaking.md` #4) or the
statements put in a method (`V10MigrationGuide.md` #19).

**The four approved defects (ruled 2026-10-31)** are ledger **84–87**. `FAQ.md` #2, #14, #16 are `[UseResiliencePipelineAsync("…", 1)]`
on `HandleAsync` with `[RequestLoggingAsync]` beside it; rule 8 stays green. #1 gained its constraint. #3 and #5 are now
`services.AddConsumers(…)` first, `.AddProducers(…)` chained off the returned builder (#3), and `services.AddBrighter().AddProducers(…)`
(#5, the CloudEvents answer), with `RmqSubscription<T>`, `RmqProducerRegistryFactory` and `RmqPublication<T>`; the answer still teaches
that `AddConsumers` replaces V9's `AddServiceActivator` and `AddProducers` its `UseExternalBus`. The deprecation note under #14 gained the
sync/async pairing, because `[UseResiliencePipeline]` belongs on `Handle`. `CommandProcessorConfigurationReference.md`
#20 is the static constructor's twelve converters, written as the statements that build the same `JsonSerializerOptions` (the class is
`static`, so it cannot be redeclared), and the prose cites the file; #21 and its prose take `Action<JsonSerializerOptions>`, which receives a copy
(`probe/s5/json-options`: 12 converters before, 12 after, `WriteIndented` true on the copy and false on the instance read before the call).
#21's example sets `WriteIndented`, not `PropertyNameCaseInsensitive`, which is already `true` by default and so showed an override that overrides nothing.

**Defects the prediction did not name (ledger 88–91), each hidden behind *parse* or *defect* and repaired in place.**
- **`FAQ.md` #20 (ledger 88).** `channelCount: 3`; the parameter is `noOfPerformers`. Fourteen other pages name `noOfPerformers`.
- **`CommandProcessorConfigurationReference.md` (ledger 89).** `ExternalBusConfiguration` twice, in the prose before #15 and before the Outbox
  example; `ProducersConfiguration`.
- **`BrighterInboxSupport.md` #2 and `BrighterOutboxSupport.md` #2 (ledger 90).** Two APIs wrong at once: `GetExistsQuery` takes the schema first
  (`sqlConnection.Database`), and `ExecuteScalar() != null` is always true, because the query is `SELECT EXISTS (…)`. The page's code returned
  at "exists" on the first run and never created the table. Run against a throwaway MySQL 8 container (`probe/s5/mysql-exists`): absent,
  the scalar is `0` and the corrected test false; present, `1` and true; the old test true in both. The container is removed.
- **`CommandProcessorConfigurationReference.md` #10 (ledger 91)** is 7.1's `Environment` row, repaired as a `Startup` taking
  `IConfiguration` and `IWebHostEnvironment` by constructor.

**What the hard blocks hid beyond that, repaired.** `OutboxArchiver.md` #2: `DynamoDbLockingProvider` lives in
`Paramore.Brighter.Locking.DynamoDb` (lower-case `b`), not `…DynamoDB`; the block carried no usings. `DispatcherConfigurationReference.md` #2:
`amqp://guest:guest@local:5672` for `localhost`. `CommandProcessorConfigurationReference.md` #5: `options =>` followed by two statements and
no braces. #15 and #17: an unclosed method and a stray `...` after the chain. `InMemoryOptions.md` #3: `GetSubscriptions()` was called and
never declared, which the first compile hid behind the other errors; the four `/* production config */` arguments are now built from
`configuration`. `Logging.md` #4: `CommandProcessorBuilder.StartNew()…Build()` skipped four required steps; it now takes the chain
`HowConfiguringTheDispatcherWorks.md` shows.

**One content question, not repaired.** `OutboxArchiver.md` § *InMemory Archive Example Usage* shows a Claim Check mapper under a heading
about the InMemory archive provider. The block now compiles (it gained `MapToRequest` and `[RetrieveClaim(0)]`, ledger 92), but it does not
illustrate the heading. Replacing it would write new content for the page, so it is put to the maintainer.

**Packages and the pins.** D25: `Npgsql.OpenTelemetry` **9.0.3** on both pins (**114** `PackageReference`s each; **580** reference assemblies
on each, one more than 7.2). `PostgreSQLMessageBroker.md` #8 moved FAILED (`CS7036`) → **BUILT** on the pin alone, no page changed, and its row
has left § *Blocks that stay FAILED*. D24 is ratified. Nothing else was pinned: the V4 pin is still **28 of 28**.

**Units.** Three are new (`BrighterOutboxSupportContext`: `AddGreeting`, `Person`, `Greeting`, `GreetingMade`; `CausationTrackingStoresContext`:
`MyTransaction`; `DispatcherConfigurationReferenceContext`: `GreetingMade` and `DbConnectionString()`), each with a `pages.tsv` row. Five are
extended, each stub a type or member a BUILT block names: `CommandProcessorConfigurationReferenceContext` (`MyRequestHandlerAsync`,
`DebugRequestContextFactory`), `LoggingContext` (`subscriberRegistry`, `handlerFactory`), `MigratingToNullableReferenceTypesContext`
(`OrderCreatedEvent`, `OrderDto`), `NullableReferenceTypesContext` (`User`, `user`) and `OutboxArchiverContext` (`LargeDataCommand`);
`V10MigrationGuideContext`'s comment now names block 5. `MyCommand` and `PersonCreated` were not stubbed on `V10MigrationGuide.md`.

**Prose added** (the packages, and the sentences the repairs need): the exporter packages before the Zipkin and Azure Monitor examples
(`ConfiguringOpenTelemetry.md`); the constants the Inbox and Outbox builders' examples now declare, in place of "we assume"; "Below is a
handler that uses Dapper", for what had been a fragment; the clause before `V10MigrationGuide.md` #16; and the `Logging.md` lead-in citing
`CommandProcessor.cs`.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *Predicted (7.1)*, the 30 *parse* blocks: *"candidates for D3's single option shown alone"* (0–6 SKIPPED) | 0 SKIPPED; each is whole | the prediction stands, reconciled at 7.7 |
| § *Predicted (7.1)*, *Blocks expected to stay FAILED*: `Logging.md` #5 *"0–1 source-generated"*, *same-page* 3–8 | `Logging.md` #5 stays (listed, D21 not used); *same-page* is **4** on S5 (`V10MigrationGuide.md` #3, #10, #16, #17) and *page-type* **4** (`NullableReferenceTypes.md` #4, #5, #7; `V10MigrationGuide.md` #25), all in § *Blocks that stay FAILED* | the prediction stands |
| § *Predicted (7.1)*, the *Defects the second compile read*: 5 API defects | 4 approved and ledgered (84–87), and **4 more** (88–91) in blocks 7.1 read as *parse* | the table stands; the four are ledgered |
| § *The repairs (7.2)*, *"`PostgreSQLMessageBroker.md` #8 FAILED (`CS7036`)"* | **BUILT** after D25 | 7.2's row stands as the reading at 7.2 |
| § *Predicted (7.1)*, scaffold rule *89–98 units* | **97** units, 0 violations, 0 compile errors | in band |

**Measured after 7.3** (`3e8f0f3` + this task's tree; `git add -A` first):

| Gate | After 7.2 | After the D25 pin | After 7.3 | |
|---|---|---|---|---|
| `--report` | 994 / 831 / 134 / 29, 87 findings | 994 / 832 / 133 / 29 | **994 / 883 / 82 / 29**, 140 findings, exit 1 | 51 FAILED → BUILT and nothing else moved; the 140 are 125 BUILT blocks not yet baselined and 15 BUILT blocks whose page gained or changed a unit, all 7.7's |
| scaffold rule · pin rule | 94 units, 0 · 4 pages, 0 | | **97** units, 0 violations, 0 compile errors · 4 pages, 0 violations | |
| reference assemblies | 579 | 580 | **580** on each pin | D25 |
| `groups.py`, S5 | 18 pages, 66 FAILED | | **4 pages, 15 FAILED** (7 *defect*, 4 *same-page*, 4 *page-type*), 160 BUILT of 182, 7 SKIPPED | two methods: the FAILED column, 15; `--report` on the 22 S5 pages, 160 BUILT / 15 FAILED / 7 SKIPPED, 182 blocks. `TestDoubleOptions.md` holds 6, 7.4's; no S5 page has nothing BUILT |
| `pagelint` | 0 errors / 142 | | **0 errors / 97** | S5's 81 → **36** (`--per-page` on the 22 pages), nothing off-S5 moved; `--changed origin/master` 0 errors; `--plant` 5 / 5 |
| `linkcheck` · `symbolcheck` · `optioncheck` | 169 / 0 · 24 / 14 · 0 / 59 / 533 | | 169 / 0 · 24 entries, 14 silenced, exit 0 · 0 mismatches (13 tables, 94 rows on the 18 pages touched) | no table was touched |
| `versioncheck` · shape · redirects | 0 of 18 · 161 pages, widest 12 · 77 / 7858 | | unmoved | |

Against 7.1's bands, which 7.7 reconciles: BUILT **883** against **866–899** at the close (7.4 and 7.5 to come); FAILED **82** against
**76–90**; `pagelint` **97** against **90–115**; units 97 in band.

**For `symbolwatch.tsv`, a recommendation for 7.6 (the row is not added here).** `UseRabbitMQ` resolves at neither 10.8.1 nor
`origin/master` (`git -C ../Brighter grep -lw UseRabbitMQ <ref> -- 'src/*.cs'` finds nothing at either), is dead as `AddServiceActivator`
was (D5), and was written as current code on `FAQ.md` (ledger 87). The corpus has 0 hits now; a row with `AddConsumers(…).AddProducers(…)`
as the replacement keeps it so.

**For 7.5.** Blocks 7.3 wrote or edited whose text asserts behaviour, none run here but ledger 86 and 90:
- `FAQ.md` #20: `noOfPerformers: 3` is the number of concurrent message pumps (the transport pages' tables say *"Threads reading this channel, each with
  its own message pump"*); #3: the `AddConsumers`-first chain registers both sides
- `CommandProcessorConfigurationReference.md` #10: `PostConfigure<BrighterOptions>` reaches the command processor's `RequestContextFactory`
  (with `InMemoryOptions.md`), as 7.1 listed
- `MongoDBInbox.md` #4, `MongoDBOutbox.md` #4: `TimeToLive` deletes messages after 30 days
- `V10MigrationGuide.md` #19: no mapper registration is needed, the default mapper serialising
- `TurningOnReplayOnSeen.md` #4: a `Post` without the pipeline's context is a silent no-op under Replay
- `SweeperCircuitBreaking.md` #1: `CooldownCount` defaults to 10 (read: `OutboxCircuitBreakerOptions.cs:37`)
- `TestDoubleOptions.md` #2: `Observe<T>()` dequeues in FIFO order, consuming
- `OutboxArchiver.md` #4: a `[ClaimCheck]` on `MapToMessage` pairs with `[RetrieveClaim]` on `MapToRequest`

**Questions for the maintainer, from 7.3.** (1) Ledger 88–91 were not in 7.1's prediction; each is repaired and ledgered, as obligation 21 asks.
(2) `OutboxArchiver.md` § *InMemory Archive Example Usage*: replace the Claim Check mapper with an example that uses the InMemory archive provider,
or leave it? (3) `Logging.md` #5 is listed as FAILED. Widen D21 to *a source-generated partial quoted from Brighter's source* and SKIP it, or leave it listed?

**Ruled 2026-11-01:** (1) ledger 88–91 confirmed. (2) Replace it with a real `InMemoryArchiveProvider` example, heading kept. (3) Option (a):
make it whole as a top-level `internal static partial class Log`; if `blockcheck` runs no source generator, leave it listed on `CS8795`.
Carried out at 7.4.

#### The repairs (7.4)

**`TestDoubleOptions.md`'s six Shouldly blocks are BUILT.** #1, #5, #6, #7, #8 and #9 each gained `using Paramore.Brighter.Testing;` and
`using Shouldly;`, and #6 and #8 `using System.Linq;` for `.First()`. The *Installation* section now says the examples assert with the
`Shouldly` package's `ShouldBe…` methods, so the reader adds it to run them as written. The sentence sits beside the `Paramore.Brighter.Testing`
reference rather than under a heading of its own, because the Moq and NSubstitute sentences 7.2 added already name their packages where
their examples are. `grep -c Shouldly contents/TestDoubleOptions.md` → **7**, from **0** (ledger 2, closed on its row).

**Before and after, by `--report`:**

| Block | Before | After |
|---:|---|---|
| 1 | FAILED, 5 diagnostics, `CS0103`, `CS0246` | **BUILT** |
| 5 | FAILED, 4, `CS0103`, `CS0246` | **BUILT** |
| 6 | FAILED, 5, `CS0103`, `CS0246` | **BUILT** |
| 7 | FAILED, 5, `CS0103`, `CS0246` | **BUILT** |
| 8 | FAILED, 8, `CS0103`, `CS0246`, `CS8130` | **BUILT** |
| 9 | FAILED, 4, `CS0103`, `CS0246` | **BUILT** |

The `CS0103` and `CS0246` left after 2.7's pin were the page's types and the missing `using`s. `TestDoubleOptionsContext` gained a stub for
each type a block names and no block declares: `PlaceOrder`, `OrderPlaced`, `PlaceOrderHandler`, `OrderHandler`, `TriggerCommand`,
`DownstreamCommand`, `SendNotification`, `NotificationSent`, `NotificationHandler`, `ProcessPayment`, `PaymentProcessed`,
`TransactionalHandler` and `CommandA`, plus `MyHandler.Handle(TriggerCommand)` for #5. Each stub has only the members a block names. Each
handler stub makes the call its block then checks (`Publish`, `Send`, `Post`, `DepositPost`), as 7.2's `MyHandler` does. The scaffold rule
reads **0 violations**.

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *Shouldly in the pin (2.7)*: #8's `CS8130` and `CS1503` on the deconstruction *"likely an API mismatch, read at 7.4"* | **not an API mismatch**. `DepositedRequests` is an `IReadOnlyDictionary<Id, IRequest>` (`SpyCommandProcessor.cs:65` at 10.8.1), whose `KeyValuePair` deconstructs. With `.First()` unresolved for want of `System.Linq`, the compiler could not infer `id` and `request`. Given the `using`, #8 builds with no other change, and `ClearOutbox(new[] { id })` takes the `Id[]` it declares (`:428`) | 2.7's text stands as its reading; no ledger row, since nothing on the page was wrong but the `using` |
| § *Predicted (7.1)*: the six Shouldly blocks BUILT or listed | **6 BUILT**, 0 listed | in band |

**Ruling 2: `OutboxArchiver.md` § *InMemory Archive Example Usage*** (heading kept, so its anchor holds). The Claim Check mapper is replaced
by an example read at the 10.8.1 tag. `UseOutboxArchiver<TTransaction>` registers an `OutboxArchiver<Message, TTransaction>` over the
registered Outbox and archive provider (`HostedServiceCollectionExtensions.cs:58–74`), and `TimedOutboxArchiver` calls its `Archive` or
`ArchiveAsync` with `MinimumAge` on each tick (`TimedOutboxArchiver.cs:118–120`). So the example builds one directly over an
`InMemoryOutbox` with an `InMemoryArchiveProvider`, the shape a test takes. It adds a message, marks it dispatched two days ago, calls
`Archive(TimeSpan.FromDays(1))` and reads `ArchivedMessages`, the provider's `Dictionary<string, Message>` keyed by message Id
(`InMemoryArchiveProvider.cs:38`). `TTransaction` is `CommittableTransaction`, the type `InMemoryOutbox.Add` takes (`InMemoryOutbox.cs:131`),
as § *InMemory Archive Configuration* above it already uses. The block builds (#4, BUILT before and after). `LargeDataCommand` left
`OutboxArchiverContext`, since no block names it any more, and the unit keeps `services` for #3. No other page names `LargeMessageMapper`,
so nothing else loses its example (`grep -rn LargeMessageMapper contents/` → 0).

Two things the example avoids, read at the tag. A `FakeTimeProvider` advanced past `MinimumAge` would also pass the `InMemoryBox`'s
`ExpirationScanInterval` (10 minutes) and `EntryTimeToLive` (5 minutes) (`InMemoryBox.cs:68`, `:76`). That hands expiry to a background
worker, which races the archive. Marking the message dispatched in the past with `TimeProvider.System` keeps the test deterministic.
`DispatchedMessages` selects `TimeFlushed <= now − dispatchedSince` (`InMemoryOutbox.cs:309–312`), so a message dispatched two days ago is
eligible at one day.

**Ruling 3: `Logging.md` #5 is whole, and stays FAILED on `CS8795`.** It is now a top-level `internal static partial class Log`. The lead-in
says the Command Processor declares it in a `Log` class nested inside it (`CommandProcessor.cs:1615` at 10.8.1, `private`), and that the page
shows one of them as a top-level `internal` class, the form it takes in your own code, so it no longer claims to be verbatim.
`blockcheck --explain Logging_5` still reads `CS8795`: blockcheck compiles with no analyzers, so the `Microsoft.Extensions.Logging` generator
never writes the method's body. It was compiled two-way in `probe/s5/logger-message`, against `Microsoft.Extensions.Logging.Abstractions`
9.0.0:

| Run | Result |
|---|---|
| Case: `dotnet build`, the generator running, as in a reader's project | **0 errors** |
| Control: `dotnet build -p:DropGen=true`, every `Analyzer` removed before `CoreCompile`, as blockcheck compiles | **`CS8795`**, blockcheck's diagnostic |
| Second control: the former top-level `private static partial class Log`, generator running | **`CS1527`** |

So the block is now one a reader can compile; before, out of its class, it could not. The gate cannot show this without running
generators. The row stays in § *Blocks that stay FAILED*, annotated with the ruling, and no skip reason was written. This is the ruling's fallback,
and **7.4 returns PAUSE on it**. A choice is open: whether `blockcheck` should run the generators its pinned packages ship (a tool change, so
scope), or the row stays as it is. **Ruled 2026-11-01: (c).** The row stays listed as FAILED on `CS8795`; teaching `blockcheck` to
run the generators its pinned packages ship is a follow-up after spec 018, not done in 018 (§ *Rulings (session 136)*).

**Off-group: none.** No recurrence grep was needed. The Shouldly calls are all on this page (7.1's grep, ledger 2). `grep -rn 'LoggerMessage(' contents/`
→ 1, `Logging.md` #5. The only InMemory archive example is the one replaced.
**No new defect.** #8's `CS8130` was a missing `using` (above).

**Measured after 7.4** (`ee54515` + this task's tree; `git add -A` first):

| Gate | After 7.3 | After 7.4 | |
|---|---|---|---|
| `--report` | 994 / 883 / 82 / 29, 140 findings | **994 / 889 / 76 / 29**, 146 findings, exit 1 | the six → BUILT and nothing else moved (`diff` of verdicts by page and ordinal). The 146 are **131** BUILT blocks not yet baselined and **15** whose page gained or changed a unit (counted by their lines in the output), all 7.7's |
| scaffold rule · pin rule · assemblies | 97 units, 0 · 4 pages, 0 · 580 | **97** units, 0 violations, 0 compile errors · 4 pages, 0 · **580** on each pin; the V4 pin 28 of 28 | no unit added; two edited |
| `groups.py`, S5 | 4 pages, 15 FAILED | **3 pages, 9 FAILED** (1 *defect*, 4 *same-page*, 4 *page-type*), 0 with nothing BUILT | two methods: the FAILED column, 9; `--report` on the 22 S5 pages, **166 BUILT / 9 FAILED / 7 SKIPPED**, 182 blocks. `--classify` 1 parse, 0 built-by-using, 5 defect, 38 same-page, 3 values, 29 page-type |
| `pagelint` | 0 errors / 97 | **0 errors / 91** | `TestDoubleOptions.md` 6 → **0** (`--per-page`, before and after), nothing else moved; the three touched pages 0 / 0; `--changed origin/master` 0 errors; `--plant` 5 / 5 |
| `linkcheck` · `symbolcheck` · `optioncheck` | 169 / 0 · 24 / 14 · — | 169 / 0 · 24 entries, 14 silenced, exit 0 · `OutboxArchiver.md` 0 mismatches, 1 table, 4 rows (not touched; run because the page carries one) | |

Against 7.1's bands, which 7.7 reconciles: BUILT **889** (866–899), FAILED **76** (76–90), `pagelint` **91** (90–115), units 97 (89–98):
all in band.

**For 7.5.** Blocks 7.4 wrote or made build whose text asserts behaviour, none run here:
- `OutboxArchiver.md` #4: `Archive(TimeSpan.FromDays(1))` moves a message dispatched two days ago into `ArchivedMessages`, keyed by its Id,
  and deletes it from the Outbox. Control: a message dispatched within the day, or never dispatched, stays in the Outbox and is not archived
- `TestDoubleOptions.md` #8: `ClearOutbox` moves a deposited request to the observation queue, so `Observe<PaymentProcessed>()` returns it
  only after the clear; #9: `Reset()` empties `RecordedCalls` and `DepositedRequests`. The stubs' handlers make the calls the reader's would
- 7.3's item *"`OutboxArchiver.md` #4: a `[ClaimCheck]` on `MapToMessage` pairs with `[RetrieveClaim]` on `MapToRequest`"* lapses: the block
  no longer shows a mapper

#### The behavioural claims (7.5, AC10)

**Every run is against the released packages at the pins** (Brighter 10.8.1; Moq 4.21.0, NSubstitute 6.2.0, Shouldly 4.3.0;
`Polly.Extensions` 8.8.0), as `probe/s5/<name>/` projects, never a `ProjectReference`. Each prints `ok` or `FAIL` per row and exits 1 on
any `FAIL`. Each block is copied as the page shows it, the probe's own lines marked `// probe`. Two claims needed infrastructure, and each got
a disposable container, started for the run and removed after it, as the csproj comments say: `mongo-ttl` ran against `mongo:7` with
`ttlMonitorSleepSecs=1` on port 27019, and `v10-migration`'s Kafka rows against `apache/kafka:4.0.2`. The repository's own `brighter-*`
containers were not touched. 7.3's two runs (ledger 86, 90) are listed as 7.3 recorded them, not rerun.

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `MongoDBOutbox.md` #4 and `MongoDBInbox.md` #4, as they stood: `TimeToLive = TimeSpan.FromDays(30) // Messages will be automatically deleted after 30 days`; *"MongoDB will automatically create a TTL index on the `TimeStamp` field and remove expired documents"* | each block's configuration (`MakeCollection` left at its default, `Assume`), one message or entry stored; then a 2 s `TimeToLive` and 10 s waited | `MakeCollection = CreateIfNotExists`; and, as the instrument, a raw document with a BSON date `TimeStamp` in the control's collection | `mongo-ttl` | no TTL index; the message left / `brighter_ttl_Outbox` (and `_Inbox`) on `TimeStamp`, `expireAfterSeconds` 2592000, and **the message still left**; the instrument removed, so the monitor ran. `TimeStamp` is stored as `{ DateTime, Ticks, Offset }`. **Overturned** (ledger 93) |
| `MongoDbDistributedLock.md`, as it stood: *"a TTL index expires stale lock documents automatically, which protects you if an instance crashes"* | a lock obtained and never released, `TimeToLive` 2 s, `CreateIfNotExists`; a second instance asks 10 s later | the lock released, then obtained | `mongo-ttl` | `null`: still held / obtained. **Overturned** (ledger 93) |
| The repair the Mongo pages now show: a TTL index on `TimeStamp.DateTime` | that index on the default collection, a message, an entry and an unreleased lock, 10 s waited | the case rows above, without it | `mongo-ttl` | 0 of 1 message, 0 of 1 entry left; the lock obtained again / 1, 1, held |
| `FAQ.md` #13 with the handler under it (#14), as they stood: a pipeline registered with `services.AddResiliencePipeline("MyRetryPolicy", …)` is the one `[UseResiliencePipelineAsync("MyRetryPolicy", 1)]` runs | the page's registration, the handler failing once | the same retry in a `ResiliencePipelineRegistry<string>` given to `AddBrighter`'s `options.ResiliencePipelineRegistry`, which is #13 as repaired | `faq-claims` | `ConfigurationException`, inner `KeyNotFoundException` *"Unable to find a resilience pipeline associated with the key 'MyRetryPolicy'"*; the handler never ran / no exception, the handler ran twice. **Overturned** (ledger 94) |
| `FAQ.md` #16, as it stood: `services.AddResiliencePipeline("MyTimeout", …)` | the page's registration | the repaired form, a 1 s timeout over a 3 s handler | `faq-claims` | `ConfigurationException`, the same inner / `TimeoutRejectedException`. **Overturned** (ledger 94) |
| `FAQ.md` #10–#12: `Send` of an `ICommand`-typed variable throws *"ArgumentException … No command handler was found for the typeof command Paramore.Brighter.ICommand - a command should have exactly one handler."*; `(dynamic)` resolves the runtime type | #10 | #11, the concrete type; #12, `(dynamic)` | `faq-claims` | `ArgumentException` with that message, word for word / handled once; handled once |
| `FAQ.md` #20: `noOfPerformers: 3  // Number of concurrent message pumps` | #20's `RmqSubscription` (`NoOfPerformers` 3), and the Dispatcher `AddConsumers` builds for an in-memory subscription with `noOfPerformers: 3`, received | `noOfPerformers: 1` | `faq-claims` | 3 consumers / 1 |
| `FAQ.md` #3: the `AddConsumers`-first chain registers both sides | #3 as shown, no broker (registrations read, not connected) | `AddBrighter().AddProducers(…)` alone | `faq-claims` | `IDispatcher` registered and resolved, the producer registry's topics `[my.event]` / no `IDispatcher` |
| `CommandProcessorConfigurationReference.md` #9: `PostConfigure<BrighterOptions>` sets the factory the command processor uses, and runs after the `AddBrighter` delegate | #9; and `AddBrighter` setting a factory of its own beside the `PostConfigure` | no `PostConfigure` | `options-pattern` | the handler's context from `TestRequestContextFactory`; the `PostConfigure` one wins / `RequestContext`, Brighter's own |
| `CommandProcessorConfigurationReference.md` #10: in Development, `DebugRequestContextFactory` | `Startup` with a Development environment, a `Brighter` section bound | Production | `options-pattern` | `DebugRequestContextFactory`'s / Brighter's own |
| `CommandProcessorConfigurationReference.md` #11: two providers in one process, each with its own factory | `ParallelTestsA`'s and `ParallelTestsB`'s registrations, sent A, B, A | each other | `options-pattern` | `ContextA`, `ContextB`, `ContextA` |
| `InMemoryOptions.md` #1: the same, beside `AddProducers` and an `InMemoryOutbox` | #1 | as #9 | `options-pattern` | `TestRequestContextFactory`'s |
| `TurningOnReplayOnSeen.md` #1 and #4 ✅: with `Context as RequestContext` on `Post`, a duplicate does not run the handler and marks the original message undispatched, for the Sweeper to resend | each handler (#4's carry no `[UseInbox]`, so each was given #1's), the same command sent twice, then the outstanding messages cleared | #4 ❌, `Post` with no context: *"a silent no-op — no exception, nothing resent"* | `replay-context` | handler once, no exception, 1 outstanding, the bus 1 then 2 after the clear / handler once, no exception, 0 outstanding, the bus 1 |
| `BrighterInboxSupport.md` #1: `[UseInboxAsync(step:0, …, onceOnly: true)]` with the default action throws `OnceOnlyException` on a duplicate | the same event published twice | an event with its own Id | `replay-context` | `OnceOnlyException` / handled |
| `TestDoubleOptions.md` #2 and the sentence after it: `Observe<T>()` dequeues FIFO, consuming, and throws `InvalidOperationException` when none is left | A, an event, B sent; Observe three times | `GetRequests<T>()` twice | `spy-processor` | A, B, then `InvalidOperationException` / `AB`, `AB`, and A still observable after |
| `TestDoubleOptions.md` #8: `ClearOutbox` moves a deposit to the observation queue | `Observe` after the clear | `Observe` before it | `spy-processor` | the deposit, `Amount` 99.99 / `InvalidOperationException` |
| `TestDoubleOptions.md` #9: `Reset()` empties `RecordedCalls` and `DepositedRequests` | a `Send` and a `DepositPost`, then `Reset` | no `Reset` | `spy-processor` | `false`, 0, 0 / `true`, 2, 1 |
| `TestDoubleOptions.md` #11, #12: `Times.Once` and `Received(1)` verify one `Publish` | a handler publishing once | twice | `spy-processor` | both pass / `MockException`, `ReceivedCallsException` |
| `OutboxArchiver.md` #4: `Archive(TimeSpan.FromDays(1))` moves a message dispatched two days ago into `ArchivedMessages`, keyed by its Id, and out of the Outbox | #4 | a message dispatched an hour ago; one never dispatched | `outbox-archiver` | archived, gone from the Outbox / neither archived, both kept |
| `OutboxArchiver.md` #3: `CommittableTransaction` is the InMemory Outbox's transaction type | #3's `UseOutboxArchiver<CommittableTransaction>`, the archiver resolved and run | `UseOutboxArchiver<IDbTransaction>` | `outbox-archiver` | 1 archived / `ArgumentException` *"A sync Outbox must be defined."*, 0 |
| `SweeperCircuitBreaking.md` #1: `CooldownCount … (default: 10)` | `new OutboxCircuitBreakerOptions()` | set to 3 | `outbox-archiver` | 10 / 3. 7.1 read it from `OutboxCircuitBreakerOptions.cs:37`; now run |
| `V10MigrationGuide.md` #19: no mapper registration, the default mapper serialising | `PersonCreatedPublisher.PublishAsync` on the in-memory bus | an explicit async mapper registered for the type | `v10-migration` | a JSON body, `name` Alice, `email` alice@example.com / the explicit mapper ran (its header marker present) |
| `V10MigrationGuide.md` #22–#24: the obsolete properties are back-fill defaults; `ClassicGroupProtocol` gives the same settings; `ConsumerGroupProtocol` is KIP-848 | each subscription's consumer, built by `KafkaMessageConsumerFactory` against the broker, its `ConsumerConfig` read | a subscription setting none | `v10-migration` | both properties `[Obsolete]`; #22 and #23 `Classic`, 30000, `Range`; #24 `Consumer`, unset, unset / `Classic`, 10000, `RoundRobin` |
| `CommandProcessorConfigurationReference.md` #20, #21 (ledger 86) | 7.3's | 7.3's | `json-options` (7.3) | 12 converters; `WriteIndented` true on the copy / false on the instance read before the call |
| `BrighterInboxSupport.md` #2, `BrighterOutboxSupport.md` #2 (ledger 90) | 7.3's, table absent | table present | `mysql-exists` (7.3) | `0`, the corrected test false / `1`, true; the old test true in both |

**Two claims overturned, each repaired at every recurrence (obligation 21)**, the recurrence greps run after the repair (obligation 22):
- **Ledger 93.** `MongoDBOutbox.md` and `MongoDBInbox.md`: § *Time-To-Live (TTL) Support* now says what `TimeToLive` does at 10.8.1 and why
  it removes nothing, and #4 is the index a reader creates on `TimeStamp.DateTime`, the form the probe shows removing documents. #1's comment
  (*"Optional: Auto-expire messages after 7 days"*) now points to that section; the Outbox's `TimeToLive` option row, both pages' structure lists
  and the Inbox's `ExpireAfterSeconds` line say the same. `MongoDbDistributedLock.md`'s options paragraph, § *Provisioning* and *Notes*, and
  `DistributedLock.md`'s provider table, say a crashed holder's lock stays held unless the reader adds that index. The three headings are kept,
  so their anchors hold. `FirestoreDistributedLock.md`'s *"TTL document"* row is Firestore's and not this defect
- **Ledger 94.** `FAQ.md` #13 and #16 now build a `ResiliencePipelineRegistry<string>` and give it to `AddBrighter`; the sentence before #13 says
  Brighter looks only there, and what a `services.AddResiliencePipeline` registration gets instead. No other page registers a pipeline that
  way (`grep -rnF 'services.AddResiliencePipeline(' contents/` → 0)

**Candidate upstream issue, for the maintainer (not filed).** `Paramore.Brighter.MongoDb` 10.8.1, and `origin/master` unchanged:
`MongoDbCollectionConfiguration.TimeToLive` never expires a document. `BaseMongoDb.CreateCollection` creates the TTL index only on the
`CreateIfNotExists` path (`BaseMongoDb.cs:108–118`), and indexes `TimeStamp`, a `DateTimeOffset` that MongoDB.Driver 3.x serialises as an
embedded document, which a TTL index ignores. `OutboxMessage`, `InboxMessage` and `LockMessage` all carry it, so stale locks are never
released either. A fix would index `TimeStamp.DateTime` (or store `TimeStamp` as a BSON date) and create the index whenever `TimeToLive` is set.
`probe/s5/mongo-ttl` reproduces it.
**Filed as #4716 and #4717 (2026-11-01).**

**Said and measured.**

| Said | Measured | Where the text now stands |
|---|---|---|
| § *Predicted (7.1)*: *"`MongoDBInbox.md` #4, `MongoDBOutbox.md` #4: `TimeToLive` deletes messages after 30 days"*, a claim to run | it deletes nothing, on either `MakeCollection` (above) | ledger 93 |
| § *The repairs (7.2)*, *For 7.5*: *"`FAQ.md` #13 … the one `[UseResiliencePipelineAsync("MyRetryPolicy", …)]` finds. It is the claim D24 depends on"* | it is not found; and once #13 is repaired, no block on the corpus calls `AddResiliencePipeline`, so D24's `Polly.Extensions` pin serves no block (`grep -rn 'AddResiliencePipeline(' contents/` → 0) | ledger 94; the pin is put to the maintainer below |
| 7.1's claim list: `SweeperCircuitBreaking.md` #1, *"Read, not run"* | run, 10 | it holds |

**Read, not run:**
- `V10MigrationGuide.md`, *"requires Apache Kafka 4.0 or later"*: the probe reads the consumer's configuration and joins no group
- `TurningOnReplayOnSeen.md` § *The symptom*: the Debug log line and the span event names; the probe observes the Outbox and the bus only
- `FAQ.md` #3 over a live RabbitMQ: the registrations are read, not connected
- 7.3's `OutboxArchiver.md` #4 claim about a `[ClaimCheck]`/`[RetrieveClaim]` pair lapsed at 7.4, as § *The repairs (7.4)* says

**Follow-up after 018** (ruled 2026-11-01, § *Rulings (session 136)*). Teach `blockcheck` to run the source generators its pinned packages ship,
so a block such as `Logging.md` #5, a `[LoggerMessage]` partial, can build as it does in a reader's project (`probe/s5/logger-message`). Recorded
here because this spec keeps no follow-ups list and § *Friction ledger* is written at 8.5; 8.5 should carry it there.

**Measured after 7.5** (`20a27ad` + this task's tree; `git add -A` first):

| Gate | After 7.4 | After 7.5 | |
|---|---|---|---|
| `--report` | 994 / 889 / 76 / 29, 146 findings | **994 / 889 / 76 / 29**, 146 findings, exit 1 | the six blocks edited (`FAQ.md` #13, #16; `MongoDBOutbox.md` #1, #4; `MongoDBInbox.md` #1, #4) stay BUILT and nothing moved. Two methods: the summary line, and `cut -f1 \| sort \| uniq -c` of the rows, 889 / 76 / 29. The 146 are 7.7's, as at 7.4 |
| scaffold rule · pin rule · assemblies | 97 units, 0 · 4 pages, 0 · 580 | **97** units, 0 violations, 0 compile errors · 4 pages, 0 · **580** on each pin; the V4 pin 28 of 28 | no unit changed |
| `pagelint` | 0 errors / 91 | **0 errors / 91** | the five touched pages 0 errors / 3 warnings, which they carried before; `--changed origin/master` 0 errors; `--plant` 5 / 5 |
| `linkcheck` · `symbolcheck` · `optioncheck` | 169 / 0 · 24 / 14 · — | 169 / 0 · 24 entries, 14 silenced, exit 0 · 0 mismatches, 2 tables, 12 rows on the five pages (`MongoDBOutbox.md`'s `TimeToLive` row's description changed) | |

**Questions for the maintainer, from 7.5.** (1) Ledger 93 and 94 are new defects, each repaired at every recurrence; confirm them. (2) The Mongo
`TimeToLive` defect is upstream (above): file it? (3) D24: with `FAQ.md` #13 and #16 repaired, no block needs `Polly.Extensions`, and the FAQ no longer names
the package. Keep the pin (one line in each project, harmless) or revert it?

**Ruled 2026-11-01:** (1) ledger 93 confirmed and ledger 94 accepted. (2) Filed, by the orchestrator, as **Brighter #4716** (`TimeToLive`
ignored unless `MakeCollection` is `CreateIfNotExists`) and **#4717** (the TTL index on `TimeStamp` never expires a document). (3) Reverted:
`Polly.Extensions` leaves both pins (D24). Carried out at 7.6.

#### Second pass (7.6)

**Every phase-7 ledger row holds.** Rows 84–94 each give their *After* from their grep, run as written on the tree: **0** for every row
but 84 (`PipelineValidation.md`'s `text` log line, as recorded), 85 (1 hit, constrained) and 86 (1, the converter now listed), whose greps
count sites that are now right. Row 92's mapper scan, committed at last as `probe/mapper_members.py` (obligation 18: 7.1, 7.3 and this
task ran it), reads **0**; run over the tree at `dc09559`, it names the two blocks 7.1 found, so it can fail. 7.1's recurrence greps, run corpus-wide as written:

| Defect | 7.1's grep | At `dc09559` | On the tree | Ledger |
|---|---|---:|---:|---:|
| `UseResiliencePipeline` given its step first | `grep -rnE '\[UseResiliencePipeline(Async)?\([0-9]' contents/` | 4 | **1**, the log line | 84 |
| A handler attribute on a class | 7.1's `-A2` grep | 3 | **0** | 84 |
| A generic handler base without its constraint | `grep -rnE 'class \w+<T>\s*:\s*RequestHandler(Async)?<T>\s*$' contents/` | 1 | **1**, constrained on the next line | 85 |
| `JsonSerialisationOptions` short of 10.8.1 | `grep -rnF 'Action\<JsonSerialisationOptions\>'`; `grep -c SubscriptionNameConverter` | 1; 0 | **0**; **1** | 86 |
| `UseRabbitMQ` and `services.AddProducers` | `grep -rnE 'UseRabbitMQ\('`; `grep -rnE '[Ss]ervices\.AddProducers\('` | 3; 2 | **0**; **0** | 87 |
| 017's mapper missing a member | `probe/mapper_members.py` | 2 | **0** | 92 |
| `System.Environment` bound for a host's | `grep -rnE '(^\|[^.A-Za-z_])Environment\.IsDevelopment\(\)'` | 1 | **0** | 91 |
| Arguments given as a comment | `grep -rnE '\(/\* [^*]+ \*/\)'` | 8 | **2**, `MigratingToPollyV8.md` #5 (SKIPPED, V9) and `UsingSweeperCircuitBreaking.md` #3 (BUILT), as 7.1 read them | — |
| A source-generated `[LoggerMessage]` | `grep -rn 'LoggerMessage('` | 1 | **1**, `Logging.md` #5, listed | — |
| Ledger 2, Shouldly unnamed | `grep -c Shouldly contents/TestDoubleOptions.md` | 0 | **7** | 2 |
| Ledger 3, `…DynamoDb.V4` | `grep -rn 'Inbox.DynamoDb.V4'` | 1 | **0**, repaired here | 3 |
| Unpinned packages | `Substitute\.For<\|using NSubstitute`; `\[Benchmark\|BenchmarkDotNet` | 1; 1 | 2; 3, each now pinned (D22) and named by its page | — |

**Widened, by shape rather than words:**

| Defect | Widened grep | Hits | Reading |
|---|---|---:|---|
| 3, a package id misspelt | `Paramore\.Brighter\.(Inbox\|Outbox\|Locking)\.Dynamo[Dd][Bb](\.V4)?`, against each package's `<id>` in its `.nuspec` | 3 misspelt at `8c90dcc` | **a recurrence**: `DynamoInbox.md` spells the v3 Inbox package `…Inbox.DynamoDb` as well, and `DynamoOutbox.md` the v3 Outbox package `…Outbox.DynamoDb`; the ids are `…DynamoDB`. Repaired (ledger 3). The `Locking.DynamoDb` hits name a namespace, which 10.8.1 spells `DynamoDb` in two of its three files (`AWSSQSMigrateToV10.md` says so), and are right |
| 84, a resilience or policy attribute given its step first | `\[(UseResiliencePipeline\|UsePolicy\|FallbackPolicy\|TimeoutPolicy)(Async)?\(\s*[^"\s)]` | 36 | every one a named `step:` argument, a constant, a `string[]`, or Darker's `[FallbackPolicy(step)]`; the log line |
| 84, any attribute on a class | an attribute line directly above a `class` declaration | 8 | **a recurrence**: `FAQ.md` #15, `[TimeoutPolicy]` on a `RequestHandlerAsync` class, which 7.1's list (`Timeout(`, not `TimeoutPolicy(`) could not match. The other seven are `[PublicationTopic]`, `[ApiController]`/`[Route]` and test-class attributes, valid on a class |
| 84, an attribute's kind against its method | every attribute in the run of lines above a `Handle`, `HandleAsync`, `Execute` or `ExecuteAsync`, its `Async` suffix against the method's | 45 at `8c90dcc`, 44 on the tree | **a new defect**, ledger 95: `V10MigrationGuide.md` #8, `[TimeoutPolicy]` on `HandleAsync`. The rest are Darker's attributes, which carry no suffix and decorate either method, and `PipelineValidation.md`'s marked block |
| 85, a generic base without its constraint | `class \w+<\w+>\s*:\s*(RequestHandler\|QueryHandler\|IHandleRequests\|IAmAMessageMapper)` | 2 | both constrained |
| 86, `JsonSerialisationOptions` as a type | `JsonSerialisationOptions\b(\.\w+)?`; `ConfigureJsonSerialisation` | 34; 3 | 31 `.Options`, the live property; two prose names of the class; the file name; the two `ConfigureJsonSerialisation` calls take `JsonSerializerOptions` |
| 87, `AddProducers` off the wrong receiver, an invented fluent transport | `(services\|serviceCollection\|builder\.Services\|Services)\s*\.\s*AddProducers\(`; `\.Use(RabbitMQ\|Rmq\|Kafka\|Sqs\|Sns\|AzureServiceBus\|Redis\|MsSql\|Postgres\|InMemory)\w*\(`; `\.(Publication\|Subscription)<\w+>\(` | 0; 3; 0 | Hangfire's `UseRedisStorage` and Quartz's `UsePostgres`, live in their packages and in BUILT blocks |
| 88, a pump count misnamed | `(channel\|pump\|performer\|consumer\|thread)s?Count`, case-blind; `noOf\w+\|numberOf\w+` | 0; 32 | `noOfPerformers`/`NoOfPerformers`; the Control API's `numberOfPerformers`, `numberOfActivePerformers` (its route and JSON); `numberOfShards` |
| 89, a V9 bus configuration type | `\w*BusConfiguration\b\|ExternalBus\(\|IAmExternalBus\w*` | 4 | `AzureServiceBusConfiguration`, the live class or its page's file name in a link |
| 90, an exists query and its test | `GetExistsQuery\(`; `ExecuteScalar(Async)?\(` | 3; 4 | the two repaired; `MsSqlQueueBuilder.GetExistsQuery("QueueData", schemaName: "dbo")`, matching `(string queueTableName, string? schemaName = "dbo")` (`MsSqlQueueBuilder.cs:114`); the two repaired tests and two DDL executions |
| 91, a host's environment | `Environment\.(Is\w+\|EnvironmentName)\b`, unqualified | 0 | |
| 93, a TTL claim | `TimeToLive\|\bTTL\b\|ExpireAfter\|expireAfter` | 49 lines on 17 pages | the four Mongo and lock pages as repaired; the rest are other stores', transports' and schedulers' TTLs (Azure Service Bus, Redis, Firestore, the InMemory stores, RabbitMQ, DynamoDB), not this mechanism. `DynamoOutbox.md`'s `TimeToLive` row (*"before DynamoDB expires it"*) is a behavioural claim no task has run; read, not run |
| 94, a DI-registered pipeline | `AddResiliencePipeline\|ResiliencePipelineProvider\|AddResiliencePipelines` | 1 | `FAQ.md`'s sentence saying Brighter does not read it |
| Arguments given as a comment | `\(\s*/\*[^*]*\*/\s*\)\|\(\s*\.\.\.\s*\)\|=\s*/\*[^*]*\*/\s*[;,]` | 13 | prose, inline code and a code comment (`HangfireScheduler.md`), SKIPPED V9 forms (`V10MigrationGuide.md`, `MigratingToPollyV8.md`), and BUILT `UsingSweeperCircuitBreaking.md` #3 |
| A package on no pin | every `dotnet add package`, `Install-Package` and `PackageReference Include` on the corpus, against both `refs` projects | 10 not on a pin | Hangfire, Quartz, TickerQ, the Analyzer and `Microsoft.Extensions.Hosting` are transitive or install steps; no FAILED block fails for want of one (each FAILED block on their pages is listed *same-page*). `Polly` and `Polly.Extensions` on `MigratingToPollyV8.md`: the observation below. `Paramore.Brighter.MessagingGateway.RMQ`: the next row |
| A package that does not exist | every `Paramore.Brighter…` id the corpus tells a reader to install (`dotnet add package`, `PackageReference`, a bulleted package name), 61, each against NuGet's version index for a stable 10.x release | 2 with none | **a new defect**, ledger 96: `Paramore.Brighter.MessagingGateway.RMQ` stops at `10.0.0-preview.4`; V10 ships `…RMQ.Async` and `…RMQ.Sync` (10.0.0 through 10.8.1). Named on `V10MigrationGuide.md` (`dotnet add package … --version 10.0.0`) and `CommandProcessorConfigurationReference.md` (*Transports*), whose blocks import `…RMQ.Async`. Repaired. `RabbitMQConnectionStability.md` names it as a log-category prefix, which is right. The other, `Paramore.Brighter.Outbox.{DB}`, is a placeholder |

**The recurrences, repaired (obligation 21).**
- **Ledger 3, widened.** `DynamoInbox.md`'s two package names and `DynamoOutbox.md`'s one now read `…DynamoDB` (prose only; no block moves)
- **Ledger 96.** `V10MigrationGuide.md`'s install step and `CommandProcessorConfigurationReference.md`'s *Transports* now name
  `Paramore.Brighter.MessagingGateway.RMQ.Async`, the package each page's blocks import (prose and a `bash` fence; no C# block moves)
- **Ledger 84 → 95.** `FAQ.md` #15 and `V10MigrationGuide.md` #8, each a V9 form SKIPPED beside its V10 replacement, are now `[TimeoutPolicy]` on a
  synchronous `Handle`, with the `using`s a V9 reader needs (`Paramore.Brighter`, `Paramore.Brighter.Policies.Attributes`, both live at 9.33). The
  sentence before each says the attribute has no async form, so in V9 it decorates a synchronous `Handle`. `probe/s5/timeout-policy-kind`, against
  10.8.1, two-way:

| Claim | Case | Control | Probe | Result |
|---|---|---|---|---|
| `V10MigrationGuide.md` #8 as it stood: `[TimeoutPolicy]` on an async handler's `HandleAsync` is the V9 form | the block, sent with `SendAsync` | the repaired form, `[TimeoutPolicy]` on `Handle`: a handler slower than the timeout, and one faster | `timeout-policy-kind` | `ConfigurationException`, *"All handlers in an async pipeline must derive from IHandleRequestsAsync"*, naming `TimeoutPolicyHandler<T>` / cut off with `OperationCanceledException`; handled once |
| `FAQ.md` #15 as it stood: `[TimeoutPolicy]` on the class | `dotnet build -p:ClassLevel=true` | `dotnet build`, the repaired form | `timeout-policy-kind` | `CS0592` / 0 errors |

  The cut-off surfaces as `OperationCanceledException`, not `TimeoutException`: `TimeoutPolicyHandler.Handle` waits on the token its timer
  cancels. No page says which, so nothing else changes.

**Ruling 3: D24 reverted.** The `Polly.Extensions` `PackageReference` and its comment left `refs.csproj` and `refs-v4.csproj` (**114 → 113**
`PackageReference`s each). After the rebuild each pin resolves **579** reference assemblies, from **580**. `--report` before and after, the
whole row file compared: **identical**, 994 / 889 / 76 / 29. `design.md` D24 is marked reverted, and its ruling-dates sentence says so.

**Ruling 4, an observation, the page not changed.** `MigratingToPollyV8.md` Step 1 tells the reader to add `Polly.Extensions` 8.0.0 (as a
`PackageReference`, beside `Polly` 8.0.0). Brighter reads only the `ResiliencePipelineRegistry<string>` given to `AddBrighter` (ledger 94,
`probe/s5/faq-claims`), never the registry `Polly.Extensions` puts in DI, so for Brighter that package is probably unnecessary. Its blocks need
it for nothing: none calls `AddResiliencePipeline`, and none is FAILED (#1, #3, #5 are SKIPPED V9 forms; the rest BUILT on a pin without it). A
candidate for 8.5's friction ledger, or a later spec.

**`symbolcheck` rows (7.3's recommendation; ledgers 87–89).** Each name resolved with `git -C ../Brighter grep -lw <name> <ref> -- 'src/*.cs'`
at `8.1.1399`, `9.9.13` and `9.33` (V8 and V9), `10.8.1` and `origin/master`, the live replacement the second way:

| Name | V8 · V9 (9.9.13, 9.33) | 10.8.1 · master | Replacement, live | Reading |
|---|---|---|---|---|
| `UseRabbitMQ` | 0 · 0, 0 | 0 · 0 | `AddConsumers` 6 files at each | **no row.** Never a released name, and the watchlist holds *"names known dead in a released product"*; 6.5's reading for `RabbitMQProducerRegistryFactory` and the rest. 0 sites, and the blocks that held it build, so `blockcheck` catches its return there |
| `channelCount` | 0 · 0, 0 (and `ChannelCount` 0) | 0 · 0 | `noOfPerformers` 14 files at each | **no row**, the same reading; and an argument name, as 6.5 read `compressionType:` |
| `ExternalBusConfiguration` | 0 · **5, 5** | 0 · 0 | `ProducersConfiguration` 3 files at each | **meets the watchlist's test**: V9's type for the external bus (`CommandProcessorBuilder.ExternalBus(ExternalBusConfiguration, …)` and the DI extensions at 9.33), gone at V10, and written as current prose on `CommandProcessorConfigurationReference.md` (ledger 89). A watchlist row is scope (D10), so it is **put to the maintainer, not added** |

A trial with the row in place, reverted after: `symbolcheck` **25 entries, 161 pages, 14 silenced, 0 findings**; `--verify-list` all 25 DEAD at
both refs, every replacement live. Two-way: a planted page naming `ExternalBusConfiguration` gave **1 site**, exit 1, with the row; **0**, exit 0,
without it. `tools/symbolwatch.tsv` is unchanged.

**Upstream, filed during the phase:** **#4716** and **#4717**, MongoDB `TimeToLive` (ledger 93; filed by the orchestrator, 2026-11-01).

**Off-group term:** `DynamoInbox.md` and `DynamoOutbox.md`, 017 tranche pages, prose only (ledger 3). `FAQ.md`, `V10MigrationGuide.md` and
`CommandProcessorConfigurationReference.md` are S5.

**Measured after 7.6** (`8c90dcc` + this task's tree; `git add -A` first):

| Gate | After 7.5 | After 7.6 | |
|---|---|---|---|
| `--report` | 994 / 889 / 76 / 29, 146 findings | **994 / 889 / 76 / 29**, 146 findings, exit 1 | the row files before and after, `diff` → identical. Two methods: the summary line, and `cut -f1 \| sort \| uniq -c` of the rows. The compiler's own line moved 861 / 105 → **862 / 104** built and failing: it compiles SKIPPED blocks too, and `FAQ.md` #15 no longer fails `CS0592`. The 146 are 7.7's |
| reference assemblies | 580 on each pin | **579** on each pin; the V4 pin 28 of 28 | D24 reverted |
| scaffold rule · pin rule | 97 units, 0 · 4 pages, 0 | **97** units, 0 violations, 0 compile errors · 4 pages, 0 violations | no unit changed |
| `pagelint` | 0 errors / 91 | **0 errors / 89** | the two V9 blocks gained their `using`s; the five touched pages 0 errors / 7 warnings; `--changed origin/master` 0 errors; `--plant` 5 / 5 |
| `linkcheck` · `symbolcheck` · `optioncheck` | 169 / 0 · 24 / 14 · — | 169 / 0 · 24 entries, 161 pages, 14 silenced, exit 0 · 0 mismatches, 7 tables, 56 rows on the five pages (no table touched) | |
| `versioncheck` | 0 of 18 | **0 stale pins of 18**, 5 pages | |

**Questions for the maintainer, from 7.6.** (1) Ledger 95 and 96 are new defects, found by widening ledger 84 and 7.1's package shape, each
repaired at both its sites: confirm them.
(2) Add `ExternalBusConfiguration` to `tools/symbolwatch.tsv` (replacement `ProducersConfiguration`, evidence ledger 89), as D10 added
`UseExternalBus`? (3) Rule 8 cannot see ledger 95's shape, an attribute with no `…Async` twin on `HandleAsync`, because `PAIRED` holds only the
attributes Brighter ships in both forms. Teaching it the sync-only attributes is a tool change: in 018, or a follow-up after it?

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
    design's **≥ 560**, and the nothing-BUILT figure against **≤ 14**. AC13's join command is written
    above § *Blocks that stay FAILED* and run there, zero rows either side

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

*Drawn at task 2.14 from `--classify` at `71cb09b` (phase 2's head after 2.13), and not before (AC8).
Re-drawn at task 3.1 at `f3f685f`, the pins at Brighter 10.8.0; what that moved is in § *Phase 3 as
executed*. They replace design § *The section groups*' working figures, which came from the probe.*

**Every table is drawn by one command**, over three files that committed commands write:

```bash
python3 tools/blockcheck.py --classify > classify.tsv
python3 tools/blockcheck.py --report report.tsv
python3 spec/018-compile_residual/probe/tranche_pages.py spec/017-compile_repairs/tasks.md > tranche.txt
python3 spec/018-compile_residual/probe/groups.py classify.tsv report.tsv tranche.txt
```

**Where each column comes from:**
- **FAILED and the six class columns** count `--classify`'s rows per page, by its third column
  (`class`, the second compile's). An empty cell is 0.
- **BUILT** counts `--report`'s `BUILT` rows. It is the gate's own output, not a probe's, and it
  is what the *nothing BUILT* target is measured on.
- **The rows** are every page with a FAILED block that is not one of 017's 75 tranche pages, plus
  the three tranche pages whose FAILED blocks are all on the V4 pin, marked **(V4)**. 017's tranche
  list chooses rows and never fills a column.
- **The section** of a page is the first `SUMMARY.md` section that links it.

`groups.py` exits 1 unless it can verify three things: every row is in exactly one group, the
FAILED column equals `--report`'s FAILED on the same pages, and every FAILED block on a (V4) page
is on the V4 pin.

#### S1: Scheduler

| Section | Page | FAILED | parse | built-by-using | defect | same-page | values | page-type | BUILT |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Scheduler | `AwsScheduler.md` | 23 | 5 |  | 2 |  | 6 | 10 | 1 |
| Scheduler | `AzureScheduler.md` | 21 | 5 | 6 | 2 |  | 3 | 5 | 5 |
| Scheduler | `BrighterSchedulerSupport.md` | 6 | 1 |  | 2 |  |  | 3 | 1 |
| Scheduler | `CustomScheduler.md` | 4 |  |  | 2 |  |  | 2 | 0 |
| Scheduler | `HangfireScheduler.md` | 27 | 5 |  | 3 | 2 | 13 | 4 | 2 |
| Scheduler | `InMemoryScheduler.md` | 14 | 1 | 1 |  |  | 4 | 8 | 2 |
| Scheduler | `QuartzScheduler.md` | 27 | 5 | 1 | 1 |  | 16 | 4 | 0 |
| Scheduler | `SwitchingSchedulers.md` | 7 | 7 |  |  |  |  |  | 0 |
| **S1** | **8 pages** | **129** | **29** | **8** | **12** | **2** | **42** | **36** | **11** |

S1: 8 pages, 129 FAILED, **3 with nothing BUILT**.

#### S2: Commands, Handlers and Pipelines; Get Started

| Section | Page | FAILED | parse | built-by-using | defect | same-page | values | page-type | BUILT |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Commands, Handlers and Pipelines | `AgreementDispatcher.md` | 14 | 1 |  | 1 |  |  | 12 | 0 |
| Commands, Handlers and Pipelines | `AsyncDispatchARequest.md` | 4 | 1 |  | 1 |  |  | 2 | 0 |
| Commands, Handlers and Pipelines | `DispatchingARequest.md` | 8 |  |  |  |  |  | 8 | 0 |
| Commands, Handlers and Pipelines | `FeatureSwitches.md` | 7 |  |  | 1 |  |  | 6 | 0 |
| Commands, Handlers and Pipelines | `MigratingToPollyV8.md` | 8 |  | 1 | 3 |  | 1 | 3 | 1 |
| Commands, Handlers and Pipelines | `PipelineValidation.md` | 16 | 9 |  | 2 |  | 2 | 3 | 0 |
| Commands, Handlers and Pipelines | `PolicyFallback.md` | 9 |  |  | 1 |  |  | 8 | 0 |
| Commands, Handlers and Pipelines | `PolicyRetryAndCircuitBreaker.md` | 18 | 2 | 1 |  | 1 | 6 | 8 | 1 |
| Commands, Handlers and Pipelines | `UsingTheContextBag.md` | 17 | 1 |  | 1 |  | 2 | 13 | 1 |
| Get Started | `ShowMeTheCode.md` | 6 | 1 |  | 1 |  |  | 4 | 0 |
| **S2** | **10 pages** | **107** | **15** | **2** | **11** | **1** | **11** | **67** | **3** |

S2: 10 pages, 107 FAILED, **7 with nothing BUILT**.

#### S3: Darker; Understanding Brighter

| Section | Page | FAILED | parse | built-by-using | defect | same-page | values | page-type | BUILT |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Darker | `DarkerBasicConfiguration.md` | 11 |  | 1 | 3 |  | 3 | 4 | 0 |
| Darker | `EFCoreQueryIntegration.md` | 5 | 3 |  |  |  | 1 | 1 | 0 |
| Darker | `ImplementAQueryHandler.md` | 21 | 2 | 2 | 1 | 1 |  | 15 | 2 |
| Darker | `QueriesAndQueryObjects.md` | 20 | 5 |  | 1 | 2 |  | 12 | 3 |
| Darker | `QueryPatterns.md` | 5 | 1 |  | 3 |  |  | 1 | 0 |
| Darker | `QueryPipeline.md` | 25 | 12 | 3 | 1 |  | 2 | 7 | 0 |
| Understanding Brighter | `CQRSWithBrighterAndDarker.md` | 11 | 3 | 1 | 3 | 2 |  | 2 | 2 |
| Understanding Brighter | `HowConfiguringTheCommandProcessorWorks.md` | 9 | 1 |  | 1 |  |  | 7 | 1 |
| Understanding Brighter | `HowServiceActivatorWorks.md` | 17 | 4 |  | 3 |  | 1 | 9 | 0 |
| Understanding Brighter | `ReactorAndProactor.md` | 13 |  |  |  |  |  | 13 | 0 |
| **S3** | **10 pages** | **137** | **31** | **7** | **16** | **5** | **7** | **71** | **8** |

S3: 10 pages, 137 FAILED, **6 with nothing BUILT**.

#### S4: Using an External Bus; Transports

| Section | Page | FAILED | parse | built-by-using | defect | same-page | values | page-type | BUILT |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Using an External Bus | `AsyncAPISupport.md` | 11 | 2 |  | 1 | 2 | 3 | 3 | 1 |
| Using an External Bus | `CloudEventsSupport.md` | 10 | 3 |  | 1 |  |  | 6 | 2 |
| Using an External Bus | `DefaultMessageMappers.md` | 13 | 1 |  | 1 |  |  | 11 | 2 |
| Using an External Bus | `DynamicMessageDeserialization.md` | 8 | 3 |  | 1 |  |  | 4 | 0 |
| Using an External Bus | `HandlerFailure.md` | 11 |  |  | 1 |  |  | 10 | 0 |
| Using an External Bus | `ImplementingExternalBus.md` | 3 |  |  | 1 |  |  | 2 | 0 |
| Using an External Bus | `MessageMappers.md` | 6 | 2 |  |  |  |  | 4 | 1 |
| Using an External Bus | `MessageTransforms.md` | 8 | 1 | 2 | 2 |  |  | 3 | 0 |
| Using an External Bus | `Routing.md` | 4 | 1 | 1 |  | 1 |  | 1 | 0 |
| Using an External Bus | `RoutingMultipleMessageTypes.md` | 9 |  |  |  |  | 1 | 8 | 0 |
| Using an External Bus | `S3LuggageStore.md` (V4) | 1 |  |  |  |  | 1 |  | 1 |
| Transports | `AWSSQSConfiguration.md` | 5 | 1 |  | 4 |  |  |  | 5 |
| Transports | `AWSSQSMigrateToV10.md` | 5 |  | 3 | 2 |  |  |  | 2 |
| Transports | `AzureServiceBusConfiguration.md` | 2 | 2 |  |  |  |  |  | 0 |
| Transports | `KafkaConfiguration.md` | 20 | 14 |  |  |  | 1 | 5 | 0 |
| Transports | `RabbitMQConfiguration.md` | 6 | 5 |  | 1 |  |  |  | 0 |
| Transports | `RabbitMQConnectionStability.md` | 3 |  | 1 | 1 |  |  | 1 | 0 |
| **S4** | **17 pages** | **125** | **35** | **7** | **16** | **3** | **6** | **58** | **14** |

S4: 17 pages, 125 FAILED, **10 with nothing BUILT**.

#### S5: Brighter Configuration; V10 Migration; Outbox and Inbox; Reference; Health Checks and Observability

| Section | Page | FAILED | parse | built-by-using | defect | same-page | values | page-type | BUILT |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Brighter Configuration | `BrighterBasicConfiguration.md` | 3 |  |  |  |  |  | 3 | 2 |
| Brighter Configuration | `CommandProcessorConfigurationReference.md` | 21 | 8 | 1 | 2 |  | 1 | 9 | 1 |
| Brighter Configuration | `DispatcherConfigurationReference.md` | 5 | 5 |  |  |  |  |  | 0 |
| Brighter Configuration | `InMemoryOptions.md` | 3 |  |  | 1 |  |  | 2 | 0 |
| Brighter Configuration | `TestDoubleOptions.md` | 12 | 1 | 1 | 6 |  | 1 | 3 | 0 |
| V10 Migration | `MigratingToNullableReferenceTypes.md` | 5 |  |  | 3 |  |  | 2 | 17 |
| V10 Migration | `NullableReferenceTypes.md` | 13 |  | 5 | 4 |  |  | 4 | 3 |
| V10 Migration | `V10MigrationGuide.md` | 18 | 3 | 2 |  | 3 | 1 | 9 | 2 |
| Outbox and Inbox | `BrighterInboxSupport.md` | 2 | 1 |  |  |  |  | 1 | 0 |
| Outbox and Inbox | `BrighterOutboxSupport.md` | 2 |  |  | 2 |  |  |  | 0 |
| Outbox and Inbox | `CausationTrackingStores.md` | 1 |  |  | 1 |  |  |  | 1 |
| Outbox and Inbox | `DistributedLock.md` (V4) | 1 |  |  |  |  | 1 |  | 1 |
| Outbox and Inbox | `DynamoDbDistributedLock.md` (V4) | 2 |  |  |  |  | 2 |  | 0 |
| Outbox and Inbox | `EFCoreOutbox.md` | 2 |  |  |  |  |  | 2 | 0 |
| Outbox and Inbox | `MongoDBInbox.md` | 3 | 1 | 2 |  |  |  |  | 1 |
| Outbox and Inbox | `MongoDBOutbox.md` | 2 | 1 |  | 1 |  |  |  | 3 |
| Outbox and Inbox | `OutboxArchiver.md` | 4 | 2 |  | 1 |  | 1 |  | 0 |
| Outbox and Inbox | `SweeperCircuitBreaking.md` | 3 | 1 | 1 | 1 |  |  |  | 6 |
| Outbox and Inbox | `TurningOnReplayOnSeen.md` | 6 |  |  | 1 |  | 1 | 4 | 0 |
| Reference | `FAQ.md` | 17 | 4 |  | 5 |  | 1 | 7 | 2 |
| Health Checks and Observability | `ConfiguringOpenTelemetry.md` | 6 | 3 |  | 3 |  |  |  | 0 |
| Health Checks and Observability | `Logging.md` | 3 |  |  | 2 |  |  | 1 | 2 |
| **S5** | **22 pages** | **134** | **30** | **12** | **33** | **3** | **9** | **47** | **41** |

S5: 22 pages, 134 FAILED, **10 with nothing BUILT**.

**Totals: 67 pages, 632 FAILED**, of which 140 are parse, 36 built-by-using, 88 defect,
14 same-page, 75 values and 279 page-type. **36** pages have nothing BUILT.

**Two methods agree.**
- `groups.py`'s FAILED column gives **632**, the same as `--report`'s FAILED on those 67 pages.
- `awk` over `--report` and the tranche list alone gives **628** FAILED on **64** off-tranche pages.
  Adding the **4** V4 blocks (`S3LuggageStore.md` #1, `DistributedLock.md` #2,
  `DynamoDbDistributedLock.md` #1, #2) gives 632.
- The other **39** FAILED blocks on tranche pages are not in any group.

**Against the design's working figures:**
- **S1–S3 are identical** in pages and FAILED: 8 / 129, 10 / 107 and 10 / 137.
- **S4 is 16 / 124 → 17 / 125, and S5 is 20 / 133 → 22 / 134.** The three (V4) tranche pages,
  which the task puts in S4 and S5, add 1 and 3 FAILED blocks. S5 also lost 2 at 10.8.0
  (§ *Phase 3 as executed*).
- **Nothing BUILT is 36**: the design's 36 off-tranche pages, less `CausationTrackingStores.md`,
  which gained a BUILT block at 10.8.0, plus `DynamoDbDistributedLock.md`.
- Phase 2's four new BUILT blocks are all on tranche pages, which is why no off-tranche figure moved.
- The design's *Reachable* and *Hard* columns came from the probe. They are replaced by the six
  classes, and nothing here is carried from them.

## Blocks that stay FAILED

*One row per FAILED block on a reached page: page, block, diagnostic, why it stays, phase (AC13).*

| Page | # | Diagnostic | Why it stays | Phase |
|---|---:|---|---|---:|
| `HangfireScheduler.md` | 12 | `CS0246` `HangfireAuthorizationFilter` | *same-page*: block 11 declares it, and the sentence before block 12 says so | 3 |
| `HangfireScheduler.md` | 20 | `CS0246` `HangfireAuthorizationFilter` | *same-page*, as block 12 | 3 |
| `InMemoryScheduler.md` | 14 | `CS0103` `_handlerRegistry` | *same-page*: a test method of block 9's `SchedulingTests`, which declares the field; the sentence before it says so | 3 |
| `QuartzScheduler.md` | 22 | `CS0246` `BrighterJobListener`, `BrighterTriggerListener` | *same-page*: block 15 declares `BrighterJobListener`; the trigger listener is the reader's, "written the same way", as the sentence before it says | 3 |
| `FeatureSwitches.md` | 5 | `CS0246` `MyFeatureSwitchedHandler`, `MyIncompleteHandlerAsync` | *same-page*: blocks 1 and 2 declare them; the sentence before block 5 says so | 4 |
| `PolicyRetryAndCircuitBreaker.md` | 17 | `CS0246` `MyQoSProtectedHandler`; `CS0103` `services` | *same-page*: block 1 declares the handler, as the sentence before block 17 now says | 4 |
| `CQRSWithBrighterAndDarker.md` | 1, 2 | `CS0246` `Customer`, `ICustomerRepository` | *same-page*: block 8 declares `Customer`, which no stub may supply; split from old #1 at 5.3 | 5 |
| `CQRSWithBrighterAndDarker.md` | 3 | `CS0246` `Order`, `OrderItem`, `OrderPlacedEvent`, `IOrderRepository`; `CS0103` `OrderStatus` | *same-page*: block 8 declares the write model, as the sentence before block 3 says | 5 |
| `CQRSWithBrighterAndDarker.md` | 4 | `CS0246` `OrderItemDto`, `ApplicationDbContext` | *same-page*: block 7 declares `OrderItemDto`, as the sentence before block 4 now says; the context's `Orders` are block 8's `Order`, which no stub may supply | 5 |
| `CQRSWithBrighterAndDarker.md` | 5 | `CS0246` `PlaceOrderCommandHandler`, `GetOrderDetailsQueryHandler`, `ApplicationDbContext` | *same-page*: blocks 3, 4, 9 and 11 declare the handlers, as the sentence before block 5 now says | 5 |
| `CQRSWithBrighterAndDarker.md` | 6 | `CS0246` `PlaceOrderCommand`, `GetOrderDetailsQuery`, `PlaceOrderRequest` | *same-page*: blocks 4, 7 and 10 declare the command and the query, as the sentence before block 6 now says | 5 |
| `CQRSWithBrighterAndDarker.md` | 9 | `CS0246` `PlaceOrderCommand`, `Order`, `IOrderRepository`, `IProductRepository` … | *same-page*: blocks 7 and 8, as the sentence before block 9 now says | 5 |
| `CQRSWithBrighterAndDarker.md` | 11 | `CS0246` `GetOrderDetailsQuery`, `OrderDetailsDto`, `OrderItemDetailsDto`, `ApplicationDbContext` | *same-page*: block 10 declares the query and its DTOs, as the sentence before block 11 now says | 5 |
| `CQRSWithBrighterAndDarker.md` | 12–14 | `CS0246` `PlaceOrderCommand` (12, 13), `GetOrderDetailsQuery`, `OrderDetailsDto` (14) | *same-page*: methods of the e-commerce example's handlers, as the sentence under *CQRS Common Pitfalls* now says | 5 |
| `ImplementAQueryHandler.md` | 3, 5, 6, 14 | `CS0246` `GetPeopleQuery` (and #3, #5 `PersonRepository`; #6 `IPersonRepository`) | *same-page*: block 1 declares the query, which its lead-in says the guide uses throughout | 5 |
| `ImplementAQueryHandler.md` | 16 | `CS0246` `GetPeopleQuery`, `GetPeopleQueryHandler`, `GetOrderQueryHandler` | *same-page*: blocks 1, 3 and 10 declare them. At 5.5 it lost `GetPersonQueryHandler`, which no block declared, and gained a factory for the handler that takes a repository | 5 |
| `ImplementAQueryHandler.md` | 21, 23 | `CS0246` `OrderNotFoundException`; #23 also `OrderAccessDeniedException`, `IAuthService`, and `CS1061` `Order.IsDeleted` | *same-page*: block 22 declares both exceptions, as the sentences before blocks 21 and 23 now say. `IAuthService` and `IsDeleted` are the reader's, and no unit may stub them while no BUILT block names them (unit rule 1) | 5 |
| `QueryPatterns.md` | 6 | `CS0246` `GetProductCatalogQuery` | *same-page*: block 5 declares it, as the sentence before block 6 now says | 5 |
| `AsyncAPISupport.md` | 4 | `CS0103` `rmqConnection`; `CS0246` `OrderCreatedEvent` | *same-page*: block 5 declares `OrderCreatedEvent`, as the sentence before block 4 now says. `rmqConnection` is the reader's, and no unit may stub it while no BUILT block names it (unit rule 3) | 6 |
| `AsyncAPISupport.md` | 7 | `CS0246` `OrderCreatedEvent` | *same-page*: block 5 declares it, as the sentence before block 7 now says | 6 |
| `AsyncAPISupport.md` | 9 | `CS0246` `MySchemaGenerator` | *same-page*: block 8 declares it, and the sentence before block 9 says to register your implementation | 6 |
| `AsyncAPISupport.md` | 11, 12 | `CS0246` `OrderCreatedEvent` | *same-page*: block 5 declares it, as the sentence under *Complete Examples* now says | 6 |
| `Routing.md` | 3 | `CS0246` `MyRequest` | *same-page*: block 2 declares it, and the sentence before block 3 names "this request" | 6 |
| `Routing.md` | 4 | `CS0246` `MyReply` | *same-page*: block 2 declares it, as the sentence before block 4 now says. Block 3 and block 4 build once block 2's classes are in scope; each was checked that way (0 diagnostics) before the stub was taken out again | 6 |
| `DefaultMessageMappers.md` | 6, 10 | `CS0246` `Orders`, `OrderShipped`, `AvroMessageMapperAsync<>` | *same-page*: block 4 declares the mapper and block 5 `OrderShipped`, in the namespace `Orders`; the sentences before blocks 6 and 10 are about that mapper | 6 |
| `NullableReferenceTypes.md` | 4 | `CS0246` `CreateOrderCommand`, `IOrderRepository`, `Order` | *same-page*: block 3 declares `CreateOrderCommand`, as the clause before block 4 now says. `IOrderRepository` and `Order` are the reader's, and no BUILT block names them (unit rule 1) | 7 |
| `NullableReferenceTypes.md` | 5 | `CS0246` `CreateOrderCommand`, `CreateOrderDto` | *same-page*: block 3 declares the command, as the clause before block 5 now says; `CreateOrderDto` is the reader's | 7 |
| `NullableReferenceTypes.md` | 7 | `CS0246` `CreateUserCommand`, `CreateUserDto` | *same-page*: block 6 declares the command, as the clause before block 7 now says; `CreateUserDto` is the reader's | 7 |
| `V10MigrationGuide.md` | 25 | `CS0103` `BuildServiceProvider`; `CS0246` `PersonCreated` | *same-page*: block 19 declares `PersonCreated`, and the sentence before block 25 says so; `BuildServiceProvider` is the reader's own helper, named by no other block | 7 |
| `V10MigrationGuide.md` | 16 | `CS0246` `PersonCreated` | *same-page*: block 19 declares it, and the clause before block 16 now says so. It was *parse* (a bare `new RmqPublication { … }` statement) until 7.3 made it `var publication = …` | 7 |
| `Logging.md` | 5 | `CS8795` `BuildingSendPipelineForCommand` | *source generator*: a quote of `CommandProcessor.Log`'s `[LoggerMessage]` partial (`CommandProcessor.cs:1615–1618` at 10.8.1), whose implementation a generator writes and `blockcheck` does not run. Listed, not skipped: D21's reason names a private member or the pump's own state, which a logging partial is neither. The lead-in now cites the file **Ruled 2026-11-01: option (a).** At 7.4 the block is whole, a top-level `internal static partial class Log`, and the lead-in no longer calls it verbatim; it still fails `CS8795` because `blockcheck` compiles with no analyzers and so runs no generator. `probe/s5/logger-message`: with the generator, 0 errors; with every analyzer removed, `CS8795`; the former top-level `private` form, `CS1527`. **Ruled 2026-11-01: (c).** It stays listed on `CS8795`; `blockcheck` running source generators is a follow-up after spec 018 | 7 |

## Splits

*One row per split fence: page, old ordinal, new ordinals (017 obligation 16).*

| Page | Old # | New # | Phase |
|---|---:|---|---:|
| `BrighterSchedulerSupport.md` | 2 | 2, 3 | 3 |
| `BrighterSchedulerSupport.md` | 3 | 4, 5 | 3 |
| `HangfireScheduler.md` | 25 | 25, 26 | 3 |
| `QuartzScheduler.md` | 23 | 23, 24 | 3 |
| `PipelineValidation.md` | 3 | 3, 4 | 4 |
| `CQRSWithBrighterAndDarker.md` | 1 | 1, 2 | 5 |
| `EFCoreQueryIntegration.md` | 1 | 1, 2 | 5 |
| `QueryPatterns.md` | 3 | 3, 4 | 5 |

## Defect ledger

*Each row written only after its grep has run as written (obligation 22). These three were found at
design, and their greps were run there.*

| # | Defect | Page(s) | Recurrence grep | Before | After | Found by | Phase |
|---:|---|---|---|---:|---:|---|---:|
| 1 | `AddServiceActivator` called as current code; dead at `10.7.0` and `master` (doc comments only) | `AwsScheduler.md`, `AzureScheduler.md` | `grep -rn 'Services.AddServiceActivator(' contents/` | **2** | **0** | re-derivation (design E2); the watchlist missed it | 3 |
| 2 | Shouldly assertions with the package never named and no `using` | `TestDoubleOptions.md` | `grep -c Shouldly contents/TestDoubleOptions.md` | **0** | **7** (≥ 7: a `using Shouldly;` in each of the six blocks and one sentence naming the package) | the second compile (design E2, *pin-gap*); repaired at 7.4 | 7 |
| 3 | Package spelt `Paramore.Brighter.Inbox.DynamoDb.V4`; the project is `…DynamoDB.V4`. Widened at 7.6 by shape, every DynamoDB Inbox and Outbox package spelt `…DynamoDb`: the v3 Inbox and Outbox packages too, whose ids are `Paramore.Brighter.Inbox.DynamoDB` and `Paramore.Brighter.Outbox.DynamoDB` (each package's `.nuspec` at 10.8.1) | `DynamoInbox.md`; `DynamoOutbox.md` (017 tranche pages; repaired under obligation 21) | `grep -rn 'Inbox.DynamoDb.V4' contents/`; `grep -rnE 'Paramore\.Brighter\.(Inbox\|Outbox)\.DynamoDb' contents/` | **1**; **3** | **0**; **0** | re-derivation (requirements, the V4 pin); repaired at 7.6 | 7 |
| 4 | `UseExternalBus` called as current code; dead at `10.8.0` and `master` (one doc comment) | `AwsScheduler.md`, `AzureScheduler.md` | `grep -rn '^\.UseExternalBus(' contents/` | **2** | **0** | task 3.1, chained after defect 1 | 3 |
| 5 | `AzureServiceBusSubscription<T>` given `deadLetterQueueHandling:` and `lockTimeout:`, parameters no overload has at 10.8.0; both settings live on `AzureServiceBusSubscriptionConfiguration` | `AzureScheduler.md` | `grep -rnE 'deadLetterQueueHandling\|lockTimeout: 60' contents/` | **2** | **0** | task 3.1, the second compile (`probe/second.py`) | 3 |
| 6 | A scheduler class implementing `IAmA(Message\|Request)Scheduler(Async\|Sync)` with two of each interface's five members, and no `CancellationToken` | `CustomScheduler.md` | `grep -rnE 'class \w+ : IAmA(Message\|Request)SchedulerAsync' contents/` | **2** | **2**, both whole: each class implements all five members of each interface, and both blocks build | task 3.1, the second compile | 3 |
| 7 | `SqsSubscription<T>` given `SubscriptionName`, `ChannelName`, `RoutingKey` positionally, which no overload takes in that order | `AwsScheduler.md` | `grep -rn -A1 'new SqsSubscription<' contents/ \| grep -E '^\S+-\s+new SubscriptionName\('` | **1** | **0** | task 3.1, the second compile, V4 pin | 3 |
| 8 | `UseScheduler(provider => …)` whose lambda returns a `QuartzSchedulerFactory` on one branch and a `HangfireMessageSchedulerFactory` on the other; `UseScheduler<T>` cannot infer one `T` | `SwitchingSchedulers.md` | `git grep -n -A12 'UseScheduler(provider =>' <ref> -- contents/ \| grep -c 'return new HangfireMessageSchedulerFactory'` | **1** | **0** | task 3.3, hidden behind *parse* | 3 |
| 9 | `UseScheduler` called on `IServiceCollection`; it extends `IBrighterBuilder` | `InMemoryScheduler.md`, `QuartzScheduler.md` | `grep -rnE '[Ss]ervices\.UseScheduler\(' contents/` | **4** | **0** | task 3.2, hidden behind *values* | 3 |
| 10 | `ServiceBusDefaultAzureCredentialClientProvider`; the class is `ServiceBusDefaultAzureClientProvider` | `AzureScheduler.md` | `grep -rn ServiceBusDefaultAzureCredentialClientProvider contents/` | **1** | **0** | task 3.2 | 3 |
| 11 | Quartz `UseClustering` called on the configurator `q`; at 3.22.4 it is on the persistent store's options | `QuartzScheduler.md` | `grep -rnE 'q\.UseClustering' contents/` | **5** | **0** | task 3.3 | 3 |
| 12 | `store.UseJsonSerializer()`, `[Obsolete]` at Quartz.Serialization.Json 3.22.4 for `UseNewtonsoftJsonSerializer()`; the package was also unpinned (D12) | `QuartzScheduler.md` | `grep -rn UseJsonSerializer contents/` | **7** | **0** | task 3.3 | 3 |
| 13 | `q.UseMisfireHandler(…)` and `q.UseProperties`, neither of which the Quartz configurator has; `MisfireThreshold` is a property of `q`, and `UseProperties` one of the store's | `QuartzScheduler.md` | `grep -rnE 'UseMisfire.?Handler\|q\.UseProperties' contents/` | **2** | **0** | task 3.3 | 3 |
| 14 | `GetOrCreateMessageSchedulerId` set on `QuartzSchedulerFactory`, whose message hook is `GetOrCreateSchedulerId` (as the page's own table says) | `QuartzScheduler.md` | `grep -n 'GetOrCreateMessageSchedulerId =' contents/QuartzScheduler.md` | **1** | **0** | task 3.3 | 3 |
| 15 | Hangfire.PostgreSql's `SchemaName` set on the bootstrapper options; it is on `PostgreSqlStorageOptions` | `HangfireScheduler.md` | `grep -rnE 'options\.SchemaName' contents/` | **1** | **0** | task 3.2 | 3 |
| 16 | `AllowAllDashboardAuthorizationFilter` used as if Hangfire shipped it; no pinned package declares it | `HangfireScheduler.md` | `grep -rn AllowAllDashboardAuthorizationFilter contents/` against `grep -rn 'class AllowAllDashboardAuthorizationFilter' contents/` | **1 / 0** | **2 / 1**: used once, declared once | task 3.2 | 3 |
| 17 | A test fixture passing an object to `.Handlers(…)`, which takes an `Action<IAmASubscriberRegistry>`, and setting a request's `Id` from a `Guid`; `Id` converts only from `string` | `InMemoryScheduler.md` | `grep -rnF '.Handlers(_' contents/`; `grep -rnF 'new TestCommand { Id = Guid.NewGuid() }' contents/` | **1**; **2** | **0**; **0** | task 3.3 | 3 |
| 18 | `HandleAsync(FireAzureScheduler …)` shown as the reader's handler; Brighter's `AzureSchedulerFiredHandler` handles it | `AzureScheduler.md` | `grep -rnF 'Task<FireAzureScheduler> HandleAsync' contents/` | **1** | **0** | task 3.3 | 3 |
| 19 | `SchedulerType` and `FireRequestMessage`, neither in Brighter 10.8.0; they are `RequestSchedulerType` and `FireSchedulerRequest` | `CustomScheduler.md` | `grep -rnE '[^t]SchedulerType type\|FireRequestMessage' contents/` | **4** | **0** | task 3.3, with ledger 6 | 3 |
| 20 | `new HangfireMessageSchedulerFactory()` while services are configured; it throws, since its `Client` default reads Hangfire's `JobStorage.Current` before `AddHangfire` has set it (BrighterCommand/Brighter#4600) | `BrighterSchedulerSupport.md`, `HangfireScheduler.md`, `InMemoryOptions.md`, `InMemoryScheduler.md`, `SchedulingAMessage.md`, `SwitchingSchedulers.md` | `grep -rnE 'new HangfireMessageSchedulerFactory(\(\)\|$)' contents/` | **12** | **0** | task 3.5, `probe/s1/hangfire-factory` | 3 |
| 21 | A V9 handler shown as current: `new List<Guid>()` for the ids `DepositPostAsync` returns (`Id` at V10), a `_uow`, DapperExtensions' `Predicates.Field` and `GetListAsync`, which no pinned package has | `DispatchingARequest.md`; recurs on `BrighterOutboxSupport.md`, `EFCoreOutbox.md`, `MongoDBOutbox.md` | `grep -rnF 'new List<Guid>()' contents/` | **4** | **0** | task 4.2, hidden behind *page-type* | 4 |
| 22 | `policyRegistry.Add("MyCircuitBreakerPolicy", policy)` registers the retry policy of the block before under the circuit breaker's name | `MigratingToPollyV8.md` | `grep -rnF 'CircuitBreakerPolicy", policy)' contents/` | **1** | **0** | task 4.2, hidden behind *values* | 4 |
| 23 | A Polly property `"BrighterContext"` said to hold the request context; Brighter adds none, and runs the pipeline with `RequestContext.ResilienceContext` | `PolicyRetryAndCircuitBreaker.md` | `grep -rnF '"BrighterContext"' contents/` | **1** | **0** | task 4.2, hidden behind *values* | 4 |
| 24 | `UseTypePipeline = true` said to look up `"<handler type full name>.<policy>"`; Brighter calls `GetPipeline<TRequest>(policy)` | `PolicyRetryAndCircuitBreaker.md` | `grep -rnF 'handlerTypeName}.' contents/` | **2** | **0** | task 4.2, `probe/s2/type-pipeline` | 4 |
| 25 | `AddHedging` on an untyped pipeline builder; hedging needs one typed for its result | `PolicyRetryAndCircuitBreaker.md` | `grep -rnF 'TryAddBuilder("MyHedgingPipeline"' contents/` | **1** | **0** | task 4.2, hidden behind *values* | 4 |
| 26 | A missing pipeline said to throw `InvalidOperationException: Resilience pipeline with key … not found`; `Send` throws `ConfigurationException`, whose inner `KeyNotFoundException` names the key | `PolicyRetryAndCircuitBreaker.md` | `grep -rnF 'InvalidOperationException: Resilience pipeline' contents/` | **2** | **0** | task 4.2, `probe/s2/type-pipeline` | 4 |
| 27 | `[Validation(step:, timing:)]` shown as an attribute beside Brighter's; no Brighter 10.8.0 package declares one | `MigratingToPollyV8.md` | `grep -rnF '[Validation(' contents/` | **2** | **0** | task 4.1, read; removed at 4.2 | 4 |
| 28 | `RegisterAsync` called inside `.Handlers(…)`, whose registry has no `RegisterAsync`; it belongs in `.AsyncHandlers(…)` | `AgreementDispatcher.md` | `grep -rn -A30 '\.Handlers(registry' contents/ \| grep -c RegisterAsync` | **1** | **0** | task 4.1, the second compile | 4 |
| 29 | `StatusOf<MyIncompleteHandler>()`, a handler the page never declares; it declares `MyIncompleteHandlerAsync` | `FeatureSwitches.md` | `grep -rnF 'MyIncompleteHandler>' contents/` | **1** | **0** | task 4.2 | 4 |
| 30 | `RequestContextBagNames` listed without `CausationId`, which 10.8.0 added | `UsingTheContextBag.md` | `grep -c CausationId contents/UsingTheContextBag.md` | **0** | **1** | task 4.2 | 4 |
| 31 | `Register<T>(routingFunc: …)`; the parameter is `router` on every `Register` and `RegisterAsync` overload at 10.8.0 | `AgreementDispatcher.md` | `grep -rn routingFunc contents/` | **2** | **0** | task 4.3, hidden behind *parse* | 4 |
| 32 | `IRequestContext.Span` given OpenTelemetry's `TelemetrySpan` API: `SetAttribute` and `AddEvent(string)`; it is a `System.Diagnostics.Activity`, with `SetTag` and `AddEvent(ActivityEvent)` | `UsingTheContextBag.md`; recurs on `CloudEventsSupport.md` | `grep -rnE 'Span\??\.(SetAttribute\(\|AddEvent\(")' contents/` | **8** | **0** | task 4.1, the second compile | 4 |
| 33 | `int.Seconds()`, a test library's extension no pinned package declares | `MigratingToPollyV8.md` | `grep -rnE '[0-9]\.Seconds\(\)' contents/` | **4** | **0** | task 4.1, the second compile | 4 |
| 34 | .NET Framework's `ConfigurationManager.AppSettings` in a V10 registry, which also called `.ToLower()` on a missing key (D14) | `FeatureSwitches.md` | `grep -rn 'ConfigurationManager.AppSettings' contents/` | **1** | **0** | task 4.1, the second compile | 4 |
| 35 | A query handler without the sample's `where p.Name = @name`, returning every person's greetings, then `Single()`; and an `AddGreeting { PersonId }` the sample does not have (D15) | `ShowMeTheCode.md` | `grep -c 'where p.Name = @name' contents/ShowMeTheCode.md`; `grep -rn 'addGreeting.PersonId' contents/` | **0**; **1** | **1**; **0** | task 4.3, read against `samples/WebAPI/WebAPI_Dapper` at 10.8.0 | 4 |
| 36 | A MongoDB handler taking `IAmATransactionConnectionProvider`, which nothing registers for MongoDB, casting it to `MongoDbUnitOfWork`, then reading `unitOfWork.Database` and `.Session`, which that type does not have at 10.8.0 | `MongoDBOutbox.md`; the claim recurs on `BrighterOutboxSupport.md` | `grep -rnE 'unitOfWork\.(Database\|Session)\|as MongoDbUnitOfWork' contents/` | **3** | **0** | task 4.3, read beside ledger 21; `probe/s5/mongo-outbox` (D17) | 4 |
| 37 | A route returning several handlers for a *Command*, which `Send` refuses; and a handler list written as a sequence ("Always validate", "Always finalize"), which `Publish` runs in parallel | `AgreementDispatcher.md`; the claim recurs on `AgreementDispatcherRouting.md` | `grep -rnF 'Always validate' contents/`; `grep -rnF 'Supports multiple handlers' contents/` | **1**; **1** | **0**; **0** | task 4.4, `probe/s2/agreement-dispatcher`; the recurrence at 4.5 | 4 |
| 38 | Two `[UsePolicy]` attributes on one method advised; `UsePolicyAttribute` has no `AllowMultiple`, so it is `CS0579` | `MigratingToPollyV8.md` | `grep -rn 'earlier step than' contents/` | **1** | **0** | task 4.4, `probe/s2/legacy-policy-order` | 4 |
| 39 | `new PolicyRegistry()` handed to Brighter, which throws `ConfigurationException` without `CommandProcessor.RETRYPOLICY` and `CIRCUITBREAKER`; `DefaultPolicy` holds both | `MigratingToPollyV8.md`; recurs on `HowConfiguringTheCommandProcessorWorks.md` | `grep -rnF 'new PolicyRegistry()' contents/` | **3** | **0** | task 4.4, `probe/s2/legacy-policy-order` | 4 |
| 40 | #4414's fix said to be *"merged and not yet released"*, with a workaround *"until it is"*; it is in 10.8.0, the pinned release | `SchedulingAMessage.md`; the framing recurs on `InMemoryScheduler.md`, `PostgreSQLMessageBroker.md` | `grep -rnF 'merged and not yet released' contents/` | **1** | **0** | task 4.5, `probe/s1/scheduler-autoscan` | 4 |
| 41 | #4335 said unfixed: a consumer-only application gets no global Inbox, *"in no release yet"*; 10.8.0 fixes it | `BrighterInboxSupport.md`; recurs on `DynamoInbox.md`, `FirestoreInbox.md`, `InMemoryInbox.md`, `MongoDBInbox.md`, `MSSQLInbox.md`, `MySQLInbox.md`, `PostgresInbox.md`, `SpannerInbox.md`, `SqliteInbox.md` | `grep -rnE 'In Brighter 10\.7\.0 this configuration\|in no release yet' contents/` | **10** | **0** | task 4.5a, `probe/s5/inbox-no-producers` | 4 |
| 42 | #4437 stated as current: the InMemory scheduler cannot cancel or reschedule a command processor's request; 10.8.0 fixes it | `SchedulingAMessage.md`, `FAQ.md`, `InMemoryScheduler.md` | `grep -rnE 'At Brighter 10\.7\.0 the InMemory scheduler' contents/` | **3** | **0** | task 4.5a, `probe/s1/inmemory-cancel` | 4 |
| 43 | `OnceOnlyAction.Replay` said to be *"not in a released package yet"*; it is in 10.8.0 | `DispatcherConfigurationReference.md` | `grep -rnF 'not in a released package yet' contents/` | **1** | **0** | task 4.5a, `probe/s5/replay-released` | 4 |
| 44 | #4432 stated as current: Decompress misses a body `WrapAsync` compressed; 10.8.0 fixes it | `Compression.md` | `grep -rnF 'At 10.7.0, Decompress does not' contents/` | **1** | **0** | task 4.5a, `probe/s5/compress-async` | 4 |
| 45 | #4510 stated as current: no Command Processor span without an external bus; 10.8.0 fixes it | `Telemetry.md`, `ConfiguringOpenTelemetry.md` | `grep -rnE 'is given the tracer only when\|Command Processor takes that tracer only when' contents/` | **2** | **0** | task 4.5a, `probe/s5/tracer-no-bus` | 4 |
| 46 | #4453 and #4454 stated as current monitoring limits; 10.8.0 fixes both | `Monitoring.md` | `grep -rnF 'Two defects in Brighter 10.7.0' contents/` | **1** | **0** | task 4.5a, `probe/s5/monitor-defects` | 4 |
| 47 | The control-bus sender registered only as `IAmAControlBusSender`; `[MonitorAsync]` resolves `IAmAControlBusSenderAsync`, so it fails at 10.7.0 and 10.8.0 | `Monitoring.md` | `grep -c 'IAmAControlBusSenderAsync)controlBusSender' contents/Monitoring.md` | **0** | **1** | task 4.5a, `probe/s5/monitor-defects` | 4 |
| 48 | `InstrumentationOptions` listed without `CircuitBreaker`, which 10.8.0 added to `All` | `Telemetry.md` | `grep -cE '^\| `CircuitBreaker` \|' contents/Telemetry.md` | **0** | **1** | task 4.5a, read | 4 |
| 49 | #4443 and #4444 stated as current: the DynamoDB and Spanner Outboxes ignore `TrippedTopics`; 10.8.0 fixes both | `SweeperCircuitBreaking.md` | `grep -rnF 'it accepts the list and ignores it' contents/` | **1** | **0** | task 4.5a, `probe/s5/tripped-topics` | 4 |
| 50 | `messaging.system` said to be always `internal_bus` on pump spans; at 10.8.0 it is the transport's | `Telemetry.md` | `grep -rnF 'Always `internal_bus` on pump spans' contents/` | **1** | **0** | task 4.5a, `probe/s5/messaging-system` | 4 |
| 51 | Relational DDL that is not 10.8.0's `GetDDL`: three blocks 10.7.0's output, without `CausationId`, and three no release generates, one of them invalid SQLite | `MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md`, `SqliteOutbox.md`, `SpannerOutbox.md`, `SpannerInbox.md` | `grep -LiE causationid` on the six pages | **6** | **0** | task 4.5b, `probe/s5/ddl-refresh` | 4 |
| 52 | Swashbuckle's `AddSwaggerGen`, `UseSwagger` and `UseSwaggerUI` in a Darker setup, from a package the page never names and no pin carries | `DarkerBasicConfiguration.md` | `grep -rnE 'AddSwaggerGen\|UseSwagger' contents/` | **3** | **0** | task 5.1, the second compile; removed at 5.2 | 5 |
| 53 | The message pump's private calls misquoted: `TranslateMessage` with two arguments (10.8.0 takes three), `InvokeDispatchRequest(request, message, context)` (it takes `request, context`), the Reactor's dispatch shown as `InitRequestContext`, an awaited call labelled Reactor, and translation before the context it uses is created | `HowServiceActivatorWorks.md` | `grep -rnE 'TranslateMessage\(message, context\)\|InvokeDispatchRequest\(request, message' contents/` | **3** | **0** | task 5.2, hidden behind *page-type*; read against `Reactor.cs`, `Proactor.cs` at 10.8.0 | 5 |
| 54 | A route returning `Type[]`; at 10.8.0 `RegisterAsync<TRequest>`'s router returns `List<Type>` | `HowServiceActivatorWorks.md` | `grep -rnE 'return new( Type)?\[\] ?\{ ?typeof' contents/` | **2** | **0** | task 5.2, hidden behind *page-type* | 5 |
| 55 | Darker's interfaces listed short of 4.1.1: `IQuery<TResult>` without its `IQuery` base, `IQueryHandler<,>` with two of its four methods and without `IQueryHandler.Context`; and a handler implementing the short listing | `QueriesAndQueryObjects.md`, `ImplementAQueryHandler.md` | `grep -rnE 'interface IQuery<out TResult>$\|interface IQueryHandler<in TQuery, TResult> where' contents/`; `grep -rnE 'public Order Fallback\(GetOrderQuery' contents/` | **2**; **0** | **0**; **1** | task 5.1, the second compile (`ImplementAQueryHandler.md` #13); the listings read at 5.2 | 5 |
| 56 | A V9 handler factory shown as current: `Create(Type)` and `Release(IHandleRequests)` on `IAmAHandlerFactory`, which at 10.8.0 declares only `CreatePipelineScope()`; the members are `IAmAHandlerFactorySync`'s, each with an `IAmALifetime` | `HowConfiguringTheCommandProcessorWorks.md` | `grep -rnE 'IHandleRequests\??\s+Create\(Type \w+\)\|void Release\(IHandleRequests \w+\)\|: IAmAHandlerFactory(\s\|$\|,\|\{)' contents/` | **3** | **0** | task 5.1, the second compile | 5 |
| 57 | The Dispatcher's instrumentation set on `AddBrighter`'s options, which configure the Command Processor; the Dispatcher reads `AddConsumers`' (`ServiceCollectionExtensions.cs:180`) | `HowServiceActivatorWorks.md` | `grep -rn -B2 'options.InstrumentationOptions = InstrumentationOptions.All;' contents/ \| grep -c AddBrighter` | **1** | **0** | task 5.3, hidden behind *parse*. Not a recurrence: `Telemetry.md` and `ConfiguringOpenTelemetry.md` set the Command Processor's on `AddBrighter` and the Dispatcher's on `AddConsumers`; run at 5.4 (`probe/s3/dispatcher-instrumentation`), confirmed | 5 |
| 58 | A control-bus sender's `Send`, and `ConfigurationCommandType.StopChannel`, `.StartChannel` with a settable string `SubscriptionName`; at 10.8.0 the sender has `Post`, the members are `CM_STOPCHANNEL`, `CM_STARTCHANNEL`, and the constructor takes a `SubscriptionName` | `HowServiceActivatorWorks.md` | `grep -rnE 'ConfigurationCommandType\.(Stop\|Start)Channel\|controlBusSender\.Send\(' contents/` | **2** | **0** | task 5.1, the second compile | 5 |
| 59 | Dead-letter parameters on `Subscription<T>`, which has none; at 10.8.0 only `RmqSubscription` takes them | `HowServiceActivatorWorks.md` | `grep -rn -B4 deadLetterChannelName contents/ \| grep -c 'new Subscription<'` | **1** | **0** | task 5.1, the second compile | 5 |
| 60 | A `[FallbackPolicy]` handler without `FallbackAsync` said to fail at runtime; `QueryHandlerAsync.FallbackAsync` returns `default(TResult)` (`QueryHandlerAsync.cs` at 4.1.1) | `QueryPipeline.md` | `grep -rnE 'or you.ll get a runtime error' contents/` | **1** | **0** | task 5.3, read against 4.1.1; run at 5.4 (`probe/s3/darker-pipeline`), confirmed | 5 |
| 61 | A Darker policy registry holding only a custom name, passed to `AddPolicies`, which throws `ConfigurationException` without `Constants.RetryPolicyName` and `Constants.CircuitBreakerPolicyName` (`QueryProcessorBuilderExtensions.cs` at 4.1.1) | `QueryPipeline.md` | `grep -rn -B10 '\.AddPolicies(' contents/ \| grep -c 'new PolicyRegistry()'` | **1** | **0** | task 5.4, read against 4.1.1 and run (`probe/s3/darker-pipeline`) | 5 |
| 62 | `AddBrighter`'s options said to configure the Command Processor beside `AddConsumers`; with `AddConsumers` alone, its options configure both, `ConsumersOptions` being a `BrighterOptions` | `HowServiceActivatorWorks.md` | `grep -rn 'separately, from `AddBrighter`' contents/` | **1** | **0** | task 5.4, run (`probe/s3/dispatcher-instrumentation`); written at 5.3 with ledger 57 | 5 |
| 63 | Manual registration building a handler that takes a repository with `Activator.CreateInstance`, which throws `MissingMethodException`; and registering a `GetPersonQueryHandler` no block declares | `ImplementAQueryHandler.md` | `grep -rn 'GetPersonQueryHandler' contents/ImplementAQueryHandler.md` | **1** | **0** | task 5.5, run (`probe/s3/darker-pipeline`) | 5 |
| 64 | A domain exception named `UnauthorizedAccessException`, hiding `System.UnauthorizedAccessException` | `ImplementAQueryHandler.md` | `grep -rn 'class UnauthorizedAccessException' contents/` | **1** | **0** | task 5.5, read | 5 |
| 65 | `name:` for `subscriptionName:` on `SqsSubscription<T>`, whose constructors take `subscriptionName` (`CS1739`) | `AWSSQSConfiguration.md` | `grep -rnE 'name: new SubscriptionName' contents/` | **4** | **0** | task 6.1, the second compile; repaired at 6.2 | 6 |
| 66 | `circuitBreakerTimeInMilliseconds` for `circuitBreakTimeInMilliseconds`, the name `AmqpUriSpecification` takes (`CS1739`); once in a block, once in a block hidden by a parse error, once in prose | `RabbitMQConnectionStability.md`, `RabbitMQConfiguration.md` | `grep -rn circuitBreakerTimeInMilliseconds contents/` | **3** | **0** | task 6.1, the second compile; the prose and the parse block repaired with it (obligation 21) | 6 |
| 67 | `RabbitMQProducerRegistryFactory`, a type that does not exist; both RMQ packages have `RmqProducerRegistryFactory` | `CloudEventsSupport.md`, `DefaultMessageMappers.md` | `grep -rn RabbitMQProducerRegistryFactory contents/` | **2** | **0** | task 6.2, hidden behind *page-type* | 6 |
| 68 | A plain `Publication` given to a transport's producer registry factory, which takes the transport's own: `RmqPublication` for RMQ, `KafkaPublication` for Kafka (`CS1503`) | `CloudEventsSupport.md`, `DefaultMessageMappers.md`, `RoutingMultipleMessageTypes.md` | `grep -rn -B40 -A14 -E 'new (Rmq\|Kafka\|RabbitMQ)ProducerRegistryFactory\(' contents/ \| grep -E 'new Publication$' \| sort -u \| wc -l` | **7**, of which 5 are this defect | **1** | task 6.2, hidden behind *page-type*. Of the 2 other hits, `InMemoryOptions.md`'s factory takes `Publication`, and `CloudEventsSupport.md` #2 passes its `Publication` to no factory. The latter has left the window since (the `using`s above it); the former is the 1 | 6 |
| 69 | `new KafkaMessagingGatewayConfiguration { /* ... */ }` and `{...}`, leaving `BootStrapServers`, which is `required`, to a comment or an ellipsis (`CS9035`) | `RoutingMultipleMessageTypes.md`; recurs on `KafkaConfiguration.md` | `grep -rnF 'KafkaMessagingGatewayConfiguration { /* ... */ }' contents/`; `grep -rnF 'KafkaMessagingGatewayConfiguration {...}' contents/` | **1**; **2** | **0**; **0** | task 6.2, hidden behind *page-type*; the recurrence hidden behind *parse* (`KafkaConfiguration.md` #7, #8), repaired at 6.3 (obligation 21) | 6 |
| 70 | `compressionType: CompressionType.Gzip` for `compressionMethod: CompressionMethod.GZip`, the argument and enum `CompressAttribute` takes (`CompressAttribute.cs:47`, `CompressionMethod.cs:3` at 10.8.1) | `MessageTransforms.md` | `grep -rnE 'compressionType\|CompressionType\.' contents/` | **2** | **0** | task 6.1, the second compile; repaired at 6.3 | 6 |
| 71 | A V9 handler shown as current: LibLog's `ILog` and `LogProvider`, and a constructor taking a logger for `RequestHandler`, none of which exist at 10.8.1 | `ImplementingExternalBus.md` | `grep -rnE 'LogProvider\|\bILog\b\|: base\(logger\)' contents/` | **2** | **0** | task 6.1, the second compile; repaired at 6.3 | 6 |
| 72 | `[RemovePII]` and `[RestorePII]` shown as transforms Brighter ships; it ships `ClaimCheck`/`RetrieveClaim`, `Compress`/`Decompress`, `CloudEvents` and the JustSaying pair, and the page declares neither. Replaced by the shipped `Compress` then `ClaimCheck` on send, `RetrieveClaim` then `Decompress` on receive | `MessageTransforms.md` | `grep -rnE 'RemovePII\|RestorePII' contents/` | **2** | **0** | task 6.1, the second compile; repaired at 6.3 | 6 |
| 73 | `request.Id = Guid.Parse(…)`, a `Guid` assigned to `Id`, which converts only from `string` (`CS0029`); and `new JProperty("Id", request.Id)`, which Newtonsoft cannot build from the `Id` record (`ArgumentException`), so the mapper on the page throws when it serialises | `Routing.md` | `grep -rnF 'request.Id = Guid.Parse' contents/`; `grep -rnF 'new JProperty("Id", request.Id)' contents/` | **1**; **1** | **0**; **0** | task 6.3, hidden behind *same-page* (#3, `MyRequest` unresolved, so `request.Id` was untyped) and *parse* (#4); `probe/s4/routing-id` | 6 |
| 74 | `new Uri("/orders/12345")` shown as a good, resource-specific CloudEvents `source`; with the default `UriKind` it is the absolute `file:///orders/12345` on macOS and Linux and a `UriFormatException` on Windows. The relative reference needs `UriKind.Relative` | `CloudEventsSupport.md` | `grep -rnF 'new Uri("/orders/12345")' contents/` | **1** | **0** | task 6.3, hidden behind *parse*; a scratch `Uri` probe (macOS, .NET 9) | 6 |
| 75 | `builder.Services.AddConsumers(…)` inside `ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)`, where `builder` is declared nowhere; the method's own `services` is the collection | `AzureServiceBusConfiguration.md` | `grep -rn -B40 'builder.Services.AddConsumers' contents/ \| grep -c 'ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)'` | **1** | **0** | task 6.3, hidden behind *parse*. Not a recurrence: the other ten `builder.Services` sites are top-level statements | 6 |
| 76 | The `TransformAttribute` listing short of 10.8.1's: `object[] InitializerParams()` returning `new object[0]`, where the source has `object?[]` and `[]` (`TransformAttributeBase.cs:25`), and the class left unclosed | `MessageTransforms.md` | `grep -rnF 'public virtual object[] InitializerParams' contents/` | **1** | **0** | task 6.3, hidden behind *parse* | 6 |
| 77 | Transforms said to run in ascending `step` order on send and in reverse on receive; at 10.8.1 both pipelines run the highest step first (`TransformPipelineBuilder.cs`, `FindWrapTransforms` and `FindUnwrapTransforms`, `OrderByDescending`). So #8's `Compress` 0 and `ClaimCheck` 1 claim-checked before compressing, its *"First: Compress"* false, and § *Step*'s *"lower"* backwards. Recurs on `HandlingLargeMessages.md`: the claim check *"unwraps at `step: 0` by convention because it has to run before anything that reads the body"* | `MessageTransforms.md`; `HandlingLargeMessages.md` | `grep -rnF 'On receive, they execute in reverse order' contents/`; `grep -rnF 'step value for compression was lower' contents/`; `grep -rnE '\[Compress\(step: 0, compressionMethod: CompressionMethod.GZip\)\]  // First' contents/`; `grep -rnF 'The claim check unwraps at `step: 0` by convention because it has to run before' contents/` | **1**; **1**; **1**; **1** | **0**; **0**; **0**; **0** | task 6.4, run (`probe/s4/transform-order`). The step numbers were 6.3's (ledger 72); the sentences and § *Step* predate it. The recurrence found by task 6.5's widened grep and repaired there (obligation 21) | 6 |
| 78 | Brighter said to ship only the Claim Check and Compression transforms; `Paramore.Brighter` 10.8.1 also ships `CloudEvents`/`ReadFromCloudEvents`, and `Paramore.Brighter.Transformers.JustSaying` and `…MassTransit` ship a pair each | `DefaultMessageMappers.md` | `grep -rnF 'Brighter ships the Claim Check and Compression transforms;' contents/` | **1** | **0** | task 6.4, run (`probe/s4/shipped-transforms`); written at `254e8a7` | 6 |
| 79 | `CooperativeSticky` said to be unsupported with Brighter's manual commits and to throw `ArgumentOutOfRangeException`; at 10.8.1 (Confluent.Kafka 2.16.0) it round-trips and rebalances without an exception, as `RoundRobin` does | `KafkaConfiguration.md` | `grep -rnE 'CooperativeSticky.*([Nn]ot supported\|throw)' contents/` | **3** | **0** | task 6.4, run against `apache/kafka:4.0.2` (`probe/s4/kafka-consumer`) | 6 |
| 80 | `retryWaitInMilliseconds` said to be the wait between connection retries; Brighter waits `retryWaitInMilliseconds × 2^attempt` (`ConnectionPolicyFactory.cs:66` in RMQ.Async, `ConnectionPoolFactory.cs:69` in RMQ.Sync at 10.8.1), so 500, 1000, 2000 … ms for 250 | `RabbitMQConnectionStability.md`, `RabbitMQConfiguration.md` | `grep -rnE 'Wait between retries\|Time to wait between retry attempts\|retry interval \(defaults to 1000ms\)' contents/` | **3** | **0** | task 6.4, run (`probe/s4/rmq-connection`); the comment in a block 6.2 edited, the two descriptions beside it (obligation 21) | 6 |
| 81 | FIFO topic and queue names said to need a `.fifo` suffix; Brighter appends it when the name lacks it (`AWSNameExtensions.cs`, `Truncate`, at 10.8.1) | `AWSSQSConfiguration.md` | `grep -rnF 'must end with `.fifo`' contents/` | **2** | **0** | task 6.4, run on LocalStack 4.4 (`probe/s4/sqs-channels`) | 6 |
| 82 | RabbitMQ's blocked and unblocked events said to be logged *"automatically"* once a provider is added, and the DI extensions said to leave *"nothing to do"*; each Brighter type logs through a `static readonly` logger from `ApplicationLogging.LoggerFactory` at its first log, which is the container's factory only once the command processor (`ServiceCollectionExtensions.cs:776`) or the Dispatcher (`:141`, ServiceActivator) is built, so a type that logs earlier never reaches the providers | `RabbitMQConnectionStability.md`; `Logging.md` | `grep -rnF 'Blocked events will be logged automatically' contents/`; `grep -rnF 'Brighter automatically logs blocked and unblocked events' contents/`; `grep -rnF 'If you use the DI extensions, there is nothing to do' contents/` | **1**; **1**; **1** | **0**; **0**; **0** | task 6.5's Serilog addendum, run against `rabbitmq:3.13-management` (`probe/s4/rmq-logging`); the `Logging.md` recurrence by `grep -rn 'ApplicationLogging\|ILoggerFactory' contents/` (obligation 21) | 6 |
| 83 | The blocked and unblocked sample lines showing `amqp://localhost:5672`; the gateway logs the sanitised URI, credentials masked and a trailing slash (`RmqMessagingGatewayConnection.cs:137`) | `RabbitMQConnectionStability.md` | `grep -rnF 'Subscription to amqp://localhost:5672' contents/` | **2** | **0** | task 6.5's Serilog addendum, the probe's output (`probe/s4/rmq-logging`) | 6 |
| 84 | `[UseResiliencePipeline(1, "…")]` with the step first, where `UseResiliencePipelineAttribute(string policy, int step)` takes the policy first (`UseResiliencePipelineAttribute.cs:68`), and on a handler class, where `[AttributeUsage(AttributeTargets.Method)]` (`:52`) allows only a method; the handlers are `RequestHandlerAsync`, so the attribute is `[UseResiliencePipelineAsync("…", 1)]` on `HandleAsync`, with `[RequestLoggingAsync]` moved beside it | `FAQ.md` | `grep -rnE '\[UseResiliencePipeline(Async)?\([0-9]' contents/`; `grep -rn -A2 -E '^\[(RequestLogging\|UseResiliencePipeline\|UsePolicy\|UseInbox\|FallbackPolicy\|FeatureSwitch\|Monitor\|ValidateRequest\|DeferMessageOnError\|RejectMessageOnError\|DontAckOnError\|Timeout)(Async)?\(' contents/ \| grep -E '^\S+-[0-9]+-\s*public (abstract \|sealed )?class '` | **3** (+1, `PipelineValidation.md`'s `text` log line, not a call); **3** | **0** (+ the same log line); **0** | task 7.1, the second compile (`CS1503`, `CS0592`); ruled 2026-10-31, repaired at 7.3. Widened at 7.6: the second grep's list named `Timeout(`, not `TimeoutPolicy(`; with `TimeoutPolicy` added it read **1** at `8c90dcc`, `FAQ.md` #15 (*Old (V9)*), and **0** after, repaired with ledger 95 | 7 |
| 85 | `MyHandlerBase<T> : RequestHandlerAsync<T>` without the `where T : class, IRequest` the base requires (`RequestHandlerAsync.cs:54`) | `FAQ.md` | `grep -rn -A1 -E 'class \w+<T>\s*:\s*RequestHandler(Async)?<T>\s*$' contents/`, read for a `where` on the next line | **1** unconstrained | **0** (1 hit, constrained) | task 7.1, the second compile (`CS0452`); ruled 2026-10-31, repaired at 7.3 | 7 |
| 86 | The `JsonSerialisationOptions` static-constructor listing showing 4 of the 12 converters 10.8.1 adds (`JsonSerialisationOptions.cs:33–44`), and `ConfigureJsonSerialisation` said to take an `Action<JsonSerialisationOptions>` (the class is static; it takes `Action<JsonSerializerOptions>`, `ServiceCollectionExtensions.cs:866–867`). Run: `probe/s5/json-options` reads 12 converters in the listed order, and the call keeps all 12 on a copy | `CommandProcessorConfigurationReference.md` | `grep -rnF 'Action\<JsonSerialisationOptions\>' contents/`; `grep -c SubscriptionNameConverter contents/CommandProcessorConfigurationReference.md` | **1**; **0** | **0**; **1** | task 7.1, the second compile (`CS1520`, `CS0118`); ruled 2026-10-31, repaired at 7.3 | 7 |
| 87 | `services.AddProducers(o => o.UseRabbitMQ(…).Publication<T>(…))` and `.Subscription<T>(…)`: `AddProducers` extends `IBrighterBuilder`, not `IServiceCollection`, and `UseRabbitMQ` exists nowhere in `src` at 10.8.1 or `origin/master`. Rewritten to `services.AddConsumers(…)` with `RmqSubscription<T>`, then `.AddProducers(…)` with `RmqProducerRegistryFactory` and `RmqPublication<T>` | `FAQ.md` | `grep -rnE 'UseRabbitMQ\(' contents/`; `grep -rnE '[Ss]ervices\.AddProducers\(' contents/` | **3** lines in 2 blocks; **2** | **0**; **0** | task 7.1, hidden behind *parse*; ruled 2026-10-31, repaired at 7.3 | 7 |
| 88 | `channelCount: 3`, a parameter no `Subscription` has; the count of message pumps is `noOfPerformers` (`Subscription.cs`, 10.8.1) | `FAQ.md` | `grep -rn 'channelCount' contents/` | **1** | **0** | task 7.3, hidden behind *parse* (#20); confirmed by the maintainer 2026-11-01 | 7 |
| 89 | `ExternalBusConfiguration`, a type that exists nowhere in `src` at 10.8.1; the `AddProducers` delegate takes a `ProducersConfiguration` (`ServiceCollectionExtensions.cs:312`) | `CommandProcessorConfigurationReference.md` | `grep -rn 'ExternalBusConfiguration' contents/` | **2** | **0** | task 7.3, reading the prose beside #15 and #17; confirmed by the maintainer 2026-11-01 | 7 |
| 90 | `MySqlInboxBuilder.GetExistsQuery(INBOX_TABLE_NAME)` and `MySqlOutboxBuilder.GetExistsQuery(OUTBOX_TABLE_NAME)`, each one argument where both take `(tableSchema, tableName)`; and `ExecuteScalar() != null` as the exists test, where the query is `SELECT EXISTS (…) AS TableExists`, which returns a row either way, so the test is always true and the table is never created. Run: `probe/s5/mysql-exists`, against MySQL 8 and the released 10.8.1 packages. Case, table absent: `ExecuteScalar()` is `0` (Int64) and `Convert.ToInt32(…) == 1` is false; control, table present: `1` and true; the old test says true in both | `BrighterInboxSupport.md`; `BrighterOutboxSupport.md` | `grep -rnE 'ExistsQuery\((INBOX\|OUTBOX)_TABLE_NAME\)' contents/`; `grep -rnF 'ExecuteScalar() != null' contents/` | **2**; **2** | **0**; **0** | task 7.3, hidden behind *parse* (Inbox #2) and *defect* (Outbox #2, `CS1106`: an extension method outside a static class); run; confirmed by the maintainer 2026-11-01 | 7 |
| 91 | `Environment.IsDevelopment()` in a `Startup` method, where `Environment` binds to `System.Environment`; the host's is `IWebHostEnvironment`, which a `Startup` takes by constructor | `CommandProcessorConfigurationReference.md` | `grep -rnE '(^\|[^.A-Za-z_])Environment\.IsDevelopment\(\)' contents/` | **1** | **0** | task 7.1, the second compile (`CS0117`); repaired at 7.3; confirmed by the maintainer 2026-11-01 | 7 |
| 92 | An `IAmAMessageMapper<T>` shown with one of its two members, under a `// ...` that declares nothing about the other (017's recurring defect): `MapToMessage` missing from `OrderEventMessageMapper`, `MapToRequest` from `LargeMessageMapper` | `MigratingToNullableReferenceTypes.md`; `OutboxArchiver.md` | a scan of every C# fence declaring `class … : IAmAMessageMapper(Async)?<` for both `MapTo…` members, run at `HEAD` and on the tree | **2** | **0** | task 7.1, the second compile (`CS0535`); repaired at 7.3; `OutboxArchiver.md`'s mapper was later replaced by an `InMemoryArchiveProvider` example at 7.4 (ruled 2026-11-01) | 7 |
| 93 | `TimeToLive` said to make MongoDB delete documents (*"Messages will be automatically deleted after 30 days"*, *"MongoDB will automatically create a TTL index on the `TimeStamp` field and remove expired documents"*, *"a TTL index expires stale lock documents automatically"*). At 10.8.1 it deletes nothing: Brighter creates its index only when it creates the collection (`MakeCollection = CreateIfNotExists`; the default, `Assume`, makes none, `BaseMongoDb.cs:83–118`), and that index, on `TimeStamp`, matches no date, because the driver stores the `DateTimeOffset` as an embedded `{ DateTime, Ticks, Offset }` document. Run: `probe/s5/mongo-ttl`, MongoDB 7 with the TTL monitor every second. The pages now say so, and show a TTL index on `TimeStamp.DateTime`, which the probe shows removing the message, the Inbox entry and an unreleased lock | `MongoDBOutbox.md`; `MongoDBInbox.md`; `MongoDbDistributedLock.md`; `DistributedLock.md` | `grep -rnE 'automatically deleted after\|Auto-expire messages\|remove expired documents\|expires them through a MongoDB TTL index\|a TTL index expires stale lock\|before the TTL index removes it\|Used for the TTL index' contents/`; `grep -rn 'MongoDb.*\| TTL document \|' contents/` | **10** (Outbox 5, Inbox 3, lock 2); **1** | **0**; **0** | task 7.5, run with a control (`MongoDBInbox.md` #4, `MongoDBOutbox.md` #4); an upstream candidate, not filed. **Filed as Brighter #4716 and #4717 (2026-11-01)**; confirmed by the maintainer 2026-11-01 | 7 |
| 94 | `services.AddResiliencePipeline("MyRetryPolicy", …)` (and `"MyTimeout"`) shown as the registration `[UseResiliencePipelineAsync("…", 1)]` finds. Brighter looks pipelines up only in its own registry, `options.ResiliencePipelineRegistry` or a new one (`BrighterResiliencePipelineRegistry`, 10.8.1), and never reads the one `Polly.Extensions` puts in DI. Run: `probe/s5/faq-claims`, the handler never runs and the request fails `ConfigurationException`, inner `KeyNotFoundException`; the registry given to `AddBrighter` retries (control). #13 and #16 now build that registry | `FAQ.md` | `grep -rnF 'services.AddResiliencePipeline(' contents/` | **2** | **0** | task 7.5, run with a control (`FAQ.md` #13, #16) | 7 |
| 95 | `[TimeoutPolicy]` shown on an async handler as the V9 form: on `HandleAsync` (`V10MigrationGuide.md` #8, *Before (V9)*), and on a `RequestHandlerAsync` class (`FAQ.md` #15, *Old (V9)*, ledger 84's shape). The attribute has no async form, and its handler is synchronous (`TimeoutPolicyHandler<T> : RequestHandler<T>` at 9.33 and 10.8.1), so building an async pipeline with it throws `ConfigurationException` (*"All handlers in an async pipeline must derive from IHandleRequestsAsync"*, `PipelineBuilder.cs:294`, `:350` at 9.33), and on a class it is `CS0592`. Run: `probe/s5/timeout-policy-kind`, against 10.8.1, where the attribute is `[Obsolete]` and unchanged in kind. Both blocks are now a synchronous `Handle`, and the sentence before each says why. Rule 8 cannot see it: `TimeoutPolicy` has no `…Async` twin, so it is not in `PAIRED` | `V10MigrationGuide.md`; `FAQ.md` | `grep -rn -A1 -E '\[TimeoutPolicy\(' contents/ \| grep -E 'HandleAsync\|class '` | **2** | **0** | task 7.6, ledger 84's grep widened by shape, then the attribute-kind scan over every attribute above a `Handle`/`HandleAsync`; run with a control | 7 |
| 96 | `Paramore.Brighter.MessagingGateway.RMQ` named as the V10 RabbitMQ package (`dotnet add package … --version 10.0.0`; *"provided by the NuGet package"*). NuGet's index for that id stops at `10.0.0-preview.4`; V10 ships `Paramore.Brighter.MessagingGateway.RMQ.Async` and `….RMQ.Sync` (10.0.0 to 10.8.1; `src/` at 10.0.0 and 10.8.1 has only those two), and both pages' blocks import `….RMQ.Async` | `V10MigrationGuide.md`; `CommandProcessorConfigurationReference.md` | `grep -rnE 'MessagingGateway\.RMQ(\*\*\| \|$)' contents/` | **2** | **0** | task 7.6, 7.1's unpinned-package shape widened to every `Paramore.Brighter…` id against NuGet | 7 |

## Friction ledger

*Numbered from 81, continuing 017's 67–80. Written at task 8.5.*

## What the tasks review found — 2026-10-05

| # | Found | Now |
|---:|---|---|
| 1 | Task 1.6's grep for stale rule claims, `'rule 7\|seven rules\|rules 1'`, matches **0** lines in `.claude/commands/`, so it could not find what it was for | `grep -rn 'pagelint' .claude/commands/` → 2 files, each hit read |
| 2 | AC13's join had no home: no task wrote the command | 8.2 writes it above § *Blocks that stay FAILED* and runs it |
| 3 | Task 4.3 named *"15 handler blocks"*, a figure nothing produced | it names the blocks § *Phase 2 as executed* lists |

Checked and holding: 61 tasks by `grep -c` (8 / 15 / 7 / 6 / 6 / 6 / 7 / 6); every new check has a
red-proof task with a two-way control; the expected outputs that can be read today hold (rule 8's one
hit is `PipelineValidation.md` block 7; `grep -c Shouldly contents/TestDoubleOptions.md` → 0;
E6's 23 / 8; `pagelint` 524).
