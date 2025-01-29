using System.Net.Http;
using System.Text.Json;
using System.Text;
using IdentityModel.Client;
using System.Net.Http.Headers;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public class Client
    {
        private readonly OAuthHttpClient httpClient;

        // Default constructor (uses a standard HttpClientHandler)
        public Client(string clientId, string clientSecret)
            : this(clientId, clientSecret, new HttpClientHandler()) { }

        // Constructor that accepts a custom HttpClientHandler
        public Client(string clientId, string clientSecret, HttpClientHandler httpClientHandler)
        {
            var request = new ClientCredentialsTokenRequest
            {
                ClientCredentialStyle = ClientCredentialStyle.PostBody,
                Address = "https://mail.ser.proofpoint.com/v1/token",
                ClientId = clientId,
                ClientSecret = clientSecret,
            };

            httpClient = new OAuthHttpClient(new HttpClient(httpClientHandler), request);
            
            httpClient.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharp-SER-API/1.0");
            httpClient.HttpClient.BaseAddress = new Uri("https://mail.ser.proofpoint.com/v1/");
        }

        public async Task<SendResult> Send(Message message)
        {
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("send", content);

            string? responseJson = await response.Content.ReadAsStringAsync();

            return new SendResult(response, responseJson);
        }
    }
}