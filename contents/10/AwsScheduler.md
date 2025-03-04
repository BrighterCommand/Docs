# AWS Scheduler

[AWS Scheduler](https://aws.amazon.com/blogs/compute/introducing-amazon-eventbridge-scheduler/) is a service on AWS that allows calls of any AWS API, on V10 we have added support to AWS Scheduler for [Brighter's scheduler support](/contents/BrighterScheduleroSupport.md).

## Usage

AWS Scheduler has 2 ways of scheduler a message:

- Scheduler the `FireAwsSchedule` to a specific SNS/SQS, making it necessary to configure the `FireAwsSchedule` consumer.

- Scheduler a message to target SNS/SQS directly, this happens only when Brighter is scheduling a message

For this, we will need the *Paramore.Brighter.MessageScheduler.Aws* packages.

* **Paramore.Brighter.MessageScheduler.Aws**

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
                  new SqsSubscription<FireAwsScheduler>(
                        new SubscriptionName("paramore.example.scheduler-message"),
                        new ChannelName("message-scheduler-channel"),
                        new RoutingKey("message-scheduler-topic"),
                        bufferSize: 10,
                        timeOut: TimeSpan.FromMilliseconds(20),
                        lockTimeout: 30),
 };
 ...  
 })
 .UseScheduler(new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
 {
 // The external SNS/SQS used to fire scheduler messages
            SchedulerTopicOrQueue = new RoutingKey("message-scheduler-topic"),
            OnConflict = OnSchedulerConflict.Overwrite
 });
}
...
```

## Role

AWS Scheduler requires an AWS Role that allows assume and have permission to `sqs:SendMessage` and `sns:Publish`.

A sample of the role definition:

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
```

The policy
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
 }]
}
```


```c#
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    MakeRole = OnMissingRole.Create // Brighter will create the role if it doesn't exist. 
}
```

## Configuration
### Group
AWS Scheduler has a group concept, which allows companies to organize the scheduler better.

It can be configurated by the property `Group`,

```c#
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    Group = new SchedulerGroup
 {
        Name = "some-group" // It's mandatory and by default, Brighter is going to use the "default" group
        Tags = new List<Tag>(), // It's optional and by default, Brighter will set the "Source" tag as "Brighter",
        MakeSchedulerGroup = OnMissingSchedulerGroup.Create // If Brighter should or not create the Scheduler Group, by default Brighter will assume that exists.
 }
}
```

### Custom Scheduler Name
By default Brighter uses a `Guid.NewGuid()` to define the scheduler name, it can be customized by

```c#
_ = new AwsSchedulerFactory(new AWSMessagingGatewayConnection(), "some-role")
{
    GetOrCreateMessageSchedulerId = message => message.Id;
    GetOrCreateRequestSchedulerId => request => 
 {
        if(request is MyCommand command)
 {
            return command.SomeProperty;
 }

        return request.Id.ToString();
 };
}
```

### Scheduler name conflict
Depending on the scheduler name strategy Brighter will need to handle conflict, for solving this problem Brighter has 2 ways:

- Throw - Brighter will throw an exception, so whoever is trying to create the scheduler will need to decide what to do.
- Overwrite - Brighter will call `UpdateScheduler`