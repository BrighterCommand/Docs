---
description: "Brighter can validate a request in its handler pipeline before your business handler runs."
layout:
  description:
    visible: false
---

# Request Validation

> **How-to** · Applies to **Brighter V10**

Brighter can validate a request in its handler pipeline **before** your business
handler runs. Validation is opt-in per handler: you mark the handler's
`Handle`/`HandleAsync` method with `[ValidateRequest]`, and you choose *how* the
request is validated by registering one of three provider packages —
FluentValidation, DataAnnotations, or Brighter's own Specification pattern. When a
request is invalid, the pipeline throws a `RequestValidationException` carrying the
individual failures, and your business handler never runs.

## Request Validation vs. Pipeline Validation

These two features sound alike but solve different problems. Read this first so you
land on the right page:

- **Request validation** (this page) checks the **data in a request at runtime** —
  is the name present, is the email well-formed — and rejects bad input before your
  handler runs.
- **[Pipeline validation](/contents/PipelineValidation.md)** checks that your
  **handler pipelines are wired correctly at startup** — that every request has a
  handler, that attributes resolve — and reports configuration mistakes before any
  message flows.

They even use different error types. Request validation produces a
`RequestValidationError`; pipeline validation produces a `ValidationError`. If you
are configuring `.ValidatePipelines()`, you want the other page.

## Why Validate in the Pipeline

Validating in the pipeline, rather than inside your handler, has several benefits:

- **Reject bad input early.** Invalid data is caught before it reaches your
  business logic or your database — no half-applied work, no exceptions thrown deep
  in domain code.
- **Keep validation as an orthogonal concern.** Validation is a cross-cutting
  concern, so it belongs in [middleware](/contents/BuildingAPipeline.md), not
  copied into every handler.
- **Simplify your handler and domain code.** Lifting validation out of the handler
  keeps it focused on business logic instead of guarding its inputs. By the time
  your `Handle` method runs, the request is already known to be valid.
- **Catch failures in one place.** Every provider throws the same
  `RequestValidationException`, so code at the edge — for example an API mapping a
  failure to HTTP 422 — catches one exception type regardless of which validation
  framework you chose.

Request validation builds on Brighter's standard attribute-and-handler middleware
model. If you are new to that model, read
[Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) first;
this page assumes you understand how an attribute inserts a handler into the
pipeline.

## How Request Validation Works

You mark the target handler's `Handle` method with `[ValidateRequest]`:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.RequestValidation.Attributes;

public sealed class GreetingCommandHandler : RequestHandler<GreetingCommand>
{
    [ValidateRequest(step: 1, timing: HandlerTiming.Before)]
    public override GreetingCommand Handle(GreetingCommand command)
    {
        Console.WriteLine($"Hello {command.Name}");
        return base.Handle(command);
    }
}
```

As with any pipeline attribute, `step` orders this handler relative to other
attributes on the method, and `timing` runs it **before** the target handler.
`timing` defaults to `HandlerTiming.Before`, so `[ValidateRequest(step: 1)]` is
equivalent. See [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md)
for how `step` and `timing` work in general.

What makes validation **provider-agnostic** is how the attribute is wired:

1. `[ValidateRequest]` targets an **abstract** base handler,
   `ValidateRequestHandler<TRequest>`, that owns the shared behaviour — guard the
   request, ask a provider for the failures, throw if there are any, otherwise call
   the next handler.
2. Registering a provider maps that abstract handler to a **concrete**
   implementation. `UseFluentValidation()`, `UseDataAnnotations()`, and
   `UseSpecification()` each register the mapping for you.

Because the attribute points at the abstract handler and the provider supplies the
concrete one, **the same `[ValidateRequest]` attribute works for every framework**.
You switch providers by changing the `UseX()` call, not by touching your handlers.

When the provider finds failures, the handler throws a `RequestValidationException`
and your business handler never runs.

> **Using an External Bus?** When validation runs in a handler dispatched from a
> message broker, a thrown `RequestValidationException` is an unhandled exception
> leaving the pipeline. By default Brighter's message pump logs it and acknowledges
> (discards) the message — it does **not** automatically requeue or dead-letter.
> Read [Error Handling](/contents/HandlerFailure.md) to choose what should happen to
> an invalid message (requeue, reject to a Dead Letter Queue, and so on).

## Request Validation Quick Start

This example validates a `RegisterUser` command with **DataAnnotations** — the
provider with the least ceremony, because the rules live on the request itself.

**1. Declare the rules on the request** with DataAnnotations attributes:

```csharp
using System.ComponentModel.DataAnnotations;
using Paramore.Brighter;

