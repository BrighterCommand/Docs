# Design: Request Validation in the Brighter Pipeline

**Spec:** 006-validation-attribute
**Created:** 2026-06-17
**Status:** Design Phase (awaiting review)
**Traces:** `requirements.md` (approved)

This design turns the approved requirements into a writeable outline for a single
new page, `contents/RequestValidation.md`, plus a small Glossary update and a
SUMMARY.md entry.

## Documentation Structure

### File hierarchy

```
contents/
├── RequestValidation.md      (NEW)  ← the whole feature, one page
├── Glossary.md               (EDIT) ← add "Request Validation" term (P2)
├── BuildingAPipeline.md       (link target — attribute/handler model)
├── PipelineValidation.md      (link target — the OTHER validation feature)
├── AsyncDispatchARequest.md   (link target — async pipeline)
└── BasicConcepts.md           (link target — Command / Request / Handler)
SUMMARY.md                     (EDIT) ← add page under middleware section
```

`RequestValidation.md` is self-contained: it explains the concept, then walks
through usage. It does **not** re-teach the pipeline/attribute machinery — it links
to `BuildingAPipeline.md` for that.

### Reading order for users

1. Land on **Request Validation** from the middleware section of the ToC.
2. Read the intro + the "Request validation vs. pipeline validation" callout to
   confirm they're in the right place.
3. Read "How it works" (provider-agnostic model) → "Quick start" to get something
   running.
4. Jump to "Choosing a provider", then the one provider section that fits them.
5. Read "Handling validation failures" to wire up the catch site.
6. Optional tails: "Async validation", "Writing a custom provider".

Newcomers who don't yet know the pipeline follow the link to
`BuildingAPipeline.md` from "How it works" and return.

## File-by-File Outline

### 1. `contents/RequestValidation.md` (NEW)

- **Purpose:** Explain and show how to validate a request in the Brighter pipeline
  before the business handler runs, using `[ValidateRequest]` and one of three
  providers.
- **Target length:** ~350–430 lines.
- **Type:** Conceptual + how-to hybrid (per CLAUDE.md).

#### Section outline

```
# Request Validation                                            (H1)

  intro — 2–3 sentences: validate a request in the pipeline before the
  business handler runs; opt-in per handler; one attribute, three providers;
  invalid → RequestValidationException, handler never runs.

## Request Validation vs. Pipeline Validation                   (H2)
  Short callout. THIS page = runtime validation of a request's data.
  PipelineValidation.md = startup checks that your handler pipelines are
  wired correctly. Different concern, different type
  (RequestValidationError here vs. ValidationError there). Cross-link.

## Why Validate in the Pipeline                                 (H2)
  - Reject bad input before it reaches business logic / the database.
  - Cross-cutting concern → belongs in middleware, not every handler.
  - Simplifies handler/domain code: lifting validation out as an orthogonal
    concern keeps the business handler focused on business logic, not on
    guarding its inputs.
  - One exception type at the edge regardless of provider (e.g. map to 422).
  Link BuildingAPipeline.md (orthogonal concerns / Russian Doll).

## How It Works                                                 (H2)
  The provider-agnostic model:
  - Mark Handle/HandleAsync with [ValidateRequest] / [ValidateRequestAsync].
  - The attribute points at an ABSTRACT base validation handler.
  - Registering a provider (UseX()) maps that abstract handler to a concrete
    implementation → one attribute works for every framework.
  - On failure the handler throws RequestValidationException; the business
    handler never runs.
  - NOTE (External Bus): when validation runs in a handler dispatched from an
    External Bus, a thrown RequestValidationException is an unhandled exception
    leaving the pipeline — by default the message pump logs it and acknowledges
    (discards) the message. Readers should consult Error Handling
    (HandlerFailure.md) to understand their options (requeue, DLQ, etc.).
    Cross-link HandlerFailure.md here.
  Mention step / timing (step:1, timing: HandlerTiming.Before) and link
  BuildingAPipeline.md for the attribute mechanics.

## Quick Start                                                  (H2)
  Minimal end-to-end with ONE provider (DataAnnotations — least ceremony):
  1. Annotate/define rules.   2. Mark the handler.   3. Register UseDataAnnotations().
  4. Send + catch RequestValidationException.

## Choosing a Provider                                          (H2)
  Comparison table (see Code/Tables plan) + one line each on when to reach
  for which.

## FluentValidation                                            (H2)
  When to use; register a IValidator<TRequest> + UseFluentValidation();
  validator example; note it resolves IValidator<TRequest> from the container.

## DataAnnotations                                             (H2)
  When to use; constraints live on the request type; UseDataAnnotations();
  request-with-attributes example; nothing else to register.

## Specification                                               (H2)
  When to use; ISpecification<TRequest> built with And/Or; UseSpecification();
  specification example.
### Lifetime: register per request                             (H3)
  Specification<T> records per-evaluation state → register transient/scoped,
  NEVER singleton. (Reproduce the sample's warning.)

## Handling Validation Failures                                (H2)
  RequestValidationException.Errors : IReadOnlyCollection<RequestValidationError>.
  RequestValidationError(PropertyName, ErrorMessage, AttemptedValue?, ErrorCode?).
  Show iterating errors; note mapping to HTTP 422 at an API edge.
  External Bus reminder: on a consumer the exception is caught by the message
  pump, not your caller — link Error Handling (HandlerFailure.md) for what
  happens to the message and how to choose requeue / DLQ.
### Missing validator is a configuration error                 (H3)
  [ValidateRequest] with no registered validator → ConfigurationException
  (fail-fast, not silently skipped).

## Async Validation                                            (H2)
  [ValidateRequestAsync] + SendAsync; same UseX() registration wires both
  pipelines. Link AsyncDispatchARequest.md.

## Writing a Custom Provider                                   (H2)
  LIGHT (for teams with an internal framework):
  - Derive from the abstract base handler ValidateRequestHandler<TRequest> /
    ValidateRequestHandlerAsync<TRequest>.
  - Implement Validate / ValidateAsync to return RequestValidationError[].
  - Add a UseX() extension mapping the abstract handler → your concrete one.
  Skeleton only; point to ADR-0063 and a shipped provider for the full pattern.

## Further Reading                                             (H2)
  BuildingAPipeline.md, PipelineValidation.md, HandlerFailure.md (Error
  Handling), AsyncDispatchARequest.md, BasicConcepts.md, ADR-0063, ADR-0040,
  samples/Validation/.
```

