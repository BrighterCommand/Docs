# How Configuring a Dispatcher for an External Bus Works

TODO: Review for V9

In order to receive messages from Message Oriented Middleware (MoM) such as RabbitMQ or Kafka you have to configure a *Dispatcher*. The *Dispatcher* works with a *Command Processor* to deliver messages read from a queue or stream to your *Request Handler*. You write a Request Handler as you would for a request sent over an Internal Bus, and hook it up to Message Oriented Middleware via a *Dispatcher*. 

For each message source (queue or stream) that you listen to, the Dispatcher lets you run one or more *Performers*. A *Performer* is a single-threaded message pump. As such, ordering is guaranteed on a *Peformer*. You can run multiple *Peformers* to utilize the [Competing Consumers](https://www.enterpriseintegrationpatterns.com/CompetingConsumers.html) pattern, at the cost of ordering.

If you are using .NET Core Dependency Injection, we provide extension methods to **HostBuilder** to help you configure a Dispatcher. This information is then for background only. If you are not using **HostBuilder** you will need to perform the following steps explicitly in your code.

## Configuring the Dispatcher 

We provide a Dispatch Builder that has a progressive interface to assist you in configuring a **Dispatcher**

You need to consider the following when configuring the Dispatcher

-   Logging
-   Command Processor
-   Message Mappers
-   Channel Factory
-   Connection List

Of these, **Logging** and the **Command Processor** are covered in [Basic Configuration](BasicConfiguration.html).

### Message Mappers

We use **IAmAMessageMapper\<T\>** to map between messages in the External Bus and a **Message**.

A **Message** consists of two parts, a **Message Header** and **Message Body**. The header contains metadata about the message. Key properties are **TimeStamp**, **Topic**, and **Id**. The body consists of the
serialized **IRequest** sent over the External Bus.

We dispatch a **Message** using either **commandProcessor.Send()** or **commandProcessor.Publish()** depending on whether the **MessageHeader.MessageType** is **MT_COMMAND** or **MT_EVENT**.

You create a **Message Mapper** by deriving from **IAmAMessageMapper\<TaskReminderCommand\>** and implementing the **MapToMessage()** and **MapToRequest** methods.

``` csharp
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
```

You then need to register your Message Mapper so that we can find it, using a class that derives from **IAmAMessageMapperRegistry**. We recommend using **MessageMapperRegistry** unless you have more specific
requirements.

``` csharp
var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
{
    { typeof(GreetingCommand), typeof(GreetingCommandMessageMapper) }
};
```

### Channel Factory

The Channel Factory is where we take a dependency on a specific Broker. We pass the **Dispatcher** an instances of **InputChannelFactory** passing it an implementation of **IAmAChannelFactory**. The channel
factory is used to create channels that wrap the underlying Message-Oriented Middleware that you are using.

For production use we support
[RabbitMQ](https://github.com/BrighterCommand/Brighter/tree/master/src/Paramore.Brighter.MessagingGateway.RMQ) as a Broker. We are actively working on other implementations.

You can see the code for this in the full builder snippet below.

We don\'t cover details of how to implement a Channel Factory here, for simplicity.

### Connection List

Brighter supports one or more connections.

The most important part of a connection to understand is the **routing key**. This must be the same as the topic you set in the **Message Header** when sending. In addition the **dataType** should be the name
of the **Command** or **Event** derived type that you want to deserialize into i.e. we will use reflection to create an instance of this type.

You must set the **connectionName** and **channelName**. The naming scheme is at your discretion. We often use the namespace of the producer\'s type that serializes into the message on the wire.

The **timeOutInMilliseconds** sets how long we wait for a message before timing out. Note that after a timeout we will wait for messages on the channel again, following a delay. This just allows us to yield to
receive control messages on the message pump.

``` csharp
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
```

### Creating a Builder

This code fragment shows putting the whole thing together

``` csharp
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
        .NoExternalBus()
        .RequestContextFactory(new InMemoryRequestContextFactory())
        .Build())
    .MessageMappers(messageMapperRegistry)
    .ChannelFactory(new InputChannelFactory(rmqMessageConsumerFactory))
    .Connections(connections)
    .Build();
```

## Running The Dispatcher

To ensure that messages reach the handlers from the queue you have to run a **Dispatcher**.

The Dispatcher reads messages of input channels. Internally it creates a message pump for each channel, and allocates a thread to run that message pump. The pump consumes messages from the channel, using the
**Message Mapper** to translate them into a **Message** and from there a **Command** or **Event**. It then dispatches those to handlers (using the Brighter **Command Processor**).

To use the Dispatcher you need to host it in a consumer application. Usually a console application or Windows Service is appropriate. 

We recommend using HostBuilder, but if not you will need to use something like [Topshelf](http://topshelf-project.com/) to host your consumers.

The following code shows an example of using the **Dispatcher** from Topshelf. The key methods are **Dispatcher.Receive()** to start the message pumps and **Dispatcher.End()** to shut them.

We do allow you to start and stop individual channels, but this is an advanced feature for operating the services.

``` csharp
internal class GreetingService : ServiceControl
{
    private Dispatcher _dispatcher;

    public GreetingService()
    {
       /* Configfuration Code Goes here*/
    }

    public bool Start(HostControl hostControl)
    {
        _dispatcher.Receive();
        return true;
    }

    public bool Stop(HostControl hostControl)
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
```

