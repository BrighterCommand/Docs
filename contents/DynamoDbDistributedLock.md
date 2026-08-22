---
description: "The DynamoDB locking provider implements Brighter's distributed lock on top of Amazon DynamoDB, so a single Outbox Sweeper and Archiver run when you scale out."
layout:
  description:
    visible: false
---

# DynamoDB Distributed Lock

> **Reference** · Applies to **Brighter V10**

The DynamoDB locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) on top of Amazon DynamoDB, so a single [Outbox
Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when you
scale out. It is a natural fit when you already use the [DynamoDB
Outbox](/contents/DynamoOutbox.md).

## DynamoDB Distributed Lock Package

Add the locking package for AWS SDK v4:

* **Paramore.Brighter.Locking.DynamoDB.V4**

Prefer the **V4** package for new projects. The earlier `Paramore.Brighter.Locking.DynamoDB`
package targets AWS SDK v3, which is out of support on AWS, and exists only to aid
migration.

The provider stores its locks in a DynamoDB table that you must create in advance (see
[Provisioning](#dynamodb-distributed-lock-provisioning)).

## DynamoDB Distributed Lock Configuration

Configure the provider with `DynamoDbLockingProvider`, passing your `IAmazonDynamoDB`
client and a `DynamoDbLockingProviderOptions`:

```csharp
new DynamoDbLockingProvider(
    dynamoDb,
    new DynamoDbLockingProviderOptions(
        lockTableName: "brighter-locks",
        leaseholderGroupId: "sweeper-group"));
```

`DynamoDbLockingProviderOptions` takes two required values in its constructor and exposes
two optional settings:

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LockTableName` | `string` | *(required)* | The DynamoDB table that holds the locks. |
| `LeaseholderGroupId` | `string` | *(required)* | Identifies the group of instances that share the lock. All instances that must coordinate use the same value. |
| `LeaseValidity` | `TimeSpan` | 1 minute | How long the lease is held before it expires automatically. Set it comfortably longer than a Sweeper/Archiver cycle. |
| `ManuallyReleaseLock` | `bool` | `false` | When `false`, the lock simply expires after `LeaseValidity`; when `true`, it is released explicitly on completion. |

## DynamoDB Distributed Lock Example

```csharp
var dynamoDb = new AmazonDynamoDBClient();

services
    .AddSingleton<IAmazonDynamoDB>(dynamoDb)
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = new DynamoDbOutbox(dynamoDb, new DynamoDbConfiguration { /* ... */ });
        opt.ConnectionProvider = typeof(DynamoDbUnitOfWork);
        opt.TransactionProvider = typeof(DynamoDbUnitOfWork);

        opt.DistributedLock = new DynamoDbLockingProvider(
            dynamoDb,
            new DynamoDbLockingProviderOptions("brighter-locks", "sweeper-group")
            {
                LeaseValidity = TimeSpan.FromMinutes(2)
            });
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## DynamoDB Distributed Lock Provisioning

The lock table must exist before the provider runs. Create a table whose partition key
matches the provider's lock items, with the name you pass as `LockTableName`. If you
already provision a DynamoDB Outbox table, provision the lock table the same way (for
example with the AWS SDK, CDK, or Terraform). Consider enabling DynamoDB's
[time-to-live](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/TTL.html)
feature so expired lock items are cleaned up automatically.

## DynamoDB Distributed Lock Notes

- Use the same `LeaseholderGroupId` for every instance that must share the lock; a
  different value creates an independent lock.
- Keep `LeaseValidity` longer than a typical Sweeper or Archiver batch so the lease does
  not expire mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [DynamoDB Outbox](/contents/DynamoOutbox.md) — the matching Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
