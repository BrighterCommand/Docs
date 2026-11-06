// Claim: the mappers on Routing.md carry the request's Id across the wire. Case: the page's mapper,
// serialising `request.Id.Value`. Control: the page's mapper as it stood, serialising `request.Id`
// (the record), which Newtonsoft writes as an object that `(string)` cannot read back.
using System;
using Newtonsoft.Json.Linq;
using Paramore.Brighter;

public class MyRequest(ReplyAddress sendersAddress) : Request(sendersAddress);

public static class Program
{
    static string Serialise(MyRequest request, bool value) =>
        new JObject(new JProperty("Id", value ? request.Id.Value : request.Id)).ToString();

    static Id Deserialise(string json) => new Id((string) JObject.Parse(json)["Id"]!);

    public static void Main()
    {
        var request = new MyRequest(new ReplyAddress("replies", Id.Random()));

        var back = Deserialise(Serialise(request, value: true));
        Console.WriteLine($"case: round trip preserves the Id = {back.Value == request.Id.Value}");

        try
        {
            Deserialise(Serialise(request, value: false));
            Console.WriteLine("control: round trip of the record did NOT throw");
        }
        catch (Exception e)
        {
            Console.WriteLine($"control: the record round trip throws {e.GetType().Name}: {e.Message}");
        }
    }
}
