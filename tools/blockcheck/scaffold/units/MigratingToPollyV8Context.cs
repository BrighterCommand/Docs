// Types and values MigratingToPollyV8.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static MigratingToPollyV8Context;

using System;
using System.Data;
using Paramore.Brighter;
using Polly.Registry;

public static class MigratingToPollyV8Context
{
    public static HandlerConfiguration handlerConfiguration => null!;
    public static PolicyRegistry policyRegistry => null!;
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
}

// blocks 4, 7, 10: `RequestHandler<MyCommand>`
public class MyCommand() : Command(Id.Random());

// blocks 11, 12: `editTaskCommand.TaskId`, `.TaskName`, `.TaskDescription`, `.TaskDueDate`
public class EditTaskCommand() : Command(Id.Random())
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public DateTime? TaskDueDate { get; set; }
}

// blocks 11, 12: the handler's task store; `BeginTransaction()`, `FindById(…)`, `Update(task)`
public interface ITasksDAO
{
    IDbTransaction BeginTransaction();
    Task FindById(int taskId);
    void Update(Task task);
}

// blocks 11, 12: the page's own task entity, `task.TaskName`, `.TaskDescription`, `.DueDate`
public class Task
{
    public string TaskName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}
