# Brighter Scheduler Support

In distributed applications, it is often necessary to schedule messages for deferred execution. Brighter provides built-in scheduling capabilities to handle that scenarios.


## Scheduling Methods

The `IAmACommandProcessor` interface includes methods for scheduling messages:

```csharp
public interface IAmACommandProcessor
{
    // Synchronous methods
    string Send<T>(DateTimeOffset at, T command);
    string Send<T>(TimeSpan delay, T command);
    string Publish<T>(DateTimeOffset at, T @event);
    string Publish<T>(TimeSpan delay, T @event);
    string Post<T>(DateTimeOffset at, T @event);
    string Post<T>(TimeSpan delay, T @event);

    // Asynchronous methods
    Task<string> SendAsync<T>(DateTimeOffset at, T command);
    Task<string> SendAsync<T>(TimeSpan delay, T command);
    Task<string> PublishAsync<T>(DateTimeOffset at, T @event);
    Task<string> PublishAsync<T>(TimeSpan delay, T @event);
    Task<string> PostAsync<T>(DateTimeOffset at, T @event);
    Task<string> PostAsync<T>(TimeSpan delay, T @event);
}
```

## Scheduler Types

Brighter supports three scheduling mechanisms:

### 1. **Transport-Based Scheduler**
Uses native transport APIs (e.g., Azure Service Bus `EnqueuedTimeUtc` or AWS SQS delay).

**Limitations**:
- No support for modifying/canceling schedules on some platforms.
 - Some transport can have limitation, like AWS SQS that only support max of 15 minutes of delay

### 2. **Message-Based Scheduler**
Fallback for transports without native scheduling (e.g., AWS SQS <15min delays).

Example:
  ```csharp
  messageScheduler.Scheduler(TimeSpan.FromMinutes(1), new DailyReportEvent());
  ```

### 3. **Request-Based Scheduler**
Directly schedules via `IAmACommandProcessor` methods (`Send`, `Publish`, `Post`).

Respects sync/async execution flow:
```csharp
// Synchronous scheduling
commandProcessor.Send(TimeSpan.FromMinutes(1), new MyCommand());
commandProcessor.Publish(DateTimeOffset.UtcNow.AddSeconds(10), new MyCommand());

// Asynchronous scheduling
await commandProcessor.PublishAsync(TimeSpan.FromHours(1), new MyEvent());
await commandProcessor.PostAsync(DateTimeOffset.UtcNow.AddSeconds(10), new MyCommand());
```