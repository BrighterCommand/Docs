# Compression

The Compression transform helps us reduce the size of a message using a compression algorithm. It is an efficient approach to reducing the size of a payload. 

We offer [gzip](https://en.wikipedia.org/wiki/Gzip) on netstandard2.0 and add [deflate](https://en.wikipedia.org/wiki/Deflate) and [brotli](https://en.wikipedia.org/wiki/Brotli) on net6+.

## Compress and Decompress

We treat Compress and Decompress as [Transformer](/contents/MessageMappers.md#message-transformer-factory) middleware.

We provide a **WrapWithAttribute** of **Compress** that will use the **CompressPayloadTransformer** to compress the body of your message using your choice of compression algorithm.  The claim check has a threshold, over which messages will be compressed.

In the following example we compress any string larger than 150K

``` csharp
[Compress(0, CompressionMethod.GZip, CompressionLevel.Optimal, 150)]
public Message MapToMessage(GreetingEvent request)
{
	var header = new MessageHeader(messageId: request.Id, topic: typeof(GreetingEvent).FullName.ToValidSNSTopicName(), messageType: MessageType.MT_EVENT);
	var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
	var message = new Message(header, body);
	return message;
}
```

We provide a matching **UnwrapWithAttribute** of **Decompress** that will use the **CompressPayloadTransformer** to decompress the body of your message using the algorithm the message body was compressed with. If the string is not compressed, we take no action. This supports the scenario where some messages on a channel are small enough not to cross the threshold for compression, but others will be large and require compression. (If you want compress all messages on a channel, regardless of individual size, just set your threshold to zero).

In this example, we look for a GZip compressed string and if we find it, decompress the body.

```csharp
[Decompress(0, CompressionMethod.GZip)]
public GreetingEvent MapToRequest(Message message)
{
	var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
	
	return greetingCommand;
}
```


### Impact of Compression

When we compress a message we change the *Content Type* header (content-type) for the message to reflect the compressed type: "application/gzip" for GZip, "application/deflate" for Deflate and "application/br" for Brotli. We store the pre-compression content type, in the *Original Content Type* (originalContentType) header.

Compression produces binary content. Where middleware requires that we transmit the message as text (for example over HTTPs such as SNS) we use a base64 string to ensure that the translation to and from text does not corrupt the data. Because turning binary data into a base64 string inflates it, you may need to adjust for that. As an example, if the limit of the middleware is 256K, a string that compresses to more than 192K will breach your limit. This is particularly useful to note if your strategy is to compress a string, and then use a [Claim Check](ClaimCheck.md) to offload any payloads that remain too large. In the example case your claim check would need to be at 192K and not 256K.






