using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Represents the result of sending a message via the mail relay, encapsulating the HTTP response and parsed data.
    /// </summary>
    public sealed class SendResult
    {
        /// <summary>
        /// Gets the unique identifier of the sent message, if available in the response.
        /// </summary>
        public string MessageId { get; } = string.Empty;

        /// <summary>
        /// Gets the reason or status description from the response, if provided.
        /// </summary>
        public string Reason { get; } = string.Empty;

        /// <summary>
        /// Gets the request identifier associated with the send operation, if present.
        /// </summary>
        public string RequestId { get; } = string.Empty;

        /// <summary>
        /// Gets the original HTTP response message received from the mail relay.
        /// </summary>
        public HttpResponseMessage HttpResponse { get; }

        /// <summary>
        /// Gets the raw JSON string returned in the HTTP response body.
        /// </summary>
        public string RawJson { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SendResult"/> with the specified HTTP response and raw JSON content.
        /// </summary>
        /// <param name="httpResponse">The HTTP response message from the mail relay. Must not be null.</param>
        /// <param name="rawJson">The raw JSON string from the response, or null if unavailable.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpResponse"/> is null.</exception>
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

        /// <summary>
        /// Creates a <see cref="SendResult"/> instance asynchronously from an HTTP response message.
        /// </summary>
        /// <param name="httpResponse">The HTTP response message to process.</param>
        /// <returns>A task that resolves to a new <see cref="SendResult"/> instance.</returns>
        /// <remarks>This method is intended for internal use within the assembly to process HTTP responses.</remarks>
        internal static async Task<SendResult> CreateAsync(HttpResponseMessage httpResponse)
        {
            string responseJson = await httpResponse.Content.ReadAsStringAsync();
            return new SendResult(httpResponse, responseJson);
        }
    }
}