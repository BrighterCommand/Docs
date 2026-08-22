---
description: "This guide presents common query patterns you'll encounter when building real-world applications with Darker."
layout:
  description:
    visible: false
---

# Query Patterns

> **How-to** · Applies to **Darker V4**

## Query Pattern Introduction

This guide presents common query patterns you'll encounter when building real-world applications with Darker. While [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) covers the fundamentals of query design, and [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) covers basic handler implementation, these pages focus on practical patterns for complex scenarios including pagination, projections, aggregations, and Entity Framework Core integration.

These patterns address real challenges like handling large data sets, optimizing query performance, working with related data, and implementing caching strategies. Each pattern includes complete, working examples that you can adapt to your specific needs.

## Performance Best Practices

### Pattern: Select Only What You Need

**Always project to DTOs** rather than loading full entities:

```csharp
// ✅ Good: Select only needed fields
.Select(o => new OrderDto
{
    Id = o.Id,
    OrderDate = o.OrderDate,
    CustomerName = o.Customer.Name
})

// ❌ Bad: Load entire entity
.Select(o => o)  // or .ToList() directly
```

### Pattern: Avoid N+1 Queries

**N+1 problem:** Loading a collection, then querying related data for each item.

```csharp
// ❌ Bad: N+1 queries (1 query for orders + N queries for customers)
var orders = await _dbContext.Orders.ToListAsync();
foreach (var order in orders)
{
    var customer = await _dbContext.Customers
        .FirstAsync(c => c.Id == order.CustomerId);  // N queries!
}

// ✅ Good: Single query with Include
var orders = await _dbContext.Orders
    .Include(o => o.Customer)
    .ToListAsync();

// ✅ Good: Single query with projection
var orders = await _dbContext.Orders
    .Select(o => new OrderDto
    {
        OrderId = o.Id,
        CustomerName = o.Customer.Name  // Automatic join
    })
    .ToListAsync();
```

### Pattern: Use Async All the Way

**Always use async methods** for I/O operations:

```csharp
// ✅ Good: Async all the way
public override async Task<Result> ExecuteAsync(Query query, CancellationToken ct)
{
    return await _dbContext.Products
        .ToListAsync(ct);  // Async
}

// ❌ Bad: Blocking on async
public override Task<Result> ExecuteAsync(Query query, CancellationToken ct)
{
    return Task.FromResult(_dbContext.Products.ToList());  // Blocking!
}
```

## Real-World Example: Product Catalog Query

Here's a complete, production-ready example combining multiple patterns:

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Query with multiple filters and pagination
public sealed class GetProductCatalogQuery : IQuery<PagedResult<ProductCatalogItemDto>>
{
    public GetProductCatalogQuery(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStockOnly = null,
        string sortBy = "Name",
        bool sortDescending = false)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        PageNumber = pageNumber;
        PageSize = pageSize;
        SearchTerm = searchTerm;
        CategoryId = categoryId;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        InStockOnly = inStockOnly;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
    public string? SearchTerm { get; }
    public int? CategoryId { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }
    public bool? InStockOnly { get; }
    public string SortBy { get; }
    public bool SortDescending { get; }
}

public class ProductCatalogItemDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool InStock { get; set; }
    public string ImageUrl { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountedPrice { get; set; }
}

