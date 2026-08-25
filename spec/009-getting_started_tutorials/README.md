# Spec 009: Getting Started Tutorials

**Created:** 2026-08-03
**Status:** Requirements Phase

## Topic Overview

Brighter has no tutorial. Not a thin one — none at all.

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) puts it politely: *"I
don't always need a complete explanation, sometimes I just want to get started."*
The linked [Reddit thread](https://www.reddit.com/r/dotnet/comments/1okkoqt/lets_criticise_brighters_documentaion/)
suggests this is not one voice.

Our current *Overview* section — the first three pages a newcomer meets — is:

| Page | What it actually is |
|---|---|
| `ShowMeTheCode.md` | A showcase. It opens by stating *"It's not about how... It's not about why."* Disconnected snippets; you cannot follow it to a running result. |
| `BasicConcepts.md` | 24 term definitions — a glossary. |
| `WhyBrighter.md` | Philosophy: Reactor pattern, type over convention. |

So the on-ramp is a trailer, a vocabulary list, and an argument. Nowhere do we say
*"do these seven things and you will have a working message flow."*

In Diátaxis terms a tutorial is a **guaranteed successful learning experience**: the
reader follows it exactly, it works, and they gain confidence. It is explicitly not
the place for options, trade-offs or completeness — every fork in the road is a place
the learner can fail. That discipline is what distinguishes these new pages from
`ShowMeTheCode.md`, which stays as a showcase.

## Proposed Tutorials

A four-rung ladder, each building on the last, each ending in something the reader has
actually run:

1. **Your First Command** — in-process only. Install the package, define a command,
   write a handler, wire `AddBrighter().AutoFromAssemblies()`, send it, see output.
   No transport, no broker, no Docker. Target: 10 minutes.
2. **Your First Message Over a Broker** — RabbitMQ via a supplied `docker-compose.yml`.
   Producer posts, Dispatcher consumes, InMemory Outbox. Introduces publication,
   subscription and routing key as *concepts encountered in passing*, each linked out
   to reference rather than explained inline.
3. **Adding a Durable Outbox** — replace the InMemory Outbox with Postgres, show the
   transactional guarantee, run the Sweeper. This is the step that turns a demo into
   something production-shaped, and it is where most readers currently get lost.
4. **Streaming with Kafka** — see below.

Each tutorial states its prerequisites, pins its package versions, and ends with a
"what you built / what to read next" section pointing at how-to and explanation pages.

### Why Kafka gets its own rung

Brighter's Kafka support is genuinely stronger than most competing .NET libraries, and
the reason is architectural rather than incidental. Brighter's message pump is
**single-threaded per channel**, which maps directly onto how a Kafka consumer in a
consumer group reads assigned partitions: one pump per partition assignment,
sequential processing, offsets committed in order. Libraries that dispatch each
message onto the thread pool have to fight to preserve per-partition ordering and to
commit offsets safely; Brighter gets both from the shape of the Reactor pump.

That is a real differentiator and it is currently invisible to a newcomer — Kafka
appears only as a configuration reference page filed under *"Guaranteed At Least
Once"*. The tutorial should:

- Get a topic with multiple partitions running and messages flowing
- Show a consumer group scaling out, and what happens to partition assignment when a
  second instance joins
- Demonstrate that per-partition ordering holds, and connect that to the single-threaded
  pump — with the deeper treatment linked out to `ReactorAndProactor.md`
- Cover offset-commit behaviour, since that is where Kafka newcomers lose messages

Tutorial discipline still applies: no branching, no option tours. The architectural
point is *shown working*, then linked out for the explanation.

## Scope

- Four new tutorial pages under `contents/`
- A companion runnable solution per tutorial in `../Brighter/samples/` — reusing or
  extending an existing sample where one fits, writing a new one where none does
- New *Get Started* section in `SUMMARY.md` (coordinated with Spec 010)
- Every code block complete and compilable, `using` directives included

## Tutorial Code: Sanctioned Exception to the Brighter Read-Only Rule

`CLAUDE.md` states that we never modify the Brighter or Darker repositories. **An
explicit exception is granted for tutorial sample code** (approved 2026-08-03).
Tutorial samples must live next to the other samples so they are built, compiled and
kept honest by Brighter's own CI — a tutorial whose code only exists inside a markdown
fence rots silently.

The exception is narrow:

- **Scope:** additions and extensions under `../Brighter/samples/` (and `../Darker/`
  equivalents) for tutorial purposes only
- **Mechanism:** a pull request against the Brighter repository, reviewed as normal —
  never a direct commit
- **Preference order:** reuse an existing sample → extend an existing sample → write a
  new one
- **Not covered:** `src/`, `tests/`, ADRs, release notes, or any other Brighter
  directory, which remain strictly read-only

Existing candidates to assess for reuse: `samples/CommandProcessor` (rung 1),
`samples/TaskQueue` (rungs 2 and 4), `samples/WebAPI` (rung 3).

## Out of Scope

- Rewriting `ShowMeTheCode.md`. It stays as a showcase; Spec 010 re-files it.
- Darker tutorials. Worth doing, but land the Brighter ladder first.

## Source Material

- `../Brighter/samples/` — `CommandProcessor`, `TaskQueue`, `WebAPI` are the obvious
  starting points for tutorial code
- `../Brighter/release_notes.md` for current V10 package versions
- `contents/BrighterBasicConfiguration.md` (1,068 lines) — the reference these
  tutorials deliberately do *not* duplicate

## Open Questions

Both resolved 2026-08-03 — see `requirements.md` § Resolved Questions.

1. ~~Pin package versions or float?~~ **Pin, verified per release.** A release
   checklist item (deliverable D9) requires re-running all four tutorials and bumping
   the pinned versions each release.
2. ~~Does Kafka need a multi-broker `docker-compose`?~~ **No.** Rebalancing is a
   function of partitions and consumer instances, not brokers. The existing
   single-broker `docker-compose-kafka.yaml` plus a second *consumer instance* shows
   the point, and is both simpler to run and closer to how services really scale out.

## Dependencies

- **Coordinates with** Spec 010 — tutorials need the new *Get Started* section
- **Independent of** Specs 011–013; this is additive and carries no breakage risk,
  which is why it runs first and in parallel

## Execution Order

Spec numbers are identifiers, not a sequence. This spec starts **first and runs in
parallel** with the others. Agreed order across the programme is
**011 → 010 → 012 → 013**, with 009 alongside throughout.

## Status Checklist

- [x] Requirements gathered (`requirements.md`, 2026-08-03)
- [x] Requirements reviewed and approved (2026-08-03)
- [x] Documentation outline created (`design.md`, 2026-08-03)
- [x] Outline reviewed and approved (2026-08-03; six findings applied, D12 added)
- [x] Writing tasks identified (`tasks.md`, 2026-08-23; reviewed and approved 2026-08-24 —
      39 tasks across 12 phases)
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

**Spent 2026-08-24 — for the current position read `tasks.md`'s phase table, not this
section.** It named Phase 2 and was stale by Phase 3; a pointer at one phase rots every time
a phase lands, and `tasks.md` re-derives its own tally rather than incrementing it. All three
items below were resolved during requirements and design and the list was never updated; the
checklist above is the current state. Kept rather than deleted because it records what this
spec did not yet know on 2026-08-03. Note item 2 says *three* tutorials: the ladder is
**four** rungs, settled at design.

1. Resolve the tutorial-code hosting question above
2. Confirm the three-tutorial ladder is the right shape and ordering
3. Create requirements document

## Notes

- **Every tutorial must be executed end to end before it ships.** A tutorial that
  fails at step 4 is worse than no tutorial — it is the thing that sends people to
  Reddit.
- Follow `CLAUDE.md` standards; run `python3 tools/linkcheck.py` after adding pages.
</content>
