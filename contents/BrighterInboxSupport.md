# Brighter Inbox Support

## Guaranteed, At Least Once

Messaging makes the *guaranteed, at least once* promise.

- Guaranteed: A broker writes a copy of your message to disk, so that it is not lost. An [Outbox](/contents/OutboxPattern.md) writes the message to the application's database to ensure it is not lost.
- At Least Once: In a distributed system you cannot guarantee that a writer will receive a response that it has successfully persisted a message, it must choose to retry the persistence if it does not receive an acknowledgement. This means that duplicates will occur, hence *at least once*.

## Guaranteed, Once Only

There are two possible reactions to the *at least once* problem.

- Idempotency: Ensure that when receiving a message, handling it multiple times will not have side-effects
- De-duplication: Ensure that when receiving a message, you check whether you have handled it before, and discard if you have already processed it.

Events are often idempotent, whilst commands often require de-duplication.

## Inbox

An inbox records messages that you have received and processed. Brighter provides inbox implementations built over a range of databases. If your preferred database is not included, see [implementing an inbox](#implementing-an-inbox). Brighter also makes available an in-memory inbox which is intended for development only as it will not work across multiple consumers, or survive process restarts.

You can configure the usage of an inbox for de-duplication on a per-handler, or per command processor basis.

### Adding an Inbox to a Handler

The inbox is middleware and forms a part of the internal bus pipeline. When added to the pipeline, it will run before your handler, and after subsequent handlers (see the [Russian Doll Model](/contents/BuildingAnAsyncPipeline.md)).

- Before: Check to see if we have already seen this request
- After: Add this request to those that we have already seen.

You also need to configure what action the inbox takes when it has seen a request:

- OnceOnly: Does the inbox reject duplicates, or does it simply record requests. WARNING: Defaults to false - so it won't reject duplicates unless you set this parameter to true, just log requests that pass through this handler.
- OnceOnlyAction: What does the inbox do, when a duplicate is encountered. Defaults to Throw.
    - Warn: Just log that a duplicate was received
    - Throw: Throws a OnceOnlyException	

**If you wish to terminate processing on a duplicate, you should set OnceOnly to true; the OnceOnlyAction will default Throw, which is what you need to terminate processing.**

In the context of the Service Activator (listening to messages over middleware) throwing a OnceOnlyException will result in the message being acked (because it has already been processed).

The inbox is global to your application and uses the request id; you will want to distinguish requests in the inbox if you need to store the same request id for different pipelines. For example, if you deliver an event to multiple handlers, each handler has a request with the same request id. 

Use the **contextKey** parameter to the attribute to disambiguate the request id. We recommend using the type of the handler, as the id usually needs to be unique via pipeline.

There are two versions of the attribute: sync and async. Ensure that you choose the correct version, which should [match your handler](/contents/DispatchingARequest.md#pipelines-must-be-homogeneous).

``` csharp
       [UseInboxAsync(step:0, contextKey: typeof(GreetingMadeHandlerAsync), onceOnly: true )]
        public override async Task<GreetingMade> HandleAsync(GreetingMade @event, CancellationToken cancellationToken = default(CancellationToken))
        {    
            Console.WriteLine($"Greeting Received: {@event.Greeting}");
            
            return await base.HandleAsync(@event, cancellationToken);
        }
```

### Inbox Configuration

Your inbox is configured as part of the Brighter extensions to ServiceCollection. See [Inbox Configuration](/contents/BrighterBasicConfiguration.md#inbox) for more.

## Clearing the Inbox

As of V9, clearing the inbox is deferred to the implementer i.e. Brighter will not do this for you. Typically this involves creating a cron job, or agent, that clears inbox entries that are outside of the window during which they may be resent.

Later versions of Brighter may include data retention policy options that let you configure clearing an inbox.

## Non-Transactional Inbox

As of V9 Brighter's inbox is not transactional, that is it does not participate in the transaction that may write to disk as a result of processing a message. This means that the inbox could fail if your changes to state as a result of processing a request are made, but the inbox is not updated.

Later versions of Brighter may address including the inbox within a transaction, as outbox does today.

## Implementing an Inbox

You can refer to existing inbox implementations if you need to implement an inbox that Brighter does not support.

