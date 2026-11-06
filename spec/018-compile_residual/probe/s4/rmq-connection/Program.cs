// Task 6.4, RabbitMQConnectionStability.md #1 (and RabbitMQConfiguration.md #4, the same three arguments) against
// Paramore.Brighter.MessagingGateway.RMQ.Async 10.8.1. Brighter builds its connection policies in
// ConnectionPolicyFactory from the connection's AmqpUriSpecification; the probe drives those policies directly with
// a BrokerUnreachableException, as an unreachable broker raises it.
//  Retry, #1's comments "Number of retry attempts" and "Wait between retries" (and the reliability list:
//    "retryWaitInMilliseconds: Time to wait between retry attempts"). Case: #1's connection, 5 and 250 ms; the gaps
//    between attempts are measured. Control: the same with retryWaitInMilliseconds: 500, which must double them.
//  Circuit breaker, "circuitBreakTimeInMilliseconds: Time to wait before attempting to reconnect" (#4's 30000).
//    One unreachable broker opens the circuit. Case: #1's 30000 - a call 1 s later is refused, BrokenCircuitException.
//    Control: circuitBreakTimeInMilliseconds: 500 - the call 1 s later goes through.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client.Exceptions;

static RmqMessagingGatewayConnection Connection(int retryWait, int circuitBreak) =>
    // #1, verbatim but for the two values the controls vary
    new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(
            uri: new Uri("amqp://guest:guest@localhost:5672"),
            connectionRetryCount: 5,                  // Number of retry attempts
            retryWaitInMilliseconds: retryWait,       // Back-off: 500 ms before the first retry, doubling
            circuitBreakTimeInMilliseconds: circuitBreak   // Circuit breaker timeout
        ),
        Exchange = new Exchange("paramore.brighter.exchange"),
        Heartbeat = 20  // Heartbeat interval in seconds
    };

static async Task<(int attempts, long[] gaps)> Retry(int retryWait)
{
    var policies = new ConnectionPolicyFactory(Connection(retryWait, 30000));
    var at = new List<long>();
    var clock = Stopwatch.StartNew();
    try
    {
        await policies.RetryPolicyAsync.ExecuteAsync(_ =>
        {
            at.Add(clock.ElapsedMilliseconds);
            throw new BrokerUnreachableException(new Exception("no broker"));
        }, new Dictionary<string, object> { ["queueName"] = "probe" });
    }
    catch (BrokerUnreachableException) { }
    return (at.Count, at.Zip(at.Skip(1), (a, b) => b - a).ToArray());
}

static async Task<string> Breaker(int circuitBreak)
{
    var policies = new ConnectionPolicyFactory(Connection(250, circuitBreak));
    try { await policies.CircuitBreakerPolicyAsync.ExecuteAsync(() => throw new BrokerUnreachableException(new Exception("no broker"))); }
    catch (BrokerUnreachableException) { }
    await Task.Delay(1000);
    try { await policies.CircuitBreakerPolicyAsync.ExecuteAsync(() => Task.CompletedTask); return "1 s later the call goes through"; }
    catch (BrokenCircuitException) { return "1 s later the call is refused, BrokenCircuitException"; }
}

var failed = false;
void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

var r250 = await Retry(250);
var r500 = await Retry(500);
Console.WriteLine($"retry case    250 ms: {r250.attempts} attempts, gaps {string.Join(", ", r250.gaps)} ms");
Console.WriteLine($"retry control 500 ms: {r500.attempts} attempts, gaps {string.Join(", ", r500.gaps)} ms");
Row("connectionRetryCount 5 is 5 retries after the first attempt", r250.attempts == 6 && r500.attempts == 6, $"{r250.attempts} attempts");
Console.WriteLine($"as it stood, \"Wait between retries\" of 250 ms: {(r250.gaps.All(g => g < 400) ? "holds" : "does not hold")}");
Row("as repaired: 500 ms before the first retry, doubling; the control doubles every gap", Within(r250.gaps, [500, 1000, 2000, 4000, 8000]) && Within(r500.gaps, [1000, 2000, 4000, 8000, 16000]), "");

var b30000 = await Breaker(30000);
var b500 = await Breaker(500);
Console.WriteLine($"breaker case    30000 ms: {b30000}");
Console.WriteLine($"breaker control   500 ms: {b500}");
Row("circuitBreakTimeInMilliseconds is how long the circuit stays open", b30000.Contains("refused") && b500.Contains("goes through"), "");
return failed ? 1 : 0;

static bool Within(long[] gaps, int[] expected) => gaps.Length == expected.Length && gaps.Zip(expected).All(p => p.First >= p.Second - 50 && p.First <= p.Second + 250);
