---
description: "When a message body outgrows what your transport will carry, a claim check stores the payload elsewhere and sends a token in its place."
layout:
  description:
    visible: false
---

# Put a Large Payload Behind a Claim Check

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Claim Check](/contents/ClaimCheck.md), [Message Mappers](/contents/MessageMappers.md)

When a message body outgrows what your transport will carry, a claim check stores the payload
elsewhere and sends a token in its place.

[Claim Check](/contents/ClaimCheck.md) explains the pattern and the two attributes that
implement it. This guide is the wiring: which store to pick, how to register it, where the
threshold is measured, and how to tell whether the payload really left.

**Registering the store is the step people miss.** `AddBrighter` finishes by registering a
`NullLuggageStore`, so a mapper carrying `[ClaimCheck]` with no store configured compiles,
starts, and throws `NotImplementedException` the first time it maps a message. Step 3 is that
registration.

## Step 1: Find Your Transport's Message Size Limit

The limit is your broker's, not Brighter's. **Brighter neither enforces a maximum nor reports
one** — an oversized message is rejected by the transport, at publish time, with whatever that
broker's client throws.

These are the published limits for the transports Brighter ships against. Check them against
your broker's own documentation and your own plan or tier, because several are configurable and
one of them changes with what you pay:

| Transport | Limit | Configurable? |
|---|---|---|
| AWS SQS and SNS | 256 KiB | No, for standard delivery |
| Azure Service Bus | 256 KB Standard, 100 MB Premium | By tier |
| Kafka | 1 MiB by default (`message.max.bytes`) | Yes, broker and topic |
| RabbitMQ | 128 MiB default cap since 3.8 (`max-message-size`) | Yes |
| Redis | 512 MB per value | Effectively no |
| PostgreSQL, MSSQL, MySQL | governed by the column type | Yes, by schema |

**One thing you cannot set through Brighter, despite appearances.** Azure Service Bus's
administration wrapper takes a `maxMessageSizeInKilobytes` argument on `CreateQueueAsync` and
`CreateTopicAsync` (`AzureServiceBusWrappers/AdministrationClientWrapper.cs:69`, `:140`), but
**nothing in the product ever supplies it** — there is no call site outside the wrapper and its
interface. Set the entity's maximum in Azure, not in your Brighter configuration.

**The size that counts is the serialized body.** The claim check compares
`message.Body.Memory.Length` — the bytes your mapper produced — against the threshold. Headers
are not included in that comparison but *are* carried by the broker, so a body sitting just
under a hard transport limit can still be rejected once its headers are added. Leave room.

## Step 2: Choose a Luggage Store

Seven implementations of `IAmAStorageProvider` and `IAmAStorageProviderAsync` ship with V10.
Every store implements both interfaces, which is what the registration in step 3 requires:

| Store | Package | Options type |
|---|---|---|
| `S3LuggageStore` | `Paramore.Brighter.Transformers.AWS` | `S3LuggageOptions(AWSS3Connection connection, string bucketName)` |
| `S3LuggageStore` | `Paramore.Brighter.Transformers.AWS.V4` | the same, built against AWS SDK v4 |
| `AzureBlobLuggageStore` | `Paramore.Brighter.Transformers.Azure` | `AzureBlobLuggageOptions` — `ContainerUri` and `Credential`, or `ConnectionString` and `ContainerName` |
| `GcsLuggageStore` | `Paramore.Brighter.Transformers.Gcp` | `GcsLuggageOptions` — `ProjectId`, `Bucket`, optional `Credential` |
| `MongoDbLuggageStore` | `Paramore.Brighter.Transformers.MongoGridFS` | `MongoDbLuggageStoreOptions(string connectionString, string database, string bucketName)` |
| `FileSystemStorageProvider` | `Paramore.Brighter` (core) | `FileSystemOptions(string path)` |
| `InMemoryStorageProvider` | `Paramore.Brighter` (core) | none — a default constructor |

**The two AWS packages are one store with two SDK generations**, not two features. Take
`Paramore.Brighter.Transformers.AWS.V4` if the rest of your application is on AWS SDK v4, and
the unsuffixed package otherwise. Referencing both puts two types called `S3LuggageStore` in
scope and you will be qualifying namespaces for the rest of the file.

**`InMemoryStorageProvider` is for tests**, and it is genuinely useful there — step 6 uses it.
It holds the luggage in the process that stored it, so a separate consumer process finds
nothing.

