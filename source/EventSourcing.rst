Event Sourcing
--------------

If we dispatch commands to a target handler, and have a pipeline which
acts as a preprocessor one obvious orthogonal operation is to log our
commands so that we can understand the commands that result in current
system state. We can examine the logs, particularly if there is a
problem, to understand what commands were sent to the application to
create the current state.

We can also infer that if the current application state is a function of
those commands, then we could potentially recreate the same application
state by replaying those commands.

Now, within a log file, we are going to have to fiddle with
`sed <http://www.grymoire.com/Unix/Sed.html>`__ or
`awk <http://www.grymoire.com/Unix/Awk.html>`__ to pull out the commands
into a text file and then run those through something that reposts them.
That works but it can be a little awkward and as such is a barrier to
entry.

So if we stored these to a Command Store it could be much easier to
slice and dice the data, and to extract it for replay.

In his bliki on `Event
Sourcing <https://martinfowler.com/eaaDev/EventSourcing.html>`__ Martin
Fowler describes using an architecture that "guarantee[s] that all
changes to the domain objects are initiated by the event objects" and
one implementation approach is that an event processor sequentially logs
the event which is then applied to the domain. The system of record can
be either the events (perhaps rebuilt overnight) or application state
(in which case the events are only used for analysis or recovery.

Brighter has an event processor equivalent, as it is a command processor
and dispatcher, so it only needs to persist the commands to enable
support for Event Sourcing. So if you make all your changes to the
domain through a command dispatcher such as Brighter you can meet the
requirements for Event Sourcing by persisting your commands in a way
that facilitates querying or replay. As Brighter has a pipeline through
it's command processor it is natural to simply add an attribute to the
target handler that persists the command before it is applied to domain
model. This is essentially a write-ahead log.

Martin lists some issues to consider: new features, defect fixes, and
temporal logic. A particular issue is external gateways.

Command or Event Sourcing
-------------------------

One complaint about Martin's article is that a Command is the intent to
change the system, but an event is the result of that change. Because
this model records the change to state many people prefer to refer to
this approach as Command Sourcing and reserve Event Sourcing for `Greg
Young's related
idea <https://cqrs.wordpress.com/documents/events-as-storage-mechanism/>`__
of storing the results of those commands, the changes that would be made
to application state, so that those changes can be replayed instead of
the commands, that could have side affects. We don't explicitly provide
help for that approach.

For clarity we will use the term Command Sourcing for Brighter's support
for Martin Fowler's description of `Event
Sourcing <https://martinfowler.com/eaaDev/EventSourcing.html>`__

Command Sourcing in Brighter
----------------------------

Brighter supports Command Sourcing through the use of its
**UseCommandSourcingAttribute**. By adding the attribute to a handler
you gain support for logging that **Command** to a **Command Store** A
Command Store needs to implement **IAmACommandStore** and we provide an
`MSSQL Command
Store <https://github.com/BrighterCommand/Brighter/tree/master/Brighter.commandprocessor.commandstore.mssql>`__
implementation. You can choose to persist the Command to the Store
before or after the handler. We recommend Before as this gives you the
assurance that if writing the Command to the Store fails, the Handler
will not run, meaning that your Store reflects your application state.

The following code shows a handler marked up for Command Sourcing

.. highlight:: csharp

::

     internal class GreetingCommandHandler : RequestHandler<GreetingCommand>
     {
        [UseCommandSourcing(step: 1, timing: HandlerTiming.Before)]
        public override GreetingCommand Handle(GreetingCommand command)
        {
            Console.WriteLine("Hello {0}", command.Name);
            return base.Handle(command);
        }
     }


Inerrnally the `Monitor
Handler <https://github.com/BrighterCommand/Brighter/blob/master/src/Paramore.Brighter/Monitoring/Handlers/MonitorHandler.cs>`__
that Brighter uses to write to the Command Store takes a reference to an
**IAmACommandStore**, so you also need to configure your application to
provide an implementation at runtime when you provide instances of the
Handler from your Handler Factory implementation. The example code
relies on the TinyIoC Inversion of Control container to hookup the
Handler and Command Store.

.. highlight:: csharp

::

    private static void Main(string[] args)
    {
        var dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8)), "App_Data\\CommandStore.sdf");
        var connectionString = "DataSource=\"" + dbPath + "\"";
        var configuration = new MsSqlCommandStoreConfiguration(connectionString, "Commands", MsSqlCommandStoreConfiguration.DatabaseType.SqlCe);
        var commandStore = new MsSqlCommandStore(configuration);

        var registry = new SubscriberRegistry();
        registry.Register<GreetingCommand, GreetingCommandHandler>();

        var tinyIoCContainer = new TinyIoCContainer();
        tinyIoCContainer.Register<IHandleRequests<GreetingCommand>, GreetingCommandHandler>();
        tinyIoCContainer.Register<IAmACommandStore>(commandStore);

        var builder = CommandProcessorBuilder.With()
            .Handlers(new HandlerConfiguration(
                subscriberRegistry: registry,
                handlerFactory: new TinyIocHandlerFactory(tinyIoCContainer)
            ))
            .DefaultPolicy()
            .NoTaskQueues()
            .RequestContextFactory(new InMemoryRequestContextFactory());

            var commandProcessor = builder.Build();

            var greetingCommand = new GreetingCommand("Ian");

            commandProcessor.Send(greetingCommand);

            var retrievedCommand = commandStore.Get<GreetingCommand>(greetingCommand.Id).Result;

            var commandAsJson = JsonConvert.SerializeObject(retrievedCommand);

            Console.WriteLine(string.Format("Command retrieved from store: {0}", commandAsJson));

            Console.ReadLine();
    }

The example code also shows retrieving the command from the store, using
the **IAmACommandStore.Get** method, passing in the Id of the Command.

The retrieved command could be replayed, although in this case we simply
log it to the console.
