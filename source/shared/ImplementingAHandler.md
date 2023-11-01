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
