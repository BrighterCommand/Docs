// Types and values HandlingLargeMessages.md names in its blocks and never shows.
//
// The page registers its store against a `services` collection and an `awsCredentials` it
// leaves to the reader's host, and its mapper maps a `LargeOrderPlaced` it never declares.
// Values return a default and do nothing; a block that calls a member of one is checked
// against the real type.
//
// blockcheck: using static HandlingLargeMessagesContext;

using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

public static class HandlingLargeMessagesContext
{
    // block 1: `services.AddBrighter()`
    public static IServiceCollection services => null!;
    // block 1: `new AWSS3Connection(awsCredentials, RegionEndpoint.EUWest1)`
    public static AWSCredentials awsCredentials => null!;
}

// block 2: `IAmAMessageMapper<LargeOrderPlaced>`, `request.Id`
public class LargeOrderPlaced() : Event(Id.Random());
