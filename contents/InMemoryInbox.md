---
description: "The in-process Inbox: when to use it, how to configure it, and its limits."
layout:
  description:
    visible: false
---

# InMemory Inbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)

The in-process Inbox: when to use it, how to configure it, and its limits. It is part of Brighter's [InMemory options for development and testing](/contents/InMemoryOptions.md).


The InMemory Inbox provides message deduplication without requiring a database.

## When to Use the InMemory Inbox

**Perfect for**:

- Unit testing duplicate message handling
- Development without database dependencies

**Production Use Cases** (limited):

- Single-process applications
- Short-lived message deduplication windows
- Non-critical deduplication scenarios

## InMemory Inbox Configuration

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

var bus = new InternalBus();

services.AddConsumers(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.InboxConfiguration = new InboxConfiguration(
        new InMemoryInbox(TimeProvider.System),
        actionOnExists: OnceOnlyAction.Warn
    );
    options.Subscriptions = subscriptions;
    options.DefaultChannelFactory = new InMemoryChannelFactory(bus, TimeProvider.System);
})
.AutoFromAssemblies();
```

In Brighter 10.7.0 this configuration takes effect only in an application that also calls `AddProducers`; see [Global Inbox Configuration in a Consumer-Only Application](/contents/BrighterInboxSupport.md#global-inbox-configuration-in-a-consumer-only-application).

## InMemory Inbox Example Usage

```csharp
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Attributes;

public class PersonCreatedHandler : RequestHandlerAsync<PersonCreated>
{
    private readonly PersonRepository _repository;

    [UseInboxAsync(0, typeof(PersonCreatedHandler), true, onceOnlyAction: OnceOnlyAction.Warn)]
    public override async Task<PersonCreated> HandleAsync(
        PersonCreated @event,
        CancellationToken cancellationToken = default)
    {
        // Inbox ensures this handler processes each message only once
        var person = await _repository.GetByIdAsync(@event.PersonId);
        person.MarkAsCreated();
        await _repository.SaveAsync(person);

        return await base.HandleAsync(@event, cancellationToken);
    }
}
```

The attribute decides what happens to a duplicate for this handler, whatever the configuration says. `onceOnlyAction` defaults to `Throw`, so without it a duplicate raises `OnceOnlyException` even though the configuration above asks for `Warn`. The configuration's `actionOnExists` applies only to handlers that carry no `[UseInboxAsync]` attribute of their own.

## InMemory Inbox Limitations

- **No persistence**: Deduplication state lost on restart
- **Single process**: Cannot deduplicate across instances
- **Memory bound**: Seen message IDs are held in memory. When adding an entry finds `EntryLimit` entries or more (2048 by default), the oldest are removed down to `CompactionPercentage` of the limit (half, by default), at most once per `ExpirationScanInterval`
- **A short deduplication window**: An entry expires `EntryTimeToLive` after it is written (5 minutes by default), and a scan removes it at most once per `ExpirationScanInterval` (10 minutes by default), started when an entry is added or read. A duplicate that arrives after its entry has gone is handled again

## Further Reading

- [InMemory Options for Development and Testing](/contents/InMemoryOptions.md) - The full set, and testing patterns
