# Glossary

This glossary provides definitions for key terms used in Brighter and Darker. Terms are organized by category for easy navigation.

## Core Concepts

### Request

A message sent over a bus. The base type for Commands, Events, and Queries. A Request represents an instruction or notification that needs to be processed by a handler.

See: [Implementing Command, Events and Queries](/contents/ImplementingCommand.md)

### Command

An instruction to execute some behavior that may update state. Commands are imperative - they tell the system to do something. A command should have one handler.

Example: `AddGreeting`, `DeletePerson`, `UpdateOrder`

See: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

### Event

A notification that something has happened. Events are facts about the past. Multiple handlers can subscribe to an event. Events do not update state directly - they inform interested parties about state changes that have already occurred.

Example: `GreetingMade`, `PersonDeleted`, `OrderPlaced`

See: [Publishing Events](/contents/DistributedTaskQueue.md)

### Query

A request for data that does not update state. Queries are processed by the Query Processor (Darker). A query returns a Result. Queries implement `IQuery<TResult>` where `TResult` is the type of data returned.

Example: `GetPersonNameQuery`, `GetOrderDetailsQuery`, `SearchProductsQuery`

See: [Queries and Query Objects](/contents/QueriesAndQueryObjects.md), [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)

### Query Object

A pattern where query parameters are encapsulated in an object that implements `IQuery<TResult>`. The Query Object pattern separates the definition of a query from its execution, enabling middleware pipelines and testability.

See: [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)

### Query Handler

A class that executes a query and returns results. Query handlers contain the logic for retrieving and projecting data. Handlers derive from `QueryHandler<TQuery, TResult>` (synchronous) or `QueryHandlerAsync<TQuery, TResult>` (asynchronous).

See: [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)

### Query Pipeline

A chain of decorators (middleware) that wrap a query handler to provide cross-cutting concerns like logging, retry, circuit breakers, and fallback policies. Similar to Brighter's request pipeline but optimized for read operations.

See: [Query Pipeline and Decorators](/contents/QueryPipeline.md)

### Query Decorator

A middleware component that wraps query handlers to add cross-cutting functionality. Common decorators include `QueryLogging`, `RetryableQuery`, and `FallbackPolicy`.

See: [Query Pipeline and Decorators](/contents/QueryPipeline.md)

## Processors

### Command Processor

The component that sends and publishes Commands and Events. The Command Processor provides middleware functionality like retry, logging, and timeouts through a pipeline. It separates the sender from the receiver.

Interface: `IAmACommandProcessor`

See: [How the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md)

### Query Processor

The component that executes Queries and returns Results. The Query Processor provides middleware functionality similar to the Command Processor, including logging, retry, and fallback policies. Part of Darker. The Query Processor dispatches queries to query handlers through a pipeline of decorators.

Interface: `IQueryProcessor`

See: [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md), [Query Pipeline and Decorators](/contents/QueryPipeline.md)

## Brighter and Darker

### Brighter

The framework for Commands and Events - messages that may update state. Brighter provides both in-process and out-of-process messaging capabilities.

See: [Show me the code!](/contents/ShowMeTheCode.md)

### Darker

The framework for Queries - messages that return data without updating state. Darker follows the same patterns as Brighter (handlers, pipelines, policies) but is optimized for read operations. Darker implements the Query Object pattern and provides decorators for logging, retry, circuit breakers, and fallback.

See: [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md), [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)

## Dispatcher

### Dispatcher

The component that listens for messages from external middleware (like RabbitMQ or Kafka) and dispatches them to handlers. The Dispatcher is the runtime component that processes messages from a queue or topic.

**Note**: The assembly name is `Paramore.Brighter.ServiceActivator`, but the concept and class are referred to as "Dispatcher" throughout the documentation.

See: [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md)

### Consumer

A Dispatcher that listens to external messages from middleware and forwards them to handlers. Configured using subscriptions.

See: [Configuring the Dispatcher](/contents/BrighterBasicConfiguration.md)

### ServiceActivator

The assembly name for the Dispatcher component (`Paramore.Brighter.ServiceActivator`). When referring to the concept or runtime behavior, use "Dispatcher" instead.

## Handler & Pipeline

### Handler

A class that processes a Request (Command, Event, or Query). Handlers contain the business logic for responding to requests.

Interface: `IHandleRequests<T>` (synchronous) or `IHandleRequestsAsync<T>` (asynchronous)

See: [Implementing a Handler](/contents/ImplementingAHandler.md)

### Request Handler

The specific interface for handling requests. Handlers derive from `RequestHandler<T>` or `RequestHandlerAsync<T>`.

See: [Request Handlers and Pipelines](/contents/BuildingAPipeline.md)

