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
using Microsoft.AspNetCore.Builder;
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

- **Default retry policy** (`Constants.RetryPolicyName`): retries three times, waiting 50, 100 and 150 milliseconds
- **Default circuit breaker** (`Constants.CircuitBreakerPolicyName`): opens after a single failure and stays open for 500 milliseconds

Registering a policy does not apply it. A policy runs only for a handler whose `ExecuteAsync` carries `[RetryableQuery]`, and the attribute runs the one policy it names: `Constants.RetryPolicyName` unless you pass another name. See [Query Pipeline and Decorators](/contents/QueryPipeline.md) for the attribute.

The defaults are a starting point. Their waits are short, and their circuit breaker opens on the first failure, so consider registering your own.

## Custom Query Policy Registry

For more control over resilience policies, you can create a custom policy registry with specific retry strategies, circuit breakers, and timeout policies:

```csharp
using Microsoft.AspNetCore.Builder;
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
    // Retry three times, waiting longer before each attempt
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
using System;
using System.Net.Http;
using Polly;

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

**Retry with callback:**
```csharp
using System;
using Polly;

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
using System;
using Polly;

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

**Retry and circuit breaker in one policy:**

`[RetryableQuery]` runs the one policy it names, so a handler that names a circuit breaker gets no retry. To retry *and* break, wrap the two and register the wrap under its own name:

```csharp
using System;
using Paramore.Darker.Policies;
using Polly;
using Polly.Registry;

var retry = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(new[]
    {
        TimeSpan.FromMilliseconds(50),
        TimeSpan.FromMilliseconds(100),
        TimeSpan.FromMilliseconds(150)
    });

var breaker = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(30));

var policyRegistry = new PolicyRegistry
{
    { Constants.RetryPolicyName, retry },
    { Constants.CircuitBreakerPolicyName, breaker },
    { "ExternalApiRetryAndBreak", Policy.WrapAsync(retry, breaker) }
};
```

A handler then names the wrap, `[RetryableQuery(2, "ExternalApiRetryAndBreak")]`. The retry is on the outside, so every attempt passes through the breaker: after two failures the breaker opens, and the retries left fail at once with `BrokenCircuitException`, without reaching your handler. `AddPolicies()` requires both `Constants` names in the registry, whatever else you register, and throws a `ConfigurationException` if either is missing.

For more information on Polly policies, see the [Polly documentation](https://github.com/App-vNext/Polly) and the Brighter documentation on [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md).

## Further Reading

- [Query Pipeline and Decorators](/contents/QueryPipeline.md) - The decorators these policies drive
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md) - Brighter's equivalent, in more detail
- [Darker and Brighter Pipelines](/contents/DarkerAndBrighterPipelines.md) - Where the two pipelines agree and differ
