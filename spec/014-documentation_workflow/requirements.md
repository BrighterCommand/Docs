# Spec 014: The Documentation Workflow Itself — Requirements

**Created:** 2026-09-12
**Status:** **APPROVED 2026-09-12** — `.requirements-approved`. Reviewed against the
`/spec:review` checklist; **five findings, one blocking**, all applied. The blocking one added
**P0-7 / D12, the census probe that gates D8**, and struck AC9's promise that the corpus would be
green after three repairs — a promise resting on a number nobody had measured. **All five
questions are answered.** **Q1: build the
symbol-census gate in 014** (→ P0-5, and Q2 resolves by its own conditional). **Q3: move only
what a command cites.** **Q4: cold-start is out of scope** — struck from §4 and kept as the later
measure. **Q5: `/spec:update-task` is in scope as P2-1.** All five ruled 2026-09-12; §13 carries
each with its reasoning. Ready for `/spec:review`.

> Every figure in this document was derived on **2026-09-12** against Docs `master` at
> **`c0d410a`**, with the command beside it. A total with no ref is not a fact — re-derive
> before quoting.

> **This is the one spec whose subject is the process running it.** Friction met while running
> the workflow on itself is evidence, not an annoyance, and §14 records it as it happens. That
> instruction is 014's README's own, and honouring it is the first thing this document does.

---

## 1. Topic overview

The nine `/spec:*` commands in `.claude/commands/spec/` are the workflow every specification in
this programme has run through. This spec updates them to match what the programme learned.

It changes **process, not documentation**. No page under `contents/` moves, no `SUMMARY.md`
entry changes, and the corpus at close is byte-identical to the corpus at open except where a
held defect is repaired under §7's P0-4.

**The thesis, stated so it can be argued with:** this programme's method exists, is written
down, and is written down in three places the workflow does not read.

| Where the method lives | Size | Commands that name it |
|---|---|---|
| `CLAUDE.md` — conventions, page types, and the ledger mapping each convention to a `pagelint` rule | 1,006 lines | **2 of 9** (`implement.md` ×3, `new.md` ×1) |
| `PROMPT.md` — the operating memory: rulings, gate numbers, red-proof discipline, census method | 4,194 lines, **and `.gitignore:16`** | **0 of 9** |
| Five closed specs' `tasks.md` — 009's acceptance pass, 010's split rules, 011's conventions, 012's ten standing obligations, 013's defect ledger | 240 tasks | **0 of 9** |

```bash
grep -c "CLAUDE.md" .claude/commands/spec/*.md    # implement 3, new 1, seven zeros
grep -l "PROMPT"    .claude/commands/spec/*.md    # no output
git check-ignore -v PROMPT.md                     # .gitignore:16
```

**The second row is the sharpest and it is easy to read past.** The file carrying the rulings a
session must not relitigate is not in the repository at all. It survives because each session
reads it and rewrites it; nothing else transmits it. A command cannot cite it, a reviewer cannot
diff it, and a new contributor never sees it.

**The third row is where the programme's own words indict it.** Each closed spec ends by writing
its method into a committed document, and each next spec inherits that method only through a
person who happened to read the previous `tasks.md`. That is precisely the transmission failure
this programme has recorded about every other kind of knowledge, pointed at itself.

---

## 2. Current state — re-derived, not inherited

014's README was written 2026-09-04 and carries a measurement table. **Defect 9 is that no
command warns an inherited list rots**, so the first obligation of this phase is to re-run that
table rather than quote it. Re-run 2026-09-12, across the same 451 lines:

| Term | Files mentioning it, 2026-09-04 | Re-measured 2026-09-12 |
|---|---|---|
| `banner`, `Applies to`, the page-type vocabulary, Diátaxis | none | **unchanged — none** |
| `pagelint`, `urlmap`, `versioncheck`, `optioncheck` | none | **unchanged — none** |
| `linkcheck` | `review.md` only | **unchanged — `review.md` only** |
| front matter / `description:` for a page | none | **unchanged — none** |
| compiling, `using` directives, rule 6 | none | **unchanged — none** |
| acceptance pass, red-proof, re-deriving a total | none | **unchanged — none** |
| `acceptance criteri`, `open question` | 0 files each | **unchanged — 0 each** |

**One near-miss, discounted the same way the README discounted `description:`.** `grep -ril
Reference` returns six files, and **all eight matches are the verb** — *"Reference the
requirements for traceability"*, *"Reference source code in ../Brighter"*. None is the page
type. A grep that matches the wrong thing is this programme's most-repeated defect, so the
match was read before it was counted.

