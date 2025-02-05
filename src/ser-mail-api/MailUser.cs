using System;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public class MailUser
    {
        private string _email = null!;

        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Email must not be null.");

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email must not be empty or contain only whitespace.", nameof(value));

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
            catch (FormatException ex)
            {
                throw new ArgumentException($"Invalid email format: '{email}'.", nameof(email), ex);
            }
        }
    }
}
