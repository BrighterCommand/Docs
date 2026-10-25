// Task 6.4, HandlerFailure.md. Each page handler is copied as the page shows it, in its own namespace; ProcessOrder
// is the probe's (it counts its runs, and fails while Fail.Remaining > 0). "OrderRetryPolicy" and
// "OrderCircuitBreakerAndRetry", which the page names and does not show, are the probe's: a retry of 3, no delay,
// that handles the reader's transient InvalidOperationException only.
//  #1  [DeferMessageOnError(0, 5000)]: any unhandled exception becomes DeferMessageAction, Delay 5 s, the original
//      as InnerException. Control: the same handler without the attribute; and delayMilliseconds omitted -> Delay
//      null, "the Subscription default is used".
//  #2  Defer at step 0 outside the retry at step 1: the retry runs out, then the backstop defers. Control: the steps
//      swapped, the backstop inside the retry, which defers on the first failure and is never retried.
//  #5  [RejectMessageOnError(0)] outside the retry: RejectMessageAction after 4 runs, InnerException the original.
//  #10, #11 the Pipeline Ordering handlers, sync and async: the same.
//  #8  [DontAckOnError(0)]: DontAckAction, InnerException the original.
//  #6  [FeatureSwitch(..., dontAck: true)] with the handler Off: DontAckAction, handler not run. Control: dontAck
//      false: no exception, handler not run, "silently consumes the message".
//  #9  OrderMessageMapper: a body that is not JSON -> InvalidMessageAction, InnerException JsonException; "null" ->
//      InvalidMessageAction. Control: a valid body maps.
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.Extensions;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.FeatureSwitch;
using Paramore.Brighter.FeatureSwitch.Providers;
using Polly;
using Polly.Registry;
using Polly.Retry;

public class PlaceOrder() : Command(Id.Random()) { public string OrderId { get; set; } = "o-1"; }   // probe

public static class Probe
{
    public static int Runs, Remaining;
    public static void ProcessOrder(PlaceOrder _) { Runs++; if (Remaining-- > 0) throw new InvalidOperationException("transient"); }
    public static Task ProcessOrderAsync(PlaceOrder c, CancellationToken _) { ProcessOrder(c); return Task.CompletedTask; }
}

public static class Program
{
    static bool failed;

