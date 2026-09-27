// Types and values SchedulingAMessage.md names in its blocks and never declares.
//
// The code examples are services and a handler written against a domain the page never shows —
// orders, users, notifications, payments. Each stub carries only the members a block names; `Order`
// is read as a type the page never shows (spec 017 task 1.10), never as `StackExchange.Redis.Order`.
//
// blockcheck: using static SchedulingAMessageContext;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class SchedulingAMessageContext
{
    // blocks 7–10: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// blocks 1, 4: `_repository.SaveAsync(order)`, `order.Id`, `order.ProcessSchedulerId`, `order.Status`
public class Order
{
    public Guid Id { get; set; }
    public string? ProcessSchedulerId { get; set; }
    public OrderStatus Status { get; set; }
}

// block 4: `OrderStatus.Cancelled`
public enum OrderStatus { Cancelled }

// blocks 1, 4: `_repository.SaveAsync`, `GetAsync`, `UpdateAsync`
public interface IOrderRepository
{
    Task SaveAsync(Order order);
    Task<Order> GetAsync(Guid orderId);
    Task UpdateAsync(Order order);
}

// block 1: `new ProcessOrderCommand { OrderId = order.Id }`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// block 2: `_repository.SaveAsync(user)`, `user.Id`
public class User
{
    public Guid Id { get; set; }
}

// block 2: `_repository.SaveAsync(user)`
public interface IUserRepository
{
    Task SaveAsync(User user);
}

// block 2: `new SendWelcomeEmailCommand { UserId = user.Id }`
public class SendWelcomeEmailCommand() : Command(Id.Random())
{
    public Guid UserId { get; set; }
}

// block 2: `new SendReminderEmailCommand { UserId = user.Id }`
public class SendReminderEmailCommand() : Command(Id.Random())
{
    public Guid UserId { get; set; }
}

// block 3: `request.Delay`, `request.UserId`, `request.Message`
public class NotificationRequest
{
    public TimeSpan Delay { get; set; }
    public Guid UserId { get; set; }
    public string? Message { get; set; }
}

// block 3: `new NotificationEvent { UserId = …, Message = … }`
public class NotificationEvent() : Event(Id.Random())
{
    public Guid UserId { get; set; }
    public string? Message { get; set; }
}

// block 5: `command.AttemptNumber = attemptNumber + 1`
public class OperationCommand() : Command(Id.Random())
{
    public int AttemptNumber { get; set; }
}

// block 6: `RequestHandlerAsync<ProcessPaymentCommand>`, `command.PaymentId`
public class ProcessPaymentCommand() : Command(Id.Random())
{
    public Guid PaymentId { get; set; }
}

// block 6: `_paymentGateway.ProcessAsync(command.PaymentId, cancellationToken)`
public interface IPaymentGateway
{
    Task ProcessAsync(Guid paymentId, CancellationToken cancellationToken);
}

// block 6: `catch (PaymentGatewayUnavailableException)`
public class PaymentGatewayUnavailableException : Exception;

// block 6: `catch (PaymentDeclinedException ex)`
public class PaymentDeclinedException : Exception;

// block 7: `registry.RegisterAsync<ProcessOrderCommand, ProcessOrderHandlerAsync>()`
public class ProcessOrderHandlerAsync : RequestHandlerAsync<ProcessOrderCommand>;
