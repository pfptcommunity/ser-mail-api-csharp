using IdentityModel.Client;
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

            return await SendResult.CreateAsync(response);
        }
    }
}