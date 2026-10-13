// Values SwitchingSchedulers.md names in its blocks and never declares.
//
// Each block is the part of an application's startup that changes when the scheduler does; the
// rest of that startup, and the values it builds, are the page's other scheduler pages' to show.
//
// blockcheck: using static SwitchingSchedulersContext;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.AWSSQS;
using Paramore.Brighter.MessagingGateway.AzureServiceBus.ClientProvider;

public static class SwitchingSchedulersContext
{
    // blocks 1–7: `services.AddBrighter(…)`
    public static IServiceCollection services => null!;
    // block 3: `config.UseSqlServerStorage(connectionString)`
    public static string connectionString => null!;
    // block 5: `new AwsSchedulerFactory(awsConnection, "scheduler-role")`
    public static AWSMessagingGatewayConnection awsConnection => null!;
    // block 6: `new AzureServiceBusSchedulerFactory(clientProvider, …)`
    public static IServiceBusClientProvider clientProvider => null!;
    // block 7: `configuration.GetValue<bool>("UseQuartz")`
    public static IConfiguration configuration => null!;
}
