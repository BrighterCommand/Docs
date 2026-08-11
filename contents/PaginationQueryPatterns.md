# Pagination Query Patterns

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Patterns](/contents/QueryPatterns.md)

How to page through a large result set in Darker, by offset and by cursor, with the trade-offs of each. For the patterns these build on, see [Query Patterns](/contents/QueryPatterns.md).

## Offset-Based Pagination Pattern

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

## Cursor-Based Pagination Pattern

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

## Further Reading

- [Query Patterns](/contents/QueryPatterns.md) - Performance guidance and a worked example
- [Parameterized Query Patterns](/contents/ParameterizedQueryPatterns.md) - Lookups, filtered lists and multi-criteria search
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Returning only the fields a caller needs
- [Collection and Aggregation Query Patterns](/contents/AggregationQueryPatterns.md) - Collections, counts and summary statistics
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Tracking, eager loading and compiled queries
