// Task 7.5, FAQ.md. Page code is copied as the page shows it; the probe's own lines are marked // probe.
//  #10-#12: Send with an ICommand-typed variable throws ArgumentException "No command handler was found for the typeof
//      command Paramore.Brighter.ICommand - a command should have exactly one handler." Control: the concrete type (#11),
//      and (dynamic) (#12), each reaching the handler.
//  #13 + the handler under "2. Apply to handler": a pipeline registered with services.AddResiliencePipeline("MyRetryPolicy")
//      is the one [UseResiliencePipelineAsync("MyRetryPolicy", 1)] runs. The handler fails once, then succeeds. Control:
//      the same pipeline registered in a ResiliencePipelineRegistry<string> given to AddBrighter's
//      options.ResiliencePipelineRegistry (PolicyRetryAndCircuitBreaker.md's form, and #13 as repaired at 7.5).
//      The case is FAIL at 10.8.1: Brighter builds its own registry (BrighterResiliencePipelineRegistry) and never reads
//      the one Polly.Extensions registers in DI. Ledger 94.
//  New (V10) timeout block (#16): services.AddResiliencePipeline("MyTimeout") the same way. Control: the repaired form, a
//      1 s timeout (the page's is 5 s) over a 3 s handler, so the probe can see the timeout fire.
//  #20: noOfPerformers is the number of concurrent message pumps. Case: 3 on the page's RmqSubscription, and the
//      Dispatcher's consumers for an in-memory subscription with noOfPerformers 3. Control: 1.
//  #3: the AddConsumers-first chain registers both sides. Case: #3 as shown. Control: AddBrighter().AddProducers(...)
//      alone, which registers no Dispatcher.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.Policies.Attributes;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;

public class GreetingCommand(string name) : Command(Id.Random()) { public string Name { get; } = name; }   // probe
public class MyCommand() : Command(Id.Random());   // probe
public class MyEvent() : Event(Id.Random());   // probe

public class GreetingCommandHandler : RequestHandler<GreetingCommand>   // probe
{
    public static int Handled;
    public override GreetingCommand Handle(GreetingCommand command) { Handled++; return base.Handle(command); }
}

public static class Fail { public static int Remaining, Runs; }   // probe

// FAQ.md, "2. Apply to handler", as the page shows it but for the probe's counting line
public class MyHandler : RequestHandlerAsync<MyCommand>
{
    [UseResiliencePipelineAsync("MyRetryPolicy", 1)]
    public override async Task<MyCommand> HandleAsync(
        MyCommand command, CancellationToken cancellationToken = default)
    {
        Fail.Runs++; if (Fail.Remaining-- > 0) throw new InvalidOperationException("transient");   // probe
        // Handler logic
        return await base.HandleAsync(command, cancellationToken);
    }
}

// FAQ.md, New (V10) timeout block's handler, renamed so both live in one assembly
public class MyTimeoutHandler : RequestHandlerAsync<MyCommand>
{
    [UseResiliencePipelineAsync("MyTimeout", 1)]
    public override async Task<MyCommand> HandleAsync(
        MyCommand command, CancellationToken cancellationToken = default)
    {
        Fail.Runs++; await Task.Delay(TimeSpan.FromSeconds(Fail.Remaining), cancellationToken);   // probe: sleeps, honouring the token
        return await base.HandleAsync(command, cancellationToken);
    }
}

public static class Program
{
    static bool failed;
    static void Row(string name, bool asPageSays, string detail)
    { failed |= !asPageSays; Console.WriteLine($"{(asPageSays ? "ok  " : "FAIL")} {name}: {detail}"); }

    static string Outcome(Action a)
    {
        try { a(); return "no exception"; }
        catch (Exception e) { var x = e is AggregateException ag ? ag.InnerException! : e; return $"{x.GetType().Name}: {x.Message}{(x.InnerException is { } i ? $" [inner {i.GetType().Name}: {i.Message}]" : "")}"; }
    }

