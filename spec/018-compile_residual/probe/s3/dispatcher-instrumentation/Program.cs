// Task 5.4, HowServiceActivatorWorks.md #11 (ledger 57) and the sentence before it.
// Claim: the Dispatcher reads its InstrumentationOptions from AddConsumers' options; "the Command
// Processor takes its own, separately, from AddBrighter's options".
// Read first: ConsumersOptions derives from BrighterOptions, and AddConsumers hands the same instance
// to BrighterHandlerBuilder, which builds the Command Processor.
// Cases, one process each:
//   consumers-all            AddConsumers(All)                         -- the page's block
//   consumers-info           AddConsumers(RequestInformation)          -- control: no RequestBody
//   brighter-then-consumers  AddBrighter(All), then AddConsumers(RequestInformation).Handlers(...)
//   transport-pages          AddBrighter(All).Handlers(...), then AddConsumers(RequestInformation), bare:
//                            the shape of GcpPubSub, MQTT, MSSQLMessageBroker, Redis and RocketMQ #1
// A raw message is enqueued on the internal bus, so every Command Processor span comes from the pump.
// Prints, for the pump's process span and the Command Processor's spans under it, whether each
// carries its body tag (RequestBody) and its request-information tag.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var mode = args.Length > 0 ? args[0] : "consumers-all";
var spans = new List<Activity>();
using var listener = new ActivityListener
{
    ShouldListenTo = s => s.Name.Equals("paramore.brighter", StringComparison.OrdinalIgnoreCase),
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = a => { lock (spans) spans.Add(a); }
};
ActivitySource.AddActivityListener(listener);

var topic = new RoutingKey($"probe.greeting.{Guid.NewGuid():N}");
var bus = new InternalBus();
var subscription = new Subscription<Greeting>(new SubscriptionName("probe.greeting"),
    new ChannelName(topic.Value), topic, timeOut: TimeSpan.FromMilliseconds(200),
    messagePumpType: MessagePumpType.Reactor);

var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton<IAmABrighterTracer>(new BrighterTracer(TimeProvider.System));

if (mode == "brighter-then-consumers")                                         // probe
    builder.Services.AddBrighter(o => o.InstrumentationOptions = InstrumentationOptions.All);
if (mode == "transport-pages")                                                 // probe
    builder.Services.AddBrighter(o => o.InstrumentationOptions = InstrumentationOptions.All)
        .Handlers(r => r.Register<Greeting, GreetingHandler>());

var consumerOptions = mode == "consumers-all" ? InstrumentationOptions.All : InstrumentationOptions.RequestInformation;
var consumers = builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = [subscription];
        options.DefaultChannelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
        options.InstrumentationOptions = consumerOptions;
    });
if (mode != "transport-pages") consumers.Handlers(r => r.Register<Greeting, GreetingHandler>());
builder.Services.AddHostedService<ServiceActivatorHostedService>();

using var host = builder.Build();
await host.StartAsync();
var greeting = new Greeting { Name = "probe" };
bus.Enqueue(new Message(new MessageHeader(greeting.Id, topic, MessageType.MT_EVENT),
    new MessageBody(JsonSerializer.Serialize(greeting, new JsonSerializerOptions(JsonSerializerDefaults.Web)))));
var handled = await Task.WhenAny(GreetingHandler.Handled.Task, Task.Delay(TimeSpan.FromSeconds(20))) == GreetingHandler.Handled.Task;
await Task.Delay(500);
await host.StopAsync();

List<Activity> all;
lock (spans) all = spans.ToList();
var process = all.Where(a => a.Kind == ActivityKind.Consumer && a.DisplayName.Contains("process")).ToList();
var processIds = process.Select(p => p.Id).ToHashSet();
var cp = all.Where(a => a.Kind == ActivityKind.Internal && a.ParentId is not null && processIds.Contains(a.ParentId)).ToList();
static string Has(IEnumerable<Activity> xs, string tag) => xs.Any() ? (xs.All(a => a.GetTagItem(tag) is not null) ? "yes" : "no") : "n/a";
var pumpBody = Has(process, BrighterSemanticConventions.MessageBody);
var cpBody = Has(cp, BrighterSemanticConventions.RequestBody);
Console.WriteLine($"{mode}: handled={handled}; pump process spans {process.Count}: messaging.message.body={pumpBody}; " +
                  $"Command Processor spans {cp.Count} [{string.Join(", ", cp.Select(a => a.DisplayName).Distinct())}]: " +
                  $"request.body={cpBody}, request.id={Has(cp, BrighterSemanticConventions.RequestId)}");
// Expected, from the source read above: one options instance for both when AddConsumers stands alone;
// with AddBrighter first, the Command Processor and its registry are AddBrighter's
var expected = mode switch
{
    "consumers-all" => (true, "yes", "yes"),
    "consumers-info" => (true, "no", "no"),
    "brighter-then-consumers" => (false, "no", "yes"),
    "transport-pages" => (true, "no", "yes"),
    _ => throw new ArgumentException(mode)
};
var ok = (handled, pumpBody, cpBody) == expected;
Console.WriteLine($"{(ok ? "ok" : "FAIL")} {mode}: expected handled={expected.Item1}, pump body={expected.Item2}, Command Processor body={expected.Item3}");
return ok ? 0 : 1;

public class Greeting() : Event(Id.Random()) { public string Name { get; set; } = ""; }
public class GreetingHandler : RequestHandler<Greeting>
{
    public static readonly TaskCompletionSource Handled = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public override Greeting Handle(Greeting e) { Handled.TrySetResult(); return base.Handle(e); }
}
