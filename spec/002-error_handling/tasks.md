# Writing Tasks: Error Handling Documentation

## Overview

**Total tasks:** 16
**Phases:** 4

| Phase | Goal | Tasks |
|-------|------|-------|
| 1. Research & Preparation | Verify understanding of APIs and existing docs | 2 |
| 2. Core Documentation (P0) | Write HandlerFailure.md and ErrorHandlingOptions.md | 9 |
| 3. Supporting Documentation (P1) | Update BasicConcepts.md and SUMMARY.md | 2 |
| 4. Polish & Review | Verify code examples, check links, final edit | 3 |

---

## Phase 1: Research & Preparation

**Goal:** Read source code and existing docs to confirm the design is accurate before writing.

- [x] **Task 1.1:** Verify action exception APIs and message pump catch chain
  - Input: `Brighter/src/Paramore.Brighter/Actions/` (all 4 action classes), `Brighter/src/Paramore.Brighter.ServiceActivator/Reactor.cs` (exception handling section)
  - Output: Notes confirming: constructor signatures, catch order in Reactor, what increments UnacceptableMessageCount
  - Notes: Check that the catch chain order in design.md (Defer → DontAck → Reject → Invalid → other) matches the actual code. If not, update design.md before writing.

- [x] **Task 1.2:** Verify backstop attribute and handler APIs
  - Input: `Brighter/src/Paramore.Brighter/Reject/` (attributes + handlers), `Brighter/src/Paramore.Brighter/DontAck/` (attributes + handlers)
  - Output: Notes confirming: attribute constructor params, handler behavior, async variant names
  - Notes: Confirm both sync and async variants exist for both RejectMessageOnError and DontAckOnError.

---

## Phase 2: Core Documentation (P0)

**Goal:** Write the two main documentation files.

**Dependencies:** Phase 1 must be complete before starting Phase 2.

### HandlerFailure.md (Tasks 2.1–2.6)

Write sections in order. Each task produces one or two sections of HandlerFailure.md.

- [x] **Task 2.1:** Write Introduction, The Default: Always Acknowledge, and How the Message Pump Handles Exceptions
  - Input: design.md sections for these headings, `Reactor.cs` catch chain (from Task 1.1 notes)
  - Output: `contents/HandlerFailure.md` — H1 title, Introduction (~5 lines), The Default: Always Acknowledge (~30 lines), How the Message Pump Handles Exceptions (~20 lines)
  - Notes: This replaces the existing file entirely. Start fresh. The "Default: Always Acknowledge" section is the most important — emphasize that ack-on-failure is deliberate poison message prevention, and that all other behaviors require opting in via action exceptions or middleware.

- [x] **Task 2.2:** Write Choosing an Error Handling Strategy
  - Input: design.md decision guide, cross-link targets (PolicyRetryAndCircuitBreaker.md, PolicyFallback.md)
  - Output: `contents/HandlerFailure.md` — Choosing an Error Handling Strategy section (~30 lines)
  - Notes: Prose decision guide, not a flowchart. Each bullet: scenario → recommended approach → link. End with the default (do nothing → ack and discard).

- [x] **Task 2.3:** Write Requeue with Delay (DeferMessageAction)
  - Input: design.md section, `DeferMessageAction.cs` XML docs, existing HandlerFailure.md content on DeferMessageAction (for reference only)
  - Output: `contents/HandlerFailure.md` — Requeue with Delay section (~40 lines) with 2 code examples
  - Notes: Code example 1: throw DeferMessageAction directly in a handler. Code example 2: handler with UseResiliencePipeline + DeferMessageAction pattern (abbreviated — omit pipeline registration, link to PolicyRetryAndCircuitBreaker.md).

- [x] **Task 2.4:** Write Reject to Dead Letter Queue (RejectMessageAction)
  - Input: design.md section, `RejectMessageAction.cs` XML docs, `RejectMessageOnErrorAttribute.cs` XML docs
  - Output: `contents/HandlerFailure.md` — Reject to DLQ section (~40 lines) with 2 code examples
  - Notes: Code example 1: throw RejectMessageAction directly. Code example 2: use RejectMessageOnErrorAttribute as a backstop. Mention that if no DLQ is configured, message is ack'd and discarded.

- [x] **Task 2.5:** Write Don't Acknowledge (DontAckAction) and Invalid Message Handling (InvalidMessageAction)
  - Input: design.md sections, `DontAckAction.cs` XML docs, `DontAckOnErrorAttribute.cs` XML docs, `InvalidMessageAction.cs` XML docs, spec `0020-DontAckAction` for transport nack behaviors
  - Output: `contents/HandlerFailure.md` — Don't Acknowledge section (~40 lines, including transport behavior table) + Invalid Message Handling section (~20 lines) with 3 code examples
  - Notes: Include the transport nack behavior table from design.md. For InvalidMessageAction, show example in a message mapper (not a handler). Mention DontAckDelay (default 1s).

