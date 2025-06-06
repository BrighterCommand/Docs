
# Returning Results from a Handler

We use [Command-Query
separation](https://martinfowler.com/bliki/CommandQuerySeparation.html) so a Command does not have return value and **CommandDispatcher.Send()** does not return anything. Our project Darker provides a Query Processor that can be used to return results in response to queries. You can use both together to provide CQRS.

This in turn leads to a set of questions that we need to answer about common scenarios:

-   How do I handle failure? With no return value, what do I do if my  handler fails?
-   How do I communicate the outcome of a command? 

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

This approach lets you take action in response to a *Command* by raising an *Event* within your handler using **CommandProcessor.Publish** or via an *External Bus* using **CommandProcessor.Post/CommandProcessor.DepositPost**.

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
