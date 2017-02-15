Getting Started
===============

Brighter is intended to be a library and not a framework, so you can use
it with ASP.NET MVC or WebAPI, Nancy, OpenRasta, Topshelf etc. You use
it to decouple your Invoker and Receiver via a Command without coupling
the Invoker to a concrete Command. In addition, you can support
orthogonal requirements such as Retry, Timeout, Circuit Breaker and
Logging without polluting your domain code. Finally, we let you defer
work that does not need synchronous execution to a task queue, using a
Broker architectural style so that this is seamless to the invoker.

Requirements
------------

Brighter depends on .NET 4.5.

It should be possible to support earlier versions once the build process
for the project improves from its mostly manual proposition today. It's
on the roadmap.

Brighter uses a number of NuGet packages

The core package **Paramore.Brighter.CommandProcessor** depends on:

-  `LibLog <https://github.com/damianh/LibLog>`__
-  `Json.NET <http://james.newtonking.com/json>`__
-  `Polly <https://github.com/michael-wolfenden/Polly>`__

The major pain point here is the Json.NET dependency as it is likely
other projects you depend on will also pull in this project. If you have
conflicts, try using `Assembly
Redirection <https://msdn.microsoft.com/en-us/library/7wd6ex19%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396>`__
to resolve. It is less likely that you will have conflicts with Polly,
but again use the strategy above. In a worst case scenario, fork the
code, rebuild with the required version, fix any issues, and deploy as a
lib not a package.

We support LibLog for logging by Brighter.\* projects with the namespace
**paramore.brighter.commandprocessor.Logging** and the interface
**ILog**.

The core package **Paramore.Brighter.ServiceActivator** depends on:

-  `Polly <https://github.com/michael-wolfenden/Polly>`__

In addition a number of the packages implement interfaces provided by
CommandProcessor and depend specific NuGet packages for those
implementations.

**Brighter.CommandProcessor.MessageStore.RavenDB** depends on

-  `RavenDB Client <http://ravendb.net/>`__

**Brighter.CommandProcessor.MessageStore.MSSql** depends on

-  `Microsoft SQL Server Compact
   Edition <https://www.nuget.org/packages/Microsoft.SqlServer.Compact>`__
-  `Json.NET <http://james.newtonking.com/json>`__

**Brighter.CommandProcessor.MessagingGateway.RMQ** depends on

-  `RabbitMQ.Client <http://www.rabbitmq.com/dotnet.html>`__
-  `Json.NET <http://james.newtonking.com/json>`__
-  `Polly <https://github.com/michael-wolfenden/Polly>`__

**Brighter.CommandProcessor.MessagingGateway.RestMS** depends on

-  `Thinktecture.IdentityModel.Hawk <https://github.com/thinktecture>`__

Get the Library
---------------

The Brighter packages are available on
`NuGet <http://www.nuget.org/packages?q=brighter>`__ by searching for
Brighter

The following packages are currently available:

-  **Paramore.Brighter.CommandProcessor** - the core library, provides a
   `Command Dispatcher and Command
   Processor <CommandsCommandDispatcherandProcessor.html>`__
-  **Paramore.Brighter.ServiceActivator** - provides consumer support
   for deferring long-running work to a `task/work
   queue <https://www.rabbitmq.com/tutorials/tutorial-two-python.html>`__
   The core library provides support for the producer
-  **Paramore.Brighter.MessagingGateway.RMQ** - provides support for
   using RabbitMQ as a broker
-  **Paramore.Brighter.MessageStore.RavenDB** - provides support for
   using RavenDB to replay messages sent to the broker
-  **Paramore.Brighter.MessageStore.MSSQL** - provides support for using
   MSSql to replay messages sent to the broker

First Steps
-----------

First Steps provides a simple sample to handle a command message
synchronously

-  `Hello World <HelloWorldExample.html>`__

Next Steps
----------

Afterwards Next Steps looks at the examples provided and discusses event
publishing, and asynchronous events via a Broker

-  `Greetings <GreetingsExample.html>`__
-  `Tasks <TasksExample.html>`__

Configuration
-------------

As ever with this kind of library, configuration is one of the most
painful parts. We try to ease the pain through a fluent interface

-  `Configuration <BasicConfiguration.html>`__
