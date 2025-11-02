# Brighter V10 Documentation Tasks

This document contains the structured task list for updating the Brighter/Darker documentation for V10. Each task is numbered and includes scope, dependencies, and acceptance criteria.

## Task Status Legend
- ⬜ Not Started
- 🔄 In Progress
- ✅ Completed
- ⏸️ Blocked (waiting for clarification)

---

## Phase 1: Foundation & Migration (Critical Path)

### TASK-001: Create V10 Migration Guide ⬜
**Priority**: CRITICAL
**Estimated Effort**: Large
**Dependencies**: None
**File**: `Docs/contents/MigrationV10.md` (NEW)

**Description**: Create comprehensive migration guide from V9 to V10

**Content Requirements**:
- Summary of all breaking changes
- Step-by-step migration checklist
- Code comparison table (V9 vs V10)
- Update Subscription configuration (isAsync/runAsync → messagePumpType)
- Update timeouts (milliseconds → TimeSpan)
- Handle nullable reference types
- Update builder calls (UseExternalBus → AddProducers, AddServiceActivator → AddConsumers)
- Migrate policies ([TimeoutPolicy] → [UseResiliencePipeline], [UsePolicy] → [UseResiliencePipeline])
- Message ID changes (GUID → string)
- Database schema updates for Inbox/Outbox
- Generic message pumps - remove type parameters
- Common migration issues and solutions

**Acceptance Criteria**:
- [ ] All breaking changes from release notes covered
- [ ] Each breaking change has before/after code example
- [ ] Step-by-step checklist provided
- [ ] Links to relevant detailed documentation
- [ ] Added to SUMMARY.md

---

### TASK-002: Update SUMMARY.md Structure ⬜
**Priority**: HIGH
**Estimated Effort**: Small
**Dependencies**: None
**File**: `Docs/SUMMARY.md`

**Description**: Reorganize and update documentation table of contents

**Content Requirements**:
- Add all new V10 feature documentation pages
- Add "Migration" section with V10 migration guide
- Add "Advanced Patterns" section (Agreement Dispatcher, Sweeper Circuit Breaking)
- Ensure "Scheduler" section has all scheduler types
- Use "Dispatcher" terminology in headings (not ServiceActivator unless referring to assembly)
- Ensure logical flow: Overview → Basic Config → Handlers → Messaging → Advanced
- Clear separation of Brighter and Darker content where needed

**Acceptance Criteria**:
- [ ] All new pages added
- [ ] Logical grouping maintained
- [ ] Migration guide prominent
- [ ] "Dispatcher" terminology used appropriately
- [ ] Clear navigation for newcomers vs advanced users

---

### TASK-003: Simplify Show Me The Code ⬜
**Priority**: HIGH
**Estimated Effort**: Medium
**Dependencies**: TASK-005 (Default Message Mappers)
**File**: `Docs/contents/ShowMeTheCode.md`

**Description**: Simplify the introductory code example to help newcomers get started

**Content Requirements**:
- Replace complex Post example with simple Send (no transactions)
- Use InMemory Outbox in simple example
- Use default message mapper (no explicit mapper needed in V10)
- Add clear note explaining this is simplified version
- Add note that it uses InMemory Outbox (not durable)
- Link to full Outbox documentation for production example
- Move current complex example to BrighterOutboxSupport.md
- Ensure all code examples use V10 syntax
- Reference WebAPI sample for fully-featured example
- Emphasize "start simple, add complexity later" philosophy per user feedback

**Acceptance Criteria**:
- [ ] Simple example uses basic Send without transaction
- [ ] InMemory Outbox clearly indicated with limitations
- [ ] No explicit message mapper shown (uses default)
- [ ] Link to detailed Outbox docs provided
- [ ] Link to WebAPI sample provided
- [ ] Complex example preserved in Outbox documentation
- [ ] Clear progression from simple to complex

---

## Phase 2: Core Framework Features (High Priority)

### TASK-004: Cloud Events Support Documentation ⬜
**Priority**: HIGH
**Estimated Effort**: Large
**Dependencies**: None
**File**: `Docs/contents/CloudEventsSupport.md` (NEW)

**Description**: Document full CloudEvents specification support

**Content Requirements**:
- What are CloudEvents and why they matter (interoperability, CNCF standard)
- Reference .NET 9 Eventing Framework, Azure Event Grid adoption
- **Mandatory CloudEvents properties**: id, source, type, datacontenttype
- Optional CloudEvents properties: dataschema, subject, time, specversion
- **Binary mode** (headers): Recommended when protocol supports headers
- **Structured mode** (JSON body): Recommended when protocol has insufficient headers (SNS/SQS)
- Setting CloudEvents in Publication
- Reading CloudEvents in Message Mapper
- CloudEvents header mapping across transports (AMQP, Kafka, RabbitMQ, etc.)
- CloudEvents type for message routing (link to TASK-006)
- OTel integration via CloudEvents attributes (traceParent, traceState)
- DataRef for Claim Check pattern
- Migration from V9 custom headers to CloudEvents
- Code examples for binary mode
- Code examples for structured mode
- Transport-specific examples (RabbitMQ binary, SNS/SQS structured)

