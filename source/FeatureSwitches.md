# Feature Switches

We provide a **FeatureSwitch** Attribute that you can use on your
**IHandleRequests\<TRequest\>.Handle()** method. The **FeatureSwitch**
Attribute that you have configured will determine whether or not the
**IHandleRequests\<TRequest\>.Handle()** will be executed.

## Using the Feature Switch Attribute

By adding the **FeatureSwitch** Attribute, you instruct the Command
Processor to do one of the following:

-   run the handler as normal, this is **FeatureSwitchStatus.On**.
-   not execute the handler, this is **FeatureSwitchStatus.Off**.
-   detemine whether to run the handler based on a **Feature Switch
    Registry**, [creating of which is described
    later](FeatureSwitches.html#building-a-config-for-feature-switches-with-fluentconfigregistrybuilder).

In the following example, **MyFeatureSwitchedHandler** will only be run
if it has been configured in the **Feature Switch Registry** and set to
**FeatureSwitchStatus.On**.

``` csharp
class MyFeatureSwitchedHandler : RequestHandler<MyCommand>
{
    [FeatureSwitch(typeof(MyFeatureSwitchedHandler), FeatureSwitchStatus.Config, step: 1)]
    public override MyCommand Handle (MyCommand command)
    {
        /* Do work */
        return base.Handle(command);
    }
}
```

In the second example, **MyIncompleteHandler** will not be run in the
pipeline.

``` csharp
class MyIncompleteHandler : RequestHandler<MyCommand>
{
    [FeatureSwitch(typeof(MyIncompleteHandler), FeatureSwitchStatus.Off, step: 1)]
    public override MyCommand Handle(MyCommand command)
    {
        /* Nothing implmented so we're skipping this handler */
        return base.Handle(command);
    }
}
```

## Building a config for Feature Switches with FluentConfigRegistryBuilder

We provide a **FluentConfigRegistryBuilder** to build a mapping of
request handlers to **FeatureSwitchStatus**. For each Handler that you
wish to feature switch you supply a type and a status using a fluent
API. The valid statuses used in the builder are
**FeatureSwitchStatus.On** and **FeatureSwitchStatus.Off**.

``` csharp
var featureSwitchRegistry = FluentConfigRegistryBuilder
                            .With()
                            .StatusOf<MyFeatureSwitchedHandler>().Is(FeatureSwitchStatus.On)
                            .StatusOf<MyIncompleteHandler>().Is(FeatureSwitchStatus.Off)
                            .Build();
```

## Implementing a custom Feature Switch Registry

The **FluentConfigRegistryBuilder** provides compile time configuration
of **FeatureSwitch** Attributes. If this is not suitable to your needs
then you can write you own Feature Switch Registry using the
[IAmAFeatureSwitchRegistry](https://github.com/BrighterCommand/Brighter/blob/master/src/Paramore.Brighter/FeatureSwitch/IAmAFeatureSwitchRegistry.cs)
interface. The two requirements of this interface is a
[MissingConfigStrategy](https://github.com/BrighterCommand/Brighter/blob/master/src/Paramore.Brighter/FeatureSwitch/MissingConfigStrategy.cs),
and an implementation of **StatusOf(Type type)** which returns a
**FeatureSwitchStatus**.

The **MissingConfigStrategy** determines how the Command Processor
should behave when a Handler is decorated with a **FeatureSwitch**
Attribute that is set to **FeatureSwitchStatus.Config** does not exist
in the registry.

Your implementation of the **StatusOf** method is used to determine the
**FeatureSwitchStatus** of the Handler type that is passed in as a
parameter. How you store and retrieve these configurations is then up to
you.

In the following example there are two FeatureSwitches configured in the
**AppSettings.config**. We then build an **AppSettingsConfigRegistry**.
The **StatusOf** method is implemetned to read the config from the App
Settings and return the status for the given type.

``` xml
<appSettings>
    <add key="FeatureSwitch::Namespace.MyFeatureSwitchedHandler" value="on"/>
    <add key="FeatureSwitch::Namespace.MyIncompleteHandler" value="off"/>
</appSettings>    
```

``` csharp
class AppSettingsConfigRegistry : IAmAFeatureSwitchRegistry
{
    public MissingConfigStrategy MissingConfigStrategy { get; set; }

    public FeatureSwitchStatus StatusOf(Type handler)
    {            
        var configStatus = ConfigurationManager.AppSettings[$"FeatureSwitch::{handler}"].ToLower();

        switch (configStatus)
        {
            case "on":
                return FeatureSwitchStatus.On;
            case "off":
                return FeatureSwitchStatus.Off;
            default:
                return MissingConfigStrategy is MissingConfigStrategy.SilentOn 
                            ? FeatureSwitchStatus.On 
                            : MissingConfigStrategy is MissingConfigStrategy.SilentOff 
                                ? FeatureSwitchStatus.Off 
                                : throw new InvalidOperationException($"No Feature Switch configuration for {handler} specified");                    
        }
    }
}
```

## Setting Feature Switching Registry

We associate a **Feature Switch Registry** with a **Command Processor**
by passing it into the constructor of the **Command Processor**. For
convenience, we provide a **Commmand Processor Builder** that helps you
configure new instances of **Command Processor**.

``` csharp
var featureSwitchRegistry = FluentConfigRegistryBuilder
                            .With()
                            .StatusOf<MyFeatureSwitchedConfigHandler>().Is(FeatureSwitchStatus.Off)
                            .Build();

var builder = CommandProcessorBuilder
                    .With()
                    .Handlers(new HandlerConfiguration(_registry, _handlerFactory))
                    .DefaultPolicy()
                    .NoTaskQueues()
                    .ConfigureFeatureSwitches(fluentConfig)
                    .RequestContextFactory(new InMemoryRequestContextFactory());

var commandProcessor = builder.Build();
```
