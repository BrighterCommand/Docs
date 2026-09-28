// Types and values HowConfiguringTheDispatcherWorks.md names in its blocks and never declares.
//
// The page builds a Dispatcher by hand for a `GreetingCommand` and its mapper, from a handler
// factory, subscriber registry and mapper factory that How Configuring the Command Processor Works
// sets up. The mapper stub derives from Brighter's own `JsonMessageMapper<T>`, so it declares no
// member of its own: no block names one.
//
// blockcheck: using static HowConfiguringTheDispatcherWorksContext;

using Paramore.Brighter;
using Paramore.Brighter.MessageMappers;
using Paramore.Brighter.ServiceActivator;

public static class HowConfiguringTheDispatcherWorksContext
{
    // blocks 1, 2: `new MessageMapperRegistry(messageMapperFactory, null)`
    public static IAmAMessageMapperFactory messageMapperFactory => null!;

    // block 2: `new HandlerConfiguration(subscriberRegistry, handlerFactory)`
    public static IAmASubscriberRegistry subscriberRegistry => null!;
    public static IAmAHandlerFactory handlerFactory => null!;

    // block 2: `_dispatcher = DispatchBuilder.StartNew()…Build();`
    public static IDispatcher? _dispatcher;
}

// blocks 1, 2: `Register<GreetingCommand, GreetingCommandMessageMapper>()`, `RmqSubscription<GreetingCommand>`
public class GreetingCommand() : Command(Id.Random());

// blocks 1, 2: `Register<GreetingCommand, GreetingCommandMessageMapper>()`
public class GreetingCommandMessageMapper : JsonMessageMapper<GreetingCommand>;
