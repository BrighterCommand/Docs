// Types and values UsingTheContextBag.md names in its blocks and never declares.
//
// The handlers read and write the context bag for requests and services the page assumes rather
// than defines. Blocks 4, 13, 14 and 16–18 are lines shown alone, using a `context`, a handler's
// `Context` and values the surrounding code would have. Each stub carries only the members the
// page's blocks use; values return a default and do nothing.
//
// blockcheck: using static UsingTheContextBagContext;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public static class UsingTheContextBagContext
{
    // block 4: `context.Bag[…]`, a context the caller has created
    public static RequestContext context => null!;
    // blocks 13, 14, 16, 17: `Context.Bag[…]`, `Context.Span`, a handler's context
    public static IRequestContext Context => null!;
    // blocks 13, 14, 16, 18: the values the lines put in the bag
    public static string tenantId => null!;
    public static string userId => null!;
    // block 17: `Context.Span.SetTag("custom.id", id)`
    public static string id => null!;
    public static Dictionary<string, object> headers => null!;
    public static Dictionary<string, object> criticalHeaders => null!;
    // block 18: `commandProcessor.PostAsync(orderCreated, …)`
    public static IAmACommandProcessor commandProcessor => null!;
    public static OrderCreated orderCreated => null!;
}

// blocks 1, 3, 7–11, 15: `RequestHandler<MyCommand>`; `command.TenantId`, `command.Priority`;
// block 8: `command.BusinessId`, `command.OrderValue`
public class MyCommand() : Command(Id.Random())
{
    public string TenantId { get; set; } = string.Empty;
    public Priority Priority { get; set; }
    public string BusinessId { get; set; } = string.Empty;
    public decimal OrderValue { get; set; }
}

// block 9: `Priority.High`
public enum Priority { High }

// block 2: `request.TenantId`, `request.OrderId`
public class CreateOrderRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}

// blocks 2, 18: `new OrderCreated { OrderId = … }`
public class OrderCreated() : Event(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
}

// block 3: `new TenantWorkDone(command.TenantId)`
public class TenantWorkDone(string tenantId) : Event(Id.Random());

// block 5: `command.Priority`, `command.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public string Priority { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}

// block 5: `orderService.Process(command)`
public interface IOrderService
{
    void Process(ProcessOrderCommand command);
}

// block 5: `new OrderProcessed(command.OrderId)`
public class OrderProcessed(string orderId) : Event(Id.Random());

// block 6: `command.BusinessContext`
public class PublishEventCommand() : Command(Id.Random())
{
    public string BusinessContext { get; set; } = string.Empty;
}

// block 6: `new BusinessEventRaised(command.BusinessContext)`
public class BusinessEventRaised(string businessContext) : Event(Id.Random());

// block 7: `service.ProcessAsync(command, cancellationToken)`; block 8: `service.Process(command)`
public interface IMyService
{
    Task ProcessAsync(MyCommand command, CancellationToken cancellationToken);
    void Process(MyCommand command);
}

// block 15: `new DisposableResource()`, disposed through `IDisposable`
public class DisposableResource : System.IDisposable
{
    public void Dispose() { }
}
