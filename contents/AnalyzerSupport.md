# Analyzer Support

> **Reference** · Applies to **Brighter V10**

Brighter provides Roslyn analyzers that detect common configuration and message-mapping mistakes while you write and build your application. The analyzers surface these problems as IDE and compiler warnings, before they can become runtime errors or subtle production behavior.

The Brighter analyzer package also includes code fixes for supported diagnostics. A code fix lets your IDE apply the recommended change through Quick Actions instead of editing the code manually.

## Installing the Analyzer

Add the Brighter analyzer NuGet package to each project that creates Brighter publications, subscriptions, or message mappers:

```shell
dotnet add package Paramore.Brighter.Analyzer.Package
```

The analyzer and code-fix assemblies load automatically for the project. You do not need to register the analyzer in your Brighter configuration.

## Diagnostic Reference

| ID | Severity | Detects | Code fix |
| --- | --- | --- | --- |
| **BRT001** | Warning | A `Publication` is created without assigning `RequestType`. | No |
| **BRT002** | Warning | The type assigned to `RequestType` does not implement `IRequest`. | No |
| **BRT003** | Warning | A `Subscription` is created without specifying `MessagePumpType`. | No |
| **BRT004** | Warning | A wrap attribute is applied to the wrong message-mapper method. | No |
| **BRT005** | Warning | An unwrap attribute is applied to the wrong message-mapper method. | No |
| **BRT006** | Warning | A `KafkaPublication` is created without an explicit `Partitioner` assignment. | Yes |
| **BRT007** | Warning | A `KafkaPublication` uses `Partitioner.ConsistentRandom`. | Yes |
| **BRT008** | Warning | A `KafkaPublication` uses `Partitioner.Consistent`. | Yes |

## Kafka Partitioner Diagnostics

The Kafka partitioner analyzer checks `KafkaPublication` and `KafkaPublication<T>` object creations. It helps you make an explicit partitioner choice and recommends the Murmur2-based partitioners for new publications.

When you do not assign `Partitioner`, `KafkaPublication` currently defaults to `Partitioner.ConsistentRandom`. That default preserves compatibility, but it also hides an important partitioning decision. The partitioner controls how message keys map to Kafka partitions; an uneven mapping can create *hot partitions*, where a small number of partitions and consumers receive a disproportionate share of the work.

For new publications, prefer `Partitioner.Murmur2Random`. It uses the Murmur2 hash for keyed messages and spreads unkeyed messages randomly across partitions. Use `Partitioner.Murmur2` when you do not expect unkeyed messages and want those messages to use a single deterministic partition.

### BRT006: Missing Partitioner

**BRT006** warns when a `KafkaPublication` does not assign `Partitioner` explicitly:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created")
    // Warning: Partitioner assignment is missing.
};
```

Set the partitioner explicitly:

```csharp
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created"),
    Partitioner = Partitioner.Murmur2Random
};
```

The code fix adds `Partitioner = Partitioner.Murmur2Random` to the publication initializer.

### BRT007: ConsistentRandom Partitioner Used

**BRT007** warns when a publication uses `Partitioner.ConsistentRandom`:

```csharp
var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created"),
    Partitioner = Partitioner.ConsistentRandom // Warning: prefer Murmur2Random.
};
```

For a new publication, change the value to `Murmur2Random`:

```csharp
var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created"),
    Partitioner = Partitioner.Murmur2Random
};
```

The code fix replaces `Partitioner.ConsistentRandom` with `Partitioner.Murmur2Random`.

### BRT008: Consistent Partitioner Used

**BRT008** warns when a publication uses `Partitioner.Consistent`:

```csharp
var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created"),
    Partitioner = Partitioner.Consistent // Warning: prefer Murmur2.
};
```

For a new publication, change the value to `Murmur2`:

```csharp
var publication = new KafkaPublication
{
    Topic = new RoutingKey("orders.created"),
    Partitioner = Partitioner.Murmur2
};
```

The code fix replaces `Partitioner.Consistent` with `Partitioner.Murmur2`.

## Applying Code Fixes

The analyzer package includes code fixes for the Kafka partitioner diagnostics:

| Diagnostic | Quick Action |
| --- | --- |
| **BRT006** | Set `Partitioner` to `Partitioner.Murmur2Random` |
| **BRT007** | Use `Partitioner.Murmur2Random` |
| **BRT008** | Use `Partitioner.Murmur2` |

To apply a fix:

1. Place the caret on the warning in your IDE.
2. Open Quick Actions, usually with **Ctrl+.** or the light-bulb icon.
3. Select the recommended partitioner action.
4. Review the change before saving.

The code-fix providers support batch fixing, so IDEs that expose Roslyn **Fix All** operations can apply the same fix across a document, project, or solution.

If the file does not already import the Kafka namespace, make sure the fixed code can resolve the `Partitioner` enum:

```csharp
using Paramore.Brighter.MessagingGateway.Kafka;
```

## Existing Kafka Topics

Review partitioner warnings carefully before changing an existing topic. Different hash algorithms can map the same partition key to different partitions. Changing from `ConsistentRandom` to `Murmur2Random`, or from `Consistent` to `Murmur2`, can therefore move keys between partitions and affect per-key ordering during the transition.

For an existing publication that must preserve its current key-to-partition mapping, you can keep the existing partitioner and suppress the warning locally.

Use a pragma around a single publication:

```csharp
#pragma warning disable BRT007
var publication = new KafkaPublication
{
    Topic = new RoutingKey("legacy.orders.created"),
    Partitioner = Partitioner.ConsistentRandom // Intentional: preserve existing key mapping.
};
#pragma warning restore BRT007
```

Or configure the diagnostic in `.editorconfig`:

```ini
dotnet_diagnostic.BRT007.severity = none
```

Prefer a narrow suppression with an explanatory comment over disabling the diagnostic globally.

## Analyzer Best Practices

- Set `Partitioner` explicitly on every `KafkaPublication`.
- Use `Partitioner.Murmur2Random` for new publications unless you have a specific compatibility requirement.
- Treat a partitioner change on an existing topic as a key-mapping change, not just a code cleanup.
- Use **Fix All** only after checking that the publications in scope are safe to migrate.
- Keep analyzer warnings enabled so new publications do not silently inherit the legacy default.

## Further Reading

- [Kafka Configuration: Kafka Hash Partitioning](/contents/KafkaConfiguration.md#kafka-hash-partitioning)
- [Message Mappers](/contents/MessageMappers.md)
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer/Analyzers/KafkaPublicationPartitionerAnalyzer.cs`
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer.CodeFixes/CodeFixes/MissingPartitionerCodeFixProvider.cs`
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer.CodeFixes/CodeFixes/PartitionerValueCodeFixProvider.cs`
