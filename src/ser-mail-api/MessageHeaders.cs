using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Represents the headers of a mail message, specifically the sender information.
    /// </summary>
    public sealed class MessageHeaders
    {
        /// <summary>
        /// Gets the sender of the message as specified in the headers.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the value is set to null.</exception>
        [JsonPropertyName("from")]
        public MailUser From { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="MessageHeaders"/> with the specified sender.
        /// </summary>
        /// <param name="from">The sender of the message as specified in the headers.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="from"/> is null.</exception>
        public MessageHeaders(MailUser from)
        {
            From = from ?? throw new ArgumentNullException(nameof(from), "Header from address must not be null.");
        }
    }
}