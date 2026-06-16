# Requirements: Request Validation in the Brighter Pipeline

**Spec:** 006-validation-attribute
**Created:** 2026-06-16
**Status:** Requirements Phase (awaiting review)

## Topic Overview

Brighter has added the ability to **validate a request before the business
handler runs**, as a step in the request-handling pipeline. Validation is
opt-in per handler: you mark the handler's `Handle`/`HandleAsync` method with
`[ValidateRequest]` (or `[ValidateRequestAsync]`), and you choose *how* the
request is validated by registering one of three provider packages —
**FluentValidation**, **DataAnnotations**, or Brighter's own **Specification**
pattern. When a request is invalid, the pipeline throws a
`RequestValidationException` carrying the individual failures, and the business
handler never runs.

This documentation is needed because:

- The feature is **new** (ADR-0063, June 2026) and has **no current coverage**
  in the Docs repository.
- It introduces a provider-agnostic attribute with three pluggable backends, a
  pattern that needs a clear "what / why / how" explanation.
- There is a **naming collision risk** with an existing, unrelated feature
  (*startup pipeline validation*, ADR-0053, documented in `PipelineValidation.md`).
  The docs must clearly distinguish runtime *request* validation from
  startup *pipeline* validation.

## Current State

**What exists today:**

- `contents/BuildingAPipeline.md` — explains the attribute + handler middleware
  model (Russian Doll, `RequestHandlerAttribute`, `GetHandlerType()`,
  `step`/`timing`). This is the conceptual foundation request validation builds
  on, and the new doc should link to it rather than re-explain it.
- `contents/PipelineValidation.md` — documents **startup** pipeline validation
  and diagnostics (ADR-0053/0054). This is a **different concern** and a likely
  source of confusion; the new doc must cross-link and disambiguate.
- `contents/BasicConcepts.md` and `contents/Glossary.md` — glossary terms for
  Command, Event, Request, Handler, Pipeline.

**What is missing:**

- No documentation of `[ValidateRequest]` / `[ValidateRequestAsync]`.
- No documentation of the three provider packages or their `UseX()` registration.
- No documentation of `RequestValidationException` / `RequestValidationError`.
- No documentation of how a caller (e.g. an API mapping to HTTP 422) handles
  validation failures.
- No glossary entry for "request validation" or the Specification pattern as a
  validation provider.

## Target State

A reader should be able to:

1. Understand what request validation is, why it runs in the pipeline, and how it
   differs from startup pipeline validation.
2. Add validation to a handler with `[ValidateRequest]` and pick a provider.
3. Choose the *right* provider for their situation (decision guidance).
4. Write validators for each of the three providers (FluentValidation,
   DataAnnotations, Specification).
5. Handle a `RequestValidationException` and read its structured errors.
6. Use the async pipeline (`[ValidateRequestAsync]` + `SendAsync`).
7. Understand failure modes — invalid request vs. missing validator
   (`ConfigurationException`, fail-fast).

## Target Audience

- **Primary:** Intermediate Brighter users who already dispatch commands and
  understand handlers, and now want to validate input.
- **Secondary:** Beginners (must define terms and link to pipeline basics) and
  advanced users (provider internals, writing a custom provider, source links).

## Source Material

**ADRs (`../Brighter/docs/adr/`):**

- `0063-request-validation-handler.md` — **primary source**: the full design,
  naming decisions, core vs. provider split, missing-validator behaviour.
- `0040-add-the-specification-pattern.md` — background on the Specification pattern.
- `0053-pipeline-validation-at-startup.md` — the *other* validation feature, to
  disambiguate from.

**Core source (`../Brighter/src/Paramore.Brighter/RequestValidation/`):**

- `Attributes/ValidateRequestAttribute.cs`, `Attributes/ValidateRequestAsyncAttribute.cs`
  (namespace `Paramore.Brighter.RequestValidation.Attributes`) — extend
  `RequestHandlerAttribute`; ctor takes `step` and `timing`.
- `Handlers/ValidateRequestHandler.cs`, `Handlers/ValidateRequestHandlerAsync.cs`
  (namespace `Paramore.Brighter.RequestValidation.Handlers`) — **abstract** base
  handlers; own the shared behaviour (null-guard, ask provider for failures,
  throw on failure, otherwise call `base.Handle`); expose abstract
  `Validate`/`ValidateAsync`.
- `RequestValidationException.cs` — `public class RequestValidationException :
  Exception`; carries `IReadOnlyCollection<RequestValidationError> Errors`.
- `RequestValidationError.cs` — `public sealed record RequestValidationError(
  string PropertyName, string ErrorMessage, object? AttemptedValue = null,
  string? ErrorCode = null)`.

**Provider packages (`../Brighter/src/`):**

- `Paramore.Brighter.Validation.FluentValidation` —
  `FluentValidationBuilderExtensions.UseFluentValidation(this IBrighterBuilder)`;
  resolves `IValidator<TRequest>` from the container.
- `Paramore.Brighter.Validation.DataAnnotations` —
  `DataAnnotationsBuilderExtensions.UseDataAnnotations(this IBrighterBuilder)`;
  validates `System.ComponentModel.DataAnnotations` attributes on the request.
- `Paramore.Brighter.Validation.Specification` —
  `SpecificationBuilderExtensions.UseSpecification(this IBrighterBuilder)`;
  resolves `ISpecification<TRequest>`.

**Specification types (core, `Paramore.Brighter`):**

- `ISpecification<TData>` / `Specification<T>` — `IsSatisfiedBy`, `And`, `Or`,
  `AndNot`, `OrNot`; ctor overloads taking a predicate + `ValidationError`
  factory. `ValidationError`, `ValidationSeverity`.

**Samples (`../Brighter/samples/Validation/`):** **primary, tested examples.**

