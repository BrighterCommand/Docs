// Task 7.5, MongoDBInbox.md #4 and MongoDBOutbox.md #4. Each block's configuration is copied as the page shows it; only
// the connection string (the probe's container) and the database name (one per run) are the probe's.
//  Index: does a TTL index exist after a message is stored through the page's configuration? Case: #4 as shown, which
//         leaves MakeCollection at its default (Assume). Control: the same with MakeCollection = CreateIfNotExists.
//  Delete: is a stored message removed once its TimeToLive has passed? TimeToLive 2 s, the TTL monitor waking every
//         second, 10 s waited. Case and control as above.
//  Instrument: a raw document whose TimeStamp is a BSON Date, written into the control's collection, must be removed
//         in the same wait; without it, "not deleted" cannot be told from a TTL monitor that never ran.
//  Lock: MongoDbDistributedLock.md, "a TTL index expires stale lock documents automatically, which protects you if an
//         instance crashes before releasing its lock". A lock obtained and never released, TimeToLive 2 s, CreateIfNotExists
//         (the best case, since the index then exists): after 10 s, can a second instance obtain it? Control: the lock
//         released, then obtained again.
//  Workaround: a TTL index the reader creates on TimeStamp.DateTime (Brighter stores a DateTimeOffset as an embedded
//         document of DateTime, Ticks and Offset, and MongoDB expires only BSON Dates). Case: the index, on the default
//         (Assume) collection, a message stored, 10 s waited. Control: the case rows above, which have no such index.
// Exits 1 if any row is FAIL; a row is FAIL when the result differs from what the page said before 7.5. At 10.8.1 every
// case row is FAIL (ledger 93), and the workaround rows are what the pages now show.
using System;
using System.Linq;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.MongoDb;
using Paramore.Brighter.Locking.MongoDb;
using Paramore.Brighter.MongoDb;
using Paramore.Brighter.Outbox.MongoDb;

public class GreetingCommand() : Command(Id.Random()) { public string Greeting { get; set; } = "hello"; }   // probe

public static class Program
{
    const string Server = "mongodb://localhost:27019";   // probe
    static bool failed;
    static void Row(string name, bool asPageSays, string detail)
    { failed |= !asPageSays; Console.WriteLine($"{(asPageSays ? "ok  " : "FAIL")} {name}: {detail}"); }

    static int run;
    static (IMongoDatabase db, string collection) Store(bool outbox, TimeSpan ttl, OnResolvingACollection? make)
    {
        var dbName = $"probe_ttl_{Environment.ProcessId}_{++run}";   // probe
        var collection = new MongoDbCollectionConfiguration
        {
            Name = outbox ? "Outbox" : "Inbox",
            TimeToLive = ttl // Messages will be automatically deleted after 30 days
        };
        if (make is { } m) collection.MakeCollection = m;   // probe: the control
        var mongoDbConfiguration = new MongoDbConfiguration(Server, dbName);
        if (outbox) mongoDbConfiguration.Outbox = collection; else mongoDbConfiguration.Inbox = collection;

        if (outbox)
        {
            var store = new MongoDbOutbox(mongoDbConfiguration);
            var message = new Message(new MessageHeader(Id.Random(), new RoutingKey("greeting"), MessageType.MT_EVENT),
                new MessageBody("{\"Greeting\":\"hello\"}"));
            store.Add(message, new RequestContext());
        }
        else
        {
            var store = new MongoDbInbox(mongoDbConfiguration);
            store.Add(new GreetingCommand(), "GreetingHandler", new RequestContext());
        }
        return (mongoDbConfiguration.Client.GetDatabase(dbName), collection.Name);
    }

    static string Ttl(IMongoDatabase db, string name) =>
        string.Join(", ", db.GetCollection<BsonDocument>(name).Indexes.List().ToList()
            .Where(i => i.Contains("expireAfterSeconds"))
            .Select(i => $"{i["name"]} on {string.Join(",", i["key"].AsBsonDocument.Names)}, expireAfterSeconds {i["expireAfterSeconds"]}"));

    static string TimeStampType(IMongoDatabase db, string name) =>
        db.GetCollection<BsonDocument>(name).Find(new BsonDocument("TimeStamp", new BsonDocument("$exists", true)))
            .FirstOrDefault()?["TimeStamp"].BsonType.ToString() ?? "(no document)";

