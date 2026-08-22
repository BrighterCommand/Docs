---
description: "This section provides a complete example showing both producer and consumer using transactional messaging with the Outbox and Inbox patterns."
layout:
  description:
    visible: false
---

# Transactional Messaging with the Outbox

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Outbox Support](/contents/BrighterOutboxSupport.md)

This section provides a complete example showing both **producer** and **consumer** using transactional messaging with the Outbox and Inbox patterns. This is the **production-recommended approach** for guaranteed, at-least-once delivery.

## Producer: Using DepositPost with Transactions

The following example shows a handler that writes to the database and sends a message, all within a single transaction:

``` csharp
// ...
public class AddGreetingHandlerAsync : RequestHandlerAsync<AddGreeting>
{
    private readonly ILogger<AddGreetingHandlerAsync> _logger;
    private readonly IAmACommandProcessor _postBox;
    private readonly IAmATransactionConnectionProvider _transactionProvider;

    public AddGreetingHandlerAsync(
        IAmATransactionConnectionProvider transactionProvider,
        IAmACommandProcessor postBox,
        ILogger<AddGreetingHandlerAsync> logger)
    {
        _transactionProvider = transactionProvider;
        _postBox = postBox;
        _logger = logger;
    }

    [RequestLoggingAsync(0, HandlerTiming.Before)]
    [UsePolicyAsync(step: 1, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<AddGreeting> HandleAsync(
        AddGreeting addGreeting,
        CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();

        // The transaction provider (unit of work) gives us a connection and transaction
        // that the Outbox can share 'behind the scenes'.
        var conn = await _transactionProvider.GetConnectionAsync(cancellationToken);
        var tx = await _transactionProvider.GetTransactionAsync(cancellationToken);
        try
        {
            var people = await conn.QueryAsync<Person>(
                "select * from Person where name = @name",
                new { name = addGreeting.Name },
                tx
            );
            var person = people.SingleOrDefault();

            if (person != null)
            {
                var greeting = new Greeting(addGreeting.Greeting, person);

                // Write the entity to the database
                await conn.ExecuteAsync(
                    "insert into Greeting (Message, Recipient_Id) values (@Message, @RecipientId)",
                    new { greeting.Message, greeting.RecipientId },
                    tx);

                // Write the message to the Outbox in the same transaction
                posts.Add(await _postBox.DepositPostAsync(
                    new GreetingMade(greeting.Greet()),
                    _transactionProvider,
                    cancellationToken: cancellationToken));

                // Commit both the entity write and the outgoing message
                await _transactionProvider.CommitAsync(cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown handling Add Greeting request");
            // Rollback both the entity change and the outgoing message
            await _transactionProvider.RollbackAsync(cancellationToken);
            return await base.HandleAsync(addGreeting, cancellationToken);
        }
        finally
        {
            _transactionProvider.Close();
        }

        // Dispatch the message(s) via a transport.
        // Alternatively, let the Sweeper handle this, at the cost of increased latency.
        await _postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(addGreeting, cancellationToken);
    }
}
```

## Consumer: Using Inbox for Deduplication

The following example shows a consumer that receives a message and uses the Inbox pattern to prevent duplicate processing:

``` csharp
// ...
public class GreetingMadeHandler : RequestHandlerAsync<GreetingMade>
{
    private readonly ILogger<GreetingMadeHandler> _logger;
    private readonly IAmACommandProcessor _postBox;
    private readonly IAmATransactionConnectionProvider _transactionConnectionProvider;

    public GreetingMadeHandler(
        IAmATransactionConnectionProvider transactionConnectionProvider,
        IAmACommandProcessor postBox,
        ILogger<GreetingMadeHandler> logger)
    {
        _transactionConnectionProvider = transactionConnectionProvider;
        _postBox = postBox;
        _logger = logger;
    }

    [UseInboxAsync(step: 0, contextKey: typeof(GreetingMadeHandler), onceOnly: true)]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    [UsePolicyAsync(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<GreetingMade> HandleAsync(
        GreetingMade @event,
        CancellationToken cancellationToken = default)
    {
        var posts = new List<Id>();

        var conn = await _transactionConnectionProvider.GetConnectionAsync(cancellationToken);
        var tx = await _transactionConnectionProvider.GetTransactionAsync(cancellationToken);
        try
        {
            var salutation = new Salutation(@event.Greeting);

            // Write to database
            await conn.ExecuteAsync(
                "insert into Salutation (greeting) values (@greeting)",
                new { greeting = salutation.Greeting },
                tx);

            // Write outgoing message in the same transaction
            posts.Add(await _postBox.DepositPostAsync(
                new SalutationReceived(DateTimeOffset.Now),
                _transactionConnectionProvider,
                cancellationToken: cancellationToken));

            // Commit both writes
            await _transactionConnectionProvider.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not save salutation");

            // Rollback both entity write and Outbox write
            await _transactionConnectionProvider.RollbackAsync(cancellationToken);

            return await base.HandleAsync(@event, cancellationToken);
        }
        finally
        {
            _transactionConnectionProvider.Close();
        }

        // Dispatch messages
        await _postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(@event, cancellationToken);
    }
}
```

**Key Points:**
- **UseInboxAsync** attribute ensures the message is only processed once
- **DepositPostAsync** writes to the Outbox within the transaction
- **ClearOutboxAsync** sends the message after the transaction commits
- Both entity writes and message writes succeed or fail together

For a simpler, non-transactional approach suitable for getting started, see [Show me the code!](/contents/ShowMeTheCode.md#using-an-external-bus).

This handler drops a duplicate: the Inbox stops it, and `SalutationReceived` is not sent again. If a duplicate should instead push a stalled flow forward — resending the messages the first run produced, without running the handler again — see [Replay On Seen](/contents/ReplayOnSeen.md).

## Further Reading

- [Outbox Support](/contents/BrighterOutboxSupport.md) - The Outbox pattern, Post, and Deposit and Clear
- [Inbox Support](/contents/BrighterInboxSupport.md) - Deduplicating messages on the consumer
- [Replay On Seen](/contents/ReplayOnSeen.md) - Pushing a stalled flow forward instead of dropping a duplicate
