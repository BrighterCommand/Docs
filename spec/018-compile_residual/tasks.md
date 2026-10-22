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
- [ ] **Task 6.3:** Repair S4's hard blocks
  - Input: S4's table, hard rows; Brighter `10.8.0` source, the pinned release
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
  - Input: S5's table, hard rows; Brighter `10.8.0` source, the pinned release
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
| `AsyncAPISupport.md` | 9 | `CS0246` `MySchemaGenerator` | *same-page*: block 8 declares it, and the sentence before block 9 says to register your implementation | 6 |
| `AsyncAPISupport.md` | 11, 12 | `CS0246` `OrderCreatedEvent` | *same-page*: block 5 declares it, as the sentence under *Complete Examples* now says | 6 |
| `Routing.md` | 3 | `CS0246` `MyRequest` | *same-page*: block 2 declares it, and the sentence before block 3 names "this request" | 6 |
| `DefaultMessageMappers.md` | 6, 10 | `CS0246` `Orders`, `OrderShipped`, `AvroMessageMapperAsync<>` | *same-page*: block 4 declares the mapper and block 5 `OrderShipped`, in the namespace `Orders`; the sentences before blocks 6 and 10 are about that mapper | 6 |
| `RabbitMQConnectionStability.md` | 3 | `CS0103` `Log`, `services`; `CS0246` `LoggerConfiguration` | Serilog (and its Seq sink) is on no pin, and the page names it alone. Adding it moves the pin's 553 assemblies and is not a repair of this page: put to 6.5 | 6 |

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
| 2 | Shouldly assertions with the package never named and no `using` | `TestDoubleOptions.md` | `grep -c Shouldly contents/TestDoubleOptions.md` | **0** | | the second compile (design E2, *pin-gap*) | 7 |
| 3 | Package spelt `Paramore.Brighter.Inbox.DynamoDb.V4`; the project is `…DynamoDB.V4` | `DynamoInbox.md` (a 017 tranche page; repaired under obligation 21) | `grep -rn 'Inbox.DynamoDb.V4' contents/` | **1** | | re-derivation (requirements, the V4 pin) | 7 |
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
| 69 | `new KafkaMessagingGatewayConfiguration { /* ... */ }`, leaving `BootStrapServers`, which is `required`, to a comment (`CS9035`) | `RoutingMultipleMessageTypes.md` | `grep -rnF 'KafkaMessagingGatewayConfiguration { /* ... */ }' contents/` | **1** | **0** | task 6.2, hidden behind *page-type* | 6 |

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
