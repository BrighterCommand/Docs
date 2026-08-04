# Tasks: Spec 011 — Page Type Discipline and Machine-Readable Conventions

**Created:** 2026-08-03
**Revised:** 2026-08-03 at review — six findings applied; `design.md` and
`requirements.md` amended with them (see *Applied at review* below)
**Design:** [`design.md`](design.md) (approved 2026-08-03)
**Requirements:** [`requirements.md`](requirements.md) (approved 2026-08-03)

### Applied at review (2026-08-03)

1. **Rule 3a compares `slug()`, not raw text** — expect **48** colliding texts at Task
   2.8, not 50. Instances are 256 either way. New section below; design §3 gains
   *Rules 3a and 3b compare `slug()`*; requirements § Measurements carries both columns
2. **Rule 3b's twelve pages enumerated in full** under Task 2.8 — design §3 named ten
   and closed with "+ 2 more", which made that task's check unrunnable. The two missing
   are `AWSSQSConfiguration.md` and `BrighterSchedulerSupport.md`, and
   `InMemoryOptions.md` has a fourth repeat the old table omitted
3. **Task 4.5 traced** — the three content merges had no row in the traceability table
4. **Acceptance criteria renumbered 1–8** in requirements; two were both numbered 6.
   Task 7.4 and design's traceability row updated
5. **Tasks 2.4 and 2.6 are not parallel** — both are branches of `check_code_blocks`
6. **`md_files()` is repo-wide, not `contents/`** — noted in Task 2.1, along with
   confirmation that importing `linkcheck.py` is safe (it has a `__main__` guard)

## Overview

**43 tasks across 7 phases.** The phases *are* design §12's ten sequencing steps,
regrouped only where consecutive steps share a verification. The order is load-bearing —
each step is verifiable before the next begins, and no step depends on a rule that is not
yet true. Do not reorder.

| Phase | Design §12 steps | Goal | Tasks |
|---|---|---|---|
| 1 | 1–2 | Green CI baseline on the untouched tree; conventions written down | 1.1–1.4 |
| 2 | 3 | `pagelint.py` written and run **locally**, counts reconciled | 2.1–2.9 |
| 3 | 4–5 | Page types reviewed by a human; banner sweep across 105 pages | 3.1–3.5 |
| 4 | 6 | Heading de-duplication + the three content merges | 4.1–4.6 |
| 5 | 7 | `pagelint.py` gates the build; `docs.yml` ready for 009's D9 | 5.1–5.3 |
| 6 | 8–9 | The two demonstrator splits, 5 new pages, redirects | 6.1–6.12 |
| 7 | 10 | Worklist for Spec 010; P1 items; acceptance pass | 7.1–7.4 |

### Session boundary — stop after Phase 2

Per the programme plan, **the first execution session runs Phases 1 and 2 only.** Phase 2
ends by comparing the linter's output against numbers predicted by throwaway scripts. If
they disagree materially, *the rules are wrong, not the corpus* — stop and revise the
design rather than sweeping. Starting the 105-page sweep in the same session that first
runs the linter forfeits that check.

---

## Three corpus facts established while writing this list (2026-08-03)

All three are cheap to verify and all three would otherwise surface as confusing failures
mid-sweep. The third was added at this list's own review.

### The corpus is 105 pages — `VersionBegin.md` and `VersionEnd.md` are deleted

`contents/` held 107 `.md` files, two of which were not pages:

```
contents/VersionBegin.md   # H1 "Version Beginning" + the line "Beginning of Version"
contents/VersionEnd.md     # H1 "Version End"       + the line "End of Version"
```

They were a hack predating GitBook's support for multiple versions, and were **deleted on
the maintainer's instruction (2026-08-03)** rather than exempted. Nothing linked to them,
neither appeared in `SUMMARY.md`, and `linkcheck.py` is clean before and after.

**This is the better outcome than an exemption list.** Every page under `contents/` is now
reader-facing, so `pagelint.py` needs no notion of an exempt page and the banner rule has
no "does this one count?" edge. `linkcheck.py`'s `NON_CONTENT` set existed solely for these
two files and has been removed with them, which also tightens the orphan check: a page
that is not reachable from `SUMMARY.md` must now be either linked or deleted.

| Design §12 step 3 said | Expect instead | Why |
|---|---|---|
| **107** banner errors | **105** | Two non-pages deleted, not exempted |
| 256 cross-page heading errors (50 texts) | **unchanged** | Rule 3a is H2-only; neither file had an H2 |
| 34 within-page heading errors (12 pages) | **unchanged** | Same reason |

`requirements.md` and `design.md` have been updated to 105 throughout, with the
reconciliation recorded in requirements § Measurements. The corpus is now **105 pages,
33,952 lines**; `linkcheck.py` reports **107 files** = 105 pages + `SUMMARY.md` +
`README.md`. `modemix.py` globs `contents/*.md` (`modemix.py:20`) and so now sees 105
without modification; its heading figures were never affected.

### `.gitbook.yaml` contains zero-width spaces

Verified by byte inspection — the file is:

```
b'root: ./\n\n\xe2\x80\x8bstructure:  \n    readme: README.md  \n    summary: SUMMARY.md\xe2\x80\x8b\n'
```

Two **U+200B ZERO WIDTH SPACE** characters (`e2 80 8b`): one immediately before
`structure:`, one immediately after `SUMMARY.md`. The key is literally `​structure:`,
and both content lines carry trailing double-spaces. This compounds the design's warning
that malformed indentation disables GitBook redirects *silently rather than erroring* —
see Task 6.12, which must strip these rather than edit around them.

### Rule 3a's text count depends on normalisation; its instance count does not

Found at this list's review, by running the collision analysis both ways. Four H2 texts
are emphasised, all four on the four outbox pages (`MSSQLOutbox.md`, `MySQLOutbox.md`,
`PostgresOutbox.md`, `SqliteOutbox.md`): `## **Provisioning the Outbox Table**`,
`## **NuGet Packages**`, `## **Database Table Schema**`, `## **Configuration**`. Two of
them have plain twins elsewhere — `Configuration` ×22 and `NuGet Packages` ×5 — which are
distinct texts raw and the same text after `slug()`, which is what GitBook does when it
builds the anchor.

| Comparison | Colliding texts | …needing qualification | Instances |
|---|---|---|---|
| Raw — the figure quoted throughout requirements | 53 | **50** | **256** |
| `slug()`-normalised — **what `pagelint.py` does** | 51 | **48** | **256** |

