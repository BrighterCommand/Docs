// Types and values ImplementingExternalBus.md names in its blocks and never declares.
//
// Block 1 builds the reader's `TaskReminderCommand` from a `reminder` the reader's web layer
// supplies, and posts it through the command processor its host owns. The command stub is a request
// whose constructor takes the four arguments the block names; the `reminder` is a tuple of
// exactly the strings the block reads.
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

// blocks 1, 2: `new TaskReminderCommand(taskName: …, dueDate: …, recipient: …, copyTo: …)`,
// `IAmAMessageMapper<TaskReminderCommand>`, `request.Id`
public class TaskReminderCommand(string taskName, DateTime dueDate, string recipient, string copyTo)
    : Command(Id.Random());
