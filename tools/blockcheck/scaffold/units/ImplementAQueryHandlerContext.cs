// Types and values ImplementAQueryHandler.md names in its blocks and never declares.
//
// The page's handlers read orders, customers, caches and statistics through repositories and
// services it never shows. Block 15 continues a `builder` and registers handlers from assemblies of
// the reader's own. Each stub carries only the members the page's blocks use.
//
// blockcheck: using static ImplementAQueryHandlerContext;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Paramore.Darker;

public static class ImplementAQueryHandlerContext
{
    // block 15: `builder.Services.AddDarker()`
    public static WebApplicationBuilder builder => null!;
}

// block 7: `QueryHandler<GetCachedCountQuery, int>`, `query.CacheKey`
public class GetCachedCountQuery : IQuery<int>
{
    public string CacheKey { get; set; } = "";
}

// block 7: `_cache.GetCount(query.CacheKey)`
public interface ICache
{
    int GetCount(string key);
}

// block 9: `new Statistics { TotalCount = 100, AverageValue = 50.5m }`
public class Statistics
{
    public int TotalCount { get; set; }
    public decimal AverageValue { get; set; }
}

// block 9: `_cache.Get<Statistics>(query.Key)`, `_cache.Set(query.Key, stats)`
public interface IInMemoryCache
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
}

// blocks 10, 11, 13, 18: `query.OrderId`
public class GetOrderQuery : IQuery<Order>
{
    public int OrderId { get; set; }
}

// block 18: `order.Id`, `order.Customer.Name`, `order.Total`, `order.Items`. No block names the type of an
// item, only its members (`i.Product.Name`, `i.Quantity`, `i.Price`), so it is a tuple rather than a stub type
public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal Total { get; set; }
    public List<(Product Product, int Quantity, decimal Price)> Items { get; set; } = new();
}

public class Product
{
    public string Name { get; set; } = "";
}

// blocks 17, 18, 20: `customer.Name`, `order.Customer.Name`; `Customer?`
public class Customer
{
    public string Name { get; set; } = "";
}

// blocks 10, 11, 13, 18, 19: `GetById`, `GetByIdAsync`, `GetActiveOrdersAsync`
public interface IOrderRepository
{
    Order GetById(int orderId);
    Task<Order> GetByIdAsync(int orderId, CancellationToken cancellationToken);
    Task<IEnumerable<OrderSummary>> GetActiveOrdersAsync(CancellationToken cancellationToken);
}

// blocks 17, 20: `GetByIdAsync(query.CustomerId, …)`, `FindByEmailAsync(query.Email, …)`
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(int customerId, CancellationToken cancellationToken);
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

// block 17: `QueryHandlerAsync<GetCustomerNameQuery, string>`, `query.CustomerId`
public class GetCustomerNameQuery : IQuery<string>
{
    public int CustomerId { get; set; }
}

// block 18: `query.OrderId`; `new OrderDetailsDto { OrderId, CustomerName, TotalAmount, Items }`
public class GetOrderDetailsQuery : IQuery<OrderDetailsDto>
{
    public int OrderId { get; set; }
}

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

// block 18: `new OrderItemDto { ProductName, Quantity, Price }`
public class OrderItemDto
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

// block 19: `QueryHandlerAsync<GetActiveOrdersQuery, IReadOnlyList<OrderSummary>>`
public class GetActiveOrdersQuery : IQuery<IReadOnlyList<OrderSummary>>;

public class OrderSummary;

// block 20: `query.Email`
public class FindCustomerByEmailQuery : IQuery<Customer?>
{
    public string Email { get; set; } = "";
}

// block 15: `typeof(CustomerQuery).Assembly`, each in a module of the reader's
public class CustomerQuery;

public class OrderQuery;

public class ProductQuery;
