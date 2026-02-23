# Error Handling Options

Brighter's [message pump](/contents/HowServiceActivatorWorks.md) uses `Subscription` properties to control how it handles errors. These properties configure requeue behavior, Dead Letter Queue (DLQ) routing, and pump termination thresholds.

This document covers the configuration side of error handling. For an overview of strategies and when to use each one, see [Error Handling](/contents/HandlerFailure.md).

## Subscription Properties

### RequeueCount

| | |
|---|---|
| **Type** | `int` |
| **Default** | `-1` (unlimited) |

The maximum number of times a message can be requeued (via `DeferMessageAction`) before the pump treats it as a [poison message](/contents/BasicConcepts.md). When the count is exceeded, the message is rejected — routed to the DLQ if one is configured, or acknowledged and discarded otherwise.

Set this to a positive integer to prevent infinite retry loops. A value of `-1` disables the limit, allowing unlimited requeues. A value of `0` means the message is rejected on the first `DeferMessageAction` without any requeue.

### RequeueDelay

| | |
|---|---|
| **Type** | `TimeSpan` |
| **Default** | `TimeSpan.Zero` |

How long a requeued message waits before becoming visible on the channel again. How the delay is implemented depends on the transport — some support native delay, others rely on a configured scheduler.

### UnacceptableMessageLimit

| | |
|---|---|
| **Type** | `int` |
| **Default** | `0` (disabled) |

The number of unacceptable messages the pump tolerates before shutting down. An unacceptable message is one that triggers a `RejectMessageAction`, `DontAckAction`, `InvalidMessageAction`, or any unhandled exception. `DeferMessageAction` does not count.

Set this to a positive integer to protect against mass message loss during systemic failures — for example, a database outage that causes every message to fail and be rejected to the DLQ. A value of `0` or less disables the limit.

### UnacceptableMessageLimitWindow

| | |
|---|---|
| **Type** | `TimeSpan?` |
| **Default** | `null` (count never resets) |

The time window over which unacceptable messages are counted. The count resets at the end of each window.

Without a window, the count accumulates over the pump's entire lifetime. Even a low rate of errors will eventually reach the limit and shut down the pump. Setting a window means only bursts of errors during a single window trigger a shutdown.

### DontAckDelay

| | |
|---|---|
| **Type** | `TimeSpan` |
| **Default** | `TimeSpan.FromSeconds(1)` |

The delay the pump waits after a `DontAckAction` before processing the next message. This prevents tight-loop CPU burn when a message is repeatedly not acknowledged.

This property is set on the `MessagePump` directly, not on the `Subscription`.

### Configuration Example

```csharp
var subscription = new RmqSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order.queue"),
    routingKey: new RoutingKey("order.place"),
    requeueCount: 3,                                           // Requeue up to 3 times
    requeueDelay: TimeSpan.FromSeconds(5),                     // 5-second delay between requeues
    unacceptableMessageLimit: 10,                              // Stop pump after 10 errors
    unacceptableMessageLimitWindow: TimeSpan.FromMinutes(5),   // Reset count every 5 minutes
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

## Dead Letter Queue Configuration

### How DLQ Routing Works

When a message is rejected (via `RejectMessageAction`, or when `RequeueCount` is exceeded), the message pump decides where to send it:

1. **Delivery errors** (`RejectMessageAction`, requeue count exceeded): routed to the `DeadLetterRoutingKey` channel if configured, or acknowledged and discarded otherwise.
2. **Invalid messages** (`InvalidMessageAction`): routed to the `InvalidMessageRoutingKey` channel if configured, falling back to `DeadLetterRoutingKey`, or acknowledged and discarded if neither is set.

Subscriptions that support Brighter-managed DLQ implement `IUseBrighterDeadLetterSupport`. Those that also support separate invalid message routing implement `IUseBrighterInvalidMessageSupport`.

### Native vs Brighter-Managed DLQ

Some transports provide native DLQ support. For transports without native DLQ, Brighter manages the routing by producing rejected messages to a separate channel.

| Transport | DLQ Type | Invalid Message | Notes |
|-----------|----------|-----------------|-------|
| RabbitMQ | Native (Dead Letter Exchange) | Brighter-managed | Uses DLX with routing key |
| AWS SQS | Brighter-managed | Brighter-managed | Direct send to DLQ queue |
| Azure Service Bus | Native | Brighter-managed | Built-in DLQ per subscription |
| Kafka | Brighter-managed | Brighter-managed | Lazy producer to DLQ topic |
| Redis | Brighter-managed | Brighter-managed | No native DLQ (`BLPOP` is destructive) |
| MsSql | Brighter-managed | Brighter-managed | Same table, different topic value |
| PostgreSQL | Brighter-managed | Brighter-managed | Visibility timeout model |
| MQTT | Brighter-managed | Brighter-managed | Fire-and-forget, no ack concept |

### Configuring DLQ on a Subscription

Set `deadLetterRoutingKey` and optionally `invalidMessageRoutingKey` on your subscription. Both are constructor parameters available on all transport-specific subscription types.

**Kafka subscription with DLQ and invalid message routing:**

```csharp
var subscription = new KafkaSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order-consumer"),
    routingKey: new RoutingKey("orders"),
    groupId: "order-service",
    deadLetterRoutingKey: new RoutingKey("orders.dlq"),
    invalidMessageRoutingKey: new RoutingKey("orders.invalid"),
    requeueCount: 3,
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

