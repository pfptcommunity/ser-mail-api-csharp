using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Represents a mail user with an email address and an optional name.
    /// </summary>
    public class MailUser
    {
        /// <summary>
        /// The backing field for the <see cref="Email"/> property.
        /// </summary>
        private string _email = null!;

        /// <summary>
        /// Gets or sets the email address of the mail user.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the value is set to null.</exception>
        /// <exception cref="ArgumentException">Thrown when the value is empty, whitespace, or an invalid email format.</exception>
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

        /// <summary>
        /// Gets the optional name of the mail user, or null if not specified.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string? Name { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="MailUser"/> with the specified email address and no name.
        /// </summary>
        /// <param name="email">The email address of the mail user.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or an invalid format.</exception>
        public MailUser(string email) : this(email, null) { }

        /// <summary>
        /// Initializes a new instance of <see cref="MailUser"/> with the specified email address and optional name.
        /// </summary>
        /// <param name="email">The email address of the mail user.</param>
        /// <param name="name">The optional name of the mail user, or null if not specified.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is empty, whitespace, or an invalid format.</exception>
        public MailUser(string email, string? name = null)
        {
            Email = email;
            Name = name;
        }

        /// <summary>
        /// Returns a JSON string representation of the mail user.
        /// </summary>
        /// <returns>A formatted JSON string.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// Validates the format of an email address.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is not a valid email format.</exception>
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