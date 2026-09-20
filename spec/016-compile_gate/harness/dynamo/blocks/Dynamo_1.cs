using System;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.DynamoDb;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Outbox.DynamoDB;
using Paramore.Brighter.Outbox.Hosting;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_DynamoOutbox_md_1
{

public void ConfigureServices(IServiceCollection services)
{
    // ... dynamoDb is your IAmazonDynamoDB client, producerRegistry your transport
    services.AddBrighter()
        .AddProducers(configure =>
        {
            configure.ProducerRegistry = producerRegistry;
            configure.Outbox = new DynamoDbOutbox(
                dynamoDb, new DynamoDbConfiguration(), TimeProvider.System);
            configure.ConnectionProvider = typeof(DynamoDbUnitOfWork);
            configure.TransactionProvider = typeof(DynamoDbUnitOfWork);
        })
        .UseOutboxSweeper()
        .AutoFromAssemblies();
}

}
