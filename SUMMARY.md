## Get Started

* [Why Brighter?](/contents/WhyBrighter.md)
* [Basic Concepts](/contents/BasicConcepts.md)
* [Show me the code!](/contents/ShowMeTheCode.md)

## Commands, Handlers and Pipelines

* [Requests, Commands and Events](/contents/Requests%2C%20Commands%20and%20Events.md)
* [Dispatching Requests](/contents/DispatchingARequest.md)
  * [Dispatching an Async Request](/contents/AsyncDispatchARequest.md)
  * [Returning Results from a Handler](/contents/ReturningResultsFromAHandler.md)
* [How to Implement a Request Handler](/contents/ImplementingAHandler.md)
  * [How to Implement an Async Request Handler](/contents/ImplementingAsyncHandler.md)
* [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md)
  * [Building an Async Pipeline of Request Handlers](/contents/BuildingAnAsyncPipeline.md)
  * [Passing Information Between Handlers in the Pipeline](/contents/UsingTheContextBag.md)
  * [Pipeline Validation and Diagnostics](/contents/PipelineValidation.md)
  * [Request Validation](/contents/RequestValidation.md)
  * [Feature Switches](/contents/FeatureSwitches.md)
  * [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)
  * [Failure and Fallback](/contents/PolicyFallback.md)
* [Agreement Dispatcher](/contents/AgreementDispatcher.md)

## Brighter Configuration

* [Basic Configuration](/contents/BrighterBasicConfiguration.md)
  * [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md)
  * [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md)
* [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)
* [Test Double Options for Command Processor](/contents/TestDoubleOptions.md)
* [Analyzer Support](/contents/AnalyzerSupport.md)

## Using an External Bus

* [Using an External Bus](/contents/ImplementingExternalBus.md)
* [Routing](/contents/Routing.md)
* [Message Mappers](/contents/MessageMappers.md)
  * [Default Message Mappers](/contents/DefaultMessageMappers.md)
* [Cloud Events Support](/contents/CloudEventsSupport.md)
* [Claim Check](/contents/ClaimCheck.md)
  * [S3 Luggage Store](/contents/S3LuggageStore.md)
* [Compression](/contents/Compression.md)
* [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md)
* [AsyncAPI Document Generation](/contents/AsyncAPISupport.md)
* [Error Handling](/contents/HandlerFailure.md)
  * [Error Handling Options](/contents/ErrorHandlingOptions.md)

## Transports

* [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)
  * [RabbitMQ Durability: Quorum Queues and Persistence](/contents/RabbitMQDurability.md)
  * [Migrating to Quorum Queues](/contents/RabbitMQMigrateToQuorumQueues.md)
  * [RabbitMQ Connection Stability](/contents/RabbitMQConnectionStability.md)
* [Kafka Configuration](/contents/KafkaConfiguration.md)
* [AWS SNS and SQS Configuration](/contents/AWSSQSConfiguration.md)
* [Azure Service Bus Configuration](/contents/AzureServiceBusConfiguration.md)
* [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md)
* [Brighter Control API](/contents/BrighterControlAPI.md)

## Outbox and Inbox

* [Outbox Support](/contents/BrighterOutboxSupport.md)
  * [MSSQL Outbox](/contents/MSSQLOutbox.md)
  * [MySQL Outbox](/contents/MySQLOutbox.md)
  * [Postgres Outbox](/contents/PostgresOutbox.md)
  * [Sqlite Outbox](/contents/SqliteOutbox.md)
  * [Dapper Outbox](/contents/DapperOutbox.md)
  * [EF Core Outbox](/contents/EFCoreOutbox.md)
  * [Dynamo Outbox](/contents/DynamoOutbox.md)
  * [MongoDb Outbox](/contents/MongoDBOutbox.md)
* [Azure Blob Archive Provider](/contents/AzureBlobArchiveProvider.md)
  * [Azure Archive Provider Configuration](/contents/AzureBlobConfiguration.md)
