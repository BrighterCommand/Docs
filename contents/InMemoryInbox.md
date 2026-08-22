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
// ...
var bus = new InternalBus();

services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddConsumers(options =>
{
    options.Inbox = new InboxConfiguration(
        new InMemoryInbox(TimeProvider.System),
        InboxConfiguration.NoActionOnExists
    );
    options.Subscriptions = subscriptions;
    options.ChannelFactory = new InMemoryChannelFactory(bus);
})
.AutoFromAssemblies();
```

## InMemory Inbox Example Usage

```csharp
// ...
[UseInboxAsync(step: 0, contextKey: typeof(PersonCreatedHandler), onceOnly: true)]
public class PersonCreatedHandler : RequestHandlerAsync<PersonCreated>
{
    private readonly PersonRepository _repository;

    [UseInboxAsync(0, typeof(PersonCreatedHandler), true)]
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

## InMemory Inbox Limitations

- **No persistence**: Deduplication state lost on restart
- **Single process**: Cannot deduplicate across instances
- **Memory bound**: All seen message IDs held in memory
- **No cleanup**: Old entries remain until process restart

## Further Reading

- [InMemory Options for Development and Testing](/contents/InMemoryOptions.md) - The full set, and testing patterns
