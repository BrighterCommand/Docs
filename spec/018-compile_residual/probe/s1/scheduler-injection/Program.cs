// Task 3.5. Claim, made by every page that injects IAmAMessageSchedulerAsync or
// IAmARequestSchedulerAsync into a service (QuartzScheduler.md #12, HangfireScheduler.md #14, …):
// the scheduler is in the container to be injected. Brighter 10.8.0 registers it from
// UseScheduler(factory), and from UseScheduler(provider => factory) registers only the factory.
//   case     UseScheduler(provider => new QuartzSchedulerFactory(…)), QuartzScheduler.md block 1's form
//   control  UseScheduler(new InMemorySchedulerFactory()), the instance overload
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Quartz;
using Quartz;

var rows = new (string label, Action<IBrighterBuilder> use)[]
{
    ("case    UseScheduler(provider => new QuartzSchedulerFactory(…))", b => b.UseScheduler(provider =>
    {
        var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
        var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        return new QuartzSchedulerFactory(scheduler);
    })),
    ("control UseScheduler(new InMemorySchedulerFactory())", b => b.UseScheduler(new InMemorySchedulerFactory())),
};
foreach (var (label, use) in rows)
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddQuartz();
    use(services.AddBrighter().AutoFromAssemblies());
    var provider = services.BuildServiceProvider();
    var message = provider.GetService<IAmAMessageSchedulerAsync>() != null;
    var request = provider.GetService<IAmARequestSchedulerAsync>() != null;
    var factory = provider.GetService<IAmAMessageSchedulerFactory>() != null;
    Console.WriteLine($"{label}: factory {factory}, IAmAMessageSchedulerAsync {message}, IAmARequestSchedulerAsync {request}");
}
