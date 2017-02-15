`Next <RabbitMQConfiguration.html>`__

`Prev <Routing.html>`__

How Brighter configures Task Queues
-----------------------------------

In order to use the distributed task queue you need to configure the
Dispatcher.

There are two steps. The first is in-code configuration of your options
and the second is configuration of your files in a configuration file.

Why the split?
~~~~~~~~~~~~~~

Generally we prefer to configure design time options in code because
that is the easiest to manage and only run-time options in an external
configuration file.

In essence, anything that you want to configure post-build i.e. when you
deploy needs to go into external configuration, and anything you can
configure at build should go into code.

Because you may choose to configure the channels that a service
processes at runtime we configure them there. An example use case here
is that you may have busy channels that need more consumers to process a
backlog. You can add channels to existing services at run-time to help
share the load, and then remove those channels later once the backlog
has been worked through.

Configuring the Dispatcher in Code
----------------------------------

We provide a Dispatch Builder that has a progressive interface to assist
you in configuring a **Dispatcher**

You need to consider the following when configuring the Dispatcher

-  Logging
-  Command Processor
-  Message Mappers
-  Channel Factory
-  Connection List

Of these **Logging** and the **Command Processor** are covered in
**Basic Configuration**.

Message Mappers
~~~~~~~~~~~~~~~

We use **IAmAMessageMapper<T>** to map between messages in the Task
Queue and a **Message**.

A **Message** consists of two parts, a **Message Header** and **Message
Body**. The header contains metadata about the message. Key properties
are time **TimeStamp**, **Topic**, and **Id**. The body consists of the
serialized **IRequest** sent over the Task Queue.

We dispatch a **Message** using either **CommandProcessor.Send()** or
**CommandProcessor.Publish()** depending on whether the
**MessageHeader.MessageType** is **MT\_COMMAND** or **MT\_EVENT**.

You create a **Message Mapper** by deriving from
**IAmAMessageMapper<TaskReminderCommand>** and implementing the
**MapToMessage()** and **MapToRequest** methods.

::

    public class TaskReminderCommandMessageMapper : IAmAMessageMapper<TaskReminderCommand>
    {
        public Message MapToMessage(TaskReminderCommand request)
        {
            var header = new MessageHeader(messageId: request.Id, topic: "Task.Reminder", messageType: MessageType.MT_COMMAND);
            var body = new MessageBody(JsonConvert.SerializeObject(request));
            var message = new Message(header, body);
            return message;
        }

        public TaskReminderCommand MapToRequest(Message message)
        {
            return JsonConvert.DeserializeObject<TaskReminderCommand>(message.Body.Value);
        }
    }
            

You then need to register your Message Mapper so that we can find it,
using a class that derives from **IAmAMessageMapperRegistry**. We
recommend using **MessageMapperRegistry** unless you have more specific
requirements.

::

    var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
    {
        {typeof(GreetingCommand), typeof(GreetingCommandMessageMapper)}
    };
            

Channel Factory
~~~~~~~~~~~~~~~

The Channel Factory is where we take a dependency on a specific Broker.
We pass the **Dispatcher** an instances of **InputChannelFactory**
passing it an implementation of **IAmAChannelFactory**. The channel
factory is used to create channels that wrap the underlying
Message-Oriented Middleware that you are using.

For production use we support RabbitMQ
` <https://github.com/iancooper/Paramore/tree/master/Brighter/paramore.brighter.commandprocessor.messaginggateway.rmq>`__
as a Broker. We are actively working on other implementations.

You can see the code for this in the full builder snipped below.

We don't cover details of how to implement a Channel Factory here, for
simplicity.

Connection List
~~~~~~~~~~~~~~~

The preferred way to get a connection list is via configuration. You can
pass it in directly. This is mainly intended to support our Control Bus
or testing and we don't cover that here.

Again, you can see the code for this in the full builder snippet below.

What you do care about is the shape of the configuration entries. In
your configuration file you first need to ensure that you have the
Configuration Section Handler for Service Activator registered

::

    <configSections>
    <section name="rmqMessagingGateway" type="paramore.brighter.commandprocessor.messaginggateway.rmq.MessagingGatewayConfiguration.RMQMessagingGatewayConfigurationSection, paramore.brighter.commandprocessor.messaginggateway.rmq" allowLocation="true" allowDefinition="Everywhere" />
    </configSections>
            

And then you need to configure your channels. The important part is the
**routing key**. This must be the same as the topic you set in the
**Message Header** when sending. In addition the **dataType** should be
the name of the **Command** or **Event** derived type that you want to
deserialize into.

You must set the **connectionName** and **channelName**. The naming
scheme is at your discretion. The **timeOutInMilliseconds** sets how
long we wait for a message before timing out. Note that after a timeout
we will wait for messages on the channel again, following a delay. This
just allows us to yield to receive control messages on the message pump.

::

    <serviceActivatorConnections>
    <connections>
    <add connectionName="paramore.example.greeting" channelName="greeting.command" routingKey="greeting.command" dataType="Greetings.Ports.Commands.GreetingCommand" timeOutInMilliseconds="200" />
    </connections>
    </serviceActivatorConnections>
            

Creating a Bulder
~~~~~~~~~~~~~~~~~

This code fragment shows putting the whole thing together

::

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
