---
description: "The Claim Check pattern helps us reduce the size of our messages, without losing information that we need to exchange."
layout:
  description:
    visible: false
---

# Claim Check

> **Explanation** · Applies to **Brighter V10**

The [Claim Check](https://www.enterpriseintegrationpatterns.com/patterns/messaging/StoreInLibrary.html) pattern helps us reduce the size of our messages, without losing information that we need to exchange. 

Instead of being transmitted in the body of the message, the payload is written to a distributed file storage and a token to retrieve the payload is sent instead. The receiver can read the payload by taking the reference and requesting it from the distributed file storage. The metaphor here is a luggage check. Instead of carrying large items of luggage aboard an aircraft we check them into the hold of the aircraft. The airline gives us a claim check for our luggage, that matches a tag on the bag. This pattern is sometimes called Reference Based Messaging.

## Claim Check and Retrieve Claim

We treat the Claim Check pattern as [Transformer](/contents/MessageTransforms.md#message-transformer-factory) middleware.

We provide a **WrapWithAttribute** of **ClaimCheck** that will use the **ClaimCheckTransformer** to upload the body of your **Message** to a *luggage store* replacing it with a body that contains an claim check for the body, as well as setting a message header of "claim_check_header" with the claim. The trigger for this behavior can be controlled by a threshold parameter that sets the size above which the message body should be moved to the *luggage store*.

In the following example we add the **ClaimCheck** attribute to the *Message Mapper* with a trigger at 256Kb

```csharp
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;

[ClaimCheck(step: 0, thresholdInKb: 256)]
public Message MapToMessage(GreetingEvent request, Publication publication)
{
	var header = new MessageHeader(messageId: request.Id, topic: publication.Topic!, messageType: MessageType.MT_EVENT);
	var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
	var message = new Message(header, body);
	return message;
}
```

We provide a matching **UnwrapWithAttribute** of **RetrieveClaim** that will use the **ClaimCheckTransformer** to download the body of your **Message** from a luggage store and replace the existing body (likely a claim check reference) with the downloaded content.

```csharp
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;

[RetrieveClaim(step: 0, retain: false)]
public GreetingEvent MapToRequest(Message message)
{
	var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
	
	return greetingCommand!;
}
```

An optional parameter 'retain' determines if we keep the body in storage after it is retrieved or delete it. The default is to delete it.

The outcome of these attributes is that the uploading of the body to the *luggage store* and downloading from it is transparent to your code. You serialize your **IRequest** to a **Message** as normal, or serialize your **Message** to an **IRequest** as normal - everything happens in the middleware pipeline.

## The Luggage Store

The *luggage store* is where we store the body of the message for later retrieval. We provide implementations of the Luggage Store interface for popular distributed stores, but you can implement the interface for any that we do not provide.

```csharp
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter.Observability;

public interface IAmAStorageProviderAsync
{
    IAmABrighterTracer? Tracer { get; set; }
    Task EnsureStoreExistsAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(string claimCheck, CancellationToken cancellationToken = default);
    Task<Stream> RetrieveAsync(string claimCheck, CancellationToken cancellationToken = default);
    Task<bool> HasClaimAsync(string claimCheck, CancellationToken cancellationToken = default);
    Task<string> StoreAsync(Stream stream, CancellationToken cancellationToken = default);
}
```

* `Tracer`: the tracer used to capture telemetry. You do not set this — the registration does
* `EnsureStoreExistsAsync`: creates the store, or checks that it is there, according to `StorageOptions.Strategy`
* `DeleteAsync`: deletes an item from the store
* `RetrieveAsync`: creates a stream for a download from the store
* `HasClaimAsync`: does the claim check exist in the store
* `StoreAsync`: puts a stream into the store and returns a claim, an identifier that can later be used to delete, retrieve or check for the existence of what was stored

There is a synchronous `IAmAStorageProvider` alongside it carrying the same operations, and
**every store implements both** — which is what registration requires.

## Luggage Store Implementations

Seven implementations ship with V10:

| Store | Package |
|---|---|
| `S3LuggageStore` | `Paramore.Brighter.Transformers.AWS`, and `Paramore.Brighter.Transformers.AWS.V4` for AWS SDK v4 |
| `AzureBlobLuggageStore` | `Paramore.Brighter.Transformers.Azure` |
| `GcsLuggageStore` | `Paramore.Brighter.Transformers.Gcp` |
| `MongoDbLuggageStore` | `Paramore.Brighter.Transformers.MongoGridFS` |
| `FileSystemStorageProvider` | `Paramore.Brighter` (core) |
| `InMemoryStorageProvider` | `Paramore.Brighter` (core) |
| `NullLuggageStore` | `Paramore.Brighter` (core) — the default, and every method throws |

**Registering one of them is a step of its own**, and `AddBrighter` leaves you with the null
store until you do. See
[Put a Large Payload Behind a Claim Check](/contents/HandlingLargeMessages.md), which covers the
registration, the threshold and how to tell whether the payload really left.

* [S3 Luggage Store](/contents/S3LuggageStore.md)
