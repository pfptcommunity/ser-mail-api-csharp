using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Proofpoint.SecureEmailRelay.Mail
{
    internal class OAuthHttpClient : IHttpClient
    {
        public HttpClient HttpClient { get; }
        protected readonly ClientCredentialsTokenRequest _clientCredentials;
        private string? _accessToken;
        private DateTime? _tokenExpiration;
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        public OAuthHttpClient(ClientCredentialsTokenRequest clientCredentials)
        {
            _clientCredentials = clientCredentials ?? throw new ArgumentNullException(nameof(clientCredentials));
            HttpClient = new HttpClient();
        }

        public OAuthHttpClient(HttpClient httpClient, ClientCredentialsTokenRequest clientCredentials)
        {
            _clientCredentials = clientCredentials ?? throw new ArgumentNullException(nameof(clientCredentials));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration.HasValue && DateTime.UtcNow < _tokenExpiration.Value)
            {
                return; // Token is still valid
            }

            await _tokenSemaphore.WaitAsync();
            try
            {
                // Double-check within the lock
                if (string.IsNullOrEmpty(_accessToken) || !_tokenExpiration.HasValue || DateTime.UtcNow >= _tokenExpiration.Value)
                {
                    await RefreshTokenAsync();
                }
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private async Task RefreshTokenAsync()
        {
            var tokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(_clientCredentials);

            if (tokenResponse.IsError)
            {
                Console.WriteLine($"Error: {tokenResponse.Error}\n");
                Console.WriteLine($"HTTP Status Code: {tokenResponse.HttpStatusCode}\n");
                Console.WriteLine($"Raw Response: {tokenResponse.Raw}\n");
                throw new HttpRequestException($"Failed to retrieve access token: {tokenResponse.Error}");
            }

            _accessToken = tokenResponse.AccessToken;

            if (tokenResponse.HttpResponse is not null)
            {
                var responseContent = await tokenResponse.HttpResponse.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("token_expires_date_time", out var expiresDateTime))
                    {
                        _tokenExpiration = DateTime.Parse(expiresDateTime.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    else if (root.TryGetProperty("expires_in", out var expiresIn))
                    {
                        _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn.GetInt32() - 60);
                    }
                    else
                    {
                        throw new InvalidOperationException("The token response does not contain expiration details.");
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException("Failed to parse token expiration details.", ex);
                }
            }
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            if (content is not null)
            {
                request.Content = content;
            }
            return request;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync();
            var request = CreateRequest(HttpMethod.Get, requestUri);
            return await HttpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync();
            var request = CreateRequest(HttpMethod.Post, requestUri, content);
            return await HttpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync();
            var request = CreateRequest(HttpMethod.Put, requestUri, content);
            return await HttpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync();
            var request = CreateRequest(HttpMethod.Delete, requestUri);
            return await HttpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            await EnsureTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return await HttpClient.SendAsync(request, cancellationToken);
        }
    }
}
