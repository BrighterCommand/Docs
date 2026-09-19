---
description: "The Dapper Outbox allows integration between Dapper and Brighter's outbox support."
layout:
  description:
    visible: false
---

# Dapper Outbox

> **Reference** · Applies to **Brighter V10**

## Dapper Outbox Usage
The Dapper Outbox allows integration between Dapper and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support).

Dapper needs no Brighter package of its own at V10. You need the *Outbox* package for your RDBMS and the package that carries its connection and transaction providers:

* **Paramore.Brighter.Outbox.{DB}**
* **Paramore.Brighter.{DB}**

Obviously, {DB} should match. In the example below we use MySql, so we would need the following packages, alongside **Dapper** itself:

* **Paramore.Brighter.Outbox.MySql**
* **Paramore.Brighter.MySql**

> **Coming from V9?** The **Paramore.Brighter.{DB}.Dapper** packages stopped at 9.9.13 and there is no V10 release of any of them. Their Unit of Work is now **IAmATransactionConnectionProvider**, which lives in the **Paramore.Brighter.{DB}** package and hands you the same **DbConnection** and **DbTransaction** Dapper's extension methods take.

As described in [Command Processor Configuration Reference](/contents/CommandProcessorConfigurationReference.md#outbox-support), we configure Brighter to use an outbox by setting **Outbox** on the options passed to **AddProducers()**.

As we want to use Dapper, we also set **ConnectionProvider** and **TransactionProvider** so that we can share your transaction scope when persisting messages to the outbox.


``` csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox.MySql;
using Paramore.Brighter.MySql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MySql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    var configuration = new RelationalDatabaseConfiguration(
        connectionString,
        databaseName: "brighter_test",
        outBoxTableName: "outbox_messages",
        inboxTableName: "inbox_messages");

    services.AddConsumers(options =>
        {
            options.InboxConfiguration = new InboxConfiguration(new MySqlInbox(configuration));
        })
        .AddProducers(configure =>
        {
            configure.Outbox = new MySqlOutbox(configuration);
            configure.ConnectionProvider = typeof(MySqlConnectionProvider);
            configure.TransactionProvider = typeof(MySqlTransactionProvider);
        })
        .UseOutboxSweeper()
        .AutoFromAssemblies();
}

```

In our handler we take a dependency on Brighter's **IAmATransactionConnectionProvider**. We explicitly start a transaction within the handler on the Database within the provider. Dapper provides extension methods on a DbConnection for typical CRUD operations. Our provider wraps that DbConnection, and allows you to create a DB transaction associated with that DbConnection. You must use our method, and not create the transaction directly via the connection, because we cannot obtain that transaction. Sharing that transaction allows us to insert a message into the Outbox within the same transaction.

We call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed we can call **ClearOutboxAsync** to immediately clear, or we can rely on the Outbox Sweeper, if we have configured one to clear for us. (There are equivalent synchronous versions of these APIs).

``` csharp
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;

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
```

## Brighter Unit of Work without Dapper

Because Brighter's transaction provider just wraps a DbConnection and its associated transaction, it can be used to provide a DbTransaction that works with the outbox whenever you want to use DbConnection to interface with a database. Whilst Dapper adds value on top of DbConnection, it is just a set of extension methods, and our transaction provider does not depend upon Dapper itself.


