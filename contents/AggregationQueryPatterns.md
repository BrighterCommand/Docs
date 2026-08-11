# Collection and Aggregation Query Patterns

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Patterns](/contents/QueryPatterns.md)

How to return whole collections, counts, and summary statistics from a query handler. For the patterns these build on, see [Query Patterns](/contents/QueryPatterns.md).

## Small Collection Pattern (Get All)

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

## Count Query Pattern

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

## Summary and Statistics Aggregation Pattern

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

## Further Reading

- [Query Patterns](/contents/QueryPatterns.md) - Performance guidance and a worked example
- [Parameterized Query Patterns](/contents/ParameterizedQueryPatterns.md) - Lookups, filtered lists and multi-criteria search
- [Pagination Query Patterns](/contents/PaginationQueryPatterns.md) - Offset-based and cursor-based paging
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Returning only the fields a caller needs
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Tracking, eager loading and compiled queries
