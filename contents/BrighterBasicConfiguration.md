---
description: "Configuration is the most labor-intensive part of using Brighter."
layout:
  description:
    visible: false
---

# Basic Configuration

> **How-to** · Applies to **Brighter V10**

Configuration is the most labor-intensive part of using Brighter. Once you have configured Brighter, using its model of requests and handlers is straightforward.

## Using .NET Core Dependency Injection

This section covers using .NET Core Dependency Injection to configure Brighter. If you want to use an alternative DI container then see the section [How Configuration Works](/contents/HowConfiguringTheCommandProcessorWorks.md)

We divide configuration into two sections, depending on your requirements:

* [**Configuring The Command Processor**](#configuring-the-command-processor): This section covers configuring the **Command Processor**. Use this if you want to dispatch requests to handlers, or publish messages from your application on an external bus
* [**Configuring The Dispatcher**](#configuring-the-dispatcher): This section covers configuring the **Dispatcher** (message consumer). Use this if you want to read messages from a transport (and then dispatch to handlers).

## Configuring The Command Processor

Take a dependency on **Paramore.Brighter.Extensions.DependencyInjection**, call
`AddBrighter()` on your service collection, and let Brighter find your *Request
Handlers* and *Message Mappers* by scanning the assembly that holds your
requests:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;

builder.Services
    .AddBrighter()
    .AutoFromAssemblies([typeof(GreetingCommand).Assembly]);
```

That is the whole of the minimum. `AddBrighter()` takes an optional
`Action<BrighterOptions>` for handler and mapper lifetimes, a policy registry and
the rest. `AutoFromAssemblies()` already scans every non-framework assembly loaded
into the AppDomain — Brighter's own assemblies, `Microsoft.*` and `System.*` are
excluded — and its argument names *additional* assemblies to scan on top of those,
which is what you need when an assembly holding handlers has not been loaded yet.
Every option either call accepts is in
[Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md).

Add an External Bus, an Outbox or a Sweeper on top of that call when you need
them — each is a further method on the `IBrighterBuilder` the call returns, and
each is documented on that same reference page.

### Putting It All Together

Putting all this together, a typical configuration might looks as follows:

``` csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.MySql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MySql;

public void ConfigureServices(IServiceCollection services)
{

    var outboxConfiguration = new RelationalDatabaseConfiguration(DbConnectionString());
    services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);

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
                    WaitForConfirmsTimeOutInMilliseconds = 1000,
                    MakeChannels = OnMissingChannel.Create
                }}
            ).Create();

           // Outbox thresholds are producers configuration, not publication
           configure.MaxOutStandingMessages = 5;
           configure.MaxOutStandingCheckInterval = TimeSpan.FromMilliseconds(500);
           configure.Outbox = new MySqlOutbox(outboxConfiguration);
           configure.TransactionProvider = typeof(MySqlEntityFrameworkConnectionProvider<GreetingsEntityGateway>);
           configure.ConnectionProvider = typeof(MySqlConnectionProvider);
        })
        .UseOutboxSweeper()
        .AutoFromAssemblies();
}

```

## Configuring The Dispatcher

A *consumer* reads messages from Message-Oriented Middleware (MoM), and a *producer* puts messages onto the MoM for the *consumer* to read.

A *consumer* waits for messages to appear on the queue, reads them, and then calls your *Request Handler* code to react. The component that listens for messages and dispatches them to handlers is called a **Dispatcher**. (In Enterprise Integration Patterns terminology, this is called a [*Service Activator*](https://www.enterpriseintegrationpatterns.com/patterns/messaging/MessagingAdapter.html), and the assembly name reflects this, but we use "Dispatcher" for simplicity.) <!-- pagelint: allow-serviceactivator -->

To use Brighter's Dispatcher you will need to take a dependency on the NuGet package:

* `Paramore.Brighter.ServiceActivator` (assembly name, the component is called Dispatcher)

The service collection extensions, subscriptions, gateway connections, channel
factories, lifetimes and the Inbox option are all in
[Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md). What follows is running
the Dispatcher once you have configured it.

### Running the Dispatcher

To run the Dispatcher we add it as a [Hosted Service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio). 

We provide the class `ServiceActivatorHostedService` for this in the NuGet package:

* `Paramore.Brighter.ServiceActivator.Extensions.Hosting`

The `ServiceActivatorHostedService` calls the **Dispatcher.Receive** method which starts message pumps for the configured *Subscriptions*.

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

### A Complete Dispatcher Example

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
            messagePumpType: MessagePumpType.Proactor,
            timeOut: TimeSpan.FromMilliseconds(200),
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri($"amqp://guest:guest@localhost:5672")),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
        options.UseScoped = true;
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
        options.CommandProcessorLifetime = ServiceLifetime.Scoped;
        options.PolicyRegistry = new SalutationPolicy();
        options.InboxConfiguration =  new InboxConfiguration(
            inbox: new MySqlInbox(new RelationalDatabaseConfiguration(DbConnectionString()))
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Throw
        );
    })
    .AddProducers((configure) =>
    {
        configure.ProducerRegistry = producerRegistry;
        configure.Outbox = outbox;
        configure.TransactionProvider = transactionProvider;
        configure.ConnectionProvider = connectionProvider;
    })
    .AutoFromAssemblies();
    
    services.AddHostedService<ServiceActivatorHostedService>();

}

```


## Samples

Brighter includes a comprehensive set of [Samples](https://github.com/BrighterCommand/Brighter/tree/master/samples) in its main repo that you can review for clarity on how Brighter works and should be configured.

## Further Reading

- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md) — every producer-side option in one place
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md) — every consumer-side option in one place
- [V10 Migration Guide](/contents/V10MigrationGuide.md) — what changed from V9, including the configuration method renames
- [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md) — using an alternative DI container
