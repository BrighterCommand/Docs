# Spec 010: Information Architecture — Tasks

**Created:** 2026-08-08 · **Status:** **REVIEWED AND APPROVED 2026-08-08** — five findings
applied in place, four tallies and one omission; see **§4**. No verdict, threshold, placement
or ruling moved.
**Works from:** `design.md` (approved 2026-08-08, `.design-approved`) and `requirements.md`
(approved 2026-08-06)
**Executes against:** `spec/011-authoring_conventions/worklist.md` (42 rows, 26 `split`, 16 `keep`)

**Total tasks: 52, across 11 phases.**

---

## 1. How this list is organised

**Phase N is PR N.** Design §10 is already a task plan — eleven pull requests, each a
coherent unit of work merged before the next branch starts. This document does not
re-sequence it; it fills in what each PR has to do, and settles the seven things the
design deliberately left to this phase.

| Phase / PR | Goal | Tasks | Deliverables |
|---:|---|---:|---|
| **1** | Plan ratified, nesting pinned, #67 re-checked | 3 | this document, §12 README amendments |
| **2** | **The tree.** One PR, and the one that must not be partial | 10 | D1, D2, D3 |
| **3** | Content defects, the duplication **verification**, and the split harness | 4 | D9, D5 |
| **4** | Scheduler family — 6 rows, 2 new pages | 4 | D4 |
| **5** | Outbox and Inbox — 4 rows, **8** new pages, 24 of the 34 anchor links | 5 | D4 |
| **6** | Darker — 5 rows, 12 new pages | 5 | D4 |
| **7** | Using an External Bus — 4 rows, 3 new pages | 4 | D4 |
| **8** | Transports — 2 rows, 2 new pages | 2 | D4 |
| **9** | The rest of §6d — 5 rows, 5 new pages | 6 | D4 |
| **10** | `llms.txt` — **re-scope D6 first; the platform already does most of it (§3)** | 4 | D6 |
| **11** | Glossary links, the two carried-over chores, acceptance | 5 | D8, P2-1, P2-3 |

> **The new-page counts in that table count what each phase *creates*, and they sum to 32.**
> Corrected at review: phase 5 read *7 new pages*, which is the number landing in *Outbox and
> Inbox* — design §7.6's section column — rather than the number Task 5.4 and its siblings
> create. `InMemoryTransport.md` is created in phase 5 and files into *Transports*, so the
> phase creates **8**. As written the column summed to 31 against Appendix A's 32, which is
> the arithmetic that exposed it. **Appendix A is the authority on placement; this column
> counts authorship.**

**PRs 4–9 are individually shippable and individually abandonable.** AC7 is per-split and
**partial completion is an explicit valid end state** — the maintainer's ruling. Nothing in
this list re-introduces an all-26-splits gate: a phase that lands three of its five rows has
landed three splits, and the spec is accepted on what it landed.

**PR 2 is the exception.** It is the tree every later PR files into, so it goes in whole or
not at all.

### Dependencies

```text
1 ──> 2 ──> 3 ──> 4 ─┐
                 ├─> 5 ─┤
                 ├─> 6 ─┼──> 10 ──> 11
                 ├─> 7 ─┤
                 ├─> 8 ─┤
                 └─> 9 ─┘
```

- **Phases 4–9 are independent of each other** and may be done in any order, or not at all.
  They share only `SUMMARY.md`, and each touches its own lines of it.
- **Phase 3 gates phase 6** on one point only: Task 3.1's verification must return before
  Task 6.4 moves `QueriesAndQueryObjects.md`'s `## Query Patterns`. Task 3.4 (the D5 check)
  gates every split task in 4–9.
- **Phase 10 waits on 4–9** — `llms.txt` covers whatever pages exist when it runs, so it
  runs last among the content phases. If phases are abandoned it still runs, over a smaller
  corpus.

### The standing obligations — every split task in phases 4–9 owes all seven

Do not restate these in each task; they are assumed by all of them.

1. **Move the text; do not improve it** (`worklist.md` §4 rule 4). The only new prose this
   spec authors is one sentence, in Task 7.1.
2. **The core keeps its original filename** (rule 1), so no published URL moves and no
   page-level redirect is created by a split.
3. **Grep for every anchor before moving the heading that owns it** (rule 2). Design §8 has
   the measured inbound table, but **re-derive per split** — it is a snapshot, not a
   guarantee.
4. **Every new page carries a banner** one blank line below the H1, separator ` · ` (U+00B7),
   and a *Prerequisites* segment naming the core it came from. Every `##` heading is
   qualified by its subject and unique across pages; the five navigation headings are exempt
   and stay uniform.
5. **Every new page gets a `SUMMARY.md` entry at the position Appendix A pins**, in the same
   commit that creates it. `linkcheck.py`'s orphan check has no exemptions.
6. **Run the D5 check (Task 3.4), then `linkcheck.py` and `pagelint.py`** after every split.
   A block moved verbatim that cannot carry its `using` directives marks the omission
   `// ...`, which downgrades to a warning and never silences. **Do not backfill namespaces
   you have not checked.**
7. **Append a row to `spec/011-authoring_conventions/pagetypes.tsv` for every page created,
   and edit the row for every page retyped.** Append; never re-sort — the file's order
   follows no single rule and re-sorting churns 57 reviewed rows for nothing. **Added at
   review, because nothing else in this document required it**: the TSV is 110 rows against a
   corpus that ends at 142, `apply_banners.py` reads it, and a version bump would therefore
   have **skipped all 32 new pages**. That is not hypothetical — Spec 011's Phase 6 splits
   left five pages out of the TSV for exactly this reason, fixed in `5498cd6`. Session 9's two
   parity checks (banner type ↔ `verdict`, banner version ↔ `applies`, both 110/110) would
   have started failing on 32 pages, and **`pagelint.py` never reads the TSV, so no tool sees
   it** — the same invisible-to-a-green-build shape as 011's AC5 failure. Two pages are
   *retypes* rather than creations: `ReplayOnSeen.md` (Task 5.2, Reference → Explanation) and
   `InMemoryOptions.md` (Task 5.5, Reference → How-to). The sweep now preserves a
   *Prerequisites* segment (`5498cd6`), so re-running it after an edit is safe.

### Two conventions this document holds itself to

- **A page's length is `len(text.splitlines())`.** Not `wc -l`, which under-reports the 17
  files with no trailing newline, and not `read().split("\n")`, which over-reports the 93
  with one. This is design §16 finding 1, and it is the fifteenth lesson in `PROMPT.md`.
- **Every "after" line count quoted below is arithmetic on today's sections**, not a count
  of a page that exists (design §15). They are budgets, not assertions, and requalified
  headings and new lead-ins will move them.

---

## 2. What this phase settled — the seven items design left open

`PROMPT.md`'s handover names seven. Each is discharged here, and two of them produced a
finding.

| # | Item | Where it is settled |
|---|---|---|
| 1 | Where each of the 32 new pages nests | **Appendix A** — derived and measured, two findings |
| 2 | PR 3 verifies the duplication before anything moves | **Task 3.1**, written so "not duplicate" is a passing outcome |
| 3 | `BrighterOutboxSupport.md` is a split target *and* a parent page | **Task 5.1** |
| 4 | `BoxProvisioning.md#when-to-use-box-provisioning` must not be requalified | **Task 2.8** and Task 11.5 — no task in this list touches that heading |
| 5 | The `HowServiceActivatorWorks.md:147` fold | **Task 3.3** |
| 6 | Every new page is 100% added lines | Standing obligation 6, above |
| 7 | Verify `.gitbook.yaml` **before** merging PR 2 | **Task 2.6** and Task 2.8 |

**Three findings. The first started as a constraint and ended as a measurement.**

- **S3 was an assumption. It is now measured, and it is four segments, not three.** Pinning
  the nesting forced `MigratingToPollyV8.md` to become a *sibling of its own source*, because
  `PolicyRetryAndCircuitBreaker.md` already publishes at three
  (`commands-handlers-and-pipelines/buildingapipeline/policyretryandcircuitbreaker`) and a
  child would have breached S3. Asking **what caused S3** showed that its stated rationale
  was the absence of evidence — *"three is the deepest the live site is known to work at"* —
  which is a fact about our own `SUMMARY.md`, not about the platform. Measured 2026-08-08:
  GitBook's own documentation publishes **30 pages at four segments**, and PRs #83/#84
  established the same for this site, with the probe reverted minutes later and the tree left
  byte-identical. **Design §4 is amended and §17 records the evidence.** Consequences:
  `MigratingToPollyV8.md` nests under its own source, and `AzureBlobArchiveProvider.md` and
  its configuration child nest under the new `OutboxArchiver.md`.
- **`OutboxArchiver.md` and `TransactionalMessagingWithTheOutbox.md` are top-level**, and
  *Outbox and Inbox* lands at **9** top-level entries. Design §7.6 intended 10; nesting all
  seven new pages would have given 8. Nine is 10 minus `AzureBlobArchiveProvider.md`, which
  now nests under `OutboxArchiver.md` — the archive provider belongs under the archiver, and
  §7.6's reason for keeping it top-level was the S3 ceiling that no longer exists. §15
  flagged this column as an intention rather than a measurement and named 15 as the breach
  case; the pinned answer is 9, three entries under S2. **No verdict changes**, and the one
  threshold that moved, moved because it was measured.
- **The cost, stated rather than buried:** `AzureBlobArchiveProvider.md` and
  `AzureBlobConfiguration.md` are **the only two pages whose URL moves twice** — to *Outbox
  and Inbox* in PR 2, then under `OutboxArchiver.md` in PR 5, because `OutboxArchiver.md`
  does not exist until PR 5 and PR 2 touches no page body. **PR 5 owes two extra redirect
  entries** for the intermediate paths. Design §7.6's "no page's URL moves twice" holds for
  the other 140.

**And a third finding, from enumerating rather than reading.** Requirements §12 (AC8) and
§14 both say the 16 `keep` rows cover **fifteen** distinct pages. Enumerated, they name
**sixteen**:

