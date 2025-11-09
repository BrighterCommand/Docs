# Brighter V10 Documentation Progress

**Last Updated**: 2025-01-04

## Project Status

- **Total Tasks**: 27
- **Completed**: 12
- **In Progress**: 0
- **Remaining**: 15
- **Completion**: 44%

---

## Setup Phase ✅

### Documentation Infrastructure

- ✅ Created `CLAUDE.md` - Instructions for Claude as technical documentation writer
- ✅ Created `REQUIREMENTS.md` - Requirements document with user answers
- ✅ Created `TASKS.md` - Detailed task breakdown (28 tasks)
- ✅ Created `PROGRESS.md` - This tracking file

---

## Phase 1: Core Framework Features (11/11 completed) ✅

### TASK-001: Cloud Events Support Documentation ✅

**Status**: Completed - 2025-01-02
**File**: `Docs/contents/CloudEventsSupport.md` (NEW)
**Priority**: HIGH
**Notes**: Comprehensive documentation covering CloudEvents specification, binary/structured modes, transport integration, and migration guidance

### TASK-002: Default Message Mappers Documentation ✅

**File**: `Docs/contents/DefaultMessageMappers.md` (NEW)
**Priority**: HIGH
**Notes**: Documented V10 simplification - no longer need explicit mappers for JSON. Covered default mappers (JsonMessageMapper, CloudEventJsonMessageMapper), transform pipelines with ClaimCheck example, and migration from V9

### TASK-003: Dynamic Message Deserialization Documentation ✅

**Status**: Completed - 2025-01-02
**File**: `Docs/contents/DynamicMessageDeserialization.md` (NEW)
**Priority**: HIGH
**Notes**: Documented content-based routing with getRequestType callback. Covered DataType Channel vs dynamic deserialization, CloudEvents type routing, custom routing strategies, performance considerations, and integration with Agreement Dispatcher

### TASK-004: Agreement Dispatcher Documentation ✅

**Status**: Completed - 2025-01-02
**File**: `Docs/contents/AgreementDispatcher.md` (NEW)
**Priority**: HIGH

### TASK-005: Reactor and Proactor Documentation ✅

**Status**: Completed - 2025-01-04
**Files**: `Docs/contents/ReactorAndProactor.md` (NEW) + updates throughout
**Priority**: HIGH
**Notes**: Created comprehensive documentation covering Reactor (blocking I/O) and Proactor (non-blocking I/O) concurrency models. Updated HowServiceActivatorWorks.md with proper Dispatcher terminology and Reactor/Proactor patterns. Replaced all references to deprecated `isAsync`/`runAsync` parameters with `messagePumpType` throughout documentation. Updated subscription examples in RabbitMQConfiguration.md, BrighterBasicConfiguration.md, and AzureServiceBusConfiguration.md. Added transport support matrix. Added to SUMMARY.md under "Under the Hood" section.

### TASK-006: Scheduled Requests/Messaging Overview ✅

**Status**: Completed - 2025-01-04
**File**: Updated `Docs/contents/BrighterSchedulerSupport.md`
**Priority**: HIGH
**Notes**: Completely rewrote scheduling documentation with comprehensive overview. Documented Send/Publish/Post with DateTimeOffset and TimeSpan parameters. Explained how scheduling works internally (FireSchedulerMessage, FireSchedulerRequest). Documented Requeue with Delay feature with transport-specific behavior. Added native delay support table for transports (RabbitMQ and Azure Service Bus native, others require scheduler). Created scheduler comparison table with recommendations. Included scheduler ID return value documentation for cancel/reschedule. Added decision guide flowchart. Provided extensive code examples including basic scheduling, cancellation, retry with exponential backoff, and requeue with delay. Included configuration examples for Hangfire, Quartz, and InMemory. Added links to specific scheduler documentation.

---

### TASK-007: InMemory Scheduler Documentation ✅

