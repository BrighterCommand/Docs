---
description: "When your Inbox recognises a message it has already handled, it normally throws or logs a warning and stops — the work is done, so there is nothing to do."
layout:
  description:
    visible: false
---

# Replay On Seen

> **Explanation** · Applies to **Brighter V10**

> **Not in a released package yet.** Replay On Seen ships **after Brighter 10.7.0**, which is
> the current release. `OnceOnlyAction.Replay`, `SupportsCausationTracking` and the Causation Id
> plumbing this page describes are on Brighter's development branch and are in no version you
> can install today, so treat what follows as the feature as it will ship.

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
posts — see [You Must Thread Your RequestContext](/contents/TurningOnReplayOnSeen.md#you-must-thread-your-requestcontext).

**Causation Id is not Correlation Id.**
Brighter already carries a `CorrelationId`, used to tie a reply back to its request in
request-reply exchanges; the Causation Id is a separate value with a separate job, and
neither is derived from the other. It is likewise distinct from `JobId` and `WorkflowId`,
which are reserved for workflow orchestration.

### What Happens on a Duplicate

When the Inbox recognises a request it has already handled and the action is `Replay`:

```text
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
matters, and see [Before You Enable It](/contents/TurningOnReplayOnSeen.md#before-you-enable-it) for why a Sweeper is not
optional here.

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
trade the [Sweeper section](/contents/TurningOnReplayOnSeen.md#replay-only-ever-sends-through-the-sweeper) asks you to size for.

The full rationale, including the alternatives that were considered and rejected, is in
[ADR 0057 — Replay Outbox Messages on Inbox Duplicate Detection](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0057-replay-outbox-on-inbox-duplicate.md).
Note the filename: several ADRs share the number 0057.

## Further Reading

- [Turning On Replay On Seen](/contents/TurningOnReplayOnSeen.md) — the procedure, and a
  worked example
- [Replay On Seen Reference](/contents/ReplayOnSeenReference.md) — store support, when it
  does not fire, observability and limitations
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