### Pipeline

A chain of handlers with middleware that processes a request. The pipeline allows you to compose cross-cutting concerns (logging, retry, validation) around your business logic.

See: [Building a Pipeline](/contents/BuildingAPipeline.md)

### Middleware

A handler that wraps other handlers to provide cross-cutting functionality. Middleware handlers run before and/or after the main handler. Common middleware includes logging, retry, timeout, and validation.

Examples: `RequestLoggingAsync`, `UseResiliencePipeline`, `UseInboxAsync`

See: [Building a Pipeline](/contents/BuildingAPipeline.md)

## Patterns

### Outbox

A pattern for ensuring that messages are sent reliably. The Outbox writes messages to a database in the same transaction as entity changes, guaranteeing that both succeed or fail together. Messages are then dispatched from the Outbox to the message broker.

See: [Outbox Pattern](/contents/OutboxPattern.md), [Outbox Support](/contents/BrighterOutboxSupport.md)

### Inbox

A pattern for deduplication - ensuring that a message is only processed once. The Inbox tracks which messages have been processed and prevents duplicate processing of the same message.

See: [Inbox Configuration](/contents/InboxConfiguration.md)

### Sweeper

A background process that monitors an Outbox and dispatches messages that have not yet been sent. The Sweeper provides guaranteed, at-least-once delivery by continuously attempting to send messages until they succeed.

See: [Outbox Support](/contents/BrighterOutboxSupport.md#implicit-clear)

### Claim Check

A pattern for handling large messages. Instead of sending the entire message through the broker, the large payload is stored externally (e.g., in S3 or blob storage), and only a reference (claim check) is sent in the message. The receiver retrieves the payload using the claim check.

See: [Default Message Mappers](/contents/DefaultMessageMappers.md), [S3 Luggage Store](/contents/S3LuggageStore.md)

## Messaging

### Producer

A component that sends messages to external middleware. Configured using publications.

See: [Configuring Producers](/contents/BrighterBasicConfiguration.md)

### Subscription

Configuration for a Consumer that defines how to receive messages from a specific topic or queue. Includes message pump configuration, routing keys, and other transport-specific settings.

See: [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md), [Kafka Configuration](/contents/KafkaConfiguration.md)

### Publication

Configuration for a Producer that defines how to send messages to a specific topic or queue. Maps message types to routing keys and topics.

See: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

### Message Mapper

A component that converts between Requests (C# objects) and Messages (wire format). Message Mappers handle serialization, deserialization, and header mapping.

Interface: `IAmAMessageMapper<T>`

**Note**: In V10, explicit message mappers are often not needed - default JSON mappers are used automatically.

See: [Message Mappers](/contents/MessageMappers.md), [Default Message Mappers](/contents/DefaultMessageMappers.md)

### Transform

A message transformation applied in a pipeline. Transforms can compress, encrypt, or apply other transformations to messages. Used with attributes like `[ClaimCheck]`, `[Compression]`, `[Encryption]`.

See: [Default Message Mappers](/contents/DefaultMessageMappers.md)

### Message

The wire format representation of a Request. Contains headers (metadata) and body (payload). Messages are transported over middleware like RabbitMQ, Kafka, or AWS SQS.

Class: `Message`

See: [Message Mappers](/contents/MessageMappers.md)

## Routing

### Routing Key

The destination identifier for a message. In RabbitMQ, this is the routing key. In Kafka, this is the topic. The routing key determines where messages are sent.

See: [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)

### Topic

A message category or channel. In pub-sub systems, publishers send to topics, and subscribers listen to topics. In Kafka, topics are first-class constructs. In RabbitMQ, topics are implemented using exchanges and routing keys.

See: [Kafka Configuration](/contents/KafkaConfiguration.md)

### CloudEvents Type

The message type identifier in the CloudEvents specification. Used for message routing and type resolution in dynamic deserialization scenarios. Format: reverse DNS notation (e.g., `io.goparamore.task.created`).

See: [CloudEvents Support](/contents/CloudEventsSupport.md), [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)

### DataType Channel

The default routing pattern in Brighter - one message type per channel. Each channel (topic/queue) carries only one type of message, so the channel name determines the message type.

See: [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)

### Agreement Dispatcher

A pattern for dynamic handler selection based on request content or context. Instead of a 1-to-1 mapping between request type and handler, Agreement Dispatcher allows runtime selection of which handler(s) should process a request.

Named after Martin Fowler's [Agreement Dispatcher pattern](https://martinfowler.com/eaaDev/AgreementDispatcher.html).

See: [Agreement Dispatcher](/contents/AgreementDispatcher.md)

## Concurrency

### Reactor

A concurrency pattern using blocking I/O. The Reactor pattern processes messages synchronously, which provides faster performance per message (no context switch overhead) but may limit throughput under high load.

Configured with: `MessagePumpType.Reactor`

See: [Reactor and Proactor](/contents/ReactorAndProactor.md)

### Proactor

A concurrency pattern using non-blocking I/O. The Proactor pattern processes messages asynchronously with `ConfigureAwait(false)`, which provides better throughput (yields threads during I/O) but adds context switch overhead.

Configured with: `MessagePumpType.Proactor`

See: [Reactor and Proactor](/contents/ReactorAndProactor.md)

### Performer

The message pump - a single-threaded component that pulls messages from middleware and dispatches them to handlers. The Performer can operate in either Reactor or Proactor mode.

**Note**: In earlier versions, this was called "Message Pump". Current terminology uses "Performer" for the single-threaded pump.

See: [Reactor and Proactor](/contents/ReactorAndProactor.md)

### Message Pump

The component that retrieves messages from a transport and dispatches them to handlers. Runs on a single thread (the Performer). Can operate in Reactor mode (blocking) or Proactor mode (non-blocking).

See: [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md)

## Resilience

### Resilience Pipeline

A Polly v8 pipeline that provides resilience strategies like retry, circuit breaker, timeout, rate limiter, fallback, and hedging. Resilience pipelines replace the legacy `UsePolicy` attributes in V10.

Configured with: `[UseResiliencePipeline]` attribute

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md)

### Circuit Breaker

A resilience pattern that prevents cascading failures. When a certain threshold of failures is reached, the circuit breaker "opens" and stops sending requests to the failing service. After a cooldown period, it enters a "half-open" state to test if the service has recovered.

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md)

