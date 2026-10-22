// Brighter 10.8.1: [UseInbox] on a handler when AddConsumers sets no InboxConfiguration (the new default).
// Case: no InboxConfiguration, through both AddConsumers overloads. Control: InboxConfiguration set.
// Run with -p:BrighterVersion=10.8.1 and -p:BrighterVersion=10.8.0 (where the default registered an InMemoryInbox).
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

Console.WriteLine($"Brighter {typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion.Split('+')[0]}");

string Run(Func<IServiceCollection, IBrighterBuilder> addConsumers)
{
    Counter.Handled = 0;
    var services = new ServiceCollection();
    services.AddLogging();
    addConsumers(services).Handlers(r => r.Register<Ping, PingHandler>());
    using var sp = services.BuildServiceProvider();
    var outcome = new List<string>();
    try
    {
        var cp = sp.GetRequiredService<IAmACommandProcessor>();
        var ping = new Ping();
        for (var i = 0; i < 2; i++)
            try { cp.Send(ping); outcome.Add("sent"); }
            catch (Exception e) { outcome.Add(e.GetType().Name + (e.InnerException is null ? "" : $"<{e.InnerException.GetType().Name}>")); if (Environment.GetEnvironmentVariable("VERBOSE") is not null) Console.WriteLine($"    {e.Message} | {e.InnerException?.Message}\n    at {e.StackTrace?.Split("\n").FirstOrDefault()?.Trim()}"); }
    }
    catch (Exception e) { outcome.Add("resolve: " + e.GetType().Name); }
    return $"{string.Join(", ", outcome)}; handled {Counter.Handled}";
}

Console.WriteLine($"Action overload, no InboxConfiguration : {Run(s => s.AddConsumers(_ => { }))}");
Console.WriteLine($"Action overload, InboxConfiguration    : {Run(s => s.AddConsumers(o => o.InboxConfiguration = new InboxConfiguration()))}");
Console.WriteLine($"Func overload,   no InboxConfiguration : {Run(s => s.AddConsumers(_ => new ConsumersOptions()))}");
Console.WriteLine($"Func overload,   InboxConfiguration    : {Run(s => s.AddConsumers(_ => new ConsumersOptions { InboxConfiguration = new InboxConfiguration() }))}");

public class Ping() : Command(Id.Random());
public static class Counter { public static int Handled; }
public class PingHandler : RequestHandler<Ping>
{
    [UseInbox(step: 0, contextKey: typeof(PingHandler), onceOnly: true)]
    public override Ping Handle(Ping command) { Counter.Handled++; return base.Handle(command); }
}
