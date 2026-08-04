# Implementing a Query Handler

> **How-to** · Applies to **Darker V4**

## Introduction

Query handlers are the entry point to your query execution logic in Darker. A query handler receives a query object, executes the necessary logic to retrieve or compute the requested data, and returns the result. Query handlers are always part of an internal bus and form part of a middleware pipeline, similar to how request handlers work in Brighter.

Each query handler is responsible for:

- Receiving a specific query type
- Executing the logic to retrieve the data
- Returning a strongly-typed result
- Participating in the query pipeline (decorators can be applied)

Query handlers are analogous to methods on an ASP.NET Controller, but with the benefits of separation of concerns, testability, and the ability to apply cross-cutting concerns through decorators.

For more information on the Query Processor and how it dispatches queries to handlers, see [Basic Concepts](/contents/BasicConcepts.md#query-processor).

## Query Objects

Before implementing a query handler, you need a query object that defines what data to retrieve. Query objects implement the `IQuery<TResult>` interface.

### Defining a Query

Here are the two queries we'll use in examples throughout this guide:

**Simple query (no parameters):**

```csharp
using Paramore.Darker;
using System.Collections.Generic;

public sealed class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}
```

**Parameterized query:**

```csharp
using Paramore.Darker;

public sealed class GetPersonNameQuery : IQuery<string>
{
    public GetPersonNameQuery(int personId)
    {
        PersonId = personId;
    }

    public int PersonId { get; }
}
```

For detailed information on designing query objects, see [Queries and Query Objects](/contents/QueriesAndQueryObjects.md).

### Query Design Guidelines

When designing queries that will be handled by your handlers:

- Keep queries immutable (read-only properties)
- Include all parameters needed to execute the query
- Use descriptive names (GetX, FindX, SearchX patterns)
- Validate parameters in the constructor
- Use appropriate result types in `IQuery<TResult>`

## Handler Implementation Patterns

Darker provides three ways to implement query handlers, each suited to different scenarios. The asynchronous handler pattern is recommended for most applications.

## Pattern 1: Asynchronous Handler (Recommended)

The `QueryHandlerAsync<TQuery, TResult>` base class is the recommended approach for implementing query handlers. It supports asynchronous operations, which is essential for I/O-bound operations like database queries.

### QueryHandlerAsync<TQuery, TResult>

To create an asynchronous query handler, inherit from `QueryHandlerAsync<TQuery, TResult>` and override the `ExecuteAsync` method:

```csharp
using Paramore.Darker;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPeopleQueryHandler : QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>
{
    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetPeopleQuery query,
        CancellationToken cancellationToken = default)
    {
        // Your query logic here
        var repository = new PersonRepository();
        return await repository.GetAllAsync(cancellationToken);
    }
}
```

### ExecuteAsync Method Signature

The `ExecuteAsync` method has the following signature:

```csharp
public override async Task<TResult> ExecuteAsync(
    TQuery query,
    CancellationToken cancellationToken = default)
```

**Parameters:**

- `query` - The query object containing the parameters for this query
- `cancellationToken` - Token to cancel the async operation (optional, defaults to `default`)

**Returns:** A `Task<TResult>` containing the query result

### CancellationToken Support

Always accept and use the `CancellationToken` parameter:

```csharp
public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
    GetPeopleQuery query,
    CancellationToken cancellationToken = default)
{
    // Pass the cancellation token to async operations
    var repository = new PersonRepository();
    var people = await repository.GetAllAsync(cancellationToken);

    // Check if cancellation was requested
    cancellationToken.ThrowIfCancellationRequested();

    return people;
}
```

Passing the cancellation token allows the operation to be cancelled if the request is aborted, improving application responsiveness and resource usage.

### When to Use Async Handlers

Use `QueryHandlerAsync<TQuery, TResult>` when your handler performs:

- **Database queries** - Entity Framework, Dapper, ADO.NET
- **HTTP requests** - Calling external APIs or services
- **File I/O** - Reading from files or streams
- **Any I/O-bound operation** - Operations that wait on external resources

Modern .NET applications should default to async handlers unless there's a specific reason to use synchronous handlers.

### Complete Example with Decorators

Here's a complete example showing a handler with decorators applied:

```csharp
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp.QueryHandlers;

public sealed class GetPeopleQueryHandler : QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>
{
    private readonly IPersonRepository _repository;

    public GetPeopleQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]
    [RetryableQuery(2, "DefaultCircuitBreaker")]
    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetPeopleQuery query,
        CancellationToken cancellationToken = default)
    {
        var people = await _repository.GetAllAsync(cancellationToken);
        return people;
    }
}
```

The decorators are applied using attributes with step numbers that control their execution order. For more information on decorators and the query pipeline, see [Query Pipeline](/contents/QueryPipeline.md).

Working example: `Darker/samples/SampleMinimalApi/QueryHandlers/GetPeopleQueryHandler.cs`

## Pattern 2: Synchronous Handler

The `QueryHandler<TQuery, TResult>` base class is used for synchronous query handlers. Use this pattern only when you have a specific need for synchronous execution.

### QueryHandler<TQuery, TResult>

To create a synchronous query handler, inherit from `QueryHandler<TQuery, TResult>` and override the `Execute` method:

```csharp
using Paramore.Darker;

public sealed class GetCachedCountQueryHandler : QueryHandler<GetCachedCountQuery, int>
{
    private readonly ICache _cache;

    public GetCachedCountQueryHandler(ICache cache)
    {
        _cache = cache;
    }

    public override int Execute(GetCachedCountQuery query)
    {
        // Synchronous operation - reading from in-memory cache
        return _cache.GetCount(query.CacheKey);
    }
}
```

### Execute Method Signature

The `Execute` method has the following signature:

```csharp
public override TResult Execute(TQuery query)
```

**Parameters:**
- `query` - The query object containing the parameters for this query

**Returns:** The query result of type `TResult`

Note that synchronous handlers do not receive a `CancellationToken`.

### When to Use Synchronous Handlers

Use `QueryHandler<TQuery, TResult>` only when:
- Your handler performs **purely in-memory operations** (cache lookups, calculations)
- You're working in a **synchronous context** that cannot be made async
- You have a **specific performance requirement** for synchronous execution
- You're integrating with **legacy synchronous code**

**Important:** For database queries, HTTP calls, file I/O, or any operation that waits on external resources, use `QueryHandlerAsync<TQuery, TResult>` instead.

### Complete Synchronous Handler Example

```csharp
using Paramore.Darker;
using Paramore.Darker.QueryLogging;

namespace MyApp.QueryHandlers;

public sealed class GetStatisticsQuery : IQuery<Statistics>
{
    public GetStatisticsQuery(string key)
    {
        Key = key;
    }

    public string Key { get; }
}

public sealed class GetStatisticsQueryHandler : QueryHandler<GetStatisticsQuery, Statistics>
{
    private readonly IInMemoryCache _cache;

    public GetStatisticsQueryHandler(IInMemoryCache cache)
    {
        _cache = cache;
    }

    [QueryLogging(1)]
    public override Statistics Execute(GetStatisticsQuery query)
    {
        // Synchronous in-memory operation
        var stats = _cache.Get<Statistics>(query.Key);

        if (stats == null)
        {
            // Calculate statistics from in-memory data
            stats = CalculateStatistics();
            _cache.Set(query.Key, stats);
        }

        return stats;
    }

    private Statistics CalculateStatistics()
    {
        // Pure computation, no I/O
        return new Statistics
        {
            TotalCount = 100,
            AverageValue = 50.5m
        };
    }
}
```

### Converting from Synchronous to Asynchronous

If you later need to add async operations, you can convert a synchronous handler to async:

```csharp
// Before (synchronous)
public sealed class GetOrderQueryHandler : QueryHandler<GetOrderQuery, Order>
{
    public override Order Execute(GetOrderQuery query)
    {
        return _repository.GetById(query.OrderId);
    }
}

// After (asynchronous)
public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(query.OrderId, cancellationToken);
    }
}
```

## Pattern 3: Direct IQueryHandler Implementation

For maximum control, you can implement the `IQueryHandler<TQuery, TResult>` interface directly. This gives you full control over the handler implementation but requires more boilerplate code.

### IQueryHandler<TQuery, TResult> Interface

The interface defines both synchronous and asynchronous execution methods:

```csharp
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    TResult Execute(TQuery query);
    Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### When to Use Direct Implementation

Use `IQueryHandler<TQuery, TResult>` directly when you need:

- **Maximum control** over both sync and async implementations
- **Custom lifetime management** beyond what the base classes provide
- **Advanced scenarios** not covered by the base classes
- **Conditional sync/async** execution based on runtime conditions

### Complete Direct Implementation Example

```csharp
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, Order>
{
    private readonly IOrderRepository _repository;
    private readonly bool _useAsync;

    public GetOrderQueryHandler(IOrderRepository repository, bool useAsync = true)
    {
        _repository = repository;
        _useAsync = useAsync;
    }

    public Order Execute(GetOrderQuery query)
    {
        // Synchronous implementation
        return _repository.GetById(query.OrderId);
    }

    public async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        // Asynchronous implementation
        return await _repository.GetByIdAsync(query.OrderId, cancellationToken);
    }
}
```

**Note:** Most applications should use `QueryHandlerAsync<TQuery, TResult>` or `QueryHandler<TQuery, TResult>` instead of implementing the interface directly. Direct implementation adds complexity and is rarely needed.

## Query Handler Registration

Query handlers must be registered with the Darker query processor before they can be used. Darker provides two approaches: automatic assembly scanning (recommended) and manual registration.

### Automatic Registration (Recommended)

Use `AddHandlersFromAssemblies` to automatically discover and register all query handlers in one or more assemblies:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(GetPeopleQuery).Assembly);
```

