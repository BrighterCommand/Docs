# Supporting Logging

## Logger

We use [LibLog](https://github.com/damianh/LibLog) so that we do not
have to depend on a given logging library. You will want to add a
logger, supported by LibLog to your project (as per the instructions for
LibLog) if you want to see log output from Brighter.

## Testing

LibLog can either keep the ILog used by the client library private to
the library, or allow the client library to expose it. Brighter chooses
to expose the ILog implementation that we receive from ILog.

Why?

When testing you may want to avoid a dependency on the static logger
that LibLog wraps your current logging framework with, as static
variables can make tests behave erratially when run in parallel, or as
part of a suite. For this reason we tend to add a constructor that
explicitly takes an instance of Brighter\'s ILog to allow you to provide
an [Test Double](https://en.wikipedia.org/wiki/Test_double) of that ILog
implementation.

``` csharp
[Subject(typeof(PipelineBuilder<>))]
public class When_Building_A_Handler_For_A_Command
{
    private static PipelineBuilder<MyCommand> s_chain_Builder;
    private static IHandleRequests<MyCommand> s_chain_Of_Responsibility;
    private static RequestContext s_request_context;

    private Establish _context = () =>
    {
        var logger = A.Fake<ILog>();
        var registry = new SubscriberRegistry();
        registry.Register<MyCommand, MyCommandHandler>();
        var handlerFactory = new TestHandlerFactory<MyCommand, MyCommandHandler>(() => new MyCommandHandler(logger));
        s_request_context = new RequestContext();

        s_chain_Builder = new PipelineBuilder<MyCommand>(registry, handlerFactory, logger);
    };

    private Because _of = () => s_chain_Of_Responsibility = s_chain_Builder.Build(s_request_context).First();

    private It _should_have_set_the_context_on_the_handler = () => s_chain_Of_Responsibility.Context.ShouldNotBeNull();
    private It _should_use_the_context_that_we_passed_in = () => s_chain_Of_Responsibility.Context.ShouldBeTheSameAs(s_request_context);
}
```

However, outside of testing, you rarely need to provide the instance of
ILog. This is because in the base class **RequestHandler\<T\>** the
default constructor simply grabs the current logger from LibLog.

``` csharp
// Initializes a new instance of the  class.
protected RequestHandler()
: this(LogProvider.GetCurrentClassLogger())
{}
```

The only time you might want to change this, is if you intend to call a
more derived constructor in your own code, that contains the ILog used
by testing. In this case, you can still keep ILog out of your code, by
providing the call to LibLog yourself
(**LogProvider.GetCurrentClassLogger()**)

``` csharp
public MailTaskReminderHandler(IAmAMailGateway mailGateway, IAmACommandProcessor commandProcessor)
    : this(mailGateway, commandProcessor, LogProvider.GetCurrentClassLogger())
{}

public MailTaskReminderHandler(IAmAMailGateway mailGateway, IAmACommandProcessor commandProcessor, ILog logger)
    : base(logger)
{
    _mailGateway = mailGateway;
    _commandProcessor = commandProcessor;
}
```
