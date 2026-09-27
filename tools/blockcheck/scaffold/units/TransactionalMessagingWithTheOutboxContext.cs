// Types TransactionalMessagingWithTheOutbox.md names in its blocks and never declares.
//
// Both handlers are the Brighter WebAPI_Dapper sample's — block 1 is `GreetingsApp`'s
// `AddGreetingHandlerAsync`, block 2 `SalutationApp`'s `GreetingMadeHandlerAsync` — and the page
// shows none of the requests, entities or policy names they use. Each stub carries only the members
// a block names, typed as the sample types them.

using System;
using Paramore.Brighter;

// blocks 1, 2: `[UsePolicyAsync(…, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]` — the sample's
// `Policies/Retry.cs`
public static class Retry
{
    public const string EXPONENTIAL_RETRYPOLICYASYNC = "GreetingsPorts.Policies.ExponenttialRetryPolicyAsync";
}

// block 1: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
}

// block 1: `conn.QueryAsync<Person>(…)`
public class Person;

// block 1: `new Greeting(addGreeting.Greeting, person)`, `greeting.Message`, `greeting.RecipientId`,
// `greeting.Greet()`
public class Greeting(string message, Person recipient)
{
    public string? Message { get; set; } = message;
    public long RecipientId { get; set; }
    public string Greet() => $"{Message}!";
}

// block 1: `new GreetingMade(greeting.Greet())`; block 2: `RequestHandlerAsync<GreetingMade>`,
// `@event.Greeting`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; set; } = greeting;
}

// block 2: `new Salutation(@event.Greeting)`, `salutation.Greeting`
public class Salutation(string greeting)
{
    public string Greeting { get; set; } = greeting;
}

// block 2: `new SalutationReceived(DateTimeOffset.Now)`
public class SalutationReceived(DateTimeOffset receivedAt) : Event(Id.Random());
