using Proofpoint.SecureEmailRelay.Mail;
using System.Text;
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
Message message = new("This is a test email", new MailUser("sender@proofpoint.com", "Joe Sender"));

// Add content body
message.AddContent(new Content("This is a test message", ContentType.Text));
message.AddContent(new Content("<b>This is a test message</b>", ContentType.Html));

// Add To
message.AddTo(new MailUser("to_recipient1@proofpoint.com", "Recipient 1"));
message.AddTo(new MailUser("to_recipient2@proofpoint.com", "Recipient 2"));

// Add CC
message.AddCc(new MailUser("cc_recipient1@proofpoint.com", "Carbon Copy 1"));
message.AddCc(new MailUser("cc_recipient2@proofpoint.com", "Carbon Copy 2"));

// Add BCC
message.AddBcc(new MailUser("bcc_recipient2@proofpoint.com", "Blind Carbon Copy 1"));
message.AddBcc(new MailUser("bcc_recipient2@proofpoint.com", "Blind Carbon Copy 2"));

// Reply To
message.AddReplyTo(new MailUser("reply_to1@proofpoint.com", "Reply To 1"));
message.AddReplyTo(new MailUser("reply_to2@proofpoint.com", "Reply To 2"));

// Add Base64 empty attachment, this currently doesn't work with the REST API.
message.AddAttachment(Attachment.FromBase64String("", "empty.txt", "text/plain", Disposition.Attachment));

// Add Base64 encoded attachment
message.AddAttachment(Attachment.FromBase64String("VGhpcyBpcyBhIHRlc3Qh", "test.txt", "text/plain", Disposition.Attachment));

// Add file attachment from disk, if disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(Attachment.FromFile(@"C:\temp\file.csv", Disposition.Attachment));

// Convert the string to a byte array using UTF8 encoding
byte[] bytes = Encoding.UTF8.GetBytes("This is a sample text stream.");

// Add byte array as attachment, if disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(Attachment.FromBytes(bytes, "bytes.txt", "text/plain", Disposition.Attachment));

// Add empty attachment, this currently doesn't work with the REST API. 
message.AddAttachment(Attachment.FromBytes(Array.Empty<byte>(), "nobytes.txt", "text/plain", Disposition.Attachment));

// Print Message object as JSON text
Console.WriteLine(message.ToString());

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");