### Retry

A resilience pattern that automatically retries failed operations. Retry policies can use fixed delays, exponential backoff, or jitter to space out retry attempts.

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md)

### Fallback

A resilience pattern that provides alternative behavior when an operation fails. Fallback can return default values, compensate for failures, queue work for later, or raise alerts.

See: [Fallback Policy](/contents/PolicyFallback.md)

### Timeout

A resilience pattern that limits how long an operation can run. Timeouts prevent operations from hanging indefinitely and consuming resources.

**Note**: `TimeoutPolicy` is deprecated in V10. Use Resilience Pipelines with timeout strategy instead.

See: [Resilience Pipelines](/contents/PolicyRetryAndCircuitBreaker.md)

### Sweeper Circuit Breaking

A pattern for preventing failures when posting to one topic from blocking posting to other topics. The sweeper tracks failures per topic and "trips" a circuit breaker for problematic topics, allowing healthy topics to continue operating.

See: [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)

## CloudEvents

### CloudEvents

A CNCF specification for describing event data in a common format. CloudEvents provides interoperability between different messaging systems and frameworks.

Specification: [CloudEvents v1.0.2](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/spec.md)

See: [CloudEvents Support](/contents/CloudEventsSupport.md)

### Binary-mode CloudEvents

A CloudEvents encoding where attributes are stored in protocol headers and the event data is the message body. Recommended when the protocol supports rich headers (e.g., AMQP, Kafka, RabbitMQ).

See: [CloudEvents Support](/contents/CloudEventsSupport.md)

### Structured-mode CloudEvents

A CloudEvents encoding where the entire event (attributes + data) is stored in the message body as JSON. Recommended when the protocol has limited header support (e.g., AWS SNS/SQS).

See: [CloudEvents Support](/contents/CloudEventsSupport.md)

## Context

### Request Context

An object that carries contextual information through the handler pipeline. Provides access to headers, partition keys, telemetry spans, resilience context, and the originating message.

Interface: `IRequestContext`

See: [Using the Context Bag](/contents/UsingTheContextBag.md), [Request Context Improvements](/contents/DispatchingARequest.md)

### Context Bag

A key-value store within the Request Context for passing custom data between handlers in a pipeline. Use well-known keys from `RequestContextBagNames` for standard values.

See: [Using the Context Bag](/contents/UsingTheContextBag.md)

### Originating Message

The original wire-format Message that triggered a handler (available in Consumers). Accessible via `Context.OriginatingMessage`. Useful for debugging, auditing, and accessing transport-specific metadata.

See: [Request Context Improvements](/contents/DispatchingARequest.md)

### Partition Key

A value used to determine which partition a message should be routed to in partitioned systems like Kafka. Ensures messages with the same key go to the same partition, maintaining order.

See: [Request Context Improvements](/contents/DispatchingARequest.md)

## Scheduling

### Scheduler

A component that handles delayed message delivery. Schedulers allow you to send messages at a future time or after a delay.

