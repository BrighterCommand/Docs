---
description: "A key constraint of the Pipes and Filters architectural style is that Filters do not share state."
layout:
  description:
    visible: false
---

# Passing Information Between Handlers in the Pipeline

> **How-to** · Applies to **Brighter V10**

A key constraint of the Pipes and Filters architectural style is that Filters do not share state. One reason is that this limits your ability to recompose the pipeline as steps must follow other steps.

However, when dealing with Handlers that implement orthogonal concerns it can be useful to pass context along the chain. Given that many orthogonal concerns have constraints about ordering anyway, we can live with the ordering constraints imposed by passing context.

The first thing is to avoid adding extra properties to the Command to support handling state for these orthogonal Filter steps in your pipeline. This couples your **Command** to orthogonal concerns and you really only want to bind it to your **Target Handler**.

Instead we provide a **Request Context** and **Context Bag** as part of the Command Dispatcher which is injected into each Handler in the Pipeline. The lifetime of this **Request Context** is the lifetime of the Request (although you will need to take responsibility for freeing any unmanaged resources you place into the **Context Bag** for example when code called after the Handler that inserts the resource into the Bag returns to the Handler).

## Using the Context Bag

The **Context Bag** is a `ConcurrentDictionary<string, object>` that allows you to pass arbitrary data between handlers in the pipeline.

```csharp
public class MyContextAwareCommandHandler : RequestHandler<MyCommand>
{
    public static string TestString { get; set; }

    public override MyCommand Handle(MyCommand command)
    {
        LogContext();
        return base.Handle(command);
    }

    private void LogContext()
    {
        TestString = (string)Context.Bag["TestString"];
        Context.Bag["MyContextAwareCommandHandler"] = "I was called and set the context";
    }
}
```

Internally we use the **Context Bag** in a number of the Quality of Service supporting Attributes we provide. See [Fallback](/contents/PolicyFallback.md) for example.

## Request Context Capabilities

### Setting Request Context Explicitly

You can set the **RequestContext** explicitly when calling `Send`, `Publish`, or `DepositPost` methods. This allows you to set properties of the **RequestContext** for transmission to the **RequestHandler** instead of having a new context created by the **RequestContextFactory** for that pipeline.

```csharp
public class OrderController : ControllerBase
{
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var context = new RequestContext();

        // Set custom properties on context
        context.Bag[RequestContextBagNames.PartitionKey] = request.TenantId;
        context.Bag[RequestContextBagNames.Headers] = new Dictionary<string, object>
        {
            ["x-correlation-id"] = HttpContext.TraceIdentifier,
            ["x-tenant-id"] = request.TenantId
        };

        // Pass context explicitly
        await _commandProcessor.SendAsync(
            new CreateOrderCommand { OrderId = request.OrderId },
            requestContext: context
        );

        return Accepted();
    }
}
```

### Partition Key

The **PartitionKey** allows you to control message routing to specific partitions in messaging systems like Kafka, Azure Service Bus, or AWS Kinesis. This is useful for ensuring related messages are processed in order.

**Setting Partition Key via Context Bag:**

```csharp
public class TenantAwareHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Set partition key for message routing
        Context.Bag[RequestContextBagNames.PartitionKey] = command.TenantId;

        return base.Handle(command);
    }
}
```

**Or using a PartitionKey object:**

```csharp
Context.Bag[RequestContextBagNames.PartitionKey] = new PartitionKey("customer-1234");
```

**Important Notes:**

- This value is optional and may be ignored by the transport if partitioning isn't supported
- Custom message mappers may choose to ignore this value entirely
- When not set, the transport's default partitioning strategy will be used
- Default Brighter mappers will use this value when present

### Custom Headers

You can add custom headers to messages dynamically via the Request Context. These headers are merged with any static headers configured on the Publication.

```csharp
public class OrderProcessingHandler : RequestHandler<ProcessOrderCommand>
{
    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        // Add custom headers dynamically
        Context.Bag[RequestContextBagNames.Headers] = new Dictionary<string, object>
        {
            ["x-custom-header"] = "runtime-value",
            ["x-timestamp"] = DateTime.UtcNow,
            ["x-order-priority"] = command.Priority,
            ["x-processing-region"] = Environment.GetEnvironmentVariable("REGION")
        };

        // Process order...
        _orderService.Process(command);

        return base.Handle(command);
    }
}
```

**Important Notes:**

