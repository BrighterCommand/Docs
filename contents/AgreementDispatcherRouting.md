# Agreement Dispatcher Routing

> **Explanation** · Applies to **Brighter V10** · Prerequisites: [Agreement Dispatcher](/contents/AgreementDispatcher.md)

Why you would route a request by its content rather than its type, what that buys you, and
what it costs. Five worked scenarios, the two things the pattern cannot do, and the
measured overhead. [Agreement Dispatcher](/contents/AgreementDispatcher.md) is the page that
shows you how to register one.

## Standard vs Agreement Dispatcher Routing

### Standard 1-to-1 Routing (Default)

In standard Brighter routing, each request type maps to exactly one handler type at compile-time:

```csharp
// ...
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
// ...
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
// ...
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
// ...
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
// ...
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
// ...
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
// ...
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

## Agreement Dispatcher Limitations

### Cannot Use AutoFromAssemblies

Agreement Dispatcher requires explicit handler registration:

```csharp
// ...
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
// ...
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
// ...
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

## Agreement Dispatcher Performance Implications

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
// ...
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

## Further Reading

- [Agreement Dispatcher](/contents/AgreementDispatcher.md) - Registration syntax and a complete example
- [Martin Fowler: Agreement Dispatcher](https://martinfowler.com/eaaDev/AgreementDispatcher.html) - Original pattern description
- [Dynamic Message Deserialization](/contents/DynamicMessageDeserialization.md) - Content-based type routing
- [Routing](/contents/Routing.md) - Standard routing in Brighter
