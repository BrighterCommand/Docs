---
description: "Query objects should validate their parameters to ensure they receive valid data."
layout:
  description:
    visible: false
---

# Query Object Validation

> **How-to** · Applies to **Darker V4** · Prerequisites: [Queries and Query Objects](/contents/QueriesAndQueryObjects.md)

Query objects should validate their parameters to ensure they receive valid data. Simple validation belongs in the constructor, while complex validation should be handled by the handler or a validation framework.

## Query Object Constructor Validation

Use guard clauses in the constructor for simple validation:

```csharp
using System;
using Paramore.Darker;

public sealed class GetOrdersPageQuery : IQuery<PagedResult<Order>>
{
    public GetOrdersPageQuery(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be positive");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be between 1 and 100");

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
}
```

## Query Object Validation Attributes

For ASP.NET scenarios, you can use data annotations that are validated by the framework:

```csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Paramore.Darker;

public sealed class SearchProductsQuery : IQuery<IReadOnlyList<Product>>
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SearchTerm { get; init; } = string.Empty;

    [Range(1, 1000)]
    public int MaxResults { get; init; } = 50;
}
```

ASP.NET Core checks these attributes when it binds the query from a request, but it rejects an invalid query for you in only two cases:

- A controller marked `[ApiController]` returns 400 before the action runs
- A minimal API endpoint returns 400 once you call `builder.Services.AddValidation()`, which needs .NET 10 or later

Anywhere else the invalid query reaches your code. A controller without `[ApiController]` runs the action with `ModelState.IsValid` set to `false`, so check it before you send the query; a minimal API without `AddValidation()` does not check the attributes at all.

## Where to Validate a Query Object

**Constructor validation (recommended for queries):**

- Parameter null checks
- Range validation for numeric values
- Format validation for strings
- Basic business invariants

**Handler validation (for complex rules):**

- Database existence checks
- Authorization checks
- Complex business rules
- Cross-field validation

**Framework validation (ASP.NET):**

- Model binding validation
- Data annotations
- Request validation

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Darker;

// Simple validation in constructor
public sealed class GetUserQuery : IQuery<User>
{
    public GetUserQuery(string email)
    {
        Email = !string.IsNullOrWhiteSpace(email)
            ? email
            : throw new ArgumentException("Email cannot be empty", nameof(email));
    }

    public string Email { get; }
}

// Complex validation in handler
public class GetUserQueryHandler : QueryHandlerAsync<GetUserQuery, User>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public override async Task<User> ExecuteAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        // Check if user exists (complex validation)
        var user = await _repository.FindByEmailAsync(query.Email, cancellationToken);

        if (user == null)
            throw new UserNotFoundException($"User with email {query.Email} not found");

        return user;
    }
}
```

## Further Reading

- [Queries and Query Objects](/contents/QueriesAndQueryObjects.md) - The query objects being validated
- [Query Result Types](/contents/QueryResultTypes.md) - What a validated query returns
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - Where an invalid query would otherwise arrive
