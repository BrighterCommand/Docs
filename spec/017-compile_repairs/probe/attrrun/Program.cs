using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Policies.Attributes;
using Polly;
using Polly.Registry;
using Paramore.Brighter.Extensions;

public class Mismatched : Command { public Mismatched() : base(Id.Random()) { } }
public class Matched : Command { public Matched() : base(Id.Random()) { } }

public class MismatchedHandler : RequestHandlerAsync<Mismatched>
{
    [UseResiliencePipeline("retry", step: 1)]
    public override async Task<Mismatched> HandleAsync(Mismatched c, CancellationToken ct = default)
    { Console.WriteLine("  handler ran"); return await base.HandleAsync(c, ct); }
}
public class MatchedHandler : RequestHandlerAsync<Matched>
{
    [UseResiliencePipelineAsync("retry", step: 1)]
    public override async Task<Matched> HandleAsync(Matched c, CancellationToken ct = default)
    { Console.WriteLine("  handler ran"); return await base.HandleAsync(c, ct); }
}

public static class Program
{
    public static async Task Main()
    {
        var registry = new ResiliencePipelineRegistry<string>().AddBrighterDefault();
        registry.TryAddBuilder("retry", (b, _) => b.AddRetry(new Polly.Retry.RetryStrategyOptions()));
        var services = new ServiceCollection();
        services.AddBrighter(o => o.ResiliencePipelineRegistry = registry)
            .AutoFromAssemblies([typeof(Program).Assembly]);
        var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
        foreach (var (name, send) in new (string, Func<Task>)[] {
            ("control: UseResiliencePipelineAsync on HandleAsync", () => cp.SendAsync(new Matched())),
            ("case:    UseResiliencePipeline on HandleAsync", () => cp.SendAsync(new Mismatched())) })
        {
            Console.WriteLine(name);
            try { await send(); Console.WriteLine("  -> completed"); }
            catch (Exception e) { Console.WriteLine($"  -> {e.GetType().Name}: {e.Message.Split('\n')[0]}"); }
        }
    }
}
