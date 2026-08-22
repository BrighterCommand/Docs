---
description: "CloudEvents defines both required and optional attributes for events."
layout:
  description:
    visible: false
---

# CloudEvents Reference

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Cloud Events Support](/contents/CloudEventsSupport.md)

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

## CloudEvents Across Transports

Brighter maps CloudEvents to transport-specific formats automatically. The transport layer handles the conversion based on the protocol's capabilities.

### RabbitMQ (AMQP 0-9-1)

RabbitMQ uses **binary mode** with CloudEvents mapped to message headers:

```csharp
// ...
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
// ...
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
// ...
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
// ...
var publication = new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};
```

See: [HTTP Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/http-protocol-binding.md) (Azure Service Bus follows HTTP binding)

## Further Reading

- [Cloud Events Support](/contents/CloudEventsSupport.md) - Content modes, publication and mappers
- [CloudEvents Specification](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md) - Full specification
- [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) - Routing on the CloudEvents type
