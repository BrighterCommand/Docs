// Task 7.5, TestDoubleOptions.md. Page code is copied as the page shows it; the request types and handlers are the probe's,
// each making the call its block then checks (as the blocks' scaffold unit does).
//  #2 and the sentence after it: Observe<T>() dequeues the next request of type T in FIFO order, consuming it, and throws
//      InvalidOperationException when none is left. Case: two MyCommands sent, A then B, a MyEvent between. Control:
//      GetRequests<T>() (Layer 2, "non-destructive, can call repeatedly").
//  #8: ClearOutbox moves a deposited request to the observation queue. Case: Observe after the clear. Control: Observe
//      before it.
//  #9: Reset() empties RecordedCalls and DepositedRequests. Case: a Send and a DepositPost, then Reset. Control: the same
//      without Reset.
//  #11, #12: Times.Once / Received(1) verify one Publish. Case: a handler publishing once. Control: twice, which each
//      framework must reject.
using System;
using System.Linq;
using Moq;
using NSubstitute;
using Paramore.Brighter;
using Paramore.Brighter.Testing;
using Shouldly;

public class MyCommand() : Command(Id.Random()) { public string Tag { get; set; } = ""; }   // probe
public class MyEvent() : Event(Id.Random());   // probe
public class CommandA() : Command(Id.Random());   // probe
public class ProcessPayment() : Command(Id.Random()) { public decimal Amount { get; set; } }   // probe
public class PaymentProcessed() : Event(Id.Random()) { public decimal Amount { get; set; } }   // probe
public class TransactionalHandler(IAmACommandProcessor p) : RequestHandler<ProcessPayment>   // probe
{
    public override ProcessPayment Handle(ProcessPayment c) { p.DepositPost(new PaymentProcessed { Amount = c.Amount }); return base.Handle(c); }
}
public class MyHandler(IAmACommandProcessor p, int publishes = 1) : RequestHandler<MyCommand>   // probe
{
    public override MyCommand Handle(MyCommand c) { for (var i = 0; i < publishes; i++) p.Publish(new MyEvent()); return base.Handle(c); }
}

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }
    static string Try(Action a) { try { a(); return "passed"; } catch (Exception e) { return $"{e.GetType().Name}"; } }

    public static int Main()
    {
        // #2
        {
            var spy = new SpyCommandProcessor();
            spy.Send(new MyCommand { Tag = "A" });
            spy.Publish(new MyEvent());
            spy.Send(new MyCommand { Tag = "B" });

            // Was a specific method type called?
            var wasSent = spy.WasCalled(CommandType.Send);          // true/false
            var wasPublished = spy.WasCalled(CommandType.Publish);  // true/false
            var wasPosted = spy.WasCalled(CommandType.Post);        // true/false

            // How many times?
            var sendCount = spy.CallCount(CommandType.Send);        // int

            // Dequeue requests in FIFO order (consuming)
            var command = spy.Observe<MyCommand>();
            var @event = spy.Observe<MyEvent>();

            var second = spy.Observe<MyCommand>();
            var third = Try(() => spy.Observe<MyCommand>());
            Row("TestDoubleOptions.md #2, Observe<T> FIFO and consuming", wasSent && wasPublished && !wasPosted && sendCount == 2
                && command.Tag == "A" && @event != null && second.Tag == "B" && third == "InvalidOperationException",
                $"sent {wasSent}, published {wasPublished}, posted {wasPosted}, {sendCount} sends; Observe A then {second.Tag}, then {third}");

            var spy2 = new SpyCommandProcessor();
            spy2.Send(new MyCommand { Tag = "A" }); spy2.Send(new MyCommand { Tag = "B" });
            var g1 = string.Join("", spy2.GetRequests<MyCommand>().Select(c => c.Tag));
            var g2 = string.Join("", spy2.GetRequests<MyCommand>().Select(c => c.Tag));
            Console.WriteLine($"     control, GetRequests twice: {g1}, {g2}; Observe after them: {spy2.Observe<MyCommand>().Tag}");
        }

        // #8
        {
            var spy = new SpyCommandProcessor();
            var handler = new TransactionalHandler(spy);

            handler.Handle(new ProcessPayment { Amount = 99.99m });

            // Verify the deposit
            spy.WasCalled(CommandType.Deposit).ShouldBeTrue();
            spy.DepositedRequests.Count.ShouldBe(1);

            // Get the deposited request
            var (id, request) = spy.DepositedRequests.First();
            var deposited = request.ShouldBeOfType<PaymentProcessed>();
            deposited.Amount.ShouldBe(99.99m);

            var before = Try(() => spy.Observe<PaymentProcessed>());   // probe: the control

            // Simulate ClearOutbox (moves deposits to observation queue)
            spy.ClearOutbox(new[] { id });

            // Now it's available via Observe
            var cleared = spy.Observe<PaymentProcessed>();
            cleared.Amount.ShouldBe(99.99m);
            Row("TestDoubleOptions.md #8, ClearOutbox makes the deposit observable", before == "InvalidOperationException" && cleared.Amount == 99.99m,
                $"Observe before the clear: {before}; after: Amount {cleared.Amount}; still in DepositedRequests: {spy.DepositedRequests.Count}");
        }

        // #9
        foreach (var reset in new[] { true, false })
        {
            var spy = new SpyCommandProcessor();

            // First scenario
            spy.Send(new CommandA());
            spy.DepositPost(new MyEvent());   // probe: so DepositedRequests has something to clear
            spy.WasCalled(CommandType.Send).ShouldBeTrue();

            // Reset for next scenario
            if (reset) spy.Reset();

            var result = $"WasCalled(Send) {spy.WasCalled(CommandType.Send)}, RecordedCalls {spy.RecordedCalls.Count}, DepositedRequests {spy.DepositedRequests.Count}";
            if (reset) Row("TestDoubleOptions.md #9, Reset", !spy.WasCalled(CommandType.Send) && spy.RecordedCalls.Count == 0 && spy.DepositedRequests.Count == 0, result);
            else Console.WriteLine($"     control, no Reset: {result}");
        }

        // #11
        foreach (var n in new[] { 1, 2 })
        {
            var mock = new Mock<IAmACommandProcessor>();
            var handler = new MyHandler(mock.Object, n);

            handler.Handle(new MyCommand());

            var r = Try(() => mock.Verify(p => p.Publish(It.IsAny<MyEvent>(), It.IsAny<RequestContext>()), Times.Once));
            if (n == 1) Row("TestDoubleOptions.md #11, Moq Times.Once, one Publish", r == "passed", r);
            else Console.WriteLine($"     control, two Publishes: {r}");
        }

        // #12
        foreach (var n in new[] { 1, 2 })
        {
            var substitute = Substitute.For<IAmACommandProcessor>();
            var handler = new MyHandler(substitute, n);

            handler.Handle(new MyCommand());

            var r = Try(() => substitute.Received(1).Publish(Arg.Any<MyEvent>(), Arg.Any<RequestContext>()));
            if (n == 1) Row("TestDoubleOptions.md #12, NSubstitute Received(1), one Publish", r == "passed", r);
            else Console.WriteLine($"     control, two Publishes: {r}");
        }
        return failed ? 1 : 0;
    }
}
