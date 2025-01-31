using System.Text.Json;
using System.Text.Json.Serialization;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class SendResult
    {
        public string MessageId { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public string RequestId { get; init; } = string.Empty;

        public HttpResponseMessage HttpResponse { get; init; }
        public string RawJson { get; init; }

        private SendResult(HttpResponseMessage httpResponse, string? rawJson)
        {
            HttpResponse = httpResponse;
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

        internal static async Task<SendResult> CreateAsync(HttpResponseMessage httpResponse)
        {
            string responseJson = await httpResponse.Content.ReadAsStringAsync();

            return new SendResult(httpResponse, responseJson);
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

}