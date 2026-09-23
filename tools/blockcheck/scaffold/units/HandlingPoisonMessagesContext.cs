// Values HandlingPoisonMessages.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
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
}
