# RabbitMQ Configuration

## General

RabbitMQ is OSS message-oriented-middleware and is [well documented](https://www.rabbitmq.com/documentation.html). Brighter handles the details of sending to or receiving from RabbitMQ. You may find it useful to understand the [building blocks](https://www.rabbitmq.com/tutorials/amqp-concepts.html) of the protocol. You might find the [documentation for the .NET SDK](https://www.rabbitmq.com/dotnet-api-guide.html) helpful when debugging, but you should not have to interact with it directly to use Brighter.

RabbitMQ offers an API that defines primitives used to configure the middleware used for messaging:

- **Exchange**: A routing table. Different types of exchanges route messages differently
- **Queue**: A store-and-forward queue over which a consumer receives messages. A message is locked whilst a consumer has read it, until they ack it, upon which it is deleted from the queue, or nack it, upon which it is requeued or sent to a DLQ.
- **Binding**: Adds a queue as a target for a routing rule on an exchange. The routing key is used for this on a direct exchange (on the default exchange the routing key is the queue name).

We connect to RabbitMQ via a multiplexed TCP/IP connection - RabbitMQ calls these channels. Brighter uses a push consumer, so it has an open channel and can be seen on the consumers list in the management console. Brighter maintains a pool of connections and when asked for a new connection will take one from it's pool in preference to creating a new one.

## Connection

The Connection to RabbitMQ is provided by an **RmqMessagingGatewayConnection** which allows you to configure the following:

* **Name**: A unique name for the connection, for diagnostic purposes
* **AmqpUri**: A connection to AMQP in the form of an [RabbitMQ Uri](https://www.rabbitmq.com/uri-spec.html) **Uri** with reliability options for a retry count (defaults to 3), **ConnectionRetryCount**, retry interval (defaults to 1000ms) **RetryWaitInMilliseconds** and a circuit breaker retry timeout (defaults to 60000ms), **CircuitBreakTimeInMilliseconds**, which introduces a delay when connections exceed the retry count.
* **Exchange**: The definition of the exchange. **Name** is the identifier for the exchange. All exchanges have a [**Type**](https://www.rabbitmq.com/tutorials/amqp-concepts.html), and the default is **ExchangeType.Direct**, but it is a string value that supports all RabbitMQ exchange types on the .NET SDK. The **Durable** flag is used to indicate if the exchange definition survives node failure or restart of the broker which defaults to *false*. **SupportDelay** indicates if the Exchange supports retry with delay, which defaults to *false*.
* **DeadLetterExchange**: Another exchange definition, but this one is used to host any Dead Letter Queues (DLQ). This could be the same exchange, but normal practice is to use a different exchange.
* **Heartbeat**: RabbitMQ uses a heartbeat to determine if a connection has died. This sets the interval for that heartbeat. Defaults to 20s.
* **PersistMessages**: Should messages be saved to disk? Saving messages to disk allows them to be recovered if a node fails, defaults to *false*.

In RabbitMQ, recreating an exiting primitive is a no-op provided the definition does not change.

The following code creates a typical RabbitMQ connection (here shown as part of configuring an External Bus):

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(new RmqProducerRegistryFactory(
                    new RmqMessagingGatewayConnection
                    {
                        Name = "MyCommandConnection",
                        AmpqUri = new AmqpUriSpecification(
                            new Uri("amqp://guest:guest@localhost:5672")
                            connectionRetryCount: 5,
                            retryWaitInMilliseconds: 250,
                            circuitBreakerTimeInMilliseconds = 30000
                        ),
                        Exchange = new Exchange("paramore.brighter.exchange", durable: true, supportDelay: true),
                        DeadLetterExchange = new Exchange("paramore.brighter.exchange.dlq", durable: true, supportDelay: false),
                        Heartbeat = 15,
                        PersistMessages = true
                    },
            ... //publication, see below
        ).Create()
}
```

## Publication

For more on a *Publication* see the material on an *External Bus* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

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
        .UseExternalBus({
            ...//connection information, see above
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
}
```


## Subscription

For more on a *Subscription* see the material on configuring *Service Activator* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-service-activator).

We support a number of RabbitMQ specific *Subscription* options:

* **DeadLetterChannelName**: The name of the queue to subscribe to DLQ notifications for this subscription (without a queue, the messages sent to the Dead Letter Exchange (DLX) will not be stored) 
* **DeadLetterRoutingKey**: The routing key that binds the DLQ to the DLX
* **HighAvailability**: [Deprecated] Not used on versions of RabbitMQ 3+. Prior to this, configuring that a queue should be mirrored was an API option, now it is a configuration management option on the broker.
* **IsDurable**: Should subscription definitions survive a restart of nodes in the broker.
* **MaxQueueLength**: [Deprecated] Prefer to use policy to set this instead (see [RabbitMQ docs](https://www.rabbitmq.com/maxlength.html)). The maximum length a RabbitMQ queue can grow to, before new messages are rejected (and sent to a DLQ if there is one).

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
            runAsync: false,
            timeoutInMilliseconds: 200,
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

    services.AddServiceActivator(options =>
        {
            options.Subscriptions = subscriptions;
            options.ChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
            ... //see Basic Configuration
        })
```

### Ack and Nack

We use RabbitMQ's queues to subscribe to a routing key on an exchange.

When we Accept/Ack a message, in response to a handler chain completing, we Ack the message to RabbitMQ using **Channel.BasicAck**. Note that we only Ack a message once we have completed running the chain. 

When we Reject/Nack a message (see [Handler Failure](/contents/HandlerFailure.md) for more on failure) then we use **Channel.Reject** to delete the message, and move it to a DLQ if there is one.

Brighter has an internal buffer for messages pushed to a *Performer* (a thread running a message pump). This buffer has thread affinity (in RabbitMQ we have to Ack or Nack from the thread that received the message). When a consumer closes its connection to RabbitMQ, messages in the buffer that have not been Ack'd or Nack'd will be returned to the queue.



