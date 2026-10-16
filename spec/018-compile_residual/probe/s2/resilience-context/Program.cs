// PolicyRetryAndCircuitBreaker.md #15 and UsingTheContextBag.md #10, against 10.8.0. Each case registers
// the page's MyContextAwarePipeline (retry, 3 attempts; the probe drops the default delay) with an
// OnRetry that records what it reads under "tenant-id", and sends a command whose handler fails once.
//   #15 "When you set RequestContext.ResilienceContext before you send, Brighter runs the handler's
//       resilience pipeline with that context, so a callback such as OnRetry can read the properties"
//       case:    the page's send, a ResilienceContext holding "tenant-42" - OnRetry reads tenant-42
//       control: the same send without a RequestContext - OnRetry reads nothing
//   #10 "The ResilienceContext can be used to share data across different resilience policies": the
//       handler sets "tenant-id" on Context.ResilienceContext when it is not null
//       case:    the caller attaches an empty ResilienceContext - OnRetry reads the handler's value
//       control: the caller attaches none - Context.ResilienceContext is null, so the handler sets
//                nothing and OnRetry reads nothing
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

failed += Run<ContextAwareHandler>("#15 case     ResilienceContext attached, holding tenant-42", attach: true, preset: "tenant-42", expect: "tenant-42");
failed += Run<ContextAwareHandler>("#15 control  no RequestContext", attach: false, preset: null, expect: "(none)");
failed += Run<ResilientHandler>("#10 case     empty ResilienceContext attached", attach: true, preset: null, expect: "from-handler");
failed += Run<ResilientHandler>("#10 control  no RequestContext", attach: false, preset: null, expect: "(none)");
return failed == 0 ? 0 : 1;

static int Run<THandler>(string name, bool attach, string? preset, string expect) where THandler : class, IHandleRequests<MyCommand>
{
    var seen = "(none)";
    var resiliencePipelineRegistry = new ResiliencePipelineRegistry<string>().AddBrighterDefault();

    // verbatim from the page's block #15, but for the delay and the recorded value
    var tenantKey = new ResiliencePropertyKey<string>("tenant-id");

    resiliencePipelineRegistry.TryAddBuilder("MyContextAwarePipeline",
        (builder, context) => builder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.Zero,                                                      // probe
            OnRetry = args =>
            {
                // Read what the caller put into Polly's resilience context
                if (args.Context.Properties.TryGetValue(tenantKey, out var tenantId))
                {
                    // Use the value for logging, tracing, etc.
                    seen = tenantId;                                                    // probe
                }
                return default;
            }
        }));

    var services = new ServiceCollection();
    services.AddBrighter(o => o.ResiliencePipelineRegistry = resiliencePipelineRegistry)
        .Handlers(h => h.Register<MyCommand, THandler>());
    var commandProcessor = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    Runs.Count = 0;

    if (attach)
    {
        // When you send, attach the resilience context to the request context
        var resilienceContext = ResilienceContextPool.Shared.Get();
        if (preset != null) resilienceContext.Properties.Set(tenantKey, preset);

        commandProcessor.Send(new MyCommand(), new RequestContext { ResilienceContext = resilienceContext });

        ResilienceContextPool.Shared.Return(resilienceContext);
    }
    else
    {
        commandProcessor.Send(new MyCommand());
    }

    var ok = seen == expect && Runs.Count == 2;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: handler ran {Runs.Count} time(s), OnRetry read {seen}");
    return ok ? 0 : 1;
}

public static class Runs { public static int Count; }

public class MyCommand() : Command(Id.Random())
{
    public string TenantId { get; set; } = "from-handler";
}

public class ContextAwareHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline("MyContextAwarePipeline", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        if (++Runs.Count == 1) throw new InvalidOperationException("first attempt fails");
        return base.Handle(command);
    }
}

// UsingTheContextBag.md #10, verbatim but for the attribute and the failure, which the probe adds
public class ResilientHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline("MyContextAwarePipeline", step: 1)]                          // probe
    public override MyCommand Handle(MyCommand command)
    {
        // Access Polly resilience context
        if (Context.ResilienceContext != null)
        {
            Context.ResilienceContext.Properties.Set(
                new ResiliencePropertyKey<string>("tenant-id"),
                command.TenantId
            );

            Context.ResilienceContext.Properties.Set(
                new ResiliencePropertyKey<int>("retry-count"),
                0
            );
        }

        if (++Runs.Count == 1) throw new InvalidOperationException("first attempt fails"); // probe
        return base.Handle(command);
    }
}
