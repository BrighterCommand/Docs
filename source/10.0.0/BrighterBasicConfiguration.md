# **Basic Configuration**

Configuration is the most labor-intensive part of using Brighter.Once you have configured Brighter, using its model of requests and handlers is straightforward

## **Using .NET Core Dependency Injection**

This section covers using .NET Core Dependency Injection to configure Brighter. If you want to use an alternative DI container then see the section [How Configuration Works](/contents/HowConfigurationWorks.md) 

We divide configuration into two sections, depending on your requirements:

* [**Configuring The Command Processor**](#configuring-the-command-processor): This section covers configuring the **Command Processor**. Use this if you want to dispatch requests to handlers, or publish messages from your application on an external bus
* [**Configuring The Service Activator**](#configuring-the-service-activator): This section covers configuring the **Service Activator**. Use this if you want to read messages from a transport (and then dispatch to handlers).


## **Configuring The Command Processor**

### **Command Processor Service Collection Extensions** 

Brighter's package:

* **Paramore.Brighter.Extensions.DependencyInjection** 
 
 provides extension methods for **ServiceCollection** that can be used to add Brighter to the .NET Core DI Framework.

By adding the package you can call the **AddBrighter()** extension method.

If you are using a **Startup** class's **ConfigureServices** method  call the following:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
}

```

if you are using .NET 6 you can make the call direction on your **HostBuilder**'s Services property.

The **AddBrighter()** method takes an **`Action<BrighterOptions>`** delegate. The extension method supplies the delegate with a **BrighterOptions** object that allows you to configure how Brighter runs.

The **AddBrighter()** method returns an **IBrighterBuilder** interface. **IBrighterBuilder** is a [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) that you can use to configure additional Brighter properties (see [Brighter Builder Fluent Interface](#brighter-builder-fluent-interface)).

#### **Adding Polly Policies**

Brighter uses Polly policies for both internal reliability, and to support adding a custom policy to a handler for reliability.

To use a Polly policy with Brighter you need to register it first with a Polly **PolicyRegistry**. In this example we register both Synchronous and Asynchronous Polly policies with the registry.

``` csharp
    var retryPolicy = Policy.Handle<Exception>().WaitAndRetry(new[] 
        { 
            TimeSpan.FromMilliseconds(50), 
            TimeSpan.FromMilliseconds(100), 
            TimeSpan.FromMilliseconds(150) });
    
    var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(1, 
    TimeSpan.FromMilliseconds(500));
    
    var retryPolicyAsync = Policy.Handle<Exception>()
        .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
    
    var circuitBreakerPolicyAsync = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));

    var policyRegistry = new PolicyRegistry()
    {
        { "SyncRetryPolicy", retryPolicy },
        { "SyncCircuitBreakerPolicy", circuitBreakerPolicy },
        { "AsyncRetryPolicy", retryPolicyAsync },
        { "AsyncCircuitBreakerPolicy", circuitBreakerPolicyAsync }
    };

```

And you can use them in  you own handler like this:

``` csharp
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    static MyQoSProtectedHandler()
    {
        ReceivedCommand = false;
    }

    [UsePolicy(policy: "SyncRetryPolicy", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        /*Do work that could throw error because of distributed computing reliability*/
    }
}
```

See the section [Policy Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) for more on using Polly policies with handlers.

With the Polly Policy Registry filled, you need to tell Brighter where to find the Policy Registry:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
        options.PolicyRegistry = policyRegistry
    )
}

```

#### **Configuring Lifetimes**

Brighter can register your *Request Handlers* and *Message Mappers* for you (see [IBrighter Builder Fluent Interface](#ibrighterbuilder-fluent-interface)). When we register types for you with ServiceCollection, we need to register them with a given lifetime (see [Dependency Injection Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)).

We also allow you to set the lifetime for the CommandProcessor.

We recommend the following lifetimes:

* If you are using *Scoped* lifetimes, for example with EF Core, make your *Request Handlers* and your *Command Processor* Scoped as well.
* If you are not using *Scoped* lifetimes you can use *Transient* lifetimes for *Request Handlers* and a *Singleton* lifetime for the *Command Processor*.
* Your *Message Mappers* should not have state and can be *Singletons*.

(Be cautious about using *Singleton* lifetimes for *Request Handlers*. Even if your *Request Handler* is stateless today, and so does not risk carrying state across requests, a common bug is that state is added to an existing *Request Handler* which has previously been registered as a *Singleton*.)

You configure the lifetimes for the different types that Brighter can create at run-time as follows:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.CommandProcessorLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
    );
}

