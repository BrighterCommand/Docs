// Types InMemoryOptions.md names in its blocks and never declares.
//
// The page wires the InMemory components for the reader's own `PersonCreated` event, and a request
// context factory a test substitutes. Each stub carries only the members a block names.

using Paramore.Brighter;

// blocks 1, 2: `typeof(PersonCreated)`, `InMemorySubscription<PersonCreated>`,
// `new PersonCreated { Name = "Alice" }`
public class PersonCreated() : Event(Id.Random())
{
    public string Name { get; set; } = string.Empty;
}

// block 1: `options.RequestContextFactory = new TestRequestContextFactory();`
public class TestRequestContextFactory : InMemoryRequestContextFactory;
