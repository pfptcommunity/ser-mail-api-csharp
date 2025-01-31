# Proofpoint Secure Email Relay Mail API Package
[![NuGet Downloads](https://img.shields.io/nuget/dt/Proofpoint.SecureEmailRelay.Mail.svg)](https://www.nuget.org/packages/Proofpoint.SecureEmailRelay.Mail)  
Library implements all the functions of the SER Email Relay API via C#.

### Requirements:

* .NET 6.0

### Sending an Email Message

```C#
using Proofpoint.SecureEmailRelay.Mail;
using System.Text;

Client client = new("<client_id>", "<client_secret>");

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
message.AddAttachment(Attachment.FromBase64String("", "empty.txt", "text/plain2", Disposition.Attachment));

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
```

The following JSON data is a dump of the message object based on the code above.

```json
{
  "attachments": [
    {
      "content": "",
      "disposition": "attachment",
      "filename": "empty.txt",
      "id": "2fa8a8cd-7661-4bfe-8a9b-2c1dde469206",
      "type": "text/plain"
    },
    {
      "content": "VGhpcyBpcyBhIHRlc3Qh",
      "disposition": "attachment",
      "filename": "test.txt",
      "id": "9adcbd55-325c-4f74-ad5b-6fe5fbefd3b0",
      "type": "text/plain"
    },
    {
      "content": "77u/IlVzZXIiLCJTZW50Q291bnQiLCJSZWNlaXZlZENvdW50Ig0KIm5vcmVwbHlAcHJvb2Zwb2ludC5jb20sIGxqZXJhYmVrQHBmcHQuaW8iLCIwIiwiMCINCg==",
      "disposition": "attachment",
      "filename": "file.csv",
      "id": "a43a0f62-02ac-4631-bb6d-ec1518b8b57c",
      "type": "text/csv"
    },
    {
      "content": "VGhpcyBpcyBhIHNhbXBsZSB0ZXh0IHN0cmVhbS4=",
      "disposition": "attachment",
      "filename": "bytes.txt",
      "id": "7a0c7928-0d14-456a-81d8-01e4169964f2",
      "type": "text/plain"
    },
    {
      "content": "",
      "disposition": "attachment",
      "filename": "nobytes.txt",
      "id": "b9dddb78-6448-4b7b-b766-a922e58d9ddf",
      "type": "text/plain"
    }
  ],
  "content": [
    {
      "body": "This is a test message",
      "type": "text/plain"
    },
    {
      "body": "\u003Cb\u003EThis is a test message\u003C/b\u003E",
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
      "email": "to_recipient1@proofpoint.com",
      "name": "Recipient 1"
    },
    {
      "email": "to_recipient2@proofpoint.com",
      "name": "Recipient 2"
    }
  ],
  "cc": [
    {
      "email": "cc_recipient1@proofpoint.com",
      "name": "Carbon Copy 1"
    },
    {
      "email": "cc_recipient2@proofpoint.com",
      "name": "Carbon Copy 2"
    }
  ],
  "bcc": [
    {
      "email": "bcc_recipient2@proofpoint.com",
      "name": "Blind Carbon Copy 1"
    },
    {
      "email": "bcc_recipient2@proofpoint.com",
      "name": "Blind Carbon Copy 2"
    }
  ],
  "replyTos": [
    {
      "email": "reply_to1@proofpoint.com",
      "name": "Reply To 1"
    },
    {
      "email": "reply_to2@proofpoint.com",
      "name": "Reply To 2"
    }
  ]
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

This type of content leads to use issuing an error.

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
