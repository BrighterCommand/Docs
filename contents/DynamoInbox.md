---
description: "The DynamoDb Inbox allows use of DynamoDb for Brighter's inbox support."
layout:
  description:
    visible: false
---

# Dynamo Inbox

> **Reference** · Applies to **Brighter V10**

## Dynamo Inbox Usage
The DynamoDb Inbox allows use of DynamoDb for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Dispatcher Configuration Reference](/contents/DispatcherConfigurationReference.md#inbox).

For this we will need the *Inbox* packages for the DynamoDb *Inbox*.

**AWS SDK v3** (legacy support):
* **Paramore.Brighter.Inbox.DynamoDb**

**AWS SDK v4** (recommended for new projects):
* **Paramore.Brighter.Inbox.DynamoDb.V4**

See [AWS SQS Migration](/contents/AWSSQSMigrateToV10.md#migrating-from-aws-sdk-v3-to-v4) for migration guidance between v3 and v4.

``` csharp
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.DynamoDB;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;

private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
	var dynamoDb = new AmazonDynamoDBClient(credentials, new AmazonDynamoDBConfig { ServiceURL = "http://dynamodb.us-east-1.amazonaws.com"; });

	services.AddConsumers(opt => 
	{
		opt.Inbox = new InboxConfiguration(new DynamoDbInbox(dynamoDb, new DynamoDbInboxConfiguration()));
		...
	});
}
...

```
