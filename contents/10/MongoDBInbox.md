# MongoDB Inbox

## Usage
The MongoDB Inbox allows use of MongoDB for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#inbox).

For this we will need the *Inbox* packages for the MySQL *Inbox*.

* **Paramore.Brighter.Inbox.Mongo**

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddServiceActivator(options =>
        { ...  })
        .UseExternalInbox(
            new MongoDbInbox(new MongoDbConfiguration("mongodb://root:example@localhost:27017", "brighter", "outbox_message"));
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        );
}

...

```



