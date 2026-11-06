// Task 6.4, RoutingMultipleMessageTypes.md. The callbacks are the pages', verbatim, lifted out of their transport's
// subscription (the pump calls getRequestType with the received Message; DynamicMessageDeserialization's probe,
// dynamic-request-type, runs that path on the pump). The router is #6's, run through Brighter's Command Processor.
//  #3: OrderType header "Create"/"Update"/"Cancel" -> the matching type. Control: an unknown value and no header ->
//      InvalidMessageAction.
//  #4: the body's eventType -> the matching type. Control: an unknown eventType, no eventType, and a body that is not
//      JSON -> InvalidMessageAction, the last with the JsonException as InnerException.
//  #6: the router sends an OrderCreated to the handler for its Country: US, UK, else the default. Control: Country
//      "FR" -> the default handler only.
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Actions;
using Paramore.Brighter.Extensions.DependencyInjection;

public class CreateOrder() : Command(Id.Random()); public class UpdateOrder() : Command(Id.Random()); public class CancelOrder() : Command(Id.Random());   // probe
public class UserCreated() : Event(Id.Random()); public class UserUpdated() : Event(Id.Random()); public class UserDeleted() : Event(Id.Random());          // probe
public class OrderCreated() : Event(Id.Random()) { public string Country { get; set; } = ""; }                                                              // probe
public static class Ran { public static readonly List<string> Handlers = new(); }
public class USOrderCreatedHandler : RequestHandler<OrderCreated> { public override OrderCreated Handle(OrderCreated e) { Ran.Handlers.Add("US"); return base.Handle(e); } }
public class UKOrderCreatedHandler : RequestHandler<OrderCreated> { public override OrderCreated Handle(OrderCreated e) { Ran.Handlers.Add("UK"); return base.Handle(e); } }
public class DefaultOrderCreatedHandler : RequestHandler<OrderCreated> { public override OrderCreated Handle(OrderCreated e) { Ran.Handlers.Add("Default"); return base.Handle(e); } }

public static class Program
{
    static bool failed;
    static void Row(string name, string expect, string got) { var ok = got == expect; failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {got}{(ok ? "" : $" (expected {expect})")}"); }

    static string Call(Func<Message, Type> f, Message m)
    {
        try { return f(m).Name; }
        catch (Exception e) { return $"{e.GetType().Name}{(e.InnerException is null ? "" : $" ({e.InnerException.GetType().Name}{(e.InnerException is JsonException ? ", a JsonException" : "")})")}"; }
    }

    static Message WithHeader(string? orderType)
    {
        var m = new Message(new MessageHeader(Id.Random(), new RoutingKey("orders"), MessageType.MT_COMMAND), new MessageBody("{}"));
        if (orderType is not null) m.Header.Bag["OrderType"] = orderType;
        return m;
    }

    static Message WithBody(string body) => new(new MessageHeader(Id.Random(), new RoutingKey("events"), MessageType.MT_EVENT), new MessageBody(body));

    public static int Main()
    {
        // #3's getRequestType, verbatim
        Func<Message, Type> byHeader = message =>
        {
            // Route based on custom header
            if (message.Header.Bag.TryGetValue("OrderType", out var orderType))
            {
                return orderType switch
                {
                    "Create" => typeof(CreateOrder),
                    "Update" => typeof(UpdateOrder),
                    "Cancel" => typeof(CancelOrder),
                    _ => throw new InvalidMessageAction($"Unknown order type: {orderType}")
                };
            }

            throw new InvalidMessageAction("OrderType header not found");
        };
        Row("#3 case     Create, Update, Cancel", "CreateOrder, UpdateOrder, CancelOrder",
            $"{Call(byHeader, WithHeader("Create"))}, {Call(byHeader, WithHeader("Update"))}, {Call(byHeader, WithHeader("Cancel"))}");
        Row("#3 control  Refund, no header", "InvalidMessageAction, InvalidMessageAction", $"{Call(byHeader, WithHeader("Refund"))}, {Call(byHeader, WithHeader(null))}");

        // #4's getRequestType, verbatim
        Func<Message, Type> byBody = message =>
        {
            try
            {
                using var doc = JsonDocument.Parse(message.Body.Value);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object ||
                    !root.TryGetProperty("eventType", out var eventType) ||
                    eventType.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidMessageAction(
                        "Message body must be an object with a string eventType property");
                }

                return eventType.GetString() switch
                {
                    "UserCreated" => typeof(UserCreated),
                    "UserUpdated" => typeof(UserUpdated),
                    "UserDeleted" => typeof(UserDeleted),
                    _ => throw new InvalidMessageAction($"Unknown event type: {eventType}")
                };
            }
            catch (JsonException ex)
            {
                throw new InvalidMessageAction("Message body is not valid JSON", ex);
            }
        };
        Row("#4 case     eventType UserCreated, UserUpdated, UserDeleted", "UserCreated, UserUpdated, UserDeleted",
            $"{Call(byBody, WithBody("{\"eventType\":\"UserCreated\"}"))}, {Call(byBody, WithBody("{\"eventType\":\"UserUpdated\"}"))}, {Call(byBody, WithBody("{\"eventType\":\"UserDeleted\"}"))}");
        Row("#4 control  unknown, missing, not JSON", "InvalidMessageAction, InvalidMessageAction, InvalidMessageAction (JsonReaderException, a JsonException)",
            $"{Call(byBody, WithBody("{\"eventType\":\"UserMerged\"}"))}, {Call(byBody, WithBody("{\"id\":1}"))}, {Call(byBody, WithBody("not json"))}");

        // #6's router, verbatim, on AddBrighter's builder (the page registers it on AddConsumers')
        var services = new ServiceCollection();
        services.AddBrighter()
            .Handlers(registry =>
            {
                registry.Register<OrderCreated>((request, context) =>
                {
                    var order = request as OrderCreated;

                    // Route to different handlers based on order properties
                    if (order?.Country == "US")
                        return [typeof(USOrderCreatedHandler)];
                    if (order?.Country == "UK")
                        return [typeof(UKOrderCreatedHandler)];

                    return [typeof(DefaultOrderCreatedHandler)];
                },
                [
                    typeof(USOrderCreatedHandler),
                    typeof(UKOrderCreatedHandler),
                    typeof(DefaultOrderCreatedHandler)
                ]);
            });
        var cp = services.BuildServiceProvider().GetRequiredService<IAmACommandProcessor>();
        string Publish(string country) { Ran.Handlers.Clear(); cp.Publish(new OrderCreated { Country = country }); return string.Join("+", Ran.Handlers); }
        Row("#6 case     US, UK", "US, UK", $"{Publish("US")}, {Publish("UK")}");
        Row("#6 control  FR", "Default", Publish("FR"));
        return failed ? 1 : 0;
    }
}
