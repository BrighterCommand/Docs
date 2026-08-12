# Spec 010: Information Architecture — Tasks

**Created:** 2026-08-08 · **Status:** **REVIEWED AND APPROVED 2026-08-08** — five findings
applied in place, four tallies and one omission; see **§4**. No verdict, threshold, placement
or ruling moved.
**Works from:** `design.md` (approved 2026-08-08, `.design-approved`) and `requirements.md`
(approved 2026-08-06)
**Executes against:** `spec/011-authoring_conventions/worklist.md` (42 rows, 26 `split`, 16 `keep`)

**Total tasks: 52, across 11 phases. 37 done — Phases 1 through 8.**

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
| **4** | Scheduler family — 6 rows, 2 new pages · **DONE 2026-08-09** | 4 | D4 |
| **5** | Outbox and Inbox — 4 rows, **8** new pages, 24 of the 34 anchor links · **DONE 2026-08-09** | 5 | D4 |
| **6** | Darker — 5 rows, 12 new pages · **DONE 2026-08-11** | 5 | D4 |
| **7** | Using an External Bus — 4 rows, 3 new pages · **DONE 2026-08-11** | 4 | D4 |
| **8** | Transports — 2 rows, 2 new pages · **DONE 2026-08-12** | 2 | D4 |
| **9** | The rest of §6d — 5 rows, 5 new pages · **DONE 2026-08-12** | 6 | D4 |
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

- [x] **Task 1.3:** Merge PR #82
  - Input: PR #82 on `docs/spec-010-design`
  - Output: `design.md`, `tasks.md`, `SUMMARY.target.md` and the README amendments on
    `master`
  - Notes: `master` requires a review and GitHub blocks self-approval, so this is
    `gh pr merge 82 --merge --admin`. Touches no file under `contents/`, so linkcheck stays
    at 112 files and pagelint at 0 errors / 802 warnings.
  - **Done 2026-08-08.** Merged as **`25a578c`**. Six files, all under
    `spec/010-information_architecture/`: `design.md`, `tasks.md`, `SUMMARY.target.md`,
    `README.md` and the two approval markers. **All checks passed before the merge** — both CI
    `check` jobs, both `versions` jobs, and both GitBook checks. Baseline on `master`
    afterwards, unchanged: **linkcheck 112 files clean, pagelint 0 errors / 802 warnings across
    110 pages.** **Phase 1 is complete and PR 1 is closed.** This tick could only be recorded
    after the merge it describes, so it lands as the first commit of the phase-2 branch.

---

## Phase 2 — The tree (PR 2)

**Goal:** D1, D2 and D3 land together. `SUMMARY.md` becomes the twelve-section tree, 74
redirects ship with it, and `urlmap.py` moves to `tools/` with two new checks gating CI.
**No page body is touched by this phase at all** — that separation is what makes the splits
safe to interleave afterwards.

- [x] **Task 2.1:** Move `urlmap.py` to `tools/`
  - Input: `spec/010-information_architecture/urlmap.py` (validated 110/110 against the live
    sitemap, re-verified 2026-08-06)
  - Output: `tools/urlmap.py`; the spec copy deleted; `__pycache__` not committed
  - Notes: D3 is *packaging, not invention*. Keep `--verify`'s exit-2-on-unreachable
    behaviour exactly as it is — an unreachable authority is not a pass. Keep the tolerant
    `^\s*##\s+` section regex; it must model what GitBook does, not what the file ought to
    say.

- [x] **Task 2.2:** Add `--check-shape`
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

- [x] **Task 2.3:** Add `--check-redirects`
  - Input: `design.md` §9.1; requirements P0-3 and §2.4
  - Output: `tools/urlmap.py --check-redirects`, exit 0/1
  - Notes: Three assertions — every `redirects:` value resolves to a file that exists, every
    key is a path that no longer publishes, and **the whole of `.gitbook.yaml` is printable
    ASCII**. Parse the flat `key: value` block in ~15 lines of Python: PyYAML is absent from
    this environment and `ruby -ryaml` is an accident of the machine. A YAML parser would
    have parsed `​structure:` happily, which is the entire reason the byte check exists.

- [x] **Task 2.4:** Install the new `SUMMARY.md`
  - Input: `spec/010-information_architecture/SUMMARY.target.md` — 145 lines, 110 links, 12
    sections, pure ASCII, verified at design review
  - Output: `SUMMARY.md` replaced wholesale
  - Notes: This is D1 and it is a copy, not a rewrite — every figure in the design
    reproduces against that file, so retyping it would put them at risk for nothing. The
    encoded link `Requests%2C%20Commands%20and%20Events.md` stays: Q6 is dropped, and the
    awkward filename makes the better URL (§6.3).

- [x] **Task 2.5:** Generate and **type** the `redirects:` block
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

- [x] **Task 2.6:** Verify `.gitbook.yaml` mechanically — **AC4**
  - Input: the file as written by Task 2.5
  - Output: a recorded parse result and byte inspection, both clean
  - Notes: Malformed indentation disables redirects *silently* rather than erroring. Run
    `--check-redirects` and, independently, assert no byte outside printable ASCII anywhere
    in the file. **Before merge, never after** — see Task 2.8.

- [x] **Task 2.7:** Wire both checks into CI
  - Input: `.github/workflows/docs.yml`, the `check` job
  - Output: `--check-shape` and `--check-redirects` run on every push and PR
  - Notes: `--verify` stays **out** of CI: it depends on an external site and would make the
    build flaky. Q10's answer is yes for the two checks that read only the repository. A
    redirect block complete at merge and incomplete three PRs later is the same silent
    failure in slow motion, which is why these gate rather than being run once.

- [x] **Task 2.8:** The pre-merge gate
  - Input: the branch as it stands
  - Output: a recorded result for each of six checks
  - Notes: `linkcheck.py` clean; `pagelint.py` 0 errors; `--check-shape` green;
    `--check-redirects` green; the block has **74** entries, matching design §5's measured
    figure; and #67 re-checked for a reply (Task 1.2). **Redirects cache with
    `stale-while-revalidate=2592000` — thirty days — so a wrong redirect outlives its fix at
    the edge.** Also confirm no task in this PR requalified
    `BoxProvisioning.md#when-to-use-box-provisioning`; Spec 009's rung 3 links to it and
    redirects cannot fix a fragment.

- [x] **Task 2.9:** Post-merge live sample — **AC5b**
  - Input: the published site, 25–45 seconds after merge
  - Output: recorded status, `location:` header and body size for a sample of the 74
  - Notes: **Every cached response reports `200`**, genuine 404s and genuine redirects
    alike, so *status code alone is worthless on this site*. The tell is that no genuine
    page response carries a `location:` header; body size separates the rest — ~192 KB for a
    redirect, ~189.5 KB for the 404 shell, **584 KB for a real page**. Sample the sections
    that were renamed, including the 3-segment nested paths under *Transports*.

- [x] **Task 2.10:** Re-probe PR #77's old path
  - Input: `command-processors-and-dispatchers/commandscommanddispatcherandprocessor`
  - Output: a recorded observation of whether it still carries a `location:` header
  - Notes: **The one open platform unknown** — whether GitBook's automatic redirects
    *persist*. They may be tied to revision history, and one session could not test it. This
    is a measurement, not a gate: the `.gitbook.yaml` block ships regardless, because that
    is what it is for. If the header has gone, the block is the reason nothing broke.

### Tasks 2.8–2.10 as executed — 2026-08-08, PR #85 merged as `1bae048`

**PHASE 2 IS COMPLETE. D1, D2 and D3 are delivered and measured on the live site.**

**Task 2.8 — the pre-merge gate, six checks plus two.** All green: `linkcheck.py` clean at 112
files; `pagelint.py` 0 errors / 802 warnings, unchanged; `--check-shape` 0 failures;
`--check-redirects` 0 failures at 75 entries and 7,673 bytes; the block holding **75 entries,
74 of them new**; and #67 re-checked — still OPEN, `updatedAt` 2026-08-03, two comments, both
the maintainer's. Additionally `BoxProvisioning.md#when-to-use-box-provisioning` is untouched
and its one inbound link (from `BoxProvisioningConfiguration.md:13`) resolves.

> **One check reported a pass that proves nothing, and it says so.** `pagelint.py --changed
> origin/master` returned 0 errors, but this PR touches **no file under `contents/`**, so there
> were no C# blocks in the diff to be strict about. That is the correct result and it is
> **necessarily vacuous** — recorded rather than presented as evidence the gate works.

**Task 2.9 — AC5b. All 74 keys redirect, and the verdict took two passes to get right.**
Swept every key in the block:

| Response | Keys | Reading |
|---|---:|---|
| `307`, one `location:` header | **67** | cold cache — the true status shows through |
| `200`, one `location:` header | **7** | **warm cache** — still redirecting |
| anything with no `location:` header | **0** | — |

**The seven are exactly the paths sampled by hand minutes earlier**, which is what warmed them.
The sweep's own pass condition required `code == "307"` and therefore labelled seven correct
redirects `NOT REDIRECTING`. **The data was right and the verdict was wrong**, which is the trap
requirements §16 documents — *every cached response reports 200, genuine 404s and genuine
redirects alike; the reliable tell is the `location:` header.* The tool had the tell and the
condition ignored it.

The three fingerprint classes all reproduced, and one number has moved:

| Path class | Status (cold) | `location:` | Body |
|---|---|---:|---|
| key in the block | **307** | 1 | ~192 KB |
| never existed, not in the block | **404** | 0 | **189,511 bytes** |
| a real page in the new tree | **200** | 0 | **786 KB – 1.1 MB** |

The 404 shell matches the recorded ~189.5 KB to the byte. **A real page is no longer ~584 KB**
— today it is 786 KB to 1.1 MB, so the *classes* still separate cleanly but the absolute
figures have drifted. Compare classes, not remembered sizes. Only **one** of the 74 keys is a
three-segment path (`guaranteed-at-least-once/azureblobarchiveprovider/azureblobconfiguration`)
— of the nine pages publishing three deep in the old tree, eight sat under sections whose names
deliberately did not change. `sitemap-pages.xml` stays at **111**, so no page was lost.

**Task 2.10 — the probe cannot answer its own question, and that is the finding.** PR #77's old
path still carries `location:` a day later, pointing at
`understanding-brighter/commandscommanddispatcherandprocessor`. But **that key is in the
`redirects:` block** — at line 17 since PR #78, and its post-#77 successor is line 49 among
today's 74. So a `location:` header cannot distinguish *automatic redirects persisting* from
*our own block firing*. D0's own lesson applies to the probe D0 designed: **never conclude "the
redirect works" from a single successful request; it proves only that something redirected.**

**What the probe does establish is better than what it was for.** The value at line 17 is the
**repository** path `contents/CommandsCommandDispatcherandProcessor.md`, and the `location:`
header names **today's** URL rather than yesterday's intermediate one. So a redirect entry
**re-resolved itself across a second move of the same page** — predicted by D0, now measured.
That is the property PR 5 depends on when it re-parents the two Azure Blob pages.

**Whether automatic redirects persist is therefore no longer answerable at this path, and no
longer load-bearing.** Answering it needs an old path that moved and is *absent* from the
block, and after PR 2 there is no such path — which is exactly what shipping the block
belt-and-braces was for. **Do not re-run this probe expecting an answer; record it as closed by
construction.**

### Tasks 2.1–2.7 as executed — 2026-08-08

**All seven done on `docs/spec-010-tree`. Tasks 2.8–2.10 remain**: 2.8 is the pre-merge gate,
and 2.9/2.10 can only run *after* the merge, against the published site.

**Task 2.1 — the move carried a silent bug, and it was one line.** `REPO` was
`Path(__file__).resolve().parents[2]`, correct from `spec/010-information_architecture/` and
**wrong from `tools/`**, where it resolves to the directory *above* the repository. Nothing
would have errored: every path still resolves, just against the wrong tree. Now `parents[1]`
with a comment saying why. `--verify`'s exit-2-on-unreachable and the tolerant `^\s*##\s+`
section regex are untouched.

**Task 2.2 — `--check-shape` proved itself red without a synthetic probe.** Run against the
**old** nineteen-section tree it reported **9 failures**, and every one is a real defect the
restructure exists to remove:

| Failure | Detail |
|---|---|
| S1 × 6 | the six singleton sections — `commands-processors-and-dispatchers`, `cqrs-patterns`, `darker-configuration`, `faq`, `reference`, `task-queues` |
| S2 × 2 | `brighter-request-handlers-and-middleware-pipelines` at **14** entries, `outbox-and-inbox` at **20** |
| P0-1 × 1 | `SUMMARY.md:154` — ` ## Under the Hood`, the leading space |

Against the new tree: **0 failures — 110 pages, 12 sections, deepest 3 of 4 segments, widest
10 of 12 top-level entries.** Both figures are design's: PR 2's own tree never needs the
fourth segment, and 10 is the maximum Appendix A predicted. **S3 was then forced red on
purpose** — a page nested to five segments reported `S3: … publishes at 5 segments` and exit 1,
and the file was restored byte-identical. The four-segment case passes, which is the ceiling
measured in design §17.

**Task 2.3 — `--check-redirects`, and all three assertions forced red.** The first attempt at
the middle one **was vacuous and said it passed**: the mutation targeted `contents/Routing.md`,
which is not in the block, so it changed nothing and the check reported 0 failures. Redone
against a value actually present, with an assertion that the mutation lands before the result
is read. *A check that passes has not necessarily checked anything* applies to the test as much
as to the tool.

| Assertion | Forced by | Result |
|---|---|---|
| every byte printable ASCII | U+200B inserted into `structure:` — the exact historical bug | exit 1, `byte 16 is 0xe2` |
| value resolves to a real file | a present value repointed at `contents/NoSuchPage.md` | exit 1, named the line |
| key no longer publishes | a live key appended (`get-started/showmethecode`) | exit 1, *"can never fire"* |

