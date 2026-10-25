// Values RabbitMQConnectionStability.md names in its blocks and never shows.
//
// Block 3 adds logging to the `services` collection the reader's host owns; the value is typed
// from a pinned package and returns a default.
//
// blockcheck: using static RabbitMQConnectionStabilityContext;

using Microsoft.Extensions.DependencyInjection;

public static class RabbitMQConnectionStabilityContext
{
    // block 3: `services.AddLogging(…)`
    public static IServiceCollection services => null!;
}
