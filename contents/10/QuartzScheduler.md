# Quartz scheduler

[Quartz](https://www.quartz-scheduler.net/) is one of the most used schedulers in the .NET community, on V10 we have added support to Quartz for [Brighter's scheduler support](/contents/BrighterScheduleroSupport.md).

## Usage
For this we will need the *Paramore.Brighter.MessageScheduler.Quartz* packages.

* **Paramore.Brighter.MessageScheduler.Quartz**

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

By default Brighter uses a `Guid.NewGuid()` to define the scheduler name, it can be customized by

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

Allow the customer to define a Quartz group that Brighter will use

```c#
_  = new QuartzSchedulerFactory(factory.GetScheduler().GetAwaiter().GetResult())
{
    Group = "some-group"
}
```