This scans the assembly and registers all classes that inherit from `QueryHandler<,>`, `QueryHandlerAsync<,>`, or implement `IQueryHandler<,>`.

**Multiple assemblies:**

```csharp
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(
        typeof(CustomerQuery).Assembly,
        typeof(OrderQuery).Assembly,
        typeof(ProductQuery).Assembly);
```

**Convention-based discovery:**

The assembly scanner looks for:
- Public classes (not abstract)
- That implement query handler interfaces
- With parameterless constructors or constructors that can be resolved from DI

### Manual Registration

For fine-grained control, register handlers explicitly using `QueryHandlerRegistry`:

```csharp
using Paramore.Darker;
using Paramore.Darker.Builder;

var registry = new QueryHandlerRegistry();

// Register each handler explicitly
registry.Register<GetPeopleQuery, IReadOnlyDictionary<int, string>, GetPeopleQueryHandler>();
registry.Register<GetPersonNameQuery, string, GetPersonQueryHandler>();
registry.Register<GetOrderQuery, Order, GetOrderQueryHandler>();

// Build the query processor
IQueryProcessor queryProcessor = QueryProcessorBuilder.With()
    .Handlers(registry, Activator.CreateInstance, t => {}, Activator.CreateInstance)
    .InMemoryQueryContextFactory()
    .Build();
```

