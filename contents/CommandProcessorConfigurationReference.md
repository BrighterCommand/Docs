# Command Processor Configuration Reference

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Every option for configuring a Brighter **Command Processor** — the service
collection extensions, the validation Brighter applies to them, and the
`IBrighterBuilder` fluent interface that configures an External Bus, an Outbox
and JSON serialisation.

This page is consulted rather than read through. For the one path that works,
start at [Basic Configuration](/contents/BrighterBasicConfiguration.md).

## Command Processor Service Collection Extensions

Brighter's package:

* **Paramore.Brighter.Extensions.DependencyInjection** 
 
 provides extension methods for **ServiceCollection** that can be used to add Brighter to the .NET Core DI Framework.

By adding the package you can call the **AddBrighter()** extension method.

If you are using a **Startup** class's **ConfigureServices** method  call the following:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
}

```

if you are using .NET 6 you can make the call directly on your **HostBuilder**'s Services property.

The **AddBrighter()** method takes an **`Action<BrighterOptions>`** delegate. The extension method supplies the delegate with a **BrighterOptions** object that allows you to configure how Brighter runs.

The **AddBrighter()** method returns an **IBrighterBuilder** interface. **IBrighterBuilder** is a [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) that you can use to configure additional Brighter properties (see [Brighter Builder Fluent Interface](#brighter-builder-fluent-interface)).

### Adding Polly Resilience Pipelines

**V10**: Brighter supports Polly v8 resilience pipelines for both internal reliability and custom resilience strategies in your handlers.

To use Polly with Brighter, register resilience pipelines with a **ResiliencePipelineRegistry<string>**:

``` csharp
using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.CircuitBreaker;

var resiliencePipelineRegistry = new ResiliencePipelineRegistry<string>();

resiliencePipelineRegistry.TryAddBuilder("RetryPipeline",
    (builder, context) => builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(50),
        BackoffType = DelayBackoffType.Linear
    }));

resiliencePipelineRegistry.TryAddBuilder("CircuitBreakerPipeline",
    (builder, context) => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(30)
    }));
```

And use them in your handler like this:

``` csharp
// ...
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline(policy: "RetryPipeline", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        // Do work that could throw errors due to distributed computing reliability
        return base.Handle(command);
    }
}
```

See the section [Policy Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) for more on using Polly resilience pipelines with handlers.

With the resilience pipeline registry configured, you need to tell Brighter where to find it:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
        options.PolicyRegistry = new PolicyRegistry() // Optional: for legacy Polly v7 policies
    )
    .ConfigureResiliencePipelines(registry =>
    {
        registry.TryAddBuilder("RetryPipeline", /* ... */);
        registry.TryAddBuilder("CircuitBreakerPipeline", /* ... */);
    });
}
```

