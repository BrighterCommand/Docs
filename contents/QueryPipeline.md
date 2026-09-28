---
description: "The Query Pipeline in Darker provides a powerful way to add cross-cutting concerns to your query handlers without modifying the handler code itself."
layout:
  description:
    visible: false
---

# Query Pipeline and Decorators

> **How-to** · Applies to **Darker V4**

## Query Pipeline Introduction

The Query Pipeline in Darker provides a powerful way to add cross-cutting concerns to your query handlers without modifying the handler code itself. Using decorators (also called middleware), you can add capabilities like logging, retry logic, circuit breakers, and fallback behavior to any query handler through simple attribute annotations.

Darker's pipeline uses the same **Russian Doll Model** as [Brighter](/contents/BuildingAPipeline.md), where each decorator in the pipeline encompasses the call to the next decorator or handler, allowing the chain to behave like a call stack. This architectural pattern enables you to compose complex behavior from simple, focused decorators that remain independent and testable.

This approach follows the [Decorator Pattern](https://en.wikipedia.org/wiki/Decorator_pattern) and allows you to separate cross-cutting concerns from your core query logic. For more information on pipelines and the Russian Doll Model, see [Basic Concepts](/contents/BasicConcepts.md#pipeline).

## How the Query Pipeline Works

### Pipeline Execution Flow

When you call `IQueryProcessor.ExecuteAsync(query)`, Darker constructs a pipeline of decorators around your query handler based on the attributes you've applied to the handler's `ExecuteAsync` method. The execution flows through each decorator in order before reaching your handler:

```text
QueryProcessor.ExecuteAsync(query)
        ↓
[QueryLogging Decorator - Step 1]
    Logs query details
        ↓
[FallbackPolicy Decorator - Step 2]
    Catches exceptions, provides fallback
        ↓
[RetryableQuery Decorator - Step 3]
    Retries on transient failures
    Circuit breaker protection
        ↓
[Target Query Handler]
    Your ExecuteAsync implementation
        ↓
    Result (flows back up through decorators)
        ↓
[RetryableQuery completes]
        ↓
[FallbackPolicy completes]
        ↓
[QueryLogging completes]
        ↓
Result returned to caller
```

Each decorator in the pipeline can:
- Execute logic before calling the next handler in the chain
- Execute logic after the next handler completes
- Transform the query or result
- Handle exceptions from downstream handlers
- Short-circuit the pipeline and return early

### Decorator Ordering

The order in which decorators execute is controlled by the **step number** specified in each attribute. Decorators execute in ascending order by step number:

```csharp
using Paramore.Darker;
using Paramore.Darker.Attributes;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPersonQueryHandler : QueryHandlerAsync<GetPersonNameQuery, string>
{
    [QueryLogging(1)]              // Executes FIRST (step 1)
    [FallbackPolicy(2)]            // Executes SECOND (step 2)
    [RetryableQuery(3)]            // Executes THIRD (step 3)
    public override async Task<string> ExecuteAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        // ... your query logic here
        // This executes LAST, after all decorators
        return string.Empty;
    }
}
```

**Why ordering matters:** the decorator with the lowest step is the outermost, and wraps everything with a higher step.

- **Logging first (step 1)** logs each call once: the query, then the total time it took across every retry, marked *(with fallback)* when the fallback supplied the result. It does not log individual retries, and a query that fails after its last retry logs only its *Executing* line
- **Fallback outside retry (step 2 before 3)**: the retry makes all its attempts, and the fallback runs only when they are exhausted
- **Retry innermost (step 3)**: each attempt re-runs only the handler

You can adjust the ordering to suit your needs, as long as fallback stays outside retry. Put retry at a lower step than fallback and the fallback handles the first exception, so nothing is retried. Put logging inside retry (`[RetryableQuery(1)]`, `[QueryLogging(2)]`) when you want an *Executing* line for every attempt; only the attempt that succeeds logs its completion.

## Available Decorators

### QueryLogging Decorator

The `QueryLogging` decorator provides JSON-based logging of query execution, including query parameters, execution time, and result summaries. This is invaluable for debugging, monitoring, and auditing query operations.

#### Configuration

First, add the query logging decorator to your Darker configuration in `Program.cs`:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.QueryLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddJsonQueryLogging();  // Add logging decorator

var app = builder.Build();
app.Run();
```

The `AddJsonQueryLogging()` method registers the logging decorator in the Darker pipeline, making it available for use in your query handlers.

#### Usage

Apply the `[QueryLogging]` attribute to your query handler's `ExecuteAsync` method with a step number:

```csharp
using Paramore.Darker;
using Paramore.Darker.QueryLogging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPeopleQueryHandler : QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>
{
    private readonly IPersonRepository _repository;

    public GetPeopleQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]  // Execute logging as the first decorator
    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetPeopleQuery query,
        CancellationToken cancellationToken = default)
    {
        var people = await _repository.GetAllAsync(cancellationToken);
        return people;
    }
}
```

#### What Gets Logged

The QueryLogging decorator logs:

- **Query type**: The full type name of the query being executed
- **Query parameters**: JSON serialization of the query object and its properties
- **Execution time**: The time taken to execute the query
- **Result summary**: A summary of the result (typically type and count for collections)
- **Timestamp**: When the query was executed

This information is written to your application's configured logging output (console, file, Application Insights, etc.) at the Information level by default.

### Policy Decorators (Resilience)

Darker integrates with [Polly](https://github.com/App-vNext/Polly) to provide resilience and transient fault handling through policy decorators. These decorators allow you to add retry logic, circuit breakers, and fallback behavior to your query handlers.

#### RetryableQuery Decorator

The `RetryableQuery` decorator runs a query inside a Polly policy from your policy registry: by default the retry policy that `AddDefaultPolicies()` registers, so a query that hits a transient failure is tried again. Name a different policy, such as a circuit breaker, and it runs that policy instead.

**Purpose:**

- Retry queries that fail due to transient errors (network issues, temporary unavailability)
- Protect downstream services with circuit breaker patterns
- Improve application resilience without changing query handler code

**Use Cases:**

- Querying external HTTP APIs that may have intermittent connectivity issues
- Database queries that may experience transient connection failures
- Any query operation that interacts with unreliable external dependencies

**Configuration:**

First, add policies to your Darker configuration:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddDefaultPolicies();  // Adds default retry and circuit breaker policies

var app = builder.Build();
app.Run();
```

