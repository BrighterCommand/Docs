// Phase 8, ruling (d). ReactorAndProactor.md § Handler and Mapper Requirements: "Mixing sync and async handlers or
// middleware will cause runtime errors", and § What Happens If You Mix Them?'s table:
//   Reactor  + IHandleRequestsAsync<T> -> "Runtime error: Reactor pump cannot dispatch to async handlers"
//   Proactor + IHandleRequests<T>      -> "Runtime error: Proactor pump cannot dispatch to sync handlers"
// Each case is a real pump (ServiceActivator.Reactor / Proactor) reading one MT_COMMAND from the in-memory transport,
// dispatching through a CommandProcessor built by AddBrighter(), with the handler registered the page's wrong way.
// Each control is the same pump and message with the handler of the matching kind. The mapper is JsonMessageMapper<T>,
// registered for both kinds, so only the handler's kind differs between case and control.
// Recorded per row: handler invocations, whether the pump stopped by itself (no quit message needed), its Status,
// whether the message reached the dead-letter topic (rejected), and every Warning+ log entry with its exception.
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Logging;
using Paramore.Brighter.MessageMappers;
using Paramore.Brighter.ServiceActivator;

var log = new ConcurrentQueue<string>();
ApplicationLogging.LoggerFactory = LoggerFactory.Create(b => b.AddProvider(new Capture(log)).SetMinimumLevel(LogLevel.Warning));

var results = new[]
{
    await Run("case    Reactor  + async handler", MessagePumpType.Reactor, asyncHandler: true),
    await Run("control Reactor  + sync handler ", MessagePumpType.Reactor, asyncHandler: false),
    await Run("case    Proactor + sync handler ", MessagePumpType.Proactor, asyncHandler: false),
    await Run("control Proactor + async handler", MessagePumpType.Proactor, asyncHandler: true),
};
// The probe could fail: a control that does not handle, or a case that does, exits 1.
var ok = !results[0] && results[1] && !results[2] && results[3];
Console.WriteLine(ok ? "PROBE: cases failed to dispatch, controls handled" : "PROBE: UNEXPECTED");
return ok ? 0 : 1;

async Task<bool> Run(string label, MessagePumpType pumpType, bool asyncHandler)
{
    SyncHandler.Handled = 0; AsyncHandler.Handled = 0;
    while (log.TryDequeue(out _)) { }

    var services = new ServiceCollection();
    var brighter = services.AddBrighter();
    if (asyncHandler) brighter.AsyncHandlers(r => r.RegisterAsync<MyCommand, AsyncHandler>());
    else brighter.Handlers(r => r.Register<MyCommand, SyncHandler>());
    var provider = services.BuildServiceProvider();
    var commandProcessor = provider.GetRequiredService<IAmACommandProcessor>();

    var routingKey = new RoutingKey("my.command");
    var deadLetter = new RoutingKey("my.command.dlq");
    var bus = new InternalBus();
    var time = TimeProvider.System;
    var consumer = new InMemoryMessageConsumer(routingKey, bus, time, deadLetterTopic: deadLetter);

    var mappers = new MessageMapperRegistry(
        new SimpleMessageMapperFactory(_ => new JsonMessageMapper<MyCommand>()),
        new SimpleMessageMapperFactoryAsync(_ => new JsonMessageMapper<MyCommand>()));
    mappers.Register<MyCommand, JsonMessageMapper<MyCommand>>();
    mappers.RegisterAsync<MyCommand, JsonMessageMapper<MyCommand>>();

    var command = new MyCommand { Value = "hello" };
    var message = await new JsonMessageMapper<MyCommand>().MapToMessageAsync(command, new Publication { Topic = routingKey });
    bus.Enqueue(message);

    IAmAMessagePump pump;
    Action quit;
    var quitMessage = new Message(new MessageHeader(string.Empty, RoutingKey.Empty, MessageType.MT_QUIT), new MessageBody(""));
    if (pumpType == MessagePumpType.Reactor)
    {
        var channel = new Channel(new ChannelName("my.channel"), routingKey, consumer);
        pump = new Reactor(commandProcessor, _ => typeof(MyCommand), mappers, new EmptyMessageTransformerFactory(),
            new InMemoryRequestContextFactory(), channel) { TimeOut = TimeSpan.FromMilliseconds(200) };
        quit = () => channel.Enqueue(quitMessage);
    }
    else
    {
        var channel = new ChannelAsync(new ChannelName("my.channel"), routingKey, consumer);
        pump = new Proactor(commandProcessor, _ => typeof(MyCommand), mappers, new EmptyMessageTransformerFactoryAsync(),
            new InMemoryRequestContextFactory(), channel) { TimeOut = TimeSpan.FromMilliseconds(200) };
        quit = () => channel.Enqueue(quitMessage);
    }

    var run = Task.Factory.StartNew(() => pump.Run(), TaskCreationOptions.LongRunning);
    var stoppedByItself = await Task.WhenAny(run, Task.Delay(2000)) == run;
    if (!stoppedByItself) quit();
    var ended = await Task.WhenAny(run, Task.Delay(5000)) == run;

    var handled = SyncHandler.Handled + AsyncHandler.Handled;
    var rejected = bus.Stream(deadLetter).Count();
    var status = ((MessagePump)pump).Status;
    Console.WriteLine($"{label}: handled={handled} stoppedByItself={stoppedByItself} ended={ended} status={status} deadLettered={rejected}");
    foreach (var entry in log) Console.WriteLine($"    {entry}");
    return handled == 1;
}

public class MyCommand() : Command(Id.Random())
{
    public string Value { get; set; } = "";
}

public class SyncHandler : RequestHandler<MyCommand>
{
    public static int Handled;
    public override MyCommand Handle(MyCommand command)
    {
        Handled++;
        return base.Handle(command);
    }
}

public class AsyncHandler : RequestHandlerAsync<MyCommand>
{
    public static int Handled;
    public override async Task<MyCommand> HandleAsync(MyCommand command, CancellationToken cancellationToken = default)
    {
        Handled++;
        return await base.HandleAsync(command, cancellationToken);
    }
}

sealed class Capture(ConcurrentQueue<string> log) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(log, categoryName);
    public void Dispose() { }

    sealed class Logger(ConcurrentQueue<string> log, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var text = $"[{logLevel}] {category.Split('.').Last()}: {formatter(state, exception)}";
            for (var e = exception; e != null; e = e.InnerException)
                text += $"\n        {e.GetType().FullName}: {e.Message}";
            log.Enqueue(text);
        }
    }
}
