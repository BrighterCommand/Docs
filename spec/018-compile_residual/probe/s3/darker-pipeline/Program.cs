// Task 5.4, Darker 4.1.1. Each row prints ok or FAIL; the process exits 1 on any FAIL.
// Every container shares one ILoggerFactory, because Darker's decorators cache a static logger
// from ApplicationLogging the first time each generic type is used.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Attributes;
using Paramore.Darker.Exceptions;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using Polly;
using Polly.Registry;

var log = new CaptureProvider();
var loggerFactory = LoggerFactory.Create(b => b.AddProvider(log).SetMinimumLevel(LogLevel.Information));
var failures = 0;
void Row(string name, bool ok, string detail) { Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); if (!ok) failures++; }

IQueryProcessor Build<THandler>(Action<IDarkerHandlerBuilder>? extend = null) where THandler : class, IQueryHandler<GetOrderQuery, OrderData>
{
    var services = new ServiceCollection();
    services.AddSingleton(loggerFactory);
    var darker = services.AddDarker().AddHandlers(r => r.Register<GetOrderQuery, OrderData, THandler>());
    darker.AddJsonQueryLogging();
    extend?.Invoke(darker);
    return services.BuildServiceProvider().GetRequiredService<IQueryProcessor>();
}
async Task<(OrderData? result, Exception? error)> Run(IQueryProcessor qp)
{
    Api.Reset();
    try { return (await qp.ExecuteAsync(new GetOrderQuery(42)), null); }
    catch (Exception ex) { return (null, ex); }
}
string Show((OrderData? result, Exception? error) r) =>
    r.error is not null ? $"{r.error.GetType().Name}: {r.error.Message}" : r.result is null ? "null" : r.result.Status;

// ---- Pitfall 1, QueryPipeline.md: retry at a lower step than fallback, nothing is retried
{
    var bad = await Run(Build<RetryOutsideFallback>(d => d.AddDefaultPolicies()));
    var badCalls = Api.Calls;
    var good = await Run(Build<FallbackOutsideRetry>(d => d.AddDefaultPolicies()));
    Row("pitfall 1, case: [RetryableQuery(1)][FallbackPolicy(2)]", badCalls == 1 && bad.result?.Status == "unavailable",
        $"{badCalls} attempt(s), {Show(bad)}");
    Row("pitfall 1, control: [FallbackPolicy(1)][RetryableQuery(2)]", Api.Calls == 4 && good.result?.Status == "unavailable",
        $"{Api.Calls} attempt(s), {Show(good)}");
}

// ---- Decorator Ordering bullets: what logging at step 1 sees, and logging inside retry
{
    log.Clear();
    var r = await Run(Build<LogFallbackRetry>(d => d.AddDefaultPolicies()));
    Row("ordering, logging outside fallback and retry: one Executing, one completion (with fallback), no per-retry lines",
        Api.Calls == 4 && log.Count("Executing async query") == 1 && log.Count("completed") == 1 && log.Count("(with fallback)") == 1,
        $"{Api.Calls} attempts, {log.Count("Executing async query")} Executing, {log.Count("completed")} completed, {log.Count("(with fallback)")} with fallback, {Show(r)}");

    log.Clear();
    r = await Run(Build<LogRetryNoFallback>(d => d.AddDefaultPolicies()));
    Row("ordering, a query failing after its last retry logs only its Executing line",
        Api.Calls == 4 && log.Count("Executing async query") == 1 && log.Count("completed") == 0 && r.error is not null,
        $"{Api.Calls} attempts, {log.Count("Executing async query")} Executing, {log.Count("completed")} completed, {Show(r)}");

    log.Clear();
    Api.FailFirst = 2;
    var qp = Build<RetryOutsideLogging>(d => d.AddDefaultPolicies());
    Api.Reset(failFirst: 2);
    var ok = await qp.ExecuteAsync(new GetOrderQuery(42));
    Row("ordering, logging inside retry: an Executing line per attempt, a completion only for the one that succeeds",
        Api.Calls == 3 && log.Count("Executing async query") == 3 && log.Count("completed") == 1,
        $"{Api.Calls} attempts, {log.Count("Executing async query")} Executing, {log.Count("completed")} completed, {ok.Status}");

    log.Clear();
    Api.Reset();
    Api.FailFirst = 0;
    var success = await Build<LogFallbackRetry>(d => d.AddDefaultPolicies()).ExecuteAsync(new GetOrderQuery(42));
    Row("ordering, control: a query that succeeds logs its completion without (with fallback)",
        log.Count("completed") == 1 && log.Count("(with fallback)") == 0, $"{log.Count("completed")} completed, {log.Count("(with fallback)")} with fallback, {success.Status}");
}

// ---- Pitfall 2: [RetryableQuery] without AddDefaultPolicies() or AddPolicies()
{
    var noPolicies = await Run(Build<RetryOnly>());
    var withPolicies = await Run(Build<RetryOnly>(d => d.AddDefaultPolicies()));
    Row("pitfall 2, case: no policies configured", noPolicies.error is not null, Show(noPolicies));
    Row("pitfall 2, control: AddDefaultPolicies()", withPolicies.error is null && withPolicies.result?.Status == "ok", Show(withPolicies));
}

