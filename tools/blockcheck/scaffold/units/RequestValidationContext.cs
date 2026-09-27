// Types and values RequestValidation.md names in its blocks and never shows.
//
// The page validates a `GreetingCommand` and a `PlaceOrder` it assumes rather than defines, and
// its registration and dispatch snippets run against a `builder` and a `commandProcessor` it
// never creates. Each stub carries only the members the page's blocks use. Values return a
// default and do nothing; a block that calls a member of one is checked against the real type.
//
// blockcheck: using static RequestValidationContext;

using Microsoft.AspNetCore.Builder;
using Paramore.Brighter;

public static class RequestValidationContext
{
    // blocks 4 and 8: `builder.Services`
    public static WebApplicationBuilder builder => null!;
    // block 12: `commandProcessor.Send(command)`
    public static IAmACommandProcessor commandProcessor => null!;
    public static IRequest command => null!;
}

// block 1: `command.Name`; block 6: `RuleFor(command => command.Name)`, `command.Email`
public class GreetingCommand() : Command(Id.Random())
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// block 9: `order.Quantity > 0`, `order.Sku`
public class PlaceOrder() : Command(Id.Random())
{
    public int Quantity { get; set; }
    public string Sku { get; set; } = string.Empty;
}
