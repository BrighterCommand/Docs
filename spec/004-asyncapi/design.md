# Design: AsyncAPI Documentation

## Documentation Structure

```
contents/
├���─ AsyncAPISupport.md          ← NEW (main documentation page)
├── Requests, Commands and Events.md  ← UPDATE (add cross-link)
SUMMARY.md                      ← UPDATE (add entry)
```

**Reading order:** Users arrive at `AsyncAPISupport.md` either from the SUMMARY.md navigation (under "Using an External Bus") or from the cross-link in "Requests, Commands and Events.md". The page is self-contained — it assumes familiarity with Brighter's external bus but not with AsyncAPI itself.

---

## SUMMARY.md Changes

**Placement:** After "Dynamic Message Deserialization" and before "Error Handling" in the "Using an External Bus" section.

**Before:**
```markdown
 * [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) 
 * [Error Handling](/contents/HandlerFailure.md)
```

**After:**
```markdown
 * [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) 
 * [AsyncAPI Document Generation](/contents/AsyncAPISupport.md)
 * [Error Handling](/contents/HandlerFailure.md)
```

---

## File: `contents/AsyncAPISupport.md`

**Purpose:** Document Brighter's AsyncAPI 3.0 document generation feature — from concept through setup, configuration, and CI/CD integration.

**Target length:** ~350–400 lines

**Cross-links to include:**
- `/contents/BasicConcepts.md` — for IRequest, Event, Command definitions
- `/contents/Routing.md` — for RoutingKey concepts
- `/contents/ImplementingExternalBus.md` — for external bus setup context
- `/contents/RabbitMQConfiguration.md` — referenced from RMQ example
- `/contents/KafkaConfiguration.md` — referenced from Kafka example
- `/contents/DefaultMessageMappers.md` — message mapper context

**Glossary terms to reference:** IRequest, Event, Command, RoutingKey, Subscription, Publication

---

### Section Outline

#### H1: AsyncAPI Document Generation

Brief introduction (3-4 sentences): what AsyncAPI is, what this package does, and why it matters.

#### H2: What is AsyncAPI?

- 2-3 paragraphs explaining AsyncAPI as "OpenAPI for async messaging"
- What problems it solves: stale documentation, no machine-readable contracts, difficulty onboarding new developers
- Link to the official AsyncAPI specification
- Note that Brighter generates AsyncAPI 3.0 documents

#### H2: Getting Started

##### H3: Prerequisites

- Bullet list: existing Brighter service with external bus configured, .NET 8.0+
- NuGet packages required: `Paramore.Brighter.AsyncAPI` and `Paramore.Brighter.AsyncAPI.NJsonSchema`

##### H3: Adding AsyncAPI Generation

- Code example: minimal `UseAsyncApi()` registration (Code Example 1)
- Code example: calling `GenerateAsyncApiDocumentAsync()` (Code Example 2)
- Explain that both JSON and YAML files are generated automatically
- Show expected output file names

#### H2: How Brighter Discovers Your Messaging Contracts

Brief intro paragraph explaining the three discovery mechanisms and their priority order.

##### H3: Subscriptions (Receive Operations)

- Subscriptions registered via `AddConsumers()` become `receive` operations
- Each subscription's `RoutingKey` becomes a channel address
- The `RequestType` on the subscription provides the message schema
- Note: requires `AddConsumers()` registration; send-only apps produce no receive operations

##### H3: Publications (Send Operations)

- Publications registered in the producer registry become `send` operations
- Each publication's `Topic` becomes a channel address
- The `RequestType` (from typed publications like `RmqPublication<T>`) provides the message schema

##### H3: Assembly Scanning (Send Operations)

- `IRequest` types decorated with `[PublicationTopic]` are discovered automatically
- Code example: decorating an event with `[PublicationTopic]` (Code Example 3)
- DI-registered sources take priority — assembly scanning does not create duplicates
- Can be disabled with `DisableAssemblyScanning = true`

##### H3: Deduplication

- Brief explanation: same routing key from multiple sources produces one channel
- Multiple operations on the same channel (e.g. one send + one receive) are supported
- Same `IRequest` type across sources produces one message component

#### H2: Configuration

##### H3: AsyncApiOptions

- Table of all configuration properties with descriptions and defaults:
  - `Title` (default: "Brighter Application")
  - `Version` (default: "1.0.0")
  - `Description` (optional)
  - `Servers` (optional dictionary of server definitions)
  - `AssembliesToScan` (optional, defaults to entry assembly)
  - `DisableAssemblyScanning` (default: false)
  - `SupplementalPublications` (optional, for publications without `[PublicationTopic]`)
- Code example: full configuration with servers (Code Example 4)

##### H3: Output Formats

- JSON is the canonical format, written to the specified path
- YAML is generated alongside automatically (same name, `.yaml` extension)
- Both files describe the same document — YAML is derived from the JSON output

#### H2: Custom Schema Generation

