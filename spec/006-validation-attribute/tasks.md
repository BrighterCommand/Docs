# Writing Tasks: Request Validation in the Brighter Pipeline

**Spec:** 006-validation-attribute
**Created:** 2026-06-17
**Status:** Tasks Phase (awaiting review)
**Traces:** `design.md` (approved), `requirements.md` (approved)

## Overview

- **Total tasks:** 18 across 4 phases.
- **Primary deliverable:** `contents/RequestValidation.md` (one page, ~350–430 lines).
- **Secondary:** `contents/Glossary.md` edit (P2), `SUMMARY.md` edit.

| Phase | Goal | Tasks |
|-------|------|-------|
| 1. Research & Preparation | Confirm source accuracy; run the three samples | 1.1 – 1.3 |
| 2. Core Documentation (P0) | Write the essential sections of `RequestValidation.md` | 2.1 – 2.8 |
| 3. Supporting Documentation (P1/P2) | Decision guidance, custom provider, glossary | 3.1 – 3.4 |
| 4. Polish & Review | SUMMARY.md, links, code verification, final edit | 4.1 – 4.3 |

**Dependencies at a glance:** Phase 1 → Phase 2. Within Phase 2, Task 2.1 (file
skeleton) comes first; 2.2–2.8 can then be written in any order. Phase 3 depends on
the relevant Phase 2 sections existing. Phase 4 is last and depends on all writing
being done.

---

## Phase 1: Research & Preparation

- [x] **Task 1.1:** Run all three sample apps and capture their console output
  - Input: `../Brighter/samples/Validation/{FluentValidationSample,DataAnnotationsSample,SpecificationSample}/`; the sample `README.md`
  - Output: Verified valid/invalid output for each (the error lines the docs will describe), saved as notes
  - Notes: `dotnet run --project samples/Validation/<name>`. Confirms `RequestValidationException.Errors` shape and the printed `PropertyName: ErrorMessage`.
  - **DONE:** All three build and run. Invalid runs each report 2 errors: FV `Name`/`Email` "must not be empty"; DA `Name` "must not be empty" + `Email` "must be a valid email address"; Spec `Quantity` "must be greater than zero" + `Sku` "must not be empty". Valid runs reach the handler.

- [x] **Task 1.2:** Verify core type signatures against source
  - Input: `../Brighter/src/Paramore.Brighter/RequestValidation/` — `Attributes/`, `Handlers/`, `RequestValidationException.cs`, `RequestValidationError.cs`
  - Output: Confirmed namespaces, ctor params (`step`, `timing`), abstract base handler names, `RequestValidationError(PropertyName, ErrorMessage, AttemptedValue?, ErrorCode?)`, `Errors` collection type
  - Notes: These are quoted verbatim in the doc — accuracy matters. Cross-check ADR-0063.
  - **DONE / refinements:** `ValidateRequestAttribute(int step, HandlerTiming timing = HandlerTiming.Before)` — **timing defaults to `Before`** (`[ValidateRequest(step: 1)]` is valid). `GetHandlerType()` → `typeof(ValidateRequestHandler<>)` (async → `ValidateRequestHandlerAsync<>`). Abstract hook is **`protected abstract IReadOnlyCollection<RequestValidationError> Validate(TRequest)`** (async: `protected abstract Task<IReadOnlyCollection<RequestValidationError>> ValidateAsync(TRequest, CancellationToken)`). Base handler null-guards (`ArgumentNullException`), throws `RequestValidationException($"Validation failed for {TRequest.Name}.", errors)` when any, else calls `base.Handle`.

