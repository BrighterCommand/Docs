# Dispatching Requests

Once you have [implemented your Request Handler](ImplementingAHandler.html), you will want to dispatch **Commands** or **Events** to that Handler.

## Registering a Handler

In order for a **Command Dispatcher** to find a Handler for your **Command** or **Event** you need to register the association between that **Command** or **Event** and your Handler.

The **Subscriber Registry** is where you register your Handlers.

``` csharp
var subscriberRegistry = new SubscriberRegistry();
subscriberRegistry.Register<GreetingCommand, GreetingCommandHandler>();
```

## Dispatching Requests

Once you have registered your Handlers, you can dispatch requests to them. To do that you simply use the **CommandProcessor.Send()** method passing in an instance of your command.

``` csharp
commandProcessor.Send(new GreetingCommand("Ian"));
```

## Building a Command Dispatcher

We associate a **Subscriber Registry** with a **Command Processor** by passing it into the constructor of the **Command Processor**. For convenience, we provide a **Commmand Processor Builder** that helps you
configure new instances of **Command Processor**.

``` csharp
var logger = LogProvider.For<Program>();

var registry = new SubscriberRegistry();
registry.Register<GreetingCommand, GreetingCommandHandler>();


var builder = CommandProcessorBuilder.With()
    .Handlers(new HandlerConfiguration(
        subscriberRegistry: registry,
        handlerFactory: new SimpleHandlerFactory(logger)
    ))
    .DefaultPolicy()
    .NoTaskQueues()
    .RequestContextFactory(new InMemoryRequestContextFactory());

var commandProcessor = builder.Build();
```

We cover [configuration of a Command Processor](BasicConfiguration.html)
in more detail later.

## Returning results to the caller.

We use [Command-Query
separation](https://martinfowler.com/bliki/CommandQuerySeparation.html) so a Command does not have return value and **CommandDispatcher.Send()** does not return anything.

This in turn leads to a set of questions that we need to answer about common scenarios:

-   How do I handle failure? With no return value, what do I do if my  handler fails?
-   How do I communicate the outcome of a command? 

We discuss these issues below.

## Handling Failure

If we don\'t allow return values, what do you do on failure?

-   The basic failure strategy is to throw an exception. This will terminate the request handling pipeline.
-   If you want *Internal Bus* support for [Retry, and Circuit Breaker](PolicyRetryAndCircuitBreaker.html) you can use our support for [Polly](https://github.com/App-vNext/Polly) Policies
-   If you want to Requeue (with Delay) to an *External Bus*, you should throw a **DeferMessageAction** exception.
-   Finally you can use our support for a [Fallback](PolicyFallback.html) handler to provide backstop exception handling.
-   You can also build your own exception handling into your [Pipeline](BuildingAPipeline.html).

We discuss these options in more detail in [Handler Failure](/contents/HandlerFailure.md).

## Communicating the Outcome of a Command

Sometimes you need to provide information to the caller about the outcome of a *Command*, instead of listening for an *Event* an. 

How do you communicate the outcome of handling a *Command*? There are two options, which depend on circumstance:

* Raise an *Event*
* Update a field on the *Command*

### Raising an Event

This approach let's you take action in response to a *Command* by raising an *Event* within your handler using **CommandProcessor.Publish** or via an *External Bus* using **CommandProcessor.Post/CommandProcessor.DepositPost**.

If you use an **Internal Bus** these handlers will run immediately, in their own pipeline, before your handler exits. If you use an **External Bus** you offload the work to another process. 

### Update a field on the Command

If you are using an *Internal Bus* and need a return value from a *Command* you will note that **CommandProcessor.Send** has a void return value, so you cannot return a value from the handler.

What happens if the caller needs to know the outcome, and can't be signalled via an *Event*?

In that case add a property to the **Command** that you can initialize from the Handler. As an example, what happens if you need to return the identity of a newly created entity, so that you can use **Darker** to retrieve its details? In this case you can create a **NewEntityIdentity** property in your command that you write a newly created entity\'s identity to in the Handler, and then inspect the property in your **Command** in the calling code after the call to **commandProcessor.Send(command)** completes.

You can think of these as *out* parameters.

``` csharp
var createTaskCommand = new CreateTaskCommand();
commandProcessor.Send(createTaskCommand);
var newTaskId = createTaskCommand.TaskId;
```

## Using the base class when dispatching a message

All **Command** or **Event** messages derive from **IRequest** and **ICommand** and **IEvent** respectively. So it may seem natural to create a collection of them, for example **List\<IRequest\>**, and then
process a set of messages by enumerating over them.

When you try this, you will encounter the issue that we dispatch based on the concrete type of the **Command** or **Event**. In other words the type you register via the **SubscriberRegistry.** Because
**CommandProcessor.Send()** is actually **CommandProcessor.Send\<T\>()** you need to provide the concrete type in the call for the compiler to determine the type to use with the cool as the concrete type.

If you try this:

``` csharp
ICommand command = new GreetingCommand("Ian");
commandProcessor.Send(command);
```

Then you will get this error: *\"ArgumentException \"No command handler was found for the typeof command Brighter.commandprocessor.ICommand - a command should have exactly one handler.\"\"*

Now, you don\'t see this issue if you pass the concrete type in, so the compiler can correctly resolve the run-time type.

``` csharp
commandProcessor.Send(new GreetingCommand("Ian"));
```

So what can you do if you must pass the base class to the **Command Processor** i.e. because you are using a list.

The workaround is to use the dynamic keyword. Using the dynamic keyword means that the type will be evaluated using RTTI, which will successfully pick up the type that you need.

``` csharp
ICommand command = new GreetingCommand("Ian");
commandProcessor.Send((dynamic)command);
```
