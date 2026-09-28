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

- **Source Name**: `Paramore.Brighter`. OpenTelemetry matches source names without regard to case, so `paramore.brighter` also works
- **Version**: The `Paramore.Brighter` assembly's version, which is `10.0.0.0` at package version 10.7.0

### Registering Brighter's Tracer

Brighter writes to that source only through a tracer, an `IAmABrighterTracer`, registered in your
container. `AddBrighter()` does not register one, so listening to the source is not enough:
`AddSource("paramore.brighter")` on its own records no Brighter span.
`AddBrighterInstrumentation()`, from the `Paramore.Brighter.Extensions.Diagnostics` package, does
both: it registers the tracer and adds the source.

Use it on the tracer provider that `AddOpenTelemetry()` builds, which shares your application's
container. A provider built with `Sdk.CreateTracerProviderBuilder()` keeps its own services, so the
tracer it registers is not one Brighter can find.

### Basic Configuration

The following code configures OpenTelemetry to:

- Enable tracing
- Set the service name
- Register Brighter's tracer and listen to its source
- Export traces over OTLP

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter.Extensions.Diagnostics;

const string serviceName = "MyService";

var services = new ServiceCollection();

services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddBrighterInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://localhost:4317");
        }));
```

The packages are `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`
and `Paramore.Brighter.Extensions.Diagnostics`.

### Configuration with Different Backends

#### Jaeger

Jaeger receives OTLP, and OpenTelemetry has deprecated its Jaeger exporter in favour of OTLP. Point
the [OTLP exporter](#otlp-opentelemetry-protocol) at Jaeger's OTLP endpoint, port 4317 for gRPC.

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
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.Diagnostics;
using Paramore.Brighter.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OrderService"))
    .WithTracing(tracing =>
    {
        tracing
            .AddBrighterInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4317");
            });
    });

// Configure Brighter, and how much its spans record
builder.Services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.InstrumentationOptions = InstrumentationOptions.RequestInformation;
})
.AddProducers(configure =>
{
    // ... producer registry
    configure.InstrumentationOptions = InstrumentationOptions.RequestInformation
                                     | InstrumentationOptions.Messaging;
})
.AutoFromAssemblies();

var app = builder.Build();
app.Run();
```

`AddAspNetCoreInstrumentation()` and `AddHttpClientInstrumentation()` come from
`OpenTelemetry.Instrumentation.AspNetCore` and `OpenTelemetry.Instrumentation.Http`.

### Consumer Service (Dispatcher)

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.Diagnostics;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    // Configure OpenTelemetry
    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("TaskProcessor"))
        .WithTracing(tracing =>
        {
            tracing
                .AddBrighterInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:4317");
                });
        });

    // Configure Brighter Consumer, and how much its spans record
    services.AddConsumers(options =>
    {
        // ... subscriptions and channel factory
        // Leave out RequestBody, which records the message body and is expensive
        options.InstrumentationOptions = InstrumentationOptions.RequestInformation
                                       | InstrumentationOptions.Messaging;
    })
    .AutoFromAssemblies();
});

var host = builder.Build();
await host.RunAsync();
```

---

## OpenTelemetry Distributed Tracing Example

The traces a producer and a consumer record for one event, measured with the in-memory transport
and Outbox. [Telemetry](/contents/Telemetry.md) describes each span.

The request, which deposits the event in the Outbox:

```text
ASP.NET Request (OrderService): "POST /api/orders"
  └─> "CreateOrderCommand send"
      └─> Handler events: CreateOrderCommandHandler
      └─> "OrderCreatedEvent deposit"
          └─> "add.message outbox requests"
```

Clearing the Outbox, which sends the message, in a trace of its own:

```text
"paramore.brighter.clear_messages create"
  └─> "retrieve.message outbox requests"
  └─> "paramore.brighter.clear_messages clear"
      └─> "orders.created publish"
```

The consumer, whose `process` span continues the trace of the `publish` span above:

```text
"orders.created process"
  └─> "OrderCreatedEvent create"
      └─> "OrderCreatedEvent publish", one per handler
          └─> "message.exists inbox requests"
          └─> "add.message inbox requests"
          └─> Handler events: SendEmailHandler
```

---

## Further Reading

- [Telemetry](/contents/Telemetry.md) - The spans Brighter emits, component by component
- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
- [OpenTelemetry Semantic Conventions for Messaging](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/)