```

### **Brighter Builder Fluent Interface**

#### **Type Registration**
The **IBrighterBuilder** fluent interface can scan your assemblies for your *Request Handlers* (inherit from **IHandleRequests<>** or **IHandleRequestsAsync<>**) and *Message Mappers* (inherit from **IAmAMessageMapper<>**) and register then with the **ServiceCollection**. This is the most common way to register your code.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AutoFromAssemblies();
}

```

The code scans any loaded assemblies. If you need to register types from assemblies that are not yet loaded, you can provide a list of additional assemblies to scan as an argument to the call to **AutoFromAssemblies()**.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AutoFromAssemblies(typeof(MyRequestHandlerAsync).Assembly);
}

```

Instead of using **AutoFromAssemblies** you can exert more fine-grained control over the registration, by explicitly registering your *Request Handlers* and *Message Mappers*. We don't recommend this, but make it available for cases where the automatic registration does not meet your needs.

* **MapperRegistryFromAssemblies()**, **HandlersFromAssemblies()** and **AsyncHandlersFromAssemblies** are the methods called by **AutoFromAssemblies()** and can be called explicitly.
* **Handlers()**, **AsyncHandlers()** and **MapperRegistry()** accept an **Action<>** delegate that respectively provide you with **IAmASubscriberRegistry** or **IAmAnAsyncSubscriberRegistry** to register your RequestHandlers explicitly or a **ServiceCollectionMapperRegistry** to register your mappers. This gives you explicit control over what you register.

#### **Using an External Bus**

Using an *External Bus* allows you to send messages between processes using a message-oriented middleware transport (such as RabbitMQ or Kafka). (For symmetry, we refer to the usage of the *Command Processor* without an external bus as using an *Internal Bus*).

When raising a message on the *Internal Bus*, you use one of the following methods on the *Command Processor*:

* **Send()** and **SendAsync()** - Sends a *Command* to one *Request Handler*.
* **Publish()** and **PublishAsync()** - Broadcasts an *Event* to zero or more *Request Handlers*.

When raising a message on an *External Bus*, you use the following methods on the *CommandProcessor*:

* **Post()** and **PostAsync()** - Immediately posts a *Command* or *Event* to another process via the external Bus
* **DepositPost()** and **DepositPostAsync()** - Puts on or many *Command*(s) or *Event*(s) in the *Outbox* for later delivery
* **ClearOutbox()** and **ClearOutboxAsync()** - Clears the *Outbox*, posting un-dispatched messages to another process via the *External Bus*.
* **ClearAsyncOutbox()** - Implicitly clears the **Outbox**, similar to above however allows bulk dispatching of messages onto a **Transport**.

The major difference here is whether or not you wish to use an *Outbox* for Transactional Messaging. (See [Outbox Pattern](/contents/OutboxPattern.md) and [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) for more on Brighter and the Outbox Pattern).

To use an *External Bus*, you need to supply Brighter with configuration information that tells Brighter what middleware you are using and how to find it. (You don't need to do anything to configure an *Internal Bus*, it is always available.)

In order to provide Brighter with this information we need to provide it with an implementation of **IAmAProducerRegistry** for the middleware you intend to use for the *External Bus*.

#### **Transports and Gateways**

*Transports* are how Brighter supports specific Message-Oriented-Middleware (MoM). *Transports* are provided in separate NuGet packages so that you can take a dependency only on the transport that you need. Brighter supports a number of different *transports*. 

A *Gateway Connection* is how you configure connection to MoM within a *transport*. As an example, the *Gateway Connection* **RMqGatewayConnection** is used to connect to RabbitMQ. Internally the *Gateway Connection* is used to create a *Gateway* object which wraps the client SDK for the MoM.

We go into more depth on the fields you set here in sections dealing with specific transports.

#### **Publications**

A *Publication* configures a transport for sending a message to it's associated MoM. So an **RmqPublication** configures how we publish a message to RabbitMQ. There are a number of common properties to all publications.

* **MakeChannels**: Do you want Brighter to create the infrastructure? Brighter can create infrastructure that it needs, and is aware of: **OnMissingChannel.Create**. So a publication can create the topic to send messages to. Alternatively if you create the channel by another method, such as IaaC, we can verify the infrastructure on startup: **OnMissingChannel.Validate**. Finally, you can avoid the performance cost of runtime checks by assuming your infrastructure exists: **OnMissingChannel.Assume**.
* **MaxOutstandingMessages**: How large can the number of messages in the Outbox grow before we stop allowing new messages to be published and raise an **OutboxLimitReachedException**.
* **MaxOutStandingCheckIntervalMilliSeconds**: How often do we check to see if the Outbox is full.
* **Topic**: A Topic is the key used within the MoM to route messages. Publishers publish to a topic and subscribers, subscribe to it. We use a class **RoutingKey** to encapsulate the identifier used for a topic. The name the MoM uses for a topic may vary. Kafka & SNS use *topic* whilst RMQ uses *routingkey* 

#### **Transport NuGet Packages**

We use the naming convention **Paramore.Brighter.MessagingGateway.{TRANSPORT}** for *transports* where {TRANSPORT} is the name of the middleware. 

In this example we will show using an implementation of **IAmAProducerRegistry** for RabbitMQ, provided by the NuGet package: 

* **Paramore.Brighter.MessagingGateway.RMQ**

See the documentation for detail on specific *transports* on how to configure them for use with Brighter, for now it is enough to know that you need to provide a *Messaging Gateway* which tells us how to reach the middleware and a *Publication* which tells us how to configure the middleware.

*Transports* provide an **IAmAProducerRegistryFactory()** to allow you to create multiple *Publications* connected to the same middleware.

#### Retry and Circuit Breaker with an External Bus

When posting a request to the External Bus we use a Polly policy internally to control Retry and Circuit Breaker in case the External Bus is not available. These policies have defaults but you can configure the behavior using the policy keys: 

* **Paramore.RETRYPOLICY**
* **Paramore.CIRCUITBREAKER**

#### **Bus Example**

Putting this together, an example configuration for an External Bus for a local RabbitMQ instance could look like this:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(new RmqProducerRegistryFactory(
                    new RmqMessagingGatewayConnection
                    {
                        AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                        Exchange = new Exchange("paramore.brighter.exchange"),
                    },
                    new RmqPublication[]{
                        new RmqPublication
                    {
                        Topic = new RoutingKey("GreetingMade"),
                        MaxOutStandingMessages = 5,
                        MaxOutStandingCheckIntervalMilliSeconds = 500,
                        WaitForConfirmsTimeOutInMilliseconds = 1000,
                        MakeChannels = OnMissingChannel.Create
                    }}
                ).Create()
            )

            ...
}
```

