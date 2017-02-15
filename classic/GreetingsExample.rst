Greetings Example
=================

This tutorial takes you building the Greetings project, which is Hello
World via a Task Queue. The walkthrough will build the example availabe
in the Examples folder of Brighter available in the public repo at
`Greetings
Example <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/Greetings>`__
if you want to follow along there instead of typing in the code.

Note that you will need to have
`RabbitMQ <https://www.rabbitmq.com/download.html>`__ installed to step
through this example as a tutorial.

Step One
~~~~~~~~

Create a C# Console Application, targeting .NET 4.5.

Note that you can use any kind of application with
Brighter.ServiceActivator, it's purpose is to take messages of a task
queue and route to a Request Handler seamlessly. We use a console
application as we intend to build a Windows Service which is one of the
most common use cases for the `Service Activator
pattern <http://www.eaipatterns.com/MessagingAdapter.html>`__.

|image0|

Step Two
~~~~~~~~

Install the **Paramore.Brighter.ServiceActivator** package from NuGet

-  PM> Install-Package paramore.brighter.serviceactivator

|image1|

This will install **Paramore.Brighter.Serviceactivator** and it's
dependencies **Paramore.Brighter.CommandProcessor**, **Polly**, and
**Newtonsoft.Json**.

Although the Service Activator provides support for a consumer reading
messages of an Input Channel, we need to supply a concrete
implementation of **IAmAMessageConsumer** which abstracts the
Message-Oriented-Middleware used by that Input Channel for the task
queue implementation.

Note that your app.config file will be updated by this install. Although
you have no channels as yet, the configuration section for you to add
them has been added.

::

    <?xml version="1.0" encoding="utf-8"?>
        <configuration>
            <configSections>
                <section name="serviceActivatorConnections"
    type="paramore.brighter.serviceactivator.ServiceActivatorConfiguration.ServiceActivatorConfigurationSection, paramore.brighter.serviceactivator"
    allowLocation="true" allowDefinition="Everywhere"/>
            </configSections>
            <serviceActivatorConnections>
                <connections>
                </connections>
            </serviceActivatorConnections>
        </configuration>
            

Install the **Paramore.Brighter.CommandProcessor.MessagingGateway.RMQ**
package from NuGet

-  PM> Install-Package
   paramore.brighter.commandprocessor.messaginggateway.rmq

|image2|

This will install **RabbitMQ.Client** as a dependency.

This will install the
Paramore.Brighter.CommandProcessor.MessageingGateway.RMQ package which
provides support for a Task Queue implemented in `Rabbit
MQ <http://www.rabbitmq.com/>`__

Step Three
~~~~~~~~~~

Use Topshelf to run the console application as a service

Install the `TopShelf <http://topshelf-project.com/>`__ package from
NuGet

-  PM> Install-Package Topshelf

|image3| |image4|

You will need to accept the licence for
`Topshelf <http://topshelf-project.com/>`__. We don't go into detail on
Topshelf here, please see that package's own documentation for how to
use it in more depth.

Step Four
~~~~~~~~~

We use `TinyIoC <https://github.com/grumpydev/TinyIoC>`__ as a DI
container within Greetings, so we need to add that package into the
solution as well. Brighter is a `DI Friendly
Frameworks <http://blog.ploeh.dk/2014/05/19/di-friendly-framework/>`__
so you can use the DI container of your choice with Brighter.

-  PM> Install-Package TinyIoC

|image5|

Step Five
~~~~~~~~~

Brighter uses `LibLog <https://github.com/damianh/LibLog>`__ to abstract
the implementation details of a client's logger. Greetings uses log4net
as the concrete logger so we need to add a NuGet reference to that
project too.

-  PM> Install-Package LibLog

|image6|

Step Six
~~~~~~~~

We use boiler plate code to implement the Main method to configure the
Topshelf service.

