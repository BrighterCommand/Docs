# Dispatching Requests

Once you have [implemented your Request Handler](ImplementingAHandler.html), you will want to dispatch **Commands** or **Events** to that Handler.

## Usage

In the following example code we register a handler, create a *Command Processor*, and then use that *Command Processor* to dispatch a request to the handler.


``` csharp
  public class Program
    {
        private static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.AddBrighter().AutoFromAssemblies();
                })
                .UseConsoleLifetime()
                .Build();

            var commandProcessor = host.Services.GetService<IAmACommandProcessor>();

            commandProcessor.Send(new GreetingCommand("Ian"));

            host.WaitForShutdown();
        }
   }
```

## Registering a Handler

In order for the *Command Processor* to find a Handler for your **Command** or **Event** you need to register the association between that **Command** or **Event** and your Handler.

Brighter's **HostBuilder** support provides **AutoFromAssemblies** to register any *Request Handlers* in the project. See [Basic Configuration](/contents/BrighterBasicConfiguration.md) for more. If you are not using **HostBuilder** and or **ServiceCollection** you will need to register your handlers yourself. See [How Configuring the Command Processor Works](/contents/HowConfiguringTheCommandProcessorWorks.md). 

### Taking a Dependency on a Command Processor

#### Producers

Typically, a producer is an ASP.NET WebAPI or MVC app. In this case you take a dependency in your *Controller* on the **IAmACommandProcessor** interface, which is satisfied via *ServiceCollection*.

If you intend to dispatch messages to another app, via message oriented middleware, your Brighter configuration will need a **Publication** which identifies how to to do that.

#### Consumer

An *Internal Bus* consumer is just a handler, typically registered through Brighter's ServiceCollection integration via our HostBuilder extension. It can thus take dependencies on other registered services within your app.

An *External Bus* consumer is just a handler, but typically you host it using Brighter's *Service Activator*. You configure *Service Activator* to listens to messages flowing over message oriented middleware through a **Subscription**. *Service Activator* takes care of listening to messages arriving via the middleware, and delivering them to your handler code. In this way the complexity of using middleware is abstracted away from you, and you can focus on the business logic in your handler that you want to run in response to a message.

### Pipelines Must be Homogeneous

Brighter only supports pipelines that are solely **IHandleRequestsAsync** or **IHandleRequests**. In particular, note that middleware (attributes on your handler) must be of the same type as the rest of your pipeline. A common mistake is to **UsePolicy** when you mean **UsePolicyAsync**.

## Dispatching Requests

Once you have registered your Handlers, you can dispatch requests to them. 

### Internal Bus: Send & Publish

When using an *Internal Bus*, the *Command Processor* has two options for dispatching messages:

* **Send**: Used with a **Command**, send expects one, and only one, receiver.
* **Publish**: used with an **Event**, publish expects zero or more receivers.

All methods have versions that support async...await.

#### Internal Bus: Sending a Command

A **Command** is an instruction to do work. We only expect one recipient to do the work, and side-effects mean that we want to ensure that only one receiver actions it as it typically mutates state.

To send a **Command** you simply use **CommandProcessor.Send()** 

``` csharp
commandProcessor.Send(new GreetingCommand("Ian"));
```

NOTE: On a call to **CommandProcessor.Send()** the execution path flows to the handler. The Internal Bus is not buffered.

#### Internal Bus: Returning results of a Command to the caller.

Brighter follows Command-Query separation, and a Command does not have return value. So **CommandDispatcher.Send()** does not return anything. Please see a discussion on how to handle this in [Returning Results from a Handler](/contents/ReturningResultsFromAHandler.md). Also note that **Darker** provides our support for a **Query** over an Internal Bus.

#### Internal Bus: Publishing an Event

An **Event** is a fact, often the results of work that has been done. It is not atypical to raise an event to indicate the results of a **Command** having been actioned.

``` csharp
commandProcessor.Publish(new GreetingEvent("Ian has been greeted"));
```

NOTE: On a call to **CommandProcessor.Publish()** the execution path flows to all handlers in a loop. The Internal Bus is not buffered. 

### External Bus: Post, Deposit and Clear

When using an [External Bus](/contents/ImplementingExternalBus.md) the *Command Processor* has two options for dispatching a message:

* **DepositPost** and **ClearOutbox**: This is a two-step approach to dispatching a message via middleware. It allows you to include the **DepositPost** call that puts the message in your [Outbox](/contents/BrighterOutboxSupport.md) within a database transation, so that you can achieve transactional messaging (either the message is placed in the Outbox and the change is made to any entities, or nothing is written to either).
* **Post**: This is a one-step approach to dispatching a message via middleware. Use it if you do not need transactional messaging, as described above. 

All methods have versions that support async...await.

In both cases, if you use an Outbox with external storage, the message will be eventually delivered if it is written to the Outbox, provided that you run an *Outbox Sweeper* to dispatch any messages in the Outbox that have not been marked as dispatched.

In this example we use **CommandProcessor.Post()** to dispatch a message over middleware.

``` csharp
commandProcessor.Post(new GreetingCommand("Ian"));
```

In this exaple, we use **CommandProcessor.DepositPost()** and **CommandProcessor.ClearOutbox** to raise a transactional message. We then immediately clear it to lower latency. (We could have relied on an Outbox Sweeper and you should have an Outbox Sweeper in case this was to fail).

In this example we are using Dapper as the library for writing our entities to the Db, and have used Brighter's Unit of Work support for that (passed into the handler constructor).

```csharp
public override async Task<AddGreeting> HandleAsync(AddGreeting addGreeting, CancellationToken cancellationToken = default(CancellationToken))
{
    var posts = new List<Guid>();
    
    //We use the unit of work to grab connection and transaction, because Outbox needs
    //to share them 'behind the scenes'

    var conn = await _uow.GetConnectionAsync(cancellationToken);
    await conn.OpenAsync(cancellationToken);
    var tx = _uow.GetTransaction();
    try
    {
        var searchbyName = Predicates.Field<Person>(p => p.Name, Operator.Eq, addGreeting.Name);
        var people = await conn.GetListAsync<Person>(searchbyName, transaction: tx);
        var person = people.Single();
        
        var greeting = new Greeting(addGreeting.Greeting, person);
        
        //write the added child entity to the Db
        await conn.InsertAsync<Greeting>(greeting, tx);

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

#### Message Mapper, MT_COMMAND and MT_EVENT

When sending a message, the [Message Mapper](/contents/MessageMappers.md) is invoked to map your request to a Brighter **Message** which can be sent over message oriented middleware.

Given you may have both a **Command** and an **Event** how do we preserve that behavior (a command expects one handler, an event zero or more) in listening applications?

By setting the **Message.MessageType** to **MT_COMMAND** or **MT_EVENT** you indicate whether you expect this message to be treated as a **Command** or an **Event**. We flow that information in the message headers when sending over middleware.

When *Service Activator* listens to messages it expects that the **MessageType** matches the type of **IRequest**, either **Command** or **Event** that your message mapper code transforms the message into. It will then use **CommandProcessor.Send()** to dispatch messages to a single handler, or **CommandProcessor.Publish** to dispatch messages to zero or more handlers, as appropriate.
