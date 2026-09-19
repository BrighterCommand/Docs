# Spec 015: Census Triage — Tasks

**Created:** 2026-09-18 · **Status:** Reviewed 2026-09-18 — **three findings, all applied; the
requirements amended.** Awaiting `.tasks-approved`.

**Tasks review, 2026-09-18.** Three findings against this list, each applied where it was found:
**task 1.1's prediction was measured at the wrong SHA pair** (929 → 819 comes from a run at
`6145913a0`, not the pin — obligation 9 biting the list that states it), **phase 4's red run had no
stated other half** (4.3's red and 4.9's green are one two-way control and nothing said so), and
**task 3.6 quoted 110 rather than re-deriving it**, which AC8 forbids in the same breath it asks for
the figure. **The total did not move: 41 before and after**, because all three were repairs to
existing tasks rather than missing ones.

**And one finding against `requirements.md`, amended at the maintainer's instruction:** **AC11 and
P0-6 still named the ≥3-page slice Q2's reversal abolished**, along with target state 2–3 and the
note under AC5. P0-6's was not a wording defect — that slice contains **no Brighter API**, so a P0-6
bounded by it is a P0-6 bounded to nothing, satisfiable by repairing nothing while the twelve dead
APIs stayed on `ShowMeTheCode.md`. **An amendment that lands in some places and not others leaves
the unamended ones looking deliberate.**

**Works from:** `design.md` (approved 2026-09-18, `.design-approved`; Q2 reversed at that review,
three probes run, all three changed it) and `requirements.md` (approved 2026-09-18,
`.requirements-approved`; six questions ruled, three overturning the recommendation, AC2 and AC3
rewritten at the review because neither could fail).

**Total tasks: 41, across 5 phases and 5 pull requests.** Re-derived rather than counted by hand,
and the phase table below sums to 41 independently:

```bash
grep -c '^- \[.\] \*\*Task' spec/015-census_triage/tasks.md     # 41
```

**Keep both and re-derive both after any edit.** 009's D-table spent three sessions wrong because a
count was edited beside the row it counted, and 012's two counts disagreed by one for exactly as
long as it took to notice that the review had added a task and not the row above it.

| Phase / PR | Goal | Tasks | Deliverables | Published site |
|---:|---|---:|---|---|
| **1** | **The instrument** — P0-2's pin and SHA header, then P0-1's member filter, in that order | 11 | D1 | untouched |
| **2** | **The triage method and its runner** — P0-3 written, with the `\b` trap red-proofed | 6 | D3 (method), D5 | untouched |
| **3** | **The execution** — P0-4 over all 819, and AC8's boundary | 8 | D3 (record) | untouched |
| **4** | **The rows and the repairs** — P0-5 and P0-6, **one PR, rows first on the branch** | 10 | D0, D2, D4 | **CHANGED — sign-off** |
| **5** | **Acceptance** — the walk, the backwards check, both ledgers, the close | 6 | D6 | untouched |

---

## 1. How this list is organised

**One phase is one pull request**, merged before the next branch starts. `tools/README.md`
§ *One phase is one pull request* carries the contract and the five things it obliges; it is cited,
not restated, and its gate figures are cited from it rather than pasted here.

**The default Research → Core → Supporting → Polish shape does not fit, and the reason is worth
naming rather than silently departing from.** The research is already done — the three probes
shipped in #167 and the design ran all three — so a research phase would have nothing in it. What
remains is one instrument, one method, one execution of that method, and one repair, and each of
those is finishable on its own day. **The phases are the design's deliverables in dependency
order**, and the last one is acceptance, which is always true.

**This spec writes no documentation page.** `design.md` §12 records why the outline, `SUMMARY.md`,
nesting, redirects and glossary sections are all N/A: 015 creates no page. Phase 4 *edits* nine
that already exist and is forbidden from changing their type, banner or opening sentence.

### The standing obligations — every task owes all ten

Do not restate them per task. Restating invites the reader to treat the unrestated ones as
optional — and **1 to 7 are the programme's, unchanged; 8 to 10 are 015's own**, added because this
spec's subject is an instrument whose output is a claim about another repository.

1. **Re-derive any count before quoting it**, with the command beside the figure and **two methods
   that agree** — **and re-derive the number the decision turns on, not the number the document
   leads with** (friction 43). The requirements carried 112 in good faith after re-deriving 819 by
   two agreeing methods, because both methods agreed on the number that did not discriminate.
2. **Record the mismatch before fixing it.** The corrected state is the only thing left afterwards,
   so a spec that does not write down what was wrong cannot show the corpus was ever wrong. Phase
   4's red `symbolcheck` run is the whole evidential product of the repair.
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof
   with its output recorded, and **every control is two-way** — the positive case goes **outside**
   the enumeration (friction 36) **and it must be able to pass at all** (friction 44).
4. **Prose and permission ship together**, read in both directions.
5. **Cite `CLAUDE.md` and `tools/README.md`; never restate them** — and when a phase edits
   `CLAUDE.md`, grep the commands for the claim it changed (friction 37). No phase here expects to.
6. **Predict gate movement before the work, including "none"** — and say *why* none, then
   reconcile. `git add` before any `--changed` run, or the strict pass sees nothing.
7. **Ask before merging anything that changes the published site, and ask for the head-ref deletion
   by name in the same breath.** A merge is not a deletion. **Phase 4 only.**
8. **Every verdict carries evidence and a control** (AC6). Clearing a name because it "looks like an
   example" is the reasoning that left `ConfigureBrighter` unexamined for a year, and it is the
   criterion the requirements name as the one that fails quietly.
9. **Every figure about the world is measured at the pinned SHA pair**, and the run that produced it
   prints those SHAs in its header. A before-figure and an after-figure taken at different SHAs are
   two measurements of two worlds — which is what made AC2 unmeasurable until Q6.
10. **A gate number changes in `tools/README.md` and nowhere else** (constraint 6).

### Dependencies

- **P0-2 gates P0-1, inside phase 1.** AC2 requires the before- and after-counts at the same
  resolved SHA pair and AC3's header is what proves it, so the pin and the header must exist
  *before* the before-run. **This inverts the requirements' numbering** and it is deliberate: the
  requirements number the items, they do not order them.
- **1 gates 2.** The triage runs at the census's pinned SHAs (finding 2), which do not exist as a
  constant until phase 1 ships.
