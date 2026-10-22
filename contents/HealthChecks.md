---
description: "Brighter provides ASP.NET Core health checks that report whether the Dispatcher's consumers are running and reading their channels."
layout:
  description:
    visible: false
---

# Health Checks

> **How-to** · Applies to **Brighter V10**

Brighter provides ASP.NET Core health checks that report whether the **Dispatcher**'s consumers are running and reading their channels.

From Brighter 10.8.1, you register them with `AddBrighterConsumerHealthChecks`. Each check carries a tag that names what to do when it is not Healthy — restart the process, or take it out of service — so you can route each one to the right probe. For more on the framework itself, see [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks).

## Configure Health Checks

`AddBrighterConsumerHealthChecks` comes from the `Paramore.Brighter.ServiceActivator.Extensions.Diagnostics` package. It registers two checks, both of which read the `IDispatcher` that `AddConsumers` registers:

```csharp
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Observability;
using Paramore.Brighter.ServiceActivator.Extensions.Diagnostics.HealthChecks;

// ... Web Application Builder code, including AddConsumers, goes here

builder.Services.AddHealthChecks()
    .AddBrighterConsumerHealthChecks(channelFailureThreshold: 3);

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

app.MapHealthChecks("/health/detail", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var content = new
        {
            Status = report.Status.ToString(),
            Results = report.Entries.ToDictionary(e => e.Key,
                e => new
                {
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = e.Value.Duration
                }),
            TotalDuration = report.TotalDuration
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(content, JsonSerialisationOptions.Options));
    }
});

app.Run();
```

`channelFailureThreshold` defaults to 3 and must be greater than zero; otherwise `AddBrighterConsumerHealthChecks` throws `ArgumentOutOfRangeException` when you register it.

ASP.NET Core returns HTTP 200 for Degraded by default, so a probe endpoint that should fail on Degraded must map it to 503, as `/health/live` and `/health/ready` do above. Point your orchestrator's liveness probe at `/health/live` and its readiness probe at `/health/ready`.

The `/health/detail` endpoint has no predicate, so it selects every check and returns a detailed response, for example:

```json
{
  "status": "Healthy",
  "results": {
    "brighter-consumer-take-out-of-service": {
      "status": "Healthy",
      "description": "The persistent_channel_failures condition is absent: no consumer has 3 consecutive channel failures.",
      "duration": "00:00:00.0001235"
    },
    "brighter-consumer-restart": {
      "status": "Healthy",
      "description": "All 1 expected consumers are running.",
      "duration": "00:00:00.0001712"
    }
  },
  "totalDuration": "00:00:00.0002042"
}
```

### The Obsolete Dispatcher Health Check

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

The obsolete check still works: it reports every consumer condition with one status, and it carries no tag, so an endpoint that selects by tag never selects it. Do not register it together with `AddBrighterConsumerHealthChecks`. Migrate in this order, so that your existing endpoints keep their behaviour:

1. Give every existing health endpoint a `Predicate` that excludes the `brighter-restart`, `brighter-take-out-of-service` and `brighter-investigate` tags. An endpoint with no predicate selects every check, including the new ones.
2. If you also use the outbox sweeper's health checks, register them now with `AddBrighterOutboxSweeperHealthChecks`, from the `Paramore.Brighter.Outbox.Hosting` package.
3. Add the liveness and readiness endpoints, and an operator endpoint if you want one, beside the old ones.
4. Run the old and new endpoints side by side. Your consumer monitoring stays on the old endpoints.
5. In one change, replace `BrighterServiceActivatorHealthCheck` with `AddBrighterConsumerHealthChecks` and move your probes to the new endpoints, adding your own checks (a database, for example) to the new predicates by name or tag. Missing consumers move from readiness to liveness at this step.
6. Before you move to V11, remove any old endpoints that remain.

## Health Status

`AddBrighterConsumerHealthChecks` registers two checks, each with failure status Degraded, so an unexpected exception inside a check reports Degraded rather than Unhealthy:

| Check | Tag | Status |
| ----- | --- | ------ |
| `brighter-consumer-take-out-of-service` | `brighter-take-out-of-service` | **Degraded** when any consumer has at least `channelFailureThreshold` consecutive channel failures. Never Unhealthy |
| `brighter-consumer-restart` | `brighter-restart` | **Unhealthy** when consumers are expected and none is running. **Degraded** when some, but not all, are running |

A consumer that keeps failing to read its channel cannot do useful work, so the take-out-of-service check belongs on readiness. Brighter does not recreate a consumer whose message pump has stopped, so missing consumers are a restart condition and belong on liveness.

The restart check reports Healthy when no Dispatcher is registered, when the Dispatcher is not running (not yet started, or `End` has been called), and while `Receive`, `Open` or `SetActivePerformers` is still creating consumers. It compares the totals across all subscriptions, and ignores a subscription you have shut on purpose with `Shut`. After you scale up or reopen a subscription it can report Degraded briefly, so give your liveness probe a period and failure threshold that tolerate that.

The obsolete `BrighterServiceActivatorHealthCheck` reports these statuses:

| Scenario | Status |
| -------- | ------ |
| All Message Pumps are running, and none has 3 or more consecutive channel failures | Healthy |
| Some Message Pumps are running | Degraded |
| From Brighter 10.8.1: any Message Pump has 3 or more consecutive channel failures | Degraded |
| No Message Pumps are running | Unhealthy |

In the event of a Degraded status, the `/health/detail` endpoint names the subscriptions that are missing consumers, and the consumers that are failing to read their channels.
