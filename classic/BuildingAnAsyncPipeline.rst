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

`Next <Monitoring.html>`__

`Prev <ImplementingAsyncHandler.html>`__

Brighter
========

Building a Pipeline of Async Request Handlers
---------------------------------------------

Once you are using the features of Brighter to act as a `command
dispatcher <CommandsCommandDispatcherandProcessor.html>`__ and send or
publish messages to a target handler, you may want to use its `command
processor <CommandsCommandDispatcherandProcessor.html>`__ features to
handle orthogonal operations.

Implementing a Pipeline
-----------------------

The first step in building a pipeline is to decide that we want an
orthogonal operation in our pipeline. Let us assume that we want to do
command sourcing.

Because you do not want to write an orthogonal handler for every Command
or Event type, these handlers should remain generic types. At runtime
the framework will request HandlerFactory creates an instance of the
generic type specialized for the type parameter of the Command or Event
being passed along the pipeline.

The limitation here is that you can only make assumptions about the type
you receive into the pipeline from the constraints on the generic type.

Although it is possible to implement the
`IHandleRequestsAsync <https://github.com/iancooper/Paramore/blob/master/Brighter/paramore.brighter.commandprocessor/IHandleRequestsAsync.cs>`__
interface directly, we recommend deriving your handler from
`RequestHandlerAsync<T> <https://github.com/iancooper/Paramore/blob/master/Brighter/paramore.brighter.commandprocessor/RequestHandlerAsync.cs>`__.

Let us assume that we want to log all requests travelling through the
pipeline. (We provide this for you in the
Paramore.Brighter.CommandProcessor packages so this for illustration
only). We could implement a generic handler as follows:

::

    public class CommandSourcingHandlerAsync<T> : RequestHandlerAsync<T> where T : class, IRequest
    {
        private readonly IAmACommandStoreAsync _commandStore;

        public CommandSourcingHandlerAsync(IAmACommandStoreAsync commandStore)
            : this(commandStore, LogProvider.GetCurrentClassLogger())
        { }


        public CommandSourcingHandlerAsync(IAmACommandStoreAsync commandStore, ILog logger) : base(logger)
        {
            _commandStore = commandStore;
        }

        public override async Task<T> HandleAsync(T command, CancellationToken? ct = null)
        {
            logger.DebugFormat("Writing command {0} to the Command Store", command.Id);

            await _commandStore.AddAsync(command, -1, ct).ConfigureAwait(ContinueOnCapturedContext);

            return await base.HandleAsync(command, ct).ConfigureAwait(ContinueOnCapturedContext);
        }
    }

Our HandleAsync method is the method which will be called by the
pipeline to service the request. After we log we call **return await
base.HandleAsync(command, ct)** to ensure that the next handler in the
chain is called.

If we failed to do this, the *target handler* would not be called nor
any subsequent handlers in the chain. This call to the next item in the
chain is how we support the 'Russian Doll' model - because the next
handler is called within the scope of this handler, we can manage when
it is called handle exceptions, units of work, etc.

It is worth remembering that handlers may be called after the target
handler (in essence you can designate an orthogonal handler as the sink
handler when configuring your pipeline). For this reason **\*\*all\*\***
handlers should remember to call their successor, **even \*\*your\*\*
target handler**.

We now need to tell our pipeline to call this orthogonal handler before
our target handler. To do this we use attributes. The code we want to
write looks like this:

::

    internal class GreetingCommandRequestHandlerAsync : RequestHandlerAsync<GreetingCommand>
    {
        [UseCommandSourcingAsync(step: 1, timing: HandlerTiming.Before)]
        public override async Task<GreetingCommand> HandleAsync(GreetingCommand command, CancellationToken? ct = null)
        {
            var api = new IpFyApi(new Uri("https://api.ipify.org"));

            var result = await api.GetAsync(ct);

            Console.WriteLine("Hello {0}", command.Name);
            Console.WriteLine(result.Success ? "Your public IP addres is {0}" : "Call to IpFy API failed : {0}", result.Message);
            return await base.HandleAsync(command, ct).ConfigureAwait(base.ContinueOnCapturedContext);
        }
    }

The **UseCommandSourcingAsync** Attribute tells the Command Processor to
insert a Logging handler into the request handling pipeline
before(\ **HandlerTiming.Before**) we run the target handler. It tells
the Command Processor that we want it to be the first handler to run if
we have multiple orthogonal handlers i.e. attributes (**step: 1**).

We implement the **UseCommandSourcingAsyncAttribute** by creating our
own Attribute class, derived from **RequestHandlerAttribute**.

::

    public class UseCommandSourcingAsyncAttribute : RequestHandlerAttribute
    {

        public UseCommandSourcingAsyncAttribute(int step, HandlerTiming timing = HandlerTiming.Before)
            : base(step, timing)
        { }


        public override Type GetHandlerType()
        {
            return typeof (CommandSourcingHandlerAsync<>);
        }
    }

The most important part of this implementation is the GetHandlerType()
method, where we return the type of our handler. At runtime the Command
Processor uses reflection to determine what attributes are on the target
handler and requests an instance of that type from the user-supplied
**Handler Factory**.

Your Handler Factory needs to respond to requests for instances of a
**RequestHandlerAsync<T>** specialized for a concrete type. For example,
if you create a\ **CommandSourcingHandlerAsync<TRequest>** we will ask
you for a **CommandSourcingHandlerAsync<MyCommand>** etc. Depending on
your implementation of HandlerFactory, you may need to register an
implementation for every concrete instance of your handler with your
underlying IoC container etc.

Note that as we rely on an user supplied implementation of
**IAmAHandlerFactoryAsync** to instantiate Handlers, you can have any
dependencies in the constructor of your handler that you can resolve at
runtime. In this case we pass in an ILog reference to actually log to.

You may wish to pass parameter from your Attribute to the handler.
Attributes can have constructor parameters or public members that you
can set when adding the Attribute to a target method. These can only be
compile time constants, see the documentation
`here <https://msdn.microsoft.com/en-us/library/aa664615%28v=vs.71%29.aspx>`__.
After the Command Processor calls your Handler Factory to create an
instance of your type it calls the
**RequestHandler.InitializeFromAttributeParams** method on that created
type and passes it the object array defined in the
**RequestHandlerAttribute.InitializerParams**. By this approach, you can
pass parameters to the handler, for example the Timing parameter is
passed to the handler above.

It is worth noting that you are limited when using Attributes to provide
constructor values that are compile time constants, you cannot pass
dynamic information. To put it another way you are limited to value set
at design time not at run time.

In fact, you can use this approach to pass any data to the handler on
initialization, not just attribute constructor or property values, but
you are constrained to what you can access from the context of the
Attribute at run time. It can be tempting to set retrieve global state
via the `Service
Locator <http://en.wikipedia.org/wiki/Service_locator_pattern>`__
pattern at this point. Avoid that temptation as it creates coupling
between your Attribute and global state reducing modifiability.

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

