// Types and values V10MigrationGuide.md names in its blocks and never declares.
//
// The guide shows the V9 form and the V10 form of the reader's own code, and the reader's events,
// commands and registrations are not on the page. Every value member is typed from a pinned
// package or the BCL and returns a default; a stub is a request with only the members a block
// names. `PersonCreated` and `MyCommand` are declared by blocks on the page, so no stub stands in
// for them.
//
// blockcheck: using static V10MigrationGuideContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Polly.Registry;

public static class V10MigrationGuideContext
{
    // blocks 11, 21: `services.AddBrighter(…)`, `services.AddConsumers(…)`
    public static IServiceCollection services => null!;
    // block 11: `options.ResiliencePipelineRegistry = resiliencePipelineRegistry;`
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
    // block 21: `options.Subscriptions = subscriptions;`
    public static Subscription[] subscriptions => null!;
    // block 26: `_commandProcessor.SendAsync(new TestCommand { Value = i })`
    public static IAmACommandProcessor _commandProcessor => null!;
}

// blocks 5, 7: `new RmqPublication<MyEvent>(…)`, `new Subscription<MyEvent>(…)`
public class MyEvent() : Event(Id.Random());

// block 13: `PlaceOrderAsync(OrderPlaced orderPlaced)`, `orderPlaced.TenantId`
public class OrderPlaced() : Event(Id.Random())
{
    public string TenantId { get; set; } = string.Empty;
}

// block 20: `typeof(TaskCreated)`, `typeof(TaskUpdated)`, `typeof(TaskDeleted)`
public class TaskCreated() : Event(Id.Random());
public class TaskUpdated() : Event(Id.Random());
public class TaskDeleted() : Event(Id.Random());

// blocks 21–24: `typeof(GreetingEvent)`, `KafkaSubscription<GreetingEvent>`
public class GreetingEvent() : Event(Id.Random());

// block 26: `new TestCommand { Value = i }`
public class TestCommand() : Command(Id.Random())
{
    public int Value { get; set; }
}
