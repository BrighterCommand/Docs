---
description: "Darker is the query-side counterpart to Brighter, implementing the Query Object pattern for CQRS (Command Query Responsibility Segregation) architectures."
layout:
  description:
    visible: false
---

# Basic Configuration

> **How-to** · Applies to **Darker V4**

## Darker Configuration Introduction

Darker is the query-side counterpart to [Brighter](/contents/BrighterBasicConfiguration.md), implementing the Query Object pattern for CQRS (Command Query Responsibility Segregation) architectures. While Brighter handles commands and events that change state, Darker provides a pipeline for executing queries that read state. Together, they form a complete CQRS solution for .NET applications.

You use Darker when you want to separate the parameters of a query from the execution of that query, typically when you need to add cross-cutting concerns such as logging, retry policies, or circuit breakers to your query handling. For more information on CQRS patterns and how Brighter and Darker work together, see [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md).

## Darker Configuration Prerequisites

### .NET Version Requirements

Darker supports .NET 8.0 and later versions. The sample code in this documentation is compatible with both .NET 8.0 and .NET 9.0.

### NuGet Packages

You will need the following NuGet packages to use Darker:

- **Paramore.Darker** - Core library providing the `IQueryProcessor` and query handler infrastructure
- **Paramore.Darker.AspNetCore** - ASP.NET Core integration providing the `AddDarker` extension method for dependency injection
- **Paramore.Darker.QueryLogging** - Optional package for JSON-based query logging
- **Paramore.Darker.Policies** - Optional package for integrating Polly resilience policies (retry, circuit breaker, fallback)

Install the packages using the .NET CLI:

```bash
dotnet add package Paramore.Darker
dotnet add package Paramore.Darker.AspNetCore
dotnet add package Paramore.Darker.QueryLogging
dotnet add package Paramore.Darker.Policies
```

Or using Package Manager Console:

```powershell
Install-Package Paramore.Darker
Install-Package Paramore.Darker.AspNetCore
Install-Package Paramore.Darker.QueryLogging
Install-Package Paramore.Darker.Policies
```

## Quick Start with ASP.NET Core

### Basic Setup

To configure Darker in an ASP.NET Core application, use the `AddDarker` extension method in your `Program.cs` or `Startup.cs` file. Darker integrates with the ASP.NET Core dependency injection container, making the `IQueryProcessor` available for injection into your controllers and endpoints.

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

var builder = WebApplication.CreateBuilder(args);

// Add Darker to the service collection
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddJsonQueryLogging()
    .AddDefaultPolicies();

var app = builder.Build();

// ... configure middleware and endpoints

app.Run();
```

The `AddHandlersFromAssemblies` method scans the specified assembly for query handlers and registers them automatically. This is the recommended approach for registering handlers.

### Minimal API Example

Here's a complete example of using Darker with ASP.NET Core Minimal APIs:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Darker
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly);

var app = builder.Build();

// Define a query endpoint
app.MapGet("/people", async (IQueryProcessor queryProcessor) =>
{
    var query = new GetPeopleQuery();
    var result = await queryProcessor.ExecuteAsync(query);
    return Results.Ok(result);
});

// Define a parameterized query endpoint
app.MapGet("/people/{id:int}", async (IQueryProcessor queryProcessor, int id) =>
{
    var query = new GetPersonNameQuery(id);
    var result = await queryProcessor.ExecuteAsync(query);
    return Results.Ok(result);
});

app.Run();
```

In this example, `IQueryProcessor` is injected directly into the endpoint handlers. The query processor dispatches the query to the appropriate query handler.

### MVC Controller Example

If you're using MVC controllers, inject `IQueryProcessor` into your controller's constructor:

```csharp
using Microsoft.AspNetCore.Mvc;
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IQueryProcessor _queryProcessor;

    public PeopleController(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var query = new GetPeopleQuery();
        var result = await _queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var query = new GetPersonNameQuery(id);
        var result = await _queryProcessor.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
}
```

Working examples can be found in the Darker samples: `Darker/samples/SampleMinimalApi/`

## Using IQueryProcessor

The `IQueryProcessor` is the central interface for executing queries in Darker. Once configured, you inject it into your controllers, endpoints, or services to dispatch queries to their handlers.

### ExecuteAsync Pattern (Recommended)

The asynchronous `ExecuteAsync` method is recommended for most scenarios, especially when your query involves I/O operations like database access:

```csharp
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

public class OrderService
{
    private readonly IQueryProcessor _queryProcessor;

    public OrderService(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    public async Task<OrderDetails> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var query = new GetOrderDetailsQuery(orderId);
        var result = await _queryProcessor.ExecuteAsync(query, cancellationToken);
        return result;
    }
}
```

Always pass the `CancellationToken` to `ExecuteAsync` when available. This allows query execution to be cancelled if the request is aborted, improving application responsiveness and resource usage.

### Execute Pattern (Synchronous)

For synchronous query execution (rare cases where async is not needed), use the `Execute` method:

```csharp
using Paramore.Darker;

public class CacheService
{
    private readonly IQueryProcessor _queryProcessor;

    public CacheService(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    public int GetCachedCount()
    {
        var query = new GetCountQuery();
        var result = _queryProcessor.Execute(query);
        return result;
    }
}
```

Use the synchronous `Execute` method only when:
- Your query handler performs purely in-memory operations
- You're working in a synchronous context that cannot be made async
- You have a specific performance requirement for synchronous execution

For most modern applications, prefer `ExecuteAsync`.

## Configuration with Decorators

Darker supports decorators (middleware) that wrap query handlers to provide cross-cutting concerns like logging and resilience policies. You can add decorators during configuration.

### Adding Query Logging

The `AddJsonQueryLogging` extension adds a decorator that logs query execution details in JSON format:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.QueryLogging;

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddJsonQueryLogging();
```

This will log:

- The query type and parameters
- Execution time
- Query results (summary)

Query logging is useful for debugging and monitoring query execution in your application. For more details on using the logging decorator in handlers, see [Query Pipeline](/contents/QueryPipeline.md).

### Adding Policies

Darker integrates with [Polly](https://github.com/App-vNext/Polly) to provide resilience and transient fault handling through policies like retry, circuit breaker, and fallback.

**Using Default Policies:**

The `AddDefaultPolicies` method adds a default set of policies:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddDefaultPolicies();
```

**Using Custom Policy Registry:**

For more control, you can create a custom policy registry with specific retry strategies, circuit breakers, and timeout policies:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Polly;
using Polly.Registry;

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddPolicies(ConfigurePolicies());

static IPolicyRegistry<string> ConfigurePolicies()
{
    var defaultRetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(150)
        });

    var circuitBreakerPolicy = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));

    var policyRegistry = new PolicyRegistry
    {
        {Constants.RetryPolicyName, defaultRetryPolicy},
        {Constants.CircuitBreakerPolicyName, circuitBreakerPolicy}
    };

    return policyRegistry;
}
```

Once policies are configured, you can apply them to query handlers using attributes. For details on using policy decorators in handlers, see [Query Pipeline](/contents/QueryPipeline.md).

## Common Configuration Patterns

### Pattern: Basic Web API Setup

This is the most common setup for an ASP.NET Core Web API using Darker:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

var builder = WebApplication.CreateBuilder(args);

// Add Darker with logging and policies
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddJsonQueryLogging()
    .AddDefaultPolicies();

// Add other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Pattern: With EF Core DbContext

When using Entity Framework Core, you must configure the Query Processor with scoped lifetime to match the DbContext:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core DbContext (scoped by default)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Darker with scoped lifetime to match EF Core
builder.Services.AddDarker(options =>
{
    // Critical: Match the DbContext lifetime
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(Program).Assembly);

var app = builder.Build();
app.Run();
```

Without the scoped configuration, you'll encounter exceptions about disposed DbContext instances.

### Pattern: Multiple Handler Assemblies

When your query handlers are spread across multiple assemblies, register all of them:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register handlers from multiple assemblies
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(
        typeof(CustomerQueries.GetCustomerQuery).Assembly,    // Customer module
        typeof(OrderQueries.GetOrderQuery).Assembly,          // Order module
        typeof(ProductQueries.GetProductQuery).Assembly);     // Product module

var app = builder.Build();
app.Run();
```

This pattern is useful in modular monoliths or when organizing queries by domain.

## Darker Configuration Troubleshooting

### Common Issues

**Handler not found errors**

If you receive an exception that a handler cannot be found for a query:
- Verify that the handler class implements `QueryHandler<TQuery, TResult>` or `QueryHandlerAsync<TQuery, TResult>`
- Ensure the handler's assembly is registered with `AddHandlersFromAssemblies`
- Check that the query and handler types match exactly (including generic type parameters)
- Verify the handler class is public and not abstract

**Lifetime scope issues with EF Core**

If you see exceptions about a disposed DbContext:
- Ensure you've configured `QueryProcessorLifetime = ServiceLifetime.Scoped` in the Darker options
- Verify your DbContext is registered with scoped lifetime (default for EF Core)
- Check that you're not trying to use the query result after the scope has been disposed

**Assembly scanning not finding handlers**

If handlers aren't being registered automatically:
- Verify you're passing the correct assembly to `AddHandlersFromAssemblies`
- Ensure handlers are in the same assembly or you've registered all relevant assemblies
- Check that handler classes are public and not nested within other classes
- Verify handlers follow the naming conventions (end with "Handler" or "QueryHandler")

**Policy not found errors**

If you see exceptions about missing policies:
- Ensure you've called `AddDefaultPolicies()` or `AddPolicies(registry)`
- Verify the policy name in your attribute matches the name in the policy registry
- Check that the policy registry is correctly configured before being passed to `AddPolicies`

## Further Reading

- [Darker Configuration Reference](/contents/DarkerConfigurationReference.md) - Query processor lifetime and handler registration strategies
- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - Learn how to design query objects
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Detailed guide to implementing query handlers
- [Query Pipeline](/contents/QueryPipeline.md) - Understanding decorators, policies, and the query pipeline
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Architectural patterns for CQRS

