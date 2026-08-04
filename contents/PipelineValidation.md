# Pipeline Validation and Diagnostics

> **How-to** · Applies to **Brighter V10**

Brighter can validate your pipeline configuration at startup, catching common mistakes before any messages are sent or consumed. It can also log a diagnostic report showing exactly how your pipelines, publications, and subscriptions are wired. Both features are opt-in and configured via extension methods on **IBrighterBuilder**.

## Quick Start

Add `.ValidatePipelines()` and `.DescribePipelines()` to your Brighter configuration chain:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;

builder.Services.AddBrighter(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
    })
    .AutoFromAssemblies([typeof(MyHandler).Assembly])
    .ValidatePipelines()
    .DescribePipelines();
```

When the application starts:

- **ValidatePipelines** checks your configuration against a set of rules. If any errors are found, it throws a `PipelineValidationException` with all errors listed, preventing the application from starting.
- **DescribePipelines** logs a structured report of your configured pipelines to `ILogger` — a summary at **Information** level and full detail at **Debug** level.

These two methods are independent — you can enable one without the other.

## What Gets Checked

Validation checks scale automatically to your configuration. If you only call `AddBrighter()`, only handler pipeline checks run. If you also call `AddProducers()` or `AddConsumers()`, the corresponding checks are included automatically.

### Handler Pipeline Checks (AddBrighter)

These checks apply to all Brighter applications, including those that only use the command processor without messaging.

| Rule | Severity | What It Checks |
|------|----------|----------------|
| Handler type visibility | Error | Handler class must be `public`. Brighter only discovers public handler types — a non-public handler will silently not be found by the pipeline builder. |
| Sync/async attribute consistency | Error | Async handlers (`IHandleRequestsAsync<T>`) must use async attributes (e.g. `RejectMessageOnErrorAsyncAttribute`). Sync handlers must use sync attributes. A mismatch will throw a `ConfigurationException` at pipeline build time. |
| Backstop attribute ordering | Warning | Backstop error-handling attributes (`RejectMessageOnError`, `DeferMessageOnError`, `DontAckOnError`) should be at the outermost position (lowest step number). If a backstop has a higher step number than a resilience pipeline attribute, it will never execute on failure. |
| Replay requires causation tracking | Error and Warning | A pipeline using `OnceOnlyAction.Replay` needs an Inbox and an Outbox that both implement the causation-tracking role interfaces *and* whose live schemas support it. A store that does not implement the interface is an Error; an un-migrated schema, a missing Outbox, or a probe that could not reach the store is a Warning. Only pipelines configured for `Replay` are checked. See [Replay On Seen](/contents/ReplayOnSeen.md). |

**Example error messages:**

```
Handler type 'MyNamespace.OrderHandler' is not public — Brighter only supports
public handler types. Make the class public so the pipeline builder can find it

Async handler uses sync attribute 'RejectMessageOnErrorAttribute' at step 0 —
this will throw a ConfigurationException at pipeline build time

'RejectMessageOnError' at step 5 is after 'UseResiliencePipeline' at step 3 —
in Brighter, lower step values are outer wrappers, so the backstop will never
execute on failure

Handler 'ProcessPaymentHandler': OnceOnlyAction.Replay requires a
causation-tracking outbox, but the configured outbox 'MyCustomOutbox' does not
implement IAmACausationTrackingOutbox — Replay cannot reset the dispatched
state of the original messages

Handler 'ProcessPaymentHandler': OnceOnlyAction.Replay requires causation
tracking, but the inbox store schema does not support it — migrate the inbox
schema to add the CausationId column for Replay to work
```

### Producer Checks (AddProducers)

These checks apply when you configure outgoing messages with `AddProducers()`.

| Rule | Severity | What It Checks |
|------|----------|----------------|
| Publication RequestType set | Error | Every `Publication` must have a `RequestType` set. A null `RequestType` causes `Post()` / `Deposit()` to throw `ConfigurationException` at runtime. |
| Publication RequestType implements IRequest | Error | The `RequestType` must implement `IRequest`. A type that doesn't implement `IRequest` will fail at runtime. |

**Example error messages:**

```
Publication.RequestType is null — Post()/Deposit() will throw ConfigurationException

Publication.RequestType 'MyNamespace.OrderData' does not implement IRequest
```

### Consumer Checks (AddConsumers)

These checks apply when you configure incoming messages with `AddConsumers()`.

| Rule | Severity | What It Checks |
|------|----------|----------------|
| Pump/handler type match | Error | A `Reactor` (sync) subscription must have a sync handler (`IHandleRequests<T>`). A `Proactor` (async) subscription must have an async handler (`IHandleRequestsAsync<T>`). |
| Handler registered | Error | Every subscription must have at least one handler registered for its `RequestType`. Without a handler, messages will be received but cannot be dispatched. |
| RequestType subtype | Warning | The subscription's `RequestType` should implement `ICommand` or `IEvent`, not just `IRequest`. Implementing only `IRequest` is unusual and may indicate misconfiguration. |

**Example error messages:**

```
Subscription uses Reactor (sync) pump but handler 'OrderHandler' is async —
use Proactor for async handlers

