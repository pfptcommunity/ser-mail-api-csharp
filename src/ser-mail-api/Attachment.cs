using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Defines the possible dispositions of an attachment in a mail message.
    /// </summary>
    public enum Disposition
    {
        /// <summary>
        /// Indicates the attachment is embedded inline within the message body.
        /// </summary>
        Inline,

        /// <summary>
        /// Indicates the attachment is a separate file attachment.
        /// </summary>
        Attachment
    }

    /// <summary>
    /// Custom JSON converter for serializing and deserializing <see cref="Disposition"/> values.
    /// </summary>
    internal class DispositionJsonConverter : JsonConverter<Disposition>
    {
        /// <summary>
        /// Reads and converts a JSON string to a <see cref="Disposition"/> value.
        /// </summary>
        /// <param name="reader">The JSON reader to read from.</param>
        /// <param name="typeToConvert">The type being converted (Disposition).</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>The parsed <see cref="Disposition"/> value.</returns>
        /// <exception cref="JsonException">Thrown when the JSON string is not a valid disposition value.</exception>
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

        /// <summary>
        /// Writes a <see cref="Disposition"/> value as a JSON string.
        /// </summary>
        /// <param name="writer">The JSON writer to write to.</param>
        /// <param name="value">The <see cref="Disposition"/> value to write.</param>
        /// <param name="options">The serialization options.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the disposition value is invalid.</exception>
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

    /// <summary>
    /// Represents an attachment for a mail message, including its content, filename, MIME type, and disposition.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Provides a default MIME type mapper for deducing MIME types from filenames.
        /// </summary>
        public static readonly IMimeMapper MimeTypeMapper = new DefaultMimeMapper();

        /// <summary>
        /// Gets the Base64-encoded content of the attachment.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; }

        /// <summary>
        /// Gets the disposition of the attachment (inline or attachment).
        /// </summary>
        [JsonConverter(typeof(DispositionJsonConverter))]
        [JsonPropertyName("disposition")]
        public Disposition Disposition { get; }

        /// <summary>
        /// Gets the filename of the attachment.
        /// </summary>
        [JsonPropertyName("filename")]
        public string Filename { get; }

        /// <summary>
        /// Gets the Content-ID of the attachment. Obsolete; use <see cref="ContentId"/> instead.
        /// </summary>
        [Obsolete]
        [JsonIgnore]
        public string? Id => ContentId;

        /// <summary>
        /// Gets the Content-ID of the attachment, used for inline references. Null for attachments with <see cref="Disposition.Attachment"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public string? ContentId { get; }

        /// <summary>
        /// Gets the MIME type of the attachment content.
        /// </summary>
        [JsonPropertyName("type")]
        public string MimeType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Attachment"/> with the specified properties.
        /// </summary>
        /// <param name="content">Base64-encoded content of the attachment. Empty strings are valid for empty content.</param>
        /// <param name="filename">Filename of the attachment.</param>
        /// <param name="mimeType">MIME type of the content. If null, deduced from the filename.</param>
        /// <param name="disposition">Disposition of the attachment. Defaults to <see cref="Disposition.Attachment"/>.</param>
        /// <param name="contentId">Content-ID of the attachment. If null or whitespace, a random UUID is generated for inline attachments.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> or <paramref name="filename"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is invalid Base64, <paramref name="filename"/> is empty or too long, or <paramref name="mimeType"/> is resolved to an invalid value.</exception>
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

        /// <summary>
        /// Creates an <see cref="Attachment"/> from Base64-encoded content.
        /// </summary>
        /// <param name="base64Content">The Base64-encoded content of the attachment.</param>
        /// <param name="filename">The filename of the attachment.</param>
        /// <param name="mimeType">The MIME type, or null to deduce from the filename.</param>
        /// <param name="disposition">The disposition of the attachment.</param>
        /// <param name="contentId">The Content-ID, or null for default behavior.</param>
        /// <returns>A new <see cref="Attachment"/> instance.</returns>
        public static Attachment FromBase64(string base64Content, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
            => new(base64Content, filename, mimeType, disposition, contentId);

        /// <summary>
        /// Creates an <see cref="Attachment"/> from a file on disk.
        /// </summary>
        /// <param name="filePath">The path to the file to attach.</param>
        /// <param name="disposition">The disposition of the attachment.</param>
        /// <param name="contentId">The Content-ID, or null for default behavior.</param>
        /// <param name="filename">The filename, or null to use the file's name.</param>
        /// <param name="mimeType">The MIME type, or null to deduce from the filename.</param>
        /// <returns>A new <see cref="Attachment"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file at <paramref name="filePath"/> does not exist.</exception>
        public static Attachment FromFile(string filePath, Disposition disposition = Disposition.Attachment, string? contentId = null, string? filename = null, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null, empty, or contain only whitespace.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: '{filePath}'.", filePath);

            filename ??= Path.GetFileName(filePath);
            return new Attachment(EncodeFileContent(filePath), filename, mimeType, disposition, contentId);
        }

        /// <summary>
        /// Creates an <see cref="Attachment"/> from a byte array.
        /// </summary>
        /// <param name="data">The byte array content of the attachment.</param>
        /// <param name="filename">The filename of the attachment.</param>
        /// <param name="mimeType">The MIME type, or null to deduce from the filename.</param>
        /// <param name="disposition">The disposition of the attachment.</param>
        /// <param name="contentId">The Content-ID, or null for default behavior.</param>
        /// <returns>A new <see cref="Attachment"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public static Attachment FromBytes(byte[] data, string filename, string? mimeType = null, Disposition disposition = Disposition.Attachment, string? contentId = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Byte array must not be null.");
            return new Attachment(Convert.ToBase64String(data), filename, mimeType, disposition, contentId);
        }

        /// <summary>
        /// Validates whether a string is a valid Base64-encoded value.
        /// </summary>
        /// <param name="base64String">The string to validate.</param>
        /// <returns>True if the string is valid Base64 (including empty strings); otherwise, false.</returns>
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

        /// <summary>
        /// Encodes the content of a file to a Base64 string.
        /// </summary>
        /// <param name="filePath">The path to the file to encode.</param>
        /// <returns>The Base64-encoded string representation of the file content.</returns>
        private static string EncodeFileContent(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes); // Empty files return ""
        }

        /// <summary>
        /// Returns a JSON string representation of the attachment.
        /// </summary>
        /// <returns>A formatted JSON string.</returns>
        public override string ToString()
            => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        /// <summary>
        /// Creates a new builder instance for constructing an <see cref="Attachment"/> fluently.
        /// </summary>
        /// <returns>A new <see cref="AttachmentBuilder"/> instance.</returns>
        public static AttachmentBuilder Builder() => new AttachmentBuilder();

        /// <summary>
        /// Defines the initial step interface for building an <see cref="Attachment"/> with content.
        /// </summary>
        public interface IInitialStep
        {
            /// <summary>
            /// Sets the attachment content from a Base64-encoded string and proceeds to optional configuration.
            /// </summary>
            /// <param name="base64Content">The Base64-encoded content.</param>
            /// <param name="filename">The filename of the attachment.</param>
            /// <returns>An <see cref="IOptionalStep"/> for further configuration.</returns>
            IOptionalStep FromBase64(string base64Content, string filename);

            /// <summary>
            /// Sets the attachment content from a file and proceeds to optional configuration.
            /// </summary>
            /// <param name="filePath">The path to the file.</param>
            /// <returns>An <see cref="IOptionalStep"/> for further configuration.</returns>
            IOptionalStep FromFile(string filePath);

            /// <summary>
            /// Sets the attachment content from a byte array and proceeds to optional configuration.
            /// </summary>
            /// <param name="data">The byte array content.</param>
            /// <param name="filename">The filename of the attachment.</param>
            /// <returns>An <see cref="IOptionalStep"/> for further configuration.</returns>
            IOptionalStep FromBytes(byte[] data, string filename);
        }

        /// <summary>
        /// Defines the optional step interface for configuring an <see cref="Attachment"/> before building.
        /// </summary>
        public interface IOptionalStep
        {
            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Attachment"/>.
            /// </summary>
            /// <returns>The current <see cref="IOptionalStep"/> for chaining.</returns>
            IOptionalStep DispositionAttached();

            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Inline"/> without a specific Content-ID.
            /// </summary>
            /// <returns>The current <see cref="IOptionalStep"/> for chaining.</returns>
            IOptionalStep DispositionInline();

            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Inline"/> with a specified Content-ID.
            /// </summary>
            /// <param name="contentId">The Content-ID for the inline attachment.</param>
            /// <returns>The current <see cref="IOptionalStep"/> for chaining.</returns>
            IOptionalStep DispositionInline(string contentId);

            /// <summary>
            /// Sets or overrides the filename of the attachment.
            /// </summary>
            /// <param name="filename">The filename to use.</param>
            /// <returns>The current <see cref="IOptionalStep"/> for chaining.</returns>
            IOptionalStep Filename(string filename);

            /// <summary>
            /// Sets the MIME type of the attachment content.
            /// </summary>
            /// <param name="mimeType">The MIME type to use.</param>
            /// <returns>The current <see cref="IOptionalStep"/> for chaining.</returns>
            IOptionalStep MimeType(string mimeType);

            /// <summary>
            /// Builds and returns the final <see cref="Attachment"/> instance.
            /// </summary>
            /// <returns>The constructed <see cref="Attachment"/>.</returns>
            Attachment Build();
        }

        /// <summary>
        /// Provides a fluent builder for creating an <see cref="Attachment"/> instance step-by-step.
        /// </summary>
        public class AttachmentBuilder : IInitialStep, IOptionalStep
        {
            private string? _content;
            private string? _filename;
            private string? _mimeType;
            private Disposition _disposition;
            private string? _contentId;

            /// <summary>
            /// Initializes a new instance of <see cref="AttachmentBuilder"/> with default settings.
            /// </summary>
            internal AttachmentBuilder()
            {
                DispositionAttached();
            }

            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Attachment"/> and clears the Content-ID.
            /// </summary>
            /// <returns>The current builder instance for chaining.</returns>
            public IOptionalStep DispositionAttached()
            {
                _disposition = Disposition.Attachment;
                _contentId = null;
                return this;
            }

            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Inline"/> and clears the Content-ID.
            /// </summary>
            /// <returns>The current builder instance for chaining.</returns>
            public IOptionalStep DispositionInline()
            {
                _disposition = Disposition.Inline;
                _contentId = null;
                return this;
            }

            /// <summary>
            /// Sets the disposition to <see cref="Disposition.Inline"/> with a specified Content-ID.
            /// </summary>
            /// <param name="contentId">The Content-ID for the inline attachment.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentId"/> is null.</exception>
            public IOptionalStep DispositionInline(string contentId)
            {
                _contentId = contentId ?? throw new ArgumentNullException(nameof(contentId), "ContentId must not be null.");
                _disposition = Disposition.Inline;
                return this;
            }

            /// <summary>
            /// Sets or overrides the filename of the attachment.
            /// </summary>
            /// <param name="filename">The filename to use.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="filename"/> is null.</exception>
            public IOptionalStep Filename(string filename)
            {
                _filename = filename ?? throw new ArgumentNullException(nameof(filename), "Filename must not be null.");
                return this;
            }

            /// <summary>
            /// Sets the MIME type of the attachment content.
            /// </summary>
            /// <param name="mimeType">The MIME type to use.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="mimeType"/> is null.</exception>
            public IOptionalStep MimeType(string mimeType)
            {
                _mimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType), "MimeType must not be null.");
                return this;
            }

            /// <summary>
            /// Sets the attachment content from a file and uses the file's name as the default filename.
            /// </summary>
            /// <param name="filePath">The path to the file to attach.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
            /// <exception cref="FileNotFoundException">Thrown when the file at <paramref name="filePath"/> does not exist.</exception>
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

            /// <summary>
            /// Sets the attachment content from a byte array.
            /// </summary>
            /// <param name="data">The byte array content.</param>
            /// <param name="filename">The filename of the attachment.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="filename"/> is null.</exception>
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

            /// <summary>
            /// Sets the attachment content from a Base64-encoded string.
            /// </summary>
            /// <param name="base64Content">The Base64-encoded content.</param>
            /// <param name="filename">The filename of the attachment.</param>
            /// <returns>The current builder instance for chaining.</returns>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="base64Content"/> or <paramref name="filename"/> is null.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="base64Content"/> is not valid Base64.</exception>
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

            /// <summary>
            /// Builds and returns the final <see cref="Attachment"/> instance.
            /// </summary>
            /// <returns>The constructed <see cref="Attachment"/>.</returns>
            /// <exception cref="InvalidOperationException">Thrown if content or filename is not set via an initial step.</exception>
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