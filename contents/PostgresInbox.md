---
description: "The Postgres Inbox allows use of Postgres for Brighter's inbox support."
layout:
  description:
    visible: false
---

# Postgres Inbox

> **Reference** · Applies to **Brighter V10**

## Postgres Inbox Usage
The Postgres Inbox allows use of Postgres for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox).

For this we will need the *Inbox* packages for the Postgres *Inbox*.

* **Paramore.Brighter.Inbox.Postgres**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Postgres;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        });

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(options =>
        {
            var config = new RelationalDatabaseConfiguration(connectionString, "brightertests", inboxTableName: "inboxmessages");
            options.InboxConfiguration = new InboxConfiguration(new PostgreSqlInbox(config));
            // ...
        });
}

// ...
```

In Brighter 10.7.0 this configuration takes effect only in an application that also calls `AddProducers`; see [Global Inbox Configuration in a Consumer-Only Application](/contents/BrighterInboxSupport.md#global-inbox-configuration-in-a-consumer-only-application).

## Provisioning the Postgres Inbox Table

You have two equally valid options for creating and maintaining the Inbox table:

**Option A — Let Brighter provision and migrate it for you.**

Brighter ships a library that creates the Inbox table on first start and evolves its schema across Brighter releases. See [Database Provisioning](/contents/BoxProvisioning.md) and [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md). The PostgreSQL Inbox is at schema version 1 — the table shipped with its final column set, so there are no inbox migrations for this backend to apply.

**Option B — Manage the DDL yourself.**

Use `PostgreSqlInboxBuilder.GetDDL()` to obtain the DDL Brighter ships and apply it via your own tooling (FluentMigrator, Flyway, Liquibase, or hand-rolled scripts).

Choose based on fit; neither option is deprecated.



