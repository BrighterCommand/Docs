---
description: "A poison message is one that fails every time you process it, and this guide routes it to a dead letter queue instead of letting it block the channel."
layout:
  description:
    visible: false
---
# Handle a Poison Message and Route It to a Dead Letter Queue

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Error Handling](/contents/HandlerFailure.md), [Error Handling Options](/contents/ErrorHandlingOptions.md)

A poison message is one that fails every time you process it, and this guide routes it to a dead
letter queue instead of letting it block the channel.

[Error Handling](/contents/HandlerFailure.md) explains what each error action *means* and
[Error Handling Options](/contents/ErrorHandlingOptions.md) lists the settings that control them.
This guide is the route between them: the order to do things in, and the two decisions that are
easy to get wrong.

## Step 1: Confirm You Have a Poison Message

A poison message fails *deterministically*. A message that fails once and succeeds on retry is a
transient failure, and requeue already handles it — sending it to a dead letter queue would lose
work that would have completed.

Look for the same message id failing repeatedly:

```text
warn: Paramore.Brighter.ServiceActivator.Reactor[0]
      MessagePump: Failed to process message 019308f1-... from order.queue, requeueing
warn: Paramore.Brighter.ServiceActivator.Reactor[0]
      MessagePump: Failed to process message 019308f1-... from order.queue, requeueing
warn: Paramore.Brighter.ServiceActivator.Reactor[0]
      MessagePump: Requeue count exceeded for message 019308f1-...
```

The same id, the same failure, and a requeue count that runs out. If the ids differ each time you
have a failing *handler*, not a poison *message*, and the rest of this guide will hide the problem
rather than solve it.

Two other symptoms point here:

- **The channel stops moving.** One message that never acknowledges and never leaves blocks
  everything behind it on transports that preserve order.