::

    public static void Main()
    {
        /*
        * Send a message in this format to this service and it will print it out
        * We document this here so that you can simply paste this into the RMQ web portal
        * to see commands flowing through the system.
        * {"Greeting":"hello world","Id":"0a81cbbc-5f82-4912-99ee-19f0b7ee4bc8"}
        */

        HostFactory.Run(x => x.Service<GreetingService>(sc =>
        {
            sc.ConstructUsing(() => new GreetingService());

            // the start and stop methods for the service
            sc.WhenStarted((s, hostcontrol) => s.Start(hostcontrol));
            sc.WhenStopped((s, hostcontrol) => s.Stop(hostcontrol));

            // optional, when shutdown is supported
            sc.WhenShutdown((s, hostcontrol) => s.Shutdown(hostcontrol));
        }));
    }
            

A summary of this code is: it provides callbacks for Topshelf to call in
response to OS instructions to a Windows Service to start, stop or
shutdown. In other words it configures how we respond to service
lifetime events. We use a class called GreetingService to implement our
response.

Step Seven
~~~~~~~~~~

We now need to implement the GreetingsService to respond to the control
messages. Add a new class to the project called GreetingService and
enter the following code:

::

    using System;
    using Greetings.Ports.CommandHandlers;
    using Greetings.Ports.Commands;
    using Greetings.Ports.Mappers;
    using paramore.brighter.commandprocessor;
    using paramore.brighter.commandprocessor.Logging;
    using paramore.brighter.commandprocessor.messaginggateway.rmq;
    using paramore.brighter.serviceactivator;
    using Polly;
    using TinyIoC;
    using Topshelf;

    namespace Greetings.Adapters.ServiceHost
    {
        internal class GreetingService : ServiceControl
        {
            private Dispatcher _dispatcher;

            public GreetingService()
            {
                log4net.Config.XmlConfigurator.Configure();

                var container = new TinyIoCContainer();

                var handlerFactory = new TinyIocHandlerFactory(container);
                var messageMapperFactory = new TinyIoCMessageMapperFactory(container);
                container.Register<IHandleRequests<GreetingCommand>, GreetingCommandHandler>();

                var subscriberRegistry = new SubscriberRegistry();
                subscriberRegistry.Register<GreetingCommand, GreetingCommandHandler>();

                //create policies
                var retryPolicy = Policy
                .Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromMilliseconds(50),
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(150)
                    });

                var circuitBreakerPolicy = Policy
                    .Handle<Exception>()
                        .CircuitBreaker(1, TimeSpan.FromMilliseconds(500));

                        var policyRegistry = new PolicyRegistry()
                        {
                            {CommandProcessor.RETRYPOLICY, retryPolicy},
                            {CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy}
                        };

                //create message mappers
                var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
                {
                    {typeof(GreetingCommand), typeof(GreetingCommandMessageMapper)}
                };

                //create the gateway
                var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(logger);
                var builder = DispatchBuilder
                    .With()
                    .CommandProcessor(CommandProcessorBuilder.With()
                        .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
                        .Policies(policyRegistry)
                        .NoTaskQueues()
                        .RequestContextFactory(new InMemoryRequestContextFactory())
                        .Build()
                    )
                    .MessageMappers(messageMapperRegistry)
                    .ChannelFactory(new InputChannelFactory(rmqMessageConsumerFactory))
                    .ConnectionsFromConfiguration();
                 _dispatcher = builder.Build();
            }

            public bool Start(HostControl hostControl)
            {
            _dispatcher.Receive();
            return true;
            }

            public bool Stop(HostControl hostControl)s
            {
            _dispatcher.End().Wait();
            _dispatcher = null;
            return false;
            }

            public void Shutdown(HostControl hostcontrol)
            {
                if (_dispatcher != null)
                    _dispatcher.End();
                return;
            }
        }
     }
            

The key behavior of Greeting is to configure the **Command Processor**
and the **Dispatcher**. We covered the basics of the CommandProcessor in
the `Hello World Example <HelloWorldExample.html>`__.

