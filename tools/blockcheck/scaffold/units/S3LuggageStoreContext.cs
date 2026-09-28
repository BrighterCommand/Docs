// The value S3LuggageStore.md names in its blocks and never declares.
//
// The page registers against a `serviceCollection` it leaves to the reader's host. The member is
// typed from a pinned package and returns a default, so the call a block makes on it is checked
// against the real type.
//
// blockcheck: using static S3LuggageStoreContext;

using Microsoft.Extensions.DependencyInjection;

public static class S3LuggageStoreContext
{
    // block 2: `serviceCollection.AddHttpClient();`
    public static IServiceCollection serviceCollection => null!;
}
