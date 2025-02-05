using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public enum Disposition
    {
        Inline,
        Attachment
    }

    internal class DispositionJsonConverter : JsonConverter<Disposition>
    {
        public override Disposition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            return stringValue switch
            {
                "inline" => Disposition.Inline,
                "attachment" => Disposition.Attachment,
                _ => throw new JsonException($"Invalid value for Disposition: {stringValue}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Disposition value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                Disposition.Inline => "inline",
                Disposition.Attachment => "attachment",
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Invalid Disposition value: {value}")
            };

            writer.WriteStringValue(stringValue);
        }
    }

    public class Attachment
    {
        public static readonly IMimeMapper MimeTypeMapper = new DefaultMimeMapper();

        [JsonPropertyName("content")]
        public string Content { get; }

        [JsonConverter(typeof(DispositionJsonConverter))]
        [JsonPropertyName("disposition")]
        public Disposition Disposition { get; }

        [JsonPropertyName("filename")]
        public string Filename { get; }

        [JsonPropertyName("id")]
        public string? Id { get; }

        [JsonPropertyName("type")]
        public string MimeType { get; }

        /// <summary>
        /// Represents an attachment with content, filename, MIME type, and disposition.
        /// </summary>
        /// <param name="content">Base64 encoded content of the attachment.</param>
        /// <param name="filename">Filename of the attachment.</param>
        /// <param name="mimeType">
        /// MIME type of the content. If <c>null</c>, it will try to deduce it from the filename.
        /// </param>
        /// <param name="disposition">
        /// The disposition of the attachment (inline or attachment). Default is <see cref="Disposition.Attachment"/>.
        /// </param>
        /// <param name="cId">
        /// The Content-ID of the attachment. If not specified, for an inline attachment, the value will be a random UUID.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when the base64 content is invalid, filename is empty or too long, or the MIME type is empty.
        /// </exception>
        private Attachment(string content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? cId = null)
        {
            if (!TryDecodeBase64(content, out _))
                throw new ArgumentException("Invalid Base64 content", nameof(content));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));

            if (mimeType != null && string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("Mime type must be a non-empty string.", nameof(mimeType));

            if (filename.Length > 1000)
            {
                throw new ArgumentException("Filename must be at most 1000 characters long");
            }

            // User provided mime_type or try to deduce it from filename
            if (mimeType == null)
            {
                mimeType = MimeTypeMapper.GetMimeType(filename);
            }

            if (string.IsNullOrWhiteSpace(mimeType))
            {
                throw new ArgumentException("Mime type must be a non-empty string");
            }

            if (string.IsNullOrWhiteSpace(cId))
            {
                Id = Guid.NewGuid().ToString(); // Generate a UUID
            }
            else
            {
                Id = cId; // Use provided string
            }

            // CID only applies to inline attachments
            if (disposition == Disposition.Attachment)
                Id = null;

            Content = content;
            Disposition = disposition;
            Filename = filename;
            MimeType = mimeType;
        }

        // Factory Methods
        [Obsolete]
        public static Attachment FromBase64String(string base64Content, string filename, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
        {
            if (validateMimeType && !MimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"MIME type '{mimeType}' appears to be invalid. Consider disabling MIME type validation.", nameof(mimeType));

            return new Attachment(base64Content, filename, mimeType, disposition);
        }


        public static Attachment FromBase64(string base64Content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? cId = null)
            => new(base64Content, filename, mimeType, disposition, cId);

        [Obsolete]
        public static Attachment FromFile(string filePath, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
        {
            if (validateMimeType && !MimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"MIME type '{mimeType}' appears to be invalid. Consider disabling MIME type validation.", nameof(mimeType));

            return FromFile(filePath, disposition, mimeType: mimeType);
        }
        public static Attachment FromFile(string filePath, Disposition disposition = Disposition.Attachment, string? cId = null, string? filename = null, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            if (string.IsNullOrWhiteSpace(filename))
                filename = Path.GetFileName(filePath);

            return new Attachment(EncodeFileContent(filePath), filename, mimeType, disposition, cId);
        }

        public static Attachment FromBytes(byte[] data, string filename, string mimeType, Disposition disposition = Disposition.Attachment)
        {
            string base64Content = "";

            if (data.Length > 0)
                base64Content = Convert.ToBase64String(data);

            return new Attachment(base64Content, filename, mimeType, disposition);
        }

        private static bool TryDecodeBase64(string base64String, out byte[]? decodedBytes)
        {
            try
            {
                if (base64String.Length > 0 && string.IsNullOrWhiteSpace(base64String))
                {
                    decodedBytes = Array.Empty<byte>();
                    return false;
                }
                decodedBytes = Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                decodedBytes = null;
                return false;
            }
        }

        private static string EncodeFileContent(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            if (fileBytes.Length == 0)
                throw new ArgumentException($"File '{filePath}' is empty and cannot be converted to an attachment.");

            return Convert.ToBase64String(fileBytes);
        }

        public override string ToString()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
