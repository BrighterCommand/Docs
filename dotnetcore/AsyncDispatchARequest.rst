Paramore
========

Libraries and supporting examples for use with the Ports and Adapters
and CQRS architectural styles for .NET, with support for Task Queues

`View the Project on GitHub
iancooper/Paramore <https://github.com/iancooper/Paramore>`__

-  `Download **ZIP
   File** <https://github.com/iancooper/Paramore/zipball/master>`__
-  `Download **TAR
   Ball** <https://github.com/iancooper/Paramore/tarball/master>`__
-  `View On **GitHub** <https://github.com/iancooper/Paramore>`__

`Paramore Home <../index.html>`__

`Brighter Home <Brighter.html>`__

`Next <ImplementingAsyncHandler.html>`__

`Prev <AWSSQSConfiguration.html>`__

Brighter
========

Dispatching Requests Asynchronously
-----------------------------------

Brighter supports an asynchronous `Command Dispatcher and Command
Processor <CommandsCommandDispatcherandProcessor.html>`__.

Using an asynchronous approach to dispatch can be valuable when the work
done by a handler can be done concurrently with other work. Instead of
blocking on the call to **Send** or **Publish** the calling thread can
continue to do work, with a continuation executing once the operation
completes. See the MSDN article `Asynchronous Programming with Async and
Await <https://msdn.microsoft.com/en-us/library/hh191443.aspx>`__

Brighter supports using the async...await pattern in .NET to allow your
code to avoid blocking. We provide asynchronous versions of the
**Command Dispatcher** methods i.e. **CommandProcessor.SendAsync()**,
**CommandProcessor.PublishAsync()**, and
**CommandProcessor.PostAsync()**.

Usage
~~~~~

In the following example code we register a handler, create a command
processor, and then use that command processor to send a command to the
handler asynchronously.

Note that this code is the same as the equivalent code for calling the
command processor synchonously - apart from the use of async
alternatives i.e. **SubscriberRegistry.RegisterAsync()** instead of
**SubscriberRegistry.Register()** and **CommandProcessor.SendAsync()**
instead of **CommandProcessor.Send()**.

Note also that we have a **SimpleHandlerFactoryAsync** as this factory
needs to return handlers that implement **IHandleRequestsAsync** not
**IHandleRequests**.

::

    private static async Task MainAsync()
    {
        var registry = new SubscriberRegistry();
        registry.RegisterAsync<GreetingCommand, GreetingCommandRequestHandlerAsync>();

        var builder = CommandProcessorBuilder.With()
        .Handlers(new HandlerConfiguration(registry, new SimpleHandlerFactoryAsync()))
        .DefaultPolicy()
        .NoTaskQueues()
        .RequestContextFactory(new InMemoryRequestContextFactory());

        var commandProcessor = builder.Build();

        await commandProcessor.SendAsync(new GreetingCommand("Ian"));

        Console.ReadLine();
    }

Note that line: **Console.ReadLine()** is a continuation. Control passes
back to the calling method after the await, and subsequent lines of code
run after that method returns.

Registering a Handler
~~~~~~~~~~~~~~~~~~~~~

In order for a **Command Dispatcher** to find a Handler for your
**Command** or **Event** you need to register the association between
that **Command** or **Event** and your Handler.

The **Subscriber Registry** is where you register your Handlers.

The **SubscriberRegistry.RegisterAsync()** expects a handler that
implements **IHandleRequestsAsync**

::

    var registry = new SubscriberRegistry();
    registry.RegisterAsync<GreetingCommand, GreetingCommandRequestHandlerAsync>();
        

Pipelines Must be Homogeneous
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Brighter only supports pipelines that are solely
**IHandleRequestsAsync** or **IHandleRequests**.

This is due to expectation of the caller using an **\*Async** method
that the code will execute asynchronously - allowing some handlers in
the chain to block would defy that expectations. The async...await
pattern is often described as 'viral' because it spreads up the chain of
callers to be effective. Brighter is no exception in this regard.

Dispatching Requests
~~~~~~~~~~~~~~~~~~~~

Once you have registered your Handlers, you can dispatch requests to
them. To do that you simply use the **CommandProcessor.SendAsync()** (or
**CommandProcessor.PublishAsync()** or
**CommandProcessor.PublishAsync()**) method passing in an instance of
your command.

::

        await commandProcessor.SendAsync(new GreetingCommand("Ian"));

Cancellation
^^^^^^^^^^^^

Brighter supports the cancellation of asynchronous operations.

The asynchronous methods: **SendAsync**, **PublishAsync**, and
**PostAsync** all accept a **CancellationToken** and pass this token
down the pipeline. The parameter defaults to null where the call does
not intend to cancel.

The responsibility for checking for a cancellation request lies with the
individual handlers, which must determine what action to take if
cancellation had been signalled.

