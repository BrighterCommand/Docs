// Types and values MigratingToNullableReferenceTypes.md names in its blocks and never declares.
//
// Identifiers only for the values: every member is typed from a pinned package or the BCL, returns
// a default, and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails. The types are the reader's
// own command and service, with only the members a block names.
//
// blockcheck: using static MigratingToNullableReferenceTypesContext;

using Paramore.Brighter;

public static class MigratingToNullableReferenceTypesContext
{
    public static string name => null!;
    // block 8: `string? name = GetName();`, a method that may return null
    public static string? GetName() => null;
}

// block 21: `RequestHandler<ProcessOrderCommand>`, `command.OrderId`
public class ProcessOrderCommand() : Command(Id.Random())
{
    public string? OrderId { get; set; }
}

// block 21: `_orderService.ProcessOrder(command.OrderId)`
public interface IOrderService
{
    void ProcessOrder(string orderId);
}
