---
description: "The MySQL locking provider implements Brighter's distributed lock using MySQL's named locks (GET_LOCK / RELEASE_LOCK), so a single Outbox Sweeper and Archiver run when you scale out."
layout:
  description:
    visible: false
---

# MySQL Distributed Lock

> **Reference** · Applies to **Brighter V10**

The MySQL locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using MySQL's named locks (`GET_LOCK` /
`RELEASE_LOCK`), so a single [Outbox
Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when you
scale out. It pairs naturally with the [MySQL Outbox](/contents/MySQLOutbox.md).

## MySQL Distributed Lock Package

* **Paramore.Brighter.Locking.MySql**

Named locks are a built-in MySQL feature, so there is **no table to provision** — see
[Provisioning](#mysql-distributed-lock-provisioning).

## MySQL Distributed Lock Configuration

**Like the MS SQL provider, the MySQL provider has no options type at all**, so there is no
table of settings on this page. You construct it with a `MySqlConnectionProvider`, which it
uses to open the session that holds the lock. The connection provider is built from an
`IAmARelationalDatabaseConfiguration` (a `RelationalDatabaseConfiguration`) carrying your
connection string, and its options are documented once in the [Relational Database
Configuration Reference](/contents/RelationalDatabaseConfigurationReference.md):

```csharp
var configuration = new RelationalDatabaseConfiguration(
    connectionString: "Server=localhost;Database=orders;Uid=app;Pwd=secret;");

var lockingProvider = new MySqlLockingProvider(new MySqlConnectionProvider(configuration));
```

There are no lease settings to tune — the lock is held for the lifetime of the database
session.

## MySQL Distributed Lock Example

```csharp
var configuration = new RelationalDatabaseConfiguration(
    "Server=localhost;Database=orders;Uid=app;Pwd=secret;");

services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your MySQL Outbox */;
        opt.ConnectionProvider = typeof(MySqlConnectionProvider);
        opt.TransactionProvider = typeof(MySqlTransactionProvider);

        opt.DistributedLock = new MySqlLockingProvider(new MySqlConnectionProvider(configuration));
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## MySQL Distributed Lock Provisioning

None. MySQL named locks are held in the server and scoped to the session that acquires
them, so the provider needs no table or schema. The lock is released when the session
ends — including if the holding instance crashes and its connection drops.

## MySQL Distributed Lock Notes

- MySQL limits lock names to 64 characters, so the provider hashes the resource name
  (SHA-512, truncated) to produce a valid, stable lock name. You do not need to do
  anything for this.
- Acquisition uses a one-second timeout: if another instance holds the lock, the attempt
  returns without acquiring and the Sweeper/Archiver abandons that cycle.
- Because the lock is tied to a session rather than a timed lease, there is no
  `LeaseValidity` to tune.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [MySQL Outbox](/contents/MySQLOutbox.md) — the matching Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
