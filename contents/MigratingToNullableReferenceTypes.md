# Migrating to Nullable Reference Types

> **How-to** · Applies to **Brighter V10** · Prerequisites: [Nullable Reference Types](/contents/NullableReferenceTypes.md)

Four steps to turn nullable reference types on in a project that uses Brighter: enable the
feature, work through the compiler warnings it raises, and update your handlers and message
mappers for the annotations. [Nullable Reference Types](/contents/NullableReferenceTypes.md)
explains what the annotations mean and how Brighter's own API is annotated.

## Step 1: Enable Nullable Reference Types

If you haven't already, enable nullable reference types in your project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

## Step 2: Address Nullable Compiler Warnings

After enabling nullable reference types, you'll see compiler warnings. Address them systematically:

### CS8600: Converting null literal or possible null value to non-nullable type

**Problem**:

```csharp
// ...
string name = null; // ⚠️ CS8600
```

**Solutions**:

**Option 1**: Make the type nullable:

```csharp
// ...
string? name = null; // ✅
```

**Option 2**: Provide a non-null value:

```csharp
// ...
string name = "default"; // ✅
```

### CS8601: Possible null reference assignment

**Problem**:

```csharp
// ...
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
// ...
public string? CustomerName { get; set; }
```

**Option 2**: Initialize with a default value:

```csharp
// ...
public string CustomerName { get; set; } = string.Empty;
```

**Option 3**: Use required keyword (C# 11+):

```csharp
// ...
public required string CustomerName { get; set; }

// Must be initialized:
var command = new CreateOrderCommand
{
    CustomerName = "John" // Required
};
```

### CS8602: Dereference of a possibly null reference

**Problem**:

```csharp
// ...
string? name = GetName();
int length = name.Length; // ⚠️ CS8602: name might be null
```

**Solutions**:

**Option 1**: Null check:

```csharp
// ...
if (name != null)
{
    int length = name.Length; // ✅
}
```

**Option 2**: Null-conditional operator:

```csharp
// ...
int? length = name?.Length; // ✅ Returns null if name is null
```

**Option 3**: Null-coalescing operator:

```csharp
// ...
string safeName = name ?? "default";
int length = safeName.Length; // ✅
```

**Option 4**: Null-forgiving operator (use cautiously):

```csharp
// ...
int length = name!.Length; // ⚠️ Asserts name is not null (throws at runtime if wrong)
```

### CS8603: Possible null reference return

**Problem**:

```csharp
// ...
public string GetName()
{
    return null; // ⚠️ CS8603
}
```

**Solutions**:

**Option 1**: Make return type nullable:

```csharp
// ...
public string? GetName()
{
    return null; // ✅
}
```

**Option 2**: Return a non-null value:

```csharp
// ...
public string GetName()
{
    return "default"; // ✅
}
```

### CS8618: Non-nullable field must contain a non-null value when exiting constructor

**Problem**:

```csharp
// ...
public class Order
{
    public string CustomerName { get; set; } // ⚠️ CS8618
}
```

**Solutions**:

**Option 1**: Initialize in constructor:

```csharp
// ...
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
// ...
public class Order
{
    public string CustomerName { get; set; } = string.Empty; // ✅
}
```

**Option 3**: Make nullable if appropriate:

```csharp
// ...
public class Order
{
    public string? CustomerName { get; set; } // ✅
}
```

**Option 4**: Use required keyword (C# 11+):

```csharp
// ...
public class Order
{
    public required string CustomerName { get; set; } // ✅
}
```

## Step 3: Update Handler Code for Nullability

Review your handlers for null safety:

```csharp
// ...
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

## Step 4: Update Message Mappers for Nullability

Ensure message mappers handle deserialization nullability:

```csharp
// ...
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

## Further Reading

- [Nullable Reference Types](/contents/NullableReferenceTypes.md) - What the annotations mean, and how Brighter's API uses them
- [V10 Migration Guide](/contents/V10MigrationGuide.md) - The wider V10 upgrade
- [Microsoft: Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
