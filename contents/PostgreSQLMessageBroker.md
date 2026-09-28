---
description: "Brighter supports for using PostgreSQL as a message broker, enabling pub/sub messaging patterns using your existing PostgreSQL infrastructure."
layout:
  description:
    visible: false
---

# PostgreSQL Message Broker

> **Reference** · Applies to **Brighter V10**

Brighter supports for using PostgreSQL as a message broker, enabling pub/sub messaging patterns using your existing PostgreSQL infrastructure.

## PostgreSQL Message Broker Overview

The PostgreSQL message broker uses a table-based queue approach where messages are stored in a PostgreSQL table and retrieved by consumers. This provides a lightweight messaging solution that leverages your existing PostgreSQL database without requiring additional message broker infrastructure.

### How the PostgreSQL Broker Works

1. **Producer**: Inserts messages into a queue store table
2. **Consumer**: Retrieves messages from the queue store table based on visibility timeout
3. **Acknowledgement**: Deletes processed messages from the table
4. **Reject/Requeue**: Deletes or updates messages based on processing outcome

The system uses a visibility timeout mechanism (similar to AWS SQS) where messages become invisible to other consumers once retrieved, preventing duplicate processing.

---

## PostgreSQL Message Broker Configuration

### NuGet Package

Install the PostgreSQL messaging gateway package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.Postgres
```

### Database Table

Brighter creates the queue store table for you when a publication or subscription sets `MakeChannels = OnMissingChannel.Create`. Set `OnMissingChannel.Validate` instead to manage the table yourself — this is the DDL Brighter runs, and the one to match:

```sql
CREATE TABLE IF NOT EXISTS "{schema}"."{queue_store_table}"
(
    "id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "visible_timeout" TIMESTAMPTZ,
    "queue" VARCHAR(255),
    "content" JSON
);

CREATE INDEX IF NOT EXISTS "{schema}_{queue_store_table}_queue_visible_timeout_idx"
    ON "{schema}"."{queue_store_table}"("queue", "visible_timeout") INCLUDE ("id");
```

The `content` column is `JSONB` rather than `JSON` when the payload is binary — see `binaryMessagePayload` below.

**Index Requirements**: The index on `(queue, visible_timeout)` is critical for performance.

---

## Producer Configuration

### Basic Producer Setup

```csharp
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Postgres;

// Database configuration
var postgresConfiguration = new RelationalDatabaseConfiguration(
    connectionString: "Host=localhost;Database=myapp;Username=user;Password=pass",
    queueStoreTable: "brighter_messages",
    schemaName: "public",
    binaryMessagePayload: true  // Use JSONB for better performance
);

// The gateway connection wraps that configuration; the producer registry takes this,
// not the configuration itself
var connection = new PostgresMessagingGatewayConnection(postgresConfiguration);

// Publication configuration
var publications = new List<PostgresPublication>
{
    new PostgresPublication<OrderCreatedEvent>
    {
        Topic = new RoutingKey("orders.created"),
        SchemaName = "public",
        QueueStoreTable = "brighter_messages",
        BinaryMessagePayload = true  // JSONB
    }
};

// Producer registry
var producerRegistry = new PostgresProducerRegistryFactory(
    connection,
    publications
).Create();

// Configure Brighter
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(configure =>
{
    configure.ProducerRegistry = producerRegistry;
})
.AutoFromAssemblies();
```

### Publishing Messages

`PostAsync` sends a request through its publication's producer, so here it inserts a row into the
queue store table:

```csharp
using System;
using System.Threading.Tasks;
using Paramore.Brighter;

public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public OrderService(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        // ... process the order

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = command.OrderId,
            CustomerId = command.CustomerId,
            TotalAmount = command.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        // Write the event to the queue store table
        await _commandProcessor.PostAsync(orderCreatedEvent);
    }
}
```

`PublishAsync` would not reach the table: it dispatches the event to handlers in this process.

---

## Consumer Configuration

### Basic Consumer Setup

The producer writes each message under its publication's `Topic`, and a subscription reads the
messages written under its `channelName`. **The two must be the same string**: a subscription whose
`channelName` differs from the `Topic` never receives a message.

```csharp
using System;
using System.Collections.Generic;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

// Database configuration
var postgresConfiguration = new RelationalDatabaseConfiguration(
    connectionString: "Host=localhost;Database=myapp;Username=user;Password=pass",
    queueStoreTable: "brighter_messages",
    schemaName: "public",
    binaryMessagePayload: true
);

