---
description: "To add support for a different scheduler provider, implement the appropriate interfaces based on your use case."
layout:
  description:
    visible: false
---

# Custom Scheduler

> **How-to** · Applies to **Brighter V10**

To add support for a different scheduler provider, implement the appropriate interfaces based on your use case.

## Message Scheduler

The **message scheduler** is used by `IAmAMessageProducer` (sync/async) when:
- The transport lacks native delay support, or
- The delay exceeds the transport's threshold (e.g., AWS SQS 15-minute limit).

### Message Scheduler Implementation Steps

1. **Schedule Messages**  
   Implement `IAmAMessageSchedulerAsync` and `IAmAMessageSchedulerSync` to interface with your scheduler. Each interface has five members: schedule at a time or after a delay, reschedule, and cancel. Store a serialized `FireSchedulerMessage` as the job's payload, which is what Brighter's own schedulers do:

   ```csharp
   using System;
   using System.Text.Json;
   using System.Threading;
   using System.Threading.Tasks;
   using Paramore.Brighter;
   using Paramore.Brighter.JsonConverters;
   using Paramore.Brighter.Scheduler.Events;

   public class CustomMessageScheduler(CustomSchedulerAPI scheduler, TimeProvider timeProvider)
       : IAmAMessageSchedulerAsync, IAmAMessageSchedulerSync
   {
       public string Schedule(Message message, DateTimeOffset at)
           => scheduler.Schedule(Fire(message, async: false, at));

       public string Schedule(Message message, TimeSpan delay)
           => Schedule(message, timeProvider.GetUtcNow() + delay);

       public bool ReScheduler(string schedulerId, DateTimeOffset at)
           => scheduler.Reschedule(schedulerId, at);

       public bool ReScheduler(string schedulerId, TimeSpan delay)
           => ReScheduler(schedulerId, timeProvider.GetUtcNow() + delay);

       public void Cancel(string id) => scheduler.Cancel(id);

       public Task<string> ScheduleAsync(Message message, DateTimeOffset at, CancellationToken cancellationToken = default)
           => scheduler.ScheduleAsync(Fire(message, async: true, at), cancellationToken);

       public Task<string> ScheduleAsync(Message message, TimeSpan delay, CancellationToken cancellationToken = default)
           => ScheduleAsync(message, timeProvider.GetUtcNow() + delay, cancellationToken);

       public Task<bool> ReSchedulerAsync(string schedulerId, DateTimeOffset at, CancellationToken cancellationToken = default)
           => Task.FromResult(ReScheduler(schedulerId, at));

       public Task<bool> ReSchedulerAsync(string schedulerId, TimeSpan delay, CancellationToken cancellationToken = default)
           => Task.FromResult(ReScheduler(schedulerId, delay));

       public Task CancelAsync(string id, CancellationToken cancellationToken = default)
       {
           Cancel(id);
           return Task.CompletedTask;
       }

       // Async records which call scheduled the message, so the handler fires it the same way
       private static CustomSchedulerObject Fire(Message message, bool async, DateTimeOffset at) => new()
       {
           Data = JsonSerializer.Serialize(
               new FireSchedulerMessage { Message = message, Async = async },
               JsonSerialisationOptions.Options),
           At = at
       };
   }
   ```

   `CustomSchedulerAPI` stands for your scheduler's client. It stores a `CustomSchedulerObject` (a payload and the time it is due), returns an id, and calls you back with the payload when the time comes.

2. **Handle Scheduled Messages**  
   When the scheduler triggers, deserialize the payload and send it to Brighter:

   ```csharp
   using System.Text.Json;
   using System.Threading.Tasks;
   using Paramore.Brighter;
   using Paramore.Brighter.JsonConverters;
   using Paramore.Brighter.Scheduler.Events;

   public class CustomSchedulerHandler(IAmACommandProcessor processor)
   {
       public async Task ExecuteAsync(CustomSchedulerObject obj)
       {
           var fire = JsonSerializer.Deserialize<FireSchedulerMessage>(obj.Data, JsonSerialisationOptions.Options)!;
           await processor.SendAsync(fire);
       }
   }
   ```

3. **Register the Scheduler**  
   Implement `IAmAMessageSchedulerFactory` to integrate with Brighter.

## Request Scheduler

The **request scheduler** is used by `IAmACommandProcessor` when methods like `Send`, `Publish`, or `Post` are called with a `DateTimeOffset` or `TimeSpan`.

