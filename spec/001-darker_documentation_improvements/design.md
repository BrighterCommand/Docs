# Technical Design: Darker Documentation Improvements

## Overview

This design document specifies the technical approach for creating comprehensive documentation for Darker, the query-side CQRS framework that complements Brighter. The documentation will follow the established patterns in the Brighter documentation while addressing the unique aspects of query processing.

## Documentation Architecture

### Documentation Hierarchy

```
Darker Documentation
│
├── Conceptual Foundation
│   ├── CQRS with Brighter and Darker (new)
│   └── Basic Concepts (existing - will add Darker terms)
│
├── Configuration & Setup
│   ├── DarkerBasicConfiguration (existing - expand)
│   └── Advanced DI Integration (embedded in basic config)
│
├── Core Concepts
│   ├── Queries and Query Objects (new)
│   └── Query Handlers (existing - complete)
│
├── Pipeline & Middleware
│   ├── Query Pipeline (new)
│
└── Patterns & Best Practices
    ├── Query Patterns (new)
    └── Cross-references to CQRS patterns
```

### Document Dependencies

```
Flow for New Users:

1. BasicConcepts.md (updated with Darker terms)
   ↓
2. CQRSWithBrighterAndDarker.md (conceptual overview)
   ↓
3. DarkerBasicConfiguration.md (getting started)
   ↓
4. QueriesAndQueryObjects.md (query design)
   ↓
5. ImplementAQueryHandler.md (implementation)
   ↓
6. QueryPipeline.md (middleware/decorators)
   ↓
7. QueryPatterns.md (advanced patterns)
```

## File-by-File Design Specifications

### 1. DarkerBasicConfiguration.md (P0 - Expand Existing)

**Target Length:** 350-400 lines
**Status:** Existing file with only title - complete rewrite

**Structure:**

```markdown
# Basic Configuration

## Introduction

- What is Darker?
- When to use Darker (2-3 sentences)
- Link to CQRSWithBrighterAndDarker.md

## Prerequisites

- .NET version requirements
- NuGet packages listing:
  - Paramore.Darker (core)
  - Paramore.Darker.AspNetCore
  - Paramore.Darker.QueryLogging
  - Paramore.Darker.Policies

## Quick Start with ASP.NET Core

### Basic Setup

[Code example from SampleMinimalApi/Program.cs]
- AddDarker() extension
- AddHandlersFromAssemblies()
- Injecting IQueryProcessor

### Minimal API Example

[Complete working example with endpoint]

### MVC Controller Example

[Controller-based example]

## Configuration Options

### Query Processor Lifetime

[Critical EF Core scoping example]
- Default: Transient
- When to use Scoped (EF Core scenario)
- Code example with options

### Handler Registration Strategies

- Assembly scanning (recommended)
- Manual registration via QueryHandlerRegistry

## Using IQueryProcessor

### ExecuteAsync Pattern

[Code example with CancellationToken]

### Execute Pattern (Synchronous)

[Code example for sync queries]

## Non-ASP.NET Configuration

### QueryProcessorBuilder Pattern

[README.md example]
- QueryHandlerRegistry
- Factory functions
- InMemoryQueryContextFactory

## Configuration with Decorators

### Adding Query Logging

[AddJsonQueryLogging example]

### Adding Policies

[AddDefaultPolicies and custom policy registry]

## Common Configuration Patterns

### Pattern: Basic Web API Setup

[Complete startup configuration]

### Pattern: With EF Core DbContext

[Scoped lifetime configuration]

### Pattern: Multiple Handler Assemblies

[Multiple assembly registration]

## Troubleshooting

### Common Issues
- Handler not found errors
- Lifetime scope issues with EF Core
- Assembly scanning not finding handlers

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)
- [Query Pipeline](/contents/QueryPipeline.md)
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)
```

**Code Examples Required:**

1. Basic Program.cs setup (from SampleMinimalApi)
2. Minimal API endpoint with IQueryProcessor
3. MVC Controller with IQueryProcessor
4. Scoped lifetime configuration for EF Core
5. QueryProcessorBuilder non-ASP.NET example
6. SimpleInjector integration example
7. Policy registry configuration (from DarkerSettings)

**Source References:**

- `Darker/samples/SampleMinimalApi/Program.cs`
- `Darker/samples/SampleMinimalApi/DarkerSettings.cs`
- `Darker/README.md`

---

### 2. ImplementAQueryHandler.md (P0 - Complete Existing)

**Target Length:** 450-500 lines
**Status:** Exists with "TODO" only - complete implementation

**Structure:**

# Implementing a Query Handler

## Introduction

