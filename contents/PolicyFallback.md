# Fallback

You may want some sort of backstop exception handler, that allows you to take compensating action, such as undoing any partially committed work, issuing a compensating transaction, or queuing work for later delivery (perhaps using the [External Bus](/contents/ImplementingExternalBus.md)).

To support this we provide a **IHandleRequests\<TRequest\>Fallback** method. In the Fallback method you write your code to run in the event of failure.

## Calling the Fallback Pipeline

We provide a **FallbackPolicy** Attribute that you can use on your **IHandleRequests\<TRequest\>.Handle()** method. The implementation of the **Fallback Policy Handler** is straightforward: it creates a backstop exception handler by encompassing later requests in the [Request Handling Pipeline](BuildingAPipeline.html) in a try\...catch block. You can configure it to catch all exceptions, or just [Broken Circuit Exceptions](PolicyRetryAndCircuitBreaker.html) when a Circuit Breaker has tripped.

When the **Fallback Policy Handler** catches an exception it calls the **IHandleRequests\<TRequest\>.Fallback()** method of the next Handler in the pipeline, as determined by **IHandleRequests\<TRequest\>.Successor**

The implementation of **RequestHandler\<T\>.Fallback()** uses the same [Russian Doll](BuildingAPipeline.html) approach as it uses for **RequestHandler\<T\>.Handle()**. This means that the request to take compensating action for failure, flows through the same pipeline as the request for service, allowing each Handler in the chain to contribute.

In addition the **Fallback Policy Handler** makes the originating exception available to subsequent Handlers using the **Context Bag** with the key: **CAUSE_OF_FALLBACK_EXCEPTION**

## Using the FallbackPolicy Attribute

The following example shows a Handler with **Request Handler Attributes** for [Retry and Circuit Breaker policies](PolicyRetryAndCircuitBreaker.html) that is configured with a **Fallback Policy** which catches a **Broken Circuit Exception** (raised when the Circuit Breaker is tripped) and initiates the Fallback chain.

### Example with Resilience Pipelines (V10)

```csharp
public class MyFallbackProtectedHandler: RequestHandler<MyCommand>
{
    [FallbackPolicy(backstop: false, circuitBreaker: true, step: 1)]
    [UseResiliencePipeline("MyCircuitBreakerPipeline", step: 2)]
    [UseResiliencePipeline("MyRetryPipeline", step: 3)]
    public override MyCommand Handle(MyCommand command)
    {
        // Do some work that can fail
        var result = CallExternalService();
        return base.Handle(command);
    }

    public override MyCommand Fallback(MyCommand command)
    {
        if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
        {
            var exception = Context.Bag[FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION] as Exception;

            // Log the failure
            _logger.LogError(exception, "Handler failed, executing fallback");

            // Take compensating action
            if (exception is BrokenCircuitException)
            {
                // Circuit breaker is open, queue for later retry
                _messageQueue.QueueForRetry(command);
            }
            else
            {
                // Other failure, send compensating transaction
                _commandProcessor.Send(new UndoCommand { OriginalCommandId = command.Id });
            }
        }
        return base.Fallback(command);
    }
}
```

### Example with Legacy Policies (V9)

```csharp
public class MyFallbackProtectedHandler: RequestHandler<MyCommand>
{
    [FallbackPolicy(backstop: false, circuitBreaker: true, step: 1)]
    [UsePolicy(new [] {"MyCircuitBreakerStrategy", "MyRetryStrategy"}, step: 2)]
    public override MyCommand Handle(MyCommand command)
    {
        /*Do some work that can fail*/
    }

    public override MyCommand Fallback(MyCommand command)
    {
        if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
        {
            /*Use fallback information to determine what action to take*/
        }
        return base.Fallback(command);
    }
}
```

## Scope of a Fallback

Where you put any **FallbackPolicy** attribute determines what exceptions it will call your Fallback method to guard against. This is controlled by the **Step** parameter. Remember that you encapsulate anything with a higher **Step** and can react to an exception thrown there.

### Pipeline Order

