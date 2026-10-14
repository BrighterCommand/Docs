// Task 3.5, QuartzScheduler.md. Claims:
//   block 2   clustering is enabled by store.UseClustering(…) inside UsePersistentStore
//   block 10  q.MisfireThreshold sets the misfire threshold
//   block 8   GetOrCreateSchedulerId / GetOrCreateRequestSchedulerId decide the scheduler id
// Cases: the property each block sets, read back from QuartzOptions; for block 8, the id a schedule
// returns. Controls: the same configuration without the call (property absent), and the default
// factory (a fresh id, not the message's).
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessageScheduler.Quartz;
using Quartz;

var failures = 0;
void Check(string what, bool ok) { Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {what}"); if (!ok) failures++; }
QuartzOptions Options(Action<IServiceCollectionQuartzConfigurator> configure)
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddQuartz(configure);
    return services.BuildServiceProvider().GetRequiredService<IOptions<QuartzOptions>>().Value;
}
var connectionString = "Server=localhost;Database=Quartz";

// block 2's store, as the page writes it
var clustered = Options(q => q.UsePersistentStore(store =>
{
    store.UseSqlServer(sqlServer => { sqlServer.ConnectionString = connectionString; sqlServer.TablePrefix = "QRTZ_"; });
    store.UseNewtonsoftJsonSerializer();  // Use JSON serialization for job data

    // Optional: Enable clustering for HA
    store.UseClustering(clustering =>
    {
        clustering.CheckinInterval = TimeSpan.FromSeconds(20);
        clustering.CheckinMisfireThreshold = TimeSpan.FromSeconds(30);
    });
}));
var unclustered = Options(q => q.UsePersistentStore(store =>
{
    store.UseSqlServer(sqlServer => { sqlServer.ConnectionString = connectionString; sqlServer.TablePrefix = "QRTZ_"; });
    store.UseNewtonsoftJsonSerializer();
}));
string? Get(QuartzOptions o, string key) => o.TryGetValue(key, out var v) ? v : null;
Check($"case    block 2: quartz.jobStore.clustered = {Get(clustered, "quartz.jobStore.clustered") ?? "(absent)"}, checkin {Get(clustered, "quartz.jobStore.clusterCheckinInterval") ?? "(absent)"}",
    Get(clustered, "quartz.jobStore.clustered") == "true");
Check($"control block 2 without UseClustering: quartz.jobStore.clustered = {Get(unclustered, "quartz.jobStore.clustered") ?? "(absent)"}",
    Get(unclustered, "quartz.jobStore.clustered") != "true");

// block 10
var misfire = Options(q => { q.MisfireThreshold = TimeSpan.FromMinutes(1); });
var noMisfire = Options(q => { });
Check($"case    block 10: quartz.jobStore.misfireThreshold = {Get(misfire, "quartz.jobStore.misfireThreshold") ?? "(absent)"}",
    Get(misfire, "quartz.jobStore.misfireThreshold") == "60000");
Check($"control block 10 without it: quartz.jobStore.misfireThreshold = {Get(noMisfire, "quartz.jobStore.misfireThreshold") ?? "(absent)"}",
    Get(noMisfire, "quartz.jobStore.misfireThreshold") != "60000");

// block 8, against Quartz's in-memory store
foreach (var custom in new[] { true, false })
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddQuartz();
    var builder = services.AddBrighter(options =>
    {
        // ...
    });
    if (custom)
    {
        // ---- block 8's UseScheduler, verbatim ----
        builder.UseScheduler(provider =>
        {
            var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
            var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

            return new QuartzSchedulerFactory(scheduler)
            {
                // Customize message scheduler ID
                GetOrCreateSchedulerId = message => message.Id.Value,

                // Customize request scheduler ID
                GetOrCreateRequestSchedulerId = request =>
                {
                    if (request is MyCommand command)
                    {
                        return command.CorrelationId.ToString();
                    }
                    return request.Id.ToString();
                }
            };
        });
        // ---- end ----
    }
    else
    {
        builder.UseScheduler(provider => new QuartzSchedulerFactory(
            provider.GetRequiredService<ISchedulerFactory>().GetScheduler().GetAwaiter().GetResult()));
    }
    var sp = services.BuildServiceProvider();
    var message = new Message(new MessageHeader(Id.Random(), new RoutingKey("t"), MessageType.MT_COMMAND), new MessageBody("x"));
    var messageId = await sp.GetRequiredService<IAmAMessageSchedulerAsync>().ScheduleAsync(message, TimeSpan.FromHours(1));
    var command = new MyCommand { CorrelationId = Id.Random() };
    var requestId = await sp.GetRequiredService<IAmARequestSchedulerAsync>().ScheduleAsync(command, RequestSchedulerType.Send, TimeSpan.FromHours(1));
    var label = custom ? "case    block 8" : "control default factory";
    Check($"{label}: message id is the message's = {messageId == message.Id.Value}, request id is the correlation id = {requestId == command.CorrelationId.ToString()}",
        (messageId == message.Id.Value) == custom && (requestId == command.CorrelationId.ToString()) == custom);
}
return failures == 0 ? 0 : 1;

public class MyCommand() : Command(Id.Random());