- **1 gates 3.** The corpus P0-4 triages is the census P0-1 leaves.
- **2 gates 3.** A method executed before it is written down is a method nobody can check.
- **3 gates 4.** Which names get rows and which pages get repaired is exactly what P0-4 decides.
  **The twelve in `design.md` §5.1 are a floor, not the answer** — 87 of the 99 survivors are unread.
- **4 is one PR, not two, and phase 3 must not pre-empt it.** See finding 4.
- **Phases 2, 3 and 4 are individually abandonable; 1 is not**, because P0-1 and P0-2 are the only
  changes to a shipped tool and the rest of the spec quotes their output.

---

## 2. Counts re-derived at the top of this list

Per standing obligation 1. **Measured 2026-09-18 against Docs `master` `1524662`.** Three inherited
figures held, one moved, and one ref moved underneath all of them.

| Inherited from `design.md` | Command | Result |
|---|---|---|
| census reports **929** candidates | `symbolcheck.py --census \| tail -1`, and `methodprobe.py \| head -1` | **929**, two methods |
| **110** removable, **819** after the filter | `methodprobe.py \| head -4` and `perpageprobe.py \| head -4` | **110 / 819**, two methods |
| per-page bands **36 / 57 / 65 / 103 / 210** | `perpageprobe.py \| head -4` | unmoved, all five |
| the blind spot is **one name wide** | `perpageprobe.py \| tail -3` | **1** — `Greeting`, at 7 pages |
| the twelve touch **"at least six pages"** | two greps, below | **9 pages, 18 sites** — finding 3 |
| `../Brighter` `origin/master` is **09f5d988f** | `git -C ../Brighter rev-parse --short origin/master` | **`6145913a0`** — finding 1 |
| `10.7.0` · `../Darker` `origin/master` · `4.1.1` | `git rev-parse --short`, each | **`c1b8af886` · `2f76cda` · `ddb71ee`** — all three unmoved |

**The drift control comes first, before any figure above is trusted.** `perpageprobe.py` copies
`census()`, and a copy drifts; run with no extra filter it must reproduce the shipped tool exactly
(`design.md` §10.2 — 2019 = 2019, and its two code paths agree at 1749). Re-run it at the top of
phase 1 rather than inheriting the design's run.

### Finding 1 — `origin/master` moved between design approval and this list, and Q6's pin earned itself two days after the ruling

```bash
git -C ../Brighter rev-parse --short 09f5d988f          # 09f5d988f   the design's pin, still resolves
git -C ../Brighter log -1 --format=%cI 09f5d988f        # 2026-09-16T19:42:56+02:00
git -C ../Brighter log -1 --format=%cI origin/master    # 2026-09-18T15:17:20+02:00
git -C ../Brighter log --oneline 09f5d988f..origin/master -- 'src/*.cs' | wc -l   # 1
```

**One commit touching `src/*.cs` — a one-line change in `CommandProcessor.cs`.** The census reports
**929 at both SHAs**, and per friction 43 that is not evidence that nothing changed; it is one
number that happens to survive. **The pin ships as the design recorded it — `09f5d988f` — and
`2f76cda` for Darker**, because the value a design was approved on is the value that reproduces its
figures, and because constraint 10 makes refreshing a pin a deliberate commit with the new count
beside it rather than a drive-by in a phase about something else.

### Finding 2 — the triage's own queries must run at the pin, and the design's commands say `origin/master`

`design.md` §4.3's stage-1 and stage-2 queries, and §4.2's live control, all name `origin/master`.
They were written when `origin/master` and the pin were the same SHA. **They are not the same SHA
now**, so as written the census would nominate a name from one world and the verdict would describe
another.

**Decision, taken here and not in the design: P0-4's history and live queries run at
`CENSUS_PINS`'s SHAs.** It costs nothing — a SHA is a valid revision argument everywhere
`origin/master` was — and it makes AC5's row count and each row's evidence statements about one
world. Recorded as a decision this list takes that the design did not, rather than applied quietly.

### Finding 3 — the twelve names touch nine pages and eighteen sites, not six

```bash
for n in UseMsSqlOutbox CommandProcessorLifetime IUnitOfWork UseInMemoryOutbox NoTaskQueues \
         UseScoped UseMySqlOutbox UseDynamoDbOutbox AddS3LuggageStore BeginOrGetTransaction \
         UseDynamoDbTransactionConnectionProvider UseMySqTransactionConnectionProvider; do
  grep -rlw "$n" contents/; done | sort -u | wc -l          # 9   method 1, per name
grep -rlE '\b(UseMsSqlOutbox|CommandProcessorLifetime|IUnitOfWork|UseInMemoryOutbox|NoTaskQueues|UseScoped|UseMySqlOutbox|UseDynamoDbOutbox|AddS3LuggageStore|BeginOrGetTransaction|UseDynamoDbTransactionConnectionProvider|UseMySqTransactionConnectionProvider)\b' \
  contents/ | wc -l                                          # 9   method 2, one alternation
```

**Nine pages, eighteen sites: fourteen inside C# fences across ten distinct blocks, four in prose.**

| Page | Fenced sites | Prose sites |
|---|---:|---:|
| `BrighterBasicConfiguration.md` · `DispatchingARequest.md` · `FeatureSwitches.md` · `ShowMeTheCode.md` · `UsingSweeperCircuitBreaking.md` · `DapperOutbox.md` | 1 block each | 0 |
| `DynamoOutbox.md` | 2 blocks | 0 |
| `SweeperCircuitBreaking.md` | 1 block | **3** — a bulleted list of provider methods |
| `S3LuggageStore.md` | 1 block | **1** |

The design's *"at least six pages"* was drawn from the five names whose pages §5.1 listed, and it
said it was a lower bound. **It is a lower bound in two directions**: nine pages for the twelve, and
the twelve are themselves a floor because 87 of the 99 stage-1 survivors have not been read.

**The four prose sites are not automatically repairs.** Ruling 6 — *a removed name may stay in prose
behind a visible opt-out* — and `SweeperCircuitBreaking.md`'s three are a list of provider methods
in running text, which is precisely the case that ruling was written for.

### Finding 4 — P0-5 and P0-6 cannot be two pull requests in that order

