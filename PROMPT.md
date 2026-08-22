# Resume Prompt: Brighter Docs Session State

**Last Updated**: 2026-08-22 (end of session 22)
**Programme**: Documentation restructure, specs 009–013, responding to [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)
**Active Spec**: **`010-information_architecture`** — requirements APPROVED 2026-08-06,
D0 EXECUTED 2026-08-07, **`design.md` APPROVED 2026-08-08**, **`tasks.md` REVIEWED AND
APPROVED 2026-08-08**, and **PHASES 1 THROUGH 10 ARE COMPLETE.**
All three markers exist: `.requirements-approved`, `.design-approved`, `.tasks-approved`.
Spec 011 is COMPLETE (43/43); spec 005 is COMPLETE (14/14); do not re-open either.

> ## PHASE 10 IS DONE. ONLY PHASE 11 — CLOSE — REMAINS. Read this before anything else.
>
> **47 of 52 tasks are done.** The corpus is **142 pages**; `SUMMARY.md` is the twelve-section
> tree; all 32 new pages landed in Phases 3–9; and Phase 10 shipped in session 22 as PRs
> **#96, #97, #98, #99, #101, #102** on `master` and **#100, #103** on **`v9`**.
>
> - **DO NOT BUILD `tools/llmstxt.py`. D6 IS SUPERSEDED.** GitBook owns `/llms.txt`, generates
>   it from the published tree, and cannot be overridden. **Task 10.3 ruled *fix it at
>   source*** and **AC9 is already narrowed** in requirements §12, design §14 and design §9.2.
> - **Every page carries `description:` front matter**, written by **`pagelint.py --fix`**,
>   which now has a **third repair** for exactly this. A new page gets its description from
>   the tool. **Do not write a sweep script.**
> - **Measured live 2026-08-22: `/llms.txt` is 56,527 bytes; our space 143 of 143 entries carry
>   a description and the V9 space 58 of 58.** Do not re-derive; re-measure only if the tree moves.
> - **`pagelint.py` has a rule 7** — six labels, `SUMMARY *` and `DESCRIPTION *`. The ledger in
>   `CLAUDE.md` lists all of them. **`CLAUDE.md`'s § llms.txt is marked SUPERSEDED and not yet
>   rewritten — that is Phase 11's**, along with everything else below.
> - **The `v9` branch is IN SCOPE and its Git Sync IS LIVE** (proved by the descriptions
>   reaching the index). It syncs the *V9 Paramore Brighter Documentation* section of the same
>   site. Its `SUMMARY.md` now maps **1:1, 57 entries to 57 files**, and its `.gitbook.yaml`
>   is **retyped, 173 bytes, pure ASCII, with one `redirects:` entry.**
> - **The site's base URL is
>   `https://brightercommand.gitbook.io/paramore-brighter-documentation`** — hyphens, one
>   segment.

**Next action: Phase 11 = PR 11, close.** Five tasks — 11.1 Glossary links (24 terms, zero
today), 11.2 the trailing-newline sweep (**re-derive the count**), 11.3 echo the changed-range
count in `docs.yml`, 11.4 the acceptance pass AC1–AC9, 11.5 the final gate.

> **Phase 11 has gained four items from session 22. None is optional; all are recorded in
> `tasks.md`.**
>
> 1. **`CLAUDE.md`'s § llms.txt describes a file this spec does not build.** It carries a
>    SUPERSEDED note. Rewrite it to the platform's actual format, `- [Title](url): description`,
>    **with no type field**.
> 2. **25 internal `.html` links across ~15 pages, and `linkcheck.py` cannot see one of them**
>    — they are not `.md`, so it skips them. **One target, `QualityOfServicePatterns.html`, has
>    no page at all**: a dead link green in every CI run this repo has done. The rewrite is
>    **not** mechanical — the file is `CommandsCommandDispatcher*a*ndProcessor.md`, so the
>    obvious conversion lands on **WRONG CASE**, and some anchors never existed
>    (`#command-processor` is really `#the-command-processor-pattern`). **Consider teaching
>    `linkcheck.py` to see them, or they will come back.**
> 3. **`BuildingAnAsyncPipeline.md` has two H1s**, lines 1 and 8. No rule sees it: `NO H1`
>    fires only on *no* H1 and rule 3b starts at `##`. Nothing links the second one's anchor,
>    so demoting is safe — but it then becomes a `##` that must be unique and qualified.
> 4. **`README.md`'s front matter is enforced by nothing**, because `pagelint.py` reads
>    `contents/` only. It is correct today and no rule will notice if it stops being.

**Do not re-do any of this**: D0 (session 11), the depth probe (session 14, design §17),
Appendix A (measured, session 21), Phases 2–9 (sessions 15–21), or **Phase 10 (session 22 —
the mechanism, the ruling, rule 7, the 142-page sweep, and the `v9` work)**.

**Read, in this order:** `tasks.md` **§3** (what the platform ships), then ***Phase 10, the
investigation***, ***Task 10.4 as executed***, ***The sweep as executed*** and ***Phase 10
closed*** — the last four are session 22 and contain every finding above. Then §1's **seven**
standing obligations, then **Phase 11's five tasks**. Then `design.md` **§9.2** (superseded in
mechanism, vindicated in rule) and **§14** (AC by AC; AC9 already narrowed).
**Phases 4 through 9 as executed are history; read them only if you are splitting a page in
012 or 013**, and then read Phase 6's first and Phase 9's second.

**Branch**: **`master`, clean, nothing in flight.** Head **`3178d68`**. `v9` at **`bb44d98`**.
All five gates green: linkcheck **144** files, pagelint 0 errors / **791** warnings,
`--check-shape` 0 at **142 pages / 12 sections / deepest 4 / widest 10**, `--check-redirects`
0 at **77** entries, `--changed origin/master` 0. `pagetypes.tsv` is **142 rows** and parity
is **142/142, checked both directions**.

> **Six things Phases 6 through 9 proved, kept for whoever splits a page next.** No phase in
> this spec splits anything again; 012 and 013 will.
>
> 1. **Grep each source for `](#` as well as grepping the corpus for the page** — and then
>    grep again *after*. Phases 6, 7 and 8 each found three anchors before a line moved.
>    **Phase 9's five sources carried none at all**, which is a fact you establish rather than
>    assume — it is the reason that phase had no anchor work.
> 2. **Appendix B is indexed by *target*, and a split can move the *source* line too.** Phase 6's
>    one inbound anchor had **both ends move in the same PR**. **Phase 8 met the benign form**:
>    two same-page anchors whose source line *and* target heading moved together, so one stayed
>    a working anchor untouched and the other needed a repoint only because its heading was
>    requalified. Read from Appendix B alone, both looked like nothing to do.
> 3. **Check the promoted headings for uniqueness, then read them for attribution.** Phase 6's
>    35 promotions collided **zero** times and most still needed requalifying. Phase 8's six
>    collided zero times and five needed it. **Phase 9's eleven collided zero times and eight
>    needed it.** **Four phases running** — uniqueness has a tool, attribution does not.
> 4. **A heading nothing needs requalified is a heading whose slug you get to keep.** Phase 7
>    left `### Message Transformer Factory` alone, Phase 8 `### Migrating from AWS SDK v3 to v4`,
>    and both turned three repoints into a pure path change. **Phase 9 is the counter-case**:
>    `## Migration Guide: V9 to V10` had to be requalified, so its one repoint changed path
>    *and* fragment.
> 5. **Re-measure the source at the ref you are cutting from, not just the total.** Phase 8's
>    row said 662 and 662 was right *when written*; PR #90 added a line months later. Phase 9
>    checked all five sources first and **all five reproduced** — the check is cheap and the
>    failure is silent.
> 6. **Verify the defect in the direction the claim makes.** Phase 9's Task 9.6 prescribed a
>    *"missing reverse pointer"* that had existed since the page was created. Both pages linked
>    each other, so `grep -l` on the pair said yes — to a different question. **A one-directional
>    claim needs a one-directional grep.**
>
> **Carry the three lines Phases 4, 5 and 6 drew:**
>
> - **On conflicting copies:** a merge that forces a choice resolves **to the source**; a page
>   that would otherwise ship, in the same PR, a claim contradicting another page in that PR is
>   **corrected**; everything else **moves verbatim and is recorded**. Phase 9's one correctness
>   fix is this line: *"see migration guide above"* had no above once the guide moved.
> - **On misfiled content:** do not *newly* create a misfiling — Task 5.1's three Outbox H3s
>   stayed in the core rather than riding onto the Archiver page — but do not silently delete
>   content either. Where a block is already misfiled and only its page changes, move it and
>   record it.
> - **On the shape of a split page** (Phase 6 settled it, Phase 9 extended it): a page built from
>   **one** `##` section takes that heading as its **H1**, drops it from the body, and **promotes
>   its `###` to `##`**, requalified — and **if there are `####` beneath, promote the whole
>   subtree by one**, or you leave a skipped level. A page built from **several** keeps them as
>   `##`. And a **written lead-in only where the moved section brought no intro of its own** —
>   otherwise the page opens twice. **Watch for the H1 colliding with a section title**:
>   `pagelint.py` cannot see it, because rule 3b starts at `##`.

> **`--admin` is authorised, standing, from 2026-08-09.** `master` requires a review and
> GitHub blocks self-approval, so every merge needs `gh pr merge <n> --merge --admin`. The
> classifier treats the **review bypass** as separate from the merge, and a bare *"let's
> merge"* once got the call denied. The maintainer has now granted the flag outright — **do
> not ask about it again.** **Still ask before merging anything that changes the published
> site**; one authorisation covers one PR. One PR per coherent unit, merged before the next
> branch starts.

**Do not re-do any of this**: D0 (session 11), the depth probe (session 14, design §17),
the design's figures (session 13 re-derived every one), Appendix A (session 14 measured it,
session 15 re-derived all 32 rows and all 12 entry counts, **session 21 confirmed all twelve
section rows against the finished tree**), or Phases 2–9 (sessions 15–21 shipped them —
**`noloss.py` is written, proved red four ways, and every split it was written for has run**).

**Read, in this order:** `tasks.md` **§3 first** — what the platform already ships, because it
**re-scopes D6 and Phase 10 is next** — then **§4** (the tasks review's five findings), then
**Phase 9 as executed** (the defect that was not there, the `####` case, and the H1/H2 collision
the linter cannot see), then **Phase 8 as executed** (the source measurement a later PR
overtook, and the anchor pair that moved together), then §1's **seven** standing obligations
(obligation 7 covers `pagetypes.tsv`), then **Phase 10's four tasks** and **Phase 11's five**.
Then `design.md` **§9.2** (D6 as designed, now partly superseded by §3), **§14** (how each AC is
met — AC9 is the one that must narrow), **§17** (S3 at four segments) and **§16** (the design
review's five findings). **Phases 4 through 9 as executed are history now; read them only if
you are splitting a page in 012 or 013**, and if you are, read Phase 6's first (it settled the
shape rule) and Phase 9's second (it extended it).

**Branch**: **`master`, clean, nothing in flight.** Head **`e7c9d71`** — PR #95 merged
2026-08-12. All gates green: linkcheck **144** files, pagelint 0 errors / **791** warnings, `--check-shape` 0 at **142
pages / 12 sections / deepest 4 / widest 10**, `--check-redirects` 0 at **77** entries,
`--changed origin/master` 0 with the five new files staged. `pagetypes.tsv` is **142 rows** and
banner parity is **142/142**, checked both directions.

**Base**: **everything is merged; nothing is outstanding.** PR #74 (011 Phases 1–6 + Task 7.1)
→ `b5500bb`; #75 → `61a83ed`; #76 (011 Phase 7) → `4001b0a`; **#77/#78/#79 (D0)** → `06c9e62`;
**#80** (010's requirements) → `7d774d2`; **#81** (005 closed) → `c4aedb5`; **#83/#84** (the
depth probe and its revert, net-zero) → `2c8c4aa`; **#82** (010's design + reviewed tasks) →
`25a578c`; **#85 (PHASE 2 — the tree, the redirects, urlmap in CI)** → `1bae048`; **#86**
(Phase 2's post-merge measurements) → `9ecae58`; **#87 (PHASE 3 — the three content defects
and `noloss.py`)** → `438166b`; **#88 (PHASE 4 — the scheduler family)** → `8d0b5a4`;
**#89 (PHASE 5 — Outbox and Inbox)** → `5e1dd81`; **#91** (Phase 5's post-merge measurements)
→ `643562f`; **#90** (the scheduling-overload fix, ten calls across three pages) → `a12d5d9`;
**#92 (PHASE 6 — Darker, 12 new pages, no URL moves)** → `47e03b0`;
**#93 (PHASE 7 — Using an External Bus, 3 new pages, no URL moves)** → `7810f38`;
**#94 (PHASE 8 — Transports, 2 new pages, no URL moves)** → `3f0f4dd`.
**#95 (PHASE 9 — the rest of §6d, 5 new pages, no URL moves)** → `e7c9d71`.

**Repo**: `Docs` (GitBook source for Brighter/Darker)

> **The linter is green and the corpus is why.** Every page carries a banner, no `##`
> heading text repeats across pages outside the navigation allowlist, no heading repeats
> within a page, and "ServiceActivator" is gone from prose. That is 324 errors taken to
> zero across two sessions, all of it in `contents/`.
>
> **Session 4 — the banner (8 commits, pushed):**
>
> - **`021b5d2`** — `pagetypes.tsv` generated by `proposetypes.py` (Task 3.1)
> - **`5dde877`** — the 29 unproposed rows ruled on; **the `BasicConcepts` → `Glossary`
>   merge withdrawn** (see *Decisions already made*)
> - **`fe421e0`** — all 105 verdicts filled (Task 3.2)
> - **`726eb67`** — `apply_banners.py` and the `applies` column (Task 3.3)
> - **`de19283`** — **the sweep: 105 files, 212 insertions, 0 deletions** (Task 3.4)
> - **`deb837b`** — what the sweep nearly carried, recorded
> - **`9d670f8`** — two banners corrected after a Brighter/Darker parallelism check
> - **`3b57017`** — **the banner vocabulary corrected: there is no Darker V10**
>
> **Session 5 — rule 5 and all of Phase 4 (7 commits; pushed in session 6):**
>
> - **`741890f`** — rule 5's 28 terminology findings cleared → 0
> - **`8aa54fb`** — how they were cleared, and the rule-5 gap it exposed
> - **`d232324`** — Task 4.5: the three "duplicate content" defects, **none of which
>   were duplicate content** (see *Suggested next session*)
> - **`64dd111`** — Task 4.3: 31 within-page headings qualified → rule 3b at 0
> - **`0b1b841`** — Tasks 4.1–4.4: **260 headings + 19 links across 74 files** → rule 3a
>   at 0, and `pagelint.py` to 0 errors overall
> - **`a9c9503`** — Phase 4 recorded; `qualify.py` and `dedupe_within.py` kept
> - **`e88ac82`** — the maintainer's ruling to leave rule 5 as a prose rule
>
> **Session 6 — Phase 5, the gate (4 commits, pushed):**
>
> - **`492546b`** — Task 5.1: `pagelint.py` in `docs.yml`, repo-wide + PR-only `--changed`
> - **`56106a6`** — Task 5.3: daily `schedule:` and the guarded `versions` job for 009's D9
> - **`04de6d3`** — `base_ref` moved off the command line into `env`
> - **`e9a9c43`** — Phase 5 recorded; **AC6 closed**
>
> **Session 7 — all of Phase 6, both splits (5 commits, pushed):**
>
> - **`e0faceb`** — rule 6 gains the `// ...` escape it had always advertised
> - **`758f391`** — Tasks 6.1–6.6: `RabbitMQConfiguration.md` 566 → 331 + 3 pages
> - **`df2d3a5`** — Tasks 6.7–6.11: `BrighterBasicConfiguration.md` 1,070 → 237 + 2 pages
> - **`6661073`** — Task 6.12: `.gitbook.yaml` fixed, no redirect needed; Phase 6 recorded
> - **`5498cd6`** — `apply_banners.py` no longer eats prerequisites; the 5 new pages
>   added to `pagetypes.tsv`
>
> **Session 8 — the worklist, the render check, and two merges (2 commits):**
>
> - **`a48e8ef`** — Task 7.1: `worklist.md`, 42 rows, **Spec 010 unblocked**
> - **`fdfc27e`** — Task 3.5: the banner confirmed rendering as a `<blockquote>` callout
>
> **Session 9 — all of Phase 7; SPEC 011 CLOSED (3 commits, merged as PR #76):**
>
> - **`841b949`** — Task 7.2: the last 34 fences tagged, rule 4 to a repo-wide error
> - **`5640a0c`** — Task 7.3: `pagelint.py --fix`, rehearsed against a real V11 bump
> - **`957811d`** — Task 7.4: the acceptance pass; **AC5 failed and was fixed**
>
> **Session 10 — Spec 010 opened; requirements written, NOT approved (2 commits,
> unmerged on `docs/spec-010-requirements`):**
>
> - **`0ee2f11`** — `requirements.md` + `urlmap.py`; **the URL model verified 110/110**
> - **`793c629`** — Q7, the scope question (26 splits in one spec) stated rather than left
>   for the reviewer to find
>
> **Session 11 — 010's requirements REVIEWED AND APPROVED (1 commit, unmerged):**
>
> - **`c72b01d`** (now `cc4653c` after rebase) — **Q1 answered from the documentation, and
>   the risk that replaces it.** Every load-bearing figure re-derived and **all held — the
>   first review in this programme to find no wrong number.** `.requirements-approved` created
> - **`03a5100`** — **D0 as executed.** Requirements §16
>
> **Session 11 also ran D0 on the live site — three PRs, all merged to `master`:**
>
> - **PR #77** (`1cdf07d`) — the section rename, **deliberately with no redirect**
> - **PR #78** — the `redirects:` block, with the probe key that was the only thing
>   capable of proving it works
> - **PR #79** (`06c9e62`) — probe key removed, both mechanisms measured
> - **PR #80** (`7d774d2`) — the four spec-document commits, including §16 *D0 as executed*
> - **PR #81** (`c4aedb5`) — **spec 005 closed at 14/14**; Task 3.3 was a contingency that
>   never fired, and two stale totals were corrected on the way out
>
> **Session 12 — 010's design WRITTEN (2 commits; merged in PR #82):**
>
> - **`84af62b`** — twelve sections, 74 URLs moved, 32 pages from 26 splits
> - **`fe8e489`** — page length is not an acceptance criterion, and the sixteen pages that
>   prove it
>
> **Session 13 — 010's design REVIEWED AND APPROVED (1 commit, unmerged):**
>
> - **`92c58ba`** — **five findings, every one a tally; no verdict, threshold or ruling
>   moved.** The counting convention corrected (23 line counts), the scheduler fold-up
>   96 → 85 and 741 → 729 with the ruling intact, §4's entry count 10 → 8, §15 gains the
>   entries-column admission, D7 ratified. `.design-approved` created
>
> **Session 14 — 010's TASKS written, and the first rule in this programme to move
> (2 commits on the branch, unmerged; 2 merged to `master` and cancelled):**
>
> - **`27a4c42`** — `tasks.md`: **52 tasks, 11 phases, Phase N = PR N.** Appendix A pins
>   where each of the 32 new pages nests; Appendix B the anchor obligations per task;
>   Appendix C what each phase must not do
> - **PR #83 → PR #84** — **the depth probe, published and reverted.** `master` ends
>   byte-identical to where it started
> - **`a91aa0a`** — **S3 amended from ≤3 to ≤4 segments, measured.** design §17 records it;
>   `MigratingToPollyV8.md` and the Azure Blob pair re-placed
>
> **Sessions 4–9 are all on `master` now**, via PR #74 (`b5500bb`), PR #75
> (`61a83ed`) and PR #76 (`4001b0a`). **EVERYTHING THROUGH SESSION 19 IS MERGED.** Sessions
> 10–11 landed as PRs #77–#81; sessions 12–14 as PR #82 (`25a578c`); session 15 as PRs #85
> (`1bae048`, **Phase 2**) and #86 (`9ecae58`); session 16 as #87 (`438166b`, **Phase 3**);
> session 17 as #88 (`8d0b5a4`, **Phase 4**), #89 (`5e1dd81`, **Phase 5**), #91 (`643562f`)
> and #90 (`a12d5d9`); session 18 as **#92** (`47e03b0`, **Phase 6**); session 19 as **#93** (`7810f38`,
**Phase 7**). From session 4:
> `118c889` the repo's first CI,
> `deee51d` `CLAUDE.md`'s *Page Conventions* + amended pattern, `335f078` `pagelint.py`
> and its measured baseline.
>
> `linkcheck.py` clean throughout (now `122 files checked`). **`pagelint.py` is in
> `docs.yml`, proven to fail the build, and `--changed` has now produced a real error on a
> real page** — Phase 4's red-proof, after three phases of vacuous passes. See *What session
> 6 established* and *What session 17 established*.

> **Purpose**: Local working notes so a fresh Claude Code session can resume without
> re-reading the whole conversation. Gitignored — do not commit.

## How to resume

1. Read this file end-to-end.
2. Read the README of the spec you're picking up — the rationale lives there and is
   **not** repeated here.