**Task 2.4 — `SUMMARY.md` is byte-identical to `SUMMARY.target.md`** (`cmp` clean), 145 lines,
110 links, 12 sections, no non-ASCII. A copy, not a retyping, so every figure in the design
still reproduces against the installed file.

**Task 2.5 — 74 entries, and the block now holds 75.** `--redirects` emitted exactly **74 of
110 pages moved**, matching design §5. **The seventy-fifth is D0's own entry** for #77's
section rename, already in the file since PR #78 — so *Task 2.8's gate should expect 75 in the
file and 74 new*, which is the kind of off-by-one that reads as a defect when it is not.

> **On "type the block; never paste it".** The entries were **generated by `tools/urlmap.py`
> and appended programmatically**, not pasted from GitBook's documented example — which is
> where this repo's two U+200B characters came from and which still carries them today. That
> honours the hazard the instruction is about. Hand-typing 74 paths would have traded an
> invisible-character risk for a larger typo risk, and the block is asserted on bytes either
> way.

**Task 2.6 — verified by two independent parsers, before merge.** `--check-redirects` reports
**0 failures, 75 entries, 7,673 bytes, all printable ASCII**. Independently, `ruby -ryaml`
reads three top-level keys (`root`, `structure`, `redirects`), **`structure:` resolving as a
real key** rather than the U+200B impostor, 75 string values, 0 leading slashes and 0 values
outside `contents/`. Two parsers agreeing is the point; our own parser alone would only prove
self-consistency.

**Task 2.7 — both checks are in `docs.yml`'s `check` job**, seven steps now. `--verify` stays
out deliberately: it fetches the live sitemap, and a check that goes red because the site was
slow teaches people to ignore red builds.

---

## Phase 3 — Defects, the verification, and the harness (PR 3)

**Goal:** the three `worklist.md` §7 content defects (D9 / P1-4), and the D5 check that
every split in phases 4–9 depends on.

- [x] **Task 3.1:** **Verify** the `QueriesAndQueryObjects.md:746` ↔ `QueryPatterns.md`
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

- [x] **Task 3.2:** Fix `## How It Work` in `SweeperCircuitBreaking.md`
  - Input: `contents/SweeperCircuitBreaking.md:16`
  - Output: the heading reads `## How Sweeper Circuit Breaking Works`
  - Notes: A missing "s", single occurrence in the corpus. The heading is requalified in the
    same edit because rule 3a requires it, and the section stays in the core (design §7.7
    item 5). Grep for inbound links to `#how-it-work` before renaming — design §8 records
    none for this page, but re-derive rather than trust it.

- [x] **Task 3.3:** Fold `HowServiceActivatorWorks.md:147` into
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

- [x] **Task 3.4:** Write the D5 no-information-loss check
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

### Phase 3 as executed — 2026-08-08

**PHASE 3 IS COMPLETE. D9 and D5 are delivered.** 17 of 52 tasks done. Three pages touched,
one tool added; no page created, no URL moved, `SUMMARY.md` untouched. All four gates green
and unchanged in shape: linkcheck 112 files, `--check-shape` 0, `--check-redirects` 0 at 75
entries. **Pagelint's debt falls 802 → 797**, which is exactly the five C# blocks Task 3.3
removed — `contents/HowServiceActivatorWorks.md` goes from 22 ` ```csharp ` fences to 17.

**Task 3.1 — NOT DUPLICATE, and that is the passing outcome.** So the `## Query Patterns`
section stays where it is, Task 6.4 does **not** delete it, D5 does **not** need to run
against `QueryPatterns.md`, and `QueriesAndQueryObjects.md` lands at **~579** (877 − 179 −
119) rather than lower.

The two sections pair one-to-one by subject and share no prose at all:

| `QueriesAndQueryObjects.md` §`## Query Patterns` | `QueryPatterns.md` |
|---|---|
| `### Pattern: Pagination Query` (23) — `GetOrdersPageQuery` alone | `### Pattern: Offset-Based Pagination` (125) — the same class **plus** `PagedResult<T>`, `OrderDto`, an EF Core handler, when-to-use and trade-offs. `### Pattern: Cursor-Based Pagination` (106) has no counterpart at all |
| `### Pattern: Search Query` (26) — `SearchProductsQuery`, constructor parameters | `### Pattern: Search with Multiple Criteria` (102) — same class name, **init-only properties**, different fields (`NameFilter`, `CategoryId`, `InStock`), plus `ProductDto` and a handler |
| `### Pattern: Projection Query` (25) — `GetCustomerSummaryQuery` + `CustomerSummary` | `### Pattern: Simple Projection` (53) — `GetCustomerSummariesQuery` (**plural, no parameters**) + `CustomerSummaryDto` + a handler |
| `### Pattern: Aggregation Query` (26) — `GetSalesStatisticsQuery` + `SalesStatistics` (4 fields) | `### Pattern: Summary/Statistics` (70) — same query class, `SalesStatisticsDto` with **5** fields, plus a handler |