```csharp
[FallbackPolicy(backstop: true, step: 1)]         // Outermost: Catches ALL exceptions
[UseResiliencePipeline("CircuitBreaker", step: 2)] // Middle: Circuit breaker
[UseResiliencePipeline("Retry", step: 3)]          // Innermost: Retry
public override MyCommand Handle(MyCommand command)
{
    // Handler logic
}
```

**Execution flow**:

1. Fallback wraps everything (catches all exceptions from steps 2, 3, and handler)
2. Circuit breaker wraps retry and handler (fails fast if open)
3. Retry wraps handler (retries on failures)
4. Handler executes

If the handler throws an exception:

1. Retry catches it and retries (up to max attempts)
2. If retries are exhausted, exception bubbles to Circuit Breaker
3. If Circuit Breaker opens or threshold is hit, BrokenCircuitException is thrown
4. Fallback catches BrokenCircuitException and calls Fallback() method

## Fallback Policy Options

### backstop Parameter

When `backstop: true`, the Fallback Policy catches **all exceptions**, not just circuit breaker exceptions.

```csharp
[FallbackPolicy(backstop: true, step: 1)]
public override MyCommand Handle(MyCommand command)
{
    // Any exception will trigger fallback
}
```

### circuitBreaker Parameter

When `circuitBreaker: true` (and `backstop: false`), the Fallback Policy only catches **BrokenCircuitException** exceptions.

```csharp
[FallbackPolicy(backstop: false, circuitBreaker: true, step: 1)]
[UseResiliencePipeline("MyCircuitBreakerPipeline", step: 2)]
public override MyCommand Handle(MyCommand command)
{
    // Only BrokenCircuitException will trigger fallback
}
```

## Common Fallback Patterns

### 1. Compensating Transaction

When a handler fails, issue a compensating transaction to undo any partially committed work.

```csharp
public override MyCommand Fallback(MyCommand command)
{
    if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
    {
        var exception = Context.Bag[FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION] as Exception;
        _logger.LogError(exception, "Handler failed, issuing compensating transaction");

        // Send compensating command to undo changes
        _commandProcessor.Send(new CancelOrderCommand
        {
            OrderId = command.OrderId,
            Reason = "Handler failure"
        });
    }
    return base.Fallback(command);
}
```

### 2. Queue for Later Delivery

When a circuit breaker opens (service is down), queue the message for later retry using the [External Bus](ImplementingExternalBus.md).

```csharp
public override MyCommand Fallback(MyCommand command)
{
    if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
    {
        var exception = Context.Bag[FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION] as Exception;

        if (exception is BrokenCircuitException)
        {
            _logger.LogWarning("Circuit breaker open, queueing message for later delivery");

            // Use External Bus to queue message for later retry
            _commandProcessor.Post(command, new RequestContext());
        }
    }
    return base.Fallback(command);
}
```

### 3. Default Value or Cached Response

When a handler fails, return a default value or cached response for graceful degradation.

```csharp
public class GetProductHandler : RequestHandler<GetProductQuery>
{
    private readonly IProductCache _cache;

    [FallbackPolicy(backstop: true, step: 1)]
    [UseResiliencePipeline("ProductServicePipeline", step: 2)]
    public override GetProductQuery Handle(GetProductQuery query)
    {
        // Call external product service
        query.Product = _productService.GetProduct(query.ProductId);
        return base.Handle(query);
    }

    public override GetProductQuery Fallback(GetProductQuery query)
    {
        if (Context.Bag.ContainsKey(FallbackPolicyHandler<GetProductQuery>.CAUSE_OF_FALLBACK_EXCEPTION))
        {
            _logger.LogWarning("Product service failed, returning cached value");

            // Return cached product or default
            query.Product = _cache.GetProduct(query.ProductId) ?? Product.Default;
        }
        return base.Fallback(query);
    }
}
```

### 4. Alert and Fail Gracefully

Log critical errors and alert operations team, then fail gracefully.

