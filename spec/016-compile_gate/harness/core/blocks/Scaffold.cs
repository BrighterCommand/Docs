// Page-context types and values the extracted blocks name but do not define.
// Nothing here comes from a page; it exists so a block can be compiled alone.
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
