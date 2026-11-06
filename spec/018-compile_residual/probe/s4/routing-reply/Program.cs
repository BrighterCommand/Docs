// Task 6.4, Routing.md #3 and #4, verbatim, with #2's MyRequest and MyReply: "we set the MessageHeader with the topic
// the Consumer should reply to ... the correlation id", "When we serialize back to a Command we set the ReplyAddress
// with the Topic and Correlation Id", and the reply mapper "uses the ReplyAddress to route the Message via its
// MessageHeader back to the caller". (The Id round trip is routing-id's, task 6.3.)
// Case: a request sent with ReplyAddress ("replies.a", c1) is mapped out and in; the consumer's reply is built from
// the ReplyAddress it received, mapped out and in. Control: a second request with ("replies.b", c2) - its reply goes
// to replies.b with c2, so the topic is the request's, not the mapper's.
using System;
using Newtonsoft.Json.Linq;
using Paramore.Brighter;

// #2, verbatim
public class MyRequest : Request
{
    public MyRequest(ReplyAddress sendersAddress) : base(sendersAddress)
    {
    }
}

public class MyReply : Reply
{
    public MyReply(ReplyAddress sendersAddress) : base(sendersAddress)
    {
    }
}

// #3, verbatim
public class MyRequestMessageMapper : IAmAMessageMapper<MyRequest>
{
    public IRequestContext? Context { get; set; }

    public Message MapToMessage(MyRequest request, Publication publication)
    {
        var header = new MessageHeader(
        messageId: request.Id,
        topic: "MyRequest",
        messageType: MessageType.MT_COMMAND,
        correlationId: request.ReplyAddress.CorrelationId,
        replyTo: request.ReplyAddress.Topic);

        var json = new JObject(new JProperty("Id", request.Id.Value));
        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public MyRequest MapToRequest(Message message)
    {
        var replyAddress = new ReplyAddress(topic: message.Header.ReplyTo, correlationId: message.Header.CorrelationId);
        var request = new MyRequest(replyAddress);
        var messageBody = JObject.Parse(message.Body.Value);
        request.Id = new Id((string) messageBody["Id"]!);
        return request;
    }
}

// #4, verbatim
internal class MyReplyMessageMapper : IAmAMessageMapper<MyReply>
{
    public IRequestContext? Context { get; set; }

    public Message MapToMessage(MyReply request, Publication publication)
    {
        var header = new MessageHeader(
            messageId:request.Id,
            topic: request.SendersAddress.Topic,
            messageType: MessageType.MT_COMMAND,
            timeStamp: DateTime.UtcNow,
            correlationId: request.SendersAddress.CorrelationId
        );

        var json = new JObject(new JProperty("Id", request.Id.Value));

        var body = new MessageBody(json.ToString());
        var message = new Message(header, body);
        return message;
    }

    public MyReply MapToRequest(Message message)
    {
        var replyAddress = new ReplyAddress(message.Header.Topic, message.Header.CorrelationId);

        var reply = new MyReply(replyAddress);

        var messageBody = JObject.Parse(message.Body.Value);
        reply.Id = new Id((string) messageBody["Id"]!);

        return reply;
    }
}

public static class Program
{
    static string Trip(string topic)
    {
        var correlation = Id.Random();
        var publication = new Publication();
        var requestOut = new MyRequestMessageMapper().MapToMessage(new MyRequest(new ReplyAddress(topic, correlation)), publication);
        var received = new MyRequestMessageMapper().MapToRequest(requestOut);                      // at the consumer
        var replyOut = new MyReplyMessageMapper().MapToMessage(new MyReply(received.ReplyAddress), publication);
        var reply = new MyReplyMessageMapper().MapToRequest(replyOut);                              // back at the sender
        var ok = requestOut.Header.Topic.Value == "MyRequest" && requestOut.Header.ReplyTo!.Value == topic
                 && replyOut.Header.Topic.Value == topic && replyOut.Header.CorrelationId == correlation
                 && reply.SendersAddress.CorrelationId == correlation;
        return $"request on {requestOut.Header.Topic.Value} replyTo {requestOut.Header.ReplyTo}; reply on {replyOut.Header.Topic.Value}, correlation kept {replyOut.Header.CorrelationId == correlation && reply.SendersAddress.CorrelationId == correlation} => {(ok ? "ok" : "FAIL")}";
    }

    public static int Main()
    {
        var a = Trip("replies.a");
        var b = Trip("replies.b");
        Console.WriteLine($"case    {a}");
        Console.WriteLine($"control {b}");
        return a.EndsWith("ok") && b.EndsWith("ok") ? 0 : 1;
    }
}
