// Types and values HowConfiguringTheCommandProcessorWorks.md names in its blocks and never declares.
//
// Every value is typed from a pinned package, the BCL or a stub below, returns a default, and does
// nothing. A block that calls a member of one of these is checked against the real type, so a wrong
// member or argument still fails. The handlers send a reminder through a mail gateway the page names
// and never shows, and key their pipelines and policies by "a global readonly reference, not shown
// here": `Globals`.
//
// blockcheck: using static HowConfiguringTheCommandProcessorWorksContext;

using System;
using Paramore.Brighter;
using Polly.Registry;

public static class HowConfiguringTheCommandProcessorWorksContext
{
    public static IAmAHandlerFactorySync handlerFactory => null!;
    public static SubscriberRegistry subscriberRegistry => null!;
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
    // blocks 5, 8, 9: `_mailGateway.Send(new TaskReminder(…))`. No block names the gateway's type, so it
    // is an `object` and `Send` an extension, checked against the `TaskReminder` it is given
    public static object _mailGateway => null!;
    public static void Send(this object mailGateway, TaskReminder reminder) { }
}

// blocks 4–9: "a global readonly reference, not shown here"; `const`, since attributes take them
public static class Globals
{
    public const string MYRETRYPIPELINE = "MyRetryPipeline";
    public const string MYCIRCUITBREAKER = "MyCircuitBreaker";
    public const string MYCIRCUITBREAKERANDRETRY = "MyCircuitBreakerAndRetry";
    public const string MYRETRYPOLICY = "MyRetryPolicy";
}

// block 1: `registry.Register<GreetingCommand, GreetingCommandHandler>()`
public class GreetingCommand() : Command(Id.Random());

public class GreetingCommandHandler : RequestHandler<GreetingCommand>;

// blocks 5, 6, 8, 9: `Handle(TaskReminderCommand command)`, `command.TaskName`, `.DueDate`, `.Recipient`, `.CopyTo`
public class TaskReminderCommand() : Command(Id.Random())
{
    public string TaskName { get; set; } = "";
    public DateTime DueDate { get; set; }
    public string Recipient { get; set; } = "";
    public string CopyTo { get; set; } = "";
}

// blocks 5, 8, 9: `new TaskReminder(taskName: …, dueDate: …, reminderTo: …, copyReminderTo: …)`
public class TaskReminder(TaskName taskName, DateTime dueDate, EmailAddress reminderTo, EmailAddress copyReminderTo);

// blocks 5, 8, 9: `new TaskName(command.TaskName)`, `new EmailAddress(command.Recipient)`
public class TaskName(string name);

public class EmailAddress(string address);
