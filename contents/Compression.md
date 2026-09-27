---
description: "The Compression transform helps us reduce the size of a message using a compression algorithm."
layout:
  description:
    visible: false
---

# Compression

> **Explanation** · Applies to **Brighter V10**

The Compression transform helps us reduce the size of a message using a compression algorithm. It is an efficient approach to reducing the size of a payload. 

`CompressionMethod.GZip` ([gzip](https://en.wikipedia.org/wiki/Gzip)) works on every target Brighter ships for. `CompressionMethod.Zlib` ([zlib](https://en.wikipedia.org/wiki/Zlib), a [deflate](https://en.wikipedia.org/wiki/Deflate) stream) and `CompressionMethod.Brotli` ([brotli](https://en.wikipedia.org/wiki/Brotli)) need .NET 8 or later; on netstandard2.0 they throw `ArgumentException`.

## Compress and Decompress

We treat Compress and Decompress as [Transformer](/contents/MessageTransforms.md#message-transformer-factory) middleware.

We provide a **WrapWithAttribute** of **Compress** that will use the **CompressPayloadTransformer** to compress the body of your message using your choice of compression algorithm. **Compress** has a threshold in Kb: a body smaller than it is sent uncompressed.

In the following example we compress any body of 150Kb or more

```csharp
using System.IO.Compression;
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;
using Paramore.Brighter.Transforms.Transformers;

[Compress(0, CompressionMethod.GZip, CompressionLevel.Optimal, 150)]
public Message MapToMessage(GreetingEvent request, Publication publication)
{
	var header = new MessageHeader(messageId: request.Id, topic: publication.Topic!, messageType: MessageType.MT_EVENT);
	var body = new MessageBody(JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));
	var message = new Message(header, body);
	return message;
}
```

We provide a matching **UnwrapWithAttribute** of **Decompress** that will use the **CompressPayloadTransformer** to decompress the body of your message using the algorithm the message body was compressed with. If the string is not compressed, we take no action. This supports the scenario where some messages on a channel are small enough not to cross the threshold for compression, but others will be large and require compression. (If you want to compress all messages on a channel, regardless of individual size, set your threshold to zero.)

In this example, we look for a GZip compressed body and if we find it, decompress the body.

**At 10.7.0, Decompress does not recognise a body compressed on the async path.** `WrapAsync` — which `PostAsync` and `DepositPostAsync` use — sets the *Content Type* to `application/gzip; charset=utf-8`, and **Decompress** compares the whole *Content Type* with `application/gzip`, so the message reaches your mapper still compressed. The same holds for Zlib and Brotli. A body compressed by the synchronous `Post` path round-trips. This is [BrighterCommand/Brighter#4432](https://github.com/BrighterCommand/Brighter/issues/4432).

```csharp
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;
using Paramore.Brighter.Transforms.Transformers;

[Decompress(0, CompressionMethod.GZip)]
public GreetingEvent MapToRequest(Message message)
{
	var greetingCommand = JsonSerializer.Deserialize<GreetingEvent>(message.Body.Value, JsonSerialisationOptions.Options);
	
	return greetingCommand;
}
```


### Impact of Compression

When we compress a message we change the *Content Type* header (content-type) for the message to reflect the compressed type: "application/gzip" for GZip, "application/deflate" for Zlib and "application/br" for Brotli. We store the pre-compression content type, in the *Original Content Type* (originalContentType) header.

Compression produces binary content. Where middleware requires that we transmit the message as text (for example over HTTPs such as SNS) we use a base64 string to ensure that the translation to and from text does not corrupt the data. Because turning binary data into a base64 string inflates it, you may need to adjust for that. As an example, if the limit of the middleware is 256K, a string that compresses to more than 192K will breach your limit. This is particularly useful to note if your strategy is to compress a string, and then use a [Claim Check](ClaimCheck.md) to offload any payloads that remain too large. In the example case your claim check would need to be at 192K and not 256K.






