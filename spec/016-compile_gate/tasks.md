# Spec 016: A Committed Compile Gate — Tasks

**Created:** 2026-09-20
**Status:** **CLOSED — 39 of 39, 2026-09-25.** Awaiting the maintainer's reading of § *Acceptance walk* for `.accepted`. Tasks reviewed 2026-09-20, six findings, amended
**Requirements:** approved 2026-09-20 · **Design:** approved 2026-09-20

**Five phases, 39 tasks, one pull request per phase.** The list was 37 at the review; the two
inserted tasks are numbered `1.6a` and `4.3a` so that no later task is renumbered.
§ *What the tasks review found* maps each finding to its task.

Phases merge under `tools/README.md` § *One phase is one pull request*, including its rule that
**a gate and the corpus that satisfies it merge together, or the gate merges second**. That rule
puts the CI job in phase 3.

---

## 1. Standing obligations — stated once, binding every task

They are not repeated per task.

The programme's seven:

1. **Re-derive any count before quoting it** — command beside the figure, **two methods that
   agree**, and re-derive the number the **decision** turns on rather than the one the document
   leads with. A figure inherited from `requirements.md`, `design.md` or an earlier phase is stale.
2. **Record the mismatch before fixing it**, so the record shows the corpus was wrong.
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof
   with its output recorded here, and **every control is two-way** — the positive case goes
   **outside** the enumeration the instrument was built from, **and it must be able to pass**.
4. **Prose and permission ship together**, read in both directions.
5. **Cite `CLAUDE.md` and `tools/README.md`; never restate them.** When a phase edits
   `CLAUDE.md`, grep the commands for the claim it changed.
6. **Predict gate movement before the work, including "none"** — with the mechanism, then reconcile.
7. **Ask before merging anything that changes the published site**, and ask for the head-ref
   deletion **by name** in the same breath.

016 adds five, each from a defect this spec met:

8. **The unit of compilation is one block.** Never a batch, never a shared project. Friction 54:
   two blocks reporting 20 and 8 errors in a batch of two reported **zero** in a batch of five.
9. **A corpus is enumerated through `pagelint.Page`, and its size is re-derived by a second
   method.** Friction 53: the obvious grep sees 835 of 985.
10. **A gate number changes in `tools/README.md` and nowhere else.**
11. **No criterion and no check reads an exit code through a pipe, and none is satisfiable by a
    missing path.** `$?` after a pipe is the last stage's; `grep … | wc -l` prints `0` whether the
    corpus is clean or the path is absent. Both occurred in this spec's own criteria.
12. **Run the committed form, from a clean directory.** Friction 56: the probe's documented recipe
    had never been executed and would have reported all 985 blocks broken.

---

## 2. The phases

| Phase | Goal | Tasks | Published site | PR |
|---:|---|---:|---|---|
| **1** | **The instrument** — extractor, Roslyn compiler, reference pin, and five red-proofs. **Not yet a gate** | 11 | untouched | one |
| **2** | **The corpus run** — P0-6's distribution over all 985, the parse triage, and the claim/context split P0-9's boundary needs | 6 | untouched | one |
| **3** | **The baseline and the gate** — scaffold, opt-out, ratchet, `tools/README.md` row 9 **and** the CI job, together | 9 | untouched | one |
| **4** | **The repairs** — P0-9, defects of claim | 7 | **CHANGED — needs sign-off** | one |
| **5** | **Acceptance** — the walk, the backwards check, both ledgers, the close | 6 | untouched | one |

