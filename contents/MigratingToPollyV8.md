# Migrating to Polly v8

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)

Both tails of the Polly story in one place: the four steps that take a V9 handler using
`[UsePolicy]` to a V10 handler using `[UseResiliencePipeline]`, and the deprecated Polly v7
attribute itself, kept because it is what you are migrating *from*.
[Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) is the
current reference.

## Polly v8 Migration Guide: V9 to V10

### Step 1: Update NuGet Packages

```xml
<!-- Remove old Polly v7 -->
<PackageReference Include="Polly" Version="7.x.x" Remove="true" />

<!-- Add Polly v8 -->
<PackageReference Include="Polly" Version="8.0.0" />
<PackageReference Include="Polly.Extensions" Version="8.0.0" />
```

### Step 2: Replace Policy Registry with Resilience Pipeline Registry

**V9**:

```csharp
// ...
var policyRegistry = new PolicyRegistry();

var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

policyRegistry.Add("MyRetryPolicy", retryPolicy);
```

**V10**:

```csharp
// ...
var resiliencePipelineRegistry = new ResiliencePipelineRegistry<string>();

resiliencePipelineRegistry.TryAddBuilder("MyRetryPipeline",
    (builder, context) => builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    }));
```

### Step 3: Replace Attributes in Handlers

**V9**:

```csharp
// ...
internal class MyHandler : RequestHandler<MyCommand>
{
    [UsePolicy("MyRetryPolicy", step: 1)]
    [TimeoutPolicy(milliseconds: 5000, step: 2)]
    public override MyCommand Handle(MyCommand command)
    {
        // Handler logic
    }
}
```

**V10**:

```csharp
// ...
internal class MyHandler : RequestHandler<MyCommand>
{
    [UseResiliencePipeline("MyRetryPipeline", step: 1)]
    [UseResiliencePipeline("MyTimeoutPipeline", step: 2)]
    public override MyCommand Handle(MyCommand command)
    {
        // Handler logic
    }
}
```

### Step 4: Update CommandProcessor Configuration

**V9**:

```csharp
// ...
var commandProcessor = CommandProcessorBuilder.With()
    .Handlers(/* ... */)
    .Policies(policyRegistry)
    .Build();
```

**V10**:

```csharp
// ...
var commandProcessor = CommandProcessorBuilder.With()
    .Handlers(/* ... */)
    .Policies(policyRegistry)  // Optional: Keep for legacy v7 policies during migration
    .ResiliencePipelines(resiliencePipelineRegistry)  // New: Polly v8 pipelines
    .Build();
```

> **Note**: You can use both `Policies()` and `ResiliencePipelines()` during migration to support both legacy `UsePolicy` and new `UseResiliencePipeline` attributes.

---

## Legacy: Using Polly v7 Policies (Deprecated)

> **⚠️ DEPRECATED**: The following section documents the legacy Polly v7 `UsePolicy` attribute, which is deprecated in favor of `UseResiliencePipeline`. This is maintained for backward compatibility only.

### Using Brighter's UsePolicy Attribute (Legacy)

By adding the **UsePolicy** attribute, you instruct the Command Processor to insert a handler (filter) into the pipeline that runs all later steps using that Polly policy.

```csharp
// ...
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UsePolicy(policy: "MyExceptionPolicy", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        /*Do work that could throw error because of distributed computing reliability*/
    }
}
```

To configure the Polly policy you use the PolicyRegistry to register the Polly Policy with a name. At runtime we look up that Policy by name.

```csharp
// ...
var policyRegistry = new PolicyRegistry();

var policy = Policy
    .Handle<Exception>()
    .WaitAndRetry(new[]
    {
        1.Seconds(),
        2.Seconds(),
        3.Seconds()
    }, (exception, timeSpan) =>
    {
        s_retryCount++;
    });

policyRegistry.Add("MyExceptionPolicy", policy);
```

You can use multiple policies with a handler, instead of passing in a single policy identifier, you can pass in an array of policy identifiers:

So if in addition to the above policy we have:

```csharp
// ...
var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(
		1, TimeSpan.FromMilliseconds(500));

policyRegistry.Add("MyCircuitBreakerPolicy", policy);
```