**There is also a `NullLuggageStore`, and you never register it deliberately.** It is what
`AddBrighter` leaves you with, and every one of its methods throws. See step 3.

**Every store shares one option**, from the `StorageOptions` base: `Strategy`, which is
`StorageStrategy.CreateIfMissing` by default and `StorageStrategy.Validate` if you would rather
the store be provisioned by your infrastructure and Brighter merely check that it is there.

## Step 3: Register the Luggage Store

`UseExternalLuggageStore<TStoreProvider>` extends `IBrighterBuilder`, so it chains off
`AddBrighter`. Three overloads, differing only in who constructs the store:

```csharp
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Transformers.AWS;
using Paramore.Brighter.Transforms.Storage;

// 1. Brighter constructs it — the store needs a public constructor the container can call
services.AddBrighter()
    .UseExternalLuggageStore<InMemoryStorageProvider>();

// 2. You construct it, and hand over the instance
services.AddBrighter()
    .UseExternalLuggageStore(new FileSystemStorageProvider(
        new FileSystemOptions("/var/brighter/luggage")));

// 3. A factory, when the store needs something from the container
services.AddBrighter()
    .UseExternalLuggageStore(provider => new S3LuggageStore(
        new S3LuggageOptions(
            new AWSS3Connection(awsCredentials, RegionEndpoint.EUWest1),
            bucketName: "my-brighter-luggage")
        {
            HttpClientFactory = provider.GetRequiredService<IHttpClientFactory>()
        }));
```

**Register after `AddBrighter`, and yours wins.** `AddBrighter` ends with
`UseExternalLuggageStore<NullLuggageStore>()` (`ServiceCollectionExtensions.cs:222-223`), so a
store is always registered. The overloads use `AddSingleton` rather than `TryAdd`, and
`GetRequiredService` resolves the **last** registration, so calling
`UseExternalLuggageStore` after `AddBrighter` displaces the null store. This is the opposite of
`AddBrighterDefault`'s "register yours first" rule — the two use different registration methods
and the order that works for one is the order that fails for the other.

**Skip this step and every method of the null store throws.** Resolving
`IAmAStorageProvider` from a container configured with `AddBrighter()` and nothing else gives
you:

```text
System.NotImplementedException: This is a null store, you must register a real store after Brighter
   at Paramore.Brighter.Transforms.Storage.NullLuggageStore.EnsureStoreExists()
```

**Note where that throw comes from — it is on resolution, not on threshold.** The registration
wraps your store in a factory that sets its `Tracer` and calls `EnsureStoreExists()` *before
handing it out*, so the failure lands the first time anything resolves the store, which is when
the transform pipeline for a `[ClaimCheck]` mapper is first built. Whatever size that first
message happens to be, a small one buys you no reprieve.

The same eager `EnsureStoreExists()` is what provisions a *real* store, so a missing bucket or
container surfaces at the same moment, under whatever `StorageOptions.Strategy` you chose.

## Step 4: Attach the Claim Check to Your Mapper

The claim check is transform middleware, so it attaches to a **message mapper** — one attribute
on the way out, one on the way back:

```csharp
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;

public class LargeOrderMessageMapper : IAmAMessageMapper<LargeOrderPlaced>
{
    public IRequestContext? Context { get; set; }

    [ClaimCheck(step: 0, thresholdInKb: 200)]
    public Message MapToMessage(LargeOrderPlaced request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic!,
            messageType: MessageType.MT_EVENT);

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options));

        return new Message(header, body);
    }

    [RetrieveClaim(step: 0, retain: false)]
    public LargeOrderPlaced MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<LargeOrderPlaced>(
            message.Body.Value, JsonSerialisationOptions.Options)!;
    }
}
```

**You need a mapper of your own to attach these to.** An attribute goes on a method of a type
you own, and you do not own `JsonMessageMapper<TRequest>` — so a claim check means writing the
mapper out, as above, rather than leaning on
[default message mappers](/contents/DefaultMessageMappers.md). That is the same constraint
[Message Transforms](/contents/MessageTransforms.md) states for any transform of your own.

**`retain: false` is the default and it deletes the luggage** once the receiver has read it.
Set `retain: true` when more than one consumer reads the same message, or the second reader
will find the claim check pointing at nothing.

**Register the mapper**, or none of this runs:

```csharp
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Transforms.Storage;

services.AddBrighter()
    .UseExternalLuggageStore<InMemoryStorageProvider>()
    .AutoFromAssemblies([typeof(LargeOrderMessageMapper).Assembly]);
```

