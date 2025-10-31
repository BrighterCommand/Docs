# **Using the PostgreSQL Outbox**

The PostgreSQL Outbox provides a message store for the [Transactional Outbox pattern](/contents/BrighterOutboxSupport.md) using a PostgreSQL database. This ensures that messages are saved within the same transaction as your business logic and published to a message broker later.

## **NuGet Packages**

To use the PostgreSQL Outbox, you need to install the following packages from NuGet. If you are using Entity Framework Core, you will also need the EF Core integration package.

```powershell
Install-Package Paramore.Brighter.PostgreSql
Install-Package Paramore.Brighter.Outbox.PostgreSql
```

For Entity Framework Core support:
```powershell
Install-Package Paramore.Brighter.PostgreSql.EntityFrameworkCore
```

## **Database Table Schema**

The PostgreSQL Outbox requires a specific table in your database to store messages before they are dispatched. You can generate the necessary SQL Data Definition Language (DDL) script to create this table using the `PostgreSqlOutboxBuilder` helper class.

**Note:** You are responsible for creating and maintaining this table. This includes tasks such as adding indexes to optimize query performance and managing schema migrations when updating to new versions of Brighter that may require additional columns.

### **Generating the DDL**

The `PostgreSqlOutboxBuilder.GetDDL()` method creates the SQL script for you. You can execute this script against your database to create the outbox table.

```csharp
// The table name can be whatever you choose.
string tableName = "Outbox"; 

// The DDL for a table that stores the message body as TEXT
string ddl = PostgreSqlOutboxBuilder.GetDDL(tableName);

// The DDL for a table that stores the message body as BYTEA
// Useful if your message body is binary
string binaryDdl = PostgreSqlOutboxBuilder.GetDDL(tableName, hasBinaryMessagePayload: true);
```

### **Example SQL Script**

Running `PostgreSqlOutboxBuilder.GetDDL("Outbox")` will generate the following SQL script:

```sql
CREATE TABLE "Outbox" ( 
    "MessageId" VARCHAR(255) NOT NULL, 
    "Topic" VARCHAR(255) NOT NULL, 
    "MessageType" VARCHAR(32) NOT NULL, 
    "Timestamp" TIMESTAMPTZ(3) NOT NULL, 
    "CorrelationId" VARCHAR(255) NULL,
    "ReplyTo" VARCHAR(255) NULL,
    "ContentType" VARCHAR(128) NULL, 
    "PartitionKey" VARCHAR(255) NULL, 
    "WorkflowId" VARCHAR(255) NULL,
    "JobId" VARCHAR(255) NULL,
    "Dispatched" TIMESTAMPTZ(3) NULL, 
    "HeaderBag" TEXT NOT NULL, 
    "Body" TEXT NOT NULL , 
    "Source" VARCHAR(255) NULL,
    "Type" VARCHAR(255) NULL,
    "DataSchema" VARCHAR(255) NULL,
    "Subject" VARCHAR(255) NULL,
    "TraceParent" VARCHAR(255) NULL,
    "TraceState" VARCHAR(255) NULL,
    "Baggage" TEXT NULL,
    "Created" TIMESTAMPTZ(3) NOT NULL DEFAULT NOW(),
    "CreatedID" INT NOT NULL GENERATED ALWAYS AS IDENTITY, 
    UNIQUE("CreatedID"),
    PRIMARY KEY ("MessageId")
);
```

## **Configuration**

To configure the PostgreSQL Outbox, you need to provide an outbox implementation in the `AddProducers` configuration when setting up Brighter.

### **1. Provide Database Configuration**

First, define the configuration for your PostgreSQL database connection. We recommend retrieving the connection string from your application's configuration (e.g., `appsettings.json`) rather than hardcoding it.

```csharp
// Get connection string from configuration
var connectionString = "Host=localhost;Port=5432;Database=brighter;Username=postgres;Password=password";

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
- Use `PostgreSqlUnitOfWork` for ADO.NET-based transaction management.
- Use `PostgreSqlEntityFrameworkConnectionProvider<T>` if you are using Entity Framework Core, where `T` is your `DbContext`.

### **Example with Entity Framework Core**

For more detailed information on integrating with Entity Framework Core, please see the [EF Core Outbox documentation](/contents/EFCoreOutbox.md).

Here is a complete example of configuring the PostgreSQL Outbox with EF Core.

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
    var connectionString = "Host=localhost;Port=5432;Database=brighter;Username=postgres;Password=password";
    
    // 1. Add your DbContext
    services.AddDbContext<MyDbContext>(options => 
        options.UseNpgsql(connectionString)
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
        producers.Outbox = new PostgreSqlOutbox(outboxConfiguration);
        producers.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        // Use the EF Core transaction provider with your DbContext
        producers.TransactionProvider = typeof(PostgreSqlEntityFrameworkTransactionProvider<MyDbContext>);
        
        // ... configure your producers (e.g., for RabbitMQ, Kafka)
    })
    .UseOutboxSweeper() // Optionally add the background sweeper service
    .AutoFromAssemblies(); // Scan for handlers and mappers
}
```