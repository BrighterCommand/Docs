# AWS SQS Configuration

## General

SNS and SQS are proprietary message-oriented-middleware available on the AWS platform. Both are well documented: see [SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html) and [SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html). Brighter handles the details of sending to SNS using an SQS queue for the consumer. You might find the [documentation for the AWS .NET SDK](https://docs.aws.amazon.com/sdk-for-net/) helpful when debugging, but you should not have to interact with it directly to use Brighter.

It is useful to understand the relationship between these two components:

- **SNS**: A routing table, [SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html) provides routing for messages to subscribers. Subscribers include, but are not limited to, SQS [see SNS Subscribe Protocol](https://docs.aws.amazon.com/sns/latest/api/API_Subscribe.html). An entry in the table is a **Topic**.
- **SQS**: A store-and-forward queue over which a consumer receives messages. A message is locked whilst a consumer has read it, until they ack it, upon which it is deleted from the queue, or nack it, upon which it is unlocked. A policy controls movement of messages that cannot be delivered to a DLQ. [SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html) may be used for point-to-point integration, and does not require SNS.

Brighter only supports the scenario where SNS is used as a routing table, and an SQS subscribes to a **topic**. It does not support stand-alone SQS queues. Point-to-point scenarios can be modelled as an SNS **topic** with one subscribing queue.

## Connection

The Connection to AWS is provided by an **AWSMessagingGatewayConnection**. This is a wrapper around AWS credentials and region, that allows us to create the .NET clients that abstract various AWS HTTP APIs. We require the following parameters:

- **Credentials**: An instance of *AWSCredentials*. Storing and retrieving the credentials is a detail for your application and may vary by environment. There is AWS discussion of credentials resolution [here](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html)
- **Region**: The *RegionEndpoint* to use. SNS is a regional service, so we need to know which region to provision infrastructure in, or find it from.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials)
	    throw InvalidOperationException("Missing AWS Credentials);

    services.AddBrighter(...)
        .UseExternalBus((configure) =>
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

For more on a *Publication* see the material on an *External Bus* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

Brighter's **Routing Key** represents the [SNS Topic Name](https://docs.aws.amazon.com/sns/latest/api/API_CreateTopic.html).

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

### Other Attributes
- **SNSAttributes**: This property lets you pass through an instance of **SNSAttributes** which has properties representing the attributes used when creating a **Topic**. These are only used if you are creating a **Topic**.
    - **DeliveryPolicy**: The policy that defines how Amazon SNS retries failed deliveries to HTTP/S endpoints.
    - **Policy**: The policy that defines who can access your topic. By default, only the topic owner can publish or subscribe to the topic.
    - **Tags**: A list of resource tags to use.

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    if (!new CredentialProfileStoreChain().TryGetAWSCredentials("default", out var credentials)
	    throw InvalidOperationException("Missing AWS Credentials);

    services.AddBrighter(...)
        .UseExternalBus((configure) =>
        { 
            configure.ProducerRegistry = new SnsProducerRegistryFactory(
                ...,//connection, see above
                new SnsPublication[]
                {
                    new SnsPublication()
                    {
                        Topic = new RoutingKey("GreetingEvent"),
                        FindTopicBy = TopicFindBy.Convention
                    }
                }
            ).Create();
        })
}
```

## Subscription

As normal with Brighter, we allow **Topic** creation from the *Subscription*. Because this works in the same way as the *Publication* see the notes under [Publication](#publication) for further detail on the options that you can configure around creation or validation.

In SNS you need to [subscribe](https://docs.aws.amazon.com/sns/latest/api/API_Subscribe.html) to a **Topic** to receive messages from that **Topic**. Brighter subscribes using an SQS queue (there are other options for SNS, but Brighter does not use those). Much of the *Subscription* configuration allows you to control the parameters of that *Subscription*.

We support the following properties on an *SQS Subscription* most of which relate to the creation of the [SQS Queue](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_CreateQueue.html) with which we subscribe:

- **LockTimeout**: How long, in seconds, a 'lock' is held on a message for one consumer before it times out(*VisibilityTimeout*). Default is 10s.
- **DelaySeconds**:  The length of time, in seconds, for which the delivery of all messages in the queue is delayed. Default is 0.
- **MessageRetentionPeriod**: The length of time, in seconds, for which Amazon SQS retains a message on a queue before deleting it. Default is 4 days.
- **IAMPolicy**: The queue's policy. A valid [AWS policy](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies.html).
- **RawMessageDelivery**: Indicate that the Raw Message Delivery setting is enabled or disabled. Defaults to true.
- **RedrivePolicy**: The parameters for the dead-letter queue functionality of the source queue. An instance of the **RedrivePolicy** class, which has the following parameters:
    - **MaxReceiveCount**: The maximum number of requeues for a message before we push it to the DLQ instead
    - **DeadlLetterQueueName**: The name of the dead letter queue we want to associate with any redrive policy.


``` csharp
private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    (AWSCredentials credentials, RegionEndpoint region) = CredentialsChain.GetAwsCredentials();
    var awsConnection = new AWSMessagingGatewayConnection(credentials, region);

     var subscriptions = new Subscription[]
    {
        new SqsSubscription<GreetingEvent>(
            name: new SubscriptionName("Subscription-Name),
            channelName: new ChannelName("Channel-Name"),
            routingKey: new RoutingKey("arn:aws:sns:us-east-2:444455556666:MyTopic"),
            findTopicBy: TopicFindBy.Arn,
            makeChannels: OnMissingChannel.Validate
        );
    }

     var sqsMessageConsumerFactory = new SqsMessageConsumerFactory(awsConnection);

    services.AddServiceActivator(options =>
        {
            options.Subscriptions = subscriptions;
            options.ChannelFactory = new ChannelFactory(sqsMessageConsumerFactory);
            ... //see Basic Configuration
        })
```

### Ack and Nack

As elsewhere, Brighter only Acks after your handler has run to process the message. We will Ack unless you throw a **DeferMessageAction**. See [Handler Failure](/contents/HandlerFailure.md) for more.

An Ack will delete the message from the SQS queue using the SDK's **DeleteMessageAsync**.

In response to a DeferMessageAction we will requeue, using the SDK's **ChangeMessageVisibilityAsync** to make the message available again to other consumers.

On a Nack, we will move the message to a DLQ, if there is one. We Nack when we exceed the requeue count for a message, or we raise a ConfigurationException.





