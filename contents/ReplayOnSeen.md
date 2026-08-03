# Replay On Seen

When your [Inbox](/contents/BrighterInboxSupport.md) recognises a message it has already
handled, it normally throws or logs a warning and stops — the work is done, so there is
nothing to do. `OnceOnlyAction.Replay` changes what a duplicate means: instead of stopping,
Brighter re-sends the messages that handler already produced, by clearing their dispatched
state in the [Outbox](/contents/BrighterOutboxSupport.md) so the Sweeper picks them up
again. The handler itself never runs a second time.

## The Problem: A Workflow That Stalls Halfway

Consider an order flow spread across three services, each step driven by a command:

1. **Order** handles `PlaceOrder` and, once the order is written, deposits `ProcessPayment`.
2. **Payment** handles `ProcessPayment`, takes the money, and deposits `ShipOrder`.
3. **Shipping** handles `ShipOrder` and books the courier.

Now suppose Shipping is down for an hour. Payment ran to completion — the money is taken,
and `ShipOrder` was dispatched onto the broker — but nothing ever consumed it. Perhaps the
queue was misconfigured, perhaps the message hit the dead letter queue, perhaps Shipping
crashed mid-restart and lost it. However it happened, the customer has paid and no parcel
is going anywhere. The flow has stalled halfway.

The obvious recovery is to re-send `PlaceOrder` and let the flow run again. Today that
achieves nothing:

- Order's Inbox recognises `PlaceOrder` as already seen. With `OnceOnlyAction.Throw` it
  raises a `OnceOnlyException`; with `Warn` it logs and returns. Either way the handler
  does not run.
- Because the handler does not run, it does not deposit `ProcessPayment`.
- Payment never hears anything, so it never deposits `ShipOrder`.

Nothing moves. The Inbox has done exactly what you asked of it, and the flow is still
stuck at the same step.

### Why de-duplication alone is not enough

The Inbox exists to stop a handler running twice. That is a real guarantee, and you want
it: re-running `ProcessPaymentHandler` would take the money a second time.

But de-duplication protects the *handler*; it does not protect the *flow*. Skipping the
handler also skips everything the handler emitted, and the downstream steps are waiting on
exactly those messages. So the Inbox turns "this step already ran" into "this step and
every step after it are unreachable" — which is a strictly stronger claim than the one it
is entitled to make.

The information needed to do better is already on disk. The Outbox still holds the
`ProcessPayment` message from the original run, together with the fact that it was
produced while handling `PlaceOrder`. Nothing has to be recomputed or guessed. It only has
to be sent again.

## How Replay Walks the Flow Forward

`OnceOnlyAction.Replay` uses that stored link. When the Inbox recognises a duplicate,
Brighter looks up the messages the original handling produced and marks them undispatched,
which puts them back in the Sweeper's path. The handler is skipped, exactly as it would be
under `Throw` or `Warn` — but its downstream effects are re-sent.

Applied to the stalled order, and with `Replay` configured on each step:

1. You re-send `PlaceOrder`.
2. Order's Inbox sees a duplicate. `PlaceOrderHandler` does **not** run. The original
   `ProcessPayment` message is marked undispatched, and the Sweeper resends it.
3. Payment's Inbox sees `ProcessPayment` as a duplicate. `ProcessPaymentHandler` does
   **not** run — the money is not taken again. The original `ShipOrder` message is marked
   undispatched, and the Sweeper resends it.
4. Shipping's Inbox has never seen `ShipOrder`. There is no duplicate, so
   `ShipOrderHandler` runs normally and books the courier.

The flow advances one step per hop, skipping the work that was already done, until it
reaches the step that never ran — which executes for the first time. That step-by-step
advance is the point of the feature.

### The same messages, not new ones

Replay re-dispatches the *stored* messages from the original handling. It does not
re-generate them, because it does not re-run the handler that would have produced them.

That distinction matters more than it first appears. Re-running `PlaceOrderHandler` an hour
later could produce a *different* `ProcessPayment` — a repriced order, a changed address, a
customer who has since been flagged. The downstream steps would then be reacting to a
message that never existed in the original flow. Replaying the stored message means the
system converges on the state the first run was heading for, rather than on a new state
assembled from whatever is true now.

It also means replay is safe with respect to the side effects the Inbox was protecting: no
handler body executes, so nothing is charged, written, or called twice.

### Every already-seen step needs Replay

> **⚠ Important**: The cascade only propagates through handlers that are themselves
> configured with `OnceOnlyAction.Replay`. One step left on `Throw` or `Warn` stops it
> dead.

