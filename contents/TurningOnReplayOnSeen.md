---
description: "This page is the procedure: what to switch on, what you must thread through your code, what to check before you enable it, and a worked example."
layout:
  description:
    visible: false
---

# Turning On Replay On Seen

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Replay On Seen](/contents/ReplayOnSeen.md)

This page is the procedure: what to switch on, what you must thread through your code, what to check before you enable it, and a worked example. For what Replay On Seen is and why it behaves as it does, see [Replay On Seen](/contents/ReplayOnSeen.md).

## Turning It On

`Replay` is a third value of `OnceOnlyAction`, alongside `Throw` and `Warn`. You choose it
the same way you choose those: per handler with the `[UseInbox]` attribute, or globally
through `InboxConfiguration`. Whichever route you take, `onceOnly` must be `true` —
without it the Inbox records requests but never checks for duplicates, so the replay path
is never reached.

### On a Handler

Set `onceOnlyAction: OnceOnlyAction.Replay` on the attribute:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;

public class ProcessPaymentHandler : RequestHandler<ProcessPayment>
{
    private readonly IAmACommandProcessor _commandProcessor;

    public ProcessPaymentHandler(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    [UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandler), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Replay)]
    public override ProcessPayment Handle(ProcessPayment command)
    {
        // ... take the payment ...

        // Pass the pipeline's Context so ShipOrder is stored under this invocation's
        // Causation Id — that is what a later replay will match on
        _commandProcessor.Post(new ShipOrder { OrderId = command.OrderId },
            Context as RequestContext);

        return base.Handle(command);
    }
}
```

Two details carry over from any other Inbox configuration. `contextKey` disambiguates the
request id when the same id reaches more than one pipeline — the handler's own type is the
usual choice. And `step` places the Inbox in the pipeline; it must run before your handler,
which it does by default (`HandlerTiming.Before`).

The one detail specific to replay is the `Context` passed to `Post`. Without it the
outgoing message is stored with no Causation Id and a later replay finds nothing to
resend — silently. That failure is common enough to have
[its own section below](#you-must-thread-your-requestcontext).

For an async handler, use `[UseInboxAsync]` on a `RequestHandlerAsync<T>`:

```csharp
public class ProcessPaymentHandlerAsync : RequestHandlerAsync<ProcessPayment>
{
    private readonly IAmACommandProcessor _commandProcessor;

