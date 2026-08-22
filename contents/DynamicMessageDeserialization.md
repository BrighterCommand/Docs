---
description: "Brighter supports dynamic type resolution, allowing you to route multiple message types through a single channel."
layout:
  description:
    visible: false
---

# Dynamic Message Deserialization

> **Explanation** · Applies to **Brighter V10**

## Dynamic Deserialization Overview

Brighter supports dynamic type resolution, allowing you to route multiple message types through a single channel. Instead of determining the message type at compile-time through generic parameters, you can use content-based routing where the message type is determined at runtime from metadata.

This enables more flexible messaging patterns while maintaining type safety once the message type is resolved.

## DataType Channel Pattern (Default)

By default, Brighter uses the **DataType Channel** pattern from [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/patterns/messaging/DatatypeChannel.html). In this pattern, each channel carries messages of a single, known type.

### DataType Channel Example

```csharp
// DataType Channel - One type per subscription
var subscription = new KafkaSubscription<TaskCreated>(
    new SubscriptionName("paramore.example.tasks"),
    channelName: new ChannelName("task.created"),
    routingKey: new RoutingKey("task.created"),
    groupId: "task-processor",
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

**Characteristics:**

- Simple and straightforward
- Type-safe at compile-time
- One handler per channel
- Recommended for most scenarios
- Requires separate channel per message type
- Cannot handle message type evolution on same channel

**When to use DataType Channel:**

- You have distinct topics/queues for each message type
- Message types are stable and don't evolve frequently
- You want compile-time type safety
- Simple producer-consumer patterns

This is Brighter's default and recommended approach for most scenarios.

## Dynamic Message Deserialization

Dynamic message deserialization allows multiple message types on the same channel by resolving the type at runtime based on message metadata.

### When to Use Dynamic Deserialization

Dynamic deserialization is useful when:

- **Multiple related message types** share a single topic/queue
- **Message type evolution** - new message types added to existing channels
- **CloudEvents-based routing** - using the CloudEvents `type` attribute
- **Content-based routing** - routing decisions based on message content
- **Shared infrastructure** - multiple teams publishing to common topics

### How It Works

Instead of specifying the type via a generic parameter, you provide a `getRequestType` callback in your Subscription that examines the message and returns the appropriate type:

```csharp
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.tasks"),
    channelName: new ChannelName("task.state"),
    routingKey: new RoutingKey("task.update"),
    getRequestType: message => /* return type based on message */,
    groupId: "task-processor",
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

## Performance Considerations

Dynamic message deserialization has a small performance overhead compared to DataType Channel:

### Runtime Type Resolution

**DataType Channel (Compile-Time):**

- Type known at compile-time via generic parameter
- Message mapper pipeline pre-built
- Minimal runtime overhead

**Dynamic Deserialization (Runtime):**

- Type determined by executing callback function
- Message mapper pipeline built on first use per type
- Pipeline cached for subsequent messages of same type

## Dynamic Deserialization Best Practices

### 1. Use CloudEvents Type for Routing

CloudEvents provides a standard, interoperable way to identify message types:

```csharp
// Good - Standard CloudEvents type
getRequestType: message => message.Header.Type switch
{
    var t when t == new CloudEventsType("com.example.order.created")
        => typeof(OrderCreated),
    // ...
}

// Bad - Custom header parsing
getRequestType: message =>
{
    var type = message.Header.Bag["MessageType"];
    // Non-standard approach
}
```

### 2. Provide Comprehensive Type Mappings

Handle all expected message types and provide a clear error for unmapped types:

```csharp
// Good - Clear error message
getRequestType: message => message.Header.Type switch
{
    var t when t == new CloudEventsType("com.example.task.created")
        => typeof(TaskCreated),
    var t when t == new CloudEventsType("com.example.task.updated")
        => typeof(TaskUpdated),
    _ => throw new ArgumentException(
        $"No type mapping found for CloudEvents type '{message.Header.Type}'. " +
        $"Supported types: com.example.task.created, com.example.task.updated",
        nameof(message)
    )
}

// Bad - Generic error
getRequestType: message => message.Header.Type switch
{
    var t when t == new CloudEventsType("com.example.task.created")
        => typeof(TaskCreated),
    _ => throw new Exception("Unknown message type")
}
```

### 3. Use Meaningful CloudEvents Types

Follow reverse-DNS naming for CloudEvents types:

```csharp
// Good - Reverse DNS, hierarchical
new CloudEventsType("io.goparamore.task.created")
new CloudEventsType("com.example.inventory.item.added")

// Bad - Too generic
new CloudEventsType("TaskCreated")

// Bad - Not following conventions
new CloudEventsType("CREATE_TASK")
```

See [CloudEvents Support](CloudEventsSupport.md) for more on CloudEvents type naming.

### 4. Consider DataType Channel First

Start with DataType Channel (one type per topic) unless you have a specific need for dynamic deserialization:

```csharp
// Good - Simple DataType Channel when possible
var subscription = new KafkaSubscription<TaskCreated>(
    // ...
);
```

Only use dynamic when needed:

- Multiple related types on same channel
- Message evolution scenarios
- CloudEvents-based integration

### 5. Cache Performance-Critical Paths

If performance is critical, pre-warm the pipeline cache:

```csharp
// Send one message of each type at startup to warm caches
await _commandProcessor.PublishAsync(new TaskCreated { /* ... */ });
await _commandProcessor.PublishAsync(new TaskUpdated { /* ... */ });
await _commandProcessor.PublishAsync(new TaskCompleted { /* ... */ });

// Subsequent messages will use cached pipelines
```

### 6. Document Type Mappings

Document which CloudEvents types map to which Request types:

```csharp
/// <summary>
/// Subscription for task state changes.
/// Supports the following CloudEvents types:
/// - io.goparamore.task.created → TaskCreated
/// - io.goparamore.task.updated → TaskUpdated
/// - io.goparamore.task.completed → TaskCompleted
/// </summary>
var subscription = new KafkaSubscription(
    // ...
    getRequestType: message => message.Header.Type switch
    {
        // ...
    }
);
```

## Comparison: DataType Channel vs Dynamic Deserialization

| Aspect | DataType Channel | Dynamic Deserialization |
|--------|------------------|------------------------|
| **Type Resolution** | Compile-time (generic) | Runtime (callback) |
| **Performance** | Fastest | Fast |
| **Flexibility** | One type per channel | Multiple types per channel |
| **Type Safety** | Compile-time | Runtime (after resolution) |
| **Setup Complexity** | Simple | Moderate |
| **Message Evolution** | Requires new channels | Same channel |
| **CloudEvents Integration** | Not needed | Natural fit |
| **When to Use** | Default, most scenarios | Multiple types, evolution |

## Dynamic Deserialization Error Handling

Handle unmapped message types gracefully:

```csharp
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.tasks"),
    channelName: new ChannelName("task.events"),
    routingKey: new RoutingKey("task.events"),
    getRequestType: message =>
    {
        try
        {
            return message.Header.Type switch
            {
                var t when t == new CloudEventsType("io.goparamore.task.created")
                    => typeof(TaskCreated),
                var t when t == new CloudEventsType("io.goparamore.task.updated")
                    => typeof(TaskUpdated),
                _ => throw new ArgumentException(
                    $"Unmapped CloudEvents type: {message.Header.Type}. " +
                    $"Message ID: {message.Id}",
                    nameof(message)
                )
            };
        }
        catch (Exception ex)
        {
            // Log the error with full message context
            _logger.LogError(ex,
                "Failed to resolve message type. MessageId: {MessageId}, Type: {Type}",
                message.Id,
                message.Header.Type);
            throw;
        }
    },
    // ... other config
);
```

Failed messages will go to the dead letter queue based on your failure handling configuration.

## Further Reading

- [Routing Multiple Message Types](/contents/RoutingMultipleMessageTypes.md) - Routing several types down one channel
- [Cloud Events Support](CloudEventsSupport.md) - Understanding CloudEvents in Brighter
- [Agreement Dispatcher](AgreementDispatcher.md) - Dynamic handler selection
- [Default Message Mappers](DefaultMessageMappers.md) - Automatic message mapping
- [Routing](Routing.md) - Message routing in Brighter
- [Enterprise Integration Patterns: Datatype Channel](https://www.enterpriseintegrationpatterns.com/patterns/messaging/DatatypeChannel.html)

## Dynamic Deserialization Sample Code

Full working examples can be found in the Brighter samples:

- **Dynamic Deserialization**: `Brighter/samples/TaskQueue/` - Examples using CloudEvents type routing
- **Multi-type Channels**: `Brighter/samples/MultiBus/` - Multiple message types on shared infrastructure
