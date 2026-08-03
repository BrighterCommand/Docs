# Implementing Causation Tracking in Your Own Store

[Replay On Seen](/contents/ReplayOnSeen.md) needs two things from your storage: an Inbox
that can hand back the [Causation Id](/contents/ReplayOnSeen.md#causation-id) it recorded
when a request was first handled, and an Outbox that can find every message stored under
that Causation Id and make it outstanding again. A **Causation Id** is the value shared by
an Inbox entry and every Outbox message the handler produced during that invocation; it
defaults to the handled request's own `Id`. Brighter asks for both capabilities through a
pair of optional role interfaces, and this page covers what your implementation has to do.

> This page is for people **writing** an Inbox or Outbox — a backend Brighter does not
> ship, or a wrapper of your own. If you are using a Brighter-maintained store, it already
> implements everything below; see
> [Store Support](/contents/ReplayOnSeen.md#store-support) instead.

Causation tracking is an **optional role interface** on each box, separate from the core
Inbox and Outbox interfaces. A store that does not implement it keeps working exactly as
before — it simply never participates in replay.

## The two interfaces

Both live in the `Paramore.Brighter` namespace.

`IAmACausationTrackingInbox` has three jobs:

| Member | What your implementation must do |
|---|---|
| `SupportsCausationTracking()` / `…Async()` | Report whether the **live** store can hold a Causation Id right now |
| `GetCausationId(id, contextKey, …)` / `…Async()` | Return the Causation Id stored against an entry, or `null` if there is none |
| *(your `Add`)* | Read the Causation Id out of the request context and store it with the entry |

`IAmACausationTrackingOutbox` mirrors it:

| Member | What your implementation must do |
|---|---|
| `SupportsCausationTracking()` / `…Async()` | As above |
| `ReplayCausation(causationId, …)` / `…Async()` | Clear the dispatched state of every message stored under that Causation Id, so the Sweeper resends them. Return `true` if you did it, `false` if it was a no-op |
| *(your `Add`)* | Read the Causation Id out of the request context and store it with the message |

Note that storing the Causation Id is **not** on either interface. It happens inside your
`Add`, which already receives the `RequestContext` it needs:

```csharp
using Paramore.Brighter;

private static string? ReadCausationId(RequestContext? requestContext)
    => requestContext?.Bag.TryGetValue(RequestContextBagNames.CausationId, out var value) == true
        ? value as string
        : null;
```

## Three rules that are easy to get wrong

**`SupportsCausationTracking()` must report the live state, not your intent.** It is not
"does this class implement the interface" — the class obviously does, or the method would not
be there. It is "can the store *this instance is talking to* hold a Causation Id today". For a
schemaless store that is genuinely always `true`. For anything with a schema, go and look:
Brighter's relational stores query for the column, and its DynamoDB Outbox calls
`DescribeTable` for the index. Returning an optimistic `true` makes
[pipeline validation](/contents/PipelineValidation.md) pass and then fails at runtime, which
is precisely the outcome the method exists to prevent.

**Never throw for an unsupported store — degrade.** `GetCausationId` returns `null` and
`ReplayCausation` returns `false`. A duplicate arriving at an un-migrated store must not
unwind the consumer pipeline with a schema error, and the `false` return is what lets
Brighter record a
[`Replay Skipped` event](/contents/ReplayOnSeen.md#replay-versus-replay-skipped) rather than
claiming a replay that never happened.

**Gate your own write path on the same answer.** If your `Add` unconditionally writes a
Causation Id, an un-migrated store starts failing deposits the moment someone upgrades. Ask
the same probe, and fall back to your original write when it says no. If the probe is
expensive, memoize it — and read
[Upgrading Without Migrating](/contents/ReplayOnSeen.md#upgrading-without-migrating) for the
restart consequence that memoizing carries.

## A skeleton

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public class MyOutbox : IAmAnOutboxSync<Message, MyTransaction>, IAmACausationTrackingOutbox
{
    private bool? _causationSupported;

    // ... the core Outbox members: Get, MarkDispatched, OutstandingMessages, Delete ...

    public void Add(Message message, RequestContext requestContext, int outBoxTimeout = -1,
        IAmABoxTransactionProvider<MyTransaction>? transactionProvider = null)
    {
        var causationId = ReadCausationId(requestContext);

        // Gate the write on the same probe: an un-migrated store keeps its original shape
        if (SupportsCausationTracking())
            StoreWithCausation(message, causationId);
        else
            Store(message);
    }

    public bool SupportsCausationTracking()
        => _causationSupported ??= CausationColumnExists();   // a live check, memoized

    public Task<bool> SupportsCausationTrackingAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(SupportsCausationTracking());

    public bool ReplayCausation(string causationId, RequestContext? requestContext,
        Dictionary<string, object>? args = null)
    {
        // A no-op, not an exception, when the store cannot support it
        if (!SupportsCausationTracking())
            return false;

        foreach (var message in FindByCausation(causationId))
            ClearDispatchedState(message);   // the Sweeper takes it from here

        return true;
    }

    public Task<bool> ReplayCausationAsync(string causationId, RequestContext? requestContext,
        Dictionary<string, object>? args = null, CancellationToken cancellationToken = default)
        => Task.FromResult(ReplayCausation(causationId, requestContext, args));
}
```

The Inbox side is the same shape: probe, store the Causation Id in `Add` when the probe says
you can, and return it from `GetCausationId` — or `null`.

## Registration

Nothing extra to do. `AddProducers` checks whether your Outbox implements
`IAmACausationTrackingOutbox` and registers the same instance under that interface as well.
The Inbox handler takes it as an optional constructor dependency, so an Outbox that does not
implement the role resolves to `null` and the handler degrades to a plain skip.

## Further Reading

- [Replay On Seen](/contents/ReplayOnSeen.md) — what replay does, and how the Causation Id
  links an Inbox entry to the messages it produced
- [Store Support](/contents/ReplayOnSeen.md#store-support) — what the Brighter-maintained
  stores already do, and what each needs before replay works
- [Pipeline Validation](/contents/PipelineValidation.md) — the startup checks your
  `SupportsCausationTracking()` answer feeds
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Outbox, the Sweeper, and
  delivery guarantees
- [Inbox Support](/contents/BrighterInboxSupport.md) — configuring an Inbox, and the other
  `OnceOnlyAction` values
- [ADR 0057 — Replay Outbox Messages on Inbox Duplicate Detection](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0057-replay-outbox-on-inbox-duplicate.md)
  — the design rationale
