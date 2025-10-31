# Hangfire scheduler

[Hangfire](https://www.hangfire.io/) is one of the most widely used schedulers in the .NET community. In V10 we have added support to Quartz for [Brighter's scheduler functionality](/contents/BrighterSchedulerSupport.md).

## Usage

For this we will need the *Paramore.Brighter.MessageScheduler.Hangfire* packages.

### Example Configuration
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
        .AddSingleton<BrighterHangfireSchedulerJob>()
        .AddHangfire(opt => { ... })
        .AddServiceActivator(opt =>{ ...  })
        .UseScheduler(new HangfireSchedulerFactory());
}
...
```

## Configuration

### Queue
Allow customers to define a custom Hangfire queue that Brighter will use for scheduler a message

```c#
_  = new HangfireSchedulerFactory()
{
    Queue = "some-queue"
}
```