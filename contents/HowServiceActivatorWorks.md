---
description: "The Dispatcher is the component in the Brighter.ServiceActivator assembly that orchestrates your Performers."
layout:
  description:
    visible: false
---

# How The Dispatcher Works

> **Explanation** · Applies to **Brighter V10**

The **Dispatcher** is the component in the `Brighter.ServiceActivator` assembly that orchestrates your `Performers`. Each `Performer` is a single-thread that runs a `MessagePump`. The `MessagePump` consumes messages from streams or queues, calls a `IAmAMessageMapper` or `IAmAMessageMapperAsync` to map them to a C# type and then dispatches that type to your registered `IHandleRequests` or `IHandleRequestsAsync`. The Command Processor handles in-process request dispatching, and the Dispatcher orchestrates the threads that provide external message consumption and routing.

## Dispatcher Overview

When you configure Brighter to consume messages from external brokers (using `AddConsumers()`), you're setting up the Dispatcher.

The `Dispatcher`:

1. **Orchestrates** your `Performers` which is a thread reading messages. You can scale-out your `Performers` for the Competing Consumers pattern.

The `Performer`:

1. **Listens** to a configured channel on an external message brokers
2. **Retrieves** messages using a Message Pump (a Reactor or a Proactor)

The `Message Pump`:

1. **Deserializes** messages into requests using Message Mappers
2. **Dispatches** requests to registered Handlers via the Command Processor
3. **Acknowledges** the message pump acknowledges or rejects messages based on a handler completing without an exception

## Dispatcher Architecture

```text
External Message Broker
         ↓
  [Dispatcher] (ServiceActivator assembly)
         ↓
    [Performer]
                    ↓
      [Message Pump - Reactor or Proactor  
                  ↓   
        [Message Mapper] (Deserialize)
                  ↓
        [Command Processor] (Handler Pipeline)
                    ↓
               [Your Handler]
```

## The Message Pump

The `MessagePump` retrieves messages from external brokers. It operates in one of two concurrency models:

### Reactor Pattern (Blocking I/O)

- Uses synchronous message retrieval
- Blocks thread during I/O operations
- Lower latency per message
- Uses `MessagePumpType.Reactor`

### Proactor Pattern (Non-blocking I/O)

- Uses asynchronous message retrieval
- Yields thread during I/O operations
- Higher throughput
- Uses `MessagePumpType.Proactor`

**See [Reactor and Proactor](ReactorAndProactor.md) for detailed information on choosing between these patterns.**

## Message Flow

Let's trace a message from the broker to your handler:

### 1. Message Pump Retrieves Message

The Performer's message pump polls its channel for new messages (`Reactor.cs:123`, `Proactor.cs:162` at 10.8.0):

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Reactor.cs:123 and Proactor.cs:162, the pump's Channel -->
```csharp
// Reactor (blocking), Reactor.cs:123
// ...
message = Channel.Receive(TimeOut);
// ...

// Proactor (non-blocking), Proactor.cs:162
// ...
message = await Channel.ReceiveAsync(TimeOut);
// ...
```

### 2. Request Context Creation

The Message Pump creates a `RequestContext` that includes:

- The original `Message` (via `OriginatingMessage` property)
- Span for tracing (OpenTelemetry integration)
- Channel Name
- When the request started

Both pumps make the same call inside their message loop (`Reactor.cs:208`, `Proactor.cs:247` at 10.8.0):

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Reactor.cs:208 and Proactor.cs:247, InitRequestContext -->
```csharp
// Reactor.cs:208, Proactor.cs:247
// ...
RequestContext context = InitRequestContext(processSpan, message);
// ...
```

### 3. Message Deserialization

