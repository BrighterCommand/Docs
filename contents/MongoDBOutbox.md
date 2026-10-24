---
description: "The MongoDB Outbox allows integration between MongoDB and Brighter's outbox support."
layout:
  description:
    visible: false
---

# MongoDB Outbox

> **Reference** · Applies to **Brighter V10**

## MongoDB Outbox Usage
The MongoDB Outbox allows integration between MongoDB and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support).

To support transactional messaging when using MongoDB requires us to use MongoDB's support for ACID transactions. You should understand best practices for using transactions with MongoDB, including replica set requirements for multi-document transactions.

For this we will need the *Outbox* package for MongoDB:

* **Paramore.Brighter.Outbox.MongoDb**

**Paramore.Brighter.Outbox.MongoDb** will pull in another package:

* **Paramore.Brighter.MongoDb**

## MongoDB Outbox NuGet Packages

To use the MongoDB Outbox, you need to install the following packages from NuGet:

```powershell
dotnet add package Paramore.Brighter.Outbox.MongoDb
dotnet add package Paramore.Brighter.MongoDb
```

## MongoDB Outbox Collection Management

The MongoDB Outbox supports different strategies for collection management through the `MakeCollection` property:

- **`OnResolvingACollection.Assume`**: Assumes the collection already exists (default behavior)
- **`OnResolvingACollection.Validate`**: Validates that the collection exists, throws an exception if it doesn't
- **`OnResolvingACollection.CreateIfNotExists`**: Creates the collection if it doesn't exist

**Note:** You are responsible for creating and maintaining the collection if you choose to manage it manually. This includes tasks such as adding indexes to optimize query performance and configuring Time-To-Live (TTL) indexes for automatic message cleanup.

## MongoDB Outbox Options

`MongoDbConfiguration` is constructed with a client and a database name, and carries the rest
as properties. The [MongoDB Inbox](/contents/MongoDBInbox.md) and the
[MongoDB distributed lock](/contents/MongoDbDistributedLock.md) take the same type, so one
instance can configure all three.

<!-- optioncheck: Paramore.Brighter.MongoDb.MongoDbConfiguration
     manual: Client — set from a constructor parameter, so an instance reads back that argument rather than a default
     manual: TimeProvider — initialised to TimeProvider.System, which has no printable value
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `Client` | `IMongoClient` | `none` | Supplies the MongoDB driver client used for every collection Brighter opens. |
| `TimeProvider` | `TimeProvider` | `TimeProvider.System` | Supplies the clock used for message timestamps and time-to-live expiry. |
| `DatabaseSettings` | `MongoDatabaseSettings?` | `null` | Sets the database-level settings, such as read preference and write concern. |
| `InstrumentationOptions` | `InstrumentationOptions` | `All` | Sets how much telemetry detail the Outbox emits. |
| `Outbox` | `MongoDbCollectionConfiguration?` | `null` | Configures the collection this Outbox writes messages to. |
| `Inbox` | `MongoDbCollectionConfiguration?` | `null` | Configures the collection the [MongoDB Inbox](/contents/MongoDBInbox.md) writes to. |
| `Locking` | `MongoDbCollectionConfiguration?` | `null` | Configures the collection the [MongoDB distributed lock](/contents/MongoDbDistributedLock.md) writes its lock documents to. |

**`databaseName` is a constructor parameter, not a property you can set.** It surfaces as the
get-only `DatabaseName`, and there is no default: a `MongoDbConfiguration` cannot be constructed
without it. `Client` has no default either — pass an `IMongoClient`, or use the constructor that
takes a connection string and builds one for you.

**`Outbox` defaults to `null` and the Outbox will not run without it**, so read that `null` as
*you must set this* rather than as *this is optional*.

Each of `Outbox`, `Inbox` and `Locking` takes a `MongoDbCollectionConfiguration`:

<!-- optioncheck: Paramore.Brighter.MongoDb.MongoDbCollectionConfiguration -->

| Option | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | `""` | Names the MongoDB collection. |
| `MakeCollection` | `OnResolvingACollection` | `Assume` | Sets what Brighter does when it resolves the collection — see [Collection Management](#mongodb-outbox-collection-management). |
| `Settings` | `MongoCollectionSettings?` | `null` | Sets the collection-level settings used when the collection is opened. |
| `CreateCollectionOptions` | `CreateCollectionOptions?` | `null` | Supplies the options used when Brighter creates the collection. |
| `TimeToLive` | `TimeSpan?` | `null` | Sets how long a document survives before the TTL index removes it. |

## MongoDB Outbox Configuration

### Basic Configuration

As described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support), we configure Brighter to use an outbox with the `AddProducers` method call.

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MongoDb;

