using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class SendResult
    {
        public string MessageId { get; } = string.Empty;
        public string Reason { get; } = string.Empty;
        public string RequestId { get; } = string.Empty;

        public HttpResponseMessage HttpResponse { get; }

        public string RawJson { get; }

        private SendResult(HttpResponseMessage httpResponse, string rawJson)
        {
            HttpResponse = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse), "HTTP response cannot be null.");
            RawJson = rawJson ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(RawJson))
            {
                try
                {
                    var parsedJson = JsonSerializer.Deserialize<Dictionary<string, string>>(RawJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (parsedJson != null)
                    {
                        MessageId = parsedJson.TryGetValue("message_id", out var msgId) ? msgId : string.Empty;
                        Reason = parsedJson.TryGetValue("reason", out var reason) ? reason : string.Empty;
                        RequestId = parsedJson.TryGetValue("request_id", out var reqId) ? reqId : string.Empty;
                    }
                }
                catch
                {
                    // Handle JSON parsing failure gracefully
                }
            }
        }

        internal static async Task<SendResult> CreateAsync(HttpResponseMessage httpResponse)
        {
            string responseJson = await httpResponse.Content.ReadAsStringAsync();
            return new SendResult(httpResponse, responseJson);
        }
    }
}
