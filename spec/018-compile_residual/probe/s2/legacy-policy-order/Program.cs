// MigratingToPollyV8.md § Legacy: Using Polly v7 Policies, against 10.8.0.
//   Claim 1: "Where we have multiple policies they are evaluated left to right, so in this case
//            'MyCircuitBreakerPolicy' wraps 'MyExceptionPolicy'."
//   Claim 2: "put a Circuit Breaker UsePolicy attribute as an earlier step than the Retry UsePolicy
//            attribute. If retries expire, the exception will bubble out to the Circuit Breaker."
// The handler always throws, and each case sends twice with fresh policies. The page's circuit breaker
// is verbatim (breaks on 1 exception, for 500 ms); its retry is the page's three retries without the
// 1-3 s waits, which this probe does not need.
//   case:    [UsePolicy(new [] {"MyCircuitBreakerPolicy", "MyExceptionPolicy"}, step: 1)], the page's.
//            If the breaker wraps the retry, the first send makes 4 attempts and then opens the breaker,
//            so the second send throws BrokenCircuitException without reaching the handler: 4 attempts
//   control: the array reversed. The retry wraps the breaker, the first attempt opens it, and every
//            retry meets BrokenCircuitException: 1 attempt
//   Claim 3, unstated: the page's `var policyRegistry = new PolicyRegistry();` is a registry Brighter
//            accepts. Both AddBrighter and CommandProcessorBuilder.Resilience require the
//            CommandProcessor.RETRYPOLICY and CIRCUITBREAKER policies in it.
//   case:    the page's new PolicyRegistry(), with the page's two policies: resolving throws
//   control: new DefaultPolicy(), Brighter's registry holding them, with the same two: resolves
//            (claims 1's case and control use it)
// Claim 2 is not run: UsePolicyAttribute has no AllowMultiple, so two [UsePolicy] attributes on one
// method do not compile. `dotnet build -p:DefineConstants=TWO_ATTRIBUTES` shows CS0579.
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Policies.Attributes;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;

var failed = 0;
failed += Run<MyQoSProtectedHandler>("claim 3 case     new PolicyRegistry()", () => new PolicyRegistry(), expectAttempts: -1);
failed += Run<MyQoSProtectedHandler>("claim 3 control  new DefaultPolicy()", () => new DefaultPolicy(), expectAttempts: 4);
failed += Run<MyQoSProtectedHandler>("claim 1 case     [CB, Exception]: the breaker wraps the retry", () => new DefaultPolicy(), expectAttempts: 4);
failed += Run<ReversedHandler>("claim 1 control  [Exception, CB]: the retry wraps the breaker", () => new DefaultPolicy(), expectAttempts: 1);
return failed == 0 ? 0 : 1;

// expectAttempts -1: resolving the command processor throws
static int Run<THandler>(string name, Func<PolicyRegistry> newRegistry, int expectAttempts) where THandler : class, IHandleRequests<MyCommand>
{
    var policyRegistry = newRegistry();
    var policy = Policy.Handle<Exception>().Retry(3);                                   // probe: no waits
    policyRegistry.Add("MyExceptionPolicy", policy);
    var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(
        1, TimeSpan.FromMilliseconds(500));
    policyRegistry.Add("MyCircuitBreakerPolicy", circuitBreakerPolicy);

    var services = new ServiceCollection();
    services.AddBrighter(o => o.PolicyRegistry = policyRegistry)
        .Handlers(h => h.Register<MyCommand, THandler>());
    IAmACommandProcessor cp;
    try { cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>(); }
    catch (Exception e)
    {
        var resolved = expectAttempts == -1;
        Console.WriteLine($"{(resolved ? "ok  " : "FAIL")} {name}: {e.GetType().Name}: {e.Message.Split('\n')[0]}");
        return resolved ? 0 : 1;
    }
    if (expectAttempts == -1) { Console.WriteLine($"FAIL {name}: resolved"); return 1; }

    Attempts.Count = 0;
    var outcomes = new string[2];
    for (var i = 0; i < 2; i++)
    {
        try { cp.Send(new MyCommand()); outcomes[i] = "returned"; }
        catch (Exception e) { outcomes[i] = Unwrap(e).GetType().Name; }
    }
    var ok = Attempts.Count == expectAttempts;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {Attempts.Count} attempt(s); send 1 {outcomes[0]}, send 2 {outcomes[1]}");
    return ok ? 0 : 1;
}

static Exception Unwrap(Exception e)
{
    while (e.InnerException != null && e is not BrokenCircuitException && e is not InvalidOperationException) e = e.InnerException;
    return e;
}

public static class Attempts { public static int Count; }

public class MyCommand() : Command(Id.Random());

// verbatim from the page, but for the body, which counts the attempt and fails
public class MyQoSProtectedHandler : RequestHandler<MyCommand>
{
    [UsePolicy(new [] {"MyCircuitBreakerPolicy", "MyExceptionPolicy"} , step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        Attempts.Count++;                                                               // probe
        throw new InvalidOperationException("a distributed computing failure");         // probe
    }
}

public class ReversedHandler : RequestHandler<MyCommand>
{
    [UsePolicy(new [] {"MyExceptionPolicy", "MyCircuitBreakerPolicy"} , step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        Attempts.Count++;
        throw new InvalidOperationException("a distributed computing failure");
    }
}

#if TWO_ATTRIBUTES
// The page's claim 2: the breaker as an earlier step than the retry
public class TwoAttributeHandler : RequestHandler<MyCommand>
{
    [UsePolicy("MyCircuitBreakerPolicy", step: 1)]
    [UsePolicy("MyExceptionPolicy", step: 2)]
    public override MyCommand Handle(MyCommand command) => base.Handle(command);
}
#endif
