---
description: "Brighter provides comprehensive OpenTelemetry integration for distributed tracing across message boundaries, enabling end-to-end observability in distributed systems."
layout:
  description:
    visible: false
---

# Telemetry

> **Reference** · Applies to **Brighter V10**

Brighter provides comprehensive OpenTelemetry integration for distributed tracing across message boundaries, enabling end-to-end observability in distributed systems.

## OpenTelemetry Semantic Conventions

**V10 introduces support for [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/concepts/semantic-conventions/)**, replacing the custom conventions used in V9.

- **[OTel Semantic Conventions for Messaging](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/)**: Standard span names and attributes for messaging operations
- **[W3C TraceContext](https://w3c.github.io/trace-context/)**: Standard context propagation across service boundaries
- **[CloudEvents Integration](https://opentelemetry.io/docs/specs/semconv/cloudevents/cloudevents-spans/)**: Trace propagation via CloudEvents `traceparent` and `tracestate` headers
- **Configurable Instrumentation**: Fine-grained control over what attributes are recorded
- **Comprehensive Coverage**: Tracing for Command Processor, Dispatcher, Outbox, Inbox, and Transform pipelines

---

## Enabling Brighter's Spans

Brighter records spans only when a tracer, an `IAmABrighterTracer`, is registered in your container,
and `AddBrighter()` does not register one. `AddBrighterInstrumentation()`, from the
`Paramore.Brighter.Extensions.Diagnostics` package, registers it and adds its `ActivitySource`,
`Paramore.Brighter`, to your tracer provider:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Paramore.Brighter.Extensions.Diagnostics;

var services = new ServiceCollection();

services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddBrighterInstrumentation()
        .AddOtlpExporter());
```

`AddOpenTelemetry()` comes from `OpenTelemetry.Extensions.Hosting`, and `AddOtlpExporter()` from
`OpenTelemetry.Exporter.OpenTelemetryProtocol`. Without a registered tracer,
`AddSource("paramore.brighter")` listens to a source nothing writes to, and no Brighter span
appears. [Configuring OpenTelemetry](/contents/ConfiguringOpenTelemetry.md) shows the setup for a
producer and a consumer.

**The Command Processor also needs an external bus.** At 10.7.0 it is given the tracer only when
you configure producers with `AddProducers`; with none, `Send`, `Publish` and their async forms
record no span, even with the tracer registered. This is reported as [#4510](https://github.com/BrighterCommand/Brighter/issues/4510).

---

## Configurable Instrumentation

V10 provides fine-grained control over which attributes are recorded to optimize performance and reduce costs.

### Instrumentation Options

`InstrumentationOptions` is a flags enum, so you combine the attributes you want with `|`. Set it on the Command Processor through `AddBrighter`, and on producers through `AddProducers`. Left unset, it is `None`: the spans are still created, with no Brighter attributes on them:

| Flag | Records |
|---|---|
| `RequestInformation` | Request ID, type and operation; on messages, the CloudEvents ID, type, source and subject |
| `RequestBody` | The request body as JSON; on messages, the message body (expensive) |
| `RequestContext` | Nothing at 10.7.0: no span reads this flag |
| `Messaging` | Messaging attributes: destination, partition, message ID and type, body size, headers |
| `DatabaseInformation` | Database attributes for Outbox and Inbox operations |
| `ClamCheck` | Claim check operations (the member is spelled `ClamCheck`) |
| `Brighter` | Brighter's handler instrumentation |
| `All` | Every flag above |
| `None` | Nothing |

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;

var services = new ServiceCollection();

services.AddBrighter(options =>
    {
        // Command Processor spans: request ID, type and operation
        options.InstrumentationOptions = InstrumentationOptions.RequestInformation;
    })
    .AddProducers(configure =>
    {
        // Producer spans: add the messaging attributes, but not the request body
        configure.InstrumentationOptions = InstrumentationOptions.RequestInformation
                                         | InstrumentationOptions.Messaging;
    });
```

**Best Practice**: Only enable `RequestBody` in development or debugging scenarios, as it can significantly increase trace size and cost.

---

## Command Processor Spans

When Brighter operates as a Command Processor, it creates spans for each operation. A span is named
for the request type's name, without its namespace:

### Span Names and Operations

| Operation | Span Name | Kind | Description |
|-----------|-----------|------|-------------|
| `send` | `<request type> send` | Internal | Command routed to single handler |
| `create` | `<request type> create` | Internal | An event published to its handlers; the parent of one `publish` span per handler |
| `publish` | `<request type> publish` | Internal | Event dispatched to one of its handlers |
| `deposit` | `<request type> deposit` | Internal | Request transformed and stored in Outbox |
| `scheduler` | `<request type> scheduler` | Internal | Request handed to the scheduler, with a delay or a time |
| `create` (clear) | `paramore.brighter.clear_messages create` | Producer | Messages cleared from the Outbox; the parent of a `clear` span |
| `publish` (messaging) | `<routing key> publish` | Producer | Message sent to the broker |

### Example: Send Operation

```csharp
using Paramore.Brighter;

// Creates span: "ProcessOrderCommand send"
await commandProcessor.SendAsync(new ProcessOrderCommand { OrderId = 123 });
```

### Command Processor Attributes

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `paramore.brighter.request.id` | string | Request ID | `"01a0e7cf-3c53-75f1-8566-3fd80badd0a1"` |
| `paramore.brighter.request.type` | string | Request type's name | `"ProcessOrderCommand"` |
| `paramore.brighter.request.body` | string | Request as JSON | `{"orderId":123, ...}` |
| `paramore.brighter.operation` | string | Operation performed | `"send"` |
| `messaging.operation.type` | string | Operation performed | `"send"` |

### Adding Custom Span Attributes

At 10.7.0 Brighter copies nothing from the request context onto a span. To record an attribute of
your own, set it on `Activity.Current` in your handler: while the handler runs, that is the Command
Processor's span for the request.

```csharp
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public class ProcessOrderHandler : RequestHandlerAsync<ProcessOrderCommand>
{
    public override async Task<ProcessOrderCommand> HandleAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // Recorded on the "ProcessOrderCommand send" span
        Activity.Current?.SetTag("app.order_id", command.OrderId);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

### Handler Pipeline Events

Brighter records an event on the span for each handler entered in the pipeline, named for the
handler:

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `paramore.brighter.handler.name` | string | Handler type's name | `"ProcessOrderHandler"` |
| `paramore.brighter.handler.type` | string | Sync or async | `"async"` |
| `paramore.brighter.is_sink` | bool | Final handler in chain | `true` |

---

## Dispatcher (Consumer) Spans

When Brighter operates as a Dispatcher (message consumer), each message pump creates spans named for
the routing key it reads, whichever transport it reads from:

### Span Names

| Span Name | Kind | Description |
|-----------|------|-------------|
| `<routing key> begin` | Consumer | The message pump starting |
| `<routing key> receive` | Consumer | One read of the channel, a root span, including reads that find no message |
| `<routing key> process` | Consumer | One message handled; its parent is the producer's span, carried in the message |

### Example Flow

Measured with the in-memory transport, a published event and its handler:

```text
order.placed publish (Producer, the sending service)
  └─> order.placed process (Consumer)
      └─> OrderPlaced create (Internal)
          └─> OrderPlaced publish (Internal)
              └─> Handler events: OrderPlacedHandler
```

### Message Attributes

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `messaging.system` | string | Always `internal_bus` on pump spans at 10.7.0, whichever transport the pump reads | `"internal_bus"` |
| `messaging.destination.name` | string | Routing key | `"order.placed"` |
| `messaging.operation.type` | string | Operation type | `"receive"`, `"process"` |
| `messaging.message.id` | string | Message ID | `"01a0e7cf-3c70-7089-9e82-425719101515"` |
| `messaging.message.type` | string | Message type | `"MT_EVENT"` |
| `messaging.destination.partition.id` | string | Partition key | `"customer-12345"` |
| `messaging.message.body.size` | int | Payload size in bytes | `66` |
| `paramore.brighter.handled_count` | int | Times the message has been handled | `0` |

---

## Outbox Tracing

Outbox operations create child spans for database operations, each named
`<operation> <database name> <table>`:

### Deposit Operation

```text
OrderPlaced deposit (Internal)
  └─> Mapper and transform events: JsonMessageMapper`1, CloudEventsTransformer
  └─> add.message outbox requests (Client)
```

### Clear Operation

```text
paramore.brighter.clear_messages create (Producer)
  └─> retrieve.message outbox requests (Client)
  └─> paramore.brighter.clear_messages clear (Producer)
      └─> order.placed publish (Producer)
  └─> count.outstanding_messages outbox requests (Client)

order.placed settle (Producer), a trace of its own
  └─> mark_as_dispatched.outstanding_messages outbox requests (Client)
```

### Database Span Attributes

Outbox and Inbox database operations follow [OTel Database Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/database/database-spans/):

| Attribute | Description | Example |
|-----------|-------------|---------|
| `db.system` | Database type; `brighter` for the in-memory stores | `"postgresql"` |
| `db.name` | The configuration's database name, not the server's | `"Brighter"` |
| `db.table` | Table | `"outbox"` |
| `db.operation` | Operation | `"add.message"`, `"retrieve.message"`, `"mark_as_dispatched.outstanding_messages"` |
| `db.operation.name` | SQL verb, on relational stores | `"INSERT"` |
| `db.query.text` | SQL, on relational stores | `INSERT INTO {0} (...) VALUES (...)` |

The PostgreSQL examples above are from `PostgreSqlOutbox`; `db.name` is the `databaseName` of its
`RelationalDatabaseConfiguration`, which defaults to `Brighter`.

---

## Inbox Tracing

Inbox operations create child spans for deduplication checks, under the span of the request the
Inbox guards:

```text
order.placed process (Consumer)
  └─> OrderPlaced create (Internal)
      └─> OrderPlaced publish (Internal)
          └─> message.exists inbox requests (Client)
          └─> add.message inbox requests (Client)
```

### Inbox Operations

| Operation | Span Name | Description |
|-----------|-----------|-------------|
| Check | `message.exists <database name> <table>` | Check if message already processed |
| Add | `add.message <database name> <table>` | Record message as processed |

---

## Transform Pipeline Tracing

The Claim Check transform creates a span for each call to its luggage store, named
`<operation> <provider> <bucket>`, with `claim_check.*` attributes: `claim_check.operation`, and,
with `ClamCheck` set, `claim_check.provider`, `claim_check.bucket_name`, `claim_check.id` and
`claim_check.content_lenght` (the attribute is spelled that way).

### Claim Check

```text
BigEvent deposit (Internal)
  └─> store.message <provider> <bucket> (Client)
```

### Retrieve Claim

```text
big.event process (Consumer)
  └─> retrieve.message <provider> <bucket> (Client)
  └─> delete.message <provider> <bucket> (Client)
```

The `delete.message` span appears because `[RetrieveClaim]` defaults to `retain: false`. The trees
were measured with the in-memory luggage store, whose provider and bucket are both `in-memory`.

---

## W3C TraceContext Propagation

Brighter automatically propagates trace context across service boundaries using [W3C TraceContext](https://w3c.github.io/trace-context/).

### How It Works

1. **Producer**: Brighter writes the producer span's `traceparent` and `tracestate` into the message
2. **Consumer**: Brighter reads them, and the `process` span becomes a child of the producer's span, in the same trace

### Message Headers

Each transport names the headers its own way. Measured on RabbitMQ
(`Paramore.Brighter.MessagingGateway.RMQ.Async`) and Kafka:

```text
RabbitMQ:
  cloudEvents_traceparent: 00-3519372db2bb5402dc1be3b11c7fc4e5-8675f8104bc40dbe-01
  cloudevents_tracestate: congo=t61rcWkgMzE

Kafka:
  ce_traceparent: 00-c37cfd2c01adac953978b1abfb2c8d4c-3ae8fe73da7b454f-01
  ce_tracestate: congo=t61rcWkgMzE
```

RabbitMQ also writes the trace state under `cloudevents_:tracestate`, an older spelling kept for
consumers that still read it. Brighter injects through OpenTelemetry's default propagator, which
the OpenTelemetry SDK sets up; without the SDK, nothing is written.

### Integration with ASP.NET

Brighter participates in existing traces. When called from an ASP.NET controller, the Command Processor span becomes a child of the ASP.NET request span:

```text
ASP.NET Request: "POST /orders"
  └─> Command Processor: "ProcessOrderCommand send"
      └─> Handler events: OrderHandler
```

---

## CloudEvents Integration

When using [CloudEvents](CloudEventsSupport.md), Brighter propagates trace context via the [CloudEvents Distributed Tracing Extension](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md).

### CloudEvents Attributes

CloudEvents adds alternative attribute names following [CloudEvents Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/cloudevents/cloudevents-spans/):

| Messaging Convention | CloudEvents Convention | Value |
|---------------------|------------------------|-------|
| `messaging.message.id` | `cloudevents.event_id` | Message ID |
| N/A | `cloudevents.event_source` | Event source |
| N/A | `cloudevents.event_type` | Event type |
| N/A | `cloudevents.event_subject` | Event subject |
| N/A | `cloudevents.event_spec_version` | `"1.0"` |

### Enabling CloudEvents Conventions

There is no separate switch for the CloudEvents attributes. Brighter records them on message spans whenever `InstrumentationOptions.RequestInformation` is set, and records the messaging attributes when `InstrumentationOptions.Messaging` is set:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;

var services = new ServiceCollection();

services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.InstrumentationOptions = InstrumentationOptions.RequestInformation
                                         | InstrumentationOptions.Messaging;
    });
