Paramore
========

Libraries and supporting examples for use with the Ports and Adapters
and CQRS architectural styles for .NET, with support for Task Queues

`View the Project on GitHub
iancooper/Paramore <https://github.com/iancooper/Paramore>`__

-  `Download **ZIP
   File** <https://github.com/iancooper/Paramore/zipball/master>`__
-  `Download **TAR
   Ball** <https://github.com/iancooper/Paramore/tarball/master>`__
-  `View On **GitHub** <https://github.com/iancooper/Paramore>`__

`Paramore Home <../index.html>`__

`Brighter Home <Brighter.html>`__

`Prev <CommandsCommandDispatcherandProcessor.html>`__

Brighter
========

Decoupling via a Command Dispatcher
-----------------------------------

When we think about a
`layered <http://domaindrivendesign.org/node/118>`__\ or
`hexagonal <http://alistair.cockburn.us/Hexagonal+architecture>`__\ architecture
it is common to identify the need for a service layer. The service layer
both provides a facade over our domain layer to applications acting as
an API definition and contains the co-ordination and control logic for
orchestrating how we respond to requests.

One option to implement this is the notion of a service as a class:

::

    public class MyFatDomainService
    {
        public void CreateMyThing(
                CreateMyThingCommand createMyThingCommand)
        {
            /*Stuff*/
        }

        public void UpdateMyThingForFoo(
                FooCommand fooHappened)
        {
            /*Other Stuff*/
        }

        public void UpdateMyThingForBar(
                BarCommand barHappened)
        {
            /*Other Stuff*/
        }

        /*Loads more of these*/
    }
            

Another option is to use the `Command Dispatcher
pattern <http://hillside.net/plop/plop2001/accepted_submissions/PLoP2001/bdupireandebfernandez0/PLoP2001_bdupireandebfernandez0_1.pdf>`__.
There are some keys to understanding the choice to use a Command
Dispatcher to implement your service layer.

The `Interface Segregation
Principle <http://www.objectmentor.com/resources/articles/isp.pdf>`__
states that clients should not be forced to depend on methods on an
interface that they do not use. This is because we do not want to update
the client because the interface changes to service other clients in a
way that the client itself does not care about. *Operation script* style
domain service classes force consumers (for example MVC controllers) to
become dependent on methods on the domain service class that they do not
consume.

Now this can be obviated by having the domain service implement a number
of interfaces, and hand to its clients interfaces that only cover the
concerns they have. With application service layers this naturally tends
towards one method per interface.

::

    public interface ICreateMyThingDomainService
    {
        void CreateMyThing(
                CreateMyThingCommand createMyThingCommand);
    }

    public interface IUpdateMyThingForFooDomainService
    {
        void UpdateMyThingForFoo(
                FooCommand fooHappened);
    }

    public interface IUpdateMyThingForBarDomainService
    {
        void UpdateMyThingForBar(
                BarCommand barHappened);
    }

    public class MyFatDomainService :
                ICreateMyThingDomainService,
                IUpdateMyThingForFooDomainService,
                IUpdateMyThingForBarDomainService
    {
        public void CreateMyThing(
                CreateMyThingCommand createMyThingCommand)
        {
            /*Stuff*/
        }

        public void UpdateMyThingForFoo(
                FooCommand fooHappened)
        {
            /*Other Stuff*/
        }

        public void UpdateMyThingForBar(
                BarCommand barHappened)
        {
            /*Other Stuff*/
        }

        /*Loads more of these*/

    }
            

Now the `Single Responsibility
Principle <http://www.objectmentor.com/resources/articles/srp.pdf>`__
suggests that a class should have one and only one reason to change. All
these separate interfaces begin to suggest that a separate class might
be better for each interface, to avoid updating a class for concerns
that it does not have.

In addition, a single service class results in our class collecting
depdendencies for all its methods. Where we have an explosion of
dependencies for our service, it can be hard to get our service under
test, or makes the tests unintelligible and results in `anti-patterns
like
auto-mocking <http://altnetseattle.pbworks.com/w/page/12367942/Why%20We%20Stopped%20Using%20the%20Auto-Mocking%20Container%20and%20What%27s%20Next>`__.
The need for auto-mocking may be seen as a design smell: you have too
many dependencies; the resolution might be to use a Command Dispatcher.

::

    public interface ICreateMyThingDomainService
    {
        void CreateMyThing(
                CreateMyThingCommand createMyThingCommand);
    }

    public class CreateMyThingDomainService :
                ICreateMyThingDomainService
    {
        public void CreateMyThing(
                CreateMyThingCommand createMyThingCommand)
        {
            /*Stuff */
        }
    }

    public interface IUpdateMyThingForFooDomainService
    {
        void UpdateMyThingForFoo(FooCommand fooHappened);
    }

    public class UpdateMyThingForFooDomainService :
                IUpdateMyThingForBarDomainService
    {
        public void UpdateMyThingForBar(
                BarCommand barHappened)
        {
            /*Other Stuff*/
        }
    }

    public interface IUpdateMyThingForFooDomainService
    {
        void UpdateMyThingForBar(FooCommand barHappened);
    }

    public class UpdateMyThingForFooDomainService :
                IUpdateMyThingForFooDomainService
    {
        public void UpdateMyThingForFoo(
                FooCommand barHappened)
        {
            /*Other Stuff*/
        }
    }
            

Having split these individual classes out we might choose to avoid
calling them directly, but instead decide to send a message to them.
There are a number of reasons for this.

The first is that we decouple the caller from the service. This is
useful where we might want to change what the service does – for example
handle requests asynchronously, without modifying the caller.

This also serves to reduce the number of interfaces that we must
implement as the generic interface can stand in for most of them.

 

::

    public interface IHandleMessages<T>
    {
        void Handle(T command);
    }

    public class CreateMyThingHandler : IHandleMessages
    {
        public void Handles(
                CreateMyThingCommand createMyThingCommand)
        {
            /*Stuff */
        }
    }
            

We gain some dependency advantages from the split into separate
handlers, because each handler will have fewer dependencies than a
service. But in addition we can separate concerns in our handlers, such
that we focus on updating a small part of our domain model or object
graph in each handler (in DDD terms we focus on an
`Aggregate <http://domaindrivendesign.org/node/88>`__).

By restricting a handler to updating one Aggregate we can treat the
handler as a transactional boundary. We initialize the state of our
model by reading from the backing store, update one Aggregate in the
model, and commit the changes to the backing store. If we need to notify
other Aggregates of this change, because they need to updated in an
eventually consistent fashion then we can publish a message (a Domain
Event) from the service that handles the initial request. Publish calls
zero to many handlers that update aggregates that care about this change
in their own transaction and consistency boundary. Because the Command
Dispatcher passes the event to those handlers, we do not have any direct
dependency on them, reducing our coupling.

::

    public class CreateMyThingHandler : IHandleMessages
    {
        IProcessCommands _commandProcessor;
        IMyThingRepository _myThingRepository;

        public CreateMyThingHandler(
            IProcessCommands commandProcessor,
            IMyThingRepository myThingRepository
                )
        {
            _commandProcessor = commandProcessor;
            _myThingRepository = myThingRepository;
        }

        public void Handles(
            CreateMyThingCommand createMyThingCommand)
        {
            /*Use Factory or Factory Method to create a my thing  */
             /*save my thing to a repository*/

            _commandProcessor.Publish(
                new MyThingCreated
                    {/* properties that other consumers care about*/}
                );
        }
    }
            

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

