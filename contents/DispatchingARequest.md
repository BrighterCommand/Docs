# Dispatching Requests

Once you have [implemented your Request Handler](ImplementingAHandler.html), you will want to dispatch **Commands** or **Events** to that Handler.

## Usage

In the following example code we register a handler, create a command processor, and then use that command processor to send a command to the handler.


``` csharp
  public class Program
    {
        private static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.AddBrighter().AutoFromAssemblies();
                })
                .UseConsoleLifetime()
                .Build();

            var commandProcessor = host.Services.GetService<IAmACommandProcessor>();

            commandProcessor.Send(new GreetingCommand("Ian"));

            host.WaitForShutdown();
        }
   }
```

## Registering a Handler

In order for a **Command Dispatcher** to find a Handler for your **Command** or **Event** you need to register the association between that **Command** or **Event** and your Handler.

Brighter's **HostBuilder** support provides **AutoFromAssemblies** to register any *Request Handlers* in the project. See [Basic Configuration](/contents/BrighterBasicConfiguration.md) for more.

### Pipelines Must be Homogeneous

Brighter only supports pipelines that are solely **IHandleRequestsAsync** or **IHandleRequests**.

## Dispatching Requests

Once you have registered your Handlers, you can dispatch requests to them. To do that you simply use the **CommandProcessor.Send()** or a **CommandProcessor.Publish()** method passing in an instance of your command. *Send* expects one handler, *Publish* expects zero or more. (You can use **commandProcessor.DepositPostAsync** and **commandProcessor.ClearOutboxAsync** with an External Bus).

``` csharp
commandProcessor.Send(new GreetingCommand("Ian"));
```

### Returning results to the caller.

A Command does not have return value and **CommandDispatcher.Send()** does not return anything. Please see a discussion on how to handle this in [Returning Results from a Handler](/contents/ReturningResultsFromAHandler.md).