This is the single most common way to configure the feature and see nothing happen. Suppose
Payment is left on the default `Throw` while Order is set to `Replay`:

1. You re-send `PlaceOrder`. Order replays `ProcessPayment`. So far so good.
2. Payment's Inbox sees the duplicate `ProcessPayment` and throws. Nothing is replayed.
3. `ShipOrder` is never re-sent. Shipping is never reached.

The flow advanced exactly one step and stalled again — one step further along than before,
and still short of the handler that actually failed. Set `Replay` on **every** handler the
flow passes back through, not just the one at the front.

A step that has never been seen needs nothing: its Inbox finds no duplicate, so its
handler runs normally whatever `OnceOnlyAction` it is configured with. It is only the
already-seen steps between the message you re-send and the failure point that need
`Replay`.

## Causation Id

For replay to resend the right messages, Brighter has to know which Outbox messages came
out of which handler invocation. That link is the **Causation Id**.

A Causation Id answers one question: *when I handled request X, which messages did I
produce?* Every message a handler deposits during a single invocation is stamped with the
same Causation Id, and so is the Inbox entry recording that the request was handled. The
two share a value, which is what lets a later duplicate find its own downstream messages.

By default the Causation Id is **the handled request's own `Id`**. Handling `PlaceOrder`
stamps the Inbox entry and the resulting `ProcessPayment` message with `PlaceOrder`'s id.
You do not have to set anything.

**How it travels.** The Causation Id rides in the pipeline's `RequestContext.Bag`, under the well-known key
`RequestContextBagNames.CausationId` (the literal string `"Brighter-CausationId"`):

- The Inbox handler runs first in the pipeline. If the Bag has no Causation Id yet, it
  puts the request's `Id` there. If a value is already present, it is left alone — that is
  the escape hatch for callers who need to control causation themselves.
- Your handler runs and deposits messages. The Outbox `Add` reads the Causation Id out of
  the same Bag and stores it against each message.
- Your handler returns. The Inbox handler writes the Inbox entry, storing the same
  Causation Id against it.

