// Types and values PolicyFallback.md names in its blocks and never declares.
//
// The handlers take compensating action against services, queues and stores the page names in
// their constructors and never shows. Blocks 6 and 7 are a `Fallback` method shown alone, using
// the logger and command processor a handler is given. Each stub carries only the members the
// page's blocks use; values return a default and do nothing.
//
// blockcheck: using static PolicyFallbackContext;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Polly.Registry;

public static class PolicyFallbackContext
{
    // blocks 6, 7: a `Fallback` shown alone, logging and sending through its handler's dependencies
    public static ILogger _logger => null!;
    public static IAmACommandProcessor _commandProcessor => null!;
    // block 10: `resiliencePipelineRegistry.TryAddBuilder<Product>(…)`, `services.AddSingleton(…)`
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
    public static IServiceCollection services => null!;
}

// blocks 1, 3–7, 9: `RequestHandler<MyCommand>`; block 6: `command.OrderId`
public class MyCommand() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
}

// block 1: `externalService.Call(command)`
public interface IExternalService
{
    void Call(MyCommand command);
}

// block 1: `retryQueue.QueueForRetry(command)`
public interface IRetryQueue
{
    void QueueForRetry(MyCommand command);
}

// block 1: `new UndoCommand { OriginalCommandId = command.Id }`
public class UndoCommand() : Command(Id.Random())
{
    public Id? OriginalCommandId { get; set; }
}

// block 6: `new CancelOrderCommand { OrderId = …, Reason = … }`
public class CancelOrderCommand() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// blocks 8, 10: `Product.Default`
public class Product
{
    public static Product Default => null!;
}

// block 8: `query.ProductId`, `query.Product`
public class GetProductQuery() : Command(Id.Random())
{
    public string ProductId { get; set; } = string.Empty;
    public Product? Product { get; set; }
}

// blocks 8, 10: `productService.GetProduct(…)`
public interface IProductService
{
    Product GetProduct(string productId);
}

// block 8: `cache.GetProduct(query.ProductId)`
public interface IProductCache
{
    Product? GetProduct(string productId);
}

// block 9: `alertService.SendAlert(new Alert { Severity = …, Message = …, Context = command })`
public interface IAlertService
{
    void SendAlert(Alert alert);
}

public class Alert
{
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public IRequest? Context { get; set; }
}

public enum AlertSeverity { Critical }

// block 9: `failedCommandRepository.Add(new FailedCommand { CommandId = …, Exception = …, Timestamp = … })`
public interface IFailedCommandRepository
{
    void Add(FailedCommand failedCommand);
}

public class FailedCommand
{
    public Id? CommandId { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; }
}

// block 10: `command.ProductId`, `command.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public string ProductId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}

// block 10: `orderService.CreateOrder(command, product)`, `orderService.CancelOrder(command.OrderId)`
public interface IOrderService
{
    void CreateOrder(ProcessOrderCommand command, Product product);
    void CancelOrder(string orderId);
}
