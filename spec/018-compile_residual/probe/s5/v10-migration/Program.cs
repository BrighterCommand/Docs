// Task 7.5, V10MigrationGuide.md. Page code is copied as the page shows it; the probe's own lines are marked // probe.
//  #19: "No mapper registration needed ... Brighter uses JsonMessageMapper<T> by default": PersonCreatedPublisher's
//      PostAsync puts a JSON body on the bus with no mapper registered. Control: an explicit mapper registered for the
//      type, whose output differs (a header bag marker), is used instead.
//  #22-#24 and the prose before them: KafkaSubscription.SessionTimeout and PartitionAssignmentStrategy are [Obsolete] and
//      "still work (their values are used as back-fill defaults)"; #23's ClassicGroupProtocol gives the consumer the same
//      settings as #22; #24's ConsumerGroupProtocol opts into KIP-848. Each consumer is built by Brighter's
//      KafkaMessageConsumerFactory and its Confluent ConsumerConfig read. Control: a subscription setting none of them.
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.Kafka;
using Paramore.Brighter.Observability;

// #19, as the page shows it
public class PersonCreated() : Event(Id.Random())
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class PersonCreatedPublisher(IAmACommandProcessor commandProcessor)
{
    public async Task PublishAsync() =>
        await commandProcessor.PostAsync(new PersonCreated
        {
            Name = "Alice",
            Email = "alice@example.com"
        });
}

public class MarkedMapper : IAmAMessageMapperAsync<PersonCreated>   // probe: the control's explicit mapper, async as PostAsync needs
{
    public IRequestContext? Context { get; set; }
    public static int Called;
    public Task<Message> MapToMessageAsync(PersonCreated request, Publication publication, System.Threading.CancellationToken ct = default)
    {
        Called++;
        var header = new MessageHeader(request.Id, publication.Topic!, MessageType.MT_EVENT);
        header.Bag["mapper"] = "explicit";
        return Task.FromResult(new Message(header, new MessageBody(JsonSerializer.Serialize(request))));
    }
    public Task<PersonCreated> MapToRequestAsync(Message message, System.Threading.CancellationToken ct = default) => Task.FromResult(JsonSerializer.Deserialize<PersonCreated>(message.Body.Value)!);
}

public class GreetingEvent() : Event(Id.Random());   // probe

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static async Task<Message> Publish(bool explicitMapper)
    {
        var services = new ServiceCollection();
        var bus = new InternalBus();
        var b = services.AddBrighter().AddProducers(o =>
        {
            o.ProducerRegistry = new InMemoryProducerRegistryFactory(bus,
                [new Publication { Topic = new RoutingKey("PersonCreated"), RequestType = typeof(PersonCreated) }], InstrumentationOptions.All).Create();
            o.Outbox = new InMemoryOutbox(TimeProvider.System);
        });
        if (explicitMapper) b.MapperRegistry(r => r.RegisterAsync<PersonCreated, MarkedMapper>());
        var sp = services.BuildServiceProvider();
        await new PersonCreatedPublisher(sp.GetRequiredService<IAmACommandProcessor>()).PublishAsync();
        return bus.Stream(new RoutingKey("PersonCreated")).Single();
    }

    static readonly KafkaMessagingGatewayConfiguration Gateway = new() { Name = "s5probe", BootStrapServers = ["localhost:9092"] };   // probe
    static ConsumerConfig ConfigOf(KafkaSubscription subscription)
    {
        var consumer = new KafkaMessageConsumerFactory(Gateway).Create(subscription);
        var config = (ConsumerConfig)consumer.GetType().GetField("_consumerConfig", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(consumer)!;
        consumer.Dispose();
        return config;
    }
    static string Show(ConsumerConfig c) => $"group.protocol {c.GroupProtocol?.ToString() ?? "unset"}, session.timeout.ms {c.SessionTimeoutMs?.ToString() ?? "unset"}, partition.assignment.strategy {c.PartitionAssignmentStrategy?.ToString() ?? "unset"}";

    public static async Task<int> Main()
    {
        // #19
        var m = await Publish(false);
        var body = JsonDocument.Parse(m.Body.Value).RootElement;
        Row("V10MigrationGuide.md #19, no mapper registered", body.GetProperty("name").GetString() == "Alice" && !m.Header.Bag.ContainsKey("mapper"),
            $"on the bus: {m.Body.Value.Length} bytes, name {body.GetProperty("name").GetString()}, email {body.GetProperty("email").GetString()}, type {m.Header.MessageType}");
        var c = await Publish(true);
        Console.WriteLine($"     control, an explicit mapper registered: MarkedMapper called {MarkedMapper.Called}; header bag mapper = {(c.Header.Bag.TryGetValue("mapper", out var v) ? v : "(none)")}");
        if (MarkedMapper.Called != 1) failed = true;

        // #22-#24
        var obsolete = new[] { "SessionTimeout", "PartitionAssignmentStrategy" }
            .Select(p => $"{p} {(typeof(KafkaSubscription).GetProperty(p)!.GetCustomAttribute<ObsoleteAttribute>() != null ? "[Obsolete]" : "not obsolete")}");
        Console.WriteLine($"     {string.Join(", ", obsolete)}");

        var s22 = new KafkaSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: new RoutingKey("greeting.event"),
            groupId: "my-consumer-group",
            sessionTimeout: TimeSpan.FromSeconds(30),
            partitionAssignmentStrategy: PartitionAssignmentStrategy.Range
        );
        var s23 = new KafkaSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: new RoutingKey("greeting.event"),
            groupId: "my-consumer-group"
        )
        {
            GroupProtocol = new ClassicGroupProtocol
            {
                SessionTimeout = TimeSpan.FromSeconds(30),
                PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range
            }
        };
        var s24 = new KafkaSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: new RoutingKey("greeting.event"),
            groupId: "my-consumer-group"
        )
        {
            GroupProtocol = new ConsumerGroupProtocol()
        };
        var none = new KafkaSubscription<GreetingEvent>(new SubscriptionName("paramore.example.greeting"), new ChannelName("greeting.event"),
            new RoutingKey("greeting.event"), groupId: "my-consumer-group");   // probe: control

        var c22 = ConfigOf(s22); var c23 = ConfigOf(s23); var c24 = ConfigOf(s24); var cn = ConfigOf(none);
        Row("V10MigrationGuide.md #22, the obsolete properties still work", c22.SessionTimeoutMs == 30000 && c22.PartitionAssignmentStrategy == PartitionAssignmentStrategy.Range, Show(c22));
        Row("V10MigrationGuide.md #23, ClassicGroupProtocol, the same settings", c23.SessionTimeoutMs == 30000 && c23.PartitionAssignmentStrategy == PartitionAssignmentStrategy.Range && c23.GroupProtocol is null or GroupProtocol.Classic, Show(c23));
        Row("V10MigrationGuide.md #24, ConsumerGroupProtocol is KIP-848", c24.GroupProtocol == GroupProtocol.Consumer, Show(c24));
        Console.WriteLine($"     control, none set: {Show(cn)}");
        return failed ? 1 : 0;
    }
}
