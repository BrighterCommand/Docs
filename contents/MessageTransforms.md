---
description: "A transform is middleware in the mapping pipeline, and Brighter finds it by reflecting over the attributes on your mapper's MapToMessage and MapToRequest methods."
layout:
  description:
    visible: false
---

# Message Transforms

> **Explanation** · Applies to **Brighter V10** · Prerequisites: [Message Mappers](/contents/MessageMappers.md)

A transform is middleware in the *mapping* pipeline, and Brighter finds it by reflecting over the attributes on your mapper's **MapToMessage** and **MapToRequest** methods. So a transform of your own requires a **custom message mapper** to attach it to: the [default message mappers](/contents/DefaultMessageMappers.md) carry only the `[CloudEvents]` transform Brighter puts there itself, and you cannot add an attribute to a type you do not own.

## Message Transformers

Some concerns are orthogonal to how you map a **IRequest** into a **Message** or how you map a **Message** into an **IRequest**. Instead they concern how we process that Message. A typical list of such concerns might include: handling large message payloads (compression of moving to a distributed file store), encryption, registering or validating schema, and adding common metadata to headers.

A *Transform* is a middleware that runs as part of the pipeline we use to map a **IRequest** into a **Message** or how you map a **Message** into an **IRequest**. A transform implements an **IMessageTransformAsync**. (All transforms are async).

``` csharp
// ...
public interface IAmAMessageTransformAsync : IDisposable
{
    void InitializeWrapFromAttributeParams(params object[] initializerList);
    void InitializeUnwrapFromAttributeParams(params object[] initializerList);
    Task<Message> WrapAsync(Message message, CancellationToken cancellationToken);
    Task<Message> UnwrapAsync(Message message, CancellationToken cancellationToken);
}
```

### Wrap

When we *wrap* the source is the *Message Mapper* and the transform is applied to the **Message** that you generate from the **IRequest** in your **MapToMessage**.

You indicate that you wish to *wrap* a *Message Mapper* with the **WrapWithAttribute** associated with the **IMessageTransformAsync** you want to apply to the **Message** you have created from the **IRequest**. In the example below we use a **ClaimCheck** to move large message payloads (those over the threshold) into a *luggage store* (for example an S3 bucket).

``` csharp
// ...
[ClaimCheck(step:0, thresholdInKb: 256)]
public Message MapToMessage(GreetingEvent request)
{
    var header = new MessageHeader(messageId: request.Id, topic: typeof(GreetingEvent).FullName.ToValidSNSTopicName(), messageType: MessageType.MT_EVENT);
    var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
    var message = new Message(header, body);
    return message;
}
```

### Unwrap

When we *unwrap* the sink is the *Message Mapper* and the transform is applied to the **Message** before you turn it into an **IRequest** in your **MapToRequest**.

You indicate that you wish to *unwrap* a *Message Mapper* with the **UnwrapWithAttribute** associated with the **IMessageTransformAsync** you want to apply to the **Message** before you create your **IRequest**. In the example below we use a **RetrieveClaim** to retrieve a large message payload (most likely stored by a Claim Check in a *luggage store*) that will provide the body of our **Message** before we deserialize it to the **IRequest**. 

``` csharp
// ...
[RetrieveClaim(step:0)]
public GreetingEvent MapToRequest(Message message)
{
    var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
    
    return greetingCommand;
}

```

### Transform, Wrap and Unwrap

Usually your **WrapWithAttribute** and **UnwrapWithAttribute** are paired and opposite. Usually they associate with a common **IMessageTransformAsync** that implements support for both transforms: the **WrapWithAttribute** results in the **WrapAsync** method of the transform being called (the **Message** is passed to it); the **UnwrapWithAttribute** results in the **UnwrapAsync** method being called (again the **Message** is passed to it).

Both the **WrapWithAttribute** and the **UnwrapWithAttribute** are a type of **TransformAttribute**

``` csharp
// ...

public abstract class TransformAttribute : Attribute
    {
        public int Step { get; set; }
        public abstract Type GetHandlerType();
        public virtual object[] InitializerParams()
        {
            return new object[0];
        }

```

To implement a **TransformAttribute** you need to create a derived type that overrides the **GetHandlerType** to return the type of your **IMessageTransformAsync**. 

#### Step

Step specifies the order in which a transform runs (attributes are not guaranteed to be made available in top-down order by reflection). This can be important in transforms. Imagine that you want to compress any message over 256Kb, but because a large enough message might still not be small enough after compression,  a message that is *still* over 256Kb to distributed storage. In this case you would want to make sure that the step value for compression was lower than the step value to offload to distributed storage.

#### Passing Parameters to a Transform

If you want to pass parameters to your transform, they must be available at compile time as arguments to your derived **TransformAttribute**. The parameters of your attribute's constructor can be set from an attribute. Your attribute can then store these parameters in private fields. We call your derived attributes **InitializeParams** method after instantiating your **IMessageTransformAsync**, and pass the values to that object via either the **InitializeWrapFromAttributeParams** or **InitializeUnwrapFromAttributeParams** as appropriate for the type of **TransformAttribute** (either **WrapWithAttribute** or **UnwrapWithAttribute**).

