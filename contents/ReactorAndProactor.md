---
description: "Brighter V10 introduces clearer terminology for its concurrency models: Reactor and Proactor."
layout:
  description:
    visible: false
---

# Reactor and Proactor: Concurrency Models

> **Explanation** · Applies to **Brighter V10**

Brighter V10 introduces clearer terminology for its concurrency models: **Reactor** and **Proactor**. 

## Reactor and Proactor Overview

When consuming messages from external message brokers, Brighter uses a **Performer** (the message pump) to retrieve messages and dispatch them to handlers. The Performer is single-threaded in both concurrency models, but the way it handles I/O differs significantly.

### Reactor Pattern

The **Reactor pattern** uses **blocking I/O** with synchronous operations. When the Performer makes a synchronous call (e.g., to retrieve a message), it blocks the thread until the operation completes. This also blocks the message pump. Whilst this might seem negative, blocking the pump ensures that messages will be processed sequentially on a single thread.

With a Reactor you handlers, mappers and middleware should be sync.

**Characteristics:**

- Blocking I/O operations
- No context switching during I/O
- **Faster performance** per operation (lower latency)
- Thread remains busy during I/O
- Uses `MessagePumpType.Reactor`

**When to use Reactor:**

- When minimizing latency is critical
- When throughput requirements are moderate
- When you have sufficient threads available
- For traditional synchronous workloads

### Proactor Pattern

The **Proactor pattern** uses **non-blocking I/O** with asynchronous operations. When the Performer makes an asynchronous I/O call, it yields the thread during I/O operations. Brighter has it's own synchronization context, and work will resume on Brighter's thread when the I/O completes, unless you use ConfigureAwait(false) to force it onto a thread pool thread.

With a Proactor your handlers, mappers and middleware should be async.

**Characteristics:**

- Non-blocking I/O operations
- Thread yielded during I/O (context switch occurs)
- **Better throughput** (more operations per thread)
- Higher per-operation latency due to context switching
- Uses `MessagePumpType.Proactor`

**When to use Proactor:**

- When maximizing throughput is critical
- In container environments with limited threads
- With competing consumers pattern
- When processing high volumes of messages
- When I/O wait time is significant

## Performance vs. Throughput Trade-offs

| Aspect | Reactor (Blocking) | Proactor (Non-blocking) |
|--------|-------------------|------------------------|
| **Latency** | Lower (no context switch) | Higher (context switch overhead) |
| **Throughput** | Lower (thread stays busy) | Higher (thread yielded during I/O) |
| **Thread Efficiency** | One thread per pump | Multiple pumps can share threads |
| **Resource Usage** | More threads needed | Fewer threads needed |
| **Best For** | Low latency, moderate volume | High volume, container environments |

## Handler and Mapper Requirements

**Critical:** Your choice of Reactor or Proactor determines which handler and mapper types you must use. Mixing sync and async implementations will cause runtime errors.

### Reactor Pattern Requirements

When using `MessagePumpType.Reactor`, you must use **synchronous** implementations:

#### Reactor Handlers
Implement `IHandleRequests<T>` (not `IHandleRequestsAsync<T>`):

```csharp
public class MyCommandHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Synchronous business logic
        _repository.Save(command.Data);

        return base.Handle(command);
    }
}
```

#### Reactor Message Mappers
Use synchronous `MapToMessage` and `MapToRequest` methods:

```csharp
public class MyCommandMessageMapper : IAmAMessageMapper<MyCommand>
{
    public Message MapToMessage(MyCommand request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_COMMAND
        );
        var body = new MessageBody(JsonSerializer.Serialize(request));
        return new Message(header, body);
    }

    public MyCommand MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<MyCommand>(message.Body.Value);
    }
}
```

#### Reactor Middleware/Attributes
Use synchronous handler attributes and middleware:

```csharp
public class MyCommandHandler : RequestHandler<MyCommand>
{
    [UsePolicy("RetryPolicy", step: 1)]  // Synchronous policy
    [RequestLogging(step: 0, timing: HandlerTiming.Before)]  // Synchronous logging
    public override MyCommand Handle(MyCommand command)
    {
        // Handler logic
        return base.Handle(command);
    }
}
```

### Proactor Pattern Requirements

When using `MessagePumpType.Proactor`, you must use **asynchronous** implementations:

#### Proactor Handlers
Implement `IHandleRequestsAsync<T>` (not `IHandleRequests<T>`):

```csharp
public class MyCommandHandlerAsync : RequestHandlerAsync<MyCommand>
{
    public override async Task<MyCommand> HandleAsync(
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Asynchronous business logic
        await _repository.SaveAsync(command.Data, cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

#### Proactor Message Mappers
Message mappers remain synchronous (they don't perform I/O), but the mapper is called from an async context:

```csharp
public class MyCommandMessageMapper : IAmAMessageMapper<MyCommand>
{
    // Same synchronous implementation as Reactor
    public Message MapToMessage(MyCommand request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_COMMAND
        );
        var body = new MessageBody(JsonSerializer.Serialize(request));
        return new Message(header, body);
    }

    public MyCommand MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<MyCommand>(message.Body.Value);
    }
}
```

**Note:** Message mappers don't have async variants because they typically don't perform I/O operations—they just transform data structures. If your mapper needs to perform async I/O (e.g., reading from a claim check store), use a custom mapper with synchronous wrapper methods that call `Task.Run()` or similar.

#### Proactor Middleware/Attributes
Use asynchronous handler attributes and middleware:

```csharp
public class MyCommandHandlerAsync : RequestHandlerAsync<MyCommand>
{
    [UseResiliencePipeline("RetryPipeline", step: 1)]  // Async resilience pipeline
    [RequestLoggingAsync(step: 0, timing: HandlerTiming.Before)]  // Async logging
    public override async Task<MyCommand> HandleAsync(
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Handler logic
        return await base.HandleAsync(command, cancellationToken);
    }
}
```

### What Happens If You Mix Them?

Mixing sync and async implementations causes runtime errors:

| Configuration | Handler Type | Result |
|---------------|-------------|---------|
| `MessagePumpType.Reactor` | `IHandleRequests<T>` | Works correctly |
| `MessagePumpType.Reactor` | `IHandleRequestsAsync<T>` | **Runtime error**: Reactor pump cannot dispatch to async handlers |
| `MessagePumpType.Proactor` | `IHandleRequestsAsync<T>` | Works correctly |
| `MessagePumpType.Proactor` | `IHandleRequests<T>` | **Runtime error**: Proactor pump cannot dispatch to sync handlers |

**Best Practice:** Be consistent throughout your entire pipeline:
- Reactor → All synchronous handlers, middleware, and policies
- Proactor → All asynchronous handlers, middleware, and policies

### Checking Your Implementation

To verify your handlers match your pump type:

```csharp
// Reactor - Check for IHandleRequests<T>
public class MyHandler : RequestHandler<MyCommand>  // Correct base class
{
    public override MyCommand Handle(MyCommand command)  // Sync method
    {
        return base.Handle(command);
    }
}

