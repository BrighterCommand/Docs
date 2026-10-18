// Types QueriesAndQueryObjects.md names in its blocks and never declares.
//
// The page's queries return orders, products, customers and pages of them, from a domain model it
// never shows. Each stub carries only the members the page's blocks use.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// blocks 5, 7, 10, 11, 18, 19: `IQuery<Order>`, `IQuery<PagedResult<Order>>`
public class Order;

// blocks 4, 20: `IQuery<IReadOnlyList<OrderSummary>>`, `IQuery<PagedResult<OrderSummary>>`
public class OrderSummary;

// block 4: `OrderStatus? status`
public enum OrderStatus;

// blocks 5, 11, 20: `IQuery<PagedResult<Order>>`
public class PagedResult<T>;

// blocks 6, 9, 12, 18, 21: `IQuery<Product>`, `IQuery<IReadOnlyList<Product>>`; block 12: `p.Price`
public class Product
{
    public decimal Price { get; set; }
}

// blocks 10, 18: `IQuery<Customer>`, `IQuery<Customer?>`
public class Customer;

// block 19: `_repository.GetByIdAsync(query.OrderId, cancellationToken)`
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int orderId, CancellationToken cancellationToken);
}
