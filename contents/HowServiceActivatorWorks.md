# How The Dispatcher Works

The **Dispatcher** is the component in the `Brighter.ServiceActivator` assembly orchestrates your `Performers`. Each `Performer` is a single-thread that runs a `MessagePump`. The `MessagePump` consumes messages from streams or queues, calls a `IAmAMessageMapper` or `IAmAMessageMapperAsync` to map them to a C# type and then dispatches that type to your registered `IHandleRequests` or `IHandleRequestsAsync`. The Command Processor handles in-process request dispatching, and the Dispatcher orchestrates the threads that provide external message consumption and routing.

## Overview

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

## Architecture

```
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
// Reactor (blocking)
var request = TranslateMessage(message, context);
```

```csharp

// V10: Default mapper used automatically if no custom mapper registered
// Reactor (blocking)
var request = await TranslateMessage(message, context);
```

### 3. Request Context Creation

The Message Pump creates a `RequestContext` that includes:

- The original `Message` (via `OriginatingMessage` property)
- Span for tracing (OpenTelemetry integration)
- Channel Name
- When the request started

```csharp

RequestContext context = InitRequestContext(span, message);

```

### 4. Handler Dispatch

The request is dispatched to the Command Processor, which executes the handler pipeline:

```csharp
// Reactor
RequestContext context = InitRequestContext(span, message);
```

```csharp
// Proactor
await InvokeDispatchRequest(request, message, context);
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

## Configuration

### Basic Dispatcher Configuration

You configure the Dispatcher when setting up your service, using `AddConsumers`:

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

## Error Handling

### Defer Message Action Exception

When a handler throws a `DeferMessageAction` exception:

1. **Requeue Decision** → Dispatcher checks `requeueCount`

   - If retries remain → Message requeued (with any configured delay)
   - If retries exhausted → Message sent to DLQ (if configured) or rejected

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

### Configuration Exception

When the pump receives a `ConfigurationException` it indicates that an expected Message Mapper or Handler could not be found or created.

### Other Exception

The message is acknowledged, to prevent a poison pill message from blocking further message consumption.

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

- **Message Ordering** → We use a single threaded message pump which will preserve ordering for streams, but not queues.
- **Idempotency** → Use Inbox pattern for deduplication
- **Load Distribution** → Broker-dependent behavior

## Monitoring and Observability

### OpenTelemetry Integration

The Message Pump automatically creates spans for:

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

- "Configure the Dispatcher to consume messages"
- "The Dispatcher uses Performers to retrieve messages"
- "The Brighter.ServiceActivator assembly contains the Dispatcher"
- "Configure the ServiceActivator to consume messages" (incorrect - too vague)

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
