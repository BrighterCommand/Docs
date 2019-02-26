Distributed Task Queue Configuration
------------------------------------

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
`Basic Configuration <BasicConfiguration.html>`__.

Message Mappers
~~~~~~~~~~~~~~~

We use **IAmAMessageMapper<T>** to map between messages in the Task
Queue and a **Message**.

A **Message** consists of two parts, a **Message Header** and **Message
Body**. The header contains metadata about the message. Key properties
are time **TimeStamp**, **Topic**, and **Id**. The body consists of the
serialized **IRequest** sent over the Task Queue.

We dispatch a **Message** using either **commandProcessor.Send()** or
**commandProcessor.Publish()** depending on whether the
**MessageHeader.MessageType** is **MT\_COMMAND** or **MT\_EVENT**.

You create a **Message Mapper** by deriving from
**IAmAMessageMapper<TaskReminderCommand>** and implementing the
**MapToMessage()** and **MapToRequest** methods.

.. highlight:: csharp

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

.. highlight:: csharp

::

    var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
    {
        { typeof(GreetingCommand), typeof(GreetingCommandMessageMapper) }
    };


Channel Factory
~~~~~~~~~~~~~~~

The Channel Factory is where we take a dependency on a specific Broker.
We pass the **Dispatcher** an instances of **InputChannelFactory**
passing it an implementation of **IAmAChannelFactory**. The channel
factory is used to create channels that wrap the underlying
Message-Oriented Middleware that you are using.

For production use we support `RabbitMQ <https://github.com/BrighterCommand/Brighter/tree/master/src/Paramore.Brighter.MessagingGateway.RMQ>`_
as a Broker. We are actively working on other implementations.

You can see the code for this in the full builder snipped below.

We don't cover details of how to implement a Channel Factory here, for
simplicity.

Connection List
~~~~~~~~~~~~~~~

Brighter supports configuration of a service activator via code. A   
Service Activator supports one or more connections.

The most important part of a connection to understand is the
**routing key**. This must be the same as the topic you set in the
**Message Header** when sending. In addition the **dataType** should be
the name of the **Command** or **Event** derived type that you want to
deserialize into i.e. we will use reflection to create an instance of this type.

You must set the **connectionName** and **channelName**. The naming
scheme is at your discretion. We often use the namespace of the producer's type
that serializes into the message on the wire 

The **timeOutInMilliseconds** sets how long we wait for a message before timing out. 
Note that after a timeout we will wait for messages on the channel again, 
following a delay. This just allows us to yield to receive control messages on the message pump.

.. highlight:: csharp

::

        var connections = new List<Connection>
        {
            new Connection(
                new ConnectionName("paramore.example.greeting"),
                new InputChannelFactory(rmqMessageConsumerFactory, rmqMessageProducerFactory),
                typeof(GreetingEvent),
                new ChannelName("greeting.event"),
                "greeting.event",
                timeoutInMilliseconds: 200)
        };


Creating a Builder
~~~~~~~~~~~~~~~~~~

This code fragment shows putting the whole thing together

.. highlight:: csharp

::

    // create message mappers
    var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
    {
        { typeof(GreetingCommand), typeof(GreetingCommandMessageMapper) }
    };

    // create the gateway
    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(logger);
    _dispatcher = DispatchBuilder.With()
        .CommandProcessor(CommandProcessorBuilder.With()
            .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
            .Policies(policyRegistry)
            .NoTaskQueues()
            .RequestContextFactory(new InMemoryRequestContextFactory())
            .Build())
        .MessageMappers(messageMapperRegistry)
        .ChannelFactory(new InputChannelFactory(rmqMessageConsumerFactory))
        .Connections(connections)
        .Build();
