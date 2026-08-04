# RabbitMQ Configuration

> **Reference** · Applies to **Brighter V10**

## RabbitMQ General

RabbitMQ is OSS message-oriented-middleware and is [well documented](https://www.rabbitmq.com/documentation.html). Brighter handles the details of sending to or receiving from RabbitMQ. You may find it useful to understand the [building blocks](https://www.rabbitmq.com/tutorials/amqp-concepts.html) of the protocol. You might find the [documentation for the .NET SDK](https://www.rabbitmq.com/dotnet-api-guide.html) helpful when debugging, but you should not have to interact with it directly to use Brighter.

RabbitMQ offers an API that defines primitives used to configure the middleware used for messaging:

- **Exchange**: A routing table. Different types of exchanges route messages differently. An entry in the table is a **Routing Key**.
- **Queue**: A store-and-forward queue over which a consumer receives messages. A message is locked whilst a consumer has read it, until they ack it, upon which it is deleted from the queue, or nack it, upon which it is requeued or sent to a DLQ.
- **Binding**: Adds a queue as a target for a routing rule on an exchange. The routing key is used for this on a direct exchange (on the default exchange the routing key is the queue name).

We connect to RabbitMQ via a multiplexed TCP/IP connection - RabbitMQ calls these channels. Brighter uses a push consumer, so it has an open channel and can be seen on the consumers list in the management console. Brighter maintains a pool of connections and when asked for a new connection will take one from it's pool in preference to creating a new one.

## RabbitMQ.Client v7 Support

The `RabbitMQ.Client` library introduced significant breaking changes in version 7, most notably making its API entirely asynchronous. To support this new version without imposing a breaking change on all existing Brighter users, a new, separate package has been created:

- [Paramore.Brighter.MessagingGateway.RMQ.Async](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.RMQ.Async)

This is important because it allows you to choose the implementation that best fits your project:

- For existing projects, you can continue to use the `Paramore.Brighter.MessagingGateway.RMQ.Sync` package with `RabbitMQ.Client` v6.x and its synchronous API.
- For new projects, or when you are ready to adopt the `async`-native client, you can use the new `Paramore.Brighter.MessagingGateway.RMQ.Async` package. This package is designed to work with the fully asynchronous API of `RabbitMQ.Client` v7+, which can offer better performance and aligns with modern .NET asynchronous programming patterns.

## Breaking Changes: Package Rename and Proactor Subscription

With the introduction of the `Paramore.Brighter.MessagingGateway.RMQ.Async` package, the original `Paramore.Brighter.MessagingGateway.RMQ` package has been renamed to `Paramore.Brighter.MessagingGateway.RMQ.Sync`. This change better reflects its synchronous nature.

A significant breaking change is the removal of the proactor subscription model from the `Paramore.Brighter.MessagingGateway.RMQ.Sync` package. The proactor pattern is inherently asynchronous and is better suited for the new fully asynchronous `RabbitMQ.Client` v7.

If your application relies on proactor subscriptions for efficient, non-blocking message consumption, you must migrate to the `Paramore.Brighter.MessagingGateway.RMQ.Async` package. This package provides a native, high-performance asynchronous consumer that integrates correctly with the `RabbitMQ.Client` v7+ API.

## RabbitMQ Connection

The Connection to RabbitMQ is provided by an **RmqMessagingGatewayConnection** which allows you to configure the following:

- **Name**: A unique name for the connection, for diagnostic purposes
- **AmqpUri**: A connection to AMQP in the form of an [RabbitMQ Uri](https://www.rabbitmq.com/uri-spec.html) **Uri** with reliability options for a retry count (defaults to 3), **ConnectionRetryCount**, retry interval (defaults to 1000ms) **RetryWaitInMilliseconds** and a circuit breaker retry timeout (defaults to 60000ms), **CircuitBreakTimeInMilliseconds**, which introduces a delay when connections exceed the retry count.
- **Exchange**: The definition of the exchange. **Name** is the identifier for the exchange. All exchanges have a [**Type**](https://www.rabbitmq.com/tutorials/amqp-concepts.html), and the default is **ExchangeType.Direct**, but it is a string value that supports all RabbitMQ exchange types on the .NET SDK. The **Durable** flag is used to indicate if the exchange definition survives node failure or restart of the broker which defaults to *false*. **SupportDelay** indicates if the Exchange supports retry with delay, which defaults to *false*.
- **DeadLetterExchange**: Another exchange definition, but this one is used to host any Dead Letter Queues (DLQ). This could be the same exchange, but normal practice is to use a different exchange.
- **Heartbeat**: RabbitMQ uses a heartbeat to determine if a connection has died. This sets the interval for that heartbeat. Defaults to 20s.
- **PersistMessages**: Should messages be saved to disk? Saving messages to disk allows them to be recovered if a node fails, defaults to *false*. See [Persistent Messages](#persistent-messages) for more details.
- **ContinuationTimeout**: RabbitMQ protocol timeouts in seconds. Defaults to 20s. See [ConnectionFactory.ContinuationTimeout](https://www.rabbitmq.com/dotnet-api-guide.html) for more information.

In RabbitMQ, recreating an exiting primitive is a no-op provided the definition does not change.

The following code creates a typical RabbitMQ connection (here shown as part of configuring an External Bus):

``` csharp
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

                ...//publication, see below
            
            ).Create();
        }    
}
```

## RabbitMQ Publication

For more on a *Publication* see the material on an *Add Producers* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

We only support one custom property on RabbitMQ which configures shutdown delay to await pending confirmations. 

* **WaitForConfirmsTimeOutInMilliseconds**

Under the hood, Brighter uses [Publisher Confirms](https://www.rabbitmq.com/confirms.html) to update its Outbox for the dispatch time. This means that when publishing a message we allow RabbitMQ to confirm delivery of a message to all available nodes asynchronously, and then call us back, over blocking. This allows for higher throughput. But it means that we cannot update the Outbox to show a message as dispatched, until we receive the callback, which may occur after your handler pipeline for that message has completed and the message has been acknowledged.  

When shutting down a producer, it is possible that not all confirms have yet been received from RabbitMQ. The delay instructs Brighter to wait for a period of time, in order to allow the confirms to arrive. 

Missing a confirm will cause the *Outbox Sweeper* to resend a message, as it will not be marked as dispatched. (This is why we refer to Guaranteed *At Least Once* because there are many opportunities where messages may be duplicated in order to guarantee they were sent).  

The following code creates a *Publication* for RabbitMQ when configuring an *External Bus*

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
      .AddProducers((configure) =>
        {
            configure.ProducerRegistry = new RmqProducerRegistryFactory(
 
                ...//connection, see above

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
}
```

## Putting It Together

Our combined code for the *Connection*  with a single *Publication* looks like this

``` csharp
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
        }
}
```

## RabbitMQ Subscription

For more on a *Subscription* see the material on configuring the *Dispatcher* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-dispatcher).

We support a number of RabbitMQ specific *Subscription* options:

- **DeadLetterChannelName**: The name of the queue to subscribe to DLQ notifications for this subscription (without a queue, the messages sent to the Dead Letter Exchange (DLX) will not be stored) 
- **DeadLetterRoutingKey**: The routing key that binds the DLQ to the DLX
- **HighAvailability**: [Deprecated] Not used on versions of RabbitMQ 3+. Prior to this, configuring that a queue should be mirrored was an API option, now it is a configuration management option on the broker.
- **IsDurable**: Should subscription definitions survive a restart of nodes in the broker.
- **MaxQueueLength**: [Deprecated] Prefer to use policy to set this instead (see [RabbitMQ docs](https://www.rabbitmq.com/maxlength.html)). The maximum length a RabbitMQ queue can grow to, before new messages are rejected (and sent to a DLQ if there is one).

This is a typical *Subscription* configuration in a Consumer application:

``` csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new RmqSubscription<GreetingMade>(
            new SubscriptionName("paramore.sample.salutationanalytics"),
            new ChannelName("SalutationAnalytics"),
            new RoutingKey("GreetingMade"),
            messagePumpType: MessagePumpType.Reactor,
            timeOut: TimeSpan.FromMilliseconds(200),
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(
                    new Uri("amqp://guest:guest@localhost:5672")
                    connectionRetryCount: 5,
                    retryWaitInMilliseconds: 250,
                    circuitBreakerTimeInMilliseconds = 30000
                ),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    services.AddConsumers(options =>
        {
            options.Subscriptions = subscriptions;
            options.ChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
            ... //see Basic Configuration
        })
```

## Quorum Queues
### What are Quorum Queues?

[Quorum queues](https://www.rabbitmq.com/docs/quorum-queues) are a modern queue type introduced in RabbitMQ 3.8, designed to provide high availability and data consistency using the [Raft consensus algorithm](https://raft.github.io/). Unlike classic queues that use mirroring for high availability, quorum queues use a replicated state machine approach that ensures stronger consistency guarantees.

### Classic vs Quorum Queues

| Feature | Classic Queues | Quorum Queues |
|---------|---------------|---------------|
| **Purpose** | High throughput and low latency | High availability and data consistency |
| **Replication** | Mirroring (optional) | Built-in Raft-based replication |
| **Consistency** | Weaker guarantees | Strong consistency (Raft consensus) |
| **Performance** | Higher throughput | Lower throughput, more overhead |
| **Cluster Requirements** | Any cluster size | Requires at least 3 nodes for optimal performance |
| **Durability** | Optional | Required (isDurable must be true) |
| **HighAvailability Flag** | Supported (deprecated in RMQ 3+) | Not supported (highAvailability must be false) |
| **Use Cases** | Real-time streaming, high-volume processing | Financial transactions, critical business processes |

### When to Use Quorum Queues

Use **Quorum Queues** when:

- **Data consistency is critical**: Financial transactions, order processing, critical business logic
- **Message durability is essential**: Messages must survive node failures
- **You have a cluster**: Quorum queues require at least 3 nodes for optimal fault tolerance
- **You can accept lower throughput**: The Raft consensus algorithm adds overhead

Use **Classic Queues** when:

- **High throughput is priority**: Real-time data streaming, high-volume message processing
- **Low latency is required**: Time-sensitive applications
- **Message loss is acceptable**: Non-critical notifications, telemetry data
- **Single-node deployment**: Classic queues work well with a single node

### Configuration Requirements

To use quorum queues, you must configure the subscription with:

- **queueType**: Set to `QueueType.Quorum`
- **isDurable**: Must be `true` (required for quorum queues)
- **highAvailability**: Must be `false` (quorum queues provide their own replication)

```csharp
var subscription = new RmqSubscription<GreetingMade>(
    new SubscriptionName("paramore.sample.salutationanalytics"),
    new ChannelName("SalutationAnalytics"),
    new RoutingKey("GreetingMade"),
    messagePumpType: MessagePumpType.Proactor,
    timeOut: TimeSpan.FromMilliseconds(200),
    isDurable: true,              // Required for quorum queues
    highAvailability: false,      // Must be false for quorum queues
    queueType: QueueType.Quorum,  // Use quorum queue
    makeChannels: OnMissingChannel.Create
);
```

### Validation

If you attempt to create a quorum queue without meeting the configuration requirements, Brighter will throw an exception during queue creation. The validation ensures:

- `isDurable` is `true`
- `highAvailability` is `false`

### Quorum Queue Best Practices

1. **Use at least 3 nodes**: Quorum queues are designed for clusters with at least 3 nodes. A single node or 2-node cluster defeats the purpose of the Raft consensus algorithm.

2. **Monitor queue performance**: Quorum queues have higher overhead than classic queues. Monitor throughput and latency to ensure they meet your requirements.

3. **Use for critical workflows only**: Reserve quorum queues for messages where consistency and durability are critical. Use classic queues for high-throughput, less critical workloads.

4. **Consider message size**: Quorum queues replicate messages across multiple nodes, so large messages can impact network bandwidth and storage.

5. **Plan for cluster capacity**: Each quorum queue replicates to multiple nodes, consuming more disk space and memory than classic queues.

### Migration from Classic to Quorum Queues

To migrate an existing subscription from classic to quorum queues:

1. **Create a new quorum queue** with a different name
2. **Update producers** to publish to the new queue
3. **Deploy new consumers** subscribing to the quorum queue
4. **Drain the classic queue** by processing remaining messages
5. **Remove the classic queue** once drained

Do not attempt to change a classic queue to a quorum queue in place, as this requires deleting and recreating the queue, which would result in message loss.

### Ack and Nack

We use RabbitMQ's queues to subscribe to a routing key on an exchange.

When we Accept/Ack a message, in response to a handler chain completing, we Ack the message to RabbitMQ using **Channel.BasicAck**. Note that we only Ack a message once we have completed running the chain. 

When we Reject/Nack a message (see [Handler Failure](/contents/HandlerFailure.md) for more on failure) then we use **Channel.Reject** to delete the message, and move it to a DLQ if there is one.

Brighter has an internal buffer for messages pushed to a *Performer* (a thread running a message pump). This buffer has thread affinity (in RabbitMQ we have to Ack or Nack from the thread that received the message). When a consumer closes its connection to RabbitMQ, messages in the buffer that have not been Ack'd or Nack'd will be returned to the queue.

## Persistent Messages

RabbitMQ supports message persistence, which saves messages to disk to ensure they survive broker restarts or node failures. Brighter supports persistent messages through the `PersistMessages` configuration property.

### What is Message Persistence?

Message persistence in RabbitMQ involves two components:

1. **Durable Queues**: Queue definitions that survive broker restarts
2. **Persistent Messages**: Individual messages marked for disk storage

When both components are enabled, messages will survive broker restarts. However, there is a small window between when RabbitMQ receives a message and when it's written to disk, during which messages could be lost if the broker crashes.

### Enabling Persistent Messages

To enable message persistence, set `PersistMessages = true` in your `RmqMessagingGatewayConnection`:

```csharp
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange", durable: true),
    PersistMessages = true  // Enable message persistence
};
```

### Configuration for Persistent Messages

For full persistence, you should configure:

**Producer Configuration**:

- Set `PersistMessages = true` on the connection
- Set `durable: true` on the Exchange definition
- Messages will be marked with `DeliveryMode = Persistent`

**Consumer Configuration**:

- Set `isDurable: true` on the subscription
- This ensures the queue definition survives broker restarts

**Complete Example**:

```csharp
// Producer Configuration
services.AddBrighter(...)
    .AddProducers((configure) =>
    {
        configure.ProducerRegistry = new RmqProducerRegistryFactory(
            new RmqMessagingGatewayConnection
            {
                AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                Exchange = new Exchange(
                    name: "paramore.brighter.exchange",
                    type: ExchangeType.Direct,
                    durable: true  // Exchange survives restarts
                ),
                PersistMessages = true  // Messages saved to disk
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
    });

// Consumer Configuration
var subscriptions = new Subscription[]
{
    new RmqSubscription<GreetingMade>(
        new SubscriptionName("paramore.sample.salutationanalytics"),
        new ChannelName("SalutationAnalytics"),
        new RoutingKey("GreetingMade"),
        messagePumpType: MessagePumpType.Proactor,
        timeOut: TimeSpan.FromMilliseconds(200),
        isDurable: true,  // Queue survives restarts
        makeChannels: OnMissingChannel.Create
    )
};

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange(
        name: "paramore.brighter.exchange",
        type: ExchangeType.Direct,
        durable: true
    )
};
```

### Performance Considerations

Message persistence comes with performance trade-offs:

- **Slower throughput**: Writing to disk is slower than keeping messages in memory
- **Increased latency**: Each message write involves disk I/O
- **Disk space**: Persistent messages consume disk storage
- **Fsync operations**: RabbitMQ periodically flushes to disk (configurable)

For high-throughput applications where message loss is acceptable, consider using non-persistent messages (the default).

### When to Use Persistent Messages

Use **Persistent Messages** when:

- **Message loss is unacceptable**: Financial transactions, critical business data
- **Broker restarts must not lose data**: Long-running workflows, state management
- **Regulatory requirements**: Audit trails, compliance scenarios
- **Combined with Outbox pattern**: Ensures at-least-once delivery guarantees

Do not use persistent messages when:

- **High throughput is critical**: Real-time streaming, telemetry data
- **Message loss is acceptable**: Non-critical notifications, cache updates
- **Short message lifetime**: Messages that expire quickly
- **Memory-based queues**: Testing, development environments

### Persistent Message Best Practices

1. **Use with quorum queues**: Quorum queues require persistence and provide stronger durability guarantees.

2. **Enable Publisher Confirms**: Brighter uses publisher confirms by default to ensure RabbitMQ has accepted messages.

3. **Monitor disk space**: Persistent messages consume disk storage. Monitor and alert on disk usage.

4. **Use TTL for persistent messages**: Set a time-to-live on messages to prevent indefinite accumulation.

5. **Combine with Outbox pattern**: For transactional messaging, use the Outbox pattern with persistent messages.

6. **Test recovery scenarios**: Regularly test broker restart scenarios to validate persistence behavior.

## Connection Stability

V10 includes improvements to RabbitMQ connection handling and error recovery, making applications more resilient to network issues and broker restarts.

### Improvements in V10

1. **Enhanced connection pooling**: Improved connection pool management to prevent ghost connections
2. **Better error handling**: More robust error recovery for connection failures
3. **Automatic reconnection**: Improved logic for reconnecting after connection loss
4. **Blocked/Unblocked event monitoring**: Automatic logging of channel blocked events (see below)

### Connection Retry Configuration

The `AmqpUriSpecification` provides several options for connection reliability:

```csharp
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(
        uri: new Uri("amqp://guest:guest@localhost:5672"),
        connectionRetryCount: 5,                  // Number of retry attempts
        retryWaitInMilliseconds: 250,             // Wait between retries
        circuitBreakerTimeInMilliseconds: 30000   // Circuit breaker timeout
    ),
    Exchange = new Exchange("paramore.brighter.exchange"),
    Heartbeat = 20  // Heartbeat interval in seconds
};
```

**Configuration Options**:

- **connectionRetryCount**: Number of times to retry connecting to RabbitMQ before giving up (default: 3)
- **retryWaitInMilliseconds**: Time to wait between retry attempts (default: 1000ms)
- **circuitBreakerTimeInMilliseconds**: Time to wait before attempting to reconnect after exceeding retry count (default: 60000ms)
- **Heartbeat**: Interval for RabbitMQ heartbeat checks to detect dead connections (default: 20s)

### Heartbeat Configuration

RabbitMQ uses heartbeats to detect dead TCP connections. If a connection doesn't send a heartbeat within the configured interval, RabbitMQ considers it dead and closes the connection.

```csharp
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange"),
    Heartbeat = 30  // Check connection health every 30 seconds
};
```

### Best Practices for Connection Stability

1. **Use appropriate retry counts**: Set `connectionRetryCount` based on your network stability and failover time requirements.

2. **Tune heartbeat interval**: Balance between detecting dead connections quickly (shorter interval) and network overhead (longer interval).

3. **Monitor connection events**: Use structured logging to track connection failures and recoveries.

4. **Handle blocked connections**: Monitor blocked/unblocked events to detect backpressure issues (see below).

5. **Use connection pooling**: Brighter manages a connection pool. Avoid creating multiple `RmqMessagingGatewayConnection` instances.

6. **Test network failures**: Regularly test connection resilience with network partitions and broker restarts.

## Blocked and Unblocked Channel Events

RabbitMQ can block connections when resources are exhausted (memory, disk space, or alarms). Brighter automatically subscribes to blocked/unblocked events and logs them for monitoring.

### What are Blocked Connections?

RabbitMQ blocks a connection when:

- **Memory alarm triggered**: Broker memory usage exceeds the threshold
- **Disk alarm triggered**: Broker disk space is low
- **Resource limits**: Other broker resource limits are reached

When blocked, producers cannot publish messages, and the connection is paused until resources are available.

### Automatic Event Logging

Brighter automatically logs blocked and unblocked events:

**Blocked Event**:
```
[Warning] RMQMessagingGateway: Subscription to amqp://localhost:5672 blocked. Reason: {reason}
```

**Unblocked Event**:
```
[Information] RMQMessagingGateway: Subscription to amqp://localhost:5672 unblocked
```

### Monitoring Blocked Connections

To monitor blocked connections in your application:

1. **Enable structured logging**: Configure your logger to capture warnings and information logs from Brighter.

2. **Alert on blocked events**: Set up alerts when connections are blocked to investigate resource issues.

3. **Monitor RabbitMQ metrics**: Use RabbitMQ Management UI or Prometheus to track memory and disk usage.

### Example Logging Configuration

```csharp
// Using Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

// Blocked events will be logged automatically
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});
```

### Handling Blocked Connections in Production

When a connection is blocked:

1. **Check RabbitMQ status**: Use the Management UI to identify the alarm type
2. **Investigate resource usage**: Check memory, disk, and queue depths
3. **Increase resources**: Add more memory/disk or scale out the cluster
4. **Adjust queue policies**: Set max lengths or TTLs to prevent unbounded growth
5. **Monitor continuously**: Set up dashboards and alerts for RabbitMQ health

### Best Practices for Blocked Connections

1. **Monitor resource usage**: Regularly check RabbitMQ memory and disk usage to prevent alarms.

2. **Set resource limits**: Configure memory and disk watermarks appropriately for your workload.

3. **Use persistent queues carefully**: Persistent messages consume more disk space.

4. **Implement queue depth limits**: Use max length policies to prevent unbounded queue growth.

5. **Alert on blocked events**: Create alerts for blocked connection warnings to respond quickly.

6. **Test blocking scenarios**: Regularly test how your application behaves when RabbitMQ blocks connections.