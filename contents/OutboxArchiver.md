---
description: "The Outbox Archiver is a background service that monitors an Outbox and moves messages older than a certain age into long-term storage, keeping your Outbox small."
layout:
  description:
    visible: false
---

# Outbox Archiver

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](/contents/BrighterOutboxSupport.md)

The **Outbox Archiver** is a background service that monitors an **Outbox** and moves messages older than a certain age into long-term storage, keeping your Outbox small. It is the clean-out stage of the Outbox life-cycle: the [Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) dispatches messages, and the Archiver later retires the ones that have been sent.

Like the Sweeper, the Archiver should run as a **singleton** — only one Archiver per Outbox at a time. It shares the same [distributed lock](/contents/DistributedLock.md) mechanism, taking a lock on the resource named `"Archiver"`, so the same configured `opt.DistributedLock` coordinates both the Sweeper and the Archiver.

You register the Archiver with `UseOutboxArchiver<TTransaction>`, passing an archive provider (an `IAmAnArchiveProvider`) and options. For a ready-made archive provider, see the [Azure Archive Provider](/contents/AzureBlobArchiveProvider.md).

```csharp
// ...
.UseOutboxArchiver<TransactWriteItemsRequest>(
    archiveProvider,
    opt =>
    {
        opt.MinimumAge = TimeSpan.FromHours(24);   // archive messages dispatched over a day ago
        opt.ArchiveBatchSize = 100;
    });
```

`TTransaction` is the **transaction type** your Outbox writes to — the same type Brighter wraps in a transaction when it stores a message — not the transaction *provider* you register on `opt.ConnectionProvider`/`opt.TransactionProvider`. Use the type from the column below for your store:

| Outbox | `TTransaction` to use | Namespace |
| --- | --- | --- |
| SQL Server, PostgreSQL, MySQL, SQLite, Spanner | `DbTransaction` | `System.Data.Common` |
| DynamoDB | `TransactWriteItemsRequest` | `Amazon.DynamoDBv2.Model` |
| MongoDB | `IClientSessionHandle` | `MongoDB.Driver` |
| Firestore | `FirestoreTransaction` | `Paramore.Brighter.Firestore` |

For example, a DynamoDB Outbox uses `UseOutboxArchiver<TransactWriteItemsRequest>`. The `DynamoDbUnitOfWork` you register as the provider is *not* a transaction type, so `UseOutboxArchiver<DynamoDbUnitOfWork>` does not compile.

## Timed Outbox Archiver Options

The second argument to `UseOutboxArchiver<TTransaction>` configures the Archiver with a
`TimedOutboxArchiverOptions`:

<!-- optioncheck: Paramore.Brighter.Outbox.Hosting.TimedOutboxArchiverOptions -->

| Option | Type | Default | Description |
|---|---|---|---|
| `TimerInterval` | `int` | `15` | How many seconds the Archiver waits between checks for messages eligible for archival. |
| `MinimumAge` | `TimeSpan` | `86400000 ms` | How long since a message was dispatched before it becomes eligible for archival. |
| `ArchiveBatchSize` | `int` | `100` | How many messages the Archiver moves to the archive provider in each check. |
| `Instrumentation` | `InstrumentationOptions` | `All` | How much telemetry detail the Archiver emits. |

## Running the Sweeper and Archiver Out of Process

Running the Sweeper or Archiver on a background thread inside your producer application is fine for development, but in production that thread competes with your application for scheduling, and you have to take care that only one instance runs. At scale, the cleaner option is a **dedicated worker executable** that hosts only the Sweeper and Archiver, configured with the same external Outbox and a [distributed lock](/contents/DistributedLock.md). You then schedule it with your container orchestrator (Kubernetes or similar).

The worker below uses DynamoDB; swap the three fenced lines for your own database, Outbox, and lock provider.

```csharp
// ...
var dynamoDb = new AmazonDynamoDBClient();
IAmAnArchiveProvider archiveProvider = /* your archive provider, e.g. an S3/blob archive */;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton<IAmazonDynamoDB>(dynamoDb)
    .AddBrighter()
    .AddProducers(opt =>
    {
        // ---- swap these for your backend ----
        opt.Outbox = new DynamoDbOutbox(dynamoDb, new DynamoDbConfiguration { /* ... */ });
        opt.ConnectionProvider = typeof(DynamoDbUnitOfWork);
        opt.TransactionProvider = typeof(DynamoDbUnitOfWork);
        opt.DistributedLock = new DynamoDbLockingProvider(
            dynamoDb, new DynamoDbLockingProviderOptions("brighter-locks", "sweeper-group"));
        // --------------------------------------
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; })
    // TTransaction is the Outbox's transaction type — TransactWriteItemsRequest for DynamoDB
    // (see the transaction-type table above for other stores).
    .UseOutboxArchiver<TransactWriteItemsRequest>(
        archiveProvider, opt => { opt.MinimumAge = TimeSpan.FromHours(24); });

var host = builder.Build();
await host.RunAsync();
```

Because the distributed lock guarantees a single active Sweeper and Archiver, you do **not** have to pin the deployment to a single replica. You can run several replicas for resilience, and the lock ensures only one does the work at a time:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: brighter-outbox-worker
spec:
  replicas: 2          # safe: the distributed lock keeps a single Sweeper/Archiver active
  selector:
    matchLabels:
      app: brighter-outbox-worker
  template:
    metadata:
      labels:
        app: brighter-outbox-worker
    spec:
      containers:
        - name: worker
          image: your-registry/brighter-outbox-worker:latest
```

## InMemory Archive

The InMemory Archive stores dispatched messages in memory for diagnostics and replay.

### When to Use the InMemory Archive

**Perfect for**:

- Testing message archiving
- Development and debugging
- Inspecting sent messages in tests

**Not recommended for production** due to unbounded memory growth.

### InMemory Archive Configuration

```csharp
// ...
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.UseOutboxArchiver(new InMemoryArchiveProvider())
.AddProducers(/* producer configuration */);
```

### InMemory Archive Example Usage

```csharp
// ...
public class LargeMessageMapper : IAmAMessageMapper<LargeDataCommand>
{
    private readonly IAmAStorageProviderAsync _storageProvider;

    [ClaimCheck(0, thresholdInKb: 5)]  // Store payloads > 5KB
    public Message MapToMessage(LargeDataCommand request)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: new RoutingKey("LargeData"),
            messageType: MessageType.MT_COMMAND
        );

        var body = new MessageBody(JsonSerializer.Serialize(request));
        return new Message(header, body);
    }
}
```

## Further Reading

- [Outbox Support](/contents/BrighterOutboxSupport.md) - The Outbox pattern in Brighter, and the Sweeper
- [Azure Blob Archive Provider](/contents/AzureBlobArchiveProvider.md) - A ready-made archive provider
- [Distributed Lock](/contents/DistributedLock.md) - The lock that keeps a single Archiver active
- [Database Provisioning](/contents/BoxProvisioning.md) - Creating the Outbox table
