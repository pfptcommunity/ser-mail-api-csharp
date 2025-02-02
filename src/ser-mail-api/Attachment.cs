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

        private static IMimeTypeMapper _defaultMimeTypeMapper = new DefaultMimeTypeMapper();

        [JsonPropertyName("content")]
        public string Content { get; }

        [JsonConverter(typeof(DispositionJsonConverter))]
        [JsonPropertyName("disposition")]
        public Disposition Disposition { get; }

        [JsonPropertyName("filename")]
        public string Filename { get; }

        [JsonPropertyName("id")]
        public string Id { get; }

        [JsonPropertyName("type")]
        public string MimeType { get; }

        private Attachment(string content, string filename, string mimeType, Disposition disposition)
        {
            if (!TryDecodeBase64(content, out _))
                throw new ArgumentException("Invalid Base64 content", nameof(content));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be empty or whitespace.", nameof(filename));

            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type must be a non-empty string.", nameof(mimeType));

            if (!_defaultMimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"The specified MIME type '{mimeType}' is not recognized or supported. Ensure it is a valid and standard MIME type.", nameof(mimeType));

            Content = content;
            Disposition = disposition;
            Filename = filename;
            MimeType = mimeType;
            Id = Guid.NewGuid().ToString();
        }

        public static Attachment FromBase64String(string base64Content, string filename, string mimeType, Disposition disposition = Disposition.Attachment)
            => FromBase64String(base64Content, filename, mimeType, _defaultMimeTypeMapper, disposition);

        public static Attachment FromBase64String(string base64Content, string filename, string mimeType, IMimeTypeMapper mimeTypeMapper, Disposition disposition = Disposition.Attachment)
        {
            if (!mimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"Invalid MIME type '{mimeType}'. Ensure it is a valid and standard MIME type.", nameof(mimeType));

            return new Attachment(base64Content, filename, mimeType, disposition);
        }

        public static Attachment FromFile(string filePath, Disposition disposition = Disposition.Attachment)
            => FromFile(filePath, _defaultMimeTypeMapper, disposition);

        public static Attachment FromFile(string filePath, IMimeTypeMapper mimeTypeMapper, Disposition disposition = Disposition.Attachment)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return new Attachment(EncodeFileContent(filePath), Path.GetFileName(filePath), mimeTypeMapper.GetMimeType(filePath), disposition);
        }

        public static Attachment FromFile(string filePath, string mimeType, Disposition disposition = Disposition.Attachment)
            => FromFile(filePath, mimeType, _defaultMimeTypeMapper, disposition);

        public static Attachment FromFile(string filePath, string mimeType, IMimeTypeMapper mimeTypeMapper, Disposition disposition = Disposition.Attachment)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            if (!mimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"Invalid MIME type '{mimeType}'. Ensure it is a valid and standard MIME type.", nameof(mimeType));

            return new Attachment(EncodeFileContent(filePath), Path.GetFileName(filePath), mimeType, disposition);
        }

        public static Attachment FromBytes(byte[] data, string filename, string mimeType, Disposition disposition = Disposition.Attachment)
            => FromBytes(data, filename, mimeType, _defaultMimeTypeMapper, disposition);

        public static Attachment FromBytes(byte[] data, string filename, string mimeType, IMimeTypeMapper mimeTypeMapper, Disposition disposition = Disposition.Attachment)
        {
            if (!mimeTypeMapper.IsValidMimeType(mimeType))
                throw new ArgumentException($"Invalid MIME type '{mimeType}'. Ensure it is a valid and standard MIME type.", nameof(mimeType));

            string base64Content = data.Length > 0 ? Convert.ToBase64String(data) : string.Empty;
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
        public static void SetDefaultMimeTypeMapper(IMimeTypeMapper mimeTypeMapper)
        {
            _defaultMimeTypeMapper = mimeTypeMapper ?? throw new ArgumentNullException(nameof(mimeTypeMapper));
        }

        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

}
