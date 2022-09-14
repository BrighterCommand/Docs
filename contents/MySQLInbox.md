# MySQL Inbox

## Usage
The MySQL Inbox allows use of MySQL for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md).

For this we will need the *Inbox* packages for the MySQL *Inbox*.

* **Paramore.Brighter.Inbox.MySql**

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
            new MySqlInbox(new MySqlInboxConfiguration("server=localhost; port=3306; uid=root; pwd=root; database=Salutations", "Inbox");
            new InboxConfiguration(
                scope: InboxScope.Commands,
                onceOnly: true,
                actionOnExists: OnceOnlyAction.Throw
            )
        );
}

...

```



