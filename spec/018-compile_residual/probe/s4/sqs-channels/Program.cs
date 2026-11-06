// Task 6.4, AWSSQSConfiguration.md #7-#10 and the sentences before them, against LocalStack 4.4.
// Each subscription is the page's, verbatim but for its names, which carry a run suffix so that every run makes new
// resources. Brighter's ChannelFactory makes the channel (makeChannels: Create); the probe then reads what exists.
//  #7  Pub/Sub, standard: an SNS topic, an SQS queue subscribed to it, the queue not FIFO.
//  #8  Pub/Sub, FIFO: the topic and queue FIFO, content-based deduplication on the topic.
//  #9  Point-to-point, standard: a queue and no topic.
//  #10 Point-to-point, FIFO: a FIFO queue with content-based deduplication, no topic.
//  The sentences before #8 and #10: "both the topic and queue names must end with .fifo" / "The queue name must end
//  with .fifo" (as it stood at 6.3). Control: #8 and #10 with names that do not end in .fifo.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.AWSSQS;

public class GreetingEvent() : Event(Id.Random());   // probe

public static class Program
{
    static readonly BasicAWSCredentials Credentials = new("test", "test");
    static readonly AWSMessagingGatewayConnection Connection = new(Credentials, RegionEndpoint.USEast1, c => c.ServiceURL = "http://localhost:4566");
    static readonly AmazonSQSClient Sqs = new(Credentials, new AmazonSQSConfig { ServiceURL = "http://localhost:4566" });
    static readonly AmazonSimpleNotificationServiceClient Sns = new(Credentials, new AmazonSimpleNotificationServiceConfig { ServiceURL = "http://localhost:4566" });
    static readonly string Run = Guid.NewGuid().ToString("N")[..6];

    static async Task<string> Made(SqsSubscription subscription)
    {
        string created;
        try { new ChannelFactory(Connection).CreateSyncChannel(subscription); created = "made"; }
        catch (Exception e) { return $"throws {e.GetType().Name}: {e.Message.Split('\n')[0]}"; }
        var queues = (await Sqs.ListQueuesAsync(new ListQueuesRequest { QueueNamePrefix = "" })).QueueUrls.Where(u => u.Contains(Run)).ToList();
        var parts = new List<string>();
        foreach (var url in queues)
        {
            var name = url.Split('/').Last();
            if (!name.Contains(subscription.ChannelName.Value.Replace('.', '_').Replace("_fifo", "").Split('_')[0])) continue;
            var attrs = (await Sqs.GetQueueAttributesAsync(url, ["FifoQueue", "ContentBasedDeduplication"])).Attributes;
            parts.Add($"queue {name} fifo={attrs.GetValueOrDefault("FifoQueue", "false")} dedup={attrs.GetValueOrDefault("ContentBasedDeduplication", "false")}");
        }
        var topics = (await Sns.ListTopicsAsync()).Topics.Where(t => t.TopicArn.Contains(Run)).ToList();
        foreach (var t in topics)
        {
            var a = (await Sns.GetTopicAttributesAsync(t.TopicArn)).Attributes;
            var subs = (await Sns.ListSubscriptionsByTopicAsync(t.TopicArn)).Subscriptions.Count;
            parts.Add($"topic {t.TopicArn.Split(':').Last()} fifo={a.GetValueOrDefault("FifoTopic", "false")} dedup={a.GetValueOrDefault("ContentBasedDeduplication", "false")} subscriptions={subs}");
        }
        foreach (var url in queues) await Sqs.DeleteQueueAsync(url);
        foreach (var t in topics) await Sns.DeleteTopicAsync(t.TopicArn);
        return $"{created}: {string.Join("; ", parts)}";
    }

    public static async Task<int> Main()
    {
        var failed = false;
        void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

        // #7
        var r7 = await Made(new SqsSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("greeting.subscriber"),
            channelName: new ChannelName($"greeting{Run}.queue"),
            routingKey: new RoutingKey($"greeting{Run}.topic"),
            channelType: ChannelType.PubSub,
            makeChannels: OnMissingChannel.Create
        ));
        Row("#7  Pub/Sub standard", r7.Contains("queue ") && r7.Contains("fifo=false dedup=false; topic") && r7.Contains("subscriptions=1"), r7);

        // #8
        var r8 = await Made(new SqsSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("greeting.subscriber.fifo"),
            channelName: new ChannelName($"greeting{Run}.queue.fifo"),
            routingKey: new RoutingKey($"greeting{Run}.topic.fifo"),
            channelType: ChannelType.PubSub,
            makeChannels: OnMissingChannel.Create,
            topicAttributes: new SnsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true),
            queueAttributes: new SqsAttributes(type: SqsType.Fifo)
        ));
        Row("#8  Pub/Sub FIFO", r8.Contains(".fifo fifo=true") && r8.Contains("topic") && r8.Contains("fifo=true dedup=true subscriptions=1"), r8);

        // #9
        var r9 = await Made(new SqsSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("greeting.consumer"),
            channelName: new ChannelName($"greeting{Run}.queue"),
            routingKey: new RoutingKey($"greeting{Run}.queue"),
            channelType: ChannelType.PointToPoint,
            makeChannels: OnMissingChannel.Create
        ));
        Row("#9  point-to-point standard", r9.Contains("fifo=false") && !r9.Contains("topic "), r9);

        // #10
        var r10 = await Made(new SqsSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("greeting.consumer.fifo"),
            channelName: new ChannelName($"greeting{Run}.queue.fifo"),
            routingKey: new RoutingKey($"greeting{Run}.queue.fifo"),
            channelType: ChannelType.PointToPoint,
            makeChannels: OnMissingChannel.Create,
            queueAttributes: new SqsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true)
        ));
        Row("#10 point-to-point FIFO", r10.Contains("fifo=true dedup=true") && !r10.Contains("topic "), r10);

        // controls: the FIFO subscriptions with names that do not end in .fifo
        var c8 = await Made(new SqsSubscription<GreetingEvent>(subscriptionName: new SubscriptionName("greeting.subscriber.fifo"),
            channelName: new ChannelName($"greeting{Run}.queue"), routingKey: new RoutingKey($"greeting{Run}.topic"),
            channelType: ChannelType.PubSub, makeChannels: OnMissingChannel.Create,
            topicAttributes: new SnsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true), queueAttributes: new SqsAttributes(type: SqsType.Fifo)));
        Console.WriteLine($"control #8 without .fifo: {c8}");
        var c10 = await Made(new SqsSubscription<GreetingEvent>(subscriptionName: new SubscriptionName("greeting.consumer.fifo"),
            channelName: new ChannelName($"greeting{Run}.queue"), routingKey: new RoutingKey($"greeting{Run}.queue"),
            channelType: ChannelType.PointToPoint, makeChannels: OnMissingChannel.Create,
            queueAttributes: new SqsAttributes(type: SqsType.Fifo, contentBasedDeduplication: true)));
        Console.WriteLine($"control #10 without .fifo: {c10}");
        Console.WriteLine($"as it stood, \"must end with .fifo\": {(c8.StartsWith("made") && c10.StartsWith("made") ? "does not hold, names without it work" : "holds")}");
        Row("as repaired: Brighter appends .fifo when the name lacks it", c8.Contains("_queue.fifo fifo=true") && c8.Contains("_topic.fifo fifo=true") && c10.Contains("_queue.fifo fifo=true"), "both controls");
        return failed ? 1 : 0;
    }
}
