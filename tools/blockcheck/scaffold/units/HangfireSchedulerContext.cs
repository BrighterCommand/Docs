// Types and values HangfireScheduler.md names in its blocks and never declares.
//
// Block 1 is the whole of an application's startup; the blocks after it change one part of it,
// and name the `builder`, `app` and Hangfire `configuration` that startup has in scope. The
// services schedule work from a domain the page never shows.
//
// blockcheck: using static HangfireSchedulerContext;

using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

public static class HangfireSchedulerContext
{
    // blocks 2–4, 10, 16, 18, 23, 27, 28: `builder.Services…`, as block 1 declares it
    public static WebApplicationBuilder builder => null!;
    // blocks 11, 20, 30: `app.UseHangfireDashboard(…)`, as block 1 declares it
    public static WebApplication app => null!;
    // blocks 5–9, 19: the `IGlobalConfiguration` that `AddHangfire(configuration => …)` passes
    public static IGlobalConfiguration configuration => null!;
    // blocks 5–8, 19: the storage's connection string
    public static string connectionString => null!;
    // block 16: `UseSqlServerStorage(sharedConnectionString)`
    public static string sharedConnectionString => null!;
    // block 17: `loggerFactory.CreateLogger<BrighterJobLogFilter>()`
    public static ILoggerFactory loggerFactory => null!;
    // block 21: `brighter.UseScheduler(…)`, the builder `AddBrighter` returns
    public static IBrighterBuilder brighter => null!;
    // block 22: a service's fields and arguments
    public static IAmACommandProcessor _commandProcessor => null!;
    public static IOrderRepository _repository => null!;
    public static TimeSpan delay => default;
    public static IRequest command => null!;
    public static Order order => null!;
}

// blocks 13, 14, 22: `order.Id`, `order.ProcessSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public string? ProcessSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 14: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 13, 14, 22: `repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// block 13: `new ProcessOrderCommand { OrderId = order.Id }`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// block 15: `new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }`
public class GenerateDailyReportCommand() : Command(Id.Random())
{
    public DateTime Date { get; set; }
}
