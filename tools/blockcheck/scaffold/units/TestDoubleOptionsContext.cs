// Types and values TestDoubleOptions.md names in its blocks and never declares.
//
// The page shows how to verify a handler's calls to the command processor, and the handlers and
// requests it verifies are the reader's own. Every value member is typed from a pinned package
// and returns a default; a stub is a request, or a handler with only the members a block names.
//
// blockcheck: using static TestDoubleOptionsContext;

using Paramore.Brighter;
using Paramore.Brighter.Testing;

public static class TestDoubleOptionsContext
{
    // blocks 3, 4: `spy.GetRequests<MyCommand>()`, `spy.GetCalls(CommandType.Send)`, `spy.RecordedCalls`
    public static SpyCommandProcessor spy => null!;
}

// blocks 3, 11, 12: `spy.GetRequests<MyCommand>()`, `handler.Handle(new MyCommand())`
public class MyCommand() : Command(Id.Random());

// blocks 11, 12: `p.Publish(It.IsAny<MyEvent>(), …)`
public class MyEvent() : Event(Id.Random());

// blocks 11, 12: `new MyHandler(mock.Object)`, `handler.Handle(new MyCommand())`
public class MyHandler(IAmACommandProcessor commandProcessor)
{
    public void Handle(MyCommand command) => commandProcessor.Publish(new MyEvent());
}
