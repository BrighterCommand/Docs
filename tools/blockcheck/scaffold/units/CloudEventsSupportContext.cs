// Types and values CloudEventsSupport.md names in its blocks and never declares.
//
// The page's blocks publish, map and subscribe for the reader's own events and command, and register
// them against a `services` collection the host owns. Each value is typed from a pinned package and
// returns a default. Each request stub is a request and nothing more: no block names a member of
// its own.
//
// blockcheck: using static CloudEventsSupportContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class CloudEventsSupportContext
{
    // blocks 1, 16: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// blocks 1–4, 16: `typeof(OrderCreated)`, `IAmAMessageMapper<OrderCreated>`
public class OrderCreated() : Event(Id.Random());

// block 4: `typeof(OrderUpdated)`
public class OrderUpdated() : Event(Id.Random());

// block 6: `IAmAMessageMapper<LargeOrder>`
public class LargeOrder() : Event(Id.Random());

// block 5: `public override MyCommand Handle(MyCommand command)`
public class MyCommand() : Command(Id.Random());
