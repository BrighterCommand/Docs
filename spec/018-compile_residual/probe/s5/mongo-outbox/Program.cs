// R5: does MongoDBOutbox.md's handler work, and does the rewrite?
//
// Needs a single-node replica set, since MongoDB transactions need one:
//   docker run -d --name probe-mongo -p 27017:27017 mongo:7 --replSet rs0 --bind_ip_all
//   docker exec probe-mongo mongosh --eval 'rs.initiate({_id:"rs0",members:[{_id:0,host:"localhost:27017"}]})'
//
// Case A, the page as it was: the handler takes IAmATransactionConnectionProvider and casts it to
//   MongoDbUnitOfWork. Expected: no such registration, or a null cast.
// Case B, the form R5 first proposed: IAmAMongoDbTransactionProvider passed to DepositPostAsync.
//   C is its control. Both fail the same way, which is the finding: that interface is an
//   IAmABoxTransactionProvider<IClientSession>, and Brighter configures IClientSessionHandle.
// Case B', the page now: the provider used as IAmABoxTransactionProvider<IClientSessionHandle>.
//   Expected: the person carries the greeting, the Outbox holds one message, and ClearOutboxAsync
//   puts it on the bus.
// Case C', the control for B': the same handler with a failure planted after the deposit. The page
//   catches, rolls back and returns. Expected: neither the greeting nor the message is written.
//   Without it, B' cannot tell a shared transaction from two independent writes.
// Also reported: what DI resolves, and the transaction type Brighter takes from MongoDbUnitOfWork.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Outbox.MongoDb;

const string connectionString = "mongodb://localhost:27017/?replicaSet=rs0&directConnection=true";
var databaseName = "probe_" + Guid.NewGuid().ToString("N")[..8];
var configuration = new MongoDbConfiguration(connectionString, databaseName)
{
    Outbox = new MongoDbCollectionConfiguration { Name = "Outbox", MakeCollection = OnResolvingACollection.CreateIfNotExists }
};
var db = configuration.Client.GetDatabase(databaseName);
await db.CreateCollectionAsync("People");
await db.CreateCollectionAsync("Outbox");

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IAmAMongoDbConfiguration>(configuration);
services.AddSingleton(new Probe());
var bus = new InternalBus();
services.AddBrighter()
    .AddProducers(producers =>
    {
        producers.Outbox = new MongoDbOutbox(configuration);
        producers.ConnectionProvider = typeof(MongoDbConnectionProvider);
        producers.TransactionProvider = typeof(MongoDbUnitOfWork);
        producers.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
            [new Publication { Topic = new RoutingKey("greeting.made"), RequestType = typeof(GreetingMade) }],
            InstrumentationOptions.None).Create();
    })
    .AutoFromAssemblies();
var sp = services.BuildServiceProvider();

var boxProvider = sp.GetRequiredService<IAmABoxTransactionProvider>();
Console.WriteLine($"registered IAmABoxTransactionProvider: {boxProvider.GetType().Name}");
Console.WriteLine($"transaction type Brighter takes from it: {CommandProcessor.GetTransactionTypeFromTransactionProvider(boxProvider).Name}");
Console.WriteLine($"IAmAMongoDbTransactionProvider resolves: {sp.GetService<IAmAMongoDbTransactionProvider>()?.GetType().Name ?? "NO"}");
Console.WriteLine($"IAmATransactionConnectionProvider resolves: {sp.GetService<IAmATransactionConnectionProvider>()?.GetType().Name ?? "NO"}");

async Task Run(string name, IRequest command, Func<IAmACommandProcessor, Task> send)
{
    var people = db.GetCollection<Person>("People");
    await people.DeleteManyAsync(FilterDefinition<Person>.Empty);
    await people.InsertOneAsync(new Person { Name = "Ian" });
    await db.GetCollection<BsonDocument>("Outbox").DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
    var onBusBefore = bus.Stream(new RoutingKey("greeting.made")).Count();
    string outcome;
    try
    {
        using var scope = sp.CreateScope();
        await send(scope.ServiceProvider.GetRequiredService<IAmACommandProcessor>());
        outcome = "returned";
    }
    catch (Exception e)
    {
        var inner = e;
        while (inner.InnerException != null) inner = inner.InnerException;
        outcome = $"threw {e.GetType().Name} <- {inner.GetType().Name}: {inner.Message.Split('\n')[0]}";
    }
    var greetings = (await people.Find(p => p.Name == "Ian").FirstAsync()).Greetings.Count;
    var outbox = await db.GetCollection<BsonDocument>("Outbox").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
    var dispatched = bus.Stream(new RoutingKey("greeting.made")).Count() - onBusBefore;
    Console.WriteLine($"{name}: {outcome}; greetings on Ian = {greetings}; Outbox messages = {outbox}; on the bus = {dispatched}");
}