We use a **DispatchBuilder** to build a **Dispatcher**, which dispatches
messages from a `Task Queue <ImplementingDistributedTaskQueue.html>`__
to a Command Handler. The principle is that once configured you can send
messages to handlers in the service without having to write the
infrastructure code around reading from a queue, translating the message
body into an IRequest (Command or Event), and dispatching to a handler.
The goal here is that the task queue should remain transparent to the
developer, who simply uses **IAmACommandProcessor.Post** to send a
message from one process and then uses the **Dispatcher** to read that
same message and pass to a handler in another.

We create a **Command Processor** as part of creating our **Dispatcher**
to map de-serialized Commands or Events to handlers. Note that it may
seem counter-intuitive that we set no Task Queue on the Command
Processor. This is because we are not sending to a task queue from this
service, just reading, so we do not need to configure **Command
Processor** for sending only receiving. The `Tasks
Example <TasksExample.html>`__ shows an application that has both
sending and receiving components.

We add both a `Retry Policy and a Circuit Breaker
Policy <QualityOfServicePatterns.html>`__ using the
`Polly <https://github.com/michael-wolfenden/Polly>`__ library. We
create policies to decide what intervals to retry at in the event of
failure, and how long to break a circuit for in the presence of
persistent failure. We register these policies in the
**PolicyRegistry**, using the well-known names
**CommandProcessor.RETRYPOLICY** and
**CommandProcessor.CIRCUITBREAKER**. Internally, CommandProcessor uses
the policies you register when you call **IAmACommandProcessor.Post** to
push a message onto a Task Queue, but you can re-use them yourself. As
discussed above, we are not doing a Post here.

(You can also use policies in your own handlers as discussed
`here <PolicyRetryAndCircuitBreaker.html>`__).

We register implementations of **IAmAMessageMapper** with the
**MessageMapperRegistry** to map the message body from the Task Queue
into Commands and Events. In this case we only have one:
**GreetingCommandMessageMapper** which we use to map a
**GreetingCommand** to and from the message body (as JSON).

In order to read messages from a Task Queue we need a
**IAmAMessageConsumerFactory**. In this case we are reading from a
RabbitMQ Task Queue so we use **RmqMessageConsumerFactory**. We set this
as the parameter to an **InputChannelFactory** and pass to the
**DispatchBuilder**

The **Input Channel** is an abstraction over the stream from which we
read messages - mostly implemented using Message-Oriented Middleware -
and **Dispatcher** uses the **InputChannelFactory** to create instances
of the stream to read from, as specified in configuration. We pass the
application protocol specific factory to this, so that we can create
input channels for that protocol. The use of abstraction is intended to
allow support for different protocols and implementations of those
protocols to be used as the stream that underlies the Task Queue

As outlined in `Hello World <HelloWorldExample.html>`__ our goal is to
be a `DI Friendly
Frameworks <http://blog.ploeh.dk/2014/05/19/di-friendly-framework/>`__
so we rely on the client implementing a factory to provide instances of
handlers and message mappers to us. In this example we use
`TinyIoC <https://github.com/grumpydev/TinyIoC>`__ as our DI framework
and implement the required factories using that DI framework.

Step Eight
~~~~~~~~~~

Add a TinyIocHandlerFactory class to the project and enter the following
code

::

    using System;
    using paramore.brighter.commandprocessor;
    using TinyIoC;

    namespace Greetings.Adapters.ServiceHost
    {
        internal class TinyIocHandlerFactory : IAmAHandlerFactory
        {
           private readonly TinyIoCContainer _container;

        public TinyIocHandlerFactory(TinyIoCContainer container)
        {
            _container = container;
        }

        public IHandleRequests Create(Type handlerType)
        {
            return (IHandleRequests)_container.Resolve(handlerType);
        }

        public void Release(IHandleRequests handler)
        {
            var disposable = handler as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
                handler = null;
            }
        }
    }
        

Add a TinyIoCMessageMapperFactory class to the project and enter the
following code

