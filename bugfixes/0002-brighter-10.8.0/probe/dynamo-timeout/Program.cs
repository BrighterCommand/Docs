// Brighter 10.8.0: DynamoDbConfiguration.timeout is a whole-operation deadline (DynamoOutbox.md).
// Case: a 1 ms deadline cancels AddAsync. Controls: timeout 0 succeeds; a 1 ms configuration with a
// per-call outBoxTimeout of 0 succeeds; a configured timeout below -1 throws. Needs DynamoDB Local on 8000.
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Paramore.Brighter;
using Paramore.Brighter.DynamoDb;
using Paramore.Brighter.Outbox.DynamoDB;

var client = new AmazonDynamoDBClient(new BasicAWSCredentials("x", "x"),
    new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" });
var request = new DynamoDbTableFactory().GenerateCreateTableRequest<MessageItem>(
    new DynamoDbCreateProvisionedThroughput(
        new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
        new Dictionary<string, ProvisionedThroughput?>
        {
            ["Outstanding"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["OutstandingAllTopics"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["Delivered"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["DeliveredAllTopics"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
            ["Causation"] = new() { ReadCapacityUnits = 10, WriteCapacityUnits = 10 },
        }));
var builder = new DynamoDbTableBuilder(client);
await builder.Build(request);
await builder.EnsureTablesReady([request.TableName], TableStatus.ACTIVE);

Message NewMessage() => new(new MessageHeader(Id.Random(), new RoutingKey("probe"), MessageType.MT_EVENT),
    new MessageBody("{}"));

async Task<string> Run(string label, int configured, int perCall)
{
    var outbox = new DynamoDbOutbox(client, new DynamoDbConfiguration(request.TableName, timeout: configured));
    try { await outbox.AddAsync(NewMessage(), new RequestContext(), perCall); return $"{label}: added"; }
    catch (Exception e) { return $"{label}: {e.GetType().FullName} (is OperationCanceledException: {e is OperationCanceledException})"; }
}

Console.WriteLine(await Run("case    configured 1 ms", 1, -1));
Console.WriteLine(await Run("control configured 0", 0, -1));
Console.WriteLine(await Run("control configured -1", -1, -1));
Console.WriteLine(await Run("control configured 1 ms, per call 0", 1, 0));
Console.WriteLine(await Run("control default 500 ms", 500, -1));
try { _ = new DynamoDbConfiguration(timeout: -2); Console.WriteLine("control timeout -2: constructed"); }
catch (Exception e) { Console.WriteLine($"control timeout -2: {e.GetType().Name}"); }
