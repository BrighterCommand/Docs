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

## Benefits

### Use Existing Infrastructure

- **No additional services**: Uses your existing PostgreSQL database
- **Simplified operations**: One less service to manage, monitor, and maintain
- **Reduced costs**: No separate message broker licensing or infrastructure

### Transactional Guarantees

- **Atomic operations**: Messages and business data in the same database
- **Strong consistency**: ACID guarantees for message operations
- **Simplified transactions**: No distributed transactions needed

### Familiar Tooling

- **Standard SQL**: Use familiar PostgreSQL tools for monitoring and debugging
- **Built-in monitoring**: Query tables directly to see queue depth and message status
- **Easy troubleshooting**: Direct database access for investigating issues

---

## When to Use

**Ideal For**:

- **Low to moderate message volumes** (< 1000 messages/second)
- **Applications already using PostgreSQL** for data persistence
- **Transactional messaging** scenarios requiring atomicity with database operations
- **Development and testing** with simplified infrastructure
- **Microservices** where each service has its own PostgreSQL database

**Not Suitable For**:

- **High-volume scenarios** (> 1000 messages/second)
- **Large messages** (PostgreSQL has practical limits for row sizes)
- **Complex routing requirements** (better served by RabbitMQ or Kafka)
- **Cross-organization messaging** (where dedicated broker provides better isolation)

---

## PostgreSQL Message Broker Limitations

### Performance Constraints

- **Database overhead**: Message operations add load to your database
- **Polling model**: Consumers poll the database periodically (not push-based)
- **Scalability limits**: Database connection pooling and table locking can become bottlenecks

### Message Size

- **Practical limit**: ~1MB per message (PostgreSQL row size limits)
- **Recommendation**: Use [Claim Check pattern](ClaimCheck.md) for large payloads

### No Native Routing

- **Simple pub/sub only**: No complex routing like RabbitMQ exchanges
- **Queue-based**: Each consumer reads from a specific queue (channel)
- **Manual fanout**: Publish to multiple channels for fanout patterns

---

## PostgreSQL Message Broker Configuration

### NuGet Package

Install the PostgreSQL messaging gateway package:

```bash
dotnet add package Paramore.Brighter.MessagingGateway.Postgres
```

### Database Table

Create the queue store table in your PostgreSQL database:

```sql
CREATE TABLE IF NOT EXISTS {schema}.{queue_store_table}
(
    "id" BIGSERIAL PRIMARY KEY,
    "queue" VARCHAR(255) NOT NULL,
    "content" JSONB NOT NULL,
    "visible_timeout" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_at" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_{queue_store_table}_queue_visible
    ON {schema}.{queue_store_table}("queue", "visible_timeout");
```

**Index Requirements**: The index on `(queue, visible_timeout)` is critical for performance.

---

## Producer Configuration

### Basic Producer Setup

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.PostgreSql;

// Database configuration
var postgresConfiguration = new RelationalDatabaseConfiguration(
    connectionString: "Host=localhost;Database=myapp;Username=user;Password=pass",
    queueStoreTable: "brighter_messages",
    schemaName: "public",
    binaryMessagePayload: true  // Use JSONB for better performance
);

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
    postgresConfiguration,
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

```csharp
public class OrderService
{
    private readonly IAmACommandProcessor _commandProcessor;

    public OrderService(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        // Process order
        var order = ProcessOrder(command);

        // Publish event
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        await _commandProcessor.PublishAsync(orderCreatedEvent);
    }
}
```

---

## Consumer Configuration

### Basic Consumer Setup

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.PostgreSql;

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
        channelName: new ChannelName("orders.created.consumer"),
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

// Channel factory
var channelFactory = new PostgresChannelFactory(postgresConfiguration);

// Configure Brighter Consumer
services.AddConsumers(options =>
{
    options.Subscriptions = subscriptions;
    options.ChannelFactory = channelFactory;
})
.AutoFromAssemblies();
```

### Consuming Messages

```csharp
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

### PostgresPublication

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `Topic` | `RoutingKey` | Message routing key (queue name) | Required |
| `SchemaName` | `string?` | Database schema name | `"public"` |
| `QueueStoreTable` | `string?` | Queue store table name | From configuration |
| `BinaryMessagePayload` | `bool?` | Use JSONB instead of JSON | From configuration |

