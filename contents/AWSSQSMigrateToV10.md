---
description: "Brighter provides support for both version 3 and version 4 of the AWS SDK for .NET through separate NuGet packages."
layout:
  description:
    visible: false
---

# Migrating AWS SQS to V10

> **How-to** · Applies to **Brighter V10** · Prerequisites: [AWS SQS Configuration](/contents/AWSSQSConfiguration.md)

## AWS SDK v4 Support

Brighter provides support for both version 3 and version 4 of the AWS SDK for .NET through separate NuGet packages. This approach is crucial for managing dependencies and avoiding conflicts within your applications.

*   **Paramore.Brighter.MessagingGateway.AWSSQS**: This package depends on version 3.x of the AWS SDK (`AWSSDK.SQS` and `AWSSDK.SimpleNotificationService`).
*   **Paramore.Brighter.MessagingGateway.AWSSQS.V4**: This package depends on version 4.x of the AWS SDK. You can find it on NuGet [here](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.AWSSQS.V4).

Major versions of the AWS SDK often introduce breaking changes. By offering two distinct packages, Brighter ensures that you can choose the one that aligns with the AWS SDK version used in your project. This prevents "dependency hell" and allows for a smoother migration path if you decide to upgrade from AWS SDK v3 to v4, without being forced to upgrade all your Brighter-related packages at once.
### FIFO Queue Support

Brighter provides support for FIFO (First-In-First-Out) queues and topics:

- **Content-based deduplication**: Automatic deduplication based on message content
- **Message groups**: Support for message group IDs for ordered processing within groups
- **High-throughput FIFO**: Configuration options for high-throughput FIFO queues
- **Better error handling**: Improved handling of FIFO-specific errors (duplicate messages, invalid group IDs)

### Why Separate Packages?

AWS SDK v4 introduced significant changes to improve performance, reduce package size, and modernize the API. Key changes include:

- **Async-first design**: All APIs are now async by default
- **Reduced allocations**: Better memory efficiency
- **Modular packages**: Smaller, more focused packages
- **Improved credential resolution**: Better defaults for credential discovery

By maintaining separate Brighter packages for v3 and v4, you can:

- **Avoid dependency conflicts**: No version collisions between AWS SDK packages
- **Migrate incrementally**: Upgrade one component at a time
- **Support legacy code**: Keep existing applications on v3 while new projects use v4
- **Test thoroughly**: Validate each migration step before moving forward

### Available Packages for SDK v3 and v4

We provides complete support for AWS SDK version 4, while maintaining backwards compatibility with version 3. This allows you to migrate at your own pace without being forced to upgrade all components at once.

**Available Packages**:
- **Paramore.Brighter.MessagingGateway.AWSSQS**: AWS SDK v3 (legacy support)
- **Paramore.Brighter.MessagingGateway.AWSSQS.V4**: AWS SDK v4 (recommended for new projects)
- **Paramore.Brighter.Outbox.DynamoDB**: DynamoDB Outbox with SDK v3
- **Paramore.Brighter.Outbox.DynamoDB.V4**: DynamoDB Outbox with SDK v4
- **Paramore.Brighter.Inbox.DynamoDB**: DynamoDB Inbox with SDK v3
- **Paramore.Brighter.Inbox.DynamoDB.V4**: DynamoDB Inbox with SDK v4
- **Paramore.Brighter.Transformers.AWS**: S3 Luggage Store with SDK v3
- **Paramore.Brighter.Transformers.AWS.V4**: S3 Luggage Store with SDK v4

