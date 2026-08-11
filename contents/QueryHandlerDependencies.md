# Query Handler Dependencies

> **How-to** · Applies to **Darker V4** · Prerequisites: [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)

Query handlers typically need dependencies like repositories, database contexts, or services to execute queries. Darker supports dependency injection for handler dependencies.

## Query Handler Constructor Injection

Inject dependencies through the handler's constructor:

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(
        IOrderRepository repository,
        ILogger<GetOrderQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<Order> ExecuteAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving order {OrderId}", query.OrderId);

        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        return order;
    }
}
```

Dependencies are resolved automatically by the DI container when the handler is instantiated.

## Scoped Query Handler Dependencies (EF Core DbContext)

When using Entity Framework Core, inject the DbContext as a scoped dependency. Remember to configure Darker with scoped lifetime:

```csharp
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCustomerWithOrdersQueryHandler : QueryHandlerAsync<GetCustomerWithOrdersQuery, CustomerDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetCustomerWithOrdersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<CustomerDto> ExecuteAsync(
        GetCustomerWithOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .AsNoTracking()  // Read-only optimization
            .Where(c => c.Id == query.CustomerId)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                OrderCount = c.Orders.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}
```

**Important:** Ensure you've configured Darker with scoped lifetime in your `Program.cs`:

```csharp
// ...
builder.Services.AddDarker(options =>
{
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(Program).Assembly);
```

## Multiple Query Handler Dependencies

Handlers can have multiple dependencies injected:

```csharp
// ...
public sealed class GetOrderSummaryQueryHandler : QueryHandlerAsync<GetOrderSummaryQuery, OrderSummary>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;

    public GetOrderSummaryQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IPricingService pricingService,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _pricingService = pricingService;
        _mapper = mapper;
    }

    public override async Task<OrderSummary> ExecuteAsync(
        GetOrderSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, cancellationToken);
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        var pricing = await _pricingService.CalculateTotalAsync(order, cancellationToken);

        return _mapper.Map<OrderSummary>((order, customer, pricing));
    }
}
```

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - The handler patterns these dependencies serve
- [Testing Query Handlers](/contents/TestingQueryHandlers.md) - Substituting these dependencies in a test
- [Entity Framework Core Query Integration](/contents/EFCoreQueryIntegration.md) - Scoping a `DbContext` in a query handler
