# Spec 010: Information Architecture Restructure

**Created:** 2026-08-03
**Status:** Requirements Phase

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
    guaranteed-at-least-once/rabbitmqconfiguration: transports/RabbitMQConfiguration.md
  ```

  Constraints: **no leading slashes** on either side; all paths are relative to `root`;
  malformed indentation silently disables redirects rather than erroring, so the
  block needs mechanical verification rather than eyeballing.
- Generate `llms.txt` from the new `SUMMARY.md` (rationale in Spec 011) — the
  generator belongs here because it is derived from the tree
- Extend `tools/linkcheck.py` to validate that redirect targets resolve

## Out of Scope

- Editing page *bodies*. Splitting mixed-mode pages is
  [Spec 011](../011-authoring_conventions/README.md). The `BasicConcepts` → `Glossary`
  merge that this line used to except is withdrawn, so there is now no body-editing
  carve-out at all.

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

- Current `SUMMARY.md` — 162 lines, 110 pages under `contents/`
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

- **URL breakage** is the dominant risk. Mitigation: redirects block plus an explicit
  old-path → new-path table covering all 110 pages.
- **Review size.** A full TOC rewrite is a large diff. Mitigation: get the proposed
  `SUMMARY.md` approved at design phase, before any file moves happen.

## Status Checklist

- [ ] Requirements gathered
- [ ] Requirements reviewed and approved
- [ ] Documentation outline created
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Agree the target section list and ordering
2. Produce the old-path → new-path mapping for all 110 pages
3. Confirm GitBook redirect syntax and any limits
4. Create requirements document

## Notes

- **No information loss** governs this spec.
- Run `python3 tools/linkcheck.py` across the whole repo after every move.
</content>