### 2.1 Nothing has changed because nothing has been changed

```bash
git log --format='%h %ad %s' --date=short -- .claude/commands/spec/
```

| Commit | Date | What it did |
|---|---|---|
| `9d20c23` | **2026-08-03** | added the `linkcheck.py` block to `review.md` |
| `c753e80` | 2026-06-17 | guarded the `.current-spec` lookup |
| `57945ba` | 2026-02-23 | adapted the commands for documentation |

**2026-08-03 is the day specs 009, 010, 011 and 012 were all created** — so the last change to
any command landed before the first of them was executed. Since then: **five specs closed or
run, 240 tasks, zero command edits.**

```bash
for d in 009 010 011 012 013; do grep -c '^- \[.\] \*\*Task' spec/$d*/tasks.md; done
# 40  56  43  58  43   =  240
```

### 2.2 The `allowed-tools` frontmatter forbids the checks the commands would need

This was not in the README and is measured here. Each command declares `allowed-tools`, and
**eight of nine permit no `python3` at all**:

```bash
grep -c "python3" .claude/commands/spec/*.md
# review.md: 4 — the rest: 0
```

`review.md`'s permission is narrow by design: `Bash(python3 tools/linkcheck.py:*)`, one tool of
seven. So **adding a gate to a command is two edits, not one** — the prose that says to run it
and the frontmatter that permits it. A design that changes only the prose produces a command
which asks for something it cannot do, which is worse than silence: it reads as covered.

### 2.3 One of seven gates is wired in, and the one that is, is wired in well

`/spec:review` runs `linkcheck.py` and then **distinguishes pre-existing breakage from breakage
the spec caused**, blocking only on the second and only after writing has begun. That is the
right shape and it is the model for the other six. This spec copies it rather than inventing
something.

---

## 3. The defect ledger — **seventeen inherited where fifteen were recorded, plus one found here = eighteen**

**The count was wrong before this phase started, and finding that is defect 9 landing on 014 at
its first opportunity.** `PROMPT.md` records *"fifteen (5 at filing, +4 from 013's requirements,
+4 from 013's design §13, +2 from 013's phase 5)"*. That arithmetic omits
`spec/013-howto_guides/tasks.md` §5, which records two more and says in its own words that they
are **"new, beyond requirements §14's four and design §13's four."**

| # | Defect | Source | Repaired by |
|---:|---|---|---|
| 1 | `/spec:implement` prescribes one page skeleton for every page, contradicting `CLAUDE.md`'s `## Step N:` convention | README, at filing | D5 |
| 2 | `/spec:implement`'s *Structure* line omits the banner — rule 1, an error on every page | README | D5 |
| 3 | Its *Quality Check* is a by-eye checklist where seven tools exist, and it does not separate the mechanical from the editorial | README | D5, D6 |
| 4 | `/spec:tasks`' phase template predates the phase-is-a-PR contract, standing obligations, red-proofs, and *record the mismatch before fixing it* | README | D4 |
| 5 | No command mentions an acceptance pass | README | D2, D4, D6 |
| 6 | Neither `acceptance criteria` nor `open questions` appears in any command — and **both criteria ever found unmet at a close were criteria the workflow never asked for** (009's AC7, 012's AC1) | 013 requirements §14 | D2, D1 |
| 7 | `/spec:requirements`' Research Steps are feature-shaped — ADRs, release notes, source, samples — and name neither discussions, issues nor `FAQ.md` | 013 requirements §14 | D2 |
| 8 | **Nothing asks whether the API a page names exists.** 013 found ten dead call sites across five pages, all green under every gate | 013 requirements §14 | D3, D5, D6, **D8** |
| 9 | No command warns that an inherited list rots. 013's README named six gaps and five were already closed | 013 requirements §14 | D1, D2, D3, D4 |
| 10 | `/spec:design` has no step for verifying the API a design will print — *"note source file/sample"* asks where an example came from, never whether it is correct | 013 design §13 | D3 |
| 11 | Nothing asks a design to re-verify its own requirements; *"Reference the requirements for traceability"* reads as *quote them* | 013 design §13 | D3 |
| 12 | `/spec:design` prescribes no page-type, banner, heading, opening-sentence or front-matter convention | 013 design §13 | D3 |
| 13 | No command asks a phase to predict which gates it will move — and three of 013's phases expected no movement, which is exactly when a vacuous pass is invisible | 013 design §13 | D3, D4 |
| 14 | `/spec:tasks` proposes *Research → Core → Supporting → Polish* without saying it is a default, inviting a task list that fights its own design | **013 tasks §5 — uncounted until now** | D4 |
| 15 | No command asks a task list to re-derive the counts it inherits | **013 tasks §5 — uncounted until now** | D4 |
| 16 | AC7's wording says a guide *"ends with"* a verification step where it means *"contains"* one | 013 phase 5 | D2 |
| 17 | AC8 has no tool — the check that exists is `pagetypes.tsv` row count against published page count, and **nothing reads that file** | 013 phase 5 | D2, D7 |