**Status**: Completed - 2025-01-04
**File**: `Docs/contents/InMemoryScheduler.md` (NEW)
**Priority**: MEDIUM
**Notes**: Created comprehensive InMemory Scheduler documentation. Documented what it is (timer-based, no persistence), when to use (testing, development, demos), production limitations (not durable), acceptable production scenarios (loss of scheduled work acceptable). Included ITimerProvider internals, configuration with InMemorySchedulerFactory, code examples (basic scheduling, cancellation, testing), integration with UseScheduler(). Added prominent warnings about durability throughout. Created comparison table with production schedulers. Provided migration examples to production schedulers. Added to SUMMARY.md under "Scheduler" section.

### TASK-008: Quartz Scheduler Documentation ✅

**Status**: Completed - 2025-01-04
**File**: Updated `Docs/contents/QuartzScheduler.md`
**Priority**: MEDIUM
**Notes**: Completely rewrote Quartz Scheduler documentation with production-grade guidance. Documented production recommendation alongside Hangfire. Explained Quartz benefits (battle-tested, persistent, distributed, reliable, flexible, strong naming). Documented how Brighter integrates (QuartzBrighterJob, BrighterResolver, QuartzSchedulerFactory). Added NuGet packages section. Created comprehensive configuration examples (basic, persistent job store, appsettings.json). Documented persistence options for SQL Server, PostgreSQL, MySQL, and in-memory (dev only). Added advanced configuration (custom scheduler ID generation, job groups, misfire handling). Provided code examples (basic scheduling, cancellation, absolute time). Documented clustering and HA setup with best practices. Added monitoring and observability section (Quartz listeners, health checks). Included 8 best practices with good/bad examples. Added troubleshooting section (jobs not executing, multiple execution, database deadlocks). Provided migration examples from InMemory and Hangfire. Added links to Quartz documentation and database scripts.

### TASK-009: Hangfire Scheduler Documentation ✅

**Status**: Completed - 2025-01-04
**File**: Updated `Docs/contents/HangfireScheduler.md`
**Priority**: MEDIUM
**Notes**: Completely rewrote Hangfire Scheduler documentation with production-grade guidance. Documented production recommendation alongside Quartz. Added prominent strong naming caveat (Hangfire assembly NOT strong-named due to Hangfire.Core limitation). Explained Hangfire benefits (dashboard, background jobs, persistence, ease of use). Documented how Brighter integrates (BrighterHangfireSchedulerJob, HangfireMessageSchedulerFactory, JobActivator). Added NuGet packages section. Created comprehensive configuration examples (basic, persistent storage, dashboard with authentication). Documented storage options for SQL Server, PostgreSQL, MySQL, Redis, MongoDB. Added dashboard configuration with authorization. Provided code examples (basic scheduling, cancellation, absolute time). Documented HA setup with multiple servers. Added monitoring section with job filters and health checks. Created comparison table: Hangfire vs Quartz (highlighting strong naming difference). Included 8 best practices with good/bad code comparisons. Added troubleshooting section (dashboard not accessible, jobs not executing, database deadlocks). Provided migration examples from InMemory and Quartz schedulers. Added links to Hangfire documentation and related Brighter scheduler docs.

### TASK-010: AWS Scheduler Documentation ✅

**Status**: Completed - 2025-01-04
**File**: Updated `Docs/contents/AwsScheduler.md`
**Priority**: MEDIUM
**Notes**: Completely rewrote AWS Scheduler documentation with comprehensive cloud-native guidance. Documented AWS EventBridge Scheduler benefits (serverless, scalable, reliable, cost-effective, native AWS integration). Explained when to use AWS Scheduler (AWS infrastructure, serverless architecture, scalability needs). Documented two integration approaches: Direct to Target (UseMessageTopicAsTarget=true, recommended) and Via FireAwsScheduler message (for request scheduling). Added comprehensive IAM role requirements section (trust policy, permissions policy, automatic role creation with OnMissingRole). Documented NuGet packages for AWS SDK v4 (recommended) and v3 (legacy). Created configuration examples (basic, Dispatcher with FireAwsScheduler, scheduler groups, flexible time window, custom scheduler ID). Provided 4 code examples (basic scheduling, absolute time, cancellation, publishing events). Created scheduling modes comparison table (Direct vs FireAwsScheduler). Added comparison table: AWS Scheduler vs Quartz vs Hangfire vs InMemory (highlighting cloud-native, serverless, pay-per-use benefits). Included 8 best practices with good/bad code comparisons. Added troubleshooting section (schedules not executing, access denied, schedule conflicts, high costs). Provided migration examples from InMemory and Quartz/Hangfire schedulers. Added links to AWS EventBridge Scheduler documentation, pricing, quotas, and LocalStack. Summary explains when to use AWS Scheduler vs other schedulers.

