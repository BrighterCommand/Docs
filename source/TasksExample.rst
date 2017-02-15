Tasks Example
=============

This tutorial takes you through buiding the TaskList project, which is
ToDo MVC and Reminder Mail Service via a Task Queue. The application
will manage a user's todo list, and sends notifications about imminent
or overdue tasks. The walkthrough will build the examples available in
the `Examples folder of Brighter available in the public
repo <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples>`__
if you want to follow along there.

Note that you will need to have
`RabbitMQ <https://www.rabbitmq.com/download.html>`__ installed to step
through this example as a tutorial.

Design Overview
~~~~~~~~~~~~~~~

Tasks is intended to give a slightly fuller example of the use of
Brighter to support a Web UI with supporting API and Application
Service. Whilst we show snippets of code here, you will either want to
clone Paramore from GitHub to follow along, or `browse the
code <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples>`__
on the web.

The separate components are:

-  `Tasklist
   UI <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/TaskListUI>`__:
   An ASP.NET MVC UI SPA for the Tasklist API - least interesting from a
   Brighter perspective
-  `Tasklist <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/TaskList>`__:
   An HTTP API for tasks. Hosted in OpenRasta, uses
   **Paramore.Brighter.CommandProcessor**
-  `TaskMailer <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/TaskMailer>`__:
   As service that notifies task owners of upcoming or overdue tasks,
   uses **Paramore.Brighter.ServiceActivator**
-  `Tasks <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/Tasks>`__:
   A core library that models Tasks used by both Tasklist and TaskMailer

In a production SOA application we would consider all of these
components to be part of the Tasks SOA and would be happy for these
Autonomous Components within that to share a model, as we are not
crossing an SOA boundary. If we were to cross an SOA boundary we would
not share the core library as we do here.

Another way of stating this is these components are logically within a
single bounded context, even though they are physically separate and we
are using messaging to support Work Queues not integration SOA
boundaries. We could use Brighter for SOA integration, but we would not
share a model in that case, as a Service should share schema not type,
and have explicit boundaries.

Prerequisites
~~~~~~~~~~~~~

We will assume you are familiar with:

-  `Hello
   World <http://iancooper.github.io/Paramore/HelloWorldExample.html>`__
   and
   `Greetings <http://iancooper.github.io/Paramore/GreetingsExample.html>`__
   examples
-  can add
   `paramore.brighter.ServiceActivator <http://iancooper.github.io/Paramore/GreetingsExample.html>`__
   and
   `paramore.brighter.ComandProcessor <http://iancooper.github.io/Paramore/HelloWorldExample.html>`__
   components
-  can `build/configure building paramore.brighter.CommandProcessor and
   Policies <http://iancooper.github.io/Paramore/GreetingsExample.html>`__
-  understand `message
   Mappers <http://iancooper.github.io/Paramore/GreetingsExample.html>`__
-  and can configure `TinyIoc <https://github.com/grumpydev/TinyIoC>`__

Component Description
---------------------

Tasklist UI
~~~~~~~~~~~

A demo Web UI based upon todoMVC to offer a SPA-like representation of
the TaskList API data

Provides the following functionality:

-  Get all Tasks
-  Complete a Task
-  Send Reminder for a Task

Hosted within iisExpress, uses MVC API to host SPA website written using
jQuery, mustache and bootstrap

Where does Brighter fit in?
^^^^^^^^^^^^^^^^^^^^^^^^^^^

The UI acts as a dumb client to the Tasklist service. As such the
`app.js <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskListUI/app/app.js>`__
file that contains the core logic issues GETs/POSTs that are processed
by the TaskList API.

Tasklist (API)
~~~~~~~~~~~~~~

Tasklist uses a Ports and Adapters architecture and the structure of the
assembly surfaces that architectural style.

Tasklist is responsible for providing API endpoints to support a UI
implemented as a thin OpenRasta-API veneer to our Core Domain

TaskEndPointHandler
^^^^^^^^^^^^^^^^^^^

Responsible for REST operations on Task resources

REST Operation

Port

Description

GET /

ITaskListRetriever

Returns all tasks from the TaskList using a Port against our preferred
Thin-Read model - a Retriever

GET /taskId

ITaskRetriever

Returns a specified task from the TaskList using a Port against our
preferred Thin-Read model - a Retriever

POST {TaskModel}

AddTaskCommandHandler

Adds a new task specified by the TaskModel a Port against our preferred
Domain-Write model - a **IHandleRequests<TRequest>**

DELETE /taskId

CompleteTaskCommandHandler

Completes a given task specified by the taskId with a Port against our
preferred Domain-Write model - a **IHandleRequests<TRequest>**

. A command is therefore dispatched to the CommandProcessor

TaskReminderEndpointHandler
^^^^^^^^^^^^^^^^^^^^^^^^^^^

Responsible for REST operations on Task Reminder resources. Implemented
as a shallow Adaptor to transformm POSTed arguments into Commands that
will be handled by a Port

