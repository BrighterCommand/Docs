# Spec 016: A Committed Compile Gate — Tasks

**Created:** 2026-09-20
**Status:** Tasks Phase — **reviewed 2026-09-20, six findings, amended**
**Requirements:** approved 2026-09-20 · **Design:** approved 2026-09-20

**Five phases, 39 tasks, one pull request per phase.** The list was 37 at the review, and
§ *What the tasks review found* records what that review found and which task each finding
produced. **The two inserted tasks are numbered `1.6a` and `4.3a` rather than renumbering their
phases** — every later task keeps the number other tasks already cite, and the insert stays visible
instead of being smoothed away.

The contract a phase merges under is
`tools/README.md` § *One phase is one pull request* — cited, not restated, including its rule that
**a gate and the corpus that satisfies it merge together, or the gate merges second**, which is
what puts the CI job in phase 3 and not phase 1.

---

## 1. Standing obligations — stated once, binding every task

**These are not repeated per task.** Restating some invites the reader to treat the unrestated ones
as optional.

The programme's seven:

1. **Re-derive any count before quoting it** — command beside the figure, **two methods that
   agree**, and re-derive the number the **decision** turns on rather than the one the document
   leads with. A figure inherited from `requirements.md`, `design.md` or an earlier phase has
   already begun to rot.
2. **Record the mismatch before fixing it.** The corrected state is all that survives otherwise, and
   a spec that cannot show the corpus was *ever* wrong has no evidential product.
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof
   with its output recorded here, and **every control is two-way** — the positive case goes
   **outside** the enumeration the instrument was built from, **and it must be able to pass**.
4. **Prose and permission ship together**, read in both directions.
5. **Cite `CLAUDE.md` and `tools/README.md`; never restate them.** And when a phase edits
   `CLAUDE.md`, grep the commands for the claim it changed.
6. **Predict gate movement before the work, including "none"** — with the mechanism, then reconcile.
7. **Ask before merging anything that changes the published site**, and ask for the head-ref
   deletion **by name** in the same breath.

**016 adds five, and each one is a defect this spec has already met:**

8. **The unit of compilation is one block.** Never a batch, never a shared project. Friction 54:
   two blocks reporting 20 and 8 errors in a batch of two reported **zero** in a batch of five. Any
   proposal to "just compile them together" is this defect returning.
9. **A corpus is enumerated through `pagelint.Page`, and its size is re-derived by a second
   method.** Friction 53: the obvious grep sees 835 of 985 and reports success on what it found.
10. **A gate number changes in `tools/README.md` and nowhere else.**
11. **No criterion and no check reads an exit code through a pipe, and none is satisfiable by a
    missing path.** `$?` after a pipe is the last stage's; `grep … | wc -l` prints `0` whether the
    corpus is clean or the path is absent. Both were found in this spec's own criteria.
12. **Run the committed form, from a clean directory.** Friction 56: the probe's documented recipe
    had never been executed and would have reported all 985 blocks broken — believably, and in the
    direction that confirms the thesis.

---

## 2. The phases

| Phase | Goal | Tasks | Published site | PR |
|---:|---|---:|---|---|
| **1** | **The instrument** — extractor, Roslyn compiler, reference pin, and five red-proofs. **Not yet a gate** | 11 | untouched | one |
| **2** | **The corpus run** — P0-6's distribution over all 985, the parse triage, and the claim/context split P0-9's boundary needs | 6 | untouched | one |
| **3** | **The baseline and the gate** — scaffold, opt-out, ratchet, `tools/README.md` row 9 **and** the CI job, together | 9 | untouched | one |
| **4** | **The repairs** — P0-9, defects of claim | 7 | **CHANGED — needs sign-off** | one |
| **5** | **Acceptance** — the walk, the backwards check, both ledgers, the close | 6 | untouched | one |

