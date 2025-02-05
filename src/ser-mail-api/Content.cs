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
                "text" => ContentType.Text,
                "html" => ContentType.Html,
                _ => throw new JsonException($"Invalid ContentType value: '{stringValue}'.")
            };
        }

        public override void Write(Utf8JsonWriter writer, ContentType value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                ContentType.Text => "text/plain",
                ContentType.Html => "text/html",
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Invalid ContentType value: '{value}'.")
            };

            writer.WriteStringValue(stringValue);
        }
    }

    public sealed class Content
    {
        private string _body = null!;

        [JsonPropertyName("body")]
        public string Body
        {
            get => _body;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Body must not be null.");

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Body must not be empty or contain only whitespace.", nameof(value));

                _body = value;
            }
        }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(ContentTypeJsonConverter))]
        public ContentType ContentType { get; }

        public Content(string body, ContentType contentType)
        {
            ContentType = contentType;
            Body = body;
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