// Subscription configuration
var subscriptions = new List<PostgresSubscription>
{
    new PostgresSubscription<OrderCreatedEvent>(
        channelName: new ChannelName("orders.created"),  // the publication's Topic
        routingKey: new RoutingKey("orders.created"),
        bufferSize: 10,                         // Number of messages to retrieve at once
        noOfPerformers: 1,                      // Number of concurrent consumers
        timeOut: TimeSpan.FromSeconds(30),
        messagePumpType: MessagePumpType.Proactor,
        makeChannels: OnMissingChannel.Create,
        visibleTimeout: TimeSpan.FromSeconds(30), // Message visibility timeout
        schemaName: "public",
        queueStoreTable: "brighter_messages",
        binaryMessagePayload: true
    )
};

// Channel factory: it takes the configuration wrapped in a gateway connection
var channelFactory = new PostgresChannelFactory(new PostgresMessagingGatewayConnection(postgresConfiguration));

// Configure Brighter Consumer
services.AddConsumers(options =>
{
    options.Subscriptions = subscriptions;
    options.DefaultChannelFactory = channelFactory;
})
.AutoFromAssemblies();
```

### Consuming Messages

```csharp
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;

public class OrderCreatedEventHandler : RequestHandlerAsync<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IEmailService emailService,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public override async Task<OrderCreatedEvent> HandleAsync(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing order created event for Order {OrderId}",
            @event.OrderId);

        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(@event);

        return await base.HandleAsync(@event, cancellationToken);
    }
}
```

---

## PostgreSQL Message Broker Configuration Options

Three of the four settings below default to `null`, and a `null` is not "unset": the schema, the
queue store table and the payload format each fall back to the same setting on the
[relational database configuration](/contents/RelationalDatabaseConfigurationReference.md#relational-database-configuration-options)
the connection carries, and the schema falls back once more to `public`.

### PostgresPublication Options

`PostgresPublication` takes its options as properties and adds these three to the
[base publication options](/contents/CommandProcessorConfigurationReference.md#publication-options),
which carry `Topic`.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.Postgres.PostgresPublication -->

| Option | Type | Default | Description |
|---|---|---|---|
| `SchemaName` | `string?` | `null` | The schema the queue store table lives in. |
| `QueueStoreTable` | `string?` | `null` | The table messages are written to. |
| `BinaryMessagePayload` | `bool?` | `null` | Whether the payload column is written as JSONB rather than JSON. |

### PostgresSubscription Options

`PostgresSubscription` takes its options as constructor arguments, so the option is the
parameter you type. The seventeen it shares with
[`Subscription`](/contents/DispatcherConfigurationReference.md#subscription-options) behave
the same way here; the other seven are PostgreSQL's own.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.Postgres.PostgresSubscription
     manual: dataType — assigned to RequestType, and the constructor rejects its own default, so there is no default to read
     manual: getRequestType — assigned to MapRequestType, and the body substitutes a function returning RequestType when it is null
     manual: messagePumpType — the constructor rejects its own default of Unknown, so there is no default to read
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription for diagnostics; read back as `Name`. |
| `channelName` | `ChannelName` | `none` | Names the queue this subscription reads; it must match the publication's `Topic`. |
| `routingKey` | `RoutingKey` | `none` | The routing key messages are written under. |
| `dataType` | `Type?` | `none` | The request type messages on this queue are translated into; read back as `RequestType`. |
| `getRequestType` | `Func<Message, Type>?` | derives the type from `dataType` | Determines the request type from the message rather than from the queue. |
| `bufferSize` | `int` | `1` | Messages read from the queue at once and held in the channel. |
| `noOfPerformers` | `int` | `1` | Threads reading this queue, each with its own message pump. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits before treating the queue as empty. |
| `requeueCount` | `int` | `-1` | Times a message is handled before it is rejected as a poison pill, so `3` is two requeues; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | How long delivery of a requeued message is delayed. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages before the channel stops; 0 disables the limit. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | The window the unacceptable-message count resets at the end of. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor concurrency model. |
| `channelFactory` | `IAmAChannelFactory?` | `null` | Creates the channel; falls back to `DefaultChannelFactory` when null. |
| `makeChannels` | `OnMissingChannel` | `Create` | Whether Brighter creates the queue store table, validates it, or assumes it. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | How long the pump pauses after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | How long the pump pauses after a channel failure. |
| `schemaName` | `string?` | `null` | The schema the queue store table lives in. |
| `queueStoreTable` | `string?` | `null` | The table messages are read from. |
| `visibleTimeout` | `TimeSpan?` | `30000 ms` | How long a read message stays invisible to other consumers. |
| `tableWithLargeMessage` | `bool` | `false` | Whether payloads are read as streams to support large messages. |
| `binaryMessagePayload` | `bool?` | `null` | Whether the payload column is read as JSONB rather than JSON. |
| `deadLetterRoutingKey` | `RoutingKey?` | `null` | The routing key messages are dead-lettered to. |
| `invalidMessageRoutingKey` | `RoutingKey?` | `null` | The routing key unacceptable messages are routed to. |

The request type parameter is `dataType` here rather than `requestType`, which is what every
other transport in this documentation calls it, and it is read back as `RequestType`.

The generic form `PostgresSubscription<T>`, which every example above uses, takes the same
options and supplies four defaults the table cannot: `dataType` is `T`, and
`subscriptionName`, `channelName` and `routingKey` are `T`'s full name. It leaves
`messagePumpType` required, so state Reactor or Proactor on every subscription.

### PostgresMessagingGatewayConnection Options

The connection wraps the relational database configuration rather than adding settings of its
own.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.Postgres.PostgresMessagingGatewayConnection -->

| Option | Type | Default | Description |
|---|---|---|---|
| `configuration` | `RelationalDatabaseConfiguration` | `none` | The connection string, schema and table names for the queue store; read back as `Configuration`. |

The eight options on that configuration are documented once, at
[Relational Database Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md),
because seventeen Brighter components share them.

---

## Message Visibility

The PostgreSQL message broker uses a **visibility timeout** mechanism to prevent duplicate processing:

### How Message Visibility Works

1. **Message Published**: `visible_timeout` set to `CURRENT_TIMESTAMP`
2. **Message Retrieved**: Consumer reads messages where `visible_timeout <= CURRENT_TIMESTAMP`
3. **Processing**: The same statement moves the message's `visible_timeout` to `CURRENT_TIMESTAMP` plus the subscription's `visibleTimeout`, so other consumers skip it
4. **Acknowledged**: Message deleted from table
5. **Timeout Expires**: If not acknowledged, message becomes visible again

### Visibility Timeout Example

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;

var subscription = new PostgresSubscription<OrderEvent>(
    messagePumpType: MessagePumpType.Proactor,
    // Message invisible for 60 seconds after retrieval
    visibleTimeout: TimeSpan.FromSeconds(60)
);
```

