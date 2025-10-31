# Custom Scheduler

To add support for a different scheduler provider, implement the appropriate interfaces based on your use case.

## Message Scheduler

The **message scheduler** is used by `IAmAMessageProducer` (sync/async) when:
- The transport lacks native delay support, or
- The delay exceeds the transport's threshold (e.g., AWS SQS 15-minute limit).

### Implementation Steps

1. **Schedule Messages**  
   Implement `IAmAMessageSchedulerAsync` and `IAmAMessageSchedulerSync` to interface with your scheduler:

   ```csharp
   public class CustomMessageScheduler : IAmAMessageSchedulerAsync, IAmAMessageSchedulerSync
   {
       private readonly CustomSchedulerAPI _scheduler;

       public CustomMessageScheduler(CustomSchedulerAPI scheduler)
       {
           _scheduler = scheduler;
       }

       public async Task<string> ScheduleAsync(Message message, DateTimeOffset at)
       {
           return await _scheduler.ScheduleAsync(
               new CustomSchedulerObject 
               { 
                   Data = new { Message = message, Async = true }, 
                   At = at 
               });
       }

       public string Schedule(Message message, DateTimeOffset at)
       {
           return _scheduler.Schedule(
               new CustomSchedulerObject 
               { 
                   Data = new { Message = message, Async = false }, 
                   At = at 
               });
       }
   }
   ```

2. **Handle Scheduled Messages**  
   When the scheduler triggers, forward the message to Brighter:

   ```csharp
   public class CustomSchedulerHandler
   {
       private readonly IAmACommandProcessor _processor;

       public CustomSchedulerHandler(IAmACommandProcessor processor)
       {
           _processor = processor;
       }

       public async Task ExecuteAsync(CustomSchedulerObject obj)
       {
           await _processor.SendAsync(new FireSchedulerMessage
           {
               Message = obj.Data.Message,
               Async = obj.Data.Async
           });
       }
   }
   ```

3. **Register the Scheduler**  
   Implement `IAmAMessageSchedulerFactory` to integrate with Brighter.

## Request Scheduler

The **request scheduler** is used by `IAmACommandProcessor` when methods like `Send`, `Publish`, or `Post` are called with a `DateTimeOffset` or `TimeSpan`.

### Implementation Steps

1. **Schedule Requests**  
   Implement `IAmARequestSchedulerAsync` and `IAmARequestSchedulerSync`:

   ```csharp
   public class CustomRequestScheduler : IAmARequestSchedulerAsync, IAmARequestSchedulerSync
   {
       private readonly CustomSchedulerAPI _scheduler;

       public CustomRequestScheduler(CustomSchedulerAPI scheduler)
       {
           _scheduler = scheduler;
       }

       public async Task<string> ScheduleAsync<T>(T request, SchedulerType type, DateTimeOffset at)
       {
           return await _scheduler.ScheduleAsync(
               new CustomSchedulerObject 
               { 
                   Data = new 
                   { 
                       RequestType = typeof(T).FullName, 
                       Data = Serialize(request), 
                       Async = true 
                   }, 
                   At = at 
               });
       }

       public string Schedule<T>(T request, SchedulerType type, DateTimeOffset at)
       {
           return _scheduler.Schedule(
               new CustomSchedulerObject 
               { 
                   Data = new 
                   { 
                       RequestType = typeof(T).FullName, 
                       Data = Serialize(request), 
                       Async = false 
                   }, 
                   At = at 
               });
       }
   }
   ```

2. **Handle Scheduled Requests**  
   Forward requests to Brighter for processing:

   ```csharp
   public class CustomRequestHandler
   {
       private readonly IAmACommandProcessor _processor;

       public CustomRequestHandler(IAmACommandProcessor processor)
       {
           _processor = processor;
       }

       public async Task ExecuteAsync(CustomSchedulerObject obj)
       {
           await _processor.SendAsync(new FireRequestMessage
           {
               RequestType = obj.Data.RequestType,
               RequestData = obj.Data.Data,
               Async = obj.Data.Async
           });
       }
   }
   ```

3. **Register the Scheduler**  
   Implement `IAmARequestSchedulerFactory` to integrate with Brighter.


## Key Requirements
- **Flow Preservation**:  
  Ensure `Async = true`/`false` is set correctly to match the original call (sync/async).
- **Serialization**:  
  Use a consistent format (e.g., JSON) for `RequestData` in `FireRequestMessage`.


**Note**: Replace `CustomSchedulerAPI`, `CustomSchedulerObject`, and serialization logic with your actual implementation details.