// Task 4.5a (D19), Telemetry.md § the message pump's span attributes.
// Claim: messaging.system is "Always internal_bus on pump spans at 10.7.0, whichever transport the
// pump reads". At 10.8.0, Reactor and Proactor read it from the channel (IHaveAMessagingSystem).
// Case: "postgres", a Reactor over the Postgres transport, at 10.8.0. Controls: the same at 10.7.0
// (the claim), and "inmemory" at each version, where internal_bus is the right answer.
// The tracer is registered as AddBrighterInstrumentation() registers it, and an ActivityListener
// stands in for the OpenTelemetry SDK. Prints the distinct messaging.system values on Consumer spans.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Postgres;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

var mode = args.Length > 0 ? args[0] : "postgres";
var version = FileVersionInfo.GetVersionInfo(typeof(CommandProcessor).Assembly.Location).ProductVersion?.Split('+')[0];
var systems = new List<string>();
using var listener = new ActivityListener
{
    ShouldListenTo = s => s.Name.Equals("paramore.brighter", StringComparison.OrdinalIgnoreCase),
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStarted = a =>
    {
        var system = a.GetTagItem("messaging.system")?.ToString();
        if (a.Kind == ActivityKind.Consumer && system is not null) lock (systems) systems.Add($"{a.DisplayName}={system}");
    }
};
ActivitySource.AddActivityListener(listener);

var topic = new RoutingKey($"probe.greeting.{Guid.NewGuid():N}");
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton<IAmABrighterTracer>(new BrighterTracer(TimeProvider.System));

IAmAProducerRegistry producers;
IAmAChannelFactory channelFactory;
Subscription subscription;
if (mode == "postgres")
{
    var connection = new PostgresMessagingGatewayConnection(new RelationalDatabaseConfiguration(
        "Host=localhost;Username=postgres;Password=password;Database=brightertests;"));
    channelFactory = new PostgresChannelFactory(connection);
    subscription = new PostgresSubscription<Greeting>(new SubscriptionName("probe.greeting"),
        new ChannelName(topic.Value), topic, timeOut: TimeSpan.FromMilliseconds(500),
        messagePumpType: MessagePumpType.Reactor, makeChannels: OnMissingChannel.Create);
    producers = new PostgresProducerRegistryFactory(connection,
        [new PostgresPublication<Greeting> { Topic = topic, MakeChannels = OnMissingChannel.Create }]).Create();
}
else
{
    var bus = new InternalBus();
    channelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
    subscription = new Subscription<Greeting>(new SubscriptionName("probe.greeting"),
        new ChannelName(topic.Value), topic, timeOut: TimeSpan.FromMilliseconds(500),
        messagePumpType: MessagePumpType.Reactor);
    producers = new ProducerRegistry(new Dictionary<RoutingKey, IAmAMessageProducer>
    {
        [topic] = new InMemoryMessageProducer(bus, new Publication { Topic = topic, RequestType = typeof(Greeting) })
    });
}

builder.Services.AddConsumers(options =>
    {
        options.Subscriptions = [subscription];
        options.DefaultChannelFactory = channelFactory;
        options.InstrumentationOptions = InstrumentationOptions.All;
    })
    .AddProducers(p => p.ProducerRegistry = producers)
    .Handlers(r => r.Register<Greeting, GreetingHandler>());
builder.Services.AddHostedService<ServiceActivatorHostedService>();

using var host = builder.Build();
await host.StartAsync();
host.Services.GetRequiredService<IAmACommandProcessor>().Post(new Greeting());
var handled = await Task.WhenAny(GreetingHandler.Handled.Task, Task.Delay(TimeSpan.FromSeconds(20))) == GreetingHandler.Handled.Task;
await host.StopAsync();

string[] distinct;
lock (systems) distinct = systems.Select(s => s.Split('=').Last()).Distinct().ToArray();
Console.WriteLine($"{version} {mode}: handled={handled}, messaging.system on pump spans = [{string.Join(", ", distinct)}] ({systems.Count} spans)");
return handled && distinct.Length > 0 ? 0 : 1;

public class Greeting() : Event(Id.Random());
public class GreetingHandler : RequestHandler<Greeting>
{
    public static readonly TaskCompletionSource Handled = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public override Greeting Handle(Greeting e) { Handled.TrySetResult(); return base.Handle(e); }
}
