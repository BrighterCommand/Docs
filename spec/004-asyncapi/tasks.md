# Writing Tasks: AsyncAPI Documentation

**Spec:** 004-asyncapi
**Total tasks:** 12
**Estimated output:** ~400 lines in `contents/AsyncAPISupport.md` plus minor updates to 2 existing files

---

## Phase 1: Research & Preparation

**Goal:** Verify understanding of the feature by running the samples and reviewing the generated output.

- [x] **Task 1.1:** Run the RMQ sample and capture generated output
  - Input: `Brighter/samples/AsyncAPI/RMQAsyncAPI/Program.cs`
  - Output: Understanding of generated JSON/YAML structure; save a representative output snippet for Code Example 8
  - Notes: Run `dotnet run -- --generate-asyncapi` from the sample directory. If RabbitMQ is unavailable, the sample should still generate the document (it builds the host but doesn't start it). Review the generated `asyncapi.json` and `asyncapi.yaml` to understand the output shape.

- [x] **Task 1.2:** Review the `PublicationTopicAttribute` to understand assembly scanning
  - Input: `Brighter/src/Paramore.Brighter/PublicationTopicAttribute.cs`, `Brighter/src/Paramore.Brighter.AsyncAPI/AsyncApiDocumentGenerator.cs` (lines 221-275)
  - Output: Clear understanding of how the attribute maps to channel addresses for Code Example 3
  - Notes: Verify the attribute constructor signature and the `Destination.RoutingKey.Value` access pattern.

---

## Phase 2: Core Documentation (P0)

**Goal:** Write the main documentation file with all P0 content.

**Dependencies:** Phase 1 must be complete (need the generated output sample).

- [x] **Task 2.1:** Write H1 introduction and H2 "What is AsyncAPI?" section
  - Input: design.md section outline, AsyncAPI specification site (https://www.asyncapi.com/docs/reference/specification/v3.0.0)
  - Output: `contents/AsyncAPISupport.md` lines 1-40 (title, intro paragraph, conceptual explanation)
  - Notes: Keep the conceptual section brief (3-4 paragraphs). Explain AsyncAPI as "OpenAPI for async messaging". Link to the official spec. Note that Brighter generates v3.0.0 documents.

- [x] **Task 2.2:** Write H2 "Getting Started" section (Prerequisites + Adding AsyncAPI Generation)
  - Input: design.md, `Brighter/src/Paramore.Brighter.AsyncAPI/AsyncApiBrighterBuilderExtensions.cs`, `Brighter/src/Paramore.Brighter.AsyncAPI/AsyncApiHostExtensions.cs`
  - Output: `contents/AsyncAPISupport.md` "Getting Started" section (~40 lines) with Code Examples 1 and 2
  - Notes: Clearly state both NuGet packages are required (`Paramore.Brighter.AsyncAPI` + `Paramore.Brighter.AsyncAPI.NJsonSchema`). Show minimal `UseAsyncApi()` call in context, then the `GenerateAsyncApiDocumentAsync()` call. Mention both JSON and YAML are produced.

- [x] **Task 2.3:** Write H2 "How Brighter Discovers Your Messaging Contracts" section
  - Input: design.md, `AsyncApiDocumentGenerator.cs` (the three discovery loops), `RMQAsyncAPI/Events/OrderCreatedEvent.cs`
  - Output: `contents/AsyncAPISupport.md` "Discovery" section (~80 lines) with Code Example 3, covering Subscriptions, Publications, Assembly Scanning, and Deduplication subsections
  - Notes: Explain priority order (DI wins over assembly scanning). Include the `[PublicationTopic]` attribute example. Note that send-only and receive-only apps both work without errors.

- [x] **Task 2.4:** Write H2 "Configuration" section (AsyncApiOptions + Output Formats)
  - Input: design.md, `Brighter/src/Paramore.Brighter.AsyncAPI/AsyncApiOptions.cs`
  - Output: `contents/AsyncAPISupport.md` "Configuration" section (~50 lines) with Code Example 4 and options table
  - Notes: Use a markdown table for the options (Property | Type | Default | Description). Show a full configuration example including `Servers` dictionary. Explain JSON/YAML dual output.

- [x] **Task 2.5:** Write H2 "Custom Schema Generation" section
  - Input: design.md, `Brighter/src/Paramore.Brighter.AsyncAPI/IAmASchemaGenerator.cs`, `AsyncApiBrighterBuilderExtensions.cs` (reflection loading pattern)
  - Output: `contents/AsyncAPISupport.md` "Custom Schema Generation" section (~30 lines) with Code Example 5
  - Notes: Explain the interface contract (return `V3SchemaDefinition`, return empty schema on null/error). Show how to register a custom implementation before `UseAsyncApi()`. Note that NJsonSchema honours `DataAnnotations` and `JsonProperty` attributes.

---

## Phase 3: Supporting Documentation (P1)

**Goal:** Add CI/CD integration guidance and complete examples.

**Dependencies:** Phase 2 must be complete (these sections reference earlier content).

- [x] **Task 3.1:** Write H2 "CI/CD Integration" section
  - Input: design.md, `RMQAsyncAPI/Program.cs:106-111`, `KafkaAsyncAPI/Program.cs:117-122`
  - Output: `contents/AsyncAPISupport.md` "CI/CD Integration" section (~25 lines) with Code Example 6
  - Notes: Show the `--generate-asyncapi` argument pattern. Mention `asyncapi validate` CLI tool. Emphasise that the host must be built but doesn't need to run (no broker connection required for RMQ; Kafka has a specific pattern).

- [x] **Task 3.2:** Write H2 "Complete Examples" — RabbitMQ subsection
  - Input: `Brighter/samples/AsyncAPI/RMQAsyncAPI/Program.cs`, generated JSON from Task 1.1
  - Output: `contents/AsyncAPISupport.md` RabbitMQ example section (~60 lines) with Code Examples 7 and 8
  - Notes: Simplify the sample (remove licence header, simplify comments). Show the complete Program.cs. Follow with an abbreviated JSON output showing the document structure (info, one channel, one operation, one message in components).

- [x] **Task 3.3:** Write H2 "Complete Examples" — Kafka subsection
  - Input: `Brighter/samples/AsyncAPI/KafkaAsyncAPI/Program.cs`
  - Output: `contents/AsyncAPISupport.md` Kafka example section (~50 lines) with Code Example 9
  - Notes: Highlight the key Kafka difference: `KafkaProducerRegistryFactory.Create()` opens a real broker connection, so the sample conditionally skips producer registry creation in `--generate-asyncapi` mode. Assembly scanning via `[PublicationTopic]` fills the gap. Add a brief explanatory paragraph before the code.

- [x] **Task 3.4:** Write H2 "Further Reading" section
  - Input: design.md cross-links list
  - Output: `contents/AsyncAPISupport.md` "Further Reading" section (~15 lines)
  - Notes: Link to AsyncAPI spec, AsyncAPI Studio, both sample directories, and related Brighter docs (External Bus, Routing, Message Mappers, RabbitMQ Configuration, Kafka Configuration).

---

## Phase 4: Polish & Integration

**Goal:** Update SUMMARY.md, add cross-links, and verify the documentation is complete.

**Dependencies:** Phases 2 and 3 must be complete.

- [x] **Task 4.1:** Update SUMMARY.md and add cross-link in "Requests, Commands and Events.md"
  - Input: design.md SUMMARY.md diff, design.md cross-link update
  - Output: Updated `SUMMARY.md` (one new line), updated `contents/Requests, Commands and Events.md` (one sentence added)
  - Notes: Add ` * [AsyncAPI Document Generation](/contents/AsyncAPISupport.md)` after the Dynamic Message Deserialization entry. Add cross-link sentence after the existing AsyncAPI mention on line 31.

- [x] **Task 4.2:** Final review — verify code examples, cross-links, and quality checklist
  - Input: Complete `contents/AsyncAPISupport.md`, CLAUDE.md quality checklist
  - Output: Any corrections needed; mark spec tasks as complete
  - Notes: Check all code examples compile conceptually (correct using statements, correct API calls). Verify all internal links point to real files. Check terminology consistency. Verify the document follows the style guide (active voice, second person, present tense). Ensure no orphaned files.

---

## Task Dependencies Summary

```
Phase 1 (1.1, 1.2) — independent, can run in parallel
    │
    ▼
Phase 2 (2.1 → 2.2 → 2.3 → 2.4 → 2.5) — sequential, builds the file top-to-bottom
    │
    ▼
Phase 3 (3.1, 3.2, 3.3, 3.4) — 3.1-3.3 depend on Phase 2; 3.4 is independent
    │
    ▼
Phase 4 (4.1 → 4.2) — sequential, final integration
```
