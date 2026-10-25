// Task 6.4, MessageTransforms.md #8 (§ Chaining Multiple Transforms) and the sentence after it, as they stood at
// 6.3: "[Compress(step: 0)] // First: Compress, [ClaimCheck(step: 1)] // Second", "[RetrieveClaim(step: 0)] // First,
// [Decompress(step: 1)] // Second", and "The transforms execute in order based on the `step` parameter. On receive,
// they execute in reverse order." Run against the released Paramore.Brighter 10.8.1.
//  A. Recording transforms, wrap and unwrap at steps 0 and 1, read the order directly.
//     Case: A at step 0, B at step 1. Control: the steps swapped, which must swap the recorded order.
//  B. Brighter's own Compress and ClaimCheck through its TransformPipelineBuilder, an InMemoryStorageProvider as the
//     luggage store, threshold 512 KB.
//     B1 (send). A 600 KB body of one repeated character compresses far below 512 KB, so it is claim-checked only
//        if ClaimCheck runs before Compress. Case: the page's mapper as repaired (Compress 1, ClaimCheck 0).
//        Control: the page's mapper as it stood (Compress 0, ClaimCheck 1).
//     B2 (receive). A 1.2 MB body of random hex is still over 512 KB once compressed, so the luggage stored is
//        compressed, and the receive must retrieve it before it decompresses. Case: as repaired (RetrieveClaim 1,
//        Decompress 0). Control: as it stood (RetrieveClaim 0, Decompress 1).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Transforms.Attributes;
using Paramore.Brighter.Transforms.Storage;
using Paramore.Brighter.Transforms.Transformers;

public class LargeOrder() : Event(Id.Random()) { public string Payload { get; set; } = ""; }   // probe: the reader's own type

// --- B: the page's block 8 as repaired, verbatim ---
public class CompressedClaimCheckMapper : IAmAMessageMapper<LargeOrder>
{
    public IRequestContext? Context { get; set; }

