// EFCoreOutbox.md, against 10.8.0 and MySQL 8:
//   docker run -d --name probe-mysql -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=greetings -p 3307:3306 mysql:8
//
// Claim 1: the handler can take the IAmATransactionConnectionProvider "that Brighter registers for the
//   TransactionProvider set above", and starting the transaction through it "begins it on the
//   context's database". Reported: what resolves, and the context's CurrentTransaction once it has.
// Claim 2: "EF Core's SaveChangesAsync and the Outbox write share it", so committing writes both and
//   rolling back writes neither.
//   case:    the page's handler - the greeting is saved, the Outbox holds the message, ClearOutboxAsync
//            puts it on the bus
//   control: the page's handler with a failure planted after SaveChangesAsync, before the commit -
//            it catches and rolls back: no greeting, no message
//   control: as the last, but the message deposited without the provider - the greeting rolls back
//            and the message stays. Without it, the control above cannot tell one transaction from
//            two writes that both happened to fail
// The registration is the page's, with the probe's connection string, an explicit server version in
// place of ServerVersion.AutoDetect, the bus an InMemory producer, and UseOutboxSweeper() left out.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MySql;
using Paramore.Brighter.MySql.EntityFrameworkCore;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Outbox.MySql;

const string connectionString = "Server=localhost;Port=3307;Database=greetings;Uid=root;Pwd=root;";
static string DbConnectionString() => connectionString;

var services = new ServiceCollection();
services.AddLogging();
var bus = new InternalBus();

// the page's ConfigureServices
var outboxConfiguration = new RelationalDatabaseConfiguration(DbConnectionString(), outBoxTableName: "Outbox");
services.AddSingleton<IAmARelationalDatabaseConfiguration>(outboxConfiguration);
services.AddDbContext<GreetingsEntityGateway>(options =>
    options.UseMySql(DbConnectionString(), new MySqlServerVersion(new Version(8, 0))));  // probe: no AutoDetect

services.AddBrighter()
    .AddProducers(producers =>
    {
        producers.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,              // probe
            [new Publication { Topic = new RoutingKey("greeting.made"), RequestType = typeof(GreetingMade) }],
            InstrumentationOptions.None).Create();
        producers.Outbox = new MySqlOutbox(outboxConfiguration);
        producers.ConnectionProvider = typeof(MySqlConnectionProvider);
        // Use the EF Core transaction provider with your DbContext
        producers.TransactionProvider = typeof(MySqlEntityFrameworkTransactionProvider<GreetingsEntityGateway>);
    })
    .AutoFromAssemblies();
var sp = services.BuildServiceProvider();

// schema: the context's tables and Brighter's Outbox
using (var scope = sp.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GreetingsEntityGateway>();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    await db.Database.ExecuteSqlRawAsync(MySqlOutboxBuilder.GetDDL("Outbox"));
}

var failed = 0;

// Claim 1
using (var scope = sp.CreateScope())
{
    var provider = scope.ServiceProvider.GetService<IAmATransactionConnectionProvider>();
    var uow = scope.ServiceProvider.GetRequiredService<GreetingsEntityGateway>();
    var resolved = provider?.GetType().Name ?? "NO";
    var shared = false;
    if (provider != null)
    {
        await provider.GetTransactionAsync();
        shared = uow.Database.CurrentTransaction != null;
        await provider.RollbackAsync();
        provider.Close();
    }
    var ok = provider is MySqlEntityFrameworkTransactionProvider<GreetingsEntityGateway> && shared;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} claim 1  IAmATransactionConnectionProvider resolves: {resolved}; the context has a transaction after GetTransactionAsync: {shared}");
    failed += ok ? 0 : 1;
}

// Claim 2
failed += await Run("claim 2 case     the page's handler", Plant.None, expect: (1, 1, 1));
failed += await Run("claim 2 control  failure before the commit", Plant.FailBeforeCommit, expect: (0, 0, 0));
failed += await Run("claim 2 control  the same, deposited without the provider", Plant.FailBeforeCommitDepositOutside, expect: (0, 1, 0));