#### **Outbox Support**

TODO: V10 Changes

If you intend to use Brighter's *Outbox* support for Transactional Messaging then you need to provide us with details of your *Outbox*.

Brighter provides a number of *Outbox* implementations for common Dbs (and you can write your own for a Db that we do not support). For this discussion we will look at Brighter's support for working with EF Core. See the documentation for working with specific *Outbox* implementations.

EF Core supports a number of databases and you should pick the packages that match the Dy you want to use with EF Core. In this case we will choose MySQL.

For this we will need the *Outbox* packages for the MySQL *Outbox*.

* **Paramore.Brighter.MySql**
* **Paramore.Brighter.Outbox.MySql**

For a given backing store the pattern should be Paramore.Brighter.{DATABASE} and Paramore.Brighter.Outbox.{DATABASE} where {DATABASE} is the name of the Db that you are using.

In addition for an ORM you will need to add the package that supports the ORM, in this case EF Core:

* **Paramore.Brighter.MySql.EntityFrameworkCore**

For a given ORM the pattern should be Paramore.Brighter.{ORM}.{DATABASE} where {ORM} is the ORM you are choosing and {DATABASE} is the Db you are using with the ORM.

To configure our *Outbox* we then need to use the Use{DATABASE}Outbox method call, where {DATABASE} is the {DATABASE} that we want, passing in the configuration for our Db so that we can access it. In our case this will be **UseMySqlOutbox()**.

As we want to use an ORM, in our case EF Core, we have to tell the Outbox how to access EF Core transactions - as we need to participate in a transaction with the ORM. We call a method for the Db, Use{DATABASE}TransactionConnectionProvider, where {DATABASE} is our Db, so in our case **UseMySqlTransactionConnectionProvider()**.

As a parameter to Use{DATABASE}TransactionConnectionProvider we need to provide a *Transaction Provider* for the ORM we are using, in our case this is *MySqlEntityFrameworkConnectionProvider<>).