```bash
python3 - <<'PY'
import re
rows = [l for l in open("spec/011-authoring_conventions/worklist.md") if re.match(r'^\| `', l)]
keep = [l for l in rows if re.search(r'\*\*keep', l)]
pages = {p for l in keep for p in re.findall(r'`([^`]+\.md)`', l.split("|")[1])}
print(len(keep), "rows,", len(pages), "distinct pages")   # 16 rows, 16 distinct pages
PY
```

Two rows name `TickerQScheduler.md` (§6a and §6e) and **one row names two pages** —
`HandlerFailure.md` + `ErrorHandlingOptions.md`, listed as a pair because the maintainer
raised a live question about them. So 16 rows produce 17 page mentions, and deduplicating
gives 16 pages, not 15. **Fifteen is the count of distinct *rows by subject*, not of pages.**
Recorded here rather than edited into an approved document; **AC8's substance is untouched**
— all 16 rows are honoured either way, and §7 touches none of them. It is the fourteenth
wrong tally in this programme, and the fourteenth that is only a tally.

---

## 3. What the platform already does, measured 2026-08-08

Checked while reviewing Task 10.3, and it re-scopes D6 rather than answering the question
that was asked. **All of this is live on our site today, automatic, with no configuration.**

| Endpoint | Result |
|---|---|
| `/llms.txt` | **200**, 27,470 bytes, `text/markdown`, **170 link entries** |
| `/llms-full.txt` | **200**, 1,332,541 bytes — the whole corpus in one file |
| any page + `.md` | **200**, `text/markdown` — raw source, no HTML |
| any page + `?ask=<question>` | documented query endpoint |
| `/~gitbook/mcp` | **405 to a GET**, so the route exists; it is an MCP server, POST-only |

**Four findings, in descending order of how much they matter:**

1. **59 of the 170 entries are V9, and nothing marks them.** They come from a separate
   *V9 Paramore Brighter Documentation* space and sit in the same index as our 111. Fetching
   both as markdown: a V10 page leads with `# Kafka Configuration` then
   `> **Reference** · Applies to **Brighter V10**`; **the V9 page goes straight from `#
   Basic Concepts` to `## Command` with no version marker at all.** The banner works —
   *and the index undoes it.* Task 10.2.
2. **The format `CLAUDE.md` specifies is already the platform's format.** GitBook's own
   `llms.txt` emits `- [Title](url): description` on **294 of 668** entries. Ours emits
   descriptions on **zero**, because no page has one set. Task 10.1.
3. **We cannot serve our own `/llms.txt`.** GitBook owns that path. A generated file at the
   repository root would live on GitHub only, competing with the canonical one — which
   changes what D6 is *for*, and is why Task 10.3 is a ruling rather than an implementation.
4. **Sections in the generated file are GitBook *spaces*, not our information
   architecture** — three `##` groups, none of them our twelve. Whatever the ruling, the
   tree this spec builds does not reach that file.

> **An incidental corroboration, and it is still live.** GitBook's *Content configuration*
> page — the one this repo's `.gitbook.yaml` was written from — **still contains U+200B in
> its `### Structure` heading today**, fetched 2026-08-08. The contamination recorded in
> requirements §2.4 is present, not historical. **Type the block; never paste it.**

---

## 4. What the tasks review changed — 2026-08-08

**Five findings: four tallies and one omission. No verdict, threshold, placement or ruling
moved.** Each is corrected in place above, with a note saying what it said before. The
figures that reproduced are listed after, so they are cited rather than recomputed.

| # | Finding | Was | Is |
|---|---|---|---|
| 1 | Phase 5's new-page count, §1's table and the phase goal | 7 | **8** — the column summed to 31 against Appendix A's 32 |
| 2 | Phase 5's share of the anchor links, in **three** places | 19 of 34 | **24** links, **11** of the ≈19 repoints |
| 3 | Appendix A's depth-table introduction, contradicting its own rows | six sources, other 20 | **seven** sources, other **19** |
| 4 | `DefaultMessageMappers.md`, Task 7.2 against Task 7.1 | 479 | **478** — the file has a trailing newline |
| 5 | **`pagetypes.tsv` was named once in the whole document** | — | **standing obligation 7**, plus a parity line in Task 11.4 |

**Finding 5 is the one that could have broken something.** Nothing required the 32 new pages
to be added to `pagetypes.tsv`, which is 110 rows against a corpus ending at 142.
`apply_banners.py` reads that file, so the next version bump would have skipped all 32 — the
identical defect Spec 011's Phase 6 splits produced and `5498cd6` fixed. **No tool checks the
parity**: `pagelint.py` never reads the TSV, so a green build says nothing about it, which is
the same shape as 011's AC5 failure. Two *retypes* were half-covered — Task 5.2 named the TSV,
Task 5.5 did not.

**Findings 1–3 share a mechanism worth naming: each number was right about something.** Seven
is the count of new pages landing in *Outbox and Inbox*; nineteen is the whole-spec repoint
total; six-plus-twenty sums to the correct 26. A figure that is true of a neighbouring quantity
survives review because it reads true in the sentence it is in. **Enumerate the table; do not
read its introduction.**

**Re-derived at review and reproduced exactly — cite these:**

- **52 tasks**, and all eleven per-phase counts in §1's table
- **Appendix A**: 32 rows, 5 top-level, 27 nested, maximum depth 4 reached by exactly two
  pages; the section allocation reproduces design §7.6 **row for row**
- **Appendix A's entries table: all twelve rows**, computed by applying its nesting to
  `SUMMARY.target.md` — *Transports* 7, *Outbox and Inbox* **9**, *Scheduler* 4, maximum 10.
  **S1, S2 and S3 all hold**, with two entries of headroom under S2
- **All 36 quoted `##` section spans**, and **25 of 26** whole-page line counts (the 26th is
  finding 4). Every "after" figure is consistent arithmetic on them: 517−151−169=197,
  578−212=366, 695−365=330, 1291−985=306, 928−159−53=716, 615−245=370, 687−247=440
- **74 moved / 36 unchanged**, recomputed from `SUMMARY.target.md` against today's `SUMMARY.md`
- **26 split rows, 16 `keep` rows, 16 distinct `keep` pages** — §2's third finding reproduces
- `## How It Work` at `SweeperCircuitBreaking.md:16`, the only occurrence in the corpus
- `AWSSQSConfiguration.md` at **615** is right under both counting conventions, exactly as
  Task 8.1 claims: the file has no trailing newline
- `## Transformers` (119) and `## Legacy: Using Polly v7 Policies (Deprecated)` (151) are each
  the **last** `##` on their page, which is why both notes flag the counting artefact
- `ErrorHandlingOptions.md` is **already nested** under `HandlerFailure.md` in
  `SUMMARY.target.md`, so Task 9.6 moves no URL and Appendix A's *only two pages re-parent*
  holds

---

## Phase 1 — Plan (PR 1)

**Goal:** the plan is ratified and merged before any file under `contents/` is touched.
PR #82 is already open with `design.md`, `SUMMARY.target.md` and the three README
amendments (design §12); this document joins it.

- [x] **Task 1.1:** Ratify Appendix A — the nesting of all 32 new pages
  - Input: Appendix A of this document; `design.md` §7.6 and §15; `SUMMARY.target.md`
  - Output: maintainer's confirmation, or corrections applied to Appendix A
  - Notes: This is the item design §15 named as *not measured*. Two answers changed as a
    result — `MigratingToPollyV8.md`'s sibling placement (forced by S3) and *Outbox and
    Inbox* at 8 entries rather than 10. Neither is discretionary; both fall out of the
    measured depths in Appendix A's second table. **If a placement is overruled, re-run the
    depth check before accepting it** — the S3 ceiling is what makes these forced.

- [x] **Task 1.2:** Re-check [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)
  - Input: the issue thread
  - Output: a note in this file recording the state and date
  - Notes: Last checked 2026-08-08 — **two comments, both the maintainer's**, unchanged
    since 2026-08-03. Diátaxis-as-authoring-discipline was flagged there for pushback, and
    the redirect commitment is public. **Check again before PR 2 merges** (Task 2.8), not
    just here.
  - **Done 2026-08-08.** State: **OPEN**, `updatedAt` **2026-08-03T11:43:25Z**, **2 comments,
    both `iancooper`** — the acknowledgement and the follow-up naming #72. **No reply from
    `RubenvanderHout`.** So nothing external constrains the design: neither of the two things
    flagged for pushback (Diátaxis as authoring discipline, and the prose-vs-generated
    reference distinction) has drawn one. The issue **stays open until the work lands** — do
    not close it on spec approval. Re-check at Task 2.8, because PR 2 is the merge that moves
    74 public URLs and is where a late objection would be most expensive.

- [ ] **Task 1.3:** Merge PR #82
  - Input: PR #82 on `docs/spec-010-design`
  - Output: `design.md`, `tasks.md`, `SUMMARY.target.md` and the README amendments on
    `master`
  - Notes: `master` requires a review and GitHub blocks self-approval, so this is
    `gh pr merge 82 --merge --admin`. Touches no file under `contents/`, so linkcheck stays
    at 112 files and pagelint at 0 errors / 802 warnings.

---

## Phase 2 — The tree (PR 2)

**Goal:** D1, D2 and D3 land together. `SUMMARY.md` becomes the twelve-section tree, 74
redirects ship with it, and `urlmap.py` moves to `tools/` with two new checks gating CI.
**No page body is touched by this phase at all** — that separation is what makes the splits
safe to interleave afterwards.

- [ ] **Task 2.1:** Move `urlmap.py` to `tools/`
  - Input: `spec/010-information_architecture/urlmap.py` (validated 110/110 against the live
    sitemap, re-verified 2026-08-06)
  - Output: `tools/urlmap.py`; the spec copy deleted; `__pycache__` not committed
  - Notes: D3 is *packaging, not invention*. Keep `--verify`'s exit-2-on-unreachable
    behaviour exactly as it is — an unreachable authority is not a pass. Keep the tolerant
    `^\s*##\s+` section regex; it must model what GitBook does, not what the file ought to
    say.