> **Note**: For legacy Polly v7 policies using `[UsePolicy]`, see the [migration guide](/contents/PolicyRetryAndCircuitBreaker.md#migration-guide-v9-to-v10) for updating to V10 resilience pipelines.

### Configuring Lifetimes

Brighter can register your *Request Handlers* and *Message Mappers* for you (see [IBrighter Builder Fluent Interface](#brighter-builder-fluent-interface)). 

When we register *Request Handlers* and *Message Mappers* for you with ServiceCollection, we need to register them with a given lifetime (see [Dependency Injection Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)).

We recommend the following lifetimes:

* If you are using *Scoped* lifetimes, for example with EF Core, make your *Request Handlers* *Scoped* as well.
* If you are not using *Scoped* lifetimes you can use *Transient* lifetimes for *Request Handlers*.
* You can use the default Transient lifetime for *Message Mappers* but as your *Message Mapper* generally does not have state that persists between invocations you can save allocations by making it a *Singleton*.

(Be cautious about using *Singleton* lifetimes for *Request Handlers*. Even if your *Request Handler* is stateless today, and so does not risk carrying state across requests, a common bug is that state is added to an existing *Request Handler* which has previously been registered as a *Singleton*.)

You configure the lifetimes for the different types that Brighter can create at run-time as follows:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
    );
}

```

### Service Provider Function Overloads

In addition to the standard `Action<BrighterOptions>` delegate, Brighter provides overloads that accept `Func<IServiceProvider, T>` delegates. These enable **deferred resolution** - your configuration code can access services from the DI container that may be registered later in the setup process.

**Why Use Service Provider Overloads?**

- **Deferred Resolution**: Access services registered elsewhere in your DI configuration
- **Runtime Service Access**: Resolve services like `IConfiguration`, custom factories, or test doubles at runtime
- **Test Isolation**: Each test can have its own isolated configuration without static state conflicts

**AddBrighter with Service Provider**

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    // Register a custom factory first
    services.AddSingleton<IAmARequestContextFactory, MyCustomContextFactory>();

    // Use the Func<IServiceProvider, BrighterOptions> overload
    services.AddBrighter(sp => new BrighterOptions
    {
        HandlerLifetime = ServiceLifetime.Scoped,
        MapperLifetime = ServiceLifetime.Singleton,
        RequestContextFactory = sp.GetRequiredService<IAmARequestContextFactory>()
    })
    .AutoFromAssemblies();
}
```

**AddProducers with Service Provider**

The `AddProducers` method also supports a service provider overload for configuring your external bus producers:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
    })
    .AddProducers(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("RabbitMQ");

        return new ProducersConfiguration
        {
            ProducerRegistry = new RmqProducerRegistryFactory(
                new RmqMessagingGatewayConnection
                {
                    AmpqUri = new AmqpUriSpecification(new Uri(connectionString)),
                    Exchange = new Exchange("paramore.brighter.exchange"),
                },
                new RmqPublication[]
                {
                    new RmqPublication
                    {
                        Topic = new RoutingKey("GreetingMade"),
                        MakeChannels = OnMissingChannel.Create
                    }
                }
            ).Create()
        };
    })
    .AutoFromAssemblies();
}
```

**AddConsumers with Service Provider**

Similarly, `AddConsumers` supports deferred resolution:

``` csharp
// ...
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();

        return new ConsumersOptions
        {
            Subscriptions = GetSubscriptions(sp),
            DefaultChannelFactory = CreateChannelFactory(sp, configuration),
            HandlerLifetime = ServiceLifetime.Scoped
        };
    })
    .AutoFromAssemblies();
}
```

### Using the Options Pattern

Brighter integrates with Microsoft's [Options pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options), allowing you to use `Configure<T>` and `PostConfigure<T>` to modify options after initial registration. This is particularly useful for testing scenarios where you need to override specific settings.

**PostConfigure for Test Overrides**

The `PostConfigure<BrighterOptions>` method runs after all `Configure` calls, allowing you to override settings for tests:

``` csharp
// ...
// In your test setup
public class MyIntegrationTests
{
    private ServiceProvider BuildTestServiceProvider()
    {
        var services = new ServiceCollection();

        // Standard Brighter configuration (could be shared with production)
        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .AutoFromAssemblies();

        // Test-specific overrides using PostConfigure
        services.PostConfigure<BrighterOptions>(options =>
        {
            options.RequestContextFactory = new TestRequestContextFactory();
        });

        return services.BuildServiceProvider();
    }
}
```

**Combining with Standard .NET Options Patterns**

You can combine Brighter's configuration with other Options pattern features:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    // Bind from configuration
    services.Configure<BrighterOptions>(Configuration.GetSection("Brighter"));

    // Add Brighter with defaults
    services.AddBrighter(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
    })
    .AutoFromAssemblies();

    // Environment-specific overrides
    if (Environment.IsDevelopment())
    {
        services.PostConfigure<BrighterOptions>(options =>
        {
            options.RequestContextFactory = new DebugRequestContextFactory();
        });
    }
}
```

**Benefits for Parallel Test Execution**

The combination of service provider overloads and the Options pattern enables parallel test execution without the need for test serialization:

- Each test can build its own `ServiceProvider` with isolated configuration
- No static state conflicts between tests running in parallel
- Test-specific overrides don't affect other tests