- Explain `IAmASchemaGenerator` interface
- Default uses NJsonSchema — supports `System.ComponentModel.DataAnnotations` and `JsonProperty` attributes
- Code example: registering a custom schema generator (Code Example 5)
- Note: register before calling `UseAsyncApi()` to override the default

#### H2: CI/CD Integration

- Pattern: add a `--generate-asyncapi` argument check to Program.cs
- Code example: the argument-check pattern (Code Example 6)
- Mention `asyncapi validate` CLI tool for validation in pipelines
- Note: generation happens after `Build()` — the host must be built but doesn't need to run

#### H2: Complete Examples

##### H3: RabbitMQ Example

- Full Program.cs showing a realistic setup with both subscriptions and publications
- Code example: complete working example based on the RMQ sample (Code Example 7)
- Show a sample of the generated JSON output (abbreviated) (Code Example 8)

##### H3: Kafka Example

- Full Program.cs showing Kafka-specific patterns
- Code example: complete working example based on the Kafka sample (Code Example 9)
- Highlight Kafka-specific considerations: deferred producer registry creation (Kafka opens a broker connection at construction time), using assembly scanning as a workaround for document generation without a live broker

#### H2: Further Reading

- Link to AsyncAPI specification: https://www.asyncapi.com/
- Link to AsyncAPI Studio for visualizing documents
- Link to working samples: `Brighter/samples/AsyncAPI/RMQAsyncAPI/` and `Brighter/samples/AsyncAPI/KafkaAsyncAPI/`
- Link to related Brighter docs: External Bus, Routing, Message Mappers

---

## Code Examples Plan

| # | Description | Source | Complete or Abbreviated |
|---|-------------|--------|------------------------|
| 1 | Minimal `UseAsyncApi()` registration | Written from scratch (based on samples) | Abbreviated — shows only the `.UseAsyncApi()` call in context of `AddBrighter()` |
| 2 | Calling `GenerateAsyncApiDocumentAsync()` | Based on `RMQAsyncAPI/Program.cs:108` | Complete — 4-5 lines showing the host extension call |
| 3 | `[PublicationTopic]` attribute on an event | Based on `RMQAsyncAPI/Events/OrderCreatedEvent.cs` | Complete — full class definition |
| 4 | Full `AsyncApiOptions` configuration with servers | Based on `RMQAsyncAPI/Program.cs:84-98` | Complete — shows all option properties |
| 5 | Custom `IAmASchemaGenerator` registration | Written from scratch | Abbreviated — shows DI registration pattern, not full implementation |
| 6 | `--generate-asyncapi` argument check pattern | Based on `RMQAsyncAPI/Program.cs:106-111` | Complete — the if-block |
| 7 | Complete working example (RMQ) | Based on `RMQAsyncAPI/Program.cs` (simplified) | Complete — full Program.cs without licence header |
| 8 | Sample generated JSON output | Generated from RMQ sample (or written to illustrate structure) | Abbreviated — show info, one channel, one operation, one message component |
| 9 | Complete working example (Kafka) | Based on `KafkaAsyncAPI/Program.cs` (simplified) | Complete — full Program.cs without licence header, highlighting deferred producer registry pattern |

---

## File Update: `contents/Requests, Commands and Events.md`

**Change:** Add a cross-link where AsyncAPI is already mentioned (line 31).

**Current text:**
> ...you should share a schema that defines the shape of the message (for example [AsyncAPI](https://www.asyncapi.com/).

**Updated text:**
> ...you should share a schema that defines the shape of the message (for example [AsyncAPI](https://www.asyncapi.com/). Brighter can [generate AsyncAPI documents automatically](/contents/AsyncAPISupport.md) from your service configuration.

---

## Style Notes

### Terminology

- **AsyncAPI** — always capitalised this way (not "Async API" or "asyncapi")
- **document** — the generated output (not "spec" or "specification" to avoid confusion with the AsyncAPI specification itself)
- **channel** — an AsyncAPI concept mapping to a topic/queue/routing key
- **operation** — a send or receive action on a channel
- **discovery** — the process by which Brighter finds messaging contracts (not "scanning" which implies only assembly scanning)

### Deviations from Standard Pattern

- The "Key Concepts" section is merged into the "What is AsyncAPI?" and "How Brighter Discovers" sections rather than being a standalone section, because the concepts are best explained alongside their usage.
- No "Common Pitfalls" section — the feature has a small surface area and few gotchas. The one notable pitfall (needing both NuGet packages) is covered in Prerequisites.

### Package References

The documentation must clearly state that users need **two** NuGet packages:
1. `Paramore.Brighter.AsyncAPI` — the core generator
2. `Paramore.Brighter.AsyncAPI.NJsonSchema` — the default schema generator (loaded via reflection)

This separation exists because schema generation is pluggable via `IAmASchemaGenerator`. Users who provide their own implementation don't need the NJsonSchema package.
