using System;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using static PageContext;   // page-context helpers the page names but does not define

public static class Block_ShowMeTheCode_md_6
{
    public static async System.Threading.Tasks.Task Run(Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {

services.AddBrighter(options =>
{
    // Configure handlers
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.MapperLifetime = ServiceLifetime.Singleton;
})
.AddProducers(configure =>
{
    // Configure your transport (RabbitMQ, Kafka, AWS, etc.)
    var connection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    configure.ProducerRegistry = new RmqProducerRegistryFactory(
        connection,
        new[] { new RmqPublication { Topic = new RoutingKey("greeting.made"), RequestType = typeof(GreetingMade) } }
    ).Create();

    // A development Outbox that lives in memory; this is also what you get
    // if you set no Outbox at all
    configure.Outbox = new InMemoryOutbox(TimeProvider.System);
})
.AutoFromAssemblies(); // Auto-discover handlers
    }
}