**Why this is not Research → Core → Supporting → Polish.** The default would put the CI job in a
"supporting" phase after the baseline, and would put the corpus measurement in "research" before
the instrument exists. Neither survives contact with this design: **the measurement cannot happen
until the instrument is built** (phase 1 → 2), and **the gate must not merge before the corpus that
satisfies it** (`tools/README.md`'s rule 3), which is why phase 3 carries both the baseline and the
job rather than splitting them. Phase 4 is separate from phase 3 for one reason only: **it is the
only phase that changes the published site**, and a sign-off should not be buried in a PR that is
mostly tooling.

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

- [ ] **Task 3.3:** Write the scaffold files the initial baseline needs
  - Input: task 2.1's failures in the context class, grouped by page
  - Output: one prelude per admitted page under `tools/blockcheck/scaffold/`, each listed by `--list-scaffold`
  - Notes: **a prelude supplies identifiers, never behaviour, and never a type the page tells the reader to write.** AC13 is a maintainer reading this boundary.

- [ ] **Task 3.4:** Define and populate `baseline.tsv`
  - Input: tasks 2.1 and 3.3
  - Output: `tools/blockcheck/baseline.tsv` — page, ordinal, scaffold, and the ref it was admitted at — holding **only blocks that compile without any page being edited**, plus the count in this file
  - Notes: keeping page edits out of this phase is what makes phase 3 site-neutral. Blocks needing a `using` on the page belong to phase 4 or to backlog item 2.

- [ ] **Task 3.5:** Implement the ratchet, enforced in both directions
  - Input: AC9
  - Output: exit 1 when a baselined block stops being `CLEAN`, **and** exit 1 when a baselined row names a block that no longer exists
  - Notes: without the second, deleting a page silently shrinks the gate's corpus and the gate still says `0 findings`.

- [ ] **Task 3.6:** Red-proof the ratchet, both directions, with its green control
  - Input: task 3.5
  - Output: three recorded runs — a baselined block broken on purpose → exit 1 with the compiler's code; a phantom row added → exit 1; and **the unmodified state → exit 0**. Every edit reverted, verified by `git diff` → empty
  - Notes: the third run is the control that the first two were not passing for some other reason.

- [ ] **Task 3.7:** Add row 9 to `tools/README.md`
  - Input: `tools/README.md` § *The eight gates*, § *What each gate actually checks*
  - Output: the row — command, corpus, expected figure, **and the ref it was measured at** — plus the heading and any prose that says "eight" updated to nine
  - Notes: obligation 10. **And grep the whole repository for "eight gates"** — the count lives in prose in more than one place, and a header that disagrees with its own body is this programme's most-repeated defect.

- [ ] **Task 3.8:** Add the CI job
  - Input: `.github/workflows/docs.yml`'s `options` job and its comments
  - Output: the new job — `setup-dotnet`, restore the pinned refs, run the gate — with **no guard, no `|| true`, and no `schedule:`**, each choice carrying the comment that says why
  - Notes: Q8 and Q9 are ruled by this task's choices; record both rulings in one line each.

- [ ] **Task 3.9:** Predict and reconcile the eight — now nine — gates
  - Input: as task 1.10
  - Output: § *Phase 3 prediction* and § *Phase 3 as executed*
  - Notes: **`linkcheck` is predicted to MOVE if and only if this phase adds a `.md` under `tools/`.** The design chose not to. If the reconciliation shows 166, the cause is a file somebody added without noticing the rule.

---

## Phase 4 — The repairs *(7 tasks, one PR, CHANGES THE PUBLISHED SITE)*

**Goal:** P0-9. **Obligation 7 binds on this PR** — sign-off before merge, head-ref deletion asked
for by name in the same breath.

- [ ] **Task 4.1:** Re-derive the claim list before repairing anything
  - Input: task 2.3's claim list
  - Output: the list re-derived at this phase's HEAD, with the command, and any difference from phase 2's explained rather than adopted
  - Notes: phase 2's list was measured at a ref two merges back. 015's phase 4 found three inherited figures stale before it began.

- [ ] **Task 4.2:** Repair the before/after-in-one-fence blocks
  - Input: task 2.5's confirmed list, `CLAUDE.md` § *Version markers on code*
  - Output: each such block split into two fenced blocks — **and the labels chosen by what the pair actually is.** A **version** pair (`CloudEventsSupport.md`, `PolicyRetryAndCircuitBreaker.md`: `// V9` above `// V10` in one fence) takes ❌ for the superseded form and ✅ for the current one. A pair that is **not** about versions takes an ordinary labelled heading or bold lead-in and **no ❌/✅ marker at all**
  - Notes: the repair is a convention this repository already has, so this is not an invention — but *(review finding 1)* **the convention is `CLAUDE.md` § *Version markers on code*, which opens *"Where V9 and V10 differ"*, and one block on the list is not that.** `ImplementAQueryHandler.md` block 10 is `// Before (synchronous)` / `// After (asynchronous)` — **Reactor and Proactor, both current in V10.** Marking `Execute` ❌ *superseded* would assert something false on the page and contradict `ReactorAndProactor.md`; label those two *Synchronous* and *Asynchronous* instead. **Do not change any page's banner, type, headings or opening sentence** — a heading is a published URL.

- [ ] **Task 4.3:** Repair the remaining defects of claim
  - Input: task 4.1's list
  - Output: the edits, page by page, each one named in this file with what it claimed and what is true
  - Notes: obligation 2 — record the mismatch before fixing it. **Every edited block is built before the commit**, and a repair that needs more than its defect fixed says so (015's phase 4 found fifteen blocks broken in ways the site was not about).

- [ ] **Task 4.3a:** Give every block this phase creates or leaves uncompilable its opt-out, with a reason *(review finding 3)*
  - Input: tasks 4.2 and 4.3, task 3.1's marker, task 3.2's rule
  - Output: a skip marker with a reason on **every ❌ block this phase creates** — the split makes one per version pair, and P0-5's rule covers *any* block that must not compile, not only the eight that existed at phase 3 — **and** on any split half that is still a **fragment** rather than a compilable block; plus the run's `N skipped` before and after, so the count moves by exactly the number added
  - Notes: at least three of the candidates are bare signature comparisons — `CloudEventsSupport.md` shows `public Message MapToMessage(OrderCreated request)` against its V10 form, with no bodies — and **splitting a fragment yields two fragments, neither of which compiles.** Without this task those blocks are silently absent from the baseline and the gate still reports `0 findings`: a repair that leaves nothing measurable behind. The alternative — rewriting them into compilable examples — is a page change beyond the defect, so it is 4.3's *"a repair that needs more than its defect fixed says so"*, not a default.

- [ ] **Task 4.4:** Update the baseline and scaffold for every repaired block
  - Input: tasks 4.2 and 4.3
  - Output: `baseline.tsv` rows added for blocks that now compile, and the run green at the end of the branch
  - Notes: the gate shipped in phase 3, so **this branch must end green or it reddens `master`**.

- [ ] **Task 4.5:** Record the site change and take the sign-off
  - Input: the branch's `git diff --stat` against `master`
  - Output: the list of pages changed, in this file, and the sign-off asked for **with the head-ref deletion named in the same breath**
  - Notes: obligation 7. A merge is not a deletion, and one authorisation covers one PR.

- [ ] **Task 4.6:** Predict and reconcile the nine gates
  - Input: as task 1.10
  - Output: § *Phase 4 prediction* and § *Phase 4 as executed*
  - Notes: **this is the phase where `pagelint` may move**, and the direction is down if a repaired block gains `using` directives. Predict the number **before** the work and reconcile; `tools/README.md` row 2 owns the figure. `symbolcheck` and `optioncheck` both read `contents/` and may move too — predict each, do not assume.

---

## Phase 5 — Acceptance *(6 tasks, one PR, no page touched)*

- [ ] **Task 5.1:** Walk the three criteria with no instrument, first
  - Input: AC13, AC14, AC15
  - Output: § *Acceptance walk* — for each, who read it, what they read, and what they found
  - Notes: **both criteria ever found unmet at a close were unmarked ones.** AC13 and AC14 are the maintainer's readings of the scaffold boundary; AC15 is a check that this spec's own documents never claim the gate checks behaviour.

- [ ] **Task 5.2:** Walk the twelve instrumented criteria, running each named instrument
  - Input: AC1–AC12
  - Output: each criterion with its command and that command's actual output
  - Notes: eleven of the twelve were **deferred** at the requirements review — they name a tool that did not exist. This is the walk where they stop being deferred. **Check what the instrument prints against what the criterion claims**, which is how AC3 and AC1's pipe defects were found in the first place. **AC11 is met here rather than by a tenth predict-and-reconcile task**: phases 1–4 each carry one, and this phase touches only `spec/` and `tools/README.md`, so its own prediction is *none* for all nine — say so in the walk rather than leaving the pattern to break silently at the last phase.

- [ ] **Task 5.3:** The backwards check — what changed that should not have
  - Input: `git diff --stat` against the ref at which 016 opened
  - Output: the page set this spec changed, compared against phase 4's declared list, with any difference explained
  - Notes: *"while I'm here"* is how a spec quietly widens.

- [ ] **Task 5.4:** Write the defect ledger
  - Input: every phase's findings, plus the **nine defects found before phase 1 began** — three at the requirements review, three at the design review, the Q6 amendment misses, and the six before/after blocks
  - Output: § *Defect ledger* with a **found by** column distinguishing a tool's win from a re-derivation's
  - Notes: 015's split was 12 tool / 4 control / 14 person. This spec's will look different and the difference is the interesting part.

- [ ] **Task 5.5:** Write the friction ledger
  - Input: frictions **52–56**, already written in `requirements.md` and `design.md`, plus anything phases 1–4 met
  - Output: § *Workflow friction* in this file, continuing from 56
  - Notes: re-derive the ledger's total rather than inheriting a header — this file's own count has been wrong in a heading while its body disagreed, twice.

- [ ] **Task 5.6:** Close the spec
  - Input: everything above
  - Output: the `README.md` checklist at its final count, § *What 016 shipped*, and **the residual gap in one sentence** — the line 017 starts from
  - Notes: 015's closing sentence became this spec. Write the next one as though somebody will have to execute it, because they will.

---

## What the tasks review found — 2026-09-20, six findings, all before a line was built

**Obligation 2 applies to this list as much as to the corpus**, so the findings are recorded here
rather than absorbed into the tasks that answer them. Four came from reading the plan against
`CLAUDE.md` and the corpus; two came from running the second method the plan itself asks for.

| # | Finding | Answered by |
|---:|---|---|
| **1** | **Task 4.2 prescribed a marker that would publish a falsehood.** ❌/✅ is `CLAUDE.md` § *Version markers on code*, which opens *"Where V9 and V10 differ"*. `ImplementAQueryHandler.md` block 10 is sync → async — **Reactor and Proactor, both current in V10** — so ❌ on `Execute` asserts a supersession that `ReactorAndProactor.md` denies | task 4.2, rewritten to choose the label by what the pair is |
| **2** | **The design's "6 blocks, 4 pages" over-counts, and the second method is what shows it.** Nine candidates, five pages — and reading strikes out four | task 2.5, which now expects the list to shrink |
| **3** | **Phase 4 creates ❌ blocks and nothing marked them.** P0-5 covers *any* block that must not compile, not only the eight that existed at phase 3; and three candidates are bare signature fragments, where splitting yields two fragments and no compilable block | **task 4.3a, inserted** |
| **4** | **Exit 2 was promised and never produced.** No task emitted it, and its real-world cause — an unrestored reference set — makes all 985 blocks fail *believably*, in the direction that confirms the thesis | **task 1.6a, inserted**, plus 1.6's named conditions |
| **5** | **Obligation 8, the load-bearing one, had no red-proof.** Friction 54's mechanism is a parse failure suppressing binding across a shared compilation; a `CS0246` plant and a clean plant are both still reported under batching, so the existing pair cannot fail if the rule is broken | task 1.8's third plant |
| **6** | **Task 1.9's control was one-way while the checklist called it two-way.** Two identical runs are also what a tool that cached, or did nothing, would print | task 1.9's second direction, and the checklist row now says why |

**The measurement behind findings 1–3**, run at the review's HEAD, enumerating through
`pagelint.Page` because the grep is blind to 150 blocks *(friction 53)*:

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

**Nine candidates, and four are not the defect.** `SwitchingSchedulers.md` already does it the way
the repair prescribes — *Before* and *After* in **separate** fences, one complete block each — and
`AgreementDispatcherRouting.md` block 3 is not the shape at all: `// Before Jan 2025` and
`// After Jan 2025` are comments **inside one routing lambda**, about tax rules and not about two
forms of the same code. It reached the design's list as a grep hit nobody read.

**The same run re-derives the corpus: 985 C# blocks across 162 pages loaded**, which is the
design's figure by an independent path, and the reason the numbers above are quotable.

**None of this is adopted by arithmetic.** Task 2.5 still owns the confirmed list and still derives
it two ways at its own HEAD; what changed is that it now knows the list can shrink, and which four
candidates to examine first.

---

## Totals

**39 tasks** — 11 + 6 + 9 + 7 + 6. Re-derive rather than trust this line:

```bash
grep -c '^- \[.\] \*\*Task' spec/016-compile_gate/tasks.md      # 39
```

**The phase table sums to 39 independently**, which is the second method, and the phase headings
carry the same per-phase figures, which is a third claim that could drift from either.

**It was 37 at the review.** Two tasks were inserted — `1.6a` and `4.3a` — and the numbering says
so rather than hiding it.

---

## Task quality checklist, applied to this list

| | |
|---|---|
| Each phase is one PR, deliverable-shaped | five phases, and § *The phases* says **why the default was left** — the measurement cannot precede the instrument, and the gate cannot precede its corpus |
| Standing obligations stated once | §1, twelve of them, **not repeated per task**; five are 016's own and each is a defect already met |
| Every new check has a red-proof with a two-way control | tasks 1.6a, 1.7, 1.8, 1.9 and 3.6 — 3.6 carries the green control explicitly, and **1.6a and 1.9's second direction exist because this row was false when the review read it**: 1.9 was one-way and exit 2 had no proof at all |
| Inherited counts re-derived by two methods | tasks 1.2, 2.5 and 4.1; task 2.5 notes the case where the two methods are **not** corroborating each other, **and that its list can shrink** |
| Every Output names something that can be seen to exist | file paths, `.tsv` rows, named sections, recorded runs — no "update the docs" |
| Acceptance last, owning the walk and both ledgers | phase 5, and it starts with the three criteria that have no instrument |

---

**Next step: `/spec:review`.** Two things want confirming rather than assuming: **P0-9's boundary**
(task 2.3 makes it measurable, and the claim list's size decides whether phase 4 is a phase), and
**the choice to keep page edits out of phase 3** so that only one phase needs a sign-off.

---

## Phase 1 prediction

**Written 2026-09-20, before any gate was run in this phase** — which is the thing AC11 is about.
`tools/blockcheck.py` existed when this section was typed; no gate had been executed since the
phase began, so the prediction below is a prediction and not a reading. `tools/README.md` owns the
expected figures and this section cites rather than copies them.

**All eight: none.** The mechanism is what makes that checkable, and it is not the same mechanism
twice:

| # | Gate | Prediction | Why, from mechanism |
|---:|---|---|---|
| 1 | `linkcheck` | **none** | It walks `.md` files and `tools/` **is inside that walk** — writing `tools/README.md` moved it 164 → 165. Phase 1 adds `tools/blockcheck.py`, `tools/blockcheck/*.cs`, `*.csproj` and files under `tools/blockcheck/scaffold/`, and **not one `.md`**. The gate's documentation is row 9 of the file that already exists, which is task 3.7. Adding `tools/blockcheck/README.md` would move this row to 166 — a choice, and this phase declines it |
| 2 | `pagelint` | **none** | Corpus is `contents/` + `README.md`. A `.py`, a `.cs` and a `.csproj` cannot enter it. **The predicted fall to come is phase 4's, not this phase's**: rule 6's warning count moves only when a *page's* C# block gains `using` directives |
| 3 | shape | **none** | Reads `SUMMARY.md`. No page is added, moved or renamed |
| 4 | redirects | **none** | Reads `.gitbook.yaml`. Untouched |
| 5 | `versioncheck` | **none** | It reads version pins **in page prose** — 18 across 5 pages. Task 1.4 pins 67 packages in a `.csproj`, where `optioncheck`'s 63 already sit unread |
| 6 | `optioncheck` | **none** | Reflects marked option tables in `contents/` against pinned types. No table is touched. **It is the row most likely to move by accident**, because it is the other `dotnet` tool under `tools/` and task 1.3 adds a second project beside it — but its corpus is pages, not projects |
| 7 | `--verify` | **none** | Fetches the live sitemap and compares it with the predicted tree. Nothing published changes |
| 8 | `symbolcheck` | **none** | Corpus is `contents/`. No page is edited in this phase at all |

**Seven of these are vacuous passes and the eighth is too.** That is the case worth writing down:
a gate that has silently stopped checking looks exactly like a gate that correctly reports no
movement, and the only defence is that the *before* figures were read at the start and the *after*
figures at the end, both against `tools/README.md`.

---

## Phase 1 as executed

### The eight gates, read before the work *(the prediction's baseline)*

Run at `1f5fc10` with `tools/blockcheck.py` present and nothing else built. **Each verdict line is
the gate's own last line, and each exit code was read bare** — `<cmd> > /tmp/g.out 2>&1; echo $?`:

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

**Eight for eight against `tools/README.md`'s rows**, including both rows that carry the second ref
`3be2a78`. Nothing has moved, which is what makes the end-of-phase reading in task 1.10 a
comparison rather than a fresh measurement.

> **The first attempt at this table read every exit code through a pipe.** The form was
> `python3 tools/linkcheck.py | tail -3; echo $?`, which prints `tail`'s code and would have read
> **0** for all eight however they had ended. That is **constraint 11**, met while running the
> check that exists because of it, within an hour of typing the obligation out. The second form
> above redirects to a file and reads `$?` with nothing between. Recorded rather than quietly
> re-run: the pipe is what a person reaches for, which is why the rule needs to be a rule.

### The enumerator and extractor *(task 1.1)*

`tools/blockcheck.py` enumerates through `pagelint.load_pages()` and `pagelint.Page`, filters on
`pagelint.CSHARP_TAGS`, and hoists `using` directives with `pagelint.USING_RE`. **Four names
imported, no second copy of any of them** — constraint 2, and friction 53's answer.

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

**985 across 145 is `requirements.md` § *Current state* reproduced by the tool**, and the 20
`KafkaConfiguration.md` rows are AC2's named case — the page whose every fence is ` ``` csharp`
with a space, which a grep-shaped extractor sees as holding no C# at all.

**The corpus is defined as "the pages `pagelint` lints", not "the pages under `contents/`".** Those
are the same 985 blocks today, because the only other page it loads is `README.md` and that page
carries **0** C# blocks. Defining it the wider way means a C# block arriving on the site root is
compiled rather than silently exempt; defining it the narrower way would have been invisible until
the day it mattered.

**`--list` prints rows to stdout and its summary to stderr**, so `wc -l` of a redirect is the
number of blocks and nothing else, and every row is newline-terminated — friction 55, which cost
this programme a member of an enumeration once already.

**Exit codes, all read bare:**

```text
--list, corpus present            0
--show <page> <n>, block exists   0
--show <page> 99, no such block   2
no mode at all (the gate)         2    "the gate itself is not built yet: nothing was checked"
```

**An empty enumeration is exit 2 and not exit 0**, written into the tool rather than left to a
future task: zero blocks is a broken corpus walk, and a gate reporting a clean nothing is this
programme's twelve-times-met failure.

**The extractor is observable**, which is what makes task 1.7 possible at all. `--show` writes the
block body verbatim to stdout:

```bash
python3 tools/blockcheck.py --show contents/KafkaConfiguration.md 1 > /tmp/b1.cs
sed -n '72,86p' contents/KafkaConfiguration.md > /tmp/page.cs
diff /tmp/page.cs /tmp/b1.cs        # empty
```

The line range is the tool's own report — `lines 71-87` on stderr, the fences — and the block is a
**tab-indented** one, so the diff is also a check that leading whitespace survives. That is a spot
check by hand; the corpus-wide claim is task 1.7's, and this one is not a substitute for it.

**The shape counts reproduce the design's 11 / 306 / 136 / 532 exactly**, from a classifier that
uses `pagelint.USING_RE` where the probe used its own weaker one. That agreement is recorded here
and **is not task 1.2's second method** — both figures come from the same classifier, run once.
Task 1.2 still owes an independent count.

**What this task did not do, deliberately:** it classifies but does not wrap. The four wrapper
rules have a *test* half and an *emission* half; `--list` cannot print a shape the tool has not
decided, so the tests land here and the emission lands in task 1.2 with its counts.

**A drift found while reading the inputs, not fixed here:** `requirements.md` AC1 names the
verdicts `BUILT · SKIPPED · NOT COMPILABLE · FAILED`, and `design.md` § *The verdict model* names
them `CLEAN · FAILED · SKIPPED · NOT COMPILABLE`. **`BUILT` and `CLEAN` are the same verdict under
two names**, and the tool can only print one. Recorded now, before the tool emits either, so that
whichever one task 1.3 prints is a decision rather than a coin toss — obligation 2.

**Ruled at task 1.3: the tool prints `BUILT`.** AC1 is what phase 5 walks, and its instrument reads
the verdict as a field of the report; a criterion cannot be satisfied by a tool that prints a
synonym of what it asks for. `design.md`'s `CLEAN` stands in the approved design as the same
verdict under its older name, and is not edited — a figure or a term inside approved plan text is
anchored by its approval.

### The four wrapper rules, and their counts measured twice *(task 1.2)*

`WRAPPERS` in `tools/blockcheck.py` holds the emission half; `classify()` holds the test half. The
closing braces are **derived** — one `}` per `{` in the opening lines — because a wrapper whose two
halves are maintained separately is a wrapper that will one day not balance.

| Shape | Test | Wrapper | `--list` | the probe |
|---|---|---|---:|---:|
| `namespaced` | declares its own `namespace` | none; `using`s hoisted above it | 11 | 11 |
| `types` | declares a `class`/`record`/`interface`/`struct`/`enum` | `namespace B_<id>` | 306 | 306 |
| `members` | a line opens with an access or member modifier | the above, plus `public class Holder` | 136 | 136 |
| `statements` | anything else | the above, plus `public async Task Run()` | 532 | 532 |
| | | | **985** | **985** |

**Method 1 — this tool.** Method 2 — the **committed** probe, a different implementation of the
same four tests, run from `/tmp` so that nothing in the working directory could be feeding it
*(obligation 12)*:

```bash
python3 tools/blockcheck.py --stage /tmp/bc12/staged
awk -F'\t' '{n[$4]++} END{for(k in n) print n[k], k}' /tmp/bc12/staged/index.tsv

cd /tmp && python3 <repo>/spec/016-compile_gate/probe/gen.py /tmp/bc12probe/blocks
```

**They agree on all four, and they are not the same code**: the probe's `using` test is its own
and rejects `using static X;` and `using Alias = X.Y;`, where this tool uses `pagelint.USING_RE`.
The agreement says the shape of a block does not turn on that difference — every block that is
nothing but directives classifies `statements` under both.

**What this pair does NOT double-measure, and saying so is the point:** both walk the corpus
through `pagelint.Page`, because constraint 2 forbids a second fence parser. The corpus *size*
therefore needs its second method from somewhere else, and it has one — the grep, which is
**friction 53's own number**:

```bash
grep -r '^```csharp$' contents/ | wc -l      # 835 blocks
grep -rl '^```csharp$' contents/ | wc -l     # 117 pages
```

**835 against 985, and the 150 reconcile exactly**, by opening-fence spelling:

```text
  835  exactly ```csharp
  140  ``` csharp (one space)
    8  indented
    1  c# tag
    1  ```csharp + trailing space
  985  total
```

The grep's 835 is not an approximation of 985 and must not be read as corroborating it: it is the
**visible** part of the corpus, and the gap is a decomposed list rather than a discrepancy. Two
methods agreeing is evidence; two methods disagreeing by a number you can name, line by line, is
better evidence.

**985 blocks produced 985 files.** That is a collision check and not a formality — `<page>_<n>`
identifiers that collided would overwrite each other silently, and the staged directory would hold
fewer files than the index has rows:

```bash
wc -l < /tmp/bc12/staged/index.tsv      # 985
ls /tmp/bc12/staged/*.cs | wc -l        # 985
```

**`NOT COMPILABLE` is 0 of 985**, by construction rather than by luck: `classify()` ends in an
unconditional `statements`, so a block can only be unwrapped if it raises. The verdict stays in the
model because a future rule could narrow that fall-through, and a category that exists only when it
is non-zero is a category nobody notices arriving.

### The Roslyn tool, and the reference pin *(tasks 1.3 and 1.4)*

`tools/blockcheck/Program.cs` compiles **one `CSharpCompilation` per block** — obligation 8, written
into the code's opening comment with friction 54's measurements beside it, so that a later reader
meets the reason before the loop. Reference assemblies **are** shared between compilations and
diagnostics are not: a `MetadataReference` is an immutable input, a diagnostic bag is a sink, and
the defect was always the sink.

It prints `id<TAB>verdict<TAB>error count<TAB>distinct codes` and nothing else to stdout. **Its own
exit code is 0 or 2, never 1** — a failing block is data, and the corpus verdict belongs to the
Python half that owns the 0/1/2 contract.

`tools/blockcheck/refs/refs.csproj` is **67 packages, every one with an explicit version, and no
sources**, carried forward verbatim from the probe:

```bash
grep -c 'PackageReference' tools/blockcheck/refs/refs.csproj   # 67
grep -c 'Version="'        tools/blockcheck/refs/refs.csproj   # 67
grep -c 'ProjectReference' tools/blockcheck/refs/refs.csproj   # 0
grep -c 'ProjectReference' tools/blockcheck/blockcheck.csproj  # 0
```

Both projects target **net9.0**, where the probe used net8.0 — matching `tools/optioncheck` and the
`options` job's `setup-dotnet 9.0.x`, so that phase 3's CI job needs no second runtime. The move is
recorded here because it is the named cause of any difference from the probe's figures. There was
none; see below.

#### The reference set states itself, because the step that is documented is the step that gets skipped

**The probe's recipe has a hand-copy in it**, and it is load-bearing:

```bash
cp bin/Debug/net8.0/*.dll <refdir>/
cp $(dirname $(which dotnet))/packs/Microsoft.NETCore.App.Ref/8.0.0/ref/net8.0/*.dll <refdir>/
```

**The first build of this tool did the first line and not the second**, because globbing the output
directory is the obvious thing and the second line is a step in a README. What came back:

```text
233 reference assemblies
985 blocks, 1 built, 984 failing
981 CS0518   predefined type 'System.Object' is not defined
```

**99.9% of the corpus broken, and the gate had not checked one line of it.** That is friction 56
again, one layer down: not a probe recipe nobody ran, but a *step* nobody ran, and the failure is
believable and points the way the thesis does. The repair is not to document the step harder.
`refs.csproj` now writes its **own** resolved reference set at build time —

```xml
<Target Name="WriteReferenceList" AfterTargets="ResolveReferences">
  <WriteLinesToFile File="$(OutputPath)refs.txt" Lines="@(ReferencePath)" ... />
</Target>
```

— which is exactly what the compiler would have been handed: the 67 packages **and** the targeting
pack for the project's own `TargetFramework`, with no SDK version written down anywhere and no copy
step to forget. **497 assemblies**, against the probe's hand-assembled 391. The tool takes that file
rather than a directory, and **exits 2 if it is absent or names an assembly that is not there**,
because a partly-present reference set fails blocks for a reason that is not the block's.

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

**60 and 925 are the probe's numbers to the block** — and not by inheritance. Joined on block id
against the **committed** `probe/verdicts.tsv`:

```bash
join -t$'\t' -j1 <mine, BUILT→CLEAN> <probe/verdicts.tsv> | wc -l    # 985
awk -F'\t' '$2!=$3{d++} END{print d+0, "disagreements"}'             # 0
```

**985 of 985 verdicts identical**, across a different target framework, a differently assembled
reference set of a different size, and a second implementation of the tool. That is the strongest
control this phase has, and it is the one nobody planned: it was available only because the probe
was committed with its output.

**A correction to the design's error-code figures, in the direction that matters.** `design.md`
quotes `CS0246` at 714 and `CS0103` at 653; this run reads **738** and **677** over the same
verdicts. The probe's row wrote `.Take(6)` distinct codes per block, so **every per-code figure it
published is a floor**, and a block failing seven different ways contributed six. The design's
figures are not wrong about anything they decided — the gap is 24 blocks and 24 blocks — but a
number that silently truncates is worth catching before phase 2 builds a triage on it. This tool
writes every distinct code.

### The scaffold, and the two-way control that says it does something *(task 1.5)*

**A scaffold is declared in `tools/blockcheck/scaffold/pages.tsv`, one row per page**, and it has
two parts because the harness 015 built had two:

| Part | Where | What it does |
|---|---|---|
| a **unit** | `scaffold/units/*.cs` | an extra compilation unit, compiled **in that block's own compilation** — types and values the page names but no block defines |
| a **prelude** | `scaffold/preludes/*.txt` | **replaces** the generic wrapper for that page's blocks: a typed handler class, so a block that is the body of a method has the method to sit in |

A unit also declares the `using` lines it wants injected, in the unit itself rather than in a
second file:

```csharp
// blockcheck: using static PageContext;
```

That one line is the whole mechanism by which a page's named-but-undefined values — a connection
string, a producer registry, an outbox configuration — reach a block, and it is `015`'s. The four
preludes and `PageContext.cs` are carried forward verbatim from
`spec/016-compile_gate/harness/`.

**The unit rides in the block's own compilation and nowhere else**, so obligation 8 is untouched:
one block, one `CSharpCompilation`, now with two trees in it. A scaffold can help the block it was
given to and cannot silence another.

**A scaffold that does not parse stops the run** — exit 2, nothing checked. That is friction 54's
mechanism arriving through the back door: a parse error in a shared tree suppresses semantic
binding across the compilation it is in, so a broken scaffold would hand every page that uses it a
**silent pass**. It is checked once, before any block is compiled.

**Errors reported against a scaffold's tree are not counted against the block.** They are an
instrument fault, not a documentation defect, and charging them to the page would fail every page
sharing that scaffold for a reason no reader could act on. They are not swallowed either: the run
prints `WARNING: N error(s) reported against scaffold trees`.

#### `--list-scaffold`, and why it reads the tree rather than grepping it

```bash
python3 tools/blockcheck.py --list-scaffold > /tmp/sc.txt; echo $?    # 0, read bare
wc -l < /tmp/sc.txt                                                   # 56
find tools/blockcheck/scaffold -type f | wc -l                        # 6
```

```text
55 identifiers from 1 unit(s) and 4 prelude(s); 1 page(s) scaffolded
```

**Six files, five of them scaffold and one the map** — AC8's count, stated with the map named
rather than quietly included. The 56th row is the injected `using`, which is supplied from outside
the page exactly as the identifiers are and would otherwise be the one thing the listing did not
say.

The identifiers come from **Roslyn's syntax tree**, not a regex: a listing that under-reports
claims the block was helped less than it was, and that is the only direction that matters here.
Preludes are fragments — they open braces they do not close — so each is rendered the way `wrap()`
renders it, with `__NAME__` substituted and one `}` per `{@`, which is also a check that its braces
balance under the rule the tool actually applies.

#### The control: one page in, one block moves

`contents/DapperOutbox.md` is the phase-1 row in `pages.tsv`. Same corpus, same references, the
scaffold the only difference:

```text
                  no scaffold                  PageContext
DapperOutbox_1    FAILED  1 error   CS0103     BUILT   0 errors
DapperOutbox_2    FAILED 16 errors  CS0103…    FAILED 12 errors  CS0103,CS0115,CS0117
corpus            985 blocks, 60 built         985 blocks, 61 built
```

**Both directions, and neither is the whole corpus.** Block 1 moves from `FAILED` to `BUILT`, which
is the scaffold doing something; block 2 stays `FAILED` with a *different* error set, which is the
scaffold not being a way to make a page green. The corpus moves by exactly one block. A scaffold
that had quietly helped everything would have shown up here as a jump, and that is the failure this
control is for.

**The staged directory holds 986 `.cs` files and the corpus is 985.** The extra one is the staged
copy of the unit, and the tool reads `index.tsv` rather than globbing the directory — a glob would
have compiled the scaffold as though it were documentation and reported a corpus of 986:

```bash
ls /tmp/bc15/*.cs | wc -l        # 986
wc -l < /tmp/bc15/index.tsv      # 985
wc -l < /tmp/bc15/verdicts.tsv   # 985
```

The unit is **copied** into the staged directory rather than referenced where it lives, so that
what was compiled can be read afterwards by somebody who does not know how the run was configured.

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

**61 + 924 = 985**, which is AC1's sum. Rows go to stdout (or the named file) and **every other
line goes to stderr**, so the redirect holds rows and nothing else.

**`NOT_COMPILABLE` carries an underscore, and that is a repair to AC1's instrument, not a typo.**
AC1 counts verdicts with `awk '{n[$1]++}'`, whose default field separator is whitespace; the
spelling `NOT COMPILABLE` would split into two fields and be counted as `NOT`. It is invisible
today — the count is 0 of 985 — and it is a trap the first day it is not. Same family as the
criteria that read an exit code through a pipe: the instrument, not the claim.

**The run states its scope before its verdict, and states what it is not:**

```text
985 blocks: 61 BUILT, 924 FAILED
no baseline yet: this is a measurement, not a gate
0 findings
```

**`0 findings` over 924 failures is a true statement and a dangerous one**, so the line above it is
mandatory. Nothing is required to compile until `baseline.tsv` exists in phase 3, and a run that
printed only the zero would read as a green gate over an unchecked corpus. This is the phase that
says *"it is a tool, not a gate"*; the tool has to say it too.

#### Exit 2, proved four ways, every code read bare

**Obligation 3 and review finding 4.** Exit 2's real-world cause makes all 985 blocks fail
*believably*, so the proof is not only the code but **no per-block verdicts printed**:

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

**The second row is the one worth having.** A reference list that is 497 of 498 present is the
state that reads as a measurement: the blocks that needed that assembly fail, with the compiler's
own error codes, and everything else passes. It is exit 2 rather than a warning, because a gate
that checked 99.8% of what it was told to check has not checked the corpus.

`refs.txt` was restored and `diff`ed byte-identical afterwards, and every exit code above was read
with `echo $?` directly after the command, never after a pipe — **constraint 11**, which this
session had already broken once while reading the eight gates.

**Nine states exit 2 and they are named in one place**, as a block comment above `mode_report` in
`tools/blockcheck.py`: four are enforced in Python, three in `tools/blockcheck/Program.cs` and
propagated, and the remaining two — `pagelint` unimportable, and an empty enumeration — sit in
`main`. Four of the nine are proved above; the scaffold-parse one is proved by task 1.8's third
plant.

### Extraction, red-proofed both ways *(task 1.7)*

```bash
python3 tools/blockcheck.py --verify-extraction; echo $?     # 0, read bare
```

```text
985 of 985 identical, 1 with `using` directives hoisted
```

**N of N, with N the corpus count.** The check reconstructs the same split the stager made —
`using` directives hoisted above the wrapper, the rest in place — rather than ignoring the
difference, so "identical" means *every other byte*: not a space, not an indent, not a line ending.

**The red half, and it is one byte.** A single trailing space appended to line 9 of one staged
block, inside the block's own body:

```text
exit=1
984 of 985 identical, 1 with `using` directives hoisted
contents/KafkaConfiguration.md	1	KafkaConfiguration_1	NOT IDENTICAL
```

**Exactly that block, named, and nothing else moved.** The green half is the half that feels
unnecessary and the half that catches a check which has stopped running — 985 of 985 is only worth
something because 984 of 985 is reachable.

#### The hoisting count is 1, and the block it names is a finding for phase 2

`985 of 985 identical, **1** with directives hoisted` looked wrong — 248 blocks carry a `using`
line, so the number that moves lines ought to be larger. It is not: hoisting only *moves* anything
when a directive sits below a non-directive line, and in 984 blocks the directives are already at
the top. The one exception:

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

**That is a before/after pair inside one fence**, of exactly the shape task 2.5 confirms — and it
is not on the design's list, nor on the nine candidates the tasks review produced, because it
carries no `// Before`, `// After`, `// V9` or `// V10` comment. The marker method cannot see it;
**a duplicated `using` directive can**. Carried to task 2.5 as a second method to run, not repaired
here: phase 1 touches no page.

It is also the one block where the difference between *the page's bytes* and *what the compiler
read* is visible, which is why this check reports the hoisting count rather than folding it into
the identical count.

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

The first two are the pair the design asked for, and they are the easy half: a name that does not
exist must come back `FAILED` with the compiler's own code, and a trivial class must come back
`BUILT`, because a harness that had silently stopped compiling would report everything `FAILED` and
read as thorough.

#### Review finding 5 was right that obligation 8 had no red-proof, and wrong about what would give it one

The task list specified a **parse-broken third plant** in the same run as the `CS0246` plant, on the
design's stated mechanism: a parse failure suppresses semantic binding across a shared compilation,
so under batching the `CS0246` plant would go silent.

**It does not go silent.** Built with the blocks deliberately batched into one `CSharpCompilation` —
the regression the rule forbids — all three plants reported exactly what they report per-block:

```text
                      per block                    batched
Plant_Clean           BUILT   0                    BUILT   0
Plant_Missing         FAILED  1  CS0246            FAILED  1  CS0246
Plant_Unparseable     FAILED  5  CS1026,CS1513     FAILED  5  CS1026,CS1513
```

**The third plant cannot fail when the rule is broken**, which is the exact defect review finding 5
set out to fix, one level further in. It was caught by doing what the finding asked — building the
regression and running the control against it — rather than by planting the file and assuming.

#### What batching actually does, measured over all 985

A scratch probe put every staged block in **one** compilation and reported diagnostics per tree:

```text
one compilation per block      985 blocks, 61 built, 924 failing
one compilation, all 985       985 blocks, 64 built, 921 failing
                               6,331 errors: 865 CS1xxx/CS8xxx, 5,466 semantic
```

**Four false `BUILT` verdicts**, named, and every one a tutorial:

```text
TutorialFirstCommand_2   TutorialFirstCommand_3   TutorialFirstMessage_3   TutorialDurableOutbox_3
```

All four fail alone with **`CS0246`** and build in the batch, and the mechanism is **visibility, not
diagnostics**: a tutorial declares a type in one block and uses it in the next, and the 11
`namespaced` blocks are emitted verbatim into the namespace the page wrote. In one compilation,
block 2 resolves a name block 1 declared — which is precisely what a reader copying block 2 alone
cannot do. **The verdict a batch gives those four is the opposite of the fact the gate exists to
state.**

So `Plant_Leak_A` and `Plant_Leak_B` replace the parse-broken plant as obligation 8's guard, and
they reproduce the corpus defect in two files:

```text
                      per block                    batched
Plant_Leak_A          BUILT   0                    BUILT   0
Plant_Leak_B          FAILED  1  CS0246            BUILT   0      <- false clean
```

**`Plant_Unparseable` stays**, because the parse-failure case is still a case — it is how the
scaffold-parse guard is stated, and a corpus of 218 blocks with `CS1xxx` errors is not hypothetical
— but it is no longer claimed to guard obligation 8, because it does not.

#### The correction, stated plainly: obligation 8 stands, its published cause does not

`design.md` § *The finding nobody was looking for* reports that batching suppressed semantic
binding wholesale — *1,444 errors and not one semantic error* across all 985, and *446 blocks
certified clean of which a 20-block sample was 100% false-clean*. **Re-run in this repository, with
Roslyn 4.11 on net9.0, that does not reproduce**: one compilation of all 985 reported **6,331**
errors of which **5,466 were semantic**, and it certified **64** clean against 61 — three more, not
386 more.

**The design's conclusion is untouched and is now proved by a control that can fail.** One
compilation per block remains the only arrangement in which a verdict means anything, obligation 8
is unchanged, and the argument for it is this repository's own measurement rather than an inherited
one. The design is **not edited**: an approved document's figures are anchored at their approval,
and a spec that rewrites its own history keeps no evidence that it ever measured anything.

**What this cost and why it was worth it:** the plant the plan named would have been committed,
recorded as passing, and believed — a red-proof that is green for the wrong reason, which is the
same shape as every plausible zero in this programme's ledger. It took building the regression to
find out. **A control is only a control once you have watched it fail.**

### No vacuous pass, and the second direction that makes the first mean something *(task 1.9)*

**Two consecutive runs, nothing touched:**

```text
run1  exit=0  wall=26s   985 blocks: 61 BUILT, 924 FAILED
run2  exit=0  wall=23s   985 blocks: 61 BUILT, 924 FAILED
diff /tmp/run1.tsv /tmp/run2.tsv      empty
```

**Same corpus count, same distribution, same 985 rows to the byte** — and *that is also exactly
what a tool which had cached, or silently done nothing the second time, would print*. Session 75's
0.54s MSBuild run reporting 0 warnings is the same shape: a tool declining to work and reporting no
problems.

**So the second direction, over an input changed on purpose.** One line added inside block 1 of
`contents/AWSSQSConfiguration.md`, a block that was `BUILT`:

```csharp
        NoSuchTypeXyz123 deliberate = null;
```

```text
run3  exit=0   985 blocks: 60 BUILT, 925 FAILED

1c1
< BUILT	contents/AWSSQSConfiguration.md	1	AWSSQSConfiguration_1	0
> FAILED	contents/AWSSQSConfiguration.md	1	AWSSQSConfiguration_1	1	CS0246
```

**One block moved and exactly one line of the report changed.** The change was then reverted:

```bash
git checkout contents/AWSSQSConfiguration.md
git diff contents/AWSSQSConfiguration.md      # empty
```

```text
run4  exit=0   985 blocks: 61 BUILT, 924 FAILED
diff /tmp/run1.tsv /tmp/run4.tsv      empty
```

**Four runs: same, same, different, same again.** Sameness is only evidence once difference has
been shown to be reachable, and run 4 is what says the difference was the input rather than the
instrument drifting.

**Do not quote a single wall-clock as the figure.** 26s and 23s here; the probe's four runs over
identical inputs gave 6.5s, 22.9s, 39.5s and 26.2s. The cost of this corpus is *tens of seconds*,
which is the shape of the number phase 3's CI job is budgeted from — not 23.

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

**Eight for eight, and `pagelint --changed origin/master` is green too** — the mode that matters on
a pull request, and the one that would have caught a C# block added to a page.

**The mechanism held where it was most likely to break.** `linkcheck` walks `tools/`, so the
prediction depended on this phase adding **no `.md`** there, and it did not:

```bash
git diff --cached --name-only | grep '^tools/' | sed 's/.*\.//' | sort | uniq -c
#    7 cs      2 csproj      1 py      2 tsv      4 txt
git diff --cached --name-only --diff-filter=A | grep '\.md$'      # nothing
```

Seventeen files changed and not one of them is a page or a `.md`. The gate's documentation is row 9
of `tools/README.md`, which task 3.7 writes; a `tools/blockcheck/README.md` would have taken
`linkcheck` to 166, and that would have been a choice rather than an accident.

**"None" for all eight is a vacuous pass eight times over**, which is why the before-figures were
read at the top of the phase and cited to `tools/README.md` rather than re-derived at the end from
the same run. A gate that had silently stopped checking reads exactly like a gate correctly
reporting no movement.

---

## Phase 2 prediction

**The prediction is the task list's own, and it pre-dates the work by commit order rather than by a
date in prose.** Task 2.6's *Notes* read **"prediction is none; this phase commits a `.tsv` and edits
this file"**, and they were approved at `7b493c3` — before a line of phase 2 ran. That is the form
015's acceptance walk settled on: *"the prediction came first"* is checked with `git log`, not with a
sentence claiming it.

**All eight: none**, and the mechanism is phase 1's, unchanged. No page under `contents/` is edited,
so rules 1–7, `symbolcheck`'s corpus, `optioncheck`'s marked tables and `versioncheck`'s prose pins
cannot move; `SUMMARY.md`, `.gitbook.yaml` and the published tree are untouched, so shape, redirects
and `--verify` cannot. `linkcheck` walks `tools/`, so the prediction depends on this phase adding no
`.md` **there** — and this phase adds a `.tsv`, two `.cs` plants, and edits to a `.py`, a `.cs` and a
`.csproj`.

**Eight vacuous passes again**, with the same defence as phase 1: the before-figures were read
against `tools/README.md` at the top of the phase, not re-derived from the closing run.

---

## Phase 2 as executed

### The corpus run *(task 2.1)*

**985 blocks: 68 BUILT, 917 FAILED, 0 SKIPPED, 0 NOT_COMPILABLE.** The command, and its exit code
read bare:

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

**`tools/blockcheck/verdicts.tsv` is that run, committed** — 985 rows, one per block, `verdict TAB
page TAB ordinal TAB ident TAB error count TAB distinct codes`. It carries no header, deliberately:
phase 3 diffs a run against a baseline, and a comment row would have to be stripped by both sides.

**The run is the committed form from a clean directory** — obligation 12. `git ls-files` piped
through `tar` into a scratch directory, both projects built there, and the output `diff`ed against
the in-tree run: **identical, 985 of 985 rows**. The clean copy is what produced `verdicts.tsv`.

**The four counts sum to the corpus count, and two of them are 0 for reasons that are not the same
reason.** Phase 2 made the run print all four:

| Verdict | Count | Why |
|---|---:|---|
| `BUILT` | **68** | compiles against the released packages, with its declared wrapper and page scaffold |
| `FAILED` | **917** | at least one error attributed to the block's own tree |
| `SKIPPED` | **0** | **there is no opt-out to carry it.** Q5 is open and `<!-- blockcheck: skip … -->` is phase 3's |
| `NOT_COMPILABLE` | **0** | `classify()` returns a shape for every block, so nothing can reach this verdict — see the parse triage below, which is where that stops being a good thing |

> **A summary that lists only the verdicts it saw cannot be added up.** Before this change the run
> printed `61 BUILT, 924 FAILED`, which is indistinguishable from a tool that has no `SKIPPED`
> verdict at all — and AC1 asks for *four counts summing to the corpus count*. `VERDICTS` is now the
> vocabulary in one place and the summary is built from it.

**Per shape, because the wrapper is the thing most likely to be buying a verdict** (AC14's subject):

```text
namespaced      4 built of   11
toplevel        0 built of    9
types          29 built of  303
members        11 built of  130
statements     24 built of  532
```

**And per page, which is the number phase 3's baseline actually needs:** 35 pages have at least one
block that builds, and **only 5 pages — 7 blocks — build in their entirety**
(`BoxProvisioning.md`, `FirestoreInbox.md`, `FirestoreOutbox.md`, `SpannerInbox.md`,
`SpannerOutbox.md`). A per-*page* ratchet would therefore admit 7 blocks of 985. The baseline has to
be per block, and phase 3's task 3.1 should say so.

**The design measured 60 clean with no scaffold; this run reads 68.** The movement is accounted for
and none of it is drift: **+1** `DapperOutbox.md` from phase 1's scaffold unit, **+7** from Q3
admitting Darker's four packages (task 2.4). With the reference set at phase 1's 497 assemblies the
same instrument reads **61**, which is phase 1's figure re-derived rather than inherited.

### The parse triage *(task 2.2)*

**180 of 985 blocks do not parse.** Three numbers exist for this and they are not the same
measurement, which is the finding:

| | Count | Instrument |
|---|---:|---|
| `design.md` § *The probe* | **131** | the probe, a different TFM and reference set, un-triaged |
| task 2.2's own filter — `CS1002`/`CS1513`/`CS1519`/`CS8635` | **165** | `grep -cE` over the corpus run |
| **the parser** | **180** | `blockcheck --parse`, syntax-only, no references and no binder |

**The filter is a subset, and the 22 blocks it misses are named.** `comm` over the two id lists:
**22 in the parser's set and not the filter's, 0 the other way.** They fail on `CS1525`,
`CS1022`, `CS0116`, `CS8124`, `CS8803` — `AzureBlobDistributedLock_2`, `FAQ_16`,
`HangfireScheduler_18`, `MigratingToPollyV8_12`, `QuartzScheduler_15`, `QuartzScheduler_16`,
`ShowMeTheCode_3`, `SweeperCircuitBreaking_4`, `UsingTheContextBag_16`, `V10MigrationGuide_19` and
twelve more. **"And friends" is an enumeration by guesswork**, and it under-reports in the direction
that makes the corpus look healthier. `--parse` asks the parser instead:
`SyntaxTree.GetDiagnostics()` *is* the parse.

**Two-way red-proof of `--parse`, against the plants and not against `contents/`:**

```text
Plant_Unparseable   BROKEN   5   CS1026,CS1513     <- the check fires
Plant_Missing       PARSES   0                     <- FAILS the compile on CS0246, and parses
Plant_Clean         PARSES   0
Plant_Leak_A/B      PARSES   0
```

`Plant_Missing` is the direction that matters: it is a *failing* block that parses, so the mode is
separating parse from bind rather than echoing the compile verdict. Bad arguments and a directory
with no `index.tsv` both exit **2** with no rows.

**The triage, 180 blocks across 67 pages, and the class the task list did not name is the biggest
one:**

| | Blocks | What it is | Named example |
|---|---:|---|---|
| **A documented omission written where C# needs a token** | **93** | `...` or a comment standing in for an expression — `opt.Outbox = /* your MS SQL Outbox */;`, `Partition = //derived from the region…`, `.AddBrighter(options => { ... })`. The page is honest and the fence can never parse | `MsSqlDistributedLock.md#2`, `AWSSQSConfiguration.md#2` |
| **An excerpt of a larger expression** | **77** | the fence holds part of an argument list, an object initializer or a fluent chain — `new Subscription(` with no `;`, a block opening on `.UseAsyncApi(opts => …)`, `OnConflict = OnSchedulerConflict.Overwrite` alone | `AsyncAPISupport.md#6`, `AwsScheduler.md#23` |
| **A declaration and a statement in one fence, in the order C# forbids** | **10** | a full class, then the line that registers it. C# allows top-level statements *above* type declarations and not below: `CS8803`. 9 of the 10 are exactly that; the tenth is two constructor signatures with elided bodies | `HangfireScheduler.md#18`, `UsingTheContextBag.md#16` |
| **before/after pair in one fence** | **0** | task 2.5's set is in the triage's scope and not in its result: all four confirmed pairs **parse**, and fail on binding | — |
| **a genuine fragment the wrapper mis-shaped** | **7 → 0** | measured, fixed in this PR, below | `FAQ.md#16` |

**The wrapper question was answered by trying all four rules, not by opinion.** Every one of the 187
parse failures was re-staged under each of the four wrapper rules — 748 variants — and the parser
asked which of them parse. **7 blocks parse under a rule `classify()` had not chosen**, and all 7 are
one shape: **statements above a declaration**, which is a `Program.cs` and needs no wrapper at all.
Wrapped as `types` the leading statements land at namespace level and the block cannot parse under
any circumstances.

So `classify()` gained a fifth shape, `toplevel`, with the empty wrapper:

```text
before   11 namespaced, 306 types, 136 members, 532 statements
after    11 namespaced, 9 toplevel, 303 types, 130 members, 532 statements
parse    187 broken  ->  180 broken          exactly the 7 the variant sweep predicted
verdict  0 blocks moved BUILT -> FAILED or FAILED -> BUILT
```

**Order is the whole rule, and that is what stops it over-reaching.** A statement *after* a
declaration is `CS8803` and cannot parse unwrapped either, so `UsingTheContextBag.md` block 16 — a
class, then a line of usage — is **not** this shape and stays a page defect. The two look alike in a
diff and the compiler separates them. `plants/Plant_TopLevel.cs` and `Plant_TopLevel_Bad.cs` are
that pair, and the difference between them is line order:

```text
Plant_TopLevel                        BUILT    0
Plant_TopLevel_Bad                    FAILED   1   CS8803
Plant_TopLevel, wrapped as `types`    FAILED  10   CS0106,CS0116,CS1002,CS1022,CS1026,CS1031,CS1520,CS8124
```

**The third line is the red-proof**: the same plant, under the rule the old code chose, with ten
errors. A rule that rescued both plants would be rescuing by shape rather than by grammar.

**And the fix uncovered an instrument artefact underneath it.** A block with top-level statements is
only legal in an executable, so every `toplevel` block earned `CS8805` — *"Program using top-level
statements must be an executable"* — a verdict about `OutputKind.DynamicallyLinkedLibrary` and not
about the page. The Roslyn half now asks the syntax tree whether the block has global statements and
compiles those as `ConsoleApplication`: **`CS8805` on 15 blocks before, 0 after, and no verdict
changed.** It is asked of `GlobalStatementSyntax` rather than read from the staged index's shape,
because the index is Python's *claim* and the tree is what the parser found.

### Claim and context *(task 2.3)*

**This is the boundary the maintainer's ruling did not set, and it is flagged in `requirements.md`
P0-9 as the sentence to overrule.** The answer: **the claim list is small — 14 blocks across 12
pages — and phase 4 is a phase, not a spec.** The context count is the rest, and it is large by
construction.

**The split is measured in four steps, each with its command:**

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
| **cascade of an unresolved name** | 22 | 20 | the named type, or an argument's type, is one the same block failed to resolve. **Context** |
| **the page's own type, partly shown** | 22 | 8 | `CS0535` on `MyOutbox`, `CS1729` on a handler the page declares elsewhere — the page elides members it does not need. **Context** |
| **a library name** | 35 | 19 | the type resolved out of a pinned package and the member or signature is not there. **The claim list's input** |

**Of the 19, eleven name a Brighter or Darker type and eight name a third-party one**, and the two
halves are not equally decidable:

| Block | Diagnostic | Verdict |
|---|---|---|
| `Telemetry.md#1` | `InstrumentationOptions` has none of `RecordRequestInformation`, `RecordRequestBody`, `RecordRequestContext`, `RecordMessageInformation`, `RecordMessageBody`, `RecordMessageHeaders`, `RecordServerInformation` | **claim.** The released names are `RequestInformation`, `MessageBody`, `MessageHeaders` — the `Record` prefix is gone. Seven members, one page, and the page is *about* telemetry |
| `TurningOnReplayOnSeen.md#1`, `#6` | `OnceOnlyAction` has no `Replay`; and `contextKey:` takes `string?`, not `System.Type` | **claim**, twice in one attribute, on the page about replay-on-seen |
| `CausationTrackingStores.md#1`, `ReplayOnSeenReference.md#1` | `RequestContextBagNames` has no `CausationId` | **claim** |
| `CQRSWithBrighterAndDarker.md#7` | `RequestLoggingAttribute` has a required `timing` parameter the block omits | **claim** |
| `InMemoryOptions.md#2`, `#3` | `IAmACommandProcessor.ClearOutbox` has a required `posts`; `IAmAnOutbox` is not generic | **claim** |
| `InMemoryScheduler.md#4` | `IAmAMessageSchedulerFactory` cannot be the `T` the block passes it as | **claim** |
| `PostgreSQLMessageBroker.md#3` | a `RelationalDatabaseConfiguration` where a messaging-gateway configuration is wanted | **claim** |
| `Logging.md#4` | `INeedAHandlers` has no `Build` | **struck out by reading.** The block's `// ... handler configuration, policies …` elides the chain steps *between* `StartNew()` and `Build()`. It is class A of the parse triage wearing a semantic diagnostic |
| `QuartzScheduler.md#1`, `TickerQScheduler.md#1` | `IServiceProvider` has no `GetRequiredService` | **struck out, and proved.** `GetRequiredService` is an extension method; the block lacks the `using`. Two planted blocks differing only by `using Microsoft.Extensions.DependencyInjection;` read **FAILED CS1061** and **BUILT** |
| `ConfiguringOpenTelemetry.md#1`, `#6`, `#7`, `HangfireScheduler.md#11`, `MigratingToPollyV8.md#1`, `#8` | `AddJaegerExporter`, `TracerProvider.Run`/`RunAsync`, `DashboardContext.GetHttpContext`, `int.Seconds` | **undecided, and it is the pin's question not the page's.** No Jaeger exporter and no `Hangfire.AspNetCore` assembly is in the reference set at all. Phase 3 decides whether the pin grows; only then does the diagnostic mean anything about the page |

**So P0-9's input is 10 blocks of claim plus task 2.5's 4 fence-pairs — 14 blocks across 12 pages.**

> **The claim list is a FLOOR, and the mechanism is demonstrable rather than argued.** A block that
> fails to resolve its own base type never reaches the binding that would expose a false claim
> underneath. `ImplementingAHandler.md` block 1 reports three diagnostics, of which `CS1729` on
> `Command` is a cascade; add the two `using` directives it omits and it is **BUILT** — so there was
> no claim defect hiding there. That cuts both ways, and it is why the ratchet is page by page:
> **every block admitted to the baseline is one whose claims have actually been checked.** The 917
> failures cannot be triaged once, at the top, by a tool.

**The context count is 903 blocks** — 917 failing less the 14 — and it is dominated by `CS0246`
(736 blocks) and `CS0103` (675). That is backlog item 2, the debt `tools/README.md` row 2 counts as
744 `using`-directive warnings, and it falls page by page as pages enter the baseline.

### Q3 ruled by measurement — Darker joins *(task 2.4)*

**Yes.** Darker's packages restore alongside Brighter's with no conflict, and admitting them moved
**7 blocks from FAILED to BUILT and 0 the other way**:

```text
refs.txt   497 assemblies  ->  501       61 built  ->  68 built
gained     ImplementAQueryHandler#1 #2, PaginationQueryPatterns#1,
           ParameterizedQueryPatterns#3 #5, QueriesAndQueryObjects#2 #3
lost       none
```

**Four packages, not six.** `Paramore.Darker`, `.AspNetCore`, `.Policies` and `.QueryLogging` at
**4.1.1**; `requirements.md` P2-2's other two strings — `Paramore.Darker.Builder` and
`Paramore.Darker.Policies.Constants` — are a namespace and a type, and pinning them would fail the
restore. Darker versions independently of Brighter and the pin says so in its own comment.

**The work is carried here, in phase 2, and the sentence is the one task 2.4 asked for**: the pin is
phase 1's file, so a *yes* edits `refs.csproj` from phase 2 — done — and **Darker's blocks then enter
phase 3's baseline like any others.** 17 Darker-only pages carry 135 of the 985 blocks, so this is
not a rounding error in the corpus; P2-2 is promoted to in-scope by this ruling, and the task that
owns it is phase 3's baseline task rather than a new one.

### The before/after-in-one-fence set *(task 2.5)*

**Confirmed: 4 blocks across 2 pages.** The design said six across four, and the difference is all
reading:

| Block | Two methods | Verdict |
|---|---|---|
| `ImplementAQueryHandler.md#10` | `CS0101` **and** markers | **confirmed** — two `GetOrderQueryHandler` classes in one fence, the corpus's only `CS0101` |
| `CloudEventsSupport.md#7`, `#8` | `CS0128` **and** markers | **confirmed** — `var messageId` / `var correlationId` declared twice, V9 then V10 |
| `CloudEventsSupport.md#9` | markers only | **confirmed**, and it is **task 4.3a's case**: two method *signatures* with no bodies, so splitting yields two fragments and no compilable block |
| `AgreementDispatcherRouting.md#3` | markers only | **struck out.** `// Before Jan 2025` / `// After Jan 2025` are tax rules inside one routing lambda, exactly as the tasks review said |
| `PolicyRetryAndCircuitBreaker.md#6` | markers only | **struck out.** The V9 form is *commented out* inside a single live class — already the shape `CLAUDE.md` prescribes, not a pair of live declarations |

**The design's claim that the grep is the only instrument that sees five of the six is false, and
that matters more than the count.** `CS0101` is duplicate *types* only; the family is
`CS0101`/`CS0111`/`CS0128`, and with all three the compiler sees **21 blocks**, including
`CloudEventsSupport.md#7` and `#8`. So method 1 is the wider net here and the marker grep is the
narrower one:

```bash
grep -E 'CS0101|CS0111|CS0128' tools/blockcheck/verdicts.tsv | cut -f4   # 21 blocks
# markers, both vocabularies, comment-initial only:
#   ^\s*//+\s*(Before|V9|Old)\b  AND  ^\s*//+\s*(After|V10|New)\b        #  6 blocks
```

**The marker vocabulary has to be narrow or it is useless.** A wide version — any comment mentioning
*before*, *after*, *old*, *new*, ❌ or ✅ — returns **18 candidates**, including three tutorials
whose prose says *"before the call"*. Comment-initial markers return 6, of which reading keeps 4.

**And the 21 the compiler finds are mostly a different shape, which phase 4 should not silently
inherit:**

| | Blocks | Shape | P0-9? |
|---|---:|---|---|
| version pair, V9 → V10 | 3 (+1 marker-only) | the set above | **yes** |
| bad/good pair, ❌/✅ or `// Good`/`// Bad` | 9 | `QueryPatterns.md#2`, `NullableReferenceTypes.md#10`, `HangfireScheduler.md#25` … | **no** — nothing false is claimed; it is a teaching device |
| problem/solution, or two alternatives | 7 | `AWSSQSMigrateToV10.md#5` `#6`, `BrighterSchedulerSupport.md#2` `#3`, `PostgreSQLBrokerTradeOffs.md#1` | **no**, on the same reasoning |
| duplicate for another reason | 2 | `FAQ.md#8`, `InMemoryOptions.md#2` | **no** |

**All 18 share one mechanism with the 4** — two mutually exclusive snippets in one fence, so the
fence can never compile — and the repair is identical: one fence each. **That is a recommendation and
not a decision:** P0-9 is scoped to defects of *claim*, and a ❌/✅ pair asserts nothing false. If the
maintainer wants the shape repaired wherever it appears, phase 4 grows by 18 mechanical splits and
this is the sentence to say so against.

**`AWSSQSMigrateToV10.md#1` — phase 1's finding — is not in either list**, and that is the honest
result rather than an oversight: it is a before/after pair carrying **no marker at all**, found only
because `--verify-extraction` reported one block with a duplicated `using`. A third method, for one
block, and it stays on the record as the case both of these methods miss.

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

**Eight for eight.** `tools/README.md` owns these figures and this table cites them; the one row that
could have moved by accident is `linkcheck`, and the phase added no `.md` under `tools/`.

### Frictions 57, 58 and 59 — for task 5.5, which writes the ledger

| | |
|---:|---|
| **57** | **A generated reference list survives the build that failed to produce it.** Adding Darker to `refs.csproj` left a double hyphen inside an XML comment, so the project failed to **load**; no target ran, the previous `refs.txt` stayed on disk, and the corpus run that followed reported the same **61 built** and looked healthy. It had measured the old pin. An *absent* reference list was already exit 2; a **stale** one was indistinguishable from a current one, and it is the more believable failure because nothing is missing. Deleting the file before the build would not have caught it either — a project that fails to load runs no targets. `refs.txt` now opens with `# refs.csproj SHA256 …` and `blockcheck.py` hashes the project and compares: wrong stamp, or no stamp, is exit 2 with no report file at all. Red-proofed both ways |
| **58** | **`zsh` does not word-split an unquoted variable, so a 68-argument list arrives as one argument.** `--explain … $ids` printed **nothing**, and nothing is exactly what "no claim defects in the corpus" looks like. The same shape as friction 52 in the reassuring direction: a control that measures nothing reports agreement. `${=ids}`, or an array, or counting the arguments the tool received |
| **59** | **A verdict nothing can emit reports zero of itself and the zero reads as coverage.** `NOT COMPILABLE` is **0 of 985** — `design.md` Q4 calls that *"a verdict that currently has no members"* and phase 1 called it *"by construction"* — yet **77 blocks are excerpts of a larger expression** and **93 carry an omission where C# needs a token**. They are reported as `FAILED`, which says the documentation is broken when what is true is that the fence was never a program. `classify()` cannot return *no*, so the verdict is unreachable rather than empty. **Phase 3's decision, flagged and not taken here:** the opt-out Q5 describes is the mechanism, and the direction of risk runs the wrong way — a heuristic that guesses `NOT COMPILABLE` would silence real defects, so the reason has to be written on the page by a person |

**Phase 2 is six tasks and it changed the instrument in four places**, each one a defect it met:
the fifth shape (`toplevel`), the output kind for top-level statements, the `--parse` and `--explain`
modes, and the pin stamp. `--verify-extraction` still reports **985 of 985 identical**, and the seven
plants read exactly as `plants/index.tsv` predicts.

---

## Phase 3 prediction

**The prediction is the approved task list's own, and it pre-dates the work by commit order.** Task
3.9's *Notes* read **"`linkcheck` is predicted to MOVE if and only if this phase adds a `.md` under
`tools/`. The design chose not to. If the reconciliation shows 166, the cause is a file somebody
added without noticing the rule"** — approved at `1f5fc10`, before a line of phase 3 ran. That is
015's form: *"the prediction came first"* is checked with `git log`, not with a sentence claiming it.

**All nine: none**, and row 9 is a special case worth stating rather than glossing. No page under
`contents/` is edited in this phase — that is the whole reason phase 3 is site-neutral and phase 4 is
the only one needing a sign-off — so rules 1–7, `symbolcheck`'s corpus, `optioncheck`'s marked tables
and `versioncheck`'s prose pins cannot move; `SUMMARY.md`, `.gitbook.yaml` and the published tree are
untouched, so shape, redirects and `--verify` cannot. `linkcheck` walks `tools/`, and this phase adds
a `.tsv`, a `.json`, edits to a `.py` and a `.yml`, and **prose inside an existing `.md`** — row 9
goes into `tools/README.md`, which is already in the corpus at 165.

**Row 9 predicts its own first figure rather than a movement**, since a gate that did not exist
cannot have moved. Its expected reading is recorded when task 3.7 writes it, with the ref it was
measured at, per obligation 10.

**Nine vacuous passes**, with phases 1 and 2's defence: the before-figures are read against
`tools/README.md` at the top of the phase, not re-derived from the closing run.

**AMENDED BY TASK 3.2, ruled by the maintainer in session 81: `contents/` IS edited, and the site
is still not.** The paragraph above says *"No page under `contents/` is edited in this phase"*, and
task 3.2 cannot be done without editing one: a skip marker lives on the page, above the block it
excuses. **Five pages gain twelve lines, every one an HTML comment**, which renders to nothing. The
same shape is already on six pages as `<!-- pagelint: allow-serviceactivator -->`. So phase 3
remains site-neutral in the sense obligation 7 cares about, **no sign-off is owed**, and the prediction
for rules 1–7, `symbolcheck`, `optioncheck` and `versioncheck` stands, because none of them reads
an HTML comment as prose. The paragraph above is left as written, since this amendment is the
record of it being wrong.

---

## Phase 3 as executed

### The opt-out, and why this one has to say why *(task 3.1)*

**Q5 is ruled: `<!-- blockcheck: skip <reason> -->`, on its own line, binding THE NEXT C# BLOCK, and
the reason is part of the syntax.** Two of those three follow the opt-outs this repository already
has. The third does not, and the difference is the ruling.

`pagelint` has `<!-- pagelint: allow-serviceactivator -->` and `symbolcheck` has
`<!-- symbolcheck: allow IMessageScheduler -->`. **Neither carries a reason, and neither needs one:**
each names what it silences, the page discusses that name, and a reader who wants the reason reads
the paragraph the marker sits in. **A block that fails to compile carries no such recovery.** It can
fail for a dozen reasons; "somebody decided this one was fine" is not checkable against anything, and
`design.md`'s own words for friction 59 are that *"the reason has to be written on the page by a
person, never guessed by a heuristic"*. So a marker without a reason binds nothing and is reported.

**Page-wide was rejected, and the argument is `symbolcheck`'s transplanted verbatim.** Its `opt_outs`
docstring already says a page-wide silence *"would let a page opting out of a name it discusses on
purpose silently opt out of a second, dead name that arrived on that page two years later, and
nothing would ever say so"*. A page-wide `skip` written for one ❌ V9 example would absorb the next
block someone adds beneath it. The marker binds one block.

**Three ways a marker binds nothing, and they are not one defect:**

| | Reported as | Why |
|---|---|---|
| no reason given | **error**, a finding, exit 1 | It reads as an opt-out and grants none. The block it appears to excuse is **still judged** — which is the trap, and the reason the reason is mandatory |
| no C# block follows it | warning | Dead weight. `symbolcheck`'s rule: debt that fails the build gets deleted rather than understood |
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

**The last row is the control that makes the other four mean something** — a page with no marker
produces no binding *and no problem*, so the three problem kinds are not firing on everything they
see. **The positive case is first and it passes**, which obligation 3 requires of a two-way control.

#### And end to end, over the whole corpus, four runs

`contents/MigratingToPollyV8.md` block 5 — line 97, the `❌ **V9 — superseded**` block, and a real
member of task 3.2's set rather than a plant:

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

**Run 3 is the one worth reading twice.** The marker is still there and still says `skip`; the block
is back at `FAILED` and counted. A reasonless opt-out does not quietly become a silence — the
corpus is judged exactly as if the marker were absent, and the run says so and fails.

**Run 4 is phase 1's no-vacuous-pass discipline**: `git checkout -- contents/MigratingToPollyV8.md`,
`git diff --stat` on that path empty, and `diff` of run 4's report against run 1's reports no
difference across all 985 rows. Same, different, different, same again.

**`0 findings` and `0 findings, 1 skipped` are printed as different strings**, which is ruling 4 and
was already wired in phase 1 — this task gave it something to count. The four verdicts still sum to
985 in every run above, which is AC1.

#### Two things this task changed beyond the marker

**`VERDICTS`' comment was carrying a claim that had just become false.** It read *"SKIPPED has no
opt-out to carry it until phase 3"*; phase 3 is now, so it says what is true instead, and it takes
the correction phase 2 made to `NOT_COMPILABLE` with it — **empty by construction, which friction 59
records as a defect rather than as coverage.**

**`enumerate_blocks` now filters the page's C# fences once and enumerates them**, where it used to
carry a manual `ordinal` counter through a loop over every fence. The refactor is not cosmetic: the
skip scan needs the list of C# fence start lines *before* the blocks are built. It is behaviour-
preserving and was checked as such — `--list` reads **985 C# blocks across 145 pages: 11 namespaced,
9 toplevel, 303 types, 130 members, 532 statements**, identical to phase 2's figures on every one of
the five shapes.

### The V9 skips, and why the design's 8 was the wrong 8 *(task 3.2)*

**Re-derived rather than inherited, and the set is 12 blocks across 5 pages. The design's 8 was a
different set.** Two methods, then reading:

```text
grep -rn '^❌' contents/   ->    8 lines, 3 pages     the design's figure, reproduced
grep -rn '❌'  contents/   ->   43 lines, 11 pages
```

**Of the anchored 8, one is a version marker.** `MigratingToPollyV8.md:95` reads
`❌ **V9 — superseded**`. The other seven are `❌ Bad:` in `QueryPipeline.md` (5) and
`CQRSWithBrighterAndDarker.md` (2), which is the bad/good convention and has nothing to do with
`CLAUDE.md` § *Version markers on code*. **Of the unanchored 35, none is a version marker either.**
Reading all 43 lines by context, they are table cells (`BrighterSchedulerSupport.md`, `FAQ.md`),
pro/con bullets (`EFCoreQueryIntegration.md`, `PaginationQueryPatterns.md`, `FAQ.md`), or trailing
comments inside a fence marking a problem line (`AWSSQSMigrateToV10.md`, `KafkaConfiguration.md`,
`QueryPatterns.md`, `TurningOnReplayOnSeen.md`, `EFCoreQueryIntegration.md`). The claim PROMPT.md
flagged for checking, *"most are inline in tables and prose"*, holds for all 35.

**So ❌ finds one V9 block, and the V9 blocks are not the ❌ blocks.** A third method, the label
directly above each C# fence, finds eleven V9 forms that carry no ❌ at all, because those pages
predate the convention and label in their own words: `**Before (V9)**:`, `**V9**:`, `**Old (V9):**`,
`### V9 Configuration (Deprecated)`, `### Example with Legacy Policies (V9)`. **The task's own
Notes are the reason they belong:** *"`CLAUDE.md` requires these blocks to exist"* is true of a
superseded form whether or not a ❌ sits above it.

| Page | Blocks | Label on the page |
|---|---|---|
| `MigratingToPollyV8.md` | #1, #3, #5 | `**V9**:` twice; `❌ **V9 — superseded**` |
| `V10MigrationGuide.md` | #1, #4, #6, #8, #19, #15 | `**Before (V9)**` ×5; `Before:`, a `Guid` request Id, which the prose above it names as V9 |
| `FAQ.md` | #15 | `**Old (V9):**` |
| `ReactorAndProactor.md` | #11 | `### V9 Configuration (Deprecated)` |
| `PolicyFallback.md` | #2 | `### Example with Legacy Policies (V9)` |

**Three were read and left out, and the reason for each is written down so that nobody adds them by
pattern:**

- `V10MigrationGuide.md` #23, `**Before**:` under KIP-848: V10 API marked `[Obsolete]`, which the page
  says *"still work"*. That makes it a V10 block with a warning, and the gate should judge it.
- `MigratingToPollyV8.md` #7–#11, under `### Using Brighter's UsePolicy Attribute (Legacy)`: the page
  presents these as *legacy* V10 usage and does not label them V9. The page labels them, and
  this set does not relabel them.
- `AWSSQSMigrateToV10.md` #2, `**V3 Approach**`: that is the AWS SDK's v3, used through a Brighter V10
  package. The page is about the SDK, not about Brighter V9.

**The rule is the page's label, not whether the API was removed, and that choice has a cost.** It
was nearly written the other way. The first draft of this record excluded #7–#11 because
*"`UsePolicy` still ships"*, and checking that claim against `../Brighter/src` found that it is true
of **four of the twelve as well**. `FAQ.md` #15 and `MigratingToPollyV8.md` #3 use `TimeoutPolicy`,
`PolicyFallback.md` #2 uses `UsePolicy` and `FallbackPolicy`, and `MigratingToPollyV8.md` #1 uses a
Polly v7 `PolicyRegistry`. All four still ship in V10, `UsePolicy` marked
`[Obsolete("Migrate to UseResiliencePipeline")]`. The other eight are described by their pages as
surfaces V10 removed or changed, such as `isAsync`/`runAsync` on `Subscription` or a `Guid` request
Id. **Those eight were not each checked against source**, so *"an API-removal rule would give eight"*
is an upper bound, not a count. A label rule gives twelve. The label rule was chosen because
it is the one a reader can check against the page. **The cost:** those four could be made to compile
against the pin with a scaffold, and the skip now means the gate will not notice if they stop doing
so. The skip is acceptable for a form the page tells the reader to migrate away from. It is a choice,
not a certainty, and this paragraph exists so it can be overruled.

**All 12 were `FAILED` before the markers went in. None was `BUILT`.** A skip on a building block
would have hidden nothing and still claimed an exemption. That was checked against run 0 rather than
assumed.

#### The run

```text
run 0  control, unmodified   985: 68 BUILT, 917 FAILED,  0 SKIPPED, 0 NOT_COMPILABLE   exit 0
                             0 findings
run 1  twelve markers        985: 68 BUILT, 905 FAILED, 12 SKIPPED, 0 NOT_COMPILABLE   exit 0
                             ----- skipped by opt-out (12) -----   each with its reason
                             0 findings, 12 skipped
```

**`diff` of run 0 against run 1 shows exactly 12 rows changed, every one of them `FAILED → SKIPPED`**,
with page and ordinal unchanged on all 985. The markers therefore bound the blocks they were written
for and did not shift another page's ordinals. `git diff --stat` reads **5 files, 12 insertions, 0
deletions**. Every reason names the label the page uses, so a reader can check a skip against the
page without trusting the tool's word for it.

**Seven of eight gates were re-run after the edit, and all read `tools/README.md`'s figures:**
`linkcheck` 165/0; `pagelint` 0 errors, 744 warnings, 162 pages; shape 161/12/12-of-20/4-of-4;
redirects 77 entries, 7858 bytes; `versioncheck` 0 of 18 across 5; `optioncheck` 0 across 59
tables, 519 rows; `symbolcheck` 0 findings, 22 entries, 161 pages, 3 silenced. `--verify` was not
run. It compares against the published tree, and nothing it reads has changed.

**This does not add ❌ to the eleven.** Putting the convention's label on a page is a change a
reader sees, and that belongs in phase 4 or the backlog. The skip reason says what each block is
without it.
