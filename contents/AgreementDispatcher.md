# Agreement Dispatcher

> **How-to** · Applies to **Brighter V10**

## Agreement Dispatcher Overview

The **Agreement Dispatcher** is a pattern for routing requests to handlers dynamically based on the request's content or context, rather than using a fixed type-to-handler mapping. This pattern, described by Martin Fowler in [Patterns of Enterprise Application Architecture](https://martinfowler.com/eaaDev/AgreementDispatcher.html), enables flexible routing logic that can change based on business rules, time, geography, or other runtime conditions.

Brighter supports an Agreement Dispatcher, allowing you to register a lambda function that determines which handler(s) should process a request at runtime.

## Registration Syntax

### Basic Registration

```csharp
registry.Register<TRequest>(
    routingFunc: (request, context) => { /* return handler types */ },
    handlerTypes: [typeof(Handler1), typeof(Handler2), ...]
);
```

**Parameters:**

- **routingFunc**: Lambda that takes `IRequest` and `IRequestContext`, returns `List<Type>` of handlers
- **handlerTypes**: Array of all possible handler types (for DI registration)

### Accessing Request Content

The routing function receives `IRequest`, which you must cast to your specific type:

```csharp
registry.Register<MyCommand>((request, context) =>
{
    // Cast to your specific type to access properties
    var myCommand = request as MyCommand;

    if (myCommand?.Value == "special")
        return [typeof(SpecialHandler)];

    return [typeof(StandardHandler)];
},
[typeof(SpecialHandler), typeof(StandardHandler)]);
```

**Why the cast?** The registry supports multiple request types, so the lambda signature uses `IRequest`. You need to cast to access type-specific properties.

### Accessing Request Context

The `IRequestContext` provides additional information:

```csharp
registry.Register<ProcessOrder>((request, context) =>
{
    var order = request as ProcessOrder;

    // Access context properties
    var userId = context.Bag.TryGetValue("UserId", out var id) ? id : null;
    var tenant = context.Bag.TryGetValue("TenantId", out var t) ? t : null;

    // Route based on context
    if (tenant?.ToString() == "premium")
        return [typeof(PremiumOrderHandler)];

    return [typeof(StandardOrderHandler)];
},
[typeof(PremiumOrderHandler), typeof(StandardOrderHandler)]);
```

### Returning Multiple Handlers

Agreement Dispatcher can return multiple handlers, but it must still obey the rule that `Send` expects a single handler and `Publish` can have zero-to-many handlers. If you return multiple handlers in a `Send` request pipeline, Brighter will throw an exception.

## Synchronous and Asynchronous Registration

Agreement Dispatcher supports both sync and async handlers:

### Synchronous Registration

```csharp
registry.Register<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    return cmd?.Priority == "High"
        ? [typeof(HighPriorityHandler)]
        : [typeof(StandardHandler)];
},
[typeof(HighPriorityHandler), typeof(StandardHandler)]);
```

### Asynchronous Registration

```csharp
registry.RegisterAsync<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    return cmd?.Priority == "High"
        ? [typeof(HighPriorityHandlerAsync)]
        : [typeof(StandardHandlerAsync)];
},
[typeof(HighPriorityHandlerAsync), typeof(StandardHandlerAsync)]);
```

**Note**: The routing lambda itself is always synchronous. Only the handler execution is async when using `RegisterAsync`.

## Integration with Dynamic Message Deserialization

Agreement Dispatcher can be combined with [Dynamic Message Deserialization](DynamicMessageDeserialization.md) for two-level routing:

```csharp
// Level 1: Dynamic deserialization (CloudEvents type → Request type)
var subscription = new KafkaSubscription(
    new SubscriptionName("paramore.example.orders"),
    channelName: new ChannelName("orders"),
    routingKey: new RoutingKey("orders"),
    getRequestType: message => message.Header.Type switch
    {
        var t when t == new CloudEventsType("com.example.order.created")
            => typeof(OrderCreated),
        _ => throw new ArgumentException($"Unknown type: {message.Header.Type}")
    },
    groupId: "order-processor",
    timeOut: TimeSpan.FromMilliseconds(100)
);

// Level 2: Agreement dispatcher (Request content → Handler)
services.AddBrighter(options => { })
    .AddConsumers(options => { options.Subscriptions = new[] { subscription }; })
    .Handlers(registry =>
    {
        registry.Register<OrderCreated>((request, context) =>
        {
            var order = request as OrderCreated;

            // Route to different handlers based on order content
            return order?.Country switch
            {
                "US" => [typeof(USOrderCreatedHandler)],
                "UK" => [typeof(UKOrderCreatedHandler)],
                _ => [typeof(InternationalOrderCreatedHandler)]
            };
        },
        [
            typeof(USOrderCreatedHandler),
            typeof(UKOrderCreatedHandler),
            typeof(InternationalOrderCreatedHandler)
        ]);
    });
```

This provides powerful, flexible routing:

1. **CloudEvents type** determines the Request type
2. **Request content** determines the Handler

## Complete Example

Here's a complete example showing Agreement Dispatcher with multiple routing strategies:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddBrighter(options =>
        {
            options.HandlerLifetime = ServiceLifetime.Scoped;
        })
        .Handlers(registry =>
        {
            // Time-based routing for tax calculations
            registry.Register<CalculateTax>((request, context) =>
            {
                var taxRequest = request as CalculateTax;
                var effectiveDate = taxRequest?.EffectiveDate ?? DateTime.UtcNow;

                if (effectiveDate < new DateTime(2025, 1, 1))
                    return [typeof(TaxCalculator2024)];

                return [typeof(TaxCalculator2025)];
            },
            [typeof(TaxCalculator2024), typeof(TaxCalculator2025)]);

            // Country-based routing for payments
            registry.RegisterAsync<ProcessPayment>((request, context) =>
            {
                var payment = request as ProcessPayment;

                return payment?.Country switch
                {
                    "US" => [typeof(StripePaymentHandlerAsync)],
                    "UK" => [typeof(PayPalPaymentHandlerAsync)],
                    "JP" => [typeof(LocalPaymentHandlerAsync)],
                    _ => [typeof(InternationalPaymentHandlerAsync)]
                };
            },
            [
                typeof(StripePaymentHandlerAsync),
                typeof(PayPalPaymentHandlerAsync),
                typeof(LocalPaymentHandlerAsync),
                typeof(InternationalPaymentHandlerAsync)
            ]);

            // Content-based routing with multiple handlers
            registry.Register<ProcessOrder>((request, context) =>
            {
                var order = request as ProcessOrder;
                var handlers = new List<Type>
                {
                    typeof(ValidateOrderHandler)  // Always validate
                };

                // High-value orders get fraud check
                if (order?.Total > 10000m)
                    handlers.Add(typeof(FraudCheckHandler));

                // International orders need approval
                if (order?.IsInternational == true)
                    handlers.Add(typeof(ApprovalHandler));

                handlers.Add(typeof(FinalizeOrderHandler));  // Always finalize

                return handlers;
            },
            [
                typeof(ValidateOrderHandler),
                typeof(FraudCheckHandler),
                typeof(ApprovalHandler),
                typeof(FinalizeOrderHandler)
            ]);

            // Standard routing for simple commands
            registry.Register<SimpleCommand, SimpleCommandHandler>();
        });
    }
}
```

## Agreement Dispatcher Best Practices

### 1. Keep Routing Logic Simple

Routing lambdas should be fast and deterministic:

```csharp
// Good - Simple, fast
registry.Register<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    return cmd?.Priority == "High"
        ? [typeof(HighPriorityHandler)]
        : [typeof(StandardHandler)];
},
[typeof(HighPriorityHandler), typeof(StandardHandler)]);

