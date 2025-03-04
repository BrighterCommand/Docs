# Custom scheduler

If you need to add support to a different scheduler provider, you can do it by implementing a different interface depending on your case.

## Message scheduler
The message scheduler is used by `IAmAMessageProducer` (`Sync` and Async`) when the producer doesn't support a delay message or when the delay has passed a threshold (it's a different value depending on the transport). 

The first step is to schedule the message on the custom scheduler, you can do it by implementing the `IAmAMessageSchedulerAsync` and `IAmAMessageSchedulerSync`.

The next consumes the scheduler message, once the scheduler message is in the customer scheduler message handler, it's necessary to call `IAmACommandProcessor.SendAsync` passing the `FireSchedulerMessage` as the first argument. The brighter will to the rest of the work to you,  passing it to the correct `IAmAMessageProducer`

The last step is to implement the `IAmAMessageSchedulerFactory`.

Beware that Brighter should honour the customer flow, so if a customer calls the `SchedulerAsync`, then the fired message will use the `SendAsync`, same for `Sync` calls,  `Scheduler` -> `Send`. You can honour this by setting the `Async` as `true` or `false` on the `FireSchedulerMessage`.

sample:
```c#
public class CustomMessageScheduler(CustomSchedulerAPI scheduler) : IAmAMessageSchedulerAsync, IAmAMessageSchedulerSync
{
    public async Task<string> SchedulerAsync(Message message, DateTimeOffset at)
    {
        return await scheduler.SechedulerAsync(new CustomSchedulerObject { Data = new {message, Async = true }, At = at } ));
    }

    public string Scheduler(Message message, DateTimeOffset at)
    {
        return scheduler.Secheduler(new CustomSchedulerObject { Data = new {message, Async = false }, At = at } ));
    }

    ....
}


public class CustomerSchedulerHandler(IAmACommandProcessor prrocessor)
{
    public async Task ExecuteAsync(CustomeScheudlerObject obj)
    {
        await processor.SendAsync(new FireSchedulerMessage
        {
            Message = obj.Data.Message,
            Async = obj.Data.Async
        });
    }
}
```

## Request Scheduler

The request scheduler is used by `IAmACommandProcessor` when the customer calls `Send`, `Publish` and `Post` with a `DateTimeOffset` or a `TimeSpan`

The first step is to request the message on the custom scheduler to implement the `IAmARequestSchedulerAsync` and `IAmARequestSchedulerSync`.

The next consumes the scheduler message, once the scheduler message is in the customer scheduler message handler, it's necessary to call `IAmACommandProcessor.SendAsync` passing the `FireRequestMessage` as the first argument. The brighter will to the rest of the work to you,  by calling the correct method.

The last step is to implement the `IAmARequestSchedulerFactory`.

Beware that Brighter should honour the customer flow, so if a customer calls the `SendAsync`, then the fired message should use the `SendAsync`, same for `Sync` calls,  `Publish` -> `Publish`. You can honour this by setting the `Async` as `true` or `false` on the `FireRequestMessage`.


sample:
```c#
public class CustomRequestScheduler(CustomSchedulerAPI scheduler) : IAmARequestSchedulerAsync, IAmARequestSchedulerSync
{
    public async Task<string> SchedulerAsync<T>(T req, SchedulerType type, DateTimeOffset at)
    {
        return await scheduler.SechedulerAsync(new CustomSchedulerObject { Data = new { ReqTypeName = typeof(T).FullName, Data = Serialize(req), Async = true }, At = at } ));
    }

    public string Scheduler<T>(T req, SchedulerType type, DateTimeOffset at)
    {
        return scheduler.Secheduler(new CustomSchedulerObject { Data = new {  ReqTypeName = typeof(T).FullName, Data = Serialize(req), type, Async = false }, At = at } ));
    }

    ....
}


public class CustomerSchedulerHandler(IAmACommandProcessor prrocessor)
{
    public async Task ExecuteAsync(CustomeScheudlerObject obj)
    {
        await processor.SendAsync(new FireRequestMessage
        {
            RequestType = obj.Data.ReqTypeName,
            RequestData = obj.Data.Data,
            Async = obj.Data.Async
        });
    }
}
```