### TASK-011: Azure Scheduler Documentation ✅

**Status**: Completed - 2025-01-04
**File**: Updated `Docs/contents/AzureScheduler.md`
**Priority**: MEDIUM
**Notes**: Completely rewrote Azure Service Bus Scheduler documentation with comprehensive cloud-native guidance. Documented Azure Service Bus native scheduling using ScheduledEnqueueTimeUtc (built into Service Bus, no separate service). Explained important limitation: Azure does NOT support rescheduling (must cancel + schedule). Documented when to use Azure Service Bus Scheduler (Azure infrastructure, using Service Bus, simplicity) vs when NOT to use (multi-cloud, reschedule support needed). Explained FireAzureScheduler integration approach (centralized scheduler topic, not direct to target like AWS). Added comprehensive authentication section: Managed Identity (recommended), Visual Studio credentials (dev), connection string, Default Azure credentials. Documented RBAC permissions required (Azure Service Bus Data Sender, Data Receiver). Added NuGet package section (Paramore.Brighter.MessageScheduler.Azure). Created configuration examples (basic, Dispatcher with FireAzureScheduler, custom sender options, custom TimeProvider). Provided 4 code examples (basic scheduling, absolute time, cancellation, rescheduling with cancel+schedule pattern). Created comparison table: Azure vs AWS vs Quartz vs Hangfire vs InMemory (highlighting native scheduling, no reschedule, built-in). Included 8 best practices with good/bad code comparisons (use Managed Identity, separate scheduler topic, cancel+schedule pattern, message TTL, Azure Monitor, Premium tier, DLQ, Azurite for testing). Added troubleshooting section (messages not executing, auth failures, reschedule not working, dead-letter queue). Provided migration examples from InMemory and Quartz/Hangfire schedulers. Added links to Azure Service Bus documentation, scheduled messages, pricing, RBAC roles, Managed Identity. Summary explains native integration, managed service, simplicity, and Azure ecosystem integration. Prominent note about no reschedule support.
---

## Phase 2: Breaking Changes & Updates (1/5 completed)

### TASK-012: Request Context Improvements Documentation ✅

**Status**: Completed - 2025-01-04
**Files**: Updated `Docs/contents/UsingTheContextBag.md` and `Docs/contents/DispatchingARequest.md`
**Priority**: MEDIUM
**Notes**: Comprehensively updated Request Context documentation with all V10 capabilities. Updated UsingTheContextBag.md with new sections: Setting Request Context Explicitly (pass context to Send/Publish/DepositPost), Partition Key (message routing control via RequestContextBagNames.PartitionKey), Custom Headers (dynamic headers via RequestContextBagNames.Headers), CloudEvents Extensions (extension properties via RequestContextBagNames.CloudEventsAdditionalProperties), Originating Message (access original message in consumers for debugging/auditing), OpenTelemetry Span (custom attributes/events via Context.Span), Destination Override (runtime routing via Context.Destination), Resilience Context (Polly V8 integration), Resilience Pipeline Registry (access pre-configured pipelines). Documented Well-Known Context Bag Keys from RequestContextBagNames class. Added 5 best practices (use well-known keys, clean up resources, document custom keys, null checks, explicit context for important metadata). Included comprehensive code examples for each capability. Updated DispatchingARequest.md with new section "Setting Request Context Explicitly" with 3 examples: partition key and headers, CloudEvents extensions, transactional messaging with context. Cross-referenced between both documents and other related documentation (Telemetry, Polly, CloudEvents).

