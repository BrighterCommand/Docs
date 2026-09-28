---
description: "The options AddDarker and AddHandlersFromAssemblies take: the query processor's service lifetime, and the strategies available for registering handlers."
layout:
  description:
    visible: false
---

# Darker Configuration Reference

> **Reference** · Applies to **Darker V4** · Prerequisites: [Basic Configuration](/contents/DarkerBasicConfiguration.md)

The options `AddDarker` and `AddHandlersFromAssemblies` take: the query processor's service lifetime, and the strategies available for registering handlers. For the configuration these options sit inside, see [Basic Configuration](/contents/DarkerBasicConfiguration.md).

## Darker Query Processor Lifetime

By default, the `IQueryProcessor` is registered with a **Singleton** lifetime, meaning one instance serves the whole application, and it resolves your handlers from the root service provider. However, if you're using Entity Framework Core, you need to register the Query Processor with a **Scoped** lifetime to match the EF Core DbContext lifetime.

**Default Configuration (Singleton):**

```csharp
using Paramore.Darker.AspNetCore;

builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(Program).Assembly);
```

**Scoped Configuration (Required for EF Core):**

When using Entity Framework Core, the DbContext is registered as scoped by default. To ensure Darker works correctly with EF Core, you must configure the Query Processor to use the same scoped lifetime:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;

builder.Services.AddDarker(options =>
{
    // EFCore registers DbContext as scoped
    // Query Processor must also be scoped to work with EF Core
    options.QueryProcessorLifetime = ServiceLifetime.Scoped;
})
.AddHandlersFromAssemblies(typeof(Program).Assembly);
```

If you don't configure the scoped lifetime when using EF Core, the singleton query processor resolves each handler, and so its `DbContext`, from the root provider. Where scope validation is on, as it is by default in the Development environment, the first query throws an `InvalidOperationException`, *"Cannot resolve … from root provider because it requires scoped service"*. Elsewhere every query shares one `DbContext` for the life of the application.

## Darker Handler Registration Strategies

Darker provides two ways to register query handlers: automatic assembly scanning (recommended) and manual registration.

**Assembly Scanning (Recommended):**

The `AddHandlersFromAssemblies` method scans one or more assemblies and automatically registers all query handlers it finds:

```csharp
using Paramore.Darker.AspNetCore;

// Scan a single assembly
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(typeof(GetPeopleQuery).Assembly);

// Scan multiple assemblies
builder.Services.AddDarker()
    .AddHandlersFromAssemblies(
        typeof(GetPeopleQuery).Assembly,
        typeof(GetOrdersQuery).Assembly);
```

This approach follows convention over configuration and is the easiest way to register handlers.

**Manual Registration:**

For more control over handler registration, you can use `QueryHandlerRegistry` to register handlers explicitly:

```csharp
using System;
using System.Collections.Generic;
using Paramore.Darker;
using Paramore.Darker.Builder;

var registry = new QueryHandlerRegistry();
registry.Register<GetPeopleQuery, IReadOnlyDictionary<int, string>, GetPeopleQueryHandler>();
registry.Register<GetPersonNameQuery, string, GetPersonQueryHandler>();

IQueryProcessor queryProcessor = QueryProcessorBuilder.With()
    .Handlers(
        registry,
        t => (IQueryHandler)Activator.CreateInstance(t)!,
        t => { },
        t => (IQueryHandlerDecorator)Activator.CreateInstance(t)!)
    .InMemoryQueryContextFactory()
    .Build();
```

`Handlers` takes a factory for handlers and one for decorators, each a `Func<Type, …>` returning Darker's interface, so `Activator.CreateInstance`, which returns `object`, needs a cast; it also needs each handler to have a parameterless constructor. Manual registration is useful when you need fine-grained control over which handlers are registered or when you're not using ASP.NET Core's dependency injection.

## Further Reading

- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - The configuration these options sit inside
- [Query Pipeline and Decorators](/contents/QueryPipeline.md) - Configuring decorators and policies
- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - The handlers being registered
