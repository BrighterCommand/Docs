---
description: "The MySQL Inbox allows use of MySQL for Brighter's inbox support."
layout:
  description:
    visible: false
---

# MySQL Inbox

> **Reference** · Applies to **Brighter V10**

## MySQL Inbox Usage
The MySQL Inbox allows use of MySQL for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox).

For this we will need the *Inbox* packages for the MySQL *Inbox*.

* **Paramore.Brighter.Inbox.MySql**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.MySql;
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
            var configuration = new RelationalDatabaseConfiguration(connectionString,  "brighter_test", inboxTableName: "inbox_messages");            
            options.InboxConfiguration = new InboxConfiguration(new MySqlInbox(configuration));
            // ...
        });
}

// ...
```

In Brighter 10.7.0 this configuration takes effect only in an application that also calls `AddProducers`; see [Global Inbox Configuration in a Consumer-Only Application](/contents/BrighterInboxSupport.md#global-inbox-configuration-in-a-consumer-only-application).

## Provisioning the MySQL Inbox Table

You have two equally valid options for creating and maintaining the Inbox table:

**Option A — Let Brighter provision and migrate it for you.**

Brighter ships a library that creates the Inbox table on first start and evolves its schema across Brighter releases. See [Database Provisioning](/contents/BoxProvisioning.md) and [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md).

**Option B — Manage the DDL yourself.**

Use `MySqlInboxBuilder.GetDDL()` to obtain the DDL Brighter ships and apply it via your own tooling (FluentMigrator, Flyway, Liquibase, or hand-rolled scripts).

Choose based on fit; neither option is deprecated.



