---
description: "Brighter provides ASP.NET Core health checks for the Dispatcher's consumers and the outbox sweeper, each tagged with what to do when it is not Healthy."
layout:
  description:
    visible: false
---

# Health Checks

> **How-to** · Applies to **Brighter V10**

Brighter provides ASP.NET Core health checks for the **Dispatcher**'s consumers and the outbox sweeper, each tagged with what to do when it is not Healthy.

From Brighter 10.8.1, you register them with `AddBrighterConsumerHealthChecks` and `AddBrighterOutboxSweeperHealthChecks`. Both are opt-in. Each check carries one tag that names its remedy: restart the process, take it out of service, or investigate. You route each tag to the right endpoint. For more on the framework itself, see [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks).

## Remedy Kinds

Every Brighter check reports one *remedy kind*, which tells an orchestrator or an operator what to do when the check is not Healthy. The tag on the check names the kind, so an endpoint selects checks by tag:

| Remedy kind | Tag | Constant | Route it to |
| ----------- | --- | -------- | ----------- |
| Restart | `brighter-restart` | `RemedyCheckConvention.RestartTag` | A liveness probe. The process cannot recover on its own |
| Take out of service | `brighter-take-out-of-service` | `RemedyCheckConvention.TakeOutOfServiceTag` | A readiness probe. The process cannot do useful work, but restarting it may not help |
| Investigate | `brighter-investigate` | `RemedyCheckConvention.InvestigateTag` | Monitoring only. Fail neither probe |

The tags carry a `brighter-` prefix so that Brighter's checks never join an endpoint that selects your own `live` or `ready` tags.

Every Brighter check is registered with failure status **Degraded**, so an unexpected exception inside a check reports Degraded rather than Unhealthy. ASP.NET Core returns HTTP 200 for Degraded by default. A probe endpoint that should fail on Degraded must therefore map it to 503, and on a liveness endpoint that means an exception in a check restarts the process.

### Remedy Check Data

Each result carries `Data` in the same shape, whichever check produced it:

- `remedy_kind`: `restart`, `take_out_of_service` or `investigate`
- `conditions`: the names of the conditions present, or an empty array when the check is Healthy
- one entry per condition present, keyed by the condition's name, holding that condition's values

When more than one condition is present, the description joins their descriptions with `"; "`. For example, the sweeper's investigate check reports two tripped topics like this:

```json
{
  "remedy_kind": "investigate",
  "conditions": [
    "tripped_topics"
  ],
  "tripped_topics": {
    "count": 2,
    "topics": [
      "orders",
      "payments"
    ]
  }
}
```

To give your own checks the same shape, use the types in `Paramore.Brighter.Observability`. `RemedyKind` is an enum of the three kinds. `RemedyCondition` is a record struct of `Name`, `Description` and `Values`. `RemedyCheckConvention` holds the tag constants, and `Tag`, `Name`, `Data` and `Describe`, which build the tag, the `remedy_kind` value, the `Data` dictionary and the description from a kind and its conditions.

## Configure Health Checks

`AddBrighterConsumerHealthChecks` comes from the `Paramore.Brighter.ServiceActivator.Extensions.Diagnostics` package, and reads the `IDispatcher` that `AddConsumers` registers. `AddBrighterOutboxSweeperHealthChecks` comes from the `Paramore.Brighter.Outbox.Hosting` package, and reads the sweeper that `UseOutboxSweeper` registers. Register the ones for the parts of Brighter you run, then map one endpoint per remedy kind:

