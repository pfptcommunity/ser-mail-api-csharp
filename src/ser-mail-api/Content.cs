using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public enum ContentType
    {
        Text,
        Html
    }

    internal class ContentTypeJsonConverter : JsonConverter<ContentType>
    {
        public override ContentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            return stringValue switch
            {
                "inline" => ContentType.Text,
                "attachment" => ContentType.Html,
                _ => throw new JsonException($"Invalid value for ContentType: {stringValue}")
            };
        }

        public override void Write(Utf8JsonWriter writer, ContentType value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                ContentType.Text => "text/plain",
                ContentType.Html => "text/html",
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Invalid ContentType value: {value}")
            };

            writer.WriteStringValue(stringValue);
        }
    }

    public sealed class Content
    {
        private string _body = string.Empty;

        [JsonPropertyName("body")]
        public string Body
        {
            get => _body;
            set => _body = value ?? throw new ArgumentNullException(nameof(value), "Body cannot be null.");
        }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(ContentTypeJsonConverter))]
        public ContentType ContentType { get; set; }

        public Content(string body, ContentType contentType)
        {
            Body = body;
            ContentType = contentType;
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
