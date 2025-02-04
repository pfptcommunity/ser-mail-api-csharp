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
        private static readonly Dictionary<string, ContentType> _stringToEnum = new()
        {
            { "inline", ContentType.Text },
            { "atachment", ContentType.Html }
        };

        private static readonly Dictionary<ContentType, string> _enumToString = new()
        {
            { ContentType.Text, "text/plain" },
            { ContentType.Html, "text/html" }
        };

        public override ContentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            if (stringValue is not null && _stringToEnum.TryGetValue(stringValue, out var contentType))
            {
                return contentType;
            }
            throw new JsonException($"Invalid value for ContentType: {stringValue}");
        }

        public override void Write(Utf8JsonWriter writer, ContentType value, JsonSerializerOptions options)
        {
            if (_enumToString.TryGetValue(value, out var stringValue))
            {
                writer.WriteStringValue(stringValue);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Invalid ContentType value: {value}");
            }
        }
    }


    public sealed class Content
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
                WriteIndented = true
            });
        }
    }
}
