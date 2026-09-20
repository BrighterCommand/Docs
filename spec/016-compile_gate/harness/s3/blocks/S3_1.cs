using System.Net.Http;
using Amazon;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Transformers.AWS.V4;
using Paramore.Brighter.Transforms.Storage;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_S3LuggageStore_md_1
{
    public async System.Threading.Tasks.Task Run(Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {

serviceCollection.AddBrighter()
    .UseExternalLuggageStore(provider => new S3LuggageStore(
        new S3LuggageOptions(
            new AWSS3Connection(credentials, RegionEndpoint.EUWest1),
            "brightersamplebucketb0561a06-70ec-11ed-a1eb-0242ac120002")
        {
            BucketRegion = S3Region.EUWest1,
            HttpClientFactory = provider.GetService<IHttpClientFactory>(),
            Strategy = StorageStrategy.CreateIfMissing
        }));
    }
}