    public ProcessPaymentHandlerAsync(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    [UseInboxAsync(step: 1, contextKey: typeof(ProcessPaymentHandlerAsync), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Replay)]
    public override async Task<ProcessPayment> HandleAsync(ProcessPayment command,
        CancellationToken cancellationToken = default)
    {
        // ... take the payment ...

        await _commandProcessor.PostAsync(new ShipOrder { OrderId = command.OrderId },
            Context as RequestContext, cancellationToken: cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

Pick the attribute that matches your handler — `[UseInbox]` for `RequestHandler<T>`,
`[UseInboxAsync]` for `RequestHandlerAsync<T>`. Mixing them breaks the rule that
[pipelines must be homogeneous](/contents/DispatchingARequest.md#pipelines-must-be-homogeneous),
and [pipeline validation](/contents/PipelineValidation.md) reports it at startup.

### Globally

To make `Replay` the default for every handler, set `actionOnExists` when you construct
the `InboxConfiguration` you hand to `AddConsumers`:

```csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(options =>
    {
        options.InboxConfiguration = new InboxConfiguration(
            inbox: new MySqlInbox(new RelationalDatabaseConfiguration(DbConnectionString())),
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Replay);

        // ... other consumer options
    });
}
```

`ActionOnExists` is set through the constructor and is read-only afterwards, so you choose
replay when you build the configuration rather than flipping it on an existing instance.
The remaining parameters behave exactly as they do for `Throw` and `Warn`; see
[Inbox Configuration](/contents/DispatcherConfigurationReference.md#inbox) for the full set.

`InboxScope.Commands` is the usual scope here. Replay resends a handler invocation's
downstream messages, and it is commands — one request, one handler — where that maps
cleanly; an event delivered to several handlers is a case worth understanding before you
opt into it, covered under [Limitations](/contents/ReplayOnSeenReference.md#replay-on-seen-limitations).

A global setting still applies per handler, so a handler carrying its own `[UseInbox]`
attribute keeps that attribute's `onceOnlyAction`, and a handler marked
`[NoGlobalInbox]` opts out entirely. Setting `Replay` globally is therefore the
straightforward way to satisfy the
[every-step requirement](/contents/ReplayOnSeen.md#every-already-seen-step-needs-replay) — it covers the whole
cascade in one place.

## You Must Thread Your RequestContext

> **⚠ Required**: Every handler that deposits messages you want replayed must pass its own
> `Context` to `Post` or `DepositPost`. Omit it and replay becomes a silent no-op — no
> exception, no error log, nothing resent.

This is the step most people miss, and because it fails silently it is worth getting right
before you enable anything else.

### Why it matters

The Causation Id lives in the pipeline's `RequestContext.Bag`. The Inbox handler puts it
there, and the Outbox `Add` reads it back out — but only from the context it is actually
given. Call `Post` with no context and the Command Processor creates a **fresh**
`RequestContext` for that call. The new context has an empty Bag, so:

1. The outgoing message is stored with a `null` Causation Id.
2. The Inbox entry still gets the right Causation Id, because the Inbox handler used the
   pipeline's context.
3. Later, a duplicate arrives. The Inbox hands over its Causation Id, the Outbox looks for
   messages stored under it, and finds none — the messages are all sitting there with
   `null`.

The deposit worked. The dispatch worked. The Inbox recorded everything correctly. Only the
link between them is missing, and nothing in normal operation reveals that until the day
you need to replay.

### The fix

```csharp
// ...
// ❌ Silent no-op under Replay — a fresh context, so the Causation Id is lost
public override ProcessPayment Handle(ProcessPayment command)
{
    _commandProcessor.Post(new ShipOrder { OrderId = command.OrderId });
    return base.Handle(command);
}

// ✅ Threads the pipeline's context, so the message is stored under this
//    invocation's Causation Id and a later replay can match it
public override ProcessPayment Handle(ProcessPayment command)
{
    _commandProcessor.Post(new ShipOrder { OrderId = command.OrderId },
        Context as RequestContext);
    return base.Handle(command);
}
```

`Context` is typed as `IRequestContext?` on `RequestHandler<T>` and `RequestHandlerAsync<T>`,
while `Post` takes a `RequestContext?`, so the `as RequestContext` cast is what makes this
compile. With Brighter's own request context factory the cast always succeeds. If you have
supplied a custom `IAmARequestContextFactory` that returns something other than
`RequestContext`, the cast yields `null` — which puts you back in the ❌ case, and is
covered under [When Replay Does Not Fire](/contents/ReplayOnSeenReference.md#when-replay-does-not-fire).

### Which calls this applies to

Any call that puts a message in the Outbox takes a `RequestContext?` parameter, and all of
them need yours:

- `Post` and `PostAsync`
- `DepositPost` and `DepositPostAsync`

The rule is the same in each case: pass `Context as RequestContext` rather than letting the
parameter default. This matters only for handlers whose messages you want replayed —
posting from outside a handler pipeline, where there is no Causation Id to inherit, is
unaffected.

### The symptom

There is no exception and no warning, so you have to know what to look for:

- A duplicate arrives, the handler correctly does not run, and **no messages are resent**.
  The Sweeper has nothing outstanding to pick up.
- The Outbox rows for the original run have a `null` causation column.
- The replay attempt is logged at **Debug** level, so at default log levels you see
  nothing at all.
- If you are collecting traces, the pipeline span carries a
  `UseInboxHandler Duplicate Replay Skipped` event rather than
  `UseInboxHandler Duplicate Replay` — see [Observability](/contents/ReplayOnSeenReference.md#observability).

If replay appears to do nothing, check the causation column on the Outbox rows first. A
column full of nulls means the context was not threaded; that is the fastest way to
confirm this particular fault rather than one of the
[other reasons replay stays quiet](/contents/ReplayOnSeenReference.md#when-replay-does-not-fire).

## Before You Enable It

Replay depends on six things being true at once. Most of them fail quietly — only the two
marked as startup errors stop your host — so it is worth walking the list before you set
`OnceOnlyAction.Replay` anywhere.

| Requirement | How to satisfy it | What happens if you don't |
|---|---|---|
| The Inbox implements `IAmACausationTrackingInbox` | Every Brighter-maintained Inbox already does. A hand-written Inbox must implement it — see [Implementing Causation Tracking in Your Own Store](/contents/CausationTrackingStores.md) | Startup **error** from [pipeline validation](/contents/PipelineValidation.md): replay has no way to look up the original Causation Id |
| The Outbox implements `IAmACausationTrackingOutbox` | Same — every Brighter-maintained Outbox does | Startup **error**. If there is no Outbox at all, you get a **warning** instead: the duplicate is skipped and nothing is resent |
| The store schema carries the causation column | Run [box provisioning](/contents/BoxProvisioning.md) to bring the schema up to date; on DynamoDB, create the [Causation index](/contents/DynamoOutbox.md#replay-support-the-causation-index). See [Store Support](/contents/ReplayOnSeenReference.md#store-support) for what each store needs | Startup **warning** only — the host starts. At runtime the store reports no causation support, duplicates are skipped, and nothing is resent |
| An Outbox Sweeper is running | Register one with `.UseOutboxSweeper(...)` — see below | The messages are marked undispatched and stay that way. Nothing reaches the broker |
| Handlers pass their `Context` when posting | `Context as RequestContext` on every `Post`/`DepositPost` — see [You Must Thread Your RequestContext](#you-must-thread-your-requestcontext) | Messages are stored with a `null` Causation Id. Replay finds nothing to resend, silently |
| Every already-seen step is set to `Replay` | The attribute on each handler, or `actionOnExists` globally — see [Turning It On](#turning-it-on) | The cascade stops at the first step still on `Throw` or `Warn` — see [Every already-seen step needs Replay](/contents/ReplayOnSeen.md#every-already-seen-step-needs-replay) |

Note how much of this is warnings rather than errors. A schema that was never migrated
does not fail startup; it produces a log line at boot and then a system that looks healthy
and never replays anything. [When Replay Does Not Fire](/contents/ReplayOnSeenReference.md#when-replay-does-not-fire) lists
each of these symptoms with the message you will see.

### Replay only ever sends through the Sweeper

The Sweeper row deserves expanding, because replay has **no immediate-send path**. When a
duplicate arrives, all the Outbox does is clear the dispatched state of the messages
stored under that Causation Id. Something else has to notice they are outstanding and
dispatch them, and that something is the Sweeper.

This holds even if you never think about the Sweeper today. Code that calls `Post` gets
its messages sent in-line, so a producer with no Sweeper can appear to work — but that
in-line clear belongs to the deposit, not to replay. Nothing in the replay path sends
anything.

The `InMemoryOutbox` is no exception: it supports replay perfectly well, and it still
needs a Sweeper to dispatch what replay marks outstanding.

Register one on your producer registration:

```csharp
using Paramore.Brighter.Outbox.Hosting;

services.AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = new MySqlOutbox(new RelationalDatabaseConfiguration(DbConnectionString()));
        // ... connection and transaction providers
    })
    .UseOutboxSweeper(opt =>
    {
        opt.TimerInterval = 5;                            // seconds between sweeps
        opt.MinimumMessageAge = TimeSpan.FromSeconds(5);  // how settled a message must be
        opt.BatchSize = 100;
    });
```

The interval is your recovery latency. Each hop in a cascade waits for a sweep, so a
three-step flow takes at least three intervals to walk forward.

Running a Sweeper is not a replay-specific requirement — it is how Brighter guarantees
delivery at all, and [You always need a Sweeper](/contents/BrighterOutboxSupport.md#you-always-need-a-sweeper)
explains why, including how it interacts with transports such as RabbitMQ and Kafka that
confirm sends asynchronously. Replay only makes the requirement unavoidable. If you scale
your producer out, keep a
[single Sweeper active with a distributed lock](/contents/DistributedLock.md).

## Upgrading Without Migrating

Taking a Brighter version that has causation tracking does not oblige you to migrate
anything. An un-migrated store keeps depositing exactly as it did before — same INSERT, same
columns, same behaviour — and you migrate when you decide to adopt replay, not when you take
the package.

That works because the stores check their own schema rather than assuming it. Each
relational store probes once for the causation column, and the answer gates two things: what
`SupportsCausationTracking()` reports to
[pipeline validation](/contents/PipelineValidation.md), and which INSERT the write path
uses. Column present, and `Add` writes the Causation Id along with the message. Column
absent, and `Add` falls back to the original INSERT and writes nothing extra.

The read path is gated by the same answer, so an un-migrated store degrades rather than
failing:

- The Inbox returns `null` from `GetCausationId` instead of selecting a column that is not
  there.
- The Outbox returns `false` from `ReplayCausation` instead of issuing an `UPDATE ... WHERE
  CausationId = @id` against a missing column.

That second one covers the mixed state, where you migrate one box and not the other. A
migrated Inbox hands over a perfectly good Causation Id, and an un-migrated Outbox declines
it and reports that nothing was resent — rather than throwing a SQL error into your consumer
pipeline.

DynamoDB works the same way with a different question: its Outbox probes `DescribeTable` for
the Causation index rather than probing for a column, and writing the attribute to a table
without the index is a harmless no-op.

### The probe is memoized, so migrating mid-process needs a restart

The probe runs once per store instance and the result is cached for that instance's
lifetime. It is never re-checked and never invalidated. Since your stores are typically
singletons, "once per store instance" means once per process.

That is what you want for throughput — no schema query per deposit — but it has one
operational consequence:

> **⚠ A store built before the migration caches "no causation support" and keeps that answer
> until the process restarts.** Migrating the database under a running host does not turn
> replay on in that host.

Usually this never comes up. [Box provisioning](/contents/BoxProvisioning.md) runs at host
startup, before your stores handle any traffic, so the first probe already sees the migrated
schema. You hit it when the migration happens somewhere else — a DBA applying the DDL by
hand, a separate provisioning job, a release pipeline that migrates while the previous
version is still serving. In each case the fix is the same: restart the hosts after
migrating. See [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) for
what to expect from a migration run and what to check afterwards.

The symptom is worth recognising, because it looks exactly like a schema problem you have
already fixed: the database has the column, `psql` or SSMS confirms it, and startup
validation still warns that the store schema does not support causation tracking. It is
reporting what the store probed, which may be older than what you are looking at.

### Migrating does not backfill

One last consequence of migrating late. Inbox entries written before the migration have no
Causation Id, and adding the column does not go back and work out what it should have been —
the link between an entry and the messages it produced exists only if it was recorded while
the request was being handled. Requests handled after the migration replay normally;
requests handled before it never will. If you are enabling replay to recover a flow that is
stalled *right now*, that flow's original handling predates the column, and replay cannot
help with it.

## A Worked Example

Here is the stalled order from the top of the page, followed all the way through: Payment
handled `ProcessPayment` and deposited `ShipOrder`, but Shipping never received it. Both
handlers are configured for replay; only Payment has ever run.

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;

// Payment service — has already handled this command once
public class ProcessPaymentHandler : RequestHandler<ProcessPayment>
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IPaymentGateway _payments;

    public ProcessPaymentHandler(IAmACommandProcessor commandProcessor, IPaymentGateway payments)
    {
        _commandProcessor = commandProcessor;
        _payments = payments;
    }

    [UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandler), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Replay)]
    public override ProcessPayment Handle(ProcessPayment command)
    {
        _payments.Charge(command.OrderId, command.Amount);

        // Threading Context stores ShipOrder under this invocation's Causation Id
        _commandProcessor.Post(new ShipOrder { OrderId = command.OrderId },
            Context as RequestContext);

        return base.Handle(command);
    }
}

// Shipping service — has never seen this order
public class ShipOrderHandler : RequestHandler<ShipOrder>
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly ICourier _courier;

    public ShipOrderHandler(IAmACommandProcessor commandProcessor, ICourier courier)
    {
        _commandProcessor = commandProcessor;
        _courier = courier;
    }

    [UseInbox(step: 1, contextKey: typeof(ShipOrderHandler), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Replay)]
    public override ShipOrder Handle(ShipOrder command)
    {
        var consignment = _courier.Book(command.OrderId);

        _commandProcessor.Post(new OrderShipped { OrderId = command.OrderId, Consignment = consignment },
            Context as RequestContext);

        return base.Handle(command);
    }
}
```

`ShipOrderHandler` is configured for replay even though it has never run. That costs nothing
while its Inbox is empty, and it means the step is ready if the flow ever has to be walked
forward *past* it.

### The original run

1. **`ProcessPayment` arrives** at the Payment service. Its Inbox has no entry for this id,
   so the pipeline continues into the handler. The Inbox handler first stamps the Bag with
   the Causation Id — the command's own `Id`, call it `pay-1`.
2. **The handler charges the card and posts `ShipOrder`**, passing `Context as
   RequestContext`. The Outbox stores `ShipOrder` with `CausationId = pay-1`.
3. **The handler returns and the Inbox entry is written**, also carrying `pay-1`. Note the
   order: the entry is written *after* your handler completes, not before.
4. **`ShipOrder` is dispatched** to the broker and never arrives at a consumer — the queue is
   misbound, or Shipping is down and the message ends up dead-lettered. Shipping's Inbox
   stays empty.

At this point the money is taken, no parcel is moving, and the two stores hold:

| Store | Contents |
|---|---|
| Payment Inbox | one entry for `ProcessPayment`, `CausationId = pay-1` |
| Payment Outbox | one `ShipOrder` message, `CausationId = pay-1`, marked dispatched |
| Shipping Inbox | empty |

### The recovery

5. **You re-send `ProcessPayment`** — the same command, the same id.
6. **Payment's Inbox finds the duplicate.** The action is `Replay`, so the Inbox handler asks
   for the stored Causation Id, gets `pay-1`, and calls `ReplayCausation("pay-1")` on the
   Outbox. Every message stored under `pay-1` — the one `ShipOrder` — has its dispatched
   state cleared and becomes outstanding again.
7. **`ProcessPaymentHandler` never runs.** The card is not charged a second time, and no new
   `ShipOrder` is generated. The pipeline span picks up a `UseInboxHandler Duplicate Replay`
   event tagged with `pay-1`.
8. **The Sweeper's next pass** finds the outstanding `ShipOrder` and dispatches it. It is the
   *same* message, with the same message id and the same body as the one deposited in step 2.
9. **Shipping receives `ShipOrder`.** Its Inbox has never seen this id, so there is no
   duplicate to act on and `ShipOrderHandler` runs normally — for the first time. The courier
   is booked, `OrderShipped` is deposited under a fresh Causation Id, and Shipping writes its
   own Inbox entry.

| Store | Contents after recovery |
|---|---|
| Payment Inbox | unchanged — still one entry, `CausationId = pay-1` |
| Payment Outbox | the same `ShipOrder` message, dispatched again |
| Shipping Inbox | one entry for `ShipOrder` |
| Shipping Outbox | one `OrderShipped` message under its own Causation Id |

The flow reached the step that had never run, and the step that had already run was skipped
rather than repeated. Nothing was recomputed: the `ShipOrder` that finally arrived is the
one Payment produced an hour earlier.

### What backs this up

Brighter's test suite verifies this mechanism end to end in
`Paramore.Brighter.Core.Tests/OnceOnly/When_a_seen_message_is_replayed_end_to_end.cs`, which
is the reference implementation to read — there is no sample application for replay yet. Two
differences from the walkthrough above are worth knowing if you go and read it:

- **The test covers one step, not a cascade.** It has a single handler forwarding a single
  event, and proves that a duplicate resends that event without re-running the handler. The
  two-step version above extrapolates from that; each hop is the same mechanism repeated.
- **The test does not run a Sweeper.** It calls `ClearOutbox` directly to re-dispatch what
  replay made outstanding, because a background Sweeper would make the test timing-dependent.
  Replay's own effect stops at "the message is outstanding again" — in production, step 8 is
  the Sweeper's job.
