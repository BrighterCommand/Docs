---
description: "How to return only the fields a caller needs: simple projections, projections across joins, and calculated fields."
layout:
  description:
    visible: false
---

# Projection Query Patterns

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Patterns](/contents/QueryPatterns.md)

How to return only the fields a caller needs: simple projections, projections across joins, and calculated fields. For the patterns these build on, see [Query Patterns](/contents/QueryPatterns.md).

## Simple Projection Pattern

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

## Complex Projection Pattern with Joins

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

## Calculated Fields Projection Pattern

**Use Case:** Include computed values in query results

Calculated fields can be computed in the query projection (database) or in the handler code (application).

**Database-computed fields (preferred for performance):**
```csharp
// ...
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
// ...
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

## Further Reading

- [Query Patterns](/contents/QueryPatterns.md) - Performance guidance and a worked example
- [Parameterized Query Patterns](/contents/ParameterizedQueryPatterns.md) - Lookups, filtered lists and multi-criteria search
- [Pagination Query Patterns](/contents/PaginationQueryPatterns.md) - Offset-based and cursor-based paging
- [Collection and Aggregation Query Patterns](/contents/AggregationQueryPatterns.md) - Collections, counts and summary statistics
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Tracking, eager loading and compiled queries
