// Task 4.5a (D19), #4335. Claim (BrighterInboxSupport.md): at 10.7.0 the InboxConfiguration set in
// AddConsumers reaches handlers' pipelines only when the application also calls AddProducers; in a
// consumer-only application a duplicate is handled again, with no error. Workaround: [UseInbox] on
// the handler, which works whether or not producers are registered.
// Cases, by argument:
//   consumer  - AddConsumers with a global InboxConfiguration, no AddProducers; the same command twice
//   producers - the same, plus AddProducers (the page: "gets the global Inbox as configured")
//   attribute - consumer-only, no global configuration; the handler carries [UseInbox] (the workaround)
// Each prints the handler count and whether the duplicate threw. The Inbox did its job when the count
// is 1; the defect shows as a count of 2. Exit 0 when the count is 1, else 1.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var scenario = args.Length > 0 ? args[0] : "consumer";
var inbox = new InMemoryInbox(TimeProvider.System);
var services = new ServiceCollection();
services.AddSingleton<IAmAnInboxSync>(inbox);
services.AddSingleton<IAmAnInboxAsync>(inbox);

var builder = services.AddConsumers(options =>
{
    if (scenario != "attribute")
        options.InboxConfiguration = new InboxConfiguration(inbox, actionOnExists: OnceOnlyAction.Throw);
});
if (scenario == "producers")
{
    var bus = new InternalBus();
    builder.AddProducers(options =>
    {
        options.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
            new[] { new Publication { Topic = new RoutingKey("probe.topic"), RequestType = typeof(GlobalCommand) } },
            InstrumentationOptions.None).Create();
    });
}
builder.Handlers(registry =>
{
    registry.Register<GlobalCommand, GlobalCommandHandler>();
    registry.Register<AttributedCommand, AttributedCommandHandler>();
});

using var provider = services.BuildServiceProvider();
var processor = provider.GetRequiredService<IAmACommandProcessor>();

string thrown = "none";
int count;
if (scenario == "attribute")
{
    var command = new AttributedCommand();
    processor.Send(command);
    try { processor.Send(command); } catch (Exception e) { thrown = e.GetType().Name; }
    count = AttributedCommandHandler.Handled;
}
else
{
    var command = new GlobalCommand();
    processor.Send(command);
    try { processor.Send(command); } catch (Exception e) { thrown = e.GetType().Name; }
    count = GlobalCommandHandler.Handled;
}

var version = typeof(CommandProcessor).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false) is [System.Reflection.AssemblyInformationalVersionAttribute v, ..] ? v.InformationalVersion.Split('+')[0] : "?";
Console.WriteLine($"{(count == 1 ? "ok  " : "FAIL")} {version} {scenario}: handled = {count}, duplicate threw {thrown}");
return count == 1 ? 0 : 1;

public class GlobalCommand() : Command(Id.Random());
public class AttributedCommand() : Command(Id.Random());

public class GlobalCommandHandler : RequestHandler<GlobalCommand>
{
    public static int Handled;
    public override GlobalCommand Handle(GlobalCommand command)
    {
        Handled++;
        return base.Handle(command);
    }
}

public class AttributedCommandHandler : RequestHandler<AttributedCommand>
{
    public static int Handled;
    [UseInbox(step: 0, contextKey: typeof(AttributedCommandHandler), onceOnly: true, onceOnlyAction: OnceOnlyAction.Throw)]
    public override AttributedCommand Handle(AttributedCommand command)
    {
        Handled++;
        return base.Handle(command);
    }
}
