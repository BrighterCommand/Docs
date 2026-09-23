// Values MSSQLTransportInboxAndOutbox.md and PostgreSQLTransportAndOutbox.md name
// in their blocks and never declare.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static RelationalTransportContext;

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Paramore.Brighter;

public static class RelationalTransportContext
{
    public static WebApplicationBuilder builder => null!;
    public static RelationalDatabaseConfiguration configuration => null!;
    public static IEnumerable<Subscription> subscriptions => null!;
}