**Usage:**

Apply the `[RetryableQuery]` attribute with a step number, and optionally the name of the policy to run:

```csharp
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPeopleQueryHandler : QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>
{
    private readonly IPersonRepository _repository;

    public GetPeopleQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]
    [RetryableQuery(2)]            // Retry with the default retry policy
    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetPeopleQuery query,
        CancellationToken cancellationToken = default)
    {
        var people = await _repository.GetAllAsync(cancellationToken);
        return people;
    }
}
```

The `RetryableQuery` attribute takes two parameters:

- **Step number**: Controls when this decorator executes in the pipeline (typically after logging and fallback)
- **Policy name** (optional): The name of the policy in your policy registry that the decorator runs. It defaults to `Constants.RetryPolicyName`, the retry policy `AddDefaultPolicies()` registers

The decorator runs that one policy. With the default, a query that fails is executed again, up to the retries your policy allows (see [Configuring Polly Policies](/contents/QueryPipelinePolicies.md)). Naming a circuit breaker runs the breaker *instead of* the retry, not as well; to retry and break, register a policy that wraps both and name that.

#### FallbackPolicy Decorator

The `FallbackPolicy` decorator provides a way to return a default or degraded result when a query fails, rather than propagating the exception to the caller. This is essential for providing graceful degradation in user-facing applications.

**Purpose:**

- Provide default values when queries fail
- Enable degraded service modes
- Improve user experience by avoiding error messages

**Use Cases:**

- Returning cached or default data when a primary data source is unavailable
- Providing placeholder content when real-time data cannot be retrieved
- Implementing graceful degradation for non-critical queries

**Configuration:**

The FallbackPolicy decorator is available when you add policies to Darker:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddDefaultPolicies();  // Includes fallback support

