# Design: Error Handling Documentation

## Documentation Structure

```
Error Handling Documentation
│
├── Existing (cross-link, do not rewrite)
│   ├── PolicyRetryAndCircuitBreaker.md  — Polly resilience pipelines (retry, circuit breaker, timeout)
│   └── PolicyFallback.md               — Fallback handler pattern
│
├── Rewrite
│   └── HandlerFailure.md               — Comprehensive error handling guide
│
├── Create
│   └── ErrorHandlingOptions.md          — DLQ and subscription error configuration
│
└── Update
    ├── BasicConcepts.md                 — Add glossary entries
    └── SUMMARY.md                       — Rename entry, add new file
```

### Reading Order

A user new to Brighter error handling would read:

1. **HandlerFailure.md** — Understand the overall model and choose a strategy
2. **PolicyRetryAndCircuitBreaker.md** — Configure retry and circuit breaker (already exists)
3. **PolicyFallback.md** — Configure fallback handlers (already exists)
4. **ErrorHandlingOptions.md** — Configure DLQ, requeue limits, and subscription error properties

---

## File-by-File Outline

### 1. HandlerFailure.md (Rewrite)

**Purpose:** Explain all error handling strategies available in Brighter and help users choose the right one.

**Target length:** 350-400 lines

**Cross-links:**
- [PolicyRetryAndCircuitBreaker.md](/contents/PolicyRetryAndCircuitBreaker.md) — for retry/circuit breaker details
- [PolicyFallback.md](/contents/PolicyFallback.md) — for fallback handler details
- [ErrorHandlingOptions.md](/contents/ErrorHandlingOptions.md) — for subscription configuration
- [BuildingAPipeline.md](/contents/BuildingAPipeline.md) — for custom middleware
- [HowServiceActivatorWorks.md](/contents/HowServiceActivatorWorks.md) — for message pump internals

**Glossary terms to reference:** Dead Letter Queue, Nack, Poison Message (defined in BasicConcepts.md)

#### Section Outline

