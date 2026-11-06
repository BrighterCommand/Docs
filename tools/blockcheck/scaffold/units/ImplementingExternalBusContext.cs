// Types and values ImplementingExternalBus.md names in its blocks and never declares.
//
// Block 1 builds the reader's `TaskReminderCommand` from a `reminder` the reader's web layer
// supplies, and posts it through the command processor its host owns. The command stub is a request
// whose constructor takes the four arguments the block names; the `reminder` is a tuple of
// exactly the strings the block reads. Block 3's handler reads the command's four members and
// sends a `TaskReminder` through the reader's mail gateway; each of those is a stub of exactly the
// members the block calls.
//
// blockcheck: using static ImplementingExternalBusContext;

using System;
using Paramore.Brighter;

public static class ImplementingExternalBusContext
{
    // block 1: `reminder.TaskName`, `reminder.DueDate`, `reminder.Recipient`, `reminder.CopyTo`
    public static (string TaskName, string DueDate, string Recipient, string CopyTo) reminder => default;

    // block 1: `_commandProcessor.Post(reminderCommand)`
    public static IAmACommandProcessor _commandProcessor => null!;
}

// blocks 1–3: `new TaskReminderCommand(taskName: …, dueDate: …, recipient: …, copyTo: …)`,
// `IAmAMessageMapper<TaskReminderCommand>`, `request.Id`; block 3 reads its four members
public class TaskReminderCommand(string taskName, DateTime dueDate, string recipient, string copyTo)
    : Command(Id.Random())
{
    public string TaskName { get; } = taskName;
    public DateTime DueDate { get; } = dueDate;
    public string Recipient { get; } = recipient;
    public string CopyTo { get; } = copyTo;
}

// block 3: `private readonly IAmAMailGateway _mailGateway`, `_mailGateway.Send(new TaskReminder(…))`
public interface IAmAMailGateway
{
    void Send(TaskReminder reminder);
}

// block 3: `new TaskReminder(taskName: …, dueDate: …, reminderTo: …, copyReminderTo: …)`
public class TaskReminder(TaskName taskName, DateTime dueDate, EmailAddress reminderTo, EmailAddress copyReminderTo);

// block 3: `new TaskName(command.TaskName)`
public class TaskName(string name);

// block 3: `new EmailAddress(command.Recipient)`
public class EmailAddress(string address);
