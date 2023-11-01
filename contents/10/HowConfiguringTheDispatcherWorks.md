# How Configuring a Dispatcher for an External Bus Works

In order to receive messages from Message Oriented Middleware (MoM) such as RabbitMQ or Kafka you have to configure a *Dispatcher*. The *Dispatcher* works with a *Command Processor* to deliver messages read from a queue or stream to your *Request Handler*. You write a Request Handler as you would for a request sent over an Internal Bus, and hook it up to Message Oriented Middleware via a *Dispatcher*. 

For each message source (queue or stream) that you listen to, the Dispatcher lets you run one or more *Performers*. A *Performer* is a single-threaded message pump. As such, ordering is guaranteed on a *Peformer*. You can run multiple *Peformers* to utilize the [Competing Consumers](https://www.enterpriseintegrationpatterns.com/CompetingConsumers.html) pattern, at the cost of ordering.

If you are using .NET Core Dependency Injection, we provide extension methods to **HostBuilder** to help you configure a Dispatcher. This information is then for background only, but may be useful when debugging. Just follow the steps outlined in [BasicConfiguration](/contents/BrighterBasicConfiguration.md). 

If you are not using **HostBuilder** you will need to perform the following steps explicitly in your code.

## Configuring the Dispatcher 

We provide a Dispatch Builder that has a progressive interface to assist you in configuring a **Dispatcher**

You need to consider the following when configuring the Dispatcher

-   Command Processor
-   Message Mappers
-   Channel Factory
-   Connection List

Configuring the **Command Processor** is covered in [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md).

### Message Mappers

You need to register your [Message Mapper](/contents/MessageMappers.md) so that we can find it. The registry must implement **IAmAMessageMapperRegistry**. We recommend using Brighter's **MessageMapperRegistry** unless you have more specific requirements.

``` csharp
var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
{
    { typeof(GreetingCommand), typeof(GreetingCommandMessageMapper) }
};
```

### Channel Factory

The Channel Factory is where we take a dependency on a specific Broker. We pass the **Dispatcher** an instances of **InputChannelFactory** which in turn has a dependency on implementation of **IAmAChannelFactory**. The channel factory is used to create channels that wrap the underlying Message-Oriented Middleware that you are using.

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
    .Subscribers(subscriptions)
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

