// Types and values QueryPipeline.md names in its blocks and never declares.
//
// The page's handlers read people, customers and orders through repositories it never shows.
// Blocks 18 and 19 continue the `builder` of the blocks before them. Each stub carries only the
// members the page's blocks use. No block names the type a customer or an order is read as, only its
// members, so each is a tuple rather than a stub type.
//
// blockcheck: using static QueryPipelineContext;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Paramore.Darker;

public static class QueryPipelineContext
{
    // blocks 18, 19, 21: `builder.Services.AddDarker()`
    public static WebApplicationBuilder builder => null!;
    // blocks 8, 12, 13, 16, 17, 20–23: an `ExecuteAsync` shown alone, querying its handler's external API
    public static IExternalApi _externalApi => null!;
    // block 24: a `FallbackAsync` shown alone, calling "another external service"
    public static IExternalApi _alternativeService => null!;
}

// blocks 8, 12–14, 16, 17, 20–25: `QueryHandlerAsync<GetOrderQuery, OrderData>`, `query.OrderId`
public class GetOrderQuery : IQuery<OrderData>
{
    public int OrderId { get; set; }
}

// blocks 13, 23, 25: `Task.FromResult(OrderData.Unavailable)`
public class OrderData
{
    public static OrderData Unavailable => new();
}

// block 14: `externalApi.GetOrderAsync(query.OrderId, cancellationToken)`
public interface IExternalApi
{
    Task<OrderData> GetOrderAsync(int orderId, CancellationToken cancellationToken);
}

// block 14: `orders.GetAsync(query.OrderId, cancellationToken)`
public interface IOrderStore
{
    Task<OrderData> GetAsync(int orderId, CancellationToken cancellationToken);
}

// blocks 3, 5, 9: `QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>`
public class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>;

// blocks 1, 7, 10: `QueryHandlerAsync<GetPersonNameQuery, string>`
public class GetPersonNameQuery : IQuery<string>
{
    public int PersonId { get; set; }
}

// blocks 3, 5, 9: `_repository.GetAllAsync(cancellationToken)`; blocks 7, 10: `GetNameByIdAsync(query.PersonId, …)`
public interface IPersonRepository
{
    Task<IReadOnlyDictionary<int, string>> GetAllAsync(CancellationToken cancellationToken);
    Task<string> GetNameByIdAsync(int personId, CancellationToken cancellationToken);
}

// block 11: `QueryHandlerAsync<GetCustomerOrderSummaryQuery, CustomerOrderSummary>`, `query.CustomerId`
public class GetCustomerOrderSummaryQuery : IQuery<CustomerOrderSummary>
{
    public int CustomerId { get; set; }
}

// block 11: `new CustomerOrderSummary { CustomerId, CustomerName, OrderCount, TotalValue }`
public class CustomerOrderSummary
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public int OrderCount { get; set; }
    public decimal TotalValue { get; set; }
}

// block 11: `_customerRepository.GetByIdAsync(query.CustomerId, …)`, `customer.Id`, `customer.Name`
public interface ICustomerRepository
{
    Task<(int Id, string Name)> GetByIdAsync(int customerId, CancellationToken cancellationToken);
}

// block 11: `_orderRepository.GetByCustomerIdAsync(query.CustomerId, …)`, `orders.Count`, `o.Total`
public interface IOrderRepository
{
    Task<List<(int Id, decimal Total)>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
}