- [x] **Task 2.6:** Write Backstop Attributes, Unacceptable Message Limit, and Further Reading
  - Input: design.md sections, `RejectMessageOnErrorAttribute.cs` and `DontAckOnErrorAttribute.cs` XML docs (for pipeline ordering examples)
  - Output: `contents/HandlerFailure.md` — Backstop Attributes section (~40 lines) + Unacceptable Message Limit section (~20 lines) + Further Reading (~5 lines) with 2 code examples
  - Notes: Backstop code examples should show a complete pipeline: backstop at step 0, retry at step 1, handler. Show both sync and async variants. Unacceptable Message Limit links to ErrorHandlingOptions.md for configuration details.

### ErrorHandlingOptions.md (Tasks 2.7–2.9)

Write sections in order. Can be started in parallel with HandlerFailure.md tasks 2.3–2.6.

- [x] **Task 2.7:** Write Introduction and Subscription Properties
  - Input: design.md section, `Subscription.cs` properties (RequeueCount, RequeueDelay, UnacceptableMessageLimit, UnacceptableMessageLimitWindow), `MessagePump.cs` (DontAckDelay)
  - Output: `contents/ErrorHandlingOptions.md` — H1 title, Introduction (~5 lines), Subscription Properties section (~60 lines) with 1 code example
  - Notes: This is a new file. For each property: type, default, what it does, when to use it. Code example: complete subscription with all error properties set.

- [x] **Task 2.8:** Write Dead Letter Queue Configuration
  - Input: design.md section, `IUseBrighterDeadLetterSupport.cs`, `IUseBrighterInvalidMessageSupport.cs`, `DeadLetterNamingConvention.cs`, `InvalidMessageNamingConvention.cs`, `KafkaSubscription.cs`, `SqsSubscription.cs`
  - Output: `contents/ErrorHandlingOptions.md` — Dead Letter Queue Configuration section (~60 lines, including native vs Brighter-managed table) with 3 code examples
  - Notes: Include the transport DLQ table from design.md. Code examples: Kafka subscription with DLQ + invalid, SQS subscription with DLQ, custom naming convention. Mention message enrichment metadata (OriginalTopic, RejectionReason, RejectionTimestamp, OriginalMessageType).

- [x] **Task 2.9:** Write Common Configurations and Further Reading
  - Input: design.md section
  - Output: `contents/ErrorHandlingOptions.md` — Common Configurations section (~30 lines) with 2 code examples + Further Reading (~5 lines)
  - Notes: Two recipes: "Retry 3 Times Then DLQ" (RequeueCount + DLQ) and "Stop Pump After 10 Errors in 5 Minutes" (UnacceptableMessageLimit + Window). These should be complete, copy-paste-ready subscription configurations.

---

## Phase 3: Supporting Documentation (P1)

**Goal:** Update glossary and table of contents.

**Dependencies:** Phase 2 must be complete (so we know the final file names and section anchors).

- [x] **Task 3.1:** Update BasicConcepts.md with glossary entries
  - Input: design.md glossary terms section, existing `contents/BasicConcepts.md`
  - Output: `contents/BasicConcepts.md` — 3 new glossary entries added in alphabetical position
  - Notes: Add: Dead Letter Queue (DLQ), Nack (Negative Acknowledgment), Poison Message. Keep entries concise (2-3 sentences each). BasicConcepts.md already mentions DLQ and nack briefly in the Message Queue definition — the new entries expand on those with Brighter-specific context.

- [x] **Task 3.2:** Update SUMMARY.md
  - Input: design.md SUMMARY.md section
  - Output: `SUMMARY.md` — rename "Failure and Dead Letter Queues" to "Error Handling", add "Error Handling Options" entry
  - Notes: The two changes are in the "Using an External Bus" section. No other changes needed. Verify the links resolve correctly.

---

## Phase 4: Polish & Review

**Goal:** Verify quality across all changes.

**Dependencies:** Phases 2 and 3 must be complete.

- [x] **Task 4.1:** Verify all code examples compile
  - Input: All code examples in `contents/HandlerFailure.md` and `contents/ErrorHandlingOptions.md`
  - Output: Confirmation that examples use correct class names, constructors, and namespaces from the actual source code
  - Notes: Check each example against the source: correct attribute names (RejectMessageOnErrorAttribute vs RejectMessageOnErrorAsyncAttribute), correct constructor params, correct using statements. Do not run a compiler — verify by reading the source.

- [x] **Task 4.2:** Verify all cross-links and anchors
  - Input: All links in HandlerFailure.md, ErrorHandlingOptions.md, and SUMMARY.md
  - Output: Confirmation that all internal links point to existing files and anchors
  - Notes: Check: relative paths are correct, linked files exist, section anchors match actual headings. Check that HandlerFailure.md links to PolicyRetryAndCircuitBreaker.md, PolicyFallback.md, ErrorHandlingOptions.md, BuildingAPipeline.md, HowServiceActivatorWorks.md.

- [x] **Task 4.3:** Final read-through for clarity and consistency
  - Input: Complete `contents/HandlerFailure.md` and `contents/ErrorHandlingOptions.md`
  - Output: Any final edits for tone, terminology consistency, and CLAUDE.md compliance
  - Notes: Check: second person voice ("you"), active voice, present tense, no orphaned files, no duplicate content, terms defined on first use, consistent use of "action exception" and "backstop attribute" terminology.