- Purpose of query handlers
- Handler as entry point to query execution
- Link to Query Processor concept

## Query Objects

### Defining a Query

[IQuery<TResult> interface explanation]

### Simple Query Example

[GetPeopleQuery - no parameters]

### Parameterized Query Example

[GetPersonNameQuery with PersonId]

### Query Design Guidelines

- Immutability
- Value object pattern
- Validation placement

## Handler Implementation Patterns

### Pattern 1: Asynchronous Handler (Recommended)

#### QueryHandlerAsync<TQuery, TResult>

[GetPeopleQueryHandler example]
- ExecuteAsync method
- CancellationToken support
- Async/await patterns

#### When to Use

- I/O-bound operations (database, HTTP)
- Modern .NET applications
- ASP.NET Core applications

#### Complete Example

[Full GetPeopleQueryHandler from sample]

### Pattern 2: Synchronous Handler

#### QueryHandler<TQuery, TResult>

[Hypothetical synchronous example]
- Execute method
- When synchronous makes sense

#### When to Use

- In-memory operations
- Legacy integration
- Computational queries

#### Complete Example

[Synchronous handler example]

### Pattern 3: Direct IQueryHandler Implementation

#### IQueryHandler<TQuery, TResult> Interface

[Interface definition]

#### When to Use

- Maximum control
- Custom lifetime management
- Advanced scenarios

#### Complete Example

[Direct interface implementation]

## Query Handler Registration

### Automatic Registration (Recommended)

[AddHandlersFromAssemblies example]
- Assembly scanning
- Convention-based discovery

### Manual Registration

[QueryHandlerRegistry example]
- Explicit registration
- Per-handler control

## Working with Dependencies

### Constructor Injection

[Handler with repository dependency]

### Scoped Dependencies (EF Core)

[DbContext injection example]

### Multiple Dependencies

[Handler with multiple services]

## Query Results

### Simple Results

[Returning primitive types]

### Complex Results

[Returning DTOs and projections]

### Collections

[Returning IEnumerable, List, Dictionary]

### Null Handling

[Nullable reference types]

## Error Handling in Handlers

### Throwing Exceptions

[When and how to throw]

### Domain Exceptions

[Custom exception example]

### Validation

[Pre-execution validation]

## Testing Query Handlers

### Test Driven Development

[Handler test example]
- Replacing dependencies with in memory solutions, including Sqlite 
- Asserting results

## Acceptance Tests

- Use a real database to replace any in-memory version used for tests

## Best Practices

- Keep handlers focused (Single Responsibility)
- Use async for I/O operations
- Validate query parameters
- Return appropriate result types
- Handle nulls explicitly
- Use CancellationToken

## Common Pitfalls

- Forgetting CancellationToken parameter
- Using wrong handler base class
- Lifetime scope mismatches
- Not registering handlers

## Further Reading

- [Query Pipeline](/contents/QueryPipeline.md)
- [Query Patterns](/contents/QueryPatterns.md)
- [Basic Configuration](/contents/DarkerBasicConfiguration.md)

**Code Examples Required:**

1. GetPeopleQuery (simple, no params)
2. GetPersonNameQuery (with parameters)
3. GetPeopleQueryHandler (async, from sample)
4. GetPersonQueryHandler (with decorators, from sample)
5. Synchronous handler example
6. Direct IQueryHandler implementation
7. Handler with constructor dependencies
8. Handler with EF Core DbContext
9. Manual registration with QueryHandlerRegistry
10. Unit test example

**Source References:**
- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPeopleQueryHandler.cs`
- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPersonQueryHandler.cs`
- `Darker/README.md`

---

### 3. QueryPipeline.md (P0 - New File)

**Target Length:** 400-450 lines
**Status:** New file

**Structure:**

# Query Pipeline and Decorators

## Introduction

- What is a query pipeline?
- Russian Doll Model (from BasicConcepts.md)
- Comparison with Brighter's request pipeline

## How the Query Pipeline Works

### Pipeline Execution Flow

[ASCII diagram showing QueryProcessor → Decorator → Handler]

```
QueryProcessor.ExecuteAsync(query)
        ↓
[QueryLogging Decorator]
        ↓
[Retry Policy Decorator]
        ↓
[CircuitBreaker Decorator]
        ↓
[Target Query Handler]
        ↓
    Result (flows back up)
```

### Decorator Ordering

- Controlled by step number in attributes
- Execution order (1, 2, 3...)
- Why order matters

## Available Decorators

### QueryLogging Decorator

#### Purpose

- Logs query execution
- JSON serialization of queries
- Performance tracking

#### Configuration