```csharp
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

// ... Web Application Builder code, including AddConsumers and UseOutboxSweeper, goes here

builder.Services.AddHealthChecks()
    .AddBrighterConsumerHealthChecks(channelFailureThreshold: 3)
    .AddBrighterOutboxSweeperHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(RemedyCheckConvention.RestartTag),
    ResultStatusCodes = { [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(RemedyCheckConvention.TakeOutOfServiceTag),
    ResultStatusCodes = { [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable }
});

app.MapHealthChecks("/health/operator", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(RemedyCheckConvention.InvestigateTag),
    ResponseWriter = BrighterHealthReportJsonWriter.WriteAsync
});

app.Run();

public static class BrighterHealthReportJsonWriter
{
    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        using var buffer = new MemoryStream();
        await using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("status", report.Status.ToString());
        writer.WriteStartArray("entries");

        foreach (var (name, entry) in report.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteString("status", entry.Status.ToString());

            writer.WriteStartArray("tags");
            foreach (var tag in entry.Tags)
            {
                writer.WriteStringValue(tag);
            }
            writer.WriteEndArray();

            writer.WriteString("description", entry.Description);

            writer.WriteStartObject("data");
            foreach (var (key, value) in entry.Data)
            {
                writer.WritePropertyName(key);
                JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object));
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        await writer.FlushAsync();
        buffer.Position = 0;
        await buffer.CopyToAsync(context.Response.Body);
    }
}
```

Point your orchestrator's liveness probe at `/health/live` and its readiness probe at `/health/ready`. Both map Degraded to 503. Map Degraded to 503 only on an endpoint that selects a single remedy tag: a Degraded restart check belongs on liveness, and a Degraded investigate check should fail neither probe. Brighter's earlier advice to map Degraded to 503 on readiness now applies to the take-out-of-service tag only.

The `/health/operator` endpoint is for monitoring, not for a probe. It keeps the default status codes, so it returns HTTP 200 while a condition is present, and the default response writer would write only the word `Degraded`. Brighter ships no response writer, and ASP.NET Core has no JSON one, so `BrighterHealthReportJsonWriter` above writes the overall status and, for each check, its name, status, tags, description and `Data`. It buffers the report in a `MemoryStream` and copies it to the response in one write. The community `UIResponseWriter`, from the `AspNetCore.HealthChecks.UI.Client` package, is an alternative if you would rather add a package than keep the writer. A writer is set per endpoint, so to see the restart and take-out-of-service checks' `Data` as well, add another endpoint that selects all three tags and uses the same writer.

With two topics tripped, `/health/operator` returns:

```json
{
  "status": "Degraded",
  "entries": [
    {
      "name": "brighter-outbox-sweeper-investigate",
      "status": "Degraded",
      "tags": [
        "brighter-investigate"
      ],
      "description": "2 topics are circuit-broken and are not being swept until their cooldown ends: orders, payments.",
      "data": {
        "remedy_kind": "investigate",
        "conditions": [
          "tripped_topics"
        ],
        "tripped_topics": {
          "count": 2,
          "topics": [
            "orders",
            "payments"
          ]
        }
      }
    }
  ]
}
```

## Consumer Health Checks

`AddBrighterConsumerHealthChecks` registers two checks:

| Check | Tag | Condition | Status |
| ----- | --- | --------- | ------ |
| `brighter-consumer-take-out-of-service` | `brighter-take-out-of-service` | `persistent_channel_failures` | **Degraded** when any consumer has at least `channelFailureThreshold` consecutive channel failures. Never Unhealthy |
| `brighter-consumer-restart` | `brighter-restart` | `missing_consumers` | **Unhealthy** when consumers are expected and none is running. **Degraded** when some, but not all, are running |

`channelFailureThreshold` defaults to 3 and must be greater than zero; otherwise `AddBrighterConsumerHealthChecks` throws `ArgumentOutOfRangeException` when you register it.

A consumer that keeps failing to read its channel cannot do useful work, so the take-out-of-service check belongs on readiness. Brighter does not recreate a consumer whose message pump has stopped, so missing consumers are a restart condition and belong on liveness.

The restart check reports Healthy when no Dispatcher is registered, when the Dispatcher is not running (not yet started, or `End` has been called), and while `Receive`, `Open` or `SetActivePerformers` is still creating consumers. It compares the totals across all subscriptions, and ignores a subscription you have shut on purpose with `Shut`. After you scale up or reopen a subscription it can report Degraded briefly, so give your liveness probe a period and failure threshold that tolerate that.

