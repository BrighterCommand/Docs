# How to Implement a Request Handler

To implement a handler, derive from RequestHandler\<T\> where T should be the **Command** or **Event** derived type that you wish to handle. Then override the base class Handle() method to implement your handling
for the Command or Event.

For example, assume that you want to handle the **Command** GreetingCommand

``` csharp
public class GreetingCommand : Command
{
    public GreetingCommand(string name)
        : base(Guid.NewGuid())
    {
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; private set; }
}
```

Then derive your handler from **RequestHandler\<GreetingCommand\>** and accept a parameter of that type on the overriden **Handle()** method.

``` csharp
public class GreetingCommandHandler : RequestHandler<GreetingCommand>
{
    public override GreetingCommand Handle(GreetingCommand command)
    {
        Console.WriteLine("Hello {0}", command.Name);
        return base.Handle(command);
    }
}
```

## What is the difference between a Command and an Event?

We use the term **Request** for a data object containing parameters that you want to dispatch to a handler. Brighter uses the interface **IRequest** for this concept. Confusingly, both **Command** or
**Event** which implement **IRequest** are examples of the [Command Pattern](https://brightercommand.github.io/Brighter/CommandsCommandDispatcherandProcessor.html). It is easiest to say that within Brighter **IRequest** is the abstraction that represents the Command from the [Command Pattern](https://brightercommand.github.io/Brighter/CommandsCommandDispatcherandProcessor.html).

Why have both **Command** and **Event**? The difference is in how the **Command Dispatcher** dispatches them to handlers.

-   A **Command** is an imperative instruction to do something; it only has one handler. We will throw an error for multiple registered handlers of a command.
-   An **Event** is a notification that something has happened; it has zero or more handlers.

The difference is best explained by the following analogy. If I say \"Bob, make me a cup of coffee,\" I am giving a Command, an imperative instruction. My expectation is that Bob will make me coffee. If Bob does
not, then we have a failure condition (and I am thirsty and cranky). If I say \"I could do with a cup of coffee,\" then I am indicating a state of thirst and caffeine-withdrawal. If Bob or Alice make me a coffee I will be very grateful, but there is no expectation that they will.

So choosing between **Command** or **Event** effects how the **Command Dispatcher** routes requests.

See [Dispatching a Request](DispatchingARequest.html) for more on how to dispatch **Requests** to handlers.
