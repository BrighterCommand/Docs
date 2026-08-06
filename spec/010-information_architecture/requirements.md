# Spec 010: Information Architecture — Requirements

**Created:** 2026-08-06 · **Status:** Draft, awaiting review
**Responds to:** [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)
**Depends on:** Spec 011 (complete) — `CLAUDE.md` § Page Conventions, `tools/pagelint.py`,
and `spec/011-authoring_conventions/worklist.md`

---

## 1. Topic overview

`SUMMARY.md` is organised around **Brighter's architecture**, not around what a reader is
trying to do. #67 diagnosed it precisely: *"I don't think the issue lies with the
information in the documentation. The main problem seems to be its organization."*

This spec re-files the corpus around reader intent and executes the page splits Spec 011
measured but deliberately left behind. The governing constraint is **no information
loss**: this is re-filing, re-titling and separating modes, never a cull.

---

## 2. The URL model — measured, and it governs everything here

**This is the central finding of the requirements phase, and it was verified rather than
assumed.** Every claim in this section reproduces against the live site.

A published URL is:

```text
<site-root>/<slug(SUMMARY H2 section)>/[slug(ancestor page filename)/]*<slug(filename)>
```

where `slug(s)` lowercases, replaces each run of non-alphanumeric characters with a single
hyphen, and trims leading/trailing hyphens. `Requests, Commands and Events.md` publishes as
`requests-commands-and-events`; the section *Command, Processors and Dispatchers* publishes
as `command-processors-and-dispatchers`.

**Verification.** A 40-line predictor reading only `SUMMARY.md` reproduced **110 of 110**
published URLs exactly — no misses, no extras — against
`sitemap-pages.xml` (111 `<loc>` entries: 110 pages plus the index). The slug function
alone reproduced **110 of 110** leaf slugs.

Four consequences, each of which changes the plan the README assumed:

1. **The section heading is part of every URL.** Renaming *Guaranteed At Least Once* to
   *Transports* moves all **11** URLs beneath it even though no file moves and no file is
   edited. Re-titling is not cosmetic here; it is a URL change.
2. **`contents/` is flat and stays flat.** A page's URL is a function of where it sits in
   `SUMMARY.md`, not of its path on disk. **010 therefore moves no files.** It rewrites
   one file, plus `.gitbook.yaml`.
3. **Internal links do not break.** All **763** internal links in the corpus are of the
   form `/contents/Page.md`, and all **110** distinct targets stay exactly where they are.
   Only *external, inbound* links break. This substantially reduces the risk the README
   named as dominant — but see the anchor caveat in §7.
4. **SUMMARY nesting deepens the URL.** Nine pages publish at three segments
   (`outbox-and-inbox/distributedlock/mssqldistributedlock`). Re-parenting a page under a
   different page moves its URL just as a section change does.

**Consequence for the redirect map: it is mechanically derivable.** Run the predictor over
the old `SUMMARY.md` and the new one, diff the two path sets, and emit one redirect per
page whose path changed. This must be a tool, not a hand-maintained table — see D3.

### 2.1 Redirect syntax, confirmed against GitBook's own documentation

```yaml
redirects:
  previous/page: new-folder/page.md
```

- The **key** is the old *published* path; the **value** is the path to the Markdown file
  **in the repository**, relative to `root`.
- **No leading slashes** on either side.
- **Malformed indentation disables redirects silently** rather than erroring.

**The README's example is wrong and must not be copied.** It reads
`guaranteed-at-least-once/rabbitmqconfiguration: transports/RabbitMQConfiguration.md`,
whose target presumes a `transports/` directory that does not exist and that this spec does
not create. Because `contents/` stays flat, every redirect target in this repository is
`contents/<FileName>.md`:

```yaml
redirects:
  guaranteed-at-least-once/rabbitmqconfiguration: contents/RabbitMQConfiguration.md
```

**One open risk.** GitBook's documentation states that *"if you're setting up a redirect
for an old page to a new one, you will need to remove the old page in order for the
redirect to work."* Our case is different in kind — the file is never removed, it simply
publishes at a new URL because its section changed — so the old path ceases to exist
without the page ceasing to exist. That should be exactly what redirects are for, but it
is not the case the sentence describes. **Q1 below makes verifying this a gate, not an
assumption.**

---

## 3. Current state — measured 2026-08-06

Re-derive with the commands given; do not cite these from memory.

