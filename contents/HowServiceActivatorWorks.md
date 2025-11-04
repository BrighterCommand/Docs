# How The Dispatcher Works

The **Dispatcher** is the component in the `Brighter.ServiceActivator` assembly that consumes messages from external message brokers and dispatches them to your registered handlers. While the Command Processor handles in-process request dispatching, the Dispatcher handles external message consumption and routing.

## Overview

When you configure Brighter to consume messages from external brokers (using `AddConsumers()`), you're setting up the Dispatcher. The Dispatcher:

1. **Listens** to configured channels on external message brokers
2. **Retrieves** messages using a Performer (message pump)
3. **Deserializes** messages into requests using message mappers
4. **Dispatches** requests to registered handlers via the Command Processor
5. **Acknowledges** or rejects messages based on handler success

## Architecture

```
External Message Broker
         ↓
    [Performer] (Message Pump - Reactor or Proactor)
         ↓
   [Dispatcher] (ServiceActivator assembly)
         ↓
  [Message Mapper] (Deserialize)
         ↓
 [Command Processor] (Handler Pipeline)
         ↓
    [Your Handler]
```

## The Performer (Message Pump)

The **Performer** is the message pump that retrieves messages from external brokers. It's a core component of the Dispatcher and operates in one of two concurrency models:

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

### 1. Performer Retrieves Message

The Performer polls the configured channel for new messages:

```csharp
// Reactor (blocking)
Message message = channel.Receive(timeOut);

// Proactor (non-blocking)
Message message = await channel.ReceiveAsync(timeOut, cancellationToken);
```

### 2. Message Deserialization

The Dispatcher uses the registered message mapper to convert the `Message` into a request object:

```csharp
// V10: Default mapper used automatically if no custom mapper registered
MyCommand command = messageMapper.MapToRequest(message);
```

### 3. Request Context Creation

The Dispatcher creates a `RequestContext` that includes:
- The original `Message` (via `OriginatingMessage` property)
- Span for tracing (OpenTelemetry integration)
- Custom headers and metadata
- Partition key (if applicable)

### 4. Handler Dispatch

The request is dispatched to the Command Processor, which executes the handler pipeline:

```csharp
// Reactor
commandProcessor.Send(command);

// Proactor
await commandProcessor.SendAsync(command, cancellationToken);
```

### 5. Message Acknowledgment

Based on the handler result:
- **Success** → Message acknowledged (removed from broker)
- **Unhandled Exception** → Message rejected or requeued
- **Requeue** → Message requeued for later processing

## Configuration

### Basic Dispatcher Configuration

Configure the Dispatcher when setting up your service:

```csharp
services.AddBrighter(...)
    .AddConsumers(options =>
    {
        options.AddSubscription<MyCommand>(
            new Subscription<MyCommand>(
                new SubscriptionName("my.subscription"),
                channelName: new ChannelName("my.channel"),
                routingKey: new RoutingKey("my.routing.key"),
                messagePumpType: MessagePumpType.Proactor,  // Choose concurrency model
                timeOut: TimeSpan.FromMilliseconds(200),
                makeChannels: OnMissingChannel.Create,
                requeueCount: 3,
                requeueDelayInMilliseconds: 1000,
                noOfPerformers: 1  // Number of concurrent message pumps
            )
        );
    });
```

### Key Configuration Options

#### noOfPerformers
Controls how many concurrent Performers (message pumps) run for this subscription:

```csharp
noOfPerformers: 3  // Three concurrent pumps reading from the same channel
```

**Notes:**
- Each Performer is single-threaded
- Multiple Performers enable competing consumers pattern
- Useful for high-volume scenarios
- Consider message ordering requirements

#### messagePumpType
Chooses between Reactor and Proactor patterns:

```csharp
messagePumpType: MessagePumpType.Reactor   // Blocking I/O
messagePumpType: MessagePumpType.Proactor  // Non-blocking I/O
```