- [ ] **Task 2.2:** Add `--check-shape`
  - Input: `design.md` §4 (S1/S2/S3) and §9.1
  - Output: `tools/urlmap.py --check-shape`, exit 0/1
  - Notes: Asserts **S1** every section holds ≥2 pages, **S2** ≤12 top-level entries, **S3**
    no published path exceeds **4 segments** — and that no `SUMMARY.md` heading carries
    leading whitespace. That last assertion is P0-1's, kept because the hazard is the class
    and not the ` ## Under the Hood` instance the new tree deletes. **S3 is 4, measured, not
    3, assumed** — design §17. **Prove it fails**: temporarily nest one page to five segments
    and confirm S3 goes red, then revert. A check that has never been red is a check nobody
    has tested — and S3's own ceiling stood unquestioned through requirements, design and a
    design review because nobody asked what caused it.

- [ ] **Task 2.3:** Add `--check-redirects`
  - Input: `design.md` §9.1; requirements P0-3 and §2.4
  - Output: `tools/urlmap.py --check-redirects`, exit 0/1
  - Notes: Three assertions — every `redirects:` value resolves to a file that exists, every
    key is a path that no longer publishes, and **the whole of `.gitbook.yaml` is printable
    ASCII**. Parse the flat `key: value` block in ~15 lines of Python: PyYAML is absent from
    this environment and `ruby -ryaml` is an accident of the machine. A YAML parser would
    have parsed `​structure:` happily, which is the entire reason the byte check exists.

- [ ] **Task 2.4:** Install the new `SUMMARY.md`
  - Input: `spec/010-information_architecture/SUMMARY.target.md` — 145 lines, 110 links, 12
    sections, pure ASCII, verified at design review
  - Output: `SUMMARY.md` replaced wholesale
  - Notes: This is D1 and it is a copy, not a rewrite — every figure in the design
    reproduces against that file, so retyping it would put them at risk for nothing. The
    encoded link `Requests%2C%20Commands%20and%20Events.md` stays: Q6 is dropped, and the
    awkward filename makes the better URL (§6.3).

- [ ] **Task 2.5:** Generate and **type** the `redirects:` block
  - Input: `python3 tools/urlmap.py --redirects <old SUMMARY.md>`; requirements §2.1
  - Output: `.gitbook.yaml` gains a `redirects:` block of **74** entries
  - Notes: **Type the block; never paste it.** GitBook's own published `.gitbook.yaml`
    example still carries two U+200B zero-width spaces today, with the `redirects:` snippet
    inside the same code block — that is where this repo's went, and one of them was in a
    *key*, so GitBook had never read the `structure:` block at all and nothing looked
    broken. Key is the old **published** path, value is the **repository** path
    (`contents/<FileName>.md`), no leading slashes on either side. The block is written
    **once**: a redirect value does not go stale when its page moves again, because GitBook
    resolves the repository path to wherever that page currently publishes (requirements
    §16).

- [ ] **Task 2.6:** Verify `.gitbook.yaml` mechanically — **AC4**
  - Input: the file as written by Task 2.5
  - Output: a recorded parse result and byte inspection, both clean
  - Notes: Malformed indentation disables redirects *silently* rather than erroring. Run
    `--check-redirects` and, independently, assert no byte outside printable ASCII anywhere
    in the file. **Before merge, never after** — see Task 2.8.

- [ ] **Task 2.7:** Wire both checks into CI
  - Input: `.github/workflows/docs.yml`, the `check` job
  - Output: `--check-shape` and `--check-redirects` run on every push and PR
  - Notes: `--verify` stays **out** of CI: it depends on an external site and would make the
    build flaky. Q10's answer is yes for the two checks that read only the repository. A
    redirect block complete at merge and incomplete three PRs later is the same silent
    failure in slow motion, which is why these gate rather than being run once.

- [ ] **Task 2.8:** The pre-merge gate
  - Input: the branch as it stands
  - Output: a recorded result for each of six checks
  - Notes: `linkcheck.py` clean; `pagelint.py` 0 errors; `--check-shape` green;
    `--check-redirects` green; the block has **74** entries, matching design §5's measured
    figure; and #67 re-checked for a reply (Task 1.2). **Redirects cache with
    `stale-while-revalidate=2592000` — thirty days — so a wrong redirect outlives its fix at
    the edge.** Also confirm no task in this PR requalified
    `BoxProvisioning.md#when-to-use-box-provisioning`; Spec 009's rung 3 links to it and
    redirects cannot fix a fragment.

- [ ] **Task 2.9:** Post-merge live sample — **AC5b**
  - Input: the published site, 25–45 seconds after merge
  - Output: recorded status, `location:` header and body size for a sample of the 74
  - Notes: **Every cached response reports `200`**, genuine 404s and genuine redirects
    alike, so *status code alone is worthless on this site*. The tell is that no genuine
    page response carries a `location:` header; body size separates the rest — ~192 KB for a
    redirect, ~189.5 KB for the 404 shell, **584 KB for a real page**. Sample the sections
    that were renamed, including the 3-segment nested paths under *Transports*.

- [ ] **Task 2.10:** Re-probe PR #77's old path
  - Input: `command-processors-and-dispatchers/commandscommanddispatcherandprocessor`
  - Output: a recorded observation of whether it still carries a `location:` header
  - Notes: **The one open platform unknown** — whether GitBook's automatic redirects
    *persist*. They may be tied to revision history, and one session could not test it. This
    is a measurement, not a gate: the `.gitbook.yaml` block ships regardless, because that
    is what it is for. If the header has gone, the block is the reason nothing broke.

---

## Phase 3 — Defects, the verification, and the harness (PR 3)

**Goal:** the three `worklist.md` §7 content defects (D9 / P1-4), and the D5 check that
every split in phases 4–9 depends on.

- [ ] **Task 3.1:** **Verify** the `QueriesAndQueryObjects.md:746` ↔ `QueryPatterns.md`
      duplication
  - Input: `contents/QueriesAndQueryObjects.md` `## Query Patterns` (102 lines) against the
    whole of `contents/QueryPatterns.md`; `worklist.md` §7
  - Output: a written finding recorded in this file — *duplicate* or *not duplicate*, with
    the evidence
  - Notes: **This is a finding, not an edit, and "not duplicate" is a passing outcome.**
    Spec 011's Task 4.5 is the precedent and the reason this is a separate task: three
    specified "duplicate content" defects turned out not to exist, one of the cited line
    numbers pointed into an unrelated glossary entry, and executing them as written would
    have destroyed correct material. **Verify the defect exists before fixing it.** If it
    duplicates, the section is deleted and replaced with a link, and Task 6.4 runs D5
    against `QueryPatterns.md` as well as against the split original. If it does not, Task
    6.4 treats it as ordinary content and `QueriesAndQueryObjects.md` stays nearer ~579
    lines than below it.

- [ ] **Task 3.2:** Fix `## How It Work` in `SweeperCircuitBreaking.md`
  - Input: `contents/SweeperCircuitBreaking.md:16`
  - Output: the heading reads `## How Sweeper Circuit Breaking Works`
  - Notes: A missing "s", single occurrence in the corpus. The heading is requalified in the
    same edit because rule 3a requires it, and the section stays in the core (design §7.7
    item 5). Grep for inbound links to `#how-it-work` before renaming — design §8 records
    none for this page, but re-derive rather than trust it.

- [ ] **Task 3.3:** Fold `HowServiceActivatorWorks.md:147` into
      `DispatcherConfigurationReference.md`
  - Input: `contents/HowServiceActivatorWorks.md` `## Dispatcher Configuration` (76 lines);
    `contents/DispatcherConfigurationReference.md`
  - Output: the overlap removed from the explanation, a link in its place; anything the
    reference page lacks folded into it
  - Notes: Created by Spec 011's own Phase 6 split, which did not check the explanation page
    for overlapping configuration material. **No linter can see this** — it is why it is a
    task and not a rule. `worklist.md` §6e types the page **keep — but fold**: do not split
    the explanation. Run the D5 check over the union of the two pages, not over each
    separately, since this removes duplication rather than moving content.

- [ ] **Task 3.4:** Write the D5 no-information-loss check
  - Input: the method both Spec 011 demonstrator splits used — every substantive line of the
    original tested for verbatim presence across the resulting pages
  - Output: `spec/010-information_architecture/noloss.py`, taking an original at a git ref
    plus the resulting pages, and reporting every line that survives nowhere
  - Notes: **"I read the diff" is not this check** (`worklist.md` §4 rule 4). On the two
    demonstrators it returned 24 lines and 4 lines, and every one was a deliberate edit —
    that signature is what a clean run looks like, not zero. Deliberate removals are
    expected (folded guidance, repointed anchors, dropped duplicate prose), so the output is
    read, not merely exit-coded. **Prove it fails**: delete a line from a split result and
    confirm it is reported.

---

## Phase 4 — Scheduler family (PR 4)

**Goal:** `worklist.md` §5a executed — five Reference cores, one shared how-to, one enriched
overview. 6 rows, 2 new pages, 8 pages touched. Design §7.1.

- [ ] **Task 4.1:** Create `SwitchingSchedulers.md`
  - Input: the five `… Migration from Other Schedulers` sections — `HangfireScheduler.md`
    (36), `AwsScheduler.md` (52), `QuartzScheduler.md` (38), `AzureScheduler.md` (61),
    `InMemoryScheduler.md` (34) = **221 lines**
  - Output: `contents/SwitchingSchedulers.md`, How-to, **top-level in *Scheduler***
  - Notes: The five sections are near-copies — `HangfireScheduler.md:751` and
    `AwsScheduler.md:692` differ only in which factory they name — so the matrix collapses
    to one before/after per target under a shared preamble. Design §7.1 gives the outline:
    `## Why You Would Switch Schedulers`, `## Switching Schedulers: What Changes and What
    Does Not`, one **H3 per target**, `## Switching Schedulers Verification`,
    `## Further Reading`. **Top-level placement is forced by S3**, not preferred: all five
    donor pages already publish at three segments. Five sections merging into one page is
    the case where rule 3b (no repeated heading within a page) bites — the H3-per-target
    shape is what avoids it.

- [ ] **Task 4.2:** Create `SchedulingAMessage.md`
  - Input: `BrighterSchedulerSupport.md` `## Brighter Scheduler Code Examples` (167) and
    `## Brighter Scheduler Configuration Examples` (45) = **212 lines**
  - Output: `contents/SchedulingAMessage.md`, How-to, **top-level in *Scheduler***;
    `BrighterSchedulerSupport.md` 578 → 366
  - Notes: 212 lines of how-to on an Explanation page is the mode mix this row exists to
    fix. Prerequisites segment names `[Scheduler](/contents/BrighterSchedulerSupport.md)`.

