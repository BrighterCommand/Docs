# Default Message Mappers

> **How-to** · Applies to **Brighter V10**

## Using Default Message Mappers

You do not need to implement `IAmAMessageMapper` for every message type.

In earlier versions, every message sent via an external bus required a message mapper implementation. This created duplicated code when in many cases, what was required was to take details from your `Publication` and then serialize the body to JSON.

To prevent this, Brighter provides **default message mappers** that handle common serialization patterns automatically. For most use cases, you can use a default and only create custom mappers when you need specialized message transformation.

## Default Mappers Provided

Brighter includes two default message mappers for JSON serialization:

### 1. `JsonMessageMapper<T>` (Binary-Mode CloudEvents)

The **`JsonMessageMapper<T>`** is the default mapper that uses JSON serialization with **binary-mode CloudEvents**.

**Characteristics:**

- Serializes your Request (Command/Event) to JSON
- Uses binary-mode CloudEvents (attributes in headers).
- Populate CloudEvents headers from your Publication.
- The default when using `AutoFromAssemblies()` or `MapperRegistry` when configuring Brighter, but you can override with your own `defaultMessageMapper` or `asyncDefaultMessageMapper` instead.

**CloudEvents Mode:** Binary (recommended for protocols with header support like RabbitMQ, Kafka)

### 2. `CloudEventJsonMessageMapper<T>` (Structured-Mode CloudEvents)

The **`CloudEventJsonMessageMapper<T>`** uses JSON serialization with **structured-mode CloudEvents**.

**Characteristics:**

- Serializes your Request to JSON
- Uses structured-mode CloudEvents (attributes in JSON body)
- Populate CloudEvents headers from your Publication.
- Can be set as default mapper
- Useful for protocols without header support or with constrained header support (small number of headers)

**CloudEvents Mode:** Structured (recommended for AWS SNS/SQS)

## How Default Mappers Work

Brighter looks for first for an explicit `IAmAMessageMapper<T>` registration. If none is found, it falls back to the default mapper.

### Automatic Usage (No Registration Needed)

This is the simplest and recommended approach for most scenarios:

```csharp
services.AddBrighter(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
})
.AddProducers(configure =>
{
    configure.ProducerRegistry = new RabbitMQProducerRegistryFactory(
        new RmqMessagingGatewayConnection { /* ... */ },
        [
            new Publication
            {
                Topic = new RoutingKey("orders.created"),
                RequestType = typeof(OrderCreated),
                Source = new Uri("https://example.com/orders"),
                Type = new CloudEventsType("com.example.order.created")
            }
        ]
    ).Create();
})
.AutoFromAssemblies([typeof(OrderCreated).Assembly]);

// No message mapper registration needed!
// Brighter will use JsonMessageMapper<OrderCreated> automatically
```

When you publish an `OrderCreated` event:

```csharp
await _commandProcessor.PublishAsync(new OrderCreated
{
    Id = Guid.NewGuid().ToString(),
    CustomerId = "12345",
    Total = 99.99m
});
```

Brighter automatically:

1. Uses `JsonMessageMapper<OrderCreated>`
2. Serializes to JSON
3. Applies CloudEvents headers from Publication
4. Sends the message with binary-mode CloudEvents

### Choosing a Different Default Mapper

You can configure which default mapper to use:

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly],
        defaultMessageMapper: typeof(CloudEventJsonMessageMapper<>),      // For producers
        asyncDefaultMessageMapper: typeof(CloudEventJsonMessageMapper<>)  // For async producers
    );
```

This configures structured-mode CloudEvents as the default, useful when your primary transport is AWS SNS/SQS.

## When You Need Custom Message Mappers

While default mappers handle most scenarios, you still need custom `IAmAMessageMapper` implementations in these cases:

### 1. Non-JSON Serialization Formats

If you need a format other than JSON (Avro, ProtoBuf, XML, etc.) You can register your own default message mapper for these:

```csharp
public class AvroMessageMapper<T> : IAmAMessageMapper<T> where T : class, IRequest
{

    private ISchemaRegistryClient _schemaRegistry;
    private IEnumerable<KeyValuePair<string, string>> _config; 

    public IRequestContext? Context { get; set; }

    public AvroMessageMapper<T>(ISchemaRegistryClient schemaRegistry, IEnumerable<KeyValuePair<string, string>> config) 
    {
        _schemaRegistry = schemaRegistry;
        _config = config;        
    }

    public Message MapToMessage(T request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_EVENT
        );

        // Serialize using Avro
        var avroSerializer = new AvroSerializer<T>(_schemaRegistry, _config);
        var body = new MessageBody(
            avroSerializer.SerializeAsync(request).AsSyncOverAsync(),
            CharacterEncoding.Raw
        );

        return new Message(header, body);
    }

    public T MapToRequest(Message message)
    {
        var avroDeserializer = new AvroDeserializer<T>();
        return avroDeserializer.DeserializeAsync(message.Body.Bytes).AsSyncOverAsync();
    }
}

