# DynamoDb Outbox

> **Reference** · Applies to **Brighter V10**

## DynamoDb Outbox Usage
The DynamoDb Outbox allows integration between DynamoDb and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support).

To support transactional messaging when using DynamoDb requires us to use DynamoDb's support for ACID transactions. You should understand best practices for using transactions with DynamoDb.

For this we will need the *Outbox* package for DynamoDb:

**AWS SDK v3** (legacy support):
* **Paramore.Brighter.Outbox.DynamoDB**

**AWS SDK v4** (recommended for new projects):
* **Paramore.Brighter.Outbox.DynamoDB.V4**

**Paramore.Brighter.Outbox.DynamoDb** (or **.V4**) will pull in another package:

* **Paramore.Brighter.DynamoDb** (or **Paramore.Brighter.DynamoDb.V4**)

See [AWS SQS Migration](/contents/AWSSQSMigrateToV10.md#migrating-from-aws-sdk-v3-to-v4) for migration guidance between v3 and v4.

As described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support), we configure Brighter to use an outbox with the Use{DB}Outbox method call.

As we want to use DynamoDb with the outbox, we also call: Use{DB}TransactionConnectionProvider so that we can share your transaction scope when persisting messages to the outbox.


``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .AddProducers(...)
        .UseDynamoDbOutbox(ServiceLifetime.Singleton)
        .UseDynamoDbTransactionConnectionProvider(typeof(DynamoDbUnitOfWork), ServiceLifetime.Scoped)
        .UseOutboxSweeper()

        ...
}

```

In our handler we take a dependency on Brighter's **IAmABoxTransactionConnectionProvider** interface and convert it to a **DynamoDbUnitofWork**. We explicitly start a transaction within the handler on the Database within the Unit of Work.  

We call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed we can call **ClearOutboxAsync** to immediately clear, or we can rely on the Outbox Sweeper, if we have configured one to clear for us. (There are equivalent synchronous versions of these APIs).

> **Running more than one instance?** Configure a [distributed lock](/contents/DistributedLock.md) so only one Sweeper (and Archiver) runs at a time — see [DynamoDB Distributed Lock](/contents/DynamoDbDistributedLock.md).

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

## Replay Support: The Causation Index

[Replay On Seen](/contents/ReplayOnSeen.md) resends every Outbox message produced under a given Causation Id. On DynamoDB that means querying on a non-key attribute, which needs a Global Secondary Index — by default one named **Causation**, with `CausationId` as its hash key.

This applies to both `Paramore.Brighter.Outbox.DynamoDB` and `.V4`. The DynamoDB **Inbox** needs nothing: it looks a Causation Id up by the table's own primary key.

### A new table gets the index for free

`MessageItem.CausationId` is decorated with `[DynamoDBGlobalSecondaryIndexHashKey(indexName: "Causation")]`, and `DynamoDbTableFactory` reflects over those attributes when it builds the request. So a table you create through the factory already has the index — you only add a throughput entry for it:

```csharp
var createTableRequest = new DynamoDbTableFactory().GenerateCreateTableRequest<MessageItem>(
    new DynamoDbCreateProvisionedThroughput(
        new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
        new Dictionary<string, ProvisionedThroughput?>
        {
            ["Outstanding"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["OutstandingAllTopics"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["Delivered"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["DeliveredAllTopics"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["Causation"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },  // replay
        }));

var builder = new DynamoDbTableBuilder(client);
await builder.Build(createTableRequest);
await builder.EnsureTablesReady([createTableRequest.TableName], TableStatus.ACTIVE);
```

### Adding the index to an existing table

A table provisioned before replay shipped does not have the index, and there is no migration runner for DynamoDB. Add it yourself with `UpdateTable`:

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

var request = new UpdateTableRequest
{
    TableName = "brighter_outbox",

    // The index's key attribute has to be declared, even though the table already stores it
    AttributeDefinitions =
    [
        new AttributeDefinition
        {
            AttributeName = "CausationId",
            AttributeType = ScalarAttributeType.S
        }
    ],

    GlobalSecondaryIndexUpdates =
    [
        new GlobalSecondaryIndexUpdate
        {
            Create = new CreateGlobalSecondaryIndexAction
            {
                IndexName = "Causation",
                KeySchema =
                [
                    new KeySchemaElement { AttributeName = "CausationId", KeyType = KeyType.HASH }
                ],

                // Replay only reads MessageId, which is the base table's hash key and is
                // therefore always present in a KEYS_ONLY index
                Projection = new Projection { ProjectionType = ProjectionType.KEYS_ONLY },

                // Omit this on a PAY_PER_REQUEST table — it applies to PROVISIONED billing only
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 10,
                    WriteCapacityUnits = 10
                }
            }
        }
    ]
};

await client.UpdateTableAsync(request);
```

Two operational notes:

- **The index backfills asynchronously.** DynamoDB populates a new GSI in the background, and the index reports `CREATING` until it finishes. Until then a replay may find only part of a causation's messages, so wait for the index to become `ACTIVE` before you rely on it.
- **Restart your hosts afterwards.** The Outbox probes for the index once, with `DescribeTable`, and caches the answer for the life of the store instance. An Outbox built before the index existed keeps reporting that replay is unsupported until the process restarts.

### The index name

`DynamoDbConfiguration.CausationIndexName` defaults to `"Causation"`. **Leave it there.**

It has a public setter, unlike the name suggests it should be treated — but it does not behave like the `Outstanding` and `Delivered` index names, which you can rename freely. The Causation index name is also declared on `MessageItem` as an attribute argument, and attribute arguments must be compile-time constants, so the annotation cannot read your configured value. `DynamoDbTableFactory` therefore always generates an index called `Causation`, whatever you set here.

Point `CausationIndexName` at another name and the probe and the replay query both target a GSI the table model never declares. Nothing errors: `SupportsCausationTracking()` simply reports `false`, and replay silently finds no messages. If you have a genuine reason to rename it, you must create the index under that name yourself.

### If you skip the index

Nothing breaks, and replay never happens:

- **Deposits are unaffected.** The `CausationId` attribute is still written; with no index to populate it is simply an ordinary attribute.
- **Startup warns.** `SupportsCausationTracking()` reports `false`, and [pipeline validation](/contents/PipelineValidation.md) raises a *warning* — not an error — for any handler configured with `OnceOnlyAction.Replay`.
- **Duplicates are skipped quietly.** `ReplayCausation` returns `false` rather than throwing, so a duplicate does not fail the consumer pipeline with a DynamoDB `ValidationException`. Nothing is resent.

See [When Replay Does Not Fire](/contents/ReplayOnSeenReference.md#when-replay-does-not-fire) for how to tell this apart from the other reasons a replay produces nothing.
