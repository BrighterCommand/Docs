// Task 7.5. Page handlers are copied as the page shows them; the probe's own lines are marked // probe.
//  TurningOnReplayOnSeen.md #1 (ProcessPaymentHandler) and #4's ✅ handler: with the pipeline's Context passed to Post, a
//      duplicate ProcessPayment does not run the handler and marks the original ShipOrder undispatched again, for the
//      Sweeper to resend ("Replay only ever sends through the Sweeper"). Control: #4's ❌ handler, Post with no context:
//      "a silent no-op under Replay — no exception, nothing resent". #4's two handlers carry no [UseInbox]; each is given
//      #1's attribute, which is what makes Replay apply to it.
//  BrighterInboxSupport.md #1: [UseInboxAsync(step:0, ..., onceOnly: true)] with the default action throws
//      OnceOnlyException on a duplicate. Control: a second event with its own Id, handled.
// Each run: Send the command, Send the same instance again, then count the Outbox's outstanding messages and what is on
// the bus, then ClearOutbox what is outstanding (what the Sweeper would do) and count the bus again.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.Inbox.Exceptions;
using Paramore.Brighter.Observability;

public class ProcessPayment() : Command(Id.Random()) { public string OrderId { get; set; } = "ORD-1"; }   // probe
public class ShipOrder() : Command(Id.Random()) { public string OrderId { get; set; } = ""; }   // probe
public class GreetingMade() : Event(Id.Random()) { public string Greeting { get; set; } = "hello"; }   // probe
public static class Runs { public static int Count; }   // probe

// TurningOnReplayOnSeen.md #1
public class ProcessPaymentHandler : RequestHandler<ProcessPayment>
{
    private readonly IAmACommandProcessor _commandProcessor;

    public ProcessPaymentHandler(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    [UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandler), onceOnly: true,
        onceOnlyAction: OnceOnlyAction.Replay)]
    public override ProcessPayment Handle(ProcessPayment command)
    {
        Runs.Count++;   // probe
        // ... take the payment ...

        // Pass the pipeline's Context so ShipOrder is stored under this invocation's
        // Causation Id — that is what a later replay will match on
        _commandProcessor.Post(new ShipOrder { OrderId = command.OrderId },
            Context as RequestContext);

        return base.Handle(command);
    }
}

// TurningOnReplayOnSeen.md #4
// ❌ Silent no-op under Replay — a fresh context, so the Causation Id is lost
public class ProcessPaymentHandlerWithoutContext(IAmACommandProcessor commandProcessor)
    : RequestHandler<ProcessPayment>
{
    [UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandlerWithoutContext), onceOnly: true, onceOnlyAction: OnceOnlyAction.Replay)]   // probe: #1's attribute
    public override ProcessPayment Handle(ProcessPayment command)
    {
        Runs.Count++;   // probe
        commandProcessor.Post(new ShipOrder { OrderId = command.OrderId });
        return base.Handle(command);
    }
}

// ✅ Threads the pipeline's context, so the message is stored under this
//    invocation's Causation Id and a later replay can match it
public class ProcessPaymentHandlerWithContext(IAmACommandProcessor commandProcessor)
    : RequestHandler<ProcessPayment>
{
    [UseInbox(step: 1, contextKey: typeof(ProcessPaymentHandlerWithContext), onceOnly: true, onceOnlyAction: OnceOnlyAction.Replay)]   // probe: #1's attribute
    public override ProcessPayment Handle(ProcessPayment command)
    {
        Runs.Count++;   // probe
        commandProcessor.Post(new ShipOrder { OrderId = command.OrderId },
            Context as RequestContext);
        return base.Handle(command);
    }
}