A `symbolwatch.tsv` row makes `symbolcheck` fire on every site of its symbol. **Add the twelve rows
while the names are still printed on nine pages and the gate goes red — on the branch, and then on
`master`, because `.github/workflows/docs.yml` triggers `on: push`.** That is ruling 8: *a gate and
the corpus that satisfies it merge together, or the gate merges second.*

**And the red is the red-proof, so the order inside the branch is the opposite of the order between
branches.** Repair first and the rows can never be shown to fire — a green from an instrument that
has never had anything to find, which is the trap this programme has now met eight times. So:
**rows, then the red run recorded, then the repairs, then the green run — one pull request.**

---

## Phase 1 — the instrument: the pin, the header, then the filter *(11 tasks, one PR)*

**Branch:** `spec/015-phase1-instrument`. **Goal:** `--census` prints four SHAs it actually resolved
and stops counting a page's own helper methods. **Changes no published page**, so no sign-off — but
it still owes obligations 1 and 2.

- [x] **Task 1.1:** Re-run the drift control, and write the movement prediction **dated and before
      any edit to `tools/symbolcheck.py`**
  - Input: `design.md` §10.2 (the drift control), §2's table, §11's gate predictions
  - Output: a § *Phase 1 prediction* section in this file, committed **ahead of** the commit that
    changes `symbolcheck.py`, carrying: the drift-control output, the predicted candidate count
    after P0-1, and a predicted figure for each of the eight gates including every "none" with its
    reason
  - Notes: AC2 requires the prediction to pre-date the change, and a prediction committed in the
    same commit as the change is a prediction nobody can date. `design.md` §11 names two "none"s
    worth distrusting — `linkcheck`'s, because `tools/` is inside its walk, and `pagelint`'s
    repo-wide warning count. **The predicted 929 → 819 was itself measured at `6145913a0`, not at
    the pin** (finding 1), so it is a prediction about a world one commit away from the one task 1.5
    measures. Say so in the prediction. If the before-run at the pin is not 929, **that is a finding
    about the census's sensitivity to the ref, not a failed prediction** — record it and carry the
    pinned figure forward, because obligation 9 makes the pinned pair the one that counts

- [x] **Task 1.2:** Add `CENSUS_PINS` and `resolve_sha` to `tools/symbolcheck.py`, census-scoped
  - Input: `design.md` §3.1 (the constant, the function and the comment that explains the scoping),
    `tools/symbolcheck.py:103-106` (`PRODUCT_REFS`, which must not change), `:484-488` (`CensusError`
    → exit 2)
  - Output: `CENSUS_PINS` in `tools/symbolcheck.py` with `brighter: 09f5d988f` and
    `darker: 2f76cda`, a `resolve_sha` that raises `CensusError`, and a `--census` header printing
    **four** rows each carrying a SHA
  - Notes: the comment in §3.1 explaining *why the pin is census-scoped* ships with the constant —
    it is the only place the reasoning survives, and a later reader deleting the scoping would
    freeze the one gate whose purpose is noticing the world move. The tags carry **no `v` prefix**:
    `v10.7.0` is `fatal: Needed a single revision` in both repositories

- [x] **Task 1.3:** Red-proof `resolve_sha` both ways
  - Input: `design.md` §3.2's recorded run
  - Output: both outputs pasted into § *Phase 1 as executed* — four refs resolving to the four SHAs
    in §2's table above, and an unresolvable pin producing `CensusError` with **exit code 2** shown
    by `echo $?`
  - Notes: the negative case is the one that matters. A pin that silently falls back to
    `origin/master` produces a census whose header says one thing and whose numbers mean another;
    `tools/README.md`'s exit-code contract already makes exit 2 *nothing was checked*

- [x] **Task 1.4:** Prove the gate and `--verify-list` still follow `origin/master`, not the pin
  - Input: `tools/symbolcheck.py:623` (`verify_row` reads `PRODUCT_REFS` directly),
    `design.md` §3.3
  - Output: in § *Phase 1 as executed*, the output of `python3 tools/symbolcheck.py --verify-list`
    naming `origin/master` rather than a SHA, beside a plain `python3 tools/symbolcheck.py` run at
    `tools/README.md`'s figures
  - Notes: this is AC6 — the criterion that the pin did not leak into the gate. Finding 1 makes it
    checkable rather than rhetorical: the two refs now resolve to different commits, so a gate that
    had silently adopted the pin would say so

- [x] **Task 1.5:** Record the **before** census count, at the pin
  - Input: task 1.2's header
  - Output: in § *Phase 1 as executed*, the full `--census` header and the candidate total, with the
    four SHAs visible in the same paste
  - Notes: obligation 9. This run is one half of AC2 and it is worthless unless the header beside it
    shows the SHAs. Today's total is **929**; re-derive rather than quote it, because it was measured
    at `6145913a0` and this run is at `09f5d988f`

- [ ] **Task 1.6:** Add `MEMBER_DECL_RE` and the per-page member filter
  - Input: `design.md` §2.2 (the regex and the two lines it goes beside), §2.1 (why *per page* and
    not all-or-nothing), `tools/symbolcheck.py:462-473` (`DECL_RE`, applied per page inside the loop)
  - Output: three lines in `census()` — the regex, and `declared.update(MEMBER_DECL_RE.findall(
    strip_noncode(body)))` beside the existing `DECL_RE` line
  - Notes: **keep the `strip_noncode` asymmetry and comment it.** Tidying `DECL_RE` into the
    stripped body in the same commit would move the type filter's output and the member filter's at
    once, and neither movement could then be attributed

- [ ] **Task 1.7:** Record the **after** count at the same SHA pair, and reconcile against 1.1
  - Input: task 1.1's prediction, task 1.5's before-run
  - Output: in § *Phase 1 as executed*, the second `--census` header and total, an explicit
    *predicted N, measured M* line, and an explanation of any difference rather than a quiet
    adoption of the new number
  - Notes: AC2 depends on AC3 — the two headers must show the same four SHAs, and that is checkable
    by eye in the paste. Expected movement is **110 names**, 929 → 819

