---
description: "A message mapper turns domain code into a Brighter Message."
layout:
  description:
    visible: false
---

# Message Mappers

> **Explanation** · Applies to **Brighter V10**

A message mapper turns domain code into a Brighter **Message**. A Brighter **Message** has a **MessageHeader** for information about the message. Key properties are: **TimeStamp**, **Topic**, and **Id**.  The **Message** also has a **MessageBody**, which contains the payload. 

The messageType parameter tells the Dispatcher that listens to this message, how to treat it, as a Command or an Event. Brighter's *Dispatcher* dispatches a **Message** using either **commandProcessor.Send()** for **MT_COMMAND** or **commandProcessor.Publish()** for **MT_EVENT**.

Typically, you serialize your request as the **MessageBody** for in **MapToMessage** and serialize your **MessageBody** into a request in **MapToRequest**.

The body is a byte[] and as such we can support any format that can be converted into a byte[] as the message body.

Because [message oriented middleware](/contents/BasicConcepts.md#message-oriented-middleware-mom) typically looks in a header for routing information, you add your routing information in the **MessageHeader**.

Each individual transport has code to turn a Brighter format message into a message oriented middleware compatible message, and vice versa, so your code only needs to translate to and from the Brighter format.

In V10 you usually do not write one. [Default Message Mappers](/contents/DefaultMessageMappers.md) are the default route; write the mapper below when you need a custom one — and note that a [message transform](/contents/MessageTransforms.md) is one of the reasons you would.

## Writing A Message Mapper

We use **IAmAMessageMapper\<T\>** to map between messages in the External Bus and a **Message**.

You create a **Message Mapper** by deriving from **IAmAMessageMapper\<TaskReminderCommand\>** and implementing the **MapToMessage()** and **MapToRequest** methods.

An example follows:

``` csharp
public class GreetingMadeMessageMapper : IAmAMessageMapper<GreetingMade>
{
    public Message MapToMessage(GreetingMade request)
    {
        var header = new MessageHeader(messageId: request.Id, topic: "GreetingMade", messageType: MessageType.MT_EVENT); 
        var payload = System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.General));
        var body = new MessageBody(payload, ApplicationJson, CharacterEncoding.UTF8);
        var message = new Message(header, body);
        return message;
    }
    
    public GreetingMade MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<GreetingMade>(message.Body.Value, JsonSerialisationOptions.Options);
    }
}
```

## Brighter Message Structure

Brighter divides a message into two parts:

* Header: The header contains metadata (data about the message). It is typically used to control how we process the payload or provide additional context about it.
* Body: The body contains the payload, which is usually the **Command** or **Event** being raised for the consumer to action

### The Message Header

The Message Header has a number of Brighter defined properties and a bag that can be used for user-defined properties.

#### Common Properties

* **Id**(GUID): The identifier for this message
* **Topic**(string): The topic this message should be sent to, used to route the message in most transports
* **MessageType** (enum): The type of message: (Unacceptable (not translated), None (null object), Command, Event, Document, Quit (terminats a pump))
* **CorrelationId** (GUID): Is this message a response to another message (usually an event reply to a command), if so this is the id that links them
* **ReplyTo** (string): A topic to reply to. In a request-reply set this to tell the receiver where to send replies
* **ContentType** (string): Normally, allow the **MessageBody** (below) to set this.
* **PartitionKey** (string): Where consistent hashing is used to partition a stream, what is the value to partition on

#### Brighter Properties

* **DelayedMilliseconds** (int): If we chose to retry with a delay, how long for?
* **HandledCount** (int): How many times have we tried to handle this message
* **Telemetry** (MessageTelemetry): Open Telemetry information for the message

#### Routing

In **MapToMessage**, the **topic** parameter on the **MessageHeader** controls the topic (or routing key) which we use when publishing a message to the external bus. We use this value when using the SDK for the message oriented middleware transport to publish a message on that transport.

For this reason it is the **MessageMapper** that controls how messages published to the external bus are routed.


### The Message Body 

The Message Body stores the content for transmission over a transport as a byte[]. This supports both plain text and binary payloads. Your choice of payload type is constrained by what the transport requires or supports.

In many cases the easiest option is to send the payload as plain text, as this is the easiest to inspect if you need to debug your messages. In this case the simplest path is to serialize the **Command** or **Event** as JSON and deserialize from that JSON. MessageBody contains a constructor that takes a string with two optional parameters, a media type (which defaults to **application/json**) and a character encoding type for the string (which defaults to **CharacterEncoding.UTF8**),

```csharp
public MessageBody(string body, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
{
    ...
```

which can be used as follows (or omitting the default parameters)

```csharp

var payload = System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.General));
var body = new MessageBody(payload, ApplicationJson, CharacterEncoding.UTF8);

```

If your payload is binary, then we provide two constructors that can be used to write bytes. For backwards compatibility these constructors also default to application/json and UTF-8. However, if you have binary content we recommend setting the media type to application/octet-stream and the character encoding to either **CharacterEncoding.Base64** if it needs transmission as a string, or **CharacterEncoding.Raw** if not).

```csharp

public MessageBody(byte[] bytes, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
{
    ...

public MessageBody(in ReadOnlyMemory<byte> body, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
{
    ...

```

For example, when writing a Kafka payload with leading bytes indicating the schema id, you would want to use a binary payload because conversion to and from a UTF8 string is lossy. Here we serialize the payload with the Kafka header (Magic Byte (0) + Schema Id Bytes) and a JSON payload using the Confluent Serdes serializer. Even though we serialize to JSON, because of the header bytes we treat the payload as binary:

```csharp

public Message MapToMessage(GreetingEvent request)
{
    var header = new MessageHeader(messageId: request.Id, topic: Topic, messageType: MessageType.MT_EVENT);
    //This uses the Confluent JSON serializer, which wraps Newtonsoft but also performs schema registration and validation
    var serializer = new JsonSerializer<GreetingEvent>(_schemaRegistryClient, ConfluentJsonSerializationConfig.SerdesJsonSerializerConfig(), ConfluentJsonSerializationConfig.NJsonSchemaGeneratorSettings()).AsSyncOverAsync();
    var s = serializer.Serialize(request, _serializationContext);
    var body = new MessageBody(s, MediaTypeNames.Application.Octet, CharacterEncoding.Raw);
    header.PartitionKey = _partitionKey;

    var message = new Message(header, body);
    return message;
}

```

The **Value** property of the **MessageBody** returns a string depending on the character encoding type of the body. If you do not set a character encoding then we assume a standard UTF8 **string**; if you set the character encoding to base64 or raw, we return a base64 string; if you set the character encoding to ascii we will return an ascii string.


### Options for System.Text.Json Serialization

The most common solution to serialization of the message payload is to use System.Text.Json to convert the message's metadata to JSON for sending over a messaging middleware transport. You can adjust the behavior of this serialization through our **JsonSerialisationOptions**. See [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#configuring-json-serialization) for more on how to set your options.

You can then use this, when you want to set options consistently for message serialization.

``` csharp
   public GreetingMade MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<GreetingMade>(message.Body.Value, JsonSerialisationOptions.Options);
    }
```

## Further Reading

- [Default Message Mappers](/contents/DefaultMessageMappers.md) - The default route, and when a custom mapper is needed
- [Message Transforms](/contents/MessageTransforms.md) - Wrap, unwrap and the transformer factory
- [Cloud Events Support](/contents/CloudEventsSupport.md) - CloudEvents attributes on a mapped message
