// Task 3.5, BrighterSchedulerSupport.md. Claims:
//   block 8  a handler that throws DeferMessageAction has its message requeued, with a delay
//   block 9  requeueCount: 3 means "Handle at most 3 times: 2 requeues", and requeueDelay is the
//            wait between attempts
// Case: block 8's handler, whose service always throws the transient error, under block 9's
// subscription, its requeueDelay shortened from 5 s (DELAY_MS, default 500) so the probe is quick.
// Control: the same pump and subscription with a service that succeeds: handled once.
// Measured: each gap is requeueDelay plus up to one empty-channel poll (500 ms by default) --
// 500 ms gave 1010/1007 ms, 1500 ms gave 2015/1508 ms -- so the delay is a floor, not the wait.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var failures = 0;
foreach (var failing in new[] { true, false })
{
    Attempts.Times.Clear();
    var bus = new InternalBus();
    var topic = new RoutingKey("order.process");
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSingleton<IOrderService>(new OrderService(failing));
    services.AddConsumers(options =>
        {
            options.Subscriptions =
            [
                // ---- block 9, verbatim but for the delay ----
                new Subscription<ProcessOrderCommand>(
                    new SubscriptionName("order.processor"),
                    channelName: new ChannelName("orders"),
                    routingKey: new RoutingKey("order.process"),
                    messagePumpType: MessagePumpType.Proactor,
                    requeueCount: 3,                        // Handle at most 3 times: 2 requeues
                    requeueDelay: TimeSpan.FromMilliseconds(Delay.Ms),  // the page waits 5 seconds
                    timeOut: TimeSpan.FromMilliseconds(200)
                )
            ];
            options.DefaultChannelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
        })
        .AddProducers(o => o.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
            [new Publication { Topic = topic, RequestType = typeof(ProcessOrderCommand) }], InstrumentationOptions.None).Create())
        .UseScheduler(new InMemorySchedulerFactory())
        .AutoFromAssemblies([typeof(ProcessOrderCommand).Assembly]);
    var provider = services.BuildServiceProvider();

    var dispatcher = provider.GetRequiredService<IDispatcher>();
    dispatcher.Receive();
    await provider.GetRequiredService<IAmACommandProcessor>().PostAsync(new ProcessOrderCommand { OrderId = Guid.NewGuid() });
    await Task.Delay(TimeSpan.FromMilliseconds(Delay.Ms * 3 + 2500));
    await dispatcher.End();

    var times = Attempts.Times.OrderBy(t => t).ToArray();
    var gaps = times.Zip(times.Skip(1), (a, b) => (b - a).TotalMilliseconds).ToArray();
    var label = failing ? "case   " : "control";
    var ok = failing ? times.Length == 3 && gaps.All(g => g >= Delay.Ms - 50) : times.Length == 1;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {label} handled {times.Length} time(s), gaps {string.Join(", ", gaps.Select(g => $"{g:F0} ms"))}");
    if (!ok) failures++;
}
return failures == 0 ? 0 : 1;

public class ProcessOrderCommand() : Command(Id.Random()) { public Guid OrderId { get; set; } }
public interface IOrderService { Task ProcessAsync(Guid orderId, CancellationToken cancellationToken); }
public class DatabaseUnavailableException : Exception;
public class OrderService(bool failing) : IOrderService
{
    public Task ProcessAsync(Guid orderId, CancellationToken cancellationToken)
        => failing ? throw new DatabaseUnavailableException() : Task.CompletedTask;
}
public static class Attempts { public static readonly ConcurrentBag<DateTime> Times = []; }

// ---- block 8, verbatim but for the line that records each attempt ----
public class ProcessOrderHandlerAsync(IOrderService orderService) : RequestHandlerAsync<ProcessOrderCommand>
{
    public override async Task<ProcessOrderCommand> HandleAsync(
        ProcessOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        Attempts.Times.Add(DateTime.UtcNow);  // probe
        try
        {
            await orderService.ProcessAsync(command.OrderId, cancellationToken);
            return await base.HandleAsync(command, cancellationToken);
        }
        catch (DatabaseUnavailableException)
        {
            // Requeue with delay to give database time to recover
            throw new DeferMessageAction();
        }
    }
}
public static class Delay { public static int Ms = int.TryParse(Environment.GetEnvironmentVariable("DELAY_MS"), out var ms) ? ms : 500; }
