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

- **Send Emails**: Easily compose and send emails with minimal code using a fluent builder pattern.
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

        // Create a new Message object using the fluid builder
        var message = Message.Builder()
            .From("sender@example.com", "Joe Sender")
            .To("recipient1@example.com", "Recipient 1")
            .To("recipient2@example.com", "Recipient 2")
            .Subject("This is a test email")
            .Content("This is a test message", ContentType.Text) // Plain text alternative
            .Attachment(Attachment.Builder()
                .FromFile("C:/temp/logo.png")
                .DispositionInline(out string logoId) // Dynamic Content-ID
                .Build())
            .Content($"<b>This is a test message</b><br><img src=\"cid:{logoId}\">", ContentType.Html) // Single HTML body
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
                .MimeType("text/plain")
                .Build())
            .ReplyTo("noreply@proofpoint.com", "No Reply")
            .Build();

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
// MIME type deduced as "text/csv", disposition defaults to "Disposition.Attachment"
Attachment.Builder().FromFile("C:/temp/file.csv").Build();

// Throws an error due to unknown MIME type
Attachment.Builder().FromFile("C:/temp/file.unknown").Build();

// Specify filename and MIME type for unknown extension
Attachment.Builder()
    .FromFile("C:/temp/file.unknown")
    .Filename("unknown.txt")
    .MimeType("text/plain")
    .Build();
```

## Inline Attachments and Content-IDs

Attachments default to Disposition.Attachment. For inline attachments referenced in HTML (e.g., `<img src="cid:logo">`), use AttachmentBuilder’s `DispositionInline` methods to set the disposition and manage the `ContentId`:

### Using a Dynamically Generated Content-ID

Generate a unique ContentId (UUID) inline and use it in the HTML body:
```csharp
var message = Message.Builder()
    .From("sender@example.com", "Joe Sender")
    .To("recipient@example.com")
    .Subject("Test Email")
    .Content("Plain text fallback", ContentType.Text)
    .Attachment(Attachment.Builder()
        .FromFile("C:/temp/logo.png")
        .DispositionInline(out string logoId) // Dynamic UUID
        .Build())
    .Content($"<b>Test</b><br><img src=\"cid:{logoId}\">", ContentType.Html)
    .Build();
```

### Setting a Custom Content-ID

Specify a custom `ContentId` directly within the builder:

```csharp
var message = Message.Builder()
    .From("sender@example.com", "Joe Sender")
    .To("recipient@example.com")
    .Subject("Test Email")
    .Content("Plain text fallback", ContentType.Text)
    .Attachment(Attachment.Builder()
        .FromFile("C:/temp/logo.png")
        .DispositionInline("logo") // Custom ContentId
        .Build())
    .Content("<b>Test</b><br><img src=\"cid:logo\">", ContentType.Html)
    .Build();
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

