---
description: "This FAQ addresses common questions about using Brighter and Darker, organized by category."
layout:
  description:
    visible: false
---

# Frequently Asked Questions

> **Reference** · Applies to **Brighter V10**

This FAQ addresses common questions about using Brighter and Darker, organized by category. For V10-specific changes, see the [V10 Migration Guide](/contents/V10MigrationGuide.md).

## Table of Contents

- [Getting Started](#getting-started-questions)
- [Configuration](#configuration-questions)
- [Messaging](#messaging-questions)
- [Handlers & Pipelines](#handlers--pipelines-questions)
- [Resilience & Policies](#resilience--policies-questions)
- [Scheduling](#scheduling-questions)
- [Migration](#migration-questions)
- [Performance & Concurrency](#performance--concurrency-questions)

---

## Getting Started Questions

### How do I get started with Brighter?

Start with the simplest possible setup and add complexity as needed:

1. **Read** [Show me the code!](/contents/ShowMeTheCode.md) for quick examples
2. **Start simple**: Use `Send()` and handlers without external messaging
3. **Add external bus**: Use `PostAsync()` with InMemory Outbox (development only)
4. **Add reliability**: Switch to `DepositPost` + database-backed Outbox (production)
5. **Add deduplication**: Add Inbox pattern for consumers
6. **Explore samples**: Check out the [WebAPI Sample](https://github.com/BrighterCommand/Brighter/tree/master/samples/WebAPI)

**Philosophy**: Don't over-engineer early. Use defaults, avoid premature abstraction, and add features as you need them.

### Do I need to write message mappers in V10?

**No!** In V10, you typically don't need explicit message mappers for JSON serialization.

Brighter V10 provides **default message mappers** that automatically serialize/deserialize JSON messages:

- **JsonMessageMapper** - Binary-mode CloudEvents (default)
- **CloudEventJsonMessageMapper** - Structured-mode CloudEvents

**When you still need custom mappers:**

- Non-JSON formats (Avro, ProtoBuf)
- Transform pipelines (ClaimCheck, Compression, Encryption)
- Custom serialization logic

See: [Default Message Mappers](/contents/DefaultMessageMappers.md)

### What's the difference between Command, Event, and Query?

- **Command**: An instruction to do something (may update state). Has exactly one handler. Example: `CreateOrder`, `UpdateUser`
- **Event**: A notification that something happened (past tense). Can have multiple handlers. Example: `OrderCreated`, `UserUpdated`
- **Query**: A request for data (does not update state). Returns a result. Example: `GetOrderById`, `FindUsers`

**Commands and Events** use Brighter (Command Processor).
**Queries** use Darker (Query Processor).

See: [Show me the code!](/contents/ShowMeTheCode.md)

### Should I use InMemory options in production?

**Generally no** - InMemory options (Outbox, Inbox, Scheduler, Transport) are **not durable**. If your application crashes, you lose data.

**InMemory is for:**

- Development and testing (fast, zero dependencies)
- Demos and experimentation
- Limited production scenarios where data loss is acceptable

**For production**, use:

- Database-backed Outbox/Inbox (SQL Server, PostgreSQL, MySQL, DynamoDB, MongoDB)
- Production schedulers (Quartz, Hangfire, AWS Scheduler, Azure Scheduler)
- Real message brokers (RabbitMQ, Kafka, AWS SNS/SQS, Azure Service Bus)

See: [InMemory Options](/contents/InMemoryOptions.md)

### How do I structure my handlers?

Follow these guidelines:

1. **One responsibility per handler** - Each handler should do one thing
2. **Use attributes for cross-cutting concerns** - Logging, retry, timeouts via attributes
3. **Don't create handler base classes** - Use attributes instead of inheritance for common functionality
4. **Keep handlers thin** - Delegate to domain services or repositories
5. **Avoid sharing state** - Handlers should be stateless (use Request Context for passing data)

**Bad** (custom base class):

```csharp
public abstract class MyHandlerBase<T> : RequestHandlerAsync<T>
{
    // Custom logging, retry logic...
}
```

**Good** (use attributes):
```csharp
[RequestLoggingAsync(0, HandlerTiming.Before)]
[UseResiliencePipeline(1, "MyRetryPolicy")]
public class MyHandler : RequestHandlerAsync<MyCommand>
{
    // Business logic only
}
```

---

## Configuration Questions

### What's the difference between AddProducers and AddConsumers?

In V10, configuration was simplified:

- **AddProducers()**: Configures message producers (sending messages to external bus). Replaces V9's `UseExternalBus()`
- **AddConsumers()**: Configures message consumers (receiving messages from external bus). Replaces V9's `AddServiceActivator()`

**Example:**

```csharp
services.AddBrighter()
    .AutoFromAssemblies();

// Configure producers
services.AddProducers(options =>
{
    options.UseRabbitMQ(...)
        .Publication<MyEvent>(p => p.Topic = new RoutingKey("my.event"));
});

// Configure consumers
services.AddConsumers(options =>
{
    options.UseRabbitMQ(...)
        .Subscription<MyEvent>(s => s.ChannelName = new ChannelName("my.queue"));
});
```

See: [Basic Configuration](/contents/BrighterBasicConfiguration.md), [V10 Migration Guide](/contents/V10MigrationGuide.md)

### When should I use Reactor vs Proactor?

**Reactor** (blocking I/O):

- Faster per-message performance (no context switches)
- Better for CPU-bound operations

**Proactor** (non-blocking I/O):

- Better throughput (yields threads during I/O)
- Slightly slower per-message (context switch overhead)

**Configure with:**

```csharp
new Subscription(
    messagePumpType: MessagePumpType.Proactor  // or MessagePumpType.Reactor
)
```

**Recommendation**: Use Proactor for most scenarios (better scalability). Use Reactor for CPU-intensive workloads.

Specific transports may behave better with particular message pump models. For example, Kafka works better with the Reactor model, and RabbitMQ V7+ with the Proactor model.

See: [Reactor and Proactor](/contents/ReactorAndProactor.md)

### How do I configure CloudEvents?

In V10, CloudEvents support is built-in. Configure in your Publication:

```csharp
services.AddProducers(options =>
{
    options.UseRabbitMQ(...)
        .Publication<MyEvent>(publication =>
        {
            publication.Topic = new RoutingKey("my.event");
            publication.Source = new Uri("https://myapp.example.com");
            publication.Type = new CloudEventsType("com.example.myevent");
            publication.Subject = "order/123";
        });
});
```

**Binary vs Structured mode:**

- **Binary-mode** (default): CloudEvents attributes in headers, data in body. Use with RabbitMQ, Kafka, AMQP.
- **Structured-mode**: Entire CloudEvents envelope in JSON body. Use with AWS SNS/SQS (limited headers).

See: [CloudEvents Support](/contents/CloudEventsSupport.md)

---

## Messaging Questions

### What's the difference between Post and DepositPost?

- **Post()**: Writes to the `InMemoryOutbox` and then publishes via the transport. No database transaction. Simple, but no guarantees. A sweeper can pick up failed sends, you can run the Sweeper in the same process, as the outbox is local to the process.
- **DepositPost()**: Writes to Outbox, you should pass in your database transaction provider to ensure that it participates in the same transaction that writes your entity. Guarantees entity writes and message writes succeed/fail together. You may use` ClearOutbox` to publish immediately, passing in a list of Ids to publish, or rely on your Sweeper to process un-dispatched messages. Waiting for the Sweeper increased latency because you wait for the next polling loop to publish.

**Use Post when:**

- Getting started (simplest approach)
- Using InMemory Outbox (development)
- Message loss is acceptable

**Use DepositPost when:**

- Production systems
- Need transactional guarantees
- Database-backed Outbox

See: [Outbox Support](/contents/BrighterOutboxSupport.md)

### When should I use `SendAsync` or `PublishAsync` vs External Bus?

**c`SendAsync` or `PublishAsync:**

- Avoids blocking I/O
- Increases throughput (thread reuse)
- Caller waits for result
- Simple programming model
- Work lost if process crashes

**External Bus (`PostAsync` / message queue):**

- Hands off work to another process
- Caller doesn't wait (eventual consistency)
- Reliable (guaranteed delivery via queue)
- More complex (async notification of completion)
- Work survives process crashes

**Recommendations:**

- Use async handlers for operations < 200ms
- Use External Bus for long-running operations (> 200ms)
- Use External Bus for CPU-bound operations
- Use External Bus when reliability matters (work survives crashes)

### Can I handle multiple message types on one queue/topic?

Yes! Use **Dynamic Deserialization** with a `getRequestType` callback:

```csharp
new KafkaSubscription(
    new SubscriptionName("task.updates"),
    channelName: new ChannelName("task.state"),
    routingKey: new RoutingKey("task.update"),
    getRequestType: message => message switch
    {
        var m when m.Header.Type == new CloudEventsType("io.goparamore.task.created")
            => typeof(TaskCreated),
        var m when m.Header.Type == new CloudEventsType("io.goparamore.task.updated")
            => typeof(TaskUpdated),
        _ => throw new ArgumentException($"Unknown message type: {message.Header.Type}")
    }
)
```

**However**, the **DataType Channel** pattern (one type per channel) is simpler and recommended for most scenarios.

See: [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)

### How do I handle large messages?

Use the **Claim Check** pattern:

1. Store large payload externally (S3, blob storage)
2. Send only a reference (claim check) in the message
3. Receiver retrieves payload using the claim check

**With transforms:**

```csharp
public class MyMessageMapper : IAmAMessageMapper<MyEvent>
{
    [ClaimCheck(0, thresholdInKb: 256)]
    public Message MapToMessage(MyEvent request, Publication publication)
    {
        // Automatically stores payloads > 256KB externally
    }
}
```

See: [Default Message Mappers](/contents/DefaultMessageMappers.md), [S3 Luggage Store](/contents/S3LuggageStore.md)

---

## Handlers & Pipelines Questions

### How do I pass data between handlers in a pipeline?

Use the **Request Context**:

```csharp
// First handler
public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken ct)
{
    Context.Bag["UserId"] = GetCurrentUserId();
    return await base.HandleAsync(command, ct);
}

// Next handler
public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken ct)
{
    var userId = Context.Bag["UserId"] as string;
    // Use userId...
    return await base.HandleAsync(command, ct);
}
```

**Use well-known keys** from `RequestContextBagNames` when available.

See: [Using the Context Bag](/contents/UsingTheContextBag.md)

### When should I use Agreement Dispatcher?

Use Agreement Dispatcher when you need **dynamic handler selection** based on request content or context:

**Use cases:**

- Time-based routing (rules change over time)
- Order journeys (different routes based on order contents)
- Country-specific business logic
- Versioning scenarios
- State-based routing

**Example:**

```csharp
registry.RegisterAsync<MyCommand>((request, context) =>
{
    var myCommand = request as MyCommand;
    if (myCommand?.Value == "first")
        return [typeof(MyImplicitHandlerAsync)];

    return [typeof(MyCommandHandlerAsync)];
},
    [typeof(MyImplicitHandlerAsync), typeof(MyCommandHandlerAsync)]
);
```

**Note**: You cannot use `AutoFromAssemblies()` with Agreement Dispatcher - must use `Handlers()` method.

See: [Agreement Dispatcher](/contents/AgreementDispatcher.md)

### How do I iterate over a list of requests to dispatch them? 

All **Command** or **Event** messages derive from **IRequest** and **ICommand** and **IEvent** respectively. So it may seem natural to create a collection of them, for example **List\<IRequest\>**, and then
process a set of messages by enumerating over them.

When you try this, you will encounter the issue that we dispatch based on the concrete type of the **Command** or **Event**. In other words the type you register via the **SubscriberRegistry.** Because
**CommandProcessor.Send()** is actually **CommandProcessor.Send\<T\>()** you need to provide the concrete type in the call for the compiler to determine the type to use with the cool as the concrete type.

If you try this:

``` csharp
ICommand command = new GreetingCommand("Ian");
commandProcessor.Send(command);
```

Then you will get this error: *\"ArgumentException \"No command handler was found for the typeof command Brighter.commandprocessor.ICommand - a command should have exactly one handler.\"\"*

Now, you don\'t see this issue if you pass the concrete type in, so the compiler can correctly resolve the run-time type.

``` csharp
commandProcessor.Send(new GreetingCommand("Ian"));
```

So what can you do if you must pass the base class to the **Command Processor** i.e. because you are using a list.

The workaround is to use the dynamic keyword. Using the dynamic keyword means that the type will be evaluated using RTTI, which will successfully pick up the type that you need.

``` csharp
ICommand command = new GreetingCommand("Ian");
commandProcessor.Send((dynamic)command);
```

---

## Resilience & Policies Questions

### How do I add retry logic to my handlers?

In V10, use **Resilience Pipelines** with Polly v8:

**1. Configure the pipeline:**

```csharp
services.AddResiliencePipeline("MyRetryPolicy", builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    });
});
```

**2. Apply to handler:**

```csharp
[UseResiliencePipeline(1, "MyRetryPolicy")]
public class MyHandler : RequestHandlerAsync<MyCommand>
{
    // Handler logic
}
```

**Note**: `[UsePolicy]` and `[TimeoutPolicy]` are deprecated in V10. Migrate to `[UseResiliencePipeline]`.

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md), [V10 Migration Guide](/contents/V10MigrationGuide.md)

### What resilience strategies are available?

Polly v8 provides these strategies (all available via Resilience Pipelines):

- **Retry** - Automatic retry with configurable delays
- **Circuit Breaker** - Prevent cascading failures
- **Timeout** - Limit operation duration
- **Rate Limiter** - Control request rate
- **Fallback** - Alternative behavior on failure
- **Hedging** - Send duplicate requests for low latency

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md)

### What happened to TimeoutPolicy in V10?

`[TimeoutPolicy]` is **deprecated in V10** and will be removed in V11.

**Migrate to Resilience Pipeline:**

**Old (V9):**

```csharp
[TimeoutPolicy(step: 1, milliseconds: 5000)]
public class MyHandler : RequestHandlerAsync<MyCommand> { }
```

**New (V10):**

```csharp
// Configure pipeline
services.AddResiliencePipeline("MyTimeout", builder =>
{
    builder.AddTimeout(TimeSpan.FromSeconds(5));
});

// Apply to handler
[UseResiliencePipeline(1, "MyTimeout")]
public class MyHandler : RequestHandlerAsync<MyCommand> { }
```

See: [V10 Migration Guide](/contents/V10MigrationGuide.md)

---

## Scheduling Questions

### What scheduler should I use in production?

**For production**, use:

- **Quartz.NET** - Battle-tested, persistent, distributed, clustering support
- **Hangfire** - Persistent, web dashboard, easy setup (⚠️ not strong-named)
- **AWS Scheduler** - Serverless, cloud-native (AWS only)
- **Azure Scheduler** - Managed service, built into Service Bus (Azure only, no reschedule support)

**For development/testing:**

- **InMemory Scheduler** - Simple, fast, but not durable

**Comparison:**

| Feature | Quartz | Hangfire | AWS | Azure | InMemory |
|---------|--------|----------|-----|-------|----------|
| Production-ready | ✅ | ✅ | ✅ | ✅ | ❌ |
| Persistent | ✅ | ✅ | ✅ | ✅ | ❌ |
| Clustering | ✅ | ✅ | N/A | N/A | ❌ |
| Dashboard | ❌ | ✅ | ✅ | ✅ | ❌ |
| Reschedule | ✅ | ✅ | ✅ | ❌ | ✅ |
| Strong-named | ✅ | ❌ | ✅ | ✅ | ✅ |

See: [Scheduler Support](/contents/BrighterSchedulerSupport.md)

### How do I schedule a message for later?

Use `SendAsync()` or `PostAsync()` with a delay:

```csharp
// Schedule with DateTimeOffset
await commandProcessor.SendAsync(
    new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero),
    new MyCommand())
);

// Schedule with TimeSpan delay
await commandProcessor.PostAsync(
    TimeSpan.FromHours(24),
    new MyEvent()
);

// Returns scheduler ID for cancellation/reschedule
var schedulerId = await commandProcessor.SendAsync(command, delay);
```

**Note**: Requires a configured scheduler (Quartz, Hangfire, AWS, Azure, or InMemory).

See: [Scheduler Support](/contents/BrighterSchedulerSupport.md)

### Can I cancel or reschedule a scheduled message?

Yes, using the scheduler ID returned when scheduling:

**Cancel:**

```csharp
var schedulerId = await commandProcessor.SendAsync(command, delay);
await scheduler.CancelAsync(schedulerId);
```

**Reschedule:**

```csharp
var schedulerId = await commandProcessor.SendAsync(command, delay);
await scheduler.RescheduleAsync(schedulerId, newDelay);
```

**Note**: Azure Service Bus Scheduler does NOT support reschedule - you must cancel and create a new schedule.

See: [Scheduler Support](/contents/BrighterSchedulerSupport.md)

---

## Migration Questions

### How do I migrate from V9 to V10?

Follow the step-by-step [V10 Migration Guide](/contents/V10MigrationGuide.md).

**Key breaking changes:**
1. **Nullable Reference Types** - Enable in project, address compiler warnings
2. **Configuration Methods** - `UseExternalBus()` → `AddProducers()`, `AddServiceActivator()` → `AddConsumers()`
3. **Message Pump** - `runAsync` parameter → `messagePumpType: MessagePumpType.Reactor/Proactor`
4. **Polly** - `[TimeoutPolicy]` deprecated, use `[UseResiliencePipeline]`
5. **Request Context** - New properties added (PartitionKey, CustomHeaders, etc.)

**Typical migration time: 1-4 hours**

See: [V10 Migration Guide](/contents/V10MigrationGuide.md)

### What changed with OpenTelemetry in V10?

V10 now uses **OpenTelemetry Semantic Conventions** instead of custom conventions.

**Breaking changes:**

- Span names changed to follow OTel conventions
- Attribute names follow `paramore.brighter.*` and `messaging.*` namespaces
- W3C TraceContext propagation (traceparent/tracestate headers)

**Benefits:**

- Better interoperability with other systems
- Standard observability tooling works out-of-the-box
- CloudEvents integration for trace propagation

See: [Telemetry](/contents/Telemetry.md), [V10 Migration Guide](/contents/V10MigrationGuide.md)

### Do I need to update my message mappers for V10?

**Maybe not!** V10 provides default mappers for JSON serialization.

**If you have explicit JSON mappers**, you can likely **remove them** and use the default mappers.

**Keep custom mappers if:**

- Using non-JSON formats (Avro, ProtoBuf)
- Using transform pipelines (ClaimCheck, Compression, Encryption)
- Have custom serialization logic

See: [Default Message Mappers](/contents/DefaultMessageMappers.md), [V10 Migration Guide](/contents/V10MigrationGuide.md)

---

## Performance & Concurrency Questions

### Which message pump type gives better throughput?

Proactor, in most cases — it yields its thread during I/O. Reactor is faster per
message. See [When should I use Reactor vs Proactor?](#when-should-i-use-reactor-vs-proactor)
under Configuration for the full comparison and how to configure it.

### How many message pumps should I configure per queue?

**Start with 1 pump per queue** and increase based on monitoring.

**Considerations:**

- More pumps = higher throughput, more concurrent processing
- But also = more database connections, more memory, more competing consumers
- Depends on: message rate, processing time, available resources

**Recommendations:**

- Start with 1 pump per queue
- Monitor: queue depth, processing latency, CPU/memory usage
- Scale up if: queues backing up, high latency, low resource utilization
- Scale down if: low message rate, resource constraints

**Configure with:**

```csharp
new Subscription(
    // Other settings...
    channelCount: 3  // Number of concurrent message pumps
)
```

### Should I use competing consumers or a single consumer?

**Competing Consumers** (multiple instances):
- ✅ Higher throughput
- ✅ Better fault tolerance (one instance fails, others continue)
- ✅ Easier to scale horizontally
- ❌ Messages processed out-of-order (unless using partitions)

**Single Consumer:**
- ✅ Guaranteed message ordering
- ✅ Simpler reasoning about state
- ❌ Lower throughput
- ❌ Single point of failure

**Recommendation**: Use competing consumers with partition keys for ordering when needed.

See: [Request Context](/contents/DispatchingARequest.md) for partition key configuration

---

## See Also

- [Glossary](/contents/Glossary.md) - Definitions of key terms
- [V10 Migration Guide](/contents/V10MigrationGuide.md) - Upgrading from V9
- [Show me the code!](/contents/ShowMeTheCode.md) - Quick start examples
- [WebAPI Sample](https://github.com/BrighterCommand/Brighter/tree/master/samples/WebAPI) - Production example
