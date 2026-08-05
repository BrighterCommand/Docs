# Queries and Query Objects

> **Explanation** · Applies to **Darker V4**

## Query Object Introduction

The Query Object pattern separates the parameters of a query from the execution of that query. In Darker, queries are simple objects that encapsulate the data needed to perform a query, while query handlers contain the logic to execute the query and return results.

This separation provides several benefits:
- Clear separation between what you want to query (the query object) and how to query it (the query handler)
- Easy testing - query objects are just data containers
- Reusable query definitions across different parts of your application
- Type-safe query parameters and results

For more information on how queries fit into CQRS architectures, see [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md).

## The IQuery<TResult> Interface

All queries in Darker implement the `IQuery<TResult>` interface, which is a marker interface that defines the result type of the query.

### Interface Definition

```csharp
public interface IQuery<out TResult>
{
}
```

The interface itself has no methods or properties - it simply marks a class as a query and specifies what type of result the query will return.

### Type Parameter

The `TResult` type parameter specifies the type that will be returned when the query is executed:

- `IQuery<string>` - Returns a string
- `IQuery<int>` - Returns an integer
- `IQuery<OrderDetails>` - Returns an OrderDetails object
- `IQuery<IReadOnlyList<Customer>>` - Returns a read-only list of customers
- `IQuery<IReadOnlyDictionary<int, string>>` - Returns a read-only dictionary

The query processor uses this type information to match queries to their handlers and ensure type safety throughout the pipeline.

## Designing Query Objects

Query objects should be simple, immutable data containers that hold the parameters needed to execute a query. They should not contain any business logic or query execution code.

### Simple Queries (No Parameters)

The simplest queries have no parameters - they simply indicate what data you want to retrieve:

```csharp
using Paramore.Darker;
using System.Collections.Generic;

public sealed class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}
```

This query returns all people as a dictionary mapping IDs to names. It has no properties because it doesn't need any parameters - the handler will return all available data.

**When to use simple queries:**

- Retrieving all items in a collection (when the collection is reasonably sized)
- Dashboard or summary queries that don't need filtering
- Queries that return system-wide configuration or settings

### Parameterized Queries

Most queries need parameters to specify what data to retrieve:

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

This query takes a `personId` parameter to specify which person's name to retrieve. The parameter is passed through the constructor and exposed as a read-only property.

**When to use parameterized queries:**

- Single entity lookups by ID or unique key
- Filtered collections (e.g., orders for a specific customer)
- Searches with specific criteria

### Complex Query Parameters

For queries with multiple parameters or complex filtering criteria, include all necessary parameters as properties:

```csharp
using Paramore.Darker;
using System;
using System.Collections.Generic;

public sealed class SearchOrdersQuery : IQuery<IReadOnlyList<OrderSummary>>
{
    public SearchOrdersQuery(
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        OrderStatus? status = null)
    {
        CustomerId = customerId;
        FromDate = fromDate;
        ToDate = toDate;
        Status = status;
    }

    public int? CustomerId { get; }
    public DateTime? FromDate { get; }
    public DateTime? ToDate { get; }
    public OrderStatus? Status { get; }
}
```

This query supports multiple optional filter parameters, allowing flexible searching.

**Optional Parameters:**

Use nullable types for optional parameters:

```csharp
public sealed class GetOrdersPageQuery : IQuery<PagedResult<Order>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize, string? sortBy = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
    public string? SortBy { get; }
}
```

**Filter Objects:**

For very complex queries, consider using a separate filter object:

```csharp
public sealed class ProductSearchQuery : IQuery<IReadOnlyList<Product>>
{
    public ProductSearchQuery(ProductSearchFilter filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public ProductSearchFilter Filter { get; }
}

public class ProductSearchFilter
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Category { get; set; }
    public bool? InStock { get; set; }
}
```

This approach is useful when the same filter criteria are used across multiple queries.

## Query Object Design Principles

### Immutability

Query objects should be immutable - once created, their state cannot be changed. This makes queries predictable, thread-safe, and easy to reason about.

**Use read-only properties:**

```csharp
public sealed class GetOrderQuery : IQuery<Order>
{
    public GetOrderQuery(int orderId)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }  // Get-only property
}
```

