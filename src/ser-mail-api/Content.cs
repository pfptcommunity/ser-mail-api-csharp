using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Defines the possible content types for a mail message body.
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Represents plain text content.
        /// </summary>
        Text,

        /// <summary>
        /// Represents HTML-formatted content.
        /// </summary>
        Html
    }

    /// <summary>
    /// Custom JSON converter for serializing and deserializing <see cref="ContentType"/> values.
    /// </summary>
    internal class ContentTypeJsonConverter : JsonConverter<ContentType>
    {
        /// <summary>
        /// Reads and converts a JSON string to a <see cref="ContentType"/> value.
        /// </summary>
        /// <param name="reader">The JSON reader to read from.</param>
        /// <param name="typeToConvert">The type being converted (ContentType).</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>The parsed <see cref="ContentType"/> value.</returns>
        /// <exception cref="JsonException">Thrown when the JSON string is not a valid content type.</exception>
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

        /// <summary>
        /// Writes a <see cref="ContentType"/> value as a JSON string using MIME type format.
        /// </summary>
        /// <param name="writer">The JSON writer to write to.</param>
        /// <param name="value">The <see cref="ContentType"/> value to write.</param>
        /// <param name="options">The serialization options.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the content type value is invalid.</exception>
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

    /// <summary>
    /// Represents the content of a mail message, including its body and type.
    /// </summary>
    public sealed class Content
    {
        /// <summary>
        /// Gets the body text of the content.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the value is set to null.</exception>
        /// <exception cref="ArgumentException">Thrown when the value is empty or contains only whitespace.</exception>
        [JsonPropertyName("body")]
        public string Body { get; }

        /// <summary>
        /// Gets the type of the content (text or HTML).
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(ContentTypeJsonConverter))]
        public ContentType ContentType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Content"/> with the specified body and content type.
        /// </summary>
        /// <param name="body">The body text of the content.</param>
        /// <param name="contentType">The type of the content (text or HTML).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="body"/> is empty or contains only whitespace.</exception>
        public Content(string body, ContentType contentType)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body), "Body must not be null.");

            // TODO: This should be allowed
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body must not be empty or contain only whitespace.", nameof(body));

            Body = body;
            ContentType = contentType;
        }

        /// <summary>
        /// Returns a JSON string representation of the content.
        /// </summary>
        /// <returns>A formatted JSON string.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}