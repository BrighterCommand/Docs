# Message Mappers

A message mapper turns domain code into a Brighter **Message**. A Brighter **Message** has a **MessageHeader** for information about the message. Key properties are: **TimeStamp**, **Topic**, and **Id**.  The **Message** also has a **MessageBody**, which contains the payload. 

The messageType parameter tells the Dispatcher that listens to this message, how to treat it, as a Command or an Event. Brighter's *Dispatcher* dispatches a **Message** using either **commandProcessor.Send()** for **MT_COMMAND** or **commandProcessor.Publish()** for **MT_EVENT**.

Typically, you serialize your request as the **MessageBody** for in **MapToMessage** and serialize your **MessageBody** into a request in **MapToRequest**.

The body is a byte[] and as such we can support any format that can be converted into a byte[] as the message body.

Because [message oriented middleware](#message-oriented-middleware-mom) typically looks in a header for routing information, you add your routing information in the **MessageHeader**.

Each individual transport has code to turn a Brighter format message into a message oriented middleware compatible message, and vice versa, so your code only needs to translate to and from the Brighter format.

## Writing A Message Mapper

We use **IAmAMessageMapper\<T\>** to map between messages in the External Bus and a **Message**.

You create a **Message Mapper** by deriving from **IAmAMessageMapper\<TaskReminderCommand\>** and implementing the **MapToMessage()** and **MapToRequest** methods.

An example follows:

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
