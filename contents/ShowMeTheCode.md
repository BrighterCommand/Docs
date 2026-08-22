---
description: "There is an old principle: show don't tell, and this introduction is about showing you what you can do with Brighter and Darker."
layout:
  description:
    visible: false
---

# Show me the code!

> **How-to** · Applies to **Brighter V10 and Darker V4**

There is an old principle: show don't tell, and this introduction is about showing you what you can do with Brighter and Darker. It's not about how - more detailed documentation elsewhere shows you how to write this code. It's not about why - articles elsewhere discuss some of the reasons behind this approach. It is just, let me see how Brighter works. 

## Brighter and Darker

### Brighter is about Requests

A *Request* is a message sent over a bus. A request may update state.

A *Command* is an instruction to execute some behavior. An *Event* is a notification.

You use the *Command Processor* to separate the sender from the receiver, and to provide middleware functionality like a retry.

### Darker is about Queries

A *Query* is a message executed via a bus that returns a *Result*. A query does not update state.

You use the *Query Processor* to separate the requester from the replier, and to provide middleware functionality like a retry.

### Middleware

Both Brighter and Darker allow you to provide middleware that runs between a request or query being made and being handled. The middleware used by a handler is configured by attributes.

### Sending and Querying Example

In this example, we show sending a command, and querying for the results of issuing it, from within an ASP.NET WebAPI controller method.

``` csharp
[Route("{name}/new")]
[HttpPost]
public async Task<ActionResult<FindPersonsGreetings>> Post(string name, NewGreeting newGreeting)
{
	await _commandProcessor.SendAsync(new AddGreeting(name, newGreeting.Greeting));

	var personsGreetings = await _queryProcessor.ExecuteAsync(new FindGreetingsForPerson(name));

	if (personsGreetings == null) return new NotFoundResult();

	return Ok(personsGreetings);
}
```

### Handling Examples

Handler code listens for and responds to requests or queries. The handler for the above request and query are:

``` csharp
[RequestLoggingAsync(0, HandlerTiming.Before)]
[UsePolicyAsync(step:1, policy: Policies.Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
public override async Task<AddPerson> HandleAsync(AddPerson addPerson, CancellationToken cancellationToken = default)
{
	await using var connection = await _relationalDbConnectionProvider.GetConnectionAsync(cancellationToken);
	await connection.ExecuteAsync("insert into Person (Name) values (@Name)", new {Name = addPerson.Name});
	return await base.HandleAsync(addPerson, cancellationToken);
}
```

``` csharp
[QueryLogging(0)]
[RetryableQuery(1, Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
public override async Task<FindPersonsGreetings> ExecuteAsync(FindGreetingsForPerson query, CancellationToken cancellationToken = new CancellationToken())
{
	//Retrieving parent and child is a bit tricky with Dapper. From raw SQL We wget back a set that has a row-per-child. We need to turn that
	//into one entity per parent, with a collection of children. To do that we bring everything back into memory, group by parent id and collate all
	//the children for that group.

	var sql = @"select p.Id, p.Name, g.Id, g.Message 
		from Person p
		inner join Greeting g on g.Recipient_Id = p.Id";
	await using var connection = await _relationalDbConnectionProvider.GetConnectionAsync(cancellationToken);
	var people = await connection.QueryAsync<Person, Greeting, Person>(sql, (person, greeting) =>
	{
		person.Greetings.Add(greeting);

		return person;
	}, splitOn: "Id");
	
	if (!people.Any())
	{
		return new FindPersonsGreetings(){Name = query.Name, Greetings = Array.Empty<Salutation>()};
	}

	var peopleGreetings = people.GroupBy(p => p.Id).Select(grp =>
	{
		var groupedPerson = grp.First();
		groupedPerson.Greetings = grp.Select(p => p.Greetings.Single()).ToList();
		return groupedPerson;
	});

	var person = peopleGreetings.Single();

	return new FindPersonsGreetings
	{
		Name = person.Name, Greetings = person.Greetings.Select(g => new Salutation(g.Greet()))
	};
}

}
```

## Using an External Bus

As well as using an Internal Bus, in Brighter you can use an External Bus - middleware such as RabbitMQ or Kafka - to send a request between processes. Brighter supports both sending a request, and provides a *Dispatcher* that can listen for requests on middleware and forward it to a handler.

