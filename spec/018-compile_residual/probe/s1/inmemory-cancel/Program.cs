// Task 4.5a (D19), #4437. Claim (SchedulingAMessage.md, FAQ.md, InMemoryScheduler.md): at 10.7.0 the
// InMemory scheduler cannot cancel or reschedule a request scheduled through the command processor:
// CancelAsync returns and the request still runs when it falls due. The pages' form is FAQ.md's:
//   var schedulerId = await commandProcessor.SendAsync(delay, command);
//   await scheduler.CancelAsync(schedulerId);   // or ReSchedulerAsync(schedulerId, newDelay)
// Cases, one per process (args[0]):
//   plain          control: SendAsync with a delay, not cancelled; expect handled = 1 at the delay
//   cancel         the pages' cancel; the claim says handled = 1 at 10.7.0
//   reschedule     the pages' reschedule to a later delay; the claim says it still runs at the first
//   direct-cancel  InMemoryScheduler.md's workaround: scheduler.ScheduleAsync, then CancelAsync
// The scheduler is the IAmARequestSchedulerAsync that UseScheduler registers.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var mode = args.Length > 0 ? args[0] : "plain";
var services = new ServiceCollection();
services.AddBrighter()
    .UseScheduler(new InMemorySchedulerFactory())
    .AsyncHandlers(r => r.RegisterAsync<ScheduledCommand, ScheduledCommandHandler>());
var provider = services.BuildServiceProvider();
var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();
var scheduler = provider.GetRequiredService<IAmARequestSchedulerAsync>();

var delay = TimeSpan.FromMilliseconds(300);
string id;
if (mode == "direct-cancel")
    id = await scheduler.ScheduleAsync(new ScheduledCommand(), RequestSchedulerType.Send, delay);
else
    id = await commandProcessor.SendAsync(delay, new ScheduledCommand());

var rescheduled = (bool?)null;
if (mode is "cancel" or "direct-cancel") await scheduler.CancelAsync(id);
if (mode == "reschedule") rescheduled = await scheduler.ReSchedulerAsync(id, TimeSpan.FromMilliseconds(1500));

await Task.Delay(900);                       // past the first delay, before the rescheduled one
var atFirst = ScheduledCommandHandler.Handled;
await Task.Delay(1500);                      // past the rescheduled one
var atEnd = ScheduledCommandHandler.Handled;

var version = typeof(CommandProcessor).Assembly.GetName().Version;
var fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(CommandProcessor).Assembly.Location).ProductVersion?.Split('+')[0];
Console.WriteLine($"{fv} {mode}: id='{id}' rescheduled={rescheduled?.ToString() ?? "-"} handled at 0.9s = {atFirst}, at 2.4s = {atEnd}");
return 0;

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