### TASK-013: Polly Resilience Pipeline Documentation ⬜

**Status**: Not Started
**Priority**: MEDIUM

### TASK-014: Simplified Configuration Documentation ⬜

**Status**: Not Started
**Priority**: MEDIUM

### TASK-015: OpenTelemetry Integration Documentation ⬜

**Status**: Not Started
**Priority**: MEDIUM
**Dependencies**: TASK-004

### TASK-016: Nullable Reference Types Documentation ⬜

**Status**: Not Started
**Priority**: MEDIUM

---

## Phase 3: Transport & Infrastructure

### TASK-017: PostgreSQL Message Broker Documentation ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-018: RabbitMQ Enhancements Documentation ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-019: Kafka Improvements Documentation ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-020: AWS Improvements Documentation ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-021: Sweeper Circuit Breaking Documentation ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-022: InMemory Options Overview Documentation ⬜

**Status**: Not Started
**Priority**: LOW
**Dependencies**: TASK-010

---

## Phase 4: Foundation & Migration

### TASK-023: Create V10 Migration Guide ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-024: Update SUMMARY.md Structure ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-025: Simplify Show Me The Code

**Status**: Not Started
**Priority**: LOW

---

## Phase 6: Glossary & Reference

### TASK-026: Create Terminology Glossary ⬜

**Status**: Not Started
**Priority**: LOW

### TASK-027: Update FAQ

**Status**: Not Started
**Priority**: LOW

---

## Completed Tasks Log

### 2025-01-02

**TASK-001: Cloud Events Support Documentation** ✅

- Created comprehensive CloudEvents documentation
- File: `Docs/contents/CloudEventsSupport.md`
- Covered: CloudEvents specification, required/optional attributes, binary vs structured modes
- Included: Transport-specific examples (RabbitMQ, Kafka, AWS, Azure)
- Documented: CloudEvents type for routing, OTel integration, DataRef for Claim Check
- Added: Migration guide from V9 custom headers to V10 CloudEvents
- Best practices and recommendations included

**TASK-002: Default Message Mappers Documentation** ✅

- Created comprehensive documentation on V10 default mappers
- File: `Docs/contents/DefaultMessageMappers.md`
- Explained V10 simplification: no explicit mappers needed for JSON serialization
- Covered both default mappers: JsonMessageMapper (binary-mode) and CloudEventJsonMessageMapper (structured-mode)
- Documented when custom mappers are still needed: non-JSON formats and transform pipelines
- Included real ClaimCheck transform example from samples
- Showed transform chaining (PII removal, compression, claim check)
- Provided migration guidance from V9 explicit mappers to V10 defaults
- Configuration examples for all scenarios
- Best practices and sample code references

**TASK-003: Dynamic Message Deserialization Documentation** ✅

- Created comprehensive documentation on content-based routing
- File: `Docs/contents/DynamicMessageDeserialization.md`
- Explained DataType Channel pattern (default, one type per channel)
- Documented dynamic deserialization with getRequestType callback
- CloudEvents type routing examples (primary approach)
- Custom routing strategies (headers, body content)
- Handler routing after type resolution
- Integration with Agreement Dispatcher for two-level routing
- Performance considerations and caching behavior
- Configuration examples for Kafka, RabbitMQ, AWS SQS
- Best practices for choosing DataType vs dynamic
- Comparison table and error handling
- Links to related documentation

**TASK-004: Agreement Dispatcher Documentation** ✅

- Pattern explained with Martin Fowler reference
- Multiple use cases documented with examples
- Registration examples provided
- AutoFromAssemblies limitation clear
- Performance implications documented
- Comparison with standard routing included
- Added to SUMMARY.md under "Advanced Patterns" section

### 2025-01-04

**TASK-005: Reactor and Proactor Documentation** ✅

