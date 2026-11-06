// Task 6.5, the Serilog ruling. RabbitMQConnectionStability.md #3's comment, "Blocked events will be logged
// automatically", against Paramore.Brighter.MessagingGateway.RMQ.Async and .Extensions.DependencyInjection 10.8.1.
//
// The gateway logs through a static logger, ApplicationLogging.CreateLogger<RmqMessageGateway>() (RmqMessageGateway.cs:57),
// taken from ApplicationLogging.LoggerFactory, which starts as a LoggerFactory with no providers and is replaced by the
// container's ILoggerFactory only when the command processor (ServiceCollectionExtensions.cs:776) or the Dispatcher
// (ServiceActivator's ServiceCollectionExtensions.cs:141) is built. A logger taken from the empty factory stays bound to it.
//
// The block is the page's, as rewritten; the probe adds a capturing provider and a real broker alarm:
// rabbitmqctl set_vm_memory_high_watermark 0 makes the broker send connection.blocked, and 0.4 connection.unblocked.
//   case:    the producer registry made in AddProducers' configure, as every RMQ page shows it; the gateway first
//            used by the command processor's PostAsync
//   control: the same, but the producer sends once before the command processor is resolved (a startup warm-up)
// The broker's own view of the connection (rabbitmqctl list_connections state) is printed after the alarm, so a
// missing log line cannot be mistaken for a block that never happened. Each arm is its own process. It prints what reached the capturing provider from the RMQ categories, and the
// blocked/unblocked lines in particular.
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Logging;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

var arm = args.Length > 0 ? args[0] : "case";
var capture = new CaptureProvider();                                                   // probe
var services = new ServiceCollection();

// RabbitMQConnectionStability.md #3, as rewritten
services.AddLogging(logging =>
{
    logging.AddConsole();
    // Blocked (Warning) and unblocked (Information) events, connection failures and recoveries
    logging.AddFilter("Paramore.Brighter.MessagingGateway.RMQ", LogLevel.Information);
    logging.AddProvider(capture);                                                      // probe
    logging.AddFilter<CaptureProvider>("Paramore.Brighter.MessagingGateway.RMQ", LogLevel.Debug); // probe: the gateway's Debug lines too
});

var topic = new RoutingKey("probe.ping");                                              // probe
IAmAProducerRegistry? registry = null;                                                 // probe
services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = registry = new RmqProducerRegistryFactory(
            new RmqMessagingGatewayConnection
            {
                AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5679")),
                Exchange = new Exchange("probe.exchange")
            },
            [new RmqPublication { Topic = topic, RequestType = typeof(Ping), MakeChannels = OnMissingChannel.Create }]
        ).Create();
    });

if (arm == "control")
{
    var producer = (IAmAMessageProducerAsync)registry!.LookupBy(topic);
    await producer.SendAsync(new Message(
        new MessageHeader(Id.Random(), topic, MessageType.MT_EVENT), new MessageBody("{}")));
    Console.WriteLine("control: the producer sent once before the command processor was resolved");
}

var provider = services.BuildServiceProvider();
var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();
Console.WriteLine($"ApplicationLogging.LoggerFactory is the container's: {ReferenceEquals(ApplicationLogging.LoggerFactory, provider.GetRequiredService<ILoggerFactory>())}");

await commandProcessor.PostAsync(new Ping());
Rabbitmqctl("set_vm_memory_high_watermark", "0");
_ = commandProcessor.PostAsync(new Ping());          // a publish under the alarm is what blocks the connection; not awaited
await Task.Delay(3000);
Console.WriteLine($"broker: {Rabbitmqctl("list_connections", "state").Trim().Replace('\n', ' ')}");
var blocked = await Wait(r => r.Message.Contains(" blocked."));
Rabbitmqctl("set_vm_memory_high_watermark", "0.4");
var unblocked = await Wait(r => r.Message.Contains(" unblocked"));

Console.WriteLine($"--- {arm}: RMQ records that reached the capturing provider ---");
foreach (var r in capture.Records.Where(r => r.Category.StartsWith("Paramore.Brighter.MessagingGateway.RMQ")))
    Console.WriteLine($"  [{r.Level}] {r.Category.Split('.').Last()}: {r.Message}");
Console.WriteLine($"{arm}: the gateway's own Debug line (Opening channel) reached it = {capture.Records.Any(r => r.Message.Contains("Opening channel"))}");
Console.WriteLine($"{arm}: blocked logged = {blocked}; unblocked logged = {unblocked}");
// the page: lines reach the providers when the command processor is resolved first (case), and none do when a producer
// logged before it was (control)
var expected = arm != "control";
var ok = blocked == expected && unblocked == expected;
Console.WriteLine(ok ? $"ok   {arm}" : $"FAIL {arm}: expected blocked and unblocked logged = {expected}");
return ok ? 0 : 1;

async Task<bool> Wait(Func<(string Category, LogLevel Level, string Message), bool> match)
{
    var clock = Stopwatch.StartNew();
    while (clock.Elapsed < TimeSpan.FromSeconds(15))
    {
        if (capture.Records.Any(match)) return true;
        await Task.Delay(200);
    }
    return false;
}

static string Rabbitmqctl(params string[] arguments)
{
    var psi = new ProcessStartInfo("docker") { RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var a in new[] { "exec", "probe-rmq-logging", "rabbitmqctl" }.Concat(arguments)) psi.ArgumentList.Add(a);
    using var p = Process.Start(psi)!;
    var output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
    return output;
}

public class Ping() : Event(Id.Random());

public sealed class CaptureProvider : ILoggerProvider
{
    public ConcurrentQueue<(string Category, LogLevel Level, string Message)> Records { get; } = new();
    public ILogger CreateLogger(string categoryName) => new CaptureLogger(categoryName, Records);
    public void Dispose() { }

    private sealed class CaptureLogger(string category, ConcurrentQueue<(string, LogLevel, string)> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => records.Enqueue((category, logLevel, formatter(state, exception)));
    }
}