**Expect 48, not 50, at Task 2.8.** The instance count is 256 either way; only the text
count moves, by exactly those two merges — `## Configuration` rises from 22 instances to
26, `## NuGet Packages` from 5 to 9. Without this written down, Phase 2's reconciliation
would trip its own stop-rule over a normalisation choice nobody had recorded. Design §3
now states the rule (*Rules 3a and 3b compare `slug()`, not raw text*) and requirements
§ Measurements carries both columns. All four emphasised texts need qualifying under
either comparison, since each already appears on four pages; they are a named sub-case
for Task 4.2 because qualifying them is also the moment to drop the emphasis, which is
the convention nowhere else in the corpus.

---

## Phase 1 — Baseline and Conventions (design §12 steps 1–2)

Goal: the repository's first CI workflow, green on a tree nothing has touched, and the
conventions recorded in `CLAUDE.md` before any tool enforces them.

- [x] **Task 1.1:** Create `.github/workflows/docs.yml` running `linkcheck.py` alone
  - Input: design §4 (the YAML is given complete and runnable), `tools/linkcheck.py`
  - Output: `.github/workflows/docs.yml` — first workflow in the repo; `.github/` does not exist yet
  - Notes: Ship **only** the `linkcheck.py` step now. `fetch-depth: 0` from the start, even though nothing needs the merge-base until Task 5.1 — adding it later invites forgetting it. `actions/checkout@v4` and `actions/setup-python@v5` are the only external actions permitted, and both tools are stdlib-only, so the build cannot fail for reasons unrelated to the docs. **Must be green on the untouched tree** — this is the baseline that makes every later failure attributable (AC6). Verify by pushing the branch and reading the run, not by asserting it locally.

- [x] **Task 1.2:** Amend `CLAUDE.md`'s *File Organization Pattern*
  - Input: design §2a (amended text quoted verbatim), requirements § "This contradicts `CLAUDE.md` today"
  - Output: `CLAUDE.md`, the *File Organization Pattern* list — same skeleton, same ordering, plus the banner at position 2 and the qualification rule at position 4
  - Notes: As written today the documented standard prescribes `## Configuration`, `## Best Practices`, `## Common Pitfalls` and `## Sample Code` — four of the worst collisions. An author following it produces a page the linter rejects. **Same commit as Task 1.3**; either alone leaves the file contradicting itself.

- [x] **Task 1.3:** Add the *Page Conventions* section to `CLAUDE.md`
  - Input: design §2b (six-subsection table), §1 (banner grammar + 5 worked examples), §7 (`llms.txt` format), requirements § The Conventions
  - Output: `CLAUDE.md`, new *Page Conventions* section with six subsections: page banner · heading qualification · version markers on code · complete code blocks · `llms.txt` · enforcement
  - Notes: Copy `BANNER_RE` and the five-item navigation allowlist **verbatim** — this section and `pagelint.py` must not drift, and the allowlist is declared canonical in requirements. Separator is ` · ` (U+00B7 with a space either side), not a hyphen or pipe. State the `Reference`-as-catch-all rule and why a fifth vocabulary value was rejected. The ❌/✅ version-marker pair needs a **real** V9→V10 example — take one from `V10MigrationGuide.md` rather than inventing one. Commit with Task 1.2.

- [x] **Task 1.4:** Build the `CLAUDE.md` ↔ linter rule ledger
  - Input: Tasks 1.2–1.3 output, design §3 rules table, requirements § D4
  - Output: A short two-column table in the *Enforcement* subsection: each convention ↔ the linter rule that checks it
  - Notes: AC5 is bidirectional — every documented convention has a rule, every rule is documented. Writing the mapping down before the linter exists means Phase 2 implements against a list rather than a memory. A rule in only one of the two places is how the next round of decay begins.

**Verified by:** green CI run on the untouched tree; `CLAUDE.md` reviewed and internally
consistent. No code depends on `CLAUDE.md` yet, so review is the only gate.

---

## Phase 2 — The Linter (design §12 step 3)

Goal: `tools/pagelint.py`, all six rules, run **locally against the untouched tree**. Not
in CI — it will fail loudly, and that is the point.

- [x] **Task 2.1:** Write the `pagelint.py` skeleton — page set, imports, output contract
  - Input: design §3 (signatures given), `tools/linkcheck.py` in full (225 lines — the pattern to follow)
  - Output: `tools/pagelint.py` — module docstring, `from linkcheck import md_files, slug, HEADING_RE`, `NAV_ALLOWLIST`, `BANNER_RE`, `PAGE_TYPES`, a `Finding` record, `main()` with optional path arguments and the exit-code contract
  - Notes: **Import, do not duplicate** (requirements Q5). Verified at review that this works as designed: `linkcheck.py` defines `md_files` (`:85`), `slug` (`:55`) and `HEADING_RE` (`:52`) at module scope and guards its entry point with `if __name__ == '__main__'` (`:219`), so importing it runs nothing. Mirror its conventions so someone who has read one can read the other: stdlib only, single file, `path:line: RULE: message`, exit 1 on any error, 0 when only warnings, 2 on bad arguments. The page set for page-level rules is **every** file under `contents/` — all 105 are reader-facing now that the two version markers are deleted, so there is no exemption list to carry and none should be invented. **`md_files()` is not that set:** it walks the whole repo minus `SKIP_DIRS`/`SKIP_FILES` (`linkcheck.py:44`, `:47`), so it returns root `README.md` and `SUMMARY.md` too. `pagelint.py` filters to `contents/` itself — otherwise the first run reports a missing banner on `SUMMARY.md` and the reconciliation starts with a bug to explain. `README.md` and `SUMMARY.md` are not pages.

- [x] **Task 2.2:** Implement rules 1 and 2 — banner presence and grammar
  - Input: design §1 (`BANNER_RE` given complete), design §3 rules table
  - Output: `check_banner(path, lines) -> list[Finding]`
  - Notes: Rule 1 — the first non-blank line **after the H1** must be the banner. Rule 2 — it must match `BANNER_RE`. Measured: 0 of 105 pages currently open with a blockquote after the H1, so there are no legacy exceptions and no ambiguity about whether a banner is present. Error message must show a ready-to-paste example (design §3 output sample).

