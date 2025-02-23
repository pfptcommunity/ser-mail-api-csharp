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
        /// <param name="content">Base64-encoded content of the attachment. Empty strings are valid for empty content.</param>
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
        /// Thrown when the Base64 content is invalid, filename is empty or too long, or the MIME type is empty.
        /// </exception>
        private Attachment(string content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content), "Content cannot be null.");
            if (filename == null)
                throw new ArgumentNullException(nameof(filename), "Filename cannot be null.");
            if (!IsValidBase64(content))
                throw new ArgumentException("Content must be a valid Base64-encoded string.", nameof(content));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be empty or contain only whitespace.", nameof(filename));
            if (filename.Length > 1000)
                throw new ArgumentException("Filename must not exceed 1000 characters.", nameof(filename));
            if (mimeType != null && string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type cannot be empty or contain only whitespace.", nameof(mimeType));

            mimeType ??= MimeTypeMapper.GetMimeType(filename);
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type must be a valid, non-empty string.", nameof(mimeType));

            this.ContentId = string.IsNullOrWhiteSpace(contentId) ? Guid.NewGuid().ToString() : contentId;
            if (disposition == Disposition.Attachment)
                this.ContentId = null;

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

        private static bool IsValidBase64(string base64String)
        {
            if (base64String == null)
                return false;
            if (base64String.Length == 0)
                return true; // Empty string is valid
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string EncodeFileContent(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes); // Empty files return ""
        }

        public override string ToString()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        public static AttachmentBuilder Builder() => new AttachmentBuilder();

        public interface IInitialStep
        {
            IOptionalStep FromBase64(string base64Content, string filename);
            IOptionalStep FromFile(string filePath);
            IOptionalStep FromBytes(byte[] data, string filename);
        }

        public interface IOptionalStep
        {
            IOptionalStep DispositionAttached();
            IOptionalStep DispositionInline();
            IOptionalStep DispositionInline(string contentId);
            IOptionalStep Filename(string filename);
            IOptionalStep MimeType(string mimeType);
            Attachment Build();
        }

        public class AttachmentBuilder : IInitialStep, IOptionalStep
        {
            private string? _content;
            private string? _filename;
            private string? _mimeType;
            private Disposition _disposition;
            private string? _contentId;

            internal AttachmentBuilder()
            {
                DispositionAttached();
            }

            public IOptionalStep DispositionAttached()
            {
                _disposition = Disposition.Attachment;
                _contentId = null;
                return this;
            }

            public IOptionalStep DispositionInline()
            {
                _disposition = Disposition.Inline;
                _contentId = null;
                return this;
            }

            public IOptionalStep DispositionInline(string contentId)
            {
                _contentId = contentId ?? throw new ArgumentNullException(nameof(contentId), "ContentId must not be null.");
                _disposition = Disposition.Inline;
                return this;
            }

            public IOptionalStep Filename(string filename)
            {
                _filename = filename ?? throw new ArgumentNullException(nameof(filename), "Filename must not be null.");
                return this;
            }

            public IOptionalStep MimeType(string mimeType)
            {
                _mimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType), "MimeType must not be null.");
                return this;
            }

            public IOptionalStep FromFile(string filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("File path cannot be null, empty, or contain only whitespace.", nameof(filePath));
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: '{filePath}'.", filePath);

                _filename = Path.GetFileName(filePath);
                _content = EncodeFileContent(filePath);
                return this;
            }

            public IOptionalStep FromBytes(byte[] data, string filename)
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data), "Byte array must not be null.");
                if (filename == null)
                    throw new ArgumentNullException(nameof(filename), "Filename must not be null.");

                _content = Convert.ToBase64String(data);
                _filename = filename;
                return this;
            }

            public IOptionalStep FromBase64(string base64Content, string filename)
            {
                if (base64Content == null)
                    throw new ArgumentNullException(nameof(base64Content), "Base64 content must not be null.");
                if (filename == null)
                    throw new ArgumentNullException(nameof(filename), "Filename must not be null.");
                if (!IsValidBase64(base64Content))
                    throw new ArgumentException("Content must be a valid Base64-encoded string.", nameof(base64Content));

                _content = base64Content;
                _filename = filename;
                return this;
            }

            public Attachment Build()
            {
                // This should never happen because the builder enforces step 1
                if (_content == null)
                    throw new InvalidOperationException("Content must be set before building the attachment. Call FromBase64, FromFile, or FromBytes first.");
                if (_filename == null)
                    throw new InvalidOperationException("Filename must be set before building the attachment. Call FromBase64, FromFile, or FromBytes first.");

                return new Attachment(_content, _filename, _mimeType, _disposition, _contentId);
            }
        }
    }
}