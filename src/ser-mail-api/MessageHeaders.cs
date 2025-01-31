using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    sealed public class MessageHeaders
    {
        [JsonPropertyName("from")]
        public MailUser From { get; set; }

        public MessageHeaders(MailUser from)
        {
            From = from;
        }
    }
}