``` csharp
// ...
public class ParallelTestsA
{
    [Fact]
    public async Task Test_With_Custom_Context()
    {
        var services = new ServiceCollection();
        services.AddBrighter(options => { })
            .AutoFromAssemblies();

        services.PostConfigure<BrighterOptions>(o =>
            o.RequestContextFactory = new TestContextFactoryA());

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IAmACommandProcessor>();
        // Test runs isolated from other parallel tests
    }
}

public class ParallelTestsB
{
    [Fact]
    public async Task Test_With_Different_Context()
    {
        var services = new ServiceCollection();
        services.AddBrighter(options => { })
            .AutoFromAssemblies();

        services.PostConfigure<BrighterOptions>(o =>
            o.RequestContextFactory = new TestContextFactoryB());

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IAmACommandProcessor>();
        // Runs in parallel with TestsA without conflicts
    }
}
```

## Validating Your Configuration

We recommend enabling pipeline validation and diagnostics as part of your standard configuration. This catches common misconfiguration errors — such as sync/async mismatches, incorrect attribute ordering, and missing handlers — at startup rather than at runtime.

``` csharp
// ...
builder.Services.AddBrighter(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
    })
    .AutoFromAssemblies()
    .ValidatePipelines()
    .DescribePipelines();
```

**ValidatePipelines** checks your configuration and throws a `PipelineValidationException` if errors are found. **DescribePipelines** logs a structured report of your configured pipelines to `ILogger`. Both are opt-in and independent of each other.

For full details on what gets checked, how to configure validation flags, and how to interpret the diagnostic report, see [Pipeline Validation and Diagnostics](/contents/PipelineValidation.md).

## Brighter Builder Fluent Interface

### Type Registration

The **IBrighterBuilder** fluent interface can scan your assemblies for your *Request Handlers* (inherit from **IHandleRequests<>** or **IHandleRequestsAsync<>**) and *Message Mappers* (inherit from **IAmAMessageMapper<>**) and register then with the **ServiceCollection**. This is the most common way to register your code.

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AutoFromAssemblies();
}

```

The code scans any loaded assemblies. If you need to register types from assemblies that are not yet loaded, you can provide a list of additional assemblies to scan as an argument to the call to **AutoFromAssemblies()**.

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AutoFromAssemblies(typeof(MyRequestHandlerAsync).Assembly);
}

```

Instead of using **AutoFromAssemblies** you can exert more fine-grained control over the registration, by explicitly registering your *Request Handlers* and *Message Mappers*. We recommend this for cases where the automatic registration does not meet your needs, such as using Brighter's support for an Agreement Dispatcher:

* **MapperRegistryFromAssemblies()**, **HandlersFromAssemblies()** and **AsyncHandlersFromAssemblies** are the methods called by **AutoFromAssemblies()** and can be called explicitly.
* **Handlers()**, **AsyncHandlers()** and **MapperRegistry()** accept an **Action<>** delegate that respectively provide you with **IAmASubscriberRegistry** or **IAmAnAsyncSubscriberRegistry** to register your RequestHandlers explicitly or a **ServiceCollectionMapperRegistry** to register your mappers. This gives you explicit control over what you register.

### Using an External Bus

Using an *External Bus* allows you to send messages between processes using a message-oriented middleware transport (such as RabbitMQ or Kafka). (For symmetry, we refer to the usage of the *Command Processor* without an external bus as using an *Internal Bus*).

When raising a message on the *Internal Bus*, you use one of the following methods on the *Command Processor*:

* **Send()** and **SendAsync()** - Sends a *Command* to one *Request Handler*.
* **Publish()** and **PublishAsync()** - Broadcasts an *Event* to zero or more *Request Handlers*.

When raising a message on an *External Bus*, you use the following methods on the *CommandProcessor*:

* **Post()** and **PostAsync()** - Immediately posts a *Command* or *Event* to another process via the external Bus
* **DepositPost()** and **DepositPostAsync()** - Puts one or many *Command*(s) or *Event*(s) in the *Outbox* for later delivery
* **ClearOutbox()** and **ClearOutboxAsync()** - Clears the *Outbox*, posting un-dispatched messages to another process via the *External Bus*.
* **ClearAsyncOutbox()** - Implicitly clears the **Outbox**, similar to above however allows bulk dispatching of messages onto a **Transport**.

