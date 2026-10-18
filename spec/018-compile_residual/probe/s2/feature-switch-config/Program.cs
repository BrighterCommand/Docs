// FeatureSwitches.md, against 10.8.0. Every handler is [FeatureSwitch(..., FeatureSwitchStatus.Config)],
// so the registry decides whether it runs.
//   #6: ConfigurationFeatureSwitchRegistry reads appsettings.json's FeatureSwitches section, keyed by the
//       handler's full (dotted) type name, and falls back to MissingConfigStrategy when there is no key
//       case:    "MyNamespace.MyFeatureSwitchedHandler": "on" - runs
//       control: "MyNamespace.MyIncompleteHandler": "off" - skipped
//       case:    no key, MissingConfigStrategy.SilentOff - skipped
//       control: no key, MissingConfigStrategy.SilentOn - runs
//       case:    no key, the default MissingConfigStrategy.Exception - Send throws
//   § the opening paragraph: "With no registry at all, FeatureSwitchStatus.Config behaves as On.
//       With a registry that has no entry for the handler, its MissingConfigStrategy decides; the
//       FluentConfigRegistryBuilder's default throws a ConfigurationException."
//       case:    no registry - runs
//       case:    FluentConfigRegistryBuilder with no entry for the handler - Send throws
//                ConfigurationException
//       control: FluentConfigRegistryBuilder with the handler On - runs
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.FeatureSwitch;
using Paramore.Brighter.FeatureSwitch.Providers;
using MyNamespace;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var failed = 0;
failed += Run<MyFeatureSwitchedHandler>("#6 case     key \"on\"", new ConfigurationFeatureSwitchRegistry(configuration), "runs");
failed += Run<MyIncompleteHandler>("#6 control  key \"off\"", new ConfigurationFeatureSwitchRegistry(configuration), "skipped");
failed += Run<MyUnlistedHandler>("#6 case     no key, SilentOff", new ConfigurationFeatureSwitchRegistry(configuration) { MissingConfigStrategy = MissingConfigStrategy.SilentOff }, "skipped");
failed += Run<MyUnlistedHandler>("#6 control  no key, SilentOn", new ConfigurationFeatureSwitchRegistry(configuration) { MissingConfigStrategy = MissingConfigStrategy.SilentOn }, "runs");
failed += Run<MyUnlistedHandler>("#6 case     no key, Exception (default)", new ConfigurationFeatureSwitchRegistry(configuration), "throws InvalidOperationException");
failed += Run<MyUnlistedHandler>("intro case     no registry", null, "runs");
failed += Run<MyUnlistedHandler>("intro case     Fluent registry, no entry", FluentConfigRegistryBuilder.With().StatusOf<MyFeatureSwitchedHandler>().Is(FeatureSwitchStatus.On).Build(), "throws ConfigurationException");
failed += Run<MyUnlistedHandler>("intro control  Fluent registry, entry On", FluentConfigRegistryBuilder.With().StatusOf<MyUnlistedHandler>().Is(FeatureSwitchStatus.On).Build(), "runs");
return failed == 0 ? 0 : 1;

static int Run<THandler>(string name, IAmAFeatureSwitchRegistry? registry, string expect) where THandler : class, IHandleRequests<MyCommand>
{
    var services = new ServiceCollection();
    services.AddBrighter(o => o.FeatureSwitchRegistry = registry)
        .Handlers(h => h.Register<MyCommand, THandler>());
    var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    Runs.Count = 0;
    string outcome;
    try { cp.Send(new MyCommand()); outcome = Runs.Count == 1 ? "runs" : "skipped"; }
    catch (Exception e)
    {
        Console.WriteLine($"     Send threw {e.GetType().Name}");
        while (e.InnerException != null && e is not InvalidOperationException && e is not ConfigurationException) e = e.InnerException;
        outcome = $"throws {e.GetType().Name}";
        Console.WriteLine($"     {e.Message.Split('\n')[0]}");
    }
    var ok = outcome == expect;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {outcome}");
    return ok ? 0 : 1;
}

// verbatim from the page's block #6
public class ConfigurationFeatureSwitchRegistry(IConfiguration configuration) : IAmAFeatureSwitchRegistry
{
    public MissingConfigStrategy MissingConfigStrategy { get; set; } = MissingConfigStrategy.Exception;

    public FeatureSwitchStatus StatusOf(Type handler)
    {
        var configStatus = configuration[$"FeatureSwitches:{handler.FullName}"]?.ToLowerInvariant();

        return configStatus switch
        {
            "on" => FeatureSwitchStatus.On,
            "off" => FeatureSwitchStatus.Off,
            _ => MissingConfigStrategy switch
            {
                MissingConfigStrategy.SilentOn => FeatureSwitchStatus.On,
                MissingConfigStrategy.SilentOff => FeatureSwitchStatus.Off,
                _ => throw new InvalidOperationException($"No Feature Switch configuration for {handler} specified")
            }
        };
    }
}

namespace MyNamespace
{
    using Paramore.Brighter.FeatureSwitch.Attributes;

    public static class Runs { public static int Count; }

    public class MyCommand() : Command(Id.Random());

    public class MyFeatureSwitchedHandler : RequestHandler<MyCommand>
    {
        [FeatureSwitch(typeof(MyFeatureSwitchedHandler), FeatureSwitchStatus.Config, step: 1)]
        public override MyCommand Handle(MyCommand command) { Runs.Count++; return base.Handle(command); }
    }

    public class MyIncompleteHandler : RequestHandler<MyCommand>
    {
        [FeatureSwitch(typeof(MyIncompleteHandler), FeatureSwitchStatus.Config, step: 1)]
        public override MyCommand Handle(MyCommand command) { Runs.Count++; return base.Handle(command); }
    }

    public class MyUnlistedHandler : RequestHandler<MyCommand>
    {
        [FeatureSwitch(typeof(MyUnlistedHandler), FeatureSwitchStatus.Config, step: 1)]
        public override MyCommand Handle(MyCommand command) { Runs.Count++; return base.Handle(command); }
    }
}
