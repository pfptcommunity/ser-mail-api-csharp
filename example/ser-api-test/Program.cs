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

// Create a new Message object using the fluid builder with manual and dynamic ContentId in one HTML body
var message = Message.Builder()
    .From("sender@example.com", "Joe Sender")
    .To("recipient1@example.com", "Recipient 1")
    .To("recipient2@example.com", "Recipient 2")
    .Subject("This is a test email")
    .Content("This is a test message", ContentType.Text) // Plain text alternative
    .Attachment(Attachment.Builder()
        .FromFile("C:/temp/logo_a.png")
        .DispositionInline("logo") // Manual ContentId
        .Build())
    .Attachment(Attachment.Builder()
        .FromFile("C:/temp/logo_b.png")
        .DispositionInline(out string dynamicCid) // Dynamic ContentId
        .Build())
    .Content($"<b>Static CID</b><br><img src=\"cid:logo\"><br><b>Dynamic CID</b><br><img src=\"cid:{dynamicCid}\">", ContentType.Html)
    .Cc("cc1@example.com", "CC Recipient 1")
    .Cc("cc2@example.com", "CC Recipient 2")
    .Bcc("bcc1@example.com", "BCC Recipient 1")
    .Bcc("bcc2@example.com", "BCC Recipient 2")
    .Attachment(Attachment.Builder()
        .FromBase64("VGhpcyBpcyBhIHRlc3Qh", "test.txt")
        .Build())
    .Attachment(Attachment.Builder()
        .FromFile("C:/temp/file.csv")
        .Build())
    .Attachment(Attachment.Builder()
        .FromBytes(new byte[] { 1, 2, 3 }, "bytes.txt")
        .Build())
    .ReplyTo("noreply@example.com", "No Reply")
    .Build();

// Send the email
var result = await client.Send(message);

Console.WriteLine($"HTTP Response: {result.HttpResponse.StatusCode}/{(int)result.HttpResponse.StatusCode}");
Console.WriteLine($"Reason: {result.Reason}");
Console.WriteLine($"Message ID: {result.MessageId}");
Console.WriteLine($"Request ID: {result.RequestId}");