Finally, if we want the *Outbox* to use a background thread to clear un-dispatched items from the *Outbox*, and we do in most circumstances, otherwise they will not be dispatched, we need to run an *Outbox Sweeper* to do this work.

To add the *Outbox Sweeper* you will need to take a dependency on another NuGet package:

* **Paramore.Brighter.Extensions.Hosting**

This results in:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(...)
        .UseMySqlOutbox(new MySqlConfiguration(DbConnectionString(), _outBoxTableName), typeof(MySqlConnectionProvider), ServiceLifetime.Singleton)
        .UseMySqTransactionConnectionProvider(typeof(MySqlEntityFrameworkConnectionProvider<GreetingsEntityGateway>), ServiceLifetime.Scoped)
        .UseOutboxSweeper()

        ...
}

```

(**UseExternalBus()** has optional parameters for use with Request-Reply support for some transports. We don't cover that here, instead see [Direct Messaging](/contents/Routing.md#direct-messaging) for more).

#### **Configuring JSON Serialization**

Brighter defines a set of serialization options for use when it needs to serialize messages to JSON. Internally we use these options in our transports, when serializing messages to an external bus and deserializing from an external bus. You may wish to use these options in your own [*Message Mapper*](/contents/MessageMappers.md) implementation.

By default our JSONSerialization Options are configured as follows:

``` csharp
static JsonSerialisationOptions()
{
    var opts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true
    };

    opts.Converters.Add(new JsonStringConverter());
    opts.Converters.Add(new DictionaryStringObjectJsonConverter());
    opts.Converters.Add(new ObjectToInferredTypesConverter());
    opts.Converters.Add(new JsonStringEnumConverter());

    Options = opts;
}
```

You can use the **IBrighterBuilder** extension **ConfigureJsonSerialisation** to override these values. The method takes an **Action\<JsonSerialisationOptions\>** lambda expression that allows you to override these defaults. For example:

```csharp

.ConfigureJsonSerialisation((options) =>
{
    options.PropertyNameCaseInsensitive = true;
})

```

If you want to use this configured set of JSON Serialization options in your own code, you can, by using the static property JsonSerialisationOptions.Options. For example:

```csharp
public GreetingMade MapToRequest(Message message)
{
    return JsonSerializer.Deserialize<GreetingMade>(message.Body.Value, JsonSerialisationOptions.Options);
}
```

### **Putting It All Together**

Putting all this together, a typical configuration might looks as follows:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
            options.MapperLifetime = ServiceLifetime.Singleton;
            options.PolicyRegistry = policyRegistry;
        })
        .ConfigureJsonSerialisation((options) =>
        {
            options.PropertyNameCaseInsensitive = true;
        })
        .UseExternalBus(new RmqProducerRegistryFactory(
                new RmqMessagingGatewayConnection
                {
                    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@rabbitmq:5672")),
                    Exchange = new Exchange("paramore.brighter.exchange"),
                },
                new RmqPublication[] {
                    new RmqPublication
                    {
                        Topic = new RoutingKey("GreetingMade"),
                        MaxOutStandingMessages = 5,
                        MaxOutStandingCheckIntervalMilliSeconds = 500,
                        WaitForConfirmsTimeOutInMilliseconds = 1000,
                        MakeChannels = OnMissingChannel.Create
                    }}
            ).Create()
        )
        .UseMySqlOutbox(new MySqlConfiguration(DbConnectionString(), _outBoxTableName), typeof(MySqlConnectionProvider), ServiceLifetime.Singleton)
        .UseMySqTransactionConnectionProvider(typeof(MySqlEntityFrameworkConnectionProvider<GreetingsEntityGateway>), ServiceLifetime.Scoped)
        .UseOutboxSweeper()
        .AutoFromAssemblies();
}

```

## **Configuring The Service Activator**

A *consumer* reads messages from Message-Oriented Middleware (MoM), and a *producer* puts messages onto the MoM for the *consumer* to read.