```
# Error Handling

## Introduction (5-10 lines)
  Brief: Brighter uses exception-based flow control in the message pump.
  The default is to always acknowledge the message — whether the handler
  succeeds or throws. Any other behavior requires explicit action.

## The Default: Always Acknowledge (30-40 lines)
  This is the most important section in the document. Emphasize clearly:

  **Brighter's default behavior is to acknowledge (ack) every message,
  regardless of whether the handler succeeds or fails.**

  - If the handler completes without throwing → message is ack'd (success).
  - If an unhandled exception leaves the pipeline → message is also ack'd.

  **Why?** To avoid poison messages. A poison message is one that can never
  be processed successfully. If the message pump re-delivered it on failure,
  the handler would fail again, re-deliver again, fail again — an infinite
  loop that blocks the pump from processing other messages.

  By ack'ing on failure, Brighter ensures the pump moves on. The assumption
  is that non-recovered errors are non-transient: something is wrong with
  the message or the handler logic, and retrying won't help. Operators
  investigate from logs and traces.

  **This means:** if you do nothing, errors are logged and the message is
  discarded. Every other error handling behavior described in this document
  requires you to opt in, using either:
  - **Action exceptions** — special exceptions you throw in your handler
    (DeferMessageAction, RejectMessageAction, DontAckAction, InvalidMessageAction)
  - **Middleware attributes** — attributes on your handler method that catch
    exceptions and convert them to action exceptions
    (RejectMessageOnErrorAttribute, DontAckOnErrorAttribute,
    FallbackPolicyAttribute, UseResiliencePipelineAttribute)

  The rest of this document explains each option and when to use it.

## How the Message Pump Handles Exceptions (20-30 lines)
  Explain the message pump's catch chain — the order matters:
  1. DeferMessageAction → requeue with delay
  2. DontAckAction → nack, leave on channel
  3. RejectMessageAction → reject, route to DLQ
  4. InvalidMessageAction → route to invalid channel (or DLQ fallback)
  5. Any other exception → ack (the default described above)

  These are exceptions used as flow control signals. Your handler (or
  middleware) throws them; the message pump catches them and acts accordingly.
  They are not bugs — they are how you communicate intent to the pump.

## Choosing an Error Handling Strategy (30-40 lines)
  Decision guide in prose (not a flowchart):

  - **Transient error, retry quickly:** Use UseResiliencePipelineAttribute with retry.
    Link → PolicyRetryAndCircuitBreaker.md
  - **Transient error, retry later:** Throw DeferMessageAction to requeue with delay.
  - **Non-transient error, preserve for investigation:** Throw RejectMessageAction to send to DLQ.
    Or use RejectMessageOnErrorAttribute as a backstop.
  - **Temporary block, try again after timeout:** Throw DontAckAction to leave on channel.
    Or use DontAckOnErrorAttribute as a backstop.
  - **Deserialization failure:** Throw InvalidMessageAction (typically in a message mapper).
  - **Compensating action before failing:** Use FallbackPolicyAttribute.
    Link → PolicyFallback.md
  - **Default (no action):** Let the exception propagate. Message is ack'd and discarded.

## Requeue with Delay (DeferMessageAction) (40-50 lines)
  ### What It Does
  Rejects the message and requeues it on the External Bus with a delay.
  Configured via Subscription.RequeueDelay and Subscription.RequeueCount.
  When RequeueCount is exceeded, the message is rejected (and sent to DLQ if configured).

  ### When to Use It
  Transient failures where retrying after a delay may succeed.
  Example: downstream service temporarily unavailable.

  ### Code Example: Throwing DeferMessageAction directly
  ### Code Example: Handler with retry then defer pattern

## Reject to Dead Letter Queue (RejectMessageAction) (40-50 lines)
  ### What It Does
  Ends processing and routes the message to a Dead Letter Queue.
  If no DLQ configured, message is ack'd and discarded (with logging).

  ### When to Use It
  Non-transient errors where the message should be preserved for investigation.
  Example: business validation failure, corrupt data requiring manual review.

  ### Code Example: Throwing RejectMessageAction directly
  ### Code Example: Using RejectMessageOnErrorAttribute as a backstop

## Don't Acknowledge (DontAckAction) (40-50 lines)
  ### What It Does
  Leaves the message unacknowledged on the channel.
  Transport re-delivers after its visibility timeout.
  A configurable delay (DontAckDelay, default 1s) prevents CPU spin.
  Increments unacceptable message count.

  ### When to Use It
  Temporary blocking scenarios (feature switches, maintenance windows).
  When you want the transport's native re-delivery, not Brighter's requeue.

  ### Transport Behavior Table
  | Transport | Nack Behavior |
  |-----------|---------------|
  | RabbitMQ | BasicNack with requeue: true |
  | AWS SQS | ChangeMessageVisibility to 0 (immediate re-visibility) |
  | Azure Service Bus | AbandonMessage (release lock) |
  | Kafka | No-op (don't commit offset) |
  | Redis | No-op (BLPOP is destructive) |
  | MQTT | No-op (no ack concept) |

  ### Code Example: Throwing DontAckAction directly
  ### Code Example: Using DontAckOnErrorAttribute as a backstop

## Invalid Message Handling (InvalidMessageAction) (20-30 lines)
  ### What It Does
  Routes undeserializable messages to an invalid message channel.
  Routing priority: invalid message channel → DLQ → ack and log.

  ### When to Use It
  Typically thrown from message mappers on deserialization failure.
  Separates "bad message" from "bad processing" in your monitoring.

  ### Code Example: Throwing InvalidMessageAction in a message mapper

## Backstop Attributes (40-50 lines)
  ### What They Do
  Wrap the handler pipeline in a try/catch and convert unhandled exceptions
  to the appropriate action exception.

  ### RejectMessageOnErrorAttribute
  Catches any exception → throws RejectMessageAction → message goes to DLQ.
  Sync: RejectMessageOnErrorAttribute. Async: RejectMessageOnErrorAsyncAttribute.

  ### DontAckOnErrorAttribute
  Catches any exception → throws DontAckAction → message stays on channel.
  Sync: DontAckOnErrorAttribute. Async: DontAckOnErrorAsyncAttribute.

  ### Pipeline Ordering
  Backstop attributes should be at the outermost position (lowest step number).
  Retry/circuit breaker should be inside (higher step number).

  ### Code Example: Typical pipeline with backstop + retry
  ### Code Example: Async pipeline with backstop + retry

## Unacceptable Message Limit (20-30 lines)
  Explain UnacceptableMessageLimit and UnacceptableMessageLimitWindow.
  What counts as unacceptable: RejectMessageAction, DontAckAction, InvalidMessageAction.
  When limit is reached, pump stops.
  Link → ErrorHandlingOptions.md for configuration details.

## Further Reading (5-10 lines)
  - [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)
  - [Fallback Handlers](/contents/PolicyFallback.md)
  - [Error Handling Options](/contents/ErrorHandlingOptions.md)
  - [Building a Pipeline](/contents/BuildingAPipeline.md)
  - [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md)
```

