# Dispatcher Configuration Reference

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Every option for configuring a Brighter **Dispatcher** — the service collection
extensions that register it, its subscriptions, gateway connections, channel
factories and lifetimes, and the Inbox option that matters when receiving
requests.

This page is consulted rather than read through. For the one path that works,
start at [Basic Configuration](/contents/BrighterBasicConfiguration.md).

## Dispatcher Service Collection Extensions

We provide support for configuring .NET Core's **HostBuilder** as a message consumer (Dispatcher) for use with MoM. We use Brighter's Command Processor to dispatch the messages read by the Dispatcher. If you are not using **HostBuilder** then you will need to configure the Dispatcher yourself. See [How Configuring the Dispatcher Works](/contents/HowConfiguringTheDispatcherWorks.md) for more.

To use Brighter's Dispatcher with **HostBuilder** you will need to take a dependency on the following NuGet packages:

* `Paramore.Brighter.ServiceActivator.Extensions.Hosting`
* `Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection`

These provide an extension method **AddConsumers()** that can be used to add Brighter to the .NET Core DI Framework.

By adding the package you can call the **AddConsumers()** extension method.

If you are using a **HostBuilder** class's **ConfigureServices** method  call the following:

``` csharp
// ...
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            services.AddConsumers(...)
        }

```

if you are using .NET 6 you can make the call direction on your **HostBuilder**'s Services property.

The **AddConsumers()** method takes an **`Action<ServiceActivatorOptions>`** delegate. The extension method supplies the delegate with a `ServiceActivatorOptions` object that allows you to configure how Brighter runs.

