# Spec 010: Information Architecture Restructure

**Created:** 2026-08-03
**Status:** **COMPLETE 2026-08-22 — 56 of 56 tasks, all eleven phases, AC1 through AC9 walked.**
Requirements approved 2026-08-06, D0 executed 2026-08-07, design approved 2026-08-08, tasks
approved 2026-08-08. The acceptance pass and its evidence are in
[`tasks.md`](tasks.md) § *The acceptance pass as executed*.

> **The list grew from 52 tasks to 56 at Phase 11**, when four findings session 22 had
> recorded in prose became numbered tasks. Nothing was re-planned; a paragraph is not a box,
> and only a box gets walked at acceptance.

> **This README is the rationale, not the plan.** Where it disagrees with
> [`requirements.md`](requirements.md) or [`design.md`](design.md), they win. Three
> passages were stale and are corrected in place below: the *Proposed Target Structure*,
> the redirect example, and *Out of Scope*.

## Topic Overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) diagnoses our core
problem accurately: *"I don't think the issue lies with the information in the
documentation. The main problem seems to be its organization."*

`SUMMARY.md` is organised around **Brighter's architecture**, not around what a
reader is trying to do. Concrete symptoms in the current TOC:

- **Transports are filed under a delivery guarantee.** RabbitMQ, Kafka, AWS SNS/SQS
  and Azure Service Bus live under a section called *"Guaranteed At Least Once"*.
  That is a correct statement about delivery semantics and a useless signpost for
  someone looking for "how do I use Brighter with Kafka".
- **Explanatory material is scattered across five sections**, mostly at the bottom:
  *Why Brighter?* (top), *Command, Processors and Dispatchers*, *Event Driven
  Architectures*, *Task Queues*, *Under the Hood*.
- **"Reference" contains one page** — the Glossary.
- ~~**Two glossaries.**~~ **Withdrawn 2026-08-04 on the maintainer's ruling.**
  `BasicConcepts.md` (24 terms) does occupy one of three prime Overview slots and its
  terms do also appear in `Glossary.md` (100 terms) — but the separation is deliberate,
  not duplication. See *The `BasicConcepts` merge is withdrawn* below.
- **Migration content sits mid-tree**, between provisioning and conceptual sections.

This spec restructures `SUMMARY.md` into a reader-intent hierarchy. The governing
constraint is **lose no information** — this is re-filing and re-titling, not a cull.
Where content moves it moves whole; where two pages overlap they merge with both sets
of facts preserved.

## Proposed Target Structure

Diátaxis supplies the top-level shape, with one pragmatic exception. A literal
four-bucket split would shred each technology into four pages (tutorial / how-to /
reference / explanation) across 7 transports, 8 outboxes, 6 inboxes, 7 schedulers and
7 distributed locks. Readers navigate that material by **technology** ("I'm on
Kafka"), and want configuration and gotchas in one place. Those families therefore
stay grouped, and get *within-page* mode discipline from
[Spec 011](../011-authoring_conventions/README.md) instead.

```
Get Started        Spec 009 tutorials, then Show me the code, Why Brighter?
How To             task-phrased recipes (Spec 013)
Transports         by technology: RabbitMQ, Kafka, SNS/SQS, ASB, Postgres, ...
Outbox and Inbox   by store
Schedulers         by implementation
Reference          config tables (Spec 012), Glossary, FAQ, V10 Migration
Explanation        Under the Hood, EDA patterns, Task Queues, CQRS, Reactor/Proactor
```

> **Superseded 2026-08-07 by [`design.md`](design.md) §3.** These seven buckets were a
> starting point, never an approval — requirements §4 says so explicitly. Two things
> defeated them. A *How To* section collects roughly 40 pages once Spec 013's guides land,
> which is the *Outbox and Inbox* problem under a new name; and the buckets have nowhere to
> put the Brighter core — handlers, pipelines, configuration and the external bus — so it
> would all have landed in *How To* as well. **The approved shape is twelve sections and no
> *How To* section at all**: a how-to lives beside its subject. All 110 pages are placed in
> design §3.1.

## Scope

- Rewrite `SUMMARY.md` to the agreed structure
- ~~Merge `BasicConcepts.md` into `Glossary.md`~~ — **withdrawn 2026-08-04**, see
  below. Consider instead adding per-term links from `BasicConcepts.md` into the fuller
  `Glossary.md` entry
- Retitle sections to reader intent — e.g. *"Guaranteed At Least Once"* becomes
  *"Transports"*, with the delivery-guarantee material moved into Explanation rather
  than discarded
- **Redirects plan.** `.gitbook.yaml` currently declares only `root`,
  `structure.readme` and `structure.summary`; there is no redirects block. Every
  published GitBook URL derives from the current tree, so restructuring without
  redirects breaks all inbound links and search results. Populating `redirects:` is a
  required deliverable.

  Syntax confirmed against GitBook documentation (2026-08-03):

  ```yaml
  redirects:
    guaranteed-at-least-once/rabbitmqconfiguration: contents/RabbitMQConfiguration.md
  ```

  Constraints: **no leading slashes** on either side; all paths are relative to `root`;
  malformed indentation silently disables redirects rather than erroring, so the
  block needs mechanical verification rather than eyeballing.

  > **Corrected 2026-08-07.** This example used to target
  > `transports/RabbitMQConfiguration.md` — **a directory that does not exist and that this
  > spec does not create.** The value is a path to the Markdown file *in the repository*,
  > and `contents/` is flat and stays flat, so every target in this repo is
  > `contents/<FileName>.md`. Requirements §2 is why no file moves on disk at all.
  >
  > Two facts measured on the live site since (requirements §16): GitBook resolves the
  > value to wherever that page **currently** publishes, so an entry does not go stale when
  > its page moves again; and **type this block, never paste it** — GitBook's own
  > documented example carries two U+200B zero-width spaces to this day.
- Generate `llms.txt` from the new `SUMMARY.md` (rationale in Spec 011) — the
  generator belongs here because it is derived from the tree
- Extend `tools/linkcheck.py` to validate that redirect targets resolve

## Out of Scope

- **Writing new how-to guides** — Spec 013. The splits *extract* how-tos that already
  exist inside larger pages; they author none.
- **Writing tutorials** — Spec 009. This spec creates the *Get Started* section they land
  in, and must not consume the material 009 needs.
- **Generated configuration tables** — Spec 012.
- **Merging `BasicConcepts.md` into `Glossary.md`** — withdrawn 2026-08-04, see below.
- **Rewriting Darker page *content*.** Re-filing and splitting are safe; `../Darker` HEAD
  is ahead of the deployed 4.1.1 and the site publishes the deployed version.
- **Moving files on disk.** Requirements §2 establishes it is unnecessary.

> **Corrected 2026-08-07.** This section used to read *"Editing page bodies. Splitting
> mixed-mode pages is Spec 011."* **Splitting moved from 011 into 010 on 2026-08-03**,
> precisely because a split page needs a name, a `SUMMARY.md` entry and possibly a
> redirect — all files 010 is already changing. `spec/011-authoring_conventions/worklist.md`
> names Spec 010 as its executor in its own header. **Page bodies are in scope**, for the
> 26 splits and for the three content defects in that file's §7.

### The `BasicConcepts` merge is withdrawn (2026-08-04)

Ruled by the maintainer during Spec 011's Task 3.2 page-type review. The two pages do
two different jobs, and the smaller is not redundant for overlapping with the larger:

> We want to ensure that key terms are easily understood, without needing to review the
> whole glossary. Possibly greater use of links would help, but this was separated with
> that goal in mind, which should be considered.

`BasicConcepts.md` is a curated 24-term orientation set for a newcomer;
`Glossary.md` is the complete 100-term reference. The audit measured the overlap and
inferred duplication from it without asking why the smaller set existed.

Two completed specs corroborate that this is a live convention rather than an accident:
spec 002's Task 3.1 deliberately added three terms to `BasicConcepts.md` (DLQ, Nack,
Poison Message), and spec 006 lists it as a link target on first use of *Command* and
*Request*.

