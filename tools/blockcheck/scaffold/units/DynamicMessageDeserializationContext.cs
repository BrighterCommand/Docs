// Types and values DynamicMessageDeserialization.md names in its blocks and never declares.
//
// The page subscribes to the reader's own task events, and logs from a `_logger` the reader's
// class holds. Each event stub is a request and nothing more; the logger is typed from a pinned
// package and does nothing, so the call a block makes on it is checked against the real type.
//
// blockcheck: using static DynamicMessageDeserializationContext;

using Microsoft.Extensions.Logging;
using Paramore.Brighter;

public static class DynamicMessageDeserializationContext
{
    // block 8: `_logger.LogError(ex, …)`
    public static ILogger _logger => null!;
}

// blocks 1, 2, 4, 8: `typeof(TaskCreated)`, `KafkaSubscription<TaskCreated>`
public class TaskCreated() : Event(Id.Random());

// blocks 4, 8: `typeof(TaskUpdated)`
public class TaskUpdated() : Event(Id.Random());

// block 3: `typeof(OrderCreated)`
public class OrderCreated() : Event(Id.Random());
