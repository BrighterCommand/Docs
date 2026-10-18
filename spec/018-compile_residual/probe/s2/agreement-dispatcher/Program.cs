// AgreementDispatcher.md #7 and #12, against 10.8.0.
//   #7: the page's ProcessOrder route returns ValidateOrderHandler and FinalizeOrderHandler for every
//       order, plus FraudCheckHandler and ApprovalHandler on some. ProcessOrder is a Command, and the
//       page's own § Returning Multiple Handlers says Send throws on more than one handler.
//       case:    Send a domestic, low-value order (the fewest handlers the route can return: two)
//       control: the same route on an Event, published with a high-value international order - every
//                returned handler runs (compared as a set; the order they ran in is printed)
//       control: the route returning one handler - Send runs it
//   #7, the page now: the route on OrderPlaced, an Event, published
//       case:    a high-value international order - all three handlers run (compared as a set)
//       control: a domestic, low-value order - OrderConfirmationHandler alone
//       control: FraudCheckHandler throws - the other two still run, and Publish throws
//                AggregateException ("one handler failing does not stop the others")
//   #12: "Handler3 missing - will fail at runtime"
//       case:    the page's Bad registration, returning Handler3, which the list omits
//       control: the page's Good registration, whose route returns Handler1, which the list holds
//       control: the Good list, with the route returning Handler3 - listed, so it runs
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var failed = 0;

failed += Run("#7 case     Command, page's route, Send", h =>
    // verbatim from the page's block #7
    h.Register<ProcessOrder>((request, context) =>
    {
        var order = request as ProcessOrder;
        var handlers = new List<Type>
        {
            typeof(ValidateOrderHandler)  // Always validate
        };

        // High-value orders get fraud check
        if (order?.Total > 10000m)
            handlers.Add(typeof(FraudCheckHandler));

        // International orders need approval
        if (order?.IsInternational == true)
            handlers.Add(typeof(ApprovalHandler));

        handlers.Add(typeof(FinalizeOrderHandler));  // Always finalize

        return handlers;
    },
    [
        typeof(ValidateOrderHandler),
        typeof(FraudCheckHandler),
        typeof(ApprovalHandler),
        typeof(FinalizeOrderHandler)
    ]),
    cp => cp.Send(new ProcessOrder { Total = 50m, IsInternational = false }),
    expect: "throws");

failed += Run("#7 control  Event, same route, Publish", h =>
    h.Register<OrderPlaced>((request, context) =>                                    // probe: Event
    {
        var order = request as OrderPlaced;
        var handlers = new List<Type> { typeof(ValidateOrderPlacedHandler) };
        if (order?.Total > 10000m) handlers.Add(typeof(FraudCheckOrderPlacedHandler));
        if (order?.IsInternational == true) handlers.Add(typeof(ApprovalOrderPlacedHandler));
        handlers.Add(typeof(FinalizeOrderPlacedHandler));
        return handlers;
    },
    [typeof(ValidateOrderPlacedHandler), typeof(FraudCheckOrderPlacedHandler), typeof(ApprovalOrderPlacedHandler), typeof(FinalizeOrderPlacedHandler)]),
    cp => cp.Publish(new OrderPlaced { Total = 50000m, IsInternational = true }),
    expect: "ApprovalOrderPlacedHandler,FinalizeOrderPlacedHandler,FraudCheckOrderPlacedHandler,ValidateOrderPlacedHandler",
    sorted: true);  // every returned handler runs; the order is printed, not asserted

failed += Run("#7 control  Command, one handler returned, Send", h =>
    h.Register<ProcessOrder>((request, context) => [typeof(ValidateOrderHandler)],
        [typeof(ValidateOrderHandler), typeof(FinalizeOrderHandler)]),
    cp => cp.Send(new ProcessOrder { Total = 50m }),
    expect: "ValidateOrderHandler");

failed += Run("#7 now      high-value international, Publish", PageNow.Routes.PageNow,
    cp => cp.Publish(new PageNow.OrderPlaced { Total = 50000m, IsInternational = true }),
    expect: "CustomsDeclarationHandler,FraudCheckHandler,OrderConfirmationHandler", sorted: true);
failed += Run("#7 now      domestic low-value, Publish", PageNow.Routes.PageNow,
    cp => cp.Publish(new PageNow.OrderPlaced { Total = 50m }),
    expect: "OrderConfirmationHandler");
failed += Run("#7 now      FraudCheckHandler throws", PageNow.Routes.PageNow,
    cp =>
    {
        try { cp.Publish(new PageNow.OrderPlaced { Total = 50000m, IsInternational = true, FailFraudCheck = true }); }
        catch (AggregateException) { Ran.Handlers.Add("AggregateException"); }
    },
    expect: "AggregateException,CustomsDeclarationHandler,OrderConfirmationHandler", sorted: true);