- Headers set here take precedence over static header configurations
- Custom message mappers may ignore these headers entirely
- Merged with `Publication.DefaultHeaders` by default Brighter mappers

### CloudEvents Extensions

When using CloudEvents, you can add custom extension properties via the Request Context.

```csharp
public class EventPublishingHandler : RequestHandler<PublishEventCommand>
{
    public override PublishEventCommand Handle(PublishEventCommand command)
    {
        // Add CloudEvents extension properties
        Context.Bag[RequestContextBagNames.CloudEventsAdditionalProperties] = new Dictionary<string, object>
        {
            ["myextension"] = "value",
            ["numericExtension"] = 42,
            ["businessContext"] = command.BusinessContext
        };

        return base.Handle(command);
    }
}
```

These properties will be serialized as CloudEvent extensions in the generated message envelope.

### Originating Message

For consumers (subscribers to queues or streams), the Request Context now provides access to the **OriginatingMessage**. This allows you to examine properties of the message that was received, which is useful for debugging and accessing message metadata.

```csharp
public class MessageConsumerHandler : RequestHandlerAsync<MyCommand>
{
    public override async Task<MyCommand> HandleAsync(
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Access the original message that started this pipeline
        if (Context.OriginatingMessage != null)
        {
            _logger.LogInformation(
                "Processing message {MessageId} from topic {Topic} with correlation {CorrelationId}",
                Context.OriginatingMessage.Id,
                Context.OriginatingMessage.Header.Topic,
                Context.OriginatingMessage.Header.CorrelationId
            );

            // Access message headers
            var customHeader = Context.OriginatingMessage.Header.Bag["x-custom-header"];

            // Access CloudEvents properties
            var cloudEventType = Context.OriginatingMessage.Header.Type;
            var source = Context.OriginatingMessage.Header.Source;
        }

        // Process command...
        await _service.ProcessAsync(command, cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

**Use Cases:**

- **Debugging**: Understand how we got to this request
- **Auditing**: Log message metadata for compliance
- **Routing**: Access original message routing keys
- **Headers**: Read custom headers that weren't mapped to the command

**Important**: This is not thread-safe; the assumption is that you set this from a single thread and access the message from multiple threads. It is not intended to be set from multiple threads.

### OpenTelemetry Span

The Request Context provides access to the current **Span (Activity)** for adding custom OpenTelemetry attributes, events, and tags.

```csharp
public class TracingHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Add custom attributes to current span
        if (Context.Span != null)
        {
            Context.Span.SetAttribute("custom.business.id", command.BusinessId);
            Context.Span.SetAttribute("custom.tenant.id", command.TenantId);
            Context.Span.SetAttribute("custom.operation.type", "order-processing");

            // Add events
            Context.Span.AddEvent("Order validation started");

            // Add tags
            Context.Span.SetTag("order.value", command.OrderValue);
        }

        // Process command...
        _service.Process(command);

        if (Context.Span != null)
        {
            Context.Span.AddEvent("Order validation completed");
        }

        return base.Handle(command);
    }
}
```

See [Telemetry](Telemetry.md) for more information on OpenTelemetry integration.

### Destination Override

You can override the destination topic and CloudEvents type for a message using the **Destination** property.

```csharp
public class DynamicRoutingHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Override destination based on command properties
        if (command.Priority == Priority.High)
        {
            Context.Destination = new ProducerKey(
                new RoutingKey("high-priority-topic"),
                new CloudEventsType("com.mycompany.priority.high")
            );
        }
        else
        {
            Context.Destination = new ProducerKey(
                new RoutingKey("standard-topic"),
                new CloudEventsType("com.mycompany.priority.standard")
            );
        }

        return base.Handle(command);
    }
}
```

This allows runtime routing decisions based on command content or business rules.

### Resilience Context

The Request Context integrates with Polly's **ResiliencePipeline** (V8+) for resilience operations. The **ResilienceContext** can be used to share data across different resilience policies during execution.

```csharp
public class ResilientHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Access Polly resilience context
        if (Context.ResilienceContext != null)
        {
            Context.ResilienceContext.Properties.Set(
                new ResiliencePropertyKey<string>("tenant-id"),
                command.TenantId
            );

            Context.ResilienceContext.Properties.Set(
                new ResiliencePropertyKey<int>("retry-count"),
                0
            );
        }

        return base.Handle(command);
    }
}
```

See [Polly Resilience Pipeline](PolicyRetryAndCircuitBreaker.md) for more information.

### Resilience Pipeline Registry

Access pre-configured resilience pipelines by name:

```csharp
public class PipelineAwareHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        // Access resilience pipelines from registry
        if (Context.ResiliencePipeline != null)
        {
            var pipeline = Context.ResiliencePipeline.GetPipeline("my-custom-pipeline");

            // Use pipeline for operations
            pipeline.Execute(() =>
            {
                // Your resilient operation
            });
        }

        return base.Handle(command);
    }
}
```

## Well-Known Context Bag Keys

Brighter provides well-known keys for the Context Bag via the `RequestContextBagNames` class:

```csharp
public static class RequestContextBagNames
{
    // Custom headers for messages
    public const string Headers = "Brighter-Headers";

