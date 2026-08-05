# Agreement Dispatcher

> **How-to** · Applies to **Brighter V10**

## Agreement Dispatcher Overview

The **Agreement Dispatcher** is a pattern for routing requests to handlers dynamically based on the request's content or context, rather than using a fixed type-to-handler mapping. This pattern, described by Martin Fowler in [Patterns of Enterprise Application Architecture](https://martinfowler.com/eaaDev/AgreementDispatcher.html), enables flexible routing logic that can change based on business rules, time, geography, or other runtime conditions.

Brighter supports an Agreement Dispatcher, allowing you to register a lambda function that determines which handler(s) should process a request at runtime.

## Standard vs Agreement Dispatcher Routing

### Standard 1-to-1 Routing (Default)

In standard Brighter routing, each request type maps to exactly one handler type at compile-time:

```csharp
services.AddBrighter(options => { })
    .Handlers(registry =>
    {
        // Fixed mapping: MyCommand always goes to MyCommandHandler
        registry.Register<MyCommand, MyCommandHandler>();
    });
```

**Characteristics:**

- Simple and predictable
- Type-safe at compile-time
- Fast (no runtime lookup)
- Works with `AutoFromAssemblies()`
- Cannot change routing based on request content
- Cannot route to different handlers over time

**When to use standard routing:**

- Handler selection doesn't depend on request content
- One handler per command/event is sufficient
- Simple, straightforward scenarios

This is Brighter's default and recommended approach for most scenarios.

### Agreement Dispatcher Routing

Agreement Dispatcher allows dynamic handler selection based on request content or context:

```csharp
services.AddBrighter(options => { })
    .Handlers(registry =>
    {
        // Dynamic mapping: handler chosen at runtime
        registry.Register<MyCommand>((request, context) =>
        {
            var command = request as MyCommand;
            if (command?.Priority == "High")
                return [typeof(HighPriorityHandler)];

            return [typeof(StandardHandler)];
        },
        [
            typeof(HighPriorityHandler),
            typeof(StandardHandler)
        ]);
    });
```

**Characteristics:**

- Flexible routing based on content
- Can change behavior over time
- Supports multiple handlers
- Access to request context
- Cannot use `AutoFromAssemblies()`
- Must register handlers explicitly
- Small performance overhead (lambda execution)

**When to use Agreement Dispatcher:**

- Handler selection depends on request content
- Business rules change over time
- Geography or customer-specific routing
- A/B testing or feature flags
- Multi-tenant routing

## Agreement Dispatcher Use Cases

### 1. Time-Based Routing

Route to different handlers as business rules evolve over time:

```csharp
registry.Register<ProcessOrder>((request, context) =>
{
    var order = request as ProcessOrder;
    var orderDate = order?.OrderDate ?? DateTime.UtcNow;

    // Before Jan 2025: Use old tax rules
    if (orderDate < new DateTime(2025, 1, 1))
        return [typeof(LegacyTaxOrderHandler)];

    // After Jan 2025: Use new tax rules
    return [typeof(ModernTaxOrderHandler)];
},
[
    typeof(LegacyTaxOrderHandler),
    typeof(ModernTaxOrderHandler)
]);
```

**Scenario**: Tax regulations change, but you need to process old orders with old rules and new orders with new rules.

### 2. Country-Specific Business Logic

Route based on geographical requirements:

```csharp
registry.Register<ProcessPayment>((request, context) =>
{
    var payment = request as ProcessPayment;

    return payment?.Country switch
    {
        "US" => [typeof(USPaymentHandler)],
        "UK" => [typeof(UKPaymentHandler)],
        "EU" => [typeof(EUPaymentHandler)],
        "JP" => [typeof(JapanPaymentHandler)],
        _ => [typeof(InternationalPaymentHandler)]
    };
},
[
    typeof(USPaymentHandler),
    typeof(UKPaymentHandler),
    typeof(EUPaymentHandler),
    typeof(JapanPaymentHandler),
    typeof(InternationalPaymentHandler)
]);
```

**Scenario**: Payment processing varies significantly by country (regulations, currencies, payment methods).

### 3. Order Journey Based on Contents

Different order types require different processing workflows:

```csharp
registry.Register<ProcessOrder>((request, context) =>
{
    var order = request as ProcessOrder;

    // Digital orders: instant fulfillment
    if (order?.Type == OrderType.Digital)
        return [typeof(DigitalOrderHandler)];

    // Pre-orders: different workflow
    if (order?.IsPreOrder == true)
        return [typeof(PreOrderHandler)];

    // Hazardous materials: special handling
    if (order?.ContainsHazardousMaterials == true)
        return [typeof(HazmatOrderHandler)];

    // Standard physical orders
    return [typeof(StandardOrderHandler)];
},
[
    typeof(DigitalOrderHandler),
    typeof(PreOrderHandler),
    typeof(HazmatOrderHandler),
    typeof(StandardOrderHandler)
]);
```

**Scenario**: Orders follow different workflows based on their characteristics.

### 4. Versioning and Migration

Support multiple API versions simultaneously:

```csharp
registry.RegisterAsync<CreateUser>((request, context) =>
{
    var createUser = request as CreateUser;

    // Route based on API version in request
    return createUser?.ApiVersion switch
    {
        "v1" => [typeof(CreateUserV1HandlerAsync)],
        "v2" => [typeof(CreateUserV2HandlerAsync)],
        "v3" => [typeof(CreateUserV3HandlerAsync)],
        _ => [typeof(CreateUserLatestHandlerAsync)]
    };
},
[
    typeof(CreateUserV1HandlerAsync),
    typeof(CreateUserV2HandlerAsync),
    typeof(CreateUserV3HandlerAsync),
    typeof(CreateUserLatestHandlerAsync)
]);
```

**Scenario**: Maintain backward compatibility while rolling out new API versions.

### 5. State-Based Routing

Route based on current state or status:

```csharp
registry.Register<ProcessRefund>((request, context) =>
{
    var refund = request as ProcessRefund;

    return refund?.OrderStatus switch
    {
        OrderStatus.Pending => [typeof(CancelOrderRefundHandler)],
        OrderStatus.Shipped => [typeof(ReturnAndRefundHandler)],
        OrderStatus.Delivered => [typeof(FullRefundHandler)],
        OrderStatus.PartiallyReturned => [typeof(PartialRefundHandler)],
        _ => throw new InvalidOperationException($"Cannot refund order in status: {refund?.OrderStatus}")
    };
},
[
    typeof(CancelOrderRefundHandler),
    typeof(ReturnAndRefundHandler),
    typeof(FullRefundHandler),
    typeof(PartialRefundHandler)
]);
```

**Scenario**: Refund processing varies based on order state.

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

## Agreement Dispatcher Limitations

### Cannot Use AutoFromAssemblies

Agreement Dispatcher requires explicit handler registration:

```csharp
// Cannot use AutoFromAssemblies with Agreement Dispatcher
services.AddBrighter(options => { })
    .Handlers(registry =>
    {
        registry.Register<MyCommand>((request, context) => { /* ... */ },
            [typeof(Handler1), typeof(Handler2)]);
    })
    // .AutoFromAssemblies() won't work with Agreement Dispatcher
```

**Why?** `AutoFromAssemblies()` creates fixed 1-to-1 mappings. Agreement Dispatcher needs explicit lambda registration and handler type lists for DI.

**Solution**: Use `.Handlers()` to register Agreement Dispatcher routes explicitly:

```csharp
services.AddBrighter(options => { })
    .Handlers(registry =>
    {
        // Agreement dispatcher routes
        registry.Register<MyCommand>((request, context) => { /* ... */ },
            [typeof(Handler1), typeof(Handler2)]);

        // You can still mix with standard routes
        registry.Register<OtherCommand, OtherCommandHandler>();
    });
```

### Handler Types Must Be Provided

You must provide all possible handler types for DI registration:

```csharp
registry.Register<MyCommand>((request, context) =>
{
    // Your routing logic...
},
[
    // All handlers that might be returned must be listed here
    typeof(Handler1),
    typeof(Handler2),
    typeof(Handler3)
]);
```

**Why?** Brighter registers these handler types with the DI container so they can be resolved at runtime.

## Performance Implications

Agreement Dispatcher has a small performance overhead compared to standard routing:

### Overhead Breakdown

**Standard Routing:**

- Handler type lookup: Dictionary lookup (~O(1))
- No lambda execution

**Agreement Dispatcher:**

- Lambda execution
- Handler type lookup: Dictionary lookup (~O(1))

### Performance Considerations

For most applications, this overhead is negligible:

- **Acceptable**: Web APIs, message processing, background jobs
- **Acceptable**: 99.9% of scenarios
- **Consider carefully**: Ultra-low latency systems (microsecond SLAs)
- **Consider carefully**: Millions of messages per second

**Optimization tip**: Keep routing lambdas simple. Avoid expensive operations like database calls or external API calls.

```csharp
// Good - Simple, fast routing logic
registry.Register<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    return cmd?.Type == "Fast" ? [typeof(FastHandler)] : [typeof(SlowHandler)];
},
[typeof(FastHandler), typeof(SlowHandler)]);

// Bad - Expensive operation in routing lambda
registry.Register<MyCommand>((request, context) =>
{
    var cmd = request as MyCommand;
    // DON'T DO THIS: Database call in routing lambda!
    var config = _database.GetConfig(cmd.Id);  // Expensive!
    return config.UseFastPath ? [typeof(FastHandler)] : [typeof(SlowHandler)];
},
[typeof(FastHandler), typeof(SlowHandler)]);
```

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

- [Martin Fowler: Agreement Dispatcher](https://martinfowler.com/eaaDev/AgreementDispatcher.html) - Original pattern description
- [Dynamic Message Deserialization](DynamicMessageDeserialization.md) - Content-based type routing
- [Request Handlers](BuildingAPipeline.md) - Handler basics
- [Routing](Routing.md) - Standard routing in Brighter

## Agreement Dispatcher Sample Code

Full working examples can be found in the Brighter samples:

- **Agreement Dispatcher**: `Brighter/samples/WebAPI/` - Examples of dynamic handler selection
- **Multi-handler**: Various samples showing handler pipeline composition