#### Code Examples Plan

| # | Description | Source | Complete or Abbreviated |
|---|-------------|--------|------------------------|
| 1 | Throw DeferMessageAction in a handler | Written from scratch, modeled on XML docs | Complete |
| 2 | Handler with UseResiliencePipeline + DeferMessageAction | Written from scratch | Abbreviated (omit pipeline registration) |
| 3 | Throw RejectMessageAction in a handler | Written from scratch, modeled on XML docs | Complete |
| 4 | RejectMessageOnErrorAttribute on a handler method | From RejectMessageOnErrorAttribute.cs XML docs | Complete |
| 5 | Throw DontAckAction in a handler | Written from scratch, modeled on XML docs | Complete |
| 6 | DontAckOnErrorAttribute on a handler method | From DontAckOnErrorAttribute.cs XML docs | Complete |
| 7 | Throw InvalidMessageAction in a message mapper | Written from scratch | Complete |
| 8 | Typical pipeline: RejectMessageOnError(0) + UseResiliencePipeline(1) | Composite of source XML docs | Complete |
| 9 | Async pipeline: RejectMessageOnErrorAsync(0) + UseResiliencePipelineAsync(1) | Composite of source XML docs | Complete |

---

### 2. ErrorHandlingOptions.md (Create)

**Purpose:** Document the subscription and message pump properties that control error handling behavior.

**Target length:** 200-250 lines

**Cross-links:**
- [HandlerFailure.md](/contents/HandlerFailure.md) — for the error handling strategies these options support
- Transport-specific configuration docs (RabbitMQConfiguration.md, KafkaConfiguration.md, AWSSQSConfiguration.md, etc.)

**Glossary terms to reference:** Dead Letter Queue, Poison Message

#### Section Outline

