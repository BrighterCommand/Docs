# Postgres Distributed Lock

> **Reference** · Applies to **Brighter V10**

The Postgres locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using PostgreSQL **advisory locks**, so a single
[Outbox Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when
you scale out. It pairs naturally with the [Postgres Outbox](/contents/PostgresOutbox.md).

## Package

* **Paramore.Brighter.Locking.PostgresSql**

Advisory locks are a built-in PostgreSQL feature, so there is **no table to provision** —
see [Provisioning](#provisioning).

## Configuration

Configure the provider with `PostgresLockingProvider`, passing a
`PostgresLockingProviderOptions` that carries the connection string:

```csharp
new PostgresLockingProvider(
    new PostgresLockingProviderOptions(
        connectionString: "Host=localhost;Database=orders;Username=app;Password=secret"));
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ConnectionString` | `string` | *(required)* | The connection string used to open the session that holds the advisory lock. |

## Example

```csharp
const string connectionString = "Host=localhost;Database=orders;Username=app;Password=secret";

services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your Postgres Outbox */;
        opt.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        opt.TransactionProvider = typeof(PostgreSqlTransactionProvider);

        opt.DistributedLock = new PostgresLockingProvider(
            new PostgresLockingProviderOptions(connectionString));
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## Provisioning

None. PostgreSQL advisory locks are session-scoped locks that live in the server's
memory, so the provider needs no table or schema. The lock is held for the lifetime of
the database session and is released when that session closes — including if the holding
instance crashes and its connection drops.

## Notes

- Because the lock is tied to a session rather than a timed lease, there is no
  `LeaseValidity` to tune. Recovery after a crash happens as soon as the dropped
  connection is detected by the server.
- Point the provider at the same PostgreSQL instance as your Outbox so the Sweeper,
  Archiver, and lock all share infrastructure.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [Postgres Outbox](/contents/PostgresOutbox.md) — the matching Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