It should be noted the Handler contains no Port for handling the
command. This will be processed by the separate TaskMailer Service.

REST Operation

Mapper/Port

Description

POST {TaskReminderModel}

n/a

Receives the arguments via HTTP and posts a TaskReminderCommand

Where does Brighter fit in?
^^^^^^^^^^^^^^^^^^^^^^^^^^^

Once `OpenRasta <http://openrasta.org/>`__ unpacks the Request (in
`TaskEndPointHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Handlers/TaskEndPointHandler.cs>`__
and
`TaskReminderEndpointHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Handlers/TaskReminderEndpointHandler.cs>`__)
the relevant Port is invoked.

On the Read-side this is a View Model retriever, and in TaskList this
derives from
`SimpleDataRetriever <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Ports/ViewModelRetrievers/SimpleDataRetriever.cs>`__.

The Write-side will invoke a Handler in Tasklist that maps to
IHandle<TaskReminderCommand> or IHandle<TaskReminderCommand>. These
handlers can be found in Tasks
`MailTaskReminderHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/Tasks/Ports/MailTaskReminderHandler.cs>`__
and
`AddTaskCommandHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/Tasks/Ports/Handlers/AddTaskCommandHandler.cs>`__.

TaskMailer
~~~~~~~~~~

A separate service responsible for sending emails using Azure SendGrid

REST Operation

Port

Description

POST {TaskReminderModel}

TaskReminderCommandMessageMapper

API to receive the POST to send a Reminder. The TaskReminderCommand is
then mapped through TaskReminderCommandMessageMapper to demonstrate
Message Transformation

MailTaskReminderHander

Processes transformed message and initiates Mail Sending using the
MailGateway.

Where does Brighter fit in?
^^^^^^^^^^^^^^^^^^^^^^^^^^^

A standalone service that implements Brighter's Service Activator. This
allows `mapping of closures to commands (via IoC), specifying Policy and
building Dispatcher and
CommandProcessor <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskMailer/Adapters/ServiceHost/TaskMailerService.cs>`__.

The TaskMailService class achieves all of this in < 100 lines, thanks to
`Paramore.Brighter.ServiceActivator <https://www.nuget.org/packages/paramore.brighter.serviceactivator/>`__

Tasks
~~~~~

Tasks is the Core Domain model using a **`Ports and Adapters
architecture <PortsAndAdapters.html>`__** and the structure of the
assembly surfaces that architectural style.

The **Ports** folder contains a folder for our **Handlers**, which
implement **IHandleRequests<TRequest>**. These form the ports into our
application.

Where does Brighter fit in?
^^^^^^^^^^^^^^^^^^^^^^^^^^^

This is a library component of TaskList containing core domain. As such
Commands are here (and referenced by consumers/producers internal to the
SOA boundary). Core Ports are also held here along with Adaptors.

As such **Paramore.Brighter.ComamndProcessor** is referenced to allow
coding Write-side ports, and to adapt any incoming Commands/Events.

Walkthrough - Get Tasks
-----------------------

UI
~~

The UI loads jquery and fires a js request to the hosted API (at
``localhost:49743``). Almost all code is in a singular js file... a
snippet shows the expected jQuery code:

::

    var taskVm = function () {
        var baseUri = 'http://localhost:49743/tasks';
        var getTasksInternal = function(getCallback) {
            $.ajax({
                url: baseUri,
                dataType: 'json',
                type: 'GET',
                success: function(data) { getCallback(data); }
            });
        };
        ...
        }
        return {
            getTasks: getTasksInternal,
            ...
        };
    }();

TaskList API
~~~~~~~~~~~~

Once received, an OpenRasta translates the GET and invokes the
configured handler,
`TaskEndPointHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Handlers/TaskEndPointHandler.cs>`__.

::

    [HttpOperation(HttpMethod.GET)]
    public OperationResult Get()
    {
        TaskListModel responseResource = _taskListRetriever.RetrieveTasks();
        return new OperationResult.OK { ResponseResource = responseResource };
    }

Our read-side is simple, with no adaptor needed (as no input is passed
over HTTP). Therefore we can simply invoke the read-side Port, the
`TaskList View Model
Retriever <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Ports/ViewModelRetrievers/TaskListRetriever.cs>`__:

TaskList View Model Retriever
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

::

    public dynamic RetrieveTasks()
    {
        var db = Database.Opener.OpenFile(DatabasePath);
        var tasks = db.Tasks.All().ToList();
        var taskList = new TaskListModel(tasks, _hostName);
        return taskList;
    }

Walkthrough - Add a Task
------------------------

UI
~~

The javascript ViewModel sends a js request to the hosted API (at
``localhost:49743``):

::

    var taskVm = function () {
        ...
        var addTaskInternal = function(taskText, addCallback) {
            $.ajax({
                url: baseUri,
                dataType: 'text', //to process location, not json
                type: 'POST',
                success: function(data) { addCallback(data); },
                contentType: "application/json",
                data: '{"dueDate": "' + dueDateFixed + '", "taskDescription": "' + taskText + '", "taskName": "' + taskText + '"}'
            });
        };
        ...
        }
        return {
            addTask: addTaskInternal,
            ...
        };
    }();

