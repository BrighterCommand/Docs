---
description: "This page is what you read when deciding whether to use PostgreSQL as a broker at all: what it buys you, where it runs out, how it stores a payload, and how it compares with the dedicated brokers."
layout:
  description:
    visible: false
---

# PostgreSQL Broker Trade-Offs

> **Explanation** · Applies to **Brighter V10** · Prerequisites: [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md)

This page is what you read when deciding whether to use PostgreSQL as a broker at all: what
it buys you, where it runs out, how it stores a payload, and how it compares with the
dedicated brokers. [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) is the
page to read once you have decided.

## PostgreSQL Message Broker Benefits

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

## When to Use the PostgreSQL Message Broker

**Ideal For**:

- **Low to moderate message volumes** (< 1000 messages/second)
- **Applications already using PostgreSQL** for data persistence
- **Transactional messaging** scenarios requiring atomicity with database operations
- **Development and testing** with simplified infrastructure
- **Microservices** where each service has its own PostgreSQL database

**Not Suitable For**:

- **High-volume scenarios** (> 1000 messages/second)
- **Large messages** (each message is stored whole in one table column, so large payloads add to the database's load)
- **Complex routing requirements** (better served by RabbitMQ or Kafka)
- **Cross-organization messaging** (where dedicated broker provides better isolation)

---

## PostgreSQL Message Broker Limitations

### Performance Constraints

- **Database overhead**: Message operations add load to your database
- **Polling model**: Consumers poll the database periodically (not push-based)
- **Scalability limits**: Database connection pooling and table locking can become bottlenecks

### Message Size

- **Limit**: Brighter stores the whole message as one value in the `content` column. With JSONB,
  PostgreSQL caps that value at 268,435,455 bytes (256 MB), and the stored message is larger than
  the payload it carries. We have tested payloads up to 50 MB; larger ones may fit, but a 150 MB
  payload is rejected on insert
- **Recommendation**: Use [Claim Check pattern](ClaimCheck.md) for large payloads, well before that limit

### No Native Routing

- **Simple pub/sub only**: No complex routing like RabbitMQ exchanges
- **Queue-based**: Each consumer reads from a specific queue (channel)
- **Manual fanout**: Publish to multiple channels for fanout patterns

---

## PostgreSQL JSON vs JSONB

PostgreSQL supports two JSON data types:

| Feature | JSON | JSONB |
|---------|------|-------|
| **Storage** | Text-based | Binary |
| **Performance** | Slower queries | Faster queries |
| **Size** | Smaller | Larger (pre-parsed) |
| **Indexing** | Limited | Full indexing support |
| **Recommendation** | Low volume | **Production use** |

### JSONB Configuration

`binaryMessagePayload` chooses the column type. Set it to `true` for JSONB, the recommended
form:

```csharp
using Paramore.Brighter;

var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    queueStoreTable: "brighter_messages",
    binaryMessagePayload: true  // JSONB
);
```

Leave it `false`, its default, for JSON and smaller storage:

```csharp
using Paramore.Brighter;

var configuration = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    queueStoreTable: "brighter_messages",
    binaryMessagePayload: false  // JSON
);
```

---

## PostgreSQL Message Broker Compared with Other Transports

| Feature | PostgreSQL | RabbitMQ | Kafka | AWS SQS |
|---------|------------|----------|-------|---------|
| **Setup Complexity** | Low | Medium | High | Low |
| **Throughput** | Low-Medium | High | Very High | Medium |
| **Message Size** | tested to 50MB (JSONB) | 128MB | ~1MB | 1MiB |
| **Persistence** | Database | Disk/Memory | Disk | Managed |
| **Routing** | Simple | Advanced | Topic-based | Simple |
| **Transactional** | Yes (local) | No | No | No |
| **Ordering** | Queue-level | Queue-level | Partition-level | FIFO queues |
| **Operational Cost** | Low (existing DB) | Medium | High | Pay-per-use |
| **Best For** | Low volume, transactional | General messaging | Event streaming | AWS ecosystem |

AWS SQS accepts messages up to 1 MiB. Brighter's support for SQS messages over 256 KB ships in the
release after 10.7.0.

---

## Further Reading

- [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) - Producer, consumer and configuration options
- [Claim Check Pattern](/contents/ClaimCheck.md) - Keeping large payloads out of the queue table
- [PostgreSQL Outbox](/contents/PostgresOutbox.md) - The transactional outbox on the same database
