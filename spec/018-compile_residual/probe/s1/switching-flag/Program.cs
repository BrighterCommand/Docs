// Task 3.5, SwitchingSchedulers.md block 7. Claim: the application chooses its scheduler at startup
// from a flag, so the change "can be rolled forward and back without a redeployment". Case:
// UseQuartz = true, and Brighter's message and request scheduler factories are Quartz's. Control:
// UseQuartz = false, and they are Hangfire's. Both resolved from the provider Brighter builds.
using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Hangfire;
using Paramore.Brighter.MessageScheduler.Quartz;
using Quartz;

var failures = 0;
foreach (var (flag, expected) in new[] { ("true", typeof(QuartzSchedulerFactory)), ("false", typeof(HangfireMessageSchedulerFactory)) })
{
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["UseQuartz"] = flag }).Build();
    var services = new ServiceCollection();
    services.AddLogging();  // the host's, which Quartz requires
    services.AddQuartz();   // the Quartz the block's lambda resolves ISchedulerFactory from
    services.AddHangfire(c => c.UseMemoryStorage());  // and the Hangfire, as block 3 registers it

    // ---- SwitchingSchedulers.md block 7, verbatim ----
    // Run both schedulers temporarily
    var brighter = services.AddBrighter(options =>
        {
            // ...
        })
        .AutoFromAssemblies();

    // Choose based on feature flag or configuration
    if (configuration.GetValue<bool>("UseQuartz"))
    {
        brighter.UseScheduler(provider =>
        {
            var factory = provider.GetRequiredService<ISchedulerFactory>();
            return new QuartzSchedulerFactory(factory.GetScheduler().GetAwaiter().GetResult());
        });
    }
    else
    {
        brighter.UseScheduler(provider =>
        {
            // Resolve Hangfire's client first: that initialises its storage (Brighter#4600)
            var client = provider.GetRequiredService<IBackgroundJobClientV2>();
            return new HangfireMessageSchedulerFactory { Client = client };
        });
    }
    // ---- end of block 7 ----

    var provider = services.BuildServiceProvider();
    var message = provider.GetRequiredService<IAmAMessageSchedulerFactory>().GetType();
    var request = provider.GetRequiredService<IAmARequestSchedulerFactory>().GetType();
    var ok = message == expected && request == expected;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {(flag == "true" ? "case   " : "control")} UseQuartz={flag}: message {message.Name}, request {request.Name}");
    if (!ok) failures++;
}
return failures == 0 ? 0 : 1;
