# Requirements: Pipeline Validation at Startup Documentation

**Spec**: 003-pipeline-validation-at-startup
**Created**: 2026-04-14

## Topic Overview

Brighter now supports opt-in pipeline validation and diagnostic reporting at startup. When enabled, Brighter checks the developer's pipeline configuration for common mistakes (sync/async mismatches, backstop attribute ordering, missing handlers, pump/handler mismatches) and can log a structured diagnostic report showing exactly how pipelines, publications, and subscriptions are wired.

This documentation needs to explain the feature, show how to enable it, describe what it checks, and help developers interpret validation errors and diagnostic output.

## Current State

### What Exists Today

- **Feature is fully implemented** in Brighter source code (`ValidatePipelines()` and `DescribePipelines()` extension methods on `IBrighterBuilder`)
- **Sample usage** exists in `Brighter/samples/WebAPI/` projects showing `.ValidatePipelines().DescribePipelines()` in the configuration chain
- **43+ test files** covering all validation rules and configuration scenarios
- **ADR 0053** documents the design rationale (`Brighter/docs/adr/0053-pipeline-validation-at-startup.md`)
- **Spec 0023** documents requirements and tasks (`Brighter/specs/0023-Pipeline-Validation-At-Startup/`)

### What's Missing

- **Zero documentation** in the Docs repository — no page covers this feature
- **No mention** in `BrighterBasicConfiguration.md` or any other existing configuration page
- **No error reference** — developers have no guide for interpreting validation errors or knowing what checks are performed
- **No diagnostic output examples** — developers don't know what to expect in their logs
- **No guidance** on when to enable validation, how to configure flags, or how validation interacts with the three configuration paths

## Target State

After this documentation is complete:

1. A dedicated page (`PipelineValidation.md`) explains the feature end-to-end
2. Existing configuration pages (`BrighterBasicConfiguration.md`) recommend enabling validation and diagnostics as a best practice
3. Developers can quickly add validation to their project and understand what it checks
4. Developers can interpret validation errors and diagnostic output without reading source code

## Target Audience

- **Beginners**: Need to know validation exists and should be enabled; need clear error messages to fix misconfiguration
- **Intermediate**: Need to understand all validation rules, how to configure flags (`throwOnError`, `enabled`), and how validation scales across configuration paths
- **Advanced**: Need to understand the architecture (hosted services, specification pattern), how consumer-owned validation works, and how to gate validation on environment

## Source Material

| Source | Location | Use |
|--------|----------|-----|
| ADR 0053 | `../Brighter/docs/adr/0053-pipeline-validation-at-startup.md` | Design rationale, architecture overview, key interfaces |
| Spec 0023 requirements | `../Brighter/specs/0023-Pipeline-Validation-At-Startup/requirements.md` | Functional requirements (FR-1 through FR-15), acceptance criteria |
| Extension methods | `../Brighter/src/Paramore.Brighter.Extensions.DependencyInjection/BrighterPipelineValidationExtensions.cs` | API signatures, registration details |
| Validation rules | `../Brighter/src/Paramore.Brighter/Validation/HandlerPipelineValidationRules.cs` | Handler validation checks |
| Producer rules | `../Brighter/src/Paramore.Brighter/Validation/ProducerValidationRules.cs` | Producer validation checks |
| Consumer rules | `../Brighter/src/Paramore.Brighter.ServiceActivator/Validation/ConsumerValidationRules.cs` | Consumer validation checks |
| Diagnostic writer | `../Brighter/src/Paramore.Brighter/Validation/PipelineDiagnosticWriter.cs` | Diagnostic output format |
| Sample projects | `../Brighter/samples/WebAPI/` | Working usage examples |
| Test suite | `../Brighter/tests/Paramore.Brighter.Core.Tests/Validation/` | All validation scenarios, expected error messages |

## Scope

### P0 — Must Have

