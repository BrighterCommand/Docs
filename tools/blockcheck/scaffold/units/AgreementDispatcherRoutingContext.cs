// Types and values AgreementDispatcherRouting.md names in its blocks and never declares.
//
// The page explains routing by content through five scenarios, each registering a route over
// requests and handlers it names and never shows. A request stub carries only the properties a
// route reads; a handler stub is a handler of its request and nothing more, since a block names it
// only in `typeof(…)`.
//
// blockcheck: using static AgreementDispatcherRoutingContext;

using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

public static class AgreementDispatcherRoutingContext
{
    // blocks 1, 2, 8, 9: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;

    // blocks 3–7, 10, 11: `registry.Register<…>(…)`, `registry.RegisterAsync<…>(…)`
    public static ServiceCollectionSubscriberRegistry registry => null!;
}

// blocks 1, 2, 8–11: `registry.Register<MyCommand…>`, `command?.Priority`, `cmd?.Type`
public class MyCommand() : Command(Id.Random())
{
    public string? Priority { get; set; }
    public string? Type { get; set; }
}

// block 1: `registry.Register<MyCommand, MyCommandHandler>()`
public class MyCommandHandler : RequestHandler<MyCommand>;

// block 2
public class HighPriorityHandler : RequestHandler<MyCommand>;
public class StandardHandler : RequestHandler<MyCommand>;

// blocks 8–10
public class Handler1 : RequestHandler<MyCommand>;
public class Handler2 : RequestHandler<MyCommand>;
public class Handler3 : RequestHandler<MyCommand>;

// block 11
public class FastHandler : RequestHandler<MyCommand>;
public class SlowHandler : RequestHandler<MyCommand>;

// block 9: `registry.Register<OtherCommand, OtherCommandHandler>()`
public class OtherCommand() : Command(Id.Random());
public class OtherCommandHandler : RequestHandler<OtherCommand>;

// blocks 3, 5: `order?.OrderDate`, `order?.Type`, `order?.IsPreOrder`, `order?.ContainsHazardousMaterials`
public class ProcessOrder() : Command(Id.Random())
{
    public DateTime? OrderDate { get; set; }
    public OrderType Type { get; set; }
    public bool IsPreOrder { get; set; }
    public bool ContainsHazardousMaterials { get; set; }
}

// block 5: `OrderType.Digital`
public enum OrderType { Digital }

// block 3
public class LegacyTaxOrderHandler : RequestHandler<ProcessOrder>;
public class ModernTaxOrderHandler : RequestHandler<ProcessOrder>;

// block 5
public class DigitalOrderHandler : RequestHandler<ProcessOrder>;
public class PreOrderHandler : RequestHandler<ProcessOrder>;
public class HazmatOrderHandler : RequestHandler<ProcessOrder>;
public class StandardOrderHandler : RequestHandler<ProcessOrder>;

// block 4: `payment?.Country`
public class ProcessPayment() : Command(Id.Random())
{
    public string? Country { get; set; }
}

// block 4
public class USPaymentHandler : RequestHandler<ProcessPayment>;
public class UKPaymentHandler : RequestHandler<ProcessPayment>;
public class EUPaymentHandler : RequestHandler<ProcessPayment>;
public class JapanPaymentHandler : RequestHandler<ProcessPayment>;
public class InternationalPaymentHandler : RequestHandler<ProcessPayment>;

// block 6: `createUser?.ApiVersion`
public class CreateUser() : Command(Id.Random())
{
    public string? ApiVersion { get; set; }
}

// block 6
public class CreateUserV1HandlerAsync : RequestHandlerAsync<CreateUser>;
public class CreateUserV2HandlerAsync : RequestHandlerAsync<CreateUser>;
public class CreateUserV3HandlerAsync : RequestHandlerAsync<CreateUser>;
public class CreateUserLatestHandlerAsync : RequestHandlerAsync<CreateUser>;

// block 7: `refund?.OrderStatus`
public class ProcessRefund() : Command(Id.Random())
{
    public OrderStatus? OrderStatus { get; set; }
}

// block 7: `OrderStatus.Pending`, `.Shipped`, `.Delivered`, `.PartiallyReturned`
public enum OrderStatus { Pending, Shipped, Delivered, PartiallyReturned }

// block 7
public class CancelOrderRefundHandler : RequestHandler<ProcessRefund>;
public class ReturnAndRefundHandler : RequestHandler<ProcessRefund>;
public class FullRefundHandler : RequestHandler<ProcessRefund>;
public class PartialRefundHandler : RequestHandler<ProcessRefund>;
