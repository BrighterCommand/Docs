---
description: "The Azure Blob locking provider implements Brighter's distributed lock using blob leases in Azure Blob Storage, so a single Outbox Sweeper and Archiver run when you scale out."
layout:
  description:
    visible: false
---

# Azure Blob Distributed Lock

> **Reference** · Applies to **Brighter V10**

The Azure Blob locking provider implements Brighter's [distributed
lock](/contents/DistributedLock.md) using **blob leases** in Azure Blob Storage, so a
single [Outbox Sweeper](/contents/BrighterOutboxSupport.md#implicit-clear) and Archiver
run when you scale out. It is a good choice when your workloads already run on Azure.

## Azure Blob Distributed Lock Package

* **Paramore.Brighter.Locking.Azure**

The provider takes a lease on a blob inside a container that you must create in advance —
see [Provisioning](#azure-blob-distributed-lock-provisioning).

## Azure Blob Distributed Lock Configuration

Configure the provider with `AzureBlobLockingProvider`, passing an
`AzureBlobLockingProviderOptions`. The options constructor takes the container URI and a
`TokenCredential`:

```csharp
new AzureBlobLockingProvider(
    new AzureBlobLockingProviderOptions(
        blobContainerUri: new Uri("https://myaccount.blob.core.windows.net/brighter-locks"),
        tokenCredential: new DefaultAzureCredential()));
```

<!-- optioncheck: Paramore.Brighter.Locking.Azure.AzureBlobLockingProviderOptions
     manual: BlobContainerUri — set from a required constructor parameter, so an instance reads back that argument rather than a default
     manual: TokenCredential — set from a required constructor parameter, so an instance reads back that argument rather than a default
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `BlobContainerUri` | `Uri` | `none` | Names the blob container that holds the lock blobs. |
| `TokenCredential` | `TokenCredential` | `none` | Authenticates to the container, for example with `DefaultAzureCredential`. |
| `LeaseValidity` | `TimeSpan` | `60000 ms` | How long the blob lease is held before it expires automatically. |

All three properties are `init`-only, so set them in the constructor call or an object
initialiser. Set `LeaseValidity` longer than a Sweeper or Archiver cycle.

**`StorageLocationFunc` is a public field rather than a property**, which is why it is not in
the table above: it is a `Func<string, string>` mapping a resource name to the blob name that
holds its lock, and it defaults to `resource => $"lock-{resource}"`. Assign it in an object
initialiser like any other member.

## Azure Blob Distributed Lock Example

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

## Azure Blob Distributed Lock Provisioning

Create the blob container referenced by `BlobContainerUri` before the provider runs. The
provider creates the per-resource lock blobs (named by `StorageLocationFunc`) inside that
container as needed; it does not create the container itself. Grant the
`TokenCredential` permission to read, write, and lease blobs in the container.

## Azure Blob Distributed Lock Notes

- Keep `LeaseValidity` longer than a typical Sweeper or Archiver batch so the lease does
  not expire mid-run. See [Lease Expiry vs Manual
  Release](/contents/DistributedLock.md#lease-expiry-vs-manual-release).
- If the holding instance crashes, the blob lease expires after `LeaseValidity` and
  another instance can take over on a later cycle.

## Further Reading

- [Distributed Lock](/contents/DistributedLock.md) — concepts and the full provider list
- [Outbox Support](/contents/BrighterOutboxSupport.md) — the Sweeper and Archiver
