# Error Handling

When your handler throws an exception on the [External Bus](/contents/DispatchingARequest.md), Brighter's message pump catches it and decides what to do with the message. The default is to acknowledge the message and move on. Every other behavior — requeue, reject to a Dead Letter Queue, leave unacknowledged — requires you to opt in by throwing a specific action exception or adding middleware to your pipeline.

This guide explains each error handling strategy and helps you choose the right one.

## The Default: Always Acknowledge

**Brighter acknowledges every message, whether your handler succeeds or throws an unhandled exception.**

This is deliberate. If the message pump re-delivered a message every time processing failed, a single bad message — a [poison message](/contents/BasicConcepts.md) — could block the pump indefinitely. The handler would fail, the message would be re-delivered, the handler would fail again, and nothing else on that channel would be processed.

By acknowledging on failure, Brighter ensures the pump moves on. The assumption is that an unhandled exception represents a non-transient error: something is wrong with the message or the handler logic, and retrying the same message won't help. Operators investigate from logs and distributed traces.

This means:

- If your handler completes without throwing, the message is acknowledged (success).
- If an unhandled exception leaves the [pipeline](/contents/BuildingAPipeline.md), the message is also acknowledged and discarded.
- Errors are logged, but the message is gone.

Every other error handling behavior described in this document requires you to opt in, using either:

- **Action exceptions** — special exceptions you throw in your handler or middleware (`DeferMessageAction`, `RejectMessageAction`, `DontAckAction`, `InvalidMessageAction`). These are not bugs; they are flow control signals that tell the message pump what to do.
- **Backstop attributes** — middleware attributes on your handler method that catch unhandled exceptions and convert them to action exceptions (`RejectMessageOnErrorAttribute`, `DontAckOnErrorAttribute`).
- **Resilience middleware** — retry and circuit breaker policies that handle transient errors before they reach the pump (`UseResiliencePipelineAttribute`, `FallbackPolicyAttribute`).

The rest of this document explains each option and when to use it.

## How the Message Pump Handles Exceptions

The [message pump](/contents/HowServiceActivatorWorks.md) wraps your handler invocation in a try/catch chain. The catch order determines priority when exceptions propagate:

1. **`DeferMessageAction`** — Requeue the message with a delay.
2. **`DontAckAction`** — Leave the message unacknowledged on the channel; the transport re-delivers it.
3. **`RejectMessageAction`** — Reject the message and route it to a Dead Letter Queue (if configured).
4. **`InvalidMessageAction`** — Route the message to an invalid message channel (or fall back to the DLQ).
5. **Any other exception** — Acknowledge the message and discard it (the default described above).

