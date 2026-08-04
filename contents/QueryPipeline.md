# Query Pipeline and Decorators

> **How-to** · Applies to **Darker V4**

## Query Pipeline Introduction

The Query Pipeline in Darker provides a powerful way to add cross-cutting concerns to your query handlers without modifying the handler code itself. Using decorators (also called middleware), you can add capabilities like logging, retry logic, circuit breakers, and fallback behavior to any query handler through simple attribute annotations.

Darker's pipeline uses the same **Russian Doll Model** as [Brighter](/contents/BuildingAPipeline.md), where each decorator in the pipeline encompasses the call to the next decorator or handler, allowing the chain to behave like a call stack. This architectural pattern enables you to compose complex behavior from simple, focused decorators that remain independent and testable.

This approach follows the [Decorator Pattern](https://en.wikipedia.org/wiki/Decorator_pattern) and allows you to separate cross-cutting concerns from your core query logic. For more information on pipelines and the Russian Doll Model, see [Basic Concepts](/contents/BasicConcepts.md#pipeline).

## How the Query Pipeline Works

### Pipeline Execution Flow

When you call `IQueryProcessor.ExecuteAsync(query)`, Darker constructs a pipeline of decorators around your query handler based on the attributes you've applied to the handler's `ExecuteAsync` method. The execution flows through each decorator in order before reaching your handler:

```
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
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetPersonQueryHandler : QueryHandlerAsync<GetPersonNameQuery, string>
{
    [QueryLogging(1)]              // Executes FIRST (step 1)
    [FallbackPolicy(2)]            // Executes SECOND (step 2)
    [RetryableQuery(3, "MyCircuitBreaker")]  // Executes THIRD (step 3)
    public override async Task<string> ExecuteAsync(
        GetPersonNameQuery query,
        CancellationToken cancellationToken = default)
    {
        // Your query logic here
        // This executes LAST, after all decorators
    }
}
```

**Why ordering matters:**

- **Logging should typically be first (step 1)**: This ensures all operations are logged, including retries and fallbacks
- **Fallback before retry (step 2 before 3)**: If a retry exhausts its attempts, the fallback can provide a default result
- **Retry with circuit breaker should be last (step 3)**: This ensures retries happen after other decorators have a chance to handle the request

However, you can adjust the ordering to suit your specific needs. For example, you might want retry before fallback if you only want to use the fallback when all retries are exhausted.

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

The `RetryableQuery` decorator automatically retries a query when it encounters transient failures. It integrates with Polly circuit breakers to prevent overwhelming failing systems.

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

Apply the `[RetryableQuery]` attribute with a step number and the name of a circuit breaker policy:

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
    [RetryableQuery(2, "DefaultCircuitBreaker")]  // Retry with circuit breaker
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
- **Circuit breaker name**: The name of a circuit breaker policy in your policy registry

When a query fails, the retry policy will attempt to execute it again based on your policy configuration (see [Configuring Polly Policies](#configuring-polly-policies)). If failures continue, the circuit breaker will open, preventing further attempts until the circuit closes again.

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
    [RetryableQuery(3, "DefaultCircuitBreaker")]
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

Both `RetryableQuery` and custom policies can integrate with Polly circuit breakers. A circuit breaker prevents your application from repeatedly attempting operations that are likely to fail, giving failing systems time to recover.

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

Circuit breakers are specified by name in the `RetryableQuery` attribute:

```csharp
[RetryableQuery(2, "ExternalApiCircuitBreaker")]
public override async Task<OrderData> ExecuteAsync(
    GetOrderQuery query,
    CancellationToken cancellationToken = default)
{
    // Query external API
}
```

You can use different circuit breakers for different types of failures or different external dependencies. See [Configuring Polly Policies](#configuring-polly-policies) for how to define circuit breakers.

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

This is the most common pattern for query handlers that interact with external dependencies. Logging provides visibility into query execution and retries, while the retry policy handles transient failures:

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

    [QueryLogging(1)]              // Log all executions, including retries
    [RetryableQuery(2, "DefaultCircuitBreaker")]  // Retry on transient failures
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

- Complete visibility into query execution, including retries
- Automatic recovery from transient failures
- Circuit breaker protection against cascading failures

### Pattern: Logging + Fallback + Retry

This pattern adds fallback behavior to provide graceful degradation when all retries are exhausted. This is especially valuable for user-facing queries where you want to avoid showing error messages:

```csharp
using Paramore.Darker;
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
    [RetryableQuery(3, "DefaultCircuitBreaker")]  // Retry before falling back
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

When your query handler interacts with multiple external systems, you can apply different circuit breakers to different failure scenarios:

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

## Configuring Polly Policies

Darker's policy decorators are powered by [Polly](https://github.com/App-vNext/Polly), a .NET resilience and transient-fault-handling library. You can configure policies to control retry behavior, circuit breaker thresholds, and timeouts.

### Default Policies

The simplest way to add policies is to use `AddDefaultPolicies()`:

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

The default policies provide:

- **Default retry policy**: Retries with exponential backoff
- **Default circuit breaker**: Opens after consecutive failures, closes after a timeout period

These policies are sufficient for many applications and provide a good starting point for resilience.

### Custom Policy Registry

For more control over resilience policies, you can create a custom policy registry with specific retry strategies, circuit breakers, and timeout policies:

```csharp
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Policies;
using Polly;
using Polly.Registry;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly)
    .AddPolicies(ConfigurePolicies());

var app = builder.Build();
app.Run();

static IPolicyRegistry<string> ConfigurePolicies()
{
    // Retry policy with exponential backoff
    var defaultRetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromMilliseconds(50),   // First retry after 50ms
            TimeSpan.FromMilliseconds(100),  // Second retry after 100ms
            TimeSpan.FromMilliseconds(150)   // Third retry after 150ms
        });

    // Circuit breaker that opens after 1 failure, stays open for 500ms
    var defaultCircuitBreaker = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 1,
            durationOfBreak: TimeSpan.FromMilliseconds(500));

    // Specific circuit breaker for critical operations
    var criticalCircuitBreaker = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 3,  // More tolerant
            durationOfBreak: TimeSpan.FromSeconds(30));  // Longer break

    // Register policies with names
    var policyRegistry = new PolicyRegistry
    {
        { Constants.RetryPolicyName, defaultRetryPolicy },
        { Constants.CircuitBreakerPolicyName, defaultCircuitBreaker },
        { "CriticalCircuitBreaker", criticalCircuitBreaker }
    };

    return policyRegistry;
}
```

### Policy Naming Convention

Darker provides constants for common policy names in the `Paramore.Darker.Policies.Constants` class:

- `Constants.RetryPolicyName` - Default retry policy name
- `Constants.CircuitBreakerPolicyName` - Default circuit breaker policy name

**Best practices:**

- Use the provided constants for default policies
- Use descriptive names for custom circuit breakers (e.g., "ExternalApiCircuitBreaker", "DatabaseCircuitBreaker")
- Document your policy names in a central configuration class
- Consider creating a constants class for policy names used across your application:

```csharp
public static class QueryPolicies
{
    public const string DatabaseCircuitBreaker = "DatabaseCircuitBreaker";
    public const string ExternalApiCircuitBreaker = "ExternalApiCircuitBreaker";
    public const string CacheCircuitBreaker = "CacheCircuitBreaker";
}
```

### Advanced Policy Configurations

Polly supports many advanced resilience patterns:

**Handle specific exceptions:**
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

**Retry with callback:**
```csharp
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(
        new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200) },
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            // Log retry attempts
            Console.WriteLine($"Retry {retryCount} after {timeSpan}");
        });