[AddJsonQueryLogging example]

#### Usage

[QueryLogging attribute example]

```csharp
[QueryLogging(1)]
public override async Task<Result> ExecuteAsync(...)
```

#### What Gets Logged

- Query type
- Query parameters
- Execution time
- Result summary

### Policy Decorators (Resilience)

#### RetryableQuery Decorator

**Purpose:** Retry failed queries
**Use Cases:** Transient failures, network issues

[RetryableQuery attribute example from GetPeopleQueryHandler]

```csharp
[RetryableQuery(2, "CircuitBreakerName")]
```

**Policy Configuration:**

[DarkerSettings.cs policy registry example]

#### FallbackPolicy Decorator

**Purpose:** Provide fallback result on failure
**Use Cases:** Degraded service, default values

[FallbackPolicy attribute example from GetPersonQueryHandler]

**Implementing Fallback:**

```csharp
[FallbackPolicy(2)]
public override async Task<string> ExecuteAsync(...)
{
    // primary implementation
}

public override Task<string> FallbackAsync(...)
{
    return Task.FromResult("default value");
}
```

#### CircuitBreaker Integration

- Specified by policy name
- Configured in policy registry
- Multiple circuit breakers

### Custom Decorators

#### Creating Custom Decorators

[Interface and base class for custom decorator]

#### Registration

[How to register custom decorators]

## Decorator Patterns

### Pattern: Logging + Retry

[Complete example from GetPeopleQueryHandler]

```csharp
[QueryLogging(1)]
[RetryableQuery(2, "DefaultCircuitBreaker")]
```

### Pattern: Logging + Fallback + Retry

[Complete example from GetPersonQueryHandler]

```csharp
[QueryLogging(1)]
[FallbackPolicy(2)]
[RetryableQuery(3, "SpecificCircuitBreaker")]
```

### Pattern: Multiple Circuit Breakers

[Example with different breakers for different failures]

## Configuring Polly Policies

### Default Policies

[AddDefaultPolicies example]

### Custom Policy Registry

[DarkerSettings.ConfigurePolicies() example]
- Retry policies
- Circuit breaker policies
- Timeout policies

### Policy Naming Convention

[Constants and naming patterns]

## Pipeline Context

### Sharing Data Between Decorators

[If supported - check Darker source]

### Context Bag Pattern

[If applicable]

## Comparison with Brighter Pipeline

### Similarities

- Russian Doll Model
- Attribute-based ordering
- Policy integration

### Differences

- Query-specific decorators
- No external bus support
- Return values (queries return results)

## Best Practices

- Order decorators logically (logging first)
- Use circuit breakers for external dependencies
- Implement fallbacks for user-facing queries
- Keep decorators focused and composable
- Use named circuit breakers for different failure types

## Common Pitfalls

- Wrong decorator ordering
- Forgetting to configure policies
- Circuit breaker naming mismatches
- Fallback not implemented when using FallbackPolicy

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)
- [Building a Pipeline of Request Handlers](/contents/BuildingAPipeline.md) (Brighter)
- [Supporting Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)

**Code Examples Required:**

1. ASCII pipeline execution diagram
2. QueryLogging decorator usage
3. RetryableQuery decorator usage
4. FallbackPolicy decorator with FallbackAsync implementation
5. Multiple decorators together (from GetPersonQueryHandler)
6. Policy registry configuration (from DarkerSettings)
7. Custom decorator skeleton (if feasible)

**Source References:**

- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPeopleQueryHandler.cs`
- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPersonQueryHandler.cs`
- `Darker/samples/SampleMinimalApi/DarkerSettings.cs`

---

### 4. QueriesAndQueryObjects.md (P0 - New File)

**Target Length:** 300-350 lines
**Status:** New file

**Structure:**

# Queries and Query Objects

## Introduction
- Query Object pattern
- Separation of query parameters from execution
- Link to CQRS concepts

## The IQuery<TResult> Interface

### Interface Definition

```csharp
public interface IQuery<out TResult> { }
```

### Type Parameter

- TResult: The type returned by the query

## Designing Query Objects

### Simple Queries (No Parameters)

[GetPeopleQuery example]

```csharp
public sealed class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>>
{
}
```

**When to Use:**

- List/collection queries
- Dashboard queries
- Queries with no filter parameters

### Parameterized Queries

[GetPersonNameQuery example]

```csharp
public sealed class GetPersonNameQuery : IQuery<string>
{
    public GetPersonNameQuery(int personId)
    {
        PersonId = personId;
    }

    public int PersonId { get; }
}
```

**When to Use:**

- Single entity lookups
- Filtered queries
- Searches with criteria

