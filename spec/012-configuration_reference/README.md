# Spec 012: Configuration Reference Tables

**Created:** 2026-08-03
**Status:** Requirements Phase

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

Implementation questions for the requirements phase:

- **Language.** Reflection over .NET assemblies is natural in C#, but the repo's
  existing tooling is Python (`linkcheck.py`) and this repo has no .NET build. Options:
  a small C# tool run via `dotnet run`, or Python parsing the source with a C# grammar
  (more fragile). Leaning C#.
- **Default extraction.** Defaults set in property initialisers are readable by
  reflection on an instantiated options object; defaults set in constructors or applied
  later in a builder are harder. Scope the checker to what it can determine reliably,
  and mark the rest as manually verified rather than silently missing them.
- **Assembly source.** Reference the sibling `../Brighter` checkout, or restore
  published NuGet packages for a pinned version? The latter is more reproducible and
  matches what users actually consume.

## Risks

- **Checker complexity.** The tool is real engineering, not a script, and could outgrow
  the docs work it supports. Mitigation: scope it to reliably-determinable defaults and
  accept partial coverage rather than gold-plating.
- **Volume.** This is the largest spec by raw page count. It parallelises well — each
  surface is independent — but the review load is real.

## Status Checklist

- [ ] Requirements gathered
- [ ] Requirements reviewed and approved
- [ ] Documentation outline created
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Execution Order

Spec numbers are identifiers, not a sequence. Programme order is
**011 → 010 → 012 → 013**, with Spec 009 in parallel throughout.

## Next Steps

1. Agree the table format and required columns
2. Choose the checker's implementation language and assembly source
3. Prioritise surfaces (transports first — highest traffic)
4. Create requirements document

## Notes

- Verify every default against source. A wrong default in a reference table is worse
  than an absent one.
</content>
