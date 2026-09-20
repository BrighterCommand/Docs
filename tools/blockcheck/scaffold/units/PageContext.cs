// Page-context types and values the extracted blocks name but do not define.
// Nothing here comes from a page; it exists so a block can be compiled alone.
//
// THE RULE THIS FILE LIVES UNDER: it supplies IDENTIFIERS, never BEHAVIOUR. A
// prelude may declare that a page's `services` exists and what type it has; it
// may not define a type the page tells the reader to write. Where the line is
// drawn is checked by reading -- AC13 -- against what `--list-scaffold` prints.
//
// Carried forward from spec 015 phase 4's compile harness, which is committed
// at spec/016-compile_gate/harness/core/blocks/Scaffold.cs.
//
// blockcheck: using static PageContext;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public class GreetingMade : Event
{
    public GreetingMade(string greeting) : base(Guid.NewGuid().ToString()) { Greeting = greeting; }
    public string Greeting { get; set; }
}

public class AddGreeting : Command
{
    public AddGreeting() : base(Guid.NewGuid().ToString()) { }
    public string Name { get; set; }
    public string Greeting { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public List<string> Greetings { get; set; } = new List<string>();
}

public class Greeting
{
    public Greeting(string message, Person recipient) { Message = message; RecipientId = recipient.Name; }
    public string Message { get; set; }
    public string RecipientId { get; set; }
    public string Greet() => Message;
}

public class CreateOrderCommand : Command
{
    public CreateOrderCommand() : base(Guid.NewGuid().ToString()) { }
    public string TenantId { get; set; }
    public string OrderId { get; set; }
}

public class OrderCreatedEvent : Event
{
    public OrderCreatedEvent() : base(Guid.NewGuid().ToString()) { }
    public string OrderId { get; set; }
}

public interface IOrderRepository
{
    Task AddAsync(CreateOrderCommand command, CancellationToken cancellationToken);
}

public class MyFeatureSwitchedConfigHandler : RequestHandler<AddGreeting> { }

// Values the pages name in a `// ...` omission: a producer registry, a database
// configuration, a connection string, AWS credentials, a DynamoDb client.
public static class PageContext
{
    public static string DbConnectionString() => "Server=localhost;Database=Greetings;";
    public static string connectionString => DbConnectionString();
    public static IAmAProducerRegistry producerRegistry => null;
    public static RelationalDatabaseConfiguration outboxConfiguration =>
        new RelationalDatabaseConfiguration(DbConnectionString(), outBoxTableName: "Outbox");
    public static IAmAHandlerFactory _handlerFactory => null;
    public static SubscriberRegistry _registry => null;
}

// The sample's own Polly policy registry, named by BrighterBasicConfiguration.md.
public class SalutationPolicy : Polly.Registry.PolicyRegistry { }
