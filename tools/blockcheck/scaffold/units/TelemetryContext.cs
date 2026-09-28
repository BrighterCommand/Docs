// The type and value Telemetry.md names in its blocks and never declares.
//
// The page sends a `ProcessOrderCommand`, the reader's own, through the reader's command processor
// and handles it, and never shows the command. The value is typed from a pinned package and
// returns a default; the stub carries only the member the blocks name.
//
// blockcheck: using static TelemetryContext;

using Paramore.Brighter;

public static class TelemetryContext
{
    // block 3: `await commandProcessor.SendAsync(new ProcessOrderCommand { OrderId = 123 });`
    public static IAmACommandProcessor commandProcessor => null!;
}

// blocks 3, 4: `new ProcessOrderCommand { OrderId = 123 }`, `RequestHandlerAsync<ProcessOrderCommand>`,
// `command.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public int OrderId { get; set; }
}
