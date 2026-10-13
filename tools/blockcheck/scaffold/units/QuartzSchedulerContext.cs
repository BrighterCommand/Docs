// Types and values QuartzScheduler.md names in its blocks and never declares.
//
// Block 1 is the whole of an application's startup; the blocks after it change one part of it,
// and name the `builder` it has in scope, or the `q` and `store` that `AddQuartz(q => …)` and
// `UsePersistentStore(store => …)` pass. The services schedule work from a domain the page never
// shows.
//
// blockcheck: using static QuartzSchedulerContext;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Quartz;

public static class QuartzSchedulerContext
{
    // blocks 2, 3, 8, 10, 14–16: `builder.Services…`, as block 1 declares it
    public static WebApplicationBuilder builder => null!;
    // blocks 4–7, 17, 22: the configurator `AddQuartz(q => …)` passes
    public static IServiceCollectionQuartzConfigurator q => null!;
    // blocks 18, 20, 21, 27, 28: the options `UsePersistentStore(store => …)` passes
    public static SchedulerBuilder.PersistentStoreOptions store => null!;
    // blocks 4–6, 14, 17: the job store's connection string
    public static string connectionString => null!;
    // block 9: the `IScheduler` block 8 resolves
    public static IScheduler scheduler => null!;
    // block 26: `_schedulerFactory.GetScheduler()`, `services.AddSingleton<…>()`
    public static ISchedulerFactory _schedulerFactory => null!;
    public static IServiceCollection services => null!;
    // block 19: a service's fields and arguments
    public static IAmACommandProcessor _commandProcessor => null!;
    public static IOrderRepository _repository => null!;
    public static TimeSpan delay => default;
    public static IRequest command => null!;
    public static Order order => null!;
}

// blocks 11, 12, 19: `order.Id`, `order.ProcessSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public string? ProcessSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 12: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 11, 12, 19: `repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// block 11: `new ProcessOrderCommand { OrderId = order.Id }`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// block 13: `new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }`
public class GenerateDailyReportCommand() : Command(Id.Random())
{
    public DateTime Date { get; set; }
}

// block 8: `request is MyCommand command`, `command.CorrelationId`
public class MyCommand() : Command(Id.Random());
