# Requirements: AsyncAPI Documentation

## Topic Overview

Brighter includes a `Paramore.Brighter.AsyncAPI` package that automatically generates [AsyncAPI 3.0](https://www.asyncapi.com/docs/reference/specification/v3.0.0) documents from a service's runtime configuration. This documentation will explain what AsyncAPI is, why it matters for message-driven architectures, and how to use Brighter's AsyncAPI support to produce machine-readable API documentation.

AsyncAPI is the equivalent of OpenAPI (Swagger) for asynchronous messaging — it defines the channels a service publishes to and subscribes from, along with JSON Schema for each message type. Brighter can auto-generate this document by inspecting registered subscriptions, publications, and `[PublicationTopic]`-decorated `IRequest` types.

**Why this documentation is needed:** There is currently no documentation for this feature. The only mention of AsyncAPI in the existing docs is a passing reference in "Requests, Commands and Events.md" recommending schema sharing. Users have no way to discover or learn how to use this capability without reading source code.

## Current State

- **Existing documentation:** No dedicated AsyncAPI documentation exists.
- **Single mention:** `contents/Requests, Commands and Events.md` mentions AsyncAPI briefly as a schema-sharing standard.
- **Source material available:** Full specification in `Brighter/specs/0023-asyncapi-document-generation/`, two working samples (RMQ and Kafka), and complete source code in `Brighter/src/Paramore.Brighter.AsyncAPI/`.

## Target State

A documentation section that enables users to:
1. Understand what AsyncAPI is and why it's valuable for Brighter services
2. Set up AsyncAPI document generation in their service with minimal code
3. Understand the three discovery mechanisms (subscriptions, publications, assembly scanning)
4. Customize the generated document (title, version, servers, schema generation)
5. Integrate document generation into their CI/CD pipeline
6. Validate their generated documents using the AsyncAPI CLI

## Target Audience

- **Primary:** Intermediate developers already using Brighter's external bus features who want to document their messaging contracts
- **Secondary:** Beginners evaluating Brighter who want to understand its tooling capabilities
- **Tertiary:** Advanced users who want to implement custom schema generators or extend the generated documents

## Source Material

| Source | Location | Purpose |
|--------|----------|---------|
| Feature spec | `Brighter/specs/0023-asyncapi-document-generation/` | Full PRD and requirements |
| Source code | `Brighter/src/Paramore.Brighter.AsyncAPI/` | Public API surface |
| NJsonSchema package | `Brighter/src/Paramore.Brighter.AsyncAPI.NJsonSchema/` | Default schema generator |
| RMQ sample | `Brighter/samples/AsyncAPI/RMQAsyncAPI/` | End-to-end RabbitMQ example |
| Kafka sample | `Brighter/samples/AsyncAPI/KafkaAsyncAPI/` | End-to-end Kafka example |
| PublicationTopic attribute | `Brighter/src/Paramore.Brighter/PublicationTopicAttribute.cs` | Assembly scanning attribute |
| AsyncAPI specification | https://www.asyncapi.com/docs/reference/specification/v3.0.0 | External reference |

## Scope

### P0 — Must Have

1. **What is AsyncAPI** — Brief conceptual introduction explaining AsyncAPI's role as OpenAPI for async messaging, and why machine-readable messaging contracts matter
2. **Getting started** — Step-by-step setup guide showing how to add the NuGet package, call `UseAsyncApi()`, and generate a document with `GenerateAsyncApiDocumentAsync()`
3. **Discovery mechanisms** — Explain the three ways Brighter discovers messaging contracts:
   - Subscriptions registered via `AddConsumers()` → `receive` operations
   - Publications registered via `AddProducers()` → `send` operations  
   - Assembly scanning for `[PublicationTopic]`-decorated `IRequest` types → `send` operations
4. **Configuration options** — Document `AsyncApiOptions` properties: `Title`, `Version`, `Description`, `Servers`, `AssembliesToScan`, `DisableAssemblyScanning`, `SupplementalPublications`
5. **Output formats** — Explain that both JSON and YAML files are generated, with JSON as the canonical format

### P1 — Should Have

6. **The `[PublicationTopic]` attribute** — How to decorate event types for assembly scanning discovery
7. **Custom schema generation** — How to implement `IAmASchemaGenerator` to substitute the default NJsonSchema-based generation
8. **CI/CD integration** — Pattern for using `--generate-asyncapi` argument to generate documents in a build pipeline
9. **Deduplication behaviour** — How Brighter handles the same routing key appearing in multiple sources (subscriptions, publications, assembly scanning)
10. **Working with send-only and receive-only services** — Explain that both configurations work without errors

### P2 — Nice to Have

11. **Validating generated documents** — Using the AsyncAPI CLI (`asyncapi validate`) to validate output
12. **Using generated documents with AsyncAPI tooling** — Brief pointers to AsyncAPI Studio, Microcks, code generators
13. **Kafka sample walkthrough** — Explain the Kafka-specific patterns (deferred producer registry creation)

## Out of Scope

- Internal implementation details of `AsyncApiDocumentGenerator` (not user-facing)
- The Neuroglia AsyncAPI .NET SDK internals (third-party library)
- HTTP endpoint exposure (not implemented — file generation only)
- Bindings, tags, and protocol-specific metadata (not yet supported)
- MSBuild tasks or dotnet global tools (not implemented)
- Message filtering (all subscriptions/publications are included)
- Explaining the AsyncAPI specification itself in depth (link to external docs)

## Documentation Deliverables

### New Files

| File | Description |
|------|-------------|
| `contents/AsyncAPISupport.md` | Main documentation page covering what AsyncAPI is, getting started, configuration, discovery mechanisms, output formats, CI/CD integration, and custom schema generation |

### Files to Update

| File | Change |
|------|--------|
| `SUMMARY.md` | Add AsyncAPI entry to the documentation |
| `contents/Requests, Commands and Events.md` | Add cross-link to the new AsyncAPI documentation where AsyncAPI is mentioned |

## SUMMARY.md Changes

Add a new entry in the "Using an External Bus" section, after "Dynamic Message Deserialization" and before "Error Handling":

```markdown
 * [AsyncAPI Document Generation](/contents/AsyncAPISupport.md)
```

This placement is appropriate because AsyncAPI generation is a feature of the external bus infrastructure — it documents the channels and messages that the external bus uses.

## Constraints

### Terminology

- Use "AsyncAPI" (capital A, capital P, capital I) — this is the official project name
- Use "document" not "spec" when referring to the generated output (to avoid confusion with the AsyncAPI specification itself)
- Use "channel" for topics/queues (AsyncAPI terminology)
- Use "operation" for send/receive actions (AsyncAPI terminology)
- Refer to the package as `Paramore.Brighter.AsyncAPI`

### Code Examples

- All examples must use V10 patterns
- Show both the `UseAsyncApi()` registration and the `GenerateAsyncApiDocumentAsync()` call
- Use realistic domain examples (Orders, Payments) consistent with the samples
- Include `using` statements for clarity
- Reference the working samples: `Brighter/samples/AsyncAPI/RMQAsyncAPI/` and `Brighter/samples/AsyncAPI/KafkaAsyncAPI/`

### Cross-Linking

- Link to `BasicConcepts.md` for IRequest, Event, Command definitions
- Link to `Routing.md` for RoutingKey concepts
- Link to `DefaultMessageMappers.md` for message mapper context
- Link to relevant transport configuration pages (RabbitMQ, Kafka) from examples
- Link to external AsyncAPI specification site

### Style

- Follow the how-to guide pattern: goal statement, prerequisites, steps, expected outcome
- Keep the conceptual introduction brief (3-4 paragraphs max)
- Lead with the simplest setup, then progressively add complexity
- Show expected output (a sample of what the generated JSON looks like)
