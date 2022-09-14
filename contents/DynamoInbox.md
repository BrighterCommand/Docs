# Dynamo Inbox

## Usage
The DynamoDb Inbox allows use of DynamoDb for [Brighter's inbox support](/contents/BrighterInboxSupport.md). The configuration is described in [Basic Configuration](/contents/BrighterBasicConfiguration.md).

For this we will need the *Inbox* packages for the DynamoDb *Inbox*.

* **Paramore.Brighter.Inbox.DynamoDb**

``` csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(hostContext, services) =>
        {
            ConfigureBrighter(hostContext, services);
        }

private static void ConfigureBrighter(HostBuilderContext hostContext, IServiceCollection services)
{
	var dynamoDb = new AmazonDynamoDBClient(credentials, new AmazonDynamoDBConfig { ServiceURL = "http://dynamodb.us-east-1.amazonaws.com"; });

	services.AddServiceActivator(options =>
	{ ...  })
	.UseExternalInbox(
		new DynamoDbInbox(dynamoDb);
		new InboxConfiguration(
		scope: InboxScope.Commands,
		onceOnly: true,
		actionOnExists: OnceOnlyAction.Throw
		)
	);
}

...

```