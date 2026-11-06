// Types and values BrighterBasicConfiguration.md names in its blocks and never declares.
//
// The page configures Brighter for the reader's own Greetings application: a command, an event, an
// Entity Framework context and a policy registry it does not show. Every value member is typed
// from a pinned package or the BCL and returns a default; a stub is a request, a context or a
// registry with only the members a block names.
//
// blockcheck: using static BrighterBasicConfigurationContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Polly.Registry;

public static class BrighterBasicConfigurationContext
{
    // block 1: `builder.Services`
    public static WebApplicationBuilder builder => null!;
    // blocks 2, 5: `new RelationalDatabaseConfiguration(DbConnectionString(), …)`
    public static string DbConnectionString() => "Server=localhost;Database=Greetings;";
    // block 2: `options.PolicyRegistry = policyRegistry;`
    public static PolicyRegistry policyRegistry => null!;
}

// block 1: `typeof(GreetingCommand).Assembly`
public class GreetingCommand() : Command(Id.Random());

// blocks 2, 5: `typeof(GreetingMade)`, `RmqSubscription<GreetingMade>`
public class GreetingMade() : Event(Id.Random());

// block 2: `AddDbContext<GreetingsEntityGateway>`, `MySqlEntityFrameworkTransactionProvider<GreetingsEntityGateway>`
public class GreetingsEntityGateway(DbContextOptions<GreetingsEntityGateway> options) : DbContext(options);

// block 5: `options.PolicyRegistry = new SalutationPolicy();`
public class SalutationPolicy : PolicyRegistry;
