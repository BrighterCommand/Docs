---
description: "Build a .NET console app that sends a command to its handler through Brighter's Command Processor, in-process, with no broker and no Docker."
layout:
  description:
    visible: false
---

# Your First Command

> **Tutorial** · Applies to **Brighter V10**

Build a .NET console app that sends a [command](/contents/Glossary.md#command) to its
[handler](/contents/Glossary.md#handler) through Brighter's
[Command Processor](/contents/Glossary.md#command-processor), in-process, with no broker and
no Docker.

This is the first rung of the ladder. Everything here runs in one process, so you can see
what Brighter does to a request before any messaging is involved. The rungs above add a
broker, then durability, then streaming — each one a small delta on what you build here.

## What You'll Build: Your First Command

A single console application with three files:

| File | What it is |
|---|---|
| `GreetingCommand.cs` | the request — an instruction to do something, carrying a name |
| `GreetingCommandHandler.cs` | the code that runs when that command is sent |
| `Program.cs` | wiring: register Brighter, resolve the Command Processor, send |

When you run it, the handler prints `Hello Ian` and the program exits.

## Before You Start Your First Command

- **The .NET 9 SDK.** Check with `dotnet --version`.
- **No Docker, and no broker.** Rung 1 is deliberately in-process.
- **About ten minutes**, nearly all of it reading and typing. The machine work — create,
  restore, build, run — measured **11 seconds** on a clean machine with an empty NuGet
  package cache. If your restore takes minutes rather than seconds, the problem is your
  package feed, not this tutorial.

## Step 1: Create the Console Project

```bash
dotnet new console -n HelloWorld -f net9.0
cd HelloWorld
dotnet add package Paramore.Brighter --version 10.7.0
dotnet add package Paramore.Brighter.Extensions.DependencyInjection --version 10.7.0
dotnet add package Microsoft.Extensions.Hosting --version 9.0.0
```

`Paramore.Brighter` carries `Command`, `RequestHandler` and `IAmACommandProcessor`.
`Paramore.Brighter.Extensions.DependencyInjection` carries `AddBrighter`.
`Microsoft.Extensions.Hosting` gives you the generic host that owns the service container.

**Expected result:** `HelloWorld.csproj` now contains these three references, and no others.

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
  <PackageReference Include="Paramore.Brighter" Version="10.7.0" />
  <PackageReference Include="Paramore.Brighter.Extensions.DependencyInjection" Version="10.7.0" />
</ItemGroup>
```

> **This is the one place the page and the sample differ on purpose.** The working sample
> this tutorial is drawn from —
> [`samples/CommandProcessor/HelloWorld`](https://github.com/BrighterCommand/Brighter/tree/master/samples/CommandProcessor/HelloWorld)
> — references Brighter by `ProjectReference` into the source tree, because it is built
> inside that repository. You are installing released packages, so you pin a version. The
> C# below is otherwise the sample's code, unchanged.

## Step 2: Define the Greeting Command

A command is an instruction to do one thing, addressed to one handler. Derive it from
`Command`, which supplies the identity Brighter uses to trace the request through the
pipeline.

```csharp
using Paramore.Brighter;

namespace HelloWorld
{
    public sealed class GreetingCommand(string name) : Command(Id.Random())
    {
        public string Name { get; } = name;
    }
}
```

Put this in `GreetingCommand.cs`.

**Expected result:** the project still builds — `dotnet build` reports `0 Error(s)`.

## Step 3: Write the Greeting Command Handler

A handler is the target of exactly one request type. Deriving from
`RequestHandler<GreetingCommand>` is what tells Brighter this class handles that command;
you do not register it anywhere by hand.

```csharp
using System;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;

namespace HelloWorld
{
    public sealed class GreetingCommandHandler : RequestHandler<GreetingCommand>
    {
        [RequestLogging(step: 1, timing: HandlerTiming.Before)]
        public override GreetingCommand Handle(GreetingCommand greetingCommand)
        {
            Console.WriteLine($"Hello {greetingCommand.Name}");

            return base.Handle(greetingCommand);
        }
    }
}
```

Put this in `GreetingCommandHandler.cs`.

Two details worth naming now, because every rung above reuses them:

- **`return base.Handle(greetingCommand)`** passes the request to the next step in the
  pipeline. Return anything else and you truncate the pipeline.
- **`[RequestLogging(...)]`** is an attribute that inserts a logging step *before* your
  handler runs. That is Brighter's middleware in miniature: attributes on the handler
  method build a pipeline around it. See
  [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md).

**Expected result:** still `0 Error(s)`. Nothing runs yet — nothing has sent the command.

## Step 4: Wire Up Brighter

```csharp
using HelloWorld;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddBrighter().AutoFromAssemblies();
var host = builder.Build();

var commandProcessor = host.Services.GetRequiredService<IAmACommandProcessor>();

commandProcessor.Send(new GreetingCommand("Ian"));
```

Replace the contents of `Program.cs` with this.

`AddBrighter()` registers the Command Processor and its dependencies.
`AutoFromAssemblies()` scans the assemblies loaded in the current application — skipping
`System.*`, `Microsoft.*` and Brighter's own — for classes deriving from `RequestHandler<T>`,
and registers each against the request type in its generic argument. That is how
`GreetingCommandHandler` is found without you naming it anywhere.

`Send` dispatches to exactly one handler and is synchronous: it returns when the handler
has finished. Use `Publish` when you want an [event](/contents/Glossary.md#event) delivered
to every subscriber instead, and `Post` when you want it sent over a broker — see
[Dispatching Requests](/contents/DispatchingARequest.md).

> **The sample ends with `host.Run()` and this page does not.** `host.Run()` blocks until
> the process is signalled, and `AddBrighter()` on its own registers no hosted service, so
> here it would leave you at a prompt that never returns. Keep it when your app has
> something to host — a consumer, a web server — which is what the sample's siblings do.

## Step 5: Run Your First Command

```bash
dotnet run
```

**Expected result** — three log entries, then the greeting, then your shell prompt back:

```text
info: Paramore.Brighter.CommandProcessor[148105941]
      Building send pipeline for command: HelloWorld.GreetingCommand 01a03600-c575-7836-841d-111f5a2e43fa
info: Paramore.Brighter.CommandProcessor[780428052]
      Found 1 pipelines for command: HelloWorld.GreetingCommand 01a03600-c575-7836-841d-111f5a2e43fa
info: Paramore.Brighter.Logging.Handlers.RequestLoggingHandler[600284706]
      Logging handler pipeline call. Pipeline timing Before target, for HelloWorld.GreetingCommand with values of {"name":"Ian","correlationId":null,"id":"01a03600-c575-7836-841d-111f5a2e43fa"} at: 08/24/2026 23:00:15
Hello Ian
```

The identifier and the timestamp differ on every run; everything else is fixed. The process
exits with code `0`.

If you see `Found 0 pipelines`, `AutoFromAssemblies()` did not find your handler — check
that `GreetingCommandHandler` derives from `RequestHandler<GreetingCommand>` and is in the
same assembly as `Program.cs`.

## What Your First Command Showed You

You sent a request and Brighter delivered it to a handler you never registered. Three things
did that work, and all three scale up unchanged:

- **The Command Processor is the only thing you call.** Your code never names a handler.
  Which method you call decides how the request travels — `Send` here, `Post` once there
  is a broker — but the handler is always reached the same way.
- **Handlers are found by their generic argument.** `RequestHandler<GreetingCommand>` is
  both the declaration and the registration.
- **The pipeline is built per request, not once per application.** `Building send pipeline`
  appears on every `Send`, because `Send` constructs the chain — the logging step, then your
  handler — and runs it. Attributes decide what goes into it.

What you have not got yet is durability or another process. Nothing left this application,
so nothing survived it. That is the next rung, which sends the same shape of request over
RabbitMQ to a consumer running separately.

## Further Reading

- [Basic Concepts](/contents/BasicConcepts.md) — commands, events and requests, defined
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) — what
  `[RequestLogging]` is an instance of
- [Basic Configuration](/contents/BrighterBasicConfiguration.md) — what
  `AddBrighter()` accepts beyond the defaults
- [Dispatching Requests](/contents/DispatchingARequest.md) — `Send`, `Publish` and `Post`
  compared
- [Glossary](/contents/Glossary.md) — every term this page linked, and the rest
