# Parameterized Query Patterns

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Patterns](/contents/QueryPatterns.md)

Query recipes that take parameters: looking up a single entity, filtering a list, and searching on several criteria at once. For the patterns these build on, see [Query Patterns](/contents/QueryPatterns.md).

## Parameterized Query Pattern: Single Entity Lookup

**Use Case:** Retrieve a single entity by its unique identifier

This is the most common query pattern - retrieving one entity when you have its ID or another unique key.

```csharp
using Paramore.Darker;

// Query by primary key
public sealed class GetPersonNameQuery : IQuery<string>
{
    public GetPersonNameQuery(int personId)
    {
        PersonId = personId;
    }

    public int PersonId { get; }
}

// Query by unique alternate key
public sealed class GetCustomerByEmailQuery : IQuery<CustomerDto?>
{
    public GetCustomerByEmailQuery(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Email = email;
    }

    public string Email { get; }
}

// Query by composite key
public sealed class GetOrderLineQuery : IQuery<OrderLineDto?>
{
    public GetOrderLineQuery(int orderId, int lineNumber)
    {
        OrderId = orderId;
        LineNumber = lineNumber;
    }

    public int OrderId { get; }
    public int LineNumber { get; }
}
```

**Handler Example:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCustomerByEmailQueryHandler :
    QueryHandlerAsync<GetCustomerByEmailQuery, CustomerDto?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCustomerByEmailQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CustomerDto?> ExecuteAsync(
        GetCustomerByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .Where(c => c.Email == query.Email)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**When to use:** Direct entity lookups by ID, email, username, or other unique keys.

## Parameterized Query Pattern: Filtered List

**Use Case:** Retrieve a list of entities matching specific criteria

```csharp
using Paramore.Darker;
using System;
using System.Collections.Generic;

public sealed class GetOrdersByCustomerQuery : IQuery<IReadOnlyList<OrderSummaryDto>>
{
    public GetOrdersByCustomerQuery(int customerId, DateTime? since = null)
    {
        CustomerId = customerId;
        Since = since;
    }

    public int CustomerId { get; }
    public DateTime? Since { get; }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
}
```

**Handler with optional filters:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Task;

public sealed class GetOrdersByCustomerQueryHandler :
    QueryHandlerAsync<GetOrdersByCustomerQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrdersByCustomerQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<IReadOnlyList<OrderSummaryDto>> ExecuteAsync(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = _dbContext.Orders
            .Where(o => o.CustomerId == query.CustomerId);

        // Apply optional filter
        if (query.Since.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= query.Since.Value);
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                TotalAmount = o.Items.Sum(i => i.Quantity * i.UnitPrice)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return orders;
    }
}
```

**When to use:** Filtered lists where you know all results will fit in memory (typically < 1000 items). For larger result sets, use pagination.

## Parameterized Query Pattern: Search with Multiple Criteria

**Use Case:** Complex search with multiple optional filters

```csharp
using Paramore.Darker;
using System;
using System.Collections.Generic;

public sealed class SearchProductsQuery : IQuery<IReadOnlyList<ProductDto>>
{
    public string? NameFilter { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

**Handler with multiple optional criteria:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class SearchProductsQueryHandler :
    QueryHandlerAsync<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public SearchProductsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<IReadOnlyList<ProductDto>> ExecuteAsync(
        SearchProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = _dbContext.Products.AsQueryable();

        // Apply filters conditionally
        if (!string.IsNullOrWhiteSpace(query.NameFilter))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.NameFilter));
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p =>
                p.CategoryId == query.CategoryId.Value);
        }

        if (query.InStock.HasValue && query.InStock.Value)
        {
            productsQuery = productsQuery.Where(p => p.StockQuantity > 0);
        }

        var products = await productsQuery
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return products;
    }
}
```

**When to use:** Search interfaces with multiple optional filter parameters. Consider adding pagination for production use.

## Further Reading

- [Query Patterns](/contents/QueryPatterns.md) - Performance guidance and a worked example
- [Pagination Query Patterns](/contents/PaginationQueryPatterns.md) - Offset-based and cursor-based paging
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Returning only the fields a caller needs
- [Collection and Aggregation Query Patterns](/contents/AggregationQueryPatterns.md) - Collections, counts and summary statistics
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Tracking, eager loading and compiled queries
