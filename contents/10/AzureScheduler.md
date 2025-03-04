# AWS Scheduler

[Azure Service Bus scheduler message](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing) allows Brighter to schedule a message. On V10, we have added support to Azure Service Buse (scheduler message) for [Brighter's scheduler support](/contents/BrighterScheduleroSupport.md).

## Usage

Using the Azure Service scheduler message is necessary to create a subscription for `FireAzureSchedule`.

For this we will need the *Paramore.Brighter.MessageScheduler.Azure* packages.

* **Paramore.Brighter.MessageScheduler.Azure**

```csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services
        .AddServiceActivator(options => 
        {
            options.Subscription = new[]{
                ...
                  new AzureServiceBusSubscription<FireAwsScheduler>(
                        new SubscriptionName("paramore.example.scheduler-message"),
                        new ChannelName("message-scheduler-channel"),
                        new RoutingKey("message-scheduler-topic"),
                        bufferSize: 10,
                        timeOut: TimeSpan.FromMilliseconds(20),
                        lockTimeout: 30),
            };
             ...  
        })
        .UseScheduler(new AwsSchedulerFactoryAzureServiceBusSchedulerFactory(new ServiceBusVisualStudioCredentialClientProvider(),  "some-role"));
}
...
```

## Rescheduler

Unfortunately, the Azure Service Bus doesn't support changing the `EnqueuedTimeUtc`. If you need to reschedule a message, you must click `Cancel` and `Scheduler` again.