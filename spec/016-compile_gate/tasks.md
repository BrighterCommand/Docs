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

- [ ] **Task 1.1:** Write the enumerator and extractor in `tools/blockcheck.py`
  - Input: `tools/pagelint.py` (`FENCE_RE`, `class Page`, `load_pages`), `spec/016-compile_gate/probe/gen.py`, `spec/016-compile_gate/harness/extract.py`
  - Output: `tools/blockcheck.py` with a `--list` mode printing one line per C# block as `page<TAB>ordinal<TAB>shape`, and **no absolute path anywhere** — `grep -c '/Users/' tools/blockcheck.py` → 0
  - Notes: the probe and the harness both hardcode a path to `tools/`; that is acceptable in a probe and is the first thing a tool must not do.

- [ ] **Task 1.2:** Implement the four wrapper rules and re-derive their counts
  - Input: `design.md` § *The four wrapper rules*
  - Output: the classifier in `tools/blockcheck.py`, and a table in this file recording `namespaced / types / members / statements` **measured twice** — once by `blockcheck.py --list`, once by an independent script — with both commands shown
  - Notes: the design's figures are 11 / 306 / 136 / 532 summing to 985. They are the *design's*; obligation 1 says measure, not inherit.

- [ ] **Task 1.3:** Write the Roslyn compiler tool
  - Input: `spec/016-compile_gate/probe/roslyn/Program.cs`, `tools/optioncheck/Program.cs` for the committed-tool shape
  - Output: `tools/blockcheck/blockcheck.csproj` and `tools/blockcheck/Program.cs`, compiling **one `CSharpCompilation` per block** and emitting `id<TAB>verdict<TAB>error count<TAB>distinct codes`
  - Notes: obligation 8 is the whole design here. A single `Compilation` holding many blocks is the defect, not an optimisation.

- [ ] **Task 1.4:** Write the reference project — the pin, in one place
  - Input: `tools/optioncheck/optioncheck.csproj`, `spec/016-compile_gate/probe/refs.csproj`
  - Output: `tools/blockcheck/refs/refs.csproj` — the package set with an explicit version on every entry, `CopyLocalLockFileAssemblies=true`, **no sources**, and a comment saying the pin lives here and nowhere else
  - Notes: a project that compiles blocks cannot also assemble references — its build fails and leaves `bin/` empty. That is friction 56, and this task exists because of it.

- [ ] **Task 1.5:** Implement the scaffold mechanism and its listing
  - Input: `spec/016-compile_gate/harness/preludes/`, `harness/core/blocks/Scaffold.cs`, `design.md` § *The scaffold*
  - Output: `tools/blockcheck/scaffold/` holding at least the harness's four preludes, and `--list-scaffold` printing every identifier supplied from outside a page, with a count
  - Notes: AC8. A `CLEAN` verdict that does not say what it was given is not a verdict.

- [ ] **Task 1.6:** Implement the exit-code contract and the report modes
  - Input: `tools/README.md` § *Exit codes — one contract, all of them*
  - Output: `--report` writing to a file and returning **0 / 1 / 2** read bare; the run's last line printing `N findings` or `N findings, M skipped`; **and the conditions that must exit 2 named in the source** — the reference set unrestored, the scaffold directory absent, `pagelint` unimportable
  - Notes: obligation 11. The contract is unreadable downstream of a `|`, so the tool must not require one. **2 is the code this tool is most likely to need and least likely to emit:** with `bin/` empty every one of the 985 blocks fails, which is a believable red in the direction that confirms the thesis — friction 56, as a runtime state rather than a documentation defect. A gate with no references has *not checked* the corpus.

- [ ] **Task 1.6a:** Red-proof exit 2 — the unchecked state, with its control *(review finding 4)*
  - Input: task 1.6, `tools/README.md` § *Exit codes — one contract, all of them*
  - Output: two recorded runs, both codes read **bare** — the reference set deliberately unrestored → **exit 2** with a message naming what was missing and **no per-block verdicts printed**; the same command with references present → **0 or 1**
  - Notes: obligation 3, applied to the code no other task produces. Without this, exit 2 is a promise in a task description, and the failure it guards is the one that looks most like a successful measurement.

- [ ] **Task 1.7:** Red-proof the extraction — byte-identity, two-way
  - Input: task 1.1's output
  - Output: `--verify-extraction` reporting **N of N identical** where N is task 1.2's corpus count; **and** a recorded run where one staged block has a byte appended and the check reports exactly that block
  - Notes: the green half is the half that feels unnecessary and the half that catches a check which has stopped running.

- [ ] **Task 1.8:** Red-proof the compile verdict — planted pair, outside the corpus
  - Input: `design.md` § *Its controls*
  - Output: recorded output showing a planted `NoSuchTypeXyz123` block → `FAIL` with `CS0246`, and a planted trivial class → `CLEAN`; **neither planted file is a block from `contents/`**. **A third plant, and it is the one that guards obligation 8:** a deliberately unparseable block in the **same run** as the `CS0246` plant, with the run still reporting `CS0246` on the second — recorded, and re-run whenever the compilation structure is touched
  - Notes: obligation 3, and *(review finding 5)* for the third plant. The first two cannot detect the defect this design exists to prevent: friction 54's mechanism is **a parse failure suppressing binding across a shared compilation**, so a regression to batching would leave a `CS0246` plant and a clean plant both reported and the instrument looking sensitive. **Obligation 8 is load-bearing and, without this plant, is the one rule here that no check has ever been able to fail.**

