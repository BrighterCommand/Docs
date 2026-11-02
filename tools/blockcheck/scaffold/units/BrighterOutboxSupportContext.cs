// Types BrighterOutboxSupport.md names in its blocks and never declares.
//
// Block 1 is a handler in the reader's own Greetings application, and the page names the command it
// handles, the entities it reads and writes, and the event it deposits, which are the reader's own.
// Each stub carries only the members the block names.

using Paramore.Brighter;

// block 1: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
}

// block 1: `conn.QueryAsync<Person>(…)`, `new Greeting(addGreeting.Greeting, person)`
public class Person;

// block 1: `greeting.Message`, `greeting.RecipientId`, `greeting.Greet()`
public class Greeting(string message, Person recipient)
{
    public string Message { get; } = message;
    public int RecipientId { get; } = 0;
    public string Greet() => Message;
}

// block 1: `new GreetingMade(greeting.Greet())`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}
