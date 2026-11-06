// Task 6.4, against the released 10.8.1 packages.
// DefaultMessageMappers.md
//  #8, #12: AutoFromAssemblies with no default named -> JsonMessageMapper<T>, binary mode (the attributes in the
//     header, the body the request's JSON). #3, #9: defaultMessageMapper: typeof(CloudEventJsonMessageMapper<>) ->
//     that mapper, structured mode (the envelope in the body). #15: typeof(JsonMessageMapper<>) named -> the same as
//     #8. Each read from the registry Brighter builds, ResolveMapperInfo(typeof(OrderCreated)), and the resolved mapper
//     run. Control: an explicit mapper registered for the request wins over either default.
// CloudEventsSupport.md
//  #1: the Publication's Source, Type and Subject reach the message's header (the default mapper). Control: a
//     Publication without them - Brighter's defaults.
//  #13-#15: the same for the good examples, a relative Source among them (ledger 74), and Subject.
//  #6: LargeOrderMapper, verbatim, through TransformPipelineBuilder with an InMemoryStorageProvider: over 256 KB the
//     payload is stored and the header's DataRef set, and the receive restores it. Control: 100 KB - neither.
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.MessageMappers;
using Paramore.Brighter.Transforms.Attributes;
using Paramore.Brighter.Transforms.Storage;
using Paramore.Brighter.Transforms.Transformers;

public class OrderCreated() : Event(Id.Random()) { public string CustomerId { get; set; } = "12345"; public decimal Total { get; set; } = 99.99m; }   // probe
public class LargeOrder() : Event(Id.Random()) { public string Payload { get; set; } = ""; }                                                          // probe
public class ExplicitMapper<T> : JsonMessageMapper<T> where T : class, IRequest;                                                              // probe: the control's explicit mapper, open so the scan skips it

// CloudEventsSupport.md #6, verbatim
public class LargeOrderMapper : IAmAMessageMapper<LargeOrder>
{
    public IRequestContext? Context { get; set; }

    [ClaimCheck(step: 0, thresholdInKb: 256)]
    public Message MapToMessage(LargeOrder request, Publication publication)
    {
        // If the message exceeds 256KB, Brighter will:
        // 1. Store the payload in a luggage store (e.g., S3)
        // 2. Set the CloudEvents dataref attribute to the storage location
        // 3. Send a lightweight message with just the reference

        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic!,                                  // probe: nullable
            messageType: MessageType.MT_EVENT
        );

        var body = new MessageBody(
            JsonSerializer.Serialize(request, JsonSerialisationOptions.Options)
        );

