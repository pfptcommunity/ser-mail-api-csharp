using System.Text;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public sealed class Client
    {
        private readonly OAuthHttpClient httpClient;

        public Client(string clientId, string clientSecret)
            : this(clientId, clientSecret, new HttpClientHandler()) { }

        public Client(string clientId, string clientSecret, HttpClientHandler httpClientHandler)
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
                    new HttpClient(httpClientHandler)
                );

                httpClient.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharp-SER-API/1.0");
                httpClient.HttpClient.BaseAddress = new Uri("https://mail.ser.proofpoint.com/v1/");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize the Proofpoint SER API client.", ex);
            }
        }

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
