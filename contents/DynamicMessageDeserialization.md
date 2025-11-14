# Dynamic Message Deserialization

## Overview

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

## Using CloudEvents Type for Routing

The most common approach for dynamic deserialization is using the **CloudEvents `type` attribute**. This provides a standard, interoperable way to identify message types.

### CloudEvents Type Routing Example

```csharp
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.taskstate"),
    channelName: new ChannelName("task.state"),
    routingKey: new RoutingKey("task.update"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("io.goparamore.task.created")
            => typeof(TaskCreated),
        var t when t == new CloudEventsType("io.goparamore.task.updated")
            => typeof(TaskUpdated),
        var t when t == new CloudEventsType("io.goparamore.task.completed")
            => typeof(TaskCompleted),
        _ => throw new ArgumentException(
            $"No type mapping found for message with CloudEvents type {message.Header.Type}",
            nameof(message)
        )
    },
    groupId: "kafka-TaskProcessor-Sample",
    timeOut: TimeSpan.FromMilliseconds(100),
    offsetDefault: AutoOffsetReset.Earliest,
    commitBatchSize: 5,
    sweepUncommittedOffsetsInterval: TimeSpan.FromMilliseconds(10000),
    messagePumpType: MessagePumpType.Proactor
);
```

**How it works:**

1. Message arrives on `task.update` channel
2. Brighter populates `message.Header.Type` (CloudEvents type attribute)
3. Callback matches CloudEvents type to Request type
4. Brighter deserializes message to correct Request type
5. Routes to appropriate handler based on type

### Setting CloudEvents Type on Publication

On the producer side, set the CloudEvents `type` in your Publication:

```csharp
var publications = new[]
{
    new Publication
    {
        Topic = new RoutingKey("task.update"),
        RequestType = typeof(TaskCreated),
        Source = new Uri("https://example.com/tasks"),
        Type = new CloudEventsType("io.goparamore.task.created")
    },
    new Publication
    {
        Topic = new RoutingKey("task.update"),
        RequestType = typeof(TaskUpdated),
        Source = new Uri("https://example.com/tasks"),
        Type = new CloudEventsType("io.goparamore.task.updated")
    },
    new Publication
    {
        Topic = new RoutingKey("task.update"),
        RequestType = typeof(TaskCompleted),
        Source = new Uri("https://example.com/tasks"),
        Type = new CloudEventsType("io.goparamore.task.completed")
    }
};

services.AddBrighter(options => { })
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new KafkaProducerRegistryFactory(
            new KafkaMessagingGatewayConfiguration { /* ... */ },
            publications
        ).Create();
    });
```

All three message types go to the same `task.update` topic, distinguished by their CloudEvents `type`.

## Custom Routing Strategies

While CloudEvents type is recommended, you can implement any routing strategy by examining message properties.

### Routing by Custom Header

