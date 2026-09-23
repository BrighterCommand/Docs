// Values MigratingToPollyV8.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static MigratingToPollyV8Context;

using Paramore.Brighter;
using Polly.Registry;

public static class MigratingToPollyV8Context
{
    public static HandlerConfiguration handlerConfiguration => null!;
    public static PolicyRegistry policyRegistry => null!;
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
}