**SQS subscription with DLQ:**

```csharp
var subscription = new SqsSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order-queue"),
    channelType: ChannelType.PubSub,
    routingKey: new RoutingKey("orders"),
    deadLetterRoutingKey: new RoutingKey("orders-dlq"),
    requeueCount: 3,
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

### DLQ Naming Conventions

Brighter provides `DeadLetterNamingConvention` and `InvalidMessageNamingConvention` helper classes to generate consistent channel names from a source topic. The default templates are `{0}.dlq` and `{0}.invalid` respectively.

```csharp
var dlqNaming = new DeadLetterNamingConvention();              // "{0}.dlq"
var invalidNaming = new InvalidMessageNamingConvention();      // "{0}.invalid"

var sourceTopic = new RoutingKey("orders");
var dlqKey = dlqNaming.MakeChannelName(sourceTopic);           // "orders.dlq"
var invalidKey = invalidNaming.MakeChannelName(sourceTopic);   // "orders.invalid"

// Use a custom template
var customDlq = new DeadLetterNamingConvention("dead-letter-{0}");
customDlq.MakeChannelName(sourceTopic);                        // "dead-letter-orders"
```

### Message Enrichment

When a message is routed to a DLQ or invalid message channel, Brighter adds metadata to the message header bag:

| Header | Description |
|--------|-------------|
| `OriginalTopic` | The topic the message was originally consumed from |
| `RejectionReason` | `DeliveryError` (handler failure) or `Unacceptable` (deserialization failure) |
| `RejectionTimestamp` | UTC ISO-8601 timestamp of when the message was rejected |
| `OriginalMessageType` | The original message type before rejection |
| `RejectionMessage` | Description of the rejection reason (if provided) |

This metadata helps operators investigate why a message was rejected and trace it back to its source.

## Common Configurations

### Retry 3 Times Then DLQ

A common pattern: requeue the message up to 3 times with a delay, then route to the DLQ. Combine with `RejectMessageOnErrorAttribute` on the handler to ensure unhandled exceptions also go to the DLQ.

```csharp
var subscription = new KafkaSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order-consumer"),
    routingKey: new RoutingKey("orders"),
    groupId: "order-service",
    requeueCount: 3,                                           // Allow 3 requeues
    requeueDelay: TimeSpan.FromSeconds(10),                    // 10-second delay between requeues
    deadLetterRoutingKey: new RoutingKey("orders.dlq"),        // Route to DLQ after exhausting requeues
    invalidMessageRoutingKey: new RoutingKey("orders.invalid"),
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

With this configuration, a handler that throws `DeferMessageAction` will requeue the message up to 3 times. On the 4th failure, the message is rejected and routed to `orders.dlq`.

### Stop Pump After 10 Errors in 5 Minutes

Protect against mass message loss during systemic failures by limiting the number of unacceptable messages within a time window.

```csharp
var subscription = new SqsSubscription<PlaceOrder>(
    subscriptionName: new SubscriptionName("Order Processor"),
    channelName: new ChannelName("order-queue"),
    channelType: ChannelType.PubSub,
    routingKey: new RoutingKey("orders"),
    unacceptableMessageLimit: 10,                              // Stop after 10 errors
    unacceptableMessageLimitWindow: TimeSpan.FromMinutes(5),   // Reset count every 5 minutes
    deadLetterRoutingKey: new RoutingKey("orders-dlq"),
    messagePumpType: MessagePumpType.Reactor,
    makeChannels: OnMissingChannel.Create
);
```

With this configuration, if 10 or more messages are rejected, [nacked](/contents/BasicConcepts.md#nack-negative-acknowledgment), or fail with unhandled exceptions within a 5-minute window, the pump shuts down. This gives operators time to investigate before more messages are lost. If errors are spread out — fewer than 10 per window — the pump continues running.

## Further Reading

- [Error Handling](/contents/HandlerFailure.md) — strategies for handling errors in handlers
- [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) — configure Polly resilience pipelines
- [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md) — message pump internals
