// Task 7.6. Page code is copied as the page shows it; the probe's own lines are marked // probe.
//  Case: V10MigrationGuide.md, "Before (V9)" as it stood: [TimeoutPolicy] on an async handler's HandleAsync. The
//      timeout handler is synchronous (TimeoutPolicyHandler<T> : RequestHandler<T>), so building the async pipeline
//      throws ConfigurationException ("All handlers in an async pipeline must derive from IHandleRequestsAsync").
//  Control: the repaired form, [TimeoutPolicy] on a synchronous Handle: a handler slower than the timeout is cut off,
//      and one faster than it is handled. The cut-off surfaces as OperationCanceledException, not TimeoutException:
//      TimeoutPolicyHandler.Handle waits with task.Wait(ct) on the token its timer cancels (10.8.1). No page says which.
//  FAQ.md #15 as it stood, the attribute on the class: built with -p:ClassLevel=true, CS0592 (RequestHandlerAttribute is
//      [AttributeUsage(AttributeTargets.Method)] at 9.33 and 10.8.1).
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Policies.Attributes;

public class MyCommand() : Command(Id.Random());   // probe
public class SlowCommand() : Command(Id.Random());   // probe
public class QuickCommand() : Command(Id.Random());   // probe

// V10MigrationGuide.md, "Before (V9)", as it stood
public class MyHandler : RequestHandlerAsync<MyCommand>
{
    [TimeoutPolicy(milliseconds: 5000, step: 1)]
    public override async Task<MyCommand> HandleAsync(
        MyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Handler logic
        return await base.HandleAsync(command, cancellationToken);
    }
}

// The repaired form, timeouts shortened so the probe can see one fire
public class SlowHandler : RequestHandler<SlowCommand>
{
    [TimeoutPolicy(milliseconds: 500, step: 1)]
    public override SlowCommand Handle(SlowCommand command)
    {
        Thread.Sleep(2000);   // probe
        return base.Handle(command);
    }
}

public class QuickHandler : RequestHandler<QuickCommand>
{
    public static int Handled;   // probe
    [TimeoutPolicy(milliseconds: 5000, step: 1)]
    public override QuickCommand Handle(QuickCommand command)
    {
        Handled++;   // probe
        return base.Handle(command);
    }
}

#if CLASSLEVEL
// FAQ.md #15, "Old (V9)", as it stood
[TimeoutPolicy(step: 1, milliseconds: 5000)]
public class MyClassLevelHandler : RequestHandlerAsync<MyCommand> { }
#endif

public static class Program   // probe
{
    static int failures;
    static void Row(string name, bool ok, string detail) { Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); if (!ok) failures++; }

    static Exception? Inner(Exception e) { while (e is AggregateException { InnerException: { } i }) e = i; return e; }

    public static async Task<int> Main()
    {
        var services = new ServiceCollection();
        services.AddBrighter().AutoFromAssemblies([typeof(Program).Assembly]);
        var processor = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();

        try { await processor.SendAsync(new MyCommand()); Row("case: TimeoutPolicy on HandleAsync", false, "handled, no exception"); }
        catch (Exception e)
        {
            var x = Inner(e)!;
            Row("case: TimeoutPolicy on HandleAsync", x is ConfigurationException && x.ToString().Contains("async pipeline must derive from IHandleRequestsAsync"),
                $"{x.GetType().Name}: {x.Message}");
        }

        try { processor.Send(new SlowCommand()); Row("control: TimeoutPolicy on Handle, slower than the timeout", false, "handled, no exception"); }
        catch (Exception e) { var x = Inner(e)!; Row("control: TimeoutPolicy on Handle, slower than the timeout", x is OperationCanceledException or TimeoutException, x.GetType().Name); }

        try { processor.Send(new QuickCommand()); Row("control: TimeoutPolicy on Handle, faster than the timeout", QuickHandler.Handled == 1, $"handled {QuickHandler.Handled}"); }
        catch (Exception e) { Row("control: TimeoutPolicy on Handle, faster than the timeout", false, Inner(e)!.ToString()); }

        return failures == 0 ? 0 : 1;
    }
}
