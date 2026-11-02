// Types MongoDBOutbox.md names in its blocks and never declares.
//
// Block 5's handler adds a greeting to a person document in the reader's own application, and the
// page shows the command, the event and the document type. Each stub carries only the members the
// block names.

using System.Collections.Generic;
using Paramore.Brighter;

// block 5: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
}

// block 5: `new GreetingMade(addGreeting.Greeting)`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}

// block 5: `GetCollection<Person>("People")`, `p.Name`, `Push(p => p.Greetings, …)`
public class Person
{
    public string Name { get; set; } = string.Empty;
    public List<string> Greetings { get; set; } = new();
}
