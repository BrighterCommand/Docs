# Cloud Events Support

> **How-to** · Applies to **Brighter V10**

## What are CloudEvents?

[CloudEvents](https://cloudevents.io/) is a [CNCF](https://www.cncf.io/) specification for describing event data in a common, standardized way. Without a standard for metadata, each messaging framework uses its own set of metadata fields, making it difficult to write interoperable code that works across different messaging systems.

CloudEvents solves this problem by providing a [specification](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md) that defines:

- A standard set of metadata attributes for events
- Multiple content modes (binary and structured)
- Protocol bindings for various transport protocols

Brighter V10 provides full CloudEvents specification support, making it easy to build interoperable, event-driven systems.

## CloudEvents Attributes

CloudEvents defines both required and optional attributes for events. Brighter supports all CloudEvents attributes.

### Required Attributes

These attributes must be present in every CloudEvent:

| Attribute | Type | Description |
|-----------|------|-------------|
| **id** | String | Unique identifier for the event (Brighter message ID) |
| **source** | URI-reference | Context in which the event occurred |
| **type** | String | Type of event (e.g., "com.example.order.created") |
| **specversion** | String | CloudEvents specification version (e.g., "1.0") |

### Important Optional Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| **datacontenttype** | String | Content type of the data (e.g., "application/json") |
| **dataschema** | URI | Schema that the data adheres to |
| **subject** | String | Subject of the event in the context of the source |
| **time** | Timestamp | When the event occurred |

### Extension Attributes

CloudEvents supports extension attributes for additional metadata:

| Extension | Specification | Purpose |
|-----------|--------------|---------|
| **traceparent** | [Distributed Tracing](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md) | W3C Trace Context for distributed tracing |
| **tracestate** | [Distributed Tracing](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md) | Vendor-specific trace information |
| **dataref** | [DataRef](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/dataref.md) | Reference to data stored elsewhere (Claim Check pattern) |

## Content Modes

CloudEvents can be transmitted in two modes, and Brighter supports both:

### Binary-Mode (Recommended)

In binary-mode, CloudEvents attributes are mapped to protocol headers, and the event data is placed in the message body.

**When to use binary mode:**
- The transport protocol supports headers (RabbitMQ, Kafka, AMQP)
- You want efficient serialization
- You want to inspect event metadata without deserializing the body

**Example RabbitMQ message with binary CloudEvents:**
```
Headers:
  ce_id: "a89b61a2-5c5c-4d7e-8b8f-2e0f9c1d3e4f"
  ce_source: "https://example.com/orders"
  ce_type: "com.example.order.created"
  ce_specversion: "1.0"
  ce_datacontenttype: "application/json"
  ce_time: "2025-01-02T10:30:00Z"

Body:
  {"orderId": "12345", "customerId": "67890", "total": 99.99}
```

### Structured Content Mode

In structured mode, both CloudEvents attributes and data are placed in the message body as a JSON object.

**When to use structured mode:**
- The transport has insufficient header support (AWS SNS/SQS)
- You need to preserve all metadata in a single payload
- The protocol doesn't support custom headers well

**Example SNS/SQS message with structured CloudEvents:**
```json
{
  "specversion": "1.0",
  "type": "com.example.order.created",
  "source": "https://example.com/orders",
  "id": "a89b61a2-5c5c-4d7e-8b8f-2e0f9c1d3e4f",
  "time": "2025-01-02T10:30:00Z",
  "datacontenttype": "application/json",
  "data": {
    "orderId": "12345",
    "customerId": "67890",
    "total": 99.99
  }
}
```

## Setting CloudEvents in Publication

CloudEvents properties are configured in the `Publication` when you register producers. The Publication is passed to your message mapper, which can access these properties.

### Basic Publication with CloudEvents

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("com.example.orders"),
    RequestType = typeof(OrderCreated),

    // CloudEvents attributes
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created"),
    Subject = "order/12345"
};

services.AddBrighter(options => { /* ... */ })
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new RabbitMQProducerRegistryFactory(
            new RmqMessagingGatewayConnection { /* ... */ },
            new[] { publication }
        ).Create();
    });