    static IAmACommandProcessor Build<THandler>(IAmAFeatureSwitchRegistry? switches, bool async) where THandler : class
    {
        var registry = new ResiliencePipelineRegistry<string>().AddBrighterDefault();
        foreach (var name in new[] { "OrderRetryPolicy", "OrderCircuitBreakerAndRetry" })
            registry.TryAddBuilder(name, (b, _) => b.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3, Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
            }));
        var services = new ServiceCollection();
        services.AddBrighter(o => { o.ResiliencePipelineRegistry = registry; o.FeatureSwitchRegistry = switches; })
            .Handlers(h => { if (!async) h.Register<PlaceOrder>((_, _) => [typeof(THandler)], [typeof(THandler)]); })
            .AsyncHandlers(h => { if (async) h.RegisterAsync<PlaceOrder>((_, _) => [typeof(THandler)], [typeof(THandler)]); });
        return services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
    }

    static (string outcome, Exception? e) Send<THandler>(int failures, IAmAFeatureSwitchRegistry? switches = null, bool async = false) where THandler : class
    {
        var cp = Build<THandler>(switches, async);
        Probe.Runs = 0; Probe.Remaining = failures;
        try
        {
            if (async) cp.SendAsync(new PlaceOrder()).GetAwaiter().GetResult(); else cp.Send(new PlaceOrder());
            return ($"no exception, {Probe.Runs} run(s)", null);
        }
        catch (Exception e)
        {
            var inner = e.InnerException is null ? "" : $", InnerException {e.InnerException.GetType().Name}";
            var delay = e is DeferMessageAction d ? $", Delay {(d.Delay?.ToString() ?? "null")}" : "";
            return ($"{e.GetType().Name}{inner}{delay}, {Probe.Runs} run(s)", e);
        }
    }

    static void Row(string name, string expect, string got)
    {
        var ok = got == expect; failed |= !ok;
        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {got}{(ok ? "" : $"  (expected {expect})")}");
    }

    public static int Main()
    {
        const int always = 100;
        Row("#1 case     Defer(0, 5000)", "DeferMessageAction, InnerException InvalidOperationException, Delay 00:00:05, 1 run(s)", Send<P1.OrderHandler>(always).outcome);
        Row("#1 control  no attribute", "InvalidOperationException, 1 run(s)", Send<Control.OrderHandler>(always).outcome);
        Row("#1 control  Defer(0), no delay", "DeferMessageAction, InnerException InvalidOperationException, Delay null, 1 run(s)", Send<Control.NoDelayHandler>(always).outcome);
        Row("#2 case     Defer(0) outside retry(1)", "DeferMessageAction, InnerException InvalidOperationException, Delay 00:00:05, 4 run(s)", Send<P2.OrderHandler>(always).outcome);
        Row("#2 case     transient, fails twice", "no exception, 3 run(s)", Send<P2.OrderHandler>(2).outcome);
        Row("#2 control  Defer(2) inside retry(1)", "DeferMessageAction, InnerException InvalidOperationException, Delay 00:00:05, 1 run(s)", Send<Control.DeferInsideHandler>(always).outcome);
        Row("#5 case     Reject(0) outside retry(1)", "RejectMessageAction, InnerException InvalidOperationException, 4 run(s)", Send<P5.OrderHandler>(always).outcome);
        Row("#10 case    Reject(0) outside retry(1)", "RejectMessageAction, InnerException InvalidOperationException, 4 run(s)", Send<P10.OrderHandler>(always).outcome);
        Row("#11 case    RejectAsync(0) outside retry(1)", "RejectMessageAction, InnerException InvalidOperationException, 4 run(s)", Send<P11.OrderHandler>(always, async: true).outcome);
        Row("#10 control Reject(2) inside retry(1)", "RejectMessageAction, InnerException InvalidOperationException, 1 run(s)", Send<Control.RejectInsideHandler>(always).outcome);
        Row("#8 case     DontAck(0) outside retry(1)", "DontAckAction, InnerException InvalidOperationException, 4 run(s)", Send<P8.OrderHandler>(always).outcome);

        var off = FluentConfigRegistryBuilder.With().StatusOf<P6.OrderHandler>().Is(FeatureSwitchStatus.Off)
            .StatusOf<Control.SwitchNoDontAckHandler>().Is(FeatureSwitchStatus.Off).Build();
        Row("#6 case     FeatureSwitch Off, dontAck: true", "DontAckAction, 0 run(s)", Send<P6.OrderHandler>(0, off).outcome);
        Row("#6 control  FeatureSwitch Off, dontAck: false", "no exception, 0 run(s)", Send<Control.SwitchNoDontAckHandler>(0, off).outcome);

        var mapper = new P9.OrderMessageMapper();
        string Map(string body)
        {
            try { mapper.MapToRequest(new Message(new MessageHeader(Id.Random(), new RoutingKey("orders"), MessageType.MT_COMMAND), new MessageBody(body))); return "maps"; }
            catch (Exception e) { return $"{e.GetType().Name}{(e.InnerException is null ? "" : $", InnerException {e.InnerException.GetType().Name}")}"; }
        }
        Row("#9 case     body not JSON", "InvalidMessageAction, InnerException JsonException", Map("not json"));
        Row("#9 case     body null", "InvalidMessageAction", Map("null"));
        Row("#9 control  valid body", "maps", Map("{\"OrderId\":\"o-2\",\"Id\":\"" + Guid.NewGuid() + "\"}"));
        return failed ? 1 : 0;
    }
}