**Or use init-only setters (C# 9+):**

```csharp
public sealed class GetCustomerQuery : IQuery<Customer>
{
    public int CustomerId { get; init; }
    public bool IncludeOrders { get; init; }
}

// Usage:
var query = new GetCustomerQuery
{
    CustomerId = 123,
    IncludeOrders = true
};
```

**Avoid mutable properties:**

```csharp
// Bad - mutable query
public class GetProductQuery : IQuery<Product>
{
    public int ProductId { get; set; }  // Don't do this!
}
```

### Value Object Pattern

Queries behave like value objects - their identity is based on their values, not on object reference. Two query objects with the same parameter values should be considered equal.

```csharp
public sealed class GetOrderQuery : IQuery<Order>
{
    public GetOrderQuery(int orderId)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }

    // Optional: Override equality for testing or caching
    public override bool Equals(object? obj)
    {
        return obj is GetOrderQuery other && OrderId == other.OrderId;
    }

    public override int GetHashCode()
    {
        return OrderId.GetHashCode();
    }
}
```

While not required, implementing equality can be useful for testing or caching scenarios.

### Encapsulation

Keep query internals simple and focused on data. Any computed or derived values should be calculated in the handler, not the query.

**Good encapsulation:**

```csharp
public sealed class GetOrdersPageQuery : IQuery<PagedResult<Order>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be positive");
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100");

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}
```

**Avoid business logic in queries:**

```csharp
// Bad - business logic in query
public sealed class GetDiscountedProductsQuery : IQuery<IReadOnlyList<Product>>
{
    public decimal GetDiscountPrice(Product product)  // Don't do this!
    {
        return product.Price * 0.9m;
    }
}
```

Business logic and calculations should live in handlers or domain objects, not in query objects.

## Query Result Types

The result type specified in `IQuery<TResult>` can be any C# type. Choose the appropriate type based on what data the query needs to return.

### Primitive Types

Use primitive types for simple single-value queries:

```csharp
public sealed class GetOrderCountQuery : IQuery<int>
{
}

public sealed class GetCustomerNameQuery : IQuery<string>
{
    public GetCustomerNameQuery(int customerId)
    {
        CustomerId = customerId;
    }

    public int CustomerId { get; }
}

public sealed class IsProductAvailableQuery : IQuery<bool>
{
    public IsProductAvailableQuery(int productId)
    {
        ProductId = productId;
    }

    public int ProductId { get; }
}
```

### DTOs and Projections

For complex data, return Data Transfer Objects (DTOs) or projections:

```csharp
public sealed class GetOrderDetailsQuery : IQuery<OrderDetailsDto>
{
    public GetOrderDetailsQuery(int orderId)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }
}

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

DTOs are useful for projecting only the data needed by the UI or API, avoiding over-fetching.

### Collections

Use collection types for queries that return multiple items:

```csharp
// List
public sealed class GetCustomersQuery : IQuery<List<Customer>>
{
}

// IReadOnlyList (preferred for immutability)
public sealed class GetActiveOrdersQuery : IQuery<IReadOnlyList<OrderSummary>>
{
}

// IEnumerable (for streaming scenarios)
public sealed class GetLargeDataSetQuery : IQuery<IEnumerable<DataRow>>
{
}

// Array
public sealed class GetTopProductsQuery : IQuery<Product[]>
{
    public GetTopProductsQuery(int count)
    {
        Count = count;
    }

    public int Count { get; }
}
```

Prefer `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for query results to make it clear that the data should not be modified.

### Dictionaries

Use dictionaries when returning key-value pairs:

```csharp
public sealed class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}

public sealed class GetProductPricesQuery : IQuery<Dictionary<string, decimal>>
{
}
```

Dictionaries are useful for lookup scenarios where you need fast access by key.

### Nullable Results

Use nullable types when a query might not return a result:

```csharp
public sealed class FindCustomerByEmailQuery : IQuery<Customer?>
{
    public FindCustomerByEmailQuery(string email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public string Email { get; }
}

public sealed class GetOptionalConfigurationQuery : IQuery<string?>
{
    public GetOptionalConfigurationQuery(string key)
    {
        Key = key;
    }

    public string Key { get; }
}
```

Nullable types make it explicit that a query may return no result, forcing callers to handle the null case.

### Complex Result Types

For advanced scenarios, you can return tuples, custom result wrappers, or domain objects:

```csharp
// Tuple
public sealed class GetOrderSummaryQuery : IQuery<(int TotalOrders, decimal TotalRevenue, decimal AverageOrderValue)>
{
}

// Custom result wrapper
public sealed class GetOrdersPageQuery : IQuery<PagedResult<Order>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

## Validation in Query Objects

Query objects should validate their parameters to ensure they receive valid data. Simple validation belongs in the constructor, while complex validation should be handled by the handler or a validation framework.

### Constructor Validation

Use guard clauses in the constructor for simple validation:

```csharp
using System;
using Paramore.Darker;

public sealed class GetOrdersPageQuery : IQuery<PagedResult<Order>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be positive");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be between 1 and 100");

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}
```

### Validation Attributes

For ASP.NET scenarios, you can use data annotations that are validated by the framework:

```csharp
using System.ComponentModel.DataAnnotations;
using Paramore.Darker;

