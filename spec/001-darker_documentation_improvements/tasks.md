# Implementation Tasks: Darker Documentation Improvements

## Overview

This task list breaks down the Darker documentation improvements into specific, actionable tasks organized by phase and priority. Tasks are designed to enable incremental development with clear dependencies.

**Success Criteria:**

- All tasks completed
- All code examples compile
- All cross-links functional
- CLAUDE.md compliance verified

---

## Phase 1: Foundation and Core Documentation

**Goal:** Complete core Darker documentation files
**Priority:** Must complete before Phase 2

### Task Group 1.1: DarkerBasicConfiguration.md

**Dependencies:** None - can start immediately

- [ ] **Task 1.1.1:** Write Introduction and Prerequisites sections
  - Review [design specification](./design.md) for this file
  - Identify code examples needed from [samples](../../../Darker/samples/SampleMinimalApi/)
  - Write "What is Darker?" introduction (2-3 sentences)
  - Add link to CQRSWithBrighterAndDarker.md (placeholder for now)
  - Document .NET version requirements
  - List all NuGet packages with descriptions

- [ ] **Task 1.1.2:** Write Quick Start with ASP.NET Core section
  - Review [design specification](./design.md) for this file
  - Extract and document basic setup from [samples](../../../Darker/samples/SampleMinimalApi/Program.cs)
  - Create Minimal API example with complete endpoint code
  - Create MVC Controller example (adapt from Minimal API pattern)
  - Include using statements for all examples

- [ ] **Task 1.1.3:** Write Configuration Options section
  - Review [design specification](./design.md) for this file
  - Document QueryProcessorLifetime with default and scoped options
  - Create EF Core scoping example with explanation
  - Document handler registration strategies (assembly scanning vs manual)
  - Include code examples for each approach

- [ ] **Task 1.1.4:** Write Using IQueryProcessor section
  - Review [design specification](./design.md) for this file
  - Document ExecuteAsync pattern with CancellationToken
  - Document Execute pattern for synchronous queries
  - Create complete examples for both patterns
  - Show injection into controllers/endpoints

- [ ] **Task 1.1.5:** Write Configuration with Decorators section
  - Review [design specification](./design.md) for this file
  - Document AddJsonQueryLogging with example
  - Document AddDefaultPolicies with example
  - Create custom policy registry example from DarkerSettings.cs

- [ ] **Task 1.1.6:** Write Common Configuration Patterns section
  - Review [design specification](./design.md) for this file
  - Create "Basic Web API Setup" complete pattern
  - Create "With EF Core DbContext" complete pattern
  - Create "Multiple Handler Assemblies" pattern
  - Include full working examples for each

- [ ] **Task 1.1.7:** Write Troubleshooting and Further Reading sections
  - Review [design specification](./design.md) for this file
  - List 3-5 common issues with solutions
  - Add Further Reading links (use placeholder URLs for not-yet-created docs)

- [ ] **Task 1.1.8:** Review and validate DarkerBasicConfiguration.md
  - Review [design specification](./design.md) for this file
  - Verify all code examples have using statements
  - Check target length (350-400 lines)
  - Verify CLAUDE.md compliance
  - Test markdown rendering

---

### Task Group 1.2: QueriesAndQueryObjects.md (Concepts)

**Dependencies:** None - can work in parallel with 1.1

- [ ] **Task 1.2.1:** Write Introduction and IQuery<TResult> Interface sections
  - Review [design specification](./design.md) for this file
  - Explain Query Object pattern
  - Document IQuery<TResult> interface
  - Explain type parameter usage
  - Link to CQRS concepts (placeholder)

- [ ] **Task 1.2.2:** Write Designing Query Objects section
  - Review [design specification](./design.md) for this file
  - Document simple queries (no parameters) with GetPeopleQuery
  - Document parameterized queries with GetPersonNameQuery
  - Create examples of complex query parameters
  - Create examples of optional parameters
  - Create filter object example

- [ ] **Task 1.2.3:** Write Query Object Design Principles section
  - Review [design specification](./design.md) for this file
  - Document immutability principles with examples
  - Document value object pattern
  - Document encapsulation patterns
  - Create concrete code examples for each

- [ ] **Task 1.2.4:** Write Query Result Types section
  - Review [design specification](./design.md) for this file
  - Create examples for primitive types
  - Create examples for DTOs and projections
  - Create examples for collections (List, IReadOnlyList, Dictionary)
  - Document nullable results with examples

