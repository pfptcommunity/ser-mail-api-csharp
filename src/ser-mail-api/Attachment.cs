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
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonConverter(typeof(DispositionJsonConverter))]
        [JsonPropertyName("disposition")]
        public Disposition Disposition { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string MimeType { get; set; }

        public Attachment(string content, string filename, string mimeType, Disposition disposition = Disposition.Attachment, bool validateMimeType = true)
        {
            if (!TryDecodeBase64(content, out _)) throw new ArgumentException("Invalid Base64 content", nameof(content));
            if (filename.Length > 1000) throw new ArgumentException("Filename must be at most 1000 characters long", nameof(filename));
            if (string.IsNullOrWhiteSpace(mimeType)) throw new ArgumentException("Mime type must be a non-empty string", nameof(mimeType));
            if (validateMimeType && !MimeTypesMap.IsMimeTypeMapped(mimeType)) throw new ArgumentException($"Mime type '{mimeType}' appears to be invalid, consider disabling mime type validation.", nameof(mimeType));
            if (validateMimeType && !StringComparer.OrdinalIgnoreCase.Equals(MimeTypesMap.GetMimeType(filename), mimeType)) throw new ArgumentException($"Specified mime type '{mimeType}' conflicts with inferred type '{MimeTypesMap.GetMimeType(filename)}' based on file name '{filename}', consider disabling mime type validation.");

            Content = content;
            Disposition = disposition;
            Filename = filename;
            MimeType = mimeType;
            Id = Guid.NewGuid().ToString();
        }

        private bool TryDecodeBase64(string base64String, out byte[]? decodedBytes) // Make decodedBytes nullable
        {
            try
            {
                decodedBytes = Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                decodedBytes = Array.Empty<byte>(); // Assign an empty byte array instead of null
                return false;
            }
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