# MSSQL Inbox

## Usage
The MSSQL Inbox allows use of MSSQL for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md).

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
    services.AddServiceActivator(options =>
        { ...  })
        .UseExternalInbox(
            new MsSqlInbox(new MsSqlInboxConfiguration("server=localhost; port=3306; uid=root; pwd=root; database=Salutations", "Inbox");
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        );
}

...

```

