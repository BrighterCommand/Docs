# How to Implement an Asynchronous Request Handler

To implement an asynchronous handler, derive from **RequestHandlerAsync\<T\>** where *T* should be the **Command** or **Event** derived type that you wish to handle. Then override the base
class **RequestHandlerAsync\<T\>.HandleAsync()** method to implement your handling for the Command or Event.

For example, assume that you want to handle the **Command** GreetingCommand

``` csharp
public class GreetingCommand : IRequest
{
    public GreetingCommand(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; private set; }
}
```

Then derive your handler from **RequestHandlerAsync\<GreetingCommand\>** and accept a parameter of that type on the overriden **HandleAsync()** method, along with a nullable cancellation token - which you should
default to null.

To ensure that the pipeline runs, you should return the result of the next handler in the chain, by awaiting the base class **HandleAsync()**.

(Because the next element in the pipeline should also be async, you should always await the result of this call.)

``` csharp
public class GreetingCommandRequestHandlerAsync : RequestHandlerAsync<GreetingCommand>
{
    public override async Task HandleAsync(GreetingCommand command, CancellationToken? ct = null)
    {
        var api = new IpFyApi(new Uri("https://api.ipify.org"));

        var result = await api.GetAsync(ct);

        Console.WriteLine("Hello {0}", command.Name);
        Console.WriteLine(result.Success ? "Your public IP addres is {0}" : "Call to IpFy API failed : {0}",
        result.Message);
        return await base.HandleAsync(command, ct).ConfigureAwait(base.ContinueOnCapturedContext);
    }
}
```

Note how we use **ConfigureAwait()** when calling the next handler in the chain, and set the value to the **RequestHandlerAsync\<GreetingCommand\>.ContinueOnCapturedContext** property. This ensures that we utilize any override of the default (which is to use the Task Scheduler) made when the call to **SendAsync**, **PublishAsync**, or **PostAsync** was made.

It is worth noting that although the override forces you to return a **Task\<T\>** it does not force you to add the **async** keyword to the method to compile. This risks introducing a subtle bug. You can await a
method that returns a **Task\<T\>** but creation of the state machine in the caller depends on the presence of the **async** keyword. If your handler does not await anything, you will not be forced to add the
**async** keyword. Your handler will run sychronously in this context, which may not be what you expect.

Remembering to always await the base class **HandleAsync()** mitigates against this as even if your handler does not do asynchronous work, you will be forced to add **async** to the signature.
