# **Using the MSSQL Outbox**

The MSSQL Outbox provides a message store for the [Transactional Outbox pattern](/contents/BrighterOutboxSupport.md) using a Microsoft SQL Server database. This ensures that messages are saved within the same transaction as your business logic and published to a message broker later.

## **NuGet Packages**

To use the MSSQL Outbox, you need to install the following packages from NuGet. If you are using Entity Framework Core, you will also need the EF Core integration package.

```powershell
Install-Package Paramore.Brighter.MsSql
Install-Package Paramore.Brighter.Outbox.MsSql
```

For Entity Framework Core support:
```powershell
Install-Package Paramore.Brighter.MsSql.EntityFrameworkCore
```

## **Database Table Schema**

The MSSQL Outbox requires a specific table in your database to store messages before they are dispatched. You can generate the necessary SQL Data Definition Language (DDL) script to create this table using the `MsSqlOutboxBuilder` helper class.

**Note:** You are responsible for creating and maintaining this table. This includes tasks such as adding indexes to optimize query performance and managing schema migrations when updating to new versions of Brighter that may require additional columns.

### **Generating the DDL**

The `MsSqlOutboxBuilder.GetDDL()` method creates the SQL script for you. You can execute this script against your database to create the outbox table.

```csharp
// The table name can be whatever you choose.
string tableName = "Outbox"; 

// The DDL for a table that stores the message body as NVARCHAR(MAX)
string ddl = MsSqlOutboxBuilder.GetDDL(tableName);

// The DDL for a table that stores the message body as VARBINARY(MAX)
// Useful if your message body is binary
string binaryDdl = MsSqlOutboxBuilder.GetDDL(tableName, hasBinaryMessagePayload: true);
```

### **Example SQL Script**

Running `MsSqlOutboxBuilder.GetDDL("Outbox")` will generate the following SQL script:

```sql
CREATE TABLE Outbox (
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    [Topic] NVARCHAR(255) NOT NULL,
    [MessageType] NVARCHAR(32) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL,
    [CorrelationId] UNIQUEIDENTIFIER NULL,
    [ReplyTo] NVARCHAR(255) NULL,
    [ContentType] NVARCHAR(128) NULL,
    [PartitionKey] NVARCHAR(255) NULL,
    [WorkflowId] NVARCHAR(255) NULL,
    [JobId] NVARCHAR(255) NULL,
    [Dispatched] DATETIME2 NULL,
    [HeaderBag] NVARCHAR(MAX) NOT NULL,
    [Body] NVARCHAR(MAX) NOT NULL,
    [Source] NVARCHAR(255) NULL,
    [Type] NVARCHAR(255) NULL,
    [DataSchema] NVARCHAR(255) NULL,
    [Subject] NVARCHAR(255) NULL,
    [TraceParent] NVARCHAR(255) NULL,
    [TraceState] NVARCHAR(255) NULL,
    [Baggage] NVARCHAR(MAX) NULL,
    [Created] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedID] INT IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK_Outbox] PRIMARY KEY CLUSTERED ([MessageId] ASC)
);
```

## **Configuration**

To configure the MSSQL Outbox, you need to provide an outbox implementation in the `AddProducers` configuration when setting up Brighter.

### **1. Provide Database Configuration**

First, define the configuration for your SQL Server database connection. We recommend retrieving the connection string from your application's configuration (e.g., `appsettings.json`) rather than hardcoding it.

```csharp
// Get connection string from configuration
var connectionString = "Server=localhost;Database=brighter;User Id=sa;Password=your_password;TrustServerCertificate=True;";

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
- Use `MsSqlUnitOfWork` for ADO.NET-based transaction management.
- Use `MsSqlEntityFrameworkConnectionProvider<T>` if you are using Entity Framework Core, where `T` is your `DbContext`.

### **Example with Entity Framework Core**

For more detailed information on integrating with Entity Framework Core, please see the [EF Core Outbox documentation](/contents/EFCoreOutbox.md).

Here is a complete example of configuring the MSSQL Outbox with EF Core.

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
    var connectionString = "Server=localhost;Database=brighter;User Id=sa;Password=your_password;TrustServerCertificate=True;";
    
    // 1. Add your DbContext
    services.AddDbContext<MyDbContext>(options => 
        options.UseSqlServer(connectionString)
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
        producers.Outbox = new MsSqlOutbox(outboxConfiguration);
        producers.ConnectionProvider = typeof(MsSqlConnectionProvider);
        // Use the EF Core transaction provider with your DbContext
        producers.TransactionProvider = typeof(MsSqlEntityFrameworkTransactionProvider<MyDbContext>);
        
        // ... configure your producers (e.g., for RabbitMQ, Kafka)
    })
    .UseOutboxSweeper() // Optionally add the background sweeper service
    .AutoFromAssemblies(); // Scan for handlers and mappers
}
```