// BrighterInboxSupport.md #1, in the class the page's fragment belongs to
public class GreetingMadeHandlerAsync : RequestHandlerAsync<GreetingMade>   // probe: the class
{
       [UseInboxAsync(step:0, contextKey: typeof(GreetingMadeHandlerAsync), onceOnly: true )]
        public override async Task<GreetingMade> HandleAsync(GreetingMade @event, CancellationToken cancellationToken = default(CancellationToken))
        {    
            Console.WriteLine($"     Greeting Received: {@event.Greeting}");
            
            return await base.HandleAsync(@event, cancellationToken);
        }
}

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static (IAmACommandProcessor, InMemoryOutbox, InternalBus) Build(Action<IBrighterBuilder> handlers)
    {
        var services = new ServiceCollection();
        var bus = new InternalBus();
        var outbox = new InMemoryOutbox(TimeProvider.System);
        services.AddSingleton<IAmAnInboxSync>(new InMemoryInbox(TimeProvider.System));
        services.AddSingleton<IAmAnInboxAsync>(new InMemoryInbox(TimeProvider.System));
        var b = services.AddBrighter().AddProducers(o =>
        {
            o.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
                [new Publication { Topic = new RoutingKey("ShipOrder"), RequestType = typeof(ShipOrder) }], InstrumentationOptions.All).Create();
            o.Outbox = outbox;
        });
        handlers(b);
        return (services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>(), outbox, bus);
    }

    static string Replay<THandler>() where THandler : class, IHandleRequests<ProcessPayment>
    {
        var (cp, outbox, bus) = Build(b => b.Handlers(h => h.Register<ProcessPayment, THandler>()));
        Runs.Count = 0;
        var command = new ProcessPayment();
        cp.Send(command);
        var afterFirst = outbox.OutstandingMessages(TimeSpan.Zero, new RequestContext()).Count();
        string second;
        try { cp.Send(command); second = "no exception"; } catch (Exception e) { second = e.GetType().Name; }
        var outstanding = outbox.OutstandingMessages(TimeSpan.Zero, new RequestContext()).Select(m => m.Id).ToArray();
        var onBus = bus.Stream(new RoutingKey("ShipOrder")).Count();
        if (outstanding.Length > 0) cp.ClearOutbox(outstanding, new RequestContext());
        var afterClear = bus.Stream(new RoutingKey("ShipOrder")).Count();
        return $"handler ran {Runs.Count}; duplicate: {second}; outstanding after the first send {afterFirst}, after the duplicate {outstanding.Length}; on the bus {onBus}, after clearing the outstanding {afterClear}";
    }

    public static async Task<int> Main()
    {
        var one = Replay<ProcessPaymentHandler>();
        Row("TurningOnReplayOnSeen.md #1, Context passed", one.Contains("handler ran 1;") && one.Contains("after the duplicate 1;") && one.EndsWith("after clearing the outstanding 2"), one);
        var with = Replay<ProcessPaymentHandlerWithContext>();
        Row("TurningOnReplayOnSeen.md #4 ✅, Context passed", with.Contains("handler ran 1;") && with.Contains("after the duplicate 1;") && with.EndsWith("after clearing the outstanding 2"), with);
        var without = Replay<ProcessPaymentHandlerWithoutContext>();
        Row("TurningOnReplayOnSeen.md #4 ❌, no context: a silent no-op", without.Contains("handler ran 1;") && without.Contains("duplicate: no exception") && without.Contains("after the duplicate 0;"), without);

        // BrighterInboxSupport.md #1
        var (cp, _, _) = Build(b => b.AsyncHandlers(h => h.RegisterAsync<GreetingMade, GreetingMadeHandlerAsync>()));
        var greeting = new GreetingMade();
        await cp.PublishAsync(greeting);
        string dup;
        try { await cp.PublishAsync(greeting); dup = "no exception"; }
        catch (Exception e) { var x = e is AggregateException a ? a.InnerExceptions.First() : e; dup = x.GetType().Name; }
        Row("BrighterInboxSupport.md #1, a duplicate with onceOnly and the default action", dup == nameof(OnceOnlyException), dup);
        string other;
        try { await cp.PublishAsync(new GreetingMade { Greeting = "another" }); other = "no exception"; } catch (Exception e) { other = e.GetType().Name; }
        Console.WriteLine($"     control, a new event Id: {other}");
        return failed ? 1 : 0;
    }
}
