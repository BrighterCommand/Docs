# MongoDB Inbox

## Usage
The MongoDB Inbox allows use of MongoDB for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#inbox).

For this we will need the *Inbox* packages for the MongoDB *Inbox*.

* **Paramore.Brighter.Inbox.MongoDb**
* **Paramore.Brighter.MongoDb**

## Configuration

### Basic Configuration

```csharp
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
            TimeToLive = TimeSpan.FromDays(7) // Optional: Auto-expire messages after 7 days
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

### Advanced Configuration

For more advanced scenarios, you can provide custom MongoDB client settings and collection configurations:

```csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
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
        Inbox = new MongoDbCollectionConfiguration
        {
            Name = "Inbox",
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

## Collection Management

The MongoDB Inbox supports different strategies for collection management through the `MakeCollection` property:

- **`OnResolvingACollection.Assume`**: Assumes the collection already exists (default behavior)
- **`OnResolvingACollection.Validate`**: Validates that the collection exists, throws an exception if it doesn't
- **`OnResolvingACollection.Create`**: Creates the collection if it doesn't exist

## Time-To-Live (TTL) Support

The MongoDB Inbox supports automatic message expiration using MongoDB's TTL feature. You can configure this through the `TimeToLive` property in the collection configuration:

```csharp
Inbox = new MongoDbCollectionConfiguration
{
    Name = "Inbox",
    TimeToLive = TimeSpan.FromDays(30) // Messages will be automatically deleted after 30 days
}
```

When TTL is configured, MongoDB will automatically create a TTL index on the `TimeStamp` field and remove expired documents.

## Message Structure

The MongoDB Inbox stores messages with the following structure:

- **Id**: Composite key containing the command ID and context key
- **TimeStamp**: When the message was created
- **CommandType**: The full type name of the command
- **CommandBody**: JSON serialized command data
- **ExpireAfterSeconds**: Optional expiration time for individual messages

## Error Handling

The MongoDB Inbox will throw appropriate exceptions for various error conditions:

- **ArgumentException**: When required configuration is missing
- **InboxException**: For inbox-specific errors like duplicate messages when `OnceOnlyAction.Throw` is configured

## Dependencies

The MongoDB Inbox requires the following NuGet packages:

````powershell
dotnet add package Paramore.Brighter.Inbox.MongoDb
dotnet add package Paramore.Brighter.MongoDb
````