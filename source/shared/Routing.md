# Routing

### Routing Messages

A producer routes messages to subscribers by setting a **Topic** on the **MessageHeader**. A **Topic** is just a string that you intend to use as a unique identifier for this message. A simple scheme can be the
typename of the event for the Producer.

When implementing an **IAmAMessageMapper\<T\>** you set the **Topic** in the **MessageHeader** when serializing your **Command** or **Event** to disk. In the following example we set the **Topic** to *Task.Completed*.

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

The **routingKey** property of the connection must be the same key as used in the message mapper. This is passed to the broker to inform it that we want to subscribe to messages with that routing key on this channel.

## Events 

Brighter has a default Publish-Subscribe approach to events.

A broker provides an intermediary between the producer of a message and a consumer. A consumer registers interest in messages that have a key or topic. A producer sends messages with a key or topic to a broker, and the broker sends a copy of that message to every subscribing consumer.


## Commands 

For a command the producer knows its consumer. A command may be fire and forget, which means it does not expect a reply, or request-reply which means that it does. In request-reply the receiver knows its sender as well.

The reason you might choose a command over an event is causality.

Consider an application that needs to bill a customer's credit card.

In an event driven approach, we could make the assumption that the transaction will succeed, raise an event to bill the customer and process the payment asynchronously. The producer of the billing request continues as though the transaction had succeeded. Eventually the customer is billed, and we are consistent. Our reason for taking this approach may be that our payment provider is often slow to respond and we do not want to make the customer wait whilst we handle details of their payment. If we fail to bill the customer we have to take compensating action - raising a billing failed event, which may alert an operator and email the customer. 

The problem is that this compensating action has to "chase" the success path, which may have already taken actions, such as shipping to the customer, that become expensive to reverse.

With a command, we decide that as a payment transaction can fail we do not want to process the order until the payment has been received. In this case, our requirement is that we receive a response to our **Command** to bill. To route a command the Producer may send a reply-address to the Consumer so that it can send a response back on a 'private' channel. In our case, that reply-address is a topic that the sender subscribes to, in order to receive the response.

Usually the Producer creates a topic for all of its replies, and matches request to response via a correlation id. This is simply a unique identifier that the Producer adds to the outgoing message.

(Because the customer has to wait, in case we want to signal an error we are probably returning a 202 Accepted from our HTTP API with a link to a resource to monitor for the results of the transaction. In our client we display a progress indicator until we have completed the transaction.)

To help route direct messages we provide two classes, **Request** and **Reply** but the real work occurs within the message mapper itself.

Note also the correlation id that is added to the **ReplyAddress**.

``` csharp

public class MyRequest : Request
{
    public MyRequest(ReplyAddress sendersAddress) : base(sendersAddress)
    {
    }
}

public class MyReply : Reply
{
    public MyReply(ReplyAddress sendersAddress) : base(sendersAddress)
    {
    }
}
```

When we convert this request into a **Message** via an **IAmAMessageMapper** we set the **MessageHeader** with the topic the Consumer should reply to. We also set the correlation id of the sender\'s message on the header.

In the following code we also serialize the message back to a **Command** which is then routed by Brighter to a handler. When we serialize back to a **Command** we set the **ReplyAddress** with the Topic and Correlation Id.

``` csharp
public class MyRequestMessageMapper : IAmAMessageMapper<MyRequest>
{
    public Message MapToMessage(MyRequest request)
    {
        var header = new MessageHeader(
        messageId: request.Id,
        topic: "MyRequest",
        messageType: MessageType.MT_COMMAND,
        correlationId: request.ReplyAddress.CorrelationId,
        replyTo: request.ReplyAddress.Topic);

        var json = new JObject(new JProperty("Id", request.Id));
        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public MyRequest MapToRequest(Message message)
    {
        var replyAddress = new ReplyAddress(topic: message.Header.ReplyTo, correlationId: message.Header.CorrelationId);
        var request = new MyRequest(replyAddress);
        var messageBody = JObject.Parse(message.Body.Value);
        request.Id = Guid.Parse((string) messageBody["Id"]);
        return request;
    }
}
```

When we reply, we again use the message mapper to ensure that we route correctly. Again the key to responding is the **IAmAMessageMapper** implementation which uses the **ReplyAddress** to route the **Message** via its **MessageHeader** back to the caller. Note that whilst the response could be considered an event - a fact raised in response to a command - because it only has one Consumer, the sender, we route it as a command. If you want to broadcast the outcome, treat it as an event, but add **ReplyAddress** to your class derived from **Event** to correlate with the command.

``` csharp
internal class MyReplyMessageMapper : IAmAMessageMapper<MyReply>
{
    public Message MapToMessage(MyReply request)
    {
        var header = new MessageHeader(
            messageId:request.Id,
            topic: request.SendersAddress.Topic,
            messageType: MessageType.MT_COMMAND,
            timeStamp: DateTime.UtcNow,
            correlationId: request.SendersAddress.CorrelationId
        );

        var json = ...//serialize the reply body

        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public MyReply MapToRequest(Message message)
    {
        var replyAddress = new ReplyAddress(message.Header.Topic, message.Header.CorrelationId);

        var reply = new MyReply(replyAddress);
        
        ...//deserialize the body

        return reply;
    }
}
```

## Summary

The key to understanding routing in Brighter is that the **IAmAMessageMapper** implementation provides the point at which you control routing by setting the **MessageHeader**.
