// Types and values InMemoryTransport.md names in its blocks and never declares.
//
// The producer and the complete example publish and subscribe to a `GreetingMade` the page never shows. Every value member is typed
// from a pinned package or the BCL, returns a default, and does nothing. A block that calls a
// member of one of these is checked against the real type, so a wrong member or argument still
// fails.
//
// blockcheck: using static InMemoryTransportContext;

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class InMemoryTransportContext
{
    // blocks 2, 3: `services.AddBrighter(…)`, `services.AddConsumers(…)`
    public static IServiceCollection services => null!;
    // block 3: `options.Subscriptions = subscriptions`
    public static IEnumerable<Subscription> subscriptions => null!;
}

// blocks 2, 4: `RequestType = typeof(GreetingMade)`; block 4: `new Subscription<GreetingMade>(…)`
public class GreetingMade() : Event(Id.Random());