See [AWS SQS V10 Migration Path](#aws-sqs-v10-migration-path) for migration guidance.

## AWS SQS V10 Migration Path

V10 maintains backwards compatibility with existing configurations while providing new features:

1. **Continue using v3**: Existing code using AWS SDK v3 continues to work without changes
2. **Gradual migration**: Migrate components one at a time to AWS SDK v4
3. **Side-by-side**: Run v3 and v4 packages in the same application (different transport types)
4. **New projects**: Start with v4 packages for the latest features and performance

See [Migration Guidance](#migrating-from-aws-sdk-v3-to-v4) below for step-by-step instructions.

### Migrating from AWS SDK v3 to v4

AWS SDK v4 introduced significant breaking changes to improve performance, modernize the API, and better align with AWS best practices. Brighter V10 supports both versions through separate packages, allowing you to migrate at your own pace.

#### Step-by-Step Migration

**1. Identify Your Current Packages**

First, determine which Brighter AWS packages you're currently using:

```bash
# Check your project file for v3 packages
dotnet list package | grep "Paramore.Brighter.*AWS"
```

**2. Install v4 Packages**

Replace v3 packages with their v4 equivalents:

| V3 Package | V4 Package |
|------------|------------|
| `Paramore.Brighter.MessagingGateway.AWSSQS` | `Paramore.Brighter.MessagingGateway.AWSSQS.V4` |
| `Paramore.Brighter.Outbox.DynamoDB` | `Paramore.Brighter.Outbox.DynamoDB.V4` |
| `Paramore.Brighter.Inbox.DynamoDB` | `Paramore.Brighter.Inbox.DynamoDB.V4` |
| `Paramore.Brighter.Transformers.AWS` | `Paramore.Brighter.Transformers.AWS.V4` |

```bash
# Remove v3 package
dotnet remove package Paramore.Brighter.MessagingGateway.AWSSQS

# Add v4 package
dotnet add package Paramore.Brighter.MessagingGateway.AWSSQS.V4
```

**3. Update Namespace References**

The namespace structure remains the same in most cases, but you'll need to update AWS SDK namespace imports:

```csharp
// V3
using Amazon.SimpleNotificationService;
using Amazon.SQS;

// V4 - Same namespaces, different package versions
using Amazon.SimpleNotificationService;
using Amazon.SQS;
```

**4. Update Credentials and Configuration**

AWS SDK v4 introduced new credential and configuration patterns:

**V3 Approach**:

```csharp
// ...
// V3 - Using profile
var chain = new CredentialProfileStoreChain();
if (!chain.TryGetAWSCredentials("default", out var credentials))
{
    throw new InvalidOperationException("Missing AWS Credentials");
}

var region = RegionEndpoint.GetBySystemName("us-east-1");
var connection = new AwsMessagingGatewayConnection(credentials, region);
```

**V4 Approach**:

```csharp
// ...
// V4 - Using profile (similar, but with v4 SDK)
var chain = new CredentialProfileStoreChain();
if (!chain.TryGetAWSCredentials("default", out var credentials))
{
    throw new InvalidOperationException("Missing AWS Credentials");
}

var region = RegionEndpoint.GetBySystemName("us-east-1");
var connection = new AwsMessagingGatewayConnection(credentials, region);
```

**V4 - Using Default Credentials** (recommended):

```csharp
// ...
// V4 - Let SDK resolve credentials automatically
var credentials = FallbackCredentialsFactory.GetCredentials();
var region = FallbackRegionFactory.GetRegionEndpoint();
var connection = new AwsMessagingGatewayConnection(credentials, region);
```

**5. Test Thoroughly**

After migration, test all AWS interactions:

- SNS topic publishing
- SQS queue publishing and consumption
- DynamoDB Outbox/Inbox operations
- S3 Luggage Store operations
- IAM permissions and credentials

**6. Update CI/CD Pipelines**

Ensure your build and deployment pipelines reference the correct package versions and AWS SDK dependencies.

#### Key Differences Between v3 and v4

| Aspect | V3 | V4 |
|--------|----|----|
| **Credentials** | Manual credential resolution | Improved default credential chain |
| **Async APIs** | Mix of sync/async | Async-first design |
| **Performance** | Good | Optimized with reduced allocations |
| **Dependencies** | Larger package size | Smaller, more modular packages |
| **Service Clients** | Synchronous construction | Async construction patterns |

#### Common Migration Issues

**Issue 1: Credential Resolution Fails**

```csharp
// ...
// Problem: Credentials not found
var chain = new CredentialProfileStoreChain();
if (!chain.TryGetAWSCredentials("default", out var credentials))
{
    throw new InvalidOperationException("Missing AWS Credentials");
}

// Solution: Use fallback credentials
var credentials = FallbackCredentialsFactory.GetCredentials(); // Checks env vars, profiles, IAM roles
```

**Issue 2: Region Not Set**

```csharp
// ...
// Problem: Region not specified
var connection = new AwsMessagingGatewayConnection(credentials, null); // ❌

// Solution: Provide region explicitly or use fallback
var region = FallbackRegionFactory.GetRegionEndpoint()
    ?? RegionEndpoint.USEast1; // Fallback to default
var connection = new AwsMessagingGatewayConnection(credentials, region);
```

**Issue 3: Package Version Conflicts**

```csharp
// ...
// Problem: Mixing v3 and v4 packages in the same project for the same AWS service
// Install-Package Paramore.Brighter.MessagingGateway.AWSSQS
// Install-Package Paramore.Brighter.MessagingGateway.AWSSQS.V4 // ❌ Conflict

// Solution: Use one version per AWS service
// For SQS/SNS, choose either v3 OR v4, not both
// You CAN mix if using different AWS services (e.g., SQS v4 + DynamoDB v3)
```

### Gradual Migration Strategy

You don't have to migrate everything at once. Here's a recommended phased approach:

**Phase 1: Messaging Gateway** (SNS/SQS)
1. Migrate `Paramore.Brighter.MessagingGateway.AWSSQS` to v4
2. Update configuration and test messaging
3. Deploy and monitor

**Phase 2: Outbox** (if using DynamoDB Outbox)
1. Migrate `Paramore.Brighter.Outbox.DynamoDB` to v4
2. Test transactional messaging
3. Deploy and monitor

**Phase 3: Inbox** (if using DynamoDB Inbox)
1. Migrate `Paramore.Brighter.Inbox.DynamoDB` to v4
2. Test deduplication
3. Deploy and monitor

**Phase 4: Transformers** (if using S3 Luggage Store)
1. Migrate `Paramore.Brighter.Transformers.AWS` to v4
2. Test claim check pattern
3. Deploy and monitor

### Best Practices for Migration

1. **Test in Lower Environments First**: Migrate dev → staging → production
2. **Use Feature Flags**: Enable v4 for a subset of traffic initially
3. **Monitor Metrics**: Watch for changes in latency, error rates, and AWS API calls
4. **Have Rollback Plan**: Keep v3 packages available for quick rollback if needed
5. **Update Documentation**: Document which components use v3 vs v4
6. **Coordinate with Team**: Ensure all developers understand the migration plan

## Further Reading

- [AWS SQS Configuration](/contents/AWSSQSConfiguration.md) - Connection, publication and subscription parameters
- [S3 Luggage Store](/contents/S3LuggageStore.md) - The claim check transformer, v3 and v4 packages
- [Brighter V10 Migration Guide](/contents/V10MigrationGuide.md) - The V9 to V10 changes that are not AWS-specific