- Created comprehensive ReactorAndProactor.md documentation
- File: `Docs/contents/ReactorAndProactor.md`
- Covered: Reactor pattern (blocking I/O) and Proactor pattern (non-blocking I/O)
- Documented: Performance vs throughput trade-offs, Performer (message pump) details
- Included: Transport native support matrix for all major transports
- Explained: ConfigureAwait(false) warning and synchronization context
- Created: Migration guide from V9 `isAsync`/`runAsync` to V10 `MessagePumpType`
- Provided: Decision guide for choosing between patterns with example scenarios
- Updated: HowServiceActivatorWorks.md with comprehensive Dispatcher documentation
- Replaced: All references to "ServiceActivator" with "Dispatcher" terminology (except assembly name)
- Updated: Subscription examples in RabbitMQConfiguration.md, BrighterBasicConfiguration.md, AzureServiceBusConfiguration.md
- Changed: `runAsync`/`isAsync` → `messagePumpType: MessagePumpType.Reactor/Proactor`
- Changed: `timeoutInMilliseconds` → `timeOut: TimeSpan.FromMilliseconds()`
- Added: ReactorAndProactor.md to SUMMARY.md under "Under the Hood" section

**TASK-006: Scheduled Requests/Messaging Overview** ✅

- Completely rewrote BrighterSchedulerSupport.md with comprehensive scheduling documentation
- File: `Docs/contents/BrighterSchedulerSupport.md`
- Added: Overview and use cases section with real-world scenarios
- Documented: All Send/Publish/Post variants with DateTimeOffset and TimeSpan parameters
- Explained: How scheduling works internally (FireSchedulerMessage vs FireSchedulerRequest)
- Documented: Return value (scheduler ID) for cancellation and reschedule
- Created: Native transport delay support table (RabbitMQ, Azure Service Bus native; others require scheduler)
- Created: Scheduler comparison table with Production Use, Persistence, Cancellation, Cloud Native columns
- Provided: Scheduler recommendations for Production (Quartz, Hangfire), Cloud (AWS, Azure), Dev/Test (InMemory)
- Added: Decision guide flowchart for choosing the right scheduler
- Documented: Requeue with Delay feature with transport-specific behavior
- Included: 8 comprehensive code examples (basic scheduling, cancellation, retry with backoff, requeue in handler)
- Added: Configuration examples for Hangfire, Quartz, and InMemory schedulers
- Included: 10 best practices for using scheduling
- Added: Links to all specific scheduler documentation pages
- Clear warnings about InMemory scheduler durability for production use

**TASK-007: InMemory Scheduler Documentation** ✅

- Created comprehensive InMemoryScheduler.md documentation
- File: `Docs/contents/InMemoryScheduler.md`
- Added: Prominent warning at top about production limitations
- Documented: What InMemory Scheduler is (timer-based using ITimerProvider, no persistence)
- Explained: Architecture and internal workings with visual flow diagram
- Documented: When to use (unit tests, local development, demos, limited production)
- Explained: When NOT to use (critical work, long delays, need durability)
- Added: Recommended use cases section with code examples
- Added: Limited production scenarios section with acceptable use cases
- Included: Configuration examples (basic, environment-specific, custom timer provider)
- Documented: NuGet package (Paramore.Brighter.InMemoryScheduler)
- Provided: 5 comprehensive code examples (basic scheduling, absolute time, cancellation, testing)
- Included: Complete unit test example with cancellation testing
- Added: Features section (supported features vs limitations)
- Created: Comparison table with production schedulers (InMemory vs Quartz vs Hangfire vs AWS vs Azure)
- Provided: 5 best practices with good/bad code examples
- Added: Migration guide to production schedulers (Hangfire, Quartz)
- Included: Troubleshooting section (jobs not executing, jobs lost after restart, memory usage)
- Added: Links to all related scheduler documentation
- Warnings: Emphasized "NOT durable", "NOT for production", "crashes lose all scheduled messages"
- Added to SUMMARY.md under "Scheduler" section (placed after main Scheduler doc, before production schedulers)

**TASK-008: Quartz Scheduler Documentation** ✅

