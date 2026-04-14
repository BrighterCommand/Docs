# Design: Pipeline Validation at Startup Documentation

**Spec**: 003-pipeline-validation-at-startup
**Created**: 2026-04-14

## Documentation Structure

```
contents/
├── BrighterBasicConfiguration.md   ← UPDATE: add validation recommendation callout
├── HowConfiguringTheDispatcherWorks.md  ← UPDATE: add consumer validation note
└── PipelineValidation.md          ← NEW: dedicated feature page (~300 lines)

SUMMARY.md                          ← UPDATE: add entry under "Brighter Configuration"
```

### Reading Order

1. Developer reads `BrighterBasicConfiguration.md` → sees recommendation to enable validation → follows link
2. Developer reads `PipelineValidation.md` → understands the feature, enables it, knows how to interpret output
3. Developer configuring consumers reads `HowConfiguringTheDispatcherWorks.md` → sees note about consumer validation → follows link

---

## File-by-File Outline

### 1. `contents/PipelineValidation.md` (NEW)

**Purpose**: Dedicated documentation for the pipeline validation and diagnostic report feature.
**Target length**: ~300 lines
**Cross-links to**: `BrighterBasicConfiguration.md`, `BuildingAPipeline.md`, `BuildingAnAsyncPipeline.md`, `Routing.md`, `HowConfiguringTheDispatcherWorks.md`
**Glossary terms to reference**: pipeline, middleware, handler, message mapper, subscription, publication, Reactor, Proactor

#### Section Outline

```
# Pipeline Validation and Diagnostics                          (H1)

  [2-3 sentence intro: what it is, why it matters, opt-in]

## Quick Start                                                  (H2)

  [Minimal code example: AddBrighter + ValidatePipelines + DescribePipelines]
  [What happens: validation runs at startup, diagnostic report logged]

## What Gets Checked                                            (H2)

  [Brief explanation of three configuration paths]

### Handler Pipeline Checks (AddBrighter)                       (H3)

  [Table: Rule, Severity, What it checks, Example error message]
  - Handler type visibility (Error)
  - Sync/async attribute consistency (Error)
  - Backstop attribute ordering (Warning)

### Producer Checks (AddProducers)                              (H3)

  [Table: Rule, Severity, What it checks, Example error message]
  - Publication RequestType not set (Error)
  - Publication RequestType doesn't implement IRequest (Error)

### Consumer Checks (AddConsumers)                              (H3)

  [Table: Rule, Severity, What it checks, Example error message]
  - Pump/handler type mismatch (Error)
  - No handler registered for subscription (Error)
  - RequestType implements neither ICommand nor IEvent (Warning)

## Diagnostic Report                                            (H2)

  [Explain DescribePipelines() output]

### Summary (Information Level)                                 (H3)

  [Example log line at Information level]

### Full Detail (Debug Level)                                   (H3)

  [Annotated example showing Handler Pipelines, Publications, Subscriptions sections]

## Configuration                                                (H2)

### Enabling Validation and Diagnostics                         (H3)

  [Code example: .ValidatePipelines().DescribePipelines()]
  [Explain these are independent — can enable one without the other]

### Controlling Error Behavior                                  (H3)

  [Code example: .ValidatePipelines(throwOnError: false)]
  [Explain: errors logged but startup continues]

### Conditional Enablement                                      (H3)

  [Code example: gating on IHostEnvironment]
  [Code example: gating on IConfiguration]

### How Validation Scales to Your Configuration                 (H3)

  [Explain: AddBrighter-only → handler checks only]
  [AddBrighter + AddProducers → handler + producer checks]
  [All three → all checks]
  [No special configuration needed — validation detects what's registered]

## Common Mistakes and Fixes                                    (H2)

  [Before/after code pairs for each validation rule]

### Async Handler with Sync Attributes                          (H3)
### Backstop After Resilience Pipeline                          (H3)
### Reactor Subscription with Async Handler                     (H3)
### Missing Handler for Subscription                            (H3)

## Further Reading                                              (H2)

  [Links to BuildingAPipeline.md, BuildingAnAsyncPipeline.md,
   Routing.md, HowConfiguringTheDispatcherWorks.md,
   PolicyRetryAndCircuitBreaker.md]
  [Link to ADR 0053 for design rationale]
```

#### Key Code Examples Needed

