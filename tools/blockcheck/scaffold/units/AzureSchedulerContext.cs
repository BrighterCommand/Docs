// Types and values AzureScheduler.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static AzureSchedulerContext;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

public static class AzureSchedulerContext
{
    // blocks 17, 24: `_commandProcessor.SendAsync(…)`, a field of the caller's
    public static IAmACommandProcessor _commandProcessor => null!;
    // blocks 17, 18, 24: `_scheduler.CancelAsync(…)`
    public static IAmAMessageSchedulerAsync _scheduler => null!;
    // block 17: `SendAsync(newDelay, command)`
    public static IRequest command => null!;
    // blocks 17, 18: `existingSchedulerId`
    public static string existingSchedulerId => null!;
    // blocks 17, 18: `newDelay`
    public static TimeSpan newDelay => default;
    // blocks 7, 8, 23: `new AzureServiceBusSchedulerFactory(clientProvider, …)`, built in block 1
    public static IServiceBusClientProvider clientProvider => null!;
    // block 22: `environment.IsDevelopment()`
    public static IHostEnvironment environment => null!;
}

// blocks 9, 11, 12: `order.Id`, `order.Total`, `order.PaymentSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string? PaymentSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 11: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 9, 11, 12: `repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// blocks 9, 12, 25: `new ProcessPaymentCommand { OrderId = order.Id, Amount = order.Total }`
public class ProcessPaymentCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}

// block 10: `new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }`
public class GenerateDailyReportCommand() : Command(Id.Random())
{
    public DateTime Date { get; set; }
}