All action exceptions except `DeferMessageAction` increment an internal unacceptable message counter. If that counter reaches the configured `UnacceptableMessageLimit`, the pump shuts down to prevent mass message loss. See [Unacceptable Message Limit](#unacceptable-message-limit) for details.

## Choosing an Error Handling Strategy

The right strategy depends on why processing failed and what you want to happen to the message:

- **Transient error, retry immediately:** Use `UseResiliencePipelineAttribute` with a Polly retry policy to retry within the same message pump cycle. If all retries fail, the exception propagates to the pump. See [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md).
- **Transient error, retry later:** Throw `DeferMessageAction` to requeue the message on the External Bus with a delay. The message becomes available to any consumer after the delay expires. See [Requeue with Delay](#requeue-with-delay-defermessageaction).
- **Non-transient error, preserve for investigation:** Throw `RejectMessageAction` to route the message to a Dead Letter Queue (DLQ). Or use `RejectMessageOnErrorAttribute` as a backstop to catch any unhandled exception and reject. See [Reject to Dead Letter Queue](#reject-to-dead-letter-queue-rejectmessageaction).
- **Temporary block, try again after transport timeout:** Throw `DontAckAction` to leave the message on the channel. The transport re-delivers it after its visibility timeout expires. Or use `DontAckOnErrorAttribute` as a backstop. See [Don't Acknowledge](#dont-acknowledge-dontackaction).
- **Deserialization failure:** Throw `InvalidMessageAction` from a message mapper to route the message to an invalid message channel, keeping it separate from processing errors. See [Invalid Message Handling](#invalid-message-handling-invalidmessageaction).
- **Compensating action before failing:** Use `FallbackPolicyAttribute` to run cleanup or compensating logic when the handler fails, then let the exception propagate. See [Fallback Handlers](/contents/PolicyFallback.md).
- **Default (do nothing):** Let the exception propagate. The message is acknowledged and discarded. This is appropriate when errors are non-transient and you rely on logs and traces for investigation.

## Requeue with Delay (DeferMessageAction)

### What It Does

Throwing `DeferMessageAction` tells the message pump to reject the current message and requeue it on the External Bus with a delay. After the delay expires, the message becomes available for consumption again — by any consumer on that channel, not necessarily the same instance.

You control requeue behavior through two `Subscription` properties:

- **`RequeueDelay`** — how long the message waits before becoming visible again (default: `TimeSpan.Zero`).
- **`RequeueCount`** — the maximum number of times a message can be requeued before it is treated as a poison message (default: `-1`, which means infinite). When the count is exceeded, the message is rejected and routed to the Dead Letter Queue if one is configured, or acknowledged and discarded otherwise.

How the delay is implemented depends on the transport. Some transports support native delay; others rely on a configured `IAmARequestScheduler`. See [Error Handling Options](/contents/ErrorHandlingOptions.md) for configuration details.

### When to Use It

Use `DeferMessageAction` for transient failures where retrying the same message after a delay is likely to succeed. Typical scenarios include a downstream service that is temporarily unavailable or a rate limit that resets after a short period.

### Throwing DeferMessageAction Directly

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    private readonly IOrderService _orderService;

    public OrderHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override PlaceOrder Handle(PlaceOrder command)
    {
        try
        {
            _orderService.Place(command.OrderId, command.Amount);
        }
        catch (ServiceUnavailableException)
        {
            // Requeue the message — it will be retried after the configured delay
            throw new DeferMessageAction();
        }

        return base.Handle(command);
    }
}
```

### Retry Then Requeue Pattern

A common pattern is to combine in-process retry with deferred requeue. The resilience pipeline retries immediately a few times; if all retries fail, the handler catches the final exception and defers to retry later on the External Bus.

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    private readonly IOrderService _orderService;

    // Retry 3 times in-process, then requeue on the External Bus
    [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
    public override PlaceOrder Handle(PlaceOrder command)
    {
        try
        {
            _orderService.Place(command.OrderId, command.Amount);
        }
        catch (ServiceUnavailableException)
        {
            throw new DeferMessageAction();
        }

        return base.Handle(command);
    }
}
```

For details on configuring the resilience pipeline, see [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md).

## Reject to Dead Letter Queue (RejectMessageAction)

### What It Does

Throwing `RejectMessageAction` tells the message pump to end processing and route the message to a Dead Letter Queue (DLQ). The message is preserved in the DLQ for later investigation — operators can inspect, replay, or discard it manually.

If no DLQ is configured on the subscription, the message is acknowledged and discarded with a warning logged. The behavior is the same as the default, but with an explicit log entry indicating the message was rejected.

### When to Use It

Use `RejectMessageAction` for non-transient errors where the message should be preserved for investigation rather than silently discarded. Typical scenarios include business validation failures, corrupt data requiring manual review, or messages that reference entities that no longer exist.

### Throwing RejectMessageAction Directly

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    private readonly IOrderRepository _repository;

    public OrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public override PlaceOrder Handle(PlaceOrder command)
    {
        var customer = _repository.GetCustomer(command.CustomerId);

        if (customer is null)
        {
            // Customer no longer exists — reject to DLQ for investigation
            throw new RejectMessageAction($"Customer {command.CustomerId} not found");
        }

        _repository.PlaceOrder(command.OrderId, customer);
        return base.Handle(command);
    }
}
```

### Using RejectMessageOnErrorAttribute as a Backstop

Instead of catching every possible exception in your handler, you can add `RejectMessageOnErrorAttribute` to your pipeline. It wraps the handler invocation in a try/catch and converts any unhandled exception into a `RejectMessageAction`, ensuring the message goes to the DLQ rather than being silently discarded.

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    // Backstop: any unhandled exception → reject to DLQ
    [RejectMessageOnError(step: 0)]
    [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
    public override PlaceOrder Handle(PlaceOrder command)
    {
        // If this throws after retries are exhausted,
        // the backstop catches it and rejects to DLQ
        ProcessOrder(command);
        return base.Handle(command);
    }

    // ...
}
```

For async handlers, use `RejectMessageOnErrorAsyncAttribute` instead. The behavior is identical.

See [Backstop Attributes](#backstop-attributes) for pipeline ordering guidance and [Error Handling Options](/contents/ErrorHandlingOptions.md) for DLQ configuration.

## Don't Acknowledge (DontAckAction)

### What It Does

Throwing `DontAckAction` tells the message pump to leave the message unacknowledged on the channel. The transport re-delivers it after its visibility timeout expires. A configurable delay (`DontAckDelay`, default 1 second) pauses the pump before processing the next message, preventing tight-loop CPU burn when a message is repeatedly not acknowledged.

Each `DontAckAction` increments the unacceptable message counter. If the counter reaches the configured `UnacceptableMessageLimit`, the pump shuts down.

### When to Use It

Use `DontAckAction` when you want the transport's native re-delivery rather than Brighter's requeue. Typical scenarios include:

- A feature flag is temporarily disabled, and you want to hold messages until it is re-enabled.
- A dependent resource is under maintenance, and you prefer the message to stay on the channel rather than being requeued.
- You want the transport's visibility timeout to control when the message is retried, rather than Brighter's `RequeueDelay`.

### Transport Nack Behavior

A [nack](/contents/BasicConcepts.md#nack-negative-acknowledgment) (negative acknowledgment) signals to the transport that a message was not successfully processed. How the transport handles a nack varies:

| Transport | Nack Behavior |
|-----------|---------------|
| RabbitMQ | `BasicNack` with `requeue: true` — message is immediately re-queued |
| AWS SQS | `ChangeMessageVisibility` to 0 — message becomes immediately visible |
| Azure Service Bus | `AbandonMessage` — lock is released, message becomes available |
| Kafka | No-op (offset is not committed) — message is re-delivered on next poll |
| Redis | No-op (`BLPOP` is destructive — the message is already consumed) |
| MQTT | No-op (no acknowledgment concept in MQTT) |

### Throwing DontAckAction Directly

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    private readonly IFeatureFlags _features;
    private readonly IOrderService _orderService;

    public OrderHandler(IFeatureFlags features, IOrderService orderService)
    {
        _features = features;
        _orderService = orderService;
    }

    public override PlaceOrder Handle(PlaceOrder command)
    {
        if (!_features.IsEnabled("order-processing"))
        {
            // Feature is disabled — leave on channel, transport will re-deliver
            throw new DontAckAction("Order processing is temporarily disabled");
        }

        _orderService.Place(command.OrderId, command.Amount);
        return base.Handle(command);
    }
}
```

### Using DontAckOnErrorAttribute as a Backstop

Like `RejectMessageOnErrorAttribute`, you can use `DontAckOnErrorAttribute` to convert any unhandled exception into a `DontAckAction`. The message stays on the channel instead of being discarded.

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    // Backstop: any unhandled exception → don't ack, leave on channel
    [DontAckOnError(step: 0)]
    [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
    public override PlaceOrder Handle(PlaceOrder command)
    {
        ProcessOrder(command);
        return base.Handle(command);
    }

    // ...
}
```

For async handlers, use `DontAckOnErrorAsyncAttribute` instead.

## Invalid Message Handling (InvalidMessageAction)

### What It Does

Throwing `InvalidMessageAction` tells the message pump to route the message to an invalid message channel. This separates deserialization failures ("bad message") from processing failures ("bad handler") in your monitoring.

The message pump routes the message using this priority:

1. Invalid message channel (if configured on the subscription).
2. Dead Letter Queue (if no invalid message channel is configured).
3. Acknowledge and log (if neither is configured).

### When to Use It

Throw `InvalidMessageAction` from a message mapper when deserialization fails due to schema mismatches, versioning issues, or malformed content. Do not throw it from a handler — use `RejectMessageAction` instead for messages that deserialize successfully but cannot be processed.

### Throwing InvalidMessageAction in a Message Mapper

```csharp
public class OrderMessageMapper : IAmAMessageMapper<PlaceOrder>
{
    public PlaceOrder MapToRequest(Message message)
    {
        try
        {
            var body = JsonSerializer.Deserialize<PlaceOrder>(message.Body.Value);
            if (body is null)
                throw new InvalidMessageAction("Message body deserialized to null");
            return body;
        }
        catch (JsonException ex)
        {
            throw new InvalidMessageAction("Failed to deserialize PlaceOrder", ex);
        }
    }

    public Message MapToMessage(PlaceOrder request, Publication publication)
    {
        // ... serialization logic
    }
}
```

See [Error Handling Options](/contents/ErrorHandlingOptions.md) for configuring invalid message channels and DLQ routing.

## Backstop Attributes

Backstop attributes wrap your handler pipeline in a try/catch and convert any unhandled exception into the appropriate action exception. They are the simplest way to ensure messages are not silently discarded when something unexpected goes wrong.

### RejectMessageOnErrorAttribute

Catches any exception thrown by the inner pipeline and throws `RejectMessageAction`, routing the message to the DLQ. The original exception is preserved as the `InnerException` and logged at `Error` level before rethrowing.

- Sync: `RejectMessageOnErrorAttribute`
- Async: `RejectMessageOnErrorAsyncAttribute`

### DontAckOnErrorAttribute

Catches any exception thrown by the inner pipeline and throws `DontAckAction`, leaving the message on the channel for the transport to re-deliver. The original exception is preserved as the `InnerException`.

- Sync: `DontAckOnErrorAttribute`
- Async: `DontAckOnErrorAsyncAttribute`

### Pipeline Ordering

Backstop attributes should be at the **outermost** position in the pipeline (lowest step number, typically `step: 0`). Retry and circuit breaker middleware should be **inside** (higher step numbers). This ensures the backstop only fires after all retry attempts are exhausted.

```csharp
public class OrderHandler : RequestHandler<PlaceOrder>
{
    [RejectMessageOnError(step: 0)]                       // Outermost: backstop
    [UseResiliencePipeline("OrderCircuitBreaker", step: 1)] // Middle: circuit breaker
    [UseResiliencePipeline("OrderRetryPolicy", step: 2)]    // Innermost: retry
    public override PlaceOrder Handle(PlaceOrder command)
    {
        // 1. Retry wraps the handler (retries transient failures)
        // 2. Circuit breaker wraps retry (stops retrying if too many failures)
        // 3. Backstop wraps everything (rejects to DLQ if all else fails)
        ProcessOrder(command);
        return base.Handle(command);
    }

    // ...
}
```

The async equivalent uses the async variants of each attribute:

```csharp
public class OrderHandler : RequestHandlerAsync<PlaceOrder>
{
    [RejectMessageOnErrorAsync(step: 0)]
    [UseResiliencePipelineAsync("OrderCircuitBreaker", step: 1)]
    [UseResiliencePipelineAsync("OrderRetryPolicy", step: 2)]
    public override async Task<PlaceOrder> HandleAsync(
        PlaceOrder command, CancellationToken cancellationToken = default)
    {
        await ProcessOrderAsync(command, cancellationToken);
        return await base.HandleAsync(command, cancellationToken);
    }

    // ...
}
```

For more on pipeline construction, see [Building a Pipeline](/contents/BuildingAPipeline.md).

## Unacceptable Message Limit

Every time the message pump catches a `RejectMessageAction`, `DontAckAction`, `InvalidMessageAction`, or any other unhandled exception, it increments an internal **unacceptable message counter**. For unhandled exceptions (not action exceptions), the message is still acknowledged and discarded — but the counter increments, tracking that something went wrong. `DeferMessageAction` does not increment this counter because a deferred message is a normal retry, not an error.

If the counter reaches the configured `UnacceptableMessageLimit`, the pump shuts down. This protects against mass message loss during systemic failures — for example, a database outage that causes every message to fail.

You can optionally configure an `UnacceptableMessageLimitWindow` to reset the counter periodically. Without a window, the counter accumulates over the pump's entire lifetime and eventually triggers a shutdown even if errors are spread across hours.

See [Error Handling Options](/contents/ErrorHandlingOptions.md) for configuration details.

## Further Reading

- [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) — configure Polly resilience pipelines for in-process retry
- [Fallback Handlers](/contents/PolicyFallback.md) — run compensating logic before failing
- [Error Handling Options](/contents/ErrorHandlingOptions.md) — configure DLQ, requeue limits, and pump thresholds
- [Building a Pipeline](/contents/BuildingAPipeline.md) — create custom middleware for error handling
- [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md) — understand the message pump internals