### Complex Query Parameters

#### Multiple Parameters

[Example with multiple filters]

#### Optional Parameters

[Example with nullable parameters]

#### Filter Objects

[Example with complex filter object]

## Query Object Design Principles

### Immutability

- Read-only properties
- Constructor initialization
- No setters (or init-only)

### Value Object Pattern

- Queries as value objects
- Equality based on parameters
- No identity

### Encapsulation

- Public properties for handler access
- Private fields if needed
- Computed properties

## Query Result Types

### Primitive Types

```csharp
public class GetCountQuery : IQuery<int> { }
```

### DTOs and Projections

```csharp
public class GetPersonDetailsQuery : IQuery<PersonDetailsDto> { }
```

### Collections

```csharp
public class GetOrdersQuery : IQuery<IReadOnlyList<OrderDto>> { }
```

### Dictionaries

[GetPeopleQuery returning dictionary]

### Nullable Results

```csharp
public class FindPersonQuery : IQuery<PersonDto?> { }
```

## Validation in Query Objects

### Constructor Validation

[Example with guard clauses]

### Validation Attributes

[Example with data annotations]

### Where to Validate

- Simple validation in constructor
- Complex validation in handler
- Framework validation in web layer

## Query Naming Conventions

### Recommended Patterns

- GetXQuery (single item)
- GetXListQuery / GetXsQuery (collection)
- FindXQuery (may return null)
- SearchXQuery (with criteria)

### Examples

```csharp
GetPersonQuery
GetPeopleQuery
FindOrderByIdQuery
SearchProductsQuery
```

## Query Organization

### File Structure

```
Queries/
  ├── GetPeopleQuery.cs
  ├── GetPersonNameQuery.cs
  └── SearchOrdersQuery.cs
```

### Colocation with Handlers

[Discussion of keeping queries with handlers]

### Shared Query Library

[Discussion of shared query definitions]

## Query Patterns

### Pattern: Pagination Query

[Example with page/size parameters]

### Pattern: Search Query

[Example with filter criteria]

### Pattern: Projection Query

[Example requesting specific fields]

### Pattern: Aggregation Query

[Example for reports/summaries]

## Best Practices

- Make queries immutable
- Use descriptive names
- Keep queries simple (just parameters)
- Validate critical parameters
- Use appropriate result types
- Use read-only collections for results
- Consider nullable reference types

## Common Pitfalls

- Mutable query objects
- Business logic in queries
- Complex validation in queries
- Missing null handling
- Inappropriate result types

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)
- [Query Patterns](/contents/QueryPatterns.md)
- [Basic Concepts](/contents/BasicConcepts.md)
- [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)

**Code Examples Required:**

1. GetPeopleQuery (simple, no params)
2. GetPersonNameQuery (with parameters)
3. Complex query with multiple parameters
4. Query with optional parameters
5. Query with validation
6. Various result type examples
7. Pagination query example
8. Search query example

**Source References:**

- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPeopleQueryHandler.cs`
- `Darker/samples/SampleMinimalApi/QueryHandlers/GetPersonQueryHandler.cs`

---

### 5. CQRSWithBrighterAndDarker.md (P1 - New File)

**Target Length:** 350-400 lines
**Status:** New file

**Structure:**

# CQRS with Brighter and Darker

## Introduction

- What is CQRS?
- Why separate reads and writes?
- Brighter + Darker as complete CQRS solution

## CQRS Fundamentals

### Command Query Separation (CQS)

[Reference BasicConcepts.md]

- Commands change state
- Queries return state
- No mixing

### Command Query Responsibility Segregation (CQRS)

- Separate models for reads and writes
- Different optimization strategies
- Eventual consistency considerations

## Brighter: The Command Side

### What Brighter Provides

- Command processing
- Event publishing
- External bus support
- Outbox pattern

### Command Pattern

[Brief example of command handler]

- Link to Brighter documentation

## Darker: The Query Side

### What Darker Provides

- Query processing
- Read-optimized handlers
- Decorator pipeline
- No external bus (queries are local)

### Query Pattern

[Brief example of query handler]

## Integrating Brighter and Darker

### Complete CQRS Architecture

```
┌─────────────────────────────────────────┐
│           Web Application               │
├─────────────────┬───────────────────────┤
│   Controllers   │    Controllers        │
│                 │                       │
│  CommandProcessor  QueryProcessor      │
│   (Brighter)    │    (Darker)          │
├─────────────────┼───────────────────────┤
│ Command Handlers│  Query Handlers      │
│                 │                       │
│  Write Model    │    Read Model        │
│  (Normalized)   │  (Denormalized)      │
│                 │                       │
│  Write DB       │    Read DB           │
│  (or same DB)   │  (or same DB)        │
└─────────────────┴───────────────────────┘
```

### Example Application Structure

[Folder structure showing Brighter and Darker together]

### Dependency Injection Setup

[ConfigureServices with both AddBrighter and AddDarker]

## Use Cases and Patterns

### Pattern: Simple CQRS (Same Database)

**Scenario:** Single database, different models

[Code example showing command and query on same entity]

- Command: CreateOrder
- Query: GetOrderDetails
- Same DB, optimized differently

### Pattern: Separate Read/Write Databases

**Scenario:** Write to primary, read from replica

[Architecture diagram]

- Commands to write database
- Queries to read replica
- Replication lag considerations

### Pattern: Event-Sourced Writes, Projected Reads

**Scenario:** Event sourcing with read projections

[Architecture diagram]

- Commands produce events
- Events update projections
- Queries read projections
- Link to Event Driven Collaboration

### Pattern: Task-Based UI

**Scenario:** User actions as commands

[Example of UI calling commands and queries]

- Form submission → Command
- Page load → Query
- No mixing

## When to Use CQRS

### Good Use Cases

- Complex domains with different read/write needs
- High read/write ratio differences
- Multiple read representations needed
- Event-driven architectures
- Microservices with separate concerns

### When to Avoid

- Simple CRUD applications
- Small applications
- Limited team experience
- No clear read/write separation

## Benefits of Brighter + Darker

- Clear separation of concerns
- Optimized read and write paths
- Consistent patterns (both use pipelines)
- Support for distributed systems (Brighter)
- Resilience patterns (both)
- Easy to test

## Trade-offs and Considerations

### Complexity

- More code than simple CRUD
- Two frameworks to learn
- Separate models to maintain

### Eventual Consistency

- When using separate databases
- Read models may lag
- UI considerations

### Learning Curve

- CQRS concepts
- Two frameworks
- Pattern enforcement

## Example: E-Commerce Order System

### Write Side (Brighter)

[PlaceOrderCommand example]

- Validation
- Business rules
- Event publishing

### Read Side (Darker)

[GetOrderDetailsQuery example]

- Denormalized view
- Optimized for display
- No business rules

### Complete Flow

1. User submits order → PlaceOrderCommand
2. Handler validates and creates order
3. OrderPlacedEvent published
4. Read model updated (sync or async)
5. User views order → GetOrderDetailsQuery
6. Handler returns optimized view

## Best Practices

- Use Brighter for all state changes
- Use Darker for all state queries
- Keep read and write models separate
- Don't query in command handlers
- Don't update in query handlers
- Use events to sync read models (if separate)
- Handle eventual consistency in UI

## Common Pitfalls

- Querying in command handlers
- Updating in query handlers
- Mixing command and query logic
- Over-engineering simple scenarios
- Ignoring eventual consistency
- Not training team on CQRS

## Further Reading

- [Basic Concepts](/contents/BasicConcepts.md)
- [Event Driven Collaboration](/contents/EventDrivenCollaboration.md)
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)
- [Dispatching Requests](/contents/DispatchingARequest.md)
- [Brighter Basic Configuration](/contents/BrighterBasicConfiguration.md)
- [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md)

**Code Examples Required:**

1. Combined DI setup (AddBrighter + AddDarker)
2. Architecture diagrams (ASCII art)
3. Command example (brief, link to Brighter docs)
4. Query example (brief, link to Darker docs)
5. Same database pattern example
6. E-commerce order flow example

**Source References:**
- `Docs/contents/BasicConcepts.md`
- `Docs/contents/EventDrivenCollaboration.md`
- `Docs/contents/BrighterBasicConfiguration.md`

---

### 6. QueryPatterns.md (P1 - New File)

**Target Length:** 350-400 lines
**Status:** New file

**Structure:**

# Query Patterns

## Introduction
- Common query patterns in real applications
- Practical examples beyond basic CRUD
- Performance considerations

## Parameterized Query Patterns

### Pattern: Single Entity Lookup

**Use Case:** Retrieve one entity by identifier

[GetPersonNameQuery example]

**Variations:**
- By ID
- By unique key
- By composite key

### Pattern: Filtered List

**Use Case:** List with filter criteria

[Example: GetOrdersByCustomerQuery]

### Pattern: Search with Criteria

**Use Case:** Complex search with multiple filters

[Example: SearchProductsQuery with multiple filters]

## Pagination Patterns

### Pattern: Offset-Based Pagination

[Example: GetOrdersPageQuery]

```csharp
public class GetOrdersPageQuery : IQuery<PagedResult<OrderDto>>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
```

### Pattern: Cursor-Based Pagination

[Example for large datasets]

### Pattern: Keyset Pagination

[Example for stable pagination]

## Projection Patterns

### Pattern: Simple Projection

**Use Case:** Return subset of properties

[Example: GetCustomerSummaryQuery]

### Pattern: Complex Projection

**Use Case:** Aggregate data from multiple sources

[Example: GetOrderDetailsWithCustomerQuery]

### Pattern: Calculated Fields

**Use Case:** Computed values in result

[Example: GetOrderWithTotalsQuery]

## Collection Query Patterns

### Pattern: Get All (Small Collections)

[GetPeopleQuery example - small, cacheable]

### Pattern: Get Many with Filter

[Example: GetActiveCustomersQuery]

### Pattern: Grouped Results

[Example: GetOrdersByStatusQuery returning grouped data]

## Aggregation Patterns

### Pattern: Count Query

[Example: GetOrderCountQuery]

### Pattern: Summary/Statistics

[Example: GetSalesStatisticsQuery]

### Pattern: Report Query

[Example: GetMonthlySalesReportQuery]

## Entity Framework Core Integration

### Pattern: IQueryable Projection

[Example with EF Core Select]

### Pattern: Include Related Data

[Example with Include/ThenInclude]

### Pattern: AsNoTracking for Reads

[Example showing read-only optimization]

### Pattern: Compiled Queries

[Example for frequently executed queries]

### Scoping with EF Core

[Reminder about ServiceLifetime.Scoped]
[Link to DarkerBasicConfiguration.md]

## Caching Patterns

### Pattern: Query Result Caching

[Example with decorator or manual caching]

### Pattern: Cache Invalidation

[Discussion of cache keys and invalidation]

### Pattern: Conditional Caching

[Example caching some queries, not others]

## Multiple Data Source Patterns

### Pattern: Query with Fallback Sources

[Example trying multiple sources]

### Pattern: Combined Results

[Example merging results from multiple sources]

### Pattern: Enriched Results

[Example: query database, enrich from API]

## Sorting and Ordering

### Pattern: Dynamic Sorting

[Example with sort parameter]

### Pattern: Default Ordering

[Example with consistent default sort]

## Null Handling Patterns

### Pattern: Nullable Result

[Example: FindOrderQuery returning null if not found]

### Pattern: Option/Maybe Result

[Example with Option<T> if applicable]

### Pattern: Default Value

[Example with fallback using FallbackAsync]

## Performance Patterns

### Pattern: Select Only What You Need

[Example with specific field selection]

### Pattern: Batch Queries

[Example reducing N+1 queries]

### Pattern: Async All the Way

[Example with proper async/await]

## Real-World Example: E-Commerce Queries

### Get Product Catalog (Paginated)

[Complete example]

### Search Products

[Complete example with filters]

### Get Order Details

[Complete example with related data]

### Get Customer Dashboard

[Complete example with aggregations]

## Best Practices

- Use pagination for large result sets
- Project only needed fields
- Use async queries for I/O
- Cache appropriately
- Handle nulls explicitly
- Use AsNoTracking with EF Core
- Avoid N+1 queries
- Consider read replicas for scale

## Common Pitfalls

- Loading entire collections without pagination
- Forgetting AsNoTracking
- N+1 query problems
- Over-fetching data
- Under-fetching (multiple roundtrips)
- Not using CancellationToken
- Caching too aggressively

## Performance Considerations

- Database indexes for query filters
- Appropriate page sizes
- Query execution plan analysis
- Read replica usage
- Connection pooling
- Compiled queries for hot paths

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)
- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)
- [Query Pipeline](/contents/QueryPipeline.md)
- [Darker Basic Configuration](/contents/DarkerBasicConfiguration.md)

**Code Examples Required:**

1. GetPersonNameQuery (single entity)
2. Filtered list query
3. Search with multiple criteria
4. Pagination query (offset-based)
5. Projection query
6. Aggregation query
7. EF Core patterns (AsNoTracking, Include)
8. Caching decorator example
9. 2-3 complete real-world examples

**Source References:**

- `Darker/samples/SampleMinimalApi/QueryHandlers/*.cs`
- EF Core best practices (external knowledge)

---

## SUMMARY.md Updates

### New "Darker" Section Structure

```markdown
## Darker Configuration

 * [Basic Configuration](/contents/DarkerBasicConfiguration.md)

## Darker Query Handlers and Middleware Pipelines

 * [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)
 * [How to Implement a Query Handler](/contents/ImplementAQueryHandler.md)
 * [Query Pipeline and Decorators](/contents/QueryPipeline.md)
 * [Query Patterns](/contents/QueryPatterns.md)

## CQRS Patterns

 * [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)
```

**Location in SUMMARY.md:**

- Place after "Brighter Configuration" section (line ~13)
- Before "Brighter Request Handlers and Middleware Pipelines"
- Creates parallel structure with Brighter sections

### Alternative: Expand Existing "Darker Configuration" Section

If keeping minimal changes:

```markdown
## Darker Configuration

 * [Basic Configuration](/contents/DarkerBasicConfiguration.md)
 * [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)
 * [How to Implement a Query Handler](/contents/ImplementAQueryHandler.md)
 * [Query Pipeline and Decorators](/contents/QueryPipeline.md)
 * [Query Patterns](/contents/QueryPatterns.md)
 * [CQRS with Brighter and Darker](/contents/CQRSWithBrighterAndDarker.md)
```

**Recommendation:** Use expanded structure for better organization.

---

## Minor Updates to Existing Files

### BasicConcepts.md Updates (P1)

**Changes Required:**
1. Verify Query Processor section is accurate (lines 80-91)
2. Add cross-reference to DarkerBasicConfiguration.md
3. Ensure consistency with new Darker terminology

**Estimated effort:** 30 minutes

### Glossary.md Updates (P1)

**New Terms to Add:**

- IQuery<TResult>
- Query Object
- Query Handler (expand definition)
- Query Processor (expand definition)
- Query Pipeline
- Query Decorator

**Estimated effort:** 45 minutes

### EventDrivenCollaboration.md Updates (P2)

**Changes Required:**

1. Add section on query-side in event-driven systems
2. Reference Darker for read model queries
3. Link to CQRSWithBrighterAndDarker.md

**Estimated effort:** 1 hour

---

## Technology Decisions

### Markdown Standards

- GitHub-flavored markdown
- GitBook compatibility
- PascalCase file names
- Relative links for internal references

### Code Example Standards

- C# with syntax highlighting (```csharp)
- .NET 8/9 (from SampleMinimalApi)
- Async/await patterns (modern .NET)
- Using statements when necessary
- Comments for omitted code (`// ...`)

### Documentation Tools

- No special tools required
- Standard markdown editors
- Git for version control
- GitBook for publishing (existing setup)

---

## Code Example Strategy

### Source Material Priority

1. **Primary:** `Darker/samples/SampleMinimalApi/` (working code)
2. **Secondary:** `Darker/README.md` (documented examples)
3. **Tertiary:** Create minimal examples when needed

### Testing Strategy

- Reference working sample code when possible
- Create new examples based on sample patterns
- Validate code examples compile
- Note: Cannot modify or run code in Darker repository

### Example Reuse

- Reuse GetPeopleQuery/Handler across multiple docs
- Reuse GetPersonNameQuery/Handler for decorator examples
- Create variants when demonstrating specific patterns
- Always link back to full sample

---

## Implementation Approach

### Phase 1: P0 Requirements (Must Have)

**Files to Create/Complete:**

1. DarkerBasicConfiguration.md (expand)
2. ImplementAQueryHandler.md (complete)
3. QueriesAndQueryObjects.md (new)
4. QueryPipeline.md (new)
5. Update SUMMARY.md

**Estimated effort:** 12-15 hours

**Dependencies:**

- Can be done in parallel after basic structure agreed
- SUMMARY.md updated last

**Order of implementation:**

1. DarkerBasicConfiguration.md (foundation)
2. QueriesAndQueryObjects.md (concepts before implementation)
3. ImplementAQueryHandler.md (implementation)
4. QueryPipeline.md (advanced features)
5. SUMMARY.md updates

### Phase 2: P1 Requirements (Should Have)

**Files to Create/Complete:**

1. CQRSWithBrighterAndDarker.md (new)
2. QueryPatterns.md (new)
3. Update BasicConcepts.md
4. Update Glossary.md

**Estimated effort:** 8-10 hours

**Dependencies:**

- Requires Phase 1 completion for accurate cross-links
- Can be done partially in parallel

### Phase 3: P2 Requirements (Nice to Have)

**Future consideration based on time/priority**

---

## Risk Mitigation

### Risk: Limited Sample Code

**Mitigation:**

- Leverage SampleMinimalApi extensively
- Create examples based on patterns in sample
- Reference README.md for additional patterns
- Review Darker source code for interface definitions

**Impact:** Medium
**Likelihood:** Low (sample code is adequate)

### Risk: Darker Feature Gaps

**Mitigation:**

- Review Darker source code to verify features
- Check for features mentioned in README but not in sample
- Document what exists, not what should exist
- Note limitations where appropriate

**Impact:** Low
**Likelihood:** Low (Darker is stable)

### Risk: Inconsistency with Brighter Docs

**Mitigation:**

- Use Brighter docs as template
- Follow CLAUDE.md guidelines strictly
- Cross-reference terminology in BasicConcepts.md
- Review for consistent voice and style

**Impact:** Medium
**Likelihood:** Low (guidelines are clear)

### Risk: Code Examples Don't Compile

**Mitigation:**

- Base examples on working sample code
- Use complete using statements
- Note any pseudo-code clearly
- Validate syntax for common errors

**Impact:** High (NFR-2 requirement)
**Likelihood:** Low (using working samples)

### Risk: SUMMARY.md Breaking Changes

**Mitigation:**

- Test all links after updates
- Maintain existing structure
- Add, don't remove or reorganize
- Review GitBook rendering

**Impact:** High (affects navigation)
**Likelihood:** Very Low (additive changes only)

### Risk: Documentation Scope Creep

**Mitigation:**

- Stick to approved requirements
- Focus on P0, then P1, defer P2
- Reference Out of Scope section
- Track completion against success metrics

**Impact:** Medium (delays completion)
**Likelihood:** Medium (comprehensive documentation)

---

## Success Criteria

### Completion Criteria (from Requirements)

✅ **Must Achieve:**
- [ ] 6+ new/completed documentation files
- [ ] DarkerBasicConfiguration.md: 300+ lines
- [ ] ImplementAQueryHandler.md: 400+ lines
- [ ] All P0 requirements complete
- [ ] SUMMARY.md updated correctly

✅ **Should Achieve:**
- [ ] 80%+ of P1 requirements
- [ ] All code examples compile
- [ ] All internal links work
- [ ] Consistent CLAUDE.md compliance

### Quality Gates

**Before marking complete:**
1. All code examples include using statements
2. All cross-links tested
3. Terminology consistent with Glossary
4. CLAUDE.md checklist passed
5. Similar depth to equivalent Brighter docs

---

## Next Steps

After design approval:

1. **Use `/spec:tasks`** to break down into implementation tasks
   - Task per file (or per major section)
   - Dependencies identified
   - Estimated effort per task

2. **Use `/spec:implement`** to execute implementation
   - Work through tasks systematically
   - Test as you go
   - Update SUMMARY.md at end

3. **Review and refinement**
   - Cross-link verification
   - Code example validation
   - Style consistency check

---

## Appendix: ASCII Diagrams

### Query Pipeline Execution Flow

```
User Request
     ↓
┌────────────────────┐
│  IQueryProcessor   │
│  ExecuteAsync()    │
└────────┬───────────┘
         ↓
┌────────────────────┐
│ [QueryLogging]     │  ← Decorator (step 1)
│ Logs query start   │
└────────┬───────────┘
         ↓
┌────────────────────┐
│ [FallbackPolicy]   │  ← Decorator (step 2)
│ Wraps execution    │
└────────┬───────────┘
         ↓
┌────────────────────┐
│ [RetryableQuery]   │  ← Decorator (step 3)
│ Retry on failure   │
└────────┬───────────┘
         ↓
┌────────────────────┐
│ Query Handler      │  ← Target Handler
│ ExecuteAsync()     │
│ Returns Result     │
└────────┬───────────┘
         ↓
    Result flows back
    through decorators
         ↓
    User Response
```

### CQRS Architecture with Brighter and Darker

```
┌──────────────────────────────────────────────────────┐
│                  Web Application                     │
└───────────────────────┬──────────────────────────────┘
                        ↓
        ┌───────────────┴────────────────┐
        ↓                                ↓
┌───────────────────┐          ┌─────────────────────┐
│   Write Side      │          │    Read Side        │
│   (Brighter)      │          │    (Darker)         │
├───────────────────┤          ├─────────────────────┤
│ ICommandProcessor │          │  IQueryProcessor    │
│       ↓           │          │        ↓            │
│ Command Handlers  │          │  Query Handlers     │
│       ↓           │          │        ↓            │
│  Write Model      │          │   Read Model        │
│  (Normalized)     │          │ (Denormalized)      │
│       ↓           │          │        ↓            │
│  Write Database   │══Events═▶│  Read Database      │
└───────────────────┘          └─────────────────────┘
```

---

**Design document complete and ready for review.**

Please use `/spec:review design` to provide feedback or `/spec:approve design` to proceed to task breakdown.
