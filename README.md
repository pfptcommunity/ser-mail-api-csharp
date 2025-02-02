# Proofpoint Secure Email Relay Mail API Package
[![NuGet Downloads](https://img.shields.io/nuget/dt/Proofpoint.SecureEmailRelay.Mail.svg)](https://www.nuget.org/packages/Proofpoint.SecureEmailRelay.Mail)  
Library implements all the functions of the SER Email Relay API via C#.

### Requirements:

* .NET 6.0

### Sending a Simple Email with Plain Text and HTML Content
```csharp
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

Message message = new("Hello World Email", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("Hello, this is a test email.", ContentType.Text));
message.AddContent(new Content("<b>Hello, this is a test email.</b>", ContentType.Html));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

### Sending an Email with an Attachment (File from Disk)
```csharp
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

Message message = new("Email with Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains an attachment.", ContentType.Text));

message.AddAttachment(Attachment.FromFile(@"C:\path\to\document.pdf"));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

### Sending an Email with an Inline Image

```csharp
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

Message message = new("Email with Inline Image", new MailUser("sender@example.com", "John Doe"));

// Create an inline image attachment (Content-ID is auto-generated)
var logo = Attachment.FromFile(@"C:\path\logo.png", Disposition.Inline);

// Add the attachment to the message
message.AddAttachment(logo);

// Reference the attachment using Content-ID inside the email body
message.AddContent(new Content($"<img src=\"cid:{logo.Id}\">", ContentType.Html));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

### Sending an Email with a Base64 Encoded Attachment
```csharp
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

string base64Content = Convert.ToBase64String(File.ReadAllBytes(@"C:\path\to\document.pdf"));

Message message = new("Email with Base64 Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains a Base64 encoded attachment.", ContentType.Text));

message.AddAttachment(Attachment.FromBase64String(base64Content, "document.pdf", "application/pdf"));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

### Sending an Email with an Attachment from a Byte Array
```
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

byte[] fileBytes = Encoding.UTF8.GetBytes("This is a sample text file.");

Message message = new("Email with Byte Array Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains a text file attachment.", ContentType.Text));

message.AddAttachment(Attachment.FromBytes(fileBytes, "sample.txt", "text/plain"));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```


### Sending an Email with Multiple Recipients (To, CC, BCC)

```csharp
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("your_client_id", "your_client_secret");

Message message = new("Multi-Recipient Email", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email has multiple recipients.", ContentType.Text));

message.AddTo(new MailUser("to1@example.com", "To Recipient 1"));
message.AddTo(new MailUser("to2@example.com", "To Recipient 2"));

message.AddCc(new MailUser("cc1@example.com", "CC Recipient 1"));
message.AddCc(new MailUser("cc2@example.com", "CC Recipient 2"));

message.AddBcc(new MailUser("bcc1@example.com", "BCC Recipient 1"));
message.AddBcc(new MailUser("bcc2@example.com", "BCC Recipient 2"));

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

### Proxy Support

`Proofpoint.SecureEmailRelay.Mail` supports HTTP and HTTPS proxies by allowing users to pass a custom `HttpClientHandler` when initializing the `Client`.

To configure an HTTP(S) proxy, create a **custom `HttpClientHandler`** and pass it to the client:

```csharp
using System.Net;
using Proofpoint.SecureEmailRelay.Mail;

// Configure an HTTP/HTTPS proxy
var proxy = new WebProxy("http://your-proxy-server:port")
{
    Credentials = new NetworkCredential("your-username", "your-password") // Optional authentication
};

var httpClientHandler = new HttpClientHandler
{
    Proxy = proxy,
    UseProxy = true // Ensures proxy usage
};

// Initialize the client with proxy support
Client client = new("<client_id>", "<client_secret>", httpClientHandler);
```

### Known Issues

I've reached out to product management regarding a possible bug caused by empty file content. Empty files should be allowed to be sent via email but the API currently will reject those messages. This is most likely a corner case but in the event a system generates an empty file, that file will be rejected by the REST endpoint. Below is an example of empty content.

```json
{
      "content": "",
      "disposition": "attachment",
      "filename": "empty.txt",
      "id": "1ed38149-70b2-4476-84a1-83e73913d43c",
      "type": "text/plain"
}
```

This type of content leads to an REST response with error.

```
Status Code: 400 BadRequest
Message ID:
Reason: attachments[0].content is required
Request ID: fe9a1acf60a20c9d90bed843f6530156
Raw JSON: {"request_id":"fe9a1acf60a20c9d90bed843f6530156","reason":"attachments[0].content is required"}
```

### Limitations

There are no known limitations.

For more information please see: https://api-docs.ser.proofpoint.com/docs/email-submission
