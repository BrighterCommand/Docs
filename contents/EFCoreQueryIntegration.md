---
description: "How to run Darker queries against Entity Framework Core: tracking, eager loading, handler lifetime and compiled queries."
layout:
  description:
    visible: false
---

# Entity Framework Core Query Integration

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Patterns](/contents/QueryPatterns.md)

How to run Darker queries against Entity Framework Core: tracking, eager loading, handler lifetime and compiled queries. For the patterns these build on, see [Query Patterns](/contents/QueryPatterns.md).

## EF Core AsNoTracking for Read-Only Queries

**Always use `AsNoTracking()` for query handlers**. Since queries don't modify data, change tracking is unnecessary overhead.

```csharp
// ...
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

## EF Core Include for Related Data (Eager Loading)

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
// ...
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

## Scoped Handler Lifetime for EF Core

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

For more details, see [Darker Configuration Reference](/contents/DarkerConfigurationReference.md#darker-query-processor-lifetime).

## EF Core Compiled Queries

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

## Further Reading

- [Query Patterns](/contents/QueryPatterns.md) - Performance guidance and a worked example
- [Parameterized Query Patterns](/contents/ParameterizedQueryPatterns.md) - Lookups, filtered lists and multi-criteria search
- [Pagination Query Patterns](/contents/PaginationQueryPatterns.md) - Offset-based and cursor-based paging
- [Projection Query Patterns](/contents/ProjectionQueryPatterns.md) - Returning only the fields a caller needs
- [Collection and Aggregation Query Patterns](/contents/AggregationQueryPatterns.md) - Collections, counts and summary statistics