public sealed class SearchProductsQuery : IQuery<IReadOnlyList<Product>>
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SearchTerm { get; init; } = string.Empty;

    [Range(1, 1000)]
    public int MaxResults { get; init; } = 50;
}
```

The ASP.NET model binder will validate these attributes before the query reaches your handler.

### Where to Validate

**Constructor validation (recommended for queries):**

- Parameter null checks
- Range validation for numeric values
- Format validation for strings
- Basic business invariants

**Handler validation (for complex rules):**

- Database existence checks
- Authorization checks
- Complex business rules
- Cross-field validation

**Framework validation (ASP.NET):**

- Model binding validation
- Data annotations
- Request validation

```csharp
// Simple validation in constructor
public sealed class GetUserQuery : IQuery<User>
{
    public GetUserQuery(string email)
    {
        Email = !string.IsNullOrWhiteSpace(email)
            ? email
            : throw new ArgumentException("Email cannot be empty", nameof(email));
    }

    public string Email { get; }
}

// Complex validation in handler
public class GetUserQueryHandler : QueryHandlerAsync<GetUserQuery, User>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public override async Task<User> ExecuteAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        // Check if user exists (complex validation)
        var user = await _repository.FindByEmailAsync(query.Email, cancellationToken);

        if (user == null)
            throw new UserNotFoundException($"User with email {query.Email} not found");

        return user;
    }
}
```

## Query Naming Conventions

Consistent naming helps developers understand what a query does at a glance.

### Recommended Patterns

**GetXQuery** - Retrieve a single item (expected to exist):
```csharp
GetOrderQuery
GetCustomerQuery
GetProductDetailsQuery
```

**GetXsQuery or GetXListQuery** - Retrieve a collection:
```csharp
GetOrdersQuery
GetCustomersQuery
GetProductListQuery
```

**FindXQuery** - Retrieve a single item (may not exist, returns null):
```csharp
FindCustomerByEmailQuery
FindOrderQuery
FindUserQuery
```

**SearchXQuery** - Search with criteria:
```csharp
SearchProductsQuery
SearchOrdersQuery
SearchCustomersQuery
```

**ListXQuery** - Retrieve a list (alternative to GetXsQuery):
```csharp
ListActiveOrdersQuery
ListRecentProductsQuery
```

### Naming Examples

```csharp
// Good query names
public sealed class GetOrderQuery : IQuery<Order> { }
public sealed class GetOrdersQuery : IQuery<IReadOnlyList<Order>> { }
public sealed class FindCustomerByEmailQuery : IQuery<Customer?> { }
public sealed class SearchProductsQuery : IQuery<IReadOnlyList<Product>> { }
public sealed class GetOrderCountQuery : IQuery<int> { }
public sealed class IsProductAvailableQuery : IQuery<bool> { }

// Avoid vague names
public sealed class Query1 : IQuery<Order> { }  // Bad
public sealed class GetDataQuery : IQuery<object> { }  // Bad
public sealed class DoStuffQuery : IQuery<string> { }  // Bad
```

Use descriptive, specific names that clearly communicate the query's purpose.

## Query Organization

### File Structure

Organize query files in a way that makes them easy to find and maintain:

**Option 1: Queries folder**
```
/Queries
    GetOrderQuery.cs
    GetCustomerQuery.cs
    SearchProductsQuery.cs
