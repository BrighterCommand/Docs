// MongoDBOutbox.md: which transaction provider DepositPostAsync accepts. Run twice:
//   dotnet run -p:BrighterVersion=10.8.1   (the case)
//   dotnet run -p:BrighterVersion=10.8.0   (the control: the page's old claim)
// Needs MongoDB as a replica set: docker run -d --rm --name mongo-probe -p 27018:27017 mongo:7 --replSet rs0
//   docker exec mongo-probe mongosh --eval 'rs.initiate()'
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Paramore.Brighter;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Outbox.MongoDb;

var version = typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
    .OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion;
Console.WriteLine($"Brighter {version}");

var client = new MongoClient("mongodb://localhost:27018/?directConnection=true");
await client.DropDatabaseAsync("ProbeDb");
var config = new MongoDbConfiguration(client, "ProbeDb")
{
    Outbox = new MongoDbCollectionConfiguration { Name = "Outbox", MakeCollection = OnResolvingACollection.Validate }
};
// Collections must exist before a multi-document transaction writes to them.
await client.GetDatabase("ProbeDb").CreateCollectionAsync("People");
await client.GetDatabase("ProbeDb").CreateCollectionAsync("Outbox");

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IAmAMongoDbConfiguration>(config);
services.AddBrighter()
    .AddProducers(p =>
    {
        p.Outbox = new MongoDbOutbox(config);
        p.ConnectionProvider = typeof(MongoDbConnectionProvider);
        p.TransactionProvider = typeof(MongoDbUnitOfWork);
        p.ProducerRegistry = new InMemoryProducerRegistryFactory(new InternalBus(),
            [new Publication { Topic = new RoutingKey("greeting"), RequestType = typeof(Greeting) }],
            InstrumentationOptions.None).Create();
    });
using var sp = services.BuildServiceProvider();
var postBox = sp.GetRequiredService<IAmACommandProcessor>();

async Task<string> Case(string label, bool cast)
{
    using var scope = sp.CreateScope();
    var provider = scope.ServiceProvider.GetRequiredService<IAmAMongoDbTransactionProvider>();
    var asHandle = (IAmABoxTransactionProvider<IClientSessionHandle>)provider;
    try
    {
        var session = await ((IAmABoxTransactionProvider<IClientSessionHandle>)provider).GetTransactionAsync();
        await client.GetDatabase("ProbeDb").GetCollection<MongoDB.Bson.BsonDocument>("People")
            .InsertOneAsync(session, new MongoDB.Bson.BsonDocument("Name", label));
        var id = cast
            ? await postBox.DepositPostAsync(new Greeting(), asHandle)
            : await postBox.DepositPostAsync(new Greeting(), provider);
        await provider.CommitAsync(CancellationToken.None);
        return $"ok, deposited {id}";
    }
    catch (Exception e)
    {
        try { await provider.RollbackAsync(CancellationToken.None); } catch { }
        return $"{e.GetType().Name}: {e.Message}";
    }
    finally { provider.Close(); }
}

Console.WriteLine($"pass IAmAMongoDbTransactionProvider directly : {await Case("direct", false)}");
Console.WriteLine($"pass as IAmABoxTransactionProvider<IClientSessionHandle>: {await Case("cast", true)}");
Console.WriteLine($"Outbox documents: {await client.GetDatabase("ProbeDb").GetCollection<MongoDB.Bson.BsonDocument>("Outbox").CountDocumentsAsync(FilterDefinition<MongoDB.Bson.BsonDocument>.Empty)}, People documents: {await client.GetDatabase("ProbeDb").GetCollection<MongoDB.Bson.BsonDocument>("People").CountDocumentsAsync(FilterDefinition<MongoDB.Bson.BsonDocument>.Empty)}");

public class Greeting() : Event(Id.Random());
