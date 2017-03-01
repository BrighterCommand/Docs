Supporting Retry and Circuit Breaker
-------------------------------------

Brighter is a `Command
Processor <CommandsCommandDispatcherandProcessor.html>`__ and supports a
`pipeline of Handlers to handle orthogonal
requests <BuildingAPipeline.html>`__.

Amongst the valuable uses of orthogonal requests is patterns to support
Quality of Service in a distributed environment: `Timeout, Retry, and
Circuit Breaker <QualityOfServicePatterns.html>`__.

Even if you don't believe that you are writing a distributed system that
needs this protection, consider that as soon as you have multiple
processes, such as a database server, you are.

Brighter uses `Polly <https://github.com/App-vNext/Polly>`__ to
support Retry and Circuit-Breaker. Through our `Russian Doll
Model <BuildingAPipeline.html>`__ we are able to run the target handler
in the context of a Policy Handler, that catches exceptions, and applies
a Policy on how to deal with them.

Using Brighter's UsePolicy Attribute
------------------------------------

By adding the **UsePolicy** attribute, you instruct the Command
Processor to insert a handler (filter) into the pipeline that runs all
later steps using that Polly policy.

.. highlight:: csharp

::

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


To configure the Polly policy you use the PolicyRegistry to register the
Polly Policy with a name. At runtime we look up that Policy by name.

.. highlight:: csharp

::

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


When creating policies, refer to the
`Polly <https://github.com/App-vNext/Polly>`__ documentation.

Whilst `**Polly** <https://github.com/App-vNext/Polly>`__ does
not support a Policy that is both Circuit Breaker and Retry i.e. retry n
times with an interval between each retry, and then break circuit, to
implement that simply put a Circuit Breaker UsePolicy attribute as an
earlier step than the Retry UsePolicy attribute. If retries expire, the
exception will bubble out to the Circuit Breaker.

Retry and Circuit Breaker with Task Queues
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

When posting a request to a `Task
Queue <ImplementingDistributedTaskQueue.html>`__ we mandate use of a
Polly policy to control Retry and Circuit Breaker in case the output
channel is not available. These are configured using the constants:
**Paramore.RETRYPOLICY** and **Paramore.CIRCUITBREAKER**

Timeout
-------

You should not allow a handler that calls out to another process (e.g. a
call to a Database, queue, or an API) to run without a
`timeout <QualityOfServicePatterns.html>`__. If the process has failed,
you will consumer a resource in your application polling that resource.
This can cause your application to fail because another process failed.

Usually the client library you are using will have a timeout value that
you can set.

In some scenarios the client library does not provide a timeout, so you
have no way to abort.

We provide the Timeout attribute for that circumstance. You can apply it
to a Handler to force that Handler into a thread which we will timeout,
if it does not complete within the required time period.

.. highlight:: csharp

::

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
