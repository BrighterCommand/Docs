# Replay On Seen Reference

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Replay On Seen](/contents/ReplayOnSeen.md)

Which stores support Replay On Seen, the cases where it does not fire, the events it emits, and its limitations. For the concept see [Replay On Seen](/contents/ReplayOnSeen.md); for the procedure see [Turning On Replay On Seen](/contents/TurningOnReplayOnSeen.md).

## Store Support

Every Inbox and Outbox Brighter ships implements the causation-tracking role interfaces, so
there is no store you have to swap out to use replay. What differs is whether the *store
itself* can hold a Causation Id yet — a relational table needs a column, DynamoDB's Outbox
needs an index, and a schemaless store needs nothing at all.

Each store answers that question for itself, at runtime, through
`SupportsCausationTracking()`. Some stores return `true` outright; the relational stores and
the DynamoDB Outbox go and look.

| Store | Inbox | Outbox | What you must do first |
|---|---|---|---|
| MSSQL, MySQL, PostgreSQL, SQLite | Needs the causation column | Needs the causation column | Run [box provisioning](/contents/BoxProvisioning.md#per-backend-support) to bring both boxes up to date — Outbox **V8**, Inbox **V3** (**V2** on PostgreSQL) |
| Spanner | Needs the causation column | Needs the causation column | Same columns, but the Spanner provisioner is fresh-install-only. A new database gets the current shape; an existing one needs the column added by hand |
| MongoDB | Nothing to do | Nothing to do | Nothing — the document model carries the field without a schema change |
| Firestore | Nothing to do | Nothing to do | Nothing |
| DynamoDB (and `.V4`) | Nothing to do | Needs the **Causation** global secondary index | Create the index — see [Replay Support: The Causation Index](/contents/DynamoOutbox.md#replay-support-the-causation-index) |
| In-memory | Nothing to do | Nothing to do | Nothing — but you still need a Sweeper. Development and testing only |

DynamoDB is the one asymmetric row. Its Inbox looks the Causation Id up by the table's own
primary key, so it needs no index; its Outbox has to find every message carrying a given
Causation Id, which is a query on a non-key attribute and therefore needs the GSI. The
Outbox probes for that index with `DescribeTable` and reports no causation support if it is
absent.

**Both boxes have to be ready.** Replay reads the Causation Id from the Inbox and then asks
the Outbox to replay it, so a migrated Outbox next to an un-migrated Inbox replays nothing —
and so does the reverse. [Pipeline validation](/contents/PipelineValidation.md) reports each
store separately at startup, but only as a warning: a half-migrated pair starts cleanly and
then quietly does nothing. See [When Replay Does Not Fire](#when-replay-does-not-fire).

**Column casing varies by backend.** PostgreSQL folds unquoted identifiers to lower case, so
its column is `causationid`; MSSQL, MySQL, SQLite, and Spanner use `CausationId`. The sized
backends also use different widths for the two boxes — 255 characters on the Outbox, 256 on
the Inbox. If you are checking a migration by hand, or writing a query against the causation
column, use the casing your own backend uses rather than the one you saw in another
example.

Migrating a live deployment is safe: an un-migrated store keeps accepting deposits, because
the write path falls back to the old shape when the column is missing. That fallback, and
the restart it implies once you *have* migrated, is covered under
[Upgrading Without Migrating](/contents/TurningOnReplayOnSeen.md#upgrading-without-migrating).

Writing your own Inbox or Outbox, rather than using one Brighter ships? The role interfaces
it has to implement, and the three rules that are easy to get wrong, are covered in
[Implementing Causation Tracking in Your Own Store](/contents/CausationTrackingStores.md).

## When Replay Does Not Fire

Replay is quiet when it works and quiet when it doesn't. A duplicate arrives, your handler
correctly does not run, and the Inbox handler returns — and from the outside that looks the
same whether messages were resent or nothing happened at all. The replay attempt is logged
at **Debug** level, so at default log levels a working replay and a broken one produce
identical output: none.

Two places tell you the truth. [Pipeline validation](/contents/PipelineValidation.md)
catches the misconfigurations it can see at startup, and traces distinguish a real replay
from a no-op at runtime — see [Observability](#observability). This section covers both,
starting with what validation reports.

### What validation reports at startup

The `ReplayRequiresCausationTracking` rule runs as part of the handler pipeline checks, and
only for pipelines that actually configure `OnceOnlyAction.Replay` — a pipeline on `Throw`
or `Warn` produces nothing. It checks the Inbox, then the Outbox, and it probes the live
stores for their schema capability rather than assuming it.

| Finding | Severity | Cause | Fix |
|---|---|---|---|
| Inbox is not causation-tracking | **Error** | The configured Inbox does not implement `IAmACausationTrackingInbox`, so there is no way to look up the original Causation Id | Use a Brighter-maintained Inbox, or implement the interface — see [Implementing Causation Tracking in Your Own Store](/contents/CausationTrackingStores.md) |
| Inbox schema does not support it | Warning | The Inbox implements the interface, but the live store reports no causation support — usually an un-migrated schema | Migrate the Inbox. See [Store Support](#store-support) |
| No Outbox to replay | Warning | Replay is configured on a handler with no Outbox injected | Expected for a terminal step that deposits nothing. Otherwise, configure an Outbox |
| Outbox is not causation-tracking | **Error** | The configured Outbox does not implement `IAmACausationTrackingOutbox`, so the dispatched state of the original messages cannot be reset | Use a Brighter-maintained Outbox, or implement the interface |
| Outbox schema does not support it | Warning | The Outbox implements the interface, but the live store reports no causation support — an un-migrated schema, or on DynamoDB a missing [Causation index](/contents/DynamoOutbox.md#replay-support-the-causation-index) | Migrate the Outbox, or create the index |
| Capability probe failed | Warning | The probe itself threw — the store was unreachable, or the connecting account cannot read the schema | Fix connectivity or permissions. Until you do, the store's capability is unknown and its schema finding is suppressed |

Each message is prefixed with the handler it came from — `Handler 'ProcessPaymentHandler'`.
An unconfigured Inbox renders as `'(none)'` in the first message. The messages read:

```text
OnceOnlyAction.Replay requires a causation-tracking inbox, but the configured
inbox 'MyCustomInbox' does not implement IAmACausationTrackingInbox — Replay
cannot find the causation id of the original handling

OnceOnlyAction.Replay requires causation tracking, but the inbox store schema
does not support it — migrate the inbox schema to add the CausationId column
for Replay to work

OnceOnlyAction.Replay has no outbox to replay — on a duplicate the handler is
skipped and no messages are resent (Replay is a graceful terminal step without
an outbox)

OnceOnlyAction.Replay requires a causation-tracking outbox, but the configured
outbox 'MyCustomOutbox' does not implement IAmACausationTrackingOutbox — Replay
cannot reset the dispatched state of the original messages

OnceOnlyAction.Replay requires causation tracking, but the outbox store schema
does not support it — migrate the outbox schema to add the CausationId column
for Replay to work

OnceOnlyAction.Replay could not verify causation-tracking support on the outbox
store — the schema capability probe failed (SqlException: Login failed for user
'brighter'). Ensure the store is reachable and its schema is migrated before
relying on Replay
```

Note the store names in the two "does not implement" messages. Every Inbox and Outbox
Brighter ships implements the role interfaces, so those two findings can only ever name a
store you wrote yourself — a Brighter-maintained store that is merely un-migrated produces
the *schema* warning instead.

Two more details worth knowing when you read the output. Only the two "does not implement"
findings are **Errors**; everything else is a Warning, which means a host with a completely
un-migrated schema starts perfectly cleanly and then never replays anything. And the Inbox
findings come first: if there is no Outbox, or the Outbox is not causation-tracking,
validation stops there and reports nothing about the Outbox schema — fix what it reports
and run again rather than assuming the list is exhaustive.

### The failures validation cannot see

Everything above is visible from your configuration at startup. The rest is not — these
produce no finding, no exception, and no log above Debug. They are the reasons a correctly
validated system still replays nothing.

**Your handler did not thread its `Context`.** The outgoing messages were stored with a
`null` Causation Id, so the Outbox finds nothing under the Inbox's id. This is the most
common cause by a wide margin; check the causation column on the Outbox rows first. See
[You Must Thread Your RequestContext](/contents/TurningOnReplayOnSeen.md#you-must-thread-your-requestcontext).

**A custom request context factory.** If you have supplied an `IAmARequestContextFactory`
that returns something other than a `RequestContext`, the Inbox handler cannot use the
pipeline's context at all and falls back to a throwaway whose Bag never reaches the Outbox.
This one does leave a trace: a single warning, logged once per process the first time a
Replay pipeline hits it.

```text
A custom IRequestContext (not a RequestContext) was supplied; the causation id
cannot flow to downstream handlers, so OnceOnlyAction.Replay will be a no-op
```

Treat "once" as best effort rather than a guarantee — under concurrent first hits you may
see it a couple of times.

**The Inbox entry predates causation tracking.** An entry written before you migrated the
schema, or before you turned replay on, has no Causation Id stored against it. There is no
backfill, and there is nothing to reconstruct it from: the link between an Inbox entry and
the messages it produced only exists if it was recorded at the time. Requests handled from
now on are fine; historical ones can never be replayed.

**A downstream step is still on `Throw` or `Warn`.** Replay fires correctly at the step you
re-sent, and the cascade stops at the first handler that is not configured for it. The
symptom is distinctive — the flow moves forward by exactly one hop and stalls again. See
[Every already-seen step needs Replay](/contents/ReplayOnSeen.md#every-already-seen-step-needs-replay).

**The step has no Outbox at all.** A handler that deposits nothing has nothing to replay,
so the duplicate is simply skipped. That is correct behaviour for a terminal step, and
validation says so with a warning rather than an error.

**No Sweeper is running.** Replay did its job — the messages are outstanding in the
Outbox — but nothing is dispatching them. Check whether the rows have had their dispatched
state cleared: if they have, the fault is downstream of replay, in the Sweeper. See
[Replay only ever sends through the Sweeper](/contents/TurningOnReplayOnSeen.md#replay-only-ever-sends-through-the-sweeper).

## Observability

Because replay is silent at default log levels, traces are the practical way to see it
working. The Inbox handler records what it did as an **event on the existing pipeline span** —
it creates no span of its own — so you find it on the span for the request that turned out to
be a duplicate.

| Path | Event name | Tags |
|---|---|---|
| First handling — the entry is stored | `UseInboxHandler Add` | request id |
| Duplicate, action is `Throw` | `UseInboxHandler Duplicate Throw` | request id |
| Duplicate, action is `Warn` | `UseInboxHandler Duplicate Warn` | request id |
| Duplicate, action is `Replay` — messages resent | `UseInboxHandler Duplicate Replay` | request id, Causation Id |
| Duplicate, action is `Replay` — nothing resent | `UseInboxHandler Duplicate Replay Skipped` | request id, Causation Id (null) |

The event names are the same on the sync and async handlers — there is no
`UseInboxHandlerAsync` variant to look for.

The tags are `paramore.brighter.request.id` and, on the two replay events,
`paramore.brighter.causation_id`. The Causation Id tag is what lets you follow a replay:
take its value, and every Outbox row it resent carries the same one.

### Replay versus Replay Skipped

That last pair of rows is the most useful thing on the page for anyone operating this
feature. `Replay` and `Replay Skipped` are the difference between "the duplicate resent its
messages" and "the duplicate resent nothing" — a distinction that produces no other visible
signal anywhere.

`Replay Skipped` is emitted whenever no messages went out: the Inbox held no Causation Id
for that entry, or the Outbox declined the replay because its schema does not support
causation tracking. Both are covered under
[When Replay Does Not Fire](#when-replay-does-not-fire). A null `paramore.brighter.causation_id`
tag alongside the Skipped event narrows it to the first case.

If you alert on anything here, alert on `Replay Skipped`. It means a duplicate arrived at a
handler you deliberately configured for replay, and the recovery you were counting on did
not happen.

### Turning the events on

The events are written only when there is a span to write them to and the pipeline's
`InstrumentationOptions` include `InstrumentationOptions.Brighter`. If you have narrowed
instrumentation to trim trace volume, you will have narrowed these away too. See
[Configurable Instrumentation](/contents/Telemetry.md#configurable-instrumentation) for the
options, and [Inbox Tracing](/contents/Telemetry.md#inbox-tracing) for the spans the Inbox
produces around them.

Two log lines complement the events: the replay attempt itself, at **Debug** — so raise the
level for `UseInboxHandler` if you want it in logs rather than traces — and the
once-per-process `CustomContextDisablesReplay` warning, quoted in full under
[The failures validation cannot see](#the-failures-validation-cannot-see).

### The re-dispatch is a separate trace

Do not expect the resent messages to appear under the trace that triggered the replay. They
will not, and nothing is broken.

The replay only clears dispatched state; the [Sweeper](/contents/TurningOnReplayOnSeen.md#replay-only-ever-sends-through-the-sweeper)
does the sending, later, on its own schedule. It starts its own `Activity` for each sweep,
and a single sweep may carry messages from several unrelated replays, so there is no parent
span to attach to — a link would have to be one-to-many and would tie the dispatch to
whichever replay happened to be first. The Causation Id tag is the join instead: it appears
on the replay event, and it is stored on every message the sweep picks up.

For the same reason, the elapsed time between the replay event and the messages arriving is
Sweeper latency, not handler latency. If a cascade looks slow in your traces, look at
`TimerInterval` and `MinimumMessageAge` before you look at your handlers.

## Replay On Seen Limitations

Replay is a small mechanism — re-deliver messages that were already recorded — and most of
its limitations follow from how small it is.

### It is not orchestration

The cascade can look like a workflow engine walking a process forward, and it is worth being
clear that it is not one. There is no coordinator: nothing holds a model of the flow, knows
which step it reached, or decides what should happen next. Each step advances only because
its own Inbox happens to be configured for replay, and the flow stops wherever that stops
being true.

Nothing follows from that beyond re-delivery, so:

- **There is no compensation.** Replay moves a stalled flow forward. It cannot unwind one,
  and a step whose original run was wrong stays wrong — its stored messages are exactly what
  gets resent.
- **There is no ordering guarantee.** Messages resent under one Causation Id are made
  outstanding together and the Sweeper dispatches them in whatever order it picks them up.
- **There is no completion signal.** Nothing tells you the flow finished, or that it stalled
  again at a later step.

If you need a coordinator, compensation, or a durable model of where a process has got to,
you need a workflow, not replay.

### One Causation Id can cover more than one handler

The default Causation Id is the handled request's own `Id`. If that id reaches several
`[UseInbox]` handlers, every message any of them deposited is stored under the same value,
and a replay triggered by any one of them resends all of them.

This is intended. It also rarely bites, because `[UseInbox]` is normally applied to Commands
— one request, one handler — rather than to Events, which are delivered to many. It is worth
thinking about before you put replay on an event.

If you need finer control, set the Causation Id yourself. The Inbox handler only supplies a
default when the Bag has no value, so a value you put there first wins:

```csharp
using Paramore.Brighter;

var context = new RequestContext();
context.Bag[RequestContextBagNames.CausationId] = batchId;

// Everything this invocation deposits is stored under batchId, and a replay
// triggered by this request resends exactly that set
_commandProcessor.Send(new ProcessPayment { OrderId = orderId }, context);
```

This works wherever you control the `RequestContext` handed to the Command Processor. For
requests arriving from a broker the pump builds the context, so the default applies unless
you have replaced the context factory.

### A replayed message can be dispatched twice

Replay clears dispatched state while the Sweeper may be mid-pass over the same rows. The
worst case is that a message goes out twice.

That is not a new hazard. Brighter's delivery guarantee is at-least-once with or without
replay, so a consumer that could not cope with a duplicate was already exposed. The Inbox on
the receiving side is the answer, as it is for every other source of duplicates.

### Historical entries can never be replayed

An Inbox entry written before causation tracking was in place has no Causation Id, nothing
backfills it, and a duplicate that lands on it produces a
[`Replay Skipped` event](#replay-versus-replay-skipped). See
[Migrating does not backfill](/contents/TurningOnReplayOnSeen.md#migrating-does-not-backfill).

### Replaying a large causation is one write

Clearing the dispatched state of every message under a Causation Id is a single statement,
and its cost scales with how many messages that handler deposited. The V8 Outbox migration
indexes the causation column precisely so this stays cheap; on DynamoDB the same job is done
by the Causation index. A handler that deposits a handful of messages per invocation — the
normal case — is not something you need to think about.