then you can add them both to your handler as follows:

```csharp
// ...
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UsePolicy(new [] {"MyCircuitBreakerPolicy", "MyExceptionPolicy"} , step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        /*Do work that could throw error because of distributed computing reliability*/
    }
}
```

Where we have multiple policies they are evaluated left to right, so in this case "MyCircuitBreakerPolicy" wraps "MyExceptionPolicy".

When creating policies, refer to the [Polly](https://github.com/App-vNext/Polly) documentation.

Whilst [Polly](https://github.com/App-vNext/Polly) does not support a Policy that is both Circuit Breaker and Retry i.e. retry n times with an interval between each retry, and then break circuit, to implement that simply put a Circuit Breaker UsePolicy attribute as an earlier step than the Retry UsePolicy attribute. If retries expire, the exception will bubble out to the Circuit Breaker.

### Timeout (Legacy - Deprecated)

> **⚠️ DEPRECATED**: The `TimeoutPolicy` attribute is obsolete in V10 and will be removed in V11. Use `UseResiliencePipeline` with Polly's Timeout strategy instead.

You should not allow a handler that calls out to another process (e.g. a call to a Database, queue, or an API) to run without a timeout. If the process has failed, you will consume a resource in your application polling that resource. This can cause your application to fail because another process failed.

Usually the client library you are using will have a timeout value that you can set.

In some scenarios the client library does not provide a timeout, so you have no way to abort.

We provide the Timeout attribute for that circumstance. You can apply it to a Handler to force that Handler into a thread which we will timeout, if it does not complete within the required time period.

```csharp
// ...
public class EditTaskCommandHandler : RequestHandler<EditTaskCommand>
{
    private readonly ITasksDAO _tasksDAO;

    public EditTaskCommandHandler(ITasksDAO tasksDAO)
    {
        _tasksDAO = tasksDAO;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [Validation(step: 2, timing: HandlerTiming.Before)]
    [TimeoutPolicy(step: 3, milliseconds: 300)]  // ⚠️ DEPRECATED
    public override EditTaskCommand Handle(EditTaskCommand editTaskCommand)
    {
        using (var scope = _tasksDAO.BeginTransaction())
        {
            Task task = _tasksDAO.FindById(editTaskCommand.TaskId);

            task.TaskName = editTaskCommand.TaskName;
            task.TaskDescription = editTaskCommand.TaskDescription;
            task.DueDate = editTaskCommand.TaskDueDate;

            _tasksDAO.Update(task);
            scope.Commit();
        }

        return editTaskCommand;
    }
}
```

**V10 Replacement**:

```csharp
// ...
// Configure timeout pipeline
resiliencePipelineRegistry.TryAddBuilder("EditTaskTimeout",
    (builder, context) => builder.AddTimeout(TimeSpan.FromMilliseconds(300)));

// Use in handler
public class EditTaskCommandHandler : RequestHandler<EditTaskCommand>
{
    private readonly ITasksDAO _tasksDAO;

    public EditTaskCommandHandler(ITasksDAO tasksDAO)
    {
        _tasksDAO = tasksDAO;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [Validation(step: 2, timing: HandlerTiming.Before)]
    [UseResiliencePipeline("EditTaskTimeout", step: 3)]  // ✅ V10 recommended
    public override EditTaskCommand Handle(EditTaskCommand editTaskCommand)
    {
        using (var scope = _tasksDAO.BeginTransaction())
        {
            Task task = _tasksDAO.FindById(editTaskCommand.TaskId);

            task.TaskName = editTaskCommand.TaskName;
            task.TaskDescription = editTaskCommand.TaskDescription;
            task.DueDate = editTaskCommand.TaskDueDate;

            _tasksDAO.Update(task);
            scope.Commit();
        }

        return editTaskCommand;
    }
}
```

---

## Further Reading

- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) - Polly v8 resilience pipelines in Brighter today
- [Polly v7 to v8 Migration Guide](https://www.pollydocs.org/migration-v8.html)
- [V10 Migration Guide](/contents/V10MigrationGuide.md) - The wider V10 upgrade