public sealed class RegisterUser() : Command(Id.Random())
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name must not be empty")]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email must not be empty")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    public string Email { get; set; } = string.Empty;
}
```

**2. Mark the handler** with `[ValidateRequest]`:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.RequestValidation.Attributes;

public sealed class RegisterUserHandler : RequestHandler<RegisterUser>
{
    [ValidateRequest(step: 1, timing: HandlerTiming.Before)]
    public override RegisterUser Handle(RegisterUser command)
    {
        Console.WriteLine($"Registered {command.Name} <{command.Email}>");
        return base.Handle(command);
    }
}
```

**3. Register Brighter and the provider.** `UseDataAnnotations()` maps the
`[ValidateRequest]` attribute to the DataAnnotations implementation:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Validation.DataAnnotations;

builder.Services
    .AddBrighter()
    .AutoFromAssemblies()
    .UseDataAnnotations();
```

**4. Send a request and handle failures.** A valid request reaches the handler; an
invalid one is rejected before the handler runs:

```csharp
using Paramore.Brighter.RequestValidation;

// Valid — reaches the handler
commandProcessor.Send(new RegisterUser { Name = "Ada", Email = "ada@example.com" });

// Invalid — rejected before the handler runs
try
{
    commandProcessor.Send(new RegisterUser { Name = "", Email = "not-an-email" });
}
catch (RequestValidationException exception)
{
    foreach (var error in exception.Errors)
        Console.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
}
```

The invalid request prints both failures and never reaches `RegisterUserHandler`:

```text
  - Name: Name must not be empty
  - Email: Email must be a valid email address
```

A complete, runnable version of this is in
`Brighter/samples/Validation/DataAnnotationsSample/`.

## Choosing a Provider

All three providers share the same `[ValidateRequest]` attribute and throw the same
`RequestValidationException`; they differ in **where the rules live** and **what
dependency you take**.

| Provider | Where rules live | Extra dependency | Reach for it when |
|----------|------------------|------------------|-------------------|
| **FluentValidation** | A separate `IValidator<TRequest>` class | FluentValidation | You want fluent, reusable rule sets kept apart from the request type. |
| **DataAnnotations** | Attributes on the request type | None (built into .NET) | The rules are simple and you want them to travel with the request. |
| **Specification** | An `ISpecification<TRequest>` you compose | None (ships in core) | You want composable domain rules expressed as specifications, with no extra dependency. |

You are not locked in: because only the `UseX()` registration differs, you can
start with one provider and move to another without changing your handlers. Pick one
provider per service — the registration maps the validation handler globally, so a
single service uses a single provider.

## FluentValidation

Reach for [FluentValidation](https://fluentvalidation.net/) when you want fluent,
reusable rule sets that live **apart** from the request type. Each validated request
has an `IValidator<TRequest>` registered in the container, and the provider resolves
it to validate the request.

Install the `Paramore.Brighter.Validation.FluentValidation` package, then write a
validator:

```csharp
using FluentValidation;

public sealed class GreetingCommandValidator : AbstractValidator<GreetingCommand>
{
    public GreetingCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().WithMessage("Name must not be empty");
        RuleFor(command => command.Email).NotEmpty().WithMessage("Email must not be empty");
    }
}
```

Register the validator and call `UseFluentValidation()`:

```csharp
using FluentValidation;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Validation.FluentValidation;

builder.Services.AddSingleton<IValidator<GreetingCommand>>(new GreetingCommandValidator());

builder.Services
    .AddBrighter()
    // ... other configuration
    .AutoFromAssemblies()
    .UseFluentValidation();
```

`UseFluentValidation()` only maps the `[ValidateRequest]` attribute to the
FluentValidation handler — it does **not** discover your validators. You register
each `IValidator<TRequest>` yourself, either explicitly as above or in bulk with
`services.AddValidatorsFromAssembly(...)`. If a request is marked with
`[ValidateRequest]` but no validator is registered for it, the provider throws a
`ConfigurationException` (see [Missing validator is a configuration
error](#missing-validator-is-a-configuration-error)).

## DataAnnotations

Reach for [DataAnnotations](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations)
when the rules are simple and you want them to **travel with the request**. The
constraints are declared as attributes — `[Required]`, `[EmailAddress]`,
`[Range]`, and so on — directly on the request type, so nothing else needs
registering.

The [Quick Start](#request-validation-quick-start) above is a complete DataAnnotations example: the
`RegisterUser` request carries its own `[Required]` and `[EmailAddress]`
constraints, and registration is a single call:

```csharp
using Paramore.Brighter.Validation.DataAnnotations;