- [ ] **Task 1.8:** Red-proof the member filter, with a positive case outside the enumeration
  - Input: `design.md` §2.3 (the blind spot, one name wide), §10.4's three controls, friction 36
  - Output: in § *Phase 1 as executed*: `methodprobe.py` reporting **0** removable after the change
    (AC1's red-proof), the three controls from §10.4, and a **two-way planted control** — a
    scratch page carrying a modifier-bearing declaration whose name must disappear from the census,
    and one carrying a **modifier-less** declaration whose name must survive
  - Notes: the modifier-less case is the positive from outside the enumeration, and it is expected
    to **survive** — the filter is anchored on a modifier by design. A control that proves the blind
    spot exists and is one name wide is worth more than one that proves the filter works on cases it
    was built from. Do not commit the scratch pages; paste their output

- [ ] **Task 1.9:** Write P1-1 — how much of finding E's 808-name gap is recoverable
  - Input: `design.md` §8's staged table, `spec/014-documentation_workflow/tasks.md` finding E
  - Output: a § *P1-1: the gap is not members* section in this file carrying the re-derived staged
    table (`perpageprobe.py --stages`) and the sentence that **481 of 808 are unexplained by member
    declarations of any kind**
  - Notes: **do not "correct" 831 or 1,211 in 014's `design.md`.** They are the numbers that design
    was approved on and 015's job is to explain them, not overwrite them

- [ ] **Task 1.10:** Re-run the eight gates and reconcile against the prediction
  - Input: task 1.1's predicted figures, `tools/README.md`'s table
  - Output: an eight-row table in § *Phase 1 as executed*, predicted against measured, citing
    `tools/README.md`'s figures rather than pasting them
  - Notes: `spec/` is in `linkcheck`'s `SKIP_DIRS` (`tools/linkcheck.py:52`) so this file is outside
    its corpus — but `tools/` is inside the walk, which moved `linkcheck` 164 → 165 once before. A
    gate's scope is not the site's scope

- [ ] **Task 1.11:** Write § *Phase 1 as executed*
  - Input: tasks 1.3 to 1.8's recorded output
  - Output: a section in this file carrying the two census headers, the two red-proofs, the controls,
    the reconciliation and any finding the task list did not predict
  - Notes: findings the phase did not expect go here under their own heading, as 014 did — they are
    what the next phase reads

---

## Phase 2 — the triage method, written and runnable *(6 tasks, one PR)*

**Branch:** `spec/015-phase2-method`. **Goal:** AC4 — a written method with a stopping condition,
and a runner that can execute it over 819 names without a person retyping a git command 1,638 times.
**Changes no published page.**

- [ ] **Task 2.1:** Write `triage.md`'s method section
  - Input: `design.md` §4 in full — §4.1 (the discriminator already inside the census), §4.2 (three
    verdicts), §4.3 (the bracket-class form and the `\b` trap), §4.4 (two stages and stage 2.5),
    §4.5 (the stopping condition), §4.6 (the planted positives)
  - Output: `spec/015-census_triage/triage.md`, §§ 1–4: the three-valued vocabulary, the three
    stages with their measured costs, the stopping condition stated as **every name in the census
    has a verdict**, and the statement that **`EXISTED, REMOVED` is provisional** because the
    pickaxe reports a name leaving the source and not whose name it was
  - Notes: AC4 has no instrument and is read by the maintainer, so write it to be read. The
    stopping condition is the clause a method without one becomes a backlog over; the rejected
    heuristic — *stop after N consecutive NEVER EXISTED* — is recorded with its reason, because
    page-spread ordering puts the invented domain first and any run-length rule stops before the
    part of the list most likely to hold a real name (friction 45)

- [ ] **Task 2.2:** Write `probe/triagerun.py`
  - Input: `design.md` §4.3's two queries and §10.5's pilot output; finding 2 above
  - Output: `spec/015-census_triage/probe/triagerun.py`, taking the census's candidate list and
    emitting one row per name: name, page-spread, stage-1 counts per product, stage-2 counts per
    product, live counts, and the stage-2.5 dot evidence
  - Notes: **it queries `CENSUS_PINS`'s SHAs, not `origin/master`** (finding 2). It must be
    resumable or checkpointed — stage 1 alone is 420s and stage 2 about 18 minutes, and a run that
    loses its output to a timeout is a run nobody repeats

- [ ] **Task 2.3:** Red-proof the query form — the `\b` trap, recorded as a broken instrument
  - Input: `design.md` §4.3's four-row table
  - Output: in § *Phase 2 as executed*, all four forms run against `IAmACommandStoreAsync` (which
    **must** stay > 0) and `Date`, with the two `\b`/`\<\>` forms shown returning **0 for the
    positive control**
  - Notes: obligation 2 — record the mismatch before fixing it. This is **plausible zero number
    eight** and the second one this programme has found in a word-boundary flag; the paste is the
    only thing that stops a later reader "simplifying" the bracket class back to `\b`

- [ ] **Task 2.4:** Wire the three two-way controls and the two planted positives into the runner
  - Input: `design.md` §10.4's control block, §4.6 (why the plant stays now the corpus yields)
  - Output: `triagerun.py` asserting, on every run: `IAmACommandStoreAsync` → EXISTED, REMOVED;
    `IAmAMessageScheduler` → LIVE; `OrderId` → NEVER EXISTED; and both planted names classifying
    EXISTED, REMOVED — with the assertions printed, not merely evaluated
  - Notes: friction 44 — **check each control can pass at all** before trusting a red one. The
    plant's justification changed when Q2 was reversed and it still stands: twelve positives
    somewhere in 819 rows does not show that row 400 was classified by a working instrument

- [ ] **Task 2.5:** Re-run the eight gates; predicted **none**
  - Input: `tools/README.md`'s table, `design.md` §11
  - Output: the eight-row predicted-against-measured table in § *Phase 2 as executed*
  - Notes: predicted none because this phase writes only under `spec/`, which `linkcheck` skips and
    no other gate walks. Say that, rather than reporting a pass

- [ ] **Task 2.6:** Write § *Phase 2 as executed*
  - Input: tasks 2.3 and 2.4's output
  - Output: a section in this file with the four query forms, the control block, and the runner's
    measured cost per name per product
  - Notes: record the measured per-name costs against the design's 0.4s and 5.4s — phase 3's budget
    is derived from them and an inherited timing is still an inherited number

---

## Phase 3 — execute the method over all 819 *(8 tasks, one PR)*

**Branch:** `spec/015-phase3-triage`. **Goal:** AC5 — a verdict row for every name in the census,
each with evidence and a control. **Changes no published page.** Roughly 25 minutes of machine time
and an afternoon of stage 3.

- [ ] **Task 3.1:** Run stage 1 over the whole census, at the pin
  - Input: phase 1's `--census` output as the candidate list; `triagerun.py`
  - Output: a checkpoint file under `spec/015-census_triage/` holding one stage-1 row per name, and
    in this file the measured wall clock, the count screened out and the count surviving
  - Notes: the design measured **420s, 720 screened out, 99 survivors** at `09f5d988f`; re-derive.
    A zero at stage 1 is **conclusive** — if the identifier ever existed, its substring existed — and
    that asymmetry is what makes it safe to screen 720 names and never look at them again

- [ ] **Task 3.2:** Run stage 2 over the survivors
  - Input: task 3.1's checkpoint
  - Output: stage-2 counts per product appended to every survivor row, with the run's wall clock
  - Notes: the bracket-class form only. If the survivor count is far from 99, stop and reconcile
    before spending 18 minutes — a large divergence from the design's measurement means the corpus
    or the query changed, and finding 1 says the corpus can move under you

- [ ] **Task 3.3:** Add the stage-2.5 dot evidence
  - Input: `design.md` §10.6 — `ConfigurationManager.GetSection(…)` against
    `private readonly IAmACommandStoreAsync _commandStore;`
  - Output: for every stage-2 survivor, the first historical `+`/`-` line its name appears on, in
    the row
  - Notes: **evidence, never a filter.** A Brighter extension method is called with a dot too, which
    is the collision D12 named and `bclprobe` demonstrated. Presenting it as a filter is how the
    twelve would have been lost

- [ ] **Task 3.4:** Stage 3 — read the survivors and rule each one
  - Input: task 3.3's rows; for each survivor, the diff hunks its name appears in
  - Output: a `LIVE` / `EXISTED, REMOVED` / `NEVER EXISTED` verdict per survivor, each with the
    quoted source line that decided it and the control it was checked against
  - Notes: this is the boundary the whole spec turns on — *the product's own API* against *a name
    the product's source merely contained*. All seven of the ≥3 head's survivors read as **uses**;
    the twelve in the tail read as **declarations**. **87 of the 99 are unread** and the twelve are
    a floor. A `LIVE` verdict is a defect in the instrument, not in a page, and gets recorded as one

- [ ] **Task 3.5:** Write `triage.md`'s record — one row per name
  - Input: tasks 3.1 to 3.4
  - Output: `triage.md` §5, a table with **one row per census name**, each carrying the verdict, the
    two commands that produced it, and its control
  - Notes: AC5 is checked two ways — `grep -c` on the table against the count `--census` reports, and
    the sum of the three verdict classes against the same total. The 720 screened at stage 1 get
    rows too: *screened at stage 1, 0 history* is a verdict with evidence

- [ ] **Task 3.6:** Write AC8 — what 015 did **not** triage, each figure with its command
  - Input: `design.md` §9's table
  - Output: a § *What 015 did not triage* section in this file, carrying the page-declared members
    P0-1 removes — **110 today, re-derived from phase 1's own run at the pin, not from this
    number** — the `NOISE_EXACT`/`NOISE_PREFIX` drops
    (`tools/symbolcheck.py:348-359`), the names that resolve, and the **prose surface** — each with
    the command that produced its number
  - Notes: the last row is the one to write carefully. **Only C# fences have been swept**, and a
    document reporting 819 verdicts invites the reading that the documentation has been. D12
    conclusion 2: 160 of 161 pages carry an unresolved prose token

- [ ] **Task 3.7:** Re-run the eight gates; predicted **none**
  - Input: `tools/README.md`, `design.md` §11
  - Output: the predicted-against-measured table in § *Phase 3 as executed*
  - Notes: `symbolwatch.tsv` is untouched in this phase — the rows are phase 4's, deliberately, for
    the reason in finding 4

- [ ] **Task 3.8:** Write § *Phase 3 as executed*
  - Input: the run outputs and the stage-3 rulings
  - Output: a section in this file carrying the three stages' measured costs, the survivor counts by
    band, the confirmed-dead set with its page and site counts, and any finding the design missed
  - Notes: name the confirmed-dead set explicitly, with page and site counts re-derived by two
    methods — phase 4's whole scope is that list and inheriting it is how a repair phase misses a page

---

## Phase 4 — the watchlist rows and the page repairs *(10 tasks, one PR)*

**Branch:** `spec/015-phase4-repairs`. **Goal:** P0-5 and P0-6. **This is the only phase that
changes the published site**, so obligation 7 binds: ask before merging and name the head-ref
deletion in the same breath.

**The order inside the branch is rows → red run → repairs → green run** (finding 4). Reversing it
loses the only proof the rows do anything.

- [ ] **Task 4.1:** Re-derive the repair scope from phase 3's confirmed-dead set
  - Input: task 3.8's list
  - Output: in this file, a table of confirmed-dead name → pages → sites → fenced blocks, each count
    produced by **two** methods (per-name `grep -rlw` piped to `sort -u`, and one alternation
    `grep -rlE`), with prose sites separated from fenced ones
  - Notes: today, for the design's twelve, that is **9 pages, 18 sites, 10 fenced blocks, 4 prose
    sites** (finding 3). Phase 3 will have added names, so re-derive rather than starting from this

- [ ] **Task 4.2:** Add the `symbolwatch.tsv` rows
  - Input: `tools/symbolwatch.tsv`'s existing rows as the format, task 4.1's list, `design.md` §6
  - Output: one row per confirmed-dead name — symbol, product, replacement, evidence, first_seen —
    committed **before** any page is edited
  - Notes: **the replacement column is a name, not a type.** 014's watchlist named the right name
    and the wrong type at 9 of 17 sites: applied verbatim those repairs satisfy the gate and do not
    compile. Read the replacement type's members before writing it down. A name with no replacement
    says so rather than inventing one

- [ ] **Task 4.3:** Record the red run — the gate firing on every site, before the repair
  - Input: task 4.2's rows
  - Output: the full `python3 tools/symbolcheck.py` output pasted into § *Phase 4 as executed*,
    exit code shown, naming every site of every new row
  - Notes: obligation 2, and the only red-proof these rows will ever get. The site count here must
    equal task 4.1's — if the gate sees fewer, the difference is a site the gate cannot see and that
    is a finding about the instrument, not a rounding error. **This run is one half of a two-way
    control and task 4.9's green run is the other**; neither half means anything alone, because a
    gate that has never been red and a gate that is still red are both uninformative

- [ ] **Task 4.4:** Repair the two starting pages
  - Input: task 4.3's site list for `ShowMeTheCode.md` and `BrighterBasicConfiguration.md`;
    `../Brighter/samples/` for the V10 form of whatever the dead call did; `CLAUDE.md`'s
    version-marker and complete-code-block conventions
  - Output: the named sites gone from both pages, each edited fence carrying its `using` directives
  - Notes: these are where a newcomer starts, which is the worst possible place for a block that no
    longer compiles. `CommandProcessorLifetime` and `UseInMemoryOutbox` are the design's two names
    here; phase 3 may add more. **Change nothing else about the page** — no banner, no page type, no
    opening sentence; a triage repair that reorganises a page cannot be reviewed as a triage repair

- [ ] **Task 4.5:** Repair the outbox and sweeper family
  - Input: task 4.3's site list for `SweeperCircuitBreaking.md`, `UsingSweeperCircuitBreaking.md`,
    `DapperOutbox.md`, `DynamoOutbox.md`, `S3LuggageStore.md`
  - Output: the named fenced sites gone from all five pages, `using` directives carried
  - Notes: `DynamoOutbox.md` has **two** blocks. `UseMySqTransactionConnectionProvider` is a
    misspelling of a name that was itself removed — record which of the two defects it is before
    repairing it, because they are different findings

- [ ] **Task 4.6:** Repair `DispatchingARequest.md` and `FeatureSwitches.md`
  - Input: task 4.3's site list for both; `IUnitOfWork` and `NoTaskQueues` are the design's names
  - Output: the named fenced sites gone from both pages, `using` directives carried
  - Notes: `FeatureSwitches.md` is one of the five pages carrying 011's unresolved Darker exclusion
    claim. **Do not touch that claim here** — it is blocked on Darker source ahead of the deployed
    release and is not this spec's to rule on

- [ ] **Task 4.7:** Rule on the four prose sites — rewrite or opt out
  - Input: `SweeperCircuitBreaking.md:230,232,234` (a bulleted list of provider methods) and
    `S3LuggageStore.md:38`; ruling 6 and ruling 4
  - Output: each prose site either rewritten or carrying a `<!-- symbolcheck: allow <name> -->`
    opt-out, and `symbolcheck` printing *0 findings, N silenced* with N re-derived
  - Notes: **an opt-out is never silent** — *0 findings* and *0 findings, N silenced* are different
    claims and only one of them is true. `DispatcherConfigurationReference.md`'s `UseExternalInbox`
    is the existing precedent

- [ ] **Task 4.8:** Build every edited block against the **released** packages
  - Input: each block task 4.4–4.6 touched; `CLAUDE.md` § *Compiling an example, and against what*
  - Output: in § *Phase 4 as executed*, a per-block build result — the scratch project's package
    references and the build output — and `python3 tools/pagelint.py --changed origin/master` at
    **0 errors**
  - Notes: AC12. **Never a `ProjectReference` into `../Brighter/src`** — that compiles against
    unreleased code and vouches for an API nobody can install, which spec 013 phase 2 did. `git add`
    before the `--changed` run or the strict pass sees nothing. A block whose omission is genuine
    says `// ...` and is downgraded to a counted warning, not silenced

- [ ] **Task 4.9:** Re-run the eight gates, reconcile, and update `tools/README.md`
  - Input: `tools/README.md`'s table, `design.md` §11
  - Output: the predicted-against-measured table in § *Phase 4 as executed*, and the `symbolcheck`
    row in `tools/README.md` updated to the new entry count — **that file and nowhere else**
  - Notes: two figures are expected to move and both must be explained: `symbolcheck`'s entry count
    by exactly the number of rows added, and `pagelint`'s repo-wide warning count, which may **fall**
    as repaired blocks gain their directives. **Name the blocks that moved it** — a debt figure that
    drops for unexplained reasons stops meaning anything. `--verify-list` must stay at 0 findings
    with every new row DEAD at both refs of its product

- [ ] **Task 4.10:** Write § *Phase 4 as executed*, and ask for the merge
  - Input: tasks 4.3 to 4.9
  - Output: a section in this file carrying the red run, the repairs by page, the per-block build
    results, the gate reconciliation — and, separately, the merge ask naming the branch to delete
  - Notes: obligation 7. **A merge is not a deletion**; both are asked for by name, in one breath,
    and the deletion is not assumed from the merge

---

## Phase 5 — acceptance *(6 tasks, one PR)*

**Branch:** `spec/015-phase5-acceptance`. **Goal:** the walk, the backwards check, both ledgers and
the close. **Changes no published page.**

- [ ] **Task 5.1:** Walk the criteria with **no instrument** first — AC4, AC6, AC10
  - Input: `requirements.md` § *Acceptance criteria*; `triage.md`; every document 015 ships
  - Output: a § *Acceptance walk* section, three entries, each naming who read what and what they
    found
  - Notes: **both criteria ever found unmet at a close — 009's AC7 and 012's AC1 — were unmarked
    ones**, and 014's five acceptance repairs all came out of its two uninstrumented criteria while
    every instrumented one was green and uninformative. AC6 is the one that fails quietly: sample
    verdict rows and check each cites a control, not just an adjective

- [ ] **Task 5.2:** Walk the instrumented criteria — AC1, AC2, AC3, AC5, AC7, AC9, AC11, AC12
  - Input: each criterion's named instrument in `requirements.md`
  - Output: eight entries in the same section, each with the command **run at the walk** and its
    output, against what the criterion claims
  - Notes: friction 39 — **run the instrument and check its output against what the criterion
    says**, do not tick from the phase write-up. AC3 is the worked example of why: its first form
    was green before its feature existed, satisfied by `cceeded` inside `Succeeded`

- [ ] **Task 5.3:** The backwards check — what changed that should not have
  - Input: `git diff --stat 1524662..HEAD`
  - Output: a paragraph naming every file 015 touched, with the pages reconciled against task 4.1's
    list and any file outside `tools/symbolcheck.py`, `tools/symbolwatch.tsv`, `tools/README.md`,
    `spec/015-census_triage/` and task 4.1's pages explained or reverted
  - Notes: *"while I'm here"* is how a spec quietly widens. A repaired page that also gained a
    heading fix is two changes in a diff that claims to be one

- [ ] **Task 5.4:** Write the defect ledger
  - Input: every phase's *as executed* section
  - Output: a § *Defect ledger* table — every defect 015 found, with a **found by** column
    distinguishing the instrument's wins from the re-derivation's
  - Notes: the distinction is the point. Finding 3's nine pages and finding 1's moved ref were both
    found by re-derivation at the top of this list, not by any gate — and the twelve names were
    found by a design measuring something it had been told to skip

- [ ] **Task 5.5:** Write the friction ledger, from 46
  - Input: this spec's phases; the ledger stands at **45**, with 41–42 in `requirements.md` and
    43–45 in `design.md`
  - Output: a § *Workflow friction* section in this file, numbered from **46**, contiguous
  - Notes: re-derive the inherited 45 rather than trusting this line — the ledger header in
    `PROMPT.md` said 39 while its own body cited a friction 40, for two sessions. Friction is what
    the next spec inherits and it is as much a product of 015 as `triage.md` is

- [ ] **Task 5.6:** Close — the README checklist and the residual gap
  - Input: `spec/015-census_triage/README.md`'s status checklist; the acceptance walk
  - Output: the README checklist completed, and **one sentence** naming what 015 leaves behind, in
    the shape of 014's *"a dead API written into prose on an existing page — uncompiled, and not on
    `symbolwatch.tsv` — is caught by nothing"*
  - Notes: that sentence is the line the next spec starts from, and 014's was worth more than its
    thirty-nine tasks. The candidates today are the **prose surface** D12 ruled unusable and the
    **757 using-directive blocks** — the half of the census problem only a compiler can resolve.
    `.accepted` is the maintainer's to create, after the walk is read

---

## 3. What this list does not plan

- **Promoting `--census` to a gate.** Forbidden by its own docstring and by ruling 12; exit 0
  whatever it finds.
- **The BCL/ASP.NET resolution filter.** Measured, rejected, kept as evidence — `bclprobe.py` is an
  **input** to stage 3, never a filter.
- **The 757 using-directive blocks**, except the blocks phase 4 edits, which are pulled into rule 6's
  strict scope by being edited at all.
- **The prose-surface census.** AC8's last row says so in the record, in its own words.
- **Anything in `../Brighter` or `../Darker`.** Read-only; nothing here needs a sample, and the
  tutorial-samples exception is not reached.
- **P1-2**, withdrawn when Q2 was reversed — a policy about what you did not triage is not needed
  once you have triaged it.

---

## Phase 1 prediction

**Written 2026-09-19, on `spec/015-phase1-instrument`, and committed in its own commit *before* any
edit to `tools/symbolcheck.py`.** AC2 asks for a before- and an after-count, and a prediction
committed alongside the change it predicts is a prediction nobody can date. Task 1.1.

### The drift control, re-run first

Every figure below rests on `perpageprobe.py` still being a faithful copy of `census()`
(`design.md` §10.2). Run with no extra filter it must reproduce the shipped tool exactly, and its
two code paths must agree:

```text
sc.census(pages())[0]                  -> 2019   (counts['candidates'] = 2019)
stage_count(pages(), [])               -> 2019   MATCH
stage_count(pages(), [METHOD_DECL_RE]) -> 1749
len(census_per_page(pages()))          -> 1749   AGREE
```

**2019 = 2019, and the two probe paths agree at 1749.** The copy has not drifted, so §2's table is
measuring what P0-1 would ship.

> **`design.md` §10.2's four lines are shorthand, not a runnable script, and the first attempt to
> run them verbatim returned `2`.** `census()` returns a **tuple** — `(candidates, counts)` — so
> `len(sc.census(pages()))` is the length of that tuple and not a candidate count. The harness was
> wrong and the tool was fine. It is recorded because `2` is only obviously absurd here: had the
> control been `len()` of something two-element-shaped and plausible, it would have read as drift in
> `census()` and sent phase 1 looking for a defect that does not exist. **A control's own harness is
> an instrument and it gets no exemption from being checked.** Both halves are now asserted by
> name — `MATCH` against the shipped tool, `AGREE` between the probe's two paths — rather than
> eyeballed.

### The refs, re-resolved today

Unmoved since this list was written on 2026-09-18, which is a measurement and not an assumption:

```bash
git -C ../Brighter rev-parse --short origin/master   # 6145913a0
git -C ../Brighter rev-parse --short 10.7.0          # c1b8af886
git -C ../Brighter rev-parse --short 09f5d988f       # 09f5d988f   the pin, still resolves
git -C ../Darker   rev-parse --short origin/master   # 2f76cda
git -C ../Darker   rev-parse --short 4.1.1           # ddb71ee
git -C ../Darker   rev-parse --short 2f76cda         # 2f76cda
```

**Brighter's `origin/master` is still one commit ahead of the pin; Darker's *is* the pin.** So this
phase is the first thing 015 does with the two Brighter refs genuinely different, which is what
makes task 1.4's AC6 check informative rather than rhetorical.

### The prediction: 929 → 819

Re-derived today by two methods that agree, at `6145913a0` / `2f76cda`:

```bash
python3 spec/015-census_triage/probe/methodprobe.py  | head -4   # 929, 110, 31, 819
python3 spec/015-census_triage/probe/perpageprobe.py | head -4   # 929 / 819 / 819
```

| | names | ≥7 | ≥5 | ≥4 | ≥3 | ≥2 |
|---|---:|---:|---:|---:|---:|---:|
| shipped today, types only | 929 | 40 | 63 | 73 | 115 | 233 |
| all-or-nothing (`methodprobe`) | 819 | 38 | 60 | 70 | 112 | 220 |
| **per page — what P0-1 ships** | **819** | 36 | 57 | 65 | 103 | 210 |

**Predicted after P0-1: 819 candidates, a movement of 110 names.** Predicted blind spot: **1** name,
`Greeting`, at 7 pages — a modifier-less declaration the filter is anchored not to see.

> **This prediction was measured at `6145913a0`, and task 1.5's before-run is at the pin
> `09f5d988f`.** They are one commit apart — a one-line change in `CommandProcessor.cs` — so this is
> a prediction about a world one commit away from the one the phase measures. **If the before-run at
> the pin is not 929, that is a finding about the census's sensitivity to the ref, not a failed
> prediction** (finding 1, and obligation 9). Record it, and carry the *pinned* figure forward: the
> pinned pair is the one that counts, because it is the pair both halves of AC2 are measured at.

### The eight gates, predicted before the work

Per obligation 6 — including every "none", with the reason it is none. Figures are **cited from
`tools/README.md`, not pasted here** (obligation 10); the prediction is movement, not a number.

| # | Gate | Predicted | Why |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | Two independent reasons, and the second is the load-bearing one. `spec/` is in `SKIP_DIRS` (`tools/linkcheck.py:52`), so this file is outside the corpus — **and the walk only opens `.md` files at all** (`tools/linkcheck.py:112`), so `tools/symbolcheck.py` cannot enter it either. The 164 → 165 precedent was a **new `.md` file inside `tools/`**; phase 1 adds no `.md` anywhere in the walk |
| 2 | `pagelint` | **none**, errors and warnings both | Its corpus is `contents/` plus the root `README.md`. Phase 1 touches neither. The warning count is the number most likely to move unintentionally across this spec, but only P0-6 edits a C# block on a page |
| 3 | shape | **none** | `SUMMARY.md` is untouched; no page is created, nested or moved |
| 4 | redirects | **none** | Redirects follow `SUMMARY.md`, which is untouched |
| 5 | `versioncheck` | **none** | It reads version pins in published pages; phase 1 edits a tool and a spec document |
| 6 | `optioncheck` | **none** | It reflects over marked option tables in pages; none is touched |
| 7 | `--verify` | **none** | The published-URL set is unchanged, for the same reason as shape |
| 8 | `symbolcheck` | **none** — gate **and** `--verify-list` | The mechanical reason, not a judgement: `census()` and `universe()` are reached **only** from `run_census`, which is reached only from `--census` (`tools/symbolcheck.py:761`). `CENSUS_PINS` and `MEMBER_DECL_RE` are both inside that path. The gate and `verify_row` read `PRODUCT_REFS` directly (`:623`), which task 1.2 must not touch |

**The two predictions of "none" worth distrusting are 1 and 2** (`design.md` §11 names both). Neither
is trusted here on the grounds that it did not move last time: `linkcheck`'s is re-argued from the
`.md`-only walk, and `pagelint`'s from a corpus phase 1 does not touch. Task 1.10 reconciles all
eight against this table, and an unpredicted movement is a finding rather than a number to adopt.

---

## Phase 1 as executed

**Written across 2026-09-19, on `spec/015-phase1-instrument`.** Tasks 1.3 to 1.8's recorded output,
the reconciliation against § *Phase 1 prediction*, and the findings the list did not predict.

### The pin resolves, and an unresolvable one is exit 2 *(task 1.3)*

Both halves run, because a function printed in a design is a function nobody has executed. The
**positive** half — all four refs the header will print, resolved through `resolve_sha` itself
rather than through a shell `git rev-parse` standing in for it:

```text
../Brighter  10.7.0       -> c1b8af886
../Brighter  09f5d988f    -> 09f5d988f
../Darker    4.1.1        -> ddb71ee
../Darker    2f76cda      -> 2f76cda
```

The **negative** half is the one that matters. `CENSUS_PINS['brighter']` set to `deadbeef1` — a SHA
this checkout has never had — driven through the real `--census` entry point, not through a direct
call to `resolve_sha`:

```text
census cannot run: ../Brighter cannot resolve deadbeef1: fatal: Needed a single revision. A pinned
census that silently falls back to a branch is a figure wearing another figure's SHA
exit 2
```

**Exit 2, and not one census number printed** — `tools/README.md`'s contract is *nothing was
checked*, and nothing was. The tempting alternative, a warning and a fall back to `origin/master`,
produces a census whose header says one thing and whose numbers mean another; this is the run that
shows it cannot happen.

### The gate and `--verify-list` still follow `origin/master` *(task 1.4)*

AC6. This is checkable rather than rhetorical only because finding 1 moved the two refs apart:
`origin/master` is `6145913a0` and the pin is `09f5d988f`, so a gate that had silently adopted the
pin would now say so.

```text
control: CommandProcessor         LIVE         43 files at 10.7.0, 45 at origin/master
control: IAmAnIbox                DEAD         0 files at 10.7.0, 0 at origin/master

IAmACommandStoreAsync        brighter  DEAD         0 at 10.7.0, 0 at origin/master
UseExternalInbox             brighter  DEAD         0 at 10.7.0, 0 at origin/master
IAmAnIbox                    brighter  DEAD         0 at 10.7.0, 0 at origin/master
IMessageScheduler            brighter  DEAD         0 at 10.7.0, 0 at origin/master
IMessageSchedulerFactory     brighter  DEAD         0 at 10.7.0, 0 at origin/master

All 5 entries still dead at both refs of their product, and every named replacement still live.
```

**Seven rows, and every one names `origin/master`.** Not a SHA, and not the pin. Beside it the gate
itself, at `tools/README.md`'s figure and unmoved: `No watchlisted symbols found (5 entries, 161
pages checked, 1 silenced)`, exit 0 — the silenced site being
`DispatcherConfigurationReference.md`'s `UseExternalInbox`, which is a precedent and not a hole.

### The before-count, at the pin *(task 1.5)*

Obligation 9: this run is one half of AC2, and it is worthless unless the header beside it shows
the SHAs. Here they are in the same paste, which is the whole point of AC3:

```text
token sets, src/ only, release tag and pinned master per product:
  brighter  10.7.0          c1b8af886     7593 tokens
  brighter  pinned master   09f5d988f     7713 tokens
  darker    4.1.1           ddb71ee        313 tokens
  darker    pinned master   2f76cda        313 tokens
controls OK: CommandProcessor present in every Brighter set, IAmAnIbox in none

pages examined                     : 161
...with at least one C# fence      : 145
distinct tokens in those fences    : 2764
...after comments and strings      : 2280
...after page declarations, noise  : 2019
UNRESOLVED at src/ of both products, both refs : 929
pages carrying at least one        : 129 of 145
```

**929 at the pin, which is what the prediction said — and the prediction was measured at
`6145913a0`.** The census therefore reports the same total at both SHAs, and **friction 43 says
exactly what that is worth: a number that survives a change is not evidence that nothing changed.**
The commit between them touches `CommandProcessor.cs`, and `CommandProcessor` is a name the census
resolves at every ref, so there was never a mechanism by which this number could have moved. The
figure carried forward is the pinned one, because both halves of AC2 must come from one world.
