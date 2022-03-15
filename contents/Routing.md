# Routing

## Publish-Subscribe

Brighter has a default Publish-Subscribe to messaging.

A broker provides an intermediary between the producer of a message and
a consumer. A consumer registers interest in messages that have a key or
topic. A producer sends messages with a key or topic to a broker, and
the broker sends a copy of that message to every subscribing consumer.

In messaging we sometimes refer to the list of subscribers as a
**Recipient List**, and because consumers can register their interest at
runtime instead of build we sometimes calls this a **Dynamic Recipient
List**.

The publish subscribe model works particularly well with an
**Event-Driven Architecture** (EDA). In an EDA one process, the
publisher, raises an **Event** to indicate that something of interest
happened within the process, such as an order being raised or a new
customer being added, and subscribers who receive that message can act
upon it. Processes can communicate back and forth with one process
publishing an **Event** and another system reacting and publishing its
**Event** message in turn. A **correlation id**, a unique identifier
shared by the messages allows the original producer to correlate events
raised by other producers to its message.

The advantage of publish-subscribe is coupling. Because producers do not
need to know about consumers and vice-versa then we can change the list
of consumers without the producer needing to change, or change the
producer without the consumer needing to know.

### Routing Publish-Subscribe Messages

A producer routes messages to subscribers by setting a **Topic** on the
**MessageHeader**. A **Topic** is just a string that you intend to use
as a unique identifier for this message. A simple scheme can be the
typename of the event for the Producer.

When implementing an **IAmAMessageMapper\<T\>** you set the **Topic** in
the **MessageHeader** when serializing your **Command** or **Event** to
disk. In the following example we set the **Topic** to *Task.Completed*.

``` csharp
public class TaskCompletedEventMapper : IAmAMessageMapper<TaskCompletedEvent>
{
    public Message MapToMessage(TaskCompletedEvent request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: "Task.Completed", messageType: MessageType.MT_EVENT);
        var body = new MessageBody(JsonConvert.SerializeObject(request));
        var message = new Message(header, body);
        return message;
    }
}
```

On the consumer side we configure **Perfomer**\'s to subscribe to
notifications from a Broker via a **Channel**. That **Channel**
subscribes to the **Topic**. So in the above example to receive
**TaskCompletedEvent** the **Channel** would need to subscribe to the
*Task.Completed* **Topic**.

We can configure consumers with code or configuration.

Configuration is often used to allow us to make run-time changes to
subscriber lists easily - by changing the configuration file and
restarting the consumer (another alternative is to use the Control Bus
to configure the consumer at run-time).

To configure a consumer using a configuration file we use the service
activator configuration section, which needs to be added to the
**\<configSections\>** element of your configuration file.

``` xml
<section name="serviceActivatorConnections"
         type="paramore.brighter.serviceactivator.ServiceActivatorConfiguration.ServiceActivatorConfigurationSection, paramore.brighter.serviceactivator"
         allowLocation="true"
         allowDefinition="Everywhere" />
```

To configure individual consumers we need to add elements to the
**\<serviceActivatorConnections\>** element.

``` xml
<serviceActivatorConnections>
    <connections>
        <add connectionName="paramore.example.greeting"
             channelName="greeting.command"
             routingKey="greeting.command"
             dataType="Greetings.Ports.Commands.GreetingCommand"
             timeOutInMilliseconds="200" />
    </connections>
</serviceActivatorConnections>
```

The **routingKey** property of the connection must be the same key as
used in the message mapper. This is passed to the broker to inform it
that we want to subscribe to messages with that routing key on this
channel.

## Direct Messaging

In direct messaging a producer knows its consumer. A direct message can
be fire and forget, which means it does not expect a reply, or
request-reply which means that it does. In request-reply the receiver
knows its sender as well.

The reason you might choose direct messaging over publish-subscribe is
consistency.

When using publish-subscribe work queues we are eventually consistent
-at some point in the future we will process the message, relying on
\'at least once\' delivery properties of the message queue. We don\'t
know anything about \'when\' that will happen. This means that two
processes in our system may be inconsistent for a period of time - there
is latency between them.

Consider an application that needs to bill a customer\'s credit card.

In an event driven approach, we could make the assumption that the
transaction will succeed, raise a request to bill the customer and
process the payment asynchronously. The producer of the billing request
continues as though the transaction had succeeded. Eventually the
customer is billed, and we are consistent. If we fail to bill the
customer we have to take compensating action - raising a billing failed
event, which may alert an operator and email the customer.

Our reason for taking this approach may be that our payment provider is
often slow to respond and we do not want to make the customer wait
whilst we handle details of their payment. This may not simply be about
responsiveness to the customer - it may be about scaling our system.

In a direct messaging approach, we decide that as many payment
transactions fail we do not want to process the order until the payment
has been received. At the same time for throughput on our web server we
want to work asynchronously and hand off the request to another process
which calls the payment provider. Most likely we return a 202 Accepted
from our HTTP API with a link to a resource to monitor for the results
of the transaction. In our client we display a progress indicator until
we have completed the transaction.

In this case, our requirement is that we receive a response to our
**Command** to bill.

To route this kind of message the Producer needs to send a reply-address
to the Consumer so that it can send a response back. In our case, that
reply-address is a topic that the sender subscribes to, in order to
receive the response.

