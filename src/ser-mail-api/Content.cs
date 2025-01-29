using System.Text.Json;
using System.Text.Json.Serialization;

namespace ser_mail_api
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

    // Content Class
    public class Content
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }

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
                WriteIndented = true // Enables pretty-printing
            });
        }
    }
}
