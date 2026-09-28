// Types DarkerQueryPatternsContext's four pages name in their blocks and never declare:
// AggregationQueryPatterns.md, ProjectionQueryPatterns.md, ParameterizedQueryPatterns.md and
// QueryHandlerDependencies.md.
//
// Their handlers query one EF Core model the pages never show — customers, orders, their items and
// products, categories — through an `ApplicationDbContext`, or through repositories and services
// over it. Each stub carries only the members a block names. `Order` is read as a type the pages
// never show (spec 017 task 1.10), never as `StackExchange.Redis.Order`.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

// Aggregation 1–3, Projection 1, 2, QueryHandlerDependencies 2: `_dbContext.Categories`, `.Orders`, `.Customers`
public class ApplicationDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
}

// Aggregation 1: `ToDictionaryAsync(c => c.Id, c => c.Name, …)` into `IReadOnlyDictionary<int, string>`
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// Aggregation 2, 3, Projection 2, QueryHandlerDependencies 4: `o.Status`, `o.CustomerId`, `o.OrderDate`,
// `o.Items`, `o.Id`, `o.Customer`, `o.ShippingAddress`; `order.CustomerId`. No block names the type of
// an item or an address, only their members (`i.Quantity`, `i.UnitPrice`, `i.Product`,
// `o.ShippingAddress.Street`, `.City`), so each is a tuple rather than a stub type
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public Customer Customer { get; set; } = null!;
    public (string Street, string City) ShippingAddress { get; set; }
    public List<(int Quantity, decimal UnitPrice, Product Product)> Items { get; set; } = new();
}

// Aggregation 2: `OrderStatus.Pending`
public enum OrderStatus { Pending }

// Projection 2: `.ThenInclude(i => i.Product)`, `i.Product.Name`
public class Product
{
    public string Name { get; set; } = "";
}

// Projection 1, 2, QueryHandlerDependencies 2: `c.Id`, `c.Name`, `c.Email`, `c.Orders.Count`
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public List<Order> Orders { get; set; } = new();
}

// Parameterized 1: `IQuery<CustomerDto?>`; QueryHandlerDependencies 2: `new CustomerDto { Id, Name, OrderCount }`
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int OrderCount { get; set; }
}

// Parameterized 1: `IQuery<OrderLineDto?>`
public class OrderLineDto { }

// QueryHandlerDependencies 1: `QueryHandlerAsync<GetOrderQuery, Order>`, `query.OrderId`
public class GetOrderQuery : IQuery<Order>
{
    public int OrderId { get; set; }
}

// QueryHandlerDependencies 2: `QueryHandlerAsync<GetCustomerWithOrdersQuery, CustomerDto>`, `query.CustomerId`
public class GetCustomerWithOrdersQuery : IQuery<CustomerDto>
{
    public int CustomerId { get; set; }
}

// QueryHandlerDependencies 4: `QueryHandlerAsync<GetOrderSummaryQuery, OrderSummary>`, `query.OrderId`
public class GetOrderSummaryQuery : IQuery<OrderSummary>
{
    public int OrderId { get; set; }
}

// QueryHandlerDependencies 4: `_mapper.Map<OrderSummary>(…)`
public class OrderSummary { }

// QueryHandlerDependencies 1, 4: `_repository.GetByIdAsync(query.OrderId, cancellationToken)`
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int orderId, CancellationToken cancellationToken);
}

// QueryHandlerDependencies 4: `_customerRepository.GetByIdAsync(order.CustomerId, cancellationToken)`
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(int customerId, CancellationToken cancellationToken);
}

// QueryHandlerDependencies 4: `_pricingService.CalculateTotalAsync(order, cancellationToken)`
public interface IPricingService
{
    Task<decimal> CalculateTotalAsync(Order order, CancellationToken cancellationToken);
}

// QueryHandlerDependencies 4: `_mapper.Map<OrderSummary>((order, customer, pricing))` — an object
// mapper the page never names, shaped as AutoMapper's `IMapper.Map<TDestination>(object)`
public interface IMapper
{
    TDestination Map<TDestination>(object source);
}
