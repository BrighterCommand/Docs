// Types and values KafkaConfiguration.md names in its blocks and never declares.
//
// Every subscription and mapper on the page is for the reader's own `GreetingEvent`, which the
// page never shows, and block 19 registers the schema registry client against the `services`
// collection the reader's host owns. The stub is a request and nothing more: the mapper reads
// `request.Id`, which `Event` supplies, and no block names a member of its own. The value is typed
// from a pinned package and returns a default.
//
// blockcheck: using static KafkaConfigurationContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class KafkaConfigurationContext
{
    // block 19: `services.AddSingleton<ISchemaRegistryClient>(cachedSchemaRegistryClient)`
    public static IServiceCollection services => null!;
}

// blocks 14–17, 20: `new KafkaSubscription<GreetingEvent>(…)`, `IAmAMessageMapper<GreetingEvent>`
public class GreetingEvent() : Event(Id.Random());
