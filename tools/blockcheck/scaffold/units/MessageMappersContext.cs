// Types and values MessageMappers.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails. The two events are the
// reader's own, which the page's mappers map; the four values are the fields of the Kafka mapper
// whose `MapToMessage` block 6 shows on its own.
//
// blockcheck: using static MessageMappersContext;

using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Paramore.Brighter;

public static class MessageMappersContext
{
    public static IRequest request => null!;

    // block 6: `topic: Topic`
    public static RoutingKey Topic => null!;

    // block 6: `new JsonSerializer<GreetingEvent>(_schemaRegistryClient, …)`
    public static ISchemaRegistryClient _schemaRegistryClient => null!;

    // block 6: `serializer.Serialize(request, _serializationContext)`
    public static SerializationContext _serializationContext => default;

    // block 6: `header.PartitionKey = _partitionKey`
    public static PartitionKey _partitionKey => default!;
}

// blocks 1, 2, 7: `IAmAMessageMapper<GreetingMade>`, `request.Id`
public class GreetingMade() : Event(Id.Random());

// block 6: `MapToMessage(GreetingEvent request, …)`, `request.Id`
public class GreetingEvent() : Event(Id.Random());
