using Proofpoint.SecureEmailRelay.Mail;
using System.Text.Json;

// Read stashed API Key Data
string jsonText = File.ReadAllText(@"C:\temp\ser.api_key");
var apiKeyData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

Client? client = null;

if (apiKeyData != null &&
    apiKeyData.TryGetValue("client_id", out string? clientId) &&
    apiKeyData.TryGetValue("client_secret", out string? clientSecret))
{
    client = new(clientId, clientSecret);
}
else
{
    Console.WriteLine("Keys not found in JSON.");
    Environment.Exit(1);
}


// Create a new Message object
var message = new Message("This is a test email", new MailUser("sender@example.com", "Joe Sender"));

// Add text content body
message.AddContent(new Content("This is a test message", ContentType.Text));

// Add HTML content body, with embedded image
message.AddContent(new Content("<b>This is a test message</b><br><img src=\"cid:logo\">", ContentType.Html));

// Create an inline attachment from disk and set the cid
message.AddAttachment(Attachment.FromFile("C:/temp/logo.png", Disposition.Inline, "logo"));

// Add recipients
message.AddTo(new MailUser("recipient1@example.com", "Recipient 1"));
message.AddTo(new MailUser("recipient2@example.com", "Recipient 2"));

// Add CC
message.AddCc(new MailUser("cc1@example.com", "CC Recipient 1"));
message.AddCc(new MailUser("cc2@example.com", "CC Recipient 2"));

// Add BCC
message.AddBcc(new MailUser("bcc1@example.com", "BCC Recipient 1"));
message.AddBcc(new MailUser("bcc2@example.com", "BCC Recipient 2"));

// Add attachments
message.AddAttachment(Attachment.FromBase64("", "test.txt"));
message.AddAttachment(Attachment.FromFile("C:/temp/file.csv"));
message.AddAttachment(Attachment.FromBytes(new byte[] { 1, 2, 3 }, "bytes.txt", "text/plain"));

// Set Reply-To
message.AddReplyTo(new MailUser("noreply@proofpoint.com", "No Reply"));

// Send the email
var result = await client.Send(message);

Console.WriteLine($"HTTP Response: {result.HttpResponse.StatusCode}/{(int)result.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {result.Reason}");
Console.WriteLine($"Message ID: {result.MessageId}");
Console.WriteLine($"Request ID: {result.RequestId}");