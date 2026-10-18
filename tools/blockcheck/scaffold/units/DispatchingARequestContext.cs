// Types and values DispatchingARequest.md names in its blocks and never declares.
//
// The page dispatches requests from a greetings and an orders domain it assumes rather than
// defines. Block 8 is a handler's `HandleAsync` shown alone; the sentence before it names the
// three dependencies its constructor takes. Values return a default and do nothing; a block that
// calls a member of one is checked against the real type.
//
// blockcheck: using static DispatchingARequestContext;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;

public static class DispatchingARequestContext
{
    // blocks 5–7: `commandProcessor.Send(…)`, as block 1 resolves it
    public static IAmACommandProcessor commandProcessor => null!;
    // block 8: the handler's constructor arguments, as the sentence before it says
    public static IAmATransactionConnectionProvider _transactionProvider => null!;
    public static IAmACommandProcessor _postBox => null!;
    public static ILogger _logger => null!;
}

// blocks 1, 5, 7: `new GreetingCommand("Ian")`
public class GreetingCommand(string name) : Command(Id.Random());

// block 6: `new GreetingEvent("Ian has been greeted")`
public class GreetingEvent(string greeting) : Event(Id.Random());

// block 2: `request.TenantId`, `request.OrderId`
public class CreateOrderRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

// blocks 2, 4: `new CreateOrderCommand { OrderId = …, TenantId = … }`
public class CreateOrderCommand() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

// blocks 3, 4: `new OrderCreatedEvent { OrderId = …, CustomerId = … }`
public class OrderCreatedEvent() : Event(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
}

// block 3: `order.TotalValue`, `order.CustomerCategory`, `order.ShippingAddress.Region`, `order.Id`, `order.CustomerId`
public class Order
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public string CustomerCategory { get; set; } = string.Empty;
    public ShippingAddress ShippingAddress { get; set; } = new();
}

// block 3: `order.ShippingAddress.Region`
public class ShippingAddress
{
    public string Region { get; set; } = string.Empty;
}

// block 4: `orderRepository.AddAsync(command, cancellationToken)`
public interface IOrderRepository
{
    Task AddAsync(CreateOrderCommand command, CancellationToken cancellationToken);
}

// block 8: `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
}

// block 8: `conn.QueryAsync<Person>(…)`
public class Person;

// block 8: `new Greeting(addGreeting.Greeting, person)`, `greeting.Message`, `greeting.RecipientId`, `greeting.Greet()`
public class Greeting(string message, Person recipient)
{
    public string Message { get; } = message;
    public long RecipientId { get; }
    public string Greet() => Message;
}

// block 8: `new GreetingMade(greeting.Greet())`
public class GreetingMade(string greeting) : Event(Id.Random());