- [ ] **Task 1.2.5:** Write Validation and Naming Conventions sections
  - Review [design specification](./design.md) for this file
  - Create constructor validation example with guard clauses
  - Document where to validate (constructor vs handler vs web layer)
  - Document naming conventions (GetX, FindX, SearchX)
  - Provide 5-6 naming examples

- [ ] **Task 1.2.6:** Write Query Organization and Query Patterns sections
  - Review [design specification](./design.md) for this file
  - Document file structure approaches
  - Discuss colocation vs shared libraries
  - Create pagination query example
  - Create search query example
  - Create projection and aggregation examples

- [ ] **Task 1.2.7:** Write Best Practices, Common Pitfalls, and Further Reading
  - Review [design specification](./design.md) for this file
  - List 6-8 best practices
  - List 4-5 common pitfalls
  - Add Further Reading links (placeholders for not-yet-created docs)

---

### Task Group 1.3: ImplementAQueryHandler.md (Implementation)

**Dependencies:** 1.2 complete (references query objects)

- [ ] **Task 1.3.1:** Write Introduction and Query Objects section
  - Review [design specification](./design.md) for this file
  - Write purpose of query handlers introduction
  - Link to Query Processor concept in BasicConcepts.md
  - Create "Defining a Query" subsection referencing QueriesAndQueryObjects.md
  - Include GetPeopleQuery and GetPersonNameQuery examples
  - Document query design guidelines

- [ ] **Task 1.3.2:** Write Pattern 1: Asynchronous Handler section
  - Review [design specification](./design.md) for this file
  - Document QueryHandlerAsync<TQuery, TResult> pattern
  - Create complete GetPeopleQueryHandler example from sample
  - Explain ExecuteAsync method signature
  - Document CancellationToken usage
  - Document when to use async handlers

- [ ] **Task 1.3.3:** Write Pattern 2: Synchronous Handler section
  - Review [design specification](./design.md) for this file
  - Document QueryHandler<TQuery, TResult> pattern
  - Create hypothetical synchronous example
  - Explain Execute method signature
  - Document when to use synchronous handlers

- [ ] **Task 1.3.4:** Write Pattern 3: Direct IQueryHandler Implementation section
  - Review [design specification](./design.md) for this file
  - Document IQueryHandler<TQuery, TResult> interface
  - Create direct interface implementation example
  - Document when to use this approach (advanced scenarios)

- [ ] **Task 1.3.5:** Write Query Handler Registration section
  - Review [design specification](./design.md) for this file
  - Document automatic registration with AddHandlersFromAssemblies
  - Create assembly scanning example
  - Document manual registration with QueryHandlerRegistry
  - Create explicit registration example

- [ ] **Task 1.3.6:** Write Working with Dependencies section
  - Review [design specification](./design.md) for this file
  - Create constructor injection example with repository
  - Create scoped dependency example with EF Core DbContext
  - Create multiple dependencies example
  - Include using statements and complete code

- [ ] **Task 1.3.7:** Write Query Results and Error Handling sections
  - Review [design specification](./design.md) for this file
  - Create examples for simple, complex, and collection results
  - Document null handling with nullable reference types
  - Document exception throwing patterns
  - Create custom domain exception example
  - Document validation patterns

- [ ] **Task 1.3.8:** Write Testing Query Handlers section
  - Review [design specification](./design.md) for this file
  - Create unit test example with mocked dependencies
  - Document test-driven development approach
  - Create acceptance test example with real database
  - Include using statements and complete test code

- [ ] **Task 1.3.9:** Write Best Practices, Common Pitfalls, and Further Reading
  - Review [design specification](./design.md) for this file
  - List 6-8 best practices
  - List 4-5 common pitfalls
  - Add Further Reading links

---

### Task Group 1.4: QueryPipeline.md (Advanced Features)

**Dependencies:** 1.3 complete (references handlers)

- [ ] **Task 1.4.1:** Write Introduction and How Pipeline Works sections
  - Review [design specification](./design.md) for this file
  - Explain query pipeline concept
  - Reference Russian Doll Model from BasicConcepts.md
  - Compare with Brighter's request pipeline
  - Create ASCII pipeline execution flow diagram
  - Explain decorator ordering with step numbers

