// Types and values Logging.md names in its blocks and never declares.
//
// Block 3 scans the assembly of the reader's own command, which the page does not show. Block 4
// builds a Command Processor by hand from the registry and handler factory the reader has made for
// their own handlers, which the page does not show either. Every value member is typed from a
// pinned package and returns a default; a stub is a request with no member.
//
// blockcheck: using static LoggingContext;

using Paramore.Brighter;

public static class LoggingContext
{
    // block 4: `new HandlerConfiguration(subscriberRegistry, handlerFactory)`
    public static SubscriberRegistry subscriberRegistry => null!;
    public static IAmAHandlerFactory handlerFactory => null!;
}

// block 3: `typeof(GreetingCommand).Assembly`
public class GreetingCommand() : Command(Id.Random());
