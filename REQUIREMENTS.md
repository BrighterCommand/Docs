# Brighter V10 Documentation Requirements

## Overview

This document outlines the requirements for updating and improving the Brighter/Darker documentation to reflect the V10 release and address user feedback about clarity and accessibility.

## Goals

1. **Document all V10 new features** - Ensure complete coverage of new capabilities
2. **Improve accessibility for newcomers** - Make it easier to get started with Brighter
3. **Maintain depth for advanced users** - Provide comprehensive information for power users
4. **Improve clarity and terminology** - Address feedback about confusing terms and examples

## V10 New Features to Document

### High Priority Features (Core Framework Changes)

#### 1. Cloud Events Support
- **Status**: NEW - No existing documentation
- **Scope**: Full CloudEvents specification support
- **Content Needed**:
  - What CloudEvents are and why they matter
  - Binary vs. Structured CloudEvents mode
  - How to set CloudEvents properties in Publication
  - How CloudEvents headers work across transports
  - Migration from V9 custom headers to CloudEvents
  - Code examples for both modes

#### 2. Default Message Mappers
- **Status**: NEW - No existing documentation
- **Scope**: Eliminate need for explicit IAmAMessageMapper implementation
- **Content Needed**:
  - Explain the default JsonMessageMapper
  - Explain CloudEventJsonMessageMapper
  - When you still need custom mappers
  - How to configure your own default mapper
  - Migration guide from explicit mappers
  - Code examples

#### 3. Dynamic Message Deserialization
- **Status**: NEW - No existing documentation
- **Scope**: Content-based routing via getRequestType callback
- **Content Needed**:
  - Explain DataType Channel vs. content-based routing
  - When to use dynamic deserialization
  - How to implement getRequestType callback
  - Using CloudEvents type for routing
  - Code examples with multiple message types on one channel

#### 4. Agreement Dispatcher
- **Status**: NEW - No existing documentation
- **Scope**: Dynamic handler resolution at runtime
- **Content Needed**:
  - Explain Agreement Dispatcher pattern (reference Martin Fowler)
  - Use cases (time-based rules, content-based routing)
  - How to register agreement dispatcher routes
  - Cannot use AutoFromAssemblies with this feature
  - Code examples with multiple handlers
  - Difference from standard 1-to-1 handler mapping

#### 5. Reactor and Proactor (Terminology Change)
- **Status**: UPDATE EXISTING - Replace blocking/non-blocking terminology
- **Scope**: Rename and clarify concurrency models
- **Content Needed**:
  - Explain Reactor pattern (blocking I/O, single-threaded)
  - Explain Proactor pattern (non-blocking I/O, async)
  - When to use each model
  - Performance vs. throughput trade-offs
  - Update all references from "blocking/non-blocking" to "Reactor/Proactor"
  - Update MessagePumpType configuration examples
  - Migration guide from isAsync/runAsync to MessagePumpType

#### 6. Scheduled Requests/Messaging
- **Status**: NEW - Basic documentation exists for some schedulers
- **Scope**: Integration with schedulers for delayed sending
- **Content Needed**:
  - Overview of scheduling support
  - Using Send/Publish/Post with DateTimeOffset
  - In-Memory Scheduler (for testing/dev)
  - Quartz.NET integration
  - Hangfire integration
  - AWS Scheduler integration
  - Azure Scheduler integration
  - Custom scheduler implementation
  - Using "Requeue with Delay" where transport doesn't support it
  - Code examples for each scheduler type

### Medium Priority Features (Breaking Changes & Improvements)

#### 7. Request Context Improvements
- **Status**: UPDATE EXISTING
- **Scope**: New capabilities in RequestContext
- **Content Needed**:
  - Setting RequestContext explicitly in Send/Publish/DepositPost
  - OriginatingMessage property for consumers
  - PartitionKey support
  - Custom headers via context
  - Resilience Context integration
  - Code examples

#### 8. Polly Resilience Pipeline
- **Status**: UPDATE EXISTING
- **Scope**: Replace legacy TimeoutPolicy with UseResiliencePipeline
- **Content Needed**:
  - Migration from [TimeoutPolicy] to [UseResiliencePipeline]
  - Migration from [UsePolicy] to [UseResiliencePipeline]
  - Full Polly v8 support
  - CancellationToken integration
  - Request context integration with Polly's resilience context
  - Code examples
  - Update all existing policy documentation

