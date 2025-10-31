# EF Core Outbox

## Usage
The EFCore Outbox allows integration between EF Core and [Brighter's outbox support](/contents/BrighterOutboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support).

For this we will need the *Outbox* package for EF Core. Packages for EF Core exist for the following RDBMS: MSSQL, MYSQL, Postgres, and Sqlite. Packages have the naming convention:

* **Paramore.Brighter.{DB}.EntityFrameworkCore**

In addition, you will need the Outbox package for the relevant RDBMS:

* **Paramore.Brighter.Outbox.{DB}**

Obviously, {DB} should match. In the example below we use MySql, so we would need the following packages:

* **Paramore.Brighter.MySql.EntityFrameworkCore**
* **Paramore.Brighter.Outbox.MySql**

**Paramore.Brighter.MySql.EntityFrameworkCore** will pull in another package

* **Paramore.Brighter.MySql**

As described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#outbox-support), we configure Brighter to use an outbox with the Use{DB}Outbox method call.

As we want to use EF Core, we also call: Use{DB}TransactionConnectionProvider so that we can share your transaction scope when persisting messages to the outbox.


``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBrighter(...)
        .UseExternalBus(...)
        .UseMySqlOutbox(new MySqlConfiguration(DbConnectionString(), _outBoxTableName), typeof(MySqlConnectionProvider), ServiceLifetime.Singleton)
        .UseMySqTransactionConnectionProvider(typeof(MySqlEntityFrameworkConnectionProvider<GreetingsEntityGateway>), ServiceLifetime.Scoped)
        .UseOutboxSweeper()

        ...
}

```

In our handler we take a dependency on our EF Core Context (derived from Db context). We explicitly start a transaction within the handler, because the Outbox is not within the Db Context we cannot rely on the DBContext's implicit transaction.

We call **DepositPostAsync** within that transaction to write the message to the Outbox. Once the transaction has closed we can call **ClearOutboxAsync** to immediately clear, or we can rely on the Outbox Sweeper, if we have configured one to clear for us. (There are equivalent synchronous versions of these APIs).x

``` csharp
 public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default(CancellationToken))
{
	var posts = new List<Guid>();
	
	//We span a Db outside of EF's control, so start an explicit transactional scope
	var tx = await _uow.Database.BeginTransactionAsync(cancellationToken);
	try
	{
	var person = await _uow.People
		.Where(p => p.Name == addGreeting.Name)
		.SingleAsync(cancellationToken);
	
	var greeting = new Greeting(addGreeting.Greeting);
	
	person.AddGreeting(greeting);
	
	//Now write the message we want to send to the Db in the same transaction.
	posts.Add(await _postBox.DepositPostAsync(new GreetingMade(greeting.Greet()), cancellationToken: cancellationToken));
	
	//write the changed entity to the Db
	await _uow.SaveChangesAsync(cancellationToken);

	//write new person and the associated message to the Db
	await tx.CommitAsync(cancellationToken);
	}
	catch (Exception)
	{
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

