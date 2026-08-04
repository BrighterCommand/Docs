# Azure Blob Distributed Lock

> **Reference** · Applies to **Brighter V10**

The Azure Blob locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using **blob leases** in Azure Blob Storage, so a
single [Outbox Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver
run when you scale out. It is a good choice when your workloads already run on Azure.

## Package

* **Paramore.Brighter.Locking.Azure**

The provider takes a lease on a blob inside a container that you must create in advance —
see [Provisioning](#provisioning).

## Configuration

Configure the provider with `AzureBlobLockingProvider`, passing an
`AzureBlobLockingProviderOptions`. The options constructor takes the container URI and a
`TokenCredential`:

```csharp
new AzureBlobLockingProvider(
    new AzureBlobLockingProviderOptions(
        blobContainerUri: new Uri("https://myaccount.blob.core.windows.net/brighter-locks"),
        tokenCredential: new DefaultAzureCredential()));
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `BlobContainerUri` | `Uri` | *(required)* | The URI of the blob container that holds the lock blobs. |
| `TokenCredential` | `TokenCredential` | *(required)* | The Azure credential used to authenticate, for example `DefaultAzureCredential`. |
| `LeaseValidity` | `TimeSpan` | 1 minute | How long the blob lease is held before it expires automatically. Set it longer than a Sweeper/Archiver cycle. |
| `StorageLocationFunc` | `Func<string, string>` | `resource => $"lock-{resource}"` | Maps a resource name to the blob name used for its lock. |

## Example

```csharp
services
    .AddBrighter()
    .AddProducers(opt =>
    {
        opt.Outbox = /* your external Outbox */;
        // ... connection/transaction providers for your Outbox ...

        opt.DistributedLock = new AzureBlobLockingProvider(
            new AzureBlobLockingProviderOptions(
                new Uri("https://myaccount.blob.core.windows.net/brighter-locks"),
                new DefaultAzureCredential())
            {
                LeaseValidity = TimeSpan.FromMinutes(2)
            });
    })
    .UseOutboxSweeper(opt => { opt.BatchSize = 10; });
```

## Provisioning

Create the blob container referenced by `BlobContainerUri` before the provider runs. The
provider creates the per-resource lock blobs (named by `StorageLocationFunc`) inside that
container as needed; it does not create the container itself. Grant the
`TokenCredential` permission to read, write, and lease blobs in the container.

## Notes

- Keep `LeaseValidity` longer than a typical Sweeper or Archiver batch so the lease does
  not expire mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).
- If the holding instance crashes, the blob lease expires after `LeaseValidity` and
  another instance can take over on a later cycle.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
