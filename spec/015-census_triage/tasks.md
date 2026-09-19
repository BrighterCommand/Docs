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

- [x] **Task 1.6:** Add `MEMBER_DECL_RE` and the per-page member filter
  - Input: `design.md` §2.2 (the regex and the two lines it goes beside), §2.1 (why *per page* and
    not all-or-nothing), `tools/symbolcheck.py:462-473` (`DECL_RE`, applied per page inside the loop)
  - Output: three lines in `census()` — the regex, and `declared.update(MEMBER_DECL_RE.findall(
    strip_noncode(body)))` beside the existing `DECL_RE` line
  - Notes: **keep the `strip_noncode` asymmetry and comment it.** Tidying `DECL_RE` into the
    stripped body in the same commit would move the type filter's output and the member filter's at
    once, and neither movement could then be attributed

- [x] **Task 1.7:** Record the **after** count at the same SHA pair, and reconcile against 1.1
  - Input: task 1.1's prediction, task 1.5's before-run
  - Output: in § *Phase 1 as executed*, the second `--census` header and total, an explicit
    *predicted N, measured M* line, and an explanation of any difference rather than a quiet
    adoption of the new number
  - Notes: AC2 depends on AC3 — the two headers must show the same four SHAs, and that is checkable
    by eye in the paste. Expected movement is **110 names**, 929 → 819