The Dispatcher uses the registered message mapper to convert the `Message` into a request object, passing the context it has just created. If you register no custom mapper, V10 uses the default mapper. The callback hands back the transform pipeline the mapper ran, so the pump can release it: straight away, or after the handler has run if a transform left cleanup pending.

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Reactor.cs:211, TranslateMessage -->
```csharp
// Reactor (blocking), Reactor.cs:211
// ...
var request = TranslateMessage(message, context, pipeline => unwrapPipeline = pipeline);
// ...
```

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Proactor.cs:250, TranslateMessage -->
```csharp
// Proactor (non-blocking), Proactor.cs:250
// ...
var request = await TranslateMessage(message, context, pipeline => unwrapPipeline = pipeline);
// ...
```

### 4. Handler Dispatch

The request is dispatched to the Command Processor, which executes the handler pipeline:

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Reactor.cs:219, InvokeDispatchRequest -->
```csharp
// Reactor, Reactor.cs:219
// ...
InvokeDispatchRequest(request, context);
// ...
```

<!-- blockcheck: skip a line quoted from Brighter's source, calling a private member or reading the pump's own state: Proactor.cs:258, InvokeDispatchRequest -->
```csharp
// Proactor, Proactor.cs:258
// ...
await InvokeDispatchRequest(request, context);
// ...
```

### 5. Message Acknowledgment, Rejection, Requeue and Fallback

A `IHandleRequests` does not have a return value. Instead, whether we acknowledge or reject the message on the queue or the record in a stream depends on whether the handler throws an exception, or terminates without an exception.

Terminates without an exception:

- **Success** → Message acknowledged (removed from broker)

If the Handler terminates with an exception, that exception determines whether we acknowledge, reject or requeue. A `ConfigurationException` is thrown by the Message Pump due to a failure to find a message mapper or handler.

You can throw a `DeferMessageAction` if you Handler encounters a transient fault (such as network or database failure that cannot be retried successfully)

- **ConfigurationException** - Message rejected, channel closed
- **DeferMessageAction** - Requeues the message
- **Unhandled Exception** → Message acknowledged (to prevent a poison pill message)

You can use a Polly resilience policy via [UseResiliencePipeline] to retry the handler, provide a circuit breaker, etc. This will kick in before your exception bubbles out to the Message Pump.

You can use a [FallbackPolicy] to catch an exception that bubbles out of your handler, and resilience policy, to call the `Fallback` method on your `IHandleRequests` derived class (you need to override this and provide an implementation). The `RequestContext` contains the original exception under the key of "FallbackPolicyHandler{TRequest}.CAUSE_OF_FALLBACK_EXCEPTION".

## Dispatcher Configuration

You configure the Dispatcher with `AddConsumers`, and it is the *Subscription* that carries the options the mechanics above depend on: **NoOfPerformers** sets how many Performers compete for a channel, **MessagePumpType** chooses between the Reactor and Proactor patterns, **TimeOut** governs how long a read waits, and **RequeueCount** with **RequeueDelay** control what happens after a handler fails.

