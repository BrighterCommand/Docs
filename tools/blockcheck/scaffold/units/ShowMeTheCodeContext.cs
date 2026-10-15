// Types and values ShowMeTheCode.md names in its blocks and never declares.
//
// The page shows handlers from the WebAPI sample's GreetingsApp one method at a time, so the
// fields their constructors set, the sample's requests and its policy names are never shown.
// Each stub carries only the members the page's blocks use; values return a default and do
// nothing.
//
// blockcheck: using static ShowMeTheCodeContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class ShowMeTheCodeContext
{
    // blocks 2, 4, 5: the handler's connection provider, set by its constructor in the sample
    public static IAmARelationalDbConnectionProvider _relationalDbConnectionProvider => null!;
    // block 4: the handler's command processor
    public static IAmACommandProcessor _commandProcessor => null!;
    // block 6: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
}

// blocks 4, 5: `Retry.EXPONENTIAL_RETRYPOLICYASYNC`, the sample's policy names
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

// block 4: `addGreeting.Greeting`, `addGreeting.PersonId`
public class AddGreeting() : Command(Id.Random())
{
    public string Greeting { get; set; } = string.Empty;
    public long PersonId { get; set; }
}

// blocks 4–6: `new GreetingMade(…)`, `@event.Greeting`
public class GreetingMade(string greeting) : Event(Id.Random())
{
    public string Greeting { get; } = greeting;
}