```

### Additional CloudEvents Properties

You can set additional CloudEvents properties in the Publication:

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("com.example.orders"),
    RequestType = typeof(OrderCreated),

    // Required CloudEvents
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created"),

    // Optional CloudEvents
    DataSchema = new Uri("https://example.com/schemas/order-created.json"),
    Subject = "order/12345",
};
```

## Using CloudEvents in Message Mappers

In V10, Brighter passes the `Publication` to your message mapper, giving you access to CloudEvents properties. However, most users won't need custom mappers anymore thanks to [default message mappers](DefaultMessageMappers.md).

### Default Mappers Handle CloudEvents Automatically

Brighter V10 includes default message mappers that automatically handle CloudEvents:

- **`JsonMessageMapper<T>`**: Uses **binary mode** CloudEvents (headers)
- **`CloudEventJsonMessageMapper<T>`**: Uses **structured mode** CloudEvents (JSON body)

If you're using default mappers (recommended), CloudEvents are handled automatically based on your Publication configuration.

### Custom Mapper with CloudEvents

If you need a custom mapper (e.g., for transforms), you can access CloudEvents from the Publication:

```csharp
public class OrderCreatedMapper : IAmAMessageMapper<OrderCreated>
{
    public IRequestContext? Context { get; set; }

    public Message MapToMessage(OrderCreated request, Publication publication)
    {
        // CloudEvents properties come from the Publication
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT,
            source: publication.Source,           // CloudEvents source
            type: publication.Type                 // CloudEvents type
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    public OrderCreated MapToRequest(Message message)
    {
        // CloudEvents attributes available in message header, available via RequestContext's OriginatingMessage property
        var orderCreated = JsonSerializer.Deserialize<OrderCreated>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return orderCreated!;
    }
}
```

## CloudEvents Across Transports

Brighter maps CloudEvents to transport-specific formats automatically. The transport layer handles the conversion based on the protocol's capabilities.

### RabbitMQ (AMQP 0-9-1)

RabbitMQ uses **binary mode** with CloudEvents mapped to message headers:

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// Headers will include:
// ce_id, ce_source, ce_type, ce_specversion, ce_datacontenttype
```

See: [AMQP Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/amqp-protocol-binding.md)

### Kafka

Kafka uses **binary mode** with CloudEvents in message headers:

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created"),
    PartitionKey = "customer-12345"  // Kafka partition key
};
```

See: [Kafka Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/kafka-protocol-binding.md)

### AWS SNS/SQS

AWS SNS/SQS has limited header support, so Brighter uses **structured mode**:

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// The entire CloudEvents envelope (including data) is in the message body
```

### Azure Service Bus

Azure Service Bus supports **binary mode** with headers:

```csharp
var publication = new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};
```

See: [HTTP Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/http-protocol-binding.md) (Azure Service Bus follows HTTP binding)

## CloudEvents Type for Message Routing

One powerful use of CloudEvents is content-based routing using the `type` attribute. This allows multiple message types on a single channel.

See [Dynamic Message Deserialization](DynamicMessageDeserialization.md) for details on using CloudEvents type for routing.

**Example:**
```csharp
new KafkaSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.created")
            => typeof(OrderCreated),
        var t when t == new CloudEventsType("com.example.order.updated")
            => typeof(OrderUpdated),
        _ => throw new ArgumentException($"Unknown CloudEvents type: {message.Header.Type}")
    },
    // ... other config
)
```

## OpenTelemetry Integration

CloudEvents includes extensions for distributed tracing that integrate with OpenTelemetry:

- **traceparent**: W3C Trace Context propagation
- **tracestate**: Vendor-specific trace information

Brighter automatically propagates OpenTelemetry trace context using CloudEvents distributed tracing extensions. See [OpenTelemetry Integration](Telemetry.md) for details.

```csharp
public override MyCommand Handle(MyCommand command)
{
    // Access the current span from context
    Context.Span.SetAttribute("custom.attribute", "value");

    // Trace context is automatically propagated via CloudEvents headers
    // when you publish/post messages

    return base.Handle(command);
}
```

## Claim Check Pattern with DataRef

CloudEvents supports the [DataRef extension](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/dataref.md) for the Claim Check pattern, where large payloads are stored externally and only a reference is included in the message.

```csharp
public class LargeOrderMapper : IAmAMessageMapper<LargeOrder>
{
    [ClaimCheck(step: 0, thresholdInKb: 256)]
    public Message MapToMessage(LargeOrder request, Publication publication)
    {
        // If the message exceeds 256KB, Brighter will:
        // 1. Store the payload in a luggage store (e.g., S3)
        // 2. Set the CloudEvents dataref attribute to the storage location
        // 3. Send a lightweight message with just the reference

        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    [RetrieveClaim(step: 0)]
    public LargeOrder MapToRequest(Message message)
    {
        // Brighter automatically retrieves the payload using the dataref
        var order = JsonSerializer.Deserialize<LargeOrder>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return order!;
    }
}
```

See [Claim Check Pattern](ClaimCheck.md) for more details.

### Breaking Changes

When migrating to V10, be aware of these CloudEvents-related breaking changes:

1. **Message ID**: Changed from `Guid` to `string`
   ```csharp
   // V9
   var messageId = Guid.NewGuid();