- `README.md` — concise overview of all three providers.
- `FluentValidationSample/` — `IValidator<GreetingCommand>` + `[ValidateRequest]`.
- `DataAnnotationsSample/` — `[Required]`/`[EmailAddress]` on `RegisterUser`.
- `SpecificationSample/` — `OrderSpecification` composed with `And`; **note the
  per-request (transient/scoped) lifetime requirement** for `Specification<T>`.

## Scope

### P0 — Must Have

1. **Conceptual intro**: what request validation is, why validate in the
   pipeline (fail before the business handler; reject bad input early), and an
   explicit contrast with startup pipeline validation (`PipelineValidation.md`).
2. **The provider-agnostic model**: one `[ValidateRequest]` attribute → abstract
   base handler → concrete provider chosen by which `UseX()` is registered. Link
   to `BuildingAPipeline.md` for the underlying attribute/handler mechanism.
3. **Quick start**: minimal end-to-end example (mark handler, register a provider,
   send a request, catch `RequestValidationException`).
4. **All three providers**, each with: when to reach for it, registration
   (`UseFluentValidation()` / `UseDataAnnotations()` / `UseSpecification()`), and
   a complete validator example drawn from the samples.
5. **Failure handling**: `RequestValidationException` and `RequestValidationError`
   (PropertyName, ErrorMessage, AttemptedValue, ErrorCode); how to iterate errors.
6. **Missing validator = `ConfigurationException`** (fail-fast, not silently
   skipped).
7. **Async pipeline**: `[ValidateRequestAsync]` + `SendAsync`; same registration.

### P1 — Should Have

8. **Provider decision guidance** — a short comparison table (FluentValidation =
   reusable fluent rule sets apart from the request; DataAnnotations = simple rules
   that travel with the request; Specification = composable domain rules, no extra
   dependency).
9. **Specification lifetime gotcha** — `Specification<T>` records per-evaluation
   state; register transient/scoped, never singleton.
10. **Mapping to HTTP** — note that one exception type lets edge code map to e.g.
    HTTP 422 regardless of provider.
11. **`step`/`timing`** explanation in the validation context (why
    `step: 1, timing: HandlerTiming.Before`), linking to `BuildingAPipeline.md`.

### P2 — Nice to Have

12. **Writing a custom provider** — keep it **light but sufficient for teams with
    an internal validation framework**: show the shape (derive from the abstract
    base handler, implement `Validate`/`ValidateAsync` to return
    `RequestValidationError`s, add a `UseX()` registration that maps the abstract
    handler to the concrete one), and point to ADR-0063 and a shipped provider for
    the full pattern. A short worked skeleton, not an exhaustive guide.
13. **Glossary entries** for "request validation" and "Specification pattern".
14. **Darker note** — request validation for Darker queries is out of scope /
    future work (per ADR-0063), stated briefly so readers don't go looking.

## Out of Scope

- **Startup pipeline validation / Roslyn analyzers** (ADR-0053/0054) — already in
  `PipelineValidation.md`; only cross-link and disambiguate.
- **Darker query validation** — not yet shipped (ADR-0063 defers it to V5).
- A full tutorial on FluentValidation, DataAnnotations, or the Specification
  pattern as libraries — link out; document only the Brighter integration.
- The Specification pattern's use in the Mediator / Agreement Dispatcher
  (different use of the same type) beyond a passing cross-link.

## Documentation Deliverables

1. **`contents/RequestValidation.md`** (new) — **a single page** covering
   everything: conceptual intro, provider-agnostic model, quick start, the three
   providers, failure handling, async, decision guidance, gotchas, a light
   custom-provider skeleton, and further reading. Follows the conceptual + how-to
   hybrid pattern. Target ~300–450 lines.

2. **`contents/Glossary.md`** (update, P2) — add "Request Validation" entry and a
   cross-reference for the Specification pattern as a validation provider.

*(Decision: one page. Keep per-provider detail in this single file. Only split if
it grows past ~500 lines in practice.)*

## SUMMARY.md Changes

Add the new page to the **"Brighter Request Handlers and Middleware Pipelines"**
section, after Feature Switches (it is a pipeline middleware concern):

```markdown
## Brighter Request Handlers and Middleware Pipelines

 ...
 * [Feature Switches](/contents/FeatureSwitches.md)
 * [Request Validation](/contents/RequestValidation.md)
```

## Constraints

- Follow **CLAUDE.md**: PascalCase filename, V10 patterns, `csharp` fenced code,
  second person, active voice, "Dispatcher" not "ServiceActivator".
- **Test all code examples** against the `samples/Validation/` projects; prefer
  copying from the working samples over inventing new snippets.
- **Terminology**: use "request validation" (runtime) and reserve "pipeline
  validation" for the startup feature — never blur the two.
- **Cross-link** on first mention: `BuildingAPipeline.md` (attribute/handler
  model), `PipelineValidation.md` (the other validation feature),
  `AsyncDispatchARequest.md` (async pipeline), `BasicConcepts.md` (Command/Request).
- Reference exact source paths and ADR-0063 for advanced readers.
- Do **not** modify any files under `../Brighter`.

## Resolved Decisions

1. **Page structure** — *Single page* (`contents/RequestValidation.md`). Per-provider
   detail stays in the one file.
2. **Section placement** — Under *"Brighter Request Handlers and Middleware
   Pipelines"*, after Feature Switches.
3. **Custom-provider depth** — *Light but sufficient for teams with an internal
   framework*: a short worked skeleton (abstract base handler + `Validate` +
   `UseX()` registration) plus a pointer to ADR-0063 and a shipped provider. Not an
   exhaustive guide.

---

**Next step:** run `/spec:review` when ready to approve these requirements.
