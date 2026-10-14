// Task 3.5, CustomScheduler.md. Claims: a message scheduled through CustomMessageScheduler (block 1)
// is produced when the job fires and CustomSchedulerHandler (block 2) sends its payload; a request
// scheduled through CustomRequestScheduler (block 3) reaches its handler when CustomRequestHandler
// (block 4) sends its payload. Case: schedule, fire. Control: schedule, do not fire.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Scheduler.Events;

var failures = 0;
void Check(string what, bool ok) { Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {what}"); if (!ok) failures++; }

foreach (var fire in new[] { true, false })
{
    var label = fire ? "case   " : "control";
    var bus = new InternalBus();
    var topic = new RoutingKey("greetings");
    var services = new ServiceCollection();
    services.AddSingleton<Seen>();
    services.AddBrighter()
        .AddProducers(o => o.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
            [new Publication { Topic = topic, RequestType = typeof(Greeting) }], InstrumentationOptions.None).Create())
        .AutoFromAssemblies([typeof(Greeting).Assembly]);
    var provider = services.BuildServiceProvider();
    var processor = provider.GetRequiredService<IAmACommandProcessor>();
    var api = new CustomSchedulerAPI();

    // Message: blocks 1 and 2
    var message = new Message(new MessageHeader(Id.Random(), topic, MessageType.MT_COMMAND), new MessageBody("hello"));
    var messageScheduler = new CustomMessageScheduler(api, TimeProvider.System);
    var messageJob = await messageScheduler.ScheduleAsync(message, TimeSpan.FromMinutes(5));
    if (fire) await new CustomSchedulerHandler(processor).ExecuteAsync(api.Jobs[messageJob]);
    var produced = bus.Stream(topic).Any(m => m.Body.Value == "hello");
    Check($"{label} message: produced on '{topic}' = {produced}", produced == fire);

    // Request: blocks 3 and 4
    var requestScheduler = new CustomRequestScheduler(api, TimeProvider.System);
    var greeting = new Greeting { Name = "Ada" };
    var requestJob = requestScheduler.Schedule(greeting, RequestSchedulerType.Send, TimeSpan.FromMinutes(5));
    if (fire) await new CustomRequestHandler(processor).ExecuteAsync(api.Jobs[requestJob]);
    var handled = provider.GetRequiredService<Seen>().Names.Contains("Ada");
    Check($"{label} request: GreetingHandler saw 'Ada' = {handled}", handled == fire);
}
return failures == 0 ? 0 : 1;

public class Greeting() : Command(Id.Random()) { public string Name { get; set; } = ""; }
public class Seen { public List<string> Names { get; } = []; }
public class GreetingHandler(Seen seen) : RequestHandler<Greeting>
{
    public override Greeting Handle(Greeting command) { seen.Names.Add(command.Name); return base.Handle(command); }
}

// The reader's scheduler client, faked: it keeps each job until the probe fires it.
public class CustomSchedulerAPI
{
    public Dictionary<string, CustomSchedulerObject> Jobs { get; } = [];
    public string Schedule(CustomSchedulerObject job) { var id = Guid.NewGuid().ToString(); Jobs[id] = job; return id; }
    public Task<string> ScheduleAsync(CustomSchedulerObject job, CancellationToken cancellationToken) => Task.FromResult(Schedule(job));
    public bool Reschedule(string id, DateTimeOffset at) { if (!Jobs.TryGetValue(id, out var j)) return false; j.At = at; return true; }
    public void Cancel(string id) => Jobs.Remove(id);
}
public class CustomSchedulerObject { public string Data { get; set; } = string.Empty; public DateTimeOffset At { get; set; } }

