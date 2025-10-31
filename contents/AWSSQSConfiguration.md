# AWS SQS Configuration

## General

SNS and SQS are proprietary message-oriented-middleware available on the AWS platform. Both are well documented: see [SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html) and [SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html). Brighter handles the details of sending to SNS using an SQS queue for the consumer. You might find the [documentation for the AWS .NET SDK](https://docs.aws.amazon.com/sdk-for-net/) helpful when debugging, but you should not have to interact with it directly to use Brighter.

It is useful to understand the relationship between these two components:

- **SNS**: A routing table, [SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html) provides routing for messages to subscribers. Subscribers include, but are not limited to, SQS [see SNS Subscribe Protocol](https://docs.aws.amazon.com/sns/latest/api/API_Subscribe.html). An entry in the table is a **Topic**.
- **SQS**: A store-and-forward queue over which a consumer receives messages. A message is locked whilst a consumer has read it, until they ack it, upon which it is deleted from the queue, or nack it, upon which it is unlocked. A policy controls movement of messages that cannot be delivered to a DLQ. [SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html) may be used for point-to-point integration, and does not require SNS.

Brighter supports multiple AWS messaging patterns:

1. **SNS/SQS Pattern**: SNS is used as a routing table with SQS queues subscribing to topics (primary pattern)
2. **Direct SQS**: Direct publication to and consumption from SQS queues for point-to-point scenarios
3. **FIFO Support**: Both SNS FIFO topics and SQS FIFO queues are supported for ordered message delivery

Point-to-point scenarios can be modelled either as an SNS **topic** with one subscribing queue or as direct SQS queue communication.

## Connection

The Connection to AWS is provided by an **AWSMessagingGatewayConnection**. This is a wrapper around AWS credentials and region, that allows us to create the .NET clients that abstract various AWS HTTP APIs. We require the following parameters:

- **Credentials**: An instance of *AWSCredentials*. Storing and retrieving the credentials is a detail for your application and may vary by environment. There is AWS discussion of credentials resolution [here](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html)
- **Region**: The *RegionEndpoint* to use. SNS is a regional service, so we need to know which region to provision infrastructure in, or find it from.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials)
	{
        throw InvalidOperationException("Missing AWS Credentials");
    }

    services.AddBrighter(...)
        .AddProducers((configure) =>
        { 
            configure.ProducerRegistry = new SnsProducerRegistryFactory(
                    new AwsMessagingGatewayConnection(credentials, Environment.GetEnvironmentVariable("AWS_REGION"))
                    ,
                    ... //publication, see below
            ).Create();
        })
}
```
## Publication

For more on a *Publication* see the material on an *Add Producers* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

Brighter's **Routing Key** represents the [SNS Topic Name](https://docs.aws.amazon.com/sns/latest/api/API_CreateTopic.html) or [SQS Queue Name](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_CreateQueue.html).

### Finding and Creating Topics
Depending on the option you choose for how we handle required messaging infrastructure (Create, Validate, Assume), we will need to determine if a **Topic** already exists, when we want to create it if missing, or validate it. 

Naively using the AWS SDK's **FindTopic** method is an expensive operation. This enumerates all the **Topics** in that region, looking for those that have a matching name. Under-the-hood the client SDK pages through your topics. If you have a significant number of topics, this is expensive and subject to rate limiting. 

As creating a **Topic** is an *idempotent* operation in SNS, if asked to Create we do so without first searching to see if it already exists because of the cost of validation.

If you create your infrastructure out-of-band, and ask us validate it exists, to mitigate the cost of searching for topics, we provide several options under **FindTopicBy**.

- **FindTopicBy**: How do we find the topic: 
    - **TopicFindBy.Arn** -> On a *Publication*, the routing key is the **Topic** name, but you explicitly supply the ARN in another field: **TopicArn**. On a *Subscription* the routing key is the **Topic** ARN.
    - **TopicFindBy.Convention** -> The routing key is the **Topic** name, and we use convention to construct the ARN from it
    - **TopicFindBy.Name** -> The routing key is the **Topic** name & we use ListTopics to find it (rate limited 30/s)

#### TopicFindBy.Arn 
We use **GetTopicAttributesAsync** SDK method to request attributes of a Topic with the ARN supplied in **TopicArn**. If this call fails with a NotFoundException, we know that the Topic does not exist. This is a *hack*, but is much more efficient than enumeration as a way of determining if the ARN exists.

#### TopicFindBy.Convention 
If you supply only the **Topic** name via the routing key, we construct the ARN by convention as follows:

``` csharp
var arn = new Arn
    {
        Partition = //derived from the partition of the region you supplied to us,
        Service = "sns",
        Region = //derived from the system name of the region you supplied to us,
        AccountId = //your account id - derived from the credentials you supplied,
        Resource = topicName
    }
