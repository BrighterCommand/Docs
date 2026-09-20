using System.Net.Mime;
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;

public class GreetingMadeMessageMapper : IAmAMessageMapper<GreetingMade>
{
    public IRequestContext Context { get; set; }

    public Message MapToMessage(GreetingMade request, Publication publication)
    {
        var header = new MessageHeader(messageId: request.Id, topic: new RoutingKey("GreetingMade"), messageType: MessageType.MT_EVENT);
        var payload = System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.General));
        var body = new MessageBody(payload, new ContentType(MediaTypeNames.Application.Json), CharacterEncoding.UTF8);
        var message = new Message(header, body);
        return message;
    }

    public GreetingMade MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<GreetingMade>(message.Body.Value, JsonSerialisationOptions.Options);
    }
}