**The order is not Research → Core → Supporting → Polish.** The corpus cannot be measured before the
instrument exists (phase 1 → 2), and the gate must not merge before the corpus that satisfies it
(`tools/README.md`'s rule 3), so phase 3 carries both the baseline and the job. Phase 4 is separate
because it is the only phase that changes the published site, and its sign-off gets its own PR.

---

## Phase 1 — The instrument *(11 tasks, one PR, no page touched)*

**Goal:** `python3 tools/blockcheck.py` gives every C# block a verdict, from committed files, with
no CI job and no baseline. **It is a tool at the end of this phase, not a gate.**

- [x] **Task 1.1:** Write the enumerator and extractor in `tools/blockcheck.py`
  - Input: `tools/pagelint.py` (`FENCE_RE`, `class Page`, `load_pages`), `spec/016-compile_gate/probe/gen.py`, `spec/016-compile_gate/harness/extract.py`
  - Output: `tools/blockcheck.py` with a `--list` mode printing one line per C# block as `page<TAB>ordinal<TAB>shape`, and **no absolute path anywhere** — `grep -c '/Users/' tools/blockcheck.py` → 0
  - Notes: the probe and the harness both hardcode a path to `tools/`; that is acceptable in a probe and is the first thing a tool must not do.

- [x] **Task 1.2:** Implement the four wrapper rules and re-derive their counts
  - Input: `design.md` § *The four wrapper rules*
  - Output: the classifier in `tools/blockcheck.py`, and a table in this file recording `namespaced / types / members / statements` **measured twice** — once by `blockcheck.py --list`, once by an independent script — with both commands shown
  - Notes: the design's figures are 11 / 306 / 136 / 532 summing to 985. They are the *design's*; obligation 1 says measure, not inherit.

- [x] **Task 1.3:** Write the Roslyn compiler tool
  - Input: `spec/016-compile_gate/probe/roslyn/Program.cs`, `tools/optioncheck/Program.cs` for the committed-tool shape
  - Output: `tools/blockcheck/blockcheck.csproj` and `tools/blockcheck/Program.cs`, compiling **one `CSharpCompilation` per block** and emitting `id<TAB>verdict<TAB>error count<TAB>distinct codes`
  - Notes: obligation 8 is the whole design here. A single `Compilation` holding many blocks is the defect, not an optimisation.

- [x] **Task 1.4:** Write the reference project — the pin, in one place
  - Input: `tools/optioncheck/optioncheck.csproj`, `spec/016-compile_gate/probe/refs.csproj`
  - Output: `tools/blockcheck/refs/refs.csproj` — the package set with an explicit version on every entry, `CopyLocalLockFileAssemblies=true`, **no sources**, and a comment saying the pin lives here and nowhere else
  - Notes: a project that compiles blocks cannot also assemble references — its build fails and leaves `bin/` empty. That is friction 56, and this task exists because of it.

- [x] **Task 1.5:** Implement the scaffold mechanism and its listing
  - Input: `spec/016-compile_gate/harness/preludes/`, `harness/core/blocks/Scaffold.cs`, `design.md` § *The scaffold*
  - Output: `tools/blockcheck/scaffold/` holding at least the harness's four preludes, and `--list-scaffold` printing every identifier supplied from outside a page, with a count
  - Notes: AC8. A `CLEAN` verdict that does not say what it was given is not a verdict.

- [x] **Task 1.6:** Implement the exit-code contract and the report modes
  - Input: `tools/README.md` § *Exit codes — one contract, all of them*
  - Output: `--report` writing to a file and returning **0 / 1 / 2** read bare; the run's last line printing `N findings` or `N findings, M skipped`; **and the conditions that must exit 2 named in the source** — the reference set unrestored, the scaffold directory absent, `pagelint` unimportable
  - Notes: obligation 11. The contract is unreadable downstream of a `|`, so the tool must not require one. **2 is the code this tool is most likely to need and least likely to emit:** with `bin/` empty every one of the 985 blocks fails, which is a believable red in the direction that confirms the thesis — friction 56, as a runtime state rather than a documentation defect. A gate with no references has *not checked* the corpus.

- [x] **Task 1.6a:** Red-proof exit 2 — the unchecked state, with its control *(review finding 4)*
  - Input: task 1.6, `tools/README.md` § *Exit codes — one contract, all of them*
  - Output: two recorded runs, both codes read **bare** — the reference set deliberately unrestored → **exit 2** with a message naming what was missing and **no per-block verdicts printed**; the same command with references present → **0 or 1**
  - Notes: obligation 3, applied to the code no other task produces. Without this, exit 2 is a promise in a task description, and the failure it guards is the one that looks most like a successful measurement.

- [x] **Task 1.7:** Red-proof the extraction — byte-identity, two-way
  - Input: task 1.1's output
  - Output: `--verify-extraction` reporting **N of N identical** where N is task 1.2's corpus count; **and** a recorded run where one staged block has a byte appended and the check reports exactly that block
  - Notes: the green half is the half that feels unnecessary and the half that catches a check which has stopped running.

- [x] **Task 1.8:** Red-proof the compile verdict — planted pair, outside the corpus
  - Input: `design.md` § *Its controls*
  - Output: recorded output showing a planted `NoSuchTypeXyz123` block → `FAIL` with `CS0246`, and a planted trivial class → `CLEAN`; **neither planted file is a block from `contents/`**. **A third plant, and it is the one that guards obligation 8:** a deliberately unparseable block in the **same run** as the `CS0246` plant, with the run still reporting `CS0246` on the second — recorded, and re-run whenever the compilation structure is touched
  - Notes: obligation 3, and *(review finding 5)* for the third plant. The first two cannot detect the defect this design exists to prevent: friction 54's mechanism is **a parse failure suppressing binding across a shared compilation**, so a regression to batching would leave a `CS0246` plant and a clean plant both reported and the instrument looking sensitive. **Obligation 8 is load-bearing and, without this plant, is the one rule here that no check has ever been able to fail.**

- [x] **Task 1.9:** Red-proof the no-vacuous-pass rule
  - Input: `design.md`'s cost table; session 75's finding that a 0.54s build reporting 0 warnings is MSBuild declining to work
  - Output: two consecutive runs with nothing touched, recorded, reporting the **same** block count and the same verdict distribution; plus the wall-clock of each. **And the second direction:** one further run over an input changed on purpose — a single staged block broken — which must report a **different** distribution, the change then reverted and `git diff` shown empty
  - Notes: AC5, and *(review finding 6)* for the second direction. **Two identical runs are also exactly what a tool that cached, or that silently did nothing the second time, would print** — which is the same shape as session 75's 0.54s build reporting 0 warnings. Sameness is only evidence once difference has been shown to be reachable. **Do not quote a single wall-clock as the figure** — four probe runs over identical inputs gave 6.5s, 22.9s, 39.5s and 26.2s.

- [x] **Task 1.10:** Predict and reconcile the eight gates
  - Input: `tools/README.md` § *The eight gates*, `design.md` § *Predicted gate movement*
  - Output: a § *Phase 1 prediction* and § *Phase 1 as executed* in this file, each gate with its predicted movement, its mechanism, and what it actually did
  - Notes: the prediction is **none** for all eight and the mechanism matters — `tools/` **is** inside `linkcheck`'s walk, so adding any `.md` there moves it. This phase deliberately adds none.

---

## Phase 2 — The corpus run *(6 tasks, one PR, no page touched)*

**Goal:** P0-6. The distribution nobody has, and the evidence P0-9's boundary and phase 3's baseline
both depend on.

- [x] **Task 2.1:** Run the instrument over all 985 blocks and publish the distribution
  - Input: phase 1's tool, run from a clean checkout per obligation 12
  - Output: § *The corpus run* in this file — `BUILT / FAILED / SKIPPED / NOT COMPILABLE` counts summing to the corpus count, the command beside them, and `tools/blockcheck/verdicts.tsv` committed
  - Notes: the design measured 60 clean / 925 failing with **no scaffold**. With phase 1's scaffold the clean count should rise; **if it does not, that is a finding about the scaffold, not a number to adopt.**

- [x] **Task 2.2:** Triage the blocks that fail to parse
  - Input: task 2.1's output, filtered to `CS1002`/`CS1513`/`CS1519`/`CS8635` and friends
  - Output: a table splitting them into *before/after pair in one fence*, *genuine fragment the wrapper mis-shaped*, and *the page is wrong* — with a count per class and a named example each
  - Notes: the design counted **131** unparsed and explicitly did not split them. A wrapper defect found here is a phase-1 bug to fix in this PR, not a page defect.

- [x] **Task 2.3:** Split the failures into defects of claim and defects of context
  - Input: task 2.1's verdicts, `requirements.md` P0-9's scope line
  - Output: two lists in this file — the **claim** list, which is P0-9's input, and the **context** count, which is backlog item 2's; plus the command that produces each
  - Notes: this is the boundary the maintainer's ruling did not set and the assistant proposed. **If the claim list is large, say so plainly** — it is the number that decides whether phase 4 is a phase or a spec.

- [x] **Task 2.4:** Rule Q3 by measurement — does Darker join?
  - Input: the 6 `Paramore.Darker*` strings, `requirements.md` Q3
  - Output: a recorded restore of the reference project **with** Darker's packages added, its exit code read bare, and a one-line ruling in this file — **and, if the ruling is *yes*, the sentence saying which phase carries the work**: the pin is phase 1's file, so a *yes* edits it from phase 2, and Darker's blocks then enter phase 3's baseline like any others
  - Notes: two of the six strings are namespaces, not packages. Resolve which is which before adding anything. P2-2 is the requirements' out-of-scope entry for Darker; a *yes* here promotes it, and a promotion with no task is how scope arrives unowned.

- [x] **Task 2.5:** Confirm the before/after-in-one-fence set, and look for more of the shape
  - Input: `design.md` § *What the repair phase will actually repair*
  - Output: the confirmed list with page and ordinal per block, **re-derived by two methods** — the `CS0101` verdicts and a comment-marker grep covering **both vocabularies**, `// Before` / `// After` *and* `// V9` / `// V10` — plus, explicitly, **the blocks the second method finds that the first does not AND the candidates that reading strikes out**
  - Notes: the design says six blocks across four pages, of which exactly one raises `CS0101`. The other five collide with nothing and are invisible to the compiler — **so the grep is not a corroboration of the compiler here, it is the only instrument that sees five of the six.** *(Review finding 2:)* **the marker method at the review's HEAD returns nine candidates across five pages, and reading subtracts four of them** — see § *What the tasks review found*. This list is therefore expected to *shrink* as well as grow, and a candidate struck out is recorded with the reason, not deleted.

- [x] **Task 2.6:** Predict and reconcile the eight gates
  - Input: as task 1.10
  - Output: § *Phase 2 prediction* and § *Phase 2 as executed*
  - Notes: prediction is **none**; this phase commits a `.tsv` and edits this file.

---

## Phase 3 — The baseline and the gate *(9 tasks, one PR, no page touched)*

**Goal:** P0-4, P0-5 and P0-8. The gate and the corpus that satisfies it merge **together**, per
`tools/README.md`'s rule 3.

- [x] **Task 3.1:** Rule Q5 and implement the opt-out
  - Input: `tools/pagelint.py`'s `allow-serviceactivator` comment, `tools/symbolcheck.py`'s `allow <name>` comment
  - Output: the marker implemented in `tools/blockcheck.py`, **requiring a reason**, and a recorded run showing a skip printed with its reason
  - Notes: ruling 4 — an opt-out is never silent, and `0 findings` and `0 findings, 8 skipped` are different claims.

- [x] **Task 3.2:** Mark the ❌ V9 blocks as skipped
  - Input: the blocks behind a ❌ marker — the design counted **8 across 3 pages**; re-derive
  - Output: the markers in place, and the run reporting the skip count
  - Notes: `CLAUDE.md` § *Version markers on code* **requires** these blocks to exist. A gate demanding they compile would demand the documentation stop showing what it must show.

- [x] **Task 3.3:** Write the scaffold files the initial baseline needs
  - Input: task 2.1's failures in the context class, grouped by page
  - Output: one prelude per admitted page under `tools/blockcheck/scaffold/`, each listed by `--list-scaffold`
  - Notes: **a prelude supplies identifiers, never behaviour, and never a type the page tells the reader to write.** AC13 is a maintainer reading this boundary.

- [x] **Task 3.4:** Define and populate `baseline.tsv`
  - Input: tasks 2.1 and 3.3
  - Output: `tools/blockcheck/baseline.tsv` — page, ordinal, scaffold, and the ref it was admitted at — holding **only blocks that compile without any page being edited**, plus the count in this file
  - Notes: keeping page edits out of this phase is what makes phase 3 site-neutral. Blocks needing a `using` on the page belong to phase 4 or to backlog item 2.

- [x] **Task 3.5:** Implement the ratchet, enforced in both directions
  - Input: AC9
  - Output: exit 1 when a baselined block stops being `CLEAN`, **and** exit 1 when a baselined row names a block that no longer exists
  - Notes: without the second, deleting a page silently shrinks the gate's corpus and the gate still says `0 findings`.

- [x] **Task 3.6:** Red-proof the ratchet, both directions, with its green control
  - Input: task 3.5
  - Output: three recorded runs — a baselined block broken on purpose → exit 1 with the compiler's code; a phantom row added → exit 1; and **the unmodified state → exit 0**. Every edit reverted, verified by `git diff` → empty
  - Notes: the third run is the control that the first two were not passing for some other reason.

- [x] **Task 3.7:** Add row 9 to `tools/README.md`
  - Input: `tools/README.md` § *The eight gates*, § *What each gate actually checks*
  - Output: the row — command, corpus, expected figure, **and the ref it was measured at** — plus the heading and any prose that says "eight" updated to nine
  - Notes: obligation 10. **And grep the whole repository for "eight gates"** — the count lives in prose in more than one place, and a header that disagrees with its own body is this programme's most-repeated defect.

- [x] **Task 3.8:** Add the CI job
  - Input: `.github/workflows/docs.yml`'s `options` job and its comments
  - Output: the new job — `setup-dotnet`, restore the pinned refs, run the gate — with **no guard, no `|| true`, and no `schedule:`**, each choice carrying the comment that says why
  - Notes: Q8 and Q9 are ruled by this task's choices; record both rulings in one line each.

- [x] **Task 3.9:** Predict and reconcile the eight — now nine — gates
  - Input: as task 1.10
  - Output: § *Phase 3 prediction* and § *Phase 3 as executed*
  - Notes: **`linkcheck` is predicted to MOVE if and only if this phase adds a `.md` under `tools/`.** The design chose not to. If the reconciliation shows 166, the cause is a file somebody added without noticing the rule.

---

## Phase 4 — The repairs *(7 tasks, one PR, CHANGES THE PUBLISHED SITE)*

**Goal:** P0-9. **Obligation 7 binds on this PR** — sign-off before merge, head-ref deletion asked
for by name in the same breath.

- [x] **Task 4.1:** Re-derive the claim list before repairing anything
  - Input: task 2.3's claim list
  - Output: the list re-derived at this phase's HEAD, with the command, and any difference from phase 2's explained rather than adopted
  - Notes: phase 2's list was measured at a ref two merges back. 015's phase 4 found three inherited figures stale before it began.

- [x] **Task 4.2:** Repair the before/after-in-one-fence blocks
  - Input: task 2.5's confirmed list, `CLAUDE.md` § *Version markers on code*
  - Output: each such block split into two fenced blocks — **and the labels chosen by what the pair actually is.** A **version** pair (`CloudEventsSupport.md`, `PolicyRetryAndCircuitBreaker.md`: `// V9` above `// V10` in one fence) takes ❌ for the superseded form and ✅ for the current one. A pair that is **not** about versions takes an ordinary labelled heading or bold lead-in and **no ❌/✅ marker at all**
  - Notes: the repair is a convention this repository already has, so this is not an invention — but *(review finding 1)* **the convention is `CLAUDE.md` § *Version markers on code*, which opens *"Where V9 and V10 differ"*, and one block on the list is not that.** `ImplementAQueryHandler.md` block 10 is `// Before (synchronous)` / `// After (asynchronous)` — **Reactor and Proactor, both current in V10.** Marking `Execute` ❌ *superseded* would assert something false on the page and contradict `ReactorAndProactor.md`; label those two *Synchronous* and *Asynchronous* instead. **Do not change any page's banner, type, headings or opening sentence** — a heading is a published URL.

- [x] **Task 4.3:** Repair the remaining defects of claim
  - Input: task 4.1's list
  - Output: the edits, page by page, each one named in this file with what it claimed and what is true
  - Notes: obligation 2 — record the mismatch before fixing it. **Every edited block is built before the commit**, and a repair that needs more than its defect fixed says so (015's phase 4 found fifteen blocks broken in ways the site was not about).

- [x] **Task 4.3a:** Give every block this phase creates or leaves uncompilable its opt-out, with a reason *(review finding 3)*
  - Input: tasks 4.2 and 4.3, task 3.1's marker, task 3.2's rule
  - Output: a skip marker with a reason on **every ❌ block this phase creates** — the split makes one per version pair, and P0-5's rule covers *any* block that must not compile, not only the eight that existed at phase 3 — **and** on any split half that is still a **fragment** rather than a compilable block; plus the run's `N skipped` before and after, so the count moves by exactly the number added
  - Notes: at least three of the candidates are bare signature comparisons — `CloudEventsSupport.md` shows `public Message MapToMessage(OrderCreated request)` against its V10 form, with no bodies — and **splitting a fragment yields two fragments, neither of which compiles.** Without this task those blocks are silently absent from the baseline and the gate still reports `0 findings`: a repair that leaves nothing measurable behind. The alternative — rewriting them into compilable examples — is a page change beyond the defect, so it is 4.3's *"a repair that needs more than its defect fixed says so"*, not a default.

- [x] **Task 4.4:** Update the baseline and scaffold for every repaired block
  - Input: tasks 4.2 and 4.3
  - Output: `baseline.tsv` rows added for blocks that now compile, and the run green at the end of the branch
  - Notes: the gate shipped in phase 3, so **this branch must end green or it reddens `master`**.

- [x] **Task 4.5:** Record the site change and take the sign-off
  - Input: the branch's `git diff --stat` against `master`
  - Output: the list of pages changed, in this file, and the sign-off asked for **with the head-ref deletion named in the same breath**
  - Notes: obligation 7. A merge is not a deletion, and one authorisation covers one PR.

- [x] **Task 4.6:** Predict and reconcile the nine gates
  - Input: as task 1.10
  - Output: § *Phase 4 prediction* and § *Phase 4 as executed*
  - Notes: **this is the phase where `pagelint` may move**, and the direction is down if a repaired block gains `using` directives. Predict the number **before** the work and reconcile; `tools/README.md` row 2 owns the figure. `symbolcheck` and `optioncheck` both read `contents/` and may move too — predict each, do not assume.

---

## Phase 5 — Acceptance *(6 tasks, one PR, no page touched)*

- [x] **Task 5.1:** Walk the three criteria with no instrument, first
  - Input: AC13, AC14, AC15
  - Output: § *Acceptance walk* — for each, who read it, what they read, and what they found
  - Notes: **both criteria ever found unmet at a close were unmarked ones.** AC13 and AC14 are the maintainer's readings of the scaffold boundary; AC15 is a check that this spec's own documents never claim the gate checks behaviour.

- [x] **Task 5.2:** Walk the twelve instrumented criteria, running each named instrument
  - Input: AC1–AC12
  - Output: each criterion with its command and that command's actual output
  - Notes: eleven of the twelve were **deferred** at the requirements review — they name a tool that did not exist. This is the walk where they stop being deferred. **Check what the instrument prints against what the criterion claims**, which is how AC3 and AC1's pipe defects were found in the first place. **AC11 is met here rather than by a tenth predict-and-reconcile task**: phases 1–4 each carry one, and this phase touches only `spec/` and `tools/README.md`, so its own prediction is *none* for all nine — say so in the walk rather than leaving the pattern to break silently at the last phase.

- [x] **Task 5.3:** The backwards check — what changed that should not have
  - Input: `git diff --stat` against the ref at which 016 opened
  - Output: the page set this spec changed, compared against phase 4's declared list, with any difference explained
  - Notes: *"while I'm here"* is how a spec quietly widens.

- [x] **Task 5.4:** Write the defect ledger
  - Input: every phase's findings, plus the **nine defects found before phase 1 began** — three at the requirements review, three at the design review, the Q6 amendment misses, and the six before/after blocks
  - Output: § *Defect ledger* with a **found by** column distinguishing a tool's win from a re-derivation's
  - Notes: 015's split was 12 tool / 4 control / 14 person. This spec's will look different and the difference is the interesting part.

- [x] **Task 5.5:** Write the friction ledger
  - Input: frictions **52–56**, already written in `requirements.md` and `design.md`, plus anything phases 1–4 met
  - Output: § *Workflow friction* in this file, continuing from 56
  - Notes: re-derive the ledger's total rather than inheriting a header — this file's own count has been wrong in a heading while its body disagreed, twice.

- [x] **Task 5.6:** Close the spec
  - Input: everything above
  - Output: the `README.md` checklist at its final count, § *What 016 shipped*, and **the residual gap in one sentence** — the line 017 starts from
  - Notes: 015's closing sentence became this spec. Write the next one as though somebody will have to execute it, because they will.

---

## What the tasks review found — 2026-09-20, six findings, all before a line was built

Four came from reading the plan against `CLAUDE.md` and the corpus; two from running the second
method the plan asks for.

| # | Finding | Answered by |
|---:|---|---|
| **1** | **Task 4.2 prescribed a marker that would publish a falsehood.** ❌/✅ is `CLAUDE.md` § *Version markers on code*, which opens *"Where V9 and V10 differ"*. `ImplementAQueryHandler.md` block 10 is sync → async — **Reactor and Proactor, both current in V10** — so ❌ on `Execute` asserts a supersession that `ReactorAndProactor.md` denies | task 4.2, rewritten to choose the label by what the pair is |
| **2** | **The design's "6 blocks, 4 pages" over-counts.** The second method finds nine candidates on five pages, and reading strikes out four | task 2.5, which now expects the list to shrink |
| **3** | **Phase 4 creates ❌ blocks and nothing marked them.** P0-5 covers *any* block that must not compile, not only the eight that existed at phase 3; three candidates are bare signature fragments, where splitting yields two fragments | **task 4.3a, inserted** |
| **4** | **Exit 2 was promised and never produced.** No task emitted it, and its real-world cause — an unrestored reference set — makes all 985 blocks fail believably | **task 1.6a, inserted**, plus 1.6's named conditions |
| **5** | **Obligation 8 had no red-proof.** Friction 54's mechanism is a parse failure suppressing binding across a shared compilation; a `CS0246` plant and a clean plant are both still reported under batching, so the existing pair cannot fail if the rule is broken | task 1.8's third plant |
| **6** | **Task 1.9's control was one-way while the checklist called it two-way.** Two identical runs are also what a tool that cached, or did nothing, would print | task 1.9's second direction |

**The measurement behind findings 1–3**, run at the review's HEAD through `pagelint.Page`
*(friction 53)*:

```bash
python3 - <<'PY'
import sys, re
sys.path.insert(0, 'tools'); import pagelint
v9 = re.compile(r'^\s*//\s*V9\b', re.M); v10 = re.compile(r'^\s*//\s*V10\b', re.M)
ba = re.compile(r'^\s*//\s*(Before|After)\b', re.M)
for rel, pg in sorted(pagelint.load_pages().items()):
    n = 0
    for b in pg.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp', 'c#', 'cs'):
            continue
        n += 1
        body = '\n'.join(b['body'])
        kind = 'V9/V10' if v9.search(body) and v10.search(body) else (
               'Before/After' if ba.search(body) else None)
        if kind:
            print(rel, n, b['start'], kind)
PY
```

```text
AgreementDispatcherRouting.md     3   99  Before/After
CloudEventsSupport.md             7  273  V9/V10
CloudEventsSupport.md             8  282  V9/V10
CloudEventsSupport.md             9  291  V9/V10
ImplementAQueryHandler.md        10  306  Before/After
PolicyRetryAndCircuitBreaker.md   6  147  V9/V10
SwitchingSchedulers.md            2   37  Before/After
SwitchingSchedulers.md            5   88  Before/After
SwitchingSchedulers.md            6  109  Before/After
```

**Nine candidates; four are not the defect.** `SwitchingSchedulers.md` already puts *Before* and
*After* in separate fences, one complete block each. `AgreementDispatcherRouting.md` block 3 is not
the shape: `// Before Jan 2025` and `// After Jan 2025` are tax-rule comments inside one routing
lambda.

**The same run loads 985 C# blocks across 162 pages**, the design's figure by an independent path.

Task 2.5 still owns the confirmed list and derives it two ways at its own HEAD.

---

## Totals

**39 tasks** — 11 + 6 + 9 + 7 + 6:

```bash
grep -c '^- \[.\] \*\*Task' spec/016-compile_gate/tasks.md      # 39
```

The phase table sums to 39 independently. It was 37 at the review; `1.6a` and `4.3a` were inserted.

---

## Task quality checklist, applied to this list

| | |
|---|---|
| Each phase is one PR, deliverable-shaped | five phases; § *The phases* gives the ordering |
| Standing obligations stated once | §1, twelve, not repeated per task; five are 016's own |
| Every new check has a red-proof with a two-way control | tasks 1.6a, 1.7, 1.8, 1.9 and 3.6; 3.6 carries the green control. 1.6a and 1.9's second direction were added at the review |
| Inherited counts re-derived by two methods | tasks 1.2, 2.5 and 4.1; task 2.5 expects its list to shrink |
| Every Output names something that can be seen to exist | file paths, `.tsv` rows, named sections, recorded runs |
| Acceptance last, owning the walk and both ledgers | phase 5, starting with the three criteria that have no instrument |

---

**Next step: `/spec:review`.** To confirm: **P0-9's boundary** (task 2.3 measures it, and the claim
list's size decides whether phase 4 is a phase), and **keeping page edits out of phase 3** so only
one phase needs a sign-off.

---

## Phase 1 prediction

**Written 2026-09-20, before any gate was run in this phase.** `tools/README.md` owns the expected
figures; this section cites them.

**All eight: none.**

| # | Gate | Prediction | Why, from mechanism |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | It walks `.md` files, including `tools/`; writing `tools/README.md` moved it 164 → 165. Phase 1 adds `tools/blockcheck.py`, `tools/blockcheck/*.cs`, `*.csproj` and files under `tools/blockcheck/scaffold/`, and **no `.md`**. A `tools/blockcheck/README.md` would move it to 166 |
| 2 | `pagelint` | **none** | Corpus is `contents/` + `README.md`; a `.py`, `.cs` or `.csproj` cannot enter it. Rule 6's count moves only when a page's C# block gains `using` directives, which is phase 4 |
| 3 | shape | **none** | Reads `SUMMARY.md`. No page is added, moved or renamed |
| 4 | redirects | **none** | Reads `.gitbook.yaml`. Untouched |
| 5 | `versioncheck` | **none** | Reads version pins in page prose, 18 across 5 pages. Task 1.4's 67 pins are in a `.csproj` |
| 6 | `optioncheck` | **none** | Reflects marked option tables in `contents/`. No table is touched; task 1.3's second `dotnet` project is outside its corpus |
| 7 | `--verify` | **none** | Compares the live sitemap with the predicted tree. Nothing published changes |
| 8 | `symbolcheck` | **none** | Corpus is `contents/`. No page is edited |

All eight are vacuous passes. The defence is that the *before* figures are read at the start and
the *after* figures at the end, both against `tools/README.md`.

---

## Phase 1 as executed

### The eight gates, read before the work *(the prediction's baseline)*

Run at `1f5fc10` with `tools/blockcheck.py` present and nothing else built. Each line is the gate's
own last line, each exit code read bare — `<cmd> > /tmp/g.out 2>&1; echo $?`:

```text
exit=0  No broken internal links (165 files checked).
exit=0  0 errors, 744 warnings (using-directive debt: 744 blocks across 115 pages) across 162 pages.
exit=0  0 shape failures — 161 pages, 12 sections, deepest 4 of 4 segments, widest 12 of 20 top-level entries
exit=0  0 redirect failures — 77 entries, 7858 bytes, all printable ASCII
exit=0  0 stale pins of 18 examined across 5 page(s).
exit=0  0 mismatches across 59 tables and 519 rows.
exit=0  No watchlisted symbols found (22 entries, 161 pages checked, 3 silenced).
exit=0  predicted 161, published 161, 161 agree
```

**Eight for eight against `tools/README.md`'s rows**, including both rows carrying the second ref
`3be2a78`.

> The first reading piped every gate through `tail` before `echo $?`, which reports `tail`'s code
> — **0** for all eight whatever they returned. Constraint 11; re-read in the form above.

### The enumerator and extractor *(task 1.1)*

`tools/blockcheck.py` enumerates through `pagelint.load_pages()` and `pagelint.Page`, filters on
`pagelint.CSHARP_TAGS`, and hoists `using` directives with `pagelint.USING_RE`. **Four names
imported, no second copy of any** — constraint 2, and friction 53's answer.

```bash
python3 tools/blockcheck.py --list > /tmp/l.tsv; echo $?      # 0, read bare
wc -l < /tmp/l.tsv                                            # 985
awk -F'\t' '{print $1}' /tmp/l.tsv | sort -u | wc -l          # 145
grep -c '/Users/' tools/blockcheck.py                         # 0
grep -c '^contents/KafkaConfiguration.md	' /tmp/l.tsv      # 20
```

```text
985 C# blocks across 145 pages: 11 namespaced, 306 types, 136 members, 532 statements
```

**985 across 145** reproduces `requirements.md` § *Current state*. The 20 `KafkaConfiguration.md`
rows are AC2's named case: every fence on that page is ` ``` csharp` with a space.

**The corpus is "the pages `pagelint` lints", not "the pages under `contents/`".** Today these are
the same 985 blocks, because the only other page, `README.md`, carries **0** C# blocks. A C# block
added to the site root will be compiled.

**`--list` prints rows to stdout and its summary to stderr**, so `wc -l` of a redirect counts
blocks only, and every row is newline-terminated (friction 55).

**Exit codes, all read bare:**

```text
--list, corpus present            0
--show <page> <n>, block exists   0
--show <page> 99, no such block   2
no mode at all (the gate)         2    "the gate itself is not built yet: nothing was checked"
```

**An empty enumeration is exit 2, not exit 0.**

`--show` writes the block body verbatim to stdout:

```bash
python3 tools/blockcheck.py --show contents/KafkaConfiguration.md 1 > /tmp/b1.cs
sed -n '72,86p' contents/KafkaConfiguration.md > /tmp/page.cs
diff /tmp/page.cs /tmp/b1.cs        # empty
```

The line range is the tool's own report (`lines 71-87`, the fences). The block is tab-indented, so
the empty diff also shows leading whitespace survives. The corpus-wide check is task 1.7's.

**The shape counts reproduce the design's 11 / 306 / 136 / 532 exactly.** Both figures come from
the same classifier, so this is not task 1.2's second method.

This task classifies but does not wrap; the emission half of the wrapper rules is task 1.2's.

**Verdict names drifted between approved documents:** `requirements.md` AC1 says
`BUILT · SKIPPED · NOT COMPILABLE · FAILED`; `design.md` § *The verdict model* says
`CLEAN · FAILED · SKIPPED · NOT COMPILABLE`. `BUILT` and `CLEAN` are the same verdict.

**Ruled at task 1.3: the tool prints `BUILT`**, the name AC1's instrument reads. `design.md`'s
`CLEAN` is left as approved.

### The four wrapper rules, and their counts measured twice *(task 1.2)*

`WRAPPERS` in `tools/blockcheck.py` holds the emission half; `classify()` holds the test half. The
closing braces are derived — one `}` per `{` in the opening lines.

| Shape | Test | Wrapper | `--list` | the probe |
|---|---|---|---:|---:|
| `namespaced` | declares its own `namespace` | none; `using`s hoisted above it | 11 | 11 |
| `types` | declares a `class`/`record`/`interface`/`struct`/`enum` | `namespace B_<id>` | 306 | 306 |
| `members` | a line opens with an access or member modifier | the above, plus `public class Holder` | 136 | 136 |
| `statements` | anything else | the above, plus `public async Task Run()` | 532 | 532 |
| | | | **985** | **985** |

Method 1 is this tool. Method 2 is the committed probe, a separate implementation of the same four
tests, run from `/tmp` *(obligation 12)*:

```bash
python3 tools/blockcheck.py --stage /tmp/bc12/staged
awk -F'\t' '{n[$4]++} END{for(k in n) print n[k], k}' /tmp/bc12/staged/index.tsv

cd /tmp && python3 <repo>/spec/016-compile_gate/probe/gen.py /tmp/bc12probe/blocks
```

**They agree on all four.** The probe's `using` test rejects `using static X;` and
`using Alias = X.Y;` where this tool uses `pagelint.USING_RE`; no block's shape turns on the
difference.

Both walk the corpus through `pagelint.Page`, so the corpus *size* takes its second method from the
grep — friction 53's own number:

```bash
grep -r '^```csharp$' contents/ | wc -l      # 835 blocks
grep -rl '^```csharp$' contents/ | wc -l     # 117 pages
```

**835 against 985, and the 150 reconcile exactly** by opening-fence spelling:

```text
  835  exactly ```csharp
  140  ``` csharp (one space)
    8  indented
    1  c# tag
    1  ```csharp + trailing space
  985  total
```

The 835 is the part of the corpus a grep can see, not a corroboration of 985.

**985 blocks produced 985 files**, so no `<page>_<n>` identifiers collided:

```bash
wc -l < /tmp/bc12/staged/index.tsv      # 985
ls /tmp/bc12/staged/*.cs | wc -l        # 985
```

**`NOT COMPILABLE` is 0 of 985 by construction:** `classify()` ends in an unconditional
`statements`, so a block is unwrapped only if it raises. The verdict stays in the model.

### The Roslyn tool, and the reference pin *(tasks 1.3 and 1.4)*

`tools/blockcheck/Program.cs` compiles **one `CSharpCompilation` per block** (obligation 8; the
opening comment carries friction 54's measurements). Reference assemblies are shared between
compilations; diagnostics are not.

It prints `id<TAB>verdict<TAB>error count<TAB>distinct codes` and nothing else to stdout. **Its exit
code is 0 or 2, never 1**: the corpus verdict belongs to the Python half, which owns the 0/1/2
contract.

`tools/blockcheck/refs/refs.csproj` is **67 packages, every one with an explicit version, and no
sources**, carried forward verbatim from the probe:

```bash
grep -c 'PackageReference' tools/blockcheck/refs/refs.csproj   # 67
grep -c 'Version="'        tools/blockcheck/refs/refs.csproj   # 67
grep -c 'ProjectReference' tools/blockcheck/refs/refs.csproj   # 0
grep -c 'ProjectReference' tools/blockcheck/blockcheck.csproj  # 0
```

Both projects target **net9.0** (the probe used net8.0), matching `tools/optioncheck` and the
`options` job's `setup-dotnet 9.0.x`. It changed none of the probe's figures; see below.

#### The reference set states itself, because the step that is documented is the step that gets skipped

The probe's recipe has a hand-copy step:

```bash
cp bin/Debug/net8.0/*.dll <refdir>/
cp $(dirname $(which dotnet))/packs/Microsoft.NETCore.App.Ref/8.0.0/ref/net8.0/*.dll <refdir>/
```

**The first build did the first line and not the second:**

```text
233 reference assemblies
985 blocks, 1 built, 984 failing
981 CS0518   predefined type 'System.Object' is not defined
```

**99.9% of the corpus reported broken with nothing checked** — friction 56, one layer down.
`refs.csproj` now writes its own resolved reference set at build time:

```xml
<Target Name="WriteReferenceList" AfterTargets="ResolveReferences">
  <WriteLinesToFile File="$(OutputPath)refs.txt" Lines="@(ReferencePath)" ... />
</Target>
```

That is exactly what the compiler is handed: the 67 packages and the targeting pack for the
project's own `TargetFramework`, with no SDK version written down and no copy step. **497
assemblies**, against the probe's hand-assembled 391. The tool takes that file and **exits 2 if it
is absent or names an assembly that is not there**.

#### The corpus run, and the probe reproduced exactly

```bash
dotnet build tools/blockcheck/refs/refs.csproj -c Release          # exit 0
dotnet build tools/blockcheck/blockcheck.csproj -c Release         # exit 0
python3 tools/blockcheck.py --stage /tmp/bc12/staged               # 985 staged
dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll \
    /tmp/bc12/staged tools/blockcheck/refs/bin/Release/net9.0/refs.txt /tmp/bc12/verdicts.tsv
```

```text
497 reference assemblies
985 blocks, 60 built, 925 failing
22.0s total, 22ms per block
```

**60 and 925 match the probe block for block**, joined on block id against the committed
`probe/verdicts.tsv`:

```bash
join -t$'\t' -j1 <mine, BUILT→CLEAN> <probe/verdicts.tsv> | wc -l    # 985
awk -F'\t' '$2!=$3{d++} END{print d+0, "disagreements"}'             # 0
```

**985 of 985 verdicts identical**, across a different target framework, a different reference set
and a second implementation of the tool.

**The design's error-code figures are floors.** `design.md` quotes `CS0246` at 714 and `CS0103` at
653; this run reads **738** and **677** over the same verdicts. The probe wrote `.Take(6)` distinct
codes per block, so a block failing seven ways contributed six. No decision in the design turns on
the gap. This tool writes every distinct code.

### The scaffold, and the two-way control that says it does something *(task 1.5)*

**A scaffold is declared in `tools/blockcheck/scaffold/pages.tsv`, one row per page**, in two parts,
as 015's harness had:

| Part | Where | What it does |
|---|---|---|
| a **unit** | `scaffold/units/*.cs` | an extra compilation unit, compiled **in that block's own compilation** — types and values the page names but no block defines |
| a **prelude** | `scaffold/preludes/*.txt` | **replaces** the generic wrapper for that page's blocks: a typed handler class, so a block that is the body of a method has the method to sit in |

A unit declares the `using` lines it wants injected, in the unit itself:

```csharp
// blockcheck: using static PageContext;
```

That line is how a page's named-but-undefined values reach a block. The four preludes and
`PageContext.cs` are carried forward verbatim from `spec/016-compile_gate/harness/`.

**The unit rides in the block's own compilation and nowhere else**: one block, one
`CSharpCompilation`, with two trees. A scaffold can help its own block and cannot silence another.

**A scaffold that does not parse stops the run** — exit 2, nothing checked, tested once before any
block compiles. A parse error in a shared tree suppresses binding across its compilation, so a
broken scaffold would pass every page using it.

**Errors reported against a scaffold's tree are not counted against the block**, and are printed as
`WARNING: N error(s) reported against scaffold trees`.

#### `--list-scaffold`, and why it reads the tree rather than grepping it

```bash
python3 tools/blockcheck.py --list-scaffold > /tmp/sc.txt; echo $?    # 0, read bare
wc -l < /tmp/sc.txt                                                   # 56
find tools/blockcheck/scaffold -type f | wc -l                        # 6
```

```text
55 identifiers from 1 unit(s) and 4 prelude(s); 1 page(s) scaffolded
```

**Six files: five scaffold and one map** (AC8's count). The 56th row is the injected `using`.

The identifiers come from **Roslyn's syntax tree**, not a regex, so the listing cannot under-report.
Each prelude is rendered as `wrap()` renders it, with `__NAME__` substituted and one `}` per `{@`,
which also checks its braces balance.

#### The control: one page in, one block moves

`contents/DapperOutbox.md` is the phase-1 row in `pages.tsv`. Same corpus, same references, the
scaffold the only difference:

```text
                  no scaffold                  PageContext
DapperOutbox_1    FAILED  1 error   CS0103     BUILT   0 errors
DapperOutbox_2    FAILED 16 errors  CS0103…    FAILED 12 errors  CS0103,CS0115,CS0117
corpus            985 blocks, 60 built         985 blocks, 61 built
```

**Block 1 moves to `BUILT`; block 2 stays `FAILED` with a different error set; the corpus moves by
exactly one.**

**The staged directory holds 986 `.cs` files for a 985-block corpus.** The extra one is the staged
unit; the tool reads `index.tsv`, not a glob:

```bash
ls /tmp/bc15/*.cs | wc -l        # 986
wc -l < /tmp/bc15/index.tsv      # 985
wc -l < /tmp/bc15/verdicts.tsv   # 985
```

The unit is copied into the staged directory, so what was compiled can be read afterwards.

### The exit contract, and exit 2 red-proofed four ways *(tasks 1.6 and 1.6a)*

`--report` compiles the whole corpus and writes one row per block, **verdict first**:

```text
BUILT	contents/AWSSQSConfiguration.md	1	AWSSQSConfiguration_1	0
FAILED	contents/AWSSQSConfiguration.md	2	AWSSQSConfiguration_2	6	CS0103,CS0246,CS1002
```

```bash
python3 tools/blockcheck.py --report > /tmp/r.tsv; echo $?    # 0, read bare
awk '{n[$1]++} END{for(k in n) print k, n[k]}' /tmp/r.tsv     # BUILT 61, FAILED 924
wc -l < /tmp/r.tsv                                            # 985
```

**61 + 924 = 985**, AC1's sum. Rows go to stdout (or the named file); every other line goes to
stderr.

**`NOT_COMPILABLE` carries an underscore.** AC1 counts verdicts with `awk '{n[$1]++}'`, which splits
on whitespace; `NOT COMPILABLE` would be counted as `NOT`.

**The run states its scope before its verdict:**

```text
985 blocks: 61 BUILT, 924 FAILED
no baseline yet: this is a measurement, not a gate
0 findings
```

The middle line is mandatory until `baseline.tsv` exists in phase 3: `0 findings` over 924 failures
is otherwise indistinguishable from a green gate.

#### Exit 2, proved four ways, every code read bare

Obligation 3 and review finding 4. The proof is the code **and no per-block verdicts printed**:

| The state | Exit | Report rows |
|---|---:|---:|
| the reference set unrestored — `refs/bin` moved away | **2** | **0** |
| `refs.txt` present, naming **one** assembly that is not there | **2** | **0** |
| the Roslyn tool not built | **2** | **0** |
| **the control: everything restored** | **0** | **985** |

```text
no reference list at …/refs/bin/Release/net9.0/refs.txt: the reference project has not been
restored and built, so nothing was checked
  dotnet build tools/blockcheck/refs/refs.csproj -c Release
  dotnet build tools/blockcheck/blockcheck.csproj -c Release

1 of 498 reference assemblies are missing or unreadable, first: /nonexistent/Paramore.Brighter.dll
NOTHING WAS CHECKED
```

**One missing assembly of 498 is exit 2, not a warning.** Otherwise the blocks needing it fail with
real error codes and the rest pass, which reads as a measurement.

`refs.txt` was restored and `diff`ed byte-identical afterwards; every exit code was read with
`echo $?` directly after the command.

**Nine states exit 2, named in one block comment above `mode_report` in `tools/blockcheck.py`**:
four enforced in Python, three in `tools/blockcheck/Program.cs` and propagated, and two —
`pagelint` unimportable, an empty enumeration — in `main`. Four are proved above; the scaffold-parse
one by task 1.8's third plant.

### Extraction, red-proofed both ways *(task 1.7)*

```bash
python3 tools/blockcheck.py --verify-extraction; echo $?     # 0, read bare
```

```text
985 of 985 identical, 1 with `using` directives hoisted
```

**N of N, with N the corpus count.** The check reconstructs the stager's split — `using`
directives hoisted above the wrapper, the rest in place — so "identical" covers every other byte,
including whitespace and line endings.

**The red half: one trailing space** appended to line 9 of one staged block:

```text
exit=1
984 of 985 identical, 1 with `using` directives hoisted
contents/KafkaConfiguration.md	1	KafkaConfiguration_1	NOT IDENTICAL
```

**Exactly that block, named, and nothing else moved.**

#### The hoisting count is 1, and the block it names is a finding for phase 2

248 blocks carry a `using` line, but hoisting moves a line only when a directive sits below a
non-directive line. In 984 blocks the directives are already at the top. The one exception:

```bash
python3 tools/blockcheck.py --show contents/AWSSQSMigrateToV10.md 1
```

```csharp
// V3
using Amazon.SimpleNotificationService;
using Amazon.SQS;

// V4 - Same namespaces, different package versions
using Amazon.SimpleNotificationService;
using Amazon.SQS;
```

**A before/after pair inside one fence**, the shape task 2.5 confirms. It is on neither the
design's list nor the tasks review's nine candidates, because it carries no `// Before`, `// After`,
`// V9` or `// V10` marker; the duplicated `using` is what shows it. Carried to task 2.5 as a
second method. Phase 1 touches no page.

### The compile verdict red-proofed — and the design's central mechanism did not reproduce *(task 1.8)*

**Five plants, committed at `tools/blockcheck/plants/`, none of them a block from `contents/`:**

```bash
dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll \
    tools/blockcheck/plants tools/blockcheck/refs/bin/Release/net9.0/refs.txt
```

```text
Plant_Clean	BUILT	0
Plant_Missing	FAILED	1	CS0246
Plant_Unparseable	FAILED	5	CS1026,CS1513
Plant_Leak_A	BUILT	0
Plant_Leak_B	FAILED	1	CS0246
5 blocks, 2 built, 3 failing
```

`Plant_Missing` must come back `FAILED` with the compiler's own code and `Plant_Clean` must come
back `BUILT`.

#### Review finding 5 was right that obligation 8 had no red-proof, and wrong about what would give it one

The task list specified a **parse-broken third plant**, on the design's stated mechanism: a parse
failure suppresses semantic binding across a shared compilation, so under batching the `CS0246`
plant would go silent.

**It does not go silent.** With the blocks deliberately batched into one `CSharpCompilation`, all
three plants report exactly what they report per block:

```text
                      per block                    batched
Plant_Clean           BUILT   0                    BUILT   0
Plant_Missing         FAILED  1  CS0246            FAILED  1  CS0246
Plant_Unparseable     FAILED  5  CS1026,CS1513     FAILED  5  CS1026,CS1513
```

**So the parse-broken plant cannot fail when obligation 8 is broken.**

#### What batching actually does, measured over all 985

A scratch probe put every staged block in **one** compilation and reported diagnostics per tree:

```text
one compilation per block      985 blocks, 61 built, 924 failing
one compilation, all 985       985 blocks, 64 built, 921 failing
                               6,331 errors: 865 CS1xxx/CS8xxx, 5,466 semantic
```

**Four false `BUILT` verdicts, every one a tutorial:**

```text
TutorialFirstCommand_2   TutorialFirstCommand_3   TutorialFirstMessage_3   TutorialDurableOutbox_3
```

All four fail alone with **`CS0246`** and build in the batch. The mechanism is **visibility, not
diagnostics**: a tutorial declares a type in one block and uses it in the next, and the 11
`namespaced` blocks are emitted into the namespace the page wrote. In one compilation block 2
resolves a name block 1 declared, which a reader copying block 2 alone cannot do.

`Plant_Leak_A` and `Plant_Leak_B` replace the parse-broken plant as obligation 8's guard:

```text
                      per block                    batched
Plant_Leak_A          BUILT   0                    BUILT   0
Plant_Leak_B          FAILED  1  CS0246            BUILT   0      <- false clean
```

**`Plant_Unparseable` stays** as the scaffold-parse case (218 corpus blocks carry `CS1xxx` errors),
and no longer claims to guard obligation 8.

#### The correction, stated plainly: obligation 8 stands, its published cause does not

`design.md` § *The finding nobody was looking for* reports that batching suppressed semantic binding
wholesale: *1,444 errors and not one semantic error* across all 985, and *446 blocks certified clean
of which a 20-block sample was 100% false-clean*. **Re-run here, with Roslyn 4.11 on net9.0, it does
not reproduce:** one compilation of all 985 reported **6,331** errors, **5,466** of them semantic,
and certified **64** clean against 61 — three more, not 386.

**Obligation 8 is unchanged**, now resting on this repository's measurement. `design.md` is not
edited.

### No vacuous pass, and the second direction that makes the first mean something *(task 1.9)*

**Two consecutive runs, nothing touched:**

```text
run1  exit=0  wall=26s   985 blocks: 61 BUILT, 924 FAILED
run2  exit=0  wall=23s   985 blocks: 61 BUILT, 924 FAILED
diff /tmp/run1.tsv /tmp/run2.tsv      empty
```

**Identical to the byte** — which a tool that cached, or did nothing the second time, would also
print. So the second direction: one line added inside block 1 of `contents/AWSSQSConfiguration.md`,
a `BUILT` block:

```csharp
        NoSuchTypeXyz123 deliberate = null;
```

```text
run3  exit=0   985 blocks: 60 BUILT, 925 FAILED

1c1
< BUILT	contents/AWSSQSConfiguration.md	1	AWSSQSConfiguration_1	0
> FAILED	contents/AWSSQSConfiguration.md	1	AWSSQSConfiguration_1	1	CS0246
```

**One block moved, one line of the report changed.** Reverted:

```bash
git checkout contents/AWSSQSConfiguration.md
git diff contents/AWSSQSConfiguration.md      # empty
```

```text
run4  exit=0   985 blocks: 61 BUILT, 924 FAILED
diff /tmp/run1.tsv /tmp/run4.tsv      empty
```

**Four runs: same, same, different, same.**

**Wall clock is tens of seconds, not a single figure:** 26s and 23s here; the probe's four runs over
identical inputs gave 6.5s, 22.9s, 39.5s and 26.2s.

### The eight gates, reconciled *(task 1.10)*

Run at the end of the phase, `git add -A` first so that `--changed` sees the diff, every exit code
read bare:

| # | Gate | Predicted | Read at the start | Read at the end | |
|---:|---|---|---|---|---|
| 1 | `linkcheck` | none | 165 files, 0 broken | **165 files, 0 broken** | ✅ |
| 2 | `pagelint` | none | 0 errors, 744 warnings, 162 pages | **0 errors, 744 warnings, 162 pages** | ✅ |
| 3 | shape | none | 161 pages, 12 sections | **161 pages, 12 sections** | ✅ |
| 4 | redirects | none | 77 entries, 7858 bytes | **77 entries, 7858 bytes** | ✅ |
| 5 | `versioncheck` | none | 0 stale pins of 18 | **0 stale pins of 18** | ✅ |
| 6 | `optioncheck` | none | 0 mismatches, 59 tables, 519 rows | **0 mismatches, 59 tables, 519 rows** | ✅ |
| 7 | `symbolcheck` | none | 0 findings, 22 entries, 3 silenced | **0 findings, 22 entries, 3 silenced** | ✅ |
| 8 | `--verify` | none | 161 predicted = 161 published | **161 predicted = 161 published** | ✅ |

**Eight for eight, and `pagelint --changed origin/master` is green.**

`linkcheck`'s prediction depended on no `.md` being added under `tools/`:

```bash
git diff --cached --name-only | grep '^tools/' | sed 's/.*\.//' | sort | uniq -c
#    7 cs      2 csproj      1 py      2 tsv      4 txt
git diff --cached --name-only --diff-filter=A | grep '\.md$'      # nothing
```

Seventeen files changed; none is a page or a `.md`.

The before-figures were read at the top of the phase and cited to `tools/README.md`, not re-derived
from the closing run.

---

## Phase 2 prediction

**All eight: none.** Task 2.6's *Notes* read **"prediction is none; this phase commits a `.tsv` and
edits this file"**, approved at `7b493c3`, before phase 2 ran.

The mechanism is phase 1's. No page under `contents/` is edited, so rules 1–7, `symbolcheck`,
`optioncheck` and `versioncheck` cannot move; `SUMMARY.md`, `.gitbook.yaml` and the published tree
are untouched, so shape, redirects and `--verify` cannot. `linkcheck` walks `tools/`, and this phase
adds no `.md` there: it adds a `.tsv` and two `.cs` plants, and edits a `.py`, a `.cs` and a
`.csproj`.

The before-figures are `tools/README.md`'s at the top of the phase, not re-derived from the closing
run.

---

## Phase 2 as executed

### The corpus run *(task 2.1)*

**985 blocks: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE**, exit code read bare:

```bash
dotnet build tools/blockcheck/refs/refs.csproj -c Release
dotnet build tools/blockcheck/blockcheck.csproj -c Release
python3 tools/blockcheck.py --report /tmp/r.tsv; echo $?      # 0
awk '{n[$1]++} END{for(k in n) print k,n[k]}' /tmp/r.tsv      # BUILT 68, FAILED 917
```

```text
985 blocks staged in …, 2 with a scaffold unit, 1 pages in the map
501 reference assemblies
985 blocks, 68 built, 917 failing, 1 scaffold unit(s)
7.8s total, 8ms per block
985 blocks: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE
no baseline yet: this is a measurement, not a gate
0 findings
```

**`tools/blockcheck/verdicts.tsv` is that run, committed**: 985 rows, `verdict TAB page TAB ordinal
TAB ident TAB error count TAB distinct codes`, no header, so phase 3 can diff it against a baseline
without stripping one.

**Run from a clean directory** (obligation 12): `git ls-files` through `tar` into a scratch
directory, both projects built there, output identical to the in-tree run, **985 of 985 rows**. The
clean copy produced `verdicts.tsv`.

**The four counts sum to the corpus count.** The run now prints all four; before, it printed only
the verdicts it saw (`61 BUILT, 924 FAILED`), which cannot be added up. `VERDICTS` holds the
vocabulary in one place.

| Verdict | Count | Why |
|---|---:|---|
| `BUILT` | **68** | compiles against the released packages, with its wrapper and page scaffold |
| `FAILED` | **917** | at least one error in the block's own tree |
| `SKIPPED` | **0** | no opt-out exists yet; Q5 and the marker are phase 3's |
| `NOT_COMPILABLE` | **0** | `classify()` returns a shape for every block, so nothing reaches this verdict (friction 59) |

**Per shape** (the wrapper is AC14's subject):

```text
namespaced      4 built of   11
toplevel        0 built of    9
types          29 built of  303
members        11 built of  130
statements     24 built of  532
```

**Per page:** 35 pages have at least one block that builds; **5 pages, 7 blocks, build in their
entirety** (`BoxProvisioning.md`, `FirestoreInbox.md`, `FirestoreOutbox.md`, `SpannerInbox.md`,
`SpannerOutbox.md`). A per-page ratchet would admit 7 of 985, so the baseline is per block.

**The design measured 60 clean with no scaffold; this run reads 68.** +1 `DapperOutbox.md` from phase
1's scaffold unit, +7 from Darker's four packages (task 2.4). At phase 1's 497 assemblies the same
instrument reads **61**, phase 1's figure.

### The parse triage *(task 2.2)*

**180 of 985 blocks do not parse.** Three measurements exist:

| | Count | Instrument |
|---|---:|---|
| `design.md` § *The probe* | **131** | the probe, a different TFM and reference set, un-triaged |
| task 2.2's own filter — `CS1002`/`CS1513`/`CS1519`/`CS8635` | **165** | `grep -cE` over the corpus run |
| **the parser** | **180** | `blockcheck --parse`, syntax-only, no references and no binder |

**The filter is a subset and misses 22 blocks**, by `comm` over the two id lists (0 the other way).
They fail on `CS1525`, `CS1022`, `CS0116`, `CS8124`, `CS8803`: `AzureBlobDistributedLock_2`,
`FAQ_16`, `HangfireScheduler_18`, `MigratingToPollyV8_12`, `QuartzScheduler_15`,
`QuartzScheduler_16`, `ShowMeTheCode_3`, `SweeperCircuitBreaking_4`, `UsingTheContextBag_16`,
`V10MigrationGuide_19` and twelve more. `--parse` uses `SyntaxTree.GetDiagnostics()`, which is the
parse itself.

**`--parse` red-proofed both ways, against the plants:**

```text
Plant_Unparseable   BROKEN   5   CS1026,CS1513     <- the check fires
Plant_Missing       PARSES   0                     <- FAILS the compile on CS0246, and parses
Plant_Clean         PARSES   0
Plant_Leak_A/B      PARSES   0
```

`Plant_Missing` fails the compile and parses, so the mode separates parse from bind. Bad arguments
and a directory with no `index.tsv` both exit **2** with no rows.

**The triage, 180 blocks across 67 pages:**

| | Blocks | What it is | Named example |
|---|---:|---|---|
| **A documented omission written where C# needs a token** | **93** | `...` or a comment standing in for an expression — `opt.Outbox = /* your MS SQL Outbox */;`, `Partition = //derived from the region…`, `.AddBrighter(options => { ... })` | `MsSqlDistributedLock.md#2`, `AWSSQSConfiguration.md#2` |
| **An excerpt of a larger expression** | **77** | part of an argument list, object initializer or fluent chain — `new Subscription(` with no `;`, a block opening on `.UseAsyncApi(opts => …)`, `OnConflict = OnSchedulerConflict.Overwrite` alone | `AsyncAPISupport.md#6`, `AwsScheduler.md#23` |
| **A declaration and a statement in one fence, in the order C# forbids** | **10** | a class, then the line that registers it: `CS8803`. 9 of 10; the tenth is two constructor signatures with elided bodies | `HangfireScheduler.md#18`, `UsingTheContextBag.md#16` |
| **before/after pair in one fence** | **0** | all four of task 2.5's pairs parse and fail on binding | — |
| **a genuine fragment the wrapper mis-shaped** | **7 → 0** | fixed in this PR, below | `FAQ.md#16` |

**All 187 parse failures were re-staged under each of the four wrapper rules** (748 variants).
**7 parse under a rule `classify()` had not chosen**, all one shape: statements above a declaration,
which is a `Program.cs` and needs no wrapper. Wrapped as `types`, the leading statements land at
namespace level and cannot parse.

`classify()` gained a fifth shape, `toplevel`, with the empty wrapper:

```text
before   11 namespaced, 306 types, 136 members, 532 statements
after    11 namespaced, 9 toplevel, 303 types, 130 members, 532 statements
parse    187 broken  ->  180 broken          exactly the 7 the variant sweep predicted
verdict  0 blocks moved BUILT -> FAILED or FAILED -> BUILT
```

**The rule is order.** A statement *after* a declaration is `CS8803` unwrapped too, so
`UsingTheContextBag.md` block 16, a class then a line of usage, is not this shape and stays a page
defect. `plants/Plant_TopLevel.cs` and `Plant_TopLevel_Bad.cs` differ only by line order:

```text
Plant_TopLevel                        BUILT    0
Plant_TopLevel_Bad                    FAILED   1   CS8803
Plant_TopLevel, wrapped as `types`    FAILED  10   CS0106,CS0116,CS1002,CS1022,CS1026,CS1031,CS1520,CS8124
```

The third line is the red-proof: the same plant under the old rule, ten errors.

**Top-level blocks then earned `CS8805`**, *"Program using top-level statements must be an
executable"*, from `OutputKind.DynamicallyLinkedLibrary`, not from the page. The Roslyn half now
checks the tree for `GlobalStatementSyntax` and compiles those blocks as `ConsoleApplication`:
**`CS8805` on 15 blocks before, 0 after, no verdict changed.** It reads the tree rather than the
staged index's shape, because the tree is what the parser found.

### Claim and context *(task 2.3)*

**The claim list is 14 blocks across 12 pages, so phase 4 is a phase, not a spec.** `requirements.md`
P0-9 left this boundary unset. The context count is the rest.

**The split, measured:**

```bash
# 1. blocks carrying a diagnostic whose SHAPE is a claim about an API
grep -cE 'CS0117|CS1729|CS1061|CS0535|CS0115|CS1503|CS7036|CS0738|CS0311|CS0308' \
     tools/blockcheck/verdicts.tsv                                      # 69 blocks
# 2. the diagnostics in full, which is what tells a cascade from a claim
dotnet tools/blockcheck/bin/Release/net9.0/blockcheck.dll --explain <staged> <refs.txt> <id>...
```

| | Diagnostics | Blocks | What it is |
|---|---:|---:|---|
| **cascade of the wrapper** | 32 | 23 | the base type never resolved, so the base is `object` and the override reports `CS0117`/`CS0115`: *"'object' does not contain a definition for 'HandleAsync'"*. **Context** |
| **cascade of an unresolved name** | 22 | 20 | the named type, or an argument's type, failed to resolve in the same block. **Context** |
| **the page's own type, partly shown** | 22 | 8 | `CS0535` on `MyOutbox`, `CS1729` on a handler the page declares elsewhere. **Context** |
| **a library name** | 35 | 19 | the type resolved from a pinned package and the member or signature is not there. **The claim list's input** |

**Of the 19, eleven name a Brighter or Darker type and eight a third-party one:**

| Block | Diagnostic | Verdict |
|---|---|---|
| `Telemetry.md#1` | `InstrumentationOptions` has none of `RecordRequestInformation`, `RecordRequestBody`, `RecordRequestContext`, `RecordMessageInformation`, `RecordMessageBody`, `RecordMessageHeaders`, `RecordServerInformation` | **claim.** The released names are `RequestInformation`, `MessageBody`, `MessageHeaders` — the `Record` prefix is gone |
| `TurningOnReplayOnSeen.md#1`, `#6` | `OnceOnlyAction` has no `Replay`; and `contextKey:` takes `string?`, not `System.Type` | **claim**, twice in one attribute |
| `CausationTrackingStores.md#1`, `ReplayOnSeenReference.md#1` | `RequestContextBagNames` has no `CausationId` | **claim** |
| `CQRSWithBrighterAndDarker.md#7` | `RequestLoggingAttribute` has a required `timing` parameter the block omits | **claim** |
| `InMemoryOptions.md#2`, `#3` | `IAmACommandProcessor.ClearOutbox` has a required `posts`; `IAmAnOutbox` is not generic | **claim** |
| `InMemoryScheduler.md#4` | `IAmAMessageSchedulerFactory` cannot be the `T` the block passes it as | **claim** |
| `PostgreSQLMessageBroker.md#3` | a `RelationalDatabaseConfiguration` where a messaging-gateway configuration is wanted | **claim** |
| `Logging.md#4` | `INeedAHandlers` has no `Build` | **not a claim.** `// ... handler configuration, policies …` elides the chain steps between `StartNew()` and `Build()` |
| `QuartzScheduler.md#1`, `TickerQScheduler.md#1` | `IServiceProvider` has no `GetRequiredService` | **not a claim.** A missing `using`: two plants differing only by `using Microsoft.Extensions.DependencyInjection;` read **FAILED CS1061** and **BUILT** |
| `ConfiguringOpenTelemetry.md#1`, `#6`, `#7`, `HangfireScheduler.md#11`, `MigratingToPollyV8.md#1`, `#8` | `AddJaegerExporter`, `TracerProvider.Run`/`RunAsync`, `DashboardContext.GetHttpContext`, `int.Seconds` | **undecided: the pin's question.** No Jaeger exporter and no `Hangfire.AspNetCore` assembly is in the reference set |

**P0-9's input: 10 blocks of claim plus task 2.5's 4 fence pairs, 14 blocks across 12 pages.**

> **The claim list is a floor.** A block that fails to resolve its own base type never reaches the
> binding that would expose a false claim. `ImplementingAHandler.md` block 1 reports three
> diagnostics, `CS1729` on `Command` among them as a cascade; with its two omitted `using`
> directives it is **BUILT**, so no claim defect was hiding there. The ratchet is page by page, and
> **every block admitted to the baseline is one whose claims have actually been checked.** The 917
> failures cannot be triaged once, at the top, by a tool.

**The context count is 903 blocks** (917 less 14), dominated by `CS0246` (736 blocks) and `CS0103`
(675). That is backlog item 2, the 744 `using`-directive warnings `tools/README.md` row 2 counts.

### Q3 ruled by measurement — Darker joins *(task 2.4)*

**Yes.** Darker's packages restore alongside Brighter's with no conflict, and moved **7 blocks from
FAILED to BUILT and 0 the other way**:

```text
refs.txt   497 assemblies  ->  501       61 built  ->  68 built
gained     ImplementAQueryHandler#1 #2, PaginationQueryPatterns#1,
           ParameterizedQueryPatterns#3 #5, QueriesAndQueryObjects#2 #3
lost       none
```

**Four packages, not six.** `Paramore.Darker`, `.AspNetCore`, `.Policies` and `.QueryLogging` at
**4.1.1**. `requirements.md` P2-2's other two, `Paramore.Darker.Builder` and
`Paramore.Darker.Policies.Constants`, are a namespace and a type; pinning them would fail the
restore. Darker versions independently of Brighter, and the pin's comment says so.

`refs.csproj` is edited in phase 2, and **Darker's blocks enter phase 3's baseline like any
others.** 17 Darker-only pages carry 135 of the 985 blocks. P2-2 is promoted to in-scope, owned by
phase 3's baseline task.

### The before/after-in-one-fence set *(task 2.5)*

**Confirmed: 4 blocks across 2 pages.** The design said six across four:

| Block | Two methods | Verdict |
|---|---|---|
| `ImplementAQueryHandler.md#10` | `CS0101` **and** markers | **confirmed** — two `GetOrderQueryHandler` classes in one fence, the corpus's only `CS0101` |
| `CloudEventsSupport.md#7`, `#8` | `CS0128` **and** markers | **confirmed** — `var messageId` / `var correlationId` declared twice, V9 then V10 |
| `CloudEventsSupport.md#9` | markers only | **confirmed**; two method signatures with no bodies, so splitting yields two fragments (task 4.3a) |
| `AgreementDispatcherRouting.md#3` | markers only | **struck out.** `// Before Jan 2025` / `// After Jan 2025` are tax rules inside one routing lambda |
| `PolicyRetryAndCircuitBreaker.md#6` | markers only | **struck out.** The V9 form is commented out inside one live class, the shape `CLAUDE.md` prescribes |

**The design said the grep is the only instrument that sees five of the six; that is false.**
`CS0101` is duplicate types only; with the family `CS0101`/`CS0111`/`CS0128` the compiler sees
**21 blocks**, including `CloudEventsSupport.md#7` and `#8`. The compiler is the wider net:

```bash
grep -E 'CS0101|CS0111|CS0128' tools/blockcheck/verdicts.tsv | cut -f4   # 21 blocks
# markers, both vocabularies, comment-initial only:
#   ^\s*//+\s*(Before|V9|Old)\b  AND  ^\s*//+\s*(After|V10|New)\b        #  6 blocks
```

**Comment-initial markers return 6, of which reading keeps 4.** A wide vocabulary (any comment
mentioning *before*, *after*, *old*, *new*, ❌ or ✅) returns 18, including three tutorials whose
prose says *"before the call"*.

**The 21 the compiler finds are mostly other shapes:**

| | Blocks | Shape | P0-9? |
|---|---:|---|---|
| version pair, V9 → V10 | 3 (+1 marker-only) | the set above | **yes** |
| bad/good pair, ❌/✅ or `// Good`/`// Bad` | 9 | `QueryPatterns.md#2`, `NullableReferenceTypes.md#10`, `HangfireScheduler.md#25` … | **no** — a teaching device, nothing false claimed |
| problem/solution, or two alternatives | 7 | `AWSSQSMigrateToV10.md#5` `#6`, `BrighterSchedulerSupport.md#2` `#3`, `PostgreSQLBrokerTradeOffs.md#1` | **no** |
| duplicate for another reason | 2 | `FAQ.md#8`, `InMemoryOptions.md#2` | **no** |

**All 18 share the mechanism** (two mutually exclusive snippets in one fence) and the repair (one
fence each). **Recommendation, not decision:** P0-9 is scoped to defects of claim, so these 18 are
out unless the maintainer extends phase 4 by 18 mechanical splits.

**`AWSSQSMigrateToV10.md#1`, phase 1's finding, is in neither list**: a before/after pair with no
marker, found only by `--verify-extraction`'s duplicated `using`.

### The eight gates, reconciled *(task 2.6)*

Run at the end of the phase with `git add -A` first, every exit code read bare:

| # | Gate | Predicted | Read at the start | Read at the end | |
|---:|---|---|---|---|---|
| 1 | `linkcheck` | none | 165 files, 0 broken | **165 files, 0 broken** | ✅ |
| 2 | `pagelint` | none | 0 errors, 744 warnings, 162 pages | **0 errors, 744 warnings, 162 pages** | ✅ |
| 3 | shape | none | 161 pages, 12 sections | **161 pages, 12 sections** | ✅ |
| 4 | redirects | none | 77 entries, 7858 bytes | **77 entries, 7858 bytes** | ✅ |
| 5 | `versioncheck` | none | 0 stale pins of 18 | **0 stale pins of 18** | ✅ |
| 6 | `optioncheck` | none | 0 mismatches, 59 tables, 519 rows | **0 mismatches, 59 tables, 519 rows** | ✅ |
| 7 | `symbolcheck` | none | 0 findings, 22 entries, 3 silenced | **0 findings, 22 entries, 3 silenced** | ✅ |
| 8 | `--verify` | none | 161 predicted = 161 published | **161 predicted = 161 published** | ✅ |

**Eight for eight.** No `.md` was added under `tools/`.

### Frictions 57, 58 and 59 — for task 5.5, which writes the ledger

| | |
|---:|---|
| **57** | **A generated reference list survives the build that failed to produce it.** Adding Darker to `refs.csproj` left a double hyphen inside an XML comment, so the project failed to **load**; no target ran, the old `refs.txt` stayed, and the next corpus run reported the same **61 built**. It had measured the old pin. An absent reference list was already exit 2; a **stale** one was not, and deleting it first would not help, because a project that fails to load runs no targets. `refs.txt` now opens with `# refs.csproj SHA256 …`, and `blockcheck.py` compares it to the project's hash: wrong or missing stamp is exit 2 with no report file. Red-proofed both ways |
| **58** | **`zsh` does not word-split an unquoted variable, so a 68-argument list arrives as one argument.** `--explain … $ids` printed **nothing**, which is also what "no claim defects" looks like. Friction 52's shape in the reassuring direction. `${=ids}`, an array, or counting the arguments received |
| **59** | **A verdict nothing can emit reports zero of itself, and the zero reads as coverage.** `NOT COMPILABLE` is **0 of 985** (`design.md` Q4: *"a verdict that currently has no members"*; phase 1: *"by construction"*), yet **77 blocks are excerpts of a larger expression** and **93 carry an omission where C# needs a token**. They report `FAILED`, blaming the page for a fence that was never a program. `classify()` cannot return *no*. **For phase 3:** the mechanism is Q5's opt-out, with the reason written on the page by a person, because a heuristic guessing `NOT COMPILABLE` would silence real defects |

**Phase 2 changed the instrument in four places:** the fifth shape (`toplevel`), the output kind for
top-level statements, the `--parse` and `--explain` modes, and the pin stamp.
`--verify-extraction` still reports **985 of 985 identical**, and the seven plants read as
`plants/index.tsv` predicts.

---

## Phase 3 prediction

**All nine: none.** Task 3.9's *Notes*, approved at `1f5fc10` before phase 3 ran: **"`linkcheck` is
predicted to MOVE if and only if this phase adds a `.md` under `tools/`. The design chose not to. If
the reconciliation shows 166, the cause is a file somebody added without noticing the rule"**.

Predicted that no page under `contents/` is edited; measured **five pages edited, twelve lines, every
one an HTML-comment skip marker** (task 3.2), ruled site-neutral by the maintainer, so no sign-off
was owed. No rule of `pagelint`, `symbolcheck`, `optioncheck` or `versioncheck` reads an HTML
comment as prose, so none of them can move; `SUMMARY.md`, `.gitbook.yaml` and the published tree are untouched, so shape, redirects
and `--verify` cannot. `linkcheck` walks `tools/`; this phase adds a `.tsv` and a `.json`, edits a
`.py` and a `.yml`, and adds prose to `tools/README.md`, already in its 165.

**Row 9 predicts its first figure, not a movement.** Task 3.7 records it with the ref it was
measured at (obligation 10).

The before-figures are `tools/README.md`'s at the top of the phase.

---

## Phase 3 as executed

### The opt-out, and why this one has to say why *(task 3.1)*

**Q5 is ruled: `<!-- blockcheck: skip <reason> -->`, on its own line, binding the next C# block, with
the reason part of the syntax.** The line placement and the one-block binding follow this
repository's existing opt-outs; the mandatory reason does not.

`pagelint`'s `<!-- pagelint: allow-serviceactivator -->` and `symbolcheck`'s
`<!-- symbolcheck: allow IMessageScheduler -->` carry no reason because each names what it silences,
and the surrounding paragraph discusses that name. A block that fails to compile can fail for many
reasons, none recoverable from the marker, and `design.md` (friction 59) requires that *"the reason
has to be written on the page by a person, never guessed by a heuristic"*. A marker without a
reason binds nothing and is reported.

**Page-wide was rejected**, on `symbolcheck`'s argument: its `opt_outs` docstring says a page-wide
silence *"would let a page opting out of a name it discusses on purpose silently opt out of a
second, dead name that arrived on that page two years later, and nothing would ever say so"*. A
page-wide `skip` for one ❌ V9 example would absorb the next block added beneath it. The marker
binds one block.

**Three ways a marker binds nothing:**

| | Reported as | Why |
|---|---|---|
| no reason given | **error**, a finding, exit 1 | It grants no opt-out, and the block is **still judged** |
| no C# block follows it | warning | Dead weight |
| that block already has a marker | warning | Two reasons, no way to tell which the tool used |

#### Red-proofed five ways against the real code path, none of them a page under `contents/`

Five synthetic pages built through the real `pagelint.Page` and the real `enumerate_blocks`:

```text
binds                blocks=1 skip=['V9 form, required by CLAUDE.md']  problems=[]
no-reason            blocks=1 skip=[None]  problems=[('no reason given', 4)]
binds-nothing        blocks=1 skip=[None]  problems=[('no C# block follows it', 8)]
two-markers          blocks=1 skip=['first reason']  problems=[('that block already has a marker', 5)]
control-no-marker    blocks=1 skip=[None]  problems=[]
```

The control, a page with no marker, produces no binding and no problem. The positive case passes.

#### And end to end, over the whole corpus, four runs

`contents/MigratingToPollyV8.md` block 5, line 97, the `❌ **V9 — superseded**` block, a real member
of task 3.2's set:

```text
run 1  control, unmodified      985: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE   exit 0
                                0 findings
       the block                FAILED  contents/MigratingToPollyV8.md  5  ...  2  CS0103

run 2  marker with a reason     985: 68 BUILT, 916 FAILED, 1 SKIPPED, 0 NOT_COMPILABLE   exit 0
                                ----- skipped by opt-out (1) -----
                                contents/MigratingToPollyV8.md:98  block 5 — V9 form, required
                                  by CLAUDE.md § Version markers on code
                                0 findings, 1 skipped

run 3  the SAME marker, reason  985: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE   exit 1
       removed                  ----- malformed opt-out (1) -----
                                contents/MigratingToPollyV8.md:96  no reason given —
                                  <!-- blockcheck: skip -->
                                1 findings

run 4  reverted                 985: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE   exit 0
                                0 findings          985 of 985 rows identical to run 1
```

In run 3 the reasonless marker leaves the block `FAILED` and counted, and the run fails. Run 4:
`git checkout -- contents/MigratingToPollyV8.md`, `git diff --stat` on that path empty, and run 4's
report identical to run 1's across all 985 rows. `0 findings` and `0 findings, 1 skipped` print as
different strings (ruling 4). The four verdicts sum to 985 in every run (AC1).

#### Two things this task changed beyond the marker

**`VERDICTS`' comment was false and is corrected.** It read *"SKIPPED has no opt-out to carry it
until phase 3"*. It now also records `NOT_COMPILABLE` as **empty by construction, a defect (friction
59), not coverage**.

**`enumerate_blocks` now filters the page's C# fences once and enumerates them**, replacing a manual
`ordinal` counter, because the skip scan needs the C# fence start lines before the blocks are built.
Behaviour is unchanged: `--list` reads **985 C# blocks across 145 pages: 11 namespaced, 9 toplevel,
303 types, 130 members, 532 statements**, identical to phase 2 in all five shapes.

### The V9 skips, and why the design's 8 was the wrong 8 *(task 3.2)*

**The set is 12 blocks across 5 pages. The design said 8, and its 8 was a different set.**

```text
grep -rn '^❌' contents/   ->    8 lines, 3 pages     the design's figure, reproduced
grep -rn '❌'  contents/   ->   43 lines, 11 pages
```

**Of the anchored 8, one is a version marker**: `MigratingToPollyV8.md:95`, `❌ **V9 — superseded**`.
The other seven are `❌ Bad:` in `QueryPipeline.md` (5) and `CQRSWithBrighterAndDarker.md` (2), the
bad/good convention, not `CLAUDE.md` § *Version markers on code*. **None of the unanchored 35 is a
version marker**: they are table cells (`BrighterSchedulerSupport.md`, `FAQ.md`), pro/con bullets
(`EFCoreQueryIntegration.md`, `PaginationQueryPatterns.md`, `FAQ.md`), or trailing comments inside a
fence (`AWSSQSMigrateToV10.md`, `KafkaConfiguration.md`, `QueryPatterns.md`,
`TurningOnReplayOnSeen.md`, `EFCoreQueryIntegration.md`).

**A third method, the label directly above each C# fence, finds eleven V9 forms with no ❌**, on
pages that predate the convention and label in their own words: `**Before (V9)**:`, `**V9**:`,
`**Old (V9):**`, `### V9 Configuration (Deprecated)`, `### Example with Legacy Policies (V9)`. The
task's Notes, *"`CLAUDE.md` requires these blocks to exist"*, apply to them as much as to the ❌ one.

| Page | Blocks | Label on the page |
|---|---|---|
| `MigratingToPollyV8.md` | #1, #3, #5 | `**V9**:` twice; `❌ **V9 — superseded**` |
| `V10MigrationGuide.md` | #1, #4, #6, #8, #19, #15 | `**Before (V9)**` ×5; `Before:`, a `Guid` request Id, which the prose above it names as V9 |
| `FAQ.md` | #15 | `**Old (V9):**` |
| `ReactorAndProactor.md` | #11 | `### V9 Configuration (Deprecated)` |
| `PolicyFallback.md` | #2 | `### Example with Legacy Policies (V9)` |

**Three candidates are excluded:**

- `V10MigrationGuide.md` #23, `**Before**:` under KIP-848: a V10 API marked `[Obsolete]`, which the
  page says *"still work"*. A V10 block with a warning, so the gate judges it.
- `MigratingToPollyV8.md` #7–#11, under `### Using Brighter's UsePolicy Attribute (Legacy)`: the page
  presents these as legacy V10 usage, not V9.
- `AWSSQSMigrateToV10.md` #2, `**V3 Approach**`: the AWS SDK's v3, used through a Brighter V10 package.

**The rule is the page's label, not whether the API was removed.** Four of the twelve use APIs V10
still ships: `FAQ.md` #15 and `MigratingToPollyV8.md` #3 use `TimeoutPolicy`, `PolicyFallback.md` #2
uses `UsePolicy` and `FallbackPolicy`, and `MigratingToPollyV8.md` #1 uses a Polly v7
`PolicyRegistry`; `UsePolicy` is marked `[Obsolete("Migrate to UseResiliencePipeline")]`. The other
eight are described by their pages as removed or changed surfaces (`isAsync`/`runAsync` on
`Subscription`, a `Guid` request Id) and were not each checked against source, so an API-removal
rule would give **at most 8**. The label rule gives 12 and can be checked against the page. Its
cost: those four could be scaffolded to compile, and the skip means the gate will not notice if they
stop doing so. Open for the maintainer.

**All 12 were `FAILED` before the markers went in; none was `BUILT`** (run 0).

#### The run

```text
run 0  control, unmodified   985: 68 BUILT, 917 FAILED,  0 SKIPPED, 0 NOT_COMPILABLE   exit 0
                             0 findings
run 1  twelve markers        985: 68 BUILT, 905 FAILED, 12 SKIPPED, 0 NOT_COMPILABLE   exit 0
                             ----- skipped by opt-out (12) -----   each with its reason
                             0 findings, 12 skipped
```

**Run 0 against run 1: exactly 12 rows changed, every one `FAILED → SKIPPED`**, page and ordinal
unchanged on all 985. `git diff --stat` reads **5 files, 12 insertions, 0 deletions**. Every reason
names the label the page uses.

**Seven of eight gates re-run after the edit, all at `tools/README.md`'s figures:** `linkcheck`
165/0; `pagelint` 0 errors, 744 warnings, 162 pages; shape 161/12/12-of-20/4-of-4; redirects 77
entries, 7858 bytes; `versioncheck` 0 of 18 across 5; `optioncheck` 0 across 59 tables, 519 rows;
`symbolcheck` 0 findings, 22 entries, 161 pages, 3 silenced. `--verify` was not run: nothing it reads
changed.

**No ❌ was added to the eleven.** That is a visible page change, for phase 4 or the backlog.

### The scaffold, and the line AC13 will be read against *(task 3.3)*

**68 → 92 BUILT: +24 blocks across 13 pages, from 12 new units. No block moved the other way, no
error landed on a scaffold tree, and every one of the 100 identifiers `--list-scaffold` prints is
named by a block that builds with it.** No prelude was added.

#### What the scaffold could be for: measured, not guessed

Every one of the 905 failing blocks run through `--explain`, and every missing name looked up in a
dump of **27,921 public types** from the 501 pinned assemblies (`System.Reflection.Metadata` over
`refs.txt`, in a scratch project, not committed):

| | Blocks | What it means |
|---|---:|---|
| **import** | 364 | a missing name **is** a pinned type: the page needs a `using`. Not scaffoldable; phase 4 or backlog item 2 |
| **other** | 342 | a diagnostic that is not a missing name: parse, claim, cascade |
| **context** | **199** | every error is a missing name, and none of them is a type anything ships |

**The 199 split three ways; only the first is admitted:**

| | Blocks | Example | This task |
|---|---:|---|---|
| **values only**, lower-case or `_` names | **76** across 26 pages | `services`, `builder`, `commandProcessor`, `resiliencePipelineRegistry` | **scaffolded** |
| **a type the page names and never shows** | 102 | `StandardHandler`, `OrderStatus`, `IPersonRepository` | **not scaffolded: the maintainer's call at AC13** |
| **a type another block on the same page declares** | 21 | `GreetingCommand` in the tutorials | **not scaffolded**: a copy of the page's own type recreates obligation 8's leak |

**The middle row is AC13's question.** Design rule 1 reads *"it may not define a type the page tells
the reader to write"*, and whether a handler the page routes to and never prints falls under it is
the maintainer's reading. If it is read as identifiers, it is the next tranche and needs no new
mechanism.

#### The rule the 76 were held to, and what it excluded before anything ran

**A value is typed from a pinned package or the BCL, returns a default, and does nothing.**

- **No `dynamic`.** A `dynamic` value would compile every member access on it.
- **A value needing a type nothing ships is excluded.** `entity` and `_repository` in
  `HangfireScheduler.md#22` / `QuartzScheduler.md#19` need a domain type. Quartz's `q` and `store`
  (8 blocks) need `IServiceCollectionQuartzConfigurator`, which is **not in the pin**, the same pin
  question as the Jaeger and `Hangfire.AspNetCore` rows in phase 2's claim table.

#### What the 76 did, with context supplied

```text
76 value-only candidates
   10 excluded by the rule above, before the run
   24 BUILT                                    <- admitted
   42 still FAILED, now for a reason the missing name was hiding:
        38  CS1061 / CS0246 on an extension method or type the block never imported,
            or one from a package the pin does not carry (AddBrighter, AddHangfireServer,
            AddTickerQ, AddHttpClient, AddOpenTelemetry, AddMsSqlOutbox, AddCircuitBreaker)
         4  CLAIMS, new since phase 2's list
```

**Four new claims**, which read `CS0103` unscaffolded:

| Block | Diagnostic | What the page says that is not so |
|---|---|---|
| `FAQ.md#18`, `#19` | `CS1503`: argument 2 cannot convert `TimeSpan` to `RequestContext?` | `commandProcessor.SendAsync(command, delay)`: arguments in the wrong order |
| `FAQ.md#19` | `CS1061`: no `RescheduleAsync` on `IAmAMessageSchedulerAsync` | the method is `ReSchedulerAsync`, as `TickerQScheduler.md#6` spells it |
| `AzureScheduler.md#18` | `CS1061`: no `ReScheduleAsync` | the block, commented *"Won't work!"*, calls a method that does not exist |
| `SweeperCircuitBreaking.md#9` | `CS7036`: `IAmAnOutboxProducerMediator.ClearOutboxAsync` requires `requestContext` | the call omits a required argument |

**They are input to task 4.1, not adopted rows.** The FAQ unit that exposed them is not committed,
because FAQ admits no block; phase 4 re-adds it when it repairs the page (task 4.4).

#### The trim, and why it did not move the result

The first run mapped 26 pages to 20 units. **13 pages admitted nothing** (every candidate hit a
missing `using`), so their units and rows came out, and members used only by still-failing blocks
were deleted. Re-run: **the BUILT set is identical, 92 of 92 by `diff`.**

```text
python3 tools/blockcheck.py --list-scaffold    ->  100 identifiers from 13 unit(s) and 4 prelude(s);
                                                   14 page(s) scaffolded
find tools/blockcheck/scaffold -type f | wc -l ->  18    13 units + 4 preludes + the map
```

**The listing is 114 lines**: 100 identifiers plus one injected `using static` per scaffolded page.

#### Red-proof: a scaffolded value does not buy a verdict

`TickerQScheduler.md#6` is admitted on a unit that supplies `_scheduler` as
`IAmAMessageSchedulerAsync`. One character changed on the page, `ReSchedulerAsync` → `ReScheduleAsync`:

```text
control      TickerQScheduler_6   BUILT   0
broken       TickerQScheduler_6   FAILED  1  CS1061       corpus 91 BUILT
reverted     TickerQScheduler_6   BUILT   0               985 of 985 rows identical to the control
```

**The value is typed, so the member is checked.** `git diff --stat contents/` is empty after the
revert. The broken run exits **0** because there is no baseline until task 3.5; task 3.6 proves the
exit code.

#### One instrument quirk, recorded and not fixed

`--explain` prints `985 blocks, 0 built, 985 failing` on stderr whatever it is asked: blocks it was
not asked about fall through uncounted. The stdout rows are right. A one-line fix for the next time
`Program.cs` is opened.

**`tools/blockcheck/verdicts.tsv` still reads 68 BUILT, deliberately.** It is phase 2's committed
corpus run, and no tool reads it (`grep` over `tools/*.py`, `tools/README.md` and `.github/`).
`baseline.tsv` carries the current admitted set.

### The baseline *(task 3.4)*

**`tools/blockcheck/baseline.tsv`: 92 rows across 44 pages, measured at `280d1b7`, no page edited
to earn one.** Columns: page, ordinal, scaffold, and the ref it was admitted at, stated in the file's
header.

**The population is the whole BUILT set.** The task text names one direction of AC9, a row whose
block has gone; AC9's instrument (*"delete one line from `baseline.tsv` and confirm exit 1"*) and
P0-4 (*"when the list disagrees with the corpus in either direction"*) name the other. So a block
that builds and has no row is a failure, and a repair that makes a block build brings its row in the
same PR. Task 3.5 implements all three conditions.

**There is no mode that writes it.** It was generated by filtering `--report` rows to `BUILT` and
taking the scaffold name from `load_scaffold()` as `--list-scaffold` prints it. A change to this file
is a diff someone reads.

| | Rows |
|---|---:|
| no scaffold | 46 |
| a scaffold, and it is **needed** — 24 from task 3.3, plus `DapperOutbox.md#1` on phase 1's `PageContext` | 25 |
| a scaffold present on the page and **not needed**, BUILT before any unit existed | 21 |
| **total** | **92** — equal to the corpus run's `BUILT` count |

**The 21 are recorded as compiled, not as needed**: the column says what the block was compiled
with.

**`MigratingToNullableReferenceTypes.md` carries 17 rows**, the most of any page: one-line
nullable-annotation illustrations, on a unit that supplies one `string`.

### The ratchet *(task 3.5)*

**`--report` is now the gate.** It holds the corpus to `baseline.tsv` and reports four
disagreements, each under its own heading:

| Heading | Means | Fixed in |
|---|---|---|
| `stopped building` | a listed block is `FAILED`, `NOT_COMPILABLE` or **`SKIPPED`** | the page |
| `baselined block no longer exists` | a row names a page/ordinal the corpus does not have | `baseline.tsv` |
| `builds and is not baselined` | the ratchet. The line printed is the row to add | `baseline.tsv` |
| `scaffold changed since admission` | the block builds, but with a different scaffold than it was admitted on | either |

*Builds and is not baselined* is AC9's instrument and P0-4's *"either direction"*. *Scaffold changed*
follows from the column: a row claims the block builds given that scaffold. **`SKIPPED` counts as
stopped building**, so a marker on the page cannot take a block out of the gate without editing
`baseline.tsv`.

**A missing or malformed baseline is exit 2** (condition 11): an absent file, a row without four
fields, a non-numeric ordinal, or **a block listed twice**, which would let one row be deleted
unnoticed.

**A failing block with no row is not a finding**: that is the 881-block debt. The `no baseline yet`
line is replaced by `baseline: 92 blocks required to build`, and the docstring's *"does not yet
gate"* paragraph is rewritten.

### The red-proof *(task 3.6)*

**Every condition, each exit code read bare, every edit reverted**, on `TickerQScheduler.md#6`, a
baselined block admitted on a scaffold:

```text
1  baselined block broken    ReSchedulerAsync -> ReScheduleAsync   exit 1   stopped building: FAILED CS1061
2  phantom row                block 99 appended                    exit 1   baselined block no longer exists
3  a row deleted (AC9)        block 6's row removed                exit 1   builds and is not baselined + the row to add
4  scaffold column wrong      TickerQSchedulerContext.cs -> -      exit 1   scaffold changed since admission
5  skip marker above it       <!-- blockcheck: skip … -->          exit 1   stopped building: SKIPPED by an opt-out
6  duplicate row              first row appended again             exit 2   "is already listed at line 22"
7  baseline absent            file moved away                      exit 2   "no baseline at …: nothing was checked"
8  CONTROL, all reverted                                          exit 0   0 findings, 12 skipped
                              985 of 985 rows identical to the pre-red-proof run;  git status: tools/blockcheck.py only
```

**Runs 5 and 6 each needed a second attempt:** a marker inserted by line number landed inside the
fence (friction 62), and `grep … baseline.tsv >> baseline.tsv` appended nothing (friction 61). Both
were re-run correctly; the table shows the valid runs.

**One defect fixed:** run 5 printed `SKIPPED SKIPPED`. It now reads *"SKIPPED by an opt-out, which
cannot excuse a baselined block — remove the marker or the row"*.

**The other modes did not move:** `--list` reads 985 across 145 in the same five shapes;
`--verify-extraction` reads 985 of 985 identical; the bare invocation exits 2.

### Row 9 *(task 3.7)*

**`tools/README.md` row 9: `python3 tools/blockcheck.py --report`, its figure, measured at
`1e1944d`**, plus the heading and opening line changed to *nine*, the CI-placement list, the scope
paragraph, the other modes, a `--baseline`-flag sentence beside `symbolcheck`'s `--watchlist` one,
and a *what it checks* entry followed by the AC15 sentence: a green `blockcheck` means the listed
blocks compile, not that they are right.

**`git grep -i -E '\beight\b'` returned nothing**, with `tools/README.md:3` reading *"Eight
commands"*: git's ERE has no `\b` (friction 60). `git grep -w` found **seven** hits outside `spec/`:

| Where | Changed? |
|---|---|
| `tools/README.md` ×3: opening line, heading, *"Three of the eight"* | **yes**: nine, and *four of the nine* |
| `.claude/commands/spec/review.md`: *"All eight run here"*, *"Two of the eight…"*, *"same for all eight"* | **yes** |
| `.claude/commands/spec/review.md:117`: *"`/spec:review` runs eight gates"* | **no**: it quotes 014's AC5 verbatim |
| `.claude/commands/spec/design.md`: *"`tools/README.md` has the eight"* | **yes** |
| `contents/MSSQLMessageBroker.md` ×2, `PostgreSQLMessageBroker.md` | **no**: eight *options* |

**`/spec:review` gains a `### blockcheck` section** that builds both projects and runs `--report`,
plus three `allowed-tools` grants. It filters the twelve skip lines (they carry ` block N — `, which
no finding line does) so a finding cannot fall out of its `tail`. Run as written, it prints the
scope, `skipped by opt-out (12)`, `baseline: 92 blocks required to build` and the verdict.

#### AC10, and the instrument's own defect

```text
grep -rn '92 BUILT, 881 FAILED' --include='*.md' --include='*.yml' --include='*.py' . > /tmp/f
grep -vcE '^(\./)?spec/' /tmp/f      ->  1    tools/README.md:52, row 9
same for '985 blocks'                 ->  2    tools/README.md and a comment in tools/blockcheck.py   <- the control
```

**AC10's instrument as `requirements.md` writes it, `grep -vc '^./spec/'`, excludes nothing on
macOS**: BSD `grep -rn … .` prints `spec/016…` without the `./`. With it, the control read **3**, two
of them inside `spec/`. `^(\./)?spec/` matches both forms. Recorded as a defect in an approved
criterion, not edited there; phase 5 should use the working form.

**`linkcheck` reads 165 files, 0 broken**: row 9 went into a file already in its corpus.

### The CI job *(task 3.8)*

**`.github/workflows/docs.yml` gains a `blocks` job:** checkout, `setup-dotnet` 9.0.x,
`setup-python` 3.12, the two builds as their own steps, then
`python3 tools/blockcheck.py --report "$RUNNER_TEMP/blockcheck.tsv"`, with no guard, no `|| true`,
and rows to a file, so the step's exit code is the tool's.

**Q8 — RULED: no scheduled run, via `if: github.event_name != 'schedule'`.** **Q9 — RULED:
pinned**; all 71 `PackageReference`s in `refs.csproj` already name one exact version.

**`schedule:` is declared once, at the top of the file, and triggers every job without an event
filter** (friction 63). Scheduled run `35722167211` ran **`check`, `versions` and `options`**, all
`success`. **The `options` job's comment says *"NO `schedule:` TRIGGER either"*, and it runs
daily.** Its pinned verdict repeats, so nothing has failed. Not fixed here, as it changes an existing
gate's triggers: one line, `if: github.event_name != 'schedule'` on `options`, the maintainer's call.
The `blocks` job's comment cites it.

**Replayed on a clean checkout (obligation 12):** `git ls-files` through `tar` into a scratch
directory with no `bin/`, both builds, the gate invocation as written. **Exit 0, `baseline: 92
blocks required to build`, `0 findings, 12 skipped`, 985 of 985 rows identical to the in-tree run.**
The workflow parses (Ruby's YAML: four jobs, the `if:` and six steps as written). The job on GitHub's
Linux runner, with a cold NuGet cache, is the PR's first run and goes in task 3.9.

### The nine gates, reconciled *(task 3.9)*

**All nine at `ecefa13`, every exit code read bare. The prediction was *none* for the eight existing
gates, and none moved.**

| # | Gate | Predicted | Read at `ecefa13` | |
|---:|---|---|---|---|
| 1 | `linkcheck` | none — **moves iff a `.md` is added under `tools/`** | 165 files, 0 broken | ✅ none added |
| 2 | `pagelint` | none | 0 errors, 744 warnings, 162 pages | ✅ the skip markers reach no rule |
| 3 | shape | none | 161 / 12 sections / 4 of 4 / 12 of 20 | ✅ |
| 4 | redirects | none | 77 entries, 7858 bytes | ✅ |
| 5 | `versioncheck` | none | 0 stale of 18, across 5 | ✅ |
| 6 | `optioncheck` | none | 0 mismatches, 59 tables, 519 rows | ✅ |
| 7 | `--verify` | none | 161 predicted = 161 published | ✅ reached the live sitemap |
| 8 | `symbolcheck` | none | 0 findings, 22 entries, 161 pages, 3 silenced | ✅ |
| 9 | `blockcheck` | **its first figure, not a movement** | row 9's figure, 0 findings, 12 skipped | ✅ equal to row 9 |

The before-figures are `tools/README.md`'s rows at the top of the phase; rows 1–8 are unedited in
this phase's diff of that file.

**The prediction said *"No page under `contents/` is edited"*; task 3.2 put twelve HTML-comment lines
on five pages**, ruled site-neutral by the maintainer.
Rows 2 and 8, the two that could have seen them, did not move.

#### Phase 3 in one table

| | |
|---|---|
| **The corpus** | 985 blocks: 92 BUILT, 881 FAILED, 12 SKIPPED, 0 NOT_COMPILABLE (row 9 owns the figure) |
| **The gate** | `baseline.tsv`, 92 rows across 44 pages. Four findings, both directions, red-proofed seven ways |
| **The scaffold** | 13 units, 4 preludes, 100 identifiers, 14 pages. Values only, typed from the pin |
| **For the maintainer** | the 102 blocks needing an unshown type (AC13); the `options` job's schedule; AC10's `^./spec/` under BSD grep; the label-vs-API rule for the V9 skips |
| **For phase 4** | four new claims found by scaffolding: `FAQ.md#18`, `#19`, `AzureScheduler.md#18`, `SweeperCircuitBreaking.md#9` |

---

## Phase 4 prediction

**Written after the page edits, before gates 1 and 3–8 were run.** `pagelint` and `blockcheck` had
already been run to build the repairs, so their rows are readings, not predictions.

| # | Gate | Predicted | Why |
|---:|---|---|---|
| 1 | `linkcheck` | none | no link added or retargeted; no `.md` under `tools/` |
| 2 | `pagelint` | **moves**, direction down | *read, not predicted:* 744 → 743 |
| 3, 4, 7 | shape, redirects, `--verify` | none | `SUMMARY.md` and `.gitbook.yaml` untouched; no heading changed |
| 5 | `versioncheck` | none | no prose pin edited |
| 6 | `optioncheck` | none | the one table added (`Telemetry.md`'s flags) carries no `optioncheck` marker |
| 8 | `symbolcheck` | none | no watchlisted name added; `UsePolicy` removed from two blocks |
| 9 | `blockcheck` | **moves** | *read, not predicted:* +4 blocks from the splits, +4 skips, +9 BUILT and +9 rows |

---

## Phase 4 as executed

### The claim list, re-derived *(task 4.1)*

**At `master` = `a194c4a`: 18 candidate blocks across 13 pages; 14 across 10 kept.** Method 1 is
phase 2's claim-shaped diagnostic grep over a fresh `--report`, diffed against `verdicts.tsv`.
Method 2 is `--explain` on every entrant.

```bash
P='CS0117|CS1729|CS1061|CS0535|CS0115|CS1503|CS7036|CS0738|CS0311|CS0308'
grep -cE "$P" tools/blockcheck/verdicts.tsv    # 69   phase 2
grep -cE "$P" /tmp/r0.tsv                      # 72   this HEAD
diff <(grep -E "$P" verdicts.tsv | cut -f4) <(grep -E "$P" r0.tsv | cut -f4)
#   - MigratingToPollyV8_1, V10MigrationGuide_15        now SKIPPED, task 3.2
#   + AzureScheduler_18                                  claim, exposed by phase 3's scaffold
#   + MigratingToPollyV8_12, SweeperCircuitBreaking_2 _5 _7   CS1061 on AddTimeout/AddSingleton/AddBrighter:
#                                                        a missing `using`, so context
```

All ten of phase 2's claims still carry their diagnostic, and phase 3's four were confirmed with the
FAQ unit restored. Four were struck out:

| Block | Why it is not a defect |
|---|---|
| `TurningOnReplayOnSeen.md#1`, `#6` | `OnceOnlyAction.Replay` is on Brighter `master`, not at 10.7.0, and the page says so: *"Not in a released package yet."* Phase 2's second diagnostic, *"`contextKey:` takes `string?`, not `System.Type`"*, is a cascade: 10.7.0 has a `UseInboxAttribute(int, Type, …)` overload, which fails only because `Replay` does not bind |
| `CausationTrackingStores.md#1`, `ReplayOnSeenReference.md#1` | `RequestContextBagNames.CausationId` is on `master`, not at 10.7.0; both pages carry the same banner |

They stay FAILED, not skipped, so that when the pin moves past 10.7.0 and they build, the ratchet
demands their rows.

Two of phase 2's figures were wrong:

- *"14 blocks across 12 pages"*: the 14 blocks are on **10** pages, 8 for the claims and 2 for the
  fence pairs.
- *"The released names are `RequestInformation`, `MessageBody`, `MessageHeaders` — the `Record`
  prefix is gone"*: `InstrumentationOptions` is a **`[Flags]` enum**. `MessageBody` and
  `MessageHeaders` do not exist, and neither does `BrighterInstrumentation`, the type the block
  reads it from.

### The fence pairs *(task 4.2)*

| Block | Now | Labels |
|---|---|---|
| `ImplementAQueryHandler.md#10` | #10 and #11 | **Synchronous:** / **Asynchronous:**, no ❌/✅ (review finding 1) |
| `CloudEventsSupport.md#7`, `#8`, `#9` | #7–#12 | ❌ **V9 — superseded** / ✅ **V10 — current** |

Items 1 and 2 said *"Changed from `Guid` to `string`"*. At 10.7.0 `MessageHeader.MessageId` and
`.CorrelationId` are **`Id`**, which converts implicitly from `string`. The prose and both V10
halves now say `Id`, and both halves build.

### The repairs, page by page *(task 4.3)*

**From the list:**

| Block | Claimed | True at 10.7.0 |
|---|---|---|
| `Telemetry.md#1` | `BrighterInstrumentation.InstrumentationOptions.CommandProcessorInstrumentationOptions = new InstrumentationOptions { RecordRequestInformation = true, … }` | `InstrumentationOptions` is a flags enum, set on `BrighterOptions` through `AddBrighter` and on `ProducersConfiguration` through `AddProducers`. Block rewritten; the flags table beside it is read from `BrighterTracer.cs` |
| `CQRSWithBrighterAndDarker.md#7` | `[RequestLogging(step: 1)]`, `[UsePolicy(…)]` on `HandleAsync` | `RequestLoggingAttribute` requires `timing`; both are sync attributes on an async handler, which Brighter rejects at pipeline build; `UsePolicy` is `[Obsolete]`. Now `RequestLoggingAsync` and `UseResiliencePipelineAsync`, matching the page's `ResiliencePipelineRegistry` |
| `InMemoryOptions.md#2` | `ClearOutboxAsync()` with no ids; a field declared without a type; `.UseInMemoryArchiveProvider()`; `Assert.Any` | Rewritten and **run** (below) |
| `InMemoryOptions.md#3` | `IAmAnOutbox<Message, CommittableTransaction>`; `.UseOutbox(…)`; `UseScheduler` given an `IAmAMessageSchedulerFactory`; `new HangfireMessageSchedulerFactory(connectionString)` | `IAmAnOutbox` is not generic; there is no `UseOutbox`, the Outbox is `ProducersConfiguration.Outbox`; `UseScheduler<T>` needs `T` to be **both** factory interfaces; Hangfire's factory has no such constructor. `publication` was used without being declared |
| `InMemoryScheduler.md#4` | the same helper, and the same Hangfire constructor | branches at the call site on concrete types. Now fails only on `args` (below) |
| `PostgreSQLMessageBroker.md#3` | `new PostgresChannelFactory(RelationalDatabaseConfiguration)` | takes a `PostgresMessagingGatewayConnection`, as `PostgreSQLTransportAndOutbox.md` says |
| `FAQ.md#18`, `#19` | `SendAsync(command, delay)`; `RescheduleAsync` | `SendAsync(delay, command)`; `ReSchedulerAsync`. `scheduler` is named as the `IAmARequestSchedulerAsync` |
| `AzureScheduler.md#18` | `ReScheduleAsync` *"Won't work!"* | `ReSchedulerAsync` exists and **returns `false`** in `AzureServiceBusScheduler`; the comment now says so |
| `SweeperCircuitBreaking.md#9` | `postBox.ClearOutboxAsync(messageIds)` | `postBox` is the internal mediator, whose overload requires a `requestContext`. Now `commandProcessor.ClearOutboxAsync(messageIds)`, as block 5 of the page writes it |

**The same falsehood elsewhere,** repaired wherever a grep found it again:

| Where | Falsehood |
|---|---|
| `Telemetry.md#4` | the same invented API, plus `UseCloudEventsConventionsAttributes`, which does not exist. CloudEvents attributes are recorded under `RequestInformation`; there is no switch |
| `ConfiguringOpenTelemetry.md#6`, `#7` | the same invented API. Their Jaeger and `TracerProvider.Run` diagnostics remain phase 2's undecided pin question, untouched |
| `CQRSWithBrighterAndDarker.md#2` | the same sync-on-async attributes |
| `CQRSWithBrighterAndDarker.md#2`, `#6` | `PlaceOrderCommand : IRequest` never implements `Id` or `CorrelationId`. Now `: Command` with `base(Id.Random())`. **#6 builds** |
| `DarkerAndBrighterPipelines.md#1` | sync attributes on an async handler, and `HandleAsync` returning `Task<AddGreetingResponse>` |
| `BrighterSchedulerSupport.md#5`, `SchedulingAMessage.md#7` | `new HangfireMessageSchedulerFactory(connectionString: …)`, and a `scheduler:` argument name `UseScheduler` does not have |
| `SchedulingAMessage.md#8`, `#9` | `QuartzMessageSchedulerFactory` does not exist; it is `QuartzSchedulerFactory(IScheduler)`. `scheduler:` again |
| `OutboxArchiver.md#3` | `UseOutboxArchiver(provider)` has no non-generic form. Now `<CommittableTransaction>` |
| `FAQ.md#17` | a stray `)`: the block did not parse |
| two ```` ```text ```` diagrams | `SendAsync(command, delay)` in `BrighterSchedulerSupport.md` and `InMemoryScheduler.md` |

#### `InMemoryOptions.md#2`, run with a control

**The rewritten block compiled and still carried two defects.** Built and run in a scratch project
against the 10.7.0 packages, tests called directly:

```text
as first rewritten           ConfigurationException: You must set a message pump type
+ messagePumpType            PASS publish     FAIL schedule: More than one handler was found for FireSchedulerRequest
- AutoFromAssemblies()       PASS publish     PASS schedule          <- what the page now shows
control: no RequestType      FAIL publish: No producer found for request type    <- the page's original Publication
```

The schedule test is its own control: `Assert.Empty` before the delay, `Assert.NotEmpty` after. The
original's second test asserted that a scheduled **`SendAsync`** puts a message on the bus; it fires
the command at a local handler instead. It is now a scheduled **`PostAsync`**.

**The duplicate handler is a Brighter 10.7.0 defect:**

```text
services.AddBrighter().AutoFromAssemblies()        FireSchedulerRequestHandler, FireSchedulerRequestHandler
services.AddBrighter()   (control, no scan)        FireSchedulerRequestHandler
```

Any DI application that calls `AutoFromAssemblies()` and schedules a request through the InMemory
scheduler throws at fire time. The page's test registers no handlers and omits the call, with a
comment. Filed upstream as BrighterCommand/Brighter#4414.

### The opt-outs *(task 4.3a)*

**12 → 16 skipped, +4, one per marker added:** the three ❌ halves in `CloudEventsSupport.md`, and the
V10 half of item 3, `public Message MapToMessage(…)`, a bare signature. The V9 half of item 3 carries
the V9 reason. The `ImplementAQueryHandler` halves are complete classes failing on the page's own
`_repository` and domain types, which is context, so they carry no marker.

### The baseline *(task 4.4)*

**92 → 101 rows, +9, at `9c57ae2`:** `AzureScheduler.md#18`, `CQRSWithBrighterAndDarker.md#6`,
`CloudEventsSupport.md#8`, `#10`, `FAQ.md#18`, `#19`, `SweeperCircuitBreaking.md#9`,
`Telemetry.md#1`, `#4`. No baselined block moved; the splits renumbered later blocks on two pages,
none of them baselined.

`FAQ.md` regains `FAQContext`, five values. **Red-proofed:** with the page's original text restored,
`FAQ_18` reads `FAILED CS1503` and `FAQ_19` `CS1061,CS1503`; reverted, both BUILT.

### Found and not repaired — for 017

- **`InMemoryScheduler.md` § *Configuration with Custom Timer Provider*.** `ITimerProvider` does not
  exist at 10.7.0; the scheduler takes a `TimeProvider`. A section rewrite.
- **The sync-attribute-on-async-handler shape was not surveyed**, only repaired on the three blocks
  it was found on. `ReactorAndProactor.md`, `HowConfiguringTheCommandProcessorWorks.md`,
  `PolicyFallback.md` and `ImplementingExternalBus.md` also carry `[UsePolicy(`.
- **`CQRSWithBrighterAndDarker.md:700`**, `Id = command.Id`: `command.Id` is now an `Id`, and the
  page never shows `Order`, so whether that assigns is unknown.
- **`InMemoryScheduler.md#4` fails on `args`.** The repair removed its trailing `static` method, so it
  is now shape **`statements`**, and the `statements` wrapper, `Holder.Run()`, has no `args`. An
  instrument quirk, not a page defect.

### The site change *(task 4.5)*

```text
git diff --stat master -- contents/     15 pages
AzureScheduler  BrighterSchedulerSupport  CQRSWithBrighterAndDarker  CloudEventsSupport
ConfiguringOpenTelemetry  DarkerAndBrighterPipelines  FAQ  ImplementAQueryHandler  InMemoryOptions
InMemoryScheduler  OutboxArchiver  PostgreSQLMessageBroker  SchedulingAMessage
SweeperCircuitBreaking  Telemetry
```

No banner, page type, heading or opening sentence changed, so no URL and no `description:` moved;
gates 3, 4 and 7 confirm.

**Signed off 2026-09-24**, with the deletion. PR #183 merged `--admin` as `8ccefd6`; the post-merge
`master` run `35965289904` passed all four jobs; `spec/016-phase4` deleted locally and on the remote.

### The nine gates, reconciled *(task 4.6)*

| # | Gate | Predicted | Read at `b941837` | |
|---:|---|---|---|---|
| 1 | `linkcheck` | none | 165 files, 0 broken | ✅ |
| 2 | `pagelint` | down | 0 errors, **743** warnings, 162 pages | ✅ read, not predicted |
| 3 | shape | none | 161 / 12 sections / 4 of 4 / 12 of 20 | ✅ |
| 4 | redirects | none | 77 entries, 7858 bytes | ✅ |
| 5 | `versioncheck` | none | 0 stale of 18, across 5 | ✅ |
| 6 | `optioncheck` | none | 0 mismatches, 59 tables, 519 rows | ✅ |
| 7 | `--verify` | none | 161 predicted = 161 published | ✅ |
| 8 | `symbolcheck` | none | 0 findings, 22 entries, 161 pages, 3 silenced | ✅ |
| 9 | `blockcheck` | moves | **989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED — 0 findings, 16 skipped** | ✅ read, not predicted |

`tools/README.md` rows 2 and 9 carry `b941837`; no other file quotes either figure.

---

## Phase 5 prediction

**None, for all nine, written before any gate was run.** Phase 5 edits `spec/` and `tools/`, adds no
`.md` under `tools/`, and touches no page. Deleting the four preludes leaves 989 of 989 `--report`
rows identical, because no page was mapped to one: it moves `--list-scaffold` (106 → **84**
identifiers) and no verdict.

---

## Phase 5 as executed

### Acceptance walk: the three criteria with no instrument *(task 5.1)*

Walked first.

| # | Who read it | What they read | Found |
|---|---|---|---|
| **AC13** | **the maintainer**, 2026-09-25 | the unit rule, the 14 units and `--list-scaffold`'s 106 identifiers, the red-proofs showing a scaffolded value still has its members checked, and the four preludes | **Accepted.** The 102 blocks needing a type the page names and never shows go to **017**, which may scaffold them as stubs. **The four preludes were ruled deleted**: carried from 015's harness, mapped to no page, two of them declaring domain types (`CreateOrderCommand`, `IOrderRepository`) the unit rule forbids. The mechanism stays, unused |
| **AC14** | **the maintainer**, 2026-09-25 | a **seeded** random sample (`random.seed(16)`), 6 of the 67 wrapped BUILT blocks, each shown as the page's lines beside what the staged file adds | **Accepted.** The wrapper adds exactly `using static <PageContext>;`, `namespace B_<id>`, `class Holder` and `async Task Run() { … }`: no `using` directive and no type |
| **AC15** | the walker | `behaviou?r` over this spec's four documents, `tools/README.md`, `blockcheck.py`, `Program.cs` and the workflow; then `correct\|prove\|verif…\|guarantee\|honest` restricted to lines about the gate | **Met.** Every hit is a disclaimer, and `tools/README.md` row 9 says a compiling block can still assert false behaviour |

### Acceptance walk: the twelve instrumented criteria *(task 5.2)*

Run at this branch's HEAD, every exit code read bare.

| # | Command, as the criterion names it | Output | |
|---:|---|---|---|
| **AC1** | `--report > r; echo $?`, then `awk` over `r` | exit **0**; `BUILT 101`, `FAILED 872`, `SKIPPED 16` = **989** rows | ✅ 989: phase 4's splits added four blocks |
| **AC2** | `--list > l; echo $?`; `wc -l`; `pagelint.Page` count; `KafkaConfiguration.md` | exit **0**; **989** = **989**; **20** of 20 Kafka blocks, every one of them a `` ``` csharp`` fence | ✅ |
| **AC3** | `ls tools/blockcheck/*.csproj`, then `ProjectReference` / `Version=` / `PackageReference` | `blockcheck.csproj`: 0 / 1 / 1 | ✅ The glob does not reach `refs/refs.csproj`, which holds the pin; it reads 0 / 71 / 71 |
| **AC4** | plants; then break a baselined block and restore it | plants: all seven exactly as `plants/index.tsv` records; `Telemetry.md#1`, `RequestContext` → `RecordRequestContext`: exit **1**, `FAILED CS0117`, *"stopped building … admitted BUILT at `9c57ae2`"*; restored: `diff` empty, `git diff --quiet`, exit **0** | ✅ the positive case is the plants, outside the corpus |
| **AC5** | two `--report` runs, `diff`, wall clock | reports identical; **6.87 s** and **6.82 s** | ✅ |
| **AC6** | `--verify-extraction` | exit **0**; **989 of 989 identical**, 1 with `using` directives hoisted | ✅ N = AC1's count |
| **AC7** | the last line; `--list-skips` | `0 findings, 16 skipped`; `--list-skips` exits **2**, *"unknown mode"* | ✅ Met by `--report`: 16 reasons, 15 of them V9 forms. `--list-skips` does not exist |
| **AC8** | `--list-scaffold`; files under `scaffold/` | exit **0**; *"84 identifiers from 14 unit(s) and 0 prelude(s); 15 page(s)"*; 14 unit files | ✅ 15 pages from 14 units: `RelationalTransportContext` serves two |
| **AC9** | delete a row; add a row for a missing block | `Telemetry.md#1`'s row deleted: exit **1**, *"BUILT, not in the baseline"*; `Telemetry.md` block **99** added: exit **1**, *"the page has no such block"*; both reverted: `git diff --quiet`, exit **0** | ✅ |
| **AC10** | `grep -rn '101 BUILT' … > f; grep -vc '^./spec/' f` | **as written: 2.** Anchored `^(\./)?spec/`: **1**, `tools/README.md:55`. Control, `'985 blocks'`: **2** (`tools/README.md` and a comment in `blockcheck.py`) | ✅ Met with `^(\./)?spec/`. As written, the pattern excludes nothing under BSD grep |
| **AC11** | each gate's own command, and the prediction's commit order | phases 1–3 and 5: prediction before the work; phase 4: after the page edits, before gates 1 and 3–8 | ⚠️ Met for 4 of 5 phases. Phase 4's prediction came after the page edits |
| **AC12** | the `--report` distribution, committed with its command | phase 2's `verdicts.tsv` (985) and § *The corpus run*; the current distribution in `tools/README.md` row 9 and above | ✅ |

**The instruments named for AC3, AC7 and AC10 need correcting. That goes to 017.**

**The nine gates, reconciled.** Predicted *none*, read at this branch's HEAD after the preludes were
deleted:

| # | Gate | Read | |
|---:|---|---|---|
| 1 | `linkcheck` | 165 files, 0 broken | ✅ |
| 2 | `pagelint` | 0 errors, 743 warnings, 162 pages | ✅ |
| 3 | shape | 161 / 12 sections / 4 of 4 / 12 of 20 | ✅ |
| 4 | redirects | 77 entries, 7858 bytes | ✅ |
| 5 | `versioncheck` | 0 stale of 18, across 5 | ✅ |
| 6 | `optioncheck` | 0 mismatches, 59 tables, 519 rows | ✅ |
| 7 | `--verify` | 161 predicted = 161 published | ✅ reached the live sitemap |
| 8 | `symbolcheck` | 0 findings, 22 entries, 161 pages, 3 silenced | ✅ |
| 9 | `blockcheck` | 989 blocks: 101 BUILT, 872 FAILED, 16 SKIPPED — 0 findings, 16 skipped | ✅ |

### The backwards check *(task 5.3)*

```text
git diff --name-only dac5954^ HEAD -- contents/ README.md SUMMARY.md .gitbook.yaml   ->  19 files
  phase 4 (a194c4a..8ccefd6)      15 pages     the declared list, § The site change
  phase 3 (cfc62c4..a194c4a)       5 pages     12 added lines, every one `<!-- blockcheck: skip … -->`
  both                             1           FAQ.md
  in neither                       0
  phases 1 and 2                   0
```

**19 = 15 + 5 − 1; nothing is outside the declared sets.** `SUMMARY.md`, `README.md` and
`.gitbook.yaml` are untouched. Phase 3's only `+` or `-` lines are the twelve markers. Phase 4's
repairs beyond the claim list are all named in § *The repairs*.

### Defect ledger *(task 5.4)*

**Found by** is one of five. **tool**: an instrument's own verdict. **control**: a red-proof or
a control's other half. **running**: executing a written-down command or example as written.
**re-derivation**: a second method for a figure. **reading**: a person.

**Before phase 1.** The task's input counts nine; the record supports the rows below. The
*"Q6 amendment misses"* are enumerated nowhere, so they are one row.

| # | Defect | Found by |
|---:|---|---|
| 1 | AC3 was satisfiable by the tool not existing | running |
| 2 | AC1, AC2, AC10 read the exit code through a pipe | control |
| 3 | the requirements' checklist overstated *"12 instrumented"* | re-derivation |
| 4 | the probe's recipe had never been executed (friction 56) | running |
| 5 | a wall-clock quoted from the fastest of four runs | re-derivation |
| 6 | a `CS0101` claim in the design was false | running |
| 7 | the Q6 amendment misses, unenumerated | reading |
| 8 | the design's *"6 before/after blocks, 4 pages"* over-counted | re-derivation |
| 9–14 | the tasks review's six: a ❌ that would publish a falsehood; the same over-count; ❌ blocks nothing marked; exit 2 promised and never produced; obligation 8 with no red-proof; a one-way control called two-way | reading ×5, re-derivation ×1 |

**Phases 1–5.**

| # | Phase | Defect | Found by |
|---:|---|---|---|
| 15 | 1 | the design's central batching mechanism did not reproduce as described (task 1.8) | control |
| 16 | 1 | `AWSSQSMigrateToV10.md#1`, a before/after pair with no marker | tool (`--verify-extraction`) |
| 17 | 2 | a fifth shape, `toplevel`: 7 blocks parse only under a rule `classify()` had not chosen | tool |
| 18 | 2 | a stale `refs.txt` survived a failed build and measured the old pin (friction 57) | control |
| 19 | 2 | `$ids` unsplit under zsh, so `--explain` printed nothing (friction 58) | control |
| 20 | 2 | `NOT COMPILABLE` unreachable, its zero read as coverage (friction 59) | re-derivation |
| 21 | 2 | 180 blocks fail to parse, against 131 and 165 | re-derivation |
| 22 | 2 | ten claim blocks and four fence pairs | tool + reading |
| 23 | 2 | *"the grep is the only instrument that sees five of the six"* was false | tool |
| 24 | 3 | the design's 8 V9 skips were the wrong 8; the set is 12 on 5 pages | re-derivation |
| 25 | 3 | four new claims exposed by scaffolding (`FAQ.md#18`, `#19`, `AzureScheduler.md#18`, `SweeperCircuitBreaking.md#9`) | tool |
| 26 | 3 | `VERDICTS`' comment asserted something that had just become false | reading |
| 27 | 3 | a finding printed `SKIPPED SKIPPED` | control |
| 28 | 3 | `git grep '\beight\b'` blind to *"Eight"* | re-derivation |
| 29 | 3 | AC10's `^./spec/` excludes nothing under BSD grep | control |
| 30 | 3 | the `options` job runs daily, against its own comment | running (a scheduled run read) |
| 31 | 3 | a README sentence asserting a re-run that had not happened | reading |
| 32 | 4 | `Telemetry.md#1` was an invented API, and phase 2's recorded fix was a guess | reading (the source) |
| 33 | 4 | phase 2's *"12 pages"* was 10 | re-derivation |
| 34 | 4 | four claims were declared-unreleased, not false; one diagnostic was a cascade | reading |
| 35 | 4 | *"Guid → string"*: the type is `Id` | tool (after the split) |
| 36 | 4 | the invented API on 2 more blocks; the Hangfire constructor on 4 pages; `QuartzMessageSchedulerFactory`; sync attributes on async handlers ×3; `UseOutbox`; `IAmAnOutbox<,>`; `UseScheduler` given one interface; `PlaceOrderCommand : IRequest` ×2; `UseOutboxArchiver` non-generic; `SendAsync` reversed ×5; a stray `)` | reading + tool |
| 37 | 4 | `InMemoryOptions.md#2`: a missing message pump type, a `Publication` with no `RequestType`, and a scheduled `Send` asserted to reach the bus | running + control |
| 38 | 4 | **Brighter 10.7.0: `AutoFromAssemblies()` registers `FireSchedulerRequestHandler` twice** — BrighterCommand/Brighter#4414 | running + control |
| 39 | 4 | `ITimerProvider` does not exist (not repaired, for 017) | reading |
| 40 | 4 | the prediction was written after the work | reading |
| 41 | 5 | AC3's glob misses the file that holds the pin | running |
| 42 | 5 | AC7 names a `--list-skips` flag that was never built | running |
| 43 | 5 | AC10 as written *fails* a met criterion | running |
| 44 | 5 | four dormant preludes declaring forbidden domain types | reading |
| 45 | 5 | phase 4 attributed `args` to the wrong wrapper | reading |

```bash
# counted from the table's last column, rows 9–14 as six, a combined row by its first finder
#   reading 15 · running 9 · re-derivation 9 · control 6 · tool 6        = 45
```

**45 defects.** 015's split was 12 tool / 4 control / 14 person. Reading is the largest category;
the tool found 6 by its own verdict and pointed at most of the rest. **Running, 9**, is what a compile
gate cannot absorb: rows 37 and 38 compiled cleanly and were wrong.

### Workflow friction *(task 5.5)*

The ledger stood at 51 when 016 opened. 016 wrote 52–59 as it went: 52–53 in `requirements.md`,
54–56 in `design.md`, 57–59 above. It adds:

| | |
|---:|---|
| **60** | **git's ERE has no `\b`.** `git grep -i -E '\beight\b'` returned nothing with *"Eight commands"* in the file; `-w` found seven. A plausible zero |
| **61** | **`grep … file >> file` appends nothing.** An edit that did not happen and a check that does not fire print the same output. Read the file after the edit |
| **62** | **A marker inserted by line number landed inside a fence.** It corrupted the block, bound the next one, and fired the finding for the wrong reason |
| **63** | **`schedule:` at the top of a workflow triggers every job without an event filter.** A comment saying otherwise survived because a pinned verdict repeats |
| **64** | **A diff dropped `--show`'s first body line as a header** and reported the page's own `using` directives as added by the wrapper, which would have failed AC14 on a clean wrapper. One staged file read beside its page settled it |
| **65** | **A prediction cannot follow work that uses the gates as instruments.** Phase 4's repair loop ran `pagelint --changed` and `blockcheck` repeatedly, so two of nine gates were read before the prediction was written. Write the prediction as the phase's first commit |
| **66** | **A fix written beside a diagnostic reads as a measurement.** Phase 2 recorded *"the released names are `RequestInformation`, `MessageBody`, `MessageHeaders`"* beside a measured `CS0117`; the names were a guess and two of the three do not exist. Keep what the compiler said and what was inferred in separate columns |

```bash
# 51 + 8 (52–59) + 7 (60–66) = 66
grep -oE '^\| \*\*6[0-6]\*\*' spec/016-compile_gate/tasks.md | wc -l     # 7
```

### What 016 shipped *(task 5.6)*

| | |
|---|---|
| **The gate** | `tools/blockcheck.py` + a Roslyn compiler, **one `Compilation` per block**, against **71 packages pinned in one file**, stamped so a stale reference list is exit 2 |
| **The corpus** | **989 C# blocks across 145 pages**, enumerated through `pagelint.Page` and byte-identical to the pages |
| **The baseline** | **101 blocks required to build**, enforced both ways in CI's `blocks` job; a repair brings its row in the same PR |
| **The opt-out** | 16 skips, each with a written reason, printed on every run |
| **The scaffold** | 14 units, 84 identifiers, values only, typed from the pin |
| **The repairs** | 19 pages changed across phases 3 and 4; 14 listed claims repaired, and every recurrence of the same falsehood a grep found |
| **Upstream** | BrighterCommand/Brighter#4414 |

**The residual gap, the line 017 starts from:** **872 of 989 C# blocks still do not compile against
the released packages. Most need a `using` directive or a stub for a type the page names but never
shows (102 of them are ruled scaffoldable). The gate holds only the 101 that do, so 017 raises that
number page by page, starting with those 102, and corrects the instruments for AC3, AC7 and AC10.**

