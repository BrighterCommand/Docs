---
description: "The most common approach for dynamic deserialization is using the CloudEvents type attribute."
layout:
  description:
    visible: false
---

# Routing Multiple Message Types

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)

## Using CloudEvents Type for Routing

The most common approach for dynamic deserialization is using the **CloudEvents `type` attribute**. This provides a standard, interoperable way to identify message types.

### CloudEvents Type Routing Example

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.Kafka;
using Confluent.Kafka;

// ...
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
        _ => throw new InvalidMessageAction(
            $"No type mapping found for message with CloudEvents type {message.Header.Type}"
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

For an unsupported type, throw `InvalidMessageAction` to request rejection as `Unacceptable`. The rejection destination depends on the transport and configuration; see [Dynamic Deserialization Error Handling](/contents/DynamicMessageDeserialization.md#dynamic-deserialization-error-handling). An ordinary `ArgumentException` is logged and acknowledged instead.

### Setting CloudEvents Type on Publication

On the producer side, set the CloudEvents `type` in your Publication:

```csharp
// ...
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
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

// ...
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
                _ => throw new InvalidMessageAction($"Unknown order type: {orderType}")
            };
        }

        throw new InvalidMessageAction("OrderType header not found");
    },
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

### Routing by Message Body Content

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;
using System.Text.Json;

// ...
var subscription = new AzureServiceBusSubscription(
    new SubscriptionName("paramore.example.events"),
    channelName: new ChannelName("events"),
    routingKey: new RoutingKey("events"),
    getRequestType: message =>
    {
        try
        {
            using var doc = JsonDocument.Parse(message.Body.Value);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("eventType", out var eventType) ||
                eventType.ValueKind != JsonValueKind.String)
            {
                throw new InvalidMessageAction(
                    "Message body must be an object with a string eventType property");
            }

            return eventType.GetString() switch
            {
                "UserCreated" => typeof(UserCreated),
                "UserUpdated" => typeof(UserUpdated),
                "UserDeleted" => typeof(UserDeleted),
                _ => throw new InvalidMessageAction($"Unknown event type: {eventType}")
            };
        }
        catch (JsonException ex)
        {
            throw new InvalidMessageAction("Message body is not valid JSON", ex);
        }
    },
    timeOut: TimeSpan.FromMilliseconds(100)
);
```

**Note:** Parsing the body for routing is less efficient than using headers, but can be useful when integrating with systems that don't support custom headers.

## Handler Routing

Once the request type is resolved, Brighter routes the message to the appropriate handler using its standard handler resolution:

### Standard 1-to-1 Handler Mapping

```csharp
// ...
services.AddConsumers(options =>
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
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;

// First: Resolve message type dynamically
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.created")
            => typeof(OrderCreated),
        _ => throw new InvalidMessageAction($"Unknown type: {message.Header.Type}")
    },
    groupId: "order-processor"
);

// Second: Dynamically choose handler based on content
services.AddConsumers(options => { options.Subscriptions = new[] { subscription }; })
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

## Routing Configuration Examples

### Kafka with CloudEvents Routing

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.Kafka;
using Confluent.Kafka;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;

// ...
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
        _ => throw new InvalidMessageAction(
            $"Unmapped CloudEvents type: {message.Header.Type}"
        )
    },
    groupId: "inventory-processor",
    timeOut: TimeSpan.FromMilliseconds(100),
    offsetDefault: AutoOffsetReset.Earliest,
    messagePumpType: MessagePumpType.Proactor
);

services.AddConsumers(options =>
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
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

// ...
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
        _ => throw new InvalidMessageAction(
            $"Unknown notification type: {message.Header.Type}"
        )
    },
    timeOut: TimeSpan.FromMilliseconds(100),
    messagePumpType: MessagePumpType.Proactor
);
```

### AWS SQS with CloudEvents Routing

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.MessagingGateway.AWSSQS;

// ...
var subscription = new SqsSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    channelType: ChannelType.PubSub,
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.placed")
            => typeof(OrderPlaced),
        var t when t == new CloudEventsType("com.example.order.shipped")
            => typeof(OrderShipped),
        var t when t == new CloudEventsType("com.example.order.delivered")
            => typeof(OrderDelivered),
        _ => throw new InvalidMessageAction(
            $"Unrecognized order event: {message.Header.Type}"
        )
    },
    bufferSize: 10,
    timeOut: TimeSpan.FromMilliseconds(100),
    queueAttributes: new SqsAttributes(lockTimeout: TimeSpan.FromSeconds(30))
);
```

## Further Reading

- [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) - Why a channel would carry more than one type
- [Cloud Events Support](/contents/CloudEventsSupport.md) - Setting the CloudEvents type on a publication
- [Agreement Dispatcher](/contents/AgreementDispatcher.md) - Dynamic handler selection
- [Routing](/contents/Routing.md) - Message routing in Brighter
