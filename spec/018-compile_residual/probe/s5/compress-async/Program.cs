// Task 4.5a (D19), Compression.md § Decompress. Claim: at 10.7.0 WrapAsync sets the Content Type
// to "application/gzip; charset=utf-8", Decompress compares the whole Content Type with
// "application/gzip", so the body reaches the mapper still compressed; the sync Wrap round-trips.
// Case: 10.8.0. Control: 10.7.0. Within each run, the sync Wrap is the second control.
// Each of GZip, Zlib and Brotli: wrap (async or sync), then unwrap through the transformer.
using System;
using System.IO.Compression;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.Transforms.Transformers;

var json = "{\"Name\":\"" + new string('x', 4096) + "\"}";
var failed = false;
foreach (var method in new[] { CompressionMethod.GZip, CompressionMethod.Zlib, CompressionMethod.Brotli })
foreach (var path in new[] { "async", "sync" })
{
    var message = new Message(
        new MessageHeader(Id.Random(), new RoutingKey("greeting"), MessageType.MT_EVENT,
            contentType: new ContentType("application/json") { CharSet = "utf-8" }),
        new MessageBody(json));

    var wrap = new CompressPayloadTransformer();
    wrap.InitializeWrapFromAttributeParams(method, CompressionLevel.Optimal, 0);
    var wrapped = path == "async"
        ? await wrap.WrapAsync(message, new Publication())
        : wrap.Wrap(message, new Publication());
    var wrappedType = wrapped.Header.ContentType?.ToString();

    var unwrap = new CompressPayloadTransformer();
    unwrap.InitializeUnwrapFromAttributeParams(method);
    var unwrapped = path == "async" ? await unwrap.UnwrapAsync(wrapped) : unwrap.Unwrap(wrapped);

    var roundTrips = unwrapped.Body.Value == json;
    failed |= path == "sync" && !roundTrips;   // the sync path is expected to round-trip at both versions
    Console.WriteLine($"{typeof(Message).Assembly.GetName().Version} {method,-6} {path,-5} wrapped as '{wrappedType}': {(roundTrips ? "decompressed" : "STILL COMPRESSED")}");
}
return failed ? 1 : 0;
