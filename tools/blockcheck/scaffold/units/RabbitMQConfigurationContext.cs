// Types and values RabbitMQConfiguration.md names in its blocks and never shows.
//
// The subscription blocks subscribe to `GreetingMade`, the reader's own event, which the page
// does not declare. The stub is a request and nothing more: no block names a member of it. Block 6
// registers its producers and consumers against the `services` collection the reader's host owns;
// the value is typed from a pinned package and returns a default.
//
// blockcheck: using static RabbitMQConfigurationContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class RabbitMQConfigurationContext
{
    // block 6: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// block 5: `new RmqSubscription<GreetingMade>(…)`
public class GreetingMade() : Event(Id.Random());