3. **You are picking up Spec 010 after its LAST CONTENT PHASE: Phases 1 through 9 are done,
   and Phase 9 shipped as PR #95.** What remains is Phase 10 (`llms.txt`) and Phase 11 (close).
   **Do not split another page.** Read
   `spec/010-information_architecture/tasks.md` first — **§3 before anything else**, because it
   is the measurement that **re-scopes D6** and D6 is what Phase 10 builds. Then **§4** (the
   tasks review's five findings), then **Phase 9 as executed** (the defect that was not there,
   the `####` case, and the H1/H2 collision the linter cannot see), then §1's **seven** standing
   obligations (7 covers `pagetypes.tsv`), then §2 (the seven items design left open, all
   discharged), then **Phase 10's four tasks** and **Phase 11's five**, then **Appendix A**
   (**now measured against the finished tree — all 32 pages exist and all twelve section rows
   reproduce; do not re-derive it**) and Appendix C (what each phase must not do).
   **Phases 4 through 8 as executed are history**; read them only if you are splitting a page in
   012 or 013, and then read Phase 6's first (it settled the shape rule) and Phase 9's second
   (it extended it). Appendix B is spent — every anchor it tracked has been repointed.
   Then `spec/010-information_architecture/design.md` — **§9.2** (D6 as designed, partly
   superseded by §3), **§14** (AC by AC; **AC9 must narrow if D6 narrows**), **§17 before §16**
   (§17 is the S3 measurement and supersedes §4's threshold and §7.6's placement reasoning; §16
   is the review's five findings), then §10 (the eleven-PR plan the tasks execute) and §11
   (recorded, deliberately not acted on — one of its four entries was corrected at execution).
   Then `spec/010-information_architecture/requirements.md` — it is **approved**, and
   its §§2.2–2.4, §15 and §16 are the parts that changed at review. **`worklist.md` is spent
   too**: all 26 splits it scored have been executed or explicitly kept.
   **011 itself is finished; do not re-open it.**
   Only if you need 011's history: `classification-notes.md`, and `tasks.md`
   §§ *Measured baseline*, *Phase 4/6 as executed* and *Tasks 7.1–7.4 as executed*.
4. Confirm branch state: `git status`, `git log --oneline -5`. Expect **`master` at `e7c9d71`,
   clean, nothing in flight.** **All three markers exist** —
   `.requirements-approved`, `.design-approved`, `.tasks-approved`. **Do not re-run D0**
   (PRs #77–#79), **do not re-run the depth probe** (#83/#84 — design §17 has the result),
   **do not re-do Phase 2** (#85/#86 — `SUMMARY.md` *is* the new tree already), and **do not
   re-do Phases 3 through 9** (#87, #88, #89/#91, #92, #93, #94, #95).
5. Run all five gates before touching anything, so you know the starting numbers:

   ```bash
   python3 tools/linkcheck.py            # No broken internal links (144 files checked).
   python3 tools/pagelint.py             # 0 errors, 791 warnings (791 blocks / 118 pages), 142 pages
   python3 tools/urlmap.py --check-shape # 0 failures — 142 pages, 12 sections, deepest 4, widest 10
   python3 tools/urlmap.py --check-redirects  # 0 failures — 77 entries, 7858 bytes, printable ASCII
   python3 tools/pagelint.py --changed origin/master   # 0 errors — and STAGE new files first
   ```

   Those are the figures after Phase 9, with `pagetypes.tsv` at **142 rows** and banner parity
   **142/142**.

   The pagelint warning count is *exactly* the using-directive debt — the 34 language-tag
   warnings became errors and were cleared in session 9. **Anything else means the tree
   moved.** `--check-shape` and `--check-redirects` gate CI; `--verify` is deliberately
   **not** in CI, and exits 2 rather than passing if the site is unreachable. **`--changed`
   only sees staged files** — `git add` before you trust it.
6. Start at "Suggested next session" below.

**Forty-four things this programme has learned the hard way**, all of which cost a rework.
**Enumerate this list; do not read its heading** — the count below was **one low** from before
session 17 until 2026-08-11, and every increment since preserved the error, because each was
`previous heading + n` rather than a re-derivation. It is a lesson in this very
list, committed against the list itself. **Forty-four was re-derived by running the command
below, not by adding seven to thirty-seven.** Re-derive with:

```bash
awk '/things this programme has learned/,/^## Where things stand/' PROMPT.md | grep -c '^- \*\*'
```



- **A rule can be green and reading the wrong input, and proving it fires does not prove it
  reads.** `pagelint.py` rule 7 passed **0 errors across 142 pages** the day after its four
  branches were each forced red on purpose. It was **wrong about 20 of them**: the extractor
  read one *line*, this corpus hard-wraps its prose, and twenty summaries were truncated
  mid-sentence — four mid-link, one to *"We use [Command-Query"*. **Every fragment passed**,
  because a fragment is short, unique and free of a trailing colon. Joining the paragraph
  immediately surfaced **six genuine over-length findings the truncation had hidden**, which
  is the measure of the vacuity. Found by **dumping all 142 extracted sentences and reading
  them**, not by any check. The programme already knew *a check that passes has not
  necessarily checked anything*; the new half is that **a red-proof answers "does this rule
  fire", never "is this rule looking at the right thing"** — and only the data answers the
  second.

- **"Assert that your mutation landed" is not strong enough: assert it produced the input the
  branch rejects.** Three of rule 7's red-proofs reported SILENT and **all three were the
  probe's fault**. Each asserted `text != orig`, which was true, while producing something the
  branch was never meant to reject: replacing one sentence left the next one on the line, so
  it no longer ended in a colon; deleting the intro merely promoted the following paragraph;
  the "too long" string was still under 200 rendered. **Three correct rules would have been
  recorded as broken.** The fix is to call the extractor directly and assert the property
  *before* running the tool.

- **A check can report its own success as a failure, and only a red baseline shows it.**
  `front_matter_description()` returned `(value, lineno)` into a caller reading the second slot
  as a *reason*, so a perfectly correct page printed `DESCRIPTION UNREADABLE: 2` — a line
  number for a message. Visible only because the red-proof **printed a baseline first and the
  baseline was red**. A probe that starts at its first mutation sees three FIREDs and calls it
  proven.

- **A preview environment does not regenerate everything, and the un-regenerated artefact
  looks like a negative result.** GitBook builds a preview revision per PR, which made a
  merge-and-hope into a measurement — but its `/llms.txt` is **the live index with one URL
  rewritten**, not a rebuild. Probing it for a description that had not shipped returned
  nothing, which reads exactly like *the mechanism does not work*. **Proved otherwise by
  bytes**: preview minus exactly one 33-character revision segment is byte-identical to live.
  **Before reading a probe's silence as evidence, establish that the thing you probed is
  capable of showing the answer.**

- **A platform default can duplicate your content, and the switch is only obvious afterwards.**
  `layout.description.visible` defaults to **visible**, so GitBook renders a page's
  `description:` as a subtitle under the H1. A sweep deriving each description from the page's
  own opening sentence would have made **all 142 pages open twice** — design §7's *"otherwise
  the page opens twice"*, arriving from a direction no rule was watching: not a lead-in an
  author wrote, but a duplicate the platform renders. **Measure what a new field does to the
  page, not only to the index.**

- **The exception read as the rule — the second rule in this programme to move, and both moved
  the same way.** GitBook was said to slug a page from its `SUMMARY.md` title, so repointing an
  entry at a real file would fill the existing URL without moving it. **It moved.** The slug
  comes from the **filename**, and falls back to the title **only when no file exists** — which
  is exactly why seven fileless entries carried title-shaped slugs while fifty-one real pages
  carried filename-shaped ones. **The evidence was in a URL list printed before the prediction
  was made**, both shapes adjacent. Same shape as S3's *"three is the deepest the live site is
  known to work at"*: **a property of our own data mistaken for a property of the platform.**

- **A sentence promoted to a description gets read properly for the first time.** Two
  opening lines that had sat unread for years — *"the component in the assembly orchestrates
  your Performers"* and *"a Command does not have return value"* — were about to become two
  pages' public `<meta name="description">`. Neither is a defect any tool here can see. **The
  same pass found `AzureBlobConfiguration.md` opening with an Azure *Service Bus* paragraph,
  wrong since 2023-04-04 and green under every check**, and `Logging.md` published with a body
  consisting of the word `TODO`. **Changing what a line is *for* is the cheapest review a
  corpus ever gets.**

- **A claim about a *direction* needs a grep in that direction, and `grep -l` on the pair
  answers a different question.** Task 9.6 and design §11 both stated that *"today only
  `ErrorHandlingOptions.md` points at `HandlerFailure.md`"* and prescribed adding *the missing
  reverse pointer*. **`HandlerFailure.md` has linked `ErrorHandlingOptions.md` since `ac0c727`
  created the page on 2026-02-23** — five links then, six now — and the `SUMMARY.md` nesting the
  task also asked for had landed in PR 2. **Two of the task's three deliverables were already
  true**, and it reduced to the banner segment. Nothing was destroyed, because the remedy was
  additive; had it been *"delete the duplicate"* it would have been Task 4.5 again. What let it
  survive design, a design review, a tasks phase and a tasks review is that **both pages do link
  to each other**, so every cheap check says yes: `grep -rl` on the pair, `linkcheck.py`, and
  reading either page. Only `grep -n 'ErrorHandlingOptions' HandlerFailure.md` — the asymmetric
  one — disagrees. **This is the fourth face of "verify the defect exists before fixing it"**:
  verify the defect (Task 4.5), verify what the defect *is*
  (`HowServiceActivatorWorks.md`), verify the *fix* (Task 7.1), and now **verify the
  *direction*.**

- **A figure can be right when it is written and wrong when you use it, and nothing in this
  programme was watching for that.** Task 8.2 records `PostgreSQLMessageBroker.md` at **662
  lines**. It is **663**, and 662 was correct at `25a578c`, the commit that approved the tasks:
  **PR #90** — the scheduling-overload fix, which belonged to no phase — added one `// ...` line
  to `## Scheduled Messages` four days later. Traced ref by ref rather than assumed: 662 at
  `25a578c`, 662 at `f9f042d~1`, **663** at `f9f042d` and today. Nothing broke; the section was
  one the split keeps, every quoted span still reproduced, and the core budget moved 552 → 553.
  **All eighteen previously wrong figures in this programme were wrong when they were written**
  — bad tallies, a bad line-counting call, a number true of a neighbouring quantity. This one
  was *right*, and the corpus moved underneath it. **So "re-derive a total; never add to one" is
  not enough: re-measure the source at the ref you are cutting from.** A plan that ships months
  after it was approved is measuring a different file, and the phases most exposed are the ones
  that waited longest — **which is Phase 9, the last of them.**

- **A ruling can be right about the defect and wrong about the remedy, and the remedy is what
  you ship.** Task 7.1 carries **the one piece of new prose this entire spec authors**, and its
  ruling was explicit: the transforms page *must state that transforms require a custom mapper*.
  The ruling is right that the corpus is unclear. Written from the ruling alone the sentence
  would have been **false**: `JsonMessageMapper<TRequest>`, the default mapper, already carries
  **`[CloudEvents(0)]`**, which *is* a `WrapWithAttribute` — so default mappers **do** run a
  transform. What is true is narrower and had to be read out of
  `TransformPipelineBuilder.BuildWrapPipeline`, which discovers transforms from
  `GetCustomAttributes<WrapWithAttribute>` on **the mapper you registered**: a transform *of your
  own* needs a custom mapper to attach it to, because you cannot add an attribute to a type you
  do not own. **The programme already knew to verify a defect before fixing it (Task 4.5) and to
  verify what the defect actually is (`HowServiceActivatorWorks.md`). This is the third face of
  it: verify the *fix*.** It is also the highest-stakes sentence in the spec — the only one no
  source page can be checked against, because it does not exist anywhere yet.

- **An inbound-anchor table is indexed by its *target*, and a split can move the *source* line
  too.** Appendix B records `DarkerBasicConfiguration.md#query-processor-lifetime` against the
  page that **owns the heading**, and it is right to. What it has no column for is that Phase 6
  moved **both ends in one PR**: the target heading went to `DarkerConfigurationReference.md`
  under Task 6.5, while the line carrying the link sat at `QueryPatterns.md:944`, inside a
  section Task 6.1 moved to `EFCoreQueryIntegration.md`. **Neither task's row mentioned the
  other**, and a repoint applied at the line number the table implies would have edited a line
  that was no longer there. It survived because the anchor grep ran *before* the cut and the
  repoint was applied where the line had **landed**. This is the third distinct way Appendix B
  has been incomplete — it does not count a page's own anchors (Phase 5), it goes stale in the
  direction of links the splits *add* (Phase 4), and now it cannot see a moving source.
  **The table is a starting point for a grep, never a substitute for one.**

- **A rule with a tool for half of it will pass the half nobody checks.** Rule 3a says a `##`
  heading must be **unique across pages** *and* **qualified by its subject**. `pagelint.py`
  checks uniqueness; nothing checks qualification. Phase 6 promoted **35** headings to `##` and
  **not one collided** — a completely green rule-3a run was available for the taking, on a set
  containing `## Collections`, `## Dictionaries`, `## Similarities`, `## Differences`,
  `## Acceptance Tests` and `## Default Policies`. Every one is unique. Every one is
  unattributable in a retrieval chunk, which is the entire reason the convention exists.
  **When a convention has a mechanical half and an editorial half, a green build is evidence
  about the mechanical half only** — and the editorial half is usually the one the convention
  was written for.

- **Probing a brand-new URL during the sync window can cache a 404 for it, and the `.md`
  variant is what tells you so.** After PR #89, `outbox-and-inbox/outboxarchiver` returned a
  **190,021-byte 404 shell** — while sitting in `sitemap-pages.xml`, while both its children
  published, and while seven of the eight other new pages returned real bodies from the *same*
  probe batch. It still did minutes later, and a `?cb=` query string does not bust GitBook's
  edge. The programme knew *a wrong redirect outlives its fix at the edge*; **the mirror case —
  a premature probe manufacturing a wrong 404 — was not recorded.**
  **The discriminator:** `curl <path>.md`. GitBook serves raw markdown on a different route
  that the poisoned probe never touched, and it returned `# Outbox Archiver`, 9,145 bytes,
  `200`. **Confirmed resolved**: after PRs #90 and #91 deployed, the same HTML route returned
  **658,502 bytes** — a real page — with nothing about the page having changed. It was the
  edge, and later deploys invalidated it. **So: wait for `sitemap-pages.xml` to move before
  probing new URLs, and if an HTML route 404s on a page the sitemap lists, check `.md` before
  concluding anything.**
  *(One sentence in `tasks.md`'s post-merge note still reads "almost certainly" — it is now
  measured. Carry that correction in Phase 6's PR rather than opening one for a sentence.)*


- **A page's own internal anchors are invisible to the inbound-link table, and a split turns
  them into cross-page links.** Appendix B counts links *from other pages*. `ReplayOnSeen.md`
  carries **27 same-page anchors across 14 targets** — more than the whole spec's inbound
  budget of 34 — and a three-way split breaks every one that crosses a seam. Appendix B says 8
  for that page and is right about what it counts. **Before splitting, grep the page for
  `](#` as well as grepping the corpus for the page.** Phase 5 handled it by construction:
  map every heading to its destination through `linkcheck.py`'s `slug()`, assert the ranges
  cover every line exactly once, rewrite only the anchors whose destination differs, and
  assert none is left unmapped.

- **A section span measured by heading boundaries is not a measurement of subject.**
  `BrighterOutboxSupport.md`'s `## Outbox Archiver` spans 151 lines and design §7.1 spent all
  151 as archiver material. Its last three H3s — `Outbox Configuration`, `Provisioning the
  Outbox Table`, `Outbox Builder`, 60 lines — are about the **Outbox**, misfiled under the
  Archiver heading in the source. Moving them would have put Outbox DDL on a page called
  *Outbox Archiver*: **an IA defect created by the IA spec.** The same shape as design §16
  finding 2, where matching the word "Comparison" swept in a section that compared something
  else. **Read the section before you spend its line count.**


- **Nothing recorded the site's base URL, and guessing it produced a false negative.** Verifying
  Phase 4's publish took three probes. `…gitbook.io/paramore.brighter.documentation/<path>`
  **307s** — a base-level redirect, so every path looked like a redirect including a control
  that had never existed. Following it gave a doubled segment that **404s for every path**,
  including `scheduler/brighterschedulersupport`, a page that has published for years. The
  canonical base is **`https://brightercommand.gitbook.io/paramore-brighter-documentation`** —
  hyphens, one segment. **Record it here so nobody re-derives it**, and note what saved both
  probes: a control. The first needed a known-**bad** path to expose the base redirect; the
  second needed a known-**good** page to expose the wrong base. **Carry both controls, not one.**

- **A genuine page can be 678 KB.** The response-fingerprint table has said **786 KB – 1.1 MB**
  since 2026-08-08, itself a correction of a remembered ~584 KB. Phase 4's two new pages
  measured **690,166** and **677,836** bytes — genuine `200`s with no `location:` header, and
  both *below* the recorded floor, because they are short pages. The 404 shell is still
  ~189.5 KB and the classes still separate perfectly. **This is the second time the magnitudes
  drifted while the classification held. Stop quoting the range; the discriminator is that no
  genuine page response carries a `location:` header.**


- **`git diff` cannot see untracked files, so a new page is invisible to `--changed` until you
  stage it.** `pagelint.py`'s `changed_ranges()` shells out to `git diff --unified=0 <base> --`.
  Phase 4's first `--changed` run reported 0 errors while its two new pages contributed **no
  strict ranges at all** — a vacuous pass on exactly the pages the phase existed to make strict.
  `git add` and it is real: 16 blocks strict, and removing one `// ...` makes that block an
  error. **This is the third time the same prediction has failed** — first on file granularity
  in Phase 3, now on the index — and each time the pass looked identical to a real one. In CI
  the files are always committed, so this fails *only* locally, which is worse: it is the run
  you do before you push.

- **Grep for the finding, not for the word you expect to describe it.** The red-proof above
  first searched `pagelint`'s output for the literal string `error` and reported *"NO ERROR —
  GATE IS VACUOUS"*. Findings never carry that word: an error is the **absence** of the
  `(warning)` suffix. Exit code 1 was the only honest signal, and it disagreed with the grep.
  **When a probe's assertion disagrees with its exit code, the assertion is the suspect.**

- **A snapshot of inbound anchors goes stale in the direction nobody checks.** Appendix B warns
  that its table is a snapshot because the splits *break* links. Phase 4's obligation-3 grep
  returned zero before the split and **two afterwards** — both created by the split itself,
  both pointing at a heading the fold keeps. The write-up's first draft asserted "zero fragment
  links target these pages" in the present tense and was already false as it was typed. **The
  splits add links as well as break them; re-run the grep after, not only before.**

- **A figure that was never wrong can still not be the whole answer.** Spec 011 recorded
  **24 lines and 4 lines** for its two demonstrator splits' no-information-loss checks. Session
  16's `noloss.py` returns **46 and 11** — and reproduces both recorded figures *exactly, as
  subsets*. The recorded numbers were the **prose** lines; they never counted the **20
  requalified headings**, which are precisely the anchor breaks standing obligation 3 exists to
  catch. Nothing contradicted anything: every extra line is documented somewhere in 011's
  write-up, just not in the D5 count. **This is the first correction in this programme that is
  neither a wrong tally nor a wrong rule** — the number was right about what it measured, and
  what it measured was half the question. **Ask what a figure counted, not just whether it is
  right.**
- **Duplication rots asymmetrically, and the copy that rots is the one nobody consults.** The
  `HowServiceActivatorWorks.md:147` overlap was specified as 76 redundant lines. It was worse:
  its code called **`options.AddSubscription<MyCommand>(...)`, which does not exist** —
  `ConsumersOptions` exposes `Subscriptions`, and the only `AddSubscription` in the Brighter
  source is `Dispatcher`'s **private** one. A reader who followed the explanation page would
  not have compiled. **So "verify the defect exists before fixing it" has a second half: verify
  what the defect actually is.** Both copies were checked against the source, and only then was
  it clear which one to delete.
- **A prediction can be right about the phase and wrong about the unit.** The handover said
  Phase 3 is the first PR where `pagelint.py --changed` stops being vacuous, because Phase 2
  touched no file under `contents/`. Phase 3 touched three — and the run was **still
  necessarily vacuous**, because the rule is strict **per code block overlapping the diff**,
  and the diff was five single prose lines. **File granularity was the wrong unit.** The gate
  was confirmed live by probe instead of inferred from a pass. It first bites in **Phase 4**,
  the first phase to create a page.
- **`git checkout --` is not an undo for a probe.** A red-proof for `--changed` mutated a page
  that also held uncommitted work, then "restored" it with `git checkout -- <page>` — which
  restores from `HEAD`, discarding the uncommitted edit along with the probe. It was caught
  only because the probe asserted *restored byte-identical* and that assertion failed.
  **Copy the file aside and restore from the copy**, and keep the byte-identity assertion.

- **A test can be vacuous in exactly the way a check can, and it is harder to notice.**
  Session 15 forced `--check-redirects` red three ways to prove it worked. The middle
  mutation repointed `contents/Routing.md` at a missing file — and **`Routing.md` is not in
  the redirects block**, so the mutation changed nothing and the check reported a **pass**.
  Read as written, that pass says *"the assertion does not fire on a missing file"*: the
  opposite of the truth. Redone with `assert mutated != good` before the result is read.
  **When you prove a check red, assert that your mutation landed.** The programme already knew
  a green check can be vacuous; it had not applied that to the tests of its own checks.

- **A probe covered by your own safety net cannot measure the safety net.** Task 2.10 existed
  to answer the one open platform question — do GitBook's automatic redirects *persist*? It
  re-probed PR #77's old path, found a `location:` header a day later, and **proved nothing**,
  because that key is in our own `.gitbook.yaml` block (line 17, since PR #78). Automatic
  persistence and our block firing are indistinguishable from that response. **D0's own
  lesson, applied to D0's own probe:** never conclude "the redirect works" from one successful
  request. The question is now unanswerable at every path — after PR 2 no moved path is absent
  from the block — and **that is what shipping belt-and-braces was for.** Closed by
  construction; do not re-run it expecting an answer.

- **A remembered magnitude drifts even when the classification holds.** The response
  fingerprint table said a real page is **~584 KB**. Measured 2026-08-08 after PR #85: real
  pages are **786 KB to 1.1 MB**, while the 404 shell is still **189,511 bytes to the byte**
  and a redirect still ~192 KB. The three *classes* separate as cleanly as ever and the
  discriminator — **no genuine page response carries a `location:` header** — is untouched.
  But anyone checking "is this ~584 KB?" would now get the wrong answer on a correct page.
  **Compare classes, not remembered sizes.**

- **A path constant is silently wrong the moment its file moves.** `urlmap.py` computed
  `REPO = Path(__file__).resolve().parents[2]`, correct under
  `spec/010-information_architecture/` and **wrong under `tools/`**, where it resolves to the
  directory *above* the repository. Nothing errors: every path still resolves, just against
  the wrong tree, and `SUMMARY.md` simply appears not to exist. Caught by reasoning about the
  move rather than by any test. **When you move a script, re-derive every path it computes
  from `__file__`.**

- **A threshold justified by "X is the deepest we know to work" is not a threshold — it is a
  measurement nobody has taken.** 010's **S3** capped a published path at three segments
  because *"three is the deepest the live site is known to work at (9 pages publish there
  today). Four is unverified."* Read closely, that is a fact about **our own `SUMMARY.md`** —
  nine pages happen to be nested one level and none two — and says nothing about GitBook. It
  passed requirements, design **and** a design review unchallenged, and had already bent two
  placements out of shape before anyone asked what caused it: `MigratingToPollyV8.md` was
  going to become a *sibling of the page it was extracted from*. **Measured in five minutes
  and two PRs**: GitBook's own docs publish 30 of 182 pages at four segments, and #83/#84
  proved it on this site. **S3 is the FIRST RULE in this programme to move; all fourteen
  previous corrections were tallies.** When a rule's stated rationale is the absence of
  evidence, go and get the evidence.

- **Fix the line-counting convention before you count anything.** A page's length is
  `len(text.splitlines())`. `wc -l` counts *newlines*, so it under-reports the **17** files
  with no trailing newline; `read().split("\n")` counts a *phantom empty final line*, so it
  over-reports the **93** that have one. 010's design used the second and was one line high
  on 23 of its 26 split rows — and *correct* on the three files with no trailing newline,
  which is what makes it so hard to spot. The same artefact inflates the span of the **last
  `## ` section on a page** and no other, so 58 of 60 measured spans were right. It had
  already put design and `worklist.md` into disagreement about the same pages
  (`QueryPipeline.md` 929 against 928) with nobody noticing. **Two tools that disagree by
  one about the same file are not both right.**

- **A heuristic in the style guide is not an acceptance criterion.** `CLAUDE.md` says a
  file over ~500 lines should *"consider splitting"*, and 011 then measured that length
  does not predict anything: `KafkaConfiguration.md` is **608 lines of one mode and is a
  `keep`**, while `BrighterBasicConfiguration.md` **scored two modes and was split** at
  1,070. Mode mixing is the criterion. **Sixteen pages stay over 500 after 010 completes,
  seven of them `keep` rows AC8 obliges us to honour** — and the four family *parent* pages
  are excepted outright, because a middle navigation layer needs a real page to hang it
  from and a stub cannot. **Do not review against a number the corpus already disproved.**

- **When a board says "nearly done", read the unfinished item before believing it.** Spec
  005 showed **13/14** for two months. The open box was a *reserved contingency* whose own
  note said to mark it n/a if its trigger never fired — and it never fired. Nothing was
  outstanding; the tally was just never closed. **An unticked box is a claim, not a fact.**

- **Quote the whole rule, not the sentence that scared you.** 010's requirements were held
  up for a session by *"you will need to remove the old page in order for the redirect to
  work"*. The sentence before it — **"as long as a page exists *for a path*, GitBook won't
  be looking for a possible redirect"** — is the actual rule, is keyed on the path rather
  than the page, and resolves in our favour. The blocking quote was the *consequence* of a
  rule nobody had read. **Fetch the source and read the paragraph, not the clause someone
  pasted.**
- **A vendor's own example can be the bug.** The two U+200B zero-width spaces that stopped
  GitBook reading `.gitbook.yaml`'s `structure:` block for months are **in GitBook's
  published example, today**, and the `redirects:` snippet sits inside the same code block.
  The next person to write a config block by pasting the documented one reintroduces it.
  **Type config, never paste it** — and assert on bytes.
- **A summarising fetch is not a measurement.** `WebFetch` on `sitemap-pages.xml` reported
  **127** URLs; the raw XML has **111**. The wrong figure was plausible, contradicted a
  recorded fact from two sessions earlier, and would have quietly become the denominator
  of 010's redirect coverage. Anything that reads a document through a model and hands
  back a *number* is an estimate. **Fetch the bytes and count them yourself.**

- **A rule that never fires is invisible to everything but an enumeration.**
  `pagelint.py` could emit eight rule labels; `CLAUDE.md`'s ledger listed seven. `NO H1`
  was missing for four sessions of the ledger being actively read and edited, and
  nothing was ever broken by it — every page has an H1, so the rule had never fired
  once. Found only by listing both sides and diffing them, at the acceptance pass. **To
  check parity, enumerate; do not read.**

- **A summary can go stale while the thing it summarises stays correct.** The page-type
  tally read `48 / 30 / 27` for four sessions because it was written just before a
  verdict changed on the same page of notes, and re-derived never afterwards.
  `pagetypes.tsv` was right the entire time, and nothing ever contradicted anything —
  which is exactly why nobody caught it. **Re-derive a total; never add to one.**
- **A green linter is not a correct page.** The first banner sweep left the banner with
  no blank line after it, running into the following heading, and `pagelint.py` passed —
  rule 1 only asks what the first non-blank line after the H1 is. Read the diff.
- **Predicted counts are predictions.** **Six** of the figures 011's approved design quoted
  have now been wrong, and one corpus fact behind two of them had never been recorded.
  **Eight across the programme** once `worklist.md`'s keep-count (26/16, not 30/12) and
  005's task total (14, not 15) are counted — and **thirteen** after 010's design review
  found five more (23 line counts from one bad convention, the 96/85 fold-up, the 8/10
  entry count, and two section spans) — and **fourteen** after session 14 enumerated
  `worklist.md`'s `keep` rows and found they name **sixteen** distinct pages, not the fifteen
  requirements §12 and §14 both assert. **Every one of the fourteen was a tally, never a
  verdict** — the rules were derived from the corpus and have held; the numbers were
  estimated from it and have not. When you meet a total, re-derive it.
  **The corollary session 13 adds: a measurement is only as good as its instrument.** Five
  of those trace to one wrong line-counting call, not five careless readings.
  **And the exception session 14 adds: the "never a rule" half of that claim is now spent.**
  **S3 moved** — see the sixteenth lesson above. A rule can be wrong too, when its rationale
  was never anything but an untested assumption.
- **Verify the defect exists before fixing it.** Task 4.5 was specified as merging three
  duplicate-content pairs. Two were not duplicates and the third was not content — one
  of the cited line numbers pointed into an unrelated glossary entry. Had it been
  executed as written, it would have destroyed correct material.
- **Check a claim against the source, not the assumption.** The banner asserted
  "Darker V10" on 10 pages. Darker's latest release is 4.1.1 and has never had a V10.
- **A check that passes has not necessarily checked anything.** `pagelint.py --changed`
  reported 0 errors, which is indistinguishable from a run that found no ranges to be
  strict about. It was real — 118 files, 465 hunks — but the only way to know was to
  force it red on purpose. Prove a new gate fails before trusting it to pass.
- **A tool can advertise a remedy it does not implement.** Rule 6's message offered
  "mark the omission with `// ...`" and `CLAUDE.md` listed `// ...` among the parts a
  tool can check; `check_code_blocks` only ever tested for `using`. Nothing caught it
  because nobody had tried the second remedy. Read the code behind the message.
- **A property can stop being true without anyone changing the thing it describes.**
  "Re-running the banner sweep is safe and idempotent" was accurate for months, because
  no page carried a Prerequisites segment. The splits created five that did, and the
  sweep silently began deleting them — the script never changed. When you add a case a
  tool was never exercised on, re-derive its guarantees rather than inheriting them.

---

## Where things stand

**Every convention 011 set out to establish is now true of every page, a tool proves it,
and the build fails when it stops being true.** All **142** pages carry a banner stating type
and version — 110 when 011 closed, plus the thirty-two Phases 4 through 9 created; headings are
qualified by their subject across and within pages;
"Dispatcher" has replaced "ServiceActivator" in prose. `pagelint.py` reports **0 errors**
and **runs in CI**. Also new since the programme started: **CI**, and the **conventions
written into `CLAUDE.md`**.

That closes the argument the preceding four phases existed to make — an unenforced
convention decays, which is what the audit measured in the first place. **Both
demonstrator splits landed in session 7**, taking the corpus from 105 pages to 110, and
**Phase 7 closed the spec in session 9**: every fence carries a language tag, rule 4 is
a repo-wide error, `pagelint.py --fix` makes the next version bump one edit plus one
command, and all eight acceptance criteria have been walked.

**Nothing in 011 remains open.** The one debt still standing is deliberate and
recorded: C# blocks with no `using` directives, which is AC1's baseline to shrink. It was
**802 across 93 pages** when 011 closed and is **791 across 118 pages** today — down 11 on
genuine edits (Phase 3's fold removed five, Phase 4's net was −4, Phase 5's −2, and **Phases 6,
7, 8 and 9 were all zero**), and spread wider because the splits created pages. Phases 6 and 7
marked 20 blocks `// ...` each, Phase 8 marked 7 and **Phase 9 marked 51** — and the total did
not move on any of the four occasions, which is the marker behaving as designed: it downgrades
and never silences. Q4's two strictness levels retire the debt as pages are edited; a sweep
would touch 118 pages to change nothing a reader can see.

- **009** — requirements **and** design both approved (design reviewed 2026-08-03,
  six findings applied). Not started; runs in parallel. **D12 is no longer blocked** —
  see *Spec 009* below. **D9's CI slot is already built** — a `versions` job in
  `docs.yml`, guarded by `if [ -f tools/versioncheck.py ]`; landing D9 is one file plus
  removing that guard
- **005** — **COMPLETE 2026-08-07, 14/14.** Closed after review against 010: of the five
  pages it owns, only `BrighterOutboxSupport.md` has further work, and that is a *split*
  on 010's worklist. Nothing to pick up
- **011** — **COMPLETE.** 43/43 tasks, AC1–AC8 walked, merged. Nothing to pick up
- **010** — **ACTIVE. 47 of 52 tasks done; PHASES 1 THROUGH 10 DONE. ONLY PHASE 11 (CLOSE)
  REMAINS.** All three phases approved (requirements 2026-08-06, design and
  tasks 2026-08-08). **The twelve-section tree is LIVE**, 74 URLs moved, all 74 redirects
  measured working, **all 32 new pages have landed**, and
  `tools/urlmap.py --check-shape`/`--check-redirects` gate CI. **D6 is SUPERSEDED — GitBook
  owns `/llms.txt` and we fixed it at source instead; every page carries `description:` front
  matter and AC9 is already narrowed.** **Next action: Phase 11 = PR 11, close.**
  **S3 is ≤4 segments on a live measurement — design §17**, and exactly two pages reach it.
  **Amended 2026-08-04** — the `BasicConcepts` merge is withdrawn. It executes against
  `spec/011-authoring_conventions/worklist.md`, which stands alone. ~~Its README is stale
  in two places~~ — **all three stale passages fixed in PR #82**
- **012, 013** — `README.md` only; next action `/spec:requirements`. **013's was amended
  2026-08-04** — it gained five content gaps found while classifying

### Page types, as reviewed by the maintainer

| Type | Today (142) | 011 close (110) | 2026-08-03 (105) |
|---|---|---|---|
| Reference | **55** | 50 | 48 |
| How-to | **53** | 33 | 31 |
| Explanation | **34** | 27 | 26 |
| **Tutorial** | **0** | **0** | **0** |

The right-hand column is the maintainer's original review of 105 pages; the middle adds the
five pages 011's Phase 6 splits created (2 Reference, 2 How-to, 1 Explanation); the left adds
all of 010's Phases 4 through 9.

**Mind which quantity you are reading.** The ten pages Phases 4 and 5 *created* are **5
Reference and 5 How-to**. The column moves by **+3 / +6 / +1**, because **two retypes** also
land: `ReplayOnSeen.md` Reference → Explanation (Task 5.2) and `InMemoryOptions.md`
Reference → How-to (Task 5.5). Creations and net delta are different numbers and both read
true in a sentence about "the ten pages" — which is the mechanism behind four of this
programme's wrong tallies. **Re-derived from `pagetypes.tsv` 2026-08-12 after Phase 9, not added
to.** Phase 6's twelve
are **9 How-to, 2 Explanation, 1 Reference**, and there are no retypes in it, so for once the
creations and the deltas are the same numbers. **Phase 7's three are one of each** — one
Reference, one How-to, one Explanation — and again no retypes, so `+1 / +1 / +1`.
**Phase 8's two are one How-to and one Explanation**, no retypes, so `+0 / +1 / +1`.
**Phase 9's five are three How-to and two Explanation**, no retypes, so `+0 / +3 / +2` — which
is what takes Reference to a standstill at 55 while the other two columns keep moving.

> **Corrected 2026-08-05 at Task 7.1: this table read `50 / 32 / 28`**, inherited from
> `classification-notes.md` §11's `48 / 30 / 27`. That tally was written before §10's own
> `QueryPipeline.md` Explanation → How-to correction was applied, and never re-derived.
> **`pagetypes.tsv` was right the whole time** — only the summary of it drifted, which is
> the hard version to notice: no two figures ever disagreed. Re-derive, never add to a
> total:
> `awk -F'\t' 'NR>1{c[$5]++} END{for (k in c) print k, c[k]}' spec/011-authoring_conventions/pagetypes.tsv`

**No page is a tutorial.** That is correct today and is precisely the gap Spec 009
exists to fill — it is the substance of #67, now measured rather than asserted.

**The plan through Phase 2 was to prove the conventions before sweeping, and it paid
off — repeatedly.** Running the linter disproved three predicted figures and turned up a
corpus fact nobody had recorded. Reviewing the sweep's diff rather than trusting a green
linter caught two further defects. Phase 4 then disproved two more predictions and found
that one specified task — merging three "duplicate content" pairs — described defects
that did not exist. Phase 6 found design §10's central instruction resting on an
inverted premise: "moved verbatim" was impossible to honour, because a new page is
entirely added lines and its blocks are already strict.

**Six of 011's approved design's figures have now been wrong, one of its premises, and
none of its rules — and 010's design added five more figures at its own review, again with
no rule touched.** That asymmetry is the pattern: the rules were derived from the
corpus, the numbers and the assumptions were estimated from it. Written up in *Audit
data* and 011's `tasks.md` §§ *Measured baseline*, *Phase 4 as executed* and *Phase 6 as
executed*.

**Two facts discovered at 011's review that shaped the programme — both now acted on:**

- ~~**The Docs repo has no CI.**~~ **Fixed 2026-08-04.** `.github/workflows/docs.yml`
  exists and runs `linkcheck.py`. This was also the explanation for the audit findings:
  every unenforced `CLAUDE.md` rule had decayed, and the 14% `using`-directive
  compliance is what that looks like after a couple of years.
- ~~**`CLAUDE.md` contradicts the heading convention.**~~ **Fixed 2026-08-04.** Its
  *File Organization Pattern* prescribed `## Configuration`, `## Best Practices`,
  `## Common Pitfalls`, `## Sample Code` — four of the worst collisions. The pattern was
  amended in the same commit that added the *Page Conventions* section.

| Spec | Directory | Phase |
|---|---|---|
| 001 Darker Doc Improvements | `spec/001-darker_documentation_improvements/` | **OUT OF SCOPE 2026-08-07** — unapproved 52-task list, expected to be superseded by 010/013. Do not pick up |
| 005 Database Migration | `spec/005-database_migration/` | **COMPLETE 2026-08-07 — 14/14.** Closed after review against 010 |
| 009 Getting Started Tutorials | `spec/009-getting_started_tutorials/` | Requirements + design approved 2026-08-03 |
| 010 Information Architecture | `spec/010-information_architecture/` | **ACTIVE — all three phases approved; PHASES 1-10 DONE (PRs #82, #85-#103), 47 of 52 tasks. The twelve-section tree is live, 74 URLs moved, all 32 new pages landed at 142 pages, and every page in both spaces carries a `description:` that reaches the canonical `/llms.txt` — 143 of 143 ours, 58 of 58 V9. Next: Phase 11 = PR 11, close.** Amended 2026-08-04, the `BasicConcepts` merge is withdrawn |
| 011 Authoring Conventions | `spec/011-authoring_conventions/` | **COMPLETE 2026-08-06 — 43/43 tasks, AC1–AC8 walked, merged as PR #76. Nothing to pick up** |
| 012 Configuration Reference | `spec/012-configuration_reference/` | README only |
| 013 How-To Guides | `spec/013-howto_guides/` | README only — **amended 2026-08-04** with five content gaps from 011's Task 3.2 |

Each README carries topic overview, scope, out-of-scope, source material,
dependencies, risks and open questions.

### Where the detail lives — read these, don't re-derive them

```
spec/009-getting_started_tutorials/
├── README.md          rationale for the four-rung ladder
├── requirements.md    APPROVED — samples survey, Q1–Q5 decisions, acceptance criteria
└── design.md          APPROVED — page-by-page outlines, D9/D11/D12, all 6 items closed

spec/010-information_architecture/
├── README.md          rationale. THE THREE STALE PASSAGES ARE FIXED 2026-08-07 — Out of
│                      Scope, the transports/ redirect example, and the seven-bucket
│                      structure, each corrected in place with the reason
├── tasks.md           ★★ THE OPERATIONAL DOCUMENT. REVIEWED AND APPROVED 2026-08-08.
│                      52 tasks, 11 phases, Phase N = PR N; 37 DONE (Phases 1-8).
│                      §4 FIRST — the tasks review's five findings, four tallies and the
│                      pagetypes.tsv omission. Then PHASE 8 AS EXECUTED (the source
│                      measurement a later PR overtook, and the anchor pair that moved
│                      together), PHASE 7 (the correctness fix checked against the source),
│                      PHASE 6 (the split-page shape rule and the lead-in rule), PHASE 5
│                      (the anchor-mapping method, the ## Outbox Archiver deviation, and the
│                      two standing lines on conflicting copies and misfiled content), then
│                      PHASE 4 AS EXECUTED, then PHASE 3 AS EXECUTED, then the two
│                      "as executed"
│                      sections at the end of Phase 2. Then §1's SEVEN standing obligations
│                      and covers pagetypes.tsv. Then §2 (the seven items design left
│                      open), §3 (what GitBook already ships — it re-scopes D6), PHASE 9's
│                      six tasks, APPENDIX A (RATIFIED and re-derived; where all 32
│                      new pages nest — do not re-derive), Appendix B (anchor obligations
│                      per task), Appendix C (what each phase must not do)
├── design.md          ★ APPROVED 2026-08-08, PR #82. §17 FIRST (S3 measured at four
│                      segments 2026-08-08; SUPERSEDES §4's threshold and §7.6's
│                      placement reasoning), then §16 (the review's five
│                      findings), then §15 (measured vs not), §3 the twelve-section tree, §4 Q8's S1/S2/S3 threshold,
│                      §7 the 26 splits page by page, §7.7 the FIVE DEVIATIONS from
│                      worklist.md's shape column, §7.8 WHY 500 LINES IS NOT A CRITERION
│                      and the 16 pages that stay over it, §8 the anchor-link table,
│                      §9 D3/D6/D7, §10 the eleven-PR sequence, §14 AC mapping
├── SUMMARY.target.md  ★ the twelve-section tree as PR 2 shipped it: 145 lines, 110 links,
│                      12 sections, pure ASCII. **INSTALLED 2026-08-08 (PR #85), and the
│                      repo's SUMMARY.md HAS SINCE MOVED PAST IT** — Phases 4 and 5 added
│                      ten entries, so it is 120 links now. This file is a RECORD, not a
│                      description of the current tree, and NOT a file to install or diff
│                      against. design.md's figures reproduce against it, not against
│                      today's SUMMARY.md
├── requirements.md    ★ APPROVED 2026-08-06. §2 is the verified URL model everything
│                      rests on; §2.2 answers Q1 from the source; §2.3 is the automatic-
│                      redirect risk that REPLACED it and reshaped D0; §2.4 the U+200B
│                      provenance; §7 P0-0..P2; §12 AC5a/AC5b and the per-split AC7;
│                      §13 Q2–Q10 (Q1 closed); §15 what the review changed
├── .requirements-approved · .design-approved · .tasks-approved   all three markers exist
├── noloss.py          ★ D5, Task 3.4, 2026-08-08. THE HARNESS EVERY SPLIT IN PHASE 9
│                      OWES A RUN OF. Originals at a git ref, results in the working
│                      tree or at --result-ref; both sides take several paths, because
│                      a fold's check is the union. READ ITS DOCSTRING — it carries the
│                      contract, including why a heading is compared by its slug() and
│                      why a clean run is NOT zero. Calibrated against 011's two
│                      demonstrators (46 and 11, reproducing the recorded 24 and 4 as
│                      subsets); proved red four ways. Used in anger in Phases 3-8
│                      — 104 lines over the scheduler union, 18 over the ReplayOnSeen split,
│                      2 over each of the two clean ones, 50 across Phase 6's five invocations,
│                      5 across Phase 7's three and 6 across Phase 8's two, every one a heading
│                      or a link. A FOLD'S
│                      CHECK IS ONE INVOCATION OVER THE UNION, never one per page
└── (no urlmap.py)     ★ MOVED to `tools/urlmap.py` in Task 2.1, 2026-08-08. See the
                       Tooling section for its five modes and which two gate CI. The move
                       needed a one-line REPO fix that nothing would have caught

spec/011-authoring_conventions/
├── README.md              rationale; front-matter ruling. Superseded on numbers — says 110
│                          pages, which the corpus has now coincidentally become; the
│                          figure it meant was wrong when written
├── requirements.md        APPROVED — measurements, Q1–Q5, linter rules, CI finding
├── design.md              APPROVED — banner grammar, pagelint spec, both split plans, §12
│                          sequencing. §1's version vocabulary CORRECTED 2026-08-04
├── tasks.md               APPROVED — 43 tasks / 7 phases, 39 ticked. Opens with § Applied
│                          at review; closes with §§ Measured baseline, Phase 4 as
│                          executed, Phase 6 as executed and Task 7.1 as executed — READ
│                          THOSE, they carry every finding that contradicts the approved
│                          design
├── worklist.md            ★ D8, Task 7.1 — the 42-row split worklist SPEC 010 EXECUTES
│                          AGAINST. Written to stand alone: 010 needs no other file here.
│                          12 rows say `keep`, on purpose. §5 carries the three decisions
│                          that cover many pages at once; §7 three content defects
├── classification-notes.md  ★ READ THIS — Task 3.2's rulings and everything the
│                          classification turned up: the BasicConcepts reversal, the four
│                          missing how-tos, the scheduler split pattern, the Darker
│                          findings, and two verdicts that are the assistant's call. §11's
│                          tally was stale until 2026-08-05 — see the correction in it
├── pagetypes.tsv          110 rows, all verdicts filled — the 5 Phase 6 split pages were
│                          appended 2026-08-05. The `applies` column identifies the 10
│                          Darker-touching pages as a set. Order is NOT sorted by any
│                          single rule; append rather than re-sort
├── proposetypes.py        generated the proposals (Task 3.1)
├── apply_banners.py       the banner sweep. KEPT past its scheduled deletion — used 4 times
├── qualify.py             ★ Tasks 4.1/4.2 — the cross-page heading sweep. Its SUBJECT /
│                          OVERRIDE / KEEP tables ARE Task 4.1's reviewed proposal list.
│                          Run bare to print the plan, `--apply` to write it; it repoints
│                          moved anchors in the same pass
├── dedupe_within.py       Task 4.3 — the within-page sweep. Line-targeted, asserts every
│                          line before writing, and writes nothing if any is stale
└── modemix.py             the mode-mixing analysis, and the source of worklist.md's
                           scores. Its heading figures were confirmed by pagelint.py; its
                           code-block figures were not — see the baseline. Re-run it
                           rather than citing the numbers from memory: they have moved
```

`design.md` in each is the document to work from; `requirements.md` explains *why* and
records what was rejected. **Both were revised at review** — the approved versions
differ substantially from first drafts, so don't work from memory of an earlier state.

For 011, `tasks.md` is now the operational document: its phases *are* design §12's ten
steps, and it opens with two corpus findings (the 105-page count and `.gitbook.yaml`'s
zero-width spaces) that the approved design predates. Both are now resolved; the file
closes with §§ *Phase 4 as executed* and *Phase 6 as executed*.

---

## Execution order

**011 → 010 → 012 → 013, with 009 running in parallel throughout.**

Spec numbers are identifiers, **not** a sequence — an explicit decision, so don't
"correct" the ordering to match the numbering.

> **Scope is 009–013. Ruled 2026-08-07.** Specs 001–008 predate this programme.
>
> - **001 `darker_documentation_improvements` — ignore it.** It carries an unapproved
>   52-task list, and this programme's work is expected to **supersede** it: 010 re-files
>   the Darker pages and Q2 asks whether *Darker* becomes top-level, while `worklist.md`
>   §5b already splits `QueryPipeline.md`. Re-planning it before 010 lands would be
>   planning against a tree that is about to change. **Do not pick it up, and do not treat
>   its 0/52 as outstanding work on the board.**
> - **002–008 are complete**, 005 as of 2026-08-07 — see below.
>
> So the only live specs are **009 (parallel), 010 (active), 012 and 013 (README only)**.

Why 011 precedes 010: 011 added a page banner to every page, 010 moves every
page. Doing 010 first means touching every page twice and reviewing two large
overlapping diffs.

**The same reasoning moved page-splitting from 011 into 010** (decided 2026-08-03).
Splitting creates pages needing names, `SUMMARY.md` entries and redirects — 010's job,
on files 010 is already editing. 011 keeps the rule, the linter, two demonstrator
splits and a scored worklist for 010 to execute. **Both splits and the worklist landed
2026-08-05, so 010 waits on nothing here.**

Why 009 is parallel: purely additive, no breakage risk, and it's the visible win that
answers the public criticism soonest.

---

## Suggested next session

**Start Phase 10 = PR 10.** Phases 1 through 9 are merged; `master` is at `e7c9d71`;
**43 of 52 tasks.** **The content phases are over — do not split another page in this spec.**

### Phase 10, and the thing to do before writing any of it

**D6 must be re-scoped before it is built.** `tasks.md` §3 is the measurement, taken 2026-08-08:
GitBook already ships `/llms.txt`, `/llms-full.txt`, `.md` variants of every page and an MCP
server, and **we cannot override `/llms.txt`**. The design's §9.2 predates that. So:

- **Task 10.3 is a maintainer ruling, not a build.** The question is no longer *"repository paths
  or published URLs"* — it is what, if anything, we should ship that the platform does not.
- **AC9 must narrow if D6 narrows.** Design §14 maps the criteria; if the ruling is *"the
  platform covers it"*, AC9 has to say so rather than be quietly declared met.
- Whatever it generates reads `SUMMARY.md` for the tree and each page's **banner** for the type,
  which is why the banner had to land first. Both are true of all **142** pages today.

Then **Phase 11 = PR 11, close** — Glossary links, the two carried-over chores (P2-1, P2-3), and
the acceptance pass. Design §14 is the AC-by-AC map to walk.

**What still needs a human:**

- **Tasks 10.1–10.3 — what D6 is now for.** The maintainer's ruling, as above.
- ~~**Task 1.1 — ratify Appendix A.**~~ **RATIFIED 2026-08-08**, and **now measured**: all 32
  pages exist, all twelve section rows reproduce, and the two pages that reach S3's ceiling of
  four segments are the two Appendix A named.

### Phase 9, as it actually went — 2026-08-12

All five sources measured before the cut, and **every whole-page count and every span reproduced
`tasks.md` exactly**, at `3f0f4dd`. That is the first time in this spec all of a phase's figures
held, and it is why Phase 8's lesson is worth the five minutes:

| Task | Source | Lines | Spans | Sum |
|---|---|---:|---|---:|
| 9.1 | `CQRSWithBrighterAndDarker.md` | 1,144 | 448–656 | 209 |
| 9.2 | `NullableReferenceTypes.md` | 711 | 166–429 | 264 |
| 9.3 | `AgreementDispatcher.md` | 720 | 11–84 · 85–230 · 325–377 · 378–424 | 320 |
| 9.4 | `PolicyRetryAndCircuitBreaker.md` | 687 | 377–472 · 537–687 | 247 |
| 9.5 | `Telemetry.md` | 597 | 19–99 · 366–461 · 462–491 | 207 |

**Task 9.3's naming trap was real** — `Use Cases` and `Limitations` do not exist as strings on
the page, because Spec 011's `0b1b841` qualified them years before this design was written. The
spans were right; only the titles were pre-qualification.

### What session 21 established — PHASE 9 SHIPPED, and the defect that was not there

One PR: **#95** (`e7c9d71`). **43 of 52 tasks done, and the content
phases are over.** Five pages created, seven touched, and **no URL moved** — every core keeps
its filename and all five new pages are new paths, so `.gitbook.yaml` stayed at 77 entries, the
same shape as Phases 6, 7 and 8. Full write-up in `tasks.md` *Phase 9 as executed*.

| | Was | Is |
|---|---:|---:|
| `CQRSWithBrighterAndDarker.md` | 1,144 | **936** |
| `CQRSUseCasesAndPatterns.md` (Explanation) | — | **225** |
| `NullableReferenceTypes.md` | 711 | **448** |
| `MigratingToNullableReferenceTypes.md` (How-to) | — | **298** |
| `AgreementDispatcher.md` | 720 | **401** |
| `AgreementDispatcherRouting.md` (Explanation) | — | **346** |
| `PolicyRetryAndCircuitBreaker.md` | 687 | **438** |
| `MigratingToPollyV8.md` (How-to) | — | **277** |
| `Telemetry.md` | 597 | **391** |
| `ConfiguringOpenTelemetry.md` (How-to) | — | **220** |

**All 32 new pages have landed and Appendix A's section table reproduces row for row** —
enumerated from `SUMMARY.md`, not read off the table being checked: 142 pages, 12 sections,
**S1 2 · S2 10 of 12 · S3 4 of 4**, the ceiling reached by exactly `AzureBlobConfiguration.md`
and `MigratingToPollyV8.md`. The latter publishes at
`commands-handlers-and-pipelines/buildingapipeline/policyretryandcircuitbreaker/migratingtopollyv8`
— the four-segment placement that caused S3 to be measured in the first place.

**The finding is the first lesson above.** Task 9.6 and design §11 both prescribed *"the missing
reverse pointer"* from `HandlerFailure.md` to `ErrorHandlingOptions.md`. It was never missing —
six links today, five since `ac0c727` created the page on 2026-02-23 — and the `SUMMARY.md`
nesting the task also asked for landed in PR 2. **Two of three deliverables were already true**;
the task reduced to the *Prerequisites* segment in the banner, which was genuinely absent.
Corrected in place in both `tasks.md` and `design.md`. **A one-directional claim needs a
one-directional grep.**

**`PolicyRetryAndCircuitBreaker.md` is the only core in this spec to land *under* budget**, 438
against ~440. The `## Legacy` section was the last on the page, so removing it orphaned the
`---` that introduced it; separator, blank and the file's trailing blank went too. Recorded
rather than absorbed — under budget is the direction nobody checks.

**Two shape cases the rules had not met.** `MigratingToNullableReferenceTypes.md` is the first
single-section page with `####` beneath its `###`; Phase 6's rule stops at *"promote the `###`"*,
which would have left five `####` under a `##`. The whole subtree is promoted by one.
`ConfiguringOpenTelemetry.md` is built from three sections and the **first has the same text as
the H1 Appendix A's filename pins** — `pagelint.py` is silent, because rule 3b starts at `##`.
Requalified to `## Setting Up OpenTelemetry`.

**Uniqueness green, attribution poor — for the fourth phase running.** All eleven moving or
promoted `##` headings were unique across the corpus, and eight still needed requalifying:
`## Pattern: Task-Based UI`, `## Step 2: Address Compiler Warnings`,
`## Performance Implications`, `## Complete Configuration Example`,
`## Distributed Tracing Example`.

**The anchors were quiet, and that was established rather than assumed.** Appendix B reproduced
exactly — 2 inbound on `Telemetry.md` (0 repoints, both stay in the core), 1 on
`PolicyRetryAndCircuitBreaker.md` (repointed), 0 on the other three. **None of the five sources
carries a same-page anchor**, so Phase 8's second-kind work had no equivalent here. Phase 5's
mapping method was run anyway. Both greps re-run after the cut: no link added.

**One correctness fix, created by the split** — `PolicyRetryAndCircuitBreaker.md`'s
*"see migration guide above"* had no above once the guide moved, and now links the page it went
to. Every other `above`/`below` on both pages was checked and still resolves.

**D5: 13 lines across five invocations** — twelve requalified or dropped headings and that one
prose line. No code survives nowhere.

**`pagelint --changed` bit for the fourth phase running, and hardest yet**: **51 errors**, all
`USING DIRECTIVES` on the five new pages (22/12/11/4/2), red with the files staged and green
after the `// ...` markers. Proved live, not by probe. Debt unchanged at **791**, spread
113 → **118** pages.

### What session 20 established — PHASE 8 SHIPPED, and a figure that went stale after it was written

One PR: **#94** (`3f0f4dd`). **37 of 52 tasks done.** Two pages created, five touched, and
**no URL moved** — both cores keep their filenames and both new pages are new paths, so
`.gitbook.yaml` stayed at 77 entries and the phase owed no redirect, the same shape as Phases 6
and 7. Full write-up in `tasks.md` *Phase 8 as executed*.

| | Was | Is |
|---|---:|---:|
| `AWSSQSConfiguration.md` | 615 | **375** — 615 − 245, +5 for a *Further Reading* the page never had |
| `AWSSQSMigrateToV10.md` (How-to) | — | **261** |
| `PostgreSQLMessageBroker.md` | 663 | **554** — 663 − 110, +1 for the child pointer |
| `PostgreSQLBrokerTradeOffs.md` (Explanation) | — | **125** |

*Transports* lands at **12 pages / 7 top-level entries** — Appendix A's row exactly — with both
new pages at three segments.

**The finding is a new shape for this programme, and it is the first lesson above.**
`PostgreSQLMessageBroker.md` is **663** lines against Task 8.2's 662, and **662 was right when
it was written**: PR #90, which belonged to no phase, added one `// ...` line to
`## Scheduled Messages` four days after the tasks were approved. Traced ref by ref — 662 at
`25a578c`, 662 at `f9f042d~1`, 663 at `f9f042d`. Nothing broke, because the section stays in the
core; the budget moved 552 → **553**. All eighteen previously wrong figures were wrong *when
written*. **Re-measure the source at the ref you are cutting from.**

**`AWSSQSConfiguration.md` reproduced at 615 under both counting conventions** — it has no
trailing newline, one of the three files that does — and both its spans matched exactly:
49 + 196 = 245.

**Appendix B held on what it counts, and the work was in what it does not.** Three inbound
anchors on `AWSSQSConfiguration.md`, all `#migrating-from-aws-sdk-v3-to-v4`, all repointed
(`S3LuggageStore.md:15`, `DynamoOutbox.md:22`, `DynamoInbox.md:16`), and all three carrying pages
outside this phase's sources — Phase 6's moving-source case checked, not assumed. Zero on the
PostgreSQL page. But the page carries **six same-page anchors**, mapped heading by heading before
the cut: four core → core and untouched, and **two whose source line and target heading move
together onto the new page**. One needed a repoint (its heading was requalified); the other did
not, because `### Migrating from AWS SDK v3 to v4` was left alone — rule 3a does not reach an H3,
and keeping its slug made all three inbound repoints a pure path change. **Read from Appendix B
alone, both looked like nothing to do, and one of them was.**

**Phase 6's third finding reproduced for the third phase running.** All six moving `##` headings
were **unique across the corpus** — `## Benefits`, `## When to Use`, `## V10 Migration Path`,
`## JSON vs JSONB`, `## Comparison with Other Transports` — so a completely green rule-3a run was
available and worth nothing. Five requalified against the source page's own established form;
`## AWS SDK v4 Support` and `## PostgreSQL Message Broker Limitations` left alone.

**D5: 6 lines across two invocations** (2 and 4), every one a requalified heading or the anchor
repointed in the same pass. No prose and no code survives nowhere.
`## PostgreSQL Message Broker Limitations` is **absent** from the 8.2 list, which is the check
confirming the heading left alone was genuinely left alone.

**`pagelint --changed` bit for the third phase running, and was proved live rather than by
probe** — 7 errors, all `USING DIRECTIVES` on the two new pages with the files staged, red before
the `// ...` markers and green after. Real AWS SDK and Brighter types whose namespaces were not
checked, so the omission is marked, not guessed. Debt unchanged at **791**, spread 111 → **113**
pages.

**Published and verified live.** The sitemap moved to **138** before any probe — Phase 5's lesson
applied — and both new pages returned genuine `200`s with **no `location:` header**
(824,395 and 772,902 bytes). **Both controls carried:** a known-good page (`hangfirescheduler`,
1,215,402 bytes) and a known-bad path returning a real `404` at **196,885** bytes. The classes
still separate perfectly. On the `.md` route, `## AWS SQS V10 Migration Path` is present on the
child and `V10 Migration Path` returns **zero** matches on the core; `## Benefits` returns zero on
the PostgreSQL core.

**One thing recorded that has nothing to do with Phase 8:**
`contents/ReturningResultsFromAHandler.md` **opens with a blank line before its H1**. Found by the
banner-parity check, which assumed the H1 was line 1 — the script was wrong, not the page.
`pagelint.py` accepts it, because rule 1 asks only what the first non-blank line *after* the H1
is. The next parity check will meet it too.

### What session 19 established — PHASE 7 SHIPPED, and the correctness fix that needed checking

One PR: **#93** (`7810f38`). **35 of 52 tasks done.** Three pages created, seven touched, and
**no URL moved** — all four cores keep their filenames and all three new pages are new paths, so
`.gitbook.yaml` stayed at 77 entries and the phase owed no redirect. Full write-up in `tasks.md`
*Phase 7 as executed*.

**Every predicted figure reproduced, before a line was moved.** All four page lengths
(266 / 478 / 475 / 597), all eight quoted `##` spans, and all four arithmetic cores —
**147 / 333 / 369 / 281** — matched design §7.5 exactly; shipped at 154 / 334 / 370 / 282 after
the *Further Reading* entries pointing at the new children. *Using an External Bus* stays at **9**
top-level entries and goes to **15 pages**, Appendix A's row to the letter, every new page at
three segments.

**The finding is the one piece of new prose this spec authors, and the source contradicted the
ruling's wording** — the first of the two new lessons above. `JsonMessageMapper<TRequest>`
already carries `[CloudEvents(0)]`, a `WrapWithAttribute`, so *"transforms require a custom
mapper"* written literally would have shipped false.

**Appendix B held exactly**: 3 inbound anchors on `MessageMappers.md`, all three repointing, 0 on
the other three sources, 0 same-page anchors anywhere. **Phase 6's moving-source case was checked
rather than assumed** — all three carrying pages sit outside this phase's sources.

**Every page here is built from *several* `##` sections**, so Phase 6's other shape branch applied
for the first time at scale: sections stay `##`, nothing is promoted, and the phase requalified
**one** heading against Phase 6's thirty-five. `### Message Transformer Factory` was deliberately
left alone, which kept its slug and made all three repoints a pure path change.

**D5: 5 lines across three invocations**, every one a requalified heading or a link. Task 7.1's is
**one invocation over the union**, because §5c folds two sources into one page.

**Two link labels corrected** under the line Phase 4 drew, both naming content this PR moved:
`CloudEventsSupport.md`'s *"transform pipelines"* link, and `DefaultMessageMappers.md` calling
`MessageMappers.md` *"Legacy V9 mapper documentation"* while that page ships banner'd
**Brighter V10** in the same PR.

**Published and verified live.** The sitemap moved to **136** before any probe — Phase 5's lesson
applied — and all three new pages returned genuine `200`s with **no `location:` header**
(800,333 / 678,814 / 830,615 bytes). **Both controls were carried:** a known-good page
(`hangfirescheduler`, 1,211,117 bytes) and a known-bad path, which returned a real `404` at
**196,438** bytes. The classes still separate perfectly. `### Message Transformer Factory` was
confirmed present on the published `.md`, and `## Transformers` confirmed gone from
`messagemappers.md`.

### What session 18 established — PHASE 6 SHIPPED, the largest phase in the spec

One PR: **#92** (`47e03b0`). **31 of 52 tasks done.** Twelve pages created, five touched, and
**no URL moved** — every core keeps its filename and all twelve new pages are new paths, so
`.gitbook.yaml` stayed at 77 entries and the phase owed no redirect. Full write-up in `tasks.md`
*Phase 6 as executed*.

**Every predicted figure reproduced, and this time before a line was moved.** All five page
lengths (1,291 / 935 / 928 / 877 / 510), all eleven quoted `##` spans, and all five arithmetic
cores — **306 / 646 / 716 / 579 / 435** — matched design §7.3 exactly; shipped at
311 / 648 / 718 / 581 / 436 after the *Further Reading* entries pointing at the new children.
*Darker* stays at **5 top-level entries** and goes to **17 pages**, Appendix A's row to the
letter, every new page at three segments.

**The finding is the anchor whose source moved too** — the first of the two new lessons above.
Appendix B's one predicted inbound link had **both ends move in the same PR**, under two
different tasks, neither of which mentioned the other.

**The second finding is that a completely green rule-3a run was available and would have been
worthless.** 35 headings promoted, **zero collisions**, and most of them still needed
requalifying: `## Collections`, `## Similarities`, `## Default Policies`. Uniqueness has a tool;
attribution does not.

**Two shape questions Phase 5 left ambiguous are now settled**, because Phase 6 hit them twelve
times instead of twice — the single-section split shape, and when a written lead-in is
warranted. Both are in the *Suggested next session* block above and in `tasks.md`.

**D5: 50 lines across five invocations**, every one a requalified heading, a section heading
absorbed into an H1, or a repointed anchor. **No prose and no code survives nowhere** — the
cleanest split phase in the spec, which is what a single-`##`-section shape buys you.

**Task 3.1's gate was open and the second branch was taken as written.**
`QueriesAndQueryObjects.md`'s `## Query Patterns` stays, was not replaced by a link, D5 did not
run against `QueryPatterns.md` for Task 6.4, and the core landed at **579** — the design figure
to the line. Nothing was re-verified.

**Carried and discharged:** `tasks.md`'s Phase 5 post-merge note no longer says the
`OutboxArchiver.md` cached 404 was *"almost certainly"* an edge artefact. It is measured, and
the 658,502-byte figure is in the document.

### What session 17 also established — PHASE 5 SHIPPED, plus two follow-up PRs

Three PRs: **#89** (`5e1dd81`, Phase 5), **#91** (`643562f`, its post-merge measurements) and
**#90** (`a12d5d9`, the scheduling-overload fix). **26 of 52 tasks done.** Eight pages
created, ten touched. Full write-up in `tasks.md` *Phase 5 as executed*.

> **Phase 5 MERGED** as PR #89 → `5e1dd81`, with #91 (`643562f`) its post-merge measurements
> and #90 (`a12d5d9`) the scheduling-overload fix. **`--admin` is now standing authorisation**
> — the maintainer granted it 2026-08-09, so do not ask about the flag again; still ask about
> merging any PR that changes the published site. Verify `--check-redirects` **before**
> merging, never after: the edge cache holds a wrong redirect for 30 days.

**Exactly two URLs move**, and they are the two the spec always knew would move twice:
`outbox-and-inbox/azureblobarchiveprovider` and its configuration child, now nested under
`OutboxArchiver.md`. **Both intermediate paths have a redirect entry**, derived from
`git show origin/master:SUMMARY.md` rather than from memory. `.gitbook.yaml` **75 → 77**
entries, 7,673 → 7,858 bytes, printable ASCII.

**Three pinned figures reproduced exactly.** *Outbox and Inbox* lands at **9** top-level
entries — Appendix A's pinned answer against design §7.6's intended 10.
`AzureBlobConfiguration.md` is the **first page in the corpus to publish at four segments**,
S3's measured ceiling, and Appendix A named exactly it. And **Appendix B held**: 24 inbound
links, **11 repointed** (5 + 6), 13 stayed.

**The finding is the two new lessons above** — 27 uncounted internal anchors, and a section
span that was not all one subject. A third, smaller: **Task 5.2 and design §8 route
`#replay-versus-replay-skipped` to the wrong page.** It sits inside `## Observability`, which
is the Reference page, not the how-to. Found by *mapping* the headings rather than reading the
note, and it would have shipped as a 404 into the middle of a page.

**Task 5.4's scheduler merge added nothing, and that is a result.** `InMemoryOptions.md`'s
53-line `## InMemory Scheduler` is entirely duplicated by `InMemoryScheduler.md` — including
*"Demos and proof-of-concepts"* **verbatim** — and twice tells the reader to go there instead.
Task 3.1 inverted: *not duplicate* was a passing outcome there, *entirely duplicate* is one
here, and both had to be measured.

> **A defect Phase 4 shipped, found in Phase 5 for an unrelated reason, and awaiting a
> ruling.** `SchedulingAMessage.md` carries **four non-compiling scheduling calls**.
> `IAmACommandProcessor` declares `SendAsync<TRequest>(DateTimeOffset at, TRequest command, …)`
> and `SendAsync<TRequest>(TimeSpan delay, TRequest command, …)` — **the time comes first**,
> and `PostAsync` is the same. There is no overload taking the request first with a named
> `at:`/`delay:` argument. Moved verbatim, so obligation 1 was honoured, but it was **not**
> among the three defects Phase 4 recorded. **FIXED in PR #90** — and the sweep found the
> defect was older and wider than the page 010 created: **ten calls across three pages**, four
> on `SchedulingAMessage.md`, five on `BrighterSchedulerSupport.md`, one on
> `PostgreSQLMessageBroker.md`. Editing those blocks made four of them overlap the diff, so
> `--changed` turned them strict and reported errors; they carry fictional user types, so the
> omission is marked `// ...` rather than guessed at.

### What session 17 established — PHASE 4 SHIPPED, and the gate finally bit

One PR: **#88** (`8d0b5a4`). **21 of 52 tasks done.** Two pages created
(`SwitchingSchedulers.md`, `SchedulingAMessage.md`), eight touched, `TickerQScheduler.md` and
`CustomScheduler.md` untouched. **No URL moved**, so no redirect is owed. Full write-up in
`tasks.md` *Phase 4 as executed*.

**Every predicted line count reproduced exactly** — a first for a split phase. Each core is
design §7.1's figure **+1**, and the +1 is the same line on all five: the *Switching
Schedulers* entry added to *Related Documentation*. 771 / 701 / 732 / 633 / 496, and
`BrighterSchedulerSupport.md` at **391** against a ~400 budget.

**`pagelint.py --changed` bit for the first time, and needed the files staged.** See the two
new lessons above. Staged: 16 blocks strict, all reporting the `// ...` downgrade; one marker
removed → `SwitchingSchedulers.md:52: USING DIRECTIVES` as an **error**, exit 1, warning
repo-wide, restored byte-identical from a copy.

**D5 over the union: 104 of 2,480 lines, in six groups summing to 104 exactly** — 49 table
rows superseded by the merged comparison table, 4 comparison headings, 9 folded bullets, 3
corrected claims, 2 requalified headings, 38 lines of collapsed migration sections. Tasks 4.1,
4.2 and 4.3 interlock across the same six pages, so it is **one invocation**, not three.

**The finding of the phase is Task 3.3's, one phase later.**
`BrighterSchedulerSupport.md`'s `## Choosing a Scheduler` said AWS was `Limited`/`Limited` and
Azure `No`/`No` for cancel and reschedule. Checked against `../Brighter`: **every scheduler
cancels, and Azure is the only one that cannot reschedule** — its `ReSchedulerAsync` is
`=> Task.FromResult(false)`; nothing throws `NotImplementedException` anywhere. The four
technology pages' own tables were right. **Duplication rots asymmetrically, and the copy that
rots is the general one nobody consults for a specific answer.**

**The line drawn on obligation 1, so Phase 5 can apply it:** a merge that forces a choice
between conflicting copies resolves **to the source**; a page that would otherwise ship, in
the same PR, a sentence contradicting a table in that PR is **corrected**; everything else
**moves verbatim and is recorded**. Three defects recorded and deliberately not fixed:
`QuartzMessageSchedulerFactory` does not exist, `HangfireMessageSchedulerFactory` takes no
constructor arguments, and `UseScheduler` is on `IBrighterBuilder` not `IServiceCollection`.

**Two deviations from design §7.1's outline**, both recorded rather than silently absorbed:
**four target H3s, not five** (no source section documents switching *to* InMemory — its own
section documents switching *away*, which is the shared *before*), and **no
`## Switching Schedulers Verification`** (no source material, and new prose is reserved to
Task 7.1). Same shape as design §16 finding 2: the outline listed five *schedulers*, not five
*sections*.

### What session 16 established — PHASE 3 SHIPPED, and the D5 harness exists

One PR: **#87** (`438166b`). **17 of 52 tasks done.** Three pages touched, one tool added; no
page created, no URL moved, `SUMMARY.md` untouched. Full write-up in `tasks.md`
*Phase 3 as executed*. Gates on `master`: linkcheck 112 files, pagelint **0 errors / 797
warnings**, `--check-shape` 0, `--check-redirects` 0 at 75 entries.

**`spec/010-information_architecture/noloss.py` is the deliverable that matters** — D5, and
every split in Phases 4–9 owes it a run. Originals at a git ref, results in the working tree
(or at `--result-ref`); both sides take several paths, because a fold's check is the union.

**Its one non-obvious design decision: a heading is compared by the anchor it produces**, via
`linkcheck.py`'s own `slug()`, imported rather than copied. A section that becomes a page
promotes its headings and may shed their bold, and `slug()` ignores both — the anchor
survives, nothing is lost. **Requalifying a heading under rule 3a does change the slug and is
reported**, which is the class of change obligation 3 exists to catch. Not a softening: it is
the equivalence relation the rest of the tooling already uses.

**Proved red four ways, with the mutation asserted before each result was read:**

| | |
|---|---|
| identity, a page against itself | **0 of 92** — no false positives |
| one substantive line deleted | reported, exit 1; restored byte-identical |
| heading promoted a level **and** bolded | **not** reported — *matched on its anchor alone* |
| heading requalified | **reported** — the allowance is not a hole |

**Task 3.1: NOT DUPLICATE, and that is the passing outcome.** `QueriesAndQueryObjects.md`
`## Query Patterns` shares **32 of its 58 substantive lines verbatim** with
`QueryPatterns.md` — and **every one of the 32 is a code fragment or a fence. Not one prose
line is shared.** A 55% line overlap that is 0% prose overlap is two pages using the same
worked domain. The relationship is level of detail: query object *shape* against end-to-end
recipe. **So Task 6.4 keeps the section and lands at ~579**, and its note in `tasks.md` now
says so. Recorded but not acted on: the two pages **disagree about the same code**
(`SalesStatistics` vs `SalesStatisticsDto`, `CustomerSummary` vs `CustomerSummaryDto`,
`SearchProductsQuery` with constructor parameters vs init-only properties).

**Task 3.3 is the finding of the phase — see the second new lesson above.** The duplicate copy
did not compile. `HowServiceActivatorWorks.md` 486 → **416** (the 76-line section becomes a
6-line pointer, so −70, not −76); the reference page stays at **233**, because both folds
extend existing bullets rather than adding lines. **D5 over the union returned 40 lines, all
accounted for.** Debt 802 → **797**, exactly the five C# blocks removed.

**Task 3.2:** `## How It Work` → `## How Sweeper Circuit Breaking Works`, the corpus house
form. **Zero** inbound `#how-it-work` links, re-derived across `contents/`, `SUMMARY.md` and
`.gitbook.yaml` rather than trusted from design §8.

### What session 15 established — tasks REVIEWED, and PHASES 1 AND 2 SHIPPED

Four PRs merged: **#82** (`25a578c`, the reviewed tasks), **#85** (`1bae048`, **Phase 2 — the
tree**), **#86** (`9ecae58`, Phase 2's post-merge measurements). `master` ends at `9ecae58`.
**13 of 52 tasks done.** Full write-ups in `tasks.md` §4 and its two *as executed* sections.

**The tasks review found five things: four tallies and one omission.** No verdict, threshold,
placement or ruling moved. **The four tallies shared a mechanism worth knowing** — each number
was *true of something adjacent*, so it read correctly in the sentence it sat in:

| Was | Is | Why it survived |
|---|---|---|
| Phase 5 makes 7 new pages | **8** | 7 is the count landing in *Outbox and Inbox*; `InMemoryTransport.md` files into *Transports* |
| Phase 5 carries "19 of the 34" anchor links, in **three** places | **24** links, **11** repoints | 19 is the *whole-spec* repoint total |
| Appendix A: "six sources at three segments", "the other 20" | **seven** and **19** | 6 + 20 = 26, the correct total, with both halves off by one |
| `DefaultMessageMappers.md` 479 lines | **478** | the retired `split("\n")` convention, in one spot; Task 7.1 already said 478 |

**The omission is the one that could have broken something: nothing added the 32 new pages to
`pagetypes.tsv`.** Named once in 1,119 lines, never in `design.md`. The file is 110 rows
against a corpus ending at 142; `apply_banners.py` reads it, so **a version bump would have
skipped all 32** — the identical defect Spec 011's Phase 6 splits produced and `5498cd6` fixed.
**`pagelint.py` never reads the TSV, so no tool sees it.** Now **standing obligation 7**, plus
a parity line in Task 11.4. Two *retypes* were half-covered — Task 5.2 named the TSV, 5.5 did not.

**Everything load-bearing was re-derived and held**: 52 tasks and all eleven per-phase counts;
Appendix A's 32 rows, 5 top-level, 27 nested, max depth 4 on exactly two pages, section
allocation matching design §7.6 row for row; **all twelve rows of the entries table**, computed
by applying Appendix A's nesting to `SUMMARY.target.md`; **all 36 quoted section spans** and 25
of 26 page lengths; 74/36; 26 split and 16 `keep` rows naming 16 distinct pages.

**Phase 2 shipped, and both new checks were proven red before being trusted green.**

| | |
|---|---|
| `--check-shape` on the **old** 19-section tree | **9 real failures** — 6 singleton sections (S1), 2 sections at 14 and 20 entries (S2), the leading space at `SUMMARY.md:154` (P0-1) |
| `--check-shape` on the new tree | **0** — 110 pages, 12 sections, deepest **3** of 4, widest **10** of 12 |
| S3 forced red | a page nested to 5 segments → exit 1; file restored byte-identical |
| `--check-redirects`, all three assertions | forced red by a U+200B in `structure:`, a value repointed at a missing file, and a live key that could never fire |
| `.gitbook.yaml` verified by **two** parsers | ours (75 entries, 7,673 bytes, all printable ASCII) and `ruby -ryaml` (3 top-level keys, `structure:` a **real** key, 0 leading slashes) |

**All 74 redirect keys were measured, and the sweep's own verdict was wrong.** 67 returned
`307`; **7 returned `200` while still carrying a `location:` header** — and those 7 are exactly
the paths sampled by hand minutes earlier, which is what warmed their cache. The sweep's pass
condition required `code == "307"` and reported them `NOT REDIRECTING`. **The data was right and
the verdict was wrong**, which is the documented cache trap firing on our own script.

**Three figures to carry.** The block holds **75** entries, not 74 — the extra is D0's own,
so a gate expecting 74 in the file reads a non-defect as one. The 404 shell is still
**189,511 bytes**; a real page is **no longer ~584 KB** but 786 KB – 1.1 MB. And
`sitemap-pages.xml` stays at **111**, so no page was lost.

**What is now closed by construction rather than measured:** whether GitBook's automatic
redirects persist. See the second new lesson above — the probe's key is in our own block, and
after PR 2 no moved path is absent from it.

**Two passes were recorded as non-evidence rather than dressed up.** `pagelint.py --changed`
reported 0 errors on PRs #82, #85 and #86 **necessarily vacuously** — none touches a file under
`contents/`. And the redirect entries were **generated by `tools/urlmap.py` and appended
programmatically**, not hand-typed: *"type the block, never paste it"* is about vendor
contamination and invisible characters, and hand-typing 74 paths trades that for a worse typo
risk. The block is asserted on bytes either way.

### What session 14 established — 010's TASKS, and S3 measured

Two commits on the branch (`27a4c42`, `a91aa0a`) plus PRs #83/#84 to `master`, which cancel.

**`spec/010-information_architecture/tasks.md` — 52 tasks, 11 phases, `Phase N = PR N`.**
Design §10 was already a task plan, so the document fills it in rather than re-sequencing it.
PRs 4–9 stay individually shippable and individually abandonable; **no task re-introduces an
all-26-splits gate**, because AC7 is per-split. Three appendices: **A** the nesting of all 32
new pages, **B** the anchor obligations per task, **C** what each phase must not do.

**S3 was an assumption, and measuring it is the session's real finding.** The rule capped a
published path at three segments because *"three is the deepest the live site is known to
work at"* — a fact about our own `SUMMARY.md`, not about GitBook. It had already forced
`MigratingToPollyV8.md` to be a *sibling of the page it was extracted from*. Measured:

| | |
|---|---|
| GitBook's own docs | **30 of 182 pages at four segments** below the site root |
| This site (PR #83) | probe page present in `sitemap-pages.xml` (112, was 111), `200`, **0 `location:` headers**, **529,080-byte** body against a ~189.5 KB 404 shell |
| After PR #84 | sitemap back to **111**, tree byte-identical to `c4aedb5` |
| `urlmap.py` | needed **no change** — its `[<ancestor>/]*` was never bounded |

**The method is the reusable part, and it is D0's:** a **new** page at a path that had never
existed, so no automatic redirect could mask the result, and no existing URL churned. The
status code was ignored — every cached response on this site reports `200`.

**What moved:** S3 → **≤4 segments** (design §4, recorded in **§17**);
`MigratingToPollyV8.md` nests under its own source; `OutboxArchiver.md` and
`TransactionalMessagingWithTheOutbox.md` go top-level; `AzureBlobArchiveProvider.md` and its
configuration child nest under the archiver; *Outbox and Inbox* lands at **9** top-level
entries; deepest path after all splits is **4**, reached by exactly two pages.

**What did not:** the twelve-section tree, every `keep` verdict, the 74/36 URL split, the 32
new pages, and **PR 2's own tree, whose maximum depth is still 3** — nothing in the 110-page
restructure needs the fourth segment.

**A fourteenth wrong tally, found by enumerating.** Requirements §12 and §14 say the 16
`keep` rows cover **fifteen** distinct pages. They name **sixteen** — `TickerQScheduler.md`
appears in two rows and one row names two pages (`HandlerFailure.md` +
`ErrorHandlingOptions.md`). Fifteen counts *rows by subject*. **AC8's substance is
untouched**; recorded in `tasks.md` §2 rather than edited into an approved document.

**Appendix A RATIFIED 2026-08-08**, placement by placement. 27 of 32 are derivations; the
five decisions stand as written, including both scheduler how-tos top-level (4 entries, as
§7.6 intended) and `MessageTransforms.md` under `MessageMappers.md` — which S3 used to force
and which is now the right call anyway, since the page exists to say transforms need a
**custom** mapper.

**No file under `contents/` is changed** — linkcheck 112 files, pagelint 0 errors / 802
warnings, both unchanged from session 13.

### What session 14 found about `llms.txt` — D6 is largely already done, by GitBook

Full write-up in **`tasks.md` §3**. Measured on the live site 2026-08-08; **all of it is
automatic, with no configuration, and it re-scopes D6 rather than answering Q9's question.**

| Endpoint | Result |
|---|---|
| `/llms.txt` | **200**, 27,470 bytes, `text/markdown`, **170 entries** |
| `/llms-full.txt` | **200**, 1,332,541 bytes — the whole corpus |
| any page + `.md` | **200**, raw markdown, **our banner intact** |
| any page + `?ask=` | documented query endpoint |
| `/~gitbook/mcp` | **405 to a GET** — the route exists; an MCP server, POST-only |

- **59 of the 170 entries are V9, with no discriminator.** A separate *V9 Paramore Brighter
  Documentation* space is in the same index as our 111. A V10 page's `.md` leads with
  `> **Reference** · Applies to **Brighter V10**`; **the V9 page has no banner at all.** The
  banner works and the index undoes it — 011's exact failure mode, one level up. **Task 10.2.**
- **`CLAUDE.md`'s format is already the platform's format.** GitBook's own `llms.txt` emits
  `- [Title](url): description` on **294 of 668** entries; ours emits **zero**, because no
  page has a description set. **Task 10.1** establishes how one is set — and note the 011
  front-matter ruling was about rendering *metadata into the page body*, not about
  `description`. **Quote the whole rule, not the sentence that scared you.**
- **We cannot serve our own `/llms.txt`** — GitBook owns the path, so a repo-root file is
  GitHub-only. That changes what D6 is *for*, which is why **Task 10.3 is a ruling**, and
  **AC9 must narrow if D6 does.**
- **Its sections are GitBook spaces, not our IA.** The twelve-section tree never reaches it.
- **Q9's opening-sentence rule survives regardless** and is now better justified than when it
  was written: that sentence is what the `.md` variant leads with. **Task 10.4** — as
  `llmstxt.py`'s validator, or as a new `pagelint.py` rule.

> **Still live, not history:** GitBook's *Content configuration* page — the one this repo's
> `.gitbook.yaml` was written from — **still contains U+200B in its `### Structure` heading
> today**, fetched 2026-08-08. **Type config; never paste it.**

### What session 13 established — 010's design REVIEWED AND APPROVED

Full write-up in **`design.md` §16**. One commit, `92c58ba`, pushed to PR #82.

**Every load-bearing figure was re-derived against the corpus.** These reproduced exactly
and should be cited, not recomputed: the **74 moved / 36 unchanged** URL split (110 → 110,
0 dropped, 0 added, depth 3, via the design's own snippet); `SUMMARY.target.md` at **145
lines / 110 links / 12 sections / pure ASCII**; **all 12 rows** of §4's pages-and-entries
table; **all 19 rows** of §3.2's *Today* column, 110 pages, 6 singletons; §8's **34** inbound
anchor links across exactly **7** pages with the *moves? yes* rows summing to **19**;
`worklist.md` at **42 / 26 / 16 across 41 distinct pages**, TickerQ twice; the design's split
set **equal to the worklist's, 26 for 26**, so **AC8 holds**; the **32** new pages, matching
§7.6 section by section, 110 + 32 = **142**; §7.8's sixteen **complete with no omissions**
(30 pages exceed 500 today and the other 14 all drop below); and **58 of 60** cited section
spans.

**Five findings, every one a tally. No verdict, threshold or ruling moved.**

1. **Whole-page line counts were one too high on 23 of 26 split rows** — the counting
   convention, now the fifteenth hard-won lesson above. Two cited spans were affected and
   no others, because only a page's *last* `## ` section inherits the artefact.
2. **The scheduler fold-up is 85 lines across four sections, not 96 across six** — and this
   was the load-bearing one. The 96 came from matching the word "Comparison", which swept in
   `AwsScheduler.md`'s `## Scheduling Modes Comparison`, a table comparing **AWS's own two
   scheduling modes** (direct-to-target vs `FireAwsScheduler`). It stays in the AWS core.
   `QuartzScheduler.md` has no comparison section at all. §7.1's per-page table had already
   excluded it, so 11 lines were on both sides of the ledger. Arithmetic is now
   **578 − 212 + 278 + 85 = 729**. **The deviation STANDS** — 729 still exceeds today's 578,
   which was the entire argument. **`worklist.md` §5a/§6a say "six" and are wrong for the
   same reason; 011 is closed and was not re-opened for it.**
3. **§4's prose contradicted its own table** — *Outbox and Inbox* shows **8** top-level
   entries before the splits, not 10. Ten is the post-split figure from §7.6.
4. **§7.6's entries column is an intention, not a measurement**, and §15 now says so. §4's
   column reproduces from `SUMMARY.target.md`; §7.6's depends on where the 32 new pages
   nest, which no file yet records. **If all seven of *Outbox and Inbox*'s new pages landed
   top-level it would show 15 and breach S2's ceiling of 12** — `--check-shape` contains it,
   but the tasks phase should pin the nesting down.
5. **D7 ratified as delivered differently** — redirect validation lives in
   `urlmap.py --check-redirects`, not in `linkcheck.py`, which has no URL model.

**#67 re-checked 2026-08-08: still two comments, both the maintainer's**, unchanged since
2026-08-03. Nothing external constrains the design. Check again before PR 2 merges.

**No file under `contents/` was touched** — linkcheck 112 files clean, pagelint 0 errors /
802 warnings, both unchanged.

### Review scope — the constraint that shaped session 13's review, kept for the tasks phase

**Do not review this design against a 500-line page limit.** `CLAUDE.md` says *"if a file
exceeds ~500 lines, consider splitting into logical sub-topics"* — a prompt to think, not a
threshold, and **the design deliberately leaves sixteen pages above it**. The full list,
with the reason for each, is **`design.md` §7.8**. Applying 500 as a rule would reject work
that Spec 011 already measured and ruled on.

The criterion is **mode mixing, not length**, and `worklist.md` §2 disproved length outright
before 010 started: `KafkaConfiguration.md` is **608 lines of a single mode and is a
`keep`**, while `BrighterBasicConfiguration.md` **scored two modes and was split anyway** at
1,070 lines doing two different jobs. Length is a symptom that is right often enough to
mislead — which is exactly why 011 built a mode score and then warned that the score is
wrong in both directions too.

**Seven of the sixteen are `keep` rows** — `V10MigrationGuide.md` (891), `FAQ.md` (649),
`Glossary.md` (591), `KafkaConfiguration.md` (608),
`CommandProcessorConfigurationReference.md` (672), `AsyncAPISupport.md` (516),
`RequestValidation.md` (501). **AC8 requires all 16 `keep` verdicts to be honoured**, and
those rows exist precisely so 010 does not re-open settled questions. Re-opening one on
length is the failure mode they were written against.

**The four parent pages are excepted by design, and `BrighterSchedulerSupport.md` is the
one to name.** It is the only page in the corpus that *grows* — both donor and receiver,
shedding 212 lines of how-to and taking in the four comparison sections that duplicate its
own `## Choosing a Scheduler`. It lands at **~400 as designed**, and at **729 if §7.7's
first deviation is overruled**; in that case the 729 is the consequence of the ruling, not
a defect in the split. Same protection for `BrighterOutboxSupport.md`,
`BrighterInboxSupport.md` and `DistributedLock.md`: **a middle navigation layer needs a real
page to hang it from** (requirements §3.1), and that page has to earn its place with
content. Shrinking the four parents to stubs to satisfy a line count would hollow out the
navigation the whole tree depends on.

**All four things the review was told to attack were attacked, and the results are in
§16.** The tree stands; §7.7's five deviations stand; §15's not-measured list gained an
item; and the figures were re-derived, which is where the five findings came from.

### What session 12 established — the design itself

Full document: `spec/010-information_architecture/design.md`. **Read §16 first** — the
review's five findings — then §15, which separates what was measured from what was not.
**The figures below are the corrected ones**; where session 12's draft differed, §16 says
why.

- **The tree is twelve sections** (from 19), all 110 pages placed, written out in full as
  **`spec/010-information_architecture/SUMMARY.target.md`** — the actual file PR 2 installs,
  kept beside the design so every figure reproduces instead of being trusted.
- **74 of 110 URLs move, 36 do not** — measured with the validated predictor, not estimated.
  The 36 is a design output: **seven of the twelve section names are deliberately unchanged**
  (*Brighter Configuration*, *Using an External Bus*, *Outbox and Inbox*, *Scheduler*,
  *Health Checks and Observability*, *V10 Migration*, *Reference*). That is Q3's answer.
- **The 26 splits produce 32 pages; 110 → 142.** Derived row by row from measured `## `
  section spans, never guessed. Deliberately *not* rounded or padded.
- **Q8's threshold counts entries, not pages** — ≥2 pages, **≤12 top-level entries**, ≤4 URL
  segments. *Outbox and Inbox* holds 32 pages and shows **eight** entries before the splits
  and **nine** after, because 21 are stores nested under three parents. The failure the
  requirements named was the flat list, not the total. Becomes `urlmap.py --check-shape` and
  gates CI, which is also Q10's answer. ~~The post-split entry counts are an intention, not
  a measurement~~ — **pinned and measured 2026-08-08 in `tasks.md` Appendix A**, which
  supersedes design §7.6's column. Nine, not the ten §7.6 intended.
- **Q9 takes neither horn.** `llms.txt`'s sentence is the page's own opening sentence,
  extracted, and the build **fails** when it is missing, over 200 chars, ends in a colon or
  duplicates another page's. A page whose first sentence cannot be read alone has a bad
  first sentence. Expect the first run to fail on a double-figure number of pages.
- **Q5: `ReplayOnSeen.md`'s core is the Explanation**, and the load-bearing reason is that
  `outbox-and-inbox/replayonseen` is one of the 36 URLs that does not move.
- **Q6 is dropped, on a fact only the slug function shows.**
  `Requests, Commands and Events.md` publishes as `requests-commands-and-events`; the
  PascalCase rename `CLAUDE.md` requires would publish as `requestscommandsandevents`. **The
  awkward filename makes the better URL.** `Requests-Commands-And-Events.md` would preserve
  it exactly but breaks PascalCase. Neither trade is worth taking.
- **There is no *How To* section, and 013 does not get one either.** The README's seven
  buckets had nowhere to put the Brighter core — handlers, pipelines, configuration, the
  external bus — so it would all have landed in *How To* beside 013's guides. A how-to lives
  beside its subject; a *Guides* section only becomes worth its place at three or more
  genuinely cross-cutting guides, and that is 013's call.

**Two findings the review checked hardest — both survived:**

- **The scheduler fold-up, executed literally, makes the page it drains larger.**
  `worklist.md`'s Hangfire row folds each scheduler's `Overview` + `How Brighter Integrates`
  up into `BrighterSchedulerSupport.md`. Those five pairs are **278 lines**; the page is 578,
  sheds 212 → 366, and adding 278 + 85 returns it to **729** — larger than it is today and
  larger than anything the split drains. Only the comparisons fold up, which is what §5a's
  own conclusion ("one enriched overview") says. **This is one of five deviations from the
  shape column, all collected in design §7.7 with their arithmetic**, so they can be
  overruled in one place. No verdict changed; all 16 `keep` rows honoured. **Session 13
  corrected the fold-up from 96 lines to 85 and the total from 741 to 729** — the AWS
  "Scheduling Modes Comparison" is not a scheduler comparison — **and the conclusion held.**
- **Anchor links were counted before the work rather than during it.** The
  `BrighterBasicConfiguration.md` split cost 28 repoints across 20 pages. All 26 splits
  together carry **34 inbound anchor links across 7 pages** — 19 pages have none — and only
  **≈19 links actually need repointing**, because most linked anchors sit in sections that
  stay in the core. Design §8 has the per-anchor table.

**Also landed in PR #82:** the README's three stale passages corrected in place (Out of
Scope, the `transports/` redirect example, and the seven-bucket structure), and
`SUMMARY.target.md`. **No file under `contents/` was touched** — linkcheck 112 files clean,
pagelint 0 errors / 802 warnings, both unchanged.

**The design's own list of what it did not measure** (§15): every "after" line count is
arithmetic on today's sections, not a count of a page that exists; the 32 changes if any of
the five §7.7 deviations is overruled (items 1/2/3/5 add 5 pages, item 4 removes 3); and the
`QueriesAndQueryObjects.md:746` ↔ `QueryPatterns.md` duplication is **flagged, not verified**
— PR 3 verifies it before anything moves, because Spec 011's Task 4.5 found three specified
"duplicate content" defects that did not exist.

### What design had to produce — ALL FIVE DELIVERED, verified at review

`spec/010-information_architecture/design.md`, working from the approved
`requirements.md`. The deliverables were D0–D9 in §9; **every one is covered**, D7
deliberately differently (finding 5). Checked off at the session-13 review:

1. ~~**The target `SUMMARY.md` tree, in full**~~ — **§3, and `SUMMARY.target.md` is the
   literal file.** The README's seven buckets are dead.
2. ~~**Where each of the 110 pages lands**, plus the pages the splits create~~ — **all 110
   placed; 32 new pages, derived row by row.** Re-derived at review: enumerating §7 gives
   exactly 32 and the per-section allocation matches §7.6.
3. ~~**A before/after for `SUMMARY.md`**~~ — **§3.2 maps all 19 sections**, and
   `SUMMARY.target.md` is the runnable "after". Better than a diff, because it reproduces.
4. ~~**The redirect story**~~ — **§10 PR 2, 74 entries, written once** (D0 proved an entry
   does not go stale when its page moves again).
5. ~~**Split outlines for the 26 rows**~~ — **§7, with measured source spans.** §7.7
   collects the five shape deviations; all 16 `keep` rows honoured, confirmed by set
   comparison at review.

**Two structural constraints bind the tree, and both are already measured:**

- **GitBook offers only H2 groups and page-parented children — there is no sub-group.**
  So **a middle navigation layer needs a real page to hang it from** (requirements §3.1).
  Four such pages already exist: `BrighterOutboxSupport.md`, `BrighterInboxSupport.md`,
  `BrighterSchedulerSupport.md`, `DistributedLock.md`. Where a family needs a middle layer
  and has no parent, either it stays flat or the overview page becomes a deliverable.
- **Nesting deepens the URL.** Nine pages already publish three segments deep, so
  re-parenting moves a URL exactly as a section rename does.

### The design decisions — ALL CLOSED, Q1–Q10

**Nothing in requirements §13 remains open.** Q1 was answered from the source and measured;
Q7 was ruled (one spec, sequenced; AC7 per-split); **Q2–Q6 and Q8–Q10 were answered in
`design.md` and approved 2026-08-08.** Summary of the answers, all in design §1's table:

- **Q2** the twelve-section tree, §3 — *Darker* **does** become top-level (5 pages, one
  section, from 2). **Q8** ≥2 pages / ≤12 top-level entries / **≤4 URL segments** (was 3;
  measured 2026-08-08, design §17), §4.
- **Q3** 74 move, 36 do not; **seven section names deliberately kept**, §5.1.
- **Q4** restructure first, splits second, eleven PRs, §10. **Q5** `ReplayOnSeen.md`'s
  Explanation is the core, §7.4. **Q6** the rename is dropped — the awkward filename makes
  the better URL, §6.3.
- **Q9** the summary is the page's own opening sentence, extracted, build fails when it is
  unusable, §9.2. **Q10** yes — `--check-shape` and `--check-redirects` in CI, no YAML
  dependency, §9.1.

**Do not re-open any of these.** If one must be revisited, design §7.7 collects the five
shape deviations so they can be overruled together, and §16 records what the review changed.

### Fixed at design — nothing outstanding

- ~~**010's README is stale in two places.**~~ **All three passages fixed in PR #82** — *Out
  of Scope*, the `transports/` redirect example, and the seven-bucket structure, each
  corrected in place with the reason.
- ~~**`SUMMARY.md:154`**~~ — the leading-space heading is **retired by construction**: the
  ` ## Under the Hood` section no longer exists in the target tree. P0-1 still asserts no
  heading carries leading whitespace, because the hazard is the class, not the instance, and
  `--check-shape` now owns that assertion.

### D0 — EXECUTED 2026-08-07. Do not re-run it

Full write-up in requirements **§16**. Both mechanisms work:

| Question | Answer | Evidence |
|---|---|---|
| Does GitBook auto-redirect a Git-synced `SUMMARY.md` rename? | **Yes** | **307** to the new path **25s** after PR #77 merged, with **no `redirects:` block anywhere in the repo** |
| Are `.gitbook.yaml` redirects read on this site? | **Yes** | A probe key for a path that had never existed began redirecting after #78; a control key *absent* from the block kept 404ing |
| Does the site plan permit section-level redirects? | **Yes** | No plan gate was hit |

**The method is the part worth remembering.** Shipping the rename *with* its redirect —
the obvious way — would have gone green on the automatic 307 while proving nothing about
`.gitbook.yaml`. And because step 1 answered "yes", the *planned* step 2 became
untestable: the moved path now resolves, so the resolution order never reaches
`.gitbook.yaml`. Step 2 was redesigned mid-experiment around **a probe key for a path that
had never existed**, which no automatic redirect can mask, plus **a control key absent from
the block**. The control is what makes it a measurement rather than a hope.

**Three findings that change how D2 and D3 get built — carry these into design:**

- **The redirect value is a repository path, and GitBook resolves it to wherever that page
  *currently* publishes.** The probe pointed at
  `contents/CommandsCommandDispatcherandProcessor.md` and landed on the **post**-rename
  URL. **A redirect entry does not go stale when its page moves again**, so the block can
  be written once rather than re-derived after every move. This matters for a spec that
  will move some pages twice.
- **D2/D3 are belt-and-braces, not load-bearing** — automatic redirects appear to cover
  010's whole case. **But nothing establishes they *persist*.** They may be tied to
  revision history and one session cannot test it. **Ship the block anyway**; it is cheap
  and it is the safety net.
- **Redirect responses cache with `stale-while-revalidate=2592000` — 30 days.** The probe
  key kept redirecting for over an hour after #79 removed it. **A wrong redirect outlives
  its fix at the edge**, so verify the block *before* merging, never after.

> **The trap, and it will catch the next person: every *cached* response reports `200`** —
> genuine 404s and genuine redirects alike. Only an uncached request shows the true status,
> and session 15 confirmed that a **cold** path does report `307`/`404`/`200` truthfully. So
> **status is informative cold and worthless warm**, and the one tell that holds in both
> states is that **no genuine page response carries a `location:` header**.
>
> | Path class | Status (cold) | `location:` | Body — **re-measured 2026-08-08** |
> |---|---|---|---|
> | key **in** the redirects block | `307` | **1** | ~192 KB |
> | key **not** in the block | `404` | 0 | **~203,300 bytes** (404 shell) |

> **The 404 shell moved again, measured 2026-08-22 on the `v9` space: 203,286 and
> 203,334 bytes**, against the **189,511** recorded here since 2026-08-08 and the
> ~584 KB before that. **Third drift, and the classification has held every time** —
> the same probe returned `307` with a `location:` header for a redirected path, `200`
> with none for two real pages (one 732,174 bytes, one short), and the shell for two
> paths that do not resolve. **Compare classes, not remembered sizes**; the
> discriminator is still that no genuine page response carries a `location:` header.
> Carry both controls — a known-good path and a known-bad one — in every redirect probe.
> | renamed-away path (automatic 307) | `307` | **1** | ~192 KB |
> | real page | `200` | 0 | **786 KB – 1.1 MB** |
>
> **The magnitudes drifted; the classes did not.** That last row read **584 KB** until
> 2026-08-08 and is now 786 KB to 1.1 MB, while the 404 shell is unchanged to the byte.
> **Compare classes, not remembered sizes** — a correct page fails a "~584 KB?" check today.

**Two operational facts, both measured:** GitBook sync latency is **25–45 seconds** from
merge to published, and **PyYAML is not available in this environment** — `ruby -ryaml`
is, and was used for P0-3's parse. D7's parser choice needs deciding, which is Q10.

**Q7 is ruled: one spec, sequenced, and AC7 is now per-split** with partial completion an
explicit valid end state. That ruling is what makes "interruptible" real — the old AC7
required all 26 splits before the spec could be accepted at all. Do not re-open it.

### Verified — do not re-derive these

All re-measured at the session-11 review and **every one held**, which is a first for this
programme: the URL model (**110/110** live, via `python3
tools/urlmap.py --verify`), the slug function (110/110), the
sitemap at **111** entries, **19** sections with every row of requirements §3's size table
exact, six singleton sections, `SUMMARY.md` at 110 links / 110 distinct targets, and
`worklist.md` at **42 rows / 26 split / 16 keep across 41 distinct pages**. `--verify`
exits 2 rather than passing if the site is unreachable.

> **`SUMMARY.md:154` reads ` ## Under the Hood` with a leading space** no other heading
> has. CommonMark tolerates three; at four it stops being a heading and GitBook would fold
> `HowBrighterWorks.md`, `HowServiceActivatorWorks.md` and `ReactorAndProactor.md` into
> *Task Queues* — silently, moving three URLs. **It produced a wrong re-derivation during
> the review itself** (18 sections and five singletons, both wrong); `urlmap.py` scored
> 110/110 anyway because its `^\s*##\s+` is deliberately tolerant, which is correct — it
> must model what GitBook does, not what the file ought to say. **Normalise the line at
> P0-1.**

### Not verified — challenge these

- ~~**§4's target tree.**~~ **Replaced 2026-08-07 by `design.md` §3** — twelve sections,
  all 110 pages placed, written out as `SUMMARY.target.md`. The seven buckets are dead:
  besides the ~40-page *How To*, they had **nowhere to put the Brighter core**, so
  handlers, pipelines, configuration and the external bus would have landed there too.
  **REVIEWED AND APPROVED 2026-08-08** — the tree was the first thing attacked and it
  stands unchanged.
- ~~**How many pages the 26 splits create.**~~ **Derived 2026-08-07: 32, taking 110 →
  142**, row by row from measured `## ` section spans. It is a count of what the design
  decides to create, **not a prediction** — and design §15 says exactly how it moves if any
  of the five §7.7 deviations is overruled (items 1/2/3/5 add 5; item 4 removes 3).
- ~~**Whether a Git-synced `SUMMARY.md` change triggers GitBook's automatic 307.**~~
  **Measured 2026-08-07: it does**, within 25 seconds. See *D0 — EXECUTED* above.
- ~~**Which site plan the published docs are on.**~~ **Section-level redirects work**; no
  plan gate was hit.
- ~~**Whether GitBook's automatic redirects PERSIST.**~~ **CLOSED BY CONSTRUCTION
  2026-08-08, not by measurement — and do not re-open it.** Task 2.10 re-probed PR #77's old
  path and found a `location:` header a day later, pointing at today's URL. It proves
  nothing: **that key is in our own `.gitbook.yaml` block** (line 17, since PR #78), so
  automatic persistence and our block firing are indistinguishable from the response.
  Answering it needs an old path that moved and is *absent* from the block, and after PR 2
  **there is none** — which is exactly what belt-and-braces was for. **What the probe did
  establish is better:** the entry's *repository*-path value **re-resolved across a second
  move of the same page**, naming today's URL rather than the intermediate one. D0 predicted
  it; it is now measured, and it is the property PR 5 leans on to re-parent the two Azure
  Blob pages.
- ~~**Design §7.6's *top-level entries* column — an intention, not a measurement.**~~
  **PINNED 2026-08-08 in `tasks.md` Appendix A**, measured against `SUMMARY.target.md`.
  5 top-level, 27 nested; the section allocation reproduces §7.6 row for row; 142 pages.
  Eleven of twelve entry counts reproduce; ***Outbox and Inbox* lands at 9**, not the
  intended 10, and the breach case §15 named (15 entries) does not arise. **Do not
  re-derive it** — and note it is now the authority, superseding §7.6's column.
- **Every "after" line count in design §7 is arithmetic on today's sections**, not a count
  of a page that exists. `InMemoryScheduler.md` at ~495 is the one to watch — four lines
  under 500 by arithmetic alone, so a banner and two lead-ins put it back over.

### Spec 005, closed 2026-08-07 — and why it was never 13/14

It had shown **13/14** since May. **Task 3.3 was not a task**: a reserved placeholder whose
own note said *"held open in case Task 1.1 surfaces drift that materially changes the
configuration page's code examples **after they've been written** … if 1.1 comes back clean,
mark this task n/a and skip."*

Task 1.1 did **not** come back clean — six drift items against ADR 0053. But it found them
at the pre-flight gate **before** any page was written and folded them into the design
tables in the same pass, and Task 2.2's result confirms every one reached the published
page. **3.3 guards against drift found *after* writing; that is exactly what AC-T3 was
designed to prevent, and it worked.** Marked n/a per its own instruction. **005 is 14/14.**

- **Checked against 010 before closing.** Of the five pages 005 owns, four are only
  re-filed. Only **`BrighterOutboxSupport.md`** has further work, and it is a *split* on
  `worklist.md` (517 lines; `Outbox Archiver` at 151 and `Complete Example` at 168 move
  out). A split relocates content and invalidates none of it, so **005 needed no redoing
  either side of the restructure**.
- **Note for design:** `BrighterOutboxSupport.md` is simultaneously a **split target** and
  one of the four **parent pages** §3.1 relies on for a middle navigation layer. 010 does
  both jobs to that page at once — the core keeps its filename, as both 011 demonstrators
  did.
- **`BoxProvisioning.md#when-to-use-box-provisioning` exists and resolves** (line 7). 009's
  rung 3 links to it for the Option B path. **Do not requalify that heading without
  repointing** — `linkcheck.py` catches it, redirects cannot.
- **The eighth wrong figure, and it was in the tally again.** 005's header said *"Total
  tasks: 15"* and its Phase 2 row said 6 against an actual 5. Real total **14**. The tasks
  were right the whole time. Re-derive:
  `grep -c '^- \[[ x]\] \*\*Task' spec/005-database_migration/tasks.md`

### What session 11 established — Q1 answered, and the risk that replaced it

Full write-up in `spec/010-information_architecture/requirements.md` §§2.2–2.4 and §15.
Every quote below was fetched as **raw bytes** and read in full, which is the point:

- **Q1 is answered by the documentation, and it resolves in our favour.** The blocking
  sentence had been quoted from its second half. In full, from GitBook's *Git Sync
  troubleshooting*: *"as long as a page exists **for a path**, GitBook won't be looking
  for a possible redirect. So if you're setting up a redirect for an old page to a new
  one, you will need to remove the old page in order for the redirect to work."* **The
  rule is the first sentence and it is keyed on the path, not the page.** "Remove the old
  page" is that rule stated for the case where the file still sits on the old path — a
  special case, not an extra condition. Corroborated independently by the published
  resolution order, which falls through to `.gitbook.yaml` *precisely when a URL cannot be
  resolved*.
- **The risk that replaces it is worse, and it inverts the experiment** — automatic 307s,
  consulted first, and **D0 measured that they do fire for our case**. See *D0 — EXECUTED*
  above; this is why D0 had to be two publishes and a comparison rather than one check.
- **The U+200B zero-width spaces came from GitBook's own documentation, and they are
  still there.** Byte-inspecting the *Content configuration* page on 2026-08-06 finds
  U+200B at exactly two positions in its `.gitbook.yaml` example — at `structure:` and
  after `SUMMARY.md` — the same two characters this repo carried until 2026-08-05, **with
  the `redirects:` snippet inside the same code block.** Writing D2 by pasting the
  documented example reintroduces the bug. `.gitbook.yaml` is confirmed clean today (no
  non-ASCII at all). P0-3 now says: **type the block, never paste it.**
- **AC7 is per-split and partial completion is a valid end state** — the maintainer's
  ruling. Without it, Q7's "interruptible" was contradicted by an acceptance criterion
  requiring all 26 splits.
- **Every load-bearing figure was re-derived and all of them held.** The first review in
  this programme to find no wrong number — which is what requirements §14 predicts once
  figures are measured rather than estimated.
- **Three questions were left open rather than decided** — Q8 (AC6's threshold), Q9
  (`llms.txt`'s summary source), Q10 (`urlmap.py` in CI). They are design calls, and
  deciding them in requirements would have been guessing.

### What session 10 established — the URL model, verified 110/110

Full write-up in `spec/010-information_architecture/requirements.md` §2. Cite these; the
whole spec rests on them and they were measured, not assumed:

- **A published URL is `<slug(SUMMARY H2)>/[<slug(ancestor page)>/]*<slug(filename)>`.**
  `slug()` lowercases, collapses non-alphanumeric runs to one hyphen, and trims. A
  predictor reading **only `SUMMARY.md`** reproduces **110 of 110** published URLs against
  the live `sitemap-pages.xml` — no misses, no extras.
- **The path on disk plays no part.** `contents/` is flat and stays flat, so **010 moves
  no files**: it rewrites `SUMMARY.md` and `.gitbook.yaml`.
- **Renaming a section moves every URL beneath it** with nothing else touched —
  *Guaranteed At Least Once* → *Transports* is 11 URL changes and zero file edits.
- **All 763 internal links survive**, because every one of the 110 targets stays put.
  Only inbound *external* links break. That is much narrower than the README's "URL
  breakage is the dominant risk", though **anchor-level links still break on splits** and
  redirects cannot fix fragments.
- **SUMMARY nesting deepens the URL** — 9 pages publish three segments deep, so
  re-parenting moves a URL just as a section change does.
- **`tools/urlmap.py`** (moved there in Task 2.1) packages the predictor: bare to print
  the tree, `--verify` against the live sitemap (**an unreachable authority exits 2, it
  does not pass**), `--redirects OLD_SUMMARY` to emit the `.gitbook.yaml` block. **Proven
  in both directions rather than trusted to pass**: a section rename emits 11 redirects
  including the 3-segment nested one, unnesting the distributed locks emits 7, and a page
  dropped from the tree is **refused with exit 1**.
- **The redirect target is a repository path.** GitBook's syntax is
  `old/published/path: contents/FileName.md`. **The README's example is wrong** — it
  targets `transports/RabbitMQConfiguration.md`, a directory that does not exist and that
  010 does not create.
- **`sitemap-pages.xml` has 111 `<loc>` entries.** A WebFetch summariser reported 127;
  counting the raw XML gave 111, matching session 8. **Count the bytes, not the summary.**

**Two stale claims in 010's README, to fix at design:**

- Its *Out of Scope* section assigns page splitting to 011 and puts page bodies out of
  scope. **Splitting moved into 010 on 2026-08-03** and `worklist.md` names 010 as its
  executor in its own header. Page bodies are in scope, for the 26 splits and for the
  three §7 content defects.
- Its redirect example, above.

**And a seventh wrong figure, this one in `worklist.md` §1:** it says "Twelve of the 42
rows say `keep`". It is **sixteen rows across fifteen distinct pages** —
`TickerQScheduler.md` is listed twice, in §6a and again in §6e. The split/keep breakdown
is **26/16**, not 30/12. No verdict changes, but a plan sized on "30 splits" over-provisions
and one sized on "12 keeps" under-honours the rows whose entire purpose is to stop 010
reopening settled questions. Re-derivation snippet is in requirements §14.

**#67 checked 2026-08-06: still no reply.** The last three comments are all the
maintainer's, so nothing external constrains the design yet. Check again before finalising.

010 needs exactly two things from 011, and both exist:

- **`spec/011-authoring_conventions/worklist.md`** — 42 rows, written to stand alone.
  **Read this instead of 011's other files**; it needs none of them.
- **The conventions in `CLAUDE.md` § Page Conventions**, enforced by `pagelint.py`. Any
  page 010 creates or moves must carry a banner and qualified headings, or CI fails it.

Three things 010 owns that 011 deliberately left for it, all recorded in `worklist.md`:
the `redirects:` block in `.gitbook.yaml` for pages 010 moves, the scheduler-family fold
(§5 — **5 references + 1 shared how-to + 1 enriched overview, not 18 pages**), and the
`HowServiceActivatorWorks.md:147` ↔ `DispatcherConfigurationReference.md` overlap that
Phase 6's split created and no linter can see.

**Before finalising 010, check [#67](https://github.com/BrighterCommand/Docs/issues/67)
for a reply** — Diátaxis-as-authoring-discipline was flagged there for pushback.

### Phase 7 — all done (session 9)

- ~~**Task 7.1**~~ **Done 2026-08-05.** `worklist.md`, 42 rows.
- ~~**Task 7.2**~~ **Done 2026-08-06.** 34 fences tagged, rule 4 a repo-wide error.
- ~~**Task 7.3**~~ **Done 2026-08-06.** `pagelint.py --fix`.
- ~~**Task 7.4**~~ **Done 2026-08-06.** AC1–AC8 walked; AC5 failed and was fixed.
- ~~**Task 3.5**~~ **Done 2026-08-05** against the published site.

### What session 9 established — Phase 7 is done, 011 is closed

Full write-ups in `tasks.md` §§ *Task 7.2 as executed*, *Task 7.3 as executed* and
*Task 7.4 as executed*. Cite these:

- **All 34 untagged fences were prose — 1 `bash`, 33 `text`, no code at all.** That is
  the explanation for the debt, not a curiosity: an author writing C# reaches for
  ```` ```csharp ```` for the highlighting, while an author drawing a diagram has no
  language in mind and types a bare fence. **Corroborated by a number that did not
  move** — the using debt is 802/93 before and after, so nothing tagged was C#.
- **`pagelint.py --fix` exists and was rehearsed against a real V11 bump**: 110 stale
  banners, 110 fixed, page types unmoved (50/33/27), all five Prerequisites segments
  intact, one line changed per page, trailing newlines preserved, second run a no-op.
  **The bump is one edit to `APPLIES_TO` plus `--fix`** — there is no migration map;
  a stale value's product set selects its replacement.
- **`--fix` never decides a page type**, and cannot launder a bad one into a green
  build: given `**Guide** · Applies to **Brighter V9**` it fixes the version, leaves the
  type, and the page still fails rule 2. **Do not extend it to page types.**
- **The language-tag half was validated against ground truth** — the 34 fences 7.2 had
  just tagged by hand. It tags 26, holds 8, and **all 26 match the hand verdict**. It
  has never produced a wrong tag; its only failure mode is doing nothing and saying so.
- **AC5 failed.** The linter emits eight rule labels and the ledger listed seven —
  `NO H1` was never written down, and had never fired because every page has an H1.
  Fixed. A second drift of the same shape: `docs.yml`'s comment still said untagged
  fences were warnings. **Both were changed where enforced, not everywhere described.**
- **Two checks nobody had run, both green**: every page's banner *type* matches its
  reviewed verdict in `pagetypes.tsv` (110/110), and every page's *Applies to* matches
  its `applies` column (110/110, 100/5/5). The 10 Darker-touching pages really are the
  set that column names.
- **AC4 was re-derived without the linter** — 688 distinct non-navigation `##` slugs,
  0 repeated. Confirming rule 3a with the tool that enforces rule 3a proves only
  self-consistency.
- **Still worth doing, not blocking anything:** `docs.yml` could echo the changed-range
  count so a `--changed` run proves its own non-vacuity instead of relying on a note.

### What session 8 established — Tasks 7.1 and 3.5 done, PRs #74 and #75 merged

**PR #74 merged** (`b5500bb`), so all of Phases 1–6 plus Task 7.1 is on `master`.
**Task 3.5 is closed too**, checked against the published site rather than a preview.

- **The banner renders as a real callout.** GitBook emits `<h1>` … `</header>`, then the
  banner as the first element of the content body:
  `<blockquote class="… border-l-2 pl-6 py-3 border-tint …">`. `**Reference**` becomes
  `<strong>`, the ` · ` separators survive as U+00B7, and the Prerequisites link is a
  real `<a href>` returning **200**. 12 pages sampled, **including all five that carry a
  Prerequisites segment** — the segment had never been exercised in a rendered page
  before the Phase 6 splits created them.
- **The obvious check would have proved nothing.** Reading a page through a
  Markdown-converting fetcher returns `> **Explanation** · Applies to …`, which is
  exactly what a correctly-rendered blockquote converts *back* to — and equally what an
  unrendered literal banner looks like. Indistinguishable after conversion, and emitting
  it literally was the whole concern behind requirements Q1. **The check had to be run
  against raw HTML**, and was.
- **The published site is 111 URLs** (`sitemap-pages.xml`) — 110 pages plus the index —
  so all five split pages are live and none is orphaned in the published tree.


`spec/011-authoring_conventions/worklist.md`, **42 rows**. Full write-up in `tasks.md`
§ *Task 7.1 as executed*. Cite these:

- **The cohort is 30 pages at ≥3 modes, not 31, and 13 at four, not 14** — the RabbitMQ
  split and nothing else. Re-scoring every page at `335f078` and diffing against HEAD
  shows **only the seven split-affected pages changed at all**: Phase 4 qualified 260
  headings across 74 files and **moved no page's mode score**.
- **`BrighterBasicConfiguration.md` scored 2, not 3**, so it was never in the 31 —
  `31 − 1 = 30` is the right arithmetic, not `31 − 2`. It was split on being 1,070 lines
  of two different jobs, which the score never showed. **The sixth of the approved
  design's figures to be wrong.**
- **The page-type tally was stale in two places and `pagetypes.tsv` was right
  throughout.** `50 / 32 / 28` here and `48 / 30 / 27` in `classification-notes.md` §11
  both predate §10's own `QueryPipeline.md` Explanation → How-to correction. The truth is
  **50 / 33 / 27**. Nothing ever disagreed with anything, which is why it survived four
  sessions — **re-derive totals, never add to them.**
- **The scheduler family is six pages, not seven.** `classification-notes.md` §6 counts
  `PostgreSQLMessageBroker.md` among them; it is a **transport** (`SUMMARY.md:72`) that
  shares the template by coincidence.
- **The family's migration sections are near-copies** — all "swap the factory" — and its
  comparison sections duplicate `BrighterSchedulerSupport.md`'s existing
  `## Choosing a Scheduler`. So the family resolves to **5 references + 1 shared how-to +
  1 enriched overview, not 18 pages.** One decision, not six.
  **Corrected 2026-08-08:** this said "six migration sections" and "six comparison
  sections". There are **five migration sections** (TickerQ has none) totalling 221 lines,
  and **four comparison sections** totalling 85 (Quartz has none; the fifth name match is
  AWS's own modes table, which stays). `worklist.md` §5a/§6a carry the same wrong count.
  **The rulings are unaffected** — only the tallies were wrong, again.
- **Phase 6's split created an overlap nothing checks for.**
  `HowServiceActivatorWorks.md:147` `## Dispatcher Configuration` (76 lines) now
  duplicates `DispatcherConfigurationReference.md`, created three commits earlier. No
  linter can see this; recorded as a fold for 010.
- **Twelve rows say `keep`, deliberately** — a list of only split candidates would invite
  010 to re-open every page it omitted. `ReactorAndProactor.md` (442 lines, four modes,
  one argument) and `TickerQScheduler.md` (234 lines, template with nothing to move)
  carry the reasoning for *not* splitting.

### What session 7 established — Phase 6 is done, don't redo it

Both splits landed. Full write-up in `tasks.md` § *Phase 6 as executed*. Cite these:

- **`RabbitMQConfiguration.md` 566 → 331** plus `RabbitMQDurability.md` (Explanation),
  `RabbitMQMigrateToQuorumQueues.md` and `RabbitMQConnectionStability.md` (How-to).
- **`BrighterBasicConfiguration.md` 1,070 → 237** plus
  `CommandProcessorConfigurationReference.md` (673) and
  `DispatcherConfigurationReference.md` (234). Both cores kept their file name; the
  6 inbound `#configuring-the-dispatcher` links never moved, and **28 anchor links
  across 20 pages were repointed** to the new reference pages.
- **Design §10's "moved verbatim" premise was inverted, and it nearly failed the
  build.** A new page is 100% added lines, so every moved block is *already* strict
  under `--changed` — 42 using-less blocks, 42 errors the moment a PR opened. Rule 6
  now honours the `// ...` escape its own message had always advertised: **it
  downgrades to a warning and never silences**, so the debt stays counted. Ruled by the
  maintainer over backfilling the blocks or exempting new files. **Do not re-raise it.**
- **No information loss was checked mechanically, not by eye**, on both splits — every
  substantive line of each original tested for verbatim presence across the resulting
  pages. Only the deliberate edits came back: 24 lines (RabbitMQ: one repointed anchor
  + the 23 folded `Best Practices` items) and 4 lines (three stranded links + one
  redundant lead-in).
- **`.gitbook.yaml` needed no redirects, and its zero-width spaces were worse than
  recorded.** The key was literally `​structure`, so **GitBook had never read that
  block** — it fell back to defaults naming the same two files, which is why nothing
  looked broken. Fixed and verified by byte inspection. See *Platform facts*.
- **One deliberate departure from Task 6.3**: `Ack and Nack` stayed in the Reference
  core rather than moving to the migration how-to, because it contains no migration
  content and would have re-mixed the modes the split exists to separate.
- **`apply_banners.py` was stripping prerequisites, and is fixed** (`5498cd6`). The five
  new pages were also missing from `pagetypes.tsv`, so a version bump would have skipped
  them. Both closed — the TSV is 110 rows and the sweep preserves a segment it still
  refuses to author. See *Tooling* for how to test it if you touch it.

### What session 6 established — Phase 5 is done, don't redo it

`pagelint.py` runs in CI, repo-wide and PR-strict, and **the gate is demonstrated rather
than assumed**. Full write-up in `tasks.md` § *Phase 5 as executed*. Cite these:

- **The `pull_request` event resolves `origin/<base_ref>`.** Proven by PR #73, a
  deliberate one-character probe, closed unmerged. The contingency `git fetch origin
  ${{ github.base_ref }}` **is not needed and was not added** — `fetch-depth: 0` suffices
  even though the checked-out ref is a merge commit.
- **`--changed` reporting 0 errors on this branch is true, not vacuous.**
  `changed_ranges('origin/master')` returns 118 files and 465 hunks; the branch's edits
  are banner and heading lines and genuinely overlap no C# block. Checked, because that
  reading has the same shape as the failure Task 5.2 exists to catch.
- **Block granularity works in both directions.** One character changed inside the
  using-less block at `AWSSQSConfiguration.md:29` turns *that block alone* into an error
  — the other fifteen on the page stay warnings — while the repo-wide step stays green
  at 0 errors in the same run.
- **The `versions` job is 009 D9's slot, pre-built.** Guarded by
  `if [ -f tools/versioncheck.py ]`, with a daily `schedule:` trigger. The guard
  propagates exit codes under `bash -e {0}` — verified, including **2** — so 009's "exit
  2 is not a pass" survives it. **Remove the guard when D9 lands**; a guard inherited
  rather than removed un-gates the check silently.

### What session 5 did, and the four findings worth carrying

Rule 5 cleared (`741890f`) and all of Phase 4 (`d232324`, `64dd111`, `0b1b841`). Full
write-ups in `classification-notes.md` §8 and `tasks.md` § *Phase 4 as executed*. Do not
re-derive these:

- **Task 4.5's premise was wrong — there were no duplicate pairs to merge.**
  `Glossary.md` defines `Dispatcher` and `CloudEvents` **once each**; the audit's second
  line number for `Dispatcher` (`:393`) points into the unrelated `Timeout` entry. What
  actually collided was a *section* heading with its own first *term* two lines below,
  so `#dispatcher` resolved to the section. Fixed by naming the sections for what they
  hold — `## Dispatcher and Consumers`, `## CloudEvents and Encodings`. **Consequence:
  Spec 009's D12 is not blocked**; it can link `Glossary.md#dispatcher` today.
- **Task 4.4 predicted 8 anchor links to repoint; there are 18 anchors carrying 28
  links.** Where an anchor had inbound links the *other* page moved instead —
  `BasicConcepts.md` keeps `## Command` (8 links), `BrighterBasicConfiguration.md` keeps
  `#configuring-the-dispatcher` (6 links, 4 cross-page) — which cut the rewrites to 19.
- **Two links had always resolved to the wrong section** while passing `linkcheck.py`,
  because the anchor existed just not where the author meant.
  `KafkaConfiguration.md:227` said "see Configuration Callback *below*" and landed on
  the publication hook above it. That is rule 3a's argument in miniature.
- **Rule 5 cannot see headings**, and that is now a ruling rather than an open question.

> **Ruled by the maintainer 2026-08-04: leave rule 5 as it is** — the risk is low enough
> to be a future problem, solved then. **Do not re-raise it**; it is accepted, not
> overlooked. Rule 5 stays a prose rule, `CLAUDE.md`'s ledger stays correct as written.

### What Phase 4 left for Phase 6 — both done in session 7

- ~~**`BrighterBasicConfiguration.md` keeps its bold `## **Configuring The Dispatcher****
  and its anchor.**~~ **Done 2026-08-05 in the split** (`df2d3a5`). Every heading on the
  page shed its bold; `slug()` ignores emphasis, so all 6 inbound links to
  `#configuring-the-dispatcher` were unaffected, as predicted. The H2 itself stayed
  behind when its children moved to `DispatcherConfigurationReference.md`, which is
  precisely what let the children move at all.
- **`qualify.py` and `dedupe_within.py`** are kept in `spec/011-authoring_conventions/`
  beside `apply_banners.py`. `qualify.py`'s `SUBJECT`/`OVERRIDE`/`KEEP` tables **are**
  Task 4.1's reviewed proposal list — if a heading needs rethinking, edit the table and
  re-run rather than hand-editing pages.

### Blocked, not forgotten

- **The Darker pages** — deferred on the maintainer's instruction, see below.
- ~~**A PR for this branch**~~ — **PR #74 merged 2026-08-05** as `b5500bb`. The
  `--changed` gate ran for real on all 25 commits and reported 0 errors. **One caveat
  worth carrying:** on that branch a live `--changed` run and a vacuous one produce
  identical output, because every block the splits moved is marked `// ...` and
  downgrades to a warning either way. Non-vacuity was proven separately — locally at 118
  files / 465 hunks, and by forcing the gate red. **Consider echoing the changed-range
  count from `docs.yml`** so a future run proves itself instead of relying on this note.

## Spec 009 — decisions and findings (2026-08-03)

Parked but current; 009 has not started. Cite these, don't re-derive them.

### What 009's design review changed

Six findings, all applied; `design.md` went 468 → 693 lines. Cite these, don't
re-derive them.

- **New deliverable D12 — five missing glossary terms**, P0, sequenced *first* because
  it gates rung 3. `at-least-once`, `Box Provisioning`, `partition`, `consumer group`
  and `offset` are linked by the tutorials and **do not exist** in `Glossary.md`. Absent
  is different in kind from badly-explained: it fails 009's own AC8, so it belongs to
  009 and not to 010/013. All glossary links now carry `#anchor`, which puts them under
  `linkcheck.py`'s MISSING ANCHOR check. Confirmed present: `#command` `#event`
  `#command-processor` `#handler` `#outbox` `#sweeper` `#subscription` `#publication`
  `#routing-key` `#reactor` `#message-pump` `#partition-key`. Note `Dispatcher` is
  defined twice (`Glossary.md:95`, `:393`) — wait for 011 step 6 to merge them before
  linking it.
- **D9's version authority moved to NuGet.** Requirements assumed
  `../Brighter/release_notes.md`; CI has no sibling Brighter checkout, so that made the
  gate unrunnable anywhere but a laptop — the exact failure mode D9 exists to prevent.
  Now `api.nuget.org/v3-flatcontainer/paramore.brighter/index.json`, highest
  non-prerelease, with `--release-notes PATH` kept as an offline fallback. Exit `2`
  (authority unreachable) is **not** a pass. Runs on PR *and* daily, because the
  triggering event is a release in another repo.
- **D9 and D11 now have outlines at all** — previously they appeared only in the file
  tree and one sequencing row.
- **Verification item 3 resolved**, closing all six. See the pump correction above.
- **Rung 4's prerequisite is rungs 2 *and* 3** in banner, prose and diagram — it was
  stated three different ways.
- **Licence-region elision** is AC3's second documented exception (alongside the
  `.csproj` divergence). Every sample `.cs` opens with a ~25-line MIT `#region Licence`;
  page blocks start at the first `using`.
- **PR-stall contingency**: D1 is unexposed (reuses `HelloWorld`, no PR). D2/D3 wait
  rather than inlining the sample — that would fail AC4, not satisfy it. Ship whatever
  prefix of the ladder is complete, and escalate on #67 rather than going quiet.

### The 009 finding most likely to bite

**Samples reference Brighter by `ProjectReference` into `../../../src/`, not
`PackageReference`** (see `samples/CommandProcessor/HelloWorld/HelloWorld.csproj`),
and their third-party references carry no `Version` (central package management). So
Brighter's CI proves the sample compiles against **tip-of-tree source**, never against
the released package a reader installs. Tutorial samples keep that convention; the
prose shows `PackageReference` with a pinned version, and the `.csproj` is the one
place page and sample legitimately differ. Three partial guarantees compose to cover
it — CI, `tools/versioncheck.py` (to build), and a timed clean-machine run per
release. Full table in `requirements.md` § Q1.

### Two further 009 decisions taken 2026-08-03

- **Rung 4 runs Kafka as a Reactor, not a Proactor.** Verified supported:
  `KafkaMessageConsumer` implements `IAmAMessageConsumerSync`
  (`KafkaMessageConsumer.cs:47`) and `KafkaSubscription` already defaults to
  `MessagePumpType.Reactor` (`KafkaSubscription.cs:298`). The single-threaded pump is
  then visible in the code rather than argued around. **Consequence:** the tutorial
  sample needs a **sync** handler and mapper; `KafkaTaskQueue` supplies only the
  `…Async` pair.
- **Rung 3 uses Box Provisioning.** `UseBoxProvisioning(opts =>
  opts.AddPostgreSqlOutbox(cfg))` creates the table on the fresh-install path at
  startup — no migration assemblies, no generated DDL, no second terminal. Packages:
  `Paramore.Brighter.BoxProvisioning` + `…BoxProvisioning.PostgreSql`, on top of the
  Outbox package. The page must still state the `CREATE TABLE` rights requirement and
  link to `BoxProvisioning.md#when-to-use-box-provisioning` for the Option B path.

**Verification items resolved during 009's design (2026-08-03)** — cite, don't re-derive:

- **Kafka partition key**: `KafkaMessagePublisher.cs:79` sets
  `Key = message.Header.PartitionKey.Value`, so ordering is *per key*
- **Offset commits**: batched, `commitBatchSize` default **10**
  (`KafkaMessageConsumer.cs:122`, `:319`, `:855`); offsets **are** committed for
  revoked partitions on rebalance (`:245`, `:895`), so the rebalance demo is safe. A
  crash redelivers up to one batch — that's rung 4's at-least-once point
- **Provisioning method is `AddPostgreSqlOutbox`**, not `AddPostgresOutbox`
  (`PostgreSqlBoxProvisioningExtensions.cs:17,48`) — requirements corrected
- **Rung 3's transaction shape**: copy
  `samples/WebAPI/WebAPI_Dapper/GreetingsApp/Handlers/AddGreetingHandlerAsync.cs` —
  `GetConnectionAsync` / `GetTransactionAsync` → business write → `DepositPostAsync`
  → `CommitAsync`. Rung 3 deliberately **omits `ClearOutboxAsync`** so the Sweeper is
  visibly the thing that dispatches

All six verification items are now closed. The only thing still to establish at
writing time is the exact pinned versions — which is `versioncheck.py`'s job, not a
research question.

---

## Decisions already made — do not relitigate

**Diátaxis is an authoring discipline, not top-level navigation.** A literal
four-bucket split would shred each technology into four pages across 7 transports,
8 outboxes, 6 inboxes, 7 schedulers and 7 distributed locks. Readers navigate that
material by technology, so those families stay grouped and get mode discipline
*within* pages.

**No information loss.** The restructure re-files and re-titles; it does not cull.

> **The `BasicConcepts.md` → `Glossary.md` merge is withdrawn (2026-08-04).** Ruled by
> the maintainer at 011's Task 3.2. The separation is deliberate: `BasicConcepts.md` is
> a curated 24-term orientation set a newcomer can read *without* working through the
> 100-term `Glossary.md`. The audit measured the overlap and inferred duplication from
> it without asking why the smaller set existed. Specs 002 and 006 both maintain the
> page on purpose, which is the corroboration. **Per-term links from `BasicConcepts.md`
> into the matching `Glossary.md` anchor replace the merge.** 010's README is amended;
> full note in `spec/011-authoring_conventions/classification-notes.md`.

**Front matter is ruled out — use a visible banner instead.** GitBook's own docs say
`.gitbook.yaml` is the supported route, not front matter, and
[GitbookIO/gitbook#1079](https://github.com/GitbookIO/gitbook/issues/1079) reports
front matter rendering literally into the page body. The replacement, below the H1:

```markdown
> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)
```

Better for both audiences: readers see it, and it survives the front-matter stripping
retrieval chunkers apply.

**Config reference drift is handled by tooling, not discipline.** Spec 012 builds a
checker that reflects over Brighter assemblies and diffs against our option tables,
exiting non-zero on mismatch.

**Kafka gets its own tutorial rung** because Brighter's single-threaded pump lines up
with consumer-group membership — per-partition ordering and safe offset commits fall
out of the architecture rather than being fought for. A genuine differentiator,
currently invisible to newcomers.

> **Corrected 2026-08-03 at 009's design review.** An earlier draft of this file said
> the pump "maps one-to-one onto consumer-group partition assignment". **That
> overstates it.** It is one pump per *performer*, hence per **group member** — not one
> pump per partition. `noOfPerformers` defaults to **1** (`Subscription.cs:201`) and
> `Dispatcher.CreateConsumers` builds one `Consumer`, so one channel and one pump, per
> performer (`Dispatcher.cs:589`). At the default, one process is one group member and
> Kafka assigns it *every* partition, which its single thread drains sequentially.
> Per-partition — hence per-key — ordering holds because of that single thread, not
> because of a per-partition pump. Do not let the looser phrasing reach a page.

---

## Sanctioned exception to the Brighter read-only rule

`CLAUDE.md` has been amended (committed on the branch). Tutorial sample code **may** be
added to `../Brighter/samples/` and the `../Darker/` equivalent:

- **Always via pull request** against the source repo. Never a direct commit.
- **Preference order**: reuse existing sample → extend existing sample → write new.
- **Samples directories only.** `src/`, `tests/`, `docs/adr/`, release notes and every
  other directory remain strictly read-only.

Rationale: tutorial code living only inside a markdown fence rots silently; in
`samples/` it is compiled by Brighter's CI.

**Survey done 2026-08-03** (Brighter at **10.7.0**) — full table in 009's
`requirements.md` § Current State. Conclusions:

- Rung 1 → **reuse `samples/CommandProcessor/HelloWorld` as-is** (114 lines, already
  minimal)
- Rung 2 → **new `samples/Tutorials/02-FirstMessage`** from `RMQTaskQueue` minus
  Serilog, `CustomPublicationFinder`, second event type
- Rung 3 → **new `samples/Tutorials/03-DurableOutbox`**. `WebAPI_Dapper` is not
  tutorial-shaped: 4 databases × 2 transports by env var, migration assemblies,
  telemetry, and a README step to hand-edit an absolute DB path before running the
  Sweeper in a second terminal
- Rung 4 → **new `samples/Tutorials/04-Kafka`** from `KafkaTaskQueue` minus the Polly
  registry; it already creates a 3-partition topic with a consumer group

---

## Audit data (measured 2026-08-03 — cite rather than re-derive)

**Re-measured by script 2026-08-03 during 011's requirements. Some earlier figures
were wrong — these supersede them.** Method and full tables in
`spec/011-authoring_conventions/requirements.md`; regenerate with
`python3 spec/011-authoring_conventions/modemix.py` from the repo root.

- **105** pages under `contents/`, 33,952 lines, **as measured 2026-08-03**. Was 107
  until `VersionBegin.md` and `VersionEnd.md` were deleted (see below); earlier notes
  said 110. **Now 110 pages** — the two Phase 6 splits added five — and `linkcheck.py`
  reports "112 files" = 110 pages + `SUMMARY.md` + `README.md`. Every figure below is
  the 2026-08-03 baseline unless it says otherwise; treat "105" in this section as
  historical
- **`VersionBegin.md` / `VersionEnd.md` deleted 2026-08-03** on the maintainer's
  instruction — a hack predating GitBook's support for multiple versions. Nothing linked
  to them, neither was in `SUMMARY.md`, each held an H1 plus one line. `linkcheck.py`'s
  `NON_CONTENT` exemption existed **solely** for these two and was removed with them, so
  every page under `contents/` must now be reachable from `SUMMARY.md` with no
  exemptions, and `pagelint.py` needs no exemption list. **Every page count in 011 is
  105**; the heading figures below are unaffected, since neither file had an H2
- ~~**0 of 105** pages carry any page metadata~~ — **all 110 do.** 105 as of
  2026-08-04, plus the 5 Phase 6 split pages. 0 lacked an H1 and 0 opened with a
  blockquote after the H1, so the insertion was mechanical and unambiguous exactly as
  predicted: 212 insertions, 0 deletions. **Re-running the sweep is safe and
  idempotent** — true again as of `5498cd6`, and it had quietly stopped being true the
  moment the splits created pages carrying a Prerequisites segment. A corrected verdict
  costs one `pagetypes.tsv` edit and a re-run
- **17 of 110 files do not end with a newline** (was 18 of 105; one gained one in the
  Phase 6 splits, re-derived 2026-08-06). Left alone deliberately — normalising
  them inside the banner commit would have put 18 unrelated changes into a diff whose
  whole claim was that it contained nothing but banners. Tidy separately if at all
- Heading collisions **across** pages: 53 texts on more than one page, 297 instances.
  41 are navigation (`Further Reading` 29, `Related Documentation` 10, `See Also` 2)
  and stay uniform → **measured by `pagelint.py` 2026-08-04: 262 instances across 50
  texts normalised**, against 256 across 50 raw. The predicted "48 normalised" was
  wrong — `slug()` lowercases as well as stripping emphasis, so it merges case variants
  too, and two of those pairs (`How It Works`/`How it works`, `**Configuring The
  Dispatcher**`/`Configuring the Dispatcher`) were *not* collisions raw and become
  collisions normalised. That is the +6. Full derivation in `tasks.md` § *Measured
  baseline*. The emphasis merges are —
  `## **Configuration**` ×4 into `Configuration` ×22 (→26), and `## **NuGet Packages**`
  ×4 into `NuGet Packages` ×5 (→9). All four bold H2 texts live on the four outbox
  pages (`MSSQLOutbox`, `MySQLOutbox`, `PostgresOutbox`, `SqliteOutbox`) and collide
  under either comparison. Worst: `## Best Practices` 26, `## Configuration` 26,
  `## Troubleshooting` 14, `## Summary` 14, `## Usage` 13, `## Overview` 10.
  **All resolved 2026-08-04 (`0b1b841`) — rule 3a is at 0.** These figures are now
  history; do not re-measure them expecting to find collisions
- Heading collisions **within** a single page (H2–H4): **12 pages, 34 instances** —
  `InMemoryOptions.md` alone accounts for 12 (`When to Use` ×5, `Configuration` ×5,
  `Example Usage` ×3, `Limitations` ×3). Found late, at 011's design review; the first
  rule draft missed this class entirely. **All 12 pages are enumerated in 011's
  `tasks.md` under Task 2.8** — the design's own table named ten and said "+ 2 more".
  **All resolved 2026-08-04 (`d232324`, `64dd111`) — rule 3b is at 0**
- Cross-page **H3** duplication is deliberately *not* a defect: 39 texts on multiple
  pages, 112 instances. `### Basic Configuration` under
  `## Hangfire Scheduler Configuration` is perfectly attributable
- **940** C#-tagged code blocks, only **136 (14%)** with `using` directives — leaving a
  debt of **804 blocks across 89 pages**, which is the baseline AC1 records — **now 802
  across 93** after the Phase 6 splits redistributed them and dropped two duplicates. **Measured
  at 796/133 by a script matching `^```csharp`**, which misses the **143 blocks written
  ```` ``` csharp ```` with a space.** CommonMark trims the info string, so those are
  ordinary C# blocks that render as such
- **36** fenced blocks carry no language tag, across 14 pages — **not 185.** The 185
  figure counted those same 149 space-separated info strings as untagged; `36 + 149 =
  185` reproduces it exactly. One corpus fact explains both discrepancies, and it shrank
  P1 Task 7.2 from 185 fences to 36 — then **34**, after the RabbitMQ split tagged two
  log-output fences ```` ```text ````. **All 34 were tagged 2026-08-06 and rule 4 is now
  a repo-wide error, so these figures are history — 0 untagged fences remain.** The 149
  space-separated fences are still space-separated, still correct, and still not a
  defect
- **299** internal links carry an anchor. The predicted **8** pointing at a generic
  anchor was wrong: the real figure was **28 links across 18 anchors**, of which 19 were
  repointed and the rest preserved by moving the *other* side of the collision. All done
  in `0b1b841`
- Mode mixing: **14** pages score all four Diátaxis modes, **31** score three or more,
  **21** are >500 lines with ≥3 modes
- Largest pages **as measured 2026-08-03**: `QueryPatterns.md` 1289,
  `CQRSWithBrighterAndDarker.md` 1142, `BrighterBasicConfiguration.md` 1068,
  `ReplayOnSeen.md` 1037. **`BrighterBasicConfiguration.md` is now 237** — split
  2026-08-05, so the top three are the first, second and fourth of these
- ~~Worst mode-mixing: `RabbitMQConfiguration.md` (566 lines — reference + explanation +
  how-to + guidance interleaved), `BrighterBasicConfiguration.md`~~ — **both split
  2026-08-05**, and they were the two demonstrators precisely because they scored worst.
  `worklist.md` inherits the rest
- Two glossaries: `BasicConcepts.md` (24 terms) is a subset of `Glossary.md` (100)
- ~~**Content defects found while measuring**: `Glossary.md` defines `Dispatcher` twice
  and `CloudEvents` twice; `FAQ.md` asks "When should I use Reactor vs Proactor?"
  twice~~ — **this was wrong on all three counts, disproved 2026-08-04 by reading the
  files.** The Glossary defines each term **once**; the cited second line number for
  `Dispatcher` (`:393`) is inside the unrelated `Timeout` entry. What collided was a
  *section* heading with its own first *term*. `FAQ.md`'s second heading held a
  one-line signpost, not a second answer. Fixed as heading renames in `d232324`; full
  account in `tasks.md` § *Phase 4 as executed*

Regenerate the mode-mixing and heading analysis with
`python3 spec/011-authoring_conventions/modemix.py` from the repo root — it supersedes
the ad-hoc `grep | sort | uniq -c` used in session 1, which double-counted by not
excluding navigation headings.

---

## Platform facts (verified against GitBook docs; redirect behaviour re-read 2026-08-06)

`.gitbook.yaml` declares only `root`, `structure.readme`, `structure.summary` — **no
redirects block**. Every published URL derives from the current tree, so 010 must add
one.

```yaml
redirects:
  guaranteed-at-least-once/rabbitmqconfiguration: contents/RabbitMQConfiguration.md
```

- **The key is the old *published* path; the value is the *repository* path** to the
  markdown file, relative to `root`. Because `contents/` stays flat, every target in this
  repo is `contents/<FileName>.md`. **010's README shows `transports/…`, a directory that
  does not exist and that 010 does not create — do not copy it**
- **No leading slashes** on either side
- **Malformed indentation disables redirects silently** rather than erroring — verify
  mechanically, don't eyeball

**How GitBook resolves a URL — three steps, and the order is what matters:**

1. Site content resolves to its canonical URL, **following any automatically created
   redirects** (HTTP 307, created "whenever pages are moved or renamed")
2. **If the URL cannot be resolved**, it is checked against **section-level redirects** —
   the `.gitbook.yaml` block above
3. Finally, site-level redirects (the dashboard UI; documented as **Premium and Ultimate
   site plans** — a gate that has never been checked for this site)

**All three steps were exercised against the live site on 2026-08-07 (D0). Measured, not
inferred:**

- **A redirect fires for a page that still exists but publishes elsewhere.** The rule is
  *"as long as a page exists **for a path**"* — path-keyed. Step 2's precondition is our
  exact case. **This closed 010's Q1.**
- **Step 1 fires for our case, and it masks step 2.** A Git-synced `SUMMARY.md` rename
  **does** count as a "move": the old path returned **307** twenty-five seconds after the
  rename merged, with no `redirects:` block in the repo at all. So **never conclude "the
  redirect works" from a single successful request** — it proves only that *something*
  redirected.
- **Step 2 works too, and the plan permits it.** Proven with a probe key for a path that
  had never existed, against a control key absent from the block. **The value is a
  repository path and GitBook resolves it to wherever that page currently publishes**, so
  an entry does not go stale when its page moves again. **Confirmed across a SECOND move
  2026-08-08**: after PR #85 moved a page that PR #77 had already moved, the D0 entry's
  `location:` header named the *new* URL, not the intermediate one. **This is what PR 5
  depends on** to re-parent the two Azure Blob pages.
- **Redirects cache hard**: `stale-while-revalidate=2592000`, thirty days. A wrong redirect
  outlives its fix at the edge. **Verify before merging.** Session 15 also saw
  `s-maxage=773` on a warm redirect, so the edge revalidates on its own clock.
- **Sync latency: 25–45 seconds** from merge to published. **Session 14 saw a page live on
  the first poll** and **session 15 measured PR #85 live between 15 and 30 seconds**, so
  treat 25s as a floor rather than a typical value. **`sitemap-pages.xml` is the readiness
  signal only for added pages** — after a pure re-tree it stays at 111 and tells you nothing.
  The usable signal is a **new** URL returning a large body.
- **On a COLD cache the true status code shows through.** Session 15 measured `307` on 67 of
  74 freshly-moved paths, `404` on a never-existing control and `200` on real pages. The
  cached-200 trap applies to **warm** responses: the remaining 7 of the 74 reported `200`
  *with* a `location:` header, and they were exactly the paths probed by hand minutes
  earlier. **Status is informative when the path is cold and worthless once it is warm** —
  so the `location:` header remains the only tell you can rely on unconditionally.
- **A four-segment published path works.** Measured 2026-08-08 (PRs #83/#84) with a **new**
  page at a path that had never existed — present in `sitemap-pages.xml`, `200`, **no
  `location:` header**, 529,080-byte body. GitBook's own documentation independently
  publishes 30 of 182 pages at four segments below its site root. **Five segments is still
  untested**, and GitBook's own site does not go there either. Full write-up: design §17.
- **Every cached response reports `200`**, genuine 404s and genuine redirects alike. The
  reliable tell is that **no genuine page response carries a `location:` header**. See the
  fingerprint table under *Suggested next session*.

> **Where the zero-width spaces came from — established 2026-08-06.** They are in
> **GitBook's own published `.gitbook.yaml` example**, today: byte-inspecting the *Content
> configuration* page finds U+200B at `structure:` and after `SUMMARY.md`, the same two
> positions this repo carried, **and the `redirects:` snippet sits inside that same code
> block.** So writing 010's D2 by pasting the documented example reintroduces the bug into
> a file whose failure mode is silence. **Type config blocks; never paste them**, and
> assert on bytes. `.gitbook.yaml` is clean today — verified, no non-ASCII at all.

~~**`.gitbook.yaml` contains two U+200B zero-width spaces**~~ — **fixed 2026-08-05**
(`6661073`, Task 6.12). Worth keeping because of what it turned out to mean: the key was
literally `​structure:`, **so GitBook had never read that block at all.** It fell back
to its defaults, which happen to name `README.md` and `SUMMARY.md` — the same two files
the block specified — which is exactly why nothing ever looked broken. The value was
`SUMMARY.md​` to match, so fixing only the key would have converted a silently-ignored
block into a silently-broken one. Both characters and the trailing double-spaces are
gone, verified by byte inspection. **If you edit this file, verify it the same way** —
combined with the silent-failure behaviour above, an invisible character in a YAML key
is that failure mode at its worst.

**No page-level redirect was required by the splits** (Task 6.12's other half). Both
cores kept their original file name, so no published URL moved; the new pages are new
URLs with nothing to redirect *from*. What broke was **anchor-level** links, which
GitBook redirects cannot address — they operate on pages, not fragments — so they were
repointed directly. Adding a `redirects:` block remains Spec 010's deliverable.

---

## Tooling

- `.github/workflows/docs.yml` — **the repo's CI, added 2026-08-04; the gate completed
  2026-08-05.** Two jobs. `check` runs `linkcheck.py`, then `pagelint.py` repo-wide, then
  on pull requests `pagelint.py --changed "origin/$BASE_REF"` — `base_ref` goes through
  `env` rather than into the command line, because git ref names may contain `;`, `&` and
  `$`. `versions` is 009 D9's pre-built slot, a no-op behind
  `if [ -f tools/versioncheck.py ]`, with a daily `schedule:` trigger. `fetch-depth: 0`
  is what makes the merge-base resolvable and is confirmed sufficient on a real
  `pull_request` event
- `python3 tools/linkcheck.py` — reports MISSING FILE, MISSING ANCHOR, WRONG CASE,
  ORPHAN. Exits non-zero. **Run after every change that adds or retargets links, and
  after adding any page.** Clean (**`144 files checked`**), **now in CI.** Its `NON_CONTENT`
  exemption was removed 2026-08-03 with the two version-marker pages, so the orphan
  check has no exemptions at all
- `python3 tools/pagelint.py` — **added 2026-08-04.** Six rules: banner presence and
  grammar, heading uniqueness across pages (H2) and within a page (H2–H4), language
  tags, `ServiceActivator` in prose, `using` directives. Imports `md_files`, `slug` and
  `HEADING_RE` from `linkcheck.py` and filters to `contents/` itself. Accepts paths;
  `--changed <ref>` makes the code rules errors **for blocks overlapping the diff**, not
  whole files. Exit 1 on errors, 2 on bad arguments or an unusable git history — it
  refuses to pass vacuously. **In CI as of 2026-08-05, and proven to fail the build** —
  see *What session 6 established*.

  **Current state: 0 errors, 791 warnings across 142 pages** — and **0 errors under
  `--changed origin/master` too**. Every rule it enforces is satisfied on every page,
  and the 791 warnings are **exactly** the using-directive debt (791 blocks across
  113 pages, rule 6) with nothing else mixed in: rule 4's 34 became errors and were
  cleared in session 9. That debt is deliberate and is AC1's baseline. **It was 802
  until session 16**, when Phase 3's fold removed five C# blocks with the duplicated
  section that held them — the first genuine reduction, rather than a sweep — then 793
  after Phase 4 and **791** after Phase 5. **`--changed` only sees staged files**, so
  `git add` before trusting a local run.

  **Every `--changed` run so far has been vacuous, and each for a different reason.**
  PRs #82/#85/#86 touched no file under `contents/`; PR #87 touched three, but the rule
  is strict **per block overlapping the diff** and that diff was five single prose
  lines. It first bites in **Phase 4**, the first phase to create a page. Until then,
  confirm it live by probe rather than inferring it from a pass.

  **`--fix` (Task 7.3) repairs two rules and refuses the rest.** It retargets a stale
  banner *version segment* and tags an untagged fence ```` ```text ```` when nothing in
  the block looks like code. **A version bump is one edit to `APPLIES_TO` plus
  `python3 tools/pagelint.py --fix`** — proven on a full V11 rehearsal. It **never**
  decides a page type and cannot launder a bad one into a green build; do not extend it
  to page types, to rule 6, or to `--changed` (which it rejects with exit 2).

  **Rule 6 has an escape, added 2026-08-05 (`e0faceb`):** a C# block that marks its
  omission `// ...` stays a **warning even under `--changed`**, and is still counted.
  Without it the two splits could not have moved a single block verbatim — a new page
  is 100% added lines, so every moved block is already strict. The remedy was already
  advertised by rule 6's own message and by `CLAUDE.md`; only the implementation was
  missing. **It downgrades, never silences** — do not "simplify" it into an exemption.

  **`APPLIES_TO` is the single source of truth for banner versions.** `CLAUDE.md`
  documents it and `apply_banners.py` imports it. `BANNER_RE` validates a banner;
  **`BANNER_SHAPE_RE` recognises one as ours** even when its vocabulary is stale — the
  two differ at exactly every version bump.
- `python3 spec/011-authoring_conventions/apply_banners.py` — the sweep. **Kept past its
  scheduled deletion**, and used five times: the sweep, two page-type corrections, the
  Darker version retarget, and the Phase 6 pages. Reads `verdict`, never `proposed`; a
  blank verdict is a hard stop that writes nothing. Delete it when the verdicts are
  final.

  **It never authors a Prerequisites segment but now preserves one** (`5498cd6`). It had
  been stripping them: the script builds a banner from `verdict` and `applies` alone and
  overwrites whatever it finds, which was harmless only while no page exercised the
  segment. The splits made five that do, and the next run deleted all five. If you touch
  this script, the test is a **forced** rewrite — "110 unchanged" proves nothing, since a
  no-op run cannot lose anything.

  `pagetypes.tsv` is **110 rows**. Its order is not sorted by any single rule, so
  **append new rows** rather than re-sorting; re-sorting churns 57 reviewed rows for
  nothing.
- `python3 spec/011-authoring_conventions/qualify.py` — the cross-page heading sweep
  (Tasks 4.1/4.2). **Run bare to print the plan; `--apply` to write it.** It rewrites
  headings *and* repoints every link whose anchor moved, in one pass, so there is no
  window where the tree is inconsistent. To rethink a heading, edit its `SUBJECT`,
  `OVERRIDE` or `KEEP` entry and re-run rather than hand-editing pages.
- `python3 spec/011-authoring_conventions/dedupe_within.py` — the within-page sweep
  (Task 4.3). Line-targeted, because the whole point is telling repeated texts apart on
  one page. Verifies every target line before writing any file, so a stale line number
  aborts the run rather than half-rewriting the tree.
- `python3 tools/urlmap.py` — **the URL predictor, and the only tool in this repo validated
  against the live site.** **Moved from `spec/010-information_architecture/` in Task 2.1
  (D3), 2026-08-08** — and the move needed a one-line fix nothing would have caught:
  `REPO` was `parents[2]`, which from `tools/` resolves *above* the repository. Four modes:

  | Mode | Does | In CI |
  |---|---|---|
  | *(bare)* | prints the predicted tree | no |
  | `--verify` | checks against `sitemap-pages.xml`; **exit 2 if unreachable — not a pass** | **no**, deliberately — it would make the build flaky |
  | `--redirects OLD_SUMMARY` | emits the `.gitbook.yaml` block; **exit 1** if a page is in one tree and not the other | no |
  | `--check-shape` | **S1** ≥2 pages/section, **S2** ≤12 top-level entries, **S3** ≤4 URL segments, and no `SUMMARY.md` heading with leading whitespace | **yes** |
  | `--check-redirects` | every value resolves to a real file, every key no longer publishes, **every byte printable ASCII** | **yes** |

  Its section regex is `^\s*##\s+`, tolerant of leading whitespace **on purpose** — it must
  model what GitBook does, not what the file ought to say; `--check-shape` is what asserts
  the file does not *rely* on that tolerance. **Both new checks were proven red before being
  trusted green** (session 15), and `--check-shape` needed no synthetic probe: on the old
  19-section tree it reported nine real failures. **The byte check is not a style rule** — a
  YAML parser would have parsed `​structure:` happily, which is why GitBook never read that
  block for months.
- **GitBook already ships the LLM-facing layer** — `/llms.txt`, `/llms-full.txt`, a `.md`
  variant of every page, an `?ask=` endpoint and an MCP server at `/~gitbook/mcp`, all
  automatic. **This is not planned work; it exists today.** What it lacks is per-page
  descriptions and any V9/V10 discrimination — see *What session 14 found* above, and
  **do not build `llms.txt` before Tasks 10.1–10.3 rule on what D6 is for**
- Planned: `llms.txt` generator (010) — **re-scoped, see above**; `optioncheck` (012), `versioncheck` (009 —
  diffs versions pinned in tutorial prose against the latest non-prerelease on NuGet,
  exits non-zero on drift; **exit 2, authority unreachable, is not a pass**)

---

## Darker versions independently of Brighter (established 2026-08-04)

**`Paramore.Darker`'s latest release is 4.1.1.** Its tags — 4.1.1, 4.1.0, 4.0.1, 4.0.0,
3.0.0, 2.0.79 — are an independent line. **There is no Darker V10 and there never has
been.** 011's design assumed otherwise and its banner vocabulary shipped that claim onto
10 pages before it was caught; corrected to
`APPLIES_TO = ('Brighter V10 and Darker V4', 'Brighter V10', 'Darker V4')`, defined once
in `tools/pagelint.py`.

**Darker's next release is in flight** — `../Darker` HEAD is `4.1.1-7-g2f76cda`, ahead of
the deployed version. **Do not update Darker page content from that working tree**: it
documents unreleased behaviour, and the docs site publishes the deployed version. The ten
Darker-touching pages are positioned for a clean update when it lands — they are
identifiable from the `applies` column of `pagetypes.tsv`, and the bump is one edit to
`APPLIES_TO`, one to that column, and a re-run of `apply_banners.py`.

**Darker is Brighter's sister project — the query side to Brighter's command side.** Its
documentation sits alongside and often parallels Brighter's without being the same. Four
of the five parallel page pairs already agree on page type. The one asymmetry worth
knowing: `QueryPipeline.md` is **928 lines** against `BuildingAPipeline.md`'s **177**,
because the Darker page absorbs decorators and Polly configuration that Brighter splits
across three pages. That is an architectural difference, and it is **worklist §5b** —
`QueryPipeline.md` splits along the seams Brighter already has, restoring the parallel.

---

## Open questions needing a human decision

1. ~~**009** — pin versions or float?~~ **Resolved 2026-08-03: pin, verified per
   release**, with a release-checklist item to re-run all four tutorials.
2. ~~**009** — multi-broker Kafka compose?~~ **Resolved 2026-08-03: no.** Rebalancing
   comes from partitions × consumer instances; the existing single-broker compose plus
   a second consumer app shows it.
3. ~~**011** — page-splitting threshold?~~ **Resolved 2026-08-03: not by size, and
   splitting moved to 010.** 011 ships two demonstrator splits plus a scored worklist;
   010 executes the rest while re-filing, so split pages are touched once. **Both
   demonstrator splits landed 2026-08-05, and the worklist 010 executes against is
   `spec/011-authoring_conventions/worklist.md`.**
4. ~~**011** — banner above or below the H1?~~ **Resolved 2026-08-03: below**, applied
   to all 105 pages 2026-08-04, and **confirmed in the rendered page 2026-08-05** —
   GitBook emits it as a real `<blockquote>` immediately after the `<h1>`. Fully closed.
5. **012** — checker in C# or Python; reflect over sibling checkout or restored NuGet
   packages?
6. ~~**010** — three questions deliberately left for design~~ — **all three answered
   2026-08-07 in `design.md`, and APPROVED at review 2026-08-08.** Q8: **≤12 top-level entries** per section,
   ≥2 pages, **≤4 URL segments** (was 3 until measured 2026-08-08, design §17) — counting
   *entries* rather than pages is what makes 32 pages
   in *Outbox and Inbox* navigable (design §4). Q9: the summary is **the page's own opening
   sentence**, extracted, with the build failing when it is unusable (§9.2). Q10: **yes**,
   `--check-shape` and `--check-redirects` in CI, and **no YAML dependency** — PyYAML is
   absent and `ruby -ryaml` is an accident of the machine, so D7 parses the flat block in
   ~15 lines and asserts on bytes (§9.1). **Q2/Q3/Q4/Q5/Q6 are answered too**; only Q7 was
   already ruled. **All of it was reviewed and approved 2026-08-08** — see *What session 13 established*.
7. ~~**010** — does a Git-synced `SUMMARY.md` change trigger GitBook's automatic 307?~~
   **Resolved 2026-08-07 by publishing: yes, within 25 seconds, with no redirects block in
   the repo.** `.gitbook.yaml` redirects were separately proven to work, against a control.
   **D2/D3 are belt-and-braces rather than load-bearing** — ship them anyway, because
   whether automatic redirects *persist* is still unknown. Requirements §16.
8. ~~**010** — do redirects fire for a page that still exists but publishes
   elsewhere?~~ **Resolved 2026-08-06 from the source: yes.** The rule is path-keyed —
   *"as long as a page exists **for a path**"* — and the published resolution order falls
   through to `.gitbook.yaml` exactly when a URL fails to resolve. The earlier draft had
   quoted only the consequence clause. See *Platform facts*. **Do not re-raise it**; what
   remains open is item 7, which is a different question.
9. **011 / Darker** — **does `Applies to **Brighter V10**` wrongly exclude Darker on any
   of the 96 pages that now carry it?** On a cross-cutting page that banner is an
   *exclusion claim*: it tells a Darker reader the page is not for them. Some are
   certainly right — `RequestValidation.md` says in its own text that validation applies
   to Brighter requests and not Darker queries. The unverified ones are the middleware
   and resilience pages: `PolicyRetryAndCircuitBreaker.md`, `PolicyFallback.md`,
   `UsingTheContextBag.md`, `FeatureSwitches.md`, `Telemetry.md`. `QueryPipeline.md`
   documents Polly for Darker, so the resilience story is shared in substance even where
   the pages are not. **Settling this needs the Darker source, which is currently ahead
   of the deployed release — so it waits** (see *Darker versions independently* above).
   The banner has at least made an implicit claim explicit on every page at once, which
   is the first time it has been checkable at all.
10. **011** — **two verdicts are the assistant's call, not a maintainer ruling.**
   `CustomScheduler.md` → How-to, and `V10MigrationGuide.md` → How-to, the latter
   **contradicting approved design §1** which argued Reference. Recorded in
   `classification-notes.md` §7 with the reasoning and what to revert if overruled.
11. ~~**011** — should rule 5 also check headings?~~ **Resolved 2026-08-04: no, leave it
   as a prose rule.** The risk is low enough to be a future problem, solved then. The
   gap is accepted rather than overlooked — **do not re-raise it.** Recorded in
   `classification-notes.md` §8 and `tasks.md`.

---

## Public commitments (we said we'd do these)

Posted to [#67](https://github.com/BrighterCommand/Docs/issues/67#issuecomment-5165867593)
and [PR #72](https://github.com/BrighterCommand/Docs/pull/72), visible to the issue
author:

- PostgreSQL-for-both-transport-and-outbox how-to ships in the **first batch** of spec
  013 guides — his own example, and there is currently no such page
- Redirects will preserve existing links through the restructure. **Observed working on
  the live site 2026-08-07** — both GitBook's automatic 307 and the `.gitbook.yaml` block,
  the latter against a control. The commitment is now backed by measurement rather than by
  documentation. **The one caveat to keep**: nothing establishes that automatic redirects
  persist, which is why the block ships anyway
- Diátaxis-as-authoring-discipline and the prose-vs-generated reference distinction
  were both flagged for pushback; **check #67 for a reply before finalising 010 or 012**

#67 stays open until the work lands. Don't close it on spec approval.

---

## Standing constraints

`CLAUDE.md` is the authority. Most likely to bite:

- Never modify `../Brighter` or `../Darker` except the tutorial-samples exception above
- Always update `SUMMARY.md` when adding or moving pages; never create orphans
- Test all code examples before finalising
- V10 patterns only; mark deprecated features
- Prefer "Dispatcher" over "ServiceActivator"
