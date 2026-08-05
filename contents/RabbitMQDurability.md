# RabbitMQ Durability: Quorum Queues and Persistence

> **Explanation** · Applies to **Brighter V10** · Prerequisites: [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)

RabbitMQ offers two independent durability mechanisms, and they answer different
questions. Quorum queues decide what happens when a *node* fails; message persistence
decides what happens when the *broker* restarts. This page explains what each one buys
you and what it costs, so you can choose deliberately rather than enabling both by
reflex.

For the settings themselves, see
[RabbitMQ Quorum Queue Requirements](/contents/RabbitMQConfiguration.md#rabbitmq-quorum-queue-requirements)
and [RabbitMQ Persistence Options](/contents/RabbitMQConfiguration.md#rabbitmq-persistence-options).

## What are Quorum Queues?

[Quorum queues](https://www.rabbitmq.com/docs/quorum-queues) are a modern queue type introduced in RabbitMQ 3.8, designed to provide high availability and data consistency using the [Raft consensus algorithm](https://raft.github.io/). Unlike classic queues that use mirroring for high availability, quorum queues use a replicated state machine approach that ensures stronger consistency guarantees.

## Classic vs Quorum Queues

| Feature | Classic Queues | Quorum Queues |
|---------|---------------|---------------|
| **Purpose** | High throughput and low latency | High availability and data consistency |
| **Replication** | Mirroring (optional) | Built-in Raft-based replication |
| **Consistency** | Weaker guarantees | Strong consistency (Raft consensus) |
| **Performance** | Higher throughput | Lower throughput, more overhead |
| **Cluster Requirements** | Any cluster size | Requires at least 3 nodes for optimal performance |
| **Durability** | Optional | Required (isDurable must be true) |
| **HighAvailability Flag** | Supported (deprecated in RMQ 3+) | Not supported (highAvailability must be false) |
| **Use Cases** | Real-time streaming, high-volume processing | Financial transactions, critical business processes |

## When to Use Quorum Queues

Use **Quorum Queues** when:

- **Data consistency is critical**: Financial transactions, order processing, critical business logic
- **Message durability is essential**: Messages must survive node failures
- **You have a cluster**: Quorum queues require at least 3 nodes for optimal fault tolerance
- **You can accept lower throughput**: The Raft consensus algorithm adds overhead

Use **Classic Queues** when:

- **High throughput is priority**: Real-time data streaming, high-volume message processing
- **Low latency is required**: Time-sensitive applications
- **Message loss is acceptable**: Non-critical notifications, telemetry data
- **Single-node deployment**: Classic queues work well with a single node

Four further considerations weigh on that choice, and each of them is a reason a
quorum queue can disappoint you in a deployment that looks fine on paper:

- **A single node or 2-node cluster defeats the purpose.** Quorum queues are designed
  for clusters of at least 3 nodes; below that, the Raft consensus algorithm costs you
  its overhead without buying you its guarantee.
- **The overhead is measurable, so measure it.** Quorum queues have higher overhead
  than classic queues. Monitor throughput and latency to confirm they still meet your
  requirements.
- **Reserve them for the workflows that need them.** Consistency and durability are
  worth paying for on some messages and not on others; classic queues remain the right
  answer for high-throughput, less critical workloads.
- **Message size and cluster capacity multiply.** Each quorum queue replicates every
  message across multiple nodes, so large messages consume network bandwidth, and each
  queue consumes more disk space and memory than its classic equivalent. Plan capacity
  for the replication factor, not the message count.

## What is Message Persistence?

RabbitMQ supports message persistence, which saves messages to disk to ensure they survive broker restarts or node failures. Brighter supports persistent messages through the `PersistMessages` configuration property.

Message persistence in RabbitMQ involves two components:

1. **Durable Queues**: Queue definitions that survive broker restarts
2. **Persistent Messages**: Individual messages marked for disk storage

When both components are enabled, messages will survive broker restarts. However, there is a small window between when RabbitMQ receives a message and when it's written to disk, during which messages could be lost if the broker crashes.

That window is why persistence is a *reduction* in the probability of loss rather than
an elimination of it, and why the Outbox pattern remains the thing that gives you an
at-least-once guarantee.

## When to Use Persistent Messages

Use **Persistent Messages** when:

- **Message loss is unacceptable**: Financial transactions, critical business data
- **Broker restarts must not lose data**: Long-running workflows, state management
- **Regulatory requirements**: Audit trails, compliance scenarios
- **Combined with Outbox pattern**: Ensures at-least-once delivery guarantees

Do not use persistent messages when:

- **High throughput is critical**: Real-time streaming, telemetry data
- **Message loss is acceptable**: Non-critical notifications, cache updates
- **Short message lifetime**: Messages that expire quickly
- **Memory-based queues**: Testing, development environments

Three things follow from that, and the first two mean persistence is rarely a decision
you make on its own:

- **Quorum queues require it.** They provide stronger durability guarantees precisely
  because they will not run without durable definitions, so choosing quorum queues has
  already chosen persistence for you.
- **The [Outbox](/contents/BrighterOutboxSupport.md) is the other half.** For
  transactional messaging, persistence protects the message once RabbitMQ has it; the
  Outbox protects it before RabbitMQ has it. Neither substitutes for the other.
- **Publisher confirms are already on.** Brighter uses
  [Publisher Confirms](https://www.rabbitmq.com/confirms.html) by default to establish
  that RabbitMQ has accepted a message, which is what makes the Outbox's dispatch
  timestamp meaningful. You do not enable this; you rely on it.

## Persistence Performance Considerations

Message persistence comes with performance trade-offs:

- **Slower throughput**: Writing to disk is slower than keeping messages in memory
- **Increased latency**: Each message write involves disk I/O
- **Disk space**: Persistent messages consume disk storage, so monitor and alert on disk usage — a broker that fills its disk blocks its producers rather than failing them, which reads as a hang
- **Fsync operations**: RabbitMQ periodically flushes to disk (configurable)

For high-throughput applications where message loss is acceptable, consider using non-persistent messages (the default).

## Further Reading

- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md) — the parameters behind both mechanisms
- [Migrating to Quorum Queues](/contents/RabbitMQMigrateToQuorumQueues.md) — how to move an existing subscription
- [RabbitMQ Connection Stability](/contents/RabbitMQConnectionStability.md) — retry, heartbeats and blocked connections
- [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) — the guarantee persistence does not provide on its own