// ---- the page's handlers, as the page shows them; `using static Probe` supplies ProcessOrder ----
namespace P1
{
    using Paramore.Brighter;
    using Paramore.Brighter.Defer.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        // Any unhandled exception → requeue with a 5-second delay
        [DeferMessageOnError(step: 0, delayMilliseconds: 5000)]
        public override PlaceOrder Handle(PlaceOrder command)
        {
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P2
{
    using Paramore.Brighter;
    using Paramore.Brighter.Defer.Attributes;
    using Paramore.Brighter.Policies.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        // Outermost: catch any unhandled exception and requeue with delay
        [DeferMessageOnError(step: 0, delayMilliseconds: 5000)]
        // Innermost: retry transient failures in-process first
        [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
        public override PlaceOrder Handle(PlaceOrder command)
        {
            // 1. Retry policy retries transient failures in-process
            // 2. If all retries are exhausted, the exception propagates
            // 3. DeferMessageOnError catches it and requeues with a 5-second delay
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P5
{
    using Paramore.Brighter;
    using Paramore.Brighter.Policies.Attributes;
    using Paramore.Brighter.Reject.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        // Backstop: any unhandled exception → reject to DLQ
        [RejectMessageOnError(step: 0)]
        [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
        public override PlaceOrder Handle(PlaceOrder command)
        {
            // If this throws after retries are exhausted,
            // the backstop catches it and rejects to DLQ
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P6
{
    using Paramore.Brighter;
    using Paramore.Brighter.FeatureSwitch;
    using Paramore.Brighter.FeatureSwitch.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        [FeatureSwitch(typeof(OrderHandler), FeatureSwitchStatus.Config, step: 1, dontAck: true)]
        public override PlaceOrder Handle(PlaceOrder command)
        {
            // If the feature is switched off, the message stays on the channel
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P8
{
    using Paramore.Brighter;
    using Paramore.Brighter.DontAck.Attributes;
    using Paramore.Brighter.Policies.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        // Backstop: any unhandled exception → don't ack, leave on channel
        [DontAckOnError(step: 0)]
        [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
        public override PlaceOrder Handle(PlaceOrder command)
        {
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P9
{
    using System.Text.Json;
    using Paramore.Brighter;
    using Paramore.Brighter.Actions;

    public class OrderMessageMapper : IAmAMessageMapper<PlaceOrder>
    {
        public IRequestContext? Context { get; set; }

        public PlaceOrder MapToRequest(Message message)
        {
            try
            {
                var body = JsonSerializer.Deserialize<PlaceOrder>(message.Body.Value);
                if (body is null)
                    throw new InvalidMessageAction("Message body deserialized to null");
                return body;
            }
            catch (JsonException ex)
            {
                throw new InvalidMessageAction("Failed to deserialize PlaceOrder", ex);
            }
        }

        public Message MapToMessage(PlaceOrder request, Publication publication)
        {
            var header = new MessageHeader(
                messageId: request.Id,
                topic: publication.Topic!,                                   // probe: nullable
                messageType: MessageType.MT_COMMAND
            );

            var body = new MessageBody(JsonSerializer.Serialize(request));

            return new Message(header, body);
        }
    }
}

namespace P10
{
    using Paramore.Brighter;
    using Paramore.Brighter.Policies.Attributes;
    using Paramore.Brighter.Reject.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        [RejectMessageOnError(step: 0)]                                // Outermost: backstop
        [UseResiliencePipeline("OrderCircuitBreakerAndRetry", step: 1)] // Inside: circuit breaker, then retry
        public override PlaceOrder Handle(PlaceOrder command)
        {
            // 1. Retry wraps the handler (retries transient failures)
            // 2. Circuit breaker wraps retry (stops retrying if too many failures)
            // 3. Backstop wraps everything (rejects to DLQ if all else fails)
            ProcessOrder(command);
            return base.Handle(command);
        }
    }
}

namespace P11
{
    using System.Threading;
    using System.Threading.Tasks;
    using Paramore.Brighter;
    using Paramore.Brighter.Policies.Attributes;
    using Paramore.Brighter.Reject.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandlerAsync<PlaceOrder>
    {
        [RejectMessageOnErrorAsync(step: 0)]
        [UseResiliencePipelineAsync("OrderCircuitBreakerAndRetry", step: 1)]
        public override async Task<PlaceOrder> HandleAsync(
            PlaceOrder command, CancellationToken cancellationToken = default)
        {
            await ProcessOrderAsync(command, cancellationToken);
            return await base.HandleAsync(command, cancellationToken);
        }
    }
}

// ---- the probe's controls ----
namespace Control
{
    using Paramore.Brighter;
    using Paramore.Brighter.Defer.Attributes;
    using Paramore.Brighter.FeatureSwitch;
    using Paramore.Brighter.FeatureSwitch.Attributes;
    using Paramore.Brighter.Policies.Attributes;
    using Paramore.Brighter.Reject.Attributes;
    using static Probe;

    public class OrderHandler : RequestHandler<PlaceOrder>
    {
        public override PlaceOrder Handle(PlaceOrder command) { ProcessOrder(command); return base.Handle(command); }
    }
    public class NoDelayHandler : RequestHandler<PlaceOrder>
    {
        [DeferMessageOnError(step: 0)]
        public override PlaceOrder Handle(PlaceOrder command) { ProcessOrder(command); return base.Handle(command); }
    }
    public class DeferInsideHandler : RequestHandler<PlaceOrder>
    {
        [DeferMessageOnError(step: 2, delayMilliseconds: 5000)]
        [UseResiliencePipeline("OrderRetryPolicy", step: 1)]
        public override PlaceOrder Handle(PlaceOrder command) { ProcessOrder(command); return base.Handle(command); }
    }
    public class RejectInsideHandler : RequestHandler<PlaceOrder>
    {
        [RejectMessageOnError(step: 2)]
        [UseResiliencePipeline("OrderCircuitBreakerAndRetry", step: 1)]
        public override PlaceOrder Handle(PlaceOrder command) { ProcessOrder(command); return base.Handle(command); }
    }
    public class SwitchNoDontAckHandler : RequestHandler<PlaceOrder>
    {
        [FeatureSwitch(typeof(SwitchNoDontAckHandler), FeatureSwitchStatus.Config, step: 1)]
        public override PlaceOrder Handle(PlaceOrder command) { ProcessOrder(command); return base.Handle(command); }
    }
}
