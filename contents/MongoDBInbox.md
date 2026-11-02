---
description: "The MongoDB Inbox allows use of MongoDB for Brighter's inbox support."
layout:
  description:
    visible: false
---

# MongoDB Inbox

> **Reference** · Applies to **Brighter V10**

## MongoDB Inbox Usage
The MongoDB Inbox allows use of MongoDB for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox).

For this we will need the *Inbox* packages for the MongoDB *Inbox*.

* **Paramore.Brighter.Inbox.MongoDb**
* **Paramore.Brighter.MongoDb**

## MongoDB Inbox Configuration

### Basic Configuration

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.MongoDb;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        });

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    // MongoDB connection string
    var connectionString = "mongodb://localhost:27017";
    var databaseName = "BrighterDatabase";
    
    // Configure MongoDB
    var mongoDbConfiguration = new MongoDbConfiguration(connectionString, databaseName)
    {
        Inbox = new MongoDbCollectionConfiguration
        {
            Name = "Inbox",
            MakeCollection = OnResolvingACollection.Validate,
            TimeToLive = TimeSpan.FromDays(7) // Optional: see Time-To-Live (TTL) Support below
        }
    };

    services.AddConsumers(options =>
    {
        options.InboxConfiguration = new InboxConfiguration(
            new MongoDbInbox(mongoDbConfiguration),
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Throw
        );
    });
}
```

Before Brighter 10.8.0 this configuration took effect only in an application that also called `AddProducers`; see [Global Inbox Configuration in a Consumer-Only Application](/contents/BrighterInboxSupport.md#global-inbox-configuration-in-a-consumer-only-application).

### Advanced Configuration

For more advanced scenarios, you can provide custom MongoDB client settings and collection configurations:

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.MongoDb;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    // Create MongoDB client with custom settings
    var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
    settings.RetryWrites = true;
    settings.RetryReads = true;
    var mongoClient = new MongoClient(settings);
    
    var mongoDbConfiguration = new MongoDbConfiguration(mongoClient, "BrighterDatabase")
    {
        Inbox = new MongoDbCollectionConfiguration
        {
            Name = "Inbox",
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

    services.AddConsumers(options =>
    {
        options.InboxConfiguration = new InboxConfiguration(
            new MongoDbInbox(mongoDbConfiguration),
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Throw
        );
    });
}
```

### Using a Connection Provider

You can also use a custom connection provider for more control over the MongoDB connection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.MongoDb;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var mongoDbConfiguration = new MongoDbConfiguration("mongodb://localhost:27017", "BrighterDatabase")
    {
        Inbox = new MongoDbCollectionConfiguration
        {
            Name = "Inbox",
            MakeCollection = OnResolvingACollection.Validate
        }
    };
    
    var connectionProvider = new MongoDbConnectionProvider(mongoDbConfiguration);

    services.AddConsumers(options =>
    {
        options.InboxConfiguration = new InboxConfiguration(
            new MongoDbInbox(connectionProvider, mongoDbConfiguration),
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Throw
        );
    });
}
```

## MongoDB Inbox Collection Management

The MongoDB Inbox supports different strategies for collection management through the `MakeCollection` property:

- **`OnResolvingACollection.Assume`**: Assumes the collection already exists (default behavior)
- **`OnResolvingACollection.Validate`**: Validates that the collection exists, throws an exception if it doesn't
- **`OnResolvingACollection.CreateIfNotExists`**: Creates the collection if it doesn't exist

## MongoDB Inbox Time-To-Live (TTL) Support

`TimeToLive` asks for a MongoDB TTL index on the `TimeStamp` field. At Brighter 10.8.1 that index expires nothing, so if you want MongoDB to remove old entries, create the index yourself:

- Brighter creates its index only when it creates the collection, which it does only with `MakeCollection = OnResolvingACollection.CreateIfNotExists`. Under the default, `Assume`, there is no index.
- The index it creates, `brighter_ttl_Inbox` on `TimeStamp`, removes no document. Brighter stores `TimeStamp` as an embedded document of `DateTime`, `Ticks` and `Offset`, and MongoDB expires a document only when the indexed field holds a date.

A TTL index on `TimeStamp.DateTime` does expire them:

```csharp
using System;
using MongoDB.Bson;
using MongoDB.Driver;

var inbox = new MongoClient("mongodb://localhost:27017")
    .GetDatabase("BrighterDatabase")
    .GetCollection<BsonDocument>("Inbox");

// Remove each document 30 days after its TimeStamp
inbox.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
    Builders<BsonDocument>.IndexKeys.Ascending("TimeStamp.DateTime"),
    new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) }));
```

MongoDB's TTL monitor runs once a minute, so a document can outlive its time-to-live by up to a minute.

## MongoDB Inbox Message Structure

The MongoDB Inbox stores messages with the following structure:

- **Id**: Composite key containing the command ID and context key
- **TimeStamp**: When the message was created, as an embedded document of `DateTime`, `Ticks` and `Offset`
- **CommandType**: The full type name of the command
- **CommandBody**: JSON serialized command data
- **ExpireAfterSeconds**: The collection's `TimeToLive` in seconds, written onto each document; MongoDB does not act on it

## MongoDB Inbox Error Handling

The MongoDB Inbox will throw appropriate exceptions for various error conditions:

- **ArgumentException**: When required configuration is missing
- **InboxException**: For inbox-specific errors like duplicate messages when `OnceOnlyAction.Throw` is configured

## Dependencies

The MongoDB Inbox requires the following NuGet packages:

````powershell
dotnet add package Paramore.Brighter.Inbox.MongoDb
dotnet add package Paramore.Brighter.MongoDb
````
