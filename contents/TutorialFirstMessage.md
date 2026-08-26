---
description: "Send an event from one process, over a RabbitMQ exchange, to a second process that handles it."
layout:
  description:
    visible: false
---

# Your First Message Over a Broker

> **Tutorial** · Applies to **Brighter V10** · Prerequisites: [Your First Command](/contents/TutorialFirstCommand.md)

Send an [event](/contents/Glossary.md#event) from one process, over a RabbitMQ exchange, to a
second process that handles it.

This is the second rung of the ladder. Rung 1 sent a request to a handler inside one process.
Here the handler moves into a *different* process, and the two only ever agree on a name — the
[routing key](/contents/Glossary.md#routing-key). Your code barely changes; what changes is
that the two halves can now be deployed, restarted and scaled apart from each other.

## What You'll Build: Your First Message Over a Broker

RabbitMQ in Docker, and a solution with three projects:

| Project | What it is |
|---|---|
| `Greetings` | a class library holding `GreetingEvent` — the one type both processes share |
| `GreetingsSender` | a console app that publishes the event and exits |
| `GreetingsReceiver` | a console app that runs until you stop it, handling whatever arrives |

The sender prints `Published greeting.event` and exits. The receiver prints
`Received: Hello from the sender` and keeps running.

The shared library is the only code the two processes have in common. That is deliberate: in a
real system the sender and the receiver are separately deployed, and often separately owned. A
shared library is convenient here, not required — what actually couples them is the routing
key and the shape of the JSON on the wire.

## Before You Start Your First Message

- **Rung 1 complete.** [Your First Command](/contents/TutorialFirstCommand.md) introduced the
  [Command Processor](/contents/Glossary.md#command-processor), handlers and
  `AutoFromAssemblies()`. This page assumes all three.
- **The .NET 9 SDK.** Check with `dotnet --version`.
- **Docker Desktop**, running, with ports **5672** (AMQP) and **15672** (the management UI)
  free. If something else is on those ports, RabbitMQ will start and your apps will not
  connect.
- **About twenty minutes.** The machine work — create, restore, build — measured **23 seconds**
  on a clean machine with an empty NuGet package cache. Pulling the `rabbitmq:management`
  image the first time takes longer and depends on your connection.

## Step 1: Start RabbitMQ

Create `docker-compose.yml` in a new folder:

```yaml
services:
  rabbitmq:
    image: rabbitmq:management
    hostname: rabbitmq-server
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
```

```bash
docker compose up -d
```

**Expected result:** `docker ps` shows one container running.

> **That command returns before RabbitMQ accepts connections**, because this compose file has
> no healthcheck. Wait until <http://localhost:15672> answers — `guest` / `guest` — before you
> start either app. Starting early prints connection failures until the broker comes up, which
> looks exactly like a broken sample and is not one.

## Step 2: Define the Greeting Event

```bash
dotnet new classlib -n Greetings -f net9.0
dotnet add Greetings package Paramore.Brighter --version 10.7.0
```

Delete the generated `Class1.cs`, and put this in `Greetings/GreetingEvent.cs`:

```csharp
using Paramore.Brighter;

namespace Greetings;

/// <summary>
/// The event both processes share. The sender publishes it; the receiver handles it.
/// The property has a setter because the message is deserialized into a new instance
/// by the receiver, which uses the parameterless constructor.
/// </summary>
public class GreetingEvent : Event
{
    public GreetingEvent() : base(Id.Random()) { }

    public GreetingEvent(string greeting) : base(Id.Random())
    {
        Greeting = greeting;
    }

    public string Greeting { get; set; } = string.Empty;
}
```

An **event** derives from `Event`, where rung 1's command derived from `Command`. The
difference is intent, and it is not cosmetic: a command is addressed to exactly one handler,
while an event is a statement of fact that any number of subscribers may act on. Crossing a
broker is what makes that distinction pay.

> **Both the parameterless constructor and the setter are load-bearing.** The receiver does not
> get your object; it gets bytes, which `System.Text.Json` turns into a *new* instance. Given
> two constructors and no `[JsonConstructor]`, it picks the parameterless one and then assigns
> properties — so a get-only `Greeting` would arrive as an empty string, with no error
> anywhere. If your receiver prints `Received:` and nothing else, this is why.

## Step 3: Build the Sender

```bash
dotnet new console -n GreetingsSender -f net9.0
dotnet add GreetingsSender package Paramore.Brighter.Extensions.DependencyInjection --version 10.7.0
dotnet add GreetingsSender package Paramore.Brighter.MessagingGateway.RMQ.Async --version 10.7.0
dotnet add GreetingsSender package Microsoft.Extensions.Hosting --version 10.0.10
dotnet add GreetingsSender reference Greetings
```

Replace `GreetingsSender/Program.cs`:

```csharp
using System;
using Greetings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange")
};

// A publication tells Brighter where a request type goes: which routing key, on which broker.
// Naming GreetingEvent here is also what loads the Greetings assembly, which matters below:
// AutoFromAssemblies scans the assemblies loaded so far, so anything it must find has to have
// been touched before the call. Reordering this below the registration is a silent no-op.
var producerRegistry = new RmqProducerRegistryFactory(
    rmqConnection,
    [
        new RmqPublication<GreetingEvent>
        {
            Topic = new RoutingKey("greeting.event"),
            MakeChannels = OnMissingChannel.Create
        }
    ]).Create();

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddBrighter()
    .AddProducers(configure => configure.ProducerRegistry = producerRegistry)
    .AutoFromAssemblies();

// The host is built but never run: Post is synchronous, so all we need from it is the
// container. Rung 3 does run the host, because it hosts the Outbox Sweeper.
using (var host = builder.Build())
{
    var commandProcessor = host.Services.GetRequiredService<IAmACommandProcessor>();

    commandProcessor.Post(new GreetingEvent("Hello from the sender"));
}
// Publisher confirms are asynchronous: Post returns once the message is on its way, and the
// broker's acknowledgement arrives later. Disposing the host waits for it — bounded by
// RmqPublication.WaitForConfirmsTimeOutInMilliseconds, 500ms by default — which is why this
// line sits after the brace. Note what it does and does not say: the broker has had its
// chance to confirm. A nack surfaces as a log line rather than an exception, so this message
// means "sent", not "accepted".
Console.WriteLine("Published greeting.event");
```

Three things are new since rung 1:

- **A [publication](/contents/Glossary.md#publication)** is the outbound half of the
  arrangement: *this request type goes to this routing key on this broker*.
  `MakeChannels = OnMissingChannel.Create` tells Brighter to declare the exchange if it is not
  there, which is what saves you a broker-setup step.
- **`Post`, not `Send`.** Rung 1's `Send` ran a handler in-process and returned when it
  finished. `Post` hands the request to the transport. Nothing in this process handles it.
- **The `using (…) { }` block is deliberate, and so is the line after it.** Publisher confirms
  are asynchronous: `Post` returns once the message is on its way, and the broker's
  acknowledgement arrives later. Disposing the host is what waits for it, bounded by
  `RmqPublication.WaitForConfirmsTimeOutInMilliseconds` — **500 ms** by default. Written as
  `using var host`, the `Console.WriteLine` would run *before* that wait.

> **`Published greeting.event` means "sent", not "accepted".** A negative acknowledgement from
> the broker, or a confirm window that lapses, surfaces as a log line rather than an exception —
> so this message prints either way. Rung 3 is where the message stops depending on this process
> staying alive to be delivered at all.

## Step 4: Build the Receiver

```bash
dotnet new console -n GreetingsReceiver -f net9.0
dotnet add GreetingsReceiver package Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection --version 10.7.0
dotnet add GreetingsReceiver package Paramore.Brighter.ServiceActivator.Extensions.Hosting --version 10.7.0
dotnet add GreetingsReceiver package Paramore.Brighter.MessagingGateway.RMQ.Async --version 10.7.0
dotnet add GreetingsReceiver package Microsoft.Extensions.Hosting --version 10.0.10
dotnet add GreetingsReceiver reference Greetings
```

> **The package names say `ServiceActivator` and this page says Dispatcher.** They are the same
> thing: *Dispatcher* is the V10 name for the component that owns the message pumps, and
> `ServiceActivator` is the older name, still carried by the assemblies and the
> `ServiceActivatorHostedService` type so that existing code keeps compiling. You are on the
> right page. See [Dispatcher](/contents/Glossary.md#dispatcher).
>
> This is also why rung 2 pins `Microsoft.Extensions.Hosting` at **10.0.10** rather than rung
> 1's `9.0.0` — `Paramore.Brighter.ServiceActivator.Extensions.Hosting` requires it, and the
> older pin fails the build with `NU1605`.

Put this in `GreetingsReceiver/GreetingEventHandler.cs`:

```csharp
using System;
using Greetings;
using Paramore.Brighter;

namespace GreetingsReceiver;

/// <summary>
/// The pump reads a message from the channel, the mapper turns it back into a
/// <see cref="GreetingEvent"/>, and Brighter dispatches it here. This is a synchronous
/// handler because the subscription runs a Reactor pump.
/// </summary>
public class GreetingEventHandler : RequestHandler<GreetingEvent>
{
    public override GreetingEvent Handle(GreetingEvent @event)
    {
        Console.WriteLine($"Received: {@event.Greeting}");

        return base.Handle(@event);
    }
}
```

That is rung 1's handler with a different request type. Nothing about it knows a broker exists,
which is the point.

Replace `GreetingsReceiver/Program.cs`:

```csharp
using System;
using Greetings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;

// A subscription is the consuming half of a publication: the queue to read, the routing
// key bound to it, and how the pump should run. This is also the process that declares
// the queue and its binding, which is why it has to run first the first time.
//
// Naming GreetingEvent here is also what loads the Greetings assembly. AutoFromAssemblies
// below scans the assemblies loaded so far, so reordering this beneath it is a silent no-op.
//
// isDurable defaults to false, so the queue does not survive a broker restart. That is a
// property of the *queue*, and it is a different question from whether a message survives
// the *sender* crashing — which is what rung 3's durable Outbox is about.
var subscriptions = new Subscription[]
{
    new RmqSubscription<GreetingEvent>(
        new SubscriptionName("greeting.subscription"),
        new ChannelName("greeting.event"),
        new RoutingKey("greeting.event"),
        messagePumpType: MessagePumpType.Reactor,
        makeChannels: OnMissingChannel.Create)
};

var rmqConnection = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
    Exchange = new Exchange("paramore.brighter.exchange")
};

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rmqConnection));
    })
    .AutoFromAssemblies();

builder.Services.AddHostedService<ServiceActivatorHostedService>();

var host = builder.Build();

await host.RunAsync();
```

- **A [subscription](/contents/Glossary.md#subscription)** mirrors the publication: the queue to
  read, the routing key bound to it, and how to run the pump. `"greeting.event"` appears here
  twice — as the channel (queue) name and as the routing key — and once in the sender. Three
  bare strings, deliberately not a shared constant: agreeing on a name over the wire *is* the
  coupling, and hiding it behind a constant would hide the lesson.
- **`MessagePumpType.Reactor`** runs a single-threaded pump, so your handler can be synchronous
  and needs no locking. See [Reactor and Proactor](/contents/ReactorAndProactor.md).
- **`host.RunAsync()`, which rung 1 did not have.** Now there *is* something to host: the
  Dispatcher runs until you stop it.

## Step 5: Run Both Processes

Two terminals, **the receiver first** — it is the process that declares the queue and binding:

```bash
dotnet run --project GreetingsReceiver
```

```bash
dotnet run --project GreetingsSender
```

**Expected result** — the receiver, ending at the greeting and then waiting:

```text
info: Paramore.Brighter.ServiceActivator.Extensions.Hosting.ServiceActivatorHostedService[0]
      Starting hosted service dispatcher
info: Paramore.Brighter.ServiceActivator.Dispatcher[2014651798]
      Dispatcher: Creating consumer number 1 for subscription: greeting.subscription
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessageConsumer[784437019]
      RmqMessageConsumer: Created consumer for queue greeting.event with routing key greeting.event via exchange paramore.brighter.exchange on subscription amqp://*****@localhost:5672/
info: Paramore.Brighter.CommandProcessor[258521805]
      Found 1 pipelines for event: Greetings.GreetingEvent 01a03a64-1e77-7c06-b3d7-7d73826a9fa5
Received: Hello from the sender
info: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessageConsumer[22364147]
      RmqMessageConsumer: Acknowledging message 01a03a64-1e77-7c06-b3d7-7d73826a9fa5 as completed with delivery tag 1
```

And the sender, which exits:

```text
info: Paramore.Brighter.CommandProcessor[1310740404]
      Decoupled invocation of message: Topic:greeting.event Id:01a03a64-1e77-7c06-b3d7-7d73826a9fa5
info: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessageProducer[1379693184]
      Published message: 01a03a64-1e77-7c06-b3d7-7d73826a9fa5
Published greeting.event
```

Both listings are trimmed: each process also logs the whole serialized message on one very long
line, and the receiver logs its hosting environment and content root. Identifiers and
timestamps differ on every run.

The decoded body is worth seeing once, because you never wrote any code to produce it:

```json
{"greeting":"Hello from the sender","correlationId":null,"id":"01a03a64-1e77-7c06-b3d7-7d73826a9fa5"}
```

Brighter serialized your event with its default mapper, `JsonMessageMapper<T>`, and
deserialized it on the other side. Registering a mapper by hand is something you do when the
wire format has to match someone else's contract, not to get started.

> **If you ran the sender first, on a brand-new broker, nothing arrives.** The sender declares
> the *exchange*; the receiver declares the *queue* and the binding. With no queue bound,
> RabbitMQ discards the message — and the publish still succeeds, so the sender prints
> `Published greeting.event` and exits `0`. Start the receiver, then run the sender again.
>
> **Order only matters that first time.** The queue is declared with `autoDelete: false`, so
> once the receiver has run, the queue and its binding outlive it and survive until the broker
> restarts. After that you can run the sender with nothing listening, and the message waits in
> the queue until the receiver comes back.

## Step 6: See It in RabbitMQ

Open <http://localhost:15672> and log in with `guest` / `guest`. Under **Exchanges** and
**Queues** you will find what your two processes declared:

| | Name | Notable |
|---|---|---|
| Exchange | `paramore.brighter.exchange` | type `direct` — the routing key must match exactly |
| Queue | `greeting.event` | `0` messages once the receiver has drained it, `1` consumer |
| Binding | exchange → queue | on routing key `greeting.event` |

If the queue is missing, the receiver has not run. If it is there with messages piling up, the
receiver has stopped but its queue survived — which is the `autoDelete: false` behaviour above.

Both are declared **non-durable**, so a broker restart removes them. That is a property of the
*queue*, and a different question from whether a message survives the *sender* crashing — which
is rung 3's subject.

Stop the broker when you are done:

```bash
docker compose down
```

## What Your First Message Showed You

The handler did not change in any way that matters, and it now runs in another process:

- **A publication and a subscription are two halves of one agreement**, and the agreement is a
  string. The sender knows a routing key; the receiver knows the same routing key. Neither
  knows the other exists.
- **The Dispatcher is the consuming counterpart of the Command Processor.** It owns the pump
  that reads the queue, hands each message to Brighter, and acknowledges it once your handler
  returns. You configure it; you never call it.
- **Acknowledgement happens after your handler returns**, which is why a crash mid-handler
  redelivers rather than loses. That is at-least-once delivery, and it means your handler
  should tolerate seeing the same message twice.

What you still do not have is durability. The message existed only in RabbitMQ: if the sender
had crashed between writing its data and publishing, the two would have disagreed permanently.
[The next rung](/contents/TutorialDurableOutbox.md) puts an Outbox in the same transaction as
your business data, so the message is either stored with it or not at all.

## Further Reading

- [Your First Command](/contents/TutorialFirstCommand.md) — rung 1, if you skipped it
- [RabbitMQ Configuration](/contents/RabbitMQConfiguration.md) — every option on the
  connection, publication and subscription used above
- [Reactor and Proactor](/contents/ReactorAndProactor.md) — what `MessagePumpType.Reactor`
  chose, and when to choose the other one
- [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md) — pumps, performers and
  channels underneath the subscription
- [Dispatching Requests](/contents/DispatchingARequest.md) — `Send`, `Publish` and `Post`
  compared
- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — what `AddBrighter()` and
  `AddConsumers()` accept beyond these defaults
- [Glossary](/contents/Glossary.md) — every term this page linked, and the rest