::

    using System;
    using paramore.brighter.commandprocessor;
    using TinyIoC;

    namespace Greetings.Adapters.ServiceHost
    {
        internal class TinyIoCMessageMapperFactory : IAmAMessageMapperFactory
        {
            private readonly TinyIoCContainer _container;

            public TinyIoCMessageMapperFactory(TinyIoCContainer container)
            {
                _container = container;
            }

            public IAmAMessageMapper Create(Type messageMapperType)
            {
                return (IAmAMessageMapper)_container.Resolve(messageMapperType);
            }
        }
    }
            

Step Nine
~~~~~~~~~

Now we need to add the GreetingCommand itself. Add a new class
GreetingCommand to the project and enter the following code.

::

    using System;
    using paramore.brighter.commandprocessor;

    namespace Greetings.Ports.Commands
    {
        public class GreetingCommand : Command
        {
            public GreetingCommand() : base(Guid.NewGuid()) { }

            public GreetingCommand(string greeting) : base(Guid.NewGuid())
            {
                Greeting = greeting;
            }

         public string Greeting { get; set; }
        }
    }
            

We simply derive our class from **Command** and add a property that
allows you to set the Greeting which we intend to send.

Step Ten
~~~~~~~~

Once we have a command we need to add the code for its **Message
Mapper** which we use to de-serialize the message from the wire
protocol. Add a class GreetingCommandMessageMapper to the project.

::

    using Greetings.Ports.Commands;
    using Newtonsoft.Json;
    using paramore.brighter.commandprocessor;

    namespace Greetings.Ports.Mappers
    {
       internal class GreetingCommandMessageMapper : IAmAMessageMapper<GreetingCommand>
       {
           public Message MapToMessage(GreetingCommand request)
           {
               var header = new MessageHeader(messageId: request.Id, topic: "greeting.command", messageType: MessageType.MT_COMMAND);
               var body = new MessageBody(JsonConvert.SerializeObject(request));
               var message = new Message(header, body);
               return message;
           }

           public GreetingCommand MapToRequest(Message message)
           {
            return JsonConvert.DeserializeObject<GreetingCommand>(message.Body.Value);
           }
       }
    }
            

A message has a header - where we write metadata about the message - and
a body - where we write the contents of the message.

When mapping to a message, on the header, we set the **Message Type** to
**MT\_COMMAND** because we want only one handler in the target to
receive the message. The topic is used for routing subscribers to the
message use the topic to indicate their interest in receiving the
message

The body of the message is a JSON string representing the
GreetingCommand

Because we don't send from this service, we don't need MapToMessage and
could simply throw a NotImplemented exception instead.

When mapping back to a request we simply serialize the entity body into
the Command we want to raise.

Step Eleven
~~~~~~~~~~~

Now we need to add the handler, which actually does the work. Add a new
class GreetingCommandHandler to the project

::

    using System;
    using Greetings.Ports.Commands;
    using paramore.brighter.commandprocessor;
    using paramore.brighter.commandprocessor.Logging;

    namespace Greetings.Ports.CommandHandlers
    {
        internal class GreetingCommandHandler : RequestHandler<GreetingCommand>
        {
            public override GreetingCommand Handle(GreetingCommand command)
            {
                Console.WriteLine("Received Greeting. Message Follows");
                Console.WriteLine("----------------------------------");
                Console.WriteLine(command.Greeting);
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Message Ends");
                return base.Handle(command);
            }
        }
    }
            

We derive from **RequestHandler** to reduce the boiler plate code we
need to write, and override the **Handle()** method to provide an
implementation that just echoes the greeting out to the console.

Step Twelve
~~~~~~~~~~~

Build the project

Step Fourteen
~~~~~~~~~~~~~

Now we need to configure the service to read from the input channels

Amend your app.config file as follows

Add the following to your configSections, for the RMQ consumer and
log4net

::

    <section name="rmqMessagingGateway" type="paramore.brighter.commandprocessor.messaginggateway.rmq.MessagingGatewayConfiguration.RMQMessagingGatewayConfigurationSection, paramore.brighter.commandprocessor.messaginggateway.rmq" allowLocation="true" allowDefinition="Everywhere" />
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
            