Usually the Producer creates a topic for all of its replies, and matches
request to response via a correlation id. This is simply a unique
identifier that the Producer adds to the outgoing message.

To help route direct messages we provide two classes, **Request** and
**Reply** but the real work occurs within the message mapper itself.

In the following code snippet we show both the Brighter library\'s
**ReplyAddress** and **Request** as well a derived class
**HeartbeatRequest** we use to represent a request for our service to
respond with status information.

Note also the correlation id that is added to the **ReplyAddress**.

``` csharp
public class ReplyAddress
{
    public ReplyAddress(string topic, Guid correlationId)
    {
        Topic = topic;
        CorrelationId = correlationId;
    }

    public string Topic { get; private set; }
    public Guid CorrelationId { get; private set; }
}

public class Request : Command
{
    public ReplyAddress ReplyAddress { get; private set; }

    public Request(ReplyAddress replyAddress) : base(Guid.NewGuid())
    {
        ReplyAddress = replyAddress;
    }
}

public class HeartbeatRequest : Request
{
    public HeartbeatRequest(ReplyAddress sendersAddress) : base(sendersAddress)
    {
    }
}
```

When we convert this request into a **Message** via an
**IAmAMessageMapper** we set the **MessageHeader** with the topic the
Consumer should reply to. We also set the correlation id of the
sender\'s message on the header.

In the following code we also serialize the message back to a
**Command** which is then routed by Brighter to a handler. When we
serialize back to a **Command** we set the **ReplyAddress** with the
Topic and Correlation Id.

``` csharp
public class HeartbeatRequestCommandMessageMapper : IAmAMessageMapper<HeartbeatRequest>
{
    public Message MapToMessage(HeartbeatRequest request)
    {
        var header = new MessageHeader(
        messageId: request.Id,
        topic: "Heartbeat",
        messageType: MessageType.MT_COMMAND,
        correlationId: request.ReplyAddress.CorrelationId,
        replyTo: request.ReplyAddress.Topic);

        var json = new JObject(new JProperty("Id", request.Id));
        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public HeartbeatRequest MapToRequest(Message message)
    {
        var replyAddress = new ReplyAddress(topic: message.Header.ReplyTo, correlationId: message.Header.CorrelationId);
        var request = new HeartbeatRequest(replyAddress);
        var messageBody = JObject.Parse(message.Body.Value);
        request.Id = Guid.Parse((string) messageBody["Id"]);
        return request;
    }
}
```

When we reply, we again use the message mapper to ensure that we route
correctly.

Our helper class this time is **Reply** which again encapsulates the
reply-to address. We set this from the **Command** in our response. In
this code our response to the **HeartbeatRequest** is to respond with a
list of running consumers in the service.

``` csharp
public class Reply : Command
{
    public ReplyAddress SendersAddress { get; private set; }

    public Reply(ReplyAddress sendersAddress) : base(Guid.NewGuid())
    {
        SendersAddress = sendersAddress;
    }
}

public class HeartbeatReply : Reply
{
    public HeartbeatReply(string hostName, ReplyAddress sendersAddress) : base(sendersAddress)
    {
        HostName = hostName;
        Consumers = new List<RunningConsumer>();
    }

    public string HostName { get; private set; }
    public IList<RunningConsumer> Consumers { get; private set; }
}

public class RunningConsumer
{
    public RunningConsumer(ConnectionName connectionName, ConsumerState state)
    {
        ConnectionName = connectionName;
        State = state;
    }

    public ConnectionName ConnectionName { get; private set; }
    public ConsumerState State { get; private set; }
}
```

Again the key to responding is the **IAmAMessageMapper** implementation
which uses the **ReplyAddress** to route the **Message** via its
**MessageHeader** back to the caller.

``` csharp
internal class HeartbeatReplyCommandMessageMapper : IAmAMessageMapper<HeartbeatReply>
{
    public Message MapToMessage(HeartbeatReply request)
    {
        var header = new MessageHeader(
            messageId:request.Id,
            topic: request.SendersAddress.Topic,
            messageType: MessageType.MT_COMMAND,
            timeStamp: DateTime.UtcNow,
            correlationId: request.SendersAddress.CorrelationId
        );

        var json = new JObject(
            new JProperty("HostName", request.HostName),
            new JProperty("Consumers",
            new JArray(
                from c in request.Consumers
                select new JObject(
                    new JProperty("ConnectionName", c.ConnectionName.ToString()),
                    new JProperty("State", c.State)
                )
                )
            )
        );

        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public HeartbeatReply MapToRequest(Message message)
    {
        var messageBody = JObject.Parse(message.Body.Value);
        var hostName = (string) messageBody["HostName"];
        var replyAddress = new ReplyAddress(message.Header.Topic, message.Header.CorrelationId);

        var reply = new HeartbeatReply(hostName, replyAddress);
        var consumers = (JArray) messageBody["Consumers"];
        foreach (var consumer in consumers)
        {
            var connectionName = new ConnectionName((string)consumer["ConnectionName"]);
            var state = (ConsumerState)Enum.Parse(typeof (ConsumerState), (string) consumer["State"]);
            reply.Consumers.Add(new RunningConsumer(connectionName, state));
        }

        return reply;
    }
}
```

## Summary

The key to understanding routing in Brighter **IAmAMessageMapper**
implementation provides the point at which you control routing by
setting the **MessageHeader**.