* [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md)
* [Inbox Support](/contents/BrighterInboxSupport.md)
  * [MSSQL Inbox](/contents/MSSQLInbox.md)
  * [MySQL Inbox](/contents/MySQLInbox.md)
  * [Postgres Inbox](/contents/PostgresInbox.md)
  * [Sqlite Inbox](/contents/SqliteInbox.md)
  * [Dynamo Inbox](/contents/DynamoInbox.md)
  * [MongoDb Inbox](/contents/MongoDBInbox.md)
* [Replay On Seen](/contents/ReplayOnSeen.md)
* [Causation Tracking in a Custom Store](/contents/CausationTrackingStores.md)
* [Distributed Lock](/contents/DistributedLock.md)
  * [DynamoDB Distributed Lock](/contents/DynamoDbDistributedLock.md)
  * [Postgres Distributed Lock](/contents/PostgresDistributedLock.md)
  * [MSSQL Distributed Lock](/contents/MsSqlDistributedLock.md)
  * [MySQL Distributed Lock](/contents/MySqlDistributedLock.md)
  * [Azure Blob Distributed Lock](/contents/AzureBlobDistributedLock.md)
  * [MongoDB Distributed Lock](/contents/MongoDbDistributedLock.md)
  * [Firestore Distributed Lock](/contents/FirestoreDistributedLock.md)
* [Box Provisioning](/contents/BoxProvisioning.md)
  * [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md)
  * [Upgrading Existing Deployments](/contents/BoxProvisioningUpgrade.md)

## Scheduler

* [Scheduler](/contents/BrighterSchedulerSupport.md)
  * [InMemory Scheduler](/contents/InMemoryScheduler.md)
  * [Hangfire](/contents/HangfireScheduler.md)
  * [Quartz](/contents/QuartzScheduler.md)
  * [TickerQ](/contents/TickerQScheduler.md)
  * [Aws Scheduler](/contents/AwsScheduler.md)
  * [Azure Scheduler](/contents/AzureScheduler.md)
* [Scheduling a Message](/contents/SchedulingAMessage.md)
* [Switching Schedulers](/contents/SwitchingSchedulers.md)
* [Custom Scheduler](/contents/CustomScheduler.md)

## Darker

* [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md)
* [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)
* [How to Implement a Query Handler](/contents/ImplementAQueryHandler.md)
* [Query Pipeline and Decorators](/contents/QueryPipeline.md)
* [Query Patterns](/contents/QueryPatterns.md)

## Health Checks and Observability

* [Logging](/contents/Logging.md)
* [Monitoring](/contents/Monitoring.md)
* [Health Checks](/contents/HealthChecks.md)
* [Telemetry](/contents/Telemetry.md)

## V10 Migration

* [V10 Migration Guide](/contents/V10MigrationGuide.md)
* [Nullable Reference Types](/contents/NullableReferenceTypes.md)

## Understanding Brighter

* [Command, Processor and Dispatcher Patterns](/contents/CommandsCommandDispatcherandProcessor.md)
* [Using a Task Queue](/contents/TaskQueuePattern.md)
* [Microservices](/contents/Microservices.md)
* [Event Driven Collaboration](/contents/EventDrivenCollaboration.md)
* [Event Carried State Transfer](/contents/EventCarriedStateTransfer.md)
* [Outbox Pattern](/contents/OutboxPattern.md)
* [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)
* [How the Command Processor Works](/contents/HowBrighterWorks.md)
  * [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md)
* [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md)
  * [How Configuring a Dispatcher for an External Bus Works](/contents/HowConfiguringTheDispatcherWorks.md)
* [Reactor and Proactor: Concurrency Models](/contents/ReactorAndProactor.md)

## Reference

* [Glossary](/contents/Glossary.md)
* [FAQ](/contents/FAQ.md)