- Completely rewrote QuartzScheduler.md with comprehensive production guidance
- File: `Docs/contents/QuartzScheduler.md`
- Added: Production recommendation section at top (alongside Hangfire as primary production scheduler)
- Documented: Quartz benefits (battle-tested, persistent, distributed, reliable, flexible, cancellation, monitoring, strong naming)
- Added: Quartz.NET overview with feature list (persistent job stores, clustering, trigger types, job chains, etc.)
- Explained: How Brighter integrates with Quartz (QuartzBrighterJob, BrighterResolver, QuartzSchedulerFactory)
- Documented: Integration flow (create job → persist → fire → QuartzBrighterJob → dispatch → handler)
- Added: NuGet packages section (Paramore.Brighter.MessageScheduler.Quartz, Quartz, Quartz.Extensions.Hosting)
- Created: Basic configuration example with QuartzBrighterJob registration
- Created: Configuration with persistent job store (SQL Server example with clustering)
- Created: Configuration with appsettings.json including full Quartz properties
- Documented: Persistence options for SQL Server, PostgreSQL, MySQL with database script links
- Added: In-memory option warning (development only, no durability)
- Documented: Advanced configuration (custom scheduler ID generation, job groups, misfire handling)
- Provided: 3 code examples (basic scheduling, cancellation, absolute time)
- Documented: Clustering and HA setup with complete configuration
- Explained: How clustering works (5 steps, shared job store, automatic failover)
- Added: Clustering best practices (SchedulerId AUTO, check-in intervals, NTP synchronization)
- Created: Monitoring and observability section with IJobListener example
- Added: Health check example with scheduler metadata
- Included: 8 best practices with good/bad code comparisons
- Added: Troubleshooting section (3 common issues with solutions)
- Provided: Migration examples from InMemory and Hangfire schedulers
- Added: Links to Quartz documentation and related Brighter scheduler docs
- Summary: Clear statement about when to use Quartz (robust, enterprise-grade, clustering, no dashboard needed)

**TASK-009: Hangfire Scheduler Documentation** ✅

- Completely rewrote HangfireScheduler.md with comprehensive production guidance
- File: `Docs/contents/HangfireScheduler.md`
- Added: Prominent strong naming caveat at top (Paramore.Brighter.MessageScheduler.Hangfire NOT strong-named)
- Explained: Strong naming limitation due to Hangfire.Core dependency, alternatives for strong-name requirements
- Documented: Hangfire benefits (dashboard, background jobs, persistence, ease of use, multiple storage options)
- Added: Hangfire overview with feature list (persistent storage, distributed processing, dashboard, automatic retries)
- Explained: How Brighter integrates with Hangfire (BrighterHangfireSchedulerJob, HangfireMessageSchedulerFactory, JobActivator)
- Documented: Integration flow (schedule → Hangfire persists → background job → BrighterHangfireSchedulerJob → handler)
- Added: NuGet packages section (Paramore.Brighter.MessageScheduler.Hangfire, Hangfire.Core, storage packages)
- Created: Basic configuration example with BrighterHangfireSchedulerJob registration
- Created: Configuration with persistent storage (SQL Server example)
- Created: Dashboard configuration with authentication (cookie-based and role-based examples)
- Documented: Storage options for SQL Server, PostgreSQL, MySQL, Redis, MongoDB with NuGet packages
- Provided: 3 code examples (basic scheduling, cancellation, absolute time)
- Documented: HA setup with multiple Hangfire servers
- Explained: How HA works (shared storage, automatic failover, distributed processing)
- Added: Monitoring section with custom job filters for logging/metrics
- Added: Health check example with Hangfire monitoring API
- Created: Comparison table: Hangfire vs Quartz (dashboard, ease of use, strong naming difference highlighted)
- Included: 8 best practices with good/bad code comparisons
- Added: Troubleshooting section (dashboard not accessible, jobs not executing, database deadlocks)
- Provided: Migration examples from InMemory and Quartz schedulers
- Added: Links to Hangfire documentation and related Brighter scheduler docs
- Summary: Clear statement about when to use Hangfire (dashboard needed, easy setup, NOT for strong-name requirements)