### PostgresSubscription

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `ChannelName` | `ChannelName` | Consumer channel name | Required |
| `RoutingKey` | `RoutingKey` | Message routing key (queue name) | Required |
| `BufferSize` | `int` | Messages to retrieve per poll | `1` |
| `NoOfPerformers` | `int` | Concurrent consumer threads | `1` |
| `VisibleTimeout` | `TimeSpan` | Message visibility timeout | `30 seconds` |
| `SchemaName` | `string?` | Database schema name | `"public"` |
| `QueueStoreTable` | `string?` | Queue store table name | From configuration |
| `BinaryMessagePayload` | `bool?` | Expect JSONB payloads | From configuration |
| `TableWithLargeMessage` | `bool` | Support for large messages as streams | `false` |

### RelationalDatabaseConfiguration

| Property | Type | Description |
|----------|------|-------------|
| `ConnectionString` | `string` | PostgreSQL connection string |
| `QueueStoreTable` | `string` | Default queue store table name |
| `SchemaName` | `string` | Default schema name |
| `BinaryMessagePayload` | `bool` | Default to JSONB |

---

## Message Visibility

The PostgreSQL message broker uses a **visibility timeout** mechanism to prevent duplicate processing:

### How Message Visibility Works

1. **Message Published**: `visible_timeout` set to `CURRENT_TIMESTAMP`
2. **Message Retrieved**: Consumer reads messages where `visible_timeout <= CURRENT_TIMESTAMP`
3. **Processing**: Message becomes invisible to other consumers (timeout not updated)
4. **Acknowledged**: Message deleted from table
5. **Timeout Expires**: If not acknowledged, message becomes visible again

### Visibility Timeout Example

```csharp
var subscription = new PostgresSubscription<OrderEvent>(
    // Message invisible for 60 seconds after retrieval
    visibleTimeout: TimeSpan.FromSeconds(60)
);
```

**Recommendation**: Set visibility timeout to **2-3x your expected processing time** to account for retries and delays.

---

## JSON vs JSONB

PostgreSQL supports two JSON data types:

| Feature | JSON | JSONB |
|---------|------|-------|
| **Storage** | Text-based | Binary |
| **Performance** | Slower queries | Faster queries |
| **Size** | Smaller | Larger (pre-parsed) |
| **Indexing** | Limited | Full indexing support |
| **Recommendation** | Low volume | **Production use** |

### JSONB Configuration

```csharp
// Use JSONB (recommended)
var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    queueStoreTable: "brighter_messages",
    binaryMessagePayload: true  // JSONB
);

// Use JSON (smaller storage)
var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    queueStoreTable: "brighter_messages",
    binaryMessagePayload: false  // JSON
);
```

---

## Scheduled Messages

PostgreSQL message broker supports message scheduling using the visibility timeout:

```csharp
// Schedule message for future delivery
var delayedEvent = new OrderReminderEvent
{
    OrderId = orderId,
    ReminderText = "Your order ships tomorrow!"
};

await _commandProcessor.PublishAsync(
    delayedEvent,
    delay: TimeSpan.FromHours(24)  // Deliver in 24 hours
);
```

**How it works**: The `visible_timeout` is set to `CURRENT_TIMESTAMP + delay`, making the message invisible until the scheduled time.

---

## Transactional Messaging

A key advantage of PostgreSQL as a message broker is **transactional messaging** with your business data:

### Using the Outbox Pattern

