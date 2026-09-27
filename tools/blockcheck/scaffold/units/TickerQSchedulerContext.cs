// Values TickerQScheduler.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static TickerQSchedulerContext;

using Microsoft.AspNetCore.Builder;
using Paramore.Brighter;

public static class TickerQSchedulerContext
{
    // blocks 5, 6: `_scheduler.CancelAsync(…)`, `_scheduler.ReSchedulerAsync(…)`
    public static IAmAMessageSchedulerAsync _scheduler => null!;

    // blocks 3, 8: `builder.Services.AddTickerQ(…)`, block 1's `WebApplication.CreateBuilder(args)`
    public static WebApplicationBuilder builder => null!;

    // block 7: `app.UseTickerQ()`, block 1's `builder.Build()`
    public static WebApplication app => null!;
}

// block 4: `new SendReminderCommand { UserId = userId }` — the page never shows the command
public class SendReminderCommand() : Command(Id.Random())
{
    public string? UserId { get; set; }
}
