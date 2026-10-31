// Types and values DefaultMessageMappers.md names in its blocks and never declares.
//
// The page registers the reader's own events and mappers against a `services` collection and posts
// through a command processor its host owns. Each value is typed from a pinned package and returns a
// default. A mapper stub derives from the default JSON mapper and adds nothing, so
// `mappers.Register<…, …>()` is checked against the real constraint. The page declares
// `OrderShipped` and `AvroMessageMapperAsync<T>` (blocks 4 and 5), which no unit may supply.
//
// blockcheck: using static DefaultMessageMappersContext;

using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.MessageMappers;

public static class DefaultMessageMappersContext
{
    // blocks 1, 3, 7–9, 11, 12, 15: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;

    // block 2: `await _commandProcessor.PostAsync(new OrderCreated { … })`
    public static IAmACommandProcessor _commandProcessor => null!;
}

// blocks 1–3, 7–9, 11, 12, 15: `typeof(OrderCreated).Assembly`; block 2:
// `new OrderCreated { Id = …, CustomerId = …, Total = … }`
public class OrderCreated() : Event(Id.Random())
{
    public string? CustomerId { get; set; }
    public decimal Total { get; set; }
}

// block 15: `typeof(CustomerCreated).Assembly`
public class CustomerCreated() : Event(Id.Random());

// blocks 7, 11: `mappers.Register<LargeOrder, CompressedOrderMapper>()`
public class LargeOrder() : Event(Id.Random());
public class CompressedOrderMapper : JsonMessageMapper<LargeOrder>;

// block 7: `mappers.Register<SensitiveOrder, SecureLargeOrderMapper>()`
public class SensitiveOrder() : Event(Id.Random());
public class SecureLargeOrderMapper : JsonMessageMapper<SensitiveOrder>;

// block 11: `mappers.Register<SensitiveData, EncryptedMapper>()`
public class SensitiveData() : Event(Id.Random());
public class EncryptedMapper : JsonMessageMapper<SensitiveData>;

// block 14: `MapToMessage(LargeEvent request, …)`, `request.Id`
public class LargeEvent() : Event(Id.Random());
