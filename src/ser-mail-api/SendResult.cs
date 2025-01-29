using System.Text.Json.Serialization;
using System.Text.Json;

public class SendResult
{
    public string MessageId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;

    public HttpResponseMessage HttpResponse { get; init; }
    public string RawJson { get; init; }

    public SendResult(HttpResponseMessage httpResponse, string? rawJson)
    {
        HttpResponse = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));
        RawJson = rawJson ?? string.Empty;

        try
        {
            if (!string.IsNullOrWhiteSpace(RawJson))
            {
                var parsedJson = JsonSerializer.Deserialize<SendResultDto>(RawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsedJson != null)
                {
                    MessageId = parsedJson.MessageId ?? string.Empty;
                    Reason = parsedJson.Reason ?? string.Empty;
                    RequestId = parsedJson.RequestId ?? string.Empty;
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse JSON response: {ex.Message}");
        }
    }

    private class SendResultDto
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }
}
