// Task 4.5a (D19), Telemetry.md § The Command Processor also needs an external bus.
// Claim: at 10.7.0, with the tracer registered and no AddProducers, Send records no span.
// The tracer is registered as AddBrighterInstrumentation() registers it (a BrighterTracer
// singleton as IAmABrighterTracer), and an ActivityListener stands in for the OpenTelemetry SDK.
// Case: 10.8.0, "nobus". Controls: 10.7.0 "nobus" (the claim), and "bus" at each version, an
// in-memory producer registered with AddProducers, where both versions should record a span.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;

var mode = args.Length > 0 ? args[0] : "nobus";
var spans = new List<string>();
using var listener = new ActivityListener
{
    ShouldListenTo = s => s.Name.Equals("paramore.brighter", StringComparison.OrdinalIgnoreCase),
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = a => { lock (spans) spans.Add(a.DisplayName); }
};
ActivitySource.AddActivityListener(listener);

var services = new ServiceCollection();
services.AddSingleton<IAmABrighterTracer>(new BrighterTracer(TimeProvider.System));
var builder = services.AddBrighter();
if (mode == "bus")
{
    var bus = new InternalBus();
    builder.AddProducers(p => p.ProducerRegistry = new ProducerRegistry(new Dictionary<RoutingKey, IAmAMessageProducer>
    {
        [new RoutingKey("unused")] = new InMemoryMessageProducer(bus, new Publication { Topic = new RoutingKey("unused") })
    }));
}
builder.Handlers(r => r.Register<Ping, PingHandler>());

var provider = services.BuildServiceProvider();
provider.GetRequiredService<IAmACommandProcessor>().Send(new Ping());

Console.WriteLine($"{(PingHandler.Handled == 1 ? "handled" : "NOT HANDLED")}, {mode}: {spans.Count} span(s) [{string.Join(", ", spans)}]");
return PingHandler.Handled == 1 ? 0 : 1;

public class Ping() : Command(Id.Random());
public class PingHandler : RequestHandler<Ping>
{
    public static int Handled;
    public override Ping Handle(Ping command) { Interlocked.Increment(ref Handled); return base.Handle(command); }
}
