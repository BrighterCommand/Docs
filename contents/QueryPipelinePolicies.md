---
description: "Darker's policy decorators are powered by Polly, a .NET resilience and transient-fault-handling library."
layout:
  description:
    visible: false
---

# Query Pipeline Policies

> **How-to** · Applies to **Darker V4** · Prerequisites: [Query Pipeline and Decorators](/contents/QueryPipeline.md)

Darker's policy decorators are powered by [Polly](https://github.com/App-vNext/Polly), a .NET resilience and transient-fault-handling library. You can configure policies to control retry behavior, circuit breaker thresholds, and timeouts.

## Default Query Pipeline Policies

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

## Custom Query Policy Registry

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

## Query Policy Naming Convention

Darker provides constants for common policy names in the `Paramore.Darker.Policies.Constants` class:

- `Constants.RetryPolicyName` - Default retry policy name
- `Constants.CircuitBreakerPolicyName` - Default circuit breaker policy name

**Best practices:**

- Use the provided constants for default policies
- Use descriptive names for custom circuit breakers (e.g., "ExternalApiCircuitBreaker", "DatabaseCircuitBreaker")
- Document your policy names in a central configuration class
- Consider creating a constants class for policy names used across your application:

```csharp
// ...
public static class QueryPolicies
{
    public const string DatabaseCircuitBreaker = "DatabaseCircuitBreaker";
    public const string ExternalApiCircuitBreaker = "ExternalApiCircuitBreaker";
    public const string CacheCircuitBreaker = "CacheCircuitBreaker";
}
```

## Advanced Query Policy Configurations

Polly supports many advanced resilience patterns:

**Handle specific exceptions:**
```csharp
// ...
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

**Retry with callback:**
```csharp
// ...
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
// ...
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

## Further Reading

- [Query Pipeline and Decorators](/contents/QueryPipeline.md) - The decorators these policies drive
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) - Brighter's equivalent, in more detail
- [Darker and Brighter Pipelines](/contents/DarkerAndBrighterPipelines.md) - Where the two pipelines agree and differ