- [ ] **Task 1.4.2:** Write QueryLogging Decorator section
  - Review [design specification](./design.md) for this file
  - Document purpose and usage
  - Create AddJsonQueryLogging configuration example
  - Create QueryLogging attribute usage example
  - Document what gets logged

- [ ] **Task 1.4.3:** Write RetryableQuery Decorator section
  - Review [design specification](./design.md) for this file
  - Document purpose and use cases
  - Create RetryableQuery attribute example from GetPeopleQueryHandler
  - Document policy configuration from DarkerSettings.cs
  - Explain circuit breaker integration

- [ ] **Task 1.4.4:** Write FallbackPolicy Decorator section
  - Review [design specification](./design.md) for this file
  - Document purpose and use cases
  - Create FallbackPolicy attribute example from GetPersonQueryHandler
  - Create FallbackAsync implementation example
  - Show complete working example with both methods

- [ ] **Task 1.4.5:** Write Custom Decorators section (if feasible)
  - Review [design specification](./design.md) for this file
  - Research custom decorator capability in Darker
  - If supported, create interface/base class example
  - Document registration approach
  - If not supported, note limitation

- [ ] **Task 1.4.6:** Write Decorator Patterns section
  - Review [design specification](./design.md) for this file
  - Create "Logging + Retry" pattern example
  - Create "Logging + Fallback + Retry" complete example from GetPersonQueryHandler
  - Create "Multiple Circuit Breakers" example

- [ ] **Task 1.4.7:** Write Configuring Polly Policies section
  - Review [design specification](./design.md) for this file
  - Document AddDefaultPolicies approach
  - Create custom policy registry example from DarkerSettings.cs
  - Document policy naming conventions
  - Show retry, circuit breaker, and timeout policies

- [ ] **Task 1.4.8:** Write Pipeline Context and Comparison sections
  - Review [design specification](./design.md) for this file
  - Research if Darker supports context bag between decorators
  - Document if available, note if not
  - Write comparison with Brighter pipeline (similarities and differences)

- [ ] **Task 1.4.9:** Write Best Practices, Common Pitfalls, and Further Reading
  - Review [design specification](./design.md) for this file
  - List best practices for decorator ordering and usage
  - List common pitfalls (ordering, policy naming, fallback implementation)
  - Add Further Reading links to Brighter policy docs

---

### Task Group 1.5: SUMMARY.md Updates

**Dependencies:** All Phase 1 files created (1.1-1.4 complete)

- [ ] **Task 1.5.1:** Create new Darker sections in SUMMARY.md
  - Add "## Darker Configuration" section
  - Add link to DarkerBasicConfiguration.md
  - Add "## Darker Query Handlers and Middleware Pipelines" section
  - Add links to QueriesAndQueryObjects.md, ImplementAQueryHandler.md, QueryPipeline.md
  - Add placeholder for QueryPatterns.md (Phase 2)
  - Verify proper indentation (spaces, not tabs)

---

## Phase 2: CQRS Concepts and Pattern

**Goal:** Add conceptual documentation and advanced patterns

### Task Group 2.1: CQRSWithBrighterAndDarker.md 

**Dependencies:** Phase 1 complete

- [ ] **Task 2.1.1:** Write Introduction and CQRS Fundamentals sections
  - Review [design specification](./design.md) for this file
  - Review BasicConcepts.md for CQS/CQRS definitions
  - Write "What is CQRS?" introduction
  - Document CQS principles (reference BasicConcepts.md)
  - Document CQRS principles (separate models)
  - Explain eventual consistency considerations

- [ ] **Task 2.1.2:** Write Brighter and Darker sections
  - Review [design specification](./design.md) for this file
  - Document what Brighter provides (command side)
  - Create brief command handler example with link to Brighter docs
  - Document what Darker provides (query side)
  - Create brief query handler example with link to Darker docs

- [ ] **Task 2.1.3:** Write Integrating Brighter and Darker section
  - Review [design specification](./design.md) for this file
  - Create ASCII architecture diagram showing both sides
  - Create folder structure example
  - Create combined DI setup example (AddBrighter + AddDarker)

- [ ] **Task 2.1.4:** Write Use Cases and Patterns sections
  - Review [design specification](./design.md) for this file
  - Create "Simple CQRS (Same Database)" pattern with example
  - Create "Separate Read/Write Databases" pattern with diagram
  - Create "Event-Sourced Writes, Projected Reads" pattern with diagram
  - Create "Task-Based UI" pattern with example