- [x] **Task 1.8:** Red-proof the member filter, with a positive case outside the enumeration
  - Input: `design.md` §2.3 (the blind spot, one name wide), §10.4's three controls, friction 36
  - Output: in § *Phase 1 as executed*: `methodprobe.py` reporting **0** removable after the change
    (AC1's red-proof), the three controls from §10.4, and a **two-way planted control** — a
    scratch page carrying a modifier-bearing declaration whose name must disappear from the census,
    and one carrying a **modifier-less** declaration whose name must survive
  - Notes: the modifier-less case is the positive from outside the enumeration, and it is expected
    to **survive** — the filter is anchored on a modifier by design. A control that proves the blind
    spot exists and is one name wide is worth more than one that proves the filter works on cases it
    was built from. Do not commit the scratch pages; paste their output

- [x] **Task 1.9:** Write P1-1 — how much of finding E's 808-name gap is recoverable
  - Input: `design.md` §8's staged table, `spec/014-documentation_workflow/tasks.md` finding E
  - Output: a § *P1-1: the gap is not members* section in this file carrying the re-derived staged
    table (`perpageprobe.py --stages`) and the sentence that **481 of 808 are unexplained by member
    declarations of any kind**
  - Notes: **do not "correct" 831 or 1,211 in 014's `design.md`.** They are the numbers that design
    was approved on and 015's job is to explain them, not overwrite them

- [x] **Task 1.10:** Re-run the eight gates and reconcile against the prediction
  - Input: task 1.1's predicted figures, `tools/README.md`'s table
  - Output: an eight-row table in § *Phase 1 as executed*, predicted against measured, citing
    `tools/README.md`'s figures rather than pasting them
  - Notes: `spec/` is in `linkcheck`'s `SKIP_DIRS` (`tools/linkcheck.py:52`) so this file is outside
    its corpus — but `tools/` is inside the walk, which moved `linkcheck` 164 → 165 once before. A
    gate's scope is not the site's scope

- [x] **Task 1.11:** Write § *Phase 1 as executed*
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

- [x] **Task 2.1:** Write `triage.md`'s method section
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

- [x] **Task 2.2:** Write `probe/triagerun.py`
  - Input: `design.md` §4.3's two queries and §10.5's pilot output; finding 2 above
  - Output: `spec/015-census_triage/probe/triagerun.py`, taking the census's candidate list and
    emitting one row per name: name, page-spread, stage-1 counts per product, stage-2 counts per
    product, live counts, and the stage-2.5 dot evidence
  - Notes: **it queries `CENSUS_PINS`'s SHAs, not `origin/master`** (finding 2). It must be
    resumable or checkpointed — stage 1 alone is 420s and stage 2 about 18 minutes, and a run that
    loses its output to a timeout is a run nobody repeats

- [x] **Task 2.3:** Red-proof the query form — the `\b` trap, recorded as a broken instrument
  - Input: `design.md` §4.3's four-row table
  - Output: in § *Phase 2 as executed*, all four forms run against `IAmACommandStoreAsync` (which
    **must** stay > 0) and `Date`, with the two `\b`/`\<\>` forms shown returning **0 for the
    positive control**
  - Notes: obligation 2 — record the mismatch before fixing it. This is **plausible zero number
    eight** and the second one this programme has found in a word-boundary flag; the paste is the
    only thing that stops a later reader "simplifying" the bracket class back to `\b`

- [x] **Task 2.4:** Wire the three two-way controls and the two planted positives into the runner
  - Input: `design.md` §10.4's control block, §4.6 (why the plant stays now the corpus yields)
  - Output: `triagerun.py` asserting, on every run: `IAmACommandStoreAsync` → EXISTED, REMOVED;
    `IAmAMessageScheduler` → LIVE; `OrderId` → NEVER EXISTED; and both planted names classifying
    EXISTED, REMOVED — with the assertions printed, not merely evaluated
  - Notes: friction 44 — **check each control can pass at all** before trusting a red one. The
    plant's justification changed when Q2 was reversed and it still stands: twelve positives
    somewhere in 819 rows does not show that row 400 was classified by a working instrument

- [x] **Task 2.5:** Re-run the eight gates; predicted **none**
  - Input: `tools/README.md`'s table, `design.md` §11
  - Output: the eight-row predicted-against-measured table in § *Phase 2 as executed*
  - Notes: predicted none because this phase writes only under `spec/`, which `linkcheck` skips and
    no other gate walks. Say that, rather than reporting a pass

- [x] **Task 2.6:** Write § *Phase 2 as executed*
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

> ### Amendment, 2026-09-19 — task 3.3 asked for the truncation phase 2 found was a defect
>
> **Taken at the maintainer's instruction, at the top of phase 3, and recorded before it was
> applied** (obligation 2). Task 3.3's Output read:
>
> > *"for every stage-2 survivor, the **first** historical `+`/`-` line its name appears on, in the
> > row"*
>
> That is `design.md` §10.6's `… | head -1`, and phase 2 measured it as the defect that produces
> **right answers with wrong reasons**. On `Date` the first line is `Get<T>(DateTime date, …)` — a
> parameter name, from which §10.6's table concluded *Own API? no*. The full set of 26 also holds
> `public DateTime Date { get; set; }`, a public property of `DynamoDbMessage` in
> `src/Paramore.Brighter.Outbox.DynamoDB/`, removed in 2019. **Same name, same query, opposite
> readings; only the truncation decided which one a person saw.** `triage.md` §1.3.
>
> **It does not overturn the verdict on `Date` — it overturns the reason, and under a three-valued
> vocabulary the reason is the verdict.** So the Output now asks for the whole distinct evidence
> set, capped, with the `dotted`/`bare` totals taken over **all** matches beside it so a reader can
> always see whether the lines in front of them are the whole set.
>
> **The runner already does this** — `EVIDENCE_CAP = 40`, `probe/triagerun.py:98` — so nothing about
> phase 3's execution changes. What changes is that the task list no longer asks for something the
> phase before it proved wrong. **The wording was the last place the finding had not reached**,
> which is [AC11 and P0-6's shape](#what-ac11-and-p0-6-said-before-the-tasks-review-and-why-they-changed)
> one phase later: an amendment that lands in some places and not others leaves the unamended ones
> looking deliberate.

- [x] **Task 3.1:** Run stage 1 over the whole census, at the pin
  - Input: phase 1's `--census` output as the candidate list; `triagerun.py`
  - Output: a checkpoint file under `spec/015-census_triage/` holding one stage-1 row per name, and
    in this file the measured wall clock, the count screened out and the count surviving
  - Notes: the design measured **420s, 720 screened out, 99 survivors** at `09f5d988f`; re-derive.
    A zero at stage 1 is **conclusive** — if the identifier ever existed, its substring existed — and
    that asymmetry is what makes it safe to screen 720 names and never look at them again

- [x] **Task 3.2:** Run stage 2 over the survivors
  - Input: task 3.1's checkpoint
  - Output: stage-2 counts per product appended to every survivor row, with the run's wall clock
  - Notes: the bracket-class form only. If the survivor count is far from 99, stop and reconcile
    before spending 18 minutes — a large divergence from the design's measurement means the corpus
    or the query changed, and finding 1 says the corpus can move under you

- [x] **Task 3.3:** Add the stage-2.5 dot evidence
  - Input: `design.md` §10.6 — `ConfigurationManager.GetSection(…)` against
    `private readonly IAmACommandStoreAsync _commandStore;`
  - Output: for every stage-2 survivor, **the whole distinct evidence set** its name appears on —
    every `+`/`-` line, deduplicated, to `EVIDENCE_CAP` per product — with the `dotted`/`bare`
    totals taken over **all** matches beside it, so the row says whether the lines shown are the
    whole set. **Amended 2026-09-19; this read "the first … line" and the amendment is above**
  - Notes: **evidence, never a filter.** A Brighter extension method is called with a dot too, which
    is the collision D12 named and `bclprobe` demonstrated. Presenting it as a filter is how the
    twelve would have been lost — and presenting **one line of it** is how `Date`'s public property
    was lost, which is what the amendment is for

- [x] **Task 3.4:** Stage 3 — read the survivors and rule each one
  - Input: task 3.3's rows; for each survivor, the diff hunks its name appears in
  - Output: a `LIVE` / `EXISTED, REMOVED` / `NEVER EXISTED` verdict per survivor, each with the
    quoted source line that decided it and the control it was checked against
  - Notes: this is the boundary the whole spec turns on — *the product's own API* against *a name
    the product's source merely contained*. All seven of the ≥3 head's survivors read as **uses**;
    the twelve in the tail read as **declarations**. **87 of the 99 are unread** and the twelve are
    a floor. A `LIVE` verdict is a defect in the instrument, not in a page, and gets recorded as one

- [x] **Task 3.5:** Write `triage.md`'s record — one row per name
  - Input: tasks 3.1 to 3.4
  - Output: `triage.md` §5, a table with **one row per census name**, each carrying the verdict, the
    two commands that produced it, and its control
  - Notes: AC5 is checked two ways — `grep -c` on the table against the count `--census` reports, and
    the sum of the three verdict classes against the same total. The 720 screened at stage 1 get
    rows too: *screened at stage 1, 0 history* is a verdict with evidence

- [x] **Task 3.6:** Write AC8 — what 015 did **not** triage, each figure with its command
  - Input: `design.md` §9's table
  - Output: a § *What 015 did not triage* section in this file, carrying the page-declared members
    P0-1 removes — **110 today, re-derived from phase 1's own run at the pin, not from this
    number** — the `NOISE_EXACT`/`NOISE_PREFIX` drops
    (`tools/symbolcheck.py:348-359`), the names that resolve, and the **prose surface** — each with
    the command that produced its number
  - Notes: the last row is the one to write carefully. **Only C# fences have been swept**, and a
    document reporting 819 verdicts invites the reading that the documentation has been. D12
    conclusion 2: 160 of 161 pages carry an unresolved prose token

- [x] **Task 3.7:** Re-run the eight gates; predicted **none**
  - Input: `tools/README.md`, `design.md` §11
  - Output: the predicted-against-measured table in § *Phase 3 as executed*
  - Notes: `symbolwatch.tsv` is untouched in this phase — the rows are phase 4's, deliberately, for
    the reason in finding 4

- [x] **Task 3.8:** Write § *Phase 3 as executed*
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

- [x] **Task 4.1:** Re-derive the repair scope from phase 3's confirmed-dead set
  - Input: task 3.8's list
  - Output: in this file, a table of confirmed-dead name → pages → sites → fenced blocks, each count
    produced by **two** methods (per-name `grep -rlw` piped to `sort -u`, and one alternation
    `grep -rlE`), with prose sites separated from fenced ones
  - Notes: today, for the design's twelve, that is **9 pages, 18 sites, 10 fenced blocks, 4 prose
    sites** (finding 3). Phase 3 will have added names, so re-derive rather than starting from this

- [x] **Task 4.2:** Add the `symbolwatch.tsv` rows
  - Input: `tools/symbolwatch.tsv`'s existing rows as the format, task 4.1's list, `design.md` §6
  - Output: one row per confirmed-dead name — symbol, product, replacement, evidence, first_seen —
    committed **before** any page is edited
  - Notes: **the replacement column is a name, not a type.** 014's watchlist named the right name
    and the wrong type at 9 of 17 sites: applied verbatim those repairs satisfy the gate and do not
    compile. Read the replacement type's members before writing it down. A name with no replacement
    says so rather than inventing one

- [x] **Task 4.3:** Record the red run — the gate firing on every site, before the repair
  - Input: task 4.2's rows
  - Output: the full `python3 tools/symbolcheck.py` output pasted into § *Phase 4 as executed*,
    exit code shown, naming every site of every new row
  - Notes: obligation 2, and the only red-proof these rows will ever get. The site count here must
    equal task 4.1's — if the gate sees fewer, the difference is a site the gate cannot see and that
    is a finding about the instrument, not a rounding error. **This run is one half of a two-way
    control and task 4.9's green run is the other**; neither half means anything alone, because a
    gate that has never been red and a gate that is still red are both uninformative

- [x] **Task 4.4:** Repair the two starting pages
  - Input: task 4.3's site list for `ShowMeTheCode.md` and `BrighterBasicConfiguration.md`;
    `../Brighter/samples/` for the V10 form of whatever the dead call did; `CLAUDE.md`'s
    version-marker and complete-code-block conventions
  - Output: the named sites gone from both pages, each edited fence carrying its `using` directives
  - Notes: these are where a newcomer starts, which is the worst possible place for a block that no
    longer compiles. `CommandProcessorLifetime` and `UseInMemoryOutbox` are the design's two names
    here; phase 3 may add more. **Change nothing else about the page** — no banner, no page type, no
    opening sentence; a triage repair that reorganises a page cannot be reviewed as a triage repair

- [x] **Task 4.5:** Repair the outbox and sweeper family
  - Input: task 4.3's site list for `SweeperCircuitBreaking.md`, `UsingSweeperCircuitBreaking.md`,
    `DapperOutbox.md`, `DynamoOutbox.md`, `S3LuggageStore.md`
  - Output: the named fenced sites gone from all five pages, `using` directives carried
  - Notes: `DynamoOutbox.md` has **two** blocks. `UseMySqTransactionConnectionProvider` is a
    misspelling of a name that was itself removed — record which of the two defects it is before
    repairing it, because they are different findings

- [x] **Task 4.6:** Repair `DispatchingARequest.md` and `FeatureSwitches.md`
  - Input: task 4.3's site list for both; `IUnitOfWork` and `NoTaskQueues` are the design's names
  - Output: the named fenced sites gone from both pages, `using` directives carried
  - Notes: `FeatureSwitches.md` is one of the five pages carrying 011's unresolved Darker exclusion
    claim. **Do not touch that claim here** — it is blocked on Darker source ahead of the deployed
    release and is not this spec's to rule on

- [x] **Task 4.7:** Rule on the four prose sites — rewrite or opt out
  - Input: `SweeperCircuitBreaking.md:230,232,234` (a bulleted list of provider methods) and
    `S3LuggageStore.md:38`; ruling 6 and ruling 4
  - Output: each prose site either rewritten or carrying a `<!-- symbolcheck: allow <name> -->`
    opt-out, and `symbolcheck` printing *0 findings, N silenced* with N re-derived
  - Notes: **an opt-out is never silent** — *0 findings* and *0 findings, N silenced* are different
    claims and only one of them is true. `DispatcherConfigurationReference.md`'s `UseExternalInbox`
    is the existing precedent

- [x] **Task 4.8:** Build every edited block against the **released** packages
  - Input: each block task 4.4–4.6 touched; `CLAUDE.md` § *Compiling an example, and against what*
  - Output: in § *Phase 4 as executed*, a per-block build result — the scratch project's package
    references and the build output — and `python3 tools/pagelint.py --changed origin/master` at
    **0 errors**
  - Notes: AC12. **Never a `ProjectReference` into `../Brighter/src`** — that compiles against
    unreleased code and vouches for an API nobody can install, which spec 013 phase 2 did. `git add`
    before the `--changed` run or the strict pass sees nothing. A block whose omission is genuine
    says `// ...` and is downgraded to a counted warning, not silenced

- [x] **Task 4.9:** Re-run the eight gates, reconcile, and update `tools/README.md`
  - Input: `tools/README.md`'s table, `design.md` §11
  - Output: the predicted-against-measured table in § *Phase 4 as executed*, and the `symbolcheck`
    row in `tools/README.md` updated to the new entry count — **that file and nowhere else**
  - Notes: two figures are expected to move and both must be explained: `symbolcheck`'s entry count
    by exactly the number of rows added, and `pagelint`'s repo-wide warning count, which may **fall**
    as repaired blocks gain their directives. **Name the blocks that moved it** — a debt figure that
    drops for unexplained reasons stops meaning anything. `--verify-list` must stay at 0 findings
    with every new row DEAD at both refs of its product

- [x] **Task 4.10:** Write § *Phase 4 as executed*, and ask for the merge
  - Input: tasks 4.3 to 4.9
  - Output: a section in this file carrying the red run, the repairs by page, the per-block build
    results, the gate reconciliation — and, separately, the merge ask naming the branch to delete
  - Notes: obligation 7. **A merge is not a deletion**; both are asked for by name, in one breath,
    and the deletion is not assumed from the merge

---

## Phase 5 — acceptance *(6 tasks, one PR)*

**Branch:** `spec/015-phase5-acceptance`. **Goal:** the walk, the backwards check, both ledgers and
the close. **Changes no published page.**

- [x] **Task 5.1:** Walk the criteria with **no instrument** first — AC4, AC6, AC10
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

### The after-count, at the same SHA pair *(task 1.7)*

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
...after page declarations, noise  : 1749
UNRESOLVED at src/ of both products, both refs : 819
pages carrying at least one        : 127 of 145
```

**Predicted 819, measured 819** — a movement of exactly the predicted 110 names. The stage above it
moved with it, 2019 → 1749, and the pages carrying at least one candidate fell 129 → 127.

**AC2 depends on AC3, and this is where that is cashed in.** The two headers are checkable against
each other by eye, and mechanically:

```bash
diff <(head -6 before.txt) <(head -6 after.txt)    # no output — the same four SHAs
```

Both halves of AC2 are therefore measurements of one world. Nothing was adopted quietly: the
prediction, the before-run and the after-run all say 929 → 819 at `09f5d988f` / `2f76cda`.

### The member filter red-proofed, with a planted case from outside the enumeration *(task 1.8)*

**AC1's red-proof first.** `methodprobe.py` asks how many candidates are declared as a method on
every page using them. Before P0-1 it answered 110; after:

```text
unresolved candidates                      : 819
declared as a method on EVERY page using it: 0
declared on SOME pages using it            : 0
remaining after the filter                 : 819
```

**0 removable, which is what AC1 names as its red-proof.** And `perpageprobe.py`'s three controls,
all green against the shipped tool:

```text
positive  ConfigureBrighter removed  : True
negative  HostBuilderContext survives : True
negative  CommandProcessor absent    : True
```

**The planted two-way control is the part that is worth more than either.** Both of the above are
built from cases the filter was designed against. Friction 36 asks for a positive case from
**outside** the enumeration, and `MEMBER_DECL_RE` is anchored on a modifier — so the outside case is
a declaration carrying none. Two scratch pages were written into `contents/`, the census run over
each with the filter **off** and **on**, and the pages deleted; they are not committed:

```text
name                  filter OFF    filter ON   verdict
ZzPlantedModifier     True          False       PASS
ZzPlantedBareName     True          True        PASS

census size  filter OFF: 2021   filter ON: 1750
```

`ZzPlantedModifier` is `public void ZzPlantedModifier(int count)` and must **disappear**.
`ZzPlantedBareName` is `void ZzPlantedBareName(int count);` in an interface body and must
**survive** — the filter is anchored not to see it, by design, and a control that proved the blind
spot *closed* would be proving the filter does something it was never built to do.

**The `filter OFF` column is friction 44's check and it is not decoration.** It shows each planted
name was in the corpus to begin with, so each assertion *could* fail. Without it, a filter that had
silently stopped running would give `False` on both — and the first row would read as a pass. The
census sizes corroborate it arithmetically: 2021 = 2019 + 2 with the filter off, 1750 = 1749 + 1
with it on, so exactly one of the two planted names survived.

### Finding — AC1's instrument exits 2 after P0-1, and that is friction 44 from the other direction

`methodprobe.py` reports the 0 that AC1 asks for **and then fails its own positive control**:

```text
controls:
  positive  ConfigureBrighter caught : False
  negative  CommandProcessor absent  : True

CONTROLS FAILED -- the numbers above mean nothing.       (exit 2)
```

**The probe is not wrong and the filter is not wrong.** `ok_pos` is
`'ConfigureBrighter' in fully`, and `fully` is computed over the census — which now removes
`ConfigureBrighter` before the probe ever sees it. The control asserts that the probe's filter
*catches* a name the shipped tool has already filtered out, so it is **unsatisfiable whatever
either does**.

This is friction 44 arriving by a route `design.md` §10.3 did not anticipate. There, the control's subject
had been **repaired out of the corpus** by spec 014. Here, **the tool absorbed the probe's own
filter** — which is what shipping P0-1 *means*. A probe that measures what a change would buy
cannot keep a control that asserts the change has not happened yet.

**Neither probe is being rewritten, and that is a decision rather than an omission.** They are the
evidence base for figures an approved design was signed off on; re-anchoring their controls now
would mean a later re-run no longer reproduces those figures, and the probes are single-use
instruments whose question P0-1 has now answered. What the record owes instead is this paragraph,
addressed to phase 5:

> **Phase 5, task 5.2, reading AC1: `methodprobe.py` exits 2 and prints `CONTROLS FAILED`. That is
> the expected post-P0-1 state and AC1 is met.** The criterion is *"reports 0 removable"*, and it
> does. Check the number, not the exit code, and check it beside `perpageprobe.py`'s three controls,
> which stay satisfiable because its positive is worded as *removed* rather than as *caught*.

**The same absorption shows up in `perpageprobe.py`'s headline table, which now prints three
identical rows** — 819 / 36 / 57 / 65 / 103 / 210 three times over. Its "shipped today (types
only)" row *is* the shipped tool, and the shipped tool now has the member filter in it. The
comparison the probe was built to make is no longer visible from inside it.

### Finding — the drift control inverts after P0-1, and it must be read the new way from here

`design.md` §10.2's control requires the probe's copy with **no extra filter** to reproduce the
shipped tool. After P0-1 that comparison is false by design:

```text
sc.census(pages())[0]                  -> 1749
stage_count(pages(), [])               -> 2019   DRIFT      <- expected from here on
stage_count(pages(), [METHOD_DECL_RE]) -> 1749
len(census_per_page(pages()))          -> 1749   AGREE
```

**From here the faithful pair is `sc.census` against `stage_count(pages(), [METHOD_DECL_RE])`, and
they agree at 1749.** The `[]` path has not broken — it has become the **pre-change baseline**, which
is a useful thing to still have: 2019 → 1749 stays reproducible in one process after the change, so
phases 3 and 5 can re-derive the movement without checking out an older ref.

**Read the old way, this control now says "the copy has drifted" about a copy that has not.** Any
phase re-running §10.2 verbatim gets a red that means the opposite of what it says.

### P1-1: the gap is not members *(task 1.9)*

```bash
python3 spec/015-census_triage/probe/perpageprobe.py --stages
```

```text
types only -- shipped today        : 2019
+ methods                          : 1749   (recovers 270)
+ properties and fields            : 1692   (recovers 327)
design.md 3.2 recorded             : 1211

unexplained after both             : 481  of the 808-name gap
```

**481 of 808 are unexplained by member declarations of any kind.** Methods recover 270, properties
and fields 57 more; neither closes the gap, so neither is a step towards reproducing 1,211.
**Whatever the original probe filtered, it was not members** — which is P1-1's answer, and it
answers the requirements' alternative offer (*"or say the original filter set is unrecoverable and
stop guessing"*) in the affirmative.

**The staged table still reproduces after P0-1**, because its "types only" row is the probe's own
copy of the type-only path rather than a call into the shipped tool. That is the second half of the
drift finding above, stated as a benefit rather than a hazard.

**Do not "correct" 831 or 1,211 in 014's `design.md`.** They are the numbers that design was
approved on; 015's job is to explain them, not to overwrite them.

### The eight gates, reconciled against the prediction *(task 1.10)*

All eight run at `b262975` + the member filter, with `git add -A` before the `--changed` pass so the
strict run had a diff to see. **Figures cited from `tools/README.md`, not pasted** — the column below
says whether each moved, which is the claim obligation 6 asks for.

| # | Gate | Predicted | Measured | Reconciliation |
|---:|---|---|---|---|
| 1 | `linkcheck` | none | **unmoved** at `tools/README.md`'s figure | The `.md`-only argument held: `tools/symbolcheck.py` is not a `.md` file and `spec/` is skipped, so nothing entered the walk |
| 2 | `pagelint` | none, errors and warnings both | **unmoved**, and `--changed` clean | The debt count did not move because no page was touched. `--changed` is 0 errors, which is AC12's shape ahead of the phase that owes it |
| 3 | shape | none | **unmoved** | `SUMMARY.md` untouched |
| 4 | redirects | none | **unmoved** | No URL moved |
| 5 | `versioncheck` | none | **unmoved** | No page pins a version in this diff |
| 6 | `optioncheck` | none | **unmoved** | No option table touched |
| 7 | `--verify` | none | **unmoved** | Published-URL set unchanged |
| 8 | `symbolcheck` | none, gate **and** `--verify-list` | **unmoved**, both | The mechanical prediction held: the pin lives inside `run_census`'s reach only, and `verify_row` still resolves against `PRODUCT_REFS` |

**Eight of eight predicted, eight of eight measured, no movement anywhere.** That is a green worth
one sentence of suspicion and no more: this phase changed a tool that no gate runs — `--census` is
not in `.github/workflows/docs.yml` at all — and edited one file inside `linkcheck`'s `SKIP_DIRS`.
**A prediction of "none" that comes true is only informative because the two named-as-doubtful ones
were re-argued from mechanism rather than from precedent**, and the mechanism is what held.

### Phase 1 as executed — what the list did not predict *(task 1.11)*

Three things, and none of them is a defect in what shipped:

1. **The drift control inverts**, and read the old way it now reports drift in a copy that has not
   drifted. Recorded above with the pair that replaces it.
2. **AC1's instrument exits 2 while meeting AC1**, because the shipped tool absorbed the filter its
   positive control asserts is not yet shipped. Friction 44 from a second direction; the note to
   phase 5 is above.
3. **A control's own harness is an instrument.** `design.md` §10.2's four lines are shorthand and
   returned `2` when run verbatim, `census()` being a two-tuple. Recorded in § *Phase 1 prediction*
   rather than quietly fixed, because the only thing that made it obvious was that `2` is absurd.

**All three are the same shape**, which is why they are grouped rather than listed apart: *an
instrument built to measure a change stops being able to measure it once the change ships.* The
probes, the drift control and `methodprobe`'s positive control are all single-use by construction,
and each says something false at exactly the moment its subject lands. Phase 2 inherits that as a
warning, because P0-3's runner is the next single-use instrument this spec builds.

**One prose repair the list did not plan, and obligation 4 asks for.** `tools/README.md` described
`--census` and `--verify-list` as one pair of commands reading the sibling repositories, which was
true until this phase gave them **different refs**. A paragraph now says which reads the pin, which
follows `origin/master`, and why the asymmetry is the point — the same reasoning the constant
carries in `symbolcheck.py`, stated once in each place it is needed rather than cross-referenced.
**No gate number changed**, so constraint 10 is not engaged; `linkcheck` re-run after the edit,
unmoved at `tools/README.md`'s figure, because an existing `.md` file gained prose and no links.

**What shipped:** `CENSUS_PINS`, `resolve_sha`, a four-row SHA header, and `MEMBER_DECL_RE` applied
per page beside `DECL_RE` with the `strip_noncode` asymmetry commented in place. **929 → 819 at
`09f5d988f` / `2f76cda`**, both halves at one SHA pair, the prediction written first.

---

## Phase 2 as executed

**Branch `spec/015-phase2-method`, 2026-09-19.** Six tasks, no published page touched. Two files:
`triage.md` (D3, the method) and `probe/triagerun.py` (D5, the runner). **Every figure below is
measured at the pin — `../Brighter` `09f5d988f`, `../Darker` `2f76cda`** — and the runner prints
all four refs it resolved before it does anything else, per obligation 9.

### The four query forms, red-proofed *(task 2.3)*

`python3 spec/015-census_triage/probe/triagerun.py --query-forms`. **It is a flag on the runner
rather than a paste**, so the claim stays runnable and a later reader tempted to "simplify" the
bracket class back to `\b` can see the cost in one command:

```text
  form                                           IAmACommandStoreAsync    Date
                                                         MUST STAY > 0
  -S<name>                    substring                              4     118
  -S'\b<name>\b'  --pickaxe-regex                                    0       0  <- BROKEN
  -S'\<<name>\>'  --pickaxe-regex                                    0       0  <- BROKEN
  -S[^A-Za-z0-9_]<name>[…]  --pickaxe-regex                          2       5
```

**All four numbers reproduce `design.md` §4.3 exactly**, at the pin rather than at `origin/master`.
The two broken forms return **0 for the positive control**, which is the whole point: read without
a control that must be non-zero, they say *no candidate has ever existed* about every name in the
census. **Plausible zero number eight.**

The runner asserts only the shipped form. Whether git's regex engine grows `\b` support one day is
not its business; whether the form the method uses can still find a known-removed API is.

### Plausible zero number NINE, found while costing the stages

**The same bracket-class form, parameterised into zsh, returns 0 for every name including the
positive control.** Printed by `--query-forms` beside the table above:

```text
the same form through zsh, with the name in a variable:
  stdout (what a `$(...)` capture reads) : '0'
  stderr (what nobody looks at)          : "zsh:1: bad math expression: operand expected at `^A-Za-z0-9...'"
  argument list, no shell                : 2
```

**`$name[` is an array subscript in zsh**, so `$name[^A-Za-z0-9_]` is parsed as a subscript and
evaluated as arithmetic. The command dies, the error goes to stderr, the count goes to stdout, and
a loop capturing only the count sees a tidy column of zeros. It was found by running exactly that
loop, and it produced `0` for `IAmACommandStoreAsync` — **the positive control, silently**.

**`design.md` §4.3's literal form has no `$` in it and is immune, which is why this survived the
design review.** The trap lives in the *parameterised* form, and the parameterised form is the only
one anybody can run 819 times. `triagerun.py` opens no shell for that reason: every query is a
`subprocess` argument list. This is the third word-boundary-shaped trap and the second one whose
victim was the control rather than the data.

### The controls, printed on every run *(task 2.4)*

```text
  OK   positive IAmACommandStoreAsync    want EXISTED, REMOVED  got EXISTED, REMOVED  [ 0p    0s  s1  4/0  s2 2/0  live   0/0 ]
         brighter history: -        private readonly IAmACommandStoreAsync _commandStore;
  OK   negative IAmAMessageScheduler     want LIVE              got LIVE              [ 0p    0s  s1 10/0  s2 9/0  live 102/0 ]
         brighter history: …c)provider.GetRequiredService<IAmAMessageScheduler>());
  OK   negative OrderId                  want NEVER EXISTED     got NEVER EXISTED     [31p  113s  s1  0/0  s2 0/0  live   0/0 ]
  OK   positive UseExternalInbox         want EXISTED, REMOVED  got EXISTED, REMOVED  [ 0p    0s  s1  2/0  s2 2/0  live   0/0 ]
         brighter history: …ublic static IBrighterBuilder UseExternalInbox(
  can they fail? the 4 controls require 3 DISTINCT verdicts: EXISTED, REMOVED, LIVE, NEVER EXISTED
  OK   plant      IAmACommandStoreAsync    absent from the census…: True
  OK   plant      UseExternalInbox         absent from the census…: True
```

They are **asserted before any row is written** and the run exits 2 if any fails, so a checkpoint
cannot fill up with rows from an instrument nobody checked.

**Friction 44 — can each control pass at all — is checked two ways rather than assumed.** A stuck
classifier would satisfy one control and fail the others, so the four required verdicts must come
out **three distinct values**; and each planted name is verified **absent from the census**, because
a plant that had drifted into the corpus would be testing the corpus rather than the mechanism.
Both checks print their own answer rather than being conditions in a comment.

### The runner reproduces the design's hand-run pilot *(task 2.2)*

Four names overlap between `design.md` §10.5's stage-2 table and a 60-name pilot run here. **All
four agree on both products' commit counts**, which is what says the runner executes the method the
design measured rather than a near relative of it:

| Name | `design.md` §10.5 B/D | `triagerun.py` B/D | |
|---|---:|---:|---|
| `Date` | 5 / 0 | **5 / 0** | agree |
| `AddHours` | 2 / 0 | **2 / 0** | agree |
| `Repository` | 7 / 3 | **7 / 3** | agree |
| `BuildServiceProvider` | 0 / 4 | **0 / 4** | agree |

The design's pilot ran at `origin/master` and this one at the pin. **That is not an independent
check of the pin** — the two refs are one commit apart, and friction 43 applies: a number that
survives a change is not evidence that nothing changed.

### The measured cost, per name and per product *(task 2.6)*

Phase 3's budget derives from these and not from `design.md` §5, which is an inherited number.
Raw per-stage, both products, measured at the pin:

| Stage | Brighter | Darker | Both |
|---|---:|---:|---:|
| 1 — plain `-S` | 0.48s | 0.04s | **0.52s** |
| 2 — bracket class | 5.6s | 0.14s | **5.75s** |
| live — `grep -lwF`, per ref | 0.06s | 0.04s | 0.10s |

**Darker costs a fortieth of Brighter and the design charged it the same.** `design.md` §5 budgets
stage 2 at `99 × 2 × 5.4s ≈ 18 min`; the measured figure is `99 × 5.75s ≈ 9.5 min`.

End to end as the runner actually executes it — including the live query at **all four refs**,
which the design costed at nothing — over a 60-name pilot:

| | Names | Per name |
|---|---:|---:|
| cleared by stage 1 | 47 | **0.66s** |
| went on to stage 2 | 13 | **5.76s** |
| **projected over the census** at §10.5's 720/99 split | 819 | ≈ **17.4 min** |

**The 720/99 split is the design's measurement, not the pilot's**, and phase 3 re-derives it: the
pilot takes the top 60 by page-spread and friction 45 says the head is unrepresentative of the tail
by construction. Only the per-name costs are phase 2's to hand over.

### The eight gates, reconciled *(task 2.5)*

Predicted **none**, and the reason rather than the pass: this phase writes only under `spec/`,
which is in `linkcheck`'s `SKIP_DIRS` (`tools/linkcheck.py:52`), and no other gate walks that
directory at all. `git add -A` ran before the `--changed` pass so the strict run had a diff to see.

| # | Gate | Predicted | Measured |
|---:|---|---|---|
| 1 | `linkcheck` | none | **unmoved** at `tools/README.md`'s figure |
| 2 | `pagelint` | none; `--changed` 0 errors | **unmoved**, `--changed` clean |
| 3 | shape | none | **unmoved** |
| 4 | redirects | none | **unmoved** |
| 5 | `versioncheck` | none | **unmoved** |
| 6 | `optioncheck` | none | **unmoved** |
| 7 | `--verify` | none | **unmoved** |
| 8 | `symbolcheck` | none | **unmoved** |

**Eight predicted, eight measured, nothing moved.** Unlike phase 1 this green needs no suspicion:
phase 1 edited a file inside `linkcheck`'s walk and had to argue from mechanism, while this phase
edits nothing any gate opens. **No figure in `tools/README.md` changed**, so constraint 10 and
obligation 10 are not engaged.

### Phase 2 as executed — what the list did not predict

**1. `git log` and `git grep` disagree about what exit 1 means, and the runner's first run stopped
on it.** `git grep` exits **1 for no matches** — the live query's commonest and most informative
answer — while `git log` exits 0 whether or not the pickaxe matched, so 1 from it is a real
failure. One set of accepted codes for both either swallows a broken `log` or refuses every absent
name, **and an absent name is what this method exists to find**. The check is per command. It is
recorded here rather than quietly fixed because it failed in the safe direction — it refused to
classify anything — and the unsafe version of the same bug reads every failed query as `0` and
calls the whole census NEVER EXISTED.

**2. The pickaxe reads string literals; the census does not.** `strip_noncode()` takes comments and
strings out of the *documentation* side before a token is nominated, and nothing does that on the
*history* side. **20 of `Date`'s 26 whole-word history occurrences carry a quote on the line** —
`[DynamoDBHashKey("Command+Date")]`, `AttributeName = "Topic+Date"`. So `EXISTED, REMOVED` has a
**third** reading beyond the two `design.md` §10.6 names: *this name was once a substring of a
string literal in the product's source*. `triage.md` §1.2 carries it.

**3. `head -1` on the evidence is how a provisional verdict hardens into a wrong reason.**
`design.md` §10.6 read the seven head survivors with `… | head -1`. On `Date` that line is
`Get<T>(DateTime date, …)`, a parameter name, and the table concludes *Own API? no*. The full set
of 26 also holds `-        public DateTime Date { get; set; }` — a **public property of
`DynamoDbMessage`** in `src/Paramore.Brighter.Outbox.DynamoDB/`, removed in 2019.

> **This does not overturn the verdict on `Date`. It overturns the reason**, and under a
> three-valued vocabulary the reason *is* the verdict. `Date` clears because the documentation's
> `Date` is a token of its own example domain — **not** because Brighter never had a public `Date`.
> Brighter did.

The runner's own first draft kept **four** evidence lines and would have repeated the defect at
scale; it now keeps up to 40 distinct lines per name per product, with the `dotted`/`bare` totals
taken over *all* matches beside them so a reader can see whether they have the whole set. **Phase 3
reads the set, never its first line.**

**4. Stage 2.5's dot evidence discriminates — and `Date` shows why it must never be a filter.**
Measured: `AddHours` 2 dotted / 0 bare and `BuildServiceProvider` 8 / 0, both pure calls on somebody
else's type; the plant `IAmACommandStoreAsync` 0 / 4, pure declaration position. The design
predicted exactly that. But `Date` comes out **0 dotted / 26 bare**, which reads as
declaration-shaped, and most of those 26 are inside string literals; and `Repository` comes out
**64 / 64**, precisely ambiguous. **Two of the four names the tell was supposed to help with are
names it would have misled on.** Printed for the person, never applied — as `design.md` §4.4 §3
required before any of this was measured.

**Three candidate frictions for phase 5, which now numbers from 46**: the zsh subscript zero, the
two-commands-two-meanings-of-exit-1 case, and evidence truncation as a source of right-answers-with-
wrong-reasons. Phase 5 decides how they are numbered; all three are recorded in full above.

**What shipped:** `spec/015-census_triage/triage.md` §§1–4 — three verdicts, four stages, the query
forms with both traps, and an **exhaustive** stopping condition — and
`spec/015-census_triage/probe/triagerun.py`, checkpointed per row with `--controls-only`,
`--query-forms`, `--limit`, `--names`, `--restart` and `--report`. **No checkpoint is committed**:
the record over all 819 is phase 3's deliverable, and this phase's pilots were written to scratch
so that phase 3's first run is a real run.

---

## Phase 3 prediction

**Written 2026-09-19 on `spec/015-phase3-triage`, before the run it predicts finished and before any
deliverable in this phase was written.** Obligation 6 for the gates; obligation 1 for everything
else — a figure inherited from `design.md` is named here as *what to re-derive*, never as a result.

### What is predicted, and what is only inherited

| | Design's figure | Status here |
|---|---:|---|
| census candidates | **819** | **re-derived at the run's start: 819**, printed by the runner from `census_candidates()`, which calls the shipped `census()` rather than a copy |
| screened out at stage 1 | 720 | **inherited — phase 3 re-derives it.** `design.md` §10.5 measured it at the pin |
| survivors into stage 2 | 99, as **19 / 12 / 68** by band | **inherited.** Task 3.2's guard: if this is far from 99, stop and reconcile rather than spend the time |
| machine cost | 25 min (design) · **17.4 min** (phase 2, measured) | phase 2's, and it is the one to beat — `design.md` §5 charged Darker Brighter's price |
| names stage 3 confirms as removed product surface | **at least twelve** | a **floor**, not a prediction. 87 of the 99 are unread, and the twelve came from looking only where the shape was obvious |

**The one prediction this phase makes that could genuinely fail is the survivor count**, and it is
the only one with a stopping rule attached to it.

### Tasks 3.1 and 3.2 come out of one run, not two, and that is a departure worth naming

The list plans stage 1 and stage 2 as two passes over the corpus. **`triagerun.py` fuses them per
name** — for each candidate it runs the live query, then stage 1, then stage 2 *only* where stage 1
was non-zero — so one invocation produces both tasks' outputs and the split is recovered from the
checkpoint rather than from two wall clocks:

```bash
python3 spec/015-census_triage/probe/triagerun.py            # both stages, checkpointed per row
```

**Recorded rather than quietly done** (obligation 2). Two things fall out of it and neither is free:

- **The per-stage wall clocks are derived, not measured directly.** Every row carries its own
  `seconds`, so *screened at stage 1* and *went to stage 2* are two populations with two means —
  which is what phase 2's cost table already is. The figures § *Phase 3 as executed* reports are
  those, with the count of rows behind each.
- **Task 3.2's "stop before spending 18 minutes" guard survives, in a different form.** The
  barrier it assumed does not exist, so the guard is the **running survivor rate against the
  checkpoint**: the rows accumulate in page-spread order and the rate is readable at any point.
  The run is resumable, so stopping costs only the rows not yet written. Friction 45 says the head
  over-represents survivors, so an early rate **above** 99/819 is expected and is not the
  divergence the guard is for.

### The eight gates, predicted before the work

Per obligation 6, including every "none" with its reason. Figures are **cited from
`tools/README.md`, never pasted** (obligation 10).

| # | Gate | Predicted | Why |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | Everything this phase writes is under `spec/`, which is in `SKIP_DIRS` (`tools/linkcheck.py:52`), and the walk opens only `.md` files (`tools/linkcheck.py:112`) — so the committed `.jsonl` checkpoint cannot enter it either. The 164 → 165 precedent was a new `.md` **inside `tools/`**; this phase adds no file anywhere in the walk |
| 2 | `pagelint` | **none**, errors and warnings both | Its corpus is `contents/` plus the root `README.md`. This phase edits neither, and writes no C# fence on any page. `--changed` must be clean |
| 3 | shape | **none** | `SUMMARY.md` is untouched; no page is created, nested or moved |
| 4 | redirects | **none** | Redirects follow `SUMMARY.md`, which is untouched |
| 5 | `versioncheck` | **none** | It reads version pins in published pages; this phase writes spec documents |
| 6 | `optioncheck` | **none** | It reflects over marked option tables in pages; none is touched |
| 7 | `--verify` | **none** | The published-URL set is unchanged, for the same reason as shape |
| 8 | `symbolcheck` | **none** — gate **and** `--verify-list` | `symbolwatch.tsv` is untouched in this phase, deliberately: the rows are phase 4's, because a row added while its name is still printed on nine pages turns the gate red on `master` (finding 4). No tool changes, so the census is unmoved too |

**The gate nobody should trust a "none" from here is 8**, and the reason is not that it might move
by accident — it is that a green `symbolcheck` in this phase is *not evidence the triage found
nothing*. The instrument knows only the names already on the watchlist. That is 014's third
sentence — *a green from a single-use instrument is a claim about the instrument* — and it is why
the twelve were invisible to it for a year.

---

## What 015 did not triage

**Task 3.6, AC8. Measured 2026-09-19 at the pinned SHA pair** — `../Brighter` `09f5d988f`,
`../Darker` `2f76cda` — except where a row says otherwise, and every figure carries the command that
produced it rather than a number carried forward from `design.md` §9.

**819 verdicts is a statement about C# fenced blocks, not about the documentation.** A record that
rules on 819 names invites the reading that the site has been swept. It has not been. What follows
is the boundary, stated as numbers so that the next spec starts from a measurement rather than from
this document's silence.

### The four surfaces the census never reaches

Three of these commands read no git history at all, so they reproduce without either sibling
repository:

```bash
python3 - <<'PY'
import os, sys
sys.path.insert(0, 'tools')
import symbolcheck as sc
pages = sorted(os.path.join(b, f) for b, _, fs in os.walk('contents')
               for f in fs if f.endswith('.md'))
cand, counts = sc.census(pages)
print('pages examined                    :', len(pages))
print('...with at least one C# fence     :', counts['fenced'])
print('...with NO C# fence               :', len(pages) - counts['fenced'])
print('distinct tokens in those fences   :', counts['raw'])
print('...after comments and strings     :', counts['stripped'])
print('...after page declarations, noise :', counts['candidates'])
stripped, declared = set(), set()
for rel in pages:
    for body in sc.csharp_blocks(open(rel, encoding='utf-8').read().splitlines()):
        declared.update(sc.DECL_RE.findall(body))
        declared.update(sc.MEMBER_DECL_RE.findall(sc.strip_noncode(body)))
        stripped.update(sc.TOKEN_RE.findall(sc.strip_noncode(body)))
noise = {t for t in stripped if t in sc.NOISE_EXACT or t.startswith(sc.NOISE_PREFIX)}
print('tokens struck by the noise sets   :', len(noise),
      '=', len({t for t in noise if t in sc.NOISE_EXACT}), 'exact +',
      len({t for t in noise if t.startswith(sc.NOISE_PREFIX)}), 'prefix')
print('distinct names declared on a page :', len(declared & stripped))
PY
```

| Not triaged | Measured | What the number means |
|---|---:|---|
| **Pages with no C# fence at all** | **16 of 161** | The census walks `csharp_blocks()` (`tools/symbolcheck.py:499`) and nothing else, so every token on these sixteen pages has never been a candidate in any run of this instrument |
| **Tokens inside comments and string literals** | **484** (2764 → 2280) | `strip_noncode()`. A name in a comment is not a use — and note that the *history* side does no such thing, which is `triage.md` §1.2's third reading |
| **Names a page declares itself** | **583 distinct**, of which the member filter is P0-1's share | `DECL_RE` plus `MEMBER_DECL_RE`, applied **per page**: a name declared on page X is not a candidate on page X and still counts on page Y. So this is not a set-level subtraction and cannot be reported as one |
| **The noise sets** | **68 distinct** — 44 `NOISE_EXACT`, 24 `NOISE_PREFIX` | `System*`, `Microsoft*`, `Task`, `Guid` and the rest, `tools/symbolcheck.py:348-359`. Deliberate, documented, and **never triaged by anything** |
| **Names that resolve** | **930** (1749 candidates → 819 unresolved) | The point of the instrument: they are live at `src/` of one of the two products, at one of the four refs |
| **The prose surface** | **160 of 161 pages**, 1,346 unresolved tokens — **inherited, not re-measured** | 014's D12 conclusion 2, `spec/014-documentation_workflow/tasks.md:213`. The figure is quoted with its source because re-deriving it needs the prose census D12 built and ruled unusable; what *is* re-derived here is the mechanism above it — the census has never opened anything but a C# fence |

**The prose row is the one to read slowly.** It is the largest untriaged surface, it is where twelve
dead APIs also appear — four of the eighteen sites finding 3 counted are in running text — and it is
the row most likely to be misread as covered. **015 swept C# fences. It did not sweep the
documentation.**

### The number `design.md` §9 leads with, re-derived rather than carried

§9's table names **110** page-declared members that P0-1 removes. That figure is about the
*unresolved* list, not the candidate list, and the two are different measurements of the same
filter:

| | Before P0-1 | After | Removed |
|---|---:|---:|---|
| **census candidates** (`...after page declarations, noise`) | 2019 | **1749** | **270** |
| **unresolved candidates** (what `--census` reports) | 929 | **819** | **110** |

Both are re-derived at the pin in § *Phase 1 as executed* — tasks 1.5 and 1.7, two `--census` runs
with byte-identical headers — and the 270 by `perpageprobe.py --stages`. **Quoting one where the
other belongs is how a filter's cost gets misstated by a factor of two and a half**, which is why
both are printed here with the thing each one counts.

---

## Phase 3 as executed

**Written 2026-09-19 on `spec/015-phase3-triage`**, against § *Phase 3 prediction* above.

### The instrument could not read its own corpus, and it found out 597 names in *(task 3.1)*

**Recorded before it was fixed** (obligation 2). The first full run died at **name 597 of 819**,
`HttpRequestException`, with the whole process and not the row:

```text
596/819 rows written, then

  File ".../probe/triagerun.py", line 177, in stage2
    out = git(repo, ['log', '-S', bracket_pattern(name), … '-p', sha, …])
  …
UnicodeDecodeError: 'utf-8' codec can't decode byte 0xa9 in position 18562: invalid start byte
```

**`git log -p` emits the bytes that are in the tree, and one of them is not UTF-8.** Located by
reading the raw stream rather than by guessing:

```text
b'#region Licence\n /* The MIT License (MIT)\n-Copyright \xa9 2014 Ian Cooper <…>'
  src/Paramore.Brighter.MessagingGateway.RESTMS/RestMsMessageConsumer.cs
```

A Latin-1 `©` in a 2014 licence header, in a gateway since deleted. `subprocess.run(text=True)`
decodes as UTF-8 and **raises**, so a corpus containing one such byte anywhere stops the run dead
whenever a name's history happens to reach that commit.

**The fix is `errors='replace'` in `git()`, and it cannot move a verdict** — `TOKEN_RE` nominates
ASCII identifiers and `word_re()` matches ASCII, so every byte the replacement touches is a byte no
query could ever have matched. That argument is a claim about the instrument, so it was run rather
than asserted, **both ways**:

```text
positive  HttpRequestException   fixed reader 3 commits, raw bytes 3 commits   MATCH
          dotted 0, bare 5, 2 distinct evidence lines
negative  DbParameter            row identical to the pre-fix run: True
negative  UseMsSqlOutbox         row identical to the pre-fix run: True
```

The **positive** half counts the same commits a second way — by counting `\x01` record marks in the
undecoded byte stream — so the fixed reader is checked against something that never decodes at all.
The **negative** half re-runs two names the pre-fix run had already written rows for and requires
the new rows to be **identical in every field**: a fix that quietly changed a clean name's evidence
would be caught there rather than at the next person to read the table.

> **Three things about where this landed.** It failed **loudly**, which is the safe direction and
> the opposite of this programme's usual finding — the same shape as phase 2's exit-1 case, where
> the unsafe version reads a broken query as `0`. It landed at **597 of 819**, deep in the one-page
> tail, so **no pilot over the head would have met it**: phase 2's 60-name pilot ran clean, and
> friction 45 says the head is unrepresentative by construction. And the checkpoint is what made it
> cost four minutes instead of thirteen — **596 rows were already fsynced**, and the resumed run
> skipped every one of them.

### The run, and the design's split reproduced to the name *(tasks 3.1 and 3.2)*

```text
python3 spec/015-census_triage/probe/triagerun.py      # 596 rows, 13:21.18, then the crash
python3 spec/015-census_triage/probe/triagerun.py      # 223 rows,  5:19.60, resumed from the checkpoint

census candidates: 819
controls -- four, three distinct verdicts required, both plants absent: all OK

verdicts:
  LIVE                  0
  EXISTED, REMOVED     44
  NEVER EXISTED       775
  total               819
```

**Two runs, 18:41 of wall clock, and the costs land on phase 2's table rather than the design's.**
Derived from the rows' own `seconds`, which is how tasks 3.1 and 3.2 are separable at all when the
runner fuses the stages:

| | Names | Mean | Total |
|---|---:|---:|---:|
| **screened at stage 1** | **720** | **0.65s** | 467.5s |
| **stage-1 survivors** | **99** | **6.11s** | 604.5s |
| …of which stage 2 found a whole identifier | 44 | 6.06s | 266.6s |
| …of which the hits were substrings only | 55 | 6.14s | 337.9s |
| **all** | **819** | **1.31s** | **1072s = 17.9 min** |

**Phase 2 projected 17.4 minutes and measured 17.9.** The design's 25 was the figure that would have
been budgeted from, and it was wrong in the direction that matters least; phase 2's was wrong by
3%.

**The split is the design's, to the name:**

| | `design.md` §10.5 | Measured here | |
|---|---:|---:|---|
| screened out at stage 1 | 720 | **720** | |
| survivors | 99 | **99** | |
| …at ≥3 pages | 19 | **19** | |
| …at 2 pages | 12 | **12** | |
| …at 1 page | 68 | **68** | |

> **This is two methods agreeing, not one method repeated.** The design's figures came from a
> hand-run shell pilot over the whole census; these come from `triagerun.py`, which opens no shell,
> queries the pin through `CENSUS_PINS` rather than `origin/master`, and reads exit codes per
> command. **Different code, same corpus, same five numbers.** That is obligation 1 satisfied by
> construction rather than by re-running the same script twice — and it is the check friction 43
> asks for, because the numbers reproduced here are the ones the *decision* turned on, not the total
> the report leads with.

**The stage-1 asymmetry paid for itself exactly as designed.** 720 names cost 0.65s each and were
never looked at again; the 99 that survived cost nine times as much. Had every name been asked the
honest question, the run would have been **819 × 6.11s ≈ 83 minutes** instead of 18.

### Stage 2.5, and what it would have decided if anyone had let it *(task 3.3)*

The dot evidence is printed beside every survivor and **applied to nothing**. Measured over the 44:

| | Dotted / bare | Reads as | Stage 3 ruled |
|---|---:|---|---|
| `LogCritical` | 18 / 0 | a call on somebody else's type | NOT SURFACE ✓ |
| `DeserializeObject` | 48 / 0 | the same | NOT SURFACE ✓ |
| `UseMsSqlOutbox` | 0 / 2 | declaration position | SURFACE ✓ |
| `Repository` | 64 / 64 | **precisely ambiguous** | NOT SURFACE, on the lines |
| `Date` | 0 / 26 | declaration position | **NOT SURFACE** — 20 of the 26 are string literals |
| `UseScoped` | 2 / 6 | mixed | SURFACE, on the lines |

**It is a good tell and it is not a verdict.** Two of the six above would have been decided wrongly
by the counts alone, and both failures are in the dangerous direction: `Date` reads
*declaration-shaped* because a string literal has no dot in front of it either. The tell is a
property of the **character before the match**, and three different things share that property —
a declaration, a string literal, and a bare identifier in an initialiser.

### Stage 3: seventeen, where twelve was the floor *(task 3.4)*

**The full rulings are `spec/015-census_triage/stage3.tsv`** — one row per survivor, each with its
quoted evidence line and a control measured by the same query at the pin. `triage.md` §5.2 carries
the set and §5.3 the evidence. The counts:

```bash
python3 spec/015-census_triage/probe/triagetable.py --check
#   needing a stage-3 ruling : 44
#     ruled SURFACE          : 17
#     ruled NOT SURFACE      : 27
#     ruled INSTRUMENT       :  0
```

**Ten pages, twenty-eight sites — 23 fenced across 15 distinct blocks, 5 in prose** — re-derived by
two agreeing greps in `triage.md` §5.2, against finding 3's nine pages and eighteen sites for the
twelve.

**`triagetable.py` is a new check, so it owes a red-proof, and it is two-way in both directions**
(obligation 3). The record is refused when the machine's output and the person's disagree about
which names need a ruling — either way round:

```text
$ triagetable.py --check --rulings <a copy with UseMsSqlOutbox deleted>
the record would misstate the run: 1 survivor(s) have no stage-3 ruling, so the
triage is unfinished: UseMsSqlOutbox                                        exit 2

$ triagetable.py --check --rulings <a copy with ZzNotASurvivor added>
the record would misstate the run: 1 ruling(s) name something that is not a
survivor of this run: ZzNotASurvivor                                        exit 2

$ triagetable.py --check                                                    exit 0
```

**The positive case is planted from outside the enumeration** (friction 36): `ZzNotASurvivor` is
not a census candidate, not a survivor and not a name in either product, so it tests the check
rather than the corpus. And the **green** third line is the half that shows the first two are
findings rather than a checker that refuses everything.

**Zero `LIVE`, and that is a measurement rather than a quiet pass.** A `LIVE` verdict would have
been a defect in the census — a name it called unresolved that resolves — and the run was capable of
returning one: the control `IAmAMessageScheduler` classifies `LIVE` at 102 live files in the same
run, immediately before the 819 rows are written.

### Phase 3 as executed — what the list did not predict

**1. The instrument could not read its own corpus.** Above, in full: a Latin-1 `©` from 2014, at
name 597 of 819, fixed with `errors='replace'` and controlled both ways.

**2. Twelve was a floor and it rose to seventeen — and the five extra ones arrived in families.**
`S3LuggageStoreCreation` and `StoreCreation` sit beside `AddS3LuggageStore`; `BeginOrGetTransactionAsync`
sits beside `BeginOrGetTransaction`; `UnitOfWork` sits beside `IUnitOfWork`. **Four of the five are
the neighbour of a name the design already had** — the option enum next to its registration method,
the async twin next to the sync one, the class next to its interface.

> **A dead API keeps company, because APIs are removed in families and pages document them in
> families.** Every one of the five is a **one-page** name, so no page-spread ordering would have
> surfaced them; what would have is asking *what else was in that commit*. Phase 4 should read the
> **block**, not the name.

**3. The fifth is `ApplicationJson`, and it is the one worth quoting at any green gate.**
`MessageMappers.md` prints Brighter's constructor signature with a default the product removed —
and **at line 132 the same page already uses the replacement**, `MediaTypeNames.Application.Octet`,
sixteen lines after the last of its five `ApplicationJson` sites. A page contradicting itself, on a name no watchlist carried, in
a block nobody compiles. It is on no list the design produced because the design never looked at
that page.

**4. The documentation reproduced the product's own typo, and that is evidence about how it rots.**
`UseMySqTransactionConnectionProvider` — `MySq`, not `MySql` — is spelled that way in Brighter's
removed source *and* on `DapperOutbox.md`. The page was copied from the source when the source was
right, which is exactly why it is wrong now: **a page that was accurate by transcription decays the
moment the thing it transcribed moves**, and nothing in this repository was watching.

**5. The survivor count is not the interesting count, and neither is the census total.** 819
candidates → 99 survivors → 44 with a whole-identifier history → **17 confirmed**. The design read
**7 of 7** head survivors and found **zero** product surface; the tail's 44 hold seventeen. The
funnel's last step is the only one a machine cannot take, and it is where every real finding was.

### The eight gates, reconciled *(task 3.7)*

Predicted **none**, eight of eight, with `git add -A` run before the `--changed` pass so the strict
run had a diff to see.

| # | Gate | Predicted | Measured |
|---:|---|---|---|
| 1 | `linkcheck` | none | **unmoved** at `tools/README.md`'s figure |
| 2 | `pagelint` | none, errors and warnings both | **unmoved**; `--changed` **0 errors** |
| 3 | shape | none | **unmoved** |
| 4 | redirects | none | **unmoved** |
| 5 | `versioncheck` | none | **unmoved** |
| 6 | `optioncheck` | none | **unmoved** |
| 7 | `--verify` | none | **unmoved** |
| 8 | `symbolcheck` | none, gate and `--verify-list` | **unmoved**, and `--verify-list` reports all 5 entries still dead with every replacement live |

**No figure in `tools/README.md` changed**, so constraint 6 and obligation 10 are not engaged. The
warning count did not move either: this phase edits no page.

> **And the green from gate 8 is the one to distrust, exactly as predicted.** `symbolcheck` reports
> *no watchlisted symbols found — 5 entries, 161 pages, 1 silenced* while **seventeen dead APIs are
> printed on ten of those pages**, because it has never been told their names. That is not a defect
> in the gate; it is the gate's scope, and it is what phase 4's rows are for. **A green from an
> instrument that has never been given the corpus is a claim about the instrument.**

---

## Phase 4 prediction

**Written 2026-09-19 on `spec/015-phase4-repairs`, before any row was added and before any page was
touched.** Obligation 6 for the gates; obligation 1 for the scope. **This is the only phase whose
prediction includes a gate going RED**, and the red is a deliverable rather than an accident.

### The repair scope, re-derived from phase 3's rulings *(task 4.1)*

**Not inherited from finding 3, and not inherited from the head of any file.** The input is
`stage3.tsv`'s `SURFACE` rows — the person's output, not the machine's — and every count below was
produced twice by two different mechanisms:

```bash
awk -F'\t' '$2=="SURFACE"{print $1}' spec/015-census_triage/stage3.tsv   # 17 names

# pages, method 1 -- per name, -w, unioned
while read n; do grep -rlw "$n" contents/; done < names | sort -u | wc -l          # 10
# pages, method 2 -- one alternation
grep -rlE "\b($(paste -sd'|' names))\b" contents/ | sort -u | wc -l                # 10
# sites, third mechanism -- every match, not every matching line
grep -rhoE "\b($(paste -sd'|' names))\b" contents/ | wc -l                         # 28
```

The fenced/prose split needs a fence parser, so it too was taken twice: once with a hand-rolled
toggle over ```` ``` ```` lines, and once through **`pagelint.Page`** — the shipped parser, which
handles tilde fences, indented fences and marker lengths the toggle does not. **Both return 23
fenced in 15 blocks and 5 in prose.**

| Page | Sites | Fenced | Blocks | Prose |
|---|---:|---:|---:|---:|
| `MessageMappers.md` | 5 | 5 | **4** | 0 |
| `S3LuggageStore.md` | 5 | 3 | 1 | **2** |
| `DapperOutbox.md` | 4 | 4 | 2 | 0 |
| `SweeperCircuitBreaking.md` | 4 | 1 | 1 | **3** |
| `DynamoOutbox.md` | 3 | 3 | 2 | 0 |
| `BrighterBasicConfiguration.md` | 2 | 2 | 1 | 0 |
| `ShowMeTheCode.md` | 2 | 2 | 1 | 0 |
| `DispatchingARequest.md` | 1 | 1 | 1 | 0 |
| `FeatureSwitches.md` | 1 | 1 | 1 | 0 |
| `UsingSweeperCircuitBreaking.md` | 1 | 1 | 1 | 0 |
| **10 pages** | **28** | **23** | **15** | **5** |

| Name | Sites | Where |
|---|---:|---|
| `ApplicationJson` | 5 | `MessageMappers.md` ×5, in **four** blocks |
| `UseMsSqlOutbox` | 3 | `SweeperCircuitBreaking.md` (1 fenced, 1 prose), `UsingSweeperCircuitBreaking.md` |
| `UseMySqlOutbox` · `UseDynamoDbOutbox` | 2 each | one fenced site and one prose site each |
| `AddS3LuggageStore` · `StoreCreation` | 2 each | `S3LuggageStore.md`, one fenced and one prose each |
| `CommandProcessorLifetime` | 2 | `BrighterBasicConfiguration.md`, `ShowMeTheCode.md` |
| the other ten | 1 each | `UseInMemoryOutbox` `UseDynamoDbTransactionConnectionProvider` `UseMySqTransactionConnectionProvider` `S3LuggageStoreCreation` `UseScoped` `NoTaskQueues` `IUnitOfWork` `UnitOfWork` `BeginOrGetTransaction` `BeginOrGetTransactionAsync` |

**Finding 3's *nine pages, eighteen sites, ten blocks, four prose* was the design's twelve names and
is now history.** The five names phase 3 added carry one page each, so they add no page of their
own — except `ApplicationJson`, which brings `MessageMappers.md` in whole: **a tenth page, five
sites and four of the fifteen blocks, from a single name no watchlist carried.**

**The task list's per-task page allocation is out of date by one page and by five names**, and the
tasks are executed against this table rather than against their own *Notes*:

| Task | Planned pages | Actually |
|---|---|---|
| 4.4 | `ShowMeTheCode.md`, `BrighterBasicConfiguration.md` | unchanged — 4 sites, 2 blocks |
| 4.5 | the outbox and sweeper family, five pages | unchanged — **plus `MessageMappers.md`**, which belongs to no planned task |
| 4.6 | `DispatchingARequest.md`, `FeatureSwitches.md` | unchanged — 2 sites |
| 4.7 | **four** prose sites | **five** — `S3LuggageStore.md:43`'s `StoreCreation` is the new one |

### The eight gates, predicted before the work

Per obligation 6. Figures are cited from `tools/README.md`, never pasted (obligation 10).

| # | Gate | Predicted | Why |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | Ten pages are edited and none is created, moved or renamed, so the file count is fixed; the repairs rewrite C# and prose, not links. A repair that adds a link would move nothing either — the gate counts files and broken targets |
| 2 | `pagelint` | **0 errors**, and the **warning count may FALL** | Rule 6 is the only rule in play: an edited C# block enters strict scope under `--changed` and owes its `using` directives, so a block that had none and gains them takes the repo-wide warning count **down**. Task 4.9 names the blocks that moved it. Errors must stay at zero, `--changed` included |
| 3 | shape | **none** | `SUMMARY.md` is untouched; no page is created, nested or moved |
| 4 | redirects | **none** | Redirects follow `SUMMARY.md` |
| 5 | `versioncheck` | **none** | It reads NuGet and GitHub version pins. None of the ten pages is among the five it examines, and no pin is written |
| 6 | `optioncheck` | **none**, and it is a **real** check here | Two of the ten pages carry marked option tables — `DynamoOutbox.md:105` and `SweeperCircuitBreaking.md:84` — so this gate has something to see for the first time in three phases. The repairs are outside both tables |
| 7 | `--verify` | **none** | The published-URL set follows `SUMMARY.md` |
| 8 | `symbolcheck` | **RED, then GREEN — and the entry count MOVES** | The rows take it from 5 entries to **22**. With the rows added and the pages unrepaired it must report **28 sites across 10 pages** (task 4.3's red run); with the repairs in it must report **0 findings, N silenced**, N re-derived from however many prose sites take an opt-out (task 4.7). `--verify-list` must stay at 0 findings with all 22 rows dead at both refs of their product |

**Two of these predictions can fail informatively and the rest cannot.** Gate 8's red is a
**prediction about the instrument**: if it sees fewer than 28 sites, the difference is a site the
gate cannot see and that is a finding about `symbolcheck`, not a rounding error. Gate 2's falling
warning count is a **prediction about the corpus**: if it does not fall, the repaired blocks did not
gain their directives, and rule 6 was satisfied by a `// ...` somebody wrote to make a warning go
away.

---

## Phase 4 as executed

**Written 2026-09-19 on `spec/015-phase4-repairs`**, against § *Phase 4 prediction* above. The
sections are in the order the branch ran them: rows, red, repairs, green.

### The rows, and what the replacement column says *(task 4.2)*

**Seventeen rows, all `brighter`, all evidenced `015 triage §5.2` and listed `2026-09-19`.** Every
one was confirmed dead **four ways** before it was written down, and the fourth is new to this
phase:

| Instrument | What it asked | Result |
|---|---|---|
| the census (phase 1) | does the name resolve at `src/` of either product, at either ref? | unresolved — that is why it is a candidate at all |
| `triagerun.py` (phase 3) | does a whole identifier of this name appear in history? | 17 of 44 ruled `SURFACE` by a person, each with a quoted removal line |
| `symbolcheck.verify_row` | `git grep -lw` at **10.7.0** and **`origin/master`** | **17 of 17 `DEAD`, 0 files at both refs** |
| **reflection over the released assemblies** | does the name exist in the **DLLs a reader installs**? | **11 of 11 probed are absent** from `Paramore.Brighter` 10.7.0 and its twelve companion packages |

**The fourth is the one that answers the question the other three only approximate.** The census and
the triage both read `src/*.cs`; `--verify-list` reads `src/*.cs`; a name can be in `src/` and not be
in the package a reader installs, and a name can be gone from `src/` for a reason that has nothing to
do with the published surface. Loading the released `Paramore.*.dll` files and asking
`GetExportedTypes()` is the only one of the four that asks about **what NuGet ships**. It agreed with
the other three on every name it could reach.

**The replacement column follows one rule, stated here because it decides whether `--verify-list`
has anything to check:** the column names **the live member a repair actually writes** when one
identifier is enough, and otherwise says what happened in parentheses. `symbolcheck.py:726` skips a
replacement beginning `(`, so a prose replacement is **never re-resolved** — the precedent is
`UseExternalInbox`'s *(removed at V10 — not a rename)*.

| Replacement | Rows | Live at both refs |
|---|---|---|
| `TransactionProvider` | `UseDynamoDbTransactionConnectionProvider`, `UseMySqTransactionConnectionProvider` | 2 files |
| `IAmATransactionConnectionProvider` | `IUnitOfWork`, `UnitOfWork` | 2 files |
| `GetTransaction` · `GetTransactionAsync` | `BeginOrGetTransaction`, `BeginOrGetTransactionAsync` | 22 · 17 files |
| `UseExternalLuggageStore` · `StorageStrategy` · `Strategy` | `AddS3LuggageStore`, `S3LuggageStoreCreation`, `StoreCreation` | 2 · 11 · 9 files |
| `NoExternalBus` | `NoTaskQueues` | 2 files |
| **prose, in parentheses — not verified** | the four `Use{DB}Outbox` rows, `CommandProcessorLifetime`, `UseScoped`, `ApplicationJson` | — |

**Six of the seventeen carry a prose replacement and that is a cost, not a preference.** Those six
buy nothing from `--verify-list` for ever. They are prose because the answer is a *shape* rather than
a name: you no longer call a method, you set a property inside `AddProducers`, and a column holding
`Outbox` would pass the check while telling a writer almost nothing. Where one identifier **is** the
answer — `NoTaskQueues` → `NoExternalBus` — the identifier is what the row carries, and eleven rows
do.

### The red run — the gate firing on every site, before any repair *(task 4.3)*

**Obligation 2, and the only red-proof these rows will ever get.** Run with the seventeen rows
committed and **no page touched**:

```bash
python3 tools/symbolcheck.py    # exit 1
```

```text
===== UseMsSqlOutbox — 3 site(s) across 2 page(s) =====
    (removed at V10 — set Outbox on AddProducers, with MsSqlOutbox)   [015 triage §5.2, listed 2026-09-19]
contents/SweeperCircuitBreaking.md:230  - **MS SQL Server** (`UseMsSqlOutbox`)
contents/SweeperCircuitBreaking.md:311  .UseMsSqlOutbox(/* outbox config */);
contents/UsingSweeperCircuitBreaking.md:39  .UseMsSqlOutbox(/* outbox configuration */)
===== UseMySqlOutbox — 2 site(s) across 2 page(s) =====
contents/DapperOutbox.md:47  .UseMySqlOutbox(new MySqlConfiguration(DbConnectionString(), _outBoxTableName), typeof(MySqlConnect…
contents/SweeperCircuitBreaking.md:232  - **MySQL** (`UseMySqlOutbox`)
===== UseDynamoDbOutbox — 2 site(s) across 2 page(s) =====
contents/DynamoOutbox.md:41  .UseDynamoDbOutbox(ServiceLifetime.Singleton)
contents/SweeperCircuitBreaking.md:234  - **DynamoDB** (`UseDynamoDbOutbox`)
===== UseInMemoryOutbox — 1 site(s) across 1 page(s) =====
contents/ShowMeTheCode.md:205  .UseInMemoryOutbox() // Simple outbox for development
===== UseDynamoDbTransactionConnectionProvider — 1 site(s) across 1 page(s) =====
contents/DynamoOutbox.md:42  .UseDynamoDbTransactionConnectionProvider(typeof(DynamoDbUnitOfWork), ServiceLifetime.Scoped)
===== UseMySqTransactionConnectionProvider — 1 site(s) across 1 page(s) =====
contents/DapperOutbox.md:48  .UseMySqTransactionConnectionProvider(typeof(Paramore.Brighter.MySql.Dapper.UnitOfWork), ServiceLif…
===== AddS3LuggageStore — 2 site(s) across 1 page(s) =====
contents/S3LuggageStore.md:29  serviceCollection.AddS3LuggageStore((options) =>
contents/S3LuggageStore.md:38  You configure an **S3LuggageStore** using the **S3LuggateOptions** provided to the callback in **Ad…
===== S3LuggageStoreCreation — 1 site(s) across 1 page(s) =====
contents/S3LuggageStore.md:34  options.StoreCreation = S3LuggageStoreCreation.CreateIfMissing;
===== StoreCreation — 2 site(s) across 1 page(s) =====
contents/S3LuggageStore.md:34  options.StoreCreation = S3LuggageStoreCreation.CreateIfMissing;
contents/S3LuggageStore.md:43  * **StoreCreation**: What should we do when determining if there is a bucket for the store?
===== CommandProcessorLifetime — 2 site(s) across 2 page(s) =====
contents/BrighterBasicConfiguration.md:222  options.CommandProcessorLifetime = ServiceLifetime.Scoped;
contents/ShowMeTheCode.md:203  options.CommandProcessorLifetime = ServiceLifetime.Scoped;
===== UseScoped — 1 site(s) across 1 page(s) =====
contents/BrighterBasicConfiguration.md:219  options.UseScoped = true;
===== NoTaskQueues — 1 site(s) across 1 page(s) =====
contents/FeatureSwitches.md:163  .NoTaskQueues()
===== IUnitOfWork — 1 site(s) across 1 page(s) =====
contents/DispatchingARequest.md:153  private readonly IUnitOfWork _uow;
===== UnitOfWork — 1 site(s) across 1 page(s) =====
contents/DapperOutbox.md:48  .UseMySqTransactionConnectionProvider(typeof(Paramore.Brighter.MySql.Dapper.UnitOfWork), ServiceLif…
===== BeginOrGetTransaction — 1 site(s) across 1 page(s) =====
contents/DynamoOutbox.md:64  var transaction = _unitOfWork.BeginOrGetTransaction();
===== BeginOrGetTransactionAsync — 1 site(s) across 1 page(s) =====
contents/DapperOutbox.md:68  var tx = await _uow.BeginOrGetTransactionAsync(cancellationToken);
===== ApplicationJson — 5 site(s) across 1 page(s) =====
contents/MessageMappers.md:41  var body = new MessageBody(payload, ApplicationJson, CharacterEncoding.UTF8);
contents/MessageMappers.md:94  public MessageBody(string body, string contentType = ApplicationJson, CharacterEncoding characterEn…
contents/MessageMappers.md:104  var body = new MessageBody(payload, ApplicationJson, CharacterEncoding.UTF8);
contents/MessageMappers.md:112  public MessageBody(byte[] bytes, string contentType = ApplicationJson, CharacterEncoding characterE…
contents/MessageMappers.md:116  public MessageBody(in ReadOnlyMemory<byte> body, string contentType = ApplicationJson, CharacterEnc…

----- silenced by opt-out (1 site(s)) -----
contents/DispatcherConfigurationReference.md  UseExternalInbox ×1

28 site(s) across 10 page(s), from 22 watchlist entries over 161 pages.
```

*(The per-row `To keep one of these on purpose…` line the gate prints after every group is elided
here, and only there: it is the same sentence seventeen times. Everything else is verbatim.)*

**28 sites across 10 pages, which is task 4.1's figure to the site.** That equality is the
prediction that could have failed: task 4.1 counted with two greps and a fence parser, and the gate
counts by its own `word_pattern()` over whole lines. **A gate seeing fewer sites than the corpus
holds would have been a finding about the instrument**; it sees all 28.

**One row's two sites are one line, and it is worth naming.** `contents/S3LuggageStore.md:34` carries
`options.StoreCreation = S3LuggageStoreCreation.CreateIfMissing;` — `StoreCreation` and
`S3LuggageStoreCreation` are **different rows**, and `word_pattern()`'s lookbehind is what stops the
shorter one matching inside the longer. `DapperOutbox.md:48` is the same shape, `UnitOfWork` inside
`Paramore.Brighter.MySql.Dapper.UnitOfWork`, where the `.` before it is what makes the match right
rather than wrong.

**And the silenced line is still there and still says 1.** *0 findings* and *0 findings, 1 silenced*
are different claims; so are *28 sites* and *28 sites, 1 silenced*. The green run at task 4.9 is the
other half of this control.

### The repairs, page by page *(tasks 4.4 to 4.7)*

**All 28 sites are gone or deliberately kept**, and the split is 26 repaired to 2 opted out. Every
replacement was established against the **released assemblies** before it was written — see the
build evidence below — and no page's banner, page type or opening sentence was touched.

| Page | Sites | What the repair was |
|---|---:|---|
| `BrighterBasicConfiguration.md` | 2 | `UseScoped` and `CommandProcessorLifetime` deleted from the `AddConsumers` options: neither exists on `BrighterOptions` at 10.7.0 and neither has a successor property |
| `ShowMeTheCode.md` | 2 | same two lines, plus `UseInMemoryOutbox()` → `configure.Outbox = new InMemoryOutbox(TimeProvider.System)` inside `AddProducers` |
| `DapperOutbox.md` | 4 | `UseMySqlOutbox`/`UseMySqTransactionConnectionProvider` → `Outbox`, `ConnectionProvider`, `TransactionProvider` on `AddProducers`; `UnitOfWork` → `IAmATransactionConnectionProvider`; `BeginOrGetTransactionAsync` → `GetTransactionAsync` |
| `DynamoOutbox.md` | 3 | the same shape, with `DynamoDbUnitOfWork` as **both** providers; `BeginOrGetTransaction` → `GetTransactionAsync` |
| `MessageMappers.md` | 5 | `ApplicationJson` → `new ContentType(MediaTypeNames.Application.Json)`, and the three printed constructor signatures replaced with the ones the product ships |
| `S3LuggageStore.md` | 5 | `AddS3LuggageStore` → `UseExternalLuggageStore`; `StoreCreation`/`S3LuggageStoreCreation` → `Strategy`/`StorageStrategy`. **Two prose sites kept** behind opt-outs |
| `SweeperCircuitBreaking.md` | 4 | the fenced `UseMsSqlOutbox` → `AddProducers`; the three-name bulleted list rewritten to the live Outbox **types** |
| `DispatchingARequest.md` | 1 | `IUnitOfWork` → `IAmATransactionConnectionProvider`, with `GetTransactionAsync` and provider-level commit/rollback |
| `FeatureSwitches.md` | 1 | `NoTaskQueues()` → `NoExternalBus()`, inside a fluent chain that had moved underneath it |
| `UsingSweeperCircuitBreaking.md` | 1 | `UseMsSqlOutbox` → `AddProducers`, and the sweeper options moved onto `UseOutboxSweeper` |

**The two kept sites are ruling 6 working as intended** (task 4.7). `S3LuggageStore.md` now carries
the `DispatcherConfigurationReference.md` pattern — two `<!-- symbolcheck: allow … -->` comments
above a visible *Coming from V9?* blockquote naming `AddS3LuggageStore` and `S3LuggageStoreCreation`
as removed — so a reader arriving from a V9 sample lands on the page that tells them so. The gate
now prints **0 findings, 3 silenced**, where it printed **0 findings, 1 silenced** before this
phase: *0 findings* alone would have been a different and weaker claim.

**Three prose sites were rewritten rather than silenced**, and the choice is the same ruling read
the other way. `SweeperCircuitBreaking.md`'s bulleted list was not *about* the removed names — it
claimed they were how you configure each store — so it now names `MsSqlOutbox`, `PostgreSqlOutbox`,
`MySqlOutbox`, `SqliteOutbox`, `DynamoDbOutbox` and `MongoDbOutbox`, each confirmed LIVE at both
refs. **Three of those six bullets named dead methods that are not on any watchlist** —
`UsePostgreSqlOutbox`, `UseSqliteOutbox` and `UseMongoDbOutbox`, all `0 at 10.7.0, 0 at
origin/master` — and they were repaired because they were beside a site the gate did name.

### Every edited block built against the released packages *(task 4.8)*

**Three scratch projects, one per package family, and no `ProjectReference` into `../Brighter/src`
anywhere.** The AWS SDK forces the split: `Paramore.Brighter.DynamoDb` pulls `AWSSDK.Core` 3.x and
`Paramore.Brighter.Transformers.AWS.V4` pulls 4.x, and NuGet refuses the pair (`NU1107`). A reader
installing one page's packages never hits it; a harness compiling every page at once does.

| Project | Packages, all at **10.7.0** unless noted | Blocks | Result |
|---|---|---:|---|
| `core` | `Paramore.Brighter`, `.Extensions.DependencyInjection`, `.ServiceActivator` ×3, `.MessagingGateway.RMQ.Async`, `.Outbox.Hosting`, `.MySql`, `.Dapper`, `.Outbox.MySql`, `.Inbox.MySql`, `.MsSql`, `.Outbox.MsSql`, `Dapper` 2.1.35 | 10 | **0 errors, 4 warnings** |
| `dynamo` | `Paramore.Brighter`, `.Extensions.DependencyInjection`, `.DynamoDb`, `.Outbox.DynamoDB`, `.Outbox.Hosting` | 2 | **0 errors, 0 warnings** |
| `s3` | `Paramore.Brighter`, `.Extensions.DependencyInjection`, `.Transformers.AWS.V4` | 1 | **0 errors, 0 warnings** |

**The block that gets compiled IS the block on the page.** `extract.py` reads the page through
`pagelint.Page` — the shipped parser, not a second one — copies the fenced body **byte for byte**,
and adds only a wrapper: a class for a bare method, a method for bare statements, nothing for a
block that is already a compilation unit. Identifiers a page names in a declared omission
(`producerRegistry`, `outboxConfiguration`, `credentials`, `dynamoDb`, `_handlerFactory`,
`_registry`, and the `Person`/`Greeting`/`AddGreeting`/`GreetingMade` domain) come from a scaffold
file that **is not part of any block** and is listed here so the boundary is visible.

**The harness was red-proofed before any result from it was believed.** One character changed in a
repaired block — `configure.Outbox` to `configure.OutboxThatDoesNotExist` — and the build failed
`CS1061` at that line; changed back, 0 errors. A compile harness that has never failed is the same
uninformative instrument as a gate that has never been red.

**The four warnings are named rather than suppressed.** Three are `CS0649` on
`DispatchingARequest.md`'s fields, which the block declares and never assigns because it is an
excerpt of a class with no constructor. The fourth is **`CS0618`: `BrighterOptions.PolicyRegistry`
is obsolete — *Migrate to ResiliencePipeline***, on `BrighterBasicConfiguration.md`. That line is
**live and therefore out of this spec's scope**: 015 rules on names the product *removed*, and a
name the product deprecated is a different finding with a different owner. It is recorded here so
the next spec does not have to find it twice.

**Two blocks are quotations of a signature and cannot compile by construction** —
`MessageMappers.md`'s printed `MessageBody` constructors, which are fragments of a class the page
does not own. They were checked the only way that means anything: **against the released
declaration, by reflection**, with `NullabilityInfoContext` so the `?` annotations are read rather
than assumed.

```text
public MessageBody(String? body, ContentType? contentType = null, CharacterEncoding characterEncoding = UTF8)
public MessageBody(Byte[]? bytes, ContentType? contentType = null, CharacterEncoding characterEncoding = UTF8)
public MessageBody(in ReadOnlyMemory`1 body, ContentType? contentType = null, CharacterEncoding characterEncoding = UTF8)
```

Both blocks now print exactly that, and both declare their elided body with `// ...`, which
`pagelint` rule 6 downgrades to a counted warning rather than silence.

### What compiling found that no watchlist could have

**Four defects, none of them a watchlisted name, all four fatal to a reader who copies the block.**
This is the AC12 argument made concrete: the census finds a name that no longer resolves; only a
compiler finds a name that resolves to something else.

| Found | Page | What it was |
|---|---|---|
| **`S3Region.EUW1` does not exist in AWS SDK v4** | `S3LuggageStore.md` | The page **recommends** the V4 package and then prints a v3-only enum member. `AWSSDK.S3` 3.7.500.5 has both `EUW1` and `EUWest1`; 4.0.101.5 has only `EUWest1`. Repaired to `EUWest1` |
| **`IAmAMessageMapper<T>` has a third member and a second parameter** | `MessageMappers.md` | V10's interface is `Context { get; set; }` plus `MapToMessage(TRequest, Publication)`. The page's mapper implemented neither, so the corpus's canonical mapper example **did not implement the interface it claimed** |
| **The fluent builder moved underneath `NoTaskQueues`** | `FeatureSwitches.md` | `With()` → `StartNew()`, `DefaultPolicy()` → `DefaultResilience()`, and `Build()` is now unreachable without `NoInstrumentation()` and `RequestSchedulerFactory(…)`. The page also passed an undeclared `fluentConfig` to `ConfigureFeatureSwitches` while building a registry it never used |
| **`Paramore.Brighter.{DB}.Dapper` has no V10 release at all** | `DapperOutbox.md` | `Paramore.Brighter.MySql.Dapper` and `.MsSql.Dapper` both stop at **9.9.13** on NuGet; `.Sqlite.Dapper` reaches only a `10.0.0-preview.6`. The page's *packages you need* list was unbuyable, and **`Paramore.Brighter.Dapper` 10.7.0 exists but carries only `DbConnectionStringProvider`** |

**The fourth is the one to read twice.** A package list is prose, it contains no identifier the
census tokenises, and it is the **first** thing a reader acts on. Nothing in this repository was
watching it, and nothing in this repository is watching it now.

### The eight gates, reconciled *(task 4.9)*

Measured at **`3be2a78`**, with `git add -A` before the `--changed` pass.

| # | Gate | Predicted | Measured |
|---:|---|---|---|
| 1 | `linkcheck` | none | **unmoved** — 165 files, 0 broken |
| 2 | `pagelint` | 0 errors; warnings may **fall** | **0 errors**, `--changed` included; warnings **757 → 744** |
| 3 | shape | none | **unmoved** |
| 4 | redirects | none | **unmoved** |
| 5 | `versioncheck` | none | **unmoved** |
| 6 | `optioncheck` | none, and a real check | **unmoved** — 0 mismatches across 59 tables, 519 rows, with `DynamoOutbox.md` and `SweeperCircuitBreaking.md` both in its corpus |
| 7 | `--verify` | none | **unmoved** — 161 predicted = 161 published |
| 8 | `symbolcheck` | **red, then green**; entries 5 → 22 | **28 sites red, then 0 findings, 3 silenced, 22 entries**; `--verify-list` green with all 22 DEAD and every named replacement LIVE |

**Both moving figures moved for the reason predicted, and both are named in `tools/README.md` and
nowhere else** (obligation 10). The **13** blocks that took `pagelint`'s warning count down are the
fifteen edited blocks minus the two `MessageBody` signature quotations, which declared their
omission with `// ...` and stayed counted. **Predicting the direction of that fall is what makes it
evidence**: a debt figure that drops without an explanation stops meaning anything, and an
unexplained fall of exactly the wrong size would have meant somebody had written `// ...` to make a
warning go away.

### Phase 4 as executed — what the list did not predict

Five, and the first is the one that changed how the phase was worked.

**1. A repair is not a deletion, because an edited block owes a compile.** The list reads as though
each site were a line to remove. It is not: removing `options.UseScoped = true;` leaves a block
that was **already broken in four other ways** — a missing parenthesis, a missing comma, four
undeclared variables, and `configure.TransactionProvider = transactionProvider` hiding the fact
that the property takes a **`Type`**. AC12 says an edited block compiles, so the block around the
site is in scope whether or not the list says so. **Every one of the fifteen edited blocks needed
more than its site removed**, and four of them needed a defect fixed that no census could see.

**2. Six of seventeen replacement columns buy nothing from `--verify-list`, for a stated reason.**
`symbolcheck.py:726` skips a replacement that begins `(`, so prose replacements are never
re-resolved. The four `Use{DB}Outbox` rows, `CommandProcessorLifetime` and `UseScoped` all carry
prose, because the answer is a *shape* — set a property inside `AddProducers` — and a column
holding `Outbox` would pass the check while telling a writer nothing. **Eleven rows do carry a
resolvable name**, and those eleven are what keeps `--verify-list` a real check rather than a
formality.

**3. The template a census cannot see: `Use{DB}Outbox` in prose.** Three pages carried the
sentence *"we configure Brighter to use an outbox with the Use{DB}Outbox method call"*.
`Use{DB}Outbox` is not an identifier, tokenises as nothing, and appears in **no** census candidate
list; `symbolcheck` will never fire on it. Two of the three were in this phase's scope and are
repaired. **`EFCoreOutbox.md` is the third and still carries both sentences** — measured after the
repairs, `grep -rn 'Use{DB}' contents/` returns **2, all on that page** — and it is the sharpest
form of the finding: **its code block was already repaired to the V10 API by somebody, and the
prose above the block was not.** It is out of 015's scope by task 4.1's list and is left for the
closing sentence to name.

**4. A heading is a published URL, so the prose under it moved and the heading did not.**
`DapperOutbox.md`'s `## Brighter Unit of Work without Dapper` now sits above a paragraph about the
transaction provider, because renaming it would move an anchor GitBook has published. `grep`
confirms nothing in `contents/`, `SUMMARY.md` or `.gitbook.yaml` links to it — the risk is an
external link, which is exactly the one this repository cannot see. Recorded rather than tidied.

**5. `S3Region.EUW1` is a class of defect this programme has not met before.** It is not a Brighter
name, not stale, and not wrong in the version it was written for: it is a **dependency's** enum
member, removed in AWS SDK v4, on a page that recommends the v4 package. Neither the census nor any
watchlist can reach it, because the name belongs to neither product. **Only compiling against the
packages the page tells a reader to install finds it**, which is the argument for AC12 stated as a
case rather than as a principle.

---

## Phase 5 prediction

**Written 2026-09-19 on `spec/015-phase5-acceptance`, in its own commit before any criterion was
walked and before any gate was run.** Obligation 6 for the gates; obligation 1 for everything else.
An acceptance phase that predicts nothing has no way to be surprised, and the two criteria this
programme has ever found unmet at a close were both found by a person reading, not by a gate.

### What this phase expects to find, and what would count as a surprise

| | Expected | What a miss would mean |
|---|---|---|
| **AC4, AC6, AC10 — the unmarked three** | **at least one finding**, on the precedent that 014's five acceptance repairs all came out of its two uninstrumented criteria | a clean walk of three unmarked criteria is the outcome to distrust, not the one to celebrate |
| **The eight instrumented criteria** | all eight green, **and uninformative** — each one was green when its phase shipped | a red here is a claim that a phase's own reconciliation was wrong, which is worth stopping for |
| **AC10 specifically** | the likeliest failure of the three. 015 quotes more numbers than 014 did, across five documents, and three of them were approved before the work that moved the figures | — |
| **The backwards check (5.3)** | **ten pages, `tools/symbolcheck.py`, `tools/symbolwatch.tsv`, `tools/README.md`, `spec/015-census_triage/`** and nothing else | a file outside that set is either scope creep or an undocumented dependency |

### The eight gates, predicted before the work

Per obligation 6, including every "none" with its reason. Figures are **cited from
`tools/README.md`, never pasted** (obligation 10).

| # | Gate | Predicted | Why |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | Everything this phase writes is under `spec/`, which is in `SKIP_DIRS` (`tools/linkcheck.py:52`). No file is added anywhere in the walk, and `tools/` — the directory whose precedent moved it 164 → 165 — gains nothing |
| 2 | `pagelint` | **none**, errors and warnings both | Its corpus is `contents/` plus the root `README.md`. This phase edits no page and writes no C# fence. The 744 warnings stand where phase 4 left them; `tools/README.md` owns the figure |
| 3 | shape | **none** | `SUMMARY.md` is untouched |
| 4 | redirects | **none** | Redirects follow `SUMMARY.md`, which is untouched |
| 5 | `versioncheck` | **none** | It reads version pins in published pages |
| 6 | `optioncheck` | **none** | It reflects over marked option tables in pages; none is touched |
| 7 | `--verify` | **none** | The published-URL set is unchanged, for the same reason as shape |
| 8 | `symbolcheck` | **none** — gate **and** `--verify-list` | `symbolwatch.tsv` and `tools/symbolcheck.py` are both untouched in this phase. It stays at *0 findings, 3 silenced, 22 entries* — and **the three silenced are part of the claim**, not a footnote |

**The "none" to distrust here is 2, and not for the usual reason.** `pagelint` cannot move on a
phase that edits no page; what it *can* do is be quoted wrongly. Phase 4 took its warning count
down, so this is the first phase in 015 that inherits a figure which moved during the spec, and a
walk that reproduces it from memory rather than from `tools/README.md` would be reporting a number
that was true a phase ago. That is the failure AC10 is pointed at, arriving through the gate table.

---

## Acceptance walk

**Walked 2026-09-19 on `spec/015-phase5-acceptance`.** The three unmarked criteria are below, first
and on their own, before any instrument was run — that ordering is task 5.1's whole point, and it is
the ordering that found something.

### AC4, AC6 and AC10 — the three with no instrument *(task 5.1)*

**Read by:** the author of this phase, against `triage.md` end to end, `stage3.tsv`'s 65 lines,
`requirements.md` § *Acceptance criteria*, and a scripted sweep of every numeric claim outside a
fenced block in all five documents 015 ships.

---

**AC4 — *the triage method is written down and names its stopping condition*. MET, and the
condition is the stronger of the two available.**

`triage.md` §4 states it in a blockquote before it argues for it: *"the method stops when
every name in the census has a verdict"* — exhaustion, not a budget and not a run-length rule.
Three things make it a real stopping condition rather than a sentence satisfying a criterion:

- **It names what it rejected and why.** *"Stop after 20 consecutive NEVER EXISTED"* was drafted and
  refused, on a measured property of this corpus: page-spread ordering ranks the documentation's own
  invented domain highest and dead product surface lowest, so **any run-length rule stops before the
  part of the list most likely to hold a real name**. Phase 3 then proved the point — the head
  contains no Brighter API and the 1–2 page tail holds all seventeen.
- **It bounds itself.** §4.1 states what sits *outside* "every name" — page-declared members, the
  noise sets, prose, and anything under `TOKEN_RE`'s four-character floor — so the condition
  inherits the census's boundaries rather than implying it swept more than it did.
- **It was executed to exhaustion and the record proves it, not the prose.** 819 rows against 819
  candidates, checked by a generator that refuses to emit when the two disagree.

**What a reader should distrust:** nothing found. The one thing worth saying is that AC4 was always
the likeliest of the three to pass, because it asks for a sentence to exist and the sentence is the
deliverable's own §4.

---

**AC6 — *each verdict cites evidence and a control*. MET, and the controls were re-measured rather
than read.**

This is the criterion the requirements name as the one that fails quietly, so reading it was not
enough. The walk did three things:

**1. Counted, at the structural level.** 44 stage-3 entries in `triage.md` §5.3, 44 `**Ruling:**`
lines, 44 `**Control:**` lines — no entry is missing either half:

```bash
grep -c '^#### ' spec/015-census_triage/triage.md        # 44
grep -c '^\*\*Ruling:\*\*'  spec/015-census_triage/triage.md   # 44
grep -c '^\*\*Control:\*\*' spec/015-census_triage/triage.md   # 44
```

**2. Read all 44 control lines for whether they discriminate.** They are of three kinds, and none of
them is an adjective: *the plant, read in the same run* (a declaration against a call — `Date`,
`AddHours`, `DbParameter`); *a named sibling measured live* (`HandlerLifetime` against
`CommandProcessorLifetime`, `UseOutboxSweeper` against `UseMsSqlOutbox`, `S3LuggageOptions` against
`S3LuggageStoreCreation`); and *the honest admission that the counts do not discriminate*, on
`Repository` at 64 dotted / 64 bare, where the ruling says so and rests on the lines instead. **The
third kind is the one that shows the criterion was taken seriously**: a control that reports its own
inability to decide is worth more than one that quietly agrees.

**3. Re-measured seven of the control figures at the pin, and re-checked all seventeen `SURFACE`
names against all four refs.** The controls are claims about the world and this walk does not
inherit them:

```bash
for n in IAmABoxTransactionProvider UseOutboxSweeper MediaTypeNames ContentType \
         S3LuggageOptions HandlerLifetime NoExternalBus; do
  git -C ../Brighter grep -lwF -- "$n" 09f5d988f -- 'src/*.cs' | wc -l   # pin
  git -C ../Brighter grep -lwF -- "$n" c1b8af886 -- 'src/*.cs' | wc -l   # 10.7.0
done
```

| Control name | Cited in the rulings | Measured at the pin | At `10.7.0` |
|---|---:|---:|---:|
| `IAmABoxTransactionProvider` | 26 | **26** | 26 |
| `MediaTypeNames` | 27 | **27** | 27 |
| `ContentType` | 67 | **67** | 67 |
| `S3LuggageOptions` | 4 | **4** | 4 |
| `HandlerLifetime` | 2 | **2** | 2 |
| `NoExternalBus` | 2 | **2** | 2 |
| `UseOutboxSweeper` | 1 | **1** | 1 |

**Seven of seven reproduce**, and the negative half reproduces too: all **17** `SURFACE` names
return **0 files at every one of the four refs** — `../Brighter` `09f5d988f` and `c1b8af886`,
`../Darker` `2f76cda` and `ddb71ee`. A control that is live where the ruled-dead name is absent, in
the same query at the same ref, is a control that could have failed.

**The judgement AC6 turns on, stated rather than buried:** the 775 rows that never reached a person
carry **their own counts as evidence** and share **the run's four controls** rather than each
carrying one of their own. `triage.md` §5.1 argues for that and the walk agrees with it — every one
of those rows was produced by one invocation of one instrument, so a per-row control would be the
same instrument tested 819 times. What makes it honest is that the four controls demand **three
distinct verdicts** between them, so a classifier stuck on any single value fails them, and both
plants are checked absent from the census on every run.

---

**AC10 — *no count in any document 015 ships is unanchored*. MET AFTER TWO REPAIRS, and both were
found here rather than by any gate.**

**How it was walked.** Every line outside a fenced block carrying a two-or-more digit figure, in all
five documents, extracted and read — 509 lines:

```bash
python3 - <<'PY'   # per file: lines with a bare figure, fences excluded
import re
for f in ['requirements.md','design.md','tasks.md','triage.md','README.md']:
    inf=False; n=0
    for l in open('spec/015-census_triage/'+f):
        if l.startswith('```'): inf=not inf; continue
        if not inf and re.search(r'(?<![\w.#-])\d{2,}(?![\w.-])', l): n+=1
    print(f, n)
PY
# requirements.md 104 · design.md 78 · tasks.md 239 · triage.md 36 · README.md 52
```

**Finding 1 — the fifth survival of the abolished slice, in `design.md` §5.** *"P0-4's deliverable
is `triage.md`: one row per name in the slice — **103 rows**"*, two lines below *"AC5 requires a row
for all 819"*. Q2 was reversed at that design's own review and §2, §5's boxes and §9 were amended;
this sentence was not. **Repaired in place with the original struck through and the reason recorded**
— which is the treatment `requirements.md` got for its four, and the reason the count is worth
stating as *four plus one* rather than as five: the tasks review found four and thought it had
finished.

**Finding 2 — `757` is a gate figure that moved during this spec, quoted bare in nine places.** It
is `pagelint`'s using-directive warning count; phase 4 took it to 744, `tools/README.md` records
both with their refs, and obligation 10 forbids either number appearing anywhere else. Nine
occurrences across five documents, none carrying a ref or a command:

| Where | What it is | Ruling |
|---|---|---|
| `triage.md`:72 | a **present-tense claim in a shipped deliverable** — *"the 757 using-directive blocks sit on"* that boundary | **Repaired.** Now cites `tools/README.md` and names the figure as one that fell during this spec |
| `requirements.md`:289, 293 · `design.md`:741 | a **dated prediction** — *"expect the repo-wide 757 to fall by a small number and say which blocks moved it"* | **Left.** The prediction came true and phase 4 named the thirteen blocks. Rewriting a prediction after the fact destroys the only evidence that it was made |
| `tasks.md`:634, 645 · `requirements.md`:193 · `design.md`:711 · `README.md`:137 | the **approved plan and its arguments**, all written before phase 4 ran | **Left.** They are dated statements of what was planned and what was true then; amending them would make the task list disagree with the list that was approved |

**The rule this walk applies, and it is the one worth carrying forward:** a figure in a *prediction*
or in an *approved plan* is anchored by its date and must not be updated; a figure in a **present-
tense claim in a deliverable** must carry its ref or cite the file that owns it. Only one of the
nine was the second kind, and it was the only one repaired.

**What AC10 could not check, and nobody should read it as having checked.** The sweep finds figures
that lack an anchor. It cannot find a figure that carries an anchor and is wrong — `triage.md`
§5.2's line references into `MessageMappers.md` (sites at 41, 94, 104, 112, 116; the replacement at
132) were true when phase 3 wrote them and are stale now that phase 4 has repaired the page. They
are **anchored** — §5 opens *"Written 2026-09-19, phase 3"* — and left, because a record of what was
found is supposed to describe the world it was found in.
