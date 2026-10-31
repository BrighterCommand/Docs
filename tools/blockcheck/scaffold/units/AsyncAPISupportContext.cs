// Types and values AsyncAPISupport.md names in its blocks and never declares.
//
// The page's fragments call into a host, a host builder and a consumers' `options` the reader's
// own Program.cs creates, and subscribe to a `PaymentReceivedEvent` the page never shows (it
// declares `OrderCreatedEvent`, in block 5). Each value is typed from a pinned package and returns
// a default, so a call a block makes on it is checked against the real type.
//
// blockcheck: using static AsyncAPISupportContext;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public static class AsyncAPISupportContext
{
    // blocks 1, 6, 7, 9: `services.AddConsumers(…)`
    public static IServiceCollection services => null!;

    // block 2: `var host = builder.Build();`
    public static IHostBuilder builder => null!;

    // block 10: `host.GenerateAsyncApiDocumentAsync("asyncapi.json")`
    public static IHost host => null!;

    // block 3: `options.Subscriptions = new Subscription[] { … }`
    public static ConsumersOptions options => null!;
}

// block 3: `new RmqSubscription<PaymentReceivedEvent>(…)`
public class PaymentReceivedEvent() : Event(Id.Random());