// ---- Pitfall 3: a policy name the registry does not hold
{
    IQueryProcessor? qp = null; Exception? buildError = null;
    try { qp = Build<NamedCircuitBreaker>(d => d.AddDefaultPolicies()); } catch (Exception ex) { buildError = ex; }
    var first = qp is null ? (null, buildError) : await Run(qp);
    Row("pitfall 3, case: [RetryableQuery(2, \"MyCircuitBreaker\")], unregistered; the processor builds, the first query throws",
        buildError is null && first.error is ConfigurationException && first.error.Message.StartsWith("Policy does not exist in policy registry"),
        $"build {(buildError is null ? "ok" : buildError.GetType().Name)}; first query {Show(first)}");

    // The page's Good block as written: a registry holding only "MyCircuitBreaker"
    var onlyNamed = new PolicyRegistry { { "MyCircuitBreaker", Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500)) } };
    Exception? goodError = null;
    try { Build<NamedCircuitBreaker>(d => d.AddPolicies(onlyNamed)); } catch (Exception ex) { goodError = ex; }
    Row("pitfall 3, the Good block as written: AddPolicies with only MyCircuitBreaker",
        goodError is ConfigurationException, goodError is null ? "accepted" : $"{goodError.GetType().Name}: {goodError.Message}");

    // The Good block as repaired, verbatim but for the qualified Constants (this file imports QueryLogging too)
    var circuitBreakerPolicy = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));

    var complete = new PolicyRegistry
    {
        { Paramore.Darker.Policies.Constants.RetryPolicyName, Policy.Handle<Exception>().WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(50 * attempt)) },
        { Paramore.Darker.Policies.Constants.CircuitBreakerPolicyName, Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500)) },
        { "MyCircuitBreaker", circuitBreakerPolicy }
    };
    var named = await Run(Build<NamedCircuitBreaker>(d => d.AddPolicies(complete)));
    Row("pitfall 3, control: the registry holding the two defaults and MyCircuitBreaker", named.error is null && named.result?.Status == "ok", Show(named));
}

// ---- Pitfall 4: [FallbackPolicy] without FallbackAsync
{
    var noOverride = await Run(Build<FallbackNoOverride>());
    var withOverride = await Run(Build<FallbackOutsideRetry>(d => d.AddDefaultPolicies()));
    var noAttribute = await Run(Build<NoFallback>());
    Row("pitfall 4, case: [FallbackPolicy] and no FallbackAsync: null, no exception", noOverride.error is null && noOverride.result is null, Show(noOverride));
    Row("pitfall 4, control: FallbackAsync overridden", withOverride.result?.Status == "unavailable", Show(withOverride));
    Row("pitfall 4, control: no [FallbackPolicy]: the exception reaches the caller", noAttribute.error is HttpRequestFailed, Show(noAttribute));
}

// ---- ImplementAQueryHandler.md: each base class's own-mode fallback returns default(TResult); the other mode throws
{
    var sync = new SyncHandler(); var async_ = new AsyncHandler();
    string Try(Func<object?> f) { try { var v = f(); return v is Task t ? Try(() => { t.GetAwaiter().GetResult(); return t.GetType().GetProperty("Result")!.GetValue(t); }) : v is null ? "default" : v.ToString()!; } catch (Exception ex) { return ex.GetType().Name; } }
    var r1 = Try(() => sync.Fallback(new GetOrderQuery(1)));
    var r2 = Try(() => sync.FallbackAsync(new GetOrderQuery(1)));
    var r3 = Try(() => sync.ExecuteAsync(new GetOrderQuery(1)));
    var r4 = Try(() => async_.FallbackAsync(new GetOrderQuery(1)));
    var r5 = Try(() => async_.Fallback(new GetOrderQuery(1)));
    var r6 = Try(() => async_.Execute(new GetOrderQuery(1)));
    Row("QueryHandler: Fallback returns default; FallbackAsync and ExecuteAsync throw NotImplementedException",
        r1 == "default" && r2 == nameof(NotImplementedException) && r3 == nameof(NotImplementedException), $"{r1}, {r2}, {r3}");
    Row("QueryHandlerAsync: FallbackAsync returns default; Fallback and Execute throw NotImplementedException",
        r4 == "default" && r5 == nameof(NotImplementedException) && r6 == nameof(NotImplementedException), $"{r4}, {r5}, {r6}");

    // Through the pipeline, the sync mode: [FallbackPolicy] on a QueryHandler that throws
    var services = new ServiceCollection();
    services.AddSingleton(loggerFactory);
    services.AddDarker().AddHandlers(r => r.Register<GetOrderQuery, OrderData, SyncFallbackHandler>());
    var syncResult = services.BuildServiceProvider().GetRequiredService<IQueryProcessor>().Execute(new GetOrderQuery(1));
    Row("QueryHandler through the pipeline: [FallbackPolicy] with no Fallback override returns default", syncResult is null, syncResult is null ? "null" : syncResult.Status);
}