### 3.1 A candidate eighteenth, found in this phase

**`/spec:update-task` ticks a box and asks for nothing.** It matches a task by number or
keyword, flips `- [ ]` to `- [x]`, and prints a percentage. 013's defect ledger entry 13 is the
consequence: **task 4.8's two `pagetypes.tsv` rows were never written and the task was ticked
anyway**, caught two PRs later. `/spec:implement` has a quality check before marking complete;
the command whose entire purpose is marking complete has none.

Recorded here rather than folded into the seventeen, because it was found by this phase and the
programme's rule is to record a defect where it was found. **Q5 ruled it in scope as P2-1**, so
the ledger is **eighteen** and §3's table is the seventeen it inherited plus this one.

---

## 4. Target state

When 014 closes:

1. **Every convention in `CLAUDE.md` that governs a page is cited — not restated — by the
   command that would cause a writer to break it.** Restating is how the two drifted apart;
   `/spec:implement`'s skeleton is the proof.
2. **All eight gates are named by the commands, and `/spec:review` runs all eight** — the seven
   that exist plus D8 — with the pre-existing-versus-yours discipline it already applies to
   `linkcheck.py`, and with `allowed-tools` that permits them.
3. **Every spec is asked what it will be judged by, and every criterion names its instrument or
   is marked as having none.** The marked ones are where every close-time failure has been.
4. **Every phase is asked to re-derive what it inherited** — its README's claims, its
   requirements' counts, its design's totals. Three consecutive phases of 013 found a stale
   inherited figure.
5. **Nothing ships an API that does not exist**, under the maintainer's three-state rule: live,
   forthcoming-and-said-so, or a defect.
6. **The gate numbers and the phase-is-a-PR contract have a committed home**, so a command can
   cite them. Q3 bounds this: only what a command cites moves, and the rest of `PROMPT.md` stays
   session state.

> **Struck by Q4, and kept visible rather than deleted.** The draft's point 6 was *"a person or
> an LLM opening the repository cold can run the programme from `CLAUDE.md` plus the commands,
> without the gitignored `PROMPT.md`."* **That is not a criterion of 014** — it has no instrument
> short of running a cold session, and a target-state item with no instrument is the exact defect
> this spec exists to stop. It is recorded as the measure 014 is judged against **later**.

---

## 5. Target audience

**Not a Brighter user.** The reader of a command is whoever runs the next specification — in
practice a session of this assistant, and in principle a human contributor.

- **The assistant, cold.** It has `CLAUDE.md`, the commands, and the repository. It does not
  have the conversation that produced the last ruling. Everything a command assumes but does not
  say is lost.
- **A human contributor.** Reads the commands to learn how documentation gets made here, and has
  no way to know that `/spec:implement`'s skeleton is wrong for a tutorial.

**This audience makes one rule harder than usual**: a command must be complete without being
long, because an over-long command is skimmed and a skimmed command is the same as an absent
one. 451 lines is the current budget for nine commands, and §11 constrains the growth.

---

## 6. Source material

**The `/spec:requirements` Research Steps are wrong for this spec and that is defect 7 measured
live** — they name ADRs, release notes, source code and samples, and 014 needs none of the four.
Recorded in §14. The real sources:

| Source | What it supplies |
|---|---|
| `.claude/commands/spec/*.md` — 451 lines, nine files | the subject itself |
| `CLAUDE.md` — 1,006 lines | the conventions the commands must cite; the convention-to-rule ledger; the `## Step N:` ruling of 2026-09-06 |
| `spec/009-getting_started_tutorials/tasks.md` | the compile-and-run pass, and AC7 found unmet at close |
| `spec/010-information_architecture/tasks.md` | the split rules, and the two kinds of `SUMMARY.md` nesting |
| `spec/011-authoring_conventions/tasks.md`, `pagetypes.tsv`, `classification-notes.md` | the page-type vocabulary and its 161-row census |
| `spec/012-configuration_reference/tasks.md` §1 and *Phase 11 as executed* | **the ten standing obligations**, the phase-is-a-PR contract, red-proofs, the 37-entry drift ledger, and AC1 found unmet at close |
| `spec/013-howto_guides/` — requirements §14, design §13, tasks §5, *Phase 5 as executed* | ten of the seventeen defects, the fifteen-entry defect ledger, **and the symbol-census gate's design spec with its five false-positive classes** |
| `PROMPT.md` — 4,194 lines, gitignored | the rulings, the gate numbers, the census method. **Cited as a source, and its uncommitted status is itself a finding (§1)** |
| `tools/` — seven gates plus `survey.py`, `apply_banners.py` | what a command can actually be told to run |