// Proactor - Check for IHandleRequestsAsync<T>
public class MyHandlerAsync : RequestHandlerAsync<MyCommand>  // Correct base class
{
    public override async Task<MyCommand> HandleAsync(  // Async method
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        return await base.HandleAsync(command, cancellationToken);
    }
}
```

## Reactor and Proactor Configuration

### Reactor Configuration

Configure a subscription to use the Reactor pattern with `MessagePumpType.Reactor`:

```csharp
var subscription = new Subscription<MyCommand>(
    new SubscriptionName("my.subscription"),
    channelName: new ChannelName("my.channel"),
    routingKey: new RoutingKey("my.routing.key"),
    messagePumpType: MessagePumpType.Reactor,  // Blocking I/O
    timeOut: TimeSpan.FromMilliseconds(200),
    makeChannels: OnMissingChannel.Create
);
```

### Proactor Configuration

Configure a subscription to use the Proactor pattern with `MessagePumpType.Proactor`:

```csharp
var subscription = new Subscription<MyCommand>(
    new SubscriptionName("my.subscription"),
    channelName: new ChannelName("my.channel"),
    routingKey: new RoutingKey("my.routing.key"),
    messagePumpType: MessagePumpType.Proactor,  // Non-blocking I/O
    timeOut: TimeSpan.FromMilliseconds(200),
    makeChannels: OnMissingChannel.Create
);
```

## Transport Native Support

Different transports have varying levels of native support for synchronous and asynchronous operations. When a transport doesn't natively support an operation mode, Brighter adapts it:

| Transport | Reactor Support | Proactor Support | Notes |
|-----------|----------------|------------------|-------|
| **Azure Service Bus** | Sync over Async | Native | Async API is native |
| **AWS (SNS/SQS)** | Sync over Async | Native | Async API is native |
| **GCP Pub/Sub** | Sync over Async | Native | Streaming pull is the default; `SubscriptionMode.Pull` has native sync and async APIs |
| **Kafka** | Native | Async over Sync | Sync API is native |
| **MQTT** | Sync over Async/Event Based | Event Based | Event-driven architecture |
| **MSSQL** | Native | Native | Both APIs supported |
| **PostgreSQL** | Native | Native | Both APIs supported |
| **RabbitMQ v6** | Sync over Async | Native | Async client recommended |
| **RabbitMQ v7** | Sync over Async | Native | Async client recommended |
| **Redis** | Native | Native | Both APIs supported |
| **RocketMQ** | Sync over Async | Native | Async API is native |

**Key:**
- **Native**: Transport SDK provides native support
- **Sync over Async**: Uses `Task.Run()` or similar to wrap async in sync
- **Async over Sync**: Uses `Task.FromResult()` or similar to wrap sync in async
- **Event Based**: Uses event-driven callbacks

## The Performer (Message Pump)

The `Performer` is an instance of Brighter's `MessagePump` implementation that retrieves messages and dispatches them to handlers. Important characteristics:

- **Single-threaded** by default in both Reactor and Proactor modes
- Runs one message at a time through the handler pipeline
- Provides a synchronization context for handler execution
- Manages the message processing lifecycle

### ConfigureAwait(false) Warning

If you use `ConfigureAwait(false)` in your handler code when using the Proactor pattern, you will bypass Brighter's synchronization context and resume on a thread pool thread:

```csharp
public class MyCommandHandlerAsync : RequestHandlerAsync<MyCommand>
{
    public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken cancellationToken = default)
    {
        // This yields to the thread pool, ignoring the synchronization context
        await SomeOperationAsync().ConfigureAwait(false);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

**Important:** Using `ConfigureAwait(false)` means:

- Your handler continues on a thread pool thread (not the Performer's thread)
- You lose the single-threaded guarantee
- Multiple handlers could execute concurrently if you have multiple Performers

For more details on `ConfigureAwait`, see the [official .NET documentation](https://devblogs.microsoft.com/dotnet/configureawait-faq/).

## Migration from V9 to V10

In Brighter V9, concurrency was configured using `isAsync` and `runAsync` flags. In V10, this has been simplified to the `MessagePumpType` enum.

### V9 Configuration (Deprecated)

```csharp
// V9 - DEPRECATED
var subscription = new Subscription<MyCommand>(
    // ... other parameters
    isAsync: true,      // Was this an async handler?
    runAsync: true      // Should we use async message pump?
);
```

### V10 Configuration (Current)

```csharp
// V10 - Use MessagePumpType
var subscription = new Subscription<MyCommand>(
    // ... other parameters
    messagePumpType: MessagePumpType.Proactor  // Clear and explicit
);
```

### Migration Steps

1. **Remove `isAsync` and `runAsync` parameters** from your subscriptions
2. **Add `messagePumpType` parameter** with appropriate value:
   - Use `MessagePumpType.Reactor` if you had `isAsync: false, runAsync: false` (or both true but blocking code)
   - Use `MessagePumpType.Proactor` if you had `isAsync: true, runAsync: true` with async handlers
3. **Update handler implementations** to match:
   - `MessagePumpType.Reactor` → Use `IHandleRequests<T>` with synchronous `Handle()` method
   - `MessagePumpType.Proactor` → Use `IHandleRequestsAsync<T>` with async `HandleAsync()` method

## Choosing Between Reactor and Proactor

Use this decision guide to choose the right pattern:

### Choose Reactor When:
- You need the lowest possible latency per message
- Message volume is moderate
- Your handlers perform synchronous operations
- You're working with transports that have native sync APIs (Kafka, MSSQL, Redis)

### Choose Proactor When:
- You need to maximize throughput
- Your handlers perform significant I/O operations
- You're working with transports that have native async APIs (AWS, Azure, RabbitMQ)

### Example Scenarios

**Low-Latency Trading System (Reactor):**
```csharp
// Need fastest possible response time per trade
var subscription = new Subscription<TradeCommand>(
    new SubscriptionName("trade.processor"),
    channelName: new ChannelName("trades"),
    routingKey: new RoutingKey("trade.execute"),
    messagePumpType: MessagePumpType.Reactor,  // Minimize latency
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

**High-Volume Order Processing (Proactor):**
```csharp
// Need to process thousands of orders per second
var subscription = new Subscription<OrderCommand>(
    new SubscriptionName("order.processor"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("order.created"),
    messagePumpType: MessagePumpType.Proactor,  // Maximize throughput
    timeOut: TimeSpan.FromMilliseconds(500)
);
```


## Reactor and Proactor Best Practices

1. **CRITICAL: Match your handler implementation to your MessagePumpType:**
   - Reactor → Synchronous handlers (`IHandleRequests<T>`)
   - Proactor → Asynchronous handlers (`IHandleRequestsAsync<T>`)
   - Mixing these will cause **runtime errors**
   - See [Handler and Mapper Requirements](#handler-and-mapper-requirements) for details

2. **Consider your transport's native support:**
   - Prefer Proactor for AWS, Azure, RabbitMQ (native async)
   - Consider Reactor for Kafka (native sync)

3. **Profile your application:**
   - Measure actual latency and throughput
   - Test both patterns under load
   - Choose based on your specific requirements

4. **Be cautious with ConfigureAwait(false):**
   - Understand you're breaking the single-threaded guarantee
   - Use only when you need thread pool execution
   - Test thoroughly for race conditions

5. **Start simple:**
   - Begin with Proactor (it's generally more flexible)
   - Switch to Reactor only if latency measurements require it
   - Don't optimize prematurely

## Related Documentation

- [How the Dispatcher Works](HowServiceActivatorWorks.md) - Dispatcher internals
- [Configuring the Dispatcher](HowConfiguringTheDispatcherWorks.md) - Dispatcher configuration
- [Configuring Subscriptions](/contents/BrighterBasicConfiguration.md#configuring-the-dispatcher) - Subscription configuration

## Reactor and Proactor Summary

- **Reactor** = Blocking I/O = Lower latency per operation = Moderate throughput = Requires **synchronous** handlers
- **Proactor** = Non-blocking I/O = Higher latency per operation = Better throughput = Requires **asynchronous** handlers
- **Performer** = Single-threaded message pump in both patterns
- **Handler Types MUST Match:** Reactor uses `IHandleRequests<T>`, Proactor uses `IHandleRequestsAsync<T>`
- **ConfigureAwait(false)** = Breaks single-threaded guarantee
- **Choose based on your requirements:** Latency-sensitive? → Reactor. Throughput-sensitive? → Proactor
- **When in doubt:** Start with Proactor (it's more flexible for most use cases)
