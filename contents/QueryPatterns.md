# Query Patterns

## Introduction

This guide presents common query patterns you'll encounter when building real-world applications with Darker. While [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) covers the fundamentals of query design, and [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) covers basic handler implementation, this document focuses on practical patterns for complex scenarios including pagination, projections, aggregations, and Entity Framework Core integration.

These patterns address real challenges like handling large data sets, optimizing query performance, working with related data, and implementing caching strategies. Each pattern includes complete, working examples that you can adapt to your specific needs.

## Parameterized Query Patterns

### Pattern: Single Entity Lookup

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

### Pattern: Filtered List

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

### Pattern: Search with Multiple Criteria

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

## Pagination Patterns

### Pattern: Offset-Based Pagination

**Use Case:** Standard pagination for most applications

Offset-based pagination is the most common pattern, using page number and page size.

```csharp
using Paramore.Darker;
using System;
using System.Collections.Generic;

public sealed class GetOrdersPageQuery : IQuery<PagedResult<OrderDto>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100");

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}

// Generic paged result wrapper
public class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
}
```

**Handler with pagination:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrdersPageQueryHandler :
    QueryHandlerAsync<GetOrdersPageQuery, PagedResult<OrderDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrdersPageQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<PagedResult<OrderDto>> ExecuteAsync(
        GetOrdersPageQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get total count (before pagination)
        var totalCount = await _dbContext.Orders
            .CountAsync(cancellationToken);

        // Get page of items
        var orders = await _dbContext.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer.Name,
                Total = o.Items.Sum(i => i.Quantity * i.UnitPrice)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>(
            orders,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
```

**When to use:** Most pagination scenarios, especially when total count is needed for UI.

**Trade-offs:**
- ✅ Simple to implement
- ✅ Supports jumping to specific pages
- ✅ Total count available for UI
- ❌ Performance degrades with large offsets (page 1000 is slow)
- ❌ Can show duplicates/skips if data changes between page requests

### Pattern: Cursor-Based Pagination

**Use Case:** Efficient pagination for large datasets or real-time data

Cursor-based pagination uses a unique identifier to mark position in the result set.

```csharp
using Paramore.Darker;
using System;
using System.Collections.Generic;

public sealed class GetOrdersCursorQuery : IQuery<CursorPagedResult<OrderDto>>
{
    public GetOrdersCursorQuery(int pageSize, int? afterOrderId = null)
    {
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100");

        PageSize = pageSize;
        AfterOrderId = afterOrderId;
    }

    public int PageSize { get; }
    public int? AfterOrderId { get; }  // Cursor
}

public class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int? NextCursor { get; init; }  // ID of last item for next page
    public bool HasMore { get; init; }
}
```

**Handler with cursor pagination:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrdersCursorQueryHandler :
    QueryHandlerAsync<GetOrdersCursorQuery, CursorPagedResult<OrderDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrdersCursorQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CursorPagedResult<OrderDto>> ExecuteAsync(
        GetOrdersCursorQuery query,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = _dbContext.Orders
            .OrderByDescending(o => o.Id);  // Must order by cursor field

        // Apply cursor filter
        if (query.AfterOrderId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.Id < query.AfterOrderId.Value);
        }

        // Get one extra item to determine if there are more results
        var orders = await ordersQuery
            .Take(query.PageSize + 1)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer.Name,
                Total = o.Items.Sum(i => i.Quantity * i.UnitPrice)
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var hasMore = orders.Count > query.PageSize;
        var items = hasMore ? orders.Take(query.PageSize).ToList() : orders;
        var nextCursor = hasMore ? items.Last().Id : (int?)null;

        return new CursorPagedResult<OrderDto>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}
```

**When to use:** Real-time feeds, infinite scroll, large datasets

**Benefits:**
- ✅ Consistent performance regardless of depth
- ✅ No duplicates/skips when data changes
- ✅ Efficient for large datasets

**Trade-offs:**
- ❌ Cannot jump to arbitrary pages
- ❌ No total count
- ❌ More complex to implement

## Projection Patterns

### Pattern: Simple Projection

**Use Case:** Return only a subset of entity properties

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCustomerSummariesQuery : IQuery<IReadOnlyList<CustomerSummaryDto>>
{
}

public class CustomerSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    // Omit sensitive fields like password hash, internal notes, etc.
}