// ---- The interfaces, read by reflection: IQueryHandler's five members; IQuery and IQuery<T> have none
{
    var members = typeof(IQueryHandler<,>).GetMembers().Concat(typeof(IQueryHandler).GetMembers())
        .Where(m => m is not System.Reflection.MethodInfo { IsSpecialName: true }).Select(m => m.Name).OrderBy(n => n).ToArray();
    Row("IQueryHandler<,> with IQueryHandler: Context, Execute, ExecuteAsync, Fallback, FallbackAsync",
        members.SequenceEqual(new[] { "Context", "Execute", "ExecuteAsync", "Fallback", "FallbackAsync" }), string.Join(", ", members));
    var queryMembers = typeof(IQuery).GetMembers().Length + typeof(IQuery<>).GetMembers().Length;
    Row("IQuery and IQuery<TResult> have no members", queryMembers == 0, $"{queryMembers} members; IQuery<T> : {string.Join(", ", typeof(IQuery<>).GetInterfaces().Select(i => i.Name))}");
    var queryCtl = typeof(IQueryHandler).GetMembers().Length;
    Row("control: the same reflection finds IQueryHandler's member", queryCtl > 0, $"{queryCtl}");
}

Console.WriteLine(failures == 0 ? "all rows ok" : $"{failures} FAIL");
return failures == 0 ? 0 : 1;

public sealed record GetOrderQuery(int OrderId) : IQuery<OrderData>;
public sealed record OrderData(string Status) { public static readonly OrderData Unavailable = new("unavailable"); }
public sealed class HttpRequestFailed() : Exception("the external API failed");

public static class Api
{
    public static int Calls;
    public static int FailFirst = int.MaxValue;
    public static void Reset(int failFirst = int.MaxValue) { Calls = 0; FailFirst = failFirst; }
    public static Task<OrderData> GetOrderAsync() => ++Calls <= FailFirst ? throw new HttpRequestFailed() : Task.FromResult(new OrderData("ok"));
}

public abstract class ThrowingHandler : QueryHandlerAsync<GetOrderQuery, OrderData>
{
    protected static Task<OrderData> Call() => Api.GetOrderAsync();
}
public abstract class WithFallback : ThrowingHandler
{
    public override Task<OrderData> FallbackAsync(GetOrderQuery query, CancellationToken cancellationToken = default) => Task.FromResult(OrderData.Unavailable);
}
public sealed class RetryOutsideFallback : WithFallback
{
    [RetryableQuery(1)] [FallbackPolicy(2)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class FallbackOutsideRetry : WithFallback
{
    [FallbackPolicy(1)] [RetryableQuery(2)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class LogFallbackRetry : WithFallback
{
    [QueryLogging(1)] [FallbackPolicy(2)] [RetryableQuery(3)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class LogRetryNoFallback : ThrowingHandler
{
    [QueryLogging(1)] [RetryableQuery(2)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class RetryOutsideLogging : ThrowingHandler
{
    [RetryableQuery(1)] [QueryLogging(2)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class RetryOnly : ThrowingHandler
{
    [RetryableQuery(1)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) { Api.FailFirst = 0; return await Call(); }
}
public sealed class NamedCircuitBreaker : ThrowingHandler
{
    [RetryableQuery(2, "MyCircuitBreaker")]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) { Api.FailFirst = 0; return await Call(); }
}
public sealed class FallbackNoOverride : ThrowingHandler
{
    [FallbackPolicy(2)]
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class NoFallback : ThrowingHandler
{
    public override async Task<OrderData> ExecuteAsync(GetOrderQuery q, CancellationToken ct = default) => await Call();
}
public sealed class SyncHandler : QueryHandler<GetOrderQuery, OrderData>
{
    public override OrderData Execute(GetOrderQuery query) => new("ok");
}
public sealed class AsyncHandler : QueryHandlerAsync<GetOrderQuery, OrderData>
{
    public override Task<OrderData> ExecuteAsync(GetOrderQuery query, CancellationToken cancellationToken = default) => Task.FromResult(new OrderData("ok"));
}
public sealed class SyncFallbackHandler : QueryHandler<GetOrderQuery, OrderData>
{
    [FallbackPolicy(1)]
    public override OrderData Execute(GetOrderQuery query) => throw new HttpRequestFailed();
}

public sealed class CaptureProvider : ILoggerProvider
{
    private readonly List<string> _lines = new();
    public void Clear() { lock (_lines) _lines.Clear(); }
    public int Count(string fragment) { lock (_lines) return _lines.Count(l => l.Contains(fragment)); }
    public ILogger CreateLogger(string category) => new Capture(this, category);
    public void Dispose() { }
    private sealed class Capture(CaptureProvider owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex, Func<TState, Exception?, string> format)
        {
            if (category.Contains("QueryLoggingDecorator")) lock (owner._lines) owner._lines.Add(format(state, ex));
        }
    }
}