builder.Services
    .AddBrighter()
    // ... other configuration
    .AutoFromAssemblies()
    .UseDataAnnotations();
```

Because the rules live on the request itself, there is no separate validator to
register and therefore nothing to leave unregistered — unlike the FluentValidation
and Specification providers, DataAnnotations has no missing-validator failure mode.

## Specification

Reach for Brighter's own Specification pattern when you want to express rules as
**composable domain specifications** without taking an extra dependency — the
`Specification<T>` type ships in the core `Paramore.Brighter` package. Each
validated request has an `ISpecification<TRequest>` registered, built by composing
rules with `And`/`Or`. Each rule pairs a predicate with the `ValidationError` it
reports when the predicate is not satisfied.

Install the `Paramore.Brighter.Validation.Specification` package, then build the
specification:

```csharp
using Paramore.Brighter;

public static class OrderSpecification
{
    public static ISpecification<PlaceOrder> Create()
        => new Specification<PlaceOrder>(
                order => order.Quantity > 0,
                order => new ValidationError(
                    ValidationSeverity.Error,
                    nameof(PlaceOrder.Quantity),
                    "Quantity must be greater than zero"))
            .And(new Specification<PlaceOrder>(
                order => !string.IsNullOrWhiteSpace(order.Sku),
                order => new ValidationError(
                    ValidationSeverity.Error,
                    nameof(PlaceOrder.Sku),
                    "Sku must not be empty")));
}
```

`And` evaluates both rules, so an order that breaks both is reported with both
errors. Register the specification and call `UseSpecification()`:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Validation.Specification;

builder.Services.AddTransient<ISpecification<PlaceOrder>>(_ => OrderSpecification.Create());

builder.Services
    .AddBrighter()
    // ... other configuration
    .AutoFromAssemblies()
    .UseSpecification();
```

As with FluentValidation, `UseSpecification()` only maps the attribute to the
Specification handler — you register each `ISpecification<TRequest>` yourself, and a
missing one throws a `ConfigurationException`.

> **Two error types, one mapping.** The rules in your specification report Brighter's
> core `ValidationError` (the same type used by [pipeline
> validation](/contents/PipelineValidation.md)). When validation fails, the provider
> maps those onto `RequestValidationError` instances on the
> `RequestValidationException` — so you *author* `ValidationError`s but *catch*
> `RequestValidationError`s.

### Lifetime: register per request

Brighter's `Specification<T>` records per-evaluation state — it stores the results
of the most recent `IsSatisfiedBy` call so it can report which rules failed.
Register it with a **per-request lifetime** (transient or scoped), **not** as a
singleton, so concurrent requests stay isolated. The registration above uses
`AddTransient`; a singleton would let one request's evaluation overwrite another's.

## Handling Validation Failures

When a request is invalid, the pipeline throws a `RequestValidationException`. Its
`Errors` property is an `IReadOnlyCollection<RequestValidationError>` — one entry
per failure, framework-agnostic regardless of which provider produced it:

```csharp
public sealed record RequestValidationError(
    string PropertyName,
    string ErrorMessage,
    object? AttemptedValue = null,
    string? ErrorCode = null);
```

`PropertyName` and `ErrorMessage` identify the field and the problem; the optional
`AttemptedValue` and `ErrorCode` carry the rejected value and a machine-readable
code when the provider supplies them. Catch the exception and read the failures:

```csharp
using Paramore.Brighter.RequestValidation;

try
{
    commandProcessor.Send(command);
}
catch (RequestValidationException exception)
{
    foreach (var error in exception.Errors)
        Console.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
}
```

Because every provider throws the **same** exception type, edge code can handle
validation failures in one place. In a web API, for example, you can catch
`RequestValidationException` and map its `Errors` to an HTTP 422 (Unprocessable
Entity) response without caring whether FluentValidation, DataAnnotations, or the
Specification provider rejected the request.

If you are validating on an **External Bus** consumer rather than at an API edge,
the exception is caught by Brighter's message pump, not by your calling code. What
happens to the message then — acknowledge, requeue, or dead-letter — is governed by
your error-handling strategy; see [Error Handling](/contents/HandlerFailure.md).

