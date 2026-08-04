# Nullable Reference Types

> **Reference** · Applies to **Brighter V10**

V10 enables [nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) across all Brighter projects, providing improved type safety and helping prevent null reference exceptions.

---

## Understanding Nullable Reference Types

### Non-Nullable by Default

In C# with nullable reference types enabled, reference types are non-nullable by default:

```csharp
// This string cannot be null
string name = "John";

// Compiler warning: Cannot convert null to non-nullable reference type
string name = null; // ⚠️ CS8600
```

### Nullable Types

Use `?` to explicitly mark types as nullable:

```csharp
// This string can be null
string? optionalName = null; // ✅ OK

// Must check for null before using
if (optionalName != null)
{
    Console.WriteLine(optionalName.Length);
}
```

---

## Impact on Brighter Code

### Commands and Events

**Required properties** should be non-nullable:

```csharp
public class CreateOrderCommand : Command
{
    public Guid OrderId { get; }
    public string CustomerName { get; }      // Non-nullable: required
    public decimal TotalAmount { get; }
    public string? Notes { get; }             // Nullable: optional

    public CreateOrderCommand(
        Guid orderId,
        string customerName,
        decimal totalAmount,
        string? notes = null)
        : base(Guid.NewGuid())
    {
        OrderId = orderId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Notes = notes;
    }
}
```

### Handlers

Handler dependencies should declare nullability appropriately:

```csharp
public class CreateOrderCommandHandler : RequestHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<CreateOrderCommandHandler> logger)
    {
        // Dependencies are non-nullable - null check not needed
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public override CreateOrderCommand Handle(CreateOrderCommand command)
    {
        // command parameter is non-nullable
        var order = new Order
        {
            Id = command.OrderId,
            CustomerName = command.CustomerName,
            TotalAmount = command.TotalAmount,
            Notes = command.Notes  // Can be null
        };

        _orderRepository.Add(order);

        return base.Handle(command);
    }
}
```

### Message Mappers

Message mappers should handle nullable message properties:

```csharp
public class CreateOrderCommandMessageMapper : IAmAMessageMapper<CreateOrderCommand>
{
    public Message MapToMessage(CreateOrderCommand request, string? topic = null)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: topic ?? "orders.create",
            messageType: MessageType.MT_COMMAND);

        var body = new MessageBody(JsonSerializer.Serialize(request));

        return new Message(header, body);
    }

    public CreateOrderCommand MapToRequest(Message message)
    {
        // Deserialize may return null
        var dto = JsonSerializer.Deserialize<CreateOrderDto>(message.Body.Value);

        // Handle potential null
        if (dto == null)
            throw new ArgumentException("Failed to deserialize message body", nameof(message));

        return new CreateOrderCommand(
            orderId: dto.OrderId,
            customerName: dto.CustomerName ?? throw new ArgumentException("CustomerName is required"),
            totalAmount: dto.TotalAmount,
            notes: dto.Notes  // Nullable
        );
    }
}
```

---

## V10 Changes

> **BREAKING CHANGE**: Nullable reference types are now enabled across all Brighter projects. You may need to update your code to address compiler warnings.

### What Changed

- **All Brighter projects** now have `<Nullable>enable</Nullable>` in their project files
- **Required properties** are now non-nullable by default
- **Optional properties** are explicitly marked as nullable with `?`
- **Compiler warnings** (CS8600-CS8629) will appear for potential null reference issues

### Benefits

1. **Compile-Time Safety**: Catch potential null reference issues before runtime
2. **Clear Intent**: Explicitly declare which properties can be null
3. **Better Documentation**: API signatures clearly show nullability expectations
4. **Reduced Runtime Errors**: Fewer `NullReferenceException` errors in production

---

## Migration Guide

### Step 1: Enable Nullable Reference Types

If you haven't already, enable nullable reference types in your project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Step 2: Address Compiler Warnings

After enabling nullable reference types, you'll see compiler warnings. Address them systematically:

#### CS8600: Converting null literal or possible null value to non-nullable type

**Problem**:

```csharp
string name = null; // ⚠️ CS8600
```

**Solutions**:

**Option 1**: Make the type nullable:

```csharp
string? name = null; // ✅
```

**Option 2**: Provide a non-null value:

```csharp
string name = "default"; // ✅
```

#### CS8601: Possible null reference assignment

**Problem**:

```csharp
public class CreateOrderCommand : Command
{
    public string CustomerName { get; set; }
}

var command = new CreateOrderCommand();
// CustomerName is null but declared as non-nullable
```

**Solutions**:

**Option 1**: Make property nullable if it can be null:

```csharp
public string? CustomerName { get; set; }
```

**Option 2**: Initialize with a default value:

```csharp
public string CustomerName { get; set; } = string.Empty;
```

