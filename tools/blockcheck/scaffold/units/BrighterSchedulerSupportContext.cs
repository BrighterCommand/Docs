// Types and values BrighterSchedulerSupport.md names in its blocks and never declares.
//
// The examples schedule commands from a domain the page never shows. Each stub carries only the
// members a block names.
//
// blockcheck: using static BrighterSchedulerSupportContext;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class BrighterSchedulerSupportContext
{
    // blocks 2–6: `commandProcessor.SendAsync(…)`, `commandProcessor.Send(…)`
    public static IAmACommandProcessor commandProcessor => null!;
    // block 2: `OrderId = orderId`
    public static Guid orderId => default;
    // block 3: `UserId = userId`
    public static Guid userId => default;
    // block 4: `TransactionId = txId`
    public static string txId => null!;
    // block 6: `OperationId = opId`
    public static Guid opId => default;
    // block 6: `await scheduler.CancelAsync(schedulerId)`
    public static IAmARequestSchedulerAsync scheduler => null!;
    // block 7: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// blocks 2, 8, 9: `new ProcessOrderCommand { OrderId = orderId }`, `command.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public Guid OrderId { get; set; }
}

// block 3: `new SendReminderCommand { UserId = userId }`
public class SendReminderCommand() : Command(Id.Random())
{
    public Guid UserId { get; set; }
}

// block 4: `new ProcessPaymentCommand { TransactionId = txId }`
public class ProcessPaymentCommand() : Command(Id.Random())
{
    public string TransactionId { get; set; } = string.Empty;
}

// block 5: `new RetryOperationCommand { AttemptNumber = 2 }`
public class RetryOperationCommand() : Command(Id.Random())
{
    public int AttemptNumber { get; set; }
}

// block 6: `new CancelableOperationCommand { OperationId = opId }`
public class CancelableOperationCommand() : Command(Id.Random())
{
    public Guid OperationId { get; set; }
}

// block 8: `orderService.ProcessAsync(command.OrderId, cancellationToken)`
public interface IOrderService
{
    Task ProcessAsync(Guid orderId, CancellationToken cancellationToken);
}

// block 8: `catch (DatabaseUnavailableException)`
public class DatabaseUnavailableException : Exception;
