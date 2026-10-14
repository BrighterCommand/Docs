// Types and values InMemoryScheduler.md names in its blocks and never declares.
//
// The services and tests run against a domain the page never shows: orders, analytics, reports,
// payments, and the test fixture's command. Each stub carries only the
// members a block names.
//
// blockcheck: using static InMemorySchedulerContext;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

public static class InMemorySchedulerContext
{
    // blocks 10–12: `brighter.UseScheduler(…)`, the builder `AddBrighter` returns (block 4)
    public static IBrighterBuilder brighter => null!;
    // block 10: `environment.IsDevelopment()`
    public static IHostEnvironment environment => null!;
    // blocks 13, 15, 16: `_commandProcessor.SendAsync(…)`, a field of the caller's
    public static IAmACommandProcessor _commandProcessor => null!;
    // blocks 13, 16: `SendAsync(…, command)`
    public static IRequest command => null!;
    // block 16: `SendAsync(delay, command)`
    public static TimeSpan delay => default;
    // block 16: `_logger.LogInformation(…)`
    public static ILogger _logger => null!;
    // block 16: `_serviceProvider.GetService<…>()`
    public static IServiceProvider _serviceProvider => null!;
}

// blocks 6, 8: `order.Id`, `order.ConfirmationSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public string? ConfirmationSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 8: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 6, 8: `repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// block 2: `repository.SaveActionAsync(userId, action)`
public interface IAnalyticsRepository
{
    Task SaveActionAsync(string userId, string action);
}

// block 2: `new AggregateAnalyticsCommand { UserId = userId }`
public class AggregateAnalyticsCommand() : Command(Id.Random())
{
    public string UserId { get; set; } = string.Empty;
}

// blocks 6, 8: `new SendOrderConfirmationCommand { OrderId = order.Id }`
public class SendOrderConfirmationCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// block 7: `new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }`
public class GenerateDailyReportCommand() : Command(Id.Random())
{
    public DateTime Date { get; set; }
}

// blocks 1, 9: `new TestCommand()`
public class TestCommand() : Command(Id.Random());

// block 15: `new ProcessPaymentCommand { Amount = 1000.00m }`
public class ProcessPaymentCommand() : Command(Id.Random())
{
    public decimal Amount { get; set; }
}

// block 16: `IHandleRequestsAsync<YourCommand>`
public class YourCommand() : Command(Id.Random());
