// Values DynamoOutbox.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static DynamoOutboxContext;

using Amazon.DynamoDBv2;
using Paramore.Brighter;

public static class DynamoOutboxContext
{
    public static IAmazonDynamoDB dynamoDb => null!;
    public static IAmAProducerRegistry producerRegistry => null!;
    public static IAmazonDynamoDB client => null!;
}