// Register as your default mapper
services.AddBrighter(options => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly],
        defaultMessageMapper: typeof(AvroMessageMapper<>)
    );
```

### 2. Transform Pipelines

When you need message transformation (Claim Check, Compression, Encryption, PII removal, etc.), you must use a custom mapper with transform attributes. See [Message Transforms](/contents/MessageTransforms.md) for the pipeline and worked examples.

## Registering Custom Mappers

### Explicit Registration

If you have specific messages that need custom mappers, register them explicitly:

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly]
        // Other types will use default JsonMessageMapper<T>
    )
    .MapperRegistry(mappers =>
    {
        // Explicit registration for LargeOrder
        mappers.Register<LargeOrder, CompressedOrderMapper>();

        // Explicit registration for SensitiveOrder
        mappers.Register<SensitiveOrder, SecureLargeOrderMapper>();
    });
```

## Configuration Reference

### Using Default Binary-Mode Mapper (Recommended)

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies([typeof(OrderCreated).Assembly]);
// Uses JsonMessageMapper<T> with binary-mode CloudEvents
```

### Using Structured-Mode as Default

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly],
        defaultMessageMapper: typeof(CloudEventJsonMessageMapper<>),
        asyncDefaultMessageMapper: typeof(CloudEventJsonMessageMapper<>)
    );
// Uses structured-mode CloudEvents (good for AWS SNS/SQS)
```

### Custom Default Mapper (e.g., Avro)

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly],
        defaultMessageMapper: typeof(AvroMessageMapper<>),
        asyncDefaultMessageMapper: typeof(AvroMessageMapper<>)
    );
// All messages use Avro serialization by default
```

### Mixed: Default + Custom Mappers

```csharp
services.AddBrighter(options => { })
    .AddProducers(configure => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly]
        // Other messages use JsonMessageMapper<T>
    )
     .MapperRegistry(mappers =>
    {
        // Specific messages with transforms
        mappers.Regiter<LargeOrder, CompressedOrderMapper>();
        mappers.Register<SensitiveData, EncryptedMapper>();
    });
```

## Default Message Mapper Best Practices

### 1. Start with Default Mappers

Use default `JsonMessageMapper<T>` for all new messages unless you have a specific need:

```csharp
// ✅ Recommended - Simple and maintainable
services.AddBrighter(options => { })
    .AutoFromAssemblies([typeof(OrderCreated).Assembly]);
```

### 2. Only Create Custom Mappers When Needed

Don't create custom mappers "just in case". Add them when you need:

- Non-JSON formats (Avro, ProtoBuf)
- Transform pipelines (Claim Check, Compression, Encryption)
- Complex message routing logic

### 3. Configure CloudEvents in Publication

CloudEvents properties belong in `Publication`, not in mappers:

```csharp
// Good - CloudEvents in Publication
new Publication
{
    Topic = new RoutingKey("orders"),
    RequestType = typeof(OrderCreated),
    Source = new Uri("https://example.com/orders"),
    Type = new CloudEventsType("com.example.order.created")
}

// Bad - Don't configure CloudEvents in mapper
// Let default mapper handle it from Publication or use transform
```

### 4. Use Transform Attributes for Cross-Cutting Concerns

Transform attributes are powerful for cross-cutting concerns:

```csharp
// Good - Use transforms for large messages
[ClaimCheck(step: 0, thresholdInKb: 256)]
public Message MapToMessage(LargeEvent request, Publication publication)
{
    // Just serialize - transform handles storage
}

// Bad - Don't implement claim check logic in mapper
// Use the attribute-based transform pipeline
```

### 5. Be Consistent with Default Mapper Choice

Choose one default mapper strategy for your application:

```csharp
// Good - Consistent default across all assemblies
services.AddBrighter(options => { })
    .AutoFromAssemblies(
        [typeof(OrderCreated).Assembly, typeof(CustomerCreated).Assembly],
        defaultMessageMapper: typeof(JsonMessageMapper<>)
    );

// Bad - Different defaults cause confusion
// Pick one and stick with it
```

## Further Reading

- [Cloud Events Support](CloudEventsSupport.md) - Understanding CloudEvents in Brighter
- [Claim Check Pattern](ClaimCheck.md) - Handling large messages
- [Message Mappers](MessageMappers.md) - Writing a custom mapper, and the Brighter message structure
- [Message Transforms](/contents/MessageTransforms.md) - Wrap, unwrap, and the transform pipeline example
- [Compression](Compression.md) - Compressing messages
- [V10 Migration Guide](V10MigrationGuide.md) - Complete migration instructions

## Default Message Mapper Sample Code

Full working examples can be found in the Brighter samples:

- **Default Mappers**: `Brighter/samples/WebAPI/` - WebAPI sample using default mappers
- **ClaimCheck Transform**: `Brighter/samples/Transforms/AWSTransfomers/ClaimCheck/` - Claim check example
- **Compression**: `Brighter/samples/Transforms/` - Various transform examples
