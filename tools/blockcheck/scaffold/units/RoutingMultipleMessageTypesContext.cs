// Types and values RoutingMultipleMessageTypes.md names in its blocks and never declares.
//
// The page routes the reader's own events and commands, through handlers it never shows. Each
// request stub is a request and nothing more, and each handler stub is a handler of its request
// and does nothing, since a block names it only in `Register<…, …>()` or `typeof(…)`. `services`
// and `subscription` are the values the reader's host holds: block 5 registers the subscription an
// earlier block builds. Each is typed from a pinned package and returns a default.
//
// blockcheck: using static RoutingMultipleMessageTypesContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class RoutingMultipleMessageTypesContext
{
    // blocks 2, 5–7: `services.AddBrighter(…)`, `services.AddConsumers(…)`
    public static IServiceCollection services => null!;

    // block 5: `options.Subscriptions = new[] { subscription }`
    public static Subscription subscription => null!;
}

// blocks 1, 2, 5: Kafka task events and their handlers
public class TaskCreated() : Event(Id.Random());
public class TaskUpdated() : Event(Id.Random());
public class TaskCompleted() : Event(Id.Random());
public class TaskCreatedHandler : RequestHandler<TaskCreated>;
public class TaskUpdatedHandler : RequestHandler<TaskUpdated>;
public class TaskCompletedHandler : RequestHandler<TaskCompleted>;

// block 3: `typeof(CreateOrder)`, `typeof(UpdateOrder)`, `typeof(CancelOrder)`
public class CreateOrder() : Command(Id.Random());
public class UpdateOrder() : Command(Id.Random());
public class CancelOrder() : Command(Id.Random());

// block 4: `typeof(UserCreated)`, `typeof(UserUpdated)`, `typeof(UserDeleted)`
public class UserCreated() : Event(Id.Random());
public class UserUpdated() : Event(Id.Random());
public class UserDeleted() : Event(Id.Random());

// block 6: `request as OrderCreated`, `order?.Country`
public class OrderCreated() : Event(Id.Random())
{
    public string? Country { get; set; }
}

// block 6: `typeof(USOrderCreatedHandler)`, …
public class USOrderCreatedHandler : RequestHandler<OrderCreated>;
public class UKOrderCreatedHandler : RequestHandler<OrderCreated>;
public class DefaultOrderCreatedHandler : RequestHandler<OrderCreated>;

// block 7: `registry.Register<ItemAdded, ItemAddedHandler>()`, …
public class ItemAdded() : Event(Id.Random());
public class ItemRemoved() : Event(Id.Random());
public class StockAdjusted() : Event(Id.Random());
public class ItemAddedHandler : RequestHandler<ItemAdded>;
public class ItemRemovedHandler : RequestHandler<ItemRemoved>;
public class StockAdjustedHandler : RequestHandler<StockAdjusted>;

// block 8: `typeof(EmailSent)`, `typeof(SmsSent)`, `typeof(PushNotificationSent)`
public class EmailSent() : Event(Id.Random());
public class SmsSent() : Event(Id.Random());
public class PushNotificationSent() : Event(Id.Random());

// block 9: `typeof(OrderPlaced)`, `typeof(OrderShipped)`, `typeof(OrderDelivered)`
public class OrderPlaced() : Event(Id.Random());
public class OrderShipped() : Event(Id.Random());
public class OrderDelivered() : Event(Id.Random());
