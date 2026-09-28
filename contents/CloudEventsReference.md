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

Your message mapper chooses the content mode, not the transport. The default
`JsonMessageMapper<T>` writes **binary mode**: the attributes travel beside the body, and the body
is your request. `CloudEventJsonMessageMapper<T>` writes **structured mode**: the body is the whole
CloudEvents envelope, with your request as its `data` — see
[Default Message Mappers](/contents/DefaultMessageMappers.md). Either way, each transport writes the
attributes where its protocol has room for them, as below.

### RabbitMQ (AMQP 0-9-1)

RabbitMQ carries the attributes as message headers, prefixed `cloudEvents_`:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

var publication = new RmqPublication<OrderCreated>
{
    Topic = new RoutingKey("orders"),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// Headers will include:
// cloudEvents_id, cloudEvents_source, cloudEvents_type, cloudEvents_specversion, cloudEvents_time
```

The content type travels in the AMQP `content-type` property rather than a header. The
`Paramore.Brighter.MessagingGateway.RMQ.Sync` package writes the same headers with the prefix
`cloudEvents:`.

See: [AMQP Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/amqp-protocol-binding.md)

### Kafka

Kafka carries the attributes as record headers, prefixed `ce_`:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

var publication = new KafkaPublication<OrderCreated>
{
    Topic = new RoutingKey("orders"),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// Headers will include:
// ce_id, ce_source, ce_type, ce_specversion, ce_time, content-type
```

The partition key belongs to each message rather than to the publication. The default mapper takes
it from the request context, and Kafka writes it as the record's key:

```csharp
using Paramore.Brighter;

var context = new RequestContext();
context.Bag[RequestContextBagNames.PartitionKey] = "customer-12345";  // the Kafka record key

await commandProcessor.PostAsync(new OrderCreated(), context);
```

See: [Kafka Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/kafka-protocol-binding.md)
and [Using the Context Bag](/contents/UsingTheContextBag.md)

### AWS SNS/SQS

Brighter writes the CloudEvents attributes together, as a JSON object in one message attribute
named `cloudeventheaders`, and the body is your request:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AWSSQS;

var publication = new SnsPublication<OrderCreated>
{
    Topic = new RoutingKey("orders"),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// The cloudeventheaders attribute holds:
// specversion, type, souce, time, datacontenttype, dataschema, baggage,
// and subject, dataref, traceparent and tracestate when they are set
```

The source is written under the key `souce`, as shown: a Brighter consumer reads it back, and a
consumer of your own has to look for that spelling. This is [BrighterCommand/Brighter#4458](https://github.com/BrighterCommand/Brighter/issues/4458). To put the whole envelope in the body, use
`CloudEventJsonMessageMapper<T>`.

### Azure Service Bus

Azure Service Bus carries the attributes as application properties, prefixed `cloudEvents:`:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;

var publication = new AzureServiceBusPublication<OrderCreated>
{
    Topic = new RoutingKey("orders"),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
};

// Application properties will include:
// cloudEvents:id, cloudEvents:source, cloudEvents:type, cloudEvents:specversion,
// cloudEvents:time, cloudEvents:contenttype
```

See: [AMQP Protocol Binding for CloudEvents](https://github.com/cloudevents/spec/blob/main/cloudevents/bindings/amqp-protocol-binding.md) (Azure Service Bus speaks AMQP 1.0)

## Further Reading

- [Cloud Events Support](/contents/CloudEventsSupport.md) - Content modes, publication and mappers
- [CloudEvents Specification](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md) - Full specification
- [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) - Routing on the CloudEvents type