// ---- CustomScheduler.md block 1, verbatim ----
public class CustomMessageScheduler(CustomSchedulerAPI scheduler, TimeProvider timeProvider)
    : IAmAMessageSchedulerAsync, IAmAMessageSchedulerSync
{
    public string Schedule(Message message, DateTimeOffset at)
        => scheduler.Schedule(Fire(message, async: false, at));

    public string Schedule(Message message, TimeSpan delay)
        => Schedule(message, timeProvider.GetUtcNow() + delay);

    public bool ReScheduler(string schedulerId, DateTimeOffset at)
        => scheduler.Reschedule(schedulerId, at);

    public bool ReScheduler(string schedulerId, TimeSpan delay)
        => ReScheduler(schedulerId, timeProvider.GetUtcNow() + delay);

    public void Cancel(string id) => scheduler.Cancel(id);

    public Task<string> ScheduleAsync(Message message, DateTimeOffset at, CancellationToken cancellationToken = default)
        => scheduler.ScheduleAsync(Fire(message, async: true, at), cancellationToken);

    public Task<string> ScheduleAsync(Message message, TimeSpan delay, CancellationToken cancellationToken = default)
        => ScheduleAsync(message, timeProvider.GetUtcNow() + delay, cancellationToken);

    public Task<bool> ReSchedulerAsync(string schedulerId, DateTimeOffset at, CancellationToken cancellationToken = default)
        => Task.FromResult(ReScheduler(schedulerId, at));

    public Task<bool> ReSchedulerAsync(string schedulerId, TimeSpan delay, CancellationToken cancellationToken = default)
        => Task.FromResult(ReScheduler(schedulerId, delay));

    public Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        Cancel(id);
        return Task.CompletedTask;
    }

    // Async records which call scheduled the message, so the handler fires it the same way
    private static CustomSchedulerObject Fire(Message message, bool async, DateTimeOffset at) => new()
    {
        Data = JsonSerializer.Serialize(
            new FireSchedulerMessage { Message = message, Async = async },
            JsonSerialisationOptions.Options),
        At = at
    };
}

// ---- block 2, verbatim ----
public class CustomSchedulerHandler(IAmACommandProcessor processor)
{
    public async Task ExecuteAsync(CustomSchedulerObject obj)
    {
        var fire = JsonSerializer.Deserialize<FireSchedulerMessage>(obj.Data, JsonSerialisationOptions.Options)!;
        await processor.SendAsync(fire);
    }
}

// ---- block 3, verbatim ----
public class CustomRequestScheduler(CustomSchedulerAPI scheduler, TimeProvider timeProvider)
    : IAmARequestSchedulerAsync, IAmARequestSchedulerSync
{
    public string Schedule<TRequest>(TRequest request, RequestSchedulerType type, DateTimeOffset at)
        where TRequest : class, IRequest
        => scheduler.Schedule(Fire(request, type, async: false, at));

    public string Schedule<TRequest>(TRequest request, RequestSchedulerType type, TimeSpan delay)
        where TRequest : class, IRequest
        => Schedule(request, type, timeProvider.GetUtcNow() + delay);

    public bool ReScheduler(string schedulerId, DateTimeOffset at)
        => scheduler.Reschedule(schedulerId, at);

    public bool ReScheduler(string schedulerId, TimeSpan delay)
        => ReScheduler(schedulerId, timeProvider.GetUtcNow() + delay);

    public void Cancel(string id) => scheduler.Cancel(id);

    public Task<string> ScheduleAsync<TRequest>(TRequest request, RequestSchedulerType type, DateTimeOffset at,
        CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
        => scheduler.ScheduleAsync(Fire(request, type, async: true, at), cancellationToken);

    public Task<string> ScheduleAsync<TRequest>(TRequest request, RequestSchedulerType type, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
        => ScheduleAsync(request, type, timeProvider.GetUtcNow() + delay, cancellationToken);

    public Task<bool> ReSchedulerAsync(string schedulerId, DateTimeOffset at, CancellationToken cancellationToken = default)
        => Task.FromResult(ReScheduler(schedulerId, at));

    public Task<bool> ReSchedulerAsync(string schedulerId, TimeSpan delay, CancellationToken cancellationToken = default)
        => Task.FromResult(ReScheduler(schedulerId, delay));

    public Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        Cancel(id);
        return Task.CompletedTask;
    }

    private static CustomSchedulerObject Fire<TRequest>(TRequest request, RequestSchedulerType type, bool async,
        DateTimeOffset at) => new()
    {
        Data = JsonSerializer.Serialize(
            new FireSchedulerRequest
            {
                SchedulerType = type,
                RequestType = typeof(TRequest).FullName!,
                RequestData = JsonSerializer.Serialize(request, JsonSerialisationOptions.Options),
                Async = async
            },
            JsonSerialisationOptions.Options),
        At = at
    };
}

// ---- block 4, verbatim ----
public class CustomRequestHandler(IAmACommandProcessor processor)
{
    public async Task ExecuteAsync(CustomSchedulerObject obj)
    {
        var fire = JsonSerializer.Deserialize<FireSchedulerRequest>(obj.Data, JsonSerialisationOptions.Options)!;
        await processor.SendAsync(fire);
    }
}
