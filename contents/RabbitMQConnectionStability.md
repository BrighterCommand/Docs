---
description: "V10 includes improvements to RabbitMQ connection handling and error recovery, making applications more resilient to network issues and broker restarts."
layout:
  description:
    visible: false
---

# RabbitMQ Connection Stability

> **How-to** · Applies to **Brighter V10** · Prerequisites: [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md)

V10 includes improvements to RabbitMQ connection handling and error recovery, making applications more resilient to network issues and broker restarts.

This guide configures those mechanisms and shows you how to observe them in
production. For the parameters themselves and their defaults, see
[RabbitMQ Connection Reliability Options](/contents/RabbitMQConfiguration.md#rabbitmq-connection-reliability-options).

## Connection Handling Improvements in V10

1. **Enhanced connection pooling**: Improved connection pool management to prevent ghost connections
2. **Better error handling**: More robust error recovery for connection failures
3. **Automatic reconnection**: Improved logic for reconnecting after connection loss
4. **Blocked/Unblocked event monitoring**: Automatic logging of channel blocked events (see below)

The first three need no configuration. The rest of this page covers the ones that do.

## Configuring RabbitMQ Connection Retry

The `AmqpUriSpecification` provides several options for connection reliability:

```csharp
using System;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

// ...
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(
        uri: new Uri("amqp://guest:guest@localhost:5672"),
        connectionRetryCount: 5,                  // Number of retry attempts
        retryWaitInMilliseconds: 250,             // Back-off: 500 ms before the first retry, doubling
        circuitBreakTimeInMilliseconds: 30000     // Circuit breaker timeout
    ),
    Exchange = new Exchange("paramore.brighter.exchange"),
    Heartbeat = 20  // Heartbeat interval in seconds
};
```

Set `connectionRetryCount` from your network's stability and your broker's failover
time: it should be high enough to ride out a failover, and low enough that a genuinely
unreachable broker trips the circuit breaker rather than retrying indefinitely.

Create one `RmqMessagingGatewayConnection` and share it. Brighter manages a pool of
connections behind it and prefers a pooled connection to a new one; constructing
several instances gives you several pools and defeats that.

Test the result. Network partitions and broker restarts are the events these settings
exist for, and a retry configuration that has never been exercised is a guess.

## Configuring RabbitMQ Heartbeats

RabbitMQ uses heartbeats to detect dead TCP connections. If a connection doesn't send a heartbeat within the configured interval, RabbitMQ considers it dead and closes the connection.

```csharp
using System;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

// ...
var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange"),
    Heartbeat = 30  // Check connection health every 30 seconds
};
```

Tuning the interval is a trade: a shorter one detects a dead connection sooner, a
longer one costs less network traffic. Start at the 20-second default and shorten it
only if your failover budget demands it.

## Handling Blocked RabbitMQ Connections

RabbitMQ can block connections when resources are exhausted (memory, disk space, or alarms). Brighter automatically subscribes to blocked/unblocked events and logs them for monitoring.

RabbitMQ blocks a connection when:

- **Memory alarm triggered**: Broker memory usage exceeds the threshold
- **Disk alarm triggered**: Broker disk space is low
- **Resource limits**: Other broker resource limits are reached

When blocked, producers cannot publish messages, and the connection is paused until resources are available.

When a connection is blocked:

1. **Check RabbitMQ status**: Use the Management UI to identify the alarm type
2. **Investigate resource usage**: Check memory, disk, and queue depths
3. **Increase resources**: Add more memory/disk or scale out the cluster
4. **Adjust queue policies**: Set max lengths or TTLs to prevent unbounded growth
5. **Monitor continuously**: Set up dashboards and alerts for RabbitMQ health

Blocking is a backpressure signal, so the durable fix is upstream of the alarm:
configure the broker's memory and disk watermarks for your workload, cap queue depth
with max-length policies so a stalled consumer cannot grow a queue without bound, and
remember that persistent messages consume disk faster than transient ones. Then
rehearse it — deliberately trigger a block in a test environment and watch what your
producers do, because a blocked connection presents as a hang rather than an error.

## Monitoring RabbitMQ Connection Events

Brighter's RabbitMQ gateway logs blocked and unblocked events through your application's
logging, with the credentials in the URI masked:

**Blocked Event**:

```text
[Warning] RMQMessagingGateway: Subscription to amqp://*****@localhost:5672/ blocked. Reason: {reason}
```

**Unblocked Event**:

```text
[Information] RMQMessagingGateway: Subscription to amqp://*****@localhost:5672/ unblocked
```

To monitor blocked connections in your application:

1. **Enable structured logging**: Configure your logger to capture warnings and information logs from Brighter.

2. **Alert on blocked events**: Set up alerts when connections are blocked to investigate resource issues.

3. **Monitor RabbitMQ metrics**: Use RabbitMQ Management UI or Prometheus to track memory and disk usage.

The same logs carry connection failures and recoveries, so route them somewhere you
can query rather than somewhere you can tail. Brighter writes them through
`Microsoft.Extensions.Logging`, under categories that begin
`Paramore.Brighter.MessagingGateway.RMQ`, so any `ILoggerProvider` you add — Serilog, Seq,
OpenTelemetry — receives them the same way the console does here:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

services.AddLogging(logging =>
{
    logging.AddConsole();
    // Blocked (Warning) and unblocked (Information) events, connection failures and recoveries
    logging.AddFilter("Paramore.Brighter.MessagingGateway.RMQ", LogLevel.Information);
});
```

**Use a producer only after the command processor has been resolved.** Brighter's
RabbitMQ classes take their loggers from your container's `ILoggerFactory` once the command
processor or the Dispatcher has been built; before that, they log to a factory with no
providers. A class that logs before then — a producer you send through at startup, say —
keeps that logger for the life of the process, and none of its lines, blocked and
unblocked events included, reach your providers.

## Further Reading

- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md) — the parameters this guide sets
- [RabbitMQ Durability](/contents/RabbitMQDurability.md) — quorum queues and message persistence
- [Migrating to Quorum Queues](/contents/RabbitMQMigrateToQuorumQueues.md) — moving an existing subscription
- [Telemetry](/contents/Telemetry.md) — Brighter's wider observability surface
