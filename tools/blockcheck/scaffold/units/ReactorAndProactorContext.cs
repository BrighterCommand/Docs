// Types and values ReactorAndProactor.md names in its blocks and never declares.
//
// The page's handlers, mappers and subscriptions carry commands it never shows, and save through a
// repository it never shows. Each stub carries only the members the page's blocks use.
//
// blockcheck: using static ReactorAndProactorContext;

using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public static class ReactorAndProactorContext
{
    // block 10: `await SomeOperationAsync().ConfigureAwait(false)`
    public static Task SomeOperationAsync() => Task.CompletedTask;
}

// blocks 1–10, 12: `RequestHandler<MyCommand>`, `Subscription<MyCommand>`; blocks 1, 4: `command.Data`
public class MyCommand() : Command(Id.Random())
{
    public string Data { get; set; } = "";
}

// block 13: `Subscription<TradeCommand>`
public class TradeCommand() : Command(Id.Random());

// block 14: `Subscription<OrderCommand>`
public class OrderCommand() : Command(Id.Random());

// blocks 1, 4: `repository.Save(command.Data)`, `repository.SaveAsync(command.Data, cancellationToken)`
public interface IRepository
{
    void Save(string data);
    Task SaveAsync(string data, CancellationToken cancellationToken);
}
