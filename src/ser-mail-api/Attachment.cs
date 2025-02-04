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
            if (Enum.TryParse(typeof(Disposition), reader.GetString(), true, out var result))
            {
                return (Disposition)result!;
            }
            throw new JsonException($"Invalid value for Disposition: {reader.GetString()}");
        }

        public override void Write(Utf8JsonWriter writer, Disposition value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLower());
        }
    }

    public sealed class Attachment
    {
        [JsonPropertyName("content")]
        public string Content { get; }

        [JsonConverter(typeof(DispositionJsonConverter))]
        [JsonPropertyName("disposition")]
        public Disposition Disposition { get; }

        [JsonPropertyName("filename")]
        public string Filename { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public string? Id { get; }

        [JsonPropertyName("type")]
        public string MimeType { get; }


        public Attachment(string content, string filename, string mimeType, Disposition disposition, string? contentId)
        {
            if (content is null)
                throw new ArgumentException("Content cannot be null.", nameof(content));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be empty.", nameof(filename));

            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME type cannot be empty.", nameof(mimeType));

            if (disposition == Disposition.Inline && string.IsNullOrWhiteSpace(contentId))
                throw new ArgumentException("Inline attachments must have a Content ID.", nameof(contentId));

            Content = content;
            Disposition = disposition;
            Filename = filename;
            MimeType = mimeType;
            Id = disposition == Disposition.Inline ? contentId : null;
        }

        public Attachment(string content, string filename, string mimeType, Disposition disposition)
            : this(content, filename, mimeType, disposition, disposition == Disposition.Inline ? Guid.NewGuid().ToString() : null)
        {
        }

        public static ISourceStep Builder() => new AttachmentBuilder();
        public static ISourceStep Builder(IMimeTypeMapper mimeTypeMapper) => new AttachmentBuilder(mimeTypeMapper);
        public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

}