#### 9. Simplified Configuration
- **Status**: UPDATE EXISTING
- **Scope**: Renamed builder methods
- **Content Needed**:
  - Update UseExternalBus → AddProducers
  - Update AddServiceActivator → AddConsumers
  - Explain rationale for simpler naming
  - Update all configuration examples throughout docs
  - Migration guide

#### 10. OpenTelemetry Integration
- **Status**: UPDATE EXISTING - May have some documentation
- **Scope**: Full OTel Semantic Conventions support
- **Content Needed**:
  - Span attributes for request handler pipelines
  - Transport tracing with W3C TraceContext
  - Outbox tracing
  - Inbox tracing
  - Claim Check tracing
  - Configurable instrumentation options
  - Breaking change: Different traces from V9
  - Code examples for configuration

### Lower Priority Features (Specific Implementations)

#### 11. InMemory Options
- **Status**: NEW
- **Scope**: In-memory implementations for testing/dev
- **Content Needed**:
  - InMemory Scheduler
  - InMemory Producers
  - InMemory Consumers
  - InMemory Outbox
  - InMemory Inbox
  - When to use (testing/dev, not production)
  - Code examples

#### 12. Transport Improvements
- **Status**: UPDATE/NEW
- **Scope**: Various transport enhancements
- **Content Needed**:
  - **PostgreSQL Message Broker** (NEW)
    - LISTEN/NOTIFY functionality
    - Configuration
    - Use cases
  - **RabbitMQ Quorum Queues** (NEW)
    - What are quorum queues
    - Configuration requirements
    - When to use vs. classic queues
  - **RabbitMQ v7 support** (UPDATE)
    - v6 for synchronous, v7 for async
  - **Kafka improvements** (UPDATE)
    - Configuration callbacks
    - Updated defaults
  - **AWS improvements** (UPDATE)
    - SQS direct publishing without SNS
    - S3 Claim-Check fixes
  - **AWS SDK v4** (UPDATE)
    - SNS/SQS support
    - DynamoDB Inbox/Outbox
    - S3 Luggage store

#### 13. Sweeper Circuit Breaking
- **Status**: NEW
- **Scope**: Topic-level circuit breaking
- **Content Needed**:
  - What is sweeper circuit breaking
  - Failure tracking per topic
  - Configurable thresholds
  - Automatic recovery
  - Bulk dispatch support
  - Code examples

#### 14. Nullable Reference Types
- **Status**: UPDATE EXISTING
- **Scope**: Breaking change - nullability enabled
- **Content Needed**:
  - What changed
  - How to handle nullable warnings
  - Migration guidance
  - Update all code examples to be null-safe

## Documentation Improvements (Non-Feature)

### 1. Show Me the Code Simplification
- **Current Issue**: Example uses complex Post with transactions and Outbox
- **Proposed Change**:
  - Show simpler example first (basic Send without transaction)
  - Note that it uses InMemory Outbox
  - Link to more detailed Outbox documentation for full example
  - Keep complex example but move it to Outbox documentation
- **Rationale**: Easier for newcomers to understand basics before complexity

### 2. Terminology Standardization
- **Current Issue**: "ServiceActivator" is confusing
- **Proposed Change**:
  - Evaluate if "Dispatcher" is better understood
  - Update terminology consistently throughout docs
  - Provide glossary of terms
  - Explain relationship between terms (e.g., Dispatcher runs ServiceActivator)
- **Rationale**: Industry-standard terms are more accessible

### 3. Migration Guide
- **Status**: Create comprehensive V9 to V10 migration guide
- **Content Needed**:
  - Summary of all breaking changes
  - Step-by-step migration instructions
  - Code comparison (V9 vs V10)
  - Common issues and solutions
  - Database schema updates

### 4. Updated SUMMARY.md
- **Status**: Add all new documentation pages
- **Content Needed**:
  - Proper organization of new topics
  - Logical grouping of related features
  - Clear navigation structure

## Questions for User

Before proceeding with creating documentation tasks, I need clarification on several features:

### Cloud Events
1. Are there specific CloudEvents properties that are most commonly used that we should emphasize?
2. Should we recommend binary or structured mode by default, or is it truly transport-dependent?

### Default Message Mappers
3. When would someone still need a custom mapper? What are the key use cases beyond simple JSON serialization?
4. Can you provide an example of a "complex transform pipeline" that requires custom mappers?

### Agreement Dispatcher
5. What are the most common real-world use cases for Agreement Dispatcher beyond time-based rules?
6. Are there performance implications when using Agreement Dispatcher vs. standard routing?

### Scheduled Requests
7. For the "Requeue with Delay" feature - which transports support native delay and which require a scheduler?
8. What are the recommended schedulers for production use?

