// Types and values HandlingPoisonMessages.md names in its blocks and never declares.
//
// The page subscribes to, handles and posts a `PlaceOrder` it never shows. Every value member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static HandlingPoisonMessagesContext;

using Paramore.Brighter;

public static class HandlingPoisonMessagesContext
{
    public static IAmAMessageConsumerSync deadLetterConsumer => null!;
    public static Message dlqMessage => null!;
    public static IAmAMessageProducerAsync producer => null!;
    // block 3: `await commandProcessor.PostAsync(new PlaceOrder { … })`
    public static IAmACommandProcessor commandProcessor => null!;
}

// blocks 1–3: `KafkaSubscription<PlaceOrder>`, `RequestHandlerAsync<PlaceOrder>`;
// block 3: `new PlaceOrder { OrderId = "poison", Quantity = -1 }`
public class PlaceOrder() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
