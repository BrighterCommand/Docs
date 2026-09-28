// Values and types the InMemory Inbox and Outbox pages name in their blocks and never declare:
// InMemoryInbox.md and InMemoryOutbox.md, whose handlers share one small Person domain.
//
// Every value member is typed from a pinned package or the BCL, returns a default, and does
// nothing. Each type stub carries only the members a block names. A block that calls a member of
// one of these is checked against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static InMemoryBoxContext;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class InMemoryBoxContext
{
    // InMemoryInbox.md block 1, InMemoryOutbox.md block 1: `services.AddConsumers(…)`, `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
    // InMemoryInbox.md block 1: `options.Subscriptions = subscriptions`
    public static IEnumerable<Subscription> subscriptions => null!;
    // InMemoryOutbox.md block 1: `options.ProducerRegistry = producerRegistry`
    public static IAmAProducerRegistry producerRegistry => null!;
}

// InMemoryOutbox.md block 2: `RequestHandlerAsync<CreatePerson>`, `command.Name`, `command.Email`
public class CreatePerson() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// InMemoryInbox.md block 2: `RequestHandlerAsync<PersonCreated>`, `@event.PersonId`;
// InMemoryOutbox.md block 2: `new PersonCreated { PersonId = person.Id }`
public class PersonCreated() : Event(Id.Random())
{
    public string PersonId { get; set; } = string.Empty;
}

// InMemoryOutbox.md block 2: `new Person(command.Name, command.Email)`, `person.Id`;
// InMemoryInbox.md block 2: `person.MarkAsCreated()`
public class Person(string name, string email)
{
    public string Id { get; } = string.Empty;
    public void MarkAsCreated() { }
}

// InMemoryInbox.md block 2: `_repository.GetByIdAsync(…)`; both pages' block 2: `_repository.SaveAsync(person)`
public class PersonRepository
{
    public Task<Person> GetByIdAsync(string id) => Task.FromResult<Person>(null!);
    public Task SaveAsync(Person person) => Task.CompletedTask;
}
