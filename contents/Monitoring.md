---
description: "A monitored handler posts an event to a control bus as it is entered and exited, so you can watch what your handlers do from outside the process."
layout:
  description:
    visible: false
---

# Monitoring

> **Reference** · Applies to **Brighter V10**

A monitored handler posts an event to a control bus as it is entered and exited, so you can watch what your handlers do from outside the process.

Each event names the handler, carries the request it handled, and records how long it took. The events are ordinary messages on a topic of your choosing, so anything that can read your broker can consume them. For tracing and metrics through OpenTelemetry instead, see [Telemetry](/contents/Telemetry.md).

## Monitoring Configuration

Monitoring needs two things in your container besides Brighter itself:

- An `IAmAControlBusSender`, which sends each `MonitorEvent` to your broker
- A `MonitorConfiguration`, which turns monitoring on and names this instance in every event

`ControlBusSenderFactory` builds a sender from an Outbox and a producer registry. The registry needs a publication whose `RequestType` is `MonitorEvent`; its `Topic` is where the events go. This example uses the in-memory bus; in an application, use your transport's producer registry factory, such as `RmqProducerRegistryFactory`, with the same publication:

```csharp
using System;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Monitoring.Configuration;
using Paramore.Brighter.Monitoring.Events;
using Paramore.Brighter.Observability;

var monitoringTopic = new RoutingKey("brighter.monitoring");

var producerRegistry = new InMemoryProducerRegistryFactory(
        new InternalBus(),
        [new Publication { Topic = monitoringTopic, RequestType = typeof(MonitorEvent) }],
        InstrumentationOptions.None)
    .Create();

var controlBusSender = new ControlBusSenderFactory().Create<Message, CommittableTransaction>(
    new InMemoryOutbox(TimeProvider.System), producerRegistry, new BrighterTracer());

var services = new ServiceCollection();
services.AddSingleton(controlBusSender);
services.AddSingleton(new MonitorConfiguration
{
    IsMonitoringEnabled = true,
    InstanceName = "OrdersService"
});

services.AddBrighter()
    .AutoFromAssemblies();
```

The sender has its own command processor and Outbox, separate from the ones your application posts through.

You do not register the monitoring handler itself. `AddBrighter()` makes Brighter's own `MonitorHandler<T>` available to the pipeline, whether you register your handlers with `AutoFromAssemblies()` or with `Handlers()`.

## Monitor Attribute Usage

Mark each handler you want monitored with `[Monitor]`, naming the handler's own type:

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Monitoring.Attributes;

public class GreetingCommand() : Command(Id.Random())
{
    public string Name { get; set; } = "";
}

public class GreetingCommandHandler : RequestHandler<GreetingCommand>
{
    [Monitor(step: 1, timing: HandlerTiming.Before, handlerType: typeof(GreetingCommandHandler))]
    public override GreetingCommand Handle(GreetingCommand command)
    {
        Console.WriteLine($"Hello {command.Name}");
        return base.Handle(command);
    }
}
```

The `step` and `timing` place the monitor in the pipeline like any other attribute. With `step: 1`, the time an event records includes every later step in the pipeline, not only your handler. `handlerType` is what the event reports as the handler's name.

`[MonitorAsync]` is the attribute for a `RequestHandlerAsync<T>`; see [Monitoring Limitations](#monitoring-limitations) before you use it.

## Turning Monitoring On and Off

`MonitorConfiguration.IsMonitoringEnabled` is read each time a monitor handler is created. With the default transient handler lifetime that is once per request, so setting it to `false` on the instance you registered stops the events from the next request, without removing any attributes. While it is `false`, the monitor simply passes the request on.

## Monitor Message Format

A monitored request produces two messages, an `EnterHandler` event before the handler runs and an `ExitHandler` event after it. Each is an `MT_EVENT` on the publication's topic, with a JSON body like this one, captured from the example above (its assembly was named `mon`):

```json
{
  "exception": null,
  "eventType": "ExitHandler",
  "eventTime": "2026-09-28T08:25:33.219522Z",
  "timeElapsedMs": 47,
  "handlerName": "GreetingCommandHandler",
  "handlerFullAssemblyName": "GreetingCommandHandler, mon, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
  "instanceName": "OrdersService",
  "requestBody": "{\"name\":\"Ada\",\"correlationId\":null,\"id\":\"01a0e71e-88ed-7acb-a5aa-a5863e075b6c\"}",
  "correlationId": null,
  "id": "01a0e71e-8923-7ae3-82a0-f3654db2fcbf"
}
```

- `timeElapsedMs` is `0` on `EnterHandler`, and the time from entry to exit on `ExitHandler`
- `requestBody` is the request serialized to JSON, as a string
- `instanceName` is `MonitorConfiguration.InstanceName`, which tells apart the instances of a service that share a topic

A consumer on that topic can forward the events to your monitoring tool, for example to Logstash and the ELK stack for dashboards.

## Monitoring Limitations

Two defects in Brighter 10.7.0 limit what monitoring can do:

- **A monitored handler that throws loses its exception.** The monitor tries to send an `ExceptionThrown` event carrying the exception, and serializing an `Exception` fails, so the caller receives a `NotSupportedException` (*"Serialization and deserialization of 'System.Reflection.MethodBase' instances is not supported"*) instead of the exception your handler threw. Monitor only handlers whose exceptions you do not need to see, until this is fixed
- **`[MonitorAsync]` cannot send through the sender `ControlBusSenderFactory` builds.** That sender has no async message mapper for `MonitorEvent`, so an async monitored handler fails with *"No message mapper defined for request"*

## Further Reading

- [Telemetry](/contents/Telemetry.md) - Tracing and metrics through OpenTelemetry
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) - How attributes such as `[Monitor]` place a handler in the pipeline
