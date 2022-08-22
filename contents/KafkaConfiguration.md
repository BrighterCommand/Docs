# Kafka Configuration

## General

Kafka is OSS message-oriented-middleware and is [well documented](https://kafka.apache.org/documentation/#gettingStarted). Brighter handles the details of sending to or receiving from Kafka. You may find it useful to understand the [building blocks](https://kafka.apache.org/documentation/#introduction) of the protocol. Brighter's Kafka support is implemented on top of the Confluent .NET client, and you might find the [documentation for the .NET client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html) helpful when debugging, but you should not have to interact with it directly to use Brighter (although we expose many of its configuration options).

Kafka has two main roles:

- **Producer**: A producer sends events to a **Topic** on a Kafka broker.
- **Consumer**: A consumer reads events from a **Topic** on a Kafka broker.

**Topics** are append-only streams of events. Multiple producers can write to a topic, and multiple consumers can read from one. A **consumer** uses an **offset** into the stream to indicate the event it wants to read. Kafka does not delete an event from the stream when it is ack'd by the consumer; instead a **consumer** increments its **offset** once an item has been read so that it can avoid processing the same event twice. See [Offset Management](#offset-management) for more on how Brighter manages **consumer offsets**. As a result the lifetime of events on a stream is instead a configuration setting for the stream. 

As a **consumer** manages an **offset** to record events that is has read, you cannot scale an application that wishes to consume a **topic** by increasing the number of **consumers**--they don't share an offset--without partitioning the **topic**. If you supply a **partition key**, a **partition** uses consistent hashing to slice a **topic** into a number of streams; otherwise it will use round-robin. See [this documentation] (https://jaceklaskowski.gitbooks.io/apache-kafka/content/kafka-producer-internals-DefaultPartitioner.html) for more. Each **partition** is only read by a single **consumer** within the application. All of the consumers for an application should share the same group id, called a **consumer group** in Kafka. As each **consumer** tracks the **offset** for the **partitions** it is reading, it is possible to have multiple **consumers** read and process the same **topic**. 

A **consumer** may read from *multiple* **partitions**, but only one **consumer** may read from a **partition** at one time in a given **consumer group**. Kafka will assign partitions across the pool of consumers for the **consumer group**. When the pool changes, a **rebalance** occurs, which may mean that a consumer changes the **partition** that it is assigned within the **consumer group**. Brighter favors *sticky assignment of partitions* to avoid unnecessary churn of partitions.

In addition to the Producer API and Consumer API Kafka streams have features such as the Streams API and the Connect API. We do not use either of these from Brighter.

## Connection

The Connection to Kafka is provided by an **KafkaMessagingGatewayConnection** which allows you to configure the following:

- **BootstrapServers**: A **bootstrap** server is a well-known broker through which we discover the servers in the Kafka cluster that we can connect to. You should supply a comma-separated list of host and port pairs. These are the addresses of the Kafka brokers in the "bootstrap" Kafka cluster.
- **Debug**: A comma-separated list of debug contexts to enable.  Producer: broker, topic, msg. Consumer: consumer, cgrp, topic, fetch.
- **Name**: An identifier to use for the client.
- **SaslMechanisms**: If any, what is the protocol used for authenticated connection to the Kafka broker: plain, scram-sha-256, scram-sha-256, gssapi (kerberos), oauthbearer
- **SaslKerberosName**: If using kerberos, what is the connection name.
- **SaslUsername**: SASL username for use with PLAIN and SASL-SCRAM
- **SaslPassword**: SASL password for use with PLAIN and SASL-SCRAM
- **SecurityProtocol**: How are messages between client and server encrypted, if at all: plaintext, ssl, saslplaintext, saslssl
- **SslCaLocation**: Where is the CA certificate located (see [here](https://docs.confluent.io/platform/current/tutorials/examples/clients/docs/csharp.html) for guidance).
- **SslKeystoreLocation**: Path to the client's keystore
- **SslKeystorePassword**: Password for the client's keystore

The following code connects to a local Kafka instance (for development):

``` csharp
	services.AddBrighter(...)
	.UseExternalBus(
	new KafkaProducerRegistryFactory(
		new KafkaMessagingGatewayConfiguration()
		{
			Name = "paramore.brighter.greetingsender",
			BootStrapServers = new[] {"localhost:9092"}
		},
		...//publication, see below
		)
	.Create())
	...

```

The following code connects to a remote Kafka instance. The settings here will depend on how your production broker is configured for access. We show getting secrets from environment variables for simplicity, again you will need to adjust this for your approach to secrets management:

``` csharp
	services.AddBrighter(...)
	.UseExternalBus(
	new KafkaProducerRegistryFactory(
		new KafkaMessagingGatewayConfiguration()
		{
			Name = "paramore.brighter.greetingsender",
			BootStrapServers = new[] { Environment.GetEnvironmentVariable("BOOSTRAP_SERVER")},
			SecurityProtocol = Paramore.Brighter.MessagingGateway.Kafka.SecurityProtocol.SaslSsl,
			SaslMechanisms = Paramore.Brighter.MessagingGateway.Kafka.SaslMechanism.Plain,
			SaslUsername = Environment.GetEnvironmentVariable("SASL_USERNAME"),
			SaslPassword = Environment.GetEnvironmentVariable("SASL_PASSWORD"),
			SslCaLocation = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "/usr/local/etc/openssl@1.1/cert.pem" : null;
		},
		...//publication, see below
		)
	.Create())
	...

```

## Publication

For more on a *Publication* see the material on an *External Bus* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#using-an-external-bus).

We allow you to configure properties for both Brighter and the Confluent .NET client. Because there are many properties on the Confluent .NET Client we also configure a callback to let you inspect and modify the configuration that we will pass to the client if you so desire. This can be used to add properties we do not support or adjust how we set them.

- **Replication**: how many ISR nodes must receive the record before the producer can consider the write successful. Default is Acks.All.
- **BatchNumberMessages**: Maximum number of messages batched in one MessageSet. Default is 10.
- **EnableIdempotence**: Messages are produced once only. Will adjust the following if not set: `max.in.flight.requests.per.connection=5` (must be less than or equal to 5), `retries=INT32_MAX` (must be greater than 0), `acks=all`, `queuing.strategy=fifo`. Default is true.
- **LingerMs**: Maximum time, in milliseconds, for buffering data on the producer queue. Default is 5.
- **MessageSendMaxRetries**: How many times to retry sending a failing MessageSet. Note: retrying may cause reordering, set the  max in flight to 1 if you need ordering by when sent. Default is 3.
- **MessageTimeoutMs**: Local message timeout. This value is only enforced locally and limits the time a produced message waits for successful delivery. A time of 0 is infinite. Default is 5000.
- **MaxInFlightRequestsPerConnection**: Maximum number of in-flight requests the  client will send. We default this to 1, so as to allow retries to not de-order the stream.
- **NumPartitions**: How many partitions for this topic. We default to 1.
- **Partitioner**: How do we partition? Defaults to Partitioner.ConsistentRandom.
- **QueueBufferingMaxMessages**: Maximum number of messages allowed on the producer queue. Defaults to 10.
- **QueueBufferingMaxKbytes**: Maximum total message size sum allowed on the producer queue. Defaults to 1048576 bytes (so for 10 messages about 104Kb per message).
- **ReplicationFactor**: What is the replication factor? How many nodes is the topic copied to on the broker? Defaults to 1.
- **RetryBackoff**: The backoff time before retrying a message send. Defaults to 100.
- **RequestTimeoutMs**: The ack timeout of the producer request. This value is only enforced by the broker and relies on Replication being != AcksEnum.None. Defaults to 500.
- **TopicFindTimeoutMs**: How long to wait when asking for topic metadata. Defaults to 5000.
- **TransactionalId**: The unique identifier for this producer, used with transactions

The following example shows how a *Publication* might be configured:

``` csharp
	services.AddBrighter(...)
	.UseExternalBus(
	new KafkaProducerRegistryFactory(
		...,//connection see above
		new KafkaPublication[] {new KafkaPublication()
                {
                    Topic = new RoutingKey("MyTopicName"),
                    NumPartitions = 3,
                    ReplicationFactor = 3,
                    MessageTimeoutMs = 1000,
                    RequestTimeoutMs = 1000,
                    MakeChannels = OnMissingChannel.Create 
                }
		)
	.Create())
	...

```

### Configuration Callback

The Confluent .NET client has a range of configuration options. Some of those can be controlled through the publication. But, to allow you the full range of configuration options for the Confluent client, including new options that may appear, we provide a callback on the **KafkaProducerRegistryFactory**. The registry exposes a method, **SetConfigHook(Action<ProducerConfig> hook)**. The method takes a *delegate* (you can pass a lambda). Your delegate will be called with the *proposed* ProducerConfig (taking into account the *Publication* settings). You can adjust additional parameters at this point.

You can use it as follows:

``` csharp

	var publication = new KafkaPublication()
	{
		Topic = new RoutingKey("MyTopicName"),
		NumPartitions = 3,
		ReplicationFactor = 3,
		MessageTimeoutMs = 1000,
		RequestTimeoutMs = 1000,
		MakeChannels = OnMissingChannel.Create 
	};
	publication.SetConfigHook(config => config.EnableGaplessGuarantee = true)

	services.AddBrighter(...)
	.UseExternalBus(
	new KafkaProducerRegistryFactory(
		...,//connection see above
		new KafkaPublication[] {publication})
	.Create())
	...

```

## Subscription

For more on a *Subscription* see the material on configuring *Service Activator* in [Basic Configuration](/contents/BrighterBasicConfiguration.md#configuring-the-service-activator).

We support a number of Kafka specific *Subscription* options:

- **CommitBatchSize**: We commit processed work (marked as acked or rejected) when a batch size worth of work has been completed (see [below](#offset-management)).
- **GroupId**: Only one consumer in a group can read from a partition at any one time; this preserves ordering. We do not default this value, and expect you to set it.
- **IsolationLevel**: Default to read only committed messages, change if you want to read uncommitted messages. May cause duplicates.
- **MaxPollIntervalMs**: How often the consumer needs to poll for new messages to be considered alive, polling greater than this interval triggers a re-balance. Kafka default to 300000ms
- **NumPartitions**: How many partitions does the topic have? Used for topic creation, if required.
- **OffsetDefault**:  What do we do if there is no offset stored in ZooKeeper for this consumer. Defaults to AutoOffsetReset.Earliest - Begin reading the stream from the start. Options include AutOffsetRest.Latest - Start from now i.e. only consume messages after we start and AutoOffsetReset.Error - which considers it an error if not reset is found
- **ReadCommittedOffsetsTimeOutMs**: How long before attempting to read back committed offsets (mainly used in debugging) is an error. Defaults to 5000.
- **ReplicationFactor**: What is the replication factor? How many nodes is the topic copied to on the broker? Defaults to 1. Used for topic creation if required.
- **SessionTimeoutMs**: If Kafka does not receive a heartbeat from the consumer within this time window, trigger a re-balance. Default is Kafka default of 10s.
- **SweepUncommittedOffsetsIntervalMs**: The interval at which we sweep, looking for offsets that have not been flushed (see [below](#offset-management)).

The following example shows how a subscription might be configured:

``` csharp
 private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
	var subscriptions = new KafkaSubscription[]
	{
		new KafkaSubscription<GreetingEvent>(
			new SubscriptionName("paramore.example.greeting"),
			channelName: new ChannelName("greeting.event"),
			routingKey: new RoutingKey("greeting.event"),
			groupId: Environment.GetEnvironmentVariable("KAFKA_GROUPID"),
			timeoutInMilliseconds: 100,
			commitBatchSize: 5,
			sweepUncommittedOffsetsIntervalMs: 3000
		)
	};
		
	//create the gateway
	var consumerFactory = new KafkaMessageConsumerFactory(
	new KafkaMessagingGatewayConfiguration {...} // see connection information above
	);

	services.AddServiceActivator(options =>
	{
	options.Subscriptions = subscriptions;
	options.ChannelFactory = new ChannelFactory(consumerFactory);
	}).AutoFromAssemblies();


	services.AddHostedService<ServiceActivatorHostedService>();
}
```

## Offset Management

It is important to understand how Brighter manages the **offset** of any **partitions** assigned to your **consumer**.

- Brighter manages committing **offsets** to Kafka. This means we set the Confluent client's *auto store* and *auto commit* properties to *false*.
- The **CommitBatchSize** setting on the *Subscription* determines the size of your buffer. A smaller buffer is less efficient, but if your consumer crashes any **offsets** pending commit in the buffer will be lost, and you will be represented with those records when you next read from the **partition**. We default this value to 10.
- We do not add an offset commit to the buffer until you Ack the request. The message pump will Ack for you once you exit your handler (via return or [throwing an exception](/contents/HandlerFailure.md)).
- Flushing the commit buffer happens on a separate thread. We only run one flush at a time, and we flush a **CommitBatchSize** number of items from the buffer. 
	- A busy consumer may not flush on every increment of the **CommitBatchSize**, as it may need to wait for the last flush to finish. 
	- We won't flush again until we cross the next multiple of the **CommitBatchSize**. For example if the **CommitBatchSize** is 10, and the handler is busy so that by the time the buffer flushes there are 13 pending commits in the buffer, the buffer would only flush 10, and 3 would remain in the buffer; we would not flush the next 10 until the buffer hit 20. 
	- If your **CommitBatchSize** is too low for the throughput, you might find that you miss a flush interval, because you are already flushing. 
	- If you miss a flush on a busy consumer, your buffer will begin to back up. If this continues, you will not catch up with subsequent flushes, which only flush the **CommitBatchSize** each time. This would lead to you continually being "backed up".
	- For this reason you must set a **CommitBatchSize** that keeps pace with the throughput of your consumer. Use a larger **CommitBatchSize** for higher throughput consumers, smaller for lower.
- We sweep uncommitted offsets at an interval. This triggers a flush if no flush has run since the last flush plus the *Subscription's* **SweepUncommittedOffsetsIntervalMs**. 
	- A sweep will not run if a flush is currently running (and will in turn block a flush).
	- A sweep flushes a **CommitBatchSize** worth of commits.
	- It is intended for low-throughput consumers where commits might otherwise languish waiting for a batch-size increment.
	- It is *not* intended to flush a buffer that backs up because the **CommitBatchSize** is too low, and won't function for that. Fix the **CommitBatchSize** instead.
- On a re-balance where we stop processing a **partition** on an individual consumer, we flush the remaining **offsets** for the revoked **partitions**.
	- We configure the consumer to use sticky assignment strategy to avoid unnecessary re-assignments (see the [Confluent documentation](https://www.confluent.io/blog/cooperative-rebalancing-in-kafka-streams-consumer-ksqldb/)). 
- On a consumer shutdown we flush the buffer to commit all **offsets**.

## Requeue with Delay

We don't currently support requeue with delay for Kafka. It might be added in a future release, where the strategy would be to:

- Publish the requeued message to a new stream
- Commit the offset
- Poll that stream with a new subscription but at a greater interval between polling (i.e. the delay)

In the interim you can manually implement that approach if required.







