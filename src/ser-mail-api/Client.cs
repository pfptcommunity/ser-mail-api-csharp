using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Provides a client for interacting with the Proofpoint Secure Email Relay API to send messages.
    /// </summary>
    public sealed class Client
    {
        /// <summary>
        /// The HTTP client used to communicate with the Proofpoint SER API, configured with OAuth authentication.
        /// </summary>
        private readonly OAuthHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of <see cref="Client"/> with the specified client ID and secret, using a default HTTP client handler.
        /// </summary>
        /// <param name="clientId">The client ID for OAuth authentication.</param>
        /// <param name="clientSecret">The client secret for OAuth authentication.</param>
        /// <param name="region">API service region selector.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="clientId"/> or <paramref name="clientSecret"/> is null, empty, or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the client fails to initialize due to configuration errors.</exception>
        public Client(string clientId, string clientSecret, Region region = Region.US)
            : this(clientId, clientSecret, new HttpClientHandler(), region) { }

        /// <summary>
        /// Initializes a new instance of <see cref="Client"/> with the specified client ID, secret, and custom HTTP client handler.
        /// </summary>
        /// <param name="clientId">The client ID for OAuth authentication.</param>
        /// <param name="clientSecret">The client secret for OAuth authentication.</param>
        /// <param name="httpClientHandler">The HTTP client handler for customizing HTTP requests.</param>
        /// <param name="region">API service region selector.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="clientId"/> or <paramref name="clientSecret"/> is null, empty, or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClientHandler"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the client fails to initialize due to configuration errors.</exception>
        public Client(string clientId, string clientSecret, HttpClientHandler httpClientHandler, Region region = Region.US)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID must not be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret must not be null or empty.", nameof(clientSecret));
            if (httpClientHandler == null)
                throw new ArgumentNullException(nameof(httpClientHandler), "HttpClientHandler must not be null.");

            try
            {
                httpClient = new OAuthHttpClient(
                    "https://mail.ser.proofpoint.com/v1/token",
                    clientId,
                    clientSecret,
                    "",
                    300, // Refresh 5 minutes early
                    new HttpClient(httpClientHandler)
                );

                httpClient.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharp-SER-API/1.0");
                httpClient.HttpClient.BaseAddress = new Uri($"https://{region.GetStringValue()}/v1/");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize the Proofpoint SER API client.", ex);
            }
        }

        /// <summary>
        /// Sends a message asynchronously to the Proofpoint Secure Email Relay API.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task that resolves to a <see cref="SendResult"/> containing the result of the send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when serialization, HTTP request, or an unexpected error occurs during the send operation.</exception>
        public async Task<SendResult> Send(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message must not be null.");

            string json;
            try
            {
                json = JsonSerializer.Serialize(message);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to serialize the message to JSON.", ex);
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync("send", content);
                return await SendResult.CreateAsync(response);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to send the email request due to an HTTP error.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while sending the email request.", ex);
            }
        }
    }
}