public void ConfigureServices(IServiceCollection services)
{
    // MongoDB connection string
    var connectionString = "mongodb://localhost:27017";
    var databaseName = "BrighterDatabase";
    
    // Configure MongoDB
    var mongoDbConfiguration = new MongoDbConfiguration(connectionString, databaseName)
    {
        Outbox = new MongoDbCollectionConfiguration
        {
            Name = "Outbox",
            MakeCollection = OnResolvingACollection.Validate,
            TimeToLive = TimeSpan.FromDays(7) // Optional: Auto-expire messages after 7 days
        }
    };

    services.AddSingleton<IAmAMongoDbConfiguration>(mongoDbConfiguration);

    services.AddBrighter()
        .AddProducers(producers =>
        {
            producers.Outbox = new MongoDbOutbox(mongoDbConfiguration);
            producers.ConnectionProvider = typeof(MongoDbConnectionProvider);
            producers.TransactionProvider = typeof(MongoDbUnitOfWork);
            
            // Configure your message producers (e.g., for RabbitMQ, Kafka)
            // ...
        })
        .UseOutboxSweeper(); // Optionally add the background sweeper service
}
```

### Advanced Configuration

For more advanced scenarios, you can provide custom MongoDB client settings and collection configurations:

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MongoDb;

public void ConfigureServices(IServiceCollection services)
{
    // Create MongoDB client with custom settings
    var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
    settings.RetryWrites = true;
    settings.RetryReads = true;
    var mongoClient = new MongoClient(settings);
    
    var mongoDbConfiguration = new MongoDbConfiguration(mongoClient, "BrighterDatabase")
    {
        Outbox = new MongoDbCollectionConfiguration
        {
            Name = "Outbox",
            MakeCollection = OnResolvingACollection.CreateIfNotExists,
            TimeToLive = TimeSpan.FromHours(24),
            Settings = new MongoCollectionSettings
            {
                ReadPreference = ReadPreference.Primary,
                WriteConcern = WriteConcern.WMajority
            },
            CreateCollectionOptions = new CreateCollectionOptions
            {
                // Additional collection creation options if needed
            }
        }
    };

    services.AddSingleton<IAmAMongoDbConfiguration>(mongoDbConfiguration);

    services.AddBrighter()
        .AddProducers(producers =>
        {
            producers.Outbox = new MongoDbOutbox(mongoDbConfiguration);
            producers.ConnectionProvider = typeof(MongoDbConnectionProvider);
            producers.TransactionProvider = typeof(MongoDbUnitOfWork);
            
            // Configure your message producers
            // ...
        })
        .UseOutboxSweeper();
}
```

### Using a Connection Provider

You can also use a custom connection provider for more control over the MongoDB connection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MongoDb;

public void ConfigureServices(IServiceCollection services)
{
    var mongoDbConfiguration = new MongoDbConfiguration("mongodb://localhost:27017", "BrighterDatabase")
    {
        Outbox = new MongoDbCollectionConfiguration
        {
            Name = "Outbox",
            MakeCollection = OnResolvingACollection.Validate
        }
    };
    
    var connectionProvider = new MongoDbConnectionProvider(mongoDbConfiguration);

    services.AddSingleton<IAmAMongoDbConfiguration>(mongoDbConfiguration);

    services.AddBrighter()
        .AddProducers(producers =>
        {
            producers.Outbox = new MongoDbOutbox(connectionProvider, mongoDbConfiguration);
            producers.ConnectionProvider = typeof(MongoDbConnectionProvider);
            producers.TransactionProvider = typeof(MongoDbUnitOfWork);
            
            // Configure your message producers
            // ...
        })
        .UseOutboxSweeper();
}
```

## MongoDB Outbox Time-To-Live (TTL) Support

The MongoDB Outbox supports automatic message expiration using MongoDB's TTL feature. You can configure this through the `TimeToLive` property in the collection configuration:

```csharp
Outbox = new MongoDbCollectionConfiguration
{
    Name = "Outbox",
    TimeToLive = TimeSpan.FromDays(30) // Messages will be automatically deleted after 30 days
}
```

When TTL is configured, MongoDB will automatically create a TTL index on the `TimeStamp` field and remove expired documents.

## Using the Outbox in Handlers

In your handler, take a dependency on **IAmAMongoDbTransactionProvider**, which Brighter registers for the `MongoDbUnitOfWork` you set as the `TransactionProvider`, and on the `IAmAMongoDbConfiguration` you registered beside it. Brighter does not register `IAmATransactionConnectionProvider` for MongoDB; that interface is for relational databases, and a handler that asks for it cannot be created.

Pass the provider to `DepositPostAsync` as it is. `IAmAMongoDbTransactionProvider` is an `IAmABoxTransactionProvider<IClientSessionHandle>`, and the `MongoDbOutbox` writes in an `IClientSessionHandle`, so that is the transaction type Brighter configures. It is also the session type the MongoDB driver's collection methods take.

> **Before Brighter 10.8.1**, the interface was an `IAmABoxTransactionProvider<IClientSession>`, so `DepositPostAsync` threw `InvalidOperationException: Supplied transaction provider doesn't match configured transaction type.` unless you cast the provider to `IAmABoxTransactionProvider<IClientSessionHandle>` first.