- **`UnacceptableMessageLimit` stops the pump.** That limit exists to stop a pump that is doing
  nothing but failing; see [Unacceptable Message Limit](/contents/HandlerFailure.md#unacceptable-message-limit).

## Step 2: Choose Between Requeue, Reject and Don't Acknowledge

Brighter gives you four actions, each an exception you throw from a handler or mapper. What each one
means is on [Error Handling](/contents/HandlerFailure.md); what matters here is which one to pick.

| You want to | Throw | Namespace |
|---|---|---|
| Try again later, a bounded number of times | `DeferMessageAction` | `Paramore.Brighter.Actions` |
| Give up and send it to the dead letter queue | `RejectMessageAction` | `Paramore.Brighter.Actions` |
| Leave it on the channel for someone else | `DontAckAction` | `Paramore.Brighter.Actions` |
| Reject a message you could not even deserialise | `InvalidMessageAction` | `Paramore.Brighter.Actions` |

**The one thing you cannot infer is what a nack does on your transport.** `DontAckAction` returns
the message to the channel on some transports and *discards it* on others. On **MSSQL, Redis and
MQTT** the read has already consumed the message, so a nack loses it — on those three
`DeferMessageAction` is the only safe choice, because it re-publishes the message rather than
trusting the transport to redeliver. The full behaviour, transport by transport, is at
[Transport Nack Behavior](/contents/HandlerFailure.md#transport-nack-behavior); read it before
choosing `DontAckAction`.

**`DontAckDelay` is not yours to set.** The pump waits one second after a `DontAckAction` before
it takes the next message, and that value lives on the `MessagePump` rather than on your
subscription — nothing copies it across, so it stays at its default. If you need to control the
pace of retries, use `DeferMessageAction` and `requeueDelay`, which you *can* configure. See
[DontAckDelay](/contents/ErrorHandlingOptions.md#dontackdelay).

## Step 3: Set a Requeue Count and a Dead Letter Routing Key

A dead letter queue only receives anything if two settings agree: a bounded `requeueCount`, so
retries eventually stop, and a `deadLetterRoutingKey`, so there is somewhere to put the message
when they do.

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

var subscription = new KafkaSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order-consumer"),
    routingKey: new RoutingKey("order.place"),
    groupId: "order-service",
    requeueCount: 3,                                  // bounded: -1 means retry forever
    requeueDelay: TimeSpan.FromSeconds(5),
    deadLetterRoutingKey: new RoutingKey("order.place.dlq"),
    invalidMessageRoutingKey: new RoutingKey("order.place.invalid"),
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

**`requeueCount` defaults to `-1`, which means retry forever.** A poison message under that default
never reaches a dead letter queue no matter how the rest is configured — it simply cycles. Setting
it is the step people miss.

Not every transport carries a Brighter-managed dead letter queue; several have a native one you
configure at the broker instead, and for those `deadLetterRoutingKey` is not the mechanism. Which
is which is at
[Native vs Brighter-Managed DLQ](/contents/ErrorHandlingOptions.md#native-vs-brighter-managed-dlq).

If you want conventional names rather than literal ones, `DeadLetterNamingConvention` and
`InvalidMessageNamingConvention` will build them for you — `{0}.dlq` and `{0}.invalid` by default.
**They are helpers you call, not a policy Brighter applies**: you pass the result to
`deadLetterRoutingKey` yourself. See
[DLQ Naming Conventions](/contents/ErrorHandlingOptions.md#dlq-naming-conventions).

## Step 4: Add a Backstop Attribute

Steps 2 and 3 only help if something actually throws the action. A handler that lets an unexpected
exception escape gets the pump's default treatment, which is not what you configured.

A backstop attribute closes that gap: it wraps the handler, catches anything unhandled, and turns
it into the action you chose.

```csharp
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.Reject.Attributes;

public class PlaceOrderHandler : RequestHandlerAsync<PlaceOrder>
{
    [RejectMessageOnErrorAsync(step: 1)]
    public override async Task<PlaceOrder> HandleAsync(
        PlaceOrder command, CancellationToken cancellationToken = default)
    {
        // any exception escaping here becomes a RejectMessageAction
        return await base.HandleAsync(command, cancellationToken);
    }
}
```

There are **six** backstop attributes, not three — each action has a sync and an async twin, and
they live in three namespaces:

| Action | Sync (Reactor) | Async (Proactor) | Namespace |
|---|---|---|---|
| Reject | `RejectMessageOnErrorAttribute` | `RejectMessageOnErrorAsyncAttribute` | `Paramore.Brighter.Reject.Attributes` |
| Defer | `DeferMessageOnErrorAttribute` | `DeferMessageOnErrorAsyncAttribute` | `Paramore.Brighter.Defer.Attributes` |
| Don't acknowledge | `DontAckOnErrorAttribute` | `DontAckOnErrorAsyncAttribute` | `Paramore.Brighter.DontAck.Attributes` |

**Picking the wrong half is a `ConfigurationException` at pipeline build time**, not a runtime
surprise — a sync handler must carry sync attributes and an async handler async ones. See
[Pipeline Validation](/contents/PipelineValidation.md).

### Which Half You Need Depends on Your Subscription, Not on the Base Class

`Subscription<T>` defaults to `Proactor`, so the general answer is "async". **Your transport's
subscription may override that**, and two of them do:

| `Subscription<T>` | Default `messagePumpType` | Attribute family |
|---|---|---|
| `SqsSubscription`, `AzureServiceBusSubscription`, `MqttSubscription`, `MsSqlSubscription`, `RedisSubscription`, `RmqSubscription` (RMQ.Async) | `Proactor` | async |
| `KafkaSubscription`, `RmqSubscription` (RMQ.Sync) | `Reactor` | **sync** |
| `PostgresSubscription`, `GcpPubSubSubscription`, `RocketMqSubscription` | `Unknown` | you must choose |

`RmqSubscription` appears twice because it is two types with one name: the one in
`Paramore.Brighter.MessagingGateway.RMQ.Async` defaults to `Proactor` and the one in
`Paramore.Brighter.MessagingGateway.RMQ.Sync` defaults to `Reactor`. Which you get is decided by
the package you referenced, not by anything in your code.

**`Unknown` is not a third pump — it throws.** Constructing one of those three without naming a
pump gives you:

```text
Paramore.Brighter.ConfigurationException: You must set a message pump type: use Reactor for
sync pipelines; use Proactor for async pipelines
```

So on Kafka, a handler written async with async attributes will not run under the default
subscription, because that subscription's default is a Reactor. Set `messagePumpType` explicitly
whichever transport you are on — it is one argument, and it makes the attribute choice follow from
something you wrote rather than from a default you inherited.

## Step 5: Verify the Message Reaches the Dead Letter Queue

Do not assume the routing worked because nothing threw. Publish a message you know will fail, let
the requeue count run out, and read the dead letter channel.

```csharp
using System;
using System.Threading.Tasks;
using Paramore.Brighter;

// a payload the handler is guaranteed to reject
await commandProcessor.PostAsync(new PlaceOrder { OrderId = "poison", Quantity = -1 });
```

Then wait for `requeueCount × requeueDelay` to elapse — with the values in step 3 that is fifteen
seconds — before looking. Reading sooner tells you nothing, because the message is still in its
retry cycle.

**Stop the consumer before you read the broker's counters, and wait for its connection to be
gone.** A stopped consumer whose connection has not yet been reaped still holds its subscription,
so the broker reports `0` messages waiting — which is exactly what a *lost* message looks like.
Confirm the connection has dropped, then read the depth of the dead letter channel. A count of one
is the result you want; a count of zero this early is almost always the connection, not the
message.

Reading the message back is the positive confirmation:

```csharp
using System;
using Paramore.Brighter;

var dlqMessage = deadLetterConsumer.Receive(TimeSpan.FromSeconds(5))[0];
Console.WriteLine(dlqMessage.Header.MessageId);   // the id you saw failing in step 1
```

Matching that id against the one from step 1 is what proves the route, rather than proving that
*some* message arrived somewhere.

## Step 6: Read the Enrichment Headers

Brighter adds metadata to a message when it routes it, so a message in a dead letter queue carries
the reason it is there — the topic it came from, whether it failed in a handler or failed to
deserialise, and when. You do not need to reconstruct any of that from logs.

```csharp
using System;
using Paramore.Brighter;

if (dlqMessage.Header.Bag.TryGetValue("RejectionReason", out var reason))
{
    Console.WriteLine($"rejected because: {reason}");
}
```

The full set of headers, with what each one holds, is at
[Message Enrichment](/contents/ErrorHandlingOptions.md#message-enrichment). `RejectionReason` is
the one that answers the first question you will have: `DeliveryError` means a handler failed,
`Unacceptable` means the message never deserialised — and those two lead to different fixes.

## Step 7: Decide Whether to Replay or Discard

A dead letter queue is a holding area, not an outcome. Every message in it needs a decision.

**Discard** when the message is malformed, superseded, or the work it asked for no longer makes
sense. Log the id and the enrichment headers first — a discarded message is unrecoverable, and the
headers are the only record that it existed.

**Replay** when the failure was in your code and you have fixed it. Replaying means publishing the
message back to its original topic, which you can read from the `OriginalTopic` header:

```csharp
using System;
using System.Threading.Tasks;
using Paramore.Brighter;

var originalTopic = dlqMessage.Header.Bag["OriginalTopic"].ToString();
var replayed = new Message(
    new MessageHeader(dlqMessage.Header.MessageId, new RoutingKey(originalTopic!), MessageType.MT_COMMAND),
    dlqMessage.Body);

await producer.SendAsync(replayed);
```

**Replaying a message your system already partly processed will double the effects that succeeded
before the failure.** If the handler is not idempotent, fix that before replaying anything in bulk.
Brighter's [Replay On Seen](/contents/ReplayOnSeen.md) is designed for exactly this problem —
**note that it ships after 10.7.0 and is in no release you can install today**, so on V10 the
protection has to come from an [Inbox](/contents/BrighterInboxSupport.md) or from handlers you have
made idempotent yourself.

## Poison Message Handling on Your Transport

Two things vary by transport, and both change what the steps above do:

- **What a nack does.** On some transports the message returns to the channel; on MSSQL, Redis and
  MQTT it is discarded. The table is at
  [Transport Nack Behavior](/contents/HandlerFailure.md#transport-nack-behavior).
- **Whether the dead letter queue is Brighter's or the broker's.** Where the broker owns it,
  `deadLetterRoutingKey` is not how you configure it. The table is at
  [Native vs Brighter-Managed DLQ](/contents/ErrorHandlingOptions.md#native-vs-brighter-managed-dlq).

Both tables are maintained on those pages rather than repeated here, so there is one copy to keep
right.

## Further Reading

- [Error Handling](/contents/HandlerFailure.md) — what each error action means, and the backstop
  attributes in full
- [Error Handling Options](/contents/ErrorHandlingOptions.md) — every subscription property that
  affects retries and dead lettering
- [Pipeline Validation](/contents/PipelineValidation.md) — why a sync attribute on an async handler
  fails at build time
- [Replay On Seen](/contents/ReplayOnSeen.md) — de-duplicating replayed messages, after 10.7.0
- [Brighter Inbox Support](/contents/BrighterInboxSupport.md) — idempotency on V10