- [ ] **Task 2.1.5:** Write When to Use CQRS section
  - Review [design specification](./design.md) for this file
  - List good use cases (5-6 scenarios)
  - List when to avoid (4-5 scenarios)
  - Document benefits of Brighter + Darker
  - Document trade-offs (complexity, eventual consistency, learning curve)

- [ ] **Task 2.1.6:** Write Example: E-Commerce Order System section
  - Review [design specification](./design.md) for this file
  - Create write side example (PlaceOrderCommand)
  - Create read side example (GetOrderDetailsQuery)
  - Document complete flow (6 steps)
  - Show how both sides work together

- [ ] **Task 2.1.8:** Write Best Practices, Common Pitfalls, and Further Reading
  - List 6-8 best practices for CQRS
  - List 6-8 common pitfalls
  - Add Further Reading links to related docs

---

### Task Group 2.2: QueryPatterns.md

**Dependencies:** Phase 1 complete

- [ ] **Task 2.2.1:** Write Introduction and Parameterized Query Patterns sections
  - Review [design specification](./design.md) for this file
  - Write introduction to common query patterns
  - Create single entity lookup pattern (reference GetPersonNameQuery)
  - Create filtered list pattern example
  - Create search with criteria pattern example

- [ ] **Task 2.2.2:** Write Pagination Patterns section
  - Review [design specification](./design.md) for this file
  - Create offset-based pagination example with PagedResult<T>
  - Create cursor-based pagination example
  - Create keyset pagination example
  - Include query and handler for each

- [ ] **Task 2.2.3:** Write Projection Patterns section
  - Review [design specification](./design.md) for this file
  - Create simple projection example (subset of properties)
  - Create complex projection example (multiple sources)
  - Create calculated fields example

- [ ] **Task 2.2.4:** Write Collection and Aggregation Patterns sections
  - Review [design specification](./design.md) for this file
  - Create "Get All" small collection example
  - Create "Get Many with Filter" example
  - Create grouped results example
  - Create count query example
  - Create summary/statistics example
  - Create report query example

- [ ] **Task 2.2.5:** Write Entity Framework Core Integration section
  - Review [design specification](./design.md) for this file
  - Create IQueryable projection example with Select
  - Create Include/ThenInclude example for related data
  - Create AsNoTracking example for read optimization
  - Create compiled query example
  - Reference scoping requirement from DarkerBasicConfiguration.md

- [ ] **Task 2.2.6:** Write Caching, Multiple Data Source, and Performance Patterns
  - Review [design specification](./design.md) for this file
  - Create query result caching pattern example
  - Document cache invalidation approaches
  - Create multiple data source pattern example
  - Create dynamic sorting example
  - Document performance best practices

- [ ] **Task 2.2.7:** Write Real-World Example section
  - Review [design specification](./design.md) for this file
  - Create complete "Get Product Catalog" paginated example
  - Create complete "Search Products" with filters example
  - Create complete "Get Order Details" with related data example
  - Each example includes query, handler, and usage

- [ ] **Task 2.2.8:** Write Best Practices, Common Pitfalls, and Further Reading
  - Review [design specification](./design.md) for this file
  - List 8-10 best practices
  - List 8-10 common pitfalls
  - Document performance considerations
  - Add Further Reading links

---

### Task Group 2.3: Update Existing Documentation

**Dependencies:** Phase 1 and 2.1-2.2 complete

- [ ] **Task 2.3.1:** Update BasicConcepts.md
  - Review Query Processor section (lines 80-91)
  - Verify accuracy against implemented Darker docs
  - Add cross-reference to DarkerBasicConfiguration.md
  - Ensure consistent terminology
  - Verify no updates needed to other sections

- [ ] **Task 2.3.2:** Update Glossary.md
  - Add IQuery<TResult> term with definition
  - Add Query Object term with definition
  - Expand Query Handler definition (reference new docs)
  - Expand Query Processor definition (reference new docs)
  - Add Query Pipeline term with definition
  - Add Query Decorator term with definition
  - Ensure alphabetical ordering maintained

- [ ] **Task 2.3.3:** Update SUMMARY.md with Phase 2 files
  - Add link to QueryPatterns.md in "Darker Query Handlers" section
  - Add "## CQRS Patterns" section
  - Add link to CQRSWithBrighterAndDarker.md
  - Verify proper placement and indentation
  - Test all new links

---

**Task list complete and ready for implementation via `/spec:implement`**