    public static int Main()
    {
        foreach (var outbox in new[] { true, false })
        {
            var page = outbox ? "MongoDBOutbox.md #4" : "MongoDBInbox.md #4";

            // Index, case: the page's block, 30 days, MakeCollection left at its default
            var (db, name) = Store(outbox, TimeSpan.FromDays(30), null);
            var ttl = Ttl(db, name);
            Row($"{page} index, as shown (MakeCollection {new MongoDbCollectionConfiguration().MakeCollection})",
                ttl.Length > 0, ttl.Length > 0 ? ttl : "no TTL index on the collection");
            // Index, control
            (db, name) = Store(outbox, TimeSpan.FromDays(30), OnResolvingACollection.CreateIfNotExists);
            ttl = Ttl(db, name);
            Row($"{page} index, control (CreateIfNotExists)", ttl.Length > 0, ttl.Length > 0 ? ttl : "no TTL index");
            Console.WriteLine($"     TimeStamp stored as BSON {TimeStampType(db, name)}");

            // Delete: a 2 s TimeToLive, case and control, plus the instrument document in the control's collection
            var (caseDb, caseName) = Store(outbox, TimeSpan.FromSeconds(2), null);
            var (ctlDb, ctlName) = Store(outbox, TimeSpan.FromSeconds(2), OnResolvingACollection.CreateIfNotExists);
            var ctl = ctlDb.GetCollection<BsonDocument>(ctlName);
            ctl.InsertOne(new BsonDocument { { "_id", "instrument" }, { "TimeStamp", DateTime.UtcNow.AddHours(-1) } });
            Thread.Sleep(TimeSpan.FromSeconds(10));
            var caseLeft = caseDb.GetCollection<BsonDocument>(caseName).CountDocuments(FilterDefinition<BsonDocument>.Empty);
            var ctlLeft = ctl.CountDocuments(new BsonDocument("_id", new BsonDocument("$ne", "instrument")));
            var instrumentLeft = ctl.CountDocuments(new BsonDocument("_id", "instrument"));
            Row($"{page} delete, as shown", caseLeft == 0, $"{caseLeft} of 1 message left 10 s after a 2 s TimeToLive");
            Row($"{page} delete, control (CreateIfNotExists)", ctlLeft == 0, $"{ctlLeft} of 1 message left");
            Console.WriteLine($"     instrument (a BSON Date TimeStamp, same collection): {(instrumentLeft == 0 ? "removed, so the TTL monitor ran" : "NOT removed: the wait proves nothing")}");
            if (instrumentLeft != 0) failed = true;
        }
        // Workaround, for the Outbox, the Inbox and the lock
        foreach (var kind in new[] { "Outbox", "Inbox" })
        {
            var (wdb, wname) = Store(kind == "Outbox", TimeSpan.FromSeconds(2), null);
            wdb.GetCollection<BsonDocument>(wname).Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("TimeStamp.DateTime"),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(2) }));
            Thread.Sleep(TimeSpan.FromSeconds(10));
            var left = wdb.GetCollection<BsonDocument>(wname).CountDocuments(FilterDefinition<BsonDocument>.Empty);
            Console.WriteLine($"     workaround, {kind}: a TTL index on TimeStamp.DateTime leaves {left} of 1 message after 10 s");
        }

        // Lock: the page's configuration (Name, TimeToLive), CreateIfNotExists so the TTL index is made
        var lockConfiguration = new MongoDbConfiguration(Server, $"probe_ttl_{Environment.ProcessId}_lock")
        {
            Locking = new MongoDbCollectionConfiguration
            {
                Name = "brighter_locks",
                TimeToLive = TimeSpan.FromSeconds(2),
                MakeCollection = OnResolvingACollection.CreateIfNotExists   // probe
            }
        };
        var crashed = new MongoDbLockingProvider(lockConfiguration);
        var first = crashed.ObtainLockAsync("sweeper", CancellationToken.None).Result;
        Console.WriteLine($"     lock index: {Ttl(lockConfiguration.Client.GetDatabase(lockConfiguration.DatabaseName), "brighter_locks")}");
        Thread.Sleep(TimeSpan.FromSeconds(10));
        var second = new MongoDbLockingProvider(lockConfiguration).ObtainLockAsync("sweeper", CancellationToken.None).Result;
        Row("MongoDbDistributedLock.md, a crashed holder's lock expires", first != null && second != null,
            $"first obtain {first ?? "null"}; 10 s after a 2 s TimeToLive, a second instance gets {second ?? "null (still held)"}");
        // Lock control: released, then obtained
        crashed.ReleaseLockAsync("sweeper", "sweeper", CancellationToken.None).Wait();
        var third = new MongoDbLockingProvider(lockConfiguration).ObtainLockAsync("sweeper", CancellationToken.None).Result;
        Console.WriteLine($"     control, released then obtained: {third ?? "null"}");
        var lockDb = lockConfiguration.Client.GetDatabase(lockConfiguration.DatabaseName);
        lockDb.GetCollection<BsonDocument>("brighter_locks").Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending("TimeStamp.DateTime"),
            new CreateIndexOptions { ExpireAfter = TimeSpan.FromSeconds(2) }));
        Thread.Sleep(TimeSpan.FromSeconds(10));
        var fourth = new MongoDbLockingProvider(lockConfiguration).ObtainLockAsync("sweeper", CancellationToken.None).Result;
        Console.WriteLine($"     workaround, lock: with a TTL index on TimeStamp.DateTime, the unreleased lock is obtained again: {fourth ?? "null (still held)"}");
        return failed ? 1 : 0;
    }
}