```

**Option 2: Feature folders**
```
/Features
    /Orders
        GetOrderQuery.cs
        GetOrdersListQuery.cs
    /Customers
        GetCustomerQuery.cs
        FindCustomerByEmailQuery.cs
```

**Option 3: Colocation with handlers**
```
/Orders
    /Queries
        GetOrderQuery.cs
        GetOrdersListQuery.cs
    /Handlers
        GetOrderQueryHandler.cs
        GetOrdersListQueryHandler.cs
```

Choose the structure that best fits your application's organization and team preferences.

### Colocation with Handlers

You can keep queries and their handlers in the same file for small projects:

```csharp
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp.Orders;

// Query
public sealed class GetOrderQuery : IQuery<Order>
{
    public GetOrderQuery(int orderId)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }
}

// Handler in same file
public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    private readonly IOrderRepository _repository;

    public GetOrderQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(query.OrderId, cancellationToken);
    }
}
```

This approach reduces file count and keeps related code together.

### Shared Query Library

For microservices or modular monoliths, consider a shared query library:

```
/MyApp.Contracts
    /Queries
        GetOrderQuery.cs
        GetCustomerQuery.cs

/MyApp.QueryHandlers
    /Orders
        GetOrderQueryHandler.cs
    /Customers
        GetCustomerQueryHandler.cs
```

This allows multiple services to reference the same query definitions without duplicating code.

## Query Patterns

### Pattern: Pagination Query

Queries for paginated data:

```csharp
public sealed class GetOrdersPageQuery : IQuery<PagedResult<OrderSummary>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize = 20)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}
```

### Pattern: Search Query

Queries with filter criteria:

```csharp
public sealed class SearchProductsQuery : IQuery<IReadOnlyList<Product>>
{
    public SearchProductsQuery(
        string? searchTerm = null,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        SearchTerm = searchTerm;
        Category = category;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }

    public string? SearchTerm { get; }
    public string? Category { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }
}
```

### Pattern: Projection Query

Queries that return specific fields:

```csharp
public sealed class GetCustomerSummaryQuery : IQuery<CustomerSummary>
{
    public GetCustomerSummaryQuery(int customerId)
    {
        CustomerId = customerId;
    }

    public int CustomerId { get; }
}

public class CustomerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    // Only the fields needed, not the entire Customer entity
}
```

### Pattern: Aggregation Query

Queries that return calculated or aggregated data:

```csharp
public sealed class GetSalesStatisticsQuery : IQuery<SalesStatistics>
{
    public GetSalesStatisticsQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
}

public class SalesStatistics
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int UniqueCustomers { get; set; }
}
```

## Query Object Best Practices

- **Make queries immutable** - Use read-only properties or init-only setters
- **Use descriptive names** - Queries should clearly indicate what data they retrieve
- **Keep queries simple** - Queries should only hold parameters, not business logic
- **Validate in constructors** - Use guard clauses for simple parameter validation
- **Use appropriate result types** - Return read-only collections, DTOs, or domain objects as appropriate
- **Use nullable types** - Make it explicit when a query may return no result
- **Seal query classes** - Use `sealed` to prevent inheritance and maintain immutability
- **Prefer composition** - Use filter objects for complex query parameters

## Query Object Common Pitfalls

- **Mutable query objects** - Avoid properties with public setters that can be changed after creation
- **Business logic in queries** - Keep calculation and business rules in handlers, not queries
- **Complex validation in queries** - Move complex validation to handlers or validation frameworks
- **Missing null handling** - Always consider whether a query can return null and use nullable types accordingly
- **Inappropriate result types** - Don't return mutable collections or over-fetch data
- **Vague naming** - Avoid generic names like "Query1" or "GetDataQuery"
- **Over-validation** - Don't perform expensive checks (like database lookups) in query constructors
- **Public mutable collections** - If a query needs a collection parameter, make it read-only

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Learn how to create handlers for your queries
- [Query Pipeline](/contents/QueryPipeline.md) - Understanding decorators and middleware for queries
- [Query Patterns](/contents/QueryPatterns.md) - Advanced patterns for real-world query scenarios
- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - Setting up Darker in your application
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Architectural patterns for CQRS