**See [Reactor and Proactor](ReactorAndProactor.md) for guidance on choosing.**

#### timeOut
How long the Performer waits for a message before polling again:

```csharp
timeOut: TimeSpan.FromMilliseconds(200)
```

**Trade-offs:**
- **Shorter timeout** → More responsive to shutdown, higher CPU usage
- **Longer timeout** → Lower CPU usage, slower shutdown response

#### requeueCount and requeueDelayInMilliseconds
Control retry behavior on handler failure:

```csharp
requeueCount: 3,                           // Retry up to 3 times
requeueDelayInMilliseconds: 1000,          // Wait 1 second between retries
```

## Dispatcher Lifecycle

### Startup

1. **Registration** → Subscriptions registered during application startup
2. **Channel Creation** → Channels created on broker (if configured)
3. **Performer Start** → Message pumps start polling for messages
4. **Connection** → Performers connect to broker and begin retrieving messages

### Runtime

The Dispatcher continuously:
1. Polls for new messages (within timeout window)
2. Deserializes messages to requests
3. Dispatches to handlers via Command Processor
4. Acknowledges or rejects messages based on handler results
5. Tracks failures for circuit breaking (see Sweeper Circuit Breaking)

### Shutdown

1. **Shutdown Signal** → Application receives shutdown notification (e.g., SIGTERM)
2. **Stop Accepting** → Performers stop accepting new messages
3. **In-Flight Completion** → Current messages complete processing
4. **Channel Close** → Connections to broker closed gracefully
5. **Cleanup** → Resources released

## Error Handling

### Unhandled Exceptions

When a handler throws an unhandled exception:

1. **Requeue Decision** → Dispatcher checks `requeueCount`
   - If retries remain → Message requeued with delay
   - If retries exhausted → Message sent to DLQ (if configured) or rejected

2. **Circuit Breaking** → Sweeper tracks failures per topic
   - If threshold exceeded → Circuit opens for that topic
   - Other topics continue operating normally

### Dead Letter Queues (DLQ)

Configure a DLQ to capture failed messages:

```csharp
var subscription = new Subscription<MyCommand>(
    // ... other config
    deadLetterChannelName: new ChannelName("my.channel.dlq"),
    deadLetterRoutingKey: new RoutingKey("my.routing.key.dlq")
);
```

**Benefits:**
- Prevents message loss
- Allows investigation of failed messages
- Enables manual reprocessing

## Advanced Features

### Dynamic Message Deserialization

Use `getRequestType` callback for content-based routing:

```csharp
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
registry.RegisterAsync<MyCommand>(
    (request, context) =>
    {
        var myCommand = request as MyCommand;
        if (myCommand?.Priority == "high")
            return new[] { typeof(HighPriorityHandlerAsync) };

        return new[] { typeof(StandardHandlerAsync) };
    },
    new[] { typeof(HighPriorityHandlerAsync), typeof(StandardHandlerAsync) }
);
```

**See [Agreement Dispatcher](AgreementDispatcher.md) for details.**

### Competing Consumers

Scale message processing with multiple Performers:

```csharp
var subscription = new Subscription<MyCommand>(
    // ... other config
    noOfPerformers: 5,  // Five concurrent consumers
    messagePumpType: MessagePumpType.Proactor
);
```

**Considerations:**
- **Message Ordering** → Not guaranteed across multiple consumers
- **Idempotency** → Use Inbox pattern for deduplication
- **Load Distribution** → Broker-dependent behavior

## Monitoring and Observability

### OpenTelemetry Integration

The Dispatcher automatically creates spans for:
- Message retrieval from broker
- Message deserialization
- Handler dispatch
- Message acknowledgment

Configure instrumentation:

```csharp
services.AddBrighter(options =>
{
    options.InstrumentationOptions = InstrumentationOptions.All;
})
```

**See [Telemetry](Telemetry.md) for details.**

### Health Checks

Monitor Dispatcher health:

```csharp
services.AddHealthChecks()
    .AddCheck<BrighterServiceActivatorHealthCheck>("brighter-dispatcher");
```

**See [Health Checks](HealthChecks.md) for details.**

### Control Bus

Control the Dispatcher at runtime:

```csharp
// Stop specific channel
controlBusSender.Send(new ConfigurationCommand(ConfigurationCommandType.StopChannel)
{
    SubscriptionName = "my.subscription"
});

// Start specific channel
controlBusSender.Send(new ConfigurationCommand(ConfigurationCommandType.StartChannel)
{
    SubscriptionName = "my.subscription"
});
```

**See [Brighter Control API](BrighterControlAPI.md) for details.**

## Best Practices

### 1. Choose the Right Concurrency Model

- Use **Proactor** for high-volume, I/O-heavy workloads
- Use **Reactor** for low-latency requirements
- See [Reactor and Proactor](ReactorAndProactor.md) for detailed guidance

### 2. Configure Appropriate Timeouts

```csharp
timeOut: TimeSpan.FromMilliseconds(200)  // Responsive but not too aggressive
```

Balance responsiveness vs. CPU usage.

### 3. Use Dead Letter Queues

Always configure DLQs for production:

```csharp
deadLetterChannelName: new ChannelName("my.channel.dlq")
```

### 4. Implement Idempotent Handlers

Messages may be delivered more than once. Use the Inbox pattern:

```csharp
[UseInbox(step: 0, contextKey: typeof(MyCommand), onceOnly: true)]
public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken ct)
{
    // Your idempotent logic here
    return await base.HandleAsync(command, ct);
}
```

### 5. Monitor and Alert

- Configure health checks
- Monitor message processing latency
- Alert on DLQ message accumulation
- Track circuit breaker state

### 6. Scale with Competing Consumers

For high-volume scenarios:

```csharp
noOfPerformers: 5  // Start with multiple performers
```

Test and adjust based on actual load.

## Relationship to ServiceActivator Assembly

The **Dispatcher** is the primary class in the `Brighter.ServiceActivator` assembly. Throughout Brighter documentation, we use the term "Dispatcher" to refer to the concept and the class. We use "ServiceActivator" only when referring to the assembly name:

- ✅ "Configure the Dispatcher to consume messages"
- ✅ "The Dispatcher uses Performers to retrieve messages"
- ✅ "The Brighter.ServiceActivator assembly contains the Dispatcher"
- ❌ "Configure the ServiceActivator to consume messages" (incorrect - too vague)

## Related Documentation

- **[Reactor and Proactor](ReactorAndProactor.md)** - Concurrency model details
- **[Brighter Basic Configuration](BrighterBasicConfiguration.md)** - Initial setup
- **[Configuring the Dispatcher](HowConfiguringTheDispatcherWorks.md)** - Advanced configuration
- **[Subscriptions and Topology](BrighterSubscriptionsAndTopology.md)** - Subscription patterns
- **[Dynamic Message Deserialization](DynamicMessageDeserialization.md)** - Content-based routing
- **[Agreement Dispatcher](AgreementDispatcher.md)** - Dynamic handler selection
- **[Health Checks](HealthChecks.md)** - Monitoring
- **[Telemetry](Telemetry.md)** - OpenTelemetry integration
- **[Control API](BrighterControlAPI.md)** - Runtime control

## Summary

The **Dispatcher** in the `Brighter.ServiceActivator` assembly is responsible for:
- Consuming messages from external brokers via Performers (message pumps)
- Converting messages to requests via message mappers
- Dispatching requests to handlers via the Command Processor
- Managing message acknowledgment and error handling
- Supporting both Reactor (blocking) and Proactor (non-blocking) patterns
- Enabling competing consumers for scalability
- Providing observability through OpenTelemetry and health checks

Understanding how the Dispatcher works helps you configure it appropriately for your workload and troubleshoot issues when they arise.
