// Task 3.5, InMemoryScheduler.md block 1. Claims: SendAsync(delay) returns a scheduler id, and the
// command runs after the delay; the block's last comment says "Verify command was handled...".
// AutoFromAssemblies() registers TestCommandHandler, which counts. Case: the block as written,
// then the count after its own Task.Delay(150). Control: the count before the delay, at once.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Xunit;

try
{
    await new Block1().Should_Schedule_Command_For_Later_Execution();
}
catch (Exception e)
{
    Console.WriteLine($"FAIL case    block 1: {e.GetType().Name} {e.Message.Split('\n')[0]}");
    return 1;
}
var ok = Block1.HandledAtOnce == 0 && TestCommandHandler.Handled == 1;
Console.WriteLine($"{(Block1.HandledAtOnce == 0 ? "ok  " : "FAIL")} control block 1: handled at once = {Block1.HandledAtOnce}");
Console.WriteLine($"{(TestCommandHandler.Handled == 1 ? "ok  " : "FAIL")} case    block 1: scheduler id returned, handled after the delay = {TestCommandHandler.Handled}");
return ok ? 0 : 1;

public class TestCommand() : Command(Id.Random());
public class TestCommandHandler : RequestHandlerAsync<TestCommand>
{
    public static int Handled;
    public override Task<TestCommand> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Handled);
        return base.HandleAsync(command, cancellationToken);
    }
}

// ---- InMemoryScheduler.md block 1, verbatim but for the class it is a method of, and the one
//      probe line that records the control ----
public class Block1
{
public static int HandledAtOnce = -1;

[Fact]
public async Task Should_Schedule_Command_For_Later_Execution()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddBrighter(options =>
        {
            // ...
        })
        .UseScheduler(new InMemorySchedulerFactory())  // Perfect for tests
        .AutoFromAssemblies();

    var provider = services.BuildServiceProvider();
    var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();

    // Act
    var schedulerId = await commandProcessor.SendAsync(
        TimeSpan.FromMilliseconds(100),
        new TestCommand()
    );
    HandledAtOnce = TestCommandHandler.Handled;  // probe: the control

    // Assert
    Assert.NotNull(schedulerId);
    await Task.Delay(150);  // Wait for scheduled execution
    // Verify command was handled...
}
}
