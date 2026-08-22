---
description: "Brighter is a Command Processor and supports a pipeline of Handlers to handle orthogonal requests."
layout:
  description:
    visible: false
---

# Supporting Retry and Circuit Breaker

> **How-to** · Applies to **Brighter V10**

Brighter is a [Command Processor](https://www.goparamore.io/control-bus-and-data-bus/) and supports a [pipeline of Handlers to handle orthogonal requests](/contents/BuildingAPipeline.md).

Amongst the valuable uses of orthogonal requests is patterns to support Quality of Service in a distributed environment: [Timeout, Retry, and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md#using-brighters-useresiliencepipeline-attribute).

Even if you don't believe that you are writing a distributed system that needs this protection, consider that as soon as you have multiple processes, such as a database server, you are distributed.

Brighter uses [Polly](https://github.com/App-vNext/Polly) to support Retry and Circuit-Breaker. Through our [Russian Doll Model](/contents/BuildingAPipeline.md) we are able to run the target handler in the context of a Policy Handler, that catches exceptions, and applies a Policy on how to deal with them.

## Polly v8 Resilience Pipelines

**Brighter supports [Polly v8](https://www.pollydocs.org/) Resilience Pipelines**, which provides a modern, streamlined API for building resilience strategies:

- **Full Polly v8 Support**: Access to all Polly v8 resilience strategies (Retry, Circuit Breaker, Timeout, Rate Limiter, Fallback, Hedging)
- **New `UseResiliencePipeline` Attribute**: Replaces `UsePolicy` attribute for Polly v8 pipelines
- **Enhanced Context Integration**: Request context integrates with Polly's resilience context
- **Proper CancellationToken Flow**: Cancellation tokens flow correctly through resilience pipelines
- **Type-Scoped Pipelines**: Support for per-handler-type pipelines (useful for Circuit Breakers)

### Migration from V9 to V10

| V9 (Polly v7) | V10 (Polly v8) | Notes |
|---------------|----------------|-------|
| `[UsePolicy]` | `[UseResiliencePipeline]` | New attribute for Polly v8 pipelines |
| `[TimeoutPolicy]` | `[UseResiliencePipeline]` with Timeout strategy | **TimeoutPolicy deprecated in V10, removed in V11** |
| `PolicyRegistry` | `ResiliencePipelineRegistry<string>` | From Polly.Registry namespace |
| Policies configured with `Policy.Handle<>()` | Pipelines configured with `ResiliencePipelineBuilder` | New fluent API |

> **⚠️ DEPRECATION NOTICE**: The `TimeoutPolicyAttribute` is marked as obsolete in V10 and will be removed in V11. Migrate to `UseResiliencePipeline` with a Timeout strategy instead.

---

## Using Brighter's UseResiliencePipeline Attribute

By adding the **UseResiliencePipeline** attribute, you instruct the Command Processor to insert a handler (filter) into the pipeline that runs all later steps using that Polly resilience pipeline.

### Basic Example

```csharp
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline(policy: "MyRetryPipeline", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        // Do work that could throw errors due to distributed computing reliability
        return base.Handle(command);
    }
}
```

### Configuring Resilience Pipelines

To configure a Polly resilience pipeline, you use the `ResiliencePipelineRegistry<string>` to register pipelines with a name. At runtime, Brighter looks up that pipeline by name.

#### Retry Strategy Example

```csharp
var resiliencePipelineRegistry = new ResiliencePipelineRegistry<string>();

resiliencePipelineRegistry.TryAddBuilder("MyRetryPipeline",
    (builder, context) => builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        OnRetry = args =>
        {
            // Log retry attempt
            Console.WriteLine($"Retry {args.AttemptNumber} after {args.RetryDelay}");
            return default;
        }
    }));
```

#### Circuit Breaker Strategy Example

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyCircuitBreakerPipeline",
    (builder, context) => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,              // Break if 50% of requests fail
        MinimumThroughput = 10,          // Need at least 10 requests before breaking
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(60),
        OnOpened = args =>
        {
            // Log circuit breaker opened
            Console.WriteLine($"Circuit breaker opened after {args.BreakDuration}");
            return default;
        },
        OnClosed = args =>
        {
            // Log circuit breaker closed
            Console.WriteLine("Circuit breaker closed");
            return default;
        }
    }));
```

#### Timeout Strategy Example

**V10**: Use Polly's Timeout strategy instead of the deprecated `TimeoutPolicyAttribute`:

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyTimeoutPipeline",
    (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(5)));
```

**Handler Usage**:

```csharp
public class MyTimedHandler : RequestHandler<MyCommand>
{
    // V9 (deprecated):
    // [TimeoutPolicy(milliseconds: 5000, step: 1)]

    // V10 (recommended):
    [UseResiliencePipeline(policy: "MyTimeoutPipeline", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        // Work that should timeout after 5 seconds
        return base.Handle(command);
    }
}
```

---

## Combining Multiple Strategies

You can combine multiple resilience strategies in a single pipeline. Strategies are applied in the order they're added (inner to outer wrapping).

### Retry + Circuit Breaker + Timeout

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyComprehensivePipeline",
    (builder, context) => builder
        .AddTimeout(TimeSpan.FromSeconds(10))              // Innermost: Timeout individual attempts
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential
        })                                                  // Middle: Retry on failures
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(60)
        }));                                                // Outermost: Circuit breaker