public sealed class GetCustomerSummariesQueryHandler :
    QueryHandlerAsync<GetCustomerSummariesQuery, IReadOnlyList<CustomerSummaryDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCustomerSummariesQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<IReadOnlyList<CustomerSummaryDto>> ExecuteAsync(
        GetCustomerSummariesQuery query,
        CancellationToken cancellationToken = default)
    {
        // EF Core translates this to SELECT Id, Name, Email only
        return await _dbContext.Customers
            .Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
```

**When to use:** Optimize queries by selecting only needed fields, hide sensitive data.

### Pattern: Complex Projection with Joins

**Use Case:** Aggregate data from multiple related entities

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrderDetailsQuery : IQuery<OrderDetailsDto?>
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
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string ShippingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public class OrderItemDto
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class GetOrderDetailsQueryHandler :
    QueryHandlerAsync<GetOrderDetailsQuery, OrderDetailsDto?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrderDetailsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<OrderDetailsDto?> ExecuteAsync(
        GetOrderDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDetailsDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer.Name,
                CustomerEmail = o.Customer.Email,
                ShippingAddress = $"{o.ShippingAddress.Street}, {o.ShippingAddress.City}",
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList(),
                SubTotal = o.Items.Sum(i => i.Quantity * i.UnitPrice),
                Tax = o.Items.Sum(i => i.Quantity * i.UnitPrice) * 0.1m,  // 10% tax
                Total = o.Items.Sum(i => i.Quantity * i.UnitPrice) * 1.1m
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**When to use:** Denormalized views that combine data from multiple entities.

### Pattern: Calculated Fields

**Use Case:** Include computed values in query results

Calculated fields can be computed in the query projection (database) or in the handler code (application).

**Database-computed fields (preferred for performance):**
```csharp
.Select(o => new OrderDto
{
    Id = o.Id,
    TotalItems = o.Items.Sum(i => i.Quantity),  // Computed in database
    TotalAmount = o.Items.Sum(i => i.Quantity * i.UnitPrice),
    AverageItemPrice = o.Items.Average(i => i.UnitPrice)
})
```

**Application-computed fields:**
```csharp
public class OrderStatisticsDto
{
    public int OrderId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; }

    // Computed property
    public decimal Tax => SubTotal * TaxRate;
    public decimal Total => SubTotal + Tax;
}
```

## Collection and Aggregation Patterns

### Pattern: Small Collection (Get All)

**Use Case:** Retrieve entire small collection that can be cached

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetAllCategoriesQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}

public sealed class GetAllCategoriesQueryHandler :
    QueryHandlerAsync<GetAllCategoriesQuery, IReadOnlyDictionary<int, string>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllCategoriesQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetAllCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return categories;
    }
}
```

**When to use:** Small, relatively static lookup tables (< 100 items), often cached.

### Pattern: Count Query

**Use Case:** Get count of items matching criteria

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPendingOrderCountQuery : IQuery<int>
{
    public GetPendingOrderCountQuery(int? customerId = null)
    {
        CustomerId = customerId;
    }

    public int? CustomerId { get; }
}

public sealed class GetPendingOrderCountQueryHandler :
    QueryHandlerAsync<GetPendingOrderCountQuery, int>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPendingOrderCountQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<int> ExecuteAsync(
        GetPendingOrderCountQuery query,
        CancellationToken cancellationToken = default)
    {
        var ordersQuery = _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Pending);

        if (query.CustomerId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CustomerId == query.CustomerId.Value);
        }

        return await ordersQuery.CountAsync(cancellationToken);
    }
}
```

**When to use:** Dashboard metrics, badge counts, pagination totals.

### Pattern: Summary/Statistics

**Use Case:** Aggregate calculations (sum, average, min, max)

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetSalesStatisticsQuery : IQuery<SalesStatisticsDto>
{
    public GetSalesStatisticsQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
}

public class SalesStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal MinimumOrderValue { get; set; }
    public decimal MaximumOrderValue { get; set; }
}

public sealed class GetSalesStatisticsQueryHandler :
    QueryHandlerAsync<GetSalesStatisticsQuery, SalesStatisticsDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetSalesStatisticsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<SalesStatisticsDto> ExecuteAsync(
        GetSalesStatisticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = _dbContext.Orders
            .Where(o => o.OrderDate >= query.StartDate && o.OrderDate <= query.EndDate);

        var statistics = await orders
            .GroupBy(o => 1)  // Group all into single group for aggregations
            .Select(g => new SalesStatisticsDto
            {
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)),
                AverageOrderValue = g.Average(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)),
                MinimumOrderValue = g.Min(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)),
                MaximumOrderValue = g.Max(o => o.Items.Sum(i => i.Quantity * i.UnitPrice))
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Return zero statistics if no orders found
        return statistics ?? new SalesStatisticsDto();
    }
}
```

