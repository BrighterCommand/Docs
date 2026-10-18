// Task 4.5, SchedulingAMessage.md § Registering Handlers When You Schedule Requests.
// Claim: at 10.7.0 AutoFromAssemblies() registers the scheduler's own handlers twice, so a request
// scheduled with SendAsync throws "More than one handler was found … FireSchedulerRequest" when it
// falls due, and with the InMemory scheduler that ends the process. The page says the fix (#4414)
// is not yet released. Case: 10.8.0. Control: 10.7.0, where the claim says it fails. Each version
// runs twice, once per scan the page names: no argument is AutoFromAssemblies(), "async" is
// AsyncHandlersFromAssemblies over this assembly.
// The process prints "handled" and exits 0 if the command runs; an unhandled throw on the
// scheduler's timer ends it with a non-zero code before that line.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var services = new ServiceCollection();
var builder = services.AddBrighter()
    .UseScheduler(new InMemorySchedulerFactory());
var scan = args.Length > 0 ? args[0] : "auto";
if (scan == "async") builder.AsyncHandlersFromAssemblies([typeof(ScheduledCommand).Assembly]);
else builder.AutoFromAssemblies();

var provider = services.BuildServiceProvider();
var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();

await commandProcessor.SendAsync(TimeSpan.FromMilliseconds(100), new ScheduledCommand());
for (var i = 0; i < 30 && ScheduledCommandHandler.Handled == 0; i++) await Task.Delay(100);

var version = typeof(CommandProcessor).Assembly.GetName().Version;
Console.WriteLine($"{version} {scan}: handled = {ScheduledCommandHandler.Handled}");
return ScheduledCommandHandler.Handled == 1 ? 0 : 1;

public class ScheduledCommand() : Command(Id.Random());

public class ScheduledCommandHandler : RequestHandlerAsync<ScheduledCommand>
{
    public static int Handled;
    public override Task<ScheduledCommand> HandleAsync(ScheduledCommand command, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref Handled);
        return base.HandleAsync(command, cancellationToken);
    }
}
