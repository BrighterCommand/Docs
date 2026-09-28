// Types CQRSUseCasesAndPatterns.md names in its blocks and never declares.
//
// Block 1 declares the write model, read model, query and context; the order's items, its
// customer and its status are the domain around them, which the page never shows. Block 2's query
// returns a DTO it never shows. Each stub carries only the members a block names, and none names
// a type a block declares, so the unit compiles beside either block.

// block 1: `new OrderItem(productId, quantity, price)`, `i.Price * i.Quantity`. No block reads
// `productId` back, so it is a parameter and not a property
public class OrderItem(int productId, int quantity, decimal price)
{
    public int Quantity { get; } = quantity;
    public decimal Price { get; } = price;
}

// block 1: `o.Customer.Name`
public class Customer
{
    public string Name { get; set; } = "";
}

// block 1: `OrderStatus.Shipped`, `OrderStatus.Cancelled`
public enum OrderStatus { Shipped, Cancelled }

// block 2: `IQuery<OrderApprovalDto>`
public class OrderApprovalDto { }