// Handler with filters, sorting, and pagination
public sealed class GetProductCatalogQueryHandler :
    QueryHandlerAsync<GetProductCatalogQuery, PagedResult<ProductCatalogItemDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProductCatalogQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [QueryLogging(step: 1)]
    [RetryableQuery(step: 2, circuitBreakerName: "DatabaseCircuitBreaker")]
    public override async Task<PagedResult<ProductCatalogItemDto>> ExecuteAsync(
        GetProductCatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        // Build base query
        var productsQuery = _dbContext.Products.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                p.Description.Contains(query.SearchTerm));
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p =>
                p.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (query.InStockOnly.HasValue && query.InStockOnly.Value)
        {
            productsQuery = productsQuery.Where(p => p.StockQuantity > 0);
        }

        // Apply sorting
        productsQuery = query.SortBy.ToLower() switch
        {
            "price" => query.SortDescending
                ? productsQuery.OrderByDescending(p => p.Price)
                : productsQuery.OrderBy(p => p.Price),
            "name" => query.SortDescending
                ? productsQuery.OrderByDescending(p => p.Name)
                : productsQuery.OrderBy(p => p.Name),
            _ => productsQuery.OrderBy(p => p.Name)
        };

        // Get total count (before pagination)
        var totalCount = await productsQuery.CountAsync(cancellationToken);

        // Apply pagination and projection
        var products = await productsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(p => p.Category)
            .Select(p => new ProductCatalogItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryName = p.Category.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                InStock = p.StockQuantity > 0,
                ImageUrl = p.ImageUrl,
                DiscountPercent = p.DiscountPercent,
                DiscountedPrice = p.DiscountPercent.HasValue
                    ? p.Price * (1 - p.DiscountPercent.Value / 100)
                    : null
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductCatalogItemDto>(
            products,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
```

**Usage in controller:**
```csharp
[HttpGet("products")]
public async Task<IActionResult> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    [FromQuery] int? categoryId = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] bool inStockOnly = false,
    [FromQuery] string sortBy = "Name",
    [FromQuery] bool sortDesc = false,
    CancellationToken cancellationToken = default)
{
    var query = new GetProductCatalogQuery(
        page,
        pageSize,
        search,
        categoryId,
        minPrice,
        maxPrice,
        inStockOnly,
        sortBy,
        sortDesc);

    var result = await _queryProcessor.ExecuteAsync(query, cancellationToken);
    return Ok(result);
}
```

## Best Practices Summary

1. **Use pagination** for any query that could return more than 100 items
2. **Project to DTOs** using `Select()` - don't return domain entities
3. **Always use `AsNoTracking()`** for read-only queries
4. **Use `Include()` wisely** to avoid N+1 queries, but prefer projection when possible
5. **Cache appropriately** - small, static lookup data is a good candidate
6. **Handle nulls explicitly** - use nullable reference types (`CustomerDto?`)
7. **Use `CancellationToken`** - pass it through to all async operations
8. **Validate query parameters** in the query constructor
9. **Use compiled queries** for hot-path queries
10. **Consider read replicas** for scaling read-heavy workloads

## Query Pattern Common Pitfalls

1. **Loading entire collections without pagination** - Always paginate large result sets
2. **Forgetting `AsNoTracking()`** - Wastes memory and CPU for read-only queries
3. **N+1 query problems** - Use `Include()` or projections to avoid multiple round trips
4. **Over-fetching data** - Select only the fields you need
5. **Under-fetching (multiple queries)** - Use joins/includes to get related data in one query
6. **Not using `CancellationToken`** - Prevents graceful cancellation of long-running queries
7. **Returning domain entities** - Always project to DTOs for the query side
8. **Caching too aggressively** - Consider staleness tolerance and cache invalidation
9. **Not optimizing database indexes** - Ensure indexes exist for filter/sort columns
10. **Ignoring query performance** - Monitor slow queries and optimize hot paths

## Further Reading

- [Parameterized Query Patterns](/contents/ParameterizedQueryPatterns.md) - Lookups, filtered lists and multi-criteria search
- [Pagination Query Patterns](/contents/PaginationQueryPatterns.md) - Offset-based and cursor-based paging
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Returning only the fields a caller needs
- [Collection and Aggregation Query Patterns](/contents/AggregationQueryPatterns.md) - Collections, counts and summary statistics
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Tracking, eager loading and compiled queries
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Basic handler implementation patterns
- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - Query design fundamentals
- [Query Pipeline](/contents/QueryPipeline.md) - Decorators, logging, and resilience policies
- [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md) - Getting started with Darker
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Architectural patterns
