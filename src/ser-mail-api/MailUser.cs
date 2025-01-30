using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class MailUser
    {
        private string _email = "";
        private string _name = "";

        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set
            {
                value = value.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be null or empty.", nameof(value));

                if (!IsValidEmail(value))
                    throw new ArgumentException($"Invalid email format: {value}", nameof(value));

                _email = value;
            }
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set => _name = value.Trim();
        }

        public MailUser(string email) : this(email, "") { }

        public MailUser(string email, string name)
        {
            Email = email.Trim();
            Name = name.Trim();
        }

        private static bool IsValidEmail(string email)
        {
            return MailAddress.TryCreate(email, out _);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
