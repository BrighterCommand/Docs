// Brighter 10.8.1 release claims, each run with a control. Run twice:
//   dotnet run -p:BrighterVersion=10.8.1   (the case)
//   dotnet run -p:BrighterVersion=10.8.0   (the control: the behaviour before the release)
// No broker or database is needed.
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Hangfire;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

string Try(Func<object?> f)
{
    try { var r = f(); return $"ok ({r ?? "null"})"; }
    catch (Exception e) { return e.GetType().Name; }
}

Console.WriteLine($"Brighter {typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion}");

// 1. HangfireScheduler.md: constructing the factory reads nothing; reading Client with no storage throws.
//    Control: with JobStorage.Current set, reading Client works.
Console.WriteLine($"hangfire  construct, no storage : {Try(() => new HangfireMessageSchedulerFactory())}");
Console.WriteLine($"hangfire  read Client, no storage: {Try(() => new HangfireMessageSchedulerFactory().Client.GetType().Name)}");
JobStorage.Current = new MemoryStorage();
Console.WriteLine($"hangfire  read Client, storage   : {Try(() => new HangfireMessageSchedulerFactory().Client.GetType().Name)}");

#if B1081
// 2. RelationalDatabaseConfigurationReference.md: outstandingCountTimeout must be > 0 and <= 2147483 s.
//    Controls: null and in-range values construct. (10.8.0 has no such parameter.)
foreach (var (label, t) in new (string, TimeSpan?)[] {
    ("null", null), ("500 ms", TimeSpan.FromMilliseconds(500)), ("2147483 s", TimeSpan.FromSeconds(2147483)),
    ("0", TimeSpan.Zero), ("-1 s", TimeSpan.FromSeconds(-1)), ("2147484 s", TimeSpan.FromSeconds(2147484)) })
    Console.WriteLine($"outstandingCountTimeout {label,-10}: {Try(() => new RelationalDatabaseConfiguration("Server=x", outstandingCountTimeout: t).OutstandingCountTimeout)}");
#endif

// 3. DispatcherConfigurationReference.md: AddConsumers registers no global Inbox by default, so the same
//    command sent twice reaches its handler twice. Control: InboxConfiguration set -> the second send is
//    stopped by the Inbox (OnceOnlyException). On 10.8.0 the default itself is the control.
int Run(Action<ConsumersOptions> configure)
{
    Counter.Handled = 0;
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddConsumers(configure).Handlers(r => r.Register<Ping, PingHandler>());
    using var sp = services.BuildServiceProvider();
    var cp = sp.GetRequiredService<IAmACommandProcessor>();
    var ping = new Ping();
    cp.Send(ping);
    try { cp.Send(ping); } catch (Exception e) { Console.WriteLine($"          second send threw {e.GetType().Name}"); }
    return Counter.Handled;
}
Console.WriteLine($"inbox     default            : handled {Run(_ => { })} of 2");
Console.WriteLine($"inbox     InboxConfiguration : handled {Run(o => o.InboxConfiguration = new InboxConfiguration())} of 2");

public class Ping() : Command(Id.Random());
public static class Counter { public static int Handled; }
public class PingHandler : RequestHandler<Ping>
{
    public override Ping Handle(Ping command) { Counter.Handled++; return base.Handle(command); }
}
