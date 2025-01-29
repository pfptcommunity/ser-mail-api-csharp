using Proofpoint.SecureEmailRelay.Mail;
using System.Text;

Client client = new("<client_id>", "<client_secret>");

// Create a new Message object
Message message = new("This is a test email", new MailUser("sender@proofpoint.com", "Joe Sender"));

// Add content body
message.AddContent(new Content("This is a test message", ContentType.Text));
message.AddContent(new Content("<b>This is a test message</b>", ContentType.Html));

// Add Recipients
message.AddTo(new MailUser("recipient1@proofpoint.com", "Recipient 1"));
//message.AddTo(new MailUser("recipient2@proofpoint.com", "Recipient 2"));

// Add CC
message.AddCc(new MailUser("cc1@proofpoint.com", "Carbon Copy 1"));
message.AddCc(new MailUser("cc2@proofpoint.com", "Carbon Copy 2"));

// Add BCC;
message.AddBcc(new MailUser("bcc2@proofpoint.com", "Blind Carbon Copy 1"));
message.AddBcc(new MailUser("bcc2@proofpoint.com", "Blind Carbon Copy 2"));

// Add Base64 Encoded Attachment
message.AddAttachment(new Attachment("VGhpcyBpcyBhIHRlc3Qh", "test.txt", "text/plain", Disposition.Attachment));

// Add File Attachment from Disk, if Disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(new FileAttachment(@"C:\temp\file.csv", Disposition.Attachment));

// Convert the string to a byte array using UTF8 encoding
byte[] bytes = Encoding.UTF8.GetBytes("This is a sample text stream.");

// Add Byte array as Attachment, if Disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(new BinaryAttachment(bytes, "bytes.txt", "text/plain", Disposition.Attachment));

// Print Message object as JSON text
Console.WriteLine(message.ToString());

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");