using (var scope = sp.CreateScope()) await scope.ServiceProvider.GetRequiredService<GreetingsEntityGateway>().Database.EnsureDeletedAsync();
return failed == 0 ? 0 : 1;

async Task<int> Run(string name, Plant plant, (int Greetings, int Outbox, int Bus) expect)
{
    using (var scope = sp.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GreetingsEntityGateway>();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Greeting; DELETE FROM Person; DELETE FROM Outbox;");
        db.People.Add(new Person("Ian"));
        await db.SaveChangesAsync();
    }
    var onBusBefore = bus.Stream(new RoutingKey("greeting.made")).Count();
    using (var scope = sp.CreateScope())
        await scope.ServiceProvider.GetRequiredService<IAmACommandProcessor>().SendAsync(new AddGreeting("Ian", "Hello") { Plant = plant });

    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    async Task<int> Count(string table)
    {
        await using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {table}", connection);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
    var measured = (await Count("Greeting"), await Count("Outbox"), bus.Stream(new RoutingKey("greeting.made")).Count() - onBusBefore);
    var ok = measured == expect;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: greetings {measured.Item1}, Outbox {measured.Item2}, on the bus {measured.Item3}");
    return ok ? 0 : 1;
}

public enum Plant { None, FailBeforeCommit, FailBeforeCommitDepositOutside }

public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
    public Plant Plant { get; init; }
}

public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}

public class Person(string name)
{
    public int Id { get; set; }
    public string Name { get; set; } = name;
    public ICollection<Greeting> Greetings { get; set; } = new List<Greeting>();

    public void AddGreeting(Greeting greeting)
    {
        greeting.Recipient = this;
        Greetings.Add(greeting);
    }
}

public class Greeting(string message)
{
    public long Id { get; set; }
    public string Message { get; set; } = message;
    public int RecipientId { get; set; }
    public Person Recipient { get; set; } = null!;

    public string Greet() => $"{Message} {Recipient.Name}!";
}

public class GreetingsEntityGateway(DbContextOptions<GreetingsEntityGateway> options) : DbContext(options)
{
    public DbSet<Person> People { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Greeting>().ToTable("Greeting")
            .HasOne(g => g.Recipient).WithMany(p => p.Greetings).HasForeignKey(g => g.RecipientId);
    }
}

// verbatim from the page, but for the probe's plants
public class AddGreetingHandlerAsync(
    GreetingsEntityGateway uow,
    IAmATransactionConnectionProvider provider,
    IAmACommandProcessor postBox)
    : RequestHandlerAsync<AddGreeting>
{
    public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();

        await provider.GetTransactionAsync(cancellationToken);
        try
        {
            var person = await uow.People
                .Where(p => p.Name == addGreeting.Name)
                .SingleAsync(cancellationToken);

            var greeting = new Greeting(addGreeting.Greeting);

            person.AddGreeting(greeting);

            //Now write the message we want to send to the Db in the same transaction.
            if (addGreeting.Plant == Plant.FailBeforeCommitDepositOutside)                            // probe
                posts.Add(await postBox.DepositPostAsync(new GreetingMade(greeting.Greet()), cancellationToken: cancellationToken));
            else
            posts.Add(await postBox.DepositPostAsync(
                new GreetingMade(greeting.Greet()),
                provider,
                cancellationToken: cancellationToken));

            //write the changed entity to the Db
            await uow.SaveChangesAsync(cancellationToken);

            if (addGreeting.Plant != Plant.None) throw new InvalidOperationException("planted failure");   // probe

            //write new person and the associated message to the Db
            await provider.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            //it went wrong, rollback the entity change and the downstream message
            await provider.RollbackAsync(cancellationToken);
            return await base.HandleAsync(addGreeting, cancellationToken);
        }
        finally
        {
            provider.Close();
        }

        //Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
        //Alternatively, you can let the Sweeper do this, but at the cost of increased latency
        await postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