#### Cross-links (first mention)

- `BuildingAPipeline.md` — in "Why Validate" and "How It Works".
- `PipelineValidation.md` — in the disambiguation callout.
- `HandlerFailure.md` (titled "Error Handling") — in "How It Works" (External Bus
  note) and again in "Handling Validation Failures": when validation runs on an
  External Bus consumer, the thrown exception's fate (ack/requeue/DLQ) is governed
  by Brighter's error-handling strategy.
- `AsyncDispatchARequest.md` — in "Async Validation".
- `BasicConcepts.md` — on first use of *Command* / *Request*.
- External: FluentValidation site, `System.ComponentModel.DataAnnotations` docs,
  ADR-0063 and ADR-0040 (source references).

#### Glossary terms

- Reference existing: *Command*, *Request*, *Handler*, *Pipeline*.
- Introduce/define: *request validation*, *validation provider*, and *Specification
  pattern* (as a validation provider).

### 2. `contents/Glossary.md` (EDIT, P2)

- **Purpose:** Add a "Request Validation" entry and a Specification-as-validator
  cross-reference so the term is discoverable.
- **Change:** One new term block linking back to `RequestValidation.md`. Keep
  alphabetical/section ordering consistent with the existing file (verify on edit).

## SUMMARY.md Changes

Placement: **"Brighter Request Handlers and Middleware Pipelines"** section, after
Feature Switches.

**Before:**

```markdown
 * [Failure and Fallback](/contents/PolicyFallback.md)
 * [Feature Switches](/contents/FeatureSwitches.md)

## Darker Query Handlers and Middleware Pipelines
```

**After:**

```markdown
 * [Failure and Fallback](/contents/PolicyFallback.md)
 * [Feature Switches](/contents/FeatureSwitches.md)
 * [Request Validation](/contents/RequestValidation.md)

## Darker Query Handlers and Middleware Pipelines
```

## Code Examples Plan

All C# examples are **copied/adapted from the tested sample projects** under
`../Brighter/samples/Validation/` unless noted. Each is verified to compile against
its sample before finalising.

