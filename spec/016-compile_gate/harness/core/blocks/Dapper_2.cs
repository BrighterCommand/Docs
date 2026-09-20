using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_DapperOutbox_md_2 : Paramore.Brighter.RequestHandlerAsync<AddGreeting>
{
    private readonly Paramore.Brighter.IAmATransactionConnectionProvider _transactionProvider = null;
    private readonly Paramore.Brighter.IAmACommandProcessor _postBox = null;
    private readonly Microsoft.Extensions.Logging.ILogger _logger = null;


public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
{
    var posts = new List<Id>();

    //We use the transaction provider to grab connection and transaction, because Outbox needs
    //to share them 'behind the scenes'
    DbConnection conn = await _transactionProvider.GetConnectionAsync(cancellationToken);
    DbTransaction tx = await _transactionProvider.GetTransactionAsync(cancellationToken);
    try
    {
        var people = await conn.QueryAsync<Person>(
            "select * from Person where name = @name",
            new { name = addGreeting.Name },
            tx);
        var person = people.Single();

        var greeting = new Greeting(addGreeting.Greeting, person);

        //write the added child entity to the Db
        await conn.ExecuteAsync(
            "insert into Greeting (Message, Recipient_Id) values (@Message, @RecipientId)",
            new { greeting.Message, greeting.RecipientId },
            tx);

        //Now write the message we want to send to the Db in the same transaction.
        posts.Add(await _postBox.DepositPostAsync(
            new GreetingMade(greeting.Greet()),
            _transactionProvider,
            cancellationToken: cancellationToken));

        //commit both new greeting and outgoing message
        await _transactionProvider.CommitAsync(cancellationToken);
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Exception thrown handling Add Greeting request");
        //it went wrong, rollback the entity change and the downstream message
        await _transactionProvider.RollbackAsync(cancellationToken);
        return await base.HandleAsync(addGreeting, cancellationToken);
    }
    finally
    {
        _transactionProvider.Close();
    }

    //Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
    //Alternatively, you can let the Sweeper do this, but at the cost of increased latency
    await _postBox.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

    return await base.HandleAsync(addGreeting, cancellationToken);
}
}