### Missing validator is a configuration error

Marking a handler with `[ValidateRequest]` but registering no validator for that
request is treated as a **configuration error, not a silent skip**. The
FluentValidation and Specification providers resolve a validator
(`IValidator<TRequest>` / `ISpecification<TRequest>`) from the container; if none is
registered, the provider throws a `ConfigurationException` — Brighter's standard
misconfiguration type — so the mistake fails fast rather than letting an unvalidated
request through.

DataAnnotations has no separate validator to register (the rules live on the
request), so it has no missing-validator failure mode.

## Async Validation

Every provider has an asynchronous counterpart for the
[async pipeline](/contents/AsyncDispatchARequest.md). Mark the handler's
`HandleAsync` method with `[ValidateRequestAsync]` instead of `[ValidateRequest]`,
and dispatch with `SendAsync`:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.RequestValidation.Attributes;

public sealed class RegisterUserHandler : RequestHandlerAsync<RegisterUser>
{
    [ValidateRequestAsync(step: 1, timing: HandlerTiming.Before)]
    public override async Task<RegisterUser> HandleAsync(
        RegisterUser command, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Registered {command.Name} <{command.Email}>");
        return await base.HandleAsync(command, cancellationToken);
    }
}
```

```csharp
await commandProcessor.SendAsync(new RegisterUser { Name = "", Email = "" });
```

Registration is identical — the same `UseFluentValidation()` /
`UseDataAnnotations()` / `UseSpecification()` call wires up **both** the sync and
async pipelines. As before, an invalid request throws a
`RequestValidationException` before `HandleAsync` runs.

## Writing a Custom Provider

If your team has an in-house validation framework, you can plug it in as a provider.
A provider is two pieces: a concrete handler that derives from the abstract base and
turns your rules into `RequestValidationError`s, and a `UseX()` extension that maps
the abstract handler to your concrete one.

```csharp
using Paramore.Brighter;
using Paramore.Brighter.RequestValidation;
using Paramore.Brighter.RequestValidation.Handlers;

// 1. Derive from the abstract base handler and return the failures.
public sealed class MyRequestHandler<TRequest> : ValidateRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    protected override IReadOnlyCollection<RequestValidationError> Validate(TRequest request)
    {
        // Run your framework's rules and map any failures onto RequestValidationError.
        // Return an empty collection when the request is valid.
        return [];
    }
}
```

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.RequestValidation.Handlers;

// 2. Map the provider-agnostic handler to your implementation.
public static class MyValidationBuilderExtensions
{
    public static IBrighterBuilder UseMyValidation(this IBrighterBuilder brighterBuilder)
    {
        brighterBuilder.Services.Add(new ServiceDescriptor(
            typeof(ValidateRequestHandler<>),
            typeof(MyRequestHandler<>),
            ServiceLifetime.Transient));
        return brighterBuilder;
    }
}
```

Provide a matching `ValidateRequestHandlerAsync<TRequest>`-derived handler (and
register it the same way) if you support the async pipeline. For the full pattern,
read [ADR-0063](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0063-request-validation-handler.md)
and one of the shipped providers under
`Brighter/src/Paramore.Brighter.Validation.*`.

## Further Reading

- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) — the
  attribute-and-handler middleware model request validation builds on.
- [Pipeline Validation and Diagnostics](/contents/PipelineValidation.md) — the
  *other* validation feature: startup checks of your pipeline configuration.
- [Error Handling](/contents/HandlerFailure.md) — what happens to an invalid
  message on an External Bus consumer, and how to choose requeue or dead-letter.
- [Dispatching An Async Request](/contents/AsyncDispatchARequest.md) — the async
  pipeline used by `[ValidateRequestAsync]`.
- [Basic Concepts](/contents/BasicConcepts.md) — Commands, Events, and Requests.
- Design rationale: [ADR-0063 Request validation handler](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0063-request-validation-handler.md)
  and [ADR-0040 Specification pattern](https://github.com/BrighterCommand/Brighter/blob/master/docs/adr/0040-add-the-specification-pattern.md).
- Working samples: `Brighter/samples/Validation/` —
  `FluentValidationSample`, `DataAnnotationsSample`, and `SpecificationSample`.

> **Darker queries.** Request validation currently applies to Brighter requests.
> The equivalent for Darker queries is planned but not yet shipped, so it is out of
> scope for this page.