```

**Circuit breaker with callbacks:**
```csharp
var circuitBreaker = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (exception, duration) =>
        {
            // Log when circuit opens
            Console.WriteLine($"Circuit breaker opened for {duration}");
        },
        onReset: () =>
        {
            // Log when circuit closes
            Console.WriteLine("Circuit breaker reset");
        });
```

For more information on Polly policies, see the [Polly documentation](https://github.com/App-vNext/Polly) and the Brighter documentation on [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md).

## Comparison with Brighter Pipeline

Darker's query pipeline shares many similarities with [Brighter's request pipeline](/contents/BuildingAPipeline.md), as both implement the same Russian Doll Model for middleware composition. However, there are some key differences to be aware of when working with both frameworks.

### Similarities

**Russian Doll Model:**
Both frameworks use the same pipeline architecture where each handler/decorator wraps the next one in the chain, allowing cross-cutting concerns to execute before and after the core handler logic.

**Attribute-Based Ordering:**
Both use attributes with step numbers to control decorator execution order:
```csharp
// Brighter
[RequestLogging(1)]
[UsePolicy("RetryPolicy", 2)]
public override Task<AddGreetingResponse> HandleAsync(AddGreetingCommand command, ...)

// Darker
[QueryLogging(1)]
[RetryableQuery(2, "DefaultCircuitBreaker")]
public override Task<string> ExecuteAsync(GetPersonNameQuery query, ...)
```

**Policy Integration:**
Both integrate with Polly for resilience policies (retry, circuit breaker, timeout).

**Extensible Architecture:**
Both support custom decorators for application-specific cross-cutting concerns.

### Differences

**Return Values:**

- **Darker**: Query handlers return results (`TResult`), and the pipeline preserves and returns these results
- **Brighter**: Command handlers typically return `void` or the command itself; events are used to signal results

**External Bus Support:**

- **Brighter**: Supports external messaging through service activators, message mappers, and external bus integration
- **Darker**: Focuses on in-process query handling only; no external messaging support

**Decorator Focus:**

- **Darker**: Decorators are query-specific (QueryLogging, RetryableQuery, FallbackPolicy)
- **Brighter**: Decorators are request-specific (RequestLogging, UsePolicy, UseInbox, UseOutbox)

**Use Cases:**

- **Darker**: Read-side operations, queries that don't change state
- **Brighter**: Write-side operations, commands that change state, event publishing

When using both Brighter and Darker together in a CQRS architecture, you'll apply similar patterns but with framework-specific decorators. For more information on using both frameworks together, see [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md).

## Query Pipeline Best Practices

**1. Order decorators logically**

Place logging first (step 1) so all operations are logged, including retries and fallbacks:
```csharp
[QueryLogging(1)]
[FallbackPolicy(2)]
[RetryableQuery(3, "CircuitBreaker")]
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

**5. Use named circuit breakers for different failure types**

Create separate circuit breakers for different external dependencies or failure scenarios:
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

Putting retry before logging means individual retry attempts won't be logged. Putting fallback before retry means the fallback will be used before retries are exhausted.

❌ Bad:
```csharp
[RetryableQuery(1, "CB")]
[QueryLogging(2)]  // Won't log individual retries
```

✅ Good:
```csharp
[QueryLogging(1)]  // Logs everything including retries
[RetryableQuery(2, "CB")]
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

**3. Circuit breaker naming mismatches**

Referencing a circuit breaker name that doesn't exist in the policy registry will cause runtime errors.

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

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Learn how to implement query handlers that use decorators
- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - Configure Darker with policies and decorators
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) - Brighter's equivalent pipeline documentation
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) - Detailed guide to Polly policies in Brighter (patterns apply to Darker)
- [Polly Documentation](https://github.com/App-vNext/Polly) - Comprehensive guide to Polly resilience policies

Working examples can be found in the Darker samples: `Darker/samples/SampleMinimalApi/QueryHandlers/`
