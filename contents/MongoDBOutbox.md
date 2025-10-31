# MongoDB Outbox

## Usage
The MongoDB Outbox allows integration between MongoDB and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support).

To support transactional messaging when using MongoDB requires us to use MongoDB's support for ACID transactions. You should understand best practices for using transactions with MongoDB, including replica set requirements for multi-document transactions.

For this we will need the *Outbox* package for MongoDB:

* **Paramore.Brighter.Outbox.MongoDb**

**Paramore.Brighter.Outbox.MongoDb** will pull in another package:

* **Paramore.Brighter.MongoDb**

## NuGet Packages

To use the MongoDB Outbox, you need to install the following packages from NuGet:

```powershell
dotnet add package Paramore.Brighter.Outbox.MongoDb
dotnet add package Paramore.Brighter.MongoDb
```

## Collection Management

The MongoDB Outbox supports different strategies for collection management through the `MakeCollection` property:

- **`OnResolvingACollection.Assume`**: Assumes the collection already exists (default behavior)
- **`OnResolvingACollection.Validate`**: Validates that the collection exists, throws an exception if it doesn't
- **`OnResolvingACollection.Create`**: Creates the collection if it doesn't exist

**Note:** You are responsible for creating and maintaining the collection if you choose to manage it manually. This includes tasks such as adding indexes to optimize query performance and configuring Time-To-Live (TTL) indexes for automatic message cleanup.

## Configuration

### Basic Configuration

As described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support), we configure Brighter to use an outbox with the `AddProducers` method call.

```csharp
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
public void ConfigureServices(IServiceCollection services)
{
    // Create MongoDB client with custom settings
    var mongoClient = new MongoClient(new MongoClientSettings
    {
        ConnectionString = new ConnectionString("mongodb://localhost:27017"),
        RetryWrites = true,
        RetryReads = true
    });
    
    var mongoDbConfiguration = new MongoDbConfiguration(mongoClient, "BrighterDatabase")
    {
        Outbox = new MongoDbCollectionConfiguration
        {
            Name = "Outbox",
            MakeCollection = OnResolvingACollection.Create,
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

## Time-To-Live (TTL) Support

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

In your handler, you take a dependency on Brighter's **IAmATransactionConnectionProvider** interface and convert it to a **MongoDbUnitOfWork**. You explicitly start a transaction within the handler and use MongoDB sessions for transactional consistency.

You call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed, you can call **ClearOutboxAsync** to immediately clear, or you can rely on the Outbox Sweeper to clear for you.

```csharp
public class AddGreetingHandler : RequestHandlerAsync<AddGreeting>
{
    private readonly IAmATransactionConnectionProvider _connectionProvider;
    private readonly IAmACommandProcessor _postBox;
    private readonly ILogger<AddGreetingHandler> _logger;

    public AddGreetingHandler(
        IAmATransactionConnectionProvider connectionProvider,
        IAmACommandProcessor postBox,
        ILogger<AddGreetingHandler> logger)
    {
        _connectionProvider = connectionProvider;
        _postBox = postBox;
        _logger = logger;
    }

    public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
    {
        var posts = new List<Guid>();

        // We use the unit of work to grab connection and session, because Outbox needs
        // to share them 'behind the scenes'
        var unitOfWork = _connectionProvider as MongoDbUnitOfWork;
        var database = unitOfWork.Database;
        var session = unitOfWork.Session;
        
        try
        {
            session.StartTransaction();
            
            var collection = database.GetCollection<Person>("People");
            
            // Find and update the person within the transaction
            var person = await collection.Find(session, p => p.Name == addGreeting.Name)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (person != null)
            {
                person.Greetings.Add(addGreeting.Greeting);
                
                // Update the person document within the transaction
                await collection.ReplaceOneAsync(session, 
                    p => p.Id == person.Id, 
                    person, 
                    cancellationToken: cancellationToken);
            }

            // Now write the message we want to send to the Outbox in the same transaction
            posts.Add(await _postBox.DepositPostAsync(
                new GreetingMade(addGreeting.Greeting), 
                cancellationToken: cancellationToken));

            // Commit both the entity change and the outgoing message
            await session.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception e)
        {   
            _logger.LogError(e, "Exception thrown handling Add Greeting request");
            // It went wrong, rollback the entity change and the downstream message
            await session.AbortTransactionAsync(cancellationToken);
            return await base.HandleAsync(addGreeting, cancellationToken);
        }

        // Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
        // Alternatively, you can let the Sweeper do this, but at the cost of increased latency
        await _postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
```

## Message Structure

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

## Error Handling

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
