using System.Text.Json;
using System.Text.Json.Serialization;

namespace ser_mail_api
{
    public class Message
    {
        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        [JsonPropertyName("content")]
        public List<Content> Content { get; set; } = new List<Content>();

        [JsonPropertyName("from")]
        public MailUser From { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, MailUser> Headers { get; set; } = new Dictionary<string, MailUser>();

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("tos")]
        public List<MailUser> Tos { get; set; } = new List<MailUser>();

        [JsonPropertyName("cc")]
        public List<MailUser> Cc { get; set; } = new List<MailUser>();

        [JsonPropertyName("bcc")]
        public List<MailUser> Bcc { get; set; } = new List<MailUser>();

        [JsonPropertyName("replyTos")]
        public List<MailUser> ReplyTos { get; set; } = new List<MailUser>();

        public Message(string subject, MailUser from)
        {
            Subject = subject;
            From = from;
            Headers["from"] = from;
        }

        public void AddAttachment(Attachment attachment) => Attachments.Add(attachment);
        public void AddContent(Content content) => Content.Add(content);
        public void AddTo(MailUser to) => Tos.Add(to);
        public void AddCc(MailUser cc) => Cc.Add(cc);
        public void AddBcc(MailUser bcc) => Bcc.Add(bcc);
        public void AddReplyTo(MailUser replyTo) => ReplyTos.Add(replyTo);

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true // Enables pretty-printing
            });
        }
    }
}