So in this example, the **ClaimCheck** takes a parameter for the *threshold* at which point we move the body of the message into distributed storage as opposed to serializing it in the message body.

``` csharp
// ...
public class ClaimCheck : WrapWithAttribute
{
    private readonly int _thresholdInKb;

    public ClaimCheck(int step, int thresholdInKb = 0) : base(step)
    {
        _thresholdInKb = thresholdInKb;
    }

    public override object[] InitializerParams()
    {
        return new object[] { _thresholdInKb };
    }

    public override Type GetHandlerType()
    {
        return typeof(ClaimCheckTransformer);
    }
}
```

### Message Transformer Factory

Because we do not know how to construct user-defined types, you have to pass us a **IAmAMessageTransformerFactory** that constructs instances of your **IMessageTransformAsync**. 

Normally, you implement this using your Inversion of Control container. We provide an implementation for the .NET Inversion of Control container **ServiceCollection** with **ServiceProviderTransformerFactory**. You need a reference to the following NuGet package:

* **Paramore.Brighter.Extensions.DependencyInjection**


If you are using HostBuilder, our extension methods mean that you benefit from automatic inclusion of the **ServiceProviderTransformerFactory** and registration of your **IMessageTransformAsync**.

## Transform Pipeline Example

Transform attributes allow you to apply transformations to messages as they're mapped. This is a powerful pattern for cross-cutting concerns like handling large messages, compression, or encryption.

### Claim Check Transform

The [Claim Check pattern](ClaimCheck.md) stores large message payloads externally (e.g., S3) and sends only a reference in the message.

Here's a real example from the Brighter samples:

```csharp
// ...
public class GreetingEventMessageMapper : IAmAMessageMapper<GreetingEvent>
{
    public IRequestContext? Context { get; set; }

    [ClaimCheck(step: 0, thresholdInKb: 256)]
    public Message MapToMessage(GreetingEvent request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        var message = new Message(header, body);
        return message;
    }

    [RetrieveClaim(step: 0)]
    public GreetingEvent MapToRequest(Message message)
    {
        var greetingEvent = JsonSerializer.Deserialize<GreetingEvent>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return greetingEvent!;
    }
}
```

**How it works:**

**On Send (MapToMessage):**

1. Handler serializes `GreetingEvent` to JSON
2. `[ClaimCheck]` attribute checks message size
3. If > 256KB, uploads payload to luggage store (e.g., S3)
4. Replaces body with reference (uses CloudEvents `dataref` extension)
5. Sends lightweight message with just the reference

**On Receive (MapToRequest):**

1. `[RetrieveClaim]` attribute checks for `dataref`
2. If present, downloads payload from luggage store
3. Replaces message body with actual payload
4. Deserializes to `GreetingEvent`

### Compression Transform

Similarly, you can compress messages:

```csharp
// ...
public class CompressedOrderMapper : IAmAMessageMapper<LargeOrder>
{
    public IRequestContext? Context { get; set; }

    [Compress(step: 0, compressionType: CompressionType.Gzip)]
    public Message MapToMessage(LargeOrder request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    [Decompress(step: 0)]
    public LargeOrder MapToRequest(Message message)
    {
        var order = JsonSerializer.Deserialize<LargeOrder>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return order!;
    }
}
```

### Chaining Multiple Transforms

You can chain transforms by using different step numbers:

```csharp
// ...
public class SecureLargeOrderMapper : IAmAMessageMapper<SensitiveOrder>
{
    public IRequestContext? Context { get; set; }

    [RemovePII(step: 0)]                          // First: Remove PII
    [Compress(step: 1, compressionType: CompressionType.Gzip)]  // Second: Compress
    [ClaimCheck(step: 2, thresholdInKb: 512)]     // Third: Claim check if still large
    public Message MapToMessage(SensitiveOrder request, Publication publication)
    {
        // Standard serialization
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    [RetrieveClaim(step: 0)]                      // First: Retrieve if claim checked
    [Decompress(step: 1)]                         // Second: Decompress
    [RestorePII(step: 2)]                         // Third: Restore PII
    public SensitiveOrder MapToRequest(Message message)
    {
        var order = JsonSerializer.Deserialize<SensitiveOrder>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return order!;
    }
}
```

The transforms execute in order based on the `step` parameter. On receive, they execute in reverse order.

## Further Reading

- [Message Mappers](/contents/MessageMappers.md) - Writing a custom message mapper
- [Default Message Mappers](/contents/DefaultMessageMappers.md) - The default route, when you need no transform of your own
- [Claim Check Pattern](/contents/ClaimCheck.md) - Offloading large payloads to a luggage store
- [Compression](/contents/Compression.md) - Compressing message bodies
- [S3 Luggage Store](/contents/S3LuggageStore.md) - Configuring an S3 bucket as the luggage store
