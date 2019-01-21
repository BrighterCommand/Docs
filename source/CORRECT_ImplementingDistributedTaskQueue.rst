Implementing a Distributed Task Queue
-------------------------------------

Brighter provides support for a `distributed task
queue <https://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf>`__.     \\ broken link
Instead of handling a command or event, synchronously and in-process,
work can be dispatched to a distributed task queue to be handled
asynchronously and out-of-process. The trade-off here is between the
cost of distribution (see `The Fallacies of Distributed
Computing <https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing>`__)
against performance.

For example you might have an HTTP API a rule that any given request to
that API must execute in under 100ms. On measuring the performance of a
key POST or PUT operation to your API you find that you exceed this
value. Upon realizing that much of your time is spent I/O you consider
two options:

-  Use the TPL (Task parallel library) to perform the work concurrently
-  Offload the work to a distributed task queue, ack the message, and
   allow the work to complete asynchronously

A problem with the TPL approach is that your operation can only meet the
100ms threshold if your work can be parallelised such that no sub-task
takes longer than 100ms. Your speed is always constrained by the slowest
operation that you need to parallelize. If you are I/O bound on a
resource experiencing contention beyond 100ms, you will not meet your
goal by introducing more threads. Your minimum time is your minimum
time.

You might try to fix this by acking (acknowledging) the request, and
completing the work asynchronously. This option is particularly
attractive if the work is I/O bound as you can process other requests
whilst you wait for the I/O to complete.

The downside of the async approach is that you risk that the work will
be lost if the server fails prior to completion of the work, or the app
simply recycles.

These requirements tend to push you in the direction of `Guaranteed
Delivery <http://www.eaipatterns.com/GuaranteedMessaging.html>`__ to
ensure that work you ack will eventually be handled.

A distributed task queue allows you offload work to another process, to
be handled asynchronously (once you push the work onto the queue, you
don't wait) and in parallel (you can use other cores to process the
work). It also allows you to ensure delivery of the message, eventually
(the queue will hold the work until a consumer is available to read it).

In addition use of a distributed task queue allows you to throttle
requests - you can hand work off from the web server to a queue that
only needs to consume at the rate you have resources to support. This
allows you to scale to meet unexpected demand, at the price of `eventual
consistency. <https://en.wikipedia.org/wiki/Eventual_consistency>`__

Brighter's Task Queue Architecture
----------------------------------

Brighter implements Task Queues using a `Message
Broker <http://www.enterpriseintegrationpatterns.com/MessageBroker.html>`__.

The producer sends a **Command** or **Event** to a `Message
Broker <http://www.enterpriseintegrationpatterns.com/MessageBroker.html>`__
using **CommandProcessor.Post()**.

We use an **IAmAMessageMapper** to map the **Command** or **Event** to a
**Message**. (Usually we just serialize the object to JSON and add to
the **MessageBody**), but if you want to use higher performance
serialization approaches, such as
`protobuf-net <https://github.com/mgravell/protobuf-net>`__, the message
mapper is agnostic to the way the body is formatted.)

When we deserialize we set the **MessageHeader** which includes a topic
(often we use a namespaced name for the **Command** or **Event**).

We store the created **Message** in a `Message
Store <http://www.enterpriseintegrationpatterns.com/MessageStore.html>`__
for use by **CommandProcessor.Repost()** if we need to resend a failed
message.

The Message Broker manages a `Recipient
List <http://www.enterpriseintegrationpatterns.com/RecipientList.html>`__
of subscribers to a topic. When it receives a **Message** the Broker
looks at the topic in the **MessageHeader** and dispatches the
**Message** to the `Recipient
Channels <http://www.enterpriseintegrationpatterns.com/MessageChannel.html>`__
identified by the Recipient List.

The consumer registers a `Recipient
Channel <http://www.enterpriseintegrationpatterns.com/MessageChannel.html>`__
to receive messages on a given topic. In other words when the consumer's
registered topic matches the producer's topic, the broker dispatches the
message to the consumer when it receives it from the producer.

A **Message** may be delivered to multiple Consumers, all of whom get
their own copy.

in addition, we can support a `Competing
Consumers <http://www.enterpriseintegrationpatterns.com/CompetingConsumers.html>`__
approach by having multiple consumers read from the same
`Channel <http://www.enterpriseintegrationpatterns.com/MessageChannel.html>`__
to allow us to scale out to meet load.

|TaskQueues|

Do I have to use a Broker, what about MSMQ?
-------------------------------------------

