// The values S3LuggageStore.md names in its blocks and never declares.
//
// The page registers against a `serviceCollection` it leaves to the reader's host, with the
// `credentials` the reader obtains. Each member is typed from a pinned package and returns a
// default, so the call a block makes on it is checked against the real type.
//
// blockcheck: using static S3LuggageStoreContext;

using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;

public static class S3LuggageStoreContext
{
    // block 2: `serviceCollection.AddHttpClient();`
    public static IServiceCollection serviceCollection => null!;

    // block 1: `new AWSS3Connection(credentials, RegionEndpoint.EUWest1)`
    public static AWSCredentials credentials => null!;
}
