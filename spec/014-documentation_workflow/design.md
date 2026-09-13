# Spec 014: The Documentation Workflow Itself — Design

**Created:** 2026-09-12
**Status:** **APPROVED 2026-09-12** — `.design-approved`. **All three §11 questions answered
yes-as-recommended**: D8 is a watchlist checker (**amending approved requirement P0-5 and AC9**),
D9 grows to four symbols across 11 pages (**amending AC7's red-proof**), and the committed home
is a new `tools/README.md`.
**Design review, 2026-09-12:** five findings, **two blocking**, all applied — the CI job moved
from phase 2 to phase 3 (a gate wired while the corpus is red turns `master` red, since
`docs.yml` runs `on: push`); `--verify-list` moved to the scheduled `versions` job (nothing in
the `check` job checks out `../Brighter`); the watchlist gained its `product` column; §4.1 gained
the coverage matrix; and the phase table now says five PRs, not six.
**Works from:** `requirements.md`, approved 2026-09-12 (`.requirements-approved`), reviewed with
five findings applied.

> Every figure here was derived on **2026-09-12** against Docs `master` at **`c0d410a`**,
> Brighter at **`10.7.0`** and **`origin/master` = `1b03491bf`**, and Darker at **`4.1.1`** and
> `origin/master` = `2f76cda`. The probe script is in the session scratchpad at
> `probe/census.py`; it is throwaway by design and is not proposed for the repository.

> **This design does not merely quote its requirements.** Defect 11 is that
> *"Reference the requirements for traceability"* reads as *quote them*, and that a design which
> only quotes inherits its requirements' errors. §3 is what happens when you do not:
> **the probe the requirements review demanded has changed what D8 should be.**

---

## 1. What this design covers

| Deliverable | Requirement | Where designed |
|---|---|---|
| D12 — the census probe | P0-7 | **§3** (executed; this design is its first consumer) |
| D8 — the symbol checker | P0-5 | **§4** — *and it is not the tool the requirements assumed* |
| D9 — corpus repairs | P0-4 | §5 — **four symbols across 11 pages, not three across 3** |
| D5, D2, D6 — `implement`, `requirements`, `review` | P0-1/2/3 | §6 |
| D3, D4, D1, D10 — `design`, `tasks`, `new`, the committed home | P1-1/2/3/4 | §7 |
| D7, P2-2, P2-3 | P2 | §8 |
| D11 — the acceptance write-up | P0-6 | §9's phase 6 |

**No page is created, moved or renamed.** §10 carries the `SUMMARY.md` non-change and its
red-proof.

---

## 2. Reading order

The commands are read in the order a spec runs, and that is the order to write them:

```text
/spec:new  ──►  /spec:requirements  ──►  /spec:design  ──►  /spec:tasks  ──►  /spec:implement
   D1              D2                       D3               D4                D5
                    │                        │                │                 │
                    └────────────────────────┴────────────────┴─────────────────┘
                                             │
                                    /spec:review  (D6) — runs the gates at every phase
                                             │
                              ┌──────────────┴───────────────┐
                     tools/ (D8, the checker)        the committed home (D10)
                                                   gate numbers · phase-is-a-PR
```

**`CLAUDE.md` sits above all of them and is cited, never restated** — constraint 1. A command
names the rule; `CLAUDE.md` says what it is.

---

## 3. D12 — the probe as executed, and it is the reason §4 exists

**The requirement:** a throwaway census over all 161 pages, reporting how many candidates need
triage and how many are genuinely red, **before D8 is designed**. *Nothing is written from a
floor.*

### 3.1 Method, and the controls that came first

A token set is built per `(repo, ref)` by extracting every `[A-Z][A-Za-z0-9_]{3,}` word from
`.cs` files at that ref; a candidate is unresolved if it is in **no** set. A presence test is
deliberately permissive — a name in a comment counts as present — so **every count below is a
lower bound on real reds.**

**Two-way controls, run before any number was read**, per 013's rule that a control proving only
absence proves nothing: `CommandProcessor` must be in every Brighter set (**43 files at 10.7.0,
45 at master**) and `IAmAnIbox` in none (**0 and 0**). The script exits on a control failure and
on any empty token set.

**That guard earned itself twice in one session.** A first attempt built the sets through a shell
function whose quoting broke the pathspec: **all six sets came back empty**, which without the
check would have read as *"nothing is unresolved — the corpus is clean."* This is the
plausible-zero failure 013 recorded about `git grep` and `\b`, met again in a different disguise.

### 3.2 What it found

| Measure | Count |
|---|---|
| Pages with at least one C# fence | **144 of 161** |
| Distinct PascalCase tokens in those fences | 2,762 |
| …after stripping comments and string literals | 2,283 |
| …after scoping out page-declared types and members, and a third-party prefix filter | 1,211 raw candidates |
| **Unresolved at `src/` of both products, both refs** | **831** |
| **Pages carrying at least one** | **127 of 144 fenced pages** |
| Prose-only surface, for comparison | 1,217 unresolved across **160 of 161 pages** |

**Three conclusions, each of which changes a design decision:**

1. **An open-world census cannot be a gate.** 831 unresolved across 127 pages is not a backlog,
   it is the normal state of a corpus that uses example domains. The top of the list is
   `CustomerId` (17 pages), `CustomerName` (12), `ProcessOrderCommand` (10), `IOrderRepository`
   (8) — the docs' own invented domain, which **is not mechanically separable from a real API
   name**. Cheap filters moved 904 → 831; there is no cheap filter that reaches zero.
2. **The prose surface is unusable for a census and fine for a watchlist.** 160 of 161 pages
   carry an unresolved prose token, because every capitalised English word qualifies. But a
   *curated* name searched in prose has no false-positive problem at all — which is the hinge
   §4 turns on.
3. **Two of the three held symbols are not in code contexts.** `UseExternalInbox` and
   `IAmAnIbox` are prose; only `IAmACommandStoreAsync` is in a fence. **A code-context census
   structurally cannot catch two of the three symbols AC9 promises it will.**

### 3.3 Two instrument findings that bind any tool written here

- **The fence regex must be `pagelint.py`'s.** **146 fences across 39 pages** are written
  ```` ``` csharp ```` with a space. `pagelint`'s `FENCE_RE` has `[ \t]*` and handles them; the
  probe's first regex did not and silently skipped all 146 — which is how
  `IAmACommandStoreAsync`, a *known* red, came back clean on the first run. **D8 imports
  `FENCE_RE` rather than writing one.**
- **Resolve against `src/` only.** `SampleMauiTestApp/Resources/Fonts/FluentUI.cs` in Darker is a
  generated glyph table contributing **1,579** distinct PascalCase names by itself. Including
  tests and samples would let a font glyph vouch for a dead API.

### 3.4 The probe found a real defect nobody knew about

**`IMessageScheduler` does not exist, at either ref, and is used in C# code blocks on 8 pages.**

```text
IMessageScheduler      10.7.0: 0 files    master: 0 files      <- the docs' spelling
IAmAMessageScheduler   10.7.0: 51         master: 51           <- the real one
CommandProcessor       10.7.0: 43         master: 45           <- control, present
IAmAnIbox              10.7.0: 0          master: 0            <- control, absent
```

`AwsScheduler.md`, `AzureScheduler.md`, `HangfireScheduler.md`, `InMemoryOptions.md`,
`InMemoryScheduler.md`, `QuartzScheduler.md`, `SchedulingAMessage.md`, `TickerQScheduler.md` —
**the entire scheduler family**, green under all seven gates, and a paste gets `CS0246`. It is
the same shape as 013's ledger entries 7, 8 and 14: a name that reads plausibly, is wrong
everywhere it appears, and has never had an instrument behind it.

---

## 4. D8 — inverted. **A watchlist checker, not a census gate**

### 4.1 The change, and why the probe forces it

| | The requirements assumed | What §3 measured |
|---|---|---|
| Model | open world — every symbol must resolve | **closed world — named dead symbols must not appear** |
| Reds today | the three held ones | **831**, of which the overwhelming majority are legitimate |
| Prose | excluded as too noisy | **included, safely** — a curated name has no noise |
| Held symbols caught | 3 of 3 | **1 of 3** under a census; **3 of 3** under a watchlist |
| Red-proof | possible | possible **and cheap** — the list is the test |

**A watchlist is a list of names known to be dead, each with the live replacement**, checked
across `contents/` in both code contexts and prose. It goes red when a listed name appears. It
has **no false positives by construction**, which is what makes it gateable on day one — and it
catches all three held symbols *and* `IMessageScheduler`, which the census could not.

**What it gives up is discovery**, and that is real: a watchlist only finds what is already on
it. §4.4 is how names get on it.

**So defect 8 is narrowed, not eliminated, and the coverage has to be stated rather than
implied** — a spec about unstated obligations does not get to leave its own gap unnamed:

| How a dead API reaches a page | What catches it |
|---|---|
| Already in the corpus, and named on the list | **`symbolcheck`**, every push |
| Written into a new or edited C# block | **the compile obligation** (D5, §6.1) — 009's harness |
| Named in a design before anything is written | **D3's API-verification step** (§7.1) |
| Already in the corpus and **not** on the list | **`--census`, run deliberately** — 831 candidates, triaged by a person |
| **Written into prose on an existing page, uncompiled and unlisted** | **nothing** |

That last row is the honest residue. It is narrower than today's gap — which is *every* row —
and it is the one to quote if anyone later claims 014 closed defect 8.

### 4.2 `tools/symbolcheck.py`

```text
python3 tools/symbolcheck.py                  # gate: watchlist over contents/
python3 tools/symbolcheck.py contents/X.md    # specific pages
python3 tools/symbolcheck.py --census         # the open-world report, NOT a gate
python3 tools/symbolcheck.py --verify-list    # every entry still dead at both refs
```

Exit 1 on any error, 0 when clean, 2 on bad arguments — the contract `linkcheck.py`,
`pagelint.py` and `urlmap.py` already share.

**The watchlist is a TSV beside the tool**, one row per dead name:

```text
symbol                  product    replacement            evidence          first_seen
IAmACommandStoreAsync   brighter   IAmAnInboxAsync        013 ledger 15     2026-09-10
UseExternalInbox        brighter   (removed at V10)       013 ledger 15     2026-09-10
IAmAnIbox               brighter   IAmAnInbox             013 ledger 15     2026-09-10
IMessageScheduler       brighter   IAmAMessageScheduler   014 probe §3.4    2026-09-12
```

**The `product` column is not decoration** — it is `brighter`, `darker` or `both`, and without it
§4.3's argument has nowhere to live. `.AddPolicies(` is dead in Brighter and **alive in Darker
4.1.1**: one row, two verdicts, and only the column tells them apart. `--verify-list` resolves a
row against its own product's refs, and the gate reports a `brighter` row only on pages whose
banner does not say *Darker V4*.

**`--verify-list` is the half that stops the list rotting**, and it is the reason the tool is not
just a `grep`: a forthcoming API is *absent today and present later*, so an entry that becomes
live must fail loudly rather than silently policing a name that came back. It resolves each
symbol at **both refs** — the requirements' constraint 1 — and reports any entry now present.

> **It does not run in the `check` job, and the design review is why.** `--verify-list` needs two
> refs of two repositories, and **nothing in `docs.yml` checks out `../Brighter` or `../Darker`**
> — every tool in the `check` job reads only this repository. It belongs in the **scheduled
> `versions` job**, with `actions/checkout` for both source repositories, on the reasoning that
> file already states for `versioncheck.py`: *"the event that invalidates a pinned version is a
> release in another repository, so a push/PR-only trigger would leave a stale pin undetected."*
> A watchlist entry is invalidated by precisely that event.
>
> **The gate half needs none of this.** Checking that a named string is absent from `contents/` is
> a text search over this repository, which is why it can be a `check`-job gate at all — and why
> splitting the two halves is a property of the design rather than a concession to CI.

**Three states, per the maintainer's 2026-09-05 ruling**: live (present at the pin), forthcoming
(absent at the pin, present on `origin/master` — **documentable, but the page must say so**), and
dead. `--verify-list` is what distinguishes state 2 from state 3, and a watchlist entry that
turns out to be *forthcoming* is removed, not repaired.

**Opt-out, following rule 5's precedent exactly:**

```markdown
<!-- symbolcheck: allow IAmACommandStoreAsync -->
```

A page that discusses a dead name on purpose — as `MSSQLOutbox.md` now says *"there is no
`MsSqlOutboxBuilder`"* — declares it per symbol, not per page.

### 4.3 Product, and why the banner is load-bearing twice

`.AddPolicies(` is dead in Brighter and **alive in Darker 4.1.1**. `--verify-list` therefore
resolves a symbol against the product the page claims, read from the banner's *Applies to* — the
only machine-readable statement of it. **This makes `pagelint` rule 2 a dependency of a second
tool**, which is worth stating in `CLAUDE.md` where rule 2 is defined: it is no longer only
about telling a reader which version they are looking at.

### 4.4 How names reach the list — `--census`, which is a report

The census survives as the discovery mechanism, run deliberately rather than in CI:

- It prints the 831 with their pages, **sorted by page-spread**, because a name on many pages is
  far likelier to be a real API than a one-page example type — `IMessageScheduler` sat at 7 in
  that ordering, among `CustomerId` and `OrderStatus`.
- **It is not a gate and must never become one without a second measurement.** If the corpus's
  example-domain problem is ever solved, re-measure before promoting it.
- **It prints its own controls and refuses to report on an empty token set** (§3.1).

### 4.5 Red-proof — AC9, restated to what is achievable

| Proof | Expected |
|---|---|
| Run the gate before D9, **locally, in phase 2** — output pasted into `tasks.md` | **red on 4 symbols across 11 pages** |
| Run it after D9, in phase 3, where the CI job also lands | **green, and not vacuous** |
| Delete an entry from the TSV, re-run | green — proving the list, not the corpus, is what fires |
| Corrupt a ref name in `--verify-list` | **exits non-zero**, never a clean pass |
| `--verify-list` against `CommandProcessor` | reports it **live**, so the list's own test discriminates |

**AC9 as approved says D8 goes red on "the three held symbols".** It is now **four**, and a
census-shaped D8 would have managed one. §11 Q1 puts the change to review rather than assuming
it.

---

## 5. D9 — the corpus repairs. **Five symbols, 11 pages**

> **AMENDED 2026-09-13 by phase 1's write-up** — `tasks.md` § *Phase 1 as executed*, finding D.
> The `IMessageScheduler` row's *8 pages* was a **substring** count covering two dead names:
> `IMessageScheduler` (7 pages, 11 sites) → `IAmAMessageScheduler`, and
> **`IMessageSchedulerFactory`** (2 sites — `InMemoryOptions.md:241`, `InMemoryScheduler.md:180`)
> → **`IAmAMessageSchedulerFactory`**, both 0 files at both refs under `git grep -lw`.
> **The page total is unchanged at 11**, so §10's red-proof and AC7 stand; the watchlist is five
> rows, and §4.2's four-row example is one row short of what task 2.1 writes.

| Symbol | Pages | Repair |
|---|---|---|
| `IAmACommandStoreAsync` | `BuildingAnAsyncPipeline.md:36,38` — **in a C# fence** | **`IAmAnInboxAsync`** — command sourcing is the Inbox side. Verified: `UseInboxHandlerAsync` takes `IAmAnInboxAsync` (`src/Paramore.Brighter/Inbox/Handlers/UseInboxHandlerAsync.cs:55,67`) |
| `UseExternalInbox` | `DispatcherConfigurationReference.md:255` — prose on a **Reference** page | describe the V10 configuration; the method is gone, not renamed |
| `IAmAnIbox` | `HowBrighterWorks.md:94` | `IAmAnInbox` — a typo |
| **`IMessageScheduler`** | **7 pages, 11 sites, §3.4** | **`IAmAMessageScheduler`** (51 files at both refs) |
| **`IMessageSchedulerFactory`** | **2 sites: `InMemoryOptions.md:241`, `InMemoryScheduler.md:180`** | **`IAmAMessageSchedulerFactory`** (11 files at both refs) |

**These are the only `contents/` changes 014 makes**, and §10's red-proof permits exactly these
11 files and fails on a twelfth.

**Three are renames and two are not.** `IMessageScheduler` → `IAmAMessageScheduler`,
`IMessageSchedulerFactory` → `IAmAMessageSchedulerFactory` and
`IAmAnIbox` → `IAmAnInbox` are substitutions with a read-through — and the first two are why
*read-through* is in that sentence, since a substring pass over the first produces the second's
answer by luck rather than by method. `IAmACommandStoreAsync` is a
rename *across a concept boundary* — the page's example is V8-era and names an interface whose
successor lives on the Inbox side, so the surrounding code has to be read, not sed'd. And
`UseExternalInbox` is not a rename at all: **the method went**, so the paragraph has to be
rewritten around what V10 actually does.

> **A near-miss worth recording, because it is this spec's own defect class.** The first draft of
> this section gave `IAmACommandStoreAsync` the replacement **`IAmAnOutbox`** — plausible, wrong,
> and asserted without resolving it. The check that caught it took one command. **A design that
> prints an API is subject to the same liveness obligation it is writing into `/spec:implement`**,
> which is §7.1's point arriving a phase early.

---

## 6. The P0 commands

### 6.1 D5 — `/spec:implement` *(defects 1, 2, 3, 8)*

Current: 42 lines. Target: **~70**, and the growth is justified per constraint 4 because three of
its five current bullets are wrong or incomplete.

| Section | Change |
|---|---|
| **Writing Guidelines → Structure** | **Replace the single skeleton.** Tutorials and how-tos use `## Step N: …`; Reference and Explanation use the qualified-section pattern. **Cite `CLAUDE.md` § *File Organization Pattern*, do not restate it** |
| **…same bullet** | Add the **banner** — rule 1, an error on every page — and the **opening sentence** and **`description:` front matter**, rule 7, by reference |
| **Quality Check** | **Split into two lists**: *what a tool decides* (`pagelint.py <path>`, `linkcheck.py <path>`, `symbolcheck.py <path>`) and *what you decide* (is the page the type it claims; does an example teach the thing; is a version marker right — the one convention with no rule) |
| **New — API liveness** | Before a page names a type or method: it is live, or forthcoming **and the page says so** with the corpus's existing form `> **Not in a released package yet.**`, or it is a defect. Cite the three-state ruling |
| **New — compile** | Every new or edited C# block compiles, 009's harness method, and `// ...` declares a genuine omission so rule 6 downgrades it honestly |
| **frontmatter** | `allowed-tools` gains `Bash(python3 tools/*.py:*)` — constraint 3 |

### 6.2 D2 — `/spec:requirements` *(defects 5, 6, 7, 9, 16, 17)*

Current: 48 lines. Target: **~85**.

- **Declare the spec's subject**: `feature` · `reader problem` · `process`. This is friction 18's
  repair — the template's ten sections assume a feature, and a process spec's *SUMMARY.md
  changes* is a category error rather than an empty section. The declaration decides which
  sections apply and **which research steps** run.
- **Research steps, per subject.** Feature → ADRs, release notes, source, samples (today's list).
  Reader problem → **GitHub discussions, issues, `FAQ.md`**, deduped **by thread, not by row**,
  paginated before any count is quoted. Process → the commands, `CLAUDE.md`, the closed specs'
  `tasks.md`.
- **Acceptance criteria by name**, and **each names its instrument or is explicitly marked as
  having none.** State why: both criteria ever found unmet at a close — 009's AC7, 012's AC1 —
  were unmarked ones. Phrase criteria as *contains*, not *ends with* (defect 16), and **a
  criterion whose instrument is "a file exists" is not instrumented** (defect 17: nothing reads
  `pagetypes.tsv`).
- **Open questions by name**, with a recommendation each.
- **Re-derive the README** before executing it, with the command beside each figure.
- **The mode mix**: for each deliverable, which of the four page types, and why. A feature that
  gets only a Reference page is a decision.

### 6.3 D6 — `/spec:review` *(defects 3, 5, 8)*

Current: 87 lines, one gate. Target: **~120**.

- **All eight gates**, each with the pre-existing-versus-yours discipline `linkcheck.py` already
  has — the model is copied, not reinvented.
- `allowed-tools` gains the seven other commands. **Today it permits `linkcheck.py` alone**, so
  prose changes without this are forbidden instructions (§2.2 of the requirements).
- **The expected numbers live in D10's committed home and are cited**, not pasted — a pasted
  number is the drift `optioncheck` exists to prevent, pointed at ourselves.
- **A fourth phase checklist: Acceptance.** Walk each criterion with evidence; **start with the
  ones marked as having no instrument**; record what was found wrong *before* it was fixed.
- Keep the existing behaviour where the requirements/design phases do not block on pre-existing
  breakage. It is right, and it is why `linkcheck` is the model.

---

## 7. The P1 commands

### 7.1 D3 — `/spec:design` *(defects 10, 11, 12, 13)*

Target: 54 → **~85** lines.

- **A page type per file, in the outline**, alongside the banner, qualified `##` headings, the
  opening sentence and `description:` front matter — all cited to `CLAUDE.md`.
- **Which of the two `SUMMARY.md` nestings** a new page takes, and that **nesting moves a URL**
  and needs a redirect.
- **Verify the API the design will print.** *"Note source file/sample"* asks where an example came
  from, never whether it is right. §3.4 is this defect met at design time: eight pages named a
  type nobody had ever resolved.
- **Re-verify the requirements, do not quote them.** §3 of this document is the instance —
  the probe changed D8's shape and D9's size.
- **Predict which gates the work will move**, including *none*, since a vacuous pass is invisible
  exactly when no movement is expected.

### 7.2 D4 — `/spec:tasks` *(defects 4, 5, 9, 14, 15)*

Target: 63 → **~95** lines.

- **One phase is one pull request**, merged before the next branch starts — cite D10's home.
- **Phases are deliverable-shaped.** The *Research → Core → Supporting → Polish* template stays
  as **an explicitly labelled default**; defect 14 is that an unlabelled default invites a task
  list that fights its own design.
- **A standing-obligations section** — the things every task owes, stated once (012 §1's ten).
- **Red-proofs**: a check that has never failed has not been shown to work.
- **Record the mismatch before fixing it**, which is the only evidence a spec produces that the
  corpus was ever wrong.
- **Re-derive inherited counts**, with two independent methods that must agree.
- **An acceptance phase, last**, owning the walk and the defect ledger.

### 7.3 D1 — `/spec:new` *(defect 9)*

Target: 63 → **~75**. The README template gains **Acceptance criteria**, **Open questions**, and
one line that has earned its place twice in this spec alone:

> **Re-derive this README before executing it.** A spec that starts by executing a stale README
> ships work that is already done. 013's README named six gaps and five were closed; 014's
> defect count was wrong by two before this phase started.

### 7.4 D10 — the committed home *(Q3)*

**`tools/README.md`, new**, because the gate numbers are facts about tools and `CLAUDE.md` §
Enforcement is about page rules. It carries: the eight commands, what each checks, the **expected
numbers at a named ref**, and the phase-is-a-PR contract. **Only what a command cites moves** —
the board, branch tips, CI census and open-question log stay in `PROMPT.md`, which stays ignored.

`CLAUDE.md` takes one addition: a line at rule 2 recording that `symbolcheck` reads the banner
for product (§4.3), so nobody weakens the banner without knowing what depends on it.

---

## 8. P2

**D7 — `/spec:update-task`**: the task's stated **Output** must exist before the box flips; if it
cannot be checked, say so in the tick. **P2-2 — `commandlint`**: every tool a command's prose
names is permitted by its own `allowed-tools`; ~40 lines, and it red-proves §2.2 rather than
trusting it. **P2-3 — `/spec:status`** reports gate state.

---

## 9. Sequencing

**Six phases, five pull requests** — phase 1 is already executed and its write-up lands with
phase 2. The requirements deliberately left the count to this phase.

| Phase | Goal | Deliverables | PR | Gated by |
|---:|---|---|---|---|
| **1** | **The probe** — executed 2026-09-12, §3 | D12 | with 2 | — |
| **2** | **`symbolcheck.py`, its watchlist, its red-proofs — and NO CI job yet** | D8 | ✔ | 1 |
| **3** | **The four corpus repairs, AND the CI job, in one PR** | D9 | ✔ | 2 |
| **4** | **The three P0 commands, and the committed home they cite** | D5, D2, D6, **D10** | ✔ | 2 |
| **5** | **The three P1 commands** | D3, D4, D1 | ✔ | 4 |
| **6** | **Acceptance** — the walk, the ledger, the friction | D11 | ✔ | all |

**1 gates 2** for the reason the requirements gave: the checker's shape is decided by the
census's numbers, and §3 changed it. **2 gates 4**, because D5 and D6 cite the tool by name and
`allowed-tools` must permit a tool that exists.

> **AMENDED 2026-09-13 — D10 moved from phase 5 to phase 4**, at the top of phase 4 and before any
> file was opened. §6.3 requires `/spec:review` to **cite** its numbers from D10's committed home
> rather than paste them; with D10 a phase later, the choice was a merged command citing a file
> that does not exist, or the pasted numbers §6.3 forbids. **The same rule that put the CI job in
> phase 3 applies**: a thing and the thing it depends on merge together, or the dependant merges
> second. Task numbering is unchanged — 5.1 keeps its number and `tasks.md` § *Phase 4 as executed*
> records it.

> ### Why the CI job ships in phase 3 and not phase 2 — the design review's blocking finding
>
> **`.github/workflows/docs.yml` triggers `on: push`.** A gate wired in phase 2, while the corpus
> is still red on four symbols across eleven pages, **turns `master` red until phase 3 merges** —
> and makes phase 2's own PR red on arrival. *"The gate must be red before the fix"* is true of
> the **proof** and fatal as a **merge**.
>
> So the red-proof is run locally in phase 2 and **recorded in `tasks.md` § *Phase 2 as
> executed*** with its output pasted, exactly where 012 put its three red-proofs. Phase 3 then
> repairs the corpus and wires the job **in the same PR**, so the first CI run of `symbolcheck`
> is green — and **not vacuous**, because the watchlist has four entries the repaired pages would
> have tripped.
>
> **012 set this precedent deliberately**: task 2.6 marked an existing table so the new checker's
> first CI run was meaningful *and* passing. The rule generalises — **a gate and the corpus that
> satisfies it merge together, or the gate merges second.**

**Phases 3 through 6 are individually abandonable; 1 and 2 are not.** A spec that stopped after
phase 4 would leave a smaller 014 that is correct throughout.

---

## 10. `SUMMARY.md` changes — none

No page is created, moved, renamed or nested. The before/after diff the command asks for is
**empty, and that is the deliverable**:

```bash
git diff --stat origin/master -- SUMMARY.md            # must be empty
git diff --name-only origin/master -- contents/ | wc -l  # must be exactly 11 (D9)
```

The second permits precisely D9's eleven pages and fails on a twelfth. **This is the criterion
014 is most likely to break quietly**, because a spec about the workflow is the easiest place to
talk yourself into a "while I'm here" edit.

### Predicted gate movement

| Gate | Predicted | Why |
|---|---|---|
| `linkcheck.py` | **164 files, 0 broken — unmoved** | no link changes |
| `pagelint.py` | **162 pages, 0 errors; warnings 768 or lower** | D9 touches prose and identifiers, not fences — but a repaired block may earn a `using` line |
| `--check-shape` | **161 / 12 / 12 of 20 — unmoved** | no tree change |
| `--check-redirects` | **77 / 7858 — unmoved** | no URL moves |
| `versioncheck.py` | **0 of 18 across 5 — unmoved** | no pins touched |
| `optioncheck` | **0 across 59 / 519 — unmoved** | no marked table touched |
| `--verify` | **161 = 161 — unmoved** | no page count change |
| **`symbolcheck.py`** | **red on 4 symbols / 11 pages at phase 2; green after phase 3** | new |

**A prediction of "unmoved" on six of seven is exactly when a vacuous pass hides**, which is why
the `--changed` scope line gets read on every PR: a new page is 100% added lines, so `git add`
first or the strict pass sees nothing.

---

## 11. Questions for review

**Q1 — Does D8 become a watchlist checker (§4) instead of the census gate the requirements
assumed? — ANSWERED 2026-09-12: YES.**
**This amends approved requirement P0-5 and AC9**, and the amendment is the design phase doing
the job defect 11 says it never does: a design that re-verifies its requirements rather than
quoting them. The requirements assumed a census because nobody had measured one.
*The reasoning, as put:* The probe measured 831 unresolved symbols across 127 of 144 fenced pages,
dominated by the docs' own example domain, which no cheap filter separates from a real API name.
A census cannot go green, and a gate that cannot go green is not a gate. The watchlist is
red-proofable on day one, has no false positives by construction, and **catches 4 of 4 known dead
symbols where the census manages 1**. The cost is honest and should be stated in the tool's own
`--help`: **it finds only what is on it**, and `--census` is how the list grows. **If the answer
is no**, D8 needs a scope the probe has not found and phase 2 should be re-planned before it
starts, not during.

**Q2 — Does D9 grow from three symbols to four, taking in `IMessageScheduler` across 8 pages? —
ANSWERED 2026-09-12: YES.** **AC7's red-proof becomes 11 files, not 3** — §10 already carries the
amended command.
*The reasoning, as put:* It is dead at both refs, it is in C# blocks, the real name is
`IAmAMessageScheduler`, and it spans the whole scheduler family. Leaving it out would publish a
known `CS0246` for the length of another spec. **It changes AC7's red-proof from 3 files to 11**,
which is why it is asked rather than absorbed.

**Q3 — Does `tools/README.md` become the committed home for the gate numbers, or do they go into
`CLAUDE.md` § Enforcement? — ANSWERED 2026-09-12: `tools/README.md`.**
`CLAUDE.md` still takes the one-line note at rule 2 recording that `symbolcheck` reads the banner
for product (§4.3), because that is a fact about a page rule.
*The reasoning, as put:* `CLAUDE.md` is the authority on what a *page* must be; the
gates' expected numbers are facts about *tools* at a ref, and they move whenever the corpus does.
Putting a number that changes every phase inside the conventions document invites exactly the
drift constraint 1 exists to prevent.

---

## 12. Traceability

| Requirement | Where satisfied | Note |
|---|---|---|
| P0-1 D5 | §6.1 | |
| P0-2 D2 | §6.2 | |
| P0-3 D6 | §6.3 | eight gates, `allowed-tools` |
| P0-4 D9 | §5 | **grew to 4 symbols / 11 pages — Q2** |
| P0-5 D8 | §4 | **inverted — Q1** |
| P0-6 D11 | §9 phase 6 | |
| P0-7 D12 | §3 | **executed** |
| P1-1 D3 | §7.1 | |
| P1-2 D4 | §7.2 | |
| P1-3 D1 | §7.3 | |
| P1-4 D10 | §7.4 | **Q3** |
| P2-1 D7, P2-2, P2-3 | §8 | |
| AC9 | §4.5 | **restated — 4 symbols, and the census-shaped promise dropped** |
| AC12 | §3 | both numbers recorded; D8's scope and D9's size both cite them |
| Constraint 3 | §6.1, §6.3 | prose and `allowed-tools` ship together |
| Constraint 4 | §6, §7 | per-command target lengths, each justified |
| Constraint 6 | §10 | predicted movement, including the six unmoved |

---

## 13. Workflow friction — more of 014's own evidence

Continuing the ledger at 22.

22. **`/spec:design`'s contract asks for five things this design cannot have and none of the
    things it needs.** *Section outline with headings (H1, H2, H3)*, *target length in lines*,
    *key code examples with source/sample*, *glossary terms*, *SUMMARY.md before/after* — the
    deliverables are command files and a Python tool. Target length was kept because it maps onto
    constraint 4; the rest are either empty or category errors. **This is friction 18 at the next
    phase, and it confirms the repair belongs in the subject declaration** (§6.2), not in a
    one-off note in `/spec:design`.

23. **Nothing in the workflow says a design may run an experiment.** §3 is the most valuable part
    of this document and the command has no slot for it — no *evidence*, *probe* or *measured*
    step anywhere in its 54 lines. D3 gets one: **a design that rests on an unmeasured number
    says which number and how it will be measured, before the design is approved.** Had the
    probe run at implementation time as a normal reading of the phases would put it, D8 would
    have been built as a census first and rewritten after.

24. **The requirements' own acceptance criterion was wrong in a way only execution revealed.**
    AC9 promised the gate would go red on three named symbols; §3.3 shows a census reaches one of
    them. **The criterion was written before anyone knew where the three symbols lived** — two
    are prose. No command asks a criterion to name where its evidence will come from, which is
    one level deeper than defect 6's *name your instrument*.
