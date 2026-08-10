# InMemory Outbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [InMemory Options for Development and Testing](/contents/InMemoryOptions.md)

The in-process Outbox: flushing, compaction, configuration and limits. It is part of Brighter's [InMemory options for development and testing](/contents/InMemoryOptions.md).


The InMemory Outbox provides transactional messaging support without requiring a database. Note that if you do not specify a persistent Outbox, we will use the InMemoryOutbox, by default. Any use of the `CommandProcessor`'s `Post` method uses the default `InMemoryOutbox` and not the persistent Outbox, as it does not take a transaction provider as an argument.

## Flush of Expired Messages

The InMemory Outbox will flush expired messages. You can configure the time limit for a message, after which it will be flushed:

- **EntryTimeToLive** Defaults to 5 minutes. Governs how long a message can remain in the Outbox.
- **ExpirationScanInterval** Defaults to 10 mins. Governs how often a scan for expired messages runs.

## Compaction of the InMemoryOutbox

The InMemoryOutbox's capacity is constrained. You can configure the limit to the number of messages the Outbox contains. If you are using the InMemoryOutbox in production scenarios, you should pay attention to this limit. Once the limit is hit, the Outbox will compact, removing older messages first. You can set a compaction percentage, which governs how many messages will be purged from the InMemoryOutbox when we compact.

- **EntryLimit** Defaults to 2048. Governs how many messages the InMemoryOutbox can hold.
- **CompactionPercentage** When we hit a capacity limit, what percentage of messages should we purge. 

## When to Use the InMemory Outbox

**Perfect for**:

- Testing transactional messaging patterns
- Unit testing the Outbox pattern
- Development without database dependencies

**Production Use Cases** (limited):

- Single-process applications
- Non-critical message publishing - the InMemoryOutbox is used in place of a persistent Outbox
- Scenarios where message loss on restart is acceptable

## InMemory Outbox Configuration

```csharp
// ...
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(options =>
{
    options.ProducerRegistry = /* your producer registry */;
    options.Outbox = new InMemoryOutbox();
})
.UseOutboxSweeper();  // Enable sweeper for reliability
```

## InMemory Outbox Example of Post

```csharp
// ...
public class CreatePersonHandler : RequestHandlerAsync<CreatePerson>
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IAmAnOutboxAsync<Message, CommittableTransaction> _outbox;
    private readonly PersonRepository _repository;

    public override async Task<CreatePerson> HandleAsync(
        CreatePerson command,
        CancellationToken cancellationToken = default)
    {
        // Start an in-memory transaction (no real transaction support)
        var person = new Person(command.Name, command.Email);
        await _repository.SaveAsync(person);

        // Deposit message to outbox (held in memory)
        await _commandProcessor.Post(new PersonCreated { PersonId = person.Id }, cancellationToken: cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
```

## InMemory Outbox Limitations

- **No persistence**: Messages lost on application restart
- **No transactions**: Cannot participate in database transactions
- **Single process**: State not shared across instances
- **Memory bound**: All outstanding messages held in memory

## Further Reading

- [InMemory Options for Development and Testing](/contents/InMemoryOptions.md) - The full set, and testing patterns
