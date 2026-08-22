---
description: "The OpenTelemetry SDK can be configured to listen to Activities emitted by Brighter."
layout:
  description:
    visible: false
---

# Configuring OpenTelemetry

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Telemetry](/contents/Telemetry.md)

## Setting Up OpenTelemetry

The OpenTelemetry SDK can be configured to listen to Activities emitted by Brighter. For more information, see [OpenTelemetry Tracing in .NET](https://opentelemetry.io/docs/instrumentation/net/getting-started/).

### Activity Source

Brighter emits traces using the following Activity Source:

- **Source Name**: `paramore.brighter`
- **Version**: Includes the Brighter version number

### Basic Configuration

The following code configures OpenTelemetry to:

- Enable tracing
- Set the service name
- Listen to Brighter and Microsoft sources
- Export traces to Jaeger

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "MyService";
var jaegerEndpoint = new Uri("http://localhost:14268/api/traces");

using var tracerProvider =
    Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddSource("paramore.brighter", "Microsoft.*")
        .AddJaegerExporter(o =>
        {
            o.Endpoint = jaegerEndpoint;
        })
        .Build();
```

### Configuration with Different Backends

#### Jaeger

```csharp
// ...
.AddJaegerExporter(o =>
{
    o.AgentHost = "localhost";
    o.AgentPort = 6831;
})
```

#### Zipkin

```csharp
// ...
.AddZipkinExporter(o =>
{
    o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
})
```

#### OTLP (OpenTelemetry Protocol)

```csharp
// ...
.AddOtlpExporter(o =>
{
    o.Endpoint = new Uri("http://localhost:4317");
    o.Protocol = OtlpExportProtocol.Grpc;
})
```

#### Azure Monitor / Application Insights

```csharp
// ...
.AddAzureMonitorTraceExporter(o =>
{
    o.ConnectionString = "InstrumentationKey=...";
})
```

---

## Complete OpenTelemetry Configuration Example

### Producer Service

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter;
using Paramore.Brighter.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("OrderService"))
            .AddSource("paramore.brighter")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4317");
            });
    });

// Configure Brighter instrumentation
var instrumentation = BrighterInstrumentation.InstrumentationOptions;
instrumentation.CommandProcessorInstrumentationOptions.RecordRequestInformation = true;
instrumentation.MessagingInstrumentationOptions.RecordMessageInformation = true;
instrumentation.MessagingInstrumentationOptions.RecordServerInformation = true;

// Configure Brighter
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(configure =>
{
    // Producer configuration
})
.AutoFromAssemblies();

var app = builder.Build();
app.Run();
```

### Consumer Service (Dispatcher)

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter;
using Paramore.Brighter.Observability;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    // Configure OpenTelemetry
    services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("TaskProcessor"))
                .AddSource("paramore.brighter")
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:4317");
                });
        });

    // Configure Brighter instrumentation
    var instrumentation = BrighterInstrumentation.InstrumentationOptions;
    instrumentation.MessagingInstrumentationOptions.RecordMessageInformation = true;
    instrumentation.MessagingInstrumentationOptions.RecordMessageBody = false; // Expensive

    // Configure Brighter Consumer
    services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
    })
    .AutoFromAssemblies();
});

var host = builder.Build();
await host.RunAsync();
```

---

## OpenTelemetry Distributed Tracing Example

A complete distributed trace across services:

```text
ASP.NET Request (OrderService): "POST /api/orders"
  └─> Command Processor: "CreateOrderCommand send"
      └─> Handler: CreateOrderCommandHandler
      └─> Deposit: "CreateOrderCommand deposit"
          └─> Outbox add (MySQL)

  ─── Outbox Sweeper ───

  └─> Clear: "clear"
      └─> Outbox get (MySQL)
      └─> Produce: "orders.created publish"
      └─> Outbox mark dispatched (MySQL)

  ─── Message Broker (RabbitMQ) ───

Dispatcher (TaskService): "orders.created process"
  └─> Inbox check (PostgreSQL)
  └─> Command Processor: "OrderCreatedEvent send"
      └─> Handler: SendEmailHandler
      └─> Handler: UpdateInventoryHandler
  └─> Inbox add (PostgreSQL)
```

---

## Further Reading

- [Telemetry](/contents/Telemetry.md) - The spans Brighter emits, component by component
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [OpenTelemetry Semantic Conventions for Messaging](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/)