**Option 3**: Use required keyword (C# 11+):

```csharp
public required string CustomerName { get; set; }

// Must be initialized:
var command = new CreateOrderCommand
{
    CustomerName = "John" // Required
};
```

#### CS8602: Dereference of a possibly null reference

**Problem**:

```csharp
string? name = GetName();
int length = name.Length; // ⚠️ CS8602: name might be null
```

**Solutions**:

**Option 1**: Null check:

```csharp
if (name != null)
{
    int length = name.Length; // ✅
}
```

**Option 2**: Null-conditional operator:

```csharp
int? length = name?.Length; // ✅ Returns null if name is null
```

**Option 3**: Null-coalescing operator:

```csharp
string safeName = name ?? "default";
int length = safeName.Length; // ✅
```

**Option 4**: Null-forgiving operator (use cautiously):

```csharp
int length = name!.Length; // ⚠️ Asserts name is not null (throws at runtime if wrong)
```

#### CS8603: Possible null reference return

**Problem**:

```csharp
public string GetName()
{
    return null; // ⚠️ CS8603
}
```

**Solutions**:

**Option 1**: Make return type nullable:

```csharp
public string? GetName()
{
    return null; // ✅
}
```

**Option 2**: Return a non-null value:

```csharp
public string GetName()
{
    return "default"; // ✅
}
```

#### CS8618: Non-nullable field must contain a non-null value when exiting constructor

**Problem**:

```csharp
public class Order
{
    public string CustomerName { get; set; } // ⚠️ CS8618
}
```

**Solutions**:

**Option 1**: Initialize in constructor:

```csharp
public class Order
{
    public string CustomerName { get; set; }

    public Order(string customerName)
    {
        CustomerName = customerName; // ✅
    }
}
```

**Option 2**: Initialize with default value:

```csharp
public class Order
{
    public string CustomerName { get; set; } = string.Empty; // ✅
}
```

**Option 3**: Make nullable if appropriate:

```csharp
public class Order
{
    public string? CustomerName { get; set; } // ✅
}
```

**Option 4**: Use required keyword (C# 11+):

```csharp
public class Order
{
    public required string CustomerName { get; set; } // ✅
}
```

### Step 3: Update Handler Code

Review your handlers for null safety:

```csharp
public class ProcessOrderHandler : RequestHandler<ProcessOrderCommand>
{
    private readonly IOrderService _orderService;

    public ProcessOrderHandler(IOrderService orderService)
    {
        // V10: Add null check for dependencies
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    public override ProcessOrderCommand Handle(ProcessOrderCommand command)
    {
        // V10: command is non-nullable, but properties might be
        if (string.IsNullOrEmpty(command.OrderId))
            throw new ArgumentException("OrderId is required", nameof(command));

        _orderService.ProcessOrder(command.OrderId);

        return base.Handle(command);
    }
}
```

### Step 4: Update Message Mappers

Ensure message mappers handle deserialization nullability:

```csharp
public class OrderEventMessageMapper : IAmAMessageMapper<OrderCreatedEvent>
{
    public OrderCreatedEvent MapToRequest(Message message)
    {
        // Deserialization can return null
        var dto = JsonSerializer.Deserialize<OrderDto>(message.Body.Value);

        // V10: Handle null explicitly
        if (dto == null)
            throw new InvalidOperationException("Failed to deserialize message");

        // Validate required properties
        if (string.IsNullOrEmpty(dto.OrderId))
            throw new ArgumentException("OrderId is required");

        return new OrderCreatedEvent(
            orderId: dto.OrderId,
            customerName: dto.CustomerName ?? "Unknown", // Handle nullable
            createdAt: dto.CreatedAt
        );
    }
}
```

---

## Best Practices

### 1. Use Non-Nullable for Required Properties

Make your intent clear by using non-nullable types for properties that should never be null:

```csharp
public class CreateUserCommand : Command
{
    public string Email { get; }           // Required - non-nullable
    public string FirstName { get; }       // Required - non-nullable
    public string LastName { get; }        // Required - non-nullable
    public string? MiddleName { get; }     // Optional - nullable

    public CreateUserCommand(
        string email,
        string firstName,
        string lastName,
        string? middleName = null)
        : base(Guid.NewGuid())
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        MiddleName = middleName;
    }
}
```

### 2. Validate at Boundaries

Validate nullability at system boundaries (message mappers, API controllers):

```csharp
public class CreateUserCommandMapper : IAmAMessageMapper<CreateUserCommand>
{
    public CreateUserCommand MapToRequest(Message message)
    {
        var dto = JsonSerializer.Deserialize<CreateUserDto>(message.Body.Value);

        // Validate at boundary
        if (dto == null)
            throw new InvalidOperationException("Message body is empty");

        if (string.IsNullOrEmpty(dto.Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrEmpty(dto.FirstName))
            throw new ArgumentException("FirstName is required");

        if (string.IsNullOrEmpty(dto.LastName))
            throw new ArgumentException("LastName is required");

        return new CreateUserCommand(
            email: dto.Email,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            middleName: dto.MiddleName
        );
    }
}
```

### 3. Use Null-Coalescing for Defaults

Use `??` operator to provide sensible defaults:

```csharp
public class OrderQuery
{
    public int PageSize { get; }
    public int PageNumber { get; }

    public OrderQuery(int? pageSize = null, int? pageNumber = null)
    {
        // Provide defaults for nullable parameters
        PageSize = pageSize ?? 10;
        PageNumber = pageNumber ?? 1;
    }
}
```

### 4. Document Nullability in XML Comments

Make nullability expectations explicit in documentation:

```csharp
/// <summary>
/// Creates a new order.
/// </summary>
/// <param name="customerId">The customer ID (required, non-null)</param>
/// <param name="notes">Optional notes (can be null)</param>
public class CreateOrderCommand : Command
{
    public Guid CustomerId { get; }
    public string? Notes { get; }
}
```

### 5. Avoid Null-Forgiving Operator Unless Certain

Use the null-forgiving operator (`!`) sparingly:

```csharp
// ⚠️ Avoid unless you're certain
var name = user.Name!; // Asserts Name is not null

// ✅ Better: Check explicitly
if (user.Name == null)
    throw new InvalidOperationException("User name is required");
var name = user.Name;
```

### 6. Use Pattern Matching for Null Checks

Modern C# pattern matching provides concise null checks:

```csharp
// Traditional
if (command.Notes != null)
{
    ProcessNotes(command.Notes);
}

// Pattern matching
if (command.Notes is { } notes)
{
    ProcessNotes(notes);
}

// Null-coalescing
var notes = command.Notes ?? "No notes provided";
```

### 7. Consider Required Members (C# 11+)

Use `required` keyword for properties that must be initialized:

```csharp
public class OrderDto
{
    public required string OrderId { get; init; }
    public required string CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string? Notes { get; init; }
}

// Must initialize required properties
var dto = new OrderDto
{
    OrderId = "123",      // Required
    CustomerId = "456",   // Required
    Amount = 100.0m,
    Notes = null          // Optional
};
```

---

## Common Patterns in Brighter

### Command with Validation

```csharp
public class UpdateProductCommand : Command
{
    public Guid ProductId { get; }
    public string Name { get; }
    public decimal? Price { get; }  // Nullable: only update if provided

    public UpdateProductCommand(
        Guid productId,
        string name,
        decimal? price = null)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price;
    }
}
```

### Event with Optional Properties

```csharp
public class OrderCompletedEvent : Event
{
    public Guid OrderId { get; }
    public DateTime CompletedAt { get; }
    public string? CompletedBy { get; }  // Nullable: may be system-generated

    public OrderCompletedEvent(
        Guid orderId,
        DateTime completedAt,
        string? completedBy = null)
        : base(Guid.NewGuid())
    {
        OrderId = orderId;
        CompletedAt = completedAt;
        CompletedBy = completedBy;
    }
}
```

### Handler with Optional Dependencies

```csharp
public class SendEmailHandler : RequestHandler<SendEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailHandler>? _logger;  // Optional

    public SendEmailHandler(
        IEmailService emailService,
        ILogger<SendEmailHandler>? logger = null)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger;  // Can be null
    }

    public override SendEmailCommand Handle(SendEmailCommand command)
    {
        _logger?.LogInformation("Sending email to {Recipient}", command.Recipient);

        _emailService.SendEmail(command.Recipient, command.Subject, command.Body);

        return base.Handle(command);
    }
}
```

---

## Troubleshooting

### Warning: Treat as Errors

Some teams treat warnings as errors. If you're seeing build failures:

```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

You must address all nullable warnings before the build succeeds.

### Suppressing Warnings (Not Recommended)

If you must temporarily suppress warnings (not recommended for production code):

```csharp
#pragma warning disable CS8600
string name = null; // Warning suppressed
#pragma warning restore CS8600
```

### Gradual Migration

For large codebases, consider enabling nullable reference types gradually:

```xml
<!-- Start with warnings only -->
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
</PropertyGroup>
```

Then address warnings incrementally and eventually enable `TreatWarningsAsErrors`.

---

## Additional Resources

- [Microsoft: Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Microsoft: Nullable Reference Types Tutorial](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/nullable-reference-types)
- [Microsoft: Nullable Attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis)
- [Brighter Commands and Events](/contents/Requests%2C%20Commands%20and%20Events.md)
- [Brighter Handlers](ImplementingAHandler.md)
- [Brighter Message Mappers](MessageMappers.md)
