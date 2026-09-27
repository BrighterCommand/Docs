// Types and values MSSQLTransportInboxAndOutbox.md and PostgreSQLTransportAndOutbox.md name
// in their blocks and never declare.
//
// Both pages publish a `GreetingEvent` and handle an `AddGreeting` from the Brighter samples they
// link to, and never show either. Every value member is typed from a pinned package or the BCL,
// returns a default, and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static RelationalTransportContext;

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Postgres;

public static class RelationalTransportContext
{
    public static WebApplicationBuilder builder => null!;
    public static RelationalDatabaseConfiguration configuration => null!;
    public static IEnumerable<Subscription> subscriptions => null!;
    // PostgreSQLTransportAndOutbox.md blocks 4, 5: `PostgresProducerRegistryFactory(connection, …)`,
    // `new PostgresChannelFactory(connection)` — step 3's `var connection`
    public static PostgresMessagingGatewayConnection connection => null!;
}

// MSSQL blocks 5, 6, 8, 9 and PostgreSQL blocks 4, 5, 6: publications, subscriptions and handlers
// of `GreetingEvent`; the handlers' `new GreetingEvent(addGreeting.Greeting)`
public class GreetingEvent(string greeting) : Event(Id.Random());

// MSSQL block 9 and PostgreSQL block 6: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Greeting`
public class AddGreeting() : Command(Id.Random())
{
    public string Greeting { get; } = string.Empty;
}
