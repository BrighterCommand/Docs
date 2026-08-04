# MSSQL Inbox

> **Reference** · Applies to **Brighter V10**

## Usage
The MSSQL Inbox allows use of MSSQL for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#inbox).

For this we will need the *Inbox* packages for the MsSQL *Inbox*.

* **Paramore.Brighter.Inbox.MsSql**

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddConsumers(options =>
        {
            var configuration = new RelationalDatabaseConfiguration(connectionString, "BrighterTests", inboxTableName: "InboxMessages");
            opt.Inbox = new InboxConfiguration(new MsSqlInbox(configuration));
            ...
        });
}

...

```

## Provisioning the Inbox Table

You have two equally valid options for creating and maintaining the Inbox table:

**Option A — Let Brighter provision and migrate it for you.**

Brighter ships a library that creates the Inbox table on first start and evolves its schema across Brighter releases. See [Database Provisioning](/contents/BoxProvisioning.md) and [Configuring Box Provisioning](/contents/BoxProvisioningConfiguration.md).

**Option B — Manage the DDL yourself.**

Use `MsSqlInboxBuilder.GetDDL()` to obtain the DDL Brighter ships and apply it via your own tooling (FluentMigrator, Flyway, Liquibase, or hand-rolled scripts).

Choose based on fit; neither option is deprecated.

