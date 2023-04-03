# Supporting Retry and Circuit Breaker

Brighter is a [Command Processor](https://www.goparamore.io/control-bus-and-data-bus/) and
supports a [pipeline of Handlers to handle orthogonal requests](BuildingAPipeline.html).

Amongst the valuable uses of orthogonal requests is patterns to support Quality of Service in a distributed environment: [Timeout, Retry, and Circuit Breaker](PolicyRetryAndCircuitBreaker.html#using-brighter-s-usepolicy-attribute).

Even if you don't believe that you are writing a distributed system that needs this protection, consider that as soon as you have multiple processes, such as a database server, you are distributed.

Brighter uses [Polly](https://github.com/App-vNext/Polly) to support Retry and Circuit-Breaker. Through our [Russian Doll Model](BuildingAPipeline.html) we are able to run the target handler in
the context of a Policy Handler, that catches exceptions, and applies a Policy on how to deal with them.

## Using Brighter's UsePolicy Attribute

By adding the **UsePolicy** attribute, you instruct the Command Processor to insert a handler (filter) into the pipeline that runs all later steps using that Polly policy.

``` csharp
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    static MyQoSProtectedHandler()
    {
        ReceivedCommand = false;
    }

    [UsePolicy(policy: "MyExceptionPolicy", step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        /*Do work that could throw error because of distributed computing reliability*/
    }
}
```

To configure the Polly policy you use the PolicyRegistry to register the Polly Policy with a name. At runtime we look up that Policy by name.

``` csharp
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

``` csharp
var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(
		1, TimeSpan.FromMilliseconds(500));

policyRegistry.Add("MyCircuitBreakerPolicy", policy);
```

then you can add them both to your handler as follows:

``` csharp
internal class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    static MyQoSProtectedHandler()
    {
        ReceivedCommand = false;
    }

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

## Timeout

You should not allow a handler that calls out to another process (e.g. a call to a Database, queue, or an API) to run without a timeout. If the process has failed, you will consumer a resource in your application
polling that resource. This can cause your application to fail because another process failed.

Usually the client library you are using will have a timeout value that you can set.

In some scenarios the client library does not provide a timeout, so you have no way to abort.

We provide the Timeout attribute for that circumstance. You can apply it to a Handler to force that Handler into a thread which we will timeout, if it does not complete within the required time period.

``` csharp
public class EditTaskCommandHandler : RequestHandler<EditTaskCommand>
{
    private readonly ITasksDAO _tasksDAO;

    public EditTaskCommandHandler(ITasksDAO tasksDAO)
    {
        _tasksDAO = tasksDAO;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [Validation(step: 2, timing: HandlerTiming.Before)]
    [TimeoutPolicy(step: 3, milliseconds: 300)]
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
