// PolicyRetryAndCircuitBreaker.md § Type-Scoped Pipelines. The page (and the attribute's own doc
// comment) says that with UseTypePipeline = true, Brighter looks the pipeline up under
// "<handler type full name>.<policy>". ResilienceExceptionPolicyHandler.cs:78 at 10.8.0 calls
// GetPipeline<TRequest>(policy) instead. Each case registers one retry pipeline (one retry, no
// delay) and sends a command whose handler fails once, so the handler running twice proves the
// pipeline was found and applied.
//   case A, the page's form: an untyped pipeline under "<OrderServiceHandler>.SharedCircuitBreaker"
//   case B, the source's form: a pipeline typed for the request, under "SharedCircuitBreaker"
//   control: no UseTypePipeline, an untyped pipeline under "SharedCircuitBreaker"
// Expected at 10.8.0: A throws before the handler runs; B and the control run it twice.
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Policies.Attributes;
using Polly;
using Polly.Registry;
using Polly.Retry;

var failed = 0;
// The page's form is expected to fail: the handler never runs, Send throws
failed += Run("case A   page's key, untyped pipeline", r => r.TryAddBuilder(
    $"{typeof(OrderServiceHandler).FullName}.SharedCircuitBreaker",
    (b, _) => b.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 1, Delay = TimeSpan.Zero })),
    typed: true, expectRuns: 0);
failed += Run("case B   plain key, pipeline typed for the request", r => r.TryAddBuilder<ProcessOrderCommand>(
    "SharedCircuitBreaker",
    (b, _) => b.AddRetry(new RetryStrategyOptions<ProcessOrderCommand> { MaxRetryAttempts = 1, Delay = TimeSpan.Zero })),
    typed: true, expectRuns: 2);
failed += Run("control  UseTypePipeline off, untyped pipeline", r => r.TryAddBuilder(
    "SharedCircuitBreaker",
    (b, _) => b.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 1, Delay = TimeSpan.Zero })),
    typed: false, expectRuns: 2);
// The troubleshooting section's symptoms: what a missing key throws, typed and untyped
failed += Run("symptom  UseTypePipeline off, nothing registered", _ => { }, typed: false, expectRuns: 0);
failed += Run("symptom  UseTypePipeline on, nothing registered", _ => { }, typed: true, expectRuns: 0);
return failed == 0 ? 0 : 1;

static int Run(string name, Action<ResiliencePipelineRegistry<string>> register, bool typed, int expectRuns)
{
    var registry = new ResiliencePipelineRegistry<string>().AddBrighterDefault();
    register(registry);
    var services = new ServiceCollection();
    services.AddBrighter(o => o.ResiliencePipelineRegistry = registry)
        .Handlers(h =>
        {
            if (typed) h.Register<ProcessOrderCommand, OrderServiceHandler>();
            else h.Register<ProcessOrderCommand, PlainHandler>();
        });
    var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    Runs.Count = 0;
    string outcome;
    try { cp.Send(new ProcessOrderCommand()); outcome = $"handler ran {Runs.Count} time(s)"; }
    catch (Exception e)
    {
        outcome = $"{e.GetType().Name}: {e.Message.Split('\n')[0]}";
        for (var inner = e.InnerException; inner != null; inner = inner.InnerException)
            outcome += $" <- {inner.GetType().Name}: {inner.Message.Split('\n')[0]}";
    }
    var ok = Runs.Count == expectRuns;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {outcome}");
    return ok ? 0 : 1;
}

public static class Runs { public static int Count; }

public class ProcessOrderCommand() : Command(Id.Random());

public class OrderServiceHandler : RequestHandler<ProcessOrderCommand>
{
    [UseResiliencePipeline("SharedCircuitBreaker", step: 1, UseTypePipeline = true)]
    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        if (++Runs.Count == 1) throw new InvalidOperationException("first attempt fails");
        return base.Handle(command);
    }
}

public class PlainHandler : RequestHandler<ProcessOrderCommand>
{
    [UseResiliencePipeline("SharedCircuitBreaker", step: 1)]
    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        if (++Runs.Count == 1) throw new InvalidOperationException("first attempt fails");
        return base.Handle(command);
    }
}
