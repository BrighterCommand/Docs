// Types and values AsyncDispatchARequest.md names in its blocks and never declares.
//
// The dispatch snippets send a `GreetingCommand` the page assumes rather than defines, through
// the `commandProcessor` block 1 resolves. Values return a default and do nothing; a block that
// calls a member of one is checked against the real type.
//
// blockcheck: using static AsyncDispatchARequestContext;

using Paramore.Brighter;

public static class AsyncDispatchARequestContext
{
    // blocks 2, 3: `commandProcessor.SendAsync(…)`, as block 1 resolves it
    public static IAmACommandProcessor commandProcessor => null!;
}

// blocks 1–4: `new GreetingCommand("Ian")`, `RequestHandlerAsync<GreetingCommand>`
public class GreetingCommand(string name) : Command(Id.Random());
