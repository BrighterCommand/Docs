# Tasks: Pipeline Validation at Startup Documentation

**Spec**: 003-pipeline-validation-at-startup
**Design**: [design.md](design.md)
**Total tasks**: 10
**Phases**: 4

---

## Phase 1: Research & Preparation

**Goal**: Verify understanding of the feature by reading source code and running samples.

- [x] **Task 1.1:** Read validation source code and verify diagnostic output format
  - Input: `../Brighter/src/Paramore.Brighter/Validation/PipelineDiagnosticWriter.cs`, `HandlerPipelineValidationRules.cs`, `ProducerValidationRules.cs`, `../Brighter/src/Paramore.Brighter.ServiceActivator/Validation/ConsumerValidationRules.cs`
  - Output: Confirmed understanding of all 8 validation rules, error message templates, and diagnostic output format
  - Notes: Compare source with the rule reference table in design.md; note any discrepancies

- [x] **Task 1.2:** Read sample project using ValidatePipelines and DescribePipelines
  - Input: `../Brighter/samples/WebAPI/` projects that call `.ValidatePipelines().DescribePipelines()`
  - Output: Confirmed working configuration pattern to use as the Quick Start example
  - Notes: Identify the simplest sample that demonstrates the feature

---

## Phase 2: Core Documentation (P0)

**Goal**: Write the dedicated `PipelineValidation.md` page — the primary deliverable.

Dependencies: Phase 1 complete.

- [x] **Task 2.1:** Write PipelineValidation.md — introduction and Quick Start
  - Input: design.md Quick Start section, sample project from Task 1.2
  - Output: `contents/PipelineValidation.md` lines 1-50 (H1 title, intro, Quick Start with code example)
  - Notes: Keep intro to 2-3 sentences. Quick Start should be a complete, runnable configuration example showing `.ValidatePipelines().DescribePipelines()` in context.

- [x] **Task 2.2:** Write PipelineValidation.md — What Gets Checked (validation rules reference)
  - Input: design.md rule reference table, `HandlerPipelineValidationRules.cs`, `ProducerValidationRules.cs`, `ConsumerValidationRules.cs`
  - Output: `contents/PipelineValidation.md` "What Gets Checked" section with three subsections (Handler Pipeline Checks, Producer Checks, Consumer Checks), each with a rule table
  - Notes: Use tables with columns: Rule, Severity, What It Checks, Example Error. Group by configuration path (AddBrighter, AddProducers, AddConsumers). Explain briefly that checks scale to what's configured.

- [x] **Task 2.3:** Write PipelineValidation.md — Diagnostic Report section
  - Input: design.md diagnostic output examples, `PipelineDiagnosticWriter.cs`
  - Output: `contents/PipelineValidation.md` "Diagnostic Report" section with Information-level summary example and annotated Debug-level full detail example
  - Notes: Show realistic output with domain examples (OrderCreated, PaymentReceived). Annotate what each section means.

- [x] **Task 2.4:** Write PipelineValidation.md — Configuration section
  - Input: design.md Configuration section, `BrighterPipelineValidationExtensions.cs`
  - Output: `contents/PipelineValidation.md` "Configuration" section with subsections: Enabling, Controlling Error Behavior, Conditional Enablement, How Validation Scales
  - Notes: Show `throwOnError: false` example, environment-gated example, IConfiguration-gated example. Explain `enabled` parameter and independence of ValidatePipelines/DescribePipelines.

- [x] **Task 2.5:** Write PipelineValidation.md — Common Mistakes and Fixes, Further Reading
  - Input: design.md Common Mistakes section, test files in `../Brighter/tests/Paramore.Brighter.Core.Tests/Validation/`
  - Output: `contents/PipelineValidation.md` "Common Mistakes and Fixes" section (4 before/after pairs) and "Further Reading" section
  - Notes: Each mistake gets its own H3 with a brief explanation and before/after code. Keep code abbreviated — show only the relevant declaration. Further Reading links to BuildingAPipeline.md, Routing.md, HowConfiguringTheDispatcherWorks.md, ADR 0053.

---

## Phase 3: Supporting Documentation (P0/P1)

**Goal**: Update existing pages and SUMMARY.md.

Dependencies: Task 2.1 complete (so the target page exists to link to).

- [x] **Task 3.1:** Update BrighterBasicConfiguration.md — add validation recommendation
  - Input: design.md section for this file, existing `contents/BrighterBasicConfiguration.md`
  - Output: New "Validating Your Configuration" subsection (~15-20 lines) added after the main AddBrighter configuration example
  - Notes: Brief intro (1-2 sentences), code example extending the existing pattern, link to PipelineValidation.md. Do not restructure existing content.

- [x] **Task 3.2:** Update HowConfiguringTheDispatcherWorks.md — add consumer validation note
  - Input: design.md section for this file, existing `contents/HowConfiguringTheDispatcherWorks.md`
  - Output: New "Validating Consumer Configuration" subsection (~10-15 lines) near the end of the consumer/subscription configuration section
  - Notes: Brief note about consumer-specific checks (pump/handler mismatch, missing handlers). Link to PipelineValidation.md. Do not restructure existing content.

---

## Phase 4: Polish & Review

**Goal**: Update SUMMARY.md, verify all links, final quality check.

Dependencies: Phases 2 and 3 complete.

- [x] **Task 4.1:** Update SUMMARY.md and verify all cross-links
  - Input: design.md SUMMARY.md changes section
  - Output: `SUMMARY.md` updated with new entry under "Brighter Configuration"; all cross-links verified (PipelineValidation ↔ BrighterBasicConfiguration ↔ HowConfiguringTheDispatcherWorks)
  - Notes: Entry goes after "Test Double Options for Command Processor". Verify no broken links in all modified files.

---

## Dependencies

```
Phase 1 (Research)
  └── Phase 2 (Core: PipelineValidation.md)
  │     Tasks 2.1 → 2.2 → 2.3 → 2.4 → 2.5 (sequential — building one file)
  └── Phase 3 (Supporting: updates to existing pages)
  │     Tasks 3.1 and 3.2 can run in parallel
  │     Depends on Task 2.1 (target page must exist to link to)
  └── Phase 4 (Polish: SUMMARY.md, link verification)
        Depends on all Phase 2 and Phase 3 tasks
```

## Summary

| Phase | Tasks | Goal |
|-------|-------|------|
| 1. Research & Preparation | 2 | Verify understanding of feature |
| 2. Core Documentation | 5 | Write PipelineValidation.md |
| 3. Supporting Documentation | 2 | Update existing pages |
| 4. Polish & Review | 1 | SUMMARY.md and link verification |
| **Total** | **10** | |