Start the transaction through the provider, write your documents in its session, and call **DepositPostAsync** with the provider so the message is written to the Outbox in the same transaction. Commit through the provider; if anything fails, roll back and neither your change nor the message is written. Once the transaction has committed, you can call **ClearOutboxAsync** to dispatch the message immediately, or you can rely on the Outbox Sweeper to dispatch it for you.

> **Running more than one instance?** Configure a [distributed lock](/contents/DistributedLock.md) so only one Sweeper (and Archiver) runs at a time — see [MongoDB Distributed Lock](/contents/MongoDbDistributedLock.md).

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Paramore.Brighter;
using Paramore.Brighter.MongoDb;

public class AddGreetingHandler(
    IAmAMongoDbTransactionProvider transactionProvider,
    IAmAMongoDbConfiguration configuration,
    IAmACommandProcessor postBox,
    ILogger<AddGreetingHandler> logger) : RequestHandlerAsync<AddGreeting>
{
    public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();

        var session = await transactionProvider.GetTransactionAsync(cancellationToken);

        try
        {
            var people = transactionProvider.Client
                .GetDatabase(configuration.DatabaseName)
                .GetCollection<Person>("People");

            // Update the person document within the transaction
            await people.UpdateOneAsync(session,
                Builders<Person>.Filter.Eq(p => p.Name, addGreeting.Name),
                Builders<Person>.Update.Push(p => p.Greetings, addGreeting.Greeting),
                cancellationToken: cancellationToken);

            // Now write the message we want to send to the Outbox in the same transaction
            posts.Add(await postBox.DepositPostAsync(
                new GreetingMade(addGreeting.Greeting),
                transactionProvider,
                cancellationToken: cancellationToken));

            // Commit both the entity change and the outgoing message
            await transactionProvider.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception thrown handling Add Greeting request");
            // It went wrong, rollback the entity change and the downstream message
            await transactionProvider.RollbackAsync(cancellationToken);
            return await base.HandleAsync(addGreeting, cancellationToken);
        }
        finally
        {
            transactionProvider.Close();
        }

        // Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
        // Alternatively, you can let the Sweeper do this, but at the cost of increased latency
        await postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
```

## MongoDB Outbox Message Structure

The MongoDB Outbox stores messages as BSON documents with the following structure:

- **MessageId**: The unique identifier for the message
- **Topic**: The topic/destination for the message
- **MessageType**: The type of the message
- **TimeStamp**: When the message was created (UTC). Used for the TTL index
- **CorrelationId**: Optional correlation identifier
- **ReplyTo**: Optional reply-to address
- **ContentType**: The content type of the message
- **PartitionKey**: Optional partition key for message routing
- **HeaderBag**: JSON serialized message headers
- **Body**: The message body as bytes
- **Dispatched**: Timestamp when the message was dispatched (null if not yet dispatched)

Here is an example of a message document stored in the outbox collection:

```json
{
  "_id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "MessageId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "Topic": "greeting.made",
  "MessageType": "MyApp.Events.GreetingMade",
  "TimeStamp": { "$date": "2025-09-22T10:00:00.000Z" },
  "CorrelationId": "correlation-123",
  "ReplyTo": null,
  "ContentType": "application/json",
  "PartitionKey": null,
  "HeaderBag": "{\"MessageId\":\"a1b2c3d4-e5f6-7890-1234-567890abcdef\"}",
  "Body": { "$binary": { "base64": "eyJtZXNzYWdlIjoiSGVsbG8gV29ybGQifQ==", "subType": "00" } },
  "Dispatched": null
}
```

## MongoDB Outbox Error Handling

The MongoDB Outbox will throw appropriate exceptions for various error conditions:

- **ArgumentException**: When required configuration is missing
- **MongoException**: For MongoDB-specific errors like connection failures or transaction conflicts
- **InvalidOperationException**: For outbox-specific errors like attempting operations without proper transaction context

## Transaction Requirements

MongoDB transactions require:
- MongoDB 4.0+ for replica sets
- MongoDB 4.2+ for sharded clusters
- Replica set deployment (transactions don't work on standalone instances)

Make sure your MongoDB deployment supports transactions before using the MongoDB Outbox pattern.
