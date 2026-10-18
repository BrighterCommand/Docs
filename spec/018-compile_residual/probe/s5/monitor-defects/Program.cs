// Task 4.5a (D19), Monitoring.md § Monitoring Limitations. The configuration is the page's block 1,
// verbatim but for the handlers it registers. Four rows per version:
//   sync ok       [Monitor] on a handler that returns: the control, works at both versions
//   sync throws   [Monitor] on a handler that throws: the caller should see its own exception;
//                 #4453 says it sees NotSupportedException (System.Reflection.MethodBase) at 10.7.0
//   async ok      [MonitorAsync] on a handler that returns; #4454 says "No message mapper defined"
//   async +reg    the same, with the sender also registered as IAmAControlBusSenderAsync, which
//                 MonitorHandlerAsync takes and the page's block does not register ("reg" argument)
//   events        how many MonitorEvent messages the sender put on the bus
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Monitoring.Attributes;
using Paramore.Brighter.Monitoring.Configuration;
using Paramore.Brighter.Monitoring.Events;
using Paramore.Brighter.Observability;

var monitoringTopic = new RoutingKey("brighter.monitoring");
var bus = new InternalBus();                                   // probe: kept, to count the events
var producerRegistry = new InMemoryProducerRegistryFactory(
        bus,
        [new Publication { Topic = monitoringTopic, RequestType = typeof(MonitorEvent) }],
        InstrumentationOptions.None)
    .Create();

var controlBusSender = new ControlBusSenderFactory().Create<Message, CommittableTransaction>(
    new InMemoryOutbox(TimeProvider.System), producerRegistry, new BrighterTracer());

var services = new ServiceCollection();
services.AddSingleton(controlBusSender);
if (args.Length > 0 && args[0] == "reg")                       // probe
    services.AddSingleton((IAmAControlBusSenderAsync)controlBusSender);
services.AddSingleton(new MonitorConfiguration
{
    IsMonitoringEnabled = true,
    InstanceName = "OrdersService"
});
services.AddBrighter()
    .AutoFromAssemblies();

var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
var version = typeof(Message).Assembly.GetName().Version;

string Run(Func<Task> act) { try { act().GetAwaiter().GetResult(); return "returns"; } catch (Exception e) { var m = ""; for (var x = e; x != null; x = x.InnerException) m += $"{(m == "" ? "" : " <- ")}{x.GetType().Name}: {x.Message.Split('\n')[0]}"; return m; } }

Console.WriteLine($"{version} sync ok     : {Run(() => { cp.Send(new Greeting()); return Task.CompletedTask; })}");
Console.WriteLine($"{version} sync throws : {Run(() => { cp.Send(new Failing()); return Task.CompletedTask; })}");
Console.WriteLine($"{version} async {(args.Length > 0 ? "+" + args[0] : "    ")} : {Run(() => cp.SendAsync(new GreetingAsync()))}");
Console.WriteLine($"{version} events      : {System.Linq.Enumerable.Count(bus.Stream(monitoringTopic))}");
return 0;

public class Greeting() : Command(Id.Random());
public class Failing() : Command(Id.Random());
public class GreetingAsync() : Command(Id.Random());

public class GreetingHandler : RequestHandler<Greeting>
{
    [Monitor(step: 1, timing: HandlerTiming.Before, handlerType: typeof(GreetingHandler))]
    public override Greeting Handle(Greeting command) => base.Handle(command);
}

public class FailingHandler : RequestHandler<Failing>
{
    [Monitor(step: 1, timing: HandlerTiming.Before, handlerType: typeof(FailingHandler))]
    public override Failing Handle(Failing command) => throw new InvalidOperationException("the handler's own exception");
}

public class GreetingHandlerAsync : RequestHandlerAsync<GreetingAsync>
{
    [MonitorAsync(step: 1, timing: HandlerTiming.Before, handlerType: typeof(GreetingHandlerAsync))]
    public override Task<GreetingAsync> HandleAsync(GreetingAsync command, CancellationToken cancellationToken = default)
        => base.HandleAsync(command, cancellationToken);
}