**No ADR, no release note, no sample, and no Brighter source.** 014 writes no page and names no
API, so the repositories at `../Brighter` and `../Darker` are read only if D8 is built, and then
only as the two refs a census resolves against.

---

## 7. Scope

### P0 — must have

- **P0-1 — `/spec:implement` stops contradicting `CLAUDE.md`.** Mode-aware structure, the banner,
  the compile obligation, API liveness, and a quality check that separates what a tool decides
  from what a person decides. *(defects 1, 2, 3, 8)*
- **P0-2 — `/spec:requirements` asks what the spec will be judged by.** Acceptance criteria and
  open questions by name; **each criterion names its instrument or is marked as having none**;
  the mode-mix step; the demand step for reader-problem specs; re-derive the README.
  *(defects 5, 6, 7, 9, 16, 17)*
- **P0-3 — `/spec:review` runs all eight gates** — the seven that exist plus D8 — with the
  pre-existing-versus-yours discipline, the `allowed-tools` to permit them, and a checklist for
  the acceptance phase. *(defects 3, 5, 8)*
- **P0-4 — the three held dead symbols are repaired.** `IAmACommandStoreAsync`
  (`BuildingAnAsyncPipeline.md:36,38`, in a C# block — a paste gets `CS0246`), `UseExternalInbox`
  (`DispatcherConfigurationReference.md:255`, on a **Reference** page), `IAmAnIbox`
  (`HowBrighterWorks.md:94`, a typo for `IAmAnInbox`). Held by 013 on instruction as the census
  gate's first red. **They are the only `contents/` change 014 makes**, and §10 says why the
  exception is stated rather than smuggled.

- **P0-5 — the symbol-census gate (D8).** Ruled in by Q1 and **gated by P0-7**. It carries three
  constraints and a false-positive budget, so it has its own subsection below rather than a line
  here.
- **P0-6 — the acceptance pass is executed and written up**, with the friction ledger and what 014
  got wrong. Defect 5 is that no command mentions an acceptance pass; a spec that repairs it and
  then skips its own would be the joke this programme keeps telling about itself.
- **P0-7 — the census probe, and it gates D8.** *(added by the requirements review,
  ruled 2026-09-12)*

  **Nothing is written from a floor.** A throwaway census runs over all 161 pages **before** D8
  is designed, and reports two numbers: **how many candidates need triage**, and **how many
  pages are genuinely red**. Only then is it decided whether the reds are repaired inside 014 or
  recorded against a later spec.

  The measurement that forced this: **2,378 distinct PascalCase identifiers appear in C# fences
  alone across the 161 pages**, before inline code spans, which 013's walk also covered —
  against the **77 candidates / 1 real** that an 18-page naive census produced. **The three held
  symbols are not known to be the only reds**; they came from a sweep targeted at specific names,
  not an exhaustive one.

  **The probe is throwaway on purpose.** It is allowed to be crude, to over-report, and to be
  deleted — its output is a number, not a gate. Building the real checker first and discovering
  the corpus is red on thirty pages is the failure this deliverable exists to prevent, because
  at that point the sunk cost argues for weakening the checker.

  **This is 012 phase 1's pattern, not a new invention** — *"three probes and the survey fix.
  Nothing is written from a floor"*, and **1 gates everything**.

### P1 — should have

- **P1-1 — `/spec:design` learns the conventions and the two verifications.** Page type per file,
  banner, qualified headings, opening sentence, front matter, the two kinds of `SUMMARY.md`
  nesting; verify the API a design will print; re-verify the requirements rather than quote them;
  predict gate movement. *(defects 10, 11, 12, 13)*
- **P1-2 — `/spec:tasks` learns the contract.** Phase-is-a-PR; the standing-obligations section;
  red-proofs; *record the mismatch before fixing it*; deliverable-shaped phases with the four-phase
  template demoted to an explicit default; re-derive inherited counts; an acceptance phase as the
  last phase. *(defects 4, 5, 9, 14, 15)*
- **P1-4 — the gate numbers and the phase-is-a-PR contract get a committed home** (D10), per Q3.
  Only what a command cites moves.
- **P1-3 — `/spec:new`'s README template carries the sections the workflow will ask for** —
  acceptance criteria, open questions, and the instruction to re-derive the README before
  executing it. A spec that starts by executing a stale README ships work already done. *(defect 9)*

### P2 — nice to have

- **P2-1 — `/spec:update-task` requires evidence to tick.** The eighteenth defect (§3.1), **ruled
  in scope 2026-09-12**: the task's stated **Output** must exist before the box flips.
- **P2-2 — a `commandlint`**: every tool a command's prose tells you to run is permitted by its
  own `allowed-tools`. Mechanical, small, and it red-proves §2.2 rather than trusting the fix.
- **P2-3 — `/spec:status` reports gate state**, not just task counts.

### P0-5 — the eighth gate. **ANSWERED 2026-09-12: build it in 014**

> **AMENDED BY DESIGN REVIEW, 2026-09-12 — read `design.md` §4, not this section, on what D8
> is.** The probe this section's ruling produced (P0-7) measured **831 unresolved symbols across
> 127 of 144 fenced pages**, dominated by the docs' own example domain. **An open-world census
> cannot go green, so it cannot be a gate.** D8 is instead a **watchlist checker** — a curated
> list of known-dead names checked in code *and* prose, no false positives by construction — with
> the census surviving as `--census`, a report that feeds the list. The three constraints below
> still bind it; what changed is the model. **This paragraph is the requirement being corrected
> by its own design phase, which is defect 11's repair working.**

**D8, the symbol-census gate**, checks that every type and method a page names exists at one of
two refs. It is the instrument behind defect 8, the defect with the highest blast radius, and
013 phase 5 already wrote its design spec: five mechanically-excludable false-positive classes,
the two-ref resolution that separates *forthcoming* from *dead*, the product disambiguation
(`.AddPolicies(` is dead in Brighter and **alive in Darker 4.1.1**), and the trap that **`git
grep` does not honour `\b`** — a pattern using it returns a clean-looking zero.

**The ruling was the recommendation**: 014's thesis is that prose obligations do not transmit
and tools do, so shipping defect 8's repair as three paragraphs in `/spec:implement` would
contradict the spec's premise inside the spec that argues it.

**What the ruling costs, recorded now so the design phase does not rediscover it.** This is the
hardest tool in the repository, and the three constraints below are requirements on D8, not
observations about it:

1. **Two refs, not one.** *Forthcoming* (absent at the pin, present on `origin/master`) is a
   legitimate state and must not be reported as dead. A checker that resolves one ref cannot
   tell state 2 from state 3 and is therefore wrong by construction.
2. **It must know which product a page is about.** `.AddPolicies(` is dead in Brighter and alive
   in Darker 4.1.1, and the two version independently. The banner's *Applies to* is the only
   machine-readable statement of a page's product, so **D8 reads the banner** — which makes
   `pagelint.py` rule 2 load-bearing for a second tool.
3. **The red-proof is two-way and is not optional.** The manual sweep that found the ten dead
   sites produced **three wrong regexes, two of which returned a plausible zero**, caught only
   by controls. So every control pairs a **known-present** symbol with a **known-absent** one; a
   control that only proves absence proves nothing about the grep. AC9 states this as the
   acceptance test.

**And the false-positive budget is the design spec, not a hope.** A naive census over 013's
eighteen pages raised **77 candidates of which 1 was real**. The five excludable classes —
methods invisible to a type index, C#'s elided `Attribute` suffix, reader-declared symbols, BCL
and third-party names, and pages that discuss a dead name on purpose — are in
`spec/013-howto_guides/tasks.md` § *What the AC10 walk taught about building the gate*, with the
`<!-- pagelint: allow-serviceactivator -->` precedent for the opt-out.

---

## 8. Out of scope

- **Rewriting `CLAUDE.md`'s conventions.** They are enforced and they hold. 014 makes the
  workflow aware of them and may *add* to `CLAUDE.md` where a convention belongs there rather
  than in a command — §11 constrains which.
- **Any page under `contents/` other than P0-4's three symbols.** §10's red-proof measures this.
- **Re-opening any closed spec**, or any of the rulings listed in `PROMPT.md` § *Rulings — do not
  relitigate any of these*.
- **The two live 011 questions** — the Darker exclusion claim on five middleware pages, and the
  two assistant-called page-type verdicts. Unrelated to the workflow.
- **Committing `PROMPT.md`.** §1 records that its uncommitted status is a finding. **Q3 bounds
  the response**: the lines a command cites get a committed home (D10), the file itself stays
  ignored, and the board, branch tips and CI census stay session state.
- **A cold-start audit.** Struck by Q4 — see §4. 014 is judged on its deliverables, not on
  whether a session with no prior context could run the programme unaided.
- **`/spec:switch` and `/spec:status` beyond P2-3.** They work.

---

## 9. Deliverables

| ID | File | Change | Priority |
|---|---|---|---|
| **D1** | `.claude/commands/spec/new.md` | README template gains acceptance criteria, open questions, and the re-derive instruction | P1-3 |
| **D2** | `.claude/commands/spec/requirements.md` | acceptance criteria and open questions by name; instrument-per-criterion; mode-mix step; demand step; re-derive step; source material widened past the feature shape | **P0-2** |
| **D3** | `.claude/commands/spec/design.md` | page type per file; the page conventions, cited; API verification; re-verify requirements; predict gate movement | P1-1 |
| **D4** | `.claude/commands/spec/tasks.md` | phase-is-a-PR; standing obligations; red-proofs; record-before-fixing; default phases marked as a default; re-derive counts; acceptance phase | P1-2 |
| **D5** | `.claude/commands/spec/implement.md` | mode-aware structure; banner; compile obligation; API liveness; tool-versus-judgement quality check | **P0-1** |
| **D6** | `.claude/commands/spec/review.md` | all eight gates, with `allowed-tools`; acceptance-phase checklist | **P0-3** |
| **D7** | `.claude/commands/spec/update-task.md` | evidence to tick | P2-1 |
| **D8** | `tools/symbolcheck.py` *(name provisional)* + its CI job in `.github/workflows/docs.yml` + its two-way red-proof | the eighth gate | **P0-5** |
| **D9** | `contents/BuildingAnAsyncPipeline.md`, `contents/DispatcherConfigurationReference.md`, `contents/HowBrighterWorks.md` | the three held dead symbols | **P0-4** |
| **D10** | `CLAUDE.md`, and/or a new `tools/README.md` | a convention that belongs there rather than in a command (§11), **plus Q3's move: the eight gates' commands and expected numbers, and the phase-is-a-PR contract, given a committed home a command can cite** | **P1-4** |
| **D11** | `spec/014-documentation_workflow/tasks.md` § *acceptance as executed* | the walk, the friction ledger, and what 014 got wrong | **P0-6** |
| **D12** | a throwaway census script and its findings, written into `tasks.md` § *the probe as executed* | **candidates to triage, and pages genuinely red, across all 161** — the number D8's design and P0-4's size both rest on. **Gates D8** | **P0-7** |

---

## 10. `SUMMARY.md` changes — **none, and here is the proof**

No page is created, moved, renamed or nested, so `SUMMARY.md` does not change and neither do
`--check-shape`, `--check-redirects` or `--verify`.

**A claim of "no change" needs an instrument or it is a hope.** The red-proof:

```bash
git diff --stat origin/master -- SUMMARY.md              # must be empty
git diff --stat origin/master -- contents/ | tail -1     # 3 files, D9 only
```

The second is the one that earns its keep: it permits exactly D9's three pages and fails on a
fourth. **This is the criterion 014 is most likely to break quietly** — a spec about the
workflow is the easiest place to talk yourself into "while I'm here" edits to a page.

---

## 11. Constraints

1. **Cite `CLAUDE.md`, do not restate it.** Restating is how the two drifted apart, and it is
   012's premise pointed at ourselves. A command says *"follow `CLAUDE.md` § Page banner"* and
   names the rule number; it does not copy the banner grammar, which would then rot.
2. **The test for what belongs in `CLAUDE.md` versus a command**: `CLAUDE.md` says what a *page*
   must be; a command says what a *phase* must do. The banner grammar is `CLAUDE.md`'s. *"Predict
   which gates this phase will move"* is a command's. Where a rule is genuinely about pages and
   missing from `CLAUDE.md`, it goes there (D10) and the command cites it.
3. **Prose obligation and tool permission ship together.** §2.2 — an instruction the
   `allowed-tools` forbids reads as covered and is not.
4. **Length budget.** The nine commands are 451 lines. The design phase sets a per-file ceiling;
   the requirement here is that **growth is justified per command**, because a command long enough
   to skim is a command nobody follows. Where a rule is long, the command links it.
5. **Every new obligation names its instrument, or says it has none.** This is defect 6's repair
   and 014 is bound by it first — see §12.
6. **The seven existing gates stay green at their recorded numbers**, and any movement is predicted before
   the work: `linkcheck` 164 files / 0 broken, `pagelint` 0 errors / 768 warnings / 162 pages,
   `--check-shape` 161 / 12 / 12 of 20, `--check-redirects` 77 entries / 7858 bytes,
   `versioncheck` 0 of 18 across 5, `optioncheck` 0 across 59 tables / 519 rows, `--verify`
   161 = 161. **D9 moves none of them**, which is exactly why those symbols survived.
7. **One phase, one pull request**, merged before the next branch starts — 012 §1's contract,
   and 014 does not get to exempt itself from a contract it is writing down.
8. **A `--admin` merge is standing-authorised for this repository; deleting a head ref is not.**
   Ask for the deletion by name in the same breath as the merge.
9. **No write to `../Brighter` or `../Darker` is anticipated.** If D8 needs one, it is a fresh
   per-PR ask.

---

## 12. Acceptance criteria

**Each names its instrument or is marked as having none.** The marked ones are where every
close-time failure in this programme has been — 009's AC7 and 012's AC1 — so they are listed
first in the review walk, not last.

| # | Criterion | Instrument |
|---|---|---|
| **AC1** | Every P0 deliverable ships, or is struck with a recorded reason | **walked — no tool** |
| **AC2** | No command contradicts `CLAUDE.md`. Walked convention by convention, not asserted | **walked — no tool** |
| **AC3** | Each of the **eighteen** defects — §3's seventeen plus §3.1's — is repaired or struck with a reason, and the count is re-derived rather than quoted | **walked — no tool**, count by `grep -c` on §3 |
| **AC4** | Every obligation a command adds names its instrument or is marked as having none | **walked — no tool** |
| **AC5** | All eight gates are named in `.claude/commands/spec/`, and `/spec:review` runs eight | `grep -l` per tool name; `grep -c python3` on `review.md`'s frontmatter |
| **AC6** | Every tool a command's prose names is permitted by its own `allowed-tools` | **walked — no tool**, unless P2-2 is built, in which case `commandlint` |
| **AC7** | `SUMMARY.md` unchanged; `contents/` changed on **exactly D9's ELEVEN pages** — *amended by design review 2026-09-12, Q2: `IMessageScheduler` adds 8* | `git diff --name-only origin/master -- contents/ \| wc -l` = 11 |
| **AC8** | The seven existing gates green at §11.6's numbers. **D8's own green is deliberately not promised here** — what it must be is *decided*, by D12's numbers, and recorded | the seven commands; D8's disposition **walked — no tool** |
| **AC9** | *Amended by design review 2026-09-12 (Q1) and tasks review 2026-09-12.* D8 goes **red on all four watchlist symbols across 11 pages** before D9 repairs them and **green after**; deleting an entry proves the list rather than the corpus is firing; `--verify-list` exits non-zero on a corrupt ref and reports `CommandProcessor` as **live**; **the `product` column and the opt-out each have a two-way control**; and every control is two-way | `symbolcheck` + the **seven** red-proofs, design §4.5 and tasks 2.7–2.10 |
| **AC12** | The probe (D12) ran over all 161 pages and its two numbers are recorded; **D8's scope and P0-4's size both cite them**; and any red the probe found that 014 does not repair is named, with where it goes | **walked — no tool**, against `tasks.md` § *the probe as executed* |
| **AC10** | 014's own friction is recorded in `tasks.md`, including anything this spec's commands made harder | **walked — no tool** |
| **AC11** | Every line Q3 moved out of `PROMPT.md` is cited by at least one command, and no line moved that nothing cites | `grep` each moved line's new home against `.claude/commands/spec/`; the reverse direction is **walked — no tool** |

**Seven and a half of twelve have no tool, and that is the honest number** — AC1, AC2, AC3, AC4,
AC10, AC12, AC6 unless P2-2 is built, and half each of AC8 and AC11. **That is worse than it
looks and is being said out loud**: a spec whose thesis is *tools transmit and prose does not*
carries more hand-walked criteria than instrumented ones. Whether a command contradicts
`CLAUDE.md` is a reading, not a regex; a checker that guessed at it would be the version-marker
rule all over again. Naming them is the repair — 009 and 012 each lost a criterion at close
because nobody knew which ones were being carried by hand.

---

## 13. Questions for the maintainer

**Q1 — Does 014 build the symbol-census gate (D8), or does it write the obligation and leave the
gate to spec 015? — ANSWERED 2026-09-12: BUILD IT.**
The recommendation was to build it in 014 and that is the ruling. D8 becomes **P0-5**, §7 carries
the three constraints the ruling implies. **AC9 lost its "green after D9" clause** to the
requirements review's finding 1, and **P0-7 / D12 — the census probe — now gates D8**. §7's
priorities are **seven P0s**. **How many phases that is belongs to the design phase** — an
earlier draft of this line asserted six, which was a task-list decision made in a requirements
document.

**Q2 — Is P1-1 (`/spec:design`) really P1? — RESOLVED BY Q1, not separately ruled.**
The stated conditional was *"it stays P1 only if D8 ships, because the gate catches downstream
what the design phase would have caught upstream."* D8 ships, so **P1-1 stays P1**. Recorded
rather than dropped, because a conditional that resolves silently is indistinguishable from one
nobody answered — and if D8 is later descoped, this reverts to P0 automatically.

**Q3 — How much of the method moves out of `PROMPT.md` and into committed documents? —
ANSWERED 2026-09-12: ONLY WHAT A COMMAND MUST CITE.**
The gate numbers go to a committed home — `CLAUDE.md` § Enforcement or a new `tools/README.md`,
the design phase picks one — and the phase-is-a-PR contract goes into `/spec:tasks`. **Everything
else in `PROMPT.md` stays uncommitted and stays session state**: the board, the branch tips, the
CI flake census, the open-question log. **The test for moving a line is whether a command cites
it**, and a line no command cites does not move however durable it looks — otherwise this becomes
Q3's second option by accretion. Deliverable **D10** absorbs this; §1's finding stands as a
finding either way.

**Q4 — Is target-state point 6 — "runnable cold, without `PROMPT.md`" — in scope? —
ANSWERED 2026-09-12: OUT OF SCOPE.**
014 updates nine commands and builds the gate. **§4's point 6 is kept as the measure 014 is
judged against later, not as a criterion of 014** — so it is struck from the target state and
recorded here, because a target-state item with no acceptance criterion is the thing this spec
exists to stop. There is no instrument for *"a cold session succeeds"* short of running one, and
naming that honestly is the point.

**Q5 — Is the candidate eighteenth defect (§3.1, `/spec:update-task` ticks without evidence) in
scope? — ANSWERED 2026-09-12: YES, AS P2-1.**
The fix is one sentence: the task's stated **Output** must exist before the box flips. It has a
real instance behind it — 013's ledger entry 13, which reached `master` and needed PR #157 to
correct — and P2 keeps it from competing with D8 for the spec's attention.

---

## 14. Workflow friction — 014's own evidence, recorded as met

Per the README: running the workflow on itself makes friction evidence. **Four met in this
phase.** They are numbered from 18, continuing §3's ledger, because they are the same kind of
thing.

18. **`/spec:requirements`' template has no shape for a process spec.** Nine of its ten
    required sections assume the deliverable is a documentation page; **SUMMARY.md changes** is
    not merely empty here, it is a category error, and §10 had to turn it into a red-proof to be
    worth writing. *Not a new defect class — it is defect 7 (feature-shaped research) widened: the
    command assumes the subject is a Brighter feature at every level, not just in its research
    steps.* The repair in D2 should say a spec may declare its subject as **feature**, **reader
    problem**, or **process**, and which sections that makes N/A.

19. **The command asks for a quality checklist it does not apply to itself.** Its *Requirements
    Quality Checklist* ends *"Reference specific source files and samples where relevant"* — and
    there is no step that checks the resulting document against that list. `/spec:review` has the
    checklist too, in a different wording, and the two have drifted: review asks *"Are SUMMARY.md
    changes specified?"*, requirements asks for *"SUMMARY.md changes"*, and neither permits "none,
    and here is the proof". **A checklist duplicated in two commands is the same restatement
    failure this spec identifies between commands and `CLAUDE.md`** — D2 and D6 should hold it
    once.

20. **Nothing told this phase to re-derive 014's own defect count, and the count was wrong.**
    §3 found seventeen where `PROMPT.md` says fifteen. Defect 9 predicted it, D1/D2 repair it,
    and it is recorded here as the measured instance: **the prediction landed on the first spec
    that ran after it was written, which is the strongest evidence the ledger has.**

21. **The research steps sent this phase to the wrong repository.** Run literally — ADRs, release
    notes, Brighter source, samples — the phase would have read four sources with nothing in them
    about its subject and never opened the nine files it is about. §6 is what the command should
    have asked for.

---

**Next step:** `/spec:design`. **Seven P0 deliverables**, one of which is a tool nobody has
written — and the first thing the design phase must respect is that **D12 gates D8**: the
corpus's red count is measured before the checker that reports it is designed.