A *consumer* waits for messages to appear on the queue, reads them, and then calls your *Request Handler* code to react. Because the •consumer* runs your code in response to an external request, a message being placed on the External Bus, we call the component that listens for messages and dispatches them a [*Service Activator*](https://www.enterpriseintegrationpatterns.com/patterns/messaging/MessagingAdapter.html)

To use Brighter's Service Activator you will need to take a dependency on the NuGet package:

* **Paramore.Brighter.ServiceActivator**

### **ServiceActivator Service Collection Extensions**

We provide support for configuring .NET Core's **HostBuilder** as a *ServiceActivator* for use with MoM. We use Brighter's Command Processor to dispatch the messages read by a *Dipatcher*. If you are not using **HostBuilder** then you will need to configure the Dispatcher yourself. See [How Configuring the Dispatcher Works](/contents/HowConfiguringTheDispatcherWorks.md) for more.

To use Brighter's *Service Activator* with **HostBuilder** you will need to take a dependency on the following NuGet packages:

* **Paramore.Brighter.ServiceActivator.Extensions.Hosting**
* **Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection**

These provide an extension method **AddServiceActivator()** that can be used to add Brighter to the .NET Core DI Framework.

By adding the package you can call the **AddServiceActivator()** extension method.

If you are using a **HostBuilder** class's **ConfigureServices** method  call the following:

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            services.AddServiceActivator(...)
        }

```

if you are using .NET 6 you can make the call direction on your **HostBuilder**'s Services property.

The **AddServiceActivator()** method takes an **`Action<ServiceActivatorOptions>`** delegate. The extension method supplies the delegate with a **ServiceActivatorOptions** object that allows you to configure how Brighter runs.

The **AddServiceActivator()** method returns an **IBrighterBuilder** interface. **IBrighterBuilder** is a [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) that you can use to configure Brighter *Command Processor* properties. It is discussed above at [Brighter Builder Fluent Interface](#brighter-builder-fluent-interface)) and the same options apply. We discuss one additional option that becomes important when receiving requests the *Inbox* in [Additional Brighter Builder Options](/contents/BasicConfiguration.md#the-inbox).

#### **Subscriptions**

When configuring your application's *Service Activator*, your *Subscriptions* indicate configure how your application will receive messages from the associated MoM queues or streams.

All *Subscriptions* lets you configure the following common properties.

* **Buffer Size**: The number of messages to hold in memory. Where the buffer is not shared, a single thread or Performer can access these; where the buffer is shared, multiple threads can access the same buffer of work. Work in a buffer is locked on queue based middleware, and thus not available to other consumers (threads or process depending if the buffer is shared or not) until *Acknowledged* or *Rejected*.
* **Channel Factory**: Creates or finds the necessary infrastructure for messaging on the MoM and wraps it in an object.
* **Channel *Name**: If queues are primitives in the MoM this names the queue, otherwise just used for diagnostics.
* **Channel Failure Delay**: How long should we delay if a channel fails before trying again, to give problems time to clear.
* **Data Type**: We use a [Datatype Channel](https://www.enterpriseintegrationpatterns.com/DatatypeChannel.html). What is the type of this channel?
* **Empty Channel Delay**: If there are no messages in the queue or stream when we read, how long should we pause before reading again?
* **MakeChannels**: Do you want Brighter to create the infrastructure? Brighter can create infrastructure that it needs, and is aware of: **OnMissingChannel.Create**. So a subscription can create the topic to send messages to, and any subscription to that topic required by the MoM, including a queue (which uses the *Channel Name*). Alternatively if you create the channel by another method, such as IaaC, we can verify the infrastructure on startup: **OnMissingChannel.Validate**. Finally, you can avoid the performance cost of runtime checks by assuming your infrastructure exists: **OnMissingChannel.Assume**.
* **Name**: What do we call this subscription for diagnostic purposes.
* **NoOfPerformers**: Effectively, how many threads do we use to read messages from the queue. As Brighter uses a Single-Threaded Apartment model, each thread has it's own message pump and is thus an in-process implementation of the [Competing Consumers](https://www.enterpriseintegrationpatterns.com/CompetingConsumers.html) pattern.
* **RequeueCount**: How many times can you retry a message before we declare it a poison pill message?
* **RequeueDelayInMilliseconds**: When we requeue a message how long should we delay it by?
* **RoutingKey**: The identifier used to routed messages to subscribers on MoM. You publish to this, and subscriber from this. This has different names; in Kafka or SNS this is a Topic, in RMQ this is the routing key.
* **RunAsync**: Is this an async pipeline? Your pipeline must be sync or async. An async pipeline can increase throughput where a handler is I/O bound by allowing the message pump to read another message whilst we await I/O completion. The cost of this is that strict ordering of messages will now be lost as processing of I/O bound requests may complete out-of-sequence. Brighter provides its own synchronization context for async operations. We recommend scaling via increasing the number of performers, unless you know that I/O is your bottleneck.
* **TimeoutInMilliseconds**: How long does a read 'wait' before assuming there are no pending messages.
* **UnaceptableMessageLimit**: Brighter will ack a message that throws an unhandled exception, thus removing it from a queue. 

For a more detailed discussion of using Requeue (with Delay) for Handler failure, (**RequeueCount** and **RequeueDelayInMilliseconds**) along with termination of a consumer due to message failure (**UnacceptableMessageLimit**) see [Handler Failure](/contents/HandlerFailure.md)

In addition, individual transports that provide access to specific MoM sub-class *Subscription* to provide properties unique to the chosen middleware. We discuss those under a section for that transport.

For RabbitMQ for example, this would look like this:

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new RmqSubscription<GreetingMade>(
            new SubscriptionName("paramore.sample.salutationanalytics"),
            new ChannelName("SalutationAnalytics"),
            new RoutingKey("GreetingMade"),
            runAsync: true,
            timeoutInMilliseconds: 200,
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    services.AddServiceActivator(options =>
        {
            options.Subscriptions = subscriptions;
        })
}

...

```

