// AgreementDispatcher.md #14 (§ AutoFromAssemblies Conflicts), against 10.8.0. A project of its own,
// because AutoFromAssemblies() scans every loaded assembly, and this one must hold only #14's handlers.
//   case:    the page's Solution - excludeDynamicHandlerTypes names the agreement's handlers - Send runs
//            the handler the route returns
//   control: the page's Problem - the same registration without excludeDynamicHandlerTypes - Send throws
//            ArgumentException, "More than one handler was found for the typeof command MyCommand"
//   control: the Solution, with the route returning MyOtherHandler - the exclusion did not unregister it
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var failed = 0;

failed += Run("case     excludeDynamicHandlerTypes, route returns MyHandler", services =>
    // verbatim from the page's block #14
    services.AddBrighter(options => { })
        .AutoFromAssemblies(excludeDynamicHandlerTypes: [typeof(MyHandler), typeof(MyOtherHandler)])
        .Handlers(registry =>
        {
            registry.Register<MyCommand>((request, context) =>
                {
                    // ... your routing logic
                    return [typeof(MyHandler)];
                },
                [typeof(MyHandler), typeof(MyOtherHandler)]);
        }),
    expect: "MyHandler");

failed += Run("control  no exclusion", services =>
    services.AddBrighter(options => { })
        .AutoFromAssemblies()                                                               // probe
        .Handlers(registry =>
        {
            registry.Register<MyCommand>((request, context) => [typeof(MyHandler)],
                [typeof(MyHandler), typeof(MyOtherHandler)]);
        }),
    expect: "throws");

failed += Run("control  excludeDynamicHandlerTypes, route returns MyOtherHandler", services =>
    services.AddBrighter(options => { })
        .AutoFromAssemblies(excludeDynamicHandlerTypes: [typeof(MyHandler), typeof(MyOtherHandler)])
        .Handlers(registry =>
        {
            registry.Register<MyCommand>((request, context) => [typeof(MyOtherHandler)],      // probe
                [typeof(MyHandler), typeof(MyOtherHandler)]);
        }),
    expect: "MyOtherHandler");

return failed == 0 ? 0 : 1;

static int Run(string name, Action<IServiceCollection> configure, string expect)
{
    var services = new ServiceCollection();
    configure(services);
    var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    Ran.Handlers.Clear();
    string outcome;
    try { cp.Send(new MyCommand()); outcome = string.Join(",", Ran.Handlers); }
    catch (Exception e)
    {
        outcome = "throws";
        Console.WriteLine($"     {e.GetType().Name}: {e.Message.Split('\n')[0]}");
    }
    var ok = outcome == expect;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {outcome}");
    return ok ? 0 : 1;
}

public static class Ran { public static readonly List<string> Handlers = []; }

public class MyCommand() : Command(Id.Random());

public class MyHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command) { Ran.Handlers.Add(nameof(MyHandler)); return base.Handle(command); }
}

public class MyOtherHandler : RequestHandler<MyCommand>
{
    public override MyCommand Handle(MyCommand command) { Ran.Handlers.Add(nameof(MyOtherHandler)); return base.Handle(command); }
}
