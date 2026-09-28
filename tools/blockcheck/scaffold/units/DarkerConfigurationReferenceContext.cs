// Types and values DarkerConfigurationReference.md names in its blocks and never declares.
//
// The page registers the query handlers Implementing a Query Handler writes, so it names the queries
// and handlers and never shows them. A handler stub is a handler of its query and nothing more:
// abstract, so it need not declare the `Execute` no block names.
//
// blockcheck: using static DarkerConfigurationReferenceContext;

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Paramore.Darker;

public static class DarkerConfigurationReferenceContext
{
    // block 3: `builder.Services.AddDarker()`
    public static WebApplicationBuilder builder => null!;
}

// blocks 3, 4: `typeof(GetPeopleQuery).Assembly`, `registry.Register<GetPeopleQuery, …>()`
public class GetPeopleQuery : IQuery<IReadOnlyDictionary<int, string>> { }

// block 3: `typeof(GetOrdersQuery).Assembly`
public class GetOrdersQuery { }

// block 4: `registry.Register<GetPersonNameQuery, string, GetPersonQueryHandler>()`
public class GetPersonNameQuery : IQuery<string> { }

// block 4: `registry.Register<GetPeopleQuery, IReadOnlyDictionary<int, string>, GetPeopleQueryHandler>()`
public abstract class GetPeopleQueryHandler : QueryHandler<GetPeopleQuery, IReadOnlyDictionary<int, string>> { }

// block 4: `registry.Register<GetPersonNameQuery, string, GetPersonQueryHandler>()`
public abstract class GetPersonQueryHandler : QueryHandler<GetPersonNameQuery, string> { }