#### **Gateway Connections & Channel Factories**

A *Gateway Connection* tells Brighter how to connect to MoM for a particular transport. The transport package will contain a *Gateway Connection*, you need to provide the information to connect to your middleware (URIs, ports, credentials etc.) Your transport package provides a *Gateway Connection*

A *Channel Factory* connects Brighter to MoM. Depending on the configuration settings for your *Subscription* it may create the required primitives (topics/routing keys, queues, streams) on MoM or simply attach to ones that you have created via Infrastructure as Code (IaC). Your transport provides a *Channel Factory* and you need to pass it a *Gateway Connection*.

For RabbitMQ, this would look like:

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri($"amqp://guest:guest@local:5672")),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    services.AddServiceActivator(options =>
        {
             options.ChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
        })
}

...

```

#### **Configuring Service Activator Lifetimes**

Under the hood your *Service Activator* uses a *Command Processor* and you will need to configure lifetimes [as discussed above](#configuring-lifetimes).

An additional requirement is configuring the lifetime of the *Command Processor* itself. Within the context of an ASP.NET application, configuring the lifetime of the **Command Processor** relies on ASP.NET creating an instance of the *Command Processor* in a request pipeline. When you are using *Service Activator* there is no ASP.NET pipeline, instead Brighter's *Dispatcher* manages the lifetime of the *Command Processor* that we pass a request to. By setting the **ServiceActivatorOptions.UseScoped** field to true, you instruct *Brighter* to use a new *Command Processor* instance for each request. This is important if you take the *Command Processor* as a dependency in any of your *Request Handlers* with a **Scoped** lifetime. If in doubt, just set **ServiceActivatorOptions.UseScoped** field to true. 


``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddServiceActivator(options =>
        {
            options.UseScoped = true;
            options.HandlerLifetime = ServiceLifetime.Scoped;
            options.MapperLifetime = ServiceLifetime.Singleton;
            options.CommandProcessorLifetime = ServiceLifetime.Scoped;
    })
}

...

```


### **Service Activator Brighter Builder Fluent Interface**

