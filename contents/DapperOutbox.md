# Dapper Outbox

## Usage
The Dapper Outbox allows integration between Dapper and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support).

For this we will need the *Outbox* package for Dapper. Packages for Dapper exist for the following RDBMS: MSSQL, MYSQL, and Sqlite. Packages have the naming convention:

* **Paramore.Brighter.{DB}.Dapper**

In addition, you will need the Outbox package for the relevant RDBMS:

* **Paramore.Brighter.Outbox.{DB}**

Obviously, {DB} should match. In the example below we use MySql, so we would need the following packages:

* **Paramore.Brighter.MySql.Dapper**
* **Paramore.Brighter.Outbox.MySql**

**Paramore.Brighter.MySql.Dapper** will pull in another two packages:

* **Paramore.Brighter.MySql**
* **Paramore.Brighter.Dapper**

As described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support), we configure Brighter to use an outbox with the Use{DB}Outbox method call.

As we want to use Dapper, we also call: Use{DB}TransactionConnectionProvider so that we can share your transaction scope when persisting messages to the outbox.


``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(...)
        .UseMySqlOutbox(new MySqlConfiguration(DbConnectionString(), _outBoxTableName), typeof(MySqlConnectionProvider), ServiceLifetime.Singleton)
        .UseMySqTransactionConnectionProvider(typeof(Paramore.Brighter.MySql.Dapper.UnitOfWork), ServiceLifetime.Scoped)
        .UseOutboxSweeper()

        ...
}

```

In our handler we take a dependency on Brighter's Dapper Unit of Work. We explicitly start a transaction within the handler on the Database within the Unit of Work. Dapper provides extension methods on a DbConnection for typical CRUD operations. Our Unit of Work wraps that DbConnection, and allows you to create a DB transaction associated with that DbConnection. You must use our method, and not create the transaction directly via the connection, because we cannot obtain that transaction. Sharing that transaction allows us to insert a message into the Outbox within the same transaction. 

We call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed we can call **ClearOutboxAsync** to immediately clear, or we can rely on the Outbox Sweeper, if we have configured one to clear for us. (There are equivalent synchronous versions of these APIs).x

``` csharp
  public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default(CancellationToken))
{
	var posts = new List<Guid>();
	
	//We use the unit of work to grab connection and transaction, because Outbox needs
	//to share them 'behind the scenes'
	
	var tx = await _uow.BeginOrGetTransactionAsync(cancellationToken);
	try
	{
	var searchbyName = Predicates.Field<Person>(p => p.Name, Operator.Eq, addGreeting.Name);
	var people = await _uow.Database.GetListAsync<Person>(searchbyName, transaction: tx);
	var person = people.Single();
	
	var greeting = new Greeting(addGreeting.Greeting, person);
	
	//write the added child entity to the Db
	await _uow.Database.InsertAsync<Greeting>(greeting, tx);

	//Now write the message we want to send to the Db in the same transaction.
	posts.Add(await _postBox.DepositPostAsync(new GreetingMade(greeting.Greet()), cancellationToken: cancellationToken));
	
	//commit both new greeting and outgoing message
	await tx.CommitAsync(cancellationToken);
	}
	catch (Exception e)
	{   
	_logger.LogError(e, "Exception thrown handling Add Greeting request");
	//it went wrong, rollback the entity change and the downstream message
	await tx.RollbackAsync(cancellationToken);
	return await base.HandleAsync(addGreeting, cancellationToken);
	}

	//Send this message via a transport. We need the ids to send just the messages here, not all outstanding ones.
	//Alternatively, you can let the Sweeper do this, but at the cost of increased latency
	await _postBox.ClearOutboxAsync(posts, cancellationToken:cancellationToken);

	return await base.HandleAsync(addGreeting, cancellationToken);
}
```

## Brighter Unit of Work without Dapper

Because the Brighter Unit of Work just wraps a DbConnection and is's associated transaction, it can be used to provide a DbTransaction that works with the outbox whenever you want to use DbConnection to interface with a database. Whilst Dapper adds value on top of DbConnection, it just a set of extension methods, and our unit of work does not depend upon Dapper itself.