    [Compress(step: 1, compressionMethod: CompressionMethod.GZip)]  // First: Compress
    [ClaimCheck(step: 0, thresholdInKb: 512)]                       // Second: Claim check if still large
    public Message MapToMessage(LargeOrder request, Publication publication)
    {
        // Standard serialization
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic!,                                // probe: the page's nullable warning
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    [RetrieveClaim(step: 1)]                      // First: Retrieve if claim checked
    [Decompress(step: 0)]                         // Second: Decompress
    public LargeOrder MapToRequest(Message message)
    {
        var order = JsonSerializer.Deserialize<LargeOrder>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return order!;
    }
}

// probe: the page's block 8 as it stood at 6.3, its attributes only; the mapping is the same
public class AsItStoodMapper : IAmAMessageMapper<LargeOrder>
{
    public IRequestContext? Context { get; set; }
    [Compress(step: 0, compressionMethod: CompressionMethod.GZip)]  // First: Compress
    [ClaimCheck(step: 1, thresholdInKb: 512)]                       // Second: Claim check if still large
    public Message MapToMessage(LargeOrder request, Publication publication) =>
        new CompressedClaimCheckMapper().MapToMessage(request, publication);
    [RetrieveClaim(step: 0)]                      // First: Retrieve if claim checked
    [Decompress(step: 1)]                         // Second: Decompress
    public LargeOrder MapToRequest(Message message) => new CompressedClaimCheckMapper().MapToRequest(message);
}

// probe: B2's control, the repaired send with the receive as it stood
public class ReceiveAsItStoodMapper : IAmAMessageMapper<LargeOrder>
{
    public IRequestContext? Context { get; set; }
    [Compress(step: 1, compressionMethod: CompressionMethod.GZip)]
    [ClaimCheck(step: 0, thresholdInKb: 512)]
    public Message MapToMessage(LargeOrder request, Publication publication) =>
        new CompressedClaimCheckMapper().MapToMessage(request, publication);
    [RetrieveClaim(step: 0)]
    [Decompress(step: 1)]
    public LargeOrder MapToRequest(Message message) => new CompressedClaimCheckMapper().MapToRequest(message);
}

// --- A: recording transforms ---
public static class Log { public static readonly List<string> Calls = new(); }
public abstract class Recorder(string name) : IAmAMessageTransform, IAmAMessageTransformAsync
{
    public IRequestContext? Context { get; set; }
    public void Dispose() { }
    public void InitializeWrapFromAttributeParams(params object?[] initializerList) { }
    public void InitializeUnwrapFromAttributeParams(params object?[] initializerList) { }
    public Message Wrap(Message message, Publication publication) { Log.Calls.Add(name); return message; }
    public Message Unwrap(Message message) { Log.Calls.Add(name); return message; }
    public Task<Message> WrapAsync(Message m, Publication p, CancellationToken ct = default) => Task.FromResult(Wrap(m, p));
    public Task<Message> UnwrapAsync(Message m, CancellationToken ct = default) => Task.FromResult(Unwrap(m));
}
public class RA() : Recorder("A"); public class RB() : Recorder("B");
public class WrapA(int step) : WrapWithAttribute(step) { public override Type GetHandlerType() => typeof(RA); }
public class WrapB(int step) : WrapWithAttribute(step) { public override Type GetHandlerType() => typeof(RB); }
public class UnwrapA(int step) : UnwrapWithAttribute(step) { public override Type GetHandlerType() => typeof(RA); }
public class UnwrapB(int step) : UnwrapWithAttribute(step) { public override Type GetHandlerType() => typeof(RB); }

public class Ping() : Event(Id.Random());
public class Ping2() : Event(Id.Random());
public class AFirst : IAmAMessageMapper<Ping>
{
    public IRequestContext? Context { get; set; }
    [WrapA(0)] [WrapB(1)]
    public Message MapToMessage(Ping r, Publication p) => new(new MessageHeader(r.Id, p.Topic!, MessageType.MT_EVENT), new MessageBody("{}"));
    [UnwrapA(0)] [UnwrapB(1)]
    public Ping MapToRequest(Message m) => new();
}
public class BFirst : IAmAMessageMapper<Ping2>
{
    public IRequestContext? Context { get; set; }
    [WrapA(1)] [WrapB(0)]
    public Message MapToMessage(Ping2 r, Publication p) => new(new MessageHeader(r.Id, p.Topic!, MessageType.MT_EVENT), new MessageBody("{}"));
    [UnwrapA(1)] [UnwrapB(0)]
    public Ping2 MapToRequest(Message m) => new();
}

public static class Program
{
    static bool failed;
    static void Check(string row, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {row}: {detail}"); }

    static TransformPipelineBuilder Builder<TReq, TMapper>(InMemoryStorageProvider store)
        where TReq : class, IRequest where TMapper : class, IAmAMessageMapper<TReq>, new()
    {
        TransformPipelineBuilder.ClearPipelineCache();
        var registry = new MessageMapperRegistry(new SimpleMessageMapperFactory(_ => new TMapper()), null);
        registry.Register<TReq, TMapper>();
        var transforms = new SimpleMessageTransformerFactory(t =>
            t == typeof(ClaimCheckTransformer) ? new ClaimCheckTransformer(store, store)
            : t == typeof(CompressPayloadTransformer) ? new CompressPayloadTransformer()
            : (IAmAMessageTransform)Activator.CreateInstance(t)!);
        return new TransformPipelineBuilder(registry, transforms);
    }

    static (string send, string receive) Record<TReq, TMapper>(TReq request)
        where TReq : class, IRequest where TMapper : class, IAmAMessageMapper<TReq>, new()
    {
        var b = Builder<TReq, TMapper>(new InMemoryStorageProvider());
        Log.Calls.Clear();
        var message = b.BuildWrapPipeline<TReq>().Wrap(request, new RequestContext(), new Publication { Topic = new RoutingKey("t") });
        var send = string.Join(",", Log.Calls);
        Log.Calls.Clear();
        b.BuildUnwrapPipeline<TReq>().Unwrap(message, new RequestContext());
        return (send, string.Join(",", Log.Calls));
    }

    static (bool claimChecked, bool intact, string outcome) RoundTrip<TMapper>(string payload) where TMapper : class, IAmAMessageMapper<LargeOrder>, new()
    {
        var store = new InMemoryStorageProvider();
        var b = Builder<LargeOrder, TMapper>(store);
        var order = new LargeOrder { Payload = payload };
        var message = b.BuildWrapPipeline<LargeOrder>().Wrap(order, new RequestContext(), new Publication { Topic = new RoutingKey("orders") });
        var claimChecked = !string.IsNullOrEmpty(message.Header.DataRef);
        var sent = $"sent {message.Body.Memory.Length} bytes, claim-checked {claimChecked}";
        try
        {
            var back = b.BuildUnwrapPipeline<LargeOrder>().Unwrap(message, new RequestContext());
            return (claimChecked, back.Payload == payload, $"{sent}; received, payload intact {back.Payload == payload}");
        }
        catch (Exception e)
        {
            return (claimChecked, false, $"{sent}; receive throws {e.GetType().Name}: {e.Message.Split('\n')[0]}");
        }
    }

    public static int Main()
    {
        var a = Record<Ping, AFirst>(new Ping());
        Console.WriteLine($"A case    (A step 0, B step 1): send {a.send}; receive {a.receive}");
        var c = Record<Ping2, BFirst>(new Ping2());
        Console.WriteLine($"A control (A step 1, B step 0): send {c.send}; receive {c.receive}");
        Check("send runs from the highest step to the lowest", a.send == "B,A" && c.send == "A,B", $"{a.send} / {c.send}");
        Check("receive runs from the highest step to the lowest, as send does", a.receive == "B,A" && c.receive == "A,B", $"{a.receive} / {c.receive}");

        var compressible = new string('x', 600 * 1024);
        var bytes = new byte[600 * 1024]; new Random(42).NextBytes(bytes);
        var incompressible = Convert.ToHexString(bytes);   // 1.2 MB of text that gzip takes to about half, still over 512 KB

        var b1 = RoundTrip<CompressedClaimCheckMapper>(compressible);
        var b1c = RoundTrip<AsItStoodMapper>(compressible);
        Console.WriteLine($"B1 case,    600 KB compressible, as repaired: {b1.outcome}");
        Console.WriteLine($"B1 control, 600 KB compressible, as it stood: {b1c.outcome}");
        Check("B1: as repaired, Compress runs before ClaimCheck", !b1.claimChecked && b1.intact && b1c.claimChecked,
            $"claim-checked {b1.claimChecked} / {b1c.claimChecked}");

        var b2 = RoundTrip<CompressedClaimCheckMapper>(incompressible);
        var b2c = RoundTrip<ReceiveAsItStoodMapper>(incompressible);
        Console.WriteLine($"B2 case,    1.2 MB random hex, as repaired: {b2.outcome}");
        Console.WriteLine($"B2 control, 1.2 MB random hex, receive as it stood: {b2c.outcome}");
        Check("B2: as repaired, RetrieveClaim runs before Decompress", b2.claimChecked && b2.intact && !b2c.intact,
            $"intact {b2.intact} / {b2c.intact}");
        return failed ? 1 : 0;
    }
}