**What replaces the merge:** per-term links from `BasicConcepts.md` into the
corresponding `Glossary.md` anchor, so a reader wanting depth has one click rather than
a second page to find. That is additive and cheap — glossary links already carry
`#anchor`s and `linkcheck.py` validates them. Full note in
[`spec/011-authoring_conventions/classification-notes.md`](../011-authoring_conventions/classification-notes.md).

## Source Material

- Current `SUMMARY.md` — 166 lines, 110 links, 19 sections; 110 pages under `contents/`
- [`SUMMARY.target.md`](SUMMARY.target.md) — the tree design §3 approves, 145 lines,
  110 links, 12 sections. The file PR 2 installs, kept here so its figures reproduce
- [Diátaxis](https://diataxis.fr/) for the four-mode vocabulary
- GitBook `.gitbook.yaml` redirects documentation
- `tools/linkcheck.py` — reports MISSING FILE, MISSING ANCHOR, WRONG CASE and ORPHAN;
  the restructure must leave it exiting zero

## Dependencies

- **Blocks** Specs 012 and 013 — both place new pages into the new tree
- **Coordinates with** Spec 009 — tutorials need the new *Get Started* section
- **Follows** Spec 011, so moved pages acquire their page banner in the same pass
  rather than being touched twice

## Execution Order

Spec numbers are identifiers, not a sequence. Programme order is
**011 → 010 → 012 → 013**, with Spec 009 in parallel throughout. This spec runs second:
Spec 011 normalises page bodies first, then this spec moves them.

## Risks

- ~~**URL breakage** is the dominant risk.~~ **Downgraded 2026-08-06/07, measured twice.**
  All **763** internal links survive, because every target stays in a flat `contents/`;
  only inbound *external* links move, and **74 of 110** do. Both redirect mechanisms were
  proven on the live site (requirements §16). What is *not* covered is **anchor-level**
  links, which GitBook redirects cannot address — design §8 measures those at 34 inbound,
  ≈19 needing repointing.
- **Review size.** A full TOC rewrite is a large diff. Mitigation: the target tree is
  written out as `SUMMARY.target.md` and approved at design, before anything is installed;
  and the restructure ships as one PR touching no page bodies at all.

## Status Checklist

- [x] Requirements gathered — 2026-08-06
- [x] Requirements reviewed and approved — 2026-08-06 (`.requirements-approved`)
- [x] Documentation outline created — `design.md`, 2026-08-07
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Review [`design.md`](design.md) — the target tree in §3, the twelve sections, and the
   five deviations from `worklist.md`'s shape column collected in §7.7
2. `/spec:tasks`, sequenced as design §10 lays out: restructure first, splits second

Everything the first three steps of the old list asked for is done and measured:
the section list and ordering are design §3, the old-path → new-path mapping is
`urlmap.py --redirects` (74 entries, generated not transcribed), and the redirect syntax
was confirmed against the live site by publishing it — requirements §16.

## Notes

- **No information loss** governs this spec.
- Run `python3 tools/linkcheck.py` across the whole repo after every move.
</content>
