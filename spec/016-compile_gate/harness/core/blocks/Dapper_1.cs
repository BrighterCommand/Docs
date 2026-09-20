using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Inbox.MySql;
using Paramore.Brighter.MySql;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.MySql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_DapperOutbox_md_1
{

public void ConfigureServices(IServiceCollection services)
{
    var configuration = new RelationalDatabaseConfiguration(
        connectionString,
        databaseName: "brighter_test",
        outBoxTableName: "outbox_messages",
        inboxTableName: "inbox_messages");

    services.AddConsumers(options =>
        {
            options.InboxConfiguration = new InboxConfiguration(new MySqlInbox(configuration));
        })
        .AddProducers(configure =>
        {
            configure.Outbox = new MySqlOutbox(configuration);
            configure.ConnectionProvider = typeof(MySqlConnectionProvider);
            configure.TransactionProvider = typeof(MySqlTransactionProvider);
        })
        .UseOutboxSweeper()
        .AutoFromAssemblies();
}

}