```

**Handler Usage**:

```csharp
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline("MyComprehensivePipeline", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        // Protected by timeout, retry, and circuit breaker
        return base.Handle(command);
    }
}
```

**How it works**:

1. Circuit breaker checks if circuit is open (fails fast if open)
2. Retry wraps the operation (retries on failures)
3. Timeout wraps each individual attempt (times out after 10 seconds per attempt)

> **Note**: If retries are exhausted, the exception will bubble out to the Circuit Breaker, which will count it as a failure.

---

## Using Multiple Pipelines on a Handler

You can apply multiple resilience pipeline attributes to a handler. Each attribute wraps subsequent steps in the pipeline.

```csharp
internal class MyMultiPipelineHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline("MyCircuitBreakerPipeline", step: 1)]
    [UseResiliencePipeline("MyRetryPipeline", step: 2)]
    public override MyCommand Handle(MyCommand command)
    {
        // Circuit breaker wraps retry, which wraps this handler
        return base.Handle(command);
    }
}
```

**Execution order**: Circuit Breaker → Retry → Handler

---

## Type-Scoped Pipelines

For strategies like Circuit Breaker, you often want a separate instance per handler type (so failures in one handler don't affect others). Use `UseTypePipeline = true` to scope pipelines by handler type.

```csharp
internal class OrderServiceHandler : RequestHandler<ProcessOrderCommand>
{
    [UseResiliencePipeline("SharedCircuitBreaker", step: 1, UseTypePipeline = true)]
    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        // Uses circuit breaker scoped to OrderServiceHandler
        return base.Handle(command);
    }
}

internal class PaymentServiceHandler : RequestHandler<ProcessPaymentCommand>
{
    [UseResiliencePipeline("SharedCircuitBreaker", step: 1, UseTypePipeline = true)]
    public override ProcessPaymentCommand Handle(ProcessPaymentCommand command)
    {
        // Uses circuit breaker scoped to PaymentServiceHandler (separate from OrderServiceHandler)
        return base.Handle(command);
    }
}
```

**How it works**: When `UseTypePipeline = true`, Brighter looks up the pipeline using a key composed of the handler type's full name + the policy name. This allows different instances of the same resilience strategy to be associated uniquely with each handler type.

**Configuration**:

```csharp
// You need to register pipelines with the type-scoped key format
var handlerTypeName = typeof(OrderServiceHandler).FullName;
resiliencePipelineRegistry.TryAddBuilder($"{handlerTypeName}.SharedCircuitBreaker",
    (builder, context) => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions { /* ... */ }));

var paymentHandlerTypeName = typeof(PaymentServiceHandler).FullName;
resiliencePipelineRegistry.TryAddBuilder($"{paymentHandlerTypeName}.SharedCircuitBreaker",
    (builder, context) => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions { /* ... */ }));
```

---

## All Available Polly v8 Strategies

Polly v8 provides the following resilience strategies, all of which can be used with Brighter:

| Strategy | Description | Use Case |
|----------|-------------|----------|
| **Retry** | Retries operations that fail transiently | Network glitches, temporary service unavailability |
| **Circuit Breaker** | Prevents cascading failures by breaking the circuit | Dependency is down, prevent overwhelming failed services |
| **Timeout** | Limits operation execution time | Prevent hanging on slow operations |
| **Rate Limiter** | Controls the rate of operations | Throttle requests to external APIs |
| **Fallback** | Provides alternative value/action on failure | Graceful degradation, cached responses |
| **Hedging** | Executes parallel operations and takes first result | Improve latency in distributed systems |

### Rate Limiter Example

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyRateLimiterPipeline",
    (builder, context) => builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        PermitLimit = 10,  // Maximum 10 concurrent requests
        QueueLimit = 20    // Queue up to 20 additional requests
    }));
```

### Hedging Example

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyHedgingPipeline",
    (builder, context) => builder.AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
    {
        MaxHedgedAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(500),
        ActionGenerator = args =>
        {
            // Generate hedged action (e.g., call different endpoint)
            return () => CallAlternativeEndpoint();
        }
    }));
```

---

## CancellationToken Integration

Polly v8 resilience pipelines properly integrate with `CancellationToken`, allowing you to cancel operations in progress.

```csharp
internal class MyCancellableHandler : RequestHandlerAsync<MyCommand>
{
    [UseResiliencePipeline("MyRetryPipeline", step: 1)]
    public override async Task<MyCommand> HandleAsync(
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        // CancellationToken flows through the resilience pipeline
        await SomeAsyncOperation(cancellationToken);
        return command;
    }
}
```

---

## Request Context Integration

The request context integrates with Polly's resilience context, allowing you to access request metadata within resilience callbacks.

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyContextAwarePipeline",
    (builder, context) => builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        OnRetry = args =>
        {
            // Access Brighter's request context through Polly's resilience context
            if (args.Context.Properties.TryGetValue("BrighterContext", out var contextObj))
            {
                var requestContext = contextObj as RequestContext;
                // Use request context for logging, tracing, etc.
            }
            return default;
        }
    }));
```

