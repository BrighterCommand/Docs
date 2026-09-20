using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MsSql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MsSql;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_UsingSweeperCircuitBreaking_md_1
{

public void ConfigureServices(IServiceCollection services)
{
    // Register circuit breaker
    services.AddSingleton<IAmAnOutboxCircuitBreaker>(
        new InMemoryOutboxCircuitBreaker()  // Uses default cooldown of 10 sweeps
    );

    // ... producerRegistry and outboxConfiguration come from your transport
    // and your database configuration
    services.AddBrighter()
        .AddProducers(configure =>
        {
            configure.ProducerRegistry = producerRegistry;
            configure.Outbox = new MsSqlOutbox(outboxConfiguration);
            configure.ConnectionProvider = typeof(MsSqlConnectionProvider);
            configure.TransactionProvider = typeof(MsSqlTransactionProvider);
        })
        .UseOutboxSweeper(options =>       // Enable sweeper with circuit breaking
        {
            options.TimerInterval = 60;    // Sweep every 60 seconds
            options.BatchSize = 100;       // Process up to 100 messages per sweep
        });
}
}