   // V10
   var messageId = Guid.NewGuid().ToString(); // or any unique string
   ```

2. **Correlation ID**: Changed from `Guid` to `string`
   ```csharp
   // V9
   var correlationId = Guid.NewGuid();

   // V10
   var correlationId = Guid.NewGuid().ToString(); // or any unique string
   ```

3. **Publication passed to mapper**: Message mappers now receive `Publication` parameter
   ```csharp
   // V9
   public Message MapToMessage(OrderCreated request)

   // V10
   public Message MapToMessage(OrderCreated request, Publication publication)
   ```

See the [V10 Migration Guide](V10MigrationGuide.md) for complete migration instructions.

## Best Practices

### 1. Choose the Right Content Mode

- Use **binary mode** for protocols with header support (RabbitMQ, Kafka, Azure Service Bus)
- Use **structured mode** for protocols with limited headers (AWS SNS/SQS)
- Brighter selects the appropriate mode automatically based on the transport

### 2. Use Meaningful CloudEvents Type

The `type` attribute should follow reverse-DNS naming:

```csharp
// Good - Reverse DNS, specific
Type = new CloudEventsType("com.example.orders.order.created")

// Bad - Too generic
Type = new CloudEventsType("OrderCreated")

// Bad - Not following conventions
Type = new CloudEventsType("CREATE_ORDER")
```

### 3. Include Source Context

The `source` should uniquely identify where the event originated:

```csharp
// Good - Specific service and environment
Source = new Uri("https://prod.example.com/orders-service")

// Good - Resource-specific
Source = new Uri("/orders/12345")

// Bad - Too vague
Source = new Uri("https://example.com")
```

### 4. Use Subject for Fine-Grained Routing

The `subject` provides additional context in the scope of the source:

```csharp
// For an order event
Source = new Uri("https://example.com/orders")
Subject = "order/12345"

// For a customer event
Source = new Uri("https://example.com/customers")
Subject = "customer/67890/profile"
```

### 5. Leverage Default Mappers

In most cases, use Brighter's default message mappers rather than implementing custom mappers:

```csharp
// Recommended - Use default mapper
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies([typeof(OrderCreated).Assembly]);

// No explicit mapper needed! CloudEvents handled automatically.
```

Only create custom mappers when you need [transform pipelines](DefaultMessageMappers.md).

## Further Reading

- [CloudEvents Primer](https://github.com/cloudevents/spec/blob/main/cloudevents/primer.md) - Introduction to CloudEvents
- [CloudEvents Specification](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md) - Full specification
- [Default Message Mappers](DefaultMessageMappers.md) - Using automatic message mapping
- [Dynamic Message Deserialization](DynamicMessageDeserialization.md) - Content-based routing with CloudEvents
- [OpenTelemetry Integration](Telemetry.md) - Distributed tracing with CloudEvents
- [Claim Check Pattern](ClaimCheck.md) - Large message handling with DataRef
- [Dapr CloudEvents](https://docs.dapr.io/developing-applications/building-blocks/pubsub/pubsub-cloudevents/) - How Dapr uses CloudEvents
