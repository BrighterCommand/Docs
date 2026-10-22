// Types and values DarkerBasicConfiguration.md names in its blocks and never declares.
//
// The page dispatches queries it never shows and registers handlers from assemblies of the
// reader's own. Blocks 6–8 continue the `builder` of the blocks before them. Each stub carries
// only the members the page's blocks use.
//
// blockcheck: using static DarkerBasicConfigurationContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

public static class DarkerBasicConfigurationContext
{
    // blocks 6–8: `builder.Services.AddDarker()`, continuing block 1's `builder`
    public static WebApplicationBuilder builder => null!;
}

// blocks 2, 3: `new GetPeopleQuery()`
public class GetPeopleQuery : IQuery<string[]>;

// blocks 2, 3: `new GetPersonNameQuery(id)`
public class GetPersonNameQuery(int id) : IQuery<string>;

// block 4: `Task<OrderDetails>`
public class OrderDetails;

// block 4: `new GetOrderDetailsQuery(orderId)`
public class GetOrderDetailsQuery(int orderId) : IQuery<OrderDetails>;

// block 5: `new GetCountQuery()`, whose result `GetCachedCount` returns as an `int`
public class GetCountQuery : IQuery<int>;

// block 10: `AddDbContext<ApplicationDbContext>(…)`
public class ApplicationDbContext : DbContext;

// block 11: `typeof(CustomerQueries.GetCustomerQuery).Assembly`, each in a module of the reader's
namespace CustomerQueries { public class GetCustomerQuery; }
namespace OrderQueries { public class GetOrderQuery; }
namespace ProductQueries { public class GetProductQuery; }
