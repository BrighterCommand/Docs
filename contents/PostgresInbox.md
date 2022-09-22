# Postgres Inbox

## Usage
The Postgres Inbox allows use of Postgres for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md#inbox).

For this we will need the *Inbox* packages for the Postgres *Inbox*.

* **Paramore.Brighter.Inbox.Postgres**

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
            new PostgresSqlInbox(new PostgresSqlInboxConfiguration("Host=localhost; Username=root; Password=root; Database=Salutations", "Inbox");
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        );
}

...

```



