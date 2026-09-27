---
description: "The Azure Blob Archive Provider writes the messages the Outbox Archiver takes from your Outbox into an Azure Blob Storage container."
layout:
  description:
    visible: false
---

# Azure Blob Archive Provider

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Outbox Archiver](/contents/OutboxArchiver.md)

The Azure Blob Archive Provider writes the messages the Outbox Archiver takes from your Outbox into an Azure Blob Storage container. It is an `IAmAnArchiveProvider` you pass to `UseOutboxArchiver<TTransaction>`; the [Outbox Archiver](/contents/OutboxArchiver.md) decides when a message is old enough to archive, and removes it from the Outbox once the provider has written it.

## Azure Blob Archive Provider Configuration

You need three packages:

* **Paramore.Brighter.Archive.Azure** — the provider, in the `Paramore.Brighter.Storage.Azure` namespace
* **Paramore.Brighter.Outbox.Hosting** — `UseOutboxArchiver`, which runs the Archiver as a hosted service
* **Azure.Identity** — a `TokenCredential` for the provider to write with. The provider package does not bring it in

```csharp
using System;
using System.Data.Common;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Storage.Azure;

private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            ConfigureBrighter(services);
        });

private static void ConfigureBrighter(IServiceCollection services)
{
    services.AddBrighter()
        .AddProducers(configure =>
        {
            // ... your producer registry, and the Outbox the Archiver reads from
        })
        // DbTransaction is the transaction type of a relational Outbox; see Outbox Archiver for the others
        .UseOutboxArchiver<DbTransaction>(
            new AzureBlobArchiveProvider(new AzureBlobArchiveProviderOptions(
                blobContainerUri: new Uri("https://brighterarchivertest.blob.core.windows.net/messagearchive"),
                tokenCredential: new AzureCliCredential(),
                accessTier: AccessTier.Cool,
                tagBlobs: true)),
            options =>
            {
                options.TimerInterval = 5;                   // every 5 seconds
                options.ArchiveBatchSize = 500;              // 500 messages at a time
                options.MinimumAge = TimeSpan.FromDays(31);  // dispatched more than a month ago
            });
}
```

`TTransaction` is your Outbox's transaction type, not its transaction provider; [Outbox Archiver](/contents/OutboxArchiver.md) lists the type for each Outbox, and the options the second argument sets.

## Azure Blob Archive Provider Options

`AzureBlobArchiveProviderOptions` takes its first four values as constructor arguments; the rest have defaults. Its properties are `init`-only, so set them, and the two functions, in an object initializer when you create the options.

| Option | Type | Default | Description |
|---|---|---|---|
| `BlobContainerUri` | `Uri` | required | The container the provider writes to. The provider does not create it |
| `TokenCredential` | `TokenCredential` | required | The credential the provider writes with — `AzureCliCredential` locally, a managed identity or `DefaultAzureCredential` in Azure |
| `AccessTier` | `AccessTier` | required | The access tier each blob is written in, such as `Hot`, `Cool` or `Archive` |
| `TagBlobs` | `bool` | required | Whether to write index tags on each blob, from `TagsFunc` |
| `MaxConcurrentUploads` | `int` | `8` | The most transfers one upload runs in parallel |
| `MaxUploadSize` | `int` | `50` | The largest chunk one transfer sends, in megabytes |
| `TagsFunc` | `Func<Message, Dictionary<string, string?>>` | the message's topic, correlation id, message type, timestamp and content type | The tags written when `TagBlobs` is `true` |
| `StorageLocationFunc` | `Func<Message, string>` | the message's Id | The name of the blob a message is written to, within the container |

## What the Azure Blob Archive Provider Writes

Each archived message becomes one blob, named by `StorageLocationFunc` — by default, the message's Id. The blob holds the message **body**; the header reaches the archive only through the tags, and only when `TagBlobs` is `true`. If you need more of the header, write it into the tags with your own `TagsFunc`.

A message whose blob already exists is not written again, so archiving a message twice is harmless.

## Further Reading

- [Outbox Archiver](/contents/OutboxArchiver.md) - When messages are archived, and the `TTransaction` for each Outbox
- [Outbox Support](/contents/BrighterOutboxSupport.md) - The Outbox, the Sweeper and the Archiver
