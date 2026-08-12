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

## Configurable Instrumentation

V10 provides fine-grained control over which attributes are recorded to optimize performance and reduce costs.

### Instrumentation Options

Configure instrumentation using `BrighterInstrumentation`:

```csharp
using Paramore.Brighter.Observability;

var instrumentation = BrighterInstrumentation.InstrumentationOptions;

// Control Command Processor attributes
instrumentation.CommandProcessorInstrumentationOptions = new InstrumentationOptions
{
    RecordRequestInformation = true,      // Request ID, type, operation
    RecordRequestBody = false,            // Request body as JSON (expensive)
    RecordRequestContext = true           // Custom span context attributes
};

// Control Message attributes (for Producers and Consumers)
instrumentation.MessagingInstrumentationOptions = new InstrumentationOptions
{
    RecordMessageInformation = true,      // Message ID, channel, partition
    RecordMessageBody = false,            // Message payload (expensive)
    RecordMessageHeaders = true,          // Message headers
    RecordServerInformation = true        // Broker address
};
```

**Best Practice**: Only enable `RecordRequestBody` and `RecordMessageBody` in development or debugging scenarios, as they can significantly increase trace size and cost.

---

## Command Processor Spans

When Brighter operates as a Command Processor, it creates spans for each operation:

### Span Names and Operations

| Operation | Span Name | Kind | Description |
|-----------|-----------|------|-------------|
| `send` | `<request type> send` | Internal | Command routed to single handler |
| `publish` | `<request type> publish` | Internal | Event routed to multiple handlers |
| `deposit` | `<request type> deposit` | Internal | Request transformed and stored in Outbox |
| `clear` | `clear` | Internal | Messages dispatched from Outbox to broker |
| `create` | `<channel> create` | Producer | Single message sent to broker |
| `publish` (messaging) | `<channel> publish` | Producer | Batch of messages sent to broker |

### Example: Send Operation

```csharp
// Creates span: "MyNamespace.ProcessOrderCommand send"
await commandProcessor.SendAsync(new ProcessOrderCommand { OrderId = 123 });
```

### Command Processor Attributes

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `paramore.brighter.requestid` | string | Request ID | `"1234-5678-9012-3456"` |
| `paramore.brighter.requestids` | string | Batch: comma-separated IDs | `"1234..., 2345..."` |
| `paramore.brighter.requesttype` | string | Full type name | `"MyNamespace.MyCommand"` |
| `paramore.brighter.request_body` | string | Request as JSON | `{"orderId": 123}` |
| `paramore.brighter.operation` | string | Operation performed | `"send"` |
| `paramore.brighter.spancontext.*` | varies | Custom context attributes | `spancontext.userid: "1234"` |

### Adding Custom Span Attributes

You can add custom attributes via the Request Context:

```csharp
var context = new RequestContext();
context.Bag["paramore.brighter.spancontext.userid"] = userId;
context.Bag["paramore.brighter.spancontext.tenantid"] = tenantId;

await commandProcessor.SendAsync(command, context);
```

Any context bag entries starting with `paramore.brighter.spancontext.` will be added as span attributes.

### Handler Pipeline Events

Brighter records an event for each handler entered in the pipeline:

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `paramore.brighter.handlername` | string | Full handler type name | `"MyNamespace.MyHandler"` |
| `paramore.brighter.handlertype` | string | Sync or async | `"async"` |
| `paramore.brighter.is_sink` | bool | Final handler in chain | `true` |

---

## Dispatcher (Consumer) Spans

When Brighter operates as a Dispatcher (message consumer), it creates spans for each message received:

### Span Names

| Transport Type | Span Name | Kind | Description |
|---------------|-----------|------|-------------|
| Pull-based (Kafka) | `<channel> receive` | Consumer | Message pulled from broker |
| Push-based (RabbitMQ) | `<channel> process` | Consumer | Message pushed by broker |

### Example Flow

```text
Dispatcher Span: "task.commands receive" (Consumer)
  └─> Message Translation (sibling)
  └─> Command Processor Span: "ProcessTaskCommand send" (Internal)
      └─> Handler Events
```

### Message Attributes

| Attribute | Type | Description | Example |
|-----------|------|-------------|---------|
| `messaging.system` | string | Broker type | `"rabbitmq"`, `"kafka"` |
| `messaging.destination` | string | Channel name | `"task.commands"` |
| `messaging.operation` | string | Operation type | `"receive"`, `"process"` |
| `messaging.message_id` | string | Message ID | `"msg-1234"` |
| `messaging.destination.partition.id` | string | Partition ID (Kafka) | `"0"` |
| `messaging.message.body.size` | int | Payload size in bytes | `1024` |
| `server.address` | string | Broker address | `"localhost:5672"` |

---

## Outbox Tracing

Outbox operations create child spans for database operations:

### Deposit Operation

```text
deposit span (Internal)
  └─> Transform pipeline spans
  └─> Outbox add span (Database)
```

### Clear Operation

```text
create/clear span (Internal)
  └─> Outbox get span (Database)
  └─> Produce message span (Producer)
  └─> Outbox mark dispatched span (Database)
```