- [x] **Task 2.3:** Implement rules 3a and 3b — heading uniqueness, two different scopes
  - Input: design §3 "Rule 3 is two rules", design §5, requirements § The navigation allowlist
  - Output: `check_headings(all_pages) -> list[Finding]`
  - Notes: **3a is H2-only, across pages. 3b is H2–H4, within one page.** The asymmetry is deliberate and documented: 39 H3 texts appear on multiple pages (112 instances) and are *not* defects, because an H3 is read under its H2. **Both compare `slug(text)`, not raw markdown** — see the normalisation finding at the top of this file and design §3; `NAV_ALLOWLIST` is likewise matched normalised, so an emphasised `## **Further Reading**` stays exempt. Uniqueness is a property of the corpus, so when invoked on a subset the function loads every page for context but reports only on the requested ones — exactly how `linkcheck.py` handles orphans. Error message names the other pages and proposes the qualifier.

- [x] **Task 2.4:** Implement rule 4 — language tag on every fence (warning)
  - Input: requirements § Measurements (185 untagged blocks), design §3 rules table
  - Output: `check_code_blocks()`, language-tag branch
  - Notes: **Warning repo-wide until P1 Task 7.2 lands**, error under `--changed`. Track fence state properly — an untagged fence *closing* a tagged block is not a violation.

- [x] **Task 2.5:** Implement rule 5 — `ServiceActivator` in prose
  - Input: design §3 "Rule 5 needs care", requirements § D4 final paragraph
  - Output: `check_terminology(path, lines) -> list[Finding]`
  - Notes: Three legitimate uses a naive ban would fire on: inside fences (`ServiceActivatorHostedService`, `Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection`), inside inline code spans, and throughout `HowServiceActivatorWorks.md`. Scan **prose outside fences and outside backticks**, and honour a file-level opt-out: `<!-- pagelint: allow-serviceactivator -->`.

- [x] **Task 2.6:** Implement rule 6 — `using` directives in C# blocks (warning, counted)
  - Input: requirements Q4 (the two strictness levels), § Measurements (796 C# blocks, 133 with `using`)
  - Output: `check_code_blocks()`, `using`-directive branch, plus the counted summary line
  - Notes: **Warning repo-wide, with a count**, so the debt is visible rather than either ignored or blocking. Requirements Q4 is explicit that the banner sweep must not trigger this rule — one mechanical line per page is not "touching" a page for code-completeness purposes, or the sweep drags 796 hand-verified edits behind it.

- [x] **Task 2.7:** Implement `--changed` with **block** granularity
  - Input: design §3 "Rule 6 and `--changed`", requirements § D4 final note
  - Output: `changed_ranges(merge_base) -> dict[path, list[(start, end)]]`, and strict-mode dispatch in `check_code_blocks`
  - Notes: A code block is *strict* only if it **overlaps a changed line range**. File-level strictness would mean a typo fix on a 700-line page obliges backfilling every block on it — which penalises exactly the small corrections we want people to make, and the predictable outcome is that they stop making them. Derive ranges from `git diff` against the merge-base. Degrade honestly if git history is unavailable (non-zero, stating why) rather than silently passing.

- [x] **Task 2.8:** Run `python3 tools/pagelint.py` repo-wide and reconcile the counts
  - Input: the untouched tree; expected counts from design §12 step 3, **corrected per the table at the top of this file**
  - Output: Saved raw output for comparison (scratch, not committed)
  - Notes: **This is the real test of the phase.** Expect **105** banner errors (rule 1, one per page — the corpus is 105 pages, see above), **256** cross-page heading errors across **48** texts (rule 3a, comparing `slug()` — 50 if you compare raw, and that difference is a normalisation choice rather than a bug, see the finding at the top of this file), **34** within-page errors across 12 pages (rule 3b). **Check rule 3b page by page against the table below, not just the total** — the same total from different pages means the rule is finding something else. **A materially different number means a rule is wrong, not the corpus.** Stop and revise the design before sweeping anything.
  - Rule 3b's twelve pages, complete (design §3 previously named ten and closed with "+ 2 more", which made this check unrunnable as written):

    | Page | Repeats | Instances |
    |---|---|---|
    | `InMemoryOptions.md` | `When to Use` ×5, `Configuration` ×5, `Limitations` ×3, `Example Usage` ×3 | 12 |
    | `HandlerFailure.md` | `What It Does` ×4, `When to Use It` ×4 | 6 |
    | `PostgreSQLMessageBroker.md` | `How It Works` ×2, `Transactional Messaging` ×2, `Configuration` ×2 | 3 |
    | `ReactorAndProactor.md` | `Handlers` ×2, `Message Mappers` ×2, `Middleware/Attributes` ×2 | 3 |
    | `RabbitMQConfiguration.md` | `Best Practices` ×3 | 2 |
    | `Glossary.md` | `Dispatcher` ×2, `CloudEvents` ×2 | 2 |
    | `AWSSQSConfiguration.md` | `AWS SDK v4 Support` ×2 | 1 |
    | `BrighterSchedulerSupport.md` | `Configuration` ×2 | 1 |
    | `CustomScheduler.md` | `Implementation Steps` ×2 | 1 |
    | `FAQ.md` | `When should I use Reactor vs Proactor?` ×2 | 1 |
    | `ImplementAQueryHandler.md` | `Complete Example` ×2 | 1 |
    | `KafkaConfiguration.md` | `Configuration Callback` ×2 | 1 |
    | | | **34** |

    `AWSSQSConfiguration.md` and `BrighterSchedulerSupport.md` are the two that were unnamed. Note `InMemoryOptions.md` has a **fourth** repeat the old table missed (`Example Usage` ×3), so Task 4.3 qualifies 12 headings there, not 10.

- [x] **Task 2.9:** Record the measured baseline
  - Input: Task 2.8 output
  - Output: A short *Measured baseline (date)* section appended to this file — errors by rule, the `using`-directive warning count and the pages it spans, plus any discrepancy against prediction and its explanation
  - Notes: AC1 requires the warning count "printed and recorded as the baseline to shrink", so it needs to live somewhere durable. Also update `PROMPT.md`'s audit-data section where the linter contradicts a scripted figure — that file is what the next session reads first.

**Verified by:** the counts above. `pagelint.py` is **not** added to `docs.yml` in this
phase — CI must stay green.

### ⛔ Stop here in the first execution session.

---

## Phase 3 — Classification and the Banner Sweep (design §12 steps 4–5)

Goal: a human page-type verdict for every page, then one mechanical 105-file commit.

- [ ] **Task 3.1:** Generate `pagetypes.tsv` — the proposals
  - Input: design §1 "Assigning a page type to 105 pages", `SUMMARY.md`, `modemix.py` scores
  - Output: `spec/011-authoring_conventions/pagetypes.tsv` — 105 rows, one per page: `path` · `proposed type` · `confidence` · `signal that fired` · `human verdict` *(blank)*
  - Notes: Four signals, in order: `SUMMARY.md` section · title verb (gerund/imperative → How-to) · single-mode score from `modemix.py` · everything else to a **review queue, not a default**. A wrong page type is worse than a slow one — it tells the reader the page is something it is not. The generator may be a throwaway script beside the spec.

- [ ] **Task 3.2:** Human review of every row
  - Input: `pagetypes.tsv`, design §1 "Pages expected to need argument"
  - Output: Verdict column complete on all 105 rows
  - Notes: Budget review time on the flagged ones: `FAQ.md`, `ShowMeTheCode.md` (a showcase — Reference fits poorly, but it is not a tutorial; Spec 009 addresses the underlying problem), `WhyBrighter.md` (Explanation), `V10MigrationGuide.md` (consulted → Reference), `Glossary.md` (Reference), and the 14 four-mode pages — whose difficulty in choosing *is* the worklist signal, so capture that difficulty for Task 7.1 as you go.

- [ ] **Task 3.3:** Write `apply_banners.py`
  - Input: design §1 "The sweep script is a separate, throwaway deliverable"
  - Output: `spec/011-authoring_conventions/apply_banners.py`
  - Notes: Reads the **verdict** column, **never** the proposal. **A blank verdict is a hard stop:** print the offending rows and exit non-zero **without writing to a single page**. Do not sweep the reviewed rows and skip the rest — half a sweep produces a 60-file diff that looks deliberate, and the missing banners then surface as linter errors with no sign that a human decision was skipped rather than a page missed. Deliberately *not* folded into `pagelint.py --fix`: the sweep is P0 and `--fix` is P1, and a design that makes a P0 step depend on a P1 tool is wrong. Lives beside the spec because it is used once; **delete it after Task 3.4**.

- [ ] **Task 3.4:** Run the sweep — one mechanical commit
  - Input: `apply_banners.py`, reviewed `pagetypes.tsv`
  - Output: 105 files under `contents/` each gaining one banner line and one blank line below the H1; `apply_banners.py` deleted
  - Notes: **Its own commit**, carrying nothing else — a 105-file diff is reviewable only if it is genuinely mechanical, and the linter rather than the reviewer verifies uniformity. Verify: rules 1 and 2 report **zero**; `linkcheck.py` still clean (banners carrying prerequisite links add link targets to check).

- [ ] **Task 3.5:** Rendered-preview sanity check
  - Input: the published GitBook site after the sweep reaches a published branch
  - Output: Confirmation that the banner renders as a callout below the title, and a note in this file if it does not
  - Notes: The one leftover from requirements Q1. Not a blocker — a blockquote is ordinary Markdown and nothing about it is version-specific — but worth doing once on the first page that ships, since it is now on all 105.

**Verified by:** rules 1 and 2 at zero; `linkcheck.py` clean; every verdict human-reviewed
(AC3).

---

## Phase 4 — Heading De-duplication (design §12 step 6)

Goal: rules 3a and 3b at zero, with the three content defects **merged rather than
renamed**.

- [ ] **Task 4.1:** Propose qualifiers for the 50 colliding texts
  - Input: design §5 "Deriving the qualifier", `pagelint.py` rule 3a output
  - Output: A reviewed proposal list (scratch): page · current heading · proposed heading
  - Notes: The qualifier is the page's subject, from its H1 with filler removed. **The script proposes, a human edits** — `## Hangfire Best Practices` beats `## Hangfire Scheduler Best Practices` and no rule can tell you that. Worst offenders: `Best Practices` ×26, `Configuration` ×26 (22 plain plus 4 emphasised, which the linter's normalisation merges), `Troubleshooting` ×14, `Summary` ×14, `Usage` ×13, `Overview` ×10.