```

These assumptions work, if the topic is created by the account your credentials belong to. If not, you can't use by convention.

Once we obtain an ARN by convention, we can then use the optimized approach described under [TopicFindBy.Arn](#topicfindbyarn) to confirm that your topic exists.

#### TopicFindBy.Name
If you supply a name, but we can't construct the ARN via the above conventions, we have to fall back to the **SDKs** **FindTopic** approach. 

Because creation is idempotent, and **FindTopic** is expensive, you are almost always better off choosing to create over validating a topic by name. 

If you are creating the topics out-of-band, by CloudFormation for example, and so do not want Brighter the risk that Brighter will create them, then you will have an ARN. In that case you should use [TopicFindBy.Arn](#topicfindbyarn) or assume that any required infrastructure exists. 

### SNS Attributes

This property allows you to pass an instance of `SnsAttributes` which contains properties representing the attributes used when creating an SNS Topic. These are only used if you are creating a topic.

*   **DeliveryPolicy**: The policy that defines how Amazon SNS retries failed deliveries to HTTP/S endpoints.
*   **Policy**: The JSON serialization of the topic's access control policy.
*   **Tags**: A list of resource tags to apply to the topic.
*   **Type**: The type of SNS topic, either `Standard` or `Fifo`.
*   **ContentBasedDeduplication**: For FIFO topics, enables content-based deduplication.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials))
        throw new InvalidOperationException("Missing AWS Credentials");

    var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1");

    services.AddBrighter()
        .AddProducers((configure) =>
        { 
            configure.ProducerRegistry = new SnsProducerRegistryFactory(
                new AwsMessagingGatewayConnection(credentials, region),
                new SnsPublication[]
                {
                    new SnsPublication
                    {
                        Topic = new RoutingKey("my-fifo-topic.fifo"),
                        FindTopicBy = TopicFindBy.Convention,
                        MakeChannels = OnMissingChannel.Create,
                        SnsAttributes = new SnsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true)
                    }
                }
            ).Create();
        });
}
```

### Finding and Creating Queues

Similar to Topics, finding or creating SQS queues depends on your infrastructure management approach (Create, Validate, or Assume). When Brighter needs to interact with SQS queues, it provides efficient options to locate them.

Depending on the option you choose under **QueueFindBy**, Brighter will use different strategies to locate your queue:

#### QueueFindBy.Name

When you provide the queue name via the routing key, Brighter uses the AWS SDK's `GetQueueUrlAsync` method to find the queue URL. This is more efficient than listing all queues as it's a direct lookup by name. If the queue doesn't exist and you've configured `OnMissingChannel.Create`, Brighter will create it.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Get AWS credentials
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials))
    {
        throw new InvalidOperationException("Missing AWS Credentials");
    }
    
    var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1");
    
    services.AddBrighter(options => {
        // Configure other Brighter options
    })
    .AddProducers(options =>
    { 
        options.ProducerRegistry = new SqsProducerRegistryFactory(
            new AwsMessagingGatewayConnection(credentials, region),
            new SqsPublication[]
            {
                new SqsPublication
                {
                    // The queue name is used as the routing key
                    Topic = new RoutingKey("my-greeting-queue"),
                    // Explicitly set to find by name (this is the default)
                    FindQueueBy = QueueFindBy.Name,
                    // Whether to create the queue if it doesn't exist
                    MakeChannels = OnMissingChannel.Create
                }
            }
        ).Create();
    });
}
```

#### QueueFindBy.Url

You directly provide the complete queue URL rather than just the name. This is the most efficient option as it requires no additional API calls to locate the queue. When using `QueueFindBy.Url`, the queue URL is provided via the `ChannelName`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Get AWS credentials
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials))
    {
        throw new InvalidOperationException("Missing AWS Credentials");
    }
    
    var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1");
    
    services.AddBrighter(options => {
        // Configure other Brighter options
    })
    .AddProducers(options =>
    { 
        options.ProducerRegistry = new SqsProducerRegistryFactory(
            new AwsMessagingGatewayConnection(credentials, region),
            new SqsPublication[]
            {
                new SqsPublication
                {
                    // The routing key is used for producer registry lookup
                    Topic = new RoutingKey("greeting-events"),
                    
                    // Set to find by URL
                    FindQueueBy = QueueFindBy.Url,
                    
                    // The queue URL is provided via ChannelName
                    ChannelName = new ChannelName("https://sqs.us-east-1.amazonaws.com/123456789012/my-greeting-queue"),
                    
                    // With QueueFindBy.Url, use Assume as we're providing a direct reference
                    MakeChannels = OnMissingChannel.Assume
                }
            }
        ).Create();
    });
}
```

