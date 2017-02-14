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

`Next <GreetingsExample.html>`__

`Prev <Introduction.html>`__

Brighter
========

Tutorial
--------

This tutorial takes you building a Hello World project. The walkthrough
will build the example availabe in the Examples folder of Brighter
available in the public repo at `Hello World
Example <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/HelloWorld>`__
if you want to follow along there instead of typing in the code.

Step One
~~~~~~~~

Create a C# Console Application, targeting .NET 4.5. (Note that you can
use any kind of host application, a console application is just the
simplest way to get up and running in an example.

|image0|

Step Two
~~~~~~~~

Install the **Paramore.Brighter.CommandProcessor** package from NuGet

-  PM> Install-Package Paramore.Brighter.CommandProcessor

|image1|

Step Three
~~~~~~~~~~

Add the following code to the Main class:

::

    var registry = new SubscriberRegistry();

    var builder = CommandProcessorBuilder.With()
        .Handlers(new HandlerConfiguration(
                subscriberRegistry: registry,
                handlerFactory: new SimpleHandlerFactory(logger)
            ))
        .DefaultPolicy()
        .NoTaskQueues()
        .RequestContextFactory(new InMemoryRequestContextFactory());

    var commandProcessor = builder.Build();
             

Requests and Request Handlers
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A *Command Dispatcher* works by looking up registered handlers for the
command or event and forwarding the request to them, i.e.
publish-subscribe.

For this you need: to publish a Command or Event derived from
**IRequest** and implement a subscribing handler that derives from
**IHandleRequests<TRequest>** where TRequest is the type of Command or
Event you are subscribing to.

You use the **SubscriberRegistry** class to register those
subscriptions. So for a command *GreetingCommand* we expect you to
register a type derived from *IHandleRequests<GreetingCommand>*, let's
call it *GreetingCommandHandler*, using the Register method on the
SubscriberRegistry e.g. *subscriberRegistry.Register<GreetingCommand,
GreetingCommandHandler>()*.

Handler Factories
^^^^^^^^^^^^^^^^^

At runtime, when you send a message to Brighter, it builds a pipeline to
handle requests. To do this it looks up all the handlers you just
registered with the subscriber registry for your command or event.

Once we have found one or more registered handlers for the type of the
request we need to create instances of them. This is complicated by your
handler having its own dependencies which need to be created, which may
have their own dependencies and so on.

We don't know how to construct your handler so we call a factory, that
you provide us, to build this entire dependency chain. This factory
needs to implement the interface defined in **IAmAHandlerFactory**.

Brighter manages the lifetimes of handlers, as we consider the request
pipeline to be a scope, and we will call your factory again asking to
release those handlers once we have terminated the pipeline and finished
processing the request. You should take appropriate action to clear up
the handler and its dependencies in response to that call

It's worth reading Mark Seeman's article on `DI Friendly
Frameworks <http://blog.ploeh.dk/2014/05/19/di-friendly-framework/>`__
to understand this technique. Brighter originally used a conforming
container but switched to user defined factories as per Mark's blog.

The Command Processor Builder
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

With this example creating an instance of the Command Processor is
straightforward: as we provide a builder with a fluent interface to help
you set up the builder. Because we ask you to provide factories where we
need to create objects in your project, there are a number of items that
you will need to configure. But you do this once, at the composition
root, are then done with it.

Simple Handler Factory
^^^^^^^^^^^^^^^^^^^^^^

In this case, the handler factory does not need to be implemented using
an IoC container. We'll show that in a fuller example, but here it is
trivial. For this simple program we just add this it the Program.cs
file.

::

    internal class SimpleHandlerFactory : IAmAHandlerFactory
    {
        public IHandleRequests Create(Type handlerType)
        {
            //TODO: Create an instance of the request type
        }

        public void Release(IHandleRequests handler)
        {
        }
    }
            

When we create a command processor we need the registry of subscribers
to message types, and the factory for creating those handlers, that we
discussed above.

Finally we want to give it a request context - a data structure passed
to each handler in the chain, with global information including a
property bag. Unless you have a need to override it, just use the
default **InMemoryRequestContextFactory** to provide instances of a
suitable context (overriding this is an advanced option - particularly
useful for testing).

Optionally we may include a registry of
`Polly <https://github.com/michael-wolfenden/Polly>`__ policies, which
you can use for Quality of Service issues (more elsewhere) and provide
support for task queues - that is handling work asynchronously by
queuing it for later execution by one or or more worker processes). We
do not need either here.

Step Four
~~~~~~~~~