See [Request Context documentation](UsingTheContextBag.md) for more details on accessing the resilience context from handlers.

---

## Registering Pipelines with CommandProcessor

When creating your `CommandProcessor`, pass the `ResiliencePipelineRegistry<string>` to the builder:

```csharp
var resiliencePipelineRegistry = new ResiliencePipelineRegistry<string>();

// Configure pipelines (see examples above)
resiliencePipelineRegistry.TryAddBuilder("MyRetryPipeline", /* ... */);
resiliencePipelineRegistry.TryAddBuilder("MyCircuitBreakerPipeline", /* ... */);

var commandProcessor = CommandProcessorBuilder.With()
    .Handlers(new HandlerConfiguration(
        subscriberRegistry: registry,
        handlerFactory: handlerFactory))
    .Policies(policyRegistry)  // Legacy Polly v7 policies (optional)
    .ResiliencePipelines(resiliencePipelineRegistry)  // Polly v8 pipelines
    .RequestContextFactory(new InMemoryRequestContextFactory())
    .Build();
```

Or using dependency injection with ASP.NET Core:

```csharp
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.Handlers(registry =>
{
    registry.Register<MyCommand, MyQoSProtectedHandler>();
})
.ConfigureResiliencePipelines(registry =>
{
    registry.TryAddBuilder("MyRetryPipeline",
        (builder, context) => builder.AddRetry(new RetryStrategyOptions { /* ... */ }));

    registry.TryAddBuilder("MyCircuitBreakerPipeline",
        (builder, context) => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions { /* ... */ }));
});
```

---

## Retry and Circuit Breaker Best Practices

1. **Use Resilience Pipelines for New Code**: Prefer `UseResiliencePipeline` over legacy `UsePolicy` for new handlers.

2. **Migrate Away from TimeoutPolicy**: Replace `[TimeoutPolicy]` with `[UseResiliencePipeline]` using Polly's Timeout strategy. TimeoutPolicy will be removed in V11.

3. **Use Type-Scoped Pipelines for Circuit Breakers**: Set `UseTypePipeline = true` when using Circuit Breakers to avoid failures in one handler affecting others.

4. **Combine Strategies Thoughtfully**: When combining timeout, retry, and circuit breaker:
   - Put timeout innermost (times out individual attempts)
   - Put retry in the middle (retries failed attempts)
   - Put circuit breaker outermost (prevents retry when service is known to be down)

5. **Configure Appropriate Delays**: Use exponential backoff for retries to avoid overwhelming recovering services.

6. **Monitor Circuit Breaker State**: Use `OnOpened`, `OnClosed`, and `OnHalfOpened` callbacks to log and monitor circuit breaker state changes.

7. **Pass CancellationTokens**: Always pass and respect `CancellationToken` in async handlers to support cancellation through resilience pipelines.

8. **Use Request Context**: Leverage request context integration for correlation IDs, tracing, and logging within resilience callbacks.

---

## Retry and Circuit Breaker Troubleshooting

### Pipeline Not Found Exception

**Symptom**: `InvalidOperationException: Resilience pipeline with key 'MyPipeline' not found`

**Solution**: Ensure you've registered the pipeline with the exact name referenced in the attribute:

```csharp
resiliencePipelineRegistry.TryAddBuilder("MyPipeline", /* ... */);
```

### Type Pipeline Not Found Exception

**Symptom**: `InvalidOperationException: Resilience pipeline with key 'MyNamespace.MyHandler.MyPipeline' not found`

**Solution**: When using `UseTypePipeline = true`, register the pipeline with the full key:

```csharp
var handlerTypeName = typeof(MyHandler).FullName;
resiliencePipelineRegistry.TryAddBuilder($"{handlerTypeName}.MyPipeline", /* ... */);
```

### TimeoutPolicy Obsolete Warning

**Symptom**: Compiler warning: `'TimeoutPolicyAttribute' is obsolete: 'It is recommended to use UsePolicyAttribute or UseResiliencePipelineAttribute instead'`

**Solution**: Replace `[TimeoutPolicy]` with `[UseResiliencePipeline]` using Polly's Timeout strategy (see [Migrating to Polly v8](/contents/MigratingToPollyV8.md)).

---

## Further Reading

- [Migrating to Polly v8](/contents/MigratingToPollyV8.md) - The V9 to V10 migration steps, and the deprecated Polly v7 attribute
- [Polly v8 Documentation](https://www.pollydocs.org/)
- [Polly v7 to v8 Migration Guide](https://www.pollydocs.org/migration-v8.html)
- [Brighter Fallback Policy](PolicyFallback.md)
- [Brighter Request Context](UsingTheContextBag.md)
- [Building a Handler Pipeline](BuildingAPipeline.md)
