# AWS Scheduler

[AWS Scheduler](https://aws.amazon.com/blogs/compute/introducing-amazon-eventbridge-scheduler/) is a service on AWS that allows calls to any AWS API at a specific time. In V10, we have added support for AWS Scheduler in Brighter's scheduler functionality.

## Usage

AWS Scheduler supports two methods for scheduling messages:

1. Schedule the `FireAwsSchedule` message to be send/publish to a specific SNS/SQS. This requires configuring the `FireAwsSchedule` consumer.
2. Schedule a message directly to the target SNS/SQS. This occurs only when Brighter is scheduling a message.

For this, you will need the `Paramore.Brighter.MessageScheduler.Aws` package.

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
                new SqsSubscription<FireAwsScheduler>
                    new SubscriptionName("paramore.example.scheduler-message"),
                    new ChannelName("message-scheduler-channel"),
                    new RoutingKey("message-scheduler-topic"),
                    bufferSize: 10,
                    timeOut: TimeSpan.FromMilliseconds(20),
                    lockTimeout: 30
                )
            };
        })
        .UseScheduler(new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
        {
            // The external SNS/SQS used to fire scheduler messages
            SchedulerTopicOrQueue = new RoutingKey("message-scheduler-topic"),
            OnConflict = OnSchedulerConflict.Overwrite
        });
}
```

## Role

AWS Scheduler requires an AWS Role that allows assumption and has permissions for `sqs:SendMessage` and `sns:Publish`.

### Sample Role Definition

#### Trust Policy
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": {
                "Service": "scheduler.amazonaws.com"
            },
            "Action": "sts:AssumeRole"
        }
    ]
}
```

#### Permissions Policy
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "sqs:SendMessage",
                "sns:Publish"
            ],
            "Resource": ["*"]
        }
    ]
}
```

### Role Creation Option

```csharp
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    MakeRole = OnMissingRole.Create // Brighter will create the role if it doesn't exist.
};
```

## Configuration

### Group

AWS Scheduler has a group concept, which allows companies to organize schedulers better. It can be configured using the `Group` property.

```csharp
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    Group = new SchedulerGroup
    {
        Name = "some-group", // It is mandatory, and by default, Brighter will use the "default" group.
        Tags = new List<Tag>(), // It is optional, and by default, Brighter will set the "Source" tag as "Brighter".
        MakeSchedulerGroup = OnMissingSchedulerGroup.Create // If Brighter should create the Scheduler Group; by default, Brighter assumes it exists.
    }
};
```

### Custom Scheduler Name

By default, Brighter uses `Guid.NewGuid()` to define the scheduler name. This behavior can be customized as follows:

```csharp
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    GetOrCreateMessageSchedulerId = message => message.Id,
    GetOrCreateRequestSchedulerId = request =>
    {
        if (request is MyCommand command)
        {
            return command.SomeProperty;
        }

        return request.Id.ToString();
    }
};
```

### Scheduler Name Conflict

Depending on the scheduler name strategy, Brighter may need to handle conflicts. For resolving this issue, Brighter provides two approaches:

1. **Throw**: Brighter will throw an exception, so whoever is trying to create the scheduler will need to decide what to do.
2. **Overwrite**: Brighter will call `UpdateScheduler`.