Brighter removes some complexity from its implementation by relying on
the Message Broker to provide a number of services. First the Broker
provides message routing. The producer does not need to have any idea
where the consumers are located, only where the broker is located. This
makes it easy to relocate your consumers, and when then begin
subscribing the Broker will figure out how to deliver to them. It also
supports a recipient list when routing messages: one producer can send
to many consumers. Second we rely on the Broker to provide a clustered
High Availability (HA) solution to queueing. We want to be able to send
a message to the Broker cluster and rely on the Broker to deliver it,
eventually.

Without a Broker, using a point-to-point solution we have to provide a
lot of this infrastructure ourselves, such as routing and distribution
and how to do so in a way that is HA.

For this reason we don't support a point-to-point approach like MSMQ or
sending directly to a service via HTTP.

(We do have an experimental implementation of an `HTTP-based
broker <https://github.com/BrighterCommand/Paramore.Contrib/tree/master/Renegade>`__
using the RESTMS specification but it is not production-grade, and only
in-memory as of today).

What happens when the consumer receives the message?
----------------------------------------------------

A consumer reads the **Message** using the `Service
Activator <http://www.enterpriseintegrationpatterns.com/MessagingAdapter.html>`__
pattern to map between an `Event Driven
Consumer <http://www.enterpriseintegrationpatterns.com/EventDrivenConsumer.html>`__
and a Handler.

The use of the Service Activator pattern means the complexity of the
distributed task queue is hidden from you. You just write a handler as
normal, but call it via post and create a message mapper, the result is
that your command is handled reliably, asynchronously, and in parallel
with little cognitive overhead. It just works!

What does this look like in code
--------------------------------

Instead of using **CommandProcessor.Send()** you use
**CommandProcessor.Post()** to send the message

.. highlight:: csharp

::

    var reminderCommand = new TaskReminderCommand(
         taskName: reminder.TaskName,
         dueDate: DateTime.Parse(reminder.DueDate),
         recipient: reminder.Recipient,
         copyTo: reminder.CopyTo);

     _commandProcessor.Post(reminderCommand);



You add a message mapper to tell Brighter how to serialize the message
for sending to your consumers.

.. highlight:: csharp

::

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



One option is to use a *Core* assembly that contains your domain model,
handlers, message mappers etc. and then pull that assembly into
endpoints that consume such as services and web endpoints. This makes it
easy to move between in-process and out-of-process versions of the
handler. It also means you don't end up writing two versions of the
mapper one on the consumer side and one on the sender side.

The `Tasks
Example <https://github.com/BrighterCommand/Brighter/tree/master/samples>`__
uses this strategy.

This model only works if your library is shared between components that
operate on the same bounded context i.e. Continuous Integration that are
released together. Never share such an assembly between projects that
should be released autonomously as it is a shared dependency. In that
case you \*\*must\*\* implement the mapper on both sides.

Then you write a handler as normal.

.. highlight:: csharp

::

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



The Dispatcher
--------------

To ensure that messages reach the handlers from the queue you have to
use the **Dispatcher**.

The Dispatcher reads messages of input channels. Internally it creates a
message pump for each channel, and allocates a thread to run that
message pump. The pump consumes messages from the channel, using the
**Message Mapper** to translate them into a **Message** and from there a
**Command** or **Event**. It then dispatches those to handlers (using
the Brighter **Command Processor**).

To use the Dispatcher you need to host it in a consumer application.
Usually a console application or Windows Service is appropriate. We
recommend using `Topshelf <http://topshelf-project.com/>`__ to host your
consumers.

The following code shows an example of using the **Dispatcher** from
Topshelf. The key methods are **Dispatcher.Receive()** to start the
message pumps and **Dispatcher.End()** to shut them.

We do allow you to start and stop individual channels, but this is an
advanced feature for operating the services.

.. highlight:: csharp

::

    internal class GreetingService : ServiceControl
    {
        private Dispatcher _dispatcher;

        public GreetingService()
        {
           /* Configfuration Code Goes here*/
        }

        public bool Start(HostControl hostControl)
        {
            _dispatcher.Receive();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _dispatcher.End().Wait();
            _dispatcher = null;
            return false;
        }

        public void Shutdown(HostControl hostcontrol)
        {
            if (_dispatcher != null)
                _dispatcher.End();
            return;
        }
    }



Configuration
-------------

So how do we route messages from the channel to the handler? The answer
is the framework uses configuration that your provide to do that.
Configuration is the subject of this documentation
`here <DistributedTaskQueueConfiguration.html>`__.

.. |TaskQueues| image:: _static/images/TaskQueues.png

