// Task 6.4, KafkaConfiguration.md, against apache/kafka:4.0.2 on localhost:9092.
//  #9-#13, #18: each configHook's settings reach the consumer's Confluent ConsumerConfig. Each hook is the page's,
//     verbatim; the consumer is built by Brighter's KafkaMessageConsumerFactory and its ConsumerConfig read by
//     reflection. Control: the same subscription without the hook - none of the values.
//  #12: the hook's SessionTimeoutMs wins over the subscription's (the hook runs after the group protocol is applied).
//     Control: no hook - the subscription's default session timeout.
//  #15, as it stood at 6.3: "CooperativeSticky ... Attempting to use it will throw an ArgumentOutOfRangeException".
//     Case: the page's ClassicGroupProtocol { PartitionAssignmentStrategy = CooperativeSticky }, (a) a message
//     produced, received, acknowledged and the consumer closed, and (b) a rebalance, a second consumer joining a
//     three-partition topic. Control: the same with RoundRobin, the default, which the page says works.
using System;
using System.Collections.Generic;
using System.Reflection;
using Confluent.Kafka;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

public class GreetingEvent() : Event(Id.Random());   // probe

public static class Program
{
    static bool failed;
    static readonly KafkaMessagingGatewayConfiguration Gateway = new() { Name = "s4probe", BootStrapServers = ["localhost:9092"] };

