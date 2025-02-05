using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public class MailUser
    {
        private string _email;

        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email address cannot be null or empty.", nameof(value));

                ValidateEmail(value);
                _email = value;
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string? Name { get; }

        public MailUser(string email) : this(email, null) { }

        public MailUser(string email, string? name = null)
        {
            Email = email;
            Name = name;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static void ValidateEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid email format", nameof(email));
            }
        }
    }
}
