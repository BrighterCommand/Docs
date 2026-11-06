// Task 6.4, DynamicMessageDeserialization.md #3 and #4 (the callback), #7 and #8 (its default arm), and the prose
// beside them: "Throw InvalidMessageAction ... This explicitly requests rejection as an unacceptable message"; "An
// ordinary exception such as ArgumentException ... If it escapes getRequestType, the dispatcher logs it, increments
// the unacceptable-message count, and acknowledges the message." Run through Brighter's Reactor on the in-memory
// transport: an InMemorySubscription with the page's callback and an invalid-message channel, three messages
// enqueued - com.example.task.created, com.example.task.updated, and com.example.task.deleted, which has no arm.
// Case: block #4's callback, verbatim - each known type reaches its handler; the unknown one is rejected as
// Unacceptable, so it moves to the invalid-message channel. Control: the same callback with an ArgumentException in
// its default arm - the known types are handled, the unknown one is acknowledged and goes nowhere.
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

public class TaskCreated() : Event(Id.Random()) { public string Name { get; set; } = ""; }   // probe
public class TaskUpdated() : Event(Id.Random()) { public string Name { get; set; } = ""; }   // probe
public static class Seen { public static int Created, Updated; }
public class TaskCreatedHandler : RequestHandler<TaskCreated> { public override TaskCreated Handle(TaskCreated e) { Seen.Created++; return base.Handle(e); } }
public class TaskUpdatedHandler : RequestHandler<TaskUpdated> { public override TaskUpdated Handle(TaskUpdated e) { Seen.Updated++; return base.Handle(e); } }

public static class Program
{
    public static async Task<int> Main()
    {
        // block #4, verbatim
        // Pass this callback as the subscription's getRequestType argument.
        Func<Message, Type> getRequestType = message => message.Header.Type switch
        {
            var t when t == new CloudEventsType("com.example.task.created")
                => typeof(TaskCreated),
            var t when t == new CloudEventsType("com.example.task.updated")
                => typeof(TaskUpdated),
            _ => throw new InvalidMessageAction(
                $"No type mapping found for CloudEvents type '{message.Header.Type}'. " +
                $"Supported types: com.example.task.created, com.example.task.updated"
            )
        };
        // probe: the control, an ordinary exception in the default arm
        Func<Message, Type> ordinary = message => message.Header.Type switch
        {
            var t when t == new CloudEventsType("com.example.task.created") => typeof(TaskCreated),
            var t when t == new CloudEventsType("com.example.task.updated") => typeof(TaskUpdated),
            _ => throw new ArgumentException($"No type mapping found for CloudEvents type '{message.Header.Type}'")
        };

        var caseResult = await Run(getRequestType);
        var controlResult = await Run(ordinary);
        Console.WriteLine($"case    InvalidMessageAction: {caseResult}");
        Console.WriteLine($"control ArgumentException:    {controlResult}");
        var ok = caseResult == "created 1, updated 1, on the invalid-message channel: com.example.task.deleted, left on the channel 0"
              && controlResult == "created 1, updated 1, on the invalid-message channel: none, left on the channel 0";
        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} known types reach their handlers; InvalidMessageAction rejects to the invalid-message channel; ArgumentException is acknowledged");
        return ok ? 0 : 1;
    }

    static async Task<string> Run(Func<Message, Type> getRequestType)
    {
        Seen.Created = Seen.Updated = 0;
        var topic = new RoutingKey($"task.events.{Guid.NewGuid():N}");
        var invalid = new RoutingKey($"task.invalid.{Guid.NewGuid():N}");
        var bus = new InternalBus();
        var subscription = new InMemorySubscription(new SubscriptionName("paramore.example.tasks"), new ChannelName(topic.Value), topic,
            getRequestType: getRequestType, timeOut: TimeSpan.FromMilliseconds(200), messagePumpType: MessagePumpType.Reactor)
        {
            InvalidMessageRoutingKey = invalid
        };

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddConsumers(options =>
            {
                options.Subscriptions = [subscription];
                options.DefaultChannelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
            })
            .AutoFromAssemblies([typeof(TaskCreated).Assembly]);
        builder.Services.AddHostedService<ServiceActivatorHostedService>();
        using var host = builder.Build();
        await host.StartAsync();

        foreach (var type in new[] { "com.example.task.created", "com.example.task.updated", "com.example.task.deleted" })
        {
            var id = Id.Random();
            bus.Enqueue(new Message(new MessageHeader(id, topic, MessageType.MT_EVENT, type: new CloudEventsType(type)),
                new MessageBody(JsonSerializer.Serialize(new { Id = id.Value, Name = type }, new JsonSerializerOptions(JsonSerializerDefaults.Web)))));
        }
        await Task.Delay(3000);
        await host.StopAsync();

        var invalidMessages = bus.Stream(invalid).Select(m => m.Header.Type.Value).ToArray();
        var left = bus.Stream(topic).Count();
        return $"created {Seen.Created}, updated {Seen.Updated}, on the invalid-message channel: {(invalidMessages.Length == 0 ? "none" : string.Join(", ", invalidMessages))}, left on the channel {left}";
    }
}
