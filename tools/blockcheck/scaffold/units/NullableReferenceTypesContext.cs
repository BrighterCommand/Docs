// Types NullableReferenceTypes.md names in its blocks and never declares.
//
// Block 15's handler sends an email through the reader's own service, and the page shows neither
// the command nor the service. Each stub carries only the members the block names. The order and
// user types blocks 4, 5 and 7 name are declared by blocks 3, 6 and 9 and stay with the page.
//
// blockcheck: using static NullableReferenceTypesContext;

using Paramore.Brighter;

public static class NullableReferenceTypesContext
{
    // block 10: `user.Name`
    public static User user => null!;
}

// block 10: `user.Name!`, `user.Name == null`
public class User
{
    public string? Name { get; set; }
}

// block 15: `RequestHandler<SendEmailCommand>`, `command.Recipient`, `.Subject`, `.Body`
public class SendEmailCommand() : Command(Id.Random())
{
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

// block 15: `_emailService.SendEmail(command.Recipient, command.Subject, command.Body)`
public interface IEmailService
{
    void SendEmail(string recipient, string subject, string body);
}