// Bad - Complex, slow
registry.Register<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    // Avoid expensive operations!
    var config = LoadConfigFromDatabase();
    var result = CallExternalApi(cmd);
    return CalculateComplexRouting(cmd, config, result);
},
[/* handlers */]);
```

### 2. Provide Clear Error Messages

Handle unexpected cases gracefully:

```csharp
registry.Register<ProcessOrder>((request, context) =>
{
    var order = request as ProcessOrder;

    return order?.Country switch
    {
        "US" => [typeof(USOrderHandler)],
        "UK" => [typeof(UKOrderHandler)],
        "EU" => [typeof(EUOrderHandler)],
        _ => throw new InvalidOperationException(
            $"No handler configured for country: {order?.Country}. " +
            $"Order ID: {order?.Id}, Supported countries: US, UK, EU"
        )
    };
},
[typeof(USOrderHandler), typeof(UKOrderHandler), typeof(EUOrderHandler)]);
```

### 3. Document Routing Rules

Document the routing logic for maintainability:

```csharp
/// <summary>
/// Routes payment processing based on country:
/// - US: Stripe
/// - UK: PayPal
/// - JP: Local payment provider
/// - Others: International gateway
/// </summary>
registry.RegisterAsync<ProcessPayment>((request, context) =>
{
    var payment = request as ProcessPayment;
    // ...
},
[/* handlers */]);
```

### 4. Use Standard Routing When Possible

Only use Agreement Dispatcher when you need dynamic routing:

```csharp
// Good - Use standard routing for simple cases
registry.Register<SimpleCommand, SimpleCommandHandler>();

// Only use Agreement Dispatcher when needed
registry.Register<ComplexCommand>((request, context) =>
{
    // Dynamic routing based on content
},
[/* handlers */]);
```

### 5. List All Possible Handlers

Always provide the complete list of handler types:

```csharp
// Good - Complete list
registry.Register<MyCommand>((request, context) => { /* ... */ },
[
    typeof(Handler1),
    typeof(Handler2),
    typeof(Handler3)
    // All handlers that might be returned
]);

// Bad - Incomplete list
registry.Register<MyCommand>((request, context) =>
{
    // Might return Handler3, but it's not in the list!
    return [typeof(Handler3)];
},
[
    typeof(Handler1),
    typeof(Handler2)
    // Handler3 missing - will fail at runtime!
]);
```

## Agreement Dispatcher Troubleshooting

### Handler Not Found Error

**Problem**: Runtime error saying handler type cannot be resolved.

**Cause**: Handler type not in the handler types array.

**Solution**: Add the handler to the array:

```csharp
registry.Register<MyCommand>((request, context) => [typeof(MyHandler)],
[
    typeof(MyHandler)  // Must be listed here!
]);
```

### AutoFromAssemblies Conflicts

**Problem**: Agreement dispatcher routes not working with `AutoFromAssemblies()`.

**Cause**: `AutoFromAssemblies()` creates fixed mappings.

**Solution**: Use explicit `.Handlers()` registration:

```csharp
// Instead of AutoFromAssemblies
services.AddBrighter(options => { })
    .Handlers(registry =>
    {
        // Explicit registration for Agreement Dispatcher
        registry.Register<MyCommand>((request, context) => { /* ... */ }, [/* handlers */]);
    });
```

## Further Reading

- [Agreement Dispatcher Routing](/contents/AgreementDispatcherRouting.md) - Why route on content, five use cases, limits and overhead
- [Martin Fowler: Agreement Dispatcher](https://martinfowler.com/eaaDev/AgreementDispatcher.html) - Original pattern description
- [Dynamic Message Deserialization](DynamicMessageDeserialization.md) - Content-based type routing
- [Request Handlers](BuildingAPipeline.md) - Handler basics
- [Routing](Routing.md) - Standard routing in Brighter

## Agreement Dispatcher Sample Code

Full working examples can be found in the Brighter samples:

- **Agreement Dispatcher**: `Brighter/samples/WebAPI/` - Examples of dynamic handler selection
- **Multi-handler**: Various samples showing handler pipeline composition
