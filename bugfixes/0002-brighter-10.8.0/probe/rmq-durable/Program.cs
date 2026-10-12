// Brighter 10.8.0: RMQ.Async subscriptions declare durable queues by default (RabbitMQConfiguration.md).
// Case: a queue that already exists as NON-durable, read with the default isDurable -> the redeclare fails.
// Controls: the same queue read with isDurable: false works; a fresh queue with the default is declared durable.
// Needs RabbitMQ on 5673 (management on 15673).
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

var http = new HttpClient { BaseAddress = new Uri("http://localhost:15673/api/") };
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest")));
// The legacy queue: created non-durable, as RMQ.Async before 10.8.0 created it by default.
(await http.PutAsync("queues/%2F/legacy.queue",
    new StringContent("{\"durable\":false,\"auto_delete\":false}", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();

var connection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5673")),
    Exchange = new Exchange("probe.exchange")
};

async Task<string> Read(string label, string queue, bool? isDurable)
{
    var subscription = isDurable is null
        ? new RmqSubscription(new SubscriptionName(label), new ChannelName(queue), new RoutingKey(queue), requestType: typeof(ProbeEvent), messagePumpType: MessagePumpType.Proactor)
        : new RmqSubscription(new SubscriptionName(label), new ChannelName(queue), new RoutingKey(queue), requestType: typeof(ProbeEvent), isDurable: isDurable.Value, messagePumpType: MessagePumpType.Proactor);
    var consumer = new RmqMessageConsumerFactory(connection).CreateAsync(subscription);
    try { var m = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(500)); return $"{label}: received {m.Length} (type {m[0].Header.MessageType})"; }
    catch (Exception e) { return $"{label}: {e.GetType().Name}: {e.Message.Split('\n')[0]} | inner: {e.InnerException?.Message.Split('\n')[0]}"; }
}

async Task<string> Durable(string queue) =>
    JsonDocument.Parse(await http.GetStringAsync($"queues/%2F/{queue}")).RootElement.GetProperty("durable").ToString();

Console.WriteLine(await Read("case    legacy non-durable queue, default isDurable", "legacy.queue", null));
Console.WriteLine(await Read("control legacy non-durable queue, isDurable: false", "legacy.queue", false));
Console.WriteLine(await Read("control fresh queue, default isDurable", "fresh.queue", null));
Console.WriteLine($"control fresh.queue declared durable: {await Durable("fresh.queue")}");
public class ProbeEvent() : Event(Id.Random());
