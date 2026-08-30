---
description: "The Firestore Inbox records the messages a consumer has already handled in a Google Cloud Firestore collection, so a redelivery is recognised rather than reprocessed."
layout:
  description:
    visible: false
---

# Firestore Inbox

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Inbox Support](/contents/BrighterInboxSupport.md)

The Firestore Inbox records the messages a consumer has already handled in a Google Cloud Firestore collection, so a redelivery is recognised rather than reprocessed. It shares its configuration type with the [Firestore Outbox](/contents/FirestoreOutbox.md).

## Firestore Inbox Configuration

```powershell
dotnet add package Paramore.Brighter.Inbox.Firestore
```

The Inbox package pulls in **Paramore.Brighter.Firestore**, which carries
`FirestoreConfiguration` and the connection provider.

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Firestore;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.Firestore;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

public static class FirestoreInboxRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var configuration = new FirestoreConfiguration(
            projectId: "my-gcp-project",
            database: "(default)")
        {
            Inbox = new FirestoreCollection
            {
                Name = "Inbox",
                Ttl = TimeSpan.FromDays(7)
            }
        };

        services.AddConsumers(options =>
        {
            options.InboxConfiguration = new InboxConfiguration(
                new FirestoreInbox(configuration),
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw);
            // ... your subscriptions and channel factory
        });
    }
}
```

`FirestoreInbox` also has a constructor taking an `IAmAFirestoreConnectionProvider` alongside
the configuration, so one client can serve the Inbox, the Outbox and the lock.

## Firestore Inbox Options

**The Inbox adds no options of its own.** It is configured through the same
`FirestoreConfiguration` the Outbox takes, and those options are documented once at
[Firestore Outbox Options](/contents/FirestoreOutbox.md#firestore-outbox-options).

Only one of them is read differently: the Inbox uses the **`Inbox`** collection where the
Outbox uses `Outbox`. It defaults to `null`, and `FirestoreInbox` throws `ArgumentException`
from its constructor when `Inbox` is null or its `Name` is empty — so `null` here means *you
must set this*, not *this is optional*. The other collections are ignored by the Inbox, which
is what lets one configuration object drive all three roles.

## Provisioning the Firestore Inbox

**There is nothing to provision.** Firestore creates the collection the first time a document
is written to it, so there is no DDL, no builder and no migration chain — and the Inbox
therefore never appears in [Database Provisioning](/contents/BoxProvisioning.md), which covers
the relational backends only.

Two things remain yours:

- **Access.** The application's service account needs read and write permission on the
  collection named by `Inbox.Name`.
- **Expiry, if you set `Ttl`.** Brighter writes a `Ttl` timestamp field onto each document and
  nothing more. Firestore removes expired documents only once you create a **TTL policy** on
  that field through the Google Cloud console or the Firestore Admin API. Without the policy
  the field is written and the documents stay, which on an Inbox means a collection that only
  ever grows.

## Further Reading

- [Inbox Support](/contents/BrighterInboxSupport.md) — what an Inbox is for and when you need one
- [Firestore Outbox](/contents/FirestoreOutbox.md) — the shared configuration type and its options
- [Firestore Distributed Lock](/contents/FirestoreDistributedLock.md) — the third role the same configuration serves
- [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox) — where `InboxConfiguration` fits