```csharp
var subscription = new RmqSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message =>
    {
        // Route based on custom header
        if (message.Header.Bag.TryGetValue("OrderType", out var orderType))
        {
            return orderType switch
            {
                "Create" => typeof(CreateOrder),
                "Update" => typeof(UpdateOrder),
                "Cancel" => typeof(CancelOrder),
                _ => throw new ArgumentException($"Unknown order type: {orderType}")
            };
        }

        throw new ArgumentException("OrderType header not found");
    },
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

### Routing by Message Body Content

```csharp
var subscription = new AzureServiceBusSubscription(
    new SubscriptionName("paramore.example.events"),
    channelName: new ChannelName("events"),
    routingKey: new RoutingKey("events"),
    getRequestType: message =>
    {
        // Parse JSON to determine type
        using var doc = JsonDocument.Parse(message.Body.Value);
        var root = doc.RootElement;

        if (root.TryGetProperty("eventType", out var eventType))
        {
            return eventType.GetString() switch
            {
                "UserCreated" => typeof(UserCreated),
                "UserUpdated" => typeof(UserUpdated),
                "UserDeleted" => typeof(UserDeleted),
                _ => throw new ArgumentException($"Unknown event type: {eventType}")
            };
        }

        throw new ArgumentException("eventType property not found in message body");
    },
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

**Note:** Parsing the body for routing is less efficient than using headers, but can be useful when integrating with systems that don't support custom headers.

## Handler Routing

Once the request type is resolved, Brighter routes the message to the appropriate handler using its standard handler resolution:

### Standard 1-to-1 Handler Mapping

```csharp
services.AddBrighter(options => { })
    .AddConsumers(options =>
    {
        options.Subscriptions = new[] { subscription };
    })
    .Handlers(registry =>
    {
        // Each message type routes to its handler
        registry.Register<TaskCreated, TaskCreatedHandler>();
        registry.Register<TaskUpdated, TaskUpdatedHandler>();
        registry.Register<TaskCompleted, TaskCompletedHandler>();
    });
```

With dynamic deserialization:

1. Message arrives on `task.update` channel
2. `getRequestType` callback returns `typeof(TaskCreated)`
3. Brighter deserializes to `TaskCreated`
4. Routes to `TaskCreatedHandler`

### Integration with Agreement Dispatcher

Dynamic message deserialization can be combined with [Agreement Dispatcher](AgreementDispatcher.md) for even more flexible routing:

```csharp
// First: Resolve message type dynamically
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.created")
            => typeof(OrderCreated),
        _ => throw new ArgumentException($"Unknown type: {message.Header.Type}")
    },
    // ... other config
);

// Second: Dynamically choose handler based on content
services.AddBrighter(options => { })
    .AddConsumers(options => { options.Subscriptions = new[] { subscription }; })
    .Handlers(registry =>
    {
        registry.Register<OrderCreated>((request, context) =>
        {
            var order = request as OrderCreated;

            // Route to different handlers based on order properties
            if (order?.Country == "US")
                return [typeof(USOrderCreatedHandler)];
            if (order?.Country == "UK")
                return [typeof(UKOrderCreatedHandler)];

            return [typeof(DefaultOrderCreatedHandler)];
        },
        [
            typeof(USOrderCreatedHandler),
            typeof(UKOrderCreatedHandler),
            typeof(DefaultOrderCreatedHandler)
        ]);
    });
```

This provides two levels of routing:

1. **Dynamic deserialization**: CloudEvents type → `OrderCreated`
2. **Agreement dispatcher**: Order content → Country-specific handler

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

## Configuration Examples

### Kafka with CloudEvents Routing

```csharp
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.inventory"),
    channelName: new ChannelName("inventory.events"),
    routingKey: new RoutingKey("inventory"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.inventory.itemadded")
            => typeof(ItemAdded),
        var t when t == new CloudEventsType("com.example.inventory.itemremoved")
            => typeof(ItemRemoved),
        var t when t == new CloudEventsType("com.example.inventory.stockadjusted")
            => typeof(StockAdjusted),
        _ => throw new ArgumentException(
            $"Unmapped CloudEvents type: {message.Header.Type}",
            nameof(message)
        )
    },
    groupId: "inventory-processor",
    timeOut: TimeSpan.FromMilliseconds(100),
    offsetDefault: AutoOffsetReset.Earliest,
    messagePumpType: MessagePumpType.Proactor
);

services.AddBrighter(options => { })
    .AddConsumers(options =>
    {
        options.Subscriptions = new[] { subscription };
    })
    .Handlers(registry =>
    {
        registry.Register<ItemAdded, ItemAddedHandler>();
        registry.Register<ItemRemoved, ItemRemovedHandler>();
        registry.Register<StockAdjusted, StockAdjustedHandler>();
    })
    .AutoFromAssemblies([typeof(ItemAdded).Assembly]);
```

### RabbitMQ with CloudEvents Routing

```csharp
var subscription = new RmqSubscription(
    new SubscriptionName("paramore.example.notifications"),
    channelName: new ChannelName("notifications"),
    routingKey: new RoutingKey("notifications.#"),  // Wildcard routing
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.email.sent")
            => typeof(EmailSent),
        var t when t == new CloudEventsType("com.example.sms.sent")
            => typeof(SmsSent),
        var t when t == new CloudEventsType("com.example.push.sent")
            => typeof(PushNotificationSent),
        _ => throw new ArgumentException(
            $"Unknown notification type: {message.Header.Type}",
            nameof(message)
        )
    },
    timeOut: TimeSpan.FromMilliseconds(100),
    messagePumpType: MessagePumpType.Proactor
);
```

### AWS SQS with CloudEvents Routing

```csharp
var subscription = new SqsSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.placed")
            => typeof(OrderPlaced),
        var t when t == new CloudEventsType("com.example.order.shipped")
            => typeof(OrderShipped),
        var t when t == new CloudEventsType("com.example.order.delivered")
            => typeof(OrderDelivered),
        _ => throw new ArgumentException(
            $"Unrecognized order event: {message.Header.Type}",
            nameof(message)
        )
    },
    bufferSize: 10,
    timeOut: TimeSpan.FromMilliseconds(100),
    lockTimeout: TimeSpan.FromSeconds(30)
);
```

## Best Practices

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

## Error Handling

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

- [Cloud Events Support](CloudEventsSupport.md) - Understanding CloudEvents in Brighter
- [Agreement Dispatcher](AgreementDispatcher.md) - Dynamic handler selection
- [Default Message Mappers](DefaultMessageMappers.md) - Automatic message mapping
- [Routing](Routing.md) - Message routing in Brighter
- [Enterprise Integration Patterns: Datatype Channel](https://www.enterpriseintegrationpatterns.com/patterns/messaging/DatatypeChannel.html)

## Sample Code

Full working examples can be found in the Brighter samples:

- **Dynamic Deserialization**: `Brighter/samples/TaskQueue/` - Examples using CloudEvents type routing
- **Multi-type Channels**: `Brighter/samples/MultiBus/` - Multiple message types on shared infrastructure
