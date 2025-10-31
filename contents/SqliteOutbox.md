# **Using the SQLite Outbox**

The SQLite Outbox provides a message store for the [Transactional Outbox pattern](/contents/BrighterOutboxSupport.md) using a SQLite database. This ensures that messages are saved within the same transaction as your business logic and published to a message broker later.

## **NuGet Packages**

To use the SQLite Outbox, you need to install the following packages from NuGet. If you are using Entity Framework Core, you will also need the EF Core integration package.

```powershell
Install-Package Paramore.Brighter.Sqlite
Install-Package Paramore.Brighter.Outbox.Sqlite
```

For Entity Framework Core support:
```powershell
Install-Package Paramore.Brighter.Sqlite.EntityFrameworkCore
```

## **Database Table Schema**

The SQLite Outbox requires a specific table in your database to store messages before they are dispatched. You can generate the necessary SQL Data Definition Language (DDL) script to create this table using the `SqliteOutboxBuilder` helper class.

**Note:** You are responsible for creating and maintaining this table. This includes tasks such as adding indexes to optimize query performance and managing schema migrations when updating to new versions of Brighter that may require additional columns.

### **Generating the DDL**

The `SqliteOutboxBuilder.GetDDL()` method creates the SQL script for you. You can execute this script against your database to create the outbox table.

```csharp
// The table name can be whatever you choose.
string tableName = "Outbox"; 

// The DDL for a table that stores the message body as TEXT
string ddl = SqliteOutboxBuilder.GetDDL(tableName);

// The DDL for a table that stores the message body as BLOB
// Useful if your message body is binary
string binaryDdl = SqliteOutboxBuilder.GetDDL(tableName, hasBinaryMessagePayload: true);
```

### **Example SQL Script**

Running `SqliteOutboxBuilder.GetDDL("Outbox")` will generate the following SQL script:

```sql
CREATE TABLE Outbox (
    "MessageId" TEXT NOT NULL,
    "Topic" TEXT NOT NULL,
    "MessageType" TEXT NOT NULL,
    "Timestamp" TEXT NOT NULL,
    "CorrelationId" TEXT NULL,
    "ReplyTo" TEXT NULL,
    "ContentType" TEXT NULL,
    "PartitionKey" TEXT NULL,
    "WorkflowId" TEXT NULL,
    "JobId" TEXT NULL,
    "Dispatched" TEXT NULL,
    "HeaderBag" TEXT NOT NULL,
    "Body" TEXT NOT NULL,
    "Source" TEXT NULL,
    "Type" TEXT NULL,
    "DataSchema" TEXT NULL,
    "Subject" TEXT NULL,
    "TraceParent" TEXT NULL,
    "TraceState" TEXT NULL,
    "Baggage" TEXT NULL,
    "Created" TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f', 'now')),
    "CreatedID" INTEGER PRIMARY KEY AUTOINCREMENT,
    CONSTRAINT "PK_Outbox" PRIMARY KEY ("MessageId")
);
```

## **Configuration**

To configure the SQLite Outbox, you need to provide an outbox implementation in the `AddProducers` configuration when setting up Brighter.

### **1. Provide Database Configuration**

First, define the configuration for your SQLite database connection. We recommend retrieving the connection string from your application's configuration (e.g., `appsettings.json`) rather than hardcoding it.

```csharp
// Get connection string from configuration
var connectionString = "Data Source=brighter.db";

// An object to hold your connection string and table name
var dbConfig = new RelationalDatabaseConfiguration(
    connectionString: connectionString,
    outBoxTableName: "Outbox"
);

// Register the configuration with the service collection
services.AddSingleton<IAmARelationalDatabaseConfiguration>(dbConfig);
```

### **2. Register the Outbox**

Next, in your `ConfigureServices` method or `Program.cs`, add the outbox configuration when calling `AddBrighter`. You need to specify the `Outbox`, `ConnectionProvider`, and `TransactionProvider`.

The `TransactionProvider` depends on how you manage your database transactions.
- Use `SqliteUnitOfWork` for ADO.NET-based transaction management.
- Use `SqliteEntityFrameworkConnectionProvider<T>` if you are using Entity Framework Core, where `T` is your `DbContext`.

### **Example with Entity Framework Core**

For more detailed information on integrating with Entity Framework Core, please see the [EF Core Outbox documentation](/contents/EFCoreOutbox.md).

Here is a complete example of configuring the SQLite Outbox with EF Core.

```csharp
// In your DbContext, you would have your entities
public class MyDbContext : DbContext
{
    // ... DbSets for your entities
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) {}
}

// In ConfigureServices or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = "Data Source=brighter.db";
    
    // 1. Add your DbContext
    services.AddDbContext<MyDbContext>(options => 
        options.UseSqlite(connectionString)
    );

    // 2. Configure the Outbox
    var outboxConfiguration = new RelationalDatabaseConfiguration(connectionString, outBoxTableName: "Outbox");
    services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);

    // 3. Configure Brighter
    services.AddBrighter(options =>
    {
        // ... other Brighter options
    })
    .AddProducers(producers =>
    {
        producers.Outbox = new SqliteOutbox(outboxConfiguration);
        producers.ConnectionProvider = typeof(SqliteConnectionProvider);
        // Use the EF Core transaction provider with your DbContext
        producers.TransactionProvider = typeof(SqliteEntityFrameworkTransactionProvider<MyDbContext>);
        
        // ... configure your producers (e.g., for RabbitMQ, Kafka)
    })
    .UseOutboxSweeper() // Optionally add the background sweeper service
    .AutoFromAssemblies(); // Scan for handlers and mappers
}
```