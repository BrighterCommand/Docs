// Types and values DispatcherConfigurationReference.md names in its blocks and never declares.
//
// The page configures a Dispatcher for the reader's own Greetings application: the event its
// subscription is for, and the helper that reads the Inbox's connection string from configuration.
// Every value member returns a default; a stub is a request or a helper with only the members a
// block names.
//
// blockcheck: using static DispatcherConfigurationReferenceContext;

using Paramore.Brighter;

public static class DispatcherConfigurationReferenceContext
{
    // block 5: `new RelationalDatabaseConfiguration(DbConnectionString())`
    public static string DbConnectionString() => "Server=localhost;Database=Greetings;";
}

// block 2: `new RmqSubscription<GreetingMade>(…)`
public class GreetingMade() : Event(Id.Random());
