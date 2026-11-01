// Values OutboxArchiver.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static OutboxArchiverContext;

using Microsoft.Extensions.DependencyInjection;

public static class OutboxArchiverContext
{
    // block 3: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// block 4: `IAmAMessageMapper<LargeDataCommand>`
public class LargeDataCommand() : Paramore.Brighter.Command(Paramore.Brighter.Id.Random());
