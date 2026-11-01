// Types and values TestDoubleOptions.md names in its blocks and never declares.
//
// The page shows how to verify a handler's calls to the command processor, and the handlers and
// requests it verifies are the reader's own. Every value member is typed from a pinned package
// and returns a default; a stub is a request, or a handler with only the members a block names.
//
// blockcheck: using static TestDoubleOptionsContext;

using Paramore.Brighter;
using Paramore.Brighter.Testing;

public static class TestDoubleOptionsContext
{
    // blocks 3, 4: `spy.GetRequests<MyCommand>()`, `spy.GetCalls(CommandType.Send)`, `spy.RecordedCalls`
    public static SpyCommandProcessor spy => null!;
}

// blocks 3, 11, 12: `spy.GetRequests<MyCommand>()`, `handler.Handle(new MyCommand())`
public class MyCommand() : Command(Id.Random());

// blocks 11, 12: `p.Publish(It.IsAny<MyEvent>(), …)`
public class MyEvent() : Event(Id.Random());

// blocks 5, 11, 12: `new MyHandler(spy)`, `handler.Handle(new TriggerCommand())`, `handler.Handle(new MyCommand())`
public class MyHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(MyCommand command) => commandProcessor.Publish(new MyEvent());
    public void Handle(TriggerCommand command) => commandProcessor.Send(new DownstreamCommand());
}

// block 1: `new PlaceOrder { ProductId = "WIDGET-1", Quantity = 3 }`; block 6: `new PlaceOrder { OrderId = "ORD-1" }`
public class PlaceOrder() : Command(Id.Random())
{
    public string ProductId { get; set; } = "";
    public int Quantity { get; set; }
    public string OrderId { get; set; } = "";
}

// block 1: `published.ProductId`; block 6: `events.First().OrderId`
public class OrderPlaced() : Event(Id.Random())
{
    public string ProductId { get; set; } = "";
    public string OrderId { get; set; } = "";
}

// block 1: `new PlaceOrderHandler(spy)`, `handler.Handle(command)`
public class PlaceOrderHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(PlaceOrder command) => commandProcessor.Publish(new OrderPlaced { ProductId = command.ProductId });
}

// block 6: `new OrderHandler(spy)`, `handler.Handle(new PlaceOrder { … })`
public class OrderHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(PlaceOrder command) => commandProcessor.Publish(new OrderPlaced { OrderId = command.OrderId });
}

// block 5: `handler.Handle(new TriggerCommand())`
public class TriggerCommand() : Command(Id.Random());

// block 5: `spy.Observe<DownstreamCommand>()`, `sent.SomeProperty`
public class DownstreamCommand() : Command(Id.Random())
{
    public string SomeProperty { get; set; } = "";
}

// block 7: `new SendNotification { UserId = "user-1" }`
public class SendNotification() : Command(Id.Random())
{
    public string UserId { get; set; } = "";
}

// block 7: `spy.Observe<NotificationSent>()`, `posted.UserId`
public class NotificationSent() : Event(Id.Random())
{
    public string UserId { get; set; } = "";
}

// block 7: `new NotificationHandler(spy)`, `handler.Handle(new SendNotification { … })`
public class NotificationHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(SendNotification command) => commandProcessor.Post(new NotificationSent { UserId = command.UserId });
}

// block 8: `new ProcessPayment { Amount = 99.99m }`
public class ProcessPayment() : Command(Id.Random())
{
    public decimal Amount { get; set; }
}

// block 8: `request.ShouldBeOfType<PaymentProcessed>()`, `deposited.Amount`
public class PaymentProcessed() : Event(Id.Random())
{
    public decimal Amount { get; set; }
}

// block 8: `new TransactionalHandler(spy)`, `handler.Handle(new ProcessPayment { … })`
public class TransactionalHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(ProcessPayment command) => commandProcessor.DepositPost(new PaymentProcessed { Amount = command.Amount });
}

// block 9: `spy.Send(new CommandA())`
public class CommandA() : Command(Id.Random());
