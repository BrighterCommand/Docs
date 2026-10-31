// Types and values TurningOnReplayOnSeen.md names in its blocks and never declares.
//
// The page follows an order through a payment service and a shipping service, and the commands,
// events and collaborators they use are the reader's own. Every value member is typed from a
// pinned package or the BCL and returns a default; a stub is a request, or an interface, with only
// the members a block names.
//
// blockcheck: using static TurningOnReplayOnSeenContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class TurningOnReplayOnSeenContext
{
    // block 3: `new RelationalDatabaseConfiguration(DbConnectionString())`
    public static string DbConnectionString() => "Server=localhost;Database=Payments;";
    // block 5: `services.AddBrighter()`
    public static IServiceCollection services => null!;
}

// blocks 1, 2, 6: `RequestHandler<ProcessPayment>`, `command.OrderId`, `command.Amount`
public class ProcessPayment() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// blocks 1, 2, 6: `new ShipOrder { OrderId = command.OrderId }`
public class ShipOrder() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
}

// block 6: `new OrderShipped { OrderId = …, Consignment = consignment }`
public class OrderShipped() : Event(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string Consignment { get; set; } = string.Empty;
}

// block 6: `_payments.Charge(command.OrderId, command.Amount)`
public interface IPaymentGateway
{
    void Charge(string orderId, decimal amount);
}

// block 6: `_courier.Book(command.OrderId)`
public interface ICourier
{
    string Book(string orderId);
}