1. **New page: `PipelineValidation.md`** — Dedicated documentation for the feature, placed under "Brighter Configuration" in SUMMARY.md
   - What pipeline validation is and why it matters
   - How to enable it (`ValidatePipelines()` and `DescribePipelines()`)
   - What it checks at each configuration path (AddBrighter, AddProducers, AddConsumers)
   - Configuration flags (`enabled`, `throwOnError`) with examples
   - Complete validation rule reference (all errors and warnings with remediation guidance)
   - Diagnostic report output examples (Information and Debug levels)
   - Code examples showing typical setup

2. **Update: `BrighterBasicConfiguration.md`** — Add a section or prominent callout recommending `ValidatePipelines()` and `DescribePipelines()` as part of standard setup, with a link to the dedicated page

### P1 — Should Have

3. **Update: `HowConfiguringTheDispatcherWorks.md`** — Add a note about validation for consumer configurations, linking to the dedicated page

4. **Diagnostic output walkthrough** — In the dedicated page, include annotated examples of diagnostic output showing what each section means

5. **Troubleshooting section** — Common validation errors with before/after code showing the fix

### P2 — Nice to Have

6. **Update: `Glossary.md`** — Add glossary entries for new terms if any are introduced (e.g., "pipeline validation", "diagnostic report")

7. **Update: error handling docs** — Cross-link from `HandlerFailure.md` or `ErrorHandlingOptions.md` to validation as a way to catch misconfiguration before runtime

## Out of Scope

- **Roslyn analyzer documentation** — The compile-time analyzers (BRT001–BRT005) are a separate feature and should be documented separately
- **Internals documentation** — The specification pattern, visitor pattern, and validation infrastructure internals are implementation details not needed by users
- **API reference** — Comprehensive API docs for `IAmAPipelineValidator`, `IAmAPipelineDiagnosticWriter`, etc. are not user-facing documentation
- **Broker connectivity** — Health checks and broker reachability are separate concerns
- **V9 migration** — This is a V10+ feature; no migration guidance needed

## Documentation Deliverables

### New Files

| File | Purpose | Priority |
|------|---------|----------|
| `contents/PipelineValidation.md` | Dedicated feature documentation | P0 |

### Updated Files

| File | Change | Priority |
|------|--------|----------|
| `contents/BrighterBasicConfiguration.md` | Add validation/diagnostics recommendation with link | P0 |
| `contents/HowConfiguringTheDispatcherWorks.md` | Add note about consumer validation with link | P1 |
| `SUMMARY.md` | Add `PipelineValidation.md` entry under "Brighter Configuration" | P0 |

## SUMMARY.md Changes

New entry placed under "Brighter Configuration", after `TestDoubleOptions.md` (the last current entry in that section):

```markdown
## Brighter Configuration

 * [Basic Configuration](/contents/BrighterBasicConfiguration.md)
 * [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md)
 * [How Configuring a Dispatcher for an External Bus Works](/contents/HowConfiguringTheDispatcherWorks.md)
 * [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)
 * [Test Double Options for Command Processor](/contents/TestDoubleOptions.md)
 * [Pipeline Validation and Diagnostics](/contents/PipelineValidation.md)       <-- NEW
```

## Constraints

### Terminology

- Use **"pipeline validation"** (not "startup validation" or "configuration validation") — consistent with the method name `ValidatePipelines()`
- Use **"diagnostic report"** (not "pipeline description" or "diagnostics") — consistent with the method name `DescribePipelines()` and the user-facing output
- Use **"Dispatcher"** not "ServiceActivator" — per V10 conventions noted in CLAUDE.md
- Use **"Reactor"** and **"Proactor"** for message pump types — per existing docs

### Style

- Follow the standard documentation file organization pattern (Title, Introduction, Key Concepts, How-to, Configuration, Best Practices, Common Pitfalls, Further Reading)
- Code examples must use C# with `csharp` syntax highlighting
- Show complete, runnable configuration examples (not fragments)
- Include `using` statements where needed for clarity
- Reference working sample projects in `Brighter/samples/WebAPI/`

### Cross-Linking

- Link from `PipelineValidation.md` to `BrighterBasicConfiguration.md`, `BuildingAPipeline.md`, `Routing.md`
- Link from `BrighterBasicConfiguration.md` to `PipelineValidation.md`
- Link from `HowConfiguringTheDispatcherWorks.md` to `PipelineValidation.md`
