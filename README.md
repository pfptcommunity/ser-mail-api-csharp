# **Proofpoint Secure Email Relay Mail API Package**
[![NuGet Downloads](https://img.shields.io/nuget/dt/Proofpoint.SecureEmailRelay.Mail.svg)](https://www.nuget.org/packages/Proofpoint.SecureEmailRelay.Mail)

The **Proofpoint Secure Email Relay Mail API** library implements all the functions of the **Secure Email Relay (SER) API** via C#. This library allows developers to send **emails with plain text, HTML content, attachments, and inline images** while integrating seamlessly with **Proofpoint's Secure Email Relay API**.

## **Requirements**
- **.NET 6.0 or later**
- **Active Proofpoint SER API credentials**

---

## **Sending a Simple Email with Plain Text and HTML Content**
The example below demonstrates sending a **basic email** with both **plain text and HTML content**.

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

---

## **Sending an Email with Attachments**
### **1️⃣ Sending a File from Disk**
The following example shows how to **send an email with an attachment** from a local file.

```csharp
Message message = new("Email with Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains an attachment.", ContentType.Text));

message.AddAttachment(Attachment.CreateBuilder()
    .FromFile(@"C:\path\to\document.pdf")
    .SetDisposition(Disposition.Attachment)
    .Build());

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);
```

---

### **2️⃣ Sending an Inline Image (Embedded in Email)**
Inline images can be embedded in HTML content using **Content-ID**.

```csharp
Message message = new("Email with Inline Image", new MailUser("sender@example.com", "John Doe"));

// Create an inline image attachment
var logo = new Attachment(
    content: Convert.ToBase64String(File.ReadAllBytes("C:\\path\\logo.png")),
    filename: "logo.png",
    mimeType: "image/png",
    disposition: Disposition.Inline
);

message.AddAttachment(logo);

// Reference the attachment inside the email body using CID
message.AddContent(new Content($"<img src=\"cid:{logo.Id}\">", ContentType.Html));

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);
```

---

### **3️⃣ Sending a Base64 Encoded Attachment**
You can also send an attachment using **Base64 encoding**.

```csharp
string base64Content = Convert.ToBase64String(File.ReadAllBytes(@"C:\path\to\document.pdf"));

Message message = new("Email with Base64 Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains a Base64 encoded attachment.", ContentType.Text));

message.AddAttachment(Attachment.CreateBuilder()
    .FromBase64String(base64Content)
    .SetFilename("document.pdf")
    .SetMimeType("application/pdf")
    .SetDisposition(Disposition.Attachment)
    .Build());

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);
```

---

### **4️⃣ Sending an Attachment from a Byte Array**
This example demonstrates how to **attach a file using a byte array**.

```csharp
byte[] fileBytes = Encoding.UTF8.GetBytes("This is a sample text file.");

Message message = new("Email with Byte Array Attachment", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email contains a text file attachment.", ContentType.Text));

message.AddAttachment(Attachment.CreateBuilder()
    .FromBytes(fileBytes)
    .SetFilename("sample.txt")
    .SetMimeType("text/plain")
    .SetDisposition(Disposition.Attachment)
    .Build());

message.AddTo(new MailUser("recipient@example.com", "Jane Doe"));

SendResult sendResult = await client.Send(message);
```

---

## **Sending an Email with Multiple Recipients (To, CC, BCC)**
The following example demonstrates how to send an email to **multiple recipients**.

```csharp
Message message = new("Multi-Recipient Email", new MailUser("sender@example.com", "John Doe"));

message.AddContent(new Content("This email has multiple recipients.", ContentType.Text));

message.AddTo(new MailUser("to1@example.com", "To Recipient 1"));
message.AddTo(new MailUser("to2@example.com", "To Recipient 2"));

message.AddCc(new MailUser("cc1@example.com", "CC Recipient 1"));
message.AddCc(new MailUser("cc2@example.com", "CC Recipient 2"));

message.AddBcc(new MailUser("bcc1@example.com", "BCC Recipient 1"));
message.AddBcc(new MailUser("bcc2@example.com", "BCC Recipient 2"));

SendResult sendResult = await client.Send(message);
```

---

## **Proxy Support**
`Proofpoint.SecureEmailRelay.Mail` supports **HTTP and HTTPS proxies** by allowing users to pass a custom `HttpClientHandler` when initializing the `Client`.

```csharp
using System.Net;

// Configure an HTTP/HTTPS proxy
var proxy = new WebProxy("http://your-proxy-server:port")
{
    Credentials = new NetworkCredential("your-username", "your-password")
};

var httpClientHandler = new HttpClientHandler
{
    Proxy = proxy,
    UseProxy = true
};

// Initialize the client with proxy support
Client client = new("<client_id>", "<client_secret>", httpClientHandler);
```

---

## **Known Issues**
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
Status Code: 400 BadRequest
Message ID:
Reason: attachments[0].content is required
Request ID: fe9a1acf60a20c9d90bed843f6530156
Raw JSON: {"request_id":"fe9a1acf60a20c9d90bed843f6530156","reason":"attachments[0].content is required"}
```
This issue has been reported to **Proofpoint Product Management**.

---

## **Limitations**
- Currently, **empty attachments are not supported** by the API.
- No other known limitations.

---

## **Additional Resources**
For more information, refer to the official **Proofpoint Secure Email Relay API documentation**:  
[**API Documentation**](https://api-docs.ser.proofpoint.com/docs/email-submission)