- [ ] **Task 4.3:** Fold the four comparison sections into `## Choosing a Scheduler`
  - Input: `HangfireScheduler.md` `## Comparison: Hangfire vs Quartz` (26),
    `AwsScheduler.md` `## AWS Scheduler Comparison with Other Schedulers` (23),
    `AzureScheduler.md` `## Azure Scheduler Comparison with Other Schedulers` (24),
    `InMemoryScheduler.md` `## Comparison with Production Schedulers` (12) = **85 lines**;
    `BrighterSchedulerSupport.md` `## Choosing a Scheduler` (97)
  - Output: one merged `## Choosing a Scheduler`; `BrighterSchedulerSupport.md` lands at
    ~400
  - Notes: **Four sections, not six** — design §16 finding 2. `QuartzScheduler.md` has no
    comparison section at all, and `AwsScheduler.md`'s `## Scheduling Modes Comparison` (11
    lines) **stays in the AWS core**: it compares AWS's own two scheduling modes,
    direct-to-target against `FireAwsScheduler`, and is not a comparison between schedulers.
    `worklist.md` §5a and §6a say "six" and are wrong on the count for this reason; 011 is
    closed and was not re-opened for it. **They fold by merger, not concatenation** — the
    duplicate prose is dropped, which is the one place in this spec where text is removed
    rather than moved, and it is removed as duplication. **Run D5 against the union** of the
    four sections and `## Choosing a Scheduler`, not against each separately.

- [ ] **Task 4.4:** Requalify, re-banner and re-file the five cores
  - Input: `HangfireScheduler.md` (832 → ~770), `AwsScheduler.md` (775 → ~700),
    `QuartzScheduler.md` (769 → ~731), `AzureScheduler.md` (717 → ~632),
    `InMemoryScheduler.md` (541 → ~495)
  - Output: five Reference cores keeping their filenames; two `SUMMARY.md` entries added for
    Tasks 4.1–4.2
  - Notes: **`TickerQScheduler.md` is a `keep`** — no migration section at all,
    `Best Practices` is 8 lines, and splitting it would produce stubs. Do not touch it.
    `InMemoryScheduler.md` keeps `## Important Warning` (16) and `## When to Use InMemory
    Scheduler` (63): it is the scheduler you must not ship, and that is the page's most
    valuable content. It also **receives** `InMemoryOptions.md`'s `InMemory Scheduler`
    section (53 lines) — but that arrives in Task 5.4, so this page is edited by two PRs and
    ~495 is before that arrival. It is four lines under 500 by arithmetic alone; a banner
    and two lead-ins put it back over, and **that is fine** — design §7.8, page length is
    not an acceptance criterion for this spec.

---

## Phase 5 — Outbox and Inbox (PR 5)

**Goal:** 4 rows, **8** new pages — seven landing in *Outbox and Inbox*, plus
`InMemoryTransport.md` which files into *Transports* — and **24 of the 34 measured inbound
anchor links land here**, carrying **11 of the ≈19 repoints**. More than half the anchor cost
of all 26 splits sits in this phase on either measure. Design §7.4, §7.5, §8.

> **Both figures corrected at review.** This read *7 new pages* and *19 of the 34*. Nineteen
> is the **whole-spec repoint** total — the sum of design §8's *moves? yes* rows — and was
> attributed to this phase in three places. Tasks 5.1 and 5.2 carry 16 + 8 = **24** inbound
> links, of which 5 + 6 = **11** need repointing. The conclusion is untouched.

- [ ] **Task 5.1:** Split `BrighterOutboxSupport.md`
  - Input: `## Outbox Archiver` (151) and `## Complete Example: Transactional Messaging`
    (169); page is **517** lines
  - Output: `contents/OutboxArchiver.md` (Reference) and
    `contents/TransactionalMessagingWithTheOutbox.md` (How-to), both **top-level in *Outbox
    and Inbox***; `AzureBlobArchiveProvider.md` and `AzureBlobConfiguration.md` **re-parented
    under `OutboxArchiver.md`**; core → ~197
  - Notes: **This page is a split target *and* one of the four parent pages** §3.1 relies on
    for a middle navigation layer — both jobs happen to it at once, and **the core keeps its
    filename**, as both 011 demonstrators did. It carries the heaviest anchor load in the
    corpus: **16 inbound links across 5 anchors from 13 files.** Only three anchors move —
    `#outbox-archiver` (3 links) and
    `#running-the-sweeper-and-archiver-out-of-process` (1) → `OutboxArchiver.md`;
    `#complete-example-transactional-messaging` (1) →
    `TransactionalMessagingWithTheOutbox.md`. **`#implicit-clear` (9 links) and
    `#you-always-need-a-sweeper` (2) stay in the core** — do not move those sections.
    `OutboxArchiver.md` also receives `InMemoryOptions.md`'s `InMemory Archive` (47) in Task
    5.4. Spec 005 owns this page and closed at 14/14 having checked it against 010: a split
    relocates content and invalidates none of it.
    **The re-parenting is this task's second job and it carries a redirect obligation.**
    `AzureBlobArchiveProvider.md` and `AzureBlobConfiguration.md` land under
    `OutboxArchiver.md` — the archive provider belongs under the archiver, and the
    configuration child then publishes at **four segments**, which design §17 measured to
    work. They are **the only two pages in this spec whose URL moves twice** (PR 2, then
    here), so **add two redirect entries** for the intermediate paths
    `outbox-and-inbox/azureblobarchiveprovider` and
    `outbox-and-inbox/azureblobarchiveprovider/azureblobconfiguration`. Verify the additions
    with `--check-redirects` before merging, not after — the 30-day cache is unforgiving.

- [ ] **Task 5.2:** Split `ReplayOnSeen.md` — **Q5, the Explanation is the core**
  - Input: 1,039 lines in three clean modes (design §7.4): Explanation 228, How-to 469,
    Reference 311
  - Output: `contents/TurningOnReplayOnSeen.md` (How-to, 469) and
    `contents/ReplayOnSeenReference.md` (Reference, 311), both nested under
    `ReplayOnSeen.md`; core keeps the filename and is **retyped Reference → Explanation**
  - Notes: The load-bearing reason is not the mode balance — **`outbox-and-inbox/replayonseen`
    is one of the 36 URLs this restructure does not move**, and rule 1 keeping the core's
    filename is what preserves it. Making the how-to the core would move the concept's URL
    to a page about switching a flag on. **8 inbound anchor links across 5 anchors from 4
    files.** `#causation-id` (2) **stays** — Causation Id is Explanation. Moving:
    `#when-replay-does-not-fire` and `#store-support` (4 together) →
    `ReplayOnSeenReference.md`; `#upgrading-without-migrating` and
    `#replay-versus-replay-skipped` (2) → `TurningOnReplayOnSeen.md`. The banner retype is a
    `pagetypes.tsv` edit plus a re-run of `apply_banners.py`, which **now preserves a
    Prerequisites segment** (`5498cd6`) — it used to strip them.

- [ ] **Task 5.3:** Split `SweeperCircuitBreaking.md`
  - Input: `## Usage Patterns` (61) and `## Advanced Scenarios` (71) = 132; page is **527**
  - Output: `contents/UsingSweeperCircuitBreaking.md` (How-to), nested under
    `SweeperCircuitBreaking.md`; core → ~395
  - Notes: **It splits once, not twice** — design §7.7 item 5. The row implies a third page,
    an Explanation from `Overview` (11) and `How It Work` (29); **40 lines is a stub**, and
    `worklist.md` §6a's own TickerQ ruling is the precedent — *the family shape does not
    oblige a split where the sections are empty*. Those two sections are the reference
    page's necessary preamble and stay. The typo is already fixed in Task 3.2.

- [ ] **Task 5.4:** Redistribute `InMemoryOptions.md` — three new pages, two donations
  - Input: `## InMemory Transport` (118), `## InMemory Outbox` (79), `## InMemory Inbox`
    (68), `## InMemory Scheduler` (53), `## InMemory Archive` (47); page is **695**
  - Output: `contents/InMemoryTransport.md` (Reference, **top-level in *Transports***),
    `contents/InMemoryOutbox.md` (Reference, nested under `BrighterOutboxSupport.md`),
    `contents/InMemoryInbox.md` (Reference, nested under `BrighterInboxSupport.md`); the
    scheduler section merged into the existing `InMemoryScheduler.md`; the archive section
    merged into `OutboxArchiver.md` from Task 5.1
  - Notes: **This is a redistribution, not a mode split** — five unrelated subjects, each
    belonging beside its own family. Creating three of the five family pages is design §7.7
    item 4, a deviation from the shape column's *"merge each into the matching family
    page"*, and the reason is that **three of those family pages do not exist**: every other
    transport has one (5), every other outbox store has one (8), every other inbox store has
    one (6). InMemory is the missing member of each set, and a reader who wants the
    in-memory outbox has no page to find. `InMemoryTransport.md` is top-level because every
    other transport is; the other two nest at three segments. **Task 5.1 must land before
    the archive donation**, and Task 4.4 before the scheduler donation if phase 4 is being
    done — if it is not, the scheduler section merges into `InMemoryScheduler.md` as it
    stands today.

- [ ] **Task 5.5:** Retype the `InMemoryOptions.md` core and repoint its five inbound links
  - Input: what remains — `Test Configuration Patterns` (42), `Complete Testing Example`
    (99), `Environment-Specific Configuration` (114); page → ~330
  - Output: core keeps its filename and its *Brighter Configuration* slot, **retyped
    Reference → How-to**; five inbound links repointed
  - Notes: What is left is a genuine testing guide, which is why the type changes. **The
    retype is a `pagetypes.tsv` edit plus a re-run of `apply_banners.py`**, as in Task 5.2 —
    standing obligation 7. The five
    links are from `ShowMeTheCode.md`, `FAQ.md`, `V10MigrationGuide.md`, `Glossary.md` and
    `SUMMARY.md` — **re-derive them rather than trusting this list**, and check whether each
    wants the testing guide or one of the three new pages. This page was the corpus's worst
    within-page heading duplicate (12 instances, fixed in 011), and the catalogue shape is
    why.