## Step 5: Choose a Threshold

`thresholdInKb` is compared as `thresholdInKb * 1024` bytes against the serialized body, and the
comparison is `body.Length < threshold` — so a body **exactly** on the threshold is checked into
the store, not sent inline.

**`thresholdInKb: 0` checks every message**, because no body is shorter than zero bytes. That is
occasionally what you want — uniform behaviour is easier to reason about than a size-dependent
branch — but it means a round trip to your store for a 200-byte event.

Pick a threshold **below your transport's limit with room for headers**, not at it. On a
256 KiB transport, something like 200 KiB leaves the headers, the CloudEvents attributes and the
claim check itself somewhere to live.

**What the message looks like once it is checked**: the body is replaced with the literal text
`Claim Check {id}`, the id goes into the header bag under `claim_check_header`, and
`MessageHeader.DataRef` is set to the same id. `DataRef` is the CloudEvents
[`dataref`](https://github.com/cloudevents/spec/blob/v1.0.2/cloudevents/extensions/dataref.md)
extension, which is what makes the claim check readable by consumers that are not Brighter —
and it is a deliberate break from V9, which used the header bag alone. On the way back the
transformer reads the bag first and falls back to `DataRef`, so it understands both.

## Step 6: Verify the Payload Went to the Store

The visible symptom of a claim check that is not working is *nothing* — the message arrives, the
handler runs, and you never learn the body travelled inline. So assert on the store.

`HasClaimAsync` answers the question directly, and `InMemoryStorageProvider` makes it a test
rather than an integration:

```csharp
using System.Text.Json;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.Transforms.Storage;
using Paramore.Brighter.Transforms.Transformers;

var store = new InMemoryStorageProvider();
var transformer = new ClaimCheckTransformer(store, store);
transformer.InitializeWrapFromAttributeParams(5);   // 5Kb threshold

var big = new string('x', 10 * 1024);
var message = new Message(
    new MessageHeader(Id.Random(), new RoutingKey("large.order"), MessageType.MT_EVENT),
    new MessageBody(JsonSerializer.Serialize(big)));

var wrapped = await transformer.WrapAsync(message, new Publication());

// The body no longer carries the payload...
Assert.StartsWith("Claim Check", wrapped.Body.Value);

// ...and the store does.
var claim = (string)wrapped.Header.Bag[ClaimCheckTransformer.CLAIM_CHECK];
Assert.True(await store.HasClaimAsync(claim));
Assert.Equal(claim, wrapped.Header.DataRef);
```

Against a real store, the same three assertions work with the store swapped: the body starts
with `Claim Check`, `HasClaimAsync` returns `true`, and your bucket or container has an object
whose name is the claim. For S3 that object sits under the `LuggagePrefix`, which defaults to
`BRIGHTER_CHECKED_LUGGAGE`.

## Claim Check Failures

**`NotImplementedException: This is a null store…`** — no store registered. Step 3.

**`NotImplementedException` on the consumer only** — the store was registered in the producer's
service collection and not the consumer's. Both ends need it: one to check the luggage in, the
other to claim it.

**The consumer gets a body of `Claim Check <guid>`** — the mapper's `MapToRequest` has no
`[RetrieveClaim]`, or its step ordering puts it after something that already tried to
deserialize. The claim check unwraps at `step: 0` by convention because it has to run before
anything that reads the body.

**The second consumer of the same message finds nothing** — `retain` defaulted to `false` and
the first consumer deleted the luggage. Set `retain: true`, and take on the deletion yourself.

**`InMemoryStorageProvider` works in tests and not between processes** — it is a dictionary in
the process that wrote it. Use `FileSystemStorageProvider` for a local multi-process run, and a
real store beyond that.

**The payload never leaves, silently** — the body is under the threshold. Print
`message.Body.Memory.Length` and compare it with `thresholdInKb * 1024`, remembering the
comparison is on the serialized bytes rather than the size of your object.

## Further Reading

- [Claim Check](/contents/ClaimCheck.md) — the pattern, and the two attributes
- [S3 Luggage Store](/contents/S3LuggageStore.md) — the AWS store in detail
- [Message Transforms](/contents/MessageTransforms.md) — how transform middleware is composed
- [Message Mappers](/contents/MessageMappers.md) — writing the mapper the attributes attach to
- [Compression](/contents/Compression.md) — the other answer to a body that is too big
- [Cloud Events Support](/contents/CloudEventsSupport.md) — the `dataref` attribute the claim check sets
