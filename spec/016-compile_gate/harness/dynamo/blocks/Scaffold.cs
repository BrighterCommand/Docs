// Page-context types the DynamoOutbox blocks name but do not define.
using System;
using System.Collections.Generic;
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

public static class PageContext
{
    public static IAmAProducerRegistry producerRegistry => null;
    public static Amazon.DynamoDBv2.IAmazonDynamoDB dynamoDb => null;
}