---

## Phase 6 — Darker (PR 6)

**Goal:** 5 rows, 12 new pages — the largest phase. `worklist.md` §5b executed: Darker
splits along the seams Brighter already has, restoring the parallel. Design §7.3.

**Binding constraint for every task in this phase: Darker content is re-filed and split,
never rewritten.** `../Darker` HEAD is `4.1.1-7-g2f76cda`, ahead of the deployed 4.1.1, and
the site publishes the deployed version. Do not update behaviour from that working tree.

- [ ] **Task 6.1:** Split `QueryPatterns.md` — the largest page in the corpus
  - Input: 1,291 lines; `Parameterized Query Patterns` (270), `Pagination Patterns` (233),
    `Projection Patterns` (173), `Collection and Aggregation Patterns` (162),
    `Entity Framework Core Integration` (147)
  - Output: `ParameterizedQueryPatterns.md`, `PaginationQueryPatterns.md`,
    `ProjectionQueryPatterns.md`, `AggregationQueryPatterns.md`, `EFCoreQueryIntegration.md`
    — all How-to, all nested under `QueryPatterns.md`; the hub → ~300
  - Notes: Six independent task-shaped recipes in one file; the score of 2 badly understates
    it. **The hub is not a stub**: it keeps the introduction, `Performance Best Practices`
    (66), `Real-World Example: Product Catalog Query` (197), `Best Practices Summary` and
    `Common Pitfalls`. Guidance folds into what it concerns (`worklist.md` §4 rule 3), and
    the performance material is cross-cutting so it belongs to the hub rather than to any
    one recipe. Five new pages sharing a parent is where rule 3a bites hardest — every `##`
    must be qualified by *its own* page's subject, not by "Query Patterns".

- [ ] **Task 6.2:** Split `ImplementAQueryHandler.md`
  - Input: 935 lines; `Testing Query Handlers` (159), `Working with Dependencies` (130)
  - Output: `TestingQueryHandlers.md` and `QueryHandlerDependencies.md`, both How-to, nested
    under `ImplementAQueryHandler.md`; core → ~646
  - Notes: Testing a handler has no business inside "implement a handler". The core stays
    large at ~646 and that is deliberate — three handler patterns (311 lines) plus
    registration and error handling, and splitting further separates a reader from the thing
    they are implementing (design §7.8).

- [ ] **Task 6.3:** Split `QueryPipeline.md`
  - Input: 928 lines; `Configuring Polly Policies` (159), `Comparison with Brighter
    Pipeline` (53)
  - Output: `QueryPipelinePolicies.md` (How-to) and `DarkerAndBrighterPipelines.md`
    (Explanation), nested under `QueryPipeline.md`; core → ~716
  - Notes: This is §5b's finding executed — Brighter decomposes the same subject across
    `BuildingAPipeline.md`, `PolicyRetryAndCircuitBreaker.md` and `PolicyFallback.md` while
    Darker keeps it in one file. **The core keeps `Available Decorators` (291) and
    `Decorator Patterns` (162) because its worklist row says so**; extracting the decorator
    reference is recorded in design §11 as a candidate for a later spec and is **not** acted
    on here. `DarkerAndBrighterPipelines.md` is thin at 53 lines and that is within the
    corpus's floor — `OutboxPattern.md` is 45 and `AzureBlobArchiveProvider.md` is 42.

- [ ] **Task 6.4:** Split `QueriesAndQueryObjects.md` — **gated by Task 3.1**
  - Input: 877 lines; `Query Result Types` (179), `Validation in Query Objects` (119); and
    Task 3.1's finding on `## Query Patterns` (102)
  - Output: `QueryResultTypes.md` (Explanation) and `QueryObjectValidation.md` (How-to),
    nested under `QueriesAndQueryObjects.md`; core → ~579, or lower if 3.1 found duplication
  - Notes: **Do not start this task before Task 3.1 returns.** If it found duplication, the
    `## Query Patterns` section is deleted and replaced with a link to `QueryPatterns.md`,
    and D5 runs against `QueryPatterns.md` as well as against this original. If it did not,
    the section stays and the core lands nearer ~579. Either outcome is valid; neither
    blocks the two extractions, which are independent of it.

- [ ] **Task 6.5:** Split `DarkerBasicConfiguration.md`
  - Input: 510 lines; `Darker Configuration Options` (75)
  - Output: `DarkerConfigurationReference.md` (Reference), nested under
    `DarkerBasicConfiguration.md`; core → ~435
  - Notes: Created **for the parallel, not for the size** — 75 lines against
    `DispatcherConfigurationReference.md`'s 233. It inherits the
    `BrighterBasicConfiguration.md` shape, and the core keeps `Quick Start`,
    `Using IQueryProcessor` and `Common Configuration Patterns`. **One inbound anchor link
    moves**: `#query-processor-lifetime` → `DarkerConfigurationReference.md`.

---

## Phase 7 — Using an External Bus (PR 7)

**Goal:** 4 rows, 3 new pages, and **the one piece of new prose this entire spec authors**.
Design §7.5, `worklist.md` §5c.

- [ ] **Task 7.1:** Create `MessageTransforms.md` — §5c, and it carries a correctness fix
  - Input: `MessageMappers.md` `## Transformers` (**119**) + `DefaultMessageMappers.md`
    `## Transform Pipeline Example` (145) = **264 lines**
  - Output: `contents/MessageTransforms.md` (Explanation), nested under `MessageMappers.md`;
    `MessageMappers.md` 266 → ~147; `DefaultMessageMappers.md` 478 → ~333
  - Notes: **This page must state that transforms require a custom mapper** and form part of
    the pipeline. The ruling was explicit that this is not currently clear, and a reader can
    today come away believing transforms work with the default mapper. **That sentence is
    the only new prose in this spec** — everything else is moved verbatim. Note the span:
    `## Transformers` is **119** lines, not 120; it is the last `## ` section on the page and
    so inherited the counting artefact design §16 finding 1 corrected. **Three inbound
    anchor links move**: `MessageMappers.md#message-transformer-factory` →
    `MessageTransforms.md`. It nests under `MessageMappers.md` (depth 2) and not under
    `DefaultMessageMappers.md`, which is already at three segments — **file under the
    shallowest source**.

- [ ] **Task 7.2:** Establish `DefaultMessageMappers.md` as the default route — §5c row 1
  - Input: `MessageMappers.md`; `DefaultMessageMappers.md` (**478** lines, already typed
    How-to)
  - Output: a link and a pointer from `MessageMappers.md`; **no new page**
  - Notes: §5c calls for a "default mapper how-to" and it **already exists**. This row is
    *establish it as the default route*, not *write it*. `worklist.md` §8 lists "how to use
    the default mapper" among the four missing how-tos for Spec 013 — this task is why that
    one is already covered. **`## Configuration Reference` (54) stays in
    `DefaultMessageMappers.md`** (design §7.7 item 3): it is the how-to's own configuration
    table, and splitting it would produce a stub.

- [ ] **Task 7.3:** Split `CloudEventsSupport.md`
  - Input: 475 lines; `CloudEvents Attributes` (34), `CloudEvents Across Transports` (72) =
    106
  - Output: `CloudEventsReference.md` (Reference), nested under `CloudEventsSupport.md`;
    core → ~369
  - Notes: `worklist.md` calls this the **highest-confidence row in the file** — the shape is
    already ruled, not proposed. The How-to core keeps the name; the required/optional/
    extension attribute tables and the per-transport matrix are consulted rather than
    followed, so they become Reference.

- [ ] **Task 7.4:** Split `DynamicMessageDeserialization.md`
  - Input: 597 lines; `Using CloudEvents Type for Routing` (83), `Custom Routing Strategies`
    (63), `Handler Routing` (77), `Configuration Examples` (93) = **316**
  - Output: `RoutingMultipleMessageTypes.md` (How-to), nested under
    `DynamicMessageDeserialization.md`; core → ~281
  - Notes: A how-to is known to be missing here — *how to route several message types down
    one channel* — and **the material for it is already on the page**, so this is an
    extraction and not new writing (`worklist.md` §8 flags it as extractable for exactly
    this reason). The Explanation core keeps `DataType Channel Pattern`,
    `Dynamic Message Deserialization`, `Performance Considerations` and `Comparison`. Four
    sections merging into one page: watch rule 3b.

---

## Phase 8 — Transports (PR 8)

**Goal:** 2 rows, 2 new pages. Design §7.2. (`InMemoryTransport.md` files into this section
but is created in Task 5.4, with the rest of the `InMemoryOptions.md` redistribution.)

- [ ] **Task 8.1:** Split `AWSSQSConfiguration.md` — the exact RabbitMQ precedent
  - Input: **615** lines; `V10 Migration Path` (196), `AWS SDK v4 Support` (49) = **245**
  - Output: `AWSSQSMigrateToV10.md` (How-to), nested under `AWSSQSConfiguration.md`; core →
    ~370, keeping its name and its four `SQS *` sections
  - Notes: 245 lines of how-to at the end of a reference page — the same shape as
    `RabbitMQConfiguration.md`, which is why `worklist.md` calls it the exact precedent.
    **615 is one of the three line counts that were right all along**: this file has no
    trailing newline, so the old convention happened to agree with `splitlines()` on it.
    **Three inbound anchor links move**: `#migrating-from-aws-sdk-v3-to-v4` →
    `AWSSQSMigrateToV10.md`. This page also carries 16 using-less C# blocks, one of which
    was Spec 011's probe for proving `--changed` works at block granularity — expect
    warnings, not errors, on any block that marks its omission `// ...`.

