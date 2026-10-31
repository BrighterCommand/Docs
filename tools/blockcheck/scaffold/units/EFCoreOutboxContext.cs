// Types and values EFCoreOutbox.md names in its blocks and never declares.
//
// The page deposits a message beside an Entity Framework write in the reader's own Greetings
// application: a context with a set of people, a person with greetings, and the command and event
// that carry them. Every value member is typed from a pinned package or the BCL and returns a
// default; a stub carries only the members a block names.
//
// blockcheck: using static EFCoreOutboxContext;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;

public static class EFCoreOutboxContext
{
    // blocks 1: `new RelationalDatabaseConfiguration(DbConnectionString())`, `UseMySql(DbConnectionString(), …)`
    public static string DbConnectionString() => "Server=localhost;Database=Greetings;";
}

// blocks 1, 2: `AddDbContext<GreetingsEntityGateway>`, `uow.People`, `uow.SaveChangesAsync(…)`
public class GreetingsEntityGateway(DbContextOptions<GreetingsEntityGateway> options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
}

// block 2: `DbSet<Person>`, `p.Name`, `person.AddGreeting(greeting)`
public class Person
{
    public string Name { get; set; } = string.Empty;
    public void AddGreeting(Greeting greeting) { }
}

// block 2: `new Greeting(addGreeting.Greeting)`, `greeting.Greet()`
public class Greeting(string message)
{
    public string Greet() => $"{message}!";
}

// block 2: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Greeting { get; set; } = string.Empty;
}

// block 2: `new GreetingMade(greeting.Greet())`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}
