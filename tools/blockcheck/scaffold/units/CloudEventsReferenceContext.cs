// The type and value CloudEventsReference.md names in its blocks and never declares.
//
// Every block configures a publication for the reader's own `OrderCreated` event, which the page
// never shows, and the Kafka section posts one through the reader's command processor. The value
// is typed from a pinned package and returns a default.
//
// blockcheck: using static CloudEventsReferenceContext;

using Paramore.Brighter;

public static class CloudEventsReferenceContext
{
    // block 3: `await commandProcessor.PostAsync(new OrderCreated(), context);`
    public static IAmACommandProcessor commandProcessor => null!;
}

// blocks 1–5: `RmqPublication<OrderCreated>`, `KafkaPublication<OrderCreated>`,
// `SnsPublication<OrderCreated>`, `AzureServiceBusPublication<OrderCreated>`; block 3:
// `new OrderCreated()`
public class OrderCreated() : Event(Id.Random());