**TASK-010: AWS Scheduler Documentation** ✅

- Completely rewrote AwsScheduler.md with comprehensive cloud-native guidance
- File: `Docs/contents/AwsScheduler.md`
- Added: AWS EventBridge Scheduler overview (serverless, scalable, reliable, cost-effective)
- Documented: When to use AWS Scheduler (AWS infrastructure, serverless, scalability) vs when NOT to use (multi-cloud, on-premises)
- Explained: Two integration approaches with flow diagrams
  - Direct to Target (UseMessageTopicAsTarget=true): EventBridge → directly to SNS/SQS (recommended, lower latency)
  - Via FireAwsScheduler: EventBridge → FireAwsScheduler message → Dispatcher → handler (for request scheduling)
- Added: Comprehensive IAM role requirements section
  - Trust policy for scheduler.amazonaws.com
  - Permissions policy for sqs:SendMessage and sns:Publish
  - Automatic role creation with OnMissingRole.Create
  - Best practice: limit Resource to specific ARNs
- Documented: NuGet packages (AWS SDK v4 recommended, v3 legacy)
- Created: 5 configuration examples (basic, Dispatcher with FireAwsScheduler, scheduler groups, flexible time window, custom scheduler ID)
- Explained: Scheduler groups for organization with tags
- Explained: Flexible time windows for cost optimization
- Explained: Custom scheduler IDs for idempotency and tracking
- Provided: 4 code examples (basic scheduling, absolute time, cancellation, publishing events)
- Created: Scheduling modes comparison table (Direct to Target vs Via FireAwsScheduler)
- Created: Comparison table: AWS Scheduler vs Quartz vs Hangfire vs InMemory
  - Highlighted: Cloud-native, serverless, pay-per-use, AWS-only
- Included: 8 best practices with good/bad code comparisons
  - Use direct to target for messages
  - Limit IAM permissions
  - Use scheduler groups for organization
  - Handle OnConflict appropriately
  - Use custom IDs for idempotency
  - Monitor with CloudWatch
  - Use flexible time windows for cost savings
  - Test with LocalStack
- Added: Troubleshooting section (schedules not executing, access denied, schedule conflicts, high costs)
- Provided: Migration examples from InMemory and Quartz/Hangfire schedulers
- Added: Links to AWS EventBridge Scheduler documentation, pricing, quotas, LocalStack
- Summary: When to use AWS Scheduler (AWS workloads, serverless, high scalability) vs alternatives

**TASK-011: Azure Service Bus Scheduler Documentation** ✅

- Completely rewrote AzureScheduler.md with comprehensive cloud-native guidance
- File: `Docs/contents/AzureScheduler.md`
- Added: Azure Service Bus Scheduler overview (native scheduling using ScheduledEnqueueTimeUtc)
- Explained: Built into Service Bus (no separate service), managed service, reliable, integrated
- Documented: Important limitation - Azure does NOT support rescheduling (must cancel + schedule)
- Documented: When to use (Azure infrastructure, using Service Bus, simplicity) vs when NOT to use (multi-cloud, reschedule support needed)
- Explained: FireAzureScheduler integration approach with flow diagram
  - Centralized scheduler topic (not direct to target like AWS)
  - Why: Azure Service Bus requires topic/queue to hold scheduled message
- Added: Comprehensive authentication section
  - Managed Identity (recommended for production)
  - Visual Studio credentials (development)
  - Connection string (simple, less secure)
  - Default Azure credentials (flexible)
- Documented: RBAC permissions required
  - Azure Service Bus Data Sender (for scheduling)
  - Azure Service Bus Data Receiver (for Dispatcher)
  - Azure CLI examples for role assignment
- Added: NuGet package section (Paramore.Brighter.MessageScheduler.Azure)
- Created: 4 configuration examples (basic, Dispatcher with FireAzureScheduler, custom sender options, custom TimeProvider)
- Provided: 4 code examples (basic scheduling, absolute time, cancellation, rescheduling with cancel+schedule pattern)
- Created: Comparison table: Azure vs AWS vs Quartz vs Hangfire vs InMemory
  - Highlighted: Native scheduling (built-in), no reschedule support, managed service
