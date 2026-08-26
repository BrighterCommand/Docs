---
description: "Brighter's tutorials are a ladder: three rungs, each one a working application, each one adding a single capability to the rung below it."
layout:
  description:
    visible: false
---

# Get Started with Brighter

> **Tutorial** · Applies to **Brighter V10**

Brighter's tutorials are a ladder: three rungs, each one a working application, each one adding
a single capability to the rung below it. You start with a request dispatched to a handler
inside one process, and finish with a message written to a database transaction and delivered
after that transaction commits. Every rung runs, and every rung is code the next one builds on.

## The Brighter Tutorial Ladder

| Rung | What you add | About | Needs Docker |
|---|---|---|---|
| [1. Your First Command](/contents/TutorialFirstCommand.md) | a [command](/contents/Glossary.md#command), a [handler](/contents/Glossary.md#handler), and the [Command Processor](/contents/Glossary.md#command-processor) that connects them | 10 minutes | no |
| [2. Your First Message Over a Broker](/contents/TutorialFirstMessage.md) | a second process, RabbitMQ between the two, and an [event](/contents/Glossary.md#event) that crosses it | 20 minutes | RabbitMQ |
| [3. Adding a Durable Outbox](/contents/TutorialDurableOutbox.md) | Postgres, one transaction covering your data *and* the message, and the [Sweeper](/contents/Glossary.md#sweeper) that dispatches it afterwards | 25 minutes | RabbitMQ and Postgres |

Those times are mostly reading and typing. The machine work — creating projects, restoring
packages, building — was measured on a clean machine with an empty NuGet package cache at
**11 seconds** for rung 1, **23 seconds** for rung 2, and **9.2 seconds** to add rung 3's
packages plus **1.3 seconds** to build. Pulling the Docker images the first time takes longer
and depends on your connection.

The rungs join up like this:

- **Rung 1 stands alone.** One console project, no broker, nothing to install but the SDK.
- **Rung 2 starts a fresh solution** of three projects: a shared class library, a sender and a
  receiver.
- **Rung 3 keeps rung 2's solution** and changes only the sender. If you intend to do rung 3,
  do not delete rung 2's work.

Start at rung 1. If you already know how a request reaches a handler, rung 2 is the first one
with a broker in it — but it assumes rung 1's vocabulary rather than repeating it.

## What You Need Installed for the Ladder

- **The .NET 9 SDK.** Check with `dotnet --version`.
- **Docker Desktop**, for rungs 2 and 3 only. Rung 1 is deliberately in-process.
- **Free ports**: **5672** and **15672** for RabbitMQ on rung 2, and **5432** for Postgres on
  rung 3. If something else is already listening, the container starts and your application
  does not connect.

Every `Paramore.Brighter*` package these pages install is pinned to the same version, so every
`dotnet add package` line you will meet has this shape:

```bash
dotnet add package Paramore.Brighter --version 10.7.0
```

Those pins are checked against NuGet on every pull request and once a day, so the versions
these pages name are versions that still exist. Each rung also repeats what it needs in its own *Before You Start*
section — nothing here has to be remembered.

## Just Want to See Brighter Code?

There are two front doors, and they answer different questions.
[Show me the code!](/contents/ShowMeTheCode.md) is the two-minute look at what Brighter and
Darker code reads like: types, attributes and calls, with nothing to install. The ladder is
for building something that runs. If you are evaluating Brighter, start with *Show me the
code!*; if you have decided to use it, start at rung 1.

## Where to Go After the Ladder

The ladder teaches by building, and it deliberately takes the shortest path through every
choice. When you want the choices themselves:

- **Configure it properly.** [Basic Configuration](/contents/BrighterBasicConfiguration.md)
  covers what `AddBrighter()`, `AddProducers()` and `AddConsumers()` accept beyond the
  defaults these pages used, and each transport has its own reference page —
  [RabbitMQ](/contents/RabbitMQConfiguration.md) is the one rung 2 configured.
- **Understand why it works that way.** [The Task Queue Pattern](/contents/TaskQueuePattern.md)
  and [Outbox Pattern Support](/contents/OutboxPattern.md) explain the two patterns rungs 2
  and 3 built, and [Reactor and Proactor](/contents/ReactorAndProactor.md) explains the
  concurrency model behind the [message pump](/contents/Glossary.md#message-pump).
- **Take it to production.** [Brighter Outbox Support](/contents/BrighterOutboxSupport.md) and
  [Transactional Messaging with the Outbox](/contents/TransactionalMessagingWithTheOutbox.md)
  pick up where rung 3 stops, with an Inbox on the consuming side.
- **Query the other side.** Darker is Brighter's query half —
  [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) is where that story
  starts.

## Further Reading

- [Why Brighter?](/contents/WhyBrighter.md) — the case for the framework, before any code
- [Basic Concepts](/contents/BasicConcepts.md) — commands, events and requests, defined in one
  place
- [Glossary](/contents/Glossary.md) — every term these tutorials link, and the rest
- [FAQ](/contents/FAQ.md) — the questions that come up once the ladder is behind you