Manual registration is useful when:

- You need precise control over which handlers are registered
- You're not using ASP.NET Core's dependency injection
- You want to register handlers conditionally
- You're working in a non-web application

## Working with Dependencies

Query handlers typically need dependencies like repositories, database contexts, or services to execute queries. Darker supports dependency injection for handler dependencies.

### Constructor Injection

Inject dependencies through the handler's constructor:

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(
        IOrderRepository repository,
        ILogger<GetOrderQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving order {OrderId}", query.OrderId);

        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        return order;
    }
}
```

Dependencies are resolved automatically by the DI container when the handler is instantiated.

### Scoped Dependencies (EF Core DbContext)

When using Entity Framework Core, inject the DbContext as a scoped dependency. Remember to configure Darker with scoped lifetime:

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCustomerWithOrdersQueryHandler : QueryHandlerAsync<GetCustomerWithOrdersQuery, CustomerDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCustomerWithOrdersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CustomerDto> ExecuteAsync(
        GetCustomerWithOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .AsNoTracking()  // Read-only optimization
            .Where(c => c.Id == query.CustomerId)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                OrderCount = c.Orders.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}
```

**Important:** Ensure you've configured Darker with scoped lifetime in your `Program.cs`:

```csharp
builder.Services.AddDarker(options =>
{
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(Program).Assembly);
```

