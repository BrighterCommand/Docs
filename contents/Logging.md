---
description: "Brighter writes its diagnostics through Microsoft.Extensions.Logging, and takes its ILoggerFactory from a single static holder rather than from your container."
layout:
  description:
    visible: false
---

# Logging

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)

Brighter writes its diagnostics through `Microsoft.Extensions.Logging`, and takes its
`ILoggerFactory` from a single static holder rather than from your container.

That one design decision explains everything else on this page, including why Brighter can
appear to log nothing at all.

## How Brighter Gets Its Logger

Every Brighter type that logs holds its logger in a static field, built once from
`Paramore.Brighter.Logging.ApplicationLogging`:

```csharp
using Microsoft.Extensions.Logging;
using Paramore.Brighter.Logging;

public class CommandProcessor
{
    private static readonly ILogger s_logger = ApplicationLogging.CreateLogger<CommandProcessor>();
    // ...
}
```

`ApplicationLogging` is a static holder with a settable factory:

```csharp
using Microsoft.Extensions.Logging;

namespace Paramore.Brighter.Logging;

public static class ApplicationLogging
{
    public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();
    public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
}
```

**The default is a `LoggerFactory` with no providers**, which accepts every call and writes
nothing. Brighter is never silent because logging is switched off; it is silent because
nothing has been given somewhere to write to.

Because `CreateLogger<T>()` is called with the type, the **category name is the full type
name** — `Paramore.Brighter.CommandProcessor`,
`Paramore.Brighter.ServiceActivator.Dispatcher`. Filtering on the `Paramore.Brighter` prefix
therefore catches all of it.

## Logging with Dependency Injection

If you use the DI extensions, there is nothing to do. Both `AddBrighter` and `AddConsumers`
resolve `ILoggerFactory` from the container as they build, and hand it to
`ApplicationLogging`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// The generic host registers ILoggerFactory, so Brighter finds one.
builder.Services.AddBrighter()
    .AutoFromAssemblies([typeof(GreetingCommand).Assembly]);

var host = builder.Build();
```

The resolution is `GetService<ILoggerFactory>()`, not `GetRequiredService`, so a container
with no factory registered is not an error — Brighter keeps the empty default and writes
nothing.

## Logging Without Dependency Injection

If you build a Command Processor by hand, assign the factory yourself:

```csharp
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging;

ApplicationLogging.LoggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var commandProcessor = CommandProcessorBuilder.StartNew()
    // ... handler configuration, policies, request context factory
    .Build();
```

**Assign it before you build anything.** Each type caches its logger in a `static readonly`
field the first time that type is used, so a factory installed afterwards is never seen by
the types that have already initialised.

## What Brighter Logs

Log methods are source-generated with `[LoggerMessage]`, so the messages are structured
rather than interpolated:

```csharp
using Microsoft.Extensions.Logging;

private static partial class Log
{
    [LoggerMessage(LogLevel.Information, "Building send pipeline for command: {CommandType} {Id}")]
    public static partial void BuildingSendPipelineForCommand(ILogger logger, Type commandType, string id);
}
```

`{CommandType}` and `{Id}` reach a structured sink as named fields, so you can query on them.

Across the Brighter source the levels divide roughly as:

| Level | Used for |
|---|---|
| `Trace` | the finest pipeline detail |
| `Debug` | pipeline construction and message pump steps |
| `Information` | dispatch, handler counts, connection lifecycle |
| `Warning` | recoverable faults — a missing mapper, a retried delivery |
| `Error` | a failure Brighter could not handle for you |

`Information` is a reasonable production level for Brighter's categories; `Debug` is verbose,
because a pipeline logs as it is built for every request.

## Logging Common Pitfalls

- **Nothing appears.** No provider is registered, or the factory was assigned after a
  Brighter type had already initialised its static logger. Check the order.
- **Logs stop when you move off DI.** The DI extensions were doing the assignment for you;
  the manual path does not.
- **Setting `ApplicationLogging.LoggerFactory` in a test affects the whole process.** It is
  static and shared, so set it once in fixture setup rather than per test.
- **Logging is not tracing.** For distributed traces across message boundaries, use
  [Telemetry](/contents/Telemetry.md); Brighter's logs are not a substitute for spans.

## Further Reading

- [Telemetry](/contents/Telemetry.md)
- [Configuring OpenTelemetry](/contents/ConfiguringOpenTelemetry.md)
- [Monitoring](/contents/Monitoring.md)
- [Basic Configuration](/contents/BrighterBasicConfiguration.md)
