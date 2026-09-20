using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox;
using Paramore.Brighter.Inbox.MySql;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.MySql;
using Paramore.Brighter.Outbox.MySql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using static PageContext;   // page-context helpers the page names but does not define

public static class Block_BrighterBasicConfiguration_md_5
{

private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(20);
            });
            ConfigureBrighter(hostContext, services);
        });

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
    var subscriptions = new Subscription[]
    {
        new RmqSubscription<GreetingMade>(
            new SubscriptionName("paramore.sample.salutationanalytics"),
            new ChannelName("SalutationAnalytics"),
            new RoutingKey("GreetingMade"),
            messagePumpType: MessagePumpType.Proactor,
            timeOut: TimeSpan.FromMilliseconds(200),
            isDurable: true,
            makeChannels: OnMissingChannel.Create), //change to OnMissingChannel.Validate if you have infrastructure declared elsewhere
    };

    var rmqConnection = new RmqMessagingGatewayConnection
    {
        AmpqUri = new AmqpUriSpecification(new Uri($"amqp://guest:guest@localhost:5672")),
        Exchange = new Exchange("paramore.brighter.exchange")
    };

    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnection);

    var outboxConfiguration = new RelationalDatabaseConfiguration(
        DbConnectionString(), outBoxTableName: "Outbox");

    services.AddConsumers(options =>
    {
        options.Subscriptions = subscriptions;
        options.DefaultChannelFactory = new ChannelFactory(rmqMessageConsumerFactory);
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.MapperLifetime = ServiceLifetime.Singleton;
        options.PolicyRegistry = new SalutationPolicy();
        options.InboxConfiguration = new InboxConfiguration(
            inbox: new MySqlInbox(new RelationalDatabaseConfiguration(DbConnectionString())),
            scope: InboxScope.Commands,
            onceOnly: true,
            actionOnExists: OnceOnlyAction.Throw
        );
    })
    .AddProducers(configure =>
    {
        configure.ProducerRegistry = new RmqProducerRegistryFactory(
            rmqConnection,
            new[] { new RmqPublication { Topic = new RoutingKey("GreetingMade"), RequestType = typeof(GreetingMade) } }
        ).Create();
        configure.Outbox = new MySqlOutbox(outboxConfiguration);
        configure.ConnectionProvider = typeof(MySqlConnectionProvider);
        configure.TransactionProvider = typeof(MySqlTransactionProvider);
    })
    .AutoFromAssemblies();

    services.AddHostedService<ServiceActivatorHostedService>();
}

}