The **AddConsumers()** method returns an **IBrighterBuilder** interface. **IBrighterBuilder** is a [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface) that you can use to configure Brighter *Command Processor* properties. It is described in [Brighter Builder Fluent Interface](/contents/CommandProcessorConfigurationReference.md#brighter-builder-fluent-interface) and the same options apply. We discuss one additional option that becomes important when receiving requests, the *Inbox*, in [Additional Brighter Builder Options](#inbox).

### Subscriptions

When configuring your application's Dispatcher (message consumer), your *Subscriptions* configure how your application will receive messages from the associated MoM queues or streams.

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
* **MessagePumpType**: Chooses the concurrency model: **MessagePumpType.Reactor** (blocking I/O, lower latency) or **MessagePumpType.Proactor** (non-blocking I/O, higher throughput). The Proactor pattern can increase throughput where a handler is I/O bound by allowing the message pump to yield the thread whilst awaiting I/O completion. The cost is that strict ordering of messages may be lost as processing of I/O bound requests may complete out-of-sequence. Brighter provides its own synchronization context for async operations. We recommend scaling via increasing the number of performers, unless you know that I/O is your bottleneck. See [Reactor and Proactor](ReactorAndProactor.md) for details.
* **TimeOut**: How long does a read 'wait' before assuming there are no pending messages.
* **UnaceptableMessageLimit**: Brighter will ack a message that throws an unhandled exception, thus removing it from a queue. 

For a more detailed discussion of using Requeue (with Delay) for Handler failure, (**RequeueCount** and **RequeueDelayInMilliseconds**) along with termination of a consumer due to message failure (**UnacceptableMessageLimit**) see [Handler Failure](/contents/HandlerFailure.md)

In addition, individual transports that provide access to specific MoM sub-class *Subscription* to provide properties unique to the chosen middleware. We discuss those under a section for that transport.

For RabbitMQ for example, this would look like this:

``` csharp
// ...
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
            messagePumpType: MessagePumpType.Proactor,
            timeOut: TimeSpan.FromMilliseconds(200),
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    services.AddConsumers(options =>
        {
            options.Subscriptions = subscriptions;
        })
}

...

```

### Gateway Connections & Channel Factories

A *Gateway Connection* tells Brighter how to connect to MoM for a particular transport. The transport package will contain a *Gateway Connection*, you need to provide the information to connect to your middleware (URIs, ports, credentials etc.) Your transport package provides a *Gateway Connection*

A *Channel Factory* connects Brighter to MoM. Depending on the configuration settings for your *Subscription* it may create the required primitives (topics/routing keys, queues, streams) on MoM or simply attach to ones that you have created via Infrastructure as Code (IaC). Your transport provides a *Channel Factory* and you need to pass it a *Gateway Connection*.

For RabbitMQ, this would look like:

``` csharp
// ...
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

    services.AddConsumers(options =>
        {
             options.DefaultChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
        })
}

...

```

### Configuring Dispatcher Lifetimes

Under the hood your Dispatcher uses a *Command Processor* and you will need to configure lifetimes as described in [Configuring Lifetimes](/contents/CommandProcessorConfigurationReference.md#configuring-lifetimes).

``` csharp
// ...
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
            options.MapperLifetime = ServiceLifetime.Singleton;
        })
}

...

```
## Dispatcher Brighter Builder Fluent Interface

The call to **AddConsumers()** returns an **IBrighterBuilder** fluent interface. This means that you can use any of the options described in [Brighter Builder Fluent Interface](/contents/CommandProcessorConfigurationReference.md#brighter-builder-fluent-interface) to configure the associated *Command Processor* such as scanning assemblies for *Request Handlers* and adding an *External Bus* and *Outbox*.

An option intended for the context of a Dispatcher (message consumer) is described below.

### Inbox

As described in the [Outbox Pattern](/contents/OutboxPattern.md) an *Outbox* offers **Guaranteed, At Least Once** delivery. It explicitly may result in you sending duplicate messages. In addition, MoM tends to offer "At Least Once" guarantees only, further creating the risk that you will receive a duplicate message.

If the request is not idempotent, you can use an Inbox to de-duplicate it. See [Inbox Support](/contents/BrighterInboxSupport.md) for more.

Configuring an *Inbox* has two elements. The first is the type of *Inbox*, the second configuration for the *Inbox* behavior.

Brighter provides a number of *Inbox* implementations for common Dbs (and you can write your own for a Db that we do not support). For this discussion we will look at Brighter's support for working with MySQL. See the documentation for working with specific *Inbox* implementations.

For this we will need the *Inbox* packages for the MySQL *Inbox*.

* **Paramore.Brighter.Inbox.MySql**

For a given backing store the pattern should be Paramore.Brighter.Inbox.{DATABASE} where {DATABASE} is the name of the Db that you are using.

To configure our *Inbox* we then need to use the UseExternalInbox method call and pass in an instance of a class that implements **IAmAnInbox**, taken from our package, and an instance of **InboxConfiguration** that tells Brighter how we want to use the Inbox.

For *Inbox Configuration* you set the following properties:

* **ActionOnExists**: What do we do if the request has been handled? The default,**OnceOnlyAction.Throw** is to throw a **OnceOnlyException**. If you take no other action this will cause the message to be rejected and sent to a DLQ if one is configured (See [Handler Failure](/contents/HandlerFailure.md)). The alternative is **OnceOnlyAction.Warn** simply logs that the request is a duplicate, but takes no other action. A third option, **OnceOnlyAction.Replay**, also skips the handler but resends the messages that handler produced the first time it ran — it has prerequisites, so see [Replay On Seen](/contents/ReplayOnSeen.md) before you choose it.
* **OnceOnly**: This defaults to *true* and will check for a duplicate and take the action indicated by **ActionOnExists**. If *false* the *Inbox* will record the request, but will take no further action. (This tends to be set to *false* if you are using the *Inbox* to record what requests caused current state only and not de-duplicate).
* **Scope**: This indicates the type of request (*Command* or *Event*) to store in the *Inbox*. By default this is set to **InboxScope.All** and captures everything but you can be explicit and just capture **InboxScope.Commands** or **InboxScope.Events**. (This tends to be set to **InboxScope.Commands** when only commands cause changes to state that are not idempotent).
* **Context**: Used to uniquely identify receipt of this request via this handler. If you are recording *Events* and have multiple handlers, then the first event handler to receive the message will block the others from doing so, unless you disambiguate the handler identity by supplying a context method.

A typical *Inbox* configuration for MySQL would be:

``` csharp
// ...
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(options =>
        {   
            options.InboxConfiguration =  new InboxConfiguration(
                    inbox: new MySqlInbox(new RelationalDatabaseConfiguration(DbConnectionString()))
                    scope: InboxScope.Commands,
                    onceOnly: true,
                    actionOnExists: OnceOnlyAction.Throw
            ...
        });
 }
 
 ...
```
Typically **DbConnectionString** would obtain the connection string for the Db from configuration.


## Further Reading

- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — the configuration path most applications should follow
- [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md) — the producer-side equivalent of this page
- [How Configuring the Dispatcher Works](/contents/HowConfiguringTheDispatcherWorks.md) — configuring a Dispatcher without HostBuilder