The major difference here is whether or not you wish to use an *Outbox* for Transactional Messaging. (See [Outbox Pattern](/contents/OutboxPattern.md) and [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) for more on Brighter and the Outbox Pattern).

### Configuring an External Bus

To use an *External Bus*, you need to supply Brighter with configuration information that tells Brighter what middleware you are using and how to find it. (You don't need to do anything to configure an *Internal Bus*, it is always available.)

The **IBrighterBuilder** interface returned from **AddBrighter** allows you to configure the properties of your external bus, by calling the **AddProducers** extension method. The AddProducers extension method takes a lambda function, whose only parameter is an **ExternalBusConfiguration**. 

``` csharp
// ...
private void ConfigureBrighter(IServiceCollection services)
{
    services.AddBrighter(options =>
        {
            ...
        })
        .AddProducers((configure) =>
        {
        })
        .AutoFromAssemblies();

```

### Transports

*Transports* are how Brighter supports specific Message-Oriented-Middleware (MoM). *Transports* are provided in separate NuGet packages so that you can take a dependency only on the transport that you need. Brighter supports a number of different *transports*. 

We use the naming convention **Paramore.Brighter.MessagingGateway.{TRANSPORT}** for *transports* where {TRANSPORT} is the name of the middleware. 

In this example we will use the transport for RabbitMQ, provided by the NuGet package: 

* **Paramore.Brighter.MessagingGateway.RMQ**

See the documentation for detail on specific *transports* on how to configure them for use with Brighter, for now it is enough to know that you need to provide a *Messaging Gateway* which tells us how to reach the middleware and a *Publication* which tells us how to configure the middleware.

### Publications

A *Publication* configures a transport for sending a message to it's associated MoM. So an **RmqPublication** configures how we publish a message to RabbitMQ. There are a number of common properties to all publications.

* **MakeChannels**: Do you want Brighter to create the infrastructure? Brighter can create infrastructure that it needs, and is aware of: **OnMissingChannel.Create**. So a publication can create the topic to send messages to. Alternatively if you create the channel by another method, such as IaaC, we can verify the infrastructure on startup: **OnMissingChannel.Validate**. Finally, you can avoid the performance cost of runtime checks by assuming your infrastructure exists: **OnMissingChannel.Assume**.
* **MaxOutstandingMessages**: How large can the number of messages in the Outbox grow before we stop allowing new messages to be published and raise an **OutboxLimitReachedException**.
* **MaxOutStandingCheckIntervalMilliSeconds**: How often do we check to see if the Outbox is full.
* **Topic**: A Topic is the key used within the MoM to route messages. Publishers publish to a topic and subscribers, subscribe to it. We use a class **RoutingKey** to encapsulate the identifier used for a topic. The name the MoM uses for a topic may vary. Kafka & SNS use *topic* whilst RMQ uses *routingkey* 

### Producer Registry

In order to provide Brighter with the means to send a message via the transport, we need to provide it with an **IAmAProducerRegistry** for the transport you intend to use for the *External Bus*.

A **producer** is the transport specific code that you need to send messages; it implements **IAmAMessageProducer**.

We register a **producer** with a **producer registry**; it needs to implement **IAmAProducerRegistry** but usually you will use the provided **ProducerRegistry**. At runtime,  we lookup the producer to use in the registry by **routing key** (aka topic).

Typically for a transport we implement a **producer registry factory**; it needs to implement **IAmAProducerRegistryFactory**. For example, for the RMQ transport, we provide **RmqProducerRegistryFactory**. A **producer registry factory** typically takes a **connection** to the broker and a collection of **publications**, and it iterates over the **publications** creating a producer for each one and registering it in a **producer registry**. It then returns a configured **producer registry**.

The following code shows an application using the RMQ transport support to create its **producer registry**.

``` csharp
// ...
var producerRegistry = new RmqProducerRegistryFactory(
    new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
        Exchange = new Exchange("paramore.brighter.exchange"),
    },
    new RmqPublication[]
    {
        new RmqPublication
        {
            Topic = new RoutingKey("GreetingMade"),
            MaxOutStandingMessages = 5,
            MaxOutStandingCheckIntervalMilliSeconds = 500,
            WaitForConfirmsTimeOutInMilliseconds = 1000,
            MakeChannels = OnMissingChannel.Create
        }
    }
).Create();
```


### Bus Example

Putting this together, an example configuration for an External Bus for a local RabbitMQ instance could look like this:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AddProducers((configure) =>
        {
            configure.ProducerRegistry = new RmqProducerRegistryFactory(
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
            ).Create();
        })
        .AutoFromAssemblies()

            ...
}
```

### Outbox Support

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

### Configuring the Outbox

To configure our *Outbox* we need to use the **ExternalBusConfiguration**.

An Outbox has three pieces:

* The *Outbox*, which implements **IAmAnOutbox**. Brighter provides implementations for a range of common Dbs. 
* The *Connection Provider* which tells Brighter how to connect to the *Outbox*
* The *Transaction Provider* which allows Brighter to participate in the same transaction that you update an entity with.

In this example, we want to use EF Core with an MySQL Outbox. See the documentation for Outboxes for specific configuration options.

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
            
    var outboxConfiguration = new RelationalDatabaseConfiguration(DbConnectionString());
    services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);

    services.AddBrighter(...)
        .AddProducers((configure) =>
        {
            configure.Outbox = new MySqlOutbox(outboxConfiguration);
            configure.TransactionProvider = typeof(MySqlEntityFrameworkConnectionProvider<GreetingsEntityGateway>);
            configure.ConnectionProvider = typeof(MySqlConnectionProvider);
        })
        .AutoFromAssemblies();

        ...
}

```