- [ ] **Task 8.2:** Split `PostgreSQLMessageBroker.md`
  - Input: 662 lines; `Benefits` (22), `When to Use` (19), `Limitations` (21),
    `JSON vs JSONB` (32), `Comparison with Other Transports` (16) = **110**
  - Output: `PostgreSQLBrokerTradeOffs.md` (Explanation), nested under
    `PostgreSQLMessageBroker.md`; core → ~552
  - Notes: **`## Transactional Messaging` (46 lines) stays in the core** — design §7.7 item
    2. Extracting it would produce a 46-line how-to that Spec 013 immediately supersedes:
    the publicly committed *PostgreSQL for both transport and outbox* guide is exactly this
    material, written properly. **Flag it to 013; do not half-extract it here.** The core
    stays over 500 at ~552 because producer (80) and consumer (81) configuration belong side
    by side. **This page is a transport, not a scheduler** — Spec 011's classification notes
    listed it as the seventh member of the scheduler family; it merely shares the template.

---

## Phase 9 — The rest of §6d (PR 9)

**Goal:** 5 rows, 5 new pages, plus one navigational fix design §11 records. Design §7.5.

- [ ] **Task 9.1:** Split `CQRSWithBrighterAndDarker.md`
  - Input: 1,144 lines; `Use Cases and Patterns` (209)
  - Output: `CQRSUseCasesAndPatterns.md` (Explanation), nested under
    `CQRSWithBrighterAndDarker.md`; core → ~935
  - Notes: **The core keeps `## Example: E-Commerce Order System` (226 lines).**
    Requirements §8 is binding — 010 must not consume material Spec 009 needs, and 226 lines
    of end-to-end example is the closest thing the corpus has to a tutorial. The page drops
    to ~709 when 009 takes it. **Flagged, not moved.** ~935 is the largest page after this
    spec completes and it is deliberate.

- [ ] **Task 9.2:** Split `NullableReferenceTypes.md`
  - Input: 711 lines; `Migration Guide` (264) — more than a third of the page
  - Output: `MigratingToNullableReferenceTypes.md` (How-to), nested under
    `NullableReferenceTypes.md`; core → ~447
  - Notes: **The type ruling already contains the split** — the page was typed Reference on
    the grounds that "the migration steps are not where the durable value is". The ruling and
    the split agree, which is why this row is low-risk.

- [ ] **Task 9.3:** Split `AgreementDispatcher.md`
  - Input: 720 lines; `Standard vs Agreement Dispatcher Routing` (74), `Use Cases` (146),
    `Limitations` (53), `Performance Implications` (47) = **320**
  - Output: `AgreementDispatcherRouting.md` (Explanation), nested under
    `AgreementDispatcher.md`; core → ~400
  - Notes: Typed How-to because "the registration syntax is the point of the page, not the
    pattern discussion around it" — which again names the split. The core keeps
    `Registration Syntax`, `Synchronous and Asynchronous Registration` and
    `Complete Example`. Four sections merging into one page: watch rule 3b.

- [ ] **Task 9.4:** Split `PolicyRetryAndCircuitBreaker.md`
  - Input: 687 lines; `Migration Guide: V9 to V10` (96) and `Legacy: Using Polly v7 Policies
    (Deprecated)` (**151**) = 247
  - Output: `MigratingToPollyV8.md` (How-to), **nested under
    `PolicyRetryAndCircuitBreaker.md`**, publishing at four segments; core → ~440
  - Notes: **This placement was the reason S3 got measured.** The page publishes at
    `commands-handlers-and-pipelines/buildingapipeline/policyretryandcircuitbreaker/migratingtopollyv8`,
    and until 2026-08-08 S3's ceiling of three would have forced it to be a *sibling of its
    own source* — the obviously wrong shape, on the strength of an untested assumption.
    Design §17 has the measurement. **One page holds both tails**: the deprecated
    Polly v7 section is what a reader migrates *from*, and splitting them apart would leave a
    deprecated 151-line page with no explanation of what replaces it. Note the span — the
    legacy section is **151** lines, not 152; it is the last `## ` on the page and inherited
    the counting artefact. **One inbound anchor link moves**: `#migration-guide-v9-to-v10` →
    `MigratingToPollyV8.md`. The core keeps `All Available Polly v8 Strategies` (244) as its
    reference table.

- [ ] **Task 9.5:** Split `Telemetry.md`
  - Input: 597 lines; `Configuring OpenTelemetry` (81), `Complete Configuration Example`
    (96), `Distributed Tracing Example` (30) = **207**
  - Output: `ConfiguringOpenTelemetry.md` (How-to), nested under `Telemetry.md`; core → ~390
  - Notes: The Reference core keeps the per-component span tables — Command Processor,
    Dispatcher, Outbox, Inbox and Transform Pipeline tracing. **Two inbound anchor links,
    neither of which moves**: `#configurable-instrumentation` and `#inbox-tracing` both stay
    in the core. Verify that before moving anything, not after.

- [ ] **Task 9.6:** Make the `HandlerFailure.md` ↔ `ErrorHandlingOptions.md` relationship
      navigational
  - Input: both pages; `worklist.md` §6e; design §11
  - Output: `ErrorHandlingOptions.md` nested under `HandlerFailure.md` in `SUMMARY.md`
    (already so in `SUMMARY.target.md`), the missing reverse pointer added, and a
    *Prerequisites* segment in `ErrorHandlingOptions.md`'s banner
  - Notes: **They are not merged.** They are the corpus's best existing explanation/reference
    pair, and merging them yields ~685 lines carrying two modes — which is what everything
    else in this spec is pulling apart. Today only `ErrorHandlingOptions.md` points at
    `HandlerFailure.md`. This is a one-line fix plus a banner segment; it is in this PR
    because it is the only phase that touches neither page's body substantially.

---

## Phase 10 — `llms.txt` and LLM-facing delivery (PR 10)

**Goal:** D6 — but D6 must be re-scoped before it is built, because **the platform already
does most of it.** See §3 for what was measured on 2026-08-08. Investigate, rule, then
implement whatever the ruling leaves.

- [ ] **Task 10.1:** Establish how a page description reaches GitBook's generated `llms.txt`
  - Input: §3's measurements; GitBook's *LLM-ready docs* and *Content configuration* pages
    (fetch the `.md` variants); the GitBook dashboard for this site
  - Output: a written finding — the mechanism, or a statement that there is none for a
    Git-synced space
  - Notes: **The format `CLAUDE.md` specifies is already achievable in the platform's own
    file** — GitBook's own `llms.txt` renders `- [Title](url): description` on 294 of its 668
    entries, which is exactly `- [Title](path): Type — one sentence.` minus the type. Ours
    renders **zero**. The question is only how a description is set. Front matter was ruled
    out for the *banner* in 011 on the strength of GitbookIO/gitbook#1079, and **that ruling
    was about rendering metadata into the page body, not about `description`** — quote the
    whole rule, not the sentence that scared you. `Content configuration` does not document
    front matter at all, so the answer may be the dashboard, or may not exist. **If a
    description can be set, the one-sentence-per-page work lands at the canonical
    `/llms.txt` URL instead of in a GitHub-only file, and D6 mostly dissolves.**

- [ ] **Task 10.2:** Establish whether the V9 space can be excluded from the generated index
  - Input: the GitBook dashboard; §3's measurement
  - Output: a written finding, and a fix if one exists
  - Notes: **This is the more consequential half.** Our `/llms.txt` lists **170** entries:
    our 111, plus **59 from a separate *V9 Paramore Brighter Documentation* space, with no
    discriminator between them.** Spot-checked, a V10 page's `.md` opens with our banner —
    `> **Reference** · Applies to **Brighter V10**` — and **a V9 page has no banner at all.**
    So the banner is doing its job on our pages while the index quietly undermines it, which
    is the exact failure the banner exists to prevent, one level up from where 011 defended
    against it. **Hiding pages will not help**: GitBook documents that hidden pages remain in
    `llms-full.txt` and in the MCP server. If the space cannot be excluded, say so and record
    it as a known limitation rather than working around it silently.