| # | Example | Source | Complete or Abbreviated |
|---|---------|--------|------------------------|
| 1 | Quick start: minimal configuration with ValidatePipelines + DescribePipelines | Based on `Brighter/samples/WebAPI/` | Complete |
| 2 | Diagnostic output: handler pipelines section | Based on `PipelineDiagnosticWriter.cs` output format | Complete (log output) |
| 3 | Diagnostic output: publications section | Based on `PipelineDiagnosticWriter.cs` output format | Complete (log output) |
| 4 | Diagnostic output: subscriptions section | Based on `PipelineDiagnosticWriter.cs` output format | Complete (log output) |
| 5 | throwOnError: false configuration | Written from scratch | Complete |
| 6 | Conditional enablement with IHostEnvironment | Written from scratch | Complete |
| 7 | Conditional enablement with IConfiguration | Written from scratch | Complete |
| 8 | Fix: async handler with sync attributes (before/after) | Written from scratch | Abbreviated |
| 9 | Fix: backstop after resilience pipeline (before/after) | Written from scratch | Abbreviated |
| 10 | Fix: Reactor with async handler (before/after) | Written from scratch | Abbreviated |
| 11 | Fix: missing handler for subscription (before/after) | Written from scratch | Abbreviated |

---

### 2. `contents/BrighterBasicConfiguration.md` (UPDATE)

**Purpose**: Add a recommendation to enable pipeline validation as part of standard setup.
**Change size**: ~15-20 lines added
**Placement**: After the main AddBrighter configuration example, before any advanced topics

#### What to Add

A new subsection or callout block recommending `ValidatePipelines()` and `DescribePipelines()`:

```
### Validating Your Configuration                              (H3)

  [1-2 sentences: Brighter can validate your pipeline configuration at startup]
  [Code example showing .ValidatePipelines().DescribePipelines() added to existing config]
  [1 sentence: link to PipelineValidation.md for details]
```

#### Code Example Needed

| # | Example | Source | Complete or Abbreviated |
|---|---------|--------|------------------------|
| 1 | AddBrighter with ValidatePipelines + DescribePipelines in existing config pattern | Extend existing example in the file | Abbreviated (add to existing) |

---

### 3. `contents/HowConfiguringTheDispatcherWorks.md` (UPDATE)

**Purpose**: Add a note about consumer-specific validation checks when using AddConsumers.
**Change size**: ~10-15 lines added
**Placement**: Near the end of the consumer/subscription configuration section

#### What to Add

A brief note or tip:

```
### Validating Consumer Configuration                          (H3)

  [1-2 sentences: when using ValidatePipelines(), consumer-specific checks run automatically]
  [Brief list: pump/handler mismatch, missing handlers, RequestType checks]
  [Link to PipelineValidation.md for full details]
```

---

### 4. `SUMMARY.md` (UPDATE)

**Purpose**: Add the new page to the table of contents.
**Change**: One line added.

#### Before

```markdown
* [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)
* [Test Double Options for Command Processor](/contents/TestDoubleOptions.md)
```

#### After

```markdown
* [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)
* [Test Double Options for Command Processor](/contents/TestDoubleOptions.md)
* [Pipeline Validation and Diagnostics](/contents/PipelineValidation.md)
```

---

## Code Examples Plan

### Complete Examples (runnable as-is)

1. **Quick Start Configuration** — Shows a realistic `AddBrighter()` call with `.ValidatePipelines().DescribePipelines()` chained. Based on the pattern in `Brighter/samples/WebAPI/` but simplified to focus on validation. Includes `using` statements.

2. **Diagnostic Output (Information level)** — Log output showing the summary line:
   ```
   Brighter: 3 handler pipelines, 2 publications, 5 subscriptions configured
   ```

3. **Diagnostic Output (Debug level)** — Full annotated log output showing all three sections:
   ```
   === Handler Pipelines ===
     OrderCreatedHandler (async)
       Pipeline: [DeferMessageOnErrorAsync(0)] → [UseResiliencePipelineAsync(1, "OrderRetry")] → OrderCreatedHandler
     PaymentReceivedHandler (async)
       Pipeline: [RejectMessageOnErrorAsync(0)] → PaymentReceivedHandler

   === Publications (Outgoing) ===
     OrderCreated → order-created (topic)
       Mapper:     OrderCreatedMessageMapper (custom)
       Transforms: [CompressPayload(0)]
     PaymentReceived → payment-received (topic)
       Mapper:     JsonMessageMapper<PaymentReceived> (default)
       Transforms: (none)

   === Subscriptions (Incoming) ===
     OrderCreated (Proactor)
       Channel:  order-created-queue → order-created
   ```

4. **throwOnError false** — Configuration example:
   ```csharp
   .ValidatePipelines(throwOnError: false)
   ```

