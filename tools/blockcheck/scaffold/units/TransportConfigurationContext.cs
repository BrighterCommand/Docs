// The type six transport configuration pages name in their blocks and never declare.
//
// AWSSQSConfiguration.md, GcpPubSubConfiguration.md, MQTTConfiguration.md, MSSQLMessageBroker.md,
// RedisConfiguration.md and RocketMQConfiguration.md each configure publications and subscriptions
// for the reader's own `GreetingEvent`, and none of them shows it.

using Paramore.Brighter;

// block 1 of each page but AWSSQSConfiguration.md: `RequestType = typeof(GreetingEvent)` and
// `new …Subscription<GreetingEvent>(…)`; AWSSQSConfiguration.md blocks 1, 3, 4, 5, 6:
// `RequestType = typeof(GreetingEvent)`
public class GreetingEvent() : Event(Id.Random());