- [ ] **Task 10.3:** Rule on D6's scope — **maintainer's call**
  - Input: Tasks 10.1 and 10.2's findings; `design.md` §9.2; requirements P1-1, D6 and **AC9**
  - Output: a ruling, and **AC9 amended to match if D6 narrows**
  - Notes: The live options are *fix it at source* (descriptions on pages, V9 excluded — the
    platform's file becomes the deliverable), *build ours anyway* as a typed, V10-only,
    IA-sectioned file at the repository root, accepting it is **not** at the canonical URL
    because GitBook owns `/llms.txt` and we cannot override it, or *drop the generator and
    keep the sentence rule* (Task 10.4). **AC9 as written — "`llms.txt` covers every page
    with type and one-line summary, generated not hand-written" — is already satisfied in
    part by the platform**, and if D6 narrows, AC9 must narrow with it rather than being
    quietly reinterpreted.

- [ ] **Task 10.4:** Keep Q9's opening-sentence rule, wherever the index ends up
  - Input: `design.md` §9.2
  - Output: the check implemented — as `tools/llmstxt.py`'s validator if D6 survives Task
    10.3, otherwise as a new `pagelint.py` rule
  - Notes: **This part of Q9 is worth having regardless of who generates the index.** A
    page's first sentence after the banner must exist, be under 200 characters, not end in a
    colon, and be unique across pages. **Expect a double-figure number of failures on the
    first run; that is the check working, not a defect in it.** A page whose opening sentence
    does not survive being read alone has a bad opening sentence, and fixing it improves the
    page for every reader — and now, demonstrably, for every retrieval client too, because
    that sentence is what GitBook's `.md` variant leads with. Fixes are one sentence each.
    If it lands as a pagelint rule, add it to `CLAUDE.md`'s ledger **and** the linter in the
    same commit — AC5 of Spec 011 failed on exactly that parity gap.

---

## Phase 11 — Close (PR 11)

**Goal:** D8, the two chores carried over from 011, and the acceptance pass.

- [ ] **Task 11.1:** D8 — per-term links from `BasicConcepts.md` into `Glossary.md`
  - Input: `BasicConcepts.md`'s 24 terms; `Glossary.md`'s 100
  - Output: each of the 24 linked to its matching `Glossary.md` anchor
  - Notes: **There are zero such links today.** This replaces the withdrawn
    `BasicConcepts.md` → `Glossary.md` merge and is purely additive. The separation is
    deliberate and was ruled by the maintainer: `BasicConcepts.md` is a curated orientation
    set a newcomer can read *without* working through the 100-term glossary. **Do not
    re-open the merge.** Every link carries an `#anchor`, which puts all 24 under
    `linkcheck.py`'s MISSING ANCHOR check — that is the point of doing it this way.

- [ ] **Task 11.2:** P2-1 — normalise the 17 files with no trailing newline
  - Input: the 17 files (re-derive the list; it was 18 of 105 before the Phase 6 splits)
  - Output: every file under `contents/` ends with a newline
  - Notes: Deliberately deferred from 011 so the banner diff contained nothing but banners.
    It lands here for the same reason in reverse — a whitespace-only sweep is safe once no
    content PR is in flight. **Re-derive the count**; it has moved once already.

- [ ] **Task 11.3:** P2-3 — echo the changed-range count from `docs.yml`
  - Input: `.github/workflows/docs.yml`
  - Output: the `--changed` step prints how many files and hunks it examined
  - Notes: `pagelint.py --changed` reporting 0 errors is indistinguishable from a run that
    found no ranges to be strict about. It was real on PR #74 — 118 files, 465 hunks — but
    the only way to know was to check locally and to force the gate red on purpose. This
    makes a run prove its own non-vacuity instead of relying on a note in `PROMPT.md`.

- [ ] **Task 11.4:** The acceptance pass — AC1 through AC9
  - Input: `design.md` §14; `requirements.md` §12
  - Output: a § *Acceptance pass as executed* appended to this file, one row per criterion
    with the evidence
  - Notes: **Walk each criterion; do not infer it from a green build.** Spec 011's own
    acceptance pass failed AC5 and found `NO H1` missing from `CLAUDE.md`'s ledger — a rule
    that had never fired once, invisible to everything but an enumeration. **To check
    parity, enumerate; do not read.** AC7 is per-split: record which splits landed, and
    **partial completion is a valid end state** — the spec is accepted on the splits it
    landed, not blocked on the ones it did not. **Check `pagetypes.tsv` parity by enumeration**
    — one row per page under `contents/`, every banner *type* matching its `verdict` and every
    banner *version* matching its `applies`. Session 9 measured 110/110 on both by hand and no
    tool checks either, so an unfixed standing obligation 7 is invisible here unless it is
    walked. AC8 requires all **16** `keep` rows honoured,
    naming **16 distinct pages** — see §2's third finding, and note that requirements §12 and
    §14 say fifteen. Verify by set comparison against `worklist.md`, not by eye.

- [ ] **Task 11.5:** The final gate
  - Input: the whole tree
  - Output: recorded results for five checks
  - Notes: `python3 tools/linkcheck.py` — **this is the task the skill's own checklist asks
    for**, and the orphan check is what enforces "never create orphaned files".
    `python3 tools/pagelint.py` at 0 errors. `tools/urlmap.py --check-shape` and
    `--check-redirects` green. And `--verify` against the live site **after** the last merge,
    remembering it exits 2 rather than passing if the site is unreachable. Confirm
    `BoxProvisioning.md#when-to-use-box-provisioning` still resolves — Spec 009's rung 3
    links to it, `linkcheck.py` catches a break and redirects cannot.

---

## Appendix A — Where each of the 32 new pages nests

> **RATIFIED 2026-08-08 (Task 1.1).** Reviewed placement by placement. With S3 measured,
> **27 of the 32 are derivations** — each nests under its single source per design §6.1.
> Five were decisions, all confirmed as written: `OutboxArchiver.md` and
> `TransactionalMessagingWithTheOutbox.md` **top-level**, with `AzureBlobArchiveProvider.md`
> and its configuration child re-parented beneath the archiver (the URL of those two moving
> twice was raised and accepted); `InMemoryTransport.md` **top-level in *Transports***,
> because every other transport is; and `SchedulingAMessage.md` and `SwitchingSchedulers.md`
> **both top-level in *Scheduler***, giving four entries as design §7.6 intended — the
> alternative buries the section's two task-shaped pages beneath six technology pages.
> `MessageTransforms.md` **stays under `MessageMappers.md`**, which S3 used to force and is
> now a choice: the page exists to state that **transforms require a custom mapper**, so
> filing it beneath the *default* mapper page would undercut the correction it carries.

**This is the item design §15 named as not measured, and Task 1.1 ratifies it.** The
placements below follow from design §6.1 (*a split page sits beside the page it came from*)
and S3, **now ≤4 segments, measured** — see design §17 and §2's first finding.

**The rule, where a page has more than one source:** file it under the **shallowest**. Where
it has no single source, it goes **top-level in its section**. Both are editorial choices
now, not S3 workarounds; the ceiling only binds at five.

| # | New page | Nests under | Section | Depth | Task |
|---:|---|---|---|---:|---|
| 1 | `MigratingToPollyV8.md` | `PolicyRetryAndCircuitBreaker.md` | Commands, Handlers and Pipelines | **4** | 9.4 |
| 2 | `AgreementDispatcherRouting.md` | `AgreementDispatcher.md` | Commands, Handlers and Pipelines | 3 | 9.3 |
| 3 | `MessageTransforms.md` | `MessageMappers.md` | Using an External Bus | 3 | 7.1 |
| 4 | `CloudEventsReference.md` | `CloudEventsSupport.md` | Using an External Bus | 3 | 7.3 |
| 5 | `RoutingMultipleMessageTypes.md` | `DynamicMessageDeserialization.md` | Using an External Bus | 3 | 7.4 |
| 6 | `InMemoryTransport.md` | *(top-level)* | Transports | 2 | 5.4 |
| 7 | `AWSSQSMigrateToV10.md` | `AWSSQSConfiguration.md` | Transports | 3 | 8.1 |
| 8 | `PostgreSQLBrokerTradeOffs.md` | `PostgreSQLMessageBroker.md` | Transports | 3 | 8.2 |
| 9 | `OutboxArchiver.md` | *(top-level)* | Outbox and Inbox | 2 | 5.1 |
| 10 | `TransactionalMessagingWithTheOutbox.md` | *(top-level)* | Outbox and Inbox | 2 | 5.1 |
| 11 | `InMemoryOutbox.md` | `BrighterOutboxSupport.md` | Outbox and Inbox | 3 | 5.4 |
| 12 | `InMemoryInbox.md` | `BrighterInboxSupport.md` | Outbox and Inbox | 3 | 5.4 |
| 13 | `TurningOnReplayOnSeen.md` | `ReplayOnSeen.md` | Outbox and Inbox | 3 | 5.2 |
| 14 | `ReplayOnSeenReference.md` | `ReplayOnSeen.md` | Outbox and Inbox | 3 | 5.2 |
| 15 | `UsingSweeperCircuitBreaking.md` | `SweeperCircuitBreaking.md` | Outbox and Inbox | 3 | 5.3 |
| 16 | `SwitchingSchedulers.md` | *(top-level — forced by S3)* | Scheduler | 2 | 4.1 |
| 17 | `SchedulingAMessage.md` | *(top-level)* | Scheduler | 2 | 4.2 |
| 18 | `ParameterizedQueryPatterns.md` | `QueryPatterns.md` | Darker | 3 | 6.1 |
| 19 | `PaginationQueryPatterns.md` | `QueryPatterns.md` | Darker | 3 | 6.1 |
| 20 | `ProjectionQueryPatterns.md` | `QueryPatterns.md` | Darker | 3 | 6.1 |
| 21 | `AggregationQueryPatterns.md` | `QueryPatterns.md` | Darker | 3 | 6.1 |
| 22 | `EFCoreQueryIntegration.md` | `QueryPatterns.md` | Darker | 3 | 6.1 |
| 23 | `TestingQueryHandlers.md` | `ImplementAQueryHandler.md` | Darker | 3 | 6.2 |
| 24 | `QueryHandlerDependencies.md` | `ImplementAQueryHandler.md` | Darker | 3 | 6.2 |
| 25 | `QueryPipelinePolicies.md` | `QueryPipeline.md` | Darker | 3 | 6.3 |
| 26 | `DarkerAndBrighterPipelines.md` | `QueryPipeline.md` | Darker | 3 | 6.3 |
| 27 | `QueryResultTypes.md` | `QueriesAndQueryObjects.md` | Darker | 3 | 6.4 |
| 28 | `QueryObjectValidation.md` | `QueriesAndQueryObjects.md` | Darker | 3 | 6.4 |
| 29 | `DarkerConfigurationReference.md` | `DarkerBasicConfiguration.md` | Darker | 3 | 6.5 |
| 30 | `ConfiguringOpenTelemetry.md` | `Telemetry.md` | Health Checks and Observability | 3 | 9.5 |
| 31 | `MigratingToNullableReferenceTypes.md` | `NullableReferenceTypes.md` | V10 Migration | 3 | 9.2 |
| 32 | `CQRSUseCasesAndPatterns.md` | `CQRSWithBrighterAndDarker.md` | Understanding Brighter | 3 | 9.1 |

**32 pages. 5 top-level, 27 nested. Maximum depth 4 — S3 holds at its measured ceiling.**

**Two existing pages also re-parent**, in Task 5.1, and they are the only pages in the corpus
that move without being split:

| Existing page | Was (after PR 2) | Becomes (PR 5) | Depth |
|---|---|---|---:|
| `AzureBlobArchiveProvider.md` | top-level in *Outbox and Inbox* | under `OutboxArchiver.md` | 3 |
| `AzureBlobConfiguration.md` | under `AzureBlobArchiveProvider.md` | unchanged parent, one deeper | **4** |

They are **the only two pages whose URL moves twice**, and PR 5 owes a redirect entry for
each intermediate path.

### What the depth measurement changed

Every split source page's published depth in `SUMMARY.target.md`, via `urlmap.py`.
**Seven** of the 26 sources sit **at three segments**, and under the old S3 ceiling of 3
nothing could nest beneath them:

| Source page | Published path | Depth |
|---|---|---:|
| `PolicyRetryAndCircuitBreaker.md` | `commands-handlers-and-pipelines/buildingapipeline/policyretryandcircuitbreaker` | **3** |
| `DefaultMessageMappers.md` | `using-an-external-bus/messagemappers/defaultmessagemappers` | **3** |
| `HangfireScheduler.md` | `scheduler/brighterschedulersupport/hangfirescheduler` | **3** |
| `QuartzScheduler.md` | `scheduler/brighterschedulersupport/quartzscheduler` | **3** |
| `AwsScheduler.md` | `scheduler/brighterschedulersupport/awsscheduler` | **3** |
| `AzureScheduler.md` | `scheduler/brighterschedulersupport/azurescheduler` | **3** |
| `InMemoryScheduler.md` | `scheduler/brighterschedulersupport/inmemoryscheduler` | **3** |
| *the other 19 sources* | | 2 |

> **Corrected at review: this said "Six sources" and "the other 20", while listing seven
> rows.** Enumerated against `SUMMARY.target.md`, it is **7 at three segments and 19 at two**,
> summing to the 26 split sources. Six plus twenty also sums to 26, which is why nothing ever
> looked wrong — the pair was internally consistent and both halves were off by one. Design
> §16 finding 3 was the same shape: prose contradicting the table beneath it. **To check a
> table, enumerate it; do not read its introduction.**

**That ceiling was an assumption, and measuring it dissolved the problem.** Design §17 has
the evidence: GitBook's own documentation publishes 30 pages at four segments, and PRs
#83/#84 established the same for this site — a new page at a path that had never existed, so
no automatic redirect could mask the result, reverted minutes later with the tree left
byte-identical to `c4aedb5`.

Where each placement stands now:

- **`MigratingToPollyV8.md` nests under its own source** (Task 9.4), at four segments. Under
  the old ceiling it would have been a *sibling of the page it was extracted from*.
- **`SwitchingSchedulers.md` stays top-level** in *Scheduler* (Task 4.1) — not because of
  S3, but because it draws from all five scheduler leaves and has no single parent.
- **`MessageTransforms.md` stays under `MessageMappers.md`** (Task 7.1) rather than under
  `DefaultMessageMappers.md`, which donates 145 of its 264 lines. Now an editorial choice —
  transforms belong with mappers generally — where it used to be forced.

Reproduce the depths. **Before PR 2 lands, point both paths at the files that exist** —
`spec/010-information_architecture/urlmap.py` and `SUMMARY.target.md` — because Task 2.1 has
not yet moved the tool and Task 2.4 has not yet installed the tree. The form below is the one
that works *after* PR 2, when `SUMMARY.md` **is** the target tree:

```bash
python3 - <<'PY'
import importlib.util
s = importlib.util.spec_from_file_location("urlmap", "tools/urlmap.py")
m = importlib.util.module_from_spec(s); s.loader.exec_module(m)
paths = m.published_paths(open("SUMMARY.md").read())
for path, f in sorted(paths.items(), key=lambda kv: -kv[0].count("/")):
    print(path.count("/") + 1, path, f)
PY
```

### Top-level entries after all 32 land

| Section | Pages | Entries | §7.6 intended |
|---|---:|---:|---:|
| Get Started | 3 | 3 | 3 |
| Commands, Handlers and Pipelines | 17 | 5 | 5 |
| Brighter Configuration | 6 | 4 | 4 |
| Using an External Bus | 15 | 9 | 9 |
| Transports | 12 | 7 | 7 |
| **Outbox and Inbox** | 39 | **9** | **10** |
| Scheduler | 10 | 4 | 4 |
| Darker | 17 | 5 | 5 |
| Health Checks and Observability | 5 | 4 | 4 |
| V10 Migration | 3 | 2 | 2 |
| Understanding Brighter | 13 | 10 | 10 |
| Reference | 2 | 2 | 2 |

**S1 ✅** minimum 2 · **S2 ✅** maximum **10**, ceiling 12, two entries of headroom ·
**S3 ✅** maximum depth **4**, ceiling 4, reached by exactly two pages —
`AzureBlobConfiguration.md` and `MigratingToPollyV8.md`.

**Eleven of the twelve rows reproduce design §7.6 exactly.** *Outbox and Inbox* comes out at
**9**: eight today, plus `OutboxArchiver.md` and `TransactionalMessagingWithTheOutbox.md`
top-level, minus `AzureBlobArchiveProvider.md`, which re-parents under the archiver. Design
§15 flagged that column as an intention rather than a measurement and named the breach case
— *"if all seven landed top-level it would show 15 and breach S2's ceiling of 12"*. They do
not; the answer is 9, three under the ceiling. **No verdict changes, and the one threshold
that moved, moved because it was measured.** `--check-shape` (Task 2.2) keeps all three rules
true from PR 2 onward rather than leaving them to be re-derived.

---

## Appendix B — The anchor obligations, by task

Design §8 measured **34 inbound anchor links across 7 of the 26 split pages; 19 pages have
none**, and **≈19 of the 34 actually need repointing** because most linked anchors sit in
sections that stay in the core. That table is a snapshot taken before the work — **re-derive
per split**, because the splits themselves add links.

| Task | Page | Links | Repoint | Stay |
|---|---|---:|---:|---|
| 5.1 | `BrighterOutboxSupport.md` | 16 | 5 | `#implicit-clear` (9), `#you-always-need-a-sweeper` (2) |
| 5.2 | `ReplayOnSeen.md` | 8 | 6 | `#causation-id` (2) |
| 8.1 | `AWSSQSConfiguration.md` | 3 | 3 | — |
| 7.1 | `MessageMappers.md` | 3 | 3 | — |
| 9.5 | `Telemetry.md` | 2 | 0 | `#configurable-instrumentation`, `#inbox-tracing` |
| 6.5 | `DarkerBasicConfiguration.md` | 1 | 1 | — |
| 9.4 | `PolicyRetryAndCircuitBreaker.md` | 1 | 1 | — |
| — | the other 19 split pages | **0** | | |

**Phase 5 carries 24 of the 34 links and 11 of the ≈19 repoints** — more than half the anchor
cost of the whole spec sits in two tasks on either measure. For scale, the
`BrighterBasicConfiguration.md` split alone cost **28 repoints across 20 pages** in Spec 011;
all 26 splits here cost ≈19, because Spec 011 counted its anchors *during* the work and this
design counted them *before* it.

> **Corrected at review: this said "Phase 5 carries 19 of the 34".** Nineteen is the
> **whole-spec repoint** figure — the sum of design §8's *moves? yes* rows, and of the Repoint
> column in the table above — and it had been attributed to phase 5 in three places, here and
> twice in §1 and phase 5's own goal. The two numbers that describe phase 5 are **24** links
> and **11** repoints. Nothing about the conclusion changes: a figure that is right about the
> spec and wrong about the phase is the easiest kind to carry, because it reads true in both
> sentences.

**The standing obligation is `grep` before the move, not `linkcheck.py` after it.**
`linkcheck.py` catches a broken anchor, which is worth having — but redirects cannot fix a
fragment, so a missed anchor is a real 404 into the middle of a page, and the cheapest place
to catch it is before the heading moves.

---

## Appendix C — What each phase must not do

Collected because every one of these has cost a rework somewhere in this programme.

- **Do not re-open a `keep` row.** 16 rows naming 16 distinct pages, and AC8 requires all of
  them honoured. `KafkaConfiguration.md` is 608 lines of a single mode and is the standing
  reminder that size misleads.
- **Do not review against a 500-line limit.** `CLAUDE.md` says *"consider splitting"* — a
  prompt to think, not a threshold — and **sixteen pages stay over 500 after this spec
  completes**, seven of them `keep` rows. Design §7.8 lists all sixteen with the reason for
  each. **Mode mixing is the criterion.**
- **Do not shrink the four parent pages to stubs.** `BrighterOutboxSupport.md`,
  `BrighterInboxSupport.md`, `BrighterSchedulerSupport.md` and `DistributedLock.md` hang the
  middle navigation layer, and GitBook offers no sub-group — a middle layer needs a real
  page, and that page has to earn its place with content (requirements §3.1).
- **Do not backfill `using` directives you have not checked.** The 802-block debt is Spec
  011's AC1 baseline and shrinks only as pages are genuinely edited. Splits move blocks; they
  do not improve them.
- **Do not rewrite Darker page content.** Re-filing and splitting are safe; `../Darker` HEAD
  is ahead of the deployed 4.1.1 and the site publishes the deployed version.
- **Do not consume Spec 009's material** — `CQRSWithBrighterAndDarker.md`'s 226-line worked
  example and `ShowMeTheCode.md`. Requirements §8 is binding.
- **Do not write new how-tos.** The splits *extract* how-tos that already exist inside larger
  pages. The four missing how-tos in `worklist.md` §8 and the PostgreSQL guide committed on
  #67 belong to Spec 013.
- **Do not add a *How To* section**, and do not create one for 013 either. A how-to lives
  beside its subject; a *Guides* section becomes worth its place at three or more genuinely
  cross-cutting guides, and that is 013's call (design §6.2).
- **Do not trust a green check to mean a check ran.** Prove `--check-shape`, `--check-redirects`
  and the D5 script red on purpose before trusting them green. `pagelint.py --changed`
  reporting 0 errors was indistinguishable from a vacuous run until someone forced it.
- **Do not re-derive a figure from memory — re-derive it from the corpus.** **Eighteen**
  figures in this programme have now been wrong — thirteen before this document, the
  `keep`-row page count above, and **four more found at this document's own review** (§4).
  Five trace to one wrong line-counting call, three to reading a total instead of enumerating
  it, and three to a number that was true of a neighbouring quantity. A page's length is
  `len(text.splitlines())`. **S3 is still the only *rule* to have moved.**
- **Do not let a page exist that `pagetypes.tsv` does not know about.** Standing obligation 7,
  and it was missing from this document until review. The TSV is what `apply_banners.py`
  reads, `pagelint.py` never reads it, and a green build therefore says nothing about it.
- **Do not inherit a threshold whose stated rationale is that nobody has tried more.**
  **S3 is the first *rule* in this programme to move** — every previous correction, all
  fourteen, was a tally. It was not wrong so much as over-restrictive: *"three is the deepest
  the live site is known to work at"* is a record of what had been tried, and it reads
  downstream as a finding about what is possible. It survived requirements, design and a
  design review unchallenged, and had already bent two placements out of shape before anyone
  asked what caused it. **When a rule's justification is the absence of evidence, that is a
  measurement waiting to be taken** — and here it cost two PRs and about five minutes.
