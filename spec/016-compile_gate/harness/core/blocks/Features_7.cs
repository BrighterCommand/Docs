using Paramore.Brighter;
using Paramore.Brighter.FeatureSwitch;
using Paramore.Brighter.FeatureSwitch.Providers;
using static PageContext;   // page-context helpers the page names but does not define

public static class Block_FeatureSwitches_md_7
{
    public static async System.Threading.Tasks.Task Run(Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {

var featureSwitchRegistry = FluentConfigRegistryBuilder
                            .With()
                            .StatusOf<MyFeatureSwitchedConfigHandler>().Is(FeatureSwitchStatus.Off)
                            .Build();

var builder = CommandProcessorBuilder
                    .StartNew()
                    .ConfigureFeatureSwitches(featureSwitchRegistry)
                    .Handlers(new HandlerConfiguration(_registry, _handlerFactory))
                    .DefaultResilience()
                    .NoExternalBus()
                    .NoInstrumentation()
                    .RequestContextFactory(new InMemoryRequestContextFactory())
                    .RequestSchedulerFactory(new InMemorySchedulerFactory());

var commandProcessor = builder.Build();
    }
}
