// Types and values ShowMeTheCode.md names in its blocks and never declares.
//
// The page shows handlers from the WebAPI sample's GreetingsApp one method at a time, so the
// fields their constructors set, the sample's requests and its policy names are never shown.
// Each stub carries only the members the page's blocks use; values return a default and do
// nothing.
//
// blockcheck: using static ShowMeTheCodeContext;

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Darker;

public static class ShowMeTheCodeContext
{
    // blocks 2, 4, 5: the handler's connection provider, set by its constructor in the sample
    public static IAmARelationalDbConnectionProvider _relationalDbConnectionProvider => null!;
    // block 4: the handler's command processor
    public static IAmACommandProcessor _commandProcessor => null!;
    // block 6: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// blocks 3–5: `Retry.EXPONENTIAL_RETRYPOLICYASYNC`, the sample's policy names
public static class Retry
{
    public const string EXPONENTIAL_RETRYPOLICYASYNC = "GreetingsApp.Policies.ExponentialRetryPolicyAsync";
}

// block 2: `Policies.Retry.EXPONENTIAL_RETRYPOLICYASYNC`, the same name through the sample's namespace
public static class Policies
{
    public static class Retry
    {
        public const string EXPONENTIAL_RETRYPOLICYASYNC = "GreetingsApp.Policies.ExponentialRetryPolicyAsync";
    }
}

// block 2: `addPerson.Name`
public class AddPerson() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
}

// blocks 1, 4: `new AddGreeting(name, newGreeting.Greeting)`, `addGreeting.Greeting`, `addGreeting.Name`
public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
}

// block 1: `Post(string name, NewGreeting newGreeting)`, the sample's request body
public class NewGreeting
{
    public string Greeting { get; set; } = string.Empty;
}

// block 1: `new FindPersonByName(name)`, whose result's `Person` the controller checks
public class FindPersonByName(string name) : IQuery<FindPersonResult>
{
    public string Name { get; } = name;
}

// block 1: `foundPerson?.Person`
public class FindPersonResult
{
    public Person? Person { get; set; }
}

// blocks 1, 3: `new FindGreetingsForPerson(name)`, `query.Name`
public class FindGreetingsForPerson(string name) : IQuery<FindPersonsGreetings>
{
    public string Name { get; } = name;
}

// blocks 1, 3: the query's result, with `Name` and `Greetings`
public class FindPersonsGreetings
{
    public string? Name { get; set; }
    public IEnumerable<Salutation>? Greetings { get; set; }
}

// block 3: `new Salutation(g.Greet())`
public class Salutation(string words);

// block 3: `p.Id`, `person.Name`, `person.Greetings`
public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public IList<Greeting> Greetings { get; set; } = new List<Greeting>();
}

// block 3: `g.Greet()`
public class Greeting
{
    public string? Message { get; set; }
    public string Greet() => $"{Message}!";
}

// blocks 4–6: `new GreetingMade(…)`, `@event.Greeting`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}