```csharp
public override MyCommand Fallback(MyCommand command)
{
    if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
    {
        var exception = Context.Bag[FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION] as Exception;

        // Log critical error
        _logger.LogCritical(exception, "Critical handler failure for command {CommandId}", command.Id);

        // Alert operations team (e.g., PagerDuty, Slack, etc.)
        _alertService.SendAlert(new Alert
        {
            Severity = AlertSeverity.Critical,
            Message = $"Handler failed: {exception.Message}",
            Context = command
        });

        // Mark command as failed for later investigation
        _failedCommandRepository.Add(new FailedCommand
        {
            CommandId = command.Id,
            Exception = exception,
            Timestamp = DateTime.UtcNow
        });
    }
    return base.Fallback(command);
}
```

## Integration with Polly Fallback Strategy

> **Note**: Polly v8 also provides a [Fallback resilience strategy](https://www.pollydocs.org/strategies/fallback.html). Brighter's `FallbackPolicy` attribute and Polly's Fallback strategy serve different purposes:

| Feature | Brighter FallbackPolicy | Polly Fallback Strategy |
|---------|-------------------------|-------------------------|
| **Purpose** | Calls handler's `Fallback()` method for compensating actions | Returns alternative value/result |
| **Use Case** | Compensating transactions, queuing for retry, complex recovery logic | Simple value substitution, cached responses |
| **Flow** | Flows through handler pipeline (Russian Doll) | Returns value directly |
| **Access** | Full access to handler dependencies and context | Limited to callback scope |

You can use both together for comprehensive error handling:

```csharp
// Configure Polly Fallback for simple value substitution
resiliencePipelineRegistry.TryAddBuilder("ProductServiceWithFallback",
    (builder, context) => builder
        .AddFallback(new FallbackStrategyOptions<Product>
        {
            FallbackAction = args => Outcome.FromResult(Product.Default)
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions { /* ... */ })
        .AddRetry(new RetryStrategyOptions { /* ... */ }));

// Use Brighter FallbackPolicy for complex recovery logic
public class ProcessOrderHandler : RequestHandler<ProcessOrderCommand>
{
    [FallbackPolicy(backstop: true, step: 1)]
    [UseResiliencePipeline("ProductServiceWithFallback", step: 2)]
    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        // Polly fallback returns default product if service fails
        var product = _productService.GetProduct(command.ProductId);

        // Process order with product
        _orderService.CreateOrder(command, product);

        return base.Handle(command);
    }

    public override ProcessOrderCommand Fallback(ProcessOrderCommand command)
    {
        // Brighter fallback handles order-level compensating actions
        if (Context.Bag.ContainsKey(FallbackPolicyHandler<ProcessOrderCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
        {
            // Cancel order, refund payment, notify customer, etc.
            _orderService.CancelOrder(command.OrderId);
        }
        return base.Fallback(command);
    }
}
```

## Best Practices

1. **Use Backstop Sparingly**: Only catch all exceptions (`backstop: true`) when you have comprehensive recovery logic. Otherwise, use `circuitBreaker: true` to catch only circuit breaker exceptions.

2. **Log Exceptions**: Always log the exception from the context bag for debugging and monitoring.

3. **Idempotent Compensating Actions**: Ensure your compensating actions are idempotent in case the fallback is called multiple times.

4. **Consider Transactional Integrity**: When using compensating transactions, ensure they maintain eventual consistency.

5. **Monitor Fallback Execution**: Add metrics and logging to track how often fallbacks are executed (indicates system health).

6. **Test Fallback Logic**: Unit test your `Fallback()` methods to ensure they handle failures correctly.

7. **Use External Bus for Retry**: When queueing failed commands for retry, use the [External Bus](ImplementingExternalBus.md) to ensure durability.

8. **Combine with Circuit Breakers**: Fallback works well with circuit breakers to prevent cascading failures and provide graceful degradation.

## Additional Resources

- [Retry and Circuit Breaker](PolicyRetryAndCircuitBreaker.md)
- [Building a Handler Pipeline](BuildingAPipeline.md)
- [Using the Context Bag](UsingTheContextBag.md)
- [External Bus](ImplementingExternalBus.md)
- [Polly Fallback Strategy](https://www.pollydocs.org/strategies/fallback.html)
