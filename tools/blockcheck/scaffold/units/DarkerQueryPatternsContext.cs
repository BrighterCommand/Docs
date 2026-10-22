// Types DarkerQueryPatternsContext's five pages name in their blocks and never declare:
// AggregationQueryPatterns.md, ProjectionQueryPatterns.md, ParameterizedQueryPatterns.md,
// QueryHandlerDependencies.md and QueryPatterns.md.
//
// Their handlers query one EF Core model the pages never show — customers, orders, their items and
// products, categories — through an `ApplicationDbContext`, or through repositories and services
// over it. Each stub carries only the members a block names. `Order` is read as a type the pages
// never show (spec 017 task 1.10), never as `StackExchange.Redis.Order`. QueryPatterns.md's blocks 1–4 read
// the context a handler is given, as `_dbContext`, outside any handler.
//
// blockcheck: using static DarkerQueryPatternsContext;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

public static class DarkerQueryPatternsContext
{
    // QueryPatterns 1–4: `_dbContext.Orders`, `_dbContext.Customers`, `_dbContext.Products`
    public static ApplicationDbContext _dbContext => null!;
}

// Aggregation 1–3, Projection 1, 2, QueryHandlerDependencies 2: `_dbContext.Categories`, `.Orders`, `.Customers`
public class ApplicationDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    // QueryPatterns 4: `_dbContext.Products`
    public DbSet<Product> Products { get; set; } = null!;
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

// Projection 2: `.ThenInclude(i => i.Product)`, `i.Product.Name`; QueryPatterns 4: `p.Id`, `p.Description`,
// `p.CategoryId`, `p.Price`, `p.StockQuantity`, `p.Category.Name`, `p.ImageUrl`, `p.DiscountPercent`
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = "";
    public decimal? DiscountPercent { get; set; }
}

// QueryPatterns 4: `new PagedResult<ProductCatalogItemDto>(products, totalCount, query.PageNumber, query.PageSize)`
public class PagedResult<T>(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize);

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

// QueryPatterns 1, 2: `new OrderDto { Id, OrderDate, CustomerName }`, `new OrderDto { OrderId, CustomerName }`
public class OrderDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = "";
}

// QueryPatterns 3, 4: `ExecuteAsync(GetProductsQuery query, …)`
public class GetProductsQuery : IQuery<List<Product>>;