### Multiple Dependencies

Handlers can have multiple dependencies injected:

```csharp
public sealed class GetOrderSummaryQueryHandler : QueryHandlerAsync<GetOrderSummaryQuery, OrderSummary>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;

    public GetOrderSummaryQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IPricingService pricingService,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _pricingService = pricingService;
        _mapper = mapper;
    }

    public override async Task<OrderSummary> ExecuteAsync(
        GetOrderSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, cancellationToken);
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        var pricing = await _pricingService.CalculateTotalAsync(order, cancellationToken);

        return _mapper.Map<OrderSummary>((order, customer, pricing));
    }
}
```

## Query Results and Error Handling

### Returning Results

Query handlers should return the type specified in `IQuery<TResult>`. The result can be any C# type.

**Simple results:**

```csharp
public sealed class GetCustomerNameQueryHandler : QueryHandlerAsync<GetCustomerNameQuery, string>
{
    public override async Task<string> ExecuteAsync(
        GetCustomerNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(query.CustomerId, cancellationToken);
        return customer.Name;
    }
}
```

**Complex results:**

```csharp
public sealed class GetOrderDetailsQueryHandler : QueryHandlerAsync<GetOrderDetailsQuery, OrderDetailsDto>
{
    public override async Task<OrderDetailsDto> ExecuteAsync(
        GetOrderDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        return new OrderDetailsDto
        {
            OrderId = order.Id,
            CustomerName = order.Customer.Name,
            TotalAmount = order.Total,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }
}
```

**Collection results:**

```csharp
public sealed class GetActiveOrdersQueryHandler : QueryHandlerAsync<GetActiveOrdersQuery, IReadOnlyList<OrderSummary>>
{
    public override async Task<IReadOnlyList<OrderSummary>> ExecuteAsync(
        GetActiveOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetActiveOrdersAsync(cancellationToken);
        return orders.ToList().AsReadOnly();
    }
}
```

### Null Handling

Use nullable reference types when a query might not return a result:

```csharp
public sealed class FindCustomerByEmailQueryHandler : QueryHandlerAsync<FindCustomerByEmailQuery, Customer?>
{
    public override async Task<Customer?> ExecuteAsync(
        FindCustomerByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        // May return null if customer not found
        var customer = await _repository.FindByEmailAsync(query.Email, cancellationToken);
        return customer;
    }
}
```

### Throwing Exceptions

Throw exceptions for exceptional situations:

```csharp
public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException($"Order with ID {query.OrderId} not found");
        }

        return order;
    }
}
```

**When to throw exceptions:**

- Entity not found (when the query expects it to exist)
- Authorization failures
- Data integrity issues
- Unrecoverable errors

**When not to throw exceptions:**

- Normal flow control (use nullable types instead)
- Expected "not found" scenarios (use `Find` pattern with nullable return)

### Domain Exceptions

Create custom exception types for domain-specific errors:

```csharp
public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(string message) : base(message) { }
}

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message) { }
}
```

### Validation

Validate complex business rules in the handler:

```csharp
public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        // Authorization check
        if (!await _authService.CanAccessOrderAsync(query.OrderId, cancellationToken))
        {
            throw new UnauthorizedAccessException($"User cannot access order {query.OrderId}");
        }

        // Business rule validation
        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException($"Order {query.OrderId} not found");
        }

        if (order.IsDeleted)
        {
            throw new InvalidOperationException("Cannot retrieve deleted orders");
        }

        return order;
    }
}
```

## Testing Query Handlers

