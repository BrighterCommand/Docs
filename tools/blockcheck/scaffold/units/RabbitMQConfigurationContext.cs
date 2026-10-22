// The type RabbitMQConfiguration.md names in its blocks and never shows.
//
// The subscription blocks subscribe to `GreetingMade`, the reader's own event, which the page
// does not declare. The stub is a request and nothing more: no block names a member of it.

using Paramore.Brighter;

// block 5: `new RmqSubscription<GreetingMade>(…)`
public class GreetingMade() : Event(Id.Random());