**Acceptance Criteria**:
- [ ] Explains CloudEvents specification and benefits
- [ ] Mandatory properties emphasized
- [ ] Binary vs structured guidance clear (protocol-dependent)
- [ ] Shows binary mode configuration
- [ ] Shows structured mode configuration
- [ ] Transport-specific examples included
- [ ] Migration guide from custom headers
- [ ] Links to message routing documentation
- [ ] Added to SUMMARY.md under "Brighter Configuration"

---

### TASK-005: Default Message Mappers Documentation ⬜
**Priority**: HIGH
**Estimated Effort**: Medium
**Dependencies**: TASK-004 (CloudEvents)
**File**: `Docs/contents/DefaultMessageMappers.md` (NEW)

**Description**: Document automatic message mapping with default mappers

**Content Requirements**:
- Explain V10 change: no longer need explicit IAmAMessageMapper for JSON
- **JsonMessageMapper** (binary CloudEvents): Default for JSON serialization
- **CloudEventJsonMessageMapper** (structured CloudEvents): Alternative default
- When default mappers are used automatically
- How to register custom default mapper type (e.g., for Avro, ProtoBuf)
- **When you still need custom IAmAMessageMapper**:
  - Non-JSON formats (Avro, ProtoBuf, etc.)
  - Transform pipelines (ClaimCheck, Compression, Encryption, PII, etc.)
- **Transform pipeline example** from `Brighter/samples/Transforms/AWSTransformers/ClaimCheck/Greetings/Ports/Mappers/GreetingEventMessageMapper.cs`:
  - Show [ClaimCheck] attribute on MapToMessage
  - Show [RetrieveClaim] attribute on MapToRequest
  - Explain transform pipeline execution
- Code examples showing auto-mapping (no mapper registration)
- Code examples showing custom default mapper registration
- Code examples showing transform-based custom mapper
- Migration from explicit mappers to default mappers
- How Publication is passed to mapper in V10

**Acceptance Criteria**:
- [ ] Explains both default mapper types
- [ ] Shows automatic usage (no registration needed)
- [ ] Shows custom default mapper registration
- [ ] Transform pipeline use case explained with code example
- [ ] ClaimCheck example included
- [ ] All use cases for custom mappers documented
- [ ] Includes migration guidance
- [ ] Update MessageMappers.md to reference this
- [ ] Added to SUMMARY.md

---

### TASK-006: Dynamic Message Deserialization Documentation ⬜
**Priority**: HIGH
**Estimated Effort**: Medium
**Dependencies**: TASK-004 (CloudEvents)
**File**: `Docs/contents/DynamicMessageDeserialization.md` (NEW)

**Description**: Document content-based routing and dynamic type resolution

**Content Requirements**:
- Explain **DataType Channel** pattern (default, one type per channel)
- When to use dynamic deserialization (multiple types on one channel)
- Using **getRequestType callback** in Subscription
- Using CloudEvents type for routing (reference CloudEvents documentation)
- Multiple message types on one channel
- How routing to handler works after type resolution
- Performance implications (runtime type resolution, caching optimization)
- Code example with switch-based routing
- Code example using CloudEvents type routing (TaskCreated, TaskUpdated example from release notes)
- How this integrates with Agreement Dispatcher (TASK-007)
- Best practices and recommendations

**Release Notes Example**:
```csharp
new KafkaSubscription(
    new SubscriptionName("paramore.example.taskstate"),
    channelName: new ChannelName("task.state"),
    routingKey:new RoutingKey("task.update"),
    getRequestType: message => message switch
    {
        var m when m.Header.Type == new CloudEventsType("io.goparamore.task.created") => typeof(TaskCreated),
        var m when m.Header.Type == new CloudEventsType("io.goparamore.task.updated") => typeof(TaskUpdated),
        _ => throw new ArgumentException($"No type mapping found for message with type {message.Header.Type}", nameof(message)),
    },
    // ... other config
)
```

**Acceptance Criteria**:
- [ ] Explains DataType Channel clearly
- [ ] Shows getRequestType callback usage
- [ ] Multiple message types example
- [ ] Integration with CloudEvents shown
- [ ] Performance notes included (caching)
- [ ] Best practices documented
- [ ] Links to Agreement Dispatcher
- [ ] Added to SUMMARY.md under "Brighter Request Handlers"

---

### TASK-007: Agreement Dispatcher Documentation ⬜
**Priority**: HIGH
**Estimated Effort**: Medium
**Dependencies**: None
**File**: `Docs/contents/AgreementDispatcher.md` (NEW)

**Description**: Document dynamic handler resolution pattern