See: [Scheduler Support](/contents/BrighterSchedulerSupport.md)

### InMemory Scheduler

A timer-based scheduler that stores scheduled work in memory. Suitable for development and testing, but not durable - crashes lose scheduled messages.

See: [InMemory Scheduler](/contents/InMemoryScheduler.md)

### Quartz Scheduler

A production scheduler using [Quartz.NET](https://www.quartz-scheduler.net/). Provides persistent, distributed scheduling with clustering support.

See: [Quartz Scheduler](/contents/QuartzScheduler.md)

### Hangfire Scheduler

A production scheduler using [Hangfire](https://www.hangfire.io/). Provides persistent scheduling with a web dashboard for monitoring.

See: [Hangfire Scheduler](/contents/HangfireScheduler.md)

### AWS Scheduler

A serverless scheduler using [AWS EventBridge Scheduler](https://docs.aws.amazon.com/eventbridge/latest/userguide/using-eventbridge-scheduler.html). Cloud-native option for AWS deployments.

See: [AWS Scheduler](/contents/AwsScheduler.md)

### Azure Scheduler

A scheduler using Azure Service Bus native scheduling features. Cloud-native option for Azure deployments. Does not support rescheduling - must cancel and create a new schedule.

See: [Azure Scheduler](/contents/AzureScheduler.md)

## Telemetry

### OpenTelemetry (OTel)

An observability framework for collecting traces, metrics, and logs. Brighter V10 uses OpenTelemetry Semantic Conventions for messaging.

See: [Telemetry](/contents/Telemetry.md)

### Activity

The .NET implementation of an OpenTelemetry Span. Represents a unit of work in a distributed trace.

See: [Telemetry](/contents/Telemetry.md)

### Span

A unit of work in a distributed trace. Spans have a start time, duration, and attributes. Spans can be nested to form a trace hierarchy.

See: [Telemetry](/contents/Telemetry.md)

### W3C TraceContext

A W3C standard for propagating trace context between services. Uses `traceparent` and `tracestate` headers.

Specification: [W3C Trace Context](https://www.w3.org/TR/trace-context/)

See: [Telemetry](/contents/Telemetry.md)

## Transports

### Transport

The middleware used to send messages between processes. Examples: RabbitMQ, Kafka, AWS SNS/SQS, Azure Service Bus, Redis, MSSQL.

See: Transport-specific configuration pages

### InMemory Transport

An in-process transport for development and testing. Messages are stored in memory and delivered within the same process. Not durable - crashes lose messages.

See: [InMemory Options](/contents/InMemoryOptions.md)

### External Bus

Middleware such as RabbitMQ or Kafka used to send messages between processes. Contrasted with the Internal Bus (in-process).

See: [Show me the code!](/contents/ShowMeTheCode.md)

### Internal Bus

In-process message delivery without external middleware. Useful for decoupling components within a single application.

See: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

## Other Terms

### Nullable Reference Types

### DepositPost

A method that writes a message to the Outbox within a database transaction. Used for transactional messaging - guarantees that entity writes and message writes succeed or fail together.

See: [Outbox Support](/contents/BrighterOutboxSupport.md)

### ClearOutbox

A method that dispatches messages from the Outbox to the message broker. Can be called explicitly (low latency) or handled by a Sweeper (automatic retry).

See: [Outbox Support](/contents/BrighterOutboxSupport.md)

### Post

A method that writes a message to the Outbox and immediately attempts to dispatch it. Does not participate in database transactions. Suitable for scenarios where you don't need transactional guarantees.

See: [Outbox Support](/contents/BrighterOutboxSupport.md)

### Dynamic Deserialization

A technique for handling multiple message types on a single channel. Uses a callback (`getRequestType`) to determine the message type at runtime, typically based on CloudEvents type or message headers.

See: [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)

## Industry Standard Terms (EIP)

Brighter and Darker implement many patterns from [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/) by Gregor Hohpe and Bobby Woolf:

- **Command Processor** = Command pattern + Chain of Responsibility
- **Dispatcher** = Message Endpoint + Competing Consumers
- **Outbox** = Transactional Outbox pattern
- **Claim Check** = Claim Check pattern
- **Message Mapper** = Message Translator
- **Pipeline** = Pipes and Filters
- **Sweeper** = Polling Consumer + Message Store
- **Agreement Dispatcher** = Content-Based Router

---

## See Also

- [V10 Migration Guide](/contents/V10MigrationGuide.md) - Migrating from V9 to V10
- [FAQ](/contents/FAQ.md) - Frequently asked questions
- [Show me the code!](/contents/ShowMeTheCode.md) - Quick start examples
