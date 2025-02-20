# Proofpoint Secure Email Relay Mail API C# Library
[![NuGet Downloads](https://img.shields.io/nuget/dt/Proofpoint.SecureEmailRelay.Mail.svg)](https://www.nuget.org/packages/Proofpoint.SecureEmailRelay.Mail)  

This library implements all the functions of the SER Email Relay API via **C#**.

## Requirements

- **.NET 6.0+**
- **HttpClient** (built-in in .NET)
- Active **Proofpoint SER API credentials**

### Installing the Package

You can install the library using **NuGet**:

```bash
# Using .NET CLI
dotnet add package Proofpoint.SecureEmailRelay.Mail

# Using NuGet Package Manager
Install-Package Proofpoint.SecureEmailRelay.Mail
```

## Features

- **Send Emails**: Easily compose and send emails with minimal code.
- **Support for Attachments**:
    - Attach files from disk
    - Encode attachments as Base64
    - Send `byte[]` attachments
- **Support for Inline HTML Content**:
    - Using the syntax `<img src="cid:logo">`
    - Content-ID can be set manually or auto-generated
- **HTML & Plain Text Content**: Supports both plain text and HTML email bodies.
- **Recipient Management**: Add `To`, `CC`, and `BCC` recipients with ease.
- **Reply Management**: Add `Reply-To` addresses to redirect replies.

## Quick Start

```csharp
using Proofpoint.SecureEmailRelay.Mail;

class Program
{
    static async Task Main()
    {
        var client = new Client("<client_id>", "<client_secret>");

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
        message.AddAttachment(Attachment.FromBase64("VGhpcyBpcyBhIHRlc3Qh", "test.txt"));
        message.AddAttachment(Attachment.FromFile("C:/temp/file.csv"));
        message.AddAttachment(Attachment.FromBytes(new byte[] { 1, 2, 3 }, "bytes.txt", "text/plain"));

        // Set Reply-To
        message.AddReplyTo(new MailUser("noreply@proofpoint.com", "No Reply"));

        // Send the email
        var result = await client.Send(message);

        Console.WriteLine($"HTTP Response: {result.HttpResponse.StatusCode}/{(int)result.HttpResponse.StatusCode}");
        Console.WriteLine($"Reason: {result.Reason}");
        Console.WriteLine($"Message ID: {result.MessageId}");
        Console.WriteLine($"Request ID: {result.RequestId}");
    }
}
```

## Attachment MIME Type Deduction Behavior

- When creating attachments, the library **automatically determines the MIME type** based on the file extension.
- If the MIME type cannot be determined, an exception is raised.

```csharp
// Create an attachment from disk; the MIME type will be "text/csv", and disposition will be "Disposition.Attachment"
Attachment.FromFile("C:/temp/file.csv");

// This will throw an error, as the MIME type is unknown
Attachment.FromFile("C:/temp/file.unknown");

// Create an attachment and specify the type information. The disposition will be "Disposition.Attachment", filename will be unknown.txt, and MIME type "text/plain"
Attachment.FromFile("C:/temp/file.unknown", filename: "unknown.txt");

// Create an attachment and specify the type information. The disposition will be "Disposition.Attachment", filename will be file.unknown, and MIME type "text/plain"
Attachment.FromFile("C:/temp/file.unknown", mimeType: "text/plain");
```

## Inline Attachments and Content-IDs

When creating attachments, they are `Disposition.Attachment` by default. To properly reference a **Content-ID** (e.g.,
`<img src="cid:logo">`), you must explicitly set the attachment disposition to `Disposition.Inline`.
If the attachment type is set to `Disposition.Inline`, a default unique **Content-ID** will be generated.

### Using a Dynamically Generated Content-ID
```csharp
var logo = Attachment.FromFile("C:/temp/logo.png", Disposition.Inline);
message.AddContent(new Content($"<b>Test</b><br><img src=\"cid:{logo.ContentId}\">", ContentType.Html));
message.AddAttachment(logo);
```

### Setting a Custom Content-ID
```csharp
message.AddAttachment(Attachment.FromFile("C:/temp/logo.png", Disposition.Inline, "logo"));
message.AddContent(new Content("<b>Test</b><br><img src=\"cid:logo\">", ContentType.Html));
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

## Known Issues

There is a known issue where **empty file content** results in a **400 Bad Request** error.

```json
{
  "content": "",
  "disposition": "attachment",
  "filename": "empty.txt",
  "id": "1ed38149-70b2-4476-84a1-83e73913d43c",
  "type": "text/plain"
}
```

🔹 **API Response:**

```
Status Code: 400/BadRequest
Reason: attachments[0].content is required
Message ID:
Request ID: fe9a1acf60a20c9d90bed843f6530156
Raw JSON: {"request_id":"fe9a1acf60a20c9d90bed843f6530156","reason":"attachments[0].content is required"}
```

This issue has been reported to **Proofpoint Product Management**.

## Limitations
- The Proofpoint API currently does not support **empty file attachments**.
- If an empty file is sent, you will receive a **400 Bad Request** error.

## Additional Resources
For more information, refer to the official **Proofpoint Secure Email Relay API documentation**:  
[**API Documentation**](https://api-docs.ser.proofpoint.com/docs/email-submission)

