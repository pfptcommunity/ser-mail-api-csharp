using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class Message
    {
        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; } = new();

        [JsonPropertyName("content")]
        public List<Content> Content { get; } = new();

        [JsonPropertyName("from")]
        public MailUser From { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("headers")]
        public MessageHeaders? Headers { get; private set; }

        [JsonPropertyName("subject")]
        public string Subject { get; }

        [JsonPropertyName("tos")]
        public List<MailUser> Tos { get; } = new();

        [JsonPropertyName("cc")]
        public List<MailUser> Cc { get; } = new();

        [JsonPropertyName("bcc")]
        public List<MailUser> Bcc { get; } = new();

        [JsonPropertyName("replyTos")]
        public List<MailUser> ReplyTos { get; } = new();

        [JsonIgnore]
        public MailUser? HeaderFrom
        {
            get => Headers?.From;
            set
            {
                if (value == null)
                {
                    Headers = null;
                }
                else if(Headers == null)
                {
                    Headers = new MessageHeaders(value);
                }
                else
                {
                    Headers.From = value;
                }
            }
        }

        public Message(string subject, MailUser from, MailUser headerFrom)
            : this(subject, from)
        {
            HeaderFrom = headerFrom;
        }


        public Message(string subject, MailUser from)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject), "Subject must not be null.");
            From = from ?? throw new ArgumentNullException(nameof(from), "Sender must not be null.");
        }

        public void AddAttachment(Attachment attachment) => Attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));
        public void AddContent(Content content) => Content.Add(content ?? throw new ArgumentNullException(nameof(content)));
        public void AddTo(MailUser to) => Tos.Add(to ?? throw new ArgumentNullException(nameof(to)));
        public void AddCc(MailUser cc) => Cc.Add(cc ?? throw new ArgumentNullException(nameof(cc)));
        public void AddBcc(MailUser bcc) => Bcc.Add(bcc ?? throw new ArgumentNullException(nameof(bcc)));
        public void AddReplyTo(MailUser replyTo) => ReplyTos.Add(replyTo ?? throw new ArgumentNullException(nameof(replyTo)));

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