var app = builder.Build();
app.Run();
```

**Usage:**

Apply the `[FallbackPolicy]` attribute and implement a `FallbackAsync` method in your handler:

```csharp
using Paramore.Darker;
using Paramore.Darker.Attributes;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPersonQueryHandler : QueryHandlerAsync<GetPersonNameQuery, string>
{
    private readonly IPersonRepository _repository;

    public GetPersonQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]
    [FallbackPolicy(2)]  // Provide fallback if query fails
    [RetryableQuery(3)]
    public override async Task<string> ExecuteAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var name = await _repository.GetNameByIdAsync(query.PersonId, cancellationToken);
        return name;
    }

    // Fallback method - called when ExecuteAsync throws an exception
    public override Task<string> FallbackAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        // Return a default value
        return Task.FromResult("Unknown");
    }
}
```

**Important:** When you use the `[FallbackPolicy]` attribute, you **must** implement the `FallbackAsync` method with the same signature (except method name) as `ExecuteAsync`. The fallback method is called when:

- The primary `ExecuteAsync` method throws an exception
- All retries have been exhausted (if using `RetryableQuery`)
- The circuit breaker is open

The fallback method should:

- Return a sensible default value
- Execute quickly (no expensive operations)
- Not throw exceptions (wrap any operations in try-catch)
- Be deterministic and predictable

#### Circuit Breaker Integration

A Polly circuit breaker is a policy like any other: register it under a name, and pass that name to `RetryableQuery`. A circuit breaker prevents your application from repeatedly attempting operations that are likely to fail, giving failing systems time to recover.

**Circuit Breaker States:**

- **Closed**: Normal operation, requests flow through
- **Open**: Too many failures occurred, requests are blocked immediately
- **Half-Open**: Testing if the system has recovered, allowing limited requests

**How it works:**

1. The circuit breaker tracks failures
2. After a threshold of consecutive failures, the circuit "opens"
3. While open, requests fail immediately without attempting the operation
4. After a timeout period, the circuit enters "half-open" state
5. A successful request closes the circuit; a failure reopens it

**Usage:**

The `RetryableQuery` attribute runs only the policy it names, so naming a circuit breaker on its own gives you a breaker with no retry. To retry and break, name a policy that wraps a retry around a breaker, as [Configuring Polly Policies](/contents/QueryPipelinePolicies.md#advanced-query-policy-configurations) shows:

```csharp
using System.Threading;
using System.Threading.Tasks;
using Paramore.Darker.Policies;