The call to **AddServiceActivator()** returns an **IBrighterBuilder** fluent interface. This means that you can use any of the options described in [Brighter Build Fluent Interfaces](#brighter-builder-fluent-interface) to configure the associated *Command Processor* such as scanning assemblies for *Request Handlers* and adding an *External Bus* and *Outbox*.

An option is intended for the context of a Service Activator is described below.

#### **Inbox**

As described in the [Outbox Pattern](/contents/OutboxPattern.md) an *Outbox* offers **Guaranteed, At Least Once** delivery. It explicitly may result in you sending duplicate messages. In addition, MoM tends to offer "At Least Once" guarantees only, further creating the risk that you will receive a duplicate message.

If the request is not idempotent, you can use an Inbox to de-duplicate it. See [Inbox Support](/contents/BrighterInboxSupport.md) for more.

Configuring an *Inbox* has two elements. The first is the type of *Inbox*, the second configuration for the *Inbox* behavior.

Brighter provides a number of *Inbox* implementations for common Dbs (and you can write your own for a Db that we do not support). For this discussion we will look at Brighter's support for working with MySQL. See the documentation for working with specific *Inbox* implementations.

For this we will need the *Inbox* packages for the MySQL *Inbox*.

* **Paramore.Brighter.Inbox.MySql**

For a given backing store the pattern should be Paramore.Brighter.Inbox.{DATABASE} where {DATABASE} is the name of the Db that you are using.

To configure our *Inbox* we then need to use the UseExternalInbox method call and pass in an instance of a class that implements **IAmAnInbox**, taken from our package, and an instance of **InboxConfiguration** that tells Brighter how we want to use the Inbox.

For *Inbox Configuration* you set the following properties:

* **ActionOnExists**: What do we do if the request has been handled? The default,**OnceOnlyAction.Throw** is to throw a **OnceOnlyException**. If you take no other action this will cause the message to be rejected and sent to a DLQ if one is configured (See [Handler Failure](/contents/HandlerFailure.md)). The alternative is **OnceOnlyAction.Warn** simply logs that the request is a duplicate, but takes no other action.
* **OnceOnly**: This defaults to *true* and will check for a duplicate and take the action indicated by **ActionOnExists**. If *false* the *Inbox* will record the request, but will take no further action. (This tends to be set to *false* if you are using the *Inbox* to record what requests caused current state only and not de-duplicate).
* **Scope**: This indicates the type of request (*Command* or *Event*) to store in the *Inbox*. By default this is set to **InboxScope.All** and captures everything but you can be explicit and just capture **InboxScope.Commands** or **InboxScope.Events**. (This tends to be set to **InboxScope.Commands** when only commands cause changes to state that are not idempotent).
* **Context**: Used to uniquely identify receipt of this request via this handler. If you are recording *Events* and have multiple handlers, then the first event handler to receive the message will block the others from doing so, unless you disambiguate the handler identity by supplying a context method.

A typical *Inbox* configuration for MySQL would be:

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddServiceActivator(options =>
        { ...  })
        .UseExternalInbox(
            new MySqlInbox(new MySqlInboxConfiguration("server=localhost; port=3306; uid=root; pwd=root; database=Salutations", "Inbox");
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        );
}

...

```
Typically you would obtain the connection string for the Db from configuration (as opposed to hard coding the string), likewise for the table name for your *Inbox*.


### Running Service Activator

To run *Service Activator* we add it as a [Hosted Service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio). 

We provide the class **ServiceActivatorHostedService** for this in the NuGet package:

* **Paramore.Brighter.ServiceActivator.Extensions.Hosting**

The **ServiceActivatorHostedService** calls the **Dispatcher.Receive** method which starts message pumps for the configured *Subscriptions*.

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    ...

    services.AddHostedService<ServiceActivatorHostedService>();
}

...

```

On shutdown Brighter will allow the current *Request Handler* to complete, then end the message pump loop and exit. If you have long-running handlers it is possible that they will not complete in the default 5s for graceful shutdown of the MS Generic Host. In this case, you need to [increase the timeout](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0#shutdowntimeout) of the host shutdown.

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(20);
            });
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    ...

    services.AddHostedService<ServiceActivatorHostedService>();
}

```

### A Complete Service Activator Example

When all of the relevant configuration sections are added together, your code will look something like this, with variations for your transport and stores.

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(20);
            });
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
   var subscriptions = new Subscription[]
    {
        new RmqSubscription<GreetingMade>(
            new SubscriptionName("paramore.sample.salutationanalytics"),
            new ChannelName("SalutationAnalytics"),
            new RoutingKey("GreetingMade"),
            runAsync: true,
            timeoutInMilliseconds: 200,
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri($"amqp://guest:guest@localhost:5672")),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    services.AddServiceActivator(options =>
        {
            options.Subscriptions = subscriptions;
            options.ChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
            options.UseScoped = true;
            options.HandlerLifetime = ServiceLifetime.Scoped;
            options.MapperLifetime = ServiceLifetime.Singleton;
            options.CommandProcessorLifetime = ServiceLifetime.Scoped;
        })
        .AutoFromAssemblies()
        .UseExternalInbox(
            new MySqlInbox(new MySqlInboxConfiguration("server=localhost; port=3306; uid=root; pwd=root; database=Salutations", "Inbox");
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        )
    
    services.AddHostedService<ServiceActivatorHostedService>();

}

```

## Samples

Brighter includes a comprehensive set of [Samples](https://github.com/BrighterCommand/Brighter/tree/master/samples) in its main repo that you can review for clarity on how Brighter works and should be configured.


