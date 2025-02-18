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
                _ => throw new JsonException($"Invalid Disposition value: '{stringValue}'.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Disposition value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                Disposition.Inline => "inline",
                Disposition.Attachment => "attachment",
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Invalid Disposition value: '{value}'.")
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

        [Obsolete]
        [JsonIgnore]
        public string? Id => ContentId;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public string? ContentId { get; }


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
        /// <param name="contentId">
        /// The Content-ID of the attachment. If not specified, for an inline attachment, the value will be a random UUID.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when the base64 content is invalid, filename is empty or too long, or the MIME type is empty.
        /// </exception>
        private Attachment(string content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
        {
            // Ensure required parameters are not null
            if (content == null)
                throw new ArgumentNullException(nameof(content), "Content cannot be null.");

            if (filename == null)
                throw new ArgumentNullException(nameof(filename), "Filename cannot be null.");

            // Validate Base64 content
            if (!TryDecodeBase64(content, out _))
                throw new ArgumentException("Content must be a valid Base64-encoded string.", nameof(content));

            // Ensure filename is not just whitespace
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be empty or contain only whitespace.", nameof(filename));

            // Ensure filename length constraint
            if (filename.Length > 1000)
                throw new ArgumentException("Filename must not exceed 1000 characters.", nameof(filename));

            // Validate MIME type
            if (mimeType != null && string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type cannot be empty or contain only whitespace.", nameof(mimeType));

            // Deduce MIME type if not provided
            mimeType ??= MimeTypeMapper.GetMimeType(filename);

            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type must be a valid, non-empty string.", nameof(mimeType));

            // Assign ID
            this.ContentId = string.IsNullOrWhiteSpace(contentId) ? Guid.NewGuid().ToString() : contentId;

            // CID only applies to inline attachments
            if (disposition == Disposition.Attachment)
                this.ContentId = null;

            // Assign values to properties
            Content = content;
            Disposition = disposition;
            Filename = filename;
            MimeType = mimeType;
        }

        public static Attachment FromBase64(string base64Content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
            => new(base64Content, filename, mimeType, disposition, contentId);


        public static Attachment FromFile(string filePath, Disposition disposition = Disposition.Attachment, string? contentId = null, string? filename = null, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null, empty, or contain only whitespace.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: '{filePath}'.", filePath);

            filename ??= Path.GetFileName(filePath);

            return new Attachment(EncodeFileContent(filePath), filename, mimeType, disposition, contentId);
        }

        public static Attachment FromBytes(byte[] data, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Byte array must not be null.");

            return new Attachment(Convert.ToBase64String(data), filename, mimeType, disposition, contentId);
        }


        private static bool TryDecodeBase64(string base64String, out byte[]? decodedBytes)
        {
            try
            {
                if (base64String == null || (base64String.Length > 0 && string.IsNullOrWhiteSpace(base64String)))
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
                throw new ArgumentException($"File '{filePath}' is empty and cannot be converted to an attachment.", nameof(filePath));

            return Convert.ToBase64String(fileBytes);
        }

        public override string ToString()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
