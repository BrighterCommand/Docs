# FAQ

## Asynchronous or External Bus

When should you use an asynchronous pipeline to handle work and when should you use an External Bus.

Using an asynchronous handler allows you to avoid blocking I/O. This can increase your throughput by allowing you to re-use threads to service new requests. Using this approach, even a single-threaded application can
achieve high throughput, if it is not CPU-bound.

Using an External Bus allows you to hand-off work to another process, to be executed at some point in the future. This also allows you to improve throughput by freeing up the thread to service new requests. We assume that we can accept dealing with that work at some point in the future i.e. we can be eventually consistent.

One disadvantage of an External Bus is that the pattern - ack to callers, and then do the work, can create additional complexity because we must deal with notifying the user of completion, or errors. Because an async operation simply has the caller wait, the programming model is simpler. The trade-off here is that the client of our process is still using resources awaiting for the request with the async operation. If the operation takes time to complete the client may not know if the operation failed and should be timed out, or is still running.

Where work is long-running there is a risk that the server faults, and we lose the long-running work. An External Bus provides reliability here, through guaranteed delivery. The queue keeps the work until it is
successfully processed and acknowledged.

Our recommendation is to use the async pattern to improve throughput where the framework supports async, such as ASP.NET WebAPI but to continue to hand-off work that takes a long time to complete to a work
queue. You may choose to define your own thresholds but we recommend that operations that take longer than 200ms to complete be handed-off. We also recommend that operations that are CPU bound be handed-off as
they diminish the throughput of your application.

## Iterating over a list of requests to dispatch them 

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