Our First Command
^^^^^^^^^^^^^^^^^

Now that you have a command processor, we want to create a message, and
a handler for that message. We'll choose to implement a command, that in
the finest tradition of demo applications just displays hello [name} on
the console.

Add a new class to the project called GreetingCommand and enter the
following code:

::

     class GreetingCommand : IRequest
     {
        public GreetingCommand(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; private set; }
     }
             

Our First Handler
^^^^^^^^^^^^^^^^^

Now that we have a Command we need to write a handler for it.

We recommend using the RequestHandler abstract base class to implement
your derived class from **IHandleRequests<TRequest>** as it handles the
basic responsibilities of a handler in the pipeline.

Add a new class to the project called GreetingCommandHandler and then
enter the following code:

::

     class GreetingCommandHandler : RequestHandler<GreetingCommand>
     {
        public override GreetingCommand Handle(GreetingCommand command)
        {
            Console.WriteLine("Hello {0}", command.Name);
            return base.Handle(command);
        }
     }
             

You could directly implement the base class, but as we provide useful
functionality to make sure that your handler participates in the handler
pipeline correctly you should derive from this class unless you have a
compelling reason to implement that support yourself.

As each handler participates in the chain it is expected that you will
return the input you were given, the command or event, so that the next
handler in the chain can also process the request. We call the base
class Handle() method at the end, as this calls the next handler in the
pipeline for you, if there is one. In this case there is no handler, so
you could get away with just returning the Command argument, but calling
the base method is a good habit to form, as it allows you to later chain
together handlers.

In a 'real' application you would load your domain model's state from
persistent storage here, process the request using your domain model and
then save the state of the domain model. See `Commands, Command
Dispatcher and Processor <CommandsCommandDispatcherandProcessor.html>`__
for more on this idiom.

Step Four
~~~~~~~~~

Having created a handler we have to tell Brighter about it. So we need
to add it to the subscriber registry, we added above. Modify the code in
Main as follows:

::

    var registry = new SubscriberRegistry();
    registry.Register<GreetingCommand, GreetingCommandHandler>();
            

We also need to tell the handler factory how to build an instance of
this class on request. We go for a simple implementation here, just to
get up and running. This is obviously not production code. replace the
TODO in the Handler Factory above with the following code

::

    public IHandleRequests Create(Type handlerType)
    {
        return new GreetingCommandHandler();
    }
            

Step Five
~~~~~~~~~

Now that we have a handler registered, it is time to send it a message.
The command processor exposes a send for point-to-point messaging
(usually a command would have one handler), and publish for broadcast to
zero or more handlers (usually an event has zero or more handlers)

::

    commandProcessor.Send(new GreetingCommand("Ian"));
            

Step Six
~~~~~~~~

let's just review the code. It's not a fine example of software
development, but it serves to show you how Brighter works without any
fuss

::

    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogProvider.For<Program>();

            var registry = new SubscriberRegistry();
            registry.Register<GreetingCommand, GreetingCommandHandler>();

            var builder = CommandProcessorBuilder.With()
                .Handlers(new HandlerConfiguration(
                .Handlers(new HandlerConfiguration(
                        subscriberRegistry: registry,
                        handlerFactory: new SimpleHandlerFactory(logger)
                    ))
                .DefaultPolicy()
                .NoTaskQueues()
                .RequestContextFactory(new InMemoryRequestContextFactory());

            var commandProcessor = builder.Build();

            commandProcessor.Send(new GreetingCommand("Ian"));
        }

        internal class SimpleHandlerFactory : IAmAHandlerFactory
        {
            public IHandleRequests Create(Type handlerType)
            {
                return new GreetingCommandHandler();
            }

            public void Release(IHandleRequests handler)
            {
            }
        }
    }

    class GreetingCommand : IRequest
    {
        public GreetingCommand(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; private set; }
    }

    class GreetingCommandHandler : RequestHandler<GreetingCommand>
    {
        public override GreetingCommand Handle(GreetingCommand command)
        {
            Console.WriteLine("Hello {0}", command.Name);
            return base.Handle(command);
        }
    }
            

Step Seven
~~~~~~~~~~

Now just build and run. You should see your greeting pumped out to the
console.

Next Steps
~~~~~~~~~~

That is a brief introduction in how to get a command processor working.
We explore how to work with a Task Queue in the `Greetings
Example <GreetingsExample.html>`__

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

.. |image0| image:: images/HelloWorld-Step1-ConsoleProject.png
.. |image1| image:: images/NuGet-Brighter.png

