// Task 7.5. Each page block's registration is copied as the page shows it; the factories, the command and its handler
// are the probe's. The handler records the type of the RequestContext it was given, i.e. which factory made it.
//  CPCR "PostConfigure for Test Overrides": PostConfigure runs after AddBrighter's Configure. Case: the block. Control:
//      no PostConfigure (Brighter's InMemoryRequestContextFactory); and AddBrighter's delegate setting a factory of its
//      own beside the PostConfigure, which PostConfigure overrides.
//  CPCR #10 (Startup): Development reaches DebugRequestContextFactory. Control: Production.
//  CPCR #11 (ParallelTestsA/B): two providers in one process each use their own factory. Control: each other.
//  InMemoryOptions.md "Using PostConfigure for Test Overrides": the same, with AddProducers and an Outbox.
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;

public class Ping() : Command(Id.Random());   // probe
public class PersonCreated() : Event(Id.Random());   // probe
public class PingHandler : RequestHandler<Ping>   // probe
{
    public static string? Seen;
    public override Ping Handle(Ping command) { Seen = Context?.GetType().Name; return base.Handle(command); }
}
public class TestRequestContextFactory : IAmARequestContextFactory { public RequestContext Create() => new TestContext(); }   // probe
public class TestContext : RequestContext { }
public class DebugRequestContextFactory : IAmARequestContextFactory { public RequestContext Create() => new DebugContext(); }   // probe
public class DebugContext : RequestContext { }
public class TestContextFactoryA : IAmARequestContextFactory { public RequestContext Create() => new ContextA(); }   // probe
public class ContextA : RequestContext { }
public class TestContextFactoryB : IAmARequestContextFactory { public RequestContext Create() => new ContextB(); }   // probe
public class ContextB : RequestContext { }
public class OtherFactory : IAmARequestContextFactory { public RequestContext Create() => new OtherContext(); }   // probe
public class OtherContext : RequestContext { }

public class Env(string name) : IWebHostEnvironment   // probe
{
    public string EnvironmentName { get; set; } = name;
    public string ApplicationName { get; set; } = "probe";
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

// CPCR #10, as the page shows it
public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Bind from configuration
        services.Configure<BrighterOptions>(configuration.GetSection("Brighter"));

        // Add Brighter with defaults
        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AutoFromAssemblies([typeof(Startup).Assembly]);   // probe: the assembly named

        // Environment-specific overrides
        if (environment.IsDevelopment())
        {
            services.PostConfigure<BrighterOptions>(options =>
            {
                options.RequestContextFactory = new DebugRequestContextFactory();
            });
        }
    }
}

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static string Run(IServiceProvider provider)
    {
        PingHandler.Seen = null;
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IAmACommandProcessor>().Send(new Ping());
        return PingHandler.Seen ?? "(handler not run)";
    }

    public static int Main()
    {
        // CPCR "PostConfigure for Test Overrides"
        {
            var services = new ServiceCollection();
            services.AddBrighter(options => { options.HandlerLifetime = ServiceLifetime.Scoped; })
                .AutoFromAssemblies([typeof(Program).Assembly]);
            services.PostConfigure<BrighterOptions>(options => { options.RequestContextFactory = new TestRequestContextFactory(); });
            var seen = Run(services.BuildServiceProvider());
            Row("CPCR PostConfigure for Test Overrides", seen == nameof(TestContext), seen);

            var control = new ServiceCollection();
            control.AddBrighter(options => { options.HandlerLifetime = ServiceLifetime.Scoped; })
                .AutoFromAssemblies([typeof(Program).Assembly]);
            Console.WriteLine($"     control, no PostConfigure: {Run(control.BuildServiceProvider())}");

            var both = new ServiceCollection();
            both.AddBrighter(options => { options.RequestContextFactory = new OtherFactory(); })
                .AutoFromAssemblies([typeof(Program).Assembly]);
            both.PostConfigure<BrighterOptions>(options => { options.RequestContextFactory = new TestRequestContextFactory(); });
            var b = Run(both.BuildServiceProvider());
            Row("CPCR, PostConfigure runs after AddBrighter's own delegate", b == nameof(TestContext), $"{b} (AddBrighter set OtherFactory)");
        }

        // CPCR #10
        foreach (var env in new[] { Environments.Development, Environments.Production })
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                { ["Brighter:HandlerLifetime"] = "Transient" }).Build();   // probe: a section to bind
            var services = new ServiceCollection();
            new Startup(configuration, new Env(env)).ConfigureServices(services);
            var provider = services.BuildServiceProvider();
            var seen = Run(provider);
            if (env == Environments.Development)
                Row("CPCR #10, Development", seen == nameof(DebugContext), seen);
            else
                Console.WriteLine($"     control, Production: {seen}");
        }

        // CPCR #11
        {
            IServiceProvider Build(IAmARequestContextFactory f)
            {
                var services = new ServiceCollection();
                services.AddBrighter(options => { }).AutoFromAssemblies([typeof(Program).Assembly]);
                services.PostConfigure<BrighterOptions>(o => o.RequestContextFactory = f);
                return services.BuildServiceProvider();
            }
            var a = Build(new TestContextFactoryA());
            var bp = Build(new TestContextFactoryB());
            var ra = Run(a); var rb = Run(bp); var ra2 = Run(a);
            Row("CPCR #11, two providers, each its own factory", ra == nameof(ContextA) && rb == nameof(ContextB) && ra2 == nameof(ContextA), $"A {ra}, B {rb}, A again {ra2}");
        }

        // InMemoryOptions.md
        {
            var services = new ServiceCollection();
            var internalBus = new InternalBus();

            services.AddBrighter(options =>
            {
                options.HandlerLifetime = ServiceLifetime.Scoped;
            })
            .AddProducers(options =>
            {
                options.ProducerRegistry = new InMemoryProducerRegistryFactory(
                    internalBus,
                    new[] { new Publication { Topic = new RoutingKey("PersonCreated"), RequestType = typeof(PersonCreated) } },
                    InstrumentationOptions.All
                ).Create();
                options.Outbox = new InMemoryOutbox(TimeProvider.System);
            })
            .AutoFromAssemblies([typeof(Program).Assembly]);

            // Override specific options for this test
            services.PostConfigure<BrighterOptions>(options =>
            {
                options.RequestContextFactory = new TestRequestContextFactory();
            });
            var seen = Run(services.BuildServiceProvider());
            Row("InMemoryOptions.md PostConfigure", seen == nameof(TestContext), seen);
        }
        return failed ? 1 : 0;
    }
}