```
# Error Handling Options

## Introduction (5-10 lines)
  Brief: Configure how the message pump handles errors via Subscription properties.
  These options control requeue behavior, DLQ routing, and pump termination thresholds.

## Subscription Properties (60-80 lines)

  ### RequeueCount
  Type: int. Default: -1 (infinite).
  Number of times to requeue a message (via DeferMessageAction) before treating it as a poison message.
  When exceeded: message is rejected (sent to DLQ if configured, otherwise ack'd and discarded).
  Code example: setting RequeueCount in a subscription.

  ### RequeueDelay (RequeueDelayInMilliseconds)
  Type: TimeSpan. Default: TimeSpan.Zero.
  Delay before a requeued message becomes available again.
  How delay works depends on transport (native delay vs scheduler).
  Link → Scheduler docs if relevant.

  ### UnacceptableMessageLimit
  Type: int. Default: 0 (disabled).
  Number of unacceptable messages (reject, nack, invalid) before the pump stops.
  Protects against mass message loss during systemic failures.

  ### UnacceptableMessageLimitWindow
  Type: TimeSpan?. Default: null (count never resets).
  Time window for counting unacceptable messages. Count resets at end of window.
  Without a window, lifetime count accumulates and eventually stops the pump.

  ### DontAckDelay
  Type: TimeSpan. Default: 1 second.
  Delay after a DontAckAction before the pump processes the next message.
  Prevents tight-loop CPU burn when messages are repeatedly not acknowledged.

  Code example: complete subscription with all error properties set.

## Dead Letter Queue Configuration (60-80 lines)

  ### How DLQ Routing Works
  When a message is rejected (RejectMessageAction or requeue count exceeded):
  1. If transport has native DLQ support → transport handles routing
  2. If subscription implements IUseBrighterDeadLetterSupport → Brighter sends to DLQ
  3. If neither → message is ack'd and discarded (logged as warning)

  ### Native vs Brighter-Managed DLQ
  Table showing which transports support native DLQ and which use Brighter-managed:

  | Transport | DLQ Type | Invalid Message | Notes |
  |-----------|----------|-----------------|-------|
  | RabbitMQ | Native (DLX) | Brighter-managed | Uses dead letter exchange with routing key |
  | AWS SQS | Brighter-managed | Brighter-managed | Direct send to DLQ queue |
  | Azure Service Bus | Native | Brighter-managed | Built-in DLQ per subscription |
  | Kafka | Brighter-managed | Brighter-managed | Lazy producer to DLQ topic |
  | Redis | Brighter-managed | Brighter-managed | No native DLQ (BLPOP is destructive) |
  | MsSql | Brighter-managed | Brighter-managed | Same table, different topic value |
  | PostgreSQL | Brighter-managed | Brighter-managed | Visibility timeout model |
  | MQTT | Brighter-managed | Brighter-managed | Fire-and-forget, no ack concept |

  ### Configuring DLQ on a Subscription
  For Brighter-managed DLQ: set DeadLetterRoutingKey on the subscription.
  For invalid messages: set InvalidMessageRoutingKey on the subscription.

  Code example: Kafka subscription with DLQ and invalid message routing.
  Code example: SQS subscription with DLQ.

  ### DLQ Naming Conventions
  Default naming: {topic}.dlq for dead letters, {topic}.invalid for invalid messages.
  Customizable via DeadLetterNamingConvention and InvalidMessageNamingConvention.

  Code example: custom naming convention.

  ### Message Enrichment
  When a message is sent to DLQ, Brighter adds metadata to the message header bag:
  - OriginalTopic
  - RejectionReason (DeliveryError or Unacceptable)
  - RejectionTimestamp
  - OriginalMessageType
  - Transport-specific: partition, offset, consumer group

## Common Configurations (30-40 lines)

  ### Retry 3 Times Then DLQ
  Complete subscription example combining RequeueCount with DLQ.

  ### Stop Pump After 10 Errors in 5 Minutes
  Example using UnacceptableMessageLimit + UnacceptableMessageLimitWindow.

## Further Reading (5-10 lines)
  - [Error Handling](/contents/HandlerFailure.md)
  - [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)
  - Transport-specific configuration docs
```

#### Code Examples Plan

| # | Description | Source | Complete or Abbreviated |
|---|-------------|--------|------------------------|
| 1 | Subscription with RequeueCount and RequeueDelay | Written from scratch using Subscription.cs API | Complete |
| 2 | Subscription with UnacceptableMessageLimit and Window | Written from scratch | Complete |
| 3 | Kafka subscription with DeadLetterRoutingKey and InvalidMessageRoutingKey | From KafkaSubscription.cs constructor | Complete |
| 4 | SQS subscription with DeadLetterRoutingKey | From SqsSubscription.cs constructor | Complete |
| 5 | Custom DLQ naming convention | From DeadLetterNamingConvention.cs | Abbreviated |
| 6 | Complete subscription: retry 3 times then DLQ | Written from scratch | Complete |
| 7 | Subscription with UnacceptableMessageLimit + Window | Written from scratch | Complete |