await Run("A  page as it was (cast IAmATransactionConnectionProvider)", null!, cp => cp.SendAsync(new AddGreetingCast("Ian", "Hello")));
await Run("B  R5's first form, IAmAMongoDbTransactionProvider", null!, cp => cp.SendAsync(new AddGreeting("Ian", "Hello")));
await Run("C  control, B failing after the deposit", null!, cp => cp.SendAsync(new AddGreeting("Ian", "Hello") { FailAfterDeposit = true }));
await Run("B' the page now, the provider as IAmABoxTransactionProvider<IClientSessionHandle>", null!, cp => cp.SendAsync(new AddGreetingHandle("Ian", "Hello")));
await Run("C' control, B' failing after the deposit", null!, cp => cp.SendAsync(new AddGreetingHandle("Ian", "Hello") { FailAfterDeposit = true }));

await configuration.Client.DropDatabaseAsync(databaseName);

public class Probe;

public class Person
{
    [BsonId] public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Greetings { get; set; } = new();
}

public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
    public bool FailAfterDeposit { get; init; }
}
public class AddGreetingCast(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
}
public class AddGreetingHandle(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
    public bool FailAfterDeposit { get; init; }
}

public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}

// B and C: the form R5 first proposed
public class AddGreetingHandler(IAmAMongoDbTransactionProvider transactionProvider, IAmACommandProcessor postBox, IAmAMongoDbConfiguration configuration)
    : RequestHandlerAsync<AddGreeting>
{
    public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();
        // Through IAmAMongoDbTransactionProvider the session is an IClientSession; the driver's
        // collection methods take the IClientSessionHandle it is
        var session = (IClientSessionHandle)await transactionProvider.GetTransactionAsync(cancellationToken);
        try
        {
            var people = transactionProvider.Client.GetDatabase(configuration.DatabaseName).GetCollection<Person>("People");
            await people.UpdateOneAsync(session,
                Builders<Person>.Filter.Eq(p => p.Name, addGreeting.Name),
                Builders<Person>.Update.Push(p => p.Greetings, addGreeting.Greeting),
                cancellationToken: cancellationToken);

            posts.Add(await postBox.DepositPostAsync(new GreetingMade(addGreeting.Greeting), transactionProvider, cancellationToken: cancellationToken));

            if (addGreeting.FailAfterDeposit) throw new InvalidOperationException("planted failure after the deposit");

            await transactionProvider.CommitAsync(cancellationToken);
        }
        catch
        {
            await transactionProvider.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            transactionProvider.Close();
        }
        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}

// B' and C': MongoDBOutbox.md's handler as the page now shows it, with the planted failure for C'
public class AddGreetingHandleHandler(
    IAmAMongoDbTransactionProvider transactionProvider,
    IAmAMongoDbConfiguration configuration,
    IAmACommandProcessor postBox,
    ILogger<AddGreetingHandleHandler> logger) : RequestHandlerAsync<AddGreetingHandle>
{
    public override async Task<AddGreetingHandle> HandleAsync(AddGreetingHandle addGreeting, CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();

        // The MongoDB Outbox writes in an IClientSessionHandle, so use the provider as one
        var unitOfWork = (IAmABoxTransactionProvider<IClientSessionHandle>)transactionProvider;
        var session = await unitOfWork.GetTransactionAsync(cancellationToken);

        try
        {
            var people = transactionProvider.Client
                .GetDatabase(configuration.DatabaseName)
                .GetCollection<Person>("People");

            // Update the person document within the transaction
            await people.UpdateOneAsync(session,
                Builders<Person>.Filter.Eq(p => p.Name, addGreeting.Name),
                Builders<Person>.Update.Push(p => p.Greetings, addGreeting.Greeting),
                cancellationToken: cancellationToken);

            // Now write the message we want to send to the Outbox in the same transaction
            posts.Add(await postBox.DepositPostAsync(
                new GreetingMade(addGreeting.Greeting),
                unitOfWork,
                cancellationToken: cancellationToken));

            // probe only, C': fail after the deposit
            if (addGreeting.FailAfterDeposit) throw new InvalidOperationException("planted failure after the deposit");

            // Commit both the entity change and the outgoing message
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception thrown handling Add Greeting request");
            // It went wrong, rollback the entity change and the downstream message
            await unitOfWork.RollbackAsync(cancellationToken);
            return await base.HandleAsync(addGreeting, cancellationToken);
        }
        finally
        {
            unitOfWork.Close();
        }

        // Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
        // Alternatively, you can let the Sweeper do this, but at the cost of increased latency
        await postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}

// A: the page's form
public class AddGreetingCastHandler(IAmATransactionConnectionProvider connectionProvider) : RequestHandlerAsync<AddGreetingCast>
{
    public override async Task<AddGreetingCast> HandleAsync(AddGreetingCast command, CancellationToken cancellationToken = default)
    {
        var unitOfWork = connectionProvider as MongoDbUnitOfWork;
        Console.WriteLine($"   A: cast gives {(unitOfWork == null ? "null" : "a MongoDbUnitOfWork")}");
        return await base.HandleAsync(command, cancellationToken);
    }
}
