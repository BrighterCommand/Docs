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
They replace design § *The section groups*' working figures, which came from the probe.*

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
| Understanding Brighter | `HowConfiguringTheCommandProcessorWorks.md` | 9 | 1 |  |  |  |  | 8 | 1 |
| Understanding Brighter | `HowServiceActivatorWorks.md` | 17 | 4 |  | 3 |  | 1 | 9 | 0 |
| Understanding Brighter | `ReactorAndProactor.md` | 13 |  |  |  |  |  | 13 | 0 |
| **S3** | **10 pages** | **137** | **31** | **7** | **15** | **5** | **7** | **72** | **8** |

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
| V10 Migration | `V10MigrationGuide.md` | 19 | 3 | 2 | 1 | 3 | 1 | 9 | 2 |
| Outbox and Inbox | `BrighterInboxSupport.md` | 2 | 1 |  |  |  |  | 1 | 0 |
| Outbox and Inbox | `BrighterOutboxSupport.md` | 2 |  |  | 2 |  |  |  | 0 |
| Outbox and Inbox | `CausationTrackingStores.md` | 2 |  |  | 2 |  |  |  | 0 |
| Outbox and Inbox | `DistributedLock.md` (V4) | 1 |  |  |  |  | 1 |  | 1 |
| Outbox and Inbox | `DynamoDbDistributedLock.md` (V4) | 2 |  |  |  |  | 2 |  | 0 |
| Outbox and Inbox | `EFCoreOutbox.md` | 2 |  |  |  |  |  | 2 | 0 |
| Outbox and Inbox | `MongoDBInbox.md` | 3 | 1 | 2 |  |  |  |  | 1 |
| Outbox and Inbox | `MongoDBOutbox.md` | 2 | 1 |  | 1 |  |  |  | 3 |
| Outbox and Inbox | `OutboxArchiver.md` | 4 | 2 |  | 1 |  | 1 |  | 0 |
| Outbox and Inbox | `SweeperCircuitBreaking.md` | 3 | 1 | 1 | 1 |  |  |  | 6 |
| Outbox and Inbox | `TurningOnReplayOnSeen.md` | 6 |  |  | 5 |  | 1 |  | 0 |
| Reference | `FAQ.md` | 17 | 4 |  | 5 |  | 1 | 7 | 2 |
| Health Checks and Observability | `ConfiguringOpenTelemetry.md` | 6 | 3 |  | 3 |  |  |  | 0 |
| Health Checks and Observability | `Logging.md` | 3 |  |  | 2 |  |  | 1 | 2 |
| **S5** | **22 pages** | **136** | **30** | **12** | **39** | **3** | **9** | **43** | **40** |

S5: 22 pages, 136 FAILED, **11 with nothing BUILT**.

**Totals: 67 pages, 634 FAILED**, of which 140 are parse, 36 built-by-using, 93 defect,
14 same-page, 75 values and 276 page-type. **37** pages have nothing BUILT.

**Two methods agree.**
- `groups.py`'s FAILED column gives **634**, the same as `--report`'s FAILED on those 67 pages.
- `awk` over `--report` and the tranche list alone gives **630** FAILED on **64** off-tranche pages.
  Adding the **4** V4 blocks (`S3LuggageStore.md` #1, `DistributedLock.md` #2,
  `DynamoDbDistributedLock.md` #1, #2) gives 634.
- The other **36** FAILED blocks on tranche pages are not in any group.

**Against the design's working figures:**
- **S1–S3 are identical** in pages and FAILED: 8 / 129, 10 / 107 and 10 / 137.
- **S4 is 16 / 124 → 17 / 125, and S5 is 20 / 133 → 22 / 136.** The only difference is the three
  (V4) tranche pages, which the task puts in S4 and S5, adding 1 and 3 FAILED blocks.
- **Nothing BUILT is 36 → 37**, again only `DynamoDbDistributedLock.md`. That is the design's 36
  off-tranche pages, and the 39 in § *The targets* is still 36 + 3.
- Phase 2's four new BUILT blocks are all on tranche pages, which is why no off-tranche figure moved.
- The design's *Reachable* and *Hard* columns came from the probe. They are replaced by the six
  classes, and nothing here is carried from them.

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