**When to use:** Reports, dashboards, analytics.

## Entity Framework Core Integration

### Pattern: AsNoTracking for Read-Only Queries

**Always use `AsNoTracking()` for query handlers**. Since queries don't modify data, change tracking is unnecessary overhead.

```csharp
// ✅ Good: AsNoTracking for read-only queries
public override async Task<List<ProductDto>> ExecuteAsync(...)
{
    return await _dbContext.Products
        .AsNoTracking()  // Disables change tracking
        .Select(p => new ProductDto { /* ... */ })
        .ToListAsync(cancellationToken);
}

// ❌ Bad: Change tracking enabled (default)
public override async Task<List<Product>> ExecuteAsync(...)
{
    // Change tracking is enabled by default - unnecessary for queries!
    return await _dbContext.Products
        .ToListAsync(cancellationToken);
}
```

**Performance benefits:**
- Reduced memory usage
- Faster query execution
- No overhead for tracking entity state

### Pattern: Include Related Data (Eager Loading)

**Use `Include()` and `ThenInclude()` to load related entities** in a single query, avoiding N+1 query problems.

```csharp
using Microsoft.EntityFrameworkCore;

public override async Task<OrderDto> ExecuteAsync(...)
{
    return await _dbContext.Orders
        .Include(o => o.Customer)           // Load related Customer
        .Include(o => o.Items)              // Load related OrderItems collection
            .ThenInclude(i => i.Product)    // Load Product for each OrderItem
        .Where(o => o.Id == query.OrderId)
        .Select(o => new OrderDto
        {
            OrderId = o.Id,
            CustomerName = o.Customer.Name,  // Access included Customer
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductName = i.Product.Name  // Access included Product
            }).ToList()
        })
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken);
}
```

**When to use:** When you need related data and want to avoid multiple database round trips.

**Alternative: Projection without Include:**
```csharp
// You don't need Include if you're projecting with Select
public override async Task<OrderDto> ExecuteAsync(...)
{
    return await _dbContext.Orders
        .Where(o => o.Id == query.OrderId)
        .Select(o => new OrderDto  // Projection handles joins automatically
        {
            OrderId = o.Id,
            CustomerName = o.Customer.Name,  // EF Core joins automatically
            ItemCount = o.Items.Count()
        })
        .AsNoTracking()
        .FirstOrDefaultAsync(cancellationToken);
}
```

### Pattern: Scoped Lifetime for EF Core

**Critical:** When using Entity Framework Core, configure the Query Processor with scoped lifetime to match the DbContext lifetime:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

builder.Services.AddDarker(options =>
{
    // Match DbContext lifetime (scoped by default)
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(Program).Assembly);
```

For more details, see [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md#query-processor-lifetime).

### Pattern: Compiled Queries

**Use compiled queries for frequently executed queries** to improve performance by caching the query translation.

```csharp
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetCustomerByIdQueryHandler : QueryHandlerAsync<GetCustomerByIdQuery, CustomerDto?>
{
    private static readonly Func<ApplicationDbContext, int, CancellationToken, Task<CustomerDto?>>
        CompiledQuery = EF.CompileAsyncQuery(
            (ApplicationDbContext context, int customerId, CancellationToken ct) =>
                context.Customers
                    .Where(c => c.Id == customerId)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email
                    })
                    .AsNoTracking()
                    .FirstOrDefault());

    private readonly ApplicationDbContext _dbContext;

    public GetCustomerByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CustomerDto?> ExecuteAsync(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return await CompiledQuery(_dbContext, query.CustomerId, cancellationToken);
    }
}
```

**When to use:** Hot-path queries executed frequently (thousands of times per second).

**Trade-offs:**
- ✅ Faster query execution (cached translation)
- ❌ More complex code
- ❌ Only beneficial for high-frequency queries

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

## Common Pitfalls

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

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Basic handler implementation patterns
- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - Query design fundamentals
- [Query Pipeline](/contents/QueryPipeline.md) - Decorators, logging, and resilience policies
- [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md) - Getting started with Darker
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Architectural patterns
