---
description: "Query handlers are easy to test because they have clear inputs (queries) and outputs (results), with dependencies that can be mocked or replaced."
layout:
  description:
    visible: false
---

# Testing Query Handlers

> **How-to** · Applies to **Darker V4** · Prerequisites: [Implementing a Query Handler](/contents/ImplementAQueryHandler.md)

Query handlers are easy to test because they have clear inputs (queries) and outputs (results), with dependencies that can be mocked or replaced.

## Query Handler Test-Driven Development

Use Test-Driven Development (TDD) to design query handlers:

```csharp
using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;

public class GetOrderQueryHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidOrderId_ReturnsOrder()
    {
        // Arrange
        var expectedOrder = new Order { Id = 123, CustomerName = "John Doe" };
        var mockRepository = new Mock<IOrderRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        var handler = new GetOrderQueryHandler(mockRepository.Object);
        var query = new GetOrderQuery(123);

        // Act
        var result = await handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("John Doe", result.CustomerName);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidOrderId_ThrowsNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new GetOrderQueryHandler(mockRepository.Object);
        var query = new GetOrderQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            handler.ExecuteAsync(query, CancellationToken.None));
    }
}
```

## Replacing Query Handler Dependencies with In-Memory Solutions

For integration tests, replace real dependencies with in-memory alternatives:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

public class GetCustomerQueryHandlerIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetCustomerQueryHandler _handler;

    public GetCustomerQueryHandlerIntegrationTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed test data
        _dbContext.Customers.Add(new Customer { Id = 1, Name = "Test Customer" });
        _dbContext.SaveChanges();

        _handler = new GetCustomerQueryHandler(_dbContext);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var query = new GetCustomerQuery(1);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.Name);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
}
```

## Query Handler Acceptance Tests

For acceptance tests, use a real database to verify the entire query flow:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

[Collection("Database")]
public class OrderQueryAcceptanceTests
{
    private readonly IQueryProcessor _queryProcessor;
    private readonly TestDatabase _database;

    public OrderQueryAcceptanceTests(DatabaseFixture fixture)
    {
        _database = fixture.Database;
        _queryProcessor = fixture.ServiceProvider.GetRequiredService<IQueryProcessor>();

        // Seed test data
        _database.SeedOrders();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsCompleteOrderDetails()
    {
        // Arrange
        var query = new GetOrderQuery(1);

        // Act
        var result = await _queryProcessor.ExecuteAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotEmpty(result.Items);
        Assert.True(result.Total > 0);
    }
}
```

## Further Reading

- [Implementing a Query Handler](/contents/ImplementAQueryHandler.md) - The handler being tested
- [Query Handler Dependencies](/contents/QueryHandlerDependencies.md) - The dependencies a test substitutes
- [Basic Configuration](/contents/DarkerBasicConfiguration.md) - Registering handlers for an acceptance test