```csharp
public class OrderService
{
    private readonly OrderDbContext _dbContext;
    private readonly IAmACommandProcessor _commandProcessor;

    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Save order to database
            var order = new Order { /* ... */ };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // 2. Deposit event to Outbox (same transaction)
            var orderCreatedEvent = new OrderCreatedEvent { /* ... */ };
            await _commandProcessor.DepositPostAsync(orderCreatedEvent);

            // 3. Commit transaction (atomically saves order and outbox message)
            await transaction.CommitAsync();

            // 4. Clear outbox to publish message
            await _commandProcessor.ClearOutboxAsync(new[] { orderCreatedEvent.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

See [Outbox Pattern](OutboxPattern.md) and [PostgreSQL Outbox](PostgresOutbox.md) for more details.

---

## PostgreSQL Message Broker Monitoring and Observability

### Query Queue Depth

```sql
-- Current queue depth by queue
SELECT
    "queue",
    COUNT(*) as message_count,
    MIN("visible_timeout") as oldest_visible,
    MAX("created_at") as newest_message
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
-- Messages invisible for too long (potential failures)
SELECT
    "id",
    "queue",
    "created_at",
    "visible_timeout",
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - "visible_timeout")) as seconds_overdue
FROM "public"."brighter_messages"
WHERE "visible_timeout" < CURRENT_TIMESTAMP - INTERVAL '5 minutes'
ORDER BY "created_at";
```

### OpenTelemetry Integration

PostgreSQL message broker operations are automatically traced when [OpenTelemetry](Telemetry.md) is configured:

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("paramore.brighter")
            .AddNpgsql()  // PostgreSQL spans
            .AddOtlpExporter();
    });
```

---

## PostgreSQL Message Broker Best Practices

### 1. Use JSONB for Production

```csharp
var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    binaryMessagePayload: true  // JSONB for better performance
);
```

### 2. Set Appropriate Visibility Timeout

```csharp
var subscription = new PostgresSubscription<OrderEvent>(
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

Implement cleanup for old messages (if not using auto-vacuum):

```sql
-- Delete messages older than 7 days
DELETE FROM brighter_messages
WHERE "created_at" < CURRENT_TIMESTAMP - INTERVAL '7 days';
```

### 7. Use Claim Check for Large Messages

For messages > 100KB, use the [Claim Check pattern](ClaimCheck.md):

```csharp
[ClaimCheck(threshold: 102400, dataStore: typeof(S3LuggageStore))]
public class ProcessLargeOrderCommand : Command
{
    public byte[] LargePayload { get; set; }  // Stored in S3, not in database
}
```

### 8. Separate Queue Tables for High Volume

For high-volume queues, use dedicated tables:

```csharp
// High-volume queue
var highVolumeSubscription = new PostgresSubscription<HighVolumeEvent>(
    queueStoreTable: "brighter_high_volume_messages"  // Separate table
);

// Normal queue
var normalSubscription = new PostgresSubscription<NormalEvent>(
    queueStoreTable: "brighter_messages"  // Shared table
);
```

---

## Comparison with Other Transports

| Feature | PostgreSQL | RabbitMQ | Kafka | AWS SQS |
|---------|------------|----------|-------|---------|
| **Setup Complexity** | Low | Medium | High | Low |
| **Throughput** | Low-Medium | High | Very High | Medium |
| **Message Size** | ~1MB | 128MB | ~1MB | 256KB |
| **Persistence** | Database | Disk/Memory | Disk | Managed |
| **Routing** | Simple | Advanced | Topic-based | Simple |
| **Transactional** | Yes (local) | No | No | No |
| **Ordering** | Queue-level | Queue-level | Partition-level | FIFO queues |
| **Operational Cost** | Low (existing DB) | Medium | High | Pay-per-use |
| **Best For** | Low volume, transactional | General messaging | Event streaming | AWS ecosystem |

---

## PostgreSQL Message Broker Troubleshooting

### Messages Not Being Consumed

**Problem**: Messages remain in the queue but are not processed.

**Solutions**:

1. Check visibility timeout hasn't expired:
   ```sql
   SELECT * FROM brighter_messages
   WHERE queue = 'your.queue' AND visible_timeout > CURRENT_TIMESTAMP;
   ```
2. Verify consumer is running and subscriptions match queue names
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
3. Increase `timeOut` to reduce polling frequency
4. Consider using `bufferSize > 1` to retrieve multiple messages per poll

---

## Further Reading

- [PostgreSQL Outbox](PostgresOutbox.md)
- [PostgreSQL Inbox](PostgresInbox.md)
- [Outbox Pattern](OutboxPattern.md)
- [Claim Check Pattern](ClaimCheck.md)
- [OpenTelemetry Integration](Telemetry.md)
- [Transactional Messaging](/contents/BrighterOutboxSupport.md#complete-example-transactional-messaging)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql - .NET PostgreSQL Driver](https://www.npgsql.org/)