No handler registered for 'OrderCreated' — messages will be received but
cannot be dispatched

RequestType 'MyMessage' implements neither ICommand nor IEvent — consider
implementing one of these marker interfaces
```

## Diagnostic Report

The `DescribePipelines()` method logs a structured report showing how your pipelines are wired. This helps you confirm that handlers, mappers, and transforms are resolved as you expect.

### Summary (Information Level)

At `Information` log level, a single summary line is logged:

```
Brighter: 3 handler pipelines, 2 publications, 5 subscriptions configured
```

The summary includes counts only for the configuration paths you use. If you don't call `AddProducers()`, publications won't appear in the summary.

### Full Detail (Debug Level)

At `Debug` log level, the report shows the full wiring for each configuration path. Here is an example with all three paths configured:

```
=== Handler Pipelines ===
  OrderCreatedHandler (async)
    Pipeline: [DeferMessageOnErrorAsync(0)] → [UseResiliencePipelineAsync(1)] → OrderCreatedHandler
  PaymentReceivedHandler (async)
    Pipeline: [RejectMessageOnErrorAsync(0)] → PaymentReceivedHandler

=== Publications (Outgoing) ===
  OrderCreated → order-created
    Mapper:     OrderCreatedMessageMapper (custom)
    Transforms: [CompressPayload(0)]
  PaymentReceived → payment-received
    Mapper:     JsonMessageMapper<PaymentReceived> (default)
    Transforms: (none)

=== Subscriptions (Incoming) ===
  OrderCreated (Proactor)
    Channel:  order-created-queue → order-created
  PaymentReceived (Proactor)
    Channel:  payment-received-queue → payment-received
```

Key things to look for in the report:

- **Pipeline chain**: Verify that backstop attributes come first (lowest step number) and the handler is last.
- **Mapper resolution**: Check whether a *custom* or *default* mapper is used. If you expected a custom mapper but see `(default)`, your mapper may not be registered correctly.
- **Transforms**: Verify that outgoing transforms (`WrapWith`) appear on publications and incoming transforms (`UnwrapWith`) are configured where expected.

## Configuration

### Enabling Validation and Diagnostics

Both methods are called on the **IBrighterBuilder** returned by `AddBrighter()`:

```csharp
builder.Services.AddBrighter(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
    })
    .AutoFromAssemblies()
    .ValidatePipelines()    // enable startup validation
    .DescribePipelines();   // enable diagnostic report
```

You can enable either one independently:

```csharp
// Validation only, no diagnostic report
.ValidatePipelines()

// Diagnostic report only, no validation
.DescribePipelines()
```

### Controlling Error Behavior

By default, validation errors throw a `PipelineValidationException` and prevent the application from starting. You can change this to log errors without stopping startup:

```csharp
.ValidatePipelines(throwOnError: false)
```

When `throwOnError` is `false`:

- Validation errors are logged at `LogLevel.Error`
- Validation warnings are logged at `LogLevel.Warning`
- The application continues starting

When `throwOnError` is `true` (the default):

- Validation errors throw `PipelineValidationException` (which extends `ConfigurationException`)
- The exception message lists all errors found
- Validation warnings are still logged at `LogLevel.Warning`

Warnings never prevent startup regardless of the `throwOnError` setting.

### Conditional Enablement

Both methods accept an `enabled` parameter that controls whether the feature is registered at all. This lets you gate validation on environment or configuration without removing the method call:

```csharp
// Only validate in Development
.ValidatePipelines(enabled: builder.Environment.IsDevelopment())
.DescribePipelines(enabled: builder.Environment.IsDevelopment())
```

```csharp
// Gate on a configuration setting
.ValidatePipelines(enabled: builder.Configuration.GetValue<bool>("Brighter:ValidatePipelines"))
```

When `enabled` is `false`, no services are registered and there is zero overhead at startup.

### How Validation Scales to Your Configuration

You don't need to configure which checks to run — validation automatically detects what you've configured:

| Configuration | What Gets Validated |
|---------------|-------------------|
| `AddBrighter()` only | Handler pipeline checks |
| `AddBrighter()` + `AddProducers()` | Handler pipeline + producer checks |
| `AddBrighter()` + `AddConsumers()` | Handler pipeline + consumer checks |
| `AddBrighter()` + `AddProducers()` + `AddConsumers()` | All checks |

When `AddConsumers()` is used, validation is deferred to the `ServiceActivatorHostedService` so that it runs before the dispatcher starts receiving messages.

## Common Mistakes and Fixes

### Async Handler with Sync Attributes

An async handler must use async versions of pipeline attributes.

**Before** (error):

```csharp
public class OrderHandler : RequestHandlerAsync<OrderCreated>
{
    [RejectMessageOnError(step: 0)]  // wrong: sync attribute on async handler
    public override async Task<OrderCreated> HandleAsync(OrderCreated command, 
        CancellationToken cancellationToken = default)
    {
        // ...
    }
}
```

**After** (fixed):

```csharp
public class OrderHandler : RequestHandlerAsync<OrderCreated>
{
    [RejectMessageOnErrorAsync(step: 0)]  // correct: async attribute
    public override async Task<OrderCreated> HandleAsync(OrderCreated command, 
        CancellationToken cancellationToken = default)
    {
        // ...
    }
}
```

### Backstop After Resilience Pipeline

The backstop attribute should have a lower step number than the resilience pipeline so it wraps the entire pipeline and catches any exceptions.

**Before** (warning):

```csharp
[UseResiliencePipeline(step: 0, "RetryPipeline")]  // runs first (inner)
[RejectMessageOnErrorAsync(step: 1)]                // runs second (outer) — too late!
public override async Task<OrderCreated> HandleAsync(OrderCreated command, ...)
```

**After** (fixed):

```csharp
[RejectMessageOnErrorAsync(step: 0)]                // runs first (outermost)
[UseResiliencePipeline(step: 1, "RetryPipeline")]    // runs second (inner)
public override async Task<OrderCreated> HandleAsync(OrderCreated command, ...)
```

In Brighter, lower step numbers are outer wrappers. The backstop needs to be outermost so it catches exceptions from the resilience pipeline and any handlers inside it.

### Reactor Subscription with Async Handler

A `Reactor` subscription uses synchronous message pumping and requires a sync handler. If your handler is async, use `Proactor` instead.

**Before** (error):

```csharp
// Handler is async
public class OrderHandler : RequestHandlerAsync<OrderCreated> { ... }