- [ ] **Task 1.9:** Red-proof the no-vacuous-pass rule
  - Input: `design.md`'s cost table; session 75's finding that a 0.54s build reporting 0 warnings is MSBuild declining to work
  - Output: two consecutive runs with nothing touched, recorded, reporting the **same** block count and the same verdict distribution; plus the wall-clock of each. **And the second direction:** one further run over an input changed on purpose — a single staged block broken — which must report a **different** distribution, the change then reverted and `git diff` shown empty
  - Notes: AC5, and *(review finding 6)* for the second direction. **Two identical runs are also exactly what a tool that cached, or that silently did nothing the second time, would print** — which is the same shape as session 75's 0.54s build reporting 0 warnings. Sameness is only evidence once difference has been shown to be reachable. **Do not quote a single wall-clock as the figure** — four probe runs over identical inputs gave 6.5s, 22.9s, 39.5s and 26.2s.

- [ ] **Task 1.10:** Predict and reconcile the eight gates
  - Input: `tools/README.md` § *The eight gates*, `design.md` § *Predicted gate movement*
  - Output: a § *Phase 1 prediction* and § *Phase 1 as executed* in this file, each gate with its predicted movement, its mechanism, and what it actually did
  - Notes: the prediction is **none** for all eight and the mechanism matters — `tools/` **is** inside `linkcheck`'s walk, so adding any `.md` there moves it. This phase deliberately adds none.

---

## Phase 2 — The corpus run *(6 tasks, one PR, no page touched)*

**Goal:** P0-6. The distribution nobody has, and the evidence P0-9's boundary and phase 3's baseline
both depend on.

- [ ] **Task 2.1:** Run the instrument over all 985 blocks and publish the distribution
  - Input: phase 1's tool, run from a clean checkout per obligation 12
  - Output: § *The corpus run* in this file — `BUILT / FAILED / SKIPPED / NOT COMPILABLE` counts summing to the corpus count, the command beside them, and `tools/blockcheck/verdicts.tsv` committed
  - Notes: the design measured 60 clean / 925 failing with **no scaffold**. With phase 1's scaffold the clean count should rise; **if it does not, that is a finding about the scaffold, not a number to adopt.**

- [ ] **Task 2.2:** Triage the blocks that fail to parse
  - Input: task 2.1's output, filtered to `CS1002`/`CS1513`/`CS1519`/`CS8635` and friends
  - Output: a table splitting them into *before/after pair in one fence*, *genuine fragment the wrapper mis-shaped*, and *the page is wrong* — with a count per class and a named example each
  - Notes: the design counted **131** unparsed and explicitly did not split them. A wrapper defect found here is a phase-1 bug to fix in this PR, not a page defect.

- [ ] **Task 2.3:** Split the failures into defects of claim and defects of context
  - Input: task 2.1's verdicts, `requirements.md` P0-9's scope line
  - Output: two lists in this file — the **claim** list, which is P0-9's input, and the **context** count, which is backlog item 2's; plus the command that produces each
  - Notes: this is the boundary the maintainer's ruling did not set and the assistant proposed. **If the claim list is large, say so plainly** — it is the number that decides whether phase 4 is a phase or a spec.

- [ ] **Task 2.4:** Rule Q3 by measurement — does Darker join?
  - Input: the 6 `Paramore.Darker*` strings, `requirements.md` Q3
  - Output: a recorded restore of the reference project **with** Darker's packages added, its exit code read bare, and a one-line ruling in this file — **and, if the ruling is *yes*, the sentence saying which phase carries the work**: the pin is phase 1's file, so a *yes* edits it from phase 2, and Darker's blocks then enter phase 3's baseline like any others
  - Notes: two of the six strings are namespaces, not packages. Resolve which is which before adding anything. P2-2 is the requirements' out-of-scope entry for Darker; a *yes* here promotes it, and a promotion with no task is how scope arrives unowned.

- [ ] **Task 2.5:** Confirm the before/after-in-one-fence set, and look for more of the shape
  - Input: `design.md` § *What the repair phase will actually repair*
  - Output: the confirmed list with page and ordinal per block, **re-derived by two methods** — the `CS0101` verdicts and a comment-marker grep covering **both vocabularies**, `// Before` / `// After` *and* `// V9` / `// V10` — plus, explicitly, **the blocks the second method finds that the first does not AND the candidates that reading strikes out**
  - Notes: the design says six blocks across four pages, of which exactly one raises `CS0101`. The other five collide with nothing and are invisible to the compiler — **so the grep is not a corroboration of the compiler here, it is the only instrument that sees five of the six.** *(Review finding 2:)* **the marker method at the review's HEAD returns nine candidates across five pages, and reading subtracts four of them** — see § *What the tasks review found*. This list is therefore expected to *shrink* as well as grow, and a candidate struck out is recorded with the reason, not deleted.

- [ ] **Task 2.6:** Predict and reconcile the eight gates
  - Input: as task 1.10
  - Output: § *Phase 2 prediction* and § *Phase 2 as executed*
  - Notes: prediction is **none**; this phase commits a `.tsv` and edits this file.

---

## Phase 3 — The baseline and the gate *(9 tasks, one PR, no page touched)*

**Goal:** P0-4, P0-5 and P0-8. The gate and the corpus that satisfies it merge **together**, per
`tools/README.md`'s rule 3.

- [ ] **Task 3.1:** Rule Q5 and implement the opt-out
  - Input: `tools/pagelint.py`'s `allow-serviceactivator` comment, `tools/symbolcheck.py`'s `allow <name>` comment
  - Output: the marker implemented in `tools/blockcheck.py`, **requiring a reason**, and a recorded run showing a skip printed with its reason
  - Notes: ruling 4 — an opt-out is never silent, and `0 findings` and `0 findings, 8 skipped` are different claims.

- [ ] **Task 3.2:** Mark the ❌ V9 blocks as skipped
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