TaskList API
~~~~~~~~~~~~

Again OpenRasta translates the POST and invokes the configured handler,
`TaskEndPointHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Handlers/TaskEndPointHandler.cs>`__.

Our write-side is more involved. The received data is transformed into a
Command that can be understood by the core domain (AddTaskCommand. The
command is then posted to the CommandProcessor to locate the approptiate
handler:

::

    [HttpOperation(HttpMethod.POST)]
    public OperationResult Post(TaskModel newTask)
    {
        var addTaskCommand = new AddTaskCommand(
            taskName: newTask.TaskName,
            taskDescription: newTask.TaskDescription,
            dueDate: DateTime.Parse(newTask.DueDate)
            );

        _commandProcessor.Send(addTaskCommand);

        return new OperationResult.Created
        {
            RedirectLocation = new Uri(string.Format("{0}/tasks/{1}", _communicationContext.ApplicationBaseUri, addTaskCommand.TaskId))
        };
    }

The relevant handler
(`AddTaskCommandHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/Tasks/Ports/Handlers/AddTaskCommandHandler.cs>`__)
will handle the command (as it was configured to handle AddTaskCommand
in the `TaskList dependency
registrar <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Configuration/DependencyRegistrar.cs>`__.

The handler is very simple - simpyl adding the given state using the
TasksDao:

::

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [Validation(step: 2, timing: HandlerTiming.Before)]
    [UsePolicy(CommandProcessor.RETRYPOLICY, step: 3)]
    public override AddTaskCommand Handle(AddTaskCommand addTaskCommand)
    {
        using (var scope = _tasksDAO.BeginTransaction())
        {
            var inserted = _tasksDAO.Add(
                new Task(
                    taskName: addTaskCommand.TaskName,
                    taskDecription: addTaskCommand.TaskDescription,
                    dueDate: addTaskCommand.TaskDueDate
                    )
                );

            scope.Commit();

            addTaskCommand.TaskId = inserted.Id;
        }

        return addTaskCommand;
    }

The TasksDAO is an abstraction over the sqlce datastore for Tasks and
uses `simple.data <https://github.com/markrendle/Simple.Data>`__ to
access the database.

Walkthrough - Mail a reminder
-----------------------------

UI
~~

From the UI 'completing' a task is modelled as a DELETE being issued to
the relevant tasks endpoint:

::

    var taskVm = function () {
        ...
        var completeTaskInternal = function(taskId, completeCb) {
            $.ajax({
                url: baseUri + '/' + taskId,
                dataType: 'text',
                type: 'DELETE',
                success: function(data) { completeCb(data); }
            });
        };
        ...
        }
        return {
            completeTask: completeTaskInternal,
            ...
        };
    }();

TaskList API
~~~~~~~~~~~~

OpenRasta translates the DELETE and invokes the configured handler,
`TaskReminderEndPointHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskList/Adapters/API/Handlers/TaskReminderEndPointHandler.cs>`__.

The received data is transformed into a Command that can be understood
by the core domain (TaskReminderCommand). The command is then posted tp
the CommandProcessor to locate the approptiate handler.

::

    [HttpOperation(HttpMethod.POST)]
    public OperationResult Post(TaskReminderModel reminder)
    {
        var reminderCommand = new TaskReminderCommand(
            taskName: reminder.TaskName,
            dueDate: DateTime.Parse(reminder.DueDate),
            recipient: reminder.Recipient,
            copyTo: reminder.CopyTo
            );
        _commandProcessor.Post(reminderCommand);

        return new OperationResult.OK(){StatusCode = (int)HttpStatusCode.Accepted};
    }

The relevant handler
(`MailTaskReminderHandler <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/Tasks/Ports/MailTaskReminderHandler.cs>`__)
will pick up the command (as it was configured to handle
TaskReminderCommand in the `TaskMailerService (that handles it's own
dependency
registration) <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/TaskMailer/Adapters/ServiceHost/TaskMailerService.cs>`__.

The handler is very simple - it invokes the MailGateway to send the
email:

::

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    [UsePolicy(CommandProcessor.CIRCUITBREAKER, step: 2)]
    [UsePolicy(CommandProcessor.RETRYPOLICY, step: 3)]
    public override TaskReminderCommand Handle(TaskReminderCommand command)
    {
        _mailGateway.Send(new TaskReminder(
            taskName: new TaskName(command.TaskName),
            dueDate: command.DueDate,
            reminderTo: new EmailAddress(command.Recipient),
            copyReminderTo: new EmailAddress(command.CopyTo)
            ));

        return base.Handle(command);
    }

The resulting functionality is exposed as a 'ToDo MVC' clone. The real
value is understanding the Paramore concepts at play. |image0|

.. |image0| image:: images/todo.png

