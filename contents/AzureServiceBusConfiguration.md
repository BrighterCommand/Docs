# Azure Service Bus Configuration

## General
Azure Service Bus (ASB) is a fully managed enterprise message broker and is [well documented](https://docs.microsoft.com/en-us/azure/service-bus-messaging/) Brighter handles the details of sending to or receiving from ASB.  You may find it useful to understand the [concepts](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions) of the ASB.

## Connection
The connection to ASB id defined by an **IServiceBusClientProvider**, Brighter proviedes the following Implimentations

* **ServiceBusChainedClientProvider**: A client provider that allows you to specific a chain of **TokenCredentials** to authenticate with.

* **ServiceBusConnectionStringClientProvider**: A client provider that accepts a connection string (containg Authentication information)

* **ServiceBusDefaultAzureClientProvider**: A client provider that uses the Default Azure Credential to authenticate.

* **ServiceBusManagedIdentityClientProvider**: A client provider that uses Azure Managed Identity to authenticate.

* **ServiceBusVisualStudioCredentialClientProvider**: A client provider that uses Visual Studio Credential to authenticate.

In Brighter's implementation of the Messaging Gateway *Publications* and *Subscriptions* have their own Individual configuration.

## Publication

No custom properties are supported for ASB

Basic Brighter configutarion publications is as follows

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(
        new AzureServiceBusProducerRegistryFactory(
                asbConnection,
                new AzureServiceBusPublication[]
                {
                    new() { Topic = new RoutingKey("greeting.event") },
                    new() { Topic = new RoutingKey("greeting.addGreetingCommand") },
                    new() { Topic = new RoutingKey("greeting.Asyncevent") }
                }
            )
            .Create()
    )
}
```

For more on a *Publication* see the material on an *External Bus* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

## Subscription

For more on a *Subscription* see the material on configuring *Service Activator* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-service-activator).

When 

We support a number of ASB specific *Subscription* options:

* **MaxDeliveryCount**: The Maximum amount of times that a Message can be delivered before it is dead Lettered. This differs from **requeue count** as this is used by the transport in the event of lock expiry (in the event of process failure or processing taking too long) **default:** 5

* **DeadLetteringOnMessageExpiration**: Dead letter a message when it expires **default:** true

* **LockDuration**: How long message locks are held for **default:** true

* **DefaultMessageTimeToLive**: How long messages sit in the queue before they expire **default:** 1 minute

* **SqlFilter**: A Sql Filter to apply to the *subscription* see [Topic Filters](https://docs.microsoft.com/en-us/azure/service-bus-messaging/topic-filters) **default:** none


This is a typical *Subscription* configuration in a Consumer application:

``` csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new AzureServiceBusSubscription<GreetingAsyncEvent>(
            new SubscriptionName(GreetingEventAsyncMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(GreetingEventAsyncMessageMapper.Topic),
            timeoutInMilliseconds: 400,
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            isAsync: true,
            noOfPerformers: 2, unacceptableMessageLimit: 1),
        new AzureServiceBusSubscription<GreetingEvent>(
            new SubscriptionName(GreetingEventMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(GreetingEventMessageMapper.Topic),
            timeoutInMilliseconds: 400,
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            isAsync: false,
            noOfPerformers: 2),
        new AzureServiceBusSubscription<AddGreetingCommand>(
            new SubscriptionName(AddGreetingMessageMapper.Topic),
            new ChannelName(subscriptionName),
            new RoutingKey(AddGreetingMessageMapper.Topic),
            timeoutInMilliseconds: 400,
            makeChannels: OnMissingChannel.Create,
            requeueCount: 3,
            isAsync: true,
            noOfPerformers: 2)
    };

    var clientProvider = new ServiceBusVisualStudioCredentialClientProvider("my-awesome-asb.servicebus.windows.net");

    var asbConsumerFactory = new AzureServiceBusConsumerFactory(clientProvider);

    builder.Services.AddServiceActivator(options =>
    {
        options.Subscriptions = subscriptions;
        options.ChannelFactory = new AzureServiceBusChannelFactory(asbConsumerFactory);
        
    }
```

## Complete Reject

We use ASB's *Subscription* to surscribe to a Topic on a namespace.

When we Complete a message, in response to a handler chain completing, we Complete the message on ASB using **messageReceiver.CompleteMessageAsync**. Note that we only Complete a message once we have completed running the chain and only if AckOnRead is set to false (as the messages is removed from the queue otherwise).

When we Dead Letter a message (see [Handler Failure](/contents/HandlerFailure.md) for more on failure) then we use **messageReceiver.DeadLetterMessageAsync** to delete the message, and move it to a DLQ.