    static IAmACommandProcessor Processor<THandler>(Action<IServiceCollection> pipelines, ResiliencePipelineRegistry<string>? registry)
        where THandler : class
    {
        var services = new ServiceCollection();
        pipelines(services);
        services.AddBrighter(o => { if (registry != null) o.ResiliencePipelineRegistry = registry; })
            .AsyncHandlers(h => h.RegisterAsync<MyCommand>((_, _) => [typeof(THandler)], [typeof(THandler)]));
        return services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    }

    public static async Task<int> Main()
    {
        // #10-#12
        {
            var services = new ServiceCollection();
            services.AddBrighter().Handlers(h => h.Register<GreetingCommand, GreetingCommandHandler>());
            var commandProcessor = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();

            var ten = Outcome(() =>
            {
                ICommand command = new GreetingCommand("Ian");
                commandProcessor.Send(command);
            });
            Row("FAQ.md #10, Send(ICommand)", ten.StartsWith("ArgumentException: No command handler was found for the typeof command Paramore.Brighter.ICommand - a command should have exactly one handler."), ten);
            GreetingCommandHandler.Handled = 0;
            var eleven = Outcome(() => commandProcessor.Send(new GreetingCommand("Ian")));
            Row("FAQ.md #11, the concrete type", eleven == "no exception" && GreetingCommandHandler.Handled == 1, $"{eleven}, handled {GreetingCommandHandler.Handled}");
            GreetingCommandHandler.Handled = 0;
            var twelve = Outcome(() =>
            {
                ICommand command = new GreetingCommand("Ian");
                commandProcessor.Send((dynamic)command);
            });
            Row("FAQ.md #12, (dynamic)", twelve == "no exception" && GreetingCommandHandler.Handled == 1, $"{twelve}, handled {GreetingCommandHandler.Handled}");
        }

        // #13, the page's registration
        {
            var processor = Processor<MyHandler>(services =>
            {
                services.AddResiliencePipeline("MyRetryPolicy", builder =>
                {
                    builder.AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromSeconds(1),
                        BackoffType = DelayBackoffType.Exponential
                    });
                });
            }, null);
            Fail.Runs = 0; Fail.Remaining = 1;
            var r = Outcome(() => processor.SendAsync(new MyCommand()).GetAwaiter().GetResult());
            Row("FAQ.md #13, services.AddResiliencePipeline(\"MyRetryPolicy\") reaches the attribute", r == "no exception" && Fail.Runs == 2, $"{r}; handler ran {Fail.Runs}");

