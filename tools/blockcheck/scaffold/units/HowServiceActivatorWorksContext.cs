// Types and values HowServiceActivatorWorks.md names in its blocks and never declares.
//
// The page's subscriptions and handlers carry a `MyCommand` and two task events it never shows,
// register routes on a registry and health checks on a service collection it never creates. Each
// stub carries only the members the page's blocks use.
//
// blockcheck: using static HowServiceActivatorWorksContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class HowServiceActivatorWorksContext
{
    // block 9: `registry.RegisterAsync<MyCommand>(…)`
    public static IAmAnAsyncSubcriberRegistry registry => null!;
    // block 12: `services.AddHealthChecks()`
    public static IServiceCollection services => null!;
}

// blocks 9, 10, 16: `Subscription<MyCommand>`, `RequestHandlerAsync<MyCommand>`; block 9: `myCommand?.Priority`
public class MyCommand() : Command(Id.Random())
{
    public string Priority { get; set; } = "";
}

// block 8: `typeof(TaskCreated)`, `typeof(TaskUpdated)`
public class TaskCreated() : Event(Id.Random());

public class TaskUpdated() : Event(Id.Random());

// block 9: `typeof(HighPriorityHandlerAsync)`, `typeof(StandardHandlerAsync)`
public class HighPriorityHandlerAsync : RequestHandlerAsync<MyCommand>;

public class StandardHandlerAsync : RequestHandlerAsync<MyCommand>;