| # | Example | Source | Complete / Abbrev |
|---|---------|--------|-------------------|
| 1 | Mark a handler with `[ValidateRequest(step: 1, timing: HandlerTiming.Before)]` | `FluentValidationSample/GreetingCommandHandler.cs` | Complete |
| 2 | **Quick start** — `RegisterUser` with DataAnnotations + handler + `UseDataAnnotations()` + send/catch | `DataAnnotationsSample/{RegisterUser,RegisterUserHandler,Program}.cs` | Complete |
| 3 | Provider comparison table | Written (from sample README) | n/a (table) |
| 4 | FluentValidation: `GreetingCommandValidator : AbstractValidator<GreetingCommand>` | `FluentValidationSample/GreetingCommandValidator.cs` | Complete |
| 5 | FluentValidation registration: `AddSingleton<IValidator<GreetingCommand>>(...)` + `.UseFluentValidation()` | `FluentValidationSample/Program.cs` | Abbrev (`// ...`) |
| 6 | DataAnnotations request: `[Required]` / `[EmailAddress]` on `RegisterUser` | `DataAnnotationsSample/RegisterUser.cs` | Complete |
| 7 | DataAnnotations registration: `.UseDataAnnotations()` | `DataAnnotationsSample/Program.cs` | Abbrev |
| 8 | Specification: `OrderSpecification.Create()` with `And` + `ValidationError` | `SpecificationSample/OrderSpecification.cs` | Complete |
| 9 | Specification registration: `AddTransient<ISpecification<PlaceOrder>>(...)` + `.UseSpecification()` (lifetime note) | `SpecificationSample/Program.cs` | Complete |
| 10 | Catch `RequestValidationException` and iterate `Errors` (PropertyName/ErrorMessage) | `*/Program.cs` (shared shape) | Complete |
| 11 | Missing-validator note (no code, or 2-line `ConfigurationException` mention) | ADR-0063 | Abbrev / prose |
| 12 | Async: `[ValidateRequestAsync]` + `await commandProcessor.SendAsync(...)` | Adapt sample (README "Async" section) | Abbrev |
| 13 | Custom-provider skeleton: derive base handler, implement `Validate`, `UseX()` | Written from `src/Paramore.Brighter/RequestValidation/Handlers/` + a provider's `*BuilderExtensions.cs` | Abbrev skeleton |

Source types to reference precisely (namespaces):

- `Paramore.Brighter.RequestValidation.Attributes.ValidateRequestAttribute` /
  `ValidateRequestAsyncAttribute`
- `Paramore.Brighter.RequestValidation.Handlers.ValidateRequestHandler<TRequest>` /
  `ValidateRequestHandlerAsync<TRequest>` (abstract bases)
- `Paramore.Brighter.RequestValidation.RequestValidationException` (`.Errors`)
- `Paramore.Brighter.RequestValidation.RequestValidationError(PropertyName,
  ErrorMessage, AttemptedValue?, ErrorCode?)`
- Providers: `UseFluentValidation()`, `UseDataAnnotations()`, `UseSpecification()`
  (each `this IBrighterBuilder`)
- `ISpecification<T>` / `Specification<T>`, `ValidationError`, `ValidationSeverity`

## Style Notes

### Terminology decisions

- **"Request validation"** = the runtime feature on this page. **"Pipeline
  validation"** = the startup feature (`PipelineValidation.md`). Never blur them;
  the disambiguation callout is mandatory and appears near the top.
- Two distinct error types must be named carefully: **`RequestValidationError`**
  (this feature) vs. the pre-existing **`ValidationError`** (startup pipeline
  validation, and the Specification pattern's per-rule error). Where both appear
  near each other (Specification section + failure section), fully qualify or call
  out the difference so readers don't conflate them.
- Use "provider" for a validation backend (FluentValidation / DataAnnotations /
  Specification). Use "Dispatcher" not "ServiceActivator" (CLAUDE.md).

### Deviations from standard patterns

- A **disambiguation callout near the top** is non-standard but justified by the
  naming collision with `PipelineValidation.md`.
- The "Writing a Custom Provider" section is deliberately a **skeleton, not a full
  tutorial** (per resolved decision 3) — it points out to ADR-0063 rather than
  exhaustively documenting the abstract base handler.
- Single page rather than concept + per-provider split (resolved decision 1).

## Traceability to Requirements

| Requirement | Covered by section |
|---|---|
| P0-1 Conceptual intro + disambiguation | Intro, "vs. Pipeline Validation", "Why Validate" |
| P0-2 Provider-agnostic model | "How It Works" |
| P0-3 Quick start | "Quick Start" |
| P0-4 Three providers | FluentValidation / DataAnnotations / Specification |
| P0-5 Failure handling | "Handling Validation Failures" |
| P0-6 Missing validator → ConfigurationException | "Missing validator…" (H3) |
| P0-7 Async pipeline | "Async Validation" |
| P1-8 Decision guidance | "Choosing a Provider" |
| P1-9 Specification lifetime | "Lifetime: register per request" (H3) |
| P1-10 HTTP mapping | "Handling Validation Failures" |
| P1-11 step/timing | "How It Works" |
| P2-12 Custom provider | "Writing a Custom Provider" |
| P2-13 Glossary | `Glossary.md` edit |
| P2-14 Darker note | brief line in "Further Reading" / intro |

---

**Next step:** run `/spec:review` to approve this design, then `/spec:tasks`.
