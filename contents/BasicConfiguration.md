# **Basic Configuration**

Configuration is the most labor-intensive part of using Brighter.Once you have configured Brighter, using its model of requests and handlers is straightforward

## **Using .NET Core Dependency Injection**

This section covers using .NET Core Dependency Injection to configure Brighter. If you want to use an alternative DI container then see the section [How Configuration Works](/contents/HowConfigurationWorks.md) 

We divide configuration into two sections, depending on your requirements:

* [**Configuring The Command Processor**](#configuring-the-command-processor): This section covers configuring the **Command Processor**. Use this if you want to dispatch requests to handlers, or publish messages from your application on an external bus
* [**Configuring The Service Activator**](#configuring-the-service-activator): This section covers configuring the **Service Activator**. Use this if you want to read messages from a transport (and then dispatch to handlers).


## **Configuring The Command Processor**


### **Service Collection Extensions** 

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
* **DepositPost()** and **DepositPostAsync()** - Puts a *Command* or *Event* in the *Outbox* for later delivery
* **ClearOutbox()** and **ClearOutboxAsync()** Clears the *Outbox*, posting un-dispatched messages to another process via the *External Bus*.

The major difference here is whether or not you wish to use an *Outbox* for Transactional Messaging. (See [Outbox Pattern](/contents/OutboxPattern.md) and [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) for more on Brighter and the Outbox Pattern).

To use an *External Bus*, you need to supply Brighter with configuration information that tells Brighter what middleware you are using and how to find it. (You don't need to do anything to configure an *Internal Bus*, it is always available.)

In order to provide Brighter with this information we need to provide it with an implementation of **IAmAProducerRegistry** for the middleware you intend to use for the *External Bus*.

*Transports* are how Brighter supports specific message-oriented-middleware. *Transports* are provided in separate NuGet packages so that you can take a dependency only on the transport that you need. Brighter supports a number of different *transports*. We use the naming convention **Paramore.Brighter.MessagingGateway.{TRANSPORT}** for *transports* where {TRANSPORT} is the name of the middleware. 

In this example we will show using an implementation of **IAmAProducerRegistry** for RabbitMQ, provided by the NuGet package: 

* **Paramore.Brighter.MessagingGateway.RMQ**

See the documentation for detail on specific *transports* on how to configure them for use with Brighter, for now it is enough to know that you need to provide a *Messaging Gateway* which tells us how to reach the middleware and a *Publication* which tells us how to configure the middleware.

*Transports* provide an **IAmAProducerRegistryFactory()** to allow you to create multiple *Publications* connected to the same middleware.

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

If you intend to use Brighter's *Outbox* support for Transactional Messaging then you need to provide us with details of your *Outbox*.

Brighter provides a number of *Outbox* implementations for common Dbs (and you can write your own for a Db that we do not support). For this discussion we will look at Brighter's support for working with EF Core. See the documentation for working with specific *Outbox* implementations.

EF Core supports a number of databases and you should pick the packages that match the Dy you want to use with EF Core. In this case we will choose MySQL.

For this we will need the *Outbox* packages for the MySQL *Outbox*.

* **Paramore.Brighter.MySql**
* **Paramore.Brighter.Outbox.MySql**

For a given backing store the pattern should be Paramore.Brighter.{DATABASE} and Paramore.Brighter.Outbox.{DATABASE} where {DATABASE} is the name of the Db that you are using.

In addition for an ORM you will need to add the package that supports the ORM, in this case EF Core:

* **Paramore.Brighter.MySql.EntityFrameworkCore**

For a given ORM the pattern should be Paramore.Brighter.{ORM}.{DATABASE where {ORM} is the ORM you are choosing and {DATABASE} is the Db you are using with the ORM.

To configure our *Outbox* we then need to use the Use{DATABASE}Outbox method call, where {DATABASE} is the {DATABASE} that we want, passing in the configuration for our Db so that we can access it. In our case this will be **UseMySqlOutbox()**.

As we want to use an ORM, in our case EF Core, we have to tell the Outbox how to access EF Core transactions - as we need to participate in a transaction with the ORM. We call a method for the Db, Use{DATABASE}TransactionConnectionProvider, where {DATABASE} is our Db, so in our case **UseMySqlTransactionConnectionProvider()**.

As a parameter to Use{DATABASE}TransactionConnectionProvider we need to provide a *Transaction Provider* for the ORM we are using, in our case this is *MySqlEntityFrameworkConnectionProvider<>).

Finally, if we want the *Outbox* to use a background thread to clear un-dispatched items from the *Outbox*, and we do in most circumstances, otherwise they will not be dispatched, we need to run an *Outbox Sweeper* to do this work.

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