Measured rather than judged: of the section's **58 substantive lines, 32 (55%) appear
verbatim somewhere in `QueryPatterns.md`** — and **every one of the 32 is a code fragment or
a fence** (`PageNumber = pageNumber;`, `public int PageSize { get; }`, ` ```csharp `). **Not
one prose line is shared.** A 55% line overlap that is 0% prose overlap is the signature of
two pages using the same worked domain, not of one copied from the other.

The relationship is **level of detail**: the page about query objects shows the query object
*shape*; the page about patterns shows the end-to-end recipe, handler and data access
included. That is the relationship a Reference page has with a How-to, and deleting either
side would lose material.

> **Recorded, deliberately not acted on: the two pages disagree about the same code.**
> `SalesStatistics` against `SalesStatisticsDto`, `CustomerSummary` against
> `CustomerSummaryDto`, and `SearchProductsQuery` declared with constructor parameters on one
> page and init-only properties on the other. Same names, different definitions. That is a
> divergence risk and a candidate for a later spec; it is **not** duplication, and fixing it
> is barred here by *move the text, do not improve it* and by Appendix C's rule against
> rewriting Darker content.

**Task 3.2 — done, and the anchor check re-derived rather than trusted.** `## How It Work` →
`## How Sweeper Circuit Breaking Works`, which is the corpus's house form (`## How Box
Provisioning Works`, `## How Default Mappers Work`, `## How Request Validation Works`) and
collides with no existing `##`. Design §8 records no inbound links for this page; re-derived
across `contents/`, `SUMMARY.md` **and** `.gitbook.yaml`, `#how-it-work` appears **zero**
times, and the string `How It Work` no longer appears anywhere in the corpus. The page stays
at 527 lines — a heading rename is one line in, one line out.

**Task 3.3 — the fold, and the duplicate copy was also wrong.** This is the finding of the
phase. The removed block configured subscriptions with

```csharp
options.AddSubscription<MyCommand>(new Subscription<MyCommand>(...));
```

and **`AddSubscription` does not exist.** `ConsumersOptions` (`src/Paramore.Brighter.Service
Activator.Extensions.DependencyInjection/ConsumersOptions.cs`) exposes `Subscriptions`, an
`IEnumerable<Subscription>`; the only `AddSubscription` in the source is
`Dispatcher.AddSubscriptionToSubscriptions`, which is **private**. The reference page's form
is the correct one and matches the working sample at
`samples/TaskQueue/RMQTaskQueueWithDLQ/GreetingsReceiverConsole/Program.cs:80` —
`options.Subscriptions = subscriptions;`. **So the overlap was not merely redundant: a reader
who followed the explanation page would not have compiled.** Duplication rots asymmetrically,
and the copy that rots is the one nobody consults for parameters.

What actually moved, which is less than the 76 lines suggests:

| | |
|---|---|
| `HowServiceActivatorWorks.md` | **486 → 416**, not 410: the 76-line section is replaced by a 6-line pointer, so the delta is **−70** |
| `DispatcherConfigurationReference.md` | **233 → 233** — both folds extend existing bullets, so no line is added |
| Folded in | the **TimeOut trade-off pair** (shorter = responsive to shutdown, higher CPU; longer = lower CPU, slower shutdown), and the **ordering caveat on multiple Performers**. The reference noted ordering loss only under Proactor |
| Not folded in | the `AddSubscription` block (does not compile), and the `messagePumpType` / `requeueCount` material, both of which the reference already covers more fully — `RequeueCount` there also points at [Handler Failure](/contents/HandlerFailure.md) |
| Anchors | **zero** inbound links to `#dispatcher-configuration` or to any of the five sub-headings, across `contents/`, `SUMMARY.md` and `.gitbook.yaml`. Every inbound link to this page targets the page, not a fragment. No repoint |

**D5 over the union returned 40 lines, and every one is accounted for**: 15 for the
non-compiling `AddSubscription` block, 5 sub-headings, 7 `noOfPerformers` lines (3 folded
into the reference, the rest restatement), 4 `messagePumpType`, 5 `timeOut` (2 folded), 3
`requeueCount`, and **2 in the reference page itself** — the two bullets whose old, shorter
text no longer exists verbatim because the fold extended them. The check names both, with
their replacements as the nearest surviving line, which is the fold showing up as what it is.

**Task 3.4 — `noloss.py`, and calibrating it produced a finding.** It takes originals at a
git ref and results in the working tree (or at `--result-ref`), and reports every substantive
line surviving nowhere. Both sides accept several paths, because Tasks 3.3 and 4.3 fold rather
than move and their check is the union.

Three design points, all in the docstring: a line is substantive if it holds an alphanumeric
character; presence is set membership, not multiplicity; and **a heading is compared by the
anchor it produces**, via `linkcheck.py`'s own `slug()`, imported rather than copied.
That last is not a softening — a section that becomes a page promotes its headings and may
shed their bold, and `slug()` ignores both, so the anchor survives and nothing is lost.
Requalifying a heading under rule 3a **does** change the slug and **is** reported, which is
exactly the class of change standing obligation 3 exists to catch. On the first demonstrator
it accounts for 7 of 53 lines; the other 20 heading lines are genuine requalifications and
are still reported.

> **The calibration finding: 011's recorded 24 and 4 were the *prose* lines.** Run against
> the two demonstrators (`758f391`, `df2d3a5`) this tool returns **46 and 11**, and
> reproduces both recorded figures exactly as subsets — the 23 folded `Best Practices` items
> plus one repointed anchor on the first, the three stranded links plus the fold's lead-in on
> the second. The extra 22 and 7 are 20 requalified headings, 2 lines merged into surviving
> sentences, 4 structural labels dropped in a fold, and 3 lines of the two `Quick Migration
> Guide` blocks — the latter visible in 011's own debt figure falling 804 → 802.
> **Nothing here contradicts 011**: every extra line is documented somewhere in its write-up,
> just not in the D5 count. **A figure that was never wrong can still not be the whole
> answer** — and the lines it did not cover are the anchor breaks, which is the half you would
> choose to keep if you could only keep one.

**Proved red four ways, with the mutation asserted before each result was read** — session
15's first lesson applied:

| Probe | Result |
|---|---|
| identity, page against itself at `HEAD` | **0 of 92** — no false positives |
| one substantive line deleted | reported, exit 1; file restored byte-identical |
| heading promoted a level *and* made bold | **not** reported, counted as *matched on its anchor alone* |
| heading requalified (`## What are Quorum Queues?` → `## What RabbitMQ Quorum Queues Are`) | **reported** — the allowance is not a hole |

Argument handling exits 2 for a missing `--ref`, a missing `--result`, a path absent at the
ref, and a path given before either flag.

> **A restore step ate an uncommitted edit.** The `pagelint --changed` probe below finished
> with `git checkout -- <page>`, which restores from `HEAD` — and the page also held Task
> 3.3's fold, uncommitted. The probe's own "restored byte-identical" assertion caught it by
> failing, and the edit was redone and re-verified against the same D5 figure of 40.
> **`git checkout --` is not an undo for a probe applied on top of uncommitted work**; copy
> the file aside and restore from the copy.

**`pagelint.py --changed` is *still* vacuous here, and the handover predicted otherwise.**
`PROMPT.md` said Phase 3 is the first PR where it stops being vacuous, on the reasoning that
Phase 2 touched no file under `contents/`. Three pages under `contents/` are touched now, and
the run still reports 0 errors **necessarily** — because the rule is strict per *block
overlapping the diff*, and this diff is five single prose lines:

```text
contents/DispatcherConfigurationReference.md: [(59, 59), (64, 64)]
contents/HowServiceActivatorWorks.md:         [(149, 149), (151, 151)]
contents/SweeperCircuitBreaking.md:           [(16, 16)]
```

No C# block overlaps any of them. **File granularity was the wrong unit for the prediction**;
Q4's design is block granularity, deliberately, so that fixing a typo on a 700-line page
obliges nothing. The gate was confirmed live by probe rather than inferred from a pass: a
`using`-less C# block inserted into the changed page reported
`HowServiceActivatorWorks.md:153: USING DIRECTIVES` as an **error** under `--changed` and a
**warning** repo-wide. **`--changed` first bites for real in Phase 4**, which is the first
phase to create a page — and a new page is 100% added lines, so all of its blocks are strict.

---

## Phase 4 — Scheduler family (PR 4)

**Goal:** `worklist.md` §5a executed — five Reference cores, one shared how-to, one enriched
overview. 6 rows, 2 new pages, 8 pages touched. Design §7.1.

- [x] **Task 4.1:** Create `SwitchingSchedulers.md`
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

- [x] **Task 4.2:** Create `SchedulingAMessage.md`
  - Input: `BrighterSchedulerSupport.md` `## Brighter Scheduler Code Examples` (167) and
    `## Brighter Scheduler Configuration Examples` (45) = **212 lines**
  - Output: `contents/SchedulingAMessage.md`, How-to, **top-level in *Scheduler***;
    `BrighterSchedulerSupport.md` 578 → 366
  - Notes: 212 lines of how-to on an Explanation page is the mode mix this row exists to
    fix. Prerequisites segment names `[Scheduler](/contents/BrighterSchedulerSupport.md)`.

- [x] **Task 4.3:** Fold the four comparison sections into `## Choosing a Scheduler`
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

- [x] **Task 4.4:** Requalify, re-banner and re-file the five cores
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

### Phase 4 as executed — 2026-08-09

**PHASE 4 IS COMPLETE. 21 of 52 tasks done.** Two pages created, eight touched,
`TickerQScheduler.md` and `CustomScheduler.md` untouched. **No URL moved** — the five cores
keep their filenames and the two new pages are new paths — so `.gitbook.yaml` is unchanged at
75 entries and this phase owes no redirect.

All five gates: linkcheck **114 files**, pagelint **0 errors / 793 warnings**, `--check-shape`
0 at **112 pages / 12 sections / deepest 3 / widest 10**, `--check-redirects` 0,
`pagelint --changed origin/master` **0 errors and, for the first time, non-vacuous**.

**Every predicted line count reproduced exactly**, which is a first for a split phase in this
programme. Each core is design's figure **+1**, and the +1 is the same line on all five — the
`Switching Schedulers` entry added to *Related Documentation*:

| Page | Design | Actual | |
|---|---:|---:|---|
| `HangfireScheduler.md` | ~770 | **771** | 832 − 36 migration − 26 comparison + 1 link |
| `AwsScheduler.md` | ~700 | **701** | 775 − 52 − 23 + 1 |
| `QuartzScheduler.md` | ~731 | **732** | 769 − 38 + 1 (no comparison section) |
| `AzureScheduler.md` | ~632 | **633** | 717 − 61 − 24 + 1 |
| `InMemoryScheduler.md` | ~495 | **496** | 541 − 34 − 12 + 1 |
| `BrighterSchedulerSupport.md` | ~400 | **391** | 578 − 212 out, +25 net in after the merge |
| `SwitchingSchedulers.md` | — | **166** | from 221 donated lines |
| `SchedulingAMessage.md` | — | **233** | from 212 donated lines |

Design §7.1's four measured spans — 221 migration, 85 comparison, 212 how-to, 97
`## Choosing a Scheduler` — all reproduced against the corpus before anything was cut.

#### The gate finally bit, and it needed the files staged to do it

`PROMPT.md` predicted `pagelint.py --changed` would stop being vacuous in Phase 4, because a
new page is 100% added lines. It does — **but not until the new files are added to the
index.** `changed_ranges()` shells out to `git diff --unified=0 <base> --`, and **`git diff`
cannot see untracked files**. The first `--changed` run here reported 0 errors while the two
new pages contributed **no strict ranges at all** — a vacuous pass of exactly the kind session
15's lesson warns about, one phase after the same prediction already failed once on file
granularity.

Staged, the run is real: **16 blocks strict** — 9 on `SchedulingAMessage.md`, 7 on
`SwitchingSchedulers.md` — every one reporting the *elided* message, which is the `// ...`
downgrade behaving as Q4 designed it.

Proved red, with the mutation asserted and the file restored from a copy rather than from
`HEAD` (session 16's lesson):

| Probe | Result |
|---|---|
| one `// ...` removed from a new page's block | **`SwitchingSchedulers.md:52: USING DIRECTIVES`** — an *error*, exit **1** |
| the same page, repo-wide | the same block, a **warning** |
| restore from the copy | **byte-identical**; back to 0 errors |

**This is the first real error `--changed` has produced on a real page** in this programme. It
was also nearly missed a second way: the probe's first grep looked for the literal word
`error` in the output, and findings do not carry it — an error is the *absence* of the
`(warning)` suffix. Exit code 1 was the only honest signal. **Grep for the finding, not for
the word you expect to describe it.**

#### D5: 104 lines, all six groups accounted for

Tasks 4.1, 4.2 and 4.3 interlock across the same six pages — 4.3 folds *from* the pages 4.1
drains — so the check is **one invocation over the union**, not three. Running them separately
would report each folded line as lost, which is the noise `noloss.py`'s docstring warns trains
you to skim.

```bash
python3 spec/010-information_architecture/noloss.py --ref $(git merge-base origin/master HEAD) \
  --original contents/BrighterSchedulerSupport.md contents/HangfireScheduler.md \
             contents/AwsScheduler.md contents/QuartzScheduler.md \
             contents/AzureScheduler.md contents/InMemoryScheduler.md \
  --result   <the same six> contents/SwitchingSchedulers.md contents/SchedulingAMessage.md
```

**104 of 2,480 substantive lines survive nowhere.** Per page: Hangfire 25, AWS 21, Azure 20,
InMemory 18, `BrighterSchedulerSupport` 11, Quartz 9 — and the six groups sum to 104 exactly:

| | Lines | What |
|---|---:|---|
| Table rows superseded | **49** | the four incoming comparison tables and the old `### Scheduler Comparison`, replaced by one merged table |
| Comparison headings | **4** | the four `## … Comparison …` headings the fold removes |
| Choose/when-to-use bullets | **9** | folded into `### Scheduler Recommendations`, duplicates dropped |
| Corrected claims | **3** | see below |
| Requalified `##` headings | **2** | `## Brighter Scheduler Code Examples` → `## Message Scheduling Code Examples`, and the configuration pair |
| Migration sections collapsed | **38** | five near-copies → one shared preamble plus one *after* per target |

Nothing in that list is a surprise, and nothing is unexplained. The nearest-surviving-line
column earns its place here: `| **Strong Naming** | ✅ | ✅ | ✅ | ❌ | ✅ |` against the merged
`| **Strong Naming** | ✅ | ❌ | ✅ | ✅ | ✅ |` at 0.90 is the column reordering, visible at a
glance and impossible to see in a diff of two tables with different column counts.

**One row was dropped as genuine duplication and is worth naming:** Hangfire's
`| **Monitoring** | ✅ Excellent (dashboard) | ⚠️ Limited |`. For those two schedulers it
restates the `Dashboard` row, and no source states a value for the other three.

#### The finding of the phase: the general page had the facts wrong, again

Task 4.3 is a *merger*, so where the five tables disagreed about the same cell the merge
forced a choice. Checked against `../Brighter` HEAD rather than adjudicated between the pages:

| | Cancel | Reschedule |
|---|---|---|
| `AwsScheduler` (`MessageScheduler.Aws`, `.AWS.V4`) | `DeleteScheduleAsync` | `GetScheduleAsync` + `UpdateScheduleAsync` |
| `AzureServiceBusScheduler` | `CancelScheduledMessageAsync` | **`=> Task.FromResult(false)`** — the only implementation that does not reschedule |
| `QuartzScheduler` | `scheduler.DeleteJob` | `scheduler.RescheduleJob` |
| `HangfireMessageScheduler` | `client.Delete` | `client.Reschedule` |
| `InMemoryScheduler` | removes and disposes the timer | `Timer.Change` |

**No implementation throws `NotImplementedException` or `NotSupportedException`.** So
cancellation is universal, and Azure's missing reschedule is the single real limitation.

`BrighterSchedulerSupport.md`'s `## Choosing a Scheduler` said **AWS `Limited`/`Limited` and
Azure `No`/`No`** in its table, *"Limited cancellation/reschedule support"* in the AWS bullets
and *"No direct cancellation support"* in the Azure bullets. The four technology pages'
own tables said ✅ and were right. **This is Task 3.3's lesson repeating exactly one phase
later: duplication rots asymmetrically, and the copy that rots is the general one nobody
consults for a specific answer.** Three claims corrected, all of them forced by the merge.

**The same wrong claim travelled into a moved page, and that one was corrected against the
rule.** `## Brighter Scheduler Code Examples` carried *"AWS Scheduler and Azure Service Bus
Scheduler have limited or no cancellation support"*, which moves verbatim to
`SchedulingAMessage.md` under standing obligation 1. It was rewritten instead. **The line
drawn, so the next phase can apply it:** a merge that forces a choice between conflicting
copies resolves to the source; a page that would otherwise ship, in this PR, a sentence
contradicting a table in the same PR is corrected; **everything else moves verbatim and is
recorded.**

#### Recorded, deliberately not acted on

Three defects found while merging that no merge forced a choice about. Phase 3's precedent
(the `QueriesAndQueryObjects.md` / `QueryPatterns.md` code disagreement) applies: record, do
not fix, because *move the text, do not improve it* is what keeps a restructure PR reviewable.

- **`QuartzMessageSchedulerFactory` does not exist.** The source has
  `QuartzSchedulerFactory(IScheduler scheduler)`. The non-existent name appears in
  `InMemoryScheduler.md`'s migration section — dropped by the collapse — **and in
  `BrighterSchedulerSupport.md`'s `## Brighter Scheduler Configuration Examples`**, which
  moved verbatim to `SchedulingAMessage.md` and still carries it.
- **`HangfireMessageSchedulerFactory` takes no constructor arguments.** It exposes `Queue`,
  `Client` and `TimeProvider` as settable properties. Both
  `new HangfireMessageSchedulerFactory(connectionString: …)` forms in the corpus do not
  compile; one is on the moved page.
- **`UseScheduler` is an extension on `IBrighterBuilder`, not `IServiceCollection`.** Bare
  `services.UseScheduler(…)` appears in the Hangfire and Quartz migration sections — where
  the collapse forced a choice, so the merged page uses the `AddBrighter(…).UseScheduler(…)`
  chain the AWS, Azure and InMemory sections already used. It survives elsewhere in the
  corpus; `noloss.py` surfaced `builder.Services.UseScheduler(provider =>` as a nearest
  surviving line, so `QuartzScheduler.md` still has one outside the split.

Also unchanged: `IMessageScheduler` in the cancellation example (the interfaces are
`IAmAMessageSchedulerSync`/`Async`), and **TickerQ is absent from every comparison table in
the corpus**, including the merged one. Adding it would be new content.

#### Two deviations from design §7.1's outline

1. **Four target H3s, not five.** The outline reads *"Switching to Hangfire / Quartz / Aws /
   Azure / InMemory"*. There is no source section for switching **to** InMemory — its own
   section, `## Migration to Production Scheduler`, documents switching **away**, and is
   therefore the *before* half every other target shares. Same shape as design §16 finding 2,
   where matching on a word gave six comparison sections and enumerating gave four: **the
   outline listed five schedulers, not five sections.** The page's shared preamble carries
   both before-forms (from InMemory, from Quartz) and each H3 carries only the after.
2. **No `## Switching Schedulers Verification`.** The outline calls for it; **no source
   section supplies any material for it**, and writing one is new prose, which standing
   obligation 1 reserves to Task 7.1. Omitted rather than invented. A later spec that writes
   verification steps for a scheduler swap should add it.

A third, smaller: `## Running Two Schedulers During a Transition` is **added** to the outline,
because Hangfire's `### From Quartz` and Quartz's `### From Hangfire` are mirror-image
feature-flag blocks with real content and nowhere else to go. They collapse to one block with
*"Reverse the condition to migrate the other way."*

#### Obligations 3 and 7

**Obligation 3, re-derived not trusted:** *before* the split, across `contents/`, `SUMMARY.md`
and `.gitbook.yaml`, **zero** fragment links targeted any of the six scheduler pages. Appendix
B predicts 0 for all of them and it held — but it was re-derived, and it takes thirty seconds:

```bash
grep -rEo '\([^)]*(Hangfire|Aws|Quartz|Azure|InMemory|BrighterScheduler)[^)]*#[^)]*\)' contents/ SUMMARY.md .gitbook.yaml
```

**Run it after the split and it returns two** — `SwitchingSchedulers.md` and
`SchedulingAMessage.md` each link to
`BrighterSchedulerSupport.md#choosing-a-scheduler`, and both were created by this phase.
`## Choosing a Scheduler` is a heading the fold *keeps*, so both resolve, and `linkcheck.py`
confirms it. Recorded because the naive present-tense claim — *"zero fragment links target
these pages"* — was written first and was already false by the time it was written.
**Appendix B's warning that the table is a snapshot cuts both ways: the splits add links as
well as break them.**

**Obligation 7:** `pagetypes.tsv` is **112 rows plus header**, up from 110. Both new pages are
`How-to` / `How-to` / `Brighter V10`, appended rather than re-sorted.

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

- [x] **Task 5.1:** Split `BrighterOutboxSupport.md`
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

- [x] **Task 5.2:** Split `ReplayOnSeen.md` — **Q5, the Explanation is the core**
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

- [x] **Task 5.3:** Split `SweeperCircuitBreaking.md`
  - Input: `## Usage Patterns` (61) and `## Advanced Scenarios` (71) = 132; page is **527**
  - Output: `contents/UsingSweeperCircuitBreaking.md` (How-to), nested under
    `SweeperCircuitBreaking.md`; core → ~395
  - Notes: **It splits once, not twice** — design §7.7 item 5. The row implies a third page,
    an Explanation from `Overview` (11) and `How It Work` (29); **40 lines is a stub**, and
    `worklist.md` §6a's own TickerQ ruling is the precedent — *the family shape does not
    oblige a split where the sections are empty*. Those two sections are the reference
    page's necessary preamble and stay. The typo is already fixed in Task 3.2.

- [x] **Task 5.4:** Redistribute `InMemoryOptions.md` — three new pages, two donations
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

- [x] **Task 5.5:** Retype the `InMemoryOptions.md` core and repoint its five inbound links
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

### Phase 5 as executed — 2026-08-09

**PHASE 5 IS COMPLETE. 26 of 52 tasks done.** Eight pages created, ten touched. **Exactly two
URLs move** — `AzureBlobArchiveProvider.md` and `AzureBlobConfiguration.md`, the two pages this
spec always knew would move twice — and **both intermediate paths have a redirect entry**,
added and verified *before* merge. `.gitbook.yaml` 75 → **77** entries, 7,673 → 7,858 bytes,
all printable ASCII.

All five gates: linkcheck **122 files**, pagelint **0 errors / 791 warnings**, `--check-shape`
0 at **120 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at 77 entries,
`pagelint --changed origin/master` 0 errors over **109 strict blocks**.

**Three pinned figures reproduced exactly.** *Outbox and Inbox* lands at **9** top-level
entries, the number Appendix A pinned against design §7.6's intended 10.
`AzureBlobConfiguration.md` publishes at **four segments** — the first page in the corpus to
reach S3's measured ceiling, and Appendix A predicted exactly it. And **Appendix B held on
both counts**: 24 inbound anchor links across the phase's two heavy pages, of which **11 were
repointed** (5 in Task 5.1, 6 in Task 5.2) and 13 stayed.

| Page | Design | Actual | |
|---|---:|---:|---|
| `BrighterOutboxSupport.md` | ~197 | **264** | see the deviation below; +7 for *Further Reading* |
| `OutboxArchiver.md` | — | **148** | 91 archiver lines + the 47-line InMemory Archive donation |
| `TransactionalMessagingWithTheOutbox.md` | — | **179** | from 169 donated |
| `ReplayOnSeen.md` | 259 | **263** | +4 for two *Further Reading* entries |
| `TurningOnReplayOnSeen.md` | 469 | **475** | +6 banner and lead-in |
| `ReplayOnSeenReference.md` | 311 | **316** | +5 banner and lead-in |
| `SweeperCircuitBreaking.md` | ~395 | **401** | 395 on the nose, +6 for *Further Reading* |
| `UsingSweeperCircuitBreaking.md` | — | **148** | from 132 donated |
| `InMemoryOptions.md` | ~330 | **338** | 330 on the nose, +8 for *Further Reading* |
| `InMemoryTransport.md` / `InMemoryOutbox.md` / `InMemoryInbox.md` | — | **129 / 89 / 78** | from 118 / 79 / 68 |

#### The deviation: `## Outbox Archiver` was not all archiver

`BrighterOutboxSupport.md`'s `## Outbox Archiver` spans 151 lines, and design §7.1's arithmetic
(517 − 151 − 169 = 197) treats all 151 as archiver material. **Its last three H3s are not.**
`### Outbox Configuration` (4 lines), `### Provisioning the Outbox Table` (4) and
`### Outbox Builder` (52) are about configuring and provisioning the **Outbox**; none mentions
the Archiver. They are misfiled *within the source page*, and the mechanical span measurement
could not see it.

Moving them would have put Outbox DDL on a page titled *Outbox Archiver* — **an IA defect
created by the IA spec**. So only 198–288 moved (91 lines), and the three H3s were **promoted
to `##` in the core**, which is where they belong. The core lands at **257 + 7 = 264** rather
than ~197. No inbound link targets any of the three, re-derived. Same shape as design §16
finding 2: **a span measured by heading boundaries is not a measurement of subject.**

#### `ReplayOnSeen.md` had 27 internal anchors, and Appendix B does not count those

This is the finding that generalises. **Appendix B counts *inbound* links from other pages. It
does not count a page's *own* internal anchors** — and a three-way split converts every one
that crosses a seam into a cross-page link. `ReplayOnSeen.md` carries **27 same-page anchors**
across 14 distinct targets, more than the whole spec's inbound budget of 34.

They were handled by construction rather than by hand: every heading in the original was
mapped to its destination page via `linkcheck.py`'s `slug()`, the three ranges were asserted to
cover every line exactly once, and each `](#slug)` was rewritten to `](/contents/<dest>.md#slug)`
only where destination ≠ self. **Zero anchors were left unmapped**, asserted before writing.
D5 then reported **18 lines: 1 banner retype and 17 lines carrying repointed anchors** —
nothing else.

**Carry this into Phase 6.** `QueryPipeline.md` (928) and `QueriesAndQueryObjects.md` (877)
split five and two ways; count their internal anchors *before* starting, not after.

> **And a routing error, found by mapping rather than reading.** Task 5.2's note sends
> `#replay-versus-replay-skipped` to `TurningOnReplayOnSeen.md`. `### Replay versus Replay
> Skipped` sits at line 731, inside `## Observability` (709–777), which is the **Reference**
> page. Routed there instead. Design §8 has the same error. It would have shipped as a live
> 404 into the middle of a page, which is precisely the failure Appendix B's closing paragraph
> says redirects cannot fix.

#### Task 5.4: the scheduler "merge" added nothing, and that is the finding

`InMemoryOptions.md`'s `## InMemory Scheduler` (53 lines) is **entirely duplicated** by
`InMemoryScheduler.md`, which covers every claim more fully — including *"Demos and
proof-of-concepts"* **verbatim** at line 20. The section twice tells the reader to go to that
page instead (lines 303 and 348). It is a page's summary of another page, and merging it into
its own subject is a deletion.

So the fold added **nothing** to `InMemoryScheduler.md`. Task 3.1's precedent inverted: *not
duplicate* was a passing outcome there, and *entirely duplicate* is a passing outcome here —
both are answers, and both had to be measured rather than assumed. D5 accounts for all 11
dropped lines against the target's fuller text.

The archive donation **is** additive: `OutboxArchiver.md` had no archive-provider material.

#### Recorded, deliberately not acted on

- **`### InMemory Archive Example Usage` is not an archive example.** It is a
  `LargeMessageMapper` with a `[ClaimCheck]` attribute — the Claim Check pattern, nothing to do
  with archiving. It moved to `OutboxArchiver.md` verbatim under its wrong heading. **The line
  this phase held**: do not *newly* create a misfiling (Task 5.1's three H3s stayed put), but
  do not silently delete content either. This block's heading relationship is pre-existing;
  only its page changed.
- **`SchedulingAMessage.md`, created in Phase 4, carries four non-compiling scheduling calls.**
  `IAmACommandProcessor` declares `SendAsync<TRequest>(DateTimeOffset at, TRequest command, …)`
  and `SendAsync<TRequest>(TimeSpan delay, TRequest command, …)` — **the time comes first**, and
  the same holds for `PostAsync`. There is no overload taking the request first with a named
  `at:`/`delay:` argument; the plain `SendAsync(TRequest command, …)` returns `Task`, not
  `Task<string>`. The page therefore has `SendAsync(new ProcessOrderCommand { … }, at: …)`,
  `SendAsync(new SendReminderEmailCommand { … }, delay: …)`, `PostAsync(new NotificationEvent
  { … }, delay: …)` and `SendAsync(command with { … }, delay: delay)`, none of which compile.
  Moved verbatim from `BrighterSchedulerSupport.md`, so obligation 1 was honoured — but it was
  **not** among the three defects Phase 4 recorded, and it was found only because Task 5.4
  needed the signature for an unrelated reason. **The correct form is already in the corpus**,
  on `InMemoryScheduler.md`. It is a four-line correction awaiting a ruling.

#### Post-merge: `OutboxArchiver.md` publishes, but its HTML route served a cached 404

Measured after PR #89 merged. `outbox-and-inbox/outboxarchiver` returned a **190,021-byte 404
shell** — the 404 class, body containing *"404"* and *"not found"* — while:

- `sitemap-pages.xml` listed it (and moved 113 → **121**, exactly the eight new pages);
- both its children, `…/outboxarchiver/azureblobarchiveprovider` and its configuration page,
  returned real bodies at **four segments**;
- seven of the eight other new pages returned real bodies **in the same probe batch**;
- a `?cb=` query string did not change the response.

**`curl <path>.md` settled it**: `200`, 9,145 bytes, leading `# Outbox Archiver`. GitBook has
the page and publishes it; only the HTML route was stale, from this session's own probe landing
inside the 25–45 second sync window. **Recorded as a lesson in `PROMPT.md`: wait for the sitemap
to move before probing a new URL, and check the `.md` variant before concluding a page failed to
publish.**

> **Measured, and no longer a supposition — corrected in PR 6.** This paragraph read *"almost
> certainly from this session's own probe"* when it was written. After PRs #90 and #91 deployed,
> the same HTML route returned **658,502 bytes** — a real page, in the genuine-page class, with
> nothing about the page itself having changed in between. It was the edge cache, and a later
> deploy invalidated it. The hedge is removed rather than left to be re-read as a finding.

**Both redirects were confirmed working on the second probe** — `outbox-and-inbox/azureblobarchiveprovider`
and its child returned `307` with a `location:` header, and the **pre-PR-2 key**
`guaranteed-at-least-once/azureblobarchiveprovider` still redirects too. That is D0's finding
holding across **two** moves of the same page, which is what it predicted and had never been
tested at.

---

## Phase 6 — Darker (PR 6)

**Goal:** 5 rows, 12 new pages — the largest phase. `worklist.md` §5b executed: Darker
splits along the seams Brighter already has, restoring the parallel. Design §7.3.

**Binding constraint for every task in this phase: Darker content is re-filed and split,
never rewritten.** `../Darker` HEAD is `4.1.1-7-g2f76cda`, ahead of the deployed 4.1.1, and
the site publishes the deployed version. Do not update behaviour from that working tree.

- [x] **Task 6.1:** Split `QueryPatterns.md` — the largest page in the corpus
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

- [x] **Task 6.2:** Split `ImplementAQueryHandler.md`
  - Input: 935 lines; `Testing Query Handlers` (159), `Working with Dependencies` (130)
  - Output: `TestingQueryHandlers.md` and `QueryHandlerDependencies.md`, both How-to, nested
    under `ImplementAQueryHandler.md`; core → ~646
  - Notes: Testing a handler has no business inside "implement a handler". The core stays
    large at ~646 and that is deliberate — three handler patterns (311 lines) plus
    registration and error handling, and splitting further separates a reader from the thing
    they are implementing (design §7.8).

- [x] **Task 6.3:** Split `QueryPipeline.md`
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

- [x] **Task 6.4:** Split `QueriesAndQueryObjects.md` — **the Task 3.1 gate is OPEN**
  - Input: 877 lines; `Query Result Types` (179), `Validation in Query Objects` (119); and
    Task 3.1's finding on `## Query Patterns` (102)
  - Output: `QueryResultTypes.md` (Explanation) and `QueryObjectValidation.md` (How-to),
    nested under `QueriesAndQueryObjects.md`; core → ~579, or lower if 3.1 found duplication
  - Notes: ~~Do not start this task before Task 3.1 returns.~~ **Task 3.1 returned
    2026-08-08: NOT DUPLICATE.** So take the second branch — **the `## Query Patterns`
    section stays**, it is *not* replaced by a link, **D5 does not run against
    `QueryPatterns.md`**, and the core lands at **~579**. Only the two extractions remain,
    and they were always independent of the gate. The evidence is in *Phase 3 as executed*:
    the two sections pair one-to-one by subject, share 32 of 58 substantive lines verbatim
    and **zero prose lines**, and differ in their result types on all four. Do not re-verify
    it — but do note the divergence recorded there, which this task must not try to fix.

- [x] **Task 6.5:** Split `DarkerBasicConfiguration.md`
  - Input: 510 lines; `Darker Configuration Options` (75)
  - Output: `DarkerConfigurationReference.md` (Reference), nested under
    `DarkerBasicConfiguration.md`; core → ~435
  - Notes: Created **for the parallel, not for the size** — 75 lines against
    `DispatcherConfigurationReference.md`'s 233. It inherits the
    `BrighterBasicConfiguration.md` shape, and the core keeps `Quick Start`,
    `Using IQueryProcessor` and `Common Configuration Patterns`. **One inbound anchor link
    moves**: `#query-processor-lifetime` → `DarkerConfigurationReference.md`.

### Phase 6 as executed — 2026-08-11

**PHASE 6 IS COMPLETE. 31 of 52 tasks done.** Twelve pages created, five touched. **No URL
moves** — every core keeps its filename and all twelve new pages are new paths — so
`.gitbook.yaml` is untouched at **77** entries and this phase owes no redirect.

All five gates: linkcheck **134 files**, pagelint **0 errors / 791 warnings**, `--check-shape`
0 at **132 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at 77 entries,
`pagelint --changed origin/master` 0 errors with the twelve new files **staged**.

**Every predicted figure reproduced exactly, before a single line was moved.** All five whole
page lengths (1,291 / 935 / 928 / 877 / 510) and all eleven quoted `##` spans matched design
§7.3 to the line, and so did all five arithmetic cores:

| Page | Design | After the cut | Shipped | |
|---|---:|---:|---:|---|
| `QueryPatterns.md` | ~300 | **306** | **311** | 1,291 − 985; +5 for the five recipes in *Further Reading* |
| `ImplementAQueryHandler.md` | ~646 | **646** | **648** | +2 |
| `QueryPipeline.md` | ~716 | **716** | **718** | +2 |
| `QueriesAndQueryObjects.md` | ~579 | **579** | **581** | +2 |
| `DarkerBasicConfiguration.md` | ~435 | **435** | **436** | +1 |

The twelve new pages land at 281 / 244 / 186 / 173 / 160 (Task 6.1), 139 / 166 (6.2),
170 / 61 (6.3), 192 / 127 (6.4) and 86 (6.5). `DarkerAndBrighterPipelines.md` at **61** is the
thin page design §7.3 predicted at 53 and defended against the corpus floor; it is still above
`OutboxPattern.md` at 45.

**Appendix A and Appendix B both held exactly.** *Darker* stays at **5 top-level entries** and
goes to **17 pages**, the row Appendix A pins, with every new page at **three segments** — this
phase needs no fourth. Appendix B predicted **1** inbound anchor link on
`DarkerBasicConfiguration.md` and **0** on the other four sources; measured before the cut, that
is exactly what is there.

#### The anchor count, done first and both kinds

Phase 5's lesson applied in advance: the corpus was grepped for links *into* the five sources,
and each source was grepped for `](#` links *within* itself. The total is small and the whole
of it was known before anything moved.

- **Two same-page anchors**, both in `QueryPipeline.md` at lines 234 and 358, both pointing at
  `#configuring-polly-policies` — the one section Task 6.3 extracts. Both sit in
  `## Available Decorators`, which stays in the core, so both cross the seam. Repointed to
  `/contents/QueryPipelinePolicies.md`.
- **One inbound anchor**, `QueryPatterns.md:944` → `DarkerBasicConfiguration.md#query-processor-lifetime`.

**That inbound one is the finding, and Appendix B cannot see it.** Appendix B records the link
against `DarkerBasicConfiguration.md`, and it is right: that page owns the target. What the
table has no column for is that **both ends move in the same PR**. The target heading
`### Query Processor Lifetime` goes to `DarkerConfigurationReference.md` (Task 6.5), *and the
line carrying the link* sits at line 944, inside `## Entity Framework Core Integration`, which
goes to `EFCoreQueryIntegration.md` (Task 6.1). Neither task's own row mentions the other. It
was caught because the anchor grep ran before the cut and the repoint was then applied to the
line **where it had landed**, not where the table said it was. **A snapshot of inbound anchors
is indexed by target; a split can move the source too.**

After the split, obligation 3's grep was re-run — the standing lesson that splits *add* links.
Across all seventeen Darker pages there is now exactly **one** fragment link, the repointed one,
and **zero** same-page anchors anywhere.

#### The shape rule, stated because it decides thirty-five headings

Every extraction here is a **single `##` section**, which is the simplest split shape in the
spec and the one Phase 5 already settled. The rule applied, from `InMemoryOutbox.md` and
`TransactionalMessagingWithTheOutbox.md`: **the section's `##` heading becomes the new page's
H1 and is dropped from the body; its `###` headings promote to `##` and are requalified by the
new page's subject.** Keeping the `##` would have restated the H1 immediately beneath itself on
all twelve pages.

Task 6.1's note called this the place rule 3a bites hardest, and it does — but not the way the
note expects. **Not one of the 35 promoted headings collided**, checked against every `##` in
the corpus by `pagelint.py`'s own `slug()` before any were written. The work was entirely the
*editorial* half of rule 3a: `## Collections`, `## Dictionaries`, `## Similarities`,
`## Differences`, `## Acceptance Tests` and `## Default Policies` are all unique and all
unattributable, and a checker that only tests uniqueness would have passed every one.
**Rule 3a's mechanical half and its editorial half are different jobs, and only the first has a
tool.** They became `## Collection Query Results`, `## Dictionary Query Results`,
`## Darker and Brighter Pipeline Similarities`, and so on.

The one place it reads badly, shortened by hand as `CLAUDE.md` licenses: the five
`QueryPatterns.md` recipes each carried a `### Pattern: X` prefix that says nothing once
`Pattern` is the page's subject. Where the source words already carry the family
(`Pattern: Offset-Based Pagination`) the prefix was dropped and the family kept
(`## Offset-Based Pagination Pattern`); where they did not
(`Pattern: Single Entity Lookup`) the family was prefixed
(`## Parameterized Query Pattern: Single Entity Lookup`).

#### A written lead-in only where the section brought none

Phase 5 left two precedents side by side: `TransactionalMessagingWithTheOutbox.md` opens on the
moved section's **own** intro paragraph, while `InMemoryOutbox.md` carries a written lead-in
*and* the moved intro. The first draft here took the second option twelve times and produced
six pages that open twice, saying the same thing in two voices —
`DarkerAndBrighterPipelines.md`'s lead-in and its inherited first paragraph both said the two
pipelines are alike but differ.

So the rule, stated rather than decided page by page: **a written lead-in only where the moved
section brought no intro of its own.** Six sections opened straight onto a `###` and get one
(the five `QueryPatterns.md` recipes and `DarkerConfigurationReference.md`); the other six open
on their own inherited words and the lead-in was removed. It also spends less new prose, which
obligation 1 reserves.

#### D5: 50 lines across five invocations, every one a heading or an anchor

| Task | Lines | Made of |
|---|---:|---|
| 6.1 | 20 | 4 section headings absorbed into an H1 · 15 requalified `###` · 1 repointed anchor |
| 6.2 | 7 | 1 · 6 · 0 |
| 6.3 | 10 | 2 · 6 · 2 |
| 6.4 | 10 | 1 · 9 · 0 |
| 6.5 | 3 | 1 · 2 · 0 |

**Not one line of prose, and not one line of code, survives nowhere.** Where the new H1 happens
to slugify identically to the section heading it replaces — `## Query Result Types` becoming
`# Query Result Types` — `noloss.py` matches it on its anchor and says so; that accounts for the
four sections absorbed silently rather than reported.

#### `pagelint.py --changed` bit again, and the files were staged first

Twenty errors, all `USING DIRECTIVES`, all on new pages: every line of a new page is an added
line, so every C# block in it is strict. They carry fictional domain types — `IOrderRepository`,
`OrderDto`, `PagedResult<T>` — alongside `IQuery<TResult>` and Polly's `Policy`, so under
obligation 6 the omission is **marked `// ...`, not guessed at**. The repo-wide debt is
unchanged at **791 blocks**, which is the marker behaving as designed: it downgrades to a
warning and never silences. It spread from 101 pages to **108**, because the splits created
pages, not because any block got worse.

The files were `git add`ed before the run. Unstaged, the twelve new pages contribute no strict
ranges at all and the phase they exist to make strict passes vacuously.

#### Recorded, and the two corrections this PR made

Task 3.1's gate was **open before this phase started** and the second branch was taken as
written: `QueriesAndQueryObjects.md`'s `## Query Patterns` (102 lines) **stays**, is not
replaced by a link, `D5` did not run against `QueryPatterns.md` for Task 6.4, and the core lands
at **579** — design §7.3's figure to the line. Nothing was re-verified.

Two claims this PR would otherwise have shipped false, corrected under the line Phase 4 drew:

- **`QueryPatterns.md`'s introduction said "this document focuses on ... pagination,
  projections, aggregations, and Entity Framework Core integration"** — four topics this PR
  moves off the page. Changed to "these pages focus on", one phrase, leaving the sentence and
  its links otherwise untouched. The hub is not a stub — it keeps `Performance Best Practices`,
  the 197-line `Real-World Example: Product Catalog Query`, the summary and the pitfalls — but
  it now introduces a family rather than containing it.
- **The repointed link's text named the wrong page.** `[Darker Basic Configuration](…)` retargeted
  at `DarkerConfigurationReference.md` would have told a reader it goes somewhere it does not.
  Retitled to `[Darker Configuration Reference]`. A link label is part of the link, and the split
  is what made it wrong.

Not acted on, and not defects of this phase: `QueryPipeline.md` keeps `Available Decorators`
(291) and `Decorator Patterns` (162) because its worklist row says so — extracting the decorator
reference stays design §11's candidate for a later spec. No Darker page content was rewritten;
`../Darker` HEAD is still ahead of the deployed 4.1.1.

---

## Phase 7 — Using an External Bus (PR 7)

**Goal:** 4 rows, 3 new pages, and **the one piece of new prose this entire spec authors**.
Design §7.5, `worklist.md` §5c.

- [x] **Task 7.1:** Create `MessageTransforms.md` — §5c, and it carries a correctness fix
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

- [x] **Task 7.2:** Establish `DefaultMessageMappers.md` as the default route — §5c row 1
  - Input: `MessageMappers.md`; `DefaultMessageMappers.md` (**478** lines, already typed
    How-to)
  - Output: a link and a pointer from `MessageMappers.md`; **no new page**
  - Notes: §5c calls for a "default mapper how-to" and it **already exists**. This row is
    *establish it as the default route*, not *write it*. `worklist.md` §8 lists "how to use
    the default mapper" among the four missing how-tos for Spec 013 — this task is why that
    one is already covered. **`## Configuration Reference` (54) stays in
    `DefaultMessageMappers.md`** (design §7.7 item 3): it is the how-to's own configuration
    table, and splitting it would produce a stub.

- [x] **Task 7.3:** Split `CloudEventsSupport.md`
  - Input: 475 lines; `CloudEvents Attributes` (34), `CloudEvents Across Transports` (72) =
    106
  - Output: `CloudEventsReference.md` (Reference), nested under `CloudEventsSupport.md`;
    core → ~369
  - Notes: `worklist.md` calls this the **highest-confidence row in the file** — the shape is
    already ruled, not proposed. The How-to core keeps the name; the required/optional/
    extension attribute tables and the per-transport matrix are consulted rather than
    followed, so they become Reference.

- [x] **Task 7.4:** Split `DynamicMessageDeserialization.md`
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

### Phase 7 as executed — 2026-08-11

**PHASE 7 IS COMPLETE. 35 of 52 tasks done.** Three pages created, seven touched. **No URL
moves** — all four cores keep their filenames and the three new pages are new paths — so
`.gitbook.yaml` is untouched at **77** entries and this phase owes no redirect, the same shape
as Phase 6.

All five gates: linkcheck **137 files**, pagelint **0 errors / 791 warnings**, `--check-shape`
0 at **135 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at 77 entries,
`pagelint --changed origin/master` 0 errors with the three new files **staged**. `pagetypes.tsv`
is **135 rows** and banner parity is **135/135**, checked both directions.

**Every predicted figure reproduced, measured before a line moved.** All four whole-page
lengths and all eight quoted `##` spans matched design §7.5 to the line, and so did all four
arithmetic cores:

| Page | Design | After the cut | Shipped | |
|---|---:|---:|---:|---|
| `MessageMappers.md` | ~147 | **147** | **154** | 266 − 119; +1 for Task 7.2's pointer, +6 for a *Further Reading* the page never had |
| `DefaultMessageMappers.md` | ~333 | **333** | **334** | 478 − 145; +1 |
| `CloudEventsSupport.md` | ~369 | **369** | **370** | 475 − 106; +1 |
| `DynamicMessageDeserialization.md` | ~281 | **281** | **282** | 597 − 316; +1 |

The three new pages land at 272 (7.1), 115 (7.3) and 326 (7.4) before the `// ...` markers, and
at **280 / 119 / 334** after them.

**Appendix A and Appendix B both held exactly.** *Using an External Bus* stays at **9**
top-level entries and goes to **15 pages**, the row Appendix A pins, with all three new pages at
**three segments**. Appendix B predicted **3** inbound anchor links on `MessageMappers.md`, all
3 repointing, and 0 on the other three sources; measured before the cut, that is exactly what is
there.

#### The anchor count, done first and both kinds

- **Three inbound anchors**, all `MessageMappers.md#message-transformer-factory`, at
  `S3LuggageStore.md:17`, `ClaimCheck.md:11` and `Compression.md:11`. All three carrying pages
  are **outside this phase's sources**, so Phase 6's finding — that a split can move the source
  line too — does not bite here; it was checked rather than assumed.
- **Zero same-page anchors** on any of the four sources.

`### Message Transformer Factory` was deliberately **not requalified**. It is an H3, so rule 3a
does not reach it and rule 3b is satisfied within the new page, and leaving it alone keeps the
slug — which turns all three repoints into a pure path change, `/contents/MessageMappers.md#…`
→ `/contents/MessageTransforms.md#…`. After the split the grep was re-run: three fragment links
across the seven pages, all three the repointed ones, and still zero same-page anchors.

#### The one piece of new prose in this spec, and why it was checked against the source

Task 7.1's ruling is that `MessageTransforms.md` must state that transforms **require a custom
mapper**. Written from the ruling alone it would have been wrong in a way no reader could catch.
`TransformPipelineBuilder.BuildWrapPipeline` calls `FindWrapTransforms(messageMapperLease.Instance)`,
which resolves to `GetCustomAttributes<WrapWithAttribute>(true)` on the mapper's own
`MapToMessage` — so transforms are discovered from attributes on **the mapper you registered**.
And `JsonMessageMapper<TRequest>`, the default, already carries `[CloudEvents(0)]`, which *is* a
`WrapWithAttribute`. **"Default mappers run no transforms" would therefore have been false.**

The sentence shipped says what is true: a transform of your own needs a custom mapper to attach
it to, because the default mappers carry only the `[CloudEvents]` transform Brighter puts there
and you cannot add an attribute to a type you do not own. Sources:
`Brighter/src/Paramore.Brighter/TransformPipelineBuilder.cs:342`,
`Brighter/src/Paramore.Brighter/Extensions/ReflectionExtensions.cs:36`,
`Brighter/src/Paramore.Brighter/MessageMappers/JsonMessageMapper.cs:35`. It also corroborates a
line the core already carried and keeps — `### 2. Transform Pipelines` in
`DefaultMessageMappers.md` — which is the page that was right all along.

#### Shape: two multi-section pages and one, all under the settled rule

Every page here is built from **more than one** `##` section, so Phase 6's other branch applies:
the sections **stay `##`** and no `###` is promoted. That is why this phase requalifies exactly
**one** heading against Phase 6's thirty-five.

- `MessageTransforms.md` — `## Transformers` → **`## Message Transformers`**, plus
  `## Transform Pipeline Example` unchanged. The first is the only requalification in the phase;
  `## Transformers` said nothing about which page it came from, which is rule 3a's editorial half.
- `CloudEventsReference.md` — `## CloudEvents Attributes` and `## CloudEvents Across Transports`,
  both already carrying their subject, both unique once they move.
- `RoutingMultipleMessageTypes.md` — three unchanged, and
  `## Dynamic Deserialization Configuration Examples` → **`## Routing Configuration Examples`**,
  because the old qualifier names the page it left rather than the page it landed on. Nothing
  linked to its anchor. Rule 3b holds across all four sections: nine `###` headings, no collision.

**A written lead-in on one page of three.** `MessageTransforms.md` gets one, because it is where
Task 7.1's sentence has to live. `CloudEventsReference.md` and `RoutingMultipleMessageTypes.md`
each open on a section that brought its own introduction, so they go H1 → banner → `##` and
spend no new prose, which is what obligation 1 reserves.

#### `pagelint.py --changed` bit again, and the files were staged first

**Twenty errors**, all `USING DIRECTIVES`, all on the three new pages — 8 / 4 / 8 — because every
line of a new page is an added line and every C# block in it is therefore strict. The blocks carry
fictional domain types (`LargeOrder`, `SensitiveOrder`, `RemovePII`) alongside real ones
(`Publication`, `KafkaSubscription`, `WrapWithAttribute`), so under obligation 6 the omission is
**marked `// ...`, not guessed at**. The repo-wide debt is unchanged at **791 blocks** — the
marker downgrading and never silencing — spread from 108 pages to **111** because the splits
created pages.

#### D5: 5 lines across three invocations, every one a heading or a link

| Task | Lines | Made of |
|---|---:|---|
| 7.1 (fold, one invocation over the union) | 3 | 1 requalified `##` · 1 link appended to a surviving sentence · 1 corrected link label |
| 7.3 | 1 | 1 repointed link |
| 7.4 | 1 | 1 requalified `##` |

**Not one line of prose, and not one line of code, survives nowhere.** Task 7.1's check is a
single invocation over the union of `MessageMappers.md` + `DefaultMessageMappers.md` on one side
and those two plus `MessageTransforms.md` on the other, because §5c is a fold across two sources.

#### The two corrections this PR made, under the line Phase 4 drew

Both are links whose **text** named content this PR moved, which is the shape Phase 6 met and
recorded:

- **`CloudEventsSupport.md`: "Only create custom mappers when you need
  [transform pipelines](DefaultMessageMappers.md)."** The transform pipeline material leaves
  `DefaultMessageMappers.md` under Task 7.1 in this same PR, so the link would have shipped
  pointing at a page that no longer holds what its text promises. Repointed to
  `MessageTransforms.md`; the sentence is untouched.
- **`DefaultMessageMappers.md`'s *Further Reading* called `MessageMappers.md`
  "Legacy V9 mapper documentation".** That page ships in this PR banner'd
  `Applies to **Brighter V10**` and, after Task 7.2, is the page the default route points *back*
  at for the custom case — so the label contradicted another page in the same PR. Relabelled
  "Writing a custom mapper, and the Brighter message structure".

#### Recorded, and what Task 7.2 cost in prose

Task 7.2 creates no page: its output is *a link and a pointer*, and that is one sentence added to
`MessageMappers.md` plus a *Further Reading* section the page had never had. Obligation 1 reserves
new prose to Task 7.1's sentence, and this is the one place the phase spends any beyond it —
sanctioned by Task 7.2's own Output line, recorded here rather than left for a reader of the diff
to wonder about.

Not acted on, and not defects of this phase: `DefaultMessageMappers.md` keeps
`## Configuration Reference` (54) — design §7.7 item 3, it is the how-to's own table and
extracting it produces a stub. `DynamicMessageDeserialization.md` keeps
`## Dynamic Deserialization Best Practices` (120), which is larger than any section this phase
moved; its row does not call for it and design §7.5 does not list it.

---

## Phase 8 — Transports (PR 8)

**Goal:** 2 rows, 2 new pages. Design §7.2. (`InMemoryTransport.md` files into this section
but is created in Task 5.4, with the rest of the `InMemoryOptions.md` redistribution.)

- [x] **Task 8.1:** Split `AWSSQSConfiguration.md` — the exact RabbitMQ precedent
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

- [x] **Task 8.2:** Split `PostgreSQLMessageBroker.md`
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

### Phase 8 as executed — 2026-08-12

**PHASE 8 IS COMPLETE. 37 of 52 tasks done.** Two pages created, five touched. **No URL
moves** — both cores keep their filenames and both new pages are new paths — so
`.gitbook.yaml` is untouched at **77** entries and this phase owes no redirect, the same shape
as Phases 6 and 7.

All five gates: linkcheck **139 files**, pagelint **0 errors / 791 warnings**, `--check-shape`
0 at **137 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at 77 entries,
`pagelint --changed origin/master` 0 errors with both new files **staged**. `pagetypes.tsv` is
**137 rows** and banner parity is **137/137**, checked both directions.

**Appendix A held exactly.** *Transports* lands at **12 pages and 7 top-level entries** — the
row Appendix A pins — with both new pages at **three segments**
(`transports/awssqsconfiguration/awssqsmigratetov10`,
`transports/postgresqlmessagebroker/postgresqlbrokertradeoffs`).

#### The finding: a source measurement overtaken by a later PR, not a wrong tally

**`PostgreSQLMessageBroker.md` is 663 lines, not the 662 this task records** — and 662 was
right when it was written. **PR #90** (`f9f042d`, the scheduling-overload fix) added one line
to this page: a `// ...` marker inside `## Scheduled Messages`, a section that stays in the
core. Traced rather than assumed:

| Ref | Lines |
|---|---:|
| `25a578c` (tasks approved) | 662 |
| `f9f042d~1` | 662 |
| `f9f042d` (PR #90) | **663** |
| `origin/master` | **663** |

So the core budget is **553**, not ~552, and every span this task quotes still reproduces to
the line. **This is a new shape for this programme.** Eighteen figures have been wrong and
every one was wrong when written; this one was correct when written and a later PR moved the
page underneath it. The remedy is not "re-derive the total" — it is **re-measure the source at
the ref you are cutting from**, because a phase that ships months after its plan is measuring a
different file. Appendix C's *do not re-derive a figure from memory* covers the failure but not
this cause.

`AWSSQSConfiguration.md` reproduced at **615** under both counting conventions, exactly as
Task 8.1 claims — the file has no trailing newline — and both its spans matched: `AWS SDK v4
Support` **49**, `V10 Migration Path` **196**, summing to **245**.

| Page | Design | After the cut | Shipped | |
|---|---:|---:|---:|---|
| `AWSSQSConfiguration.md` | ~370 | **370** | **375** | 615 − 245; +5 for a *Further Reading* the page never had |
| `PostgreSQLMessageBroker.md` | ~552 | **553** | **554** | 663 − 110; +1 for the child pointer |

The new pages land at 249 and 114 before their *Further Reading* and `// ...` markers, and at
**261** and **125** after them.

#### The anchor count, done first and both kinds — and the second kind was the whole story

**Appendix B held on what it counts.** Three inbound anchors on `AWSSQSConfiguration.md`, all
three `#migrating-from-aws-sdk-v3-to-v4`, all three repointing:
`S3LuggageStore.md:15`, `DynamoOutbox.md:22`, `DynamoInbox.md:16`. All three carrying pages sit
**outside this phase's sources**, so Phase 6's moving-source case was checked and does not bite.
`PostgreSQLMessageBroker.md` carries **zero** inbound fragment links, as predicted.

**What Appendix B does not count is where the work was.** `AWSSQSConfiguration.md` carries
**six same-page anchors**, and mapping every heading to its destination — Phase 5's method —
splits them three ways:

| Anchor | Source line | Target heading | Verdict |
|---|---|---|---|
| `#topicfindbyarn` ×2 | 91, 99 — core | 70 — core | core → core, untouched |
| `#sqs-publication` | 266 — core | 49 — core | core → core, untouched |
| `#finding-and-creating-queues` | 369 — core | 139 — core | core → core, untouched |
| `#v10-migration-path` | 418 — **moves** | 420 — **moves** | both land on the new page; **repointed**, because the heading was requalified |
| `#migrating-from-aws-sdk-v3-to-v4` | 429 — **moves** | 431 — **moves** | both land on the new page; **untouched**, because the H3 was not |

The last two are Phase 6's finding in its benign direction — the source line and the target
heading move **together**, so a same-page anchor stays a same-page anchor rather than breaking.
That only holds because the map was built before the cut: read from Appendix B alone, both
would have looked like nothing to do, and one of them was.

`### Migrating from AWS SDK v3 to v4` was deliberately **not requalified** — rule 3a does not
reach an H3, rule 3b is satisfied within the new page, and leaving it alone keeps the slug,
which turns all three inbound repoints into a pure path change. Phase 7's fourth line, applied
again. After the cut both greps were re-run: three fragment links across the corpus, all three
the repointed ones, and six same-page anchors split 4 on the core and 2 on the new page, with
no link added by the split.

#### Shape: both pages multi-section, and six unqualified headings nobody's tool would have caught

Both new pages are built from **more than one** `##` section, so Phase 6's second branch
applies: the sections stay `##` and nothing is promoted.

**All six moving `##` headings were unique across the corpus before they moved** — a completely
green rule-3a run was available for the taking, on `## Benefits`, `## When to Use`,
`## V10 Migration Path`, `## JSON vs JSONB` and `## Comparison with Other Transports`. That is
Phase 6's third finding reproducing exactly: uniqueness has a tool, attribution does not, and
`## Benefits` in a retrieval chunk is benefits of what?

**Five requalified, one left alone:**

| Was | Is | Why |
|---|---|---|
| `## V10 Migration Path` | `## AWS SQS V10 Migration Path` | migration path of what — the corpus has a whole V10 migration section |
| `## Benefits` | `## PostgreSQL Message Broker Benefits` | unattributable |
| `## When to Use` | `## When to Use the PostgreSQL Message Broker` | unattributable |
| `## JSON vs JSONB` | `## PostgreSQL JSON vs JSONB` | a PostgreSQL storage question, not a Brighter one |
| `## Comparison with Other Transports` | `## PostgreSQL Message Broker Compared with Other Transports` | comparison *of* what |
| `## AWS SDK v4 Support` | *(unchanged)* | already names its subject |
| `## PostgreSQL Message Broker Limitations` | *(unchanged)* | already qualified, and leaving it keeps the slug |

The qualifier is the source page's own established form — `PostgreSQL Message Broker …` — rather
than a shorter one invented here, so the new page's headings read as the same family as the core's.

**A written lead-in on one page of two.** `PostgreSQLBrokerTradeOffs.md` opens on
`## PostgreSQL Message Broker Benefits`, which goes straight to its first `###` and brought no
introduction of its own, so it gets four lines saying what the page is for.
`AWSSQSMigrateToV10.md` opens on `## AWS SDK v4 Support`, which brought its own two paragraphs,
so it goes H1 → banner → `##` and spends no new prose.

#### `pagelint.py --changed` bit for the third phase running, and the files were staged first

**Seven errors**, all `USING DIRECTIVES`, all on the two new pages — 6 / 1 — because every line
of a new page is an added line and every C# block in it is therefore strict. **The gate is
proved live in this phase rather than by a synthetic probe**: it went red before the markers
were added and green after. The blocks carry real AWS SDK and Brighter types
(`AwsMessagingGatewayConnection`, `CredentialProfileStoreChain`, `FallbackCredentialsFactory`,
`RelationalDatabaseConfiguration`) whose namespaces were **not** checked, so under obligation 6
and Appendix C the omission is **marked `// ...`, not guessed at**. Repo-wide debt is unchanged
at **791 blocks** — the marker downgrading and never silencing — spread from 111 pages to
**113**.

#### D5: 6 lines across two invocations, every one a requalified heading or its anchor

| Task | Lines | Made of |
|---|---:|---|
| 8.1 | 2 | 1 requalified `##` · 1 same-page anchor repointed in the same pass |
| 8.2 | 4 | 4 requalified `##` |

**Not one line of prose, and not one line of code, survives nowhere.**
`## PostgreSQL Message Broker Limitations` does not appear in the 8.2 list, which is the check
confirming the one heading that was left alone was genuinely left alone.

#### Recorded, and not acted on

- **`AWSSQSConfiguration.md` had no *Further Reading* at all**, so the core would have shipped
  with no pointer to its own child — `See [V10 Migration Path](#v10-migration-path)` left with
  the section that owned it. Five lines added, the same shape as Phase 7's `MessageMappers.md`,
  and recorded here rather than left for a reader of the diff to wonder about.
- **`AWSSQSMigrateToV10.md:12–13` has no blank line before `### FIFO Queue Support`.** A
  pre-existing defect at `AWSSQSConfiguration.md:378`, moved verbatim under obligation 1. It
  renders today and it renders after the move.
- **`## Transactional Messaging` (46 lines) stays in the PostgreSQL core**, design §7.7 item 2,
  **flagged to Spec 013** and not half-extracted. So does `## Scheduled Messages`, which is
  where PR #90's line went.
- **`contents/ReturningResultsFromAHandler.md` opens with a blank line before its H1.** Found by
  the banner-parity check, which assumed the H1 was line 1 and was wrong about the script rather
  than the page; `pagelint.py` accepts it, since rule 1 asks only what the first non-blank line
  *after* the H1 is. Nothing to do with this phase, recorded because the next parity check will
  meet it too.

---

## Phase 9 — The rest of §6d (PR 9)

**Goal:** 5 rows, 5 new pages, plus one navigational fix design §11 records. Design §7.5.

- [x] **Task 9.1:** Split `CQRSWithBrighterAndDarker.md`
  - Input: 1,144 lines; `Use Cases and Patterns` (209)
  - Output: `CQRSUseCasesAndPatterns.md` (Explanation), nested under
    `CQRSWithBrighterAndDarker.md`; core → ~935
  - Notes: **The core keeps `## Example: E-Commerce Order System` (226 lines).**
    Requirements §8 is binding — 010 must not consume material Spec 009 needs, and 226 lines
    of end-to-end example is the closest thing the corpus has to a tutorial. The page drops
    to ~709 when 009 takes it. **Flagged, not moved.** ~935 is the largest page after this
    spec completes and it is deliberate.

- [x] **Task 9.2:** Split `NullableReferenceTypes.md`
  - Input: 711 lines; `Migration Guide` (264) — more than a third of the page
  - Output: `MigratingToNullableReferenceTypes.md` (How-to), nested under
    `NullableReferenceTypes.md`; core → ~447
  - Notes: **The type ruling already contains the split** — the page was typed Reference on
    the grounds that "the migration steps are not where the durable value is". The ruling and
    the split agree, which is why this row is low-risk.

- [x] **Task 9.3:** Split `AgreementDispatcher.md`
  - Input: 720 lines; `Standard vs Agreement Dispatcher Routing` (74), `Use Cases` (146),
    `Limitations` (53), `Performance Implications` (47) = **320**
  - Output: `AgreementDispatcherRouting.md` (Explanation), nested under
    `AgreementDispatcher.md`; core → ~400
  - Notes: Typed How-to because "the registration syntax is the point of the page, not the
    pattern discussion around it" — which again names the split. The core keeps
    `Registration Syntax`, `Synchronous and Asynchronous Registration` and
    `Complete Example`. Four sections merging into one page: watch rule 3b.

- [x] **Task 9.4:** Split `PolicyRetryAndCircuitBreaker.md`
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

- [x] **Task 9.5:** Split `Telemetry.md`
  - Input: 597 lines; `Configuring OpenTelemetry` (81), `Complete Configuration Example`
    (96), `Distributed Tracing Example` (30) = **207**
  - Output: `ConfiguringOpenTelemetry.md` (How-to), nested under `Telemetry.md`; core → ~390
  - Notes: The Reference core keeps the per-component span tables — Command Processor,
    Dispatcher, Outbox, Inbox and Transform Pipeline tracing. **Two inbound anchor links,
    neither of which moves**: `#configurable-instrumentation` and `#inbox-tracing` both stay
    in the core. Verify that before moving anything, not after.

- [x] **Task 9.6:** Make the `HandlerFailure.md` ↔ `ErrorHandlingOptions.md` relationship
      navigational
  - Input: both pages; `worklist.md` §6e; design §11
  - Output: `ErrorHandlingOptions.md` nested under `HandlerFailure.md` in `SUMMARY.md`
    (already so in `SUMMARY.target.md`), the missing reverse pointer added, and a
    *Prerequisites* segment in `ErrorHandlingOptions.md`'s banner
  - Notes: **They are not merged.** They are the corpus's best existing explanation/reference
    pair, and merging them yields ~685 lines carrying two modes — which is what everything
    else in this spec is pulling apart. ~~Today only `ErrorHandlingOptions.md` points at
    `HandlerFailure.md`.~~ **False, and false when written — corrected 2026-08-12.**
    `HandlerFailure.md` has pointed at `ErrorHandlingOptions.md` since `ac0c727` created the
    page, six times today. The `SUMMARY.md` nesting was already done in PR 2 as well, so this
    task reduced to the banner segment. See *Phase 9 as executed*. It is in this PR because it
    is the only phase that touches neither page's body substantially.

---

### Phase 9 as executed — 2026-08-12

**PHASE 9 IS COMPLETE. 43 of 52 tasks done, and this was the last content phase.** Five pages
created, seven touched. **No URL moves** — every core keeps its filename and all five new pages
are new paths — so `.gitbook.yaml` is untouched at **77** entries and this phase owes no
redirect, the same shape as Phases 6, 7 and 8.

All five gates: linkcheck **144 files**, pagelint **0 errors / 791 warnings**, `--check-shape`
0 at **142 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at 77 entries,
`pagelint --changed origin/master` 0 errors with all five new files **staged**.
`pagetypes.tsv` is **142 rows** and banner parity is **142/142**, checked both directions.

**All 32 new pages have now landed, and Appendix A's section table reproduces row for row.**
Enumerated from `SUMMARY.md` rather than read off the table it is being checked against:

| Section | Pages | Entries | Appendix A |
|---|---:|---:|---|
| Get Started | 3 | 3 | ✓ |
| Commands, Handlers and Pipelines | 17 | 5 | ✓ |
| Brighter Configuration | 6 | 4 | ✓ |
| Using an External Bus | 15 | 9 | ✓ |
| Transports | 12 | 7 | ✓ |
| Outbox and Inbox | 39 | 9 | ✓ |
| Scheduler | 10 | 4 | ✓ |
| Darker | 17 | 5 | ✓ |
| Health Checks and Observability | 5 | 4 | ✓ |
| V10 Migration | 3 | 2 | ✓ |
| Understanding Brighter | 13 | 10 | ✓ |
| Reference | 2 | 2 | ✓ |

**142 pages. S1 ✅ 2 · S2 ✅ 10 of 12 · S3 ✅ 4 of 4**, reached by exactly the two pages
Appendix A names — and `MigratingToPollyV8.md` publishes at
`commands-handlers-and-pipelines/buildingapipeline/policyretryandcircuitbreaker/migratingtopollyv8`,
which is the placement that caused S3 to be measured in the first place.

#### The five source measurements all reproduced, for the first time in the programme

Phase 8's lesson was to re-measure the source at the ref you are cutting from. Done first, and
**every whole-page count and every span reproduced `tasks.md` exactly** at `3f0f4dd`:

| Task | Source | Lines | Spans | Sum |
|---|---|---:|---|---:|
| 9.1 | `CQRSWithBrighterAndDarker.md` | 1,144 | 448–656 | 209 |
| 9.2 | `NullableReferenceTypes.md` | 711 | 166–429 | 264 |
| 9.3 | `AgreementDispatcher.md` | 720 | 11–84 · 85–230 · 325–377 · 378–424 | 320 |
| 9.4 | `PolicyRetryAndCircuitBreaker.md` | 687 | 377–472 · 537–687 | 247 |
| 9.5 | `Telemetry.md` | 597 | 19–99 · 366–461 · 462–491 | 207 |

**Task 9.3's naming trap is real and was met.** `Use Cases` and `Limitations` do not exist as
strings on the page — Spec 011's `0b1b841` qualified them to `## Agreement Dispatcher Use Cases`
and `## Agreement Dispatcher Limitations` before this design was written — so the task's spans
are right and only its titles are pre-qualification.

| Page | Was | Budget | Shipped | |
|---|---:|---:|---:|---|
| `CQRSWithBrighterAndDarker.md` | 1,144 | ~935 | **936** | 1,144 − 209, +1 child pointer |
| `CQRSUseCasesAndPatterns.md` | — | 209 | **225** | +lead-in, +*Further Reading*, +2 `// ...` |
| `NullableReferenceTypes.md` | 711 | ~447 | **448** | 711 − 264, +1 |
| `MigratingToNullableReferenceTypes.md` | — | 264 | **298** | +22 `// ...` |
| `AgreementDispatcher.md` | 720 | ~400 | **401** | 720 − 320, +1 |
| `AgreementDispatcherRouting.md` | — | 320 | **346** | +11 `// ...` |
| `PolicyRetryAndCircuitBreaker.md` | 687 | ~440 | **438** | 687 − 247 − 3, +1 — see below |
| `MigratingToPollyV8.md` | — | 247 | **277** | +12 `// ...` |
| `Telemetry.md` | 597 | ~390 | **391** | 597 − 207, +1 |
| `ConfiguringOpenTelemetry.md` | — | 207 | **220** | +4 `// ...`, no lead-in |

**`PolicyRetryAndCircuitBreaker.md` is the one core to land *under* budget**, at 438 against
~440, and the three lines are accounted for: the `## Legacy` section was the **last** on the
page and the `---` separator introducing it was left orphaned by its removal, so the separator
and its blank line went too; the page's trailing blank line went with them. Recorded rather than
silently absorbed, because a core landing under budget is the direction nobody checks.

#### Task 9.6: the defect did not exist in the direction the design describes

Design §11 and this task both say *"today only `ErrorHandlingOptions.md` points at
`HandlerFailure.md`"*, and the remedy is *"the missing reverse pointer"*. **The reverse pointer
is not missing and never was.** `HandlerFailure.md` carries **six** links to
`ErrorHandlingOptions.md` — at lines 67, 137, 234, 381, 460 and one in *Further Reading* — and
has carried five of them since `ac0c727` created the page on 2026-02-23. Traced rather than
assumed:

| Ref | Links `HandlerFailure.md` → `ErrorHandlingOptions.md` |
|---|---:|
| `ac0c727` (page created, 2026-02-23) | 5 |
| `25a578c` (tasks approved) | 6 |
| `origin/master` | 6 |

The `SUMMARY.md` nesting was likewise already in place — it landed in PR 2, which is what the
task's own parenthesis *"already so in `SUMMARY.target.md`"* was pointing at. **So two of Task
9.6's three deliverables were already true, and the task reduces to the third**: the
*Prerequisites* segment in `ErrorHandlingOptions.md`'s banner, which was genuinely absent and is
now there.

This is **Task 4.5's shape for the fourth time** — *verify the defect exists before fixing it* —
and this instance is the mildest of the four, because acting on it as written would have added a
seventh redundant link rather than destroyed anything. What makes it worth recording is *why* it
survived: the claim is about a **direction**, both pages do link to each other, and a reader
checking "are these two pages cross-linked?" gets a yes. **A one-directional claim needs a
one-directional grep**; `grep -l` on the pair answers a different question.

#### Shape: one single-section page, four multi-section, and a `####` case the rule had not met

Phase 6's rule branched on the number of `##` sections a page is built from, and Phase 9 hit
both branches plus one case neither had:

- **Single-section** — `CQRSUseCasesAndPatterns.md` (from `## Use Cases and Patterns`) and
  `MigratingToNullableReferenceTypes.md` (from `## Migration Guide`). Heading becomes the H1,
  dropped from the body, `###` promoted to `##` and requalified.
- **Multi-section** — the other three. Sections stay `##`; nothing is promoted.

**`MigratingToNullableReferenceTypes.md` is the first single-section page with `####` beneath
its `###`.** Phase 6's rule says *"promotes its `###` to `##`"* and stops there, which would have
left five `####` sitting directly under a `##` — a skipped level, on the page's largest section.
**The whole subtree is promoted by one**: four `###` → `##`, five `####` → `###`. That is the
rule's evident intent rather than an extension of it, and it is written down here because the
rule as stated does not say so.

**`ConfiguringOpenTelemetry.md` hit a collision the shape rule cannot see.** It is built from
three sections and the first is `## Configuring OpenTelemetry` — the same text as the H1 that
Appendix A's filename pins. `pagelint.py` is silent, because rule 3b covers `##` through `####`
and an H1 is not in that range; the page would simply have opened with its own title twice. The
section is requalified to **`## Setting Up OpenTelemetry`**, which is what it does — add the
`ActivitySource`, wire the SDK, pick an exporter — and is distinct from the page it sits on.

**A written lead-in on four pages of five.** `ConfiguringOpenTelemetry.md` is the exception:
`## Configuring OpenTelemetry` brought its own introductory paragraph, so the page goes H1 →
banner → `##` and spends no new prose, exactly as `AWSSQSMigrateToV10.md` did in Phase 8. The
other four opened straight onto a `###` or a code block and got three or four lines saying what
the page is for and pointing back at the core.

#### The anchor count, both kinds, before and after — and it was quiet

**Appendix B reproduced exactly**, and so did `PROMPT.md`'s pre-flight table:

| Page | Inbound fragments | Same-page | Outcome |
|---|---:|---:|---|
| `Telemetry.md` | 2 | 0 | **0 repoints** — `#configurable-instrumentation` (line 100) and `#inbox-tracing` (line 261) both sit in sections the core keeps. Verified by mapping heading to destination, not by trusting the note |
| `PolicyRetryAndCircuitBreaker.md` | 1 | 0 | **1 repoint** — `CommandProcessorConfigurationReference.md:105` |
| `HandlerFailure.md` | 1 | 7 | **0** — Task 9.6 moves no heading and no body text, confirmed before the banner was touched |
| the other four sources | 0 | 0 | — |

**Not one of the five split sources carries a same-page anchor.** Phase 5's method was run
anyway, because that is a fact you establish rather than assume, and it is the reason this phase
has no equivalent of Phase 8's moving-source pair. Phase 6's moving-source case does not bite
either: the single repointing link sits on `CommandProcessorConfigurationReference.md`, a page
outside this phase's sources.

The repoint is **not** a pure path change: `## Migration Guide: V9 to V10` had to be requalified
(it is unattributable, and `Migration Guide` is one of the corpus's most reused phrases), so the
target became `MigratingToPollyV8.md#polly-v8-migration-guide-v9-to-v10`. Phase 7's fourth line
— *a heading nothing needs requalified is a slug you get to keep* — pays off in the other
direction here: `## Legacy: Using Polly v7 Policies (Deprecated)` already names its subject, was
left alone, and kept its slug.

Both greps were re-run after the cut: three fragment links across the corpus, all three
resolving, and no link added by any of the five splits.

#### Uniqueness green, attribution poor — for the fourth phase running

**Every one of the eleven moving or promoted `##` headings was unique across the corpus before
it moved.** A completely green rule-3a run was available for the taking on
`## Pattern: Task-Based UI`, `## Step 2: Address Compiler Warnings`,
`## Performance Implications`, `## Complete Configuration Example` and
`## Distributed Tracing Example`. Phase 6 found this, Phase 7 and Phase 8 reproduced it, and it
has now held for four phases: **uniqueness has a tool and attribution does not.**

**Eight requalified, three left alone:**

| Was | Is | Why |
|---|---|---|
| `### Pattern: Separate Read/Write Databases` | `## CQRS Pattern: Separate Read/Write Databases` | separate read/write databases *for what* |
| `### Pattern: Event-Sourced Writes, Projected Reads` | `## CQRS Pattern: Event-Sourced Writes, Projected Reads` | as above |
| `### Pattern: Task-Based UI` | `## CQRS Pattern: Task-Based UI` | as above |
| `### Step 2: Address Compiler Warnings` | `## Step 2: Address Nullable Compiler Warnings` | which compiler warnings — they are all CS86xx |
| `### Step 3: Update Handler Code` | `## Step 3: Update Handler Code for Nullability` | update it *how* |
| `### Step 4: Update Message Mappers` | `## Step 4: Update Message Mappers for Nullability` | as above |
| `## Performance Implications` | `## Agreement Dispatcher Performance Implications` | performance of what |
| `## Migration Guide: V9 to V10` | `## Polly v8 Migration Guide: V9 to V10` | the corpus has a whole V10 migration section |
| `## Complete Configuration Example` | `## Complete OpenTelemetry Configuration Example` | configuring what |
| `## Distributed Tracing Example` | `## OpenTelemetry Distributed Tracing Example` | whose tracing |
| `## Configuring OpenTelemetry` | `## Setting Up OpenTelemetry` | collided with the page's own H1 — see above |
| `### Pattern: Simple CQRS (Same Database)` | *(promoted only)* | already names CQRS |
| `## Standard vs Agreement Dispatcher Routing`, `## Agreement Dispatcher Use Cases`, `## Agreement Dispatcher Limitations` | *(unchanged)* | Spec 011 already qualified all three |
| `## Legacy: Using Polly v7 Policies (Deprecated)` | *(unchanged)* | names its subject, and leaving it keeps the slug |

The qualifier is each page's own established form — `CQRS Pattern: …`, `Agreement Dispatcher …`,
`OpenTelemetry …` — so the new pages read as the same family as the cores they came from.

#### One correctness fix, and it was created by the split

`PolicyRetryAndCircuitBreaker.md`'s *TimeoutPolicy Obsolete Warning* ended
*"…using Polly's Timeout strategy (see migration guide above)"*. After the cut there is no
migration guide above; it is on another page. Corrected in place to link
`MigratingToPollyV8.md`, under the standing line on conflicting copies — a page that would
otherwise ship, in the same PR, a claim contradicted by another page in that PR is corrected.
This is the same shape as Phase 6's repointed link whose *text* named the page it no longer went
to. Every other `above`/`below` in both pages was checked and every one still resolves:
`MigratingToPollyV8.md:149` and `:175` are internal to the moved section, and
`PolicyRetryAndCircuitBreaker.md:340` points at examples the core keeps.

#### D5: 13 lines across five invocations, twelve of them headings

| Task | Lines | Made of |
|---|---:|---|
| 9.1 | 4 | 1 `##` dropped into the H1 · 3 requalified |
| 9.2 | 4 | 1 `##` dropped into the H1 · 3 requalified |
| 9.3 | 1 | 1 requalified `##` |
| 9.4 | 2 | 1 requalified `##` · **1 prose line, the correctness fix above** |
| 9.5 | 2 | 2 requalified `##` |

**Not one line of code survives nowhere, and the single prose line is the one this write-up
names.** `## Setting Up OpenTelemetry` does not appear in the 9.5 list — the new page's H1
carries the original text and `noloss.py` matched it on its anchor — which is the check
confirming the H1/H2 collision was resolved by renaming the section, not by dropping it.

#### `pagelint.py --changed` bit for the fourth phase running, and hard

**Fifty-one errors**, all `USING DIRECTIVES`, all on the five new pages — 22 / 12 / 11 / 4 / 2 —
because every line of a new page is an added line and every C# block in it is therefore strict.
The largest count this gate has produced, and again **proved live rather than by probe**: red
with the files staged, green after the markers. Under obligation 6 and Appendix C every one is
**marked `// ...`, not guessed at** — the blocks carry real Brighter, Polly and OpenTelemetry
types whose namespaces were not checked. Repo-wide debt is unchanged at **791 blocks**, the
marker downgrading and never silencing, spread from 113 pages to **118**.

**The files were staged before the run.** `git diff` cannot see untracked files, and an unstaged
run would have reported 0 errors on exactly the five pages the phase created.

#### Recorded, and not acted on

- **`CQRSWithBrighterAndDarker.md` keeps `## Example: E-Commerce Order System` (226 lines)** and
  lands at **936**, the largest page left after this spec. Requirements §8 is binding; it drops
  to ~710 when Spec 009 takes the example. **Flagged, not moved.**
- **`AgreementDispatcherRouting.md` carries `### Agreement Dispatcher Routing`**, the same text
  as its own H1, at the point where the source contrasted it with `### Standard 1-to-1 Routing
  (Default)`. Rule 3b does not reach an H1 and the contrast is the section's whole point, so the
  text moved verbatim under obligation 1.
- **`MigratingToPollyV8.md` holds both tails**, as design §7.5 requires: the four migration steps
  and the deprecated Polly v7 attribute a reader is migrating *from*.
- **`QueryPipeline.md`'s `Available Decorators`** and **`AsyncAPISupport.md`'s
  `Complete Examples`** remain design §11's recorded non-actions. Phase 9 does not touch them.

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

### Phase 10, the investigation — 2026-08-12

**Tasks 10.1 and 10.2 are researched, 10.3 is ruled, and 10.4 is measured.** What remains is
one live probe, PR'd below, and the implementation the ruling leaves.

**§3's endpoint table re-measured today, after Phase 9.** It has moved, and the lesson that
says so is *re-measure at the ref you are cutting from*:

| | 2026-08-08 | 2026-08-12 |
|---|---:|---:|
| `/llms.txt` bytes | 27,470 | **32,792** |
| link entries | 170 | **202** |
| ours | 111 | **143** — 142 `contents/` pages + `README` |
| V9 space | 59 | **59** |
| entries carrying a description | 0 | **0** |

**All 32 pages Phases 4 through 9 created are already indexed**, hours after PR #95 merged, so
the platform's index tracks `SUMMARY.md` automatically and *master's Git Sync is provably
live today*. The file has also **gained a trailer** since 2026-08-08 — `# Agent Instructions`
and `## Querying This Documentation`, documenting the `?ask=` endpoint. Nobody configured that
either.

**Task 10.1 — the mechanism exists, and it is not on the page the spec had been reading.**
`Content configuration` does not document front matter at all, which is why §3 could not find
it. The authority is **GitBook's own skill bundle**, `GitbookIO/gitbook-skills`, at
`skills/write-docs/references/frontmatter.md` — fetched raw from GitHub, and published as
`gitbook.com/docs/skill/write-docs`:

- **`description:` in YAML front matter is a supported page field** on Git-synced markdown,
  alongside `icon:`, `hidden:`, `vars:`, `if:`, `cover:` and `layout:`.
- **`layout.description.visible` is a separate boolean** controlling whether the description
  *renders into the page body*. That is precisely the axis 011's front-matter ruling turned
  on: **GitbookIO/gitbook#1079 was about metadata rendering into the body, and the platform
  now has a switch for exactly that.** Quote the whole rule, not the sentence that scared you.
- **The gotcha is GitBook's own, and it is silent.** An unquoted `description:` containing
  `:`, `#`, `[`, `]`, `{`, `}`, `&`, `*`, `!`, `|`, `>`, `'`, `"`, `%`, `@` or `` ` `` causes
  *"silent failures in Git Sync — the page imports without a title or description with no
  error message."* Our summaries are sentences and sentences carry colons. **Quote every
  value, always**, and note that this failure mode is invisible to `pagelint.py`,
  `linkcheck.py` and `urlmap.py` alike — the page still renders, just wrong.

**What is documented is not measured.** Nothing establishes that the field reaches *our*
generated `/llms.txt`, and *a vendor's own example can be the bug*. Hence the probe.

**Task 10.2 — the V9 space is this repository, on the `v9` branch.** Measured, not assumed:
`Docs@v9` at `0548c8f`, last committed 2025-10-31, carries **59 markdown files under
`contents/`** — exactly the 59 entries in the index — and the published
`…/v9-paramore-brighter-documentation/overview/basicconcepts.md` reproduces the branch source
byte for byte, `# Basic Concepts` straight into `## Command`, with no banner. So the content
**is ours to edit**, and the discriminator can be put where the problem is.

- **There is no exclusion mechanism, and that is now checked rather than assumed.** Confirmed
  against `llm-ready-docs`, `site-sections`, `seo`, and GitBook's own `?ask=` endpoint:
  inclusion follows *published*, with no per-space or per-section toggle. **Hiding does not
  help** — GitBook documents that hidden pages stay in `llms-full.txt` and in the MCP server.
- **The fix is therefore at source, not at the index**, which is the same shape as the ruling
  Task 10.3 took: give the 59 V9 pages a `description:` that names the version, so the
  discriminator lands *in the index line itself* — the exact place a retrieval client reads.
- **Incidental, and a real defect: `v9`'s `.gitbook.yaml` still carries the contamination.**
  77 bytes, **2 × U+200B**, in `structure:` and after `SUMMARY.md` — the vendor bug
  requirements §2.4 recorded and session 7's `6661073` fixed on `master`. `master`'s is
  **7,858 bytes and pure ASCII**; `v9`'s never was. Nothing visibly broke, because both keys
  hold their default values — which is exactly how it survived on `master` too.

> **The vendor bug is worse than recorded, and still live.** Requirements §2.4 counts **two**
> U+200B in GitBook's *Content configuration* page. Fetched 2026-08-12 it holds **six U+200B
> and six U+200C** — twelve zero-width characters, across the `.gitbook.yaml` block, the
> `### Structure` heading *and* the `SUMMARY.md` example block, which requirements §2.4 does
> not mention at all. **Type config; never paste it** — and the blast radius is larger than
> the one block we knew about.

**Task 10.3 — ruled by the maintainer, 2026-08-12: fix it at source.** Of the three live
options, *build ours anyway* and *drop the generator* were both declined. Descriptions are set
on the pages, the V9 pages gain version-marked ones, and **GitBook's canonical `/llms.txt`
becomes the deliverable** rather than a GitHub-only file competing with it. **AC9 must narrow
to match** — from *"`llms.txt` covers every page with type and one-line summary, generated not
hand-written"* to descriptions set at source and carried by the canonical index. That
amendment lands with the probe's result, not before it: **AC9 is not narrowed on a mechanism
nobody has watched work.**

**Task 10.4 — measured today, and design §9.2's prediction held.** Extracting the first
sentence of the first non-blank prose line after the banner, across all 142 pages:

| Test | Failing |
|---|---:|
| no summary extractable | **0** |
| longer than 200 characters | 12 |
| ends in a colon | 2 |
| not unique across pages | 6 pages in 3 groups |
| **distinct failing pages** | **16 of 142** |

*"Expect a double-figure number of failures"* — sixteen. **Every page yields a sentence**,
which is the half that was not certain.

**And on its first run the rule found a defect no other check in this programme can see.**
`AzureBlobConfiguration.md` — the *Azure Blob* archive provider's configuration page — opened
with a paragraph about **Azure Service Bus**, copied verbatim from
`AzureServiceBusConfiguration.md`, where it is correct. It has been wrong since `96d7b546` on
**2023-04-04**. `linkcheck.py`, `pagelint.py`, `--check-shape` and `--check-redirects` are all
green on it and always were: the links resolve, the banner is well-formed, the heading is
unique and qualified, the fences are tagged. **Only the uniqueness half of the opening-sentence
rule disagreed**, and it disagreed because the sentence was byte-identical to another page's.
Checked against `src/Paramore.Brighter.Archive.Azure/AzureBlobArchiveProvider.cs` before it was
rewritten: the provider implements `IAmAnArchiveProvider` and writes to a `BlobContainerClient`.
Nothing to do with Service Bus.

**Two things this raises that are recorded and deliberately not fixed here:**

1. **The 200-character limit does not say whether it measures source or rendered text.** The
   first draft of the replacement sentence came out at **199** characters — one under — and
   would have been about 140 rendered, because the extractor counts `[text](url)` in full. A
   page can fail on the length of a URL nobody reads. The sentence was shortened rather than
   left on the boundary, but **Task 10.4's implementation has to rule on this**, and the answer
   should be *rendered*, because that is what a retrieval client sees.
2. **The page's `## Options` list is incomplete** against the source: `MaxConcurrentUploads`
   (default 8) and `MaxUploadSize` (default 50 MB) are on
   `AzureBlobArchiveProviderOptions` and on no page. **Left for Phase 11**, so the probe PR
   stays legible as a probe. Recorded here rather than fixed, which is the standing line on
   content found in passing.

**The probe, and why it carries a control.** One page, `AzureBlobConfiguration.md`, changed
twice in one commit:

- **The probe:** a quoted `description:` front-matter block, byte-identical to the page's new
  opening sentence with its markdown stripped, 103 characters, pure ASCII.
- **The control:** the wrong intro paragraph replaced with a correct one. **This is what makes
  a negative result readable.** If the description does not appear, a body change that *has*
  appeared proves the deploy landed and the mechanism failed; without it the two are
  indistinguishable, which is the vacuous-pass shape this programme has now met four times.
  Unlike D0's probe key, the control is a fix worth keeping, so nothing has to be reverted.

**All four gates were re-run with front matter present and none of them moved** — linkcheck
144 files, pagelint 0 errors / 791 warnings / 142 pages, `--check-shape` and
`--check-redirects` 0, and `--changed origin/master` 0 with the file staged. `pagelint.py`
finds the H1 by scanning for the first `#` heading outside a fence, so a YAML block above it is
simply prose to every rule. **That is a result in its own right:** the "fix it at source" route
costs nothing in tooling.

**What to do when the probe resolves:** curl `/llms.txt` and grep for the description string.
Then read the page's HTML and its `.md` variant to see whether `layout.description.visible`
defaults to rendering it into the body. Only then narrow AC9, and only then sweep the
remaining 141 pages and the 59 on `v9`.

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
