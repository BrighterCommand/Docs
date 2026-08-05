# Migrating to Quorum Queues

> **How-to** · Applies to **Brighter V10** · Prerequisites: [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)

This guide moves an existing Brighter subscription from a classic queue to a quorum
queue, and enables message persistence alongside it. It assumes you have already
decided that you want quorum queues — if you have not, read
[RabbitMQ Durability](/contents/RabbitMQDurability.md) first.

## Before You Migrate to Quorum Queues

Confirm all four of these before you start. The first three are enforced by Brighter
at queue creation; the fourth is not enforced anywhere and is the one that quietly
costs you the guarantee you are migrating for.

1. **RabbitMQ 3.8 or later**, since that is the release that introduced quorum queues.
2. **`isDurable: true` and `highAvailability: false`** on the subscription. Brighter
   throws during queue creation if either is wrong — see
   [RabbitMQ Quorum Queue Requirements](/contents/RabbitMQConfiguration.md#rabbitmq-quorum-queue-requirements)
   for the full validation rules.
3. **A window in which you can run two subscriptions at once**, because the migration
   below is a drain rather than a switch.
4. **At least three nodes in the cluster.** Nothing will stop you creating a quorum
   queue on a single node; it will simply pay the Raft overhead without providing the
   fault tolerance.

## Migration from Classic to Quorum Queues

To migrate an existing subscription from classic to quorum queues:

1. **Create a new quorum queue** with a different name
2. **Update producers** to publish to the new queue
3. **Deploy new consumers** subscribing to the quorum queue
4. **Drain the classic queue** by processing remaining messages
5. **Remove the classic queue** once drained

Do not attempt to change a classic queue to a quorum queue in place, as this requires deleting and recreating the queue, which would result in message loss.

## Draining the Classic Queue Safely

Step 4 above is the step with a failure mode. Brighter acknowledges a message only
once its handler chain has completed, and it holds messages in a per-thread buffer
before they reach that chain — so shutting a consumer down mid-drain returns the
buffered messages to the classic queue rather than losing them, but it also means the
queue is not empty just because the consumer has stopped reporting work.

Check the queue depth in the RabbitMQ Management UI, not the consumer's logs, before
you delete anything. The full semantics are in
[RabbitMQ Ack and Nack Behaviour](/contents/RabbitMQConfiguration.md#rabbitmq-ack-and-nack-behaviour).

## Enabling Persistent Messages

Quorum queues require durable definitions, so most migrations enable message
persistence at the same time. To enable message persistence, set `PersistMessages = true` in your `RmqMessagingGatewayConnection`:

```csharp
// ...
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange", durable: true),
    PersistMessages = true  // Enable message persistence
};
```

Two follow-up steps are worth doing while you are here:

1. **Set a time-to-live on the messages.** Persistent messages that nothing consumes
   accumulate on disk indefinitely; a TTL bounds that without needing an operator to
   intervene.
2. **Test a broker restart.** Persistence is only as good as the recovery you have
   actually exercised, so restart the broker with messages in flight and confirm they
   are still there afterwards. Do this on a schedule, not once at migration time.

For the producer- and consumer-side settings that persistence needs, see
[RabbitMQ Persistence Options](/contents/RabbitMQConfiguration.md#rabbitmq-persistence-options).

## Further Reading

- [RabbitMQ Durability](/contents/RabbitMQDurability.md) — why you would choose quorum queues or persistence
- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md) — the parameters this guide sets
- [RabbitMQ Connection Stability](/contents/RabbitMQConnectionStability.md) — retry, heartbeats and blocked connections
