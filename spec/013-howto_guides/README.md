# Spec 013: Task-Oriented How-To Guides

**Created:** 2026-08-03
**Status:** Requirements drafted 2026-09-04 — see [`requirements.md`](requirements.md), awaiting review

> **This README was written 2026-08-03 against a tree Specs 010 and 012 have since changed, and
> `requirements.md` §2 supersedes it on every content claim.** Measured 2026-09-04: the two named
> content gaps below are **closed** (`Logging.md` is 151 lines; `MessageTransforms.md` exists), and
> **three of the four missing how-tos are closed** — only `ClaimCheck.md`'s survives. The *Scope*
> section's first bullet is **backwards**: Spec 010 design §6.2 is headed *"There is no How To
> section, and 013 does not get one either"*. The rationale below stands; the page references do
> not.

## Topic Overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) asks for guides that
*"focus on solving specific problems users face"*, and gives two examples:

- *How to configure Brighter with PostgreSQL for both transport and outbox*
- *How to configure Brighter with RabbitMQ*

The first is the interesting one. It is a completely reasonable thing to want, it is
fully supported, and **there is no page for it** — the reader must find
`PostgreSQLMessageBroker.md` and `PostgresOutbox.md`, work out that they compose, and
reconcile two independent configuration examples themselves. The knowledge exists; the
recipe does not.

In Diátaxis terms a how-to is goal-directed: it starts from a problem the reader
already has, assumes competence, and is allowed to omit explanation. It differs from a
tutorial (which teaches, and must not branch) and from reference (which is complete,
and is not sequenced).

This spec is deliberately the **cheapest** of the five: most guides compose existing
content and link out rather than restating it. The value is in making the composition
discoverable.

## Proposed Guides

Cross-cutting recipes, phrased as reader goals. Initial candidates:

**Configuration compositions**
- Use PostgreSQL for both transport and Outbox *(issue author's example)*
- Use RabbitMQ with a database-backed Outbox
- Run Brighter with no external broker (in-memory, for tests)

**Operational problems**
- Handle poison messages and configure a DLQ
- Make a handler idempotent with the Inbox
- Retry and circuit-break a failing external call
- Replay messages after a downstream outage *(links Spec 008's Replay On Seen)*

**Development and testing**
- Test a handler pipeline without a broker
- Provision Outbox/Inbox schemas in CI
- Migrate an existing V9 application to V10 *(pointer into the migration guide, not a
  duplicate of it)*

**Observability**
- Trace a message end to end with OpenTelemetry
- Add health checks for transport and Outbox

The final list comes out of requirements. Two sources should shape it more than our
intuition: recurring questions in Brighter issues and discussions, and the
`contents/FAQ.md` — an FAQ entry that keeps getting asked is a how-to guide that does
not exist yet.

## Scope

- A *How To* section in `SUMMARY.md` (structure agreed in Spec 010)
- One page per guide: problem statement, prerequisites, numbered steps, verification
  step, links out to reference and explanation
- An index page listing guides by goal

### Content gaps handed over from Spec 011's Task 3.2 (2026-08-04)

Found while classifying all 105 pages by Diátaxis mode, and confirmed by the
maintainer. Full reasoning in
[`spec/011-authoring_conventions/classification-notes.md`](../011-authoring_conventions/classification-notes.md).

**`contents/Logging.md` is a three-line stub** — an H1 and the word `TODO`. It is
listed in `SUMMARY.md`, so it is published, navigable and invisible to
`linkcheck.py`'s orphan check *precisely because* it is linked. It carries a
`Reference` banner from 011's sweep, which is a banner on nothing. Writing it is this
spec's, and it is the clearest instance of the pattern below: the docs point at a
subject and then say nothing about it.

**Four Explanation pages have no how-to route through them.** Each explains a
mechanism well and leaves the reader without a task-shaped path:

| Explanation that exists | How-to that does not |
|---|---|
| `ClaimCheck.md` | How to put a large payload behind a claim check |
| `DynamicMessageDeserialization.md` | How to route several message types down one channel |
| `QueriesAndQueryObjects.md` | How to write a query and its handler |
| `MessageMappers.md` | How to use the default mapper — but check `DefaultMessageMappers.md` first; it may already be that page and simply not be reachable as one |

**One of these is a correctness gap, not a filing one.** The maintainer's ruling on
`MessageMappers.md` is that its `## Transformers` section should become its own
Explanation page, and that it **must state plainly that transforms require a custom
mapper and form part of the pipeline**. As written, a reader can come away believing
transforms work with the default mapper. That is wrong rather than merely
badly-filed, so it should not wait for the rest of this spec.

## Out of Scope

- Restating reference material. A guide links to the option tables from Spec 012; it
  does not copy them.
- Teaching fundamentals. How-tos assume competence; the ladder for newcomers is
  Spec 009.

## Source Material

- `contents/FAQ.md` (645 lines) — mine for questions that should be guides
- Brighter GitHub issues and discussions — recurring problems
- Existing pages — most guides compose two or three of them
- `../Brighter/samples/` — working code to point at

## Dependencies

- **Follows** Spec 010 — guides need the *How To* section to exist
- **Follows** Spec 012 — guides link to reference tables rather than restating options
- **Complements** Spec 009 — tutorials teach, how-tos solve; keep the boundary sharp

## Execution Order

Spec numbers are identifiers, not a sequence. Programme order is
**011 → 010 → 012 → 013**, with Spec 009 in parallel throughout. This spec runs last —
it composes content the earlier specs have already normalised, moved and tabulated.

## Risks

- **Combinatorial explosion.** Every transport × every outbox × every scheduler is
  hundreds of pages. Mitigation: guides are written for *demonstrated* demand — a real
  question asked more than once — never speculatively.
- **Duplication drift.** A guide that copies configuration will diverge from the
  reference. Mitigation: link, do not copy; enforce in review.

## Status Checklist

- [x] Requirements gathered — `requirements.md`, 2026-09-04
- [ ] Requirements reviewed and approved
- [ ] Documentation outline created
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Mine `FAQ.md` and the Brighter issue tracker for demonstrated demand
2. Agree the initial guide list and its priority order
3. Agree a standard how-to page template
4. Create requirements document

## Notes

- The PostgreSQL-for-both example ships in the first batch. It is the specific gap the
  issue author named, and closing it visibly answers the feedback.
</content>
