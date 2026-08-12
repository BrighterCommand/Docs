# CQRS Use Cases and Patterns

> **Explanation** · Applies to **Brighter V10 and Darker V4** · Prerequisites: [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)

Four shapes a CQRS system takes in practice, from a single database with two models to
event-sourced writes with projected reads. Each names the scenario it suits and shows the
Brighter and Darker code for it. [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)
is the page that explains the pattern itself.

## Pattern: Simple CQRS (Same Database)

**Scenario:** Single database with different models for commands and queries

This is the simplest CQRS pattern, suitable for most applications. Both commands and queries use the same database, but with different models and optimizations.

```csharp
// ...
// Write Model (Domain Entity) - normalized, enforces business rules
public class Order
{
    private readonly List<OrderItem> _items = new();

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(int productId, int quantity, decimal price)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be positive");

        _items.Add(new OrderItem(productId, quantity, price));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel shipped order");

        Status = OrderStatus.Cancelled;
    }
}

// Read Model (DTO) - denormalized, optimized for display
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }  // Joined from Customer table
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
}

// Query Handler - optimized for read performance
public class GetOrderSummaryQueryHandler :
    QueryHandlerAsync<GetOrderSummaryQuery, OrderSummaryDto>
{
    private readonly ApplicationDbContext _dbContext;

    public override async Task<OrderSummaryDto> ExecuteAsync(
        GetOrderSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                CustomerName = o.Customer.Name,  // Joined
                ItemCount = o.Items.Count,        // Aggregated
                TotalAmount = o.Items.Sum(i => i.Price * i.Quantity),
                Status = o.Status.ToString(),
                OrderDate = o.CreatedAt
            })
            .AsNoTracking()  // Read-only optimization
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**When to use:**

- Starting with CQRS
- Single data store is sufficient
- Read and write performance requirements are similar
- Simplified deployment and operations

**Benefits:**

- Simpler than separate databases
- No eventual consistency concerns
- Easier to maintain
- Still get CQRS benefits (separate models, clear responsibilities)

## CQRS Pattern: Separate Read/Write Databases

**Scenario:** Commands write to a primary database; queries read from a replica or separate optimized read database

```text
┌────────────────┐
│   Commands     │
│   (Brighter)   │
└───────┬────────┘
        ↓
   Write to Primary
        ↓
┌───────────────┐
│  Write DB     │
│  (Master)     │
└───────┬───────┘
        │ Replication / Sync
        ↓
┌───────────────┐
│  Read DB      │◄─────────┐
│  (Replica)    │          │
└───────────────┘          │
                    ┌──────┴─────┐
                    │  Queries   │
                    │  (Darker)  │
                    └────────────┘
```

**When to use:**

- High read-to-write ratio (e.g., 100:1)
- Need to scale reads independently
- Can tolerate eventual consistency
- Read database can be optimized differently (e.g., different indexes, denormalization)

**Trade-offs:**

- **Eventual consistency**: Reads may lag behind writes
- **Complexity**: Need to manage replication and sync
- **Cost**: Multiple databases to manage

## CQRS Pattern: Event-Sourced Writes, Projected Reads

**Scenario:** Commands produce events that are stored; queries read projections built from events

This advanced pattern stores all state changes as a sequence of events. The query side builds read models (projections) by replaying events.

```text
┌────────────────┐
│   Commands     │
│   (Brighter)   │
└───────┬────────┘
        ↓
  Produce Events
        ↓
┌───────────────┐       Event Stream
│  Event Store  ├─────────────────────┐
└───────────────┘                     │
                                      ↓
                             ┌────────────────┐
                             │  Projection    │
                             │  Builder       │
                             └────────┬───────┘
                                      ↓
                               Build Read Models
                                      ↓
                             ┌────────────────┐
                             │ Read Database  │◄────┐
                             └────────────────┘     │
                                             ┌──────┴─────┐
                                             │  Queries   │
                                             │  (Darker)  │
                                             └────────────┘
```

**When to use:**

- Need complete audit trail
- Temporal queries ("What was the state at time X?")
- Complex domain with many state transitions
- Need to rebuild read models from scratch

## CQRS Pattern: Task-Based UI

**Scenario:** User interface actions map directly to commands; page loads map to queries

In a task-based UI, instead of generic CRUD operations, the UI presents specific business tasks as commands:

```csharp
// ...
// Task-based commands (specific business operations)
public class ApproveOrderCommand : IRequest { /* ... */ }
public class RejectOrderCommand : IRequest { /* ... */ }
public class ShipOrderCommand : IRequest { /* ... */ }

// Generic queries for display
public class GetOrderForApprovalQuery : IQuery<OrderApprovalDto> { /* ... */ }

// Controller
[HttpPost("orders/{orderId}/approve")]
public async Task<IActionResult> ApproveOrder(int orderId)
{
    await _commandProcessor.SendAsync(new ApproveOrderCommand(orderId));
    return Ok();
}

[HttpGet("orders/{orderId}/approval")]
public async Task<IActionResult> GetOrderForApproval(int orderId)
{
    var result = await _queryProcessor.ExecuteAsync(
        new GetOrderForApprovalQuery(orderId));
    return Ok(result);
}
```

**Benefits:**

- UI reflects business domain
- Commands capture business intent
- Easier to implement business rules
- Better audit trail

---

## Further Reading

- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - The pattern, and how the two frameworks fit together
- [Event Driven Collaboration](/contents/EventDrivenCollaboration.md) - Using events in distributed systems
- [Outbox Pattern](/contents/OutboxPattern.md) - Reliable event publishing with Brighter