// But subscription uses Reactor (sync)
new RmqSubscription<OrderCreated>(
    // ...
    messagePumpType: MessagePumpType.Reactor  // wrong: sync pump for async handler
)
```

**After** (fixed):

```csharp
new RmqSubscription<OrderCreated>(
    // ...
    messagePumpType: MessagePumpType.Proactor  // correct: async pump for async handler
)
```

### Missing Handler for Subscription

Every subscription needs at least one handler registered for its `RequestType`. If you use `AutoFromAssemblies()`, ensure the handler's assembly is included.

**Before** (error):

```csharp
// Handler exists but its assembly isn't scanned
.AutoFromAssemblies()  // only scans loaded assemblies

// Subscription for OrderCreated — but OrderHandler is in a separate assembly
new RmqSubscription<OrderCreated>(...)
```

**After** (fixed):

```csharp
// Explicitly include the handler's assembly
.AutoFromAssemblies([typeof(OrderHandler).Assembly])
```

### Replay Without Causation Tracking

A handler configured with `OnceOnlyAction.Replay` needs an Inbox and an Outbox that both track Causation Ids, *and* live schemas that can store them. Validation checks each store in turn. Only a store that does not implement the role interface is an Error — an un-migrated schema is a Warning, so the host starts cleanly and replay silently does nothing.

The usual cause is provisioning one box and forgetting the other.

**Before** (warning):

```csharp
[UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandler), onceOnly: true,
    onceOnlyAction: OnceOnlyAction.Replay)]
public override ProcessPayment Handle(ProcessPayment command) { ... }

// ... but only the Outbox is provisioned, so the Inbox has no CausationId column
services.AddBrighter()
    .AddProducers(producers => { /* ... */ })
    .UseBoxProvisioning(opts =>
    {
        opts.AddMsSqlOutbox(outboxConfig);
    });
```

**After** (fixed):

```csharp
services.AddBrighter()
    .AddProducers(producers => { /* ... */ })
    .UseBoxProvisioning(opts =>
    {
        opts.AddMsSqlOutbox(outboxConfig);
        opts.AddMsSqlInbox(inboxConfig);   // both boxes need the causation column
    });
```

Replay reads the Causation Id from the Inbox and hands it to the Outbox, so either box being behind is enough to stop it. [Replay On Seen](/contents/ReplayOnSeen.md#when-replay-does-not-fire) lists every finding this rule produces, along with the runtime failures that produce no startup finding at all.

## Further Reading

- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) — how handler pipelines work
- [Building an Async Pipeline of Request Handlers](/contents/BuildingAnAsyncPipeline.md) — async pipeline patterns
- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — configuring AddBrighter, AddProducers
- [How Configuring a Dispatcher Works](/contents/HowConfiguringTheDispatcherWorks.md) — configuring AddConsumers
- [Routing](/contents/Routing.md) — publications and subscriptions
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) — resilience pipeline attributes
- [Error Handling](/contents/HandlerFailure.md) — backstop attributes and error handling options
