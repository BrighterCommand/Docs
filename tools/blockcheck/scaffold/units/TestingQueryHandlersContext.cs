// Types TestingQueryHandlers.md names in its blocks and never shows.
//
// The page tests handlers written on another page, against a domain, a DbContext and a test
// fixture it never shows. Each stub carries only the members a block names; the handlers' bodies
// are not the page's subject and throw. `Order` is read as a type the page never shows (spec 017
// task 1.10), never as `StackExchange.Redis.Order`.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

// block 1: `new Order { Id = 123, CustomerName = "John Doe" }`; block 3: `result.Items`, `result.Total`
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<object> Items { get; set; } = [];
    public decimal Total { get; set; }
}

// block 1: `r.GetByIdAsync(123, It.IsAny<CancellationToken>())`
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

// blocks 1, 3: `new GetOrderQuery(123)`
public class GetOrderQuery : IQuery<Order>
{
    public GetOrderQuery(int orderId) { }
}

// block 1: `new GetOrderQueryHandler(mockRepository.Object)`, `handler.ExecuteAsync(query, …)`
public class GetOrderQueryHandler : QueryHandlerAsync<GetOrderQuery, Order>
{
    public GetOrderQueryHandler(IOrderRepository repository) { }

    public override Task<Order> ExecuteAsync(GetOrderQuery query, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

// block 1: `Assert.ThrowsAsync<OrderNotFoundException>`
public class OrderNotFoundException : Exception { }

// block 2: `new Customer { Id = 1, Name = "Test Customer" }`, `result.Name`
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// block 2: `new ApplicationDbContext(options)`, `_dbContext.Customers`
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
}

// block 2: `new GetCustomerQuery(1)`
public class GetCustomerQuery : IQuery<Customer>
{
    public GetCustomerQuery(int customerId) { }
}

// block 2: `new GetCustomerQueryHandler(_dbContext)`, `_handler.ExecuteAsync(query, …)`
public class GetCustomerQueryHandler : QueryHandlerAsync<GetCustomerQuery, Customer>
{
    public GetCustomerQueryHandler(ApplicationDbContext dbContext) { }

    public override Task<Customer> ExecuteAsync(GetCustomerQuery query, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

// block 3: `fixture.Database`, `fixture.ServiceProvider`
public class DatabaseFixture
{
    public TestDatabase Database { get; } = new();
    public IServiceProvider ServiceProvider { get; } = null!;
}

// block 3: `_database.SeedOrders()`
public class TestDatabase
{
    public void SeedOrders() { }
}