**Content Requirements**:
- Explain **Agreement Dispatcher** pattern (reference Martin Fowler's EAA)
- Standard routing: 1-to-1 mapping of request type to handler
- Agreement routing: Dynamic selection based on request content/context
- **Use cases**:
  - Time-based routing (rules changing over time)
  - Order journeys (different routes based on order contents)
  - Country-specific business logic
  - Versioning scenarios
  - State-based routing
- Registering agreement dispatcher routes via lambda
- Request and IRequestContext parameters
- Casting IRequest to access state
- Returning List<Type> of matching handlers
- Registering handler types for DI
- **Cannot use AutoFromAssemblies** - must use Handlers() method
- Performance implications (lookup + lambda execution, minor overhead)
- Code example with time-based routing
- Code example with content-based routing (order example)
- Code example with state-based routing
- Comparison with standard 1-to-1 mapping
- Integration with dynamic message deserialization

**Release Notes Example**:
```csharp
registry.RegisterAsync<MyCommand>(((request, context) =>
{
    var myCommand = request as MyCommand;
    if (myCommand?.Value == "first")
        return [typeof(MyImplicitHandlerAsync)];

    return [typeof(MyCommandHandlerAsync)];
}),
    [typeof(MyImplicitHandlerAsync), typeof(MyCommandHandlerAsync)]
);
```

**Acceptance Criteria**:
- [ ] Pattern explained with Martin Fowler reference
- [ ] Multiple use cases documented with examples
- [ ] Registration examples provided
- [ ] AutoFromAssemblies limitation clear
- [ ] Performance implications documented
- [ ] Comparison with standard routing included
- [ ] Added to SUMMARY.md under "Advanced Patterns" section

---

### TASK-008: Reactor and Proactor Documentation ⬜
**Priority**: HIGH
**Estimated Effort**: Large
**Dependencies**: None
**Files**:
- `Docs/contents/ReactorAndProactor.md` (NEW)
- Update `Docs/contents/HowServiceActivatorWorks.md`
- Update all subscription examples throughout docs

**Description**: Document concurrency models with new terminology

**Content Requirements**:
- **Reactor pattern**: Blocking I/O, faster performance (no context switch)
- **Proactor pattern**: Non-blocking I/O, better throughput (yields thread during I/O)
- Explain **Performer** (message pump) is single-threaded in either case
- Trade-offs: Performance (Reactor) vs. Throughput (Proactor)
- ConfigureAwait(false) caveat: Uses thread pool, ignores synchronization context (link to https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- MessagePumpType.Reactor configuration
- MessagePumpType.Proactor configuration
- When to use each model (container environments, competing consumers, streams)
- **Transport native support table**:

| Transport | Reactor Support | Proactor Support |
|-----------|----------------|------------------|
| Azure Service Bus | Sync over Async | Native |
| AWS (SNS/SQS) | Sync over Async | Native |
| Kafka | Native | Async over Sync |
| MQTT | Sync over Async/Event Based | Event Based |
| MSSQL | Native | Native |
| RabbitMQ v6 | Sync over Async | Native |
| RabbitMQ v7 | Sync over Async | Native |
| Redis | Native | Native |

- Code examples for both models
- Migration from isAsync/runAsync flag to MessagePumpType
- Update HowServiceActivatorWorks.md to use Reactor/Proactor terminology
- Replace all "blocking/non-blocking" references with Reactor/Proactor
- Use "Dispatcher" terminology (not "ServiceActivator") except when referring to assembly

**Acceptance Criteria**:
- [ ] Both patterns explained clearly
- [ ] Performance vs throughput trade-offs clear
- [ ] ConfigureAwait warning included with link
- [ ] Configuration examples for both
- [ ] Transport support table included
- [ ] Migration guide included
- [ ] All docs updated (no more blocking/non-blocking terms)
- [ ] "Dispatcher" terminology used appropriately
- [ ] Added to SUMMARY.md under "Under the Hood"

---

### TASK-009: Scheduled Requests/Messaging Overview ⬜
**Priority**: HIGH
**Estimated Effort**: Medium
**Dependencies**: None
**File**: Update `Docs/contents/BrighterSchedulerSupport.md`

**Description**: Document overview of scheduling support

**Content Requirements**:
- Scheduling overview and use cases
- Send/Publish/Post with DateTimeOffset parameter
- Send/Publish/Post with TimeSpan parameter
- How scheduling works internally (FireSchedulerMessage, FireSchedulerRequest)
- **Requeue with Delay** feature
- **Native delay support**: Only RabbitMQ supports native "Requeue with Delay"
- All other transports require a scheduler
- Choosing a scheduler:
  - **Production recommended**: Quartz.NET, Hangfire
  - **Cloud providers**: AWS Scheduler (AWS), Azure Service Bus Scheduler (Azure)
  - **Testing/Dev**: InMemory Scheduler
- Scheduler comparison table
- Return value: scheduler ID for cancel/reschedule
- Links to specific scheduler documentation
- Code examples with basic scheduling
- Code examples with requeue delay

**Acceptance Criteria**:
- [ ] Overview clearly explains feature
- [ ] All Send/Publish/Post variants documented
- [ ] Transport native support clearly stated (RabbitMQ only)
- [ ] Scheduler recommendations provided
- [ ] Scheduler comparison table included
- [ ] Links to detailed scheduler docs
- [ ] Requeue with Delay explained
- [ ] Return value (scheduler ID) documented
- [ ] Existing file updated (not replaced)

---

## Phase 3: Scheduler Implementations (Medium Priority)

### TASK-010: InMemory Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Small
**Dependencies**: TASK-009
**File**: `Docs/contents/InMemoryScheduler.md` (NEW)

**Description**: Document InMemory scheduler for testing/dev

**Content Requirements**:
- What is InMemory scheduler (timer-based, no persistence)
- **When to use**: Testing, development, demos
- **Production limitations**: Not durable - crashes lose scheduled messages
- Acceptable production scenarios: Where loss of scheduled work is acceptable
- Uses ITimerProvider internally
- Configuration with InMemorySchedulerFactory
- Code examples
- Integration with UseScheduler()
- Clear warning about durability

**Acceptance Criteria**:
- [ ] Purpose clearly stated
- [ ] Use cases explicit (testing, dev, demos)
- [ ] Limitations documented (no persistence)
- [ ] Acceptable production scenarios noted
- [ ] Configuration example provided
- [ ] Production warning prominent
- [ ] Added to SUMMARY.md under "Scheduler" section

---

### TASK-011: Quartz Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Small
**Dependencies**: TASK-009
**File**: Update `Docs/contents/QuartzScheduler.md`

**Description**: Update Quartz.NET integration documentation

**Content Requirements**:
- Overview of Quartz integration
- **Production recommended** (with Hangfire)
- Quartz benefits: Mature, persistent, distributed
- BrighterResolver job factory
- Configuration with Quartz
- QuartzSchedulerFactory setup
- Registering QuartzBrighterJob
- Persistence options with Quartz (SQL stores)
- Code examples from release notes
- Best practices
- Links to Quartz documentation

**Acceptance Criteria**:
- [ ] V10 configuration shown
- [ ] Production recommendation clear
- [ ] Job factory registration clear
- [ ] Persistence options explained
- [ ] Code examples from release notes
- [ ] Best practices included
- [ ] File updated with V10 changes

---

### TASK-012: Hangfire Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Small
**Dependencies**: TASK-009
**File**: Update `Docs/contents/HangfireScheduler.md`

**Description**: Update Hangfire integration documentation

**Content Requirements**:
- Overview of Hangfire integration
- **Production recommended** (with Quartz)
- Hangfire benefits: Dashboard, easy setup, persistent
- BrighterHangfireSchedulerJob registration
- Configuration with Hangfire
- HangfireMessageSchedulerFactory setup
- **Strong naming note**: Brighter.Hangfire NOT signed due to Hangfire strong naming issue
- JobActivator registration
- Code examples from release notes
- Best practices
- Links to Hangfire documentation

**Acceptance Criteria**:
- [ ] V10 configuration shown
- [ ] Production recommendation clear
- [ ] Job registration clear
- [ ] Strong naming caveat prominent
- [ ] Code examples from release notes
- [ ] Best practices included
- [ ] File updated with V10 changes

---

### TASK-013: AWS Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Medium
**Dependencies**: TASK-009
**File**: Update `Docs/contents/AwsScheduler.md`

**Description**: Update AWS Scheduler integration documentation

**Content Requirements**:
- Overview of AWS EventBridge Scheduler integration
- **Cloud provider recommended** for AWS deployments
- Direct SNS/SQS scheduling (no handler needed for messages)
- Fire scheduler SNS/SQS for requests
- UseMessageTopicAsTarget flag (default true)
- Configuration options:
  - OnConflict (Overwrite, etc.)
  - GetOrCreateSchedulerId
  - SchedulerTopicOrQueue
- **IAM role requirements**:
  - Assume role policy (scheduler.amazonaws.com)
  - Action policy (sqs:SendMessage, sns:Publish)
- Code examples from release notes
- Architecture diagram (direct vs fire scheduler)
- Best practices

**Acceptance Criteria**:
- [ ] V10 configuration shown
- [ ] Cloud provider recommendation clear
- [ ] Both scheduling modes explained
- [ ] IAM requirements clearly documented (with JSON)
- [ ] Configuration options explained
- [ ] Code examples from release notes
- [ ] Architecture clear
- [ ] Best practices included
- [ ] File updated with V10 changes

---

### TASK-014: Azure Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Medium
**Dependencies**: TASK-009
**File**: Update `Docs/contents/AzureScheduler.md`

**Description**: Update Azure Scheduler integration documentation

**Content Requirements**:
- Overview of Azure Service Bus scheduling
- **Cloud provider recommended** for Azure deployments
- Azure Service Bus native delay support
- **No reschedule support**: Must cancel + schedule
- Fire scheduler topic/queue approach
- AzureServiceBusSchedulerFactory configuration
- SchedulerTopicOrQueue configuration
- Code examples from release notes
- Architecture diagram
- Best practices
- Limitation: No direct topic/queue scheduling (no Azure EventBridge equivalent)

**Acceptance Criteria**:
- [ ] V10 configuration shown
- [ ] Cloud provider recommendation clear
- [ ] Native delay feature explained
- [ ] Reschedule limitation prominent
- [ ] Configuration clear
- [ ] Code examples from release notes
- [ ] Architecture clear
- [ ] Best practices included
- [ ] File updated with V10 changes

---

### TASK-015: Custom Scheduler Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Small
**Dependencies**: TASK-009
**File**: Update `Docs/contents/CustomScheduler.md`

**Description**: Update custom scheduler implementation guide

**Content Requirements**:
- When to implement custom scheduler
- **IAmAMessageScheduler interface** (messages)
- **IAmARequestScheduler interface** (requests)
- IAmAMessageSchedulerAsync methods
- IAmAMessageSchedulerSync methods
- **FireSchedulerMessage** type
- **FireSchedulerRequest** type
- Implementation guide with steps
- Code examples from ADR
- Integration with UseScheduler()
- Best practices

**Acceptance Criteria**:
- [ ] All scheduler interfaces documented
- [ ] Async and sync versions explained
- [ ] Fire message types explained
- [ ] Implementation guide clear
- [ ] Code examples provided
- [ ] Integration shown
- [ ] File updated with V10 changes

---

## Phase 4: Breaking Changes & Updates (Medium Priority)

### TASK-016: Request Context Improvements Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Medium
**Dependencies**: None
**Files**:
- Update `Docs/contents/UsingTheContextBag.md`
- Update `Docs/contents/DispatchingARequest.md`

**Description**: Document new RequestContext capabilities

**Content Requirements**:
- Setting RequestContext explicitly in Send/Publish/DepositPost methods
- **OriginatingMessage** property for consumers (access original Message)
- **PartitionKey** support for dynamic partition assignment
- **Custom headers** via context
- **Resilience Context** integration with Polly
- Context.Span for custom attributes (OpenTelemetry)
- Code examples:
  - Setting partition key from command
  - Adding custom headers via context
  - Accessing OriginatingMessage in handler
  - Using Context.Span for custom OTel attributes
- Update existing context bag documentation
- Integration with CloudEvents headers

**Acceptance Criteria**:
- [ ] All new capabilities documented
- [ ] Code examples for each feature
- [ ] Integration points explained (Polly, OTel)
- [ ] OriginatingMessage use case clear
- [ ] Existing docs updated with new features

---

### TASK-017: Polly Resilience Pipeline Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Large
**Dependencies**: None
**Files**:
- Update `Docs/contents/PolicyRetryAndCircuitBreaker.md`
- Update `Docs/contents/PolicyFallback.md`
- Search and update all policy examples throughout docs

**Description**: Document Polly v8 resilience pipeline support

**Content Requirements**:
- Migration from [TimeoutPolicy] to [UseResiliencePipeline]
- Migration from [UsePolicy] to [UseResiliencePipeline]
- **Deprecation notice**: TimeoutPolicy deprecated in V10, removed in V11
- Polly v8 support overview
- Configuring resilience pipelines in DI
- Using pipeline name in attribute
- **CancellationToken integration**: Proper token flow from pipelines
- **Request context integration**: ResilienceContext property
- Full Polly v8 strategies available:
  - Retry
  - Circuit Breaker
  - Timeout
  - Rate Limiter
  - Fallback
  - Hedging
- Code examples with new attributes
- Code examples configuring pipelines
- Update all existing policy examples throughout docs
- Links to Polly v8 documentation
- Best practices

**Acceptance Criteria**:
- [ ] Migration guide clear
- [ ] New attribute usage documented
- [ ] Polly v8 integration explained
- [ ] All Polly strategies listed
- [ ] CancellationToken flow documented
- [ ] ResilienceContext integration shown
- [ ] All old policy examples updated
- [ ] Deprecation notice prominent
- [ ] Links to Polly documentation

---

### TASK-018: Simplified Configuration Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Medium
**Dependencies**: None
**Files**:
- Update `Docs/contents/BrighterBasicConfiguration.md`
- Update `Docs/contents/HowConfiguringTheCommandProcessorWorks.md`
- Update `Docs/contents/HowConfiguringTheDispatcherWorks.md`
- Search and update all configuration examples

**Description**: Document renamed configuration methods and use Dispatcher terminology

**Content Requirements**:
- **UseExternalBus() → AddProducers()** change
- **AddServiceActivator() → AddConsumers()** change
- Rationale: Simpler, clearer naming convention
- V10 configuration examples showing new methods
- Migration from V9 configuration
- Use **"Dispatcher"** terminology throughout (not "ServiceActivator")
- Clarify: ServiceActivator is the assembly name, Dispatcher is the concept/class
- Update all configuration examples throughout docs
- Code comparison (V9 vs V10)
- Links to producer/consumer configuration details

**Acceptance Criteria**:
- [ ] New method names explained
- [ ] Rationale provided
- [ ] "Dispatcher" terminology used consistently
- [ ] ServiceActivator only used for assembly references
- [ ] All configuration examples updated
- [ ] Migration guidance included
- [ ] No more references to old method names

---

### TASK-019: OpenTelemetry Integration Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Medium
**Dependencies**: TASK-004 (CloudEvents)
**File**: Update `Docs/contents/Telemetry.md`

**Description**: Document OpenTelemetry Semantic Conventions support

**Content Requirements**:
- **OTel Semantic Conventions** overview (link to spec)
- V10 change: Uses OTel conventions (different from V9 custom conventions)
- **Span attributes** for request handler pipelines
- **Transport tracing** with W3C TraceContext propagation
- **Outbox tracing**: Distributed tracing for outbox operations
- **Inbox tracing**: Distributed tracing for inbox operations
- **Claim Check tracing**: Tracing for large message pattern
- **Configurable instrumentation** options across operations
- Integration with CloudEvents (traceParent, traceState)
- Configuration examples
- Integration with tracing backends (Jaeger, Zipkin, etc.)
- Best practices
- Breaking change notice: Trace structure changed from V9

**Acceptance Criteria**:
- [ ] OTel Semantic Conventions explained
- [ ] All tracing features documented
- [ ] Configuration examples provided
- [ ] Breaking change from V9 prominent
- [ ] Backend integration examples
- [ ] CloudEvents integration explained
- [ ] W3C TraceContext propagation shown
- [ ] File updated with V10 changes

---

### TASK-020: Nullable Reference Types Documentation ⬜
**Priority**: MEDIUM
**Estimated Effort**: Large
**Dependencies**: None
**Files**: All files with code examples

**Description**: Update all code examples for nullable reference types

**Content Requirements**:
- Review all code examples in documentation
- Add null checks where appropriate
- Use nullable annotations correctly (?, null-forgiving operator)
- Ensure examples compile without warnings
- Consistent patterns:
  - Check for null before use
  - Use null-conditional operators (?.)
  - Use null-coalescing operators (??)
- Add notes about nullable reference types in migration guide
- Best practices for handling nullable in handlers

**Acceptance Criteria**:
- [ ] All code examples null-safe
- [ ] Proper nullable annotations used
- [ ] No nullable warnings in examples
- [ ] Consistent nullable patterns throughout
- [ ] Migration guide updated with nullable guidance

---

## Phase 5: Transport & Infrastructure (Lower Priority)

### TASK-021: PostgreSQL Message Broker Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: None
**File**: `Docs/contents/PostgreSQLMessageBroker.md` (NEW)

**Description**: Document PostgreSQL pub/sub messaging

**Content Requirements**:
- PostgreSQL LISTEN/NOTIFY overview
- Benefits: Use existing PostgreSQL infrastructure
- When to use: Microservices with shared PostgreSQL
- PostgreSQL as message broker configuration
- Subscription setup
- Publication setup
- Limitations:
  - Message size limits
  - Not suitable for high-volume scenarios
  - No persistence between disconnects
- Comparison with other transports
- Code examples
- Best practices
- References to existing PostgreSQL Outbox/Inbox docs

**Acceptance Criteria**:
- [ ] Feature explained clearly
- [ ] Use cases documented
- [ ] Configuration examples provided
- [ ] Limitations prominent
- [ ] Comparison with other transports
- [ ] Links to Outbox/Inbox docs
- [ ] Added to SUMMARY.md under transport section

---

### TASK-022: RabbitMQ Enhancements Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Small
**Dependencies**: None
**File**: Update `Docs/contents/RabbitMQConfiguration.md`

**Description**: Document RabbitMQ V10 enhancements

**Content Requirements**:
- **Quorum Queues**:
  - What are quorum queues
  - Benefits: Consistency, availability, replicated
  - Configuration requirements (isDurable: true, highAvailability: false)
  - When to use vs classic queues
  - Code example
- **RabbitMQ v7 client support**:
  - v6 client for Reactor pipelines
  - v7 client for Proactor pipelines
  - Both supported
- **Connection stability** improvements
- **Persistent messages** support
- **Blocked/unblocked channel events** logging
- Code examples for quorum queues
- Migration notes if applicable

**Acceptance Criteria**:
- [ ] Quorum queues explained
- [ ] Configuration requirements clear
- [ ] Use cases documented
- [ ] v6/v7 client support explained
- [ ] Connection improvements noted
- [ ] Code examples provided
- [ ] File updated with V10 changes

---

### TASK-023: Kafka Improvements Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Small
**Dependencies**: None
**File**: Update `Docs/contents/KafkaConfiguration.md`

**Description**: Document Kafka configuration improvements

**Content Requirements**:
- **Configuration callback** support in KafkaSubscription
- Allows fine-grained configuration of Kafka consumer
- **Updated default values** for better out-of-box experience
- List of default changes (if any behavioral changes)
- Benefits of new defaults
- Code examples using configuration callback
- Code examples showing new defaults
- Migration notes if defaults changed behavior

**Acceptance Criteria**:
- [ ] Callback configuration explained
- [ ] New defaults documented
- [ ] Code examples provided
- [ ] Benefits explained
- [ ] Migration notes if applicable
- [ ] File updated with V10 changes

---

### TASK-024: AWS SDK v4 Support Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: None
**Files**:
- Update `Docs/contents/AWSSQSConfiguration.md`
- Update `Docs/contents/DynamoOutbox.md`
- Update `Docs/contents/DynamoInbox.md`
- `Docs/contents/S3LuggageStore.md` (NEW)

**Description**: Document AWS SDK v4 support

**Content Requirements**:
- AWS SDK v4 support overview
- **SNS/SQS** with SDK v4:
  - Standard queues
  - FIFO queues
  - SQS direct publishing without SNS
- **DynamoDB** Inbox/Outbox with SDK v4
- **S3 Luggage store** for Claim Check pattern:
  - Configuration
  - Usage with ClaimCheck attribute
  - Code examples
- **Backwards compatibility** with SDK v3
- Migration guidance (v3 to v4)
- Configuration examples for all services
- Best practices

**Acceptance Criteria**:
- [ ] SDK v4 support documented
- [ ] All AWS services covered (SNS, SQS, DynamoDB, S3)
- [ ] SQS direct publishing explained
- [ ] S3 Luggage store documented
- [ ] Backwards compatibility noted
- [ ] Migration guidance provided
- [ ] All AWS docs updated
- [ ] Code examples for all services

---

### TASK-025: Sweeper Circuit Breaking Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: None
**File**: `Docs/contents/SweeperCircuitBreaking.md` (NEW)

**Description**: Document topic-level circuit breaking

**Content Requirements**:
- What is sweeper circuit breaking
- Purpose: Prevent cascade failures when external systems fail
- How it prevents cascade failures
- **Failure tracking** per topic
- **Configurable thresholds**: Failure count before opening
- **Cooldown periods**: Time before attempting recovery
- **Automatic recovery**: Testing after cooldown
- **Bulk dispatch support**: Circuit breaking in batch operations
- MongoDB Outbox integration
- Per-transport integration notes
- Configuration examples
- Monitoring circuit breaker state
- Metrics and observability
- Best practices

**Acceptance Criteria**:
- [ ] Feature explained clearly
- [ ] Use cases documented (cascade failure prevention)
- [ ] Configuration examples provided
- [ ] Thresholds and cooldown explained
- [ ] Bulk dispatch support noted
- [ ] Monitoring guidance included
- [ ] Integration with transports explained
- [ ] Added to SUMMARY.md under "Advanced" section

---

### TASK-026: InMemory Options Overview Documentation ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: TASK-010
**File**: `Docs/contents/InMemoryOptions.md` (NEW)

**Description**: Document all InMemory implementations

**Content Requirements**:
- Overview of InMemory options philosophy
- Purpose: Testing, development, demos, simple scenarios
- **InMemory Scheduler** (link to TASK-010)
- **InMemory Producers**
- **InMemory Consumers**
- **InMemory Outbox**
- **InMemory Inbox**
- **When to use**: Testing, development, demos
- **Production limitations**: No durability - crashes lose data
- **Acceptable production scenarios**: Where data loss is acceptable
- Configuration examples for each
- Testing scenarios and best practices
- Example test setup using all InMemory options
- WebAPI sample reference for simple setup

**Acceptance Criteria**:
- [ ] All InMemory options covered
- [ ] Philosophy explained
- [ ] Use cases clear (testing, dev, demos)
- [ ] Limitations documented
- [ ] Acceptable production scenarios noted
- [ ] Configuration examples for each
- [ ] Testing guidance included
- [ ] Reference to samples
- [ ] Added to SUMMARY.md

---

## Phase 6: Glossary & Reference (Lower Priority)

### TASK-027: Create Terminology Glossary ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: All documentation tasks
**File**: `Docs/contents/Glossary.md` (NEW)

**Description**: Create comprehensive terminology glossary

**Content Requirements**:
- **Core Concepts**:
  - Request (base type for Command, Event, Query)
  - Command (instruction to change state)
  - Event (notification of state change)
  - Query (request for data, no state change)
- **Processors**:
  - Command Processor (sends/publishes commands and events)
  - Query Processor (executes queries)
- **Brighter/Darker**:
  - Brighter (Commands and Events)
  - Darker (Queries)
- **Dispatcher**:
  - **Dispatcher** (the class that dispatches to handlers, in ServiceActivator assembly)
  - ServiceActivator (the assembly name, not the concept)
  - Consumer (listens to external messages)
- **Handler & Pipeline**:
  - Handler (processes a request)
  - Request Handler (IRequestHandler)
  - Pipeline (chain of handlers with middleware)
  - Middleware (handler that wraps other handlers)
- **Patterns**:
  - Outbox (transactional messaging pattern)
  - Inbox (deduplication pattern)
  - Sweeper (background process for outbox)
  - Claim Check (large message pattern)
- **Messaging**:
  - Producer (sends messages)
  - Consumer (receives messages)
  - Subscription (consumer configuration)
  - Publication (producer configuration)
  - Message Mapper (converts Request to Message)
  - Transform (message transformation in pipeline)
- **Routing**:
  - Routing Key (message destination)
  - Topic (message category)
  - CloudEvents Type (message type in CloudEvents)
  - Agreement Dispatcher (dynamic handler selection)
- **Concurrency**:
  - Reactor (blocking I/O, faster performance)
  - Proactor (non-blocking I/O, better throughput)
  - Performer (message pump, single-threaded)
- **Resilience**:
  - Resilience Pipeline (Polly v8 policies)
  - Circuit Breaker
  - Retry
  - Fallback
  - Timeout
- Links to detailed documentation for each term
- Comparison with industry-standard terms (EIP patterns)
- Cross-references between related terms

**Acceptance Criteria**:
- [ ] All key terms defined
- [ ] Clear, concise definitions
- [ ] Links to detailed docs
- [ ] Industry term mappings (EIP)
- [ ] Cross-references included
- [ ] Dispatcher vs ServiceActivator clear
- [ ] Reactor vs Proactor clear
- [ ] Added to SUMMARY.md

---

### TASK-028: Update FAQ ⬜
**Priority**: LOW
**Estimated Effort**: Medium
**Dependencies**: All other tasks
**File**: Update `Docs/contents/FAQ.md`

**Description**: Update FAQ with V10 information

**Content Requirements**:
- Add V10-specific questions:
  - When should I use Reactor vs Proactor?
  - Do I need message mappers in V10?
  - How do I migrate from V9 to V10?
  - When should I use Agreement Dispatcher?
  - What scheduler should I use in production?
  - Can I use InMemory options in production?
  - How do I use CloudEvents?
  - What changed with OpenTelemetry in V10?
- Update existing answers with V10 changes
- Add common migration questions
- Address user feedback: "hard to get started"
- Emphasize: Start simple, add complexity later
- Organize by category:
  - Getting Started
  - Configuration
  - Messaging
  - Handlers & Pipelines
  - Resilience & Policies
  - Scheduling
  - Migration
- Link to relevant documentation
- Reference WebAPI sample for fully-featured example

**Acceptance Criteria**:
- [ ] V10 questions added
- [ ] Existing answers updated
- [ ] Migration questions included
- [ ] "Getting started" questions prominent
- [ ] Well organized by category
- [ ] Links to detailed docs
- [ ] Sample references included
- [ ] File updated

---

## Task Execution Instructions

### For User
To request execution of a specific task:
1. **By number**: "Please complete TASK-001"
2. **By name**: "Please work on the Migration Guide"
3. **By phase**: "Please complete Phase 1 tasks"
4. **By priority**: "Please complete all HIGH priority tasks"

### For Claude
When executing a task:
1. Update task status to 🔄 In Progress
2. Create or update the specified file(s)
3. Follow all content requirements
4. Reference user answers from REQUIREMENTS.md
5. Include code examples from release notes and ADRs
6. Ensure all acceptance criteria are met
7. Update SUMMARY.md if required
8. Update task status to ✅ Completed
9. Report completion with file paths and summary

### Cross-references Between Tasks
- Many tasks reference each other - include links in documentation
- CloudEvents (TASK-004) is referenced by Default Mappers (TASK-005), Dynamic Deserialization (TASK-006), OTel (TASK-019)
- Schedulers (TASK-010-015) depend on Scheduler Overview (TASK-009)
- InMemory Overview (TASK-026) depends on InMemory Scheduler (TASK-010)

## Summary

- **Total Tasks**: 28
- **Critical**: 1 (TASK-001)
- **High Priority**: 8 (TASK-002 to TASK-009)
- **Medium Priority**: 11 (TASK-010 to TASK-020)
- **Low Priority**: 8 (TASK-021 to TASK-028)
- **No Blocked Tasks**: All questions answered ✅

## Recommended Execution Order

### Week 1: Foundation (Critical)
1. **TASK-001**: Migration Guide (CRITICAL)
2. **TASK-002**: Update SUMMARY.md
3. **TASK-003**: Simplify Show Me The Code

### Week 2: Core Features Part 1 (High Priority)
4. **TASK-004**: Cloud Events Support
5. **TASK-005**: Default Message Mappers
6. **TASK-006**: Dynamic Message Deserialization
7. **TASK-007**: Agreement Dispatcher

### Week 3: Core Features Part 2 & Schedulers (High/Medium Priority)
8. **TASK-008**: Reactor and Proactor
9. **TASK-009**: Scheduled Requests Overview
10. **TASK-010**: InMemory Scheduler
11. **TASK-011**: Quartz Scheduler
12. **TASK-012**: Hangfire Scheduler

### Week 4: Cloud Schedulers & Breaking Changes (Medium Priority)
13. **TASK-013**: AWS Scheduler
14. **TASK-014**: Azure Scheduler
15. **TASK-015**: Custom Scheduler
16. **TASK-016**: Request Context Improvements
17. **TASK-017**: Polly Resilience Pipeline

### Week 5: Configuration & Telemetry (Medium Priority)
18. **TASK-018**: Simplified Configuration
19. **TASK-019**: OpenTelemetry Integration
20. **TASK-020**: Nullable Reference Types (ongoing)

### Week 6: Transports (Low Priority)
21. **TASK-021**: PostgreSQL Message Broker
22. **TASK-022**: RabbitMQ Enhancements
23. **TASK-023**: Kafka Improvements
24. **TASK-024**: AWS SDK v4 Support

### Week 7: Advanced & Reference (Low Priority)
25. **TASK-025**: Sweeper Circuit Breaking
26. **TASK-026**: InMemory Options Overview
27. **TASK-027**: Terminology Glossary
28. **TASK-028**: Update FAQ

## Integration with Brighter Samples

Reference these samples throughout documentation:
- **WebAPI Sample**: Fully-featured example (reference in Show Me The Code)
- **Transforms Sample**: ClaimCheck example (reference in Default Message Mappers)
- Other samples in `Brighter/samples/` for specific features

## Key Themes for All Documentation

Based on user feedback, emphasize:
1. **Start Simple**: Show basic usage first
2. **Add Complexity Gradually**: Link to advanced features
3. **Don't Over-Engineer**: Warn against premature abstraction (e.g., custom handler base classes)
4. **Use Defaults**: V10 makes things simpler with defaults
5. **Reference Samples**: Point to working code in samples directory
