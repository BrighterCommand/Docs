// The type ErrorHandlingOptions.md names in its blocks and never shows.
//
// Every subscription on the page is a subscription to a `PlaceOrder`, the request the
// examples consume; the page configures its subscription and never declares it. No block
// names a member of it, so the stub is a request and nothing more.

using Paramore.Brighter;

// blocks 1, 2, 3, 5, 6: `new RmqSubscription<PlaceOrder>(…)`, `KafkaSubscription<PlaceOrder>`, `SqsSubscription<PlaceOrder>`
public class PlaceOrder() : Command(Id.Random());
