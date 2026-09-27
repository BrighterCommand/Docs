// Values the six distributed-lock provider pages name in their blocks and never declare:
// AzureBlobDistributedLock.md, FirestoreDistributedLock.md, MongoDbDistributedLock.md,
// MsSqlDistributedLock.md, MySqlDistributedLock.md and PostgresDistributedLock.md, which repeat
// one registration block per backend.
//
// Every member is typed from a pinned package or the BCL, returns a default, and does nothing. A
// block that calls a member of one of these is checked against the real type, so a wrong member
// or argument still fails.
//
// blockcheck: using static DistributedLockProviderContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class DistributedLockProviderContext
{
    // block 2: `services.AddBrighter()` — the composition root's
    public static IServiceCollection services => null!;
    // block 2: `opt.Outbox = outbox` — the reader's own Outbox, configured on its page
    public static IAmAnOutbox outbox => null!;
}
