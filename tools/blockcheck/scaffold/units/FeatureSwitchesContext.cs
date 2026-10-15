// Types and values FeatureSwitches.md names in its blocks and never declares.
//
// The handlers switch a `MyCommand` the page assumes rather than defines, and block 7 configures
// a `CommandProcessorBuilder` from a subscriber registry and handler factory it never creates.
// Values return a default and do nothing; a block that calls a member of one is checked against
// the real type.
//
// blockcheck: using static FeatureSwitchesContext;

using Paramore.Brighter;

public static class FeatureSwitchesContext
{
    // block 7: `new HandlerConfiguration(_registry, _handlerFactory)`
    public static SubscriberRegistry _registry => null!;
    public static IAmAHandlerFactorySync _handlerFactory => null!;
}

// blocks 1–4: `RequestHandler<MyCommand>`, `RequestHandlerAsync<MyCommand>`
public class MyCommand() : Command(Id.Random());

// block 7: `StatusOf<MyFeatureSwitchedConfigHandler>()`
public class MyFeatureSwitchedConfigHandler : RequestHandler<MyCommand>;
