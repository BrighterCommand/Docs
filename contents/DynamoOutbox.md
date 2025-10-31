# DynamoDb Outbox

## Usage
The DynamoDb Outbox allows integration between DynamoDb and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support).

To support transactional messaging when using DynamoDb requires us to use DynamoDb's support for ACID transactions. You should understand best practices for using transactions with DynamoDb.

For this we will need the *Outbox* package for DynamoDb:

* **Paramore.Brighter.Outbox.DynamoDB**

**Paramore.Brighter.Outbox.DynamoDb** will pull in another package:

* **Paramore.Brighter.DynamoDb**

As described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support), we configure Brighter to use an outbox with the Use{DB}Outbox method call.

As we want to use DynamoDb with the outbox, we also call: Use{DB}TransactionConnectionProvider so that we can share your transaction scope when persisting messages to the outbox.


``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(...)
        .UseDynamoDbOutbox(ServiceLifetime.Singleton)
        .UseDynamoDbTransactionConnectionProvider(typeof(DynamoDbUnitOfWork), ServiceLifetime.Scoped)
        .UseOutboxSweeper()

        ...
}

```

In our handler we take a dependency on Brighter's **IAmABoxTransactionConnectionProvider** interface and convert it to a **DynamoDbUnitofWork**. We explicitly start a transaction within the handler on the Database within the Unit of Work.  

We call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed we can call **ClearOutboxAsync** to immediately clear, or we can rely on the Outbox Sweeper, if we have configured one to clear for us. (There are equivalent synchronous versions of these APIs).x

``` csharp
public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default(CancellationToken))
{
	var posts = new List<Guid>();

	//We use the unit of work to grab connection and transaction, because Outbox needs
	//to share them 'behind the scenes'
	var context = new DynamoDBContext(_unitOfWork.DynamoDb);
	var transaction = _unitOfWork.BeginOrGetTransaction();
	try
	{
		var person = await context.LoadAsync<Person>(addGreeting.Name);

		person.Greetings.Add(addGreeting.Greeting);

		var document = context.ToDocument(person);
		var attributeValues = document.ToAttributeMap();

		//write the added child entity to the Db - just replace the whole entity as we grabbed the original
		//in production code, an update expression would be faster
		transaction.TransactItems.Add(new TransactWriteItem{Put = new Put{TableName = "People", Item = attributeValues}});

		//Now write the message we want to send to the Db in the same transaction.
		posts.Add(await _postBox.DepositPostAsync(new GreetingMade(addGreeting.Greeting), cancellationToken: cancellationToken));

		//commit both new greeting and outgoing message
		await _unitOfWork.CommitAsync(cancellationToken);
	}
	catch (Exception e)
	{   
		_logger.LogError(e, "Exception thrown handling Add Greeting request");
		//it went wrong, rollback the entity change and the downstream message
		_unitOfWork.Rollback();
		return await base.HandleAsync(addGreeting, cancellationToken);
	}

	//Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
	//Alternatively, you can let the Sweeper do this, but at the cost of increased latency
	await _postBox.ClearOutboxAsync(posts, cancellationToken:cancellationToken);

	return await base.HandleAsync(addGreeting, cancellationToken);
}
```
