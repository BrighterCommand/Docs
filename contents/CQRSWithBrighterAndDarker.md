# CQRS with Brighter and Darker

## Introduction

Command Query Responsibility Segregation (CQRS) is an architectural pattern that separates the responsibility for handling commands (operations that change state) from queries (operations that read state). **Brighter** and **Darker** together provide a complete, production-ready CQRS solution for .NET applications, where Brighter handles the command side and Darker handles the query side.

This separation allows you to optimize each side independently: write operations can focus on business logic validation and consistency, while read operations can be optimized for performance and specific client needs. Both frameworks use the same architectural patterns (request handlers, middleware pipelines, policy-based resilience), making it easier for teams to work with both sides of a CQRS architecture using consistent patterns.

In this guide, we'll explore how Brighter and Darker work together, common architectural patterns, and when CQRS is (and isn't) the right choice for your application.

## CQRS Fundamentals

### Command Query Separation (CQS)

**Command Query Separation** is a fundamental principle introduced by Bertrand Meyer that states: a method should either change state (a command) or return data (a query), but never both. This principle is defined in [Basic Concepts](/contents/BasicConcepts.md#command-query-separation-cqs):

> Command-Query separation is the principle that because a query should never have the unexpected side-effect of updating state, a query should clearly be distinguished from a command. A query reports on the state of a domain, a command changes it.

**Key principles:**

- **Commands** change state but don't return data (or return minimal acknowledgment)
- **Queries** return data but never change state
- Clear distinction prevents unexpected side effects

**Example violations of CQS:**
```csharp
// ❌ Bad: Query with side effects
public Customer GetCustomer(int id)
{
    var customer = _repository.GetById(id);
    customer.LastAccessed = DateTime.UtcNow;  // Side effect!
    _repository.Update(customer);
    return customer;
}

// ✅ Good: Separate command and query
public void UpdateLastAccessed(int customerId)  // Command
{
    var customer = _repository.GetById(customerId);
    customer.LastAccessed = DateTime.UtcNow;
    _repository.Update(customer);
}

public Customer GetCustomer(int id)  // Query
{
    return _repository.GetById(id);  // No side effects
}
```

### Command Query Responsibility Segregation (CQRS)

CQRS extends CQS to the architectural level by using **separate models** for reads and writes. While CQS separates methods, CQRS separates entire subsystems:

**Key principles:**

- **Separate models**: Write model focuses on business rules; read model focuses on presentation needs
- **Different optimization strategies**: Write side optimized for consistency and validation; read side optimized for query performance
- **Eventual consistency**: When using separate data stores, read models may lag behind write models
- **Scalability**: Read and write sides can scale independently

**CQRS spectrum:**

1. **Simple CQRS**: Same database, different models (queries and commands)
2. **Separate stores**: Different databases optimized for reads vs writes
3. **Event-sourced writes**: Commands produce events, queries read projections built from events

The level of separation you choose depends on your application's complexity and requirements.

## Brighter: The Command Side

[Brighter](https://github.com/BrighterCommand/Brighter) is the command-side framework that handles operations that change state. It provides a complete solution for command processing, event publishing, and integration with external message brokers.

### What Brighter Provides

**Command Processing:**

- Command Processor pattern for dispatching commands to handlers
- Request handlers with middleware pipelines for cross-cutting concerns
- Support for both synchronous and asynchronous operations

**Event Publishing:**

- Event-driven architectures with internal and external event publishing
- Support for multiple subscribers to events
- Integration with message-oriented middleware (RabbitMQ, AWS SNS/SQS, Kafka, Azure Service Bus)

**Reliability Patterns:**

- Retry policies and circuit breakers via Polly integration
- Outbox pattern for reliable message publishing
- Inbox pattern for idempotent message handling

**External Bus Support:**

- Message mappers for converting commands/events to messages
- Service Activator for consuming messages from queues/streams
- Support for distributed systems and microservices

### Command Handler Example

Here's a brief example of a Brighter command handler. For complete details, see [Dispatching Requests](/contents/DispatchingARequest.md):

```csharp
using Paramore.Brighter;
using System.Threading;
using System.Threading.Tasks;

// Command: represents an intent to change state
public class PlaceOrderCommand : IRequest
{
    public PlaceOrderCommand(int customerId, List<OrderItem> items)
    {
        CustomerId = customerId;
        Items = items;
    }

    public int CustomerId { get; }
    public List<OrderItem> Items { get; }
}

// Command Handler: validates and executes the command
public class PlaceOrderCommandHandler : RequestHandlerAsync<PlaceOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAmACommandProcessor _commandProcessor;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IAmACommandProcessor commandProcessor)
    {
        _orderRepository = orderRepository;
        _commandProcessor = commandProcessor;
    }

    [RequestLogging(step: 1)]
    [UsePolicy("OrderServiceRetry", step: 2)]
    public override async Task<PlaceOrderCommand> HandleAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate business rules
        ValidateOrder(command);

        // Create order
        var order = new Order
        {
            CustomerId = command.CustomerId,
            Items = command.Items,
            Status = OrderStatus.Placed,
            PlacedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        // Publish event (for eventual consistency with read model)
        await _commandProcessor.PublishAsync(
            new OrderPlacedEvent(order.Id, order.CustomerId),
            cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }

    private void ValidateOrder(PlaceOrderCommand command)
    {
        if (command.Items == null || !command.Items.Any())
            throw new InvalidOperationException("Order must contain at least one item");
    }
}
```

**Key characteristics:**

- Commands are explicit, named operations (PlaceOrderCommand)
- Handlers contain business logic and validation
- Events are published to notify other parts of the system
- Middleware provides logging, retry, and other concerns

For more information on Brighter, see [Brighter Basic Configuration](/contents/BrighterBasicConfiguration.md).

## Darker: The Query Side

[Darker](https://github.com/BrighterCommand/Darker) is the query-side framework that handles operations that read state. It provides a complete solution for query processing with the same middleware pipeline pattern as Brighter.

### What Darker Provides

**Query Processing:**

- Query Processor pattern using the Query Object pattern
- Query handlers that execute read operations
- Support for synchronous and asynchronous queries

**Read Optimization:**

- Handlers optimized for reading and projecting data
- No business logic or validation (queries should be dumb)
- Support for denormalized views and read models

**Resilience:**

- Decorator pipeline for logging, retry, and circuit breakers
- Fallback policies for graceful degradation
- Integration with Polly for transient fault handling

**No External Bus:**

- Queries are always in-process (internal bus only)
- Queries don't produce side effects, so no message publishing
- Focused on local data access

### Query Handler Example

Here's a brief example of a Darker query handler. For complete details, see [Implementing a Query Handler](/contents/ImplementAQueryHandler.md):

```csharp
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

// Query: represents parameters for reading state
public sealed class GetOrderDetailsQuery : IQuery<OrderDetailsDto>
{
    public GetOrderDetailsQuery(int orderId)
    {
        OrderId = orderId;
    }

    public int OrderId { get; }
}

// DTO: optimized for presentation
public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime PlacedAt { get; set; }
}

// Query Handler: retrieves and projects data
public sealed class GetOrderDetailsQueryHandler :
    QueryHandlerAsync<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrderDetailsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [QueryLogging(step: 1)]
    [RetryableQuery(step: 2, circuitBreakerName: "DatabaseCircuitBreaker")]
    public override async Task<OrderDetailsDto> ExecuteAsync(
        GetOrderDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Query optimized read model (denormalized if needed)
        var orderDetails = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsNoTracking()  // Read-only optimization
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDetailsDto
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList(),
                Total = o.Items.Sum(i => i.Quantity * i.Price),
                Status = o.Status.ToString(),
                PlacedAt = o.PlacedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return orderDetails;
    }
}
```

**Key characteristics:**

- Queries are just parameter objects (GetOrderDetailsQuery)
- Handlers contain only data retrieval and projection logic
- No business rules or validation in query handlers
- DTOs optimized for client/presentation needs
- Middleware provides logging, retry, and fallback

For more information on Darker, see [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md).

## Integrating Brighter and Darker

When building a CQRS application, you'll use both Brighter and Darker together in a single application. They work side-by-side, each handling their respective responsibilities.

### Complete CQRS Architecture

Here's how Brighter and Darker fit together in a typical ASP.NET Core application:

```
┌─────────────────────────────────────────────────────────────┐
│                    Web Application (ASP.NET Core)           │
├──────────────────────────┬──────────────────────────────────┤
│                          │                                  │
│   Write Side (Commands)  │    Read Side (Queries)          │
│                          │                                  │
│   Controllers/Endpoints  │    Controllers/Endpoints        │
│          ↓               │             ↓                    │
│   IAmACommandProcessor   │    IQueryProcessor              │
│     (Brighter)           │      (Darker)                   │
│          ↓               │             ↓                    │
│   Command Handlers       │    Query Handlers               │
│   • Business Logic       │    • Data Retrieval             │
│   • Validation           │    • Projection                 │
│   • Event Publishing     │    • No Business Logic          │
│          ↓               │             ↓                    │
│   Write Model            │    Read Model                   │
│   (Normalized)           │    (Denormalized)               │
│   • Business Entities    │    • DTOs/ViewModels            │
│   • Domain Rules         │    • Optimized for Queries      │
│          ↓               │             ↓                    │
│   ┌─────────────┐        │    ┌─────────────┐             │
│   │  Write DB   │◄───────┼───►│  Read DB    │             │
│   │(or same DB) │Events  │    │(or same DB) │             │
│   └─────────────┘        │    └─────────────┘             │
│          ↓               │                                  │
│   External Bus           │                                  │
│   (Optional)             │                                  │
│   • RabbitMQ             │                                  │
│   • AWS SNS/SQS          │                                  │
│   • Kafka                │                                  │
└──────────────────────────┴──────────────────────────────────┘
```

### Dependency Injection Setup

Configure both Brighter and Darker in your `Program.cs` or `Startup.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework (shared or separate contexts)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Brighter (Command Side)
builder.Services.AddBrighter(options =>
{
    // Configure Brighter options
})
.AutoFromAssemblies(typeof(PlaceOrderCommandHandler).Assembly)
.ConfigureResiliencePipelines(registry =>
{
    // Configure retry and circuit breaker policies for commands
});

// Configure Darker (Query Side)
builder.Services.AddDarker(options =>
{
    // Match DbContext lifetime for EF Core
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(GetOrderDetailsQueryHandler).Assembly)
.AddJsonQueryLogging()
.AddDefaultPolicies();

// Add other services
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Using Both in Controllers

Inject both `IAmACommandProcessor` and `IQueryProcessor` where needed:

```csharp
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public OrdersController(
        IAmACommandProcessor commandProcessor,
        IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    // Write operation - uses Brighter
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(request.CustomerId, request.Items);
        await _commandProcessor.SendAsync(command, cancellationToken: cancellationToken);
        return Accepted();
    }

    // Read operation - uses Darker
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(
        int orderId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderDetailsQuery(orderId);
        var result = await _queryProcessor.ExecuteAsync(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
```

## Use Cases and Patterns

### Pattern: Simple CQRS (Same Database)

**Scenario:** Single database with different models for commands and queries

This is the simplest CQRS pattern, suitable for most applications. Both commands and queries use the same database, but with different models and optimizations.

```csharp
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

### Pattern: Separate Read/Write Databases

**Scenario:** Commands write to a primary database; queries read from a replica or separate optimized read database

```
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

### Pattern: Event-Sourced Writes, Projected Reads

**Scenario:** Commands produce events that are stored; queries read projections built from events

This advanced pattern stores all state changes as a sequence of events. The query side builds read models (projections) by replaying events.

```
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

### Pattern: Task-Based UI

**Scenario:** User interface actions map directly to commands; page loads map to queries

In a task-based UI, instead of generic CRUD operations, the UI presents specific business tasks as commands:

```csharp
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

## When to Use CQRS

### Good Use Cases

**1. Complex domains with different read/write requirements**

When your domain has complex business logic for writes but needs simple, fast reads:

- E-commerce order processing (complex validation) vs. order history (simple display)
- Banking transactions (strict consistency) vs. account balance (read-heavy)

**2. High read/write ratio differences**

When reads vastly outnumber writes, you can:

- Scale read side independently
- Use read replicas or caches
- Optimize read queries without affecting write performance

**3. Multiple read representations needed**

When different clients need different views of the same data:

- Mobile app needs minimal data
- Web app needs detailed data
- Reporting system needs aggregated data
- Each can have its own optimized query handler

**4. Event-driven architectures**

When you're building microservices or need to integrate with other systems:

- Commands produce events
- Events update read models
- External systems can subscribe to events

**5. Need for audit trails**

When you need complete history of all changes:

- Event sourcing captures all state changes
- Query side reads from event-sourced projections

### When to Avoid

**1. Simple CRUD applications**

If your application is mostly simple create/read/update/delete operations with minimal business logic, CQRS adds unnecessary complexity.

**2. Small applications**

For small applications or MVPs, the overhead of maintaining separate command and query models may not be worth the benefits.

**3. Limited team experience**

CQRS requires understanding of:

- Command/query separation
- Eventual consistency (for separated stores)
- Event-driven patterns (for advanced scenarios)

If your team is new to these concepts, consider starting with simpler patterns.

**4. No clear read/write separation**

If your operations frequently need to query state before updating it, or if reads and writes are tightly coupled, CQRS may not provide clear benefits.

## Benefits of Brighter + Darker

**Clear separation of concerns:**

- Commands and queries are explicitly separated
- Easy to identify what changes state vs. what reads state
- Reduces accidental coupling

**Optimized read and write paths:**

- Write side focuses on consistency and business rules
- Read side focuses on query performance and presentation
- Each can be scaled and optimized independently

**Consistent patterns:**

- Both use handler pattern with middleware pipelines
- Same resilience patterns (retry, circuit breaker, fallback)
- Similar programming model makes it easier to work with both

**Support for distributed systems:**

- Brighter provides external bus integration
- Commands and events can flow between microservices
- Supports event-driven architectures

**Resilience patterns:**

- Both integrate with Polly
- Circuit breakers protect external dependencies
- Fallback policies provide graceful degradation

**Testability:**

- Handlers are easy to unit test
- Clear separation makes mocking straightforward
- Pipeline decorators are testable independently

## Trade-offs and Considerations

### Complexity

CQRS adds complexity compared to simple CRUD:

- **More code**: Separate commands, queries, handlers, and DTOs
- **Two frameworks**: Need to understand both Brighter and Darker
- **Separate models**: Write and read models must be maintained

**Mitigation strategies:**
- Start simple (same database, simple CQRS)

- Use code generation or templates for boilerplate
- Document patterns clearly
- Provide team training

### Eventual Consistency

When using separate data stores, read models may lag behind writes:

- **User experience**: Users may not immediately see their changes
- **Race conditions**: Subsequent operations may see stale data
- **Complexity**: Need to handle "not found yet" scenarios

**Mitigation strategies:**

- Design UI to acknowledge eventual consistency
- Use version numbers or timestamps
- Provide "refresh" mechanisms
- Consider same-database CQRS if consistency is critical

### Learning Curve

Teams need to learn:

- CQRS concepts and patterns
- Brighter and Darker frameworks
- When to use commands vs. queries
- How to handle eventual consistency

**Mitigation strategies:**

- Start with training and documentation
- Begin with simple scenarios
- Build reference implementations
- Pair experienced and new developers

## Example: E-Commerce Order System

Let's walk through a complete example of placing and viewing an order using Brighter and Darker.

### Write Side (Brighter)

**Command:**
```csharp
using Paramore.Brighter;
using System.Collections.Generic;

public class PlaceOrderCommand : IRequest
{
    public PlaceOrderCommand(int customerId, List<OrderItemDto> items)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Items = items;
    }

    public Guid Id { get; }
    public int CustomerId { get; }
    public List<OrderItemDto> Items { get; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

**Command Handler:**
```csharp
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class PlaceOrderCommandHandler : RequestHandlerAsync<PlaceOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAmACommandProcessor _commandProcessor;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IAmACommandProcessor commandProcessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _commandProcessor = commandProcessor;
    }

    [RequestLogging(step: 1)]
    [UsePolicy("OrderRetryPolicy", step: 2)]
    public override async Task<PlaceOrderCommand> HandleAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (command.Items == null || !command.Items.Any())
            throw new InvalidOperationException("Order must contain items");

        // Check product availability
        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                item.ProductId,
                cancellationToken);

            if (product == null)
                throw new InvalidOperationException(
                    $"Product {item.ProductId} not found");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product {item.ProductId}");
        }

        // Create order (write model)
        var order = new Order
        {
            Id = command.Id,
            CustomerId = command.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = command.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        // Publish event for eventual consistency
        var orderPlacedEvent = new OrderPlacedEvent(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.Items.Sum(i => i.Quantity * i.UnitPrice));

        await _commandProcessor.PublishAsync(
            orderPlacedEvent,
            cancellationToken: cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

### Read Side (Darker)

**Query:**
```csharp
using Paramore.Darker;

public sealed class GetOrderDetailsQuery : IQuery<OrderDetailsDto>
{
    public GetOrderDetailsQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}

public class OrderDetailsDto
{
    public Guid OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public List<OrderItemDetailsDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderItemDetailsDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
```

**Query Handler:**
```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrderDetailsQueryHandler :
    QueryHandlerAsync<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrderDetailsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [QueryLogging(step: 1)]
    [RetryableQuery(step: 2, circuitBreakerName: "DatabaseCircuitBreaker")]
    public override async Task<OrderDetailsDto> ExecuteAsync(
        GetOrderDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Optimized query with joins and projections
        var orderDetails = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()  // Read-only optimization
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDetailsDto
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemDetailsDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice
                }).ToList(),
                TotalAmount = o.Items.Sum(i => i.Quantity * i.UnitPrice)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return orderDetails;
    }
}
```

### Complete Flow

1. **User submits order** → Controller receives request, creates `PlaceOrderCommand`
2. **Command dispatched** → Brighter's `IAmACommandProcessor.SendAsync()` routes to handler
3. **Handler validates** → Checks business rules, product availability, stock levels
4. **Order created** → Write model (normalized Order entity) saved to database
5. **Event published** → `OrderPlacedEvent` published via Brighter (for read model updates)
6. **User views order** → Controller receives request, creates `GetOrderDetailsQuery`
7. **Query dispatched** → Darker's `IQueryProcessor.ExecuteAsync()` routes to handler
8. **Handler retrieves** → Query reads denormalized view with joins, optimized for display
9. **DTO returned** → Read model (OrderDetailsDto) returned to client

**Note:** In this simple example, both read and write use the same database. For eventual consistency scenarios, the `OrderPlacedEvent` would be handled by a separate event handler that updates a denormalized read model.

## Best Practices

**1. Use Brighter for all state changes**

Never update state in query handlers. All writes should go through Brighter command handlers.

**2. Use Darker for all state queries**

Query handlers should only read data. They should not validate, apply business rules, or change state.

**3. Keep read and write models separate**

Don't share domain entities between commands and queries. Use separate DTOs for query results.

**4. Don't query in command handlers**

Command handlers should focus on business logic and state changes. Minimize reads in command handlers (only read what's necessary for validation).

**5. Don't update in query handlers**

Query handlers should never modify state, not even "last accessed" timestamps or view counts.

**6. Use events to sync read models (if separate)**

When using separate read models, use Brighter events to keep them synchronized with the write model.

**7. Handle eventual consistency in UI**

Design your UI to gracefully handle scenarios where the read model hasn't caught up to the write model yet.

**8. Use consistent naming conventions**

- Commands: `VerbNounCommand` (PlaceOrderCommand, CancelOrderCommand)
- Events: `NounVerbEvent` (OrderPlacedEvent, OrderCancelledEvent)
- Queries: `GetNounQuery` or `SearchNounQuery` (GetOrderDetailsQuery, SearchOrdersQuery)

## Common Pitfalls

**1. Querying in command handlers**

❌ Bad:
```csharp
public override async Task<PlaceOrderCommand> HandleAsync(PlaceOrderCommand command, ...)
{
    // Don't query to display data to user in a command handler
    var customer = await _customerRepository.GetByIdAsync(command.CustomerId);
    _logger.LogInformation($"Order placed by {customer.Name}");  // Avoid!
}
```

✅ Good:
```csharp
public override async Task<PlaceOrderCommand> HandleAsync(PlaceOrderCommand command, ...)
{
    // Only query what's necessary for business rules
    var customer = await _customerRepository.GetByIdAsync(command.CustomerId);
    if (!customer.IsActive)
        throw new InvalidOperationException("Customer is inactive");
}
```

**2. Updating in query handlers**

❌ Bad:
```csharp
public override async Task<OrderDetailsDto> ExecuteAsync(GetOrderDetailsQuery query, ...)
{
    var order = await _repository.GetByIdAsync(query.OrderId);
    order.LastViewed = DateTime.UtcNow;  // Don't modify state in queries!
    await _repository.UpdateAsync(order);
    return MapToDto(order);
}
```

✅ Good: Create a separate command for tracking views if needed.

**3. Mixing command and query logic**

Keep command and query concerns completely separate. Don't try to combine them in a single endpoint or handler.

**4. Over-engineering simple scenarios**

Don't use CQRS for simple CRUD operations unless there's a clear benefit.

**5. Ignoring eventual consistency**

When using separate read/write stores, design your system to handle staleness gracefully.

**6. Not training team on CQRS**

CQRS requires a mindset shift. Invest in team training and documentation.

**7. Sharing domain entities between commands and queries**

Use separate DTOs for query results. Don't expose write-side domain entities directly to the read side.

**8. Forgetting about idempotency**

When using events to sync read models, ensure event handlers are idempotent (can be safely replayed).

## Further Reading

- [Basic Concepts](/contents/BasicConcepts.md) - Fundamental concepts including CQS and pipelines
- [Brighter Basic Configuration](/contents/BrighterBasicConfiguration.md) - Getting started with Brighter
- [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md) - Getting started with Darker
- [Event Driven Collaboration](/contents/EventDrivenCollaboration.md) - Using events in distributed systems
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Detailed guide to Darker query handlers
- [Dispatching Requests](/contents/DispatchingARequest.md) - Detailed guide to Brighter command handlers
- [Outbox Pattern](/contents/OutboxPattern.md) - Reliable event publishing with Brighter
