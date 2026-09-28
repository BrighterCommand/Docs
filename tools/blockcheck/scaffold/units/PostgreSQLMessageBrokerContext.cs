// Types and values PostgreSQLMessageBroker.md and PostgreSQLBrokerTradeOffs.md name in their
// blocks and never declare.
//
// The pages configure a producer and a consumer for the reader's own order events, and write to
// the reader's own `DbContext`, none of which they show. Every value member is typed from a pinned
// package or the BCL and returns a default. A stub is a request, or an interface, with only the
// members a block names; `Order` is read as a type the page never shows (1.10).
//
// blockcheck: using static PostgreSQLMessageBrokerContext;

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class PostgreSQLMessageBrokerContext
{
    // broker blocks 1, 3, 8: `services.AddBrighter(…)`, `services.AddConsumers(…)`,
    // `services.AddOpenTelemetry()`
    public static IServiceCollection services => null!;
    // broker block 9, trade-offs blocks 1, 2: `connectionString: connectionString`
    public static string connectionString => null!;
    // broker block 6: `await commandProcessor.PostAsync(…)`, `OrderId = orderId`
    public static IAmACommandProcessor commandProcessor => null!;
    public static string orderId => null!;
}

// broker blocks 1–4, 7: the event the pages publish and handle; block 2 sets these members,
// block 4 reads `OrderId`
public class OrderCreatedEvent() : Event(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// broker blocks 2, 7: `CreateOrderAsync(CreateOrderCommand command)`; block 2 reads these
public class CreateOrderCommand() : Command(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

// broker block 4: `await _emailService.SendOrderConfirmationAsync(@event)`
public interface IEmailService
{
    Task SendOrderConfirmationAsync(OrderCreatedEvent orderCreatedEvent);
}

// broker block 6: `new OrderReminderEvent { OrderId = …, ReminderText = … }`
public class OrderReminderEvent() : Event(Id.Random())
{
    public string OrderId { get; set; } = string.Empty;
    public string ReminderText { get; set; } = string.Empty;
}

// broker blocks 5, 10: `PostgresSubscription<OrderEvent>`
public class OrderEvent() : Event(Id.Random());

// broker block 13: `PostgresSubscription<HighVolumeEvent>`, `PostgresSubscription<NormalEvent>`
public class HighVolumeEvent() : Event(Id.Random());
public class NormalEvent() : Event(Id.Random());

// broker block 7: `_dbContext.Database`, `_dbContext.Orders.Add(order)`
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
}

// broker block 7: `new Order { /* ... */ }`
public class Order;