> **📝 Note: Start Simple, Add Complexity Later**
> The examples below show the **simplest way** to get started with Brighter's external bus. They use the **InMemory Outbox** for development and testing, which requires no additional infrastructure setup.
>
> **⚠️ InMemory Outbox Limitations:**

> - Not durable - crashes lose messages
> - Not suitable for production
> - No distributed transactions
>
> For **production scenarios**, see:

> - [Outbox Pattern](/contents/BrighterOutboxSupport.md) - Transactional messaging with database-backed Outbox
> - [WebAPI Sample](https://github.com/BrighterCommand/Brighter/tree/master/samples/WebAPI) - Fully-featured production example
> - [InMemory Options](/contents/InMemoryOptions.md) - When to use InMemory components

### Sending a Message to Another Process

The following code shows the simplest way to send a message to another process using Brighter's external bus:

``` csharp
[RequestLoggingAsync(0, HandlerTiming.Before)]
[UsePolicyAsync(step: 1, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
public override async Task<AddGreeting> HandleAsync(
    AddGreeting addGreeting,
    CancellationToken cancellationToken = default)
{
    await using var connection = await _relationalDbConnectionProvider.GetConnectionAsync(cancellationToken);

    // Write to database
    await connection.ExecuteAsync(
        "insert into Greeting (Message, Recipient_Id) values (@Message, @RecipientId)",
        new { Message = addGreeting.Greeting, RecipientId = addGreeting.PersonId });

    // Send message to external bus
    await _commandProcessor.PostAsync(
        new GreetingMade(addGreeting.Greeting),
        cancellationToken: cancellationToken);

    return await base.HandleAsync(addGreeting, cancellationToken);
}
```

**What's happening here:**

- The handler writes to the database
- `PostAsync()` sends a message via the configured transport (RabbitMQ, Kafka, etc.)
- The message is written to the `InMemoryOutbox`, then immediately dispatched
- **No explicit message mapper needed** - uses default JSON mappers automatically
- **No transactions** - This is the simplest approach for getting started

### Receiving a Message from Another Process

The following code receives a message sent from another process via a Dispatcher:

``` csharp
[RequestLoggingAsync(0, HandlerTiming.Before)]
[UsePolicyAsync(step: 1, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
public override async Task<GreetingMade> HandleAsync(
    GreetingMade @event,
    CancellationToken cancellationToken = default)
{
    await using var connection = await _relationalDbConnectionProvider.GetConnectionAsync(cancellationToken);

    // Process the received message
    await connection.ExecuteAsync(
        "insert into Salutation (greeting) values (@greeting)",
        new { greeting = @event.Greeting });

    return await base.HandleAsync(@event, cancellationToken);
}
```

**What's happening here:**
- The Dispatcher receives the message from the transport
- Routes it to this handler based on message type
- The handler processes the message and writes to the database
- **No explicit message mapper needed** - V10 automatically deserializes JSON messages

### Configuration Example

Here's how to configure Brighter with an InMemory Outbox and a transport:

``` csharp
services.AddBrighter(options =>
{
    // Configure handlers
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.MapperLifetime = ServiceLifetime.Singleton;
    options.CommandProcessorLifetime = ServiceLifetime.Scoped;
})
.UseInMemoryOutbox() // Simple outbox for development
.AutoFromAssemblies(); // Auto-discover handlers

services.AddProducers(options =>
{
    // Configure your transport (RabbitMQ, Kafka, AWS, etc.)
    options.UseRabbitMQ(new RabbitMqConfiguration
    {
        AmqpUri = new Uri("amqp://guest:guest@localhost:5672")
    })
    .Publication<GreetingMade>(publication =>
    {
        publication.Topic = new RoutingKey("greeting.made");
    });
});
```

### Next Steps

Once you're comfortable with these basics:

1. **Add Transactional Messaging** - Use [DepositPost and ClearOutbox](/contents/BrighterOutboxSupport.md) for guaranteed message delivery
2. **Add Deduplication** - Use [Inbox Pattern](/contents/BrighterInboxSupport.md) to handle duplicate messages
3. **Production Outbox** - Replace InMemory with [database-backed Outbox](/contents/BrighterOutboxSupport.md)
4. **Explore Samples** - See the [WebAPI Sample](https://github.com/BrighterCommand/Brighter/tree/master/samples/WebAPI) for production patterns