failed += Run("#12 case    Bad: Handler3 returned, not listed", h =>
    // verbatim from the page's block #12, Bad
    h.Register<MyCommand>((request, context) =>
    {
        // Might return Handler3, but it's not in the list!
        return [typeof(Handler3)];
    },
    [
        typeof(Handler1),
        typeof(Handler2)
        // Handler3 missing - will fail at runtime!
    ]),
    cp => cp.Send(new MyCommand()),
    expect: "throws");

failed += Run("#12 control Good: Handler1 returned, listed", h =>
    // verbatim from the page's block #12, Good
    h.Register<MyCommand>((request, context) =>
    {
        // ...
        return [typeof(Handler1)];
    },
    [
        typeof(Handler1),
        typeof(Handler2),
        typeof(Handler3)
        // All handlers that might be returned
    ]),
    cp => cp.Send(new MyCommand()),
    expect: "Handler1");

failed += Run("#12 control Good list, Handler3 returned", h =>
    h.Register<MyCommand>((request, context) => [typeof(Handler3)],                 // probe
        [typeof(Handler1), typeof(Handler2), typeof(Handler3)]),
    cp => cp.Send(new MyCommand()),
    expect: "Handler3");

return failed == 0 ? 0 : 1;

static int Run(string name, Action<IAmASubscriberRegistry> register, Action<IAmACommandProcessor> act, string expect, bool sorted = false)
{
    var services = new ServiceCollection();
    services.AddBrighter().Handlers(register);
    var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    Ran.Handlers.Clear();
    string outcome;
    try
    {
        act(cp);
        outcome = string.Join(",", Ran.Handlers);
        if (sorted)
        {
            Console.WriteLine($"     ran in the order {outcome}");
            Ran.Handlers.Sort(StringComparer.Ordinal);
            outcome = string.Join(",", Ran.Handlers);
        }
    }
    catch (Exception e)
    {
        outcome = "throws";
        var detail = $"{e.GetType().Name}: {e.Message.Split('\n')[0]}";
        for (var inner = e.InnerException; inner != null; inner = inner.InnerException)
            detail += $" <- {inner.GetType().Name}: {inner.Message.Split('\n')[0]}";
        Console.WriteLine($"     {detail}");
    }
    var ok = outcome == expect;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {outcome}");
    return ok ? 0 : 1;
}

public static class Ran { public static readonly List<string> Handlers = []; }

public class Recording<T> : RequestHandler<T> where T : class, IRequest
{
    public override T Handle(T request) { lock (Ran.Handlers) Ran.Handlers.Add(GetType().Name); return base.Handle(request); }
}

public class MyCommand() : Command(Id.Random());
public class Handler1 : Recording<MyCommand>;
public class Handler2 : Recording<MyCommand>;
public class Handler3 : Recording<MyCommand>;

public class ProcessOrder() : Command(Id.Random())
{
    public decimal Total { get; set; }
    public bool IsInternational { get; set; }
}
public class ValidateOrderHandler : Recording<ProcessOrder>;
public class FraudCheckHandler : Recording<ProcessOrder>;
public class ApprovalHandler : Recording<ProcessOrder>;
public class FinalizeOrderHandler : Recording<ProcessOrder>;

public class OrderPlaced() : Event(Id.Random())
{
    public decimal Total { get; set; }
    public bool IsInternational { get; set; }
}
public class ValidateOrderPlacedHandler : Recording<OrderPlaced>;
public class FraudCheckOrderPlacedHandler : Recording<OrderPlaced>;
public class ApprovalOrderPlacedHandler : Recording<OrderPlaced>;
public class FinalizeOrderPlacedHandler : Recording<OrderPlaced>;


namespace PageNow
{
    public static class Routes
    {
        // verbatim from the page's block #7, as repaired
        public static readonly Action<IAmASubscriberRegistry> PageNow = registry =>
            registry.Register<OrderPlaced>((request, context) =>
            {
                var order = request as OrderPlaced;
                var handlers = new List<Type>
                {
                    typeof(OrderConfirmationHandler)  // Every order is confirmed
                };

                // High-value orders get a fraud check
                if (order?.Total > 10000m)
                    handlers.Add(typeof(FraudCheckHandler));

                // International orders need a customs declaration
                if (order?.IsInternational == true)
                    handlers.Add(typeof(CustomsDeclarationHandler));

                return handlers;
            },
            [
                typeof(OrderConfirmationHandler),
                typeof(FraudCheckHandler),
                typeof(CustomsDeclarationHandler)
            ]);
    }

    public class OrderPlaced() : Event(Id.Random())
    {
        public decimal Total { get; set; }
        public bool IsInternational { get; set; }
        public bool FailFraudCheck { get; set; }
    }

    public class OrderConfirmationHandler : Recording<OrderPlaced>;
    public class CustomsDeclarationHandler : Recording<OrderPlaced>;
    public class FraudCheckHandler : RequestHandler<OrderPlaced>
    {
        public override OrderPlaced Handle(OrderPlaced order)
        {
            if (order.FailFraudCheck) throw new InvalidOperationException("fraud service unavailable");
            lock (Ran.Handlers) Ran.Handlers.Add(nameof(FraudCheckHandler));
            return base.Handle(order);
        }
    }
}
