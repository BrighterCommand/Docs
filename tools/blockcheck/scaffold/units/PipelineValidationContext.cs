// Types and values PipelineValidation.md names in its blocks and never declares.
//
// The page configures Brighter on an application `builder` and a `services` collection it never
// creates, for handlers of an `OrderCreated` it never shows. Values return a default and do
// nothing; a block that calls a member of one is checked against the real type.
//
// blockcheck: using static PipelineValidationContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

public static class PipelineValidationContext
{
    // blocks 1, 2: `builder.Services.AddBrighter(…)`
    public static WebApplicationBuilder builder => null!;
    // blocks 14, 15, 16, 17: `services.AddConsumers(…)`, `services.AddBrighter()`
    public static IServiceCollection services => null!;
    // block 14: `new RmqMessageConsumerFactory(rmqConnection)`
    public static RmqMessagingGatewayConnection rmqConnection => null!;
    // blocks 16, 17: `opts.AddMsSqlOutbox(outboxConfig)`, `opts.AddMsSqlInbox(inboxConfig)`
    public static IAmARelationalDatabaseConfiguration outboxConfig => null!;
    public static IAmARelationalDatabaseConfiguration inboxConfig => null!;
}

// block 1: `typeof(MyHandler).Assembly`
public class MyHandler;

// blocks 8–14: `RequestHandlerAsync<OrderCreated>`, `RmqSubscription<OrderCreated>`
public class OrderCreated() : Event(Id.Random());

// block 15: `typeof(OrderCreatedHandler).Assembly`, the handler in an assembly nothing has loaded
public class OrderCreatedHandler : RequestHandlerAsync<OrderCreated>;

// block 16: `RequestHandler<ProcessPayment>`
public class ProcessPayment() : Command(Id.Random());