---

### 3. BasicConcepts.md (Update)

**Purpose:** Add glossary entries for error handling terms.

**Terms to add** (in alphabetical position within the existing glossary):

- **Dead Letter Queue (DLQ)** — A queue where messages that cannot be processed are sent for later investigation. When Brighter rejects a message (via `RejectMessageAction` or exceeding the requeue count), and a DLQ is configured, the message is routed there instead of being discarded. Some transports (RabbitMQ, Azure Service Bus) provide native DLQ support; for others, Brighter manages the DLQ.

- **Nack (Negative Acknowledgment)** — Signals to the transport that a message was not successfully processed. The transport's behavior on nack varies: some re-deliver the message after a timeout, others make it immediately available. In Brighter, throwing `DontAckAction` triggers a nack.

- **Poison Message** — A message that repeatedly fails processing. Brighter uses `RequeueCount` to limit requeue attempts; when exceeded, the message is rejected to prevent infinite retry loops.

**Terms already defined (no changes needed):**
- Message Queue (line 59-63 in BasicConcepts.md already mentions nack, DLQ, poison pill)
- Circuit Breaker, Retry, Fallback, Timeout — defined elsewhere in docs

**Note:** BasicConcepts.md already mentions DLQ and nack in the Message Queue definition. The new entries expand on those brief mentions with Brighter-specific context.

---

### 4. SUMMARY.md (Update)

**Before:**
```markdown
## Using an External Bus
 ...
 * [Failure and Dead Letter Queues](/contents/HandlerFailure.md)
```

**After:**
```markdown
## Using an External Bus
 ...
 * [Error Handling](/contents/HandlerFailure.md)
 * [Error Handling Options](/contents/ErrorHandlingOptions.md)
```

Changes:
1. Rename "Failure and Dead Letter Queues" → "Error Handling"
2. Add "Error Handling Options" entry immediately after

No other SUMMARY.md changes. The retry/fallback entries stay in "Brighter Request Handlers and Middleware Pipelines" because they work on both Internal and External Bus.

---

## Style Notes

### Terminology

| Term | Usage | Notes |
|------|-------|-------|
| Action exception | Collective term for DeferMessageAction, RejectMessageAction, DontAckAction, InvalidMessageAction | Not an official Brighter term but useful in docs for clarity |
| Backstop attribute | RejectMessageOnErrorAttribute or DontAckOnErrorAttribute | From the source code XML docs |
| DLQ | Abbreviation for Dead Letter Queue | Define on first use, then use abbreviation |
| Nack | Negative acknowledgment | Already used in BasicConcepts.md |
| Poison message | Message that repeatedly fails | Already mentioned in BasicConcepts.md |
| Message pump | The Reactor or Proactor that consumes messages | Already used in existing docs |

### Deviations from Standard Pattern

- **HandlerFailure.md** does not follow the standard "Key Concepts → Usage → Configuration" pattern because it is a decision guide. Instead it follows: "How It Works → Choosing a Strategy → Strategy Details (one section each)". This is appropriate because the primary user question is "which strategy should I use?" not "how do I configure this one feature?"

- **ErrorHandlingOptions.md** follows a more standard reference pattern since it documents configuration properties.

### Sync vs Async

Each action exception and backstop attribute has sync and async variants. In the documentation:
- Show the sync variant in the main code examples
- Mention the async variant by name with a brief note (e.g., "For async handlers, use `RejectMessageOnErrorAsyncAttribute`")
- Do not duplicate every example for both sync and async — the pattern is identical
