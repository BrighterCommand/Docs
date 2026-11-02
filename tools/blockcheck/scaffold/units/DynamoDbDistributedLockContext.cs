// Values DynamoDbDistributedLock.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static DynamoDbDistributedLockContext;

using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

public static class DynamoDbDistributedLockContext
{
    // block 1: `new DynamoDbLockingProvider(dynamoDb, …)`, a client the block's reader has built
    public static IAmazonDynamoDB dynamoDb => null!;
    // block 2: `services.AddSingleton<IAmazonDynamoDB>(dynamoDb)`
    public static IServiceCollection services => null!;
}
