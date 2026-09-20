using System.Net.Mime;
using System.Text.Json;
using Paramore.Brighter;
using static PageContext;   // page-context helpers the page names but does not define

public class Block_MessageMappers_md_3
{
    private readonly GreetingMade request = new GreetingMade("hello");

    public void Run()
    {

var payload = System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.General));
var body = new MessageBody(payload, new ContentType(MediaTypeNames.Application.Json), CharacterEncoding.UTF8);
}
}
