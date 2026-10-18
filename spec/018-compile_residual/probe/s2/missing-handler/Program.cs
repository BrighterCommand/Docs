// PipelineValidation.md § Missing Handler for Subscription, against 10.8.0. "AutoFromAssemblies() scans
// the assemblies already loaded when it runs, plus any you pass it, so if your handler lives in an
// assembly nothing has loaded yet, pass that assembly explicitly", and ValidatePipelines() reports the
// subscription with no handler. OrderCreated is in Contracts.dll, OrderCreatedHandler in Handlers.dll,
// which nothing touches before AutoFromAssemblies runs. Loading is per process, so each case is a run:
//   before   case:    the page's Before block, plus .ValidatePipelines() - Handlers.dll not loaded, and
//                     the validators report "No handler registered for 'OrderCreated'"
//   after    control: the page's After block - Handlers.dll passed, no error
//   loaded   control: the Before block, run after Handlers.dll is loaded - the scan finds it, no error
// The validators are called directly, as BrighterValidationHostedService.StartAsync calls them, so that
// no hosted service starts a consumer against a broker.
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.Validation;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var mode = args.FirstOrDefault() ?? "before";
var services = new ServiceCollection();
services.AddLogging();
var rmqConnection = new RmqMessagingGatewayConnection                                   // probe
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange")
};

if (mode == "loaded") Touch.Handlers();
if (mode == "after") Touch.After(services, rmqConnection);
else Before(services, rmqConnection);

var handlersLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Handlers");
var provider = services.BuildServiceProvider();
var errors = provider.GetServices<IAmAPipelineValidator>()
    .SelectMany(v => v.Validate().Errors)
    .Select(e => e.Message.ReplaceLineEndings(" "))
    .ToList();
foreach (var e in errors) Console.WriteLine($"     error: {e}");

var missing = errors.Any(e => e.Contains("No handler registered for 'OrderCreated'"));
var ok = mode == "before" ? missing && !handlersLoaded : !missing;
Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {mode}: Handlers.dll loaded when scanned: {handlersLoaded}; missing-handler error: {missing}");
return ok ? 0 : 1;

[MethodImpl(MethodImplOptions.NoInlining)]
static void Before(IServiceCollection services, RmqMessagingGatewayConnection rmqConnection)
{
    // verbatim from the page's Before block, plus ValidatePipelines()
    services.AddConsumers(options =>
        {
            // A subscription for OrderCreated, but OrderCreatedHandler is in a separate assembly
            options.Subscriptions = [
                new RmqSubscription<OrderCreated>(
                    subscriptionName: new SubscriptionName("order-created"),
                    channelName: new ChannelName("order-created-queue"),
                    routingKey: new RoutingKey("order-created"),
                    messagePumpType: MessagePumpType.Proactor)
            ];
            options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rmqConnection));
        })
        .AutoFromAssemblies()  // scans only the assemblies already loaded, so the handler is never found
        .ValidatePipelines();                                                           // probe
}

// Methods naming Handlers.dll's types live here, so the JIT does not load it while compiling Main
static class Touch
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Handlers() => _ = typeof(global::Handlers.OrderCreatedHandler).Assembly;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void After(IServiceCollection services, RmqMessagingGatewayConnection rmqConnection)
    {
        // the page's After block, its elided options written out as the Before block has them
        services.AddConsumers(options =>
            {
                options.Subscriptions = [
                    new RmqSubscription<OrderCreated>(
                        subscriptionName: new SubscriptionName("order-created"),
                        channelName: new ChannelName("order-created-queue"),
                        routingKey: new RoutingKey("order-created"),
                        messagePumpType: MessagePumpType.Proactor)
                ];
                options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rmqConnection));
            })
            .AutoFromAssemblies([typeof(global::Handlers.OrderCreatedHandler).Assembly])  // explicitly include the handler's assembly
            .ValidatePipelines();                                                       // probe
    }
}
