// Values DynamoInbox.md names in its blocks and never declares.
//
// Every member is typed from a pinned package, returns a default, and does nothing. A block that
// calls a member of one of these is checked against the real type, so a wrong member or argument
// still fails.
//
// blockcheck: using static DynamoInboxContext;

using Amazon.Runtime;

public static class DynamoInboxContext
{
    // block 1: `new AmazonDynamoDBClient(credentials, …)` — the reader's AWS credentials
    public static AWSCredentials credentials => null!;
}