### Request Scheduler Implementation Steps

1. **Schedule Requests**  
   Implement `IAmARequestSchedulerAsync` and `IAmARequestSchedulerSync`. The payload is a serialized `FireSchedulerRequest`, which carries the request's type name and its own serialized form:

   ```csharp
   using System;
   using System.Text.Json;
   using System.Threading;
   using System.Threading.Tasks;
   using Paramore.Brighter;
   using Paramore.Brighter.JsonConverters;
   using Paramore.Brighter.Scheduler.Events;

   public class CustomRequestScheduler(CustomSchedulerAPI scheduler, TimeProvider timeProvider)
       : IAmARequestSchedulerAsync, IAmARequestSchedulerSync
   {
       public string Schedule<TRequest>(TRequest request, RequestSchedulerType type, DateTimeOffset at)
           where TRequest : class, IRequest
           => scheduler.Schedule(Fire(request, type, async: false, at));

       public string Schedule<TRequest>(TRequest request, RequestSchedulerType type, TimeSpan delay)
           where TRequest : class, IRequest
           => Schedule(request, type, timeProvider.GetUtcNow() + delay);

       public bool ReScheduler(string schedulerId, DateTimeOffset at)
           => scheduler.Reschedule(schedulerId, at);

       public bool ReScheduler(string schedulerId, TimeSpan delay)
           => ReScheduler(schedulerId, timeProvider.GetUtcNow() + delay);

       public void Cancel(string id) => scheduler.Cancel(id);

       public Task<string> ScheduleAsync<TRequest>(TRequest request, RequestSchedulerType type, DateTimeOffset at,
           CancellationToken cancellationToken = default)
           where TRequest : class, IRequest
           => scheduler.ScheduleAsync(Fire(request, type, async: true, at), cancellationToken);

       public Task<string> ScheduleAsync<TRequest>(TRequest request, RequestSchedulerType type, TimeSpan delay,
           CancellationToken cancellationToken = default)
           where TRequest : class, IRequest
           => ScheduleAsync(request, type, timeProvider.GetUtcNow() + delay, cancellationToken);

       public Task<bool> ReSchedulerAsync(string schedulerId, DateTimeOffset at, CancellationToken cancellationToken = default)
           => Task.FromResult(ReScheduler(schedulerId, at));

       public Task<bool> ReSchedulerAsync(string schedulerId, TimeSpan delay, CancellationToken cancellationToken = default)
           => Task.FromResult(ReScheduler(schedulerId, delay));

       public Task CancelAsync(string id, CancellationToken cancellationToken = default)
       {
           Cancel(id);
           return Task.CompletedTask;
       }

       private static CustomSchedulerObject Fire<TRequest>(TRequest request, RequestSchedulerType type, bool async,
           DateTimeOffset at) => new()
       {
           Data = JsonSerializer.Serialize(
               new FireSchedulerRequest
               {
                   SchedulerType = type,
                   RequestType = typeof(TRequest).FullName!,
                   RequestData = JsonSerializer.Serialize(request, JsonSerialisationOptions.Options),
                   Async = async
               },
               JsonSerialisationOptions.Options),
           At = at
       };
   }
   ```

2. **Handle Scheduled Requests**  
   Forward requests to Brighter for processing:

   ```csharp
   using System.Text.Json;
   using System.Threading.Tasks;
   using Paramore.Brighter;
   using Paramore.Brighter.JsonConverters;
   using Paramore.Brighter.Scheduler.Events;

   public class CustomRequestHandler(IAmACommandProcessor processor)
   {
       public async Task ExecuteAsync(CustomSchedulerObject obj)
       {
           var fire = JsonSerializer.Deserialize<FireSchedulerRequest>(obj.Data, JsonSerialisationOptions.Options)!;
           await processor.SendAsync(fire);
       }
   }
   ```

3. **Register the Scheduler**  
   Implement `IAmARequestSchedulerFactory` to integrate with Brighter.


## Key Requirements
- **Flow Preservation**:  
  Ensure `Async = true`/`false` is set correctly to match the original call (sync/async).
- **Serialization**:  
  Use a consistent format (e.g., JSON) for `RequestData` in `FireSchedulerRequest`.


**Note**: Replace `CustomSchedulerAPI` and `CustomSchedulerObject` with your scheduler's own client and job types.