### Consumer Health Check Data

| Condition | Values |
| --------- | ------ |
| `persistent_channel_failures` | `threshold`, and `consumers`: one entry per failing consumer, with its `subscription`, `consumer` and `count` |
| `missing_consumers` | `running_total`, `expected_total`, and `subscriptions`: one entry per subscription short of consumers, with its `subscription`, `running` and `expected` |

The restart check's `Data` also carries `consumer_operations_in_flight`, whether or not a condition is present.

### Consumer Health Check Limits

**A hung consumer operation is not detected.** The restart check reports Healthy while a consumer operation is in flight, so if `Receive`, `Open` or `SetActivePerformers` hangs while it creates a consumer, the check stays Healthy. Alert from your monitoring when `consumer_operations_in_flight` in the restart check's `Data` stays above 0.

## Outbox Sweeper Health Checks

`AddBrighterOutboxSweeperHealthChecks` registers two checks:

| Check | Tag | Condition | Degraded when |
| ----- | --- | --------- | ------------- |
| `brighter-outbox-sweeper-restart` | `brighter-restart` | `hung_sweep` | The sweep in progress has run for longer than `hangThreshold` |
| `brighter-outbox-sweeper-investigate` | `brighter-investigate` | `consecutive_failed_sweeps` | At least `failureThreshold` sweeps in a row have failed |
| | | `tripped_topics` | The active sweeper has topics whose [outbox circuit breaker](/contents/SweeperCircuitBreaking.md) has tripped, so their messages are not swept until the cooldown ends |
| | | `tripped_topics_unreadable` | The circuit breaker could not be found, or its `TrippedTopics` could not be read. Brighter logs the exception |

Neither check is ever Unhealthy. If no sweeper is registered, both report Healthy.

Pass the thresholds to change them:

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.Hosting;

builder.Services.AddHealthChecks()
    .AddBrighterOutboxSweeperHealthChecks(hangThreshold: TimeSpan.FromMinutes(2), failureThreshold: 5);
```

`hangThreshold` defaults to three times `TimedOutboxSweeperOptions.TimerInterval`, and `failureThreshold` defaults to 3. Both must be greater than zero; otherwise `AddBrighterOutboxSweeperHealthChecks` throws `ArgumentOutOfRangeException` when you register it.

The tripped-topics conditions read the `IAmAnOutboxCircuitBreaker` registered in your service provider. Register it as a singleton shared with the outbox producer mediator, as [Sweeper Circuit Breaking](/contents/UsingSweeperCircuitBreaking.md) shows, or the check may not match the topics the sweeper skips. The description names at most ten tripped topics, followed by a count of the rest.

### Sizing the Hang Threshold

Set `hangThreshold` above the longest healthy sweep, which you can read from the `sweep.duration` histogram in [Telemetry](/contents/Telemetry.md#outbox-sweeper-metrics), and above `BatchSize` times your broker's send latency. A threshold too close to a slow sweep restarts healthy processes.

**A restart can send messages twice.** If liveness restarts a sweep that was slow rather than hung, messages it had sent but not yet marked as dispatched are sent again by the next sweep. Your consumers must tolerate duplicates, which an [Inbox](/contents/BrighterInboxSupport.md) gives you.

**With the `InMemoryOutbox`, a restart discards every unsent message.** For those deployments, route the restart check to monitoring rather than to liveness, or accept the loss.

### Outbox Sweeper Health Check Data

| Condition | Values |
| --------- | ------ |
| `hung_sweep` | `running_time_seconds`, `hang_threshold_seconds`, `timer_interval_seconds` |
| `consecutive_failed_sweeps` | `count`, `threshold`, `timer_interval_seconds` |
| `tripped_topics` | `count`, `topics` |
| `tripped_topics_unreadable` | `exception_type` |

### Outbox Sweeper Health Check Limits

- **Tripped topics are read only on the active sweeper**, the instance that obtained the [distributed lock](/contents/DistributedLock.md). The tripped-topics conditions are absent on an instance that does not hold the lock, while a sweep waits for it, and after a sweep that lost it to another instance.
- **Tripped topics are one instance's view.** `InMemoryOutboxCircuitBreaker` is per instance.
- **A circuit breaker whose `TrippedTopics` blocks stalls the check.**
- **One sweeper per service provider is supported.** With more than one, what the checks report is undefined.

The [sweeper gauges](/contents/Telemetry.md#outbox-sweeper-metrics), `sweep.running_time` and `tripped_topics`, report the same state as these checks. Use them for alerts, and the checks for probes.

## The Obsolete Dispatcher Health Check

Before Brighter 10.8.1, you registered one check for the Dispatcher, `BrighterServiceActivatorHealthCheck`. From 10.8.1 it is marked `[Obsolete]`, and it is removed in V11:

❌ **Obsolete from Brighter 10.8.1** — compiles with warning `CS0618`, which fails a build that treats warnings as errors

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

builder.Services.AddHealthChecks()
    .AddCheck<BrighterServiceActivatorHealthCheck>("Brighter", HealthStatus.Unhealthy);
```

