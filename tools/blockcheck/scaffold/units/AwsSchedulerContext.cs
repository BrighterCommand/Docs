// Types and values AwsScheduler.md names in its blocks and never declares.
//
// The page is on the V4 pin, so the connection is the .V4 gateway's. The services schedule work
// from a domain the page never shows. Each stub carries only the members a block names.
//
// blockcheck: using static AwsSchedulerContext;

using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AWSSQS.V4;

public static class AwsSchedulerContext
{
    // blocks 1, 4–6, 11–16, 18, 21, 22: `new AwsSchedulerFactory(awsConnection, …)`, built in block 2
    public static AWSMessagingGatewayConnection awsConnection => null!;
    // blocks 2, 3, 19, 20: `new AWSMessagingGatewayConnection(credentials, …)`, "From AWS credentials provider"
    public static AWSCredentials credentials => null!;
    // block 19: `environment.IsDevelopment()`
    public static IHostEnvironment environment => null!;
    // block 22: `await _scheduler.CancelAsync(oldSchedulerId)`
    public static IAmAMessageSchedulerAsync _scheduler => null!;
    public static string oldSchedulerId => null!;
}

// blocks 7, 9: `order.Id`, `order.Total`, `order.PaymentSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string? PaymentSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 9: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 7, 9: `repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// block 6: `request is ProcessOrderCommand cmd`, `cmd.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// blocks 7, 15: `new ProcessPaymentCommand { OrderId = …, Amount = … }`, `cmd.TransactionId`
public class ProcessPaymentCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
}

// block 8: `new GenerateDailyReportCommand { Date = DateTime.UtcNow.Date }`
public class GenerateDailyReportCommand() : Command(Id.Random())
{
    public DateTime Date { get; set; }
}

// block 10: `user.Id`, `user.TrialStartDate`, `user.TrialEndDate`
public class User
{
    public Guid Id { get; set; }
    public DateTime TrialStartDate { get; set; }
    public DateTime TrialEndDate { get; set; }
}

// block 10: `repository.SaveAsync(user)`
public interface IUserRepository
{
    Task SaveAsync(User user);
}

// block 10: `new TrialExpiringEvent { UserId = user.Id, DaysRemaining = 7 }`
public class TrialExpiringEvent() : Event(Id.Random())
{
    public Guid UserId { get; set; }
    public int DaysRemaining { get; set; }
}
