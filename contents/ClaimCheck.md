# Claim Check

The [Claim Check](https://www.enterpriseintegrationpatterns.com/patterns/messaging/StoreInLibrary.html) pattern helps us reduce the size of our messages, without losing information that we need to exchange. 

Instead of being transmitted in the body of the message, the payload is written to a distributed file storage and a token to retrieve the payload is sent instead. The receiver can read the payload by taking the reference and requesting it from the distributed file storage. The metaphor here is a luggage check. Instead of carrying large items of luggage aboard an aircraft we check them into the hold of the aircraft. The airline gives us a claim check for our luggage, that matches a tag on the bag. This pattern is sometimes called Reference Based Messaging.

## Claim Check and Retrieve Claim

We treat the Claim Check pattern as [Transformer](/contents/MessageMappers.md#message-transformer-factory) middleware.

We provide a **WrapWithAttribute** of **ClaimCheck** that will use the **ClaimCheckTransformer** to upload the body of your **Message** to a *luggage store* replacing it with a body that contains an claim check for the body, as well as setting a message header of "claim_check_header" with the claim. The trigger for this behavior can be controlled by a threshold parameter that sets the size above which the message body should be moved to the *luggage store*.

In the following example we add the **ClaimCheck** attribute to the *Message Mapper* with a trigger at 256Kb

``` csharp
[ClaimCheck(step:0, thresholdInKb: 256)]
public Message MapToMessage(GreetingEvent request)
{
	var header = new MessageHeader(messageId: request.Id, topic: typeof(GreetingEvent).FullName.ToValidSNSTopicName(), messageType: MessageType.MT_EVENT);
	var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
	var message = new Message(header, body);
	return message;
}
```

We provide a matching **UnwrapWithAttribute** of **RetrieveClaim** that will use the **ClaimCheckTransformer** to download the body of your **Message** from a luggage store and replace the existing body (likely a claim check reference) with the downloaded content.

``` csharp
[RetrieveClaim(0, retain:false)]
public GreetingEvent MapToRequest(Message message)
{
	var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
	
	return greetingCommand;
}

```

An optional parameter 'retain' determines if we keep the body in storage after it is retrieved or delete it. The default is to delete it.

The outcome of these attributes is that the uploading of the body to the *luggage store* and downloading from it is transparent to your code. You serialize your **IRequest** to a **Message** as normal, or serialize your **Message** to an **IRequest** as normal - everything happens in the middleware pipeline.

## The Luggage Store

The *luggage store* is where we store the body of the message for later retrieval. We provide implementations of the Luggage Store interface for popular distributed stores, but you can implement the interface for any that we do not provide.

```csharp

   public interface IAmAStorageProviderAsync
    {
        Task DeleteAsync(string claimCheck, CancellationToken cancellationToken);
        Task<Stream> DownloadAsync(string claimCheck, CancellationToken cancellationToken);
        Task<bool> HasClaimAsync(string claimCheck, CancellationToken cancellationToken);
        Task<string> UploadAsync(Stream stream, CancellationToken cancellationToken);
    }

```

* DeleteAsync: Deletes a item from the store
* DownloadAsync: Creates a stream for a download from the store
* HasClaimAsync: Does the claim check exist in the store
* UploadAsync: Uploads a stream to the store and returns a claim, an identifier that can later be used to delete, download or check for the existence of the file uploaded to the store.

We provide the following implementations of **IAmAStorageProviderAsync:

* [**S3LuggageStore](/contents/S3LuggageStore.md)