- [x] **Task 1.3:** Verify provider registration + Specification API
  - Input: `../Brighter/src/Paramore.Brighter.Validation.{FluentValidation,DataAnnotations,Specification}/*BuilderExtensions.cs`; `../Brighter/src/Paramore.Brighter/Specification.cs` (and `ISpecification`)
  - Output: Confirmed `UseFluentValidation()/UseDataAnnotations()/UseSpecification()` signatures (`this IBrighterBuilder`); `Specification<T>` ctor overloads, `And/Or`, `ValidationError`/`ValidationSeverity`; confirmed missing-validator → `ConfigurationException`
  - Notes: Also confirm the `Specification<T>` per-request lifetime claim (sample README warning).
  - **DONE / refinements:** Each `UseX()` registers **both** `ValidateRequestHandler<>` and `ValidateRequestHandlerAsync<>` → its concrete handler as **Transient** ServiceDescriptors, returns the builder. **`UseFluentValidation()` does NOT register your validators** — you still register `IValidator<TRequest>` yourself (`AddSingleton(...)` / `AddValidatorsFromAssembly(...)`); same for `UseSpecification()` (register `ISpecification<TRequest>`). **Missing validator → `ConfigurationException`** confirmed for FluentValidation + Specification; **DataAnnotations has no separate validator** to miss (always validates the request's attributes).

---

## Phase 2: Core Documentation — `RequestValidation.md` (P0)

- [x] **Task 2.1:** Create the file skeleton (H1, intro, all H2/H3 headings as stubs)
  - Input: `design.md` section outline
  - Output: `contents/RequestValidation.md` with title, 2–3 sentence intro, and every heading from the outline in order
  - Notes: Establishes the structure so later tasks fill sections independently. **Blocks 2.2–2.8.**
  - **DONE:** Created `contents/RequestValidation.md` — H1, written intro, and all 13 headings (H2/H3) in order, each with an HTML-comment stub naming its task. Headings unblock 2.2–2.8.

- [x] **Task 2.2:** Write "Request Validation vs. Pipeline Validation" + "Why Validate in the Pipeline"
  - Input: `design.md`; `PipelineValidation.md`; `BuildingAPipeline.md`; `HandlerFailure.md`
  - Output: The disambiguation callout (this = runtime data validation; that = startup wiring checks; `RequestValidationError` vs. `ValidationError`) and the Why section (reject bad input early; orthogonal concern; **simplifies handler/domain code**; one exception type at the edge)
  - Notes: Cross-link `PipelineValidation.md` and `BuildingAPipeline.md` on first mention. Disambiguation appears near the top.

- [x] **Task 2.3:** Write "How It Works" (provider-agnostic model)
  - Input: `design.md`; ADR-0063; `BuildingAPipeline.md`; `HandlerFailure.md`
  - Output: Attribute → abstract base handler → concrete provider via `UseX()`; one attribute works for all; `step:1, timing: HandlerTiming.Before`; **External Bus note** linking `HandlerFailure.md` (thrown exception → pump acks/discards by default; see Error Handling for requeue/DLQ)
  - Notes: Example #1 (`[ValidateRequest(...)]` on a handler) from `FluentValidationSample/GreetingCommandHandler.cs`. Link `BuildingAPipeline.md` for mechanics.

- [x] **Task 2.4:** Write "Quick Start" (end-to-end with DataAnnotations)
  - Input: `DataAnnotationsSample/{RegisterUser,RegisterUserHandler,Program}.cs`
  - Output: Define rules → mark handler → `UseDataAnnotations()` → send + catch (complete, runnable)
  - Notes: Example #2. Keep request type consistent within the section. Least-ceremony provider first.

- [x] **Task 2.5:** Write "FluentValidation" section
  - Input: `FluentValidationSample/{GreetingCommandValidator,Program}.cs`
  - Output: When to use; validator example (#4, complete); registration `AddSingleton<IValidator<GreetingCommand>>(...)` + `.UseFluentValidation()` (#5, abbreviated)
  - Notes: Note the provider resolves `IValidator<TRequest>` from the container. Link FluentValidation site (external).

- [x] **Task 2.6:** Write "DataAnnotations" section
  - Input: `DataAnnotationsSample/{RegisterUser,Program}.cs`
  - Output: When to use; request-with-attributes example (#6, complete); `.UseDataAnnotations()` (#7, abbreviated); nothing else to register
  - Notes: May reference the Quick Start request rather than redefine it. Link `System.ComponentModel.DataAnnotations` docs (external).

- [x] **Task 2.7:** Write "Specification" section + lifetime H3
  - Input: `SpecificationSample/{OrderSpecification,Program}.cs`; ADR-0040
  - Output: When to use; `OrderSpecification.Create()` with `And` + `ValidationError` (#8, complete); registration `AddTransient<ISpecification<PlaceOrder>>(...)` + `.UseSpecification()` (#9); **"Lifetime: register per request" H3** (transient/scoped, never singleton)
  - Notes: Carefully distinguish the Specification's `ValidationError` from `RequestValidationError`. Link ADR-0040.

- [x] **Task 2.8:** Write "Handling Validation Failures" + missing-validator H3
  - Input: `design.md`; any `*/Program.cs` catch block; ADR-0063; `HandlerFailure.md`
  - Output: `RequestValidationException.Errors` (`IReadOnlyCollection<RequestValidationError>`); iterate errors (#10, complete); HTTP 422 mapping note; **External Bus reminder** linking `HandlerFailure.md`; **missing validator → `ConfigurationException`** H3 (fail-fast)
  - Notes: Example #10. Show `PropertyName`/`ErrorMessage`; mention `AttemptedValue`/`ErrorCode` exist.

---

## Phase 3: Supporting Documentation (P1/P2)

- [x] **Task 3.1:** Write "Choosing a Provider" comparison table
  - Input: sample `README.md`; sections 2.5–2.7
  - Output: Table contrasting FluentValidation (reusable fluent rules apart from request) / DataAnnotations (simple rules that travel with request) / Specification (composable domain rules, no extra dependency) + one line each (#3)
  - Notes: Depends on 2.5–2.7 wording for consistency.

- [x] **Task 3.2:** Write "Async Validation" section
  - Input: sample `README.md` "Async" section; `AsyncDispatchARequest.md`
  - Output: `[ValidateRequestAsync]` + `await commandProcessor.SendAsync(...)`; same `UseX()` wires both pipelines (#12, abbreviated)
  - Notes: Link `AsyncDispatchARequest.md`.

- [x] **Task 3.3:** Write "Writing a Custom Provider" (light skeleton) + "Further Reading"
  - Input: `src/Paramore.Brighter/RequestValidation/Handlers/`; a provider `*BuilderExtensions.cs`; ADR-0063
  - Output: Skeleton — derive `ValidateRequestHandler<TRequest>`/async, implement `Validate`/`ValidateAsync` returning `RequestValidationError[]`, add a `UseX()` registration (#13, abbreviated); point to ADR-0063 + a shipped provider; Further Reading list (incl. Darker out-of-scope note)
  - Notes: Deliberately light — for teams with an internal framework, not a full tutorial.

- [x] **Task 3.4:** Update `Glossary.md` (P2)
  - Input: `contents/Glossary.md`; finished `RequestValidation.md`
  - Output: "Request Validation" entry + Specification-as-validator cross-reference, linking `RequestValidation.md`
  - Notes: Match existing Glossary ordering/format.

---

## Phase 4: Polish & Review

- [x] **Task 4.1:** Update `SUMMARY.md`
  - Input: `design.md` before/after diff
  - Output: `* [Request Validation](/contents/RequestValidation.md)` after Feature Switches in the "Brighter Request Handlers and Middleware Pipelines" section
  - Notes: Match existing indentation (spaces). No orphaned file.

- [x] **Task 4.2:** Verify all code examples compile/run
  - Input: every snippet in `RequestValidation.md`; the sample projects
  - Output: Confirmation each example matches a building sample; fix any drift
  - Notes: Re-run samples if needed (per Task 1.1). Required by CLAUDE.md.

- [x] **Task 4.3:** Link check + final edit pass
  - Input: finished `RequestValidation.md`, `Glossary.md`, `SUMMARY.md`
  - Output: All internal links resolve (`BuildingAPipeline.md`, `PipelineValidation.md`, `HandlerFailure.md`, `AsyncDispatchARequest.md`, `BasicConcepts.md`, Glossary); terminology consistent (request vs. pipeline validation; `RequestValidationError` vs. `ValidationError`); voice/style per CLAUDE.md; length within target
  - Notes: Confirm `HandlerFailure.md` is linked under its ToC title "Error Handling". Final read-through for newcomer clarity.

---

**Next step:** run `/spec:review` to approve these tasks, then `/spec:implement` to begin writing.
