# Outbox Support

Brighter supports storing messages that are sent via an External Bus in an Outbox, as per the [Outbox Pattern](/contents/OutboxPattern.md)

This allows you to determine that a change to an entity owned by your application should always result in a message being sent i.e. you have Transactional Messaging.

There are two approaches to using Brighter's Outbox:

* Post: This does not offer Transactional Messaging, but does offer replay
* Deposit and Clear: This approach offers Transactional Messaging.

The **Post** method on the CommandProcessor in Brighter writes first to the **Outbox** and if that succeeds to the Message-Oriented Middleware. If you use Post, then your correctness options are **Ignore/Retry** or **Compensation**. You can use **Post** with **Log Tailing** or **Event Change Capture** but you have to implement those yourself.

The **DepositPost** and **ClearOutbox** methods allow you to use the **Outbox** pattern instead.

## Post

In this approach you choose to **CommandProcessor.Post** a message after your Db transaction writes entity state to the Db. You intend to rely on the *retrying* the call to the broker if it fails. You should make sure that you have setup your **CommandProcessor.RETRYPOLICY** policy with this in mind.

One caveat here is to look at the interaction of the retry on Post and any **UsePolicy** attribute for the handler. If your **CommandProcessor.RETRYPOLICY** policy bubbles up an exception following the last Retry attempt, and your **UsePolicy** attribute for the handler then catches that exception for your handler and forces a Retry, you will end up re-running the database transaction, which may result in duplicate entries. Your **UsePolicy** attribute for the handler needs to explicitly catch the Db errors you wish to retry, and not errors Posting to the message queue in this case.

(As an aside, you should generally write Retry policies to catch specific errors that you know you can retry, not all errors anyway).

In this case, you might also need to consider using a **Fallback** method via the FallbackPolicy attribute to catch **CommandProcessor.Post** exceptions that bubble out and issue a reversing transaction to kill any Db entries made
in error, or raise a log to ensure that there will be manual compensation.

**CommandProcessor.Post** still uses the **Outbox** to store messages you send, but you are not including them in the Db transaction scope, so you have no **guarantees**.

If the failure was on the call to the transport, and not the write to the **Outbox**, you will still have a **Outbox** entry that you can resend via manual compensation later. If the message is posted to the
broker, it **must** have already been written to the **Outbox**.

In you fail to write to the **Outbox**, but have successfully written the entity to the Db, you would need to compensate by reversing the write to the Db in a **Fallback** handler.

## Deposit and Clear

Brighter allows the write to the **Outbox** and the write to the Broker to be separated. This form or Brighter allows you to support Producer-Consumer correctness via the **Outbox Pattern**.

Metaphorically, you can think of this as a post box. You deposit a letter in a post box. Later the postal service clears the post box of letters and delivers them to their recipients.

Within your database transaction you write the message to the Outbox with **CommandProcessor.DepositPost**. This means that if the entity write succeeds, the corresponding write to the **Outbox** will have
taken place. This method returns the Id for that message.

(Note that we use **CommandProcessor.RETRYPOLICY** on the write, but this will only impact the attempt to write within the transaction, not the success or failure of the overall Db transaction, which is under
your control. You can safely ignore Db errors on this policy within this approach for this reason.)

You can then call **CommandProcessor.ClearPostBox** to flush one or more messages from the **Outbox** to the broker. We support multiple messages as your entity write might possibly involve sending multiple downstream messages, which you want to include in the transaction. 

It provides a stronger guarantee than the **CommandProcessor.Post** outside Db transaction with Retry approach as the write to the **Outbox** shares a transaction with the persistence of entity state.


## Bulk Deposit

Starting in v9.2.1 Brighter allows a batch of Messages to be written to the **Outbox**. If your outbox suoports Bulk (This will become a requirement in v10) **CommandProcessor.DepositPost** can be used to deposit a large number of messages in much quicker than individually.

When creating your **CommandProcessor** you can set an outbox bulk chunk size, if the amount of mesages to be deposited into the **Outbox** is greater than this number it will be broken up into chunks of no more than this size.

## Participating in Transactions