✅ **Brighter 10.8.1 and later**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

builder.Services.AddHealthChecks()
    .AddBrighterConsumerHealthChecks();
```

The obsolete check still works, and reports these statuses:

| Scenario | Status |
| -------- | ------ |
| All Message Pumps are running, and none has 3 or more consecutive channel failures | Healthy |
| Some Message Pumps are running | Degraded |
| From Brighter 10.8.1: any Message Pump has 3 or more consecutive channel failures | Degraded |
| No Message Pumps are running | Unhealthy |

It reports every consumer condition with one status, and it carries no tag, so an endpoint that selects by tag never selects it. Do not register it together with `AddBrighterConsumerHealthChecks`.

### Migrating to the Remedy Checks

Migrate in this order, so that each existing endpoint keeps what it selected and no probe loses a condition it covered:

1. **Give every existing health endpoint a predicate that excludes the three remedy tags**, as below. An endpoint with no predicate selects every registered check, so once you register the new ones it would select them too, and a hung sweep would turn your existing readiness endpoint into a 503. The predicate excludes tags that no check carries yet, so it is safe to ship first.
2. If you run the outbox sweeper, register its checks with `AddBrighterOutboxSweeperHealthChecks`.
3. Add the liveness, readiness and operator endpoints beside the old ones.
4. Run the old and new endpoints side by side. Your consumer monitoring stays on the old endpoints, which still select the obsolete check. Until step 5 the new readiness endpoint selects no check, so do not point a probe at it yet.
5. **In one change**, replace `BrighterServiceActivatorHealthCheck` with `AddBrighterConsumerHealthChecks`, and move your probes and monitoring to the new endpoints. Add your own checks (a database, for example) to the new predicates by name or tag, or keep an old endpoint for them. The obsolete check is untagged, so the new endpoints never select it: anything still watching an old endpoint for consumer conditions silently stops seeing them. Missing consumers move from readiness to liveness at this step.
6. Before you move to V11, which removes the obsolete check, move any of your own checks onto the new predicates and remove the old endpoints.

The exclusion predicate, which step 1 adds to each existing endpoint:

```csharp
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Paramore.Brighter.Observability;

// ... app is the WebApplication you built

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains(RemedyCheckConvention.RestartTag)
                         && !check.Tags.Contains(RemedyCheckConvention.TakeOutOfServiceTag)
                         && !check.Tags.Contains(RemedyCheckConvention.InvestigateTag)
});
```

## Further Reading

- [Telemetry](/contents/Telemetry.md) — the sweeper gauges and the alerts to set on them
- [Sweeper Circuit Breaking](/contents/SweeperCircuitBreaking.md) — why topics trip, and how they recover
- [How the Dispatcher Works](/contents/HowServiceActivatorWorks.md) — consumers, message pumps and channel failures
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
