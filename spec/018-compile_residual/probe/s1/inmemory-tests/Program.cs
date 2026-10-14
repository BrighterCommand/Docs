// Task 3.5, InMemoryScheduler.md. Claims, as the tests assert them:
//   block 9  Should_Execute_Scheduled_Command_After_Delay: not handled at once, handled after the delay
//   block 9  Should_Cancel_Scheduled_Command: cancelled through IAmARequestSchedulerAsync, never handled
// Each test is its own control for the others: execute and cancel assert opposite outcomes of the
// same schedule, so a fixture that handled everything, or nothing, fails one of them. The probe
// adds a third control: the cancel test with the CancelAsync line removed must see the command.
// Block 1 is ../inmemory-block1: its AutoFromAssemblies() would register this fixture's handler too.
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Xunit;

var failures = 0;
async Task Run(string name, Func<Task> test, bool shouldPass)
{
    try { await test(); Console.WriteLine($"{(shouldPass ? "ok  " : "FAIL")} {name}: passed"); if (!shouldPass) failures++; }
    catch (Exception e) { Console.WriteLine($"{(shouldPass ? "FAIL" : "ok  ")} {name}: {e.GetType().Name} {e.Message.Split('\n')[0]}"); if (shouldPass) failures++; }
}

using (var t = new SchedulingTests()) await Run("case    block 9 Should_Execute_Scheduled_Command_After_Delay", t.Should_Execute_Scheduled_Command_After_Delay, true);
using (var t = new SchedulingTests()) await Run("case    block 9 Should_Cancel_Scheduled_Command", t.Should_Cancel_Scheduled_Command, true);
using (var t = new SchedulingTests()) await Run("control block 9 cancel test without CancelAsync (must fail)", t.Should_Cancel_Without_Cancelling, false);
return failures == 0 ? 0 : 1;

// TestCommand: the page never shows it; the scaffold's stub, made real.
public class TestCommand() : Command(Id.Random());

// ---- block 9, verbatim, plus the control method below the line ----
public class SchedulingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly TestHandlerRegistry _handlerRegistry;

    public SchedulingTests()
    {
        var services = new ServiceCollection();
        _handlerRegistry = new TestHandlerRegistry();

        services.AddSingleton(_handlerRegistry);

        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .UseScheduler(new InMemorySchedulerFactory())  // InMemory for tests
        .AsyncHandlers(registry => registry.RegisterAsync<TestCommand, TestCommandHandler>());

        _serviceProvider = services.BuildServiceProvider();
        _commandProcessor = _serviceProvider.GetRequiredService<IAmACommandProcessor>();
    }

    [Fact]
    public async Task Should_Execute_Scheduled_Command_After_Delay()
    {
        // Arrange
        var command = new TestCommand();
        var delay = TimeSpan.FromMilliseconds(100);

        // Act
        var schedulerId = await _commandProcessor.SendAsync(delay, command);

        // Assert - command not yet executed
        Assert.False(_handlerRegistry.WasHandled(command.Id));

        // Wait for scheduled execution
        await Task.Delay(delay.Add(TimeSpan.FromMilliseconds(50)));

        // Assert - command executed
        Assert.True(_handlerRegistry.WasHandled(command.Id));
    }

    [Fact]
    public async Task Should_Cancel_Scheduled_Command()
    {
        // Arrange
        var command = new TestCommand();
        var delay = TimeSpan.FromSeconds(10);  // Long delay
        var scheduler = _serviceProvider.GetRequiredService<IAmARequestSchedulerAsync>();

        // Act - schedule through the scheduler that cancels it
        var schedulerId = await scheduler.ScheduleAsync(command, RequestSchedulerType.Send, delay);
        await scheduler.CancelAsync(schedulerId);  // Cancel immediately

        // Wait to ensure it would have executed
        await Task.Delay(TimeSpan.FromSeconds(11));

        // Assert - command was NOT executed
        Assert.False(_handlerRegistry.WasHandled(command.Id));
    }

    // CONTROL, not on the page: the same test with the CancelAsync line removed
    public async Task Should_Cancel_Without_Cancelling()
    {
        var command = new TestCommand();
        var delay = TimeSpan.FromSeconds(10);
        var scheduler = _serviceProvider.GetRequiredService<IAmARequestSchedulerAsync>();
        var schedulerId = await scheduler.ScheduleAsync(command, RequestSchedulerType.Send, delay);
        await Task.Delay(TimeSpan.FromSeconds(11));
        Assert.False(_handlerRegistry.WasHandled(command.Id));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

public class TestHandlerRegistry
{
    private readonly ConcurrentDictionary<Id, bool> _handled = new();

    public void Record(Id id) => _handled[id] = true;
    public bool WasHandled(Id id) => _handled.ContainsKey(id);
    public int HandledCount => _handled.Count;
}

public class TestCommandHandler(TestHandlerRegistry registry) : RequestHandlerAsync<TestCommand>
{
    public override Task<TestCommand> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
    {
        registry.Record(command.Id);
        return base.HandleAsync(command, cancellationToken);
    }
}
