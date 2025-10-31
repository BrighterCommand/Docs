# Azure Service Bus Scheduler

[Azure Service Bus timestamps](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing) allows scheduling messages for future delivery. In V10, we added support for Azure Service Bus Scheduler to Brighter's scheduling functionality.

## Usage

To use Azure Service Bus timestamp with Brighter:
1. Create a subscription for the `FireAzureSchedule` consumer.
2. Install the `Paramore.Brighter.MessageScheduler.Azure` package.

### Example Configuration

```csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        });

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services
        .AddServiceActivator(options =>
        {
            options.Subscriptions = new[]
            {
                // Subscription for FireAzureSchedule
                new AzureServiceBusSubscription<FireAzureSchedule>(
                    new SubscriptionName("paramore.example.scheduler-message"),
                    new ChannelName("message-scheduler-channel"),
                    new RoutingKey("message-scheduler-topic"),
                    bufferSize: 10,
                    timeOut: TimeSpan.FromMilliseconds(20),
                    lockTimeout: 30 // Fixed typo from 'lockTime out' to 'lockTimeout'
                )
            };
        })
        .UseScheduler(new AzureServiceBusSchedulerFactory(
            new ServiceBusVisualStudioCredentialClientProvider(), 
            "some-role"
        ));
}
```

## Important Considerations

### Rescheduling Limitations

Azure Service Bus **does not support modifying** the `EnqueuedTimeUtc` property of scheduled messages. To reschedule a message:
1. Cancel the existing scheduled message
2. Create a new scheduled message with updated timing

### Key Components

1. **Service Bus Credentials**  
   Uses `ServiceBusVisualStudioCredentialClientProvider` for authentication (requires proper Azure AD configuration).

2. **Message Handling**  
   Ensure your `FireAzureSchedule` consumer is properly configured to handle scheduled messages.