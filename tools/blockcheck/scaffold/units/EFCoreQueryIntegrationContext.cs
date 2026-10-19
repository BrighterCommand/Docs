// Types and values EFCoreQueryIntegration.md names in its blocks and never declares.
//
// The page's handlers query an EF Core model it never shows, through an `ApplicationDbContext`.
// Blocks 1–4 are an `ExecuteAsync` shown alone, reading the context its handler is given; block 5
// continues the `builder` of the block before it. Each stub carries only the members the page's
// blocks use. Where no block names the type of an entity, only its members, it is a tuple.
//
// blockcheck: using static EFCoreQueryIntegrationContext;

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

public static class EFCoreQueryIntegrationContext
{
    // block 5: `builder.Services.AddDarker(…)`
    public static WebApplicationBuilder builder => null!;
    // blocks 1–4: `_dbContext.Products`, `_dbContext.Orders`
    public static ApplicationDbContext _dbContext => null!;
}

// block 6: `context.Customers`, projected to `CustomerDto`. No block names the entity type, and a
// `DbSet<T>` needs a class, so the projection's type stands in for it
public class ApplicationDbContext : DbContext
{
    public DbSet<CustomerDto> Customers { get; set; } = null!;
    // blocks 1, 2: `_dbContext.Products`
    public DbSet<Product> Products { get; set; } = null!;
    // blocks 3, 4: `_dbContext.Orders`
    public DbSet<Order> Orders { get; set; } = null!;
}

// blocks 1, 2: `List<Product>`; `p.Id`, `p.Name`; block 3: `i.Product.Name`
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// block 1: `new ProductDto { Id = p.Id, Name = p.Name }`
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// block 1: `ExecuteAsync(GetProductsQuery …)`; block 2: `ExecuteAsync(GetProductEntitiesQuery …)`
public class GetProductsQuery : IQuery<List<ProductDto>>;

public class GetProductEntitiesQuery : IQuery<List<Product>>;

// blocks 3, 4: "each Order with its Customer"; `o.Id`, `o.Customer.Name`, `o.Items`, `i.Product`
public class Order
{
    public int Id { get; set; }
    public (int Id, string Name) Customer { get; set; }
    public List<(int Quantity, Product Product)> Items { get; set; } = new();
}

// blocks 3, 4: `ExecuteAsync(GetOrderQuery …)`, `query.OrderId`
public class GetOrderQuery : IQuery<OrderDto?>
{
    public int OrderId { get; set; }
}

// blocks 3, 4: `new OrderDto { OrderId, CustomerName, Items, ItemCount }`
public class OrderDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public List<OrderItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
}

// block 3: `new OrderItemDto { ProductName = … }`
public class OrderItemDto
{
    public string ProductName { get; set; } = "";
}

// block 6: `new CustomerDto { Id = …, Name = …, Email = … }`
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// block 6: `QueryHandlerAsync<GetCustomerByIdQuery, CustomerDto?>`, `query.CustomerId`
public class GetCustomerByIdQuery : IQuery<CustomerDto?>
{
    public int CustomerId { get; set; }
}
