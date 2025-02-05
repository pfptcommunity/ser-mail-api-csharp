using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class MessageHeaders
    {
        private MailUser _from = null!;

        [JsonPropertyName("from")]
        public MailUser From
        {
            get => _from;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Header from address must not be null.");

                _from = value;
            }
        }

        public MessageHeaders(MailUser from)
        {
            From = from;
        }
    }
}
