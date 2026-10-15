// Types and values PolicyRetryAndCircuitBreaker.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static PolicyRetryAndCircuitBreakerContext;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Polly;
using Polly.Registry;

public static class PolicyRetryAndCircuitBreakerContext
{
    public static IAmAHandlerFactorySync handlerFactory => null!;
    public static PolicyRegistry policyRegistry => null!;
    public static SubscriberRegistry registry => null!;
    // blocks 4, 5, 7, 9, 12, 13, 15: the registry block 3 creates
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
    // block 15: `commandProcessor.Send(…)`
    public static IAmACommandProcessor commandProcessor => null!;
    // block 13: the hedged call, `() => CallAlternativeEndpoint()`
    public static ValueTask<Outcome<HttpResponseMessage>> CallAlternativeEndpoint() => default;
    // block 14: `await SomeAsyncOperation(cancellationToken)`
    public static Task SomeAsyncOperation(CancellationToken cancellationToken) => Task.CompletedTask;
}

// blocks 1, 2, 6, 8, 9, 14, 15: `RequestHandler<MyCommand>`, `new MyCommand()`
public class MyCommand() : Command(Id.Random());

// block 10: `RequestHandler<ProcessOrderCommand>`
public class ProcessOrderCommand() : Command(Id.Random());

// block 10: `RequestHandler<ProcessPaymentCommand>`
public class ProcessPaymentCommand() : Command(Id.Random());
