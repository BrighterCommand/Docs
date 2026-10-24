// The types AzureServiceBusConfiguration.md names in its blocks and never shows.
//
// The subscription block subscribes to three messages of the reader's own, which the page does not
// declare. Each stub is a request and nothing more: no block names a member of one.

using Paramore.Brighter;

// block 2: `new AzureServiceBusSubscription<GreetingAsyncEvent>(…)`
public class GreetingAsyncEvent() : Event(Id.Random());

// block 2: `new AzureServiceBusSubscription<GreetingEvent>(…)`
public class GreetingEvent() : Event(Id.Random());

// block 2: `new AzureServiceBusSubscription<AddGreetingCommand>(…)`
public class AddGreetingCommand() : Command(Id.Random());