```

Set both flags and both sets of attributes will be recorded.

---

## Telemetry Best Practices

1. **Start with Minimal Instrumentation**: Enable `RequestInformation` and `Messaging`, and leave out expensive flags like `RequestBody`

2. **Use Sampling**: Configure sampling in production to reduce costs:
   ```csharp
   using Microsoft.Extensions.DependencyInjection;
   using OpenTelemetry.Trace;
   using Paramore.Brighter.Extensions.Diagnostics;

   var services = new ServiceCollection();

   services.AddOpenTelemetry()
       .WithTracing(tracing => tracing
           .AddBrighterInstrumentation()
           .SetSampler(new TraceIdRatioBasedSampler(0.1))); // Sample 10% of traces
   ```

3. **Add Custom Attributes Judiciously**: Only add attributes that are essential for debugging and analysis

4. **Monitor Trace Costs**: Large payloads and high cardinality attributes can significantly increase observability costs

5. **Use Structured Logging**: Combine tracing with structured logging for comprehensive observability

6. **Enable CloudEvents for Cross-Organization Tracing**: If exchanging messages with external systems, use CloudEvents for standard trace propagation

7. **Configure Appropriate Exporters**: Use OTLP for flexibility, or native exporters for specific backends

8. **Test Trace Propagation**: Verify that traces flow correctly across service boundaries in development

---

## Migration from V9

### Changed Span Names

| V9 Span Name | V10 Span Name |
|--------------|---------------|
| Custom handler names | `<request type> <operation>` |
| `Outbox.Add` | `<operation> <database name> <table>`, such as `add.message outbox requests` |
| Transport-specific names | `<routing key> publish`, and `<routing key> begin/receive/process` on the consumer |

### Changed Attributes

V9 used custom attribute names. V10 uses OTel standard conventions:

| V9 Attribute | V10 Attribute |
|--------------|---------------|
| Custom attributes | `paramore.brighter.*` and OTel standard attributes |
| No standard messaging attributes | `messaging.*` attributes following OTel conventions |

### Action Required

1. **Update Dashboards**: Update queries and visualizations to use V10 span names and attributes
2. **Update Alerts**: Update alert rules based on new span structure
3. **Review Instrumentation Options**: Configure which attributes to record based on your needs
4. **Test Trace Propagation**: Verify distributed traces work correctly with V10

---

## Telemetry Troubleshooting

### Traces Not Appearing

**Problem**: No traces appear in your observability backend.

**Solutions**:

- Verify Brighter's tracer is registered: `.AddBrighterInstrumentation()`, as in [Enabling Brighter's Spans](#enabling-brighters-spans)
- Check exporter configuration and endpoint
- Ensure services can reach the exporter endpoint
- Check firewall rules

### Incomplete Traces

**Problem**: Traces are missing child spans or appear disconnected.

**Solutions**:

- Verify `traceparent` header is being propagated
- Check that all services have OpenTelemetry configured
- Ensure consistent trace propagation format (W3C TraceContext)
- Review CloudEvents configuration if using CloudEvents

### High Trace Costs

**Problem**: Observability costs are too high.

**Solutions**:

- Leave `RequestBody` out of `InstrumentationOptions`
- Reduce sampling rate: `.SetSampler(new TraceIdRatioBasedSampler(0.1))`
- Disable unnecessary attribute collection
- Use tail-based sampling to only keep interesting traces: `Paramore.Brighter.Extensions.Diagnostics` adds `SetTailSampler()` to the tracer provider builder

### Missing Attributes

**Problem**: Expected attributes are not appearing on spans.

**Solutions**:

- Check `Activity.IsAllDataRequested` is true (controlled by sampling)
- Verify instrumentation options are configured correctly
- Set attributes of your own on `Activity.Current` in the handler, as in [Adding Custom Span Attributes](#adding-custom-span-attributes): Brighter copies nothing from the request context

---

## Further Reading

- [Configuring OpenTelemetry](/contents/ConfiguringOpenTelemetry.md) - Wiring up the SDK, exporters and a worked producer/consumer example
- [OpenTelemetry Semantic Conventions for Messaging](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/)
- [W3C TraceContext Specification](https://w3c.github.io/trace-context/)
- [CloudEvents Distributed Tracing Extension](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Brighter CloudEvents Support](CloudEventsSupport.md)
- [Brighter Request Context](UsingTheContextBag.md)