**Recommendation**: Set visibility timeout to **2-3x your expected processing time** to account for retries and delays.

---

## Scheduled Messages

A delayed post goes through Brighter's [scheduler](/contents/SchedulingAMessage.md), not through
the queue store table. `PostAsync` with a delay hands the request to the configured scheduler, and
the row is written, visible at once, when the delay has elapsed:

```csharp
using System;
using Paramore.Brighter;

var reminder = new OrderReminderEvent
{
    OrderId = orderId,
    ReminderText = "Your order ships tomorrow!"
};

// The row reaches the queue store table in 24 hours
await commandProcessor.PostAsync(TimeSpan.FromHours(24), reminder);
```

Without `UseScheduler()`, the scheduler is the in-memory one, which holds the delay in the
process — use a durable scheduler for a delay that must outlive it. `PublishAsync` with a delay also
goes through the scheduler, and when it falls due it dispatches to handlers in this process, so the
event never reaches the table. At 10.7.0 a scheduled request fails when it falls due if you
registered handlers with `AutoFromAssemblies()`; see
[Registering Handlers When You Schedule Requests](/contents/SchedulingAMessage.md#registering-handlers-when-you-schedule-requests).

The table's `visible_timeout` does delay one thing: a requeue. When a handler defers a message and
the subscription sets `requeueDelay`, Brighter moves the row's `visible_timeout` to
`CURRENT_TIMESTAMP` plus that delay, and no consumer reads it until then.

---

## Transactional Messaging

A key advantage of PostgreSQL as a message broker is **transactional messaging** with your business data:

### Using the Outbox Pattern

The Outbox shares your transaction only when you pass `DepositPostAsync` a transaction provider.
Registered as the producers' `TransactionProvider`,
`PostgreSqlEntityFrameworkTransactionProvider<OrderDbContext>` hands the Outbox the transaction
your `DbContext` has open — see [PostgreSQL Outbox](PostgresOutbox.md) for that registration:

```csharp
using System;
using System.Threading.Tasks;
using Paramore.Brighter;

public class OrderService
{
    private readonly OrderDbContext _dbContext;
    private readonly IAmATransactionConnectionProvider _transactionProvider;
    private readonly IAmACommandProcessor _commandProcessor;

    public OrderService(
        OrderDbContext dbContext,
        IAmATransactionConnectionProvider transactionProvider,
        IAmACommandProcessor commandProcessor)
    {
        _dbContext = dbContext;
        _transactionProvider = transactionProvider;
        _commandProcessor = commandProcessor;
    }

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Save order to database
            var order = new Order { /* ... */ };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // 2. Deposit event to Outbox, on the same transaction
            var orderCreatedEvent = new OrderCreatedEvent { /* ... */ };
            await _commandProcessor.DepositPostAsync(orderCreatedEvent, _transactionProvider);

            // 3. Commit transaction (atomically saves order and outbox message)
            await transaction.CommitAsync();

            // 4. Clear outbox to publish message
            await _commandProcessor.ClearOutboxAsync(new[] { orderCreatedEvent.Id });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

Leave out `_transactionProvider` and the deposit takes its own connection: a rollback then removes
the order and leaves the message in the Outbox, to be sent for an order that does not exist.

See [Outbox Pattern](OutboxPattern.md) and [PostgreSQL Outbox](PostgresOutbox.md) for more details.

---

## PostgreSQL Message Broker Monitoring and Observability

### Query Queue Depth

The table records no creation time. `visible_timeout` is the nearest thing: for a message waiting
to be read, it is when the message became visible.

```sql
-- Current queue depth by queue
SELECT
    "queue",
    COUNT(*) as message_count,
    MIN("visible_timeout") as oldest_visible
FROM "public"."brighter_messages"
WHERE "visible_timeout" <= CURRENT_TIMESTAMP
GROUP BY "queue"
ORDER BY message_count DESC;
```

### Query In-Flight Messages

```sql
-- Messages currently being processed (invisible)
SELECT
    "queue",
    COUNT(*) as in_flight_count
FROM "public"."brighter_messages"
WHERE "visible_timeout" > CURRENT_TIMESTAMP
GROUP BY "queue";
```

### Find Stuck Messages

```sql
-- Messages visible for more than 5 minutes and still not read
SELECT
    "id",
    "queue",
    "visible_timeout",
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - "visible_timeout")) as seconds_waiting
FROM "public"."brighter_messages"
WHERE "visible_timeout" < CURRENT_TIMESTAMP - INTERVAL '5 minutes'
ORDER BY "id";
```

### OpenTelemetry Integration

The PostgreSQL producer records its sends on Brighter's spans once Brighter's tracer is registered —
see [Enabling Brighter's Spans](/contents/Telemetry.md#enabling-brighters-spans). Npgsql's own
spans come from its `Npgsql.OpenTelemetry` package:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Trace;
using Paramore.Brighter.Extensions.Diagnostics;

services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddBrighterInstrumentation()
            .AddNpgsql()  // PostgreSQL spans
            .AddOtlpExporter();
    });
```

