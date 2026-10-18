// Types and values EFCoreQueryIntegration.md names in its blocks and never declares.
//
// The page's handlers query an EF Core model it never shows, through an `ApplicationDbContext`.
// Block 4 continues the `builder` of the block before it. Each stub carries only the members the
// page's blocks use.
//
// blockcheck: using static EFCoreQueryIntegrationContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

public static class EFCoreQueryIntegrationContext
{
    // block 4: `builder.Services.AddDarker(…)`
    public static WebApplicationBuilder builder => null!;
}

// block 5: `context.Customers`, projected to `CustomerDto`. No block names the entity type, and a
// `DbSet<T>` needs a class, so the projection's type stands in for it
public class ApplicationDbContext : DbContext
{
    public DbSet<CustomerDto> Customers { get; set; } = null!;
}

// block 5: `new CustomerDto { Id = …, Name = …, Email = … }`
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// block 5: `QueryHandlerAsync<GetCustomerByIdQuery, CustomerDto?>`, `query.CustomerId`
public class GetCustomerByIdQuery : IQuery<CustomerDto?>
{
    public int CustomerId { get; set; }
}
