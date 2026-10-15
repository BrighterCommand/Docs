// Types and values AgreementDispatcher.md names in its blocks and never declares.
//
// The page registers routes over requests and handlers it names and never shows. A request stub
// carries only the properties a route reads; a handler stub is a handler of its request and
// nothing more, since a block names it only in `typeof(…)`.
//
// blockcheck: using static AgreementDispatcherContext;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

public static class AgreementDispatcherContext
{
    // blocks 1–5, 8–13: `registry.Register<…>(…)`, `registry.RegisterAsync<…>(…)`
    public static ServiceCollectionSubscriberRegistry registry => null!;
    // blocks 6, 14: `services.AddConsumers(…)`, `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
    // block 8, the "Bad" route: the expensive calls it warns against
    public static object LoadConfigFromDatabase() => null!;
    public static object CallExternalApi(MyCommand? command) => null!;
    public static List<Type> CalculateComplexRouting(MyCommand? command, object config, object result) => null!;
}

// blocks 2, 4, 5, 8, 12–14: `request as MyCommand`, `myCommand?.Value`, `cmd?.Priority`
public class MyCommand() : Command(Id.Random())
{
    public string? Value { get; set; }
    public string? Priority { get; set; }
}

public class SpecialHandler : RequestHandler<MyCommand>;
public class StandardHandler : RequestHandler<MyCommand>;
public class HighPriorityHandler : RequestHandler<MyCommand>;
public class HighPriorityHandlerAsync : RequestHandlerAsync<MyCommand>;
public class StandardHandlerAsync : RequestHandlerAsync<MyCommand>;
public class Handler1 : RequestHandler<MyCommand>;
public class Handler2 : RequestHandler<MyCommand>;
public class Handler3 : RequestHandler<MyCommand>;
public class MyHandler : RequestHandler<MyCommand>;
public class MyOtherHandler : RequestHandler<MyCommand>;

// blocks 3, 7, 9: `order?.Country`, `order?.Total`, `order?.IsInternational`
public class ProcessOrder() : Command(Id.Random())
{
    public string? Country { get; set; }
    public decimal Total { get; set; }
    public bool IsInternational { get; set; }
}

public class PremiumOrderHandler : RequestHandler<ProcessOrder>;
public class StandardOrderHandler : RequestHandler<ProcessOrder>;
public class USOrderHandler : RequestHandler<ProcessOrder>;
public class UKOrderHandler : RequestHandler<ProcessOrder>;
public class EUOrderHandler : RequestHandler<ProcessOrder>;
public class ValidateOrderHandler : RequestHandler<ProcessOrder>;
public class FraudCheckHandler : RequestHandler<ProcessOrder>;
public class ApprovalHandler : RequestHandler<ProcessOrder>;
public class FinalizeOrderHandler : RequestHandler<ProcessOrder>;

// block 6: `order?.Country`
public class OrderCreated() : Event(Id.Random())
{
    public string? Country { get; set; }
}

public class USOrderCreatedHandler : RequestHandler<OrderCreated>;
public class UKOrderCreatedHandler : RequestHandler<OrderCreated>;
public class InternationalOrderCreatedHandler : RequestHandler<OrderCreated>;

// block 7: `taxRequest?.EffectiveDate`
public class CalculateTax() : Command(Id.Random())
{
    public DateTime? EffectiveDate { get; set; }
}

public class TaxCalculator2024 : RequestHandler<CalculateTax>;
public class TaxCalculator2025 : RequestHandler<CalculateTax>;

// blocks 7, 10: `payment?.Country`
public class ProcessPayment() : Command(Id.Random())
{
    public string? Country { get; set; }
}

public class StripePaymentHandlerAsync : RequestHandlerAsync<ProcessPayment>;
public class PayPalPaymentHandlerAsync : RequestHandlerAsync<ProcessPayment>;
public class LocalPaymentHandlerAsync : RequestHandlerAsync<ProcessPayment>;
public class InternationalPaymentHandlerAsync : RequestHandlerAsync<ProcessPayment>;

// blocks 7, 11: `Register<SimpleCommand, SimpleCommandHandler>()`, `Register<ComplexCommand>(…)`, `typeof(ComplexCommandHandler)`
public class SimpleCommand() : Command(Id.Random());
public class SimpleCommandHandler : RequestHandler<SimpleCommand>;
public class ComplexCommand() : Command(Id.Random());
public class ComplexCommandHandler : RequestHandler<ComplexCommand>;