### Database Span Attributes

Outbox and Inbox database operations follow [OTel Database Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/database/database-spans/):

| Attribute | Description | Example |
|-----------|-------------|---------|
| `db.system` | Database type | `"mysql"`, `"postgresql"` |
| `db.name` | Database name | `"myapp"` |
| `db.operation` | Operation type | `"outbox_add"`, `"outbox_get"` |

---

## Inbox Tracing

Inbox operations create child spans for deduplication checks:

```text
Dispatcher receive span (Consumer)
  └─> Message translation
      └─> Inbox check span (Database)
  └─> Command Processor send span (Internal)
```

### Inbox Operations

| Operation | Span Name | Description |
|-----------|-----------|-------------|
| Check | `inbox_check` | Check if message already processed |
| Add | `inbox_add` | Record message as processed |

---

## Transform Pipeline Tracing

Transform operations (Claim Check, Compression, Encryption) create child spans for external calls:

### Claim Check (S3 Example)

```text
deposit span (Internal)
  └─> ClaimCheck transform span
      └─> S3 put object span (HTTP Client)
          Attributes: s3.bucket, s3.key, http.request.method
```

### Retrieve Claim

```text
Message translation span
  └─> RetrieveClaim transform span
      └─> S3 get object span (HTTP Client)
          Attributes: s3.bucket, s3.key, http.request.method
```

External call spans follow their respective OTel conventions:
- **S3**: [Object Storage Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/object-stores/s3/)
- **HTTP**: [HTTP Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/http/)

---

## W3C TraceContext Propagation

Brighter automatically propagates trace context across service boundaries using [W3C TraceContext](https://w3c.github.io/trace-context/) headers.

### How It Works

1. **Producer**: Brighter injects `traceparent` and `tracestate` into message headers
2. **Consumer**: Brighter extracts `traceparent` and `tracestate` to continue the trace

### Message Headers

```text
traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01
tracestate: congo=t61rcWkgMzE
```

### Integration with ASP.NET

Brighter participates in existing traces. When called from an ASP.NET controller, the Command Processor span becomes a child of the ASP.NET request span:

```text
ASP.NET Request: "POST /orders"
  └─> Command Processor: "ProcessOrderCommand send"
      └─> Handler: OrderHandler
      └─> Publish: "OrderCreatedEvent publish"
          └─> Outbox add
```

---

## CloudEvents Integration

When using [CloudEvents](CloudEventsSupport.md), Brighter propagates trace context via the [CloudEvents Distributed Tracing Extension](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md).

### CloudEvents Attributes

CloudEvents adds alternative attribute names following [CloudEvents Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/cloudevents/cloudevents-spans/):

| Messaging Convention | CloudEvents Convention | Value |
|---------------------|------------------------|-------|
| `messaging.message_id` | `cloudevents.event_id` | Message ID |
| `messaging.destination` | `cloudevents.event_source` | Event source |
| N/A | `cloudevents.event_type` | Event type |

### Enabling CloudEvents Conventions

```csharp
var instrumentation = BrighterInstrumentation.InstrumentationOptions;

instrumentation.MessagingInstrumentationOptions.UseCloudEventsConventionsAttributes = true;
instrumentation.MessagingInstrumentationOptions.UseMessagingSemanticConventionsAttributes = true;
```

You can enable both conventions simultaneously, and both sets of attributes will be recorded.

---

## Telemetry Best Practices

1. **Start with Minimal Instrumentation**: Enable `RecordRequestInformation` and `RecordMessageInformation`, disable expensive options like `RecordRequestBody`

2. **Use Sampling**: Configure sampling in production to reduce costs:
   ```csharp
   .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of traces
   ```

3. **Add Custom Attributes Judiciously**: Only add context attributes that are essential for debugging and analysis

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
| `Outbox.Add` | Follows database conventions |
| Transport-specific names | `<channel> create/publish/receive/process` |

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

- Verify Activity Source is registered: `.AddSource("paramore.brighter")`
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

- Disable `RecordRequestBody` and `RecordMessageBody`
- Reduce sampling rate: `.SetSampler(new TraceIdRatioBasedSampler(0.1))`
- Disable unnecessary attribute collection
- Use tail-based sampling to only keep interesting traces

### Missing Attributes

**Problem**: Expected attributes are not appearing on spans.

**Solutions**:

- Check `Activity.IsAllDataRequested` is true (controlled by sampling)
- Verify instrumentation options are configured correctly
- Ensure custom context attributes start with `paramore.brighter.spancontext.`

---

## Further Reading

- [Configuring OpenTelemetry](/contents/ConfiguringOpenTelemetry.md) - Wiring up the SDK, exporters and a worked producer/consumer example
- [OpenTelemetry Semantic Conventions for Messaging](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/)
- [W3C TraceContext Specification](https://w3c.github.io/trace-context/)
- [CloudEvents Distributed Tracing Extension](https://github.com/cloudevents/spec/blob/main/cloudevents/extensions/distributed-tracing.md)
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [Brighter CloudEvents Support](CloudEventsSupport.md)
- [Brighter Request Context](UsingTheContextBag.md)
