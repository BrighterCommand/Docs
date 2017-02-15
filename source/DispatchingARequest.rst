Dispatching Requests
--------------------

Once you have `implemented your Request
Handler <ImplementingAHandler.html>`__, you will want to dispatch
**Commands** or **Events** to that Handler.

Registering a Handler
~~~~~~~~~~~~~~~~~~~~~

In order for a **Command Dispatcher** to find a Handler for your
**Command** or **Event** you need to register the association between
that **Command** or **Event** and your Handler.

The **Subscriber Registry** is where you register your Handlers.

.. highlight:: csharp

::

    var subscriberRegistry = new SubscriberRegistry();
    subscriberRegistry.Register<GreetingCommand, GreetingCommandHandler>();


Dispatching Requests
~~~~~~~~~~~~~~~~~~~~

Once you have registered your Handlers, you can dispatch requests to
them. To do that you simply use the **CommandProcessor.Send()** method
passing in an instance of your command.

.. highlight:: csharp

::

    commandProcessor.Send(new GreetingCommand("Ian"));


Building a Command Dispatcher
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

We associate a **Subscriber Registry** with a **Command Processor** by
passing it into the constructor of the **Command Processor**. For
convenience, we provide a **Commmand Processor Builder** that helps you
configure new instances of **Command Processor**.

.. highlight:: csharp

::

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


We cover `configuration of a **Command
Processor** <BasicConfiguration.html>`__ in more detail later.

Returning results to the caller.
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

We use `Command-Query
separation <http://martinfowler.com/bliki/CommandQuerySeparation.html>`__
so a Command does not have return value and **CommandDispatcher.Send()**
does not return anything.

This in turn leads to a set of questions that we need to answer about
common scenarios:

-  How do I handle failure? With no return value, what do I do if my
   handler fails
-  How do I pass information back to the caller? Creation scenarios
   particularly seem to require the caller knows about identies for
   created entities.

We discuss these issues below.

Handling Failure
~~~~~~~~~~~~~~~~

If we don't allow return values, what do you do on failure?

-  The basic failure strategy is to throw an exception. This will
   terminate the request handling pipeline.
-  If you want to support `Retry, and Circuit
   Breaker <PolicyRetryAndCircuitBreaker.html>`__ you can use our
   support for `Polly <https://github.com/michael-wolfenden/Polly>`__
   Policies
-  You can also build your own exception handling into your
   `Pipeline <BuildingAPipeline.html>`__.
-  Finally you can use our support for a
   `Fallback <PolicyFallback.html>`__ handler to provide backstop
   exception handling.

Passing Information to the Caller
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Sometimes you need to provide information to the caller about the
success of the operation. The most common requirement is the Identity of
a new created Entity so that you can query for it. For example you are
implementing a REST API and in response to a POST request you create a
new entity and want to return the entity body in the HTTP response body.

The best approach is to generate the Identity to use for the new Entity
and pass that as a parameter on the **Command**, such as Guid or Hi-Lo
identity.

But what if you are not be able to do this or want to support it for
performance reasons?

In that case add a property to the **Command** that you can initialize
from the Handler, for example create a **NewEntityIdentity** property in
your command that you write the new entity's identity to in the Handler,
and then inspect the property in your **Command** in the calling code
after the call to **CommandDispatcher.Send()** completes.

Note that you cannot use this strategy with **CommandDispatcher.Send()**
as you have no way to update the **Command** in process.

Using the base class when dispatching a message
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

All **Command** or **Event** messages derive from **IRequest** and
**ICommand** and **IEvent** respectively. So it may seem natural to
create a collection of them, for example **List<IRequest>**, and then
process a set of messages by enumerating over them.

When you try this, you will encounter the issue that we dispatch based
on the concrete type of the **Command** or **Event**. In other words the
type you register via the **SubscriberRegistry.** Because
**CommandProcessor.Send()** is actually **CommandProcessor.Send<T>()**
you need to provide the concrete type in the call for the compiler to
determine the type to use with the cool as the concrete type.

If you try this:

.. highlight:: csharp

::

    ICommand command = new GreetingCommand("Ian");
    commandProcessor.Send(command);

Then you will get this error: *"ArgumentException "No command handler
was found for the typeof command
paramore.brighter.commandprocessor.ICommand - a command should have
exactly one handler.""*

Now, you don't see this issue if you pass the concrete type in, so the
compiler can correctly resolve the run-time type.

.. highlight:: csharp

::

    commandProcessor.Send(new GreetingCommand("Ian"));

So what can you do if you must pass the base class to the **Command
Processor** i.e. because you are using a list.

The workaround is to use the dynamic keyword. Using the dynamic keyword
means that the type will be evaluated using RTTI, which will
successfully pick up the type that you need.

.. highlight:: csharp

::

    ICommand command = new GreetingCommand("Ian");
    commandProcessor.Send((dynamic)command);


See `this
discussion <https://github.com/iancooper/Paramore/issues/116>`__ for
more.
