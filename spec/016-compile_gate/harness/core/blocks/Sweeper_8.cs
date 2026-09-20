using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MsSql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MsSql;
using static PageContext;   // page-context helpers the page names but does not define

public static class Block_SweeperCircuitBreaking_md_8
{
    public static async System.Threading.Tasks.Task Run(Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
    {

// ... outboxConfiguration comes from your database configuration
services.AddBrighter()
    .AddProducers(configure =>
    {
        configure.Outbox = new MsSqlOutbox(outboxConfiguration);
        configure.ConnectionProvider = typeof(MsSqlConnectionProvider);
        configure.TransactionProvider = typeof(MsSqlTransactionProvider);
    })
    .UseOutboxSweeper();  // Required for circuit breaking to function
    }
}