Brighter has the functionality to allow the  **Outbox** to participate in the database transactions of your application so that you can ensure that distributed requests will be persisted (or fail to persist) inline with application changes.

To have the Brighter **Outbox** participate in Database transactions your command handler must take a dependency on an  **IAmATransactionConnectionProvider**. The provider will be used when **CommandProcessor.DepositPost** is called and if there is an active transaction the **Outbox** will participate in that transaction. You use the **IAmATransactionConnectionProvider** to create the transaction that you use to interact with the Db; you also use it to grab any connection to the Db that you need.  

Below is an fragment using Dapper

``` csharp
public AddGreetingHandlerAsync(IAmATransactionConnectionProvider transactionProvider, IAmACommandProcessor postBox, ILogger<AddGreetingHandlerAsync> logger)
{
    _transactionProvider = transactionProvider; 
    _postBox = postBox;
    _logger = logger;
}

//...

public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default)
{
    var posts = new List<Guid>();
    
    //We use the unit of work to grab connection and transaction, because Outbox needs
    //to share them 'behind the scenes'

    var conn = await _transactionProvider.GetConnectionAsync(cancellationToken);
    var tx = await _transactionProvider.GetTransactionAsync(cancellationToken);
    try
    {
        var people = await conn.QueryAsync<Person>(
            "select * from Person where name = @name",
            new {name = addGreeting.Name},
            tx
        );
        var person = people.SingleOrDefault();

        if (person != null)
        {
            var greeting = new Greeting(addGreeting.Greeting, person);

            //write the added child entity to the Db
            await conn.ExecuteAsync(
                "insert into Greeting (Message, Recipient_Id) values (@Message, @RecipientId)",
                new { greeting.Message, RecipientId = greeting.RecipientId },
                tx);

            //Now write the message we want to send to the Db in the same transaction.
            posts.Add(await _postBox.DepositPostAsync(
                new GreetingMade(greeting.Greet()),
                _transactionProvider,
                cancellationToken: cancellationToken));

            //commit both new greeting and outgoing message
            await _transactionProvider.CommitAsync(cancellationToken);
        }
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
    await _postBox.ClearOutboxAsync(posts, cancellationToken:cancellationToken);

    return await base.HandleAsync(addGreeting, cancellationToken);
}

```

## Post is Without Transactions

**CommandProcessor.Post** allows you to easily send a message when you are not participating in a transaction with your Db. It is important to note that **CommandProcessor.Post** will never participate in a transaction with your Outbox. 

**CommandPorcessor.Post** uses an in-memory Outbox to hold your messages and then immediately dispatches them; it will support callbacks from the middleware to confirm dispatch, but if your application crashes, your outbox will be lost. 

It is intended for scenarios where you do not write data to your Db but need to send a message, such as when state lives only on in the message, not locally to your app, or where you can accept message loss following an app crash.


## Implicit or Explicit Clearing of Messages from the Outbox

There are two approaches to dispatching messages from Brighter's **Outbox**
  * Implicitly: This relies on a **Sweeper** to dispatch messages out of process
  * Explicitly: This ensures that your message is sent sooner but will processing time to your application code.

### Explicit Clear  
  
To explicitly clear a message you can call **CommandProcessor.ClearOutbox** directly in your handler, after the Db transaction completes. This has the lowest latency. You are responsible for tracking the ids of messages that you wish to send in **CommandProcessor.ClearOutbox**, we do not maintain this state for you. 

Note that you cannnot guarantee that this will succeed, although you can Retry. We use **CommandProcessor.RETRYPOLICY** on the write to the Broker, and you should retry errors writing to the Broker in that policy. However, as the message is now in the **Outbox** you can compensate for eventual failure to write to the Broker by replaying the message from the **Outbox** at a later time.

### Implicit Clear

To implicitly clear messages from your outbox, configure a **Outbox Sweeper** to listen to your **Outbox** and dispatch messages for you.  Once an **Outbox Sweeper** is running you no longer need to call **CommandProcessor.ClearOutbox** however you still have the choice to if you feel a specific message is time sensitive.