- Included: 8 best practices with good/bad code comparisons
  - Use Managed Identity in production
  - Use separate scheduler topic
  - Handle reschedule as cancel + schedule
  - Set appropriate message TTL
  - Monitor with Azure Monitor
  - Use Service Bus Premium for production
  - Configure dead-letter queue
  - Test locally with Azurite
- Added: Troubleshooting section (messages not executing, authentication failures, reschedule not working, dead-letter queue)
- Provided: Migration examples from InMemory and Quartz/Hangfire schedulers
- Added: Links to Azure Service Bus documentation, scheduled messages, pricing, RBAC roles, Managed Identity
- Summary: Native integration, managed service, simple setup, Azure ecosystem integration
- Prominent notes throughout about no reschedule support (must use cancel + schedule pattern)

**TASK-012: Request Context Improvements Documentation** ✅

- Comprehensively updated Request Context documentation with all V10 capabilities
- Files: `Docs/contents/UsingTheContextBag.md` and `Docs/contents/DispatchingARequest.md`
- Updated UsingTheContextBag.md with new V10 sections:
  - Setting Request Context Explicitly: Pass context to Send/Publish/DepositPost methods
  - Partition Key: Control message routing via RequestContextBagNames.PartitionKey
  - Custom Headers: Dynamic headers via RequestContextBagNames.Headers
  - CloudEvents Extensions: Extension properties via RequestContextBagNames.CloudEventsAdditionalProperties
  - Originating Message: Access original message in consumers (Context.OriginatingMessage)
  - OpenTelemetry Span: Custom attributes/events via Context.Span
  - Destination Override: Runtime routing decisions via Context.Destination
  - Resilience Context: Polly V8 integration (Context.ResilienceContext)
  - Resilience Pipeline Registry: Access pre-configured pipelines (Context.ResiliencePipeline)
- Documented: Well-Known Context Bag Keys from RequestContextBagNames class
  - Headers, PartitionKey, CloudEventsAdditionalProperties, WorkflowId, JobId
- Added: 5 best practices
  - Use well-known keys (type-safe, discoverable)
  - Clean up resources (dispose unmanaged resources)
  - Document custom context keys (for maintainability)
  - Null checks before accessing properties (defensive coding)
  - Use explicit context for important metadata (routing, headers)
- Included: Comprehensive code examples for each capability (10+ examples)
- Updated DispatchingARequest.md with new section "Setting Request Context Explicitly"
  - Example: Partition key and custom headers from controller
  - Example: Publishing events with CloudEvents extensions
  - Example: Transactional messaging with explicit context (DepositPost)
- Cross-referenced: Related documentation (Telemetry, Polly, CloudEvents, Fallback)
- Explained: Integration points with Polly resilience pipelines and OpenTelemetry
- Documented: Use cases for each capability (multi-tenancy, auditing, debugging, routing)

---

## Notes

### Key Documentation Themes

- Start simple, add complexity gradually
- Reference Brighter samples directory
- Use "Dispatcher" terminology (not "ServiceActivator" except for assembly)
- Emphasize V10 defaults and simplifications
- Address user feedback: "hard to get started"

### Important References

- **Release Notes**: `Brighter/release_notes.md`
- **ADRs**: `Brighter/docs/adr/`
- **Samples**: `Brighter/samples/` (WebAPI, Transforms, etc.)
- **Requirements**: `Docs/REQUIREMENTS.md`
- **Task Details**: `Docs/TASKS.md`

---

## Next Steps

**Recommended**: Start with Phase 1 (Foundation & Migration)
1. TASK-001: Migration Guide (CRITICAL)
2. TASK-002: Update SUMMARY.md
3. TASK-003: Simplify Show Me The Code

**To Continue**: Tell Claude:
- "Continue with the next task"
- "Start TASK-001"
- "Complete Phase 1"
