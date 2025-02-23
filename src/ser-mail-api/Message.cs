using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Represents a mail message with sender, recipients, content, attachments, and optional headers.
    /// </summary>
    public sealed class Message
    {
        /// <summary>
        /// Gets the list of attachments included in the message.
        /// </summary>
        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; } = new();

        /// <summary>
        /// Gets the list of content items (e.g., text or HTML) for the message body.
        /// </summary>
        [JsonPropertyName("content")]
        public List<Content> Content { get; } = new();

        /// <summary>
        /// Gets the sender of the message.
        /// </summary>
        [JsonPropertyName("from")]
        public MailUser From { get; }

        /// <summary>
        /// Gets or sets the optional message headers, ignored in JSON if null.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("headers")]
        public MessageHeaders? Headers { get; private set; }

        /// <summary>
        /// Gets the subject line of the message.
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; }

        /// <summary>
        /// Gets the list of primary recipients (To) of the message.
        /// </summary>
        [JsonPropertyName("tos")]
        public List<MailUser> Tos { get; } = new();

        /// <summary>
        /// Gets the list of carbon copy (CC) recipients of the message.
        /// </summary>
        [JsonPropertyName("cc")]
        public List<MailUser> Cc { get; } = new();

        /// <summary>
        /// Gets the list of blind carbon copy (BCC) recipients of the message.
        /// </summary>
        [JsonPropertyName("bcc")]
        public List<MailUser> Bcc { get; } = new();

        /// <summary>
        /// Gets the list of reply-to recipients of the message.
        /// </summary>
        [JsonPropertyName("replyTos")]
        public List<MailUser> ReplyTos { get; } = new();

        /// <summary>
        /// Gets or sets the sender as specified in the message headers, ignored in JSON serialization.
        /// </summary>
        /// <remarks>Setting this property to null clears the <see cref="Headers"/>, while a non-null value initializes or updates the headers.</remarks>
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
                else if (Headers == null)
                {
                    Headers = new MessageHeaders(value);
                }
                else
                {
                    Headers.From = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Message"/> with a subject, sender, and optional header sender.
        /// </summary>
        /// <param name="subject">The subject line of the message.</param>
        /// <param name="from">The sender of the message.</param>
        /// <param name="headerFrom">The sender as specified in the message headers.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subject"/> or <paramref name="from"/> is null.</exception>
        public Message(string subject, MailUser from, MailUser headerFrom)
            : this(subject, from)
        {
            HeaderFrom = headerFrom;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Message"/> with a subject and sender.
        /// </summary>
        /// <param name="subject">The subject line of the message.</param>
        /// <param name="from">The sender of the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subject"/> or <paramref name="from"/> is null.</exception>
        public Message(string subject, MailUser from)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject), "Subject must not be null.");
            From = from ?? throw new ArgumentNullException(nameof(from), "Sender must not be null.");
        }

        /// <summary>
        /// Adds an attachment to the message.
        /// </summary>
        /// <param name="attachment">The attachment to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
        public void AddAttachment(Attachment attachment) => Attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));

        /// <summary>
        /// Adds a content item to the message body.
        /// </summary>
        /// <param name="content">The content to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
        public void AddContent(Content content) => Content.Add(content ?? throw new ArgumentNullException(nameof(content)));

        /// <summary>
        /// Adds a primary recipient (To) to the message.
        /// </summary>
        /// <param name="to">The recipient to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="to"/> is null.</exception>
        public void AddTo(MailUser to) => Tos.Add(to ?? throw new ArgumentNullException(nameof(to)));

        /// <summary>
        /// Adds a carbon copy (CC) recipient to the message.
        /// </summary>
        /// <param name="cc">The recipient to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cc"/> is null.</exception>
        public void AddCc(MailUser cc) => Cc.Add(cc ?? throw new ArgumentNullException(nameof(cc)));

        /// <summary>
        /// Adds a blind carbon copy (BCC) recipient to the message.
        /// </summary>
        /// <param name="bcc">The recipient to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bcc"/> is null.</exception>
        public void AddBcc(MailUser bcc) => Bcc.Add(bcc ?? throw new ArgumentNullException(nameof(bcc)));

        /// <summary>
        /// Adds a reply-to recipient to the message.
        /// </summary>
        /// <param name="replyTo">The reply-to recipient to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="replyTo"/> is null.</exception>
        public void AddReplyTo(MailUser replyTo) => ReplyTos.Add(replyTo ?? throw new ArgumentNullException(nameof(replyTo)));

        /// <summary>
        /// Returns a JSON string representation of the message.
        /// </summary>
        /// <returns>A formatted JSON string.</returns>
        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}