The **Outbox Sweeper** is process that monitors an **Outbox** and dispatches messages that have yet to be dispatches. Using **Outbox Sweeper** has a lower latency impact for your application, but because it keeps trying to send the messages until it succeeds is the recommended approach to *Guranteed, At Least Once, Delivery*.

The benefits of using an **Outbox Sweeper** are:
  * If there is a failure dispatch a message after it is committed to the **Outbox** it will be retried until it is dispatches
  * The ability to choose between the implicit and explicit clearing of messages

The **Timed Outbox Sweeper** has the following configurables
  * TimerInterval: The amount of seconds to wait between checks for undispatches messages (default: 5)
  * MinimumMessageAge: The age a message (in miliseconds) that a messages should be before the **OutboxSweeper** should attempt to dispatch it. (default: 5000)
  * BatchSize: The number of messages to attempt to dispatch in each check (default: 100)
  * UseBulk: Use Bulk dispatching of messages on your **Messaging Gateway** (default: false), note: not all **messaging Gateway**s support Bulk dispatching.

It is important to note that the lower the Minimum Message age is the more likely it is that your message will be dispatches more than once (as if you are explicitly clearing messages your application may have instructed the clearing of a message at the same time as the **Outbox Sweeper**)

### Singleton Sweeper

You only want one **Sweeper** running for an Outbox at any time. During development your producer may run the **Sweeper** on a thread, but as you may scale your app out (to provide resilience to failure) you would end up with multiple **Sweepers** as you scaled, which would conflict with each other.

Instead, you should add the Sweeper in it's own app and ensure that you only have one instance of that running at a time by taking a distributed lock in your application to ensure other sweepers do not run. See for example [here](https://www.weave.works/blog/kubernetes-patterns-singleton-application-pattern).

## Outbox Archiver

The **Outbox Archiver** is an out of process services that monitors an **Outbox** and will archive messages of older than a certain age.

The **Timed Outbox Archiver** has the following configurables
  * TimerInterval: The number of seconds to wait between checked for messages eligable for archival (default: 15)
  * BatchSize: The maximum number of messages to archive for each check (default: 100)
  * MinimunAge: The time ellapsed since a message was dispated in hours before it is eligable for archival (default: 24)

### Outbox Configuration

Your outbox is configured as part of the Brighter extensions to ServiceCollection. See [Outbox Configuration](/contents/BrighterBasicConfiguration.md#outbox-support) for more.

### Outbox Builder

Brighter contains DDL to configure your Outbox. For each supported database we include an **OutboxBuilder**. The Inbox Builder **GetDDL** which allows you to obtain the DDL statements required to create an Outbox. You can use this as part of your application start up to configure the Outbox if it does not already exist.

The following example shows creation of a MySql outbox.

We assume that OUTBOX_TABLE_NAME is a constant, shared with the code that configures your inbox.

``` csharp

public static IHost CreateOutbox(this IHost webHost)
{
	using (var scope = webHost.Services.CreateScope())
	{
	var services = scope.ServiceProvider;
	var env = services.GetService<IWebHostEnvironment>();
	var config = services.GetService<IConfiguration>();

	CreateOutbox(config, env);
	}

	return webHost;
}

private static void CreateOutbox(IConfiguration config, IWebHostEnvironment env)
{
	try
	{
	   var connectionString = config.GetConnectionString("Greetings");

	    using var sqlConnection = new MySqlConnection(connectionString);
            sqlConnection.Open();

            using var existsQuery = sqlConnection.CreateCommand();
            existsQuery.CommandText = MySqlOutboxBuilder.GetExistsQuery(OUTBOX_TABLE_NAME);
            bool exists = existsQuery.ExecuteScalar() != null;

            if (exists) return;

            using var command = sqlConnection.CreateCommand();
            command.CommandText = MySqlOutboxBuilder.GetDDL(OUTBOX_TABLE_NAME);
            command.ExecuteScalar();

	}
	catch (System.Exception e)
	{
	Console.WriteLine($"Issue with creating Outbox table, {e.Message}");
	//Rethrow, if we can't create the Outbox, shut down
	throw;
	}
}

```


