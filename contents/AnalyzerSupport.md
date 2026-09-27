---
description: "Brighter provides Roslyn analyzers that detect common configuration and message-mapping mistakes while you write and build your application."
layout:
  description:
    visible: false
---

# Analyzer Support

> **Reference** · Applies to **Brighter V10**

Brighter provides Roslyn analyzers that detect common configuration and message-mapping mistakes while you write and build your application. The analyzers surface these problems as IDE and compiler warnings, before they can become runtime errors or subtle production behavior.

## Installing the Analyzer

Add the Brighter analyzer NuGet package to each project that creates Brighter publications, subscriptions, or message mappers:

```shell
dotnet add package Paramore.Brighter.Analyzer.Package
```

The analyzer assembly loads automatically for the project. You do not need to register the analyzer in your Brighter configuration.

## Diagnostic Reference

| ID | Severity | Detects |
| --- | --- | --- |
| **BRT001** | Warning | A `Publication` is created without assigning `RequestType`. |
| **BRT002** | Warning | The type assigned to `RequestType` does not implement `IRequest`. |
| **BRT003** | Warning | A `Subscription` is created without specifying `MessagePumpType`. |
| **BRT004** | Warning | A wrap attribute is applied to the wrong message-mapper method. |
| **BRT005** | Warning | An unwrap attribute is applied to the wrong message-mapper method. |

## Suppressing an Analyzer Diagnostic

Where you have a reason to keep the code as it is, suppress the diagnostic as narrowly as you can. Use a pragma around the code it applies to:

```csharp
using Paramore.Brighter;

#pragma warning disable BRT001 // say here why this publication has no RequestType
var publication = new Publication
{
    Topic = new RoutingKey("orders.created")
};
#pragma warning restore BRT001
```

Or set its severity in `.editorconfig`, which applies to every file the section matches:

```ini
dotnet_diagnostic.BRT001.severity = none
```

Prefer the pragma, with a comment saying why, over disabling a diagnostic for a whole project.

## Further Reading

- [Message Mappers](/contents/MessageMappers.md) — the wrap and unwrap attributes BRT004 and BRT005 check
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer/Analyzers/PublicationRequestTypeAssignmentAnalyzer.cs`
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer/Analyzers/SubscriptionConstructorAnalyzer.cs`
- Reference code: `Brighter/src/Paramore.Brighter.Analyzer/Analyzers/WrapAttributeAnalyzer.cs`