Typically **DbConnectionString** would obtain the connection string for the Db from configuration.

### Outbox Sweeper

Finally, if we want the *Outbox* to use a background thread to clear un-dispatched items from the *Outbox*, and we do in most circumstances,  we need to run an *Outbox Sweeper* to do this work. (You can force an immediate clear within the code that produces the outgoing message using **ClearOutbox**, but you should still have a sweeper to guarantee it is sent if that call fails).

Typically you run one sweeper. Brighter provides a variety of distributed lock implementations to help you run a single sweeper at a time, with other sweepers available as a hot standby. Typically, you run the Sweeper in a stand-alone console project. The outbox documentation looks at your strategies for ensuring only one sweeper runs.

For development purposes though, you may wish to add a sweeper to the instance that you are currently running.

To add the *Outbox Sweeper* you will need to take a dependency on another NuGet package:

* **Paramore.Brighter.Outbox.Hosting**

You can then add a sweeper using "UseOutboxSweeper"

For a development version, using an internal sweeper, this results in:

``` csharp
// ...
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AddProducers(...)
         .UseOutboxSweeper()
         .AutoFromAssemblies();
        ...
}

```

### Request-Reply

(**AddProducers()** has optional parameters for use with Request-Reply support for some transports. We don't cover that here, instead see [Direct Messaging](/contents/Routing.md#commands) for more).

### Configuring JSON Serialization

Brighter defines a set of serialization options for use when it needs to serialize messages to JSON. Internally we use these options in our transports, when serializing messages to an external bus and deserializing from an external bus. We also use them in default JSON message mappers. You may wish to use these options in your own [*Message Mapper*](/contents/MessageMappers.md) implementation.

By default our JSONSerialization Options are configured as follows:

``` csharp
// ...
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
// ...

.ConfigureJsonSerialisation((options) =>
{
    options.PropertyNameCaseInsensitive = true;
})

```

If you want to use this configured set of JSON Serialization options in your own code, you can, by using the static property JsonSerialisationOptions.Options. For example:

```csharp
// ...
public GreetingMade MapToRequest(Message message)
{
    return JsonSerializer.Deserialize<GreetingMade>(message.Body.Value, JsonSerialisationOptions.Options);
}
```

### Retry and Circuit Breaker with an External Bus

When sending a request via the External Bus we use a Polly policy internally to control Retry and Circuit Breaker in case the External Bus is not available. These policies have defaults but you can configure the behavior using the policy keys:

* **Paramore.RETRYPOLICY**
* **Paramore.CIRCUITBREAKER**



## Further Reading

- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — the configuration path most applications should follow
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md) — the consumer-side equivalent of this page
- [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md) — configuring Brighter without .NET Core dependency injection
