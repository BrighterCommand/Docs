// Types and values FAQ.md names in its blocks and never declares.
//
// The answers use the reader's own commands, events and handlers, which the page does not show.
// Every value member is typed from a pinned package or the BCL, returns a default, and does
// nothing. A block that calls a member of one of these is checked against the real type, so a
// wrong member or argument still fails. A stub is a request, or a handler of one, with only the
// members a block names.
//
// blockcheck: using static FAQContext;

using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

public static class FAQContext
{
    public static IAmACommandProcessor commandProcessor => null!;
    public static IRequest command => null!;
    public static TimeSpan delay => default;
    public static IAmARequestSchedulerAsync scheduler => null!;
    public static TimeSpan newDelay => default;
    // block 9: `registry.RegisterAsync<MyCommand>(…)`
    public static ServiceCollectionSubscriberRegistry registry => null!;
    // block 13: `services.AddResiliencePipeline(…)`
    public static IServiceCollection services => null!;
}

// blocks 9, 17: `request as MyCommand`, `myCommand?.Value`, `new MyCommand()`
public class MyCommand() : Command(Id.Random())
{
    public string? Value { get; set; }
}

// blocks 7, 17: `IAmAMessageMapper<MyEvent>`, `new MyEvent()`
public class MyEvent() : Event(Id.Random());

// blocks 10–12: `new GreetingCommand("Ian")`
public class GreetingCommand(string name) : Command(Id.Random());

// blocks 6: `typeof(TaskCreated)`, `typeof(TaskUpdated)`
public class TaskCreated() : Event(Id.Random());
public class TaskUpdated() : Event(Id.Random());

// block 9: `typeof(MyImplicitHandlerAsync)`, `typeof(MyCommandHandlerAsync)`
public class MyImplicitHandlerAsync : RequestHandlerAsync<MyCommand>;
public class MyCommandHandlerAsync : RequestHandlerAsync<MyCommand>;