| Fact | Value |
|---|---|
| Pages under `contents/` | **110** |
| Links in `SUMMARY.md` | **110** (no orphans, no duplicates) |
| Top-level sections (H2) | **19** |
| Published URLs | **111** (110 + index) |
| Internal links corpus-wide | **763**, across 110 distinct targets |
| Files with a non-trivial name | **1** — `Requests, Commands and Events.md` |
| `linkcheck.py` | clean, 112 files |
| `pagelint.py` | **0 errors**, 802 warnings (the deliberate `using` debt) |
| `llms.txt` | **absent** |
| `.gitbook.yaml` | `root`, `structure.readme`, `structure.summary` — **no `redirects:` block** |

Section sizes today — the shape of the problem in one table:

| Pages | Section |
|---:|---|
| 27 | Outbox and Inbox |
| 14 | Brighter Request Handlers and Middleware Pipelines |
| 12 | Using an External Bus |
| 11 | Guaranteed At Least Once |
| 9 | Brighter Configuration |
| 8 | Scheduler |
| 4 | Health Checks and Observability · Event Driven Architectures · Darker Query Handlers |
| 3 | Overview · Database Provisioning · Under the Hood |
| 2 | V10 Migration |
| 1 | Darker Configuration · CQRS Patterns · Command, Processors and Dispatchers · Task Queues · **Reference** · FAQ |

**Six of nineteen sections hold exactly one page**, and the one named *Reference* is among
them. Meanwhile *Outbox and Inbox* holds 27. Both failures are navigational: a section of
one is not a category, and a section of 27 is not a menu.

Named symptoms, all still true:

- **Transports are filed under a delivery guarantee.** RabbitMQ, Kafka, SNS/SQS and Azure
  Service Bus sit under *Guaranteed At Least Once* — accurate about semantics, useless as
  a signpost for "how do I use Brighter with Kafka".
- **Explanatory material is scattered across five sections**, mostly at the bottom.
- **Darker is split across two sections** (*Darker Configuration*, 1 page; *Darker Query
  Handlers and Middleware Pipelines*, 4) that sit apart in the tree.
- **Migration content sits mid-tree**, between provisioning and conceptual sections.
- **No page is a Tutorial** — 50 Reference, 33 How-to, 27 Explanation, 0 Tutorial. That is
  Spec 009's gap, but 010 must leave a *Get Started* section for it to land in.

### 3.1 A structural constraint the target shape must respect

GitBook's `SUMMARY.md` offers exactly two grouping mechanisms: an **H2 heading** creates a
group, and an **indented list item** creates a child *of the page above it*. There is no
sub-group. **A middle navigation layer therefore requires a real page to hang it from** —
which is why the seven distributed locks nest under `DistributedLock.md` and publish three
segments deep.

This matters because the README's seven-bucket target would put roughly 40 pages into
*How To* once Spec 013's guides land, recreating the *Outbox and Inbox* problem under a
new name. Where a family needs a middle layer, an overview page must exist to parent it.
Four already do: `BrighterOutboxSupport.md`, `BrighterInboxSupport.md`,
`BrighterSchedulerSupport.md`, `DistributedLock.md`.

---

## 4. Target state

A reader arriving with an intent — *get started*, *do a task*, *look something up*,
*understand why* — finds the right section from the top-level list, and a reader who knows
their technology finds it grouped by technology rather than by delivery semantics.

The README's candidate shape, carried forward as a **starting point for design, not an
approved tree** (see Q2):

```text
Get Started        Spec 009 tutorials, Show me the code, Why Brighter?, Basic Concepts
How To             task-phrased recipes (Spec 013) + how-tos extracted by the splits
Transports         by technology: RabbitMQ, Kafka, SNS/SQS, ASB, Postgres, ...
Outbox and Inbox   by store, with provisioning and distributed lock beneath
Schedulers         by implementation
Darker             the query side, currently split across two sections
Reference          config references (Spec 012), Glossary, FAQ, V10 Migration
Explanation        Under the Hood, EDA patterns, Task Queues, CQRS, Reactor/Proactor
```

**Diátaxis is an authoring discipline, not top-level navigation** — a decision already
taken and publicly stated on #67, and not reopened here. A literal four-bucket split would
shred each technology into four pages across 7 transports, 8 outboxes, 6 inboxes, 7
schedulers and 7 distributed locks. Those families stay grouped **by technology** and get
mode discipline *within* pages, which is what Spec 011 built and enforced.

---

## 5. Target audience

