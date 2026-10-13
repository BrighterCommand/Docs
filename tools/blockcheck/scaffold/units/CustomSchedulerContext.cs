// Types CustomScheduler.md names in its blocks and never declares.
//
// `CustomSchedulerAPI` and `CustomSchedulerObject` stand for the reader's own scheduler client and
// job type; the page says so (`CustomScheduler.md`, the paragraph after block 1, and the closing
// note). Each stub carries only the members a block calls.

using System;
using System.Threading;
using System.Threading.Tasks;

// blocks 1, 3: `scheduler.Schedule`, `ScheduleAsync`, `Reschedule`, `Cancel`
public class CustomSchedulerAPI
{
    public string Schedule(CustomSchedulerObject job) => null!;
    public Task<string> ScheduleAsync(CustomSchedulerObject job, CancellationToken cancellationToken) => null!;
    public bool Reschedule(string id, DateTimeOffset at) => default;
    public void Cancel(string id) { }
}

// blocks 1–4: `new CustomSchedulerObject { Data = …, At = at }`, `obj.Data`
public class CustomSchedulerObject
{
    public string Data { get; set; } = string.Empty;
    public DateTimeOffset At { get; set; }
}
