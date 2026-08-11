# Darker and Brighter Pipelines

> **Explanation** · Applies to **Darker V4** · Prerequisites: [Query Pipeline and Decorators](/contents/QueryPipeline.md)

Darker's query pipeline shares many similarities with [Brighter's request pipeline](/contents/BuildingAPipeline.md), as both implement the same Russian Doll Model for middleware composition. However, there are some key differences to be aware of when working with both frameworks.

## Darker and Brighter Pipeline Similarities

**Russian Doll Model:**
Both frameworks use the same pipeline architecture where each handler/decorator wraps the next one in the chain, allowing cross-cutting concerns to execute before and after the core handler logic.

**Attribute-Based Ordering:**
Both use attributes with step numbers to control decorator execution order:
```csharp
// ...
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

## Darker and Brighter Pipeline Differences

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

## Further Reading

- [Query Pipeline and Decorators](/contents/QueryPipeline.md) - Darker's pipeline in full
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) - Brighter's pipeline in full
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md) - Why there are two pipelines at all