- [ ] **Task 4.2:** Apply cross-page qualification — 256 instances across 50 texts
  - Input: Task 4.1 proposals
  - Output: Edits across the affected pages under `contents/`
  - Notes: Mechanical commit, no prose changes riding along. The three allowlisted texts that repeat (`Further Reading` ×29, `Related Documentation` ×10, `See Also` ×2 — 41 instances) **stay exactly as they are**; their uniformity is a feature. **Named sub-case — the four outbox pages.** `MSSQLOutbox.md`, `MySQLOutbox.md`, `PostgresOutbox.md` and `SqliteOutbox.md` each carry the same four **emphasised** H2s: `## **Provisioning the Outbox Table**`, `## **NuGet Packages**`, `## **Database Table Schema**`, `## **Configuration**`. All 16 need qualifying, and qualifying them is the moment to drop the emphasis too — `## **Configuration**` on `PostgresOutbox.md` becomes `## PostgreSQL Outbox Configuration`, not `## **PostgreSQL Outbox Configuration**`. Bold H2s are the convention nowhere else, and `slug()` ignores emphasis, so dropping it leaves the anchor unchanged. Task 6.7 does the same thing on `BrighterBasicConfiguration.md`, the only other page with bold H2s.

- [ ] **Task 4.3:** Apply within-page qualification — 34 instances across 12 pages
  - Input: the complete rule 3b table under Task 2.8 (design §3's copy is now complete too)
  - Output: Edits to those 12 pages
  - Notes: Worst is `InMemoryOptions.md` — `When to Use` ×5, `Configuration` ×5, `Example Usage` ×3, `Limitations` ×3, i.e. **12** qualifications on one page, producing `#when-to-use-1`, `#when-to-use-2` … on chunks that sit next to each other. `Example Usage` was missing from the design's original table; use the Task 2.8 copy. Within a page a duplicate is **always** a defect. `RabbitMQConfiguration.md` is on this list and Phase 6a dissolves all three of its `### Best Practices` — that happens by luck; fix it here anyway so the outcome is deliberate and the page is correct before the split.

- [ ] **Task 4.4:** Repoint the 8 same-page anchor links
  - Input: design §5 "The anchor links are all same-page" (all 8 listed with line numbers)
  - Output: Updated links in the 7 distributed-lock pages (`[Provisioning](#provisioning)`) and `FAQ.md:8` (`[Configuration](#configuration)`)
  - Notes: All 8 are **same-page** links — no file prefix — so each is fixed in the same edit that renames its heading, leaving no window in which the repo is inconsistent. Run `python3 tools/linkcheck.py` after Tasks 4.2–4.4; it reports any missed one as MISSING ANCHOR. External inbound links from blog posts cannot be fixed from here; 8 is small and will never be smaller than it is now.

- [ ] **Task 4.5:** Merge the three duplicate-content pairs — **separate commit**
  - Input: design §8 "Content defects surfaced by rule 3b"
  - Output: `contents/Glossary.md` — one `Dispatcher` entry (currently `:95` and `:393`), one `CloudEvents` entry; `contents/FAQ.md` — one "When should I use Reactor vs Proactor?" answer
  - Notes: **Do not let the mechanical pass qualify the second heading of each pair.** That turns a visible duplicate into a permanent one, hidden behind a heading that now looks intentional. A glossary with two entries for one term needs the entries merged, and the merged text reviewed as a content change — hence its own small commit. Spec 009 D12 is **waiting on the `Dispatcher` merge** before it can link `Glossary.md#dispatcher`; say so in the commit message.

- [ ] **Task 4.6:** Verify the phase
  - Input: `pagelint.py`, `linkcheck.py`
  - Output: Rules 3a and 3b at zero; `linkcheck.py` clean
  - Notes: AC4 is "no `##` heading text appears on more than one page, except the navigation allowlist" — the linter reporting zero *is* the check. Confirm the remaining rule-1/2 count is still zero, i.e. no page was edited in a way that displaced its banner.

---

## Phase 5 — Gate the Build (design §12 step 7)

Goal: `pagelint.py` in CI, and `docs.yml` shaped so Spec 009's D9 is one step rather than
a restructure.

- [ ] **Task 5.1:** Add `pagelint.py` to `docs.yml`
  - Input: design §4 (full YAML), Task 1.1 output
  - Output: `.github/workflows/docs.yml` gaining a repo-wide `pagelint.py` step and a pull-request-only `--changed origin/${{ github.base_ref }}` step
  - Notes: Only now, once the sweeps have made it pass. Either tool failing fails the build (requirements § D5). Green build with both tools closes AC6.

- [ ] **Task 5.2:** Verify `--changed` actually resolves its merge-base in CI
  - Input: the first pull-request run after Task 5.1
  - Output: Confirmation the `--changed` step ran and compared against something real
  - Notes: `actions/checkout@v4` with `fetch-depth: 0` is *expected* to make `origin/<base_ref>` available, but on a `pull_request` event the checked-out ref is a merge commit and this is worth confirming from a real run rather than assuming. **A `--changed` step that silently finds no changed ranges passes vacuously** — the worst outcome, since the strict code rules would then never fire. Deliberately provoke it: open a throwaway PR touching one C# block with no `using`, and confirm the build goes red. If `origin/<base_ref>` does not resolve, add an explicit `git fetch origin ${{ github.base_ref }}`.

- [ ] **Task 5.3:** Shape `docs.yml` for Spec 009's `versioncheck.py`
  - Input: `spec/009-getting_started_tutorials/design.md` § D9 (lines 425–478) and its Sequencing step 6
  - Output: `docs.yml` gaining a daily `schedule:` trigger and a `versions` job whose step is a no-op while `tools/versioncheck.py` is absent
  - Notes: 011 owns this file, and 009's design states plainly that D9 **adds a step to it** rather than creating a second workflow. Building it with the second gate in mind is cheaper than retrofitting. Two requirements from 009: the trigger must include a **daily schedule**, because the event that invalidates a pinned version is a release in *another repository* and a PR-only trigger leaves a stale pin undetected until someone happens to touch the docs; and exit code **`2` (authority unreachable) is not a pass**. Guard the step so the build stays green before 009 lands — `if [ -f tools/versioncheck.py ]` — and leave a comment naming 009 D9 so the guard is removed rather than inherited.

---

## Phase 6 — The Demonstrator Splits (design §12 steps 8–9)

Goal: two single-mode-per-page splits establishing the pattern Spec 010 executes. In both,
**the Reference core keeps the original file name** so the most-linked URL does not move.

These are the only tasks producing prose, and design §12 says plainly that this is where
schedule pressure should give — Spec 010 needs the conventions (Phase 1) and the worklist
(Task 7.1), not the splits.

- [ ] **Task 6.1:** Reduce `RabbitMQConfiguration.md` to its Reference core (~270 lines)
  - Input: design §6a target structure, current `contents/RabbitMQConfiguration.md` (566 lines)
  - Output: General · RabbitMQ.Client v7 Support · Breaking Changes · connection/publication/subscription parameter tables · Putting It Together · configuration tables lifted from the quorum-queue, persistence, retry and heartbeat sections — **every knob, one place**
  - Notes: **Moved verbatim, not rewritten** (design §10). Any C# block that moves keeps its current form, `using` directives or not — rewriting while relocating produces a diff nobody can review, and would then oblige backfill on every block in the file under rule 6. Banner: `Reference`.

- [ ] **Task 6.2:** Write `contents/RabbitMQDurability.md` (~150 lines, Explanation)
  - Input: design §6a, the quorum-queue and persistence sections of the original
  - Output: New page — What are Quorum Queues? · Classic vs Quorum · When to Use Quorum Queues · What is Message Persistence? · When to Use Persistent Messages · Performance Considerations
  - Notes: Quorum-queue best-practice material folds into *When to Use* — **"guidance" is not a page type** (design §11). Banner: `Explanation`. Headings must be subject-qualified and globally unique from the moment the file is created; run `pagelint.py` on it before committing.

- [ ] **Task 6.3:** Write `contents/RabbitMQMigrateToQuorumQueues.md` (~90 lines, How-to)
  - Input: design §6a
  - Output: New page — Before you start (prerequisites, validation) · Migration from Classic to Quorum · Enabling Persistent Messages · Ack and Nack behaviour during migration
  - Notes: Banner: `How-to`, with a prerequisite link to `RabbitMQConfiguration.md`.

- [ ] **Task 6.4:** Write `contents/RabbitMQConnectionStability.md` (~140 lines, How-to)
  - Input: design §6a
  - Output: New page — Improvements in V10 · Configuring connection retry · Configuring heartbeats · Handling blocked connections in production · Monitoring and example logging configuration
  - Notes: Connection-stability best practices become **the step that implements them**, not a `## Best Practices` heading. Banner: `How-to`.

- [ ] **Task 6.5:** Confirm all four `Best Practices` sections have landed somewhere
  - Input: the original page's four `Best Practices` sections, Tasks 6.1–6.4
  - Output: A note in this file recording where each went
  - Notes: **No information loss** is a programme-level rule, and this is the split most at risk of quietly dropping guidance. None survives as a standalone `## Best Practices` heading, which also removes 4 of the 26 instances of that collision.

- [ ] **Task 6.6:** Add `SUMMARY.md` entries for the three new RabbitMQ pages
  - Input: design §9 (before/after shown), `SUMMARY.md:66`
  - Output: Three entries under *Guaranteed At Least Once*, **beside** `RabbitMQ Configuration`
  - Notes: Beside the page they were split from — **not** in new sections. Spec 010 restructures the whole table of contents immediately afterwards and will site them properly; inventing placement now is work done twice. Run `linkcheck.py` — its ORPHAN check is what enforces "never create orphaned files", and it only fires on a whole-repo run.

- [ ] **Task 6.7:** Reduce `BrighterBasicConfiguration.md` to its How-to core (~200 lines)
  - Input: design §6b, current page (1,068 lines — the largest and worst-mixed in the repo)
  - Output: Using .NET Core Dependency Injection · Configuring the Command Processor (**the one path that works**) · Putting It All Together · Running the Dispatcher · A Complete Dispatcher Example — linking out to both reference pages for every option
  - Notes: Normalise the `## **Bold Heading**` style while splitting (`## **Configuring The Command Processor**`) — it is the convention nowhere else, and `slug()` strips emphasis so anchors are unaffected either way. It costs nothing at the point the text is already moving. Banner: `How-to`.

- [ ] **Task 6.8:** Write `contents/CommandProcessorConfigurationReference.md` (~630 lines)
  - Input: design §6b — original lines 16–331, 332–349, 350–648
  - Output: New page — Command Processor Service Collection Extensions · Validating Your Configuration · Brighter Builder Fluent Interface
  - Notes: Verbatim move. Banner: `Reference`, prerequisite `BrighterBasicConfiguration.md`. At ~630 lines it exceeds `CLAUDE.md`'s 500-line guidance — correctly, because it is single-mode reference material and splitting a parameter list by size is exactly the error requirements § Mode mixing warns against.

- [ ] **Task 6.9:** Write `contents/DispatcherConfigurationReference.md` (~210 lines)
  - Input: design §6b — original lines 708–859, 860–918
  - Output: New page — Dispatcher Service Collection Extensions · Dispatcher Brighter Builder Fluent Interface
  - Notes: Verbatim move. Banner: `Reference`. "Dispatcher", not "ServiceActivator", in the prose — rule 5 will enforce it.

- [ ] **Task 6.10:** Fold the V10 configuration section into `V10MigrationGuide.md`
  - Input: original lines 1041–1068 (V10 Configuration Changes + Quick Migration Guide)
  - Output: `contents/V10MigrationGuide.md` **absorbing** that content; it does not survive on the split page
  - Notes: "No information loss, but no duplication either." That content belongs with the other migration material, and a reader looking for it will look there. Check for heading collisions with what the guide already has.

- [ ] **Task 6.11:** Add `SUMMARY.md` entries for the two new configuration pages
  - Input: design §9 (before/after shown), `SUMMARY.md:9`
  - Output: Two entries under *Brighter Configuration*, beside `Basic Configuration`
  - Notes: `SUMMARY.md:9` and `:19` both read `Basic Configuration` — the Brighter one is `:9`, the Darker one `:19`. Run `linkcheck.py` for the orphan check.

- [ ] **Task 6.12:** Determine and, if needed, add `.gitbook.yaml` redirects
  - Input: design §6 "Redirects", `PROMPT.md` § Platform facts, the zero-width-space finding at the top of this file
  - Output: Either a `redirects:` block, or a recorded finding that no page-level redirect is required — plus the two U+200B characters removed either way
  - Notes: **Establish first whether any page URL actually moves.** Both splits keep the Reference core under its original file name, so its published URL is unchanged; what breaks is *anchor-level* inbound links to sections that moved, and GitBook redirects operate on pages, not fragments. If that holds, the honest output is a recorded finding rather than a no-op stanza — the design's example (`guaranteed-at-least-once/rabbitmqconfiguration: contents/RabbitMQConfiguration.md`) points a URL at the page it already resolves to. The **new** pages' URLs derive from their `SUMMARY.md` placement, which Spec 010 changes immediately afterwards, so their redirects belong to 010. Adding the `redirects:` block is 010's deliverable; do it here only if 010 has not landed **and** a URL genuinely moves. Whatever the outcome: strip the U+200B before `structure:` and after `SUMMARY.md` and the trailing double-spaces, then **verify mechanically** — malformed indentation disables GitBook redirects silently rather than erroring, and an invisible character in a YAML key is that failure mode at its worst.

---

## Phase 7 — Handoff, P1, and Acceptance (design §12 step 10)

- [ ] **Task 7.1:** Write `worklist.md` for Spec 010 (D8)
  - Input: design §8 (column table), requirements § Mode mixing (the 31 pages scoring ≥3 modes, the 14 scoring four), classification difficulty captured in Task 3.2
  - Output: `spec/011-authoring_conventions/worklist.md` — Page (path + lines) · Mode score with modes named · Verdict (`split` / `keep` / `keep — outside Diátaxis`) · Proposed shape · Rationale
  - Notes: **Must stand alone without this spec in context** — Spec 010 executes against it. Seeded from the 31 pages scoring ≥3 modes, minus the two split here. **The score is a triage signal, not a verdict:** `Glossary.md` (589 lines, single mode) and `KafkaConfiguration.md` (606 lines, one mode) are the standing reminders that size and score both mislead — record them as `keep` with the reason, so 010 does not re-open the question. Pages that resisted classification in Task 3.2 go here as split candidates; pages legitimately outside Diátaxis (`FAQ.md`, `Glossary.md`, `V10MigrationGuide.md`) must **not** be recorded as split candidates.

- [ ] **Task 7.2:** P1 — add language tags to the 185 untagged fences, then flip rule 4 to error
  - Input: `pagelint.py` rule 4 warnings
  - Output: Language tags across `contents/`; rule 4 promoted to repo-wide error; `CLAUDE.md` *Enforcement* updated to match
  - Notes: Mechanical enough to sit alongside the sweeps. Pick the tag from the block's content — `csharp`, `yaml`, `json`, `bash`, `text` for output dumps. Promote the rule in the **same** commit, or the tags decay like everything else unenforced.

- [ ] **Task 7.3:** P1 — add `--fix` to `pagelint.py`
  - Input: design §1 comparison table, requirements § P1
  - Output: `--fix` covering the mechanical rules: banner **version segment**, language tags
  - Notes: Scope it narrowly — `--fix` must **never** decide a page type; it cannot know one. Its reason for existing is that the V11 bump is 105 edits, and it should be one command plus a diff review rather than a page-by-page trudge. Do **not** fold `apply_banners.py`'s TSV logic in: that would leave a durable tool carrying one-off migration logic, keyed to a file that by then records a decision made years earlier.

- [ ] **Task 7.4:** Acceptance pass and programme handoff
  - Input: requirements § Acceptance Criteria (AC1–AC8)
  - Output: A checked-off AC list appended to this file; `PROMPT.md` updated with 011's completion state and the measured baseline; `PROMPT.md` open question 3 closed
  - Notes: Walk all eight: `pagelint.py` exits 0 with the warning count recorded (AC1) · `linkcheck.py` exits 0 including orphans (AC2) · every banner human-reviewed (AC3) · no cross-page `##` collisions outside the allowlist (AC4) · `CLAUDE.md` ↔ linter parity in **both** directions and the *File Organization Pattern* no longer prescribing rejected headings (AC5) · CI green with both tools, and green on the untouched tree *before* the sweeps (AC6) · splits navigable with redirects settled (AC7) · worklist executable without re-deriving the analysis (AC8). There are eight: requirements numbered two of them "6" until this list's review renumbered them to 1–8, and design's traceability row was updated to match. Then hand to Spec 010 — it needs the conventions and the worklist, both of which now exist.

---

## Dependencies

```
1.1 ─────────────────────────────► 5.1 ──► 5.2
      (green baseline first)         │
1.2 ┬─► 1.4 ──► Phase 2             └──► 5.3  (009 D9 wiring)
1.3 ┘

2.1 ─► 2.2, 2.3, 2.5           (rules, parallelisable)
   └─► 2.4 ─► 2.6              (both branches of check_code_blocks — same file)
        └────────► 2.7 ─► 2.8 ─► 2.9 ─► ⛔ session boundary

3.1 ─► 3.2 ─► 3.3 ─► 3.4 ─► 3.5
                       └──► Phase 4 (banner must exist before headings move)

4.1 ─► 4.2 ┐
4.3 ───────┼─► 4.4 ─► 4.6 ─► Phase 5
4.5 ───────┘  (own commit)
       └──► unblocks Spec 009 D12's Glossary.md#dispatcher link

6.1 ─► 6.2, 6.3, 6.4 ─► 6.5 ─► 6.6 ┐
6.7 ─► 6.8, 6.9 ─► 6.10 ─► 6.11 ────┼─► 6.12
                                     └─► 7.1 ─► 7.4
7.2, 7.3  independent of Phase 6
```

**Cross-spec:**

- **Spec 009 is blocked on nothing here**, and stays parallel. But two links matter:
  Task 4.5 merges `Glossary.md`'s duplicate `Dispatcher` entry, which 009's D12 waits for
  before linking `Glossary.md#dispatcher`; and Task 5.3 prepares `docs.yml` for 009's
  `versioncheck.py`.
- **Spec 010 is blocked on Task 1.3** (the conventions) **and Task 7.1** (the worklist).
  Not on Phase 6 — the splits can slip without holding 010 up.
- **Spec 012's `optioncheck`** is the third tool in the `linkcheck`/`pagelint` family and
  should follow the same shape: stdlib only, single file, optional paths, non-zero exit.

## Parallelisable

- Tasks 2.2, 2.3, 2.5 — independent rule functions against one skeleton. **2.4 and 2.6
  are not independent of each other**: both implement branches of `check_code_blocks`,
  and 2.7 then changes its dispatch. Do them as one sitting, in the order 2.4 → 2.6 → 2.7
- Tasks 6.2–6.4 — three new pages from disjoint source sections
- Tasks 6.8–6.9 — likewise
- Tasks 7.2–7.3 — P1, independent of the splits

## Traceability

| Deliverable | Tasks |
|---|---|
| D1 `CLAUDE.md` conventions + pattern amendment | 1.2, 1.3, 1.4, 7.2 |
| D2 banner on every page | 3.1–3.5 |
| D3 heading de-duplication + 8 anchors | 4.1–4.4, 4.6 |
| Content defects surfaced by rule 3b (design §8) — the 3 merges | 4.5 |
| D4 `tools/pagelint.py` | 2.1–2.9, 7.3 |
| D5 `.github/workflows/docs.yml` | 1.1, 5.1, 5.2, 5.3 |
| D6 RabbitMQ split | 6.1–6.6 |
| D7 `BrighterBasicConfiguration` split | 6.7–6.11 |
| D8 worklist | 7.1 |
| D9 `llms.txt` format | 1.3 |
| Redirects | 6.12 |
| AC1–AC8 | 7.4 |

---

## Measured baseline (2026-08-04) — Task 2.9

`python3 tools/pagelint.py`, run repo-wide against the untouched tree at commit
`deee51d`. **412 errors, 840 warnings across 105 pages**, exit 1.

| Rule | Predicted | Measured | Verdict |
|---|---|---|---|
| 1 `BANNER MISSING` | 105 | **105** | exact — one per page, no page reported twice |
| 2 `BANNER MALFORMED` | 0 | **0** | exact — and 0 `NO H1`, confirming the sweep is unambiguous |
| 3a `HEADING NOT UNIQUE` | 256 instances / 48 texts | **262 / 50** | **prediction wrong, rule right** — see below |
| 3b `HEADING REPEATED` | 34 across 12 pages | **34 across the same 12** | exact, and verified page by page, not just on the total |
| 4 `LANGUAGE TAG` (warning) | 185 | **36** across 14 pages | **corpus figure wrong** — see below |
| 5 `SERVICEACTIVATOR` | not predicted | **11** across 3 pages | all explained; needs a small fix before Task 5.1 |
| 6 `USING DIRECTIVES` (warning) | 663 of 796 C# blocks | **804 of 940** | **corpus figure wrong** — same cause as rule 4 |

**No rule needed revising.** Phase 2's stop-rule asks whether a materially different
number means a rule is wrong; on inspection every discrepancy is in the prediction, and
two of the three come from a single unrecorded fact about the corpus. Rules 1, 2 and 3b
reproduced their predictions exactly, and rule 3a reproduces its *raw* prediction
exactly, which is what makes the remaining gap diagnosable rather than mysterious.

### The finding: 143 C# fences are written ```` ``` csharp ````, with a space

CommonMark trims the info string, so ```` ``` csharp ```` is a C#-tagged block and
renders as one. A script matching `^```csharp` does not see it. That single fact
explains both code-block discrepancies:

| | Counted by the earlier script | Actually |
|---|---|---|
| Fenced blocks with **no** language | 185 | **36** — 149 of the 185 have a space-separated info string |
| C#-tagged blocks | 796 | **940** — 787 exact, 143 space-separated, 8 indented, 2 other |
| C# blocks with a `using` directive | 133 | **136** |

`36 + 149 = 185` reproduces the old figure exactly, which is the confirmation that this
is the cause rather than a plausible story about it.

Two consequences, both in the same direction:

- **Task 7.2 shrinks from 185 fences to 36.** Adding a language tag to 36 blocks across
  14 pages is small enough to do alongside the sweeps rather than as a phase of its own,
  which makes flipping rule 4 to a repo-wide error cheap.
- **The `using`-directive debt is larger than recorded** — 804 blocks across 89 pages,
  not 663 across 67. Compliance is **14%**, not 16%. This changes nothing about the
  mechanism (Q4's two strictness levels retire the debt as pages are edited) but the
  baseline AC1 requires us to record is 804, and that is the number to shrink.

The 149 space-separated fences are **not** a defect and are not on any worklist. They
render correctly; they are merely inconsistent with the 787 written without the space.
Normalising them is cosmetic, and doing it as a sweep of its own would touch 40-odd
pages to change nothing a reader can see.

### Rule 3a: 262 instances across 50 texts, not 256 across 48

The raw comparison measures **50 texts / 256 instances** — exactly what requirements
§ Measurements predicts. The normalised comparison, which is what `pagelint.py` does,
measures **50 texts / 262 instances**.

The *Applied at review* finding at the top of this file predicted 48 texts / 256
instances. It was right that normalising merges the emphasised outbox headings into
their plain twins, and right that this subtracts two texts. What it missed is that
`slug()` also **lowercases**, so normalisation pulls in case-variant pairs that were not
collisions when compared raw. All four, accounting for the +6 exactly:

| Merge | Instances added |
|---|---|
| `## Common pitfalls` folds into `## Common Pitfalls` | +1 |
| `## NuGet packages` folds into the `NuGet Packages` group | +1 |
| `## How It Works` + `## How it works` — neither collided raw, together they do | +2 |
| `## **Configuring The Dispatcher**` + `## Configuring the Dispatcher` — likewise | +2 |

The last two are why the text count comes back to 50: the two emphasis merges subtract
two texts, and these two new collisions add two back. That the count is 50 under both
comparisons is a coincidence, and a misleading one — the underlying sets differ.

**Task 4.1 therefore proposes qualifiers for 50 texts / 262 instances**, and Task 4.2
applies them. The named sub-case for the four outbox pages is unchanged. Design §3 and
requirements § Measurements have been corrected.

### Rule 5's 11 findings are all real, and none is the error the rule describes

| Page | Lines | What fired |
|---|---|---|
| `BrighterBasicConfiguration.md` | 706, 714, 715, 735, 923, 925, 927 | `**Paramore.Brighter.ServiceActivator.Extensions.Hosting**`, `**ServiceActivatorHostedService**`, `**ServiceActivatorOptions**` |
| `BrighterControlAPI.md` | 7 | `**Paramore.Brighter.ServiceActivator.Control.Api**` |
| `HowServiceActivatorWorks.md` | 454, 458, 459 | the page discussing the name itself |

The design anticipated identifiers being exempt *inside inline code spans*. These eight
are identifiers written in **bold** instead of backticks, which `CLAUDE.md` already
tells authors not to do — "use `code` for code elements, file names, class names,
methods". So the rule is correct and the pages are wrong, in a small and mechanical way.

**Remediation, required before Task 5.1 puts `pagelint.py` in CI:** add
`<!-- pagelint: allow-serviceactivator -->` to `HowServiceActivatorWorks.md`, and change
the eight bolded identifiers to backticks. Both pages are already being edited in
Phase 6 — `BrighterBasicConfiguration.md` is split at Task 6.7, and its lines 706–735
and 923–927 are the Dispatcher material that moves to
`DispatcherConfigurationReference.md` at Task 6.9, whose note already says "Dispatcher,
not ServiceActivator, in the prose — rule 5 will enforce it". Do it there; the two
`BrighterControlAPI.md` and `HowServiceActivatorWorks.md` fixes are one line each.

### What the linter's own behaviour confirmed

- **`--changed` strictness is genuinely per block.** Tested against a synthetic page:
  an edit inside the second block makes only that block an error; an edit on a line
  *between* two blocks makes neither strict. This is the property Task 2.7 exists for
  and it was worth proving before CI relies on it.
- **`changed_ranges` parses multi-hunk diffs** and raises rather than returning empty
  when the ref does not resolve, so the vacuous pass Task 5.2 warns about cannot happen
  silently — the run exits 2 with the shallow-clone explanation.
- **Exit codes:** 1 with errors, 2 on a missing `--changed` ref and on a path outside
  `contents/`, 0 when clean. Same contract as `linkcheck.py`.
- **Fence-aware parsing matters.** `pagelint.py` tracks fences before reading headings,
  so a `# Install packages` comment in a bash block is not counted as an H1. The corpus
  also contains two 4-backtick fences and 18 indented ones, both handled.

### Where this leaves the phase

Phases 1 and 2 are complete. `linkcheck.py` is green in CI on the untouched tree
(run 30881245792, `107 files checked`), `pagelint.py` is written, run and reconciled,
and it is **deliberately not in `docs.yml`** — Task 5.1 adds it once the sweeps make it
pass. The next session starts at Task 3.1.
