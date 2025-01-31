# Proofpoint Secure Email Relay Mail API Package
![NuGet Downloads](https://img.shields.io/nuget/dt/Proofpoint.SecureEmailRelay.Mail.svg)  
Library implements all the functions of the SER Email Relay API via C#.

### Requirements:

* .NET 6.0

### Creating an API client object

```C#
using Proofpoint.SecureEmailRelay.Mail;

Client client = new("<client_id>", "<client_secret>");
```

### Sending an Email Message

```C#
using Proofpoint.SecureEmailRelay.Mail;
using System.Text;

Client client = new("<client_id>", "<client_secret>");

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
message.AddAttachment(new Attachment("", "empty.txt", "text/plain", Disposition.Attachment));

// Add Base64 encoded attachment
message.AddAttachment(new Attachment("VGhpcyBpcyBhIHRlc3Qh", "test.txt", "text/plain", Disposition.Attachment));

// Add file attachment from disk, if disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(new FileAttachment(@"C:\temp\file.csv", Disposition.Attachment));

// Convert the string to a byte array using UTF8 encoding
byte[] bytes = Encoding.UTF8.GetBytes("This is a sample text stream.");

// Add byte array as attachment, if disposition is not passed, the default is Disposition.Attachment
message.AddAttachment(new BinaryAttachment(bytes, "bytes.txt", "text/plain", Disposition.Attachment));

// Add empty attachment, this currently doesn't work with the REST API. 
message.AddAttachment(new BinaryAttachment(Array.Empty<byte>(), "nobytes.txt", "text/plain", Disposition.Attachment));

// Print Message object as JSON text
Console.WriteLine(message.ToString());

SendResult sendResult = await client.Send(message);

Console.WriteLine($"Status Code: {(int)sendResult.HttpResponse.StatusCode} {sendResult.HttpResponse.StatusCode}");
Console.WriteLine($"Message ID: {sendResult.MessageId}");
Console.WriteLine($"Reason: {sendResult.Reason}");
Console.WriteLine($"Request ID: {sendResult.RequestId}");
Console.WriteLine($"Raw JSON: {sendResult.RawJson}");
```

The following JSON data is a dump of the message object based on the code above.

```json
{
  "attachments": [
    {
      "content": "VGhpcyBpcyBhIHRlc3Qh",
      "disposition": "attachment",
      "filename": "test.txt",
      "id": "d10205cf-a0a3-4b9e-9a57-253fd8e1c7df",
      "type": "text/plain"
    },
    {
      "content": "77u/IlVzZXIiLCJTZW50Q291bnQiLCJSZWNlaXZlZENvdW50Ig0KIm5vcmVwbHlAcHJvb2Zwb2ludC5jb20sIGxqZXJhYmVrQHBmcHQuaW8iLCIwIiwiMCINCg==",
      "disposition": "attachment",
      "filename": "file.csv",
      "id": "f66487f5-57c2-40e0-9402-5723a85c0df0",
      "type": "application/vnd.ms-excel"
    },
    {
      "content": "VGhpcyBpcyBhIHNhbXBsZSB0ZXh0IHN0cmVhbS4=",
      "disposition": "attachment",
      "filename": "byte_stream.txt",
      "id": "bc67d5fa-345a-4436-9979-5efa68223520",
      "type": "text/plain"
    }
  ],
  "content": [
    {
      "body": "This is a test message",
      "type": "text/plain"
    },
    {
      "body": "<b>This is a test message</b>",
      "type": "text/html"
    }
  ],
  "from": {
    "email": "sender@proofpoint.com",
    "name": "Joe Sender"
  },
  "headers": {
    "from": {
      "email": "sender@proofpoint.com",
      "name": "Joe Sender"
    }
  },
  "subject": "This is a test email",
  "tos": [
    {
      "email": "recipient1@proofpoint.com",
      "name": "Recipient 1"
    },
    {
      "email": "recipient2@proofpoint.com",
      "name": "Recipient 2"
    }
  ],
  "cc": [
    {
      "email": "cc1@proofpoint.com",
      "name": "Carbon Copy 1"
    },
    {
      "email": "cc2@proofpoint.com",
      "name": "Carbon Copy 2"
    }
  ],
  "bcc": [
    {
      "email": "bcc1@proofpoint.com",
      "name": "Blind Carbon Copy 1"
    },
    {
      "email": "bcc2@proofpoint.com",
      "name": "Blind Carbon Copy 2"
    }
  ],
  "replyTos": []
}
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

### Limitations

There are no known limitations.

For more information please see: https://api-docs.ser.proofpoint.com/docs/email-submission