### Reactor vs. Proactor
9. For the terminology change - do we need to update "ServiceActivator" terminology at the same time?
10. Should we provide performance benchmarks showing the trade-offs between Reactor and Proactor?

### InMemory Options
11. You mention InMemory options are "robust" but not for production - what are the limitations that make them unsuitable for production?
12. Are there scenarios where InMemory options might be acceptable in production (e.g., low-volume internal tools)?

### General Documentation
13. Should we maintain separate documentation for Brighter and Darker, or integrate them more closely?
14. Are there specific examples or sample applications that we should reference throughout the documentation?
15. Do you have any existing user feedback or common questions that should inform the documentation?

## Answers From User
### Cloud Events
1. We should emphasize the mandatory Cloud Events attributes (id, source, type) along with (datacontenttype), though we support all the attributes
2. We should recommend binary where the underlying protocol supports headers, and structured where it does not (or has insufficient header properties such as SNS/SQS)

## Default Message Mappers
3. You would tend to need your own message mapper if you wanted your default to be something other than JSON, for example Avro or ProtoBuf. You would need a custom message mapper if you wanted to use transforms in the pipeline (which support message transformation such as claim check, compression, encryption, PII, etc)
4. An example of a transform can be found in `Brighter/samples/Transforms/AWSTransfomers/ClaimCheck/Greetings/Ports/Mappers/GreetingEventMessageMapper.cs`

```csharp
   public class GreetingEventMessageMapper : IAmAMessageMapper<GreetingEvent>
    {
        public IRequestContext Context { get; set; }

        [ClaimCheck(step:0, thresholdInKb: 256)]
        public Message MapToMessage(GreetingEvent request, Publication publication)
        {
            var header = new MessageHeader(messageId: request.Id, topic: publication.Topic, messageType: MessageType.MT_EVENT);
            var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
            var message = new Message(header, body);
            return message;
        }

        [RetrieveClaim(0)]
        public GreetingEvent MapToRequest(Message message)
        {
            var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
            
            return greetingCommand;
        }
    }
```
Where the use of the `ClaimCheck` attribute deals with large messages.

### Agreement Dispatcher
5. The most common use cases for an agreement dispatcher, other than time, are different routes depending on the state of the request such as order journeys depending on the contents of an order, country-specific business logic, or versioning issues.
6. There are minor performance differences as there is both a lookup and execution of a lambda. The biggest difference to call out, is that registration via the  `AutoFromAssemblies()` method of `ServiceCollectionBrighterBuilder` does not you providing a lambda function for routing, instead you must manually register these routes via the `Handlers` method of `ServiceCollectionBrighterBuilder` instead.

### Scheduled Requests
7. Only RabbitMQ supports a native "Requeue with Delay" other transports require a scheduler?
8. We recommend any of Quartz or Hangfire for production, and the native cloud provider schedulers (AWSScheduler or AzureServiceBusScheduler) if in those cloud providers

### Reactor vs. Proactor
9. In the Docs we should use `Dispatcher` over `ServiceActivator` as this is the driving class in the `ServiceActivator` assembly. We should maintain `ServiceActivator` when clearly talking about the assembly not the concept.
10. No, we should explain that the `Proactor` has better throughput, because it yields the thread whilst doing I/O and that the `Reactor` has faster performance because it does not context switch. We should point out that our `Performer` - the message pump - is single-threaded in either case, by default, unless you do ConfigureAwait(false) on code in your handler in which case you will use the thread pool and ignore our synchronization context. (see https://devblogs.microsoft.com/dotnet/configureawait-faq/)

### InMemory Options
11. The In-Memory tools are generally not acceptable in production because they don't provide durable execution i.e. if your app crashes, anything in an in-memory store at the time will be lost.
12. They are acceptable in production where you don't care about loss of working state in those stores when you crash.

### General Documentation
13. Brighter and Darker documentation should be integrated in Docs where it provides clarity
14. The Brighter samples directory provides a rich set of examples. The WebAPI sample if fully featured, but other samples provide simpler examples
15. Most feedback is that it is hard to "get started" for someone who has never used Brighter before. We often see over-complicated strategies to using Brighter when reviewing new users code, that has hit problems. Typically they try to derive their own handler or request types to add local features. We would recommend starting with the basic features and adding your own types in our hierarchy once you have the basic flow working. 

## Next Steps

After receiving answers to these questions:
1. Create detailed documentation tasks
2. Prioritize tasks
3. Begin systematic execution of documentation updates
4. Review and iterate based on feedback