---

## PostgreSQL Message Broker Best Practices

### 1. Use JSONB for Production

```csharp
using Paramore.Brighter;

var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    binaryMessagePayload: true  // JSONB for better performance
);
```

### 2. Set Appropriate Visibility Timeout

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;

var subscription = new PostgresSubscription<OrderEvent>(
    messagePumpType: MessagePumpType.Proactor,
    // 2-3x expected processing time
    visibleTimeout: TimeSpan.FromMinutes(5)  // Handler takes ~2 minutes max
);
```

### 3. Use Connection Pooling

```csharp
// Connection string with pooling
var connectionString = "Host=localhost;Database=myapp;Username=user;Password=pass;" +
                      "Minimum Pool Size=5;Maximum Pool Size=20";  // Connection pool
```

### 4. Monitor Queue Depth

Set up alerts for queue depth:

```sql
-- Alert if queue depth > 1000
SELECT COUNT(*) FROM brighter_messages WHERE queue = 'orders.created';
```

### 5. Index Your Queue Table

```sql
-- Critical index for performance
CREATE INDEX IF NOT EXISTS idx_messages_queue_visible
    ON brighter_messages("queue", "visible_timeout");
```

### 6. Regular Cleanup

Brighter deletes a row when its message is acknowledged or rejected, so the table holds only
messages not yet handled. Deleting old rows discards undelivered messages, so do it only for
messages nothing will read:

```sql
-- Discard messages visible for more than 7 days and never read
DELETE FROM brighter_messages
WHERE "visible_timeout" < CURRENT_TIMESTAMP - INTERVAL '7 days';
```

### 7. Use Claim Check for Large Messages

For messages > 100KB, use the [Claim Check pattern](ClaimCheck.md). The claim check attaches to
your message mapper, and its threshold is in kilobytes:

```csharp
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;

