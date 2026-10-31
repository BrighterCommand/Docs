// Types BrighterInboxSupport.md names in its blocks and never declares.
//
// Block 1 is a handler's method, and the page names the event it handles and the handler it
// belongs to, which are the reader's own. Each stub carries only the members the block names.

using Paramore.Brighter;

// block 1: `RequestHandlerAsync<GreetingMade>`, `@event.Greeting`
public class GreetingMade() : Event(Id.Random())
{
    public string Greeting { get; set; } = string.Empty;
}

// block 1: `contextKey: typeof(GreetingMadeHandlerAsync)`
public class GreetingMadeHandlerAsync : RequestHandlerAsync<GreetingMade>;