    // Partition key for message routing
    public const string PartitionKey = "Brighter-PartitionKey";

    // CloudEvents extension properties
    public const string CloudEventsAdditionalProperties = "Brighter-CloudEvents-AdditionalProperties";

    // Workflow identifier (reserved for future use)
    public const string WorkflowId = "Brighter-WorkflowId";

    // Job instance identifier (reserved for future use)
    public const string JobId = "Brighter-JobId";
}
```

Using these constants ensures consistency and avoids magic strings:

```csharp
// Good - using constants
Context.Bag[RequestContextBagNames.PartitionKey] = tenantId;

// Bad - magic strings
Context.Bag["Brighter-PartitionKey"] = tenantId;
```

## Context Bag Best Practices

### 1. Use Well-Known Keys

```csharp
// Good - Type-safe and discoverable
Context.Bag[RequestContextBagNames.Headers] = headers;

// Bad - Magic strings prone to typos
Context.Bag["my-headers"] = headers;
```

### 2. Clean Up Resources

If you place unmanaged resources in the Context Bag, ensure they are properly disposed:

```csharp
public class ResourceAwareHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command)
    {
        var resource = new DisposableResource();
        Context.Bag["MyResource"] = resource;

        try
        {
            // Use resource...
            return base.Handle(command);
        }
        finally
        {
            // Clean up
            if (Context.Bag.TryRemove("MyResource", out var obj) && obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
```

### 3. Document Custom Context Keys

If you use custom context bag keys, document them clearly:

```csharp
// Document your custom keys
public static class MyAppContextKeys
{
    // User identity for authorization checks
    public const string UserId = "MyApp-UserId";

    // Tenant identifier for multi-tenancy
    public const string TenantId = "MyApp-TenantId";
}

// Usage
Context.Bag[MyAppContextKeys.UserId] = userId;
```

### 4. Check for Null Before Accessing Properties

```csharp
// Good - defensive check
if (Context.Span != null)
{
    Context.Span.SetAttribute("custom.id", id);
}

// Bad - may throw NullReferenceException
Context.Span.SetAttribute("custom.id", id);
```

### 5. Use Explicit RequestContext for Important Metadata

```csharp
// Good - explicit context with important routing information
var context = new RequestContext();
context.Bag[RequestContextBagNames.PartitionKey] = tenantId;
context.Bag[RequestContextBagNames.Headers] = criticalHeaders;

await _commandProcessor.SendAsync(command, requestContext: context);
```

## Related Documentation

- [Dispatching Requests](DispatchingARequest.md) - How to dispatch commands and events
- [Fallback](PolicyFallback.md) - Using context for fallback scenarios
- [Telemetry](Telemetry.md) - OpenTelemetry integration with Span
- [Polly Resilience Pipeline](PolicyRetryAndCircuitBreaker.md) - Resilience context integration
- [Cloud Events Support](CloudEventsSupport.md) - CloudEvents extension properties

## Context Bag Summary

The Request Context provides a powerful mechanism for passing information between handlers in a pipeline:

- **Context Bag**: Pass arbitrary data between handlers
- **Partition Key**: Control message routing to specific partitions
- **Custom Headers**: Add dynamic headers to messages
- **CloudEvents Extensions**: Add custom CloudEvents properties
- **Originating Message**: Access original message metadata in consumers
- **OpenTelemetry Span**: Add custom traces and attributes
- **Destination Override**: Dynamic message routing
- **Resilience Context**: Integration with Polly resilience pipelines

Use these capabilities judiciously to avoid coupling your commands to orthogonal concerns while enabling necessary cross-cutting functionality.
