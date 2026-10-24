// HealthChecks.md: the Dispatcher health checks at 10.8.1, each run with a control. Run twice:
//   dotnet run -p:BrighterVersion=10.8.1   (the case)
//   dotnet run -p:BrighterVersion=10.8.0   (the control: AddBrighterConsumerHealthChecks does not exist,
//                                           and the old check does not count channel failures)
// No broker is needed: "good" reads an empty in-memory channel; "bad" reads a channel whose every
// Receive throws ChannelFailureException.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Paramore.Brighter;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

Console.WriteLine($"Brighter {typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion}");

var good = new Subscription<Ping>(new SubscriptionName("good"), new ChannelName("good"), new RoutingKey("good"),
    messagePumpType: MessagePumpType.Reactor, timeOut: TimeSpan.FromMilliseconds(100),
    channelFactory: new InMemoryChannelFactory(new InternalBus(), TimeProvider.System));
var bad = new Subscription<Ping>(new SubscriptionName("bad"), new ChannelName("bad"), new RoutingKey("bad"),
    messagePumpType: MessagePumpType.Reactor, timeOut: TimeSpan.FromMilliseconds(100),
    channelFailureDelay: TimeSpan.FromMilliseconds(100), channelFactory: new FailingChannelFactory());

async Task Run(string label, Subscription[] subscriptions, Action<IHealthChecksBuilder> register)
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddConsumers(o => o.Subscriptions = subscriptions).Handlers(r => r.Register<Ping, PingHandler>());
    register(services.AddHealthChecks());
    await using var sp = services.BuildServiceProvider();

    foreach (var r in sp.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations)
        Console.WriteLine($"  [{label}] registered {r.Name}, failureStatus {r.FailureStatus}, tags [{string.Join(",", r.Tags)}]");

    var dispatcher = sp.GetRequiredService<IDispatcher>();
    dispatcher.Receive();
    await Task.Delay(2000);   // "bad" fails ~every 100 ms, well past the default threshold of 3
    var report = await sp.GetRequiredService<HealthCheckService>().CheckHealthAsync();
    foreach (var (name, e) in report.Entries)
        Console.WriteLine($"  [{label}] {name}: {e.Status} - {e.Description}{(e.Exception is null ? "" : " EXC " + e.Exception.GetType().Name + ": " + e.Exception.Message)}");
    await dispatcher.End();
}

#pragma warning disable CS0618 // BrighterServiceActivatorHealthCheck is [Obsolete] from 10.8.1
// 1. The old form still activates, and on 10.8.1 reports channel failures as Degraded.
//    Control: only "good" -> Healthy. On 10.8.0 "bad" too reads Healthy: failures are not counted.
await Run("old, good only", [good], b => b.AddCheck<BrighterServiceActivatorHealthCheck>("Brighter", HealthStatus.Unhealthy));
await Run("old, good+bad ", [good, bad], b => b.AddCheck<BrighterServiceActivatorHealthCheck>("Brighter", HealthStatus.Unhealthy));
#pragma warning restore CS0618

#if B1081
// 2. The replacement registers two tagged checks; the take-out-of-service one reports the failing consumer.
//    Control: only "good" -> both Healthy.
await Run("new, good only", [good], b => b.AddBrighterConsumerHealthChecks());
await Run("new, good+bad ", [good, bad], b => b.AddBrighterConsumerHealthChecks());
#endif

public class Ping() : Command(Id.Random());
public class PingHandler : RequestHandler<Ping>;

public class FailingChannelFactory : IAmAChannelFactory
{
    public IAmAChannelSync CreateSyncChannel(Subscription s) => new Channel(s.ChannelName, s.RoutingKey, new FailingConsumer(), s.BufferSize);
    public IAmAChannelAsync CreateAsyncChannel(Subscription s) => throw new NotSupportedException();
    public Task<IAmAChannelAsync> CreateAsyncChannelAsync(Subscription s, CancellationToken ct = default) => throw new NotSupportedException();
}

public class FailingConsumer : IAmAMessageConsumerSync
{
    public void Acknowledge(Message message) { }
    public bool Reject(Message message, MessageRejectionReason? reason = null) => true;
    public void Purge() { }
    public Message[] Receive(TimeSpan? timeOut = null) => throw new ChannelFailureException("broker unreachable");
    public void Nack(Message message) { }
    public bool Requeue(Message message, TimeSpan? delay = null) => true;
    public void Dispose() { }
}
