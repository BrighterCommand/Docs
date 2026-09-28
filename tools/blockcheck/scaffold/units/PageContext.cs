// Values and types DapperOutbox.md names in its blocks and never declares.
//
// Block 1 passes a connection string the page elides as one the reader already has. Block 2 is the
// Brighter WebAPI_Dapper sample's `AddGreetingHandlerAsync`, and the page shows none of the
// requests or entities it uses. Each stub carries only the members a block names, typed as the
// sample types them.
//
// blockcheck: using static PageContext;

using Paramore.Brighter;

public static class PageContext
{
    // DapperOutbox.md block 1: `connectionString,`
    public static string connectionString => "Server=localhost;Database=Greetings;";
}

// block 2: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
}

// block 2: `conn.QueryAsync<Person>(…)`
public class Person;

// block 2: `new Greeting(addGreeting.Greeting, person)`, `greeting.Message`, `greeting.RecipientId`,
// `greeting.Greet()`
public class Greeting(string message, Person recipient)
{
    public string? Message { get; set; } = message;
    public long RecipientId { get; set; }
    public string Greet() => $"{Message}!";
}

// block 2: `new GreetingMade(greeting.Greet())`
public class GreetingMade(string greeting) : Event(Id.Random());
