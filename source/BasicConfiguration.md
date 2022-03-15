# Basic Configuration

We want to support using Brighter in your project with the minimum of
dependencies on other packages. Specifically we want to avoid a
dependency on Inversion Of Control (IoC) framework, or logging framework
to give you freedom over the libraries you chose for your project.

Mark Seeman\'s blogs on a [DI Friendly
Framework](http://blog.ploeh.dk/2014/05/19/di-friendly-framework/) and
[Message Dispatching without Service
Location](http://blog.ploeh.dk/2011/09/19/MessageDispatchingwithoutServiceLocation/)
are highly influential on the current implementation of Brighter. (An
earlier version of Brighter used Service Location for message dispatch
which resulted in the need for abstraction of the client\'s IoC
implementation of choice).

This does mean that clients have slightly more to implement over simply
plugging us into their IoC container, but the loose-coupling from an IoC
container is on our opinion worth that cost.

## What you need to provide

-   You need to provide a **Subscriber Registry** with all of the
    **Command**s or **Event**s you wish to handle, mapped to their
    **Request Handlers**.
-   You need to provide a **Handler Factory** to create your Handlers
-   You need to provide a **Policy Registry** if you intend to use
    [Polly](https://github.com/App-vNext/Polly) to support Retry and
    Circuit-Breaker.
-   You need to provide a **Request Context Factory**

## Subscriber Registry

The Command Dispatcher needs to be able to map **Command**s or
**Event**s to a **Request Handlers**. For a **Command** we expect one
and only one **Request Handlers** for an event we expect many. Register
your handlers with the **Subscriber Registry**

``` csharp
var registry = new SubscriberRegistry();
registry.Register<GreetingCommand, GreetingCommandHandler>();
```

We also support an initializer syntax

``` csharp
var registry = new SubscriberRegistry()
{
    {typeof(GreetingCommand), typeof(GreetingCommandHandler)}
}
```

## Handler Factory

We don\'t know how to construct your handler so we call a factory, that
you provide us, to build this entire dependency chain. This factory
needs to implement the interface defined in **IAmAHandlerFactory**.

Brighter manages the lifetimes of handlers, as we consider the request
pipeline to be a scope, and we will call your factory again asking to
release those handlers once we have terminated the pipeline and finished
processing the request. You should take appropriate action to clear up
the handler and its dependencies in response to that call

It\'s worth reading Mark Seeman\'s article on [DI Friendly
Frameworks](http://blog.ploeh.dk/2014/05/19/di-friendly-framework/) to
understand this technique. Brighter originally used a conforming
container but switched to user defined factories as per Mark\'s blog.

You can implement the Handler Factory using an IoC container, in your
own code. We will be using [TinyIoC
Container](https://github.com/grumpydev/TinyIoC). For example:

``` csharp
internal class HandlerFactory : IAmAHandlerFactory
{
    private readonly TinyIoCContainer _container;

    public HandlerFactory(TinyIoCContainer container)
    {
        _container = container;
    }

    public IHandleRequests Create(Type handlerType)
    {
        return (IHandleRequests)_container.GetInstance(handlerType);
    }

    public void Release(IHandleRequests handler)
    {
        _container.Release(handler);
    }
}
```

## Policy Registry

If you intend to use a [Polly](https://github.com/App-vNext/Polly)
Policy to support [Retry and
Circuit-Breaker](PolicyRetryAndCircuitBreaker.html) then you will need
to register the Policies in the **Policy Registry**. Registration
requires a string as a key, that you will use in your \[UsePolicy\]
attribute to choose the policy. We provide two keys:
CommandProcessor.RETRYPOLICY and CommandProcessor.CIRCUITBREAKER.

``` csharp
var retryPolicy = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromMilliseconds(500));
var policyRegistry = new PolicyRegistry() { { CommandProcessor.RETRYPOLICY, retryPolicy }, { CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy } };
```

Which you can then use in code like this:

``` csharp
[RequestLogging(step: 1, timing: HandlerTiming.Before)]
[UsePolicy(CommandProcessor.CIRCUITBREAKER, step: 2)]
[UsePolicy(CommandProcessor.RETRYPOLICY, step: 3)]
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
```

## Request Context Factory

You need to provide a factory to give us instances of a
[Context](UsingTheContextBag.html). If you have no implementation to
use, just use the default **InMemoryRequestContextFactory**

## Putting it all together

All these individual elements can be passed to a **Command Processor
Builder** to help build a **Command Processor**. This has a fluent
interface to help guide you when configuring Brighter. The result looks
like this:

``` csharp
var commandProcessor = CommandProcessorBuilder.With()
    .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
    .Policies(policyRegistry)
    .NoTaskQueues()
    .RequestContextFactory(new InMemoryRequestContextFactory())
    .Build();
```

We discuss [Task Queues](DistributedTaskQueueConfiguration.html) later.
