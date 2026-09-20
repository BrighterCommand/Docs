using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;

public class OrderHandler : RequestHandlerAsync<CreateOrderCommand>
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly IAmATransactionConnectionProvider _transactionProvider;
    private readonly IOrderRepository _orderRepository;

    public override async Task<CreateOrderCommand> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var context = new RequestContext();

        // Set routing information
        context.Bag[RequestContextBagNames.PartitionKey] = command.TenantId;
        context.Bag[RequestContextBagNames.Headers] = new Dictionary<string, object>
        {
            ["x-source-command"] = nameof(CreateOrderCommand),
            ["x-tenant-id"] = command.TenantId
        };

        var posts = new List<Id>();

        var conn = await _transactionProvider.GetConnectionAsync(cancellationToken);
        var tx = await _transactionProvider.GetTransactionAsync(cancellationToken);

        try
        {
            // Save order to database
            await _orderRepository.AddAsync(command, cancellationToken);

            // Deposit message with explicit context
            posts.Add(await _commandProcessor.DepositPostAsync(
                new OrderCreatedEvent { OrderId = command.OrderId },
                _transactionProvider,
                requestContext: context,
                cancellationToken: cancellationToken
            ));

            await _transactionProvider.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _transactionProvider.RollbackAsync(cancellationToken);
            throw;
        }

        // Clear outbox
        await _commandProcessor.ClearOutboxAsync(posts, cancellationToken: cancellationToken);

        return await base.HandleAsync(command, cancellationToken);
    }
}
