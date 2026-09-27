// Values and types DynamoOutbox.md names in its blocks and never declares.
//
// Block 2 is the Brighter WebAPI_Dynamo sample's `AddGreetingHandlerAsync`, and the page shows
// none of the requests or entities it uses; each stub carries only the members a block names,
// typed as the sample types them. Every value member is typed from a pinned package or the BCL,
// returns a default, and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static DynamoOutboxContext;

using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Paramore.Brighter;

public static class DynamoOutboxContext
{
    public static IAmazonDynamoDB dynamoDb => null!;
    public static IAmAProducerRegistry producerRegistry => null!;
    public static IAmazonDynamoDB client => null!;
}

// block 2: `RequestHandlerAsync<AddGreeting>`, `addGreeting.Name`, `addGreeting.Greeting`
public class AddGreeting(string name, string greeting) : Command(Id.Random())
{
    public string Name { get; } = name;
    public string Greeting { get; } = greeting;
}

// block 2: `context.LoadAsync<Person>(…)`, `person.Greetings.Add(…)`
public class Person
{
    public List<string> Greetings { get; set; } = new List<string>();
}

// block 2: `new GreetingMade(addGreeting.Greeting)`
public class GreetingMade(string greeting) : Event(Id.Random());