public class ProcessLargeOrderCommand() : Command(Id.Random())
{
    public byte[] LargePayload { get; set; } = [];  // Stored in the luggage store, not in the database
}

public class ProcessLargeOrderCommandMessageMapper : IAmAMessageMapper<ProcessLargeOrderCommand>
{
    public IRequestContext? Context { get; set; }

    [ClaimCheck(step: 0, thresholdInKb: 100)]
    public Message MapToMessage(ProcessLargeOrderCommand request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic!,
            messageType: MessageType.MT_COMMAND);

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));

        return new Message(header, body);
    }

    [RetrieveClaim(step: 0)]
    public ProcessLargeOrderCommand MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<ProcessLargeOrderCommand>(
            message.Body.Value, JsonSerialisationOptions.Options)!;
    }
}
```

The attribute does not name the store. You register one, such as the
[S3 Luggage Store](/contents/S3LuggageStore.md), with `UseExternalLuggageStore()` — see
[Handling Large Messages](/contents/HandlingLargeMessages.md).

### 8. Separate Queue Tables for High Volume

For high-volume queues, use dedicated tables:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;

// High-volume queue
var highVolumeSubscription = new PostgresSubscription<HighVolumeEvent>(
    messagePumpType: MessagePumpType.Proactor,
    queueStoreTable: "brighter_high_volume_messages"  // Separate table
);

// Normal queue
var normalSubscription = new PostgresSubscription<NormalEvent>(
    messagePumpType: MessagePumpType.Proactor,
    queueStoreTable: "brighter_messages"  // Shared table
);
```

---

## PostgreSQL Message Broker Troubleshooting

### Messages Not Being Consumed

**Problem**: Messages remain in the queue but are not processed.

**Solutions**:

1. Check for messages in flight — read by a consumer and not yet acknowledged:
   ```sql
   SELECT * FROM brighter_messages
   WHERE queue = 'your.queue' AND visible_timeout > CURRENT_TIMESTAMP;
   ```
2. Verify the consumer is running, and that each subscription's `channelName` is its publication's `Topic`
3. Check database connection pooling isn't exhausted
4. Review logs for consumer exceptions

### High Database Load

**Problem**: PostgreSQL CPU/disk usage is high.

**Solutions**:

1. Verify index exists on `(queue, visible_timeout)`
2. Use JSONB instead of JSON for better performance
3. Reduce `BufferSize` if retrieving too many messages at once
4. Consider partitioning the queue table for high volume
5. Use connection pooling to reduce connection overhead

### Messages Processed Multiple Times

**Problem**: Same message processed by multiple consumers.

**Solutions**:

1. Increase `visibleTimeout` to allow more processing time
2. Implement [Inbox pattern](PostgresInbox.md) for idempotency
3. Check for long-running handlers that exceed visibility timeout
4. Verify only one consumer process per subscription

### Slow Message Retrieval

**Problem**: Consumer polls are slow.

**Solutions**:

1. Add index: `CREATE INDEX ON brighter_messages(queue, visible_timeout)`
2. Use JSONB instead of JSON
3. Increase `emptyChannelDelay` to poll an empty queue less often
4. Consider using `bufferSize > 1` to retrieve multiple messages per poll

---

## Further Reading

- [PostgreSQL Broker Trade-Offs](/contents/PostgreSQLBrokerTradeOffs.md) - Benefits, limits, JSON vs JSONB, and how it compares
- [PostgreSQL Outbox](PostgresOutbox.md)
- [PostgreSQL Inbox](PostgresInbox.md)
- [Outbox Pattern](OutboxPattern.md)
- [Claim Check Pattern](ClaimCheck.md)
- [OpenTelemetry Integration](Telemetry.md)
- [Transactional Messaging](/contents/TransactionalMessagingWithTheOutbox.md)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql - .NET PostgreSQL Driver](https://www.npgsql.org/)