5. **Conditional enablement** — Two examples:
   ```csharp
   // Gate on environment
   .ValidatePipelines(enabled: builder.Environment.IsDevelopment())
   
   // Gate on configuration
   .ValidatePipelines(enabled: builder.Configuration.GetValue<bool>("Brighter:ValidatePipelines"))
   ```

### Abbreviated Examples (with `// ...`)

6-11. **Before/after fix pairs** for each common mistake — show only the relevant handler/attribute/subscription declaration, not full program setup. Use `// ...` for surrounding code.

### Example Sources

| Example | Source |
|---------|--------|
| Quick start | Based on `Brighter/samples/WebAPI/WebAPI_mTLS_TestHarness/TodoApi/Program.cs` pattern |
| Diagnostic output | Based on format in `PipelineDiagnosticWriter.cs` |
| Validation errors | Based on error messages in `HandlerPipelineValidationRules.cs`, `ProducerValidationRules.cs`, `ConsumerValidationRules.cs` |
| Fix examples | Written from scratch, based on test scenarios in `Brighter/tests/Paramore.Brighter.Core.Tests/Validation/` |

---

## Validation Rules Reference Table

This table will appear in the "What Gets Checked" section of `PipelineValidation.md`:

| Rule | Path | Severity | What It Checks | Example Error |
|------|------|----------|----------------|---------------|
| Handler type visibility | AddBrighter | Error | Handler class is `public` | `Handler type 'MyNamespace.PrivateHandler' is not public — Brighter only supports public handler types. Make the class public so the pipeline builder can find it` |
| Sync/async attribute consistency | AddBrighter | Error | Async handlers use async attributes, sync handlers use sync attributes | `Async handler uses sync attribute 'RejectMessageOnErrorAttribute' at step 0 — this will throw a ConfigurationException at pipeline build time` |
| Backstop attribute ordering | AddBrighter | Warning | Backstop attributes (Reject/Defer/DontAck) have lower step numbers than resilience attributes | `'RejectMessageOnError' at step 5 is after 'UseResiliencePipeline' at step 3 — in Brighter, lower step values are outer wrappers, so the backstop will never execute on failure` |
| Publication RequestType set | AddProducers | Error | `Publication.RequestType` is not null | `Publication.RequestType is null — Post()/Deposit() will throw ConfigurationException` |
| Publication RequestType implements IRequest | AddProducers | Error | `Publication.RequestType` implements `IRequest` | `Publication.RequestType 'MyClass' does not implement IRequest` |
| Pump/handler type match | AddConsumers | Error | Reactor subscriptions have sync handlers, Proactor subscriptions have async handlers | `Subscription uses Reactor (sync) pump but handler 'OrderHandler' is async — use Proactor for async handlers` |
| Handler registered | AddConsumers | Error | At least one handler exists for the subscription's `RequestType` | `No handler registered for 'OrderCreated' — messages will be received but cannot be dispatched` |
| RequestType subtype | AddConsumers | Warning | `RequestType` implements `ICommand` or `IEvent` (not just `IRequest`) | `RequestType 'MyMessage' implements neither ICommand nor IEvent — consider implementing one of these marker interfaces` |

---

## Style Notes

### Terminology

| Term | Usage | Notes |
|------|-------|-------|
| Pipeline validation | Primary term for the feature | Matches `ValidatePipelines()` method name |
| Diagnostic report | Term for DescribePipelines output | Matches user-facing log output |
| Backstop attribute | Reject/Defer/DontAck error handlers | Established by ADR 0053 |
| Resilience attribute | UseResiliencePipeline handler | Matches existing docs |
| Reactor / Proactor | Message pump types | Already defined in existing docs |

### Deviations from Standard Pattern

- The "Common Mistakes and Fixes" section uses before/after code pairs instead of prose — this is a deliberate deviation to make the troubleshooting content scannable and actionable
- The "What Gets Checked" section uses tables rather than prose for the rule reference — tables are more scannable for a reference section
- No separate "Best Practices" section — the recommendation to enable validation is the single best practice, and it's covered in the Quick Start and in the BrighterBasicConfiguration.md update

---

## Design Quality Checklist

- [x] Readable as a standalone document
- [x] Uses simple language and concrete examples
- [x] File and section structure obvious at a glance (tree diagram + section outline)
- [x] Enough detail that each file could be written independently
- [x] References requirements for traceability (FR-1 through FR-15 covered by rule reference table)
- [x] Code examples plan identifies source and completeness
- [x] SUMMARY.md changes shown as before/after
- [x] Cross-links identified for all files