Every option, with its defaults and a worked RabbitMQ example, is in [Subscriptions](/contents/DispatcherConfigurationReference.md#subscriptions).

## Dispatcher Lifecycle

### Startup

1. **Registration** → Subscriptions registered during application startup
2. **Channel Creation** → Channels created on broker (if configured)
3. **Performer Start** → Message pumps start polling for messages
4. **Connection** → Performers connect to broker and begin retrieving messages

### Runtime

Via the `Performers` orchestrated by the `Dispatcher`, Brighter continuously:

1. Polls for new messages (within timeout window)
2. Deserializes messages to requests
3. Dispatches to handlers via Command Processor
4. Acknowledges or rejects messages based on handler results

### Shutdown

1. **Shutdown Signal** → Application receives shutdown notification (e.g., SIGTERM)
2. **Stop Accepting** → Performers stop accepting new messages
3. **In-Flight Completion** → Current messages complete processing
4. **Channel Close** → Connections to broker closed gracefully
5. **Cleanup** → Resources released

## Dispatcher Error Handling

### Defer Message Action Exception

When a handler throws a `DeferMessageAction` exception:

1. **Requeue Decision** → Dispatcher checks `requeueCount`

   - If the message has been handled fewer than `requeueCount` times → Message requeued (with any configured delay)
   - If it has been handled `requeueCount` times → Message sent to DLQ (if configured) or rejected

### Dead Letter Queues (DLQ)

Configure a DLQ to capture failed messages. Dead letter queues are set per transport: with RabbitMQ, `RmqSubscription` takes the dead letter channel and its routing key. [Dead Letter Queue Configuration](/contents/ErrorHandlingOptions.md#dead-letter-queue-configuration) covers the other transports:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

var subscription = new RmqSubscription<MyCommand>(
    // ... other config
    deadLetterChannelName: new ChannelName("my.channel.dlq"),
    deadLetterRoutingKey: new RoutingKey("my.routing.key.dlq")
);
```

**Benefits:**

- Prevents message loss
- Allows investigation of failed messages
- Enables manual reprocessing

### Configuration Exception

When the pump receives a `ConfigurationException` it indicates that an expected Message Mapper or Handler could not be found or created.

### Other Exception

The message is acknowledged, to prevent a poison pill message from blocking further message consumption.

## Advanced Features

### Dynamic Message Deserialization

Use `getRequestType` callback for content-based routing:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

var subscription = new KafkaSubscription(
    new SubscriptionName("task.processor"),
    channelName: new ChannelName("tasks"),
    routingKey: new RoutingKey("task.*"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("task.created") => typeof(TaskCreated),
        var t when t == new CloudEventsType("task.updated") => typeof(TaskUpdated),
        _ => throw new ArgumentException($"Unknown message type: {message.Header.Type}")
    },
    messagePumpType: MessagePumpType.Proactor
);
```

**See [Dynamic Message Deserialization](DynamicMessageDeserialization.md) for details.**

### Agreement Dispatcher

Use Agreement Dispatcher for dynamic handler selection:

```csharp
using Paramore.Brighter;

registry.RegisterAsync<MyCommand>(
    (request, context) =>
    {
        var myCommand = request as MyCommand;
        if (myCommand?.Priority == "high")
            return [typeof(HighPriorityHandlerAsync)];

        return [typeof(StandardHandlerAsync)];
    },
    [typeof(HighPriorityHandlerAsync), typeof(StandardHandlerAsync)]
);
```

**See [Agreement Dispatcher](AgreementDispatcher.md) for details.**

### Competing Consumers

Scale message processing with multiple Performers:

```csharp
using Paramore.Brighter;

var subscription = new Subscription<MyCommand>(
    // ... other config
    noOfPerformers: 5,  // Five concurrent consumers
    messagePumpType: MessagePumpType.Proactor
);
```

**Considerations:**

- **Message Ordering** → We use a single threaded message pump which will preserve ordering for streams, but not queues.
- **Idempotency** → Use Inbox pattern for deduplication
- **Load Distribution** → Broker-dependent behavior

## Dispatcher Monitoring and Observability

### OpenTelemetry Integration

The Message Pump automatically creates spans for:

- Message retrieval from broker
- Message deserialization
- Handler dispatch
- Message acknowledgment

Configure the Dispatcher's instrumentation on its consumer options. The Command Processor takes its own, separately, from `AddBrighter`'s options:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;

services.AddConsumers(options =>
{
    // ... subscriptions and channel factory
    options.InstrumentationOptions = InstrumentationOptions.All;
});
```

**See [Telemetry](Telemetry.md) for details.**

### Health Checks

Monitor Dispatcher health:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

services.AddHealthChecks()
    .AddCheck<BrighterServiceActivatorHealthCheck>("brighter-dispatcher");
```

**See [Health Checks](HealthChecks.md) for details.**

### Control Bus

Control the Dispatcher at runtime:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.ServiceActivator.Ports.Commands;

// Stop specific channel
controlBusSender.Post(new ConfigurationCommand(
    ConfigurationCommandType.CM_STOPCHANNEL,
    new SubscriptionName("my.subscription")));

// Start specific channel
controlBusSender.Post(new ConfigurationCommand(
    ConfigurationCommandType.CM_STARTCHANNEL,
    new SubscriptionName("my.subscription")));
```

**See [Brighter Control API](BrighterControlAPI.md) for details.**

## Dispatcher Best Practices

### 1. Choose the Right Concurrency Model

- Use **Proactor** for high-volume, I/O-heavy workloads
- Use **Reactor** for low-latency requirements
- See [Reactor and Proactor](ReactorAndProactor.md) for detailed guidance

### 2. Configure Appropriate Timeouts

```csharp
using System;
using Paramore.Brighter;

var subscription = new Subscription<MyCommand>(
    // ... other config
    timeOut: TimeSpan.FromMilliseconds(200)  // Responsive but not too aggressive
);
```

Balance responsiveness vs. CPU usage.

### 3. Use Dead Letter Queues

Always configure DLQs for production, as [Dead Letter Queues](#dead-letter-queues-dlq) shows:

<!-- blockcheck: skip a fragment of the configuration shown whole in block 7 -->
```csharp
deadLetterChannelName: new ChannelName("my.channel.dlq")
```

### 4. Implement Idempotent Handlers

Messages may be delivered more than once. Use the Inbox pattern:

```csharp
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Attributes;

public class MyCommandHandler : RequestHandlerAsync<MyCommand>
{
    [UseInboxAsync(step: 0, contextKey: typeof(MyCommand), onceOnly: true)]
    public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken ct = default)
    {
        // Your idempotent logic here
        return await base.HandleAsync(command, ct);
    }
}
```

### 5. Monitor and Alert

- Configure health checks
- Monitor message processing latency
- Alert on DLQ message accumulation
- Track circuit breaker state

### 6. Scale with Competing Consumers

For high-volume scenarios, raise `noOfPerformers` on the subscription, as [Competing Consumers](#competing-consumers) shows:

<!-- blockcheck: skip a fragment of the configuration shown whole in block 10 -->
```csharp
noOfPerformers: 5  // Start with multiple performers
```

Test and adjust based on actual load.

## Relationship to ServiceActivator Assembly

The **Dispatcher** is the primary class in the `Brighter.ServiceActivator` assembly. Throughout Brighter documentation, we use the term "Dispatcher" to refer to the concept and the class. We use "ServiceActivator" only when referring to the assembly name: <!-- pagelint: allow-serviceactivator -->

- "Configure the Dispatcher to consume messages"
- "The Dispatcher uses Performers to retrieve messages"
- "The Brighter.ServiceActivator assembly contains the Dispatcher" <!-- pagelint: allow-serviceactivator -->
- "Configure the ServiceActivator to consume messages" (incorrect - too vague) <!-- pagelint: allow-serviceactivator -->

## Related Documentation

- **[Reactor and Proactor](ReactorAndProactor.md)** - Concurrency model details
- **[Brighter Basic Configuration](BrighterBasicConfiguration.md)** - Initial setup
- **[Configuring the Dispatcher](HowConfiguringTheDispatcherWorks.md)** - Advanced configuration
- **[Configuring Subscriptions](/contents/BrighterBasicConfiguration.md#configuring-the-dispatcher)** - Subscription patterns
- **[Dynamic Message Deserialization](DynamicMessageDeserialization.md)** - Content-based routing
- **[Agreement Dispatcher](AgreementDispatcher.md)** - Dynamic handler selection
- **[Health Checks](HealthChecks.md)** - Monitoring
- **[Telemetry](Telemetry.md)** - OpenTelemetry integration
- **[Control API](BrighterControlAPI.md)** - Runtime control

## Dispatcher Summary

The **Dispatcher** in the `Brighter.ServiceActivator` assembly is responsible for:
- Consuming messages from external brokers via Performers (message pumps)
- Converting messages to requests via message mappers
- Dispatching requests to handlers via the Command Processor
- Managing message acknowledgment and error handling
- Supporting both Reactor (blocking) and Proactor (non-blocking) patterns
- Enabling competing consumers for scalability
- Providing observability through OpenTelemetry and health checks

Understanding how the Dispatcher works helps you configure it appropriately for your workload and troubleshoot issues when they arise.