[RetryableQuery(2, "ExternalApiRetryAndBreak")]
public override async Task<OrderData> ExecuteAsync(
    GetOrderQuery query,
    CancellationToken cancellationToken = default)
{
    // Query external API
}
```

You can register a different policy for each external dependency, and name it on that dependency's handlers. See [Configuring Polly Policies](/contents/QueryPipelinePolicies.md) for how to define them.

### Custom Decorators

Darker's decorator system is extensible, allowing you to create custom decorators for your specific cross-cutting concerns. However, the mechanism for creating custom decorators is not prominently documented in the core Darker library.

Based on the Darker architecture, custom decorators would need to:

- Implement the decorator pattern around `IQueryHandler<TQuery, TResult>`
- Integrate with the Darker pipeline registration
- Support attribute-based configuration with step ordering

**Note:** If you need custom cross-cutting behavior not provided by the built-in decorators, consider:

1. **Using Polly policies**: Many custom behaviors can be implemented as Polly policies (timeout, rate limiting, caching, etc.)
2. **Wrapping the IQueryProcessor**: For application-wide concerns, you can create a wrapper around `IQueryProcessor`
3. **Contributing to Darker**: If you develop a useful custom decorator pattern, consider contributing it back to the Darker project

For most scenarios, the combination of QueryLogging, RetryableQuery, and FallbackPolicy decorators with custom Polly policies provides sufficient flexibility.

## Decorator Patterns

### Pattern: Logging + Retry

This is the most common pattern for query handlers that interact with external dependencies. Logging records each query and how long it took, retries included, while the retry policy handles transient failures:

```csharp
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPeopleQueryHandler : QueryHandlerAsync<GetPeopleQuery, IReadOnlyDictionary<int, string>>
{
    private readonly IPersonRepository _repository;

    public GetPeopleQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]              // Log each call once, timed across its retries
    [RetryableQuery(2)]            // Retry on transient failures
    public override async Task<IReadOnlyDictionary<int, string>> ExecuteAsync(
        GetPeopleQuery query,
        CancellationToken cancellationToken = default)
    {
        var people = await _repository.GetAllAsync(cancellationToken);
        return people;
    }
}
```

**When to use:**

- Queries that access databases or external APIs
- Any query that may experience transient failures
- Production queries where you need observability

**Benefits:**

- Visibility into every query call and its total duration
- Automatic recovery from transient failures
- Circuit breaker protection as well, if you name a policy that wraps a breaker inside the retry

### Pattern: Logging + Fallback + Retry

This pattern adds fallback behavior to provide graceful degradation when all retries are exhausted. This is especially valuable for user-facing queries where you want to avoid showing error messages:

```csharp
using Paramore.Darker;
using Paramore.Darker.Attributes;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPersonQueryHandler : QueryHandlerAsync<GetPersonNameQuery, string>
{
    private readonly IPersonRepository _repository;

    public GetPersonQueryHandler(IPersonRepository repository)
    {
        _repository = repository;
    }

    [QueryLogging(1)]              // Log everything
    [FallbackPolicy(2)]            // Provide fallback if needed
    [RetryableQuery(3)]            // Retry before falling back
    public override async Task<string> ExecuteAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        var name = await _repository.GetNameByIdAsync(query.PersonId, cancellationToken);
        return name;
    }

    public override Task<string> FallbackAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        // Return a friendly default when the query fails
        return Task.FromResult("Unknown Person");
    }
}
```

**Execution flow:**

1. QueryLogging logs the query attempt
2. FallbackPolicy wraps the inner execution
3. RetryableQuery attempts the query, retrying on failure
4. If all retries fail, FallbackPolicy catches the exception and calls `FallbackAsync`
5. QueryLogging logs the final result (either from successful query or fallback)

**When to use:**

- User-facing queries where errors should be handled gracefully
- Queries where a default value is acceptable when the primary source fails
- Critical paths where you want to maintain service even during partial failures

**Benefits:**

- User experience remains smooth even during failures
- Retries happen first, fallback is last resort
- Complete logging of the entire flow

### Pattern: Multiple Circuit Breakers for Different Dependencies

When your query handler interacts with multiple external systems, you can apply different circuit breakers to different failure scenarios. Each is a policy you register under its own name; the decorator runs only that policy, so a breaker named here does not also retry unless you register it wrapped in a retry:

```csharp
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCustomerOrderSummaryQueryHandler :
    QueryHandlerAsync<GetCustomerOrderSummaryQuery, CustomerOrderSummary>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrderSummaryQueryHandler(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }

    [QueryLogging(1)]
    [RetryableQuery(2, "CustomerDatabaseCircuitBreaker")]
    public override async Task<CustomerOrderSummary> ExecuteAsync(
        GetCustomerOrderSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        // This entire method is protected by CustomerDatabaseCircuitBreaker
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);
        var orders = await _orderRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        return new CustomerOrderSummary
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            OrderCount = orders.Count,
            TotalValue = orders.Sum(o => o.Total)
        };
    }
}
```

For more fine-grained control, you might create separate query handlers for each external dependency, each with its own circuit breaker, and compose them at a higher level.

**When to use:**

- Handlers that query multiple external systems
- When different dependencies have different reliability characteristics
- When you want to isolate failures to specific systems

## Query Pipeline Best Practices

**1. Order decorators logically**

Place logging first (step 1) so every call is logged once, timed across its retries and marked when a fallback answered it:
```csharp
// ...
[QueryLogging(1)]
[FallbackPolicy(2)]
[RetryableQuery(3)]
```

**2. Use circuit breakers for external dependencies**

Whenever your query interacts with external systems (databases, APIs, microservices), protect them with circuit breakers to prevent cascading failures and give failing systems time to recover.

**3. Implement fallbacks for user-facing queries**

For queries that directly serve user requests, implement fallback logic to provide graceful degradation rather than error messages:
```csharp
[FallbackPolicy(2)]
public override async Task<Result> ExecuteAsync(Query query, ...)
{
    // primary logic
}