The ability of the **\*Async** methods to take a cancellation token can
be particularly useful with ASP.NET AsyncTimeout see `here for
more. <http://dotnetcodr.com/2013/01/04/timeout-exceptions-with-asyncawait-in-net4-5-mvc4-with-c/>`__

Do Not Block When Calling \*Async Methods
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

When calling an asynchronous method you should **await** that method.
Avoid using **.Wait** or **.Result** on the **Task** returned by the
method, as this turns it back into a blocking call, which is probably
not your intent and likely undermines the reason you wanted to use an
asynchronous approach in the first place. If you find yourself using
**.Wait** or **.Result** then consider whether you would be better using
a synchronous pipeline instead.

Therefore you should only call **SendAsync**, **PublishAsync**, or
**PostAsync** from a method that is itself async and supports await,
otherwise you will block, and there will be no value to having used an
async method.

In `Ports & Adapters Architecture <PortsAndAdapters.html>`__ terms you
should use an **Adapter** layer that supports async when calling the
**Ports** layer represented by your handlers.

This creates the question: at what point do we stop being async i.e. who
waits? This is normally a responsibility of your framework which has to
understand that it can use re-use thread to service other requests, thus
improving throughput and call back to your continuation when done.

For example ASP.NET Controllers `support
async <http://www.asp.net/mvc/overview/performance/using-asynchronous-methods-in-aspnet-mvc-4>`__
can be used to call the **\*Async methods** without blocking. This
allows ASP.NET to release a thread from the thread pool to service
another request whilst the asynchronous operation completes, allowing
greater throughput on the server.

Understand Captured Contexts
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

When an awaited method completes, what thread runs any completion code?
The answer depends on the SynchronizationContext which is 'captured' at
the point await is called. For ASP.NET or Windows Forms, WPF, or Metro
apps then the SynchronizationContext means that the thread that was
running at the point we yielded runs the continuation. Otherwise the
SynchronizationContext is null and the default Task Scheduler runs the
continuation.

Why does this matter? Because if you needed to access anything that is
thread local, being called back on the wrong thread means you will not
have access to those variables.

A Windows UI for example is single-threaded via a message pump and
interacting with the UI requires you to be on that thread. See `this
article for
more. <http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx>`__

When awaiting it is possible to configure how the continuation runs - on
the SyncronizationContext or using the Task Scheduler, overriding the
default behaviour, which is to capture the SynchronizationContext.

::

    await MethodAsync(value, ct).ConfigureAwait(true);

Library writers are encouraged to default to false i.e. use the Task
Scheduler instead of the SychronizationContext.

Brighter adopts this default, but recognizes it might not be what you
want if your handler needs to run in the context of the original thread.
As a result we let you pass in a parameter on the **\*Async** calls to
change the behaviour throughout your pipeline.

::

    await commandProcessor.SendAsync(new GreetingCommand("Ian"), continueOnCapturedContext: true);

A handler exposes the parameter you supply to the call to **SendAsync**,
**PublishAsync**, or **PostAsync** via a property called
**ContinueOnCapturedContext**. That property is true if we want to use
the SynchronizationContext and not the Task Scheduler to run our
continuation.

::

    await base.HandleAsync(command, ct).ConfigureAwait(ContinueOnCapturedContext);

We recommend explicitly using this parameter when awaiting within your
own handler, such as when calling the next handler in an async pipeline.

Asynchronous vs. Work Queues
~~~~~~~~~~~~~~~~~~~~~~~~~~~~

One obvious question is: when should I use an asynchronous pipeline to
handle work and when should I use a work queue.

Using an asynchronous handler allows us to avoid blocking. This can
increase our throughput by allowing us to re-use threads to service new
requests. Using this approach, even a single-threaded application can
achieve high throughput, if it is not CPU-bound.

Using a work queue allows us to hand-off work to another process, to be
executed at some point in the future. This also allows us to improve
throughput by freeing up the thread to service new requests. We assume
that we can accept dealing with that work at some point in the future
i.e. we can be eventually consistent.

One disadvantage of a work queue is that our pattern - ack to callers,
and then do the work, can create additional complexity because we must
deal with notifying the user of completion, or errors. Because an async
operation simply has the caller wait, the programming model is simpler.
The trade-off here is that the client of our process is still using
resources awaiting for the request with the async operation. If the
operation takes time to complete the client may not know if the
operation failed and should be timed out, or is still running.

Where work is long-running there is a risk that the server faults, and
we lose the long-running work. A work queue provides reliability here,
through guaranteed delivery. The queue keeps the work until it is
successfully processed and acknowledged.

Our recommendation is to use the async pattern to improve throughput
where the framework supports async, such as ASP.NET WebAPI but to
continue to hand-off work that takes a long time to complete to a work
queue. You may choose to define your own thresholds but we recommend
that operations that take longer than 200ms to complete be handed-off.
We also recommend that operations that are CPU bound be handed-off as
they diminish the throughput of your application.

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