            // control: the same pipeline in a registry given to AddBrighter
            var registry = new ResiliencePipelineRegistry<string>();   // probe: the control
            registry.TryAddBuilder("MyRetryPolicy", (builder, _) => builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential
            }));
            processor = Processor<MyHandler>(_ => { }, registry);
            Fail.Runs = 0; Fail.Remaining = 1;
            r = Outcome(() => processor.SendAsync(new MyCommand()).GetAwaiter().GetResult());
            Console.WriteLine($"     control, options.ResiliencePipelineRegistry: {r}; handler ran {Fail.Runs}");
            if (!(r == "no exception" && Fail.Runs == 2)) failed = true;
        }

        // the New (V10) timeout block
        {
            var processor = Processor<MyTimeoutHandler>(services =>
            {
                services.AddResiliencePipeline("MyTimeout", builder =>
                {
                    builder.AddTimeout(TimeSpan.FromSeconds(5));
                });
            }, null);
            Fail.Runs = 0; Fail.Remaining = 0;
            var r = Outcome(() => processor.SendAsync(new MyCommand()).GetAwaiter().GetResult());
            Row("FAQ.md New (V10) timeout block, services.AddResiliencePipeline(\"MyTimeout\")", r == "no exception", $"{r}; handler ran {Fail.Runs}");
            var registry = new ResiliencePipelineRegistry<string>();   // probe: the control
            registry.TryAddBuilder("MyTimeout", (builder, _) => builder.AddTimeout(TimeSpan.FromSeconds(1)));
            processor = Processor<MyTimeoutHandler>(_ => { }, registry);
            Fail.Runs = 0; Fail.Remaining = 3;
            r = Outcome(() => processor.SendAsync(new MyCommand()).GetAwaiter().GetResult());
            Console.WriteLine($"     control, a 1 s timeout in options.ResiliencePipelineRegistry, a 3 s handler: {r}");
        }

        // #20
        {
            var subscription = new RmqSubscription<MyEvent>(
                // Other settings...
                noOfPerformers: 3  // Number of concurrent message pumps
            );
            Console.WriteLine($"     #20's RmqSubscription.NoOfPerformers = {subscription.NoOfPerformers}");
            foreach (var n in new[] { 3, 1 })
            {
                var bus = new InternalBus();
                var services = new ServiceCollection();
                services.AddConsumers(o =>
                {
                    o.Subscriptions = [new InMemorySubscription<MyEvent>(new SubscriptionName("s"), new ChannelName("c"), new RoutingKey("my.event"),
                        noOfPerformers: n, messagePumpType: MessagePumpType.Reactor)];
                    o.DefaultChannelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
                }).AutoFromAssemblies([typeof(Program).Assembly]);
                var dispatcher = services.BuildServiceProvider().GetRequiredService<IDispatcher>();
                dispatcher.Receive();
                await Task.Delay(300);
                var consumers = dispatcher.Consumers.Count();
                await dispatcher.End();
                if (n == 3) Row("FAQ.md #20, noOfPerformers: 3 runs 3 message pumps", consumers == 3, $"{consumers} consumers");
                else Console.WriteLine($"     control, noOfPerformers: 1: {consumers} consumer(s)");
            }
        }

        // #3
        {
            var services = new ServiceCollection();   // probe
            var rmqConnection = new RmqMessagingGatewayConnection
            {
                AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
                Exchange = new Exchange("paramore.brighter.exchange")
            };

            // Configure consumers: AddConsumers extends IServiceCollection, so it comes first
            services.AddConsumers(options =>
                {
                    options.Subscriptions =
                    [
                        new RmqSubscription<MyEvent>(
                            new SubscriptionName("my.subscription"),
                            new ChannelName("my.queue"),
                            new RoutingKey("my.event"))
                    ];
                    options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rmqConnection));
                })
                // Configure producers: AddProducers extends the IBrighterBuilder that AddConsumers returns
                .AddProducers(options =>
                {
                    options.ProducerRegistry = new RmqProducerRegistryFactory(
                        rmqConnection,
                        [new RmqPublication<MyEvent> { Topic = new RoutingKey("my.event") }]
                    ).Create();
                })
                .AutoFromAssemblies([typeof(Program).Assembly]);   // probe: the assembly named, as the page's AutoFromAssemblies() scans the caller's

            var sp = services.BuildServiceProvider();
            var dispatcherRegistered = services.Any(d => d.ServiceType == typeof(IDispatcher));
            var producers = sp.GetRequiredService<IAmAProducerRegistry>();
            var topics = string.Join(",", producers.Producers.Select(p => p.Publication.Topic?.Value));
            var subs = sp.GetRequiredService<IDispatcher>() is { } d2 ? "resolved" : "null";
            Row("FAQ.md #3, AddConsumers then AddProducers registers both sides", dispatcherRegistered && topics == "my.event",
                $"IDispatcher registered and {subs}; producer registry topics [{topics}]");

            var control = new ServiceCollection();
            control.AddBrighter().AddProducers(options =>
            {
                options.ProducerRegistry = new RmqProducerRegistryFactory(rmqConnection,
                    [new RmqPublication<MyEvent> { Topic = new RoutingKey("my.event") }]).Create();
            });
            Console.WriteLine($"     control, AddBrighter().AddProducers alone: IDispatcher registered {control.Any(d => d.ServiceType == typeof(IDispatcher))}");
        }

        return failed ? 1 : 0;
    }
}
