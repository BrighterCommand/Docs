# Using a Distributed Task Queue

Brighter provides support for a [distributed task
queue](https://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf).
Instead of handling a command or event, synchronously and in-process,
work can be dispatched to a distributed queue to be handled
asynchronously and out-of-process. The trade-off here is between the
cost of distribution (see [The Fallacies of Distributed
Computing](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing))
against performance.

For example you might have an HTTP API with a rule that any given request to
that API must execute in under 100ms. On measuring the performance of a
key POST or PUT operation to your API you find that you exceed this
value. Upon realizing that much of your time is spent I/O you consider
two options:

-   Use the TPL (Task parallel library) to perform the work concurrently
-   Offload the work to a distributed task queue, ack the message, and
    allow the work to complete asynchronously

Either way you probably return a 202 Accepted to the caller, with a Link
header that points to an endpoint where the caller can poll for
completion and/or monitor progress. This might be a resource you are
creating that will return a 404 until it exists, or a progress indicator
that indicates how far through the work you are and redirects to the
resource once it is complete. (You can store progress in a backing
store, perhaps using a distributed cache such as Redis).

There is a problem with the TPL approach is that your operation can only
meet the 100ms threshold if your work can be parallelised such that no
sub-task takes longer than 100ms. Your speed is always constrained by
the slowest operation that you need to parallelize. If you are I/O bound
on a resource experiencing contention beyond 100ms, you will not meet
your goal by introducing more threads. Your minimum time is your minimum
time.

You might try to fix this by acking (acknowledging) the request, and
completing the work asynchronously. This option is particularly
attractive if the work is I/O bound as you can process other requests
whilst you wait for the I/O to complete.

The downside of the async approach is that you risk that the work will
be lost if the server fails prior to completion of the work, or the app
simply recycles.

These requirements tend to push you in the direction of [Guaranteed
Delivery](http://www.eaipatterns.com/GuaranteedMessaging.html) to ensure
that work you ack will eventually be handled.

A distributed task queue allows you offload work to another process, to
be handled asynchronously (once you push the work onto the queue, you
don\'t wait) and in parallel (you can use other cores to process the
work). It also allows you to ensure delivery of the message, eventually
(the queue will hold the work until a consumer is available to read it).

In addition use of a distributed task queue allows you to throttle
requests - you can hand work off from the web server to a queue that
only needs to consume at the rate you have resources to support. This
allows you to scale to meet unexpected demand, at the price of [eventual
consistency.](https://en.wikipedia.org/wiki/Eventual_consistency)

# Brighter\'s Task Queue Architecture

Brighter implements Task Queues using a [Message
Broker](http://www.enterpriseintegrationpatterns.com/MessageBroker.html).

The producer sends a **Command** or **Event** to a [Message
Broker](http://www.enterpriseintegrationpatterns.com/MessageBroker.html)
using **CommandProcessor.Post()**.

We use an **IAmAMessageMapper** to map the **Command** or **Event** to a
**Message**. (Usually we just serialize the object to JSON and add to
the **MessageBody**), but if you want to use higher performance
serialization approaches, such as
[protobuf-net](https://github.com/mgravell/protobuf-net), the message
mapper is agnostic to the way the body is formatted.)

When we deserialize we set the **MessageHeader** which includes a topic
(often we use a namespaced name for the **Command** or **Event**).

We store the created **Message** in a [Message
Store](http://www.enterpriseintegrationpatterns.com/MessageStore.html)
for use by **CommandProcessor.Repost()** if we need to resend a failed
message.

The Message Broker manages a [Recipient
List](http://www.enterpriseintegrationpatterns.com/RecipientList.html)
of subscribers to a topic. When it receives a **Message** the Broker
looks at the topic in the **MessageHeader** and dispatches the
**Message** to the [Recipient
Channels](http://www.enterpriseintegrationpatterns.com/MessageChannel.html)
identified by the Recipient List.

The consumer registers a [Recipient
Channel](http://www.enterpriseintegrationpatterns.com/MessageChannel.html)
to receive messages on a given topic. In other words when the
consumer\'s registered topic matches the producer\'s topic, the broker
dispatches the message to the consumer when it receives it from the
producer.

A **Message** may be delivered to multiple Consumers, all of whom get
their own copy.

in addition, we can support a [Competing
Consumers](http://www.enterpriseintegrationpatterns.com/CompetingConsumers.html)
approach by having multiple consumers read from the same
[Channel](http://www.enterpriseintegrationpatterns.com/MessageChannel.html)
to allow us to scale out to meet load.

![TaskQueues](_static/images/TaskQueues.png)

# What happens when the consumer receives the message?

A consumer reads the **Message** using the [Service
Activator](http://www.enterpriseintegrationpatterns.com/MessagingAdapter.html)
pattern to map between an [Event Driven
Consumer](http://www.enterpriseintegrationpatterns.com/EventDrivenConsumer.html)
and a Handler.

The use of the Service Activator pattern means the complexity of the
distributed task queue is hidden from you. You just write a handler as
normal, but call it via post and create a message mapper, the result is
that your command is handled reliably, asynchronously, and in parallel
with little cognitive overhead. It just works!

# What does this look like in code

Instead of using **CommandProcessor.Send()** you use
**CommandProcessor.Post()** to send the message

``` csharp
var reminderCommand = new TaskReminderCommand(
     taskName: reminder.TaskName,
     dueDate: DateTime.Parse(reminder.DueDate),
     recipient: reminder.Recipient,
     copyTo: reminder.CopyTo);

 _commandProcessor.Post(reminderCommand);
```

You add a message mapper to tell Brighter how to serialize the message
for sending to your consumers.

``` csharp
public class TaskReminderCommandMessageMapper : IAmAMessageMapper<TaskReminderCommand>
{
    public Message MapToMessage(TaskReminderCommand request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: "Task.Reminder", messageType: MessageType.MT_COMMAND);
        var body = new MessageBody(JsonConvert.SerializeObject(request));
        var message = new Message(header, body);
        return message;
    }

    public TaskReminderCommand MapToRequest(Message message)
    {
        return JsonConvert.DeserializeObject<TaskReminderCommand>(message.Body.Value);
    }
}
```

One option is to use a *Core* assembly that contains your domain model,
handlers, message mappers etc. and then pull that assembly into
endpoints that consume such as services and web endpoints. This makes it
easy to move between in-process and out-of-process versions of the
handler. It also means you don\'t end up writing two versions of the
mapper - one on the consumer side and one on the sender side.

The [Tasks
Example](https://github.com/BrighterCommand/Brighter/tree/master/samples)
uses this strategy.

This model only works if your library is shared between components that
operate on the same bounded context i.e. Continuous Integration that are
released together. Never share such an assembly between projects that
should be released autonomously as it is a shared dependency. In that
case you \*\*must\*\* implement the mapper on both sides.

Then you write a handler as normal.

``` csharp
public class MailTaskReminderHandler : RequestHandler<TaskReminderCommand>
{
    private readonly IAmAMailGateway _mailGateway;

    public MailTaskReminderHandler(IAmAMailGateway mailGateway, IAmACommandProcessor commandProcessor)
        : this(mailGateway, commandProcessor, LogProvider.GetCurrentClassLogger())
        {}

    public MailTaskReminderHandler(IAmAMailGateway mailGateway, ILog logger) : base(logger)
    {
        _mailGateway = mailGateway;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [UsePolicy(CommandProcessor.CIRCUITBREAKER, step: 2)]
    [UsePolicy(CommandProcessor.RETRYPOLICY, step: 3)]
    public override TaskReminderCommand Handle(TaskReminderCommand command)
    {
        _mailGateway.Send(new TaskReminder(
        taskName: new TaskName(command.TaskName),
        dueDate: command.DueDate,
        reminderTo: new EmailAddress(command.Recipient),
        copyReminderTo: new EmailAddress(command.CopyTo)
        ));

        return base.Handle(command);
    }
}
```