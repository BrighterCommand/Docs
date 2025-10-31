# How Configuring the Command Processor Works

Brighter does not have a dependency on an Inversion Of Control (IoC) framework. This gives you freedom to choose the DI libraries you want for your project.

We follow an approach outlined by Mark Seeman in his blog on a [DI Friendly
Framework](http://blog.ploeh.dk/2014/05/19/di-friendly-framework/) and
[Message Dispatching without Service
Location](http://blog.ploeh.dk/2011/09/19/MessageDispatchingwithoutServiceLocation/).

This means that we can support any approach to DI that you choose, provided you implement a range of interfaces that we require to create instances of your classes at runtime.

For .NET Core's DI framework we provide the implementation of these interfaces. If you are using that approach, just follow the outline in [Basic Configuration](/contents/BrighterBasicConfiguration.md). This chapter is 'interest only' at that point, and you don't need to read it. It may be helpful for debugging.

If you choose another DI framework, this document explains  what you need to do to support that DI framework.

## CommandProcessor Configuration Dependencies

-   You need to provide a **Subscriber Registry** with all of the  **Command**s or **Event**s you wish to handle, mapped to their **Request Handlers**.
-   You need to provide a **Handler Factory** to create your Handlers
-   You need to provide a **Policy Registry** if you intend to use [Polly](https://github.com/App-vNext/Polly) to support Retry and Circuit-Breaker.
-   You need to provide a **Request Context Factory**

## Subscriber Registry

The Command Dispatcher needs to be able to map **Command**s or **Event**s to a **Request Handlers**. 

For a **Command** we expect one and only one **Request Handlers** for an event we expect many. 

YOu can use our **SubcriberRegistry** regardless of your DI framework.

Register your handlers with your **Subscriber Registry**

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

We don't know how to construct your handler so we call a factory, that you provide, to build your handler (and its entire dependency chain). 

Instead, we take a dependency on an interface for a handler factory, and you implement that. Within the handler factory you need to construct instances of your types in response to our request to create one.

For this you need to implement the interface:  **IAmAHandlerFactory**.

Brighter manages the lifetimes of handlers, as we consider the request pipeline to be a scope, and we will call your factory again informing that we have terminated the pipeline and finished processing the request. You should take any required action to clear up the handler and its dependencies in response to that call.

You can implement the Handler Factory using an IoC container. This is what Brighter does with .NET Core 

For example using [TinyIoC Container](https://github.com/grumpydev/TinyIoC):

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

If you intend to use a [Polly](https://github.com/App-vNext/Polly) Policy to support [Retry and Circuit-Breaker](PolicyRetryAndCircuitBreaker.html) then you will need to register the Policies in the **Policy Registry**. 

This is just the Polly **PolicyRegistry**.

Registration requires a string as a key, that you will use in your [UsePolicy] attribute to choose the policy. 

The two keys: CommandProcessor.RETRYPOLICY and CommandProcessor.CIRCUITBREAKER are used within Brighter to control our response to broker issues. You can override them if you wish to change our behavior from the default.

You can also use them for a generic retry policy, though we recommend building retry policies that handle the kind of exceptions that will be thrown from your handlers.

In this example, we set up a policy. To make it easy to reference the string, instead of adding it everywhere, we use a global readonly reference, not shown here.

``` csharp
var retryPolicy = 
	Policy.Handle<Exception>().WaitAndRetry(
		new[] { 
			TimeSpan.FromMilliseconds(50), 
			TimeSpan.FromMilliseconds(100), 
			TimeSpan.FromMilliseconds(150) });

var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(
		1, TimeSpan.FromMilliseconds(500));

var policyRegistry = new PolicyRegistry() { 
		{ Globals.MYRETRYPOLICY, retryPolicy }, 
		{ Globals.MYCIRCUITBREAKER, circuitBreakerPolicy } 
	};
```

When you attribute your code, you then use the key to attach a specific policy:

``` csharp
[RequestLogging(step: 1, timing: HandlerTiming.Before)]
[UsePolicy(Globals.MYRETRYPOLICY, step: 2)]
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

If you need multiple policies then you can pass them as an array. We evaluate them left to right.

``` csharp
[RequestLogging(step: 1, timing: HandlerTiming.Before)]
[UsePolicy(new [] {Globals.MYRETRYPOLICY, Globals.MYCIRCUITBREAKER}, step: 2)]
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

You need to provide a factory to give us instances of a [Context](UsingTheContextBag.html). If you have no implementation to use, just use the default **InMemoryRequestContextFactory**. Typically you would replace ours if you wanted to support initializing the context outside of our pipeline, for tracing for example.

## Command Processor Builder

All these individual elements can be passed to a **Command Processor Builder** to help build a **Command Processor**. This has a fluent interface to help guide you when configuring Brighter. The result looks like this:

``` csharp
var commandProcessor = CommandProcessorBuilder.With()
    .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
    .Policies(policyRegistry)
    .NoExternalBus()
    .RequestContextFactory(new InMemoryRequestContextFactory())
    .Build();
```
