// Task 3.5, HangfireScheduler.md block 1 and every page that registers Brighter's Hangfire
// scheduler the same way. Claim (block 1): with AddHangfire(...) first, then
// .UseScheduler(new HangfireMessageSchedulerFactory()), the application starts and Brighter
// schedules through Hangfire.
//   control  the page's form: `new HangfireMessageSchedulerFactory()` while configuring services
//   case     the repair: the factory built in UseScheduler's provider lambda, after resolving
//            the Client Hangfire registers, which initialises Hangfire's storage. Then a request is scheduled, and Hangfire's storage holds one
//            scheduled job.
using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Hangfire;

var failures = 0;

// control
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddHangfire(configuration => configuration.UseMemoryStorage());
    string outcome;
    try
    {
        services.AddBrighter().UseScheduler(new HangfireMessageSchedulerFactory()).AutoFromAssemblies();
        outcome = "configured";
    }
    catch (Exception e) { outcome = e.GetType().Name; }
    var ok = outcome == nameof(InvalidOperationException);
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} control `new HangfireMessageSchedulerFactory()` while configuring: {outcome}");
    if (!ok) failures++;
}

// case
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddHangfire(configuration => configuration.UseMemoryStorage());
    services.AddBrighter()
        .UseScheduler(provider =>
        {
            // Resolving Hangfire's client first initialises its storage
            var client = provider.GetRequiredService<IBackgroundJobClientV2>();
            return new HangfireMessageSchedulerFactory { Client = client };
        })
        .AutoFromAssemblies();
    var provider = services.BuildServiceProvider();
    string outcome; long scheduled = -1;
    try
    {
        var processor = provider.GetRequiredService<IAmACommandProcessor>();
        await processor.SendAsync(TimeSpan.FromMinutes(5), new Ping());
        scheduled = provider.GetRequiredService<JobStorage>().GetMonitoringApi().ScheduledCount();
        outcome = "scheduled";
    }
    catch (Exception e) { outcome = $"{e.GetType().Name}: {e.Message.Split('\n')[0]}"; }
    var ok = scheduled == 1;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} case    the factory built in UseScheduler's provider lambda: {outcome}, Hangfire scheduled jobs = {scheduled}");
    if (!ok) failures++;
}
return failures == 0 ? 0 : 1;

public class Ping() : Command(Id.Random());
public class PingHandler : RequestHandlerAsync<Ping>;
