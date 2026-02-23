# Requirements: Error Handling Documentation

## Topic Overview

Brighter uses an exception-based flow control model for error handling in the message pump. When a message is consumed from an External Bus and dispatched to a handler pipeline, the default behavior is:

- **Success (no exception):** The message is acknowledged (ack'd) and removed from the queue/stream.
- **Unhandled exception:** The message is also acknowledged, on the assumption that non-recovered errors are non-transient and an operator will investigate from logs.

However, Brighter provides several mechanisms to change this default:

1. **Retry with Polly** — Use `UseResiliencePipelineAttribute` (or the deprecated `UsePolicyAttribute`) to wrap handlers with retry, circuit breaker, timeout, and other Polly strategies. If all retries fail, the exception propagates and the default ack behavior applies.

2. **Requeue with delay** — Throw `DeferMessageAction` to reject and requeue the message on the External Bus with a configurable delay. Useful for transient failures that benefit from a longer wait.

3. **Reject to Dead Letter Queue** — Throw `RejectMessageAction` to reject the message and route it to a Dead Letter Queue (DLQ). Useful for non-transient errors where the message should be preserved for investigation.

4. **Nack (don't acknowledge)** — Throw `DontAckAction` to leave the message unacknowledged on the channel, so the transport re-delivers it after its visibility timeout. Useful for feature switches or temporary blocking scenarios.

5. **Invalid message routing** — Throw `InvalidMessageAction` to route messages that failed deserialization to an invalid message channel (or DLQ as fallback).

6. **Fallback** — Use `FallbackPolicyAttribute` to run compensating logic before the exception propagates.

7. **Backstop rejection** — Use `RejectMessageOnErrorAttribute` to convert any unhandled exception into a `RejectMessageAction`, sending the message to DLQ instead of the default ack-and-discard.

8. **Backstop nack** — Use `DontAckOnErrorAttribute` to convert any unhandled exception into a `DontAckAction`.

Much of this functionality (items 3-5, 7-8, and the DLQ infrastructure) is new in V10 and currently undocumented or minimally documented.

## Current State

**Existing documentation:**

| File | Lines | Coverage |
|------|-------|----------|
| `HandlerFailure.md` | 49 | Brief overview of DeferMessageAction, DLQ config, fallback links. No coverage of RejectMessageAction, DontAckAction, InvalidMessageAction, or the backstop attributes. |
| `PolicyRetryAndCircuitBreaker.md` | 686 | Comprehensive coverage of Polly resilience pipelines via `UseResiliencePipelineAttribute`. Well-written. |
| `PolicyFallback.md` | 334 | Good coverage of `FallbackPolicyAttribute` and the Fallback method override. |
| `HowServiceActivatorWorks.md` | 485 | Contains an "Error Handling" section (lines 247-283) covering DeferMessageAction and DLQ config. |

**Placement in SUMMARY.md:**

- `PolicyRetryAndCircuitBreaker.md` and `PolicyFallback.md` are under "Brighter Request Handlers and Middleware Pipelines"
- `HandlerFailure.md` is under "Using an External Bus"
- There is no unified error handling section

**Key gaps:**

- `RejectMessageAction` — not documented at all
- `DontAckAction` — not documented at all
- `InvalidMessageAction` — not documented at all
- `RejectMessageOnErrorAttribute` / `RejectMessageOnErrorAsyncAttribute` — not documented
- `DontAckOnErrorAttribute` / `DontAckOnErrorAsyncAttribute` — not documented
- DLQ configuration per transport — scattered and incomplete
- No explanation of the overall error handling model (the decision tree)
- No glossary entries for: DeferMessageAction, RejectMessageAction, DontAckAction, InvalidMessageAction, Dead Letter Queue, Nack, Poison Message
- `HandlerFailure.md` is outdated — it says unhandled exceptions result in "nack (or reject)" but doesn't explain the new explicit rejection/nack model

## Target State

A user reading the error handling documentation should be able to:

1. Understand the default error handling behavior (ack on success, ack on unhandled exception)
2. Choose the right error handling strategy for their use case
3. Configure retry and circuit breaker policies using Polly
4. Configure requeue behavior with `DeferMessageAction`
5. Configure DLQ rejection with `RejectMessageAction` and the backstop attribute
6. Configure nack behavior with `DontAckAction` and the backstop attribute
7. Understand which transports support native DLQ vs Brighter-managed DLQ
8. Configure DLQ and invalid message channels in their subscriptions

## Target Audience

- **Beginners:** Need to understand the default behavior and when to add retry policies
- **Intermediate:** Need to configure DLQ, choose between requeue/reject/nack, set up backstop handlers
- **Advanced:** Need to understand transport-specific DLQ behavior, custom middleware for error handling, the InvalidMessageAction flow

## Source Material

### ADRs (in `../Brighter/docs/adr/`)

**Rejection and DLQ:**
- `0037-reject-message-on-error-handler.md` — Defines RejectMessageOnErrorAttribute/Handler pattern
- `0038-aws-sqs-dlq-direct-send.md` — AWS SQS DLQ: direct send instead of ChangeMessageVisibility(0)
- `0039-redis-dlq-brighter-managed.md` — Redis DLQ: Brighter-managed (no native DLQ)
- `0040-mssql-dlq-brighter-managed.md` — MsSql DLQ: Brighter-managed, same table different topic
- `0041-postgres-dlq-brighter-managed.md` — PostgreSQL DLQ: Brighter-managed with visibility timeout
- `0042-rocketmq-dlq-brighter-managed.md` — RocketMQ DLQ: Brighter-managed with async producer
- `0043-mqtt-dlq-brighter-managed.md` — MQTT DLQ: Brighter-managed, fire-and-forget
- `0045-provide-dlq-where-missing.md` — Architecture for Brighter-managed DLQ across transports
- `0046-kafka-dlq-producer-for-requeue.md` — Kafka DLQ: lazy producer, naming conventions
- `0047-message-rejection-routing-strategy.md` — Routing: DeliveryError → DLQ, Unacceptable → Invalid channel → DLQ fallback

**Requeue / Scheduler:**
- `0037-universal-scheduler-delay.md` — Pluggable IAmAMessageScheduler for delayed requeue
- `0039-transport-scheduler-wiring.md` — Factory wiring for scheduler injection into consumers

**DontAck:**
- Spec `0020-DontAckAction` — DontAckAction exception, DontAckOnErrorAttribute, transport nack behaviors

### Source Code (in `../Brighter/src/Paramore.Brighter/`)

- `Actions/DeferMessageAction.cs`, `RejectMessageAction.cs`, `DontAckAction.cs`, `InvalidMessageAction.cs`
- `Reject/Attributes/RejectMessageOnErrorAttribute.cs`, `RejectMessageOnErrorAsyncAttribute.cs`
- `Reject/Handlers/RejectMessageOnErrorHandler.cs`, `RejectMessageOnErrorHandlerAsync.cs`
- `DontAck/Attributes/DontAckOnErrorAttribute.cs`, `DontAckOnErrorAsyncAttribute.cs` (if exists)
- `Policies/Attributes/UseResiliencePipelineAttribute.cs`, `FallbackPolicyAttribute.cs`
- `ServiceActivator/Reactor.cs`, `Proactor.cs` — message pump exception handling
- `ServiceActivator/MessagePump.cs` — DontAckDelay, RequeueCount, UnacceptableMessageLimit properties

### Existing Documentation
- `contents/HandlerFailure.md` — to be rewritten
- `contents/PolicyRetryAndCircuitBreaker.md` — cross-link, do not duplicate
- `contents/PolicyFallback.md` — cross-link, do not duplicate
- `contents/HowServiceActivatorWorks.md` — cross-link to its error handling section

## Scope

### P0 — Must Have

1. **Rewrite `HandlerFailure.md`** as a comprehensive error handling guide covering:
   - The default behavior (ack on success, ack on unhandled exception) and why
   - Decision tree: which strategy to use when
   - `DeferMessageAction` — requeue with delay (expand existing content)
   - `RejectMessageAction` — reject to DLQ (new)
   - `DontAckAction` — nack without acknowledgment (new)
   - `InvalidMessageAction` — invalid message routing (new)
   - `RejectMessageOnErrorAttribute` — backstop rejection handler (new)
   - `DontAckOnErrorAttribute` — backstop nack handler (new)
   - Pipeline ordering guidance (e.g., RejectMessageOnError at step 0, retry at step 1)
   - `UnacceptableMessageLimit` and `UnacceptableMessageLimitWindow` configuration
   - Cross-links to PolicyRetryAndCircuitBreaker.md and PolicyFallback.md

2. **Create `ErrorHandlingOptions.md`** covering DLQ and error channel configuration:
   - Subscription properties for error handling: `RequeueCount`, `RequeueDelayInMilliseconds`, `UnacceptableMessageLimit`, `UnacceptableMessageLimitWindow`, `DontAckDelay`
   - DLQ configuration per subscription (dead letter routing key, invalid message routing key)
   - Which transports support native DLQ vs Brighter-managed DLQ (table)
   - DLQ naming conventions (e.g., `{topic}.dlq`, `{topic}.invalid`)
   - The `IUseBrighterDeadLetterSupport` and `IUseBrighterInvalidMessageSupport` interfaces
   - Example subscription configurations for common transports

### P1 — Should Have

3. **Update `BasicConcepts.md` glossary** with new terms:
   - Dead Letter Queue (DLQ)
   - Nack / Negative Acknowledgment
   - Poison Message
   - Message Acknowledgment
   - DeferMessageAction, RejectMessageAction, DontAckAction, InvalidMessageAction

4. **Update SUMMARY.md** to reorganize error handling documentation into a coherent group

### P2 — Nice to Have

5. **Update transport-specific configuration docs** (RabbitMQConfiguration.md, KafkaConfiguration.md, etc.) to cross-link to the new error handling docs and note their DLQ behavior

6. **Add an error handling patterns section** to `HandlerFailure.md` (or as a separate file) with common recipes:
   - Retry with exponential backoff then reject to DLQ
   - Feature switch with nack
   - Graceful degradation with fallback

## Out of Scope

- Rewriting `PolicyRetryAndCircuitBreaker.md` — it is already comprehensive
- Rewriting `PolicyFallback.md` — it is already comprehensive
- Documenting the internal implementation of DLQ producers per transport (too low-level for user docs)
- Darker/query-side error handling (separate spec if needed)
- V9-to-V10 migration of error handling (covered in V10MigrationGuide.md)

## Documentation Deliverables

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `contents/HandlerFailure.md` | Rewrite | Comprehensive error handling guide: default behavior, decision tree, all four action exceptions, backstop attributes, pipeline ordering, UnacceptableMessageLimit |
| 2 | `contents/ErrorHandlingOptions.md` | Create | DLQ/error channel configuration: subscription properties, per-transport DLQ support table, naming conventions, example configurations |
| 3 | `contents/BasicConcepts.md` | Update | Add glossary entries for DLQ, Nack, Poison Message, and the four action exceptions |
| 4 | `SUMMARY.md` | Update | Reorganize error handling entries |

## SUMMARY.md Changes

**Current** (error handling entries scattered):
```
## Brighter Request Handlers and Middleware Pipelines
  ...
  * [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)
  * [Failure and Fallback](/contents/PolicyFallback.md)
  ...

## Using an External Bus
  ...
  * [Failure and Dead Letter Queues](/contents/HandlerFailure.md)
```

**Proposed** (grouped together):
```
## Brighter Request Handlers and Middleware Pipelines
  ...
  * [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)
  * [Failure and Fallback](/contents/PolicyFallback.md)
  ...

## Using an External Bus
  ...
  * [Error Handling](/contents/HandlerFailure.md)
  * [Error Handling Options](/contents/ErrorHandlingOptions.md)
```

The two retry/fallback files stay in "Request Handlers and Middleware Pipelines" because they apply to both Internal and External Bus. The error handling guide and options file stay in "Using an External Bus" because DeferMessageAction, RejectMessageAction, DontAckAction, and DLQ configuration only apply when consuming from an External Bus.

Rename the SUMMARY.md entry from "Failure and Dead Letter Queues" to "Error Handling" for clarity.

## Constraints

- Follow CLAUDE.md documentation standards (voice, structure, code examples)
- Use terminology consistent with BasicConcepts.md and Glossary
- All code examples must use V10 patterns (UseResiliencePipelineAttribute, not UsePolicyAttribute)
- Code examples should use `csharp` syntax highlighting
- Cross-link to PolicyRetryAndCircuitBreaker.md and PolicyFallback.md — do not duplicate their content
- Cross-link to transport-specific configuration docs for DLQ details
- Use relative paths for all internal links
- Define new terms on first use, link to glossary
