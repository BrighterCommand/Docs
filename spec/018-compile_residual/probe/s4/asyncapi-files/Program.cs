// Task 6.4, AsyncAPISupport.md #2 and #10, and § Output Formats: GenerateAsyncApiDocumentAsync("asyncapi.json")
// returns the document and "writes both asyncapi.json and asyncapi.yaml"; "if you pass a path without a .json
// extension it is appended"; "If UseAsyncApi() was never called, GenerateAsyncApiDocumentAsync() throws an
// InvalidOperationException". The host is #1's shape on the in-memory transport, one subscription, in a scratch
// directory.
// Case: #10's call. Controls: a path without .json; a host without UseAsyncApi().
// #8 and #9, "When UseAsyncApi() finds an existing IAmASchemaGenerator registration, it does not override it with the
// default NJsonSchema implementation". Case: #9's registration of #8's MySchemaGenerator before UseAsyncApi() - the
// generator resolved is MySchemaGenerator, and the document's message payload carries no NJsonSchema properties.
// Control: no registration - the default generator, whose payload names the event's Amount property.
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi.v3;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.AsyncAPI;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public class PaymentReceivedEvent() : Event(Id.Random()) { public decimal Amount { get; set; } }   // probe

// #8, verbatim
public class MySchemaGenerator : IAmASchemaGenerator
{
    public Task<V3SchemaDefinition?> GenerateAsync(Type? requestType, CancellationToken ct = default)
    {
        // Return a V3SchemaDefinition with your schema, or null for an empty object schema
        return Task.FromResult<V3SchemaDefinition?>(null);
    }
}

public static class Program
{
    static IHost Host(bool useAsyncApi, bool ownGenerator = false)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        if (ownGenerator) builder.Services.AddSingleton<IAmASchemaGenerator, MySchemaGenerator>();   // #9, before UseAsyncApi()
        var brighter = builder.Services.AddConsumers(options =>
        {
            options.Subscriptions = [new InMemorySubscription<PaymentReceivedEvent>(new SubscriptionName("paramore.payment"),
                new ChannelName("payment.received"), new RoutingKey("payment.received"), messagePumpType: MessagePumpType.Reactor)];
            options.DefaultChannelFactory = new InMemoryChannelFactory(new InternalBus(), TimeProvider.System);
        });
        if (useAsyncApi)
            brighter.UseAsyncApi(opts =>
            {
                opts.Title = "My Service";
                opts.Version = "1.0.0";
                opts.Description = "Order processing service messaging contracts";
            });
        return builder.Build();
    }

    public static async Task<int> Main()
    {
        var dir = Directory.CreateTempSubdirectory("asyncapi-probe").FullName;
        Directory.SetCurrentDirectory(dir);
        var failed = false;
        void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }
        string Files() => string.Join(", ", Directory.GetFiles(dir).Select(Path.GetFileName).OrderBy(f => f));

        var host = Host(useAsyncApi: true);
        // #10, verbatim
        var document = await host.GenerateAsyncApiDocumentAsync("asyncapi.json");
        Console.WriteLine($"Generated: {document.Info.Title} v{document.Info.Version} (JSON + YAML)");
        var json = File.ReadAllText("asyncapi.json");
        var yaml = File.Exists("asyncapi.yaml") ? File.ReadAllText("asyncapi.yaml") : "";
        Row("#10 case    JSON and YAML written, the same document", Files() == "asyncapi.json, asyncapi.yaml" && json.Contains("My Service") && yaml.Contains("My Service") && yaml.Contains("payment.received") && json.Contains("payment.received"),
            $"{Files()}; title in both {json.Contains("My Service") && yaml.Contains("My Service")}; JSON {json.Length} bytes, YAML {yaml.Length} bytes");

        foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
        await Host(useAsyncApi: true).GenerateAsyncApiDocumentAsync("contracts");
        Row("control     a path without .json", Files() == "contracts.json, contracts.yaml", Files());

        try { await Host(useAsyncApi: false).GenerateAsyncApiDocumentAsync("asyncapi.json"); Row("control     no UseAsyncApi()", false, "no exception"); }
        catch (Exception e) { Row("control     no UseAsyncApi()", e is InvalidOperationException, $"{e.GetType().Name}: {e.Message.Split('\n')[0]}"); }
        foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
        var own = Host(useAsyncApi: true, ownGenerator: true);
        await own.GenerateAsyncApiDocumentAsync("own.json");
        var defaultHost = Host(useAsyncApi: true);
        await defaultHost.GenerateAsyncApiDocumentAsync("default.json");
        var ownGen = own.Services.GetRequiredService<IAmASchemaGenerator>().GetType().Name;
        var defGen = defaultHost.Services.GetRequiredService<IAmASchemaGenerator>().GetType().Name;
        var ownAmount = File.ReadAllText("own.json").Contains("\"amount\"", StringComparison.OrdinalIgnoreCase);
        var defAmount = File.ReadAllText("default.json").Contains("\"amount\"", StringComparison.OrdinalIgnoreCase);
        Row("#9 case     registered first, not overridden", ownGen == "MySchemaGenerator" && !ownAmount && defGen != "MySchemaGenerator" && defAmount,
            $"{ownGen}, payload names Amount {ownAmount} / no registration: {defGen}, payload names Amount {defAmount}");
        return failed ? 1 : 0;
    }
}
