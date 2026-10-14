// Task 3.5, AzureScheduler.md. Claims:
//   block 21  DeadLetteringOnMessageExpiration = true and MaxDeliveryCount = 5 are the defaults
//   block 25  LockDuration set on the configuration is the subscription's lock duration
// Case: the subscription built as the block builds it. Control: the same subscription with no
// configuration passed, whose values must be the defaults; for block 21 they must equal the case.
using System;
using Paramore.Brighter;
using Paramore.Brighter.MessageScheduler.Azure;
using Paramore.Brighter.MessagingGateway.AzureServiceBus;

var failures = 0;
void Check(string what, bool ok) { Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {what}"); if (!ok) failures++; }

// ---- block 21, verbatim ----
// Ensure dead-letter queue is monitored
var subscription = new AzureServiceBusSubscription<FireAzureScheduler>(
    new SubscriptionName("scheduler-subscription"),
    new ChannelName("scheduler-channel"),
    new RoutingKey("brighter-scheduler-topic"),
    subscriptionConfiguration: new AzureServiceBusSubscriptionConfiguration
    {
        DeadLetteringOnMessageExpiration = true,  // Expired messages go to the DLQ (the default)
        MaxDeliveryCount = 5                      // Dead-lettered after 5 failed deliveries (the default)
    }
);
// ---- end ----
var plain = new AzureServiceBusSubscription<FireAzureScheduler>(
    new SubscriptionName("scheduler-subscription"), new ChannelName("scheduler-channel"), new RoutingKey("brighter-scheduler-topic"));
Check($"case    block 21: DLQ on expiry {subscription.Configuration.DeadLetteringOnMessageExpiration}, max deliveries {subscription.Configuration.MaxDeliveryCount}",
    subscription.Configuration.DeadLetteringOnMessageExpiration && subscription.Configuration.MaxDeliveryCount == 5);
Check($"control block 21, no configuration: DLQ on expiry {plain.Configuration.DeadLetteringOnMessageExpiration}, max deliveries {plain.Configuration.MaxDeliveryCount} (\"the default\")",
    plain.Configuration.DeadLetteringOnMessageExpiration == subscription.Configuration.DeadLetteringOnMessageExpiration
    && plain.Configuration.MaxDeliveryCount == subscription.Configuration.MaxDeliveryCount);

{
// ---- block 25's subscription, verbatim but for the `// ...` the page elides ----
var lockSubscription = new AzureServiceBusSubscription<FireAzureScheduler>(
    // ...
    subscriptionConfiguration: new AzureServiceBusSubscriptionConfiguration
    {
        LockDuration = TimeSpan.FromMinutes(2)  // Increase if processing takes longer
    }
);
// ---- end ----
var lockDefault = new AzureServiceBusSubscription<FireAzureScheduler>();
Check($"case    block 25: LockDuration {lockSubscription.Configuration.LockDuration}", lockSubscription.Configuration.LockDuration == TimeSpan.FromMinutes(2));
Check($"control block 25, no configuration: LockDuration {lockDefault.Configuration.LockDuration}, which 2 minutes increases", lockDefault.Configuration.LockDuration < lockSubscription.Configuration.LockDuration);
}
return failures == 0 ? 0 : 1;
