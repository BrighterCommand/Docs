# Quartz scheduler

[Quartz](https://www.quartz-scheduler.net/) is one of the most widely used schedulers in the .NET community. In V10 we have added support to Quartz for [Brighter's scheduler functionality](/contents/BrighterSchedulerSupport.md).


## Usage

To use Quartz as a scheduler, configure Brighter with the appropriate settings.

### Example Configuration

For this integration, you will need the `Paramore.Brighter.MessageScheduler.Quartz` package.

```csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    services
        .AddSingleton<QuartzBrighterJob>()
        .AddServiceActivator(options => { ...  })
        .UseScheduler(provider =>
        {
            var factory = provider.GetRequiredService<ISchedulerFactory>();
            return new QuartzSchedulerFactory(factory.GetScheduler().GetAwaiter().GetResult());
        });
}
...
```

## Configuration

### Custom Scheduler Name

By default, Brighter uses `Guid.NewGuid()` to define the scheduler name. This behavior can be customized:

```c#
_  = new QuartzSchedulerFactory(factory.GetScheduler().GetAwaiter().GetResult())
{
    GetOrCreateMessageSchedulerId = message => message.Id;
    GetOrCreateRequestSchedulerId => request => 
    {
        if(request is MyCommand command)
        {
            return command.SomeProperty;
        }

        return request.Id.ToString();
    };
}
```

### Group

Allow customers to define a Quartz group that Brighter will use:

```c#
_  = new QuartzSchedulerFactory(factory.GetScheduler().GetAwaiter().GetResult())
{
    Group = "some-group"
}
```