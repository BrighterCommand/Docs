# Spec 010: Information Architecture — Requirements

**Created:** 2026-08-06 · **Reviewed:** 2026-08-06 · **Status:** Approved
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
   `SUMMARY.md`, not of its path on disk. **010 therefore moves no files.** *The
   restructure* rewrites one file, plus `.gitbook.yaml` — the splits then add pages to
   `contents/` and entries to `SUMMARY.md`, and add no directories.
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

### 2.2 The "remove the old page" sentence, read in full — it resolves in our favour

An earlier draft of this section recorded the removal requirement as an open risk. **It is
not one**, and the source it cited is what settles it. From GitBook's
[Git Sync troubleshooting](https://gitbook.com/docs/getting-started/git-sync/troubleshooting)
page, quoted in full and fetched as raw bytes rather than through a summariser:

> It's also important to consider that **as long as a page exists for a path**, GitBook
> won't be looking for a possible redirect. So if you're setting up a redirect for an old
> page to a new one, you will need to remove the old page in order for the redirect to work.

**The rule is the first sentence, and it is keyed on the *path*, not on the page.** The
earlier draft quoted only the second. After a section rename no page exists for
`guaranteed-at-least-once/rabbitmqconfiguration`, so GitBook does look for the redirect.
"Remove the old page" is that same rule stated for the case where the file is still sitting
on the old path — a special case of the rule, not an additional condition.

Corroborated independently by the resolution order on
[Site redirects](https://gitbook.com/docs/docs-site/site-redirects):

> 1. Site content is resolved to its canonical URL by following any of the automatically
>    created redirects.
> 2. **If the URL cannot be resolved**, the URL is checked against section-level redirects,
>    defined in your repository's `.gitbook.yaml` file.
> 3. Finally, the URL is checked against site-level redirects.

The fall-through to `.gitbook.yaml` is gated on the URL failing to resolve, which is
exactly our condition.

### 2.3 The risk that replaces it — automatic redirects can mask the test

Same page, and this is the one that changes the plan:

> Whenever pages are moved or renamed, their canonical URL changes with them. In order to
> keep your content accessible, GitBook automatically creates a **HTTP 307** redirect from
> the old URL to the new one.

Automatic redirects are consulted **first**, before `.gitbook.yaml`. It is not established
whether GitBook treats a Git-synced `SUMMARY.md` reorganisation as a "move" for this
purpose. If it does, **a section-rename experiment that succeeds proves nothing about the
redirect block** — the automatic 307 would have carried the request either way, while the
`.gitbook.yaml` block sat silently broken, which is precisely the failure P0-3 exists to
catch. A green result would be indistinguishable from the thing it is meant to rule out.

**D0 therefore discriminates the two mechanisms rather than asking "did the old URL
work?"** — see §7 P0-0. This is 011's *a check that passes has not necessarily checked
anything* applied to the one experiment this spec depends on.

> **Measured 2026-08-06/07: they do.** The old path returned a **307** twenty-five seconds
> after the rename merged, with no `redirects:` block in the repo. **D2 and D3 are
> belt-and-braces, not load-bearing** — and the single-step version of this experiment
> would have reported success while proving nothing. Full result in §16.

### 2.4 GitBook's own `.gitbook.yaml` example is contaminated — this is live, not history

`.gitbook.yaml` carried two `U+200B` zero-width spaces until 2026-08-05, one of them inside
the `structure:` key, so GitBook had never read that block at all. **The provenance is now
established: they came from GitBook's own documentation.** Byte-inspecting the
[Content configuration](https://gitbook.com/docs/getting-started/git-sync/content-configuration)
page on 2026-08-06 finds `U+200B` at exactly two positions in its example —

```text
root: ./

​structure:
  readme: README.md
  summary: SUMMARY.md​

redirects:
  previous/page: new-folder/page.md
```

— the same two characters this repository carried, and **the `redirects:` snippet sits
inside that same contaminated code block.** Writing D2 by copying the documented example
reintroduces the bug, into a file whose failure mode is silence.

This is why P0-3 is mechanical verification and not a careful read.

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

> **`SUMMARY.md:154` is one space away from silently moving three URLs.** It reads
> ` ## Under the Hood` with a leading space that no other heading in the file has.
> CommonMark tolerates up to three leading spaces on an ATX heading; at four it stops being
> a heading altogether, and GitBook would fold `HowBrighterWorks.md`,
> `HowServiceActivatorWorks.md` and `ReactorAndProactor.md` into *Task Queues* without
> erroring. This is not hypothetical: re-deriving the section count at review with a strict
> `^## ` returned **18 sections and five singletons**, both wrong. `urlmap.py` scores 110/110
> because its `^\s*##\s+` is deliberately tolerant. **Normalise the line during P0-1** — and
> note that a tolerant parser is the right choice for `urlmap.py`, since it must model what
> GitBook does, not what the file ought to say.

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
- GitBook, three pages, all fetched as **raw bytes** and quoted verbatim in §§2.1–2.4 —
  [Content configuration](https://gitbook.com/docs/getting-started/git-sync/content-configuration)
  (`.gitbook.yaml` syntax; and the U+200B source, §2.4),
  [Git Sync troubleshooting](https://gitbook.com/docs/getting-started/git-sync/troubleshooting)
  (the "remove the old page" rule in full, §2.2), and
  [Site redirects](https://gitbook.com/docs/docs-site/site-redirects)
  (resolution order and automatic 307s, §§2.2–2.3)
- Live `sitemap-pages.xml` — the ground truth the URL model was verified against

**No Brighter ADR is relevant.** All 100 ADRs in `../Brighter/docs/adr/` concern the
framework's architecture; the one with a structural-sounding name
(`0049-testing-assembly-structure.md`) is about test assemblies. Documentation IA is a
`Docs`-repository concern with no upstream decision record.

---

## 7. Scope

### P0 — must have

- **P0-0 Establish which redirect mechanism actually carries a moved URL, before the
  rewrite.** A gate on everything else, and it must **discriminate** rather than merely
  pass (§2.3). Land one section rename as a small PR, in two observable steps:
  1. Rename the section and publish **with no entry in `redirects:`**. Request the old URL
     and record the **HTTP status and `Location`**. A `307` here means GitBook created an
     *automatic* redirect for a Git-synced structural change — in which case D2/D3 are
     belt-and-braces and a later "the redirect worked" proves nothing.
  2. Add the `.gitbook.yaml` entry and publish again. Record status and `Location` a second
     time. Only a difference between the two steps demonstrates the block was read.

  Also settle, since it is free here: **whether section-level redirects are gated by site
  plan.** Site-*level* redirects are documented as "available on Premium and Ultimate site
  plans"; section-level `.gitbook.yaml` redirects carry no documented gate, but the
  published site's plan has never been confirmed and AC3 rests on it.

  **A `404` at step 1 followed by a `200`/`301` at step 2 is the outcome that makes D2 and
  D3 load-bearing.** Any other pairing changes the spec, so it is settled before design
  rather than discovered at merge. Mirrors PR #73, which proved 011's CI gate by forcing it
  red on purpose.
- **P0-1 Rewrite `SUMMARY.md`** to the approved tree: fewer, intent-named sections; no
  section of one page; no section large enough to be unnavigable without a middle layer.
  Normalise the leading space on ` ## Under the Hood` (§3) in the same pass.
- **P0-2 Redirect map, generated not hand-written.** One entry for every page whose
  published path changes, derived by diffing the predictor's output across the two
  `SUMMARY.md` versions.
- **P0-3 Mechanical verification of `.gitbook.yaml`.** Malformed indentation disables
  redirects silently, and this file has a history of exactly that failure — it carried two
  U+200B zero-width spaces until 2026-08-05, one of them *in a key*, so GitBook had never
  read the `structure:` block at all and nothing looked broken. Verify by parsing and by
  byte inspection, never by eye.

  **The contamination source is live, and it is the example this deliverable will be
  written from** (§2.4): GitBook's own `.gitbook.yaml` documentation ships those same two
  U+200B characters, and its `redirects:` snippet sits inside the same code block. So the
  check is not guarding against a past accident — it is guarding against the most likely way
  D2 gets written. **Type the block, never paste it**, and assert on bytes: no character
  outside printable ASCII anywhere in the file.
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
| **D0** | The redirect-mechanism experiment (P0-0) — one section rename, published twice, statuses recorded; plan gating confirmed | P0, **first** |
| **D1** | Rewritten `SUMMARY.md` | P0 |
| **D2** | `redirects:` block in `.gitbook.yaml` | P0 |
| **D3** | `tools/urlmap.py` — predicts published paths from a `SUMMARY.md`; diffs two revisions; emits the redirect block | P0 |
| **D4** | 26 executed splits from `worklist.md` §6, with their new pages, banners and SUMMARY entries | P0 |
| **D5** | A no-information-loss check, run per split | P0 |
| **D6** | `tools/llmstxt.py` + generated `llms.txt` | P1 |
| **D7** | `linkcheck.py` extended to validate the redirect block | P1 |
| **D8** | Per-term `BasicConcepts.md` → `Glossary.md` links | P1 |
| **D9** | The three `worklist.md` §7 content fixes | P1 |

**D0 comes before everything, including design.** It is cheap, it is the only thing that
can invalidate D2 and D3, and §2.3 is why it must be run as a two-step comparison rather
than a single "did the URL work" check.

**D3 is the keystone** — subject to D0. It makes D2 derivable rather than transcribed, and
it is the only honest way to produce a redirect table for a tree this size. It should refuse to guess:
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
| **AC5a** | **Before the rewrite:** the redirect mechanism is established by the two-step experiment, and the result **discriminates** the `.gitbook.yaml` block from an automatic 307 | D0, statuses recorded |
| **AC5b** | **After merge:** a sample of redirects returns the new page on the live site | manual, post-merge |
| **AC6** | No section holds one page; no section is unnavigable without a middle layer | review against the final tree — **threshold still open, Q8** |
| **AC7** | **Per split:** every split that lands proves no information loss for that split, mechanically. **Partial completion of the 26 is a valid end state** — the spec is accepted on the splits it landed, not blocked on the ones it did not | D5, per split |
| **AC8** | Every `worklist.md` `keep` verdict is honoured — 16 rows across 15 pages | review |
| **AC9** | `llms.txt` covers every page with type and one-line summary, generated not hand-written | D6 |

---

## 13. Open questions for the maintainer

**Q7 changes the shape of the spec; the rest are decisions within it. Q1 was answered at
review** and is recorded here for the trail rather than as a live question.

1. ~~**Do GitBook redirects fire for a page that still exists but publishes elsewhere?**~~
   **Answered 2026-08-06 at review: yes, on the documentation — see §2.2.** The sentence an
   earlier draft treated as a blocker had been quoted from its second half only. Its
   governing clause is *"as long as a page exists **for a path**, GitBook won't be looking
   for a possible redirect"* — keyed on the path, not the page — and the published
   resolution order falls through to `.gitbook.yaml` precisely when a URL fails to resolve.
   Both are our case.

   **What survives is a different risk, and it inverts the experiment: §2.3.** Automatic
   307s are consulted *before* `.gitbook.yaml`, so a rename that "works" may prove nothing
   about the redirect block. **P0-0/D0 now runs the rename twice — without the redirect
   entry, then with it — and compares.** Still a gate, still before design, but it is now a
   discrimination rather than a smoke test.
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
7. **Is 010 too large as scoped, and should it be split into two specs?** This is the
   question I would most expect a reviewer to raise, so it is stated rather than left
   implicit. As written, 010 carries a full `SUMMARY.md` rewrite, a redirect mechanism,
   **26 page splits**, three tools (D3, D6, D7) and four content fixes. For scale: the two
   demonstrator splits in 011 took a full session each, and 010 has twenty-six.

   The two halves are genuinely separable, and §2 is why — the restructure touches
   `SUMMARY.md` and `.gitbook.yaml` and **no page bodies at all**, while the splits touch
   only page bodies and add SUMMARY entries. They share no file except `SUMMARY.md`.

   **Recommendation: keep them one spec but sequence them as Q4 says** — restructure
   first, splits second — and treat the split phase as interruptible. The argument for one
   spec is the one that moved splitting out of 011 in the first place: a split page needs
   a name, a SUMMARY entry and possibly a redirect, and doing that against a tree that is
   about to change means touching it twice. The argument against is simply size, and
   sequencing addresses size without reintroducing the double-touch.

   **If the reviewer disagrees, the clean cut is after the restructure lands** — that is a
   coherent, shippable unit on its own, and the splits become 014.

   > **Ruled 2026-08-06: one spec, sequenced, and AC7 is now per-split.** The review found
   > that "interruptible" had no force while AC7 required no information loss *across all 26
   > splits* — the spec could not be accepted until every one landed. AC7 now accepts the
   > splits that land and states partial completion as a valid end state, which is what makes
   > the sequencing answer the size objection rather than just describe it.

8. **What is AC6's threshold?** "No section is unnavigable without a middle layer" is
   unfalsifiable as written. 27 pages in *Outbox and Inbox* is agreed too many; §3.1 says
   roughly 40 in *How To* is the failure to avoid; nothing says where the line falls. A
   stated number — say, a section over ~12 pages needs a parent page or a split — makes AC6
   checkable instead of arguable. **Deliberately left open**: it is the concrete half of
   Q2, and the number should be chosen against the actual candidate tree rather than in the
   abstract.
9. **Where does `llms.txt`'s one-line summary come from?** `CLAUDE.md` fixes the format as
   `- [Title](path): Type — one sentence.` The type is derivable from the banner; **the
   sentence is not derivable from anything, and no page carries one today.** AC9 says
   "generated not hand-written" for 110+ pages, so either D6 extracts something mechanical
   (the first sentence after the banner) and that quality is accepted, or ~136 summaries get
   authored, which is a P1 the size of a small spec. Related: the documented format uses
   repository paths, but a retrieval client wants the **published URL** — which `urlmap.py`
   can now emit. Both belong to design.
10. **Should `urlmap.py` gate CI?** D3 lands at `tools/urlmap.py`, beside the two tools that
    already fail the build. P1-2 is currently a one-time validation, but a redirect block
    that is complete at merge and incomplete three PRs later is the same silent failure in
    slow motion. Making it a standing check is cheap; whether it is in scope for 010 or a
    follow-on is a call for design.

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

## 15. What the review changed (2026-08-06)

Approved with amendments. Every load-bearing figure was re-derived and **all of them held**
— worklist 42/26/16 across 41 distinct pages, `SUMMARY.md` at 110 links and 110 distinct
targets, 110 pages, 19 sections with every row of §3's size table exact, `urlmap.py
--verify` returning *predicted 110, published 110, 110 agree* against the live site, and
#67 still unanswered. **For the first time in this programme a review found no wrong
number**, which is what §14 predicts once figures are measured rather than estimated.

What changed:

- **§2.2 — Q1 answered, not deferred.** The blocking quote had been read from its second
  half. Its governing clause is path-keyed and resolves in our favour, corroborated by the
  published resolution order.
- **§2.3 — the replacement risk.** Automatic 307s are consulted before `.gitbook.yaml`, so
  the original experiment could have passed vacuously. **P0-0/D0 now discriminates.**
- **§2.4 — the U+200B provenance.** GitBook's own example is the source, it is live today,
  and the `redirects:` snippet sits inside the contaminated block. P0-3 sharpened from a
  general caution to a targeted one: type the block, never paste it.
- **§3 — `SUMMARY.md:154`.** A leading space on ` ## Under the Hood`, one space from
  silently relocating three URLs. It produced a wrong re-derivation during this very review.
- **D0 added, AC5 split into AC5a/AC5b** so the dominant risk is verified *before* the
  rewrite rather than after the merge.
- **AC7 is per-split**, with partial completion an explicit valid end state — without which
  Q7's "interruptible" was contradicted by the acceptance criteria.
- **Q8–Q10 added** rather than decided: AC6's threshold, `llms.txt`'s summary source, and
  whether `urlmap.py` gates CI. All three are design-phase calls.

## 16. D0 as executed — 2026-08-06/07, PRs #77, #78, #79

**Both mechanisms work. §2.3's second branch is the real one, and the single-step version
of this experiment would have proved nothing.**

| Question | Answer | Evidence |
|---|---|---|
| Does GitBook auto-redirect a Git-synced `SUMMARY.md` rename? | **Yes** | **307** to the new path **25s** after #77 merged, with no `redirects:` block anywhere in the repo |
| Are `.gitbook.yaml` redirects read on this site? | **Yes** | A probe key that had never existed began redirecting after #78; a control key *absent* from the block kept 404ing |
| Does the site plan permit section-level redirects? | **Yes** | Implied by the above — no plan gate was hit |

### The method mattered

Shipping the rename *with* its redirect — the obvious way — would have gone green on
GitBook's automatic 307 while proving nothing about `.gitbook.yaml`. And because step 1
answered "yes", the **planned step 2 became untestable**: the resolution order means the
moved path now resolves, so its redirect entry would never be consulted. Step 2 was
redesigned mid-experiment around a **probe key for a path that had never existed**, which
no automatic redirect can mask. The control — a key absent from the block, still 404ing —
is what makes it a measurement.

### Read the fingerprint, not the status code

**Every cached response reports `200`**, including genuine 404s and genuine redirects.
Status alone is worthless here. The discriminators:

| Path class | `location:` header | Body |
|---|---|---|
| key **in** the redirects block | **1** | ~192.3 KB |
| key **not** in the block | 0 | ~189.5 KB (404 shell) |
| renamed-away path (automatic 307) | **1** | ~192.4 KB |
| real page | 0 | **584 KB** |

Only an uncached request shows the true `307`. `x-opennext-cache: HIT` plus a `location:`
header on a `200` is a cached redirect being replayed — and **no genuine page response
carries `location:`**, which is the cleanest single tell.

### Three findings that change how D2 and D3 are built

- **The redirect value is a repository path, and GitBook resolves it to wherever that page
  *currently* publishes.** The probe pointed at
  `contents/CommandsCommandDispatcherandProcessor.md` and landed on the **post**-rename
  URL. **So a redirect entry does not go stale when its page moves again** — which matters
  for a spec that will move some pages more than once, and means the block can be written
  once rather than re-derived after every move.
- **D2/D3 drop from load-bearing to belt-and-braces.** Automatic redirects appear to cover
  010's whole case. **But nothing establishes that they *persist*** — they may be tied to
  revision history, and one session cannot test that. Ship the block anyway; it is cheap
  and it is the safety net.
- **Redirect responses are cached with `stale-while-revalidate=2592000` — 30 days.** The
  probe key kept redirecting for well over an hour after #79 removed it. **A wrong redirect
  will outlive its fix at the edge**, so verify the block *before* merging, not after.

### Two operational facts for the design phase

- **GitBook sync latency is 25–45 seconds** from merge to published. Measured, not assumed.
- **PyYAML is not available in this environment.** `ruby -ryaml` is, and was used for the
  parse half of P0-3. D7's parser choice needs deciding rather than assumed — see Q10.

The section rename itself was the independently-correct fix it was chosen to be: the comma
was misplaced, and *Command, Processors and Dispatchers* is now *Commands, Processors and
Dispatchers*. One URL of 110 moved.

---

## 17. Next step

`/spec:design`. **D0 is done and its gate is passed** — both redirect mechanisms are
measured and working, so the design can proceed on the URL model in §2 without further
platform investigation. Carry §16's three findings into D2 and D3.

**Before finalising, check [#67](https://github.com/BrighterCommand/Docs/issues/67) for a
reply.** Diátaxis-as-authoring-discipline was explicitly flagged there for pushback.
Checked 2026-08-06: **no reply yet** — the last three comments are all the maintainer's, so
nothing external constrains the design today.
