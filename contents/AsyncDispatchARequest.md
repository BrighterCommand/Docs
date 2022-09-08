# Dispatching Requests Asynchronously

Once you have [implemented your Request Handler](ImplementingAHandler.html), you will want to dispatch **Commands** or **Events** to that Handler.

## Usage

In the following example code we register a handler, create a command processor, and then use that command processor to send a command to the handler asynchronously.


``` csharp
  public class Program
    {
        private static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.AddBrighter()
                        .AutoFromAssemblies();
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

Once you have registered your Handlers, you can dispatch requests to them. To do that you simply use the **commandProcessor.SendAsync()** (or **commandProcessor.PublishAsync()**) method passing in an instance of your command. *Send* expects one handler, *Publish* expects zero or more. (You can use **commandProcessor.DepositPostAsync** and **commandProcessor.ClearOutboxAsync** with an External Bus).

``` csharp
await commandProcessor.SendAsync(new GreetingCommand("Ian"));
```

### Returning results to the caller.

A Command does not have return value and **CommandDispatcher.Send()** does not return anything. Please see a discussion on how to handle this in [Returning Results from a Handler](/contents/ReturningResultsFromAHandler.md).

### Cancellation

Brighter supports the cancellation of asynchronous operations.

The asynchronous methods: **SendAsync** and **PublishAsync** accept a **CancellationToken** and pass this token down the pipeline. The parameter defaults to default(CancellationToken) where the call does not intend to cancel.

The responsibility for checking for a cancellation request lies with the individual handlers, which must determine what action to take if cancellation had been signalled.

### Async Callback Context

When an awaited method completes, what thread runs any completion code? There are two options:

- The original thread that was running when the await began
- A new thread allocated from the thread pool

Why does this matter? Because if you needed to access anything that is thread local, being called back on a new thread means you will not have access to those variables.

As a result, when awaiting it is possible to configure how the continuation runs. 

- To run on the original thread, requires the CLR to capture information on the thread you were using. This is the SynchronizationContext; because the CLR must record this information, we refer to it as a captured context. Your execution will be queued back on to the original context, which has a performance cost.
- To run on a new thread, using the Task Scheduler to allocate from the thread pool.

You can use ConfigureAwait to control this. This article explains why you might wish to use [ConfigureAwait](https://devblogs.microsoft.com/dotnet/configureawait-faq/), in more depth.

As a library, we need to allow you to make this choice for your handler chain. For this reason, our *Async methods support the parameter **continueOnCapturedContext**. 

Library writers are encouraged to default to false i.e. use the Task Scheduler instead of the SychronizationContext. Brighter adopts this default, but it might not be what you want if your handler needs to run in the context of the original thread. As a result we let you use this parameter on the **\*Async** calls to change the behaviour throughout your pipeline.

``` csharp
await commandProcessor.SendAsync(new GreetingCommand("Ian"), continueOnCapturedContext: true);
```

A handler exposes the parameter you supply via  the property **ContinueOnCapturedContext**. 

You should pass this value via **ConfigureAwait** if you need to be able to support making this choice at the call site. For example, when you call the base handler in your return statement, to ensure that the decision as to whether to use the scheduler or the context flows down the pipeline.

``` csharp
return await base.HandleAsync(command, ct).ConfigureAwait(ContinueOnCapturedContext);
```

You can ignore this, if you want to default to using the Task Scheduler.