| Audience | What the restructure must do for them |
|---|---|
| **Newcomer** | Reach a working example without reading an architecture tour. *Get Started* must be the first section and must not be a code showcase |
| **Task-driven developer** | Find "how do I use Brighter with Kafka" by technology, and "how do I put a big payload behind a claim check" by task |
| **Experienced user** | Look up a parameter without reading an essay first — the payoff of 011's mode discipline |
| **Retrieval systems / LLMs** | `llms.txt` indexing every page with its type and a one-line summary |

---

## 6. Source material

- `SUMMARY.md` (166 lines, 110 links, 19 sections) — the object of the work
- **`spec/011-authoring_conventions/worklist.md`** — 42 rows, the split list 010 executes.
  Written to stand alone; it needs no other file from 011
- `CLAUDE.md` § Page Conventions — banner grammar, heading qualification, the navigation
  allowlist
- `tools/linkcheck.py`, `tools/pagelint.py` — both in CI, both fail the build
- `spec/010-information_architecture/README.md` — rationale, **stale in two places**
  (§8, Q2)
- [Diátaxis](https://diataxis.fr/) for the four-mode vocabulary
- GitBook `.gitbook.yaml` documentation — redirect syntax, confirmed 2026-08-06
- Live `sitemap-pages.xml` — the ground truth the URL model was verified against

**No Brighter ADR is relevant.** All 100 ADRs in `../Brighter/docs/adr/` concern the
framework's architecture; the one with a structural-sounding name
(`0049-testing-assembly-structure.md`) is about test assemblies. Documentation IA is a
`Docs`-repository concern with no upstream decision record.

---

## 7. Scope

### P0 — must have

- **P0-1 Rewrite `SUMMARY.md`** to the approved tree: fewer, intent-named sections; no
  section of one page; no section large enough to be unnavigable without a middle layer.
- **P0-2 Redirect map, generated not hand-written.** One entry for every page whose
  published path changes, derived by diffing the predictor's output across the two
  `SUMMARY.md` versions.
- **P0-3 Mechanical verification of `.gitbook.yaml`.** Malformed indentation disables
  redirects silently, and this file has a history of exactly that failure — it carried two
  U+200B zero-width spaces until 2026-08-05, one of them *in a key*, so GitBook had never
  read the `structure:` block at all and nothing looked broken. Verify by parsing and by
  byte inspection, never by eye.
- **P0-4 Execute the 26 split rows** in `worklist.md` §6, honouring §§3–5: the five rules
  the demonstrator splits established, the three cross-cutting decisions, and the 16 rows
  that say `keep`.
- **P0-5 Every new page carries a banner, a `SUMMARY.md` entry and qualified headings.**
  Non-negotiable: `pagelint.py` and `linkcheck.py`'s orphan check both fail the build.
- **P0-6 No information loss, proven mechanically.** Every substantive line of a split
  original tested for verbatim presence across the resulting pages — the check both
  demonstrator splits used. "I read the diff" is not this check.

### P1 — should have

- **P1-1 `llms.txt` generator** — built from `SUMMARY.md`, reading each page's banner for
  its type. Format is fixed in `CLAUDE.md` § llms.txt. Belongs here because it is derived
  from the tree.
- **P1-2 Extend `linkcheck.py` to validate redirect targets** — every value in
  `redirects:` must resolve to a file that exists, and every key must be a path that no
  longer publishes.
- **P1-3 Per-term links from `BasicConcepts.md` into `Glossary.md`.** 24 terms and 100
  terms respectively, and **zero links between them today**. This replaces the withdrawn
  merge.
- **P1-4 Fix the three content defects** in `worklist.md` §7 while the pages are open:
  the `## How It Work` typo at `SweeperCircuitBreaking.md:16`; the 76-line overlap between
  `HowServiceActivatorWorks.md:147` and `DispatcherConfigurationReference.md`; and the
  `QueriesAndQueryObjects.md:746` ↔ `QueryPatterns.md` duplication flag, which must be
  verified before either side is moved.

### P2 — nice to have

- **P2-1 Normalise the 17 files with no trailing newline** — deliberately deferred from
  011 so the banner diff contained nothing but banners.
- **P2-2 Rename `Requests, Commands and Events.md`.** The only file whose name needs URL
  encoding in `SUMMARY.md`. A rename changes its URL and needs a redirect like any other
  move, so it costs nothing extra *if done in the same pass* — and is not worth a separate
  one.
- **P2-3 Echo the changed-range count from `docs.yml`** so a `--changed` run proves its own
  non-vacuity. Carried over from 011; small, and this spec's PRs are where it would pay.

---

## 8. Out of scope

- **Writing new how-to guides** — Spec 013. The splits *extract* how-tos that already
  exist inside larger pages; they do not author new ones. The four missing how-tos in
  `worklist.md` §8 belong to 013, and the PostgreSQL-for-both-transport-and-outbox guide
  publicly committed on #67 is 013's first batch.
- **Writing tutorials** — Spec 009. 010 creates the *Get Started* section they land in and
  must not consume the material 009 needs: `CQRSWithBrighterAndDarker.md`'s 226-line
  worked example, and `ShowMeTheCode.md`.
- **Generated configuration tables** — Spec 012.
- **Merging `BasicConcepts.md` into `Glossary.md`** — withdrawn by the maintainer
  2026-08-04. Per-term links replace it (P1-3). Do not reopen.
- **Rewriting Darker page *content*.** Re-filing and splitting are safe; rewriting
  behaviour is not. `../Darker` HEAD is ahead of the deployed 4.1.1, and the site publishes
  the deployed version.
- **Moving files on disk.** §2 establishes this is unnecessary; doing it anyway would break
  763 internal links to buy nothing.

> **The README's *Out of Scope* section is stale and this supersedes it.** It reads
> *"Editing page bodies. Splitting mixed-mode pages is Spec 011."* Splitting was moved from
> 011 into 010 on **2026-08-03**, precisely because splitting creates pages needing names,
> `SUMMARY.md` entries and redirects — all files 010 is already changing. `worklist.md`
> states its executor as Spec 010 in its own header. **Page bodies are in scope for 010,
> for splits and for the §7 defects.** The README should be amended at design.

---

## 9. Deliverables

| ID | Deliverable | Priority |
|---|---|---|
| **D1** | Rewritten `SUMMARY.md` | P0 |
| **D2** | `redirects:` block in `.gitbook.yaml` | P0 |
| **D3** | `tools/urlmap.py` — predicts published paths from a `SUMMARY.md`; diffs two revisions; emits the redirect block | P0 |
| **D4** | 26 executed splits from `worklist.md` §6, with their new pages, banners and SUMMARY entries | P0 |
| **D5** | A no-information-loss check, run per split | P0 |
| **D6** | `tools/llmstxt.py` + generated `llms.txt` | P1 |
| **D7** | `linkcheck.py` extended to validate the redirect block | P1 |
| **D8** | Per-term `BasicConcepts.md` → `Glossary.md` links | P1 |
| **D9** | The three `worklist.md` §7 content fixes | P1 |

**D3 is the keystone.** It makes D2 derivable rather than transcribed, and it is the only
honest way to produce a redirect table for a tree this size. It should refuse to guess:
exit non-zero if a page appears in neither revision's tree, so a page dropped from
`SUMMARY.md` fails loudly instead of silently losing its URL. Its predictor is already
written and validated 110/110 — it needs packaging, not invention.

---

## 10. `SUMMARY.md` changes

The whole file is the deliverable, so "where new files go" is the design phase's output
rather than a list here. Two placement rules bind it:

- **Every page created by a split is placed beside the page it came from**, not in a
  Diátaxis bucket. A how-to extracted from `Telemetry.md` sits with observability, because
  that is where a reader looking for it will be.
- **Every family with a middle layer needs a parent page** (§3.1). Where one does not
  exist, either the family stays flat or an overview page is a deliverable of the split.

---

## 11. Constraints

- `CLAUDE.md` is the authority; `pagelint.py` and `linkcheck.py` enforce the parts a tool
  can check, and both gate CI.
- **Banner on every new page**, one blank line below the H1, separator ` · ` (U+00B7). A
  split is exactly where the *Prerequisites* segment earns its place.
- **Every `##` heading qualified by its subject** and unique across pages. The five
  navigation headings are exempt and must stay uniform.
- **"Dispatcher", not "ServiceActivator"**, in prose.
- **Anchor-level links break and redirects cannot fix them** — GitBook redirects operate on
  pages, not fragments. The `BrighterBasicConfiguration.md` split had to repoint **28
  anchor links across 20 pages** by hand. Grep for an anchor before moving the heading that
  owns it, and run `linkcheck.py` after every split.
- **A new page is 100% added lines**, so `pagelint.py --changed` makes every one of its C#
  blocks strict. A block moved verbatim marks its omission `// ...`, which downgrades the
  finding to a warning and never silences it. Do not backfill namespaces you have not
  checked.
- **One PR per coherent unit of work**, merged before the next branch starts.

---

## 12. Acceptance criteria

| # | Criterion | How it is checked |
|---|---|---|
| **AC1** | `SUMMARY.md` links all 110 pages plus every page the splits create; zero orphans | `linkcheck.py` |
| **AC2** | `linkcheck.py` and `pagelint.py` both report **0 errors** | CI |
| **AC3** | Every page whose published path changed has a redirect; no redirect points at a missing file | D3 + D7 |
| **AC4** | The redirect block is **verified mechanically** — parsed as YAML and byte-inspected for invisible characters | explicit step, not eyeball |
| **AC5** | A sample of redirects **returns the new page on the live site** after publish | manual, post-merge — Q1 |
| **AC6** | No section holds one page; no section is unnavigable without a middle layer | review against the final tree |
| **AC7** | No information loss across all 26 splits, proven per split | D5, mechanically |
| **AC8** | Every `worklist.md` `keep` verdict is honoured — 16 rows across 15 pages | review |
| **AC9** | `llms.txt` covers every page with type and one-line summary, generated not hand-written | D6 |

---

## 13. Open questions for the maintainer

1. **Do GitBook redirects fire for a page that still exists but publishes elsewhere?**
   GitBook's documentation says an old page must be *removed* for its redirect to work. Our
   pages are never removed — they change section, so the old *path* stops publishing while
   the *page* lives on. This should be the ordinary case for redirects, but it is not the
   case the documentation describes, and the failure mode is silent. **Recommendation: land
   one section rename first, as a small PR, and verify the redirect on the live site before
   committing to a 110-page rewrite.** This is the cheapest possible way to de-risk the
   spec's dominant risk, and it mirrors how PR #73 proved the CI gate in 011.
2. **The target section list.** §4 carries the README's seven buckets forward as a starting
   point, not an approval. Two things need deciding: whether *Darker* becomes a top-level
   section (it is 5 pages across 2 sections today), and how *How To* avoids becoming the
   new *Outbox and Inbox* once 013's guides arrive (§3.1).
3. **Section renames without page moves still change URLs.** Some renames are pure gain
   (*Guaranteed At Least Once* → *Transports*). Others may not be worth the redirect —
   is there any section whose name should be left alone specifically to preserve its URLs?
4. **Should the splits and the restructure be separate PRs, and in which order?**
   **Recommendation: restructure first, split second.** Restructuring is one file plus
   redirects and is reviewable in isolation; splitting afterwards adds pages to a settled
   tree, so no page's URL moves twice and no split page is published at a URL that exists
   only for one release.
5. **`ReplayOnSeen.md`'s core mode.** Its banner says Reference; `worklist.md` §6d observes
   that on the split outline the core is arguably the Explanation. Changing it changes the
   banner and the page's section.
6. **P2-2, renaming `Requests, Commands and Events.md`.** Cheap inside this pass, not worth
   a pass of its own. Take it or drop it now.

---

## 14. A correction to `worklist.md`, recorded

**§1 states "Twelve of the 42 rows say `keep`". It is sixteen rows, across fifteen distinct
pages** — `TickerQScheduler.md` is listed twice, in §6a and again in §6e. The 42-row total
is right; the split/keep breakdown is 26/16, not 30/12.

Re-derive rather than trusting either figure:

```bash
python3 - <<'PY'
import re
from collections import Counter
rows=[]; sec=None
for line in open('spec/011-authoring_conventions/worklist.md'):
    m=re.match(r'^### (6[a-e])\.',line)
    if m: sec=m.group(1); continue
    if sec and line.startswith('|'):
        c=[x.strip() for x in line.strip().strip('|').split('|')]
        if len(c)>=5 and c[0].startswith('`'): rows.append((c[0],c[4]))
print(len(rows),"rows",len({p for p,_ in rows}),"distinct pages")
print(Counter('split' if 'split' in v else 'keep' for _,v in rows))
PY
```

This is the **seventh** figure in this programme to be wrong, and it fits the established
pattern exactly: the rules were derived from the corpus and have held, while the numbers
were estimated from it and have not. It changes no verdict — every row still says what it
says — but a plan sized on "30 splits" would over-provision, and one sized on "12 keeps"
would under-honour the rows whose whole purpose is to stop 010 reopening settled questions.

---

## 15. Next step

`/spec:review` for approval, then `/spec:design`.

**Before finalising, check [#67](https://github.com/BrighterCommand/Docs/issues/67) for a
reply.** Diátaxis-as-authoring-discipline was explicitly flagged there for pushback.
Checked 2026-08-06: **no reply yet** — the last three comments are all the maintainer's, so
nothing external constrains the design today.
