---
description: "The result type specified in IQuery<TResult> can be any C# type."
layout:
  description:
    visible: false
---

# Query Result Types

> **Explanation** · Applies to **Darker V4** · Prerequisites: [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)

The result type specified in `IQuery<TResult>` can be any C# type. Choose the appropriate type based on what data the query needs to return.

## Primitive Query Results

Use primitive types for simple single-value queries:

```csharp
// ...
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

## DTO and Projection Query Results

For complex data, return Data Transfer Objects (DTOs) or projections:

```csharp
// ...
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

## Collection Query Results

Use collection types for queries that return multiple items:

```csharp
// ...
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

## Dictionary Query Results

Use dictionaries when returning key-value pairs:

```csharp
// ...
public sealed class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}

public sealed class GetProductPricesQuery : IQuery<Dictionary<string, decimal>>
{
}
```

Dictionaries are useful for lookup scenarios where you need fast access by key.

## Nullable Query Results

Use nullable types when a query might not return a result:

```csharp
// ...
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

## Complex Query Result Types

For advanced scenarios, you can return tuples, custom result wrappers, or domain objects:

```csharp
// ...
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

## Further Reading

- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - The query objects that declare these types
- [Query Object Validation](/contents/QueryObjectValidation.md) - Keeping an invalid query away from a handler
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Building projection types in a handler
