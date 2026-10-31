// Task 6.4, ImplementingExternalBus.md #3, verbatim: the handler 6.3 rewrote for V10, with [RequestLogging] and the
// legacy [UsePolicy] naming CommandProcessor.CIRCUITBREAKER and RETRYPOLICY. Unstated, and what a reader relies on:
// registered with AddBrighter's defaults, the pipeline builds and the handler sends the reminder; a gateway that fails
// transiently is retried by the RETRYPOLICY. Case: a gateway that fails once. Control: the same handler without the
// [UsePolicy] attribute - the one failure reaches the caller.
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;

public class TaskReminderCommand() : Command(Id.Random())                                     // probe: the unit's members
{
    public string TaskName { get; set; } = "Write docs"; public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public string Recipient { get; set; } = "a@example.com"; public string CopyTo { get; set; } = "b@example.com";
}
public record TaskName(string Value); public record EmailAddress(string Value);              // probe
public record TaskReminder(TaskName taskName, DateTime dueDate, EmailAddress reminderTo, EmailAddress copyReminderTo);
public interface IAmAMailGateway { void Send(TaskReminder reminder); }
public class FlakyGateway : IAmAMailGateway
{
    public static int Calls, Sent, FailFirst;
    public void Send(TaskReminder reminder) { Calls++; if (FailFirst-- > 0) throw new TimeoutException("smtp"); Sent++; }
}

// #3, verbatim
public class MailTaskReminderHandler : RequestHandler<TaskReminderCommand>
{
    private readonly IAmAMailGateway _mailGateway;

    public MailTaskReminderHandler(IAmAMailGateway mailGateway)
    {
        _mailGateway = mailGateway;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [UsePolicy(new [] {CommandProcessor.CIRCUITBREAKER, CommandProcessor.RETRYPOLICY}, step: 2)]
    public override TaskReminderCommand Handle(TaskReminderCommand command)
    {
        _mailGateway.Send(new TaskReminder(
        taskName: new TaskName(command.TaskName),
        dueDate: command.DueDate,
        reminderTo: new EmailAddress(command.Recipient),
        copyReminderTo: new EmailAddress(command.CopyTo)
        ));

        return base.Handle(command);
    }
}

// probe: the control, without [UsePolicy]
public class NoPolicyHandler(IAmAMailGateway mailGateway) : RequestHandler<TaskReminderCommand>
{
    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    public override TaskReminderCommand Handle(TaskReminderCommand command)
    {
        mailGateway.Send(new TaskReminder(new TaskName(command.TaskName), command.DueDate, new EmailAddress(command.Recipient), new EmailAddress(command.CopyTo)));
        return base.Handle(command);
    }
}

public static class Program
{
    static string Send<THandler>() where THandler : class, IHandleRequests<TaskReminderCommand>
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmAMailGateway, FlakyGateway>();
        services.AddBrighter().Handlers(h => h.Register<TaskReminderCommand, THandler>());
        var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
        FlakyGateway.Calls = FlakyGateway.Sent = 0; FlakyGateway.FailFirst = 1;
        try { cp.Send(new TaskReminderCommand()); return $"sent {FlakyGateway.Sent}, gateway called {FlakyGateway.Calls} time(s)"; }
        catch (Exception e) { return $"throws {e.GetType().Name}, gateway called {FlakyGateway.Calls} time(s)"; }
    }

    public static int Main()
    {
        var c = Send<MailTaskReminderHandler>();
        var k = Send<NoPolicyHandler>();
        Console.WriteLine($"case    #3, one transient failure: {c}");
        Console.WriteLine($"control no [UsePolicy]:            {k}");
        var ok = c == "sent 1, gateway called 2 time(s)" && k.StartsWith("throws TimeoutException, gateway called 1");
        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} the V10 handler builds under AddBrighter's defaults and RETRYPOLICY retries the gateway");
        return ok ? 0 : 1;
    }
}
