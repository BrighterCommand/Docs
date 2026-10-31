// Types and values HandlerFailure.md names in its blocks and never declares.
//
// Every handler on the page handles the reader's own `PlaceOrder` command and calls the reader's
// own services, which the page never shows: the handlers' `// ...` stands for the rest of the
// class, where `ProcessOrder` lives. The services are interfaces of exactly the members a block
// calls; `ProcessOrder` and `ProcessOrderAsync` are statics, reached through the `using static` line
// below, that do nothing. Where a block only passes a value along (`customer`), the member is typed
// `object`.
//
// blockcheck: using static HandlerFailureContext;

using System;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public static class HandlerFailureContext
{
    // blocks 1, 2, 5, 6, 8, 10: `ProcessOrder(command);`
    public static void ProcessOrder(PlaceOrder command) { }

    // block 11: `await ProcessOrderAsync(command, cancellationToken);`
    public static Task ProcessOrderAsync(PlaceOrder command, CancellationToken cancellationToken) => Task.CompletedTask;
}

// blocks 1–8, 10, 11: `RequestHandler<PlaceOrder>`; blocks 3, 4, 7: `command.OrderId`, `command.Amount`,
// `command.CustomerId`
public class PlaceOrder() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}

// blocks 3, 7: `_orderService.Place(command.OrderId, command.Amount)`
public interface IOrderService
{
    void Place(string orderId, decimal amount);
}

// block 3: `catch (ServiceUnavailableException)`
public class ServiceUnavailableException : Exception;

// block 4: `_repository.GetCustomer(command.CustomerId)`, `_repository.PlaceOrder(command.OrderId, customer)`
public interface IOrderRepository
{
    object? GetCustomer(string customerId);
    void PlaceOrder(string orderId, object customer);
}

// block 7: `_features.IsEnabled("order-processing")`
public interface IFeatureFlags
{
    bool IsEnabled(string feature);
}
