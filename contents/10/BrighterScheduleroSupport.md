# Scheduling


In a distributed application, something we want to schedule a message for many reasons, thinking that Brighter has implemented support to scheduler messages.

The `IAmACommandProcessor` has support for scheduler requests with these new methods:

```csharp
public interface IAmACommandProcessor
{
    string Send<T>(DateTimeOffset at, T command);
    string Send<T>(TimeSpan delay, T command);

    string Publish<T>(DateTimeOffset at, T event);
    string Publish<T>(TimeSpan delay, T event);

    string Post<T>(DateTimeOffset at, T event);
    string Post<T>(TimeSpan delay, T event);


    Task<string> SendAsync<T>(DateTimeOffset at, T command);
    Task<string> SendAsync<T>(TimeSpan delay, T command);

    Task<string> PublishAsync<T>(DateTimeOffset at, T event);
    Task<string> PublishAsync<T>(TimeSpan delay, T event);

    Task<string> PostAsync<T>(DateTimeOffset at, T event);
    Task<string> PostAsync<T>(TimeSpan delay, T event);
}
```

Brighter has three types of scheduler:
## Transport-based
Transport-based use of the native delay/enqueue API provided by the transport, but in many cases, the transport doesn't have support for this functionality or it doesn't support deleting/rescheduling a delayed message. This type of schedule uses only the `SendWithDelay`, and it has priority over any message-based scheduler

## Message-based
The message-based is used by `IAmAMessageProducer` when the transport doesn't have support for scheduler or when that scheduler has any type of limit, like AWS SQS where it support only 15min of delay.

## Request-based
The Request-based is used by `IAmACommandProcessor`, when the method `Send`, `Publish` and `Post` is called with a `DateTimeOffset` or `TimeSpan`.

When a request is scheduled, Brighter will respect the flow, if the request was scheduled using the sync method, when will invoke that method in a sync way, see:

```csharp
var command = new MyCommand();
commandProcessor.Send(TimeSpan.FromMinutes(1), new MyCommand());
.....

// Brigther internally
commandProcessor.Send(command)
```