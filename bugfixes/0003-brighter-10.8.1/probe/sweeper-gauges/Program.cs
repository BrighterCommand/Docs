// Telemetry.md "Outbox Sweeper Metrics": the two sweeper gauges, driven through the real TimedOutboxSweeper.
//   dotnet run -p:BrighterVersion=10.8.1   (the case)
//   dotnet run -p:BrighterVersion=10.8.0   (the control: no outbox_sweeper instrument exists; the third scenario then
//                                           crashes the process, because a failed 10.8.0 sweep is unhandled on the pool)
// A distributed lock that never returns hangs a sweep after it has started; one that returns null makes
// every sweep end at once with lock_unavailable; one that grants the lock makes the sweeper active.
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.Diagnostics;
using Paramore.Brighter.Outbox.Hosting;

Console.WriteLine($"Brighter {typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion}");

async Task Run(string label, IDistributedLock lk, IAmAnOutboxCircuitBreaker? breaker)
{
    var seen = new List<string>();
    var last = new Dictionary<string, double>();
    var recorded = new Dictionary<string, int>();
    using var listener = new MeterListener();
    listener.InstrumentPublished = (i, l) => { if (i.Name.Contains("outbox_sweeper")) { seen.Add($"{i.Name} ({i.GetType().Name.Split('`')[0]}, {i.Unit})"); l.EnableMeasurementEvents(i); } };
    void On(Instrument i, double v)
    {
        if (i.GetType().Name.StartsWith("ObservableGauge")) last[i.Name] = v;
        else recorded[i.Name] = recorded.GetValueOrDefault(i.Name) + 1;
    }
    listener.SetMeasurementEventCallback<double>((i, v, _, _) => On(i, v));
    listener.SetMeasurementEventCallback<long>((i, v, _, _) => On(i, v));
    listener.Start();

    var builder = Host.CreateApplicationBuilder();
    builder.Logging.ClearProviders();
    builder.Services.AddBrighter().UseOutboxSweeper(o => o.TimerInterval = 1);
    builder.Services.RemoveAll<IDistributedLock>();
    builder.Services.AddSingleton(lk);
    if (breaker is not null) builder.Services.AddSingleton(breaker);
    builder.Services.AddOpenTelemetry().WithMetrics(m => m.AddBrighterInstrumentation());
    using var host = builder.Build();
    await host.StartAsync();

    foreach (var t in new[] { 1500, 2000 })
    {
        await Task.Delay(t);
        listener.RecordObservableInstruments();
        Console.WriteLine($"  [{label}] gauges {string.Join(", ", last.Select(kv => $"{kv.Key.Replace("paramore.brighter.outbox_sweeper.", "")}={kv.Value:0.0}"))}; recorded {string.Join(", ", recorded.Select(kv => $"{kv.Key.Replace("paramore.brighter.outbox_sweeper.", "")}x{kv.Value}"))}");
    }
    if (label.StartsWith("hung")) Console.WriteLine($"  instruments: {string.Join("; ", seen.Distinct())}");
    _ = host.StopAsync(new CancellationTokenSource(500).Token);
}

var tripped = new InMemoryOutboxCircuitBreaker();
tripped.TripTopic(new RoutingKey("orders"));

// 1. running_time: a hung sweep shows a growing value and records no tick.lag/sweep.duration/sweeps.
//    Control: lock unavailable -> every sweep ends at once, running_time 0, sweeps recorded.
await Run("hung sweep      ", new HangingLock(), null);
await Run("lock unavailable", new NoLock(), null);
// 2. tripped_topics: the active sweeper (it obtained the lock) reads the breaker -> 1.
//    Controls: not active (lock unavailable) -> 0; breaker whose TrippedTopics throws -> 0.
await Run("active, tripped ", new GrantedLock(), tripped);
await Run("inactive,tripped", new NoLock(), tripped);
await Run("active, throws  ", new GrantedLock(), new ThrowingBreaker());

public class HangingLock : IDistributedLock
{
    public Task<string?> ObtainLockAsync(string resource, CancellationToken ct) => new TaskCompletionSource<string?>().Task;
    public Task ReleaseLockAsync(string resource, string lockId, CancellationToken ct) => Task.CompletedTask;
}
public class NoLock : IDistributedLock
{
    public Task<string?> ObtainLockAsync(string resource, CancellationToken ct) => Task.FromResult<string?>(null);
    public Task ReleaseLockAsync(string resource, string lockId, CancellationToken ct) => Task.CompletedTask;
}
public class GrantedLock : IDistributedLock
{
    public Task<string?> ObtainLockAsync(string resource, CancellationToken ct) => Task.FromResult<string?>("id");
    public Task ReleaseLockAsync(string resource, string lockId, CancellationToken ct) => Task.CompletedTask;
}
public class ThrowingBreaker : IAmAnOutboxCircuitBreaker
{
    public void CoolDown() { }
    public void TripTopic(RoutingKey topic) { }
    public IEnumerable<RoutingKey> TrippedTopics => throw new InvalidOperationException("unreadable");
}
