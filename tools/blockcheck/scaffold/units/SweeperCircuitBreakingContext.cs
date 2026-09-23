// Values SweeperCircuitBreaking.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static SweeperCircuitBreakingContext;

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class SweeperCircuitBreakingContext
{
    public static IAmACommandProcessor commandProcessor => null!;
    public static IEnumerable<Id> messageIds => null!;
    public static CancellationToken cancellationToken => default;
    public static IServiceCollection services => null!;
    public static RelationalDatabaseConfiguration outboxConfiguration => null!;
}
