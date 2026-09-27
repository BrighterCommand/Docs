// Values the four relational Outbox pages name in their blocks and never declare: MSSQLOutbox.md,
// MySQLOutbox.md, PostgresOutbox.md and SqliteOutbox.md, which repeat one set of blocks per backend.
//
// Every member is typed from a pinned package or the BCL, returns a default, and does nothing. A
// block that calls a member of one of these is checked against the real type, so a wrong member
// or argument still fails.
//
// blockcheck: using static RelationalOutboxContext;

using Microsoft.Extensions.DependencyInjection;

public static class RelationalOutboxContext
{
    // block 2: `services.AddSingleton<IAmARelationalDatabaseConfiguration>(dbConfig)` — the composition root's
    public static IServiceCollection services => null!;
}
