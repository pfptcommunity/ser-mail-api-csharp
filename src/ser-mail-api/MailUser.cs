using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    // MailUser Class
    public class MailUser
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        public MailUser(string email, string name)
        {
            Email = email;
            Name = name;
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true // Enables pretty-printing
            });
        }
    }
}