public override Task<Result> FallbackAsync(Query query, ...)
{
    return Task.FromResult(GetDefaultValue());
}
```

**4. Keep decorators focused and composable**

Each decorator should have a single responsibility. Compose multiple simple decorators rather than creating complex custom decorators.

**5. Use named policies for different dependencies**

Register a separate circuit breaker for each external dependency, and name it on that dependency's handlers:
```csharp
[RetryableQuery(2, "DatabaseCircuitBreaker")]    // For database queries
[RetryableQuery(2, "ExternalApiCircuitBreaker")] // For API queries
```

**6. Configure appropriate retry delays**

Use exponential backoff for retries to avoid overwhelming recovering systems:
```csharp
TimeSpan.FromMilliseconds(50),   // 50ms
TimeSpan.FromMilliseconds(100),  // 100ms
TimeSpan.FromMilliseconds(200),  // 200ms
```

**7. Monitor circuit breaker state changes**

Use Polly's callback methods to log or alert when circuit breakers open or close, as these indicate systemic issues.

**8. Test your fallback logic**

Ensure your `FallbackAsync` methods are tested and return appropriate default values. Fallback logic should be simple and not throw exceptions.

## Query Pipeline Common Pitfalls

**1. Wrong decorator ordering**

Putting retry at a lower step than fallback puts the retry outside the fallback: the fallback handles the first exception, and nothing is retried.

❌ Bad:
```csharp
// ...
[RetryableQuery(1)]
[FallbackPolicy(2)]  // Catches the first failure, so the retry never runs
```

✅ Good:
```csharp
// ...
[FallbackPolicy(1)]  // Runs only once the retries are exhausted
[RetryableQuery(2)]
```

**2. Forgetting to configure policies**

Using `[RetryableQuery]` without calling `AddDefaultPolicies()` or `AddPolicies()` will cause runtime errors.

❌ Bad:
```csharp
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly);
    // No policies configured!
```

✅ Good:
```csharp
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddDefaultPolicies();
```

**3. Policy naming mismatches**

Naming a policy that doesn't exist in the policy registry throws a `ConfigurationException`, *"Policy does not exist in policy registry"*, the first time the handler runs.

❌ Bad:
```csharp
[RetryableQuery(2, "MyCircuitBreaker")]  // Policy name doesn't exist
```

✅ Good:
```csharp
// Define policy
policyRegistry.Add("MyCircuitBreaker", circuitBreakerPolicy);

// Reference it
[RetryableQuery(2, "MyCircuitBreaker")]
```

**4. Fallback not implemented when using FallbackPolicy**

If you use the `[FallbackPolicy]` attribute, you **must** implement the `FallbackAsync` method, or you'll get a runtime error.

❌ Bad:
```csharp
[FallbackPolicy(2)]
public override async Task<Result> ExecuteAsync(Query query, ...)
{
    // ...
}
// Missing FallbackAsync method!
```

✅ Good:
```csharp
[FallbackPolicy(2)]
public override async Task<Result> ExecuteAsync(Query query, ...)
{
    // ...
}

public override Task<Result> FallbackAsync(Query query, ...)
{
    return Task.FromResult(defaultValue);
}
```

**5. Expensive operations in fallback logic**

Fallback methods should be fast and not call external services. The purpose is to provide a quick default, not to attempt alternative implementations.

❌ Bad:
```csharp
public override async Task<string> FallbackAsync(Query query, ...)
{
    // Calling another external service in fallback!
    return await _alternativeService.GetDataAsync();
}
```

✅ Good:
```csharp
public override Task<string> FallbackAsync(Query query, ...)
{
    // Return a simple default value
    return Task.FromResult("Default Value");
}
```

**6. Not handling CancellationToken in decorators**

Always pass the `CancellationToken` through the pipeline to allow graceful cancellation of long-running queries.

## Further Reading

- [Query Pipeline Policies](/contents/QueryPipelinePolicies.md) - Configuring the Polly policies decorators use
- [Darker and Brighter Pipelines](/contents/DarkerAndBrighterPipelines.md) - Where the two pipelines agree and differ
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Learn how to implement query handlers that use decorators
- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - Configure Darker with policies and decorators
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) - Brighter's equivalent pipeline documentation
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) - Detailed guide to Polly policies in Brighter (patterns apply to Darker)
- [Polly Documentation](https://github.com/App-vNext/Polly) - Comprehensive guide to Polly resilience policies

Working examples can be found in the Darker samples: `Darker/samples/SampleMinimalApi/QueryHandlers/`
