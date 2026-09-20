using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_DynamoOutbox_md_2 : Paramore.Brighter.RequestHandlerAsync<AddGreeting>
{
    private readonly Paramore.Brighter.DynamoDb.IAmADynamoDbTransactionProvider _transactionProvider = null;
    private readonly Paramore.Brighter.IAmACommandProcessor _postBox = null;
    private readonly Microsoft.Extensions.Logging.ILogger _logger = null;


public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
{
    var posts = new List<Id>();

    //We use the transaction provider to grab connection and transaction, because Outbox needs
    //to share them 'behind the scenes'
    var context = new DynamoDBContext(_transactionProvider.DynamoDb);
    var transaction = await _transactionProvider.GetTransactionAsync(cancellationToken);
    try
    {
        var person = await context.LoadAsync<Person>(addGreeting.Name, cancellationToken);

        person.Greetings.Add(addGreeting.Greeting);

        var document = context.ToDocument(person);
        var attributeValues = document.ToAttributeMap();

        //write the added child entity to the Db - just replace the whole entity as we grabbed the original
        //in production code, an update expression would be faster
        transaction.TransactItems.Add(new TransactWriteItem{Put = new Put{TableName = "People", Item = attributeValues}});

        //Now write the message we want to send to the Db in the same transaction.
        posts.Add(await _postBox.DepositPostAsync(
            new GreetingMade(addGreeting.Greeting),
            _transactionProvider,
            cancellationToken: cancellationToken));

        //commit both new greeting and outgoing message
        await _transactionProvider.CommitAsync(cancellationToken);
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Exception thrown handling Add Greeting request");
        //it went wrong, rollback the entity change and the downstream message
        _transactionProvider.Rollback();
        return await base.HandleAsync(addGreeting, cancellationToken);
    }

    //Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
    //Alternatively, you can let the Sweeper do this, but at the cost of increased latency
    await _postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

    return await base.HandleAsync(addGreeting, cancellationToken);
}
}
