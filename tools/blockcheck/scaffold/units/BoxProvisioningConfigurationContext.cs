// Values BoxProvisioningConfiguration.md names in its blocks and never declares.
//
// Every member is typed from a pinned package or the BCL, returns a default, and does nothing. A
// block that calls a member of one of these is checked against the real type, so a wrong member
// or argument still fails.
//
// blockcheck: using static BoxProvisioningConfigurationContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.BoxProvisioning;

public static class BoxProvisioningConfigurationContext
{
    // block 1: `builder.Configuration.GetConnectionString("BrighterDb")` — the composition root's
    public static WebApplicationBuilder builder => null!;
    // blocks 1-4: `services.AddBrighter()`
    public static IServiceCollection services => null!;
    // block 2: `connectionString: connectionString` — block 1's `var connectionString`
    public static string connectionString => string.Empty;
    // block 4: `opts.AddMsSqlOutbox(outboxConfig)` — block 2's `var outboxConfig`
    public static RelationalDatabaseConfiguration outboxConfig => null!;
    // block 4: `opts.AddMsSqlInbox(inboxConfig)` — block 2's `var inboxConfig`
    public static RelationalDatabaseConfiguration inboxConfig => null!;
    // blocks 5-9: `opts.Add{Backend}Outbox(…)` — the `UseBoxProvisioning(opts => …)` delegate's parameter
    public static BoxProvisioningOptions opts => null!;
    // blocks 5-9: `opts.Add{Backend}Outbox(rdbmsConfiguration)` — the page's "same configuration object"
    public static RelationalDatabaseConfiguration rdbmsConfiguration => null!;
}
