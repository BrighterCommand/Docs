# Spec 012: Configuration Reference Tables

**Created:** 2026-08-03
**Status:** **Design APPROVED 2026-08-27; the next phase is TASKS.** `requirements.md`
approved 2026-08-27 and merged as `9df5e89`; `design.md` approved the same day, its four
review questions answered — which added **D15** and **D16** and struck AC4's schedule
clause. Three more pages, zero more options.

> **This README is stamped 2026-08-03 and `requirements.md` supersedes it on every point
> of fact.** It was written against a tree Spec 010 has since changed and a scope list
> the source no longer matches — the transport list names six of the ten that ship at
> `10.7.0`, and its *Default extraction* bullet describes one of the three shapes
> defaults actually come in. §3.6 of `requirements.md` tabulates the disagreements; the
> *rationale* below is unaffected and still stands.

## Topic Overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) asks for reference
documentation with *"precise technical information about the API"*, citing
[Microsoft Learn's API browser](https://learn.microsoft.com/en-us/dotnet/api/).

That comparison needs unpicking, because it names two different products:

- **Generated API reference** — every public type and member, produced from XML doc
  comments by DocFX or similar. This is built from source and belongs in the
  **Brighter repository**, not here. Worth doing; a separate initiative.
- **Prose configuration reference** — the options a user actually sets: name, type,
  default, description, constraints. This lives here, and today it is inconsistent and
  buried in prose.

This spec covers the second. It is what most readers asking for "reference" actually
want: they are not looking up `IAmACommandProcessor`'s member list, they are trying to
find out what `MakeChannels` defaults to.

Today each transport documents its options differently — some as tables, some as prose
paragraphs, some only implicitly via a code sample. There is no way to answer "what
are all the RabbitMQ subscription options and their defaults" without reading 565
lines.

## Proposed Format

One consistent table per configuration surface:

| Option | Type | Default | Description |
|---|---|---|---|
| `MakeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates the queue/topic if absent, assumes it exists, or validates it. |
| `BufferSize` | `int` | `1` | Messages prefetched per read. Raise for throughput; lowers ordering guarantees. |

Tables beat prose for both audiences: humans scan them, and they extract reliably into
a retrieval chunk where the same facts written as sentences do not.

Every table is verified against the source type in `../Brighter/src/`, not against
existing documentation — the point of this spec is to be authoritative, and
transcribing our own prose would propagate any drift already there.

## Scope

Configuration reference tables for:

- **Transports** — RabbitMQ, Kafka, AWS SNS/SQS, Azure Service Bus, Postgres, plus
  in-memory (7 surfaces: connection, publication, subscription per transport where they
  differ)
- **Outboxes** — MSSQL, MySQL, Postgres, Sqlite, Dapper, EF Core, Dynamo, MongoDB
- **Inboxes** — MSSQL, MySQL, Postgres, Sqlite, Dynamo, MongoDB
- **Schedulers** — InMemory, Hangfire, Quartz, TickerQ, AWS, Azure, Custom
- **Distributed locks** — DynamoDB, Postgres, MSSQL, MySQL, Azure Blob, MongoDB, Firestore
- **Core** — `AddBrighter` options, `AddProducers`, Dispatcher/subscription options
- **Middleware attributes** — the `UsePolicy` / `UseInbox` / `RequestLogging` family:
  parameters, ordering semantics, and defaults

## Out of Scope

- Generated API reference. Recommend separately to the Brighter repo; do not attempt
  here.
- Explanatory content about *why* an option exists — that goes to the explanation
  pages per Spec 011's mode discipline.

## Source Material

- `../Brighter/src/` — the configuration and options types are the authority
- `../Brighter/release_notes.md` — for defaults changed in V10
- Existing `contents/*Configuration.md` pages — for coverage checking, **not** as the
  source of truth

## Dependencies

- **Follows** Spec 011 — tables land in pages that have already been split by mode
- **Follows** Spec 010 — new reference pages need their place in the tree

## Drift Strategy: Automated Verification (decided 2026-08-03)

A hand-maintained table of defaults is stale the moment someone changes a default in
Brighter, and — worse — stale *silently*. A wrong default in a reference table is more
damaging than a missing one, because the reader has no reason to doubt it.

**Decision: build a checker.** `tools/optioncheck.py` (or a small C# tool, see below)
reflects over the Brighter assemblies, extracts public configuration properties with
their types and default values, and diffs that against the tables in `contents/`. It
reports options documented but no longer present, options present but undocumented,
and defaults that disagree. It exits non-zero on mismatch so CI can gate on it, in the
same spirit as `tools/linkcheck.py`.

This makes the tables self-policing: the day someone changes `BufferSize`'s default in
Brighter, our build tells us the docs are wrong rather than a user discovering it.

Implementation questions for the requirements phase — **answered 2026-08-27; full
reasoning in `requirements.md` §5. Do not re-raise them:**

- ~~**Language.**~~ **C#**, run via `dotnet run`. Reflection over .NET assemblies is
  native there, and the Python alternative would have to *evaluate* default expressions
  such as `TimeSpan.FromMilliseconds(500)` that reflection simply reads.
- ~~**Assembly source.**~~ **Restored NuGet packages, pinned.** It is what readers
  consume, and — the load-bearing reason — **it runs in CI**, which a sibling checkout
  cannot. Measured on the day of the decision: `../Brighter` is on another agent's
  branch, **173 commits past `10.7.0`**, so it is not a reproducible authority either.
- **Default extraction** — the bullet below was **half right, and the wrong half is
  load-bearing.** Property initialisers are indeed readable from an instantiated object.
  But there are **three** shapes, not two, and the third is a trap: 60% of Brighter's
  defaulted constructor parameters default to `null` in the signature and get their real
  value from a `??` in the constructor body. A checker reading the signature documents
  those as `null` — **wrong, not merely missing**, which is the one failure this whole
  drift strategy exists to prevent. See `requirements.md` §5.1.

## Risks

- **Checker complexity.** The tool is real engineering, not a script, and could outgrow
  the docs work it supports. Mitigation: scope it to reliably-determinable defaults and
  accept partial coverage rather than gold-plating.
- **Volume.** This is the largest spec by raw page count. It parallelises well — each
  surface is independent — but the review load is real.

## Status Checklist

- [x] Requirements gathered — `requirements.md`, 2026-08-27
- [x] Requirements reviewed and approved — 2026-08-27, merged as `9df5e89`
- [x] Documentation outline created — `design.md`, 2026-08-27
- [x] Outline reviewed and approved — 2026-08-27, four questions answered
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Execution Order

Spec numbers are identifiers, not a sequence. Programme order is
**011 → 010 → 012 → 013**, with Spec 009 in parallel throughout.

## Next Steps

~~1. Agree the table format and required columns~~ — **proposed** in `requirements.md`
§7.1; agreed at review, not here.
~~2. Choose the checker's implementation language and assembly source~~ — **answered**,
see above.
~~4. Create requirements document~~ — **done 2026-08-27.**

What is actually next:

1. **Review `requirements.md`** (`/spec:review`). Three questions in its §13 need a
   maintainer, and the first changes the size of the spec: **five shipping transports
   have no configuration page** — GCP Pub/Sub and RocketMQ with zero corpus presence,
   MQTT, Redis and MSSQL asserted in comparison tables with no page behind them.
2. Then the design phase, prioritising surfaces (core and transports first).

## Notes

- Verify every default against source. A wrong default in a reference table is worse
  than an absent one.
