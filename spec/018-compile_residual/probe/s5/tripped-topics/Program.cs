// Task 4.5a (D19), SweeperCircuitBreaking.md § Sweeper Circuit Breaking Outbox Support.
// Claim: at 10.7.0 the DynamoDB Outbox (V3 package here) accepts TrippedTopics and ignores it, and the
// Spanner Outbox drops the filter, so a tripped topic's messages are still returned as outstanding.
// Each run adds one outstanding message on "orders" and one on "payments", then asks for the
// outstanding messages twice: with "orders" tripped (the case) and with nothing tripped (the control,
// which must return both at every version). Prints the count per topic for each query.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Google.Cloud.Spanner.Data;
using Paramore.Brighter;
using Paramore.Brighter.DynamoDb;
using Paramore.Brighter.Outbox.DynamoDB;
using Paramore.Brighter.Outbox.Spanner;

var which = args.Length > 0 ? args[0] : "dynamo";
var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(DynamoDbOutbox).Assembly.Location).ProductVersion?.Split('+')[0];
var context = new RequestContext();
var stamp = DateTimeOffset.UtcNow.AddHours(-2);
Message Make(string topic) =>
    new(new MessageHeader(Id.Random(), new RoutingKey(topic), MessageType.MT_EVENT, timeStamp: stamp),
        new MessageBody("payload"));
var orders = Make("orders");
var payments = Make("payments");
IEnumerable<RoutingKey> tripped = [new RoutingKey("orders")];

Func<IEnumerable<RoutingKey>?, Task<IEnumerable<Message>>> read;
Func<Task> cleanup;

if (which == "dynamo")
{
    Environment.SetEnvironmentVariable("AWS_ENABLE_ENDPOINT_DISCOVERY", "false");
    var client = new AmazonDynamoDBClient(new BasicAWSCredentials("FakeAccessKey", "FakeSecretKey"),
        new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" });
    var table = $"probe_tripped_{Guid.NewGuid():N}";
    var request = new DynamoDbTableFactory().GenerateCreateTableRequest<MessageItem>(
        provisionedThroughput: null, billingMode: BillingMode.PAY_PER_REQUEST);
    request.TableName = table;
    var builder = new DynamoDbTableBuilder(client);
    await builder.Build(request);
    await builder.EnsureTablesReady([table], TableStatus.ACTIVE);
    var outbox = new DynamoDbOutbox(client, new DynamoDbConfiguration(table, timeout: 10000, scanConcurrency: 1));
    await outbox.AddAsync(orders, context);
    await outbox.AddAsync(payments, context);
    read = t => outbox.OutstandingMessagesAsync(TimeSpan.FromHours(1), context, 100, 1, t);
    cleanup = () => client.DeleteTableAsync(table);
}
else
{
    Environment.SetEnvironmentVariable("SPANNER_EMULATOR_HOST", "localhost:9010");
    var cs = new SpannerConnectionStringBuilder(
        "Data Source=projects/brighter-tests/instances/brighter-spanner/databases/brightertests")
    { EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction }.ConnectionString;
    var configuration = new RelationalDatabaseConfiguration(cs, outBoxTableName: $"probe_{Guid.NewGuid():N}");
    var connection = new SpannerConnection(cs);
    await connection.OpenAsync();
    // The DDL may carry a CREATE INDEX after the CREATE TABLE (10.8.0); the emulator runs one statement per command
    foreach (var ddl in SpannerOutboxBuilder.GetDDL(configuration.OutBoxTableName).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        using var create = connection.CreateDdlCommand(ddl);
        await create.ExecuteNonQueryAsync();
    }
    var outbox = new SpannerOutbox(configuration);
    await outbox.AddAsync([orders, payments], context);
    read = t => outbox.OutstandingMessagesAsync(TimeSpan.FromHours(1), context, 100, 1, t);
    cleanup = async () =>
    {
        // indexes must go first
        using var q = connection.CreateSelectCommand($"SELECT INDEX_NAME FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '{configuration.OutBoxTableName}' AND INDEX_TYPE = 'INDEX'");
        var names = new List<string>();
        using (var r = await q.ExecuteReaderAsync()) while (await r.ReadAsync()) names.Add(r.GetString(0));
        foreach (var n in names) { using var di = connection.CreateDdlCommand($"DROP INDEX `{n}`"); await di.ExecuteNonQueryAsync(); }
        using var drop = connection.CreateDdlCommand($"DROP TABLE `{configuration.OutBoxTableName}`");
        await drop.ExecuteNonQueryAsync();
    };
}

string Count(IEnumerable<Message> ms) =>
    $"orders={ms.Count(m => m.Header.Topic.Value == "orders")} payments={ms.Count(m => m.Header.Topic.Value == "payments")}";

try
{
    var withTripped = (await read(tripped)).ToList();
    var none = (await read(null)).ToList();
    var caseOk = withTripped.All(m => m.Header.Topic.Value != "orders") && withTripped.Count(m => m.Header.Topic.Value == "payments") == 1;
    var controlOk = none.Count == 2;
    Console.WriteLine($"{version} {which} case    (orders tripped): {Count(withTripped)} -> {(caseOk ? "filtered" : "NOT filtered")}");
    Console.WriteLine($"{version} {which} control (none tripped):   {Count(none)} -> {(controlOk ? "both" : "WRONG")}");
    return controlOk ? 0 : 1;
}
finally { await cleanup(); }