Because the Bag belongs to the pipeline's `RequestContext`, everything in one invocation
sees one value. This is also why your handler has to pass its `Context` through when it
posts — see [You Must Thread Your RequestContext](#you-must-thread-your-requestcontext).

**Causation Id is not Correlation Id.**
Brighter already carries a `CorrelationId`, used to tie a reply back to its request in
request-reply exchanges; the Causation Id is a separate value with a separate job, and
neither is derived from the other. It is likewise distinct from `JobId` and `WorkflowId`,
which are reserved for workflow orchestration.

### What Happens on a Duplicate

When the Inbox recognises a request it has already handled and the action is `Replay`:

```
Duplicate PlaceOrder arrives
        │
        ▼
┌──────────────────────────────────────────────┐
│ Inbox handler                                │
│                                              │
│   inbox.Exists(id, contextKey)  ──►  true    │
│   onceOnlyAction == Replay      ──►  yes     │
│         │                                    │
│         ▼                                    │
│   inbox.GetCausationId(id, contextKey)       │
│         │  ──► the Causation Id stored when  │
│         │      PlaceOrder was first handled  │
│         ▼                                    │
│   outbox.ReplayCausation(causationId)        │
│         │  ──► clears the dispatched state   │
│         │      of every message stored under │
│         │      that Causation Id             │
│         ▼                                    │
│   return  ──►  PlaceOrderHandler never runs  │
└──────────────────────────────────────────────┘
        │
        ▼   later, on the Sweeper's next interval
┌──────────────────────────────────────────────┐
│ Outbox Sweeper                               │
│                                              │
│   finds the now-undispatched ProcessPayment  │
│   message and dispatches it to the broker    │
└──────────────────────────────────────────────┘
```

Step by step:

1. **The Inbox finds a duplicate.** `Exists` returns `true` for this request id and
   context key.
2. **The action is `Replay`**, so instead of throwing or logging a warning, the handler
   takes the replay path.
3. **The Inbox is asked for the Causation Id** it stored when the request was originally
   handled.
4. **The Outbox replays that causation.** Every message stored under that Causation Id has
   its dispatched state cleared, which makes it outstanding again — the same state a
   freshly deposited message is in.
5. **The Inbox handler returns without calling your handler.** No business logic runs, so
   nothing is charged, written, or called twice.
6. **The Sweeper resends.** On its next pass it sees the outstanding messages and
   dispatches them to the broker, exactly as it would for a newly deposited message.

Note where step 6 sits. Replay does not send anything itself — it only changes rows in the
Outbox — so the messages go out on the **Sweeper's next interval**, not the moment the
duplicate arrives. A cascade across three steps therefore takes at least three Sweeper
intervals to walk the flow forward. Size the interval accordingly if recovery time
matters, and see [Before You Enable It](#before-you-enable-it) for why a Sweeper is not
optional here.

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
[Inbox Configuration](/contents/BrighterBasicConfiguration.md#inbox) for the full set.

`InboxScope.Commands` is the usual scope here. Replay resends a handler invocation's
downstream messages, and it is commands — one request, one handler — where that maps
cleanly; an event delivered to several handlers is a case worth understanding before you
opt into it, covered under [Limitations](#limitations).

A global setting still applies per handler, so a handler carrying its own `[UseInbox]`
attribute keeps that attribute's `onceOnlyAction`, and a handler marked
`[NoGlobalInbox]` opts out entirely. Setting `Replay` globally is therefore the
straightforward way to satisfy the
[every-step requirement](#every-already-seen-step-needs-replay) — it covers the whole
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
covered under [When Replay Does Not Fire](#when-replay-does-not-fire).

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
  `UseInboxHandler Duplicate Replay` — see [Observability](#observability).

If replay appears to do nothing, check the causation column on the Outbox rows first. A
column full of nulls means the context was not threaded; that is the fastest way to
confirm this particular fault rather than one of the
[other reasons replay stays quiet](#when-replay-does-not-fire).

## Before You Enable It

Replay depends on six things being true at once. Most of them fail quietly — only the two
marked as startup errors stop your host — so it is worth walking the list before you set
`OnceOnlyAction.Replay` anywhere.

| Requirement | How to satisfy it | What happens if you don't |
|---|---|---|
| The Inbox implements `IAmACausationTrackingInbox` | Every Brighter-maintained Inbox already does. A hand-written Inbox must implement it — see [Implementing Causation Tracking in Your Own Store](/contents/CausationTrackingStores.md) | Startup **error** from [pipeline validation](/contents/PipelineValidation.md): replay has no way to look up the original Causation Id |
| The Outbox implements `IAmACausationTrackingOutbox` | Same — every Brighter-maintained Outbox does | Startup **error**. If there is no Outbox at all, you get a **warning** instead: the duplicate is skipped and nothing is resent |
| The store schema carries the causation column | Run [box provisioning](/contents/BoxProvisioning.md) to bring the schema up to date; on DynamoDB, create the [Causation index](/contents/DynamoOutbox.md#replay-support-the-causation-index). See [Store Support](#store-support) for what each store needs | Startup **warning** only — the host starts. At runtime the store reports no causation support, duplicates are skipped, and nothing is resent |
| An Outbox Sweeper is running | Register one with `.UseOutboxSweeper(...)` — see below | The messages are marked undispatched and stay that way. Nothing reaches the broker |
| Handlers pass their `Context` when posting | `Context as RequestContext` on every `Post`/`DepositPost` — see [You Must Thread Your RequestContext](#you-must-thread-your-requestcontext) | Messages are stored with a `null` Causation Id. Replay finds nothing to resend, silently |
| Every already-seen step is set to `Replay` | The attribute on each handler, or `actionOnExists` globally — see [Turning It On](#turning-it-on) | The cascade stops at the first step still on `Throw` or `Warn` — see [Every already-seen step needs Replay](#every-already-seen-step-needs-replay) |

Note how much of this is warnings rather than errors. A schema that was never migrated
does not fail startup; it produces a log line at boot and then a system that looks healthy
and never replays anything. [When Replay Does Not Fire](#when-replay-does-not-fire) lists
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
[Upgrading Without Migrating](#upgrading-without-migrating).

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

```
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
[You Must Thread Your RequestContext](#you-must-thread-your-requestcontext).

**A custom request context factory.** If you have supplied an `IAmARequestContextFactory`
that returns something other than a `RequestContext`, the Inbox handler cannot use the
pipeline's context at all and falls back to a throwaway whose Bag never reaches the Outbox.
This one does leave a trace: a single warning, logged once per process the first time a
Replay pipeline hits it.

```
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
[Every already-seen step needs Replay](#every-already-seen-step-needs-replay).

**The step has no Outbox at all.** A handler that deposits nothing has nothing to replay,
so the duplicate is simply skipped. That is correct behaviour for a terminal step, and
validation says so with a warning rather than an error.

**No Sweeper is running.** Replay did its job — the messages are outstanding in the
Outbox — but nothing is dispatching them. Check whether the rows have had their dispatched
state cleared: if they have, the fault is downstream of replay, in the Sweeper. See
[Replay only ever sends through the Sweeper](#replay-only-ever-sends-through-the-sweeper).

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

The replay only clears dispatched state; the [Sweeper](#replay-only-ever-sends-through-the-sweeper)
does the sending, later, on its own schedule. It starts its own `Activity` for each sweep,
and a single sweep may carry messages from several unrelated replays, so there is no parent
span to attach to — a link would have to be one-to-many and would tie the dispatch to
whichever replay happened to be first. The Causation Id tag is the join instead: it appears
on the replay event, and it is stored on every message the sweep picks up.

For the same reason, the elapsed time between the replay event and the messages arriving is
Sweeper latency, not handler latency. If a cascade looks slow in your traces, look at
`TimerInterval` and `MinimumMessageAge` before you look at your handlers.

## Limitations

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
[Migrating does not backfill](#migrating-does-not-backfill).

### Replaying a large causation is one write

Clearing the dispatched state of every message under a Causation Id is a single statement,
and its cost scales with how many messages that handler deposited. The V8 Outbox migration
indexes the causation column precisely so this stays cheap; on DynamoDB the same job is done
by the Causation index. A handler that deposits a handful of messages per invocation — the
normal case — is not something you need to think about.

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

## Why It Works This Way

Three of replay's design decisions shape how you use it, and each rules out an approach that
looks simpler at first.

**Why not just re-run the handler?** Because that is the one thing the Inbox exists to
prevent. If re-running were safe you would not need an Inbox in front of the handler at all,
and the flows that need replay are exactly the ones where a second run charges a card or
ships a parcel twice. There is a subtler reason too: a handler re-run an hour later reads
the state of an hour later, so it could legitimately produce *different* messages from the
ones the original run produced — see
[The same messages, not new ones](#the-same-messages-not-new-ones).

**Why a new id rather than one Brighter already carries?** Both obvious candidates mean
something else. `CorrelationId` ties a reply to its request in request-reply exchanges, and
overloading it would leave two unrelated relationships sharing one field. `JobId` identifies
a whole workflow instance, so replaying by it would resend every message from every step of
that job rather than the messages from the one step you are recovering. The Causation Id is
narrower than either on purpose: it names a single handler invocation and the messages that
came out of it.

**Why the Sweeper rather than sending immediately?** Because the Outbox already has a
component whose entire job is dispatching outstanding messages, and that reduces replay to a
change of state — mark these rows outstanding — with no new dispatch path to build, secure,
or reason about. The replayed messages then inherit everything the Sweeper already provides:
the same batching, the same retry behaviour, the same delivery guarantees as any other
outstanding message. You pay for it in latency, one sweep interval per hop, which is the
trade the [Sweeper section](#replay-only-ever-sends-through-the-sweeper) asks you to size for.

The full rationale, including the alternatives that were considered and rejected, is in
[ADR 0057 — Replay Outbox Messages on Inbox Duplicate Detection](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0057-replay-outbox-on-inbox-duplicate.md).
Note the filename: several ADRs share the number 0057.

## Further Reading

- [Inbox Support](/contents/BrighterInboxSupport.md) — configuring an Inbox, and the other
  `OnceOnlyAction` values
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Outbox, the Sweeper, and
  delivery guarantees
- [Outbox Pattern](/contents/OutboxPattern.md) — why the Outbox exists at all
- [Box Provisioning](/contents/BoxProvisioning.md) — bringing a store schema up to the
  version replay needs
- [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md) — what a migration
  run does, and what to check afterwards
- [Pipeline Validation](/contents/PipelineValidation.md) — the startup checks, including the
  replay rule
- [Implementing Causation Tracking in Your Own Store](/contents/CausationTrackingStores.md) —
  the role interfaces to implement if you write your own Inbox or Outbox
- [DynamoDb Outbox](/contents/DynamoOutbox.md) — including the Causation index replay needs
- [Telemetry](/contents/Telemetry.md) — the tracing the span events attach to
- [Glossary](/contents/Glossary.md) — Causation Id, Replay, Inbox, Outbox, Sweeper
- [ADR 0057 — Replay Outbox Messages on Inbox Duplicate Detection](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0057-replay-outbox-on-inbox-duplicate.md)
  — the design rationale
