# Azure Blob Archive Provider

## Usage
The Azure Blob Archive Provider is a provider for [Outbox Archiver](/contents/BrighterOutboxSupport.md#outbox-archiver).

For this we will need the *Archive* packages for the Azure *Archive Provider*.

* **Paramore.Brighter.Archive.Azure**

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddBrighter(options =>
        { ...  })
        .UseOutboxArchiver(
            new AzureBlobArchiveProvider(new AzureBlobArchiveProviderOptions()
                {
                    BlobContainerUri = "https://brighterarchivertest.blob.core.windows.net/messagearchive",
                    TokenCredential = New AzCliCredential();
                }
            ),
            options => {
                TimerInterval = 5; // Every 5 seconds
                BatchSize  = 500; // 500 messages at a time
                MinimumAge = 744; // 1 month
            }
        );
}

...

```

