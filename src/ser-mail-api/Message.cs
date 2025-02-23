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
        /// Initializes a new instance of <see cref="Message"/> with all required and optional fields.
        /// </summary>
        /// <param name="subject">The subject line of the message.</param>
        /// <param name="from">The sender of the message.</param>
        /// <param name="headerFrom">The sender as specified in the message headers, or null.</param>
        /// <param name="content">The list of content bodies for the message.</param>
        /// <param name="attachments">The list of attachments for the message.</param>
        /// <param name="tos">The list of primary recipients (To).</param>
        /// <param name="ccs">The list of carbon copy (CC) recipients.</param>
        /// <param name="bccs">The list of blind carbon copy (BCC) recipients.</param>
        /// <param name="replyTos">The list of reply-to recipients.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subject"/>, <paramref name="from"/>, or any collection parameter is null.</exception>
        internal Message(string subject, MailUser from, MailUser? headerFrom, List<Content> content, List<Attachment> attachments, List<MailUser> tos, List<MailUser> ccs, List<MailUser> bccs, List<MailUser> replyTos)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject), "Subject must not be null.");
            From = from ?? throw new ArgumentNullException(nameof(from), "Sender must not be null.");
            HeaderFrom = headerFrom;
            Content = content ?? throw new ArgumentNullException(nameof(content), "Content list must not be null.");
            Attachments = attachments ?? throw new ArgumentNullException(nameof(attachments), "Attachments list must not be null.");
            Tos = tos ?? throw new ArgumentNullException(nameof(tos), "To recipients list must not be null.");
            Cc = ccs ?? throw new ArgumentNullException(nameof(ccs), "CC recipients list must not be null.");
            Bcc = bccs ?? throw new ArgumentNullException(nameof(bccs), "BCC recipients list must not be null.");
            ReplyTos = replyTos ?? throw new ArgumentNullException(nameof(replyTos), "ReplyTo recipients list must not be null.");
        }

        public void AddAttachment(Attachment attachment) => Attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));
        public void AddContent(Content content) => Content.Add(content ?? throw new ArgumentNullException(nameof(content)));
        public void AddTo(MailUser to) => Tos.Add(to ?? throw new ArgumentNullException(nameof(to)));
        public void AddCc(MailUser cc) => Cc.Add(cc ?? throw new ArgumentNullException(nameof(cc)));
        public void AddBcc(MailUser bcc) => Bcc.Add(bcc ?? throw new ArgumentNullException(nameof(bcc)));
        public void AddReplyTo(MailUser replyTo) => ReplyTos.Add(replyTo ?? throw new ArgumentNullException(nameof(replyTo)));

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        /// <summary>
        /// Creates a new builder instance for constructing a <see cref="Message"/> fluently.
        /// </summary>
        /// <returns>A new <see cref="MessageBuilder"/> instance.</returns>
        public static MessageBuilder Builder() => new MessageBuilder();

        /// <summary>
        /// Defines the interface for building a <see cref="Message"/> with required and optional properties in any order.
        /// </summary>
        public interface IBuilder
        {
            /// <summary>
            /// Sets the sender of the message.
            /// </summary>
            /// <param name="from">The sender of the message.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="from"/> is null.</exception>
            IBuilder From(MailUser from);

            /// <summary>
            /// Sets the sender of the message using an email address and optional name.
            /// </summary>
            /// <param name="email">The email address of the sender.</param>
            /// <param name="name">The optional name of the sender.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder From(string email, string? name = null);

            /// <summary>
            /// Sets the subject of the message.
            /// </summary>
            /// <param name="subject">The subject line of the message.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="subject"/> is null.</exception>
            IBuilder Subject(string subject);

            /// <summary>
            /// Adds a primary recipient (To) to the message.
            /// </summary>
            /// <param name="email">The email address of the recipient.</param>
            /// <param name="name">The optional name of the recipient.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder To(string email, string? name = null);

            /// <summary>
            /// Adds a primary recipient (To) to the message using an existing <see cref="MailUser"/>.
            /// </summary>
            /// <param name="to">The recipient to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="to"/> is null.</exception>
            IBuilder To(MailUser to);

            /// <summary>
            /// Adds content to the message body with the specified body text and type.
            /// </summary>
            /// <param name="body">The body text of the content.</param>
            /// <param name="type">The type of the content (text or HTML).</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="body"/> is empty or whitespace.</exception>
            IBuilder Content(string body, ContentType type);

            /// <summary>
            /// Adds content to the message body using an existing <see cref="Content"/> object.
            /// </summary>
            /// <param name="content">The content object to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
            IBuilder Content(Content content);

            /// <summary>
            /// Adds an attachment to the message.
            /// </summary>
            /// <param name="attachment">The attachment to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="attachment"/> is null.</exception>
            IBuilder Attachment(Attachment attachment);

            /// <summary>
            /// Adds a carbon copy (CC) recipient to the message.
            /// </summary>
            /// <param name="email">The email address of the recipient.</param>
            /// <param name="name">The optional name of the recipient.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder Cc(string email, string? name = null);

            /// <summary>
            /// Adds a carbon copy (CC) recipient to the message using an existing <see cref="MailUser"/>.
            /// </summary>
            /// <param name="cc">The recipient to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="cc"/> is null.</exception>
            IBuilder Cc(MailUser cc);

            /// <summary>
            /// Adds a blind carbon copy (BCC) recipient to the message.
            /// </summary>
            /// <param name="email">The email address of the recipient.</param>
            /// <param name="name">The optional name of the recipient.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder Bcc(string email, string? name = null);

            /// <summary>
            /// Adds a blind carbon copy (BCC) recipient to the message using an existing <see cref="MailUser"/>.
            /// </summary>
            /// <param name="bcc">The recipient to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="bcc"/> is null.</exception>
            IBuilder Bcc(MailUser bcc);

            /// <summary>
            /// Adds a reply-to recipient to the message.
            /// </summary>
            /// <param name="email">The email address of the recipient.</param>
            /// <param name="name">The optional name of the recipient.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder ReplyTo(string email, string? name = null);

            /// <summary>
            /// Adds a reply-to recipient to the message using an existing <see cref="MailUser"/>.
            /// </summary>
            /// <param name="replyTo">The reply-to recipient to add.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="replyTo"/> is null.</exception>
            IBuilder ReplyTo(MailUser replyTo);

            /// <summary>
            /// Sets the sender as specified in the message headers.
            /// </summary>
            /// <param name="email">The email address of the header sender.</param>
            /// <param name="name">The optional name of the header sender.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or invalid.</exception>
            IBuilder HeaderFrom(string email, string? name = null);

            /// <summary>
            /// Sets the sender as specified in the message headers using an existing <see cref="MailUser"/>.
            /// </summary>
            /// <param name="headerFrom">The header sender to set.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="headerFrom"/> is null.</exception>
            IBuilder HeaderFrom(MailUser headerFrom);

            /// <summary>
            /// Builds and returns the final <see cref="Message"/> instance, ensuring minimum requirements are met.
            /// </summary>
            /// <returns>The constructed <see cref="Message"/>.</returns>
            /// <exception cref="InvalidOperationException">Thrown if From, at least one To recipient, Subject, or at least one Content body is missing.</exception>
            Message Build();
        }

        public class MessageBuilder : IBuilder
        {
            private string? _subject;
            private MailUser? _from;
            private MailUser? _headerFrom;
            private readonly List<Content> _content = new();
            private readonly List<Attachment> _attachments = new();
            private readonly List<MailUser> _tos = new();
            private readonly List<MailUser> _ccs = new();
            private readonly List<MailUser> _bccs = new();
            private readonly List<MailUser> _replyTos = new();

            internal MessageBuilder() { }

            public IBuilder From(MailUser from)
            {
                _from = from ?? throw new ArgumentNullException(nameof(from), "Sender must not be null.");
                return this;
            }

            public IBuilder From(string email, string? name = null)
            {
                _from = new MailUser(email, name);
                return this;
            }

            public IBuilder Subject(string subject)
            {
                _subject = subject ?? throw new ArgumentNullException(nameof(subject), "Subject must not be null.");
                return this;
            }

            public IBuilder To(MailUser to)
            {
                _tos.Add(to ?? throw new ArgumentNullException(nameof(to)));
                return this;
            }

            public IBuilder To(string email, string? name = null)
            {
                _tos.Add(new MailUser(email, name));
                return this;
            }

            public IBuilder Content(Content content)
            {
                _content.Add(content ?? throw new ArgumentNullException(nameof(content)));
                return this;
            }

            public IBuilder Content(string body, ContentType type)
            {
                _content.Add(new Content(body, type));
                return this;
            }

            public IBuilder Attachment(Attachment attachment)
            {
                _attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));
                return this;
            }

            public IBuilder Cc(MailUser cc)
            {
                _ccs.Add(cc ?? throw new ArgumentNullException(nameof(cc)));
                return this;
            }

            public IBuilder Cc(string email, string? name = null)
            {
                _ccs.Add(new MailUser(email, name));
                return this;
            }

            public IBuilder Bcc(MailUser bcc)
            {
                _bccs.Add(bcc ?? throw new ArgumentNullException(nameof(bcc)));
                return this;
            }

            public IBuilder Bcc(string email, string? name = null)
            {
                _bccs.Add(new MailUser(email, name));
                return this;
            }

            public IBuilder ReplyTo(MailUser replyTo)
            {
                _replyTos.Add(replyTo ?? throw new ArgumentNullException(nameof(replyTo)));
                return this;
            }

            public IBuilder ReplyTo(string email, string? name = null)
            {
                _replyTos.Add(new MailUser(email, name));
                return this;
            }

            public IBuilder HeaderFrom(MailUser headerFrom)
            {
                _headerFrom = headerFrom ?? throw new ArgumentNullException(nameof(headerFrom));
                return this;
            }

            public IBuilder HeaderFrom(string email, string? name = null)
            {
                _headerFrom = new MailUser(email, name);
                return this;
            }

            public Message Build()
            {
                if (_from == null)
                    throw new InvalidOperationException("A sender (From) is required. Call From() before building.");
                if (_tos.Count == 0)
                    throw new InvalidOperationException("At least one To recipient is required. Call To() before building.");
                if (_subject == null)
                    throw new InvalidOperationException("A subject is required. Call Subject() before building.");
                if (_content.Count == 0)
                    throw new InvalidOperationException("At least one content body is required. Call Content() before building.");

                return new Message(_subject, _from, _headerFrom, _content, _attachments, _tos, _ccs, _bccs, _replyTos);
            }
        }
    }
}