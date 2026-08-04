# AsyncAPI Document Generation

> **How-to** · Applies to **Brighter V10**

Brighter can automatically generate [AsyncAPI 3.0](https://www.asyncapi.com/docs/reference/specification/v3.0.0) documents from your service's runtime configuration. This gives you machine-readable documentation of your messaging contracts — the topics your service publishes to and subscribes from, along with JSON Schema for each message type — without maintaining separate documentation that drifts out of date.

## What is AsyncAPI?

[AsyncAPI](https://www.asyncapi.com/) is an open specification for defining asynchronous APIs. It serves the same role for message-driven architectures that OpenAPI (Swagger) serves for REST APIs: it describes what messages a service sends and receives, what topics or queues are involved, and what the message payloads look like.

Without AsyncAPI, teams maintain messaging documentation manually — in wikis, READMEs, or Confluence pages. This documentation inevitably becomes stale as the code evolves. AsyncAPI solves this by providing a standard, machine-readable format that can be generated directly from your code.

Brighter generates AsyncAPI 3.0 documents by inspecting your registered [subscriptions](/contents/Routing.md), publications, and message types. The generated document can be used with the broader AsyncAPI ecosystem — tools like AsyncAPI Studio for visualization, Microcks for mock generation, and code generators for consumer SDKs.

## Getting Started

### Prerequisites

- A Brighter service with an [external bus configured](/contents/ImplementingExternalBus.md)
- .NET 8.0 or later
- Two NuGet packages:

```
dotnet add package Paramore.Brighter.AsyncAPI
dotnet add package Paramore.Brighter.AsyncAPI.NJsonSchema
```

The core package (`Paramore.Brighter.AsyncAPI`) contains the document generator. The schema package (`Paramore.Brighter.AsyncAPI.NJsonSchema`) provides the default JSON Schema generator using NJsonSchema. If you implement your own `IAmASchemaGenerator`, you do not need the NJsonSchema package — but you must register your implementation before calling `UseAsyncApi()`, otherwise `UseAsyncApi()` throws an `InvalidOperationException`.

### Adding AsyncAPI Generation

Register AsyncAPI generation by calling `UseAsyncApi()` on your Brighter builder:

```csharp
using Paramore.Brighter.AsyncAPI;

services.AddConsumers(options =>
{
    // ... your subscription configuration
})
.AddProducers(configure =>
{
    // ... your producer configuration
})
.UseAsyncApi(opts =>
{
    opts.Title = "My Service";
    opts.Version = "1.0.0";
    opts.Description = "Order processing service messaging contracts";
});
```

Then generate the document after building your host:

```csharp
var host = builder.Build();

if (args.Length > 0 && args[0] == "--generate-asyncapi")
{
    var document = await host.GenerateAsyncApiDocumentAsync("asyncapi.json");
    Console.WriteLine($"AsyncAPI document generated: {document.Info.Title} v{document.Info.Version}");
    return;
}
```

This writes both `asyncapi.json` and `asyncapi.yaml` to the output path. The JSON file is the canonical output; the YAML file is derived from it for human readability.

## How Brighter Discovers Your Messaging Contracts

Brighter collects messaging contracts from three sources, processed in this priority order:

1. **Subscriptions** — topics your service consumes (registered via `AddConsumers()`)
2. **Publications** — topics your service produces (registered via `AddProducers()`)
3. **Assembly scanning** — [IRequest](/contents/BasicConcepts.md) types decorated with `[PublicationTopic]`

Deduplication is keyed on `(channel, action)`. Subscriptions produce `receive` operations; publications and assembly-scanned `[PublicationTopic]` types produce `send` operations. Sources are processed in the order above, so DI-registered publications suppress assembly-scanned sends on the same routing key. A subscription and a publication on the same routing key are *not* duplicates — they produce one channel with both a `receive` and a `send` operation.

### Subscriptions (Receive Operations)

Subscriptions registered with `AddConsumers()` become `receive` operations in the AsyncAPI document. Each subscription's [RoutingKey](/contents/Routing.md) becomes a channel address, and the `RequestType` provides the message schema.

```csharp
options.Subscriptions = new Subscription[]
{
    new RmqSubscription<PaymentReceivedEvent>(
        new SubscriptionName("paramore.payment"),
        new ChannelName("payment.received"),
        new RoutingKey("payment.received"),
        messagePumpType: MessagePumpType.Reactor,
        makeChannels: OnMissingChannel.Create)
};
```

This produces a channel at address `payment.received` with a `receive` operation and a `PaymentReceivedEvent` message schema.

Send-only applications that do not call `AddConsumers()` produce documents with no receive operations — this is not an error.

### Publications (Send Operations)

Publications registered in the producer registry become `send` operations. Each publication's `Topic` becomes the channel address. Use typed publications (e.g. `RmqPublication<T>`) to associate a `RequestType` with the publication for schema generation.

```csharp
var producerRegistry = new RmqProducerRegistryFactory(
    rmqConnection,
    new RmqPublication<OrderCreatedEvent>[]
    {
        new()
        {
            Topic = new RoutingKey("order.created"),
            MakeChannels = OnMissingChannel.Create
        }
    }).Create();
```

Receive-only applications that do not register producers produce documents with no send operations.

### Assembly Scanning (Send Operations)

You can decorate your [Event](/contents/BasicConcepts.md) or [Command](/contents/BasicConcepts.md) types with the `[PublicationTopic]` attribute to make them discoverable via assembly scanning:

```csharp
using Paramore.Brighter;

[PublicationTopic("order.created")]
public class OrderCreatedEvent : Event
{
    public OrderCreatedEvent() : base(Id.Random()) { }

    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

Assembly scanning discovers concrete, non-abstract types that implement `IRequest` and carry this attribute. The attribute's topic string becomes the channel address in the generated document.

Assembly scanning is useful when you want your message types to self-document their publication topics without requiring them to be wired into a producer registry. You can disable it with:

```csharp
.UseAsyncApi(opts =>
{
    opts.DisableAssemblyScanning = true;
})
```

### Deduplication

When the same routing key appears in multiple sources, Brighter produces one channel with multiple operations rather than duplicate channels:

- Same routing key from a subscription and a publication → one channel, one `receive` operation, one `send` operation (different actions, so not a duplicate)
- Same `IRequest` type in both a subscription and a publication → one message component in `components/messages`
- Same routing key as both a DI publication and an assembly-scanned `[PublicationTopic]` type → DI publication wins, assembly scan is skipped (both are `send`)
- A subscription on a routing key does not suppress an assembly-scanned `send` on the same key — they have different actions, so both are emitted

## Configuration

### AsyncApiOptions

Configure the generated document via the `UseAsyncApi()` delegate:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string` | `"Brighter Application"` | The `info.title` field in the generated document |
| `Version` | `string` | `"1.0.0"` | The `info.version` field |
| `Description` | `string?` | `null` | Optional `info.description` field |
| `Servers` | `Dictionary<string, V3ServerDefinition>?` | `null` | Server definitions (broker endpoints) |
| `AssembliesToScan` | `IEnumerable<Assembly>?` | Entry assembly | Assemblies to scan for `[PublicationTopic]` types |
| `DisableAssemblyScanning` | `bool` | `false` | When `true`, skips assembly scanning entirely |
| `SupplementalPublications` | `IEnumerable<Publication>?` | `null` | Additional publications to include beyond those in the producer registry |

Full configuration example:

```csharp
using Neuroglia.AsyncApi.v3;

.UseAsyncApi(opts =>
{
    opts.Title = "Order Service";
    opts.Version = "2.1.0";
    opts.Description = "Handles order lifecycle events";
    opts.Servers = new Dictionary<string, V3ServerDefinition>
    {
        ["production"] = new V3ServerDefinition
        {
            Host = "rabbitmq.internal:5672",
            Protocol = "amqp",
            Description = "Production RabbitMQ cluster"
        }
    };
    opts.AssembliesToScan = new[] { typeof(OrderCreatedEvent).Assembly };
})
```

### Output Formats

`GenerateAsyncApiDocumentAsync()` writes two files:

- **JSON** — written to the path you specify (e.g. `asyncapi.json`). This is the canonical format.
- **YAML** — written alongside with a `.yaml` extension (e.g. `asyncapi.yaml`). Derived from the JSON output for guaranteed consistency.

Both files describe the same document. Use JSON for machine consumption and YAML for human review.

## Custom Schema Generation

The default schema generator uses NJsonSchema to produce JSON Schema from your `IRequest` types. It honours `System.ComponentModel.DataAnnotations` attributes (e.g. `[Required]`, `[StringLength]`) and `System.Text.Json` attributes (e.g. `[JsonPropertyName]`).

To substitute your own schema generation logic, implement `IAmASchemaGenerator`:

```csharp
using Neuroglia.AsyncApi.v3;
using Paramore.Brighter.AsyncAPI;

public class MySchemaGenerator : IAmASchemaGenerator
{
    public Task<V3SchemaDefinition?> GenerateAsync(Type? requestType, CancellationToken ct = default)
    {
        // Return a V3SchemaDefinition with your schema, or null for an empty object schema
        // ...
    }
}
```

Register your implementation before calling `UseAsyncApi()`:

```csharp
services.AddSingleton<IAmASchemaGenerator, MySchemaGenerator>();

services.AddConsumers(options => { /* ... */ })
    .UseAsyncApi(opts => { /* ... */ });
```

When `UseAsyncApi()` finds an existing `IAmASchemaGenerator` registration, it does not override it with the default NJsonSchema implementation.

## CI/CD Integration

A common pattern is to generate the AsyncAPI document as part of your CI pipeline using a command-line argument:

```csharp
if (args.Length > 0 && args[0] == "--generate-asyncapi")
{
    var document = await host.GenerateAsyncApiDocumentAsync("asyncapi.json");
    Console.WriteLine($"Generated: {document.Info.Title} v{document.Info.Version} (JSON + YAML)");
    return;
}
```

The host must be built (so DI is resolved) but does not need to start running. No broker connection is required for RabbitMQ-based services. For Kafka, see the [Kafka example](#kafka-example) below for a pattern that avoids broker connections during generation.

If `UseAsyncApi()` was never called, `GenerateAsyncApiDocumentAsync()` throws an `InvalidOperationException` because neither `IAmAnAsyncApiDocumentGenerator` nor `IAsyncApiDocumentWriter` will be registered. The output path is also normalised: if you pass a path without a `.json` extension it is appended, and the YAML file is always written alongside with the `.yaml` extension.

You can validate the generated document using the [AsyncAPI CLI](https://www.asyncapi.com/tools/cli):

```bash
npm install -g @asyncapi/cli
asyncapi validate asyncapi.json
```

## Complete Examples

### RabbitMQ Example

This example configures a service with one subscription (receiving payment events) and one publication (sending order events), then generates the AsyncAPI document:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.AsyncAPI;
using Neuroglia.AsyncApi.v3;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange"),
};

var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

var producerRegistry = new RmqProducerRegistryFactory(
    rmqConnection,
    new RmqPublication<OrderCreatedEvent>[]
    {
        new()
        {
            WaitForConfirmsTimeOutInMilliseconds = 1000,
            MakeChannels = OnMissingChannel.Create,
            Topic = new RoutingKey("order.created")
        }
    }).Create();

var host = new HostBuilder()
    .ConfigureServices((_, services) =>
    {
        services.AddConsumers(options =>
            {
                options.Subscriptions = new Subscription[]
                {
                    new RmqSubscription<PaymentReceivedEvent>(
                        new SubscriptionName("paramore.payment"),
                        new ChannelName("payment.received"),
                        new RoutingKey("payment.received"),
                        timeOut: TimeSpan.FromMilliseconds(200),
                        messagePumpType: MessagePumpType.Reactor,
                        makeChannels: OnMissingChannel.Create)
                };
                options.DefaultChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
            })
            .AddProducers(configure =>
            {
                configure.ProducerRegistry = producerRegistry;
            })
            .UseAsyncApi(opts =>
            {
                opts.Title = "Order Service";
                opts.Version = "1.0.0";
                opts.Description = "Order processing service messaging contracts";
                opts.Servers = new Dictionary<string, V3ServerDefinition>
                {
                    ["rabbitmq"] = new V3ServerDefinition
                    {
                        Host = "localhost:5672",
                        Protocol = "amqp",
                        Description = "Local RabbitMQ broker"
                    }
                };
            })
            .AutoFromAssemblies();
    })
    .Build();

if (args.Length > 0 && args[0] == "--generate-asyncapi")
{
    var document = await host.GenerateAsyncApiDocumentAsync("asyncapi.json");
    Console.WriteLine($"Generated: {document.Info.Title} v{document.Info.Version} (JSON + YAML)");
    return;
}
```

Running `dotnet run -- --generate-asyncapi` produces the following document (abbreviated):

```json
{
  "asyncapi": "3.0.0",
  "info": {
    "title": "Order Service",
    "version": "1.0.0",
    "description": "Order processing service messaging contracts"
  },
  "servers": {
    "rabbitmq": {
      "host": "localhost:5672",
      "protocol": "amqp",
      "description": "Local RabbitMQ broker"
    }
  },
  "channels": {
    "payment_received": {
      "address": "payment.received",
      "messages": {
        "PaymentReceivedEvent": {
          "$ref": "#/components/messages/PaymentReceivedEvent"
        }
      }
    },
    "order_created": {
      "address": "order.created",
      "messages": {
        "OrderCreatedEvent": {
          "$ref": "#/components/messages/OrderCreatedEvent"
        }
      }
    }
  },
  "operations": {
    "receive_payment_received": {
      "action": "receive",
      "channel": { "$ref": "#/channels/payment_received" },
      "messages": [{ "$ref": "#/channels/payment_received/messages/PaymentReceivedEvent" }]
    },
    "send_order_created": {
      "action": "send",
      "channel": { "$ref": "#/channels/order_created" },
      "messages": [{ "$ref": "#/channels/order_created/messages/OrderCreatedEvent" }]
    }
  },
  "components": {
    "messages": {
      "PaymentReceivedEvent": {
        "contentType": "application/json",
        "payload": { "..." : "JSON Schema for PaymentReceivedEvent" },
        "name": "PaymentReceivedEvent"
      },
      "OrderCreatedEvent": {
        "contentType": "application/json",
        "payload": { "..." : "JSON Schema for OrderCreatedEvent" },
        "name": "OrderCreatedEvent"
      }
    }
  }
}
```

### Kafka Example

Kafka has a specific consideration: `KafkaProducerRegistryFactory.Create()` opens a real broker connection at construction time. When generating the AsyncAPI document, you typically don't have a live broker available (especially in CI). The solution is to conditionally skip producer registry creation and rely on `[PublicationTopic]` assembly scanning to discover publications:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.AsyncAPI;
using Neuroglia.AsyncApi.v3;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var generateAsyncApi = args.Length > 0 && args[0] == "--generate-asyncapi";

var kafkaConfig = new KafkaMessagingGatewayConfiguration
{
    Name = "paramore.brighter.orders",
    BootStrapServers = new[] { "localhost:9092" }
};

var kafkaMessageConsumerFactory = new KafkaMessageConsumerFactory(kafkaConfig);

var host = new HostBuilder()
    .ConfigureServices((_, services) =>
    {
        var brighter = services.AddConsumers(options =>
        {
            options.Subscriptions = new Subscription[]
            {
                new KafkaSubscription<PaymentReceivedEvent>(
                    new SubscriptionName("paramore.payment"),
                    new ChannelName("payment.received"),
                    new RoutingKey("payment.received"),
                    groupId: "order-service",
                    timeOut: TimeSpan.FromMilliseconds(200),
                    messagePumpType: MessagePumpType.Reactor,
                    makeChannels: OnMissingChannel.Create)
            };
            options.DefaultChannelFactory = new ChannelFactory(kafkaMessageConsumerFactory);
        });

        // Only create the producer registry when NOT generating docs,
        // because KafkaProducerRegistryFactory opens a broker connection.
        // Assembly scanning via [PublicationTopic] discovers publications instead.
        if (!generateAsyncApi)
        {
            var producerRegistry = new KafkaProducerRegistryFactory(
                kafkaConfig,
                new KafkaPublication<OrderCreatedEvent>[]
                {
                    new()
                    {
                        Topic = new RoutingKey("order.created"),
                        NumPartitions = 3,
                        MessageSendMaxRetries = 3,
                        MessageTimeoutMs = 1000,
                        MaxInFlightRequestsPerConnection = 1
                    }
                }).Create();

            brighter.AddProducers(configure =>
            {
                configure.ProducerRegistry = producerRegistry;
            });
        }

        brighter.UseAsyncApi(opts =>
            {
                opts.Title = "Order Service (Kafka)";
                opts.Version = "1.0.0";
                opts.Description = "Order processing with Kafka transport";
                opts.Servers = new Dictionary<string, V3ServerDefinition>
                {
                    ["kafka"] = new V3ServerDefinition
                    {
                        Host = "localhost:9092",
                        Protocol = "kafka",
                        Description = "Local Kafka broker"
                    }
                };
            })
            .AutoFromAssemblies();
    })
    .Build();

if (generateAsyncApi)
{
    var document = await host.GenerateAsyncApiDocumentAsync("asyncapi.json");
    Console.WriteLine($"Generated: {document.Info.Title} v{document.Info.Version} (JSON + YAML)");
    return;
}
```

The key difference is the `if (!generateAsyncApi)` guard around producer registry creation. Because `OrderCreatedEvent` carries `[PublicationTopic("order.created")]`, assembly scanning discovers it and the generated document still correctly describes the `send` operation — without needing a live Kafka broker.

## Further Reading

- [AsyncAPI Specification](https://www.asyncapi.com/docs/reference/specification/v3.0.0) — the full AsyncAPI 3.0 specification
- [AsyncAPI Studio](https://studio.asyncapi.com/) — browser-based tool for visualizing AsyncAPI documents
- [AsyncAPI CLI](https://www.asyncapi.com/tools/cli) — command-line tool for validation and code generation
- Working samples: `Brighter/samples/AsyncAPI/RMQAsyncAPI/` and `Brighter/samples/AsyncAPI/KafkaAsyncAPI/`
- [Using an External Bus](/contents/ImplementingExternalBus.md) — prerequisite for AsyncAPI generation
- [Routing](/contents/Routing.md) — understanding RoutingKey and topic configuration
- [Message Mappers](/contents/MessageMappers.md) — how messages are serialized for transport
- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md) — RabbitMQ transport setup
- [Kafka Configuration](/contents/KafkaConfiguration.md) — Kafka transport setup
