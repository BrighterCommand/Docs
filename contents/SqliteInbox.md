# Sqlite Inbox

## Usage
The Sqlite Inbox allows use of Sqlite for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#inbox).

For this we will need the *Inbox* packages for the Sqlite *Inbox*.

* **Paramore.Brighter.Inbox.Sqlite**

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
            var configuration = new RelationalDatabaseConfiguration(connectionString, "brighter", inboxTableName: "inbox_messages");
            opt.InboxConfiguration = new InboxConfiguration(new SqliteInbox(configuration), actionOnExists: OnceOnlyAction.Warn);
            ...
        });
}

...

```