Query handlers are easy to test because they have clear inputs (queries) and outputs (results), with dependencies that can be mocked or replaced.

### Test-Driven Development

Use Test-Driven Development (TDD) to design query handlers:

```csharp
using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;

public class GetOrderQueryHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidOrderId_ReturnsOrder()
    {
        // Arrange
        var expectedOrder = new Order { Id = 123, CustomerName = "John Doe" };
        var mockRepository = new Mock<IOrderRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        var handler = new GetOrderQueryHandler(mockRepository.Object);
        var query = new GetOrderQuery(123);

        // Act
        var result = await handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("John Doe", result.CustomerName);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidOrderId_ThrowsNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new GetOrderQueryHandler(mockRepository.Object);
        var query = new GetOrderQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            handler.ExecuteAsync(query, CancellationToken.None));
    }
}
```

### Replacing Dependencies with In-Memory Solutions

For integration tests, replace real dependencies with in-memory alternatives:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

public class GetCustomerQueryHandlerIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCustomerQueryHandler _handler;

    public GetCustomerQueryHandlerIntegrationTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed test data
        _dbContext.Customers.Add(new Customer { Id = 1, Name = "Test Customer" });
        _dbContext.SaveChanges();

        _handler = new GetCustomerQueryHandler(_dbContext);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var query = new GetCustomerQuery(1);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.Name);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
}
```

### Acceptance Tests

For acceptance tests, use a real database to verify the entire query flow:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

[Collection("Database")]
public class OrderQueryAcceptanceTests
{
    private readonly IQueryProcessor _queryProcessor;
    private readonly TestDatabase _database;

    public OrderQueryAcceptanceTests(DatabaseFixture fixture)
    {
        _database = fixture.Database;
        _queryProcessor = fixture.ServiceProvider.GetRequiredService<IQueryProcessor>();

        // Seed test data
        _database.SeedOrders();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsCompleteOrderDetails()
    {
        // Arrange
        var query = new GetOrderQuery(1);

        // Act
        var result = await _queryProcessor.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotEmpty(result.Items);
        Assert.True(result.Total > 0);
    }
}
```

## Best Practices

- **Keep handlers focused** - Each handler should have a single responsibility
- **Use async for I/O operations** - Always prefer `QueryHandlerAsync` for database, HTTP, or file operations
- **Validate query parameters** - Perform simple validation in the query constructor, complex validation in the handler
- **Return appropriate result types** - Use nullable types when results may not exist, read-only collections for lists
- **Handle nulls explicitly** - Use nullable reference types and be clear about when nulls can occur
- **Use CancellationToken** - Always accept and pass through cancellation tokens to allow request cancellation
- **Inject dependencies** - Use constructor injection for all handler dependencies
- **Project only what you need** - Use DTOs to return only the data required by consumers
- **Use AsNoTracking with EF Core** - Optimize read-only queries with `AsNoTracking()`
- **Keep logic in handlers, not queries** - Query objects should be simple data containers

## Common Pitfalls

- **Forgetting CancellationToken parameter** - Always include the cancellation token in async methods
- **Using wrong handler base class** - Use async handlers for I/O operations
- **Lifetime scope mismatches** - Configure scoped lifetime when using EF Core DbContext
- **Not registering handlers** - Ensure handlers are registered via assembly scanning or manual registration
- **Business logic in query objects** - Keep queries as simple parameter containers
- **Over-fetching data** - Project only the fields needed instead of returning entire entities
- **N+1 query problems** - Use `Include` or projection to avoid multiple database roundtrips
- **Not handling nulls** - Be explicit about nullable results and handle them appropriately
- **Mixing queries and commands** - Queries should never modify state
- **Complex validation in constructors** - Move complex validation logic to handlers

## Further Reading

- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - Designing query objects
- [Query Pipeline](/contents/QueryPipeline.md) - Understanding decorators and the query pipeline
- [Query Patterns](/contents/QueryPatterns.md) - Advanced patterns for real-world scenarios
- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - Setting up Darker
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Architectural patterns