        return new Message(header, body);
    }

    [RetrieveClaim(step: 0)]
    public LargeOrder MapToRequest(Message message)
    {
        // Brighter automatically retrieves the payload using the dataref
        var order = JsonSerializer.Deserialize<LargeOrder>(
            message.Body.Value,
            JsonSerialisationOptions.Options
        );

        return order!;
    }
}

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static (string mapper, bool isDefault, string mode) Resolve(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        var provider = services.BuildServiceProvider();
        var registry = ServiceCollectionExtensions.MessageMapperRegistry(provider);   // the registry AddBrighter builds for the Command Processor
        var (type, isDefault) = registry.ResolveMapperInfo(typeof(OrderCreated));
        var lease = registry.Get<OrderCreated>();
        var message = lease!.Instance.MapToMessage(new OrderCreated(), new Publication { Topic = new RoutingKey("orders"), Source = new Uri("https://example.com/orders"), Type = new CloudEventsType("com.example.order.created") });
        var body = message.Body.Value;
        var mode = body.Contains("\"specversion\"") && body.Contains("https://example.com/orders") ? "structured (envelope in the body)"
            : !body.Contains("specversion") && message.Header.Source.ToString() == "https://example.com/orders" ? "binary (attributes in the header)" : "neither";
        return (type is null ? "none" : Name(type), isDefault, mode);
    }

    static string Name(Type t) => t.IsGenericType ? $"{t.Name.Split('`')[0]}<{string.Join(",", t.GetGenericArguments().Select(a => a.Name))}>" : t.Name;

    static Message Map(Publication publication) => new JsonMessageMapper<OrderCreated>().MapToMessage(new OrderCreated(), publication);

    public static int Main()
    {
        // ---- DefaultMessageMappers.md ----
        var r8 = Resolve(services =>
            // #8, verbatim
            services.AddBrighter(options => { })
                .AddProducers(configure => { })
                .AutoFromAssemblies([typeof(OrderCreated).Assembly]));
        Row("DMM #8   no default named", r8 == ("JsonMessageMapper<OrderCreated>", true, "binary (attributes in the header)"), $"{r8}");
        var r12 = Resolve(services =>
            // #12, verbatim
            services.AddBrighter(options => { })
                .AutoFromAssemblies([typeof(OrderCreated).Assembly]));
        Row("DMM #12  no producers, no default named", r12 == ("JsonMessageMapper<OrderCreated>", true, "binary (attributes in the header)"), $"{r12}");
        var r9 = Resolve(services =>
            // #3 and #9, verbatim
            services.AddBrighter(options => { })
                .AddProducers(configure => { })
                .AutoFromAssemblies(
                    [typeof(OrderCreated).Assembly],
                    defaultMessageMapper: typeof(CloudEventJsonMessageMapper<>),
                    asyncDefaultMessageMapper: typeof(CloudEventJsonMessageMapper<>)
                ));
        Row("DMM #3/#9 CloudEventJsonMessageMapper<> named", r9 == ("CloudEventJsonMessageMapper<OrderCreated>", true, "structured (envelope in the body)"), $"{r9}");
        var r15 = Resolve(services =>
            // #15, verbatim but for the one assembly the probe has
            services.AddBrighter(options => { })
                .AutoFromAssemblies(
                    [typeof(OrderCreated).Assembly],
                    defaultMessageMapper: typeof(JsonMessageMapper<>)
                ));
        Row("DMM #15  JsonMessageMapper<> named", r15 == ("JsonMessageMapper<OrderCreated>", true, "binary (attributes in the header)"), $"{r15}");
        var rc = Resolve(services =>
            services.AddBrighter(options => { })
                .AutoFromAssemblies([typeof(OrderCreated).Assembly], defaultMessageMapper: typeof(CloudEventJsonMessageMapper<>))
                .MapperRegistry(mappers => mappers.Register<OrderCreated, ExplicitMapper<OrderCreated>>()));
        Row("DMM control explicit mapper beside a default", rc.mapper == "ExplicitMapper<OrderCreated>" && !rc.isDefault, $"{rc}");

        // ---- CloudEventsSupport.md ----
        // #1, the publication verbatim (an RmqPublication is a Publication)
        var publication = new Publication
        {
            Topic = new RoutingKey("com.example.orders"),
            RequestType = typeof(OrderCreated),

            // CloudEvents attributes
            Source = new Uri("https://example.com/orders"),
            Type = new CloudEventsType("com.example.order.created"),
            Subject = "order/12345"
        };
        var h1 = Map(publication).Header;
        var h0 = Map(new Publication { Topic = new RoutingKey("com.example.orders") }).Header;
        Row("CES #1   Source, Type, Subject reach the header", h1.Source.ToString() == "https://example.com/orders" && h1.Type.Value == "com.example.order.created" && h1.Subject == "order/12345",
            $"{h1.Source}, {h1.Type.Value}, {h1.Subject} / none set: {h0.Source}, {h0.Type.Value}, {h0.Subject ?? "null"}");

        var t13 = Map(new Publication { Topic = new RoutingKey("orders"), Type = new CloudEventsType("com.example.orders.order.created") }).Header;   // #13's specific, Topic the probe's
        Row("CES #13  Type", t13.Type.Value == "com.example.orders.order.created", t13.Type.Value);
        var s14 = Map(new Publication { Topic = new RoutingKey("orders"), Source = new Uri("/orders/12345", UriKind.Relative) }).Header;           // #14's resource, Topic the probe's
        var s14a = Map(new Publication { Topic = new RoutingKey("orders"), Source = new Uri("https://prod.example.com/orders-service") }).Header;
        var structured = new CloudEventJsonMessageMapper<OrderCreated>().MapToMessage(new OrderCreated(), new Publication { Topic = new RoutingKey("orders"), Source = new Uri("/orders/12345", UriKind.Relative) }).Body.Value;
        Row("CES #14  a relative Source reaches the header and the envelope", s14.Source.ToString() == "/orders/12345" && !s14.Source.IsAbsoluteUri && structured.Contains("\"source\":\"/orders/12345\""),
            $"{s14.Source} (absolute {s14.Source.IsAbsoluteUri}) / service: {s14a.Source}; envelope has \"source\":\"/orders/12345\" {structured.Contains("\"source\":\"/orders/12345\"")}");
        var s15 = Map(new Publication { Topic = new RoutingKey("customers"), Source = new Uri("https://example.com/customers"), Subject = "customer/67890/profile" }).Header;   // #15's customer
        Row("CES #15  Subject", s15.Subject == "customer/67890/profile" && h0.Subject is null, $"{s15.Subject} / none set: {h0.Subject ?? "null"}");

        // #6
        string ClaimCheck(int kb)
        {
            TransformPipelineBuilder.ClearPipelineCache();
            var store = new InMemoryStorageProvider();
            var registry = new MessageMapperRegistry(new SimpleMessageMapperFactory(_ => new LargeOrderMapper()), null);
            registry.Register<LargeOrder, LargeOrderMapper>();
            var builder = new TransformPipelineBuilder(registry, new SimpleMessageTransformerFactory(_ => new ClaimCheckTransformer(store, store)));
            var order = new LargeOrder { Payload = new string('x', kb * 1024) };
            var message = builder.BuildWrapPipeline<LargeOrder>().Wrap(order, new RequestContext(), new Publication { Topic = new RoutingKey("orders") });
            var dataRef = message.Header.DataRef;
            var stored = dataRef is not null && store.HasClaim(dataRef);
            var sent = message.Body.Memory.Length;
            var back = builder.BuildUnwrapPipeline<LargeOrder>().Unwrap(message, new RequestContext());
            return $"dataref {(dataRef is null ? "null" : "set")}, stored {stored}, {sent} bytes sent, restored {back.Payload == order.Payload}";
        }
        var c300 = ClaimCheck(300);
        var c100 = ClaimCheck(100);
        Row("CES #6   over 256 KB: stored, dataref set, restored", c300 == $"dataref set, stored True, {c300.Split(", ")[2]}, restored True" && c100.StartsWith("dataref null, stored False"),
            $"300 KB: {c300} / 100 KB: {c100}");
        return failed ? 1 : 0;
    }
}