When using this configuration, Brighter will use the URL provided in the `ChannelName` directly as the queue URL without any additional lookups or API calls.

### SQS attributes

This property allows you to pass an instance of `SqsAttributes` which contains properties representing the attributes used when creating an SQS Queue. These are only used if you are creating a queue.

*   **DelaySeconds**: The length of time for which the delivery of all messages in the queue is delayed. The default is 0 seconds, with a maximum of 15 minutes.
*   **MessageRetentionPeriod**: The length of time for which Amazon SQS retains a message. The default is 4 days, with a range from 60 seconds to 14 days.
*   **ContentBasedDeduplication**: For FIFO queues, enables or disables content-based deduplication.
*   **DeduplicationScope**: For high-throughput FIFO queues, specifies whether message deduplication occurs at the message group or queue level.
*   **FifoThroughputLimit**: For high-throughput FIFO queues, specifies whether the throughput quota applies to the entire queue or per message group.
*   **IamPolicy**: The queue's access control policy as a JSON string.
*   **LockTimeout**: How long a 'lock' is held on a message for a consumer to process it. This is the SQS Visibility Timeout. The default is 30 seconds, with a range from 0 seconds to 12 hours.
*   **RawMessageDelivery**: When subscribed to an SNS topic, this indicates whether to enable raw message delivery.
*   **RedrivePolicy**: The policy for moving messages to a Dead-Letter Queue (DLQ) after multiple failed processing attempts.
*   **Type**: The type of SQS queue, either `Standard` or `Fifo`.
*   **Tags**: A dictionary of key-value pairs to apply as tags to the queue.
*   **TimeOut**: The long-polling duration for receiving messages. This is the `ReceiveMessageWaitTimeSeconds`. The default is 0 seconds (short polling), with a maximum of 20 seconds.

``` csharp
var sqsPublication = new SqsPublication
{
    Topic = new RoutingKey("my-fifo-queue.fifo"),
    FindQueueBy = QueueFindBy.Name,
    MakeChannels = OnMissingChannel.Create,
    SqsAttributes = new SqsAttributes( 
        lockTimeout: TimeSpan.FromMinutes(2),
        messageRetentionPeriod: TimeSpan.FromDays(1),
        tags: new Dictionary<string, string>
        {
            { "environment", "production" },
            { "project", "brighter-example" }
        })
};
```

## Subscription

