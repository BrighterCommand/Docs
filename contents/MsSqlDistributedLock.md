# MS SQL Distributed Lock

> **Reference** · Applies to **Brighter V10**

The MS SQL locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using SQL Server **application locks**
(`sp_getapplock` / `sp_releaseapplock`), so a single [Outbox
Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver run when you
scale out. It pairs naturally with the [MSSQL Outbox](/contents/MSSQLOutbox.md).

## Package

* **Paramore.Brighter.Locking.MsSql**

Application locks are a built-in SQL Server feature, so there is **no table to
provision** — see [Provisioning](#provisioning).

## Configuration

Unlike the lease-based providers, the MS SQL provider does not take an options class.
You construct it with an `MsSqlConnectionProvider`, which it uses to open the session
that holds the application lock. The connection provider is built from an
`IAmARelationalDatabaseConfiguration` (a `RelationalDatabaseConfiguration`) carrying your
connection string:

```csharp
var configuration = new RelationalDatabaseConfiguration(
    connectionString: "Server=localhost;Database=orders;Trusted_Connection=True;");

var lockingProvider = new MsSqlLockingProvider(new MsSqlConnectionProvider(configuration));
```

There are no lease settings to tune — the lock is held for the lifetime of the database
session.

## Example

```csharp
var configuration = new RelationalDatabaseConfiguration(
    "Server=localhost;Database=orders;Trusted_Connection=True;");

services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your MS SQL Outbox */;
        opt.ConnectionProvider = typeof(MsSqlConnectionProvider);
        opt.TransactionProvider = typeof(MsSqlTransactionProvider);

        opt.DistributedLock = new MsSqlLockingProvider(new MsSqlConnectionProvider(configuration));
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## Provisioning

None. SQL Server application locks live in the server and are scoped to the session that
acquires them, so the provider needs no table or schema. The lock acquires in
`Exclusive` mode at `Session` scope and is released when the session ends — including if
the holding instance crashes and its connection drops.

## Notes

- Because the lock is tied to a session rather than a timed lease, there is no
  `LeaseValidity` to tune.
- Point the provider at the same SQL Server instance as your Outbox so the Sweeper,
  Archiver, and lock share infrastructure.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [MSSQL Outbox](/contents/MSSQLOutbox.md) — the matching Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