    static ConsumerConfig ConfigOf(Action<ConsumerConfig>? hook)
    {
        var subscription = new KafkaSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: new RoutingKey("s4probe.config"),
            groupId: "s4probe-config",
            configHook: hook);
        var consumer = new KafkaMessageConsumerFactory(Gateway).Create(subscription);
        var config = (ConsumerConfig)consumer.GetType().GetField("_consumerConfig", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(consumer)!;
        consumer.Dispose();
        return config;
    }

    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static string RoundTrip(PartitionAssignmentStrategy strategy)
    {
        var topic = new RoutingKey($"s4probe.{strategy}.{Guid.NewGuid():N}");
        var producers = new KafkaProducerRegistryFactory(Gateway,
            [new KafkaPublication { Topic = topic, MakeChannels = OnMissingChannel.Create, NumPartitions = 1, ReplicationFactor = 1 }]).Create();
        var producer = (IAmAMessageProducerSync)producers.LookupBy(topic);
        var subscription = new KafkaSubscription<GreetingEvent>(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: topic,
            groupId: $"s4probe-{strategy}-{Guid.NewGuid():N}",
            makeChannels: OnMissingChannel.Create, numOfPartitions: 1, replicationFactor: 1, commitBatchSize: 1)
        {
            GroupProtocol = new ClassicGroupProtocol
            {
                PartitionAssignmentStrategy = strategy
            }
        };
        try
        {
            var consumer = new KafkaMessageConsumerFactory(Gateway).Create(subscription);
            producer.Send(new Message(new MessageHeader(Id.Random(), topic, MessageType.MT_EVENT), new MessageBody("{}")));
            ((IDisposable)producer).Dispose();
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < deadline)
            {
                foreach (var m in consumer.Receive(TimeSpan.FromSeconds(1)))
                {
                    if (m.Header.MessageType == MessageType.MT_EVENT)
                    {
                        consumer.Acknowledge(m);
                        consumer.Dispose();
                        return "received, acknowledged, closed";
                    }
                }
            }
            consumer.Dispose();
            return "nothing received in 30 s";
        }
        catch (Exception e)
        {
            return $"throws {e.GetType().Name}: {e.Message.Split('\n')[0]}";
        }
    }

    // #15, a rebalance: consumer A owns a topic's three partitions and acknowledges what it reads; consumer B joins the group,
    // which revokes partitions from A (A's revoke handler commits its offsets); both poll, then close.
    static string Rebalance(PartitionAssignmentStrategy strategy)
    {
        var topic = new RoutingKey($"s4probe.rebalance.{strategy}.{Guid.NewGuid():N}");
        var group = $"s4probe-rebalance-{strategy}-{Guid.NewGuid():N}";
        using (var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build())   // three partitions, made here
            admin.CreateTopicsAsync([new Confluent.Kafka.Admin.TopicSpecification { Name = topic.Value, NumPartitions = 3, ReplicationFactor = 1 }]).GetAwaiter().GetResult();
        var producers = new KafkaProducerRegistryFactory(Gateway,
            [new KafkaPublication { Topic = topic, MakeChannels = OnMissingChannel.Create, NumPartitions = 3, ReplicationFactor = 1 }]).Create();
        var producer = (IAmAMessageProducerSync)producers.LookupBy(topic);
        KafkaSubscription<GreetingEvent> Subscription() => new(
            subscriptionName: new SubscriptionName("paramore.example.greeting"),
            channelName: new ChannelName("greeting.event"),
            routingKey: topic, groupId: group,
            makeChannels: OnMissingChannel.Create, numOfPartitions: 3, replicationFactor: 1, commitBatchSize: 2)
        {
            GroupProtocol = new ClassicGroupProtocol { PartitionAssignmentStrategy = strategy }
        };
        int acked = 0, ackedB = 0;
        try
        {
            using var a = new KafkaMessageConsumerFactory(Gateway).Create(Subscription());
            for (var i = 0; i < 12; i++)
                producer.Send(new Message(new MessageHeader(Id.Random(), topic, MessageType.MT_EVENT) { PartitionKey = $"key-{i}" }, new MessageBody("{}")));
            ((KafkaMessageProducer)producer).Flush();
            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (acked < 5 && DateTime.UtcNow < deadline)
                foreach (var m in a.Receive(TimeSpan.FromSeconds(1)))
                    if (m.Header.MessageType == MessageType.MT_EVENT) { a.Acknowledge(m); acked++; }
            using var b = new KafkaMessageConsumerFactory(Gateway).Create(Subscription());
            for (var round = 0; round < 15; round++)
            {
                foreach (var m in b.Receive(TimeSpan.FromSeconds(1)))
                    if (m.Header.MessageType == MessageType.MT_EVENT) { b.Acknowledge(m); ackedB++; }
                foreach (var m in a.Receive(TimeSpan.FromSeconds(1)))
                    if (m.Header.MessageType == MessageType.MT_EVENT) { a.Acknowledge(m); acked++; }
            }
            static int Assigned(object c) => ((IConsumer<string, byte[]>)c.GetType().GetField("_consumer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(c)!).Assignment.Count;
            var partitionsA = Assigned(a);
            var partitionsB = Assigned(b);
            ((KafkaMessageConsumer)a).Close(); ((KafkaMessageConsumer)b).Close();
            return $"rebalanced, A assigned {partitionsA} partition(s) and {acked} acked, B assigned {partitionsB} partition(s) and {ackedB} acked, both closed";
        }
        catch (Exception e)
        {
            return $"throws {e.GetType().Name} (A {acked} acked, B {ackedB}): {e.Message.Split('\n')[0]}";
        }
    }

    public static int Main()
    {
        var none = ConfigOf(null);

        // #9, verbatim
        var c9 = ConfigOf(config =>
        {
            config.FetchMinBytes = 1024;        // Minimum data per fetch request
            config.FetchWaitMaxMs = 100;        // Max time to wait for fetch.min.bytes
            config.FetchMaxBytes = 52428800;    // Maximum data for all partitions
        });
        Row("#9  fetch settings", c9.FetchMinBytes == 1024 && c9.FetchWaitMaxMs == 100 && c9.FetchMaxBytes == 52428800,
            $"{c9.FetchMinBytes}, {c9.FetchWaitMaxMs}, {c9.FetchMaxBytes} / no hook: {N(none.FetchMinBytes)}, {N(none.FetchWaitMaxMs)}, {N(none.FetchMaxBytes)}");

        // #10, verbatim
        var c10 = ConfigOf(config =>
        {
            config.StatisticsIntervalMs = 30000; // Emit stats every 30 seconds
        });
        Row("#10 statistics", c10.StatisticsIntervalMs == 30000, $"{c10.StatisticsIntervalMs} / no hook: {N(none.StatisticsIntervalMs)}");

        // #11, verbatim
        var c11 = ConfigOf(config =>
        {
            config.EnableSslCertificateVerification = true;
            config.SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https;
        });
        Row("#11 TLS verification", c11.EnableSslCertificateVerification == true && c11.SslEndpointIdentificationAlgorithm == SslEndpointIdentificationAlgorithm.Https,
            $"{c11.EnableSslCertificateVerification}, {c11.SslEndpointIdentificationAlgorithm} / no hook: {N(none.EnableSslCertificateVerification)}, {N(none.SslEndpointIdentificationAlgorithm)}");

        // #12, verbatim
        var c12 = ConfigOf(config =>
        {
            // Note: PartitionAssignmentStrategy should typically be set via the subscription parameter
            // But you can override it here if needed
            config.SessionTimeoutMs = 45000;  // Increase for slow rebalances
        });
        Row("#12 session timeout, the hook over the subscription", c12.SessionTimeoutMs == 45000 && none.SessionTimeoutMs != 45000,
            $"{c12.SessionTimeoutMs} / no hook: {N(none.SessionTimeoutMs)}");

        // #13, verbatim
        var c13 = ConfigOf(config =>
        {
            config.Debug = "consumer,cgrp,topic,fetch"; // Enable debug logging
            config.StatisticsIntervalMs = 10000;        // Frequent statistics for debugging
        });
        Row("#13 debug", c13.Debug == "consumer,cgrp,topic,fetch" && c13.StatisticsIntervalMs == 10000,
            $"{c13.Debug}, {c13.StatisticsIntervalMs} / no hook: {N(none.Debug)}, {N(none.StatisticsIntervalMs)}");

        // #18, verbatim
        var c18a = ConfigOf(config => config.GroupInstanceId = $"greeter-{Guid.NewGuid()}");
        var c18b = ConfigOf(config => config.GroupInstanceId = $"greeter-{Guid.NewGuid()}");
        Row("#18 a unique group.instance.id per consumer", c18a.GroupInstanceId!.StartsWith("greeter-") && c18a.GroupInstanceId != c18b.GroupInstanceId,
            $"{c18a.GroupInstanceId} / {c18b.GroupInstanceId} / no hook: {N(none.GroupInstanceId)}");

        var stickyRebalance = Rebalance(PartitionAssignmentStrategy.CooperativeSticky);
        var roundRobinRebalance = Rebalance(PartitionAssignmentStrategy.RoundRobin);
        Console.WriteLine($"#15 case    CooperativeSticky, rebalance: {stickyRebalance}");
        Console.WriteLine($"#15 control RoundRobin, rebalance: {roundRobinRebalance}");
        var sticky = RoundTrip(PartitionAssignmentStrategy.CooperativeSticky);
        var roundRobin = RoundTrip(PartitionAssignmentStrategy.RoundRobin);
        Console.WriteLine($"#15 case    CooperativeSticky: {sticky}");
        Console.WriteLine($"#15 control RoundRobin: {roundRobin}");
        Console.WriteLine($"#15 as it stood, \"will throw an ArgumentOutOfRangeException\": {(sticky.StartsWith("throws") || stickyRebalance.StartsWith("throws") ? "thrown" : "not thrown")}");
        Row("#15 as repaired: CooperativeSticky round-trips and rebalances without an exception, as RoundRobin does",
            sticky.StartsWith("received") && stickyRebalance.StartsWith("rebalanced") && roundRobin.StartsWith("received") && roundRobinRebalance.StartsWith("rebalanced"),
            "both strategies, both runs");
        return failed ? 1 : 0;
    }

    static string N(object? o) => o?.ToString() ?? "unset";
}