Add the rmwMessagingGateway section and the serviceActivatorConnections,
which configures both the AMQP URI for your RabbitMQ server (amend if
you are not using defaults) and the channel over which you subscribe to
messages

::

    <rmqMessagingGateway>
        <amqpUri uri="amqp://guest:guest@localhost:5672/%2f" />
        <exchange name="paramore.brighter.exchange" />
    </rmqMessagingGateway>
    <serviceActivatorConnections>
        <connections>
            <add connectionName="paramore.example.greeting" channelName="greeting.command" routingKey="greeting.command" dataType="Greetings.Ports.Commands.GreetingCommand" timeOutInMilliseconds="200" />
        </connections>
    </serviceActivatorConnections>
            

We also need to configure log4net:

::

    <log4net>
    <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
    <conversionPattern value="%date [%thread] %-5level %logger %ndc - %message%newline" />
    </layout>
    </appender>
    <root>
    <level value="DEBUG" />
    <appender-ref ref="ConsoleAppender" />
    </root>
    </log4net>
            

For convenience, the app.config should look like this:

::

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <configSections>
            <section name="serviceActivatorConnections" type="paramore.brighter.serviceactivator.ServiceActivatorConfiguration.ServiceActivatorConfigurationSection, paramore.brighter.serviceactivator" allowLocation="true" allowDefinition="Everywhere"/>
            <section name="rmqMessagingGateway" type="paramore.brighter.commandprocessor.messaginggateway.rmq.MessagingGatewayConfiguration.RMQMessagingGatewayConfigurationSection, paramore.brighter.commandprocessor.messaginggateway.rmq" allowLocation="true" allowDefinition="Everywhere" />
            <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
        </configSections>
        <log4net>
            <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
                <layout type="log4net.Layout.PatternLayout">
                   <conversionPattern value="%date [%thread] %-5level %logger %ndc - %message%newline" />
                </layout>
            </appender>
            <root>
                <level value="DEBUG" />
                <appender-ref ref="ConsoleAppender" />
            </root>
        </log4net>
        <runtime>
        </runtime>
        <rmqMessagingGateway>
            <amqpUri uri="amqp://guest:guest@localhost:5672/%2f" />
            <exchange name="paramore.brighter.exchange" />
        </rmqMessagingGateway>
        <serviceActivatorConnections>
            <connections>
                <add connectionName="paramore.example.greeting" channelName="greeting.command" routingKey="greeting.command" dataType="Greetings.Ports.Commands.GreetingCommand" timeOutInMilliseconds="200" />
            </connections>
        </serviceActivatorConnections>
    </configuration>
            

Step Fifteen
~~~~~~~~~~~~

Once the example is built you can run it using F5 within Visual Studio,
or navigate the binary and run that directly, as Topshelf supports
running as a console application.

To test the service use the Rabbit MQ management website, to post to a
Greeting to the queue that the service will just have created when you
ran it.

|image7|

To get the service working you only need to dispatch a simple message
body to the queue

::

    {"Greeting":"hello world","Id":"0a81cbbc-5f82-4912-99ee-19f0b7ee4bc8"}

You can do this in the Publish Message section of RabbitMQ

|image8|

And you should be able to observe the greeting you entered being output
in the console

|image9|

Next Steps
~~~~~~~~~~

The `Tasks Example <TasksExample.html>`__ contains a full example of a
distributed application, that contains a user-agent client, a REST API,
and a Windows Service that consumes work from a Task Queue

.. |image0| image:: images/Greetings-Step1-ConsoleProject.png
.. |image1| image:: images/Nuget-ServiceActivator.png
.. |image2| image:: images/Nuget-ServiceActivator.png
.. |image3| image:: images/NuGet-Topshelf.png
.. |image4| image:: images/NuGet-Topshelf-Licence.png
.. |image5| image:: images/TinyIoC-Nuget.png
.. |image6| image:: images/log4Net-NuGet.png
.. |image7| image:: images/greeting_command_queue.png
.. |image8| image:: images/publish_message.png
.. |image9| image:: images/hello%20world.png