As normal with Brighter, we allow **Topic** creation from the Subscription. Because this works in the same way as the Publication see the notes under [Publication](#publication) for further detail on the options that you can configure around creation or validation.

A subscription in Brighter represents a consumer of messages. For AWS, this can be a consumer of an SQS queue that is subscribed to an SNS topic, or a consumer of an SQS queue directly for point-to-point messaging. Both standard and FIFO queues/topics are supported.

When subscribing to an SNS topic, Brighter handles the creation of the SQS queue and the subscription to the topic. For direct SQS communication, Brighter will consume from the specified queue. Much of the Subscription configuration is focused on defining the parameters of the SQS queue that will be created or used.

* **ChannelType**: Specifies whether the subscription is for a Pub/Sub (ChannelType.PubSub) or Point-to-Point (ChannelType.PointToPoint) scenario.
* **FindTopicBy**: For Pub/Sub channels, determines how to find the SNS topic using the RoutingKey. Options are Arn, Convention, or Name. See Finding and Creating Topics for more details.
* **FindQueueBy**: Determines how to find the SQS queue using the ChannelName. Options are Url or Name. See Finding and Creating Queues for more details.
* **QueueAttributes**: An instance of SqsAttributes that defines the properties of the SQS queue to be created. See SQS attributes for a full list of available settings.
* **TopicAttributes**: For Pub/Sub channels, an instance of SnsAttributes that defines the properties of the SNS topic to be created. See SNS Attributes for more details.

#### SQS Pub/Sub

This sample demonstrates a standard Pub/Sub scenario where a consumer subscribes to an SNS topic via an SQS queue.

```csharp
var subscriptions = new Subscription[]
{
    new SqsSubscription<GreetingEvent>(
        name: new SubscriptionName("greeting.subscriber"),
        channelName: new ChannelName("greeting.queue"),
        routingKey: new RoutingKey("greeting.topic"),
        channelType: ChannelType.PubSub,
        makeChannels: OnMissingChannel.Create
    )
};
```

#### SQS FIFO Pub/Sub

For ordered message processing, you can use FIFO topics and queues. Note that both the topic and queue names must end with `.fifo`, and the `Type` in both `SnsAttributes` and `SqsAttributes` must be set to `Fifo`.

```csharp
var subscriptions = new Subscription[]
{
    new SqsSubscription<GreetingEvent>(
        name: new SubscriptionName("greeting.subscriber.fifo"),
        channelName: new ChannelName("greeting.queue.fifo"),
        routingKey: new RoutingKey("greeting.topic.fifo"),
        channelType: ChannelType.PubSub,
        makeChannels: OnMissingChannel.Create,
        topicAttributes: new SnsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true),
        queueAttributes: new SqsAttributes(type: SqsType.Fifo)
    )
};
```

#### SQS Point-to-Point

This sample shows a direct Point-to-Point communication using a standard SQS queue.

```csharp
var subscriptions = new Subscription[]
{
    new SqsSubscription<GreetingEvent>(
        name: new SubscriptionName("greeting.consumer"),
        channelName: new ChannelName("greeting.queue"),
        routingKey: new RoutingKey("greeting.queue"),
        channelType: ChannelType.PointToPoint,
        makeChannels: OnMissingChannel.Create
    )
};
```

#### SQS FIFO Point-to-Point

For ordered Point-to-Point messaging, you can use a FIFO queue. The queue name must end with `.fifo`, and the `Type` in `SqsAttributes` must be `Fifo`.

```csharp
var subscriptions = new Subscription[]
{
    new SqsSubscription<GreetingEvent>(
        name: new SubscriptionName("greeting.consumer.fifo"),
        channelName: new ChannelName("greeting.queue.fifo"),
        routingKey: new RoutingKey("greeting.queue.fifo"),
        channelType: ChannelType.PointToPoint,
        makeChannels: OnMissingChannel.Create,
        queueAttributes: new SqsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true)
    )
};
```

### Ack and Nack

As elsewhere, Brighter only Acks after your handler has run to process the message. We will Ack unless you throw a **DeferMessageAction**. See [Handler Failure](/contents/HandlerFailure.md) for more.

An Ack will delete the message from the SQS queue using the SDK's **DeleteMessageAsync**.

In response to a DeferMessageAction we will requeue, using the SDK's **ChangeMessageVisibilityAsync** to make the message available again to other consumers.

On a Nack, we will move the message to a DLQ, if there is one. We Nack when we exceed the requeue count for a message, or we raise a ConfigurationException.

## AWS SDK v4 Support

Brighter provides support for both version 3 and version 4 of the AWS SDK for .NET through separate NuGet packages. This approach is crucial for managing dependencies and avoiding conflicts within your applications.

*   **Paramore.Brighter.MessagingGateway.AWSSQS**: This package depends on version 3.x of the AWS SDK (`AWSSDK.SQS` and `AWSSDK.SimpleNotificationService`).
*   **Paramore.Brighter.MessagingGateway.AWSSQS.V4**: This package depends on version 4.x of the AWS SDK. You can find it on NuGet [here](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.AWSSQS.V4).

Major versions of the AWS SDK often introduce breaking changes. By offering two distinct packages, Brighter ensures that you can choose the one that aligns with the AWS SDK version used in your project. This prevents "dependency hell" and allows for a smoother migration path if you decide to upgrade from AWS SDK v3 to v4, without being forced to upgrade all your Brighter-related packages at once.
