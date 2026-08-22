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

```csharp
// ...
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

## PostgreSQL Message Broker Compared with Other Transports

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

## Further Reading

- [PostgreSQL Message Broker](/contents/PostgreSQLMessageBroker.md) - Producer, consumer and configuration options
- [Claim Check Pattern](/contents/ClaimCheck.md) - Keeping large payloads out of the queue table
- [PostgreSQL Outbox](/contents/PostgresOutbox.md) - The transactional outbox on the same database
